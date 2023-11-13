using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services
{
    /// <summary>
    /// Implementação da interface IUserService
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public UserService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<User>> GetUsersAsync()
        {

            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Obtem um utilizador com um Id especifico.
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Utilizador</returns>
        public async Task<User> GetUser(Guid id)
        {
            return await _context.Users
                .Include(u => u.BlockedUsers)
                    .ThenInclude(x => x.BlockedUser)
                .Include(u => u.TeamsRequests)
                    .ThenInclude(t => t.Team)
                .Include(u => u.Teams)
                .Include(u => u.Notifications)
                .Include(u => u.Language)
                .Include(u => u.ChatRooms)
                .FirstAsync(c => c.Id == id.ToString());
        }

        /// <summary>
        /// Obtem os dados necessários relacionados com o perfil de um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Utilizador</returns>
        public async Task<User> GetUserProfile(Guid id)
        {
            return await _context.Users
                .Include(u => u.Skills)
                    .ThenInclude(u => u.Endorsers)
                .Include(u => u.Experiences)
                .Include(u => u.Language)
                .Include(u => u.Preferences)
                .FirstAsync(c => c.Id == id.ToString());
        }

        /// <summary>
        /// Obtem as habilidades e as recomendações dadas para cada habilidade para um utilizador com um determinado Id
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Habilidades e recomendações associadas</returns>
        public async Task<List<SkillModel>> GetUserSkills(Guid id)
        {
            var skills = _context.SkillModel.Where(c => c.UserId == id).Include(c => c.Endorsers);
            return await skills.ToListAsync();
        }

        public async Task<UserPreferences> GetUserPreferences(string userId)
        {
            if(userId == Guid.Empty.ToString()) return new UserPreferences();

            return await _context.UserPreferences.Where(p => p.UserId == userId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Apaga toda a informação do utilizador do website
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>True se apagou com sucesso</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> DeleteUserData(string userId)
        {
            if(userId == Guid.Empty.ToString()) return false;

            var user = await GetUser(new Guid(userId));
            
            if(user == null) return false;

            var TeamOwner = await _context.Team.Include(m => m.Members).Where(t => t.OwnerId.ToString() == userId).ToListAsync();

            if(TeamOwner.Any(t => t.Members.Any()))
            {
                throw new InvalidOperationException("Give ownership of your teams first!");
            }

            try
            {
                int counter = 0;
                if (TeamOwner.Any())
                {
                    counter++;
                    _context.RemoveRange(TeamOwner);
                }

                var connections = await _context.Connections.Where(c => c.RequestedUserId == userId || c.UserId == userId).ToListAsync();

                if(connections.Any())
                {
                    counter++;
                    _context.RemoveRange(connections);
                }

                var reportsUser = await _context.ReportUser.Where(c => c.ReportedId == userId || c.ReporterId == userId).ToListAsync();

                if (reportsUser.Any())
                {
                    counter++;
                    _context.RemoveRange(reportsUser);
                }

                var reportsTeams = await _context.ReportTeams.Where(c => c.ReporterId == userId).ToListAsync();

                if (reportsTeams.Any())
                {
                    counter++;
                    _context.RemoveRange(reportsTeams);
                }

                var reportsEvents = await _context.ReportTeams.Where(c => c.ReporterId == userId).ToListAsync();

                if (reportsEvents.Any())
                {
                    counter++;
                    _context.RemoveRange(reportsEvents);
                }

                var logs = await _context.UserActionLog.Where(l => l.AuthorId == userId || l.ChangedUserId == userId).ToListAsync();

                if (logs.Any())
                {
                    counter++;
                    _context.RemoveRange(logs);
                }

                var blocks = await _context.BlockedUsersModel.Where(l => l.SourceUserId == userId || l.BlockedUserId == userId).ToListAsync();

                if (blocks.Any())
                {
                    counter++;
                    _context.RemoveRange(blocks);
                }

                var messages = await _context.ChatMessages.Include(m => m.Readers).Where(m => m.SenderId == userId).ToListAsync();

                if(messages.Any())
                {
                    counter++;
                    messages.ForEach(x => x.Readers.Clear());
                    _context.UpdateRange(messages);
                }

                var success = await _context.SaveChangesAsync();

                if (counter == 0) return true;

                return success > 0;
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;            
        }

        public async Task<bool> SetUserPreferences(UserPreferences preferences)
        {
            var success = 0;
            try
            {
                var userHasPreferences = await _context.UserPreferences.AnyAsync(p => p.UserId == preferences.UserId);

                if (!userHasPreferences)
                    _context.Add(preferences);
                else
                    _context.Update(preferences);

                success = await _context.SaveChangesAsync();
            }
            catch { 
                return false;
            }

            return success == 1;
        }

        /// <summary>
        /// Recebe uma habilidade e adiciona-a à base de dados
        /// </summary>
        /// <param name="id">Id do utilizador dono da habilidade</param>
        /// <param name="skill">Habilidade</param>
        /// <returns>Habilidade adicionada na base de dados</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado ocorre um exceção</exception>
        public async Task<SkillModel> AddSkill(Guid id, SkillModel skill)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            skill.SkillId = new Guid();
            User user = await GetCurrentUser();
            skill.UserId = Guid.Parse((ReadOnlySpan<char>)user.Id);
            var success = 0;
            try
            {
                _context.Add(skill);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return null;
            }

            if (success != 1) return null; 

            return skill;
        }

        /// <summary>
        /// Recebe uma skill e atualiza-se na base de dados
        /// </summary>
        /// <param name="skill">Skill do utilizador</param>
        /// <returns>true se foi atualizada com sucesso, falso caso contrário</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado é retornada uma exceção</exception>
        public async Task<bool> EditSkill(SkillModel skill)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await SkillIsFromUser(skill.SkillId)))
                return false;

            skill.UserId = Guid.Parse((ReadOnlySpan<char>)user.Id);

            var success = 0;
            try
            {
                _context.Update(skill);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error EditSkill");
                throw;
            }

            return success == 1;
        }
        /// <summary>
        /// Recebe um id de uma skill e vai remove-la
        /// </summary>
        /// <param name="id">Id da skill do utilizador logado</param>
        /// <returns>true se removeu com sucesso, false caso contrário</returns>
        /// <exception cref="Exception">Lança um exceção se o utilizador não estiver logado</exception>
        public async Task<bool> DeleteSkill(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await SkillIsFromUser(id)))
            {
                return false;
            }

            SkillModel skill = new SkillModel() { SkillId = id };
            var result = 0;
            try
            {
                _context.SkillModel.Remove(skill);
                result = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error DeleteSkill");
                throw;
            }

            return result == 1;
        }

        /// <summary>
        /// Obtem as experiência de um utilizador com um determinado Id
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Experiências</returns>
        public async Task<List<ExperienceModel>> GetUserExperience(Guid id)
        {
            var experiences =  _context.ExperienceModel.Where(e => e.UserId == id.ToString());
            return await experiences.ToListAsync();
        }

        /// <summary>
        /// Recebe uma experiência e adiciona-a à base de dados
        /// </summary>
        /// <param name="experience">Experiência</param>
        /// <returns>Experiência adicionada na base de dados</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado ocorre um exceção</exception>
        public async Task<ExperienceModel> AddExperience(ExperienceModel experience)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            experience.ExperienceId = new Guid();
            experience.UserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(experience);
            await _context.SaveChangesAsync();
            return experience;
        }
        /// <summary>
        /// Recebe uma experiência atualizada e atualiza-a na base de dados
        /// </summary>
        /// <param name="experience">Experiência</param>
        /// <returns>Retorna true se foi atualizada com sucesso, 0 se não foi atualizada</returns>
        /// <exception cref="Exception">Ocorre numa exceção caso o user não esteja logado</exception>
        public async Task<bool> EditExperience(ExperienceModel experience)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();
            if (experience.UserId != user.Id)
                return false;
            /*var experiences = await _context.ExperienceModel.Where(e => e.ExperienceId == experience.ExperienceId && e.ExperienceUserId == Guid.Parse(user.Id)).ToListAsync();
            if (!experiences.Any())
            {
                return false;
            }*/
            var success = 0;
            try
            {
                _context.Update(experience);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExperienceExists(experience.ExperienceId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            
            return success == 1;
        }

        public async Task<bool> DeleteExperience(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await ExperienceIsFromUser(id)))
            {
                return false;
            }

            ExperienceModel experience = new ExperienceModel() { ExperienceId = id };
            var result = 0;
            try
            {
                _context.ExperienceModel.Remove(experience);
                result = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error DeleteExperience");
                throw;
            }

            return result == 1;
        }


        /// <summary>
        /// Recebe uma habilidade e atualiza o numero de recomendações da mesma após outro utilizador recomendar a habilidade
        /// </summary>
        /// <param name="id">Id do utilizador dono da habilidade</param>
        /// <param name="skill">Habilidade</param>
        /// <returns>Habilidade atualizada na base dados</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado ocorre um exceção</exception>
        public async Task<SkillModel> AddEndorsement(Guid sk)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User endorser = await GetCurrentUser();
            SkillModel skill = await GetSkillById(sk);

            if(skill == null)
            {
                Console.WriteLine("no object");
                return null;
            }

            var result = 0;
            if(skill.Endorsers == null)
            {
                skill.Endorsers = new List<User>();
            }

            if (!skill.Endorsers.Where(c => c.Id == endorser.Id).Any())
            {
                try
                {
                    Console.WriteLine("ookkk--------------------");
                    skill.Endorsers.Add(endorser);
                    _context.Update(skill);
                    result = await _context.SaveChangesAsync();
                }
                catch
                {
                    Console.WriteLine("---------------Error-------");
                }

            }

            if (result != 1)
                return null;
            return skill;

        }
        /// <summary>
        /// Recebe uma habilidade e atualiza os endorsements da mesma, após outro utilizador retirar a recomendação dada anteriormente
        /// </summary>
        /// <param name="id">Id do utilizador dono da habilidade</param>
        /// <returns>Habilidade atualizada na base dados</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado ocorre um exceção</exception>
        public async Task<SkillModel> RemoveEndorsement(Guid id)
        {
            Console.WriteLine("entrou");
            if (_httpContextAccessor.HttpContext == null) throw new Exception();

            User endorser = await GetCurrentUser();
            SkillModel skill = await GetSkillById(id);

            if (skill == null)
            {
                Console.WriteLine("--------------Obj null");
                return null;
            }
                

            var result = 0;
            if (skill.Endorsers == null)
            {
                Console.WriteLine("Endorsers Null-------------");
                return null;
            }

            if (skill.Endorsers.Where(c => c.Id == endorser.Id).Any())
            {
                try
                {
                    Console.WriteLine("ookkk--------------------");
                    skill.Endorsers.Remove(endorser);
                    _context.Update(skill);
                    result = await _context.SaveChangesAsync();
                }
                catch
                {
                    Console.WriteLine("---------------Error-------");
                }

            }

            Console.WriteLine(endorser.Id + "ID USER ------");

            if (result != 1)
                return null;
            return skill;
        }
        /// <summary>
        /// Verifica se o utilizador com sessão iniciada está bloqueado por outro
        /// </summary>
        /// <param name="id">Id do outro utilizador</param>
        /// <returns>true se está bloqueado, false caso contrário</returns>
        public async Task<bool> IsBlockedByUser(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();
            
            var isBlocked = await IsUserBlocked(id.ToString(), user.Id);
            return isBlocked;
        }

        /// <summary>
        /// Verifica se o utilizador´com sessão iniciada já bloqueou o outro utilizador
        /// </summary>
        /// <param name="id">Id do outro utilizador</param>
        /// <returns>true se já bloqueou esses utilizador, false caso contrário</returns>
        public async Task<bool> IsAlreadyBlocked(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var alreadyBlocked = await IsUserBlocked(user.Id, id.ToString());
            return alreadyBlocked;
        }
        /// <summary>
        /// Verifica se o utilizador´com sessão iniciada é Admin/Mod/Entidade
        /// </summary>
        /// <returns>true se for, false caso contrário</returns>

        public async Task<bool> IsCurrentUserModAdminEntity()
        {
            var currentUser = await GetCurrentUser();
            var IsCurrentUserModAdminEntity = (await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "Mod") || await _userManager.IsInRoleAsync(currentUser, "Entity"));
            if (IsCurrentUserModAdminEntity)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Altera o idioma de preferencia guardado pelo utilizador
        /// </summary>
        /// <param name="code">Código do idioma (i.e.: Portugal seria pt</param>
        /// <returns>true se alterou com sucesso, false caso contrário</returns>
        public async Task<bool> ChangeSavedLanguage(string code)
        {
            var user = await GetCurrentUser();
            if (user == null) return false;

            var localizationId = await GetLocalizationIdByCode(code);

            if(localizationId == Guid.Empty) return false;

            var success = 0;
            try
            {
                user.LanguageId = localizationId;

                _context.Users.Update(user);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return success == 1;
        } 

        /// <summary>
        /// Obtem o utilizador logged in.
        /// </summary>
        /// <returns>Utilizador Loggedin</returns>
        private Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private bool ExperienceExists(Guid id)
        {
            return _context.ExperienceModel.Any(e => e.ExperienceId == id);
        }

        private async Task<bool> ExperienceIsFromUser(Guid id)
        {
            User user = await GetCurrentUser();
            return _context.ExperienceModel.Any(e => e.ExperienceId == id && e.UserId == user.Id);
        }

        private async Task<bool> SkillIsFromUser(Guid id)
        {
            User user = await GetCurrentUser();
            return _context.SkillModel.Any(s => s.SkillId == id && s.UserId == Guid.Parse(user.Id));
        }

        private async Task<SkillModel> GetSkillById(Guid id)
        {
            return await _context.SkillModel.Include(c => c.Endorsers).Where(c => c.SkillId == id).FirstOrDefaultAsync();
        }
        /// <summary>
        /// Verifica se um utilizador já bloqueou outro
        /// </summary>
        /// <param name="sourceId">Id do utilizador que bloqueou</param>
        /// <param name="blockedId">Id do utilizador bloqueado</param>
        /// <returns>true se foi encontrado esse registo, false caso contrário</returns>
        private async Task<bool> IsUserBlocked(string sourceId, string blockedId)
        {
           return await _context.BlockedUsersModel.Where(b => b.SourceUserId == sourceId && b.BlockedUserId == blockedId).AnyAsync();
        }

        /// <summary>
        /// Obtem um idioma da base de dados pelo código
        /// </summary>
        /// <param name="code">Código do idioma</param>
        /// <returns>Idioma, null caso não tenha sido encontrado</returns>
        private async Task<Guid> GetLocalizationIdByCode(string code)
        {
            var existingLocalization = await _context.Localizations.Where(l => l.Code.Equals(code)).FirstOrDefaultAsync();
            return existingLocalization != null ? existingLocalization.LocalizationId : Guid.Empty;
        }
    }
}

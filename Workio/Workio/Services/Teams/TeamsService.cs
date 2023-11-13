using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services.Teams
{
    public class TeamsService : ITeamsService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;
        private IRatingService _ratingService;

        public TeamsService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService, IRatingService ratingService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _ratingService = ratingService;
        }

        /// <summary>
        /// Obtem todas as equipas que um utilizador faz parte e é o autor para determinado utilizador.
        /// </summary>
        /// <param name="userId">Id do utilizador do qual vai se obter as equipas.</param>
        /// <returns>Coleção de todos as equipas que o utilizador faz parte incluindo as próprias.</returns>
        public async Task<ICollection<Team>> GetAllUserTeamsByUserId(Guid userId)
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Where(t => (t.Members.Any(m => m.Id == userId.ToString())) || (t.OwnerId == userId))
                .ToListAsync();
        }

        /// <summary>
        /// Cria uma equipa e guarda na base de dados
        /// </summary>
        /// <param name="team">Objeto com os dados da equipa</param>
        /// <returns>True se foi guardada com sucesso, false caso contrário</returns>
        public async Task<ICollection<Team>> GetTeams()
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.PendingList)
                .ToListAsync();
        }

        /// <summary>
        /// Obtem todas as equipas que o utilizador faz parte na base de dados owner ou membro
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public async Task<ICollection<Team>> GetMyTeams()
        {
            var user = await GetCurrentUser();
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Where(t => t.OwnerId.ToString() == user.Id || t.Members.Any(m => m.Id == user.Id))
                .ToListAsync();
        }
        /// <summary>
        /// Obtem todas as equipas que o utilizador faz parte na base de dados apenas como membro
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Coleção de equipas</returns>
        public async Task<ICollection<Team>> GetUserTeams(Guid id)
        {
            var user = await _userService.GetUser(id);
            return await _context.Team.Include(t => t.Members)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Where(t => t.Members.Any(m => m.Id == user.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Obtem todas as equipas onde o utilizador é o dono
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public async Task<ICollection<Team>> GetOwnTeams()
        {
            var user = await GetCurrentUser();
            if (user == null) return new List<Team>();
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Where(t => t.OwnerId.ToString() == user.Id && t.Status == TeamStatus.Open)
                .ToListAsync();
        }

        /// <summary>
        /// Obtem as equipas de que um utilizador é o dono.
        /// </summary>
        /// <param name="userId">Id do utilizador do qual a obter as equipas.</param>
        /// <returns>Coleção das equipas de que o utilizado referido é dono.</returns>
        public async Task<ICollection<Team>> GetOwnAllTeamsByUserId(Guid userId)
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Where(t => t.OwnerId.ToString() == userId.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Obtem uma equipa com um id especifico
        /// </summary>
        /// <param name="id">Id da equipa</param>
        /// <returns>Equipa, caso não seja possivel encontrar é retornado null</returns>
        public async Task<Team> GetTeamById(Guid id)
        {
            if (_context.Team == null)
            {
                return null;
            }

            var team = await _context.Team
                .Include(t => t.Members)
                    .ThenInclude(l => l.Language)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.Milestones)
                .Include(t => t.PendingList)
                    .ThenInclude(r => r.User)
                .Include(t => t.InvitedUsers)
                    .ThenInclude(r => r.User)
                .Include(t => t.Language)
                .FirstOrDefaultAsync(m => m.TeamId == id);

            return team;
        }

        /// <summary>
        /// Obtem a lista de utilizadores que pediram para participar na equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa da qual obtem a lista de pedidos de participação.</param>
        /// <returns>Lista com os pedidos de participação na equipa.</returns>
        public async Task<List<PendingUserTeam>> GetTeamPendingRequestsByTeamId(Guid teamId)
        {
            return await _context.PendingUsers.Include(t => t.User).Include(t => t.Team).Where(t => (t.TeamId == teamId) && (t.Status == PendingUserTeamStatus.Pending)).OrderBy(r => r.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Obtem todas as equipas na base de dados
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public async Task<bool> CreateTeam(Team team)
        {
            User user = await GetCurrentUser();

            team.OwnerId = Guid.Parse(user.Id);
      
            var success = 0;

            try
            {
                _context.Add(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }    
            catch(Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Atualiza os dados de uma equipa
        /// </summary>
        /// <param name="team">Equipa atualizada</param>
        /// <returns>true caso tenha sido atualizada com sucesso, false caso contrário</returns>
        public async Task<bool> UpdateTeam(Team team)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await UserIsOwner(team.TeamId)))
                return false;

            team.OwnerId = Guid.Parse((ReadOnlySpan<char>)user.Id);
            
            var success = 0;
            try
            {
                // Retrieve the Team entity with its related Skills entities
                var existingTeam = await _context.Team
                    .Include(t => t.Skills)
                    .Include(t => t.Positions)
                    .FirstOrDefaultAsync(t => t.TeamId == team.TeamId);

                if (existingTeam == null)
                {
                    return false;
                }

                // Update the properties of the existing Team entity
                existingTeam.TeamName = team.TeamName;
                existingTeam.Description = team.Description;
                existingTeam.LanguageLocalizationId = team.LanguageLocalizationId;
                existingTeam.Tags = team.Tags;
                existingTeam.PositionsString = team.PositionsString;

                Console.WriteLine(team.Tags + " posicoes: " + existingTeam.PositionsString);

                // update the list property of the team entity
                existingTeam.Skills.Clear(); // remove all existing tags from the list
                if (!string.IsNullOrEmpty(team.Tags))
                {
                    var tagNames = team.Tags.Split(',').Select(t => t.Trim());
                    foreach (var tagName in tagNames)
                    {
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName && t.TeamId == team.TeamId);
                        if (tag == null)
                        {
                            tag = new Tag { TagId = Guid.NewGuid(), TagName = tagName, TeamId = existingTeam.TeamId };
                            _context.Tags.Add(tag);
                        }
                        existingTeam.Skills.Add(tag);
                    }
                }

                // update the list property of the team entity
                existingTeam.Positions.Clear(); // remove all existing tags from the list
                if (!string.IsNullOrEmpty(team.PositionsString))
                {
                    var positionNames = team.PositionsString.Split(',').Select(t => t.Trim());
                    foreach (var positionName in positionNames)
                    {
                        var position = await _context.Positions.FirstOrDefaultAsync(t => t.Name == positionName && t.TeamId == team.TeamId);
                        if (position == null)
                        {
                            position = new Position { PositionId = Guid.NewGuid(), Name = positionName, TeamId = existingTeam.TeamId };
                            _context.Positions.Add(position);
                        }
                        existingTeam.Positions.Add(position);
                    }
                }

                _context.Team.Update(existingTeam);

                //_context.Entry<Team>(team).State = EntityState.Detached;
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Altera o estado da equipa
        /// </summary>
        /// <param name="teamStatus">Novo estado</param>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true caso seja alterado com sucesso, false caso contrário</returns>
        public async Task<bool> ChangeTeamStatus(TeamStatus teamStatus, Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var success = 0;
            try
            {
                team.Status = teamStatus;

                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Adiciona um user a equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a adicionar</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public async Task<bool> AddUser(Guid teamId, Guid userId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if(user.Id == userId.ToString()) return false;

            if (!(await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var newMember = await _userService.GetUser(userId);

            if (newMember == null) return false;

            var userIsMember = UserIsMember(team, userId);

            if (userIsMember) return false;

            var success = 0;
            try
            {
                team.Members.Add(newMember);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Remove um user da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a remover</param>
        /// <returns>true se conseguiu remover o utilizador, false caso contrário</returns>
        public async Task<bool> RemoveUser(Guid teamId, Guid userId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var member = await _userService.GetUser(userId);

            if(member == null) return false;

            var userIsMember = UserIsMember(team, userId);

            if (!userIsMember) return false;

            var success = 0;
            try
            {
                team.Members.Remove(member);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }


        /// <summary>
        /// Pedir para fazer parte de uma equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true se o pedido foi feito com sucesso, false caso contrário</returns>
        public async Task<bool> AskAccess(Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if ((await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            if(team.Status != TeamStatus.Open) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (userIsMember) return false;

            var alreadyInvited = await AlreadyInvited(team.TeamId, Guid.Parse(user.Id));

            if(alreadyInvited) return await AcceptInvite(teamId);

            var success = 0;
            try
            {
                PendingUserTeam pendingUser = new PendingUserTeam()
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    UserId = user.Id,
                };

                _context.PendingUsers.Add(pendingUser);

                team.PendingList.Add(pendingUser);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Aceita o pedido de um user e adiciona-o a equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public async Task<bool> AcceptAccess(Guid requestId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var request = await GetRequestById(requestId);

            if(request == null) return false;

            if (!(await UserIsOwner(request.TeamId)))
                return false;

            var team = await GetTeamById(request.TeamId);

            if (team == null) return false;

            var newMember = await _userService.GetUser(Guid.Parse(request.UserId));

            if (newMember == null) return false;

            if(newMember.Id == user.Id) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(newMember.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Accepted;
                _context.PendingUsers.Update(request);
                //team.PendingList.Remove(request);
                team.Members.Add(newMember);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Rejeitar um pedido para fazer parte de uma equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se o pedido foi feito com sucesso, false caso contrário</returns>
        public async Task<bool> RejectAccess(Guid requestId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var request = await GetRequestById(requestId);

            if (request == null) return false;

            if (!(await UserIsOwner(request.TeamId)))
                return false;

            var team = await GetTeamById(request.TeamId);

            if (team == null) return false;

            var newMember = await _userService.GetUser(Guid.Parse(request.UserId));

            if (newMember == null) return false;

            if (newMember.Id == user.Id) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(newMember.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Rejected;
                _context.PendingUsers.Update(request);
                //team.PendingList.Remove(request);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Abandonar equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true caso tenha abandonado a equipa com sucesso, false caso contrário</returns>
        public async Task<bool> LeaveTeam(Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if ((await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (!userIsMember) return false;

            var success = 0;
            try
            {
                team.Members.Remove(user);
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Convida um utilizador a participar na equipa
        /// </summary>
        /// <param name="teamId">Id da equipa para a qual está a ser convidado</param>
        /// <param name="userId">Id do utilizador convidado</param>
        /// <returns>true caso convide com sucesso, false caso contrário</returns>
        public async Task<bool> InviteUserToTeam(Guid teamId, Guid userId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var userIsMember = UserIsMember(team, userId);

            if (userIsMember) return false;
            
            var alreadyInvited = await AlreadyInvited(teamId, userId);

            if(alreadyInvited) return false;

            var isPending = await GetPendingRequest(teamId, userId);

            if (isPending != null) return await AcceptAccess(isPending.Id);

            var newMember = await _userService.GetUser(userId);

            var success = 0;
            try
            {
                TeamInviteUser invite = new TeamInviteUser()
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    UserId = userId.ToString(),
                };

                _context.TeamsRequests.Add(invite);

                team.InvitedUsers.Add(invite);
                _context.Team.Update(team);
                
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Aceita o pedido de uma equipa e adiciona-o a equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public async Task<bool> AcceptInvite(Guid requestId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var request = await GetInviteById(requestId);

            if (request == null) return false;

            if(request.UserId != user.Id) return false;

            var team = await GetTeamById(request.TeamId);

            if (team == null) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Accepted;
                _context.TeamsRequests.Update(request);
                //user.TeamsRequests.Remove(request);
                team.Members.Add(user);
                _context.Team.Update(team);
                _context.Users.Update(user);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Aceita o pedido de uma equipa e adiciona-o a equipa
        /// </summary>
        /// <param name="teamId">Id da equipa que fez o pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public async Task<bool> AcceptInviteByTeam(Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            User user = await GetCurrentUser();

            var request = await GetInviteByTeam(team.TeamId, user.Id);

            if (request == null) return false;

            if (request.UserId != user.Id) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Accepted;
                _context.TeamsRequests.Update(request);
                //user.TeamsRequests.Remove(request);
                team.Members.Add(user);
                _context.Team.Update(team);
                _context.Users.Update(user);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Rejeita o pedido de uma equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public async Task<bool> RejectInvite(Guid requestId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var request = await GetInviteById(requestId);

            if (request == null) return false;

            if (request.UserId != user.Id) return false;

            var team = await GetTeamById(request.TeamId);

            if (team == null) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Rejected;
                _context.TeamsRequests.Update(request);
                //request.Status = PendingUserTeamStatus.Rejected;
                //_context.TeamsRequests.Remove(request);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Rejeita o pedido de uma equipa
        /// </summary>
        /// <param name="teamId">Id da equipa que fez o pedido</param>
        /// <returns>true se conseguiu rejeitar o utilizador, false caso contrário</returns>
        public async Task<bool> RejectInviteByTeam(Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();

            var team = await GetTeamById(teamId);
            
            if (team == null) return false;

            User user = await GetCurrentUser();

            var request = await GetInviteByTeam(team.TeamId, user.Id);

            if (request == null) return false;

            if (request.UserId != user.Id) return false;

            var userIsMember = UserIsMember(team, Guid.Parse(user.Id));

            if (userIsMember) return false;

            var success = 0;
            try
            {
                request.Status = PendingUserTeamStatus.Rejected;
                _context.TeamsRequests.Update(request);
                //request.Status = PendingUserTeamStatus.Rejected;
                //_context.TeamsRequests.Remove(request);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Cancela o convite para participar numa equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu cancelar o pedido, false caso contrário</returns>
        public async Task<bool> CancelInvite(Guid requestId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var request = await GetInviteById(requestId);

            if (request == null) return false;

            var team = await GetTeamById(request.TeamId);

            if (team == null) return false;

            if(team.OwnerId.ToString() != user.Id) return false;

            var userInvited = await _userService.GetUser(Guid.Parse(request.UserId));

            if(userInvited == null) return false;

            var success = 0;
            try
            {
                userInvited.TeamsRequests.Remove(request);
                _context.Users.Update(userInvited);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<ICollection<Tag>> GetTags()
        {
            return await _context.Tags.ToListAsync();
        }

        public async Task<bool> CreateTag(Tag tag)
        {
            tag.TagId = Guid.NewGuid();

            _context.Add(tag);
            var success = await _context.SaveChangesAsync();

            return success == 1;
        }

        /// <summary>
        /// Adiciona uma milestone a equipa
        /// </summary>
        /// <param name="milestone">Milestone a adicionar</param>
        /// <param name="teamId">Id da equipa que vai receber a milestone</param>
        /// <returns>true caso tenha sido inserida com sucesso, false caso contrário</returns>
        public async Task<bool> AddMilestone(Milestone milestone, Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if(user == null) return false;

            var team = await GetTeamById(teamId);

            if(team == null) return false;

            if(!(await UserIsOwner(teamId))) return false;   

            var success = 0;
            try
            {
                _context.Add(milestone);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;

        }

        public async Task<bool> UpdateMilestone(Milestone milestone, Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (user == null) return false;

            if (!(await UserIsOwner(teamId))) return false;

            var exists = await MilestoneExists(milestone.MilestoneId, teamId);

            if(exists == false) return false;

            var newMilestone = await GetMilestoneById(milestone.MilestoneId, teamId);

            if(newMilestone == null) return false;

            newMilestone.Name = milestone.Name;
            newMilestone.Description = milestone.Description;
            newMilestone.LastUpdatedAt = DateTime.UtcNow;
            newMilestone.StartDate = milestone.StartDate;
            newMilestone.EndDate = milestone.EndDate;

            var success = 0;
            try
            {
                _context.Milestones.Update(newMilestone);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<bool> DeleteMilestone(Guid milestoneId, Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (user == null) return false;

            if (!(await UserIsOwner(teamId))) return false;

            var exists = await MilestoneExists(milestoneId, teamId);

            if (exists == false) return false;

            var milestone = await GetMilestoneById(milestoneId, teamId);

            if(milestone == null) return false;
            
            var success = 0;
            try
            {
                _context.Remove(milestone);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Altera o estado de uma milestone
        /// </summary>
        /// <param name="milestoneId">Id da milestone</param>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="state">Novo Estado</param>
        /// <returns>true se conseguiu alterar o estado, false caso contrário</returns>
        public async Task<bool> ChangeMilestoneStatus(Guid milestoneId, Guid teamId, MilestoneState state)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (user == null) return false;

            var milestone = await GetMilestoneById(milestoneId, teamId);

            if (milestone == null) return false;

            if (!(await UserIsOwner(teamId))) return false;

            var success = 0;
            try
            {
                milestone.State = state;
                _context.Update(milestone);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Obtem o pedido de adesão a uma equipa.
        /// </summary>
        /// <param name="id">Id do pedido.</param>
        /// <returns>Objeto do pedido de adesão.</returns>
        public async Task<PendingUserTeam> GetRequestById(Guid id)
        {
            return await _context.PendingUsers.Where(r => r.Id == id && r.Status == PendingUserTeamStatus.Pending).FirstOrDefaultAsync();
        }

        public async Task<TeamInviteUser> GetInviteById(Guid id)
        {
            return await _context.TeamsRequests.Where(r => r.Id == id && r.Status == PendingUserTeamStatus.Pending).FirstOrDefaultAsync();
        }

        public async Task<TeamInviteUser> GetInviteByTeam(Guid teamId, string userId)
        {
            return await _context.TeamsRequests.Where(r => r.TeamId == teamId && r.UserId == userId && r.Status == PendingUserTeamStatus.Pending).FirstOrDefaultAsync();
        }

        private async Task<bool> MilestoneExists(Guid milestoneId, Guid teamId)
        {
            return await _context.Team.AnyAsync(r => r.TeamId == teamId && r.Milestones != null && r.Milestones.Any(m => m.MilestoneId == milestoneId && m.TeamId == teamId));
        }

        private async Task<Milestone> GetMilestoneById(Guid milestoneId, Guid teamId)
        {
            var team = await _context.Team.Include(t => t.Milestones).Where(r => r.TeamId == teamId && r.Milestones != null && r.Milestones.Any(m => m.MilestoneId == milestoneId && m.TeamId == teamId)).FirstOrDefaultAsync();
            if (team == null || team.Milestones == null) return null;

            return team.Milestones.Where(m => m.MilestoneId == milestoneId).FirstOrDefault();
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
        /// Verifica se o utilizador atual é o dono de uma equipa
        /// </summary>
        /// <param name="id">Id da equipa</param>
        /// <returns>true se for o dono, false caso contrário</returns>
        private async Task<bool> UserIsOwner(Guid id)
        {
            User user = await GetCurrentUser();
            return _context.Team.Any(t => t.TeamId == id && t.OwnerId == Guid.Parse(user.Id));
        }

        private bool UserIsMember(Team team, Guid userId)
        {
            return team.Members.Any(m => m.Id == userId.ToString());
        }

        private async Task<PendingUserTeam> GetPendingRequest(Guid teamId, Guid userId)
        {
            return await _context.PendingUsers.Where(r => r.UserId == userId.ToString() && r.TeamId == teamId && r.Status == PendingUserTeamStatus.Pending).FirstOrDefaultAsync();
        }

        private async Task<bool> AlreadyInvited(Guid teamId, Guid userId)
        {
            return await _context.TeamsRequests.Where(r => r.UserId == userId.ToString() && r.TeamId == teamId && r.Status == PendingUserTeamStatus.Pending).AnyAsync();
        }

        /// <summary>
        /// Verifica se dois utilizadores pertencem a mesma equipa
        /// </summary>
        /// <param name="tag">Utilizador a ser avaliado</param>
        /// <returns>true caso 2 utilizadores pertençam a mesma equipa false caso contrário</returns>
        public async Task<bool> AreTeammates(Guid otherUserId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            var member = await _userService.GetUser(otherUserId);

            if (member == null) return false;

            return await _context.Team.Include(t => t.Members).Where(t => (t.OwnerId.ToString() == user.Id || t.Members.Any(t => t.Id == user.Id)) && (t.OwnerId == otherUserId || t.Members.Any(t => t.Id == otherUserId.ToString()))).AnyAsync();
        }

        /// <summary>
        /// Dá ownership a um user da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a remover</param>
        /// <returns>true se conseguiu dar ownership</returns>
        public async Task<bool> GiveOwnership(Guid teamId, Guid userId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await UserIsOwner(teamId)))
                return false;

            var team = await GetTeamById(teamId);

            if (team == null) return false;

            var member = await _userService.GetUser(userId);

            if (member == null) return false;

            var userIsMember = UserIsMember(team, userId);

            if (!userIsMember) return false;

            var success = 0;
            try
            {
                team.Members.Add(user);
                team.Members.Remove(member);
                team.OwnerId = userId;
                _context.Team.Update(team);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Dá o valor médio das avaliações dos utilizadores da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a remover</param>
        /// <returns>valor médio das avaliações dos utilizadores da equipa</returns>
        public async Task<double> GetAverageRating(Guid teamId)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();

            var team = await GetTeamById(teamId);

            if (team == null) return 0;

            double totalRatingValue = 0;

            if (_ratingService.IsRated(team.OwnerId.Value))
            {
                totalRatingValue += await _ratingService.GetTrueAverageRating(team.OwnerId.Value);
            }

            foreach(User u in team.Members)
            {
                Guid userId = new Guid(u.Id);
                if (_ratingService.IsRated(userId))
                {
                    totalRatingValue += await _ratingService.GetTrueAverageRating(userId);
                }
            }

            double averageRating = totalRatingValue / (team.Members.Count + 1);

            return averageRating;
        }

        /// <summary>
        /// Retorna as equipas abertas das quais o utilizador nao faz parte
        /// </summary>
        /// <returns>Retorna as equipas abertas das quais o utilizador nao faz parte</returns>
        public async Task<ICollection<Team>> GetOpenNewTeams()
        {
            User user = await GetCurrentUser();
            if (user == null) return null;

            Guid userId = new Guid(user.Id);

            var teams = await GetTeams();
            if (teams == null) return null;

            var openTeams = teams.Where(t => t.Status == TeamStatus.Open && t.OwnerId != userId && !t.Members.Contains(user) && !t.IsBanned).ToList();

            return openTeams;
        }

        /// <summary>
        /// Obtem a equipa de um convite pelo id do convite.
        /// </summary>
        /// <param name="inviteId">Id do convite.</param>
        /// <returns>Objeto da equipa.</returns>
        public async Task<TeamInviteUser> GetTeamByInviteId(Guid inviteId)
        {
            return await _context.TeamsRequests.Where(r => r.Id == inviteId).FirstOrDefaultAsync();
        }
    }
}

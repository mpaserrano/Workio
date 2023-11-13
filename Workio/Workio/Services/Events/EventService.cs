using MessagePack.Formatters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Workio.Models;
using System.Security.Claims;
using Workio.Data;
using Workio.Models.Events;
using Workio.Services.Interfaces;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Workio.Services.Events
{
    public class EventService : IEventsService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;

        public EventService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public async Task<bool> CreateEvent(Event @event)
        {
            User user = await GetCurrentUser();
            @event.UserId = user.Id; //Guid user.Id;

            var success = 0;

            try
            {
                _context.Add(@event);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
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
        /// Obtem o utilizador logged in.
        /// </summary>
        /// <returns>Utilizador Loggedin</returns>
        private async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Obtem todos os eventos
        /// </summary>
        /// <returns>Lista de eventos</returns>
        public async Task<ICollection<Event>> GetEvents()
        {
            var list = await _context.Event
                .Include(e => e.EventTags)
                .Include(e => e.InterestedTeams)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(m => m.Members)
                .Include(e => e.InterestedUsers)
                    .ThenInclude(u => u.User)
                .Include(e => e.EventReactions)
                .Include(e => e.UserPublisher)
                    .ThenInclude(e => e.BlockedUsers)
                .ToListAsync<Event>();
            if (list == null)
                return new List<Event>();
            return list;
        }

        /// <summary>
        /// Obtem um evento com um Id especifico.
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Evento</returns>
        public async Task<Event> GetEvent(Guid id)
        {
            return await _context.Event
                .Include(e => e.InterestedUsers)
                    .ThenInclude(u => u.User)
                .Include(t => t.EventTags)
                .Include(e => e.InterestedTeams)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(m => m.Members)
                .Include(e => e.EventReactions)
                .FirstOrDefaultAsync(c => c.EventId == id);
        }

        /// <summary>
        /// Retorna uma lista de eventos com todos os eventos promovidos pela plataforma
        /// </summary>
        /// <returns>Coleção de eventos promovidos</returns>
        public async Task<ICollection<Event>> GetFeaturedEvents()
        {
            var list = await _context.Event
                .Include(e => e.InterestedTeams)
                .Include(e => e.InterestedUsers)
                .Include(e => e.EventReactions)
                .Include(e => e.UserPublisher)
                    .ThenInclude(u => u.BlockedUsers)
                .Where(e => e.IsFeatured && e.State != EventState.Finish && e.IsBanned != true)
                .ToListAsync<Event>();
            if (list == null)
                return new List<Event>();

            if (_httpContextAccessor.HttpContext != null)
            {
                var user = await GetCurrentUser();
                if (user != null)
                    list = list.Where(e => !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId || u.SourceUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == user.Id || u.SourceUserId == user.Id))).ToList();
            }

            list.ForEach(e => e.UserPublisher?.BlockedUsers?.Clear());

            return list;
        }
        /// <summary>
        /// Obtem uma lista de eventos ordenada pelo diferença entre upvotes e downvotes de um evento
        /// </summary>
        /// <param name="maxQuantity">Número máximo de eventos a retornar</param>
        /// <returns>Lista dos eventos mais populares</returns>
        public async Task<ICollection<Event>> GetTopEvents(int maxQuantity)
        {
            var list = await _context.Event
                .Include(e => e.InterestedTeams)
                .Include(e => e.InterestedUsers)
                .Include(e => e.EventReactions)
                .Include(e => e.UserPublisher)
                    .ThenInclude(m => m.BlockedUsers)
                .OrderByDescending(e => (e.EventReactions.Where(r => r.ReactionType == EventReactionType.UpVote).Count()) - (e.EventReactions.Where(r => r.ReactionType == EventReactionType.DownVote).Count()))
                .Where(e => e.State != EventState.Finish && e.IsBanned == false)
                .ToListAsync<Event>();
            if (list == null)
                return new List<Event>();

            if (_httpContextAccessor.HttpContext != null)
            {
                var user = await GetCurrentUser();
                if (user != null)
                    list = list.Where(e => !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId || u.SourceUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == user.Id || u.SourceUserId == user.Id))).ToList();
            }

            list.ForEach(e => e.UserPublisher?.BlockedUsers?.Clear());

            return list.Take(maxQuantity).ToList();
        }
        
            

        /// <summary>
        /// Obtem uma lista de eventos que vão ocorrer nos próximos dias
        /// </summary>
        /// <param name="upto">Máximo de dias a procurar (até que dia devem ser retornados)</param>
        /// <param name="maxQuantity">Número máximo de eventos a serem retornados</param>
        /// <returns>Lista de eventos a acontecer nos próximos dias</returns>
        public async Task<ICollection<Event>> GetSoonEvents(int upto, int maxQuantity)
        {
            // Get the current date and the specified date range
            var currentDate = DateTime.Now;
            var toDate = currentDate.AddDays(upto);

            var list = await _context.Event
                .Include(e => e.InterestedTeams)
                .Include(e => e.InterestedUsers)
                .Include(e => e.EventReactions)
                .Include(e => e.UserPublisher)
                    .ThenInclude(e => e.BlockedUsers)
                .Where(e => e.StartDate >= currentDate.AddDays(1) && e.StartDate <= toDate && e.State != EventState.Finish && e.IsBanned == false)
                .OrderBy(e => e.StartDate)
                .ToListAsync<Event>();
            if (list == null)
                return new List<Event>();

            if (_httpContextAccessor.HttpContext != null)
            {
                var user = await GetCurrentUser();
                if(user != null)
                    list = list.Where(e => !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId || u.SourceUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == user.Id || u.SourceUserId == user.Id))).ToList();
            }

            list.ForEach(e => e.UserPublisher?.BlockedUsers?.Clear());

            return list.Take(maxQuantity).ToList();
        }

        /// <summary>
        /// Remove um utilizador interessado 
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se a operação tece sucesso, false se não teve</returns>
        public async Task<bool> RemoveInterestedUser(Guid id)
        {
            var @event = await GetEvent(id);
            if (@event == null || @event.State == EventState.Finish || !(await IsUserInterested(id)))
            {
                return false;
            }
            var success = 0;
            var user = await GetCurrentUser();
            var interestedUser = _context.InterestedUsers.FirstOrDefault( c => c.User.Id == user.Id.ToString() && c.Event.EventId == id);
            _context.Remove(interestedUser);
            success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Verifica se o utilizador logado de momento ja mostrou interesse no evento que se encotra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador já está interessado, falso se não estiver</returns>
        public async Task<bool> IsUserInterested(Guid id)
        { //&& c.Event.EventId == id
            var user = await GetCurrentUser();
            var result = _context.InterestedUsers.FirstOrDefault(c => c.User.Id == user.Id.ToString() && c.Event.EventId == id);
            if (result == null) { return false; }
            return true;
        }
        /// <summary>
        /// Adiciona à base de dados que o utilizador logado está interessado no evento que se encontra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador foi adicionado com sucesso, falso se não foi</returns>
        public async Task<bool> AddInterestedUser(Guid id)
        {

            var @event = await GetEvent(id);
            if (@event == null || @event.State == EventState.Finish || await IsUserInterested(id))
            {
                return false;
            }
            var user = await GetCurrentUser();
            InterestedUser interestedUser = new InterestedUser() { InterestedId = Guid.NewGuid(), Event = @event, User = user };
            _context.Add(interestedUser);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        /// <summary>
        /// Remove uma equipa interessada
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se a operação tece sucesso, false se não teve</returns>
        public async Task<bool> RemoveInterestedTeam(Guid id)
        {
            var @event = await GetEvent(id);

            if (@event == null || @event.State == EventState.Finish || !(await IsTeamInterested(id)))
            {
                return false;
            }
            var success = 0;
            var user = await GetCurrentUser();
            var interestedTeam = _context.InterestedTeams.FirstOrDefault(c => c.Team.OwnerId.ToString() == user.Id && c.Event.EventId == id);
            _context.Remove(interestedTeam);
            success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Verifica se o utilizador logado de momento ja mostrou interesse numa equipa que lidere no evento que se encotra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador já está interessado, falso se não estiver</returns>
        public async Task<bool> IsTeamInterested(Guid id)
        { 
            var user = await GetCurrentUser();
            var result = _context.InterestedTeams.FirstOrDefault(c => c.Team.OwnerId.ToString() == user.Id && c.Event.EventId == id);
            if (result == null) { return false; }
            return true;
        }
        /// <summary>
        /// Adiciona à base de dados a equipa interessada no evento.
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador foi adicionado com sucesso, falso se não foi</returns>
        public async Task<bool> AddInterestedTeam(Team team, Guid id)
        {
            var @event = await GetEvent(id);

            if (@event == null || @event.State == EventState.Finish || await IsTeamInterested(id))
            {
                return false;
            }
            InterestedTeam interestedTeam = new InterestedTeam() { InterestedId = Guid.NewGuid(), Event = @event, Team = team };
            _context.Add(interestedTeam);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }



        /// <summary>
        /// Atualiza um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        public async Task<bool> UpdateEvent(Event @event)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            User user = await GetCurrentUser();

            if (!(await isCreator(@event.EventId)))
                return false;

            @event.UserId = Guid.Parse((ReadOnlySpan<char>)user.Id).ToString();

            var success = 0;
            try
            {
                var existingEvent = await _context.Event.Include(t => t.EventTags).FirstOrDefaultAsync(e => e.EventId == @event.EventId);
                if (existingEvent == null)
                {
                    return false;
                }

                if (@event.BannerPicturePath != null)
                {
                    existingEvent.BannerPicturePath = @event.BannerPicturePath;
                }
                existingEvent.Title = @event.Title;
                existingEvent.Description = @event.Description;
                existingEvent.Tags = @event.Tags;
                existingEvent.StartDate = @event.StartDate;
                existingEvent.EndDate = @event.EndDate;
                existingEvent.State = @event.State;
                existingEvent.Url = @event.Url;
                existingEvent.IsFeatured = @event.IsFeatured;
                existingEvent.IsInPerson = @event.IsInPerson;
                existingEvent.Address = @event.Address;
                existingEvent.Latitude = @event.Latitude;
                existingEvent.Longitude = @event.Longitude;
                existingEvent.EventTags.Clear();
                if (!string.IsNullOrEmpty(@event.Tags))
                {
                    var tagNames = @event.Tags.Split(',').Select(t => t.Trim());
                    foreach (var tagName in tagNames)
                    {
                        var tag = await _context.EventTag.FirstOrDefaultAsync(t => t.EventTagName == tagName && t.EventId == @event.EventId);
                        if (tag == null)
                        {
                            tag = new EventTag { EventTagId = Guid.NewGuid(), EventTagName = tagName, EventId = existingEvent.EventId };
                            _context.EventTag.Add(tag);
                        }
                        existingEvent.EventTags.Add(tag);
                    }
                }
                _context.Event.Update(existingEvent);
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
        /// Apaga um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        public async Task<Event> RemoveEvent(Event @event)
        {
            _context.Remove(@event);
            await _context.SaveChangesAsync();
            return @event;
        }
        /// <summary>
        /// Verifica se o utilizador pode dar downvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se der downvote, falso se não</returns>
        public async Task<bool> UpVote(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var @event = await GetEvent(id);
            if (@event == null)
            {
                return false;
            }
            var user = await GetCurrentUser();
            var alreadyUpvoted = await AlreadyUpvoted(id);
            var alreadyDownvoted = await AlreadyDownvoted(id);
            if (alreadyUpvoted == true)
            {
                return false;
            }
            if (alreadyDownvoted == true)
            {
                await RemoveDownvote(id);
            }
            EventReactions ev = new EventReactions()
            {
                EventId = @event.EventId,
                ReactionId = Guid.NewGuid(),
                UserId = Guid.Parse(user.Id),
                ReactionType = EventReactionType.UpVote
            };
            _context.Add(ev);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        /// <summary>
        /// Verifica se o utilizador pode dar downvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se der downvote, falso se não</returns>
        public async Task<bool> Downvote(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var @event = await GetEvent(id);
            if (@event == null)
            {
                return false;
            }
            var user = await GetCurrentUser();
            var alreadyDownvoted = await AlreadyDownvoted(id);
            var alreadyUpvoted = await AlreadyUpvoted(id);
            if (alreadyDownvoted == true)
            {
                return false;

            }
            if (alreadyUpvoted == true)
            {
                await RemoveUpvote(id);
            }
            EventReactions ev = new EventReactions()
            {
                EventId = @event.EventId,
                ReactionId = Guid.NewGuid(),
                UserId = Guid.Parse(user.Id),
                ReactionType = EventReactionType.DownVote
            };
            var eventReactions = _context.Event.Where(x => x.EventId == @event.EventId).Include(x => x.EventReactions);
            _context.Add(ev);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Verifica se o utilizador já deu upvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se já deu upvote, falso se não</returns>
        public async Task<bool> AlreadyUpvoted(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var user = await GetCurrentUser();
            var @event = await GetEvent(id);
            var upvoted = _context.EventReactions.Any(x => x.EventId == @event.EventId && x.UserId == Guid.Parse(user.Id) && x.ReactionType == EventReactionType.UpVote);
            if (!upvoted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Verifica se o utilizador já deu downvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se já deu downvote, falso se não</returns>
        public async Task<bool> AlreadyDownvoted(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var user = await GetCurrentUser();
            var @event = await GetEvent(id);
            var upvoted = _context.EventReactions.Any(x => x.EventId == @event.EventId && x.UserId == Guid.Parse(user.Id) && x.ReactionType == EventReactionType.DownVote);
            if (!upvoted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Obtem o numero de downvotes de um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>int, numero de upvotes</returns>

        public async Task<int> GetNumberOfUpvotes(Guid id)
        {
            var @event = await GetEvent(id);
            var reactions = _context.EventReactions.Where(x => x.EventId == @event.EventId && x.ReactionType == EventReactionType.UpVote).ToList();
            int number = reactions.Count();
            return number;
        }
        /// <summary>
        /// Obtem o numero de downvotes de um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>int, numero de upvotes</returns>
        public async Task<int> GetNumberOfDownvotes(Guid id)
        {
            var @event = await GetEvent(id);
            var reactions = _context.EventReactions.Where(x => x.EventId == @event.EventId && x.ReactionType == EventReactionType.DownVote).ToList();
            int number = reactions.Count();
            return number;
        }
        /// <summary>
        /// Verifica se o utilizador atual é o criador do evento
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>true se for, falso caso contrário</returns>
        public async Task<bool> isCreator(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var @event = await GetEvent(id);
            var user = await GetCurrentUser();
            return _context.Event.Any(x => x.EventId == @event.EventId && x.UserId == user.Id.ToString());
        }
        /// <summary>
        /// Verifica se o evento já terminou
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se já tiver terminado, falso caso contrário</returns>
        public async Task<bool> isFinished(Guid id)
        {
            var @event = await GetEvent(id);
            if (@event.State == EventState.Finish)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove upvote se já existir
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se o upvote for removido com sucesso, falso caso contrário</returns>
        public async Task<bool> RemoveUpvote(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var @event = await GetEvent(id);
            var user = await GetCurrentUser();
            var alreadyUpvoted = await AlreadyUpvoted(id);
            if (alreadyUpvoted == true)
            {
                var vote = _context.EventReactions.Where(x => x.EventId == @event.EventId && x.UserId == Guid.Parse(user.Id) && x.ReactionType == EventReactionType.UpVote).First();
                _context.EventReactions.Remove(vote);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove o downvote se já existir
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se o upvote for removido com sucesso, falso caso contrário</returns>
        public async Task<bool> RemoveDownvote(Guid id)
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            var @event = await GetEvent(id);
            var user = await GetCurrentUser();
            var alreadyDownvoted = await AlreadyDownvoted(id);
            if (alreadyDownvoted == true)
            {
                var vote = _context.EventReactions.Where(x => x.EventId == @event.EventId && x.UserId == Guid.Parse(user.Id) && x.ReactionType == EventReactionType.DownVote).First();
                _context.EventReactions.Remove(vote);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Muda o state
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <param name="state">state do evento</param>
        public async Task<bool> ChangeEventStatus(Guid id, EventState state)
        {
            var success = 0;
            try
            {
                Event e = await GetEvent(id);
                if(e != null)
                {
                    e.State = state;
                    _context.Update(e);
                    success = await _context.SaveChangesAsync();
                }

            }
            catch
            {

            }

            return success > 0;
        }

        /// <summary>
        /// Retorna uma coleção de eventos em que a data de inicío dos eventos está no intrevalo recebido.
        /// </summary>
        /// <param name="minDate">Minímo do intrevalo de procura.</param>
        /// <param name="maxDate">Máximo do intrevalo de procura.</param>
        /// <returns>Coleção de eventos dentro do intrevalo.</returns>
        public async Task<ICollection<Event>> GetEventsBetweenDates(DateTime minDate, DateTime maxDate)
        {
            var list = await _context.Event
                .Include(e => e.UserPublisher)
                    .ThenInclude(e => e.BlockedUsers)
                .Where(e => (e.StartDate >= minDate && e.StartDate <= maxDate) || (e.EndDate >= minDate && e.EndDate <= maxDate))
                .OrderBy(e => e.StartDate)
                .ToListAsync<Event>();
            if (list == null)
                return new List<Event>();

            if (_httpContextAccessor.HttpContext != null)
            {
                var user = await GetCurrentUser();
                if (user != null)
                    list = list.Where(e => !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId || u.SourceUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == user.Id || u.SourceUserId == user.Id)) && e.IsBanned == false).ToList();
            }

            list.ForEach(e => e.UserPublisher = null);

            return list.ToList();
        }


        /// <summary>
        /// Atualiza o estado de todos os eventos de acordo com a data de início, de fim e o seu estado atual.
        /// </summary>
        /// <returns>True: Se as atualizaçãoes foram feitas com sucesso. False: Caso contrário.</returns>
        public async Task<bool> RefreshAllEventsState()
        {
            var operationError = false;

            try
            {
                //Update events to Finish
                var eventsFinish = await _context.Event.Where(e => DateTime.Now >= e.EndDate && e.State == EventState.OnGoing).ToListAsync();

                foreach (var e in eventsFinish)
                {
                    if (e.EndDate.Day != DateTime.Now.Day)
                    {
                        e.State = EventState.Finish;
                    }
                }

                await _context.SaveChangesAsync();

                //Update events to Ongoing
                var eventsOnGoing = await _context.Event.Where(e => e.StartDate <= DateTime.Now && e.State == EventState.Open).ToListAsync();

                foreach (var e in eventsOnGoing)
                {
                    e.State = EventState.OnGoing;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                operationError = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                operationError = true;
            }

            if (operationError)
            {
                Console.WriteLine("Error on updating events.");
                return await Task.FromResult(false);
            }

            Console.WriteLine("Events successful updated.");
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Obtem todos os eventos que um user está interessado, tanto como user ou equipa.
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de eventos</returns>
        public async Task<List<Event>> GetAllUserInterestedEvents(Guid userId)
        {
            return await _context.Event
                .Include(e => e.InterestedTeams)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(m => m.Members)
                .Include(e => e.InterestedUsers)
                    .ThenInclude(e => e.User)
                .Where(e => e.InterestedUsers.Any(i => i.User.Id == userId.ToString()) || e.InterestedTeams.Any(t => (t.Team.OwnerId == userId || t.Team.Members.Any(m => m.Id == userId.ToString())))).ToListAsync();
        }
    }
}

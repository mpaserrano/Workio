using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Matchmaking;
using Workio.Services.Teams;

namespace Workio.Services.LocalizationServices
{
    /// <summary>
    /// Fornece serviços de consulta à base de dados relativamente à comparação de informação para obter recomendação para os utilizadores.
    /// </summary>
    public class MatchmakingService : IMatchmakingService
    {
        /// <summary>
        /// Contexto da base de dados.
        /// </summary>
        private ApplicationDbContext _context;
        /// <summary>
        /// Gestor de utilizadores.
        /// </summary>
        private UserManager<User> _userManager;
        /// <summary>
        /// Serviço de pedidos HTTP.
        /// </summary>
        private IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// Serviço de utilizadores.
        /// </summary>
        private IUserService _userService;
        /// <summary>
        /// Serviço de avaliação de utilizadores.
        /// </summary>
        private IRatingService _ratingService;
        /// <summary>
        /// Serviço de equipas.
        /// </summary>
        private ITeamsService _teamsService;
        /// <summary>
        /// Serviço de eventos.
        /// </summary>
        private IEventsService _eventsService;

        /// <summary>
        /// Inicializa os parâmetros necessários.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="userManager">Gestor de utilizadores.</param>
        /// <param name="httpContextAccessor">Gestor de pedidos HTTP.</param>
        /// <param name="userService">Serviço de utilizadores.</param>
        /// <param name="ratingService">Serviço de avaliação.</param>
        /// <param name="teamsService">Serviço de equipas.</param>
        /// <param name="eventsService">Serviço de eventos.</param>
        public MatchmakingService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService, IRatingService ratingService, ITeamsService teamsService, IEventsService eventsService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _ratingService = ratingService;
            _teamsService = teamsService;
            _eventsService = eventsService;
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
        /// Obtem todas as equipas e ordena-as de acordo com o algoritmo de matchmaking.
        /// O algoritmo utiliza 5 fatores:
        /// -Tags totais do utilizador (TU)
        /// -Tags totais da equipa (TE)
        /// -Tags compativeis (tem o mesmo nome) entre o utililizador e a equipa (TC)
        /// -Rating do utilizador (RU)
        /// -Rating médio da equipa (media da rating de todos os utilizadores da equipa (RE)
        /// Formula:
        /// ((1/TE* TC) + (1/TU* TC)) - (MAX(RE, RU) - MIN(RE, RU))/10
        /// Esta formula devolve um score entre -0.5 e 2
        /// -0.5 é a pior match e 2 é a melhor match
        /// </summary>
        /// <returns>Lista de equipas ordenada</returns>
        public async Task<List<Team>> GetRecommendedTeams()
        {
            User user = await GetCurrentUser();
            Guid userId = new Guid(user.Id);
            double userRating = await _ratingService.GetTrueAverageRating(userId);
            var userTags = await _userService.GetUserSkills(userId);
            double userTotalTags = userTags.Count;

            var teams = await _teamsService.GetOpenNewTeams();

            List<Team> filtTeams = new List<Team>();

            foreach(var team in teams)
            {
                var u = await _context.Users.Include(i => i.BlockedUsers)
                    .Where(e => e.Id == team.OwnerId.ToString()).FirstOrDefaultAsync();

                if(!user.BlockedUsers.Any(i => i.BlockedUserId == u.Id) &&
                    !u.BlockedUsers.Any(i => i.BlockedUserId == user.Id))
                    filtTeams.Add(team);
            }

            teams = filtTeams;

            Dictionary<Team, double> teamScores = new Dictionary<Team, double>();

            foreach (var team in teams)
            {
                double teamRating = await _teamsService.GetAverageRating(team.TeamId);
                var teamTags = team.Skills;
                double teamTotalTags = teamTags.Count;

                //Retorna as tags que tem nomes iguais, foi complicado porque no User são SkillModel, e nas tags são Tag
                //então vê-se pelo Name ToLower quais são iguais

                HashSet<string> userTagsSet = new HashSet<string>(userTags.Select(s => s.Name.ToLower()));
                var matchingTagsList = teamTags.Where(t => userTagsSet.Contains(t.TagName.ToLower())).ToList();

                double matchingTags = matchingTagsList.Count;


                double compatibilityScorePT1 = 0;
                double compatibilityScorePT2 = 0;
                double compatibilityScore = 0;

                if (teamTotalTags == 0 || userTotalTags == 0 || matchingTags == 0)
                {
                    compatibilityScorePT1 = 0;
                }
                else
                {
                    compatibilityScorePT1 = (1 / teamTotalTags * matchingTags) + (1 / userTotalTags * matchingTags);
                }

                if (teamRating > userRating)
                {

                    compatibilityScorePT2 = (teamRating - userRating) / 10;
                }
                else
                {
                    compatibilityScorePT2 = (userRating - teamRating) / 10;
                }

                compatibilityScore = (compatibilityScorePT1 - compatibilityScorePT2);

                if(compatibilityScorePT1 > 0)
                {
                    teamScores.Add(team, compatibilityScore);
                }
            }

            var orderedTeams = teamScores.OrderBy(x => x.Value).ToList();

            List<Team> orderedList = new List<Team>();

            foreach (var team in orderedTeams)
            {
                orderedList.Add(team.Key);
            }


            return orderedList;
        }
        /// <summary>
        /// Metodo para calcular o top 5 de eventos para o utilizador
        /// </summary>
        /// <returns>Devolve uma lista com os 5 melhores eventos para o utilizador</returns>
        public async Task<List<Event>> GetRecommendedEvents()
        {
            User user = await GetCurrentUser();
            Guid userId = new Guid(user.Id);
            var userTags = await _userService.GetUserSkills(userId);
            double userTotalTags = userTags.Count;

            var rawEvents = await _eventsService.GetEvents();
            var events = rawEvents.Where(e => (e.UserId != user.Id) && (e.State == EventState.Open) && (!e.IsBanned) && !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == userId.ToString())));

            events = events.Where(e => !e.InterestedUsers.Any(u => u.User.Id == userId.ToString()) && 
                !e.InterestedTeams.Any(t => t.Team.OwnerId == userId ||
                t.Team.Members.Any(m => m.Id == userId.ToString()))
            );

            Dictionary<Event, double> eventScores = new Dictionary<Event, double>();

            foreach (var @event in events)
            {
                var eventTags = @event.EventTags;
                double eventTotalTags = eventTags.Count;

                //Retorna as tags que tem nomes iguais, foi complicado porque no User são SkillModel, e nas tags são Tag
                //então vê-se pelo Name ToLower quais são iguais

                HashSet<string> userTagsSet = new HashSet<string>(userTags.Select(s => s.Name.ToLower()));
                var matchingTagsList = eventTags.Where(t => userTagsSet.Contains(t.EventTagName.ToLower())).ToList();

                double matchingTags = matchingTagsList.Count;


                double compatibilityScorePT1 = 0;
                double compatibilityScore = 0;

                if (eventTotalTags == 0 || userTotalTags == 0 || matchingTags == 0)
                {
                    compatibilityScorePT1 = 0;
                }
                else
                {
                    compatibilityScorePT1 = (1 / eventTotalTags * matchingTags) + (1 / userTotalTags * matchingTags);
                }

                compatibilityScore = compatibilityScorePT1;

                if (compatibilityScore > 0)
                {
                    eventScores.Add(@event, compatibilityScore);
                }
            }

            var orderedEvents = eventScores.OrderBy(x => x.Value).ToList();

            List<Event> orderedList = new List<Event>();

            foreach (var @event in orderedEvents)
            {
                orderedList.Add(@event.Key);
            }


            return orderedList;
        }

        /// <summary>
        /// Recebe as coordenadas de um ponto, um evento e a distânica e retorna se
        /// o evento encontra-se a menor ou igual distancia do ponto.
        /// </summary>
        /// <param name="lat1">Latitude do ponto.</param>
        /// <param name="long1">Longitude do ponto.</param>
        /// <param name="event">Evento para a comparação.</param>
        /// <param name="distance">Distância de área pretendida para passar na validação.Se comparação das distância é menor
        /// ou igual a esta distancia é verdadeiro.</param>
        /// <returns>True: Se estiver na área. False: Se não tiver na área.</returns>
        private bool IsEventNear(double lat1, double long1, Event @event, double distance)
        {
            if (!@event.Latitude.HasValue || !@event.Longitude.HasValue) return false;
            return DistanceTo(lat1, long1, @event.Latitude.Value, @event.Longitude.Value, 'K') <= distance;
        }

        /// <summary>
        /// Retorna a distância a que um eventos está relativamente à posição recebida.
        /// </summary>
        /// <param name="latitute">Latitude da posição do utilizador.</param>
        /// <param name="longitude">Logintude da posição do utilizador.</param>
        /// <param name="event">Evento com o qual comparar as coordenadas.</param>
        /// <returns>A distância entre a posição recebida e o evento em linha reta.</returns>
        private static double EventDistance(double lat1, double long1, Event @event)
        {
            if (!@event.Latitude.HasValue || !@event.Longitude.HasValue) return -1;
            return DistanceTo(lat1, long1, @event.Latitude.Value, @event.Longitude.Value, 'K');
        }

        /// <summary>
        /// Retorna a disância entre os dois pontos de coordenadas recebidos.
        /// </summary>
        /// <param name="lat1">Latitude ponto 1.</param>
        /// <param name="lon1">Longitude ponto 1.</param>
        /// <param name="lat2">Latitude ponto 2</param>
        /// <param name="lon2">Longitude ponto 2</param>
        /// <param name="unit">Unidades (K, N, M) (Km, milhas nauticas, milhas) por default é K.</param>
        /// <returns>A distânica das coordenadas do ponto 1 aos ponto 2</returns>
        private static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }

        /// <summary>
        /// Recebe informações da localização do utilizador e área a abranger de pesquisa e
        /// retorna uma lista de eventos dos eventos presenciaís que se encontrem nessa área.
        /// </summary>
        /// <param name="latitute">Latitude da posição do utilizador.</param>
        /// <param name="longitude">Logintude da posição do utilizador.</param>
        /// <param name="distance">Área de procura por eventos.</param>
        /// <returns>Lista de eventos que estão numa área circular relativamente à posição do utilizador.</returns>
        public async Task<List<Event>> GetEventsNear(double latitute, double longitude, double distance)
        {
            //https://localhost:7212/Events/GetCloseEvents?latitude=38.6529803&longitude=-8.9797575&distance=10
            return _context.Event.Include(u => u.UserPublisher).AsEnumerable()
                .Where(e =>
                    (e.IsInPerson)
                    && (IsEventNear(latitute, longitude, e, distance)))
                .ToList();
        }

        /// <summary>
        /// Recebe informações sobre uma localização e obtem eventos próximos a essa localização até uma
        /// determinada distância.
        /// </summary>
        /// <param name="latitute">Latitude da posição do utilizador.</param>
        /// <param name="longitude">Logintude da posição do utilizador.</param>
        /// <param name="distance">Área de procura por eventos.</param>
        /// <returns>Lista de eventos que estão numa área circular relativamente à posição do utilizador.</returns>
        public async Task<IEnumerable<Object>> GetEventsNearWithDistances(double latitute, double longitude, double distance)
        {
            List<Event> eventsNearData = await GetEventsNear(latitute, longitude, distance);
            var returnData = eventsNearData.Select(e => new
            {
                Id = e.EventId,
                BannerSrc = e.BannerPicturePath,
                Title = e.Title,
                UserPublisher = e.UserPublisher.Name,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                StartDate = e.StartDate.ToString("dd/MM/yyyy"),
                EndDate = e.EndDate.ToString("dd/MM/yyyy"),
                State = e.State,
                Distance = EventDistance(latitute, longitude, e),
                IsFeatured = e.IsFeatured,
            }).OrderBy(e => e.Distance);
            return returnData.ToList();
        }
    }
}

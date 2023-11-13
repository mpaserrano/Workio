using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Security.Claims;
using Workio.Services.Events;
using Workio.Models.Events;
using Workio.Models;
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Workio.Services;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador para as inscrições nos eventos
    /// </summary>
    public class EventEnrollmentsController : Controller
    {
        /// <summary>
        /// Objeto do tipo ToastNotification para as notificações das ações realizadas
        /// </summary>
        private readonly IToastNotification _toastNotification;
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas aos eventos
        /// </summary>
        private IEventsService _eventsService;
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas aos utilizadores
        /// </summary>
        private readonly IUserService _userService;
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas às equipas
        /// </summary>
        private readonly ITeamsService _teamsService;
        /// <summary>
        /// Serviço para informações relacionadas à localização
        /// </summary>
        private readonly CommonLocalizationService _localizationService;

        /// <summary>
        /// Construtor da classe
        ///<param name="toastNotification">Objeto do tipo ToastNotification para as notificações das ações realizadas</param>
        ///<param name="eventsService">Serviço para intereações com a base de dados relacionadas aos eventos</param>
        ///<param name="userService">Serviço para intereações com a base de dados relacionadas aos utilizadores</param>
        ///<param name="teamsService">Serviço para intereações com a base de dados relacionadas às equipas</param>
        ///<param name="localizationService">Serviço para informações relacionadas à localização</param>
        /// </summary>
        public EventEnrollmentsController(IToastNotification toastNotification, IEventsService eventsService, IUserService userService, ITeamsService teamsService, CommonLocalizationService localizationService)
        {
            _toastNotification = toastNotification;
            _eventsService = eventsService;
            _userService = userService;
            _teamsService = teamsService;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Metodo utilizado para inscrever um utilizador no evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Reedireciona para a página de detalhes do evento em questão</returns>
        public async Task<IActionResult> EnrollUser(Guid id)
        {
            if (await _eventsService.IsTeamInterested(id))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("EnrollMemberWithTeam"));
                return RedirectToAction("Details", "Events", new { id = id });
            }

            var result = await _eventsService.AddInterestedUser(id);
            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("Enrolled"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("EnrollFail"));

            }

            return RedirectToAction("Details", "Events", new { id = id });
        }
        /// <summary>
        /// Metodo utilizado para desinscrever um utilizador no evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Reedireciona para a página de detalhes do evento em questão</returns>
        public async Task<IActionResult> CancellEnrollmentUser(Guid id)
        {
            var result = await _eventsService.RemoveInterestedUser(id);
            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("CancelEnroll"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CancelEnrollFail"));

            }

            return RedirectToAction("Details", "Events", new { id = id });
        }

        /// <summary>
        /// Metodo utilizado para reedirecionar para a página onde a equipa se inscreve
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Reedireciona para a página de detalhes do evento em questão</returns>
        public async Task<IActionResult> TeamEnrollment(Guid id)
        {
            if (await _eventsService.IsUserInterested(id))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CantEnrollAsUserIfTeam"));
                return RedirectToAction("Details", "Events", new { id = id });
            }
            ICollection<Team> teamlist = await _teamsService.GetTeams();
            List<Team> teamsOwned = new List<Team>();
            ViewData["id"] = id;
            foreach (Team team in teamlist)
            {
                if (team.OwnerId == CurrentUserId())
                {
                    teamsOwned.Add(team);
                }
            }
            ViewBag.OwnedTeams = new SelectList(teamsOwned, "TeamId", "TeamName");
            return View();
        }

        /// <summary>
        /// Metodo utilizado para inscrever uma equipa no evento
        /// </summary>
        /// <param name="team">equipa que deve de ser inscrita no evento</param>
        /// <param name="id">Id do evento</param>
        /// <returns>Reedireciona para a página de detalhes do evento em questão</returns>
        [HttpPost]
        public async Task<IActionResult> TeamEnrollment(Team team, Guid id)
        {
            var realTeam = await _teamsService.GetTeamById(team.TeamId);
            var result = await _eventsService.AddInterestedTeam(realTeam, id);
            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamEnroll"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("EnrollFail"));

            }

            return RedirectToAction("Details", "Events", new { id = id });
        }
        /// <summary>
        /// Metodo utilizado para desinscrever uma equipa no evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Reedireciona para a página de detalhes do evento em questão</returns>
        public async Task<IActionResult> CancelTeamEnrollment(Guid id)
        {
            var result = await _eventsService.RemoveInterestedTeam(id);
            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("CancelTeamEnroll"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CancelEnrollFail"));

            }

            return RedirectToAction("Details", "Events", new { id = id });
        }
        /// <summary>
        /// Metodo utilizado para apresentar a página de inscrições em um certo evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>View das inscrições</returns>
        public async Task<IActionResult> EventEnrollments(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }
            var result = await _eventsService.GetEvent(id);
            var users = new List<User>();
            var teams = new List<Team>();

            if (result != null)
            {
                users = result.InterestedUsers.Select(x => x.User).ToList();
                teams = result.InterestedTeams.Select(x => x.Team).ToList();
            }

            ViewBag.Users = users;
            ViewBag.Teams = teams;
            ViewBag.EventId = id;
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userService.GetUser(CurrentUserId());

                ViewBag.OwnsInterestedTeam = await _eventsService.IsTeamInterested(id);

                if (result.InterestedTeams.Where(t => t.Team.Members.Contains(user)).Count() != 0)
                {
                    ViewBag.IsMemberOfInterestedTeam = true;

                }
                else
                {
                    ViewBag.IsMemberOfInterestedTeam = false;

                }
            }
            else
            {
                ViewBag.OwnsInterestedTeam = true;
                ViewBag.IsMemberOfInterestedTeam = true;
            }


            return View(result);
        }
        /// <summary>
        /// Metodo utilizado para fornecer ao utilizador uma equipa aleatória que esteja inscrita no evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>Apresenta a equipa, ou caso o utilizador nao esteja logado fornece uma mensagem de erros</returns>
        public async Task<IActionResult> RandomTeam(Guid id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("NotLoggedIn"));
                return RedirectToAction("EventEnrollments", new { id = id });
            }
            var @event = await _eventsService.GetEvent(id);
            var interestedUsers = @event.InterestedUsers;
            var currentUser = await _userService.GetUser(CurrentUserId());
            var interestedTeams = @event.InterestedTeams.Where(t => !t.Team.Members.Contains(currentUser) && t.Team.Status == TeamStatus.Open);
            //verificar se o evento tem equipas interessadas ou se existe
            if (@event == null || interestedTeams.Count() == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("NoInterestedTeams"));
                return RedirectToAction("Details", "Events", new { id = id });
            }
            //verificar se o utilizador está a demonstrar interesse no evento
            if (await _eventsService.IsUserInterested(id))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("AlreadyEnrollAsUser"));
                return RedirectToAction("Details", "Events", new { id = id });
            }
            //verificar se o utilizador é dono de alguma equipa que esteja interessada no evento
            if (await _eventsService.IsTeamInterested(id))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("OwnATeamInterested"));
                return RedirectToAction("Details", "Events", new { id = id });
            }

            Random rnd = new Random();
            int ranIndex = rnd.Next(1, @event.InterestedTeams.Count() + 1);
            InterestedTeam team = @event.InterestedTeams.Skip(ranIndex - 1).First();


            return RedirectToAction("Details", "Teams", new { id = team.Team.TeamId });
        }

        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}

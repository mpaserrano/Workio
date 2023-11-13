using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NToastNotify;
using Workio.Data;
using Workio.Models;
using Workio.Services.LocalizationServices;
using Workio.Services.Teams;
using System.Linq;
using Workio.Services.ReportServices;
using Workio.Services.Search;
using Workio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Workio.Models.Filters;
using Workio.Services.Matchmaking;
using Workio.Services.Email.Interfaces;
using Org.BouncyCastle.Asn1.Ocsp;
using Workio.Managers.Notifications;
using Workio.Services;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Workio.Services.Chat;
using Workio.Models.Events;
using System.Web;

namespace Workio.Controllers
{
    /// <summary>
    /// Gere os pedidos relativamente às equipas.
    /// </summary>
    public class TeamsController : Controller
    {
        /// <summary>
        /// Gestão de utilizadores.
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Gestor de login.
        /// </summary>
        private SignInManager<User> _signInManager;
        /// <summary>
        /// Serviço de gestão de equipas na base de dados.
        /// </summary>
        private ITeamsService _teamsService;
        /// <summary>
        /// Serviço de mostragem de notificações.
        /// </summary>
        private readonly IToastNotification _toastNotification;
        /// <summary>
        /// Serviço de traduções.
        /// </summary>
        private readonly ILocalizationService _localizationService;
        /// <summary>
        /// Serviço de denúncias.
        /// </summary>
        private readonly IReportReasonService _reportReasonService;
        /// <summary>
        /// Serviço de pesquisa.
        /// </summary>
        private readonly ISearchService _searchService;
        /// <summary>
        /// Serviço de utilizadores.
        /// </summary>
        private readonly IUserService _userService;
        /// <summary>
        /// Serviço de comparações para pesquisa.
        /// </summary>
        private readonly IMatchmakingService _matchmakingService;
        /// <summary>
        /// Serviço de email.
        /// </summary>
        private readonly IEmailService _emailService;
        /// <summary>
        /// Serviço do chat
        /// </summary>
        private readonly IChatService _chatService;
        /// <summary>
        /// Manager das notificações
        /// </summary>
        private readonly INotificationManager _notificationManager;
        /// <summary>
        /// Serviço de tradução
        /// </summary>
        private readonly CommonLocalizationService _commonLocalizationService;

        /// <summary>
        /// Inicializa os serviços e componentes necessários. Recebe do DI.
        /// </summary>
        /// <param name="userManager">Gestão de utilizadors.</param>
        /// <param name="signInManager">Gestor de login.</param>
        /// <param name="teamsService">Serviço de equipas.</param>
        /// <param name="toastNotification">Serviço de mostragem de notificações.</param>
        /// <param name="localizationService">Serviço de traduções.</param>
        /// <param name="reportReasonService">Serviço de denúncias.</param>
        /// <param name="searchService">Serviço de procura.</param>
        /// <param name="userService">Serviço de utilizadores.</param>
        /// <param name="matchmakingService">Serviço de comparações e pesquisa.</param>
        /// <param name="emailService">Serviço de email.</param>
        /// <param name="chatService">Serviço do chat</param>
        /// <param name="notificationManager">Gestor de notificações</param>
        /// <param name="commonLocalizationService">Serviço de tradução</param>
        public TeamsController(UserManager<User> userManager, SignInManager<User> signInManager, ITeamsService teamsService, IToastNotification toastNotification, ILocalizationService localizationService,
            IReportReasonService reportReasonService, ISearchService searchService, IUserService userService, IMatchmakingService matchmakingService, IEmailService emailService,
            IChatService chatService, INotificationManager notificationManager, CommonLocalizationService commonLocalizationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _teamsService = teamsService;
            _toastNotification = toastNotification;
            _localizationService = localizationService;
            _reportReasonService = reportReasonService;
            _searchService = searchService;
            _userService = userService;
            _matchmakingService = matchmakingService;
            _emailService = emailService;
            _chatService = chatService;
            _notificationManager = notificationManager;
            _commonLocalizationService = commonLocalizationService;
        }

        // GET: Teams
        /// <summary>
        /// Recebe filtros e retorna uma página com as equipas filtradas.
        /// </summary>
        /// <param name="selectedFilters">Filtros a aplicar.</param>
        /// <returns>Página com filtros aplicados.</returns>
        public async Task<IActionResult> Index(TeamsFilterViewModel givenFilters)
        {
            List<int> selectedFilters = givenFilters.selectedFilters.ToList();
            string filterName = givenFilters.Name;

            var user = new User();
            var allTeams = new List<Team>();
            var myTeams = new List<Team>();
            var pendingList = new List<TeamInviteUser>();
            var filters = new TeamsFilterViewModel();
            ViewBag.Filters = filters;
            var unbannedteams = await _teamsService.GetTeams();
            var unbannedList = unbannedteams.Where(t => t.IsBanned == false);
            allTeams = new List<Team>(unbannedList);

            if (_signInManager.IsSignedIn(User))
            {
                user = await _userService.GetUser(CurrentUserId());
                filters.currentUserId = Guid.Parse(user.Id);
                pendingList = user.TeamsRequests.Where(c => c.Team.IsBanned != false).ToList();
                myTeams = new List<Team>((await _teamsService.GetMyTeams()));

                List<Team> filtTeams = new List<Team>();

                foreach (var team in allTeams)
                {
                    var u = await _userService.GetUser(team.OwnerId.Value);

                    if (!user.BlockedUsers.Any(i => i.BlockedUserId == u.Id) &&
                        !u.BlockedUsers.Any(i => i.BlockedUserId == user.Id))
                        filtTeams.Add(team);
                }

                allTeams = filtTeams;
            }

            if (!string.IsNullOrEmpty(filterName))
            {
                filters.Name = filterName;
                if (selectedFilters.Count() == 0)
                    selectedFilters.Add(4);
            }

            if (selectedFilters != null && selectedFilters.Count() > 0 && filters.filters != null && filters.filters.Count > 0)
            {
                if (selectedFilters.Count() > 1 && selectedFilters.Contains(4))
                    selectedFilters.Remove(4);

                filters.selectedFilters = selectedFilters.ToArray();
                ViewBag.Filters = filters;
                var filteredAllTeams = new List<Team>();
                var filteredTeams = new List<Team>();
                var filteredInvites = new List<TeamInviteUser>();

                foreach (var index in selectedFilters)
                {
                    var filter = filters.filters.FirstOrDefault(f => f.Index == index);
                    if (filter != null)
                    {
                        var _fTeams = myTeams.Where(filter.Condition).ToList();
                        foreach(Team t in _fTeams)
                        {
                            if (!filteredTeams.Contains(t))
                            {
                                filteredTeams.Add(t);
                            }
                        }

                        var _fInvite = pendingList.Where(ti => filter.Condition(ti.Team));
                        foreach (TeamInviteUser t in _fInvite)
                        {
                            if (!filteredInvites.Contains(t))
                            {
                                filteredInvites.Add(t);
                            }
                        }

                        var _fAll = allTeams.Where(filter.Condition).ToList();
                        foreach (Team t in _fAll)
                        {
                            if (!filteredAllTeams.Contains(t))
                            {
                                filteredAllTeams.Add(t);
                            }
                        }
                    }
                }

                allTeams = filteredAllTeams;
                myTeams = filteredTeams;
                user.TeamsRequests = filteredInvites;
            }

            ViewBag.Teams = allTeams;
            ViewBag.MyTeams = myTeams;

            ViewBag.RecommendedTeams = new List<Team>();

            if (_signInManager.IsSignedIn(User))
            {
                List<Team> recommendedTeams = await _matchmakingService.GetRecommendedTeams();
                if (recommendedTeams.Count > 0)
                {
                    recommendedTeams.Reverse();
                    ViewBag.RecommendedTeams = recommendedTeams;
                }
            }

            return View(user);
        }

        /// <summary>
        /// Redireciona o utilizador para o chat e mostra a conversa da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> SendMessage(Guid teamId, string? returnUrl)
        {
            var userId = CurrentUserId();

            var chat = await _chatService.GetTeamChatRoomById(teamId);

            if (chat == null)
            {
                if (returnUrl == null) returnUrl = Url.Action("Details", "Teams", new { id = teamId });
                return RedirectToAction("CreateTeamChatRoom", "Chat", new
                {
                    teamId = teamId,
                    returnUrl = returnUrl
                });
            }

            var userInChat = chat.Members.Any(m => m.UserId == userId.ToString());

            if (userInChat)
            {
                return RedirectToAction("Index", "Chat", new { roomId = chat.ChatRoomId });
            }
            
            if (returnUrl == null) returnUrl = Url.Action("Details", "Teams", new { id = teamId });

            return RedirectToAction("AddToTeamChatRoom", "Chat", new
            {
                teamId = teamId,
                userId = userId,
                returnUrl = returnUrl
            });
        }


        // GET: Teams/Details/5
        /// <summary>
        /// Mostra os detalhes de uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página com os detalhes da equipa.</returns>
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamsService.GetTeamById(id.Value);

            if (team == null || team.OwnerId == null)
            {
                return NotFound();
            }

            if (team.IsBanned)
            {
                return View("Banned");
            }

            var ownerUser = await _userService.GetUser(team.OwnerId.Value);

            if(ownerUser == null)
            {
                return NotFound();
            }

            ViewBag.IsMember = false;
            ViewBag.Owner = false;
            ViewBag.InPendingList = false;
            ViewBag.AlreadyInvited = false;
            ViewBag.OwnerUser = ownerUser;
            

            if (_signInManager.IsSignedIn(User))
            {
                var isBlocked = await _userService.IsBlockedByUser(Guid.Parse(ownerUser.Id));

                if (isBlocked && !(User.IsInRole("Admin") || User.IsInRole("Mod")))
                {
                    return View("Blocked");
                }

                if (User != null && CurrentUserId() == team.OwnerId)
                {
                    ViewBag.Owner = true;
                }
                else if (User != null && IsMember(team))
                {
                    ViewBag.IsMember = true;
                }
                else if (User != null && InPendingList(team))
                {
                    ViewBag.InPendingList = true;
                }
                else if(User != null && AlreadyInvited(team))
                {
                    ViewBag.AlreadyInvited = true;
                }
            }
            
            return View(team);
        }

        //Teams/Details/5/FinishProject
        /// <summary>
        /// Realiza a ação de terminar uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página dos detalhes da equipa atualizada.</returns>
        [ActionName("FinishProject")]
        public async Task<IActionResult> FinishProject(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _teamsService.ChangeTeamStatus(TeamStatus.Finish, id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("TeamFinished"));
                await SendTeamStatusChangeNotification(id, TeamStatus.Finish);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("TeamStatusFail"));
            }

            return RedirectToAction("Details", new { id = id});
        }

        //Teams/Details/5/OpenInscriptions
        [ActionName("OpenInscriptions")]
        public async Task<IActionResult> OpenInscriptions(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _teamsService.ChangeTeamStatus(TeamStatus.Open, id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InscriptionsOpen"));
                await SendTeamStatusChangeNotification(id, TeamStatus.Open);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("TeamStatusFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        //Teams/Details/5/CloseInscriptions
        /// <summary>
        /// Fecha o aceitamento de inscrições numa equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página de detalhes com as informações atualizadas.</returns>
        [ActionName("CloseInscriptions")]
        public async Task<IActionResult> CloseInscriptions(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _teamsService.ChangeTeamStatus(TeamStatus.Closed, id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InscriptionsClosed"));
                await SendTeamStatusChangeNotification(id, TeamStatus.Closed);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("TeamStatusFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        //Teams/Details/5/AcceptUser
        /// <summary>
        /// Aceita o pedido de adesão de um utilizador a uma equipa.
        /// </summary>
        /// <param name="id">Id do pedido de adesão.</param>
        /// <param name="teamId">Id da equipa solicitada.</param>
        /// <returns>Página de detalhes da equipa com a informações atualizada.</returns>
        [Authorize]
        [ActionName("AcceptUser")]
        public async Task<IActionResult> AcceptUser(Guid id, Guid teamId)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Guarda a informação para poder enviar o email posteriormente
            PendingUserTeam pendingUserTeam = await _teamsService.GetRequestById(id);

            var success = await _teamsService.AcceptAccess(id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UserAddedSuccess"));

                //Add user to chatroom

                var chatResult = await _chatService.AddUserToTeamChatRoom(teamId, pendingUserTeam.User);

                //Send an email notification
                //RequestId, teamId
                await SendNewTeamRequestAcceptedNotification(pendingUserTeam, teamId);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UserAddError"));
            }

            return RedirectToAction("Details", new { id = teamId });
        }

        //Teams/Details/5/RemoveUser
        /// <summary>
        /// Remove um utilzador de uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <param name="userId">Id do utilizador a remover.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        [ActionName("RemoveUser")]
        public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
        {
            if (id == null || userId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.RemoveUser(id, userId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UserRemovedSuccess"));

                // Remove user from chatroom
                var user = await _userService.GetUser(userId);

                var chatResult = await _chatService.RemoveUserFromTeamChatRoom(id, user);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UserRemoveFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        //Teams/Details/5/RejectUser
        /// <summary>
        /// Rejeita o pedido de adesão de um utilizador a um equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <param name="requestId">Id do pedido.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        [ActionName("RejectUser")]
        public async Task<IActionResult> RejectUser(Guid id, Guid requestId)
        {
            if (id == null || requestId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.RejectAccess(requestId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UserRejectedSuccess"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UserRejectedFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        //Teams/Details/5/AskAccess
        /// <summary>
        /// Executa um pedido de adesão a uma equipa por um utilizador.
        /// </summary>
        /// <param name="id">Id da equipa que pretende solicitar entrada.</param>
        /// <param name="userId">Id do utilizador.</param>
        /// <returns>Página de detalhes da equipa com a informação atualizada.</returns>
        [ActionName("AskAccess")]
        public async Task<IActionResult> AskAccess(Guid id, Guid userId)
        {
            if (id == null || userId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.AskAccess(id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("AddedPendingListSuccess"));

                //Send an email notification
                await SendNewTeamRequestNotification(id);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("AddedPendingListFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Cancela o envio de um pedido de participação.
        /// </summary>
        /// <param name="requestId">Id de pedido.</param>
        /// <param name="teamId">Id da equipa.</param>
        /// <returns>Página com os detalhes da equipa atualizados.</returns>
        [ActionName("CancelInvite")]
        public async Task<IActionResult> CancelInvite(Guid requestId, Guid teamId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.CancelInvite(requestId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteCanceledSuccess"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteCanceledFail"));
            }

            return RedirectToAction("Details", new { id = teamId });
        }

        /// <summary>
        /// Aceita um pedido de participação numa equipa.
        /// </summary>
        /// <param name="requestId">Id do pedido.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        [ActionName("AcceptInvite")]
        public async Task<IActionResult> AcceptInvite(Guid requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.AcceptInvite(requestId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteAcceptedSuccess"));

                var team = await _teamsService.GetTeamByInviteId(requestId);

                if(team != null)
                {
                    var userId = CurrentUserId();
                    var user = await _userService.GetUser(userId);

                    var chatResult = await _chatService.AddUserToTeamChatRoom(team.TeamId, user);
                }   
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteAcceptedFail"));
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Aceita o pedido de participação numa equipa pelo id de equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        public async Task<IActionResult> AcceptInviteByTeam(Guid teamId)
        {
            if (teamId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.AcceptInviteByTeam(teamId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteAcceptedSuccess"));

                // Add user to team chatroom
                var userId = CurrentUserId();

                var user = await _userService.GetUser(userId);

                var chatResult = await _chatService.AddUserToTeamChatRoom(teamId, user);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteAcceptedFail"));
            }
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        /// <summary>
        /// Rejeita um pedido de participação num equipa.
        /// </summary>
        /// <param name="requestId">Id do pedido.</param>
        /// <returns>Página de lista de equipas.</returns>
        public async Task<IActionResult> RejectInvite(Guid requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.RejectInvite(requestId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteRejectedSuccess"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteRejectedFail"));
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Rejeita um pedido de participação pelo id da equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        public async Task<IActionResult> RejectInviteByTeam(Guid teamId)
        {
            if (teamId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.RejectInviteByTeam(teamId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteRejectedSuccess"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteRejectedFail"));
            }
            return RedirectToAction(nameof(Details), new { id = teamId});
        }

        //Teams/Details/5/AskAccess
        /// <summary>
        /// Ação de deixar uma equipa por parte de um membro.
        /// </summary>
        /// <param name="id">Id da equipa a deixar.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        [ActionName("Leave")]
        public async Task<IActionResult> Leave(Guid id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _teamsService.LeaveTeam(id);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("LeftTeamSuccess"));

                // Remove user from team chat room
                var userId = CurrentUserId();

                var user = await _userService.GetUser(userId);

                var chatResult = await _chatService.RemoveUserFromTeamChatRoom(id, user);

                if(string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction(nameof(Index));

                return LocalRedirect(returnUrl);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("LeftTeamFail"));
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Details", new { id = id });

            return LocalRedirect(returnUrl);

            
        }

        // GET: Teams/Create
        /// <summary>
        /// Obtem a página para criar um equipa.
        /// </summary>
        /// <returns>Página com a criação da equipa.</returns>
        public async Task<IActionResult> Create()
        {
            if (!_signInManager.IsSignedIn(User))
                return RedirectToAction("Index");

            ViewBag.Languages = await _localizationService.GetLocalizations();
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Cria uma equipa.
        /// </summary>
        /// <param name="team">Objeto equipa com os dados para a criação.</param>
        /// <returns>Página de lista de equipas.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName,Description, LanguageLocalizationId, Tags, PositionsString")] Team team)
        {
            Console.WriteLine("Tags: " + team.Tags);
            team.TeamId = Guid.NewGuid();

            if(team.Tags != null && team.Tags != "")
            {
                string[] tagsArray = team.Tags.Split(',');
                foreach(string tag in tagsArray)
                {
                    var t = new Tag();
                    t.TagId = Guid.NewGuid();
                    t.TagName = tag.Trim();
                    team.Skills.Add(t);
                }
            }

            if(team.PositionsString != null && team.PositionsString != "")
            {
                string[] positionsArray = team.PositionsString.Split(',');
                foreach (string position in positionsArray)
                {
                    var p = new Position();
                    p.PositionId = Guid.NewGuid();
                    p.Name = position.Trim();
                    team.Positions.Add(p);
                }
            }

            var success = await _teamsService.CreateTeam(team);

            if (!success)
            {
                ViewBag.Languages = await _localizationService.GetLocalizations();
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("CreateTeamFail"));
                return View(team);
            }
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("CreateTeamSuccess"));
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Edit/5
        /// <summary>
        /// Obtem a página para editar os dados de uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página com os dados para editar.</returns>
        public async Task<IActionResult> Edit(Guid id)
        {
            var team = await _teamsService.GetTeamById(id);
            if (team == null)
            {
                return NotFound();
            }

            if (team.IsBanned)
            {
                return View("Banned");
            }

            if (team.Positions != null && team.Positions.Count > 0)
                team.PositionsString = string.Join(",", team.Positions.Select(p => p.Name));

            if (team.Skills != null && team.Skills.Count > 0)
                team.Tags = string.Join(",", team.Skills.Select(t => t.TagName));

            ViewBag.Languages = await _localizationService.GetLocalizations();
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Altera os dados da equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <param name="team">Objeto equipa.</param>
        /// <returns>Retorna a página de detalhes de uma equipa com os dados alterados.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("TeamId,TeamName,Description, LanguageLocalizationId, Tags, PositionsString")] Team team)
        {
            if (id != team.TeamId)
            {
                return NotFound();
            }

            // Split the tags string into an array of tag names
            var tagNames = team.Tags?.Split(',');
            // Create a new list of tags from the tag names
            var tags = tagNames?.Select(name => new Tag { TagName = name.Trim() }).ToList();
            // Set the Skills property of the team object to the new list of tags
            team.Skills = tags;

            // Split the position string into an array of position names
            var positionNames = team.PositionsString?.Split(',');
            // Create a new list of positions from the tag names
            var positions = positionNames?.Select(name => new Position { Name = name.Trim() }).ToList();
            // Set the Positions property of the team object to the new list of positions
            team.Positions = positions;

            var success = await _teamsService.UpdateTeam(team);

            if (!success)
            {
                ViewBag.Languages = await _localizationService.GetLocalizations();
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UpdateTeamFail"));
                return View(team);
            }
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UpdateTeamSuccess"));
            return RedirectToAction(nameof(Details), new {id = id});

        }

        // GET: Teams/Delete/5
        /// <summary>
        /// Página para apaghar uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página de confirmação para apagar a equipa.</returns>
        public async Task<IActionResult> Delete(Guid? id)
        {
            /*if (id == null || _context.Team == null)
            {
                return NotFound();
            }

            var team = await _context.Team
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }
            */
            return View();
        }

        // POST: Teams/Delete/5
        /// <summary>
        /// Apaga uma equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>Página de lista de equipas.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            /*if (_context.Team == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Team'  is null.");
            }
            var team = await _context.Team.FindAsync(id);
            if (team != null)
            {
                _context.Team.Remove(team);
            }
            
            await _context.SaveChangesAsync();*/
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Retorna um componente modal de diálog de decisão.
        /// </summary>
        /// <param name="teamId">Id de equipa</param>
        /// <returns>Componente modal.</returns>
        public IActionResult OpenModal(Guid teamId)
        {
            return ViewComponent("BootstrapModal", new { users = new List<User>(), teamId = teamId});
        }

        /// <summary>
        /// Procura pedidos de participação.
        /// </summary>
        /// <param name="query">Dados para a pesquisa.</param>
        /// <param name="teamId">Id da equipa.</param>
        /// <returns>ViewComponent com o resultado da procura.</returns>
        [Authorize]
        public async Task<IActionResult> InviteSearch(string query, Guid teamId)
        {
            // Query your database or any other data source based on the search query
            var currentUser = await _userService.GetUser(CurrentUserId());

            var results = new List<User>();
            if(query != null)
            {
                if (query.Contains("@"))
                {
                    results = await _searchService.GetUsersByEmail(query);
                }
                else
                {
                    results = await _searchService.GetUsersByName(query);
                }
                if(results != null)
                {
                    var user = results.Where(r => r.Id == CurrentUserId().ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        results.Remove(user);
                    }

                    results = results.Where(r => !r.BlockedUsers.Any(u => u.BlockedUser == currentUser) && !currentUser.BlockedUsers.Any(b => b.BlockedUserId == r.Id)).ToList();
                         
                    results = results.Where(r =>
                        // Check if the user has any team requests
                        (r.TeamsRequests.Any()
                            // Exclude the user if they have any pending requests for the team
                            && !r.TeamsRequests.Any(t => t.TeamId == teamId && t.UserId == r.Id && t.Status == PendingUserTeamStatus.Pending))
                        // Include the user if they have no team requests or none with pending status
                        || (!r.TeamsRequests.Any() && (r.Teams.Any(t => t.TeamId != teamId) || r.Teams.Count == 0))
                    ).ToList();
                }
            }
            
            if (results == null) results = new List<User>();
            return ViewComponent("BootstrapModal", new { users = results, teamId = teamId });
        }

        /// <summary>
        /// Convida um utilizador para um equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa.</param>
        /// <param name="userId">Id do utilizador.</param>
        /// <returns>Página de detalhes com as informações atualizadas.</returns>
        public async Task<IActionResult> InviteToTeam(Guid teamId, Guid userId)
        {
            Console.WriteLine($"{teamId}/{userId}");
            if(teamId == null || userId == null)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("InviteUserError"));
            }

            var success = await _teamsService.InviteUserToTeam(teamId, userId);

            if (!success)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("WhileInviteUserError"));
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("InviteUserSuccess"));
                await SendInviteToTeamNotification(teamId, userId);
            }    

            return RedirectToAction(nameof(Details), new { id = teamId});
        }

        /// <summary>
        /// Ação para adicionar uma milestone à equipa
        /// </summary>
        /// <param name="milestone">Modelo com os dados da milestone</param>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>IActionResult - Redireciona para a página onde estavamos para recarregar os dados</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMilestone(Milestone milestone, Guid teamId)
        {
            if(milestone == null || teamId == null)
            {
                return NotFound();
            }

            milestone.MilestoneId = Guid.NewGuid();
            milestone.TeamId = teamId;

            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("FillAllFields"));
                return RedirectToAction(nameof(Details), new { id = teamId });
            }

            var success = await _teamsService.AddMilestone(milestone, teamId);

            if (!success)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("AddMilestoneFail"));
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("AddMilestoneSuccess"));
            }
            
            return RedirectToAction(nameof(Details), new { id = teamId});
        }
        /// <summary>
        /// Controlador para editar uma milestone
        /// </summary>
        /// <param name="milestone">Recebe a milestone a ser atualizada</param>
        /// <param name="teamId">O id da equipa</param>
        /// <returns>Página da equipa</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMilestone(Milestone milestone, Guid milestoneId, Guid teamId)
        {
            if(milestoneId == null)
            {
                return NotFound();
            }

            milestone.MilestoneId = milestoneId;
            
            if (!ModelState.IsValid)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UpdateMilestoneFail"));
                return RedirectToAction(nameof(Details), new { id = teamId });
            }

            var result = await _teamsService.UpdateMilestone(milestone, teamId);

            if (!result)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UpdateMilestoneFail"));
            }
            else
            {
                //Success
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UpdateMilestoneSuccess"));
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }
        /// <summary>
        /// Controlador para eliminar uma milestone
        /// </summary>
        /// <param name="milestoneId">Recebe o id da milestone</param>
        /// <returns>Página da equipa atualizada</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMilestone(Guid milestoneId, Guid teamId)
        {

            if (milestoneId == null)
            {
                return NotFound();
            }

            var result = await _teamsService.DeleteMilestone(milestoneId, teamId);

            if (!result)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("DeleteMilestoneFail"));
            }
            else
            {
                //Success
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("DeleteMilestoneSuccess"));
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        /// <summary>
        /// Controlador para alterar o estado de uma milestone para complete
        /// </summary>
        /// <param name="milestoneId">Recebe o id de uma milestone</param>
        /// <param name="teamId">O id da equipa</param>
        /// <returns>Página da equipa</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMilestone(Guid milestoneId, Guid teamId)
        {
            if (milestoneId == null || teamId == null)
            {
                return NotFound();
            }

            var result = await _teamsService.ChangeMilestoneStatus(milestoneId, teamId, MilestoneState.Completed);

            if (!result)
            {
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UpdateMilestoneFail"));
            }
            else
            {
                //Success
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("UpdateMilestoneSuccess"));
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        /// <summary>
        /// Verifica se uma equipa existe.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <returns>False sempre. Nada existe é tudo uma inveção de seres superiores a nós.</returns>
        private bool TeamExists(Guid id)
        {
            //return _context.Team.Any(e => e.TeamId == id);
            return false;
        }

        /// <summary>
        /// Obtem o id do logado atual.
        /// </summary>
        /// <returns>Id do utilizador logado.</returns>
        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Verifica se o utilizador logado atual é membro da equipa.
        /// </summary>
        /// <param name="team">Objeto equipa</param>
        /// <returns>True: se o utilizador é membro. False: Se não é.</returns>
        private bool IsMember(Team team)
        {
            var userId = CurrentUserId();
            return team.Members.Any(m => m.Id == userId.ToString());
        }

        /// <summary>
        /// Verifica se o utilizador atual está pendente de entrar numa equipa.
        /// </summary>
        /// <param name="team">Objeto equipa.</param>
        /// <returns>True: se está pendente. False: se não está pendente.</returns>
        private bool InPendingList(Team team)
        {
            var userId = CurrentUserId();
            return team.PendingList.Any(m => m.UserId == userId.ToString() && m.Status == PendingUserTeamStatus.Pending);
        }

        /// <summary>
        /// Verifica se o utilizador já foi convidado para a equipa.
        /// </summary>
        /// <param name="team">Objeto equipa.</param>
        /// <returns>True: se já foi. False: se não foi.</returns>
        private bool AlreadyInvited(Team team)
        {
            var userId = CurrentUserId();
            return team.InvitedUsers.Any(m => m.UserId == userId.ToString() && m.Status == PendingUserTeamStatus.Pending);
        }

        //Teams/Details/5/GiveOwnership
        /// <summary>
        /// Transfere de proprietário da equipa.
        /// </summary>
        /// <param name="id">Id da equipa.</param>
        /// <param name="userId">Id do utilizador a ser o novo propiretário.</param>
        /// <returns>Página de detalhes da equipa atualizada.</returns>
        [ActionName("GiveOwnership")]
        public async Task<IActionResult> GiveOwnership(Guid id, Guid userId)
        {
            if (id == null || userId == null)
            {
                return NotFound();
            }

            var success = await _teamsService.GiveOwnership(id, userId);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("GiveOwnershipSuccess"));
                var previousOwnerId = CurrentUserId();
                await SendTeamOwnerChangeNotification(id, userId, previousOwnerId);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("GiveOwnershipFail"));
            }

            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Cria uma notificação e chama o gestor de notificações para notificar os interessados.
        /// </summary>
        /// <param name="teamId">Id da equipa para o qual o utilizador aplicou-se.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        private async Task<bool> SendNewTeamRequestNotification(Guid teamId)
        {
            try
            {
                //Get data to generate notification
                Team requestedTeam = await _teamsService.GetTeamById(teamId);
                User ownerUser = await _userService.GetUser(requestedTeam.OwnerId.Value);
                string link = "https://" + HttpContext.Request.Host.Value + "/Teams/Details/" + requestedTeam.TeamId;

                Notification noti = new Notification()
                {
                    Id = Guid.NewGuid(),
                    Text = _commonLocalizationService.GetLocalizedString("NewRequestTeam", ownerUser.Language.Code),
                    URL = link,
                    UserId = ownerUser.Id,
                    User = ownerUser
                };

                var success = await _notificationManager.SendNotification(ownerUser.Id, noti);
                return success;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Cria uma notificação para alertar os membros da equipa que a equipa sofreu uma alteração no estado.
        /// Chama o gestor de notificações para notificar os interessados.
        /// </summary>
        /// <param name="teamId">Id da equipa para o qual o utilizador aplicou-se.</param>
        /// <param name="status">Novo estado da equipa</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        /// <todo>Add localization for `TeamStatusUpdatedTo` - (Team was updated to {0})</todo>
        private async Task<bool> SendTeamStatusChangeNotification(Guid teamId, TeamStatus status)
        {
            try
            {
                //Get data to generate notification
                Team team = await _teamsService.GetTeamById(teamId);
                string link = "https://" + HttpContext.Request.Host.Value + "/Teams/Details/" + teamId;

                foreach(var member in team.Members)
                {
                    Notification noti = new Notification()
                    {
                        Id = Guid.NewGuid(),
                        Text = string.Format(_commonLocalizationService.GetLocalizedString("TeamStatusUpdatedTo", member.Language.Code), _commonLocalizationService.GetLocalizedString(status.ToString(), member.Language.Code)),
                        URL = link,
                        UserId = member.Id,
                        User = member
                    };

                    await _notificationManager.SendNotification(member.Id, noti);
                }

                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Cria uma notificação com a mensagem de pedido de adesão.
        /// </summary>
        /// <param name="UserRequest">Objeto do request do utilizador.</param>
        /// <param name="teamId">Id da equipa para o qual foi enviada a solicitação.</param>
        /// <returns></returns>
        private async Task<bool> SendNewTeamRequestAcceptedNotification(PendingUserTeam UserRequest, Guid teamId)
        {
            try
            {
                //Get data to generate notification
                PendingUserTeam pendingUserTeam = UserRequest;
                User requesterUser = await _userService.GetUser(new Guid(pendingUserTeam.UserId));
                Team requestedTeam = await _teamsService.GetTeamById(teamId);

                string link = "https://" + HttpContext.Request.Host.Value + "/Teams/Details/" + requestedTeam.TeamId;
                // Set the current culture to the specified culture
                // Set current culture to user's preferred language
                
                Notification noti = new Notification()
                {
                    Id = Guid.NewGuid(),
                    Text = _commonLocalizationService.GetLocalizedString("AcceptedRequestTeam", requesterUser.Language.Code),
                    URL = link,
                    UserId = requesterUser.Id,
                    User = requesterUser
                };

                
                var success = await _notificationManager.SendNotification(requesterUser.Id, noti);
                return success;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Cria uma notificação para alertar os membros da equipa que a equipa sofreu uma alteração no estado.
        /// Chama o gestor de notificações para notificar os interessados.
        /// </summary>
        /// <param name="teamId">Id da equipa para o qual o utilizador aplicou-se.</param>
        /// <param name="status">Novo estado da equipa</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        /// <todo>Add localization for `TeamNewOwner` - (Team changed owner to {0}) and for `YouAreTeamNewOwner` - (You are the new owner of {0})</todo>
        private async Task<bool> SendTeamOwnerChangeNotification(Guid teamId, Guid newOwnerId, Guid previousOwnerId)
        {
            try
            {
                //Get data to generate notification
                Team team = await _teamsService.GetTeamById(teamId);
                User previousOwner = await _userService.GetUser(previousOwnerId);
                User newOwner = await _userService.GetUser(newOwnerId);

                if (team == null || newOwner == null || previousOwner == null) return false;

                string link = "https://" + HttpContext.Request.Host.Value + "/Teams/Details/" + teamId;

                var members = team.Members.Where(t => t.Id != previousOwner.Id);

                foreach (var member in members)
                {
                    Notification noti = new Notification()
                    {
                        Id = Guid.NewGuid(),
                        Text = string.Format(_commonLocalizationService.GetLocalizedString("TeamNewOwner", member.Language.Code), HttpUtility.HtmlEncode(newOwner.Name)),
                        URL = link,
                        UserId = member.Id,
                        User = member
                    };

                    await _notificationManager.SendNotification(member.Id, noti);
                }

                Notification notiNewOwner = new Notification()
                {
                    Id = Guid.NewGuid(),
                    Text = string.Format(_commonLocalizationService.GetLocalizedString("YouAreTeamNewOwner", newOwner.Language.Code), HttpUtility.HtmlEncode(team.TeamName)),
                    URL = link,
                    UserId = newOwner.Id,
                    User = newOwner
                };

                await _notificationManager.SendNotification(newOwner.Id, notiNewOwner);

                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Cria uma notificação para notificar um utilizador que recebeu um convite.
        /// Chama o gestor de notificações para notificar os interessados.
        /// </summary>
        /// <param name="teamId">Id da equipa para o qual o utilizador aplicou-se.</param>
        /// <param name="userId">Id do utilizador convidado</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        /// <todo>Add localization for `YouWereInvitedToTeam` - (You were invited to team {0})</todo>
        private async Task<bool> SendInviteToTeamNotification(Guid teamId, Guid userId)
        {
            try
            {
                //Get data to generate notification
                Team team = await _teamsService.GetTeamById(teamId);
                User user = await _userService.GetUser(userId);

                if (team == null || user == null) return false;

                string link = "https://" + HttpContext.Request.Host.Value + "/Teams/Details/" + teamId;

                Notification notiNewOwner = new Notification()
                {
                    Id = Guid.NewGuid(),
                    Text = string.Format(_commonLocalizationService.GetLocalizedString("YouWereInvitedToTeam", user.Language.Code), HttpUtility.HtmlEncode(team.TeamName)),
                    URL = link,
                    UserId = user.Id,
                    User = user
                };

                await _notificationManager.SendNotification(user.Id, notiNewOwner);

                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<IActionResult> GetPendingListViewComponentResult(Guid teamId)
        {
            return ViewComponent("TeamPendingList", new { teamId = teamId });
        }
    }
}

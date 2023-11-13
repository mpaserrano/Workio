using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
﻿using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Workio.Data;
using Workio.Models;
using Workio.Services;
using Workio.Services.Connections;
using Workio.Services.Interfaces;
using System.Linq;
using Workio.Services.Teams;
using Workio.Managers.Notifications;
using NToastNotify;
using Workio.Services.Events;
using Workio.Services.Chat;
using Workio.Models.Chat;
using Workio.ViewModels;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador do user. Serve para dar handle do pedidos relacionados com o utilizador
    /// </summary>
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConnectionService _connectionService;
        private readonly IRatingService _ratingService;
        private readonly ITeamsService _teamsService;
        private readonly IEventsService _eventsService;

        /// <summary>
        /// Serviço do chat
        /// </summary>
        private readonly IChatService _chatService;

        /// <summary>
        /// Gestor de notificações
        /// </summary>
        private readonly INotificationManager _notificationManager;

        /// <summary>
        /// Serviço de mostragem de notificações.
        /// </summary>
        private readonly IToastNotification _toastNotification;
        /// <summary>
        /// Serviço de tradução
        /// </summary>
        private readonly CommonLocalizationService _commonLocalizationService;

        /// <summary>
        /// Constructor da classe.
        /// </summary>
        /// <param name="userManager">Variavel para aceder ao serviço</param>
        /// <param name="userService">Variavel para aceder ao serviço de utilizadores</param>
        /// <param name="connectionService">Variavel para aceder ao serviço de conexões</param>
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IUserService userService, IConnectionService connectionService, IRatingService ratingService, 
            ITeamsService teamsService, INotificationManager notificationManager,
            IToastNotification toastNotification, CommonLocalizationService commonLocalizationService,
            IEventsService eventsService, IChatService chatService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _connectionService = connectionService;
            _ratingService = ratingService;
            _teamsService = teamsService;
            _notificationManager = notificationManager;
            _toastNotification = toastNotification;
            _commonLocalizationService = commonLocalizationService;
            _eventsService = eventsService;
            _chatService = chatService;
        }

        /// <summary>
        /// Ação para aceitar uma conexão de um utilizador
        /// </summary>
        /// <param name="id">Variavel com o id do utilizador correspondente ao pedido de conexão</param>
        /// <returns>Task<IActionResult> - Retorna para a página Index. Caso dê erro retorna para a página User404</returns>
        public async Task<IActionResult> Index(Guid? id)
        {
            try
            {
                ViewData["id"] = id;
               
                var userId = id.HasValue ? id.Value : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var currentUserId = Guid.NewGuid();

                var userConnections = await _connectionService.GetUserConnectionsAsync(userId);

                // Inicializar viewbags
                ViewBag.AlreadyBlocked = false;
                ViewBag.ConnectionsNumber = userConnections.Count();
                ViewData["showMore"] = false;
                ViewBag.IsModEntityAdmin = false;
                ViewBag.IsTeamMember = false;


                if (_signInManager.IsSignedIn(User))
                {
                    currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    ViewBag.IsModEntityAdmin = await _userService.IsCurrentUserModAdminEntity();

                    // Verifica se o utilizador atual está bloqueado pelo utilizador
                    if (currentUserId != userId)
                    {
                        var isBlocked = await _userService.IsBlockedByUser(id.Value);

                        if (isBlocked)
                        {
                            return View("Blocked");
                        }

                        var alreadyBlocked = await _userService.IsAlreadyBlocked(id.Value);

                        ViewBag.AlreadyBlocked = alreadyBlocked;

                        // Verifica se são colegas de equipa para mostrar a opção de review
                        var isTeamMember = await _teamsService.AreTeammates(userId);
                        ViewBag.isTeamMember = isTeamMember;

                        if (!alreadyBlocked)
                        {
                            var existConnection = await _connectionService.GetConnectionBetweenUsers(currentUserId, userId);

                            if (existConnection != null)
                            {
                                ViewBag.State = existConnection.State;
                                ViewBag.IsRequestMine = existConnection.UserId == currentUserId.ToString();
                                ViewBag.ConnectionId = existConnection.Id;
                            }
                        }
                    }
                    else
                    {
                        ViewBag.AlreadyBlocked = false;
                        ViewBag.isTeamMember = false;
                    }
                }

                var user = await _userService.GetUserProfile(userId);
                var skills = await _userService.GetUserSkills(userId);

                var ownProfile = currentUserId == userId;

                // Setup viewbags related to profile
                ViewBag.OwnProfile = ownProfile;
                ViewBag.ConnectionParticipants = false;
                ViewBag.UserGettingRequested = false;
                ViewBag.SkillsShowMore = skills.Count > 5;
                ViewBag.ExperiencesShowMore = user.Experiences.Count > 5;
                
                var myViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    ProfilePicture = user.ProfilePicture,
                    AboutMe = user.AboutMe,
                    GitHubAcc = user.GitHubAcc,
                    LinkedInAcc = user.LinkedInAcc,
                    Skills = skills.Take(5).ToList(),
                    Experiences = user.Experiences.Take(5).ToList(),
                    OpenDMs = user.Preferences.IsDMOpen
                };

                // Setup Review 
                if (!_ratingService.IsRated(userId))
                {
                    ViewBag.AverageRating = 0;
                    ViewBag.RatingsCount = "0";
                }
                else
                {
                    var averageRating = await _ratingService.GetAverageRating(userId);
                    var ratingsCount = await _ratingService.GetNumberOfRatings(userId);

                    ViewBag.AverageRating = averageRating.ToString().Replace(".", ",");
                    ViewBag.RatingsCount = ratingsCount;

                    //Prevent from search own ratings error
                    if (!ownProfile)
                    {
                        var ratedAmount = await _ratingService.GetRatingByRatingUserId(currentUserId, userId);
                        ViewBag.RatedAmount = ratedAmount == null ? -1 : ratedAmount.Rating;
                    }
                    
                }

                return View(myViewModel);
            }
            catch
            {
                return View("User404");
            }
        }

        [Authorize]
        public async Task<IActionResult> SendMessage(Guid otherUserId, string? returnUrl)
        {
            var userId = CurrentUserId();

            var otherUserPreferences = await _userService.GetUserPreferences(otherUserId.ToString());

            var areFriends = await _connectionService.AreFriends(userId, otherUserId);

            if(otherUserPreferences.IsDMOpen == false && areFriends == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("This user has closed DMs"));
                if(returnUrl != null)
                    return LocalRedirect(returnUrl);
                return RedirectToAction("Index", new { id = otherUserId });
            }

            var chat = await _chatService.GetChatRoomBetweenUsers(userId.ToString(), otherUserId.ToString());

            if(chat == null)
            {
                return RedirectToAction("CreateUserChatRoom", "Chat", new
                {
                    otherUserId = otherUserId.ToString(),
                    returnUrl = Url.Action("Index", "User", new { id = otherUserId })
                });
            }

            var canSendMessage = await _chatService.CanSendMessageToChatRoom(userId.ToString(), chat.ChatRoomId);

            if (!canSendMessage)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error Sending Message. This user has closed DMs"));
                if (returnUrl != null)
                    return LocalRedirect(returnUrl);
                return BadRequest();
            }

            if (chat != null) return RedirectToAction("Index", "Chat", new { roomId = chat.ChatRoomId });

            else
            {
                if (returnUrl != null)
                {
                    return RedirectToAction("CreateUserChatRoom", "Chat", new
                    {
                        otherUserId = otherUserId.ToString(),
                        returnUrl = returnUrl
                    });
                }

                return RedirectToAction("CreateUserChatRoom", "Chat", new
                {
                    otherUserId = otherUserId.ToString(),
                    returnUrl = Url.Action("Index", "User", new { id = otherUserId })
                });
            }
        }

        /// <summary>
        /// Ação para adicionar uma habilidade no perfil do utilizador
        /// </summary>
        /// <param name="skillModel">Modelo com os dados da habilidade</param>
        /// <param name="showMore">Se o pedido foi feito pela página do perfil ou da listagem completa</param>
        /// <returns>IActionResult - Redireciona para a página onde estavamos para recarregar os dados</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill([Bind("Name")] SkillModel skillModel, bool showMore)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Error adding skill");
                if (!showMore)
                    return RedirectToAction("Index");
                return RedirectToAction("AllSkills");
            }
            var user_id = CurrentUserId();
            var success = await _userService.AddSkill(CurrentUserId(), skillModel);

            if(success == null)
            {
                _toastNotification.AddErrorToastMessage("Error adding skill");
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("Success adding skill");
            }
            if (!showMore)
                return RedirectToAction("Index");
            return RedirectToAction("AllSkills");
        }
        /// <summary>
        /// Controlador para editar uma skill
        /// </summary>
        /// <param name="id">Recebe o id da skill</param>
        /// <param name="skill">Os dados da skill atualizados</param>
        /// <param name="showMore">Se estavamos na página de mostrar tudo</param>
        /// <returns>Página das skills</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(Guid id, SkillModel skill, bool showMore)
        {
            skill.SkillId = id;
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Error editing skill");
                return RedirectToAction("AllSkills");
            } 
                
            var result = await _userService.EditSkill(skill);

            if (!result)
            {
                _toastNotification.AddErrorToastMessage("Error editing skill");
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("Success editing skill");
            }

            return RedirectToAction("AllSkills");
        }
        /// <summary>
        /// Controlador para eliminar uma skill
        /// </summary>
        /// <param name="id">Recebe o id da skill</param>
        /// <returns>Página das skills</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            var result = await _userService.DeleteSkill(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage("Error deleting skill");
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("Success deleting skill");
            }

            return RedirectToAction("AllSkills");
        }

        /// <summary>
        /// Ação para listar todas as habilidades
        /// </summary>
        /// <param name="id">Id do utilizador que queremos procurar as habilidades</param>
        /// <returns>IActionResult - Nova página</returns>
        public async Task<IActionResult> AllSkills(Guid? id)
        {
            try
            {
                var userId = Guid.NewGuid();
                if (!_signInManager.IsSignedIn(User) && !id.HasValue)
                {
                    return NotFound();
                }
                else if (!_signInManager.IsSignedIn(User) && id.HasValue)
                {
                    userId = id.Value;
                }
                else
                    userId = id.HasValue ? id.Value : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var skills = await _userService.GetUserSkills(userId);
                var user = await _userService.GetUser(userId);

                var ownProfile = IsOwnProfile(userId);

                ViewData["id"] = id;
                ViewBag.Name = user.Name;
                ViewBag.Skills = skills;
                ViewBag.OwnProfile = ownProfile;
                ViewBag.SkillsCount = skills.Count;
                ViewData["showMore"] = true;
                ViewBag.EditMode = TempData["EditMode"] != null;

                return View();
            }
            catch
            {
                return View("User404");
            }
        }

        /// <summary>
        /// Ação para adicionar uma experiência no perfil do utilizador
        /// </summary>
        /// <param name="experienceModel">Modelo com os dados da experiência</param>
        /// <param name="showMore">Se o pedido foi feito pela página do perfil ou da listagem completa</param>
        /// <returns>IActionResult - Redireciona para a página onde estavamos para recarregar os dados</returns>
        /// <todo>Mudar para a utilização de toasts</todo>
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience([Bind("Company,Description,EndDate,StartDate,WorkTitle")] ExperienceModel experienceModel, bool showMore)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Couldn't create experience"));
                if (!showMore)
                    return RedirectToAction("Index");
                return RedirectToAction("AllExperiences");
            }

            var success = await _userService.AddExperience(experienceModel);
            if(success == null)
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error adding experience"));
            else
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Success adding experience"));

            if (!showMore)
                return RedirectToAction("Index");
            return RedirectToAction("AllExperiences");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExperience(Guid id, [Bind("Company,Description,EndDate,StartDate,WorkTitle")] ExperienceModel experienceModel, bool showMore)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Couldn't edit experience"));
                return RedirectToAction("AllExperiences");
            }

            experienceModel.UserId = CurrentUserId().ToString();
            experienceModel.ExperienceId = id;
            var result = await _userService.EditExperience(experienceModel);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Couldn't edit experience"));
            }
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Success editing experience"));
            return RedirectToAction("AllExperiences");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(Guid id)
        {
            var result = await _userService.DeleteExperience(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Couldn't delete experience"));
            }
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Success deleting experience"));
            return RedirectToAction("AllExperiences");
        }

        /// <summary>
        /// Ação para listar todas as experiências
        /// </summary>
        /// <param name="id">Id do utilizador que queremos procurar as experiências</param>
        /// <returns>IActionResult - Nova página</returns>
        public async Task<IActionResult> AllExperiences(Guid? id)
        {
            try
            {
                var userId = id.HasValue ? id.Value : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var experiences = await _userService.GetUserExperience(userId);
                var user = await _userService.GetUserProfile(userId);

                var ownProfile = IsOwnProfile(userId);

                ViewData["id"] = id;
                ViewBag.Name = user.Name;
                ViewBag.Experiences = experiences;
                ViewBag.OwnProfile = ownProfile;
                ViewBag.ExperiencesCount = experiences.Count;
                ViewData["showMore"] = true;
                ViewBag.EditMode = TempData["EditMode"] != null;

                return View();
            }
            catch
            {
                return View("User404");
            }
        }

        public IActionResult EditModeSkill()
        {
            TempData["EditMode"] = true;
            return RedirectToAction("AllSkills");
        }

        public IActionResult EditModeExperiences()
        {
            TempData["EditMode"] = true;
            return RedirectToAction("AllExperiences");
        }

        public IActionResult OpenEditExperience(ExperienceModel experienceModel)
        {
            return PartialView("Components/ExperienceModalViewComponent", experienceModel);
        }

        public async Task<IActionResult> AddEndorsement(Guid sk, Guid id, string returnUrl)
        {
            var result = await _userService.AddEndorsement(sk);
            if (result == null)
            {
                Console.WriteLine("---------------------Error----------------!");
            }
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> RemoveEndorsement(Guid skillId, Guid id, string returnUrl)
        {
            var result = await _userService.RemoveEndorsement(skillId);
            if (result == null)
            {
                Console.WriteLine("!---------------------Error----------------!");
            }
            return LocalRedirect(returnUrl);
        }

        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Ação para listar as conexões de um utilizador
        /// </summary>
        /// <returns>Task<IActionResult> - Redireciona para a página de listagem de conexões</returns>
        [Authorize]
        public async Task<IActionResult> Connections(Guid? id)
        {
            var userId = id.HasValue ? id.Value : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ownProfile = IsOwnProfile(userId);

            if (!ownProfile)
            {
                var isBlocked = await _userService.IsBlockedByUser(id.Value);
                if (isBlocked)
                {
                    return View("Blocked");
                }
            }
            else
            {
                // Get all pendings connections
                var pendings = await _connectionService.GetUserPendingConnectionsAsync();
                ViewBag.Pending = pendings != null ? pendings.Where(c => c.RequestedUserId == userId.ToString()).Select(c => c.RequestOwner).ToList() : new List<User>();

                // Get blocked users
                var blocks = await _connectionService.GetBlockedUsersAsync(userId.ToString());
                var blockedUsers = blocks != null ? blocks : new List<User>();
                ViewBag.Blocks = blockedUsers;
            }

            // Get All accepted connections
            var connections = await _connectionService.GetUserConnectionsAsync(userId);
            ViewBag.Connections = connections != null ? connections.Select(c => c.UserId == userId.ToString() ? c.RequestedUser : c.RequestOwner) : new List<User>();
            ViewBag.OwnProfile = ownProfile;
            return View();
        }

        /// <summary>
        /// Ação para aceitar uma conexão de um utilizador
        /// </summary>
        /// <param name="id">Variavel com o id do utilizador correspondente ao pedido de conexão</param>
        /// <param name="profile">Variavel bool que quando verdadeira representa que se está no perfil de um utilizador</param>
        /// <returns>Task<IActionResult> - Retorna um redirecionamento para a ação Index caso a variavel profile seja verdadeira, caso seja falsa, retorna
        /// um redirecionamento para a ação PendingConnections</returns>
        [Authorize]
        public async Task<ActionResult> AcceptConnection(Guid id, bool profile, string returnUrl)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() == currentUserId)
            {
                return Redirect("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            }

            Connection con = new Connection()
            {
                UserId = currentUserId,
                RequestedUserId = id.ToString(),
                ConnectionDate = DateTime.UtcNow,
                State = ConnectionState.Accepted
            };

            var result = await _connectionService.UpdateConnection(con);

            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Request Accepted"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error accepting request. Request may be invalid"));
            }

            return LocalRedirect(returnUrl);
        }

        public async Task<ActionResult> RejectConnection(Guid id, string returnUrl)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() == currentUserId)
            {
                return Redirect("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            }

            Connection connection = new Connection()
            {
                UserId = currentUserId,
                RequestedUserId = id.ToString(),
            };

            var result = await _connectionService.RemoveConnection(connection);

            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Rejected request"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error while rejecting request. Request may be invalid."));
            }

            return LocalRedirect(returnUrl);
        }

        public async Task<ActionResult> CancelConnectionRequest(Guid id, string returnUrl)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() == currentUserId)
            {
                return Redirect("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            }

            Connection connection = new Connection()
            {
                UserId = currentUserId,
                RequestedUserId = id.ToString(),
            };

            var result = await _connectionService.RemoveConnection(connection);

            if (result)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Request cancelled"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error while cancelling request. Request may be invalid."));
            }

            return LocalRedirect(returnUrl);
        }

        /// <summary>
        /// Ação para enviar um pedido de conexão para um utilizador
        /// </summary>
        /// <param name="id">Variavel com o id do utilizador correspondente ao pedido de conexão</param>
        /// <returns>Task<IActionResult> - Retorna um redirecionamento para a ação Index</returns>
        [Authorize]
        public async Task<ActionResult> AddConnection(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(id.ToString() == currentUserId)
            {
                return Redirect("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            }

            var isBlocked = await _userService.IsBlockedByUser(id);

            if (isBlocked)
            {
                return View("Blocked");
            }

            var alreadyBlocked = await _userService.IsAlreadyBlocked(id);

            if(alreadyBlocked)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("You can't add someone that is blocked"));
                return RedirectToAction("Index", new { id = id });
            }

            Connection connection = new Connection() { ConnectionDate = DateTime.Now, State = ConnectionState.Pending, UserId = currentUserId, RequestedUserId = id.ToString()};
            var success = await _connectionService.AddConnection(connection);
            if (success)
            {
                await _notificationManager.SendNewConnectionNotification(id.ToString(), "https://" + HttpContext.Request.Host.Value);
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("RequestToConnectSent"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("FailedSendingRequest"));
            }

            return RedirectToAction("Index", new { id = id });
        }

        /// <summary>
        /// Ação para remover uma conexão de um utilizador
        /// </summary>
        /// <param name="id">Variavel com o id do utilizador correspondente ao pedido de conexão</param>
        /// <param name="profile">Variavel bool que quando verdadeira representa que se está no perfil de um utilizador</param>
        /// <returns>Task<IActionResult> - Retorna um redirecionamento para a ação Index caso a variavel profile seja verdadeira, caso seja falsa e
        /// a conexão , retorna
        /// um redirecionamento para a ação PendingConnections</returns>
        public async Task<ActionResult> RemoveConnection(Guid id, bool profile, string returnUrl)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() == currentUserId)
            {
                return Redirect("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            }

            Connection connection = new Connection()
            {
                UserId = currentUserId,
                RequestedUserId = id.ToString(),
            };


            var result = await _connectionService.RemoveConnection(connection);

            if(result)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Connection Removed"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("Error while removing connection. Connection may be invalid."));
            }

            return LocalRedirect(returnUrl);
        }
        /// <summary>
        /// Verifica se o perfil que o utilizador está a visitar é o seu
        /// </summary>
        /// <param name="id">Id do perfil que está a ser visitado</param>
        /// <returns>true se é o seu, false se não é</returns>
        private bool IsOwnProfile(Guid id)
        {
            if (!_signInManager.IsSignedIn(User)) return false;
            var currentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return id == currentId;
        }
    }
}


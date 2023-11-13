using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Models.Filters;
using Workio.Services;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.LocalizationServices;
using Workio.Services.Matchmaking;

namespace Workio.Controllers.Events
{
    /// <summary>
    /// Para receber, processar e responder pedidos relativamente á manipulação dos dados dos eventos
    /// </summary>
    public class EventsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private SignInManager<User> _signInManager;
        private UserManager<User> _userManager;
        private IEventsService _eventsService;
        private readonly IToastNotification _toastNotification;
        private readonly ILocalizationService _localizationService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IUserService _userService;
        private readonly CommonLocalizationService _commonLocalizationService;

        public EventsController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IEventsService eventsService,
            IToastNotification toastNotification,
            ILocalizationService localizationService,
            IMatchmakingService matchmakingService,
            IWebHostEnvironment webHostEnvironment,
            IUserService userService,
            CommonLocalizationService commonLocalizationService
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _eventsService = eventsService;
            _toastNotification = toastNotification;
            _localizationService = localizationService;
            _matchmakingService = matchmakingService;
            _webHostEnvironment = webHostEnvironment;
            _userService = userService;
            _commonLocalizationService = commonLocalizationService;
        }

        /// <summary>
        /// Valida se o user tem o loggin efectuado.
        /// </summary>
        /// <returns>true: Se o utilizador tiver login feito.\nfalse: Se o utilizador não tiver login feito.</returns>
        private bool IsUserLogged()
        {
            if (_signInManager.IsSignedIn(User)) return true;
            else return false;
        }

        /// <summary>
        /// Verifica se um fiicheiro é uma imagem válida.
        /// </summary>
        /// <param name="file">Ficheiro a validar.</param>
        /// <returns>True se for válida. False se não for um formato válido.</returns>
        private bool IsImage(IFormFile file)
        {
            if (!string.Equals(file.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cria uma select list do enum do estado dos eventos
        /// </summary>
        /// <returns>Select list para o html</returns>
        private SelectList GetEnumEventState()
        {
            var values = from EventState e in Enum.GetValues(typeof(EventState))
                         select new { Id = (int)e, Name = e.ToString() };
            SelectList sl = new SelectList(values, "Id", "Name", values);

            return sl;
        }

        // GET: Events
        /// <summary>
        /// Retorna a página principal dos eventos.
        /// </summary>
        /// <param name="openFilter">Filtro de eventos abertos.</param>
        /// <param name="onGoingFilter">Filtro de eventos a decorrer.</param>
        /// <param name="finishedFilter">Filtro de eventos terminados.</param>
        /// <returns>Página principal dos eventos.</returns>
        public async Task<IActionResult> Index(int[] selectedFilters)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = (await _userManager.GetUserAsync(User)).Id;
            var user = await _userService.GetUser(Guid.Parse(userId));
            var filters = new EventsFilterViewModel();
            ViewBag.Filters = filters;

            //Load all needed lists
            var events = new List<Event>();
            var createdEvents = new List<Event>();
            var joinedEvents = new List<Event>();
            List<Event> recommendedEvents = new List<Event>();

            events = (await _eventsService.GetEvents()).ToList();
            createdEvents = GetCreatedEvents(user, events).ToList();
            joinedEvents = GetJoinedEvents(user, events).ToList();
            recommendedEvents = await _matchmakingService.GetRecommendedEvents();

            events = events.Where(e => !user.BlockedUsers.Any(u => u.BlockedUserId == e.UserId) && (e.UserPublisher != null && !e.UserPublisher.BlockedUsers.Any(u => u.BlockedUserId == userId))).ToList();

            if (recommendedEvents.Count > 0)
            {
                recommendedEvents.Reverse();
            }

            if (selectedFilters != null && selectedFilters.Length > 0 && filters.filters != null && filters.filters.Count > 0)
            {
                filters.selectedFilters = selectedFilters;
                ViewBag.Filters = filters;
                var filteredAllEvents = new List<Event>();
                var filteredCreatedEvents = new List<Event>();
                var filteredJoinedEvents = new List<Event>();
                var filteredRecomendedEvents = new List<Event>();

                foreach (var index in selectedFilters)
                {
                    var filter = filters.filters.FirstOrDefault(f => f.Index == index);
                    if (filter != null)
                    {
                        //Filter All Events
                        var _fAll = events.Where(filter.Condition).ToList();
                        foreach (Event t in _fAll)
                        {
                            if (!filteredAllEvents.Contains(t))
                            {
                                filteredAllEvents.Add(t);
                            }
                        }

                        //Filter created events
                        var _fCreated = createdEvents.Where(filter.Condition).ToList();
                        foreach (Event t in _fCreated)
                        {
                            if (!filteredCreatedEvents.Contains(t))
                            {
                                filteredCreatedEvents.Add(t);
                            }
                        }

                        //Filter joined events
                        var _fJoined = joinedEvents.Where(filter.Condition).ToList();
                        foreach (Event t in _fAll)
                        {
                            if (!filteredJoinedEvents.Contains(t))
                            {
                                filteredJoinedEvents.Add(t);
                            }
                        }

                        //Filter recommended Events
                        var _fRecommended = recommendedEvents.Where(filter.Condition).ToList();
                        foreach (Event t in _fAll)
                        {
                            if (!filteredRecomendedEvents.Contains(t))
                            {
                                filteredRecomendedEvents.Add(t);
                            }
                        }
                    }
                }

                events = filteredAllEvents;
                createdEvents = filteredCreatedEvents;
                joinedEvents = filteredJoinedEvents;
                recommendedEvents = filteredRecomendedEvents;
            }

            //Load viewbags
            ViewBag.AllEvents = events.Where(e => e.IsBanned == false);
            ViewBag.CreatedEvents = createdEvents;
            ViewBag.JoinedEvents = joinedEvents.Where(e => e.IsBanned == false);
            ViewBag.RecommendedEvents = recommendedEvents.Where(e => e.IsBanned == false);

            return View();
        }

        // GET: Events/Details/5
        /// <summary>
        /// Processa a página de detalhes de um evento.
        /// </summary>
        /// <param name="id">Id do eventos a obter os detalhes.</param>
        /// <returns>Página de detalhes de um eventos.</returns>
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var @event = await _eventsService.GetEvent(id);

            if (@event == null)
            {
                return NotFound();
            }

            string currentUserId = User.Identity.IsAuthenticated ? CurrentUserId().ToString() : "";

            if (@event.IsBanned && @event.UserId != currentUserId && !(User.IsInRole("Admin") || User.IsInRole("Mod")))
            {
                return View("Banned");
            }

            ViewData["id"] = id;
            if (!@event.IsInPerson)
            {
                @event.LatitudeText = "null";
                @event.LongitudeText = "null";
            }
            else
            {
                @event.LatitudeText = @event.Latitude.ToString().Replace(",", ".");
                @event.LongitudeText = @event.Longitude.ToString().Replace(",", ".");
            }
            if (@event.State == EventState.Finish)
            {
                ViewBag.EventIsFinished = true;
            }
            else
            {
                ViewBag.EventIsFinished = false;
            }

            if (@event != null)
            {
                if (IsUserLogged())
                {
                    ViewBag.hasUpvote = false;
                    ViewBag.hasDownvote = false;
                    ViewBag.isCreator = false;

                    var userId = CurrentUserId().ToString();

                    var isBlocked = await _userService.IsBlockedByUser(Guid.Parse(@event.UserId));

                    if (isBlocked && @event.UserId != currentUserId && !(User.IsInRole("Admin") || User.IsInRole("Mod")))
                    {
                        return View("Blocked");
                    }

                    ViewBag.OwnsInterestedTeam = @event.InterestedTeams.Where(t => t.Team.OwnerId.ToString() == userId).FirstOrDefault() != null ? true : false;//await _eventsService.IsTeamInterested(@event.EventId);

                    if (@event.UserId == userId) ViewBag.isCreator = true;
                    else
                    {
                        var reaction = @event.EventReactions.Where(r => r.UserId.ToString() == userId).FirstOrDefault();
                        if (reaction != null)
                        {
                            if (reaction.ReactionType == EventReactionType.UpVote) ViewBag.hasUpvote = true;
                            else if (reaction.ReactionType == EventReactionType.DownVote) ViewBag.hasDownvote = true;
                        }
                    }

                    if (@event.InterestedTeams.Where(t => t.Team.Members.Any(it => it.Id == userId)).Count() != 0)
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
            }

            return View(@event);
        }

        // GET: Events/Create
        /// <summary>
        /// Obtem a página de criação de um eventos.
        /// </summary>
        /// <returns>Página de criação de um evento.</returns>
        [Authorize(Roles = "Admin,Entity, Mod")]
        public async Task<IActionResult> Create()
        {
            if (!IsUserLogged())
                return RedirectToAction("Index");

            ViewBag.Languages = await _localizationService.GetLocalizations();
            ViewBag.EventStates = GetEnumEventState();
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Cria um eventos.
        /// </summary>
        /// <param name="event">Evento a ser criado.</param>
        /// <returns>Página de detalhes com o novo evento criado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Entity, Mod")]
        public async Task<IActionResult> Create([Bind("BannerPicturePathFile,Title,Description,Tags,StartDate,EndDate,State,Url,IsFeatured,IsInPerson,Address,LatitudeText,LongitudeText")] Event @event)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index");
            Console.WriteLine("Tags: " + @event.Tags);

            if (!ModelState.IsValid)
            {
                ViewBag.Languages = await _localizationService.GetLocalizations();
                ViewBag.EventStates = GetEnumEventState();
                //Error invalid date
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventCreateFail"));
                return View(@event);
            }

            @event.EventId = Guid.NewGuid();

            if (@event.BannerPicturePathFile != null)
            {
                if (!IsImage(@event.BannerPicturePathFile))
                {
                    ViewBag.Languages = await _localizationService.GetLocalizations();
                    ViewBag.EventStates = GetEnumEventState();
                    //Error file not supported
                    _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventCreateFail"));
                    return View(@event);
                }

                string filename = @event.EventId.ToString().Replace("-", "") + Path.GetExtension(@event.BannerPicturePathFile.FileName);
                var filePath = _webHostEnvironment.WebRootPath + @"\public\uploads\events\banners\" + filename;
                Console.WriteLine($"Path of the image: {filePath.ToLower()}");
                var stream = new FileStream(filePath, FileMode.Create);
                await @event.BannerPicturePathFile.CopyToAsync(stream);
                stream.Close();
                @event.BannerPicturePath = filename;
            }
            else
            {
                @event.BannerPicturePath = "default.png";
            }

            if (@event.Tags != null && @event.Tags != "")
            {
                string[] tagsArray = @event.Tags.Split(',');
                foreach (string tag in tagsArray)
                {
                    var t = new EventTag();
                    t.EventTagId = Guid.NewGuid();
                    t.EventTagName = tag.Trim();
                    @event.EventTags.Add(t);
                }
            }

            if (@event.Url == null)
            {
                @event.Url = "";
            }

            if (!@event.IsInPerson)
            {
                @event.Latitude = null;
                @event.Longitude = null;
            }
            else
            {
                double parsedLng = 0;
                double parsedLat = 0;
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
                @event.LongitudeText = @event.LongitudeText.Replace(",", ".");
                @event.LatitudeText = @event.LatitudeText.Replace(",", ".");
                bool parsedBooleanLng = double.TryParse(@event.LongitudeText, NumberStyles.Float, culture, out parsedLng);
                bool parsedBooleanLat = double.TryParse(@event.LatitudeText, NumberStyles.Float, culture, out parsedLat);
                if (parsedBooleanLng && parsedBooleanLat)
                {
                    @event.Latitude = parsedLat;
                    @event.Longitude = parsedLng;
                }
            }

            var success = await _eventsService.CreateEvent(@event);

            if (!success)
            {
                ViewBag.Languages = await _localizationService.GetLocalizations();
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventCreateFail"));
                return View(@event);
            }
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("EventCreated"));

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Obtem a página de edição de um evento.
        /// </summary>
        /// <param name="id">Id do evento a obter a página para editar.</param>
        /// <returns>Página para editar o evento.</returns>
        [Authorize(Roles = "Admin,Entity, Mod")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var @event = await _eventsService.GetEvent(id);
            Console.WriteLine("Event:" + @event.EventId);
            if (@event == null)
            {
                return NotFound();
            }

            if (@event.IsBanned)
            {
                return View("Banned");
            }
            if (@event.EventTags != null && @event.EventTags.Count > 0)
                @event.Tags = string.Join(",", @event.EventTags.Select(t => t.EventTagName));

            if (!@event.IsInPerson)
            {
                @event.LatitudeText = "null";
                @event.LongitudeText = "null";
            }
            else
            {
                @event.LatitudeText = @event.Latitude.ToString().Replace(",", ".");
                @event.LongitudeText = @event.Longitude.ToString().Replace(",", ".");
            }

            @event.IsInPerson = !@event.IsInPerson;
            ViewBag.Languages = await _localizationService.GetLocalizations();
            ViewBag.EventStates = GetEnumEventState();
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Altera os dados de um evento.
        /// </summary>
        /// <param name="id">Id do evento a alterar.</param>
        /// <param name="event">Dados do evento para alterar.</param>
        /// <returns>Página de detalhes do evento com as alterações.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Entity, Mod")]
        public async Task<IActionResult> Edit(Guid id, [Bind("EventId,BannerPicturePathFile,Title,Description,Tags,StartDate,EndDate,State,Url,IsFeatured,IsInPerson,Address,LatitudeText,LongitudeText")] Event @event)
        {
            //@event = await _eventsService.GetEvent(id);
            @event.EventId = id;
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (@event.BannerPicturePathFile != null)
            {
                if (!IsImage(@event.BannerPicturePathFile))
                {
                    ViewBag.Languages = await _localizationService.GetLocalizations();
                    ViewBag.EventStates = GetEnumEventState();
                    //Error file not supported
                    _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventUpdateFail"));
                    return View(@event);
                }

                string filename = @event.EventId.ToString().Replace("-", "") + Path.GetExtension(@event.BannerPicturePathFile.FileName);
                var filePath = _webHostEnvironment.WebRootPath + @"\public\uploads\events\banners\" + filename;
                Console.WriteLine($"Path of the image: {filePath.ToLower()}");
                var stream = new FileStream(filePath, FileMode.Create);
                await @event.BannerPicturePathFile.CopyToAsync(stream);
                stream.Close();
                @event.BannerPicturePath = filename;
            }
            // Split the tags string into an array of tag names
            var tagNames = @event.Tags?.Split(',');
            // Create a new list of tags from the tag names
            var tags = tagNames?.Select(name => new EventTag { EventTagName = name.Trim() }).ToList();
            // Set the Skills property of the team object to the new list of tags
            @event.EventTags = tags;

            if (@event.Url == null)
            {
                @event.Url = "";
            }

            if (!@event.IsInPerson)
            {
                @event.Latitude = null;
                @event.Longitude = null;
            }
            else
            {
                double parsedLng = 0;
                double parsedLat = 0;
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
                @event.LongitudeText = @event.LongitudeText.Replace(",", ".");
                @event.LatitudeText = @event.LatitudeText.Replace(",", ".");
                bool parsedBooleanLng = double.TryParse(@event.LongitudeText, NumberStyles.Float, culture, out parsedLng);
                bool parsedBooleanLat = double.TryParse(@event.LatitudeText, NumberStyles.Float, culture, out parsedLat);
                if (parsedBooleanLng && parsedBooleanLat)
                {
                    @event.Latitude = parsedLat;
                    @event.Longitude = parsedLng;
                }
            }

            var success = await _eventsService.UpdateEvent(@event);

            if (!success)
            {
                ViewBag.Languages = await _localizationService.GetLocalizations();
                //Error
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventUpdateFail"));
                return View(@event);
            };
            Console.WriteLine(success);
            _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("EventUpdate"));
            return RedirectToAction(nameof(Details), new { id = id });

        }

        // GET: Events/Delete/5
        /// <summary>
        /// Página de confirmação para apagar um evento.
        /// </summary>
        /// <param name="id">Id do evento a apagar.</param>
        /// <returns>Página para apagar o evento.</returns>
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == null || await _eventsService.GetEvent(id) == null)
            {
                return NotFound();
            }

            var @event = await _eventsService.GetEvent(id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        /// <summary>
        /// Apaga um evento.
        /// </summary>
        /// <param name="id">Id do evento a apagar.</param>
        /// <returns>Página principal de eventos.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (await _eventsService.GetEvent(id) == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Event'  is null.");
            }
            var @event = await _eventsService.GetEvent(id);
            if (@event != null)
            {
                _eventsService.RemoveEvent(@event);
            }
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Dá um voto a um evento.
        /// </summary>
        /// <param name="id">Id do evento a dar upvote.</param>
        /// <param name="returnUrl">Url para o qual rederecionar.</param>
        /// <returns>Página referenciada no URL.</returns>
        public async Task<IActionResult> Upvote(Guid id, string returnUrl)
        {
            var result = await _eventsService.UpVote(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("UpvoteFail"));
                return LocalRedirect(returnUrl);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Upvoted"));
                return LocalRedirect(returnUrl);
            }

        }

        /// <summary>
        /// Dá um voto negativo a um evento.
        /// </summary>
        /// <param name="id">Id do evento a dar downvote.</param>
        /// <param name="returnUrl">Url para o qual rederecionar.</param>
        /// <returns>Página referenciada no URL.</returns>
        public async Task<IActionResult> Downvote(Guid id, string returnUrl)
        {
            var result = await _eventsService.Downvote(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("DownvoteFail"));
                return LocalRedirect(returnUrl);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("Downvoted"));
                return LocalRedirect(returnUrl);
            }

        }

        /// <summary>
        /// Remove um voto de um evento.
        /// </summary>
        /// <param name="id">Id do evento.</param>
        /// <param name="returnUrl">Url para o qual rederecionar.</param>
        /// <returns>Página referenciada no URL.</returns>
        public async Task<IActionResult> RemoveUpvote(Guid id, string returnUrl)
        {
            var result = await _eventsService.RemoveUpvote(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("RemoveUpvoteFail"));
                return LocalRedirect(returnUrl);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("RemoveUpvote"));
                return LocalRedirect(returnUrl);
            }
        }

        /// <summary>
        /// Remove um voto negativo de um evento.
        /// </summary>
        /// <param name="id">Id do evento.</param>
        /// <param name="returnUrl">Url para o qual rederecionar.</param>
        /// <returns>Página referenciada no URL.</returns>
        public async Task<IActionResult> RemoveDownvote(Guid id, string returnUrl)
        {
            var result = await _eventsService.RemoveDownvote(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("RemoveDownvoteFail"));
                return LocalRedirect(returnUrl);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("RemoveDownvote"));
                return LocalRedirect(returnUrl);
            }
        }

        /// <summary>
        /// Verifica se um evento existe.
        /// </summary>
        /// <param name="id">Id do evento a verificar.</param>
        /// <returns>True se existe. False se não existe.</returns>
        private bool EventExists(Guid id)
        {
          return _eventsService.GetEvent(id) != null;
        }

        /// <summary>
        /// Obtem o id do utilizado atual logado.
        /// </summary>
        /// <returns>Id do utilizador atual.</returns>
        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Verifica se um evento já terminou.
        /// </summary>
        /// <param name="id">Id do evento.</param>
        /// <returns>True se o evento está terminado. False caso contrário.</returns>
        private async Task<bool> isFinished(Guid id)
        {
            var @event = await _eventsService.GetEvent(id);
            if(@event.State == EventState.Finish)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verifica se um utilizador está interessado em um evento.
        /// </summary>
        /// <param name="id">Id do evento.</param>
        /// <returns>True se tiver. False caso contrário.</returns>
        private async Task<bool> isInterested(Guid id)
        {
            return await _eventsService.IsUserInterested(id);
        }

        /// <summary>
        /// Termina um evento.
        /// </summary>
        /// <param name="id">Id do evento a terminar.</param>
        /// <returns>Página principal de eventos.</returns>
        [ActionName("FinishMyEvent")]
        public async Task<IActionResult> FinishMyEvent(Guid id)
        {

            bool success = await _eventsService.ChangeEventStatus(id, EventState.Finish);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(_commonLocalizationService.Get("EventEnded"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_commonLocalizationService.Get("EventUpdateStatusFail"));
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Atualiza um filtro da listagem de eventos.
        /// </summary>
        /// <param name="filterOpen">Filtro de abertos.</param>
        /// <param name="filterOnGoing">Filtro de a decorrer.</param>
        /// <param name="filterFinished">Filtro de terminados.</param>
        /// <returns>Listagem de eventos com os filtros aplicados.</returns>
        [ActionName("UpdateFilter")]
        public IActionResult UpdateFilter(bool filterOpen, bool filterOnGoing, bool filterFinished)
        {
            bool FilterOpen = false;
            bool FilterOnGoing = false;
            bool FilterFinished = false;

            if (filterOpen) { FilterOpen = true; }
            if (filterOnGoing) { FilterOnGoing = true; }
            if (filterFinished) { FilterFinished = true; }
            return RedirectToAction("Index", new { openFilter = FilterOpen, onGoingFilter = FilterOnGoing, finishedFilter = FilterFinished });
        }

        /// <summary>
        /// Retorna um JSON com os eventos em que a data de inicio está entre duas datas.
        /// </summary>
        /// <param name="minDate">Data de inicío do evento do inicío do intrevalo de procura.</param>
        /// <param name="maxDate">Data de inicío do evento de fim do intrevalo de procura.</param>
        /// <returns>JSON com os eventos em que a data de inicío está no intrevalo das dastas passadas.</returns>
        public async Task<IActionResult> GetEventsInRangeStartDate(DateTime minDate, DateTime maxDate)
        {
            var data = await _eventsService.GetEventsBetweenDates(minDate, maxDate);

            var response = new
            {
                data = data
            };

            return Json(response);
        }

        private ICollection<Event> GetJoinedEvents(User user, ICollection<Event> allEvents)
        {
            return allEvents.Where(e => e.InterestedUsers != null && (e.InterestedUsers.Any(u => u.User == user) || e.InterestedTeams.Any(t => t.Team.OwnerId.ToString() == user.Id || t.Team.Members.Any(m => m.Id == user.Id)))).ToList();
        }

        private ICollection<Event> GetCreatedEvents(User user, ICollection<Event> allEvents)
        {
            return allEvents.Where(e => e.UserId == user.Id).ToList();
        }
    }
}

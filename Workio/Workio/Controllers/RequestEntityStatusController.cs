using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using NToastNotify;
using System.IO;
using Workio.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using Workio.Services.RequestEntityStatusServices;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Workio.Services;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle dos pedidos para um utilizador ser uma entidade certificada.
    /// </summary>
    public class RequestEntityStatusController : Controller
    {
        private readonly IWebHostEnvironment _webhostEnvironemnt;
        private readonly IToastNotification _toastNotification;
        private readonly IUserService _userService;
        private readonly IRequestEntityStatusService _requestEntityStatusService;
        private readonly SignInManager<User> _signInManager;
        private readonly CommonLocalizationService _localizationService;

        public RequestEntityStatusController(IWebHostEnvironment webHostEnvironment, IToastNotification toastNotification, IUserService userService, IRequestEntityStatusService requestEntityStatusService, SignInManager<User> signInManager, CommonLocalizationService localizationService)
        {
            _webhostEnvironemnt = webHostEnvironment;
            _toastNotification = toastNotification;
            _userService = userService;
            _requestEntityStatusService = requestEntityStatusService;
            _signInManager = signInManager;
            _localizationService = localizationService;
        }
        /// <summary>
        /// Ação que mostra os detalhes do pedido
        /// </summary>
        /// <param name="id">Id do pedido/param>
        /// <returns>IActionResult - Nova página</returns>

        // GET: RequestEntityStatus/Details/5
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null || await _requestEntityStatusService.GetRequestById(id) == null)
            {
                return NotFound();
            }

            var requestEntityStatus = await _requestEntityStatusService.GetRequestById(id);
            if (requestEntityStatus == null)
            {
                return NotFound();
            }

            return View(requestEntityStatus);
        }

        /// <summary>
        /// Ação que que cria pedido
        /// </summary>
        /// <returns>IActionResult - Nova página</returns
        public async Task<IActionResult> Create()
        {
            var currentUser = CurrentUserId();

            if (currentUser == Guid.Empty) return RedirectToAction("Index", "Home");

            var alreadyRequested = _requestEntityStatusService.AlreadyRequested(currentUser);
            ViewBag.AlreadyRequested = alreadyRequested;
            var isCurrentUserModAdminEntity = await _userService.IsCurrentUserModAdminEntity();
            if (isCurrentUserModAdminEntity)
            {
                return View("NotAllowedAccess");
            }
            if (alreadyRequested)
            {
                ViewBag.UserInfo = await _requestEntityStatusService.GetUserInfo(currentUser);
                ViewBag.RequestState = await _requestEntityStatusService.GetRequestStateByUserId(currentUser);
                ViewData["id"] = await _requestEntityStatusService.GetRequestId(currentUser);
            }
            return View();
        }

        /// <summary>
        /// Ação para criar um pedido para ser uma entidade certificada
        /// </summary>
        /// <param name="id">Id do utilizador que faz pedido</param>
        /// <param name="fil">Ficheiro submetido pelo utilizador</param>
        /// <param name="motivation">Razão pela qual efetua o pedido</param>
        /// <returns>IActionResult - Nova página</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? id, IFormFile FilePath, string motivation, RequestEntityStatus request)
        {
            var userId = id.HasValue ? id.Value : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string extension = Path.GetExtension(FilePath.FileName);
            var alreadyRequested = _requestEntityStatusService.AlreadyRequested(userId);
            if (alreadyRequested)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("AlreadyPendingRequest"));
                return View();
            }
            if (extension != ".pdf" && !alreadyRequested)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FileMustBePdf"));
                return View();
            }
            if (extension == ".pdf" && !alreadyRequested)
            {
                var fiName = userId.ToString() + Path.GetExtension(FilePath.FileName);
                var filePath = _webhostEnvironemnt.WebRootPath + @"\public\uploads\requests\" + fiName;
                var stream = new FileStream(filePath, FileMode.Create);
                await FilePath.CopyToAsync(stream);
                stream.Close();//Utilizar Guid para distinguir os diferentes ficheiros
                request.UserId = userId;
                request.Motivation = motivation;
                request.FilePath = filePath;
                request.OriginalFileName = FilePath.FileName;
                request.AlteredFileName = fiName;
                var success = await _requestEntityStatusService.CreateRequest(request);
                if (success)
                {
                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RequestSent"));
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RequestFail"));
                }
            }
            return View();
        }

        /// <summary>
        /// Ação que permite editar um pedido
        /// <param name="id">Id do pedido</param>
        /// </summary>
        /// <returns>IActionResult - Nova página</returns
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == null || await _requestEntityStatusService.GetRequestById(id) == null)
            {
                return NotFound();
            }
            var isCurrentUserModAdminEntity = await _userService.IsCurrentUserModAdminEntity();
            if (isCurrentUserModAdminEntity)
            {
                return View("NotAllowedAccess");
            }

            var requestEntityStatus = await _requestEntityStatusService.GetRequestById(id);
            if (requestEntityStatus == null)
            {
                return NotFound();
            }
            return View(requestEntityStatus);
        }

        /// <summary>
        /// Ação que permite editar um pedido
        /// <param name="id">Id do pedido</param>
        /// <param name="requestEntityStatus">Pedido</param>
        /// </summary>
        /// <returns>IActionResult - Nova página</returns
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, IFormFile FilePath, string motivation, RequestEntityStatus requestEntityStatus)
        {
            if (id != requestEntityStatus.Id)
            {
                return NotFound();
            };
            string extension = Path.GetExtension(FilePath.FileName);
            if (extension != ".pdf")
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FileMustBePdf"));
                return View();
            }
            else
            {
                var currentUserId = CurrentUserId();

                if (currentUserId == Guid.Empty) return RedirectToAction("Index", "Home");

                var fiName = currentUserId + Path.GetExtension(FilePath.FileName);
                var filePath = _webhostEnvironemnt.WebRootPath + @"\public\uploads\requests\" + fiName;
                var stream = new FileStream(filePath, FileMode.Create);
                await FilePath.CopyToAsync(stream);
                stream.Close();
                requestEntityStatus.UserId = currentUserId;
                requestEntityStatus.Motivation = motivation;
                requestEntityStatus.FilePath = filePath;
                requestEntityStatus.OriginalFileName = FilePath.FileName;
                requestEntityStatus.AlteredFileName = fiName;
                requestEntityStatus.RequestState = RequestState.Pending;
                var success = await _requestEntityStatusService.UpdateRequest(requestEntityStatus);
                if (success)
                {
                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RequestUpdate"));
                    return RedirectToAction("Create", "RequestEntityStatus");
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RequestUpdateFail"));
                }
            }
            return View();
        }
        public FileResult DownloadFile(string fileName)
        {
            string path = _webhostEnvironemnt.WebRootPath + @"\public\uploads\requests\" + fileName;
            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        private Guid CurrentUserId()
        {
            if (!_signInManager.IsSignedIn(User)) return Guid.Empty;
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}

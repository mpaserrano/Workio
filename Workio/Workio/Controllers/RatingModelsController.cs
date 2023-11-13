using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using Workio.Services;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using NToastNotify;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle dos Ratings
    /// </summary>
    public class RatingModelsController : Controller
    {
        private readonly IRatingService _ratingService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly CommonLocalizationService _localizationService;

        public RatingModelsController(IRatingService ratingService, IUserService userService, IToastNotification toastNotification, CommonLocalizationService localizationService)
        {
            _ratingService = ratingService;
            _userService = userService;
            _toastNotification = toastNotification;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Ação para encaminhar para a página de avaliação
        /// </summary>
        /// <param name="id">Id do rating a ser avaliado</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de avaliação </returns>
        public async Task<IActionResult> RateUser(Guid id)
        {
            var alreadyRated = await _ratingService.IsAlreadyRated(id);
            ViewBag.AlreadyRated = alreadyRated;
            ViewBag.RatingId = await _ratingService.GetRatingId(id);
            ViewBag.ValidRatings = new SelectList(Enumerable.Range(0, 6));
            ViewBag.RatedId = id;
            await getUserRatedName(id);
            return View();
        }

        /// <summary>
        /// Ação para encaminhar para a página de avaliação
        /// </summary>
        /// <param name="id">Id do utilizador a ser avaliado</param>
        /// <param name="ratingModel">Rating a ser adicionado</param>
        /// <param name="comment">Comentário opcional</param>
        /// <param name="rating">Avaliação de 0-5</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de utilizador avaliado apos avaliação com sucesso </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateUser(Guid id, RatingModel ratingModel, string? comment, int rating)
        {
            var currentUser = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var alreadyRated = await _ratingService.IsAlreadyRated(id);
            if (alreadyRated)
            {
                var ratingId = await _ratingService.GetRatingId(id);

                ratingModel.RatingId = ratingId;
                ratingModel.ReceiverId = id;
                ratingModel.RaterId = currentUser;
                ratingModel.Rating = rating;
                if (comment == null)
                {
                    ratingModel.Comment = "Did not explain rating.";
                }
                ratingModel.Comment = comment;
                var success = await _ratingService.UpdateRating(ratingModel);
                if (success)
                {

                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RatingUpdated"));
                    return RedirectToAction("Index", "User", new { id = ratingModel.ReceiverId });
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("RatingUpdateFail"));
                    return RedirectToAction("Index", "User", new { id = ratingModel.ReceiverId });
                }
                /*ModelState.AddModelError(String.Empty, _localizationService.Get("AlreadyRated"));
                return View();*/
            }
            if (ModelState.IsValid)
            {

                ratingModel.RatingId = Guid.NewGuid();
                ratingModel.ReceiverId = id;
                ratingModel.RaterId = currentUser;
                ratingModel.Rating = rating;
                if (comment == null)
                {
                    ratingModel.Comment = "Did not explain rating.";
                }
                ratingModel.Comment = comment;
                await _ratingService.AddRating(ratingModel);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("RatingSubmitted"));
                return RedirectToAction("Index", "User", new { id = id });
            }
            _toastNotification.AddErrorToastMessage(_localizationService.Get("RatingUpdateFail"));
            return RedirectToAction("Index", "User", new { id = ratingModel.ReceiverId });
        }

        /// <summary>
        /// Ação para encaminhar para de editar
        /// </summary>
        /// <param name="id">Id do rating a ser editado</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de editar rating</returns>
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == null || await _ratingService.GetRatingById(id) == null)
            {
                return NotFound();
            }

            var ratingModel = await _ratingService.GetRatingById(id);
            if (ratingModel == null)
            {
                return NotFound();
            }
            ViewBag.ValidRatings = new SelectList(Enumerable.Range(0, 6));
            ViewBag.ReceiverId = ratingModel.ReceiverId;
            return View(ratingModel);
        }

        /// <summary>
        /// Ação para encaminhar para de editar
        /// </summary>
        /// <param name="id">Id do rating a ser editado</param>
        /// /// <param name="ratingModel">Rating a ser editado</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de editar rating</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RatingModel ratingModel, string? comment, int rating)
        {
            if (id != ratingModel.RatingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ratingModel.Rating = rating;
                ratingModel.Comment = comment;
                var success = await _ratingService.UpdateRating(ratingModel);
                if (success)
                {

                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RatingUpdated"));
                    return RedirectToAction("Index", "User", new { id = ratingModel.ReceiverId });
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("RatingUpdateFail"));
                    return View(ratingModel);
                }
            }
            return View(ratingModel);
        }

        /// <summary>
        /// Mostra o nome do utilizador a ser avaliado
        /// </summary>
        /// <param name="id">Id do user</param>
        /// <returns>Task<string>nome do utilizador</returns>
        private async Task<string> getUserRatedName(Guid id)
        {
            var userToBeRated = await _userService.GetUser(id);
            string name = userToBeRated.Name;
            ViewBag.Username = name;
            return name;
        }
    }
}


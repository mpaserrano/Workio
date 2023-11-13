using Microsoft.AspNetCore.Mvc;
using Workio.Models.Events;
using Workio.Models;
using Workio.Services.Search;
using Workio.Services.Matchmaking;
using System.Collections;

namespace Workio.Controllers
{
    /// <summary>
    /// Gere os pedidos de informação para recomendações.
    /// </summary>
    public class MatchmakingController : Controller
    {
        /// <summary>
        /// Serviço para gerir as recomendações.
        /// </summary>
        private IMatchmakingService _matchmakingService;

        /// <summary>
        /// Inicializa os parâmetros necessários.
        /// </summary>
        /// <param name="matchmakingService">Serviço de recomendações.</param>
        public MatchmakingController(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        /// <summary>
        /// Retorna uma página para a procura de eventos por perto.
        /// </summary>
        /// <returns>Página de procura por eventos próximos.</returns>
        public IActionResult EventsNear()
        {
            return View();
        }

        /// <summary>
        /// Obtem uma lista de eventos próximos relativamente a uma posição e até uma distância máxima.
        /// </summary>
        /// <param name="latitude">Latitude de referência.</param>
        /// <param name="longitude">Longitude de referência.</param>
        /// <param name="distance">Distância máxima de procura.</param>
        /// <returns></returns>
        public async Task<IActionResult> NearEventsList(double latitude, double longitude, double distance)
        {
            var list = new List<Event>();
            list = await _matchmakingService.GetEventsNear(latitude, longitude, distance);

            ViewBag.Events = list;

            return View();
        }

        /// <summary>
        /// Recebe informações da localização atual do utilizador e da área de procura
        /// e retorna um json com os eventos dentro dessa área.
        /// </summary>
        /// <param name="latitude">Latitude do utilizador.</param>
        /// <param name="longitude">Logitude do utilizador.</param>
        /// <param name="distance">Distância circular de procura por eventos.</param>
        /// <returns></returns>
        public async Task<IActionResult> GetCloseEvents(double latitude, double longitude, double distance)
        {
            //var list = _matchmakingService.GetEventsNear(latitude, longitude, distance);
            var list = await _matchmakingService.GetEventsNearWithDistances(latitude, longitude, distance);

            var response = new
            {
                data = list
            };

            return Json(response);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Search;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador para dar handle dos pedidos de pesquisa
    /// </summary>
    public class SearchController : Controller
    {
        public ISearchService _searchService { get; set; }

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Ação que nos dá os utilizadores filtrados pelo parametro
        /// </summary>
        /// <param name="name">Nome ou email do utilizador a encontrar</param>
        /// <returns>Lista de utilizadores que correspondam ao parametro</returns>
        public async Task<IActionResult> Index(string name)
        {
            if(name != null)
            {
                var users = new List<User>();
                var teams = new List<Team>();
                var events = new List<Event>();

                if (name.Contains("@"))
                {
                    users = await _searchService.GetUsersByEmail(name);
                }
                else
                {
                    users = await _searchService.GetUsersByNameIgnoreAccentuatedCharacters(name);
                    teams = await _searchService.GetTeamsByNameIgnoreAccentuatedCharacters(name);
                    events = await _searchService.GetEventsByNameIgnoreAccentuatedCharacters(name);
                }

                ViewBag.Users = users.Any() ? users : null;   
                ViewBag.Teams = teams.Any() ? teams : null;
                ViewBag.Events = events.Any() ? events : null;
            }

            ViewBag.OldSearch = name != null ? name : null;
                
            return View();
        }
    }
}

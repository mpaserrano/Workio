using Microsoft.AspNetCore.Mvc;
using Workio.Models.Events;

namespace Workio.ViewComponents
{
    /// <summary>
    /// Controlador para o viewcompend relacionado a timeline de eventos.
    /// </summary>
    public class TimelineViewComponent: ViewComponent
    {
        /// <summary>
        /// Retorna a view do componente da timeline de eventos.
        /// </summary>
        /// <returns>View do componente da timeline de eventos.</returns>
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

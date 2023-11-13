using Microsoft.AspNetCore.Mvc;
using Workio.Models.Events;
using Workio.Services.Events;

namespace Workio.ViewComponents
{
    /// <summary>
    /// Componente dos banners featured que extende ViewComponent do .Net MVC
    /// </summary>
    public class FeaturedBannersViewComponent : ViewComponent
    {
        /// <summary>
        /// Serviços dos eventos
        /// </summary>
        private readonly IEventsService _eventsService;
        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="eventsService">Serviço dos eventos</param>
        public FeaturedBannersViewComponent(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }

        /// <summary>
        /// Método assincrono para renderizar o componente
        /// </summary>
        /// <returns>View do componente</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var events = await _eventsService.GetFeaturedEvents();
            if(events == null) { events = new List<Event>(); }
            return View(events);
        }
    }
}

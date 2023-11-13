using Microsoft.AspNetCore.Mvc;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Events;

namespace Workio.ViewComponents
{
    public class TopEventsViewComponent : ViewComponent
    {
        private readonly IEventsService _eventsService;

        public TopEventsViewComponent(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var events = await _eventsService.GetTopEvents(5);
            if (events == null) events = new List<Event>();
            return View(events);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Workio.Models.Events;
using Workio.Services.Events;

namespace Workio.ViewComponents
{
    public class EventsSoonViewComponent : ViewComponent
    {
        private readonly IEventsService _eventsService;

        public EventsSoonViewComponent(IEventsService eventsService) {
            _eventsService = eventsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var events = await _eventsService.GetSoonEvents(5, 5);
            if (events == null) events = new List<Event>();
            return View(events);
        }
    }
}

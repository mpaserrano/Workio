using Microsoft.AspNetCore.Mvc;
using MimeKit.Cryptography;
using Workio.Models.Events;
using Workio.Services.Events;

namespace Workio.ViewComponents
{
    public class EventMiniCardViewComponent : ViewComponent
    {
        private readonly IEventsService _eventsService;
        public EventMiniCardViewComponent(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }

        public IViewComponentResult Invoke(Event @event)
        {
            //var eventItem = _eventsService.GetEvent(id);
            return View(@event);
        }
    }
}

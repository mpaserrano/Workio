using Microsoft.AspNetCore.Mvc;
using Workio.Models.Events;
using Workio.Models;

namespace Workio.ViewComponents
{
    public class LogModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            //var eventItem = _eventsService.GetEvent(id);
            var logModel = new LogViewModel();
            return View(logModel);
        }
    }
}

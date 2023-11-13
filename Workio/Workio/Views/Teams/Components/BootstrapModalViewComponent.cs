using Microsoft.AspNetCore.Mvc;
using Workio.Models;

namespace Workio.Views.Teams.Components
{
    public class BootstrapModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ICollection<User> users, Guid teamId)
        {
            ViewBag.TeamId = teamId;
            return View(users);
        }
    }
}

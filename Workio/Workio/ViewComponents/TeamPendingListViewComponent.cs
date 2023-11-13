using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workio.Models;
using Workio.Services.Teams;

namespace Workio.ViewComponents
{
    public class TeamPendingListViewComponent : ViewComponent
    {
        private ITeamsService _teamsService;

        public TeamPendingListViewComponent(ITeamsService teamsService)
        {
            this._teamsService = teamsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid teamId)
        {
            var pendingList = await _teamsService.GetTeamPendingRequestsByTeamId(teamId);

            Guid currentUserId = User.Identity.IsAuthenticated ? new Guid(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)) : new Guid("00000000-0000-0000-0000-000000000000");

            Team team = null;

            if (pendingList.Count != 0)
            {
                team = pendingList.FirstOrDefault().Team;
            }

            if (team != null)
            {
                ViewBag.Owner = team.OwnerId == currentUserId ? true : false;
            }
            else
            {
                ViewBag.Owner = false;
            }

            return View(pendingList);
        }
    }
}

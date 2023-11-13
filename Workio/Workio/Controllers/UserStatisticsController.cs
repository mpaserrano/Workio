using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using Workio.Models;
using Workio.Services;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle dos pedidos das estatísticas do utilizador
    /// </summary>
    public class UserStatisticsController : Controller
    {
        /// <summary>
        /// Variavel de acesso ao serviço de utilizadores
        /// </summary>
        private IUserService _userService;
        /// <summary>
        /// Variavel de acesso ao serviço de equipas
        /// </summary>
        private ITeamsService _teamsService;
        /// <summary>
        /// Variavel de acesso ao serviço dos eventos
        /// </summary>
        private IEventsService _eventsService;
        /// <summary>
        /// Serviço de tradução
        /// </summary>
        private readonly CommonLocalizationService _commonLocalizationService;

        public UserStatisticsController(IUserService userService, ITeamsService teamsService, IEventsService eventsService, CommonLocalizationService commonLocalizationService)
        {
            _teamsService = teamsService;
            _userService = userService;
            _eventsService = eventsService;
            _commonLocalizationService = commonLocalizationService;
        }

        /// <summary>
        /// Metodo a geração de uma datatable com o numero de equipas que o utilizador participou e o seu status
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> TeamsParticipated(Guid? id)
        {
            var userId = id.HasValue ? id.Value : CurrentUserId();
            var userTeams = await _teamsService.GetAllUserTeamsByUserId(userId);
            DataTable dataTable = new DataTable();

            int teamsFinished = userTeams.Where(t => t.Status == TeamStatus.Finish).Count();
            int teamsOpen = userTeams.Where(t => t.Status == TeamStatus.Open).Count();
            int teamsClosed = userTeams.Where(t => t.Status == TeamStatus.Closed).Count();


            dataTable.Columns.Add("Team Status", typeof(string));
            dataTable.Columns.Add("Number of Teams", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Team Status"] = _commonLocalizationService.Get("Finished");
            row1["Number of Teams"] = teamsFinished;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Team Status"] = _commonLocalizationService.Get("Open");
            row2["Number of Teams"] = teamsOpen;
            dataTable.Rows.Add(row2);

            DataRow row3 = dataTable.NewRow();

            row3["Team Status"] = _commonLocalizationService.Get("Closed");
            row3["Number of Teams"] = teamsClosed;
            dataTable.Rows.Add(row3);

            var total = teamsFinished + teamsOpen + teamsClosed;
            //var data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray();
            var data = new {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (teamsFinished != 0 || teamsOpen != 0 || teamsClosed != 0)
            {
                return Json(data);
            }

            return Json(new {});
        }


        /// <summary>
        /// Metodo a geração de uma datatable com o numero de equipas criadas por mês
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> HowJoined(Guid? id)
        {
            var userId = id.HasValue ? id.Value : CurrentUserId();
            //var userTeams = await _teamsService.GetTeams();
            var userTeams = await _teamsService.GetAllUserTeamsByUserId(userId);
            DataTable dataTable = new DataTable();
            int teamsByRequestAccepted = 0;
            int teamsByInviteAccepted = 0;
            int teamsOwnedCount = userTeams.Where(t => t.OwnerId == userId).Count();
            
            foreach (var t in userTeams)
            {
                var invite = t.InvitedUsers.Where(x => x.UserId == userId.ToString()).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                var request = t.PendingList.Where(x => x.UserId == userId.ToString()).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                if (invite != null && request != null)
                {
                    if (invite.CreatedAt > request.CreatedAt)
                    {
                        t.PendingList.Clear();
                    }
                    else
                    {
                        t.InvitedUsers.Clear();
                    }
                }
            }

            teamsByRequestAccepted = userTeams.Count(team =>
                team.PendingList.Any(t => t.UserId == userId.ToString() && t.Status == PendingUserTeamStatus.Accepted));

            teamsByInviteAccepted = userTeams.Count(team =>
                team.InvitedUsers.Any(t => t.UserId == userId.ToString() && t.Status == PendingUserTeamStatus.Accepted));

            dataTable.Columns.Add("How you have gotten into teams", typeof(string));
            dataTable.Columns.Add("Number of times", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["How you have gotten into teams"] = _commonLocalizationService.Get("RequestAccess");
            row1["Number of times"] = teamsByRequestAccepted;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["How you have gotten into teams"] = _commonLocalizationService.Get("GotInvited");
            row2["Number of times"] = teamsByInviteAccepted;
            dataTable.Rows.Add(row2);

            DataRow row3 = dataTable.NewRow();

            row3["How you have gotten into teams"] = _commonLocalizationService.Get("Owned");
            row3["Number of times"] = teamsOwnedCount;
            dataTable.Rows.Add(row3);


            //var data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray();

            var total = teamsByRequestAccepted + teamsByInviteAccepted + teamsOwnedCount;
            //var data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray();
            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (teamsByRequestAccepted != 0 || teamsByInviteAccepted != 0 || teamsOwnedCount != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }


        /// <summary>
        /// Metodo a geração de uma datatable com o numero de equipas criadas por mês
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> EventsParticipated(Guid? id)
        {
            var userId = id.HasValue ? id.Value : CurrentUserId();
            var events = await _eventsService.GetAllUserInterestedEvents(userId);
            int eventsFinished = 0;
            int eventsOpen = 0;
            int eventsGoing = 0;
            DataTable dataTable = new DataTable();
            foreach (var @event in events)
            {
                    if (@event.State == Models.Events.EventState.Open)
                    {
                        eventsOpen++;
                    }
                    else if (@event.State == Models.Events.EventState.OnGoing)
                    {
                        eventsGoing++;
                    }
                    else
                    {
                        eventsFinished++;
                    }
            }

            dataTable.Columns.Add("Event State", typeof(string));
            dataTable.Columns.Add("Number of events", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Event State"] = _commonLocalizationService.Get("Finished");
            row1["Number of events"] = eventsFinished;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Event State"] = _commonLocalizationService.Get("Open");
            row2["Number of events"] = eventsOpen;
            dataTable.Rows.Add(row2);

            DataRow row3 = dataTable.NewRow();

            row3["Event State"] = _commonLocalizationService.Get("OnGoing");
            row3["Number of events"] = eventsGoing;
            dataTable.Rows.Add(row3);

            var total = eventsFinished + eventsOpen + eventsGoing;
            //var data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray();
            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (eventsFinished != 0 || eventsOpen != 0 || eventsGoing != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}

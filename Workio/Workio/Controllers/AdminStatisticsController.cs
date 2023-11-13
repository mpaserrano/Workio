//using Google.DataTable.Net.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify.Helpers;
using NuGet.Protocol;
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
    /// Controlador para a gestão das estatisticas apresentadas na administração
    /// </summary>
    [Authorize(Roles = "Admin, Mod")]
    public class AdminStatisticsController : Controller
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
        /// <summary>
        /// Lista de strings que contêm cada mês do ano
        /// </summary>
        private List<String> allMonths;
        /// <summary>
        /// Serviço de tradução
        /// </summary>
        private readonly CommonLocalizationService _commonLocalizationService;


        /// <summary>
        /// Constructor da classe.
        /// </summary>
        /// <param name="userService">Variavel para aceder ao serviço de utilizador</param>
        /// <param name="teamsService">Variavel para aceder ao serviço de equipas</param>
        public AdminStatisticsController(IUserService userService, ITeamsService teamsService, IEventsService eventsService, CommonLocalizationService commonLocalizationService)
        {
            _teamsService = teamsService;
            _userService = userService;
            _eventsService = eventsService;
            _commonLocalizationService = commonLocalizationService;
            allMonths = new List<String>() { _commonLocalizationService.Get("January"),
                                             _commonLocalizationService.Get("February"),
                                             _commonLocalizationService.Get("March"),
                                             _commonLocalizationService.Get("April"),
                                             _commonLocalizationService.Get("May"),
                                             _commonLocalizationService.Get("June"),
                                             _commonLocalizationService.Get("July"),
                                             _commonLocalizationService.Get("August"),
                                             _commonLocalizationService.Get("September"),
                                             _commonLocalizationService.Get("October"),
                                             _commonLocalizationService.Get("November"),
                                             _commonLocalizationService.Get("December")
            };
        }
        /// <summary>
        /// Metodo a geração de uma datatable com o numero de contas criadas por mês
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> TotalUserStats()
        {
            var users = await _userService.GetUsersAsync();
            var thisYearUsers = users.Where(u => u.RegisterAt.Year == DateTime.Now.Year);
            List<int> usersPerMonth = new List<int>();
            var monthNumber = 1;
            DataTable dataTable = new DataTable();
            int count = 1;
            List<String> months = new List<String>();

            //Criar lista com apenas o meses que ja passaram/estamos este ano
            foreach (var month in allMonths)
            {
                if (count <= DateTime.Now.Month)
                {
                    months.Add(month);
                }
                count++;
            }

            //Contagem dos utilizadores registados por mês
            foreach (var month in months)
            {
                count = 0;
                foreach (var user in thisYearUsers)
                {
                    if (user.RegisterAt.Month == monthNumber)
                    {
                        count++;
                    }
                }
                usersPerMonth.Add(count);
                monthNumber++;
            }

            dataTable.Columns.Add("Month", typeof(string));
            dataTable.Columns.Add("Users", typeof(int));

            int total = 0;

            for (int i = 0; i < months.Count(); i++)
            {
                DataRow row = dataTable.NewRow();
                row["Month"] = months[i];
                row["Users"] = usersPerMonth[i];
                dataTable.Rows.Add(row);
                total += usersPerMonth[i];
            }

            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        /// <summary>
        /// Metodo a geração de uma datatable com o numero de utilizadores tem equipa e os que não têm.
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> UsersWithTeam()
        {
            var users = await _userService.GetUsersAsync();
            DataTable dataTable = new DataTable();
            int count = 1;
            int usersWithTeams = 0;
            int usersWithoutTeams = 0;
            ICollection<Team> userTeams;
            foreach (var user in users)
            {
                userTeams = await _teamsService.GetUserTeams(Guid.Parse(user.Id));
                if (userTeams.Count() == 0)
                {
                    usersWithoutTeams++;
                }
                else
                {
                    usersWithTeams++;
                }
            }


            dataTable.Columns.Add("Found team or not", typeof(string));
            dataTable.Columns.Add("Number of Users", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Found team or not"] = _commonLocalizationService.Get("FoundTeam");
            row1["Number of Users"] = usersWithTeams;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Found team or not"] = _commonLocalizationService.Get("NoTeam");
            row2["Number of Users"] = usersWithoutTeams;
            dataTable.Rows.Add(row2);

            int total = usersWithTeams + usersWithoutTeams;

            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }


        /// <summary>
        /// Metodo a geração de uma datatable com o numero de equipas criadas por mês
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> TotalTeamsStats()
        {
            var teams = await _teamsService.GetTeams();
            var thisYearTeams = teams.Where(t => t.CreatedAt.Year == DateTime.Now.Year);
            List<int> teamsPerMonth = new List<int>();
            var monthNumber = 1;
            DataTable dataTable = new DataTable();
            int count = 1;
            List<String> months = new List<String>();

            //Criar lista com apenas o meses que ja passaram/estamos este ano
            foreach (var month in allMonths)
            {
                if (count <= DateTime.Now.Month)
                {
                    months.Add(month);
                }
                count++;
            }

            //Contagem das equipas criadas por mês
            foreach (var month in months)
            {
                count = 0;
                foreach (var user in thisYearTeams)
                {
                    if (user.CreatedAt.Month == monthNumber)
                    {
                        count++;
                    }
                }
                teamsPerMonth.Add(count);
                monthNumber++;
            }

            dataTable.Columns.Add("Month", typeof(string));
            dataTable.Columns.Add("Teams", typeof(int));

            int total = 0;

            for (int i = 0; i < months.Count(); i++)
            {
                DataRow row = dataTable.NewRow();
                row["Month"] = months[i];
                row["Teams"] = teamsPerMonth[i];
                dataTable.Rows.Add(row);
                total += teamsPerMonth[i];
            }

            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }


        /// <summary>
        /// Metodo a geração de uma datatable com o numero de eventos criados por mês
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> TotalEventsStats()
        {
            var events = await _eventsService.GetEvents();
            var thisYearEvents = events.Where(e => e.CreatedAt.Year == DateTime.Now.Year);
            List<int> eventsPerMonth = new List<int>();
            var monthNumber = 1;
            DataTable dataTable = new DataTable();
            int count = 1;
            List<String> months = new List<String>();

            //Criar lista com apenas o meses que ja passaram/estamos este ano
            foreach (var month in allMonths)
            {
                if (count <= DateTime.Now.Month)
                {
                    months.Add(month);
                }
                count++;
            }

            //Contagem dos eventos criadas por mês
            foreach (var month in months)
            {
                count = 0;
                foreach (var @event in thisYearEvents)
                {
                    if (@event.CreatedAt.Month == monthNumber)
                    {
                        count++;
                    }
                }
                eventsPerMonth.Add(count);
                monthNumber++;
            }

            dataTable.Columns.Add("Month", typeof(string));
            dataTable.Columns.Add("Events", typeof(int));

            int total = 0;

            for (int i = 0; i < months.Count(); i++)
            {
                DataRow row = dataTable.NewRow();
                row["Month"] = months[i];
                row["Events"] = eventsPerMonth[i];
                dataTable.Rows.Add(row);
                total += eventsPerMonth[i];
            }

            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        /// <summary>
        /// Metodo a geração de uma datatable com o numero de interessados vs o numero de não interessados em eventos
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> InterestedInEvents()
        {
            var events = await _eventsService.GetEvents();
            var users = await _userService.GetUsersAsync();
            DataTable dataTable = new DataTable();
            int interested = 0;
            int notInterested = 0;
            HashSet<string> interestedUsers = new HashSet<string>();

            foreach (var @event in events)
            {
                foreach (var user in users)
                {
                    if (@event.InterestedUsers.Any(e => e.User.Id == user.Id) ||
                        @event.InterestedTeams.Where(t => t.Team.Members.Any(u => u.Id == user.Id)).Count() != 0 ||
                        @event.InterestedTeams.Where(t => t.Team.OwnerId.ToString() == user.Id).Count() != 0)
                    {
                        interestedUsers.Add(user.Id);
                    }
                }
            }

            interested = interestedUsers.Count;
            notInterested = users.Count - interested;

            dataTable.Columns.Add("Interested", typeof(string));
            dataTable.Columns.Add("Number of Users", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Interested"] = _commonLocalizationService.Get("Interested");
            row1["Number of Users"] = interested;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Interested"] = _commonLocalizationService.Get("NotInterested");
            row2["Number of Users"] = notInterested;
            dataTable.Rows.Add(row2);

            var total = interested + notInterested;
            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
                totalInteressed = interested
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        /// <summary>
        /// Metodo a geração de uma datatable com o numero de utilizadores vs equipas interessados
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> InterestedUsersandTeams()
        {
            var events = await _eventsService.GetEvents();
            var users = await _userService.GetUsersAsync();
            DataTable dataTable = new DataTable();

            //Verificar se em cada evento o utilizador esta interessado ou como user, como membro de uma equipa ou como dono de uma equipa
            HashSet<string> interestedUsers = new HashSet<string>();
            HashSet<string> interestedTeams = new HashSet<string>();

            foreach (var @event in events)
            {
                foreach (var user in users)
                {
                    if (@event.InterestedUsers.Any(e => e.User.Id == user.Id)) //contar numero de utilizadores interessados no total
                    {
                        interestedUsers.Add(user.Id);
                    }
                    else if (@event.InterestedTeams.Where(t => t.Team.Members.Any(u => u.Id == user.Id)).Count() != 0 ||
                        @event.InterestedTeams.Where(t => t.Team.OwnerId.ToString() == user.Id).Count() != 0) //contar numero de equipas interessadas no total
                    {
                        interestedTeams.Add(user.Id);
                    }
                }
            }


            dataTable.Columns.Add("Interested", typeof(string));
            dataTable.Columns.Add("Number of interested", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Interested"] = _commonLocalizationService.Get("InterestedUsers");
            row1["Number of interested"] = interestedUsers.Count;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Interested"] = _commonLocalizationService.Get("InterestedTeams");
            row2["Number of interested"] = interestedTeams.Count;
            dataTable.Rows.Add(row2);

            int total = interestedUsers.Count + interestedTeams.Count;

            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        /// <summary>
        /// Metodo a geração de uma datatable com o status das equipas
        /// </summary>
        /// <returns>Task<IActionResult> - Ficheiro JSON com a datatable criada</returns
        public async Task<IActionResult> TeamsCompleted()
        {
            var teams = await _teamsService.GetTeams();
            DataTable dataTable = new DataTable();
            int teamsOpened = 0;
            int teamsFinished = 0;
            int teamsClosed = 0;

            foreach (var team in teams)
            {
                if(team.Status == TeamStatus.Finish)
                {
                    teamsFinished++;
                }else if (team.Status == TeamStatus.Closed)
                {
                    teamsClosed++;
                }else
                {
                    teamsOpened++;
                }
            }


            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("Number of Teams", typeof(int));

            DataRow row1 = dataTable.NewRow();

            row1["Status"] = _commonLocalizationService.Get("Open");
            row1["Number of Teams"] = teamsOpened;
            dataTable.Rows.Add(row1);

            DataRow row2 = dataTable.NewRow();

            row2["Status"] = _commonLocalizationService.Get("Closed");
            row2["Number of Teams"] = teamsClosed;
            dataTable.Rows.Add(row2);

            DataRow row3 = dataTable.NewRow();

            row3["Status"] = _commonLocalizationService.Get("Finished");
            row3["Number of Teams"] = teamsFinished;
            dataTable.Rows.Add(row3);

            var total = teamsFinished + teamsOpened + teamsClosed;
            //var data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray();
            var data = new
            {
                data = dataTable.AsEnumerable().Select(r => r.ItemArray).ToArray(),
                total = total,
            };

            if (total != 0)
            {
                return Json(data);
            }

            return Json(new { });
        }

        /// <summary>
        /// Metodo responsavel por redirecionar a view das estatísticas
        /// </summary>
        /// <returns>Task<IActionResult> - retorna para a view das estatisticas</returns
        public async Task<IActionResult> Statistics()
        {
            var users = await _userService.GetUsersAsync();
            var teams = await _teamsService.GetTeams();
            var events = await _eventsService.GetEvents();
            int usersWithTeams = 0;

            ICollection<Team> userTeams;
            int teamsOpened = 0;
            int teamsFinished = 0;
            int teamsClosed = 0;

            foreach (var team in teams)
            {
                if (team.Status == TeamStatus.Finish)
                {
                    teamsFinished++;
                }else if (team.Status == TeamStatus.Closed)
                {
                    teamsClosed++;
                }
                else
                {
                    teamsOpened++;
                }
                
            }

            foreach (var user in users)
            {
                userTeams = await _teamsService.GetUserTeams(Guid.Parse(user.Id));
                if (userTeams.Count() > 0)
                {
                    usersWithTeams++;
                }
            }

            //Contar todos os users interessados em eventos
            HashSet<string> interestedUsers = new HashSet<string>();
            HashSet<string> interestedTeams = new HashSet<string>();

            foreach (var @event in events)
            {
                foreach (var user in users)
                {
                    if (@event.InterestedUsers.Any(e => e.User.Id == user.Id)) //contar numero de utilizadores interessados no total
                    {
                        interestedUsers.Add(user.Id);
                    }
                    else if (@event.InterestedTeams.Where(t => t.Team.Members.Any(u => u.Id == user.Id)).Count() != 0 ||
                        @event.InterestedTeams.Where(t => t.Team.OwnerId.ToString() == user.Id).Count() != 0) //contar numero de equipas interessadas no total
                    {
                        interestedTeams.Add(user.Id);
                    }
                }
            }

            int totalInterested = interestedUsers.Count + interestedTeams.Count;


            ViewBag.FoundTeam = usersWithTeams;
            ViewBag.TotalUsers = users.Count();
            ViewBag.UsersRegisteredThisYear = users.Where(u => u.RegisterAt.Year == DateTime.Now.Year).Count();
            ViewBag.BannedUsers = users.Where(u => u.LockoutEnd > DateTime.Now).Count();
            ViewBag.TotalTeams = teams.Count();
            ViewBag.TeamsCreatedThisYear = teams.Where(t => t.CreatedAt.Year == DateTime.Now.Year).Count();
            ViewBag.SuspendedTeams = teams.Where(t => t.IsBanned).Count();
            ViewBag.TotalEvents = events.Count();
            ViewBag.EventsCreatedThisYear = events.Where(e => e.CreatedAt.Year == DateTime.Now.Year).Count();
            ViewBag.SuspendedEvents = events.Where(e => e.IsBanned).Count();
            ViewBag.EveryoneInterestedInEvents = interestedUsers.Count;
            ViewBag.UsersInterestedInEvents = interestedUsers.Count;
            ViewBag.TeamsInterestedInEvents = interestedTeams.Count;
            ViewBag.TeamsFinished = teamsFinished;
            ViewBag.TeamsOpen = teamsOpened;
            ViewBag.TeamsClose = teamsClosed;
            return View();
        }

        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}

using Moq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Controllers;
using Workio.Services.Admin.Log;
using Workio.Services.Admin;
using Workio.Services.Admin.Teams;
using Workio.Services.Interfaces;
using Workio.Services;
using Workio.Services.ReportServices;
using Workio.Services.Teams;
using Microsoft.Extensions.Localization;
using Workio.Controllers.Admin;
using Microsoft.AspNetCore.Hosting;

namespace Workio.Tests.Controller.Admin
{
    public class AdminTeamControllerTests
    {
        private Mock<IWebHostEnvironment> _webHostEnvironment;
        private Mock<IToastNotification> _toastNotification;
        private Mock<IUserService> _userService;
        private Mock<IAdminTeamService> _adminTeamService;
        private Mock<ILogsService> _logsService;
        private Mock<ITeamsService> _teamsService;
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<CommonLocalizationService> _localizationService;
        private AdminTeamController _controller;




        [Fact]
        public void SetUp()
        {

            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _toastNotification = new Mock<IToastNotification>();
            _userService = new Mock<IUserService>();
            _adminTeamService = new Mock<IAdminTeamService>();
            _logsService = new Mock<ILogsService>();
            _teamsService = new Mock<ITeamsService>();
            _reportReasonService = new Mock<IReportReasonService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new AdminTeamController(_webHostEnvironment.Object,
                                                  _toastNotification.Object,
                                                  _userService.Object,
                                                  _adminTeamService.Object,
                                                  _logsService.Object,
                                                  _teamsService.Object,
                                                  _reportReasonService.Object, _localizationService.Object);
        }

    }
}

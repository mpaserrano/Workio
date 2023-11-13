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
using Workio.Services.Interfaces;
using Workio.Services;
using Workio.Services.ReportServices;
using Workio.Services.Teams;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Workio.Controllers.Admin;
using Workio.Services.Admin.Teams;
using Workio.Services.Events;

namespace Workio.Tests.Controller.Admin
{
    public class AdminReportsControllerTests
    {
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<IUserService> _userService;
        private Mock<ITeamsService> _teamsService;
        private Mock<IEventsService> _eventsService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<ILogsService> _logsService;
        private Mock<CommonLocalizationService> _localizationService;
        private AdminReportsController _controller;




        [Fact]
        public void SetUp()
        {

            _reportReasonService = new Mock<IReportReasonService>();
            _userService = new Mock<IUserService>(); 
            _teamsService = new Mock<ITeamsService>();
            _eventsService = new Mock<IEventsService>();
            _toastNotification = new Mock<IToastNotification>();
            _logsService = new Mock<ILogsService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new AdminReportsController(_reportReasonService.Object,
                                                     _userService.Object,
                                                     _teamsService.Object,
                                                     _eventsService.Object,
                                                     _toastNotification.Object,
                                                     _logsService.Object,
                                                     _localizationService.Object);
        }

    }
}


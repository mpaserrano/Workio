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
using Microsoft.AspNetCore.Hosting;
using Workio.Controllers.Admin;
using Workio.Services.Admin.Teams;
using Workio.Services.Events;
using Workio.Services.Admin.Events;

namespace Workio.Tests.Controller.Admin
{
    public class AdminEventControllerTests
    {
        private Mock<IWebHostEnvironment> _webHostEnvironment;
        private Mock<IToastNotification> _toastNotification;
        private Mock<IUserService> _userService;
        private Mock<IAdminEventService> _adminEventService;
        private Mock<ILogsService> _logsService;
        private Mock<IEventsService> _eventService;
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<CommonLocalizationService> _commonLocalizationService;
        private AdminEventController _controller;





        [Fact]
        public void SetUp()
        {

            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _toastNotification = new Mock<IToastNotification>();
            _userService = new Mock<IUserService>();
            _adminEventService = new Mock<IAdminEventService>();
            _logsService = new Mock<ILogsService>();
            _eventService = new Mock<IEventsService>();
            _reportReasonService = new Mock<IReportReasonService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _commonLocalizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new AdminEventController(_webHostEnvironment.Object,
                                                  _toastNotification.Object,
                                                  _userService.Object,
                                                  _adminEventService.Object,
                                                  _logsService.Object,
                                                  _eventService.Object,
                                                  _reportReasonService.Object,
                                                  _commonLocalizationService.Object);
        }

    }

}

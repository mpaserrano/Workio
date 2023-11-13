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

namespace Workio.Tests.Controller.Admin
{
    public class AdminEntitiesControllerTests
    {
        private Mock<IAdminService> _adminService;
        private Mock<IUserService> _userService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<ILogsService> _logsService;
        private Mock<CommonLocalizationService> _localizationService;
        private AdminEntitiesController _controller;




        [Fact]
        public void SetUp()
        {

            _adminService = new Mock<IAdminService>();
            _userService = new Mock<IUserService>();
            _toastNotification = new Mock<IToastNotification>();
            _logsService = new Mock<ILogsService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new AdminEntitiesController(_adminService.Object,
                                                      _toastNotification.Object,
                                                      _userService.Object,
                                                      _logsService.Object,
                                                       _localizationService.Object);
        }

        
    }
}

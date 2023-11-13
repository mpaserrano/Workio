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
using Microsoft.AspNetCore.Identity;
using Workio.Models;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Workio.Services.Email.Interfaces;
using Workio.Managers.Notifications;

namespace Workio.Tests.Controller.Admin
{
    public class AdminControllerTests
    {
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<IUserService> _userService;
        private Mock<IAdminService> _adminService;
        private Mock<ILogsService> _logsService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<ITeamsService> _teamsServiceMock;
        private Mock<CommonLocalizationService> _localizationService;
        private Mock<RoleManager<IdentityRole>> _roleManager;
        private Mock<UserManager<User>> _userManager;
        private Mock<INotificationManager> _notificationManagerMock;
        private AdminController _controller;




        [Fact]
        public void SetUp()
        {

            _reportReasonService = new Mock<IReportReasonService>();
            _userService = new Mock<IUserService>();
            _adminService = new Mock<IAdminService>();
            _logsService = new Mock<ILogsService>();
            _toastNotification = new Mock<IToastNotification>();
            _emailServiceMock = new Mock<IEmailService>();
            _teamsServiceMock = new Mock<ITeamsService>();
            _notificationManagerMock= new Mock<INotificationManager>();

            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();

            _roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new AdminController(_reportReasonService.Object,
                                               _userService.Object,
                                               _adminService.Object,
                                               _logsService.Object,
                                               _toastNotification.Object,
                                               _emailServiceMock.Object,
                                               _teamsServiceMock.Object,
                                               _userManager.Object,
                                               _roleManager.Object,
                                               _localizationService.Object,
                                               _notificationManagerMock.Object);
        }

        [Fact]
        public void AdminController_Index_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Index();

            //Assert
            Assert.NotNull(result);
        }
    }
}

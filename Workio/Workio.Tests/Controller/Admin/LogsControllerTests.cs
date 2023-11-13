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
using Workio.Services.Events;
using Microsoft.AspNetCore.Identity;
using Workio.Models;
using Microsoft.AspNetCore.Http;
using Workio.Services.LocalizationServices;
using Workio.Controllers.Admin.Logs;

namespace Workio.Tests.Controller.Admin
{
    public class LogsControllerTests
    {
        private Mock<UserManager<User>> _userManager;
        private Mock<SignInManager<User>> _signInManager;
        private Mock<ILogsService> _logsService;
        private Mock<IUserService> _userService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<ILocalizationService> _localizationService;
        private LogsController _controller;




        [Fact]
        public void SetUp()
        {
            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var httpMock = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManager = new Mock<SignInManager<User>>(_userManager.Object, httpMock.Object, userClaims.Object, null, null, null, null);


            _logsService = new Mock<ILogsService>();
            _userService = new Mock<IUserService>();
            _toastNotification = new Mock<IToastNotification>();
            _localizationService= new Mock<ILocalizationService>();

            _controller = new LogsController(_signInManager.Object,
                                             _logsService.Object,
                                             _userService.Object,
                                             _toastNotification.Object,
                                             _localizationService.Object);
        }

    }
}

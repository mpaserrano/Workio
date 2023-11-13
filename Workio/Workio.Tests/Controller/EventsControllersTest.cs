using Microsoft.AspNetCore.Identity;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Models;
using Workio.Services.Events;
using Workio.Services.LocalizationServices;
using Moq;
using Workio.Controllers.Events;
using Microsoft.AspNetCore.Http;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using Workio.Services.Search;
using Workio.Services.Teams;
using Workio.Services.Matchmaking;
using Microsoft.AspNetCore.Hosting;
using Workio.Services;
using Microsoft.Extensions.Localization;

namespace Workio.Tests.Controller
{
    public class EventsControllersTest
    {
        private Mock<UserManager<User>>_userManager;
        private Mock<SignInManager<User>> _signInManager;
        private Mock<IEventsService> _eventsService;
        private Mock<IToastNotification>? _toastNotification;
        private Mock<ILocalizationService> _localizationService;
        private Mock<IMatchmakingService> _matchMakingService;
        private Mock<IWebHostEnvironment> _webHostEnvironment;
        private Mock<IUserService> _userServiceMock;
        private Mock<CommonLocalizationService> _commonLocalizationService;
        private EventsController _controller;


        [Fact]
        public void SetUp()
        {
            var store = new Mock<IUserStore<User>>();
            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            var httpMock = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManager = new Mock<SignInManager<User>>(_userManager.Object,
                                                           httpMock.Object,
                                                           userClaims.Object, null, null, null, null);
            _toastNotification = new Mock<IToastNotification>();
            _localizationService = new Mock<ILocalizationService>();
            _eventsService = new Mock<IEventsService>();
            _matchMakingService = new Mock<IMatchmakingService>();
            _userServiceMock = new Mock<IUserService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _commonLocalizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new EventsController(_signInManager.Object,
                                               _userManager.Object,
                                               _eventsService.Object,
                                               _toastNotification.Object,
                                               _localizationService.Object,
                                               _matchMakingService.Object,
                                               _webHostEnvironment.Object,
                                               _userServiceMock.Object, _commonLocalizationService.Object);
            
        }

        [Fact]
        public void EventsController_Index_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            int[] myArray = new int[3];
            var result = _controller.Index(myArray);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Details_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Details(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Create_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Create();

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Create2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Create(new Models.Events.Event());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Edit_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Edit(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Edit2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Edit(new Guid(), new Models.Events.Event());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_Delete_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Delete(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventsController_DeleteConfirmed_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.DeleteConfirmed(new Guid());

            //Assert
            Assert.NotNull(result);
        }



    }
}

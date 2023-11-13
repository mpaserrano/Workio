using Microsoft.Extensions.Localization;
using Moq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Controllers;
using Workio.Models;
using Workio.Services;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Tests.Controller
{
    public class EventEnrollmentsControllerTests
    {

        private Mock<IToastNotification> _toastNotification;
        private Mock<IEventsService> _eventsService;
        private Mock<IUserService> _userService;
        private Mock<ITeamsService> _teamService;
        private Mock<CommonLocalizationService> _localizationService;

        public EventEnrollmentsController _controller;

        [Fact]
        public void SetUp()
        {

            _toastNotification = new Mock<IToastNotification>();
            _eventsService= new Mock<IEventsService>();
            _userService = new Mock<IUserService>();
            _teamService= new Mock<ITeamsService>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new EventEnrollmentsController(_toastNotification.Object, _eventsService.Object, _userService.Object, _teamService.Object, _localizationService.Object);
        }

        [Fact]
        public void EventEnrollmentsController_EnrollUsers_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.EnrollUser(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void EventEnrollmentsController_CancellEnrollmentUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.CancellEnrollmentUser(new Guid());

            //Assert
            Assert.NotNull(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Controllers;
using Workio.Services.Events;
using Workio.Services.ReportServices;
using Workio.Services.Teams;
using Moq;
using Workio.Models;
using NToastNotify;
using Workio.Services;
using Microsoft.Extensions.Localization;

namespace Workio.Tests.Controller
{
    public class ReportsControllerTests
    {
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<ITeamsService> _teamsService;
        private Mock<IEventsService> _eventsService;
        private Mock<IToastNotification> _toastNotification;
        private ReportsController _controller;
        private Mock<CommonLocalizationService> _localizationService;

        [Fact]
        public void SetUp()
        {
            _reportReasonService = new Mock<IReportReasonService>();
            _teamsService = new Mock<ITeamsService>();
            _eventsService = new Mock<IEventsService>();
            _toastNotification = new Mock<IToastNotification>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);
            _controller = new ReportsController(_reportReasonService.Object, _teamsService.Object, _eventsService.Object, _toastNotification.Object, _localizationService.Object);
        }

        [Fact]
        public void ReportsController_ReportUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportUser(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ReportsController_ReportUser2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportUser(new ReportUser(), new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ReportsController_ReportTeam_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportTeam(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ReportsController_ReportTeam2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportTeam(new ReportTeam(), new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ReportsController_ReportEvent_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportEvent(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ReportsController_ReportEvent2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.ReportEvent(new ReportEvent(), new Guid());

            //Assert
            Assert.NotNull(result);
        }


    }

   
}

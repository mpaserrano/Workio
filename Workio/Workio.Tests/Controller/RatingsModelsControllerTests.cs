using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Services.Interfaces;
using Moq;
using Workio.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Workio.Models;
using Workio.Services.LocalizationServices;
using Workio.Services;
using Microsoft.Extensions.Localization;

namespace Workio.Tests.Controller
{
    public class RatingsModelsControllerTests
    {
        private Mock<IRatingService> _ratingService;
        private Mock<IUserService> _userService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<CommonLocalizationService> _localizationService;
        private RatingModelsController _controller;

        [Fact]
        public void SetUp()
        {
            _ratingService = new Mock<IRatingService>();
            _userService= new Mock<IUserService>();
            _toastNotification = new Mock<IToastNotification>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new RatingModelsController(_ratingService.Object, _userService.Object, _toastNotification.Object, _localizationService.Object);
        }

        [Fact]
        public void RatingModelsController_Create_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.RateUser(new Guid(), new RatingModel(), "aaaa", 5);
            var result1 = _controller.RateUser(new Guid(), new RatingModel(), "aaaa", 10);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);
        }

        [Fact]
        public void RatingModelsController_Create2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.RateUser(new Guid());
            var result1 = _controller.RateUser(new Guid());

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);
        }
    }
}

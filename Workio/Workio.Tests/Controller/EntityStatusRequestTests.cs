using Microsoft.AspNetCore.Hosting;
using NToastNotify;
using Workio.Controllers;
using Workio.Data;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Workio.Services.RequestEntityStatusServices;
using Workio.Services.Interfaces;
using Workio.Models;
using Microsoft.AspNetCore.Identity;
using Workio.Services;
using Microsoft.Extensions.Localization;

namespace Workio.Tests.Controller
{
    public class EntityStatusRequestTests
    {
        private Mock<UserManager<User>> _userManager;
        private Mock<IWebHostEnvironment> _webhostEnvironemnt;
        private Mock<IToastNotification> _toastNotification;
        private Mock<IUserService> _userService;
        private Mock<IRequestEntityStatusService> _requestEntityStatusService;
        private Mock<SignInManager<User>> _signInManagerMock;
        private Mock<CommonLocalizationService> _localizationService;
        private RequestEntityStatusController _controller;

        private ClaimsIdentity _identity;
        private ClaimsPrincipal _user;

        [Fact]
        public void SetUp()
        {
            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _webhostEnvironemnt = new Mock<IWebHostEnvironment>();
            _toastNotification = new Mock<IToastNotification>();
            _requestEntityStatusService= new Mock<IRequestEntityStatusService>();
            _userService= new Mock<IUserService>();

            var httpMock = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(_userManager.Object, httpMock.Object, userClaims.Object, null, null, null, null);
            _identity = new ClaimsIdentity();
            _user = new ClaimsPrincipal(_identity);

            var logger = new NullLogger<RequestEntityStatusController>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "7b4bcdd7-668c-4d07-9dc0-03bd01d235e8")
                                        // other required and custom claims
                                   }, "TestAuthentication"));
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _localizationService = new Mock<CommonLocalizationService>(localizer.Object);

            _controller = new RequestEntityStatusController( _webhostEnvironemnt.Object, _toastNotification.Object, _userService.Object,_requestEntityStatusService.Object, _signInManagerMock.Object, _localizationService.Object);
            _controller.ControllerContext = new ControllerContext();
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        [Fact]
        public void RequestEntityStatusController_Create_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            
            var result = _controller.Create(new Guid(), new Mock<IFormFile>().Object, "AAAAA",new RequestEntityStatus());
            

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void RequestEntityStatusController_Create2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Create();

            //Assert
            Assert.NotNull(result);
        }



    }
}

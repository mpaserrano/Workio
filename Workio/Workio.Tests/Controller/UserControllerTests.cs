using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Workio.Models;
using Workio.Services.Connections;
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Workio.Services;
using Workio.Data;
using Moq;
using Workio.Controllers;
using Microsoft.AspNetCore.Http;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Workio.Managers.Notifications;
using Microsoft.Extensions.Localization;
using NToastNotify;
using Workio.Services.Events;
using Workio.Services.Chat;

namespace Workio.Tests.Controller
{
    public class UserControllerTests
    {
        private UserController _controller;
        private Mock<UserManager<User>> _userManager;
        private Mock<SignInManager<User>> _signInManager;
        private Mock<IUserService> _userService;
        private Mock<IConnectionService> _connectionService;
        private Mock<IRatingService> _ratingService;
        private Mock<ITeamsService> _teamsService;
        private Mock<IEventsService> _eventService;
        private Mock<INotificationManager> _notificationManager;
        private Mock<IToastNotification> _toastNotification;
        private Mock<CommonLocalizationService> _commonLocalizationService;
        private Mock<IChatService> _chatServiceMock;
        private User user;
        private SkillModel skill;
        private ExperienceModel experience;
        private TempDataDictionary tempData;


        [Fact]
        public void SetUp()
        {
            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var httpMock = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManager = new Mock<SignInManager<User>>(_userManager.Object, httpMock.Object, userClaims.Object, null, null, null, null);

            _teamsService = new Mock<ITeamsService>();
            _userService = new Mock<IUserService>();
            _eventService = new Mock<IEventsService>();
            _connectionService = new Mock<IConnectionService>();
            _ratingService = new Mock<IRatingService>();
            _teamsService = new Mock<ITeamsService>();
            _notificationManager= new Mock<INotificationManager>();
            _toastNotification = new Mock<IToastNotification>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _commonLocalizationService = new Mock<CommonLocalizationService>(localizer.Object);
            _chatServiceMock= new Mock<IChatService>();


            var httpContext = new DefaultHttpContext();
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _controller = new UserController(_userManager.Object,
                                             _signInManager.Object,
                                             _userService.Object,
                                             _connectionService.Object,
                                             _ratingService.Object,
                                             _teamsService.Object,
                                             _notificationManager.Object,
                                             _toastNotification.Object,
                                             _commonLocalizationService.Object,
                                             _eventService.Object,
                                             _chatServiceMock.Object)
            {
                TempData = tempData
            };

            user = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user",
                EmailConfirmed = true
            };

            skill = new SkillModel
            {
                SkillId = new Guid("e5cb5f1f-4a05-4e86-858d-649a9df1258a"),
                Name = "C#",
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")
            };

            experience = new ExperienceModel
            {
                ExperienceId = new Guid("6b6fcc14-e0a2-4972-857c-770bf1434435"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                WorkTitle = "Workio",
                Company = "Workio.inc",
                StartDate = DateTime.Now
            };
        }

        [Fact]
        public void UserController_Index_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            

            //Act
            var result = _controller.Index(new Guid(user.Id));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void UserController_AddSkill_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddSkill(skill, true);
            var result1 = _controller.AddSkill(skill, false);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);
        }

        [Fact]
        public void UserController_EditSkill_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.EditSkill(skill.SkillId, skill, true);

            //Assert
            Assert.NotNull(result);

        }


        [Fact]
        public void UserController_DeleteSkill_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.DeleteSkill(skill.SkillId);


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_AllSkills_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AllSkills(new Guid(user.Id));

            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_AddExperience_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddExperience(experience, true);
            var result1 = _controller.AddExperience(experience, false);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);

        }

        [Fact]
        public void UserController_EditExperience_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.EditExperience(experience.ExperienceId,experience, true);
            var result1 = _controller.EditExperience(experience.ExperienceId, experience, false);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);

        }

        [Fact]
        public void UserController_DeleteExperience_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.DeleteExperience(experience.ExperienceId);


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_AllExperiences_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AllExperiences(experience.ExperienceId);


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_EditModeSkill_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.EditModeSkill();


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_EditModeExperience_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.EditModeExperiences();


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_OpenEditExperience_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.OpenEditExperience(experience);


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_AddEndorsement_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddEndorsement(skill.SkillId, new Guid(user.Id), "returnURL");


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_RemoveEndorsement_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result =  _controller.RemoveEndorsement(skill.SkillId, new Guid(user.Id), "returnURL");


            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_AcceptConnection_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AcceptConnection(new Guid(user.Id), true, "returnURL");
            var result1 = _controller.AcceptConnection(new Guid(user.Id), false, "returnURL");

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);

        }

        [Fact]
        public void UserController_AddConnection_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddConnection(new Guid(user.Id));

            //Assert
            Assert.NotNull(result);

        }

        [Fact]
        public void UserController_RejectConnection_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RemoveConnection(new Guid(user.Id), true, "returnURL");
            var result1 = _controller.RemoveConnection(new Guid(user.Id), false, "returnURL");

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);

        }

        /*
        //EXAMPLE

        [Fact]
        public void UserController_Example_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddExperience(experience, true);

            //Assert
            Assert.NotNull(result);

        }
        */


    }
}

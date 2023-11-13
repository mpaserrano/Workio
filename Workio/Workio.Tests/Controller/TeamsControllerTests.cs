using Microsoft.AspNetCore.Identity;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Models;
using Workio.Services.Interfaces;
using Workio.Services.LocalizationServices;
using Workio.Services.ReportServices;
using Workio.Services.Search;
using Workio.Services.Teams;
using Moq;
using Workio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Workio.Services.Connections;
using Workio.Services;
using Microsoft.AspNetCore.Http;
using Workio.Services.Matchmaking;
using Workio.Services.Email.Interfaces;
using Workio.Managers.Notifications;
using Microsoft.Extensions.Localization;
using Workio.Models.Filters;
using Workio.Services.Chat;

namespace Workio.Tests.Controller
{
    public class TeamsControllerTests
    {
        private Mock<UserManager<User>> _userManager;
        private Mock<SignInManager<User>> _signInManager;
        private Mock<ITeamsService> _teamsService;
        private Mock<IToastNotification> _toastNotification;
        private Mock<ILocalizationService> _localizationService;
        private Mock<IReportReasonService> _reportReasonService;
        private Mock<ISearchService> _searchService;
        private Mock<IUserService> _userService;
        private Mock<IMatchmakingService> _matchmakingService;
        private Mock<IEmailService> _emailService;
        private Mock<IChatService> _chatService;
        private Mock<INotificationManager> _notificationManager;
        private Mock<CommonLocalizationService> _commonLocalizationService;
        private TeamsController _controller;
        private User user;
        private Team team;


        [Fact]
        public void SetUp()
        {
            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var httpMock = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();
            
            _signInManager = new Mock<SignInManager<User>>(_userManager.Object, httpMock.Object, userClaims.Object, null, null, null, null);
            _teamsService = new Mock<ITeamsService>();
            _toastNotification = new Mock<IToastNotification>();
            _localizationService = new Mock<ILocalizationService>();
            _reportReasonService = new Mock<IReportReasonService>();
            _searchService = new Mock<ISearchService>();
            _userService = new Mock<IUserService>();
            _matchmakingService = new Mock<IMatchmakingService>();
            _emailService = new Mock<IEmailService>();
            _chatService = new Mock<IChatService>();
            _notificationManager = new Mock<INotificationManager>();
            Mock<IStringLocalizerFactory> localizer = new Mock<IStringLocalizerFactory>();
            _commonLocalizationService = new Mock<CommonLocalizationService>(localizer.Object);





            _controller = new TeamsController(_userManager.Object,
                                             _signInManager.Object,
                                             _teamsService.Object,
                                             _toastNotification.Object,
                                             _localizationService.Object,
                                             _reportReasonService.Object,
                                             _searchService.Object,
                                             _userService.Object,
                                             _matchmakingService.Object,
                                             _emailService.Object,
                                             _chatService.Object,
                                             _notificationManager.Object,
                                             _commonLocalizationService.Object
                                             );

            user = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user",
                EmailConfirmed = true
            };

            team = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                TeamName = "TeamName",
                Description = "Description"
            };

            
        }

        [Fact]
        public void TeamsController_Index_ReturnsSuccess()
        {
            //SetUp
            SetUp();
            var filters = new TeamsFilterViewModel();

            //Act
            var result = _controller.Index(filters);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Details_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Details(team.TeamId);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_FinishProject_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.FinishProject(team.TeamId);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_OpenInscriptions_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.OpenInscriptions(team.TeamId);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_CloseInscriptions_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.CloseInscriptions(team.TeamId);
            

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_AcceptUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AcceptUser(new Guid(user.Id),team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_RemoveUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RemoveUser(new Guid(user.Id), team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_RejectUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RejectUser(new Guid(), new Guid());


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_AskAccess_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RemoveUser(team.TeamId, new Guid(user.Id));


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_CancelInvite_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.CancelInvite(team.TeamId, new Guid());


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_AcceptInvite_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AcceptInvite(new Guid());


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_AcceptInviteByTeam_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AcceptInviteByTeam(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_RejectInvite_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RejectInvite(new Guid());


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_RejectInviteByTeam_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.RejectInviteByTeam(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Leave_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AcceptInviteByTeam(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Create_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Create();


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Create2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Create(team);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Edit_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Edit(team.TeamId);


            //Assert
            Assert.NotNull(result);
            
        }

        [Fact]
        public void TeamsController_Edit2_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Edit(team.TeamId, team);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_Delete_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Delete(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_DeleteConfirmed_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Delete(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_OpenModal_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.OpenModal(team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_InviteSearch_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.InviteSearch("query" ,team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_InviteToTeam_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.InviteToTeam(team.TeamId, new Guid(user.Id));


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_AddMilestone_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.AddMilestone(new Milestone(), team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_EditMilestone_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.EditMilestone(new Milestone(), new Guid(), team.TeamId);
                                                                  //milestone ID

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_DeleteMilestone_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.DeleteMilestone(team.TeamId, new Guid());


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_CompleteMilestone_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.CompleteMilestone(new Guid(), team.TeamId);


            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TeamsController_GiveOwnership_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.GiveOwnership(new Guid(), new Guid(user.Id));


            //Assert
            Assert.NotNull(result);
        }
        /*
        //EXAMPLE

        [Fact]
        public void TeamsController_Example_ReturnsSuccess()
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


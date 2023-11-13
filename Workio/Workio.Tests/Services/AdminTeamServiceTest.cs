using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Services.Admin.Log;
using Workio.Services.Connections;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Tests.Services
{
    public class AdminTeamServiceTest
    {

        private Team team1;
        private Team team2;

        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;
        private Mock<IUserService> userServiceMock;
        private Mock<ITeamsService> teamServiceMock;

        private void SetUp()
        {

            //Teams
            // Teams
            team1 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                TeamName = "TeamName",
                Description = "Description",
                Status = TeamStatus.Open,
                Positions = new List<Position>(),
                Members = new List<User>(),
                InvitedUsers = new List<TeamInviteUser>(),
                PendingList = new List<PendingUserTeam>(),
                Skills = new List<Tag>()
            };

            // Only the last digit in the Id is changed
            team2 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3576"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                TeamName = "TeamName2",
                Description = "Description",
                Status = TeamStatus.Open
            };

            var mockHttpContext = new Mock<HttpContext>();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                new Claim(ClaimTypes.NameIdentifier, "a2463fbc-1f6b-470d-b40d-daf9e0bc9745")
            };

            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                .Returns(new HttpContextAccessor { HttpContext = mockHttpContext.Object });
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);

            httpContextAccessor = new HttpContextAccessor { HttpContext = mockHttpContext.Object };


            var store = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var userClaims = new Mock<IUserClaimsPrincipalFactory<User>>();


            userServiceMock = new Mock<IUserService>();
            teamServiceMock = new Mock<ITeamsService>();
        }

        [Fact]
        public async Task AdminTeamService_GetTeams()
        {
            //Arrange
            SetUp();
            List<Team> teams = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminTeamService_GetTeams").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminTeamService(context, userManagerMock.Object,httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);

                context.Team.Add(team1);
                context.Team.Add(team2);
                context.SaveChanges();

                var rawTeams = await service.GetTeams();
                teams = rawTeams.ToList();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if null
                Assert.NotNull(teams);

                //Verify amount of teams
                Assert.Equal(2, teams.Count);

                //Veriffy both teams
                var r1 = teams.Where(x => x.TeamId == team1.TeamId).First();
                var r2 = teams.Where(x => x.TeamId == team2.TeamId).First();

                Assert.Equal(team1.TeamId, r1.TeamId);
                Assert.Equal(team2.TeamId, r2.TeamId);

                var service = new AdminTeamService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task AdminTeamService_BanTeam()
        {
            //Arrange
            SetUp();
            bool banned = false;
            bool banAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminTeamService_BanTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminTeamService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);

                context.Team.Add(team1);
                context.Team.Add(team2);
                context.SaveChanges();

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                banned = await service.BanTeam(team1.TeamId);
                banAgain = await service.BanTeam(team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if banned
                Assert.True(banned);
                Assert.False(banAgain);

                //Verify state on team
                var itemsInDatabase = context.Team.ToList();
                Assert.True(itemsInDatabase.First().IsBanned);

                var service = new AdminTeamService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task AdminTeamService_UnBanTeam()
        {
            //Arrange
            SetUp();
            bool unbanned = false;
            bool unbanAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminTeamService_UnBanTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminTeamService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);
                
                context.Team.Add(team1);
                context.SaveChanges();

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);

                await service.BanTeam(team1.TeamId);

                unbanned = await service.UnbanTeam(team1.TeamId);
                unbanAgain = await service.UnbanTeam(team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if banned
                Assert.True(unbanned);
                Assert.False(unbanAgain);

                //Verify state on team
                var itemsInDatabase = context.Team.ToList();
                Assert.False(itemsInDatabase.First().IsBanned);

                var service = new AdminTeamService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, teamServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }
    }
}

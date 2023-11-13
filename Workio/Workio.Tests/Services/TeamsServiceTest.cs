using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services;
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class TeamsServiceTest
    {
        private User user1;
        private User user2;
        private User user3;

        private Team team1;
        private Team team2;
        private Team team3;

        private Mock<UserManager<User>> userManagerMock;
        private Mock<SignInManager<User>> signInManagerMock;
        private Mock<IHttpContextAccessor> httpMock;
        private Mock<IUserService> userServiceMock;
        private Mock<IRatingService> ratingServiceMock;
        private HttpContextAccessor httpContextAccessor;

        private readonly ITestOutputHelper output;

        public TeamsServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void SetUp()
        {
            // Users
            user1 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user 1",
                EmailConfirmed = true

            };

            user2 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Test user 2",
                EmailConfirmed = true
            };

            user3 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9746",
                UserName = "Test@12345.com",
                Email = "Test@12345.com",
                Name = "Test user 3",
                EmailConfirmed = true
            };

            // Teams
            team1 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                TeamName = "TeamName1",
                Description = "Description",
                Status = TeamStatus.Open,
                Positions = new List<Position>(),
                Members = new List<User>(),
                InvitedUsers = new List<TeamInviteUser>(),
                PendingList = new List<PendingUserTeam>(),
                Skills = new List<Tag>(),
                Milestones = new List<Milestone>(),
                Language = new Localization()
                {
                    IconName = "Aa",
                    Language = "English",
                    Code ="en"
                }
            };

            // Only the last digit in the Id is changed
            team2 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3576"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                TeamName = "TeamName2",
                Description = "Description",
                Status = TeamStatus.Open,
                Positions = new List<Position>(),
                Members = new List<User>(),
                InvitedUsers = new List<TeamInviteUser>(),
                PendingList = new List<PendingUserTeam>(),
                Skills = new List<Tag>(),
                Milestones = new List<Milestone>(),
                Language = new Localization()
                {
                    IconName = "Aa",
                    Language = "English",
                    Code = "en"
                }
            };

            team3 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3577"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                TeamName = "TeamName3",
                Description = "Description",
                Status = TeamStatus.Open,
                Positions = new List<Position>(),
                Members = new List<User>(),
                InvitedUsers = new List<TeamInviteUser>(),
                PendingList = new List<PendingUserTeam>(),
                Skills = new List<Tag>(),
                Milestones = new List<Milestone>(),
                Language = new Localization()
                {
                    IconName = "Aa",
                    Language = "English",
                    Code = "en"
                }
            };

            //Mocks
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
            userClaims.Setup(x => x.CreateAsync(user1)).ReturnsAsync(Mock.Of<ClaimsPrincipal>);
            userClaims.Setup(x => x.CreateAsync(user2)).ReturnsAsync(Mock.Of<ClaimsPrincipal>);
            userClaims.Setup(x => x.CreateAsync(user3)).ReturnsAsync(Mock.Of<ClaimsPrincipal>);

            userServiceMock = new Mock<IUserService>();
            ratingServiceMock = new Mock<IRatingService>();
        }


        [Fact]
        public async Task TeamsService_CreateTeam()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_CreateTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var itemsInDatabase = await context.Team.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var item = await context.Team.FirstAsync();

                // Team should have defined Id
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"), item.TeamId);

                // Date should be equal to today
                var today = DateTime.Now;
                Assert.Equal(today.Date, item.CreatedAt.Date);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_GetTeams()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetTeams").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                //Clear database
                context.Database.EnsureDeleted();


                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Team.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var items = await service.GetTeams();

                foreach (var item in items)
                {
                    // Team should have defined Id
                    Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"), item.TeamId);
                    // Date should be equal to today
                    var today = DateTime.Now;
                    Assert.Equal(today.Date, item.CreatedAt.Date);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_GetMyTeams()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetMyTeams").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {


                //Clear database
                context.Database.EnsureDeleted();

                context.Users.Add(user1);
                context.SaveChanges();

                output.WriteLine(user1.Id);
                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateTeam(team1);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.CreateTeam(team2);
                await service.CreateTeam(team3);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Team.CountAsync();

                // Verify amount of teams
                Assert.Equal(3, itemsInDatabase);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var items = await service.GetMyTeams();
                Assert.Equal(1, items.Count);

                // Verify amount of teams

                foreach (var item in items)
                {
                    output.WriteLine("OwnerId: " + item.OwnerId);
                    // Team should have defined Id
                    Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"), item.TeamId);

                    // Date should be equal to today
                    var today = DateTime.Now;
                    Assert.Equal(today.Date, item.CreatedAt.Date);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task TeamServiceTest_GetTeamById()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamServiceTest_GetTeamById").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new TeamsService(context,
                                               userManagerMock.Object,
                                               httpContextAccessor,
                                               userServiceMock.Object,
                                               ratingServiceMock.Object);


                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Team.Add(team1);
                context.Team.Add(team2);
                context.SaveChanges();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TeamsService(context,
                                               userManagerMock.Object,
                                               httpContextAccessor,
                                               userServiceMock.Object,
                                               ratingServiceMock.Object);
                var items = context.Team.ToList();
                // Verify amount of teams
                Assert.Equal(2, items.Count);

                var item = await service.GetTeamById(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"));

                // Verify if null
                Assert.NotNull(item);

                // Verify id and team name
                Assert.Equal(team1.TeamId, item.TeamId);
                Assert.Equal(team1.TeamName, item.TeamName);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_UpdateTeam()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_CreateTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Team.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                team1.TeamName = "Teste AAAA";

                await service.UpdateTeam(team1);


                var item = await context.Team.FirstAsync();

                // Team should have defined Id
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"), item.TeamId);

                // Team should have new name
                Assert.Equal("Teste AAAA", item.TeamName);

                // Date should be equal to today
                var today = DateTime.Now;
                Assert.Equal(today.Date, item.CreatedAt.Date);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_ChangeTeamStatus()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ChangeTeamStatus").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Team.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                bool updated = await service.ChangeTeamStatus(TeamStatus.Closed, team1.TeamId);

                // Verify if updated
                //Assert.True(updated);

                var item = await context.Team.FirstAsync();

                // Team should have defined Id
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"), item.TeamId);

                // Team should have new status
                Assert.Equal(TeamStatus.Closed, item.Status);

                // Date should be equal to today
                var today = DateTime.Now;
                Assert.Equal(today.Date, item.CreatedAt.Date);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AddUser()
        {
            //Arrange
            SetUp();
            bool added = false;
            bool added2 = true;
            bool added3 = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AddUser").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(user1.Id)).ReturnsAsync(user1);
                userManagerMock.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                added = await service.AddUser(team1.TeamId, new Guid(user2.Id));
                added2 = await service.AddUser(team1.TeamId, new Guid(user1.Id));
                added3 = await service.AddUser(team1.TeamId, new Guid(user2.Id));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if added = true
                Assert.True(added);
                Assert.False(added2);
                Assert.False(added3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(1, t1.Members.Count);
                Assert.Contains(user2.Id, t1.Members.First().Id);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_RemoveUser()
        {
            //Arrange
            SetUp();
            bool removed = false;
            bool removed2 = true;
            bool removed3 = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_RemoveUser").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(user1.Id)).ReturnsAsync(user1);
                userManagerMock.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                await service.CreateTeam(team1);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                await service.AddUser(team1.TeamId, new Guid(user2.Id));

                removed = await service.RemoveUser(team1.TeamId, new Guid(user2.Id));
                removed2 = await service.RemoveUser(team1.TeamId, new Guid(user1.Id));
                removed3 = await service.RemoveUser(team1.TeamId, new Guid(user2.Id));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.True(removed);
                Assert.False(removed2);
                Assert.False(removed3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AskAccess()
        {
            //Arrange
            SetUp();
            bool access = false;
            bool access2 = true;
            bool access3 = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AskAccess").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();


                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                access = await service.AskAccess(team1.TeamId);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                access2 = await service.AskAccess(team1.TeamId);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                access3 = await service.AskAccess(team1.TeamId);


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.True(access);
                Assert.False(access2);
                Assert.True(access3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AcceptAccess()
        {
            //Arrange
            SetUp();
            bool acceptNotOwner = true;
            bool acceptOwner = false;
            bool acceptAlreadyAccepted = true;
            bool acceptNotExist = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AcceptAccess").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.PendingList.Add(
                    new PendingUserTeam()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                acceptNotOwner = await service.AcceptAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                acceptOwner = await service.AcceptAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                acceptAlreadyAccepted = await service.AcceptAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                acceptNotExist = await service.AcceptAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.False(acceptNotOwner);
                Assert.True(acceptOwner);
                Assert.False(acceptAlreadyAccepted);
                Assert.False(acceptNotExist);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(1, t1.Members.Count);


                //Verify user Id
                Assert.Equal(user2.Id, t1.Members.First().Id);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_RejectAccess()
        {
            //Arrange
            SetUp();
            bool rejectNotOwner = true;
            bool rejectOwner = false;
            bool rejectAlreadyAccepted = true;
            bool rejectNotExist = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_RejectAccess").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.PendingList.Add(
                    new PendingUserTeam()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                rejectNotOwner = await service.RejectAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rejectOwner = await service.RejectAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rejectAlreadyAccepted = await service.RejectAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rejectNotExist = await service.RejectAccess(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.False(rejectNotOwner);
                Assert.True(rejectOwner);
                Assert.False(rejectAlreadyAccepted);
                Assert.False(rejectNotExist);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_LeaveTeam()
        {
            //Arrange
            SetUp();
            bool leftNotMember = true;
            bool leftMember = false;
            bool leftAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_LeaveTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.Members.Add(user2);

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                leftNotMember = await service.LeaveTeam(team1.TeamId);

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                leftMember = await service.LeaveTeam(team1.TeamId);

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                leftAgain = await service.LeaveTeam(team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.False(leftNotMember);
                Assert.True(leftMember);
                Assert.False(leftAgain);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_InviteUserToTeam()
        {
            //Arrange
            SetUp();
            bool InviteNotMember = true;
            bool InviteOwner = false;
            bool InviteAgainOwner = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_TeamsService_InviteUserToTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);

                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateTeam(team1);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                InviteNotMember = await service.InviteUserToTeam(team1.TeamId, new Guid(user2.Id));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                InviteOwner = await service.InviteUserToTeam(team1.TeamId, new Guid(user2.Id));

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                InviteAgainOwner = await service.InviteUserToTeam(team1.TeamId, new Guid(user2.Id));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert if removed = true
                Assert.True(InviteOwner);
                Assert.False(InviteNotMember);
                Assert.False(InviteAgainOwner);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AcceptInvite()
        {
            //Arrange
            SetUp();
            bool acceptedNotUser = true;
            bool accepted = false;
            bool acceptedAlreadyAccepted = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AcceptInvite").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.InvitedUsers.Add(
                    new TeamInviteUser()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                acceptedNotUser = await service.AcceptInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                accepted = await service.AcceptInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
                acceptedAlreadyAccepted = await service.AcceptInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.False(acceptedNotUser);
                Assert.True(accepted);
                Assert.False(acceptedAlreadyAccepted);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(1, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AcceptInviteByTeam()
        {
            //Arrange
            SetUp();
            bool acceptedNotUser = true;
            bool accepted = false;
            bool acceptedAlreadyAccepted = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AcceptInviteByTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);
                team1.InvitedUsers.Add(
                    new TeamInviteUser()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                acceptedNotUser = await service.AcceptInviteByTeam(team1.TeamId);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                accepted = await service.AcceptInviteByTeam(team1.TeamId);
                acceptedAlreadyAccepted = await service.AcceptInviteByTeam(team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.False(acceptedNotUser);
                Assert.True(accepted);
                Assert.False(acceptedAlreadyAccepted);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(1, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_RejectInvite()
        {
            //Arrange
            SetUp();
            bool rejectedNotUser = true;
            bool rejected = false;
            bool rejecetedAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_RejectInvite").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);
                team1.InvitedUsers.Add(
                    new TeamInviteUser()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rejectedNotUser = await service.RejectInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                rejected = await service.RejectInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
                rejecetedAgain = await service.RejectInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.False(rejectedNotUser);
                Assert.True(rejected);
                Assert.False(rejecetedAgain);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_RejectInviteByTeam()
        {
            //Arrange
            SetUp();
            bool rejectedNotUser = true;
            bool rejected = false;
            bool rejecetedAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_RejectInviteByTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);
                team1.InvitedUsers.Add(
                    new TeamInviteUser()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();



                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rejectedNotUser = await service.RejectInviteByTeam(team1.TeamId);

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                rejected = await service.RejectInviteByTeam(team1.TeamId);
                rejecetedAgain = await service.RejectInviteByTeam(team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.False(rejectedNotUser);
                Assert.True(rejected);
                Assert.False(rejecetedAgain);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await service.GetTeams();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                Team t1 = itemsInDatabase.First();
                Assert.Equal(0, t1.Members.Count);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_CancelInvite()
        {
            //Arrange
            SetUp();
            bool cancelNotOwner = true;
            bool cancel = false;
            bool cancelAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_CancelInvite").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);
                team1.InvitedUsers.Add(
                    new TeamInviteUser()
                    {
                        Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                        TeamId = team1.TeamId,
                        UserId = user2.Id,
                    });

                context.Team.Add(team1);
                context.SaveChanges();



                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                cancelNotOwner = await service.CancelInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                cancel = await service.CancelInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
                cancelAgain = await service.CancelInvite(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.False(cancelNotOwner);
                Assert.True(cancel);
                Assert.False(cancelAgain);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.TeamsRequests.ToListAsync();

                // Verify amount of teams
                Assert.Equal(0, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_GetTags()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetTags").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.Skills = new List<Tag>()
                {
                    new Tag()
                    {
                        TagId = new Guid(),
                        TagName = "1",
                        TeamId = team2.TeamId
                    },
                    new Tag()
                    {
                        TagId = new Guid(),
                        TagName = "2",
                        TeamId = team2.TeamId
                    }
                };

                context.Team.Add(team1);
                context.SaveChanges();



                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                tags = await service.GetTags();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.Equal(2, tags.Count);

                var r1 = tags.Where(x => x.TagName == "1").First();
                var r2 = tags.Where(x => x.TagName == "2").First();

                Assert.NotNull(r1);
                Assert.NotNull(r2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Tags.ToListAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_CreateTag()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_CreateTag").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);

                team1.Skills = new List<Tag>()
                {
                    new Tag()
                    {
                        TagId = new Guid(),
                        TagName = "1",
                        TeamId = team2.TeamId
                    }
                };

                context.Team.Add(team1);
                context.SaveChanges();



                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateTag(new Tag()
                {
                    TagId = new Guid(),
                    TagName = "2",
                    TeamId = team2.TeamId
                });
                tags = await service.GetTags();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.Equal(2, tags.Count);

                var r1 = tags.Where(x => x.TagName == "1").First();
                var r2 = tags.Where(x => x.TagName == "2").First();

                Assert.NotNull(r1);
                Assert.NotNull(r2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Tags.ToListAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AddMilestone()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();
            bool added = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AddMilestone").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);


                context.Team.Add(team1);
                context.SaveChanges();



                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                added = await service.AddMilestone(new Milestone()
                {
                    MilestoneId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    Name = "milestone",
                    Description = "Description",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    State = MilestoneState.Active,
                    TeamId = team1.TeamId
                },
                team1.TeamId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.True(added);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Milestones.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);


                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), itemsInDatabase.First().MilestoneId);


                Team t1 = await service.GetTeamById(team1.TeamId);
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), t1.Milestones.First().MilestoneId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_UpdateMilestone()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();
            bool updated = false;
            bool updatedMilestoneNotExist = true;
            bool updatedTeamNotExist = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_UpdateMilestone").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);


                context.Team.Add(team1);
                context.SaveChanges();

                Milestone milestone = new Milestone()
                {
                    MilestoneId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    Name = "milestone",
                    Description = "Description",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    State = MilestoneState.Active,
                    TeamId = team1.TeamId
                };

                Milestone milestoneWithDifferentId = new Milestone()
                {
                    MilestoneId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3587"),
                    Name = "milestone",
                    Description = "Description",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    State = MilestoneState.Active,
                    TeamId = team1.TeamId
                };

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                await service.AddMilestone(milestone, team1.TeamId);

                milestone.Name = "Altered Name";

                updated = await service.UpdateMilestone(milestone, team1.TeamId);

                updatedMilestoneNotExist = await service.UpdateMilestone(milestoneWithDifferentId, team1.TeamId);
                updatedTeamNotExist = await service.UpdateMilestone(milestone, new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3587"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.True(updated);
                Assert.False(updatedMilestoneNotExist);
                Assert.False(updatedTeamNotExist);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Milestones.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);


                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), itemsInDatabase.First().MilestoneId);
                Assert.Equal("Altered Name", itemsInDatabase.First().Name);


                Team t1 = await service.GetTeamById(team1.TeamId);
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), t1.Milestones.First().MilestoneId);
                Assert.Equal("Altered Name", t1.Milestones.First().Name);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_DeleteMilestone()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();
            bool deleted = false;
            bool deletedMilestoneNotExist = true;
            bool deletedTeamNotExist = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_DeleteMilestone").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);


                context.Team.Add(team1);
                context.SaveChanges();

                Milestone milestone = new Milestone()
                {
                    MilestoneId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    Name = "milestone",
                    Description = "Description",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    State = MilestoneState.Active,
                    TeamId = team1.TeamId
                };

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                await service.AddMilestone(milestone, team1.TeamId);


                deleted = await service.DeleteMilestone(milestone.MilestoneId, team1.TeamId);

                deletedMilestoneNotExist = await service.DeleteMilestone(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3585"), team1.TeamId);
                deletedTeamNotExist = await service.DeleteMilestone(milestone.MilestoneId, new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3585"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.True(deleted);
                Assert.False(deletedMilestoneNotExist);
                Assert.False(deletedTeamNotExist);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Milestones.ToListAsync();

                // Verify amount of teams
                Assert.Equal(0, itemsInDatabase.Count);

                Team t1 = await service.GetTeamById(team1.TeamId);

                Assert.Equal(0, t1.Milestones.Count);
                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_ChangeMilestoneStatus()
        {
            //Arrange
            SetUp();
            ICollection<Tag> tags = new List<Tag>();
            bool changed = false;
            bool changedMilestoneNotExist = true;
            bool changedTeamNotExist = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_ChangeMilestoneStatus").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();



                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);


                context.Team.Add(team1);
                context.SaveChanges();

                Milestone milestone = new Milestone()
                {
                    MilestoneId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    Name = "milestone",
                    Description = "Description",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    State = MilestoneState.Active,
                    TeamId = team1.TeamId
                };

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                await service.AddMilestone(milestone, team1.TeamId);


                changed = await service.ChangeMilestoneStatus(milestone.MilestoneId, team1.TeamId, MilestoneState.Completed);
                changedMilestoneNotExist = await service.ChangeMilestoneStatus(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3585"), team1.TeamId, MilestoneState.Completed);
                changedTeamNotExist = await service.ChangeMilestoneStatus(milestone.MilestoneId, new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3585"), MilestoneState.Completed);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.True(changed);
                Assert.False(changedMilestoneNotExist);
                Assert.False(changedTeamNotExist);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.Milestones.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);


                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), itemsInDatabase.First().MilestoneId);
                Assert.Equal(MilestoneState.Completed, itemsInDatabase.First().State);


                Team t1 = await service.GetTeamById(team1.TeamId);
                Assert.Equal(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"), t1.Milestones.First().MilestoneId);
                Assert.Equal(MilestoneState.Completed, t1.Milestones.First().State);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task TeamsService_GetRequestById()
        {
            //Arrange
            SetUp();

            PendingUserTeam requestExists = null;
            PendingUserTeam requestNotExists = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetRequestById").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);

                PendingUserTeam request = new PendingUserTeam()
                {
                    Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    TeamId = team1.TeamId,
                    UserId = user2.Id,
                    Status = PendingUserTeamStatus.Pending
                };

                team1.PendingList.Add(request);
                    

                context.Team.Add(team1);
                context.SaveChanges();

                requestExists = await service.GetRequestById(request.Id);
                requestNotExists = await service.GetRequestById(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3589"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.NotNull(requestExists);
                Assert.Null(requestNotExists);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.PendingUsers.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_GetInviteById()
        {
            //Arrange
            SetUp();

            TeamInviteUser requestExists = null;
            TeamInviteUser requestNotExists = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetInviteById").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);

                TeamInviteUser request = new TeamInviteUser()
                {
                    Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    TeamId = team1.TeamId,
                    UserId = user2.Id,
                    Status = PendingUserTeamStatus.Pending
                };

                team1.InvitedUsers.Add(request);


                context.Team.Add(team1);
                context.SaveChanges();

                requestExists = await service.GetInviteById(request.Id);
                requestNotExists = await service.GetInviteById(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3589"));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.NotNull(requestExists);
                Assert.Null(requestNotExists);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.TeamsRequests.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_GetInviteByTeam()
        {
            //Arrange
            SetUp();

            TeamInviteUser requestExists = null;
            TeamInviteUser requestUserNotExists = null;
            TeamInviteUser requestTeamNotExists = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetInviteByTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                context.Users.Add(user1);
                context.Users.Add(user2);

                TeamInviteUser request = new TeamInviteUser()
                {
                    Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3581"),
                    TeamId = team1.TeamId,
                    UserId = user2.Id,
                    Status = PendingUserTeamStatus.Pending
                };

                team1.InvitedUsers.Add(request);


                context.Team.Add(team1);
                context.SaveChanges();

                requestExists = await service.GetInviteByTeam(team1.TeamId, user2.Id);
                requestUserNotExists = await service.GetInviteByTeam(team1.TeamId, "fea8c61b-d7e7-447b-9b5e-1ef5bf5b3589");
                requestTeamNotExists = await service.GetInviteByTeam(new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3589"), user2.Id);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.NotNull(requestExists);
                Assert.Null(requestUserNotExists);
                Assert.Null(requestTeamNotExists);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                var itemsInDatabase = await context.TeamsRequests.ToListAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task TeamsService_AreTeammates()
        {

            //Arrange
            SetUp();

            bool teammatesOwner = false;
            bool teammatesYes = false;
            bool teammatesNo = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_AreTeamMates").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user3.Id))).ReturnsAsync(user3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Users.Add(user3);

                team1.Members.Add(user2);
                team1.Members.Add(user3);

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                teammatesYes = await service.AreTeammates(new Guid(user3.Id));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                teammatesOwner = await service.AreTeammates(new Guid(user3.Id));

                await service.RemoveUser(team1.TeamId, new Guid(user3.Id));

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                teammatesNo = await service.AreTeammates(new Guid(user3.Id));
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                //Assert 
                Assert.True(teammatesYes);
                Assert.True(teammatesOwner);
                Assert.False(teammatesNo);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async Task TeamsService_GiveOwnership()
        {

            //Arrange
            SetUp();

            bool giveOwnershipFromOwnerToMember = false;
            bool giveOwnershipFromMemberToMember = true;
            bool giveOwnershipFromUnknownToMember = true;
            bool giveOwnershipFromMemberToUnknown = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GiveOwnership").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user3.Id))).ReturnsAsync(user3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);


                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Users.Add(user3);

                team1.Members.Add(user2);

                context.Team.Add(team1);
                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                giveOwnershipFromMemberToMember = await service.GiveOwnership(team1.TeamId, new Guid(user2.Id));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user3);
                giveOwnershipFromUnknownToMember = await service.GiveOwnership(team1.TeamId, new Guid(user2.Id));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                giveOwnershipFromMemberToUnknown = await service.GiveOwnership(team1.TeamId, new Guid(user3.Id));


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                giveOwnershipFromOwnerToMember = await service.GiveOwnership(team1.TeamId, new Guid(user2.Id));

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                //Assert 
                Assert.True(giveOwnershipFromOwnerToMember);
                Assert.False(giveOwnershipFromMemberToMember);
                Assert.False(giveOwnershipFromUnknownToMember);
                Assert.False(giveOwnershipFromMemberToUnknown);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                Team t1 = await service.GetTeamById(team1.TeamId);

                Assert.Equal(new Guid(user2.Id), t1.OwnerId);
                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async Task TeamsService_GetAverageRating()
        {

            //Arrange
            SetUp();

            double rating1 = 0.0;
            double rating2 = 0.0;
            double rating3 = 0.0;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetAverageRating").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user3.Id))).ReturnsAsync(user3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Users.Add(user3);

                team1.Members.Add(user2);

                context.Team.Add(team1);
                context.SaveChanges();

                ratingServiceMock.Setup(x => x.IsRated(new Guid(user1.Id))).Returns(false);
                ratingServiceMock.Setup(x => x.IsRated(new Guid(user2.Id))).Returns(false);
                rating1 = await service.GetAverageRating(team1.TeamId);

                ratingServiceMock.Setup(x => x.IsRated(new Guid(user1.Id))).Returns(true);
                ratingServiceMock.Setup(x => x.IsRated(new Guid(user2.Id))).Returns(false);
                ratingServiceMock.Setup(x => x.GetTrueAverageRating(new Guid(user1.Id))).ReturnsAsync(4.0);
                rating2 = await service.GetAverageRating(team1.TeamId);


                ratingServiceMock.Setup(x => x.IsRated(new Guid(user1.Id))).Returns(true);
                ratingServiceMock.Setup(x => x.IsRated(new Guid(user2.Id))).Returns(true);
                ratingServiceMock.Setup(x => x.GetTrueAverageRating(new Guid(user1.Id))).ReturnsAsync(4.0);
                ratingServiceMock.Setup(x => x.GetTrueAverageRating(new Guid(user2.Id))).ReturnsAsync(2.0);
                rating3 = await service.GetAverageRating(team1.TeamId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.Equal(0.0, rating1);
                Assert.Equal(2.0, rating2);
                Assert.Equal(3.0, rating3);
                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async Task TeamsService_GetOpenNewTeams()
        {

            //Arrange
            SetUp();

            ICollection<Team> list1 = new List<Team>();
            ICollection<Team> list2 = new List<Team>();
            ICollection<Team> list3 = new List<Team>();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_TeamsService_GetOpenNewTeams").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userServiceMock.Setup(x => x.GetUser(new Guid(user1.Id))).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(new Guid(user2.Id))).ReturnsAsync(user2);
                userServiceMock.Setup(x => x.GetUser(new Guid(user3.Id))).ReturnsAsync(user3);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);



                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Users.Add(user3);


                context.Team.Add(team1);

                team2.Status = TeamStatus.Closed;
                context.Team.Add(team2);

                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                list1 = await service.GetOpenNewTeams();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                list2 = await service.GetOpenNewTeams();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user3);
                list3 = await service.GetOpenNewTeams();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert 
                Assert.Equal(0, list1.Count);
                Assert.Equal(1, list2.Count);
                Assert.Equal(1, list3.Count);

                Assert.Equal(team1.TeamId, list2.First().TeamId);
                Assert.Equal(team1.TeamId, list3.First().TeamId);

                var service = new TeamsService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, ratingServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

    }
}

    

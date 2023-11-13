using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NToastNotify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Admin.Logs;
using Workio.Models.Events;
using Workio.Services.Admin;
using Workio.Services.Admin.Log;
using Xunit.Sdk;

namespace Workio.Tests.Services
{
    public class LogsServiceTest
    {

        private User user1;
        private User user2;

        private Event e1;

        private Team team1;

        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;

        private void SetUp()
        {
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
                Name = "Teste user 2",
                EmailConfirmed = true

            };


            // Events
            e1 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3590"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserPublisher = user1,
                Title = "Event 1",
                Description = "Description",
                IsBanned = false,
                IsInPerson = false,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
            };

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


        }



        [Fact]
        public async void LogsService_CreateAdminActionLog()
        {
            //Arrange
            bool created = false;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_CreateAdminActionLog").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateAdminActionLog("AA", Models.Admin.Logs.AdministrationActionLogType.Other);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if true
                Assert.NotNull(created);

                var itemsInDatabase = await context.AdministrationActionLogs.ToListAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase.Count);
                Assert.Equal("AA", itemsInDatabase.First().ActionDescription);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_CreateEventActionLog()
        {
            //Arrange
            bool created = false;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_CreateEventActionLog").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Team.Add(team1);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateEventActionLog("AA", e1.EventId.ToString() ,Models.Admin.Logs.EventActionLogType.Other);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if true
                Assert.NotNull(created);

                var itemsInDatabase = await context.EventActionLogs.ToListAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase.Count);
                Assert.Equal("AA", itemsInDatabase.First().ActionDescription);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_CreateSystemActionLog()
        {
            //Arrange
            bool created = false;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_CreateSystemActionLog").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Team.Add(team1);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateSystemLog("AA");
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if true
                Assert.NotNull(created);

                var itemsInDatabase = await context.SystemLog.ToListAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase.Count);
                Assert.Equal("AA", itemsInDatabase.First().Description);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_CreateTeamActionLog()
        {
            //Arrange
            bool created = false;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_CreateTeamActionLog").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Team.Add(team1);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateTeamActionLog("AA", team1.TeamId.ToString(), Models.Admin.Logs.TeamActionLogType.Other);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if true
                Assert.NotNull(created);

                var itemsInDatabase = await context.TeamActionLogs.ToListAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase.Count);
                Assert.Equal("AA", itemsInDatabase.First().ActionDescription);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_CreateUserActionLog()
        {
            //Arrange
            bool created = false;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_CreateUserActionLog").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Team.Add(team1);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                created = await service.CreateUserActionLog("AA", user2.Id, Models.Admin.Logs.UserActionLogType.Other);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if true
                Assert.NotNull(created);

                var itemsInDatabase = await context.UserActionLog.ToListAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase.Count);
                Assert.Equal("AA", itemsInDatabase.First().ActionDescription);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetUserActionLogData()
        {
            //Arrange
            List<UserActionLog> logs = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_GetUserActionLogData").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);


                await service.CreateUserActionLog("AA", user2.Id, Models.Admin.Logs.UserActionLogType.Other);
                await service.CreateUserActionLog("AB", user2.Id, Models.Admin.Logs.UserActionLogType.Other);

                logs = await service.GetUserActionLogData();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(logs);

                //Verify amount
                Assert.Equal(2, logs.Count());

                //Veriffy both teams
                var r1 = logs.Where(x => x.ActionDescription == "AA").First();
                var r2 = logs.Where(x => x.ActionDescription == "AB").First();

                Assert.Equal("AA", r1.ActionDescription);
                Assert.Equal("AB", r2.ActionDescription);

                var itemsInDatabase = await context.UserActionLog.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetUserActionLogByLogId()
        {
            //Arrange
            UserActionLog log = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetUserActionLogByLogId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);


                await service.CreateUserActionLog("AA", user2.Id, Models.Admin.Logs.UserActionLogType.Other);


                var itemsInDatabase = await context.UserActionLog.ToListAsync();
                var logID = itemsInDatabase.First().LogId;

                log = await service.GetUserActionLogByLogId(logID.ToString());
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(log);

                //Verify amount

                //Veriffy log
                Assert.Equal("AA", log.ActionDescription);

                var itemsInDatabase = await context.UserActionLog.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetTeamActionLogByLogId()
        {
            //Arrange
            TeamActionLog log = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetTeamActionLogByLogId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);


                await service.CreateTeamActionLog("AA", team1.TeamId.ToString(), Models.Admin.Logs.TeamActionLogType.Other);


                var itemsInDatabase = await context.TeamActionLogs.ToListAsync();
                var logID = itemsInDatabase.First().LogId;

                log = await service.GetTeamActionLogByLogId(logID.ToString());
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(log);

                //Verify amount

                //Veriffy log
                Assert.Equal("AA", log.ActionDescription);

                var itemsInDatabase = await context.TeamActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetEventActionLogByLogId()
        {
            //Arrange
            EventActionLog log = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetEventActionLogByLogId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);


                await service.CreateEventActionLog("AA", e1.EventId.ToString(), Models.Admin.Logs.EventActionLogType.Other);


                var itemsInDatabase = await context.EventActionLogs.ToListAsync();
                var logID = itemsInDatabase.First().LogId;

                log = await service.GetEventActionLogByLogId(logID.ToString());
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(log);

                //Verify amount

                //Veriffy log
                Assert.Equal("AA", log.ActionDescription);

                var itemsInDatabase = await context.EventActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetAdministrationActionLogByLogId()
        {
            //Arrange
            AdministrationActionLog log = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetAdministrationActionLogByLogId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);


                await service.CreateAdminActionLog("AA", Models.Admin.Logs.AdministrationActionLogType.Other);


                var itemsInDatabase = await context.AdministrationActionLogs.ToListAsync();
                var logID = itemsInDatabase.First().LogId;

                log = await service.GetAdministrationActionLogByLogId(logID.ToString());
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(log);

                //Verify amount

                //Veriffy log
                Assert.Equal("AA", log.ActionDescription);

                var itemsInDatabase = await context.AdministrationActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(1, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetAdminActionLogDataFiltered()
        {
            //Arrange
            IEnumerable<object> values = new List<object>();

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetAdminActionLogDataFiltered").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();

                // Do report with user 1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateAdminActionLog("AA", AdministrationActionLogType.Other);

                // Do report with user 2
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.CreateAdminActionLog("BB", AdministrationActionLogType.Other);



                values = await service.GetAdminActionLogDataFiltered("1");
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(values);

                //Verify amount
                Assert.Equal(1, values.Count());


                var itemsInDatabase = await context.AdministrationActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetEventActionLogDataFiltered()
        {
            //Arrange
            IEnumerable<object> values = new List<object>();

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetEventActionLogDataFiltered").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();

                // Do report with user 1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateEventActionLog("AA", e1.EventId.ToString(), EventActionLogType.Other);

                // Do report with user 2
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.CreateEventActionLog("BB", e1.EventId.ToString(), EventActionLogType.Other);



                values = await service.GetEventActionLogDataFiltered("1");
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(values);

                //Verify amount
                Assert.Equal(1, values.Count());


                var itemsInDatabase = await context.EventActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetTeamActionLogDataFiltered()
        {
            //Arrange
            IEnumerable<object> values = new List<object>();

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetTeamActionLogDataFiltered").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();

                // Do report with user 1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateTeamActionLog("AA", e1.EventId.ToString(), TeamActionLogType.Other);

                // Do report with user 2
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.CreateTeamActionLog("BB", e1.EventId.ToString(), TeamActionLogType.Other);



                values = await service.GetTeamActionLogDataFiltered("1");
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(values);

                //Verify amount
                Assert.Equal(1, values.Count());


                var itemsInDatabase = await context.TeamActionLogs.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void LogsService_GetUserActionLogDataFiltered()
        {
            //Arrange
            IEnumerable<object> values = new List<object>();

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_LogsService_LogsService_GetUserActionLogDataFiltered").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new LogsService(context, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                context.SaveChanges();

                // Do report with user 1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                await service.CreateUserActionLog("AA", user1.Id, UserActionLogType.Other);

                // Do report with user 2
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.CreateUserActionLog("BB", user2.Id, UserActionLogType.Other);



                values = await service.GetUserActionDataFiltered("1");
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(values);

                //Verify amount
                Assert.Equal(1, values.Count());


                var itemsInDatabase = await context.UserActionLog.ToListAsync();

                // Verification of logs ammounts
                Assert.Equal(2, itemsInDatabase.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }
    }
}

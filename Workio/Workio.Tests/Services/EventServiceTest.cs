using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class EventServiceTest
    {
        private User user1;
        private User user2;

        private Event e1;
        private Event e2;

        private Team team1;


        private readonly ITestOutputHelper output;


        private Mock<UserManager<User>> userManagerMock;
        private Mock<IHttpContextAccessor> httpMock;
        private Mock<IUserService> userServiceMock;
        private HttpContextAccessor httpContextAccessor;

        public EventServiceTest(ITestOutputHelper output)
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
                Name = "Teste user",
                EmailConfirmed = true,

            };

            user2 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Teste user",
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

            e2 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3591"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserPublisher = user2,
                Title = "Event 2",
                Description = "Description",
                IsBanned = false,
                IsInPerson = false,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
            };

            // Teams
            team1 = new Team
            {
                TeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"),
                OwnerId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                TeamName = "TeamName",
                Description = "Description",
                Status = TeamStatus.Open,
                Positions = new List<Position>(),
                Members = new List<User>(),
                InvitedUsers = new List<TeamInviteUser>(),
                PendingList = new List<PendingUserTeam>(),
                Skills = new List<Tag>()
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

            userServiceMock = new Mock<IUserService>();

        }



        [Fact]
        public async Task EventService_CreateEvent()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_CreateEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await context.Event.ToListAsync();
                foreach(var item in items)
                {
                    Assert.Equal(e1.EventId, item.EventId);
                    Assert.Equal(e1.Title, item.Title);
                    Assert.Equal(e1.UserId, item.UserId);
                }

                

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetEvents()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetEvents").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
                created = await service.CreateEvent(e2);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var items = await context.Event.ToListAsync();


                var r1 = items.Where(x => x.EventId == e1.EventId).First();
                var r2 = items.Where(x => x.EventId == e2.EventId).First();

                Assert.Equal(e1.EventId, r1.EventId);
                Assert.Equal(e2.EventId, r2.EventId);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetEvent()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var item = await service.GetEvent(e1.EventId);

                Assert.NotNull(item);
                Assert.Equal(e1.EventId,item.EventId);




                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetFeaturedEvents()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetFeaturedEvents").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;
                created = await service.CreateEvent(e1);
                created = await service.CreateEvent(e2);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetFeaturedEvents();

                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.EventId == e1.EventId).First();

                Assert.Equal(e1.EventId, r1.EventId);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetTopEvents()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetTopEvents").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;
                e1.EventReactions = new List<EventReactions>()
                {
                    new EventReactions()
                    {
                        ReactionId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9701"),
                        UserId = new Guid(user1.Id),
                        EventId = e1.EventId,
                        ReactionType = EventReactionType.UpVote
                    }
                };

                created = await service.CreateEvent(e1);
                created = await service.CreateEvent(e2);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);

                // Test with only 1 top event
                var items = await service.GetTopEvents(1);

                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.EventId == e1.EventId).First();
                Assert.NotNull(r1);
                Assert.Equal(e1.EventId, r1.EventId);


                // Test selecting 2 top events
                items = await service.GetTopEvents(2);

                Assert.Equal(2, items.Count);

                r1 = items.Where(x => x.EventId == e1.EventId).First();
                var r2 = items.Where(x => x.EventId == e2.EventId).First();
                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.Equal(e1.EventId, r1.EventId);
                Assert.Equal(e2.EventId, r2.EventId);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetSoonEvents()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetSoonEvents").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;
                e1.EventReactions = new List<EventReactions>()
                {
                    new EventReactions()
                    {
                        ReactionId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9701"),
                        UserId = new Guid(user1.Id),
                        EventId = e1.EventId,
                        ReactionType = EventReactionType.UpVote
                    }
                };

                // Adicionar 2 dias a start e end date do e1
                e1.StartDate = e1.StartDate.AddDays(2);
                e1.EndDate = e1.EndDate.AddDays(2);
                created = await service.CreateEvent(e1);


                // Adicionar 3 dias a start e end date do e2
                e2.StartDate = e2.StartDate.AddDays(3);
                e2.EndDate = e2.EndDate.AddDays(3);
                created = await service.CreateEvent(e2);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);

                // Test with only 1 soon event with a time limit of 2 day
                var items = await service.GetSoonEvents(2, 1);

                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.EventId == e1.EventId).First();
                Assert.NotNull(r1);
                Assert.Equal(e1.EventId, r1.EventId);


                // Test with only 1 soon event with a time limit of 2 day
                items = await service.GetSoonEvents(3, 1);

                Assert.Equal(1, items.Count);

                r1 = items.Where(x => x.EventId == e1.EventId).First();
                Assert.NotNull(r1);
                Assert.Equal(e1.EventId, r1.EventId);

                // Test with only 2 soon event with a time limit of 3 day
                items = await service.GetSoonEvents(3, 2);

                Assert.Equal(2, items.Count);

                r1 = items.Where(x => x.EventId == e1.EventId).First();
                var r2 = items.Where(x => x.EventId == e2.EventId).First();
                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.Equal(e1.EventId, r1.EventId);
                Assert.Equal(e2.EventId, r2.EventId);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_AddInterestedUser()
        {
            //Arrange
            SetUp();
            bool interested = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_AddInterestedUser").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                interested = await service.AddInterestedUser(e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(interested);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var e = await service.GetEvent(e1.EventId);
                var r1 = e.InterestedUsers.Where(x => x.User.Id == user2.Id).First();
                Assert.NotNull(r1);
                Assert.Equal(r1.User.Id, user2.Id); 
                


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_IsUserInterested()
        {
            //Arrange
            SetUp();
            bool isUserIntereted = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_IsUserInterested").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.AddInterestedUser(e1.EventId);

                isUserIntereted = await service.IsUserInterested(e1.EventId);   

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(isUserIntereted);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_RemoveInterestedUser()
        {
            //Arrange
            SetUp();
            bool addedInterest = false;
            bool isUserIntereted = false;
            bool removedInterest = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_IsUserInterested").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);

                addedInterest = await service.AddInterestedUser(e1.EventId);

                isUserIntereted = await service.IsUserInterested(e1.EventId);

                removedInterest = await service.RemoveInterestedUser(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(addedInterest);
                Assert.True(isUserIntereted);
                Assert.True(removedInterest);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var e = await service.GetEvent(e1.EventId);
                Assert.NotNull(e);
                Assert.Equal(0, e.InterestedUsers.Count());

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_AddInterestedTeam()
        {
            //Arrange
            SetUp();
            bool interested = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_AddInterestedTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //Add team
                context.Team.Add(team1);
                context.SaveChanges();

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                interested = await service.AddInterestedTeam(team1, e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(interested);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var e = await service.GetEvent(e1.EventId);
                var r1 = e.InterestedTeams.Where(x => x.Team.TeamId == team1.TeamId).First();
                Assert.NotNull(r1);
                Assert.Equal(r1.Team.TeamId, team1.TeamId);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task EventService_IsTeamInterested()
        {
            //Arrange
            SetUp();
            bool interested = false;
            bool isInterested = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_IsTeamInterested").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //Add team
                context.Team.Add(team1);
                context.SaveChanges();

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                interested = await service.AddInterestedTeam(team1, e1.EventId);
                isInterested = await service.IsTeamInterested(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(interested);
                Assert.True(isInterested);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_RemoveInterestedTeam()
        {
            //Arrange
            SetUp();
            bool interested = false;
            bool isInterested = false;
            bool removedInterest = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_RemoveInterestedTeam").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.IsFeatured = true;

                await service.CreateEvent(e1);
                await service.CreateEvent(e2);

                //Add team
                context.Team.Add(team1);
                context.SaveChanges();

                //User2 is now interested in event1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                interested = await service.AddInterestedTeam(team1, e1.EventId);
                isInterested = await service.IsTeamInterested(e1.EventId);
                removedInterest = await service.RemoveInterestedTeam(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(interested);
                Assert.True(isInterested);
                Assert.True(removedInterest);


                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var e = await service.GetEvent(e1.EventId);
                Assert.NotNull(e);
                Assert.Equal(0, e.InterestedTeams.Count());

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_UpdateEvent()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool updated = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_UpdateEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
                e1.Title = "Altered Title";
                updated = await service.UpdateEvent(e1);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(updated);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var itemsInDatabase = await context.Event.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await context.Event.ToListAsync();
                foreach (var item in items)
                {
                    Assert.Equal(e1.EventId, item.EventId);
                    Assert.Equal("Altered Title", item.Title);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_RemoveEvent()
        {
            //Arrange
            SetUp();
            bool created = false;
            Event removedEvent = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_RemoveEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                removedEvent = await service.RemoveEvent(e1);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.NotNull(removedEvent);

                Assert.Equal(e1.EventId, removedEvent.EventId);
                Assert.Equal(e1.Title, removedEvent.Title);



                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_UpVote()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool upvoted = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_UpVote").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                upvoted = await service.UpVote(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(upvoted);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);
                int upvoteCount = item.EventReactions.Where(x => x.ReactionType == EventReactionType.UpVote).Count();
                Assert.Equal(1, upvoteCount);

                var upvoteObject = item.EventReactions.Where(x => x.ReactionType == EventReactionType.UpVote &&
                                                                  x.UserId == new Guid(user1.Id)).First();

                Assert.NotNull(upvoteObject);
                Assert.Equal(new Guid(user1.Id), upvoteObject.UserId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task EventService_Downvote()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool downvote = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_Downvote").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                downvote = await service.Downvote(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(downvote);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);
                int upvoteCount = item.EventReactions.Where(x => x.ReactionType == EventReactionType.DownVote).Count();
                Assert.Equal(1, upvoteCount);

                var downvoteObject = item.EventReactions.Where(x => x.ReactionType == EventReactionType.DownVote &&
                                                                  x.UserId == new Guid(user1.Id)).First();

                Assert.NotNull(downvoteObject);
                Assert.Equal(new Guid(user1.Id), downvoteObject.UserId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_AlreadyUpVoted()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool beforeUpvote = true;
            bool upvoted = false;
            bool afterUpvote = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_AlreadyUpVoted").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                beforeUpvote = await service.AlreadyUpvoted(e1.EventId);

                upvoted = await service.UpVote(e1.EventId);

                afterUpvote = await service.AlreadyUpvoted(e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.False(beforeUpvote);
                Assert.True(upvoted);
                Assert.True(afterUpvote);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_AlreadyDownvoted()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool beforeDownvote = true;
            bool downvoted = false;
            bool afterDownvote = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_AlreadyDownvoted").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                beforeDownvote = await service.AlreadyDownvoted(e1.EventId);

                downvoted = await service.Downvote(e1.EventId);

                afterDownvote = await service.AlreadyDownvoted(e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.False(beforeDownvote);
                Assert.True(downvoted);
                Assert.True(afterDownvote);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetNumberOfUpvotes()
        {
            //Arrange
            SetUp();
            bool created = false;
            

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetNumberOfUpvotes").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);


                await service.UpVote(e1.EventId);

                //Change User
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.UpVote(e1.EventId);


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);
                int upvoteCount = await service.GetNumberOfUpvotes(e1.EventId);
                Assert.Equal(2, upvoteCount);

                var r1 = item.EventReactions.Where(x => x.ReactionType == EventReactionType.UpVote &&
                                                                  x.UserId == new Guid(user1.Id)).First();
                Assert.NotNull(r1);
                Assert.Equal(new Guid(user1.Id), r1.UserId);




                var r2 = item.EventReactions.Where(x => x.ReactionType == EventReactionType.UpVote &&
                                                                  x.UserId == new Guid(user2.Id)).First();

                Assert.NotNull(r2);
                Assert.Equal(new Guid(user2.Id), r2.UserId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_GetNumberOfDownvotes()
        {
            //Arrange
            SetUp();
            bool created = false;


            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetNumberOfDownvotes").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);


                await service.Downvote(e1.EventId);

                //Change User
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                await service.Downvote(e1.EventId);


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);
                int downvoteCount = await service.GetNumberOfDownvotes(e1.EventId);
                Assert.Equal(2, downvoteCount);

                var r1 = item.EventReactions.Where(x => x.ReactionType == EventReactionType.DownVote &&
                                                                  x.UserId == new Guid(user1.Id)).First();
                Assert.NotNull(r1);
                Assert.Equal(new Guid(user1.Id), r1.UserId);




                var r2 = item.EventReactions.Where(x => x.ReactionType == EventReactionType.DownVote &&
                                                                  x.UserId == new Guid(user2.Id)).First();

                Assert.NotNull(r2);
                Assert.Equal(new Guid(user2.Id), r2.UserId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_isCreator()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool creator = false;
            bool creator2 = true;


            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_isCreator").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);

                //Change to user 2 to create event
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                created = await service.CreateEvent(e2);

                //Change to user 1 to test
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                creator = await service.isCreator(e1.EventId);
                creator2 = await service.isCreator(e2.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(creator);
                Assert.False(creator2);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);
                Assert.Equal(user1.Id, item.UserId);

                item = await service.GetEvent(e2.EventId);
                Assert.NotEqual(user1.Id, item.UserId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_isFinished()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool isFinished1 = false;
            bool isFinished2 = true;


            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_isFinished").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.State = EventState.Finish;
                created = await service.CreateEvent(e1);

                created = await service.CreateEvent(e2);

                isFinished1 = await service.isFinished(e1.EventId);
                isFinished2 = await service.isFinished(e2.EventId);


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(isFinished1);
                Assert.False(isFinished2);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_RemoveUpvote()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool upvoted = false;
            bool upvoteRemoved = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_RemoveUpvote").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
                upvoted = await service.UpVote(e1.EventId);
                upvoteRemoved = await service.RemoveUpvote(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(upvoted);
                Assert.True(upvoteRemoved);


                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);

                int upvoteCount = item.EventReactions.Where(x => x.ReactionType == EventReactionType.UpVote).Count();
                Assert.Equal(0, upvoteCount);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task EventService_RemoveDownvote()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool downvoted = false;
            bool downvoteRemoved = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_RemoveDownvote").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
                downvoted = await service.UpVote(e1.EventId);
                downvoteRemoved = await service.RemoveUpvote(e1.EventId);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(downvoted);
                Assert.True(downvoteRemoved);


                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);

                int upvoteCount = item.EventReactions.Where(x => x.ReactionType == EventReactionType.DownVote).Count();
                Assert.Equal(0, upvoteCount);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task EventService_ChangeEventStatus()
        {
            //Arrange
            SetUp();
            bool created = false;
            bool changed = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_ChangeEventStatus").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                created = await service.CreateEvent(e1);
                changed = await service.ChangeEventStatus(e1.EventId, EventState.OnGoing);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);
                Assert.True(changed);


                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var item = await service.GetEvent(e1.EventId);

                Assert.Equal(EventState.OnGoing, item.State);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task EventService_GetEventsBetweenDates()
        {
            //Arrange
            SetUp();
            bool created = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EventService_GetEventsBetweenDates").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                e1.StartDate = e1.StartDate.AddDays(1);
                e1.EndDate = e1.EndDate.AddDays(1);
                created = await service.CreateEvent(e1);


                e2.StartDate = e2.StartDate.AddDays(10);
                e2.EndDate = e2.EndDate.AddDays(11);
                created = await service.CreateEvent(e2);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                Assert.True(created);


                var service = new EventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object);

                var items = await service.GetEventsBetweenDates(DateTime.Now, DateTime.Now.AddDays(7));
                Assert.Equal(1, items.Count);
                Assert.Equal(e1.EventId, items.First().EventId);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

    }
}

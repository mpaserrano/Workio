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
using Workio.Models.Events;
using Workio.Services.Admin.Log;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Tests.Services
{
    public class AdminEventServiceTest
    {

        private User user1;
        private User user2;


        private Event e1;
        private Event e2;

        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;
        private Mock<IUserService> userServiceMock;
        private Mock<IEventsService> eventServiceMock;


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
            eventServiceMock = new Mock<IEventsService>();

    }

        [Fact]
        public async Task AdminEventService_GetEvents()
        {
            //Arrange
            SetUp();
            List<Event> events = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminEventService_GetEvents").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                context.Event.Add(e1);
                context.Event.Add(e2);
                context.SaveChanges();

                var rawEvents = await service.GetEvents();
                events = rawEvents.ToList();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if null
                Assert.NotNull(events);

                //Verify amount of events
                Assert.Equal(2, events.Count);

                //Veriffy both teams
                var r1 = events.Where(x => x.EventId == e1.EventId).First();
                var r2 = events.Where(x => x.EventId == e2.EventId).First();

                Assert.Equal(e1.EventId, r1.EventId);
                Assert.Equal(e2.EventId, r2.EventId);

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task AdminEventService_BanEvent()
        {
            //Arrange
            SetUp();
            bool banned = false;
            bool banAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminEventService_BanEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                context.Event.Add(e1);
                context.Event.Add(e2);
                context.SaveChanges();

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                banned = await service.BanEvent(e1.EventId);
                banAgain = await service.BanEvent(e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if banned
                Assert.True(banned);
                Assert.False(banAgain);

                //Verify state on team
                var itemsInDatabase = context.Event.ToList();
                Assert.True(itemsInDatabase.First().IsBanned);

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task AdminEventService_UnbanEvent()
        {
            //Arrange
            SetUp();
            bool unbanned = false;
            bool unbanAgain = true;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminEventService_BanEvent").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                context.Event.Add(e1);
                context.Event.Add(e2);
                context.SaveChanges();

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                await service.BanEvent(e1.EventId);
                unbanned = await service.UnbanEvent(e1.EventId);
                unbanAgain = await service.UnbanEvent(e1.EventId);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Verify if banned
                Assert.True(unbanned);
                Assert.False(unbanAgain);

                //Verify state on team
                var itemsInDatabase = context.Event.ToList();
                Assert.False(itemsInDatabase.First().IsBanned);

                var service = new AdminEventService(context, userManagerMock.Object, httpContextAccessor, userServiceMock.Object, eventServiceMock.Object);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

    }

    
}

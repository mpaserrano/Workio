using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Interfaces;
using Workio.Services.RequestEntityStatusServices;
using Workio.Services.Search;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class SearchServiceTest
    {
        private User user;
        private User user2;
        private Team team;
        private Team team2;
        private Event e;
        private Event e2;
        private readonly ITestOutputHelper output;
        private HttpContextAccessor httpContextAccessor;


        public SearchServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void SetUp()
        {
            user = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user",
                EmailConfirmed = true

            };

            user2 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Test user",
                EmailConfirmed = true
            };

            team = new Team
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

            e = new Event()
            {
                EventId= new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3590"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserPublisher = user,
                Title = "Event 1",
                Description = "Description",
                IsBanned= false,
                IsInPerson= false,
                Url = "https://www.itch.io",
                StartDate= DateTime.Now,
                EndDate= DateTime.Now,
            };

            e2 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3591"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserPublisher = user,
                Title = "Event 2",
                Description = "Description",
                IsBanned = false,
                IsInPerson = false,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
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
        }



        [Fact]
        public async void SerachService_GetUsersByEmail()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_SerachService_GetUsersByEmail").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new SearchService(context, httpContextAccessor);

                //Add to context
                context.Users.Add(user);
                context.Users.Add(user2);

                context.Team.Add(team);
                context.Team.Add(team2);

                context.Event.Add(e);
                context.Event.Add(e2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var service = new SearchService(context, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetUsersByEmail("Test@123.com");

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(1, items.Count);


                foreach(var item in items)
                {

                    // Verify user by id
                    Assert.Equal(user.Id, item.Id);
                }

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void SerachService_GetUsersByName()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_SerachService_GetUsersByName").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new SearchService(context, httpContextAccessor);

                //Add to context
                context.Users.Add(user);
                context.Users.Add(user2);

                context.Team.Add(team);
                context.Team.Add(team2);

                context.Event.Add(e);
                context.Event.Add(e2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var service = new SearchService(context, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetUsersByName("Teste");

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(1, items.Count);


                foreach (var item in items)
                {

                    // Verify user by id
                    Assert.Equal("Teste user", item.Name);
                }

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void SerachService_GetTeamsByName()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_SerachService_GetTeamsByName").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new SearchService(context, httpContextAccessor);

                //Add to context
                context.Users.Add(user);
                context.Users.Add(user2);

                context.Team.Add(team);
                context.Team.Add(team2);

                context.Event.Add(e);
                context.Event.Add(e2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var service = new SearchService(context, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetTeamsByName("TeamName2");

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(1, items.Count);


                foreach (var item in items)
                {

                    // Verify user by id
                    Assert.Equal("TeamName2", item.TeamName);
                }

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void SerachService_GetEventsByName()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_SerachService_GetEventsByName").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new SearchService(context, httpContextAccessor);

                //Add to context
                context.Users.Add(user);
                context.Users.Add(user2);

                context.Team.Add(team);
                context.Team.Add(team2);

                context.Event.Add(e);
                context.Event.Add(e2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var service = new SearchService(context, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetEventsByName("Event 1");

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(1, items.Count);


                foreach (var item in items)
                {

                    // Verify user by id
                    Assert.Equal("Event 1", item.Title);
                }

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


    }
}

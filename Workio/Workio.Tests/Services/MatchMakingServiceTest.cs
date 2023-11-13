using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.LocalizationServices;
using Workio.Services.Teams;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class MatchMakingServiceTest
    {
        //SkillModels
        private SkillModel skill1;
        private SkillModel skill2;

        //Users
        private User user1;
        private User user2;
        private User user3;

        //Teams
        private Team team1;
        private Team team2;
        private Team team3;

        //Events
        private Event e1;
        private Event e2;
        private Event e3;

        private readonly ITestOutputHelper output;


        private Mock<UserManager<User>> userManagerMock;
        private Mock<IUserService> userServiceMock;
        private Mock<ITeamsService> teamsServiceMock;
        private Mock<IEventsService> eventsServiceMock;
        private Mock<IRatingService> ratingServiceMock;
        private HttpContextAccessor httpContextAccessor;

        public MatchMakingServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void SetUp()
        {
            skill1 = new SkillModel()
            {
                SkillId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9711"),
                Name = "1",
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Endorsers = new List<User>()
            };

            skill2 = new SkillModel()
            {
                SkillId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9712"),
                Name = "2",
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Endorsers = new List<User>()
            };


            // Users
            user1 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user 1",
                EmailConfirmed = true,
                Skills = new List<SkillModel>() { skill1, skill2 }

            };

            // Only the last digit in the Id is changed
            user2 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Teste user 2",
                EmailConfirmed = true
            };

            user3 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9746",
                UserName = "Test@12345.com",
                Email = "Test@12345.com",
                Name = "Teste user 3",
                EmailConfirmed = true
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
                Skills = new List<Tag>()
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
                Skills = new List<Tag>()
            };



            //Events
            e1 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3590"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserPublisher = user2,
                Title = "Event 1",
                Description = "Description",
                IsBanned = false,
                IsInPerson = true,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                State = EventState.Open,
                Longitude = 10.0,
                Latitude = 10.0
            };

            e2 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3591"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserPublisher = user2,
                Title = "Event 2",
                Description = "Description",
                IsBanned = false,
                IsInPerson = true,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                EventTags = new List<EventTag>(),
                State = EventState.Open,
                Longitude = 50.0,
                Latitude = 50.0
            };

            e3 = new Event()
            {
                EventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3592"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserPublisher = user2,
                Title = "Event 3",
                Description = "Description",
                IsBanned = false,
                IsInPerson = true,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                EventTags = new List<EventTag>(),
                State = EventState.Open
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
            teamsServiceMock = new Mock<ITeamsService>();
            eventsServiceMock = new Mock<IEventsService>();
            ratingServiceMock = new Mock<IRatingService>();


        }

        [Fact]
        public async Task MatchmakingService_GetRecommendedTeams()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MatchmakingService_GetRecommendedTeams").Options;

            List<Team> recTeams = null;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new MatchmakingService(context,
                                                     userManagerMock.Object,
                                                     httpContextAccessor,
                                                     userServiceMock.Object,
                                                     ratingServiceMock.Object,
                                                     teamsServiceMock.Object,
                                                     eventsServiceMock.Object);

                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                team2.Skills = new List<Tag>()
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

                team3.Skills = new List<Tag>()
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
                    },
                    new Tag()
                    {
                        TagId = new Guid(),
                        TagName = "4",
                        TeamId = team2.TeamId
                    },
                    new Tag()
                    {
                        TagId = new Guid(),
                        TagName = "5",
                        TeamId = team2.TeamId
                    }
                };



                context.Team.Add(team1);
                context.Team.Add(team2);
                context.Team.Add(team3);





                //save changes
                await context.SaveChangesAsync();

                //get user1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                //get user skills
                userServiceMock.Setup(x => x.GetUserSkills(It.IsAny<Guid>())).ReturnsAsync(user1.Skills.ToList);


                //get user Rating
                ratingServiceMock.Setup(x => x.GetTrueAverageRating(It.IsAny<Guid>())).ReturnsAsync(0);

                //get team rating
                teamsServiceMock.Setup(x => x.GetAverageRating(It.IsAny<Guid>())).ReturnsAsync(0);

                //Make List of teams
                List<Team> teams = new List<Team>() { team1, team2, team3 };

                //get openNewTeams
                teamsServiceMock.Setup(x => x.GetOpenNewTeams()).ReturnsAsync(teams);

                //Get Teams
                recTeams = await service.GetRecommendedTeams();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                // Verify if null
                Assert.NotNull(recTeams);

                //Verify Count
                Assert.Equal(2, recTeams.Count);

                //Verify order - order is reversed
                Assert.Equal("TeamName2", recTeams.Last().TeamName);
                Assert.Equal("TeamName3", recTeams.First().TeamName);

                var itemsInDatabase = await context.Team.ToListAsync();

                //Verify Number of teams in db
                Assert.Equal(3, itemsInDatabase.Count);

                


                //Clear database
                context.Database.EnsureDeleted();
            }
        }
        [Fact]
        public async Task MatchmakingService_GetRecommendedEvent()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MatchmakingService_GetRecommendedEvent").Options;

            List<Event> recEvents = null;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new MatchmakingService(context,
                                                     userManagerMock.Object,
                                                     httpContextAccessor,
                                                     userServiceMock.Object,
                                                     ratingServiceMock.Object,
                                                     teamsServiceMock.Object,
                                                     eventsServiceMock.Object);

                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);


                e2.EventTags = new List<EventTag>()
                {
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "1",
                        EventId = e1.EventId
                    },
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "2",
                        EventId = e1.EventId
                    },
                };

                e3.EventTags = new List<EventTag>()
                {
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "1",
                        EventId = e3.EventId
                    },
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "2",
                        EventId = e3.EventId
                    },
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "3",
                        EventId = e3.EventId
                    },
                    new EventTag()
                    {
                        EventTagId = new Guid(),
                        EventTagName = "4",
                        EventId = e3.EventId
                    },
                };



                context.Event.Add(e1);
                context.Event.Add(e2);
                context.Event.Add(e3);





                //save changes
                await context.SaveChangesAsync();

                //get user1
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                //get user skills
                userServiceMock.Setup(x => x.GetUserSkills(It.IsAny<Guid>())).ReturnsAsync(user1.Skills.ToList);

                //Make List of teams
                List<Event> events = new List<Event>() { e1, e2, e3 };

                //get openNewTeams
                eventsServiceMock.Setup(x => x.GetEvents()).ReturnsAsync(events);

                //Get Teams
                recEvents = await service.GetRecommendedEvents();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                // Verify if null
                Assert.NotNull(recEvents);

                //Verify Count
                Assert.Equal(2, recEvents.Count);

                //Verify order - order is reversed
                Assert.Equal("Event 2", recEvents.Last().Title);
                Assert.Equal("Event 3", recEvents.First().Title);

                var itemsInDatabase = await context.Event.ToListAsync();

                //Verify Number of teams in db
                Assert.Equal(3, itemsInDatabase.Count);




                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        
        [Fact]
        public async Task MatchmakingService_GetEventsNear()
        {
            //Arrange
            SetUp();
            List<Event> nearEvents = null;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MatchmakingService_GetEventsNear").Options;


            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new MatchmakingService(context,
                                                     userManagerMock.Object,
                                                     httpContextAccessor,
                                                     userServiceMock.Object,
                                                     ratingServiceMock.Object,
                                                     teamsServiceMock.Object,
                                                     eventsServiceMock.Object);

                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Event.Add(e2);

                //save changes
                context.SaveChanges();


                nearEvents = await service.GetEventsNear(9.9999, 9.9999, 10.0);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Event.ToListAsync();

                //Verify Number of teams in db
                Assert.Equal(2, itemsInDatabase.Count);

                // Verify if null
                Assert.NotNull(nearEvents);

                //Verify Count
                Assert.Equal(1, nearEvents.Count);

                //Verify event name
                Assert.Equal("Event 1", nearEvents.First().Title);

                




                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task MatchmakingService_GetEventsNearWithDistances()
        {
            //Arrange
            SetUp();
            IEnumerable<Object> nearEvents = new List<Object>();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MatchmakingService_GetEventsNearWithDistances").Options;


            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new MatchmakingService(context,
                                                     userManagerMock.Object,
                                                     httpContextAccessor,
                                                     userServiceMock.Object,
                                                     ratingServiceMock.Object,
                                                     teamsServiceMock.Object,
                                                     eventsServiceMock.Object);

                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                context.Event.Add(e1);
                context.Event.Add(e2);

                //save changes
                context.SaveChanges();


                nearEvents = await service.GetEventsNearWithDistances(9.9999, 9.9999, 10.0);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Event.ToListAsync();

                
                //Verify Number of teams in db
                Assert.Equal(2, itemsInDatabase.Count);

                // Verify if null
                Assert.NotNull(nearEvents);

                //Verify Count
                Assert.Equal(1, nearEvents.Count());


                //Clear database
                context.Database.EnsureDeleted();
            }
        }


    }
}

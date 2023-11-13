using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Models.Events;
using Workio.Models;
using Xunit.Abstractions;
using Workio.Services.Admin.Events;
using Workio.Services.Admin.Teams;
using Workio.Services.Admin;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Moq;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Services.Search;
using Workio.Services.ReportServices;

namespace Workio.Tests.Services
{
    public class ReportReasonServiceTest
    {
        private User user1;
        private User user2;

        private Team team1;
        private Team team2;

        private Event e1;
        private Event e2;

        private ReportReason userReportReason1;
        private ReportReason userReportReason2;
        private ReportReason teamReportReason1;
        private ReportReason teamReportReason2;
        private ReportReason eventReportReason1;
        private ReportReason eventReportReason2;

        private ReportUser userReport1;
        private ReportUser userReport2;
        private ReportTeam teamReport1;
        private ReportTeam teamReport2;
        private ReportEvent eventReport1;
        private ReportEvent eventReport2;

        private readonly ITestOutputHelper output;

        private Mock<IUserService> userServiceMock;
        private Mock<ITeamsService> teamServiceMock;
        private Mock<IEventsService> eventServiceMock;
        private Mock<IAdminEventService> adminEventServiceMock;
        private Mock<IAdminTeamService> adminTeamServiceMock;
        private Mock<IAdminService> adminServiceMock;


        public ReportReasonServiceTest(ITestOutputHelper output)
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
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserPublisher = user1,
                Title = "Event 2",
                Description = "Description",
                IsBanned = false,
                IsInPerson = false,
                Url = "https://www.itch.io",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
            };

            //Report Reasons
            //Users
            userReportReason1 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3501"),
                Reason = "User Report Reason 1",
                ReasonType = ReasonType.User
            };

            userReportReason2 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3502"),
                Reason = "User Report Reason 2",
                ReasonType = ReasonType.User
            };

            //teams
            teamReportReason1 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3503"),
                Reason = "Team Report Reason 1",
                ReasonType = ReasonType.Team
            };

            teamReportReason2 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3504"),
                Reason = "Team Report Reason 2",
                ReasonType = ReasonType.Team
            };

            //events
            eventReportReason1 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3505"),
                Reason = "User Report Reason 1",
                ReasonType = ReasonType.Event
            };

            eventReportReason2 = new ReportReason()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3506"),
                Reason = "User Report Reason 2",
                ReasonType = ReasonType.Event
            };


            // Reports
            // Users
            userReport1 = new ReportUser()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3511"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                Reporter = user1,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3501"),
                ReportReason = userReportReason1,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this",
                Date = DateTime.Now,
                ReportedId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                ReportedUser = user2
            };

            userReport2 = new ReportUser()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3512"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                Reporter = user2,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3502"),
                ReportReason = userReportReason2,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this again",
                Date = DateTime.Now,
                ReportedId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                ReportedUser = user1

            };

            // Teams
            teamReport1 = new ReportTeam()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3521"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                Reporter = user1,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3503"),
                ReportReason = teamReportReason1,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this",
                Date = DateTime.Now,
                ReportedTeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3575"),
                ReportedTeam = team1
            };

            teamReport2 = new ReportTeam()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3522"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                Reporter = user2,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3504"),
                ReportReason = teamReportReason2,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this again",
                Date = DateTime.Now,
                ReportedTeamId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3576"),
                ReportedTeam = team2
            };

            // Events
            eventReport1 = new ReportEvent()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3531"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                Reporter = user1,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3505"),
                ReportReason = eventReportReason1,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this",
                Date = DateTime.Now,
                ReportedEventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3590"),
                ReportedEvent = e1
            };

            eventReport2 = new ReportEvent()
            {
                Id = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3532"),
                ReporterId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                Reporter = user2,
                ReportReasonId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3506"),
                ReportReason = eventReportReason2,
                ReportStatus = ReportStatus.Pending,
                Description = "I reported this again",
                Date = DateTime.Now,
                ReportedEventId = new Guid("fea8c61b-d7e7-447b-9b5e-1ef5bf5b3591"),
                ReportedEvent = e2
            };

            userServiceMock = new Mock<IUserService>();
            teamServiceMock = new Mock<ITeamsService>();
            eventServiceMock = new Mock<IEventsService>();
            adminEventServiceMock= new Mock<IAdminEventService>();
            adminTeamServiceMock= new Mock<IAdminTeamService>();    
            adminServiceMock= new Mock<IAdminService>();    
        }

        [Fact]
        public async void ReportReasonService_GetReportReasonUsersAsync()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetReportReasonUsersAsync").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context, 
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                context.ReportUser.Add(userReport1);
                context.ReportUser.Add(userReport2);

                //Teams
                context.ReportTeams.Add(teamReport1);
                context.ReportTeams.Add(teamReport2);


                //Events
                context.ReportEvents.Add(eventReport1);
                context.ReportEvents.Add(eventReport2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportReason.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(6, itemsInDatabase);


                var items = await service.GetReportReasonsUserAsync();

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(2, items.Count);

                // Verify if contians report reasons
                Assert.Equal(userReportReason1.Id, items.ElementAt(0).Id);
                Assert.Equal(userReportReason2.Id, items.ElementAt(1).Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetReportReasonTeamAsync()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetReportReasonTeamAsync").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                context.ReportUser.Add(userReport1);
                context.ReportUser.Add(userReport2);

                //Teams
                context.ReportTeams.Add(teamReport1);
                context.ReportTeams.Add(teamReport2);


                //Events
                context.ReportEvents.Add(eventReport1);
                context.ReportEvents.Add(eventReport2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportReason.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(6, itemsInDatabase);


                var items = await service.GetReportReasonsTeamAsync();

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(2, items.Count);

                // Verify if contians report reasons
                Assert.Equal(teamReportReason1.Id, items.ElementAt(0).Id);
                Assert.Equal(teamReportReason2.Id, items.ElementAt(1).Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetReportReasonEventAsync()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetReportReasonEventAsync").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                context.ReportUser.Add(userReport1);
                context.ReportUser.Add(userReport2);

                //Teams
                context.ReportTeams.Add(teamReport1);
                context.ReportTeams.Add(teamReport2);


                //Events
                context.ReportEvents.Add(eventReport1);
                context.ReportEvents.Add(eventReport2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportReason.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(6, itemsInDatabase);


                var items = await service.GetReportReasonsEventAsync();

                // Verify if null
                Assert.NotNull(items);

                // Number Results Obtained
                Assert.Equal(2, items.Count);

                // Verify if contians report reasons
                Assert.Equal(eventReportReason1.Id, items.ElementAt(0).Id);
                Assert.Equal(eventReportReason2.Id, items.ElementAt(1).Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_AddUserReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AddUserReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                context.ReportTeams.Add(teamReport1);
                context.ReportTeams.Add(teamReport2);


                //Events
                context.ReportEvents.Add(eventReport1);
                context.ReportEvents.Add(eventReport2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportUser.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_AddTeamRport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AddTeamReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID




                //Events
                context.ReportEvents.Add(eventReport1);
                context.ReportEvents.Add(eventReport2);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportTeams.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_AddEventRport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AddEventReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                //Reports
                //Users
                context.ReportUser.Add(userReport1);
                context.ReportUser.Add(userReport2);

                //Teams
                context.ReportTeams.Add(teamReport1);
                context.ReportTeams.Add(teamReport2);


                context.SaveChanges();

                //Events
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                bool added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);


                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void ReportReasonService_GetUserReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetUserReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportUser.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetUserReports();

                // Verification of ReportUser ammounts
                Assert.Equal(2, items.Count);

                var r1 = items.Where(x => x.Id == userReport1.Id).First();
                var r2 = items.Where(x => x.Id == userReport2.Id).First();

                Assert.Equal(userReport1.Id, r1.Id);
                Assert.Equal(userReport2.Id, r2.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetTeamReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetTeamReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportTeams.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetTeamReports();

                // Verification of ReportUser ammounts
                Assert.Equal(2, items.Count);

                var r1 = items.Where(x => x.Id == teamReport1.Id).First();
                var r2 = items.Where(x => x.Id == teamReport2.Id).First();

                Assert.Equal(teamReport1.Id, r1.Id);
                Assert.Equal(teamReport2.Id, r2.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void ReportReasonService_GetEventReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetEventReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetEventReports();

                // Verification of ReportUser ammounts
                Assert.Equal(2, items.Count);

                var r1 = items.Where(x => x.Id == eventReport1.Id).First();
                var r2 = items.Where(x => x.Id == eventReport2.Id).First();

                Assert.Equal(eventReport1.Id, r1.Id);
                Assert.Equal(eventReport2.Id, r2.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetArchiveReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetArchiveReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetArchiveReports();

                // Verification of ReportUser ammounts
                Assert.Equal(3, items.Count);

                var r1 = items.Where(x => x.Id == userReport1.Id).First();
                var r2 = items.Where(x => x.Id == teamReport1.Id).First();
                var r3 = items.Where(x => x.Id == eventReport1.Id).First();

                Assert.Equal(userReport1.Id, r1.Id);
                Assert.Equal(teamReport1.Id, r2.Id);
                Assert.Equal(eventReport1.Id, r3.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetArchiveTeamReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetArchiveTeamReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetArchiveTeamReports();

                // Verification of ReportUser ammounts
                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.Id == teamReport1.Id).First();

                Assert.Equal(teamReport1.Id, r1.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetArchiveEventReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetArchiveEventReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetArchiveEventReports();

                // Verification of ReportUser ammounts
                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.Id == eventReport1.Id).First();

                Assert.Equal(eventReport1.Id, r1.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetArchiveUserReports()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetArchiveUserReports").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetArchiveUserReports();

                // Verification of ReportUser ammounts
                Assert.Equal(1, items.Count);

                var r1 = items.Where(x => x.Id == userReport1.Id).First();

                Assert.Equal(userReport1.Id, r1.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetUserReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetUserReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportUser.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetUserReport(userReport1.Id);

                // Verification of ReportUser ammounts
                Assert.Equal(userReport1.Id, items.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetTeamReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetTeamReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportTeams.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetTeamReport(teamReport1.Id);

                // Verification of ReportUser ammounts
                Assert.Equal(teamReport1.Id, items.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_GetEventReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_GetEventReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                userReport1.ReportStatus = ReportStatus.Accepted;
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);

                added = await service.AddUserReport(userReport2);
                Assert.True(added);

                //Teams
                //Change Status to other than pending
                teamReport1.ReportStatus = ReportStatus.Accepted;
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID

                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team2);
                added = await service.AddTeamReport(teamReport2);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport2.ReporterId = user1.Id;
                teamReport2.Reporter = user1;

                added = await service.AddTeamReport(teamReport2);
                Assert.True(added); //Reporter ID != Team OwnerID



                //Events
                //Change Status to other than pending
                eventReport1.ReportStatus = ReportStatus.Accepted;
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e1);
                added = await service.AddEventReport(eventReport1);
                Assert.False(added); //Reporter is event owner, so it doesnt work

                //Muda-se o id do reporterId
                eventReport1.ReporterId = user2.Id;
                eventReport1.Reporter = user2;

                added = await service.AddEventReport(eventReport1);
                Assert.True(added); //Reporter ID = Event Creator Id

                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(2, itemsInDatabase);

                var items = await service.GetEventReport(eventReport1.Id);

                // Verification of ReportUser ammounts
                Assert.Equal(eventReport1.Id, items.Id);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void ReportReasonService_AddNewReason()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AddNewReason").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //ReportReasons
                //Users
                bool added = await service.AddNewReason(userReportReason1);
                Assert.True(added);

                added = await service.AddNewReason(userReportReason2);
                Assert.True(added);

                //Teams
                added = await service.AddNewReason(teamReportReason1);
                Assert.True(added);

                added = await service.AddNewReason(teamReportReason2);
                Assert.True(added);

                //Events
                added = await service.AddNewReason(eventReportReason1);
                Assert.True(added);

                added = await service.AddNewReason(eventReportReason2);
                Assert.True(added);


                context.SaveChanges();


            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportReason.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(6, itemsInDatabase);

                var items = await service.GetReportReasonsUserAsync();

                // Verification of ReportReasonUser amounts
                Assert.Equal(2, items.Count);


                items = await service.GetReportReasonsTeamAsync();

                // Verification of ReportReasonUser amounts
                Assert.Equal(2, items.Count);


                items = await service.GetReportReasonsEventAsync();

                // Verification of ReportReasonUser amounts
                Assert.Equal(2, items.Count);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void ReportReasonService_RejectUserReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_RejectUserReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportUser.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);

                bool rejected = await service.RejectUserReport(userReport1.Id);

                // Verify if rejected
                Assert.True(rejected);

                var item = await service.GetUserReport(userReport1.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Rejected, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_RejectTeamReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_RejectTeamReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Teams
                //Change Status to other than pending
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                bool added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportTeams.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);

                bool rejected = await service.RejectTeamReport(teamReport1.Id);

                // Verify if rejected
                Assert.True(rejected);

                var item = await service.GetTeamReport(teamReport1.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Rejected, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_RejectEventReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_RejectEventReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Events
                //Change Status to other than pending
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                bool added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);

                bool rejected = await service.RejectEventReport(eventReport2.Id);

                // Verify if rejected
                Assert.True(rejected);

                var item = await service.GetEventReport(eventReport2.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Rejected, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }




        [Fact]
        public async void ReportReasonService_AcceptUserReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AcceptUserReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Users
                //Change Status to other than pending
                bool added = await service.AddUserReport(userReport1);
                Assert.True(added);



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportUser.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);


                adminServiceMock.Setup(x => x.SuspendUser(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);
                bool accepted = await service.AcceptUserReport(userReport1.Id);

                // Verify if rejected
                Assert.True(accepted);

                var item = await service.GetUserReport(userReport1.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Accepted, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_AcceptTeamReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AcceptTeamReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Teams
                //Change Status to other than pending
                teamServiceMock.Setup(x => x.GetTeamById(It.IsAny<Guid>())).ReturnsAsync(team1);
                bool added = await service.AddTeamReport(teamReport1);
                Assert.False(added); //Reporter ID = Team OwnerID

                //Muda-se o id do reporterId
                teamReport1.ReporterId = user2.Id;
                teamReport1.Reporter = user2;

                added = await service.AddTeamReport(teamReport1);
                Assert.True(added); //Reporter ID != Team OwnerID



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportTeams.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);

                adminTeamServiceMock.Setup(x => x.BanTeam(It.IsAny<Guid>())).ReturnsAsync(true);

                bool accepted = await service.AcceptTeamReport(teamReport1.Id);

                // Verify if rejected
                Assert.True(accepted);

                var item = await service.GetTeamReport(teamReport1.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Accepted, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void ReportReasonService_AcceptEventReport()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ReportReasonService_AcceptEventReport").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                //Add to context
                //Users
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Teams
                context.Team.Add(team1);
                context.Team.Add(team2);

                //Events
                context.Event.Add(e1);
                context.Event.Add(e2);

                //ReportReasons
                //Users
                context.ReportReason.Add(userReportReason1);
                context.ReportReason.Add(userReportReason2);

                //Teams
                context.ReportReason.Add(teamReportReason1);
                context.ReportReason.Add(teamReportReason2);

                //Events
                context.ReportReason.Add(eventReportReason1);
                context.ReportReason.Add(eventReportReason2);


                context.SaveChanges();

                //Reports
                //Events
                //Change Status to other than pending
                eventServiceMock.Setup(x => x.GetEvent(It.IsAny<Guid>())).ReturnsAsync(e2);
                bool added = await service.AddEventReport(eventReport2);
                Assert.True(added); //Reporter is not the event owner so it works



            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new ReportReasonService(context,
                                                      userServiceMock.Object,
                                                      teamServiceMock.Object,
                                                      eventServiceMock.Object,
                                                      adminEventServiceMock.Object,
                                                      adminTeamServiceMock.Object,
                                                      adminServiceMock.Object);

                var itemsInDatabase = await context.ReportEvents.CountAsync();

                // Verification of ReportUser ammounts
                Assert.Equal(1, itemsInDatabase);


                adminEventServiceMock.Setup(x => x.BanEvent(It.IsAny<Guid>())).ReturnsAsync(true);

                bool accepted = await service.AcceptEventReport(eventReport2.Id);

                // Verify if rejected
                Assert.True(accepted);

                var item = await service.GetEventReport(eventReport2.Id);

                //Verify report state
                Assert.Equal(ReportStatus.Accepted, item.ReportStatus);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }



    }
}

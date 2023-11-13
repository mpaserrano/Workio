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
using Workio.Services;
using Workio.Services.Connections;
using Workio.Services.Interfaces;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class ConnectionsServiceTest
    {
        private User user1;
        private User user2;
        private User user3;

        private Connection con1;
        private Connection con2;
        private readonly ITestOutputHelper output;


        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;

        public ConnectionsServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

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

            user3 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9751",
                UserName = "Test@1234567.com",
                Email = "Test@1234567.com",
                Name = "Teste user 3",
                EmailConfirmed = true
            };

            con1 = new Connection
            {
                Id = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9701"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                RequestedUserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                State = ConnectionState.Pending,
                ConnectionDate= DateTime.Now
            };

            con2 = new Connection
            {
                Id = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9701"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                RequestedUserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                State = ConnectionState.Pending,
                ConnectionDate = DateTime.Now
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
        }

        [Fact]
        public async Task ConnectionsServiceTest_AddConnection()
        {
            //Arrange
            SetUp();
            bool added1 = false; 

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_AddConnection").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                added1 = await service.AddConnection(con1);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.True(added1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Connections.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                var items = await context.Connections.ToListAsync();

                Assert.Equal(con1.Id, items.First().Id);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task ConnectionsServiceTest_GetConnectionsAsync()
        {
            //Arrange
            SetUp();
            bool added1 = false;

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_GetConnectionsAsync").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                added1 = await service.AddConnection(con1);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.True(added1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Connections.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                var items = await service.GetConnectionsAsync();

                Assert.Equal(1, items.Count);
                Assert.Equal(con1.Id, items.First().Id);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task ConnectionsServiceTest_RemoveConnection()
        {
            //Arrange
            SetUp();
            bool con = false;
            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_RemoveConnection").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                await service.AddConnection(con1);
                con = await service.RemoveConnection(con1);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.True(con);


                var itemsInDatabase = await context.Connections.CountAsync();
                var item = await context.Connections.FirstOrDefaultAsync();

                // Verify amount of teams
                Assert.Equal(0, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task ConnectionsServiceTest_UpdateConnection()
        {
            //Arrange
            SetUp();
            bool con = false;
            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_UpdateConnection").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                await service.AddConnection(con1);

                con1.State = ConnectionState.Accepted;
                con = await service.UpdateConnection(con1);

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.NotNull(con);


                var itemsInDatabase = await context.Connections.CountAsync();
                var item = await context.Connections.FirstOrDefaultAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                // Verify if connection is same
                Assert.Equal(ConnectionState.Accepted, item.State);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task ConnectionsServiceTest_GetUserConnectionsAsync()
        {
            //Arrange
            SetUp();
            List<User> connectedUsers = new List<User>();
            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_UpdateConnection").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                con1.State = ConnectionState.Accepted;
                await service.AddConnection(con1);

                connectedUsers = (await service.GetUserConnectionsAsync(new Guid(user1.Id))).Select(c => c.UserId == user1.Id.ToString() ? c.RequestedUser : c.RequestOwner).ToList();

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.NotNull(connectedUsers);

                //Assert connection quantity
                Assert.Equal(1, connectedUsers.Count);


                var itemsInDatabase = await context.Connections.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                // Verify if connected user is correct
                Assert.Equal(user2.Id, connectedUsers.First().Id);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task ConnectionsServiceTest_GetUserPendingConnectionsAsync()
        {
            //Arrange
            SetUp();
            List<User> pendingConnections = null;
            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_ConnectionsServiceTest_GetUserPendingConnectionsAsync").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);

                var service = new ConnectionService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();

                await service.AddConnection(con2);

                pendingConnections = (await service.GetUserPendingConnectionsAsync()).Where(c => c.RequestedUserId != user2.Id.ToString()).Select(c => c.RequestOwner).ToList();
                output.WriteLine(pendingConnections.Count.ToString());
                foreach(User u in pendingConnections)
                {
                    output.WriteLine(u.Name);
                }

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                //Assert booleans on add connection
                Assert.NotNull(pendingConnections);

                //Assert connection quantity
                Assert.Equal(1, pendingConnections.Count);


                var itemsInDatabase = await context.Connections.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                // Verify if connected user is correct
                Assert.Equal(user2.Id, pendingConnections.First().Id);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }
    }
}

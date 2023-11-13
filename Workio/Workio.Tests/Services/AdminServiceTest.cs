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
using Workio.Services.Admin;
using Workio.Services.Interfaces;
using Workio.Services.RequestEntityStatusServices;

namespace Workio.Tests.Services
{
    public class AdminServiceTest
    {

        private User user1;
        private User user2;

        private RequestEntityStatus request1;
        private RequestEntityStatus request2;

        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;
        private Mock<IUserService> userServiceMock;
        private List<string> _authorizedRoles = new List<string>() { "Admin", "Mod" };



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

            request1 = new RequestEntityStatus
            {
                Id = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"),
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Motivation = "Motivated enough!",
                FilePath = "FilePath",
                OriginalFileName = "Original File",
                AlteredFileName = "Altered File",
                RequestState = RequestState.Pending,
            };

            request2 = new RequestEntityStatus
            {
                Id = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce6"),
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                Motivation = "Motivated enough!",
                FilePath = "FilePath",
                OriginalFileName = "Original File",
                AlteredFileName = "Altered File",
                RequestState = RequestState.Pending,
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


            userServiceMock = new Mock<IUserService>();
        }

        [Fact]
        public async void AdminServiceTest_GetRequests()
        {
            //Arrange
            List<RequestEntityStatus> requests = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminServiceTest_GetRequests").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminService(context, userServiceMock.Object, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Add requests to context
                context.RequestEntityStatus.Add(request1);
                context.RequestEntityStatus.Add(request2);

                context.SaveChanges();

                requests = await service.GetRequests();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(requests);

                //Verify ammount
                Assert.Equal(2, requests.Count);

                //Verify Requests
                var r1 = requests.Where(x => x.Id == request1.Id).First();
                var r2 = requests.Where(x => x.Id == request2.Id).First();

                Assert.Equal(request1.Id, r1.Id);
                Assert.Equal(request2.Id, r2.Id);

                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void AdminServiceTest_ApproveRequest()
        {
            //Arrange
            bool approved = false;
            List<RequestEntityStatus> requests = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminServiceTest_ApproveRequest").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(It.IsAny<Guid>())).ReturnsAsync(user2);
                userManagerMock.Setup(x => x.IsInRoleAsync(user1, It.IsAny<string>())).ReturnsAsync(true);
                userManagerMock.Setup(x => x.IsInRoleAsync(user2, It.IsAny<string>())).ReturnsAsync(false);

                var service = new AdminService(context, userServiceMock.Object, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Add requests to context
                context.RequestEntityStatus.Add(request1);

                context.SaveChanges();

                approved = await service.ApproveRequest(request1.Id);

                requests = await service.GetRequests();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if approved
                Assert.True(approved);

                //Verify amount
                Assert.Equal(1, requests.Count);

                //Verify Requests
                var r1 = requests.Where(x => x.Id == request1.Id).First();

                Assert.Equal(request1.Id, r1.Id);

                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void AdminServiceTest_RejectRequest()
        {
            //Arrange
            bool rejected = false;
            List<RequestEntityStatus> requests = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminServiceTest_RejectRequest").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminService(context, userServiceMock.Object, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Add requests to context
                context.RequestEntityStatus.Add(request1);

                context.SaveChanges();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                userServiceMock.Setup(x => x.GetUser(It.IsAny<Guid>())).ReturnsAsync(user2);
                userManagerMock.Setup(x => x.IsInRoleAsync(user1, It.IsAny<string>())).ReturnsAsync(true);
                userManagerMock.Setup(x => x.IsInRoleAsync(user2, It.IsAny<string>())).ReturnsAsync(false);

                rejected = await service.RejectRequest(request1.Id);

                requests = await service.GetRequests();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.True(rejected);

                //Verify ammount
                Assert.Equal(1, requests.Count);

                //Verify Requests
                var r1 = requests.Where(x => x.Id == request1.Id).First();

                Assert.Equal(request1.Id, r1.Id);

                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void AdminServiceTest_GetRequestById()
        {
            //Arrange
            RequestEntityStatus r1 = null;
            RequestEntityStatus r2 = null;

            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AdminServiceTest_GetRequestById").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new AdminService(context, userServiceMock.Object, userManagerMock.Object, httpContextAccessor);
                //Add users to context
                context.Users.Add(user1);
                context.Users.Add(user2);

                //Add requests to context
                context.RequestEntityStatus.Add(request1);
                context.RequestEntityStatus.Add(request2);

                context.SaveChanges();

                r1 = await service.GetRequestById(request1.Id);
                r2 = await service.GetRequestById(request2.Id);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                // Verify if null
                Assert.NotNull(r1);
                Assert.NotNull(r2);

                // Verify IDs
                Assert.Equal(request1.Id, r1.Id);
                Assert.Equal(request2.Id, r2.Id);

                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        
    }
}

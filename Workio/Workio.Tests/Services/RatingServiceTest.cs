using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Workio.Models;
using Xunit.Sdk;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Services;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Workio.Services.Interfaces;

namespace Workio.Tests.Services
{
    public class RatingServiceTest
    {
        private RatingModel rating;
        private RatingModel rating2;
        private Mock<UserManager<User>> userManagerMock;
        private HttpContextAccessor httpContextAccessor;
        private User user1;
        private User user2;

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
                Id = "98fb422f-49c7-46a7-a67d-8c980a96d1ce",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Test user 2",
                EmailConfirmed = true
            };

            rating = new RatingModel
            {
                RatingId = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"),
                ReceiverId = new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"),
                Rating = 5,
                RaterId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Comment = "Comment"
            };

            //Only the last digit in the ID changes, and the rating value too
            rating2 = new RatingModel
            {
                RatingId = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce1"),
                ReceiverId = new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"),
                Rating = 4,
                RaterId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Comment = "Comment"
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
        public async void RatingServiceTest_AddRating()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_BlockService_AddRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);

                context.SaveChanges();
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);
                var item = await context.RatingModel.FirstAsync();

                // Verification of Rating Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.RatingId);

                // Verification of Receiver Id
                Assert.Equal(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"), item.ReceiverId);

                // Verification of Rater Id
                Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.RaterId);

                // Verification of rating value
                Assert.Equal(5, item.Rating);

                // Verification of Comment
                Assert.Equal("Comment", item.Comment);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetRatings()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_AddRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);
            }


            //Assert
            using (var context = new ApplicationDbContext(options))
            {

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                
                var items = await service.GetRatings();

                //Asserts
                foreach(var item in items)
                {
                    // Verification of Rating Id
                    Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.RatingId);

                    // Verification of Receiver Id
                    Assert.Equal(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"), item.ReceiverId);

                    // Verification of Rater Id
                    Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.RaterId);

                    // Verification of rating value
                    Assert.Equal(5, item.Rating);

                    // Verification of Comment
                    Assert.Equal("Comment", item.Comment);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_RemoveRating()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_RemoveRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);

            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.RemoveRating(rating);

                itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(0, itemsInDatabase);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetAverageRating()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_GetAverageRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);
                await service.AddRating(rating2);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                var avgRating = await service.GetAverageRating(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify average rating rounded to below
                Assert.Equal(4.5, avgRating);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetTrueAverageRating()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_GetTrueAverageRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor); ;
                await service.AddRating(rating);
                await service.AddRating(rating2);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                var avgRating = await service.GetTrueAverageRating(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify average rating rounded to below
                Assert.Equal(4.5, avgRating);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_IsRated()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_RemoveRating").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);

            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);

                bool rated = service.IsRated(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify is Rated with 1 rating
                Assert.True(rated);

                await service.RemoveRating(rating);

                itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(0, itemsInDatabase);


                rated = service.IsRated(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify isRated with 0 ratings
                Assert.False(rated);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetNumberOfRatings()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_GetNumberOfRatings").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);
                await service.AddRating(rating2);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                var numberOfRatings = await service.GetNumberOfRatings(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify number of ratings with 2 ratings
                Assert.Equal(2, numberOfRatings);

                await service.RemoveRating(rating);

                itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                numberOfRatings = await service.GetNumberOfRatings(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify number of ratings with 1 rating
                Assert.Equal(1, numberOfRatings);

                await service.RemoveRating(rating2);

                itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(0, itemsInDatabase);

                numberOfRatings = await service.GetNumberOfRatings(new Guid("98fb422f-49c7-46a7-a67d-8c980a96d1ce"));

                // Verify number of ratings with 0 rating
                Assert.Equal(0, numberOfRatings);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetRatingById()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_GetRatingById").Options;

            RatingModel r1 = null;
            RatingModel r2 = null;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor); ;
                await service.AddRating(rating);
                await service.AddRating(rating2);
                r1 = await service.GetRatingById(rating.RatingId);
                r2 = await service.GetRatingById(new Guid());

            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(2, itemsInDatabase);
                Assert.NotNull(r1);
                Assert.Null(r2);

                // Verification it gets rating
                Assert.Equal(rating.RatingId, r1.RatingId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_IsAlreadyRated()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_IsAlreadyRated").Options;
            bool rated = false;
            bool rated2 = true;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                rated = await service.IsAlreadyRated(new Guid(user2.Id));
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                rated2 = await service.IsAlreadyRated(new Guid(user1.Id));

            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);

                // Verify if is already rated
                Assert.True(rated);
                Assert.False(rated2);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void RatingServiceTest_GetRatingId()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RatingService_GetRatingId").Options;
            Guid ratingId = new Guid("00000000-0000-0000-0000-000000000000");
            Guid rating2Id = new Guid("00000000-0000-0000-0000-000000000000");

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);
                await service.AddRating(rating);

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user1);
                ratingId = await service.GetRatingId(new Guid(user2.Id));
                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                rating2Id = await service.GetRatingId(new Guid(user1.Id));

            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RatingService(context, userManagerMock.Object, httpContextAccessor);

                // Verify is if it gets the rating by a default GUID
                Assert.NotEqual(ratingId, new Guid("00000000-0000-0000-0000-000000000000"));
                Assert.Equal(rating2Id, new Guid("00000000-0000-0000-0000-000000000000"));
               

                itemsInDatabase = await context.RatingModel.CountAsync();

                // Verification of Rating Guid
                Assert.Equal(rating.RatingId, ratingId);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }
    }
}

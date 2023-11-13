using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Services;
using Workio.Services.RequestEntityStatusServices;

namespace Workio.Tests.Services
{
    public class RequestEntityStatusServiceTest
    {
        private RequestEntityStatus request;


        private void SetUp()
        {

            request = new RequestEntityStatus
            {
                Id = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"),
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Motivation = "Motivated enough!",
                FilePath = "FilePath",
                OriginalFileName = "Original File",
                AlteredFileName = "Altered File",
                RequestState = RequestState.Pending,
            };

        }

        [Fact]
        public async void RequestEntityStatusService_CreateRequest()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_CreateRequest").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);
                var item = await context.RequestEntityStatus.FirstAsync();

                // Verification of Request Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);

                // Verification of user Id
                Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.UserId);

                // Verification of Motivation
                Assert.Equal("Motivated enough!", item.Motivation);

                // Verification of FilePath
                Assert.Equal("FilePath", item.FilePath);

                // Verification of Original File Name
                Assert.Equal("Original File", item.OriginalFileName);

                // Verification of Altered File Name
                Assert.Equal("Altered File", item.AlteredFileName);

                // Verification of State
                Assert.Equal(RequestState.Pending, item.RequestState);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void RequestEntityStatusService_UpdateRequest()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_UpdateRequest").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RequestEntityStatusService(context);

                request.Motivation = "Not motivated";

                bool updated = await service.UpdateRequest(request);

                var item = await context.RequestEntityStatus.FirstAsync();

                // Verification of Request Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);

                // Verification of user Id
                Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.UserId);

                // Verification of Motivation
                Assert.Equal("Not motivated", item.Motivation);

                // Verification of FilePath
                Assert.Equal("FilePath", item.FilePath);

                // Verification of Original File Name
                Assert.Equal("Original File", item.OriginalFileName);

                // Verification of Altered File Name
                Assert.Equal("Altered File", item.AlteredFileName);

                // Verification of State
                Assert.Equal(RequestState.Pending, item.RequestState);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void RequestEntityStatusService_AlreadyRequested()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_AlreadyRequested").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);

                var service = new RequestEntityStatusService(context);

                bool alreadyRequested = service.AlreadyRequested(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"));

                // Verify if already requested is true with a request
                Assert.True(alreadyRequested);

                // Use a new user Id
                alreadyRequested = service.AlreadyRequested(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9749"));

                // Verify if already requested is true without requests on new userId
                Assert.False(alreadyRequested);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void RequestEntityStatusService_GetRequestById()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_GetRequestById").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RequestEntityStatusService(context);

                var item = await service.GetRequestById(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"));

                // Verification of Request Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);

                // Verification of user Id
                Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.UserId);

                // Verification of Motivation
                Assert.Equal("Motivated enough!", item.Motivation);

                // Verification of FilePath
                Assert.Equal("FilePath", item.FilePath);

                // Verification of Original File Name
                Assert.Equal("Original File", item.OriginalFileName);

                // Verification of Altered File Name
                Assert.Equal("Altered File", item.AlteredFileName);

                // Verification of State
                Assert.Equal(RequestState.Pending, item.RequestState);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void RequestEntityStatusService_GetRequestByUserId()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_GetRequestByUserId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RequestEntityStatusService(context);

                var item = await service.GetRequestStateByUserId(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"));


                // Verification of State
                Assert.Equal(RequestState.Pending, item);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }

        [Fact]
        public async void RequestEntityStatusService_GetUserInfo()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_GetUserInfo").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RequestEntityStatusService(context);

                var items = await service.GetUserInfo(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"));


                foreach(var item in items)
                {
                    // Verification of Request Id
                    Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);

                    // Verification of user Id
                    Assert.Equal(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"), item.UserId);

                    // Verification of Motivation
                    Assert.Equal("Motivated enough!", item.Motivation);

                    // Verification of FilePath
                    Assert.Equal("FilePath", item.FilePath);

                    // Verification of Original File Name
                    Assert.Equal("Original File", item.OriginalFileName);

                    // Verification of Altered File Name
                    Assert.Equal("Altered File", item.AlteredFileName);

                    // Verification of State
                    Assert.Equal(RequestState.Pending, item.RequestState);
                }
                

                //Clear database
                context.Database.EnsureDeleted();

            }
        }


        [Fact]
        public async void RequestEntityStatusService_GetRequestId()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RequestEntityStatusService_GetRequestId").Options;

            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new RequestEntityStatusService(context);
                await service.CreateRequest(request);
            }
            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.RequestEntityStatus.CountAsync();

                // Verification of Rating ammounts
                Assert.Equal(1, itemsInDatabase);


                var service = new RequestEntityStatusService(context);


                // Receives a Guid
                var item = await service.GetRequestId(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"));


                // Verification of Guid
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item);

                //Clear database
                context.Database.EnsureDeleted();

            }
        }
    }
}

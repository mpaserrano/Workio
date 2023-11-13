using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Services;
using Workio.Services.Notifications;

namespace Workio.Tests.Services
{
    public class BlockServiceTest
    {

        private User sourceUser;
        private User blockedUser;
        private BlockedUsersModel block;


        private void SetUp()
        {
            sourceUser = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user",
                EmailConfirmed = true
            };

            blockedUser = new User
            {
                Id = "98fb422f-49c7-46a7-a67d-8c980a96d1ce",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Teste user",
                EmailConfirmed = true
            };

            block = new BlockedUsersModel
            {
                Id = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"),
                SourceUser = sourceUser,
                SourceUserId = sourceUser.Id,
                BlockedUser = blockedUser,
                BlockedUserId = blockedUser.Id,
                BlockDateTime = DateTime.Now,
            };

        }

        [Fact]
        public async void BlockService_AddBlock()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_BlockService_AddBlock").Options;

            

            //Setup context for writing
            //Act
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new BlockService(context);
                await service.AddBlock(block);
            }


            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.BlockedUsersModel.CountAsync();
                Assert.Equal(1, itemsInDatabase);
                var item = await context.BlockedUsersModel.FirstAsync();
                // Verification of SourceUserId
                Assert.Equal("a2463fbc-1f6b-470d-b40d-daf9e0bc9744", item.SourceUserId);
                // Verification of BlockedUserId
                Assert.Equal("98fb422f-49c7-46a7-a67d-8c980a96d1ce", item.BlockedUserId);
                // Verification of Block Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);
                // Date should be equal to today
                var today = DateTime.Now;
                Assert.Equal(today.Date, item.BlockDateTime.Date);

            }
        }

        [Fact]
        public async void BlockService_RemoveBlock()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_BlockService_RemoveBlock").Options;


            //Act
            //Create block
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new BlockService(context);
                await service.AddBlock(block);

                //Get Block
                var item = await context.BlockedUsersModel.FirstAsync();
                // Verification of Block Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);

                //RemoveBlock
                await service.RemoveBlock(block);   
            }

            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                //Block Count
                var itemsInDatabase = await context.BlockedUsersModel.CountAsync();
                Assert.Equal(0, itemsInDatabase);
            }
        }


        [Fact]
        public async void BlockService_GetBlocks()
        {
            //Arrange
            SetUp();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_BlockService_GetBlock").Options;


            //Act
            //Create block
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                var service = new BlockService(context);
                await service.AddBlock(block);

                //Get Block
                var item = await context.BlockedUsersModel.FirstAsync();
                // Verification of Block Id
                Assert.Equal(new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"), item.Id);
            }

            //Assert
            using (var context = new ApplicationDbContext(options))
            {
                var service = new BlockService(context);

                //Block Count
                var itemsInDatabase = await service.GetBlocksAsync();
                Assert.Equal(1, itemsInDatabase.Count);
            }
        }


    }
}

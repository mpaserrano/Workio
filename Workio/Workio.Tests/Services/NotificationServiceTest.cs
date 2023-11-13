using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Services.Interfaces;
using Moq;
using Workio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using Workio.Services.Notifications;
using Microsoft.Extensions.Options;

namespace Workio.Tests.Services
{
    public class NotificationServiceTest
    {
        [Fact]
        public async Task CreateNotificationTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewNotification").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new NotificationService(context);
                var fakeUser = new User
                {
                    Id = userId,
                    UserName = "fake@example.com"
                };
                await service.CreateNotification(new Notification
                {
                    Id = id,
                    Text = "Test",
                    UserId = fakeUser.Id,
                    User = fakeUser,
                    URL = "fakeURL"
                });
            }

            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Notifications.CountAsync();
                Assert.Equal(1, itemsInDatabase);
                var item = await context.Notifications.FirstAsync();
                Assert.Equal("Test", item.Text);
                // Notification should be unread always on creation
                Assert.False(item.IsRead);
                // Notification should have defined Id
                Assert.Equal(id, item.Id);
                // Date should be equal to today
                var today = DateTime.Now;
                Assert.Equal(today.Date, item.CreatedAt.Date);
            }
        }

        [Fact]
        public async Task GetNotificationTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Notification").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                
                var fakeUser = new User
                {
                    Id = userId1,
                    UserName = "fake@example.com",
                    Name = "Tester1",
                };
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Name = "Tester2"
                };

                var not1 = new Notification
                {
                    Id = id,
                    Text = "Test",
                    UserId = fakeUser.Id,
                    User = fakeUser,
                    URL = "fakeURL"
                };
                var not2 = new Notification
                {
                    Id = Guid.NewGuid(),
                    Text = "Test2",
                    UserId = fakeUser2.Id,
                    User = fakeUser2,
                    URL = "fakeURL2"
                };

                context.Notifications.Add(not1);
                context.Notifications.Add(not2);
                await context.SaveChangesAsync();
            }
            using (var context = new ApplicationDbContext(options))
            {
                var notificationServiceMock = new NotificationService(context);
                var item = await notificationServiceMock.GetNotificationAsync(id);
                Assert.NotNull(item);
                Assert.Equal("Test", item.Text);
                Assert.Equal(id, item.Id);
                Assert.NotEqual(userId2, item.UserId);
            }
        }

        [Fact]
        public async Task MarkAsReadByIdTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Notification").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();

            var fakeUser = new User
            {
                Id = userId1,
                UserName = "fake@example.com",
                Name = "Tester1",
            };

            var not1 = new Notification
            {
                Id = id,
                Text = "Test",
                UserId = fakeUser.Id,
                User = fakeUser,
                URL = "fakeURL",
                IsRead = false
            };

            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Name = "Tester2"
                };

                var not2 = new Notification
                {
                    Id = Guid.NewGuid(),
                    Text = "Test2",
                    UserId = fakeUser2.Id,
                    User = fakeUser2,
                    URL = "fakeURL2"
                };

                context.Notifications.Add(not1);
                context.Notifications.Add(not2);
                await context.SaveChangesAsync();
            }
            using (var context = new ApplicationDbContext(options))
            {
                var notificationServiceMock = new NotificationService(context);
                var item = await notificationServiceMock.MarkAsRead(id);
                Assert.True(item);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var item = context.Notifications.Where(x => x.Id == id).FirstOrDefault();
                Assert.NotNull(item);
                Assert.True(item.IsRead);
            }
        }

        [Fact]
        public async Task MarkAsUnreadTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Notification").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();

            var fakeUser = new User
            {
                Id = userId1,
                UserName = "fake@example.com",
                Name = "Tester1",
            };

            var not1 = new Notification
            {
                Id = id,
                Text = "Test",
                UserId = fakeUser.Id,
                User = fakeUser,
                URL = "fakeURL",
                IsRead = true
            };

            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Name = "Tester2"
                };

                var not2 = new Notification
                {
                    Id = Guid.NewGuid(),
                    Text = "Test2",
                    UserId = fakeUser2.Id,
                    User = fakeUser2,
                    URL = "fakeURL2",
                    IsRead = true
                };

                context.Notifications.Add(not1);
                context.Notifications.Add(not2);
                await context.SaveChangesAsync();
            }
            using (var context = new ApplicationDbContext(options))
            {
                var notificationServiceMock = new NotificationService(context);
                var item = await notificationServiceMock.MarkAsUnread(id);
                Assert.True(item);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var item = context.Notifications.Where(x => x.Id == id).FirstOrDefault();
                Assert.NotNull(item);
                Assert.False(item.IsRead);
            }
        }
    }
}

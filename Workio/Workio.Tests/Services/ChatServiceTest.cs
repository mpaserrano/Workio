using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Data;
using Workio.Models;
using Workio.Models.Chat;
using Workio.Services.Chat;
using Workio.Services.Notifications;

namespace Workio.Tests.Services
{
    public class ChatServiceTest
    {

        [Fact]
        public async Task CreateChatRoomTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewChatRoom").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                //Clear database
                context.Database.EnsureDeleted();

                var service = new ChatService(context);
                var fakeUser = new User
                {
                    Id = userId,
                    UserName = "fake@example.com",
                    Email = "fake@example.com",
                    Name = "Fake 1",
                    EmailConfirmed = true
                };
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Email = "fake2@example.com",
                    Name = "Fake 2",
                    EmailConfirmed = true
                };

                context.Users.Add(fakeUser);
                context.Users.Add(fakeUser2);
                await context.SaveChangesAsync();

                var newChat = new ChatRoom
                {
                    ChatRoomId = id,
                    ChatRoomName = "Migos"
                };

                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser
                };

                UserChatRoom otherChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser2
                };

                newChat.Members.Add(userChat);
                newChat.Members.Add(otherChat);

                await service.CreateChatRoom(newChat);

            }

            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.ChatRooms.CountAsync();
                Assert.Equal(1, itemsInDatabase);
                var item = await context.ChatRooms.Include(c => c.Members).FirstAsync();
                Assert.Equal("Migos", item.ChatRoomName);

                // ChatRoom should have defined Id
                Assert.Equal(id, item.ChatRoomId);

                // ChatRoom should have only 2 members
                Assert.Equal(2, item.Members.Count);

                // Should be empty - no messages
                Assert.False(item.Messages.Any());
            }
        }

        /// <summary>
        /// Testa se é possível criar uma equipa com 3 membros sem fornecer uma equipa.
        /// Não deve ser possível
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateChatRoomFailTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewChatRoom").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var userId3 = Guid.NewGuid().ToString();
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                //Clear database
                context.Database.EnsureDeleted();

                var service = new ChatService(context);
                var fakeUser = new User
                {
                    Id = userId,
                    UserName = "fake@example.com",
                    Email = "fake@example.com",
                    Name = "Fake 1",
                    EmailConfirmed = true
                };
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Email = "fake2@example.com",
                    Name = "Fake 2",
                    EmailConfirmed = true
                };
                var fakeUser3 = new User
                {
                    Id = userId3,
                    UserName = "fake3@example.com",
                    Email = "fake3@example.com",
                    Name = "Fake 3",
                    EmailConfirmed = true
                };

                context.Users.Add(fakeUser);
                context.Users.Add(fakeUser2);
                context.Users.Add(fakeUser3);
                await context.SaveChangesAsync();

                var newChat = new ChatRoom
                {
                    ChatRoomId = id,
                    ChatRoomName = "Migos"
                };

                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser
                };

                UserChatRoom otherChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser2
                };

                UserChatRoom otherChat2 = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser3
                };

                newChat.Members.Add(userChat);
                newChat.Members.Add(otherChat);
                newChat.Members.Add(otherChat2);

                await service.CreateChatRoom(newChat);
            }

            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.ChatRooms.CountAsync();
                Assert.Equal(0, itemsInDatabase);
            }
        }

        /// <summary>
        /// Testa se é possível criar uma equipa com 3 membros sem fornecer uma equipa.
        /// Não deve ser possível
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateChatRoomForTeamTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewChatRoom").Options;
            //Generate a guid to use it
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var userId3 = Guid.NewGuid().ToString();
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                //Clear database
                context.Database.EnsureDeleted();

                var service = new ChatService(context);
                var fakeUser = new User
                {
                    Id = userId,
                    UserName = "fake@example.com",
                    Email = "fake@example.com",
                    Name = "Fake 1",
                    EmailConfirmed = true
                };
                var fakeUser2 = new User
                {
                    Id = userId2,
                    UserName = "fake2@example.com",
                    Email = "fake2@example.com",
                    Name = "Fake 2",
                    EmailConfirmed = true
                };
                var fakeUser3 = new User
                {
                    Id = userId3,
                    UserName = "fake3@example.com",
                    Email = "fake3@example.com",
                    Name = "Fake 3",
                    EmailConfirmed = true
                };

                var team = new Team
                {
                    TeamId = Guid.NewGuid(),
                    Description = "Test",
                    TeamName = "Test",
                    LanguageLocalizationId = Guid.NewGuid(),
                    OwnerId = Guid.Parse(userId),
                    Members = new List<User> { fakeUser2, fakeUser3 },
                    Status = TeamStatus.Open
                };

                context.Users.Add(fakeUser);
                context.Users.Add(fakeUser2);
                context.Users.Add(fakeUser3);
                await context.SaveChangesAsync();

                var newChat = new ChatRoom
                {
                    ChatRoomId = id,
                    ChatRoomName = "Migos",
                    TeamId = team.TeamId,
                    Team = team
                };

                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser
                };

                UserChatRoom otherChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser2
                };

                UserChatRoom otherChat2 = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = fakeUser3
                };

                newChat.Members.Add(userChat);
                newChat.Members.Add(otherChat);
                newChat.Members.Add(otherChat2);

                await service.CreateChatRoom(newChat);
            }

            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.ChatRooms.CountAsync();
                Assert.Equal(1, itemsInDatabase);

                var item = await context.ChatRooms.Include(c => c.Members).Include(c => c.Team).FirstAsync();
                Assert.Equal("Migos", item.ChatRoomName);

                // ChatRoom should have defined Id
                Assert.Equal(id, item.ChatRoomId);

                // ChatRoom should have only 2 members
                Assert.Equal(3, item.Members.Count);

                // Should be empty - no messages
                Assert.False(item.Messages.Any());

                // Team should be setted
                Assert.NotNull(item.Team);

                // Team has the name setted
                Assert.Equal("Test", item.Team.TeamName);
            }
        }
    }
}

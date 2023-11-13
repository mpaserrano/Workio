using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
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
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Xunit.Abstractions;

namespace Workio.Tests.Services
{
    public class UserServiceTest
    {
        private User user;
        private User user2;
        private SkillModel skill;
        private SkillModel skill2;
        private ExperienceModel exp;
        private ExperienceModel exp2;
        private BlockedUsersModel block;
        private Localization language;
        private readonly ITestOutputHelper output;


        private Mock<UserManager<User>> userManagerMock;
        private Mock<SignInManager<User>> signInManagerMock;
        private Mock<IHttpContextAccessor> httpMock;
        private Mock<IUserService> userServiceMock;
        private Mock<IRatingService> ratingServiceMock;
        private HttpContextAccessor httpContextAccessor;

        public UserServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void SetUp()
        {
            language = new Localization()
            {
                IconName = "pt",
                Code = "pt",
                Language = "Português",
                LocalizationId = Guid.NewGuid()
            };
            skill = new SkillModel()
            {
                SkillId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9746"),
                Name = "SkillName",
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9744"),
                Endorsers = new List<User>()
            };

            skill2 = new SkillModel()
            {
                SkillId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9747"),
                Name = "SkillName",
                UserId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9745"),
                Endorsers = new List<User>()
            };

            exp = new ExperienceModel()
            {
                ExperienceId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9748"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                WorkTitle = "Work Title",
                Company = "IPS",
                Description= "Description",
                StartDate = DateTime.Now
            };

            exp2 = new ExperienceModel()
            {
                ExperienceId = new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9749"),
                UserId = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                WorkTitle = "Work Title",
                Company = "IPS",
                Description = "Description",
                StartDate = DateTime.Now
            };

            user = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9744",
                UserName = "Test@123.com",
                Email = "Test@123.com",
                Name = "Teste user",
                EmailConfirmed = true,
                Skills = new List<SkillModel> { skill },
                Experiences= new List<ExperienceModel> { exp },
                LanguageId = language.LocalizationId,
                Language = language
                
            };

            user2 = new User
            {
                Id = "a2463fbc-1f6b-470d-b40d-daf9e0bc9745",
                UserName = "Test@1234.com",
                Email = "Test@1234.com",
                Name = "Teste user",
                EmailConfirmed = true,
                LanguageId = language.LocalizationId,
                Language = language
            };

            block = new BlockedUsersModel
            {
                Id = new Guid("5a44a01f-0628-4839-b991-a55aaa87dce5"),
                SourceUser = user,
                SourceUserId = user.Id,
                BlockedUser = user2,
                BlockedUserId = user2.Id,
                BlockDateTime = DateTime.Now,
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
            userClaims.Setup(x => x.CreateAsync(user)).ReturnsAsync(Mock.Of<ClaimsPrincipal>);
            userClaims.Setup(x => x.CreateAsync(user2)).ReturnsAsync(Mock.Of<ClaimsPrincipal>);

            userServiceMock = new Mock<IUserService>();
            ratingServiceMock = new Mock<IRatingService>();

        }

        [Fact]
        public async Task UserService_GetUsers()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetUsers").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUsersAsync();

                foreach(User item in items)
                {
                    // Team should have defined Id
                    Assert.Equal("a2463fbc-1f6b-470d-b40d-daf9e0bc9744", item.Id);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_GetUser()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetUser").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var item = await service.GetUser(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal("a2463fbc-1f6b-470d-b40d-daf9e0bc9744", item.Id);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_GetUserSkill()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetUserSkill").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                foreach(SkillModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal(skill.SkillId, item.SkillId);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_AddSkill()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddSkill").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user2);
                context.SaveChanges();

                service.AddSkill(new Guid(user2.Id), skill2);
                
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9745")));

                foreach (SkillModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal(skill2, item);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_EditSkill()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EditSkill").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();

                skill.Name = "AAAAAA";
                bool changed = await service.EditSkill(skill);

                Assert.True(changed);
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                foreach (SkillModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal("AAAAAA", item.Name);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_DeleteSkill()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_DeleteSkill").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();

                
            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                bool deleteSkill = await service.DeleteSkill(skill.SkillId);

                Assert.True(deleteSkill);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(0, items.Count());

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_GetUserExperience()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetUserExperience").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserExperience(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(1, items.Count());

                foreach(ExperienceModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal(exp.ExperienceId, item.ExperienceId);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_AddExperience()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddExperience").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.Users.Add(user2);
                context.SaveChanges();

                

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                ExperienceModel e1 = await service.AddExperience(exp2);
                Assert.NotNull(e1);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetUserExperience(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));
                Assert.NotNull(items);
                Assert.Equal(2, items.Count());

                foreach (ExperienceModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal("IPS", item.Company);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_EditExperience()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_EditExperience").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();


            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                exp.WorkTitle = "student";
                bool changed = await service.EditExperience(exp);
                Assert.True(changed);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserExperience(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(1, items.Count());

                foreach (ExperienceModel item in items)
                {
                    // Team should have defined Id
                    Assert.Equal(exp.ExperienceId, item.ExperienceId);

                    // Experience should have new work title
                    Assert.Equal("student", item.WorkTitle);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_DeleteExperience()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_DeleteExperience").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();

                

            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                bool deleted = await service.DeleteExperience(exp.ExperienceId);
                Assert.True(deleted);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);


                var items = await service.GetUserExperience(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(0, items.Count());

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_AddEndorsement()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddEndorsement").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.Users.Add(user2);
                context.SaveChanges();



            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);

                SkillModel sk = await service.AddEndorsement(new Guid("a2463fbc-1f6b-470d-b40d-daf9e0bc9750"));
                
                // Verify null if doesnt exist
                Assert.Null(sk);

                sk = await service.AddEndorsement(skill.SkillId);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(1, items.Count());

                foreach(SkillModel item in items)
                {
                    Assert.Contains(user2, item.Endorsers);
                }

                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_RemoveEndorsement()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_RemoveEndorsement").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.Users.Add(user2);
                context.SaveChanges();



            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);
                SkillModel sk = await service.AddEndorsement(skill.SkillId);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);


                var items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(1, items.Count());

                foreach (SkillModel item in items)
                {
                    Assert.Contains(user2, item.Endorsers);
                }

                sk = await service.RemoveEndorsement(skill.SkillId);

                items = await service.GetUserSkills(new Guid(("a2463fbc-1f6b-470d-b40d-daf9e0bc9744")));

                Assert.Equal(1, items.Count());

                foreach (SkillModel item in items)
                {
                    Assert.DoesNotContain(user2, item.Endorsers);
                }


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_IsBlockedByUser()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_IsBlockedByUser").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.Users.Add(user2);
                context.BlockedUsersModel.Add(block);
                context.SaveChanges();



            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user2);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);

                bool blocked = await service.IsBlockedByUser(new Guid(user.Id));
                
                Assert.True(blocked);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }


        [Fact]
        public async Task UserService_IsAlreadyBlocked()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_IsAlreadyBlocked").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.Users.Add(user2);
                context.BlockedUsersModel.Add(block);
                context.SaveChanges();



            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(2, itemsInDatabase);

                bool alreadyBlocked = await service.IsAlreadyBlocked(new Guid(user2.Id));

                Assert.True(alreadyBlocked);


                //Clear database
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task UserService_IsCurrentUserModAdminEntity()
        {
            //Arrange
            SetUp();

            //ApplicationDB options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_IsAlreadyBlocked").Options;

            //Act
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {

                //Clear database
                context.Database.EnsureDeleted();

                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);

                context.Users.Add(user);
                context.SaveChanges();



            }

            //Assert
            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {

                var service = new UserService(context, userManagerMock.Object, httpContextAccessor);


                userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

                var itemsInDatabase = await context.Users.CountAsync();

                // Verify amount of teams
                Assert.Equal(1, itemsInDatabase);

                bool MAE = await service.IsCurrentUserModAdminEntity();
                Assert.False(MAE);

                userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);
                MAE = await service.IsCurrentUserModAdminEntity();
                Assert.True(MAE);

                //Clear database
                context.Database.EnsureDeleted();
            }
        }



    }
}

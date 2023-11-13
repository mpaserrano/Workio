using Microsoft.AspNetCore.Identity;
using Workio.Models;

namespace Workio.Data
{
    public class SeedData
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public SeedData(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var defaultLanguage = new Localization()
            {
                LocalizationId = Guid.NewGuid(),
                Language = "English",
                IconName = "UK",
                Code = "en"
            };
            // Create roles
            string[] roleNames = { "Admin", "Entity", "User", "Mod" };
            foreach (string roleName in roleNames)
            {
                bool roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (!_context.Localizations.Any())
            {
                List<Localization> listOfLocalizations = new List<Localization>()
                {
                    new Localization()
                    {
                        LocalizationId = Guid.NewGuid(),
                        Language = "Português",
                        IconName = "Portugal",
                        Code = "pt"
                    },
                    defaultLanguage
                };

                _context.Localizations.AddRange(listOfLocalizations);
            }

            List<Guid> reasonsIds = new List<Guid>()
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };
                
            if(!_context.ReportReason.Any())
            {
                List<ReportReason> listOfReportsReasons = new List<ReportReason>()
                {
                    new ReportReason()
                    {
                        Id = reasonsIds[0],
                        Reason = "Bad Name",
                        ReasonType = ReasonType.User
                    },
                    new ReportReason()
                    {
                        Id = reasonsIds[1],
                        Reason = "NSFW Profile Picture",
                        ReasonType = ReasonType.User
                    },
                    new ReportReason()
                    {
                        Id = reasonsIds[2],
                        Reason = "Bad Name",
                        ReasonType = ReasonType.Team
                    },
                    new ReportReason()
                    {
                        Id = reasonsIds[3],
                        Reason = "Team members",
                        ReasonType = ReasonType.Team
                    },
                    new ReportReason()
                    {
                        Id = reasonsIds[4],
                        Reason = "Bad Name",
                        ReasonType = ReasonType.Event
                    },
                    new ReportReason()
                    {
                        Id = reasonsIds[5],
                        Reason = "Event doesnt exist",
                        ReasonType = ReasonType.Event
                    }
                };

                _context.ReportReason.AddRange(listOfReportsReasons);
            }

            if (!_context.ReportReasonLocalizations.Any())
            {
                List<ReportReasonLocalization> listOfReportsReasonsLocalizations = new List<ReportReasonLocalization>()
                {
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Mau Nome",
                        ReportId = reasonsIds[0]
                    },
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Imagem de perfil Imprópria",
                        ReportId = reasonsIds[1]
                    },
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Mau Nome",
                        ReportId = reasonsIds[2]
                    },
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Membros da equipa",
                        ReportId = reasonsIds[3]
                    },
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Mau Nome",
                        ReportId = reasonsIds[4]
                    },
                    new ReportReasonLocalization()
                    {
                        Id = Guid.NewGuid(),
                        LocalizationCode = "pt",
                        Description = "Evento não existe",
                        ReportId = reasonsIds[5]
                    }
                };

                _context.ReportReasonLocalizations.AddRange(listOfReportsReasonsLocalizations);
            }

            await _context.SaveChangesAsync();

            // Create default admin account
            string adminEmail = "support@workio.space";
            string adminPassword = "!Teste123";
            string adminRole = "Admin";
            string adminName = "Admin User";
            await CreateUserWithRole(adminEmail, adminPassword, adminRole, adminName, defaultLanguage);

            // Create default user account
            string userEmail = "ccroyale34@gmail.com";
            string userPassword = "!Teste123";
            string userRole = "User";
            string userName = "User";
            await CreateUserWithRole(userEmail, userPassword, userRole, userName, defaultLanguage);

            // Create user accounts for automation test purposes
            string a1userEmail = "testeautomacao1@mail.com";
            string a1userPassword = "!Teste123";
            string a1userRole = "User";
            string a1userName = "Teste Automacao User 1";
            await CreateUserWithRole(a1userEmail, a1userPassword, a1userRole, a1userName, defaultLanguage);

            // Create user accounts for automation test purposes
            string a2userEmail = "testeautomacao2@mail.com";
            string a2userPassword = "!Teste123";
            string a2userRole = "User";
            string a2userName = "Teste Automacao User 2";
            await CreateUserWithRole(a2userEmail, a2userPassword, a2userRole, a2userName, defaultLanguage);

            // Create user accounts for automation test purposes
            string a3userEmail = "testeautomacao3@mail.com";
            string a3userPassword = "!Teste123";
            string a3userRole = "User";
            string a3userName = "Teste Automacao User 3";
            await CreateUserWithRole(a3userEmail, a3userPassword, a3userRole, a3userName, defaultLanguage);

            // Create default entity account
            string entityEmail = "CE@gmail.com";
            string EntityPassword = "!Teste123";
            string entityRole = "Entity";
            string entityName = "Entity Name";
            await CreateUserWithRole(entityEmail, EntityPassword, entityRole, entityName, defaultLanguage);

            // Create default mod account
            string modEmail = "mod@gmail.com";
            string modPassword = "!Teste123";
            string modRole = "Mod";
            string modName = "Mod User";
            await CreateUserWithRole(modEmail, modPassword, modRole, modName, defaultLanguage);
        }

        private async Task CreateUserWithRole(string email, string password, string roleName, string name, Localization lang)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                Name = name,
                EmailConfirmed = true,
                LanguageId = lang.LocalizationId,
                Language = lang
            };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}

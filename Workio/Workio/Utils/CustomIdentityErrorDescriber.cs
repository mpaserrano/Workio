using Microsoft.AspNetCore.Identity;
using Workio.Services;

namespace Workio.Utils
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        private CommonLocalizationService _localizationService;

        public CustomIdentityErrorDescriber(CommonLocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = _localizationService.Get("An unknown failure has occurred.") }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = _localizationService.Get("Incorrect password.") }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = _localizationService.Get("Invalid token.") }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizationService.Get("A user with this login already exists.") }; }
        public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = string.Format("User name {0} is invalid, can only contain letters or digits.", userName) }; }
        public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(_localizationService.Get("Email {0} is invalid."), email) }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(_localizationService.Get("User Name {0} is already taken."), userName) }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = string.Format(_localizationService.Get("Email {0} is already taken."), email) }; }
        public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = string.Format(_localizationService.Get("Role name {0} is invalid."), role) }; }
        public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = string.Format(_localizationService.Get("Role name {0} is already taken."), role) }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizationService.Get("User already has a password set.") }; }
        public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizationService.Get("Lockout is not enabled for this user.") }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(_localizationService.Get("User already in role {0}."), role) }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = string.Format(_localizationService.Get("User is not in role {0}."), role) }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(_localizationService.Get("Passwords must be at least {0} characters."), length) }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizationService.Get("Passwords must have at least one non alphanumeric character.") }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizationService.Get("Passwords must have at least one digit ('0'-'9').") }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizationService.Get("Passwords must have at least one lowercase ('a'-'z').") }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizationService.Get("Passwords must have at least one uppercase ('A'-'Z').") }; }
    }
}

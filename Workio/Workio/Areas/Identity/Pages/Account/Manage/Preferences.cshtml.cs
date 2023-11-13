// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NToastNotify;
using Workio.Models;
using Workio.Services;
using Workio.Services.Interfaces;

namespace Workio.Areas.Identity.Pages.Account.Manage
{
    public class PrivacyModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IToastNotification _toastNotification;
        private readonly CommonLocalizationService _localizer;
        private readonly IUserService _userService;

        public PrivacyModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ChangePasswordModel> logger,
            IToastNotification toastNotification,
            CommonLocalizationService localizer,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _userService = userService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Descreve se o utilizador pode receber mensagens de outros utilizadores
            /// </summary>
            [Required]
            public bool IsDMOpen { get; set; }

            /// <summary>
            /// Descreve se o utilizador recebe notificações por email
            /// </summary>
            [Required]
            public bool SendEmailNotifications { get; set; }

            /// <summary>
            /// Descreve se o utilizador recebe notificações IRL
            /// </summary>
            [Required]
            public bool ReceiveIRLNotifications { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            Input = new InputModel
            {
                IsDMOpen = user.Preferences.IsDMOpen,
                SendEmailNotifications = user.Preferences.SendEmailNotifications,
                ReceiveIRLNotifications = user.Preferences.ReceiveIRLNotifications
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (user.Preferences.UserId == null)
                user.Preferences.UserId = user.Id;

            user.Preferences.IsDMOpen = Input.IsDMOpen;
            user.Preferences.SendEmailNotifications = Input.SendEmailNotifications;
            user.Preferences.ReceiveIRLNotifications = Input.ReceiveIRLNotifications;

            await _userService.SetUserPreferences(user.Preferences);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer.Get("YourPrivacySettingsHaveBeenUpdated");
            return RedirectToPage();
        }


    }
}

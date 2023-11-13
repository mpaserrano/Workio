// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Workio.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Workio.Services.Interfaces;
using System.Text.Encodings.Web;
using Workio.Services.Email.Interfaces;

namespace Workio.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        IEmailService _emailService = null;
        [BindProperty]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        private readonly UserManager<User> _userManager;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }

            User user = await _userManager.FindByEmailAsync(Email);

            // if user not registered or bad input
            if((null == user ))
            {
                ModelState.AddModelError(string.Empty, "Please check email. Retry");
                return Page();
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            // encode as base 64
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var email_encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email));

            // create the link and email or send by sms 
            // TODO: configure email

            var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email_encoded },
                    protocol: Request.Scheme);

            _emailService.SendRecoverPasswordEmail(user, callbackUrl);

            //ModelState.AddModelError(string.Empty, "We have emailed a link");

            return RedirectToPage("./ForgotPasswordConfirmation");


            // While email service not implemented
            // Redirect directly to Reset Password Page
            //return RedirectToPage("ResetPassword", new {are ="auth", code = code});

        }
    }
}

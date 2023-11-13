// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Workio.Models;
using Workio.Services;

namespace Workio.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CommonLocalizationService _stringLocalizer;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IWebHostEnvironment webHostEnvironment,
            CommonLocalizationService stringLocalizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _stringLocalizer = stringLocalizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

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
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     User's name
            /// </summary>
            [Required(ErrorMessage = "The Name field is required")]
            [DataType(DataType.Text)]
            [Display(Name = "Name")]
            [MaxLength(64, ErrorMessage = "Name must be max {0} characters long.")]
            public string Name { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            /// <summary>
            ///     User's bio
            /// </summary>
            [DataType(DataType.Text)]
            [Display(Name = "About Me")]
            [MaxLength(128, ErrorMessage = "About Me must have max {0} characters long.")]
            public string AboutMe { get; set; }
            /// <summary>
            ///     User's GitHub Handle
            /// </summary>
            [DataType(DataType.Text)]
            [Display(Name = "GitHub")]
            public string GitHub { get; set; }
            /// <summary>
            ///     User's LinkedIn Handle
            /// </summary>
            [DataType(DataType.Text)]
            [Display(Name = "LinkedIn")]
            public string LinkedIn { get; set; }

            [Display(Name = "Profile Picture")]
            [ValidateImage(ErrorMessage = "Invalid image file. Only .png, .jpg, .jpeg, .gif, and .tif files are allowed.")]
            public IFormFile ProfilePicture { get; set; }

            public string? ProfilePictureName { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            //null verifications
            var userAboutMe = "";
            if (user.AboutMe != null)
            {
                userAboutMe = user.AboutMe;
            }

            var userGitHub = "";
            if (user.GitHubAcc != null)
            {
                userGitHub = user.GitHubAcc;
            }

            var userLinkedIn = "";
            if (user.LinkedInAcc != null)
            {
                userLinkedIn = user.LinkedInAcc;
            }


            /// Removes URL part leaving only the handle
            if (userGitHub.Contains("https://www.github.com/"))
            {
                userGitHub = userGitHub.Remove(0, 23);
            }

            /// Removes URL part leaving only the handle
            if (userLinkedIn.Contains("https://www.linkedin.com/in/"))
            {
                userLinkedIn = userLinkedIn.Remove(0, 28);
            }
            FormFile file;
            file = null;

            Input = new InputModel
            {
                Name = user.Name,
                PhoneNumber = phoneNumber,
                AboutMe = userAboutMe,
                GitHub = userGitHub,
                LinkedIn = userLinkedIn,
                ProfilePicture = file,
                ProfilePictureName = user.ProfilePicture
        };
    }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.Name != user.Name)
            {
                user.Name = Input.Name;
            }

            if (Input.AboutMe != user.AboutMe)
            {
                user.AboutMe = Input.AboutMe;
            }

            //Se GitHub introduzido com o URL
            if (Input.GitHub != null && Input.GitHub.ToLower().Contains("github.com"))
            {
                user.GitHubAcc = Input.GitHub.Split(new char[] {'/'}).Last();
            }
            else //Apenas o nome de utilizador do github
            {
                user.GitHubAcc = Input.GitHub;
            }

            // Se o linkedin introduzido com o URL
            if (Input.LinkedIn != null && Input.LinkedIn.ToLower().Contains("linkedin.com"))
            {
                
                // Se o URL mete uma / depois do nome do utilizador
                if (Input.LinkedIn.EndsWith("/"))
                {
                    //Tira a barra
                    user.LinkedInAcc = Input.LinkedIn.Remove(Input.LinkedIn.Length - 1);
                    //Obtem nome
                    user.LinkedInAcc = user.LinkedInAcc.Split(new char[] { '/' }).Last();
                }
                else // Obtem nome diretamente
                {
                    user.LinkedInAcc = Input.LinkedIn.Split(new char[] { '/' }).Last();
                }
            }
            else // Se apenas o nome de utilizador
            {
                user.LinkedInAcc = Input.LinkedIn;
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                var _allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".tif" };
                if (_allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                {
                    // Save image to folder
                    string filename = user.Id.ToString().Replace("-", "") + Path.GetExtension(Input.ProfilePicture.FileName);
                    var filePath = _webHostEnvironment.WebRootPath + @"\pfp\" + filename;
                    var stream = new FileStream(filePath, FileMode.Create);
                    await Input.ProfilePicture.CopyToAsync(stream);
                    stream.Close();
                    user.ProfilePicture = filename;
                }
                else
                {
                    return RedirectToPage();
                }
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _stringLocalizer.Get("Your profile has been updated");
            return RedirectToPage();
        }

    }
}

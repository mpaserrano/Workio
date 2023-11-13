using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        public HomeController(ILogger<HomeController> logger, SignInManager<User> signInManager,
            UserManager<User> userManager, IUserService userService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ChangeCulture(string culture, string returnUrl)
        {
            // Set the current culture to the specified culture
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            // Set the response cookie to persist the culture across requests
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (_signInManager.IsSignedIn(User))
            {
                await _userService.ChangeSavedLanguage(culture);
            }

            return LocalRedirect(returnUrl);
        }

        private Guid CurrentUserId()
        {
            if (!_signInManager.IsSignedIn(User)) return Guid.Empty;
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
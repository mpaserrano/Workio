using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.Globalization;
using Workio.Configurations;
using Workio.Data;
using Workio.Models;
using Workio.Services.Connections;
using Workio.Services;
using Workio.Services.Email;
using Workio.Services.Email.Interfaces;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using Workio.Services.Search;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Workio.Services.Teams;
using Workio.Services.LocalizationServices;
using Microsoft.Extensions.DependencyInjection;
using Workio.Services.Events;
using Workio.Services.Admin.Log;
using Workio.Services.RequestEntityStatusServices;
using Workio.Services.Matchmaking;
using Workio.Services.Admin;
using Workio.Services.Admin.Teams;
using Workio.Services.Admin.Events;
using Workio.Hubs;
using Workio.Managers.Connections;
using Workio.Managers.Notifications;
using Workio.Services.Notifications;
using Workio.Services.BackgroundServices;
using Workio.Services.Chat;
using Workio.Utils;
using Workio.Resources;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddRoles<IdentityRole>(); // Add this line to add the role manager service

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IReportReasonService, ReportReasonService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ITeamsService, TeamsService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IEventsService, EventService>();
builder.Services.AddScoped<ILogsService, LogsService>();
builder.Services.AddScoped<IRequestEntityStatusService, RequestEntityStatusService>();
builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminTeamService, AdminTeamService>();
builder.Services.AddScoped<IAdminEventService, AdminEventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddSingleton<PeriodicHostedService>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<PeriodicHostedService>());




//Email
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

//External Auth Services
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
        googleOptions.ClaimActions.MapJsonKey("image", "picture", "url");
        googleOptions.Events.OnRemoteFailure = (context) =>
        {
            context.Response.Redirect("/Identity/Account/Register");
            context.HandleResponse();
            return System.Threading.Tasks.Task.CompletedTask;
        };
    })
    .AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
        microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
        microsoftOptions.SaveTokens = true;
        microsoftOptions.Scope.Add("User.Read");
        microsoftOptions.ClaimActions.MapJsonKey("image", "picture");
        microsoftOptions.Events.OnRemoteFailure = (context) =>
        {
            context.Response.Redirect("/Identity/Account/Register");
            context.HandleResponse();
            return System.Threading.Tasks.Task.CompletedTask;
        };
    })
    .AddFacebook(fbOptions =>
    {
        fbOptions.AppId = configuration["Authentication:Facebook:AppId"];
        fbOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
        fbOptions.SaveTokens = true;
        fbOptions.Scope.Add("email");
        fbOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        fbOptions.Scope.Add("public_profile");
        fbOptions.AccessDeniedPath = "/AccessDeniedPathInfo";
        fbOptions.Events.OnRemoteFailure = (context) =>
        {
            context.Response.Redirect("/Identity/Account/Register");
            context.HandleResponse();
            return System.Threading.Tasks.Task.CompletedTask;
        };
        
    });

//Add localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddSignalR();

builder.Services.AddScoped<INotificationManager, NotificationManager>();

builder.Services.AddMvc()
      .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
      .AddDataAnnotationsLocalization();
builder.Services.AddSingleton<CommonLocalizationService>();

const string defaultCulture = "en";
var supportedCultures = new[]
{
    new CultureInfo(defaultCulture),
    new CultureInfo("pt")
};
builder.Services.Configure<RequestLocalizationOptions>(options => {
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddMvc()
    .AddNToastNotifyToastr();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roles and users with roles to the database
    var seedData = new SeedData(userManager, roleManager, configuration, context);
    seedData.InitializeAsync().Wait();
}

app.UseNToastNotify();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHub<NotificationHub>("/notificationsHub", options =>
{
    options.CloseOnAuthenticationExpiration = true;
});

app.MapHub<ChatHub>("/chatHub", options =>
{
    options.CloseOnAuthenticationExpiration = true;
});

app.Run();

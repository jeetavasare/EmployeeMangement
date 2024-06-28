using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using NLog.Web.LayoutRenderers;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");
//logger.Warn("init warn");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Host.UseNLog();

builder.Services.AddMvc(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddAuthorization(options
    =>
    {
    options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role").RequireClaim("Create Role"));
    options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
    }
    
    );

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Administration/AccessDenied";
});
builder.Services.AddScoped<IEmployeeRepository,SQLEmployeeRepository>();
builder.Services.AddDbContextPool<AppDbContext>
    (options => options.UseSqlServer(config.GetConnectionString("EmployeeDBConnection")));

builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options
    => { options.Password.RequiredLength = 7; options.Password.RequireUppercase = false; }
    );
// or give the options in AddIdentity function itself
//builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => { options.Password.RequiredLength = 9; options.Password.RequireUppercase = false; })
//    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

var env = app.Environment;


if (env.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseExceptionHandler("/Error");
}
//app.Use(async (context, next) =>
//{
//    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
//    var logger = loggerFactory.CreateLogger("MiddlewareLogger");

//    logger.LogInformation("MW1: Incoming request");
//    await next();
//    logger.LogInformation("MW1: Outgoing response");
//});


//app.Use(async (context, next) =>
//{
//    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
//    var logger = loggerFactory.CreateLogger("MiddlewareLogger");

//    logger.LogInformation("MW2: Incoming request");
//    await next();
//    logger.LogInformation("MW2: Outgoing response");
//});

//app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();
app.UseAuthorization();
//app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

//custom implementation of mapdefaultcontrollerroute, equivalent to below
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

//below dosen't work
//app.UseMvc(routes =>
//{

//    routes.MapRoute("default", "{controller=Home}/{action=Details}/{id=2}");
//});

//use attribute routing only, overrides default controller route
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // This will map attribute-routed controllers
});
//app.UseFileServer();

//app.MapGet("/", () => "Hello World!");
app.MapGet("/bye", () => System.Diagnostics.Process.GetCurrentProcess().ProcessName);
app.MapGet("/displayconfig", () => config["Mykey"]);
//app.Run();
//app.Use(async (context, next) =>
//{
//    await next(); // Call the next middleware in the pipeline

//    if (context.Response.StatusCode == 404) // If no response was found
//    {
//        context.Response.ContentType = "text/plain";
//        await context.Response.WriteAsync(
//            "Terminal Middleware:Oops that's not a valid URL for this application"); //all other responses no in map.get
//    }
//});
app.Run();
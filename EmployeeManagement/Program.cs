using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddMvc();

builder.Services.AddScoped<IEmployeeRepository,SQLEmployeeRepository>();
builder.Services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(config.GetConnectionString("EmployeeDBConnection")));


var app = builder.Build();

var env = app.Environment;


if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
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


app.UseRouting();
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
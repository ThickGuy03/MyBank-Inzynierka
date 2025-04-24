using Inzynierka.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Inzynierka.Models;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF1cWGhIfEx1RHxQdld5ZFRHallYTnNWUj0eQnxTdEFiW35ZcHNUQ2NfUERxWw==");

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeFolder("/Identity/Transactions");
        options.Conventions.AuthorizeAreaFolder("Identity", "/Category");
    })
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<RecurringTransactionJob>();

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity != null && user.Identity.IsAuthenticated)
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/Identity/Dashboard");
            return;
        }
    }
    else
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/Index");
            return;
        }
    }

    await next.Invoke();
});

RecurringJob.AddOrUpdate<RecurringTransactionJob>(
    "ProcessRecurringTransactions",
    job => job.ProcessRecurringTransactions(),
    Cron.Daily(22));

app.UseHangfireDashboard("/hangfire");
app.MapRazorPages();
app.Run();
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var services = builder.Services;

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=db/app.db")
);

services.AddSingleton<TimeService>();
services.AddScoped<IQuestionProvider, ConfigQuestionsProvider>();

services.AddTransient<MessageProcessor>();
services.AddTransient<ResponseGenerator>();
services.AddTransient<ProcessMailTask>();
services.AddTransient<IBackgroundProcess, ProcessMonitor>(
    x => new ProcessMonitor(checkInterval: TimeSpan.FromSeconds(10))
);
services.AddHangfire(configuration =>
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSQLiteStorage("app.db")
        .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(10))
        .UseJobsLogger()
);

services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default" };
});

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

RecurringJob.AddOrUpdate(
    "ProcessUnreadMessages",
    (ProcessMailTask t) => t.Process(),
    Cron.Minutely
);

app.Run();

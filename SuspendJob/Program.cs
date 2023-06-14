using Hangfire;
using Hangfire.SqlServer;
using SuspendJob;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
{
    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
    QueuePollInterval = TimeSpan.Zero,
    UseRecommendedIsolationLevel = true,
    DisableGlobalLocks = true
}));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

// Add framework services.
builder.Services.AddMvc();
var app = builder.Build();
using var scopeProvider = app.Services.CreateScope();
var recurringJobManager = scopeProvider.ServiceProvider.GetRequiredService<IRecurringJobManager>();
// this is sample job
recurringJobManager.RemoveIfExists("powerfuljob");
recurringJobManager.AddOrUpdate("powerfuljob", () => Console.Write("Powerful!"), Cron.Minutely);
recurringJobManager.Trigger("powerfuljob");

app.UseStaticFiles();

app.UseHangfireDashboard();
app.UseHangfireSuspendPage();


app.MapGet("/", () => "Hello World!");
app.UseRouting();

app.UseEndpoints(routeBuilder =>
{
    routeBuilder.MapHangfireDashboard();
});
app.Run();
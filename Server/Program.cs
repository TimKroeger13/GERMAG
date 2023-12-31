using GERMAG.DataModel.Database;
using GERMAG.Server.Core.Configurations;
using GERMAG.Server.DataPulling;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var options = builder.Configuration.GetSection(ConfigurationOptions.Configuration).Get<ConfigurationOptions>()
    ?? throw new Exception("Configuration could not be found");
IEnviromentConfiguration configuration = builder.Environment.IsDevelopment() ?
    new DebugConfiguration(options) : new ReleaseConfiguration(options);
builder.Services.AddSingleton(configuration);
builder.Services.AddTransient<HttpClient>();
builder.Services.AddTransient<IDataFetcher, DataFetcher>();
builder.Services.AddTransient<IDatabaseUpdater, DatabaseUpdater>();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(configuration.DatabaseConnection, npg =>
    {
        npg.UseNetTopologySuite();
        npg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
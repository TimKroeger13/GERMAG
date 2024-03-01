using GERMAG.DataModel.Database;
using GERMAG.Server;
using GERMAG.Server.Core.Configurations;
using GERMAG.Server.DataPulling;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
builder.Services.AddHttpClient(HttpClients.LongTimeoutClient, o => o.Timeout = TimeSpan.FromMinutes(120));
builder.Services.AddHttpClient(HttpClients.Default);
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.GetAllowed, policy => policy.WithMethods("GET").AllowAnyHeader().AllowAnyOrigin());
});
builder.Services.AddTransient<IDataFetcher, DataFetcher>();
builder.Services.AddTransient<IDatabaseUpdater, DatabaseUpdater>();
builder.Services.AddTransient<IJsonDeserialize, JsonDeserialize>();
builder.Services.AddTransient<ICreateReportAsync, CreateReport>();
builder.Services.AddTransient<IParameterDeserialator, ParameterDeserialator>();
builder.Services.AddTransient<IFindAllParameterForCoordinate, FindAllParameterForCoordinate>();
builder.Services.AddTransient<ICreateReportStructure, CreateReportStructure>();
builder.Services.AddTransient<IReceiveLandParcel, ReceiveLandParcel>();
var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.DatabaseConnection);
var dataSource = dataSourceBuilder.ConfigureAndBuild();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(dataSource, npg =>
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
app.UseCors(CorsPolicies.GetAllowed);

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
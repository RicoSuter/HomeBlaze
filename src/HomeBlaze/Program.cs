using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Serilog;
using HomeBlaze.Host;
using HomeBlaze.Host.Logging;
using HomeBlaze.Host.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

var seqEndpoint = builder.Configuration.GetValue<string>("SeqEndpoint");
if (!string.IsNullOrEmpty(seqEndpoint))
{
    var serilogLogger = new LoggerConfiguration()
        .WriteTo.Seq(seqEndpoint)
        .Enrich.WithProperty("ApplicationName", "HomeBlaze")
        .Enrich.WithProperty("MachineName", Environment.MachineName)
        .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
        .CreateLogger();

    builder.Logging.AddSerilog(serilogLogger);
}

builder.Logging
    .AddConsole()
    .AddProvider(new MemoryLoggerProvider());

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.WriteIndented = true)
    .AddApplicationPart(typeof(ThingsController).Assembly);

builder.Services.AddServerSideBlazor();

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHomeBlaze(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app
    .UseRouting()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Serilog;
using HomeBlaze.Host;
using HomeBlaze.Host.Logging;

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
        .CreateLogger();

    builder.Logging.AddSerilog(serilogLogger);
}

builder.Logging.AddProvider(new MemoryLoggerProvider());

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

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
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
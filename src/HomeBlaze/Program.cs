using Microsoft.AspNetCore.Hosting.StaticWebAssets;

using MudBlazor.Services;
using Namotion.Storage;

using HomeBlaze;
using HomeBlaze.Abstractions.Services;
using Namotion.NuGetPlugins;
using Serilog;
using Namotion.Storage.Azure.Storage.Blob;
using HomeBlaze.Host;
using HomeBlaze.Host.Logging;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddHttpClient();

var storageType = builder.Configuration.GetValue<string>("Storage:Type");
switch (storageType)
{
    case "AzureBlobs":
        builder.Services.AddSingleton<IBlobContainer>(s => AzureBlobStorage
            .CreateFromConnectionString(builder.Configuration.GetValue<string>("Storage:ConnectionString"))
            .GetContainer(builder.Configuration.GetValue<string>("Storage:Container")));
        break;

    default:
        builder.Services.AddSingleton<IBlobContainer>(s => FileSystemBlobStorage
            .CreateWithBasePath(builder.Configuration.GetValue("Storage:Path", "Config")));
        break;
}


builder.Services.AddSingleton<IThingStorage, ThingRepository>();

builder.Services.AddSingleton<ITypeManager, TypeManager>();

builder.Services.AddSingleton<IThingManager, ThingManager>();
builder.Services.AddHostedService(s => (ThingManager)s.GetRequiredService<IThingManager>());

builder.Services.AddSingleton<IStateManager, StateManager>();
builder.Services.AddHostedService(s => (StateManager)s.GetRequiredService<IStateManager>());

builder.Services.AddSingleton<IEventManager, EventManager>();
builder.Services.AddHostedService(s => (EventManager)s.GetRequiredService<IEventManager>());

builder.Services.AddSingleton<IDynamicNuGetPackageLoader>(s =>
    new DynamicNuGetPackageLoader(NuGetPackageRepository.CreateForNuGetOrg(),
        s.GetRequiredService<ILogger<DynamicNuGetPackageLoader>>()));

builder.Services.AddHostedService<TimeMessagePublisher>();

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
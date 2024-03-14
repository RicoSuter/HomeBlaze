using HomeBlaze.Host.Controllers;
using HomeBlaze.Host.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HomeBlaze.Desktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });



            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif






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

            builder.Logging.AddProvider(new MemoryLoggerProvider());

            //StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            //builder.Services
            //    //.AddControllers()
            //    .AddJsonOptions(o => o.JsonSerializerOptions.WriteIndented = true)
            //    .AddApplicationPart(typeof(ThingsController).Assembly);

            //builder.Services.AddServerSideBlazor();

            //builder.Services.AddRazorPages();
            //builder.Services.AddHttpClient();
            //builder.Services.AddHomeBlaze(builder.Configuration);



            return builder.Build();
        }
    }
}

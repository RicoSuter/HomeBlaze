using FFMpegCore;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.RtspWebcam
{
    [DisplayName("RTSP Webcam")]
    [ThingSetup(typeof(RtspWebcamSetup), CanEdit = true)]
    public class RtspWebcam : PollingThing, IConnectedThing, ILastUpdatedProvider, IDisposable
    {
        public override string Title => InternalTitle + " (RTSP Webcam)";

        // Configuration

        [Configuration("title")]
        public string? InternalTitle { get; set; }

        [Configuration]
        public string? ServerUrl { get; set; }

        [Configuration]
        public string? UserName { get; set; }

        [Configuration(IsSecret = true)]
        public string? Password { get; set; }

        [Configuration]
        public bool UseFfmpeg { get; set; }

        // State

        public bool IsConnected { get; private set; }

        [State]
        public byte[]? Image { get; set; }

        [State]
        public string? ImageType { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(15);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public DateTimeOffset? LastUpdated { get; private set; }

        public RtspWebcam(IThingManager thingManager, ILogger<RtspWebcam> logger)
            : base(thingManager, logger)
        {
        }

        public async override Task PollAsync(CancellationToken cancellationToken)
        {
            if (ServerUrl != null)
            {
                var imageFile = "rtsp_temp_" + Id + ".jpg";

                Image = await Task.Run(async () =>
                {
                    var url = string.IsNullOrEmpty(UserName) ? 
                        ServerUrl : ServerUrl.Replace("rtsp://", $"rtsp://{UserName}:{Password}@");

                    var result = await FFMpegArguments
                        .FromUrlInput(new Uri(url))
                        .OutputToFile(imageFile, false, options => options
                            .WithFrameOutputCount(1)
                            .ForceFormat("image2")
                            .ForcePixelFormat("yuvj420p"))
                        .ProcessAsynchronously();

                    var image = result ? 
                        await File.ReadAllBytesAsync(imageFile, cancellationToken) : 
                        null;

                    File.Delete(imageFile);
                    return image;
                });

                if (Image != null)
                {
                    ImageType = "image/jpeg";
                    IsConnected = true;
                    LastUpdated = DateTimeOffset.Now;
                }
            }
        }
    }
}

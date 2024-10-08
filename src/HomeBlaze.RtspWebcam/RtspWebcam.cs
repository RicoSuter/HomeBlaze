﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;

namespace HomeBlaze.RtspWebcam
{
    [DisplayName("RTSP Webcam")]
    public class RtspWebcam : PollingThing,
        ICameraSensor,
        IConnectedThing, ILastUpdatedProvider, IIconProvider,
        IDisposable
    {
        private ILogger _logger;

        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(10);

        private const string ImageFormat = "image2";
        private const string PixelFormat = "yuvj420p";

        public override string Title => InternalTitle + " (RTSP Webcam)";
        
        public string IconName => "fa-solid fa-camera";

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
        public Image? Image { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(15);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public DateTimeOffset? LastUpdated { get; private set; }

        public RtspWebcam(IThingManager thingManager, ILogger<RtspWebcam> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public async override Task PollAsync(CancellationToken cancellationToken)
        {
            if (ServerUrl != null)
            {
                var bytes = await Task.Run(async () =>
                {
                    try
                    {
                        using var timeoutCts = new CancellationTokenSource(_connectionTimeout);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                        var url = string.IsNullOrEmpty(UserName) ?
                            ServerUrl : ServerUrl.Replace("rtsp://", $"rtsp://{UserName}:{Password}@");

                        using var outputStream = new MemoryStream();

                        var result = await FFMpegArguments
                            .FromUrlInput(new Uri(url), options => options
                                .WithArgument(new RtspTransportArgument("tcp")))
                            .OutputToPipe(new StreamPipeSink(outputStream), options => options
                                .WithFrameOutputCount(1)
                                .ForceFormat(ImageFormat)
                                .ForcePixelFormat(PixelFormat))
                            .CancellableThrough(cts.Token)
                            .ProcessAsynchronously(throwOnError: false);

                        cts.Token.ThrowIfCancellationRequested();
                        return outputStream.ToArray();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to retrieve image from RTSP stream.");
                        throw;
                    }
                });

                if (bytes != null)
                {
                    Image = new Image
                    {
                        Data = bytes,
                        MimeType = "image/jpeg"
                    };
                    LastUpdated = DateTimeOffset.Now;
                    IsConnected = true;
                }
                else
                {
                    Image = null;
                    IsConnected = false;
                }
            }
        }

        private class RtspTransportArgument : IArgument
        {
            private readonly string _transport;

            public string Text => "-rtsp_transport " + _transport;

            public RtspTransportArgument(string transport)
            {
                this._transport = transport;
            }
        }
    }
}

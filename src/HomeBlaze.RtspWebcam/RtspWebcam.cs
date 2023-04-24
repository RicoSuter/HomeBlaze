using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.RtspWebcam.RawFramesDecoding;
using HomeBlaze.RtspWebcam.RawFramesReceiving;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using RtspClientSharp;
using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.RtspWebcam
{
    [DisplayName("RTSP Webcam")]
    [ThingSetup(typeof(RtspWebcamSetup), CanEdit = true)]
    public class RtspWebcam : PollingThing, IConnectedThing, ILastUpdatedProvider, IDisposable
    {
        private RawFramesSource? _rawFramesSource;

        static RtspWebcam()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var architecture = Environment.Is64BitProcess ? "x64" : "x86";
            var platform = Environment.OSVersion.Platform == PlatformID.Unix ? "Unix" : "Win";

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                "/Libs/" + platform + "/" + architecture + "/" + libraryName;

            if (File.Exists(path))
            {
                return NativeLibrary.Load(path, assembly, searchPath);
            }

            // Otherwise, fallback to default import resolver.
            return IntPtr.Zero;
        }

        public override string Id => "rtsp.webcam." + InternalId;

        public override string Title => InternalTitle + " (RTSP Webcam)";

        // Configuration
        [Configuration("id")]
        public string InternalId { get; set; } = Guid.NewGuid().ToString();

        [Configuration("title")]
        public string? InternalTitle { get; set; }

        [Configuration]
        public string? ServerUrl { get; set; }

        [Configuration]
        public string? UserName { get; set; }

        [Configuration(IsSecret = true)]
        public string? Password { get; set; }

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

        public override Task PollAsync(CancellationToken cancellationToken)
        {
            if (ServerUrl == null) return Task.CompletedTask;

            var serverUri = new Uri(ServerUrl);

            if (_rawFramesSource == null ||
                IsConnected == false)
            {
                try
                {
                    var connectionParameters =
                        !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) ?
                        new ConnectionParameters(serverUri, new NetworkCredential(UserName, Password)) :
                        new ConnectionParameters(serverUri);

                    connectionParameters.RtpTransport = RtpTransportProtocol.TCP;

                    _rawFramesSource?.Stop();

                    int intervalMs = 1000;
                    int lastTimeSnapshotSaved = Environment.TickCount - intervalMs;

                    _rawFramesSource = new RawFramesSource(connectionParameters);
                    _rawFramesSource.ConnectionStatusChanged += (sender, status) => Console.WriteLine(status);

                    var decodedFrameSource = new DecodedFrameSource();
                    decodedFrameSource.FrameReceived += (sender, frame) =>
                    {
                        int ticksNow = Environment.TickCount;

                        if (Math.Abs(ticksNow - lastTimeSnapshotSaved) < intervalMs)
                        {
                            return;
                        }

                        lastTimeSnapshotSaved = ticksNow;

                        var bitmap = frame.GetBitmap();

                        var stream = new MemoryStream();
#pragma warning disable CA1416 // Validate platform compatibility
                        bitmap.Save(stream, ImageFormat.Jpeg);
#pragma warning restore CA1416 // Validate platform compatibility

                        Image = stream.ToArray();
                        ImageType = "image/jpeg";
                        IsConnected = true;
                        LastUpdated = DateTimeOffset.Now;

                        ThingManager.DetectChanges(this);
                    };

                    decodedFrameSource.SetRawFramesSource(_rawFramesSource);
                    _rawFramesSource.Start();
                }
                catch
                {
                    IsConnected = false;
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _rawFramesSource?.Stop();
            base.Dispose();
        }
    }
}

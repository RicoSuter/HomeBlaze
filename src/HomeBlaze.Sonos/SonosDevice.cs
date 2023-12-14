using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Media;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using Rssdp;
using Sonos.Base.Metadata;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using static Sonos.Base.Services.AVTransportService;
using static Sonos.Base.Services.RenderingControlService;

namespace HomeBlaze.Sonos
{
    public class SonosDevice :
        IThing,
        IIconProvider,
        INetworkAdapter,
        IAsyncDisposable,
        IAudioPlayer
    {
        private readonly SonosSystem _parent;

        private SsdpRootDevice _rootDevice;
        private global::Sonos.Base.SonosDevice _sonosDevice;
        private GetPositionInfoResponse? _positionInfo;

        public string Id => $"{_parent.Id}/devices/{Uuid}";

        public string? Title => $"{ModelName} ({RoomName})";

        public string IconName => IsAudioPlayer ? "fa-solid fa-play" : "fa-solid fa-volume-low";

        [State]
        public string Uuid => _rootDevice.Uuid;

        [State]
        public string? ModelName => _rootDevice.ModelName;

        [State]
        public string? RoomName => _rootDevice.CustomProperties.FirstOrDefault(p => p.Name == "roomName")?.Value;

        [State]
        public string? Host => _rootDevice.UrlBase.Host;

        [State]
        public string? IpAddress => IpHelper.TryGetIpAddress(Host);

        [State]
        public bool IsConnected => true;

        [State]
        public bool IsAudioPlayer { get; private set; }

        [State]
        public bool? IsAudioPlaying { get; internal set; }

        [State]
        public int AudioVolume { get; private set; }

        [State]
        public bool IsAudioMuted { get; private set; }

        [State]
        public string? CurrentAudioTrackUri { get; private set; }

        [State]
        public string? CurrentAudioTrackTitle { get; private set; }

        [State]
        public string? CurrentAudioTrackCreator { get; private set; }

        [State]
        public string? CurrentAudioTrackAlbum { get; private set; }

        [State]
        public string? CurrentAudioTrackImageUri { get; private set; }

        public SonosDevice(SonosSystem parent, SsdpRootDevice rootDevice, global::Sonos.Base.SonosDevice sonosDevice)
        {
            _parent = parent;
            _rootDevice = rootDevice;
            _sonosDevice = sonosDevice;
        }

        internal async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _sonosDevice.AVTransportService.OnEvent += OnAvTransportEvent;
                _sonosDevice.RenderingControlService.OnEvent += OnRenderingControlEvent;

                await _sonosDevice.AVTransportService.SubscribeForEventsAsync(CancellationToken.None);
                await _sonosDevice.RenderingControlService.SubscribeForEventsAsync(CancellationToken.None);
            }
            catch
            {
            }
        }

        internal async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            IsAudioPlayer = _rootDevice.Devices.Any(d => d.Services.Any(s => s.ServiceId == "urn:upnp-org:serviceId:AVTransport"));

            if (IsAudioPlayer)
            {
                var response = await _sonosDevice.AVTransportService.GetTransportInfoAsync(cancellationToken);
                IsAudioPlaying = response.CurrentTransportState == "PLAYING";

                try
                {
                    await _sonosDevice.AVTransportService.RenewEventSubscriptionAsync(cancellationToken);
                    await _sonosDevice.RenderingControlService.RenewEventSubscriptionAsync(cancellationToken);
                }
                catch
                {
                }

                if (IsAudioPlaying == true)
                {
                    _positionInfo = await _sonosDevice.AVTransportService.GetPositionInfoAsync(cancellationToken);

                    CurrentAudioTrackUri = _positionInfo?.TrackURI;
                    CurrentAudioTrackTitle = GetTrackTitle(_positionInfo?.TrackMetaData, _positionInfo?.TrackMetaDataObject);
                    CurrentAudioTrackCreator = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.Creator;
                    CurrentAudioTrackAlbum = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.Album;
                    CurrentAudioTrackImageUri = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.AlbumArtUri;
                }
                else
                {
                    _positionInfo = null;
                    CurrentAudioTrackTitle = null;
                }

                AudioVolume = await _sonosDevice.RenderingControlService.GetVolumeAsync("Master", cancellationToken);
                var mute = await _sonosDevice.RenderingControlService.GetMuteAsync(new GetMuteRequest
                {
                    InstanceID = 0,
                    Channel = "Master"
                }, cancellationToken);
                IsAudioMuted = mute.CurrentMute;
            }

            _parent.ThingManager.DetectChanges(_parent);
        }

        private static string? GetTrackTitle(string? trackMetaData, Didl? trackMetaDataObject)
        {
            string? trackTitle = null;

            if (!string.IsNullOrEmpty(trackMetaData) && trackMetaData != "NOT_IMPLEMENTED")
            {
                try
                {
                    var xml = XDocument.Parse(trackMetaData);
                    trackTitle = xml?.Root?
                        .Elements()
                        .FirstOrDefault()?
                        .Element(XName.Get("streamContent", "urn:schemas-rinconnetworks-com:metadata-1-0/"))?
                        .Value;
                }
                catch
                {
                    trackTitle = null;
                }
            }

            if (trackTitle == null)
            {
                trackTitle = trackMetaDataObject?.Items.FirstOrDefault()?.Title;
            }

            return trackTitle;
        }

        private void OnRenderingControlEvent(object? sender, IRenderingControlEvent e)
        {
            if (e.Volume?.TryGetValue("Master", out var volume) == true)
            {
                AudioVolume = volume;
                _parent.ThingManager.DetectChanges(this);
            }

            if (e.Mute?.TryGetValue("Master", out var mute) == true)
            {
                IsAudioMuted = mute;
                _parent.ThingManager.DetectChanges(this);
            }
        }

        private void OnAvTransportEvent(object? sender, IAVTransportEvent e)
        {
            CurrentAudioTrackUri = e.CurrentTrackURI;
            CurrentAudioTrackTitle = GetTrackTitle(e.CurrentTrackMetaData, e.CurrentTrackMetaDataObject);
            CurrentAudioTrackCreator = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.Creator;
            CurrentAudioTrackAlbum = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.Album;
            CurrentAudioTrackImageUri = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.AlbumArtUri;

            _parent.ThingManager.DetectChanges(this);
        }

        [Operation]
        public async Task SetVolumeAsync(int volume, CancellationToken cancellationToken = default)
        {
            await _sonosDevice.RenderingControlService.SetVolumeAsync(new SetVolumeRequest
            {
                InstanceID = 0,
                Channel = "Master",
                DesiredVolume = volume
            }, cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PlayAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.PlayAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PauseAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.PauseAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.StopAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task NextAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.NextAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PreviousAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.PreviousAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task MuteAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.RenderingControlService.SetMuteAsync(new SetMuteRequest
            {
                InstanceID = 0,
                Channel = "Master",
                DesiredMute = true
            }, cancellationToken);

            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task UnmuteAsync(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.RenderingControlService.SetMuteAsync(new SetMuteRequest
            {
                InstanceID = 0,
                Channel = "Master",
                DesiredMute = false
            }, cancellationToken);

            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task SwitchToTv(CancellationToken cancellationToken = default)
        {
            await _sonosDevice.AVTransportService.SetAVTransportURIAsync(new SetAVTransportURIRequest
            {
                InstanceID = 0,
                CurrentURI = $"x-sonos-htastream:{_rootDevice.Uuid}:spdif",
                CurrentURIMetaData = string.Empty
            }, cancellationToken);

            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PlayTrackAsync(string trackUri, string? title, CancellationToken cancellationToken = default)
        {
            //var trackUri = "aac://https://chmedia.streamabc.net/79-fm1-aacplus-64-6623023?sABC=6571p103%230%235o62sspn3q052pp705s20n0ss6qn6p2r%23gharva\u0026aw_0_1st.playerid=tunein\u0026amsparams=playerid:tunein;skey:1701953795";

            await PlayUriAsync(trackUri, string.Empty, title ?? string.Empty, false, cancellationToken);

            await PlayAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PlayRadioAsync(string radioUri, string? title, CancellationToken cancellationToken = default)
        {
            //var trackUri = "aac://https://chmedia.streamabc.net/79-fm1-aacplus-64-6623023?sABC=6571p103%230%235o62sspn3q052pp705s20n0ss6qn6p2r%23gharva\u0026aw_0_1st.playerid=tunein\u0026amsparams=playerid:tunein;skey:1701953795";

            await PlayUriAsync(radioUri, string.Empty, title ?? string.Empty, true, cancellationToken);

            await PlayAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task PlaySpotifyTrackAsync(string spotifyId, CancellationToken cancellationToken = default)
        {
            //var rand = new Random();
            //var randNumber = rand.Next(10000000, 99999999);

            //var uri = $"x-sonos-spotify:spotify:track:5F0oGBiHPVWR9ZPbwKQkih?sid=9\u0026flags=0\u0026sn=2"
            var uri = $"x-sonos-spotify:spotify:track:{spotifyId}?sid=9\u0026flags=0\u0026sn=2";

            //var metadata = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
            //                $"<item id=\"{randNumber}spotify%3atrack%3a{spotifyId}\" restricted=\"true\">" +
            //                   "<dc:title></dc:title>" +
            //                   $"<upnp:class>{ItemClass.MusicTrack}</upnp:class>" +
            //                   "<desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">SA_RINCON2311_X_#Svc2311-0-Token</desc>" +
            //                "</item>" +
            //               "</DIDL-Lite>";

            //metadata = HttpUtility.HtmlEncode(metadata);

            await PlayUriAsync(uri, string.Empty, string.Empty, false, cancellationToken);

            await PlayAsync(cancellationToken);
            await RefreshAsync(cancellationToken);
        }

        public async Task PlayUriAsync(string uri = "", string meta = "", string title = "", bool forceRadio = false, CancellationToken cancellationToken = default)
        {
            // If no metadata is provided but a title is, generate minimal metadata
            if (string.IsNullOrEmpty(meta) && !string.IsNullOrEmpty(title))
            {
                string metaTemplate = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"R:0/0/0\" parentID=\"R:0/0\" restricted=\"true\"><dc:title>{0}</dc:title><upnp:class>object.item.audioItem.audioBroadcast</upnp:class><desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">{1}</desc></item></DIDL-Lite>";
                string tuneinService = "SA_RINCON65031_";
                meta = string.Format(metaTemplate, HttpUtility.HtmlEncode(title), tuneinService);
            }

            // Change URI prefix to force radio style display and commands
            if (forceRadio)
            {
                int colon = uri.IndexOf(":");
                if (colon > 0)
                {
                    uri = "x-rincon-mp3radio" + uri.Substring(colon);
                }
            }

            await _sonosDevice.AVTransportService.SetAVTransportURIAsync(new SetAVTransportURIRequest
            {
                InstanceID = 0,
                CurrentURI = uri,
                CurrentURIMetaData = meta
            }, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            _sonosDevice.AVTransportService.OnEvent -= OnAvTransportEvent;
            _sonosDevice.RenderingControlService.OnEvent -= OnRenderingControlEvent;

            await _sonosDevice.AVTransportService.CancelEventSubscriptionAsync(CancellationToken.None);
            await _sonosDevice.RenderingControlService.CancelEventSubscriptionAsync(CancellationToken.None);
        }
    }
}

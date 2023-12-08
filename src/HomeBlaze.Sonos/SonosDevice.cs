using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using Rssdp;
using Sonos.Base.Metadata;
using Sonos.Base.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using static Sonos.Base.Services.AVTransportService;

namespace HomeBlaze.Sonos
{
    public class SonosDevice : IThing, IIconProvider, IAsyncDisposable
    {
        private readonly SonosSystem _parent;

        private SsdpRootDevice _rootDevice;
        private global::Sonos.Base.SonosDevice _sonosDevice;
        private GetPositionInfoResponse? _positionInfo;

        public string Id => $"sonos/{Uuid}";

        public string? Title => $"{ModelName} ({RoomName})";

        public string IconName => IsPlayer ? "fa-solid fa-play" : "fa-solid fa-volume-low";

        [State]
        public string Uuid => _rootDevice.Uuid;

        [State]
        public string? ModelName => _rootDevice.ModelName;

        [State]
        public string? RoomName => _rootDevice.CustomProperties.FirstOrDefault(p => p.Name == "roomName")?.Value;

        [State]
        public string? Host => _rootDevice.UrlBase.Host;

        [State]
        public bool IsPlayer { get; private set; }

        [State]
        public bool? IsPlaying { get; internal set; }

        [State]
        public int Volume { get; private set; }

        [State]
        public string? TrackUri { get; private set; }

        [State]
        public string? TrackTitle { get; private set; }

        [State]
        public string? TrackCreator { get; private set; }

        [State]
        public string? TrackAlbum { get; private set; }

        [State]
        public string? TrackImageUri { get; private set; }

        public SonosDevice(SonosSystem parent, SsdpRootDevice rootDevice, global::Sonos.Base.SonosDevice sonosDevice)
        {
            _parent = parent;
            _rootDevice = rootDevice;
            _sonosDevice = sonosDevice;
        }

        internal async Task InitializeAsync()
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

        internal async Task RefreshAsync()
        {
            IsPlayer = _rootDevice.Devices.Any(d => d.Services.Any(s => s.ServiceId == "urn:upnp-org:serviceId:AVTransport"));

            if (IsPlayer)
            {
                var response = await _sonosDevice.AVTransportService.GetTransportInfoAsync();
                IsPlaying = response.CurrentTransportState == "PLAYING";

                try
                {
                    await _sonosDevice.AVTransportService.RenewEventSubscriptionAsync(CancellationToken.None);
                    await _sonosDevice.RenderingControlService.RenewEventSubscriptionAsync(CancellationToken.None);
                }
                catch
                {
                }

                if (IsPlaying == true)
                {
                    _positionInfo = await _sonosDevice.AVTransportService.GetPositionInfoAsync();

                    TrackUri = _positionInfo?.TrackURI;
                    TrackTitle = GetTrackTitle(_positionInfo?.TrackMetaData, _positionInfo?.TrackMetaDataObject);
                    TrackCreator = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.Creator;
                    TrackAlbum = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.Album;
                    TrackImageUri = _positionInfo?.TrackMetaDataObject?.Items.FirstOrDefault()?.AlbumArtUri;
                }
                else
                {
                    _positionInfo = null;
                    TrackTitle = null;
                }

                Volume = await _sonosDevice.RenderingControlService.GetVolumeAsync();
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

        private void OnRenderingControlEvent(object? sender, RenderingControlService.IRenderingControlEvent e)
        {
            if (e.Volume?.TryGetValue("Master", out var volume) == true)
            {
                Volume = volume;

                _parent.ThingManager.DetectChanges(this);
            }
        }

        private void OnAvTransportEvent(object? sender, IAVTransportEvent e)
        {
            TrackUri = e.CurrentTrackURI;
            TrackTitle = GetTrackTitle(e.CurrentTrackMetaData, e.CurrentTrackMetaDataObject);
            TrackCreator = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.Creator;
            TrackAlbum = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.Album;
            TrackImageUri = e.CurrentTrackMetaDataObject?.Items.FirstOrDefault()?.AlbumArtUri;

            _parent.ThingManager.DetectChanges(this);
        }

        [Operation]
        public async Task SetVolumeAsync(int volume)
        {
            await _sonosDevice.RenderingControlService.SetVolumeAsync(new global::Sonos.Base.Services.RenderingControlService.SetVolumeRequest
            {
                InstanceID = 0,
                Channel = "Master",
                DesiredVolume = volume
            });
            await RefreshAsync();
        }

        [Operation]
        public async Task PlayAsync()
        {
            await _sonosDevice.PlayAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task PauseAsync()
        {
            await _sonosDevice.PauseAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task StopAsync()
        {
            await _sonosDevice.StopAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task NextAsync()
        {
            await _sonosDevice.NextAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task PreviousAsync()
        {
            await _sonosDevice.PreviousAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task SwitchToTv()
        {
            await _sonosDevice.AVTransportService.SetAVTransportURIAsync(new SetAVTransportURIRequest
            {
                InstanceID = 0,
                CurrentURI = $"x-sonos-htastream:{_rootDevice.Uuid}:spdif",
                CurrentURIMetaData = string.Empty
            });

            await RefreshAsync();
        }

        [Operation]
        public async Task PlayTrackAsync(string trackUri, string? title)
        {
            //var trackUri = "aac://https://chmedia.streamabc.net/79-fm1-aacplus-64-6623023?sABC=6571p103%230%235o62sspn3q052pp705s20n0ss6qn6p2r%23gharva\u0026aw_0_1st.playerid=tunein\u0026amsparams=playerid:tunein;skey:1701953795";

            await PlayUriAsync(trackUri, string.Empty, title ?? string.Empty, false);

            await PlayAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task PlayRadioAsync(string radioUri, string? title)
        {
            //var trackUri = "aac://https://chmedia.streamabc.net/79-fm1-aacplus-64-6623023?sABC=6571p103%230%235o62sspn3q052pp705s20n0ss6qn6p2r%23gharva\u0026aw_0_1st.playerid=tunein\u0026amsparams=playerid:tunein;skey:1701953795";

            await PlayUriAsync(radioUri, string.Empty, title ?? string.Empty, true);

            await PlayAsync();
            await RefreshAsync();
        }

        [Operation]
        public async Task PlaySpotifyTrackAsync(string spotifyId)
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

            await PlayUriAsync(uri, string.Empty, string.Empty, false);

            await PlayAsync();
            await RefreshAsync();
        }

        public async Task PlayUriAsync(string uri = "", string meta = "", string title = "", bool forceRadio = false)
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
            });
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

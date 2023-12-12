using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using MudBlazor;
using PixelByProxy.Asus.Router.Models;

namespace HomeBlaze.AsusRouter
{
    public class AsusRouterClient :
        IThing,
        IIconProvider,
        IConnectedThing,
        INetworkAdapter
    {
        internal Client Client { get; private set; }

        [ParentThing]
        public IThing? ParentThing { get; private set; }

        public string Id => $"{ParentThing?.Id}/clients/{Client.Mac}";

        public string? Title => Client.DisplayName +
            (!string.IsNullOrEmpty(IpAddress) ? " (" + IpAddress + ")" : "");

        public string IconName => "fas fa-tablet-alt";

        public Color IconColor =>
            IsConnected == true ? Color.Success :
            IsConnected == false ? Color.Error :
            Color.Default;

        [State]
        public string? Name => Client.Name;

        [State]
        public string? NickName => Client.NickName;

        [State]
        public string? IpAddress => Client.Ip;

        [State]
        public string? MacAddress => Client.Mac;

        public bool IsConnected => Client.IsOnline;

        public AsusRouterClient(Client client)
        {
            Client = client;
        }

        public void Update(Client client)
        {
            Client = client;
        }
    }
}
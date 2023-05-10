using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using MudBlazor;
using PixelByProxy.Asus.Router.Models;
using System;

namespace HomeBlaze.AsusRouter
{
    public class AsusRouterClient : IThing,
        IIconProvider, IConnectedThing
    {
        internal Client Client { get; private set; }

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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
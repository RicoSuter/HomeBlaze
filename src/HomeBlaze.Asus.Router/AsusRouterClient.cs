using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;
using MudBlazor;
using PixelByProxy.Asus.Router.Models;

namespace HomeBlaze.AsusRouter
{
    public class AsusRouterClient : IThing, IIconProvider,
        IConnectedDevice, 
        IUpdateThing<Client>
    {
        internal Client Client { get; private set; }

        public string? Id => "asus.router.client." + Client.Mac;

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
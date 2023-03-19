// -----------------------------------------------------------------------
// <copyright file="DiscoveryService.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	///     Discovers Harmony Hub clients on the local network
	/// </summary>
	public class DiscoveryService {
		/// <summary>
		///     The port to broadcast the probe on
		/// </summary>
		private const int BroadcastPort = 5224;

		/// <summary>
		///     A list of the ids of Hubs that have already been found
		/// </summary>
		private readonly List<string> FoundHubIDs = new List<string>();

		/// <summary>
		///     The port to listen for connections on
		/// </summary>
		private readonly int ListenPort;

		/// <summary>
		///     The source of the token used to cancel broadcast packets
		/// </summary>
		private CancellationTokenSource BroadcastCancellationTokenSource;

		/// <summary>
		///     The listener to wait for clients on
		/// </summary>
		private TcpListener Listener;

		/// <summary>
		///     The thread to wait for clients on
		/// </summary>
		private Thread ListenThread;

		/// <summary>
		///     Initializes a new instance of the <see cref="DiscoveryService" /> class.
		/// </summary>
		/// <param name="listenPort">The port to listen for connections on, defaults to 5446</param>
		public DiscoveryService(int listenPort = 5446) => this.ListenPort = listenPort;

		/// <summary>
		///     Event called when a hub was found
		/// </summary>
		public event EventHandler<HubFoundEventArgs> HubFound;

		/// <summary>
		///     Gets or sets the delay between discovery probes broadcasted over the network
		/// </summary>
		public int RebroadcastDelay { get; set; } = 5000;

		/// <summary>
		///     Broadcasts availability and waits for clients to identify
		/// </summary>
		public void StartDiscovery()
		{
			this.BroadcastCancellationTokenSource = new CancellationTokenSource();
			this.BroadcastContinually(this.BroadcastCancellationTokenSource.Token);
			this.StartListening();
		}

		/// <summary>
		///     Stops listening for clients
		/// </summary>
		public void StopDiscovery()
		{
			this.BroadcastCancellationTokenSource.Cancel();

			// should stop the thread as well
			this.Listener.Stop();
		}

		/// <summary>
		///     Gets the device's local IP address
		/// </summary>
		/// <returns>A IPv4 IP address string</returns>
		private static string GetLocalIPAddressString()
		{
			IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress IP in Host.AddressList)
				if (IP.AddressFamily == AddressFamily.InterNetwork)
					return IP.ToString();

			return Host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString()
				   ?? throw new WebException("No network adapters with an IPv4 address found");
		}

		/// <summary>
		///     Broadcasts the probe to 255.255.255.255, falling back to 224.0.0.1
		/// </summary>
		private void Broadcast()
		{
			try
			{
				this.BroadcastTo("255.255.255.255");
			}
			catch (SocketException)
			{
				this.BroadcastTo("224.0.0.1");
			}
		}

		/// <summary>
		///     Continually broadcasts the probe every 3 seconds
		/// </summary>
		/// <param name="token">A token used to cancel the broadcasts</param>
		private async void BroadcastContinually(CancellationToken token)
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					this.Broadcast();
					await Task.Delay(this.RebroadcastDelay, token);
				}
			}
			catch (OperationCanceledException)
			{
				// token has been canceled. exit
			}
		}

		/// <summary>
		///     Broadcasts the probe to a specific IP address
		/// </summary>
		/// <param name="ipAddress">The IPv4 address to broadcast to</param>
		private void BroadcastTo(string ipAddress)
		{
			Socket UDPSocket =
				new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
				{
					EnableBroadcast = true,
					MulticastLoopback = false
				};

			IPEndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), DiscoveryService.BroadcastPort);
			UDPSocket.Connect(RemoteEndPoint);

			byte[] ProbeBytes = Encoding.UTF8.GetBytes(this.GetProbe());
			UDPSocket.Send(ProbeBytes);
			UDPSocket.Close();
		}

		/// <summary>
		///     Gets the probe that should be broadcast
		/// </summary>
		/// <returns>The newline separated probe</returns>
		private string GetProbe() =>
			$"_logitech-reverse-bonjour._tcp.local.\n{this.ListenPort}\n{DiscoveryService.GetLocalIPAddressString()}\nstring";

		/// <summary>
		///     Starts listening for tcp clients synchronously
		/// </summary>
		private void Listen()
		{
			this.Listener.Start();
			while (true) // will wait until a client connects
				try
				{
					TcpClient Client = this.Listener.AcceptTcpClient();
					this.OnClientAccepted(Client);
				}
				catch (Exception e) when (e is InvalidOperationException || e is SocketException)
				{
					// we've been canceled, just break
					break;
				}
		}

		/// <summary>
		///     Called when a client has been accepted on the listener
		/// </summary>
		/// <param name="client">The client that has been accepted</param>
		private void OnClientAccepted(TcpClient client)
		{
			// Harmony seems to think that the data can never exceed 1024 bytes. Okay I guess
			byte[] Buffer = new byte[1024];
			int BytesRead = client.GetStream().Read(Buffer, 0, Buffer.Length);
			string ClientData = Encoding.UTF8.GetString(Buffer, 0, BytesRead);

			// probably not hub info
			if (!ClientData.Contains(";")) return;

			HubInfo HubInfo = HubInfo.Parse(ClientData);
			if (this.FoundHubIDs.Contains(HubInfo.UUID)) return;

			this.FoundHubIDs.Add(HubInfo.UUID);
			this.HubFound?.Invoke(this, new HubFoundEventArgs(HubInfo));
		}

		/// <summary>
		///     Starts listening for clients to connect after a discovery probe is sent
		/// </summary>
		private void StartListening()
		{
			this.Listener = new TcpListener(IPAddress.Any, this.ListenPort);
			this.Listener.Start();

			this.ListenThread = new Thread(this.Listen);
			this.ListenThread.Start();
		}
	}

	/// <inheritdoc />
	/// <summary>
	///     Event arguments for when a Hub was found
	/// </summary>
	public class HubFoundEventArgs : EventArgs {
		/// <inheritdoc />
		/// <summary>
		///     Initializes a new instance of the <see cref="T:Harmony.HubFoundEventArgs" /> class.
		/// </summary>
		/// <param name="hubInfo">The Hub that was found</param>
		public HubFoundEventArgs(HubInfo hubInfo) => this.HubInfo = hubInfo;

		/// <summary>
		///     Gets the Hub that was found
		/// </summary>
		public HubInfo HubInfo { get; }
	}
}
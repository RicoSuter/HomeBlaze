// -----------------------------------------------------------------------
// <copyright file="HubConnection.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.WebSockets {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Net.WebSockets;
	using System.Threading.Tasks;

	using Harmony.CommandParameters;
	using Harmony.Events;
	using Harmony.Responses;

	using Newtonsoft.Json;

	/// <summary>
	///     A WebSocket connection to a Hub and low-level methods to interface with it
	/// </summary>
	internal class HubConnection : WebSocketConnection {
		/// <summary>
		///     The ID to use when sending messages over the WebSocket
		/// </summary>
		private readonly DeviceID MessageID;

        private readonly string hostname;
        private readonly string hubRemoteId;

        /// <summary>
        ///		A value indicating whether this <see cref="HubConnection"/> has been disposed or not
        /// </summary>
        private bool IsDisposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="HubConnection" /> class.
		/// </summary>
		/// <param name="messageID">The ID to use when sending messages over the WebSocket</param>
		public HubConnection(DeviceID messageID, string hostname, string hubRemoteId)
        {
            MessageID = messageID;
            this.hostname = hostname;
            this.hubRemoteId = hubRemoteId;
        }

        /// <summary>
        ///     Event raised when a message has been received on the WebSocket
        /// </summary>
        public event EventHandler<StringResponseEventArgs> OnMessageReceived;

		/// <summary>
		///     Connects to the WebSocket
		/// </summary>
		/// <returns>When the WebSocket has connected</returns>
		public Task Connect() {
			string FullUrl = $"ws://{hostname}:8088/?domain=svcs.myharmony.com&hubId={hubRemoteId}";
			return base.Connect(FullUrl);
		}

		/// <summary>
		///		Disposes resources used by this <see cref="HubConnection"/>
		/// </summary>
		public override void Dispose() {
			this.IsDisposed = true;
			base.Dispose();
		}

		/// <summary>
		///     Sends a command with no parameters
		/// </summary>
		/// <param name="commandName">The name of the command to execute</param>
		/// <returns>When the command has been sent</returns>
		public Task<string> SendCommand(string commandName) => this.SendCommand(commandName, null);

		/// <summary>
		///     Sends a command with the specified parameters
		/// </summary>
		/// <param name="commandName">The name of the command to execute</param>
		/// <param name="parameters">The parameters to execute the command with</param>
		/// <returns>The ID of the command when the it has been sent</returns>
		public Task<string> SendCommand(string commandName, CommandParams parameters) {
			Command Command = new Command(commandName, parameters, this.MessageID.Next());
			return this.SendCommand(Command);
		}

		/// <summary>
		///     Handles listening on the websocket connection
		/// </summary>
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "when is just completely mishandled by StyleCop")]
		public override async void StartListening() {
			try {
				// main listening loop
				while (!this.IsDisposed && !this.WebSocket.CloseStatus.HasValue) {
					StringResponse Message = await this.ReceiveHarmonyMessage();

					// pass message on
					this.OnMessageReceived?.Invoke(this, new StringResponseEventArgs(Message));
				}
			} catch (Exception e) 
				when (e is InvalidOperationException || e is WebSocketException || e is HarmonyException) {
				// often other parties fail to close websockets correctly,
				// but avoid throwing an exception if that happens
				// instead, try to reconnect
				if (this.IsDisposed) return;

				// reconnect after 5s
                await Task.Delay(5000);
				await Connect();
			}	
		}

		/// <summary>
		///     Sends a command over the WebSocket
		/// </summary>
		/// <param name="command">The command to send</param>
		/// <returns>The ID of the command when the it has been sent</returns>
		private async Task<string> SendCommand(Command command) {
			string CommandJson = JsonConvert.SerializeObject(command);

			// need to surround in another object, using a string is much easier than using JSON.NET
			// nevertheless, TODO make this less weird
			try {
				await this.SendMessage("{\"hbus\": " + CommandJson + "}");
			} catch (WebSocketException) {
				throw new HarmonyException("Hub closed the connection unexpectedly");
			}

			return command.ID;
		}
	}
}
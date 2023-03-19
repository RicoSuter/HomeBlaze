// -----------------------------------------------------------------------
// <copyright file="WebSocketConnection.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.WebSockets {
	using System;
	using System.Net.WebSockets;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using Harmony.Responses;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	
	using ClientWebSocket = System.Net.WebSockets.Managed.ClientWebSocket;

	/// <summary>
	///     A handler for websocket connections
	/// </summary>
	internal abstract class WebSocketConnection : IDisposable {
		/// <summary>
		///     A semaphore to ensure multiple messages aren't sent over the websocket at once
		/// </summary>
		private readonly SemaphoreSlim SendSemaphore = new SemaphoreSlim(1);

		/// <summary>
		///     Initializes a new instance of the <see cref="WebSocketConnection" /> class.
		/// </summary>
		protected WebSocketConnection() => this.WebSocket = new ClientWebSocket();////new ClientWebSocket();

		/// <summary>
		///     Gets the websocket to communicate over
		/// </summary>
		protected ClientWebSocket WebSocket { get; }

		/// <summary>
		///     Connects to the WebSocket
		/// </summary>
		/// <param name="url">The url to connect to</param>
		/// <returns>When the WebSocket has connected</returns>
		public async Task Connect(string url)
		{
			// TODO: Improve this
			if (WebSocket.State == WebSocketState.None ||
                WebSocket.State == WebSocketState.CloseSent ||
                WebSocket.State == WebSocketState.CloseReceived ||
                WebSocket.State == WebSocketState.Aborted)
			{
                await this.WebSocket.ConnectAsync(new Uri(url), CancellationToken.None);
            }
        }

        /// <summary>
        ///     Disconnects from the WebSocket
        /// </summary>
        /// <returns>When the WebSocket has disconnected</returns>
        public async Task Disconnect() =>
			await this.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);

		/// <inheritdoc />
		/// <summary>
		///     Disposes resources used by this <see cref="T:Harmony.HubConnection" />
		/// </summary>
		public virtual async void Dispose() {
			// close then dispose the websocket
			try {
				await this.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
			}
			catch (OperationCanceledException) {
				// ignored
			}
			catch (WebSocketException) {
				// ignored
			}

			this.WebSocket.Dispose();
		}

		/// <summary>
		///     Waits for a message with serialized response data from the WebSocket
		/// </summary>
		/// <returns>The <see cref="StringResponse" /></returns>
		public async Task<StringResponse> ReceiveHarmonyMessage() {
			// harmony sends two kinds of messages: command responses and incoming messages
			// we want to generalize them into one Response<T>
			// incoming messages have the `type` property, and command the `cmd` one.
			string Message = await this.ReceiveMessage();

			try {
				JObject MessageObject = JObject.Parse(Message);
				return MessageObject.ContainsKey("cmd")
					       ? MessageObject.ToObject<StringResponse>()
					       : MessageObject.ToObject<IncomingMessage>().ToStringResponse();
			}
			catch (JsonException) {
				// failed to deserialize properly, connection likely closed
				throw new HarmonyException("Failed to receive message from Hub");
			}
		}

		/// <summary>
		///     Handles listening on a websocket connection
		/// </summary>
		public abstract void StartListening();

		/// <summary>
		///     Receives a message from the websocket connection as a UTF8 string
		/// </summary>
		/// <returns>The received message</returns>
		protected async Task<string> ReceiveMessage() {
			string ReceivedMessage = string.Empty;
			byte[] Buffer = new byte[1024 * 4];

			// receive message into a buffer until the "message finished" flag is set
			WebSocketReceiveResult Result = null;
			while (Result == null || !Result.EndOfMessage) {
				Result = await this.WebSocket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);
				ReceivedMessage += Encoding.UTF8.GetString(Buffer, 0, Result.Count);
			}

			return ReceivedMessage;
		}

		/// <summary>
		///     Sends a JSON message over the websocket
		/// </summary>
		/// <typeparam name="T">The type of the payload data being sent over the websocket</typeparam>
		/// <param name="data">The object to serialize and send over the websocket</param>
		/// <returns>When the message has been sent</returns>
		protected Task SendJsonMessage<T>(T data) => this.SendMessage(JsonConvert.SerializeObject(data));

		/// <summary>
		///     Sends a string message over the websocket using UTF-8 encoding
		/// </summary>
		/// <param name="message">The message to send over the websocket</param>
		/// <returns>When the message has been sent</returns>
		protected async Task SendMessage(string message) {
			byte[] MessageBytes = Encoding.UTF8.GetBytes(message);

			// avoid sending multiple messages at once
			await this.SendSemaphore.WaitAsync();
			try {
				// TODO: Buffer this if it's too long
				await this.WebSocket.SendAsync(
					new ArraySegment<byte>(MessageBytes),
					WebSocketMessageType.Text,
					true,
					CancellationToken.None);
			}
			finally {
				this.SendSemaphore.Release();
			}
		}
	}
}
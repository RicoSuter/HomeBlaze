// -----------------------------------------------------------------------
// <copyright file="Hub.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net;
	using System.Threading;
	using System.Threading.Tasks;

	using Harmony.CommandParameters;
	using Harmony.Events;
	using Harmony.Responses;
	using Harmony.WebSockets;

	using Newtonsoft.Json;

	/// <summary>
	///     A Harmony Hub
	/// </summary>
	[SuppressMessage(
		"ReSharper",
		"CompareOfFloatsByEqualityOperator",
		Justification = "Status codes are statically defined, never determined via math.")]
	public class Hub : IDisposable {
		/// <summary>
		///     The time that the WebSocket connection was initiated
		/// </summary>
		private DateTime ConnectedAt;

		/// <summary>
		///     The function that is currently being "held", or repeatedly pressed, if any
		/// </summary>
		private Function HeldDownFunction;

		/// <summary>
		///     Initializes a new instance of the <see cref="Hub" /> class.
		/// </summary>
		/// <param name="hubInfo">The hub's IP address and name info, usually obtained via discovery</param>
		public Hub(HubInfo hubInfo) => this.Info = hubInfo;

		/// <summary>
		///     Initializes a new instance of the <see cref="Hub" /> class.
		/// </summary>
		/// <param name="ip">The IP address of the hub</param>
		/// <param name="remoteId">The remote id of the hub</param>
		public Hub(string ip, string remoteId) => this.Info = new HubInfo { IP = ip, RemoteId = remoteId };

		/// <summary>
		///     Prevents a default instance of the <see cref="Hub" /> class from being created.
		///     Used for initializing JSON deserialization
		/// </summary>
		[JsonConstructor]
		private Hub() { }

		/// <summary>
		///     Event raised when an activity has progressed in starting or stopping
		/// </summary>
		public event EventHandler<ActivityProgressEventArgs> OnActivityProgress;

		/// <summary>
		///     Event raised when an activity has finished starting or stopping
		/// </summary>
		public event EventHandler<HarmonyEventArgs<Activity>> OnActivityRan;

		/// <summary>
		///     Event raised when the Hub has finished changing channels (only if the request was also sent by this <see cref="Hub"/> object)
		/// </summary>
		public event EventHandler<SuccessEventArgs> OnChannelChanged;

		/// <summary>
		///     Event raised when a Hub has been synchronized
		/// </summary>
		public event EventHandler<HarmonyEventArgs<HubSync>> OnHubSynchronized;

		/// <summary>
		///     Event raised when a state digest is received
		/// </summary>
		public event EventHandler<HarmonyEventArgs<StateDigest>> OnStateDigestReceived;

		/// <summary>
		///     Gets the list of synced activities on this hub
		/// </summary>
		public Activity[] Activities => this.Sync?.Activities;

		/// <summary>
		///     Gets the list of synced devices on this hub
		/// </summary>
		public Device[] Devices => this.Sync?.Devices;

		/// <summary>
		///     Gets info about the Hub's IP address, name, and more
		/// </summary>
		[JsonProperty]
		public HubInfo Info { get; private set; }

		/// <summary>
		///     Gets the last known state of the Hub
		/// </summary>
		public StateDigest State { get; private set; }

		// We can use a private setter for these because the JsonProperty attribute will make it work anyway

		/// <summary>
		///     Gets info about the Hub's devices, activities, and more
		/// </summary>
		[JsonProperty]
		public HubSync Sync { get; private set; }

		/// <summary>
		///     Gets or sets the Hub's WebSocket connection
		/// </summary>
		private HubConnection Connection { get; set; }

		/// <summary>
		///     Deserializes a JSON string into a <see cref="Hub" />
		/// </summary>
		/// <param name="json">The JSON to deserialize</param>
		/// <returns>The deserialized <see cref="Hub" /></returns>
		public static Hub FromJson(string json) => JsonConvert.DeserializeObject<Hub>(json);

		/// <summary>
		///     Tunes to a channel on the current activity
		/// </summary>
		/// <param name="channel">The channel to tune to</param>
		/// <returns>When the channel has been changed</returns>
		public Task ChangeChannel(Channel channel) => this.ChangeChannel(channel.Number);

		/// <summary>
		///     Tunes to a channel on the current activity
		/// </summary>
		/// <param name="channelNumber">The number of the channel to tune to</param>
		/// <returns>When the channel has been changed</returns>
		public Task ChangeChannel(string channelNumber) => this.ChangeChannel(channelNumber, CancellationToken.None);

		/// <summary>
		///     Tunes to a channel on the current activity
		/// </summary>
		/// <param name="channel">The channel to tune to</param>
		/// <param name="cancellationToken">A token to use to cancel waiting for a successful channel change</param>
		/// <returns>When the channel has been changed</returns>
		public Task ChangeChannel(Channel channel, CancellationToken cancellationToken) =>
			this.ChangeChannel(channel.Number, cancellationToken);

		/// <summary>
		///     Tunes to a channel on the current activity
		/// </summary>
		/// <param name="channelNumber">The number of the channel to tune to</param>
		/// <param name="cancellationToken">A token to use to cancel waiting for a successful channel change</param>
		/// <returns>When the channel has been changed</returns>
		public async Task ChangeChannel(string channelNumber, CancellationToken cancellationToken) {
			ChangeChannelParams Params = new ChangeChannelParams(channelNumber, this.GetTimestamp());
			string CommandId = await this.Connection.SendCommand("harmony.engine?changeChannel", Params);

			EventWaiter<string, SuccessEventArgs> Waiter =
				new EventWaiter<string, SuccessEventArgs>(CommandId, cancellationToken);
			this.OnChannelChanged += Waiter.WaitHandler;
			await Waiter.CompletionTask;
			this.OnChannelChanged -= Waiter.WaitHandler;
		}

		/// <summary>
		///     Connects to the Hub asynchronously
		/// </summary>
		/// <param name="deviceID">A unique identifier for the device connecting</param>
		/// <returns>When the WebSocket has connected</returns>
		public async Task ConnectAsync(DeviceID deviceID) {
			this.Connection = new HubConnection(deviceID);
			this.Connection.OnMessageReceived += this.OnHarmonyMessageReceived;
			await this.Connection.Connect(this.Info.IP, this.Info.RemoteId);
			this.ConnectedAt = DateTime.Now;

			// start listening for messages
			this.Connection.StartListening();
		}

		/// <summary>
		///     Disconnects from the Hub asynchronously
		/// </summary>
		/// <returns>When the WebSocket has disconnected</returns>
		public Task Disconnect() => this.Connection.Disconnect();

		/// <inheritdoc />
		/// <summary>
		///     Disposes resources used by this <see cref="Hub" />
		/// </summary>
		public void Dispose() => this.Connection?.Dispose();

		/// <summary>
		///     Ends the currently running activity
		/// </summary>
		/// <returns>When the activity has ended</returns>
		public async Task EndActivity() =>
			await this.RunActivity(
				this.GetRunningActivity() ?? throw new HarmonyException("There is no running activity to end"),
				false,
				CancellationToken.None);

		/// <summary>
		///     Ends the currently running activity
		/// </summary>
		/// <param name="cancellationToken">A token to use to cancel waiting for the activity to completely stop</param>
		/// <returns>When the activity has ended</returns>
		public async Task EndActivity(CancellationToken cancellationToken) =>
			await this.RunActivity(
				this.GetRunningActivity() ?? throw new HarmonyException("There is no running activity to end"),
				false,
				cancellationToken);

		/// <summary>
		///     Executes a custom sequence
		/// </summary>
		/// <param name="sequence">The sequence to execute</param>
		/// <returns>When the sequence has executed</returns>
		public Task FireSequenceAsync(Sequence sequence) => this.FireSequenceAsync(sequence.ID);

		/// <summary>
		///     Executes a custom sequence
		/// </summary>
		/// <param name="sequenceId">The id of the sequence to execute</param>
		/// <returns>When the sequence has executed</returns>
		public Task FireSequenceAsync(string sequenceId) =>
			this.HoldActionAsync($"{{\"sequenceId\": {sequenceId}}}", "press");

		/// <summary>
		///     Gets an activity by its id
		/// </summary>
		/// <param name="activityId">The id of the activity to get</param>
		/// <returns>The activity, or null if none with the provided id exists</returns>
		public Activity GetActivityById(string activityId) =>
			this.Sync.Activities.FirstOrDefault(x => x.Id == activityId);

		/// <summary>
		///     Gets an activity by its label
		/// </summary>
		/// <param name="activityLabel">The label of the activity to get</param>
		/// <returns>The activity, or null if none with the provided label exists</returns>
		public Activity GetActivityByLabel(string activityLabel) =>
			this.Sync.Activities.FirstOrDefault(x => x.Label == activityLabel);

		/// <summary>
		///     Gets a device by its id
		/// </summary>
		/// <param name="deviceId">The id of the device to find</param>
		/// <returns>The device with a matching id, or null if none exists</returns>
		public Device GetDeviceById(string deviceId) => this.Sync?.Devices.FirstOrDefault(x => x.ID == deviceId);

		/// <summary>
		///     Gets a device by its model name
		/// </summary>
		/// <param name="model">The model of the device to find</param>
		/// <returns>The device with the matching model, or null if there is none</returns>
		public Device GetDeviceByModel(string model) => this.Sync.Devices.FirstOrDefault(x => x.Model == model);

		/// <summary>
		///     Gets a device by both its model and manufacturer
		/// </summary>
		/// <param name="model">The model name of the device to find</param>
		/// <param name="manufacturer">The manufacturer of the device to find</param>
		/// <returns>The device with the matching model and manufacturer, or null if there is none</returns>
		public Device GetDeviceByModel(string model, string manufacturer) =>
			this.Sync.Devices.FirstOrDefault(x => x.Model == model && x.Manufacturer == manufacturer);

		/// <summary>
		///     Gets the channels for a specific provider
		/// </summary>
		/// <param name="providerID">The television provider's ID</param>
		/// <returns>The <see cref="ProviderInfo" /></returns>
		public async Task<ProviderInfo> GetInfoForProvider(string providerID) {
			string RequestUrl = this.Sync.Content.ContentServiceHost.Replace("{providerId}", providerID);

			return await Hub.GetJSONData<ProviderInfo>(RequestUrl);
		}

		/// <summary>
		///     Gets the currently running activity
		/// </summary>
		/// <returns>The running activity, or the PowerOff activity if there is none running</returns>
		public Activity GetRunningActivity() => this.GetActivityById(this.GetRunningActivityId());

		/// <summary>
		///     Gets the id of the currently running activity
		/// </summary>
		/// <returns>The id of the currently running activity, or "-1" if there is none</returns>
		public string GetRunningActivityId() => this.State.ActivityId;

		/// <summary>
		///		Gets a value indicating whether there is an activity other than the PowerOff activity running
		/// </summary>
		/// <returns>True if an activity with an id other than "-1" is running, false otherwise</returns>
		public bool IsPoweredOn() => this.GetRunningActivityId() != "-1";

		/// <summary>
		///     Gets the user's location, favorite channels, and id from the server
		/// </summary>
		/// <returns>The retrieved <see cref="User" /></returns>
		public async Task<User> GetUserInfo() {
			string RequestUrl = this.Sync.Content.ContentUserHost.Replace(
				"{userProfileUri}",
				Uri.EscapeDataString(this.Sync.Content.HouseholdUserProfileUri));

			return await Hub.GetJSONData<User>(RequestUrl);
		}

		/// <summary>
		///     Holds a function down for a certain amount of time
		/// </summary>
		/// <param name="function">The function to hold down</param>
		/// <param name="milliseconds">The millisecond time to hold the function for</param>
		/// <returns>After <paramref name="milliseconds" /> milliseconds</returns>
		public async Task HoldFor(Function function, int milliseconds) {
			this.StartHolding(function);
			await Task.Delay(milliseconds);
			this.StopHolding();
		}

		/// <summary>
		///     Presses a button on a device, e.g. hitting play
		/// </summary>
		/// <param name="function">The function to execute</param>
		/// <returns>When the function has executed</returns>
		public Task PressButtonAsync(Function function) => this.HoldActionAsync(function, "press");

		/// <summary>
		///     Presses multiple buttons on the device, one after the other.
		/// </summary>
		/// <param name="delay">
		///     The delay between successive button presses
		/// </param>
		/// <param name="functions">The functions to execute</param>
		/// <returns>When all functions have executed</returns>
		public async Task PressButtonsAsync(int delay, IEnumerable<Function> functions) {
			// avoid enumerating twice
			Function[] Enumerable = functions as Function[] ?? functions.ToArray();

			if (!Enumerable.Any()) return;

			// call the first function immediately
			await this.PressButtonAsync(Enumerable.First());

			foreach (Function Function in Enumerable.Skip(1)) {
				await Task.Delay(delay);
				await this.PressButtonAsync(Function);
			}
		}

		/// <summary>
		///     Presses multiple buttons on the device, one after the other.
		/// </summary>
		/// <param name="delay">
		///     The delay between button presses.
		/// </param>
		/// <param name="functions">The functions to execute</param>
		/// <returns>When all functions have executed</returns>
		public Task PressButtonsAsync(int delay, params Function[] functions) =>
			this.PressButtonsAsync(delay, (IEnumerable<Function>)functions);

		/// <summary>
		///     Sends a <see cref="FixActivityAction" /> over the websocket
		/// </summary>
		/// <param name="action">The action to execute to fix the activity</param>
		/// <returns>When the action has been sent</returns>
		public async Task SendFixActivityAction(FixActivityAction action) {
			// TODO: test if this returns a command
			await this.Connection.SendCommand(
				"vnd.logitech.harmony/vnd.logitech.harmony.engine?helpSync",
				action.GetParameters(this.GetTimestamp()));
		}

		/// <summary>
		///     Starts an activity
		/// </summary>
		/// <param name="activity">The activity to start</param>
		/// <returns>When the activity has been completely started</returns>
		public Task StartActivity(Activity activity) => this.RunActivity(activity, true, CancellationToken.None);

		/// <summary>
		///     Starts an activity
		/// </summary>
		/// <param name="activity">The activity to start</param>
		/// <param name="token">A token to use to cancel waiting for the activity to finish starting</param>
		/// <returns>When the activity has been completely started</returns>
		public Task StartActivity(Activity activity, CancellationToken token) =>
			this.RunActivity(activity, true, token);

		/// <summary>
		///     Starts holding down the given function until <see cref="StopHolding" /> is called
		/// </summary>
		/// <param name="function">The function to hold down</param>
		public void StartHolding(Function function) {
			if (this.HeldDownFunction != null) throw new HarmonyException("Already holding a function down");
			this.HeldDownFunction = function;
			this.HoldFunction();
		}

		/// <summary>
		///     Stops holding down the currently held down function
		/// </summary>
		public void StopHolding() {
			this.HeldDownFunction = null;
		}

		/// <summary>
		///     Syncs the hub's devices, activities, etc. asynchronously
		/// </summary>
		/// <returns>The configuration data</returns>
		public Task<HubSync> SyncConfigurationAsync() => this.SyncConfigurationAsync(CancellationToken.None);

		/// <summary>
		///     Syncs the hub's devices, activities, etc. asynchronously
		/// </summary>
		/// <param name="cancellationToken">A token to use to cancel waiting for the configuration to be synchronized</param>
		/// <returns>The configuration data</returns>
		public async Task<HubSync> SyncConfigurationAsync(CancellationToken cancellationToken) {
			string CommandId =
				await this.Connection.SendCommand("vnd.logitech.harmony/vnd.logitech.harmony.engine?config");

			EventWaiter<HubSync> Waiter = new EventWaiter<HubSync>(CommandId, cancellationToken);
			this.OnHubSynchronized += Waiter.WaitHandler;
			HubSync Result = await Waiter.CompletionTask;
			this.OnHubSynchronized -= Waiter.WaitHandler;
			return Result;
		}

		/// <summary>
		///     Serializes the <see cref="Hub" /> into a JSON string
		/// </summary>
		/// <returns>The serialized JSON <see cref="Hub" /></returns>
		public string ToJson() => JsonConvert.SerializeObject(this);

		/// <summary>
		///     Updates the current state of the Hub (running activity, activity status, etc.)
		/// </summary>
		/// <returns>The Hub's state</returns>
		public Task<StateDigest> UpdateStateAsync() => this.UpdateStateAsync(CancellationToken.None);

		/// <summary>
		///     Updates the current state of the Hub (running activity, activity status, etc.)
		/// </summary>
		/// <param name="cancellationToken">A token to use to cancel waiting for the state to update</param>
		/// <returns>The Hub's state</returns>
		public async Task<StateDigest> UpdateStateAsync(CancellationToken cancellationToken) {
			string CommandId = await this.Connection.SendCommand("connect.statedigest?get", new FormatParams());

			EventWaiter<StateDigest> Waiter = new EventWaiter<StateDigest>(CommandId, cancellationToken);
			this.OnStateDigestReceived += Waiter.WaitHandler;
			StateDigest Result = await Waiter.CompletionTask;
			this.OnStateDigestReceived -= Waiter.WaitHandler;
			return Result;
		}

		/// <summary>
		///     Gets JSON data from a url
		/// </summary>
		/// <typeparam name="T">The type to deserialize the JSON as</typeparam>
		/// <param name="requestUrl">The URL to request</param>
		/// <returns>The deserialized <typeparamref name="T" /></returns>
		private static async Task<T> GetJSONData<T>(string requestUrl) {
			WebClient Client = new WebClient();
			Client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
			Client.Headers.Add(
				"Logitech-API-Key",
				"f206270e-26aa-4927-8630-fd230be3fbb7"); // value is hardcoded in the app
			return JsonConvert.DeserializeObject<T>(await Client.DownloadStringTaskAsync(requestUrl));
		}

		/// <summary>
		///     Tells the Hub to continue holding down the currently held down function
		/// </summary>
		/// <returns>When the command has executed</returns>
		private Task ContinueHoldingAsync() => this.HoldActionAsync(this.HeldDownFunction, "hold");

		/// <summary>
		///     Gets the current timestamp for HoldAction
		/// </summary>
		/// <returns>The time since the WebSocket connection was made, in ms</returns>
		private long GetTimestamp() => (DateTime.Now - this.ConnectedAt).Milliseconds;

		/// <summary>
		///     Executes a holdAction command on the hub
		/// </summary>
		/// <param name="function">The function to execute</param>
		/// <param name="status">The status, either press or hold</param>
		/// <returns>When the function has executed</returns>
		private Task HoldActionAsync(Function function, string status) =>
			this.HoldActionAsync(function?.Action, status);

		/// <summary>
		///     Executes a holdAction command on the hub
		/// </summary>
		/// <param name="action">The JSON of the action to execute</param>
		/// <param name="status">The status, either press or hold</param>
		/// <returns>When the function has executed</returns>
		private async Task HoldActionAsync(string action, string status) {
			if (action == null) return;

			HoldActionParams Parameters =
				new HoldActionParams { Action = action, Status = status, Timestamp = this.GetTimestamp() };

			await this.Connection.SendCommand(
				"vnd.logitech.harmony/vnd.logitech.harmony.engine?holdAction",
				Parameters);
		}

		/// <summary>
		///     Holds down the currently held function until it becomes null
		/// </summary>
		private async void HoldFunction() {
			while (this.HeldDownFunction != null) {
				await Task.Delay(200); // delay harmony uses
				await this.ContinueHoldingAsync();
			}
		}

		/// <summary>
		///     Event handler for when a Harmony message is received from this <see cref="Connection" />
		/// </summary>
		/// <param name="sender">The <see cref="HubConnection" /> that raised this event</param>
		/// <param name="e">The event arguments containing the Harmony message</param>
		private void OnHarmonyMessageReceived(object sender, StringResponseEventArgs e) {
			//// TODO: cyclomatic complexity is 13, make cleaner
			//// TODO: consolidate command strings into one class
			////Console.WriteLine("Received message: {0} {1}", e.Response.Command, e.Response.Code);
			switch (e.Response.Command) {
				case "harmony.engine?changeChannelFinished":
				case "harmony.engine?changeChannel":
					this.OnChannelChanged?.Invoke(this, new SuccessEventArgs(e.Response));
					break;
				case "harmony.engine?helpdiscretes" when e.Response.Code != 100:
				case "harmony.engine?startActivityFinished":
					ActivityStartedResponseData Data = e.Response.DeserializeAs<ActivityStartedResponseData>();
					if (Data.ActivityId == null) break;

					Activity Started = this.GetActivityById(Data.ActivityId);
					Response<Activity> ActivityResponse = new Response<Activity> {
						                                                             Code = e.Response.Code,
						                                                             Data = Started,
						                                                             Command = e.Response.Command,
						                                                             ID = e.Response.ID,
						                                                             Message = e.Response.Message
					                                                             };
					this.OnActivityRan?.Invoke(this, new HarmonyEventArgs<Activity>(ActivityResponse));
					break;
				case "harmony.engine?startActivity":
				case "harmony.engine?helpdiscretes" when e.Response.Code == 100:
					// we don't care about home automation state right now, ignore 200.2
					// TODO: care about it
					if (e.Response.Code == 200.2) break;
					ActivityProgressResponseData
						ProgressData = e.Response.DeserializeAs<ActivityProgressResponseData>();

					this.OnActivityProgress?.Invoke(
						this,
						new ActivityProgressEventArgs(
							this.GetRunningActivity(), // running activity is switched immediately
							ProgressData.Done / (double)ProgressData.Total));
					break;
				case "vnd.logitech.harmony/vnd.logitech.harmony.engine?config":
					HarmonyEventArgs<HubSync> SyncEventArgs = e.To<HubSync>();
					this.Sync = SyncEventArgs.Response.Data;
					this.OnHubSynchronized?.Invoke(this, SyncEventArgs);
					break;
				// yes the capitalization is different
				case "connect.statedigest?get":
				case "connect.stateDigest?notify":
					HarmonyEventArgs<StateDigest> StateEventArgs = e.To<StateDigest>();
					this.State = StateEventArgs.Response.Data;
					this.OnStateDigestReceived?.Invoke(this, StateEventArgs);
					break;
			}
		}

		/// <summary>
		///     Starts or stops an activity
		/// </summary>
		/// <param name="activity">The activity to run</param>
		/// <param name="start">True to start the activity, false to end it</param>
		/// <param name="cancellationToken">A token to use to cancel waiting for the activity to start</param>
		/// <returns>When the activity has completely started or stopped</returns>
		private async Task RunActivity(Activity activity, bool start, CancellationToken cancellationToken) {
			if (activity == null)
				throw new HarmonyException("Cannot run a null activity");

			RunActivityParams Parameters = new RunActivityParams(activity.Id, this.GetTimestamp().ToString(), start);

			string MessageId = await this.Connection.SendCommand("harmony.activityengine?runactivity", Parameters);

			// we will get four things here:
			/*
			 * 1. harmony.activityengine?runactivity, noting that the command was received
			 * 2. harmony.engine?startActivity (code 200.2), noting the current home automation state (we don't care about this one)
			 * 3. harmony.engine?startActivity (code 100), indicating the progress of starting the activity
			 * 4. harmony.engine?startActivityFinished, indicating that starting has finished
			 *
			 * We don't really care about the first two, but the last two will allow us to notify progress and completion
			 */

			// let events handle progress, and just return after it's finished
			// TODO: Fix and use message id again
			EventWaiter<Activity> Waiter = new EventWaiter<Activity>("harmony.engine?startActivityFinished"/*MessageId*/, cancellationToken);
			this.OnActivityRan += Waiter.WaitHandler;
			await Waiter.CompletionTask;
			this.OnActivityRan -= Waiter.WaitHandler;
		}
	}
}
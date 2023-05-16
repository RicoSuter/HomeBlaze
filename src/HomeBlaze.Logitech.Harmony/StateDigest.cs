// -----------------------------------------------------------------------
// <copyright file="StateDigest.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System.Collections.Generic;

	using Harmony.JSON;

	using Newtonsoft.Json;

	/// <summary>
	///     Response data including the current state of the Hub
	/// </summary>
	public class StateDigest {
		/// <summary>
		///     Gets or sets the id of the account this Hub is registered to
		/// </summary>
		[JsonProperty("accountId")]
		public string AccountId { get; set; }

		/// <summary>
		///     Gets or sets the id of the currently running activity
		/// </summary>
		[JsonProperty("activityId")]
		public string ActivityId { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the Hub is currently setting up an activity
		/// </summary>
		[JsonProperty("activitySetupState")]
		public bool ActivitySetupState { get; set; }

		[JsonProperty("activityStatus")]
		public int ActivityStatus { get; set; }

		[JsonProperty("configVersion")]
		public int ConfigVersion { get; set; }

		[JsonProperty("contentVersion")]
		public int ContentVersion { get; set; }

		/// <summary>
		///     Gets or sets a list of devices currently in the setup state
		/// </summary>
		[JsonProperty("deviceSetupState")]
		public string[] DeviceSetupState { get; set; }

		/// <summary>
		///     Gets or sets the remote endpoint for Hub discovery
		/// </summary>
		[JsonProperty("discoveryServer")]
		public string DiscoveryServer { get; set; }

		/// <summary>
		///     Gets or sets the error code of the Hub, 200 indicates no problems
		/// </summary>
		[JsonProperty("errorCode")]
		public int ErrorCode { get; set; }

		/// <summary>
		///     Gets or sets the current software version of the Hub
		/// </summary>
		[JsonProperty("hubSwVersion")]
		public string HubSwVersion { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the Hub needs to be updated or not
		/// </summary>
		[JsonProperty("hubUpdate")]
		public bool HubUpdate { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether inital Hub setup is complete
		/// </summary>

		[JsonProperty("isSetupComplete")]
		public bool IsSetupComplete { get; set; }

		[JsonProperty("mode")]
		public int Mode { get; set; }

		/// <summary>
		///     Gets or sets the id of the currently running activity
		/// </summary>
		[JsonProperty("runningActivityList")]
		public string RunningActivityList { get; set; }

		[JsonProperty("runningZoneList")]
		public object[] RunningZoneList { get; set; }

		[JsonProperty("sequence")]
		public bool Sequence { get; set; }

		/// <summary>
		///     Gets or sets the id of the currently running sleep timer, -1 if there is none
		/// </summary>
		[JsonProperty("sleepTimerId")]
		public int SleepTimerId { get; set; }

		[JsonProperty("stateVersion")]
		public int StateVersion { get; set; }

		[JsonProperty("syncStatus")]
		public int SyncStatus { get; set; }

		/// <summary>
		///     Gets or sets the current device time in ms since January 1, 1970
		/// </summary>
		[JsonProperty("time")]
		public int Time { get; set; }

		/// <summary>
		///     Gets or sets the current time zone in POSIX style
		/// </summary>
		[JsonProperty("tz")]
		public string Timezone { get; set; }

		/// <summary>
		///     Gets or sets the current time offset from UTC in seconds
		/// </summary>
		[JsonProperty("tzoffset")]
		public string TimezoneOffset { get; set; }

		[JsonProperty("updates")]
		[JsonConverter(typeof(SingleOrArrayConverter<Dictionary<string, string>>))]
		public List<Dictionary<string, string>> Updates { get; set; }

		[JsonProperty("wifiStatus")]
		public int WifiStatus { get; set; }
	}
}
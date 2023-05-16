// -----------------------------------------------------------------------
// <copyright file="FixActivityParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Parameters to call the fix activity command with
	/// </summary>
	internal class FixActivityParams : CommandParams {
		/// <summary>
		///     Initializes a new instance of the <see cref="FixActivityParams" /> class.
		/// </summary>
		/// <param name="deviceID">The ID of the device to change the state of</param>
		/// <param name="state">The state to change. Either "Power" or "Input"</param>
		/// <param name="timestamp">An arbitrary timestamp</param>
		/// <param name="value">The value of the state to change</param>
		public FixActivityParams(string deviceID, string state, long timestamp, string value) {
			this.DeviceID = deviceID;
			this.State = state;
			this.Timestamp = timestamp;
			this.Value = value;
		}

		/// <summary>
		///     Gets or sets the ID of the device to change the state of
		/// </summary>
		[JsonProperty("deviceId")]
		public string DeviceID { get; set; }

		/// <summary>
		///     Gets or sets the state to change. Either "Power" or "Input"
		/// </summary>
		[JsonProperty("state")]
		public string State { get; set; }

		/// <summary>
		///     Gets or sets an arbitrary timestamp
		/// </summary>
		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }

		/// <summary>
		///     Gets or sets the value of the state to change
		/// </summary>
		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
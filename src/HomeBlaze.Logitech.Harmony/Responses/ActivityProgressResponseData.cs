// -----------------------------------------------------------------------
// <copyright file="ActivityProgressResponseData.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Responses {
	using Newtonsoft.Json;

	/// <summary>
	///     Response data for the harmony.engine?startActivity "Continue" response.
	///     This message is sent when the hub has progressed in starting/stopping an activity
	/// </summary>
	internal class ActivityProgressResponseData {
		/// <summary>
		///     Gets or sets the last powered on or off device's id
		/// </summary>
		[JsonProperty("deviceId")]
		public string DeviceID { get; set; }

		/// <summary>
		///     Gets or sets the number of devices that have been powered on or off
		/// </summary>
		[JsonProperty("done")]
		public int Done { get; set; }

		/// <summary>
		///     Gets or sets the total number of devices that need to be powered on or off
		/// </summary>
		[JsonProperty("total")]
		public int Total { get; set; }
	}
}
// -----------------------------------------------------------------------
// <copyright file="ActivityStartedResponseData.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Responses {
	using Newtonsoft.Json;

	/// <summary>
	///     Response data for the "harmony.engine?startActivityFinished" command, sent when a Hub has finished starting or
	///     stopping an activity.
	/// </summary>
	internal class ActivityStartedResponseData {
		/// <summary>
		///     Gets or sets the ID of the activity that has finished starting or stopping
		/// </summary>
		[JsonProperty("activityId")]
		public string ActivityId { get; set; }

		/// <summary>
		///     Gets or sets the error code associated with this response, 200 indicates success
		/// </summary>
		[JsonProperty("errorCode")]
		public double ErrorCode { get; set; }

		/// <summary>
		///     Gets or sets the error message associated with this response, "OK" indicates success
		/// </summary>
		[JsonProperty("errorString")]
		public string ErrorString { get; set; }
	}
}
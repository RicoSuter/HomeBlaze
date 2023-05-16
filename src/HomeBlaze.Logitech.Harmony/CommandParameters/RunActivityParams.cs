// -----------------------------------------------------------------------
// <copyright file="RunActivityParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using System.Collections.Generic;

	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Parameters used for starting an activity
	/// </summary>
	internal class RunActivityParams : CommandParams {
		/// <summary>
		///     Initializes a new instance of the <see cref="RunActivityParams" /> class.
		/// </summary>
		/// <param name="activityId">The id of the activity to start</param>
		/// <param name="timestamp">The timestamp for the activity</param>
		/// <param name="start">True to start the activity, false to end it</param>
		public RunActivityParams(string activityId, string timestamp, bool start) {
			this.ActivityId = activityId;
			this.Timestamp = timestamp;
			this.Args = new Dictionary<string, string> { { "rule", start ? "start" : "end" } };
		}

		/// <summary>
		///     Gets or sets the id of the activity to start
		/// </summary>
		[JsonProperty("activityId")]
		public string ActivityId { get; set; }

		/// <summary>
		///     Gets arguments to start the activity with
		/// </summary>
		[JsonProperty("args")]
		public Dictionary<string, string> Args { get; }

		/// <summary>
		///     Gets a value indicating whether the activity should be started asynchrnously or not
		/// </summary>
		[JsonProperty("async")]
		public bool Async => true;

		/// <summary>
		///     Gets or sets a timestamp in ms starting from an arbitrary point
		/// </summary>
		[JsonProperty("timestamp")]
		public string Timestamp { get; set; }
	}
}
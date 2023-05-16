// -----------------------------------------------------------------------
// <copyright file="HoldActionParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Parameters for the HoldAction command
	/// </summary>
	internal class HoldActionParams : CommandParams {
		/// <summary>
		///     Gets or sets the action to perform
		/// </summary>
		[JsonProperty("action")]
		public string Action { get; set; }

		/// <summary>
		///     Gets or sets how to perform this action, e.g. hold or press
		/// </summary>
		[JsonProperty("status")]
		public string Status { get; set; }

		/// <summary>
		///     Gets or sets a relative timestamp for this action in milliseconds
		/// </summary>
		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }
	}
}
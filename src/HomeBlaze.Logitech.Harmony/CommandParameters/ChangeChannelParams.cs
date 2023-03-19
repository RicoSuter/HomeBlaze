// -----------------------------------------------------------------------
// <copyright file="ChangeChannelParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Parameters for changing channels
	/// </summary>
	internal class ChangeChannelParams : CommandParams {
		/// <summary>
		///     Initializes a new instance of the <see cref="ChangeChannelParams" /> class.
		/// </summary>
		/// <param name="channelNumber">The number of the channel to change to</param>
		/// <param name="timestamp">The timestamp of when this command will be sent</param>
		public ChangeChannelParams(string channelNumber, long timestamp) {
			this.ChannelNumber = channelNumber;
			this.Timestamp = timestamp;
		}

		/// <summary>
		///     Gets the number of the channel to tune to
		/// </summary>
		[JsonProperty("channel")]
		public string ChannelNumber { get; }

		/// <summary>
		///     Gets the timestamp of when this command is sent
		/// </summary>
		[JsonProperty("timestamp")]
		public long Timestamp { get; }
	}
}
// -----------------------------------------------------------------------
// <copyright file="IncomingMessage.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Responses {
	using Harmony.JSON;

	using Newtonsoft.Json;

	/// <summary>
	///     An incoming WebSocket message from a Hub
	/// </summary>
	public class IncomingMessage {
		/// <summary>
		///     Gets or sets the serialized response data JSON
		/// </summary>
		[JsonProperty("data")]
		[JsonConverter(typeof(KeepSerializedConverter))]
		public string Data { get; set; }

		/// <summary>
		///     Gets or sets the type of this message
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }

		/// <summary>
		///     Gets this message as a <see cref="StringResponse" />
		/// </summary>
		/// <returns>A <see cref="StringResponse" /> with default status code and message values</returns>
		public StringResponse ToStringResponse() =>
			new StringResponse {
				                   Command = this.Type,
				                   Data = this.Data,
				                   Code = 200,
				                   ID = null,
				                   Message = "Success"
			                   };
	}
}
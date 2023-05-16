// -----------------------------------------------------------------------
// <copyright file="Response.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Responses {
	using Harmony.JSON;

	using Newtonsoft.Json;

	/// <summary>
	///     A Harmony Hub WebSocket response message
	/// </summary>
	/// <typeparam name="T">The type of the response data</typeparam>
	public class Response<T> {
		/// <summary>
		///     Gets or sets the status code for the response, which can have a decimal component. 200 indicates success
		/// </summary>
		[JsonProperty("code")]
		public double Code { get; set; }

		/// <summary>
		///     Gets or sets the name of the command that was executed
		/// </summary>
		[JsonProperty("cmd")]
		public string Command { get; set; }

		/// <summary>
		///     Gets or sets the response data object
		/// </summary>
		[JsonProperty("data")]
		public virtual T Data { get; set; }

		/// <summary>
		///     Gets or sets the ID that was used to send the request
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the status message
		/// </summary>
		[JsonProperty("msg")]
		public string Message { get; set; }
	}

	/// <inheritdoc />
	/// <summary>
	///     A Harmony Hub WebSocket response with string data
	/// </summary>
	public class StringResponse : Response<string> {
		/// <summary>
		///     Gets or sets the serialized response data JSON
		/// </summary>
		[JsonProperty("data")]
		[JsonConverter(typeof(KeepSerializedConverter))]
		public override string Data { get; set; }

		/// <summary>
		///     Deserializes this response's data into the specified type
		/// </summary>
		/// <typeparam name="T">The type to deserialize into</typeparam>
		/// <returns>The deserialized object</returns>
		public T DeserializeAs<T>() => JsonConvert.DeserializeObject<T>(this.Data);

		/// <summary>
		///     Deserializes this response into a <see cref="Response{T}" /> with the specified data type
		/// </summary>
		/// <typeparam name="T">The type to deserialize this response's data into</typeparam>
		/// <returns>A <see cref="Response{T}" /> with this data in it</returns>
		public Response<T> DeserializeResponseAs<T>() =>
			new Response<T> {
				                Data = this.DeserializeAs<T>(),
				                Code = this.Code,
				                Command = this.Command,
				                ID = this.ID,
				                Message = this.Message
			                };
	}
}
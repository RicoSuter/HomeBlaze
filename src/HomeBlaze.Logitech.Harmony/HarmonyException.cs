// -----------------------------------------------------------------------
// <copyright file="HarmonyException.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System;

	/// <inheritdoc />
	/// <summary>
	///     An exception caused by a Harmony related method
	/// </summary>
	internal class HarmonyException : Exception {
		/// <inheritdoc />
		/// <summary>
		///     Initializes a new instance of the <see cref="T:Harmony.HarmonyException" /> class.
		/// </summary>
		/// <param name="message">The exception message</param>
		public HarmonyException(string message)
			: base(message) { }
	}

	/// <inheritdoc />
	/// <summary>
	///     An exception caused by a failed WebSocket command
	/// </summary>
	internal class HarmonyWebSocketException : Exception {
		/// <inheritdoc />
		/// <summary>
		///     Initializes a new instance of the <see cref="T:Harmony.HarmonyWebSocketException" /> class.
		/// </summary>
		/// <param name="statusCode">The status code for the failed response</param>
		/// <param name="statusMessage">The status message for the failed response</param>
		public HarmonyWebSocketException(double statusCode, string statusMessage) {
			this.StatusCode = statusCode;
			this.StatusMessage = statusMessage;
		}

		/// <inheritdoc />
		public override string Message =>
			$"Failed to execute websocket command on the Hub. Status code: {this.StatusCode}, status message: {this.StatusMessage}";

		/// <summary>
		///     Gets the status code for the failed response
		/// </summary>
		public double StatusCode { get; }

		/// <summary>
		///     Gets the status message for the failed response
		/// </summary>
		public string StatusMessage { get; }
	}
}
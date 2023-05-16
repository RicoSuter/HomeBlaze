// -----------------------------------------------------------------------
// <copyright file="HarmonyEventArgs.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Events {
	using System;

	using Harmony.Responses;

	/// <summary>
	///     Event arguments for a Harmony WebSocket message
	/// </summary>
	/// <typeparam name="TData">The type of data received in this message</typeparam>
	public class HarmonyEventArgs<TData> : EventArgs {
		/// <summary>
		///     Initializes a new instance of the <see cref="HarmonyEventArgs{TData}" /> class.
		/// </summary>
		/// <param name="response">The response data received in the message</param>
		public HarmonyEventArgs(Response<TData> response) => this.Response = response;

		/// <summary>
		///     Gets the response data received in the message
		/// </summary>
		public Response<TData> Response { get; }
	}
}
// -----------------------------------------------------------------------
// <copyright file="StringResponseEventArgs.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Events {
	using System;

	using Harmony.Responses;

	/// <summary>
	///     Event arguments for a message received from a Harmony Hub
	/// </summary>
	public class StringResponseEventArgs : EventArgs {
		/// <summary>
		///     Initializes a new instance of the <see cref="StringResponseEventArgs" /> class.
		/// </summary>
		/// <param name="response">The response received from the Hub</param>
		public StringResponseEventArgs(StringResponse response) => this.Response = response;

		/// <summary>
		///     Gets response received from the Hub
		/// </summary>
		public StringResponse Response { get; }

		/// <summary>
		///     Gets this <see cref="StringResponseEventArgs" /> as a deserialized <see cref="HarmonyEventArgs{TData}" />
		/// </summary>
		/// <typeparam name="TData">The type of data stored in the response object</typeparam>
		/// <returns>A <see cref="HarmonyEventArgs{TData}" /> containing a <see cref="Response{TData}" /></returns>
		public HarmonyEventArgs<TData> To<TData>() =>
			new HarmonyEventArgs<TData>(this.Response.DeserializeResponseAs<TData>());
	}
}
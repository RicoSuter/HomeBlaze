// -----------------------------------------------------------------------
// <copyright file="SuccessEventArgs.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Events {
	using System;

	using Harmony.Responses;

	/// <summary>
	///     Event arguments that indicates whether a command was successful or not
	/// </summary>
	public class SuccessEventArgs : HarmonyEventArgs<string> {
		/// <summary>
		///     Initializes a new instance of the <see cref="SuccessEventArgs" /> class.
		/// </summary>
		/// <param name="response">The response data from the command</param>
		public SuccessEventArgs(StringResponse response)
			: base(response) =>
			this.Success = Math.Abs(response.Code - 200) < 1;

		/// <summary>
		///     Gets a value indicating whether or not the command was successful
		/// </summary>
		public bool Success { get; }
	}
}
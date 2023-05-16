// -----------------------------------------------------------------------
// <copyright file="FormatParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Parameters requesting a JSON-formatted response
	/// </summary>
	internal class FormatParams : CommandParams {
		/// <summary>
		///     Gets the requested format for the response for this command
		/// </summary>
		[JsonProperty("format")]
		public string Format => "json";
	}
}
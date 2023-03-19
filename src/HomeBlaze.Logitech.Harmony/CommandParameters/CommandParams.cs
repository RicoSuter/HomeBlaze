// -----------------------------------------------------------------------
// <copyright file="CommandParams.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.CommandParameters {
	using Newtonsoft.Json;

	/// <summary>
	///     Parameters for a WebSocket command
	/// </summary>
	internal abstract class CommandParams {
		/// <summary>
		///     Deserializes the parameters from a JSON string
		/// </summary>
		/// <param name="text">The JSON text to deserialize</param>
		/// <returns>The deserialized command parameters</returns>
		public CommandParams Deserialize(string text) =>
			(CommandParams)JsonConvert.DeserializeObject(text, this.GetType());
	}
}
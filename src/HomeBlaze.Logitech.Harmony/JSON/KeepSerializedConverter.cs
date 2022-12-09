// -----------------------------------------------------------------------
// <copyright file="KeepSerializedConverter.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.JSON {
	using System;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <inheritdoc />
	/// <summary>
	///     A JSON converter that will keep serialized JSON as a string
	/// </summary>
	internal class KeepSerializedConverter : JsonConverter<string> {
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override string? ReadJson(
			JsonReader reader,
			Type objectType,
			string? existingValue,
			bool hasExistingValue,
			JsonSerializer serializer) {
			JToken Token = JToken.Load(reader);
			return Token.Type == JTokenType.Object ? Token.ToString() : null;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer) =>
			throw new NotImplementedException();
	}
}
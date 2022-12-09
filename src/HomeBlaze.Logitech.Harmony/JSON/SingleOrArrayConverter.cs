// -----------------------------------------------------------------------
// <copyright file="SingleOrArrayConverter.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.JSON {
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	///     JSON converter that deserializes a JSON token into an array. If the token is a single object, it deserializes it
	///     into an array with the token as the sole element.
	/// </summary>
	/// <remarks>
	///     Stolen from https://stackoverflow.com/a/18997172
	/// </remarks>
	/// <typeparam name="T">The type of element being converted</typeparam>
	internal class SingleOrArrayConverter<T> : JsonConverter {
		/// <inheritdoc/>
		public override bool CanWrite => false;

		/// <inheritdoc/>
		public override bool CanConvert(Type objectType) => objectType == typeof(List<T>);

		/// <inheritdoc/>
		public override object? ReadJson(
			JsonReader reader,
			Type objectType,
			object? existingValue,
			JsonSerializer serializer) {
			JToken Token = JToken.Load(reader);
			if (Token.Type == JTokenType.Array) return Token.ToObject<List<T>>();
			return new List<T> { Token.ToObject<T>()! };
		}

		/// <inheritdoc/>
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
	}
}
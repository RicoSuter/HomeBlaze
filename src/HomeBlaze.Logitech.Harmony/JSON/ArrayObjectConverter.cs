// -----------------------------------------------------------------------
// <copyright file="ArrayObjectConverter.cs" company="John Lynch">
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
	///     Converts an empty array to null, or an object to <typeparamref name="T" />
	/// </summary>
	internal class ArrayObjectConverter<T> : JsonConverter<T> {
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override T ReadJson(
			JsonReader reader,
			Type objectType,
			T existingValue,
			bool hasExistingValue,
			JsonSerializer serializer) {
			switch (reader.TokenType) {
				case JsonToken.Null:
					return (T)(object)null;
				case JsonToken.StartArray:
					reader.Read();

					// yeah, I know
					return (T)(object)null;
				default:
					JObject JObject = JObject.Load(reader);
					return JObject.ToObject<T>();
			}
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) =>
			throw new NotImplementedException();
	}
}
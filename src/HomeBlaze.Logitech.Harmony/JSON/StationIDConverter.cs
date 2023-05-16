// -----------------------------------------------------------------------
// <copyright file="StationIDConverter.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.JSON {
	using System;
	using System.Linq;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <inheritdoc />
	/// <summary>
	///     Converts an array of station ids to an array of strings
	/// </summary>
	internal class StationIDConverter : JsonConverter {
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType) => true;

		/// <inheritdoc />
		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer) {
			// we are given an array of station id objects
			return JArray.Load(reader).Children().Select(child => child.SelectToken("id").Value<string>()).ToArray();
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
			throw new NotImplementedException();
	}
}
// -----------------------------------------------------------------------
// <copyright file="ActivityTypeConverter.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.JSON {
	using System;
	using System.Diagnostics.CodeAnalysis;

	using Newtonsoft.Json;

	/// <inheritdoc />
	/// <summary>
	///     Converts <see cref="ActivityType" />s from JSON
	/// </summary>
	internal class ActivityTypeConverter : JsonConverter<ActivityType> {
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		[SuppressMessage("ReSharper", "CyclomaticComplexity", Justification = "Enum switch case requires >10 paths")]
		public override ActivityType ReadJson(
			JsonReader reader,
			Type objectType,
			ActivityType existingValue,
			bool hasExistingValue,
			JsonSerializer serializer) {
			if (reader.Value is long Value) return (ActivityType)Value;

			switch ((string)reader.Value) {
				case "VirtualTelevisionN":
					return ActivityType.WatchTV;
				case "VirtualDvd":
					return ActivityType.WatchMovie;
				case "VirtualCdMulti":
					return ActivityType.PlayMusic;
				case "VirtualGameConsole":
					return ActivityType.PlayGame;
				case "SkypeCall":
					return ActivityType.SkypeCall;
				case "WatchGoogleTVLogitech":
					return ActivityType.WatchGTV;
				case "VirtualGeneric":
					return ActivityType.Generic;
				case "WatchRoku":
					return ActivityType.WatchRoku;
				case "WatchAppleTV":
					return ActivityType.WatchAppleTV;
				case "PC-TV":
					return ActivityType.PCTV;
				case "SmartTV":
					return ActivityType.WatchSmartTV;
				case "WatchFireTV":
					return ActivityType.WatchFireTV;
				case "ListenToSonos":
					return ActivityType.PlaySonos;
				case "ListenToHeos":
					return ActivityType.PlayHeos;
				default:
					return ActivityType.Generic;
			}
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, ActivityType value, JsonSerializer serializer) =>
			throw new NotImplementedException();
	}
}
// -----------------------------------------------------------------------
// <copyright file="User.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System.Collections.Generic;
	using System.Linq;

	using Harmony.JSON;

	using Newtonsoft.Json;

	/// <summary>
	///     A Harmony Hub's user
	/// </summary>
	public class User {
		/// <summary>
		///     Gets or sets the user's unique id
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the user's location
		/// </summary>
		[JsonProperty("location")]
		public Location Location { get; set; }

		/// <summary>
		///     Gets or sets the user's favorite channel preferences
		/// </summary>
		[JsonProperty("preferences")]
		public Preferences Preferences { get; set; }

		/// <summary>
		///     Gets or sets the user's profile URI (used in conjunction with the content user host url)
		/// </summary>
		[JsonProperty("user_profile_uri")]
		public string UserProfileUri { get; set; }
	}

	/// <inheritdoc />
	/// <summary>
	///     Preferences for TV Providers keyed to Device IDs
	/// </summary>
	public class Preferences : Dictionary<string, ProviderList> {
		/// <summary>
		///     Gets all TV providers for the given device
		/// </summary>
		/// <param name="device">The device to get the providers </param>
		/// <returns>The list of providers for that device</returns>
		public ProviderList GetProvidersForDevice(Device device) => this[device.ID];
	}

	/// <inheritdoc />
	/// <summary>
	///     Dictionary of favorite providers keyed to TV Provider IDs
	/// </summary>
	public class ProviderList : Dictionary<string, ProviderFavorites> {
		/// <summary>
		///     Gets the favorite channels for the default (first) TV provider
		/// </summary>
		public ProviderFavorites DefaultFavoriteChannels => this.Values.First();

		/// <summary>
		///     Gets the provider id for the default (first) TV provider
		/// </summary>
		public string DefaultProviderId => this.Keys.First();
	}

	/// <summary>
	///     A provider's favorite channels
	/// </summary>
	public class ProviderFavorites {
		/// <summary>
		///     Gets or sets the user's favorite channels under this provider
		/// </summary>
		[JsonProperty("stations")]
		[JsonConverter(typeof(StationIDConverter))]
		public string[] StationIDs { get; set; }
	}

	/// <summary>
	///     A favorite station ID
	/// </summary>
	public class StationID {
		/// <summary>
		///     Gets or sets the station's id
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }
	}

	/// <summary>
	///     A user's location
	/// </summary>
	public class Location {
		/// <summary>
		///     Gets or sets the user's two character country code
		/// </summary>
		[JsonProperty("country")]
		public string Country { get; set; }

		/// <summary>
		///     Gets or sets the user's IETF language tag
		/// </summary>
		[JsonProperty("locale")]
		public string Locale { get; set; }

		/// <summary>
		///     Gets or sets the user's postal (zip) code.
		/// </summary>
		[JsonProperty("postalCode")]
		public string PostalCode { get; set; }
	}
}

/*
	Find: "^\t\tpublic .*? ([a-z_]*) "
	Replace: "\t\t[JsonProperty("$1")]\n$0"
	 */
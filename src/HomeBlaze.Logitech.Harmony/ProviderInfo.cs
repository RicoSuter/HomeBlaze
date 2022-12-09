// -----------------------------------------------------------------------
// <copyright file="ProviderInfo.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System.Linq;

	using Newtonsoft.Json;

	/// <summary>
	///     A television provider
	/// </summary>
	public class ProviderInfo {
		/// <summary>
		///     Gets or sets the list of channels on this provider
		/// </summary>
		[JsonProperty("channels")]
		public Channel[] Channels { get; set; }

		/// <summary>
		///     Gets or sets the provider's unique id
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the location code for this provider
		/// </summary>
		[JsonProperty("location")]
		public string Location { get; set; }

		/// <summary>
		///     Gets or sets the full name of this provider, including location
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		///     Gets or sets the name of the provider's operator.
		/// </summary>
		[JsonProperty("operatorName")]
		public string OperatorName { get; set; }

		/// <summary>
		///     Gets a list of the most popular channels, sorted by their popularity
		/// </summary>
		public IOrderedEnumerable<Channel> PopularChannels =>
			this.Channels.Where(x => x.Popular).OrderBy(x => x.PopularRank);

		/// <summary>
		///     Gets or sets the service type, e.g. "Satellite"
		/// </summary>
		[JsonProperty("serviceType")]
		public string ServiceType { get; set; }

		/// <summary>
		///     Gets a channel by its number
		/// </summary>
		/// <param name="stationNumber">The <see cref="Channel.Number" /> of the channel to find</param>
		/// <returns>The channel with the matching number, or null if there is none</returns>
		public Channel GetChannelByNumber(string stationNumber) =>
			this.Channels.FirstOrDefault(x => x.Number == stationNumber);

		/// <summary>
		///     Gets a channel by its station's full name
		/// </summary>
		/// <param name="fullName">The <see cref="Station.FullName" /> of the channel to find</param>
		/// <returns>The channel with the matching full name, or null if there is none</returns>
		public Channel GetChannelByStationFullName(string fullName) =>
			this.Channels.FirstOrDefault(x => x.Station.FullName == fullName);

		/// <summary>
		///     Gets a channel by its station's id
		/// </summary>
		/// <param name="stationID">The <see cref="Station.ID" /> of the channel to find</param>
		/// <returns>The channel with the matching id, or null if there is none</returns>
		public Channel GetChannelByStationID(string stationID) =>
			this.Channels.FirstOrDefault(x => x.Station.ID == stationID);

		/// <summary>
		///     Gets a channel by its station's short name
		/// </summary>
		/// <param name="shortName">The <see cref="Station.ShortName" /> of the channel to find</param>
		/// <returns>The channel with the matching short name, or null if there is none</returns>
		public Channel GetChannelByStationShortName(string shortName) =>
			this.Channels.FirstOrDefault(x => x.Station.ShortName == shortName);
	}

	/// <summary>
	///     A television channel
	/// </summary>
	public class Channel {
		/// <summary>
		///     Gets or sets the channel's number
		/// </summary>
		[JsonProperty("number")]
		public string Number { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the channel is popular or not
		/// </summary>
		[JsonProperty("popular")]
		public bool Popular { get; set; }

		/// <summary>
		///     Gets or sets the popularity rank of the channel, from 1 to 30
		/// </summary>
		[JsonProperty("popularRank")]
		public int PopularRank { get; set; }

		/// <summary>
		///     Gets or sets the station broadcasted by this channel
		/// </summary>
		[JsonProperty("station")]
		public Station Station { get; set; }
	}

	/// <summary>
	///     A TV station (separate from a channel)
	/// </summary>
	public class Station {
		/// <summary>
		///     Gets or sets the station's affiliation number. This will be the same for associated stations
		/// </summary>
		[JsonProperty("affiliation")]
		public int Affiliation { get; set; }

		/// <summary>
		///     Gets or sets the stations broadcast attributes
		/// </summary>
		[JsonProperty("attributes")]
		public string[] Attributes { get; set; }

		/// <summary>
		///     Gets or sets the stations full name
		/// </summary>
		[JsonProperty("fullName")]
		public string FullName { get; set; }

		/// <summary>
		///     Gets or sets the station's unique Harmony ID
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the station's short name
		/// </summary>
		[JsonProperty("shortName")]
		public string ShortName { get; set; }
	}
}
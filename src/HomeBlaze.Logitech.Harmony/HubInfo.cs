// -----------------------------------------------------------------------
// <copyright file="HubInfo.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public class HubInfo {
		[JsonProperty("accountId")]
		public string AccountId { get; set; }

		[JsonProperty("current_fw_version")]
		public string CurrentFirmwareVersion { get; set; }

		[JsonProperty("discoveryServerUri")]
		public string DiscoveryServerUri { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("friendlyName")]
		public string FriendlyName { get; set; }

		[JsonProperty("host_name")]
		public string HostName { get; set; }

		[JsonProperty("hubId")]
		public string HubId { get; set; }

		[JsonProperty("hubProfiles")]
		public string HubProfiles { get; set; }

		[JsonProperty("ip")]
		public string IP { get; set; }

		[JsonProperty("minimumOpenApiClientVersionRequired")]
		public string MinimumOpenApiClientVersionRequired { get; set; }

		[JsonProperty("mode")]
		public string Mode { get; set; }

		[JsonProperty("oohEnabled")]
		public string OohEnabled { get; set; }

		[JsonProperty("openApiVersion")]
		public string OpenApiVersion { get; set; }

		[JsonProperty("port")]
		public string Port { get; set; }

		[JsonProperty("productId")]
		public string ProductId { get; set; }

		[JsonProperty("protocolVersion")]
		public string ProtocolVersion { get; set; }

		[JsonProperty("recommendedOpenApiClientVersion")]
		public string RecommendedOpenApiClientVersion { get; set; }

		[JsonProperty("remoteId")]
		public string RemoteId { get; set; }

		[JsonProperty("setupSessionClient")]
		public string SetupSessionClient { get; set; }

		[JsonProperty("setupSessionIsStale")]
		public string SetupSessionIsStale { get; set; }

		[JsonProperty("setupSessionSetupType")]
		public string SetupSessionSetupType { get; set; }

		[JsonProperty("setupSessionType")]
		public string SetupSessionType { get; set; }

		[JsonProperty("setupStatus")]
		public string SetupStatus { get; set; }

		[JsonProperty("uuid")]
		public string UUID { get; set; }

		/// <summary>
		///     Parses hub info from a string
		/// </summary>
		/// <param name="str">A string with ; separated properties and : separated keys</param>
		/// <returns>The parsed <see cref="HubInfo" /></returns>
		public static HubInfo Parse(string str) {
			string[] Properties = str.Split(';');

			// write to JObject to make it easy to deserialize
			JObject JObject = new JObject();
			foreach (string Property in Properties) {
				string[] KeyValue = Property.Split(':');
				JObject.Add(KeyValue[0], JToken.FromObject(KeyValue[1]));
			}

			return JObject.ToObject<HubInfo>();
		}
	}
}
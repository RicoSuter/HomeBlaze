// -----------------------------------------------------------------------
// <copyright file="DeviceID.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System;

	/// <summary>
	///     Represents a device id for sending messages
	/// </summary>
	public class DeviceID {
		/// <summary>
		///     The device model name
		/// </summary>
		private readonly string DeviceModelName;

		/// <summary>
		///     The device product name
		/// </summary>
		private readonly string DeviceProductName;

		/// <summary>
		///     A unique identifier for this device, can be a random UUID
		/// </summary>
		private readonly string DeviceUUID;

		/// <summary>
		///     A random integer between 100-1000 that remains constant for the life of this id, which is appended to the message
		///     id
		/// </summary>
		private readonly int MessageKey;

		/// <summary>
		///     A number starting at 0 that increases by 1 with each message sent and is appended to the message id
		/// </summary>
		private int MessageIndex;

		/// <summary>
		///     Initializes a new instance of the <see cref="DeviceID" /> class.
		/// </summary>
		/// <param name="deviceUUID">A unique identifier for this device, can be a random UUID</param>
		/// <param name="deviceProductName">The device product name</param>
		/// <param name="deviceModelName">The device model name</param>
		public DeviceID(string deviceUUID, string deviceProductName, string deviceModelName) {
			this.DeviceUUID = deviceUUID;
			this.DeviceProductName = deviceProductName;
			this.DeviceModelName = deviceModelName;

			// create random message client identifier
			Random Random = new Random();
			this.MessageKey = Random.Next(100, 1000);
		}

		/// <summary>
		///     Gets a <see cref="DeviceID" /> using this device's parameters and a random UUID
		/// </summary>
		/// <returns>A <see cref="DeviceID" /> with this device's information</returns>
		public static DeviceID GetDeviceDefault() =>
			new DeviceID(
				Guid.NewGuid().ToString("N"),
				Environment.OSVersion.Platform.ToString(),
				Environment.OSVersion.ToString().Replace(" ", string.Empty));

		/// <summary>
		///     Gets the string identifier for the next message sent
		/// </summary>
		/// <returns>A string that can be used to identify a WebSocket message</returns>
		public string Next() =>
			$"{this.DeviceUUID}#{this.DeviceProductName}#{this.DeviceModelName}-{this.MessageKey}-{this.MessageIndex++}";
	}
}
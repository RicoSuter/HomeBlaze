// -----------------------------------------------------------------------
// <copyright file="DeviceCapability.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	/// <summary>
	///     A capability that a device has, such as volume control
	/// </summary>
	public enum DeviceCapability {
		/// <summary>
		///     The device has no capabilities
		/// </summary>
		None = 0,

		/// <summary>
		///     The device can control volume
		/// </summary>
		Volume = 1,

		/// <summary>
		///     The device can change display input
		/// </summary>
		Display = 2,

		/// <summary>
		///     The device can change channels
		/// </summary>
		ChangingChannel = 3,

		/// <summary>
		///     The device can be used for playing games
		/// </summary>
		PlayGame = 4,

		/// <summary>
		///     The device can be used for playing media
		/// </summary>
		PlayMedia = 5,

		/// <summary>
		///     The device can be used for playing a movie
		/// </summary>
		PlayMovie = 6,

		/// <summary>
		///     The device can run Logitech Google TV smart software
		/// </summary>
		RunLogitechGoogleTV = 7,

		/// <summary>
		///     The device can switch inputs
		/// </summary>
		InputSwitching = 8,

		/// <summary>
		///     The device supports Netflix playback
		/// </summary>
		ControlsNetflix = 9,

		/// <summary>
		///     The device can access the internet
		/// </summary>
		AccessInternet = 10,

		/// <summary>
		///     The device supports video calling
		/// </summary>
		VideoCalling = 11,

		/// <summary>
		///     The device can control an Apple TV
		/// </summary>
		ControlsAppleTv = 12,

		/// <summary>
		///     The device can control a Roku device
		/// </summary>
		ControlsRoku = 13,

		/// <summary>
		///     The device is a home automation hub
		/// </summary>
		AutomationGateway = 14,

		/// <summary>
		///     The device controls a computer
		/// </summary>
		ControlsComputer = 15,

		/// <summary>
		///     The device can enter keyboard text
		/// </summary>
		KeyboardTextEntry = 16,

		/// <summary>
		///     The device supports Bluetooth remotes
		/// </summary>
		BluetoothHid = 17,

		/// <summary>
		///     The device supports USB remotes
		/// </summary>
		USBHid = 18,

		/// <summary>
		///     The device supports Wi-Fi control
		/// </summary>
		IPControl = 19,

		/// <summary>
		///     The device supports Bluetooth Audio/Video Remote Control Profile
		/// </summary>
		BluetoothAvrcp = 20,

		/// <summary>
		///     The device is a home automation device
		/// </summary>
		HomeAutomation = 21,

		/// <summary>
		///     The device runs MacOS
		/// </summary>
		MacOS = 22,

		/// <summary>
		///     The device runs Windows
		/// </summary>
		WindowsOS = 23,

		/// <summary>
		///     The device is a smart TV
		/// </summary>
		IsSmartTV = 24,

		/// <summary>
		///     The device supports control via a trackpad
		/// </summary>
		TrackpadHid = 25,

		/// <summary>
		///     The device supports trackpad acceleration
		/// </summary>
		AcceleratedTrackpadHid = 26,

		/// <summary>
		///     The device supports different channels
		/// </summary>
		Channels = 27,

		/// <summary>
		///     The device supports metadata on the content it is playing
		/// </summary>
		ContentMetaData = 28,

		/// <summary>
		///     The device has a single IP to be controlled with
		/// </summary>
		SingleIPDevice = 29,

		/// <summary>
		///     The device supports infrared control
		/// </summary>
		Infrared = 30,

		/// <summary>
		///     The device doesn't support text entry
		/// </summary>
		NoTextEntrySupport = 31,

		/// <summary>
		///     The device controls a media player
		/// </summary>
		ControlsMediaPlayer = 32,

		/// <summary>
		///     The device controls speakers
		/// </summary>
		ControlsSpeakers = 33
	}
}
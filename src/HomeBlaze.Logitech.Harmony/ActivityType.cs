// -----------------------------------------------------------------------
// <copyright file="ActivityType.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	/// <summary>
	///     The type of an activity, i.e. what the user does by starting it
	/// </summary>
	public enum ActivityType {
		/// <summary>
		///     The activity is used for watching television
		/// </summary>
		WatchTV,

		/// <summary>
		///     The activity is used for watching a movie
		/// </summary>
		WatchMovie,

		/// <summary>
		///     The activity is used for playing music
		/// </summary>
		PlayMusic,

		/// <summary>
		///     The activity is used for playing video games
		/// </summary>
		PlayGame,

		/// <summary>
		///     The activity is used for making a video call
		/// </summary>
		SkypeCall,

		/// <summary>
		///     The activity is used for watching a Logitech Google TV
		/// </summary>
		WatchGTV,

		/// <summary>
		///     This is a generic activity
		/// </summary>
		Generic,

		/// <summary>
		///     The activity is used for watching on a Roku device
		/// </summary>
		WatchRoku,

		/// <summary>
		///     The activity is used for watching on an Apple TV device
		/// </summary>
		WatchAppleTV,

		/// <summary>
		///     The activity is used for watching television
		/// </summary>
		PCTV,

		/// <summary>
		///     The activity is used for watching a smart television
		/// </summary>
		WatchSmartTV,

		/// <summary>
		///     The activity is used for watching on a Fire TV
		/// </summary>
		WatchFireTV,

		/// <summary>
		///     The activity is used for playing audio on Sonos speakers
		/// </summary>
		PlaySonos,

		/// <summary>
		///     The activity is used for playing audio on Heos speakers
		/// </summary>
		PlayHeos
	}
}
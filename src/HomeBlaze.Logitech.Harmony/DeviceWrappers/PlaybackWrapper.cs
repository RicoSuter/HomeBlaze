// -----------------------------------------------------------------------
// <copyright file="PlaybackWrapper.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.DeviceWrappers {
	/// <inheritdoc />
	/// <summary>
	///     A wrapper that adds convenience playback functions
	/// </summary>
	public class PlaybackWrapper : NumericNavigationWrapper {
		/// <inheritdoc />
		public PlaybackWrapper(Controllable controllable)
			: base(controllable) { }

		/// <summary>
		///     The eject function
		/// </summary>
		public Function Eject => this.Controllable.GetFunctionByName("Eject");

		/// <summary>
		///     The fast forward function
		/// </summary>
		public Function FastForward => this.Controllable.GetFunctionByName("FastForward");

		/// <summary>
		///     The pause function
		/// </summary>
		public Function Pause => this.Controllable.GetFunctionByName("Pause");

		/// <summary>
		///     The play function
		/// </summary>
		public Function Play => this.Controllable.GetFunctionByName("Play");

		/// <summary>
		///     The rewind function
		/// </summary>
		public Function Rewind => this.Controllable.GetFunctionByName("Rewind");

		/// <summary>
		///     The skip backward function
		/// </summary>
		public Function SkipBackward => this.Controllable.GetFunctionByName("SkipBackward");

		/// <summary>
		///     The skip forward function
		/// </summary>
		public Function SkipForward => this.Controllable.GetFunctionByName("SkipForward");

		/// <summary>
		///     The stop function
		/// </summary>
		public Function Stop => this.Controllable.GetFunctionByName("Stop");

		/// <summary>
		///     The volume down function
		/// </summary>
		public Function VolumeDown => this.Controllable.GetFunctionByName("VolumeDown");

		/// <summary>
		///     The volume up function
		/// </summary>
		public Function VolumeUp => this.Controllable.GetFunctionByName("VolumeUp");
	}
}
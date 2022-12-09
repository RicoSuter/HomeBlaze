// -----------------------------------------------------------------------
// <copyright file="DeviceWrapper.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.DeviceWrappers {
	/// <summary>
	///     A wrapper that adds convenience properties to a device
	/// </summary>
	public class DeviceWrapper {
		/// <summary>
		///     Initializes a new instance of the <see cref="DeviceWrapper" /> class.
		/// </summary>
		/// <param name="controllable">The device to wrap</param>
		protected DeviceWrapper(Controllable controllable) => this.Controllable = controllable;

		/// <summary>
		///     The power off function
		/// </summary>
		public Function PowerOffFunction => this.Controllable.GetFunctionByName("PowerOff");

		/// <summary>
		///     The power on function
		/// </summary>
		public Function PowerOnFunction => this.Controllable.GetFunctionByName("PowerOn");

		/// <summary>
		///     The toggle power function
		/// </summary>
		public Function PowerToggleFunction => this.Controllable.GetFunctionByName("PowerToggle");

		/// <summary>
		///     Gets the device this wrapper wraps around
		/// </summary>
		protected Controllable Controllable { get; }
	}
}
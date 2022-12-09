// -----------------------------------------------------------------------
// <copyright file="NumericNavigationWrapper.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.DeviceWrappers {
	/// <inheritdoc />
	/// <summary>
	///     A wrapper that adds convenience numeric functions (0-9, -, Enter, #), and navigation functions (up, down, left,
	///     right, enter, etc.)
	/// </summary>
	public class NumericNavigationWrapper : DeviceWrapper {
		/// <inheritdoc />
		public NumericNavigationWrapper(Controllable controllable)
			: base(controllable) { }

		/// <summary>
		///     The channel down function
		/// </summary>
		public Function ChannelDown => this.Controllable.GetFunctionByName("ChannelDown");

		/// <summary>
		///     The channel up function
		/// </summary>
		public Function ChannelUp => this.Controllable.GetFunctionByName("ChannelUp");

		/// <summary>
		///     The dash (star) function
		/// </summary>
		public Function DashStar => this.Controllable.GetFunctionByName("Star");

		/// <summary>
		///     The down direction function
		/// </summary>
		public Function Down => this.Controllable.GetFunctionByName("DirectionDown");

		/// <summary>
		///     The enter number function
		/// </summary>
		public Function EnterNumber => this.Controllable.GetFunctionByName("NumberEnter");

		/// <summary>
		///     The exit function
		/// </summary>
		public Function Exit => this.Controllable.GetFunctionByName("Exit");

		/// <summary>
		///     The info function
		/// </summary>
		public Function Info => this.Controllable.GetFunctionByName("Info");

		/// <summary>
		///     The left direction function
		/// </summary>
		public Function Left => this.Controllable.GetFunctionByName("DirectionLeft");

		/// <summary>
		///     The menu function
		/// </summary>
		public Function Menu => this.Controllable.GetFunctionByName("Menu");

		/// <summary>
		///     The mute function
		/// </summary>
		public Function Mute => this.Controllable.GetFunctionByName("Mute");

		/// <summary>
		///     The number 0 function
		/// </summary>
		public Function Number0 => this.Controllable.GetFunctionByName("Number0");

		/// <summary>
		///     The number 1 function
		/// </summary>
		public Function Number1 => this.Controllable.GetFunctionByName("Number1");

		/// <summary>
		///     The number 2 function
		/// </summary>
		public Function Number2 => this.Controllable.GetFunctionByName("Number2");

		/// <summary>
		///     The number 3 function
		/// </summary>
		public Function Number3 => this.Controllable.GetFunctionByName("Number3");

		/// <summary>
		///     The number 4 function
		/// </summary>
		public Function Number4 => this.Controllable.GetFunctionByName("Number4");

		/// <summary>
		///     The number 5 function
		/// </summary>
		public Function Number5 => this.Controllable.GetFunctionByName("Number5");

		/// <summary>
		///     The number 6 function
		/// </summary>
		public Function Number6 => this.Controllable.GetFunctionByName("Number6");

		/// <summary>
		///     The number 7 function
		/// </summary>
		public Function Number7 => this.Controllable.GetFunctionByName("Number7");

		/// <summary>
		///     The number 8 function
		/// </summary>
		public Function Number8 => this.Controllable.GetFunctionByName("Number8");

		/// <summary>
		///     The number 9 function
		/// </summary>
		public Function Number9 => this.Controllable.GetFunctionByName("Number9");

		/// <summary>
		///     The pound (#) function
		/// </summary>
		public Function Pound => this.Controllable.GetFunctionByName("Pound");

		/// <summary>
		///     The return function
		/// </summary>
		public Function Return => this.Controllable.GetFunctionByName("Return");

		/// <summary>
		///     The right direction function
		/// </summary>
		public Function Right => this.Controllable.GetFunctionByName("DirectionRight");

		/// <summary>
		///     The select function
		/// </summary>
		public Function Select => this.Controllable.GetFunctionByName("Select");

		/// <summary>
		///     The up direction function
		/// </summary>
		public Function Up => this.Controllable.GetFunctionByName("DirectionUp");
	}
}
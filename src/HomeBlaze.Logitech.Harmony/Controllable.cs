// -----------------------------------------------------------------------
// <copyright file="Controllable.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	///     A device or activity that can be controlled using <see cref="Function" />s.
	/// </summary>
	public abstract class Controllable {
		/// <summary>
		///     Gets or sets a list of control groups for the controllable, i.e. a list of device functions and their categories
		/// </summary>
		public abstract ControlGroup[] ControlGroups { get; set; }

		/// <summary>
		///     Gets a list of all functions that can be executed on this controllable, such as play or pause
		/// </summary>
		public IEnumerable<Function> Functions => this.ControlGroups.SelectMany(group => group.Functions);

		/// <summary>
		///     Gets a control group by its name
		/// </summary>
		/// <param name="name">The name of the control group</param>
		/// <returns>The control group matching the name, or null if there is none</returns>
		public ControlGroup GetControlGroupByName(string name) =>
			this.ControlGroups.FirstOrDefault(x => x.Name == name);

		/// <summary>
		///     Gets a function by its name
		/// </summary>
		/// <param name="name">The name of the function (not the label)</param>
		/// <returns>The function matching the name, or null if there is none</returns>
		public Function GetFunctionByName(string name) => this.Functions.FirstOrDefault(x => x.Name == name);
	}
}
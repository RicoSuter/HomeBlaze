// -----------------------------------------------------------------------
// <copyright file="ActivityProgressEventArgs.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Events {
	using System;

	/// <inheritdoc />
	/// <summary>
	///     Event arguments for activity start or stop progress
	/// </summary>
	public class ActivityProgressEventArgs : EventArgs {
		/// <inheritdoc />
		/// <summary>
		///     Initializes a new instance of the <see cref="ActivityProgressEventArgs" /> class.
		/// </summary>
		/// <param name="activity">The activity that is starting or stopping</param>
		/// <param name="progress">The activity's starting or stopping progress</param>
		public ActivityProgressEventArgs(Activity activity, double progress) {
			this.Activity = activity;
			this.Progress = progress;
		}

		/// <summary>
		///     Gets the activity that is starting or stopping
		/// </summary>
		public Activity Activity { get; }

		/// <summary>
		///     Gets the activity's starting or stopping progress
		/// </summary>
		public double Progress { get; }
	}
}
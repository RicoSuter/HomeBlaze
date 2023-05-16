// -----------------------------------------------------------------------
// <copyright file="EventWaiter.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony.Events {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	///     Object that enables converting event-based async to task-based async
	/// </summary>
	/// <typeparam name="TData">The type of the data contained in the event arguments</typeparam>
	/// <typeparam name="TEventArgs">
	///     The type of event arguments, extending from <see cref="HarmonyEventArgs{TData}" />
	/// </typeparam>
	internal class EventWaiter<TData, TEventArgs>
		where TEventArgs : HarmonyEventArgs<TData> {
		/// <summary>
		///     The source for the completion <see cref="Task" />
		/// </summary>
		private readonly TaskCompletionSource<TData> CompletionSource;

		/// <summary>
		///     The ID of the command this object is waiting for
		/// </summary>
		private readonly string WaitingForId;

		/// <summary>
		///     Initializes a new instance of the <see cref="EventWaiter{TData,TEventArgs}" /> class.
		/// </summary>
		/// <param name="waitingForId">The ID of the command this object is waiting for</param>
		/// <param name="cancellationToken">A cancellation token to use to cancel waiting for the event</param>
		public EventWaiter(string waitingForId, CancellationToken cancellationToken) {
			this.WaitingForId = waitingForId;
			this.CompletionSource = new TaskCompletionSource<TData>();

			this.WaitHandler = (sender, e) => {
				if (e.Response.ID != this.WaitingForId) return;
				this.CompletionSource.TrySetResult(e.Response.Data);
			};

			cancellationToken.Register(() => this.CompletionSource.TrySetCanceled());
		}

		/// <summary>
		///     Gets the task to await that will resolve when the event has been raised
		/// </summary>
		public Task<TData> CompletionTask => this.CompletionSource.Task;

		/// <summary>
		///     Gets the handler to subscribe an event to to wait for
		/// </summary>
		public EventHandler<TEventArgs> WaitHandler { get; }
	}

	/// <summary>
	///     Object that enables converting event-based async to task-based async
	/// </summary>
	/// <typeparam name="TData">The type of the data contained in the event arguments</typeparam>
	internal class EventWaiter<TData> : EventWaiter<TData, HarmonyEventArgs<TData>> {
		/// <summary>
		///     Initializes a new instance of the <see cref="EventWaiter{TData}" /> class.
		/// </summary>
		/// <param name="waitingForId">The ID of the command this object is waiting for</param>
		/// <param name="cancellationToken">A cancellation token to use to cancel waiting for the event</param>
		public EventWaiter(string waitingForId, CancellationToken cancellationToken)
			: base(waitingForId, cancellationToken) { }
	}
}
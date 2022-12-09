// -----------------------------------------------------------------------
// <copyright file="Command.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using Harmony.CommandParameters;

	using Newtonsoft.Json;

	/// <summary>
	///     A WebSocket command
	/// </summary>
	internal class Command {
		/// <summary>
		///     Initializes a new instance of the <see cref="Command" /> class.
		/// </summary>
		/// <param name="commandName">The name of the command to execute</param>
		/// <param name="parameters">The parameters to execute the command with</param>
		/// <param name="id">The id to send this command with</param>
		public Command(string commandName, CommandParams? parameters, string id) {
			this.CommandName = commandName;
			this.Parameters = parameters ?? new EmptyParams();
			this.ID = id;
		}

		/// <inheritdoc />
		/// <summary>
		///     Initializes a new instance of the <see cref="Command" /> class with no parameters.
		/// </summary>
		/// <param name="commandName">The name of the command to execute</param>
		/// <param name="id">The id to send this command with</param>
		public Command(string commandName, string id)
			: this(commandName, null, id) { }

		/// <summary>
		///     Gets or sets the name of the command to execute
		/// </summary>
		[JsonProperty("cmd")]
		public string CommandName { get; set; }

		/// <summary>
		///     Gets or sets the id to send this command with
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets any command parameters
		/// </summary>
		[JsonProperty("params")]
		public CommandParams Parameters { get; set; }
	}
}
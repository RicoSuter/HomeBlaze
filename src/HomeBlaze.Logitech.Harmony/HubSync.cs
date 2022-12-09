// -----------------------------------------------------------------------
// <copyright file="HubSync.cs" company="John Lynch">
//   This file is licensed under the MIT license.
//   Copyright (c) 2018 John Lynch
// </copyright>
// -----------------------------------------------------------------------

namespace Harmony {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Harmony.CommandParameters;
	using Harmony.JSON;

	using Newtonsoft.Json;

	/// <summary>
	///     A hub configuration
	/// </summary>
	public class HubSync {
		/// <summary>
		///     Gets or sets all activities on the Hub
		/// </summary>
		[JsonProperty("activity")]
		public Activity[] Activities { get; set; }

		/// <summary>
		///     Gets or sets content host URLs for the Hub
		/// </summary>
		[JsonProperty("content")]
		public Content Content { get; set; }

		/// <summary>
		///     Gets or sets all the devices on the Hub
		/// </summary>
		[JsonProperty("device")]
		public Device[] Devices { get; set; }

		/// <summary>
		///     Gets or sets global information about the Hub
		/// </summary>
		[JsonProperty("global")]
		public Global Global { get; set; }

		/// <summary>
		///     Gets or sets the list of all button sequences on the Hub
		/// </summary>
		[JsonProperty("sequence")]
		public Sequence[] Sequences { get; set; }

		/// <summary>
		///     Gets or sets information about the latest SLA
		/// </summary>
		[JsonProperty("sla")]
		public SLA SLA { get; set; }
	}

	/// <summary>
	///     A button sequence.
	/// </summary>
	public class Sequence {
		/// <summary>
		///     Gets or sets the sequence's id
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the name of the sequence
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	/// <summary>
	///     Global information relevant to all aspects of the device
	/// </summary>
	public class Global {
		/// <summary>
		///     Gets or sets the hub's locale as an IETF language tag
		/// </summary>
		[JsonProperty("locale")]
		public string Locale { get; set; }

		/// <summary>
		///     Gets or sets information about the current time and user account
		/// </summary>
		[JsonProperty("timeStampHash")]
		public string TimeStampHash { get; set; }
	}

	/// <summary>
	///     Information about the acceptance of the latest SLA
	/// </summary>
	public class SLA {
		/// <summary>
		///     Gets or sets a value indicating whether the latest SLA has been accepted
		/// </summary>
		[JsonProperty("latestSLAAccepted")]
		public bool LatestSLAAccepted { get; set; }

		/// <summary>
		///     Gets or sets the date of when the last SLA was accepted
		/// </summary>
		[JsonProperty("latestSLAAcceptedDate")]
		public DateTime LatestSLAAcceptedDate { get; set; }
	}

	/// <summary>
	///     A list of useful urls to request information about stations and favorite channels in the user's area
	/// </summary>
	public class Content {
		/// <summary>
		///     Gets or sets the url to use to request information about a device's favorite channels
		/// </summary>
		[JsonProperty("contentDeviceHost")]
		public string ContentDeviceHost { get; set; }

		/// <summary>
		///     Gets or sets the url to use to request images of a certain TV station by its id
		/// </summary>
		[JsonProperty("contentImageHost")]
		public string ContentImageHost { get; set; }

		/// <summary>
		///     Gets or sets the url to use to request information about the channels on a certain TV provider
		/// </summary>
		[JsonProperty("contentServiceHost")]

		public string ContentServiceHost { get; set; }

		/// <summary>
		///     Gets or sets the url to use to request information about the user's favorite channels
		/// </summary>
		[JsonProperty("contentUserHost")]
		public string ContentUserHost { get; set; }

		/// <summary>
		///     Gets or sets the url to use as a token for other host urls
		/// </summary>
		[JsonProperty("householdUserProfileUri")]
		public string HouseholdUserProfileUri { get; set; }
	}

	/// <inheritdoc />
	/// <summary>
	///     Represents a Hub Activity
	/// </summary>
	public class Activity : Controllable {
		/// <summary>
		///     Gets or sets the order of the activity relative to others
		/// </summary>
		[JsonProperty("activityOrder")]
		public int ActivityOrder { get; set; }

		/// <summary>
		///     Gets or sets the activity type display name (unused in the Harmony app)
		/// </summary>
		[JsonProperty("activityTypeDisplayName")]
		public string ActivityTypeDisplayName { get; set; }

		/// <summary>
		///     Gets or sets the base domain to request images from
		/// </summary>
		[JsonProperty("baseImageUri")]
		public string BaseImageUri { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that changes channels
		/// </summary>
		[JsonProperty("ChannelChangingActivityRole")]
		public string ChannelChangingActivityRole { get; set; }

		/// <inheritdoc />
		/// <summary>
		///     Gets or sets the list of control groups for this activity
		/// </summary>
		[JsonProperty("controlGroup")]
		public override ControlGroup[] ControlGroups { get; set; }

		/// <summary>
		///     Gets or sets a list of actions that should be executed after starting the activity
		/// </summary>
		[JsonProperty("enterActions")]
		public EnterAction[] EnterActions { get; set; }

		/// <summary>
		///     Gets or sets a list of device ids keyed to their fix activity actions
		/// </summary>
		[JsonProperty("fixit")]
		public Dictionary<string, FixActivityAction> FixActivityActions { get; set; }

		/// <summary>
		///     Gets or sets the icon to display with this activity, if <see cref="ImageKey" /> is not present
		/// </summary>
		[JsonProperty("icon")]
		public string Icon { get; set; }

		/// <summary>
		///     Gets or sets the numeric id of this activity
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		///     Gets or sets the path of this activity's icon relative to <see cref="BaseImageUri" /> if present
		/// </summary>
		[JsonProperty("imageKey")]
		public string ImageKey { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this activity controls audio/visual equipment. E.g. Watch TV is an AV
		///     activity, but Lights On is not
		/// </summary>
		[JsonProperty("isAVActivity")]
		public bool IsAVActivity { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this activity can control multiple zones
		/// </summary>
		[JsonProperty("isMultiZone")]
		public bool IsMultiZone { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether to fetch favorites from online or not
		/// </summary>
		[JsonProperty("isTuningDefault")]
		public bool IsTuningDefault { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that enters text
		/// </summary>
		[JsonProperty("KeyboardTextEntryActivityRole")]
		public string KeyboardTextEntryActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the human readable label of this activity
		/// </summary>
		[JsonProperty("label")]
		public string Label { get; set; }

		/// <summary>
		///     Gets or sets roles particular devices have in controlling this activity
		/// </summary>
		[JsonProperty("roles")]
		[JsonConverter(typeof(ArrayObjectConverter<Roles>))]
		public Roles Roles { get; set; }

		/// <summary>
		///     Gets or sets the rules that the activity can be run with. Typically these are ["start", "end"]
		/// </summary>
		[JsonProperty("rules")]
		public string[] Rules { get; set; }

		/// <summary>
		///     Gets or sets a list of sequence ids
		/// </summary>
		[JsonProperty("sequences")]
		public string[] Sequences { get; set; }

		/// <summary>
		///     Gets or sets the suggested layout for this activity
		/// </summary>
		[JsonProperty("suggestedDisplay")]
		public string SuggestedDisplay { get; set; }

		/// <summary>
		///     Gets or sets the type of activity, e.g. VirtualGameConsole
		/// </summary>
		[JsonProperty("type")]
		[JsonConverter(typeof(ActivityTypeConverter))]
		public ActivityType Type { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that controls the volume in this activity
		/// </summary>
		[JsonProperty("VolumeActivityRole")]
		public string VolumeActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the list of json-serialized zones that this activity belongs to
		/// </summary>
		[JsonProperty("zones")]
		public string[] Zones { get; set; }
	}

	/// <summary>
	///     The device roles for an activity
	/// </summary>
	public class Roles {
		/// <summary>
		///     Gets or sets the id of the device that changes channels
		/// </summary>
		public string ChannelChangingActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device controlling a Roku device
		/// </summary>
		public string ControlsRokuActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device controlling Sonos speakers
		/// </summary>
		public string ControlsSonosActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device controlling the speakers. See also <seealso cref="VolumeActivityRole" />
		/// </summary>
		public string ControlsSpeakerActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that displays the media
		/// </summary>
		public string DisplayActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that enters text
		/// </summary>
		public string KeyboardTextEntryActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device using HDMI pass through
		/// </summary>
		public string PassThroughActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that controls the game
		/// </summary>
		public string PlayGameActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device playing media
		/// </summary>
		public string PlayMediaActivityRole { get; set; }

		/// <summary>
		///     Gets or sets the id of the device that controls the volume. See also <seealso cref="ControlsSpeakerActivityRole" />
		/// </summary>
		public string VolumeActivityRole { get; set; }
	}

	/// <summary>
	///     Represents an action that can be executed to fix an activity that hasn't started correctly
	/// </summary>
	public class FixActivityAction {
		/// <summary>
		///     Gets or sets the id of the device to fix
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets the input to switch the device to. If null, don't switch input
		/// </summary>
		[JsonProperty("Input")]
		public string Input { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the device is (or should) always be on
		/// </summary>
		[JsonProperty("isAlwaysOn")]
		public bool IsAlwaysOn { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this action toggles the power or not, i.e. each time this action is
		///     executed the power will switch, or will simply turn "on" or "off" every time
		/// </summary>
		[JsonProperty("isRelativePower")]
		public bool IsRelativePower { get; set; }

		/// <summary>
		///     Gets or sets whether to turn the power "On" or "Off"
		/// </summary>
		[JsonProperty("Power")]
		public string Power { get; set; }

		/// <summary>
		///     Gets FixActivityParams representing this action
		/// </summary>
		/// <param name="timestamp">An arbitrary timestamp</param>
		/// <returns>FixActivityParams either changing input or power</returns>
		internal FixActivityParams GetParameters(long timestamp) =>
			this.Power == null
				? new FixActivityParams(this.ID, "Input", timestamp, this.Input)
				: new FixActivityParams(this.ID, "Power", timestamp, this.Power);
	}

	/// <summary>
	///     Represents an actions that should be executed after starting an activity
	/// </summary>
	public class EnterAction {
		/// <summary>
		///     Gets or sets the name of the command, e.g. "SendDelay" or "SendCommand"
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		///     Gets or sets any extra parameters needed for this enter action
		/// </summary>
		[JsonProperty("parameters")]
		public Parameters Parameters { get; set; }
	}

	/// <summary>
	///     Represents a list of parameters for an Activity's enter action
	/// </summary>
	public class Parameters {
		/// <summary>
		///     Gets or sets the name of the command to send, if any
		/// </summary>
		[JsonProperty("Command")]
		public string Command { get; set; }

		/// <summary>
		///     Gets or sets the time, in milliseconds, to delay before continuing enter actions
		/// </summary>
		[JsonProperty("DelayValue")]
		public int DelayValue { get; set; }

		/// <summary>
		///     Gets or sets the device id to execute the command on, if any
		/// </summary>
		[JsonProperty("DeviceId")]
		public string DeviceID { get; set; }

		/// <summary>
		///     Gets or sets the command modifier, e.g. "press"
		/// </summary>
		[JsonProperty("Modifier")]
		public string Modifier { get; set; }
	}

	/// <summary>
	///     Represents a list of functions grouped under a common category, such as navigation
	/// </summary>
	public class ControlGroup {
		/// <summary>
		///     Gets or sets an array of functions that fall under this category
		/// </summary>
		[JsonProperty("function")]
		public Function[] Functions { get; set; }

		/// <summary>
		///     Gets or sets the internal name identifier for this group
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		///     Gets a function by its name
		/// </summary>
		/// <param name="name">The name of the function (not the label)</param>
		/// <returns>The function matching the name, or null if there is none</returns>
		public Function GetFunctionByName(string name) => this.Functions.FirstOrDefault(x => x.Name == name);
	}

	/// <summary>
	///     Represents a function that can be executed on a device, like a play command
	/// </summary>
	public class Function {
		/// <summary>
		///     Gets or sets the JSON formatted action used to execute this function
		/// </summary>
		[JsonProperty("action")]
		public string Action { get; set; }

		/// <summary>
		///     Gets or sets the display label for this function
		/// </summary>
		[JsonProperty("label")]
		public string Label { get; set; }

		/// <summary>
		///     Gets or sets the internal name identifier for this function
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	/// <inheritdoc />
	/// <summary>
	///     Represents a device that can be controlled with the Harmony Hub
	/// </summary>
	public class Device : Controllable {
		/// <summary>
		///     Gets or sets the date the device was added to the hub
		/// </summary>
		[JsonProperty("deviceAddedDate")]
		public DateTime AddedDate { get; set; }

		/// <summary>
		///     Gets or sets the Bluetooth MAC address of the device if Bluetooth is the primary communication channel
		/// </summary>
		[JsonProperty("BTAddress")]
		public string BTAddress { get; set; }

		/// <summary>
		///     Gets or sets all of the capabilities of the device
		/// </summary>
		[JsonProperty("Capabilities")]
		public DeviceCapability[] Capabilities { get; set; }

		/// <summary>
		///     Gets or sets the content profile key, which is equal to the number in string <code>ID</code>
		/// </summary>
		[JsonProperty("contentProfileKey")]
		public int ContentProfileKey { get; set; }

		/// <inheritdoc />
		/// <summary>
		///     Gets or sets a list of control groups for the device, i.e. a list of device functions and their categories
		/// </summary>
		[JsonProperty("controlGroup")]
		public override ControlGroup[] ControlGroups { get; set; }

		/// <summary>
		///     Gets or sets the port the device is controlled on. 0 indicates this device is controlled by another Harmony remote
		/// </summary>
		[JsonProperty("ControlPort")]
		public int ControlPort { get; set; }

		/// <summary>
		///     Gets or sets the URI used along with Content.ContentDeviceHost to learn information about favorite channels on this
		///     device
		/// </summary>
		[JsonProperty("deviceProfileUri")]
		public string DeviceProfileUri { get; set; }

		/// <summary>
		///     Gets or sets the display name of the device type, this is almost always the same as <code>Type</code>
		/// </summary>
		[JsonProperty("deviceTypeDisplayName")]
		public string DeviceTypeDisplayName { get; set; }

		/// <summary>
		///     Gets or sets the dongle RFID, if present (default 0)
		/// </summary>
		[JsonProperty("DongleRFID")]
		public int DongleRFID { get; set; }

		/// <summary>
		///     Gets or sets the index of the icon representing this device
		/// </summary>
		[JsonProperty("icon")]
		public string Icon { get; set; }

		/// <summary>
		///     Gets or sets a unique numerical identifier for the device
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether there should be a keyboard provided to provide input to the device
		/// </summary>
		[JsonProperty("IsKeyboardAssociated")]
		public bool IsKeyboardAssociated { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the power can be controlled via the Hub or not
		/// </summary>
		[JsonProperty("isManualPower")]
		public bool IsManualPower { get; set; }

		/// <summary>
		///     Gets or sets a human readable full name for the device, typically including both the manufacturer and model name
		/// </summary>
		[JsonProperty("label")]
		public string Label { get; set; }

		/// <summary>
		///     Gets or sets the device manufacturer's name
		/// </summary>
		[JsonProperty("manufacturer")]
		public string Manufacturer { get; set; }

		/// <summary>
		///     Gets or sets the device's model name, e.g. "Xbox One"
		/// </summary>
		[JsonProperty("model")]
		public string Model { get; set; }

		/// <summary>
		///     Gets or sets the functions that can be used to either power on or power off the device
		/// </summary>
		[JsonProperty("powerFeatures")]
		[JsonConverter(typeof(ArrayObjectConverter<PowerFeatures>))]
		public PowerFeatures PowerFeatures { get; set; }

		/// <summary>
		///     Gets or sets the suggested type of layout that this device should be represented on, e.g. PlayGame
		/// </summary>
		[JsonProperty("suggestedDisplay")]
		public string SuggestedDisplay { get; set; }

		/// <summary>
		///     Gets or sets how the device is controlled, e.g. 1 is IR controlled, 32 is bluetooth, 65 wifi
		/// </summary>
		[JsonProperty("Transport")]
		public int Transport { get; set; }

		/// <summary>
		///     Gets or sets the type of device, e.g. "StereoReceiver"
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }

		/// <summary>
		///     Gets or sets a UUID for the device, not present on all devices
		/// </summary>
		[JsonProperty("uuid")]
		public string UUID { get; set; }
	}

	/// <summary>
	///     Represents actions that can be taken to turn a device on and off
	/// </summary>
	public class PowerFeatures {
		/// <summary>
		///     Gets or sets a list of actions that turn a device off
		/// </summary>
		public PowerAction[] PowerOffActions { get; set; }

		/// <summary>
		///     Gets or sets a list of actions that turn a device on
		/// </summary>
		public PowerAction[] PowerOnActions { get; set; }
	}

	/// <summary>
	///     Represents an action that can be taken to turn a device either on or off
	/// </summary>
	public class PowerAction {
		/// <summary>
		///     Gets or sets an id of the action, relative to other items in the list
		/// </summary>
		public int ActionId { get; set; }

		/// <summary>
		///     Gets or sets how long the command should be executed for, in milliseconds. If null, duration is the default
		/// </summary>
		public int? Duration { get; set; }

		/// <summary>
		///     Gets or sets the name of the command to execute
		/// </summary>
		public string IRCommandName { get; set; }

		/// <summary>
		///     Gets or sets the order of when power actions should be executed, starting at 0 or 1
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		///     Gets or sets the type of action to execute, e.g. IRPressAction
		/// </summary>
		[JsonProperty("__type")]
		public string Type { get; set; }
	}
}
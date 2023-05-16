﻿using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class MediaState
    {
        [JsonPropertyName("remote_control_enabled"), State]
        public bool IsRemoteControlEnabled { get; set; }
    }
}
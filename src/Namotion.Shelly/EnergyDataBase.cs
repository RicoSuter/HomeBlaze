﻿using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class EnergyDataBase
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public int Identifier { get; init; }

        /// <summary>
        /// Gets the total active energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act")]
        public decimal TotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act_ret")]
        public double TotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_energy")]
        public double PhaseATotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_ret_energy")]
        public double PhaseATotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_energy")]
        public double PhaseBTotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_ret_energy")]
        public double PhaseBTotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_energy")]
        public double PhaseCTotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_ret_energy")]
        public double PhaseCTotalActiveReturnedEnergy { get; init; }
    }
}

using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public partial class ShellyEnergyData
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public partial int Id { get; internal set; }

        /// <summary>
        /// Gets the total active energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act")]
        public partial decimal TotalActiveEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active returned energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act_ret")]
        public partial double TotalActiveReturnedEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_energy")]
        public partial double PhaseATotalActiveEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active returned energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_ret_energy")]
        public partial double PhaseATotalActiveReturnedEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_energy")]
        public partial double PhaseBTotalActiveEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active returned energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_ret_energy")]
        public partial double PhaseBTotalActiveReturnedEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_energy")]
        public partial double PhaseCTotalActiveEnergy { get; internal set; }

        /// <summary>
        /// Gets the total active returned energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_ret_energy")]
        public partial double PhaseCTotalActiveReturnedEnergy { get; internal set; }
    }
}

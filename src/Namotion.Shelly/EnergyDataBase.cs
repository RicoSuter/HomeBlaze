using HomeBlaze.Abstractions.Attributes;
using Namotion.Proxy;
using System.Text.Json.Serialization;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class EnergyDataBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public int Identifier { get; set; }

        /// <summary>
        /// Gets or sets the total active energy across all phases, measured in watts.
        /// </summary>
        [JsonPropertyName("total_act"), State(Unit = StateUnit.WattHour)]
        public decimal TotalActiveEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active returned energy across all phases, measured in watts.
        /// </summary>
        [JsonPropertyName("total_act_ret"), State(Unit = StateUnit.WattHour)]
        public double TotalActiveReturnedEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active energy for phase A, measured in watts.
        /// </summary>
        [JsonPropertyName("a_total_act_energy")]
        public double PhaseATotalActiveEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active returned energy for phase A, measured in watts.
        /// </summary>
        [JsonPropertyName("a_total_act_ret_energy")]
        public double PhaseATotalActiveReturnedEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active energy for phase B, measured in watts.
        /// </summary>
        [JsonPropertyName("b_total_act_energy")]
        public double PhaseBTotalActiveEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active returned energy for phase B, measured in watts.
        /// </summary>
        [JsonPropertyName("b_total_act_ret_energy")]
        public double PhaseBTotalActiveReturnedEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active energy for phase C, measured in watts.
        /// </summary>
        [JsonPropertyName("c_total_act_energy")]
        public double PhaseCTotalActiveEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active returned energy for phase C, measured in watts.
        /// </summary>
        [JsonPropertyName("c_total_act_ret_energy")]
        public double PhaseCTotalActiveReturnedEnergy { get; set; }
    }
}

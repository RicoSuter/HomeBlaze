using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class ShellyEnergyDataBase
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public virtual int Id { get; init; }

        /// <summary>
        /// Gets the total active energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act")]
        public virtual decimal TotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy across all phases, measured in watt-hours.
        /// </summary>
        [State(Unit = StateUnit.WattHour)]
        [JsonPropertyName("total_act_ret")]
        public virtual double TotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_energy")]
        public virtual double PhaseATotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase A, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("a_total_act_ret_energy")]
        public virtual double PhaseATotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_energy")]
        public virtual double PhaseBTotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase B, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("b_total_act_ret_energy")]
        public virtual double PhaseBTotalActiveReturnedEnergy { get; init; }

        /// <summary>
        /// Gets the total active energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_energy")]
        public virtual double PhaseCTotalActiveEnergy { get; init; }

        /// <summary>
        /// Gets the total active returned energy for phase C, measured in watt-hours.
        /// </summary>
        [JsonPropertyName("c_total_act_ret_energy")]
        public virtual double PhaseCTotalActiveReturnedEnergy { get; init; }
    }
}

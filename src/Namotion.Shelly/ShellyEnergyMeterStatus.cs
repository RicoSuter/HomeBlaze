using System.Text.Json.Serialization;
using HomeBlaze.Abstractions.Attributes;
using Namotion.Interceptor.Attributes;

namespace Namotion.Shelly;

[InterceptorSubject]
public partial class ShellyEnergyMeterStatus
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public partial int Id { get; init; }

    /// <summary>
    /// Gets the total active energy across all phases, measured in watt-hours.
    /// </summary>
    [State(Unit = StateUnit.WattHour)]
    [JsonPropertyName("total_act")]
    public partial decimal TotalActiveEnergy { get; init; }

    /// <summary>
    /// Gets the total active returned energy across all phases, measured in watt-hours.
    /// </summary>
    [State(Unit = StateUnit.WattHour)]
    [JsonPropertyName("total_act_ret")]
    public partial double TotalActiveReturnedEnergy { get; init; }

    /// <summary>
    /// Gets the total active energy for phase A, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("a_total_act_energy")]
    public partial double PhaseATotalActiveEnergy { get; init; }

    /// <summary>
    /// Gets the total active returned energy for phase A, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("a_total_act_ret_energy")]
    public partial double PhaseATotalActiveReturnedEnergy { get; init; }

    /// <summary>
    /// Gets the total active energy for phase B, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("b_total_act_energy")]
    public partial double PhaseBTotalActiveEnergy { get; init; }

    /// <summary>
    /// Gets the total active returned energy for phase B, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("b_total_act_ret_energy")]
    public partial double PhaseBTotalActiveReturnedEnergy { get; init; }

    /// <summary>
    /// Gets the total active energy for phase C, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("c_total_act_energy")]
    public partial double PhaseCTotalActiveEnergy { get; init; }

    /// <summary>
    /// Gets the total active returned energy for phase C, measured in watt-hours.
    /// </summary>
    [JsonPropertyName("c_total_act_ret_energy")]
    public partial double PhaseCTotalActiveReturnedEnergy { get; init; }
}
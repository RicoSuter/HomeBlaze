using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions;
using Namotion.Proxy;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System;

namespace Namotion.Shelly
{
    public class EnergyData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public int Identifier { get; set; }

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

        /// <summary>
        /// Gets or sets the total active energy across all phases, measured in watts.
        /// </summary>
        [JsonPropertyName("total_act")]
        public double TotalActiveEnergy { get; set; }

        /// <summary>
        /// Gets or sets the total active returned energy across all phases, measured in watts.
        /// </summary>
        [JsonPropertyName("total_act_ret")]
        public double TotalActiveReturnedEnergy { get; set; }
    }

    [GenerateProxy]
    public class ShellyEnergyMeterBase :
        IThing,
        IIconProvider,
        IPowerConsumptionSensor
    {
        string IThing.Id => Parent!.Id + "/energy-meter";

        string? IThing.Title => "EnergyMeter";

        string IIconProvider.IconName => "fas fa-bars";

        [ParentThing]
        public ShellyDevice? Parent { get; protected set; }

        public virtual decimal? PowerConsumption => Convert.ToDecimal(TotalActivePower);

        /// <summary>
        /// Gets or sets the ID of the EM1 component.
        /// </summary>
        [JsonPropertyName("id"), State]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the current of phase A in amperes.
        /// </summary>
        [JsonPropertyName("a_current"), State()]
        public double PhaseACurrent { get; set; }

        /// <summary>
        /// Gets or sets the voltage of phase A in volts.
        /// </summary>
        [JsonPropertyName("a_voltage"), State()]
        public double PhaseAVoltage { get; set; }

        /// <summary>
        /// Gets or sets the active power of phase A in watts.
        /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
        /// </summary>
        [JsonPropertyName("a_act_power"), State()]
        public double PhaseAActivePower { get; set; }

        /// <summary>
        /// Gets or sets the apparent power of phase A in volt-amperes.
        /// Apparent power is the combination of active power (real power) and reactive power.
        /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
        /// </summary>
        [JsonPropertyName("a_aprt_power"), State()]
        public double PhaseAApparentPower { get; set; }

        /// <summary>
        /// Gets or sets the power factor of phase A.
        /// </summary>
        [JsonPropertyName("a_pf"), State()]
        public double PhaseAPowerFactor { get; set; }

        /// <summary>
        /// Gets or sets the frequency of phase A in hertz.
        /// </summary>
        [JsonPropertyName("a_freq"), State()]
        public double PhaseAFrequency { get; set; }

        /// <summary>
        /// Gets or sets the current of phase B in amperes.
        /// </summary>
        [JsonPropertyName("b_current"), State()]
        public double PhaseBCurrent { get; set; }

        /// <summary>
        /// Gets or sets the voltage of phase B in volts.
        /// </summary>
        [JsonPropertyName("b_voltage"), State()]
        public double PhaseBVoltage { get; set; }

        /// <summary>
        /// Gets or sets the active power of phase B in watts.
        /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
        /// </summary>
        [JsonPropertyName("b_act_power"), State()]
        public double PhaseBActivePower { get; set; }

        /// <summary>
        /// Gets or sets the apparent power of phase B in volt-amperes.
        /// Apparent power is the combination of active power (real power) and reactive power.
        /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
        /// </summary>
        [JsonPropertyName("b_aprt_power"), State()]
        public double PhaseBApparentPower { get; set; }

        /// <summary>
        /// Gets or sets the power factor of phase B.
        /// </summary>
        [JsonPropertyName("b_pf"), State()]
        public double PhaseBPowerFactor { get; set; }

        /// <summary>
        /// Gets or sets the frequency of phase B in hertz.
        /// </summary>
        [JsonPropertyName("b_freq"), State()]
        public double PhaseBFrequency { get; set; }

        /// <summary>
        /// Gets or sets the current of phase C in amperes.
        /// </summary>
        [JsonPropertyName("c_current"), State()]
        public double PhaseCCurrent { get; set; }

        /// <summary>
        /// Gets or sets the voltage of phase C in volts.
        /// </summary>
        [JsonPropertyName("c_voltage"), State()]
        public double PhaseCVoltage { get; set; }

        /// <summary>
        /// Gets or sets the active power of phase C in watts.
        /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
        /// </summary>
        [JsonPropertyName("c_act_power"), State()]
        public double PhaseCActivePower { get; set; }

        /// <summary>
        /// Gets or sets the apparent power of phase C in volt-amperes.
        /// Apparent power is the combination of active power (real power) and reactive power.
        /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
        /// </summary>
        [JsonPropertyName("c_aprt_power"), State()]
        public double PhaseCApparentPower { get; set; }

        /// <summary>
        /// Gets or sets the power factor of phase C.
        /// </summary>
        [JsonPropertyName("c_pf"), State()]
        public double PhaseCPowerFactor { get; set; }

        /// <summary>
        /// Gets or sets the frequency of phase C in hertz.
        /// </summary>
        [JsonPropertyName("c_freq"), State()]
        public double PhaseCFrequency { get; set; }

        /// <summary>
        /// Gets or sets the current of the neutral line in amperes.
        /// </summary>
        [JsonPropertyName("n_current"), State()]
        public double? NeutralCurrent { get; set; }

        /// <summary>
        /// Gets or sets the total current in amperes.
        /// </summary>
        [JsonPropertyName("total_current"), State()]
        public double TotalCurrent { get; set; }

        /// <summary>
        /// Gets or sets the total active power in watts.
        /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
        /// </summary>
        [JsonPropertyName("total_act_power"), State()]
        public double TotalActivePower { get; set; }

        /// <summary>
        /// Gets or sets the total apparent power in volt-amperes.
        /// Apparent power is the combination of active power (real power) and reactive power.
        /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
        /// </summary>
        [JsonPropertyName("total_aprt_power"), State()]
        public double TotalApparentPower { get; set; }

        [JsonExtensionData]
        public virtual Dictionary<string, object>? ExtensionData { get; set; }

        //[JsonPropertyName("user_calibrated_phase")]
        //public List<object> UserCalibratedPhase { get; set; }
    }
}

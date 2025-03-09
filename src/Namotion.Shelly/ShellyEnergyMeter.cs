using System.Collections.Generic;
using System.Text.Json.Serialization;
using System;

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions;

using Namotion.Interceptor.Attributes;

namespace Namotion.Shelly;

[InterceptorSubject]
public partial class ShellyEnergyMeter :
    IThing,
    IIconProvider,
    IPowerConsumptionSensor,
    ILastUpdatedProvider
{
    string IThing.Id => Parent!.Id + "/energy-meter";

    string? IThing.Title => "Energy Meter";

    string IIconProvider.IconName => "fas fa-bolt";

    [ParentThing]
    internal ShellyDevice? Parent { get; set; }

    public DateTimeOffset? LastUpdated => Parent?.LastUpdated;

    [Derived]
    public decimal? PowerConsumption => TotalActivePower;

    [ScanForState]
    public partial ShellyEnergyMeterStatus? Status { get; internal set; }

    [State]
    public partial ShellyEnergyMeterPhase PhaseA { get; private set; }

    [State]
    public partial ShellyEnergyMeterPhase PhaseB { get; private set; }

    [State]
    public partial ShellyEnergyMeterPhase PhaseC { get; private set; }

    [State(Unit = StateUnit.WattHour, IsCumulative = true)]
    public decimal? TotalConsumedEnergy { get; private set; }

    /// <summary>
    /// Gets or sets the ID of the EM1 component.
    /// </summary>
    [JsonPropertyName("id"), JsonInclude]
    public partial int Id { get; internal set; }

    /// <summary>
    /// Gets or sets the total current in amperes.
    /// </summary>
    [State(Unit = StateUnit.Ampere)]
    [JsonPropertyName("total_current"), JsonInclude]
    public partial double TotalCurrent { get; internal set; }

    /// <summary>
    /// Gets or sets the total active power in watts.
    /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
    /// </summary>
    [JsonPropertyName("total_act_power"), JsonInclude]
    public partial decimal TotalActivePower { get; internal set; }

    /// <summary>
    /// Gets or sets the total apparent power in volt-amperes.
    /// Apparent power is the combination of active power (real power) and reactive power.
    /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
    /// </summary>
    [State(Unit = StateUnit.WattHour)]
    [JsonPropertyName("total_aprt_power"), JsonInclude]
    public partial double TotalApparentPower { get; internal set; }

    /// <summary>
    /// Gets or sets the current of the neutral line in amperes.
    /// </summary>
    [State(Unit = StateUnit.Ampere)]
    [JsonPropertyName("n_current"), JsonInclude]
    public partial double? NeutralCurrent { get; internal set; }

    public ShellyEnergyMeter()
    {
        PhaseA = new ShellyEnergyMeterPhase("a");
        PhaseB = new ShellyEnergyMeterPhase("b");
        PhaseC = new ShellyEnergyMeterPhase("c");
    }

    internal void Update()
    {
        TotalConsumedEnergy = Status?.TotalActiveEnergy;
        
        PhaseA.Current = PhaseACurrent;
        PhaseA.Voltage = PhaseAVoltage;
        PhaseA.ActivePower = PhaseAActivePower;
        PhaseA.ApparentPower = PhaseAApparentPower;
        PhaseA.PowerFactor = PhaseAPowerFactor;
        PhaseA.Frequency = PhaseAFrequency;

        PhaseA.TotalActiveEnergy = Status?.PhaseATotalActiveEnergy;
        PhaseA.TotalActiveReturnedEnergy = Status?.PhaseATotalActiveReturnedEnergy;

        PhaseB.Current = PhaseBCurrent;
        PhaseB.Voltage = PhaseBVoltage;
        PhaseB.ActivePower = PhaseBActivePower;
        PhaseB.ApparentPower = PhaseBApparentPower;
        PhaseB.PowerFactor = PhaseBPowerFactor;
        PhaseB.Frequency = PhaseBFrequency;

        PhaseB.TotalActiveEnergy = Status?.PhaseBTotalActiveEnergy;
        PhaseB.TotalActiveReturnedEnergy = Status?.PhaseBTotalActiveReturnedEnergy;

        PhaseC.Current = PhaseCCurrent;
        PhaseC.Voltage = PhaseCVoltage;
        PhaseC.ActivePower = PhaseCActivePower;
        PhaseC.ApparentPower = PhaseCApparentPower;
        PhaseC.PowerFactor = PhaseCPowerFactor;
        PhaseC.Frequency = PhaseCFrequency;

        PhaseC.TotalActiveEnergy = Status?.PhaseCTotalActiveEnergy;
        PhaseC.TotalActiveReturnedEnergy = Status?.PhaseCTotalActiveReturnedEnergy;
    }

    /// <summary>
    /// Gets or sets the current of phase A in amperes.
    /// </summary>
    [JsonPropertyName("a_current"), JsonInclude]
    public double PhaseACurrent { get; internal set; }

    /// <summary>
    /// Gets or sets the voltage of phase A in volts.
    /// </summary>
    [JsonPropertyName("a_voltage"), JsonInclude]
    public double PhaseAVoltage { get; internal set; }

    /// <summary>
    /// Gets or sets the active power of phase A in watts.
    /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
    /// </summary>
    [JsonPropertyName("a_act_power"), JsonInclude]
    public double PhaseAActivePower { get; internal set; }

    /// <summary>
    /// Gets or sets the apparent power of phase A in volt-amperes.
    /// Apparent power is the combination of active power (real power) and reactive power.
    /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
    /// </summary>
    [JsonPropertyName("a_aprt_power"), JsonInclude]
    public double PhaseAApparentPower { get; internal set; }

    /// <summary>
    /// Gets or sets the power factor of phase A.
    /// </summary>
    [JsonPropertyName("a_pf"), JsonInclude]
    public double PhaseAPowerFactor { get; internal set; }

    /// <summary>
    /// Gets or sets the frequency of phase A in hertz.
    /// </summary>
    [JsonPropertyName("a_freq"), JsonInclude]
    public double PhaseAFrequency { get; internal set; }

    /// <summary>
    /// Gets or sets the current of phase B in amperes.
    /// </summary>
    [JsonPropertyName("b_current"), JsonInclude]
    public double PhaseBCurrent { get; internal set; }

    /// <summary>
    /// Gets or sets the voltage of phase B in volts.
    /// </summary>
    [JsonPropertyName("b_voltage"), JsonInclude]
    public double PhaseBVoltage { get; internal set; }

    /// <summary>
    /// Gets or sets the active power of phase B in watts.
    /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
    /// </summary>
    [JsonPropertyName("b_act_power"), JsonInclude]
    public double PhaseBActivePower { get; internal set; }

    /// <summary>
    /// Gets or sets the apparent power of phase B in volt-amperes.
    /// Apparent power is the combination of active power (real power) and reactive power.
    /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
    /// </summary>
    [JsonPropertyName("b_aprt_power"), JsonInclude]
    public double PhaseBApparentPower { get; internal set; }

    /// <summary>
    /// Gets or sets the power factor of phase B.
    /// </summary>
    [JsonPropertyName("b_pf"), JsonInclude]
    public double PhaseBPowerFactor { get; internal set; }

    /// <summary>
    /// Gets or sets the frequency of phase B in hertz.
    /// </summary>
    [JsonPropertyName("b_freq"), JsonInclude]
    public double PhaseBFrequency { get; internal set; }

    /// <summary>
    /// Gets or sets the current of phase C in amperes.
    /// </summary>
    [JsonPropertyName("c_current"), JsonInclude]
    public double PhaseCCurrent { get; internal set; }

    /// <summary>
    /// Gets or sets the voltage of phase C in volts.
    /// </summary>
    [JsonPropertyName("c_voltage"), JsonInclude]
    public double PhaseCVoltage { get; internal set; }

    /// <summary>
    /// Gets or sets the active power of phase C in watts.
    /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
    /// </summary>
    [JsonPropertyName("c_act_power"), JsonInclude]
    public double PhaseCActivePower { get; internal set; }

    /// <summary>
    /// Gets or sets the apparent power of phase C in volt-amperes.
    /// Apparent power is the combination of active power (real power) and reactive power.
    /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
    /// </summary>
    [JsonPropertyName("c_aprt_power"), JsonInclude]
    public double PhaseCApparentPower { get; internal set; }

    /// <summary>
    /// Gets or sets the power factor of phase C.
    /// </summary>
    [JsonPropertyName("c_pf"), JsonInclude]
    public double PhaseCPowerFactor { get; internal set; }

    /// <summary>
    /// Gets or sets the frequency of phase C in hertz.
    /// </summary>
    [JsonPropertyName("c_freq"), JsonInclude]
    public double PhaseCFrequency { get; internal set; }
}
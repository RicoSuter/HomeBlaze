﻿using System;
using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class ShellyEnergyMeterPhaseBase :
        IThing,
        IIconProvider,
        ILastUpdatedProvider
    {
        private readonly string _phase;

        string IThing.Id => Parent!.Parent!.Id + "/energy-meter/phase-" + _phase;

        string? IThing.Title => "Phase " + _phase.ToUpperInvariant();

        string IIconProvider.IconName => "fas fa-bolt";

        [ParentThing]
        internal ShellyEnergyMeter? Parent { get; set; }

        public DateTimeOffset? LastUpdated => Parent?.LastUpdated;

        public ShellyEnergyMeterPhaseBase(string phase)
        {
            _phase = phase;
        }

        /// <summary>
        /// Gets or sets the current of phase A in amperes.
        /// </summary>
        [State(Unit = StateUnit.Ampere), JsonInclude]
        public virtual double Current { get; internal set; }

        /// <summary>
        /// Gets or sets the voltage of phase A in volts.
        /// </summary>
        [State(Unit = StateUnit.Volt), JsonInclude]
        public virtual double Voltage { get; internal set; }

        /// <summary>
        /// Gets or sets the active power of phase A in watts.
        /// Active power (real power) is the actual power consumed by electrical equipment to perform useful work, such as running a motor or lighting a bulb.
        /// </summary>
        [State(Unit = StateUnit.Watt), JsonInclude]
        public virtual double ActivePower { get; internal set; }

        /// <summary>
        /// Gets or sets the apparent power of phase A in volt-amperes.
        /// Apparent power is the combination of active power (real power) and reactive power.
        /// It represents the total power used by the electrical equipment to do work and sustain the magnetic and electric fields.
        /// </summary>
        [State(Unit = StateUnit.Watt), JsonInclude]
        public virtual double ApparentPower { get; internal set; }

        /// <summary>
        /// Gets or sets the power factor of phase A.
        /// </summary>
        [State, JsonInclude]
        public virtual double PowerFactor { get; internal set; }

        /// <summary>
        /// Gets or sets the frequency of phase A in hertz.
        /// </summary>
        [State(Unit = StateUnit.Hertz), JsonInclude]
        public virtual double Frequency { get; internal set; }

        /// <summary>
        /// Gets or sets the total active energy for phase A, measured in watts.
        /// </summary>
        [State(Unit = StateUnit.WattHour), JsonInclude]
        public double? TotalActiveEnergy { get; internal set; }

        /// <summary>
        /// Gets or sets the total active returned energy for phase A, measured in watts.
        /// </summary>
        [State(Unit = StateUnit.WattHour), JsonInclude]
        public double? TotalActiveReturnedEnergy { get; internal set; }
    }
}

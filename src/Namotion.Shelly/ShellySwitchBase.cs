﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class ShellySwitchBase :
        IThing,
        IIconProvider,
        ISwitchDevice,
        ILastUpdatedProvider,
        IObservable<SwitchEvent>,
        IDisposable
    {
        private readonly Subject<SwitchEvent> _switchEventSubject = new();

        string IThing.Id => Parent!.Id + "/switch" + Index;

        string? IThing.Title => "Switch " + Index;

        string IIconProvider.IconName => "fas fa-bars";

        [ParentThing]
        internal ShellyDevice? Parent { get; set; }

        public DateTimeOffset? LastUpdated => Parent?.LastUpdated;

        [JsonPropertyName("id"), State]
        public int? Index { get; set; }

        [JsonPropertyName("output"), State]
        public bool? IsOn { get; set; }

        [JsonPropertyName("source"), State]
        public string? Source { get; set; }

        public async Task TurnOnAsync(CancellationToken cancellationToken = default)
        {
            await Parent!.CallHttpGetAsync("relay/" + Index + "?turn=on", cancellationToken);
        }

        public async Task TurnOffAsync(CancellationToken cancellationToken = default)
        {
            await Parent!.CallHttpGetAsync("relay/" + Index + "?turn=off", cancellationToken);
        }

        internal void IsOnChanged(SwitchEvent switchEvent)
        {
            _switchEventSubject.OnNext(switchEvent);
        }

        public IDisposable Subscribe(IObserver<SwitchEvent> observer)
        {
            return _switchEventSubject.Subscribe(observer);
        }

        public void Dispose()
        {
            _switchEventSubject.Dispose();
        }
    }
}

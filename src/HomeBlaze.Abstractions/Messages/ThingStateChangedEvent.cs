﻿using HomeBlaze.Abstractions;
using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record ThingStateChangedEvent : IEvent
    {
        public DateTimeOffset ChangeDate { get; init; }

        public string? PropertyName { get; init; }

        public object? OldValue { get; init; }

        public object? NewValue { get; init; }

        public object Source { get; }

        public ThingStateChangedEvent(IThing thing)
        {
            Source = thing;
        }
    }
}

﻿using HomeBlaze.Abstractions.Attributes;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveNotificationComponent : ZwaveClassComponent
    {
        protected override string Class => "Notification";

        [State]
        public int Level { get; set; }

        [State]
        public int Status { get; set; }

        [State]
        public NotificationState Event { get; internal set; }

        [State]
        public NotificationType Type { get; internal set; }

        [State]
        public byte EndPointId { get; }

        public ZwaveNotificationComponent(ZwaveDevice parent, byte endPointId)
            : base(parent)
        {
            EndPointId = endPointId;
        }
    }
}

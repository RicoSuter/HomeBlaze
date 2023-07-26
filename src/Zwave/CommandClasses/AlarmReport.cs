using System;
using System.Collections.Generic;
using System.Linq;
using ZWave.Channel;
using ZWave.Channel.Protocol;

namespace ZWave.CommandClasses
{
    public class AlarmReport : NodeReport
    {
        public NotificationType V1Type { get; protected set; }
        public NotificationType Type { get; protected set; }
        public byte Level { get; protected set; }
        public byte Status { get; protected set; }
        public NotificationState Event { get; protected set; }
        public byte SourceNodeID { get; protected set; }
        public byte[] Params { get; protected set; }

        internal AlarmReport(Node node, byte[] payload) : base(node)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (payload.Length < 2)
                throw new ReponseFormatException($"The response was not in the expected format. Report: {GetType().Name}, Payload: {BitConverter.ToString(payload)}");

            //Version 1
            V1Type = (NotificationType)payload[0];
            Status = Level = payload[1];

            //Version 2
            if (payload.Length > 5)
            {
                SourceNodeID = payload[2];
                Status = payload[3];
                Type = (NotificationType)payload[4];
                Event = (NotificationState)((payload[4] << 8) | payload[5]);
                if (payload[5] == 0x0)
                    Event = NotificationState.Idle;
                else if (payload[5] == 0xFE)
                    Event = NotificationState.Unknown;
            }
            else
            {
                SourceNodeID = 0;
                Status = 0;
                Event = NotificationState.Unknown;
                Type = NotificationType.Unknown;
            }

            if (payload.Length > 6)
            {
                Params = new byte[payload[6]];
                Buffer.BlockCopy(payload, 7, Params, 0, Params.Length);
            }
            else
                Params = new byte[0];
        }

        public override string ToString()
        {
            return $"Type:{Type}, Level:{Level}, Event:{Event}, SourceID:{SourceNodeID}";
        }
    }
}

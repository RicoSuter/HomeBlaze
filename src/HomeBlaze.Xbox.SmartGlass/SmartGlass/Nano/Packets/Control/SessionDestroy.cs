using System;
using System.IO;
using SmartGlass.Common;
using SmartGlass.Nano;

namespace SmartGlass.Nano.Packets
{
    [ControlOpCode(ControlOpCode.SessionDestroy)]
    public class SessionDestroy : StreamerMessageWithHeader
    {
        public SessionDestroy()
            : base(ControlOpCode.SessionDestroy)
        {
        }

        internal override void DeserializeStreamer(EndianReader reader)
        {
            throw new NotImplementedException();
        }

        internal override void SerializeStreamer(EndianWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
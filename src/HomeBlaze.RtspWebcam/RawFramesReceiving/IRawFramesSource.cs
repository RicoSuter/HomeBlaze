using System;
using RtspClientSharp.RawFrames;

namespace RtspCapture.RawFramesReceiving
{
    interface IRawFramesSource
    {
        EventHandler<RawFrame>? FrameReceived { get; set; }
        EventHandler<string>? ConnectionStatusChanged { get; set; }
    }
}
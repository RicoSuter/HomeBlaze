using System;
using RtspClientSharp.RawFrames;

namespace HomeBlaze.RtspWebcam.RawFramesReceiving
{
    interface IRawFramesSource
    {
        EventHandler<RawFrame>? FrameReceived { get; set; }
        EventHandler<string>? ConnectionStatusChanged { get; set; }
    }
}
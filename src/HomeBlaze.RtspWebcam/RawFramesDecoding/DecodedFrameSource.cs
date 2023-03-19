using System;
using System.Collections.Generic;
using System.Drawing;
using RtspCapture.RawFramesDecoding;
using RtspCapture.RawFramesDecoding.DecodedFrames;
using RtspCapture.RawFramesDecoding.FFmpeg;
using RtspCapture.RawFramesReceiving;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;

namespace RtspCapture.processor
{
    class DecodedFrameSource : IDisposable
    {
        private IRawFramesSource? _rawFramesSource;
        private byte[] _decodedFrameBuffer = new byte[0];

        private PostVideoDecodingParameters _postVideoDecodingParameters = new PostVideoDecodingParameters(RectangleF.Empty,
            new Size(0, 0), ScalingPolicy.Stretch, PixelFormat.Bgr24, ScalingQuality.Nearest);

        private readonly Dictionary<FFmpegVideoCodecId, FFmpegVideoDecoder> _videoDecodersMap =
            new Dictionary<FFmpegVideoCodecId, FFmpegVideoDecoder>();

        public event EventHandler<IDecodedVideoFrame>? FrameReceived;


        public void SetRawFramesSource(IRawFramesSource rawFramesSource)
        {
            if (_rawFramesSource != null)
            {
                _rawFramesSource.FrameReceived -= OnFrameReceived;
                DropAllVideoDecoders();
            }

            _rawFramesSource = rawFramesSource;

            if (rawFramesSource == null)
                return;

            rawFramesSource.FrameReceived += OnFrameReceived;
        }

        public void Dispose()
        {
            DropAllVideoDecoders();
        }

        private void DropAllVideoDecoders()
        {
            foreach (FFmpegVideoDecoder decoder in _videoDecodersMap.Values)
                decoder.Dispose();

            _videoDecodersMap.Clear();
        }

        private void OnFrameReceived(object? sender, RawFrame rawFrame)
        {
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;

            FFmpegVideoDecoder decoder = GetDecoderForFrame(rawVideoFrame);

            if (!decoder.TryDecode(rawVideoFrame, out DecodedVideoFrameParameters? decodedFrameParameters))
                return;

            int targetWidth = decodedFrameParameters!.Width;
            int targetHeight = decodedFrameParameters.Height;

            int bufferSize = decodedFrameParameters.Height *
                             ImageUtils.GetStride(decodedFrameParameters.Width, PixelFormat.Bgr24);

            if (_decodedFrameBuffer.Length != bufferSize)
                _decodedFrameBuffer = new byte[bufferSize];

            var bufferSegment = new ArraySegment<byte>(_decodedFrameBuffer);

            if (_postVideoDecodingParameters.TargetFrameSize.Width != targetWidth ||
                _postVideoDecodingParameters.TargetFrameSize.Height != targetHeight)
            {
                _postVideoDecodingParameters = new PostVideoDecodingParameters(RectangleF.Empty,
                    new Size(targetWidth, targetHeight),
                    ScalingPolicy.Stretch, PixelFormat.Bgr24, ScalingQuality.Nearest);
            }

            IDecodedVideoFrame decodedFrame = decoder.GetDecodedFrame(bufferSegment, _postVideoDecodingParameters);

            FrameReceived?.Invoke(this, decodedFrame);
        }

        private FFmpegVideoDecoder GetDecoderForFrame(RawVideoFrame videoFrame)
        {
            FFmpegVideoCodecId codecId = DetectCodecId(videoFrame);
            if (!_videoDecodersMap.TryGetValue(codecId, out FFmpegVideoDecoder? decoder))
            {
                decoder = FFmpegVideoDecoder.CreateDecoder(codecId);
                _videoDecodersMap.Add(codecId, decoder);
            }

            return decoder;
        }

        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }

    }
}

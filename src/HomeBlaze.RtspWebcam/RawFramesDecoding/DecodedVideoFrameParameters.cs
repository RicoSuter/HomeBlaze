﻿using HomeBlaze.RtspWebcam.RawFramesDecoding.FFmpeg;

namespace HomeBlaze.RtspWebcam.RawFramesDecoding
{
    class DecodedVideoFrameParameters
    {
        public int Width { get; }

        public int Height { get; }

        public FFmpegPixelFormat PixelFormat { get; }

        public DecodedVideoFrameParameters(int width, int height, FFmpegPixelFormat pixelFormat)
        {
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
        }

        public bool AreValid => Width != 0 && Height != 0 && PixelFormat != FFmpegPixelFormat.None;

        protected bool Equals(DecodedVideoFrameParameters other)
        {
            return Width == other.Width && Height == other.Height && PixelFormat == other.PixelFormat;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DecodedVideoFrameParameters)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = hashCode * 397 ^ Height;
                hashCode = hashCode * 397 ^ (int)PixelFormat;
                return hashCode;
            }
        }
    }
}
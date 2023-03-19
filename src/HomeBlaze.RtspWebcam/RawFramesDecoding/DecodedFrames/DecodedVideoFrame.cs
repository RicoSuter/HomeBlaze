using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RtspCapture.RawFramesDecoding.DecodedFrames
{
    class DecodedVideoFrame : IDecodedVideoFrame
    {
        public DateTime Timestamp { get; }
        public ArraySegment<byte> DecodedBytes { get; }
        public int OriginalWidth { get; }
        public int OriginalHeight { get; }
        public int Width { get; }
        public int Height { get; }
        public PixelFormat Format { get; }
        public int Stride { get; }

        public DecodedVideoFrame(DateTime timestamp, ArraySegment<byte> decodedBytes, int originalWidth,
            int originalHeight, int width, int height, PixelFormat format, int stride)
        {
            Timestamp = timestamp;
            DecodedBytes = decodedBytes;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            Width = width;
            Height = height;
            Format = format;
            Stride = stride;
        }

#pragma warning disable CA1416 // Validate platform compatibility
        public Bitmap GetBitmap()
        {
            System.Drawing.Imaging.PixelFormat format;

            switch (Format)
            {
                case PixelFormat.Bgr24:
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    break;
                case PixelFormat.Abgr32:
                    format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported format");
            }

            var bitmap = new Bitmap(Width, Height, format);

            var boundsRect = new Rectangle(0, 0, Width, Height);

            BitmapData bmpData = bitmap.LockBits(boundsRect,
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(DecodedBytes.Array!, DecodedBytes.Offset, ptr, DecodedBytes.Count);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
    }
}
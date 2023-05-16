using System;
using System.Runtime.InteropServices;

namespace HomeBlaze.RtspWebcam.RawFramesDecoding.FFmpeg
{
    enum FFmpegVideoCodecId
    {
        MJPEG = 7,
        H264 = 27
    }

    [Flags]
    enum FFmpegScalingQuality
    {
        FastBilinear = 1,
        Bilinear = 2,
        Bicubic = 4,
        Point = 0x10,
        Area = 0x20,
    }

    enum FFmpegPixelFormat
    {
        None = -1,
        YUV420P = 0,
        YUYV422 = 1,
        RGB24 = 2,
        BGR24 = 3,
        YUV422P = 4,
        YUV444P = 5,
        YUV410P = 6,
        YUV411P = 7,
        GRAY8 = 8,
        ARGB = 27,
        RGBA = 28,
        ABGR = 29,
        BGRA = 30
    }

    static class FFmpegVideoPInvoke
    {
        public static int CreateVideoDecoder(FFmpegVideoCodecId videoCodecId, out IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FFmpegVideoPInvokeWin.CreateVideoDecoder(videoCodecId, out handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return FFmpegVideoPInvokeLinux.CreateVideoDecoder(videoCodecId, out handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void RemoveVideoDecoder(IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFmpegVideoPInvokeWin.RemoveVideoDecoder(handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                FFmpegVideoPInvokeLinux.RemoveVideoDecoder(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static int SetVideoDecoderExtraData(IntPtr handle, IntPtr extradata, int extradataLength)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FFmpegVideoPInvokeWin.SetVideoDecoderExtraData(handle, extradata, extradataLength);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return FFmpegVideoPInvokeLinux.SetVideoDecoderExtraData(handle, extradata, extradataLength);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static int DecodeFrame(IntPtr handle, IntPtr rawBuffer, int rawBufferLength, out int frameWidth,
            out int frameHeight, out FFmpegPixelFormat framePixelFormat)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FFmpegVideoPInvokeWin.DecodeFrame(handle, rawBuffer, rawBufferLength, out frameWidth, out frameHeight, out framePixelFormat);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return FFmpegVideoPInvokeLinux.DecodeFrame(handle, rawBuffer, rawBufferLength, out frameWidth, out frameHeight, out framePixelFormat);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static int ScaleDecodedVideoFrame(IntPtr handle, IntPtr scalerHandle, IntPtr scaledBuffer,
            int scaledBufferStride)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FFmpegVideoPInvokeWin.ScaleDecodedVideoFrame(handle, scalerHandle, scaledBuffer, scaledBufferStride);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return FFmpegVideoPInvokeLinux.ScaleDecodedVideoFrame(handle, scalerHandle, scaledBuffer, scaledBufferStride);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static int CreateVideoScaler(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
            FFmpegPixelFormat sourcePixelFormat,
            int scaledWidth, int scaledHeight, FFmpegPixelFormat scaledPixelFormat, FFmpegScalingQuality qualityFlags,
            out IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return FFmpegVideoPInvokeWin.CreateVideoScaler(sourceLeft, sourceTop, sourceWidth,
                    sourceHeight, sourcePixelFormat, scaledWidth, scaledHeight, scaledPixelFormat, qualityFlags, out handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return FFmpegVideoPInvokeLinux.CreateVideoScaler(sourceLeft, sourceTop, sourceWidth,
                    sourceHeight, sourcePixelFormat, scaledWidth, scaledHeight, scaledPixelFormat, qualityFlags, out handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void RemoveVideoScaler(IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFmpegVideoPInvokeWin.RemoveVideoScaler(handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                FFmpegVideoPInvokeLinux.RemoveVideoScaler(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
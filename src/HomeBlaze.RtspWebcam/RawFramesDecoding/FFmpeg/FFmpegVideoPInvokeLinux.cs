﻿using System;
using System.Runtime.InteropServices;

namespace HomeBlaze.RtspWebcam.RawFramesDecoding.FFmpeg
{
    static class FFmpegVideoPInvokeLinux
    {
        private const string LibraryName = "libffmpeghelper.so";

        [DllImport(LibraryName, EntryPoint = "create_video_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateVideoDecoder(FFmpegVideoCodecId videoCodecId, out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "remove_video_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveVideoDecoder(IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "set_video_decoder_extradata",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetVideoDecoderExtraData(IntPtr handle, IntPtr extradata, int extradataLength);

        [DllImport(LibraryName, EntryPoint = "decode_video_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int DecodeFrame(IntPtr handle, IntPtr rawBuffer, int rawBufferLength, out int frameWidth,
            out int frameHeight, out FFmpegPixelFormat framePixelFormat);

        [DllImport(LibraryName, EntryPoint = "scale_decoded_video_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScaleDecodedVideoFrame(IntPtr handle, IntPtr scalerHandle, IntPtr scaledBuffer,
            int scaledBufferStride);

        [DllImport(LibraryName, EntryPoint = "create_video_scaler", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateVideoScaler(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
            FFmpegPixelFormat sourcePixelFormat,
            int scaledWidth, int scaledHeight, FFmpegPixelFormat scaledPixelFormat, FFmpegScalingQuality qualityFlags,
            out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "remove_video_scaler", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveVideoScaler(IntPtr handle);
    }
}
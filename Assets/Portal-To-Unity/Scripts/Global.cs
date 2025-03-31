using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using libusbK;
using UnityEditor;
using UnityEngine;

namespace PortalToUnity
{
    public static class Global
    {
        public const int PORTAL_VENDOR_ID = 0x1430;
        public const int PORTAL_PRODUCT_ID = 0x0150;
        public const int PORTAL_PRODUCT_ID_XBOX360 = 0x1F17;
        public const int PORTAL_PRODUCT_ID_XBOXONE = 0x09AA;
        public const int REPORT_SIZE = 0x20;
        public const int FIGURE_INDICIES_COUNT = 0x10;

        public const int SALT_LENGTH = 0x35;
        public const int REGION_COUNT = 2;
        public const int BLOCK_SIZE = 0x10;
        public const int BLOCK_COUNT = 0x40;
        public const int TAG_HEADER_SIZE = BLOCK_SIZE * 2;
        public const int KEY_SIZE = TAG_HEADER_SIZE + 1 + SALT_LENGTH;
        public const int SECTOR_PERMISSION_0 = 0x690F0F0F;
        public const int SECTOR_PERMISSION_FULL = 0x69080F7F;
        public const int DATA_REGION0_OFFSET = 0x08;
        public const int DATA_REGION1_OFFSET = 0x24;

        // The audio configuration for the Traptanium Speaker, requires 8000hz mono audio
        public const int AUDIO_TARGET_SAMPLE_RATE = 8000;
        public const int AUDIO_TARGET_CHANNELS = 1;

        public static bool IsPortalOfPower(this KLST_DEVINFO_HANDLE info) => info.Common.Vid == PORTAL_VENDOR_ID && info.Common.Pid == PORTAL_PRODUCT_ID;
        public static string BytesToHexString(byte[] data) => string.Join(" ", data.Select(x => x.ToString("X2")));

        public static byte[] TrimTrailingZeros(byte[] data)
        {
            int lastIndex = Array.FindLastIndex(data, x => x != 0);

            if (lastIndex == -1)
                return new byte[0];

            byte[] trimmedArray = new byte[lastIndex + 1];
            Array.Copy(data, trimmedArray, lastIndex + 1);
            return trimmedArray;
        }

        public static byte[] AddZeroPadding(byte[] array, int amount)
        {
            if (amount == 0)
                return array;
            
            int length = array.Length;

            byte[] newArray = new byte[length + amount];
            Array.Copy(array, 0, newArray, amount, length);
            return newArray;
        }

        public static int BlockSizeOf<T>() where T : struct => Marshal.SizeOf<T>() / BLOCK_SIZE;

        public static bool IsAccessControlBlock(byte block) => (block % 4) == 3;

        public static string SpyroTagToHexString<T>(T spyroTag) where T : struct
        {
            int size = Marshal.SizeOf(spyroTag);
            byte[] bytes = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(spyroTag, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            StringBuilder hexString = new StringBuilder(size * 3);

            for (int i = 0; i < size; i++)
            {
                if (i > 0 && i % BLOCK_SIZE == 0)
                    hexString.AppendLine();

                hexString.Append($"{bytes[i]:X2} ");
            }
            return hexString.ToString();
        }

#if UNITY_EDITOR
        public static string GetSelectedPathOrFallback()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
                return "Assets/Portal-To-Unity/";

            if (!AssetDatabase.IsValidFolder(path))
                path = System.IO.Path.GetDirectoryName(path);

            return path;
        }
#endif

        // Basic audio resampling implementation. Is inferior to Unity's Import Settings and other converters, suggest pre-converting your audio, or improving this method in future
        public static AudioClip ResampleAudioClipForTraptanium(AudioClip clip)
        {
            if (clip.frequency == AUDIO_TARGET_SAMPLE_RATE && clip.channels == AUDIO_TARGET_CHANNELS)
                return clip;

            float[] audioData = new float[clip.samples * clip.channels];
            clip.GetData(audioData, 0);

            float[] convertedData = ResampleAudio(audioData, clip.frequency, clip.channels);
            convertedData = ConvertChannels(convertedData, clip.channels);

            int newSampleCount = Mathf.FloorToInt((float)convertedData.Length / AUDIO_TARGET_CHANNELS);
            AudioClip newClip = AudioClip.Create($"{clip.name} (Resampled)", newSampleCount, AUDIO_TARGET_CHANNELS, AUDIO_TARGET_SAMPLE_RATE, false);
            newClip.SetData(convertedData, 0);

            return newClip;

            static float[] ConvertChannels(float[] inputData, int inputChannels)
            {
                if (inputChannels == AUDIO_TARGET_CHANNELS)
                    return inputData;

                int sampleCount = inputData.Length / inputChannels;
                float[] outputData = new float[sampleCount * AUDIO_TARGET_CHANNELS];

                if (inputChannels == 2)
                {
                    for (int i = 0; i < sampleCount; i++)
                        outputData[i] = (inputData[i * 2] + inputData[i * 2 + 1]) / 2;
                }
                else
                    PTUManager.LogError("Unsupported channel conversion. AudioClip must be Mono or Stereo for conversion", LogPriority.High);

                return outputData;
            }

            static float[] ResampleAudio(float[] inputData, int inputSampleRate, int channels)
            {
                if (inputSampleRate == AUDIO_TARGET_SAMPLE_RATE)
                    return inputData;

                int inputSampleCount = inputData.Length / channels;
                int targetSampleCount = Mathf.FloorToInt(inputSampleCount * (AUDIO_TARGET_SAMPLE_RATE / (float)inputSampleRate));
                float[] outputData = new float[targetSampleCount * channels];

                for (int i = 0; i < targetSampleCount; i++)
                {
                    float inputIndex = i * (inputSampleRate / (float)AUDIO_TARGET_SAMPLE_RATE);
                    int index = Mathf.FloorToInt(inputIndex) * channels;

                    for (int channel = 0; channel < channels; channel++)
                    {
                        if (index < inputData.Length - channels)
                            outputData[i * channels + channel] = inputData[index + channel];
                    }
                }
                return outputData;
            }
        }
    }
}
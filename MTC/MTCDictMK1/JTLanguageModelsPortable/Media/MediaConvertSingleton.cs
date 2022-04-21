using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.MediaInterfaces;

namespace JTLanguageModelsPortable.Media
{
    public class MediaConvertSingleton
    {
        public static IMediaConvert Converter;
        public static string GetClientBinPath()
            { return Converter.GetClientBinPath(); }
        public static bool ConvertFile(string fromFilePath, string fromMimeType, string toFilePath, string toMimeType, out string message)
            { return Converter.ConvertFile(fromFilePath, fromMimeType, toFilePath, toMimeType, out message); }
        public static bool ConvertCheck(string filePath, string mimeType, out string message)
            { return Converter.ConvertCheck(filePath, mimeType, out message); }
        public static bool LazyConvert(string mainFilePath, string mainMimeType, bool force, out string message)
            { return Converter.LazyConvert(mainFilePath, mainMimeType, force, out message); }
        public static bool CopyCheck(string mainFilePath, string mainMimeType, string destPath, out string message)
            { return Converter.CopyCheck(mainFilePath, mainMimeType, destPath, out message); }
        public static bool CopyFile(string sourcePath, string destPath, out string message)
            { return Converter.CopyFile(sourcePath, destPath, out message); }
        public static bool ExecuteProgram(string command, string arguments, bool wait, out string message)
            { return Converter.ExecuteProgram(command, arguments, wait, out message); }
        public static bool ExecuteServiceProgram(string programFilePath, string programArguments, bool wait, out string programOutput, out string programErrorOutput, out string errorMessage)
            { return Converter.ExecuteServiceProgram(programFilePath, programArguments, wait, out programOutput, out programErrorOutput, out errorMessage); }
        public static void DeleteAlternates(string mainFilePath, string mainMimeType)
            { Converter.DeleteAlternates(mainFilePath, mainMimeType); }
        public static void RenameAlternates(string mainFilePathOld, string mainFilePathNew, string mainMimeType)
            { Converter.RenameAlternates(mainFilePathOld, mainFilePathNew, mainMimeType); }
        public static bool IsAlternate(string mainFilePath)
            { return Converter.IsAlternate(mainFilePath); }
        public static List<string> GetAlternates(string mainFilePath)
            { return Converter.GetAlternates(mainFilePath); }
        public static List<string> GetAllFileNames(string mainFilePath, string mainMimeType)
            { return Converter.GetAllFileNames(mainFilePath, mainMimeType); }
        public static WaveAudioBuffer GetWaveAudioBuffer(Stream audio, string mimeType, out string message)
            { return Converter.GetWaveAudioBuffer(audio, mimeType, out message); }
        public static WaveAudioBuffer Mp3Decoding(string mp3File, out string message)
            { return Converter.Mp3Decoding(mp3File, out message); }
        public static bool Mp3Encoding(string mp3File, WaveAudioBuffer waveBuffer, out string message)
            { return Converter.Mp3Encoding(mp3File, waveBuffer, out message); }
        public static WaveAudioBuffer SpeexDecoding(string speexFile, out string message)
            { return Converter.SpeexDecoding(speexFile, out message); }
        public static bool SpeexEncoding(string speexFile, WaveAudioBuffer waveBuffer, out string message)
            { return Converter.SpeexEncoding(speexFile, waveBuffer, out message); }
        public static bool RateChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData,
                int sampleRate, bool append)
            { return Converter.RateChange(inputWaveData, outputWaveData, sampleRate, append); }
        public static bool SpeedChange(string filePath, string mimeType, float speedMultiplier)
            { return Converter.SpeedChange(filePath, mimeType, speedMultiplier); }
        public static bool SpeedChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData, float speedMultiplier, bool append)
            { return Converter.SpeedChange(inputWaveData, outputWaveData, speedMultiplier, append); }
        public static bool NumberOfChannelsChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData, int newNumberOfChannels, bool append)
            { return Converter.NumberOfChannelsChange(inputWaveData, outputWaveData, newNumberOfChannels, append); }
        public static bool ConvertImageToThumbnail(string lcFilename, int lnWidth, int lnHeight)
            { return Converter.ConvertImageToThumbnail(lcFilename, lnWidth, lnHeight); }
        public static bool GetYouTubeVideo(string videoID, string baseFileName, string desiredQuality,
                string desiredMimeType,
                out string videoFile, out string mimeTypeVideo, out string message)
            { return Converter.GetYouTubeVideo(videoID, baseFileName, desiredQuality, desiredMimeType, out videoFile, out mimeTypeVideo, out message); }
    }
}

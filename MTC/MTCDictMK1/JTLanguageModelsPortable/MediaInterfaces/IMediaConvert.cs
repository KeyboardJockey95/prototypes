using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    public interface IMediaConvert
    {
        string GetClientBinPath();
        bool ConvertFile(string fromFilePath, string fromMimeType, string toFilePath, string toMimeType, out string message);
        bool ConvertCheck(string filePath, string mimeType, out string message);
        bool LazyConvert(string mainFilePath, string mainMimeType, bool force, out string message);
        bool CopyCheck(string mainFilePath, string mainMimeType, string destPath, out string message);
        bool CopyFile(string sourcePath, string destPath, out string message);
        bool ExecuteProgram(string command, string arguments, bool wait, out string message);
        bool ExecuteServiceProgram(string programFilePath, string programArguments, bool wait, out string programOutput, out string programErrorOutput, out string errorMessage);
        void DeleteAlternates(string mainFilePath, string mainMimeType);
        void RenameAlternates(string mainFilePathOld, string mainFilePathNew, string mainMimeType);
        bool IsAlternate(string filePath);
        List<string> GetAlternates(string filePath);
        List<string> GetAllFileNames(string mainFilePath, string mainMimeType);
        WaveAudioBuffer GetWaveAudioBuffer(Stream audio, string mimeType, out string message);
        WaveAudioBuffer Mp3Decoding(string mp3File, out string message);
        bool Mp3Encoding(string mp3File, WaveAudioBuffer waveBuffer, out string message);
        WaveAudioBuffer SpeexDecoding(string speedxFile, out string message);
        bool SpeexEncoding(string speedxFile, WaveAudioBuffer waveBuffer, out string message);
        bool RateChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData,
            int sampleRate, bool append);
        bool SpeedChange(string filePath, string mimeType, float speedMultiplier);
        bool SpeedChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData, float speedMultiplier, bool append);
        bool NumberOfChannelsChange(WaveAudioBuffer inputWaveData, WaveAudioBuffer outputWaveData, int newNumberOfChannels, bool append);
        bool ConvertImageToThumbnail(string lcFilename, int lnWidth, int lnHeight);
        bool GetYouTubeVideo(string videoID, string baseFileName, string desiredQuality, string desiredMimeType,
            out string videoFile, out string mimeTypeVideo, out string message);
    }
}

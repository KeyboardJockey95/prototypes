using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    public interface ISpeechToText
    {
        bool Initialize(out string errorMessage);
        bool IsSupported(LanguageID languageID);
        List<LanguageID> GetSupportedLanguages();
        bool ConvertToText(
            Stream audio,
            string mimeType,
            LanguageID languageID,
            string targetText,
            string userName,
            out String matchedText,
            out string errorMessage);
        bool MapText(
            Stream audio,
            string mimeType,
            TimeSpan segmentStart,
            TimeSpan segmentStop,
            List<LanguageID> languageIDs,
            List<string> hints,
            string userName,
            string cacheKey,        // If null, don't cache.
            out String matchedText,
            out List<TextRun> wordRuns,
            out string errorMessage);
        bool MapWaveText(
            WaveAudioBuffer waveBuffer,
            TimeSpan segmentStart,
            TimeSpan segmentStop,
            List<LanguageID> languageIDs,
            List<string> hints,
            string userName,
            string cacheKey,        // If null, don't cache.
            out String matchedText,
            out List<TextRun> wordRuns,
            out string errorMessage);
    }
}

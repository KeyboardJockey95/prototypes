using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Media
{
    public static class SpeechToTextSingleton
    {
        public static ISpeechToText SpeechEngine;

        public static void SetSpeechToText(ISpeechToText speechtoText)
        {
            SpeechEngine = speechtoText;
        }

        public static ISpeechToText GetSpeechToText()
        {
            return SpeechEngine;
        }

        public static bool Initialize(out string errorMessage)
        {
            return SpeechEngine.Initialize(out errorMessage);
        }

        public static bool IsSupported(LanguageID languageID)
        {
            return SpeechEngine.IsSupported(languageID);
        }

        public static List<LanguageID> GetSupportedLanguages()
        {
            return SpeechEngine.GetSupportedLanguages();
        }

        public static bool ConvertToText(
            Stream audio,
            string mimeType,
            LanguageID languageID,
            string targetText,
            string userName,
            out String matchedText,
            out string errorMessage)
        {
            return SpeechEngine.ConvertToText(
                audio,
                mimeType,
                languageID,
                targetText,
                userName,
                out matchedText,
                out errorMessage);
        }

        public static bool MapText(
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
            out string errorMessage)
        {
            return SpeechEngine.MapText(
                audio,
                mimeType,
                segmentStart,
                segmentStop,
                languageIDs,
                hints,
                userName,
                cacheKey,
                out matchedText,
                out wordRuns,
                out errorMessage);
        }

        public static bool MapWaveText(
            WaveAudioBuffer waveBuffer,
            TimeSpan segmentStart,
            TimeSpan segmentStop,
            List<LanguageID> languageIDs,
            List<string> hints,
            string userName,
            string cacheKey,        // If null, don't cache.
            out String matchedText,
            out List<TextRun> wordRuns,
            out string errorMessage)
        {
            return SpeechEngine.MapWaveText(
                waveBuffer,
                segmentStart,
                segmentStop,
                languageIDs,
                hints,
                userName,
                cacheKey,
                out matchedText,
                out wordRuns,
                out errorMessage);
        }
    }
}

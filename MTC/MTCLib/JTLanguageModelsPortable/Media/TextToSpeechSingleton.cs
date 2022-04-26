using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Media
{
    public static class TextToSpeechSingleton
    {
        public static ITextToSpeech SpeechEngine;
        public static void SetTextToSpeech(ITextToSpeech textToSpeech)
            { SpeechEngine = textToSpeech; }
        public static ITextToSpeech GetTextToSpeech()
            { return SpeechEngine.Clone(); }
        public static List<string> GetVoices(LanguageID languageID, UserProfile userProfile)
            { return SpeechEngine.GetVoices(languageID, userProfile); }
        public static bool SetVoice(string voiceName, LanguageID voiceLanguageID)
            { return SpeechEngine.SetVoice(voiceName, voiceLanguageID); }
        public static void SetSpeed(int speed)
            { SpeechEngine.SetSpeed(speed);  }
        public static bool SpeakAloud(string text, bool async, out string message)
            { return SpeechEngine.SpeakAloud(text, async, out message); }
        public static bool SpeakToFile(string text, string filePath, string mimeType, out string message)
            { return SpeechEngine.SpeakToFile(text, filePath, mimeType, out message); }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    public interface ITextToSpeech
    {
        ITextToSpeech Clone();
        bool SynchronizeVoiceNames(List<LanguageDescription> languageDescriptions, LanguageID uiLanguage);
        List<string> GetVoices(LanguageID languageID, UserProfile userProfile);
        bool SetVoice(string voiceName, LanguageID voiceLanguageID);
        void SetSpeed(int speed);  // -10 to 10
        bool SpeakAloud(string text, bool async, out string message);
        bool SpeakToFile(string text, string filePath, string mimeType, out string message);
        bool SpeakListToFiles(
            List<string> textList,
            List<string> fileList,
            string directoryPath,
            string mimeType,
            ref int itemIndex,
            out string message);
        void Reset();
    }
}

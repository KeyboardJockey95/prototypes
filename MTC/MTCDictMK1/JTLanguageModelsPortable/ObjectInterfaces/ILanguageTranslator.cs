using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.ObjectInterfaces
{
    public enum LanguageTranslatorSource
    {
        NotFound,
        Service,
        Dictionary,
        Conversion,
        Database,
        DatabaseCache,
        MemoryCache
    }

    public interface ILanguageTranslator
    {
        // operationPrefix: "Translate", "ContentTranslate", "UITranslation", "SynchronizeLanguagesTranslation"
        // databaseName: "UIStrings", "UIText", or "LanguageDescriptions"
        bool TranslateString(
            string operationPrefix,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out string outputString,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage);
        bool TranslateStringUser(
            string operationPrefix,
            UserRecord userRecord,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out string outputString,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage);
        bool TranslateStringWithAlternates(
            string operationPrefix,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out List<string> outputList,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage);
        bool TranslateStringUserWithAlternates(
            string operationPrefix,
            UserRecord userRecord,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out List<string> outputList,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage);
        void ConvertToPinyin(LanguageID inputLanguage, string inputString, bool toNumeric, out string outputString);
        void InputTransliterateCheck(LanguageID inputLanguage, string inputString, out string outputString);
        LanguageString GetBaseLanguageString(LanguageID destLanguageID, MultiLanguageString multiLanguageString,
            List<LanguageID> languageIDs);
        bool TranslateMultiLanguageString(MultiLanguageString sentence, List<LanguageID> languageIDs,
            out string errorMessage, bool forceIt);
        bool TranslateMultiLanguageItem(MultiLanguageItem multiLanguageItem, List<LanguageID> languageIDs,
            bool needsSentenceParsing, bool needsWordParsing, out string errorMessage, bool forceIt);
        bool TranslateSentenceRun(MultiLanguageItem multiLanguageItem, int sentenceIndex, List<LanguageID> languageIDs,
            bool needsWordParsing, out string errorMessage, bool forceIt);
        bool TranslateAnnotation(Annotation annotation, List<LanguageID> languageIDs,
            out string errorMessage, bool forceIt);
        bool TranslateDictionaryEntry(DictionaryEntry dictionaryEntry, List<LanguageID> hostLanguageIDs,
            out string errorMessage);
        bool AddTitledObjectTranslationsCheck(BaseObjectTitled item, List<LanguageID> languageIDs, bool isTranslate,
            out string message, out bool translated);
        bool AddStudyListTranslationsCheck(BaseObjectContent content, int editIndex, List<LanguageID> languageIDs,
            bool isTranslate, out string message, out bool translated);
        bool AddSpeakerNameTranslationsCheck(ContentStudyList studyList, string speakerKey, List<LanguageID> languageIDs,
            bool isTranslate, out string message, out bool translated);
        bool AddMarkupStringTranslationsCheck(MarkupTemplate markupTemplate, int editIndex,
            List<LanguageID> languageIDs, bool isTranslate, out string message, out bool translated);
        bool FixTranslation(
            string inputString,
            string fixedOutputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            UserRecord userRecord,
            out string previousOutputString,
            out string errorMessage);
        bool ClearCache(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            UserRecord userRecord,
            out string errorMessage);
        bool SynchronizeLanguageDescriptions(UserRecord userRecord, LanguageUtilities languageUtilities);
        void UpdateUserProfile(UserProfile userProfile);
        LanguageTool GetLanguageTool(LanguageID languageID);
        void SetLanguageTool(LanguageTool languageTool);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Object
{
    public abstract class LanguageTranslator : ILanguageTranslator
    {
        public string UserName;
        public UserRecord UserRecord;
        public UserProfile UserProfile;
        public bool UseAutomatedTranslation;
        public Boolean UseDictionary;
        public IMainRepository Repositories;
        public DictionaryRepository Dictionary;
        private Dictionary<string, LanguageTool> LanguageToolCache;

        public LanguageTranslator(UserRecord userRecord, UserProfile userProfile, IMainRepository repositories)
        {
            if (userRecord != null)
                UserName = userRecord.UserName;
            else
                UserName = null;

            UserRecord = userRecord;
            UserProfile = userProfile;
            Repositories = repositories;
            Dictionary = repositories.Dictionary;
            UpdateUserProfile(userProfile);
            LanguageToolCache = null;
        }

        protected abstract bool IsLanguageSupported(LanguageID languageID);

        protected abstract bool TranslateService(
            string operationPrefix,
            UserRecord userRecord,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            bool withAlternates,
            out string outputString,
            out List<string> outputList,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage);

        public virtual bool TranslateString(
            string operationPrefix,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out string outputString,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage)
        {
            bool returnValue = TranslateStringUser(
                operationPrefix,
                UserRecord,
                databaseName,
                stringID,
                inputString,
                inputLanguage,
                outputLanguage,
                out outputString,
                out translatorSource,
                out errorMessage);
            return returnValue;
        }

        public virtual bool TranslateStringUser(
            string operationPrefix,
            UserRecord userRecord,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out string outputString,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage)
        {
            string userName = (userRecord != null ? userRecord.UserName : String.Empty);
            bool gotIt = false;
            bool returnValue = true;
            List<string> translationsWithAlternates = null;

            outputString = "";
            translatorSource = LanguageTranslatorSource.NotFound;
            errorMessage = "";

            if (ApplicationData.Global.IsCanceled(userName, "AddNodeTranslations"))
                throw new OperationCanceledException(ApplicationData.Global.GetCanceledMessage(userName, "AddNodeTranslations"));

            if ((inputLanguage == null) || !inputLanguage.IsLanguage())
            {
                errorMessage = "Need an input language.";
                return false;
            }

            if ((outputLanguage == null) || !outputLanguage.IsLanguage())
            {
                errorMessage = "Need an output language.";
                return false;
            }

            switch (outputLanguage.ExtensionCode)
            {
                case "pn":
                    if (inputLanguage.LanguageCode != "zh")
                    {
                        LanguageID chineseID = LanguageLookup.ChineseSimplified;
                        string chineseText = null;
                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, chineseID, out chineseText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Dictionary;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                        {
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                chineseID,
                                false,
                                out chineseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);
                        }

                        if (gotIt)
                            ConvertToPinyin(chineseID, chineseText, false, out outputString);
                        else
                            returnValue = false;
                    }
                    else
                    {
                        ConvertToPinyin(inputLanguage, inputString, false, out outputString);
                        translatorSource = LanguageTranslatorSource.Conversion;
                        returnValue = true;
                    }
                    return returnValue;
                case "rm":
                    if (inputLanguage.LanguageCode == "ko")
                    {
                        if (inputString.Contains("\'"))
                        {
                            ConvertHangul converter = new ConvertHangul('\'', Dictionary, true);
                            if (converter.To(out outputString, inputString))
                            {
                                translatorSource = LanguageTranslatorSource.Conversion;
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                        {
                            ConvertHangul converter = new ConvertHangul('\0', Dictionary, true);
                            if (converter.To(out outputString, inputString))
                            {
                                translatorSource = LanguageTranslatorSource.Conversion;
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                    else if (outputLanguage.LanguageCode == inputLanguage.LanguageCode)
                    {
                        ConvertTransliterate converter = new ConvertTransliterate(
                            true,
                            outputLanguage,
                            inputLanguage,
                            '\0',
                            Dictionary,
                            true);
                        if (converter.To(out outputString, inputString))
                        {
                            translatorSource = LanguageTranslatorSource.Conversion;
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        LanguageID sourceID = LanguageLookup.GetLanguageIDNoAdd(outputLanguage.LanguageCode);
                        string sourceText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, sourceID, out sourceText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Dictionary;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                        {
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                sourceID,
                                false,
                                out sourceText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);
                        }

                        if (gotIt)
                        {
                            ConvertTransliterate converter = new ConvertTransliterate(
                                true,
                                outputLanguage,
                                inputLanguage,
                                '\0',
                                Dictionary,
                                true);
                            if (converter.To(out outputString, sourceText))
                                return true;
                            else
                                return false;
                        }
                        else
                            returnValue = false;
                    }
                    break;
                case "rj":
                    if (inputLanguage.LanguageCode == "ja")
                    {
                        ConvertRomaji converter = new ConvertRomaji(inputLanguage, '\0', Dictionary, true);
                        if (converter.To(out outputString, inputString))
                        {
                            translatorSource = LanguageTranslatorSource.Conversion;
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        LanguageID japaneseID = LanguageLookup.Japanese;
                        string japaneseText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, japaneseID, out japaneseText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Dictionary;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                japaneseID,
                                false,
                                out japaneseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            ConvertRomaji converter = new ConvertRomaji(japaneseID, '\0', Dictionary, true);
                            if (converter.To(out outputString, japaneseText))
                                return true;
                            else
                                return false;
                        }
                        else
                            returnValue = false;
                    }
                    break;
                case "kn":
                    if (inputLanguage.LanguageCultureExtensionCode == "ja")
                    {
                        ConvertTransliterate converter = new ConvertTransliterate(inputLanguage, outputLanguage, '\0', Dictionary, true);
                        if (converter.To(out outputString, inputString))
                        {
                            translatorSource = LanguageTranslatorSource.Conversion;
                            return true;
                        }
                        else
                            return false;
                    }
                    else if (inputLanguage.LanguageCultureExtensionCode == "ja--rj")
                    {
                        ConvertRomaji converter = new ConvertRomaji(outputLanguage, '\0', Dictionary, true);
                        if (converter.To(out outputString, inputString))
                        {
                            translatorSource = LanguageTranslatorSource.Conversion;
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        LanguageID japaneseID = LanguageLookup.Japanese;
                        string japaneseText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, japaneseID, out japaneseText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Dictionary;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                japaneseID,
                                false,
                                out japaneseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            ConvertTransliterate converter = new ConvertTransliterate(japaneseID, outputLanguage, '\0', Dictionary, true);
                            return converter.To(out outputString, japaneseText);
                        }
                        else
                            returnValue = false;
                    }
                    break;
                default:
                    break;
            }

            if (UseDictionary && (Dictionary != null))
            {
                gotIt = Dictionary.TranslateString(inputString, inputLanguage, outputLanguage, out outputString);

                if (gotIt)
                    translatorSource = LanguageTranslatorSource.Dictionary;
            }

            if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
            {
                if (!IsLanguageSupported(inputLanguage))
                    errorMessage = "Language not supported for automated translation: " + inputLanguage.LanguageName(LanguageLookup.English);
                else if (!IsLanguageSupported(outputLanguage))
                    errorMessage = "Language not supported for automated translation: " + outputLanguage.LanguageName(LanguageLookup.English);
                else
                    gotIt = TranslateService(
                        operationPrefix,
                        userRecord,
                        databaseName,
                        stringID,
                        inputString,
                        inputLanguage,
                        outputLanguage,
                        false,
                        out outputString,
                        out translationsWithAlternates,
                        out translatorSource,
                        out errorMessage);
            }

            if (!gotIt)
                returnValue = false;

            return returnValue;
        }

        public virtual bool TranslateStringWithAlternates(
            string operationPrefix,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out List<string> outputList,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage)
        {
            bool returnValue = TranslateStringUserWithAlternates(
                operationPrefix,
                UserRecord,
                databaseName,
                stringID,
                inputString,
                inputLanguage,
                outputLanguage,
                out outputList,
                out translatorSource,
                out errorMessage);
            return returnValue;
        }

        public virtual bool TranslateStringUserWithAlternates(
            string operationPrefix,
            UserRecord userRecord,
            string databaseName,
            string stringID,
            string inputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            out List<string> outputList,
            out LanguageTranslatorSource translatorSource,
            out string errorMessage)
        {
            string userName = (userRecord != null ? userRecord.UserName : String.Empty);
            bool gotIt = false;
            string result;
            List<string> translationsWithAlternates = null;
            bool returnValue = true;

            outputList = null;
            translatorSource = LanguageTranslatorSource.NotFound;
            errorMessage = "";

            if (ApplicationData.Global.IsCanceled(userName, "AddNodeTranslations"))
                throw new OperationCanceledException(ApplicationData.Global.GetCanceledMessage(userName, "AddNodeTranslations"));

            if ((inputLanguage == null) || !inputLanguage.IsLanguage())
            {
                errorMessage = "Need an input language.";
                return false;
            }

            if ((outputLanguage == null) || !outputLanguage.IsLanguage())
            {
                errorMessage = "Need an output language.";
                return false;
            }

            switch (outputLanguage.ExtensionCode)
            {
                case "pn":
                    if (inputLanguage.LanguageCode != "zh")
                    {
                        LanguageID chineseID = LanguageLookup.ChineseSimplified;
                        string chineseText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, chineseID, out chineseText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Dictionary;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                chineseID,
                                true,
                                out chineseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            outputList = new List<string>(translationsWithAlternates.Count());
                            foreach (string trans in translationsWithAlternates)
                            {
                                ConvertToPinyin(chineseID, trans, false, out result);
                                outputList.Add(result);
                            }
                        }
                        else
                            returnValue = false;
                    }
                    else
                    {
                        ConvertToPinyin(inputLanguage, inputString, false, out result);
                        outputList = new List<string>(1) { result };
                        translatorSource = LanguageTranslatorSource.Conversion;
                        returnValue = true;
                    }
                    return returnValue;
                case "rm":
                    if (inputLanguage.LanguageCode == "ko")
                    {
                        if (inputString.Contains("\'"))
                        {
                            ConvertHangul converter = new ConvertHangul('\'', Dictionary, true);
                            returnValue = converter.To(out result, inputString);
                        }
                        else
                        {
                            ConvertHangul converter = new ConvertHangul('\0', Dictionary, true);
                            returnValue = converter.To(out result, inputString);
                        }
                        if (returnValue)
                        {
                            outputList = new List<string>(1) { result };
                            translatorSource = LanguageTranslatorSource.Conversion;
                        }
                        return returnValue;
                    }
                    else if (outputLanguage.LanguageCode == inputLanguage.LanguageCode)
                    {
                        ConvertTransliterate converter = new ConvertTransliterate(
                            true,
                            outputLanguage,
                            inputLanguage,
                            '\0',
                            Dictionary,
                            true);
                        returnValue = converter.To(out result, inputString);
                        if (returnValue)
                        {
                            outputList = new List<string>(1) { result };
                            translatorSource = LanguageTranslatorSource.Conversion;
                        }
                        return returnValue;
                    }
                    else
                    {
                        LanguageID sourceID = LanguageLookup.GetLanguageIDNoAdd(outputLanguage.LanguageCode);
                        string sourceText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, sourceID, out sourceText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Database;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                sourceID,
                                true,
                                out sourceText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            ConvertTransliterate converter = new ConvertTransliterate(
                                true,
                                outputLanguage,
                                inputLanguage,
                                '\0',
                                Dictionary,
                                true);
                            outputList = new List<string>(translationsWithAlternates.Count());
                            foreach (string trans in translationsWithAlternates)
                            {
                                converter.To(out result, trans);
                                outputList.Add(result);
                            }
                        }
                        else
                            returnValue = false;
                        return returnValue;
                    }
                case "rj":
                    if (inputLanguage.LanguageCode == "ja")
                    {
                        ConvertRomaji converter = new ConvertRomaji(inputLanguage, '\0', Dictionary, true);
                        returnValue = converter.To(out result, inputString);
                        if (returnValue)
                        {
                            outputList = new List<string>(1) { result };
                            translatorSource = LanguageTranslatorSource.Conversion;
                        }
                        return returnValue;
                    }
                    else
                    {
                        LanguageID japaneseID = LanguageLookup.Japanese;
                        string japaneseText = null;

                        if (UseDictionary && (Dictionary != null))
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, japaneseID, out japaneseText);

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                japaneseID,
                                true,
                                out japaneseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            ConvertRomaji converter = new ConvertRomaji(japaneseID, '\0', Dictionary, true);
                            outputList = new List<string>(translationsWithAlternates.Count());
                            foreach (string trans in translationsWithAlternates)
                            {
                                converter.To(out result, trans);
                                outputList.Add(result);
                            }
                        }
                        else
                            returnValue = false;
                        return returnValue;
                    }
                case "kn":
                    if (inputLanguage.LanguageCultureExtensionCode == "ja")
                    {
                        ConvertTransliterate converter = new ConvertTransliterate(inputLanguage, outputLanguage, '\0', Dictionary, true);
                        returnValue = converter.To(out result, inputString);
                        if (returnValue)
                        {
                            outputList = new List<string>(1) { result };
                            translatorSource = LanguageTranslatorSource.Conversion;
                        }
                        return returnValue;
                    }
                    else if (inputLanguage.LanguageCultureExtensionCode == "ja--rj")
                    {
                        ConvertRomaji converter = new ConvertRomaji(outputLanguage, '\0', Dictionary, true);
                        returnValue = converter.From(out result, inputString);
                        if (returnValue)
                        {
                            outputList = new List<string>(1) { result };
                            translatorSource = LanguageTranslatorSource.Conversion;
                        }
                        return returnValue;
                    }
                    else
                    {
                        LanguageID japaneseID = LanguageLookup.Japanese;
                        string japaneseText = null;

                        if (UseDictionary && (Dictionary != null))
                        {
                            gotIt = Dictionary.TranslateString(inputString, inputLanguage, japaneseID, out japaneseText);

                            if (gotIt)
                                translatorSource = LanguageTranslatorSource.Database;
                        }

                        if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
                            gotIt = TranslateService(
                                operationPrefix,
                                userRecord,
                                databaseName,
                                stringID,
                                inputString,
                                inputLanguage,
                                japaneseID,
                                true,
                                out japaneseText,
                                out translationsWithAlternates,
                                out translatorSource,
                                out errorMessage);

                        if (gotIt)
                        {
                            ConvertTransliterate converter = new ConvertTransliterate(japaneseID, outputLanguage, '\0', Dictionary, true);
                            outputList = new List<string>(translationsWithAlternates.Count());
                            foreach (string trans in translationsWithAlternates)
                            {
                                converter.To(out result, trans);
                                outputList.Add(result);
                            }
                        }
                        else
                            returnValue = false;
                        return returnValue;
                    }
                default:
                    break;
            }

            if (UseDictionary && (Dictionary != null))
            {
                gotIt = Dictionary.TranslateString(inputString, inputLanguage, outputLanguage, out result);

                if (gotIt)
                {
                    outputList = new List<string>(1) { result };
                    translatorSource = LanguageTranslatorSource.Dictionary;
                }
            }

            if (!gotIt && (UseAutomatedTranslation || !UseDictionary))
            {
                if (!IsLanguageSupported(inputLanguage))
                    errorMessage = "Language not supported for automated translation: " + inputLanguage.LanguageName(LanguageLookup.English);
                else if (!IsLanguageSupported(outputLanguage))
                    errorMessage = "Language not supported for automated translation: " + outputLanguage.LanguageName(LanguageLookup.English);
                else
                    gotIt = TranslateService(
                        operationPrefix,
                        userRecord,
                        databaseName,
                        stringID,
                        inputString,
                        inputLanguage,
                        outputLanguage,
                        true,
                        out result,
                        out outputList,
                        out translatorSource,
                        out errorMessage);
            }

            if (!gotIt)
                returnValue = false;

            return returnValue;
        }

        public void ConvertToPinyin(LanguageID inputLanguage, string inputString, bool toNumeric, out string outputString)
        {
            ConvertPinyin.ConvertToPinyin(inputLanguage, inputString, toNumeric, out outputString, Dictionary,
                FormatQuickLookup.GetQuickDictionary(inputLanguage, LanguageLookup.ChinesePinyin));
        }

        public bool ConvertCharacters(LanguageID inputLanguage, LanguageID outputLanguage, char syllableSeparator,
            string inputString, out string outputString)
        {
            return ConvertTransliterate.ConvertCharacters(inputLanguage, outputLanguage, syllableSeparator, inputString, out outputString,
                Dictionary, FormatQuickLookup.GetQuickDictionary(inputLanguage, outputLanguage));
        }

        public void InputTransliterateCheck(LanguageID inputLanguage, string inputString, out string outputString)
        {
            outputString = ConvertTransliterate.InputTransliterateCheck(inputString, inputLanguage);
        }

        public LanguageString GetBaseLanguageString(LanguageID destLanguageID, MultiLanguageString multiLanguageString,
            List<LanguageID> languageIDs)
        {
            if ((multiLanguageString == null) || (multiLanguageString.LanguageStrings == null))
                return null;

            LanguageString bestLanguageString = null;
            int bestWeight = -1;

            foreach (LanguageString languageString in multiLanguageString.LanguageStrings)
            {
                if ((languageIDs != null) && !languageIDs.Contains(languageString.LanguageID))
                    continue;

                if (String.IsNullOrEmpty(languageString.Text) || (languageString.LanguageID == destLanguageID) || languageString.Text.StartsWith("(enter "))
                    continue;

                if (!IsLanguageSupported(languageString.LanguageID))
                    continue;

                int weight = LanguageLookup.GetTranslationWeight(destLanguageID, languageString.LanguageID, languageIDs);

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestLanguageString = languageString;
                }
            }

            return bestLanguageString;
        }

        public bool TranslateMultiLanguageString(MultiLanguageString multiLanguageString, List<LanguageID> languageIDs,
            out string errorMessage, bool forceIt)
        {
            LanguageTranslatorSource translatorSource;
            bool returnValue = true;

            errorMessage = "";

            if ((multiLanguageString.LanguageStrings == null) || (multiLanguageString.LanguageStrings.Count() == 0))
            {
                errorMessage = "There are no multiLanguageStrings.";
                return false;
            }

            LanguageString baseLanguageString = null;
            LanguageID inputLanguageID = null;
            LanguageID outputLanguageID = null;
            string text = String.Empty;
            string message;
            string languagesNotSupportedNames = String.Empty;
            int languagesNotSupportedCount = 0;

            int nonEmptyCount = 0;

            foreach (LanguageString sls in multiLanguageString.LanguageStrings)
            {
                if ((languageIDs != null) && !languageIDs.Contains(sls.LanguageID))
                    continue;

                if (!String.IsNullOrEmpty(sls.Text) && !sls.Text.StartsWith("(enter "))
                {
                    nonEmptyCount = 1;
                    break;
                }
            }

            if (nonEmptyCount == 0)
            {
                errorMessage = "Need at least one non-empty multiLanguageString.";
                return false;
            }

            if (forceIt)
            {
                foreach (LanguageString ls in multiLanguageString.LanguageStrings)
                {
                    if (ls.LanguageID == UserProfile.HostLanguageID)
                        continue;

                    if (!languageIDs.Contains(ls.LanguageID))
                        continue;

                    ls.Text = "";
                }
            }

            bool needToolTransliteration = false;
            LanguageID toolLanguageID = null;

            foreach (LanguageString ls in multiLanguageString.LanguageStrings)
            {
                if ((languageIDs != null) && !languageIDs.Contains(ls.LanguageID))
                    continue;

                if (!String.IsNullOrEmpty(ls.Text) && !ls.Text.StartsWith("(enter "))
                    continue;

                outputLanguageID = ls.LanguageID;
                baseLanguageString = GetBaseLanguageString(outputLanguageID, multiLanguageString, languageIDs);

                if (baseLanguageString == null)
                    continue;

                inputLanguageID = baseLanguageString.LanguageID;

                if ((outputLanguageID == LanguageLookup.JapaneseKana) || (outputLanguageID == LanguageLookup.JapaneseRomaji))
                {
                    needToolTransliteration = true;
                    toolLanguageID = LanguageLookup.Japanese;
                    continue;
                }

                bool gotIt = TranslateString(
                    "ContentTranslate",
                    null,
                    null,
                    baseLanguageString.Text,
                    inputLanguageID,
                    outputLanguageID,
                    out text,
                    out translatorSource,
                    out message);
                if (gotIt)
                    ls.Text = text;
                else
                {
                    returnValue = false;

                    if (!IsLanguageSupported(outputLanguageID))
                    {
                        languagesNotSupportedCount++;

                        if (!String.IsNullOrEmpty(languagesNotSupportedNames))
                            languagesNotSupportedNames += ", ";

                        languagesNotSupportedNames += outputLanguageID.LanguageName(LanguageLookup.English);

                        continue;
                    }

                    if (!String.IsNullOrEmpty(message) && !errorMessage.Contains(message))
                        errorMessage += message;
                }
            }

            if (needToolTransliteration)
            {
                LanguageTool tool = GetLanguageTool(toolLanguageID);

                if (tool != null)
                    returnValue = tool.TransliterateMultiLanguageString(multiLanguageString, false) && returnValue;
            }

            if (languagesNotSupportedCount != 0)
                errorMessage = "Some languages not supported: " + languagesNotSupportedNames;

            return returnValue;
        }

        private LanguageItem GetBaseLanguageItem(LanguageID destLanguageID, MultiLanguageItem multiLanguageItem, List<LanguageID> languageIDs)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.LanguageItems == null))
                return null;

            LanguageItem bestLanguageItem = null;
            int bestWeight = -1;

            foreach (LanguageItem languageItem in multiLanguageItem.LanguageItems)
            {
                if ((languageIDs != null) && !languageIDs.Contains(languageItem.LanguageID))
                    continue;

                if (String.IsNullOrEmpty(languageItem.Text) || (languageItem.LanguageID == destLanguageID) || languageItem.Text.StartsWith("(enter "))
                    continue;

                if (!IsLanguageSupported(languageItem.LanguageID))
                {
                    if (!LanguageLookup.IsPhoneticToNonRomanizedPhoneticLanguageIDCompare(destLanguageID, languageItem.LanguageID))
                        continue;
                }

                int weight = LanguageLookup.GetTranslationWeight(destLanguageID, languageItem.LanguageID, languageIDs);

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestLanguageItem = languageItem;
                }
            }

            return bestLanguageItem;
        }

        public bool TranslateMultiLanguageItem(MultiLanguageItem multiLanguageItem, List<LanguageID> languageIDs,
            bool needsSentenceParsing, bool needsWordParsing, out string errorMessage, bool forceIt)
        {
            LanguageTranslatorSource translatorSource;
            string message;
            bool returnValue = true;

            errorMessage = "";

            if ((multiLanguageItem.LanguageItems == null) || (multiLanguageItem.Count() == 0))
            {
                errorMessage = "There are no multiLanguageItems.";
                return false;
            }

            if (multiLanguageItem.Count(languageIDs) < languageIDs.Count())
                ContentUtilities.SynchronizeMultiLanguageItemLanguages(
                    multiLanguageItem, String.Empty,
                    languageIDs);

            LanguageItem baseLanguageItem = null;
            LanguageID inputLanguageID = null;
            LanguageID outputLanguageID = null;
            string text = String.Empty;
            string languagesNotSupportedNames = String.Empty;
            int languagesNotSupportedCount = 0;

            int nonEmptyCount = 0;

            foreach (LanguageItem sls in multiLanguageItem.LanguageItems)
            {
                if ((languageIDs != null) && !languageIDs.Contains(sls.LanguageID))
                    continue;

                if (!String.IsNullOrEmpty(sls.Text) && !sls.Text.StartsWith("(enter "))
                {
                    nonEmptyCount = 1;
                    break;
                }
            }

            if (nonEmptyCount == 0)
            {
                errorMessage = "Need at least one non-empty multiLanguageItem.";
                return false;
            }

            if (forceIt)
            {
                foreach (LanguageItem ls in multiLanguageItem.LanguageItems)
                {
                    if (ls.LanguageID == UserProfile.HostLanguageID)
                        continue;

                    if ((languageIDs != null) && !languageIDs.Contains(ls.LanguageID))
                        continue;

                    ls.Text = "";
                    ls.DeleteWordRuns();
                    ls.DeleteSentenceRuns();
                }
            }

            bool needToolTransliteration = false;
            LanguageID toolLanguageID = null;

            foreach (LanguageItem ls in multiLanguageItem.LanguageItems)
            {
                if ((languageIDs != null) && !languageIDs.Contains(ls.LanguageID))
                    continue;

                if (!String.IsNullOrEmpty(ls.Text) && !ls.Text.StartsWith("(enter "))
                    continue;

                outputLanguageID = ls.LanguageID;
                baseLanguageItem = GetBaseLanguageItem(outputLanguageID, multiLanguageItem, languageIDs);

                if (baseLanguageItem == null)
                    continue;

                inputLanguageID = baseLanguageItem.LanguageID;

                if ((outputLanguageID == LanguageLookup.JapaneseKana) || (outputLanguageID == LanguageLookup.JapaneseRomaji))
                {
                    needToolTransliteration = true;
                    toolLanguageID = LanguageLookup.Japanese;
                    continue;
                }

                if (LanguageLookup.HasAnyAlternates(outputLanguageID))
                {
                    LanguageTool tool = GetLanguageTool(LanguageLookup.GetRootLanguageID(outputLanguageID));
                    List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(outputLanguageID);
                    List<LanguageID> familyLanguageIDs = LanguageLookup.GetFamilyLanguageIDs(outputLanguageID);
                    bool found = false;

                    foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                    {
                        LanguageItem alternateLanguageItem = multiLanguageItem.LanguageItem(alternateLanguageID);

                        if ((alternateLanguageItem != null) && (alternateLanguageItem.HasText()))
                        {
                            bool isInflection;
                            DictionaryEntry entry = Repositories.Dictionary.Get(alternateLanguageItem.Text, alternateLanguageID);

                            if (entry == null)
                                entry = tool.LookupDictionaryEntry(
                                    alternateLanguageItem.Text,
                                    Matchers.MatchCode.Exact,
                                    familyLanguageIDs,
                                    null,
                                    out isInflection);

                            if (entry != null)
                            {
                                if (outputLanguageID == LanguageLookup.Japanese)
                                {
                                    Sense sense = entry.GetSenseIndexed(0);
                                    if (!String.IsNullOrEmpty(sense.CategoryString) && sense.CategoryString.Contains("uk"))
                                    {
                                        if (entry.LanguageID == LanguageLookup.JapaneseKana)
                                            ls.Text = entry.KeyString;
                                        else
                                            ls.Text = entry.GetFirstAlternateText(LanguageLookup.JapaneseKana);
                                        if (!String.IsNullOrEmpty(ls.Text))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                ls.Text = entry.GetFirstAlternateText(outputLanguageID);
                                if (!String.IsNullOrEmpty(ls.Text))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (found)
                        continue;
                }

                bool isAlternate = LanguageLookup.IsAlternateOfLanguageID(outputLanguageID, inputLanguageID);
                bool hasWordRuns = false;

                if (isAlternate)
                {
                    hasWordRuns = baseLanguageItem.HasWordRuns();

                    if (!hasWordRuns)
                    {
                        baseLanguageItem.LoadWordRunsFromText(Dictionary);
                        hasWordRuns = baseLanguageItem.HasWordRuns();
                    }
                }

                if (isAlternate && hasWordRuns)
                {
                    List<string> sourceFragments = null;
                    List<TextRun> sourceRuns = null;

                    baseLanguageItem.GetWordFragmentsAndRuns(ref sourceFragments, ref sourceRuns);

                    TranslateLanguageItem(
                        ls,
                        inputLanguageID,
                        sourceFragments,
                        sourceRuns,
                        needsSentenceParsing);
                }
                else
                {
                    bool gotIt = TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        baseLanguageItem.Text,
                        inputLanguageID,
                        outputLanguageID,
                        out text,
                        out translatorSource,
                        out message);

                    if (gotIt)
                    {
                        ls.Text = text;

                        if (needsSentenceParsing)
                        {
                            if (baseLanguageItem.HasSentenceRuns())
                            {
                                if (baseLanguageItem.SentenceRunCount() == 1)
                                    ls.CollapseSentenceRuns();
                                else
                                    ls.LoadSentenceRunsFromText();
                            }

                            //if ((Dictionary != null) && baseLanguageItem.HasWordRuns())
                            //    ls.LoadWordRunsFromText(Dictionary);
                        }
                        else if (ls.HasSentenceRuns())
                            ls.ResetSentenceRuns();

                        ls.UpdateWordRunsFromText(needsWordParsing, Dictionary);
                    }
                    else
                    {
                        returnValue = false;

                        if (!IsLanguageSupported(outputLanguageID))
                        {
                            languagesNotSupportedCount++;

                            if (!String.IsNullOrEmpty(languagesNotSupportedNames))
                                languagesNotSupportedNames += ", ";

                            languagesNotSupportedNames += outputLanguageID.LanguageName(LanguageLookup.English);
                            continue;
                        }

                        if (!String.IsNullOrEmpty(message) && !errorMessage.Contains(message))
                        {
                            if (!String.IsNullOrEmpty(errorMessage))
                                errorMessage += message;
                            else
                                errorMessage = message;
                        }

                        //if (!String.IsNullOrEmpty(errorMessage))
                        //    break;
                    }
                }
            }

            if (needToolTransliteration)
            {
                LanguageTool tool = GetLanguageTool(toolLanguageID);

                if (tool != null)
                    returnValue = tool.TransliterateMultiLanguageItem(multiLanguageItem, false) && returnValue;
            }

            if (multiLanguageItem.HasAnnotations())
            {
                foreach (Annotation annotation in multiLanguageItem.Annotations)
                {
                    if ((annotation.Text == null) || !annotation.Text.HasText())
                        continue;

                    if (!TranslateAnnotation(annotation, languageIDs, out message, forceIt))
                    {
                        if (String.IsNullOrEmpty(errorMessage))
                            errorMessage = message;

                        returnValue = false;

                        if (!String.IsNullOrEmpty(errorMessage))
                            break;
                    }
                }
            }

            if (languagesNotSupportedCount != 0)
                errorMessage = "Some languages not supported: " + languagesNotSupportedNames;

            return returnValue;
        }

        protected void TranslateLanguageItem(
            LanguageItem targetLanguageItem,
            LanguageID sourceLanguageID,
            List<string> sourceFragments,
            List<TextRun> sourceRuns,
            bool needsSentenceParsing)
        {
            LanguageID languageID = targetLanguageItem.LanguageID;
            int fragmentCount = sourceFragments.Count();
            int fragmentIndex;
            StringBuilder sb = new StringBuilder();
            List<TextRun> wordRuns = new List<TextRun>();
            TextRun sourceRun;
            string sourceFragment;
            TextRun wordRun;
            string wordFragment;
            bool lastWasWord = false;
            bool lastWasPunct = false;
            LanguageTool tool = GetLanguageTool(sourceLanguageID);

            if (LanguageLookup.IsRomanized(languageID))
            {
                ConvertTransliterate transliterator = new ConvertTransliterate(
                    true,
                    languageID,
                    sourceLanguageID,
                    '\0',
                    Dictionary,
                    false);

                for (fragmentIndex = 0; fragmentIndex < fragmentCount; fragmentIndex++)
                {
                    sourceFragment = sourceFragments[fragmentIndex];
                    sourceRun = sourceRuns[fragmentIndex];

                    if (!transliterator.To(out wordFragment, sourceFragment))
                        wordFragment = sourceFragment;

                    wordFragment = tool.FixupTransliteration(
                        wordFragment,
                        languageID,
                        sourceFragment,
                        sourceLanguageID,
                        true);

                    if (tool != null)
                        wordFragment = tool.FixUpText(languageID, wordFragment);

                    if (!String.IsNullOrEmpty(wordFragment))
                    {
                        if (sourceRun != null)
                        {
                            if (lastWasWord || lastWasPunct)
                                sb.Append(" ");
                            wordRun = new TextRun(sb.Length, wordFragment.Length, null);
                            wordRuns.Add(wordRun);
                            lastWasWord = true;
                        }
                        else
                        {
                            if (LanguageLookup.PunctuationNeedingSpaceAfter.Contains(wordFragment[wordFragment.Length - 1]))
                                lastWasPunct = true;
                            else
                                lastWasPunct = false;

                            if (LanguageLookup.PunctuationNeedingSpaceBefore.Contains(wordFragment[0]) &&
                                    lastWasWord)
                                wordFragment = wordFragment.Insert(0, " ");

                            lastWasWord = false;
                        }

                        sb.Append(wordFragment);
                    }
                }

                targetLanguageItem.Text = sb.ToString();
                targetLanguageItem.WordRuns = wordRuns;

                if (needsSentenceParsing)
                    targetLanguageItem.LoadSentenceRunsFromText();
            }
            else
            {
                for (fragmentIndex = 0; fragmentIndex < fragmentCount; fragmentIndex++)
                {
                    sourceFragment = sourceFragments[fragmentIndex];
                    sourceRun = sourceRuns[fragmentIndex];

                    if (tool != null)
                        wordFragment = tool.CharacterConvertLanguageText(sourceFragment, languageID);
                    else
                        wordFragment = sourceFragment;

                    if (!String.IsNullOrEmpty(wordFragment))
                    {
                        if (sourceRun != null)
                        {
                            wordRun = new TextRun(sb.Length, wordFragment.Length, null);
                            wordRuns.Add(wordRun);
                        }

                        sb.Append(wordFragment);
                    }
                }

                targetLanguageItem.Text = sb.ToString();
                targetLanguageItem.WordRuns = wordRuns;

                if (needsSentenceParsing)
                    targetLanguageItem.LoadSentenceRunsFromText();
            }
        }

        public bool TranslateSentenceRun(
            MultiLanguageItem multiLanguageItem,
            int sentenceIndex,
            List<LanguageID> languageIDs,
            bool needsWordParsing,
            out string errorMessage,
            bool forceIt)
        {
            LanguageTranslatorSource translatorSource;
            bool returnValue = true;

            errorMessage = "";

            if ((multiLanguageItem.LanguageItems == null) || (multiLanguageItem.LanguageItems.Count() == 0))
            {
                errorMessage = "There are no multiLanguageItems.";
                return false;
            }

            LanguageItem baseLanguageItem = null;
            TextRun baseTextRun = null;
            TextRun textRun = null;
            LanguageID inputLanguageID = null;
            LanguageID outputLanguageID = null;
            string text = String.Empty;

            int nonEmptyCount = 0;

            foreach (LanguageItem sli in multiLanguageItem.LanguageItems)
            {
                if (!languageIDs.Contains(sli.LanguageID))
                    continue;

                textRun = sli.GetSentenceRun(sentenceIndex);

                if ((textRun != null) && (textRun.Length != 0))
                {
                    nonEmptyCount = 1;
                    break;
                }
            }

            if (nonEmptyCount == 0)
            {
                errorMessage = "Need at least one non-empty sentence.";
                return false;
            }

            if (forceIt)
            {
                foreach (LanguageItem li in multiLanguageItem.LanguageItems)
                {
                    if (li.LanguageID == UserProfile.HostLanguageID)
                        continue;

                    if (!languageIDs.Contains(li.LanguageID))
                        continue;

                    li.ClearSentenceRunIndexed(sentenceIndex);
                }
            }

            foreach (LanguageItem li in multiLanguageItem.LanguageItems)
            {
                if (!languageIDs.Contains(li.LanguageID))
                    continue;

                textRun = li.GetSentenceRun(sentenceIndex);

                if ((textRun != null) && (textRun.Length != 0))
                    continue;

                outputLanguageID = li.LanguageID;
                baseLanguageItem = GetBaseLanguageItem(outputLanguageID, multiLanguageItem, languageIDs);

                if (baseLanguageItem == null)
                    continue;

                inputLanguageID = baseLanguageItem.LanguageID;
                baseTextRun = baseLanguageItem.GetSentenceRun(sentenceIndex);

                if (baseTextRun == null)
                    continue;

                bool gotIt = TranslateString(
                    "ContentTranslate",
                    null,
                    null,
                    baseLanguageItem.GetRunText(baseTextRun),
                    inputLanguageID,
                    outputLanguageID,
                    out text,
                    out translatorSource,
                    out errorMessage);

                if (gotIt)
                {
                    if (!li.SetRunText(sentenceIndex, text))
                        returnValue = false;
                    else if (needsWordParsing)
                    {
                        LanguageTool tool = GetLanguageTool(outputLanguageID);
                        if (tool != null)
                            multiLanguageItem.UpdateLanguageItemWordRuns(li, tool);
                        else
                            li.LoadWordRunsFromText(Dictionary);
                    }
                }
                else
                    returnValue = false;
            }

            return returnValue;
        }

        public bool TranslateAnnotation(Annotation annotation, List<LanguageID> languageIDs,
            out string errorMessage, bool forceIt)
        {
            errorMessage = null;

            if (annotation == null)
            {
                errorMessage = "The annotation is null.";
                return false;
            }

            if (!annotation.IsTextType())
                return true;

            if (annotation.Text == null)
            {
                errorMessage = "The annotation is null.";
                return false;
            }

            ObjectUtilities.PrepareMultiLanguageString(annotation.Text, String.Empty, languageIDs);

            return TranslateMultiLanguageString(annotation.Text, languageIDs, out errorMessage, forceIt);
        }

        public bool TranslateDictionaryEntry(DictionaryEntry dictionaryEntry, List<LanguageID> hostLanguageIDs,
            out string errorMessage)
        {
            LanguageTranslatorSource translatorSource;
            errorMessage = null;

            if ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0))
            {
                errorMessage = "No host languages.";
                return false;
            }

            List<LanguageID> doneTranslationIDs = new List<LanguageID>();
            Dictionary<LanguageID, string> translations = new Dictionary<LanguageID, string>();
            string word = dictionaryEntry.KeyString;
            LanguageID targetLanguageID = dictionaryEntry.LanguageID;
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(targetLanguageID);
            bool gotIt;
            bool returnValue = true;

            if ((alternateLanguageIDs != null) && (alternateLanguageIDs.Count() != 0))
            {
                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                {
                    if (doneTranslationIDs.Contains(alternateLanguageID))
                        continue;

                    if (dictionaryEntry.HasAlternateLanguage(alternateLanguageID))
                    {
                        doneTranslationIDs.Add(alternateLanguageID);
                        continue;
                    }

                    LanguageID rootAlternateLanguageID = LanguageLookup.GetRootLanguageID(alternateLanguageID);

                    if (rootAlternateLanguageID == alternateLanguageID)
                        continue;

                    string wordTranslation;

                    gotIt = TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        word,
                        targetLanguageID,
                        alternateLanguageID,
                        out wordTranslation,
                        out translatorSource,
                        out errorMessage);

                    if (gotIt)
                        translations.Add(alternateLanguageID, wordTranslation);
                    else
                        returnValue = false;

                    doneTranslationIDs.Add(alternateLanguageID);
                }
            }

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                if (doneTranslationIDs.Contains(hostLanguageID))
                    continue;

                LanguageID rootHostLanguageID = LanguageLookup.GetRootLanguageID(hostLanguageID);

                if (LanguageLookup.IsSameFamily(rootHostLanguageID, targetLanguageID))
                    continue;

                string rootTranslation;
                string wordTranslation;

                if (!translations.TryGetValue(rootHostLanguageID, out rootTranslation))
                {
                    gotIt = TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        word,
                        targetLanguageID,
                        rootHostLanguageID,
                        out rootTranslation,
                        out translatorSource,
                        out errorMessage);

                    if (gotIt)
                        translations.Add(rootHostLanguageID, rootTranslation);
                    else
                        returnValue = false;

                    doneTranslationIDs.Add(rootHostLanguageID);
                }
                else
                    gotIt = true;

                if (!gotIt)
                    continue;

                if (hostLanguageID != rootHostLanguageID)
                {
                    gotIt = TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        rootTranslation,
                        rootHostLanguageID,
                        hostLanguageID,
                        out wordTranslation,
                        out translatorSource,
                        out errorMessage);

                    if (gotIt)
                        translations.Add(hostLanguageID, wordTranslation);
                    else
                        returnValue = false;

                    doneTranslationIDs.Add(hostLanguageID);
                }
            }

            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();

            foreach (LanguageID languageID in hostLanguageIDs)
            {
                string wordTranslation;

                if (LanguageLookup.IsSameFamily(languageID, targetLanguageID))
                    continue;

                if (!translations.TryGetValue(languageID, out wordTranslation))
                    continue;

                if ((alternateLanguageIDs != null) && alternateLanguageIDs.Contains(languageID))
                {
                    int reading = dictionaryEntry.AllocateAlternateKey(languageID);
                    LanguageString alternate = new LanguageString(reading, languageID, wordTranslation);
                    dictionaryEntry.AddAlternate(alternate);
                }
                else
                {
                    ProbableMeaning probableSynonym = new ProbableMeaning(
                        wordTranslation,
                        LexicalCategory.Unknown,
                        null,
                        float.NaN,
                        0,
                        NodeUtilities.TranslateDictionarySourceID);
                    List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                    LanguageSynonyms languageSynonyms = new LanguageSynonyms(languageID, probableSynonyms);
                    languageSynonymsList.Add(languageSynonyms);
                }
            }

            Sense sense = new Sense(
                0,
                LexicalCategory.Unknown,
                "(auto)",
                0,
                languageSynonymsList,
                null);

            dictionaryEntry.AddSense(sense);

            return returnValue;
        }

        public bool AddTitledObjectTranslationsCheck(BaseObjectTitled item, List<LanguageID> languageIDs,
            bool isTranslate, out string message, out bool translated)
        {
            message = "";
            translated = false;
            bool returnValue = true;

            if (!isTranslate)
                return true;

            if (item == null)
                return true;

            if ((item.Title != null) && item.Title.HasText(languageIDs))
            {
                ObjectUtilities.PrepareMultiLanguageString(item.Title, "", languageIDs);
                if (!TranslateMultiLanguageString(item.Title, languageIDs, out message, false))
                    returnValue = false;
            }

            if ((item.Description != null) && item.Description.HasText(languageIDs))
            {
                ObjectUtilities.PrepareMultiLanguageString(item.Description, "", languageIDs);
                if (!TranslateMultiLanguageString(item.Description, languageIDs, out message, false))
                    returnValue = false;
            }

            translated = true;

            return returnValue;
        }

        public bool AddStudyListTranslationsCheck(BaseObjectContent content, int editIndex,
            List<LanguageID> languageIDs, bool isTranslate, out string message, out bool translated)
        {
            ContentStudyList studyList = content.GetContentStorageTyped<ContentStudyList>();
            bool needsSentenceParsing = content.NeedsSentenceParsing;
            bool needsWordParsing = content.NeedsWordParsing;
            bool returnValue = true;

            message = "";
            translated = false;

            if (!isTranslate)
                return true;

            if (studyList == null)
                return true;

            List<LanguageDescriptor> languageDescriptors = LanguageDescriptor.LanguageDescriptorsFromLanguageIDs(content.LanguageIDs);

            if (editIndex >= 0)
            {
                if (editIndex < 0)
                    editIndex = studyList.StudyItemCount() - 1;

                if (editIndex >= 0)
                {
                    MultiLanguageItem multiLanguageItem = studyList.GetStudyItemIndexed(editIndex);
                    ContentUtilities.PrepareMultiLanguageItem(multiLanguageItem, "", languageDescriptors);

                    if (!TranslateMultiLanguageItem(
                            multiLanguageItem,
                            languageIDs,
                            needsSentenceParsing,
                            needsWordParsing,
                            out message,
                            false))
                        returnValue = false;
                }
            }

            translated = returnValue && studyList.Modified;

            return returnValue;
        }

        public bool AddSpeakerNameTranslationsCheck(ContentStudyList studyList, string speakerKey, List<LanguageID> languageIDs,
            bool isTranslate, out string message, out bool translated)
        {
            message = "";
            translated = false;
            bool returnValue = true;

            if (!isTranslate)
                return true;

            if (studyList == null)
                return true;

            if (studyList.SpeakerNames != null)
            {
                if (!String.IsNullOrEmpty(speakerKey))
                {
                    MultiLanguageString speakerName = studyList.GetSpeakerName(speakerKey);
                    if (speakerName != null)
                    {
                        ObjectUtilities.PrepareMultiLanguageString(speakerName, "", languageIDs);
                        if (!TranslateMultiLanguageString(speakerName, languageIDs, out message, false))
                            returnValue = false;
                    }
                }
            }

            translated = true;

            return returnValue;
        }

        public bool AddMarkupStringTranslationsCheck(MarkupTemplate markupTemplate, int editIndex,
            List<LanguageID> languageIDs, bool isTranslate, out string message, out bool translated)
        {
            bool needsSentenceParsing = false;
            bool needsWordParsing = false;
            bool returnValue = true;

            message = "";
            translated = false;

            if (!isTranslate)
                return true;

            if (markupTemplate == null)
                return true;

            if (languageIDs == null)
                languageIDs = markupTemplate.LanguageIDs;

            languageIDs = LanguageLookup.ExpandLanguageIDs(languageIDs, UserProfile);

            List<LanguageDescriptor> languageDescriptors = LanguageDescriptor.LanguageDescriptorsFromLanguageIDs(languageIDs);

            if (editIndex >= 0)
            {
                if (editIndex < 0)
                    editIndex = markupTemplate.MultiLanguageItemCount() - 1;

                if (editIndex >= 0)
                {
                    MultiLanguageItem multiLanguageItem = markupTemplate.MultiLanguageItemIndexed(editIndex);
                    ContentUtilities.PrepareMultiLanguageItem(multiLanguageItem, "", languageDescriptors);

                    if (!TranslateMultiLanguageItem(
                            multiLanguageItem,
                            languageIDs,
                            needsSentenceParsing,
                            needsWordParsing,
                            out message,
                            false))
                        returnValue = false;
                }
            }

            translated = returnValue && markupTemplate.Modified;

            return returnValue;
        }

        public abstract bool FixTranslation(
            string inputString,
            string fixedOutputString,
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            UserRecord userRecord,
            out string previousOutputString,
            out string errorMessage);

        public abstract bool ClearCache(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            UserRecord userRecord,
            out string errorMessage);

        // Synchronize language descriptions to those supported by the translation service.
        public abstract bool SynchronizeLanguageDescriptions(
            UserRecord userRecord,
            LanguageUtilities languageUtilities);

        public void UpdateUserProfile(UserProfile userProfile)
        {
            UserProfile = userProfile;

            if (UserProfile != null)
            {
                UseAutomatedTranslation = UserProfile.GetUserOptionFlag("UseAutomatedTranslation", true);
                UseDictionary = UserProfile.GetUserOptionFlag("UseDictionary", false);
            }
            else
            {
                UseAutomatedTranslation = true;
                UseDictionary = false;
            }
        }

        public virtual LanguageTool GetLanguageTool(LanguageID languageID)
        {
            LanguageTool languageTool;

            if (languageID == null)
                return null;

            if (LanguageToolCache == null)
                LanguageToolCache = new Dictionary<string, LanguageTool>();
            else if (LanguageToolCache.TryGetValue(languageID.LanguageCode, out languageTool))
                return languageTool;

            languageTool = ApplicationData.LanguageTools.Create(languageID);

            if (languageTool == null)
                languageTool = new GenericLanguageTool(languageID, UserProfile.HostLanguageIDs, UserProfile.LanguageIDs);
            else
            {
                languageTool.HostLanguageIDs = UserProfile.HostLanguageIDs;
                languageTool.UserLanguageIDs = UserProfile.LanguageIDs;
            }

            LanguageToolCache.Add(languageID.LanguageCode, languageTool);

            return languageTool;
        }

        public virtual void SetLanguageTool(LanguageTool languageTool)
        {
            string languageKey = languageTool.LanguageID.LanguageCode;
            LanguageTool testLanguageTool;

            if (LanguageToolCache == null)
            {
                LanguageToolCache = new Dictionary<string, LanguageTool>();
                LanguageToolCache.Add(languageKey, languageTool);
            }
            else if (LanguageToolCache.TryGetValue(languageKey, out testLanguageTool))
                LanguageToolCache[languageKey] = languageTool;
            else
                LanguageToolCache.Add(languageKey, languageTool);
        }
    }
}

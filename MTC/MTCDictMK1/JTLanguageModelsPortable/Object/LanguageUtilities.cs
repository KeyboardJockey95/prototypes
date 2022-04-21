using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Object
{
    public class LanguageUtilities
    {
        public IMainRepository Repositories { get; set; }
        public LanguageBaseStringRepository UIStrings { get; set; }
        public LanguageBaseStringRepository UIText { get; set; }
        public LanguageID UILanguage { get; set; }
        public ILanguageTranslator Translator { get; set; }
        public UserRecord UserRecord { get; set; }

        public LanguageUtilities(LanguageBaseStringRepository uiStrings, LanguageBaseStringRepository uiText, LanguageID uiLanguage,
            ILanguageTranslator translator, UserRecord userRecord, IMainRepository repositories)
        {
            UIStrings = uiStrings;
            UIText = uiText;
            UILanguage = uiLanguage;
            Translator = translator;
            UserRecord = userRecord;
            Repositories = repositories;
        }

        public string TranslateUIString(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                BaseString baseString = TranslateString(value, value, UILanguage, Repositories.UIStrings);

                if (baseString != null)
                    return baseString.Text;
            }

            return value;
        }

        public string TranslateUIStringNonNumeric(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (ObjectUtilities.IsNumberString(value))
                    return value;

                BaseString baseString = TranslateString(value, value, UILanguage, Repositories.UIStrings);

                if (baseString != null)
                    return baseString.Text;
            }

            return value;
        }

        public List<string> TranslateUIStringList(List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();

                foreach (string str in values)
                    newValues.Add(TranslateUIString(str));
            }

            return newValues;
        }

        public List<string> TranslateUIStringListNonNumeric(List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();

                foreach (string str in values)
                    newValues.Add(TranslateUIStringNonNumeric(str));
            }

            return newValues;
        }

        public string TranslateUIText(string stringID, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                BaseString baseString = TranslateString(stringID, value, UILanguage, Repositories.UIText);

                if (baseString != null)
                    return baseString.Text;
            }

            return value;
        }

        public string TranslateUITextNonNumeric(string stringID, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (ObjectUtilities.IsNumberString(value))
                    return value;

                BaseString baseString = TranslateString(stringID, value, UILanguage, Repositories.UIText);

                if (baseString != null)
                    return baseString.Text;
            }

            return value;
        }

        public List<string> TranslateUITextList(List<string> ids, List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();
                int c = values.Count();

                for (int i = 0; i < c; i++)
                {
                    string id = ids[i];
                    string str = values[i];
                    newValues.Add(TranslateUIText(id, str));
                }
            }

            return newValues;
        }

        public List<string> TranslateUITextListNonNumeric(List<string> ids, List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();
                int c = values.Count();

                for (int i = 0; i < c; i++)
                {
                    string id = ids[i];
                    string str = values[i];
                    newValues.Add(TranslateUITextNonNumeric(id, str));
                }
            }

            return newValues;
        }

        public List<string> TranslateUITextList(string idPrefix, List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();
                int c = values.Count();

                for (int i = 0; i < c; i++)
                {
                    string id = idPrefix + i.ToString();
                    string str = values[i];
                    newValues.Add(TranslateUIText(id, str));
                }
            }

            return newValues;
        }

        public List<string> TranslateUITextListNonNumeric(string idPrefix, List<string> values)
        {
            List<string> newValues = null;

            if (values != null)
            {
                newValues = new List<string>();
                int c = values.Count();

                for (int i = 0; i < c; i++)
                {
                    string id = idPrefix + i.ToString();
                    string str = values[i];
                    newValues.Add(TranslateUITextNonNumeric(id, str));
                }
            }

            return newValues;
        }

        public string TranslateString(string value, LanguageID languageID)
        {
            if (!String.IsNullOrEmpty(value))
            {
                BaseString baseString = TranslateString(value, value, languageID, Repositories.UIStrings);

                if (baseString != null)
                    return baseString.Text;
            }

            return value;
        }

        public List<string> TranslateStrings(string value, List<LanguageID> languageIDs)
        {
            List<string> newValues = null;

            if (!String.IsNullOrEmpty(value) && (languageIDs != null))
            {
                int languageCount = languageIDs.Count();
                int languageIndex;
                LanguageID languageID;

                newValues = new List<string>(languageCount);

                for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                {
                    languageID = languageIDs[languageIndex];
                    newValues.Add(TranslateString(value, languageID));
                }
            }

            return newValues;
        }

        public List<string> TranslateStrings(List<string> values, List<LanguageID> languageIDs)
        {
            if ((values != null) && (languageIDs != null))
            {
                int languageCount = languageIDs.Count();
                int languageIndex;
                LanguageID languageID;
                List<string> newValues = new List<string>(languageCount);

                for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                {
                    languageID = languageIDs[languageIndex];
                    newValues.Add(TranslateString(values[languageIndex], languageID));
                }
            }

            return values;
        }

        public virtual BaseString TranslateString(string stringID, string englishString, LanguageID languageID,
            LanguageBaseStringRepository languageBaseStringRepository)
        {
            if ((languageID == null) || (languageID.LanguageCode == "en"))
            {
                if (!ApplicationData.IsDontInhibitEnglishTranslations)
                    return new BaseString(englishString);
            }

            string targetValue = null;
            BaseString baseString = null;
            string errorMessage = null;

            baseString = languageBaseStringRepository.Get(stringID, languageID);

            if (baseString == null)
            {
                if (languageID.LanguageCode == "en")
                    targetValue = englishString;
                else if (!String.IsNullOrEmpty(englishString) && (Translator != null))
                {
                    LanguageTranslatorSource translatorSource;

                    if (!Translator.TranslateString(
                            "UITranslation",
                            languageBaseStringRepository.Name,
                            stringID,
                            englishString,
                            LanguageLookup.English,
                            languageID,
                            out targetValue,
                            out translatorSource,
                            out errorMessage))
                        return null;

                    if (ApplicationData.IsDontInhibitEnglishTranslations)
                    {
                        BaseString englishBaseString = languageBaseStringRepository.Get(stringID, LanguageLookup.English);

                        if (englishBaseString == null)
                        {
                            englishBaseString = new BaseString(stringID, englishString);
                            languageBaseStringRepository.Add(englishBaseString, LanguageLookup.English);
                        }
                    }
                }
                else
                    return null;

                baseString = new BaseString(stringID, targetValue);
                languageBaseStringRepository.Add(baseString, languageID);
            }
            else if (String.IsNullOrEmpty(baseString.Text))
            {
                if (languageID.LanguageCode == "en")
                    targetValue = englishString;
                else if (!String.IsNullOrEmpty(englishString) && (Translator != null))
                {
                    LanguageTranslatorSource translatorSource;

                    if (!Translator.TranslateString(
                            "UITranslation",
                            languageBaseStringRepository.Name,
                            stringID,
                            englishString,
                            LanguageLookup.English,
                            languageID,
                            out targetValue,
                            out translatorSource,
                            out errorMessage))
                        return null;

                    if (ApplicationData.IsDontInhibitEnglishTranslations)
                    {
                        BaseString englishBaseString = languageBaseStringRepository.Get(stringID, LanguageLookup.English);

                        if (englishBaseString == null)
                        {
                            englishBaseString = new BaseString(stringID, englishString);
                            languageBaseStringRepository.Add(englishBaseString, LanguageLookup.English);
                        }
                    }
                }
                else
                    return null;

                baseString.Text = targetValue;
                languageBaseStringRepository.Update(baseString, languageID);
            }
            else if ((languageID.LanguageCode == "en") && (englishString != baseString.Text))
            {
                baseString.Text = englishString;
                languageBaseStringRepository.Update(baseString, languageID);
            }

            return baseString;
        }

        public void TranslateMultiLanguageString(MultiLanguageString multiLanguageString)
        {
            if (multiLanguageString == null)
                return;

            if (multiLanguageString.LanguageStrings == null)
                return;

            List<LanguageID> languageIDs = multiLanguageString.LanguageIDs;
            string errorMessage;

            if (!Translator.TranslateMultiLanguageString(multiLanguageString, languageIDs, out errorMessage, false))
                return;
        }

        public bool TranslateDictionaryEntry(DictionaryEntry dictionaryEntry, List<LanguageID> languageIDs,
            out string errorMessage)
        {
            return Translator.TranslateDictionaryEntry(dictionaryEntry, languageIDs, out errorMessage);
        }

        public static LanguageID GetHostLanguageFromUILanguage(LanguageID uiLanguage)
        {
            if (uiLanguage == null)
                return uiLanguage;

            LanguageID hostLanguage = new LanguageID(uiLanguage.LanguageCode, null, uiLanguage.ExtensionCode);

            switch (uiLanguage.LanguageCode)
            {
                case "zh":
                    switch (uiLanguage.CultureCode)
                    {
                        case "CHS":
                        case "CHT":
                            hostLanguage.CultureCode = uiLanguage.CultureCode;
                            break;
                        case "CN":
                            hostLanguage.CultureCode = "CHS";
                            break;
                        default:
                            hostLanguage.CultureCode = "CHT";
                            break;
                    }
                    if (uiLanguage.ExtensionCode == "pn")
                        hostLanguage.CultureCode = null;
                    break;
                default:
                    break;
            }

            return hostLanguage;
        }

        public bool ValidateLanguageID(LanguageID languageID, out string message)
        {
            if (languageID == null)
            {
                message = TranslateUIString("No language specified.");
                return false;
            }

            message = "";
            return true;
        }

        public void FixupLanguageDescription(LanguageDescription languageDescription)
        {
            LanguageID languageID = languageDescription.LanguageID;
            string languageAndCultureName = null;

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "en-US":
                    languageAndCultureName = "English-USA";
                    break;
                case "en-CA":
                    languageAndCultureName = "English-Canada";
                    break;
                case "zh":
                    languageAndCultureName = "Chinese";
                    break;
                case "zh-CN":
                    languageAndCultureName = "Chinese-China";
                    break;
                case "zh-TW":
                    languageAndCultureName = "Chinese-Taiwan";
                    break;
                case "zh-CHS":
                    languageAndCultureName = "Chinese-Simplified";
                    break;
                case "zh-CHT":
                    languageAndCultureName = "Chinese-Traditional";
                    break;
                case "zh--pn":
                    languageAndCultureName = "Chinese-Pinyin";
                    break;
                case "zh-CN-pn":
                    languageAndCultureName = "Chinese-China-Pinyin";
                    break;
                case "zh-TW-pn":
                    languageAndCultureName = "Chinese-Taiwan-Pinyin";
                    break;
                default:
                    break;
            }

            switch (languageID.LanguageCode)
            {
                case "zh":
                case "ja":
                case "ko":
                    languageDescription.CharacterBased = true;
                    languageDescription.DictionaryFontSize = "16";
                    break;
                default:
                    break;
            }

            if (languageAndCultureName != null)
                languageDescription.LanguageName.LanguageString(0).Text = languageAndCultureName;
        }

        public bool FixupLanguageDescriptionPinyin(LanguageDescription languageDescription)
        {
            LanguageString pinyinName = languageDescription.LanguageName.LanguageString(LanguageLookup.ChinesePinyin);

            if (pinyinName == null)
            {
                BaseString chineseName = languageDescription.LanguageName.LanguageString(LanguageLookup.ChineseSimplified);

                if ((Translator != null) && (chineseName != null))
                {
                    string pinyinString = null;

                    Translator.ConvertToPinyin(LanguageLookup.ChineseSimplified, chineseName.Text, false, out pinyinString);

                    pinyinName = new LanguageString(chineseName.Key, LanguageLookup.ChinesePinyin, pinyinString);

                    languageDescription.LanguageName.Add(pinyinName);
                    languageDescription.LanguageName = languageDescription.LanguageName;
                    return true;
                }
            }

            return false;
        }

        public static string FormatDictionaryText(
            string text,
            LanguageID languageID,
            List<LanguageDescription> languageDescriptions)
        {
            string returnValue = text;

            if ((languageDescriptions != null) && (languageID != null))
            {
                LanguageDescription languageDescription = languageDescriptions.FirstOrDefault(x => x.LanguageID == languageID);
                StringBuilder sb = new StringBuilder();

                if ((languageDescription != null)
                    && (!String.IsNullOrEmpty(languageDescription.PreferedFontName)
                        || !String.IsNullOrEmpty(languageDescription.DictionaryFontSize)))
                {
                    string sep = null;

                    sb.Append("<span style=\"");

                    if (!String.IsNullOrEmpty(languageDescription.PreferedFontName))
                    {
                        sb.Append("font-family: ");
                        sb.Append(languageDescription.PreferedFontName);
                        sb.Append(";");
                        sep = " ";
                    }

                    if (!String.IsNullOrEmpty(languageDescription.DictionaryFontSize))
                    {
                        if (!String.IsNullOrEmpty(sep))
                            sb.Append(sep);
                        sb.Append("font-size: ");
                        sb.Append(languageDescription.DictionaryFontSize);
                        sb.Append("pt;");
                    }

                    sb.Append("\">");
                    sb.Append(text);
                    sb.Append("</span>");
                }
                else
                    sb.Append(text);

                string canonicalText = TextUtilities.GetCanonicalText(text, languageID);

                sb.Append("<span class=\"exp\">");
                sb.Append(canonicalText);
                sb.Append("</span>");

                returnValue = sb.ToString();
            }

            return returnValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Object
{
    public static partial class LanguageLookup
    {
        public static bool Reduced = true;
        public static LanguageID Arabic;
        public static LanguageID ArabicVowelled;
        public static LanguageID ArabicRomanization;
        public static LanguageID Bulgarian;
        public static LanguageID Catalan;
        public static LanguageID ChineseSimplified;
        public static LanguageID ChineseTraditional;
        public static LanguageID ChinesePinyin;
        public static LanguageID Czech;
        public static LanguageID Danish;
        public static LanguageID Dutch;
        public static LanguageID EgyptianArabic;
        public static LanguageID EgyptianArabicVowelled;
        public static LanguageID EgyptianArabicRomanization;
        public static LanguageID English;
        public static LanguageID Estonian;
        public static LanguageID Finnish;
        public static LanguageID French;
        public static LanguageID German;
        public static LanguageID Greek;
        public static LanguageID GreekRomanization;
        public static LanguageID HaitianCreole;
        public static LanguageID Hebrew;
        public static LanguageID HebrewVowelled;
        public static LanguageID HebrewRomanization;
        public static LanguageID Hindi;
        public static LanguageID Hungarian;
        public static LanguageID Indonesian;
        public static LanguageID Italian;
        public static LanguageID Japanese;
        public static LanguageID JapaneseKana;
        public static LanguageID JapaneseRomaji;
        public static LanguageID Korean;
        public static LanguageID KoreanRomanization;
        public static LanguageID Latvian;
        public static LanguageID Lithuanian;
        public static LanguageID Norwegian;
        public static LanguageID Polish;
        public static LanguageID Portuguese;
        public static LanguageID Romanian;
        public static LanguageID Russian;
        public static LanguageID Slovak;
        public static LanguageID Slovenian;
        public static LanguageID Spanish;
        public static LanguageID Swedish;
        public static LanguageID Tagalog;
        public static LanguageID Thai;
        public static LanguageID Turkish;
        public static LanguageID Ukrainian;
        public static LanguageID Vietnamese;
        public static LanguageID Target;
        public static LanguageID Host;
        public static LanguageID My;
        public static LanguageID Any;
        public static LanguageID AllLanguages;
        public static LanguageID None;
        public static LanguageID Select;

        public static List<LanguageID> LanguageIDs;
        public static List<LanguageID> TargetLanguageIDs;
        public static List<LanguageID> HostLanguageIDs;
        public static List<LanguageID> MyLanguageIDs;
        public static List<LanguageID> TargetHostMyLanguageIDs;
        public static List<LanguageID> AnyLanguageIDs;
        public static List<LanguageID> NoneLanguageIDs;

        private static string _AllLanguageKeys = null;

        public static string AllLanguageKeys
        {
            get
            {
                return _AllLanguageKeys;
            }
            set
            {
                _AllLanguageKeys = value;
            }
        }

        public static Dictionary<string, LanguageID> LanguageIDDictionary;

        public static Dictionary<string, string> DefaultFonts;

        public static LanguageUtilities LanguageUtilities;

        static LanguageLookup()
        {
            NonAlphanumericAndSpaceAndPunctuationCharacters = new char[SpaceAndPunctuationCharacters.Length + NonAlphanumericCharacters.Length];
            SpaceAndPunctuationCharacters.CopyTo(NonAlphanumericAndSpaceAndPunctuationCharacters, 0);
            NonAlphanumericCharacters.CopyTo(NonAlphanumericAndSpaceAndPunctuationCharacters, SpaceAndPunctuationCharacters.Length);
            Arabic = new LanguageID("ar");
            ArabicVowelled = new LanguageID("ar--vw");
            ArabicRomanization = new LanguageID("ar--rm");
            Bulgarian = new LanguageID("bg");
            ChineseSimplified = new LanguageID("zh-CHS");
            Catalan = new LanguageID("ca");
            ChineseTraditional = new LanguageID("zh-CHT");
            ChinesePinyin = new LanguageID("zh--pn");
            Czech = new LanguageID("cs");
            Danish = new LanguageID("da");
            Dutch = new LanguageID("nl");
            EgyptianArabic = new LanguageID("eg");
            EgyptianArabicVowelled = new LanguageID("eg--vw");
            EgyptianArabicRomanization = new LanguageID("eg--rm");
            English = new LanguageID("en");
            Estonian = new LanguageID("et");
            Finnish = new LanguageID("fi");
            French = new LanguageID("fr");
            German = new LanguageID("de");
            Greek = new LanguageID("el");
            GreekRomanization = new LanguageID("el--rm");
            HaitianCreole = new LanguageID("ht");
            Hebrew = new LanguageID("he");
            HebrewVowelled = new LanguageID("he--vw");
            HebrewRomanization = new LanguageID("he--rm");
            Hindi = new LanguageID("hi");
            Hungarian = new LanguageID("hu");
            Indonesian = new LanguageID("id");
            Italian = new LanguageID("it");
            Japanese = new LanguageID("ja");
            JapaneseKana = new LanguageID("ja--kn");
            JapaneseRomaji = new LanguageID("ja--rj");
            Korean = new LanguageID("ko");
            KoreanRomanization = new LanguageID("ko--rm");
            Latvian = new LanguageID("lv");
            Lithuanian = new LanguageID("lt");
            Norwegian = new LanguageID("no");
            Polish = new LanguageID("pl");
            Portuguese = new LanguageID("pt");
            Romanian = new LanguageID("ro");
            Russian = new LanguageID("ru");
            Slovak = new LanguageID("sk");
            Slovenian = new LanguageID("sl");
            Spanish = new LanguageID("es");
            Swedish = new LanguageID("sv");
            Tagalog = new LanguageID("tl");
            Thai = new LanguageID("th");
            Turkish = new LanguageID("tr");
            Ukrainian = new LanguageID("uk");
            Vietnamese = new LanguageID("vi");
            Target = new LanguageID("(target languages)");
            Host = new LanguageID("(host languages)");
            My = new LanguageID("(my languages)");
            Any = new LanguageID("(any)");
            AllLanguages = new Object.LanguageID("(all languages)");
            None = new LanguageID("(none)");
            Select = new LanguageID("(select)");

            LanguageIDs = new List<LanguageID>(100)
            {
                Arabic,
                ArabicVowelled,
                ArabicRomanization,
                Bulgarian,
                Catalan,
                ChineseSimplified,
                ChineseTraditional,
                ChinesePinyin,
                Czech,
                Danish,
                Dutch,
                EgyptianArabic,
                EgyptianArabicVowelled,
                EgyptianArabicRomanization,
                English,
                Estonian,
                Finnish,
                French,
                German,
                Greek,
                GreekRomanization,
                HaitianCreole,
                Hebrew,
                HebrewVowelled,
                HebrewRomanization,
                Hindi,
                Hungarian,
                Indonesian,
                Italian,
                Japanese,
                JapaneseKana,
                JapaneseRomaji,
                Korean,
                KoreanRomanization,
                Latvian,
                Lithuanian,
                Norwegian,
                Polish,
                Portuguese,
                Romanian,
                Russian,
                Slovak,
                Slovenian,
                Spanish,
                Swedish,
                Tagalog,
                Thai,
                Turkish,
                Ukrainian,
                Vietnamese,
                Target,
                Host,
                My,
                Any,
                AllLanguages,
                None
            };

            TargetLanguageIDs = new List<LanguageID>(1) { Target };
            HostLanguageIDs = new List<LanguageID>(1) { Host };
            MyLanguageIDs = new List<LanguageID>(1) { My };
            TargetHostMyLanguageIDs = new List<LanguageID>(1) { Target, Host, My };
            AnyLanguageIDs = new List<LanguageID>(1) { Any };
            NoneLanguageIDs = new List<LanguageID>(1) { None };

            SetAllLanguageKeys();

            LanguageIDDictionary = new Dictionary<string, LanguageID>(100)
            {
                {"", Any},
                {"(any)", Any},
                {"(all languages)", AllLanguages},
                {"(my languages)", My},
                {"(host languages)", Host},
                {"(target languages)", Target},
                {Arabic.LanguageCultureExtensionCode, Arabic},
                {ArabicVowelled.LanguageCultureExtensionCode, ArabicVowelled},
                {ArabicRomanization.LanguageCultureExtensionCode, ArabicRomanization},
                {Bulgarian.LanguageCultureExtensionCode, Bulgarian},
                {Catalan.LanguageCultureExtensionCode, Catalan},
                {ChineseSimplified.LanguageCultureExtensionCode, ChineseSimplified},
                {ChineseTraditional.LanguageCultureExtensionCode, ChineseTraditional},
                {ChinesePinyin.LanguageCultureExtensionCode, ChinesePinyin},
                {Czech.LanguageCultureExtensionCode, Czech},
                {Danish.LanguageCultureExtensionCode, Danish},
                {Dutch.LanguageCultureExtensionCode, Dutch},
                {EgyptianArabic.LanguageCultureExtensionCode, EgyptianArabic},
                {EgyptianArabicVowelled.LanguageCultureExtensionCode, EgyptianArabicVowelled},
                {EgyptianArabicRomanization.LanguageCultureExtensionCode, EgyptianArabicRomanization},
                {English.LanguageCultureExtensionCode, English},
                {Estonian.LanguageCultureExtensionCode, Estonian},
                {Finnish.LanguageCultureExtensionCode, Finnish},
                {French.LanguageCultureExtensionCode, French},
                {German.LanguageCultureExtensionCode, German},
                {Greek.LanguageCultureExtensionCode, Greek},
                {GreekRomanization.LanguageCultureExtensionCode, GreekRomanization},
                {HaitianCreole.LanguageCultureExtensionCode, HaitianCreole},
                {Hebrew.LanguageCultureExtensionCode, Hebrew},
                {HebrewVowelled.LanguageCultureExtensionCode, HebrewVowelled},
                {HebrewRomanization.LanguageCultureExtensionCode, HebrewRomanization},
                {Hindi.LanguageCultureExtensionCode, Hindi},
                {Hungarian.LanguageCultureExtensionCode, Hungarian},
                {Indonesian.LanguageCultureExtensionCode, Indonesian},
                {Italian.LanguageCultureExtensionCode, Italian},
                {Japanese.LanguageCultureExtensionCode, Japanese},
                {JapaneseKana.LanguageCultureExtensionCode, JapaneseKana},
                {JapaneseRomaji.LanguageCultureExtensionCode, JapaneseRomaji},
                {Korean.LanguageCultureExtensionCode, Korean},
                {KoreanRomanization.LanguageCultureExtensionCode, KoreanRomanization},
                {Latvian.LanguageCultureExtensionCode, Latvian},
                {Lithuanian.LanguageCultureExtensionCode, Lithuanian},
                {Norwegian.LanguageCultureExtensionCode, Norwegian},
                {Polish.LanguageCultureExtensionCode, Polish},
                {Portuguese.LanguageCultureExtensionCode, Portuguese},
                {Romanian.LanguageCultureExtensionCode, Romanian},
                {Russian.LanguageCultureExtensionCode, Russian},
                {Slovak.LanguageCultureExtensionCode, Slovak},
                {Slovenian.LanguageCultureExtensionCode, Slovenian},
                {Spanish.LanguageCultureExtensionCode, Spanish},
                {Swedish.LanguageCultureExtensionCode, Swedish},
                {Tagalog.LanguageCultureExtensionCode, Tagalog},
                {Thai.LanguageCultureExtensionCode, Thai},
                {Turkish.LanguageCultureExtensionCode, Turkish},
                {Ukrainian.LanguageCultureExtensionCode, Ukrainian},
                {Vietnamese.LanguageCultureExtensionCode, Vietnamese}
            };
        }

        public static IDictionary<string, string> LanguageCultureExtensionCodeLanguageDictionary = new Dictionary<string, string>
        {
            {"", "Any Language"},
            {"(any)", "Any Language"},
            {"(all languages)", "All Languages"},
            {"(my languages)", "My Languages"},
            {"(host languages)", "My Host Languages"},
            {"(target languages)", "My Target Languages"},
            {"ar", "Arabic"},
            {"ar--vw", "Arabic-Vowelled"},
            {"ar--rm", "Arabic-Romanization"},
            {"bg", "Bulgarian"},
            {"ca", "Catalan"},
            {"zh", "Chinese"},
            {"zh-CHS", "Chinese-Simplified"},
            {"zh-CHT", "Chinese-Traditional"},
            {"zh--pn", "Chinese-Pinyin"},
            {"cs", "Czech"},
            {"da", "Danish"},
            {"nl", "Dutch"},
            {"eg", "Egyptian-Arabic"},
            {"eg--vw", "Egyptian-Arabic-Vowelled"},
            {"eg--rm", "Egyptian-Arabic-Romanization"},
            {"en", "English"},
            {"et", "Estonian"},
            {"fi", "Finnish"},
            {"fr", "French"},
            {"de", "German"},
            {"el", "Greek"},
            {"el--rm", "Greek-Romanization"},
            {"ht", "Haitian Creole"},
            {"he", "Hebrew"},
            {"he--vw", "Hebrew-Vowelled"},
            {"he--rm", "Hebrew-Romanization"},
            {"hi", "Hindi"},
            {"hu", "Hungarian"},
            {"id", "Indonesian"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"ja--kn", "Japanese-Kana"},
            {"ja--rj", "Japanese-Romaji"},
            {"ko", "Korean"},
            {"ko--rm", "Korean-Romanization"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"no", "Norwegian"},
            {"pl", "Polish"},
            {"pt", "Portuguese"},
            {"ro", "Romanian"},
            {"ru", "Russian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"es", "Spanish"},
            {"sv", "Swedish"},
            {"tl", "Tagalog"},
            {"th", "Thai"},
            {"tr", "Turkish"},
            {"uk", "Ukrainian"},
            {"vi", "Vietnamese"}
        };

        public static IDictionary<string, string> LanguageCultureExtensionCodeDictionary = new Dictionary<string, string>
        {
            {"", "Any Language"},
            {"(any)", "Any Language"},
            {"(all languages)", "All Languages"},
            {"(my languages)", "My Languages"},
            {"(host languages)", "Host Languages"},
            {"(target languages)", "Target Languages"},
            {"ar", "Arabic"},
            {"ar--vw", "Arabic-Vowelled"},
            {"ar--rm", "Arabic-Romanization"},
            {"bg", "Bulgarian"},
            {"ca", "Catalan"},
            {"zh-CHS", "Chinese-Simplified"},
            {"zh-CHT", "Chinese-Traditional"},
            {"zh--pn", "Chinese-Pinyin"},
            {"cs", "Czech"},
            {"da", "Danish"},
            {"nl", "Dutch"},
            {"eg", "Egyptian-Arabic"},
            {"eg--vw", "Egyptian-Arabic-Vowelled"},
            {"eg--rm", "Egyptian-Arabic-Romanization"},
            {"en", "English"},
            {"et", "Estonian"},
            {"fi", "Finnish"},
            {"fr", "French"},
            {"de", "German"},
            {"el", "Greek"},
            {"el--rm", "Greek-Romanization"},
            {"ht", "Haitian Creole"},
            {"he", "Hebrew"},
            {"he--vw", "Hebrew-Vowelled"},
            {"he--rm", "Hebrew-Romanization"},
            {"hi", "Hindi"},
            {"hu", "Hungarian"},
            {"id", "Indonesian"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"ja--kn", "Japanese-Kana"},
            {"ja--rj", "Japanese-Romaji"},
            {"ko", "Korean"},
            {"ko--rm", "Korean-Romanization"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"no", "Norwegian"},
            {"pl", "Polish"},
            {"pt", "Portuguese"},
            {"ro", "Romanian"},
            {"ru", "Russian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"es", "Spanish"},
            {"sv", "Swedish"},
            {"tl", "Tagalog"},
            {"th", "Thai"},
            {"tr", "Turkish"},
            {"uk", "Ukrainian"},
            {"vi", "Vietnamese"}
        };

        public static IDictionary<string, string> LanguageCultureCodeDictionary = new Dictionary<string, string>
        {
            {"", "Any Language"},
            {"(any)", "Any Language"},
            {"(all languages)", "All Languages"},
            {"(my languages)", "My Languages"},
            {"(host languages)", "My Host Languages"},
            {"(target languages)", "My Target Languages"},
            {"ar", "Arabic"},
            {"ar--vw", "Arabic-Vowelled"},
            {"ar--rm", "Arabic-Romanization"},
            {"bg", "Bulgarian"},
            {"ca", "Catalan"},
            {"zh-CHS", "Chinese-Simplified"},
            {"zh-CHT", "Chinese-Traditional"},
            {"zh--pn", "Chinese-Pinyin"},
            {"cs", "Czech"},
            {"da", "Danish"},
            {"nl", "Dutch"},
            {"eg", "Egyptian-Arabic"},
            {"eg--vw", "Egyptian-Arabic-Vowelled"},
            {"eg--rm", "Egyptian-Arabic-Romanization"},
            {"en", "English"},
            {"et", "Estonian"},
            {"fi", "Finnish"},
            {"fr", "French"},
            {"de", "German"},
            {"el", "Greek"},
            {"el--rm", "Greek-Romanization"},
            {"ht", "Haitian Creole"},
            {"he", "Hebrew"},
            {"he--vw", "Hebrew-Vowelled"},
            {"he--rm", "Hebrew-Romanization"},
            {"hi", "Hindi"},
            {"hu", "Hungarian"},
            {"id", "Indonesian"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"ja--kn", "Japanese-Kana"},
            {"ja--rj", "Japanese-Romaji"},
            {"ko", "Korean"},
            {"ko--rm", "Korean-Romanization"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"no", "Norwegian"},
            {"pl", "Polish"},
            {"pt", "Portuguese"},
            {"ro", "Romanian"},
            {"ru", "Russian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"es", "Spanish"},
            {"sv", "Swedish"},
            {"tl", "Tagalog"},
            {"th", "Thai"},
            {"tr", "Turkish"},
            {"uk", "Ukrainian"},
            {"vi", "Vietnamese"}
        };

        public static IDictionary<string, string> LanguageCodeDictionary = new Dictionary<string, string>
        {
            {"", "Any Language"},
            {"(any)", "Any Language"},
            {"(all languages)", "All Languages"},
            {"(my languages)", "My Languages"},
            {"(host languages)", "My Host Languages"},
            {"(target languages)", "My Target Languages"},
            {"ar", "Arabic"},
            {"bg", "Bulgarian"},
            {"ca", "Catalan"},
            {"zh", "Chinese"},
            {"cs", "Czech"},
            {"da", "Danish"},
            {"nl", "Dutch"},
            {"eg", "Egyptian Arabic"},
            {"en", "English"},
            {"et", "Estonian"},
            {"fi", "Finnish"},
            {"fr", "French"},
            {"de", "German"},
            {"el", "Greek"},
            {"ht", "Haitian Creole"},
            {"he", "Hebrew"},
            {"hi", "Hindi"},
            {"hu", "Hungarian"},
            {"id", "Indonesian"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"ko", "Korean"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"no", "Norwegian"},
            {"pl", "Polish"},
            {"pt", "Portuguese"},
            {"ro", "Romanian"},
            {"ru", "Russian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"es", "Spanish"},
            {"sv", "Swedish"},
            {"tl", "Tagalog"},
            {"th", "Thai"},
            {"tr", "Turkish"},
            {"uk", "Ukrainian"},
            {"vi", "Vietnamese"}
        };

        public static string ConvertLanguageCultureExtensionCodeToLanguage(string languageCultureExtensionCode)
        {
            string returnValue = "";
            if (languageCultureExtensionCode == null)
                return returnValue;
            bool result = LanguageCultureExtensionCodeLanguageDictionary.TryGetValue(languageCultureExtensionCode, out returnValue);
            if (!result)
                returnValue = languageCultureExtensionCode;
            return returnValue;
        }

        public static string ConvertLanguageCultureExtensionCodeToLanguageAndCulture(string languageCultureExtensionCode)
        {
            string returnValue = "";
            if (String.IsNullOrEmpty(languageCultureExtensionCode))
                return returnValue;
            bool result = LanguageCultureExtensionCodeDictionary.TryGetValue(languageCultureExtensionCode, out returnValue);
            if (!result)
                returnValue = languageCultureExtensionCode;
            return returnValue;
        }

        public static string ConvertLanguageCultureCodeToLanguageAndCulture(string languageCultureCode)
        {
            string returnValue = "";
            if (String.IsNullOrEmpty(languageCultureCode))
                return returnValue;
            bool result = LanguageCultureCodeDictionary.TryGetValue(languageCultureCode, out returnValue);
            if (!result)
                returnValue = languageCultureCode;
            return returnValue;
        }

        public static string ConvertLanguageCodeToLanguage(string languageCode)
        {
            string returnValue = "";
            if (String.IsNullOrEmpty(languageCode))
                return returnValue;
            bool result = LanguageCodeDictionary.TryGetValue(languageCode, out returnValue);
            if (!result)
                returnValue = languageCode;
            return returnValue;
        }

        public static LanguageID GetLanguageID(string languageCultureExtensionCode)
        {
            if (String.IsNullOrEmpty(languageCultureExtensionCode) || (languageCultureExtensionCode == "any") || (languageCultureExtensionCode == "(any)"))
                return Any;
            else if (String.IsNullOrEmpty(languageCultureExtensionCode) || (languageCultureExtensionCode == "(all languages)"))
                return AllLanguages;
            LanguageID languageID = null;
            if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
            {
                TryAddLanguage(languageCultureExtensionCode, languageCultureExtensionCode);
                LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID);
            }
            return languageID;
        }

        public static LanguageID GetLanguageIDNoAdd(string languageCultureExtensionCode)
        {
            LanguageID languageID = null;
            if (String.IsNullOrEmpty(languageCultureExtensionCode))
                return Any;
            if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
                languageID = new LanguageID(languageCultureExtensionCode);
            return languageID;
        }

        public static LanguageID GetLanguageIDCheck(string languageCultureExtensionCode)
        {
            LanguageID languageID = null;
            if (String.IsNullOrEmpty(languageCultureExtensionCode))
                return Any;
            if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
                languageID = null;
            return languageID;
        }

        public static LanguageID GetCanonicalLanguageID(string languageCultureExtensionCode)
        {
            LanguageID languageID = null;
            if (String.IsNullOrEmpty(languageCultureExtensionCode))
                return Any;
            if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
            {
                languageID = new LanguageID(languageCultureExtensionCode);
                switch (languageID.CultureCode)
                {
                    case "CN":
                        languageID.CultureCode = "CHS";
                        break;
                    case "TW":
                    case "HK":
                        languageID.CultureCode = "CHT";
                        break;
                    default:
                        if (languageID.LanguageCode == "zh")
                            languageID.CultureCode = "CHT";
                        else
                            languageID.CultureCode = null;
                        break;
                }
                languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
                    TryAddLanguage(languageCultureExtensionCode, languageCultureExtensionCode);
                LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID);
            }
            return languageID;
        }

        public static string GetLanguageName(string languageCultureExtensionCode, string displayLanguageCultureExtensionCode)
        {
            string str;

#if true
            LanguageDescription languageDescription = GetLanguageDescriptionFromCode(languageCultureExtensionCode);

            if ((languageDescription != null) &&
                (languageDescription.LanguageName != null) &&
                (languageDescription.LanguageName.HasText(displayLanguageCultureExtensionCode)))
            {
                str = languageDescription.LanguageName.Text(displayLanguageCultureExtensionCode);
            }
            else
            {
                str = LanguageLookup.ConvertLanguageCultureExtensionCodeToLanguage(languageCultureExtensionCode);

                if ((displayLanguageCultureExtensionCode != "en") && (LanguageUtilities != null))
                    str = LanguageUtilities.TranslateString(str, GetLanguageIDNoAdd(displayLanguageCultureExtensionCode));
            }
#else
            str = LanguageLookup.ConvertLanguageCultureExtensionCodeToLanguage(languageCultureExtensionCode);

            if ((displayLanguageCultureExtensionCode != "en") && (LanguageUtilities != null))
                str = LanguageUtilities.TranslateString(str, GetLanguageIDNoAdd(displayLanguageCultureExtensionCode));
#endif

            return str;
        }

        public static string GetLanguageName(string languageCultureExtensionCode, LanguageID displayLanguageID)
        {
            string str = GetLanguageName(languageCultureExtensionCode, displayLanguageID.LanguageCultureExtensionCode);
            return str;
        }

        public static List<string> GetLanguageNames()
        {
            List<string> names = new List<string>(LanguageCultureExtensionCodeLanguageDictionary.Count());
            foreach (KeyValuePair<string, string> kvp in LanguageCultureExtensionCodeLanguageDictionary)
                names.Add(kvp.Value);
            return names;
        }

        public static List<string> GetBaseLanguageNames()
        {
            List<string> names = new List<string>(LanguageIDs.Count());

            foreach (LanguageID languageID in LanguageIDs)
            {
                string language = languageID.Language;

                if (names.FirstOrDefault(x => x == language) == null)
                    names.Add(language);
            }

            return names;
        }

        public static List<LanguageID> GetBaseLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(LanguageIDs.Count());

            foreach (LanguageID languageID in LanguageIDs)
            {
                LanguageID baseLanguageID;

                if (!String.IsNullOrEmpty(languageID.CultureCode) || !String.IsNullOrEmpty(languageID.ExtensionCode))
                    baseLanguageID = new LanguageID(languageID.LanguageCode);
                else
                    baseLanguageID = languageID;

                if (languageIDs.FirstOrDefault(x => x == baseLanguageID) == null)
                    languageIDs.Add(baseLanguageID);
            }

            return languageIDs;
        }

        public static LanguageID GetMediaLanguageID(LanguageID languageID)
        {
            if ((languageID == null) || (String.IsNullOrEmpty(languageID.CultureCode) && String.IsNullOrEmpty(languageID.ExtensionCode)))
                return languageID;

            return GetLanguageIDNoAdd(languageID.LanguageCode);
        }

        public static List<LanguageID> GetMediaLanguageIDs(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return new List<LanguageID>();

            List<LanguageID> returnLanguageIDs = new List<LanguageID>(languageIDs.Count());

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageID mediaLanguageID = GetMediaLanguageID(languageID);

                if (!returnLanguageIDs.Contains(mediaLanguageID))
                    returnLanguageIDs.Add(mediaLanguageID);
            }

            return returnLanguageIDs;
        }

        public static List<LanguageID> GetMediaLanguageIDsFromDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            List<LanguageID> languageIDs = new List<LanguageID>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null))
                    continue;

                LanguageID mediaLanguageID = GetMediaLanguageID(languageDescriptor.LanguageID);

                if (!languageIDs.Contains(mediaLanguageID))
                    languageIDs.Add(mediaLanguageID);
            }

            return languageIDs;
        }

        public static void GetNonSpecialLanguageIDs(List<LanguageID> destArray, List<LanguageID> srcArray)
        {
            if (srcArray == null)
                return;

            foreach (LanguageID languageID in srcArray)
            {
                if (languageID == null)
                    continue;
                else if (languageID.IsLanguage())
                    destArray.Add(languageID);
            }
        }

        public static List<LanguageID> GetNonSpecialLanguageIDs(List<LanguageID> srcArray)
        {
            List<LanguageID> destArray = new List<LanguageID>();
            GetNonSpecialLanguageIDs(destArray, srcArray);
            return destArray;
        }

        public static List<LanguageID> NonSpecialLanguageIDs
        {
            get
            {
                return GetNonSpecialLanguageIDs(LanguageIDs);
            }
        }

        public static List<LanguageID> ExpandLanguageIDs(List<LanguageID> languageIDs, UserProfile userProfile)
        {
            if (languageIDs == null)
                return null;

            List<LanguageID> newLanguageIDList = new List<LanguageID>();

            foreach (LanguageID languageID in languageIDs)
            {
                switch (languageID.LanguageCode)
                {
                    case "(target languages)":
                        if (userProfile != null)
                            ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.TargetLanguageIDs);
                        break;
                    case "(host languages)":
                        if (userProfile != null)
                            ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.HostLanguageIDs);
                        break;
                    case "(my languages)":
                        if (userProfile != null)
                            ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.LanguageIDs);
                        break;
                    case "(any)":
                    case "(all languages)":
                        GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                        break;
                    case "(none)":
                        break;
                    default:
                        ObjectUtilities.ListAddUnique(newLanguageIDList, languageID);
                        break;
                }
            }

            return newLanguageIDList;
        }

        public static List<LanguageID> ExpandLanguageID(LanguageID languageID, UserProfile userProfile)
        {
            List<LanguageID> newLanguageIDList = new List<LanguageID>();

            if (languageID == null)
            {
                GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                return newLanguageIDList;
            }

            switch (languageID.LanguageCode)
            {
                case "(target languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.TargetLanguageIDs);
                    break;
                case "(host languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.HostLanguageIDs);
                    break;
                case "(my languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.LanguageIDs);
                    break;
                case "(any)":
                case "(all languages)":
                    GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                    break;
                case "(none)":
                    break;
                default:
                    ObjectUtilities.ListAddUnique(newLanguageIDList, languageID);
                    break;
            }

            return newLanguageIDList;
        }

        public static List<LanguageID> ExpandLanguageCode(string languageCode, UserProfile userProfile)
        {
            List<LanguageID> newLanguageIDList = new List<LanguageID>();

            if (String.IsNullOrEmpty(languageCode))
            {
                GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                return newLanguageIDList;
            }

            switch (languageCode)
            {
                case "(target languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.TargetLanguageIDs);
                    break;
                case "(host languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.HostLanguageIDs);
                    break;
                case "(my languages)":
                    if (userProfile != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, userProfile.LanguageIDs);
                    break;
                case "(any)":
                case "(all languages)":
                    GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                    break;
                case "(none)":
                    break;
                default:
                    ObjectUtilities.ListAddUnique(newLanguageIDList, GetLanguageIDNoAdd(languageCode));
                    break;
            }

            return newLanguageIDList;
        }

        public static List<LanguageID> ExpandLanguageCodeMedia(string languageCode, UserProfile userProfile)
        {
            List<LanguageID> newLanguageIDList = ExpandLanguageCode(languageCode, userProfile);

            newLanguageIDList = LanguageID.GetMediaLanguageIDs(newLanguageIDList);

            return newLanguageIDList;
        }

        public static List<LanguageID> ExpandLanguageID(
            LanguageID languageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            List<LanguageID> newLanguageIDList = new List<LanguageID>();

            if (languageID == null)
                return newLanguageIDList;

            switch (languageID.LanguageCode)
            {
                case "(target languages)":
                    if (targetLanguageIDs != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, targetLanguageIDs);
                    break;
                case "(host languages)":
                    if (hostLanguageIDs != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, hostLanguageIDs);
                    break;
                case "(my languages)":
                    if (targetLanguageIDs != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, targetLanguageIDs);
                    if (hostLanguageIDs != null)
                        ObjectUtilities.ListAddUniqueList(newLanguageIDList, hostLanguageIDs);
                    break;
                case "(any)":
                case "(all languages)":
                    GetNonSpecialLanguageIDs(newLanguageIDList, LanguageIDs);
                    break;
                case "(none)":
                    break;
                default:
                    ObjectUtilities.ListAddUnique(newLanguageIDList, languageID);
                    break;
            }

            return newLanguageIDList;
        }

        public static List<LanguageID> ExpandMediaLanguageCode(string mediaLanguageCode, List<LanguageID> languageIDs)
        {
            List<LanguageID> returnValue = new List<LanguageID>();

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    if (languageID.MediaLanguageCode() == mediaLanguageCode)
                        returnValue.Add(languageID);
                }
            }

            return returnValue;
        }

        public static bool IsAnyAllLanguages(List<LanguageID> languageIDs)
        {
            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    if (languageID.LanguageCode == "(all languages)")
                        return true;
                }
            }

            return false;
        }

        private static Dictionary<string, IDictionary<string, string>> CachedSortedLanguageCultureExtensionCodeDictionaries =
            new Dictionary<string, IDictionary<string, string>>();

        private static void ClearCache()
        {
            CachedSortedLanguageCultureExtensionCodeDictionaries.Clear();
        }

        public static IDictionary<string, string> GetSortedLanguageCultureExtensionCodeDictionary(
            string uiLanguageCode,
            LanguageUtilities languageUtilities)
        {
            if (uiLanguageCode == "en")
                return LanguageCultureExtensionCodeDictionary;

            IDictionary<string, string> dictionary;

            lock (CachedSortedLanguageCultureExtensionCodeDictionaries)
            {
                if (!CachedSortedLanguageCultureExtensionCodeDictionaries.TryGetValue(uiLanguageCode, out dictionary))
                {
                    dictionary = new Dictionary<string, string>();
                    List<KeyValuePair<string, string>> list = LanguageCultureExtensionCodeDictionary.ToList();
                    int count = list.Count();
                    int index;
                    for (index = 0; index < count; index++)
                    {
                        KeyValuePair<string, string> kvp = list[index];
                        string languageName = languageUtilities.TranslateUIString(kvp.Value);
                        KeyValuePair<string, string> newKvp = new KeyValuePair<string, string>(kvp.Key, languageName);
                        list[index] = newKvp;
                    }
                    list.Sort(CompareKVPs);
                    foreach (KeyValuePair<string, string> kvp1 in list)
                        dictionary.Add(kvp1.Key, kvp1.Value);
                    CachedSortedLanguageCultureExtensionCodeDictionaries.Add(uiLanguageCode, dictionary);
                }
            }

            return dictionary;
        }

        public static void AddLanguageCodeNeedingFixup(string languageCultureExtensionCode, string languageCultureExtension)
        {
            if (languageCultureExtension != languageCultureExtensionCode)
                return;

            if (LanguageCodesNeedingFixups == null)
                LanguageCodesNeedingFixups = new List<string>(1) { languageCultureExtensionCode };
            else if (!LanguageCodesNeedingFixups.Contains(languageCultureExtensionCode))
                LanguageCodesNeedingFixups.Add(languageCultureExtensionCode);
        }

        public static void FixupLanguages(IMainRepository repositories, ILanguageTranslator translator)
        {
            List<string> languageCodesNeedingFixups = LanguageCodesNeedingFixups;
            LanguageCodesNeedingFixups = null;

            if (languageCodesNeedingFixups == null)
                return;

            int index;

            for (index = 0; index < languageCodesNeedingFixups.Count(); index++)
            {
                string languageCultureExtensionCode = languageCodesNeedingFixups[index];
                LanguageDescription languageDescription = repositories.LanguageDescriptions.Get(languageCultureExtensionCode);

                if (languageDescription != null)
                {
                    string languageNameEnglish = languageDescription.LanguageName.Text(English);

                    if (String.IsNullOrEmpty(languageNameEnglish))
                    {
                        LanguageString foundLanguageString = null;

                        foreach (LanguageString languageString in languageDescription.LanguageName.LanguageStrings)
                        {
                            if (!String.IsNullOrEmpty(languageString.Text))
                            {
                                LanguageTranslatorSource translatorSource;
                                string errorMessage;

                                if (translator.TranslateString(
                                    "SynchronizeLanguagesTranslation",
                                    "LanguageDescriptions",
                                    languageCultureExtensionCode,
                                    languageString.Text,
                                    languageString.LanguageID,
                                    English,
                                    out languageNameEnglish,
                                    out translatorSource,
                                    out errorMessage))
                                {
                                    foundLanguageString = languageString;
                                    break;
                                }
                            }
                        }

                        if (foundLanguageString != null)
                        {
                            LanguageString englishLS = languageDescription.LanguageName.LanguageString(English);

                            if (englishLS == null)
                                languageDescription.LanguageName.Add(new LanguageString(languageDescription.LanguageName.Key, English,
                                    languageNameEnglish));
                            else

                                englishLS.Text = languageNameEnglish;
                        }
                    }

                    if (!String.IsNullOrEmpty(languageNameEnglish))
                    {
                        LanguageCultureExtensionCodeLanguageDictionary.Remove(languageCultureExtensionCode);
                        LanguageCultureExtensionCodeLanguageDictionary.Add(languageCultureExtensionCode, languageNameEnglish);

                        LanguageCultureExtensionCodeDictionary.Remove(languageCultureExtensionCode);
                        LanguageCultureExtensionCodeDictionary.Add(languageCultureExtensionCode, languageNameEnglish);

                        LanguageCultureCodeDictionary.Remove(languageCultureExtensionCode);
                        LanguageCultureCodeDictionary.Add(languageCultureExtensionCode, languageNameEnglish);

                        if (!languageCultureExtensionCode.Contains("-"))
                        {
                            LanguageCodeDictionary.Remove(languageCultureExtensionCode);
                            LanguageCodeDictionary.Add(languageCultureExtensionCode, languageNameEnglish);
                        }
                    }
                }
            }

            Sort();
            ClearCache();
        }

        public static void TryAddLanguage(string languageCultureExtensionCode, string languageCultureExtension)
        {
            string returnValue = "";

            if ((languageCultureExtensionCode == "zh-CHS") || (languageCultureExtensionCode == "zh-CHT"))
                return;

            LanguageID languageID = new LanguageID(languageCultureExtensionCode);
            LanguageID tmpLanguageID;
            string languageCode = languageID.LanguageCode;

            if (languageCode == null)
                languageCode = "";

            if (!LanguageIDs.Contains(languageID))
            {
                LanguageIDs.Add(languageID);
                LanguageIDs.Sort(LanguageID.CompareNames);
            }

            if (!LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out tmpLanguageID))
                LanguageIDDictionary.Add(languageCultureExtensionCode, languageID);

            if (!LanguageCultureExtensionCodeLanguageDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
                LanguageCultureExtensionCodeLanguageDictionary.Add(languageCultureExtensionCode, languageCultureExtension);

            if (!LanguageCultureExtensionCodeDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
            {
                LanguageCultureExtensionCodeDictionary.Add(languageCultureExtensionCode, languageCultureExtension);
                AddLanguageCodeNeedingFixup(languageCultureExtensionCode, languageCultureExtension);
            }

            if (!LanguageCultureCodeDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
                LanguageCultureCodeDictionary.Add(languageCultureExtensionCode, languageCultureExtension);

            if (!LanguageCodeDictionary.TryGetValue(languageCode, out returnValue))
                LanguageCodeDictionary.Add(languageCode, languageID.Language);

            ClearCache();
        }

        public static void TryDeleteLanguage(string languageCultureExtensionCode)
        {
            if (String.IsNullOrEmpty(languageCultureExtensionCode))
                return;

            string returnValue;
            LanguageID languageID = new LanguageID(languageCultureExtensionCode);
            string languageCode = languageID.LanguageCode;

            if (languageCode == null)
                languageCode = "";

            if (LanguageIDs.Contains(languageID))
                LanguageIDs.Remove(languageID);

            if (LanguageIDDictionary.TryGetValue(languageCultureExtensionCode, out languageID))
            {
                LanguageIDs.Remove(languageID);
                LanguageIDDictionary.Remove(languageCultureExtensionCode);
            }

            if (LanguageCultureExtensionCodeLanguageDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
                LanguageCultureExtensionCodeLanguageDictionary.Remove(languageCultureExtensionCode);

            if (LanguageCultureExtensionCodeDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
                LanguageCultureExtensionCodeDictionary.Remove(languageCultureExtensionCode);

            if (LanguageCultureCodeDictionary.TryGetValue(languageCultureExtensionCode, out returnValue))
                LanguageCultureCodeDictionary.Remove(languageCultureExtensionCode);

            if (LanguageCodeDictionary.TryGetValue(languageCode, out returnValue))
                LanguageCodeDictionary.Remove(languageCode);

            ClearCache();
        }

        public static void SynchronizeDictionaryEntry(IDictionary<string, string> dictionary, string languageCode, string languageName)
        {
            string currentName;

            if (dictionary.TryGetValue(languageCode, out currentName))
            {
                if (currentName != languageName)
                {
                    dictionary.Remove(languageCode);
                    dictionary.Add(languageCode, languageName);
                }
            }
            else
                dictionary.Add(languageCode, languageName);
        }

        /*
        static LanguageID CompareLanguageID;
        static int CompareLanguageDescriptionName(LanguageDescription x, LanguageDescription y)
        {
            string xs = x.LanguageName.TextFuzzy(CompareLanguageID);
            string ys = y.LanguageName.TextFuzzy(CompareLanguageID);
            return String.Compare(xs, ys);
        }
        */

        private static Dictionary<string, LanguageDescription> _LanguageDescriptionCache;
        public static Dictionary<string, LanguageDescription> LanguageDescriptionCache
        {
            get
            {
                if (_LanguageDescriptionCache == null)
                    _LanguageDescriptionCache = new Dictionary<string, LanguageDescription>();
                return _LanguageDescriptionCache;
            }
            set
            {
                _LanguageDescriptionCache = value;
            }
        }

        public static bool IsKnownLanguageCode(string languageCultureExtensionCode)
        {
            if (_LanguageDescriptionCache != null)
            {
                LanguageDescription languageDescription;

                if (LanguageDescriptionCache.TryGetValue(languageCultureExtensionCode, out languageDescription))
                    return true;
            }

            return false;
        }

        public static LanguageDescription GetLanguageDescription(LanguageID languageID)
        {
            if (languageID == null)
                return null;
            return GetLanguageDescriptionFromCode(languageID.LanguageCultureExtensionCode);
        }

        public static LanguageDescription GetLanguageDescriptionFromCode(string languageCultureExtensionCode)
        {
            Dictionary<string, LanguageDescription> languageDescriptionCache;
            lock (languageDescriptionCache = LanguageDescriptionCache)
            {
                LanguageDescription languageDescription;
                if (LanguageDescriptionCache.TryGetValue(languageCultureExtensionCode, out languageDescription))
                    return languageDescription;
                languageDescription = ApplicationData.Repositories.LanguageDescriptions.Get(
                    languageCultureExtensionCode);
                if (languageDescription != null)
                    LanguageDescriptionCache.Add(languageCultureExtensionCode, languageDescription);
                return languageDescription;
            }
        }

        public static List<LanguageDescription> QueryLanguageDescriptions(LanguageIDMatcher matcher)
        {
            Dictionary<string, LanguageDescription> languageDescriptionCache;
            lock (languageDescriptionCache = LanguageDescriptionCache)
            {
                List<LanguageDescription> languageDescriptions = null;

                if (languageDescriptionCache.Count() == 0)
                {
                    languageDescriptions = ApplicationData.Repositories.LanguageDescriptions.GetAll();
                    if (languageDescriptions != null)
                    {
                        foreach (LanguageDescription languageDescription in languageDescriptions)
                        {
                            LanguageID languageID = languageDescription.LanguageID;
                            string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                            languageDescriptionCache.Add(languageCultureExtensionCode, languageDescription);
                        }
                    }
                }

                languageDescriptions = new List<LanguageDescription>();

                foreach (KeyValuePair<string, LanguageDescription> kvp in languageDescriptionCache)
                {
                    if (matcher.Match(kvp.Value.LanguageID))
                        languageDescriptions.Add(kvp.Value);
                }

                return languageDescriptions;
            }
        }

        public static List<LanguageDescription> GetLanguageDescriptions()
        {
            Dictionary<string, LanguageDescription> languageDescriptionCache;
            lock (languageDescriptionCache = LanguageDescriptionCache)
            {
                List<LanguageDescription> languageDescriptions;
                if (languageDescriptionCache.Count() == 0)
                {
                    languageDescriptions = ApplicationData.Repositories.LanguageDescriptions.GetAll();
                    if (languageDescriptions != null)
                    {
                        foreach (LanguageDescription languageDescription in languageDescriptions)
                        {
                            LanguageID languageID = languageDescription.LanguageID;
                            string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                            languageDescriptionCache.Add(languageCultureExtensionCode, languageDescription);
                        }
                    }
                }
                else
                {
                    languageDescriptions = new List<LanguageDescription>();
                    foreach (KeyValuePair<string, LanguageDescription> kvp in languageDescriptionCache)
                        languageDescriptions.Add(kvp.Value);
                }
                return languageDescriptions;
            }
        }

        public static bool AddLanguageDescription(LanguageDescription languageDescription)
        {
            lock (LanguageDescriptionCache)
            {
                LanguageID languageID = languageDescription.LanguageID;
                string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                LanguageDescription testLanguageDescription;
                if (!LanguageDescriptionCache.TryGetValue(languageCultureExtensionCode, out testLanguageDescription))
                    LanguageDescriptionCache.Add(languageCultureExtensionCode, languageDescription);
                else if (languageDescription != testLanguageDescription)
                    LanguageDescriptionCache[languageCultureExtensionCode] = languageDescription;
                bool returnValue = ApplicationData.Repositories.LanguageDescriptions.Add(languageDescription);
                ApplicationData.Repositories.LanguageDescriptions.TouchAndClearModified();
                return returnValue;
            }
        }

        public static bool UpdateLanguageDescription(LanguageDescription languageDescription)
        {
            lock (LanguageDescriptionCache)
            {
                LanguageID languageID = languageDescription.LanguageID;
                string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                LanguageDescription testLanguageDescription;
                if (!LanguageDescriptionCache.TryGetValue(languageCultureExtensionCode, out testLanguageDescription))
                    LanguageDescriptionCache.Add(languageCultureExtensionCode, languageDescription);
                else if (languageDescription != testLanguageDescription)
                    LanguageDescriptionCache[languageCultureExtensionCode] = languageDescription;
                bool returnValue = ApplicationData.Repositories.LanguageDescriptions.Update(languageDescription);
                ApplicationData.Repositories.LanguageDescriptions.TouchAndClearModified();
                return returnValue;
            }
        }

        public static bool DeleteLanguageDescription(LanguageDescription languageDescription)
        {
            lock (LanguageDescriptionCache)
            {
                LanguageID languageID = languageDescription.LanguageID;
                string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
                LanguageDescription testLanguageDescription;
                if (LanguageDescriptionCache.TryGetValue(languageCultureExtensionCode, out testLanguageDescription))
                    LanguageDescriptionCache.Remove(languageCultureExtensionCode);
                bool returnValue = ApplicationData.Repositories.LanguageDescriptions.Delete(languageDescription);
                ApplicationData.Repositories.LanguageDescriptions.TouchAndClearModified();
                return returnValue;
            }
        }

        public static void SynchronizeToLanguageDescriptions()
        {
            LanguageDescriptionCache = null;
            List<LanguageDescription> languageDescriptions = GetLanguageDescriptions();
            SynchronizeToLanguageDescriptions(languageDescriptions);
        }

        public static void SynchronizeToLanguageDescriptions(List<LanguageDescription> languageDescriptions)
        {
            foreach (LanguageDescription languageDescription in languageDescriptions)
            {
                string languageCode = languageDescription.LanguageID.LanguageCultureExtensionCode;
                string languageName = languageDescription.LanguageName.TextFuzzy(English);

                if (!LanguageIDs.Contains(languageDescription.LanguageID))
                    LanguageIDs.Add(languageDescription.LanguageID);

                LanguageID tmpLanguageID;
                if (!LanguageIDDictionary.TryGetValue(languageCode, out tmpLanguageID))
                    LanguageIDDictionary.Add(languageCode, languageDescription.LanguageID);

                SynchronizeDictionaryEntry(LanguageCultureExtensionCodeLanguageDictionary, languageCode, languageName);
                SynchronizeDictionaryEntry(LanguageCultureExtensionCodeDictionary, languageCode, languageName);
                SynchronizeDictionaryEntry(LanguageCultureCodeDictionary, languageCode, languageName);
                SynchronizeDictionaryEntry(LanguageCodeDictionary, languageCode, languageName);
            }

            LanguageIDs.Sort(LanguageID.CompareNames);

            Sort();

            ClearCache();

            LanguagesInitialized = true;
        }

        private static int CompareKVPs(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
        {
            int returnValue = String.Compare(x.Value, y.Value);
            return returnValue;
        }

        public static void Sort()
        {
            List<KeyValuePair<string, string>> list = LanguageCultureExtensionCodeLanguageDictionary.ToList();
            list.Sort(CompareKVPs);
            LanguageCultureExtensionCodeLanguageDictionary.Clear();
            foreach (KeyValuePair<string, string> kvp1 in list)
                LanguageCultureExtensionCodeLanguageDictionary.Add(kvp1.Key, kvp1.Value);

            list = LanguageCultureExtensionCodeDictionary.ToList();
            list.Sort(CompareKVPs);
            LanguageCultureExtensionCodeDictionary.Clear();
            foreach (KeyValuePair<string, string> kvp2 in list)
                LanguageCultureExtensionCodeDictionary.Add(kvp2.Key, kvp2.Value);

            list = LanguageCultureCodeDictionary.ToList();
            list.Sort(CompareKVPs);
            LanguageCultureCodeDictionary.Clear();
            foreach (KeyValuePair<string, string> kvp3 in list)
                LanguageCultureCodeDictionary.Add(kvp3.Key, kvp3.Value);

            list = LanguageCodeDictionary.ToList();
            list.Sort(CompareKVPs);
            LanguageCodeDictionary.Clear();
            foreach (KeyValuePair<string, string> kvp4 in list)
                LanguageCodeDictionary.Add(kvp4.Key, kvp4.Value);

            SetAllLanguageKeys();
        }

        public static void SetAllLanguageKeys()
        {
            _AllLanguageKeys = LanguageID.ConvertLanguageIDListToString(NonSpecialLanguageIDs);
        }

        public static List<LanguageID> GetAlternateLanguageIDs(LanguageID languageID)
        {
            List<LanguageID> languageIDs = null;

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                    languageIDs = new List<LanguageID>(2) { ChineseTraditional, ChinesePinyin };
                    break;
                case "zh-CHT":
                    languageIDs = new List<LanguageID>(2) { ChineseSimplified, ChinesePinyin };
                    break;
                case "zh--pn":
                    languageIDs = new List<LanguageID>(2) { ChineseSimplified, ChineseTraditional };
                    break;
                case "zh":
                    languageIDs = new List<LanguageID>(2) { ChineseSimplified, ChineseTraditional, ChinesePinyin };
                    break;
                case "ko":
                    languageIDs = new List<LanguageID>(1) { KoreanRomanization };
                    break;
                case "ko--rm":
                    languageIDs = new List<LanguageID>(1) { Korean };
                    break;
                case "ja":
                    languageIDs = new List<LanguageID>(2) { JapaneseKana, JapaneseRomaji };
                    break;
                case "ja--kn":
                    languageIDs = new List<LanguageID>(2) { Japanese, JapaneseRomaji };
                    break;
                case "ja--rj":
                    languageIDs = new List<LanguageID>(2) { Japanese, JapaneseKana };
                    break;
                case "ar":
                    languageIDs = new List<LanguageID>(2) { ArabicVowelled, ArabicRomanization };
                    break;
                case "ar--vw":
                    languageIDs = new List<LanguageID>(2) { Arabic, ArabicRomanization };
                    break;
                case "ar--rm":
                    languageIDs = new List<LanguageID>(2) { Arabic, ArabicVowelled };
                    break;
                case "eg":
                    languageIDs = new List<LanguageID>(2) { EgyptianArabicVowelled, EgyptianArabicRomanization };
                    break;
                case "eg--vw":
                    languageIDs = new List<LanguageID>(2) { EgyptianArabic, EgyptianArabicRomanization };
                    break;
                case "eg--rm":
                    languageIDs = new List<LanguageID>(2) { EgyptianArabic, EgyptianArabicVowelled };
                    break;
                case "he":
                    languageIDs = new List<LanguageID>(2) { HebrewVowelled, HebrewRomanization };
                    break;
                case "he--vw":
                    languageIDs = new List<LanguageID>(2) { Hebrew, HebrewRomanization };
                    break;
                case "he--rm":
                    languageIDs = new List<LanguageID>(2) { Hebrew, HebrewVowelled };
                    break;
                case "el":
                    languageIDs = new List<LanguageID>(2) { GreekRomanization };
                    break;
                case "el--rm":
                    languageIDs = new List<LanguageID>(2) { Greek };
                    break;
                default:
                    break;
            }

            return languageIDs;
        }

        public static List<LanguageID> GetAlternateLanguageIDs(
            LanguageID targetLanguageID,
            List<LanguageID> sourceLanguageIDs)
        {
            List<LanguageID> languageIDs = null;

            foreach (LanguageID languageID in sourceLanguageIDs)
            {
                if (languageID == targetLanguageID)
                    continue;

                if (!IsAlternateOfLanguageID(languageID, targetLanguageID))
                    continue;

                if (languageIDs == null)
                    languageIDs = new List<LanguageID>() { languageID };
                else
                    languageIDs.Add(languageID);
            }

            return languageIDs;
        }

        public static List<LanguageID> GetLanguagePlusAlternateLanguageIDs(LanguageID languageID)
        {
            if (languageID == null)
                return new List<LanguageID>();

            List<LanguageID> alternates = GetAlternateLanguageIDs(languageID);
            int count = 1 + (alternates != null ? alternates.Count() : 0);
            List<LanguageID> languageIDs = new List<LanguageID>(count) { languageID };

            if (alternates != null)
                languageIDs.AddRange(alternates);

            return languageIDs;
        }

        public static List<LanguageID> GetLanguagesPlusAlternateLanguageIDs(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            List<LanguageID> allLanguageIDs = new List<LanguageID>();

            foreach (LanguageID languageID in languageIDs)
            {
                List<LanguageID> familyLanguageIDs = GetLanguagePlusAlternateLanguageIDs(languageID);
                allLanguageIDs.AddRange(familyLanguageIDs);
            }

            return allLanguageIDs;
        }

        public static bool IsMultiTransliterationLanguageID(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.LanguageCode)
            {
                case "zh":
                case "ko":
                case "ja":
                case "ar":
                case "eg":
                case "he":
                case "el":
                    return true;
                default:
                    // When LanguageDescription has a HasMultipleTransliterations member,
                    // we'll need to change this to use it. Otherwise, assume that if there
                    // is a culture code like Chinese, use it as a flag, as we don't normaly
                    // use language IDs with culture codes.
                    if (!String.IsNullOrEmpty(languageID.CultureCode))
                        return true;
                    break;
            }

            return false;
        }

        public static bool IsAlternatePhonetic(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.ExtensionCode)
            {
                case "pn":
                case "rm":
                case "kn":
                case "rj":
                case "vw":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static LanguageID GetPreferedMediaLanguageID(LanguageID languageID)
        {
            if (languageID == null)
                return languageID;

            switch (languageID.ExtensionCode)
            {
                case "pn":
                    languageID = ChineseSimplified;
                    break;
                case "rm":
                    languageID = GetLanguageIDNoAdd(languageID.LanguageCode);
                    break;
                case "rj":
                    languageID = JapaneseKana;
                    break;
                default:
                    switch (languageID.LanguageCultureExtensionCode)
                    {
                        case "ja":
                            languageID = JapaneseKana;
                            break;
                        case "ar":
                            languageID = ArabicVowelled;
                            break;
                        case "eg":
                            languageID = EgyptianArabicVowelled;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            return languageID;
        }

        public static List<LanguageID> GetRoots(List<LanguageID> languageIDs)
        {
            List<LanguageID> rootLanguageIDs = new List<LanguageID>();

            if (languageIDs == null)
                return null;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageID rootLanguageID = GetRoot(languageID);

                if (!rootLanguageIDs.Contains(rootLanguageID))
                    rootLanguageIDs.Add(rootLanguageID);
            }

            return rootLanguageIDs;
        }

        public static LanguageID GetRoot(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            foreach (LanguageID languageID in languageIDs)
            {
                if (IsRoot(languageID))
                    return languageID;
            }

            return null;
        }

        public static LanguageID GetRoot(LanguageID languageID)
        {
            if (languageID == null)
                return null;

            if (IsRoot(languageID))
                return languageID;

            LanguageID rootLanguageID = GetLanguageIDNoAdd(languageID.LanguageCultureCode);

            return rootLanguageID;
        }

        public static bool IsRoot(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            if (String.IsNullOrEmpty(languageID.CultureCode) && String.IsNullOrEmpty(languageID.ExtensionCode))
                return true;

            return false;
        }

        public static LanguageID GetRomanized(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            foreach (LanguageID languageID in languageIDs)
            {
                if (IsRomanized(languageID))
                    return languageID;
            }

            return null;
        }

        public static bool IsRomanized(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.ExtensionCode)
            {
                case "pn":
                case "rm":
                case "rj":
                case "vw":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static LanguageID GetNonRomanizedPhonetic(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            foreach (LanguageID languageID in languageIDs)
            {
                if (IsNonRomanizedAlternatePhonetic(languageID))
                    return languageID;
            }

            return null;
        }

        public static bool IsNonRomanizedAlternatePhonetic(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.ExtensionCode)
            {
                case "kn":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static bool IsVowelled(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.ExtensionCode)
            {
                case "vw":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static List<LanguageID> GetAlternatePhoneticLanguageIDs(LanguageID phoneticLanguageID)
        {
            List<LanguageID> list = new List<LanguageID>();

            switch (phoneticLanguageID.LanguageCultureExtensionCode)
            {
                case "ja--rj":
                    list.Add(JapaneseKana);
                    break;
                case "ja--kn":
                    list.Add(JapaneseRomaji);
                    break;
                case "ar--rm":
                    list.Add(Arabic);
                    break;
                case "eg--rm":
                    list.Add(EgyptianArabic);
                    break;
                case "he--rm":
                    list.Add(Hebrew);
                    break;
                case "el--rm":
                    list.Add(Greek);
                    break;
                default:
                    break;
            }

            return list;
        }

        public static List<LanguageID> GetAlternateNonPhoneticLanguageIDs(LanguageID nonPhoneticLanguageID)
        {
            List<LanguageID> list = new List<LanguageID>();

            switch (nonPhoneticLanguageID.LanguageCultureExtensionCode)
            {
                case "ja":
                    list.Add(JapaneseKana);
                    break;
                default:
                    break;
            }

            return list;
        }

        public static List<LanguageID> GetNonPhoneticLanguageIDs(LanguageID phoneticLanguageID)
        {
            List<LanguageID> list = new List<LanguageID>();

            switch (phoneticLanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    list.Add(ChineseSimplified);
                    list.Add(ChineseTraditional);
                    break;
                case "ko--rm":
                    list.Add(Korean);
                    break;
                case "ja--rj":
                    list.Add(JapaneseKana);
                    list.Add(Japanese);
                    break;
                case "ja--kn":
                    list.Add(Japanese);
                    break;
                case "ar--rm":
                    list.Add(Arabic);
                    break;
                case "eg--rm":
                    list.Add(EgyptianArabic);
                    break;
                case "he--rm":
                    list.Add(Hebrew);
                    break;
                case "el--rm":
                    list.Add(Greek);
                    break;
                default:
                    break;
            }

            return list;
        }

        public static List<LanguageID> GetPhoneticLanguageIDs(LanguageID nonPhoneticLanguageID)
        {
            List<LanguageID> list = new List<LanguageID>();

            switch (nonPhoneticLanguageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                    list.Add(ChinesePinyin);
                    break;
                case "ko":
                    list.Add(KoreanRomanization);
                    break;
                case "ja":
                case "ja--kn":
                    list.Add(JapaneseRomaji);
                    break;
                case "ar":
                    list.Add(ArabicRomanization);
                    break;
                case "eg":
                    list.Add(EgyptianArabicRomanization);
                    break;
                case "he":
                    list.Add(HebrewRomanization);
                    break;
                case "el":
                    list.Add(GreekRomanization);
                    break;
                default:
                    break;
            }

            return list;
        }

        public static LanguageID GetBestVoiceLanguageID(LanguageID rootLanguageID)
        {
            if (rootLanguageID == null)
                return rootLanguageID;

            if (IsRomanized(rootLanguageID))
                rootLanguageID = GetRootLanguageID(rootLanguageID);

            switch (rootLanguageID.LanguageCode)
            {
                case "ja":
                    return LanguageLookup.JapaneseKana;
                default:
                    break;
            }

            return rootLanguageID;
        }

        public static LanguageID GetRootLanguageID(LanguageID rootLanguageID)
        {
            if (rootLanguageID == null)
                return rootLanguageID;

            LanguageID languageID = new LanguageID(
                rootLanguageID.LanguageCode,
                (!String.IsNullOrEmpty(rootLanguageID.CultureCode) ? rootLanguageID.CultureCode : null),
                null);
            return languageID;
        }

        public static bool IsNeedTransliteration(LanguageID languageID)
        {
            if (languageID == null)
                return false;

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static bool IsUseSpacesToSeparateWords(LanguageID languageID)
        {
            if (languageID == null)
                return false;
            switch (languageID.LanguageCultureExtensionCode)
            {
                case "ar":
                case "ar--vw":
                case "ar--rm":
                case "bg":
                case "ca":
                case "zh--pn":
                case "cs":
                case "da":
                case "nl":
                case "eg":
                case "eg--vw":
                case "eg--rm":
                case "en":
                case "et":
                case "fi":
                case "fr":
                case "de":
                case "el":
                case "el--rm":
                case "ht":
                case "he":
                case "he--rm":
                case "hi":
                case "hu":
                case "id":
                case "it":
                case "ja--rj":
                case "ko--rm":
                case "lv":
                case "lt":
                case "no":
                case "pl":
                case "pt":
                case "ro":
                case "ru":
                case "sk":
                case "sl":
                case "es":
                case "sv":
                case "th":
                case "tr":
                case "uk":
                case "vi":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUseSpacesToSeparateWords(List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                return false;
            foreach (LanguageID languageID in languageIDs)
            {
                if (!IsUseSpacesToSeparateWords(languageID))
                    return false;
            }
            return true;
        }

        public static bool IsUsePeriodsToSeparateSentences(LanguageID languageID)
        {
            if (languageID == null)
                return false;
            switch (languageID.LanguageCultureExtensionCode)
            {
                case "ar":
                case "ar--vw":
                case "ar--rm":
                case "bg":
                case "ca":
                case "zh--pn":
                case "cs":
                case "da":
                case "nl":
                case "eg":
                case "eg--vw":
                case "eg--rm":
                case "en":
                case "et":
                case "fi":
                case "fr":
                case "de":
                case "el":
                case "el--rm":
                case "ht":
                case "he":
                case "he--rm":
                case "hi":
                case "hu":
                case "id":
                case "it":
                case "ja--rj":
                case "ko--rm":
                case "lv":
                case "lt":
                case "no":
                case "pl":
                case "pt":
                case "ro":
                case "ru":
                case "sk":
                case "sl":
                case "es":
                case "sv":
                case "th":
                case "tr":
                case "uk":
                case "vi":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCharacterBased(LanguageID languageID)
        {
            if (languageID == null)
                return false;
            switch (languageID.LanguageCode)
            {
                case "zh":
                    if (languageID.ExtensionCode == "pn")
                        return false;
                    return true;
                case "ja":
                    if (languageID.ExtensionCode == "rj")
                        return false;
                    return true;
                case "ko":
                    if (languageID.ExtensionCode == "rm")
                        return false;
                    return true;
                default:
                    return false;
            }
        }

        public static bool SupportsCapitalization(LanguageID languageID)
        {
            switch (languageID.ExtensionCode)
            {
                case null:
                case "":
                default:
                    break;
                case "rm":
                case "rj":
                case "pn":
                    return true;
            }

            switch (languageID.LanguageCode)
            {
                case "zh":
                case "ja":
                case "ko":
                    return false;
                // Fixme with other cases!
                default:
                    return true;
            }
        }

        public static bool HasAlternateCharacters(LanguageID languageID)
        {
            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasAlternatePhonetic(LanguageID languageID)
        {
            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                case "ko":
                case "ja":
                case "ja--kn":
                case "ar":
                case "ar--vw":
                case "eg":
                case "eg--vw":
                case "he":
                case "he-vw":
                case "el":
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasAnyAlternates(LanguageID languageID)
        {
            return HasAlternatePhonetic(languageID) || HasAlternateCharacters(languageID);
        }

        public static List<LanguageID> GetAlternateCharacterLanguageIDs(LanguageID languageID)
        {
            List<LanguageID> list = new List<LanguageID>();

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                    list.Add(ChineseTraditional);
                    break;
                case "zh-CHT":
                    list.Add(ChineseSimplified);
                    break;
                default:
                    break;
            }

            return list;
        }

        public static bool IsAlternateOfLanguageID(LanguageID languageID1, LanguageID languageID2)
        {
            if (languageID1 == languageID2)
                return false;

            if (languageID1.LanguageCode == languageID2.LanguageCode)
                return true;

            return false;
        }

        public static bool IsAlternateCharacterLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            List<LanguageID> languageIDs = GetAlternateCharacterLanguageIDs(sourceLanguageID);

            if (languageIDs.Contains(destLanguageID))
                return true;

            return false;
        }

        public static bool IsAlternatePhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            List<LanguageID> languageIDs = GetAlternatePhoneticLanguageIDs(sourceLanguageID);

            if (languageIDs.Contains(destLanguageID))
                return true;

            languageIDs = GetAlternateNonPhoneticLanguageIDs(sourceLanguageID);

            if (languageIDs.Contains(destLanguageID))
                return true;

            return false;
        }

        public static bool IsPhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            List<LanguageID> languageIDs = GetNonPhoneticLanguageIDs(sourceLanguageID);

            if (languageIDs.Contains(destLanguageID))
                return true;

            languageIDs = GetPhoneticLanguageIDs(sourceLanguageID);

            if (languageIDs.Contains(destLanguageID))
                return true;

            return false;
        }

        public static bool IsNonPhoneticToPhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            if (IsAlternatePhonetic(destLanguageID) && !IsAlternatePhonetic(sourceLanguageID))
            {
                if (destLanguageID.LanguageCode == sourceLanguageID.LanguageCode)
                    return true;
            }

            return false;
        }

        public static bool IsVowelledToPhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            if (IsRomanized(destLanguageID) && IsVowelled(sourceLanguageID))
            {
                if (destLanguageID.LanguageCode == sourceLanguageID.LanguageCode)
                    return true;
            }

            return false;
        }

        public static bool IsNonRomanizedPhoneticToPhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            if (IsRomanized(destLanguageID) && IsNonRomanizedAlternatePhonetic(sourceLanguageID))
            {
                if (destLanguageID.LanguageCode == sourceLanguageID.LanguageCode)
                    return true;
            }

            return false;
        }

        public static bool IsPhoneticToNonRomanizedPhoneticLanguageIDCompare(LanguageID destLanguageID, LanguageID sourceLanguageID)
        {
            if (IsNonRomanizedAlternatePhonetic(destLanguageID) && IsRomanized(sourceLanguageID))
            {
                if (destLanguageID.LanguageCode == sourceLanguageID.LanguageCode)
                    return true;
            }

            return false;
        }

        public static int GetTranslationWeight(LanguageID destLanguageID, LanguageID sourceLanguageID,
            List<LanguageID> userLanguageIDs)
        {
            int weight = 0;

            if (IsAlternateCharacterLanguageIDCompare(destLanguageID, sourceLanguageID))
                weight = 5;
            else if (IsVowelledToPhoneticLanguageIDCompare(destLanguageID, sourceLanguageID))
                weight = 4;
            else if (IsNonRomanizedPhoneticToPhoneticLanguageIDCompare(destLanguageID, sourceLanguageID))
                weight = 4;
            else if (IsPhoneticToNonRomanizedPhoneticLanguageIDCompare(destLanguageID, sourceLanguageID))
                weight = 4;
            else if (IsNonPhoneticToPhoneticLanguageIDCompare(destLanguageID, sourceLanguageID))
                weight = 3;
            else if (sourceLanguageID == English)
                weight = 2;
            else if (((userLanguageIDs != null) && userLanguageIDs.Contains(sourceLanguageID)) || (sourceLanguageID == English))
                weight = 1;

            return weight;
        }

        public static string GetLanguagePeriod(LanguageID languageID)
        {
            if (languageID == null)
                return ".";

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                    return "。";
                default:
                    return ".";
            }
        }

        public static string GetLanguagePeriodAndSpace(LanguageID languageID)
        {
            if (languageID == null)
                return ".";

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                case "zh--pn":
                    return "。";
                default:
                    return ". ";
            }
        }

        public static bool IsSentence(string text, LanguageID languageID)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            int index = text.Length - 1;

            while (index >= 0)
            {
                if (!Char.IsWhiteSpace(text[index]) && !MatchedEndCharacters.Contains(text[index]))
                    break;

                index--;
            }

            if (SentenceTerminatorCharacters.Contains(text[index]))
                return true;

            return false;
        }

        public static bool EndsWithPeriod(string str)
        {
            if (str == null)
                return false;

            str = str.Trim();

            foreach (char period in PeriodCharacters)
            {
                if (str.EndsWith(Convert.ToString(period)))
                    return true;
            }

            return false;
        }

        public static char GetMatchedEndChar(char chr)
        {
            char returnValue;

            switch (chr)
            {
                case '"':
                    returnValue = '"';
                    break;
                case '“':
                    returnValue = '”';
                    break;
                case '\'':
                    returnValue = '\'';
                    break;
                case '‘':
                    returnValue = '’';
                    break;
                case '(':
                    returnValue = ')';
                    break;
                case '（':
                    returnValue = '）';
                    break;
                case '[':
                    returnValue = ']';
                    break;
                case '{':
                    returnValue = '}';
                    break;
                case '【':
                    returnValue = '】';
                    break;
                case '「':
                    returnValue = '」';
                    break;
                default:
                    returnValue = chr;
                    break;
            }

            return returnValue;
        }

        public static string GetLanguageComma(LanguageID languageID)
        {
            if (languageID == null)
                return ".";

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                case "zh--pn":
                    return "，";
                default:
                    return ",";
            }
        }

        public static string GetLanguageSpace(LanguageID languageID)
        {
            if (languageID == null)
                return " ";

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh-CHS":
                case "zh-CHT":
                    return String.Empty;
                default:
                    return " ";
            }
        }

        public static string GetLanguageSpaces(LanguageID languageID, int count)
        {
            if (count == 0)
                return String.Empty;
            else
            {
                string space = GetLanguageSpace(languageID);

                switch (count)
                {
                    case 1:
                        return space;
                    case 2:
                        return space + space;
                    case 3:
                        return space + space + space;
                    case 4:
                        return space + space + space + space;
                    default:
                        {
                            StringBuilder sb = new StringBuilder();
                            while (count > 0)
                            {
                                sb.Append(space);
                                count--;
                            }
                            return sb.ToString();
                        }
                }
            }
        }

        public static List<string> GetLanguageWordSeparators(List<LanguageID> languageIDs)
        {
            int count = 0;
            if (languageIDs != null)
                count = languageIDs.Count();
            List<string> spaces = new List<string>(count);
            foreach (LanguageID languageID in languageIDs)
            {
                string spc = GetLanguageSpace(languageID);
                spaces.Add(spc);
            }
            return spaces;
        }

        public static string GetPreferredFontFamily(LanguageID languageID)
        {
            string fontFamily = "Verdana";

            if (languageID != null)
            {
                switch (languageID.LanguageCode)
                {
                    case "zh":
                        fontFamily = "Simsun";
                        break;
                    default:
                        break;
                }
            }

            return fontFamily;
        }

        private static readonly Dictionary<string, string> NonStandardLanguageMapDictionary = new Dictionary<string, string>
        {
            {"eg", "ar-EG"},
            {"zh-CN", "zh-CHS"},
            {"zh-CN-pn", ""},
            {"zh-TW", "zh-CHT"},
            {"zh-TW-pn", ""},
            {"zh--pn", ""},
            {"ja--kn", "ja"},
            {"ja--rj", ""},
            {"ko--rm", ""}
        };

        public static LanguageID GetStandardLanguageID(LanguageID languageID)
        {
            LanguageID standardLanguageID = languageID;
            string languageCode;

            if (String.IsNullOrEmpty(languageID.LanguageCode))
                return standardLanguageID;

            if (NonStandardLanguageMapDictionary.TryGetValue(languageID.LanguageCultureExtensionCode, out languageCode))
            {
                if (!String.IsNullOrEmpty(languageCode))
                    standardLanguageID = new LanguageID(languageCode);
            }
            else if (!LanguageIDs.Contains(standardLanguageID))
            {
                standardLanguageID.CultureCode = null;
                standardLanguageID.ExtensionCode = null;
            }

            return standardLanguageID;
        }

        public static List<LanguageID> GetStandardLanguages()
        {
            List<LanguageID> standardLanguages = new List<LanguageID>();

            foreach (LanguageID languageID in LanguageIDs)
            {
                string languageCode;

                if (String.IsNullOrEmpty(languageID.LanguageCode))
                    continue;

                if (NonStandardLanguageMapDictionary.TryGetValue(languageID.LanguageCultureExtensionCode, out languageCode))
                {
                    if (!String.IsNullOrEmpty(languageCode))
                        standardLanguages.Add(new LanguageID(languageCode));
                }
                else
                {
                    if (!String.IsNullOrEmpty(languageID.ExtensionCode))
                        continue;

                    if (languageID.LanguageCode.StartsWith("("))
                        continue;

                    standardLanguages.Add(new LanguageID(languageID));
                }
            }

            return standardLanguages;
        }

        public static bool IsSameFamily(LanguageID languageID1, LanguageID languageID2)
        {
            if ((languageID1 == null) || (languageID2 == null))
                return false;

            if (languageID1.LanguageCode == languageID2.LanguageCode)
                return true;

            return false;
        }

        public static List<LanguageID> GetFamilyRootLanguageIDs(List<LanguageID> languageIDs)
        {
            List<LanguageID> rootLanguageIDs = new List<LanguageID>();

            if (languageIDs == null)
                return rootLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageID rootLanguageID = GetRootLanguageID(languageID);

                if (!rootLanguageIDs.Contains(rootLanguageID))
                    rootLanguageIDs.Add(rootLanguageID);
            }

            return rootLanguageIDs;
        }

        public static List<LanguageID> GetFamilyLanguageIDs(LanguageID rootLanguageID)
        {
            List<LanguageID> familyLanguageIDs = GetLanguagePlusAlternateLanguageIDs(rootLanguageID);
            return familyLanguageIDs;
        }

        public static int GetFamilyLanguageIDCount(LanguageID rootLanguageID)
        {
            List<LanguageID> familyLanguageIDs = GetLanguagePlusAlternateLanguageIDs(rootLanguageID);
            return familyLanguageIDs.Count();
        }

        public static string ReplaceWord(
            string text,
            string pattern,
            string replacement,
            LanguageID languageID)
        {
            string returnValue = text;
            
            if (String.IsNullOrEmpty(text))
                return text;

            int startIndex = 0;

            if (IsUseSpacesToSeparateWords(languageID))
            {
                for (;;)
                {
                    int index = returnValue.IndexOf(pattern, startIndex);

                    if (index == -1)
                        break;

                    if (((index == 0) || char.IsWhiteSpace(returnValue[index - 1])) &&
                        ((index == returnValue.Length - pattern.Length) || char.IsWhiteSpace(returnValue[index + pattern.Length])))
                    {
                        returnValue = returnValue.Substring(0, index) + replacement + returnValue.Substring(index + pattern.Length);
                        startIndex = index + replacement.Length;
                    }
                    else
                        startIndex = index + pattern.Length;
                }
            }
            else
                returnValue = returnValue.Replace(pattern, replacement);

            return returnValue;
        }

        public static string AppendWord(
            string text,
            string word,
            LanguageID languageID)
        {
            string returnValue = text;

            if (String.IsNullOrEmpty(text))
                return text;

            if (IsUseSpacesToSeparateWords(languageID))
                returnValue = returnValue + " " + word;
            else
                returnValue = returnValue + word;

            return returnValue;
        }
    }
}

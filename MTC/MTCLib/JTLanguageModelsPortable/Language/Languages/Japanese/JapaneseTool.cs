using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseTool : LanguageTool
    {
        public string AuxSep = " ";

        public static LanguageID JapaneseID = LanguageLookup.Japanese;
        public static LanguageID JapaneseKanaID = LanguageLookup.JapaneseKana;
        public static LanguageID JapaneseRomajiID = LanguageLookup.JapaneseRomaji;
        public static LanguageID EnglishID = LanguageLookup.English;
        public static List<string> EDictEntriesToBeAdded;
        public static List<string> EDictEntriesToBeUpdated;
        protected ConvertTransliterate CharacterConverter;
        protected ConvertTransliterate RomanizationConverter;

        public static List<LanguageID> LocalInflectionLanguageIDs = new List<LanguageID>()
            {
                JapaneseID,
                JapaneseKanaID,
                JapaneseRomajiID,
                EnglishID
            };

        public static List<LanguageID> JapaneseLanguageIDs = new List<LanguageID>()
            {
                JapaneseID,
                JapaneseKanaID,
                JapaneseRomajiID
            };

        public static ConvertRomaji RomajiConverter = new ConvertRomaji(
            JapaneseKanaID,
            '\0',
            null,
            false);

        public static ConvertRomaji KanaConverter = new ConvertRomaji(
            JapaneseID,
            '\0',
            null,
            false);

        public JapaneseTool() : base(JapaneseID)
        {
            _TargetLanguageIDs = JapaneseLanguageIDs;
            _HostLanguageIDs = new List<LanguageID>() { LanguageLookup.English };
            CharacterConverter = null;
            RomanizationConverter = null;
            SetCanInflect("Verb", true);
            SetCanInflect("Noun", true);
            SetCanInflect("Adjective", true);
            CanDeinflect = true;
        }

        public override IBaseObject Clone()
        {
            return new JapaneseTool();
        }
    }
}

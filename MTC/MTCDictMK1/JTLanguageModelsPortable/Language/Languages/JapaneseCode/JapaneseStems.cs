using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Language
{
    public class JapaneseStems
    {
        public string DictionaryFormJapanese;
        public string DictionaryFormKana;
        public string DictionaryFormRomaji;
        public string StemJapanese;
        public string StemKana;
        public string StemRomaji;
        public string AStem;
        public string EStem;
        public string IStem;
        public string OStem;
        public string UStem;
        public string TAStem;
        public string TEStem;
        public string UStemRomaji;

        public JapaneseStems(
            string dictionaryFormJapanese,
            string dictionaryFormKana,
            string dictionaryFormRomaji,
            string stemJapanese,
            string stemKana,
            string stemRomaji,
            string aStem,
            string eStem,
            string iStem,
            string oStem,
            string uStem,
            string taStem,
            string teStem,
            string uStemRomaji)
        {
            DictionaryFormJapanese = dictionaryFormJapanese;
            DictionaryFormKana = dictionaryFormKana;
            DictionaryFormRomaji = dictionaryFormRomaji;
            StemJapanese = stemJapanese;
            StemKana = stemKana;
            StemRomaji = stemRomaji;
            AStem = aStem;
            EStem = eStem;
            IStem = iStem;
            OStem = oStem;
            UStem = uStem;
            TAStem = taStem;
            TEStem = teStem;
            UStemRomaji = uStemRomaji;
        }

        public JapaneseStems(
            string dictionaryFormJapanese,
            string dictionaryFormKana,
            string dictionaryFormRomaji,
            string stemJapanese,
            string stemKana,
            string stemRomaji)
        {
            DictionaryFormJapanese = dictionaryFormJapanese;
            DictionaryFormKana = dictionaryFormKana;
            DictionaryFormRomaji = dictionaryFormRomaji;
            StemJapanese = stemJapanese;
            StemKana = stemKana;
            StemRomaji = stemRomaji;
            AStem = String.Empty;
            EStem = String.Empty;
            IStem = String.Empty;
            OStem = String.Empty;
            UStem = String.Empty;
            TAStem = String.Empty;
            TEStem = String.Empty;
            UStemRomaji = String.Empty;
        }

        public JapaneseStems(JapaneseStems other)
        {
            DictionaryFormJapanese = other.DictionaryFormJapanese;
            DictionaryFormKana = other.DictionaryFormKana;
            DictionaryFormRomaji = other.DictionaryFormRomaji;
            StemJapanese = other.StemJapanese;
            StemKana = other.StemKana;
            StemRomaji = other.StemRomaji;
            AStem = other.AStem;
            EStem = other.EStem;
            IStem = other.IStem;
            OStem = other.OStem;
            UStem = other.UStem;
            TAStem = other.TAStem;
            TEStem = other.TEStem;
            UStemRomaji = other.UStemRomaji;
        }

        public JapaneseStems()
        {
            DictionaryFormJapanese = String.Empty;
            DictionaryFormKana = String.Empty;
            DictionaryFormRomaji = String.Empty;
            StemJapanese = String.Empty;
            StemKana = String.Empty;
            StemRomaji = String.Empty;
            AStem = String.Empty;
            EStem = String.Empty;
            IStem = String.Empty;
            OStem = String.Empty;
            UStem = String.Empty;
            TAStem = String.Empty;
            TEStem = String.Empty;
            UStemRomaji = String.Empty;
        }

        public void InsertPrefix(string kanaPrefix, string romajiPrefix)
        {
            DictionaryFormJapanese = kanaPrefix + DictionaryFormJapanese;
            DictionaryFormKana = kanaPrefix + DictionaryFormKana;
            DictionaryFormRomaji = romajiPrefix + DictionaryFormRomaji;
            StemJapanese = kanaPrefix + StemJapanese;
            StemKana = kanaPrefix + StemKana;
            StemRomaji = romajiPrefix + StemRomaji;
        }

        public void ReplaceRoot(string kanjiPrefix, string kanaPrefix)
        {
            DictionaryFormJapanese = ReplacePrefix(DictionaryFormJapanese, kanjiPrefix, kanaPrefix);
            StemJapanese = ReplacePrefix(StemJapanese, kanjiPrefix, kanaPrefix);
        }

        protected string ReplacePrefix(string str, string oldPrefix, string newPrefix)
        {
            if (str.StartsWith(oldPrefix))
                str = newPrefix + str.Substring(oldPrefix.Length);
            return str;
        }
    }
}

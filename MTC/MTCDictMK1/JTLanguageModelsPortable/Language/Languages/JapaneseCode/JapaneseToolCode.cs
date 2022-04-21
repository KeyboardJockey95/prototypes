using System;
using System.Collections.Generic;
using System.IO;
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
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseToolCode : JapaneseTool
    {
        public JapaneseToolCode()
        {
            SetCanInflect("Verb", true);
            SetCanInflect("Noun", true);
            SetCanInflect("Adjective", true);
            CanDeinflect = true;
        }

        public override IBaseObject Clone()
        {
            return new JapaneseToolCode();
        }

        public override List<Inflection> InflectAnyDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    EnglishID,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);
                switch (category)
                {
                    case LexicalCategory.Verb:
                        inflections = ConjugateVerbDictionaryFormAll(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Adjective:
                        inflections = DeclineAdjectiveDictionaryFormAll(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Noun:
                        if (IsNaAdjective(dictionaryEntry, senseIndex))
                            inflections = DeclineAdjectiveDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        else
                            inflections = ConjugateVerbDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        break;
                }
            }

            return inflections;
        }

        public override List<Inflection> InflectAnyDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    EnglishID,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);
                switch (category)
                {
                    case LexicalCategory.Verb:
                        inflections = ConjugateVerbDictionaryFormDefault(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Adjective:
                        inflections = DeclineAdjectiveDictionaryFormDefault(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Noun:
                        if (IsNaAdjective(dictionaryEntry, senseIndex))
                            inflections = DeclineAdjectiveDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        else
                            inflections = ConjugateVerbDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        break;
                }
            }

            return inflections;
        }

        public override bool InflectAnyDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            bool returnValue = false;

            inflection = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                switch (category)
                {
                    case LexicalCategory.Verb:
                        returnValue = ConjugateVerbDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                    case LexicalCategory.Adjective:
                        returnValue = DeclineAdjectiveDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                    case LexicalCategory.Noun:
                        returnValue = DeclineNounDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                }
            }

            return returnValue;
        }

        protected bool SetInflectedOutput(
            string stem,
            string ending,
            string englishInflected,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = stem + ending;
            string kanaInflected = root.Text(JapaneseKanaID) + ending;
            string romajiInflected;
            string romajiEnding;
            bool returnValue = true;

            if (RomajiConverter.ToTable(out romajiEnding, ending))
                romajiInflected = root.Text(JapaneseRomajiID) + romajiEnding;
            else
            {
                romajiInflected = kanaInflected;
                inflected.AppendError("Error getting romaji ending.");
                returnValue = false;
            }

            output.SetText(JapaneseID, japaneseInflected);
            output.SetText(JapaneseKanaID, kanaInflected);
            output.SetText(JapaneseRomajiID, romajiInflected);
            output.SetText(EnglishID, englishInflected);

            if (stem.StartsWith("お"))
            {
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");
                RemoveFirstCharacter(root);
            }

            suffix.SetText(JapaneseID, ending);
            suffix.SetText(JapaneseKanaID, ending);
            suffix.SetText(JapaneseRomajiID, romajiEnding);

            return returnValue;
        }

        protected bool SetIrregularInflectedOutput(
            string stem,
            string kanaStem,
            string romajiStem,
            string ending,
            string englishInflected,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = stem + ending;
            string kanaInflected = kanaStem + ending;
            string romajiInflected;
            string romajiEnding;
            bool returnValue = true;

            if (RomajiConverter.ToTable(out romajiEnding, ending))
            {
                //if ((stem.Length >= 1) && (stem[0] == 'し') && romajiRoot.StartsWith("su"))
                //    romajiRoot = "shi" + romajiRoot.Substring(2);
                romajiInflected = romajiStem + romajiEnding;
                if (!String.IsNullOrEmpty(AuxSep))
                    romajiInflected = romajiInflected.Replace("kudasai", AuxSep + "kudasai");
            }
            else
            {
                romajiInflected = kanaInflected;
                inflected.AppendError("Error getting romaji ending.");
                returnValue = false;
            }

            output.SetText(JapaneseID, japaneseInflected);
            output.SetText(JapaneseKanaID, kanaInflected);
            output.SetText(JapaneseRomajiID, romajiInflected);
            output.SetText(EnglishID, englishInflected);

            if (stem.StartsWith("お"))
            {
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");

                //if (stem == root.Text(JapaneseID))
                //    RemoveFirstCharacter(root);
            }

            suffix.SetText(JapaneseID, ending);
            suffix.SetText(JapaneseKanaID, ending);
            suffix.SetText(JapaneseRomajiID, romajiEnding);

            return returnValue;
        }

        protected bool SetFullInflectedOutput(
            Inflection inflected,
            string kanjiStem,
            string kanaStem,
            string romajiStem,
            string kanaEnding,
            string romajiEnding,
            string englishInflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = kanjiStem + kanaEnding;
            string kanaInflected = kanaStem + kanaEnding;
            string romajiInflected = romajiStem + romajiEnding;
            bool returnValue = true;

            if (!String.IsNullOrEmpty(AuxSep))
                romajiInflected = romajiInflected.Replace("kudasai", AuxSep + "kudasai");

            output.SetText(JapaneseID, japaneseInflected.Trim());
            output.SetText(JapaneseKanaID, kanaInflected.Trim());
            output.SetText(JapaneseRomajiID, romajiInflected.Trim());
            output.SetText(EnglishID, englishInflected.Trim());

            if (kanjiStem.StartsWith("お"))
            {
                kanjiStem = kanjiStem.Substring(1);
                kanaStem = kanaStem.Substring(1);
                romajiStem = romajiStem.Substring(1);
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");
            }

            root.SetText(JapaneseID, kanjiStem.Trim());
            root.SetText(JapaneseKanaID, kanaStem.Trim());
            root.SetText(JapaneseRomajiID, romajiStem.Trim());

            suffix.SetText(JapaneseID, kanaEnding.Trim());
            suffix.SetText(JapaneseKanaID, kanaEnding.Trim());
            suffix.SetText(JapaneseRomajiID, romajiEnding.Trim());

            return returnValue;
        }

        protected void SetText(MultiLanguageString mls, string kanji, string kana, string romaji, string english)
        {

            mls.SetText(JapaneseID, kanji);
            mls.SetText(JapaneseKanaID, kana);
            mls.SetText(JapaneseRomajiID, romaji);
            mls.SetText(EnglishID, english);
        }

        protected bool RemoveTeEnding(MultiLanguageString output)
        {
            string inText;
            string outText;
            bool returnValue;
            inText = output.Text(JapaneseID);
            returnValue = RemoveTeEnding(inText, out outText);
            if (returnValue)
            {
                output.SetText(JapaneseID, outText);
                inText = output.Text(JapaneseKanaID);
                RemoveTeEnding(inText, out outText);
                output.SetText(JapaneseKanaID, outText);
                inText = output.Text(JapaneseRomajiID);
                RemoveTeEnding(inText, out outText);
                output.SetText(JapaneseRomajiID, outText);
            }
            return returnValue;
        }

        protected bool RemoveTeEnding(string input, out string output)
        {
            if (input.EndsWith("て"))
            {
                output = input.Substring(0, input.Length - 1);
                return true;
            }
            else if (input.EndsWith("te"))
            {
                output = input.Substring(0, input.Length - 2);
                return true;
            }
            output = input;
            return false;
        }

        protected void RomajiTeFixupCheck(MultiLanguageString output, string duplicatedChar)
        {
            string str = output.Text(JapaneseRomajiID);
            if (str.EndsWith("t"))
            {
                str = str.Substring(0, str.Length - 1) + duplicatedChar;
                output.SetText(JapaneseRomajiID, str);
            }
        }

        protected bool RemoveDeEnding(MultiLanguageString output)
        {
            string inText;
            string outText;
            bool returnValue;
            inText = output.Text(JapaneseID);
            returnValue = RemoveDeEnding(inText, out outText);
            if (returnValue)
            {
                output.SetText(JapaneseID, outText);
                inText = output.Text(JapaneseKanaID);
                RemoveDeEnding(inText, out outText);
                output.SetText(JapaneseKanaID, outText);
                inText = output.Text(JapaneseRomajiID);
                RemoveDeEnding(inText, out outText);
                output.SetText(JapaneseRomajiID, outText);
            }
            return returnValue;
        }

        protected bool RemoveDeEnding(string input, out string output)
        {
            if (input.EndsWith("で"))
            {
                output = input.Substring(0, input.Length - 1);
                return true;
            }
            else if (input.EndsWith("de"))
            {
                output = input.Substring(0, input.Length - 2);
                return true;
            }
            output = input;
            return false;
        }

        protected void ReplaceEndingSuToShi(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("す"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "し";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "し";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "shi";
            }
        }

        protected void ReplaceEndingKuToKi(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("く"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "き";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "き";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ki";
            }
            else if (kanjiStem.EndsWith("来"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "来";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "き";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ki";
            }
        }

        protected void ReplaceEndingKuToKo(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("く"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "こ";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "こ";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ko";
            }
            else if (kanjiStem.EndsWith("来"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "来";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "こ";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ko";
            }
        }

        protected void FixupVerbSuru(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString root = inflected.Root;
            MultiLanguageString suffix = inflected.Suffix;
            string outputKanji = output.Text(JapaneseID);

            if (String.IsNullOrEmpty(outputKanji))
                return;

            string outputKana = output.Text(JapaneseKanaID);
            string outputRomaji = output.Text(JapaneseRomajiID);
            string rootKanji = root.Text(JapaneseID);
            string rootKana = root.Text(JapaneseKanaID);
            string rootRomaji = root.Text(JapaneseRomajiID);
            string suffixKanji = suffix.Text(JapaneseID);
            string suffixKana = suffix.Text(JapaneseKanaID);
            string suffixRomaji = suffix.Text(JapaneseRomajiID);
            char lastKanji = rootKana[rootKanji.Length - 1];
            char lastKana = rootKana[rootKana.Length - 1];
            int ofs;

            if (lastKana == 'す')
            {
                rootKanji = rootKanji.Substring(0, rootKanji.Length - 1);
                rootKana = rootKana.Substring(0, rootKana.Length - 1);

                ofs = outputKanji.IndexOf(rootKanji);
                suffixKanji = outputKanji.Substring(ofs + rootKanji.Length);

                ofs = outputKana.IndexOf(rootKana);
                suffixKana = outputKana.Substring(ofs + rootKana.Length);

                if (lastKana == 'す')
                    rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 2);
                else
                    rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 3);

                ofs = outputRomaji.IndexOf(rootRomaji);
                suffixRomaji = outputRomaji.Substring(ofs + rootRomaji.Length);

                root.SetText(JapaneseID, rootKanji);
                root.SetText(JapaneseKanaID, rootKana);
                root.SetText(JapaneseRomajiID, rootRomaji);
                suffix.SetText(JapaneseID, suffixKanji);
                suffix.SetText(JapaneseKanaID, suffixKana);
                suffix.SetText(JapaneseRomajiID, suffixRomaji);
            }
        }

        protected void FixupVerbKuru(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString root = inflected.Root;
            MultiLanguageString suffix = inflected.Suffix;
            string outputKanji = output.Text(JapaneseID);

            if (String.IsNullOrEmpty(outputKanji))
                return;

            string outputKana = output.Text(JapaneseKanaID);
            string outputRomaji = output.Text(JapaneseRomajiID);
            string rootKanji = root.Text(JapaneseID);
            string rootKana = root.Text(JapaneseKanaID);
            string rootRomaji = root.Text(JapaneseRomajiID);
            string suffixKanji = suffix.Text(JapaneseID);
            string suffixKana = suffix.Text(JapaneseKanaID);
            string suffixRomaji = suffix.Text(JapaneseRomajiID);
            char lastKanji = rootKana[rootKanji.Length - 1];
            char lastKana = rootKana[rootKana.Length - 1];
            int ofs;

            if ((lastKana == 'く') || (lastKana == 'き') || (lastKana == 'こ'))
            {
                rootKanji = rootKanji.Substring(0, rootKanji.Length - 1);
                rootKana = rootKana.Substring(0, rootKana.Length - 1);
                rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 2);

                ofs = outputKanji.IndexOf(rootKanji);
                suffixKanji = outputKanji.Substring(ofs + rootKanji.Length);

                ofs = outputKana.IndexOf(rootKana);
                suffixKana = outputKana.Substring(ofs + rootKana.Length);

                ofs = outputRomaji.IndexOf(rootRomaji);
                suffixRomaji = outputRomaji.Substring(ofs + rootRomaji.Length);

                root.SetText(JapaneseID, rootKanji);
                root.SetText(JapaneseKanaID, rootKana);
                root.SetText(JapaneseRomajiID, rootRomaji);
                suffix.SetText(JapaneseID, suffixKanji);
                suffix.SetText(JapaneseKanaID, suffixKana);
                suffix.SetText(JapaneseRomajiID, suffixRomaji);
            }
        }

        protected void FixupInflection(Inflection inflected)
        {
            //inflected.Output.SetText(EnglishID, String.Empty);
            CopyMLS(inflected.PronounOutput, inflected.Output);
            CopyMLS(inflected.RegularOutput, inflected.Output);
            CopyMLS(inflected.RegularPronounOutput, inflected.Output);
        }

        protected void CopyMLS(MultiLanguageString targetMLS, MultiLanguageString sourceMLS)
        {
            if (sourceMLS.LanguageStrings == null)
                return;

            foreach (LanguageString srcLS in sourceMLS.LanguageStrings)
            {
                LanguageString targetLS = targetMLS.LanguageString(srcLS.LanguageID);

                if (targetLS == null)
                {
                    targetLS = new LanguageString(srcLS);
                    targetMLS.Add(targetLS);
                }
                else if (!targetLS.HasText())
                    targetLS.Text = srcLS.Text;
            }
        }

        protected void AppendToOutput(
            MultiLanguageString output,
            string kanjiEnding,
            string kanaEnding,
            string romajiEnding)
        {
            output.SetText(JapaneseID, output.Text(JapaneseID) + kanjiEnding);
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + kanaEnding);
            string romajiRoot = output.Text(JapaneseRomajiID);
            if (romajiRoot.EndsWith("t"))
                romajiRoot = romajiRoot.Substring(0, romajiRoot.Length - 1) + romajiEnding.Substring(0, 1);
            output.SetText(JapaneseRomajiID, romajiRoot + romajiEnding);
        }

        protected void AppendToOutput(
            MultiLanguageString output,
            MultiLanguageString suffix)
        {
            string kanjiEnding = suffix.Text(JapaneseID);
            string kanaEnding = suffix.Text(JapaneseKanaID);
            string romajiEnding = suffix.Text(JapaneseRomajiID);
            string englishEnding = suffix.Text(EnglishID);
            output.SetText(JapaneseID, output.Text(JapaneseID) + kanjiEnding);
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + kanaEnding);
            string romajiRoot = output.Text(JapaneseRomajiID);
            if (romajiRoot.EndsWith("t"))
                romajiRoot = romajiRoot.Substring(0, romajiRoot.Length - 1) + romajiEnding.Substring(0, 1);
            output.SetText(JapaneseRomajiID, romajiRoot + romajiEnding);
            output.SetText(EnglishID, output.Text(EnglishID) + englishEnding);
        }

        protected bool ReplaceSuffix(
            MultiLanguageString mls,
            string kanjiIn,
            string kanaIn,
            string romajiIn,
            string kanjiOut,
            string kanaOut,
            string romajiOut)
        {
            string kanji = mls.Text(JapaneseID);
            string kana = mls.Text(JapaneseKanaID);
            string romaji = mls.Text(JapaneseRomajiID);
            bool returnValue = true;
            if (kanji.EndsWith(kanjiIn))
                mls.SetText(JapaneseID, kanji.Substring(0, kanjiIn.Length) + kanjiOut);
            else
                returnValue = false;
            if (kana.EndsWith(kanaIn))
                mls.SetText(JapaneseKanaID, kana.Substring(0, kanaIn.Length) + kanaOut);
            romaji = romaji.Replace(" ", "");
            if (romaji.EndsWith(romajiIn))
                mls.SetText(JapaneseRomajiID, romaji.Substring(0, romajiIn.Length) + romajiOut);
            return returnValue;
        }

        protected bool Replace(
            MultiLanguageString mls,
            string kanjiIn,
            string kanaIn,
            string romajiIn,
            string kanjiOut,
            string kanaOut,
            string romajiOut)
        {
            string kanji = mls.Text(JapaneseID);
            string kana = mls.Text(JapaneseKanaID);
            string romaji = mls.Text(JapaneseRomajiID);
            bool returnValue = true;
            if (kanji.Contains(kanjiIn))
                mls.SetText(JapaneseID, kanji.Replace(kanjiIn, kanjiOut));
            else
                returnValue = false;
            if (kana.Contains(kanaIn))
                mls.SetText(JapaneseKanaID, kana.Replace(kanaIn, kanaOut));
            romaji = romaji.Replace(" ", "");
            if (romaji.Contains(romajiIn))
                mls.SetText(JapaneseRomajiID, romaji.Replace(romajiIn, romajiOut));
            return returnValue;
        }

        protected void RemoveLastCharacter(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveFirstCharacter(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveOPrefix(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (!inText.StartsWith("お"))
                return;
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveOPrefix(Inflection inflected)
        {
            RemoveOPrefix(inflected.Root);
            RemoveOPrefix(inflected.DictionaryForm);
            RemoveOPrefix(inflected.Output);
            RemoveOPrefix(inflected.Prefix);
        }

        protected void FixupSuffix(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString root = inflected.Root;

            foreach (LanguageID languageID in JapaneseLanguageIDs)
            {
                int offset = prefix.Text(languageID).Length + root.Text(languageID).Length;
                suffix.SetText(languageID, output.Text(languageID).Substring(offset));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class LanguageTool : BaseObjectLanguage
    {
        public virtual bool Transliterate(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID,
            out string output,
            List<TextRun> wordRuns)     // May be null to skip getting word runs.
        {
            List<TextGraphNode> nodes;
            StringBuilder sb = new StringBuilder();
            int textIndex = 0;
            bool returnValue = Parse(input, inputLanguageID, out nodes);

            foreach (TextGraphNode node in nodes)
            {
                if (textIndex > node.Start)
                    throw new Exception("Oops, parsed nodes in Transliterate are out of sync.");

                if (node.Start > textIndex)
                {
                    string separatorsInput = input.Substring(textIndex, node.Start - textIndex);
                    string separatorsOutput = TransliterateSeparatorsOrUnknown(
                        separatorsInput,
                        inputLanguageID,
                        outputLanguageID,
                        sb,
                        true);
                    sb.Append(separatorsOutput);
                    textIndex += separatorsInput.Length;
                }
                else if ((textIndex != 0) && LanguageLookup.IsRomanized(outputLanguageID))
                    sb.Append(" ");

                DictionaryEntry entry = node.Entry;
                string transliteratedItem;

                if (entry == null)
                {
                    string unknownText = input.Substring(node.Start, node.Length);
                    transliteratedItem = TransliterateUnknown(
                        unknownText,
                        inputLanguageID,
                        outputLanguageID);
                }
                else
                    transliteratedItem = entry.GetTranslation(outputLanguageID);

                if (wordRuns != null)
                {
                    TextRun wordRun = new Content.TextRun(sb.Length, transliteratedItem.Length, null);
                    wordRuns.Add(wordRun);
                }

                sb.Append(transliteratedItem);
                textIndex += node.Length;
            }

            if (textIndex < input.Length)
            {
                string separatorsInput = input.Substring(textIndex, input.Length - textIndex);
                string separatorsOutput = TransliterateSeparatorsOrUnknown(
                    separatorsInput,
                    inputLanguageID,
                    outputLanguageID,
                    sb,
                    false);
                sb.Append(separatorsOutput);
            }
            else if ((textIndex != 0) && LanguageLookup.IsRomanized(outputLanguageID))
                sb.Append(" ");

            output = sb.ToString();

            return returnValue;
        }

        public virtual string TransliterateSeparatorsOrUnknown(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID,
            StringBuilder sb,
            bool isNotEnd)
        {
            return TransliterateSeparators(input, inputLanguageID, outputLanguageID, sb, isNotEnd);
        }

        public virtual string TransliterateSeparators(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID,
            StringBuilder sb,
            bool isNotEnd)
        {
            string output;

            if (LanguageLookup.IsRomanized(outputLanguageID))
            {
                output = LanguageLookup.ToRomanizedSpaceOrPunctuationString(input);
                output = TransliterateSeparatorsExpandCheck(output, sb, isNotEnd);
            }
            else
                output = input;

            return output;
        }

        public virtual string TransliterateSeparatorsExpandCheck(
            string input,
            StringBuilder sb,
            bool isNotEnd)
        {
            if (input.Length == 0)
                return input;

            string output = input;

            if (sb.Length != 0)
            {
                char firstChr = output[0];

                if (LanguageLookup.NeedSpaceBeforeCharacters.Contains(firstChr))
                    output = " " + output;
                else if (firstChr == '"')
                {
                    int i = sb.Length - 1;
                    int count = 0;

                    for (; i >= 0; i--)
                    {
                        if (sb[i] == '"')
                            count++;
                    }

                    if ((count & 1) == 0)
                        output = " " + output;
                }
                else if (firstChr == '\'')
                {
                    int i = sb.Length - 1;
                    int count = 0;

                    for (; i >= 0; i--)
                    {
                        if (sb[i] == '\'')
                            count++;
                    }

                    if ((count & 1) == 0)
                        output = " " + output;
                }
            }

            if (isNotEnd)
            {
                char lastChr = output[output.Length - 1];

                if (LanguageLookup.NeedSpaceAfterCharacters.Contains(lastChr))
                    output += " ";
                else if (lastChr == '"')
                {
                    int i = sb.Length - 1;
                    int count = 0;

                    for (; i >= 0; i--)
                    {
                        if (sb[i] == '"')
                            count++;
                    }

                    if ((count & 1) != 0)
                        output += " ";
                }
                else if (lastChr == '\'')
                {
                    int i = sb.Length - 1;
                    int count = 0;

                    for (; i >= 0; i--)
                    {
                        if (sb[i] == '\'')
                            count++;
                    }

                    if ((count & 1) != 0)
                        output += " ";
                }
            }

            return output;
        }

        public virtual string TransliterateUnknown(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID)
        {
            string output;
            // Do nothing by default.
            output = input;
            //output = "X" + input + "X";
            return output;
        }

        public virtual bool TransliterateMultiLanguageString(
            MultiLanguageString multiLanguageString,
            bool force)
        {
            LanguageID sourceLanguageID = null;
            LanguageString sourceLanguageString = null;
            int sourceLanguageIndex = 0;

            foreach (LanguageID lid in TargetLanguageIDs)
            {
                sourceLanguageString = multiLanguageString.LanguageString(lid);

                if ((sourceLanguageString != null) && sourceLanguageString.HasText())
                {
                    sourceLanguageID = lid;
                    break;
                }

                sourceLanguageIndex++;
            }

            if (sourceLanguageString == null)
                return false;

            string sourceText = sourceLanguageString.Text;
            List<TextGraphNode> nodes;
            bool returnValue = Parse(sourceText, sourceLanguageID, out nodes);

            for (int languageIndex = sourceLanguageIndex + 1; languageIndex < TargetLanguageIDs.Count(); languageIndex++)
            {
                LanguageID targetLanguageID = TargetLanguageIDs[languageIndex];
                LanguageString targetLanguageString = multiLanguageString.LanguageString(targetLanguageID);

                if (targetLanguageString == null)
                    continue;

                if (!force && targetLanguageString.HasText())
                    continue;

                StringBuilder sb = new StringBuilder();
                int textIndex = 0;

                foreach (TextGraphNode node in nodes)
                {
                    if (textIndex > node.Start)
                        throw new Exception("Oops, parsed nodes in Transliterate are out of sync.");

                    if (node.Start > textIndex)
                    {
                        string separatorsInput = sourceText.Substring(textIndex, node.Start - textIndex);
                        string separatorsOutput = TransliterateSeparatorsOrUnknown(
                            separatorsInput,
                            sourceLanguageID,
                            targetLanguageID,
                            sb,
                            true);
                        sb.Append(separatorsOutput);
                    }
                    else if ((textIndex != 0) && LanguageLookup.IsRomanized(targetLanguageID))
                        sb.Append(" ");

                    DictionaryEntry entry = node.Entry;
                    string sourceString = sourceText.Substring(node.Start, node.Length);
                    string transliteratedString;

                    if (entry == null)
                    {
                        transliteratedString = TransliterateUnknown(
                            sourceString,
                            sourceLanguageID,
                            targetLanguageID);
                    }
                    else
                        transliteratedString = entry.GetTranslation(targetLanguageID);

                    if (LanguageLookup.IsRomanized(targetLanguageID))
                    {
                        transliteratedString = FixupTransliteration(
                            transliteratedString,
                            targetLanguageID,
                            sourceString,
                            sourceLanguageID,
                            true);
                    }

                    transliteratedString = FixUpText(targetLanguageID, transliteratedString);

                    sb.Append(transliteratedString);
                    textIndex += node.Length;
                }

                if (textIndex < sourceText.Length)
                {
                    string separatorsInput = sourceText.Substring(textIndex, sourceText.Length - textIndex);
                    string separatorsOutput = TransliterateSeparatorsOrUnknown(
                        separatorsInput,
                        sourceLanguageID,
                        targetLanguageID,
                        sb,
                        false);
                    sb.Append(separatorsOutput);
                }

                targetLanguageString.Text = sb.ToString();
            }

            return returnValue;
        }

        public virtual bool TransliterateMultiLanguageItem(
            MultiLanguageItem multiLanguageItem,
            bool force)
        {
            LanguageID sourceLanguageID = null;
            LanguageItem sourceLanguageItem = null;
            int sourceLanguageIndex = 0;

            foreach (LanguageID lid in TargetLanguageIDs)
            {
                sourceLanguageItem = multiLanguageItem.LanguageItem(lid);

                if ((sourceLanguageItem != null) && sourceLanguageItem.HasText())
                {
                    sourceLanguageID = lid;
                    break;
                }

                sourceLanguageIndex++;
            }

            if (sourceLanguageItem == null)
                return false;

            string sourceText = sourceLanguageItem.Text;
            List<TextRun> sourceWordRuns = sourceLanguageItem.WordRuns;
            List<TextGraphNode> nodes;
            bool returnValue;

            InitializeWordFixesCheck(null);

            if ((sourceWordRuns != null) && (sourceWordRuns.Count() != 0))
                returnValue = ParseMatchingWordRuns(sourceText, sourceLanguageID, sourceWordRuns, out nodes);
            else
            {
                returnValue = ParseAndGetWordRuns(sourceText, sourceLanguageID, out nodes, out sourceWordRuns);
                sourceLanguageItem.WordRuns = sourceWordRuns;
            }

            sourceLanguageItem.SentenceRunCheck();

            for (int languageIndex = sourceLanguageIndex + 1; languageIndex < TargetLanguageIDs.Count(); languageIndex++)
            {
                LanguageID targetLanguageID = TargetLanguageIDs[languageIndex];
                LanguageItem targetLanguageItem = multiLanguageItem.LanguageItem(targetLanguageID);

                if (targetLanguageItem == null)
                    continue;

                if (!force && targetLanguageItem.HasText() && targetLanguageItem.HasWordRuns())
                    continue;

                StringBuilder sb = new StringBuilder();
                int textIndex = 0;
                List<TextRun> targetWordRuns = targetLanguageItem.WordRuns;

                if (targetWordRuns != null)
                    targetWordRuns.Clear();
                else
                    targetWordRuns = new List<TextRun>();

                foreach (TextGraphNode node in nodes)
                {
                    if (textIndex > node.Start)
                        throw new Exception("Oops, parsed nodes in Transliterate are out of sync.");

                    if (node.Start > textIndex)
                    {
                        string separatorsInput = sourceText.Substring(textIndex, node.Start - textIndex);
                        string separatorsOutput = TransliterateSeparatorsOrUnknown(
                            separatorsInput,
                            sourceLanguageID,
                            targetLanguageID,
                            sb,
                            true);
                        sb.Append(separatorsOutput);
                        textIndex += separatorsInput.Length;
                    }
                    else if ((textIndex != 0) && LanguageLookup.IsRomanized(targetLanguageID))
                        sb.Append(" ");

                    DictionaryEntry entry = node.Entry;
                    string sourceItem = sourceText.Substring(node.Start, node.Length);
                    string transliteratedItem;

                    if (entry == null)
                    {
                        transliteratedItem = TransliterateUnknown(
                            sourceItem,
                            sourceLanguageID,
                            targetLanguageID);
                    }
                    else
                        transliteratedItem = entry.GetTranslationNonStem(targetLanguageID);

                    transliteratedItem = FixupTransliteration(
                        transliteratedItem,
                        targetLanguageID,
                        sourceItem,
                        sourceLanguageID,
                        true);

                    transliteratedItem = FixUpText(targetLanguageID, transliteratedItem);

                    TextRun targetWordRun = new Content.TextRun(sb.Length, transliteratedItem.Length, null);
                    targetWordRuns.Add(targetWordRun);

                    sb.Append(transliteratedItem);
                    textIndex += node.Length;
                }

                if (textIndex < sourceText.Length)
                {
                    string separatorsInput = sourceText.Substring(textIndex, sourceText.Length - textIndex);
                    string separatorsOutput = TransliterateSeparatorsOrUnknown(
                        separatorsInput,
                        sourceLanguageID,
                        targetLanguageID,
                        sb,
                        false);
                    sb.Append(separatorsOutput);
                }

                targetLanguageItem.Text = sb.ToString();
                targetLanguageItem.WordRuns = targetWordRuns;
                targetLanguageItem.ReDoSentenceRuns(sourceLanguageItem);
            }

            return returnValue;
        }

        public virtual bool TransliterateLanguageItem(
            MultiLanguageItem multiLanguageItem,
            LanguageItem targetLanguageItem,
            bool force)
        {
            if (targetLanguageItem == null)
                return false;

            if (!force && targetLanguageItem.HasText() && targetLanguageItem.HasWordRuns())
                return false;

            LanguageID sourceLanguageID = null;
            LanguageItem sourceLanguageItem = null;
            LanguageID targetLanguageID = targetLanguageItem.LanguageID;
            int sourceLanguageIndex = 0;

            if (LanguageLookup.IsRomanized(targetLanguageID))
            {
                foreach (LanguageID lid in TargetLanguageIDs)
                {
                    if (LanguageLookup.IsNonRomanizedAlternatePhonetic(lid))
                    {
                        sourceLanguageItem = multiLanguageItem.LanguageItem(lid);

                        if ((sourceLanguageItem != null) && sourceLanguageItem.HasText())
                        {
                            sourceLanguageID = lid;
                            break;
                        }
                    }
                }
            }

            if (sourceLanguageID == null)
            {
                foreach (LanguageID lid in TargetLanguageIDs)
                {
                    sourceLanguageItem = multiLanguageItem.LanguageItem(lid);

                    if ((sourceLanguageItem != null) && sourceLanguageItem.HasText())
                    {
                        sourceLanguageID = lid;
                        break;
                    }

                    sourceLanguageIndex++;
                }
            }

            if (sourceLanguageItem == null)
                return false;

            string sourceText = sourceLanguageItem.Text;
            List<TextRun> sourceWordRuns = sourceLanguageItem.WordRuns;
            List<TextGraphNode> nodes;
            bool returnValue;

            InitializeWordFixesCheck(null);

            if ((sourceWordRuns != null) && (sourceWordRuns.Count() != 0))
                returnValue = ParseMatchingWordRuns(sourceText, sourceLanguageID, sourceWordRuns, out nodes);
            else
            {
                returnValue = ParseAndGetWordRuns(sourceText, sourceLanguageID, out nodes, out sourceWordRuns);
                sourceLanguageItem.WordRuns = sourceWordRuns;
            }

            sourceLanguageItem.SentenceRunCheck();

            StringBuilder sb = new StringBuilder();
            int textIndex = 0;
            List<TextRun> targetWordRuns = targetLanguageItem.WordRuns;

            if (targetWordRuns != null)
                targetWordRuns.Clear();
            else
                targetWordRuns = new List<TextRun>();

            foreach (TextGraphNode node in nodes)
            {
                if (textIndex > node.Start)
                    throw new Exception("Oops, parsed nodes in Transliterate are out of sync.");

                if (node.Start > textIndex)
                {
                    string separatorsInput = sourceText.Substring(textIndex, node.Start - textIndex);
                    string separatorsOutput = TransliterateSeparatorsOrUnknown(
                        separatorsInput,
                        sourceLanguageID,
                        targetLanguageID,
                        sb,
                        true);
                    sb.Append(separatorsOutput);
                    textIndex += separatorsInput.Length;
                }
                else if ((textIndex != 0) && LanguageLookup.IsRomanized(targetLanguageID))
                    sb.Append(" ");

                DictionaryEntry entry = node.Entry;
                string sourceItem = sourceText.Substring(node.Start, node.Length);
                string transliteratedItem;

                if (entry == null)
                {
                    transliteratedItem = TransliterateUnknown(
                        sourceItem,
                        sourceLanguageID,
                        targetLanguageID);
                }
                else
                    transliteratedItem = entry.GetTranslationNonStem(targetLanguageID);

                transliteratedItem = FixupTransliteration(
                    transliteratedItem,
                    targetLanguageID,
                    sourceItem,
                    sourceLanguageID,
                    true);

                transliteratedItem = FixUpText(targetLanguageID, transliteratedItem);

                TextRun targetWordRun = new Content.TextRun(sb.Length, transliteratedItem.Length, null);
                targetWordRuns.Add(targetWordRun);

                sb.Append(transliteratedItem);
                textIndex += node.Length;
            }

            if (textIndex < sourceText.Length)
            {
                string separatorsInput = sourceText.Substring(textIndex, sourceText.Length - textIndex);
                string separatorsOutput = TransliterateSeparatorsOrUnknown(
                    separatorsInput,
                    sourceLanguageID,
                    targetLanguageID,
                    sb,
                    false);
                sb.Append(separatorsOutput);
            }

            targetLanguageItem.Text = sb.ToString();
            targetLanguageItem.WordRuns = targetWordRuns;
            targetLanguageItem.ReDoSentenceRuns(sourceLanguageItem);

            return returnValue;
        }

        public static void FixupRomanizedSpacesUsingWordRuns(LanguageItem languageItem)
        {
            if ((languageItem == null) || !languageItem.HasText())
                return;

            List<TextRun> wordRuns = languageItem.WordRuns;

            if ((wordRuns == null) || (wordRuns.Count() == 0))
                return;

            int runCount = wordRuns.Count();
            int runIndex;
            int textOffset = 0;
            string text = languageItem.Text;
            int textLength = text.Length;

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                TextRun wordRun = wordRuns[runIndex];

                if (textOffset != 0)
                    wordRun.Start = wordRun.Start + textOffset;

                int nextCharIndex = wordRun.Stop;

                if (nextCharIndex < textLength)
                {
                    char nextChar = text[nextCharIndex];

                    if (!char.IsWhiteSpace(nextChar) && !char.IsPunctuation(nextChar))
                    {
                        text = text.Insert(nextCharIndex, " ");
                        textOffset++;
                        textLength++;
                    }
                }
            }

            languageItem.Text = text;
        }

        public virtual string TranslateNumber(LanguageID languageID, string text)
        {
            return text;
        }

        public virtual string FixUpText(LanguageID languageID, string text)
        {
            return text;
        }

        public string[] RomanizationFixupTableOverride = null;

        public virtual string FixupTransliteration(
            string transliteration,
            LanguageID transliterationLanguageID,
            string nonTransliteration,
            LanguageID nonTransliterationLanguageID,
            bool isWord)
        {
            return transliteration;
        }

        public virtual string CharacterConvertLanguageText(string text, LanguageID languageID)
        {
            return text;
        }

        public virtual List<string> GetCharacterRomanizedStrings(string targetCharStr)
        {
            LanguageID romanizedID = RomanizedLanguageID;

            if (romanizedID == null)
                return null;

            if (IsRomanizedChar(targetCharStr[0]))
                return new List<string>(1) { targetCharStr };
            else if (IsAlternateTransliterationChar(targetCharStr[0]))
            {
                string romanized;

                if (ConvertAlternateToRomanized(targetCharStr, out romanized))
                    return new List<string>(1) { romanized };

                return new List<string>(1) { targetCharStr };
            }
            else
            {
                bool isInflection;
                DictionaryEntry entry = LookupDictionaryEntry(
                    targetCharStr,
                    MatchCode.Exact,
                    RootLanguageID,
                    null,
                    out isInflection);

                if (entry != null)
                {
                    if (entry.HasAlternates())
                    {
                        List<string> romanizedStrings = new List<string>();

                        foreach (LanguageString alternate in entry.Alternates)
                        {
                            if (alternate.LanguageID == romanizedID)
                                romanizedStrings.Add(alternate.Text);
                        }

                        return romanizedStrings;
                    }
                }
            }

            return null;
        }

        public virtual bool GetCharacterRomanizedAndAlternateStrings(
            string targetCharStr,
            List<string> romanizedStrings,
            List<string> alternateStrings)
        {
            LanguageID romanizedID = RomanizedLanguageID;
            LanguageID alternateID = NonRomanizedPhoneticLanguageID;

            if (romanizedID == null)
                return false;

            if (IsRomanizedChar(targetCharStr[0]))
            {
                romanizedStrings.Add(targetCharStr);
                alternateStrings.Add(targetCharStr);
                return true;
            }
            else if (IsAlternateTransliterationChar(targetCharStr[0]))
            {
                string romanized;

                alternateStrings.Add(targetCharStr);

                if (ConvertAlternateToRomanized(targetCharStr, out romanized))
                    romanizedStrings.Add(romanized);
                else
                    return false;

                return true;
            }
            else
            {
                bool isInflection;
                DictionaryEntry entry = LookupDictionaryEntry(
                    targetCharStr,
                    MatchCode.Exact,
                    RootLanguageID,
                    null,
                    out isInflection);

                if (entry != null)
                {
                    if (entry.HasAlternates())
                    {
                        foreach (LanguageString alternate in entry.Alternates)
                        {
                            if (alternate.LanguageID == romanizedID)
                                romanizedStrings.Add(alternate.Text);
                            else if (alternate.LanguageID == alternateID)
                                alternateStrings.Add(alternate.Text);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool IsRomanizedChar(char chr)
        {
            if (chr <= '~')
                return true;

            return false;
        }

        public virtual bool IsAlternateTransliterationChar(char chr)
        {
            return false;
        }

        public virtual bool IsRomanizedDigitString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            foreach (char chr in str)
            {
                if (!char.IsDigit(chr))
                {
                    switch (chr)
                    {
                        case 'e':
                        case 'E':
                        case '-':
                            break;
                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        public virtual bool ConvertNumberToRomanized(string text, out string romanized)
        {
            romanized = null;
            return false;
        }

        public virtual bool ConvertAlternateToRomanized(string text, out string romanized)
        {
            romanized = null;
            return false;
        }
    }
}

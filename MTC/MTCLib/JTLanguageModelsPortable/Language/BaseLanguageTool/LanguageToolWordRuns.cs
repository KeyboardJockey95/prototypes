using System;
using System.Collections.Generic;
using System.IO;
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
        protected SentenceFixes _SentenceFixes;
        protected WordFixes _WordFixes;

        public virtual void GetMultiLanguageItemSentenceAndWordRuns(
            MultiLanguageItem multiLanguageItem,
            bool canUseRomanizationAsReference)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.Count() == 0))
                return;

            GetMultiLanguageItemSentenceRuns(multiLanguageItem);
            GetMultiLanguageItemWordRuns(multiLanguageItem, canUseRomanizationAsReference);
        }

        public virtual void GetMultiLanguageItemSentenceRuns(MultiLanguageItem multiLanguageItem)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.Count() == 0))
                return;

            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem != null)
                    languageItem.LoadSentenceRunsFromText();
            }
        }

        public virtual void GetMultiLanguageItemWordRuns(
            MultiLanguageItem multiLanguageItem,
            bool canUseRomanizationAsReference)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.Count() == 0))
                return;

            if (canUseRomanizationAsReference)
            {
                try
                {
                    GetMultiLanguageItemWordRunsRomanizationAsReference(multiLanguageItem);
                    return;
                }
                catch (Exception)
                {
                }
            }

            try
            {
                GetMultiLanguageItemWordRunsRootAsReference(multiLanguageItem);
                return;
            }
            catch (Exception)
            {
            }
        }

        public virtual void GetMultiLanguageItemWordRunsRootAsReference(MultiLanguageItem multiLanguageItem)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.Count() == 0))
                return;

            LanguageItem languageItem = null;

            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem != null)
                    break;
            }

            if (languageItem == null)
                return;

            LanguageID baseLanguageID = languageItem.LanguageID;

            bool isSpacedLanguage = !LanguageLookup.GetLanguageDescription(baseLanguageID).CharacterBased;

            if (isSpacedLanguage)
            {
                if (languageItem.WordRuns == null)
                    languageItem.WordRuns = new List<TextRun>();

                GetSpacedWordRuns(languageItem.Text, languageItem.WordRuns);

                return;
            }

            TextGraph textGraph = new TextGraph(null, baseLanguageID, languageItem.Text, this);

            if (!textGraph.ParseBest())
            {
                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    languageItem = multiLanguageItem.LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    languageItem.LoadWordRunsFromText(DictionaryDatabase);
                }

                return;
            }

            List<TextGraphNode> path = textGraph.Path;

            languageItem.WordRuns = new List<TextRun>();
            TextGraph.GetRunsFromPath(path, languageItem.WordRuns);

            int languageCount = TargetLanguageIDs.Count();
            int languageIndex;

            for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
            {
                LanguageID languageID = TargetLanguageIDs[languageIndex];
                LanguageItem subLanguageItem = multiLanguageItem.LanguageItem(languageID);

                if (subLanguageItem == null)
                    continue;

                if (GetLanguageItemWordRunsSynchronizedFromPath(subLanguageItem, baseLanguageID, path))
                {
                    if (LanguageLookup.IsRomanized(languageID))
                        FixupRomanizedSpacesUsingWordRuns(subLanguageItem);
                }
                else if (!LanguageLookup.GetLanguageDescription(languageID).CharacterBased)
                {
                    subLanguageItem.WordRuns = new List<TextRun>();
                    GetSpacedWordRuns(subLanguageItem.Text, subLanguageItem.WordRuns);
                }
            }
        }

        public virtual void GetMultiLanguageItemWordRunsRomanizationAsReference(MultiLanguageItem multiLanguageItem)
        {
            if ((multiLanguageItem == null) || (multiLanguageItem.Count() == 0))
                return;

            LanguageID romanizedID = RomanizedLanguageID;

            if (romanizedID == null)
            {
                GetMultiLanguageItemWordRunsRootAsReference(multiLanguageItem);
                return;
            }

            LanguageItem romanizedItem = multiLanguageItem.LanguageItem(romanizedID);

            if ((romanizedItem == null) || !romanizedItem.HasText())
            {
                GetMultiLanguageItemWordRunsRootAsReference(multiLanguageItem);
                return;
            }

            List<TextRun> romanizedWordRuns = romanizedItem.WordRuns;

            if (!romanizedItem.HasWordRuns())
            {
                romanizedItem.WordRuns = romanizedWordRuns = new List<TextRun>();
                GetSpacedWordRuns(romanizedItem.Text, romanizedWordRuns);
            }

            LanguageID rootID = RootLanguageID;
            LanguageID alternateID = NonRomanizedPhoneticLanguageID;
            LanguageItem rootLanguageItem = multiLanguageItem.LanguageItem(rootID);
            LanguageItem alternateLanguageItem = multiLanguageItem.LanguageItem(alternateID);
            List<TextRun> rootWordRuns = null;
            List<TextRun> alternateWordRuns = null;
            string rootText = null;
            string alternateText = null;
            string alternateTransliteration;

            if (rootLanguageItem != null)
            {
                rootText = rootLanguageItem.Text;
                rootWordRuns = rootLanguageItem.WordRuns;

                if (rootWordRuns == null)
                    rootLanguageItem.WordRuns = rootWordRuns = new List<TextRun>();
            }

            if (alternateLanguageItem != null)
            {
                alternateText = alternateLanguageItem.Text;
                alternateWordRuns = alternateLanguageItem.WordRuns;

                if (alternateWordRuns == null)
                    alternateLanguageItem.WordRuns = alternateWordRuns = new List<TextRun>();
            }

            if (!GetSynchronizedWordRuns(
                    rootText,
                    rootWordRuns,
                    alternateText,
                    alternateWordRuns,
                    romanizedItem.Text,
                    romanizedWordRuns,
                    out alternateTransliteration))
                GetMultiLanguageItemWordRunsRootAsReference(multiLanguageItem);
            else if ((alternateLanguageItem != null) && String.IsNullOrEmpty(alternateText))
                alternateLanguageItem.Text = alternateTransliteration;
        }

        public virtual void GetSpacedWordRuns(string text, List<TextRun> wordRuns)
        {
            string runText;
            TextRun textRun;
            int runCount = wordRuns.Count();
            int runIndex = 0;
            int charIndex;
            int startIndex = 0;
            int runLength;
            int textLength = text.Length;
            char chr;

            for (charIndex = 0; charIndex <= textLength;)
            {
                if (charIndex < textLength)
                    chr = text[charIndex];
                else
                    chr = '\n';

                if ((charIndex == textLength) ||
                    LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                    char.IsPunctuation(chr))
                {
                    int skipOffset;

                    if (chr == '.')
                    {
                        if (CheckForAbbreviation(text, charIndex, chr, out skipOffset))
                            charIndex += skipOffset;
                    }
                    else if ((chr == '\'') || (chr == '‘') || (chr == '’'))
                    {
                        if (CheckForApostrophe(text, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }
                    }

                    runLength = charIndex - startIndex;

                    if (runLength > 0)
                    {
                        runText = text.Substring(startIndex, runLength);

                        if (runIndex == wordRuns.Count())
                        {
                            textRun = new TextRun(startIndex, runLength, null);
                            wordRuns.Add(textRun);
                            runCount++;
                        }
                        else
                        {
                            textRun = wordRuns[runIndex];
                            textRun.Start = startIndex;
                            textRun.Length = runLength;
                        }

                        runIndex++;
                    }

                    // Skip to next word.
                    while ((charIndex < textLength) &&
                            (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[charIndex]) ||
                                char.IsPunctuation(text[charIndex])))
                        charIndex++;

                    startIndex = charIndex;

                    if (charIndex == textLength)
                        break;
                }
                else
                    charIndex++;
            }

            if (runIndex < runCount)
            {
                while (runCount > runIndex)
                {
                    runCount--;
                    wordRuns.RemoveAt(runCount);
                }
            }
        }

        // Synchronize root and optionally non-romanized-phonetic alternate word runs to Romanized.
        public virtual bool GetSynchronizedWordRuns(
            string rootText,
            List<TextRun> rootWordRuns,
            string alternateText,
            List<TextRun> alternateWordRuns,
            string romanizedText,
            List<TextRun> romanizedWordRuns,
            out string alternateTransliteration)
        {
            int runCount = romanizedWordRuns.Count();
            int runIndex;
            int runAheadIndex;
            string romanizedWord;
            TextRun romanizedRun;
            string rootWord;
            DictionaryEntry rootEntry;
            int rootIndex = 0;
            int rootCount = rootText.Length;
            int rootNotFoundIndex = 0;
            string alternateWord;
            string alternateWordTransliteration;
            int reading;
            DictionaryEntry alternateEntry;
            int alternateIndex = 0;
            int alternateCount = (!String.IsNullOrEmpty(alternateText) ? alternateText.Length : 0);
            int alternateNotFoundIndex = 0;
            int maxRootLength = Description.LongestDictionaryEntryLength;
            int maxAlternateLength = alternateCount;
            List<DictionaryEntry> rootEntries = new List<DictionaryEntry>();
            LanguageID rootID = RootLanguageID;
            LanguageID alternateID = NonRomanizedPhoneticLanguageID;
            LanguageID romanizedID = RomanizedLanguageID;
            bool returnValue = true;

            if (maxRootLength <= 0)
                maxRootLength = rootCount;
            else if (maxRootLength > rootCount)
                maxRootLength = rootCount;

            alternateTransliteration = String.Empty;

            if (alternateID != null)
            {
                maxAlternateLength = LanguageLookup.GetLanguageDescription(alternateID).LongestDictionaryEntryLength;

                if (maxAlternateLength <= 0)
                    maxAlternateLength = alternateCount;
                else if (maxAlternateLength > alternateCount)
                    maxAlternateLength = alternateCount;
            }

            if (String.IsNullOrEmpty(rootText))
            {
                if (String.IsNullOrEmpty(alternateText))
                    return true;

                rootText = alternateText;
                rootWordRuns = alternateWordRuns;
                rootCount = alternateCount;
                alternateText = String.Empty;
                alternateCount = 0;
            }

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                romanizedRun = romanizedWordRuns[runIndex];
                romanizedWord = romanizedText.Substring(romanizedRun.Start, romanizedRun.Length);
                rootEntry = null;
                alternateWord = null;
                alternateEntry = null;

                while ((rootIndex < rootCount) && IsWhiteSpaceOrPunctuation(rootText[rootIndex]))
                {
                    if ((alternateID != null) && (alternateCount == 0))
                    {
                        alternateTransliteration += rootText[rootIndex];
                        alternateIndex++;
                    }

                    rootIndex++;
                }

                if (rootIndex >= rootCount)
                    break;

                if (FindRootRunFromRomanized(
                    romanizedWord,
                    rootText,
                    ref rootIndex,
                    rootCount,
                    maxRootLength,
                    out rootWord,
                    out rootEntry,
                    out reading,
                    out alternateWordTransliteration))
                {
                    if (alternateID != null)
                    {
                        if (alternateCount != 0)
                        {
                            if (!FindAlternateRunFromRomanized(
                                    romanizedWord,
                                    rootEntry,
                                    alternateText,
                                    ref alternateIndex,
                                    alternateCount,
                                    maxAlternateLength,
                                    out alternateWord,
                                    out alternateEntry))
                            {
                                throw new Exception("Alternate not found for " + romanizedWord);
                                //continue;
                            }
                        }
                        else
                        {
                            if (alternateIndex != alternateTransliteration.Length)
                                throw new Exception("alternateIndex != alternateTransliteration.Length");

                            alternateTransliteration += alternateWordTransliteration;
                            alternateWord = alternateWordTransliteration;
                        }

                        AddOrUpdateRun(runIndex, alternateWordRuns, alternateWord, ref alternateIndex);
                    }

                    AddOrUpdateRun(runIndex, rootWordRuns, rootWord, ref rootIndex);
                }
                else if (runIndex == runCount - 1)
                {
                    int rootNextIndex = rootIndex;

                    if (ParseWordString(rootText, rootIndex, rootCount, out rootWord, ref rootNextIndex))
                    {
                        AddOrUpdateRun(runIndex, rootWordRuns, rootWord, ref rootIndex);

                        if ((alternateID != null) && !String.IsNullOrEmpty(alternateText))
                        {
                            if (alternateCount != 0)
                            {
                                int alternateNextIndex = alternateIndex;

                                if (ParseWordString(alternateText, alternateIndex, alternateCount, out alternateWord, ref alternateNextIndex))
                                    AddOrUpdateRun(runIndex, alternateWordRuns, alternateWord, ref alternateIndex);
                            }
                            else
                            {
                                alternateWord = rootWord;
                                alternateTransliteration += alternateWord;
                                AddOrUpdateRun(runIndex, alternateWordRuns, alternateWord, ref alternateIndex);

                                if (rootNextIndex < rootCount)
                                    alternateTransliteration += rootText.Substring(rootIndex, rootCount - rootNextIndex);
                            }
                        }
                    }
                }
                else
                {
                    bool rootFound = false;

                    rootNotFoundIndex = rootIndex;

                    for (runAheadIndex = runIndex + 1;
                        !rootFound && (runAheadIndex < runCount);
                        runAheadIndex++)
                    {
                        romanizedRun = romanizedWordRuns[runAheadIndex];
                        romanizedWord = romanizedText.Substring(romanizedRun.Start, romanizedRun.Length);
                        rootIndex = rootNotFoundIndex;

                        for (rootIndex++; !rootFound && (rootIndex < rootCount);)
                        {
                            if (FindRootRunFromRomanized(
                                romanizedWord,
                                rootText,
                                ref rootIndex,
                                rootCount,
                                maxRootLength,
                                out rootWord,
                                out rootEntry,
                                out reading,
                                out alternateWordTransliteration))
                            {
                                if ((alternateID != null) && !String.IsNullOrEmpty(alternateText))
                                {
                                    if (alternateCount != 0)
                                    {
                                        for (alternateIndex++; (alternateEntry == null) && (alternateIndex < alternateCount);)
                                        {
                                            if (!FindAlternateRunFromRomanized(
                                                    romanizedWord,
                                                    rootEntry,
                                                    alternateText,
                                                    ref alternateIndex,
                                                    alternateCount,
                                                    maxAlternateLength,
                                                    out alternateWord,
                                                    out alternateEntry))
                                                alternateIndex++;
                                        }

                                        if (alternateEntry == null)
                                            throw new Exception("Alternate not found for " + romanizedWord);
                                    }
                                    else
                                        alternateWord = alternateWordTransliteration;
                                }

                                HandleNotFoundRuns(
                                    ref runIndex,
                                    ref runCount,
                                    runAheadIndex,
                                    rootNotFoundIndex,
                                    rootIndex,
                                    rootText,
                                    rootWordRuns,
                                    alternateNotFoundIndex,
                                    ref alternateIndex,
                                    alternateText,
                                    alternateWordRuns,
                                    romanizedText,
                                    romanizedWordRuns,
                                    ref alternateTransliteration);

                                AddOrUpdateRun(runIndex, rootWordRuns, rootWord, ref rootIndex);
                                rootFound = true;
                                break;
                            }
                            else
                                rootIndex++;
                        }
                    }

                    if (!rootFound)
                    {
                        rootIndex = rootText.Length;
                        HandleNotFoundRuns(
                            ref runIndex,
                            ref runCount,
                            runAheadIndex,
                            rootNotFoundIndex,
                            rootIndex,
                            rootText,
                            rootWordRuns,
                            alternateNotFoundIndex,
                            ref alternateIndex,
                            alternateText,
                            alternateWordRuns,
                            romanizedText,
                            romanizedWordRuns,
                            ref alternateTransliteration);
                    }
                }
            }

            if (rootWordRuns.Count() > runCount)
                rootWordRuns.RemoveRange(runIndex, rootWordRuns.Count() - runCount);

            if ((alternateWordRuns != null) && (alternateWordRuns.Count() > runCount))
                alternateWordRuns.RemoveRange(runIndex, alternateWordRuns.Count() - runCount);

            if (alternateID != null)
            {
                if (alternateCount == 0)
                {
                    if (rootIndex < rootCount)
                    {
                        int index = rootIndex;
                        while ((index < rootCount) && IsWhiteSpaceOrPunctuation(rootText[index]))
                            index++;

                        if (index > rootIndex)
                            alternateTransliteration += rootText.Substring(rootIndex, index - rootIndex);
                    }
                }
            }

            while ((rootIndex < rootCount) && IsWhiteSpaceOrPunctuation(rootText[rootIndex]))
                rootIndex++;

            if (rootIndex < rootCount)
                return false;

            return returnValue;
        }

        protected bool FindRootRunFromRomanized(
            string romanizedWord,
            string rootText,
            ref int rootIndex,
            int rootCount,
            int maxRootLength,
            out string rootWord,
            out DictionaryEntry rootEntry,
            out int reading,
            out string alternateWord)
        {
            rootEntry = null;
            rootWord = null;
            reading = -1;
            alternateWord = null;

            while ((rootIndex < rootCount) && IsWhiteSpaceOrPunctuation(rootText[rootIndex]))
                rootIndex++;

            if (rootIndex >= rootCount)
                return false;

            if (IsRomanizedDigitString(romanizedWord))
            {
                int rootNextIndex = rootIndex;

                if (ParseNumberString(rootText, rootIndex, rootCount, out rootWord, ref rootNextIndex))
                {
                    alternateWord = rootWord;

                    if (!IsRomanizedDigitString(rootWord))
                    {
                        string romanizedNumber = TranslateNumber(RomanizedLanguageID, rootWord);

                        if (!String.IsNullOrEmpty(romanizedNumber))
                        {
                            if (MatchString(romanizedWord, romanizedNumber))
                            {
                                rootEntry = GetNumberDictionaryEntry(rootWord);
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                    if (!MatchString(romanizedWord, rootWord))
                        return false;

                    rootEntry = GetNumberDictionaryEntry(rootWord);
                    return true;
                }
            }

            int rootSubLength;
            List<DictionaryEntry> entries;

            for (rootSubLength = 1;
                ((rootIndex + rootSubLength) <= rootCount) && (rootSubLength <= maxRootLength);
                rootSubLength++)
            {
                rootWord = rootText.Substring(rootIndex, rootSubLength);
                entries = LookupDictionaryEntriesExact(rootWord, TargetLanguageIDs);

                if ((entries != null) && (entries.Count() != 0))
                {
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (MatchString(entry.KeyString, romanizedWord))
                        {
                            rootEntry = entry;
                            alternateWord = romanizedWord;
                            return true;
                        }
                        else if (entry.HasAlternates())
                        {
                            foreach (LanguageString alternate in entry.Alternates)
                            {
                                if (MatchString(alternate.Text, romanizedWord))
                                {
                                    rootEntry = entry;
                                    reading = alternate.KeyInt;
                                    LanguageID alternateID = NonRomanizedPhoneticLanguageID;
                                    if (alternateID != null)
                                    {
                                        if (alternateID == entry.LanguageID)
                                            alternateWord = entry.KeyString;
                                        else
                                        {
                                            LanguageString alternateAlternate = entry.GetAlternate(
                                                alternateID, alternate.KeyInt);
                                            if (alternateAlternate != null)
                                                alternateWord = alternateAlternate.Text;
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool FindAlternateRunFromRomanized(
            string romanizedWord,
            DictionaryEntry rootEntry,
            string alternateText,
            ref int alternateIndex,
            int alternateCount,
            int maxAlternateLength,
            out string alternateWord,
            out DictionaryEntry alternateEntry)
        {
            alternateWord = null;
            alternateEntry = null;

            while ((alternateIndex < alternateCount) && IsWhiteSpaceOrPunctuation(alternateText[alternateIndex]))
                alternateIndex++;

            if (alternateIndex >= alternateCount)
                return false;

            if (IsRomanizedDigitString(romanizedWord))
            {
                int alternateNextIndex = alternateIndex;

                if (ParseNumberString(alternateText, alternateIndex, alternateCount, out alternateWord, ref alternateNextIndex))
                {
                    if (!IsRomanizedDigitString(alternateWord))
                    {
                        string romanizedNumber = TranslateNumber(RomanizedLanguageID, alternateWord);

                        if (!String.IsNullOrEmpty(romanizedNumber))
                        {
                            if (MatchString(romanizedWord, romanizedNumber))
                            {
                                alternateEntry = GetNumberDictionaryEntry(alternateWord);
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                    if (!MatchString(romanizedWord, alternateWord))
                        return false;

                    alternateEntry = GetNumberDictionaryEntry(alternateWord);
                    return true;
                }
            }

            int alternateSubLength;

            for (alternateSubLength = 1;
                ((alternateIndex + alternateSubLength) < alternateCount)
                    && (alternateSubLength < maxAlternateLength);
                alternateSubLength++)
            {
                alternateWord = alternateText.Substring(alternateIndex, alternateSubLength);

                if (MatchString(rootEntry.KeyString, romanizedWord))
                {
                    alternateEntry = rootEntry;
                    return true;
                }
                else if (rootEntry.HasAlternates())
                {
                    foreach (LanguageString alternate in rootEntry.Alternates)
                    {
                        if (MatchString(alternate.Text, alternateWord))
                        {
                            alternateEntry = rootEntry;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected void AddOrUpdateRun(
            int runIndex,
            List<TextRun> wordRuns,
            string word,
            ref int textIndex)
        {
            if (String.IsNullOrEmpty(word))
                return;

            if (runIndex == wordRuns.Count())
            {
                TextRun wordRun = new TextRun(textIndex, word.Length, null);
                wordRuns.Add(wordRun);
            }
            else if (runIndex < wordRuns.Count())
            {
                TextRun wordRun = wordRuns[runIndex];
                wordRun.Start = textIndex;
                wordRun.Length = word.Length;
            }
            else
            {
                /*
                TextRun wordRun;
                while (wordRuns.Count() < runIndex)
                {
                    wordRun = new TextRun(textIndex, 0, null);
                    wordRuns.Add(wordRun);
                }
                wordRun = new TextRun(textIndex, word.Length, null);
                wordRuns.Add(wordRun);
                */
            }

            textIndex += word.Length;
        }

        protected virtual void HandleNotFoundRuns(
            ref int runIndex,
            ref int runCount,
            int runAheadIndex,
            int rootNotFoundIndex,
            int rootIndex,
            string rootText,
            List<TextRun> rootWordRuns,
            int alternateNotFoundIndex,
            ref int alternateIndex,
            string alternateText,
            List<TextRun> alternateWordRuns,
            string romanizedText,
            List<TextRun> romanizedWordRuns,
            ref string alternateTransliteration)
        {
            int startRunIndex = runIndex;
            bool haveAlternate =
                (!String.IsNullOrEmpty(alternateText)
                    && (alternateIndex != alternateNotFoundIndex)
                    && (alternateWordRuns != null));
            int segmentRunIndex;
            int rootSubIndex = rootNotFoundIndex;
            int alternateSubIndex = alternateNotFoundIndex;
            int fillInRunCount = runAheadIndex - runIndex;
            int index;
            LanguageID alternateID = NonRomanizedPhoneticLanguageID;
            int alternateCount = ((alternateText != null) ? alternateText.Length : 0);
            List<TextRun> newRootRuns = new List<TextRun>();
            List<TextRun> newAlternateRuns =
                (haveAlternate || ((alternateID != null) && (alternateCount == 0)) ? new List<TextRun>() : null);
            string alternateWord;

            for (segmentRunIndex = runIndex;
                (segmentRunIndex < runAheadIndex) && (rootSubIndex < rootIndex);
                segmentRunIndex++)
            {
                TextRun romanizedRun = romanizedWordRuns[segmentRunIndex];
                string romanizedWord = romanizedText.Substring(romanizedRun.Start, romanizedRun.Length);
                string rootWords = rootText.Substring(rootSubIndex, rootIndex - rootSubIndex);
                string matchedRootWord;
                string matchedAlternateWord;

                if (haveAlternate || (alternateID == null))
                {
                    if (MatchRomanizedString(romanizedWord, rootWords, out matchedRootWord))
                    {
                        if (haveAlternate && (alternateSubIndex < alternateIndex))
                        {
                            string alternateWords = alternateText.Substring(alternateSubIndex, alternateIndex - alternateSubIndex);

                            if (MatchRomanizedString(romanizedWord, alternateWords, out matchedAlternateWord))
                            {
                                newAlternateRuns.Add(new TextRun(alternateSubIndex, matchedAlternateWord.Length, null));
                                alternateSubIndex += matchedAlternateWord.Length;
                            }
                        }

                        newRootRuns.Add(new TextRun(rootSubIndex, matchedRootWord.Length, null));
                        rootSubIndex += matchedRootWord.Length;
                    }
                }
                else
                {
                    if (MatchRomanizedStringAndGetAlternate(romanizedWord, rootWords, out matchedRootWord, out matchedAlternateWord))
                    {
                        if (!String.IsNullOrEmpty(matchedAlternateWord))
                        {
                            newAlternateRuns.Add(new TextRun(alternateSubIndex, matchedAlternateWord.Length, null));
                            alternateSubIndex += matchedAlternateWord.Length;
                            alternateTransliteration += matchedAlternateWord;
                            alternateIndex += matchedAlternateWord.Length;
                        }

                        newRootRuns.Add(new TextRun(rootSubIndex, matchedRootWord.Length, null));
                        rootSubIndex += matchedRootWord.Length;
                    }
                }
            }

            if ((newRootRuns.Count() != 0) &&
                ((rootSubIndex == rootIndex) || (newRootRuns.Count() == fillInRunCount)))
            {
                for (int i = 0; (i < newRootRuns.Count()) && (runIndex < runAheadIndex); runIndex++, i++)
                {
                    TextRun rootRun = newRootRuns[i];

                    if (runIndex >= rootWordRuns.Count())
                        rootWordRuns.Add(rootRun);
                    else
                        rootWordRuns[runIndex] = rootRun;

                    if ((haveAlternate || ((alternateID != null) && (alternateCount == 0)))
                        && (i < newAlternateRuns.Count()))
                    {
                        TextRun alternateRun = newAlternateRuns[i];

                        if (runIndex >= alternateWordRuns.Count())
                            alternateWordRuns.Add(rootRun);
                        else
                            alternateWordRuns[runIndex] = rootRun;
                    }
                }

                return;
            }

            runIndex = startRunIndex;

            // We couldn't match the transliterations, so we combine the romanized runs if more than one.

            if (fillInRunCount > 1)
            {
                MergeRuns(
                    romanizedWordRuns,
                    runIndex,
                    runAheadIndex);
                fillInRunCount--;
                runCount -= fillInRunCount;
            }

            for (index = rootIndex - 1; index >= rootNotFoundIndex; index--)
            {
                if (IsWhiteSpaceOrPunctuation(rootText[index]))
                    rootIndex--;
                else
                    break;
            }

            string rootWord = rootText.Substring(
                rootNotFoundIndex,
                rootIndex - rootNotFoundIndex);

            AddOrUpdateRun(
                runIndex,
                rootWordRuns,
                rootWord,
                ref rootNotFoundIndex);

            if (haveAlternate)
            {
                alternateWord = alternateText.Substring(
                    alternateNotFoundIndex,
                    alternateIndex - alternateNotFoundIndex);

                AddOrUpdateRun(
                    runIndex,
                    alternateWordRuns,
                    alternateWord,
                    ref alternateNotFoundIndex);
            }
            else if ((alternateID != null) && (alternateWordRuns != null))
            {
                alternateTransliteration += rootWord;
                AddOrUpdateRun(
                    runIndex,
                    alternateWordRuns,
                    rootWord,
                    ref alternateNotFoundIndex);

                if (alternateCount == 0)
                {
                    index = rootIndex;
                    int count = rootText.Length;

                    while ((index < count) && IsWhiteSpaceOrPunctuation(rootText[index]))
                        index++;

                    if (index > rootIndex)
                        alternateTransliteration += rootText.Substring(rootIndex, index - rootIndex);

                    alternateIndex = alternateTransliteration.Length;
                }
            }

            runIndex++;
        }

        protected void MergeRuns(
            List<TextRun> runs,
            int runStartIndex,
            int runStopIndex)
        {
            int runIndex;
            TextRun startRun = null;

            if (runs == null)
                return;

            if (runStartIndex < runs.Count())
                startRun = runs[runStartIndex];
            else
                return;

            for (runIndex = runStartIndex + 1; runIndex < runStopIndex; runIndex++)
            {
                TextRun run = runs[runIndex];
                startRun.Merge(run);
            }

            int runCount = (runStopIndex - runStartIndex) - 1;

            if (runCount >= 1)
                runs.RemoveRange(runStartIndex + 1, runCount);
        }

        public virtual void SkipWhiteSpaceOrPunctuation(
            ref string text,
            ref int textIndex,
            int textStop)
        {
            while ((textIndex < textStop) && !IsWhiteSpaceOrPunctuation(text[textIndex]))
                textIndex++;
        }

        public virtual bool IsWhiteSpaceOrPunctuation(char chr)
        {
            if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(chr))
                return true;

            return char.IsWhiteSpace(chr) || char.IsPunctuation(chr);
        }

        public virtual bool IsWhiteSpace(char chr)
        {
            if (LanguageLookup.SpaceCharacters.Contains(chr))
                return true;

            return char.IsWhiteSpace(chr);
        }

        public static string NumberDictionarySourceName = "Number";

        private static int _NumberDictionarySourceID = 0;
        public static int NumberDictionarySourceID
        {
            get
            {
                if (_NumberDictionarySourceID == 0)
                    _NumberDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(NumberDictionarySourceName);
                return _NumberDictionarySourceID;
            }
        }

        public DictionaryEntry GetNumberDictionaryEntry(string numberString)
        {
            List<LanguageString> alternates = null;
            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();
            LanguageID alternateID = NonRomanizedPhoneticLanguageID;
            LanguageID romanizedID = RomanizedLanguageID;

            if (alternateID != null)
            {
                if (alternates == null)
                    alternates = new List<LanguageString>();

                LanguageString alternate = new LanguageString(0, alternateID, numberString);
                alternates.Add(alternate);
            }

            if (romanizedID != null)
            {
                if (alternates == null)
                    alternates = new List<LanguageString>();

                LanguageString alternate = new LanguageString(0, romanizedID, numberString);
                alternates.Add(alternate);
            }

            if ((HostLanguageIDs != null) && (HostLanguageIDs.Count() != 0))
            {
                foreach (LanguageID hostLanguageID in HostLanguageIDs)
                {
                    ProbableMeaning probableSynonym = new ProbableMeaning(
                        numberString,
                        LexicalCategory.Number,
                        null,
                        float.NaN,
                        0,
                        NumberDictionarySourceID);
                    List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                    LanguageSynonyms languageSynonyms = new LanguageSynonyms(hostLanguageID, probableSynonyms);
                    languageSynonymsList.Add(languageSynonyms);
                }
            }
            else
            {
                ProbableMeaning probableSynonym = new ProbableMeaning(
                    numberString,
                    LexicalCategory.Number,
                    null,
                    float.NaN,
                    0,
                    NumberDictionarySourceID);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(LanguageLookup.English, probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }

            Sense sense = new JTLanguageModelsPortable.Dictionary.Sense(
                0,
                LexicalCategory.Number,
                null,
                0,
                languageSynonymsList,
                null);
            List<Sense> senses = new List<Sense>(1) { sense };
            DictionaryEntry entry = new DictionaryEntry(
                numberString,
                RootLanguageID,
                alternates,
                0,
                NumberDictionarySourceID,
                senses,
                null);

            return entry;
        }

        public virtual void GetStudyItemLanguageItemSentenceAndWordRuns(MultiLanguageItem multiLanguageItem, LanguageID languageID)
        {
            if (multiLanguageItem == null)
                return;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem == null)
                return;

            if (languageID == LanguageID)
            {
                TextGraph textGraph = new TextGraph(null, languageID, languageItem.Text, this);

                if (!textGraph.ParseBest())
                {
                    languageItem.LoadWordRunsFromText(DictionaryDatabase);
                    languageItem.LoadSentenceRunsFromText();
                    return;
                }

                List<TextGraphNode> path = textGraph.Path;

                languageItem.WordRuns = new List<TextRun>();
                TextGraph.GetRunsFromPath(path, languageItem.WordRuns);
                languageItem.LoadSentenceRunsFromText();
            }
            else
            {
                LanguageItem baseLanguageItem = null;
                LanguageID baseLanguageID = null;

                foreach (LanguageID lid in TargetLanguageIDs)
                {
                    baseLanguageItem = multiLanguageItem.LanguageItem(lid);

                    if (baseLanguageItem != null)
                    {
                        baseLanguageID = lid;
                        break;
                    }
                }

                if (baseLanguageItem == null)
                    return;

                List<TextGraphNode> path = GetPathFromWordRuns(baseLanguageItem.Text, baseLanguageItem.WordRuns);

                if (GetLanguageItemWordRunsSynchronizedFromPath(languageItem, baseLanguageID, path))
                {
                    if (LanguageLookup.IsRomanized(languageID))
                        FixupRomanizedSpacesUsingWordRuns(languageItem);
                }
                else if (!LanguageLookup.GetLanguageDescription(languageID).CharacterBased)
                {
                    languageItem.WordRuns = new List<TextRun>();
                    GetSpacedWordRuns(languageItem.Text, languageItem.WordRuns);
                }

                languageItem.LoadSentenceRunsFromText();
            }
        }

        public virtual void GetLanguageItemSentenceAndWordRuns(LanguageItem languageItem)
        {
            if (languageItem == null)
                return;

            LanguageID baseLanguageID = languageItem.LanguageID;

            TextGraph textGraph = new TextGraph(null, baseLanguageID, languageItem.Text, this);

            if (!textGraph.ParseBest())
            {
                languageItem.LoadWordRunsFromText(DictionaryDatabase);
                languageItem.LoadSentenceRunsFromText();
                return;
            }

            List<TextGraphNode> path = textGraph.Path;

            languageItem.WordRuns = new List<TextRun>();
            TextGraph.GetRunsFromPath(path, languageItem.WordRuns);
            languageItem.LoadSentenceRunsFromText();
        }

        public virtual void GetWordRuns(
            string text,
            LanguageID languageID,
            List<TextRun> wordRuns)
        {
            TextGraph textGraph = new TextGraph(null, languageID, text, this);

            if (textGraph.ParseBest())
                textGraph.GetBestTextRuns(wordRuns);
        }

        public virtual bool GetWordRun(
            string text,
            int startIndex,
            int maxLength,
            LanguageID languageID,
            out string word,
            out int length,
            out DictionaryEntry dictionaryEntry)
        {
            DictionaryEntry rootDictionaryEntry = null;
            DictionaryEntry inflectionDictionaryEntry = null;
            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(languageID);
            int index;
            string ending;
            int dictionaryEntryMaxLength = languageDescription.LongestDictionaryEntryLength;
            int suffixMaxLength = languageDescription.LongestSuffixLength;
            int endingMaxLength;
            string bestWord = null;
            int bestLength = 0;
            string inflectionText = String.Empty;
            bool isInflection;
            bool returnValue = false;

            word = String.Empty;
            length = 0;
            dictionaryEntry = null;

            if (String.IsNullOrEmpty(text))
                return false;

            if (startIndex >= text.Length)
                return false;

            char firstChar = text[startIndex];

            if (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(firstChar) ||
                    char.IsPunctuation(firstChar))
                return false;

            if ((dictionaryEntryMaxLength != 0) && (dictionaryEntryMaxLength < maxLength))
                maxLength = dictionaryEntryMaxLength;

            for (index = startIndex + maxLength; index > startIndex; index--)
            {
                word = text.Substring(startIndex, index - startIndex);

                // If we are not the main language, do possible multi-transliteration lookup.
                if (LanguageLookup.IsMultiTransliterationLanguageID(languageID) &&
                        !LanguageLookup.IsAlternatePhonetic(languageID))
                    rootDictionaryEntry = GetDictionaryEntry(word);
                else
                    rootDictionaryEntry = GetDictionaryLanguageEntry(word, languageID);

                if (rootDictionaryEntry != null)
                {
                    if (rootDictionaryEntry.HasSenseWithStem())
                    {
                        endingMaxLength = maxLength - word.Length;

                        if (endingMaxLength != 0)
                        {
                            if ((suffixMaxLength != 0) && (endingMaxLength > suffixMaxLength))
                                endingMaxLength = suffixMaxLength;

                            ending = text.Substring(index, endingMaxLength);
                            inflectionDictionaryEntry = rootDictionaryEntry;

                            if (HandleStem(
                                    ref inflectionDictionaryEntry,
                                    ending,
                                    false,
                                    null,
                                    ref inflectionText,
                                    out isInflection) &&
                                isInflection)
                            {
                                if (word == inflectionText)
                                    continue;   // Don't use stem.
                                word = inflectionText;
                                length = word.Length;
                                dictionaryEntry = inflectionDictionaryEntry;
                                returnValue = true;
                                break;
                            }
                            else
                                continue;
                        }
                    }

                    length = word.Length;

                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestWord = word;
                    }
                }
            }

            if (bestLength > 0)
            {
                length = bestLength;
                word = bestWord;
                returnValue = true;
            }

            return returnValue;
        }

        public virtual void ResetLanguageItemWordRuns(LanguageItem languageItem)
        {
            if (languageItem == null)
                return;

            string text = languageItem.Text;
            bool isCharacterBased = LanguageLookup.IsCharacterBased(LanguageID);
            bool hasZeroWidthSpaces = false;
            List<TextRun> saveWordRuns = languageItem.WordRuns;

            if (text.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                hasZeroWidthSpaces = true;
            else if (text.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                hasZeroWidthSpaces = true;

            if (languageItem.WordRuns == null)
                languageItem.WordRuns = new List<TextRun>();

            if (!hasZeroWidthSpaces && isCharacterBased)
                GetWordRuns(text, LanguageID, languageItem.WordRuns);
            else
                SpacedLanguageItemWordRunCalculation(languageItem, hasZeroWidthSpaces);

            if ((saveWordRuns != null) && (languageItem.WordRuns != null))
            {
                int wordCount = saveWordRuns.Count();

                if (wordCount > languageItem.WordRuns.Count())
                    wordCount = languageItem.WordRuns.Count();

                for (int wordIndex = 0; wordIndex < wordCount; wordIndex++)
                {
                    TextRun oldRun = saveWordRuns[wordIndex];
                    TextRun newRun = languageItem.WordRuns[wordIndex];
                    newRun.MediaRuns = oldRun.CloneMediaRuns();
                }
            }

            languageItem.Modified = true;
        }

        protected void SpacedLanguageItemWordRunCalculation(
            LanguageItem languageItem,
            bool hasZeroWidthSpaces)
        {
            string text = languageItem.Text;
            string runText;
            TextRun textRun;
            int charIndex;
            int startIndex = 0;
            int runCount = languageItem.WordRuns.Count();
            int runIndex = 0;
            int runLength;
            int textLength = text.Length;
            char chr;

            for (charIndex = 0; charIndex <= textLength;)
            {
                if (charIndex < textLength)
                    chr = text[charIndex];
                else
                    chr = '\n';

                if ((charIndex == textLength) ||
                    LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                    char.IsPunctuation(chr))
                {
                    int skipOffset;

                    if (hasZeroWidthSpaces && (chr == ' '))
                    {
                        charIndex++;
                        continue;
                    }

                    if (chr == '.')
                    {
                        if (CheckForAbbreviation(text, charIndex, chr, out skipOffset))
                            charIndex += skipOffset;
                    }
                    else if ((chr != '\'') || (chr != '‘') || (chr != '’'))
                    {
                        if (CheckForApostrophe(text, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }
                    }
                    else if ((chr == '[') || (chr == ']'))
                    {
                        if (CheckForBracketedCorrection(text, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }
                    }

                    runLength = charIndex - startIndex;

                    if (runLength > 0)
                    {
                        runText = text.Substring(startIndex, runLength);

                        if (runIndex == languageItem.WordRuns.Count())
                        {
                            textRun = new TextRun(startIndex, runLength, null);
                            languageItem.WordRuns.Add(textRun);
                            runCount++;
                        }
                        else
                        {
                            textRun = languageItem.WordRuns[runIndex];
                            textRun.Start = startIndex;
                            textRun.Length = runLength;
                        }

                        runIndex++;
                    }

                    // Skip to next word.
                    while ((charIndex < textLength) &&
                            (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[charIndex]) ||
                                char.IsPunctuation(text[charIndex])))
                        charIndex++;

                    startIndex = charIndex;

                    if (charIndex == textLength)
                        break;
                }
                else
                    charIndex++;
            }

            if (runIndex < runCount)
            {
                while (runCount > runIndex)
                {
                    runCount--;
                    languageItem.WordRuns.RemoveAt(runCount);
                }
            }
        }

        public virtual void ResetMultiLanguageItemWordRuns(MultiLanguageItem multiLanguageItem)
        {
            if (multiLanguageItem == null)
                return;

            LanguageItem languageItem = null;

            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem != null)
                    break;
            }

            if (languageItem == null)
                return;

            LanguageID baseLanguageID = languageItem.LanguageID;

            TextGraph textGraph = new TextGraph(null, baseLanguageID, languageItem.Text, this);

            if (!textGraph.ParseBest())
            {
                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    languageItem = multiLanguageItem.LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    languageItem.AutoResetWordRuns(DictionaryDatabase);
                }

                return;
            }

            List<TextGraphNode> path = textGraph.Path;
            List<TextRun> saveWordRuns = languageItem.WordRuns;

            languageItem.WordRuns = new List<TextRun>();
            TextGraph.GetRunsFromPath(path, languageItem.WordRuns);

            RestoreWordRunMediaRuns(saveWordRuns, languageItem.WordRuns);

            int languageCount = TargetLanguageIDs.Count();
            int languageIndex;

            for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
            {
                LanguageID languageID = TargetLanguageIDs[languageIndex];
                LanguageItem subLanguageItem = multiLanguageItem.LanguageItem(languageID);

                if (subLanguageItem == null)
                    continue;

                if (GetLanguageItemWordRunsSynchronizedFromPath(subLanguageItem, baseLanguageID, path))
                {
                    if (LanguageLookup.IsRomanized(languageID))
                        FixupRomanizedSpacesUsingWordRuns(subLanguageItem);
                }
                else if (!LanguageLookup.GetLanguageDescription(languageID).CharacterBased)
                {
                    subLanguageItem.WordRuns = new List<TextRun>();
                    GetSpacedWordRuns(subLanguageItem.Text, subLanguageItem.WordRuns);
                }
            }
        }

        public bool GetLanguageItemWordRunsSynchronizedFromPath(
            LanguageItem languageItem,
            LanguageID rootLanguageID,
            List<TextGraphNode> rootPath)
        {
            bool gotIt = true;

            if (languageItem == null)
                return gotIt;

            string text = languageItem.Text;
            LanguageID languageID = languageItem.LanguageID;
            bool hasZeroWidthSpaces = false;
            List<TextRun> wordRuns = new List<TextRun>();
            List<TextRun> saveWordRuns = languageItem.WordRuns;

            if (text.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                hasZeroWidthSpaces = true;
            else if (text.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                hasZeroWidthSpaces = true;

            languageItem.WordRuns = wordRuns;

            if (!hasZeroWidthSpaces)
            {
                int charIndex;
                int runCount = languageItem.WordRuns.Count();
                int textLength = text.Length;
                char chr;
                TextGraphNode node;
                int pathIndex = 0;
                int pathCount = rootPath.Count();
                DictionaryEntry entry;

                for (charIndex = 0; (charIndex <= textLength) && (pathIndex < pathCount);)
                {
                    if (charIndex < textLength)
                        chr = text[charIndex];
                    else
                        chr = '\n';

                    if ((charIndex == textLength) ||
                        LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                        char.IsPunctuation(chr))
                    {
                        charIndex++;
                        continue;
                    }

                    node = rootPath[pathIndex];
                    entry = node.Entry;

                    if (entry == null)
                    {
                        retry1:
                        pathIndex++;

                        if (pathIndex == pathCount)
                            break;

                        node = rootPath[pathIndex];
                        entry = node.Entry;

                        if (entry != null)
                        {
                            int bestOffset = text.IndexOf(entry.KeyString, charIndex);

                            if (entry.HasAlternates())
                            {
                                foreach (LanguageString alternate in entry.Alternates)
                                {
                                    int offset = text.IndexOf(alternate.Text, charIndex, StringComparison.OrdinalIgnoreCase);

                                    if (offset != -1)
                                    {
                                        if (bestOffset != -1)
                                        {
                                            if (offset < bestOffset)
                                                bestOffset = offset;
                                        }
                                        else
                                            bestOffset = offset;
                                    }
                                }
                            }

                            if (bestOffset != -1)
                            {
                                TextRun wordRun = new TextRun(charIndex, bestOffset - charIndex, null);
                                wordRuns.Add(wordRun);
                                charIndex = bestOffset;
                            }
                        }
                        else
                            goto retry1;

                        continue;
                    }

                    gotIt = false;

                    if (entry.LanguageID == languageID)
                    {
                        if (gotIt = CheckWordRunMatch(entry.KeyString, text, ref charIndex, wordRuns))
                            pathIndex++;
                    }

                    if (!gotIt)
                    {
                        if (entry.HasAlternates())
                        {
                            foreach (LanguageString alternate in entry.Alternates)
                            {
                                if (alternate.LanguageID != languageID)
                                    continue;

                                if (gotIt = CheckWordRunMatch(alternate.Text, text, ref charIndex, wordRuns))
                                {
                                    pathIndex++;
                                    break;
                                }
                                else if (LanguageLookup.IsAlternatePhonetic(languageID))
                                {
                                    int altLength = alternate.Text.Length;
                                    if (textLength - charIndex < altLength)
                                        altLength = textLength - charIndex;
                                    string alternateText = FixupTransliteration(
                                        alternate.Text,
                                        languageID,
                                        text.Substring(charIndex, altLength),
                                        rootLanguageID,
                                        true);

                                    if (gotIt = CheckWordRunMatch(alternateText, text, ref charIndex, wordRuns))
                                    {
                                        pathIndex++;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!gotIt)
                        {
                            string key = entry.KeyString;
                            int bestOffset = text.IndexOf(key, charIndex);
                            int bestLength = 0;

                            if (bestOffset != -1)
                                bestLength = key.Length;

                            if (entry.HasAlternates())
                            {
                                foreach (LanguageString alternate in entry.Alternates)
                                {
                                    int offset = text.IndexOf(alternate.Text, charIndex, StringComparison.OrdinalIgnoreCase);

                                    if (offset != -1)
                                    {
                                        if (bestOffset != -1)
                                        {
                                            if (offset < bestOffset)
                                            {
                                                bestOffset = offset;
                                                bestLength = alternate.TextLength;
                                            }
                                            else if ((offset == bestOffset) && (alternate.TextLength > bestLength))
                                                bestLength = alternate.TextLength;
                                        }
                                        else
                                        {
                                            bestOffset = offset;
                                            bestLength = alternate.TextLength;
                                        }
                                    }
                                }
                            }

                            if (bestOffset != -1)
                            {
                                TextRun wordRun = new TextRun(bestOffset, bestLength, null);
                                wordRuns.Add(wordRun);
                                gotIt = true;
                                pathIndex++;
                                charIndex = bestOffset + bestLength;
                                continue;
                            }
                        }

                        if (!gotIt)
                        {
                            if (entry.HasAlternates())
                            {
                                foreach (LanguageString alternate in entry.Alternates)
                                {
                                    if (gotIt = CheckWordRunMatch(alternate.Text, text, ref charIndex, wordRuns))
                                    {
                                        pathIndex++;
                                        break;
                                    }
                                }
                            }

                            if (!gotIt)
                            {
                                retry2:
                                pathIndex++;

                                if (pathIndex == pathCount)
                                    break;

                                node = rootPath[pathIndex];
                                entry = node.Entry;

                                if (entry != null)
                                {
                                    int bestOffset = text.IndexOf(entry.KeyString, charIndex);

                                    if (entry.HasAlternates())
                                    {
                                        foreach (LanguageString alternate in entry.Alternates)
                                        {
                                            int offset = text.IndexOf(alternate.Text, charIndex, StringComparison.OrdinalIgnoreCase);

                                            if (offset != -1)
                                            {
                                                if (bestOffset != -1)
                                                {
                                                    if (offset < bestOffset)
                                                        bestOffset = offset;
                                                }
                                                else
                                                    bestOffset = offset;
                                            }
                                        }
                                    }

                                    if (bestOffset != -1)
                                    {
                                        TextRun wordRun = new TextRun(charIndex, bestOffset - charIndex, null);
                                        wordRuns.Add(wordRun);
                                        charIndex = bestOffset;
                                    }
                                }
                                else
                                    goto retry2;

                                continue;
                            }
                        }
                    }

                    int skipOffset;

                    if (chr == '.')
                    {
                        if (CheckForAbbreviation(text, charIndex, chr, out skipOffset))
                            charIndex += skipOffset;
                    }
                    else if ((chr == '\'') || (chr == '‘') || (chr == '’'))
                    {
                        if (CheckForApostrophe(text, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }
                    }
                    else if ((chr == '[') || (chr == ']'))
                    {
                        if (CheckForBracketedCorrection(text, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }
                    }
                }
            }
            else
                SpacedLanguageItemWordRunCalculation(languageItem, hasZeroWidthSpaces);

            RestoreWordRunMediaRuns(saveWordRuns, wordRuns);

            return gotIt;
        }

        public List<TextGraphNode> GetPathFromWordRuns(string text, List<TextRun> wordRuns)
        {
            List<TextGraphNode> nodes = new List<TextGraphNode>();

            if (wordRuns == null)
                return nodes;

            foreach (TextRun wordRun in wordRuns)
            {
                string word = text.Substring(wordRun.Start, wordRun.Length);
                bool isInflection;
                DictionaryEntry entry = LookupDictionaryEntry(
                    word,
                    MatchCode.Exact,
                    TargetLanguageIDs,
                    null,
                    out isInflection);
                TextGraphNode node = new TextGraphNode(wordRun.Start, wordRun.Length, word, entry, -1);
                nodes.Add(node);
            }

            return nodes;
        }

        protected void RestoreWordRunMediaRuns(List<TextRun> originalRuns, List<TextRun> newRuns)
        {
            if ((originalRuns != null) && (newRuns != null))
            {
                int wordCount = originalRuns.Count();

                if (wordCount > newRuns.Count())
                    wordCount = newRuns.Count();

                for (int wordIndex = 0; wordIndex < wordCount; wordIndex++)
                {
                    TextRun oldRun = originalRuns[wordIndex];
                    TextRun newRun = newRuns[wordIndex];
                    newRun.MediaRuns = oldRun.CloneMediaRuns();
                }
            }
        }

        protected bool CheckWordRunMatch(
            string pattern,
            string text,
            ref int textIndex,
            List<TextRun> wordRuns)
        {
            int patternLength = pattern.Length;
            int patternIndex;
            int textLength = text.Length;
            int textOffset = textIndex;
            char textChar;
            char patternChar;
            bool notDone = true;
            bool gotIt = false;

            for (patternIndex = 0, textOffset = textIndex; (patternIndex < patternLength) && (textOffset < textLength) && notDone;)
            {
                if (char.IsWhiteSpace(patternChar = char.ToLower(pattern[patternIndex])))
                    patternIndex++;
                else if (char.IsWhiteSpace(textChar = char.ToLower(text[textOffset])))
                    textOffset++;
                else if (textChar != patternChar)
                {
                    notDone = false;
                    gotIt = false;
                }
                else
                {
                    patternIndex++;
                    textOffset++;
                    gotIt = true;
                }
            }

            if (gotIt)
            {
                int runLength = textOffset - textIndex;
                string runText = text.Substring(textIndex, runLength);
                TextRun wordRun = new TextRun(textIndex, runLength, null);
                wordRuns.Add(wordRun);
                textIndex = textOffset;
                return true;
            }

            return false;
        }

        // Returns true if single quote character is an apostrophe.
        public virtual bool CheckForApostrophe(string text, int apostropheIndex, char apostrophe, out int skipOffset)
        {
            return CheckForApostropheCommon(
                text,
                TargetLanguageIDs[0],
                apostropheIndex,
                apostrophe,
                out skipOffset);
        }

        // Returns true if single quote character is an apostrophe.
        public static bool CheckForApostropheCommon(
            string text,
            LanguageID languageID,
            int apostropheIndex,
            char apostrophe,
            out int skipOffset)
        {
            int length = text.Length;

            skipOffset = 0;

            if ((apostrophe != '\'') && (apostrophe != '‘') && (apostrophe != '’'))
                return false;

            int firstIndex = apostropheIndex;

            for (; (firstIndex > 0) && !Char.IsWhiteSpace(text[firstIndex - 1]); firstIndex--)
                ;

            int lastIndex = apostropheIndex + 1;

            for (; (lastIndex < length) && !Char.IsWhiteSpace(text[lastIndex]); lastIndex++)
                ;

            lastIndex--;

            if (((text[firstIndex] == '\'') || (text[firstIndex] == '‘')) &&
                    ((text[lastIndex] == '\'') || (text[lastIndex] == '’')))
                return false;

            int index = apostropheIndex + 1;

            if ((index >= length) || Char.IsWhiteSpace(text[index]))
            {
                skipOffset = 1;
                return true;
            }

            char nextChr = text[index++];

            if (nextChr == '\\')
                return false;

            if (index >= length)
            {
                skipOffset = 1;
                return true;
            }

#if false
            nextChr = text[index];

            if (nextChr == '\'')
                return false;
            else if ((nextChr == '’') && (apostrophe == '‘'))
                return false;
#endif

            index = apostropheIndex - 2;

            if (index >= 0)
            {
                nextChr = text[index];

                if (nextChr == '\\')
                {
                    index--;

                    if (index >= 0)
                        nextChr = text[index];
                }

                if ((nextChr == '\'') || (nextChr == '‘'))
                    return false;
            }

            skipOffset = 1;

            return true;
        }

        // Returns true if bracketed correction.
        public virtual bool CheckForBracketedCorrection(string text, int apostropheIndex, char apostrophe, out int skipOffset)
        {
            return CheckForBracketedCorrectionCommon(
                text,
                TargetLanguageIDs[0],
                apostropheIndex,
                apostrophe,
                out skipOffset);
        }

        // Returns true if bracketed correction.
        public static bool CheckForBracketedCorrectionCommon(
            string text,
            LanguageID languageID,
            int bracketIndex,
            char bracket,
            out int skipOffset)
        {
            int length = text.Length;

            skipOffset = 0;

            if ((bracket != '[') && (bracket != ']'))
                return false;

            char chr = text[bracketIndex];
            bool hasLeadingBracket = (chr == '[');
            bool hasTrailingBracket = (chr == ']');
            bool hasOpenBracket = (chr == '[');
            bool hasCloseBracket = (chr == ']');
            int firstIndex = bracketIndex;

            for (; firstIndex >= 0; firstIndex--)
            {
                chr = text[firstIndex];

                if (chr == '[')
                {
                    hasLeadingBracket = true;
                    hasOpenBracket = true;
                }
                else if (!char.IsPunctuation(chr))
                    hasLeadingBracket = false;

                if ((firstIndex == 0) || Char.IsWhiteSpace(text[firstIndex - 1]))
                    break;
            }

            int lastIndex = bracketIndex;

            for (; (lastIndex < length) && !Char.IsWhiteSpace(text[lastIndex]); lastIndex++)
            {
                chr = text[lastIndex];

                if (chr == ']')
                {
                    hasTrailingBracket = true;
                    hasCloseBracket = true;
                }
                else if (!char.IsPunctuation(chr))
                    hasTrailingBracket = false;
            }

            lastIndex--;

            if (!hasOpenBracket || !hasCloseBracket)
                return false;

            if (hasLeadingBracket && hasTrailingBracket)
                return false;

            skipOffset = 1;

            return true;
        }

        public static bool IsUseProgrammaticAbbreviationCheck = false;

        // Returns true if period is at an abbreviation.
        public virtual bool CheckForAbbreviation(string text, int periodIndex, char period, out int skipOffset)
        {
            if (IsUseProgrammaticAbbreviationCheck)
                return CheckForAbbreviationProgrammatic(text, periodIndex, period, out skipOffset);
            else
                return CheckForAbbreviationCommon(text, LanguageID, periodIndex, period, out skipOffset);
        }

        // Returns true if period is at an abbreviation.
        public static bool CheckForAbbreviationLanguage(string text, LanguageID languageID, int periodIndex, char period, out int skipOffset)
        {
            if (IsUseProgrammaticAbbreviationCheck)
                return CheckForAbbreviationProgrammatic(text, periodIndex, period, out skipOffset);
            else
                return CheckForAbbreviationCommon(text, languageID, periodIndex, period, out skipOffset);
        }

        // Returns true if period is at an abbreviation.
        public static bool CheckForAbbreviationProgrammatic(string text, int periodIndex, char period, out int skipOffset)
        {
            int length = text.Length;
            int minLength = text.Length - periodIndex;

            skipOffset = 1;

            if (minLength < 2)
                return false;

            // Check for ordinal ("1." or "A.").
            if (periodIndex >= 1)
            {
                if (char.IsDigit(text[periodIndex - 1]))
                {
                    if (periodIndex == 1)
                        return true;

                    int i = periodIndex - 2;

                    for (; i >= 0; i--)
                    {
                        if (char.IsWhiteSpace(text[i]) || !char.IsDigit(text[i]))
                            break;
                    }

                    if ((i == 0) && char.IsDigit(text[i]))
                        return true;
                }
                else if (char.IsLetter(text[periodIndex - 1]))
                {
                    int i = periodIndex - 2;

                    for (; i >= 0; i--)
                    {
                        if (char.IsWhiteSpace(text[i]) || !char.IsLetter(text[i]))
                            break;
                    }

                    if ((i == 0) && char.IsLetter(text[i]))
                        return true;
                }
            }

            // Check for abbreviation.
            // If no space after period...
            if (((periodIndex + 1) < length) && !char.IsWhiteSpace(text[periodIndex + 1]))
            {
                if (periodIndex > 0)
                {
                    if (char.IsLetterOrDigit(text[periodIndex - 1]))
                    {
                        skipOffset++;

                        int i;

                        for (i = periodIndex + 2; i < length; i++)
                        {
                            if (char.IsWhiteSpace(text[i]))
                                return true;

                            skipOffset++;
                        }

                        if (i == length)
                            return true;
                    }
                }
            }

            return false;
        }

#if false
        public static Dictionary<string, string[]> Abbrevs = new Dictionary<string, string[]>()
        {
            {"en", new string[]
                {
                    "B.C.",
                    "A.D.",
                    "C.E.",
                    "Mr.",
                    "Mrs.",
                    "Ms.",
                    "Sr.",
                    "Jr.",
                    "Jun.",
                    "etc.",
                    "a.m.",
                    "p.m.",
                    "No.",
                    "Fig."
                }
            },
            {"es", new string[]
                {
                    "a.C.",
                    "a. de C.",
                    "a.J.C.",
                    "a. de J.C.",
                    "a. m.",
                    "apdo.",
                    "aprox.",
                    "Av.",
                    "Avda.",
                    "Bs. As.",
                    "cap.o",
                    "c.c.",
                    "Da.",
                    "d.C.",
                    "d. de C.",
                    "d.J.C.",
                    "d. de J.C.",
                    "dna.",
                    "Dr.",
                    "Dra.",
                    "EE. UU.",
                    "esq.",
                    "etc.",
                    "f.c.",
                    "F.C.",
                    "FF. AA.",
                    "Gob.",
                    "Gral.",
                    "h.",
                    "Ing.",
                    "Lic.",
                    "m.n.",
                    "ms.",
                    // "no.",  (too easily mistaken for a negative.
                    "núm.",
                    "OTAN",
                    "pág.",
                    "P.D.",
                    "Pdte.",
                    "Pdta.",
                    "p.ej.",
                    "p. m.",
                    "Prof.",
                    "Profa.",
                    "q.e.p.d.",
                    "S.A.",
                    "S.L.",
                    "Sr.",
                    "Sra.",
                    "Srta.",
                    "s.s.s.",
                    "tel.",
                    "Ud.",
                    "Vd.",
                    "Uds.",
                    "Vds.",
                    "v.",
                    "vol.",
                    "W.C."
                }
            },
            {"fr", new string[]
                {
                    "av. J.-C."
                }
            },
            {"de", new string[]
                {
                    "v. Chr."
                }
            },
            {"pt", new string[]
                {
                    "a.C.",
                    "a. de C.",
                    "a.J.C.",
                    "a. de J.C.",
                    "a. m.",
                    "apdo.",
                    "aprox.",
                    "Av.",
                    "Avda.",
                    "Bs. As.",
                    "cap.o",
                    "c.c.",
                    "Da.",
                    "d.C.",
                    "d. de C.",
                    "d.J.C.",
                    "d. de J.C.",
                    "dna.",
                    "Dr.",
                    "Dra.",
                    "EE. UU.",
                    "esq.",
                    "etc.",
                    "f.c.",
                    "F.C.",
                    "FF. AA.",
                    "Gob.",
                    "Gral.",
                    "h.",
                    "Ing.",
                    "Lic.",
                    "m.n.",
                    "ms.",
                    // "no.",
                    "núm.",
                    "OTAN",
                    "pág.",
                    "P.D.",
                    "Pdte.",
                    "Pdta.",
                    "p.ej.",
                    "p. m.",
                    "Prof.",
                    "Profa.",
                    "q.e.p.d.",
                    "S.A.",
                    "S.L.",
                    "Sr.",
                    "Sra.",
                    "Srta.",
                    "s.s.s.",
                    "tel.",
                    "Ud.",
                    "Vd.",
                    "Uds.",
                    "Vds.",
                    "v.",
                    "vol.",
                    "W.C."
                }
            },
            {"mh", new string[]
                {
                    "M.K."
                }
            }
        };
#endif

        public static string[] NoOrdinalAbbreviationLanguages = new string[]
        {
            "mh"
        };

        public virtual Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return null;
            }
        }

        public static Dictionary<string, string> GetAbbreviationDictionary(LanguageID languageID)
        {
            LanguageTool languageTool = LanguageToolFactory.Factory.GetCached(languageID);

            if (languageTool != null)
                return languageTool.AbbreviationDictionary;

            return null;
        }

        // Returns true if period is at an abbreviation.
        public static bool CheckForAbbreviationCommon(string text, LanguageID languageID, int periodIndex, char period, out int skipOffset)
        {
            string languageCode = languageID.LanguageCode;
            //string[] abbrevs = null;
            Dictionary<string, string> abbrevDict = GetAbbreviationDictionary(languageID);

            if (abbrevDict == null)
                return CheckForAbbreviationProgrammatic(text, periodIndex, period, out skipOffset);

            //if (!Abbrevs.TryGetValue(languageCode, out abbrevs) || (abbrevs == null))
            //    return CheckForAbbreviationProgrammatic(text, periodIndex, period, out skipOffset);

            int length = text.Length;
            int minLength = text.Length - periodIndex;
            bool returnValue = false;

            skipOffset = 1;

            if ((periodIndex > 0) && (minLength >= 2) && !NoOrdinalAbbreviationLanguages.Contains(languageCode))
            {
                int index = periodIndex + 1;

                if (char.IsLetterOrDigit(text[index]))
                {
                    index++;

                    while (index < length)
                    {
                        char c = text[index];

                        if (!char.IsLetterOrDigit(c) && (c != '.'))
                            break;

                        index++;
                    }

                    skipOffset = index - periodIndex;
                    return true;
                }

                if (periodIndex >= 2)
                {
                    char c1 = text[periodIndex - 1];
                    char c2 = text[periodIndex - 2];

                    if (char.IsWhiteSpace(c2) && char.IsUpper(c1))
                        return true;
                }
            }

            //foreach (string abbrev in abbrevs)
            foreach (KeyValuePair<string, string> kvp in abbrevDict)
            {
                string abbrev = kvp.Key;

                int len = abbrev.Length;

                if (periodIndex < len - 1)
                    continue;

                int subLength = abbrev.IndexOf(period) + 1;

                if (subLength == 0)
                    subLength = len;

                if ((len - subLength) > minLength)
                    continue;

                if (periodIndex - (subLength - 1) + len > text.Length)
                    continue;

                int offset = periodIndex - (subLength - 1);

                if ((offset != 0) && !char.IsWhiteSpace(text[offset - 1]))
                    continue;

                if (text.Substring(offset, len) == abbrev)
                {
                    skipOffset = (len - subLength) + 1;
                    return true;
                }
            }

            return returnValue;
        }

        protected void GetFlatSentenceRuns(
            List<MultiLanguageItem> studyItems,
            int studyItemStartIndex,
            int sentenceStartIndex,
            LanguageID languageID,
            List<TextRun> sentenceRuns,
            List<int> studyItemIndexes)
        {
            int studyItemIndex;
            int studyItemCount = studyItems.Count();
            int sentenceIndex = sentenceStartIndex;
            int sentenceCount;

            for (studyItemIndex = studyItemStartIndex; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                MultiLanguageItem studyItem = studyItems[studyItemIndex];
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.SentenceRunCheck();
                sentenceCount = languageItem.SentenceRunCount();

                for (; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    sentenceRuns.Add(languageItem.GetSentenceRun(sentenceIndex));
                    studyItemIndexes.Add(studyItemIndex);
                }

                sentenceIndex = 0;
            }
        }

        public virtual void InitializeWordFixes(string wordFixesKey)
        {
        }

        protected virtual void InitializeDefaultWordFixes()
        {
        }

        public virtual void InitializeWordFixesCheck(string wordFixesKey)
        {
            if (WordFixes == null)
                InitializeWordFixes(wordFixesKey);
        }

        public virtual void InitializeSentenceFixes(string sentenceFixesKey)
        {
            if (!String.IsNullOrEmpty(sentenceFixesKey))
            {
                string sentenceFixesFilePath = SentenceFixes.GetFilePath(sentenceFixesKey, null);
                SentenceFixes.CreateAndLoad(sentenceFixesFilePath, out _SentenceFixes);
            }
            else
                SentenceFixes = new SentenceFixes();

            InitializeDefaultSentenceFixes();
        }

        protected virtual void InitializeDefaultSentenceFixes()
        {
        }

        public virtual void InitializeSentenceFixesCheck(string sentenceFixesKey)
        {
            if (SentenceFixes == null)
                InitializeSentenceFixes(sentenceFixesKey);
        }

        public SentenceFixes SentenceFixes
        {
            get
            {
                return _SentenceFixes;
            }
            set
            {
                _SentenceFixes = value;
            }
        }

        public WordFixes WordFixes
        {
            get
            {
                return _WordFixes;
            }
            set
            {
                _WordFixes = value;
            }
        }
    }
}

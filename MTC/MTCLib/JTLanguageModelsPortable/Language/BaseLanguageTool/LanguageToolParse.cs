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
        public virtual bool Parse(string text, LanguageID languageID, out List<TextGraphNode> nodes)
        {
            TextGraph textGraph = new TextGraph(null, languageID, text, this);
            bool returnValue = textGraph.ParseBest();

            if (returnValue)
                nodes = textGraph.Path;
            else
                nodes = null;

            return returnValue;
        }

        public virtual bool ParseAll(string text, LanguageID languageID, out List<TextGraphNode> nodes)
        {
            TextGraph textGraph = new TextGraph(null, languageID, text, this);
            bool returnValue = textGraph.ParseAll();

            if (returnValue)
                nodes = textGraph.Path;
            else
                nodes = null;

            return returnValue;
        }

        public virtual bool ParseMatchingWordRuns(
            string text,
            LanguageID languageID,
            List<TextRun> wordRuns,
            out List<TextGraphNode> nodes)
        {
            TextGraph textGraph = new TextGraph(null, languageID, text, this);
            bool returnValue = textGraph.ParseMatchingWordRuns(wordRuns);

            if (returnValue)
                nodes = textGraph.Path;
            else
                nodes = null;

            return returnValue;
        }

        public virtual bool ParseAndGetWordRuns(
            string text,
            LanguageID languageID,
            out List<TextGraphNode> nodes,
            out List<TextRun> wordRuns)
        {
            TextGraph textGraph = new TextGraph(null, languageID, text, this);
            bool returnValue = textGraph.ParseBest();

            if (returnValue)
            {
                nodes = textGraph.Path;
                wordRuns = new List<TextRun>();
                TextGraph.GetRunsFromPath(nodes, wordRuns);
            }
            else
            {
                nodes = null;
                wordRuns = null;
            }

            return returnValue;
        }

        public virtual bool ParseForeignWord(string text, int textIndex, int maxLength, out string word)
        {
            return ParseWordUntilSpaceOrPunctuation(text, textIndex, maxLength, out word);
        }

        public bool ParseWordUntilSpaceOrPunctuation(string text, int textIndex, int maxLength, out string word)
        {
            word = null;
            if (String.IsNullOrEmpty(text))
                return false;
            int textLength = text.Length;
            int maxIndex = textIndex + maxLength;
            int index = textIndex;
            if (maxIndex > textLength)
                maxIndex = textLength;
            for (; index < maxIndex; index++)
            {
                char chr = text[index];
                if (char.IsWhiteSpace(chr) || char.IsPunctuation(chr))
                {
                    word = text.Substring(textIndex, index - textIndex);
                    return true;
                }
            }
            word = text.Substring(textIndex, index - textIndex);
            return true;
        }
        public virtual int CompareTextGraphNodeForWeight(
            TextGraphNode x,
            TextGraphNode y,
            TextGraph graph,
            LanguageID targetLanguageID)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            DictionaryEntry xe = x.Entry;
            DictionaryEntry ye = y.Entry;

            if ((xe == null) || (ye == null))
            {
                if (xe == ye)
                    return 0;
                else if (xe == null)
                    return -1;
                else
                    return 1;
            }

            string xt = xe.KeyString;
            string yt = ye.KeyString;

            bool xIsNumber = ObjectUtilities.IsNumberString(xt);
            bool yIsNumber = ObjectUtilities.IsNumberString(yt);

            if (xIsNumber != yIsNumber)
            {
                if (yIsNumber)
                    return -1;
                return 1;
            }

            bool xSep = false;
            bool ySep = false;
            bool xNotSep = false;
            bool yNotSep = false;

            if (_WordFixes != null)
            {
                xSep = _WordFixes.IsCompoundToSeparate(xt);
                ySep = _WordFixes.IsCompoundToSeparate(yt);
                xNotSep = _WordFixes.IsCompoundToNotSeparate(xt);
                yNotSep = _WordFixes.IsCompoundToNotSeparate(yt);
            }

            if (xSep != ySep)
            {
                if (ySep)
                    return 1;
                return -1;
            }
            else if (xNotSep != yNotSep)
            {
                if (xNotSep)
                    return 1;
                return -1;
            }

            bool xIsStem = x.Entry.HasSenseWithStemOnly();
            bool yIsStem = y.Entry.HasSenseWithStemOnly();

            if (xIsStem != yIsStem)
            {
                if (yIsStem)
                    return 1;
                return -1;
            }

            int xNextSlotBestLength = GetNextSlotBestLength(x, graph);
            int yNextSlotBestLength = GetNextSlotBestLength(y, graph);

            int xCombinedLength = (xNextSlotBestLength >= 0 ? x.Length + xNextSlotBestLength : -1);
            int yCombinedLength = (yNextSlotBestLength >= 0 ? y.Length + yNextSlotBestLength : -1);

            if (xCombinedLength > yCombinedLength)
                return 1;

            if (xCombinedLength < yCombinedLength)
                return -1;

            if (x.Length > y.Length)
                return 1;

            if (x.Length < y.Length)
                return -1;

            return 0;
        }

        public virtual int GetNextSlotBestLength(TextGraphNode node, TextGraph graph)
        {
            List<TextGraphNode> nextSlot = graph.GetSlot(node.Stop);

            if (nextSlot == null)
                return 0;

            int bestLength = -1;

            foreach (TextGraphNode nextNode in nextSlot)
            {
                if (nextNode.Length > bestLength)
                {
                    if (nextNode.Entry == null)
                        continue;

                    if (nextNode.Entry.HasSenseWithStemOnly())
                        continue;

                    bestLength = nextNode.Length;
                }
            }

            return bestLength;
        }

        public virtual bool HasTextGraphNodeProblem(TextGraphNode node)
        {
            if (node == null)
                return false;

            if ((node.Entry == null) || (node.Entry.LanguageID != LanguageID))
                return true;

            return false;
        }

        public virtual bool MarkPhrasesLanguageItem(
            MultiLanguageItem studyItem,
            LanguageItem languageItem,
            List<string> explicitPhrases,
            bool isDoDictionaryPhrases,
            bool isDoInflectionPhrases,
            out string errorMessage)
        {
            errorMessage = null;

            // Validate arguments.

            if ((studyItem == null) || (languageItem == null))
            {
                errorMessage = "MarkPhrasesLanguageItem: Null study or language item: ";
                return false;
            }

            if (!languageItem.HasWordRuns())
            {
                if (languageItem.HasText())
                {
                    errorMessage = "MarkPhrasesLanguageItem: Language item has no word runs: " + studyItem.GetNamePathStringInclusive(LanguageLookup.English);
                    return false;
                }
            }

            // Clear prior phrases.
            languageItem.PhraseRuns = null;

            string text = languageItem.Text;
            string textLower = languageItem.TextLower;
            int textLength = text.Length;
            int phraseStartIndex;
            int phraseEndIndex;
            int phraseLength;
            TextRun phraseRun;

            if ((explicitPhrases != null) && (explicitPhrases.Count() != 0))
            {
                foreach (string phrase in explicitPhrases)
                {
                    int startIndex;

                    phraseLength = phrase.Length;

                    for (startIndex = 0; startIndex < textLength;)
                    {
                        phraseStartIndex = TextUtilities.IndexOfWholeWord(textLower, phrase, startIndex);

                        if (phraseStartIndex == -1)
                            break;

                        phraseEndIndex = phraseStartIndex + phraseLength;

                        if (languageItem.HasOverlappingPhraseRun(phraseStartIndex, phraseEndIndex))
                        {
                            startIndex = phraseEndIndex;
                            continue;
                        }

                        phraseRun = new TextRun(phraseStartIndex, phraseLength, null);

                        languageItem.AddPhraseRun(phraseRun);

                        startIndex = phraseEndIndex;
                    }
                }
            }

            if (isDoDictionaryPhrases || isDoInflectionPhrases)
            {
                LanguageID languageID = languageItem.LanguageID;
                List<TextRun> wordRuns = languageItem.WordRuns;

                if (wordRuns != null)
                {
                    int wordCount = wordRuns.Count();
                    int wordIndex;

                    for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                    {
                        TextRun wordRun = wordRuns[wordIndex];
                        string word = languageItem.GetRunText(wordRun).ToLower();

                        string[] phraseCandidates = GetContainingPhrasesCached(
                            word,
                            languageID,
                            isDoDictionaryPhrases,
                            isDoInflectionPhrases);

                        if ((phraseCandidates == null) || (phraseCandidates.Length == 0))
                            continue;

                        foreach (string phrase in phraseCandidates)
                        {
                            int startIndex;

                            phraseLength = phrase.Length;

                            for (startIndex = 0; startIndex < textLength;)
                            {
                                phraseStartIndex = TextUtilities.IndexOfWholeWord(text, phrase, startIndex);

                                if (phraseStartIndex == -1)
                                    break;

                                phraseEndIndex = phraseStartIndex + phraseLength;

                                startIndex = phraseEndIndex;

                                if (languageItem.HasOverlappingPhraseRun(phraseStartIndex, phraseEndIndex))
                                    continue;

                                phraseRun = new TextRun(phraseStartIndex, phraseLength, null);

                                languageItem.AddPhraseRun(phraseRun);
                            }
                        }
                    }
                }
            }

            if (languageItem.PhraseRunCount() > 1)
                languageItem.PhraseRuns.Sort((x, y) => x.Start.CompareTo(y.Start));

            if (!languageItem.ValidatePhraseRuns(out errorMessage))
                return false;

            return true;
        }

#if true
        protected List<string> _DictionaryPhraseCacheRaw;
        protected List<string> DictionaryPhraseCacheRaw
        {
            get
            {
                if (_DictionaryPhraseCacheRaw == null)
                {
                    _DictionaryPhraseCacheRaw = new List<string>();

                    foreach (LanguageID targetLanguageID in TargetLanguageIDs)
                    {
                        List<DictionaryEntry> dictionaryEntries = DictionaryDatabase.GetAll(targetLanguageID);

                        foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                        {
                            string phraseCandidate = dictionaryEntry.KeyString;

                            if (!FilterPhraseCandidate(ref phraseCandidate))
                                continue;

                            _DictionaryPhraseCacheRaw.Add(phraseCandidate);
                        }
                    }
                }

                return _DictionaryPhraseCacheRaw;
            }
        }

        // Returns false if the phrase should not be used.
        protected bool FilterPhraseCandidate(ref string phraseCandidate)
        {
            if (!phraseCandidate.Contains(" "))
                return false;

            int startIndex, endIndex;
            int length = phraseCandidate.Length;

            for (startIndex = 0; startIndex < length; startIndex++)
            {
                char chr = phraseCandidate[startIndex];

                if (!char.IsWhiteSpace(chr) && !LanguageLookup.PunctuationCharacters.Contains(chr))
                    break;
            }

            for (endIndex = length - 1; endIndex > startIndex; endIndex--)
            {
                char chr = phraseCandidate[endIndex];

                if (!char.IsWhiteSpace(chr) && !LanguageLookup.PunctuationCharacters.Contains(chr))
                    break;
            }

            if (startIndex == endIndex)
                return false;

            if ((startIndex != 0) || (endIndex != length))
                phraseCandidate = phraseCandidate.Substring(startIndex, (endIndex - startIndex) + 1);

            return true;
        }

        protected List<string> GetDictionaryPhrasesContaining(string word)
        {
            List<string> allPhrases = DictionaryPhraseCacheRaw;
            List<string> phrases = null;

            foreach (string phrase in allPhrases)
            {
                if (TextUtilities.ContainsWholeWordCaseInsensitive(phrase, word))
                {
                    if (phrases == null)
                        phrases = new List<string>();

                    phrases.Add(phrase);

                }
            }

            return phrases;
        }

        public List<string> TooCommonKeyWords;

        protected List<string> _InflectedPhrases;
        protected List<string> InflectedPhrases
        {
            get
            {
                if (_InflectedPhrases == null)
                {
                    List<string> deinflectionKeys = DeinflectionDatabase.GetAllKeys(TargetLanguageIDs.First()).Cast<string>().ToList();

                    _InflectedPhrases = new List<string>();

                    foreach (string deinflectionKey in deinflectionKeys)
                    {
                        if (!deinflectionKey.Contains(" "))
                            continue;

                        _InflectedPhrases.Add(deinflectionKey);
                    }
                }

                return _InflectedPhrases;
            }
        }

        public virtual string[] GetContainingPhrasesCached(
            string keyWord,
            LanguageID languageID,
            bool isDoDictionaryPhrases,
            bool isDoInflectionPhrases)
        {
            string[] containingPhrases;
            const int maxPhrases = 1000;

            //if (keyWord == "prepared")
            //    ApplicationData.Global.PutConsoleMessage(keyWord);

            if (ContainingPhrasesCache.TryGetValue(keyWord, out containingPhrases))
                return containingPhrases;

            if (IsIgnoreCommonNonInflectableWord(keyWord, languageID))
            {
                ContainingPhrasesCache.Add(keyWord, null);
                return null;
            }

            List<string> phraseCandidates = null;

            if (isDoDictionaryPhrases)
            {
                phraseCandidates = GetDictionaryPhrasesContaining(keyWord);

                if (phraseCandidates != null)
                {
                    if (phraseCandidates.Count() > maxPhrases)
                    {
                        if (TooCommonKeyWords == null)
                            TooCommonKeyWords = new List<string>();
                        TooCommonKeyWords.Add(keyWord);
                        ApplicationData.Global.PutConsoleMessage("GetContainingPhrasesCached: More than " + maxPhrases.ToString() + " dictionary phrases for: " + keyWord);
                        phraseCandidates.Clear();
                    }
                }
            }

            if (isDoInflectionPhrases && (GetDeinflectionCached(keyWord, languageID) != null))
            {
#if false
                List<string> phraseCandidatesInflections = InflectedPhrases.Where(
                    x => TextUtilities.ContainsWholeWordCaseInsensitive(x, keyWord)).ToList();
#else
                string startKey = keyWord + " ";
                string middleKey = " " + keyWord + " ";
                string endKey = " " + keyWord;

                List<string> phraseCandidatesInflections = InflectedPhrases.Where(x =>
                    x.Contains(keyWord) &&
                        (x.EndsWith(endKey) || x.StartsWith(startKey) || x.Contains(middleKey))).ToList();
#endif

                // If it's this big, it's probably not a main word.
                if (phraseCandidatesInflections.Count() < maxPhrases)
                {
                    if (phraseCandidates == null)
                        phraseCandidates = phraseCandidatesInflections;
                    else
                        phraseCandidates.AddRange(phraseCandidatesInflections);
                }
                else
                {
                    if (TooCommonKeyWords == null)
                        TooCommonKeyWords = new List<string>();
                    TooCommonKeyWords.Add(keyWord);
                    ApplicationData.Global.PutConsoleMessage("GetContainingPhrasesCached: More than " + maxPhrases.ToString() + " inflection phrases for: " + keyWord);
                    phraseCandidates = null;
                }
            }

            if ((phraseCandidates == null) || (phraseCandidates.Count() == 0))
            {
                ContainingPhrasesCache.Add(keyWord, null);
                return null;
            }

            phraseCandidates.Sort((x, y) => y.Length.CompareTo(x.Length));
            containingPhrases = phraseCandidates.ToArray();
            ContainingPhrasesCache.Add(keyWord, containingPhrases);

            return containingPhrases;
        }
#else
        public virtual string[] GetContainingPhrasesCached(
            string keyWord,
            LanguageID languageID)
        {
            string[] containingPhrases;

            if (ContainingPhrasesCache.TryGetValue(keyWord, out containingPhrases))
                return containingPhrases;

            Deinflection deinflection = GetDeinflectionCached(keyWord, languageID);

            if (deinflection == null)
            {
                ContainingPhrasesCache.Add(keyWord, null);
                return null;
            }

            List<Deinflection> deinflections = DeinflectionDatabase.Lookup(keyWord, MatchCode.ContainsWord, languageID, 0, 0);

            if (deinflections != null)
            {
                List<string> phrases = new List<string>();

                foreach (Deinflection deinflectionCandidate in deinflections)
                {
                    string str = deinflectionCandidate.KeyString;

                    if (!str.Contains(" "))
                        continue;

                    if (phrases == null)
                        phrases = new List<string>();

                    phrases.Add(str);
                }

                if ((phrases != null) && (phrases.Count() != 0))
                {
                    phrases.Sort((x, y) => y.Length.CompareTo(x.Length));
                    containingPhrases = phrases.ToArray();
                }
            }

            ContainingPhrasesCache.Add(keyWord, containingPhrases);

            return containingPhrases;
        }
#endif

        public Dictionary<string, string[]> ContainingPhrasesCache
        {
            get
            {
                if (_ContainingPhrasesCache == null)
                    _ContainingPhrasesCache = new Dictionary<string, string[]>();
                return _ContainingPhrasesCache;
            }
            set
            {
                _ContainingPhrasesCache = value;
            }
        }

        public virtual string[] CommonNonInflectableWords
        {
            get
            {
                return null;
            }
        }

        public virtual bool IsIgnoreCommonNonInflectableWord(string word, LanguageID languageID)
        {
            string[] commonNonInflectableWords = CommonNonInflectableWords;

            if (commonNonInflectableWords == null)
                return false;

            if (commonNonInflectableWords.Contains(word.ToLower()))
                return true;

            return false;
        }
    }
}

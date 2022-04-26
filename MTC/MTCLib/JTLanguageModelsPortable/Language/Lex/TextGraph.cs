using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Language
{
    public class TextGraph : LanguageString
    {
        protected List<List<TextGraphNode>> TextSlots;
        protected LanguageDescription _LanguageDescription;
        protected LanguageTool _Tool;
        protected List<TextGraphNode> _Path;

        public TextGraph(object key, LanguageID languageID, string text, LanguageTool tool) : base(key, languageID, text)
        {
            TextSlots = new List<List<TextGraphNode>>(TextLength);
            _LanguageDescription = LanguageLookup.GetLanguageDescription(languageID);
            _Tool = tool;
            _Path = null;
        }

        public TextGraph(LanguageString other, LanguageTool tool)
            : base(other)
        {
            TextSlots = new List<List<TextGraphNode>>(TextLength);
            _LanguageDescription = LanguageLookup.GetLanguageDescription(LanguageID);
            _Tool = tool;
            _Path = null;
        }

        public TextGraph(object key, LanguageString other, LanguageTool tool)
            : base(key, other)
        {
            TextSlots = new List<List<TextGraphNode>>(TextLength);
            _LanguageDescription = LanguageLookup.GetLanguageDescription(LanguageID);
            _Tool = tool;
            _Path = null;
        }

        public TextGraph(XElement element, LanguageTool tool)
        {
            OnElement(element);
            _LanguageDescription = LanguageLookup.GetLanguageDescription(LanguageID);
            _Tool = tool;
            _Path = null;
        }

        public TextGraph()
        {
            ClearTextGraph();
        }

        public override void Clear()
        {
            base.Clear();
            ClearTextGraph();
        }

        public void ClearTextGraph()
        {
            TextSlots = null;
            _LanguageDescription = null;
            _Tool = null;
            _Path = null;
        }

        public override IBaseObject Clone()
        {
            return new TextGraph(this, _Tool);
        }

        public void ClearSlots()
        {
            int textIndex;
            int length = TextLength;

            TextSlots = new List<List<TextGraphNode>>();

            for (textIndex = 0; textIndex < length; textIndex++)
                TextSlots.Add(null);
        }

        public List<TextGraphNode> GetSlot(int characterIndex)
        {
            if (TextSlots == null)
                return null;

            if ((characterIndex >= 0) && (characterIndex < TextSlots.Count()))
                return TextSlots[characterIndex];

            return null;
        }

        public bool IsCharacterIndexAtEnd(int index)
        {
            if (TextSlots == null)
                return true;

            int length = TextLength;

            if (index >= length)
                return true;

            for (; index < length; index++)
            {
                if (TextSlots[index] != null)
                    return false;
            }

            return true;
        }

        public bool ParseBest()
        {
            if (!ParseRaw())
                return false;

            CalculateWeights();
            _Path = GetBestPath();

            //if (HasPathProblem())
            //    ResolveProblemsCheck();

            return true;
        }

        public bool ParseAll()
        {
            if (!ParseRaw())
                return false;

            _Path = GetAllNodes();

            return true;
        }

        public bool ParseMatchingWordRuns(
            List<TextRun> wordRuns)
        {
            if (!ParseRaw())
                return false;

            _Path = GetBestPathMatchingWordRuns(wordRuns);

            return true;
        }

        public bool ParseRaw()
        {
            if (TextSlots == null)
                return false;

            string text = Text;
            int textIndex;
            int length = TextLength;
            List<TextGraphNode> slot = null;
            List<int> slotIndexStack = new List<int>() { 0 };
            int stackIndex = 0;
            bool isSpacedLanguage = false;
            bool hasZeroWidthSpaces = false;

            ClearSlots();

            if (length == 0)
                return true;

            if (_LanguageDescription == null)
                _LanguageDescription = LanguageLookup.GetLanguageDescription(LanguageID);

            isSpacedLanguage = !_LanguageDescription.CharacterBased;

            if (!isSpacedLanguage)
            {
                if (text.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                    hasZeroWidthSpaces = true;
                else if (text.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                    hasZeroWidthSpaces = true;
            }

            if (isSpacedLanguage || hasZeroWidthSpaces)
            {
                int startIndex;

                for (startIndex = 0; startIndex < length;)
                {
                    char c = text[startIndex];

                    if (LanguageLookup.SpaceCharacters.Contains(c))
                    {
                        startIndex++;
                        continue;
                    }

                    for (textIndex = startIndex + 1; textIndex < length; textIndex++)
                    {
                        c = text[textIndex];

                        if (LanguageLookup.SpaceCharacters.Contains(c))
                            break;
                    }

                    if (textIndex <= length)
                    {
                        int wordLength = textIndex - startIndex;
                        ParseOneSlotNodeSpaced(startIndex, wordLength, out slot);
                    }

                    startIndex = textIndex + 1;
                }
            }
            else
            {
                while (stackIndex >= 0)
                {
                    textIndex = slotIndexStack[stackIndex];
                    stackIndex--;

                    if (textIndex >= length)
                        continue;

                    if (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[textIndex]) ||
                        char.IsPunctuation(text[textIndex]))
                    {
                        textIndex++;

                        while ((textIndex < length) &&
                                (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[textIndex]) ||
                                    char.IsPunctuation(text[textIndex])))
                            textIndex++;
                    }

                    if (textIndex == length)
                        continue;

                    if (TextSlots[textIndex] != null)
                        continue;

                    int maxLength = 0;

                    for (int i = textIndex;
                            (i < length) &&
                                !LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[i]) &&
                                    !char.IsPunctuation(text[i]);
                            i++)
                        maxLength++;

                    if (ParseSlotNodes(
                        textIndex,
                        maxLength,
                        out slot))
                    {
                        foreach (TextGraphNode textNode in slot)
                        {
                            stackIndex++;

                            if (stackIndex >= slotIndexStack.Count())
                                slotIndexStack.Add(textNode.Stop);
                            else
                                slotIndexStack[stackIndex] = textNode.Stop;
                        }
                    }
                }
            }

            return true;
        }

        protected bool ParseSlotNodes(
            int textIndex,
            int maxLength,
            out List<TextGraphNode> slot)
        {
            DictionaryEntry stemEntry = null;
            DictionaryEntry nonStemEntry = null;
            int index;
            string ending;
            int dictionaryEntryMaxLength = _LanguageDescription.LongestDictionaryEntryLength;
            int suffixMaxLength = _LanguageDescription.LongestSuffixLength;
            int endingMaxLength;
            string text = Text;
            string word;
            int wordLength;
            List<DictionaryEntry> inflectionEntries;
            bool foundOneOrTwo;
            bool returnValue = true;

            slot = new List<TextGraphNode>();
            TextSlots[textIndex] = slot;

            if ((dictionaryEntryMaxLength != 0) && (dictionaryEntryMaxLength < maxLength))
                maxLength = dictionaryEntryMaxLength;

            for (index = textIndex + maxLength; index > textIndex; index--)
            {
                wordLength = index - textIndex;
                word = Text.Substring(textIndex, wordLength);

                if (_Tool.IsNumberString(word))
                {
                    stemEntry = null;
                    nonStemEntry = _Tool.CreateNumberDictionaryEntry(LanguageID, word);
                    foundOneOrTwo = true;
                }
                else
                    foundOneOrTwo = _Tool.GetStemAndNonStemDictionaryEntry(word, out stemEntry, out nonStemEntry);

                if (foundOneOrTwo)
                {
                    if (stemEntry != null)
                    {
                        //if ((stemEntry.LanguageID != LanguageID) && (nonStemEntry != null) && (nonStemEntry.LanguageID == LanguageID))
                        //{
                        //    TextGraphNode node = new TextGraphNode(
                        //        textIndex,
                        //        wordLength,
                        //        word,
                        //        nonStemEntry,
                        //        1);     // For now, set weight to 1.
                        //    slot.Add(node);
                        //}
                        //else
                        {
                            endingMaxLength = maxLength - word.Length;

                            if (endingMaxLength != 0)
                            {
                                if ((suffixMaxLength != 0) && (endingMaxLength > suffixMaxLength))
                                    endingMaxLength = suffixMaxLength;

                                ending = text.Substring(index, endingMaxLength);

                                if (_Tool.GetPossibleInflections(
                                        stemEntry,
                                        word,
                                        ending,
                                        false,
                                        out inflectionEntries))
                                {
                                    foreach (DictionaryEntry inflectionEntry in inflectionEntries)
                                    {
                                        string inflectionText = inflectionEntry.KeyString;
                                        int inflectionLength = inflectionText.Length;
                                        TextGraphNode node = new TextGraphNode(
                                            textIndex,
                                            inflectionLength,
                                            inflectionText,
                                            inflectionEntry,
                                            1);     // For now, set weight to 1.
                                        slot.Add(node);
                                    }
                                }
                                else if (nonStemEntry != null)
                                {
                                    TextGraphNode node = new TextGraphNode(
                                        textIndex,
                                        wordLength,
                                        word,
                                        stemEntry,
                                        1);     // For now, set weight to 1.
                                    slot.Add(node);
                                }
                                else /*if (stemEntry.HasSenseWithoutStem())*/
                                {
                                    TextGraphNode node = new TextGraphNode(
                                        textIndex,
                                        wordLength,
                                        word,
                                        stemEntry,
                                        1);     // For now, set weight to 1.
                                    slot.Add(node);
                                }
                            }
                            else if (nonStemEntry != null)
                            {
                                TextGraphNode node = new TextGraphNode(
                                    textIndex,
                                    wordLength,
                                    word,
                                    nonStemEntry,
                                    1);     // For now, set weight to 1.
                                slot.Add(node);
                            }
                            else /*if (stemEntry.HasSenseWithoutStem())*/
                            {
                                TextGraphNode node = new TextGraphNode(
                                    textIndex,
                                    wordLength,
                                    word,
                                    stemEntry,
                                    1);     // For now, set weight to 1.
                                slot.Add(node);
                            }
                        }
                    }
                    else if (nonStemEntry != null)
                    {
                        TextGraphNode node = new TextGraphNode(
                            textIndex,
                            wordLength,
                            word,
                            nonStemEntry,
                            1);     // For now, set weight to 1.
                        slot.Add(node);
                    }
                }

                if (wordLength == 1)
                    break;
            }

            if (slot.Count() == 0)
            {
                if (!_Tool.ParseForeignWord(Text, textIndex, maxLength, out word))
                    word = Text.Substring(textIndex, 1);
                TextGraphNode badNode = new TextGraphNode(
                    textIndex,
                    word.Length,
                    word,
                    null,
                    0.0);
                slot.Add(badNode);
            }

            return returnValue;
        }

        protected bool ParseOneSlotNode(
            int textIndex,
            int maxLength,
            out List<TextGraphNode> slot)
        {
            DictionaryEntry stemEntry = null;
            DictionaryEntry nonStemEntry = null;
            int index;
            string ending;
            int suffixMaxLength = _LanguageDescription.LongestSuffixLength;
            int endingMaxLength;
            string text = Text;
            string word;
            int wordLength;
            List<DictionaryEntry> inflectionEntries;
            bool foundOneOrTwo;
            bool returnValue = true;

            slot = new List<TextGraphNode>();
            TextSlots[textIndex] = slot;

            for (index = textIndex + maxLength; index > textIndex; index--)
            {
                wordLength = index - textIndex;
                word = Text.Substring(textIndex, wordLength);

                if (_Tool.IsNumberString(word))
                {
                    stemEntry = null;
                    nonStemEntry = _Tool.CreateNumberDictionaryEntry(LanguageID, word);
                    foundOneOrTwo = true;
                }
                else
                    foundOneOrTwo = _Tool.GetStemAndNonStemDictionaryEntry(word, out stemEntry, out nonStemEntry);

                if (foundOneOrTwo)
                {
                    if (stemEntry != null)
                    {
                        endingMaxLength = maxLength - word.Length;

                        if (endingMaxLength != 0)
                        {
                            if ((suffixMaxLength != 0) && (endingMaxLength > suffixMaxLength))
                                endingMaxLength = suffixMaxLength;

                            ending = text.Substring(index, endingMaxLength);

                            if (_Tool.GetPossibleInflections(
                                    stemEntry,
                                    word,
                                    ending,
                                    false,
                                    out inflectionEntries))
                            {
                                foreach (DictionaryEntry inflectionEntry in inflectionEntries)
                                {
                                    string inflectionText = inflectionEntry.KeyString;
                                    int inflectionLength = inflectionText.Length;

                                    if (inflectionLength == maxLength)
                                    {
                                        TextGraphNode node = new TextGraphNode(
                                            textIndex,
                                            inflectionLength,
                                            inflectionText,
                                            inflectionEntry,
                                            1);     // For now, set weight to 1.
                                        slot.Add(node);
                                        return true;
                                    }
                                }
                            }
                            else if (nonStemEntry != null)
                            {
                                if (wordLength == maxLength)
                                {
                                    TextGraphNode node = new TextGraphNode(
                                        textIndex,
                                        wordLength,
                                        word,
                                        stemEntry,
                                        1);     // For now, set weight to 1.
                                    slot.Add(node);
                                    return true;
                                }
                            }
                            else
                            {
                                if (wordLength == maxLength)
                                {
                                    TextGraphNode node = new TextGraphNode(
                                        textIndex,
                                        wordLength,
                                        word,
                                        stemEntry,
                                        1);     // For now, set weight to 1.
                                    slot.Add(node);
                                }
                            }
                        }
                        else if (nonStemEntry != null)
                        {
                            if (wordLength == maxLength)
                            {
                                TextGraphNode node = new TextGraphNode(
                                    textIndex,
                                    wordLength,
                                    word,
                                    nonStemEntry,
                                    1);     // For now, set weight to 1.
                                slot.Add(node);
                                return true;
                            }
                        }
                        else if (wordLength == maxLength)
                        {
                            TextGraphNode node = new TextGraphNode(
                                textIndex,
                                wordLength,
                                word,
                                stemEntry,
                                1);     // For now, set weight to 1.
                            slot.Add(node);
                            return true;
                        }
                    }
                    else if (nonStemEntry != null)
                    {
                        if (wordLength == maxLength)
                        {
                            TextGraphNode node = new TextGraphNode(
                                textIndex,
                                wordLength,
                                word,
                                nonStemEntry,
                                1);     // For now, set weight to 1.
                            slot.Add(node);
                            return true;
                        }
                    }
                }

                if (wordLength == 1)
                    break;
            }

            if (slot.Count() == 0)
            {
                if (!_Tool.ParseForeignWord(Text, textIndex, maxLength, out word))
                    word = Text.Substring(textIndex, 1);
                TextGraphNode badNode = new TextGraphNode(
                    textIndex,
                    word.Length,
                    word,
                    null,
                    0.0);
                slot.Add(badNode);
            }

            return returnValue;
        }

        protected bool ParseOneSlotNodeSpaced(
            int textIndex,
            int maxLength,
            out List<TextGraphNode> slot)
        {
            string text = Text;
            List<DictionaryEntry> dictionaryEntries = null;
            DictionaryEntry dictionaryEntry = null;
            int startIndex = textIndex;
            int endIndex = textIndex + maxLength;
            string word;
            int wordLength;
            char chr;
            bool returnValue = true;

            slot = null;

            // Skip leading punctuation.
            for (; startIndex < endIndex; startIndex++)
            {
                chr = text[startIndex];

                if (!char.IsPunctuation(chr))
                    break;
            }

            // If all punctuation...
            if (startIndex == endIndex)
                return false;

            // Skip trailing punctuation.
            for (; endIndex > startIndex; endIndex--)
            {
                chr = text[endIndex - 1];

                if (!char.IsPunctuation(chr))
                    break;
            }

            // Strip trailing number (possibly footnote).
            chr = text[startIndex];

            if (!char.IsDigit(chr))
            {
                for (; endIndex > startIndex; endIndex--)
                {
                    chr = text[endIndex - 1];

                    if (!char.IsDigit(chr))
                        break;
                }
            }

            slot = new List<TextGraphNode>();

            wordLength = endIndex - startIndex;
            word = text.Substring(startIndex, wordLength);

            dictionaryEntries = _Tool.GetDictionaryLanguageEntries(word, _Tool.LanguageIDs);

            if (dictionaryEntry == null)
                dictionaryEntry = _Tool.GetInflectionDictionaryLanguageEntry(word, LanguageID);

            if (dictionaryEntry != null)
            {
                TextSlots[textIndex] = slot;

                TextGraphNode node = new TextGraphNode(
                    startIndex,
                    wordLength,
                    word,
                    dictionaryEntry,
                    1);     // For now, set weight to 1.

                slot.Add(node);
            }
            else
            {
                if (!_Tool.ParseForeignWord(text, startIndex, wordLength, out word))
                    word = text.Substring(startIndex, wordLength);
                else
                    wordLength = word.Length;

                TextGraphNode badNode = new TextGraphNode(
                    startIndex,
                    wordLength,
                    word,
                    null,
                    1);     // For now, set weight to 1.
                slot.Add(badNode);
            }

            return returnValue;
        }

        protected void AddTextNode(TextGraphNode textNode)
        {
            int textIndex = textNode.Start;
            List<TextGraphNode> slot = TextSlots[textIndex];

            if (slot == null)
            {
                slot = new List<TextGraphNode>();
                TextSlots[textIndex] = slot;
            }

            slot.Add(textNode);
        }

        public void CalculateWeights()
        {
            TextGraphNodeComparer comparer = new TextGraphNodeComparer(this, _Tool, LanguageID);

            foreach (List<TextGraphNode> slot in TextSlots)
            {
                if ((slot == null) || (slot.Count() == 0))
                    continue;

                if (slot.Count() != 1)
                {
                    slot.Sort(comparer);

                    int weight = 1;
                    //int weight = slot.Count();

                    foreach (TextGraphNode node in slot)
                    {
                        node.Weight = weight++;       // Set weight, longest having the highest weight.
                        //node.Weight = weight--;     // Set weight, shortest having the highest weight.
                    }
                }
            }
        }

        public bool HasPathProblem()
        {
            int pathCount = _Path.Count();
            int pathIndex;
            TextGraphNode node;

            for (pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                node = _Path[pathIndex];

                // Fix to call tool to check for language mismatch or kana-only
                if (_Tool.HasTextGraphNodeProblem(node))
                    return true;
            }

            return false;
        }

        public void ResolveProblemsCheck()
        {
            int slotCount = TextSlots.Count();
            int slotIndex;
            List<TextGraphNode> slot;
            List<int> slotReorderCounts = new List<int>(slotCount);
            int problemCount = 0;
            bool notDone = true;

            for (slotIndex = 0; slotIndex < slotCount; slotIndex++)
                slotReorderCounts.Add(0);

            while (notDone)
            {
                retry:
                {
                    int pathCount = _Path.Count();
                    int pathIndex;
                    TextGraphNode node;
                    TextGraphNode lastNode = null;

                    problemCount = 0;

                    for (pathIndex = 0; pathIndex < pathCount; pathIndex++, lastNode = node)
                    {
                        node = _Path[pathIndex];

                        // Fix to call tool to check for language mismatch or kana-only
                        if (_Tool.HasTextGraphNodeProblem(node))
                        {
                            problemCount++;
                            if (lastNode != null)
                            {
                                slotIndex = lastNode.Start;
                                slot = TextSlots[slotIndex];
                                if (slot != null)
                                {
                                    if (slot.Count() > 1)
                                    {
                                        if (slotReorderCounts[slotIndex] < slot.Count())
                                        {
                                            TextGraphNode tempNode = slot[0];
                                            slot.RemoveAt(slot.Count() - 1);
                                            slot.Insert(0, tempNode);
                                            int weight = 1;
                                            foreach (TextGraphNode slotNode in slot)
                                                slotNode.Weight = weight++;
                                            slotReorderCounts[slotIndex] = slotReorderCounts[slotIndex] + 1;
                                            _Path = GetBestPath();
                                            goto retry;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (pathIndex == pathCount)
                    {
                        if (problemCount != 0)
                            CalculateWeights();

                        notDone = false;
                    }
                }
            }
        }

        public List<TextGraphNode> Path
        {
            get
            {
                return _Path;
            }
        }

        public List<TextGraphNode> GetBestPath()
        {
            List<TextGraphNode> path = new List<TextGraphNode>();
            if (TextSlots == null)
                return path;
            int textIndex;
            int textLength = TextLength;

            for (textIndex = 0; textIndex < textLength; )
            {
                List<TextGraphNode> slot = TextSlots[textIndex];

                if (slot != null)
                {
                    TextGraphNode bestNode = GetBestSlotNode(slot);

                    if (bestNode != null)
                    {
                        if (bestNode.Length != 0)
                        {
                            path.Add(bestNode);
                            textIndex += bestNode.Length;
                        }
                        else
                            textIndex++;
                    }
                    else
                        textIndex++;
                }
                else
                    textIndex++;
            }

            return path;
        }

        public List<TextGraphNode> GetAllNodes()
        {
            List<TextGraphNode> nodes = new List<TextGraphNode>();
            if (TextSlots == null)
                return nodes;
            int textIndex;
            int textLength = TextLength;

            for (textIndex = 0; textIndex < textLength; textIndex++)
            {
                List<TextGraphNode> slot = TextSlots[textIndex];

                if (slot != null)
                {
                    foreach (TextGraphNode node in slot)
                        nodes.Add(node);
                }
            }

            return nodes;
        }

        public List<TextGraphNode> GetBestPathMatchingWordRuns(List<TextRun> wordRuns)
        {
            List<TextGraphNode> path = new List<TextGraphNode>();
            if (TextSlots == null)
                return path;
            int textIndex;
            int textLength = TextLength;

            for (textIndex = 0; textIndex < textLength;)
            {
                List<TextGraphNode> slot = TextSlots[textIndex];

                if (slot != null)
                {
                    TextRun wordRun = wordRuns.FirstOrDefault(x => x.Start == textIndex);
                    TextGraphNode bestNode;

                    if (wordRun != null)
                        bestNode = GetBestSlotNodeMatchingWordRun(slot, wordRun);
                    else
                        bestNode = GetBestSlotNode(slot);

                    if ((bestNode != null) && (bestNode.Length != 0))
                    {
                        path.Add(bestNode);
                        textIndex += bestNode.Length;
                    }
                    else
                        textIndex++;
                }
                else
                    textIndex++;
            }

            return path;
        }

        public void GetBestTextRuns(List<TextRun> textRuns)
        {
            GetRunsFromPath(_Path, textRuns);
        }

        public static void GetRunsFromPath(List<TextGraphNode> path, List<TextRun> textRuns)
        {
            textRuns.Clear();

            if (path != null)
            {
                foreach (TextGraphNode node in path)
                    textRuns.Add(new TextRun(node));
            }
        }

        public TextGraphNode GetBestSlotNode(List<TextGraphNode> slot)
        {
            TextGraphNode bestNode = null;

            if (slot != null)
            {
                double bestWeight = -1.0;

                foreach (TextGraphNode node in slot)
                {
                    if (node.Entry == null)
                        continue;

                    if (node.Weight > bestWeight)
                    {
                        if (node.Stop == TextLength)
                        {
                            bestWeight = node.Weight;
                            bestNode = node;
                        }
                        else
                        {
                            List<TextGraphNode> nextSlot = TextSlots[node.Stop];

                            // If it was punctuation...
                            if (nextSlot == null)
                            {
                                bestWeight = node.Weight;
                                bestNode = node;
                            }
                            // Else if it was not a not-found entry
                            else if ((nextSlot.Count() != 0) && (nextSlot.First().Entry != null))
                            {
                                bestWeight = node.Weight;
                                bestNode = node;
                            }
                        }
                    }
                }

                if ((bestNode == null) && (slot.Count() != 0))
                    bestNode = slot.Last();
            }

            return bestNode;
        }

        public TextGraphNode GetBestSlotNodeMatchingWordRun(
            List<TextGraphNode> slot,
            TextRun wordRun)
        {
            TextGraphNode bestNode = null;

            if (slot != null)
            {
                double bestWeight = -1.0;

                foreach (TextGraphNode node in slot)
                {
                    //if (node.Entry == null)
                    //    continue;

                    if (wordRun != null)
                    {
                        if (node.Length != wordRun.Length)
                            continue;
                    }

                    if (node.Weight > bestWeight)
                    {
                        if (node.Stop == TextLength)
                        {
                            if ((bestNode == null) || (bestNode.Entry == null))
                            {
                                bestWeight = node.Weight;
                                bestNode = node;
                            }
                        }
                        else
                        {
                            List<TextGraphNode> nextSlot = TextSlots[node.Stop];

                            if ((bestNode == null) || (bestNode.Entry == null))
                            {
                                // If it was punctuation...
                                if (nextSlot == null)
                                {
                                    bestWeight = node.Weight;
                                    bestNode = node;
                                }
                                // Else if it was not a not-found entry
                                else if ((nextSlot.Count() != 0) /*&& (nextSlot.First().Entry != null)*/)
                                {
                                    bestWeight = node.Weight;
                                    bestNode = node;
                                }
                            }
                        }
                    }
                }
            }

            return bestNode;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (TextSlots != null)
            {
                int count = TextSlots.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    List<TextGraphNode> slot = TextSlots[index];
                    if (slot != null)
                    {
                        foreach (TextGraphNode node in slot)
                            element.Add(node.GetElement("TextGraphNode"));
                    }
                }
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "TextGraphNode":
                    if (TextSlots == null)
                        TextSlots = new List<List<TextGraphNode>>(TextLength);
                    TextGraphNode node = new TextGraphNode(childElement);
                    int slotIndex = node.Start;
                    List<TextGraphNode> slot = TextSlots[slotIndex];
                    if (slot == null)
                    {
                        slot = new List<TextGraphNode>();
                        TextSlots[slotIndex] = slot;
                    }
                    slot.Add(node);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(TextGraphNode object1, TextGraphNode object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            int returnValue = TextRun.Compare(object1, object2);
            if (returnValue != 0)
                return returnValue;
            if (object1.Weight > object2.Weight)
                return 1;
            else if (object1.Weight < object2.Weight)
                return -1;
            return 0;
        }
    }
}

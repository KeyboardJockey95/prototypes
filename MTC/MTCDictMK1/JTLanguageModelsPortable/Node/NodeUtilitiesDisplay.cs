using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    partial class NodeUtilities : ControllerUtilities
    {
        public void OutputDisplayAlignment(
            StreamWriter writer,
            MultiLanguageItem studyItem,
            int sentenceIndex,
            string namePath,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool isDisplayDictionary,
            bool isUseDictionaryNoDeinflection,
            bool isUseDictionaryWithDeinflection,
            bool isUseAlignmentDictionary,
            bool isUseWordDictionary,
            Dictionary<string, ProbableDefinition> targetAlignmentDictionary,
            Dictionary<string, ProbableDefinition> targetWordDictionary,
            Dictionary<string, ProbableDefinition> hostAlignmentDictionary,
            Dictionary<string, ProbableDefinition> hostWordDictionary)
        {
            if (!String.IsNullOrEmpty(namePath))
                writer.Write(namePath + "." + sentenceIndex.ToString() + ":\n\n");

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                foreach (LanguageID hostLanguageID in hostLanguageIDs)
                {
                    OutputDisplayAlignment(
                        writer,
                        studyItem,
                        sentenceIndex,
                        targetLanguageID,
                        hostLanguageID,
                        isDisplayDictionary,
                        isUseDictionaryNoDeinflection,
                        isUseDictionaryWithDeinflection,
                        isUseAlignmentDictionary,
                        isUseWordDictionary,
                        targetAlignmentDictionary,
                        targetWordDictionary);
                }
            }

            writer.WriteLine("");

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                foreach (LanguageID targetLanguageID in targetLanguageIDs)
                {
                    OutputDisplayAlignment(
                        writer,
                        studyItem,
                        sentenceIndex,
                        hostLanguageID,
                        targetLanguageID,
                        isDisplayDictionary,
                        isUseDictionaryNoDeinflection,
                        isUseDictionaryWithDeinflection,
                        isUseAlignmentDictionary,
                        isUseWordDictionary,
                        hostAlignmentDictionary,
                        hostWordDictionary);
                }
            }

            writer.WriteLine("");
        }

        public void OutputDisplayAlignment(
            StreamWriter writer,
            MultiLanguageItem studyItem,
            int sentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            bool isDisplayDictionary,
            bool isUseDictionaryNoDeinflection,
            bool isUseDictionaryWithDeinflection,
            bool isUseAlignmentDictionary,
            bool isUseWordDictionary,
            Dictionary<string, ProbableDefinition> alignmentDictionary,
            Dictionary<string, ProbableDefinition> wordDictionary)
        {
            List<string> lines;

            FormatDisplayAlignment(
                studyItem,
                sentenceIndex,
                targetLanguageID,
                hostLanguageID,
                isDisplayDictionary,
                isUseDictionaryNoDeinflection,
                isUseDictionaryWithDeinflection,
                isUseAlignmentDictionary,
                isUseWordDictionary,
                alignmentDictionary,
                wordDictionary,
                out lines);

            if (lines != null)
            {
                foreach (string line in lines)
                    writer.WriteLine(line);
            }
        }

        public void DumpAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            string namePath,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool isDisplayDictionary,
            bool isUseDictionaryNoDeinflection,
            bool isUseDictionaryWithDeinflection,
            bool isUseAlignmentDictionary,
            bool isUseWordDictionary,
            Dictionary<string, ProbableDefinition> targetAlignmentDictionary,
            Dictionary<string, ProbableDefinition> targetWordDictionary,
            Dictionary<string, ProbableDefinition> hostAlignmentDictionary,
            Dictionary<string, ProbableDefinition> hostWordDictionary)
        {
            if (!String.IsNullOrEmpty(namePath))
                DumpString(namePath + ":\n\n");

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                foreach (LanguageID hostLanguageID in hostLanguageIDs)
                {
                    DumpAlignment(
                        studyItem,
                        sentenceIndex,
                        targetLanguageID,
                        hostLanguageID,
                        isDisplayDictionary,
                        isUseDictionaryNoDeinflection,
                        isUseDictionaryWithDeinflection,
                        isUseAlignmentDictionary,
                        isUseWordDictionary,
                        targetAlignmentDictionary,
                        targetWordDictionary);
                }
            }

            DumpString("\n");

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                foreach (LanguageID targetLanguageID in targetLanguageIDs)
                {
                    DumpAlignment(
                        studyItem,
                        sentenceIndex,
                        hostLanguageID,
                        targetLanguageID,
                        isDisplayDictionary,
                        isUseDictionaryNoDeinflection,
                        isUseDictionaryWithDeinflection,
                        isUseAlignmentDictionary,
                        isUseWordDictionary,
                        hostAlignmentDictionary,
                        hostWordDictionary);
                }
            }
        }

        public void DumpAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            bool isDisplayDictionary,
            bool isUseDictionaryNoDeinflection,
            bool isUseDictionaryWithDeinflection,
            bool isUseAlignmentDictionary,
            bool isUseWordDictionary,
            Dictionary<string, ProbableDefinition> alignmentDictionary,
            Dictionary<string, ProbableDefinition> wordDictionary)
        {
            List<string> lines;

            FormatDisplayAlignment(
                studyItem,
                sentenceIndex,
                targetLanguageID,
                hostLanguageID,
                isDisplayDictionary,
                isUseDictionaryNoDeinflection,
                isUseDictionaryWithDeinflection,
                isUseAlignmentDictionary,
                isUseWordDictionary,
                alignmentDictionary,
                wordDictionary,
                out lines);

            if (lines != null)
            {
                foreach (string line in lines)
                    DumpString(line);
            }
        }

        public void FormatDisplayAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            bool isDisplayDictionary,
            bool isUseDictionaryNoDeinflection,
            bool isUseDictionaryWithDeinflection,
            bool isUseAlignmentDictionary,
            bool isUseWordDictionary,
            Dictionary<string, ProbableDefinition> alignmentDictionary,
            Dictionary<string, ProbableDefinition> wordDictionary,
            out List<string> lines)
        {
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            lines = new List<string>();

            if ((targetLanguageItem == null) || (hostLanguageItem == null))
                return;

            TextRun targetSentenceRun = targetLanguageItem.GetSentenceRun(sentenceIndex);

            if (targetSentenceRun == null)
                return;

            int targetWordIndexStart, targetWordIndexCount;

            if (!LanguageItem.GetSentenceWordRunStartIndexAndCountStatic(
                    targetSentenceRun,
                    targetLanguageItem.WordRuns,
                    out targetWordIndexStart,
                    out targetWordIndexCount))
                return;

            int targetWordIndexStop = targetWordIndexStart + targetWordIndexCount;
            List<LanguageID> targetLanguageIDs = new List<LanguageID>() { targetLanguageID };
            List<LanguageID> hostLanguageIDs = new List<LanguageID>() { hostLanguageID };
            LanguageTool tool = null;
            MultiLanguageTool multiTool = null;

            if (isDisplayDictionary)
            {
                tool = GetLanguageTool(targetLanguageID);

                if (tool != null)
                {
                    multiTool = GetMultiLanguageTool(targetLanguageID, hostLanguageIDs);

                    if ((multiTool != null) && (multiTool.LanguageToolCount() > 1))
                        tool.MultiTool = multiTool;
                }
            }

            List<List<string>> matrix = new List<List<string>>();
            int targetWordIndex;
            int bestRowCount = 0;
            StringBuilder sb = new StringBuilder();

#if false
            try
            {
                List<TextRun> phrasedWordRuns = targetLanguageItem.PhrasedWordRuns;
            }
            catch (Exception exc)
            {
                string path = "Path: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English, "/");
                sb.AppendLine(exc.Message);
                ApplicationData.Global.PutConsoleErrorMessage(path);
                sb.AppendLine(path);
            }
#endif

            for (targetWordIndex = targetWordIndexStart; targetWordIndex < targetWordIndexStop; targetWordIndex++)
            {
                List<string> column = new List<string>();
                List<string> sepColumn = null;
                TextRun targetWordRun = targetLanguageItem.GetWordRun(targetWordIndex);

                if (targetWordRun == null)
                    continue;

                TextRun targetPhraseRun = targetLanguageItem.FindLongestPhraseRunStarting(targetWordRun.Start);

                if (targetPhraseRun != null)
                {
                    for (; targetWordIndex < targetWordIndexStop; targetWordIndex++)
                    {
                        targetWordRun = targetLanguageItem.GetWordRun(targetWordIndex);

                        if (targetWordRun.Stop == targetPhraseRun.Stop)
                            break;
                    }

                    targetWordRun = targetPhraseRun;
                }

                string targetWord = targetLanguageItem.GetRunText(targetWordRun);
                string hostAlignment = String.Empty;

                WordMapping targetWordMapping = targetWordRun.GetWordMapping(hostLanguageID);

                if ((targetWordMapping != null) && targetWordMapping.HasWordIndexes())
                {
                    int targetWordMappingCount = targetWordMapping.WordIndexCount();
                    int targetWordMappingIndex;

                    for (targetWordMappingIndex = 0; targetWordMappingIndex < targetWordMappingCount; targetWordMappingIndex++)
                    {
                        int hostWordIndex = targetWordMapping.GetWordIndex(targetWordMappingIndex);
                        TextRun hostWordRun = hostLanguageItem.GetWordRun(hostWordIndex);

                        if (hostWordRun == null)
                            continue;

                        string hostWord = hostLanguageItem.GetRunText(hostWordRun);

                        if (!hostAlignment.EndsWith(hostWord, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!String.IsNullOrEmpty(hostAlignment))
                                hostAlignment += " ";

                            hostAlignment += hostWord;
                        }
                    }
                }

                column.Add(targetWord);
                column.Add(hostAlignment);

                if (isDisplayDictionary)
                {
                    List<DictionaryEntry> dictionaryEntries = null;
                    bool isInflection = false;
                    int typeCount = 0;
                    string targetKey = targetWord.ToLower();

                    if (isUseDictionaryNoDeinflection)
                        typeCount++;

                    if (isUseDictionaryWithDeinflection && (tool != null))
                        typeCount++;

                    if (isUseAlignmentDictionary && (alignmentDictionary != null))
                        typeCount++;

                    if (isUseWordDictionary && (wordDictionary != null))
                        typeCount++;

                    bool isUsePrefix = (typeCount > 1);

                    if (isUseDictionaryNoDeinflection)
                    {
                        dictionaryEntries = Repositories.Dictionary.Lookup(
                            targetKey,
                            MatchCode.Exact,
                            targetLanguageIDs,
                            0,
                            0);
                        CollectHostColumnFromDictionaryEntries(
                            column,
                            dictionaryEntries,
                            hostLanguageID,
                            (isUsePrefix ? "ND: " : null));
                    }

                    if (isUseDictionaryWithDeinflection && (tool != null))
                    {
                        DictionaryEntry dictionaryEntry = tool.LookupDictionaryEntry(
                            targetKey,
                            MatchCode.Exact,
                            targetLanguageIDs,
                            null,
                            out isInflection);

                        if (dictionaryEntry != null)
                            dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };

                        CollectHostColumnFromDictionaryEntries(
                            column,
                            dictionaryEntries,
                            hostLanguageID,
                            (isUsePrefix ? "WD: " : null));
                    }

                    if (isUseAlignmentDictionary && (alignmentDictionary != null))
                    {
                        ProbableDefinition definition;

                        if (alignmentDictionary.TryGetValue(targetKey, out definition))
                            CollectHostColumnFromProbableMeanings(
                                column,
                                definition.HostMeanings,
                                (isUsePrefix ? "AD: " : null));
                    }

                    if (isUseWordDictionary && (wordDictionary != null))
                    {
                        ProbableDefinition definition;

                        if (alignmentDictionary.TryGetValue(targetKey, out definition))
                            CollectHostColumnFromProbableMeanings(
                                column,
                                definition.HostMeanings,
                                (isUsePrefix ? "OD: " : null));
                    }
                }

                string targetSep = String.Empty;
                TextRun targetWordRunNext = targetLanguageItem.GetWordRun(targetWordIndex + 1);
                int sepStartIndex = targetWordRun.Stop;
                int sepStopIndex;

                if (targetWordRunNext != null)
                    sepStopIndex = targetWordRunNext.Start;
                else
                    sepStopIndex = targetLanguageItem.TextLength;

                targetSep = targetLanguageItem.Text.Substring(sepStartIndex, sepStopIndex - sepStartIndex).Trim();

                if (!String.IsNullOrEmpty(targetSep))
                    sepColumn = new List<string>() { targetSep };

                matrix.Add(column);

                if (sepColumn != null)
                    matrix.Add(sepColumn);

                if (column.Count() > bestRowCount)
                    bestRowCount = column.Count();
            }

            foreach (List<string> column in matrix)
            {
                int bestColumnWidth = 0;

                foreach (string item in column)
                {
                    int length = TextUtilities.DisplayLength(item);

                    if (length > bestColumnWidth)
                        bestColumnWidth = length;
                }

                while (column.Count < bestRowCount)
                    column.Add("");

                int count = column.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    string item = column[index];
                    int length = TextUtilities.DisplayLength(item);
                    column[index] = "| " + item + TextUtilities.GetSpaces((bestColumnWidth - length) + 1);
                }
            }

            int lastRowIndex = bestRowCount - 1;

            if (bestRowCount != 0)
            {
                foreach (List<string> column in matrix)
                    sb.Append(TextUtilities.GetRepeatedCharacterString('-', TextUtilities.DisplayLength(column[0])));

                sb.Append('-');
                lines.Add(sb.ToString());
                sb.Clear();
            }

            for (int rowIndex = 0; rowIndex < bestRowCount; rowIndex++)
            {
                foreach (List<string> column in matrix)
                    sb.Append(column[rowIndex]);

                sb.Append("|");
                lines.Add(sb.ToString());
                sb.Clear();

                if ((rowIndex == 0) || (rowIndex == 1) || (rowIndex == lastRowIndex))
                {
                    foreach (List<string> column in matrix)
                        sb.Append(TextUtilities.GetRepeatedCharacterString('-', TextUtilities.DisplayLength(column[rowIndex])));

                    sb.Append('-');
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
            }
        }

        public void CollectHostColumnFromDictionaryEntries(
            List<string> column,
            List<DictionaryEntry> dictionaryEntries,
            LanguageID hostLanguageID,
            string prefix)
        {
            if (dictionaryEntries != null)
            {
                foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                {
                    if (dictionaryEntry.SenseCount == 0)
                        continue;

                    foreach (Sense sense in dictionaryEntry.Senses)
                    {
                        if (sense.LanguageSynonymsCount == 0)
                            continue;

                        foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                        {
                            if (languageSynonyms.LanguageID != hostLanguageID)
                                continue;

                            if (!languageSynonyms.HasProbableSynonyms())
                                continue;

                            foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                            {
                                string synonym = probableSynonym.Meaning;
                                string text;

                                if (!String.IsNullOrEmpty(prefix))
                                    text = prefix + synonym;
                                else
                                    text = synonym;

                                column.Add(text);
                            }
                        }
                    }
                }
            }
        }

        public void CollectHostColumnFromProbableMeanings(
            List<string> column,
            List<ProbableMeaning> hostMeanings,
            string prefix)
        {
            if (hostMeanings != null)
            {
                foreach (ProbableMeaning hostMeaning in hostMeanings)
                {
                    string text;

                    if (!String.IsNullOrEmpty(prefix))
                        text = prefix + hostMeaning.Meaning;
                    else
                        text = hostMeaning.Meaning;

                    if (hostMeaning.Frequency > 0)
                        text += " F=" + hostMeaning.Frequency.ToString();

                    if (hostMeaning.Probability > 0.0f)
                        text += " P=" + hostMeaning.Probability.ToString();

                    column.Add(text);
                }
            }
        }

        public void DumpInflectionTableStatistics(InflectorTable inflectorTable)
        {
            DumpString("InflectorTable Statistics:");
            DumpString("    Inflector count: " + inflectorTable.InflectorCount().ToString());
            DumpString("    SemiRegular count: " + inflectorTable.SemiRegularCount().ToString());
            DumpString("    SemiRegulars with only one target count: " + inflectorTable.SemiRegularOneTargetCount().ToString());
        }
    }
}

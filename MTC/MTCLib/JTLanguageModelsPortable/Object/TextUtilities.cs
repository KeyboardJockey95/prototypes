using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class KeyValuePairStringIntComparer : IComparer<KeyValuePair<string, int>>
    {
        public KeyValuePairStringIntComparer()
        {
        }

        public int Compare(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
        {
            return String.Compare(x.Key, y.Key);
        }
    }

    public static partial class TextUtilities
    {
        public static char[] PeriodSeparator = new char[1] { '.' };

        public static char EscapedChar(string str, int inIndex, int inLength, out int outIndex)
        {
            if (inIndex >= inLength)
            {
                outIndex = inIndex;
                return '\0';
            }

            char outChr = str[inIndex++];
            char nextChr = (inIndex < inLength ? str[inIndex] : '\0');

            if (outChr == '\\')
            {
                switch (nextChr)
                {
                    case 't':
                        outChr = '\t';
                        break;
                    case 'n':
                        outChr = '\n';
                        break;
                    case 'r':
                        outChr = '\r';
                        break;
                    case 'f':
                        outChr = '\f';
                        break;
                    case '\\':
                        outChr = '\\';
                        break;
                    default:
                        inIndex--;
                        break;
                }

                inIndex++;
            }

            outIndex = inIndex;

            return outChr;
        }

        public static void SkipToChar(string str, char endChar, char patternChr, int inIndex, int inCount, out int outIndex)
        {
            outIndex = inIndex + 1;
            for (inIndex++; inIndex < inCount; )
            {
                if (str[inIndex] == endChar)
                {
                    inIndex++;
                    break;
                }
                else if (str[inIndex] == patternChr)
                    break;

                switch (str[inIndex++])
                {
                    case '(':
                        SkipToChar(str, ')', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '"':
                        SkipToChar(str, '"', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '<':
                        SkipToChar(str, '>', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '[':
                        SkipToChar(str, ']', patternChr, inIndex, inCount, out inIndex);
                        break;
                    default:
                        break;
                }
            }

            if (inIndex <= inCount)
                outIndex = inIndex;
        }

        public static bool SkipTo(string text, char endChar, ref int index)
        {
            int ofs = text.IndexOf(endChar, index);
            if (ofs != -1)
            {
                index = ofs;
                return true;
            }
            return false;
        }

        public static int IndexOfNthCharacterInString(string text, char chr, int nth)
        {
            int lastIndex = 0;
            int index = -1;

            for (int i = 0; i < nth; i++)
            {
                index = text.IndexOf(chr, lastIndex);

                if (index == -1)
                    break;

                lastIndex = index + 1;
            }

            return index;
        }

        public static string WrapString(string str, int lineMaxLength)
        {
            StringBuilder sb = new StringBuilder();
            int stringLength = str.Length;
            int stringIndex;
            int lineIndex = 0;

            for (stringIndex = 0; stringIndex < stringLength; stringIndex++)
            {
                char c = str[stringIndex];

                if (c == '<')
                {
                    int i;

                    for (i = stringIndex + 1; i < stringLength; i++)
                    {
                        if (str[i] == '>')
                            break;
                    }

                    string control = str.Substring(stringIndex + 1, i - (stringIndex + 1)).Trim();
                    int controlLength = control.Length;

                    switch (control)
                    {
                        case "br":
                        case "br/":
                            lineIndex = 0;
                            break;
                        default:
                            break;
                    }

                    sb.Append("<" + control + ">");
                }
                else if (c == '\n')
                {
                    lineIndex = 0;
                    sb.AppendLine("");
                }
                else
                {
                    sb.Append(c);

                    if (++lineIndex > lineMaxLength)
                    {
                        bool found = false;

                        for (int subIndex = sb.Length - 1; subIndex >= 0; subIndex--)
                        {
                            if (sb[subIndex] == ' ')
                            {
                                lineIndex = sb.Length - subIndex + 1;
                                sb.Remove(subIndex, 1);
                                sb.Insert(subIndex, "<br/>");
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            sb.Insert(sb.Length - 1, "<br/>");
                            lineIndex = 1;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static string TruncateStringCheck(
            string input,
            int maxLength,
            string ellipses,
            bool ellipsisAtMiddle,
            bool breakAtSpace,
            int maxSpaceSearch)
        {
            if (input == null)
                return String.Empty;

            if (input.Length <= maxLength)
                return input;

            if (ellipses == null)
                ellipses = "...";

            int eLen = ellipses.Length;
            string returnValue;

            if (ellipsisAtMiddle)
            {
                int halfLength = (maxLength - eLen) / 2;
                if (breakAtSpace)
                    halfLength = FindSpaceBreakBefore(input, halfLength, maxSpaceSearch);
                returnValue = input.Substring(0, halfLength) + ellipses + input.Substring(input.Length - halfLength, halfLength);
            }
            else
            {
                if (breakAtSpace)
                    maxLength = FindSpaceBreakBefore(input, maxLength - eLen, maxSpaceSearch);
                else
                    maxLength -= eLen;
                returnValue = input.Substring(0, maxLength + 1) + ellipses;
            }

            return returnValue;
        }

        public static int FindSpaceBreakBefore(string input, int startIndex, int maxSpaceSearch)
        {
            if (input.Length <= startIndex)
                return startIndex;

            int index;
            int limit = 0;

            for (index = startIndex; index >= 0; index--)
            {
                if (char.IsWhiteSpace(input[index]))
                    return index;

                if (--limit <= 0)
                    break;
            }

            return startIndex;
        }

        public static string FormatStringRuns(string str, List<TextRun> textRuns)
        {
            if (!String.IsNullOrEmpty(str))
                return String.Empty;

            if ((textRuns == null) || (textRuns.Count() == 0))
                return "(no runs)";

            int textIndex = 0;
            int textLength = str.Length;
            StringBuilder sb = new StringBuilder();
            string subStr;

            foreach (TextRun textRun in textRuns)
            {
                if (textRun.Start > textIndex)
                {
                    int len = textRun.Start - textIndex;
                    subStr = str.Substring(textIndex, len);
                    textIndex = textRun.Start;
                    sb.Append(subStr);
                }

                sb.Append("[");

                subStr = str.Substring(textRun.Start, textRun.Length);
                textIndex = textRun.Stop;
                sb.Append(subStr);

                sb.Append("]");
            }

            if (textIndex < textLength)
            {
                int len = textLength - textIndex;
                subStr = str.Substring(textIndex, len);
                sb.Append(subStr);
            }

            return sb.ToString();
        }

        public static string FormatStudyItemWordRunComparison(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            LanguageItem languageItem;
            int runIndex;
            int runCount = studyItem.GetMaxWordCount(languageIDs);
            TextRun textRun;
            int runLength;
            StringBuilder sb = new StringBuilder();

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                runLength = 0;

                foreach (LanguageID lid in languageIDs)
                {
                    languageItem = studyItem.LanguageItem(lid);

                    if (languageItem == null)
                        continue;

                    textRun = languageItem.GetWordRun(runIndex);

                    if (textRun == null)
                        continue;

                    if (textRun.Length > runLength)
                        runLength = textRun.Length;
                }

                foreach (LanguageID lid in languageIDs)
                {
                    languageItem = studyItem.LanguageItem(lid);

                    if (languageItem == null)
                        continue;

                    textRun = languageItem.GetWordRun(runIndex);

                    sb.Append("|");

                    if (textRun != null)
                    {
                        string subStr = languageItem.GetRunText(textRun);
                        subStr = String.Format("{0,-" + runLength.ToString() + "}", subStr);
                        sb.Append(subStr);
                    }

                    sb.AppendLine("|");
                }
            }

            return sb.ToString();
        }

        public static string FormatStudyItemSentenceCounts(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            LanguageItem languageItem;
            StringBuilder sb = new StringBuilder();
            string languageCode;
            int sentenceCount;

            foreach (LanguageID lid in languageIDs)
            {
                languageItem = studyItem.LanguageItem(lid);

                if (languageItem == null)
                    continue;

                sentenceCount = languageItem.SentenceRunCount();
                languageCode = lid.LanguageCultureExtensionCode;

                if (sb.Length != 0)
                    sb.Append(", ");

                sb.Append(languageCode + "[" + sentenceCount + "]");
            }

            sb.AppendLine("");

            return sb.ToString();
        }

        public static string FormatStudyItemSentenceRunComparison(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            LanguageItem languageItem;
            int runIndex;
            int runCount = studyItem.GetMaxSentenceCount(languageIDs);
            TextRun textRun;
            StringBuilder sb = new StringBuilder();
            string languageCode;

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                foreach (LanguageID lid in languageIDs)
                {
                    languageItem = studyItem.LanguageItem(lid);

                    if (languageItem == null)
                        continue;

                    textRun = languageItem.GetSentenceRun(runIndex);
                    languageCode = lid.LanguageCultureExtensionCode;

                    sb.Append(languageCode + "[" + runIndex.ToString() + "]: ");

                    if (textRun != null)
                    {
                        string subStr = languageItem.GetRunText(textRun);
                        sb.AppendLine(subStr);
                    }
                    else
                        sb.AppendLine("(no sentence run)");
                }
            }

            sb.AppendLine("");

            return sb.ToString();
        }

        public static string FormatItemString(MultiLanguageItem multiLanguageItem, string pattern, List<LanguageDescriptor> languageDescriptors, int index, int ordinal, List<object> arguments, int maxWidth)
        {
            if (String.IsNullOrEmpty(pattern) || multiLanguageItem == null)
                return null;

            StringBuilder sb = new StringBuilder();
            int patternCount = pattern.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int argIndex = 0;
            string argumentString;
            LanguageID hostLanguageID = LanguageDescriptor.SafeLanguageIDFromLanguageDescriptors(languageDescriptors, "Host", multiLanguageItem.LanguageID(0));

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(pattern, patternIndex, patternCount, out nextPatternIndex);
                char nextPatternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                if ((patternChr == '%') && (nextPatternChr == '{'))
                {
                    int i;

                    for (i = tmpIndex; i < patternCount; i++)
                    {
                        if (pattern[i] == '}')
                            break;
                    }

                    string control = pattern.Substring(tmpIndex, i - tmpIndex);
                    int controlLength = control.Length;
                    nextPatternIndex = i + 1;
                    patternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                    for (i = 0; i < controlLength; i++)
                    {
                        if (!Char.IsDigit(control[i]))
                            break;
                    }

                    if (i != 0)
                        controlLength = Convert.ToInt32(control.Substring(0, i));
                    else
                        controlLength = 0;

                    string descriptorName = null;

                    switch (control)
                    {
                        case "t":
                            descriptorName = "Target";
                            break;
                        case "ta1":
                            descriptorName = "TargetAlternate1";
                            break;
                        case "ta2":
                            descriptorName = "TargetAlternate2";
                            break;
                        case "ta3":
                            descriptorName = "TargetAlternate3";
                            break;
                        case "h":
                            descriptorName = "Host";
                            break;
                        case "d":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                                argumentString = String.Format("{0:d" + controlLength.ToString() + "}", arguments[argIndex]);
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "s":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                            {
                                if (controlLength == 0)
                                    argumentString = arguments[argIndex].ToString();
                                else
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", arguments[argIndex]);
                            }
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "e":
                            argumentString = String.Format("{0:d" + controlLength.ToString() + "}", index + 1);
                            sb.Append(argumentString);
                            break;
                        case "o":
                            argumentString = String.Format("{0:d" + controlLength.ToString() + "}", ordinal + 1);
                            sb.Append(argumentString);
                            break;
                        case "l":
                            argumentString = String.Format("{0:d" + controlLength.ToString() + "}", multiLanguageItem.AnnotationText("Label", hostLanguageID));
                            sb.Append(argumentString);
                            break;
                        case "l:":
                            if (!String.IsNullOrEmpty(multiLanguageItem.AnnotationText("Label", hostLanguageID)))
                            {
                                argumentString = String.Format("{0:d" + controlLength.ToString() + "}: ", multiLanguageItem.AnnotationText("Label", hostLanguageID));
                                sb.Append(argumentString);
                            }
                            break;
                        default:
                            sb.Append("<" + control + ">");
                            break;
                    }

                    if (descriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (languageDescriptors != null)
                            languageDescriptor = languageDescriptors.FirstOrDefault(x => (x.Name == descriptorName) && x.Used && (x.LanguageID != null));

                        if (languageDescriptor != null)
                        {
                            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageDescriptor.LanguageID);

                            if (languageItem != null)
                            {
                                string text = TextUtilities.SubstitutionCheck(languageItem.Text, languageItem.LanguageID, languageDescriptors);
                                sb.Append(text.Replace("\r", "").Replace("\n", ""));
                            }
                        }
                    }
                }
                else
                    sb.Append(patternChr);
            }

            string str = sb.ToString();

            if (maxWidth > 0)
                return WrapString(str, maxWidth);
            else
                return str;
        }

        public static string SubstitutionCheck(string str, LanguageID displayLanguageID, UserProfile userProfile)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Contains("$"))
            {
                LanguageID uiLanguageID = userProfile.UILanguageID;
                List<LanguageID> targetLanguageIDs = userProfile.TargetLanguageIDs;
                List<LanguageID> hostLanguageIDs = userProfile.HostLanguageIDs;
                int targetLanguageCount = (targetLanguageIDs == null ? 0 : targetLanguageIDs.Count());
                int hostLanguageCount = (hostLanguageIDs == null ? 0 : hostLanguageIDs.Count());
                string defaultLanguageName;
                string defaultMediaLanguageName;
                string languageName;
                string mediaLanguageName;

                if (targetLanguageCount != 0)
                {
                    defaultLanguageName = targetLanguageIDs.Last().LanguageName(displayLanguageID);
                    defaultMediaLanguageName = targetLanguageIDs.Last().MediaLanguageName(displayLanguageID);
                    if (targetLanguageCount > 3)
                    {
                        languageName = targetLanguageIDs[3].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[3].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia3", mediaLanguageName);
                    str = str.Replace("$ TargetMedia3", mediaLanguageName);
                    str = str.Replace("$Target3", languageName);
                    str = str.Replace("$ Target3", languageName);
                    if (targetLanguageCount > 2)
                    {
                        languageName = targetLanguageIDs[2].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[2].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia2", mediaLanguageName);
                    str = str.Replace("$ TargetMedia2", mediaLanguageName);
                    str = str.Replace("$Target2", languageName);
                    str = str.Replace("$ Target2", languageName);
                    if (targetLanguageCount > 1)
                    {
                        languageName = targetLanguageIDs[1].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[1].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia1", mediaLanguageName);
                    str = str.Replace("$ TargetMedia1", mediaLanguageName);
                    str = str.Replace("$Target1", languageName);
                    str = str.Replace("$ Target1", languageName);
                    languageName = targetLanguageIDs[0].LanguageName(displayLanguageID);
                    mediaLanguageName = targetLanguageIDs[0].MediaLanguageName(displayLanguageID);
                    str = str.Replace("$TargetMedia0", mediaLanguageName);
                    str = str.Replace("$ TargetMedia0", mediaLanguageName);
                    str = str.Replace("$Target0", languageName);
                    str = str.Replace("$ Target0", languageName);
                    str = str.Replace("$TargetMedia", mediaLanguageName);
                    str = str.Replace("$ TargetMedia", mediaLanguageName);
                    str = str.Replace("$Target", languageName);
                    str = str.Replace("$ Target", languageName);
                }

                if (hostLanguageCount != 0)
                {
                    defaultLanguageName = hostLanguageIDs.Last().LanguageName(displayLanguageID);
                    defaultMediaLanguageName = hostLanguageIDs.Last().MediaLanguageName(displayLanguageID);
                    if (hostLanguageCount > 3)
                    {
                        languageName = hostLanguageIDs[3].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[3].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia3", mediaLanguageName);
                    str = str.Replace("$ HostMedia3", mediaLanguageName);
                    str = str.Replace("$Host3", languageName);
                    str = str.Replace("$ Host3", languageName);
                    if (hostLanguageCount > 2)
                    {
                        languageName = hostLanguageIDs[2].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[2].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia2", mediaLanguageName);
                    str = str.Replace("$ HostMedia2", mediaLanguageName);
                    str = str.Replace("$Host2", languageName);
                    str = str.Replace("$ Host2", languageName);
                    if (hostLanguageCount > 1)
                    {
                        languageName = hostLanguageIDs[1].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[1].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia1", mediaLanguageName);
                    str = str.Replace("$ HostMedia1", mediaLanguageName);
                    str = str.Replace("$Host1", languageName);
                    str = str.Replace("$ Host1", languageName);
                    languageName = hostLanguageIDs[0].LanguageName(displayLanguageID);
                    mediaLanguageName = hostLanguageIDs[0].MediaLanguageName(displayLanguageID);
                    str = str.Replace("$HostMedia0", mediaLanguageName);
                    str = str.Replace("$ HostMedia0", mediaLanguageName);
                    str = str.Replace("$Host0", languageName);
                    str = str.Replace("$ Host0", languageName);
                    str = str.Replace("$HostMedia", mediaLanguageName);
                    str = str.Replace("$ HostMedia", mediaLanguageName);
                    str = str.Replace("$Host", languageName);
                    str = str.Replace("$ Host", languageName);
                }

                str = str.Replace("$UIMedia", uiLanguageID.MediaLanguageName(displayLanguageID));
                str = str.Replace("$ UIMedia", uiLanguageID.MediaLanguageName(displayLanguageID));
                str = str.Replace("$UI", uiLanguageID.LanguageName(displayLanguageID));
                str = str.Replace("$ UI", uiLanguageID.LanguageName(displayLanguageID));
            }

            return str;
        }

        public static string SubstitutionCheck(string str, LanguageID displayLanguageID,
            List<LanguageDescriptor> languageDescriptors)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Contains("$"))
            {
                LanguageID uiLanguageID;
                List<LanguageID> targetLanguageIDs;
                List<LanguageID> hostLanguageIDs;
                LanguageDescriptor.GetLanguageIDsFromLanguageDescriptors(languageDescriptors, out uiLanguageID,
                    out targetLanguageIDs, out hostLanguageIDs);
                int targetLanguageCount = (targetLanguageIDs == null ? 0 : targetLanguageIDs.Count());
                int hostLanguageCount = (hostLanguageIDs == null ? 0 : hostLanguageIDs.Count());
                string defaultLanguageName;
                string defaultMediaLanguageName;
                string languageName;
                string mediaLanguageName;

                if (targetLanguageCount != 0)
                {
                    defaultLanguageName = targetLanguageIDs.Last().LanguageName(displayLanguageID);
                    defaultMediaLanguageName = targetLanguageIDs.Last().MediaLanguageName(displayLanguageID);
                    if (targetLanguageCount > 3)
                    {
                        languageName = targetLanguageIDs[3].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[3].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia3", mediaLanguageName);
                    str = str.Replace("$ TargetMedia3", mediaLanguageName);
                    str = str.Replace("$Target3", languageName);
                    str = str.Replace("$ Target3", languageName);
                    if (targetLanguageCount > 2)
                    {
                        languageName = targetLanguageIDs[2].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[2].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia2", mediaLanguageName);
                    str = str.Replace("$ TargetMedia2", mediaLanguageName);
                    str = str.Replace("$Target2", languageName);
                    str = str.Replace("$ Target2", languageName);
                    if (targetLanguageCount > 1)
                    {
                        languageName = targetLanguageIDs[1].LanguageName(displayLanguageID);
                        mediaLanguageName = targetLanguageIDs[1].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$TargetMedia1", mediaLanguageName);
                    str = str.Replace("$ TargetMedia1", mediaLanguageName);
                    str = str.Replace("$Target1", languageName);
                    str = str.Replace("$ Target1", languageName);
                    languageName = targetLanguageIDs[0].LanguageName(displayLanguageID);
                    mediaLanguageName = targetLanguageIDs[0].MediaLanguageName(displayLanguageID);
                    str = str.Replace("$TargetMedia0", mediaLanguageName);
                    str = str.Replace("$ TargetMedia0", mediaLanguageName);
                    str = str.Replace("$Target0", languageName);
                    str = str.Replace("$ Target0", languageName);
                    str = str.Replace("$TargetMedia", mediaLanguageName);
                    str = str.Replace("$ TargetMedia", mediaLanguageName);
                    str = str.Replace("$Target", languageName);
                    str = str.Replace("$ Target", languageName);
                }

                if (hostLanguageCount != 0)
                {
                    defaultLanguageName = hostLanguageIDs.Last().LanguageName(displayLanguageID);
                    defaultMediaLanguageName = hostLanguageIDs.Last().MediaLanguageName(displayLanguageID);
                    if (hostLanguageCount > 3)
                    {
                        languageName = hostLanguageIDs[3].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[3].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia3", mediaLanguageName);
                    str = str.Replace("$ HostMedia3", mediaLanguageName);
                    str = str.Replace("$Host3", languageName);
                    str = str.Replace("$ Host3", languageName);
                    if (hostLanguageCount > 2)
                    {
                        languageName = hostLanguageIDs[2].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[2].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia2", mediaLanguageName);
                    str = str.Replace("$ HostMedia2", mediaLanguageName);
                    str = str.Replace("$Host2", languageName);
                    str = str.Replace("$ Host2", languageName);
                    if (hostLanguageCount > 1)
                    {
                        languageName = hostLanguageIDs[1].LanguageName(displayLanguageID);
                        mediaLanguageName = hostLanguageIDs[1].MediaLanguageName(displayLanguageID);
                    }
                    else
                    {
                        languageName = defaultLanguageName;
                        mediaLanguageName = defaultMediaLanguageName;
                    }
                    str = str.Replace("$HostMedia1", mediaLanguageName);
                    str = str.Replace("$ HostMedia1", mediaLanguageName);
                    str = str.Replace("$Host1", languageName);
                    str = str.Replace("$ Host1", languageName);
                    languageName = hostLanguageIDs[0].LanguageName(displayLanguageID);
                    mediaLanguageName = hostLanguageIDs[0].MediaLanguageName(displayLanguageID);
                    str = str.Replace("$HostMedia0", mediaLanguageName);
                    str = str.Replace("$ HostMedia0", mediaLanguageName);
                    str = str.Replace("$Host0", languageName);
                    str = str.Replace("$ Host0", languageName);
                    str = str.Replace("$HostMedia", mediaLanguageName);
                    str = str.Replace("$ HostMedia", mediaLanguageName);
                    str = str.Replace("$Host", languageName);
                    str = str.Replace("$ Host", languageName);
                }

                str = str.Replace("$UIMedia", uiLanguageID.MediaLanguageName(displayLanguageID));
                str = str.Replace("$ UIMedia", uiLanguageID.MediaLanguageName(displayLanguageID));
                str = str.Replace("$UI", uiLanguageID.LanguageName(displayLanguageID));
                str = str.Replace("$ UI", uiLanguageID.LanguageName(displayLanguageID));
            }

            return str;
        }

        public static string Dequote(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            StringBuilder sb = new StringBuilder(str.Length);

            int count = str.Length;
            int index;

            for (index = 0; index < count; index++)
            {
                char chr = str[index];

                if (chr == '"')
                {
                    for (index++; index < count; index++)
                    {
                        char subChr = str[index];

                        if (subChr == '"')
                            break;
                        else if (subChr == '\r')
                        {
                        }
                        else if (subChr == '\n')
                            sb.Append(' ');
                        else
                            sb.Append(subChr);
                    }
                }
                else
                    sb.Append(chr);
            }

            return sb.ToString();
        }

        public static string GetCanonicalText(string inputText, LanguageID languageID)
        {
            string canonicalText;
            MatchCode matchType = MatchCode.Exact;
            ConvertCanonical canonical = new ConvertCanonical(languageID, false);

            if (String.IsNullOrEmpty(inputText))
                return inputText;

            inputText = inputText.Replace('\u00A0', ' ');
            inputText = inputText.Replace('\uC2A0', ' ');

            if (!canonical.Canonical(out matchType, out canonicalText, matchType, inputText))
                canonicalText = inputText;

            return canonicalText;
        }

        public static string GetDisplayText(string inputText, LanguageID languageID, DictionaryRepository dictionaryRepository)
        {
            string displayText;
            ConvertDisplay displayConvert = new ConvertDisplay(languageID, dictionaryRepository, true);

            if (String.IsNullOrEmpty(inputText))
                displayText = inputText;
            else
                displayConvert.Display(out displayText, inputText);

            return displayText;
        }

        public static char[] RegularExpressionCharacters =
        {
            '.',
            '*',
            '[',
            '|',
            '^',
            '$'
        };

        public static bool IsMaybeRegularExpression(string pattern)
        {
            if (String.IsNullOrEmpty(pattern))
                return false;

            foreach (char c in RegularExpressionCharacters)
            {
                if (pattern.Contains(c.ToString()))
                    return true;
            }

            return false;
        }

        public static bool RegexMatch(string value, string pattern, out bool badPattern)
        {
            badPattern = false;

            try
            {
                return Regex.IsMatch(value, pattern);
            }
            catch (Exception)
            {
                badPattern = true;
            }

            return false;
        }

        // Wild card characters.
        public static char[] WildCardCharacters = { '*', '?' };

        // Returns true if the pattern has wild card characters.
        public static bool IsWildCardPattern(string pattern)
        {
            return ContainsOneOrMoreCharacters(pattern, WildCardCharacters);
        }

        // Returns 0 if no match, 1 if fuzzy match, 2 if exact match.
        public static int WildCardTextMatch(string pattern, string text, bool ignoreCase)
        {
            if (pattern == null)
                pattern = String.Empty;

            if (text == null)
                text = String.Empty;

            if (text == pattern)
                return 2;

            if (ignoreCase)
            {
                pattern = pattern.ToLower();
                text = text.ToLower();

                if (text == pattern)
                    return 1;
            }

            if (!pattern.Contains("*") && !pattern.Contains("?"))
                return 0;

            pattern = "^" + pattern.Replace("*", ".*").Replace("?", ".") + "$";

            if (Regex.IsMatch(text, pattern))
                return 1;

            return 0;
        }

        // Returns true if match.
        public static bool IsWildCardTextMatch(string pattern, string text, bool ignoreCase)
        {
            return WildCardTextMatch(pattern, text, ignoreCase) != 0;
        }

        // Returns 0 if no match, 1 if fuzzy match, 2 if exact match.
        public static int FuzzyTextMatch(string pattern, string text)
        {
            if (pattern == null)
                pattern = String.Empty;

            if (text == null)
                text = String.Empty;

            if (text == pattern)
                return 2;

            pattern = pattern.ToLower();
            text = text.ToLower();

            if (text == pattern)
                return 1;

            pattern = MakeValidFileBase(pattern);
            text = MakeValidFileBase(text);

            if (text == pattern)
                return 1;

            return 0;
        }

        public static string MakeValidFileBase(string filePath)
        {
            filePath = filePath.Replace(" ", "");
            filePath = filePath.Replace("\r", "");
            filePath = filePath.Replace("\n", "");
            filePath = filePath.Replace("\t", "");
            filePath = filePath.Replace("#", "");
            filePath = filePath.Replace("/", "_");
            filePath = filePath.Replace("-", "");
            filePath = filePath.Replace("–", "");

            foreach (char chr in LanguageLookup.PunctuationCharacters)
                filePath = filePath.Replace(chr.ToString(), "");

            return filePath;
        }

        public static string MakeValidFileBase(string filePath, int maxLength)
        {
            filePath = MakeValidFileBase(filePath);

            if (filePath.Length > maxLength)
            {
                string remainder = filePath.Substring(maxLength - 4);
                string hashString = HashString(remainder, 4);

                filePath = filePath.Substring(0, maxLength - 4) + hashString;
            }

            return filePath;
        }

        public static string GetNamePathLabelFromNamePath(List<string> namePath, string separator)
        {
            StringBuilder sb = new StringBuilder();
            if (namePath != null)
            {
                foreach (string node in namePath)
                {
                    if (sb.Length != 0)
                        sb.Append(separator);
                    sb.Append(node);
                }
            }
            return sb.ToString();
        }

        public static string GetFilePathFromNamePath(List<string> namePath, string separator)
        {
            StringBuilder sb = new StringBuilder();
            if (namePath != null)
            {
                foreach (string node in namePath)
                {
                    if (sb.Length != 0)
                        sb.Append(separator);
                    sb.Append(MakeValidFileBase(node));
                }
            }
            return sb.ToString();
        }

        public static int Hash(string text)
        {
            int value = 0;

            foreach (char chr in text)
            {
                value += chr;
            }

            return value;
        }

        public static string HashString(string text, int maxLength)
        {
            int value = Hash(text);

            string hash = value.ToString();

            if (hash.Length > maxLength)
                hash = hash.Substring(0, maxLength);

            return hash;
        }

        public static void LoadIntListFromString(List<int> list, string str)
        {
            if (list == null)
                return;

            list.Clear();

            if (String.IsNullOrEmpty(str))
                return;

            string[] strings = str.Split(new char[] { ',' });
            int count = strings.Count();

            foreach (string s in strings)
                list.Add(Convert.ToInt32(s));
        }

        public static void LoadStringListFromString(List<string> list, string str)
        {
            if (list == null)
                return;

            if (String.IsNullOrEmpty(str))
                return;

            list.Clear();

            if (String.IsNullOrEmpty(str))
                return;

            string[] strings = str.Split(new char[] { ',' });
            int count = strings.Count();

            foreach (string s in strings)
                list.Add(DecodeDelimiter(s, ","));
        }

        public static string StrongDelimiter = "(__delimiter__)";

        public static string EncodeDelimiter(string value, string delimiter)
        {
            if (value.Contains(delimiter))
                value = value.Replace(delimiter, StrongDelimiter);
            return value;
        }

        public static string DecodeDelimiter(string value, string delimiter)
        {
            if (value.Contains(StrongDelimiter))
                value = value.Replace(StrongDelimiter, delimiter);
            return value;
        }

        public static string GetDisplayStringFromObjectList<T>(List<T> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(", ");
                sb.Append(list[index].ToString());
            }

            return sb.ToString();
        }

        public static string GetDisplayStringFromStringList(List<string> list)
        {
            return GetDisplayStringFromObjectList<string>(list);
        }

        public static string GetConcatenatedStringFromStringArray(
            string[] array,
            string separator = null)
        {
            if (array == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (string str in array)
            {
                if (!String.IsNullOrEmpty(separator) && (sb.Length != 0))
                    sb.Append(separator);

                sb.Append(str);
            }

            return sb.ToString();
        }

        public static string GetStringFromObjectList<T>(List<T> list)
        {
            return GetStringFromObjectListDelimited<T>(list, ",");
        }

        public static string GetStringFromObjectListDelimited<T>(List<T> list, string delimiter)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                object item = list[index];
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(delimiter);
                sb.Append(EncodeDelimiter(item.ToString(), delimiter));
            }

            return sb.ToString();
        }

        public static string GetStringFromObjectListQuoted<T>(List<T> list, string quoteChar)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                object item = list[index];
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(',');
                sb.Append(quoteChar + EncodeDelimiter(item.ToString(), ",") + quoteChar);
            }

            return sb.ToString();
        }

        public static string GetDelimitedStringFromObjectList<T>(List<T> list, string beginDelimiter, string endDelimiter)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                object item = list[index];
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(',');
                sb.Append(beginDelimiter + EncodeDelimiter(item.ToString(), ",") + endDelimiter);
            }

            return sb.ToString();
        }

        public static string GetDelimitedStringFromObjectList<T>(List<T> list, string delimiter)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                object item = list[index];
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(delimiter);
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }

        public static string GetUIStringFromObjectList<T>(List<T> list, LanguageUtilities languageUtilities)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                object item = list[index];
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(',');
                string itemString = item.ToString();
                itemString = languageUtilities.TranslateUIString(itemString);
                sb.Append(itemString);
            }

            return sb.ToString();
        }

        public static List<T> GetObjectListFromString<T>(string str)
        {
            List<T> list = new List<T>();
            if (String.IsNullOrEmpty(str))
                return list;
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add((T)ObjectUtilities.GetKeyFromString(DecodeDelimiter(part.Trim(), ","), null));
            return list;
        }

        public static string GetStringFromStringList(List<string> list)
        {
            return GetStringFromObjectList<string>(list);
        }

        public static string GetStringFromStringListDelimited(List<string> list, string delimiter)
        {
            return GetStringFromObjectListDelimited<string>(list, delimiter);
        }

        public static string GetStringFromStringHashSet(HashSet<string> hashSet)
        {
            return GetStringFromStringHashSetDelimited(hashSet, ",");
        }

        public static string GetStringFromStringHashSetDelimited(HashSet<string> hashSet, string delimiter)
        {
            if ((hashSet == null) || (hashSet.Count() == 0))
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            int index = 0;
            int count = hashSet.Count();

            if (delimiter == null)
                delimiter = ",";

            foreach (string item in hashSet)
            {
                if (item == null)
                    continue;
                if (index != 0)
                    sb.Append(delimiter);
                sb.Append(EncodeDelimiter(item, delimiter));
                index++;
            }

            return sb.ToString();
        }

        public static string GetStringFromStringListQuoted(List<string> list, string quoteChar)
        {
            return GetStringFromObjectListQuoted<string>(list, quoteChar);
        }

        public static string GetDelimitedStringFromStringList(List<string> list, string beginDelimiter, string endDelimiter)
        {
            return GetDelimitedStringFromObjectList<string>(list, beginDelimiter, endDelimiter);
        }

        public static string GetDelimitedStringFromStringList(List<string> list, string delimiter)
        {
            return GetDelimitedStringFromObjectList<string>(list, delimiter);
        }

        public static string GetUIStringFromStringList(List<string> list, LanguageUtilities languageUtilities)
        {
            return GetUIStringFromObjectList<string>(list, languageUtilities);
        }

        public static List<string> GetStringListFromString(string str)
        {
            List<string> list = new List<string>();
            if (String.IsNullOrEmpty(str))
                return list;
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.None);
            foreach (string part in parts)
                list.Add(DecodeDelimiter(part.Trim(), ","));
            return list;
        }

        public static List<string> GetStringListFromStringNoTrim(string str)
        {
            List<string> list = new List<string>();
            if (String.IsNullOrEmpty(str))
                return list;
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.None);
            foreach (string part in parts)
                list.Add(DecodeDelimiter(part, ","));
            return list;
        }

        public static List<string> GetStringListFromStringDelimited(string str, string delimiter)
        {
            List<string> list = new List<string>();
            if (String.IsNullOrEmpty(str))
                return list;
            string[] parts = str.Split(delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(DecodeDelimiter(part.Trim(), delimiter));
            return list;
        }

        public static HashSet<string> GetStringHashSetFromString(string str)
        {
            HashSet<string> hashSet = new HashSet<string>();
            if (String.IsNullOrEmpty(str))
                return hashSet;
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.None);
            foreach (string part in parts)
                hashSet.Add(DecodeDelimiter(part.Trim(), ","));
            return hashSet;
        }

        public static List<string> GetStringListFromLinesString(string str)
        {
            List<string> list = new List<string>();
            if (String.IsNullOrEmpty(str))
                return list;
            string[] parts = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(DecodeDelimiter(part.Trim(), ","));
            return list;
        }

        public static List<string> GetArgumentListFromString(string str)
        {
            List<string> list = new List<string>();
            if (String.IsNullOrEmpty(str))
                return list;
            int count = str.Length;
            int index;
            int parenLevel = 0;
            int startIndex = 0;
            for (index = 0; index < count; index++)
            {
                char c = str[index];
                switch (c)
                {
                    case '(':
                        parenLevel++;
                        break;
                    case ')':
                        if (parenLevel == 0)
                            throw new Exception("GetArgumentListFromString: Bad paren nesting.");
                        parenLevel--;
                        break;
                    case ',':
                        if (parenLevel == 0)
                        {
                            string term = str.Substring(startIndex, index - startIndex);
                            list.Add(term);
                            startIndex = index + 1;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (index > startIndex)
            {
                string term = str.Substring(startIndex, index - startIndex);
                list.Add(term);
            }
            return list;
        }

        public static string GetStringFromIntList(List<int> list)
        {
            return GetStringFromObjectList<int>(list);
        }

        public static List<int> GetIntListFromString(string str)
        {
            List<int> list = new List<int>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(Convert.ToInt32(part.Trim()));
            return list;
        }

        public static string GetStringFromFloatList(List<float> list)
        {
            return GetStringFromObjectList<float>(list);
        }

        public static List<float> GetFloatListFromString(string str)
        {
            List<float> list = new List<float>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(Convert.ToSingle(part.Trim()));
            return list;
        }

        public static string GetStringFromDoubleList(List<double> list)
        {
            return GetStringFromObjectList<double>(list);
        }

        public static List<double> GetDoubleListFromString(string str)
        {
            List<double> list = new List<double>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(Convert.ToSingle(part.Trim()));
            return list;
        }

        public static string GetStringFromFlagList(List<bool> list)
        {
            return GetStringFromObjectList<bool>(list);
        }

        public static List<bool> GetFlagListFromString(string str)
        {
            List<bool> list = new List<bool>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(Convert.ToBoolean(part.Trim()));
            return list;
        }

        public static string GetStringFromLanguageIDList(List<LanguageID> list)
        {
            return GetStringFromObjectList<LanguageID>(list);
        }

        public static List<LanguageID> GetLanguageIDListFromString(string str)
        {
            List<LanguageID> list = new List<LanguageID>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(LanguageLookup.GetLanguageIDNoAdd(part.Trim()));
            return list;
        }

        public static string GetStringFromTimeSpanList(List<TimeSpan> list)
        {
            return GetStringFromObjectList<TimeSpan>(list);
        }

        public static List<TimeSpan> GetTimeSpanListFromString(string str)
        {
            List<TimeSpan> list = new List<TimeSpan>();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                list.Add(TimeSpan.Parse(part.Trim()));
            return list;
        }

        public static Dictionary<string, bool> GetFlagDictionaryFromString(string str, Dictionary<string, bool> oldValue)
        {
            Dictionary<string, bool> dictionary = oldValue;

            if (dictionary == null)
                dictionary = new Dictionary<string, bool>();

            List<string> strList = GetStringListFromString(str);
            int count = strList.Count();
            int index;
            string flagName;
            string flagValueString;
            bool flagValue;

            for (index = 0; index < count; index += 2)
            {
                flagName = strList[index];
                flagValueString = strList[index + 1];
                flagValue = (flagValueString.ToLower() == "true" ? true : false);
                dictionary[flagName] = flagValue;
            }

            return dictionary;
        }

        public static string GetStringFromFlagDictionary(Dictionary<string, bool> value)
        {
            if (value == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool notFirst = false;

            foreach (KeyValuePair<string, bool> kvp in value)
            {
                if (notFirst)
                    sb.Append(",");
                else
                    notFirst = true;

                sb.Append(kvp.Key);
                sb.Append(",");
                sb.Append(kvp.Value.ToString());
            }

            return sb.ToString();
        }

        public static Dictionary<int, bool> GetIntFlagDictionaryFromString(string str, Dictionary<int, bool> oldValue)
        {
            Dictionary<int, bool> dictionary = oldValue;

            if (dictionary == null)
                dictionary = new Dictionary<int, bool>();

            List<string> strList = GetStringListFromString(str);
            int count = strList.Count();
            int index;
            string flagName;
            string flagValueString;
            int flagInt;
            bool flagValue;

            for (index = 0; index < count; index += 2)
            {
                flagName = strList[index];
                flagInt = ObjectUtilities.GetIntegerFromString(flagName, -1);
                if (flagInt < 0)
                    continue;
                flagValueString = strList[index + 1];
                flagValue = (flagValueString.ToLower() == "true" ? true : false);
                dictionary[flagInt] = flagValue;
            }

            return dictionary;
        }

        public static string GetStringFromIntFlagDictionary(Dictionary<int, bool> value)
        {
            if (value == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool notFirst = false;

            foreach (KeyValuePair<int, bool> kvp in value)
            {
                if (notFirst)
                    sb.Append(",");
                else
                    notFirst = true;

                sb.Append(kvp.Key);
                sb.Append(",");
                sb.Append(kvp.Value.ToString());
            }

            return sb.ToString();
        }

        public static Dictionary<string, string> GetStringDictionaryFromString(string str, Dictionary<string, string> oldValue)
        {
            Dictionary<string, string> dictionary = oldValue;

            if (dictionary == null)
                dictionary = new Dictionary<string, string>();

            List<string> strList = GetStringListFromString(str);
            int count = strList.Count();
            int index;
            string stringName;
            string stringValue;

            for (index = 0; index < count; index += 2)
            {
                stringName = strList[index];
                stringValue = strList[index + 1];
                dictionary[stringName] = stringValue;
            }

            return dictionary;
        }

        public static string GetStringFromStringDictionary(Dictionary<string, string> value)
        {
            if (value == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool notFirst = false;

            foreach (KeyValuePair<string, string> kvp in value)
            {
                if (notFirst)
                    sb.Append(",");
                else
                    notFirst = true;

                sb.Append(kvp.Key);
                sb.Append(",");
                sb.Append(kvp.Value);
            }

            return sb.ToString();
        }

        public static int FindBytePattern(byte[] input, byte[] pattern)
        {
            if ((input == null) || (pattern == null))
                return -1;
            int patternLength = pattern.Length;
            int inputLength = input.Length - patternLength;
            if (inputLength < 0)
                return -1;
            int j;
            int offset = -1;
            for (int i = 0; i < inputLength;)
            {
                for (j = 0; j < patternLength; j++)
                {
                    if (pattern[j] != input[i])
                        break;
                    i++;
                }
                if (j == patternLength)
                {
                    offset = i - patternLength;
                    break;
                }
                if (j != 0)
                    continue;
                i++;
            }
            return offset;
        }

        public static int FindOffsetBytePattern(byte[] input, byte[] pattern, int startOffset, int endOffset)
        {
            if ((input == null) || (pattern == null))
                return -1;
            int patternLength = pattern.Length;
            int inputLength = endOffset - patternLength;
            if (inputLength < 0)
                return -1;
            int j;
            int offset = -1;
            for (int i = startOffset; i < inputLength;)
            {
                for (j = 0; j < patternLength; j++)
                {
                    if (pattern[j] != input[i])
                        break;
                    i++;
                }
                if (j == patternLength)
                {
                    offset = i - patternLength;
                    break;
                }
                if (j != 0)
                    continue;
                i++;
            }
            return offset;
        }

        public static int FindStringInBytes(byte[] input, string pattern)
        {
            if ((input == null) || (pattern == null) || (pattern.Length == 0))
                return -1;

            byte[] patternBytes = GetBytesFromString(pattern);
            int offset = FindBytePattern(input, patternBytes);

            return offset;
        }

        public static int FindStringInBytesOffset(byte[] input, string pattern, int startOffset, int endOffset)
        {
            if ((input == null) || (pattern == null) || (pattern.Length == 0))
                return -1;

            byte[] patternBytes = GetBytesFromString(pattern);
            int offset = FindOffsetBytePattern(input, patternBytes, startOffset, endOffset);

            return offset;
        }

        public static string GetStringFromBytes(byte[] bytes, int index, int count)
        {
            string str = ApplicationData.Encoding.GetString(bytes, index, count);
            return str;
        }

        public static byte[] GetBytesFromString(string str)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(str);
            return data;
        }

        public static ushort[] GetUShortsFromString(string str)
        {
            ushort[] temp = new ushort[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                temp[i] = (ushort)str[i];
            }

            return temp;
        }

        public static string GetGenericNormalizedPunctuationString(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace('‘', '\'');
                str = str.Replace('’', '\'');
                str = str.Replace('“', '"');
                str = str.Replace('”', '"');
                str = str.Replace('。', '.');
                str = str.Replace('．', '.');
                str = str.Replace('，', ',');
                str = str.Replace(';', ';');
                str = str.Replace('；', ';');
                str = str.Replace(':', ':');
                str = str.Replace('：', ':');
                str = str.Replace("¿", "");
                str = str.Replace('？', '?');
                str = str.Replace("¡", "");
                str = str.Replace('！', '!');
                str = str.Replace('「', '"');
                str = str.Replace('」', '"');
                str = str.Replace('『', '"');
                str = str.Replace('』', '"');
                str = str.Replace('《', '"');
                str = str.Replace('》', '"');
                str = str.Replace("…", "...");
                str = str.Replace('－', '-');
                str = str.Replace('—', '-');
                str = str.Replace('、', ',');
                str = str.Replace('・', ' ');
                str = str.Replace('·', ' ');
                str = str.Replace('―', '-');

                // Remove bracketed corrections.
                str = str.Replace("[", "");
                str = str.Replace("]", "");
            }

            return str;
        }

        public static string GetGenericNormalizedString(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace('\x00A0', ' ');
            }

            return str;
        }

        public static string GetNormalizedString(string str)
        {
            return JTLanguageModelsPortable.Application.ApplicationData.Global.GetNormalizedString(str);
        }

        public static List<string> GetNormalizedStrings(List<string> strs)
        {
            if (strs == null)
                return null;

            List<string> newStrs = new List<string>(strs.Count());

            foreach (string str in strs)
                newStrs.Add(JTLanguageModelsPortable.Application.ApplicationData.Global.GetNormalizedString(str));

            return newStrs;
        }

        public static string[] GetNormalizedLowerWordArray(string text)
        {
            string[] parts = text.Split(LanguageLookup.Space);
            int c = parts.Length;
            int i;

            for (i = 0; i < c; i++)
                parts[i] = GetNormalizedString(parts[i].ToLower());

            return parts;
        }

        // From: https://www.eximiaco.tech/en/2019/11/17/computing-the-levenshtein-edit-distance-of-two-strings-using-c/
        public static int ComputeDistance(
            string first,
            string second)
        {
            if (first.Length == 0)
                return second.Length;

            if (second.Length == 0)
                return first.Length;

            var current = 1;
            var previous = 0;
            var r = new int[2, second.Length + 1];

            for (var i = 0; i <= second.Length; i++)
                r[previous, i] = i;

            for (var i = 0; i < first.Length; i++)
            {
                r[current, 0] = i + 1;

                for (var j = 1; j <= second.Length; j++)
                {
                    var cost = (second[j - 1] == first[i]) ? 0 : 1;

                    r[current, j] = Min(
                        r[previous, j] + 1,
                        r[current, j - 1] + 1,
                        r[previous, j - 1] + cost);
                }

                previous = (previous + 1) % 2;
                current = (current + 1) % 2;
            }

            return r[previous, second.Length];
        }

        // Figure out a single string-replace to get target from source.
        // Returns true if successful.
        public static bool ComputeSimpleReplacement(
            string source,
            string target,
            out string pattern,
            out string replacement)
        {
            pattern = null;
            replacement = null;

            if (String.IsNullOrEmpty(source))
                return false;

            if (String.IsNullOrEmpty(target))
                return false;

            if (source == target)
                return false;

            int sourceLength = source.Length;
            int targetLength = target.Length;
            int minLength = Math.Min(sourceLength, targetLength);
            int diffStartIndex;
            int diffEndIndex;

            // Find index of first different character.
            for (diffStartIndex = 0; diffStartIndex < minLength; diffStartIndex++)
            {
                if (source[diffStartIndex] != target[diffStartIndex])
                    break;
            }

            // Find index of first different character.
            for (diffEndIndex = 0; diffEndIndex < minLength; diffEndIndex++)
            {
                if (source[sourceLength - diffEndIndex - 1] != target[targetLength - diffEndIndex - 1])
                    break;
            }

            if ((diffStartIndex == 0) && (diffEndIndex == 0))
                return false;

            int sourceDiffEndIndex = sourceLength - diffEndIndex;
            int patternLength = sourceDiffEndIndex - diffStartIndex;

            if (patternLength <= 0)
                return false;

            pattern = source.Substring(diffStartIndex, patternLength);

            int targetDiffEndIndex = targetLength - diffEndIndex;
            int replacementLength = targetDiffEndIndex - diffStartIndex;

            replacement = target.Substring(diffStartIndex, replacementLength);

            int count = CountSubStrings(source, pattern);

            if (count > 1)
                return false;

            return true;
        }

        private static int Min(int e1, int e2, int e3) =>
            Math.Min(Math.Min(e1, e2), e3);

        public static string GetHexStringFromString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return "";

            StringBuilder sb = new StringBuilder();

            foreach (char c in str)
            {
                sb.Append(String.Format("{0:x2}", (int)c));
            }

            return sb.ToString();
        }

        public static bool IsHex(char c)
        {
            if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')) || ((c >= 'a') && (c <= 'f')))
                return true;
            return false;
        }

        public static int GetHexNibble(char c)
        {
            if ((c >= '0') && (c <= '9'))
                return c - '0';
            else if ((c >= 'A') && (c <= 'F'))
                return (c - 'A') + 10;
            else if ((c >= 'a') && (c <= 'f'))
                return (c - 'a') + 10;
            return 0;
        }

        public static byte GetHexByte(char c1, char c0)
        {
            return (byte)((GetHexNibble(c1) << 4) + GetHexNibble(c0));
        }

        public static int GetDecimalNibble(char c)
        {
            if ((c >= '0') && (c <= '9'))
                return c - '0';
            return 0;
        }

        public static string FirstWord(string str)
        {
            string[] parts = str.Split(LanguageLookup.SpaceAndPunctuationCharacters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() == 0)
                return str;
            return parts[0];
        }

        public static int GetSubstringCount(string str, string substring)
        {
            int count = 0, minIndex = str.IndexOf(substring, 0);
            while (minIndex != -1)
            {
                minIndex = str.IndexOf(substring, minIndex + substring.Length);
                count++;
            }
            return count;
        }

        public static bool GetHttpStringField(string name, byte[] data, out string value)
        {
            byte[] bytes;
            bool returnValue = GetHttpByteField(name, data, out bytes);

            if (returnValue)
                value = TextUtilities.GetStringFromBytes(bytes, 0, bytes.Length);
            else
                value = null;

            return returnValue;
        }

        public static bool GetHttpFileField(string name, byte[] data, out Stream fileStream, out string mimeType)
        {
            byte[] bytes;
            bool returnValue = GetHttpByteFieldWithType(name, data, out bytes, out mimeType);

            if (returnValue)
                fileStream = new MemoryStream(bytes);
            else
                fileStream = null;

            return returnValue;
        }

        public static bool GetHttpByteField(string name, byte[] data, out byte[] bytes)
        {
            int length = data.Length;
            string namePattern = "name=\"" + name + "\"";
            string boundary = "-----------------------------";
            int nameOffset = TextUtilities.FindStringInBytes(data, namePattern);
            bytes = null;
            if (nameOffset != -1)
            {
                int endOffset = TextUtilities.FindStringInBytesOffset(data, boundary, nameOffset, length);
                if (endOffset != -1)
                {
                    if ((endOffset > nameOffset + 2) && (data[endOffset - 2] == '\r') && (data[endOffset - 1] == '\n'))
                        endOffset -= 2;
                    string crLfCrLfPattern = "\r\n\r\n";
                    int crLfCrLfOffset = TextUtilities.FindStringInBytesOffset(data, crLfCrLfPattern, nameOffset, endOffset);
                    if (crLfCrLfOffset != -1)
                    {
                        int contentOffset = crLfCrLfOffset + crLfCrLfPattern.Length;
                        int bytesLength = endOffset - contentOffset;
                        bytes = new byte[bytesLength];
                        Array.Copy(data, contentOffset, bytes, 0, bytesLength);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool GetHttpByteFieldWithType(string name, byte[] data, out byte[] bytes, out string mimeType)
        {
            int length = data.Length;
            string namePattern = "name=\"" + name + "\"";
            string boundary = "-----------------------------";
            int nameOffset = TextUtilities.FindStringInBytes(data, namePattern);
            mimeType = null;
            bytes = null;
            if (nameOffset != -1)
            {
                int endOffset = TextUtilities.FindStringInBytesOffset(data, boundary, nameOffset, length);
                if (endOffset != -1)
                {
                    if ((endOffset > nameOffset + 2) && (data[endOffset - 2] == '\r') && (data[endOffset - 1] == '\n'))
                        endOffset -= 2;
                    string crLfCrLfPattern = "\r\n\r\n";
                    int crLfCrLfOffset = TextUtilities.FindStringInBytesOffset(data, crLfCrLfPattern, nameOffset, endOffset);
                    if (crLfCrLfOffset != -1)
                    {
                        int contentOffset = crLfCrLfOffset + crLfCrLfPattern.Length;
                        string contentTypePattern = "Content-Type: ";
                        int contentTypeOffset = TextUtilities.FindStringInBytesOffset(data, contentTypePattern, nameOffset, crLfCrLfOffset);
                        if (contentTypeOffset != -1)
                        {
                            int mimeTypeOffset = contentTypeOffset + contentTypePattern.Length;
                            mimeType = TextUtilities.GetStringFromBytes(data, mimeTypeOffset, crLfCrLfOffset - mimeTypeOffset);
                        }
                        int bytesLength = endOffset - contentOffset;
                        bytes = new byte[bytesLength];
                        Array.Copy(data, contentOffset, bytes, 0, bytesLength);
                        return true;
                    }
                }
            }
            return false;
        }

        public static byte[] Encrypt(byte[] data)
        {
            if (data == null)
                data = new byte[0];
            return data;
            //RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            //provider.ImportParameters(provider.ExportParameters(true));
            //return provider.Encrypt(data, false);
        }

        public static byte[] Decrypt(byte[] data)
        {
            if (data == null)
                data = new byte[0];
            return data;
            //RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            //provider.ImportParameters(provider.ExportParameters(true));
            //return provider.Decrypt(data, false);
        }

        public static string GetNibble(int number, int digit)
        {
            int shift = digit * 4;
            int value = (number >> shift) & 0x0f;
            return value.ToString();
        }

        public static string GetByteNibble(int number, int digit)
        {
            int shift = digit * 8;
            int value = (number >> shift) & 0x0ff;
            return value.ToString("X2");
        }

        public static string UrlEncode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace("|", "%7C");
                str = str.Replace("\"", "%22");
                str = str.Replace("<", "%3C");
                str = str.Replace(">", "%3E");
                str = str.Replace("(", "%28");
                str = str.Replace(")", "%29");
                //str = str.Replace("%", "%25");
                str = str.Replace("&", "%26");
                str = str.Replace("/", "%2F");
                str = str.Replace("=", "%3D");
                str = str.Replace("?", "%3F");
                str = str.Replace(":", "%3A");
                str = str.Replace("'", "%27");
                str = str.Replace(" ", "%20");
                str = str.Replace("\t", "%09");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
                str = str.Replace("ā", "%u0101");
                str = str.Replace("ō", "%u014D");
                str = str.Replace("ū", "%u016B");
                str = str.Replace("ē", "%u0113");
                str = str.Replace("ī", "%u012B");
                str = str.Replace("Ā", "%u0100");
                str = str.Replace("Ō", "%u014C");
                str = str.Replace("Ū", "%u016A");
                str = str.Replace("Ē", "%u0112");
                str = str.Replace("Ī", "%u012A");
            }

            return str;
        }

        public static string UrlDecode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("%7C", "|");
                    str = str.Replace("%22", "\"");
                    str = str.Replace("%3C", "<");
                    str = str.Replace("%3E", ">");
                    str = str.Replace("%28", "(");
                    str = str.Replace("%29", ")");
                    str = str.Replace("%25", "%");
                    str = str.Replace("%26", "&");
                    str = str.Replace("%2F", "/");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%3F", "?");
                    str = str.Replace("%3A", ":");
                    str = str.Replace("%27", "'");
                    str = str.Replace("%20", " ");
                    str = str.Replace("%09", "\t");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                    str = str.Replace("%u0101", "ā");
                    str = str.Replace("%u014D", "ō");
                    str = str.Replace("%u016B", "ū");
                    str = str.Replace("%u0113", "ē");
                    str = str.Replace("%u012B", "ī");
                    str = str.Replace("%u0100", "Ā");
                    str = str.Replace("%u014C", "Ō");
                    str = str.Replace("%u016A", "Ū");
                    str = str.Replace("%u0112", "Ē");
                    str = str.Replace("%u012A", "Ī");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string OptionEncode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace("%", "%25");
                str = str.Replace("|", "%7C");
                str = str.Replace("\"", "%22");
                str = str.Replace("<", "%3C");
                str = str.Replace(">", "%3E");
                str = str.Replace("'", "%27");
                str = str.Replace(".", "%2E");
                str = str.Replace(",", "%2C");
                str = str.Replace("=", "%3D");
                str = str.Replace(" ", "%20");
                str = str.Replace("\t", "%09");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static string OptionDecode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("%7C", "|");
                    str = str.Replace("%22", "\"");
                    str = str.Replace("%3C", "<");
                    str = str.Replace("%3E", ">");
                    str = str.Replace("%26", "&");
                    str = str.Replace("%2F", "/");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%3F", "?");
                    str = str.Replace("%27", "'");
                    str = str.Replace("%2E", ".");
                    str = str.Replace("%2C", ",");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%20", " ");
                    str = str.Replace("%09", "\t");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                    str = str.Replace("%25", "%");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string HtmlEncode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace("<", "&lt;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("\"", "&quot;");
                str = str.Replace("&", "&amp;");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static string HtmlDecode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("&lt;", "<");
                    str = str.Replace("&gt;", ">");
                    str = str.Replace("&quot;", "\"");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }

        /* See new definition in TextUtilitiesEntities.cs.
        public static string EntityEncode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace("<", "&lt;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("\"", "&quot;");
                str = str.Replace("&", "&amp;");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static string EntityDecode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("&lt;", "<");
                    str = str.Replace("&gt;", ">");
                    str = str.Replace("&quot;", "\"");
                    str = str.Replace("&amp;", "&");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }
        */

        public static string EncodeUTF8(string str)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(str);
            StringBuilder sb = new StringBuilder();

            foreach (byte b in data)
            {
                if (b > 0x7f)
                {
                    sb.Append('%');
                    sb.Append(String.Format("{0:x2}", (int)b));
                }
                else
                    sb.Append((char)b);
            }

            return sb.ToString();
        }

        public static string DecodeUTF8(string str)
        {
            List<byte> data = new List<byte>();
            StringBuilder sb = new StringBuilder();
            int length = str.Length;
            int index;

            for (index = 0; index < length; index++)
            {
                char c = str[index];

                if ((c == '%') && (index <= length - 3) && IsHex(str[index + 1]) && IsHex(str[index + 2]))
                {
                    data.Add(GetHexByte(str[index + 1], str[index + 2]));
                    index += 2;
                }
                else
                    data.Add((byte)c);
            }

            return ApplicationData.Encoding.GetString(data.ToArray(), 0, data.Count());
        }

        public static string JavascriptEncode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace("'", "\\'");
                str = str.Replace("\\\\'", "\\'");
                str = str.Replace("\"", "\\\"");
                str = str.Replace("\r\n", "\\n");
                str = str.Replace("\n", "\\n");
            }

            return str;
        }

        public static string MobileTextAreaDecode(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("%09", "\t");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static int CountSubStrings(string s, string substring, bool aggressiveSearch = false)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return 0;

            // if the length of substring is greater than the length of s,
            // substring cannot be found in s
            if (substring.Length > s.Length)
                return 0;

            int count = 0, n = 0;
            while ((n = s.IndexOf(substring, n, StringComparison.Ordinal)) != -1)
            {
                if (aggressiveSearch)
                    n++;
                else
                    n += substring.Length;
                count++;
            }

            return count;
        }

        public static int CountSubStringsIgnoreCase(string s, string substring, bool aggressiveSearch = false)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return 0;

            // if the length of substring is greater than the length of s,
            // substring cannot be found in s
            if (substring.Length > s.Length)
                return 0;

            int count = 0, n = 0;
            while ((n = s.IndexOf(substring, n, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                if (aggressiveSearch)
                    n++;
                else
                    n += substring.Length;
                count++;
            }

            return count;
        }

        // Works only for spaced languages.
        public static int GetWordCount(string str)
        {
            int count = CountChars(str.Trim(), ' ');
            return count + 1;
        }

        public static int CountWordsOrPhrases(string s, string substring, bool aggressiveSearch = false)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return 0;

            int sEnd = s.Length;
            int subLength = substring.Length;

            // if the length of substring is greater than the length of s,
            // substring cannot be found in s
            if (subLength > sEnd)
                return 0;

            int count = 0, n = 0;

            while ((n = s.IndexOf(substring, n, StringComparison.Ordinal)) != -1)
            {
                if ((n == 0) || char.IsWhiteSpace(s[n - 1]) || char.IsPunctuation(s[n - 1]))
                {
                    int cEnd = n + subLength;

                    if ((cEnd == sEnd) || char.IsWhiteSpace(s[cEnd]) || char.IsPunctuation(s[cEnd]))
                        count++;
                }

                if (aggressiveSearch)
                    n++;
                else
                    n += substring.Length;
            }

            return count;
        }

        public static int CountWordsOrPhrasesIgnoreCase(string s, string substring, bool aggressiveSearch = false)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return 0;

            int sEnd = s.Length;
            int subLength = substring.Length;

            // if the length of substring is greater than the length of s,
            // substring cannot be found in s
            if (subLength > sEnd)
                return 0;

            int count = 0, n = 0;
            while ((n = s.IndexOf(substring, n, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                if ((n == 0) || char.IsWhiteSpace(s[n - 1]) || char.IsPunctuation(s[n - 1]))
                {
                    int cEnd = n + subLength;

                    if ((cEnd == sEnd) || char.IsWhiteSpace(s[cEnd]) || char.IsPunctuation(s[cEnd]))
                        count++;
                }

                if (aggressiveSearch)
                    n++;
                else
                    n += substring.Length;
            }

            return count;
        }

        public static string ReplaceWordsOrPhrases(string s, string oldWordOrPhrase, string newWordOrPhrase)
        {
            // if s or oldWordOrPhrase is null or empty, oldWordOrPhrase cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(oldWordOrPhrase))
                return s;

            int sEnd = s.Length;
            int oldWordOrPhraseLength = oldWordOrPhrase.Length;
            int newWordOrPhraseLength = newWordOrPhrase.Length;

            // if the length of oldWordOrPhrase is greater than the length of s,
            // oldWordOrPhrase cannot be found in s
            if (oldWordOrPhraseLength > sEnd)
                return s;

            int n = 0;

            while ((n = s.IndexOf(oldWordOrPhrase, n, StringComparison.Ordinal)) != -1)
            {
                if ((n == 0) || char.IsWhiteSpace(s[n - 1]) || char.IsPunctuation(s[n - 1]))
                {
                    int cEnd = n + oldWordOrPhraseLength;

                    if ((cEnd == sEnd) || char.IsWhiteSpace(s[cEnd]) || char.IsPunctuation(s[cEnd]))
                    {
                        s = s.Remove(n, oldWordOrPhraseLength);
                        s = s.Insert(n, newWordOrPhrase);
                    }
                }

                n += newWordOrPhraseLength;
            }

            return s;
        }

        public static string ReplaceWordsOrPhrasesIgnoreCase(string s, string oldWordOrPhrase, string newWordOrPhrase)
        {
            // if s or oldWordOrPhrase is null or empty, oldWordOrPhrase cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(oldWordOrPhrase))
                return s;

            int sEnd = s.Length;
            int oldWordOrPhraseLength = oldWordOrPhrase.Length;
            int newWordOrPhraseLength = newWordOrPhrase.Length;

            // if the length of oldWordOrPhrase is greater than the length of s,
            // oldWordOrPhrase cannot be found in s
            if (oldWordOrPhraseLength > sEnd)
                return s;

            int n = 0;

            while ((n = s.IndexOf(oldWordOrPhrase, n, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                if ((n == 0) || char.IsWhiteSpace(s[n - 1]) || char.IsPunctuation(s[n - 1]))
                {
                    int cEnd = n + oldWordOrPhraseLength;

                    if ((cEnd == sEnd) || char.IsWhiteSpace(s[cEnd]) || char.IsPunctuation(s[cEnd]))
                    {
                        s = s.Remove(n, oldWordOrPhraseLength);
                        s = s.Insert(n, newWordOrPhrase);
                    }
                }

                n += newWordOrPhraseLength;
            }

            return s;
        }

        public static string ReplaceSubStrings(string str, string substring, string replacement)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
                return str;

            string result = str.Replace(substring, replacement);

            return result;
        }

        public static string ReplaceSubStringsIgnoreCase(string str, string substring, string replacement)
        {
            // if s or substring is null or empty, substring cannot be found in s
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
                return str;

            string result = Regex.Replace(
                str,
                Regex.Escape(substring),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );

            return result;
        }

        // Replace substrings from table, where table entries is (before), (after).
        public static string ReplaceSubStrings(string text, string[] table)
        {
            int count = table.Length;
            int index;
            string returnValue = text;

            for (index = 0; index < count; index += 2)
            {
                string pattern = table[index];
                string replacement = table[index + 1];
                returnValue = returnValue.Replace(pattern, replacement);
            }

            return returnValue;
        }

        // Replace substrings from table, where table entries is (before), (after).
        public static string ReplaceSubStringsIgnoreCase(string text, string[] table)
        {
            int count = table.Length;
            int index;
            string returnValue = text;

            for (index = 0; index < count; index += 2)
            {
                string pattern = table[index];
                string replacement = table[index + 1];
                returnValue = ReplaceSubStringsIgnoreCase(returnValue, pattern, replacement);
            }

            return returnValue;
        }

        // Replace strings from table, where table entries is (before), (after).
        public static string ReplaceStrings(string text, string[] table)
        {
            int count = table.Length;
            int index;

            for (index = 0; index < count; index += 2)
            {
                if (text == table[index])
                    return table[index + 1];
            }

            return text;
        }

        // Replace string endings from table, where table entries is (before), (after).
        public static string ReplaceStringEndings(string text, string[] table)
        {
            int count = table.Length;
            int index;

            for (index = 0; index < count; index += 2)
            {
                string pattern = table[index];

                if (text.EndsWith(pattern))
                {
                    text = text.Remove(text.Length - pattern.Length) + table[index + 1];
                    return text;
                }
            }

            return text;
        }

        // Remove a list of strings from a string.
        public static string RemoveStrings(string text, string[] removeThese)
        {
            foreach (string str in removeThese)
                text = text.Replace(str, String.Empty);

            return text;
        }

        // Remove a delimited strings from a string.
        public static string RemoveDelimitedStrings(string text, string startDelimiter, string stopDelimiter)
        {
            for (;;)
            {
                int ofs = text.IndexOf(startDelimiter);

                if (ofs == -1)
                    break;

                int endOfs = text.IndexOf(stopDelimiter, ofs);

                if (endOfs == -1)
                    break;

                text = text.Remove(ofs, (endOfs - ofs) + 1);
            }

            return text;
        }

        public static List<string> OperateOnStringList(List<string> stringList, Func<string, string> stringMethod)
        {
            if (stringList == null)
                return null;

            if (stringMethod == null)
                return stringList;

            List<string> newStringList = new List<string>(stringList.Count());

            foreach (string str in stringList)
            {
                string newString = stringMethod(str);
                newStringList.Add(newString);
            }

            return newStringList;
        }

        public static string GetMediaUrl(string serviceEndpointPrefix, string userName, string tildePath)
        {
            string url;

            if (tildePath.StartsWith("~"))
                url = serviceEndpointPrefix + tildePath.Substring(1);
            else if (tildePath.StartsWith("http://") || tildePath.StartsWith("https://"))
                url = tildePath;
            else
                url = serviceEndpointPrefix + "/Content/Media/" + userName + "/" + tildePath;

            return url;
        }

        public static string UpdateMessageInReturnUrl(string returnUrl, string message)
        {
            return UpdateFieldInUrl(returnUrl, "message", message);
        }

        public static string RemoveMessageInReturnUrl(string returnUrl)
        {
            return RemoveFieldInUrl(returnUrl, "message");
        }

        public static string UpdateFieldInUrl(string returnUrl, string fieldName, string fieldValue)
        {
            if (returnUrl != null)
            {
                int offset1 = returnUrl.IndexOf("&" + fieldName + "=");

                if (offset1 == -1)
                    offset1 = returnUrl.IndexOf("?" + fieldName + "=");

                if (offset1 != -1)
                {
                    int offset2 = returnUrl.IndexOf("&", offset1 + fieldName.Length + 2);

                    if (offset2 == -1)
                        offset2 = returnUrl.Length;

                    int length = offset2 - offset1;

                    returnUrl = returnUrl.Remove(offset1, length);
                }

                if (!String.IsNullOrEmpty(fieldValue))
                {
                    if (offset1 == -1)
                        offset1 = returnUrl.Length;

                    if (returnUrl.Contains("?"))
                        returnUrl = returnUrl.Insert(offset1, "&" + fieldName + "=" + UrlEncode(fieldValue));
                    else
                        returnUrl = returnUrl.Insert(offset1, "?" + fieldName + "=" + UrlEncode(fieldValue));
                }
                else
                {
                    int offset2 = returnUrl.IndexOf("&");
                    if (offset2 != -1)
                    {
                        offset1 = returnUrl.IndexOf("?");
                        if (offset1 == -1)
                        {
                            returnUrl = returnUrl.Remove(offset2, 1);
                            returnUrl = returnUrl.Insert(offset2, "?");
                        }
                    }
                }
            }

            return returnUrl;
        }

        public static string RemoveFieldInUrl(string url, string fieldName)
        {
            if (url != null)
            {
                int offset1 = url.IndexOf("&" + fieldName + "=");

                if (offset1 == -1)
                    offset1 = url.IndexOf("?" + fieldName + "=");

                if (offset1 != -1)
                {
                    int offset2 = url.IndexOf("&", offset1 + fieldName.Length + 2);

                    if (offset2 == -1)
                        offset2 = url.Length;

                    int length = offset2 - offset1;

                    if (url[offset1] == '?')
                        url = url.Remove(offset1 + 1, length - 1);
                    else
                        url = url.Remove(offset1, length);
                }
            }

            return url;
        }

        public static string CollapseStringList(List<string> strs)
        {
            if (strs == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (string str in strs)
                sb.Append(str);

            return sb.ToString();
        }

        public static string ExpandSymbol(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                if (Char.IsUpper(c))
                    sb.Append(" ");

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string CollapseSymbol(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            string output = input.Replace(" ", "");

            return output;
        }

        public static string LettersOnly(string input)
        {
            string output = input;

            foreach (char c in LanguageLookup.NonAlphanumericCharacters)
                output = output.Replace(c.ToString(), "");

            foreach (char c in LanguageLookup.SpaceCharacters)
                output = output.Replace(c.ToString(), "");

            return output;
        }

        // Insert a space before embedded uppercase characters.
        public static string SeparateUpperCase(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            for (int length = str.Length, i = length - 1; i > 0; i--)
            {
                if (char.IsUpper(str[i]))
                    str = str.Insert(i, " ");
            }

            return str;
        }

        // Insert a space before embedded uppercase characters.
        public static string SeparateUpperLowerCase(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            for (int length = str.Length, i = length - 1; i > 0; i--)
            {
                if (char.IsUpper(str[i]))
                {
                    str = str.Insert(i, " ");
                    string c = str.Substring(i + 1, 1).ToLower();
                    str = str.Remove(i + 1, 1);
                    str = str.Insert(i + 1, c);
                }
            }

            return str;
        }

        public static string MakeFirstLetterUpperCase(string str)
        {
            string returnValue = str.Substring(0, 1).ToUpper() + str.Substring(1);
            return returnValue;
        }

        public static string MakeWordsFirstLetterUpperCaseAndCollapse(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            string[] parts = str.Split(LanguageLookup.Space, StringSplitOptions.RemoveEmptyEntries);

            string returnValue = String.Empty;

            foreach (string part in parts)
                returnValue += part.Substring(0, 1).ToUpper() + part.Substring(1);

            return returnValue;
        }

        // Break up file path or URL with HTML line breaks.
        public static string BreakUpLines(int width, string filePath, string breakString)
        {
            if (String.IsNullOrEmpty(filePath))
                return "";

            int length = filePath.Length;

            if (length < width)
                return filePath;

            StringBuilder sb = new StringBuilder();
            int i, j;

            for (i = 0; i < length; )
            {
                bool found = false;

                j = width - 1;

                if (i + j >= length)
                    j = (length - i) - 1;

                for (; j > 5; j--)
                {
                    switch (filePath[i + j])
                    {
                        case '/':
                        case '\\':
                        case '-':
                            found = true;
                            break;
                        default:
                            break;
                    }

                    if (found)
                        break;
                }

                if (!found)
                {
                    j = width;

                    if (i + j > length)
                        j = length - i;
                }
                else
                    j++;

                sb.Append(filePath.Substring(i, j));

                if (i + j < length)
                    sb.Append(breakString);

                i += j;
            }

            return sb.ToString();
        }

        public static List<string> ParseSentencesFromRawText(string input)
        {
            List<string> sentences = new List<string>();
            int length = input.Length;
            int startIndex;
            int index = 0;
            string sentence;
            char matching = '\0';

            while ((index < length) && char.IsWhiteSpace(input[index]))
                index++;

            startIndex = index;

            for (index = 0; index < length;)
            {
                if ((matching != '\0') && (input[index] != matching))
                    index++;
                else if (input[index] == matching)
                {
                    matching = '\0';
                    index++;
                    sentence = input.Substring(startIndex, index - startIndex);
                    sentences.Add(sentence);

                    while ((index < length) && char.IsWhiteSpace(input[index]))
                        index++;

                    startIndex = index;
                }
                else if (input[index] == '{')
                {
                    matching = '}';
                    index++;
                }
                else if ((input[index] == '$') && (input[index + 1] == '('))
                {
                    matching = ')';
                    index++;
                }
                else if (LanguageLookup.SentenceTerminatorCharacters.Contains(input[index]) /*|| LanguageLookup.ColonCharacters.Contains(input[index])*/)
                {
                    index++;

                    while ((index < length) &&
                            (LanguageLookup.SentenceTerminatorCharacters.Contains(input[index]) /*|| LanguageLookup.ColonCharacters.Contains(input[index])*/))
                        index++;

                    sentence = input.Substring(startIndex, index - startIndex);
                    sentences.Add(sentence);

                    while ((index < length) && char.IsWhiteSpace(input[index]))
                        index++;

                    startIndex = index;
                }
                else
                    index++;
            }

            if (index > startIndex)
            {
                sentence = input.Substring(startIndex, index - startIndex);
                sentences.Add(sentence);
            }

            return sentences;
        }

        // Parse string into runs of word characters, white space, or punctuation.
        public static List<string> ParseRuns(string input)
        {
            if (input == null)
                return new List<string>();

            List<string> runs = new List<string>(input.Length);
            char[] whiteSpace = LanguageLookup.SpaceCharacters;
            char[] nonAlphanumericCharacters = LanguageLookup.NonAlphanumericCharacters;
            int startIndex;
            int endIndex = input.Length;
            int runIndex;
            int runLength;
            char c;
            string run;

            for (startIndex = 0; startIndex < endIndex; )
            {
                c = input[startIndex];

                if (whiteSpace.Contains(input[startIndex]))
                {
                    for (runIndex = startIndex + 1; (runIndex < endIndex) && whiteSpace.Contains(input[runIndex]); runIndex++)
                        ;
                }
                else if (nonAlphanumericCharacters.Contains(c))
                {
                    for (runIndex = startIndex + 1; (runIndex < endIndex) && nonAlphanumericCharacters.Contains(input[runIndex]); runIndex++)
                        ;
                }
                else
                {
                    for (runIndex = startIndex + 1;
                            (runIndex < endIndex) &&
                                !whiteSpace.Contains(input[runIndex]) &&
                                (!nonAlphanumericCharacters.Contains(input[runIndex]) ||
                                    ((input[runIndex] == ':') && (input[runIndex - 1] == 'u')));
                            runIndex++)
                        ;
                }

                runLength = runIndex - startIndex;
                run = input.Substring(startIndex, runLength);
                runs.Add(run);

                startIndex = runIndex;
            }

            return runs;
        }

        public static bool ParseString(
            string str,
            char nextPatternChar,
            int startIndex,
            int length,
            out string outString,
            out int endIndex)
        {
            bool returnValue = true;
            int index = startIndex;

            for (; index < length; index++)
            {
                if (str[index] == nextPatternChar)
                    break;
            }

            endIndex = index;
            int size = index - startIndex;

            if (size > 0)
                outString = str.Substring(startIndex, size);
            else
            {
                outString = String.Empty;
                returnValue = false;
            }

            return returnValue;
        }

        public static bool ParseString(
            string str,
            char[] nextPatternChars,
            int startIndex,
            int length,
            out string outString,
            out int endIndex)
        {
            bool returnValue = true;
            int index = startIndex;

            for (; index < length; index++)
            {
                if (nextPatternChars.Contains(str[index]))
                    break;
            }

            endIndex = index;
            int size = index - startIndex;

            if (size > 0)
                outString = str.Substring(startIndex, size);
            else
            {
                outString = String.Empty;
                returnValue = false;
            }

            return returnValue;
        }

        public static bool ParseInteger(ref string str, out int value)
        {
            int index = 0;

            SkipWhiteSpace(ref str);
            int count = str.Length;

            while ((index < count) && char.IsDigit(str[index]))
                index++;

            if (index == 0)
            {
                value = 0;
                return false;
            }

            string integerString = str.Substring(0, index);
            value = ObjectUtilities.GetIntegerFromString(integerString, 0);
            str = str.Substring(index);

            return true;
        }

        public static bool ParseLineNumber(ref string str, out int value)
        {
            int index = 0;

            SkipWhiteSpace(ref str);
            int count = str.Length;

            while ((index < count) && char.IsDigit(str[index]))
                index++;

            if (index == 0)
            {
                value = 0;
                return false;
            }

            string integerString = str.Substring(0, index);
            value = ObjectUtilities.GetIntegerFromString(integerString, 0);

            if (str[index] == '.')
                index++;

            while (str[index] == ' ')
                index++;

            str = str.Substring(index);

            return true;
        }

        public static bool ParseQuotedString(ref string str, out string value)
        {
            int index = 0;

            value = String.Empty;

            SkipWhiteSpace(ref str);

            if (!ParseToken(ref str, "\""))
                return false;

            while ((index < str.Length) && (str[index] != '"'))
                index++;

            value = str.Substring(0, index);

            str = str.Substring(index);

            if (!ParseToken(ref str, "\""))
                return false;

            return true;
        }

        public static bool ParseDelimitedString(ref string str, char[] delimiters, out string value)
        {
            int index = 0;

            value = String.Empty;

            SkipWhiteSpace(ref str);

            while ((index < str.Length) && !delimiters.Contains(str[index]))
                index++;

            value = str.Substring(0, index);

            str = str.Substring(index);

            return true;
        }

        public static bool ParseTimeSpan(ref string str, out TimeSpan value)
        {
            int index = 0;

            SkipWhiteSpace(ref str);

            while ((index < str.Length) && (char.IsDigit(str[index]) || (str[index] == ':') || (str[index] == '.')))
                index++;

            if (index == 0)
            {
                value = TimeSpan.Zero;
                return false;
            }

            string timeSpanString = str.Substring(0, index);
            value = ObjectUtilities.GetTimeSpanFromString(timeSpanString, TimeSpan.Zero);
            str = str.Substring(index);

            return true;
        }

        public static bool ParseToken(ref string str, string token)
        {
            SkipWhiteSpace(ref str);

            string testString = str.Substring(0, token.Length);

            if (testString == token)
            {
                str = str.Substring(token.Length);
                return true;
            }

            return false;
        }

        public static void SkipWhiteSpace(ref string str)
        {
            while (str.Length != 0)
            {
                char chr = str[0];

                if (!char.IsWhiteSpace(chr))
                    return;

                str = str.Substring(1);
            }
        }

        public static string ParseDelimitedField(
            string startDelimiter,
            string endDelimiter,
            string text,
            int nth)
        {
            string returnValue = String.Empty;
            int nextOffset = 0;

            if (String.IsNullOrEmpty(text))
                return returnValue;

            int textLength = text.Length;

            for (int i = 0; (i <= nth) && (nextOffset < textLength); i++)
            {
                int startOffset = text.IndexOf(startDelimiter, nextOffset);

                if (startOffset == -1)
                    break;

                int valueOffset = startOffset + startDelimiter.Length;
                int endOffset = text.IndexOf(endDelimiter, valueOffset);

                if (endOffset != -1)
                {
                    nextOffset = endOffset + endDelimiter.Length;

                    if (i == nth)
                        returnValue = text.Substring(valueOffset, endOffset - valueOffset);
                }
                else
                {
                    nextOffset = textLength;

                    if (i == nth)
                        returnValue = text.Substring(valueOffset);
                }
            }

            return returnValue;
        }

        public static int CountChars(string text, char chr)
        {
            int count = 0;

            foreach (char c in text)
            {
                if (c == chr)
                    count++;
            }

            return count;
        }

        public static int CountStrings(string text, string subString)
        {
            int count = 0;
            int i = 0;

            while ((i = text.IndexOf(subString, i)) != -1)
            {
                i += subString.Length;
                count++;
            }

            return count;
        }

        public static int CountStrings(string text, string[] subStrings)
        {
            int count = 0;

            foreach (string subString in subStrings)
                count += CountStrings(text, subString);

            return count;
        }

        public static int CountPunctuation(string text)
        {
            int count = 0;

            if (!String.IsNullOrEmpty(text))
            {
                foreach (char chr in text)
                {
                    if (Char.IsPunctuation(chr) || LanguageLookup.PunctuationCharacters.Contains(chr))
                        count++;
                }
            }

            return count;
        }

        public static int SumStringArrayLengths(string[] strs)
        {
            int count = 0;

            if (strs == null)
                return 0;

            foreach (string str in strs)
            {
                if (!String.IsNullOrEmpty(str))
                    count += str.Length;
            }

            return count;
        }

        public static bool ContainsCaseInsensitive(string mainString, string testString)
        {
            return mainString.IndexOf(testString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static int IndexOfCaseInsensitive(string mainString, string testString)
        {
            return mainString.IndexOf(testString, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsOneOrMoreCharacters(string text, char[] characterArray)
        {
            if ((characterArray == null) || (characterArray.Length == 0))
                return false;

            foreach (char chr in text)
            {
                if (characterArray.Contains(chr))
                    return true;
            }

            return false;
        }

        public static bool ContainsWholeWord(string text, string word)
        {
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(word))
            {
                int startOfs;
                int length = text.Length;

                for (startOfs = 0; startOfs < length; startOfs++)
                {
                    int ofs = text.IndexOf(word, startOfs);

                    if (ofs == -1)
                        return false;

                    if (ofs != 0)
                    {
                        char chr = text[ofs - 1];

                        if (char.IsLetterOrDigit(chr))
                            continue;
                    }

                    ofs += word.Length;

                    if (ofs < text.Length)
                    {
                        char chr = text[ofs];

                        if (char.IsLetterOrDigit(chr))
                            continue;
                        else if ((chr == '\'') || (chr == '’'))
                        {
                            ofs++;

                            if (ofs < text.Length)
                            {
                                chr = text[ofs];

                                if (char.IsLetterOrDigit(chr))
                                    continue;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool ContainsWholeWordCaseInsensitive(string text, string word)
        {
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(word))
                return ContainsWholeWord(text.ToLower(), word.ToLower());

            return false;
        }

        public static bool ContainsWholeWordFromList(string text, string[] words)
        {
            foreach (string word in words)
            {
                if (ContainsWholeWord(text, word))
                    return true;
            }

            return false;
        }

        public static bool ContainsWholeWordFromListCaseInsensitive(string text, string[] words)
        {
            foreach (string word in words)
            {
                if (ContainsWholeWordCaseInsensitive(text, word))
                    return true;
            }

            return false;
        }

        public static int IndexOfWholeWord(string text, string word, int startIndex = 0)
        {
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(word))
            {
            retry:
                int wordIndex = text.IndexOf(word, startIndex);

                if (wordIndex == -1)
                    return -1;

                if (wordIndex != 0)
                {
                    char chr = text[wordIndex - 1];

                    if (char.IsLetterOrDigit(chr))
                    {
                        startIndex += word.Length;

                        if (startIndex > text.Length)
                            return -1;

                        goto retry;
                    }
                }

                int ofs = wordIndex + word.Length;

                if (ofs < text.Length)
                {
                    char chr = text[ofs];

                    if (char.IsLetterOrDigit(chr))
                    {
                        startIndex += word.Length;

                        if (startIndex > text.Length)
                            return -1;

                        goto retry;
                    }
                    else if ((chr == '\'') || (chr == '’'))
                    {
                        ofs++;

                        if (ofs < text.Length)
                        {
                            chr = text[ofs];

                            if (char.IsLetterOrDigit(chr))
                            {
                                startIndex += word.Length;

                                if (startIndex > text.Length)
                                    return -1;

                                goto retry;
                            }
                        }
                    }
                }

                return wordIndex;
            }

            return -1;
        }

        public static int IndexOfWholeWordCaseInsensitive(string text, string word, int startIndex = 0)
        {
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(word))
                return IndexOfWholeWord(text.ToLower(), word.ToLower(), startIndex);

            return -1;
        }

        public static string RemoveWholeWord(string text, string word)
        {
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(word))
            {
                int wordLength = word.Length;
                int textLength = text.Length;
                int startIndex = 0;
                int ofs = -1;

                for (;  startIndex < textLength; startIndex = ofs)
                {
                    ofs = text.IndexOf(word, startIndex);

                    if (ofs == -1)
                        return text;

                    int lead = 0;
                    int trail = 0;

                    if (ofs != 0)
                    {
                        char chr = text[ofs - 1];

                        if (char.IsLetterOrDigit(chr))
                            continue;

                        lead = 1;
                    }

                    int endOfs = ofs + wordLength;

                    if (endOfs < text.Length)
                    {
                        char chr = text[endOfs];

                        if (char.IsLetterOrDigit(chr))
                            continue;
                        else if ((chr == '\'') || (chr == '’'))
                        {
                            endOfs++;

                            if (endOfs < text.Length)
                            {
                                chr = text[endOfs];

                                if (char.IsLetterOrDigit(chr))
                                    continue;
                            }
                        }

                        trail = 1;
                    }

                    if (lead == 1)
                        text = text.Remove(ofs - 1, wordLength + 1);
                    else if (trail == 1)
                        text = text.Remove(ofs, wordLength + 1);
                    else
                        text = text.Remove(ofs, wordLength);

                    textLength = text.Length;
                }
            }

            return text;
        }

        public static string RemoveWholeWords(string text, string[] words)
        {
            foreach (string word in words)
                text = RemoveWholeWord(text, word);

            return text;
        }

        public static string RemoveFirstWord(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            int ofs = text.IndexOf(' ');

            if (ofs == -1)
                return text;

            int index;
            int length = text.Length;

            for (index = ofs + 1; index < length; index++)
            {
                if (text[index] != ' ')
                    break;
            }

            return text.Substring(index);
        }

        public static string RemovePunctuationFromStart(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            int startIndex;

            for (startIndex = 0; startIndex < text.Length; startIndex++)
            {
                if (!LanguageLookup.PunctuationCharacters.Contains(text[startIndex]))
                    break;
            }

            if (startIndex != 0)
                text = text.Substring(startIndex);

            return text;
        }

        public static string RemovePunctuationFromEnd(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            int endIndex;
             
            for (endIndex = text.Length - 1; endIndex >= 0; endIndex--)
            {
                if (!LanguageLookup.PunctuationCharacters.Contains(text[endIndex]))
                    break;
            }

            int length = endIndex + 1;

            if (text.Length > length)
                text = text.Substring(0, length);

            return text;
        }

        public static string RemovePunctuationFromStartAndEnd(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            int startIndex;
            int endIndex;

            for (startIndex = 0; startIndex < text.Length; startIndex++)
            {
                if (!LanguageLookup.PunctuationCharacters.Contains(text[startIndex]))
                    break;
            }

            for (endIndex = text.Length - 1; endIndex >= 0; endIndex--)
            {
                if (!LanguageLookup.PunctuationCharacters.Contains(text[endIndex]))
                    break;
            }

            int length = (endIndex + 1) - startIndex;

            if ((length > 0) && (text.Length > length))
                text = text.Substring(startIndex, length);

            return text;
        }

        public static string RemovePunctuation(string text)
        {
            var stringBuilder = new StringBuilder();

            foreach (var c in text)
            {
                if (!char.IsPunctuation(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        public static string RemoveSpacesAndPunctuation(string text)
        {
            var stringBuilder = new StringBuilder();

            foreach (var c in text)
            {
                if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        public static string RemoveSpaces(string text)
        {
            var stringBuilder = new StringBuilder();

            foreach (var c in text)
            {
                if (!char.IsWhiteSpace(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        public static string RemoveCharactersFromEnd(string text, char[] characters)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            int index;

            for (index = text.Length - 1; index >= 0; index++)
            {
                if (!characters.Contains(text[index]))
                    break;
            }

            int length = index + 1;

            if (text.Length > length)
                text = text.Substring(0, length);

            return text;
        }

        public static char[] LineBreak = new char[] { '\n' };

        public static string TextWithHtmlLineBreaks(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";

            bool hasHtml = text.Contains("<") && text.Contains(">");

            if (hasHtml)
            {
                string returnValue = text;

                returnValue = returnValue.Replace("<", "&lt;");
                returnValue = returnValue.Replace(">", "&gt;");

                returnValue = returnValue.Replace("&lt;p&gt;", "<p>");
                returnValue = returnValue.Replace("&lt;/p&gt;", "</p>");

                if (returnValue.Contains("\t"))
                    returnValue = returnValue.Replace("\t", "<span style=\"padding-left: 25px\"></span>");

                returnValue = returnValue.Trim();

                return returnValue;
            }
            else if (text.Contains("\n") || text.Contains("\t"))
            {
                string returnValue = "";
                string[] lines = text.Split(LineBreak, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    string trimmedLine = line;

                    if (trimmedLine.Contains("<"))
                    {
                        trimmedLine = trimmedLine.Replace("<", "&lt;");
                        trimmedLine = trimmedLine.Replace(">", "&gt;");

                        trimmedLine = trimmedLine.Replace("&lt;p&gt;", "<p>");
                        trimmedLine = trimmedLine.Replace("&lt;/p&gt;", "</p>");
                    }

                    if (trimmedLine.Contains("\t"))
                        trimmedLine = trimmedLine.Replace("\t", "<span style=\"padding-left: 25px\"></span>");

                    trimmedLine = trimmedLine.Trim();

                    if (trimmedLine.Length == 0)
                        returnValue += "<div class=\"lbe\"></div>\n";
                    else
                        returnValue += "<div class=\"lb\">" + trimmedLine + "</div>\n";
                }

                return returnValue;
            }
            else
                return text;
        }

        public static string SafeHtmlString(string text)
        {
            if (text != null)
            {
                // In case the string is already encoded.
                text = ApplicationData.Global.HtmlDecode(text);

                if (text.Contains("<"))
                    text = text.Replace("<", "&lt;");

                if (text.Contains(">"))
                    text = text.Replace(">", "&gt;");
            }
            else
                text = "";

            return text;
        }

        private enum MatchState { Start, InMatch, InDiff };

        // Returns HTML spans with differences highlighted.
        public static string HighlightDiffs(
            string srcText,
            string refText,
            string matchClass,
            string diffClass)
        {
            if (srcText == refText)
                return srcText;

            int srcIndex = 0, refIndex = 0, srcCount = srcText.Length, refCount = refText.Length;
            string className;
            string lastClassName = null;

            StringBuilder sb = new StringBuilder();

            for (;;)
            {
                string run = GetDiffRun(
                    srcText,
                    ref srcIndex,
                    refText,
                    ref refIndex,
                    matchClass,
                    diffClass,
                    lastClassName,
                    out className);

                if (!String.IsNullOrEmpty(run))
                    sb.Append(run);

                if ((srcIndex == srcCount) && (refIndex == refCount))
                    break;

                lastClassName = className;
            }

            return sb.ToString();
        }

        public static string GetDiffRun(
            string srcText,
            ref int srcIndex,
            string refText,
            ref int refIndex,
            string matchClass,
            string diffClass,
            string lastClassName,
            out string className)
        {
            int srcStart = srcIndex;
            int refStart = refIndex;
            int srcCount = srcText.Length, refCount = refText.Length;
            string run = String.Empty;

            className = diffClass;

            if ((srcIndex < srcCount) && (refIndex < refCount))
            {
                if (srcText[srcIndex] == refText[refIndex])
                {
                    srcIndex++;
                    refIndex++;

                    if ((srcIndex != srcCount) && (refIndex != refCount))
                    {
                        while (srcText[srcIndex] == refText[refIndex])
                        {
                            srcIndex++;
                            refIndex++;

                            if ((srcIndex == srcCount) || (refIndex == refCount))
                                break;
                        }
                    }

                    className = matchClass;

                    run = srcText.Substring(srcStart, srcIndex - srcStart);
                }
                else
                {
                    int sc = srcCount - srcIndex;
                    int rc = refCount - refIndex;
                    int bestS = sc;
                    int bestR = rc;

                    for (int s = 0; s < sc; s++)
                    {
                        char c = srcText[srcIndex + s];

                        for (int r = 0; r < rc; r++)
                        {
                            if (refText[refIndex + r] == c)
                            {
                                if (r < bestR)
                                {
                                    bestS = s;
                                    bestR = r;
                                    break;
                                }
                            }
                        }
                    }

                    className = diffClass;

                    run = srcText.Substring(srcStart, bestS);
                    srcIndex += bestS;
                    refIndex += bestR;
                }
            }
            else if (srcIndex < srcCount)
            {
                run = srcText.Substring(srcIndex, srcCount - srcIndex);
                srcIndex = srcCount;
                refIndex = refCount;
            }
            else
            {
                while (srcIndex < srcCount)
                    run += "_";

                refIndex = refCount;
            }

            if (!String.IsNullOrEmpty(run))
            {
                if (className == lastClassName)
                {
                    int runLength = run.Length;

                    run = "<span class=\"" + diffClass + "\">" + run.Substring(0, 1) + "</span>";

                    if (runLength > 1)
                        run += "<span class=\"" + className + "\">" + run.Substring(1) + "</span>";
                }
                else
                    run = "<span class=\"" + className + "\">" + run + "</span>";
            }
            else if (className == diffClass)
            {
                if (srcIndex < srcCount)
                {
                    run = srcText.Substring(srcStart, 1);
                    run = "<span class=\"" + diffClass + "\">" + run + "</span>";

                    srcIndex++;

                    if (refIndex < refCount)
                        refIndex++;
                }
                //else
                //    run = "<span class=\"" + diffClass + "\">_</span>";
            }

            return run;
        }

        public static string FormatDateTimeRFC822(DateTime value)
        {
            string str = string.Format("{0:ddd, d MMM yyyy HH:mm:ss zzzz}", value);

            if (str[str.Length - 3] == ':')
                str = str.Remove(str.Length - 3, 1);

            return str;
        }


        static string[] sizeSuffixes = {
        "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string FormatSize(long size)
        {
            const string formatTemplate = "{0}{1:0.#} {2}";

            if (size == 0)
            {
                return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
            }

            var absSize = Math.Abs((double)size);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= sizeSuffixes.Length
                ? sizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);

            return string.Format(
                formatTemplate,
                size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
        }

        public static bool HasWhiteSpace(string text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                if (LanguageLookup.SpaceCharacters.Contains(c))
                    return true;
            }

            return false;
        }

        // Converts all space characters to standard spaces ('\x0020').
        public static string NormalizeSpaces(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            foreach (char c in LanguageLookup.JustSpaces)
                text = text.Replace(c, ' ');

            return text;
        }

        // Convert space characters to standard spaces ('\x0020') except for zero-width spaces.
        // Old: Converts all space characters to standard spaces ('\x0020'), but for
        // Old: zero-width spaces, include the zero-width space too.
        public static string NormalizeSpacesZero(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            foreach (char c in LanguageLookup.JustSpaces)
            {
                if ((c != ' ') && (c != LanguageLookup.ZeroWidthSpace) && (c != LanguageLookup.ZeroWidthNoBreakSpace))
                    text = text.Replace(c, ' ');
                /* Old
                if (c == LanguageLookup.ZeroWidthSpace)
                    text = text.Replace(LanguageLookup.ZeroWidthSpaceString, LanguageLookup.ZeroWidthSpaceStringWithSpace);
                else if (c == LanguageLookup.ZeroWidthNoBreakSpace)
                    text = text.Replace(LanguageLookup.ZeroWidthNoBreakSpaceString, LanguageLookup.ZeroWidthNoBreakSpaceStringWithSpace);
                else
                    text = text.Replace(c, ' ');
                */
            }

            return text;
        }

        public static string TrimIncludingZeroSpace(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            bool notDone = true;
            while (notDone && (text.Length != 0))
            {
                notDone = false;
                foreach (char ws in LanguageLookup.SpaceCharacters)
                {
                    if (String.IsNullOrEmpty(text))
                        return text;
                    if (text[0] == ws)
                    {
                        text = text.Substring(1);
                        notDone = true;
                        if (String.IsNullOrEmpty(text))
                            return text;
                    }
                    if (text[text.Length - 1] == ws)
                    {
                        text = text.Substring(0, text.Length - 1);
                        notDone = true;
                        if (String.IsNullOrEmpty(text))
                            return text;
                    }
                }
            }
            return text;
        }

        public static string StripWhiteSpace(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            foreach (string ws in LanguageLookup.SpaceStrings)
                text = text.Replace(ws, "");

            return text;
        }

        public static string StripMatchedChars(string text, char start, char stop)
        {
            StringBuilder sb = new StringBuilder();
            int count = text.Length;
            int index;
            int tagNestLevel = 0;
            char c;

            for (index = 0; index < count; index++)
            {
                c = text[index];

                if (c == start)
                    tagNestLevel++;
                else if (c == stop)
                    tagNestLevel--;
                else
                {
                    if (tagNestLevel <= 0)
                        sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string StripMatchedStrings(string text, string start, string stop)
        {
            StringBuilder sb = new StringBuilder();
            int count = text.Length;
            int index;
            int tagNestLevel = 0;

            for (index = 0; index < count; index++)
            {
                if (String.Compare(text, index, start, 0, start.Length) == 0)
                    tagNestLevel++;
                else if (String.Compare(text, index, stop, 0, stop.Length) == 0)
                    tagNestLevel--;
                else
                {
                    if (tagNestLevel <= 0)
                        sb.Append(text[index]);
                }
            }

            return sb.ToString();
        }

        public static string StripHtml(string text)
        {
            StringBuilder sb = new StringBuilder();
            int count = text.Length;
            int index;
            int tagNestLevel = 0;
            char c;

            for (index = 0; index < count; index++)
            {
                c = text[index];

                switch (c)
                {
                    case '<':
                        tagNestLevel++;
                        break;
                    case '>':
                        tagNestLevel--;
                        break;
                    default:
                        if (tagNestLevel <= 0)
                            sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        public static string FormatXml(string xml)
        {
            try
            {
                HtmlFormatter formatter = new HtmlFormatter();
                string newXml = formatter.Beautify(xml);
                return newXml;
                /*
                string wrapText = "<div>" + xml + "</div>";
                XDocument doc = XDocument.Parse(wrapText);
                string returnValue = doc.ToString();
                returnValue = returnValue.Substring(5, returnValue.Length - 11);
                returnValue = returnValue.Replace("\n  ", "\n");
                returnValue = returnValue.Replace("\r\n  ", "\r\n");
                return returnValue;
                */
            }
            catch (Exception)
            {
                return xml;
            }
        }

        public static bool IsHtml(string text)
        {
            return HtmlFormatter.IsHtml(text);
        }

        public static bool IsUpperString(string text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            bool isUpper = true;

            foreach (char chr in text)
            {
                if (!char.IsUpper(chr))
                {
                    isUpper = false;
                    break;
                }
            }

            return isUpper;
        }

        public static bool IsLowerString(string text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            bool isLower = true;

            foreach (char chr in text)
            {
                if (!char.IsLower(chr))
                {
                    isLower = false;
                    break;
                }
            }

            return isLower;
        }

        public static string FixupCacheKey(string cacheKey)
        {
            if (String.IsNullOrEmpty(cacheKey))
                return cacheKey;

            cacheKey = cacheKey.Replace(':', '_');

            return cacheKey;
        }

        public static string ReplaceExpPlaceholdersWithAudioButtons(List<AudioMultiReference> audioRecords, string htmlText, LanguageID languageID)
        {
            string returnValue = htmlText;
            int index = 0;
            int endIndex;
            string startPattern = "<span class=\"exp\">";
            int startPatternLength = startPattern.Length;
            string endPattern = "</span>";
            int endPatternLength = endPattern.Length;
            int keyIndex;
            int keyLength;
            string keyString;
            AudioMultiReference audioRecord;
            string matchString;
            string replacement;
            string args;

            while ((index < returnValue.Length) && ((index = returnValue.IndexOf(startPattern, index)) != -1))
            {
                keyIndex = index + startPatternLength;

                endIndex = returnValue.IndexOf(endPattern, keyIndex);

                if (endIndex == -1)
                    endIndex = returnValue.Length;

                keyLength = endIndex - keyIndex;
                keyString = returnValue.Substring(keyIndex, keyLength);

                if (audioRecords != null)
                    audioRecord = audioRecords.FirstOrDefault(
                        x => IsEqualStringsIgnoreCase(x.KeyString, keyString)
                            && (x.LanguageID == languageID));
                else
                    audioRecord = null;

                matchString = startPattern + keyString + endPattern;

                if (audioRecord != null)
                {
                    /*
                    args = "'" + audioRecord.Key + "', '" + audioRecord.LanguageID.LanguageCultureExtensionCode + "'";
                    replacement =
                        " <label class=\"playlabel\" onclick=\"PlayDictionaryAudio(" + args + ")\"><img src=\"/Content/Images/Play.png\" alt=\"Play\" class=\"buttonimage\" align=\"middle\" /><span>Play</span></label>"
                        + " <label class=\"playlabel\" onclick=\"PlaySlowDictionaryAudio(" + args + ")\"><img src=\"/Content/Images/PlaySlow.png\" alt=\"Play Slow\" class=\"buttonimage\" align=\"middle\" /><span>Play Slow</span></label>";
                    */
                    args = "'" + audioRecord.Name + "'";
                    replacement =
                        " <label class=\"playlabel\" onclick=\"jt_playAudioFile(" + args + ")\"><img src=\"/Content/Images/Play.png\" alt=\"Play\" class=\"buttonimage\" align=\"middle\" /><span>Play</span></label>"
                        + " <label class=\"playlabel\" onclick=\"jt_playSlowAudioFile(" + args + ")\"><img src=\"/Content/Images/PlaySlow.png\" alt=\"Play Slow\" class=\"buttonimage\" align=\"middle\" /><span>Play Slow</span></label>";
                }
                else
                    replacement = String.Empty;

                returnValue = returnValue.Replace(matchString, replacement);

                index = endIndex + endPatternLength;
            }

            return returnValue;
        }

        public static bool NeedSpaceAfterCharacter(char c)
        {
            if (LanguageLookup.SpaceCharacters.Contains(c))
                return false;
            string str = c.ToString();
            if (LanguageLookup.FatPunctuationCharacters.Contains(str))
                return false;
            if (LanguageLookup.FatPunctuationCharacters.Contains(str))
                return false;
            return true;
        }

        public static bool NeedSentenceSeparator(char previousChar)
        {
            bool returnValue = NeedSpaceAfterCharacter(previousChar);
            return returnValue;
        }

        public static string GetSentenceSeparator(LanguageID languageID, char previousChar)
        {
            string returnValue = " ";
            if (!NeedSpaceAfterCharacter(previousChar))
                returnValue = String.Empty;
            return returnValue;
        }

        public static string JoinSentences(string text)
        {
            string str = text.Replace("\r\n", " ");
            str = text.Replace("\r", " ");
            str = text.Replace("\n", " ");

            string lastStr;

            do
            {
                lastStr = str;
                str = str.Replace("  ", " ");
            }
            while (str != lastStr);

            return str;
        }

        public static string JoinErrorMessages(
            string msg1,
            string msg2)
        {
            string str = msg1;

            if (String.IsNullOrEmpty(msg2))
                return str;

            if (!String.IsNullOrEmpty(str))
                str += "\r\n" + msg2;
            else
                str = msg2;

            return str;
        }

        public static string GetAbbreviation(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            StringBuilder sb = new StringBuilder();

            foreach (char c in str)
            {
                if (char.IsUpper(c) || char.IsDigit(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public static int FindIndexOfFirstCharInArray(string text, char[] array)
        {
            int index;
            int length = text.Length;

            for (index = 0; index < length; index++)
            {
                char chr = text[index];

                if (array.Contains(chr))
                    return index;
            }

            return -1;
        }

        public static string FilterAsides(string str)
        {
            return FilterAsidesRaw(str, LanguageLookup.MatchedAsideCharacters);
        }

        public static string FilterSpeechAsides(string str)
        {
            return FilterAsidesRaw(str, LanguageLookup.MatchedSpeechAsideCharacters);
        }

        public static string FilterAsidesRaw(string str, char[] asides)
        {
            StringBuilder sb = new StringBuilder();

            if (str == null)
                str = String.Empty;

            int index;
            int count = str.Length;

            for (index = 0; index < count; index++)
            {
                char chr = str[index];

                if (asides.Contains(chr))
                {
                    char endChr = LanguageLookup.GetMatchedEndChar(chr);
                    for (int i = index + 1; i < count; i++)
                    {
                        if (str[i] == endChr)
                        {
                            index = i;
                            break;
                        }
                    }
                }
                else
                    sb.Append(chr);
            }

            return sb.ToString();
        }

        // Get langth of string ignoring zero-width characters.
        public static int DisplayLength(string str)
        {
            int count = 0;

            foreach (char chr in str)
            {
                switch (chr)
                {
                    // Zero-width spaces.
                    case '\x200B':
                    case '\xFEFF':
                    // Marshallese.
                    case '\u0304':
                        break;
                    case '\u0327':
                        break;
                    default:
                        count++;
                        break;
                }
            }

            return count;
        }

        // Return a string with count number of spaces.
        public static string GetSpaces(int count)
        {
            string str = String.Empty;

            while (count-- != 0)
                str += " ";

            return str;
        }

        // Return a string with count number of the character.
        public static string GetRepeatedCharacterString(char chr, int count)
        {
            string str = String.Empty;

            while (count-- != 0)
                str += chr;

            return str;
        }

        public static bool IsEqualStringsIgnoreCase(string str1, string str2)
        {
            return String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(string str1, string str2)
        {
            return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(string str1, string str2)
        {
            return str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);
        }

        public static string AppendErrorMessage(string errorMessage, string newMessage, bool uniqueOnly)
        {
            if (String.IsNullOrEmpty(errorMessage))
                return newMessage;
            else if (!uniqueOnly || !errorMessage.Contains(newMessage))
                return errorMessage + "\n" + newMessage;

            return errorMessage;
        }
    }
}

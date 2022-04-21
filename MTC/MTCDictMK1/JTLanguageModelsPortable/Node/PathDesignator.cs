using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Node
{
    public class PathDesignator
    {
        public List<string> Nodes { get; set; }
        public string NodePath { get; set; }
        public List<int> ParagraphSelection { get; set; }    // Pairs of start and end paragraph or verse numbers.
        public string ParagraphSelectionTitle { get; set; }  // i.e. "#5;7-8"
        public string VerseKey { get; set; }  // i.e. "study_summary1"  (only for 1 special verse reference)

        public PathDesignator(string pathString)
        {
            ClearPathDesignator();
            SetFromPathString(pathString);
        }

        public PathDesignator(PathDesignator other)
        {
            CopyPathDesignator(other);
        }

        public PathDesignator()
        {
            ClearPathDesignator();
        }

        public void ClearPathDesignator()
        {
            Nodes = null;
            ParagraphSelection = null;
            ParagraphSelectionTitle = String.Empty;
            VerseKey = String.Empty;
        }

        public void CopyPathDesignator(PathDesignator other)
        {
            if (other.Nodes != null)
                Nodes = new List<string>(other.Nodes);
            else
                Nodes = null;

            if (other.ParagraphSelection != null)
                ParagraphSelection = new List<int>(other.ParagraphSelection);
            else
                ParagraphSelection = null;

            ParagraphSelectionTitle = other.ParagraphSelectionTitle;
            VerseKey = other.VerseKey;
        }

        public bool SetFromPathString(string pathString)
        {
            ClearPathDesignator();
            pathString = pathString.Trim();

            if (String.IsNullOrEmpty(pathString))
                return false;

            string[] paths = pathString.Split(LanguageLookup.Pound, StringSplitOptions.None);

            NodePath = paths[0];

            string paragraphSelectionString = String.Empty;

            if (paths.Length > 1)
            {
                paragraphSelectionString = paths[1];
                ParagraphSelectionTitle = ":" + paragraphSelectionString;
            }

            string[] parts = NodePath.Split(LanguageLookup.Slash, StringSplitOptions.None);
            int partCount = parts.Count();

            Nodes = parts.ToList();

            for (int i = 0; i < Nodes.Count(); i++)
            {
                if (String.IsNullOrEmpty(Nodes[i]))
                    Nodes[i] = "*";
            }

            List<int> paragraphSelection;
            string verseKey;

            if (GetStartStopPairs(paragraphSelectionString, out paragraphSelection, out verseKey))
            {
                ParagraphSelection = paragraphSelection;
                VerseKey = verseKey;
            }

            return true;
        }

        public int GetNodeCount()
        {
            if (Nodes != null)
                return Nodes.Count();

            return 0;
        }

        public string GetNodeNameIndexed(int index)
        {
            if (Nodes == null)
                return null;

            if ((index < 0) || (index >= GetNodeCount()))
                return null;

            return Nodes[index];
        }

        public bool HasParagraphSelection()
        {
            if (ParagraphSelection == null)
                return false;

            if (ParagraphSelection.Count() == 0)
                return false;

            return true;
        }

        public List<int> GetParagraphNumbers()
        {
            List<int> numbers = new List<int>();

            for (int i = 0; i < ParagraphSelection.Count(); i += 2)
            {
                int start = ParagraphSelection[i];
                int stop = ParagraphSelection[i + 1];

                for (int number = start; number <= stop; number++)
                    numbers.Add(number);
            }

            return numbers;
        }

        public List<string> GetParagraphNumberStrings()
        {
            List<string> numbers = new List<string>();

            for (int i = 0; i < ParagraphSelection.Count(); i += 2)
            {
                int start = ParagraphSelection[i];
                int stop = ParagraphSelection[i + 1];

                for (int number = start; number <= stop; number++)
                    numbers.Add(number.ToString());
            }

            return numbers;
        }

        public bool MatchNode(BaseObjectNode node, LanguageID uiLanguageID)
        {
            List<string> namePath = node.GetNamePath(uiLanguageID);
            int namePathCount = namePath.Count();
            int nodeCount = Nodes.Count();
            int nodeIndex;

            for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                string nodePatternName = Nodes[nodeIndex];

                if (nodeIndex >= namePathCount)
                    return true;

                string nodeName = namePath[nodeIndex];

                if (nodePatternName != "*")
                {
                    if (!TextUtilities.IsEqualStringsIgnoreCase(nodeName, nodePatternName))
                        return false;
                }
            }

            return true;
        }

        public bool MatchStudyItem(MultiLanguageItem studyItem, LanguageID uiLanguageID)
        {
            if (MatchNode(studyItem.Node, uiLanguageID))
            {
                if (!String.IsNullOrEmpty(VerseKey))
                {
                    if (studyItem.KeyString != VerseKey)
                        return false;
                }

                if ((ParagraphSelection == null) || (ParagraphSelection.Count() == 0))
                    return true;

                ContentStudyList studyList = studyItem.StudyList;
                Annotation annotation = studyItem.FindAnnotation("Ordinal");
                int ordinal;

                if (annotation == null)
                    ordinal = studyList.GetStudyItemIndex(studyItem);
                else
                    ordinal = ObjectUtilities.GetIntegerFromString(annotation.Value, 0);

                int count = ParagraphSelection.Count();
                int index;

                for (index = 0; index < count; index += 2)
                {
                    if ((ordinal >= ParagraphSelection[index]) && (ordinal <= ParagraphSelection[index + 1]))
                        return true;
                }
            }

            return false;
        }

        public static List<PathDesignator> GetPathDesignators(List<string> pathPatterns)
        {
            List<PathDesignator> pathDesignators = new List<PathDesignator>();

            if (pathPatterns == null)
                pathPatterns = new List<string>() { "*/*/*" };

            foreach (string pathPattern in pathPatterns)
            {
                if (String.IsNullOrEmpty(pathPattern))
                    continue;

                PathDesignator pathDesignator = new PathDesignator(pathPattern);

                pathDesignators.Add(pathDesignator);
            }

            return pathDesignators;
        }

        // Convert string like "1,3-7" to a list of ints like {1, 1, 3, 7}.
        public static bool GetStartStopPairs(
            string paragraphSelectionString,
            out List<int> startStopPairs,
            out string verseKey)
        {
            bool returnValue = false;

            startStopPairs = null;
            verseKey = String.Empty;

            if (!String.IsNullOrEmpty(paragraphSelectionString))
            {
                string[] selections = paragraphSelectionString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                startStopPairs = new List<int>();

                foreach (string selection in selections)
                {
                    if (String.IsNullOrEmpty(selection))
                        continue;

                    if (char.IsDigit(selection[0]))
                    {
                        if (selection.Contains("-"))
                        {
                            int ofs = selection.IndexOf("-");
                            string startString = selection.Substring(0, ofs).Trim();
                            string stopString = selection.Substring(ofs + 1).Trim();
                            int start = ObjectUtilities.GetIntegerFromString(startString, 0);
                            int stop = ObjectUtilities.GetIntegerFromString(stopString, start);
                            startStopPairs.Add(start);
                            startStopPairs.Add(stop);
                        }
                        else
                        {
                            int start = ObjectUtilities.GetIntegerFromString(selection, 0);
                            startStopPairs.Add(start);
                            startStopPairs.Add(start);
                        }
                        returnValue = true;
                    }
                    else if (char.IsLetter(selection[0]))
                    {
                        verseKey = selection;
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        public static bool MatchStartStopPair(int number, List<int> startStopPairs)
        {
            if (startStopPairs == null)
                return false;

            int count = startStopPairs.Count();
            int index;

            for (index = 0; index < count; index += 2)
            {
                int start = startStopPairs[index];
                int stop = startStopPairs[index + 1];

                if ((number >= start) && (number <= stop))
                    return true;
            }

            return false;
        }
    }
}

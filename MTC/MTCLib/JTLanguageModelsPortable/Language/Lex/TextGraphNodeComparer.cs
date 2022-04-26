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
    public class TextGraphNodeComparer : IComparer<TextGraphNode>
    {
        public TextGraph Graph;
        public LanguageTool Tool;
        public LanguageID BestLanguageID;

        public TextGraphNodeComparer(TextGraph graph, LanguageTool tool, LanguageID bestLanguageID)
        {
            Graph = graph;
            Tool = tool;
            BestLanguageID = bestLanguageID;
        }

        public TextGraphNodeComparer(TextGraph graph, LanguageID bestLanguageID)
        {
            Graph = graph;
            Tool = null;
            BestLanguageID = bestLanguageID;
        }

        // Compares length of nodes.  If x length is greater than y length returns 1.
        public int Compare(TextGraphNode x, TextGraphNode y)
        {
            if (Tool != null)
                return Tool.CompareTextGraphNodeForWeight(x, y, Graph, BestLanguageID);

            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if ((x.Entry == null) || (y.Entry == null))
            {
                if (x.Entry == y.Entry)
                    return 0;
                else if (x.Entry == null)
                    return -1;
                else
                    return 1;
            }

            /*
             * This doesn't work if the embedded alternate text is valid.
            LanguageID languageIDX = x.Entry.LanguageID;
            LanguageID languageIDY = y.Entry.LanguageID;

            if (x.Text == x.Entry.)
            if (languageIDX != languageIDY)
            {
                if (languageIDX == BestLanguageID)
                    return 1;
                else if (languageIDY == BestLanguageID)
                    return -1;
            }
            */

            bool xIsStem = x.Entry.HasSenseWithStemOnly();
            bool yIsStem = y.Entry.HasSenseWithStemOnly();

            if (xIsStem != yIsStem)
            {
                if (yIsStem)
                    return 1;
                return -1;
            }

            if (x.Length > y.Length)
                return 1;

            if (x.Length < y.Length)
                return -1;

            return 0;
        }
    }
}

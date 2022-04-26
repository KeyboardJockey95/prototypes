using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class LexTable : BaseObjectLanguages
    {
        private StringMapper _CategoryMap;
        private ObjectMapper<Designator> _DesignatorMap;
        public List<LexNode> Nodes;
        public Dictionary<string, LexNode> EndingNodes;
        public int Version;

        public LexTable(
            List<LanguageID> targetLanguageIDs,
            List<Designator> designations,
            int version) : base(targetLanguageIDs.First().Key, targetLanguageIDs, null, null)
        {
            ClearLexTable();
            _DesignatorMap.AddList(designations);
            Version = version;
        }

        public LexTable(
            List<LanguageID> targetLanguageIDs,
            List<Designator> designations1,
            List<Designator> designations2,
            int version) : base(targetLanguageIDs.First().Key, targetLanguageIDs, null, null)
        {
            ClearLexTable();
            _DesignatorMap.AddList(designations1);
            _DesignatorMap.AddList(designations2);
            Version = version;
        }

        public LexTable(
            List<LanguageID> targetLanguageIDs,
            List<List<Designator>> designatorLists,
            int version) : base(targetLanguageIDs.First().Key, targetLanguageIDs, null, null)
        {
            ClearLexTable();
            foreach (List<Designator> designatorList in designatorLists)
                _DesignatorMap.AddList(designatorList);
            Version = version;
        }

        public LexTable(LexTable other)
            : base(other)
        {
            _CategoryMap = new StringMapper(other._CategoryMap);
            _DesignatorMap = new ObjectMapper<Designator>(other._DesignatorMap);
            Nodes = other.Nodes;
            EndingNodes = other.EndingNodes;
            Version = other.Version;
            ModifiedFlag = false;
        }

        public LexTable(XElement element)
        {
            ClearLexTable();
            OnElement(element);
        }

        public LexTable()
        {
            ClearLexTable();
        }

        public void ClearLexTable()
        {
            _CategoryMap = new StringMapper();
            _DesignatorMap = new ObjectMapper<Designator>();
            Nodes = new List<LexNode>();
            EndingNodes = new Dictionary<string, LexNode>();
            Version = 0;
        }

        private List<LexCategoryDesignation> GetCategoryDesignationsFromIDs(
            List<int> categoryDesignationIDs)
        {
            if ((categoryDesignationIDs == null) || (categoryDesignationIDs.Count() == 0))
                return null;

            List<LexCategoryDesignation> categoryDesignations = new List<LexCategoryDesignation>(categoryDesignationIDs.Count());
            int c = categoryDesignationIDs.Count();

            for (int i = 0; i < c; i += 2)
            {
                int categoryID = categoryDesignationIDs[i];
                int designatorID = categoryDesignationIDs[i + 1];
                string category = _CategoryMap.GetByID(categoryID);
                Designator designation = _DesignatorMap.GetByID(designatorID);
                LexCategoryDesignation categoryDesignation = new Language.LexCategoryDesignation(category, designation);
                categoryDesignations.Add(categoryDesignation);
            }

            return categoryDesignations;
        }

        public bool Parse(string input, out List<LexItem> results)
        {
            LexNode node = null;
            LexNode lastNode = null;
            LexItem lexItem;
            int length;
            int index;
            char c;

            results = null;

            if (String.IsNullOrEmpty(input))
            {
                foreach (LexNode eNode in Nodes)
                {
                    if (eNode.Key == '\0')
                    {
                        lexItem = new LexItem(
                            input,
                            new MultiLanguageString(null, TargetLanguageIDs, eNode.TextList),
                            GetCategoryDesignationsFromIDs(eNode.CategoryDesignationIDs));

                        if (results == null)
                            results = new List<LexItem>() { lexItem };
                        else
                            results.Add(lexItem);

                        return true;
                    }
                }

                return false;
            }

            length = input.Length;
            c = char.ToLower(input[0]);

            foreach (LexNode aNode in Nodes)
            {
                if (aNode.Key == c)
                {
                    node = aNode;
                    break;
                }
            }

            if (node == null)
                return false;

            if (node.CategoryDesignationCount != 0)
            {
                lexItem = new LexItem(
                    input.Substring(0, 1),
                    new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                    GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));

                if (results == null)
                    results = new List<LexItem>() { lexItem };
                else
                    results.Add(lexItem);
            }

            lastNode = node;

            for (index = 1; index < length; index++)
            {
                node = null;
                c = char.ToLower(input[index]);

                if (lastNode.Children != null)
                {
                    foreach (LexNode bNode in lastNode.Children)
                    {
                        if (bNode.Key == c)
                        {
                            node = bNode;
                            break;
                        }
                    }
                }

                if (node == null)
                    break;

                if (node.CategoryDesignationCount != 0)
                {
                    lexItem = new LexItem(
                        input.Substring(0, index + 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));

                    if (results == null)
                        results = new List<LexItem>() { lexItem };
                    else
                        results.Add(lexItem);
                }

                lastNode = node;
            }

            if (results != null)
                return true;

            return false;
        }

        public LexItem ParseShortest(string input)
        {
            LexNode node = null;
            LexNode lastNode = null;
            LexItem lexItem;
            int length;
            int index;
            char c;

            if (String.IsNullOrEmpty(input))
            {
                foreach (LexNode eNode in Nodes)
                {
                    if (eNode.Key == '\0')
                    {
                        lexItem = new LexItem(
                            input,
                            new MultiLanguageString(null, TargetLanguageIDs, eNode.TextList),
                            GetCategoryDesignationsFromIDs(eNode.CategoryDesignationIDs));
                        return lexItem;
                    }
                }

                return null;
            }

            length = input.Length;
            c = input[0];

            foreach (LexNode aNode in Nodes)
            {
                if (aNode.Key == c)
                {
                    node = aNode;
                    break;
                }
            }

            if (node == null)
                return null;

            if (length == 1)
            {
                if (node.CategoryDesignationCount != 0)
                {
                    lexItem = new LexItem(
                        input.Substring(0, 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));
                    return lexItem;
                }

                return null;
            }

            lastNode = node;

            for (index = 1; index < length; index++)
            {
                node = null;
                c = input[index];

                if (lastNode.Children != null)
                {
                    foreach (LexNode bNode in lastNode.Children)
                    {
                        if (bNode.Key == c)
                        {
                            node = bNode;
                            break;
                        }
                    }
                }

                if (node == null)
                    return null;

                if (node.CategoryDesignationCount != 0)
                {
                    lexItem = new LexItem(
                        input.Substring(0, index),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));
                    return lexItem;
                }

                lastNode = node;
            }

            return null;
        }

        public LexItem ParseLongest(string input)
        {
            LexNode node = null;
            LexNode lastNode = null;
            LexItem lexItem = null;
            int length;
            int index;
            char c;

            if (String.IsNullOrEmpty(input))
            {
                foreach (LexNode eNode in Nodes)
                {
                    if (eNode.Key == '\0')
                    {
                        lexItem = new LexItem(
                            input,
                            new MultiLanguageString(null, TargetLanguageIDs, eNode.TextList),
                            GetCategoryDesignationsFromIDs(eNode.CategoryDesignationIDs));
                        return lexItem;
                    }
                }

                return null;
            }

            length = input.Length;
            c = input[0];

            foreach (LexNode aNode in Nodes)
            {
                if (aNode.Key == c)
                {
                    node = aNode;
                    break;
                }
            }

            if (node == null)
                return null;

            if (length == 1)
            {
                if (node.CategoryDesignationCount != 0)
                {
                    lexItem = new LexItem(
                        input.Substring(0, 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));
                    return lexItem;
                }

                return null;
            }

            lastNode = node;

            for (index = 1; index < length; index++)
            {
                node = null;
                c = input[index];

                if (lastNode.Children != null)
                {
                    foreach (LexNode bNode in lastNode.Children)
                    {
                        if (bNode.Key == c)
                        {
                            node = bNode;
                            break;
                        }
                    }
                }

                if (node == null)
                {
                    if (lastNode.CategoryDesignationCount != 0)
                    {
                        lexItem = new LexItem(
                            input.Substring(0, 1),
                            new MultiLanguageString(null, TargetLanguageIDs, lastNode.TextList),
                            GetCategoryDesignationsFromIDs(lastNode.CategoryDesignationIDs));
                        return lexItem;
                    }
                    break;
                }

                if (node.CategoryDesignationCount != 0)
                    lexItem = new LexItem(
                        input.Substring(0, index + 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));

                lastNode = node;
            }

            return lexItem;
        }

        public LexItem ParseExact(string input)
        {
            LexNode node = null;
            LexNode lastNode = null;
            LexItem lexItem = null;
            int length;
            int index;
            char c;

            if (String.IsNullOrEmpty(input))
            {
                foreach (LexNode eNode in Nodes)
                {
                    if (eNode.Key == '\0')
                    {
                        lexItem = new LexItem(
                            input,
                            new MultiLanguageString(null, TargetLanguageIDs, eNode.TextList),
                            GetCategoryDesignationsFromIDs(eNode.CategoryDesignationIDs));
                        return lexItem;
                    }
                }

                return null;
            }

            length = input.Length;
            c = input[0];

            foreach (LexNode aNode in Nodes)
            {
                if (aNode.Key == c)
                {
                    node = aNode;
                    break;
                }
            }

            if (node == null)
                return null;

            if (length == 1)
            {
                if (node.CategoryDesignationCount != 0)
                {
                    lexItem = new LexItem(
                        input.Substring(0, 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));
                    return lexItem;
                }

                return null;
            }

            lastNode = node;

            for (index = 1; index < length; index++)
            {
                node = null;
                c = input[index];

                if (lastNode.Children != null)
                {
                    foreach (LexNode bNode in lastNode.Children)
                    {
                        if (bNode.Key == c)
                        {
                            node = bNode;
                            break;
                        }
                    }
                }

                if (node == null)
                    break;

                if ((node.CategoryDesignationCount != 0) && (index == length - 1))
                    lexItem = new LexItem(
                        input.Substring(0, index + 1),
                        new MultiLanguageString(null, TargetLanguageIDs, node.TextList),
                        GetCategoryDesignationsFromIDs(node.CategoryDesignationIDs));

                lastNode = node;
            }

            return lexItem;
        }

        public bool Add(string value, MultiLanguageString text, string category, Designator designation)
        {
            LexNode node = null;
            LexNode lastNode = null;
            int length;
            int index;
            char c;

            int categoryID = _CategoryMap.Add(category);
            int designationID = _DesignatorMap.Add(designation);

            if (String.IsNullOrEmpty(value))
            {
                foreach (LexNode eNode in Nodes)
                {
                    if (eNode.Key == '\0')
                    {
                        eNode.AddCategoryAndDesignationID(categoryID, designationID);
                        return true;
                    }
                }

                node = new LexNode('\0', text.StringList(TargetLanguageIDs), categoryID, designationID);
                Nodes.Add(node);
                return true;
            }

            length = value.Length;
            c = value[0];

            foreach (LexNode aNode in Nodes)
            {
                if (aNode.Key == c)
                {
                    node = aNode;
                    break;
                }
            }

            if (node == null)
            {
                if (length == 1)
                    node = new LexNode(c, text.StringList(TargetLanguageIDs), categoryID, designationID);
                else
                    node = new LexNode(c);

                Nodes.Add(node);
            }
            else if (length == 1)
            {
                if (node.TextList == null)
                    node.TextList = text.StringList(TargetLanguageIDs);

                node.AddCategoryAndDesignationID(categoryID, designationID);
                return true;
            }

            lastNode = node;

            for (index = 1; index < length; index++)
            {
                node = null;
                c = value[index];

                if (lastNode.Children != null)
                {
                    foreach (LexNode bNode in lastNode.Children)
                    {
                        if (bNode.Key == c)
                        {
                            node = bNode;
                            break;
                        }
                    }
                }

                if (node == null)
                {
                    if (index == length - 1)
                        node = new LexNode(c, text.StringList(TargetLanguageIDs), categoryID, designationID);
                    else
                        node = new LexNode(c);

                    if (lastNode.Children == null)
                        lastNode.Children = new List<LexNode>() { node };
                    else
                        lastNode.Children.Add(node);
                }
                else if (index == length - 1)
                {
                    if (node.TextList == null)
                        node.TextList = text.StringList(TargetLanguageIDs);

                    node.AddCategoryAndDesignationID(categoryID, designationID);
                }

                lastNode = node;
            }

            if (lastNode != null)
                AddEndingNode(lastNode);

            return true;
        }

        protected void AddEndingNode(LexNode node)
        {
            if (node == null)
                return;

            foreach (string key in node.TextList)
            {
                LexNode existingNode;

                if (!EndingNodes.TryGetValue(key, out existingNode))
                    EndingNodes.Add(key, node);
                else if (ObjectUtilities.CompareIntLists(node.CategoryDesignationIDs, existingNode.CategoryDesignationIDs) != 0)
                    ApplicationData.Global.PutConsoleErrorMessage("Non matching duplicate ending node for: " + key);
            }
        }

        public int GetLongest(LanguageID languageID)
        {
            int longest = 0;
            int languageIndex = TargetLanguageIDs.IndexOf(languageID);

            if (languageIndex == -1)
                return 0;

            foreach (LexNode node in Nodes)
                node.GetLongest(languageIndex, ref longest);

            return longest;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XElement("Version", Version));
            element.Add(_CategoryMap.GetElement("CategoryMap"));

            foreach (LexNode lexNode in Nodes)
            {
                XElement lexNodeBinElement = lexNode.GetElement("N");
                element.Add(lexNodeBinElement);
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Version":
                    Version = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "CategoryMap":
                    _CategoryMap.OnElement(childElement);
                    break;
                case "N":
                    {
                        LexNode lexNode = new LexNode(childElement);
                        if (Nodes != null)
                            Nodes.Add(lexNode);
                        else
                            Nodes = new List<LexNode>() { lexNode };
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

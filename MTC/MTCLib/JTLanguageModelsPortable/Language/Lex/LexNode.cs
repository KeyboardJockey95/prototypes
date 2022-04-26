using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class LexNode
    {
        public char Key;
        public List<string> TextList;
        public List<int> CategoryDesignationIDs;
        public List<LexNode> Children;

        // For leaf.
        public LexNode(char key, List<string> textList, int categoryID, int designatorID)
        {
            Key = key;
            TextList = textList;
            CategoryDesignationIDs = new List<int>() { categoryID, designatorID };
            Children = null;
        }

        // For non-leaf.
        public LexNode(char key)
        {
            Key = key;
            TextList = null;
            CategoryDesignationIDs = null;
            Children = null;
        }

        public LexNode(LexNode other)
        {
            Key = other.Key;

            if (other.TextList != null)
                TextList = new List<string>(other.TextList);
            else
                TextList = null;

            if (other.CategoryDesignationIDs != null)
                CategoryDesignationIDs = new List<int>(other.CategoryDesignationIDs);
            else
                CategoryDesignationIDs = null;

            if (other.Children != null)
                Children = new List<LexNode>(other.Children);
            else
                Children = null;
        }

        public LexNode(XElement element)
        {
            OnElement(element);
        }

        public LexNode()
        {
            ClearLexNode();
        }

        public void ClearLexNode()
        {
            Key = '\0';
            TextList = null;
            CategoryDesignationIDs = null;
            Children = null;
        }

        public int CategoryDesignationCount
        {
            get
            {
                if (CategoryDesignationIDs == null)
                    return 0;

                return CategoryDesignationIDs.Count();
            }
        }

        public void AddCategoryAndDesignationID(int categoryID, int designatorID)
        {
            if (CategoryDesignationIDs == null)
                CategoryDesignationIDs = new List<int>() { categoryID, designatorID };
            else
            {
                int c = CategoryDesignationIDs.Count();
                bool found = false;

                for (int i = 0; i < c; i += 2)
                {
                    if ((CategoryDesignationIDs[i] == categoryID) && (CategoryDesignationIDs[i + 1] == designatorID))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    CategoryDesignationIDs.Add(categoryID);
                    CategoryDesignationIDs.Add(designatorID);
                }
            }
        }

        public void GetLongest(int languageIndex, ref int longest)
        {
            if (TextList != null)
            {
                int length = TextList[languageIndex].Length;

                if (length > longest)
                    longest = length;
            }

            if (Children != null)
            {
                foreach (LexNode child in Children)
                    child.GetLongest(languageIndex, ref longest);
            }
        }

        public XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Key == '\0')
                element.Add(new XAttribute("K", ""));
            else
                element.Add(new XAttribute("K", Key.ToString()));

            if (TextList != null)
                element.Add(
                    new XAttribute(
                        "T",
                        ObjectUtilities.GetStringFromStringList(TextList)));

            if (CategoryDesignationIDs != null)
                element.Add(
                    new XAttribute(
                        "D",
                        ObjectUtilities.GetStringFromIntList(CategoryDesignationIDs)));

            if (Children != null)
            {
                foreach (LexNode childNode in Children)
                    element.Add(childNode.GetElement("C"));
            }

            return element;
        }

        public void OnElement(XElement element)
        {
            ClearLexNode();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public bool OnAttribute(XAttribute attribute)
        {
            switch (attribute.Name.LocalName)
            {
                case "K":
                    {
                        string value = attribute.Value;
                        if (!String.IsNullOrEmpty(value))
                            Key = value[0];
                        else
                            Key = '\0';
                    }
                    break;
                case "T":
                    {
                        string value = attribute.Value;
                        if (!String.IsNullOrEmpty(value))
                            TextList = ObjectUtilities.GetStringListFromStringNoTrim(value);
                        else
                            TextList = null;
                    }
                    break;
                case "D":
                    {
                        string value = attribute.Value;
                        if (!String.IsNullOrEmpty(value))
                            CategoryDesignationIDs = ObjectUtilities.GetIntListFromString(value);
                        else
                            CategoryDesignationIDs = null;
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        public bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "C":
                    LexNode childNode = new LexNode(childElement);
                    if (Children == null)
                        Children = new List<LexNode>() { childNode };
                    else
                        Children.Add(childNode);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}

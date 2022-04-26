using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Markup
{
    public class MarkupTemplate : BaseObjectTitled
    {
        protected XElement _Markup;
        protected List<MultiLanguageItem> _MultiLanguageItems;
        protected List<BaseString> _Variables;
        protected int _MultiLanguageItemOrdinal;
        protected BaseObjectTitled _LocalOwningObject;  // Not saved.

        public MarkupTemplate(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                XElement markup, List<MultiLanguageItem> multiLanguageItems)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            _Markup = markup;
            _MultiLanguageItems = multiLanguageItems;
            _Variables = new List<BaseString>();
            _MultiLanguageItemOrdinal = 0;
            _LocalOwningObject = null;
        }

        public MarkupTemplate(object key, MarkupTemplate other)
            : base(other)
        {
            CopyMarkupTemplate(other);
            Key = key;
            Modified = false;
        }

        public MarkupTemplate(MarkupTemplate other)
            : base(other)
        {
            CopyMarkupTemplate(other);
            Modified = false;
        }

        public MarkupTemplate(object key)
            : base(key)
        {
            ClearMarkupTemplate();
        }

        public MarkupTemplate(XElement element)
        {
            OnElement(element);
        }

        public MarkupTemplate()
        {
            Key = 0;
            ClearMarkupTemplate();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMarkupTemplate();
        }

        public void ClearMarkupTemplate()
        {
            _Markup = new XElement("Markup", "\n");
            _MultiLanguageItems = new List<MultiLanguageItem>();
            _Variables = new List<BaseString>();
            _MultiLanguageItemOrdinal = 0;
            _LocalOwningObject = null;
        }

        public void Copy(MarkupTemplate other)
        {
            base.Copy(other);
            CopyMarkupTemplate(other);
        }

        public void CopyMarkupTemplate(MarkupTemplate other)
        {
            CopyLanguages(other);

            _Markup = new XElement(other.Markup);

            if (other.MultiLanguageItems != null)
                _MultiLanguageItems = new List<MultiLanguageItem>(other.MultiLanguageItems);
            else
                _MultiLanguageItems = new List<MultiLanguageItem>();

            _Variables = new List<BaseString>();
            _MultiLanguageItemOrdinal = other.MultiLanguageItemOrdinal;
            _LocalOwningObject = other.LocalOwningObject;
        }

        public void CopyFrom(MarkupTemplate other)
        {
            _Markup = new XElement(other.Markup);

            if (other.MultiLanguageItems != null)
                _MultiLanguageItems = new List<MultiLanguageItem>(other.MultiLanguageItems);
            else
                _MultiLanguageItems = new List<MultiLanguageItem>();

            if (_MultiLanguageItems != null)
            {
                foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                    ContentUtilities.SynchronizeMultiLanguageItemLanguages(multiLanguageItem, String.Empty, LanguageIDs);
            }

            _Variables = new List<BaseString>();
            _MultiLanguageItemOrdinal = other.MultiLanguageItemOrdinal;
            _LocalOwningObject = other.LocalOwningObject;
        }

        public override void CopyLanguages(BaseObjectLanguages other)
        {
            base.CopyLanguages(other);

            if (_MultiLanguageItems != null)
            {
                foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                {
                    ContentUtilities.SynchronizeMultiLanguageItemLanguages(multiLanguageItem, String.Empty, LanguageIDs);
                }
            }
        }

        public void CopyProfile(MarkupTemplate other)
        {
            CopyTitledObjectAndLanguages(other);
        }

        public void CopyProfileExpand(MarkupTemplate other, UserProfile userProfile)
        {
            CopyTitledObjectAndLanguagesExpand(other, userProfile);
        }

        public override IBaseObject Clone()
        {
            return new MarkupTemplate(this);
        }

        public bool IsLocal
        {
            get
            {
                if (KeyString == "(local)")
                    return true;
                return false;
            }
        }

        public BaseObjectTitled LocalOwningObject
        {
            get
            {
                return _LocalOwningObject;
            }
            set
            {
                _LocalOwningObject = value;
            }
        }

        public override string MediaTildeUrl
        {
            get
            {
                if (IsLocal)
                {
                    if (_LocalOwningObject != null)
                        return _LocalOwningObject.MediaTildeUrl + "/Markup";
                    return null;
                }

                return ApplicationData.MediaTildeUrl + "/" + MediaUtilities.FileFriendlyName(Owner) + "/" + Directory;
            }
        }

        public XElement Markup
        {
            get
            {
                return _Markup;
            }
            set
            {
                if (value != _Markup)
                {
                    _Markup = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasMarkup()
        {
            if (_Markup == null)
                return false;

            if (_Markup.Name.LocalName.ToLower() == "markup")
                return true;

            return false;
        }

        public bool HasMarkupContents()
        {
            if (_Markup == null)
                return false;

            if (_Markup.Name.LocalName.ToLower() == "markup")
                return _Markup.HasElements;
            else if (_Markup.Name.LocalName.ToLower() == "generate")
                return true;

            return false;
        }

        public XElement FindMarkupElement(string elementName, string name, int nth)
        {
            if (_Markup == null)
                return null;

            if (name == null)
                name = String.Empty;

            int index = 0;

            if ((_Markup != null) && (_Markup.Name.LocalName.ToLower() == elementName.ToLower()))
                return _Markup;

            XElement element = FindMarkupElementRecurse(
                _Markup, elementName.ToLower(), name.ToLower(), nth, ref index);

            return element;
        }

        public XElement FindMarkupElementRecurse(XElement element, string elementName,
            string name, int nth, ref int index)
        {
            if (element == null)
                return null;

            foreach (XElement childElement in element.Elements())
            {
                if (childElement.Name.LocalName.ToLower() == elementName)
                {
                    if (!String.IsNullOrEmpty(name))
                    {
                        XAttribute attribute = childElement.Attributes().FirstOrDefault(
                            x => x.Name.LocalName.ToLower() == "name");

                        if (attribute != null)
                        {
                            if (attribute.Value.ToLower() != name)
                                continue;
                        }
                    }

                    if ((nth < 0) || (index == nth))
                        return childElement;

                    index++;
                }

                XElement foundElement = FindMarkupElementRecurse(
                    childElement, elementName, name, nth, ref index);

                if (foundElement != null)
                    return foundElement;
            }

            return null;
        }

        public int GetElementNth(XElement rootElement, XElement element)
        {
            if (rootElement == null)
                return -1;

            if (element == null)
                return -1;

            string name = element.Name.LocalName.ToLower();
            int nth = 0;

            if (GetElementNth(rootElement, element, name, ref nth))
                return nth;

            return -1;
        }

        public bool GetElementNth(XElement rootElement, XElement element, string name, ref int nth)
        {
            if (rootElement.Name.LocalName.ToLower() == name)
            {
                if ((rootElement == element) || (rootElement.ToString() == element.ToString()))
                    return true;

                nth++;
            }

            foreach (XElement childElement in rootElement.Elements())
            {
                if (GetElementNth(childElement, element, name, ref nth))
                    return true;
            }

            return false;
        }

        public List<string> GetMarkers()
        {
            if (_Markup == null)
                return null;

            List<string> markers = null;

            foreach (XElement descendent in _Markup.Descendants())
            {
                if (descendent.Name.LocalName.ToLower() == "marker")
                {
                    XAttribute attribute = descendent.Attributes().FirstOrDefault(
                        x => x.Name.LocalName.ToLower() == "name");

                    if (attribute == null)
                        continue;

                    string name = attribute.Value;

                    if (markers == null)
                        markers = new List<string>();

                    markers.Add(name);
                }
            }

            return markers;
        }

        public List<MultiLanguageItem> MultiLanguageItems
        {
            get
            {
                return _MultiLanguageItems;
            }
            set
            {
                if (value != _MultiLanguageItems)
                {
                    _MultiLanguageItems = value;
                    if (_MultiLanguageItems != null)
                    {
                        if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                            _MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
                        else if (_MultiLanguageItems.Count() == 0)
                            _MultiLanguageItemOrdinal = 0;
                    }
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageItem MultiLanguageItem(string key)
        {
            MultiLanguageItem multiLanguageItem = null;
            LanguageID hostLanguageID = null;

            if (HostLanguageIDs != null)
                hostLanguageID = HostLanguageIDs.FirstOrDefault();

            if (hostLanguageID == null)
                hostLanguageID = LanguageLookup.English;

            if ((_MultiLanguageItems != null) && !String.IsNullOrEmpty(key))
            {
                multiLanguageItem = _MultiLanguageItems.FirstOrDefault(x => x.Text(hostLanguageID) == key);

                if (multiLanguageItem == null)
                    multiLanguageItem = _MultiLanguageItems.FirstOrDefault(x => x.KeyString == key);
            }

            return multiLanguageItem;
        }

        public int MultiLanguageItemIndex(MultiLanguageItem multiLanguageItem)
        {
            if ((_MultiLanguageItems != null) && (multiLanguageItem != null))
                return _MultiLanguageItems.IndexOf(multiLanguageItem);

            return -1;
        }

        public MultiLanguageItem MultiLanguageItemIndexed(int index)
        {
            if ((_MultiLanguageItems != null) && (index >= 0) && (index < _MultiLanguageItems.Count()))
                return _MultiLanguageItems.ElementAt(index);
            return null;
        }

        public string MultiLanguageItemText(string key, LanguageID languageID)
        {
            MultiLanguageItem multiLanguageItem = MultiLanguageItem(key);
            if (multiLanguageItem != null)
                return multiLanguageItem.Text(languageID);
            return null;
        }

        public string MultiLanguageItemTextIndexed(int index, LanguageID languageID)
        {
            MultiLanguageItem multiLanguageItem = MultiLanguageItemIndexed(index);
            if (multiLanguageItem != null)
                return multiLanguageItem.Text(languageID);
            return null;
        }

        public bool AddMultiLanguageItem(MultiLanguageItem multiLanguageItem)
        {
            if (_MultiLanguageItems == null)
                _MultiLanguageItems = new List<MultiLanguageItem>(1) { multiLanguageItem };
            else
                _MultiLanguageItems.Add(multiLanguageItem);
            if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                _MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
            ModifiedFlag = true;
            return true;
        }

        public bool AddMultiLanguageItems(List<MultiLanguageItem> multiLanguageItems)
        {
            if (multiLanguageItems == null)
                return true;
            if (_MultiLanguageItems == null)
                _MultiLanguageItems = new List<MultiLanguageItem>(multiLanguageItems);
            else
                _MultiLanguageItems.AddRange(multiLanguageItems);
            if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                _MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
            ModifiedFlag = true;
            return true;
        }

        public bool InsertMultiLanguageItem(int index, MultiLanguageItem multiLanguageItem)
        {
            if (_MultiLanguageItems == null)
                _MultiLanguageItems = new List<MultiLanguageItem>(1) { multiLanguageItem };
            else if ((index >= 0) && (index <= _MultiLanguageItems.Count()))
                _MultiLanguageItems.Insert(index, multiLanguageItem);
            if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                _MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
            ModifiedFlag = true;
            return true;
        }

        public bool InsertMultiLanguageItems(int index, List<MultiLanguageItem> multiLanguageItems)
        {
            if (multiLanguageItems == null)
                return true;
            if (_MultiLanguageItems == null)
                _MultiLanguageItems = new List<MultiLanguageItem>(multiLanguageItems);
            else if ((index >= 0) && (index <= _MultiLanguageItems.Count()))
                _MultiLanguageItems.InsertRange(index, multiLanguageItems);
            if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                _MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteMultiLanguageItem(MultiLanguageItem multiLanguageItem)
        {
            if (_MultiLanguageItems != null)
            {
                if (_MultiLanguageItems.Remove(multiLanguageItem))
                {
                    MultiLanguageItemOrdinalCheck();
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteMultiLanguageItemKey(string key)
        {
            if ((_MultiLanguageItems != null) && !String.IsNullOrEmpty(key))
            {
                MultiLanguageItem multiLanguageItem = MultiLanguageItem(key);
                if (multiLanguageItem != null)
                {
                    _MultiLanguageItems.Remove(multiLanguageItem);
                    MultiLanguageItemOrdinalCheck();
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteMultiLanguageItemIndexed(int index)
        {
            if ((_MultiLanguageItems != null) && (index >= 0) && (index < _MultiLanguageItems.Count()))
            {
                _MultiLanguageItems.RemoveAt(index);
                MultiLanguageItemOrdinalCheck();
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllMultiLanguageItems()
        {
            if (_MultiLanguageItems != null)
                ModifiedFlag = true;
            _MultiLanguageItems = null;
            MultiLanguageItemOrdinalCheck();
        }

        public bool CopyMultiLanguageItemsSelected(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int studyItemCount = MultiLanguageItemCount();
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = MultiLanguageItemIndexed(studyItemIndex);

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                studyItems.Add(studyItem);
            }

            return returnValue;
        }

        public bool CutMultiLanguageItemsSelected(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int studyItemCount = MultiLanguageItemCount();
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = MultiLanguageItemIndexed(studyItemIndex);

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                studyItems.Add(studyItem);

                _MultiLanguageItems.RemoveAt(studyItemIndex);
                ContentUtilities.DeleteSelectFlags(itemSelectFlags, studyItemIndex, 1);
                studyItemCount--;
                studyItemIndex--;
                ModifiedFlag = true;
            }

            MultiLanguageItemOrdinalCheck();

            return returnValue;
        }

        public bool PasteMultiLanguageItemsPrepend(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            ContentUtilities.InsertSelectFlags(itemSelectFlags, 0, studyItems.Count(), true);
            return InsertMultiLanguageItems(0, studyItems);
        }

        public bool PasteMultiLanguageItemsInsertBefore(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int index = 0;

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.IndexOf(true);

                if (index < 0)
                    index = 0;
            }

            ContentUtilities.InsertSelectFlags(itemSelectFlags, index, studyItems.Count(), true);

            return InsertMultiLanguageItems(index, studyItems);
        }

        public bool PasteMultiLanguageItemsOverwrite(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int studyItemIndex;
            int studyItemCount = MultiLanguageItemCount();
            int sourceIndex = 0;
            MultiLanguageItem studyItem;
            bool returnValue = true;

            if ((studyItems == null) || (studyItems.Count() == 0))
                return true;

            for (studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                if (sourceIndex < studyItems.Count())
                    studyItem = studyItems[sourceIndex];
                else
                    studyItem = null;

                if (studyItem != null)
                    _MultiLanguageItems[studyItemIndex] = studyItem;
                else
                {
                    _MultiLanguageItems.RemoveAt(studyItemIndex);
                    ContentUtilities.DeleteSelectFlags(itemSelectFlags, studyItemIndex, 1);
                    studyItemCount--;
                    studyItemIndex--;
                }

                sourceIndex++;
                ModifiedFlag = true;
            }

            studyItemIndex = itemSelectFlags.LastIndexOf(true);

            if (studyItemIndex < 0)
                studyItemIndex = MultiLanguageItemCount();
            else
                studyItemIndex++;

            for (; sourceIndex < studyItems.Count(); sourceIndex++)
            {
                studyItem = studyItems[sourceIndex];
                InsertMultiLanguageItem(studyItemIndex, studyItem);
                ContentUtilities.InsertSelectFlags(itemSelectFlags, studyItemIndex, 1, true);
                studyItemIndex++;
            }

            MultiLanguageItemOrdinalCheck();

            return returnValue;
        }

        public bool PasteMultiLanguageItemsInsertAfter(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int index = MultiLanguageItemCount();

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.LastIndexOf(true);

                if (index < 0)
                    index = MultiLanguageItemCount();
                else
                    index++;
            }

            ContentUtilities.InsertSelectFlags(itemSelectFlags, index, studyItems.Count(), true);

            return InsertMultiLanguageItems(index, studyItems);
        }

        public bool PasteMultiLanguageItemsAppend(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            bool returnValue;
            ContentUtilities.InsertSelectFlags(itemSelectFlags, MultiLanguageItemCount(), studyItems.Count(), true);
            returnValue = AddMultiLanguageItems(studyItems);
            return returnValue;
        }

        public int MultiLanguageItemCount()
        {
            if (_MultiLanguageItems != null)
                return (_MultiLanguageItems.Count());
            return 0;
        }

        public int MultiLanguageItemOrdinal
        {
            get
            {
                if (_MultiLanguageItems != null)
                {
                    if (_MultiLanguageItemOrdinal < _MultiLanguageItems.Count())
                        MultiLanguageItemOrdinal = _MultiLanguageItems.Count();
                }
                return _MultiLanguageItemOrdinal;
            }
            set
            {
                if (value != _MultiLanguageItemOrdinal)
                {
                    _MultiLanguageItemOrdinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int GetAndIncrementMultiLanguageItemOrdinal()
        {
            int value = MultiLanguageItemOrdinal;
            MultiLanguageItemOrdinal = value + 1;
            return value;
        }

        public string AllocateMultiLanguageItemKey()
        {
            int ordinal = GetAndIncrementMultiLanguageItemOrdinal();
            string value = "Markup" + ordinal.ToString();
            return value;
        }

        public void MultiLanguageItemOrdinalCheck()
        {
            if ((_MultiLanguageItems == null) || (_MultiLanguageItems.Count() == 0))
                _MultiLanguageItemOrdinal = 0;
        }

        public bool HasMedia()
        {
            if (_MultiLanguageItems != null)
            {
                foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                {
                    if (multiLanguageItem.HasAnyMediaRun())
                        return true;
                }
            }

            return false;
        }

        public List<BaseString> Variables
        {
            get
            {
                return _Variables;
            }
            set
            {
                _Variables = value;
            }
        }

        public BaseString GetVariable(string key)
        {
            if (!String.IsNullOrEmpty(key))
                return _Variables.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public BaseString GetVariableIndexed(int index)
        {
            if ((index >= 0) && (index < _Variables.Count()))
                return _Variables.ElementAt(index);
            return null;
        }

        public string GetVariableText(string key)
        {
            BaseString variable = GetVariable(key);
            if (variable != null)
                return variable.Text;
            return null;
        }

        public string VariableTextIndexed(int index, LanguageID languageID)
        {
            BaseString variable = GetVariableIndexed(index);
            if (variable != null)
                return variable.Text;
            return null;
        }

        public bool AddVariable(BaseString variable)
        {
            if (_Variables == null)
                _Variables = new List<BaseString>(1) { variable };
            else
                _Variables.Add(variable);
            return true;
        }

        public bool DeleteVariable(BaseString variable)
        {
            if (_Variables.Remove(variable))
                return true;
            return false;
        }

        public bool DeleteVariableKey(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                BaseString variable = GetVariable(key);
                if (variable != null)
                {
                    _Variables.Remove(variable);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteVariableIndexed(int index)
        {
            if ((index >= 0) && (index < _Variables.Count()))
            {
                _Variables.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void DeleteAllVariables()
        {
            _Variables.Clear();
        }

        public int VariableCount()
        {
            return _Variables.Count();
        }

        public void ScanMarkupForStrings(LanguageID uiLanguageID)
        {
            ScanElement(_Markup, uiLanguageID);
        }

        public void ScanNode(XNode node, LanguageID uiLanguageID)
        {
            switch (node.GetType().Name)
            {
                case "XText":
                    ScanText(node as XText, uiLanguageID);
                    break;
                case "XElement":
                    ScanElement(node as XElement, uiLanguageID);
                    break;
                default:
                    break;
            }
        }

        public void ScanElement(XElement element, LanguageID languageID)
        {
            switch (element.Name.LocalName.ToLower())
            {
                case "insert":
                case "if":
                case "foreach":
                case "for":
                    ScanElementForName(element, languageID);
                    break;
                case "style":
                    break;
                default:
                    foreach (XAttribute attribute in element.Attributes())
                        ScanAttribute(attribute, languageID);
                    ScanChildNodes(element, languageID);
                    break;
            }
        }

        public void ScanChildNodes(XElement element, LanguageID languageID)
        {
            IEnumerable<XNode> nodes = element.Nodes();

            if (nodes != null)
            {
                int count = nodes.Count();

                for (int i = 0; i < count; i++)
                {
                    XNode node = nodes.ElementAt(i);

                    switch (node.GetType().Name)
                    {
                        case "XText":
                            ScanText(node as XText, languageID);
                            break;
                        case "XElement":
                            ScanElement(node as XElement, languageID);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        protected void ScanText(XText node, LanguageID languageID)
        {
            string input = node.Value;

            if (!input.Contains("$(") && !input.Contains("{"))
                return;

            int count = input.Length;
            int index;
            string variableText = null;
            string variableName = null;
            MultiLanguageItem multiLanguageItem;

            for (index = 0; index < count; index++)
            {
                if ((input[index] == '\\') && ((input[index + 1] == '{') || ((input[index + 1] == '$') && (input[index + 2] == '('))))
                    continue;

                if (input[index] == '{')
                {
                    int i = index + 1;
                    int e = i;

                    while (e < count)
                    {
                        if (input[e] == '}')
                        {
                            variableText = input.Substring(i, e - i);

                            variableName = FilterVariableName(variableText);

                            multiLanguageItem = MultiLanguageItem(variableName);

                            if (multiLanguageItem == null)
                            {
                                BaseString variable = GetVariable(variableName);

                                if (variable == null)
                                {
                                    multiLanguageItem = new MultiLanguageItem(variableName, languageID, variableText);
                                    AddMultiLanguageItem(multiLanguageItem);
                                }
                            }

                            break;
                        }

                        e++;
                    }

                    index = e;
                }
            }
        }

        protected void ScanElementForName(XElement element, LanguageID languageID)
        {
            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                ScanAttributeExpression(attributeValue, languageID);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        AddVariable(new BaseString(attributeValue, ""));
                        break;
                    default:
                        break;
                }
            }

            ScanChildNodes(element, languageID);
        }

        protected void ScanAttribute(XAttribute attribute, LanguageID languageID)
        {
            ScanAttributeExpression(attribute.Value.Trim(), languageID);
        }

        protected void ScanAttributeExpression(string input, LanguageID languageID)
        {
            if (!input.Contains("$(") && !input.Contains("{"))
                return;

            int count = input.Length;
            int index;
            string variableText = null;
            string variableName = null;
            MultiLanguageItem multiLanguageItem;

            for (index = 0; index < count; index++)
            {
                if ((input[index] == '\\') && ((input[index + 1] == '{') || ((input[index + 1] == '$') && (input[index + 2] == '('))))
                    continue;

                if (input[index] == '{')
                {
                    int i = index + 1;
                    int e = i;

                    while (e < count)
                    {
                        if (input[e] == '}')
                        {
                            variableText = input.Substring(i, e - i);

                            variableName = FilterVariableName(variableText);

                            multiLanguageItem = MultiLanguageItem(variableName);

                            if (multiLanguageItem == null)
                            {
                                BaseString variable = GetVariable(variableName);

                                if (variable == null)
                                {
                                    multiLanguageItem = new MultiLanguageItem(variableName, languageID, variableText);
                                    AddMultiLanguageItem(multiLanguageItem);
                                }
                            }

                            break;
                        }

                        e++;
                    }

                    index = e;
                }
            }
        }

        public string FilterVariableName(string name)
        {
            return MediaUtilities.FilterVariableName(name);
        }

        public void CollectMediaFiles(Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            if (_MultiLanguageItems != null)
            {
                string mediaTildeUrl = MediaTildeUrl;

                foreach (MultiLanguageItem studyItem in _MultiLanguageItems)
                {
                    bool useIt = true;

                    if (itemSelectFlags != null)
                    {
                        if (!itemSelectFlags.TryGetValue(studyItem.KeyString, out useIt))
                            useIt = true;
                    }

                    if (useIt)
                        studyItem.CollectMediaFiles(mediaTildeUrl, languageSelectFlags, mediaFiles, this, visitFunction);
                }
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            CollectMediaFiles(itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_MultiLanguageItems != null)
                {
                    foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                    {
                        if (multiLanguageItem.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_MultiLanguageItems != null)
                {
                    foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                        multiLanguageItem.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_MultiLanguageItemOrdinal != 0)
                element.Add(new XAttribute("MultiLanguageItemOrdinal", _MultiLanguageItemOrdinal));

            if (_Markup != null)
                element.Add(new XElement(_Markup));

            if (_MultiLanguageItems != null)
            {
                foreach (MultiLanguageItem multiLanguageItem in _MultiLanguageItems)
                    element.Add(multiLanguageItem.Xml);
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "MultiLanguageItemOrdinal":
                    _MultiLanguageItemOrdinal = Convert.ToInt32(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName.ToLower())
            {
                case "markup":
                case "generate":
                case "html":
                case "body":
                    _Markup = new XElement(childElement);
                    break;
                case "multilanguageitem":
                    MultiLanguageItem multiLanguageItem = new MultiLanguageItem(childElement);
                    AddMultiLanguageItem(multiLanguageItem);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            MarkupTemplate otherMarkupTemplate = other as MarkupTemplate;

            if (otherMarkupTemplate == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_Markup.ToString(), otherMarkupTemplate.Markup.ToString());

            if (diff != 0)
                return diff;

            if (_MultiLanguageItems.Count() != otherMarkupTemplate.MultiLanguageItems.Count())
                return _MultiLanguageItems.Count() - otherMarkupTemplate.MultiLanguageItems.Count();

            int count = _MultiLanguageItems.Count();

            for (int i = 0; i < count; i++)
            {
                diff = JTLanguageModelsPortable.Content.MultiLanguageItem.CompareMultiLanguageItem(_MultiLanguageItems[i], otherMarkupTemplate.MultiLanguageItemIndexed(i));

                if (diff != 0)
                    return diff;
            }

            return diff;
        }

        public static int Compare(MarkupTemplate object1, MarkupTemplate object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;

            if ((object)object1 == null)
                return -1;

            if ((object)object2 == null)
                return 1;

            return object1.Compare(object2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class InflectionsLayout : BaseObjectLanguages
    {
        protected string _Type;
        protected string _Layout;
        protected bool _UseHost;
        protected bool _UseAudio;
        protected bool _UsePronouns;
        protected bool _CanTogglePronouns;
        protected bool _UseLabelBorders;
        protected bool _UseDataBorders;
        protected List<InflectionsLayoutGroup> _Groups;
        // The following are not in the storage.
        protected List<Designator> _Designators;
        protected List<BaseString> _ClassifierKeys;
        protected List<BaseString> _ClassifierValues;
        protected Dictionary<string, List<BaseString>> _ClassifierKeyValueDictionary;

        public InflectionsLayout(
                string key,
                List<LanguageID> languageIDs,
                string type,
                string layout,
                bool usePronouns) :
            base(key, languageIDs, null, null)
        {
            ClearInflectionsLayout();
            _Type = type;
            _Layout = layout;
            _UsePronouns = usePronouns;
        }

    public InflectionsLayout(XElement element)
        {
            ClearInflectionsLayout();
            OnElement(element);
        }

        public InflectionsLayout(InflectionsLayout other) : base(other)
        {
            CopyInflectionsLayout(other);
        }

        public InflectionsLayout()
        {
            ClearInflectionsLayout();
        }

        public void ClearInflectionsLayout()
        {
            _Type = String.Empty;
            _Layout = String.Empty;
            _UseHost = true;
            _UseAudio = true;
            _UsePronouns = false;
            _CanTogglePronouns = true;
            _UseLabelBorders = true;
            _UseDataBorders = true;
            _Groups = null;
            _Designators = null;
            _ClassifierKeys = null;
            _ClassifierValues = null;
            _ClassifierKeyValueDictionary = null;
        }

        public void CopyInflectionsLayout(InflectionsLayout other)
        {
            _Type = other.Type;
            _Layout = other.Layout;
            _UseHost = other.UseHost;
            _UseAudio = other.UseAudio;
            _UsePronouns = other.UsePronouns;
            _CanTogglePronouns = other.CanTogglePronouns;
            _UseLabelBorders = other.UseLabelBorders;
            _UseDataBorders = other.UseDataBorders;
            _Groups = CloneGroups();
            _Designators = other.Designators;
            _ClassifierKeys = other.ClassifierKeys;
            _ClassifierValues = other.ClassifierValues;
            _ClassifierKeyValueDictionary = other.ClassifierKeyValueDictionary;
        }

        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Layout
        {
            get
            {
                return _Layout;
            }
            set
            {
                if (value != _Layout)
                {
                    _Layout = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UseHost
        {
            get
            {
                return _UseHost;
            }
            set
            {
                if (value != _UseHost)
                {
                    _UseHost = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UseAudio
        {
            get
            {
                return _UseAudio;
            }
            set
            {
                if (value != _UseAudio)
                {
                    _UseAudio = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UsePronouns
        {
            get
            {
                return _UsePronouns;
            }
            set
            {
                if (value != _UsePronouns)
                {
                    _UsePronouns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool CanTogglePronouns
        {
            get
            {
                return _CanTogglePronouns;
            }
            set
            {
                if (value != _CanTogglePronouns)
                {
                    _CanTogglePronouns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UseLabelBorders
        {
            get
            {
                return _UseLabelBorders;
            }
            set
            {
                if (value != _UseLabelBorders)
                {
                    _UseLabelBorders = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UseDataBorders
        {
            get
            {
                return _UseDataBorders;
            }
            set
            {
                if (value != _UseDataBorders)
                {
                    _UseDataBorders = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Designator> Designators
        {
            get
            {
                return _Designators;
            }
            set
            {
                _Designators = value;
            }
        }

        public List<BaseString> ClassifierKeys
        {
            get
            {
                return _ClassifierKeys;
            }
            set
            {
                _ClassifierKeys = value;
            }
        }

        public List<BaseString> ClassifierValues
        {
            get
            {
                return _ClassifierValues;
            }
            set
            {
                _ClassifierValues = value;
            }
        }

        public Dictionary<string, List<BaseString>> ClassifierKeyValueDictionary
        {
            get
            {
                return _ClassifierKeyValueDictionary;
            }
            set
            {
                _ClassifierKeyValueDictionary = value;
            }
        }


        public List<InflectionsLayoutGroup> Groups
        {
            get
            {
                return _Groups;
            }
            set
            {
                if (value != _Groups)
                {
                    _Groups = value;
                    ModifiedFlag = true;
                }
            }
        }

        public InflectionsLayoutGroup GetGroupIndexed(int index)
        {
            if (_Groups == null)
                return null;

            if ((index >= 0) && (index < _Groups.Count()))
                return _Groups[index];

            return null;
        }

        public void AppendGroup(InflectionsLayoutGroup group)
        {
            if (_Groups == null)
                _Groups = new List<InflectionsLayoutGroup>() { group };
            else
                _Groups.Add(group);
        }

        public List<InflectionsLayoutGroup> CloneGroups()
        {
            if (_Groups == null)
                return null;

            List<InflectionsLayoutGroup> groups = new List<InflectionsLayoutGroup>(_Groups.Count());

            foreach (InflectionsLayoutGroup group in _Groups)
                groups.Add(new InflectionsLayoutGroup(group));

            return groups;
        }

        public static string InflectionsLayoutBreakpoint = null;

        public Dictionary<string, List<Inflection>[,]> GetInflectionTableData(
            InflectionsLayout layout,
            List<Inflection> inflections)
        {
            Dictionary<string, List<Inflection>[,]> inflectionTableData = new Dictionary<string, List<Inflection>[,]>();
            List<InflectionsLayoutGroup> layoutGroups = new List<InflectionsLayoutGroup>();
            int dataRowCount, dataColumnCount;
            int dataRowIndex, dataColumnIndex;
            int rowCount, columnCount;
            int rowIndex, columnIndex;
            int groupColumnCount = 0;

            if (layout.Groups == null)
                return inflectionTableData;

            foreach (InflectionsLayoutGroup group in layout.Groups)
            {
                Designator groupDesignator = new Designator(null, group.CloneClassifications());
                List<InflectionsLayoutGroup> subGroups = group.SubGroups;

                if (subGroups == null)
                    subGroups = new List<InflectionsLayoutGroup>(1) { group };

                foreach (InflectionsLayoutGroup layoutGroup in subGroups)
                {
                    Designator subGroupDesignator = new Designator(groupDesignator);
                    string groupName = layoutGroup.GroupKey(layoutGroup != group ? group : null);

                    if (layoutGroup != group)
                    {
                        subGroupDesignator.CopyAndAppendUniqueClassifications(layoutGroup.Classifications);
                        subGroupDesignator.DefaultLabel();
                    }

                    InflectionsLayoutGroup dataGroup;

                    if (layoutGroup.RowCount() != 0)
                        dataGroup = layoutGroup;
                    else
                        dataGroup = group;

                    dataGroup.GetDataDimensions(out dataRowCount, out dataColumnCount);

                    List<Inflection>[,] groupData = new List<Inflection>[dataRowCount, dataColumnCount];

                    rowCount = dataGroup.RowCount();
                    dataRowIndex = 0;
                    groupColumnCount = dataGroup.ColumnCount();

                    for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        InflectionsLayoutRow layoutRow = dataGroup.GetRowIndexed(rowIndex);

                        if (layoutRow.Name == "ColumnHeadings")
                            continue;

                        Designator rowDesignator = new Designator(subGroupDesignator);

                        rowDesignator.CopyAndAppendUniqueClassifications(layoutRow.Classifications);
                        rowDesignator.DefaultLabel();

                        if (groupColumnCount != 0)
                        {
                            columnCount = groupColumnCount;
                            dataColumnIndex = 0;

                            for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
                            {
                                InflectionsLayoutColumn layoutColumn = layoutGroup.GetColumnIndexed(columnIndex);

                                Designator columnDesignator = new Designator(rowDesignator);

                                if (layoutColumn.ClassificationCount() != 0)
                                {
                                    if (layoutColumn.GetClassificationIndexed(0).Name != "None")
                                    {
                                        columnDesignator = new Designator(rowDesignator);
                                        columnDesignator.CopyAndAppendUniqueClassifications(layoutColumn.Classifications);
                                        columnDesignator.DefaultLabel();
                                    }
                                    else
                                        columnDesignator = new Designator("None", new List<Classifier>());
                                }
                                else
                                    columnDesignator = new Designator("None", new List<Classifier>());

                                List<Inflection> inflectionList = null;
                                string cellID = String.Empty;
                                bool isNone = (columnDesignator.Label == "None") || (layoutColumn.Designation.Label == "None");

                                if (!isNone)
                                {
                                    IEnumerable<Inflection> inflectionsResult = inflections.Where(x => x.Designation.MatchOrAlternate(columnDesignator));

                                    if ((inflectionsResult == null) || (inflectionsResult.Count() == 0))
                                        inflectionsResult = inflections.Where(x => x.Designation.MatchOrAlternate(layoutColumn.Designation));

                                    if (inflectionsResult != null)
                                    {
                                        inflectionList = inflectionsResult.ToList();

                                        int c = inflectionList.Count();
                                        int i;

                                        for (i = c - 1; i >= 0; i--)
                                        {
                                            Inflection inflection = inflectionList[i];

                                            if ((inflection != null) &&
                                                    ((inflection.Output == null) || !inflection.Output.HasText(FirstTargetLanguageID)))
                                            {
                                                inflectionList.RemoveAt(i);
                                            }

                                            if ((InflectionsLayoutBreakpoint != null) &&
                                                    (inflection != null) &&
                                                    inflection.Label.StartsWith(InflectionsLayoutBreakpoint))
                                                ApplicationData.Global.PutConsoleMessage(inflection.Label);
                                        }

                                        if (inflectionList.Count() == 0)
                                            inflectionList = null;
                                    }

                                    /*
                                    inflection = inflections.FirstOrDefault(x => x.Designation.MatchIntersect(columnDesignator));

                                    if (inflection == null)
                                        inflection = inflections.FirstOrDefault(x => x.Designation.MatchIntersect(layoutColumn.Designation));

                                    if ((inflection != null) &&
                                            ((inflection.Output == null) || !inflection.Output.HasText(FirstTargetLanguageID)))
                                        inflection = null;
                                    */
                                }

                                groupData[dataRowIndex, dataColumnIndex] = inflectionList;

                                dataColumnIndex++;
                            }
                        }
                        else
                        {
                            columnCount = layoutRow.CellCount();
                            dataColumnIndex = 0;

                            for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
                            {
                                InflectionsLayoutCell layoutCell = layoutRow.GetCellIndexed(columnIndex);

                                if (layoutCell.HeadingsCount() != 0)
                                    continue;

                                Designator cellDesignator = new Designator(rowDesignator);

                                if (layoutCell.ClassificationCount() != 0)
                                {
                                    if (layoutCell.GetClassificationIndexed(0).Name != "None")
                                    {
                                        cellDesignator = new Designator(rowDesignator);
                                        cellDesignator.CopyAndAppendUniqueClassifications(layoutCell.Classifications);
                                        cellDesignator.DefaultLabel();
                                    }
                                    else
                                        cellDesignator = new Designator("None", new List<Classifier>());
                                }
                                else
                                    cellDesignator = new Designator("None", new List<Classifier>());

                                List<Inflection> inflectionList = null;
                                string cellID = String.Empty;
                                bool isNone = (cellDesignator.Label == "None") || (layoutCell.Designation.Label == "None");

                                if (!isNone)
                                {
                                    IEnumerable<Inflection> inflectionsResult = inflections.Where(x => x.Designation.MatchOrAlternate(cellDesignator));

                                    if ((inflectionsResult == null) || (inflectionsResult.Count() == 0))
                                        inflectionsResult = inflections.Where(x => x.Designation.MatchOrAlternate(layoutCell.Designation));

                                    if (inflectionsResult != null)
                                    {
                                        inflectionList = inflectionsResult.ToList();

                                        int c = inflectionList.Count();
                                        int i;

                                        for (i = c - 1; i >= 0; i--)
                                        {
                                            Inflection inflection = inflectionList[i];

                                            if ((inflection != null) &&
                                                    ((inflection.Output == null) || !inflection.Output.HasText(FirstTargetLanguageID)))
                                                inflectionList.RemoveAt(i);

                                            if ((InflectionsLayoutBreakpoint != null) &&
                                                    (inflection != null) &&
                                                    inflection.Label.StartsWith(InflectionsLayoutBreakpoint))
                                                ApplicationData.Global.PutConsoleMessage(inflection.Label);
                                        }

                                        if (inflectionList.Count() == 0)
                                            inflectionList = null;
                                    }

                                    /*
                                    inflection = inflections.FirstOrDefault(x => x.Designation.MatchIntersect(cellDesignator));

                                    if (inflection == null)
                                        inflection = inflections.FirstOrDefault(x => x.Designation.MatchIntersect(layoutCell.Designation));

                                    if ((inflection != null) &&
                                            ((inflection.Output == null) || !inflection.Output.HasText(FirstTargetLanguageID)))
                                        inflection = null;
                                    */
                                }

                                groupData[dataRowIndex, dataColumnIndex] = inflectionList;

                                dataColumnIndex++;
                            }
                        }

                        dataRowIndex++;
                    }

                    inflectionTableData.Add(groupName, groupData);
                }
            }

            return inflectionTableData;
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                case DisplayDetail.Full:
                    DisplayLabel(label, indent);
                    DisplayField("Type", _Type, indent + 1);
                    DisplayField("Layout", _Layout, indent + 1);
                    DisplayField("UseHost", _UseHost.ToString(), indent + 1);
                    DisplayField("UseAudio", _UseAudio.ToString(), indent + 1);
                    DisplayField("UsePronouns", _UsePronouns.ToString(), indent + 1);
                    DisplayField("CanTogglePronouns", _CanTogglePronouns.ToString(), indent + 1);
                    DisplayField("UseLabelBorders", _UseLabelBorders.ToString(), indent + 1);
                    DisplayField("UseDataBorders", _UseDataBorders.ToString(), indent + 1);
                    if (_Groups != null)
                    {
                        foreach (InflectionsLayoutGroup group in _Groups)
                            DisplayFieldObject("Group", group, indent + 1);
                    }
                    else
                        DisplayMessage("(no groups)", indent + 1);
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Xml:
                    base.Display(label, detail, indent);
                    break;
                default:
                    break;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Key != null)
                element.Add(new XAttribute("Name", KeyString));

            string targetLanguagesKey = TargetLanguagesKey;

            if (!String.IsNullOrEmpty(targetLanguagesKey))
                element.Add(new XAttribute("LanguageIDs", targetLanguagesKey));

            if (!String.IsNullOrEmpty(_Type))
                element.Add(new XAttribute("Type", _Type));

            if (!String.IsNullOrEmpty(_Layout))
                element.Add(new XAttribute("Layout", _Layout));

            element.Add(new XElement("UseHost", _UseHost.ToString()));
            element.Add(new XElement("UseAudio", _UseAudio.ToString()));
            element.Add(new XElement("UsePronouns", _UsePronouns.ToString()));
            element.Add(new XElement("CanTogglePronouns", _CanTogglePronouns.ToString()));
            element.Add(new XElement("UseLabelBorders", _UseLabelBorders.ToString()));
            element.Add(new XElement("UseDataBorders", _UseDataBorders.ToString()));

            if (_Groups != null)
            {
                foreach (InflectionsLayoutGroup group in _Groups)
                {
                    XElement groupElement = group.GetElement("Group");
                    element.Add(groupElement);
                }
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Key = attributeValue;
                    break;
                case "LanguageIDs":
                    TargetLanguagesKey = attributeValue;
                    break;
                case "Type":
                    _Type = attributeValue;
                    break;
                case "Layout":
                    _Layout = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "UseHost":
                    _UseHost = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "UseAudio":
                    _UseAudio = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "UsePronouns":
                    _UsePronouns = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "CanTogglePronouns":
                    _CanTogglePronouns = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "UseLabelBorders":
                    _UseLabelBorders = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "UseDataBorders":
                    _UseDataBorders = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "Group":
                    {
                        InflectionsLayoutGroup group = new InflectionsLayoutGroup(childElement);
                        AppendGroup(group);
                    }
                    break;
                default:
                    throw new Exception("Unexpected child element in InflectionLayout: " + childElement.Name.LocalName);
            }

            return true;
        }
    }
}

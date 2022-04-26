using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tool
{
    public class ToolStudyItem : BaseObjectKeyed
    {
        protected string _ToolStudyListKey;
        protected ToolStudyList _ToolStudyList;                 // Not owned, and doesn't affect modified state.
        protected int _ContentStudyListKey;
        protected string _ContentStudyItemKey;
        protected MultiLanguageItem _StudyItem;                 // Not owned, and doesn't affect modified state, except for keys.
        protected List<ToolItemStatus> _StudyItemStatus;
        protected UserRunItem _UserRunItem;                     // Not owned, and doesn't affect modified state.
        protected bool _IsStudyItemHidden;
        protected MultiLanguageItem _InflectionStudyItem;
        protected Designator _Designation;
        protected LexicalCategory _Category;
        protected string _CategoryString;

        public ToolStudyItem(
                object key,
                ToolStudyList toolStudyList,
                MultiLanguageItem studyItem,
                UserRunItem userRunItem,
                List<ToolItemStatus> studyItemStatus)
            : base(key)
        {
            _ToolStudyList = toolStudyList;

            if (toolStudyList != null)
                _ToolStudyListKey = toolStudyList.KeyString;
            else
                _ToolStudyListKey = null;

            if (studyItem != null)
            {
                if (studyItem.StudyList != null)
                    _ContentStudyListKey = studyItem.StudyList.KeyInt;

                _ContentStudyItemKey = studyItem.KeyString;
            }
            else
            {
                _ContentStudyListKey = 0;
                _ContentStudyItemKey = String.Empty;
            }

            _StudyItem = studyItem;
            _StudyItemStatus = studyItemStatus;

            if (_StudyItemStatus == null)
                _StudyItemStatus = new List<ToolItemStatus>();

            _UserRunItem = userRunItem;
            _IsStudyItemHidden = false;
            _InflectionStudyItem = null;
            _Designation = null;
            _Category = LexicalCategory.Unknown;
            _CategoryString = null;
    }

    public ToolStudyItem(object key, MultiLanguageItem studyItem, List<ToolItemStatus> studyItemStatus)
            : base(key)
        {
            _ToolStudyList = null;
            _ToolStudyListKey = null;

            if (studyItem != null)
            {
                if (studyItem.StudyList != null)
                    _ContentStudyListKey = studyItem.StudyList.KeyInt;

                _ContentStudyItemKey = studyItem.KeyString;
            }
            else
            {
                _ContentStudyListKey = 0;
                _ContentStudyItemKey = String.Empty;
            }

            _StudyItem = studyItem;
            _StudyItemStatus = studyItemStatus;

            if (_StudyItemStatus == null)
                _StudyItemStatus = new List<ToolItemStatus>();

            _UserRunItem = null;
            _IsStudyItemHidden = false;
            _InflectionStudyItem = null;
            _Designation = null;
            _Category = LexicalCategory.Unknown;
            _CategoryString = null;
        }

        public ToolStudyItem(object key, MultiLanguageItem studyItem)
            : base(key)
        {
            _ToolStudyList = null;
            _ToolStudyListKey = null;

            if (studyItem != null)
            {
                if (studyItem.StudyList != null)
                    _ContentStudyListKey = studyItem.StudyList.KeyInt;

                _ContentStudyItemKey = studyItem.KeyString;
            }
            else
            {
                _ContentStudyListKey = 0;
                _ContentStudyItemKey = String.Empty;
            }

            _StudyItem = studyItem;
            _StudyItemStatus = new List<ToolItemStatus>();
            _UserRunItem = null;
            _IsStudyItemHidden = false;
            _InflectionStudyItem = null;
            _Designation = null;
            _Category = LexicalCategory.Unknown;
            _CategoryString = null;
        }

        public ToolStudyItem(ToolStudyItem other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolStudyItem(object key, ToolStudyItem other)
            : base(key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolStudyItem(XElement element)
        {
            OnElement(element);
        }

        public ToolStudyItem()
        {
            ClearToolStudyItem();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolStudyItem();
        }

        public void ClearToolStudyItem()
        {
            _ToolStudyListKey = null;
            _ToolStudyList = null;
            _ContentStudyListKey = 0;
            _ContentStudyItemKey = String.Empty;
            _StudyItem = null;
            _StudyItemStatus = new List<ToolItemStatus>();
            _UserRunItem = null;
            _IsStudyItemHidden = false;
            _InflectionStudyItem = null;
            _Designation = null;
            _Category = LexicalCategory.Unknown;
            _CategoryString = null;
        }

        public void Copy(ToolStudyItem other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            _ToolStudyList = other.ToolStudyList;
            _ToolStudyListKey = other.ToolStudyListKey;
            _ContentStudyListKey = other.ContentStudyListKey;
            _ContentStudyItemKey = other.ContentStudyItemKey;
            _StudyItem = other.StudyItem;

            if (other.StudyItemStatus != null)
                _StudyItemStatus = other.CloneStudyItemStatus();
            else
                _StudyItemStatus = new List<ToolItemStatus>();

            _UserRunItem = other.UserRunItem;
            _IsStudyItemHidden = other.IsStudyItemHidden;
            _InflectionStudyItem = null;

            if (other.InflectionStudyItem != null)
                _InflectionStudyItem = other.CloneInflectionStudyItem();
            else
                _InflectionStudyItem = null;

            _Designation = other.Designation;
            _Category = other.Category;
            _CategoryString = other.CategoryString;
        }

        public List<ToolItemStatus> CloneStudyItemStatus()
        {
            List<ToolItemStatus> studyItemStatusList = new List<ToolItemStatus>(_StudyItemStatus.Count());

            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                studyItemStatusList.Add(new ToolItemStatus(studyItemStatus));

            return studyItemStatusList;
        }

        public MultiLanguageItem CloneInflectionStudyItem()
        {
            MultiLanguageItem studyItem;

            if (_InflectionStudyItem != null)
                studyItem = new MultiLanguageItem(_InflectionStudyItem);
            else
                studyItem = null;

            return studyItem;
        }

        public ToolStudyList ToolStudyList
        {
            get
            {
                return _ToolStudyList;
            }
            set
            {
                _ToolStudyList = value;

                if (value != null)
                {
                    if (_ToolStudyListKey != value.KeyString)
                        ToolStudyListKey = value.KeyString;
                }
                else if (!String.IsNullOrEmpty(_ToolStudyListKey))
                    ToolStudyListKey = null;
            }
        }

        public string ToolStudyListKey
        {
            get
            {
                return _ToolStudyListKey;
            }
            set
            {
                if (_ToolStudyListKey != value)
                {
                    _ToolStudyListKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ContentStudyListKey
        {
            get
            {
                return _ContentStudyListKey;
            }
            set
            {
                if (_ContentStudyListKey != value)
                {
                    _ContentStudyListKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ContentStudyItemKey
        {
            get
            {
                return _ContentStudyItemKey;
            }
            set
            {
                if (_ContentStudyItemKey != value)
                {
                    _ContentStudyItemKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public UserRunItem UserRunItem
        {
            get
            {
                return _UserRunItem;
            }
            set
            {
                _UserRunItem = value;
            }
        }

        public BaseObjectContent Content
        {
            get
            {
                if (StudyItem == null)
                    return null;

                return _StudyItem.Content;
            }
        }

        public MultiLanguageItem StudyItem
        {
            get
            {
                return _StudyItem;
            }
            set
            {
                _StudyItem = value;

                if (_StudyItem != null)
                {
                    ContentStudyItemKey = _StudyItem.KeyString;

                    if (_StudyItem.StudyList != null)
                        ContentStudyListKey = _StudyItem.StudyList.KeyInt;
                }
            }
        }

        public List<ToolItemStatus> StudyItemStatus
        {
            get
            {
                return _StudyItemStatus;
            }
            set
            {
                if (value == null)
                    value = new List<ToolItemStatus>();

                if (value != _StudyItemStatus)
                {
                    _StudyItemStatus = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolItemStatus GetStatus(object configurationKey)
        {
            ToolItemStatus studyItemStatus = _StudyItemStatus.FirstOrDefault(x => x.MatchKey(configurationKey));

            if (studyItemStatus == null)
            {
                studyItemStatus = new ToolItemStatus(configurationKey, ToolItemStatusCode.Future);
                _StudyItemStatus.Add(studyItemStatus);
                studyItemStatus.Touch();
                ModifiedFlag = true;
            }

            return studyItemStatus;
        }

        public ToolItemStatus GetStatusNoCreate(object configurationKey)
        {
            ToolItemStatus studyItemStatus = _StudyItemStatus.FirstOrDefault(x => x.MatchKey(configurationKey));
            return studyItemStatus;
        }

        public void SetStatus(ToolItemStatus studyItemStatus)
        {
            if (studyItemStatus == null)
                return;

            object configurationKey = studyItemStatus.Key;
            ToolItemStatus oldStatus = _StudyItemStatus.FirstOrDefault(x => x.MatchKey(configurationKey));

            if (oldStatus != null)
                _StudyItemStatus.Remove(oldStatus);

            _StudyItemStatus.Add(studyItemStatus);
            ModifiedFlag = true;
        }

        public bool IsStudyItemHidden
        {
            get
            {
                return _IsStudyItemHidden;
            }
            set
            {
                if (value != _IsStudyItemHidden)
                {
                    _IsStudyItemHidden = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageItem InflectionStudyItem
        {
            get
            {
                return _InflectionStudyItem;
            }
            set
            {
                if (value != _InflectionStudyItem)
                {
                    _InflectionStudyItem = value;
                    _StudyItem = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Designator Designation
        {
            get
            {
                return _Designation;
            }
            set
            {
                if (value != _Designation)
                {
                    _Designation = value;
                    ModifiedFlag = true;
                }
            }
        }

        public LexicalCategory Category
        {
            get
            {
                return _Category;
            }
            set
            {
                if (value != _Category)
                {
                    _Category = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string CategoryString
        {
            get
            {
                return _CategoryString;
            }
            set
            {
                if (value != _CategoryString)
                {
                    _CategoryString = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                {
                    if (studyItemStatus.Modified)
                        return true;
                }

                if (_InflectionStudyItem != null)
                {
                    if (_InflectionStudyItem.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                    studyItemStatus.Modified = false;

                if (_InflectionStudyItem != null)
                    _InflectionStudyItem.Modified = false;
            }
        }

        // Returns true if the other studyItem was changed more recently.
        public bool MergeStatus(ToolStudyItem studyItem)
        {
            ToolItemStatus otherStatus;
            bool returnValue = false;

            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
            {
                otherStatus = studyItem.GetStatus(studyItemStatus.Key);

                if (studyItemStatus.MergeStatus(otherStatus))
                    returnValue = true;
            }

            foreach (ToolItemStatus studyItemStatus in studyItem.StudyItemStatus)
            {
                otherStatus = GetStatus(studyItemStatus.Key);

                if (!studyItemStatus.MergeStatus(otherStatus))
                    returnValue = true;
            }

            return returnValue;
        }

        public void CopyStatus(ToolStudyItem studyItem)
        {
            _StudyItemStatus = studyItem.CloneStudyItemStatus();
            ModifiedFlag = true;
        }

        public void Forget(object configurationKey)
        {
            GetStatus(configurationKey).Forget();
        }

        public void Forget()
        {
            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                studyItemStatus.Forget();
        }

        public void ForgetLearned(object configurationKey)
        {
            GetStatus(configurationKey).ForgetLearned();
        }

        public void ForgetLearned()
        {
            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                studyItemStatus.ForgetLearned();
        }

        public void Learned(object configurationKey)
        {
            GetStatus(configurationKey).Learned();
        }

        public void Learned()
        {
            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                studyItemStatus.Learned();
        }

        public void Restore(object configurationKey)
        {
            GetStatus(configurationKey).Restore();
        }

        public void Restore()
        {
            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                studyItemStatus.Restore();
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(_ToolStudyListKey))
                element.Add(new XAttribute("ToolStudyListKey", _ToolStudyListKey));

            if (_ContentStudyListKey != 0)
                element.Add(new XAttribute("ContentStudyListKey", _ContentStudyListKey.ToString()));

            if (!String.IsNullOrEmpty(_ContentStudyItemKey))
                element.Add(new XAttribute("ContentStudyItemKey", _ContentStudyItemKey.ToString()));

            if (IsStudyItemHidden)
                element.Add(new XAttribute("IsStudyItemHidden", (IsStudyItemHidden ? "true" : "false")));

            foreach (ToolItemStatus studyItemStatus in _StudyItemStatus)
                element.Add(studyItemStatus.GetElement("StudyItemStatus"));

            if (_InflectionStudyItem != null)
                element.Add(_InflectionStudyItem.GetElement("InflectionStudyItem"));

            if (_Designation != null)
                element.Add(_Designation.GetElement("Designation"));

            if (_Category != LexicalCategory.Unknown)
                element.Add(new XElement("Category", _Category.ToString()));

            if (!String.IsNullOrEmpty(_CategoryString))
                element.Add(new XElement("CategoryString", _CategoryString));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "ToolStudyListKey":
                    ToolStudyListKey = attributeValue;
                    break;
                case "ContentStudyListKey":
                    ContentStudyListKey = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "ContentStudyItemKey":
                    ContentStudyItemKey = attributeValue;
                    break;
                case "IsStudyItemHidden":
                    IsStudyItemHidden = (attributeValue == "true" ? true : false);
                    break;
                default:
                    returnValue = base.OnAttribute(attribute);
                    break;
            }

            return returnValue;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "StudyItemStatus":
                    SetStatus(new ToolItemStatus(childElement));
                    break;
                case "InflectionStudyItem":
                    _InflectionStudyItem = new MultiLanguageItem(childElement);
                    break;
                case "Designation":
                    _Designation = new Designator(childElement);
                    break;
                case "Category":
                    _Category = Sense.GetLexicalCategoryFromString(childElement.Value);
                    break;
                case "CategoryString":
                    _CategoryString = childElement.Value;
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (_ContentStudyListKey != 0)
            {
                IBaseObjectKeyed contentStudyList = fixups.Get(Source, _ContentStudyListKey.ToString());

                if (contentStudyList != null)
                {
                    int keyInt = contentStudyList.KeyInt;
                    if (_ContentStudyListKey != keyInt)
                    {
                        _ContentStudyListKey = contentStudyList.KeyInt;
                        ModifiedFlag = true;
                    }
                }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolStudyItem otherObject = other as ToolStudyItem;
            int diff;

            if (otherObject == null)
                return base.Compare(other);

            diff = ObjectUtilities.CompareKeys(this, other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_ContentStudyListKey, otherObject.ContentStudyListKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_ContentStudyItemKey, otherObject.ContentStudyItemKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBaseLists<ToolItemStatus>(_StudyItemStatus, otherObject.StudyItemStatus);
            return diff;
        }

        public static int Compare(ToolStudyItem other1, ToolStudyItem other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return 0;
            if ((object)other1 == null)
                return -1;
            if ((object)other2 == null)
                return 1;
            return other1.Compare(other2);
        }

        public static int CompareToolStudyItemLists(List<ToolStudyItem> list1, List<ToolStudyItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ToolStudyItem>(list1, list2);
        }

        public static string GetSortOrderDisplayString(
            List<string> sortOrder,
            List<LanguageDescriptor> languageDescriptors,
            LanguageID uiLanguageID)
        {
            string sortOrderString = String.Empty;

            if (sortOrder == null)
                return sortOrderString;

            foreach (string field in sortOrder)
            {
                LanguageDescriptor languageDescriptor;
                string section = String.Empty;
                bool down = false;

                switch (field)
                {
                    case "Target0Ascending":
                    case "ItemAscending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 0);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Target0Descending":
                    case "ItemDescending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 0);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "Target1Ascending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 1);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Target1Descending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 1);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "Target2Ascending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 2);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Target2Descending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Target", 2);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "Host0Ascending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 0);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Host0Descending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 0);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "Host1Ascending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 1);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Host1Descending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 1);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "Host2Ascending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 2);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        break;
                    case "Host2Descending":
                        languageDescriptor = LanguageDescriptor.GetIthLanguageDescriptor(
                            languageDescriptors, "Host", 2);
                        if (languageDescriptor != null)
                            section = languageDescriptor.LanguageID.LanguageName(uiLanguageID);
                        down = true;
                        break;
                    case "TreeAscending":
                        section = "Tree";
                        break;
                    case "TreeDescending":
                        section = "Tree";
                        down = true;
                        break;
                    case "NodeAscending":
                        section = "Node";
                        break;
                    case "NodeDescending":
                        section = "Node";
                        down = true;
                        break;
                    case "ContentAscending":
                        section = "Content";
                        break;
                    case "ContentDescending":
                        section = "Content";
                        down = true;
                        break;
                    case "PathAscending":
                        section = "Path";
                        break;
                    case "PathDescending":
                        section = "Path";
                        down = true;
                        break;
                    case "StatusAscending":
                        section = "Status";
                        break;
                    case "StatusDescending":
                        section = "Status";
                        down = true;
                        break;
                    default:
                        break;
                }

                if (!String.IsNullOrEmpty(section))
                {
                    if (down)
                        section += "-";

                    if (!String.IsNullOrEmpty(sortOrderString))
                        sortOrderString += ", ";

                    sortOrderString += section;
                }
            }

            return sortOrderString;
        }

        public static ToolStudyItemComparer GetComparer(
            List<string> sortOrder,
            LanguageID itemLanguageID,
            object configurationKey,
            List<LanguageDescriptor> languageDescriptors)
        {
            return new ToolStudyItemComparer(sortOrder, itemLanguageID, configurationKey, languageDescriptors);
        }
    }

    public class ToolStudyItemComparer : IComparer<ToolStudyItem>
    {
        public List<string> SortOrder { get; set; }
        public LanguageID ItemLanguageID { get; set; }
        public object ConfigurationKey { get; set; }
        public List<LanguageDescriptor> LanguageDescriptors { get; set; }
        public LanguageID HostLanguageID { get; set; }
        public IComparer<string> StringComparer { get; set; }

        public ToolStudyItemComparer(
            List<string> sortOrder,
            LanguageID itemLanguageID,
            object configurationKey,
            List<LanguageDescriptor> languageDescriptors)
        {
            SortOrder = sortOrder;
            ItemLanguageID = itemLanguageID;
            ConfigurationKey = configurationKey;
            LanguageDescriptors = languageDescriptors;
            HostLanguageID = languageDescriptors.First(x => x.Name == "Host").LanguageID;
            StringComparer = ApplicationData.GetComparer<string>(true);
        }

        protected LanguageDescriptor GetIthLanguageDescriptor(string name, int ith)
        {
            return LanguageDescriptor.GetIthLanguageDescriptor(LanguageDescriptors, name, ith);
        }

        public int Compare(ToolStudyItem x, ToolStudyItem y)
        {
            MultiLanguageItem xStudyItem = x.StudyItem;
            MultiLanguageItem yStudyItem = y.StudyItem;
            string xText = xStudyItem.Text(ItemLanguageID);
            string yText = yStudyItem.Text(ItemLanguageID);
            ToolItemStatus xStatus = x.GetStatus(ConfigurationKey);
            ToolItemStatus yStatus = y.GetStatus(ConfigurationKey);
            ToolItemStatusCode xState = xStatus.StatusCode;
            ToolItemStatusCode yState = yStatus.StatusCode;
            string xGrade = xStatus.Grade.ToString();
            string yGrade = yStatus.Grade.ToString();

            if ((SortOrder == null) || (SortOrder.Count() == 0))
                return StringComparer.Compare(xText, yText);
            else
            {
                foreach (string field in SortOrder)
                {
                    string str1 = String.Empty;
                    string str2 = String.Empty;
                    int num1 = 0;
                    int num2 = 0;
                    bool isNumber = false;
                    int value = 0;
                    int multiplier = 1;
                    LanguageDescriptor languageDescriptor;

                    switch (field)
                    {
                        case "Target0Ascending":
                        case "ItemAscending":
                            str1 = xText;
                            str2 = yText;
                            break;
                        case "Target0Descending":
                        case "ItemDescending":
                            str1 = xText;
                            str2 = yText;
                            multiplier = -1;
                            break;
                        case "Target1Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 1)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Target1Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 1)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Target2Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 2)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Target2Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 2)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host0Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 0)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host0Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 0)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host1Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 1)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host1Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 1)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host2Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 2)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host2Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 2)) != null)
                            {
                                str1 = xStudyItem.Text(languageDescriptor.LanguageID);
                                str2 = yStudyItem.Text(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "TreeAscending":
                            str1 = xStudyItem.TreeName(HostLanguageID).ToLower();
                            str2 = yStudyItem.TreeName(HostLanguageID).ToLower();
                            break;
                        case "TreeDescending":
                            str1 = xStudyItem.TreeName(HostLanguageID).ToLower();
                            str2 = yStudyItem.TreeName(HostLanguageID).ToLower();
                            multiplier = -1;
                            break;
                        case "NodeAscending":
                            str1 = xStudyItem.NodeName(HostLanguageID).ToLower();
                            str2 = yStudyItem.NodeName(HostLanguageID).ToLower();
                            break;
                        case "NodeDescending":
                            str1 = xStudyItem.NodeName(HostLanguageID).ToLower();
                            str2 = yStudyItem.NodeName(HostLanguageID).ToLower();
                            multiplier = -1;
                            break;
                        case "ContentAscending":
                            str1 = xStudyItem.ContentName(HostLanguageID).ToLower();
                            str2 = yStudyItem.ContentName(HostLanguageID).ToLower();
                            break;
                        case "ContentDescending":
                            str1 = xStudyItem.ContentName(HostLanguageID).ToLower();
                            str2 = yStudyItem.ContentName(HostLanguageID).ToLower();
                            multiplier = -1;
                            break;
                        case "PathAscending":
                            str1 = xStudyItem.GetFullTitleString(HostLanguageID).ToLower();
                            str2 = yStudyItem.GetFullTitleString(HostLanguageID).ToLower();
                            break;
                        case "PathDescending":
                            str1 = xStudyItem.GetFullTitleString(HostLanguageID).ToLower();
                            str2 = yStudyItem.GetFullTitleString(HostLanguageID).ToLower();
                            multiplier = -1;
                            break;
                        case "NextAscending":
                            if (xState == ToolItemStatusCode.Active)
                                str1 = xStatus.NextTouchTime.Ticks.ToString();
                            else if (xState == ToolItemStatusCode.Future)
                                str1 = "F";
                            else
                                str1 = "L";
                            if (yState == ToolItemStatusCode.Active)
                                str2 = yStatus.NextTouchTime.Ticks.ToString();
                            else if (yState == ToolItemStatusCode.Future)
                                str2 = "F";
                            else
                                str2 = "L";
                            break;
                        case "NextDescending":
                            if (xState == ToolItemStatusCode.Active)
                                str1 = xStatus.NextTouchTime.Ticks.ToString();
                            else if (xState == ToolItemStatusCode.Future)
                                str1 = "F";
                            else
                                str1 = "L";
                            if (yState == ToolItemStatusCode.Active)
                                str2 = yStatus.NextTouchTime.Ticks.ToString();
                            else if (yState == ToolItemStatusCode.Future)
                                str2 = "F";
                            else
                                str2 = "L";
                            multiplier = -1;
                            break;
                        case "StatusAscending":
                            str1 = (xState == ToolItemStatusCode.Active ? xGrade : xState.ToString());
                            str2 = (yState == ToolItemStatusCode.Active ? yGrade : yState.ToString());
                            break;
                        case "StatusDescending":
                            str1 = (xState == ToolItemStatusCode.Active ? xGrade : xState.ToString());
                            str2 = (yState == ToolItemStatusCode.Active ? yGrade : yState.ToString());
                            multiplier = -1;
                            break;
                        case "StageAscending":
                            isNumber = true;
                            num1 = xStatus.Stage;
                            num2 = yStatus.Stage;
                            break;
                        case "StageDescending":
                            isNumber = true;
                            num1 = xStatus.Stage;
                            num2 = yStatus.Stage;
                            multiplier = -1;
                            break;
                        default:
                            str1 = String.Empty;
                            str2 = String.Empty;
                            break;
                    }

                    if (isNumber)
                        value = (num1 - num2) * multiplier;
                    else
                        value = StringComparer.Compare(str1, str2) * multiplier;

                    if (value != 0)
                        return value;
                }
            }

            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Content
{
    public enum UserRunStateCode
    {
        Future,
        Active,
        Learned
    }

    public class UserRunItem : BaseObjectKeyed
    {
        protected string _Text;
        List<LanguageString> _Alternates;
        List<MultiLanguageString> _MainDefinitions;
        List<MultiLanguageString> _OtherDefinitions;
        protected UserRunStateCode _UserRunState;
        protected int _Grade;
        protected string _Owner;
        protected string _Profile;
        protected string _TreeName;
        protected string _NodeName;
        protected string _ContentName;
        protected bool _IsPhrase;

        public UserRunItem(
                string text,
                List<LanguageString> alternates,
                List<MultiLanguageString> mainDefinitions,
                List<MultiLanguageString> otherDefinitions,
                UserRunStateCode userRunState,
                int grade,
                string owner,
                string profile,
                string treeName,
                string nodeName,
                string contentName,
                bool isPhrase) :
            base(ComposeKey(owner, profile, text))
        {
            _Text = text;
            _Alternates = alternates;
            _MainDefinitions = mainDefinitions;
            _OtherDefinitions = otherDefinitions;
            _UserRunState = userRunState;
            _Grade = grade;
            _Owner = owner;
            _Profile = profile;
            _TreeName = treeName;
            _NodeName = nodeName;
            _ContentName = contentName;
            _IsPhrase = isPhrase;
        }

        public UserRunItem(UserRunItem other) : base(other)
        {
            CopyUserRunItem(other);
        }

        public UserRunItem(XElement element)
        {
            OnElement(element);
        }

        public UserRunItem()
        {
            ClearUserRunItem();
        }

        public void ClearUserRunItem()
        {
            _Text = String.Empty;
            _Alternates = null;
            _MainDefinitions = null;
            _OtherDefinitions = null;
            _UserRunState = UserRunStateCode.Future;
            _Grade = 0;
            _Owner = String.Empty;
            _Profile = String.Empty;
            _TreeName = String.Empty;
            _NodeName = String.Empty;
            _ContentName = String.Empty;
            _IsPhrase = false;
        }

        public void CopyUserRunItem(UserRunItem other)
        {
            ClearUserRunItem();

            if (other == null)
                return;
            
            _Text = other.Text;
            _Alternates = CloneAlternates();
            _MainDefinitions = CloneMainDefinitions();
            _OtherDefinitions = CloneOtherDefinitions();
            _UserRunState = other.UserRunState;
            _Grade = other.Grade;
            _Owner = other.Owner;
            _Profile = other.Profile;
            _TreeName = other.TreeName;
            _NodeName = other.NodeName;
            _ContentName = other.ContentName;
            _IsPhrase = other.IsPhrase;
        }

        public static string ComposeKey(string owner, string profile, string text)
        {
            return owner + "_" + profile + "_" + text.ToLower();
        }

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string TextLower
        {
            get
            {
                return _Text.ToLower();
            }
        }

        public bool MatchTextIgnoreCase(string otherText)
        {
            return TextUtilities.IsEqualStringsIgnoreCase(_Text, otherText);
        }

        public List<LanguageString> Alternates
        {
            get
            {
                return _Alternates;
            }
            set
            {
                if (value != _Alternates)
                {
                    _Alternates = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasAlternates()
        {
            if (_Alternates == null)
                return false;
            if (_Alternates.Count() != 0)
                return true;
            return false;
        }

        public bool HasAlternate(string text, LanguageID languageID)
        {
            if (_Alternates == null)
                return false;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                {
                    if (TextUtilities.IsEqualStringsIgnoreCase(text, alternate.Text))
                        return true;
                }
            }

            return false;
        }

        public LanguageString GetAlternate(LanguageID languageID)
        {
            if (_Alternates == null)
                return null;
            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                    return alternate;
            }

            return null;
        }

        public LanguageString GetAlternateIndexed(int index)
        {
            if (_Alternates == null)
                return null;

            if ((index >= 0) && (index < _Alternates.Count()))
                return _Alternates[index];

            return null;
        }

        public string GetAlternateText(LanguageID languageID)
        {
            if (_Alternates == null)
                return String.Empty;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                    return alternate.Text;
            }

            return String.Empty;
        }

        public void AddAlternate(string text, LanguageID languageID)
        {
            AddAlternate(new LanguageString(0, languageID, text));
        }

        public void AddAlternate(LanguageString alternate)
        {
            if (_Alternates == null)
                _Alternates = new List<LanguageString>() { alternate };
            else
                _Alternates.Add(alternate);

            ModifiedFlag = true;
        }

        public void DeleteAlternate(string text, LanguageID languageID)
        {
            if (_Alternates == null)
                return;

            int count = _Alternates.Count();
            int index;
            LanguageString alternate;

            for (index = count - 1; index >= 0; index--)
            {
                alternate = _Alternates[index];

                if ((alternate.LanguageID == languageID) && TextUtilities.IsEqualStringsIgnoreCase(text, alternate.Text))
                    _Alternates.RemoveAt(index);
            }
        }

        public void DeleteAlternateIndexed(int index)
        {
            if (_Alternates == null)
                return;

            if ((index >= 0) && (index < _Alternates.Count()))
                _Alternates.RemoveAt(index);
        }

        public int AlternateCount()
        {
            if (_Alternates == null)
                return 0;

            return _Alternates.Count();
        }

        public List<LanguageString> CloneAlternates()
        {
            if (_Alternates == null)
                return null;

            List<LanguageString> alternates = new List<LanguageString>();

            foreach (LanguageString alternate in _Alternates)
                alternates.Add(new LanguageString(alternate));

            return alternates;
        }

        public List<MultiLanguageString> MainDefinitions
        {
            get
            {
                return _MainDefinitions;
            }
            set
            {
                if (value != _MainDefinitions)
                {
                    _MainDefinitions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetMainDefinitionText(LanguageID languageID)
        {
            string definition = String.Empty;
            if (_MainDefinitions != null)
            {
                foreach (MultiLanguageString mls in _MainDefinitions)
                {
                    string text = mls.Text(languageID);
                    if (!String.IsNullOrEmpty(text))
                    {
                        if (!String.IsNullOrEmpty(definition))
                            definition += ", ";
                        definition += text;
                    }
                }
            }
            return definition;
        }

        public bool HasMainDefinitions()
        {
            if ((_MainDefinitions == null) || (_MainDefinitions.Count() == 0))
                return false;

            return true;
        }

        public bool HasMainDefinition(MultiLanguageString definition)
        {
            if ((_MainDefinitions == null) || (definition.LanguageStrings == null))
                return false;

            foreach (MultiLanguageString mls in _MainDefinitions)
            {
                bool match = true;

                foreach (LanguageString ls in definition.LanguageStrings)
                {
                    if (!TextUtilities.IsEqualStringsIgnoreCase(ls.Text, mls.Text(ls.LanguageID)))
                        match = false;
                }

                if (match)
                    return true;
            }

            return false;
        }

        public int MainDefinitionsCount()
        {
            if (_MainDefinitions != null)
                return _MainDefinitions.Count();
            return 0;
        }

        public List<MultiLanguageString> CloneMainDefinitions()
        {
            if (_MainDefinitions == null)
                return null;

            List<MultiLanguageString> mainDefinitions = new List<MultiLanguageString>();

            foreach (MultiLanguageString definition in _MainDefinitions)
                mainDefinitions.Add(new MultiLanguageString(definition));

            return mainDefinitions;
        }

        public List<MultiLanguageString> OtherDefinitions
        {
            get
            {
                return _OtherDefinitions;
            }
            set
            {
                if (value != _OtherDefinitions)
                {
                    _OtherDefinitions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<MultiLanguageString> CloneOtherDefinitions()
        {
            if (_OtherDefinitions == null)
                return null;

            List<MultiLanguageString> otherDefinitions = new List<MultiLanguageString>();

            foreach (MultiLanguageString definition in _OtherDefinitions)
                otherDefinitions.Add(new MultiLanguageString(definition));

            return otherDefinitions;
        }

        public bool HasOtherDefinitions()
        {
            if ((_OtherDefinitions == null) || (_OtherDefinitions.Count() == 0))
                return false;

            return true;
        }

        public bool HasOtherDefinition(MultiLanguageString definition)
        {
            if ((_OtherDefinitions == null) || (definition.LanguageStrings == null))
                return false;

            foreach (MultiLanguageString mls in _OtherDefinitions)
            {
                bool match = true;

                foreach (LanguageString ls in definition.LanguageStrings)
                {
                    if (!TextUtilities.IsEqualStringsIgnoreCase(ls.Text, mls.Text(ls.LanguageID)))
                        match = false;
                }

                if (match)
                    return true;
            }

            return false;
        }

        public int OtherDefinitionsCount()
        {
            if (_OtherDefinitions != null)
                return _OtherDefinitions.Count();
            return 0;
        }

        public UserRunStateCode UserRunState
        {
            get
            {
                return _UserRunState;
            }
            set
            {
                if (value != _UserRunState)
                {
                    _UserRunState = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Grade
        {
            get
            {
                return _Grade;
            }
            set
            {
                if (value != _Grade)
                {
                    _Grade = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Profile
        {
            get
            {
                return _Profile;
            }
            set
            {
                if (value != _Profile)
                {
                    _Profile = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string TreeName
        {
            get
            {
                return _TreeName;
            }
            set
            {
                if (value != _TreeName)
                {
                    _TreeName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string NodeName
        {
            get
            {
                return _NodeName;
            }
            set
            {
                if (value != _NodeName)
                {
                    _NodeName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ContentName
        {
            get
            {
                return _ContentName;
            }
            set
            {
                if (value != _ContentName)
                {
                    _ContentName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsPhrase
        {
            get
            {
                return _IsPhrase;
            }
            set
            {
                if (value != _IsPhrase)
                {
                    _IsPhrase = value;
                    ModifiedFlag = true;                }
            }
        }

        public MultiLanguageItem GetStudyItem(List<LanguageDescriptor> languageDescriptors)
        {
            MultiLanguageItem studyItem = new MultiLanguageItem(TextLower, languageDescriptors);
            bool haveTarget = false;
            LanguageID languageID;
            string text;
            int alternateCount;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                text = null;
                languageID = languageDescriptor.LanguageID;

                if (languageDescriptor.Name == "Target")
                {
                    if (!haveTarget)
                    {
                        text = _Text;
                        haveTarget = true;
                    }
                    else if (_Alternates != null)
                    {
                        text = String.Empty;
                        alternateCount = 0;

                        foreach (LanguageString alternate in _Alternates)
                        {
                            if (alternate.LanguageID != languageID)
                                continue;

                            if (alternateCount == 0)
                                text = alternate.Text;
                            else
                                text = text + ", " + alternate.Text;

                            alternateCount++;
                        }
                    }
                }
                else if ((_MainDefinitions != null) && (_MainDefinitions.Count() != 0))
                {
                    string sense = String.Empty;

                    foreach (MultiLanguageString definition in _MainDefinitions)
                    {
                        sense = definition.Text(languageID);

                        if (!String.IsNullOrEmpty(sense))
                        {
                            if (!String.IsNullOrEmpty(text))
                                text += ", ";

                            text += sense;
                        }
                    }
                }
                else
                    text = String.Empty;

                studyItem.SetText(languageID, text);
            }

            return studyItem;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XAttribute("Text", _Text));

            if ((_Alternates != null) && (_Alternates.Count() != 0))
            {
                foreach (LanguageString alternate in _Alternates)
                    element.Add(alternate.GetElement("Alternate"));
            }

            if ((_MainDefinitions != null) && (_MainDefinitions.Count() != 0))
            {
                foreach (MultiLanguageString definition in _MainDefinitions)
                    element.Add(definition.GetElement("MainDefinition"));
            }

            if ((_OtherDefinitions != null) && (_OtherDefinitions.Count() != 0))
            {
                foreach (MultiLanguageString definition in _OtherDefinitions)
                    element.Add(definition.GetElement("OtherDefinition"));
            }

            element.Add(new XAttribute("UserRunState", _UserRunState.ToString()));
            element.Add(new XAttribute("Grade", _Grade.ToString()));

            if (!String.IsNullOrEmpty(_Owner))
                element.Add(new XAttribute("Owner", _Owner));

            if (!String.IsNullOrEmpty(_Profile))
                element.Add(new XAttribute("Profile", _Profile));

            if (!String.IsNullOrEmpty(_TreeName))
                element.Add(new XAttribute("TreeName", _TreeName));

            if (!String.IsNullOrEmpty(_NodeName))
                element.Add(new XAttribute("NodeName", _NodeName));

            if (!String.IsNullOrEmpty(_ContentName))
                element.Add(new XAttribute("ContentName", _ContentName));

            if (_IsPhrase)
                element.Add(new XAttribute("IsPhrase", _IsPhrase.ToString()));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "Text":
                    _Text = attributeValue;
                    break;
                case "UserRunState":
                    _UserRunState = GetUserRunStateCodeFromString(attributeValue);
                    break;
                case "Grade":
                    _Grade = Convert.ToInt32(attributeValue);
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                case "Profile":
                    _Profile = attributeValue;
                    break;
                case "TreeName":
                    _TreeName = attributeValue;
                    break;
                case "NodeName":
                    _NodeName = attributeValue;
                    break;
                case "ContentName":
                    _ContentName = attributeValue;
                    break;
                case "IsPhrase":
                    _IsPhrase = (attributeValue == "True" ? true : false);
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
                case "Alternate":
                    if (_Alternates == null)
                        _Alternates = new List<LanguageString>();
                    _Alternates.Add(new LanguageString(childElement));
                    break;
                case "MainDefinition":
                    if (_MainDefinitions == null)
                        _MainDefinitions = new List<MultiLanguageString>();
                    _MainDefinitions.Add(new MultiLanguageString(childElement));
                    break;
                case "OtherDefinition":
                    if (_OtherDefinitions == null)
                        _OtherDefinitions = new List<MultiLanguageString>();
                    _OtherDefinitions.Add(new MultiLanguageString(childElement));
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static UserRunStateCode GetUserRunStateCodeFromString(string str)
        {
            UserRunStateCode code;

            switch (str)
            {
                case "Future":
                    code = UserRunStateCode.Future;
                    break;
                case "Active":
                    code = UserRunStateCode.Active;
                    break;
                case "Learned":
                    code = UserRunStateCode.Learned;
                    break;
                default:
                    throw new ObjectException("UserRunItem.GetUserRunStateCodeFromString:  Unknown code:  " + str);
            }

            return code;
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

        public static UserRunItemComparer GetComparer(
            List<string> sortOrder,
            List<LanguageDescriptor> languageDescriptors)
        {
            return new UserRunItemComparer(sortOrder, languageDescriptors);
        }
    }

    public class UserRunItemComparer : IComparer<UserRunItem>
    {
        public List<string> SortOrder { get; set; }
        public List<LanguageDescriptor> LanguageDescriptors { get; set; }
        public IComparer<string> StringComparer { get; set; }

        public UserRunItemComparer(
            List<string> sortOrder,
            List<LanguageDescriptor> languageDescriptors)
        {
            SortOrder = sortOrder;
            LanguageDescriptors = languageDescriptors;
            StringComparer = ApplicationData.GetComparer<string>(true);
        }

        protected LanguageDescriptor GetIthLanguageDescriptor(string name, int ith)
        {
            return LanguageDescriptor.GetIthLanguageDescriptor(LanguageDescriptors, name, ith);
        }

        public int Compare(UserRunItem x, UserRunItem y)
        {
            if ((SortOrder == null) || (SortOrder.Count() == 0))
                return StringComparer.Compare(x.Text, y.Text);
            else
            {
                foreach (string field in SortOrder)
                {
                    string str1 = String.Empty;
                    string str2 = String.Empty;
                    int value = 0;
                    int multiplier = 1;
                    LanguageDescriptor languageDescriptor;

                    switch (field)
                    {
                        case "Target0Ascending":
                        case "ItemAscending":
                            str1 = x.Text;
                            str2 = y.Text;
                            break;
                        case "Target0Descending":
                        case "ItemDescending":
                            str1 = x.Text;
                            str2 = y.Text;
                            multiplier = -1;
                            break;
                        case "Target1Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 1)) != null)
                            {
                                str1 = x.GetAlternateText(languageDescriptor.LanguageID);
                                str2 = y.GetAlternateText(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Target1Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 1)) != null)
                            {
                                str1 = x.GetAlternateText(languageDescriptor.LanguageID);
                                str2 = y.GetAlternateText(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Target2Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 2)) != null)
                            {
                                str1 = x.GetAlternateText(languageDescriptor.LanguageID);
                                str2 = y.GetAlternateText(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Target2Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Target", 2)) != null)
                            {
                                str1 = x.GetAlternateText(languageDescriptor.LanguageID);
                                str2 = y.GetAlternateText(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host0Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 0)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host0Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 0)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host1Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 1)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host1Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 1)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "Host2Ascending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 2)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            break;
                        case "Host2Descending":
                            if ((languageDescriptor = GetIthLanguageDescriptor("Host", 2)) != null)
                            {
                                str1 = x.GetMainDefinitionText(languageDescriptor.LanguageID);
                                str2 = y.GetMainDefinitionText(languageDescriptor.LanguageID);
                            }
                            multiplier = -1;
                            break;
                        case "TreeAscending":
                            str1 = x.TreeName.ToLower();
                            str2 = y.TreeName.ToLower();
                            break;
                        case "TreeDescending":
                            str1 = x.TreeName.ToLower();
                            str2 = y.TreeName.ToLower();
                            multiplier = -1;
                            break;
                        case "NodeAscending":
                            str1 = x.NodeName.ToLower();
                            str2 = y.NodeName.ToLower();
                            break;
                        case "NodeDescending":
                            str1 = x.NodeName.ToLower();
                            str2 = y.NodeName.ToLower();
                            multiplier = -1;
                            break;
                        case "ContentAscending":
                            str1 = x.ContentName.ToLower();
                            str2 = y.ContentName.ToLower();
                            break;
                        case "ContentDescending":
                            str1 = x.ContentName.ToLower();
                            str2 = y.ContentName.ToLower();
                            multiplier = -1;
                            break;
                        case "StatusAscending":
                            str1 = (x.UserRunState == UserRunStateCode.Active ? x.Grade.ToString() : x.UserRunState.ToString());
                            str2 = (y.UserRunState == UserRunStateCode.Active ? y.Grade.ToString() : y.UserRunState.ToString());
                            break;
                        case "StatusDescending":
                            str1 = (x.UserRunState == UserRunStateCode.Active ? x.Grade.ToString() : x.UserRunState.ToString());
                            str2 = (y.UserRunState == UserRunStateCode.Active ? y.Grade.ToString() : y.UserRunState.ToString());
                            multiplier = -1;
                            break;
                        default:
                            str1 = String.Empty;
                            str2 = String.Empty;
                            break;
                    }

                    value = StringComparer.Compare(str1, str2) * multiplier;

                    if (value != 0)
                        return value;
                }
            }

            return 0;
        }
    }
}

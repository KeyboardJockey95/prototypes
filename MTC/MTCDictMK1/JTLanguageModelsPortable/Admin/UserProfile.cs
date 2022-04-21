using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Admin
{
    public class UserProfile : BaseObjectKeyed
    {
        protected LanguageDescriptor _UILanguageDescriptor;
        protected List<LanguageDescriptor> _HostLanguageDescriptors;
        protected List<LanguageDescriptor> _TargetLanguageDescriptors;
        protected List<LanguageDescriptor> _DisplayLanguageDescriptors;
        protected List<string> _Teachers;
        protected bool _ShowAllTeachers;
        protected List<BaseString> _UserOptions;
        protected int _ProfileOrdinal;

        public static string DefaultPreferredFontFamily = "Arial";
        public static string DefaultPreferredFontSizeText = "12";
        public static string DefaultPreferredFontSizeFlash = "18";
        public static string DefaultPreferredFontSizeList = "12";
        public static string DefaultPreferredFontSizeSubtitles = "12";

        public UserProfile(
                string profileName,
                LanguageDescriptor uiLanguageDescriptor,
                List<LanguageDescriptor> hostLanguageDescriptors,
                List<LanguageDescriptor> targetLanguageDescriptors)
            : base(profileName)
        {
            ClearUserProfile();

            if (uiLanguageDescriptor != null)
                _UILanguageDescriptor = uiLanguageDescriptor;

            if (hostLanguageDescriptors != null)
                _HostLanguageDescriptors = hostLanguageDescriptors;

            if (targetLanguageDescriptors != null)
                _TargetLanguageDescriptors = targetLanguageDescriptors;
        }

        public UserProfile(UserProfile other)
            : base(other.Key)
        {
            ClearUserProfile();
            Copy(other);
            ModifiedFlag = false;
        }

        public UserProfile(XElement element)
        {
            OnElement(element);
        }

        public UserProfile()
        {
            ClearUserProfile();
        }

        public override void Clear()
        {
            ClearUserProfile();
            base.Clear();
        }

        public void ClearUserProfile()
        {
            _UILanguageDescriptor = new LanguageDescriptor("UI", LanguageLookup.English, true);
            _HostLanguageDescriptors = new List<LanguageDescriptor>();
            _TargetLanguageDescriptors = new List<LanguageDescriptor>();
            _DisplayLanguageDescriptors = new List<LanguageDescriptor>();
            _Teachers = new List<string>();
            _ShowAllTeachers = true;
            _UserOptions = new List<BaseString>();
            _ProfileOrdinal = 1;
        }

        public void Copy(UserProfile other)
        {
            base.Copy(other);
            _UILanguageDescriptor = new LanguageDescriptor(other.UILanguageDescriptor);
            _HostLanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(other.HostLanguageDescriptors);
            _TargetLanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(other.TargetLanguageDescriptors);
            _DisplayLanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(other.DisplayLanguageDescriptors);
            _Teachers = new List<string>(other.Teachers);
            _ShowAllTeachers = other.ShowAllTeachers;
            _UserOptions = BaseString.CopyBaseStrings(other.UserOptions);
            _ProfileOrdinal = other.ProfileOrdinal;
        }

        public override IBaseObject Clone()
        {
            return new UserProfile(this);
        }

        public string ProfileName
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public int ProfileOrdinal
        {
            get
            {
                return _ProfileOrdinal;
            }
            set
            {
                if (value != _ProfileOrdinal)
                {
                    _ProfileOrdinal = value;
                    ModifiedFlag = false;
                }
            }
        }

        public LanguageDescriptor UILanguageDescriptor
        {
            get
            {
                return _UILanguageDescriptor;
            }
            set
            {
                if (value != _UILanguageDescriptor)
                {
                    if (value != null)
                        _UILanguageDescriptor = value;
                    else
                        new LanguageDescriptor("UI", LanguageLookup.English, true);
                    ModifiedFlag = true;
                }
            }
        }

        public LanguageID UILanguageID
        {
            get
            {
                return _UILanguageDescriptor.LanguageID;
            }
            set
            {
                if (_UILanguageDescriptor.LanguageID != value)
                {
                    _UILanguageDescriptor.LanguageID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageDescriptor> LanguageDescriptors
        {
            get
            {
                List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(_HostLanguageDescriptors);
                languageDescriptors.AddRange(_TargetLanguageDescriptors);
                return languageDescriptors;
            }
            set
            {
                _HostLanguageDescriptors.Clear();
                _TargetLanguageDescriptors.Clear();
                if ((value != null) && (value.Count() != 0))
                {
                    foreach (LanguageDescriptor languageDescriptor in value)
                    {
                        switch (languageDescriptor.Name)
                        {
                            case "UI":
                                _UILanguageDescriptor = languageDescriptor;
                                break;
                            case "Host":
                                _HostLanguageDescriptors.Add(languageDescriptor);
                                break;
                            case "Target":
                                _TargetLanguageDescriptors.Add(languageDescriptor);
                                break;
                            case "Display":
                                _DisplayLanguageDescriptors.Add(languageDescriptor);
                                break;
                            default:
                                break;
                        }
                    }
                }
                ModifiedFlag = true;
            }
        }

        public List<LanguageDescriptor> ExtendedLanguageDescriptors
        {
            get
            {
                return LanguageDescriptor.GetExtendedLanguageDescriptorsFromLanguageDescriptors(LanguageDescriptors);
            }
        }

        public List<LanguageDescriptor> CloneLanguageDescriptors(List<LanguageID> masterLanguageIDs, string name)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();
            languageDescriptors.AddRange(CloneHostLanguageDescriptors(masterLanguageIDs, name));
            languageDescriptors.AddRange(CloneTargetLanguageDescriptors(masterLanguageIDs, name));
            return languageDescriptors;
        }

        public LanguageDescriptor GetLanguageDescriptor(string name, LanguageID languageID)
        {
            switch (name)
            {
                case "UI":
                    if (_UILanguageDescriptor.LanguageID == languageID)
                        return _UILanguageDescriptor;
                    break;
                case "Host":
                    return _HostLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID == languageID));
                case "Target":
                    return _TargetLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID == languageID));
                case "Display":
                    return _DisplayLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID == languageID));
                default:
                    break;
            }
            return null;
        }

        public LanguageDescriptor GetLanguageDescriptor(string name, string languageCode)
        {
            LanguageDescriptor languageDescriptor = null;

            switch (name)
            {
                case "UI":
                    if (_UILanguageDescriptor.LanguageID.LanguageCultureExtensionCode == languageCode)
                        languageDescriptor = _UILanguageDescriptor;
                    else if (_UILanguageDescriptor.LanguageID.LanguageCode == languageCode)
                        languageDescriptor = _UILanguageDescriptor;
                    break;
                case "Host":
                    languageDescriptor = _HostLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCultureExtensionCode == languageCode));
                    if (languageDescriptor == null)
                        languageDescriptor = _HostLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCode == languageCode));
                    break;
                case "Target":
                    languageDescriptor = _TargetLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCultureExtensionCode == languageCode));
                    if (languageDescriptor == null)
                        languageDescriptor = _TargetLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCode == languageCode));
                    break;
                case "Display":
                    languageDescriptor = _DisplayLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCultureExtensionCode == languageCode));
                    if (languageDescriptor == null)
                        languageDescriptor = _DisplayLanguageDescriptors.FirstOrDefault(x => (x.Name == name) && (x.LanguageID.LanguageCode == languageCode));
                    break;
                default:
                    break;
            }
            return languageDescriptor;
        }

        public LanguageDescriptor GetLanguageDescriptor(LanguageID languageID)
        {
            LanguageDescriptor languageDescriptor = _HostLanguageDescriptors.FirstOrDefault(x => (x.LanguageID == languageID));

            if (languageDescriptor != null)
                return languageDescriptor;

            languageDescriptor = _TargetLanguageDescriptors.FirstOrDefault(x => (x.LanguageID == languageID));

            if (languageDescriptor != null)
                return languageDescriptor;

            if (_UILanguageDescriptor.LanguageID == languageID)
                return _UILanguageDescriptor;

            return null;
        }

        public void AddLanguageDescriptor(LanguageDescriptor languageDescriptor)
        {
            switch (languageDescriptor.Name)
            {
                case "UI":
                    UILanguageDescriptor = languageDescriptor;
                    break;
                case "Host":
                    _HostLanguageDescriptors.Add(languageDescriptor);
                    ModifiedFlag = true;
                    break;
                case "Target":
                    _TargetLanguageDescriptors.Add(languageDescriptor);
                    ModifiedFlag = true;
                    break;
                case "Display":
                    _DisplayLanguageDescriptors.Add(languageDescriptor);
                    ModifiedFlag = true;
                    break;
                default:
                    break;
            }
        }

        public void AddLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                AddLanguageDescriptor(languageDescriptor);
        }

        public bool RemoveLanguageDescriptors(string name)
        {
            List<LanguageDescriptor> languageDescriptors = null;
            bool returnValue = false;

            switch (name)
            {
                case "UI":
                    break;
                case "Host":
                    languageDescriptors = _HostLanguageDescriptors;
                    break;
                case "Target":
                    languageDescriptors = _TargetLanguageDescriptors;
                    break;
                case "Display":
                    languageDescriptors = _DisplayLanguageDescriptors;
                    break;
                default:
                    break;
            }

            if (languageDescriptors == null)
                return returnValue;

            int count = languageDescriptors.Count();
            int index = count - 1;

            for (; index >= 0; index--)
            {
                if (languageDescriptors[index].Name == name)
                {
                    languageDescriptors.RemoveAt(index);
                    ModifiedFlag = true;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool RemoveLanguageDescriptors(string[] names)
        {
            bool returnValue = true;

            foreach (string name in names)
            {
                if (!RemoveLanguageDescriptors(name))
                    returnValue = false;
            }

            return returnValue;
        }

        public List<LanguageDescriptor> GetLanguageDescriptors(string name)
        {
            List<LanguageDescriptor> languageDescriptors = null;

            switch (name)
            {
                case "UI":
                    languageDescriptors = new List<LanguageDescriptor>(1) { _UILanguageDescriptor };
                    break;
                case "Host":
                    languageDescriptors = _HostLanguageDescriptors;
                    break;
                case "Target":
                    languageDescriptors = _TargetLanguageDescriptors;
                    break;
                case "Display":
                    languageDescriptors = _DisplayLanguageDescriptors;
                    break;
                default:
                    languageDescriptors = new List<LanguageDescriptor>();
                    break;
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetLanguageDescriptors(string[] orderedNames)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();

            foreach (string name in orderedNames)
            {
                List<LanguageDescriptor> subLanguageDescriptors = GetLanguageDescriptors(name);

                if (subLanguageDescriptors.Count() != 0)
                    languageDescriptors.AddRange(subLanguageDescriptors);
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetLanguageDescriptors(List<string> orderedNames)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();

            foreach (string name in orderedNames)
            {
                List<LanguageDescriptor> subLanguageDescriptors = GetLanguageDescriptors(name);

                if (subLanguageDescriptors.Count() != 0)
                    languageDescriptors.AddRange(subLanguageDescriptors);
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetExtendedLanguageDescriptors(string[] orderedNames)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();

            foreach (string name in orderedNames)
            {
                List<LanguageDescriptor> subLanguageDescriptors = GetLanguageDescriptors(name);

                if (subLanguageDescriptors.Count() != 0)
                {
                    languageDescriptors.AddRange(subLanguageDescriptors);

                    foreach (LanguageDescriptor subLanguageDescriptor in subLanguageDescriptors)
                    {
                        List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(subLanguageDescriptor.LanguageID);

                        foreach (LanguageID familyLanguageID in familyLanguageIDs)
                        {
                            if (languageDescriptors.FirstOrDefault(x => (x.LanguageID == familyLanguageID) && (x.Name == name)) == null)
                                languageDescriptors.Add(new LanguageDescriptor(name, familyLanguageID, true, true));
                        }
                    }
                }
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetExtendedLanguageDescriptors(List<string> orderedNames)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();

            foreach (string name in orderedNames)
            {
                List<LanguageDescriptor> subLanguageDescriptors = GetLanguageDescriptors(name);

                if (subLanguageDescriptors.Count() != 0)
                {
                    languageDescriptors.AddRange(subLanguageDescriptors);

                    foreach (LanguageDescriptor subLanguageDescriptor in subLanguageDescriptors)
                    {
                        List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(subLanguageDescriptor.LanguageID);

                        foreach (LanguageID familyLanguageID in familyLanguageIDs)
                        {
                            if (languageDescriptors.FirstOrDefault(x => (x.LanguageID == familyLanguageID) && (x.Name == name)) == null)
                                languageDescriptors.Add(new LanguageDescriptor(name, familyLanguageID, true, true));
                        }
                    }
                }
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetLanguageDescriptorsFromLanguageCode(string languageCode)
        {
            List<LanguageDescriptor> languageDescriptors = null;

            switch (languageCode)
            {
                case "(my languages)":
                    languageDescriptors = LanguageDescriptors;
                    break;
                case "(target languages)":
                    languageDescriptors = TargetLanguageDescriptors;
                    break;
                case "(host languages)":
                    languageDescriptors = HostLanguageDescriptors;
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                    languageDescriptors = LanguageDescriptors;
                    break;
                case "":
                case null:
                    languageDescriptors = new List<LanguageDescriptor>();
                    break;
                default:
                    LanguageDescriptor languageDescriptor = _HostLanguageDescriptors.FirstOrDefault(x => (x.LanguageID.LanguageCultureExtensionCode == languageCode));
                    if (languageDescriptor == null)
                        languageDescriptor = _TargetLanguageDescriptors.FirstOrDefault(x => (x.LanguageID.LanguageCultureExtensionCode == languageCode));
                    if ((languageDescriptor == null) && (_UILanguageDescriptor.LanguageID.LanguageCultureExtensionCode == languageCode))
                        languageDescriptor = _UILanguageDescriptor;
                    if (languageDescriptor != null)
                        languageDescriptors = new List<LanguageDescriptor>(1) { languageDescriptor };
                    else
                        languageDescriptors = new List<LanguageDescriptor>();
                    break;
            }

            return languageDescriptors;
        }

        public List<LanguageDescriptor> TargetLanguageDescriptors
        {
            get
            {
                return _TargetLanguageDescriptors;
            }
            set
            {
                _TargetLanguageDescriptors = value;
                ModifiedFlag = true;
            }
        }

        public List<LanguageDescriptor> ExtendedTargetLanguageDescriptors
        {
            get
            {
                return LanguageDescriptor.GetExtendedLanguageDescriptorsFromLanguageDescriptors(_TargetLanguageDescriptors);
            }
        }

        public List<LanguageDescriptor> CloneTargetLanguageDescriptors()
        {
            return LanguageDescriptor.CopyLanguageDescriptors(_TargetLanguageDescriptors);
        }

        public List<LanguageDescriptor> CloneTargetLanguageDescriptors(List<LanguageID> masterLanguageIDs, string name)
        {
            return LanguageDescriptor.CopyLanguageDescriptors(_TargetLanguageDescriptors, masterLanguageIDs, name);
        }

        public static string[] TargetHostLanguageDescriptorNames = new string[] { "Target", "Host" };
        public static string[] TargetLanguageDescriptorNames = new string[] { "Target" };

        public List<LanguageDescriptor> TargetHostLanguageDescriptors
        {
            get
            {
                return GetLanguageDescriptors(TargetHostLanguageDescriptorNames);
            }
        }

        public void SetTargetLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            _TargetLanguageDescriptors =  languageDescriptors;
        }

        public List<LanguageDescriptor> ExtendedTargetHostLanguageDescriptors
        {
            get
            {
                return GetExtendedLanguageDescriptors(TargetHostLanguageDescriptorNames);
            }
        }

        public List<LanguageDescriptor> HostLanguageDescriptors
        {
            get
            {
                return _HostLanguageDescriptors;
            }
            set
            {
                _HostLanguageDescriptors = value;
                ModifiedFlag = true;
            }
        }

        public List<LanguageDescriptor> ExtendedHostLanguageDescriptors
        {
            get
            {
                return LanguageDescriptor.GetExtendedLanguageDescriptorsFromLanguageDescriptors(_HostLanguageDescriptors);
            }
        }

        public List<LanguageDescriptor> CloneHostLanguageDescriptors()
        {
            return LanguageDescriptor.CopyLanguageDescriptors(_HostLanguageDescriptors);
        }

        public List<LanguageDescriptor> CloneHostLanguageDescriptors(List<LanguageID> masterLanguageIDs, string name)
        {
            return LanguageDescriptor.CopyLanguageDescriptors(_HostLanguageDescriptors, masterLanguageIDs, name);
        }

        public static string[] HostTargetLanguageDescriptorNames = new string[] { "Host", "Target" };
        public static string[] HostLanguageDescriptorNames = new string[] { "Host" };
        public static string[] UIHostLanguageDescriptorNames = new string[] { "UI", "Host" };

        public List<LanguageDescriptor> HostTargetLanguageDescriptors
        {
            get
            {
                return GetLanguageDescriptors(HostTargetLanguageDescriptorNames);
            }
        }

        public List<LanguageDescriptor> ExtendedHostTargetLanguageDescriptors
        {
            get
            {
                return GetExtendedLanguageDescriptors(HostTargetLanguageDescriptorNames);
            }
        }

        public List<LanguageDescriptor> UIHostLanguageDescriptors
        {
            get
            {
                List<LanguageDescriptor> languageDescriptors = HostLanguageDescriptors;
                if (languageDescriptors.FirstOrDefault(x => x.LanguageID == UILanguageID) == null)
                    languageDescriptors.Insert(0, UILanguageDescriptor);
                return languageDescriptors;
            }
        }

        public List<LanguageDescriptor> UILanguageDescriptors
        {
            get
            {
                List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(1) { UILanguageDescriptor };
                return languageDescriptors;
            }
        }

        public List<LanguageDescriptor> DisplayLanguageDescriptors
        {
            get
            {
                return _DisplayLanguageDescriptors;
            }
            set
            {
                _DisplayLanguageDescriptors = value;
                ModifiedFlag = true;
            }
        }

        public List<LanguageDescriptor> CloneDisplayLanguageDescriptors()
        {
            return LanguageDescriptor.CopyLanguageDescriptors(_DisplayLanguageDescriptors);
        }

        public List<LanguageID> GetLanguageIDs(string[] orderedNames)
        {
            List<LanguageDescriptor> languageDescriptors = GetLanguageDescriptors(orderedNames);
            List<LanguageID> languageIDs = new List<LanguageID>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                    languageIDs.Add(languageDescriptor.LanguageID);
            }

            return languageIDs;
        }

        public List<LanguageID> GetLanguageIDsFromLanguageID(LanguageID languageID)
        {
            if (languageID == null)
                return new List<LanguageID>();
            return GetLanguageIDsFromLanguageCode(languageID.LanguageCultureExtensionCode);
        }

        public List<LanguageID> GetLanguageIDsFromLanguageCode(string languageCode)
        {
            List<LanguageID> languageIDs = null;

            switch (languageCode)
            {
                case "(my languages)":
                    languageIDs = LanguageIDs;
                    break;
                case "(target languages)":
                    languageIDs = TargetLanguageIDs;
                    break;
                case "(host languages)":
                    languageIDs = HostLanguageIDs;
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                    languageIDs = LanguageLookup.LanguageIDs;
                    break;
                case "":
                case null:
                    languageIDs = new List<LanguageID>();
                    break;
                default:
                    languageIDs = new List<LanguageID>(1) { LanguageLookup.GetLanguageIDNoAdd(languageCode) };
                    break;
            }

            return languageIDs;
        }

        public LanguageID GetLanguageIDFromNameOrCode(string name)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(name);

            if (languageDescriptor == null)
                return UILanguageID;

            return languageDescriptor.LanguageID;
        }

        public LanguageDescriptor GetLanguageDescriptor(string descriptorName)
        {
            return GetLanguageDescriptors(descriptorName).FirstOrDefault();
        }

        public LanguageDescriptor GetLanguageDescriptorSafe(string descriptorName)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor == null)
                languageDescriptor = new LanguageDescriptor(descriptorName, UILanguageID, false, false);

            return languageDescriptor;
        }

        public LanguageID GetLanguageID(string descriptorName)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor != null)
                return languageDescriptor.LanguageID;

            return null;
        }

        public void SetLanguageID(string descriptorName, LanguageID languageID)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor != null)
            {
                languageDescriptor.LanguageID = languageID;
                languageDescriptor.Used = (languageID == null ? false : true);
            }
            else
            {
                RemoveLanguageDescriptors(descriptorName);
                AddLanguageDescriptor(
                    LanguageDescriptor.LanguageDescriptorFromLanguageID(descriptorName, languageID));
            }
        }

        public string ExpandLanguageCode(string languageCode)
        {
            switch (languageCode)
            {
                case "(any)":
                case "(all languages)":
                    return languageCode;
                case "(target languages)":
                    return LanguagesKey(TargetLanguageIDs);
                case "(host languages)":
                    return LanguagesKey(HostLanguageIDs);
                case "(my languages)":
                    return LanguagesKey(LanguageIDs);
                case "(none)":
                    return languageCode;
                default:
                    return languageCode;
            }
        }

        public string LanguagesKey(List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count == 0))
                return String.Empty;

            String key = "|";

            foreach (LanguageID languageID in languageIDs)
                key = key + languageID.LanguageCultureExtensionCode + "|";

            return key;
        }

        public string GetLanguageName(string descriptorName)
        {
            LanguageID languageID = GetLanguageID(descriptorName);

            if (languageID != null)
                return languageID.LanguageName(UILanguageID);

            return descriptorName;
        }

        public LanguageID HostLanguageID
        {
            get
            {
                return GetLanguageID("Host");
            }
            set
            {
                SetLanguageID("Host", value);
            }
        }

        public LanguageID HostLanguageIDSafe
        {
            get
            {
                LanguageID hostLanguageID = GetLanguageID("Host");
                if (hostLanguageID == null)
                {
                    hostLanguageID = GetLanguageID("UI");
                    if (hostLanguageID == null)
                        hostLanguageID = UILanguageID;
                    if (hostLanguageID == null)
                        hostLanguageID = LanguageLookup.English;
                }
                return hostLanguageID;
            }
            set
            {
                SetLanguageID("Target", value);
            }
        }

        public LanguageID TargetLanguageID
        {
            get
            {
                return GetLanguageID("Target");
            }
            set
            {
                SetLanguageID("Target", value);
            }
        }

        public LanguageID TargetLanguageIDSafe
        {
            get
            {
                LanguageID targetLanguageID = GetLanguageID("Target");
                if (targetLanguageID == null)
                {
                    targetLanguageID = GetLanguageID("Host");
                    if (targetLanguageID == null)
                        targetLanguageID = UILanguageID;
                    if (targetLanguageID == null)
                        targetLanguageID = LanguageLookup.English;
                }
                return targetLanguageID;
            }
            set
            {
                SetLanguageID("Target", value);
            }
        }

        /*
        public LanguageID TargetAlternate1LanguageID
        {
            get
            {
                return GetLanguageID("TargetAlternate1");
            }
            set
            {
                SetLanguageID("TargetAlternate1", value);
            }
        }

        public LanguageID TargetAlternate2LanguageID
        {
            get
            {
                return GetLanguageID("TargetAlternate2");
            }
            set
            {
                SetLanguageID("TargetAlternate2", value);
            }
        }

        public LanguageID TargetAlternate3LanguageID
        {
            get
            {
                return GetLanguageID("TargetAlternate3");
            }
            set
            {
                SetLanguageID("TargetAlternate3", value);
            }
        }
        */

        public List<LanguageID> LanguageIDs
        {
            get
            {
                return HostTargetLanguageIDs;
            }
        }

        public List<LanguageID> ExtendedLanguageIDs
        {
            get
            {
                return LanguageID.GetExtendedLanguageIDsFromLanguageIDs(LanguageIDs);
            }
        }

        public List<LanguageID> TargetLanguageIDs
        {
            get
            {
                return LanguageDescriptor.LanguageIDsFromLanguageDescriptors(_TargetLanguageDescriptors);
            }
        }

        public List<LanguageID> ExtendedTargetLanguageIDs
        {
            get
            {
                return LanguageID.GetExtendedLanguageIDsFromLanguageIDs(TargetLanguageIDs);
            }
        }

        public bool HasAnyTargetLanguages()
        {
            if (_TargetLanguageDescriptors == null)
                return false;

            if (_TargetLanguageDescriptors.Count() == 0)
                return false;

            return true;
        }

        public int TargetLanguageCount()
        {
            if (_TargetLanguageDescriptors == null)
                return 0;

            return _TargetLanguageDescriptors.Count();
        }

        public List<LanguageID> HostLanguageIDs
        {
            get
            {
                return LanguageDescriptor.LanguageIDsFromLanguageDescriptors(_HostLanguageDescriptors);
            }
        }

        public List<LanguageID> ExtendedHostLanguageIDs
        {
            get
            {
                return LanguageID.GetExtendedLanguageIDsFromLanguageIDs(HostLanguageIDs);
            }
        }

        public List<LanguageID> TargetHostLanguageIDs
        {
            get
            {
                return GetLanguageIDs(TargetHostLanguageDescriptorNames);
            }
        }

        public List<LanguageID> ExtendedTargetHostLanguageIDs
        {
            get
            {
                return LanguageID.GetExtendedLanguageIDsFromLanguageIDs(TargetHostLanguageIDs);
            }
        }

        public List<LanguageID> HostTargetLanguageIDs
        {
            get
            {
                return GetLanguageIDs(HostTargetLanguageDescriptorNames);
            }
        }

        public List<LanguageID> ExtendedHostTargetLanguageIDs
        {
            get
            {
                return LanguageID.GetExtendedLanguageIDsFromLanguageIDs(HostTargetLanguageIDs);
            }
        }

        public bool GetShowState(string key, string name, string languageCode)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(name, languageCode);

            if (languageDescriptor == null)
                languageDescriptor = GetLanguageDescriptor("Display", languageCode);

            if (languageDescriptor == null)
            {
                languageDescriptor = new LanguageDescriptor("Display", LanguageLookup.GetLanguageID(languageCode), true, true);
                AddLanguageDescriptor(languageDescriptor);
            }

            return languageDescriptor.GetShowState(key);
        }

        public void SetShowState(string key, string name, string languageCode, bool show)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(name, languageCode);

            if (languageDescriptor == null)
                languageDescriptor = GetLanguageDescriptor("Display", languageCode);

            if (languageDescriptor == null)
            {
                languageDescriptor = new LanguageDescriptor("Display", LanguageLookup.GetLanguageID(languageCode), true, true);
                AddLanguageDescriptor(languageDescriptor);
            }

            if (languageDescriptor.GetShowState(key) != show)
            {
                languageDescriptor.SetShowState(key, show);
                ModifiedFlag = true;
            }
        }

        public bool ToggleShowState(string key, string name, string languageCode)
        {
            bool show = !GetShowState(key, name, languageCode);
            SetShowState(key, name, languageCode, show);
            return show;
        }

        public void UpdateShowStates(string key, List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                {
                    languageDescriptor.Show = GetShowState(key, languageDescriptor.Name, languageDescriptor.LanguageID.LanguageCultureExtensionCode);

                    LanguageDescriptor myLanguageDescriptor = GetLanguageDescriptor(languageDescriptor.LanguageID);

                    if ((myLanguageDescriptor != null) && (myLanguageDescriptor != languageDescriptor))
                    {
                        languageDescriptor.PreferredFontFamily = myLanguageDescriptor.PreferredFontFamily;
                        languageDescriptor.PreferredFontSizeFlash = myLanguageDescriptor.PreferredFontSizeFlash;
                        languageDescriptor.PreferredFontSizeList = myLanguageDescriptor.PreferredFontSizeList;
                        languageDescriptor.PreferredFontSizeText = myLanguageDescriptor.PreferredFontSizeText;
                    }

                    LanguageID languageID = languageDescriptor.LanguageID;
                    string languageSuffix = "_" +languageID.LanguageCultureExtensionCode;

                    if (String.IsNullOrEmpty(languageDescriptor.PreferredFontFamily))
                    {
                        LanguageDescription languageDescription;
                        string defaultFontFamily = DefaultPreferredFontFamily;
                        if (((languageDescription = LanguageLookup.GetLanguageDescription(languageDescriptor.LanguageID)) != null) &&
                                !String.IsNullOrEmpty(languageDescription.PreferedFontName))
                            defaultFontFamily = languageDescription.PreferedFontName;
                        languageDescriptor.PreferredFontFamily = GetUserOptionString("PreferredFontFamily" + languageSuffix, defaultFontFamily);
                    }

                    if (String.IsNullOrEmpty(languageDescriptor.PreferredFontSizeText))
                        languageDescriptor.PreferredFontSizeText = GetUserOptionString("PreferredFontSizeText" + languageSuffix, DefaultPreferredFontSizeText);

                    if (String.IsNullOrEmpty(languageDescriptor.PreferredFontSizeFlash))
                        languageDescriptor.PreferredFontSizeFlash = GetUserOptionString("PreferredFontSizeFlash" + languageSuffix, DefaultPreferredFontSizeFlash);

                    if (String.IsNullOrEmpty(languageDescriptor.PreferredFontSizeList))
                        languageDescriptor.PreferredFontSizeList = GetUserOptionString("PreferredFontSizeList" + languageSuffix, DefaultPreferredFontSizeList);

                    if (String.IsNullOrEmpty(languageDescriptor.PreferredFontSizeSubtitles))
                        languageDescriptor.PreferredFontSizeSubtitles = GetUserOptionString("PreferredFontSizeSubtitles" + languageSuffix, DefaultPreferredFontSizeSubtitles);
                }
            }
        }

        public void ResetShowStates()
        {
            ResetLanguageDescriptorShowStates(_TargetLanguageDescriptors);
            ResetLanguageDescriptorShowStates(_HostLanguageDescriptors);
            ResetLanguageDescriptorShowStates(_DisplayLanguageDescriptors);
        }

        public void ResetLanguageDescriptorShowStates(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                    languageDescriptor.ClearShowStates();
            }
        }

        public bool GetShowLanguage(string key, string descriptorName)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor != null)
                return languageDescriptor.GetShowState(key);

            return false;
        }

        public void SetShowLanguage(string key, string descriptorName, bool show)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor != null)
                languageDescriptor.SetShowState(key, show);
        }

        public bool ShowHostLanguageID
        {
            get
            {
                return GetShowLanguage(null, "Host");
            }
            set
            {
                SetShowLanguage(null, "Host", value);
            }
        }

        public bool ShowTargetLanguageID
        {
            get
            {
                return GetShowLanguage(null, "Target");
            }
            set
            {
                SetShowLanguage(null, "Target", value);
            }
        }

        /*
        public bool ShowTargetAlternate1LanguageID
        {
            get
            {
                return GetShowLanguage("TargetAlternate1");
            }
            set
            {
                SetShowLanguage("TargetAlternate1", value);
            }
        }

        public bool ShowTargetAlternate2LanguageID
        {
            get
            {
                return GetShowLanguage("TargetAlternate2");
            }
            set
            {
                SetShowLanguage("TargetAlternate2", value);
            }
        }

        public bool ShowTargetAlternate3LanguageID
        {
            get
            {
                return GetShowLanguage("TargetAlternate3");
            }
            set
            {
                SetShowLanguage("TargetAlternate3", value);
            }
        }
        */

        public bool HasLanguage(string descriptorName)
        {
            LanguageDescriptor languageDescriptor = GetLanguageDescriptor(descriptorName);

            if (languageDescriptor != null)
                return languageDescriptor.Used;

            return false;
        }

        public bool HasHostLanguage
        {
            get
            {
                return HasLanguage("Host");
            }
        }

        public bool HasTargetLanguage
        {
            get
            {
                return HasLanguage("Target");
            }
        }

        /*
        public bool HasTargetAlternate1Language
        {
            get
            {
                return HasLanguage("TargetAlternate1");
            }
        }

        public bool HasTargetAlternate2Language
        {
            get
            {
                return HasLanguage("TargetAlternate2");
            }
        }

        public bool HasTargetAlternate3Language
        {
            get
            {
                return HasLanguage("TargetAlternate3");
            }
        }
        */

        public List<string> Teachers
        {
            get
            {
                return _Teachers;
            }
            set
            {
                if (value != _Teachers)
                {
                    if (value != null)
                        _Teachers = value;
                    else
                        _Teachers = new List<string>();

                    ModifiedFlag = true;
                }
            }
        }

        public bool ShowAllTeachers
        {
            get
            {
                return _ShowAllTeachers;
            }
            set
            {
                if (value != _ShowAllTeachers)
                {
                    _ShowAllTeachers = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<BaseString> UserOptions
        {
            get
            {
                return _UserOptions;
            }
            set
            {
                if (value != _UserOptions)
                {
                    if (value != null)
                        _UserOptions = value;
                    else
                        _UserOptions = new List<BaseString>();

                    ModifiedFlag = true;
                }
            }
        }

        public List<BaseString> CloneUserOptions()
        {
            List<BaseString> userOptions = null;
            
            if (_UserOptions != null)
                userOptions = new List<BaseString>(_UserOptions);

            return userOptions;
       }

        public bool HasUserOption(string key)
        {
            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key) != null;
            return false;
        }

        public BaseString GetUserOption(string key)
        {
            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public void SetUserOption(BaseString option)
        {
            if (_UserOptions == null)
            {
                _UserOptions = new List<BaseString>(1) { option };
                ModifiedFlag = true;
            }
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == option.KeyString);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(option);
                    ModifiedFlag = true;
                }
                else if (!String.IsNullOrEmpty(option.Text))
                {
                    if (oldUserOption.Text != option.Text)
                    {
                        oldUserOption.Text = option.Text;
                        ModifiedFlag = true;
                    }
                }
                else
                {
                    _UserOptions.Remove(oldUserOption);
                    ModifiedFlag = true;
                }
            }
        }

        public string GetUserOptionString(string key, string defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if ((option != null) && (option.Text != null))
                return option.Text;
            if (defaultValue == null)
                return String.Empty;
            return defaultValue;
        }

        public void SetUserOptionString(string key, string value)
        {
            if (_UserOptions == null)
            {
                _UserOptions = new List<BaseString>(1) { new BaseString(key, value) };
                ModifiedFlag = true;
            }
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == key);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(new BaseString(key, value));
                    ModifiedFlag = true;
                }
                else
                {
                    if (oldUserOption.Text != value)
                    {
                        oldUserOption.Text = value;
                        ModifiedFlag = true;
                    }
                }
            }
        }

        public int GetUserOptionInteger(string key, int defaultValue = 0)
        {
            string stringValue = GetUserOptionString(key);
            int integerValue = defaultValue;

            if (ObjectUtilities.IsNumberString(stringValue))
            {
                try
                {
                    integerValue = Convert.ToInt32(stringValue);
                }
                catch
                {
                }
            }

            return integerValue;
        }

        public void SetUserOptionInteger(string key, int value)
        {
            SetUserOptionString(key, value.ToString());
        }

        public float GetUserOptionFloat(string key, float defaultValue = 0)
        {
            string stringValue = GetUserOptionString(key);
            float floatValue = defaultValue;

            if (ObjectUtilities.IsFloatString(stringValue))
            {
                try
                {
                    floatValue = (float)Convert.ToDouble(stringValue);
                }
                catch
                {
                }
            }

            return floatValue;
        }

        public void SetUserOptionFloat(string key, float value)
        {
            SetUserOptionString(key, value.ToString());
        }

        public bool GetUserOptionFlag(string key, bool defaultValue = false)
        {
            switch (GetUserOptionString(key).ToLower())
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return defaultValue;
            }
        }

        public void SetUserOptionFlag(string key, bool value)
        {
            SetUserOptionString(key, (value ? "true" : "false"));
        }

        public LanguageID GetUserOptionLanguageID(string key, LanguageID defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return LanguageLookup.GetLanguageIDNoAdd(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionLanguageID(string key, LanguageID value)
        {
            SetUserOptionString(key, (value != null ? value.LanguageCultureExtensionCode : String.Empty));
        }

        public List<LanguageID> GetUserOptionLanguageIDList(string key, List<LanguageID> defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetLanguageIDListFromString(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionLanguageIDList(string key, List<LanguageID> value)
        {
            SetUserOptionString(key, (value != null ? TextUtilities.GetStringFromLanguageIDList(value) : String.Empty));
        }

        public List<string> GetUserOptionStringList(string key, List<string> defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringListFromString(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringList(string key, List<string> value)
        {
            SetUserOptionString(key, (value != null ? TextUtilities.GetStringFromStringList(value) : String.Empty));
        }

        public Dictionary<string, string> GetUserOptionStringDictionary(string key, Dictionary<string, string> defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringDictionaryFromString(oldValue, defaultValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringDictionary(string key, Dictionary<string, string> value)
        {
            SetUserOptionString(key, (value != null ? TextUtilities.GetStringFromStringDictionary(value) : String.Empty));
        }

        public bool DeleteUserOption(string key)
        {
            if (_UserOptions == null)
                return false;

            BaseString option = GetUserOption(key);

            if (option == null)
                return false;

            ModifiedFlag = true;

            return _UserOptions.Remove(option);
        }

        public void DeleteAllUserOptions()
        {
            _UserOptions = null;
            ModifiedFlag = true;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_UILanguageDescriptor != null)
                element.Add(_UILanguageDescriptor.GetElement("UILanguageDescriptor"));
            if (_HostLanguageDescriptors != null)
            {
                XElement subElement = new XElement("HostLanguageDescriptors");
                foreach (LanguageDescriptor languageDescriptor in _HostLanguageDescriptors)
                    subElement.Add(languageDescriptor.Xml);
                element.Add(subElement);
            }
            if (_TargetLanguageDescriptors != null)
            {
                XElement subElement = new XElement("TargetLanguageDescriptors");
                foreach (LanguageDescriptor languageDescriptor in _TargetLanguageDescriptors)
                    subElement.Add(languageDescriptor.Xml);
                element.Add(subElement);
            }
            if (_DisplayLanguageDescriptors != null)
            {
                XElement subElement = new XElement("DisplayLanguageDescriptors");
                foreach (LanguageDescriptor languageDescriptor in _DisplayLanguageDescriptors)
                    subElement.Add(languageDescriptor.Xml);
                element.Add(subElement);
            }
            if (_Teachers != null)
                element.Add(ObjectUtilities.GetElementFromStringList("Teachers", _Teachers));
            element.Add(new XElement("ShowAllTeachers", ShowAllTeachers.ToString()));
            if ((_UserOptions != null) && (_UserOptions.Count() != 0))
                element.Add(BaseString.GetElementFromBaseStringList("UserOptions", _UserOptions));
            element.Add(new XElement("ProfileOrdinal", _ProfileOrdinal.ToString()));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            LanguageDescriptor languageDescriptor;

            switch (childElement.Name.LocalName)
            {
                case "UILanguageDescriptor":
                    _UILanguageDescriptor = new LanguageDescriptor(childElement);
                    break;
                case "HostLanguageDescriptors":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        _HostLanguageDescriptors = new List<LanguageDescriptor>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            languageDescriptor = new LanguageDescriptor(subElement);
                            if (languageDescriptor.Used)
                                _HostLanguageDescriptors.Add(languageDescriptor);
                        }
                    }
                    break;
                case "TargetLanguageDescriptors":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        _TargetLanguageDescriptors = new List<LanguageDescriptor>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            languageDescriptor = new LanguageDescriptor(subElement);
                            if (languageDescriptor.Used)
                                _TargetLanguageDescriptors.Add(languageDescriptor);
                        }
                    }
                    break;
                case "DisplayLanguageDescriptors":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        _DisplayLanguageDescriptors = new List<LanguageDescriptor>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            languageDescriptor = new LanguageDescriptor(subElement);
                            if (languageDescriptor.Used)
                                _DisplayLanguageDescriptors.Add(languageDescriptor);
                        }
                    }
                    break;
                case "Teachers":
                    _Teachers = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "ShowAllTeachers":
                    _ShowAllTeachers = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "UserOptions":
                    _UserOptions = BaseString.GetBaseStringListFromElement(childElement);
                    break;
                case "ProfileOrdinal":
                    _ProfileOrdinal = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            UserProfile otherObject = other as UserProfile;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = _UILanguageDescriptor.Compare(otherObject.UILanguageDescriptor);
            diff = LanguageDescriptor.CompareLanguageDescriptorLists(_HostLanguageDescriptors, otherObject.HostLanguageDescriptors);
            if (diff != 0)
                return diff;
            diff = LanguageDescriptor.CompareLanguageDescriptorLists(_TargetLanguageDescriptors, otherObject.TargetLanguageDescriptors);
            if (diff != 0)
                return diff;
            diff = LanguageDescriptor.CompareLanguageDescriptorLists(_DisplayLanguageDescriptors, otherObject.DisplayLanguageDescriptors);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_Teachers, otherObject.Teachers);
            if (diff != 0)
                return diff;
            diff = ((_ShowAllTeachers == otherObject.ShowAllTeachers) ? 0 : ((_ShowAllTeachers == true) ? 1 : -1));
            if (diff != 0)
                return diff;
            diff = _ProfileOrdinal - otherObject.ProfileOrdinal;
            return diff;
        }

        public static int Compare(UserProfile object1, UserProfile object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareUserKeys(UserProfile object1, UserProfile object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }
    }
}

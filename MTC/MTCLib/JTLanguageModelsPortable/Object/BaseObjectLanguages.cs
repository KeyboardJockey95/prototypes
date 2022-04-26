using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObjectLanguages : BaseObjectKeyed
    {
        protected List<LanguageID> _TargetLanguageIDs;
        protected List<LanguageID> _HostLanguageIDs;
        protected string _Owner;

        public BaseObjectLanguages(object key, List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner)
            : base(key)
        {
            _TargetLanguageIDs = targetLanguageIDs;
            _HostLanguageIDs = hostLanguageIDs;
            _Owner = owner;
        }

        public BaseObjectLanguages(object key)
            : base(key)
        {
            ClearBaseObjectLanguages();
        }

        public BaseObjectLanguages(BaseObjectLanguages other, object key)
            : base(key)
        {
            Copy(other);
            Modified = false;
        }

        public BaseObjectLanguages(BaseObjectLanguages other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public BaseObjectLanguages(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectLanguages()
        {
            ClearBaseObjectLanguages();
        }

        public void Copy(BaseObjectLanguages other)
        {
            CopyLanguages(other);
            _Owner = other.Owner;
        }

        public void CopyDeep(BaseObjectLanguages other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectLanguages();
        }

        public void ClearBaseObjectLanguages()
        {
            _TargetLanguageIDs = null;
            _HostLanguageIDs = null;
            _Owner = null;
        }

        public virtual void CopyLanguages(BaseObjectLanguages other)
        {
            if (other.TargetLanguageIDs != null)
                _TargetLanguageIDs = new List<LanguageID>(other.TargetLanguageIDs);
            else
                _TargetLanguageIDs = null;

            if (other.HostLanguageIDs != null)
                _HostLanguageIDs = new List<LanguageID>(other.HostLanguageIDs);
            else
                _HostLanguageIDs = null;
        }

        public virtual void CopyLanguagesExpand(BaseObjectLanguages other, UserProfile userProfile)
        {
            _TargetLanguageIDs = other.ExpandTargetLanguageIDs(userProfile);
            _HostLanguageIDs = other.ExpandHostLanguageIDs(userProfile);
        }

        public virtual void CopyLanguagesFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            _TargetLanguageIDs = new List<LanguageID>();
            _HostLanguageIDs = new List<LanguageID>();

            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null))
                    continue;

                if (languageDescriptor.Name.StartsWith("Target"))
                    _TargetLanguageIDs.Add(languageDescriptor.LanguageID);
                else if (languageDescriptor.Name.StartsWith("Host"))
                    _HostLanguageIDs.Add(languageDescriptor.LanguageID);
            }
        }

        public List<LanguageDescriptor> LanguageDescriptors
        {
            get
            {
                List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();
                LanguageDescriptor languageDescriptor;

                if (_TargetLanguageIDs != null)
                {
                    foreach (LanguageID languageID in _TargetLanguageIDs)
                    {
                        languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Target", languageID);
                        languageDescriptors.Add(languageDescriptor);
                    }
                }

                if (_HostLanguageIDs != null)
                {
                    foreach (LanguageID languageID in _HostLanguageIDs)
                    {
                        languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Host", languageID);
                        languageDescriptors.Add(languageDescriptor);
                    }
                }

                return languageDescriptors;
            }
            set
            {
                CopyLanguagesFromLanguageDescriptors(value);
            }
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectLanguages(this);
        }

        public virtual List<LanguageID> LanguageIDs
        {
            get
            {
                return ObjectUtilities.ListConcatenateUnique<LanguageID>(TargetLanguageIDs, HostLanguageIDs);
            }
        }

        public List<LanguageID> ExpandLanguageIDs(UserProfile userProfile)
        {
            return LanguageLookup.ExpandLanguageIDs(LanguageIDs, userProfile);
        }

        public virtual List<LanguageID> TargetLanguageIDs
        {
            get
            {
                return _TargetLanguageIDs;
            }
            set
            {
                if (LanguageID.CompareLanguageIDLists(_TargetLanguageIDs, value) != 0)
                {
                    _TargetLanguageIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageID> CloneTargetLanguageIDs()
        {
            if (_TargetLanguageIDs != null)
                return new List<LanguageID>(_TargetLanguageIDs);

            return null;
        }

        public List<LanguageID> CloneTargetLanguageIDs(List<LanguageID> masterLanguageIDs)
        {
            if (_TargetLanguageIDs != null)
                return LanguageID.CreateIntersection(_TargetLanguageIDs, masterLanguageIDs);

            return null;
        }

        public int TargetLanguageCount()
        {
            if (_TargetLanguageIDs != null)
                return _TargetLanguageIDs.Count();

            return 0;
        }

        public List<LanguageID> ExpandTargetLanguageIDs(UserProfile userProfile)
        {
            return LanguageLookup.ExpandLanguageIDs(_TargetLanguageIDs, userProfile);
        }

        public List<LanguageDescriptor> TargetLanguageDescriptors
        {
            get
            {
                return LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Target", _TargetLanguageIDs);
            }
            set
            {
                TargetLanguageIDs = LanguageDescriptor.LanguageIDsFromLanguageDescriptors(value);
            }
        }

        public virtual List<LanguageID> HostLanguageIDs
        {
            get
            {
                return _HostLanguageIDs;
            }
            set
            {
                if (LanguageID.CompareLanguageIDLists(_HostLanguageIDs, value) != 0)
                {
                    _HostLanguageIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageID> CloneHostLanguageIDs()
        {
            if (_HostLanguageIDs != null)
                return new List<LanguageID>(_HostLanguageIDs);

            return null;
        }

        public int HostLanguageCount()
        {
            if (_HostLanguageIDs != null)
                return _HostLanguageIDs.Count();

            return 0;
        }

        public List<LanguageID> CloneHostLanguageIDs(List<LanguageID> masterLanguageIDs)
        {
            if (_HostLanguageIDs != null)
                return LanguageID.CreateIntersection(_HostLanguageIDs, masterLanguageIDs);

            return null;
        }

        public List<LanguageID> ExpandHostLanguageIDs(UserProfile userProfile)
        {
            return LanguageLookup.ExpandLanguageIDs(_HostLanguageIDs, userProfile);
        }

        public List<LanguageDescriptor> HostLanguageDescriptors
        {
            get
            {
                return LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Host", _HostLanguageIDs);
            }
            set
            {
                HostLanguageIDs = LanguageDescriptor.LanguageIDsFromLanguageDescriptors(value);
            }
        }

        public virtual LanguageID FirstTargetLanguageID
        {
            get
            {
                if ((_TargetLanguageIDs != null) && (_TargetLanguageIDs.Count() != 0))
                    return _TargetLanguageIDs.First();
                return null;
            }
        }

        public virtual LanguageID FirstHostLanguageID
        {
            get
            {
                if ((_HostLanguageIDs != null) && (_HostLanguageIDs.Count() != 0))
                    return _HostLanguageIDs.First();
                return null;
            }
        }

        public virtual LanguageID FirstTargetOrHostLanguageID
        {
            get
            {
                if ((_TargetLanguageIDs != null) && (_TargetLanguageIDs.Count() != 0))
                    return _TargetLanguageIDs.First();
                else if ((_HostLanguageIDs != null) && (_HostLanguageIDs.Count() != 0))
                    return _HostLanguageIDs.First();
                return null;
            }
        }

        public List<LanguageID> TargetHostLanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();
                List<LanguageID> targetLanguageIDs = TargetLanguageIDs;
                List<LanguageID> hostLanguageIDs = HostLanguageIDs;
                if (targetLanguageIDs != null)
                    languageIDs.AddRange(targetLanguageIDs);
                if (hostLanguageIDs != null)
                    languageIDs.AddRange(hostLanguageIDs);
                return languageIDs;
            }
        }

        public List<LanguageID> HostTargetLanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();
                List<LanguageID> targetLanguageIDs = TargetLanguageIDs;
                List<LanguageID> hostLanguageIDs = HostLanguageIDs;
                if (hostLanguageIDs != null)
                    languageIDs.AddRange(hostLanguageIDs);
                if (targetLanguageIDs != null)
                    languageIDs.AddRange(targetLanguageIDs);
                return languageIDs;
            }
        }

        public virtual List<LanguageID> MediaLanguageIDs
        {
            get
            {
                return ObjectUtilities.ListConcatenateUnique<LanguageID>(MediaTargetLanguageIDs, MediaHostLanguageIDs);
            }
        }

        public virtual List<LanguageID> MediaTargetLanguageIDs
        {
            get
            {
                return LanguageLookup.GetMediaLanguageIDs(_TargetLanguageIDs);
            }
        }

        public virtual List<LanguageID> MediaHostLanguageIDs
        {
            get
            {
                return LanguageLookup.GetMediaLanguageIDs(_HostLanguageIDs);
            }
        }

        public virtual LanguageID FirstMediaTargetLanguageID
        {
            get
            {
                if ((_TargetLanguageIDs != null) && (_TargetLanguageIDs.Count() != 0))
                    return LanguageLookup.GetMediaLanguageID(_TargetLanguageIDs.First());
                return null;
            }
        }

        public virtual LanguageID FirstMediaHostLanguageID
        {
            get
            {
                if ((_HostLanguageIDs != null) && (_HostLanguageIDs.Count() != 0))
                    return LanguageLookup.GetMediaLanguageID(_HostLanguageIDs.First());
                return null;
            }
        }

        public virtual LanguageID FirstMediaTargetOrHostLanguageID
        {
            get
            {
                if ((_TargetLanguageIDs != null) && (_TargetLanguageIDs.Count() != 0))
                    return LanguageLookup.GetMediaLanguageID(_TargetLanguageIDs.First());
                else if ((_HostLanguageIDs != null) && (_HostLanguageIDs.Count() != 0))
                    return LanguageLookup.GetMediaLanguageID(_HostLanguageIDs.First());
                return null;
            }
        }

        public List<LanguageID> ExpandMediaLanguageIDs(UserProfile userProfile)
        {
            List<LanguageID> mediaLanguageIDs = LanguageLookup.GetMediaLanguageIDs(ExpandLanguageIDs(userProfile));
            return mediaLanguageIDs;
        }

        public List<LanguageID> ExpandMediaHostLanguageIDs(UserProfile userProfile)
        {
            List<LanguageID> mediaLanguageIDs = LanguageLookup.GetMediaLanguageIDs(ExpandHostLanguageIDs(userProfile));
            return mediaLanguageIDs;
        }

        public List<LanguageID> ExpandMediaTargetLanguageIDs(UserProfile userProfile)
        {
            List<LanguageID> mediaLanguageIDs = LanguageLookup.GetMediaLanguageIDs(ExpandTargetLanguageIDs(userProfile));
            return mediaLanguageIDs;
        }

        public List<LanguageID> HostAndUILanguageIDs(UserProfile userProfile)
        {
            List<LanguageID> languageIDs = LanguageLookup.ExpandLanguageIDs(_HostLanguageIDs, userProfile);
            LanguageID uiLanguageID = userProfile.UILanguageID;

            if (languageIDs == null)
                return new List<LanguageID>(1) { uiLanguageID };
            else if (languageIDs.Contains(uiLanguageID))
                languageIDs.Remove(uiLanguageID);

            languageIDs.Insert(0, uiLanguageID);
            return languageIDs;
        }

        public List<LanguageID> HostTargetAndUILanguageIDs(UserProfile userProfile)
        {
            List<LanguageID> languageIDs = LanguageLookup.ExpandLanguageIDs(HostTargetLanguageIDs, userProfile);
            LanguageID uiLanguageID = userProfile.UILanguageID;

            if (languageIDs == null)
                return new List<LanguageID>(1) { uiLanguageID };
            else if (languageIDs.Contains(uiLanguageID))
                languageIDs.Remove(uiLanguageID);

            languageIDs.Insert(0, uiLanguageID);
            return languageIDs;
        }

        public string LanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(LanguageIDs, "|", "|", "|");
            }
        }

        public string TargetLanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(TargetLanguageIDs, "|", "|", "|");
            }
            set
            {
                char[] delimiters = { '|' };
                TargetLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(value, delimiters);
            }
        }

        public string HostLanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(HostLanguageIDs, "|", "|", "|");
            }
            set
            {
                char[] delimiters = { '|' };
                HostLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(value, delimiters);
            }
        }

        public string LanguagesKeyExpanded
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedStringExpanded(LanguageIDs, "|", "|", "|");
            }
        }

        public string TargetLanguagesKeyExpanded
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedStringExpanded(TargetLanguageIDs, "|", "|", "|");
            }
        }

        public string HostLanguagesKeyExpanded
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedStringExpanded(HostLanguageIDs, "|", "|", "|");
            }
        }

        public string TargetLanguagesSuffix
        {
            get
            {
                string suffix = String.Empty;

                if ((TargetLanguageIDs == null) || (TargetLanguageIDs.Count() == 0))
                    return suffix;

                suffix = "_";

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    if (String.IsNullOrEmpty(languageID.LanguageCode))
                        continue;

                    if (!suffix.Contains(languageID.LanguageCode))
                        suffix += languageID.LanguageCode;
                }

                return suffix;
            }
        }

        public string HostLanguagesSuffix
        {
            get
            {
                string suffix = String.Empty;

                if ((HostLanguageIDs == null) || (HostLanguageIDs.Count() == 0))
                    return suffix;

                suffix = "_";

                foreach (LanguageID languageID in HostLanguageIDs)
                {
                    if (String.IsNullOrEmpty(languageID.LanguageCode))
                        continue;

                    if (!suffix.Contains(languageID.LanguageCode))
                        suffix += languageID.LanguageCode;
                }

                return suffix;
            }
        }

        public string TargetHostLanguagesSuffix
        {
            get
            {
                string suffix = TargetLanguagesSuffix + HostLanguagesSuffix;
                return suffix;
            }
        }

        public string TargetHostMediaLanguageAbbreviations(LanguageID uiLanguageID, string delimiter)
        {
            string returnValue = LanguageID.ConvertLanguageIDListToDelimitedMediaAbbreviationString(
                LanguageIDs, String.Empty, delimiter, String.Empty, uiLanguageID);
            return returnValue;
        }

        public bool UseLanguage(LanguageID languageID, List<LanguageID> languageIDs, UserProfile userProfile)
        {
            if (languageID == null)
                return true;

            languageIDs = LanguageLookup.ExpandLanguageIDs(languageIDs, userProfile);

            if (languageIDs == null)
                return false;

            if (languageIDs.Contains(languageID))
                return true;

            foreach (LanguageID lid in languageIDs)
            {
                if (lid.LanguageCode == languageID.LanguageCode)
                    return true;
            }

            return false;
        }

        public bool UseLanguage(LanguageID languageID, UserProfile userProfile)
        {
            return UseLanguage(languageID, LanguageIDs, userProfile);
        }

        public bool UseLanguage(string languageCode, UserProfile userProfile)
        {
            switch (languageCode)
            {
                case "(my languages)":
                    if (UseTargetLanguage(userProfile))
                        return true;
                    else if (UseHostLanguage(userProfile))
                        return true;
                    break;
                case "(target languages)":
                    if (UseTargetLanguage(userProfile))
                        return true;
                    break;
                case "(host languages)":
                    if (UseHostLanguage(userProfile))
                        return true;
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                case "":
                case null:
                    return true;
                default:
                    {
                        LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
                        List<LanguageID> targetLanguageIDs = ExpandTargetLanguageIDs(userProfile);
                        List<LanguageID> hostLanguageIDs = ExpandHostLanguageIDs(userProfile);
                        if (targetLanguageIDs != null)
                        {
                            if (targetLanguageIDs.Contains(languageID))
                                return true;
                        }
                        if (hostLanguageIDs != null)
                        {
                            if (hostLanguageIDs.Contains(languageID))
                                return true;
                        }
                        if (targetLanguageIDs != null)
                        {
                            foreach (LanguageID lid in targetLanguageIDs)
                            {
                                if (lid.LanguageCode == languageID.LanguageCode)
                                    return true;
                            }
                        }
                        if (hostLanguageIDs != null)
                        {
                            foreach (LanguageID lid in hostLanguageIDs)
                            {
                                if (lid.LanguageCode == languageID.LanguageCode)
                                    return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        public bool UseTargetLanguage(string languageCode, UserProfile userProfile)
        {
            if (TargetLanguageIDs == null)
                return true;

            switch (languageCode)
            {
                case "(my languages)":
                    if (UseTargetLanguage(userProfile))
                        return true;
                    break;
                case "(target languages)":
                    if (UseTargetLanguage(userProfile))
                        return true;
                    break;
                case "(host languages)":
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                case "":
                case null:
                    return true;
                default:
                    {
                        LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
                        List<LanguageID> targetLanguageIDs = ExpandTargetLanguageIDs(userProfile);

                        if ((targetLanguageIDs != null) && (targetLanguageIDs.Count != 0))
                        {
                            if (targetLanguageIDs.Contains(languageID))
                                return true;

                            foreach (LanguageID lid in targetLanguageIDs)
                            {
                                if (lid.LanguageCode == languageID.LanguageCode)
                                    return true;
                            }
                        }
                        else
                            return true;
                    }
                    break;
            }

            return false;
        }

        public bool UseTargetLanguage(UserProfile userProfile)
        {
            List<LanguageDescriptor> targetLanguageDescriptors = userProfile.TargetLanguageDescriptors;

            if ((targetLanguageDescriptors == null) || (targetLanguageDescriptors.Count == 0))
                return true;

            foreach (LanguageDescriptor languageDescriptor in targetLanguageDescriptors)
            {
                switch (languageDescriptor.Name)
                {
                    case "Target":
                    case "TargetAlternate1":
                    case "TargetAlternate2":
                    case "TargetAlternate3":
                        if ((TargetLanguageIDs != null) && (TargetLanguageIDs.Count != 0))
                        {
                            if (UseLanguage(languageDescriptor.LanguageID, TargetLanguageIDs, userProfile))
                                return true;
                        }
                        else
                            return true;
                        break;
                    case "Host":
                    default:
                        break;
                }
            }

            return false;
        }

        public bool UseHostLanguage(string languageCode, UserProfile userProfile)
        {
            if (HostLanguageIDs == null)
                return true;

            switch (languageCode)
            {
                case "(my languages)":
                    if (UseHostLanguage(userProfile))
                        return true;
                    break;
                case "(target languages)":
                    break;
                case "(host languages)":
                    if (UseHostLanguage(userProfile))
                        return true;
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                case "":
                case null:
                    return true;
                default:
                    {
                        LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
                        List<LanguageID> hostLanguageIDs = ExpandHostLanguageIDs(userProfile);

                        if ((hostLanguageIDs != null) && (hostLanguageIDs.Count != 0))
                        {
                            if (hostLanguageIDs.Contains(languageID))
                                return true;

                            foreach (LanguageID lid in hostLanguageIDs)
                            {
                                if (lid.LanguageCode == languageID.LanguageCode)
                                    return true;
                            }
                        }
                        else
                            return true;
                    }
                    break;
            }

            return false;
        }

        public bool UseHostLanguage(UserProfile userProfile)
        {
            List<LanguageDescriptor> hostLanguageDescriptors = userProfile.HostLanguageDescriptors;

            if ((hostLanguageDescriptors == null) || (hostLanguageDescriptors.Count == 0))
                return true;

            foreach (LanguageDescriptor languageDescriptor in hostLanguageDescriptors)
            {
                switch (languageDescriptor.Name)
                {
                    case "Host":
                        if ((HostLanguageIDs != null) && (HostLanguageIDs.Count != 0))
                        {
                            if (UseLanguage(languageDescriptor.LanguageID, HostLanguageIDs, userProfile))
                                return true;
                        }
                        else
                            return true;
                        break;
                    case "Target":
                    case "TargetAlternate1":
                    case "TargetAlternate2":
                    case "TargetAlternate3":
                    default:
                        break;
                }
            }

            return false;
        }

        public bool UsesMediaLanguage(string targetLanguageCode, string hostLanguageCode, UserProfile userProfile)
        {
            bool targetMatch = false;
            bool hostMatch = false;
            List<LanguageID> targetLanguageIDs = ExpandTargetLanguageIDs(userProfile);
            List<LanguageID> hostLanguageIDs = ExpandHostLanguageIDs(userProfile);

            if ((targetLanguageIDs != null) && (targetLanguageIDs.Count() != 0))
            {
                switch (targetLanguageCode)
                {
                    case null:
                    case "":
                    case "(any)":
                    case "(all languages)":
                        targetMatch = true;
                        break;
                    case "(my languages)":
                        foreach (LanguageID languageID in userProfile.LanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in targetLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    targetMatch = true;
                                    break;
                                }
                            }
                            if (targetMatch)
                                break;
                        }
                        break;
                    case "(target languages)":
                        foreach (LanguageID languageID in userProfile.TargetLanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in targetLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    targetMatch = true;
                                    break;
                                }
                            }
                            if (targetMatch)
                                break;
                        }
                        break;
                    case "(host languages)":
                        foreach (LanguageID languageID in userProfile.HostLanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in targetLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    targetMatch = true;
                                    break;
                                }
                            }
                            if (targetMatch)
                                break;
                        }
                        break;
                    default:
                        foreach (LanguageID testLanguageID in targetLanguageIDs)
                        {
                            if (targetLanguageCode == testLanguageID.LanguageCode)
                            {
                                targetMatch = true;
                                break;
                            }
                        }
                        if (!targetMatch)
                        {
                            foreach (LanguageID testLanguageID in targetLanguageIDs)
                            {
                                if (targetLanguageCode == testLanguageID.LanguageCode)
                                {
                                    targetMatch = true;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }

            if ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0))
                hostMatch = targetMatch;
            else
            {
                switch (hostLanguageCode)
                {
                    case null:
                    case "":
                    case "(any)":
                    case "(all languages)":
                        hostMatch = true;
                        break;
                    case "(my languages)":
                        foreach (LanguageID languageID in userProfile.LanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in hostLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    hostMatch = true;
                                    break;
                                }
                            }
                            if (hostMatch)
                                break;
                        }
                        break;
                    case "(target languages)":
                        foreach (LanguageID languageID in userProfile.TargetLanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in hostLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    hostMatch = true;
                                    break;
                                }
                            }
                            if (hostMatch)
                                break;
                        }
                        break;
                    case "(host languages)":
                        foreach (LanguageID languageID in userProfile.HostLanguageIDs)
                        {
                            foreach (LanguageID testLanguageID in hostLanguageIDs)
                            {
                                if (languageID.LanguageCode == testLanguageID.LanguageCode)
                                {
                                    hostMatch = true;
                                    break;
                                }
                            }
                            if (hostMatch)
                                break;
                        }
                        break;
                    default:
                        foreach (LanguageID testLanguageID in hostLanguageIDs)
                        {
                            if (hostLanguageCode == testLanguageID.LanguageCode)
                            {
                                hostMatch = true;
                                break;
                            }
                        }
                        if (!hostMatch)
                        {
                            foreach (LanguageID testLanguageID in hostLanguageIDs)
                            {
                                if (hostLanguageCode == testLanguageID.LanguageCode)
                                {
                                    hostMatch = true;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }

            return targetMatch && hostMatch;
        }

        public bool HasLanguage(LanguageID languageID)
        {
            if (_TargetLanguageIDs != null)
            {
                if (_TargetLanguageIDs.Contains(languageID))
                    return true;
            }

            if (_HostLanguageIDs != null)
            {
                if (_HostLanguageIDs.Contains(languageID))
                    return true;
            }

            List<LanguageID> expandedTargetLanguageIDs = ExpandTargetLanguageIDs(null);

            if (expandedTargetLanguageIDs != null)
            {
                if (expandedTargetLanguageIDs.Contains(languageID))
                    return true;
            }

            List<LanguageID> expandedHostLanguageIDs = ExpandHostLanguageIDs(null);

            if (expandedHostLanguageIDs != null)
            {
                if (expandedHostLanguageIDs.Contains(languageID))
                    return true;
            }

            return false;
        }

        public bool HasLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasLanguage(languageID);
        }

        public bool HasMediaLanguage(LanguageID languageID)
        {
            if (_TargetLanguageIDs != null)
            {
                if (_TargetLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            if (_HostLanguageIDs != null)
            {
                if (_HostLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            List<LanguageID> expandedTargetLanguageIDs = ExpandTargetLanguageIDs(null);

            if (expandedTargetLanguageIDs != null)
            {
                if (expandedTargetLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            List<LanguageID> expandedHostLanguageIDs = ExpandHostLanguageIDs(null);

            if (expandedHostLanguageIDs != null)
            {
                if (expandedHostLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            return false;
        }

        public bool HasMediaLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasMediaLanguage(languageID);
        }

        public bool HasTargetLanguage(LanguageID languageID)
        {
            if (languageID == null)
            {
                if ((_TargetLanguageIDs == null) || (_TargetLanguageIDs.Count == 0))
                    return true;
            }
            else if (_TargetLanguageIDs != null)
            {
                if (_TargetLanguageIDs.Contains(languageID))
                    return true;
            }

            List<LanguageID> expandedTargetLanguageIDs = ExpandTargetLanguageIDs(null);

            if (expandedTargetLanguageIDs != null)
            {
                if (expandedTargetLanguageIDs.Contains(languageID))
                    return true;
            }

            return false;
        }

        public bool HasTargetLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasTargetLanguage(languageID);
        }

        public bool HasTargetMediaLanguage(LanguageID languageID)
        {
            if (_TargetLanguageIDs != null)
            {
                if (_TargetLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            List<LanguageID> expandedTargetLanguageIDs = ExpandTargetLanguageIDs(null);

            if (expandedTargetLanguageIDs != null)
            {
                if (expandedTargetLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            return false;
        }

        public bool HasTargetMediaLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasTargetMediaLanguage(languageID);
        }

        public bool HasHostLanguage(LanguageID languageID)
        {
            if (languageID == null)
            {
                if ((_HostLanguageIDs == null) || (_HostLanguageIDs.Count == 0))
                    return true;
            }
            else if (_HostLanguageIDs != null)
            {
                if (_HostLanguageIDs.Contains(languageID))
                    return true;
            }

            List<LanguageID> expandedHostLanguageIDs = ExpandHostLanguageIDs(null);

            if (expandedHostLanguageIDs != null)
            {
                if (expandedHostLanguageIDs.Contains(languageID))
                    return true;
            }

            return false;
        }

        public bool HasHostLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasHostLanguage(languageID);
        }

        public bool HasHostMediaLanguage(LanguageID languageID)
        {
            if (_HostLanguageIDs != null)
            {
                if (_HostLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            List<LanguageID> expandedHostLanguageIDs = ExpandHostLanguageIDs(null);

            if (expandedHostLanguageIDs != null)
            {
                if (expandedHostLanguageIDs.FirstOrDefault(x => x.MediaLanguageCompare(languageID) == 0) != null)
                    return true;
            }

            return false;
        }

        public bool HasHostMediaLanguageCode(string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return HasHostMediaLanguage(languageID);
        }

        public List<LanguageID> GetLanguageListFromLanguageID(LanguageID languageID, UserProfile userProfile)
        {
            string languageCode = (languageID != null ? languageID.LanguageCultureExtensionCode : "(my languages)");
            return GetLanguageListFromLanguageCode(languageCode, userProfile);
        }

        public List<LanguageID> GetLanguageListFromLanguageCode(string languageCode, UserProfile userProfile)
        {
            List<LanguageID> languageIDs;

            switch (languageCode)
            {
                case "(my languages)":
                    languageIDs = userProfile.LanguageIDs;
                    break;
                case "(target languages)":
                    languageIDs = userProfile.TargetLanguageIDs;
                    break;
                case "(host languages)":
                    languageIDs = userProfile.HostLanguageIDs;
                    break;
                case "(any)":
                case "(any language)":
                case "(all languages)":
                case "":
                case null:
                    return ExpandLanguageIDs(userProfile);
                default:
                    {
                        LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
                        languageIDs = new List<LanguageID>() { languageID };
                    }
                    break;
            }

            return languageIDs;
        }

        public List<LanguageID> GetLanguageListFromMediaLanguageID(LanguageID mediaLanguageID)
        {
            if (mediaLanguageID == null)
                return LanguageIDs;

            string languageCode = mediaLanguageID.LanguageCode;

            List<LanguageID> languageIDs = LanguageIDs.Where(x => x.LanguageCode == languageCode).ToList();

            return languageIDs;
        }

        public void AddLanguage(LanguageID languageID)
        {
            AddTargetLanguage(languageID);
        }

        public void AddTargetLanguage(LanguageID languageID)
        {
            if (_TargetLanguageIDs == null)
                _TargetLanguageIDs = new List<LanguageID>();

            _TargetLanguageIDs.Add(languageID);
            AddLanguage(languageID);
        }

        public void AddHostLanguage(LanguageID languageID)
        {
            if (_HostLanguageIDs == null)
                _HostLanguageIDs = new List<LanguageID>();

            _HostLanguageIDs.Add(languageID);
            AddLanguage(languageID);
        }

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString + ": " + LanguagesKey);
                    else
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, LanguagesKey);
                    break;
                case DisplayDetail.Full:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                    else
                        ObjectUtilities.DisplayLabel(this, label, indent);
                    DisplayField("TargetLanguageIDs", TargetLanguagesKey, indent + 1);
                    DisplayField("HostLanguageIDs", HostLanguagesKey, indent + 1);
                    DisplayField("Owner", Owner, indent + 1);
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
            XElement element = base.GetElement(name);
            string targetLanguagesKey = TargetLanguagesKey;
            if (!String.IsNullOrEmpty(targetLanguagesKey))
                element.Add(new XAttribute("TargetLanguagesKey", targetLanguagesKey));
            string hostLanguagesKey = HostLanguagesKey;
            if (!String.IsNullOrEmpty(hostLanguagesKey))
                element.Add(new XAttribute("HostLanguagesKey", hostLanguagesKey));
            if (!String.IsNullOrEmpty(_Owner))
                element.Add(new XAttribute("Owner", _Owner));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "TargetLanguagesKey":
                    TargetLanguagesKey = attributeValue;
                    break;
                case "HostLanguagesKey":
                    HostLanguagesKey = attributeValue;
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectLanguages otherLanguagesBase = other as BaseObjectLanguages;

            if (otherLanguagesBase == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = LanguageID.CompareLanguageIDLists(_TargetLanguageIDs, otherLanguagesBase.TargetLanguageIDs);
            if (diff != 0)
                return diff;
            diff = LanguageID.CompareLanguageIDLists(_HostLanguageIDs, otherLanguagesBase.HostLanguageIDs);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherLanguagesBase.Owner);
            return diff;
        }

        public static int Compare(BaseObjectLanguages object1, BaseObjectLanguages object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(BaseObjectLanguages object1, BaseObjectLanguages object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }
    }
}

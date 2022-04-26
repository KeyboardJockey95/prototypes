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
    public class LanguageDescription : BaseObjectKeyed
    {
        public LanguageID _LanguageID;
        public MultiLanguageString _LanguageName;
        public bool _CharacterBased;
        public bool _ReadTopToBottom;
        public bool _ReadRightToLeft;
        public string _PreferedFontName;
        public string _DictionaryFontSize;
        public List<string> _VoiceNames;
        public string _PreferedVoiceName;
        public bool _GoogleSpeechToTextSupported;
        public bool _GoogleTextToSpeechSupported;
        public bool _UseGoogleTextToSpeech;
        public string _Owner;
        public int _LongestDictionaryEntryLength;
        public int _LongestPrefixLength;
        public int _LongestSuffixLength;
        public int _LongestInflectionLength;

        public LanguageDescription(
                LanguageID languageID,
                MultiLanguageString languageName,
                bool characterBased,
                bool readTopToBottom,
                bool readRightToLeft,
                string preferedFontName,
                string dictionaryFontSize,
                List<string> voiceNames,
                string preferedVoiceName,
                string owner)
            : base(languageID != null ? languageID.LanguageCultureExtensionCode : null)
        {
            ClearLanguageDescription();
            _LanguageID = languageID;
            _LanguageName = languageName;
            _CharacterBased = characterBased;
            _ReadTopToBottom = readTopToBottom;
            _ReadRightToLeft = readRightToLeft;
            _PreferedFontName = preferedFontName;
            _DictionaryFontSize = dictionaryFontSize;
            if (voiceNames == null)
                voiceNames = new List<string>();
            _VoiceNames = voiceNames;
            _PreferedVoiceName = preferedVoiceName;
            _Owner = owner;
            _LongestDictionaryEntryLength = 0;
            _LongestPrefixLength = 0;
            _LongestSuffixLength = 0;
            _LongestInflectionLength = 0;
        }

    public LanguageDescription(LanguageDescription other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public LanguageDescription(XElement element)
        {
            OnElement(element);
        }

        public LanguageDescription()
        {
            ClearLanguageDescription();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageDescription();
        }

        public void ClearLanguageDescription()
        {
            _LanguageID = null;
            _LanguageName = null;
            _CharacterBased = false;
            _ReadTopToBottom = false;
            _ReadRightToLeft = false;
            _PreferedFontName = null;
            _DictionaryFontSize = null;
            _VoiceNames = new List<string>();
            _PreferedVoiceName = null;
            _GoogleSpeechToTextSupported = false;
            _GoogleTextToSpeechSupported = false;
            _UseGoogleTextToSpeech = false;
            _Owner = null;
            _LongestDictionaryEntryLength = 0;
            _LongestPrefixLength = 0;
            _LongestSuffixLength = 0;
            _LongestInflectionLength = 0;
        }

        public virtual void Copy(LanguageDescription other)
        {
            _LanguageID = other.LanguageID;
            _LanguageName = other.LanguageName;
            _CharacterBased = other.CharacterBased;
            _ReadTopToBottom = other.ReadTopToBottom;
            _ReadRightToLeft = other.ReadRightToLeft;
            _PreferedFontName = other.PreferedFontName;
            _DictionaryFontSize = other.DictionaryFontSize;
            _VoiceNames = other.VoiceNames;
            _PreferedVoiceName = other.PreferedVoiceName;
            _GoogleSpeechToTextSupported = other.GoogleSpeechToTextSupported;
            _GoogleTextToSpeechSupported = other.GoogleTextToSpeechSupported;
            _UseGoogleTextToSpeech = other.UseGoogleTextToSpeech;
            _Owner = other.Owner;
            _LongestDictionaryEntryLength = 0;
            _LongestPrefixLength = 0;
            _LongestSuffixLength = 0;
            _LongestInflectionLength = 0;
        }

        public override IBaseObject Clone()
        {
            return new LanguageDescription(this);
        }

        public LanguageID LanguageID
        {
            get
            {
                return _LanguageID;
            }
            set
            {
                if (value != _LanguageID)
                {
                    _LanguageID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageString LanguageName
        {
            get
            {
                return _LanguageName;
            }
            set
            {
                if (value != _LanguageName)
                {
                    _LanguageName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetLanguageName(LanguageID uiLanguageID)
        {
            if (_LanguageName == null)
                return null;
            else
                return _LanguageName.Text(uiLanguageID);
        }

        public void SetLanguageName(LanguageID uiLanguageID, string name)
        {
            if (_LanguageName != null)
                _LanguageName.SetText(uiLanguageID, name);
        }

        public bool CharacterBased
        {
            get
            {
                return _CharacterBased;
            }
            set
            {
                if (value != _CharacterBased)
                {
                    _CharacterBased = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool ReadTopToBottom
        {
            get
            {
                return _ReadTopToBottom;
            }
            set
            {
                if (value != _ReadTopToBottom)
                {
                    _ReadTopToBottom = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool ReadRightToLeft
        {
            get
            {
                return _ReadRightToLeft;
            }
            set
            {
                if (value != _ReadRightToLeft)
                {
                    _ReadRightToLeft = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PreferedFontName
        {
            get
            {
                return _PreferedFontName;
            }
            set
            {
                if (value != _PreferedFontName)
                {
                    _PreferedFontName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string DictionaryFontSize
        {
            get
            {
                return _DictionaryFontSize;
            }
            set
            {
                if (value != _DictionaryFontSize)
                {
                    _DictionaryFontSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PreferredDictionaryFontStyle
        {
            get
            {
                string fontStyle = "";
                if (!String.IsNullOrEmpty(_PreferedFontName))
                    fontStyle += "font-family:" + _PreferedFontName + ";";
                if (!String.IsNullOrEmpty(_DictionaryFontSize))
                    fontStyle += "font-size:" + _DictionaryFontSize + "pt;";
                return fontStyle;
            }
            private set { }
        }

        public List<string> VoiceNames
        {
            get
            {
                return _VoiceNames;
            }
            set
            {
                if (value != _VoiceNames)
                {
                    _VoiceNames = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PreferedVoiceName
        {
            get
            {
                return _PreferedVoiceName;
            }
            set
            {
                if (value != _PreferedVoiceName)
                {
                    _PreferedVoiceName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PreferedVoiceNameOrDefault
        {
            get
            {
                if (!String.IsNullOrEmpty(_PreferedVoiceName))
                    return _PreferedVoiceName;
                if ((_VoiceNames != null) && (_VoiceNames.Count != 0))
                    return _VoiceNames[0];
                return "";
            }
        }

        public void ClearVoiceNames()
        {
            if ((_VoiceNames == null) || (_VoiceNames.Count() == 0))
                return;

            _VoiceNames.Clear();
            ModifiedFlag = true;
        }

        public void AddVoiceName(string voiceName)
        {
            if (_VoiceNames == null)
                _VoiceNames = new List<string>();

            if (!_VoiceNames.Contains(voiceName))
            {
                _VoiceNames.Add(voiceName);
                ModifiedFlag = true;
            }
        }

        public bool HasAnyVoiceNames()
        {
            if (_VoiceNames == null)
                return false;

            if (_VoiceNames.Count == 0)
                return false;

            return true;
        }

        public bool GoogleSpeechToTextSupported
        {
            get
            {
                return _GoogleSpeechToTextSupported;
            }
            set
            {
                if (value != _GoogleSpeechToTextSupported)
                {
                    _GoogleSpeechToTextSupported = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool GoogleTextToSpeechSupported
        {
            get
            {
                return _GoogleTextToSpeechSupported;
            }
            set
            {
                if (value != _GoogleTextToSpeechSupported)
                {
                    _GoogleTextToSpeechSupported = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool UseGoogleTextToSpeech
        {
            get
            {
                return _UseGoogleTextToSpeech;
            }
            set
            {
                if (value != _UseGoogleTextToSpeech)
                {
                    _UseGoogleTextToSpeech = value;
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
                if (_Owner != value)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int LongestDictionaryEntryLength
        {
            get
            {
                return _LongestDictionaryEntryLength;
            }
            set
            {
                if (_LongestDictionaryEntryLength != value)
                {
                    _LongestDictionaryEntryLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int LongestPrefixLength
        {
            get
            {
                return _LongestPrefixLength;
            }
            set
            {
                if (_LongestPrefixLength != value)
                {
                    _LongestPrefixLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int LongestSuffixLength
        {
            get
            {
                return _LongestSuffixLength;
            }
            set
            {
                if (_LongestSuffixLength != value)
                {
                    _LongestSuffixLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int LongestInflectionLength
        {
            get
            {
                return _LongestInflectionLength;
            }
            set
            {
                if (_LongestInflectionLength != value)
                {
                    _LongestInflectionLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            string languageCode = (_LanguageID != null ? _LanguageID.LanguageCultureExtensionCode : null);
            if (languageCode != null)
                element.Add(new XElement("LanguageID", languageCode));
            if (_LanguageName != null)
                element.Add(_LanguageName.GetElement("LanguageName"));
            if (_CharacterBased)
                element.Add(new XElement("CharacterBased", _CharacterBased.ToString()));
            if (_ReadTopToBottom)
                element.Add(new XElement("ReadTopToBottom", _ReadTopToBottom.ToString()));
            if (_ReadRightToLeft)
                element.Add(new XElement("ReadRightToLeft", _ReadRightToLeft.ToString()));
            if (_PreferedFontName != null)
                element.Add(new XElement("PreferedFontName", _PreferedFontName));
            if (_DictionaryFontSize != null)
                element.Add(new XElement("DictionaryFontSize", _DictionaryFontSize));
            if ((_VoiceNames != null) && (_VoiceNames.Count() != 0))
                element.Add(new XElement("VoiceNames", ObjectUtilities.GetStringFromStringList(_VoiceNames)));
            if (_PreferedVoiceName != null)
                element.Add(new XElement("PreferedVoiceName", _PreferedVoiceName));
            if (_GoogleSpeechToTextSupported)
                element.Add(new XElement("GoogleSpeechToTextSupported", "True"));
            if (_GoogleTextToSpeechSupported)
                element.Add(new XElement("GoogleTextToSpeechSupported", "True"));
            if (_UseGoogleTextToSpeech)
                element.Add(new XElement("UseGoogleTextToSpeech", "True"));
            if (!String.IsNullOrEmpty(_Owner))
                element.Add(new XElement("Owner", _Owner));
            if (_LongestDictionaryEntryLength != 0)
                element.Add(new XElement("LongestDictionaryEntryLength", _LongestDictionaryEntryLength));
            if (_LongestPrefixLength != 0)
                element.Add(new XElement("LongestPrefixLength", _LongestPrefixLength));
            if (_LongestSuffixLength != 0)
                element.Add(new XElement("LongestSuffixLength", _LongestSuffixLength));
            if (_LongestInflectionLength != 0)
                element.Add(new XElement("LongestInflectionLength", _LongestInflectionLength));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageIDNoAdd(childElement.Value.Trim());
                    break;
                case "LanguageName":
                    LanguageName = new MultiLanguageString(childElement);
                    break;
                case "CharacterBased":
                    CharacterBased = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "ReadTopToBottom":
                    ReadTopToBottom = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "ReadRightToLeft":
                    ReadRightToLeft = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "PreferedFontName":
                    PreferedFontName = childElement.Value.Trim();
                    break;
                case "DictionaryFontSize":
                    DictionaryFontSize = childElement.Value.Trim();
                    break;
                case "VoiceNames":
                    VoiceNames = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "PreferedVoiceName":
                    PreferedVoiceName = childElement.Value.Trim();
                    break;
                case "GoogleSpeechToTextSupported":
                    GoogleSpeechToTextSupported = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "GoogleTextToSpeechSupported":
                    GoogleTextToSpeechSupported = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "UseGoogleTextToSpeech":
                    UseGoogleTextToSpeech = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "Owner":
                    Owner = childElement.Value.Trim();
                    break;
                case "LongestDictionaryEntryLength":
                    LongestDictionaryEntryLength = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "LongestPrefixLength":
                    LongestPrefixLength = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "LongestSuffixLength":
                    LongestSuffixLength = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "LongestInflectionLength":
                    LongestInflectionLength = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            LanguageDescription otherLanguageDescription = other as LanguageDescription;

            if (otherLanguageDescription == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = LanguageID.Compare(_LanguageID, otherLanguageDescription.LanguageID);
            if (diff != 0)
                return diff;
            diff = MultiLanguageString.Compare(_LanguageName, otherLanguageDescription.LanguageName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_CharacterBased, otherLanguageDescription.CharacterBased);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_ReadTopToBottom, otherLanguageDescription.ReadTopToBottom);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_ReadRightToLeft, otherLanguageDescription.ReadRightToLeft);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_PreferedFontName, otherLanguageDescription.PreferedFontName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_DictionaryFontSize, otherLanguageDescription.DictionaryFontSize);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_VoiceNames, otherLanguageDescription.VoiceNames);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_PreferedVoiceName, otherLanguageDescription.PreferedVoiceName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherLanguageDescription.Owner);
            if (diff != 0)
                return diff;
            diff = _LongestDictionaryEntryLength - otherLanguageDescription.LongestDictionaryEntryLength;
            if (diff != 0)
                return diff;
            diff = _LongestPrefixLength - otherLanguageDescription.LongestPrefixLength;
            if (diff != 0)
                return diff;
            diff = _LongestSuffixLength - otherLanguageDescription.LongestSuffixLength;
            if (diff != 0)
                return diff;
            diff = _LongestInflectionLength - otherLanguageDescription.LongestInflectionLength;
            return diff;
        }

        public static int Compare(BaseObjectLanguage object1, BaseObjectLanguage object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static List<LanguageDescription> CopyList(List<LanguageDescription> languageDescriptions)
        {
            if (languageDescriptions == null)
                return null;
            return new List<LanguageDescription>(languageDescriptions);
        }

        public static void Merge(
            LanguageDescription ld1,
            LanguageDescription ld2)
        {
            if (MultiLanguageString.Compare(ld1.LanguageName, ld2.LanguageName) != 0)
                MultiLanguageString.MergeOrCreate(ref ld1._LanguageName, ref ld2._LanguageName);

            if (ObjectUtilities.CompareBools(ld1.CharacterBased, ld2.CharacterBased) != 0)
                ld1.CharacterBased = ld2.CharacterBased = true;

            if (ObjectUtilities.CompareBools(ld1.ReadTopToBottom, ld2.ReadTopToBottom) != 0)
                ld1.ReadTopToBottom = ld2.ReadTopToBottom = true;

            if (ObjectUtilities.CompareBools(ld1.ReadRightToLeft, ld2.ReadRightToLeft) != 0)
                ld1.ReadRightToLeft = ld2.ReadRightToLeft = true;

            if (ObjectUtilities.CompareStrings(ld1.PreferedFontName, ld2.PreferedFontName) != 0)
            {
                if (String.IsNullOrEmpty(ld1.PreferedFontName))
                    ld1.PreferedFontName = ld2.PreferedFontName;
                else if (String.IsNullOrEmpty(ld2.PreferedFontName))
                    ld2.PreferedFontName = ld1.PreferedFontName;
            }

            if (ObjectUtilities.CompareStrings(ld1.DictionaryFontSize, ld2.DictionaryFontSize) != 0)
            {
                if (String.IsNullOrEmpty(ld1.DictionaryFontSize))
                    ld1.DictionaryFontSize = ld2.DictionaryFontSize;
                else if (String.IsNullOrEmpty(ld2.DictionaryFontSize))
                    ld2.DictionaryFontSize = ld1.DictionaryFontSize;
            }

            if (ObjectUtilities.CompareStringLists(ld1.VoiceNames, ld2.VoiceNames) != 0)
            {
                List<string> voiceNames = ObjectUtilities.ListConcatenateUnique(ld1.VoiceNames, ld2.VoiceNames);
                if (ObjectUtilities.CompareStringLists(ld1.VoiceNames, voiceNames) != 0)
                    ld1.VoiceNames = voiceNames;
                if (ObjectUtilities.CompareStringLists(ld2.VoiceNames, voiceNames) != 0)
                    ld2.VoiceNames = voiceNames;
            }

            if (ObjectUtilities.CompareStrings(ld1.PreferedVoiceName, ld2.PreferedVoiceName) != 0)
            {
                if (String.IsNullOrEmpty(ld1.PreferedVoiceName))
                    ld1.PreferedVoiceName = ld2.PreferedVoiceName;
                else if (String.IsNullOrEmpty(ld2.PreferedVoiceName))
                    ld2.PreferedVoiceName = ld1.PreferedVoiceName;
            }

            if (ld1.LongestDictionaryEntryLength != ld2.LongestDictionaryEntryLength)
            {
                if (ld1.LongestDictionaryEntryLength == 0)
                    ld1.LongestDictionaryEntryLength = ld2.LongestDictionaryEntryLength;
                else if (ld2.LongestDictionaryEntryLength == 0)
                    ld2.LongestDictionaryEntryLength = ld1.LongestDictionaryEntryLength;
            }

            if (ld1.LongestPrefixLength != ld2.LongestPrefixLength)
            {
                if (ld1.LongestPrefixLength == 0)
                    ld1.LongestPrefixLength = ld2.LongestPrefixLength;
                else if (ld2.LongestPrefixLength == 0)
                    ld2.LongestPrefixLength = ld1.LongestPrefixLength;
            }

            if (ld1.LongestSuffixLength != ld2.LongestSuffixLength)
            {
                if (ld1.LongestSuffixLength == 0)
                    ld1.LongestSuffixLength = ld2.LongestSuffixLength;
                else if (ld2.LongestSuffixLength == 0)
                    ld2.LongestSuffixLength = ld1.LongestSuffixLength;
            }

            if (ld1.LongestInflectionLength != ld2.LongestInflectionLength)
            {
                if (ld1.LongestInflectionLength == 0)
                    ld1.LongestInflectionLength = ld2.LongestInflectionLength;
                else if (ld2.LongestInflectionLength == 0)
                    ld2.LongestInflectionLength = ld1.LongestInflectionLength;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class AudioInstance : BaseObjectKeyed
    {
        protected string _Owner;
        protected string _MimeType;
        protected string _FileName;
        protected string _SourceName;
        protected string _SourceUrl;
        public List<KeyValuePair<string, string>> _Attributes;
        protected List<string> _Tags;
        protected int _UseVotes;
        protected int _DontUseVotes;
        protected int _SourceVotes;

        // Source names.
        public static string SynthesizedSourceName = "Synthesized";
        public static string ForvoSourceName = "Forvo";
        public static string RecordedSourceName = "Recorded";
        public static string UploadedSourceName = "Uploaded";
        public static string UnknownSourceName = "Unknown";
        public static string DontCareSourceName = "DontCare";
        public static string[] SourceNames =
        {
            "Synthesized",
            "Recorded",
            "Uploaded",
            "Forvo",
            "Unknown"
        };

        // Attribute names.
        public static string Gender = "Gender";                 // "Male", "Female"
        public static string Region = "Region";                 // "British", "American", "Spain", "Latin America", "Other", ...
        public static string Country = "Country";               // "United States", "Spain", "Mexico", ...
        public static string Accent = "Accent";                 // Regional accent within a country.
        public static string Dialect = "Dialect";               // Dialect withing a country that doesn't have language code.
        public static string Age = "Age";                       // "Child", "Teenager", "Adult", "Senior"
        public static string Speaker = "Speaker";               // Speaker, Forvo user name, or synthesized voice name.
        public static string Speed = "Speed";                   // Speed, "Fast", "Normal", "Slow", or synthesier speed number.
        public static string[] AttributeNames =
        {
            "Gender",
            "Region",
            "Country",
            "Accent",
            "Dialect",
            "Age",
            "Speaker",
            "Speed"
        };

        public AudioInstance(
                string name,
                string owner,
                string mimeType,
                string fileName,
                string sourceName,
                List<KeyValuePair<string, string>> attributes)
            : base(name)
        {
            ClearAudioInstance();
            _Owner = owner;
            _MimeType = mimeType;
            _FileName = fileName;
            _SourceName = sourceName;
            _Attributes = attributes;
        }

        public AudioInstance(AudioInstance other)
            : base(other)
        {
            CopyAudioInstance(other);
            ModifiedFlag = false;
        }

        public AudioInstance(XElement element)
        {
            OnElement(element);
        }

        public AudioInstance()
        {
            ClearAudioInstance();
        }

        public override void Clear()
        {
            base.Clear();
            ClearAudioInstance();
        }

        public void ClearAudioInstance()
        {
            _Owner = null;
            _MimeType = null;
            _FileName = null;
            _SourceName = null;
            _SourceUrl = null;
            _Attributes = null;
            _Tags = null;
            _UseVotes = 0;
            _DontUseVotes = 0;
            _SourceVotes = 0;
        }

        public virtual void CopyAudioInstance(AudioInstance other)
        {
            _Owner = other.Owner;
            _MimeType = other.MimeType;
            _FileName = other.FileName;
            _SourceName = other.SourceName;
            _SourceUrl = other.SourceUrl;
            _Attributes = other.CloneAttributes();
            _Tags = other.Tags;
            _UseVotes = other.UseVotes;
            _DontUseVotes = other.DontUseVotes;
            _SourceVotes = other.SourceVotes;
        }

        public override IBaseObject Clone()
        {
            return new AudioInstance(this);
        }

        public List<KeyValuePair<string, string>> CloneAttributes()
        {
            if (_Attributes == null)
                return null;

            List<KeyValuePair<string, string>> attributes = new List<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, string> kvp in _Attributes)
                attributes.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));

            return attributes;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Name=");
            sb.Append(Name);
            sb.Append(", ");

            sb.Append("Owner=");
            sb.Append(Owner);
            sb.Append(", ");

            sb.Append("MimeType=");
            sb.Append(MimeType);
            sb.Append(", ");

            sb.Append("FileName=");
            sb.Append(FileName);
            sb.Append(", ");

            sb.Append("SourceName=");
            sb.Append(SourceName);
            sb.Append(", ");

            if (_Attributes != null)
            {
                foreach (KeyValuePair<string, string> kvp in _Attributes)
                {
                    sb.Append(", ");
                    sb.Append(kvp.Key);
                    sb.Append("=");
                    sb.Append(kvp.Value);
                }
            }

            if ((_Tags != null) && (_Tags.Count() != 0))
            {
                sb.Append(", ");
                sb.Append("Tags=");
                sb.Append(ObjectUtilities.GetStringFromStringList(_Tags));
            }

            sb.Append("UseVotes=");
            sb.Append(_UseVotes);
            sb.Append(", ");

            sb.Append("DontUseVotes=");
            sb.Append(_DontUseVotes);
            sb.Append(", ");

            sb.Append("SourceVotes=");
            sb.Append(_SourceVotes);
            sb.Append(", ");

            sb.Append("SourceUrl=");
            sb.Append(_SourceUrl);

            return sb.ToString();
        }

        public override string Name
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

        public string MimeType
        {
            get
            {
                return _MimeType;
            }
            set
            {
                if (value != _MimeType)
                {
                    _MimeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int GetOrdinalFromFileName()
        {
            if (String.IsNullOrEmpty(_FileName))
                return -1;

            string baseName = MediaUtilities.RemoveFileExtension(_FileName);
            int index = baseName.IndexOf('_');

            if (index >= 0)
            {
                string ordinalString = baseName.Substring(index + 1);
                return ObjectUtilities.GetIntegerFromString(ordinalString, -1);
            }

            return -1;
        }

        public String GetTildeUrl(LanguageID languageID)
        {
            string audioUrl = MediaUtilities.GetAudioTildeDirectoryUrl(languageID) + "/" + _FileName;
            return audioUrl;
        }

        public String GetFilePath(LanguageID languageID)
        {
            string audioUrl = GetTildeUrl(languageID);
            string audioFilePath = ApplicationData.MapToFilePath(audioUrl);
            return audioFilePath;
        }

        public bool Exists(LanguageID languageID)
        {
            string audioFilePath = GetFilePath(languageID);
            bool exists = FileSingleton.Exists(audioFilePath);
            return exists;
        }

        public List<KeyValuePair<string, string>> Attributes
        {
            get
            {
                return _Attributes;
            }
            set
            {
                if (value != _Attributes)
                {
                    _Attributes = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetAttributeValueIndexed(int index)
        {
            if (_Attributes == null)
                return String.Empty;

            if ((index < 0) || (index >= _Attributes.Count()))
                return String.Empty;

            return _Attributes[index].Value;
        }

        public string GetAttributeNameIndexed(int index)
        {
            if (_Attributes == null)
                return String.Empty;

            if ((index < 0) || (index >= _Attributes.Count()))
                return String.Empty;

            return _Attributes[index].Key;
        }

        public string GetAttribute(string key)
        {
            if (_Attributes == null)
                return String.Empty;

            KeyValuePair<string, string> kvp = _Attributes.FirstOrDefault(x => x.Key == key);

            if (kvp.Equals(default(KeyValuePair<string, string>)))
                return String.Empty;

            return kvp.Value;
        }

        public void SetAttribute(string key, string value)
        {
            if (_Attributes == null)
            {
                _Attributes = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(key, value) };
                ModifiedFlag = true;
                return;
            }

            int c = _Attributes.Count();

            for (int i = 0; i < c; i++)
            {
                if (_Attributes[i].Key == key)
                {
                    _Attributes[i] = new KeyValuePair<string, string>(key, value);
                    ModifiedFlag = true;
                    return;
                }
            }

            _Attributes.Add(new KeyValuePair<string, string>(key, value));
            ModifiedFlag = true;
        }

        public void DeleteAttribute(string key)
        {
            if (_Attributes == null)
                return;

            int c = _Attributes.Count();

            for (int i = 0; i < c; i++)
            {
                if (_Attributes[i].Key == key)
                {
                    _Attributes.RemoveAt(i);
                    ModifiedFlag = true;
                    return;
                }
            }
        }

        public void SetAttributes(List<KeyValuePair<string, string>> attributes)
        {
            if (attributes == null)
                return;

            foreach (KeyValuePair<string, string> attribute in attributes)
                SetAttribute(attribute.Key, attribute.Value);
        }

        public bool MatchAttribute(string key, string value)
        {
            if (String.IsNullOrEmpty(key))
                return false;

            string theValue = GetAttribute(key);

            if (!TextUtilities.IsEqualStringsIgnoreCase(value, theValue))
                return false;

            return true;
        }

        public bool MatchAttributesExact(List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((attributePatterns == null) || (attributePatterns.Count() == 0))
                return true;

            foreach (KeyValuePair<string, string> kvp in attributePatterns)
            {
                string value = GetAttribute(kvp.Key);

                if (!TextUtilities.IsEqualStringsIgnoreCase(value, kvp.Value))
                    return false;
            }

            return true;
        }

        public bool MatchAttributesPresent(List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((attributePatterns == null) || (attributePatterns.Count() == 0))
                return true;

            foreach (KeyValuePair<string, string> kvp in attributePatterns)
            {
                string pattern = kvp.Value;

                if (String.IsNullOrEmpty(pattern))
                    continue;

                string value = GetAttribute(kvp.Key);

                if (!String.IsNullOrEmpty(value))
                {
                    if (!TextUtilities.IsEqualStringsIgnoreCase(value, pattern))
                        return false;
                }
            }

            return true;
        }

        public int AttributeCount()
        {
            if (_Attributes == null)
                return 0;

            return _Attributes.Count();
        }

        public bool HasAttributes()
        {
            if ((_Attributes == null) || (_Attributes.Count() == 0))
                return false;

            return true;
        }

        public string AttributesLabel(LanguageUtilities languageUtilities)
        {
            string text = String.Empty;

            if (HasAttributes())
            {
                foreach (KeyValuePair<string, string> attribute in _Attributes)
                {
                    if (!String.IsNullOrEmpty(text))
                        text += ", ";

                    string value = attribute.Value;

                    if (languageUtilities != null)
                        text += languageUtilities.TranslateUIString(value);
                    else
                        text += value;
                }

                if (!String.IsNullOrEmpty(_SourceName))
                {
                    if (!String.IsNullOrEmpty(text))
                        text += ", ";

                    if (languageUtilities != null)
                        text += languageUtilities.TranslateUIString(_SourceName);
                    else
                        text += _SourceName;
                }
            }

            return text;
        }

        public string SourceName
        {
            get
            {
                return _SourceName;
            }
            set
            {
                if (value != _SourceName)
                {
                    _SourceName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SourceUrl
        {
            get
            {
                return _SourceUrl;
            }
            set
            {
                if (value != _SourceUrl)
                {
                    _SourceUrl = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> Tags
        {
            get
            {
                return _Tags;
            }
            set
            {
                if (value != _Tags)
                {
                    _Tags = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetTagIndexed(int index)
        {
            if (_Tags == null)
                return String.Empty;

            if ((index < 0) || (index >= _Tags.Count()))
                return String.Empty;

            return _Tags[index];
        }

        public bool IsTagged(string tag)
        {
            if (_Tags == null)
                return false;

            return _Tags.Contains(tag);
        }

        public bool IsAnyTagged(List<string> tagsList)
        {
            if (_Tags == null)
                return false;

            if ((tagsList == null) || (tagsList.Count() == 0))
                return false;

            foreach (string tag in tagsList)
            {
                if (_Tags.Contains(tag))
                    return true;
            }

            return false;
        }

        public void SetTag(string tag)
        {
            if (_Tags == null)
            {
                _Tags = new List<string>() { tag };
                ModifiedFlag = true;
                return;
            }

            if (!_Tags.Contains(tag))
            {
                _Tags.Add(tag);
                ModifiedFlag = true;
            }
        }

        public void SetTags(string tags)
        {
            List<string> tagsList = ObjectUtilities.GetStringListFromString(tags);

            if (_Tags == null)
            {
                _Tags = tagsList;
                ModifiedFlag = true;
                return;
            }

            foreach (string tag in tagsList)
                SetTag(tag);
        }

        public void SetTags(List<string> tagsList)
        {
            if (tagsList == null)
                return;

            foreach (string tag in tagsList)
                SetTag(tag);
        }

        public void SortTags()
        {
            if ((_Tags != null) && (_Tags.Count() > 1))
                _Tags.Sort();
        }

        public void DeleteTag(string tag)
        {
            if (_Tags == null)
                return;

            if (_Tags.Contains(tag))
            {
                _Tags.Remove(tag);
                ModifiedFlag = true;
            }
        }

        public void DeleteTags(List<string> tagsList)
        {
            if (tagsList == null)
                return;

            foreach (string tag in tagsList)
                DeleteTag(tag);
        }

        public void DeleteAllTags()
        {
            if (_Tags == null)
                return;

            _Tags = null;
            ModifiedFlag = true;
        }

        public int TagCount()
        {
            if (_Tags == null)
                return 0;

            return _Tags.Count();
        }

        public bool HasTags()
        {
            if ((_Tags == null) || (_Tags.Count() == 0))
                return false;

            return true;
        }

        public string TagsLabel(LanguageUtilities languageUtilities)
        {
            string text = String.Empty;

            if (HasTags())
                text = ObjectUtilities.GetStringFromStringList(_Tags);

            return text;
        }

        public int UseVotes
        {
            get
            {
                return _UseVotes;
            }
            set
            {
                if (value != _UseVotes)
                {
                    _UseVotes = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int DontUseVotes
        {
            get
            {
                return _DontUseVotes;
            }
            set
            {
                if (value != _DontUseVotes)
                {
                    _DontUseVotes = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int SourceVotes
        {
            get
            {
                return _SourceVotes;
            }
            set
            {
                if (value != _SourceVotes)
                {
                    _SourceVotes = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(_Owner))
                element.Add(new XAttribute("Owner", _Owner));
            if (!String.IsNullOrEmpty(_MimeType))
                element.Add(new XAttribute("MimeType", _MimeType));
            if (!String.IsNullOrEmpty(_FileName))
                element.Add(new XAttribute("FileName", _FileName));
            if (!String.IsNullOrEmpty(_SourceName))
                element.Add(new XAttribute("SourceName", _SourceName));
            if (!String.IsNullOrEmpty(_SourceUrl))
                element.Add(new XElement("SourceUrl", _SourceUrl));
            if ((_Attributes != null) && (_Attributes.Count() != 0))
            {
                foreach (KeyValuePair<string, string> kvp in _Attributes)
                {
                    XElement attributeElement = new XElement("Attribute");
                    attributeElement.Add(new XAttribute("Key", kvp.Key));
                    attributeElement.Add(new XAttribute("Value", kvp.Value));
                    element.Add(attributeElement);
                }
            }
            if ((_Tags != null) && (_Tags.Count() != 0))
            {
                XElement tagsElement = new XElement("Tags", ObjectUtilities.GetStringFromStringList(_Tags));
                element.Add(tagsElement);
            }
            if (_UseVotes != 0)
                element.Add(new XAttribute("UseVotes", _UseVotes));
            if (_DontUseVotes != 0)
                element.Add(new XAttribute("DontUseVotes", _DontUseVotes));
            if (_SourceVotes != 0)
                element.Add(new XAttribute("SourceVotes", _SourceVotes));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":    // Legacy
                    Key = attributeValue;
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                case "MimeType":
                    _MimeType = attributeValue;
                    break;
                case "FileName":
                    _FileName = attributeValue;
                    break;
                case "SourceName":
                    _SourceName = attributeValue;
                    break;
                case "UseVotes":
                    _UseVotes = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "DontUseVotes":
                    _DontUseVotes = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "SourceVotes":
                    _SourceVotes = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Attribute":
                    {
                        string key = childElement.Attribute("Key").Value.Trim();
                        string value = childElement.Attribute("Value").Value.Trim();
                        SetAttribute(key, value);
                    }
                    break;
                case "Tags":
                    _Tags = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "SourceUrl":
                    _SourceUrl = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            AudioInstance otherAudio = other as AudioInstance;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherAudio.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_MimeType, otherAudio.MimeType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_FileName, otherAudio.FileName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_SourceName, otherAudio.SourceName);
            return diff;
        }

        public static int Compare(AudioInstance object1, AudioInstance object2)
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

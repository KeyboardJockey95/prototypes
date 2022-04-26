using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Media
{
    public class MediaInstance : BaseObjectKeyed
    {
        public string _Name;
        public string _Owner;
        public string _MimeType;
        public string _FileName;
        public string _SourceName;
        public List<KeyValuePair<string, string>> _Attributes;

        // Source names.
        public static string SynthesizedSourceName = "Synthesized";
        public static string ForvoSourceName = "Forvo";
        public static string RecordedSourceName = "Recorded";
        public static string UploadedSourceName = "Uploaded";
        public static string DontCareSourceName = "DontCare";

        // Attribute names.
        public static string Gender = "Gender";     // "Male", "Female"
        public static string Region = "Region";     // "British", "American", "Spain", "Latin America", "Other", ...
        public static string Country = "Country";   // "United States", "Spain", "Mexico", ...
        public static string Accent = "Accent";     // Regional accent within a country.
        public static string Dialect = "Dialect";   // Dialect withing a country that doesn't have language code.
        public static string Age = "Age";           // "Child", "Teenager", "Adult", "Senior"
        public static string[] AttributeNames =
        {
            "Gender",
            "Region",
            "Country",
            "Accent",
            "Dialect",
            "Age"
        };

        public MediaInstance(
                string name,
                string owner,
                string mimeType,
                string fileName,
                string sourceName,
                List<KeyValuePair<string, string>> attributes)
            : base(name)
        {
            _Name = name;
            _Owner = owner;
            _MimeType = mimeType;
            _FileName = fileName;
            _SourceName = sourceName;
            _Attributes = attributes;
        }

        public MediaInstance(MediaInstance other)
            : base(other)
        {
            CopyMediaInstance(other);
            _Modified = false;
        }

        public MediaInstance(XElement element)
        {
            OnElement(element);
        }

        public MediaInstance()
        {
            ClearMediaInstance();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMediaInstance();
        }

        public void ClearMediaInstance()
        {
            _Name = null;
            _Owner = null;
            _MimeType = null;
            _FileName = null;
            _SourceName = null;
            _Attributes = null;
        }

        public virtual void CopyMediaInstance(MediaInstance other)
        {
            _Name = other.Name;
            _Owner = other.Owner;
            _MimeType = other.MimeType;
            _FileName = other.FileName;
            _SourceName = other.SourceName;
            _Attributes = other.CloneAttributes();
        }

        public override IBaseObject Clone()
        {
            return new MediaInstance(this);
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

            return sb.ToString();
        }

        public override string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    _Modified = true;
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
                    _Modified = true;
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
                    _Modified = true;
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
                    _Modified = true;
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
                    _Modified = true;
                }
            }
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
                    _Modified = true;
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
                _Modified = true;
                return;
            }

            int c = _Attributes.Count();

            for (int i = 0; i < c; i++)
            {
                if (_Attributes[i].Key == key)
                {
                    _Attributes[i] = new KeyValuePair<string, string>(key, value);
                    _Modified = true;
                    return;
                }
            }

            _Attributes.Add(new KeyValuePair<string, string>(key, value));
            _Modified = true;
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
                    _Modified = true;
                    return;
                }
            }
        }

        public bool MatchAttributesExact(List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((attributePatterns == null) || (attributePatterns.Count() == 0))
                return true;

            foreach (KeyValuePair<string, string> kvp in attributePatterns)
            {
                string value = GetAttribute(kvp.Key);

                if (value != kvp.Value)
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
                string value = GetAttribute(kvp.Key);

                if (!String.IsNullOrEmpty(value))
                {
                    if (value != kvp.Value)
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

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(_Name))
                element.Add(new XAttribute("Name", _Name));
            if (!String.IsNullOrEmpty(_Owner))
                element.Add(new XAttribute("Owner", _Owner));
            if (!String.IsNullOrEmpty(_MimeType))
                element.Add(new XAttribute("MimeType", _MimeType));
            if (!String.IsNullOrEmpty(_FileName))
                element.Add(new XAttribute("FileName", _FileName));
            if (!String.IsNullOrEmpty(_SourceName))
                element.Add(new XAttribute("SourceName", _SourceName));
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
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    _Name = attributeValue;
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
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            MediaInstance otherAudio = other as MediaInstance;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Name, otherAudio.Name);
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

        public static int Compare(MediaInstance object1, MediaInstance object2)
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

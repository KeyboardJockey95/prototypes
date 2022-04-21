using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Media
{
    public class Voice : BaseObjectLanguage
    {
        public string _SourceName;                                  // "Windows", "Google"
        public List<KeyValuePair<string, string>> _Attributes;      // Same as for AudioReference.

        public static string WindowsSourceName = "Windows";
        public static string GoogleSourceName = "Google";

        public Voice(
                string name,
                LanguageID languageID,
                string sourceName,
                List<KeyValuePair<string, string>> attributes = null)
            : base(name, languageID)
        {
            _SourceName = sourceName;
            _Attributes = attributes;
        }

        public Voice(Voice other)
            : base(other)
        {
            CopyVoice(other);
            ModifiedFlag = false;
        }

        public Voice(XElement element)
        {
            OnElement(element);
        }

        public Voice()
        {
            ClearVoice();
        }

        public override void Clear()
        {
            base.Clear();
            ClearVoice();
        }

        public void ClearVoice()
        {
            _SourceName = null;
            _Attributes = null;
        }

        public void CopyVoice(Voice other)
        {
            _SourceName = other.SourceName;
            _Attributes = other.CloneAttributes();
        }

        public override IBaseObject Clone()
        {
            return new Voice(this);
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

            if (_Attributes != null)
            {
                foreach (KeyValuePair<string, string> kvp in _Attributes)
                {
                    sb.Append(kvp.Key);
                    sb.Append("=");
                    sb.Append(kvp.Value);
                    sb.Append(", ");
                }
            }

            sb.Append("SourceName=");
            sb.Append(SourceName);

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
                if (value != KeyString)
                    Key = value;
            }
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

        public bool HasAttributes()
        {
            if ((_Attributes != null) && (_Attributes.Count() != 0))
                return true;

            return false;
        }

        public bool HasAttribute(string key)
        {
            if ((_Attributes != null) && (_Attributes.Count() != 0))
            {
                KeyValuePair<string, string> kvp = _Attributes.FirstOrDefault(x => x.Key == key);

                if (!kvp.Equals(default(KeyValuePair<string, string>)))
                    return true;
            }

            return false;
        }

        public string GetAttribute(string key)
        {
            if (_Attributes == null)
                return null;

            KeyValuePair<string, string> kvp = _Attributes.FirstOrDefault(x => x.Key == key);

            if (kvp.Equals(default(KeyValuePair<string, string>)))
                return null;

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

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
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
            Voice otherAudio = other as Voice;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_SourceName, otherAudio.SourceName);
            return diff;
        }

        public static int Compare(Voice object1, Voice object2)
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

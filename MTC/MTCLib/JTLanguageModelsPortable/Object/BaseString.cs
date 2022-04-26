using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class BaseString : IBaseObjectKeyed
    {
        protected Type _KeyType;
        protected object _Key;
        protected string _Text;
        protected bool _Modified;
        protected DateTime _CreationTime;
        protected DateTime _ModifiedTime;

        public BaseString(object key, string text)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            if (text == null)
                _Text = String.Empty;
            else
                _Text = text;
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public BaseString(string text)
        {
            if (text == null)
                _Key = String.Empty;
            else
                _Key = text;
            _KeyType = _Key.GetType();
            if (text == null)
                _Text = String.Empty;
            else
                _Text = text;
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public BaseString(BaseString other)
        {
            Copy(other);
        }

        public BaseString(object key, BaseString other)
        {
            _Key = key;
            if (_Key != null)
                _KeyType = _Key.GetType();
            else
                _KeyType = null;
            _Text = other.Text;
            _Modified = false;
            _CreationTime = other.CreationTime;
            _ModifiedTime = other.ModifiedTime;
        }

        public BaseString(XElement element)
        {
            OnElement(element);
        }

        public BaseString()
        {
            ClearBaseString();
        }

        public virtual void Clear()
        {
            ClearBaseString();
        }

        public virtual void ClearBaseString()
        {
            _KeyType = null;
            _Key = null;
            _Text = String.Empty;
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public virtual void Copy(BaseString other)
        {
            if (other != null)
                _Key = other.Key;
            else
                _Key = null;
            if (_Key != null)
                _KeyType = _Key.GetType();
            else
                _KeyType = null;
            if (other != null)
                _Text = other.Text;
            else
                _Text = String.Empty;
            _Modified = false;
            if (other != null)
            {
                _CreationTime = other.CreationTime;
                _ModifiedTime = other.ModifiedTime;
            }
            else
            {
                _CreationTime = DateTime.MinValue;
                _ModifiedTime = DateTime.MinValue;
            }
        }

        public virtual void CopyDeep(BaseString other)
        {
            this.Copy(other);
        }

        public virtual IBaseObject Clone()
        {
            return new BaseString(this);
        }

        public Type KeyType
        {
            get
            {
                return _KeyType;
            }
            set
            {
                _KeyType = value;
            }
        }

        public bool IsIntegerKeyType
        {
            get
            {
                return ObjectUtilities.IsIntegerType(_KeyType);
            }
        }

        public virtual object Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (ObjectUtilities.CompareObjects(_Key, value) != 0)
                {
                    _Modified = true;
                    _Key = value;
                    if (_Key != null)
                        _KeyType = _Key.GetType();
                    else
                        _KeyType = null;
                }
            }
        }

        public void SetKeyNoModify(object key)
        {
            _Key = key;
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
        }

        public void ResetKeyNoModify()
        {
            _Key = BaseObjectKeyed.GetResetKeyValue(_KeyType);
        }

        public string KeyString
        {
            get
            {
                if (_Key == null)
                    return String.Empty;
                else
                    return _Key.ToString();
            }
        }

        public int KeyInt
        {
            get
            {
                if ((_Key != null) && IsIntegerKeyType)
                    return (int)_Key;
                return 0;
            }
        }

        public virtual string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (value == null)
                    value = String.Empty;

                if (value != _Text)
                    _Modified = true;

                _Text = value;
            }
        }

        public string TextPreview(int length, string ellipsis)
        {
            string text;

            if (TextLength > length)
            {
                text = _Text.Substring(0, length);

                if (!String.IsNullOrEmpty(ellipsis))
                    text += ellipsis;
            }
            else
                text = _Text;

            return text;
        }

        public string TextLower
        {
            get
            {
                if (_Text != null)
                    return _Text.ToLower();
                return String.Empty;
            }
        }

        public int TextLength
        {
            get
            {
                string text = Text;
                if (text == null)
                    return 0;
                return text.Length;
            }
        }

        public bool HasText()
        {
            return !String.IsNullOrEmpty(Text);
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(Text);
        }

        public virtual string Name
        {
            get
            {
                return KeyString;
            }
            set
            {
            }
        }

        public virtual string TypeLabel
        {
            get
            {
                return "String";
            }
        }

        public virtual Guid Guid
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
            }
        }

        public virtual string GuidString
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public virtual bool EnsureGuid()
        {
            return true;
        }

        public virtual void NewGuid()
        {
        }

        public virtual string Owner
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public virtual string Source
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public virtual bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return _CreationTime;
            }
            set
            {
                if (_CreationTime != value)
                {
                    _CreationTime = value;
                    _Modified = true;
                }
            }
        }

        public DateTime ModifiedTime
        {
            get
            {
                return _ModifiedTime;
            }
            set
            {
                if (_ModifiedTime != value)
                {
                    _ModifiedTime = value;
                    _Modified = true;
                }
            }
        }

        public void Touch()
        {
            if (_CreationTime == DateTime.MinValue)
            {
                CreationTime = DateTime.UtcNow;
                ModifiedTime = _CreationTime;
            }
            else
                ModifiedTime = DateTime.UtcNow;
        }

        public void TouchAndClearModified()
        {
            Touch();
            Modified = false;
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name, _Text);

            if (_KeyType != null)
                element.Add(new XAttribute("KeyType", _KeyType.Name));

            if (_Key != null)
                element.Add(new XAttribute("Key", _Key.ToString()));

            if (_CreationTime != DateTime.MinValue)
                element.Add(new XAttribute("CreationTime", _CreationTime.ToString()));

            if (_ModifiedTime != DateTime.MinValue)
                element.Add(new XAttribute("ModifiedTime", _ModifiedTime.ToString()));

            return element;
        }

        public virtual XElement GetElementTextElement(string name)
        {
            XElement element = new XElement(name);

            if (_KeyType != null)
                element.Add(new XAttribute("KeyType", _KeyType.Name));

            if (_Key != null)
                element.Add(new XAttribute("Key", _Key.ToString()));

            if (_Text != null)
                element.Add(new XElement("Text", _Text));

            if (_CreationTime != DateTime.MinValue)
                element.Add(new XAttribute("CreationTime", ObjectUtilities.GetStringFromDateTime(_CreationTime)));

            if (_ModifiedTime != DateTime.MinValue)
                element.Add(new XAttribute("ModifiedTime", ObjectUtilities.GetStringFromDateTime(_ModifiedTime)));

            return element;
        }

        public virtual void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            _Text = element.Value;

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }

            Modified = false;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "KeyType":
                    KeyType = ObjectUtilities.GetTypeFromString(attributeValue);
                    break;
                case "Key":
                    if (_KeyType != null)
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, _KeyType.Name);
                    else
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, null);
                    break;
                case "LanguageID":
                    // Ignore, as it might come from a LanguageString used as a BaseString.
                    break;
                case "CreationTime":
                    _CreationTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                case "ModifiedTime":
                    _ModifiedTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Text":
                    _Text = childElement.Value;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual XElement Xml
        {
            get
            {
                return GetElement(GetType().Name);
            }
            set
            {
                OnElement(value);
            }
        }

        public virtual string StringData
        {
            get
            {
                XElement element = Xml;
                string xmlString = element.ToString();
                return xmlString;
            }
            set
            {
                XElement element = XElement.Parse(value, LoadOptions.PreserveWhitespace);
                Xml = element;
            }
        }

        public virtual byte[] BinaryData
        {
            get
            {
                XElement element = Xml;
                string xmlString = element.ToString();
                byte[] data = ApplicationData.Encoding.GetBytes(xmlString);
                return data;
            }
            set
            {
                string xmlString = ApplicationData.Encoding.GetString(value, 0, value.Count());
                XElement element = XElement.Parse(xmlString, LoadOptions.PreserveWhitespace);
                Xml = element;
            }
        }

        public void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
        }

        public override string ToString()
        {
            return Text;
        }

        public bool FromString(string value)
        {
            Text = value;
            return true;
        }

        public override int GetHashCode()
        {
            return _Key.GetHashCode();
        }

        public void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Full:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString + ": " + Text);
                    else
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, Text);
                    break;
                case DisplayDetail.Xml:
                    {
                        XElement element = Xml;
                        string str = ObjectUtilities.GetIndentedElementString(element, indent + 1);
                        ObjectUtilities.DisplayLabel(this, label, indent);
                        ObjectUtilities.DisplayMessage(str, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        /*
        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseString);
        }

        public virtual bool Equals(IBaseObject obj)
        {
            return this.Equals(obj as BaseString);
        }

        public virtual bool Equals(BaseString obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(BaseString other1, BaseString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            return (other1.Compare(other2) == 0 ? true : false);
        }

        public static bool operator !=(BaseString other1, BaseString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            return (other1.Compare(other2) == 0 ? false : true);
        }
        */

        public virtual int Compare(IBaseObjectKeyed other)
        {
            if (other == null)
                return 1;

            BaseString otherString = other as BaseString;

            if (otherString == null)
                return 1;

            int returnValue = ObjectUtilities.CompareKeys(this, other);
            if (returnValue != 0)
                return returnValue;
            returnValue = ObjectUtilities.CompareDateTimes(_CreationTime, other.CreationTime);
            if (returnValue != 0)
                return returnValue;
            returnValue = ObjectUtilities.CompareDateTimes(_ModifiedTime, other.ModifiedTime);
            if (returnValue != 0)
                return returnValue;
            return Text.CompareTo(otherString.Text);
        }

        public virtual int CompareKey(object key)
        {
            return ObjectUtilities.CompareObjects(_Key, key);
        }

        public virtual bool MatchKey(object key)
        {
            return CompareKey(key) == 0;
        }

        public static int Compare(BaseString string1, BaseString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            return string1.Compare(string2);
        }

        public virtual void OnFixup(FixupDictionary fixups)
        {
        }

        public static int CompareBaseStringLists(List<BaseString> other1, List<BaseString> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return Compare(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static List<BaseString> CopyBaseStrings(List<BaseString> baseStrings)
        {
            if (baseStrings == null)
                return null;

            List<BaseString> list = new List<BaseString>(baseStrings.Count());

            foreach (BaseString baseString in baseStrings)
            {
                list.Add(new BaseString(baseString));
            }

            return list;
        }

        public static List<BaseString> GetBaseStringListFromStrings(List<string> strings)
        {
            int count = strings.Count();
            List<BaseString> list = new List<BaseString>(count);

            foreach (string str in strings)
                list.Add(new BaseString(str, str));

            return list;
        }

        public static string GetStringFromBaseStringList(List<BaseString> baseStrings)
        {
            if (baseStrings == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (BaseString s in baseStrings)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.Append(s.KeyString);
                sb.Append("=");
                if (!String.IsNullOrEmpty(s.Text))
                    sb.Append(TextUtilities.OptionEncode(s.Text));
            }
            return sb.ToString();
        }

        public static List<BaseString> GetBaseStringListFromString(string data)
        {
            char[] seps = { ',' };
            char[] equals = { '=' };
            string[] strings = data.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            List<BaseString> list = new List<BaseString>();
            foreach (string kvp in strings)
            {
                string[] components = kvp.Split(equals);
                string key;
                string value;
                if (components.Count() != 0)
                    key = components[0].Trim();
                else
                    key = String.Empty;
                if (components.Count() > 1)
                    value = TextUtilities.OptionDecode(components[1]);
                else
                    value = String.Empty;
                list.Add(new BaseString(key, value));
            }
            return list;
        }

        public static List<BaseString> GetBaseStringListFromElement(XElement element)
        {
            string data = element.Value.Trim();
            return GetBaseStringListFromString(data);
        }

        public static XElement GetElementFromBaseStringList(string name, List<BaseString> baseStrings)
        {
            if (baseStrings == null)
                return null;
            XElement element = new XElement(name);
            string data = GetStringFromBaseStringList(baseStrings);
            element.SetValue(data);
            return element;
        }

        public static string GetOptionStringFromList(List<BaseString> options, string name, string defaultValue)
        {
            if (options == null)
                return defaultValue;
            BaseString bs = options.FirstOrDefault(x => x.KeyString == name);
            if (bs == null)
                return defaultValue;
            return bs.Text;
        }

        public static int GetOptionIntegerFromList(List<BaseString> options, string name, int defaultValue)
        {
            if (options == null)
                return defaultValue;
            BaseString bs = options.FirstOrDefault(x => x.KeyString == name);
            if (bs == null)
                return defaultValue;
            return ObjectUtilities.GetIntegerFromString(bs.Text, defaultValue);
        }

        public static bool GetOptionFlagFromList(List<BaseString> options, string name, bool defaultValue)
        {
            if (options == null)
                return defaultValue;
            BaseString bs = options.FirstOrDefault(x => x.KeyString == name);
            if (bs == null)
                return defaultValue;
            return ObjectUtilities.GetBoolFromString(bs.Text, defaultValue);
        }
    }
}

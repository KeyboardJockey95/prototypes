using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class LanguageID : IBaseObjectKeyed
    {
        private string _LanguageCode;       // i.e. "en", "zh", etc.
        private string _CultureCode;        // i.e. "US", "CN" etc.
        private string _ExtensionCode;      // Null or "pn" for "Pinyin".
        protected bool _Modified;
        public static LanguageID Empty = new LanguageID("", "", "");

        public LanguageID(string languageCode, string cultureCode, string extensionCode)
        {
            _LanguageCode = languageCode;
            _CultureCode = cultureCode;
            _ExtensionCode = extensionCode;
            _Modified = false;
        }

        public LanguageID(string languageCode, string cultureCode)
        {
            _LanguageCode = languageCode;
            _CultureCode = cultureCode;
            _ExtensionCode = null;
            _Modified = false;
        }

        public LanguageID(string languageCultureExtensionCode)
        {
            LanguageCultureExtensionCode = languageCultureExtensionCode;
            _Modified = false;
        }

        public LanguageID(LanguageID other)
        {
            _LanguageCode = other.LanguageCode;
            _CultureCode = other.CultureCode;
            _ExtensionCode = other.ExtensionCode;
            _Modified = false;
        }

        public LanguageID(XElement element)
        {
            OnElement(element);
        }

        public LanguageID()
        {
            ClearLanguageID();
        }

        public virtual void Clear()
        {
            ClearLanguageID();
        }

        public void ClearLanguageID()
        {
            _LanguageCode = null;
            _CultureCode = null;
            _ExtensionCode = null;
            _Modified = false;
        }

        public virtual IBaseObject Clone()
        {
            return new LanguageID(this);
        }

        public static LanguageID CloneLanguageID(LanguageID other)
        {
            if (other == null)
                return null;

            return new LanguageID(other);
        }

        public static List<LanguageID> CloneLanguageIDs(List<LanguageID> other)
        {
            if (other == null)
                return null;

            List<LanguageID> languageIDs = new List<LanguageID>();

            foreach (LanguageID lid in other)
                languageIDs.Add(new LanguageID(lid));

            return languageIDs;
        }

        public Type KeyType
        {
            get
            {
                return typeof(string);
            }
            set
            {
            }
        }

        public bool IsIntegerKeyType
        {
            get
            {
                return false;
            }
        }

        public virtual object Key
        {
            get
            {
                string key = LanguageCultureExtensionCode;
                if (key == null)
                    key = String.Empty;
                return key;
            }
            set
            {
                LanguageCultureExtensionCode = value as string;
            }
        }

        public void SetKeyNoModify(object key)
        {
            ParseLanguageCultureExtensionCodes(key as string, out _LanguageCode, out _CultureCode, out _ExtensionCode);
        }

        public void ResetKeyNoModify()
        {
            Key = BaseObjectKeyed.GetResetKeyValue(KeyType);
        }

        public string KeyString
        {
            get
            {
                string keyString = LanguageCultureExtensionCode;
                if (keyString == null)
                    return String.Empty;
                else
                    return keyString;
            }
        }

        public int KeyInt
        {
            get
            {
                throw new ObjectException("LanguageID doesn't support integer keys.");
            }
        }

        public virtual string SymbolName
        {
            get
            {
                string languageCode = LanguageCultureExtensionCode;
                if (languageCode != null)
                {
                    string symbolName = languageCode.Replace('-', '_');
                    return symbolName;
                }
                return "any";
            }
            set
            {
                LanguageCultureExtensionCode = value.Replace('_', '-');
            }
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
                return "LanguageID";
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
            get { return DateTime.MinValue; }
            set { }
        }

        public DateTime ModifiedTime
        {
            get { return DateTime.MinValue; }
            set { }
        }

        public void Touch()
        {
        }

        public void TouchAndClearModified()
        {
            Modified = false;
        }

        public virtual XAttribute GetAttribute(string name)
        {
            XAttribute attribute = new XAttribute(name, LanguageCultureExtensionCode);
            return attribute;
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name, LanguageCultureExtensionCode);
            return element;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "KeyType":
                    break;
                case "Key":
                    Key = attributeValue;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            return false;
        }

        public virtual void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }

            LanguageCultureExtensionCode = element.Value.Trim();

            Modified = false;
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
            return LanguageCultureExtensionCode;
        }

        public bool FromString(string value)
        {
            _Modified = true;
            if (ParseLanguageCultureExtensionCodes(value, out _LanguageCode, out _CultureCode, out _ExtensionCode))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            string value = LanguageCultureExtensionCode;
            if (value != null)
                return value.GetHashCode();
            return 0;
        }

        public void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Full:
                    ObjectUtilities.DisplayLabelArgument(this, label, indent, LanguageCultureExtensionCode);
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

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LanguageID);
        }

        public bool Equals(LanguageID obj)
        {
            return (this == obj);
        }

        public static bool operator ==(LanguageID other1, LanguageID other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            if ((other1._LanguageCode == other2._LanguageCode)
                    && (other1._CultureCode == other2._CultureCode)
                    && (other1._ExtensionCode == other2._ExtensionCode))
                return true;
            return false;
        }

        public static bool operator !=(LanguageID other1, LanguageID other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            if ((other1._LanguageCode == other2._LanguageCode)
                    && (other1._CultureCode == other2._CultureCode)
                    && (other1._ExtensionCode == other2._ExtensionCode))
                return false;
            return true;
        }

        public virtual int Compare(IBaseObjectKeyed other)
        {
            if ((other == null) || String.IsNullOrEmpty((other as LanguageID).LanguageCultureExtensionCode))
            {
                if (String.IsNullOrEmpty(LanguageCultureExtensionCode))
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (String.IsNullOrEmpty(LanguageCultureExtensionCode))
                    return -1;
                else
                    return LanguageCultureExtensionCode.CompareTo((other as LanguageID).LanguageCultureExtensionCode);
            }
        }

        public int CompareFuzzy(LanguageID other)
        {
            if ((other == null) || String.IsNullOrEmpty(other.LanguageCultureExtensionCode))
            {
                if (String.IsNullOrEmpty(LanguageCultureExtensionCode))
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (String.IsNullOrEmpty(LanguageCultureExtensionCode))
                    return -1;
                else if (LanguageCultureExtensionCode == other.LanguageCultureExtensionCode)
                    return 0;
                else
                    return LanguageCode.CompareTo(other.LanguageCode);
            }
        }

        public virtual int MediaLanguageCompare(LanguageID other)
        {
            if ((other == null) || String.IsNullOrEmpty(other.LanguageCode))
            {
                if (String.IsNullOrEmpty(LanguageCode))
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (String.IsNullOrEmpty(LanguageCode))
                    return -1;
                else
                    return LanguageCode.CompareTo(other.LanguageCode);
            }
        }

        public virtual int CompareKey(object key)
        {
            return ObjectUtilities.CompareObjects(Key, key);
        }

        public virtual bool MatchKey(object key)
        {
            return CompareKey(key) == 0;
        }

        public virtual void OnFixup(FixupDictionary fixups)
        {
        }

        public static int Compare(LanguageID other1, LanguageID other2)
        {
            if (other1 != null)
                return other1.Compare(other2);
            else if (other2 != null)
                return -other1.Compare(other1);
            else
                return 0;
        }

        public static int CompareFuzzy(LanguageID other1, LanguageID other2)
        {
            if (other1 != null)
                return other1.CompareFuzzy(other2);
            else if (other2 != null)
                return -other1.CompareFuzzy(other1);
            else
                return 0;
        }

        public static int CompareNames(LanguageID other1, LanguageID other2)
        {
            if ((other1 != null) && (other2 != null))
            {
                if (other1.LanguageCode == other2.LanguageCode)
                {
                    if (other1.ExtensionCode == other2.ExtensionCode)
                        return 0;
                    else if (IsRomanizedCode(other1.ExtensionCode))
                        return 1;
                    else if (IsRomanizedCode(other2.ExtensionCode))
                        return -1;
                    else
                        return String.Compare(other1.LanguageName(LanguageLookup.English), other2.LanguageName(LanguageLookup.English));
                }
                else
                    return String.Compare(other1.LanguageName(LanguageLookup.English), other2.LanguageName(LanguageLookup.English));
            }
            else if (other1 != null)
                return 1;
            else if (other2 != null)
                return -1;
            else
                return 0;
        }

        public static bool IsRomanizedCode(string code)
        {
            switch (code)
            {
                case "pn":
                case "rj":
                case "rm":
                    return true;
                default:
                    break;
            }
            return false;
        }

        public static int CompareLanguageIDLists(List<LanguageID> other1, List<LanguageID> other2)
        {
            int count1 = (other1 == null ? 0 : other1.Count());
            int count2 = (other2 == null ? 0 : other2.Count());
            int count = (count1 > count2 ? count1 : count2);
            int index;
            int diff;
            for (index = 0; index < count; index++)
            {
                if (index >= count2)
                    return 1;
                else if (index >= count1)
                    return -1;
                else if ((diff = Compare(other1[index], other2[index])) != 0)
                    return diff;
            }
            return 0;
        }

        // Returns true if changed
        public static bool MergeLanguageIDLists(List<LanguageID> target, List<LanguageID> source)
        {
            bool returnValue = false;

            if (source == null)
                return false;

            foreach (LanguageID sourceID in source)
            {
                if (!target.Contains(sourceID))
                {
                    target.Add(sourceID);
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool IsLanguage()
        {
            if (!String.IsNullOrEmpty(_LanguageCode) && !_LanguageCode.StartsWith("("))
                return true;
            return false;
        }

        public string LanguageCode
        {
            get
            {
                return _LanguageCode;
            }
            set
            {
                if (_LanguageCode != value)
                    _Modified = true;
                _LanguageCode = value;
            }
        }

        public string CultureCode
        {
            get
            {
                return _CultureCode;
            }
            set
            {
                if (_CultureCode != value)
                    _Modified = true;
                _CultureCode = value;
            }
        }

        public string ExtensionCode
        {
            get
            {
                return _ExtensionCode;
            }
            set
            {
                if (_ExtensionCode != value)
                    _Modified = true;
                _ExtensionCode = value;
            }
        }

        public string LanguageCultureCode
        {
            get
            {
                if ((_LanguageCode != null) && (_CultureCode != null))
                    return _LanguageCode + "-" + CultureCode;
                else if (_LanguageCode != null)
                    return _LanguageCode;
                return null;
            }
            set
            {
                _Modified = true;
                ParseLanguageCultureCodes(value, out _LanguageCode, out _CultureCode);
            }
        }

        public string LanguageCultureExtensionCode
        {
            get
            {
                if (_ExtensionCode == null)
                    return LanguageCultureCode;
                string ext = "-" + _ExtensionCode;
                if ((_LanguageCode != null) && (_CultureCode != null))
                    return _LanguageCode + "-" + CultureCode + ext;
                else if (_LanguageCode != null)
                    return _LanguageCode + "-" + ext;
                return _ExtensionCode;
            }
            set
            {
                _Modified = true;
                ParseLanguageCultureExtensionCodes(value, out _LanguageCode, out _CultureCode, out _ExtensionCode);
            }
        }

        public string LanguageCultureExtensionID
        {
            get
            {
                return LanguageCultureExtensionCode.Replace('-', '_');
            }
        }

        public CultureInfo CultureInfo
        {
            get
            {
                return new CultureInfo(LanguageCultureCode);
            }
            set
            {
                if (value != null)
                    LanguageCultureCode = value.ToString();
            }
        }

        public static bool ParseLanguageCultureCodes(string value, out string languageCode, out string cultureCode)
        {
            string extensionCode;
            return ParseLanguageCultureExtensionCodes(value, out languageCode, out cultureCode, out extensionCode);
        }

        public static bool ParseLanguageCultureExtensionCodes(
            string value, out string languageCode, out string cultureCode, out string extensionCode)
        {
            if (value == null)
            {
                languageCode = null;
                cultureCode = null;
                extensionCode = null;
                return true;
            }

            int length = value.Length;
            int index;
            bool returnValue = true;

            languageCode = null;
            cultureCode = null;
            extensionCode = null;

            for (index = 0; index < length; index++)
            {
                if (value[index] == '-')
                {
                    languageCode = value.Substring(0, index);
                    if (languageCode == "")
                        languageCode = null;
                    index++;
                    break;
                }
            }

            if (index == length)
            {
                languageCode = value;
                if (languageCode == "")
                    languageCode = null;
                return returnValue;
            }

            int start = index;

            for (; index < length; index++)
            {
                if (value[index] == '-')
                {
                    cultureCode = value.Substring(start, index - start);
                    if (cultureCode == "")
                        cultureCode = null;
                    index++;
                    break;
                }
            }

            if (index == length)
            {
                cultureCode = value.Substring(start, index - start);
                if (cultureCode == "")
                    cultureCode = null;
                return returnValue;
            }

            extensionCode = value.Substring(index, length - index);
            if (extensionCode == "")
                extensionCode = null;

            return returnValue;
        }

        public static List<LanguageID> ParseLanguageIDList(string value)
        {
            char[] seps = { ',', ' ' };
            return ParseLanguageIDDelimitedList(value, seps);
        }

        public static List<LanguageID> ParseLanguageIDDelimitedList(string value, char[] delimiters)
        {
            if (value == null)
                return new List<LanguageID>();
            string[] strings = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            List<LanguageID> list = new List<LanguageID>(strings.Count());
            foreach (string str in strings)
            {
                LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(str);
                list.Add(languageID);
            }
            return list;
        }

        public static string ConvertLanguageIDListToString(IEnumerable<LanguageID> value)
        {
            return ConvertLanguageIDListToDelimitedString(value, null, ",", null);
        }

        public static string ConvertLanguageIDListToDelimitedString(IEnumerable<LanguageID> value,
            string prefix, string delimiter, string postfix)
        {
            if (value == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            foreach (LanguageID languageID in value)
            {
                if (languageID == null)
                    continue;

                if (first)
                    first = false;
                else
                    sb.Append(delimiter);
                sb.Append(languageID.LanguageCultureExtensionCode);
            }
            if (!String.IsNullOrEmpty(postfix))
                sb.Append(postfix);
            string returnValue = sb.ToString();
            return returnValue;
        }

        public static string ConvertLanguageIDListToStringExpanded(IEnumerable<LanguageID> value)
        {
            string returnValue = ConvertLanguageIDListToString(value);
            if (returnValue.Contains("(all languages)"))
                returnValue = returnValue.Replace("(all languages)", LanguageLookup.AllLanguageKeys);
            return returnValue;
        }

        public static string ConvertLanguageIDListToDelimitedStringExpanded(IEnumerable<LanguageID> value,
            string prefix, string delimiter, string postfix)
        {
            string returnValue = ConvertLanguageIDListToDelimitedString(value, prefix, delimiter, postfix);
            if (returnValue.Contains("(all languages)"))
            {
                returnValue = returnValue.Replace("(all languages)", LanguageLookup.AllLanguageKeys);
                if (delimiter != ",")
                    returnValue = returnValue.Replace(",", delimiter);
            }
            return returnValue;
        }

        public static string ConvertLanguageIDListToNameString(IEnumerable<LanguageID> value, LanguageID uiLanguageID)
        {
            return ConvertLanguageIDListToDelimitedNameString(value, null, ",", null, uiLanguageID);
        }

        public static string ConvertLanguageIDListToDelimitedNameString(IEnumerable<LanguageID> value,
            string prefix, string delimiter, string postfix, LanguageID uiLanguageID)
        {
            if (value == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            foreach (LanguageID languageID in value)
            {
                if (first)
                    first = false;
                else
                    sb.Append(delimiter);
                sb.Append(languageID.LanguageName(uiLanguageID));
            }
            if (!String.IsNullOrEmpty(postfix))
                sb.Append(postfix);
            return sb.ToString();
        }

        public static string ConvertLanguageIDListToMediaNameString(IEnumerable<LanguageID> value, LanguageID uiLanguageID)
        {
            return ConvertLanguageIDListToDelimitedMediaNameString(value, null, ",", null, uiLanguageID);
        }

        public static string ConvertLanguageIDListToDelimitedMediaNameString(IEnumerable<LanguageID> value,
            string prefix, string delimiter, string postfix, LanguageID uiLanguageID)
        {
            List<string> usedNames = new List<string>();
            if (value == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            foreach (LanguageID languageID in value)
            {
                string mediaLanguageName = languageID.MediaLanguageName(uiLanguageID);
                if (usedNames.Contains(mediaLanguageName))
                    continue;
                usedNames.Add(mediaLanguageName);
                if (first)
                    first = false;
                else
                    sb.Append(delimiter);
                sb.Append(mediaLanguageName);
            }
            if (!String.IsNullOrEmpty(postfix))
                sb.Append(postfix);
            return sb.ToString();
        }

        public static string ConvertLanguageIDListToDelimitedMediaAbbreviationString(IEnumerable<LanguageID> value,
            string prefix, string delimiter, string postfix, LanguageID uiLanguageID)
        {
            List<string> usedNames = new List<string>();
            if (value == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            foreach (LanguageID languageID in value)
            {
                string mediaLanguageName = languageID.MediaLanguageAbbreviation(uiLanguageID);
                if (usedNames.Contains(mediaLanguageName))
                    continue;
                usedNames.Add(mediaLanguageName);
                if (first)
                    first = false;
                else
                    sb.Append(delimiter);
                sb.Append(mediaLanguageName);
            }
            if (!String.IsNullOrEmpty(postfix))
                sb.Append(postfix);
            return sb.ToString();
        }

        public static List<LanguageID> CopyList(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;
            return new List<LanguageID>(languageIDs);
        }

        public static List<LanguageID> ConcatenateUnqueList(List<LanguageID> languageIDs1, List<LanguageID> languageIDs2)
        {
            List<LanguageID> languageIDs = new List<LanguageID>();
            if (languageIDs1 != null)
            {
                foreach (LanguageID languageID in languageIDs1)
                {
                    if (!languageIDs.Contains(languageID))
                        languageIDs.Add(languageID);
                }
            }
            if (languageIDs2 != null)
            {
                foreach (LanguageID languageID in languageIDs2)
                {
                    if (!languageIDs.Contains(languageID))
                        languageIDs.Add(languageID);
                }
            }
            return languageIDs;
        }

        public string Language
        {
            get
            {
                return LanguageLookup.ConvertLanguageCultureExtensionCodeToLanguage(LanguageCultureExtensionCode);
            }
        }

        public string LanguageOnly
        {
            get
            {
                return LanguageLookup.ConvertLanguageCodeToLanguage(LanguageCode);
            }
        }

        public string LanguageCultureExtension
        {
            get
            {
                return LanguageLookup.ConvertLanguageCultureExtensionCodeToLanguageAndCulture(LanguageCultureExtensionCode);
            }
        }

        public string LanguageName(LanguageID displayLanguageID)
        {
            return LanguageLookup.GetLanguageName(LanguageCultureExtensionCode, displayLanguageID.LanguageCultureExtensionCode);
        }

        public string MediaLanguageName(LanguageID displayLanguageID)
        {
            return LanguageLookup.GetLanguageName(LanguageCode, displayLanguageID.LanguageCultureExtensionCode);
        }

        public string LanguageAbbreviation(LanguageID displayLanguageID)
        {
            string str;
            string prefix = null;
            string suffix = null;

            if (!String.IsNullOrEmpty(CultureCode))
                suffix = CultureCode.ToUpper();
            else if (!String.IsNullOrEmpty(ExtensionCode))
                suffix = ExtensionCode.ToUpper();

            if ((suffix != null) && (displayLanguageID == LanguageLookup.English))
            {
                prefix = LanguageOnly;
                if (prefix.Length > 4)
                    prefix = prefix.Substring(0, 3);
                if (suffix.Length >= 3)
                    suffix = suffix.Substring(suffix.Length - 1);
                str = prefix + suffix;
            }
            else
            {
                str = ApplicationData.Repositories.TranslateUIString(Language, displayLanguageID).Text;
                if (str.Length > 4)
                    str = str.Substring(0, 3);
            }

            return str;
        }

        public string MediaLanguageAbbreviation(LanguageID displayLanguageID)
        {
            string str;

            if (displayLanguageID == LanguageLookup.English)
            {
                str = LanguageOnly;
                if (str.Length > 2)
                    str = str.Substring(0, 2);
            }
            else
            {
                BaseString baseStr = ApplicationData.Repositories.TranslateUIString(Language, displayLanguageID);
                if (baseStr != null)
                    str = baseStr.Text;
                else
                    str = Language;
                if (str.Length > 2)
                    str = str.Substring(0, 2);
            }

            return str;
        }

        public string MediaLanguageCode()
        {
            return MediaLanguageID().LanguageCultureExtensionCode;
        }

        public LanguageID MediaLanguageID()
        {
            return LanguageLookup.GetMediaLanguageID(this);
        }

        public static List<LanguageID> GetMediaLanguageIDs(
            List<LanguageID> languageIDs)
        {
            List<LanguageID> list = new List<LanguageID>();

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageID mediaLanguageID = languageID.MediaLanguageID();

                    if (!list.Contains(mediaLanguageID))
                        list.Add(mediaLanguageID);
                }
            }

            return list;
        }

        public static Dictionary<LanguageID, List<LanguageID>> GetMediaLanguageIDDictionary(
            List<LanguageID> languageIDs)
        {
            Dictionary<LanguageID, List<LanguageID>> dictionary = new Dictionary<LanguageID, List<LanguageID>>();
            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageID mediaLanguageID = languageID.MediaLanguageID();
                    List<LanguageID> list;
                    if (dictionary.TryGetValue(mediaLanguageID, out list))
                    {
                        if (!list.Contains(languageID))
                            list.Add(languageID);
                    }
                    else
                        dictionary.Add(mediaLanguageID, new List<LanguageID>() { languageID });
                }
            }
            return dictionary;
        }

        public static List<List<LanguageID>> GetMediaLanguageIDLists(
            List<LanguageID> languageIDs)
        {
            Dictionary<LanguageID, List<LanguageID>> dictionary = GetMediaLanguageIDDictionary(languageIDs);
            List<List<LanguageID>> list = new List<List<LanguageID>>();

            foreach (KeyValuePair<LanguageID, List<LanguageID>> kvp in dictionary)
                list.Add(kvp.Value);

            return list;
        }

        public LanguageDescription LanguageDescription
        {
            get
            {
                return LanguageLookup.GetLanguageDescription(this);
            }
        }


        public bool CharacterBased
        {
            get
            {
                if (LanguageDescription != null)
                    return LanguageDescription.CharacterBased;
                return false;
            }
        }

        public bool ReadTopToBottom
        {
            get
            {
                if (LanguageDescription != null)
                    return LanguageDescription.ReadTopToBottom;
                return false;
            }
        }

        public bool ReadRightToLeft
        {
            get
            {
                if (LanguageDescription != null)
                    return LanguageDescription.ReadRightToLeft;
                return false;
            }
        }

        public static List<LanguageID> CreateIntersection(List<LanguageID> source, List<LanguageID> master)
        {
            List<LanguageID> list = new List<LanguageID>();

            if (master == null)
                return list;

            if (source != null)
            {
                // Handle All Languages.
                foreach (LanguageID sid in source)
                {
                    if (!sid.IsLanguage())
                    {
                        list.AddRange(master);
                        return list;
                    }
                }

                foreach (LanguageID sid in source)
                {
                    if (master.Contains(sid))
                        list.Add(sid);
                }
            }

            return list;
        }

        public static List<LanguageID> JoinUnique(List<LanguageID> list1, List<LanguageID> list2)
        {
            if (list1 == null)
            {
                if (list2 == null)
                    return new List<LanguageID>();

                return new List<LanguageID>(list2);
            }
            else if (list2 == null)
                return new List<LanguageID>(list1);

            List<LanguageID> list = new List<LanguageID>(list1);

            foreach (LanguageID lid in list2)
            {
                if (!list.Contains(lid))
                    list.Add(lid);
            }

            return list;
        }

        public static List<string> GetLanguageCultureExtensionCodes(List<LanguageID> languageIDs)
        {
            List<string> list = new List<string>();

            if (languageIDs != null)
            {
                foreach (LanguageID sid in languageIDs)
                    list.Add(sid.LanguageCultureExtensionCode);
            }

            return list;
        }

        public static Dictionary<string, bool> GetLanguageFlagsDictionaryFromStringList(List<string> flagNames, bool value)
        {
            if (flagNames == null)
                return null;

            Dictionary<string, bool> flagDictionary = new Dictionary<string, bool>();

            foreach (string flagName in flagNames)
                flagDictionary.Add(flagName, value);

            return flagDictionary;
        }

        public static List<LanguageID> GetLanguageIDListFromLanguageCodes(List<string> languageCodes)
        {
            if (languageCodes == null)
                return null;

            int count = languageCodes.Count();
            List<LanguageID> languageIDs = new List<LanguageID>(count);

            foreach (string languageCode in languageCodes)
            {
                LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
                languageIDs.Add(languageID);
            }

            return languageIDs;
        }

        public static List<string> GetLanguageCodeListFromLanguageIDs(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            int count = languageIDs.Count();
            List<string> languageCodes = new List<string>(count);

            foreach (LanguageID languageID in languageIDs)
            {
                string languageCode = languageID.LanguageCultureExtensionCode;
                languageCodes.Add(languageCode);
            }

            return languageCodes;
        }

        public static List<LanguageID> GetExtendedLanguageIDsFromLanguageIDs(List<LanguageID> languageIDs)
        {
            List<LanguageID> extendedLanguageIDs = new List<LanguageID>();

            foreach (LanguageID languageID in languageIDs)
            {
                List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);

                foreach (LanguageID familyLanguageID in familyLanguageIDs)
                {
                    if (!extendedLanguageIDs.Contains(familyLanguageID))
                        extendedLanguageIDs.Add(familyLanguageID);
                }
            }

            return extendedLanguageIDs;
        }

        public static List<string> GetLanguageNamesFromLanguageIDs(List<LanguageID> languageIDs, LanguageID uiLanguageID)
        {
            if (languageIDs == null)
                return null;

            int count = languageIDs.Count();
            List<string> languageNames = new List<string>(count);

            foreach (LanguageID languageID in languageIDs)
            {
                string languageName = languageID.LanguageName(uiLanguageID);
                languageNames.Add(languageName);
            }

            return languageNames;
        }

        public static string GetLanguageNamesStringFromLanguageIDs(
            List<LanguageID> languageIDs,
            LanguageID uiLanguageID,
            string separator)
        {
            if (languageIDs == null)
                return null;

            int count = languageIDs.Count();
            StringBuilder sb = new StringBuilder();

            foreach (LanguageID languageID in languageIDs)
            {
                string languageName = languageID.LanguageName(uiLanguageID);

                if (sb.Length != 0)
                    sb.Append(separator);

                sb.Append(languageName);
            }

            return sb.ToString();
        }
    }
}

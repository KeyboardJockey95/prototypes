using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class LanguageString : BaseString, IBaseObjectLanguage
    {
        protected LanguageID _LanguageID;

        public LanguageString(object key, LanguageID languageID, string text)
            : base(key, text)
        {
            if (languageID == null)
                _LanguageID = LanguageLookup.Any;
            else
                _LanguageID = languageID;
        }

        public LanguageString(LanguageString other)
            : base(other)
        {
            _LanguageID = other.LanguageID;
        }

        public LanguageString(object key, LanguageString other)
            : base(key, other)
        {
            _LanguageID = other.LanguageID;
        }

        public LanguageString(XElement element)
        {
            OnElement(element);
        }

        public LanguageString()
        {
            ClearLanguageString();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageString();
        }

        public void ClearLanguageString()
        {
            _LanguageID = LanguageLookup.Any;
        }

        public virtual void Copy(LanguageString other)
        {
            base.Copy(other);
            _LanguageID = other.LanguageID;
        }

        public virtual void CopyDeep(LanguageString other)
        {
            Copy(other);
        }

        public override IBaseObject Clone()
        {
            return new LanguageString(this);
        }

        public LanguageID LanguageID
        {
            get
            {
                return _LanguageID;
            }
            set
            {
                if (value == null)
                    value = LanguageLookup.Any;
                if (LanguageID != value)
                    Modified = true;
                _LanguageID = value;
            }
        }

        public string LanguageDisplayName(LanguageID displayLanguageID)
        {
            if (_LanguageID != null)
                return _LanguageID.LanguageName(displayLanguageID);
            else
                return "(no languageID)";
        }

        public string LanguageAbbrev
        {
            get
            {
                if (_LanguageID != null)
                    return _LanguageID.LanguageCultureExtensionCode;
                else
                    return "(no languageID)";
            }
        }

        public string LanguageCode
        {
            get
            {
                if (LanguageID != null)
                    return LanguageID.LanguageCode;
                else
                    return "(no languageID)";
            }
        }

        public string LanguageName
        {
            get
            {
                if (LanguageID != null)
                    return LanguageID.LanguageName(LanguageLookup.English);
                else
                    return "(no languageID)";
            }
        }

        public bool IsOverlapping(LanguageString other)
        {
            if (other == null)
                return false;

            if (Text == other.Text)
                return true;

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            string languageCode = _LanguageID.LanguageCultureExtensionCode;
            if (languageCode != null)
                element.Add(new XAttribute("LanguageID", _LanguageID.LanguageCultureExtensionCode));
            return element;
        }

        public override XElement GetElementTextElement(string name)
        {
            XElement element = base.GetElementTextElement(name);
            string languageCode = _LanguageID.LanguageCultureExtensionCode;
            if (languageCode != null)
                element.Add(new XAttribute("LanguageID", _LanguageID.LanguageCultureExtensionCode));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageIDNoAdd(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _Key.GetHashCode();
        }

        /*
        public override bool Equals(object obj)
        {
            return this.Equals(obj as LanguageString);
        }

        public override bool Equals(IBaseObject obj)
        {
            return this.Equals(obj as LanguageString);
        }

        public override bool Equals(BaseString obj)
        {
            return this.Equals(obj as LanguageString);
        }

        public virtual bool Equals(LanguageString obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(LanguageString other1, LanguageString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            return (other1.Compare(other2) == 0 ? true : false);
        }

        public static bool operator !=(LanguageString other1, LanguageString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            return (other1.Compare(other2) == 0 ? false : true);
        }
        */

        public override int Compare(IBaseObjectKeyed other)
        {
            int diff = ObjectUtilities.CompareKeys(this, other);
            if (diff != 0)
                return diff;
            BaseString otherString = other as BaseString;
            if (otherString == null)
            {
                if (String.IsNullOrEmpty(LanguageID.LanguageCultureExtensionCode) && String.IsNullOrEmpty(Text))
                    return 0;
                else
                    return 1;
            }
            diff = Text.CompareTo(otherString.Text);
            if (diff != 0)
                return diff;
            LanguageString otherLanguageString = other as LanguageString;
            if (otherLanguageString == null)
            {
                if (String.IsNullOrEmpty(LanguageID.LanguageCultureExtensionCode))
                    return 0;
                else
                    return 1;
            }
            diff = LanguageID.Compare(LanguageID, otherLanguageString.LanguageID);
            if (diff != 0)
                return diff;
            return 0;
        }

        public static int Compare(LanguageString string1, LanguageString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            return string1.Compare(string2);
        }

        public static int CompareText(LanguageString string1, LanguageString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            return ObjectUtilities.CompareStrings(string1.Text, string2.Text);
        }

        public static int CompareLanguageName(LanguageString string1, LanguageString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            return LanguageID.CompareNames(string1.LanguageID, string2.LanguageID);
        }

        public static List<LanguageString> CloneLanguageStringList(List<LanguageString> languageStrings)
        {
            if (languageStrings == null)
                return null;

            List<LanguageString> newLanguageStrings = new List<LanguageString>();

            foreach (LanguageString ls in languageStrings)
                newLanguageStrings.Add(new LanguageString(ls));

            return newLanguageStrings;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObjectLanguage : BaseObjectKeyed, IBaseObjectLanguage
    {
        protected LanguageID _LanguageID;

        public BaseObjectLanguage(object key, LanguageID languageID)
            : base(key)
        {
            if (languageID == null)
                _LanguageID = LanguageLookup.Any;
            else
                _LanguageID = languageID;
        }

        public BaseObjectLanguage(object key)
            : base(key)
        {
            _LanguageID = LanguageLookup.Any;
        }

        public BaseObjectLanguage(BaseObjectLanguage other, object key)
            : base(key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectLanguage(BaseObjectLanguage other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectLanguage(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectLanguage()
        {
            ClearLanguageBase();
        }

        public void Copy(BaseObjectLanguage other)
        {
            if (other != null)
                _LanguageID = other.LanguageID;
            else
                _LanguageID = null;
        }

        public void CopyDeep(BaseObjectLanguage other)
        {
            if (other != null)
                _LanguageID = other.LanguageID;

            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageBase();
        }

        public void ClearLanguageBase()
        {
            _LanguageID = LanguageLookup.Any;
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectLanguage(this);
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

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString + ": " + LanguageAbbrev);
                    else
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, LanguageAbbrev);
                    break;
                case DisplayDetail.Full:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                    else
                        ObjectUtilities.DisplayLabel(this, label, indent);
                    DisplayField("LanguageID", LanguageAbbrev, indent + 1);
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
                    LanguageID = LanguageLookup.GetLanguageID(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectLanguage otherLanguageBase = other as BaseObjectLanguage;

            if (otherLanguageBase == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            return LanguageID.LanguageCultureExtensionCode.CompareTo(otherLanguageBase.LanguageID.LanguageCultureExtensionCode);
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
    }
}

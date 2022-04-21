using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPassageReference : BaseObjectLanguages
    {
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }

        public FormatPassageReference(
            string name,
            string type,
            string fileName,
            string url,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string owner) : base(name, targetLanguageIDs, hostLanguageIDs, owner)
        {
            Type = type;
            FileName = fileName;
            Url = url;
        }

        public FormatPassageReference(FormatPassageReference other) : base(other)
        {
            CopyFormatPassageReference(other);
        }

        public FormatPassageReference(XElement element) : base(element)
        {
            OnElement(element);
        }

        public override void Clear()
        {
            base.Clear();
            ClearFormatPassageReference();
        }

        public void ClearFormatPassageReference()
        {
            Type = String.Empty;
            FileName = String.Empty;
            Url = String.Empty;
        }

        public void CopyFormatPassageReference(FormatPassageReference other)
        {
            Type = other.Type;
            FileName = other.FileName;
            Url = other.Url;
        }

        public override string ToString()
        {
            return Name + ", "
                + Type + ", "
                + FileName + ","
                + Url;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(Type))
                element.Add(new XElement("Type", Type));
            if (!String.IsNullOrEmpty(FileName))
                element.Add(new XElement("FileName", FileName));
            if (!String.IsNullOrEmpty(Url))
                element.Add(new XElement("Url", Url));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Type":
                    Type = childElement.Value.Trim();
                    break;
                case "FileName":
                    FileName = childElement.Value.Trim();
                    break;
                case "Url":
                    Url = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            FormatPassageReference otherFormatPassageReference = other as FormatPassageReference;

            if (otherFormatPassageReference == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = Type.CompareTo(otherFormatPassageReference.Type);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(FileName, otherFormatPassageReference.FileName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(Url, otherFormatPassageReference.Url);
            return diff;
        }
    }
}

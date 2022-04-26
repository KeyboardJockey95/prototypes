using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObject : IBaseObject
    {
        public BaseObject(XElement element)
        {
            OnElement(element);
        }

        public BaseObject()
        {
            ClearBaseObject();
        }

        public virtual void Clear()
        {
            ClearBaseObject();
        }

        public void ClearBaseObject()
        {
        }

        public virtual IBaseObject Clone()
        {
            return new BaseObject();
        }

        public string GetClassName()
        {
            return GetType().Name;
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            return element;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            return OnUnknownAttribute(attribute);
        }

        public virtual bool OnUnknownAttribute(XAttribute attribute)
        {
#if DEBUG
            ApplicationData.Global.PutConsoleMessage("Unknown attribute: class " + GetClassName() + ", name " + attribute.Name.LocalName + ", value " + attribute.Value);
#endif
            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            return OnUnknownChildElement(childElement);
        }

        public virtual bool OnUnknownChildElement(XElement childElement)
        {
#if DEBUG
            ApplicationData.Global.PutConsoleMessage("Unknown child element: class " + GetClassName() + ", name " + childElement.Name.LocalName + ", value " + childElement.Value);
#endif
            return true;
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
                string xmlString = null;
                XElement element = null;
                xmlString = ApplicationData.Encoding.GetString(value, 0, value.Count());
                element = XElement.Parse(xmlString, LoadOptions.PreserveWhitespace);
                Xml = element;
            }
        }

        public virtual void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles,
            VisitMedia visitFunction)
        {
        }

        public void AddUniqueReference(List<IBaseObjectKeyed> references, IBaseObjectKeyed reference)
        {
            if (references == null)
                return;

            if (references.FirstOrDefault(x => (ObjectUtilities.MatchTypes(x, reference) && ObjectUtilities.MatchKeys(x, reference))) == null)
                references.Add(reference);
        }

        public virtual void Display(string label, DisplayDetail detail, int indent)
        {
            ObjectUtilities.DisplayLabel(this, label, indent);

            switch (detail)
            {
                case DisplayDetail.Lite:
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Full:
                case DisplayDetail.Xml:
                    {
                        XElement element = Xml;
                        string str = ObjectUtilities.GetIndentedElementString(element, indent + 1);
                        ObjectUtilities.DisplayMessage(str, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        public void DisplayLabel(string label, int indent)
        {
            ObjectUtilities.DisplayLabel(this, label, indent);
        }

        public void DisplayLabelArgument(string label, string argument, int indent)
        {
            ObjectUtilities.DisplayLabelArgument(this, label, argument, indent);
        }

        public void DisplayField(string name, string value, int indent)
        {
            ObjectUtilities.DisplayField(name, value, indent);
        }

        public void DisplayFieldObject(string name, IBaseObject obj, int indent)
        {
            ObjectUtilities.DisplayFieldObject(name, obj, indent);
        }

        public void DisplayMessage(string msg, int indent)
        {
            ObjectUtilities.DisplayMessage(msg, indent);
        }
    }
}

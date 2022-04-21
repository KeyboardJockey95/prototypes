using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class LexCategoryDesignation : BaseObjectKeyed
    {
        public String Category;
        public Designator Designation;

        public LexCategoryDesignation(string category, Designator designation) :
            base(ComposeKey(category, designation))
        {
            Category = category;
            Designation = designation;
        }

        public LexCategoryDesignation(LexCategoryDesignation other) :
            base(other)
        {
            Category = other.Category;
            Designation = other.Designation;
        }

        public LexCategoryDesignation(XElement element)
        {
            ClearLexCategoryDesignation();
            OnElement(element);
        }

        public LexCategoryDesignation()
        {
            ClearLexCategoryDesignation();
        }

        public void ClearLexCategoryDesignation()
        {
            Category = String.Empty;
            Designation = null;
        }

        public override string ToString()
        {
            string returnValue = Category + " - ";

            if (Designation != null)
                returnValue += Designation.Label;
            else
                returnValue += "(null)";

            return returnValue;
        }

        public static string ComposeKey(string category, Designator designation)
        {
            string key = category + " " + designation.KeyString;
            return key;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("Category", Category));
            element.Add(Designation.GetElement("Designation"));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            switch (attribute.Name.LocalName)
            {
                case "Category":
                    Category = attribute.Value.Trim();
                    break;
                default:
                    break;
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Designation":
                    Designation = new Designator(childElement);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}

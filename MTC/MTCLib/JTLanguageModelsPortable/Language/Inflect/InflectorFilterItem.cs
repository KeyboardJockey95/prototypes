using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Language
{
    public class InflectorFilterItem : BaseObjectKeyed
    {
        public string ItemType;             // Redirection, Disallow
        public LexicalCategory Category;    // Verb, Adjective, Noun, etc.
        public string CategoryString;       // Language-dictionary-specific.
        public string Class;                // i.e. verb class
        public string SubClass;             // i.e. verb sublcass
        public LiteralString Input;
        public LiteralString Output;
        public LexicalCategory NewCategory;
        public string NewCategoryString;
        public string NewClass;
        public string NewSubClass;

        public InflectorFilterItem(
            string itemType,
            LexicalCategory category,
            string categoryString,
            string className,
            string subClass,
            LiteralString input,
            LiteralString output,
            LexicalCategory newCategory,
            string newCategoryString,
            string newClass,
            string newSubClass)
        {
            ItemType = itemType;
            Category = category;
            CategoryString = categoryString;
            Class = className;
            SubClass = subClass;
            Input = input;
            Output = output;
            NewCategory = newCategory;
            NewCategoryString = newCategoryString;
            NewClass = newClass;
            NewSubClass = newSubClass;
        }

        public InflectorFilterItem(XElement element)
        {
            ClearInflectorFilterItem();
            OnElement(element);

            if (String.IsNullOrEmpty(ItemType))
                ItemType = element.Name.LocalName;
        }

        public InflectorFilterItem(InflectorFilterItem other) :
            base(other)
        {
            CopyInflectorFilterItem(other);
        }

        public InflectorFilterItem()
        {
            ClearInflectorFilterItem();
        }

        public void ClearInflectorFilterItem()
        {
            ItemType = null;
            Category = LexicalCategory.Unknown;
            CategoryString = null;
            Class = null;
            SubClass = null;
            Input = null;
            Output = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public void CopyInflectorFilterItem(InflectorFilterItem other)
        {
            ItemType = other.ItemType;
            Category = other.Category;
            CategoryString = other.CategoryString;
            Class = other.Class;
            SubClass = other.SubClass;
            Input = other.Input;
            Output = other.Output;
            NewCategory = other.NewCategory;
            NewCategoryString = other.NewCategoryString;
            NewClass = other.NewClass;
            NewSubClass = other.NewSubClass;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Category != LexicalCategory.Unknown)
                element.Add(new XAttribute("Category", Category.ToString()));

            if (!String.IsNullOrEmpty(CategoryString))
                element.Add(new XAttribute("CategoryString", CategoryString));

            if (!String.IsNullOrEmpty(Class))
                element.Add(new XAttribute("Class", Class));

            if (!String.IsNullOrEmpty(SubClass))
                element.Add(new XAttribute("SubClass", SubClass));

            if (Input != null)
            {
                string literal = Input.StringListString;
                element.Add(new XElement("Input", literal));
            }

            if (Output != null)
            {
                string literal = Output.StringListString;
                element.Add(new XElement("Output", literal));
            }

            if (NewCategory != LexicalCategory.Unknown)
                element.Add(new XAttribute("NewCategory", NewCategory.ToString()));

            if (NewCategoryString != null)
                element.Add(new XAttribute("NewCategoryString", NewCategoryString));

            if (NewClass != null)
                element.Add(new XAttribute("NewClass", NewClass));

            if (NewSubClass != null)
                element.Add(new XAttribute("NewSubClass", NewSubClass));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value;

            switch (attribute.Name.LocalName)
            {
                case "Category":
                    Category = Sense.GetLexicalCategoryFromString(attributeValue);
                    break;
                case "CategoryString":
                    CategoryString = attributeValue;
                    break;
                case "Class":
                    Class = attributeValue;
                    break;
                case "SubClass":
                    SubClass = attributeValue;
                    break;
                case "Input":
                    Input = new LiteralString(attributeValue);
                    break;
                case "Output":
                    Output = new LiteralString(attributeValue);
                    break;
                case "NewCategory":
                    NewCategory = Sense.GetLexicalCategoryFromString(attributeValue);
                    break;
                case "NewCategoryString":
                    NewCategoryString = attributeValue;
                    break;
                case "NewClass":
                    NewClass = attributeValue;
                    break;
                case "NewSubClass":
                    NewSubClass = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }
    }
}

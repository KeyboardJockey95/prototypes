using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Content
{
    public class Annotation : BaseObjectKeyed
    {
        // Key is type.
        public static string[] Types =
        {
            "Heading",          // Heading in its own row and bolder style. Text.
            "Label",            // Heading in its own column on same row. Text.
            "Prefix",           // Prefix for item. Text.
            "Suffix",           // Suffix for item. Text.
            "Note",             // An attached note. Text.
            "QuestionNote",     // An attached note, for flash, in the first card side. Text.
            "AnswerNote",       // An attached note, for flash, in the non-first card sides. Text.
            "FootNote",         // Numbered footnote. Text.
            "Category",         // Part of speech, i.e. noun, verb, etc.  Language-dependent. Text.
            "Conjugation",      // Conjugation of verb, noun, adjective, etc.  Language-dependent. Text.
            "Break",            // Row break before this item. None.
            "Style",            // HTML style for item(s). Value.
            "Tag",              // A marker for use with markup documents to identify a group of items. Value.
            "Text",             // Styled text.  Study item skipped if empty, meaning it replaces the text.
            "Ordinal",          // Ordinal number. Text is the number string.
            "Picture",          // Picture. Value is a url or file name.
            "Embedded",         // Embedded element. Value is the HTML element, limited to safe elements.
            "Start",            // Start of a section. Value is the section type or ID.
            "Stop",             // End of a section (inclusive). Value is the section type or ID.
            "NotMapped"         // Excluded from audio/text mapping.
        };
        public static string Help =
            "An annotation is for adding extra stuff to an item or items."
            + " There are several annotation types."
            + " An annotation has either a single value string, or a multi-language string set, or no value."
            + " These are the currently supported annotation types:\r\n"
            + "<table class=\"table_full\">\r\n"
            + "  <tr>\r\n"
            + "    <th>Type</th><th>Description</th><th>Value Type</th>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Heading</td><td>Heading in its own row and bolder style.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Label</td><td>Heading in its own column on same row.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Prefix</td><td>Prefix for item (not translated or spoken).</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Suffix</td><td>Suffix for item (not translated or spoken).</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Note</td><td>An attached note (translated but not spoken).</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>FootNote</td><td>Numbered footnote (translated but not spoken).</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Category</td><td>Part of speech, i.e. noun, verb, etc.  Language-dependent.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Conjugation</td><td>Conjugation of verb, noun, adjective, etc.  Language-dependent.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Break</td><td>Row break before this item.</td><td>None</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Style</td><td>HTML style for item(s).</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Tag</td><td>A marker for use with markup documents to identify a group of items.</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Text</td><td>Styled text.  Study item skipped if empty, meaning it replaces the text.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Ordinal</td><td>Ordinal number. Text is the number string.</td><td>Text</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Picture</td><td>Picture. Value is a url or file name.</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Embedded</td><td>Embedded element. Value is the HTML element, limited to safe elements.</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Start</td><td>Start of a section. Value is the section type or ID.</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>Stop</td><td>Stop of a section, inclusive. Value is the section type or ID.</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "  <tr>\r\n"
            + "    <td>NotMapped</td><td>Excluded from audio/text mapping. \"true\"/\"false\"</td><td>Value</td>\r\n"
            + "  </tr>\r\n"
            + "</table>\r\n";
        private string _Value;
        private MultiLanguageString _Text;
        private string _Tag;        // Additional symbolic identifier.

        public Annotation(string type, string value, MultiLanguageString text, string tag = null)
            : base(type)
        {
            _Value = value;
            _Text = text;
            _Tag = tag;
        }

        public Annotation(string type, string value, string tag = null)
            : base(type)
        {
            _Value = value;
            _Text = null;
            _Tag = tag;
        }

        public Annotation(Annotation other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public Annotation(XElement element)
        {
            OnElement(element);
        }

        public Annotation()
        {
            ClearAnnotation();
        }

        public override void Clear()
        {
            base.Clear();
            ClearAnnotation();
        }

        public void ClearAnnotation()
        {
            _Value = null;
            _Text = null;
            _Tag = null;
        }

        public void Copy(Annotation other)
        {
            base.Copy(other);

            if (other == null)
                ClearAnnotation();
            else
            {
                _Value = other.Value;

                if (other.Text != null)
                    _Text = new MultiLanguageString(other.Text);
                else
                    _Text = null;

                _Tag = other.Tag;
            }

            ModifiedFlag = true;
        }

        public override IBaseObject Clone()
        {
            return new Annotation(this);
        }

        public string Type
        {
            get
            {
                return KeyString;
            }
            set
            {
                if (value != KeyString)
                {
                    Key = value;
                }
            }
        }

        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasValue()
        {
            return !String.IsNullOrEmpty(_Value);
        }

        public bool ValueAsBool
        {
            get
            {
                return ObjectUtilities.GetBoolFromString(_Value, false);
            }
            set
            {
                Value = (value ? "true" : "false");
            }
        }

        public int ValueAsInteger
        {
            get
            {
                return ObjectUtilities.GetIntegerFromString(_Value, 0);
            }
            set
            {
                Value = value.ToString();
            }
        }

        public MultiLanguageString Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasText()
        {
            return _Text != null;
        }

        public virtual string GetTextString()
        {
            return GetTextString(LanguageLookup.English);
        }

        public virtual string GetTextString(LanguageID uiLanguageID)
        {
            if (_Text != null)
                return _Text.Text(uiLanguageID);
            else
                return "";
        }

        public string Tag
        {
            get
            {
                return _Tag;
            }
            set
            {
                if (value != _Tag)
                {
                    _Tag = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsValueType()
        {
            return IsValueType(Type);
        }

        public static bool IsValueType(string annotationType)
        {
            switch (annotationType)
            {
                case "Heading":
                    return false;
                case "Label":
                    return false;
                case "Prefix":
                    return false;
                case "Suffix":
                    return false;
                case "Note":
                    return false;
                case "QuestionNote":
                    return false;
                case "AnswerNote":
                    return false;
                case "FootNote":
                    return false;
                case "Category":
                    return false;
                case "Conjugation":
                    return false;
                case "Break":
                    return false;
                case "Style":
                    return true;
                case "Tag":
                    return true;
                case "Text":
                    return true;
                case "Ordinal":
                    return false;
                case "Picture":
                    return true;
                case "Embedded":
                    return true;
                case "Start":
                    return true;
                case "Stop":
                    return true;
                case "NotMapped":
                    return true;
                default:
                    return false;
            }
        }

        public bool IsTextType()
        {
            return IsTextType(Type);
        }

        public static bool IsTextType(string annotationType)
        {
            switch (annotationType)
            {
                case "Heading":
                    return true;
                case "Label":
                    return true;
                case "Prefix":
                    return true;
                case "Suffix":
                    return true;
                case "Note":
                    return true;
                case "QuestionNote":
                    return true;
                case "AnswerNote":
                    return true;
                case "FootNote":
                    return true;
                case "Category":
                    return true;
                case "Conjugation":
                    return true;
                case "Break":
                    return false;
                case "Style":
                    return false;
                case "Tag":
                    return false;
                case "Text":
                    return true;
                case "Ordinal":
                    return true;
                case "Picture":
                    return false;
                case "Embedded":
                    return false;
                case "Start":
                    return true;
                case "Stop":
                    return false;
                case "NotMapped":
                    return false;
                default:
                    return false;
            }
        }

        public bool IsOverlapping(Annotation other)
        {
            if (other == null)
                return false;

            if (Type != other.Type)
                return false;

            if (IsTextType())
            {
                if (!Text.IsOverlapping(other.Text))
                    return false;
            }
            else if (IsValueType())
            {
                if (Value != other.Value)
                    return false;
            }

            if (Tag != other.Tag)
                return false;

            return true;
        }

        public bool IsOverlappingAnchored(Annotation other, Dictionary<string, bool> anchorLanguageFlags)
        {
            if (other == null)
                return false;

            if (Type != other.Type)
                return false;

            if (IsTextType())
            {
                if (!Text.IsOverlappingAnchored(other.Text, anchorLanguageFlags))
                    return false;
            }
            else if (IsValueType())
            {
                if (Value != other.Value)
                    return false;
            }

            if (Tag != other.Tag)
                return false;

            return true;
        }

        public static List<Annotation> FilteredAnnotations(List<Annotation> annotations, List<LanguageID> languageIDs)
        {
            if (annotations == null)
                return null;

            if (languageIDs == null)
                return new List<Annotation>();

            List<Annotation> newAnnotations = new List<Annotation>(languageIDs.Count());

            foreach (Annotation annotation in annotations)
            {
                Annotation newAnnotation = new Annotation(annotation);

                if (newAnnotation.Text != null)
                    newAnnotation.Text = newAnnotation.Text.FilteredMultiLanguageString(languageIDs);

                newAnnotations.Add(newAnnotation);
            }

            return newAnnotations;
        }

        public static List<Annotation> FilteredAnnotations(List<Annotation> annotations, List<LanguageDescriptor> languageDescriptors)
        {
            if (annotations == null)
                return null;

            if (languageDescriptors == null)
                return new List<Annotation>();

            List<Annotation> newAnnotations = new List<Annotation>(languageDescriptors.Count());

            foreach (Annotation annotation in annotations)
            {
                Annotation newAnnotation = new Annotation(annotation);

                if (newAnnotation.Text != null)
                    newAnnotation.Text = newAnnotation.Text.FilteredMultiLanguageString(languageDescriptors);

                newAnnotations.Add(newAnnotation);
            }

            return newAnnotations;
        }

        public bool Merge(Annotation other)
        {
            if (IsTextType())
                Text.Merge(other.Text);

            return true;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_Text != null)
                {
                    if (_Text.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Text != null)
                    _Text.Modified = false;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("Type", Type));
            if (_Value != null)
                element.Add(new XAttribute("Value", _Value));
            if ((_Text != null) && (_Text.Count() != 0))
                element.Add(_Text.GetElement("Text"));
            if (_Tag != null)
                element.Add(new XAttribute("Tag", _Tag));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Type":
                    Type = attributeValue;
                    break;
                case "Value":
                    _Value = attributeValue;
                    break;
                case "Tag":
                    _Tag = attributeValue;
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
                case "Text":
                    _Text = new MultiLanguageString(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            Annotation otherAnnotation = other as Annotation;

            if (otherAnnotation == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_Value, otherAnnotation.Value);

            if (diff != 0)
                return diff;

            if (otherAnnotation == null)
            {
                if (_Text == null)
                    return 0;
                else
                    return 1;
            }

            if (_Text != null)
            {
                diff = _Text.Compare(otherAnnotation.Text);

                if (diff != 0)
                    return diff;
            }

            diff = ObjectUtilities.CompareStrings(_Tag, otherAnnotation.Tag);

            return diff;
        }

        public static int Compare(Annotation object1, Annotation object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareAnnotationLists(List<Annotation> list1, List<Annotation> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<Annotation>(list1, list2);
        }

        public static bool HasAnnotation(List<Annotation> annotations, string type)
        {
            if ((annotations != null) && !String.IsNullOrEmpty(type))
                return annotations.FirstOrDefault(x => x.Type == type) != null;

            return false;
        }

        public static Annotation AnnotationIndexed(List<Annotation> annotations, int index)
        {
            if ((annotations != null) && (index >= 0) && (index < annotations.Count()))
                return annotations.ElementAt(index);
            return null;
        }

        public static Annotation FindAnnotation(List<Annotation> annotations, string type)
        {
            if ((annotations != null) && !String.IsNullOrEmpty(type))
                return annotations.FirstOrDefault(x => x.Type == type);
            return null;
        }

        public static string AnnotationValue(List<Annotation> annotations, string type)
        {
            Annotation annotation = FindAnnotation(annotations, type);

            if (annotation != null)
                return annotation.Value;

            return String.Empty;
        }

        public static MultiLanguageString AnnotationMultiLanguageString(List<Annotation> annotations, string type)
        {
            Annotation annotation = FindAnnotation(annotations, type);

            if (annotation != null)
                return annotation.Text;

            return null;
        }

        public static string AnnotationText(List<Annotation> annotations, string type, LanguageID languageID)
        {
            MultiLanguageString mls = AnnotationMultiLanguageString(annotations, type);

            if (mls != null)
                return mls.Text(languageID);

            return String.Empty;
        }
    }
}

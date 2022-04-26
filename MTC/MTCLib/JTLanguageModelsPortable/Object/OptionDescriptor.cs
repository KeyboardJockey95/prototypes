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
    public class OptionDescriptor : BaseObjectKeyed
    {
        private string _Type;       // One of OptionTypes
        private string _Label;
        private string _Help;
        private string _Value;
        private List<string> _Values;
        public static List<string> OptionTypes = new List<string>()
        {
            "string",
            "bigstring",
            "text",
            "flag",
            "integer",
            "float",
            "namedLanguage",
            "stringset",
            "componentset",
            "textcomponentset"
        };

        public OptionDescriptor(string name, string type, string label, string help, string value)
            : base(name)
        {
            _Type = type;
            _Label = label;
            _Help = help;
            _Value = value;
            _Values = null;
        }

        public OptionDescriptor(string name, string type, string label, string help, string value, List<string> values)
            : base(name)
        {
            _Type = type;
            _Label = label;
            _Help = help;
            _Value = value;
            _Values = values;
        }

        public OptionDescriptor(OptionDescriptor other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public OptionDescriptor(OptionDescriptor other, object key)
        {
            Copy(other);
            Key = key;
            ModifiedFlag = false;
        }

        public OptionDescriptor(XElement element)
        {
            OnElement(element);
        }

        public OptionDescriptor()
        {
            ClearOptionDescriptor();
        }

        public override void Clear()
        {
            base.Clear();
            ClearOptionDescriptor();
        }

        public void ClearOptionDescriptor()
        {
            _Type = null;
            _Label = null;
            _Help = null;
            _Value = null;
            _Values = null;
        }

        public void Copy(OptionDescriptor other)
        {
            if (other == null)
                ClearOptionDescriptor();
            else
            {
                _Type = other.Type;
                _Label = other.Label;
                _Help = other.Help;
                _Value = other.Value;
                if (other.Values != null)
                    _Values = new List<string>(other.Values);
                else
                    _Values = null;
            }

            ModifiedFlag = true;
        }

        public override IBaseObject Clone()
        {
            return new OptionDescriptor(this);
        }

        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                if (value != _Label)
                {
                    _Label = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Help
        {
            get
            {
                return _Help;
            }
            set
            {
                if (value != _Help)
                {
                    _Help = value;
                    ModifiedFlag = true;
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

        public bool ValueFlag
        {
            get
            {
                return (_Value == "true" ? true : false);
            }
            set
            {
                string stringValue = (value == true ? "true" : "false");
                if (stringValue != _Value)
                {
                    _Value = stringValue;
                    ModifiedFlag = true;
                }
            }
        }

        public int ValueInt
        {
            get
            {
                return ObjectUtilities.GetIntegerFromString(_Value, 0);
            }
            set
            {
                string stringValue = value.ToString();
                if (stringValue != _Value)
                {
                    _Value = stringValue;
                    ModifiedFlag = true;
                }
            }
        }

        public float ValueFloat
        {
            get
            {
                return ObjectUtilities.GetFloatFromString(_Value, 0.0f);
            }
            set
            {
                string stringValue = value.ToString();
                if (stringValue != _Value)
                {
                    _Value = stringValue;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> Values
        {
            get
            {
                return _Values;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(_Values, value) != 0)
                {
                    _Values = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetValueIndexed(int index)
        {
            if ((_Values != null) && (index >= 0) && (index < _Values.Count()))
                return _Values.ElementAt(index);
            return null;
        }

        public void AddValue(string value)
        {
            if (_Values == null)
                _Values = new List<string>(1) { value };
            else
                _Values.Add(value);
            ModifiedFlag = true;
        }

        public bool InsertValue(int index, string value)
        {
            if (_Values == null)
                _Values = new List<string>(1) { value };
            else if ((index >= 0) && (index < _Values.Count()))
                _Values.Insert(index, value);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteValueIndexed(int index)
        {
            if ((_Values != null) && (index >= 0) && (index < _Values.Count()))
                _Values.RemoveAt(index);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public int ValueCount()
        {
            if (_Values != null)
                return _Values.Count();

            return 0;
        }

        public bool HasValues()
        {
            if (_Values != null)
                return _Values.Count() != 0;

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Type", _Type));
            element.Add(new XAttribute("Label", _Label));
            element.Add(new XAttribute("Help", _Help));
            if (_Value != null)
                element.Add(new XAttribute("Value", _Value));
            if ((_Values != null) && (_Values.Count() != 0))
                element.Add(new XAttribute("Values", ObjectUtilities.GetStringFromStringList(_Values)));
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
                case "Label":
                    Label = attributeValue;
                    break;
                case "Help":
                    Help = attributeValue;
                    break;
                case "Value":
                    Value = attributeValue;
                    break;
                case "Values":
                    Values = ObjectUtilities.GetStringListFromString(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            OptionDescriptor otherOptionDescriptor = other as OptionDescriptor;

            if (otherOptionDescriptor == null)
                return base.Compare(other);

            int diff = ObjectUtilities.CompareKeys(this, other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Type, otherOptionDescriptor.Type);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Label, otherOptionDescriptor.Label);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Help, otherOptionDescriptor.Help);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Value, otherOptionDescriptor.Value);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_Values, otherOptionDescriptor.Values);
            return diff;
        }

        public static int Compare(OptionDescriptor object1, OptionDescriptor object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareOptionDescriptors(List<OptionDescriptor> list1, List<OptionDescriptor> list2)
        {
            if (list1 == list2)
                return 0;

            if (list2 == null)
                return 1;

            if (list1 == null)
                return -1;

            int count1 = list1.Count();
            int count2 = list2.Count();
            int count = (count1 > count2 ? count2 : count1);
            int index;
            int diff;
            List<OptionDescriptor> options1 = new List<OptionDescriptor>(list1);
            List<OptionDescriptor> options2 = new List<OptionDescriptor>(list2);

            options1.Sort();
            options2.Sort();

            for (index = 0; index > count; index++)
            {
                diff = OptionDescriptor.Compare(options1[index], options2[index]);

                if (diff != 0)
                    return diff;
            }

            diff = count1 - count2;

            return diff;
        }

        public static List<OptionDescriptor> CopyOptionDescriptorList(List<OptionDescriptor> source)
        {
            if (source == null)
                return null;

            int count = source.Count();
            int index;
            List<OptionDescriptor> list = new List<OptionDescriptor>(count);

            for (index = 0; index < count; index++)
            {
                OptionDescriptor newItem = new OptionDescriptor(source[index]);
                list.Add(newItem);
            }

            return list;
        }

        public static bool GetFlagFromString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            switch (value.ToLower())
            {
                case "true":
                case "on":
                case "yes":
                    return true;
                default:
                    break;
            }

            return false;
        }

        public static string GetFlagStringFromString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return "false";

            switch (value.ToLower())
            {
                case "true":
                case "on":
                case "yes":
                    return "true";
                default:
                    break;
            }

            return "false";
        }
    }
}

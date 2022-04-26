using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public enum DataType
    {
        None,
        String,
        Bool,
        Integer,
        Float,
        Double,
        List,
        Object
    }

    public class DataColumn
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public object DefaultValue { get; set; }
        public DataType MajorType { get; set; }
        public DataType MinorType { get; set; }

        public DataColumn(
            string name,
            object defaultValue,
            DataType majorType,
            DataType minorType)
        {
            Name = name;
            Index = -1;
            DefaultValue = defaultValue;
            MajorType = majorType;
            MinorType = minorType;
        }

        public DataColumn(
            string name,
            object defaultValue,
            DataType majorType)
        {
            Name = name;
            Index = -1;
            DefaultValue = defaultValue;
            MajorType = majorType;
            MinorType = DataType.None;
        }

        public DataColumn(
            string name,
            object defaultValue)
        {
            Name = name;
            Index = -1;
            DefaultValue = defaultValue;
            SetTypeFromValueType(defaultValue);
        }

        public DataColumn(string name)
        {
            Name = name;
            Index = -1;
            DefaultValue = String.Empty;
            MajorType = DataType.String;
            MinorType = DataType.None;
        }

        public DataColumn(DataColumn other)
        {
            Name = other.Name;
            Index = -1;
            DefaultValue = other.DefaultValue;
            MajorType = other.MajorType;
            MinorType = other.MinorType;
        }

        public DataColumn()
        {
            Name = String.Empty;
            Index = -1;
            DefaultValue = String.Empty;
            MajorType = DataType.String;
            MinorType = DataType.None;
        }

        public string DefaultValueString
        {
            get
            {
                return (string)DefaultValue;
            }
            set
            {
                DefaultValue = value;
            }
        }

        public bool DefaultValueBool
        {
            get
            {
                if (DefaultValue == null)
                    return false;
                else if ((DefaultValue is string) && ((string)DefaultValue == String.Empty))
                    return false;

                return (bool)DefaultValue;
            }
            set
            {
                DefaultValue = value;
            }
        }

        public int DefaultValueInteger
        {
            get
            {
                return (int)DefaultValue;
            }
            set
            {
                DefaultValue = value;
            }
        }

        public float DefaultValueFloat
        {
            get
            {
                return (float)DefaultValue;
            }
            set
            {
                DefaultValue = value;
            }
        }

        public double DefaultValueDouble
        {
            get
            {
                return (double)DefaultValue;
            }
            set
            {
                DefaultValue = value;
            }
        }

        public object ParseValue(string str)
        {
            object value;

            switch (MajorType)
            {
                case DataType.None:
                case DataType.String:
                    value = str;
                    break;
                case DataType.Bool:
                    value = ObjectUtilities.GetBoolFromString(str, DefaultValueBool);
                    break;
                case DataType.Integer:
                    {
                        int index = str.IndexOf(".");
                        if (index != -1)
                            str = str.Substring(0, index);
                        value = ObjectUtilities.GetIntegerFromString(str, DefaultValueInteger);
                    }
                    break;
                case DataType.Float:
                    value = ObjectUtilities.GetFloatFromString(str, DefaultValueFloat);
                    break;
                case DataType.Double:
                    value = ObjectUtilities.GetDoubleFromString(str, DefaultValueDouble);
                    break;
                case DataType.List:
                    {
                        string[] parts = str.Split();
                        switch (MinorType)
                        {
                            case DataType.None:
                            case DataType.String:
                                {
                                    List<string> list = new List<string>();
                                    foreach (string part in parts)
                                        list.Add(part);
                                    value = list;
                                }
                                break;
                            case DataType.Bool:
                                {
                                    List<bool> list = new List<bool>();
                                    foreach (string part in parts)
                                        list.Add(ObjectUtilities.GetBoolFromString(part, false));
                                    value = list;
                                }
                                break;
                            case DataType.Integer:
                                {
                                    List<int> list = new List<int>();
                                    foreach (string part in parts)
                                    {
                                        string intStr = part;
                                        int index = intStr.IndexOf(".");
                                        if (index != -1)
                                            intStr = intStr.Substring(0, index);
                                        list.Add(ObjectUtilities.GetIntegerFromString(intStr, 0));
                                    }
                                    value = list;
                                }
                                break;
                            case DataType.Float:
                                {
                                    List<float> list = new List<float>();
                                    foreach (string part in parts)
                                        list.Add(ObjectUtilities.GetFloatFromString(part, 0.0f));
                                    value = list;
                                }
                                break;
                            case DataType.Double:
                                {
                                    List<double> list = new List<double>();
                                    foreach (string part in parts)
                                        list.Add(ObjectUtilities.GetDoubleFromString(part, 0.0));
                                    value = list;
                                }
                                break;
                            default:
                                throw new Exception("ParseValue: Unexpected list minor type.");
                        }
                    }
                    break;
                case DataType.Object:
                    throw new Exception("ParseValue: Sorry, object parsing not supported.");
                default:
                    value = str;
                    break;
            }

            return value;
        }

        public void SetTypeFromValueType(object value)
        {
            MinorType = DataType.None;

            if (value == null)
                MajorType = DataType.None;
            else if (value is string)
                MajorType = DataType.String;
            else if (value is bool)
                MajorType = DataType.Bool;
            else if (value is int)
                MajorType = DataType.Integer;
            else if (value is float)
                MajorType = DataType.Float;
            else if (value is double)
                MajorType = DataType.Double;
            else if (value is List<String>)
            {
                MajorType = DataType.List;
                MinorType = DataType.String;
            }
            else if (value is List<bool>)
            {
                MajorType = DataType.List;
                MinorType = DataType.Bool;
            }
            else if (value is List<int>)
            {
                MajorType = DataType.List;
                MinorType = DataType.Integer;
            }
            else if (value is List<float>)
            {
                MajorType = DataType.List;
                MinorType = DataType.Float;
            }
            else if (value is List<Double>)
            {
                MajorType = DataType.List;
                MinorType = DataType.Double;
            }
            else
                MajorType = DataType.Object;
        }

        public void SetTypeFromString(string value)
        {
            MinorType = DataType.None;

            if (String.IsNullOrEmpty(value))
                MajorType = DataType.String;
            else if ((value == "0") || (value == "1"))
                MajorType = DataType.Bool;
            else if (ObjectUtilities.IsIntegerString(value))
                MajorType = DataType.Integer;
            else if (ObjectUtilities.IsFloatString(value))
                MajorType = DataType.Float;
            else if (value.Contains(","))
            {
                MajorType = DataType.List;
                string[] parts = value.Split(LanguageLookup.Comma);
                string oneValue = parts[0];
                if (String.IsNullOrEmpty(oneValue))
                    MinorType = DataType.String;
                else if ((oneValue == "0") || (oneValue == "1"))
                    MinorType = DataType.Bool;
                else if (ObjectUtilities.IsIntegerString(oneValue))
                    MinorType = DataType.Integer;
                else if (ObjectUtilities.IsFloatString(oneValue))
                    MinorType = DataType.Float;
            }
            else
                MajorType = DataType.Object;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(" ");
            sb.Append(Index);
            sb.Append(" ");
            sb.Append(DefaultValue != null ? DefaultValue.ToString() : "(null)");
            sb.Append(" ");
            sb.Append(MajorType.ToString());
            sb.Append(" ");
            sb.Append(MinorType.ToString());

            return sb.ToString();
        }
    }
}

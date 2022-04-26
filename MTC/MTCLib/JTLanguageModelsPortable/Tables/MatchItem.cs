using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public enum MatchItemCompareType
    {
        Equals,
        NotEquals,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
        Contains,
        NotContains,
        Between,
        Outside
    }

    public class MatchItem
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public MatchItemCompareType CompareType { get; set; }

        public MatchItem(
            string key,
            object value,
            MatchItemCompareType compareType)
        {
            Key = key;
            Value = value;
            CompareType = compareType;
        }

        public MatchItem()
        {
            Key = String.Empty;
            Value = null;
            CompareType = MatchItemCompareType.Equals;
        }

        public bool IsMatch(object value)
        {
            int compareValue = 0;
            bool returnValue = false;

            if ((Value == null) && (value == null))
                return true;

            if ((Value == null) || (value == null))
                return false;

            switch (CompareType)
            {
                case MatchItemCompareType.Equals:
                case MatchItemCompareType.NotEquals:
                case MatchItemCompareType.LessThan:
                case MatchItemCompareType.LessOrEqual:
                case MatchItemCompareType.GreaterThan:
                case MatchItemCompareType.GreaterOrEqual:
                    if (value is string)
                    {
                        if (Value is List<string>)
                        {
                            string valueString = (string)value;
                            string[] parts = valueString.Split(LanguageLookup.Comma);
                            compareValue = ObjectUtilities.CompareStringLists(parts.ToList(), ValueAsStringList);
                        }
                        else
                            compareValue = CompareValue(Value, value);
                    }
                    else if (value is bool)
                    {
                        if (Value is bool)
                            compareValue = CompareValue(Value, value);
                        else
                            throw new Exception("Unsupported match type.");
                    }
                    else if (value is int)
                    {
                        if (Value is int)
                            compareValue = CompareValue(Value, value);
                        else if (Value is bool)
                        {
                            int intValue = (int)value;
                            bool boolValue;
                            if (intValue == 1)
                                boolValue = true;
                            else if (intValue == 0)
                                boolValue = false;
                            else
                                throw new Exception("Unsupported bool value for integer.");
                            compareValue = CompareValue(Value, boolValue);
                        }
                        else
                            throw new Exception("Unsupported match type.");
                    }
                    else if (value is float)
                    {
                        if (Value is float)
                            compareValue = CompareValue(Value, value);
                        else
                            throw new Exception("Unsupported match type.");
                    }
                    else if (value is double)
                    {
                        if (Value is double)
                            compareValue = CompareValue(Value, value);
                        else
                            throw new Exception("Unsupported match type.");
                    }
                    switch (CompareType)
                    {
                        case MatchItemCompareType.Equals:
                            returnValue = (compareValue == 0 ? true : false);
                            break;
                        case MatchItemCompareType.NotEquals:
                            returnValue = (compareValue != 0 ? true : false);
                            break;
                        case MatchItemCompareType.LessThan:
                            returnValue = (compareValue < 0 ? true : false);
                            break;
                        case MatchItemCompareType.LessOrEqual:
                            returnValue = (compareValue <= 0 ? true : false);
                            break;
                        case MatchItemCompareType.GreaterThan:
                            returnValue = (compareValue > 0 ? true : false);
                            break;
                        case MatchItemCompareType.GreaterOrEqual:
                            returnValue = (compareValue >= 0 ? true : false);
                            break;
                    }
                    break;
                case MatchItemCompareType.Contains:
                case MatchItemCompareType.NotContains:
                    if (value is string)
                    {
                        if (Value is List<string>)
                            compareValue = (ValueAsStringList.Contains((string)value) ? 1 : 0);
                        else
                            throw new Exception("Value needs to be a List<string>.");
                    }
                    else if (value is int)
                    {
                        if (Value is int)
                            compareValue = (ValueAsIntegerList.Contains((int)value) ? 0 : 1);
                        else
                            throw new Exception("Unsupported match type.");
                    }
                    else
                        throw new Exception("Type mismatch.");
                    switch (CompareType)
                    {
                        case MatchItemCompareType.Contains:
                            returnValue = (compareValue == 1 ? true : false);
                            break;
                        case MatchItemCompareType.NotContains:
                            returnValue = (compareValue != 1 ? true : false);
                            break;
                    }
                    break;
                case MatchItemCompareType.Between:
                case MatchItemCompareType.Outside:
                    throw new Exception("Unsupported compare type: " + CompareType.ToString());
                default:
                    throw new Exception("Unsupported compare type: " + CompareType.ToString());
            }

            return returnValue;
        }

        protected int CompareValue(object pattern, object value)
        {
            if ((pattern == null) && (value == null))
                return 0;

            if (pattern == null)
                return -1;

            if (value == null)
                return 1;

            if (pattern.GetType() != value.GetType())
                throw new Exception("Type mismatch: pattern = " + pattern.ToString() + " value = " + value.ToString());

            if (pattern is string)
            {
                if ((string)pattern == "*")
                    return 0;
                return StringComparer.OrdinalIgnoreCase.Compare((string)pattern, (string)value);
            }
            else if (pattern is int)
                return (int)pattern - (int)value;
            else if (pattern is bool)
            {
                int patternInt = ((bool)pattern ? 1 : 0);
                int valueInt = ((bool)value ? 1 : 0);
                return patternInt - valueInt;
            }
            else if (pattern is float)
            {
                float patternFloat = (float)pattern;
                float valueFloat = (float)value;

                if (patternFloat == valueFloat)
                    return 0;
                else if (patternFloat < valueFloat)
                    return -1;
                else
                    return 1;
            }
            else if (pattern is double)
            {
                double patternDouble = (double)pattern;
                double valueDouble = (double)value;

                if (patternDouble == valueDouble)
                    return 0;
                else if (patternDouble < valueDouble)
                    return -1;
                else
                    return 1;
            }
            else
                throw new Exception("Unsupported types:  pattern = " + pattern.ToString() + " value = " + value.ToString());
        }

        public List<string> ValueAsStringList
        {
            get
            {
                return (List<string>)Value;
            }
        }

        public List<bool> ValueAsBoolList
        {
            get
            {
                return (List<bool>)Value;
            }
        }

        public List<int> ValueAsIntegerList
        {
            get
            {
                return (List<int>)Value;
            }
        }

        public List<float> ValueAsFloatList
        {
            get
            {
                return (List<float>)Value;
            }
        }

        public List<double> ValueAsDoubleList
        {
            get
            {
                return (List<double>)Value;
            }
        }

        public override string ToString()
        {
            string valueString;

            if (Value == null)
                valueString = null;
            else if (Value is List<string>)
                valueString = ObjectUtilities.GetStringFromStringList(ValueAsStringList);
            else if (Value is List<int>)
                valueString = ObjectUtilities.GetStringFromIntList(ValueAsIntegerList);
            else if (Value is List<float>)
                valueString = ObjectUtilities.GetStringFromFloatList(ValueAsFloatList);
            else if (Value is List<double>)
                valueString = ObjectUtilities.GetStringFromDoubleList(ValueAsDoubleList);
            else
                valueString = Value.ToString();

            return "Key = " + Key + ", Value = " + valueString + ", CompareType = " + CompareType.ToString();
        }
    }
}

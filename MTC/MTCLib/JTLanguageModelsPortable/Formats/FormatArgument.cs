using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatArgument
    {
        public string Name { get; set; }
        public string NameLower { get { return Name.ToLower(); } }
        public string Type { get; set; }
        public string Direction { get; set; } // "r", "w", "rw"
        public List<object> Values { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Help { get; set; }
        public List<string> FlagOnDependents { get; set; }
        public List<string> FlagOffDependents { get; set; }
        public bool PostOnChange { get; set; }
        public static List<string> ArgumentTypes = new List<string>()
        {
            "string",
            "bigstring",
            "text",
            "flag",
            "integer",
            "float",
            "languageID",
            "languagelist",
            "titledobjectlist",
            "stringlist",
            "flaglist",
            "languageflaglist"
        };

        public FormatArgument(string name, string type, string direction, string value, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            Name = name;
            Type = type;
            Direction = direction;
            Value = value;
            Label = label;
            Help = help;
            FlagOnDependents = flagOnDependents;
            FlagOffDependents = flagOffDependents;
            PostOnChange = false;
        }

        public FormatArgument(string name, string label)
        {
            Name = name;
            Type = null;
            Direction = null;
            Value = null;
            Label = label;
            Help = null;
            FlagOnDependents = null;
            FlagOffDependents = null;
            PostOnChange = false;
        }

        public FormatArgument(string name, string type, string direction, string label)
        {
            Name = name;
            Type = type;
            Direction = direction;
            Value = null;
            Label = label;
            Help = null;
            FlagOnDependents = null;
            FlagOffDependents = null;
            PostOnChange = false;
        }

        public FormatArgument(FormatArgument other)
        {
            Copy(other);
        }

        public FormatArgument()
        {
            Name = null;
            Type = null;
            Direction = null;
            Value = null;
            Label = null;
            Help = null;
            FlagOnDependents = null;
            FlagOffDependents = null;
            PostOnChange = false;
        }

        public void Copy(FormatArgument other)
        {
            Name = other.Name;
            Type = other.Type;
            Direction = other.Direction;
            Value = other.Value;

            if (other.Values != null)
                Values = new List<object>(other.Values);
            else
                Values = null;

            Label = other.Label;
            Help = other.Help;
            if (other.FlagOnDependents != null)
                FlagOnDependents = new List<string>(other.FlagOnDependents);
            else
                FlagOnDependents = null;
            if (other.FlagOffDependents != null)
                FlagOffDependents = new List<string>(other.FlagOffDependents);
            else
                FlagOffDependents = null;
            PostOnChange = other.PostOnChange;
        }

        public List<BaseString> GetStringList()
        {
            if (Values == null)
                return null;

            return Values.Cast<BaseString>().ToList();
        }

        public void SetStringList(List<BaseString> values)
        {
            if (values == null)
                Values = null;
            else
                Values = values.Cast<object>().ToList();
        }

        public List<T> GetTitledBaseList<T>() where T : BaseObjectTitled
        {
            if (Values == null)
                return null;

            return Values.Cast<T>().ToList();
        }

        public void SetTitledBaseList<T>(List<T> values) where T : BaseObjectTitled
        {
            if (values == null)
                Values = null;
            else
                Values = values.Cast<object>().ToList();
        }

        public List<T> GetTypedValues<T>()
        {
            if (Values != null)
                return Values.Cast<T>().ToList();

            return null;
        }

        public void SetTypedValues<T>(List<T> values)
        {
            if (values != null)
                Values = values.Cast<object>().ToList();
            else
                Values = null;
        }

        public List<LanguageID> GetLanguageListValue()
        {
            List<LanguageID> list = null;

            if (Value != null)
                list = TextUtilities.GetLanguageIDListFromString(Value);

            return list;
        }

        public void SetLanguageListValue(List<LanguageID> value)
        {
            string valueString = String.Empty;

            if (value != null)
                valueString = TextUtilities.GetStringFromLanguageIDList(value);

            Value = valueString;
        }

        public Dictionary<string, bool> GetFlagDictionaryValue()
        {
            Dictionary<string, bool> flagDictionary = null;

            if (Value != null)
                flagDictionary = TextUtilities.GetFlagDictionaryFromString(Value, null);

            return flagDictionary;
        }

        public void SetFlagDictionaryValue(Dictionary<string, bool> value)
        {
            string valueString = String.Empty;

            if (value != null)
                valueString = TextUtilities.GetStringFromFlagDictionary(value);

            Value = valueString;
        }

        public bool HasFlagDependents()
        {
            if ((FlagOnDependents != null) || (FlagOffDependents != null))
                return true;
            return false;
        }

        public override string ToString()
        {
            string name = (Name != null ? Name: "(name null)");
            string label = (Label != null ? Label : "(label null)");
            string type = (Type != null ? Type : "(type null)");
            string direction = (Direction != null ? Direction : "(direction null)");
            string value = (Value != null ? Value : "(value null)");
            string help = (Help != null ? Help : "(help null)");
            return "\"" + name + "\", \"" + label + "\", \"" + type + "\", \"" + direction + "\", \"" + value + "\", \"" + help + "\"";
        }

        public static int CompareNames(FormatArgument object1, FormatArgument object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return String.Compare(object1.Name, object2.Name);
        }
    }
}

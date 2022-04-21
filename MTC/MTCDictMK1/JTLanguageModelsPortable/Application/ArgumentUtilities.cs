using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;

namespace JTLanguageModelsPortable.Application
{
    public class ArgumentUtilities : ControllerUtilities
    {
        public string Name { get; set; }                        // Name of argument group.
        public string Type { get; set; }                        // Type name of object associated with arguments.
        public string Description { get; set; }                 // Description of argument group or associate object.
        public List<FormatArgument> Arguments { get; set; }     // The list of argument descriptors with values.
        public List<string> NonOptionArguments { get; set; }    // Raw list of other string values that were not options.
        public ArgumentUtilities ParentArguments { get; set; }  // Link to parent arguments.

        public delegate FormatArgument FindArgumentDelegate(string name);
        public delegate bool HandleArgumentDelegate(FormatArgument argument);

        public ArgumentUtilities(
                string name,                                // Name of argument group.
                string type,                                // Type name of object associated with arguments.
                string description,                         // Description of argument group or associated object.
                UserRecord userRecord,                      // User record associated with this instance. May be null.
                UserProfile userProfile,                    // User profile associated with this instance. May be null.
                IMainRepository repositories,               // Repositories if needed for certain arguments, i.e. master list. May be null.
                IApplicationCookies cookies,                // Web cookies.  May be null.
                ILanguageTranslator translator,             // Tanslator to translate labels or help.  May be null for no translation.
                LanguageUtilities languageUtilities,        // Language utilities to translate labels or help.
                                                            // May be null for no translations or to create one from the translator.
                ArgumentUtilities parentArguments)          // Link to parent arguments.
            : base(
                repositories,
                cookies,
                userRecord,
                userProfile,
                translator,
                languageUtilities)
        {
            ClearArgumentUtilities();
            Name = name;
            Type = type;
            Description = description;
            UserRecord = userRecord;
            UserProfile = userProfile;
            Repositories = repositories;
            LanguageUtilities = languageUtilities;
            ParentArguments = parentArguments;
        }

        public ArgumentUtilities(
                string name,                                // Name of argument group.
                string type,                                // Type name of object associated with arguments.
                string description)                         // Description of argument group or associated object.
        {
            ClearArgumentUtilities();
            Name = name;
            Type = type;
            Description = description;
        }

        public ArgumentUtilities(ArgumentUtilities other) :
            base(other)
        {
            CopyArgumentUtilities(other);
        }

        public ArgumentUtilities()
        {
            ClearArgumentUtilities();
        }

        public void ClearArgumentUtilities()
        {
            Name = null;
            Type = null;
            Description = null;
            Arguments = null;
            NonOptionArguments = null;
            ParentArguments = null;
            UserRecord = null;
            UserProfile = null;
            Repositories = null;
            LanguageUtilities = null;
            Error = null;
        }

        public void CopyArgumentUtilities(ArgumentUtilities other)
        {
            Name = other.Name;
            Type = other.Type;
            Description = other.Description;

            if (other.Arguments != null)
            {
                Arguments = new List<FormatArgument>();
                foreach (FormatArgument argument in other.Arguments)
                {
                    FormatArgument newArgument = new FormatArgument(argument);
                    Arguments.Add(newArgument);
                }
            }
            else
                Arguments = null;

            ParentArguments = other.ParentArguments;

            UILanguageID = other.UILanguageID;
            UserRecord = other.UserRecord;
            UserProfile = other.UserProfile;
            Repositories = other.Repositories;
            LanguageUtilities = other.LanguageUtilities;
            Error = null;
        }

        public virtual ArgumentUtilities Clone()
        {
            return new ArgumentUtilities(this);
        }

        public static ArgumentUtilities CloneArgumentUtilities(ArgumentUtilities other)
        {
            if (other == null)
                return null;

            return new ArgumentUtilities(other);
        }

        // Initialize arguments.
        // Override this to populate the Arguments list.
        public virtual void InitializeArguments()
        {
        }

        // Load arguments from the command line.
        public virtual bool LoadFromCommandLine(
            string[] args,
            FindArgumentDelegate findArgumentFunction = null,
            HandleArgumentDelegate handleArgumentFunction = null)
        {
            if (args == null)
                return true;

            int argumentIndex;
            int argumentCount = args.Count();
            bool returnValue = true;

            for (argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
            {
                string arg = args[argumentIndex];
                bool argIsOption = false;
                string nextArg = (argumentIndex + 1 < argumentCount ? args[argumentIndex + 1] : null);
                bool haveAssignArg = false;
                bool nextArgIsOption = false;

                switch (arg.ToLower())
                {
                    case "--help":
                    case "/?":
                    case "-?":
                    case "?":
                        return false;
                    default:
                        break;
                }

                if (arg.StartsWith("--"))
                {
                    argIsOption = true;
                    arg = arg.Substring(2);
                }
                else if (arg.StartsWith("-"))
                {
                    argIsOption = true;
                    arg = arg.Substring(1);
                }

                int assignOffset = arg.IndexOf('=');

                if (assignOffset != -1)
                {
                    nextArg = arg.Substring(assignOffset + 1);
                    arg = arg.Substring(0, assignOffset);
                    haveAssignArg = true;
                }
                else if (!String.IsNullOrEmpty(nextArg))
                {
                    if (nextArg.StartsWith("-"))
                        nextArgIsOption = true;
                }

                if (!argIsOption)
                {
                    AddNonOptionArgument(arg);
                    continue;
                }

                FormatArgument argument = null;

                if (findArgumentFunction != null)
                    argument = findArgumentFunction(arg);

                if (argument == null)
                {
                    argument = FindArgument(arg);

                    if (argument == null)
                    {
                        PutErrorArgument("Unexpected command line argument", arg);
                        returnValue = false;
                        continue;
                    }
                }

                string argumentValue = String.Empty;

                switch (argument.Type)
                {
                    case "flag":
                        if (nextArgIsOption || String.IsNullOrEmpty(nextArg))
                            argumentValue = "true";      // Assume presence of argument means "true".
                        else
                        {
                            switch (nextArg.ToLower())
                            {
                                case "true":
                                    argumentValue = "true";
                                    break;
                                case "false":
                                    argumentValue = "false";
                                    break;
                                default:
                                    if (haveAssignArg)
                                    {
                                        PutErrorArgument("Invalid flag value", nextArg);
                                        continue;
                                    }
                                    argumentValue = "true";      // Assume presence of argument means "true".
                                    break;
                            }
                        }
                        break;
                    case "string":
                    case "bigstring":
                    case "text":
                    case "integer":
                    case "float":
                    case "languageID":
                    case "languagelist":
                    case "titledobjectlist":
                    case "stringlist":
                    case "flaglist":
                    case "languageflaglist":
                        argumentValue = nextArg;
                        break;
                    default:
                        PutErrorArgument("Unexpected argument type", argument.Type);
                        returnValue = false;
                        continue;
                }

                argument.Value = argumentValue;

                if (handleArgumentFunction != null)
                {
                    if (!handleArgumentFunction(argument))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public virtual string GetUsageMessage(string mainUsagePreamble, string prefix)
        {
            return GetUsageMessage(Arguments, mainUsagePreamble, prefix);
        }

        public virtual string GetUsageMessage(
            List<FormatArgument> arguments,
            string mainUsagePreamble,
            string prefix)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(mainUsagePreamble))
                sb.AppendLine(mainUsagePreamble);

            if (arguments != null)
            {
                foreach (FormatArgument argument in arguments)
                {
                    if (!String.IsNullOrEmpty(prefix))
                        sb.Append(prefix);

                    sb.Append("--");
                    sb.Append(argument.Name);
                    sb.Append("[=]");

                    switch (argument.Type)
                    {
                        case "flag":
                            sb.Append("true|false|(empty for true) ");
                            break;
                        case "string":
                        case "bigstring":
                        case "text":
                            sb.Append("(string) ");
                            break;
                        case "integer":
                            sb.Append("(integer) ");
                            break;
                        case "float":
                            sb.Append("(float) ");
                            break;
                        case "languageID":
                            sb.Append("(two-letter language code) ");
                            break;
                        case "languagelist":
                            sb.Append("(comma-separated list of two-letter language code) ");
                            break;
                        case "titledobjectlist":
                        case "stringlist":
                            {
                                List<BaseString> strs = argument.GetStringList();
                                if ((strs == null) || (strs.Count() == 0))
                                    sb.Append("(string - (no string list values specified) ");
                                else
                                {
                                    sb.Append("(string) One of:");

                                    foreach (BaseString str in strs)
                                        sb.Append("\n" + prefix + "    " + str.Text);
                                }
                            }
                            break;
                        case "flaglist":
                            sb.Append("(comma-separated list of true|false) ");
                            break;
                        case "languageflaglist":
                            sb.Append("(comma-separated list of true|false) ");
                            break;
                        default:
                            sb.Append("(unexpected argument type: " + argument.Type);
                            break;
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public virtual void PrefetchArguments(FormReader formReader)
        {
        }

        public virtual void LoadFromArguments()
        {
        }

        public virtual void SaveToArguments()
        {
        }

        public int ArgumentCount()
        {
            int count = 0;

            if (Arguments != null)
                count += Arguments.Count();

            if (ParentArguments != null)
                count += ParentArguments.ArgumentCount();

            return count;
        }

        public void DeleteArgument(string name)
        {
            if (Arguments == null)
                return;

            FormatArgument arg = FindArgument(name);

            if (arg != null)
                Arguments.Remove(arg);
        }

        public void DeleteArgumentIndexed(int index)
        {
            if (Arguments == null)
                return;

            if ((index >= 0) && (index < ArgumentCount()))
                Arguments.RemoveAt(index);
        }

        public int GetArgumentIndex(FormatArgument arg)
        {
            int index = -1;

            if (Arguments == null)
            {
                if (ParentArguments != null)
                    index = ParentArguments.GetArgumentIndex(arg);
            }
            else
            {
                index = Arguments.IndexOf(arg);

                if ((index == -1) && (ParentArguments != null))
                {
                    index = ParentArguments.GetArgumentIndex(arg);

                    if (index != -1)
                        index += Arguments.Count();
                }
            }

            return index;
        }

        public virtual string GetArgumentKeyName(string name)
        {
            string keyName = (!String.IsNullOrEmpty(Type) ? Type + "." : "") + name;
            return keyName;
        }

        public string GetNonOptionArgumentIndexed(int index)
        {
            if (NonOptionArguments == null)
                return null;
            else if ((index >= 0) && (index < NonOptionArguments.Count()))
                return NonOptionArguments[index];

            return null;
        }

        public void AddNonOptionArgument(string str)
        {
            if (NonOptionArguments == null)
                NonOptionArguments = new List<string>() { str };
            else
                NonOptionArguments.Add(str);
        }

        public FormatArgument GetArgumentIndexed(int index)
        {
            if (index < 0)
                return null;
            else if (Arguments == null)
            {
                if (ParentArguments != null)
                    return ParentArguments.GetArgumentIndexed(index);

                return null;
            }
            else if (index < Arguments.Count())
                return Arguments[index];
            else if (ParentArguments != null)
                return ParentArguments.GetArgumentIndexed(index - Arguments.Count());

            return null;
        }

        public FormatArgument FindArgument(string name)
        {
            FormatArgument argument = null;

            if (Arguments != null)
            {
                string nameLower = name.ToLower();
                argument = Arguments.FirstOrDefault(x => x.NameLower == nameLower);
            }

            if ((argument == null) && (ParentArguments != null))
                argument = ParentArguments.FindArgument(name);

            return argument;
        }

        public FormatArgument FindOrCreateArgument(string name, string type, string direction, string value,
            List<object> values, string label, string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, flagOnDependents, flagOffDependents);
                argument.Values = values;
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            return argument;
        }

        public string GetArgument(string name, string defaultValue = "")
        {
            FormatArgument argument = FindArgument(name);

            if (argument != null)
                return argument.Value;

            return defaultValue;
        }

        public List<FormatArgument> CloneArguments()
        {
            if (Arguments == null)
                return null;
            List<FormatArgument> arguments = new List<FormatArgument>();
            foreach (FormatArgument argument in Arguments)
            {
                FormatArgument newArgument = new FormatArgument(argument);
                arguments.Add(newArgument);
            }
            return arguments;
        }

        public void CollectArguments(List<FormatArgument> arguments)
        {
            if (Arguments != null)
                arguments.AddRange(Arguments);

            if (ParentArguments != null)
                ParentArguments.CollectArguments(arguments);
        }

        public bool SetArgumentValue(string name, string value)
        {
            FormatArgument argument = FindArgument(name);

            if (argument == null)
                return false;

            argument.Value = value;

            return true;
        }

        public bool GetArgumentAsFlag(string name, bool defaultValue = false)
        {
            string value = GetArgument(name);
            bool returnValue;

            switch (value)
            {
                case "true":
                    returnValue = true;
                    break;
                case "false":
                    returnValue = false;
                    break;
                default:
                    returnValue = defaultValue;
                    break;
            }

            return returnValue;
        }

        public int GetArgumentAsInteger(string name, int defaultValue = 0)
        {
            string value = GetArgument(name);
            return ObjectUtilities.GetIntegerFromString(value, defaultValue);
        }

        public float GetArgumentAsFloat(string name, float defaultValue = 0.0f)
        {
            string value = GetArgument(name);
            return ObjectUtilities.GetFloatFromString(value, defaultValue);
        }

        public LanguageID GetArgumentAsLanguageID(string name, LanguageID defaultValue = null)
        {
            string value = GetArgument(name);
            LanguageID returnValue = defaultValue;

            if (!String.IsNullOrEmpty(value))
                returnValue = LanguageLookup.GetLanguageIDNoAdd(value);

            return returnValue;
        }

        public List<LanguageID> GetArgumentAsLanguageIDList(string name, List<LanguageID> defaultValue = null)
        {
            string value = GetArgument(name);
            List<LanguageID> returnValue = defaultValue;

            if (!String.IsNullOrEmpty(value))
                returnValue = ObjectUtilities.GetLanguageIDListFromString(value);

            return returnValue;
        }

        public List<string> GetArgumentAsStringList(string name, List<string> defaultValue = null)
        {
            string value = GetArgument(name);
            List<string> returnValue = defaultValue;

            if (value != null)
            {
                List<string> lines = TextUtilities.GetStringListFromStringDelimited(value, "\r\n");

                returnValue = new List<string>();

                foreach (string line in lines)
                    returnValue.AddRange(TextUtilities.GetStringListFromStringDelimited(value, ","));
            }
            else
                returnValue = defaultValue;

            return returnValue;
        }

        public string GetArgumentDefaulted(string name, string type, string direction, string defaultValue, string label, string help)
        {
            if (UserProfile != null)
            {
                string profileValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValue);

                if (!String.IsNullOrEmpty(profileValue))
                    defaultValue = profileValue;
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue, null, label, help, null, null);

            if (argument != null)
                return argument.Value;

            return defaultValue;
        }

        public List<string> GetArgumentStringListDefaulted(string name, string type, string direction, List<string> defaultValue, string label, string help)
        {
            string stringValue = TextUtilities.GetStringFromStringList(defaultValue);

            if (UserProfile != null)
            {
                string profileValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), stringValue);

                if (!String.IsNullOrEmpty(profileValue))
                    stringValue = profileValue;
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, stringValue, null, label, help, null, null);

            if (argument != null)
            {
                stringValue = argument.Value;
                List<string> stringList = TextUtilities.GetStringListFromStringDelimited(stringValue, "\r\n");
                int c = stringList.Count();
                int i;
                for (i = c - 1; i >= 0; i--)
                {
                    string s = stringList[i].Trim();
                    if (String.IsNullOrEmpty(s))
                        stringList.RemoveAt(i);
                    else
                        stringList[i] = s;
                }
                return stringList;
            }

            return defaultValue;
        }

        public int GetIntegerArgumentDefaulted(string name, string type, string direction, int defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionInteger(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue.ToString(), null, label, help, null, null);

            int value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                try
                {
                    value = Convert.ToInt32(argument.Value);
                }
                catch (Exception)
                {
                }
            }

            return value;
        }

        public float GetFloatArgumentDefaulted(string name, string type, string direction, float defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionFloat(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue.ToString(), null, label, help, null, null);

            float value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                try
                {
                    value = (float)Convert.ToDouble(argument.Value);
                }
                catch (Exception)
                {
                }
            }

            return value;
        }

        public bool GetFlagArgumentDefaulted(string name, string type, string direction, bool defaultValue, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            if (UserProfile != null)
                defaultValue = (UserProfile.GetUserOptionString(GetArgumentKeyName(name), (defaultValue ? "on" : "off")) == "on");

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue ? "on" : "off", null, label,
                help, flagOnDependents, flagOffDependents);

            bool value = defaultValue;

            if ((argument != null) && (argument.Value != null))
            {
                if (argument.Value == "on")
                    value = true;
                else
                    value = false;
            }

            return value;
        }

        public Dictionary<string, bool> GetFlagListArgumentDefaulted(string name, string type, string direction,
            Dictionary<string, bool> defaultValue, List<string> values,
            string label, string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string defaultValueString = String.Empty;

            if (defaultValue != null)
                defaultValueString = TextUtilities.GetStringFromFlagDictionary(defaultValue);

            List<object> valuesObjectList = null;

            if (values != null)
                valuesObjectList = values.Cast<object>().ToList();

            //if (UserProfile != null)
            //    defaultValueString = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValueString);

            FormatArgument argument = FindOrCreateArgument(name, type, direction,
                defaultValueString, valuesObjectList, label,
                help, flagOnDependents, flagOffDependents);

            Dictionary<string, bool> value = defaultValue;

            if ((argument != null) && (argument.Value != null))
                value = TextUtilities.GetFlagDictionaryFromString(argument.Value, null);

            return value;
        }

        public LanguageID GetLanguageIDArgumentDefaulted(string name, string type, string direction, LanguageID defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionLanguageID(GetArgumentKeyName(name), defaultValue);

            string defaultString = (defaultValue != null ? defaultValue.ToString() : String.Empty);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultString, null, label, help, null, null);

            LanguageID value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
                value = LanguageLookup.GetLanguageIDNoAdd(argument.Value);

            return value;
        }

        public List<LanguageID> GetLanguageIDListArgumentDefaulted(string name, string type, string direction, List<LanguageID> defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionLanguageIDList(GetArgumentKeyName(name), defaultValue);

            string stringValue = TextUtilities.GetStringFromLanguageIDList(defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, stringValue,
                null, label, help, null, null);

            List<LanguageID> value;

            if (argument != null)
                value = TextUtilities.GetLanguageIDListFromString(argument.Value);
            else
                value = defaultValue;

            return value;
        }

        public string GetStringListArgumentDefaulted(string name, string type, string direction, string defaultValue,
            List<string> stringValues, string label, string help)
        {
            if (UserProfile != null)
            {
                string testValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValue);

                if (!String.IsNullOrEmpty(testValue))
                    defaultValue = testValue;
            }

            List<object> values = null;

            if (stringValues != null)
            {
                values = new List<object>();

                foreach (string stringValue in stringValues)
                {
                    string translatedString;
                    if (LanguageUtilities != null)
                        translatedString = LanguageUtilities.TranslateUIString(stringValue);
                    else
                        translatedString = stringValue;
                    values.Add(new BaseString(stringValue, translatedString));
                }
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue, values, label, help, null, null);

            string value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                value = argument.Value;

                if (!stringValues.Contains(value))
                    value = "(invalid)";
            }

            return value;
        }

        public string GetMasterListArgumentDefaulted(string name, string type, string direction, string defaultValue,
            string label, string help)
        {
            List<string> masterList = GetMasterStringList(defaultValue);
            return GetStringListArgumentDefaulted(name, type, direction, defaultValue,
                masterList, label, help);
        }

        public FormatArgument SetArgument(string name, string type, string direction, string value, string label, string help,
            List<string> flagOnDependents, List<string> flagOffDependents)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, flagOnDependents, flagOffDependents);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = value;

            return argument;
        }

        public FormatArgument SetArgumentStringList(string name, string type, string direction, List<string> value, string label, string help,
            List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string stringValue = TextUtilities.GetStringFromStringListDelimited(value, "\r\n");
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, stringValue, label, help, flagOnDependents, flagOffDependents);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = stringValue;

            return argument;
        }

        public FormatArgument SetIntegerArgument(string name, string type, string direction, int value, string label, string help)
        {
            return SetArgument(name, type, direction, value.ToString(), label, help, null, null);
        }

        public FormatArgument SetFloatArgument(string name, string type, string direction, float value, string label, string help)
        {
            return SetArgument(name, type, direction, value.ToString(), label, help, null, null);
        }

        public FormatArgument SetFlagArgument(string name, string type, string direction, bool value, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            return SetArgument(name, type, direction, (value ? "on" : "off"), label, help, flagOnDependents, flagOffDependents);
        }

        public FormatArgument SetFlagListArgument(string name, string type, string direction,
            Dictionary<string, bool> value, List<string> values, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string valueString = String.Empty;

            if (value != null)
                valueString = TextUtilities.GetStringFromFlagDictionary(value);

            List<object> valuesObjectList = null;

            if (values != null)
                valuesObjectList = values.Cast<object>().ToList();

            FormatArgument argument = SetArgument(name, type, direction, valueString, label, help, flagOnDependents, flagOffDependents);

            if (argument != null)
                argument.Values = valuesObjectList;

            return argument;
        }

        public FormatArgument SetLanguageIDArgument(string name, string type, string direction, LanguageID value, string label, string help)
        {
            string stringValue;
            if (value != null)
                stringValue = value.LanguageCultureExtensionCode;
            else
                stringValue = String.Empty;
            return SetArgument(name, type, direction, stringValue, label, help, null, null);
        }

        public FormatArgument SetLanguageIDListArgument(string name, string type, string direction, List<LanguageID> value, string label, string help)
        {
            string stringValue = TextUtilities.GetStringFromLanguageIDList(value);
            return SetArgument(name, type, direction, stringValue, label, help, null, null);
        }

        public FormatArgument SetStringListArgument(string name, string type, string direction, string value, List<string> stringValues,
            string label, string help)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, null, null);
                List<object> values = null;
                if (stringValues != null)
                {
                    values = new List<object>();
                    foreach (string stringValue in stringValues)
                    {
                        string translatedString;
                        if (LanguageUtilities != null)
                            translatedString = LanguageUtilities.TranslateUIString(stringValue);
                        else
                            translatedString = stringValue;
                        values.Add(new BaseString(stringValue, translatedString));
                    }
                }
                argument.Values = values;
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = value;

            return argument;
        }

        public FormatArgument SetMasterListArgument(string name, string type, string direction, string value,
            string label, string help)
        {
            List<string> masterList = GetMasterStringList(value);
            return SetStringListArgument(name, type, direction, value, masterList, label, help);
        }

        public List<string> GetMasterStringList(string currentMasterName)
        {
            List<NodeMaster> masters = Repositories.NodeMasters.GetList(UserRecord.UserName);

            if (masters == null)
                masters = new List<NodeMaster>();

            List<string> masterStrings = new List<string>();

            foreach (NodeMaster master in masters)
                masterStrings.Add(master.GetTitleString(UILanguageID));

            masterStrings.Add("(none)");

            if (!String.IsNullOrEmpty(currentMasterName) && !masterStrings.Contains(currentMasterName))
                masterStrings.Add(currentMasterName);

            return masterStrings;
        }

        public virtual void CopyArgumentValues(Format other)
        {
            if (Arguments == null)
                return;

            foreach (FormatArgument argument in Arguments)
            {
                FormatArgument otherArgument = other.FindArgument(argument.Name);

                if (otherArgument != null)
                    argument.Value = otherArgument.Value;
            }
        }

        public virtual void SaveUserOptions()
        {
            if (UserProfile == null)
                return;

            if (Arguments == null)
                return;

            foreach (FormatArgument argument in Arguments)
                UserProfile.SetUserOptionString(GetArgumentKeyName(argument.Name), argument.Value);
        }

        public virtual void DumpArguments(string label)
        {
            List<FormatArgument> arguments = new List<FormatArgument>();

            CollectArguments(arguments);

            if (!String.IsNullOrEmpty(label))
                DumpString(label + ":\n");

            if (arguments.Count() == 0)
            {
                DumpString("(no arguments");
                return;
            }

            foreach (FormatArgument argument in arguments)
                DumpArgument(argument);
        }

        public virtual void DumpArgument(FormatArgument argument)
        {
            DumpArgument(argument.Label, argument.Name, argument.Type, argument.Value);
        }

        public virtual void DumpArgument(string label, string name, string type, string value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            string quotedValue = value;

            if (type == "flag")
            {
                if ((quotedValue == null) || (quotedValue.Length == 0) || (quotedValue == "off"))
                    quotedValue = "false";
                else if ((quotedValue == "on") || (quotedValue == "yes"))
                    quotedValue = "true";
            }

            quotedValue = "\"" + quotedValue + "\"";

            if (type == "flag")
            {
                if (value == "off")
                    value = null;
            }

            if (quotedValue != null)
                msg += quotedValue;
            else
                msg += "(null)";

            if (!String.IsNullOrEmpty(name))
            {
                msg += " (--" + name;

                if (value != null)
                    msg += " " + value;

                msg += ")";
            }

            DumpString(msg);
        }

        public virtual void DumpArgumentList<T>(string label, List<T> value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            if (value != null)
            {
                int index = 0;

                foreach (object item in value)
                {
                    if (index != 0)
                        msg += ", ";

                    msg += item.ToString();
                    index++;
                }
            }
            else
                msg += "(null)";

            DumpString(msg);
        }

        public virtual void Sort()
        {
            Arguments.Sort(FormatArgument.CompareNames);
        }
    }
}

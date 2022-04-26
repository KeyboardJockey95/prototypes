using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Application
{
    public class FormUtilities : ControllerUtilities
    {
        public Dictionary<string, string> FormValues;
        public Dictionary<string, string> FieldErrors;

        public FormUtilities(IMainRepository repositories, IApplicationCookies cookies,
                UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
                LanguageUtilities languageUtilities, Dictionary<string, string> formValues)
            : base(repositories, cookies, userRecord, userProfile, translator, languageUtilities)
        {
            FormValues = formValues;
            FieldErrors = new Dictionary<string, string>();
        }

        public string this[string name]
        {
            get
            {
                if (FormValues.ContainsKey(name))
                    return FormValues[name];
                else if (name.Contains("."))
                {
                    string newName = name.Replace('.', '_');
                    if (FormValues.ContainsKey(newName))
                        return FormValues[newName];
                }
                else if (name.Contains("_"))
                {
                    string newName = name.Replace('_', '.');
                    if (FormValues.ContainsKey(newName))
                        return FormValues[newName];
                }

                return String.Empty;
            }
            set
            {
                if (FormValues.ContainsKey(name))
                    FormValues[name] = value;
                else
                    FormValues.Add(name, value);
            }
        }

        public static string[] SecurityPatternsCrossSiteScripting = new string[]
        {
            @".*((\%3c)|<)((\%73)|s|(\%53))((\%63)|c|(\%43))((\%72)|r|(\%52))((\%69)|i|(\%49))((\%70)|p|(\%50))((\%74)|t|(\%54)).*",
            @".*((\%3c)|<)((\%6f)|o|(\%4f))((\%62)|b|(\%42))((\%6a)|j|(\%4a))((\%65)|e|(\%45))((\%63)|c|(\%43))((\%74)|t|(\%54)).*"
        };

        public static bool SecurityCheck(string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;

            foreach (string pattern in SecurityPatternsCrossSiteScripting)
            {
                if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        public static bool SecurityTextCheck(string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;

            foreach (string pattern in SecurityPatternsCrossSiteScripting)
            {
                if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        public static bool SecurityFileCheck(string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;

            if (value.Contains("..") || value.Contains(".\\") || value.Contains("./"))
                return false;

            return true;
        }

        public string FieldNameMessage(string name)
        {
            string fieldName = name;

            if (String.IsNullOrEmpty(fieldName))
                return S("This field");

            return S(fieldName);
        }

        public override void PutError(string fieldName, string message, string argument)
        {
            string str;

            if (!String.IsNullOrEmpty(fieldName))
            {
                str = FieldNameMessage(fieldName) + ": " + S(message) + ": " + argument;

                if (FieldErrors.ContainsKey(fieldName))
                    FieldErrors[fieldName] += "\r\n" + str;
                else
                    FieldErrors.Add(fieldName, str);
            }
            else
                str = S(message) + ": " + argument;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else
                Error += "\r\n" + str;
        }

        public override void PutError(string fieldName, string message)
        {
            string str;

            if (!String.IsNullOrEmpty(fieldName))
            {
                str = FieldNameMessage(fieldName) + ": " + S(message);

                if (FieldErrors.ContainsKey(fieldName))
                    FieldErrors[fieldName] += "\r\n" + str;
                else
                    FieldErrors.Add(fieldName, str);
            }
            else
                str = S(message);

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else
                Error += "\r\n" + str;
        }

        public override void PutError(string message)
        {
            string str = S(message);

            if (FieldErrors.ContainsKey("All"))
                FieldErrors["All"] += "\r\n" + str;
            else
                FieldErrors.Add("All", str);

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else
                Error += "\r\n" + str;
        }

        public virtual bool IsValid
        {
            get
            {
                return FieldErrors.Count() == 0;
            }
        }

        public bool Assert(bool condition, string fieldName, string message)
        {
            if (!condition)
                PutError(fieldName, message);
            else
                return true;

            return false;
        }

        public bool AssertSecurity(string str, string fieldName)
        {
            if (!SecurityCheck(str))
            {
                Repositories.Log(
                    "Suspected security issue, fieldName=" + (fieldName != null ? fieldName : "") + ", \"" + str + "\".",
                    "AssertSecurity",
                    UserRecord);
                PutError(fieldName, FieldNameMessage(fieldName) + " " +
                    S("contains a potential security violation. Please revise to not use scripts or SQL-like statements."));
            }
            else
                return true;

            return false;
        }

        public bool AssertTextSecurity(string text, string fieldName)
        {
            if (!SecurityTextCheck(text))
            {
                Repositories.Log(
                    "Suspected security issue, fieldName=" + (fieldName != null ? fieldName : "") + ", \"" + text + "\".",
                    "AssertTextSecurity",
                    UserRecord);
                PutError(fieldName, FieldNameMessage(fieldName) + " " +
                    S("contains a potential security violation. Please revise to not use scripts."));
            }
            else
                return true;

            return false;
        }

        public bool AssertNotEmpty(string str, string fieldName)
        {
            if (InputValidator.IsEmpty(str))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("should not be empty."));
            else
                return true;

            return false;
        }

        public bool AssertNotEmpty(BaseString languageString, string fieldName)
        {
            if ((languageString == null) || InputValidator.IsEmpty(languageString.Text))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("should not be empty."));
            else
                return true;

            return false;
        }

        public bool AssertNotEmpty(LanguageString languageString, string fieldName)
        {
            if ((languageString == null) || InputValidator.IsEmpty(languageString.Text))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("should not be empty."));
            else if (!InputValidator.IsValidLanguageID(languageString.LanguageID))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("needs a language."));
            else
                return true;

            return false;
        }

        public bool AssertNotEmpty(MultiLanguageString multiLanguageString, string fieldName)
        {
            bool returnValue = true;

            if ((multiLanguageString == null) || (multiLanguageString.Count() == 0))
            {
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("should not be empty."));
                returnValue = false;
            }
            else
            {
                foreach (LanguageString languageString in multiLanguageString.LanguageStrings)
                {
                    if ((languageString == null) || InputValidator.IsEmpty(languageString.Text))
                    {
                        PutError(fieldName, FieldNameMessage(fieldName) + " " + S("should not be empty."));
                        returnValue = false;
                        break;
                    }
                    else if (!InputValidator.IsValidLanguageID(languageString.LanguageID))
                    {
                        PutError(fieldName, FieldNameMessage(fieldName) + " " + S("needs a language."));
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool AssertSelectNotEmpty(string str, string fieldName)
        {
            if (InputValidator.IsEmpty(str) || (str == "(add)") || (str == "(select)"))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("needs to be selected."));
            else
                return true;

            return false;
        }

        public string HostMemberStringError(string str, string fieldName, List<string> members)
        {
            StringBuilder sb = new StringBuilder(FieldNameMessage(fieldName), 256);
            sb.Append(" (");
            sb.Append(str);
            sb.Append(") ");
            sb.Append(S("must be one of"));
            sb.Append(": (");

            foreach (string member in members)
            {
                sb.Append(S(member));
                sb.Append(", ");
            }

            sb.Append(")");

            return sb.ToString();
        }

        public bool AssertValidSelectString(string str, string fieldName, List<string> members)
        {
            if (InputValidator.IsEmpty(str) || (str == "(select)"))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("needs to be selected."));
            else if (!InputValidator.IsValidSelectString(str, members))
                PutError(fieldName, HostMemberStringError(str, fieldName, members));
            else
                return true;

            return false;
        }

        public bool AssertValidLessonTargetString(string str, string fieldName)
        {
            return AssertValidSelectString(str, fieldName, InputValidator.LessonTextTargetStrings);
        }

        public bool AssertValidLessonFormatString(string str, string fieldName)
        {
            return AssertValidSelectString(str, fieldName, InputValidator.LessonTextFormatStrings);
        }

        public bool AssertEqual(object object1, object object2, string fieldName)
        {
            if (object1 != object2)
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("value") + " "
                    + S(object1.ToString()) + " " + S("should be") + " " + S(object2.ToString()));
            else
                return true;

            return false;
        }

        public bool AssertValidNumberString(string str, string fieldName)
        {
            if (!InputValidator.IsValidNumber(str))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("does not have a valid number."));
            else
                return true;

            return false;
        }

        public bool AssertValidUnsignedNumberString(string str, string fieldName)
        {
            if (!InputValidator.IsValidUnsignedNumber(str))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("does not have a valid number."));
            else
                return true;

            return false;
        }

        public bool AssertValidFloatString(string str, string fieldName)
        {
            if (!InputValidator.IsValidFloat(str))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("does not have a valid number."));
            else
                return true;

            return false;
        }

        public bool AssertValidLanguageID(LanguageID languageID, string fieldName)
        {
            string message;

            if (!LanguageUtilities.ValidateLanguageID(languageID, out message))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + message);
            else
                return true;

            return false;
        }

        public bool AssertPasswordConfirmation(string password, string passwordConfirmation, string fieldName)
        {
            if (password != passwordConfirmation)
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("doesn't match."));
            else
                return true;

            return false;
        }

        public bool AssertMinimumStringLength(string str, int length, string fieldName)
        {
            if (InputValidator.IsTooShort(str, length))
                PutError(fieldName, FieldNameMessage(fieldName) + " " +
                    S("must be at least") + " " + length + " " + S("characters"));
            else
                return true;

            return false;
        }

        public bool AssertValidEmailAddress(string email, string fieldName)
        {
            if (!InputValidator.IsValidEmail(email))
                PutError(fieldName, FieldNameMessage(fieldName) + " " + S("is not a valid email address."));
            else if (AssertSecurity(email, fieldName))
                return true;

            return false;
        }

        public bool AddValues(List<string> editKeys, List<string> editValues, bool clearFirst)
        {
            if ((editKeys == null) || (editValues == null))
                return true;

            int count = editKeys.Count;
            if (count != editValues.Count)
            {
                PutError("editKeys and editValues counts don't match.");
                return false;
            }

            if (clearFirst)
                FormValues.Clear();

            for (int index = 0; index < count; index++)
            {
                string editKey = editKeys[index];
                string editValue = editValues[index];
                string oldValue;

                if (FormValues.TryGetValue(editKey, out oldValue))
                    FormValues[editKey] = editValue;
                else
                    FormValues.Add(editKey, editValue);
            }

            return true;
        }

        public bool HasField(string name)
        {
            return FormValues.ContainsKey(name);
        }

        public string GetString(string name)
        {
            string value = GetString(name, String.Empty);
            return value;
        }

        public string GetString(string name, string defaultValue)
        {
            string value = this[name];
            if (String.IsNullOrEmpty(value))
                value = defaultValue;
            AssertSecurity(value, name);
            return value;
        }

        public string GetSelectString(string name)
        {
            string value = this[name];

            switch (value)
            {
                case "(select)":
                    value = String.Empty;
                    break;
                default:
                    break;
            }
            AssertSecurity(value, name);
            return value;
        }

        public int GetInteger(string name)
        {
            string value = this[name];
            if (AssertValidNumberString(value, name))
            {
                try
                {
                    int intValue = Convert.ToInt32(value);
                    return intValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        public int GetInteger(string name, int defaultValue)
        {
            string value = this[name];
            if (String.IsNullOrEmpty(value))
                return defaultValue;
            if (AssertValidNumberString(value, name))
            {
                try
                {
                    int intValue = Convert.ToInt32(value);
                    return intValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        public long GetLong(string name)
        {
            string value = this[name];
            if (AssertValidNumberString(value, name))
            {
                try
                {
                    long longValue = Convert.ToInt64(value);
                    return longValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        public long GetLong(string name, long defaultValue)
        {
            string value = this[name];
            if (String.IsNullOrEmpty(value))
                return defaultValue;
            if (AssertValidNumberString(value, name))
            {
                try
                {
                    long longValue = Convert.ToInt64(value);
                    return longValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        private static char[] commaSep = new char[] { ',' };

        public List<int> GetIntegerList(string name)
        {
            string value = this[name];
            if (value == null)
                return null;
            value = value.Trim();
            if (String.IsNullOrEmpty(value))
                return null;
            value = value.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            string[] parts = value.Split(commaSep);
            List<int> returnValue = new List<int>();
            foreach (string s in parts)
            {
                if (AssertValidNumberString(s, name))
                {
                    try
                    {
                        int intValue = Convert.ToInt32(s);
                        returnValue.Add(intValue);
                    }
                    catch (Exception)
                    {
                        PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                        return null;
                    }
                }
            }
            return returnValue;
        }

        public double GetDouble(string name)
        {
            string value = this[name];
            if (AssertValidFloatString(value, name))
            {
                try
                {
                    double doubleValue = Convert.ToDouble(value);
                    return doubleValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        public float GetFloat(string name, float defaultValue)
        {
            return (float)GetDouble(name, defaultValue);
        }

        public double GetDouble(string name, double defaultValue)
        {
            string value = this[name];
            if (String.IsNullOrEmpty(value))
                return defaultValue;
            if (AssertValidFloatString(value, name))
            {
                try
                {
                    double doubleValue = Convert.ToDouble(value);
                    return doubleValue;
                }
                catch (Exception)
                {
                    PutError(name, FieldNameMessage(name) + " " + S("does not have a valid number."));
                }
            }
            return 0;
        }

        public static bool GetFlagValue(string value)
        {
            return ((value == "on") || (value == "true") ? true : false);
        }

        public static bool GetFlagValue(string value, bool defaultValue)
        {
            return ((value == "on") || (value == "true") ? true : defaultValue);
        }

        public bool GetFlag(string name)
        {
            string value = GetString(name).ToLower();
            bool returnValue;
            switch (value)
            {
                case "on":
                case "true":
                    returnValue = true;
                    break;
                case "off":
                case "false":
                    returnValue = false;
                    break;
                default:
                    returnValue = false;
                    break;
            }
            return returnValue;
        }

        public bool GetFlag(string name, bool defaultValue)
        {
            string value = GetString(name).ToLower();
            bool returnValue;
            switch (value)
            {
                case "on":
                case "true":
                    returnValue = true;
                    break;
                case "off":
                case "false":
                    returnValue = false;
                    break;
                default:
                    returnValue = defaultValue;
                    break;
            }
            return returnValue;
        }

        public List<bool> GetFlagList(string name, int count)
        {
            List<bool> flagList = new List<bool>();

            for (int index = 0; index < count; index++)
                flagList.Add(GetFlag(name + index.ToString()));

            return flagList;
        }

        public List<bool> GetFlagList(string name)
        {
            int count = GetInteger(name + "count", 0);
            return GetFlagList(name, count);
        }

        public TimeSpan GetTimeOffset(string name)
        {
            string value = GetString(name).Trim();
            TimeSpan returnValue = TimeSpan.Zero;
            if (!String.IsNullOrEmpty(value))
            {
                try
                {
                    if (value.Contains(":"))
                        returnValue = TimeSpan.Parse(value);
                    else
                    {
                        double seconds = Double.Parse(value);
                        returnValue = MediaUtilities.ConvertSecondsToTimeSpan(seconds);
                    }
                }
                catch (Exception exception)
                {
                    string message = exception.Message;

                    if (exception.InnerException != null)
                        message += ": " + exception.InnerException;

                    PutError(name, S(name) + " " + S(message));
                }
            }
            return returnValue;
        }

        public bool GetTimeStartStop(string startName, string stopName, out TimeSpan startTime, out TimeSpan stopTime)
        {
            startTime = GetTimeOffset(startName);
            stopTime = GetTimeOffset(stopName);

            if (stopTime < startTime)
            {
                PutError(stopName, "Stop time must be less than start time.");
                return false;
            }

            return true;
        }

        public List<TimeSpan> GetTimeSpanList(string name,
            List<TimeSpan> defaultList)
        {
            List<int> ints = GetIntegerList(name);
            List<TimeSpan> returnValue = new List<TimeSpan>();

            while (ints.Count() % 4 != 0)
                ints.Add(0);

            if ((ints != null) && (ints.Count() != 0))
            {
                int count = ints.Count();
                int index;

                for (index = 0; index < count; index += 4)
                {
                    TimeSpan ts = new TimeSpan(ints[index], ints[index + 1], ints[index + 2], ints[index + 3]);
                    returnValue.Add(ts);
                }
            }

            if (returnValue.Count() == 0)
                returnValue = defaultList;

            return returnValue;
        }

        public DateTime GetDateTime(string name)
        {
            string value = GetString(name).Trim();
            DateTime returnValue = DateTime.UtcNow;
            if (!String.IsNullOrEmpty(value))
            {
                try
                {
                    returnValue = DateTime.Parse(value);
                }
                catch (Exception exception)
                {
                    string message = exception.Message;

                    if (exception.InnerException != null)
                        message += ": " + exception.InnerException;

                    PutError(name, S(name) + " " + S(message));
                }
            }
            return returnValue;
        }

        public CopyPasteType GetCopyPasteType(string name, CopyPasteType defaultValue)
        {
            string stringValue = GetString(name);
            CopyPasteType returnValue = defaultValue;

            if (!String.IsNullOrEmpty(stringValue))
                returnValue = ContentUtilities.GetCopyPasteType(stringValue);

            return returnValue;
        }

        public Dictionary<string, string> GetCheckedItemStringDictionaryNumeric(string name)
        {
            Dictionary<string, string> checkedItems = new Dictionary<string, string>();
            int offset = name.Length;

            foreach (KeyValuePair<string, string> kvp in FormValues)
            {
                string cbName = kvp.Key;

                if (!cbName.StartsWith(name))
                    continue;

                if ((offset >= cbName.Length) || !char.IsDigit(cbName[offset]))
                    continue;

                string suffix = cbName.Substring(offset);
                bool flag = GetFlag(cbName);

                if (flag)
                {
                    string keyName = name + "key_" + suffix;
                    string key = GetString(keyName);
                    checkedItems.Add(cbName, key);
                }
            }

            return checkedItems;
        }

        public List<string> GetCheckedItemStringKeysNumeric(string name)
        {
            List<string> checkedItems = new List<string>();
            int offset = name.Length;

            foreach (KeyValuePair<string, string> kvp in FormValues)
            {
                string cbName = kvp.Key;

                if (!cbName.StartsWith(name))
                    continue;

                if ((offset >= cbName.Length) || !char.IsDigit(cbName[offset]))
                    continue;

                string suffix = cbName.Substring(offset);
                bool flag = GetFlag(cbName);

                if (flag)
                {
                    string keyName = name + "key_" + suffix;
                    string key = GetString(keyName);
                    checkedItems.Add(key);
                }
            }

            return checkedItems;
        }

        public List<string> GetCheckedItemStringKeys(string name)
        {
            List<string> checkedItems = new List<string>();
            int count = GetInteger(name + "count");
            int index;

            for (index = 0; index < count; index++)
            {
                string cbName = name + index.ToString();
                bool flag = GetFlag(cbName);
                if (flag)
                {
                    string keyName = name + "key_" + index.ToString();
                    string key = GetString(keyName);
                    checkedItems.Add(key);
                }
            }

            return checkedItems;
        }

        public List<int> GetCheckedItemIntKeys(string name)
        {
            List<int> checkedItems = new List<int>();
            int count = GetInteger(name + "count");
            int index;

            for (index = 0; index < count; index++)
            {
                string cbName = name + index.ToString();
                bool flag = GetFlag(cbName);
                if (flag)
                {
                    string keyName = name + "key_" + index.ToString();
                    int key = GetInteger(keyName);
                    checkedItems.Add(key);
                }
            }

            return checkedItems;
        }

        public Dictionary<int, bool> GetCheckedItemFlags(string name)
        {
            Dictionary<int, bool> checkedItems = new Dictionary<int, bool>();
            int count = GetInteger(name + "Count");
            int index;

            for (index = 0; index < count; index++)
            {
                string cbName = name + index.ToString();
                bool flag = GetFlag(cbName);
                checkedItems.Add(index, flag);
            }

            return checkedItems;
        }

        public Dictionary<int, bool> GetCheckedItemFlags(string name, int count)
        {
            Dictionary<int, bool> checkedItems = new Dictionary<int, bool>();
            int index;

            for (index = 0; index < count; index++)
            {
                string cbName = name + index.ToString();
                bool flag = GetFlag(cbName);
                checkedItems.Add(index, flag);
            }

            return checkedItems;
        }

        public Dictionary<string, bool> GetCheckedStringFlags(string name)
        {
            Dictionary<string, bool> checkedString = new Dictionary<string, bool>();
            int count = GetInteger(name + "Count");
            int index;

            for (index = 0; index < count; index++)
            {
                string baseName = name + index.ToString();
                string str = GetString(baseName + "String");
                bool flag = GetFlag(baseName + "Flag");
                checkedString.Add(str, flag);
            }

            return checkedString;
        }

        public List<string> GetCompoundStringList(string name)
        {
            string key = name + ".Count";
            if (HasField(key))
            {
                int count = Convert.ToInt32(this[key]);
                List<string> newStringList = new List<string>(count);
                for (int index = 0; index < count; index++)
                {
                    string newString = this[name + "." + index];
                    if ((newString != "(remove)") && (newStringList.Contains(newString) == false))
                    {
                        if (AssertSecurity(newString, name))
                            newStringList.Add(newString);
                    }
                }
                return newStringList;
            }
            else
                return new List<string>();
        }

        public bool HasCompoundStringList(string name)
        {
            string key = name + ".Count";
            if (HasField(key))
                return true;
            return false;
        }

        public List<string> GetStringList(string name)
        {
            string listString = GetString(name);
            List<string> newStringList;
            if (!String.IsNullOrEmpty(listString))
                newStringList = TextUtilities.GetStringListFromString(listString);
            else
                newStringList = new List<string>();
            return newStringList;
        }

        public LanguageID GetLanguageID(string name, LanguageID defaultValue)
        {
            string languageCode = GetString(name);
            if (String.IsNullOrEmpty(languageCode))
                return defaultValue;
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return languageID;
        }

        public List<LanguageID> GetLanguageIDList(string name)
        {
            List<string> stringList = GetCompoundStringList(name);
            int count = stringList.Count();
            List<LanguageID> newLanguageIDList = new List<LanguageID>(count);
            LanguageID lid;
            bool done = false;
            foreach (string lidString in stringList)
            {
                switch (lidString)
                {
                    case "(remove)":
                    case "(add)":
                        break;
                    case "(target languages)":
                    case "(host languages)":
                    case "(my languages)":
                    case "(any)":
                    case "(all languages)":
                        lid = LanguageLookup.GetLanguageIDNoAdd(lidString);
                        newLanguageIDList =  new List<LanguageID>(1) { lid };
                        done = true;
                        break;
                    case "(none)":
                        lid = LanguageLookup.GetLanguageIDNoAdd(lidString);
                        newLanguageIDList = new List<LanguageID>();
                        done = true;
                        break;
                    default:
                        lid = LanguageLookup.GetLanguageID(lidString);
                        newLanguageIDList.Add(lid);
                        break;
                }
                if (done)
                    break;
            }
            string newLanguageCultureExtension = this[name + ".Add"];
            if (!String.IsNullOrEmpty(newLanguageCultureExtension) && (newLanguageCultureExtension != "(add)"))
                newLanguageIDList.Add(LanguageLookup.GetLanguageID(newLanguageCultureExtension));
            return newLanguageIDList;
        }

        public bool HasLanguageIDList(string name)
        {
            return HasCompoundStringList(name);
        }

        public List<LanguageID> GetLanguageIDsFromCode(string name, string defaultValue)
        {
            List<LanguageID> languageIDs = new List<LanguageID>();
            string languageCode = GetString(name);
            if (String.IsNullOrEmpty(languageCode))
                languageCode = defaultValue;
            switch (languageCode)
            {
                case "(none)":
                case "":
                case null:
                    break;
                case "(my languages)":
                    if (UserProfile != null)
                        languageIDs.AddRange(UserProfile.LanguageIDs);
                    break;
                case "(target languages)":
                    if (UserProfile != null)
                        languageIDs.AddRange(UserProfile.TargetLanguageIDs);
                    break;
                case "(host languages)":
                    if (UserProfile != null)
                        languageIDs.AddRange(UserProfile.HostLanguageIDs);
                    break;
                case "(any)":
                case "(any language)":
                    languageIDs.Add(new LanguageID("(any)"));
                    break;
                case "(all languages)":
                    languageIDs.Add(new LanguageID("(all languages)"));
                    break;
                default:
                    {
                        LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
                        languageIDs.Add(languageID);
                    }
                    break;
            }
            return languageIDs;
        }

        public List<LanguageID> GetLanguageIDCheckboxes(string name)
        {
            int count = Convert.ToInt32(this[name + ".Count"]);
            List<LanguageID> languageIDList = new List<LanguageID>(count);
            List<LanguageID> newLanguageIDList = new List<LanguageID>(count);
            string reverse = this[name + ".reverse"];

            for (int index = 0; index < count; index++)
            {
                string newString = this[name + ".lid." + index];
                languageIDList.Add(new LanguageID(newString));
            }

            for (int index = 0; index < count; index++)
            {
                string newString = this[name + "." + index];

                if (newString == "on")
                {
                    if (reverse == "on")
                        newLanguageIDList.Insert(0, languageIDList.ElementAt(index));
                    else
                        newLanguageIDList.Add(languageIDList.ElementAt(index));
                }
            }

            return newLanguageIDList;
        }

        public List<LanguageID> GetLanguageIDsFromFlags(string name, List<LanguageID> allLanguageIDs)
        {
            int count = allLanguageIDs.Count();
            List<LanguageID> languageIDList = new List<LanguageID>(count);

            for (int index = 0; index < count; index++)
            {
                string flagString = this[name + index.ToString()];
                if (flagString == "on")
                    languageIDList.Add(allLanguageIDs[index]);
            }

            return languageIDList;
        }

        public MultiLanguageString GetMultiLanguageString(string name)
        {
            string countString = this[name + ".Count"];
            if (String.IsNullOrEmpty(countString))
                return null;
            int count = Convert.ToInt32(countString);
            string stringID = GetString(name + ".sid");
            MultiLanguageString multiLanguageString = new MultiLanguageString(stringID);
            int index;

            for (index = 0; index < count; index++)
            {
                string languageCode = this[name + ".lid." + index.ToString()];
                LanguageID languageID = new LanguageID(languageCode);
                string value = GetString(name + "." + index.ToString());

                //if (!String.IsNullOrEmpty(value))
                    multiLanguageString.Add(new LanguageString(stringID, languageID, value));
            }

            return multiLanguageString;
        }

        public MultiLanguageString UpdateMultiLanguageString(string name, MultiLanguageString multiLanguageString)
        {
            if (multiLanguageString == null)
                return GetMultiLanguageString(name);

            int count = Convert.ToInt32(this[name + ".Count"]);
            string stringID = GetString(name + ".sid");
            int index;

            for (index = 0; index < count; index++)
            {
                string languageCode = this[name + ".lid." + index.ToString()];
                LanguageID languageID = new LanguageID(languageCode);
                string value = GetString(name + "." + index.ToString());
                LanguageString languageString = multiLanguageString.LanguageString(languageID);

                if (languageString == null)
                {
                    //if (!String.IsNullOrEmpty(value))
                        multiLanguageString.Add(new LanguageString(stringID, languageID, value));
                }
                //else if (String.IsNullOrEmpty(value))
                //    multiLanguageString.Delete(languageString);
                else
                    languageString.Text = value;
            }

            return multiLanguageString;
        }

        public MultiLanguageString GetTranslateMultiLanguageString(string name)
        {
            MultiLanguageString multiLanguageString = GetMultiLanguageString(name);

            if (IsTranslate())
            {
                string message;
                LanguageUtilities.Translator.TranslateMultiLanguageString(multiLanguageString, UserProfile.LanguageIDs, out message, false);
            }
            else if (IsClear())
                multiLanguageString.ClearText();

            return multiLanguageString;
        }

        public void UpdateTranslateMultiLanguageString(string name, MultiLanguageString obj)
        {
            UpdateMultiLanguageString(name, obj);

            if (IsValid)
                Translate(obj);
        }

        public List<MultiLanguageString> GetMultiLanguageStrings(string name)
        {
            List<MultiLanguageString> multiLanguageStrings = new List<MultiLanguageString>();
            MultiLanguageString multiLanguageString;
            int count = Convert.ToInt32(this[name + ".Count"]);
            int index;

            for (index = 0; index < count; index++)
            {
                string singleName = name + index.ToString();
                multiLanguageString = GetMultiLanguageString(singleName);
                multiLanguageStrings.Add(multiLanguageString);
            }

            return multiLanguageStrings;
        }

        public List<MultiLanguageString> GetTranslateMultiLanguageStrings(string name)
        {
            List<MultiLanguageString> multiLanguageStrings = GetMultiLanguageStrings(name);
            List<LanguageID> languageIDs = UserProfile.LanguageIDs;
            string message;

            if (IsTranslate())
            {
                foreach (MultiLanguageString multiLanguageString in multiLanguageStrings)
                    LanguageUtilities.Translator.TranslateMultiLanguageString(multiLanguageString, languageIDs, out message, false);
            }

            return multiLanguageStrings;
        }

        public List<MultiLanguageString> GetAddMultiLanguageStrings(string name)
        {
            List<MultiLanguageString> multiLanguageStrings = new List<MultiLanguageString>();
            MultiLanguageString multiLanguageString;
            int count = Convert.ToInt32(this[name + ".Count"]);
            int index;

            for (index = 0; index < count; index++)
            {
                string singleName = name + index.ToString();
                multiLanguageString = GetMultiLanguageString(singleName);
                string singleKeyName = name + "Key" + index.ToString();
                string key = GetString(singleKeyName);
                if (String.IsNullOrEmpty(key) || (key == "(new)"))
                {
                    if (multiLanguageString.HasLanguageID(UserProfile.HostLanguageID))
                        key = multiLanguageString.Text(UserProfile.HostLanguageID);
                    else if (multiLanguageString.HasLanguageID(UserProfile.UILanguageID))
                        key = multiLanguageString.Text(UserProfile.UILanguageID);
                    else
                        key = multiLanguageString.Text(0);
                }
                multiLanguageString.SetKeys(key);
                multiLanguageString.Modified = false;
                multiLanguageStrings.Add(multiLanguageString);
            }

            return multiLanguageStrings;
        }

        public List<MultiLanguageString> GetAddTranslateMultiLanguageStrings(string name)
        {
            List<MultiLanguageString> multiLanguageStrings = GetAddMultiLanguageStrings(name);
            List<LanguageID> languageIDs = UserProfile.LanguageIDs;
            string message;

            if (IsTranslate())
            {
                foreach (MultiLanguageString multiLanguageString in multiLanguageStrings)
                    LanguageUtilities.Translator.TranslateMultiLanguageString(multiLanguageString, languageIDs, out message, false);
            }

            return multiLanguageStrings;
        }

        public LanguageDescriptor GetLanguageDescriptor(string name, string languageDescriptorName)
        {
            string languageCode = GetString(name);

            if (String.IsNullOrEmpty(languageCode))
                return null;

            if (languageCode.StartsWith("("))
                return null;

            LanguageID languageID = LanguageLookup.GetLanguageID(languageCode);
            LanguageDescriptor languageDescriptor = new LanguageDescriptor(languageDescriptorName, languageID, true);

            return languageDescriptor;
        }

        public List<LanguageDescriptor> GetLanguageDescriptors(string name, string languageDescriptorName)
        {
            List<string> stringList = GetCompoundStringList(name);
            int count = stringList.Count();
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(count);

            foreach (string lidString in stringList)
            {
                if (lidString != "(remove)")
                {
                    LanguageID languageID = LanguageLookup.GetLanguageID(lidString);
                    LanguageDescriptor languageDescriptor = new LanguageDescriptor(languageDescriptorName, languageID, true);
                    languageDescriptors.Add(languageDescriptor);
                }
            }

            string newLanguageCultureExtension = this[name + ".Add"];

            if (!String.IsNullOrEmpty(newLanguageCultureExtension) && (newLanguageCultureExtension != "(add)"))
            {
                LanguageID languageID = LanguageLookup.GetLanguageID(newLanguageCultureExtension);
                LanguageDescriptor languageDescriptor = new LanguageDescriptor(languageDescriptorName, languageID, true);
                languageDescriptors.Add(languageDescriptor);
            }

            return languageDescriptors;
        }

        public XElement GetMarkupElement(string name)
        {
            string markupText = GetString(name);
            XElement markup = null;

            if (ApplicationData.IsMobileVersion)
                markupText = TextUtilities.MobileTextAreaDecode(markupText);

            if (!String.IsNullOrEmpty(markupText))
            {
                try
                {
                    markup = XElement.Parse(markupText, LoadOptions.PreserveWhitespace);
                }
                catch (Exception exception)
                {
                    string message = exception.Message;

                    if (exception.InnerException != null)
                        message += ": " + exception.InnerException;

                    PutError(name, S(name) + " " + S(message));
                }
            }

            return markup;
        }

        public bool IsTranslate()
        {
            string command = this["translateButton"] as string;
            if (String.IsNullOrEmpty(command))
                command = this["command"] as string;
            if (String.IsNullOrEmpty(command))
                command = this["submitButton"] as string;
            if (!String.IsNullOrEmpty(command))
            {
                if (command == "Save and Translate")
                    return true;
                else if (command == "Append and Translate")
                    return true;
                else if (command == "Translate")
                    return true;
                else if (command == "Insert and Translate")
                    return true;
                else if (command == "Reply and Translate")
                    return true;
                else if (command == "Save and Translate")
                    return true;
                else if (command == "Translate and Save")
                    return true;
                else if (command == "Add Translations")
                    return true;
            }
            return false;
        }

        public bool IsClear()
        {
            string command = this["clearButton"] as string;
            if (String.IsNullOrEmpty(command))
                command = this["command"] as string;
            if (String.IsNullOrEmpty(command))
                command = this["submitButton"] as string;
            if (command == "Clear")
                return true;
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Repository
{
    public class PersistentSettingsStorage
    {
        public string StoragePath { get; set; }

        public PersistentSettingsStorage(string storagePath)
        {
            StoragePath = storagePath;
        }

        public string GetString(string key, string defaultValue)
        {
            string path = StoragePath + ApplicationData.PlatformPathSeparator + key;
            string value;

            if (FileSingleton.Exists(path))
                value = FileSingleton.ReadAllText(path);
            else
                value = defaultValue;

            return value;
        }

        public void SetString(string key, string value)
        {
            string path = StoragePath + ApplicationData.PlatformPathSeparator + key;
            FileSingleton.WriteAllText(path, value, ApplicationData.Encoding);
        }

        public int GetInteger(string key, int defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            int value = ObjectUtilities.GetIntegerFromString(str, defaultValue);
            return value;
        }

        public void SetInteger(string key, int value)
        {
            SetString(key, value.ToString());
        }

        public long GetLong(string key, long defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            long value = ObjectUtilities.GetLongFromString(str, defaultValue);
            return value;
        }

        public void SetLong(string key, long value)
        {
            SetString(key, value.ToString());
        }

        public bool GetFlag(string key, bool defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            bool value = ObjectUtilities.GetBoolFromString(str, defaultValue);
            return value;
        }

        public void SetFlag(string key, bool value)
        {
            SetString(key, value.ToString());
        }

        public float GetFloat(string key, float defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            float value = ObjectUtilities.GetFloatFromString(str, defaultValue);
            return value;
        }

        public void SetFloat(string key, float value)
        {
            SetString(key, value.ToString());
        }

        public double GetDouble(string key, double defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            double value = ObjectUtilities.GetDoubleFromString(str, defaultValue);
            return value;
        }

        public void SetDouble(string key, double value)
        {
            SetString(key, value.ToString());
        }

        public TimeSpan GetTimeSpan(string key, TimeSpan defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            TimeSpan value = ObjectUtilities.GetTimeSpanFromString(str, defaultValue);
            return value;
        }

        public void SetTimeSpan(string key, TimeSpan value)
        {
            SetString(key, value.ToString());
        }

        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            string str = GetString(key, defaultValue.ToString());
            DateTime value = ObjectUtilities.GetDateTimeFromString(str, defaultValue);
            return value;
        }

        public void SetDateTime(string key, DateTime value)
        {
            SetString(key, value.ToString());
        }

        public bool GetStringPair(string key, out string str1, out string str2)
        {
            str1 = String.Empty;
            str2 = String.Empty;
            string rawValue = GetString(key, String.Empty);
            if (String.IsNullOrEmpty(rawValue))
                return false;
            string[] values = rawValue.Split(LanguageLookup.Bar, StringSplitOptions.None);
            if (values.Length >= 1)
                str1 = values[0];
            if (values.Length >= 2)
                str2 = values[1];
            return true;
        }

        public void SetStringPair(string key, string str1, string str2)
        {
            string rawValue = str1 + "|" + str2;
            SetString(key, rawValue);
        }

        public List<string> GetStringList(string key)
        {
            string rawValue = GetString(key, String.Empty);
            List<string> value = TextUtilities.GetStringListFromString(rawValue);
            return value;
        }

        public void SetStringList(string key, List<string> value)
        {
            string rawValue = TextUtilities.GetStringFromStringList(value);
            SetString(key, rawValue);
        }
    }
}

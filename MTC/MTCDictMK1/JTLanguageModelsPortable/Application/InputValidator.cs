using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Application
{
    public class InputValidator
    {
        static readonly string URLRegex = @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$";
        static readonly string EmailRegex = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";
        static readonly string NumberRegex = @"^[-]*[0-9]+$";
        static readonly string UnsignedNumberRegex = @"^[0-9]+$";
        static readonly string FloatRegex = @"^[-+]?[0-9]*\.?[0-9]+$";

        public static bool IsEmpty(string value)
        {
            if (String.IsNullOrEmpty((value ?? string.Empty).Trim()))
                return true;
            return false;
        }

        public static bool IsValidString(string value)
        {
            if (String.IsNullOrEmpty((value ?? string.Empty).Trim()))
                return (false);
            return (true);
        }

        public static bool IsTooLong(string value, int length)
        {
            if ((value == null) || (value.Length > length))
                return true;
            return false;
        }

        public static bool IsTooShort(string value, int length)
        {
            if ((value == null) || (value.Length < length))
                return true;
            return false;
        }

        public static bool IsRegexMatch(string value, string regex)
        {
            if (!IsValidString(value))
                return false;
            bool returnValue = false;
            try
            {
                returnValue = Regex.IsMatch(value, regex);
            }
            catch
            {
            }
            return returnValue;
        }

        public static bool IsValidNumber(string value)
        {
            if (String.IsNullOrEmpty((value ?? string.Empty).Trim()))
                return false;
            else if (!IsRegexMatch(value, NumberRegex))
                return false;
            return true;
        }

        public static bool IsValidUnsignedNumber(string value)
        {
            if (String.IsNullOrEmpty((value ?? string.Empty).Trim()))
                return false;
            else if (!IsRegexMatch(value, UnsignedNumberRegex))
                return false;
            return true;
        }

        public static bool IsValidFloat(string value)
        {
            if (String.IsNullOrEmpty((value ?? string.Empty).Trim()))
                return false;
            else if (!IsRegexMatch(value, FloatRegex))
                return false;
            return true;
        }

        public static bool IsValidURL(string value)
        {
            if (IsRegexMatch(value, URLRegex))
                return true;
            return false;
        }

        public static bool IsValidEmail(string value)
        {
            if (IsRegexMatch(value, EmailRegex))
                return true;
            return false;
        }

        public static bool IsValidLanguageID(LanguageID value)
        {
            if (value == null)
                return false;
            return true;
        }

        public static bool IsValidSelectString(string str, List<string> members)
        {
            foreach (string member in members)
            {
                if (str == member)
                    return true;
            }

            return false;
        }

        private static List<string> _LessonTextTargetStrings = null;
        public static List<string> LessonTextTargetStrings
        {
            get
            {
                if (_LessonTextTargetStrings == null)
                    _LessonTextTargetStrings = new List<string>(3) { "Transcript", "Text", "Notes" };

                return _LessonTextTargetStrings;
            }
        }

        public static bool IsValidLessonTextTarget(string str)
        {
            return IsValidSelectString(str, LessonTextTargetStrings);
        }

        private static List<string> _LessonTextFormatStrings = null;
        public static List<string> LessonTextFormatStrings
        {
            get
            {
                if (_LessonTextFormatStrings == null)
                    _LessonTextFormatStrings = new List<string>(2) { "Raw" /*, "Xml"*/ };

                return _LessonTextFormatStrings;
            }
        }

        public static bool IsValidLessonTextFormat(string str)
        {
            return IsValidSelectString(str, LessonTextFormatStrings);
        }
    }
}

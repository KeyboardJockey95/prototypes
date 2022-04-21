using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Crawlers
{
    public static class SiteTextTools
    {
        public static string Encode(string str)
        {
            if (str != null)
            {
                str = str.Replace("/", "%2F");
                str = str.Replace(":", "%3A");
                str = str.Replace("?", "%3F");
                str = str.Replace("=", "%3D");
                str = str.Replace("|", "%7C");
                str = str.Replace("\"", "%22");
                str = str.Replace("<", "%3C");
                str = str.Replace(">", "%3E");
                //str = str.Replace("%", "%25");
                str = str.Replace("'", "%27");
                str = str.Replace(" ", "%20");
                str = str.Replace("\t", "%09");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static string Decode(string str)
        {
            if (str != null)
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("%2F", "/");
                    str = str.Replace("%3A", ":");
                    str = str.Replace("%3F", "?");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%7C", "|");
                    str = str.Replace("%22", "\"");
                    str = str.Replace("%3C", "<");
                    str = str.Replace("%3E", ">");
                    str = str.Replace("%25", "%");
                    str = str.Replace("%26", "&");
                    str = str.Replace("%2F", "/");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%3F", "?");
                    str = str.Replace("%27", "'");
                    str = str.Replace("%20", " ");
                    str = str.Replace("%09", "\t");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string Indent(int indentCount, int tabSize)
        {
            int spaceCount = indentCount * tabSize;
            if (spaceCount > 80)
                spaceCount = 80;
            string indent = "                                                                               ".Substring(0, spaceCount);
            return indent;
        }

        public static void WriteIndentedText(int indentCount, int tabSize, string text, TextWriter textWriter)
        {
            StringReader stringReader = new StringReader(text);
            string indent = Indent(indentCount, tabSize);

            for (;;)
            {
                string line = stringReader.ReadLine();

                if (line == null)
                    break;

                textWriter.WriteLine(indent + line);
            }
        }

        public static string GetFileNameFromPath(string filePath)
        {
            return MediaUtilities.GetFileName(filePath);
        }

        public static string GetFileNameFromPath(string filePath, int maxLength)
        {
            string returnValue = MediaUtilities.GetFileName(filePath);

            if (returnValue.Length > maxLength)
                returnValue = returnValue.Substring(returnValue.Length - maxLength);

            return returnValue;
        }

        public static string DirectorySeparator = "\\";

        public static string ConcatenateFilePaths(string front, string back)
        {
            string filePath;

            if (!String.IsNullOrEmpty(front))
            {
                if (!front.EndsWith(DirectorySeparator) && !back.StartsWith(DirectorySeparator))
                    filePath = front + DirectorySeparator + back;
                else if (front.EndsWith(DirectorySeparator) && back.StartsWith(DirectorySeparator))
                    filePath = front + back.Substring(1);
                else
                    filePath = front + back;
            }
            else
                filePath = back;

            return filePath;
        }

        public static string ConcatenateFilePaths(string front, string middle, string back)
        {
            string filePath = ConcatenateFilePaths(front, ConcatenateFilePaths(middle, back));
            return filePath;
        }

        public static string UrlSeparator = "/";

        public static string ConcatenateUrls(string front, string back)
        {
            string filePath;

            if (back.StartsWith("http"))
                return back;

            if (back.StartsWith("~"))
                back = back.Substring(1);

            if (!String.IsNullOrEmpty(front))
            {
                if (!front.EndsWith(UrlSeparator) && !back.StartsWith(UrlSeparator))
                    filePath = front + UrlSeparator + back;
                else if (front.EndsWith(UrlSeparator) && back.StartsWith(UrlSeparator))
                    filePath = front + back.Substring(1);
                else
                    filePath = front + back;
            }
            else
                filePath = back;

            return filePath;
        }

        public static string ConvertFilePathToUrl(string urlPrefix, string filePath)
        {
            string partialUrl = filePath.Replace(DirectorySeparator, UrlSeparator);
            return ConcatenateUrls(urlPrefix, partialUrl);
        }

        public static string MakeValidFileBase(string filePath)
        {
            foreach (char chr in LanguageLookup.PunctuationCharacters)
                filePath = filePath.Replace(chr.ToString(), "");

            filePath = filePath.Replace(" ", "");
            filePath = filePath.Replace("\r", "");
            filePath = filePath.Replace("\n", "");
            filePath = filePath.Replace("\t", "");
            filePath = filePath.Replace("#", "");
            filePath = filePath.Replace("/", "_");
            filePath = filePath.Replace("-", "");
            filePath = filePath.Replace("–", "");
            filePath = filePath.Replace("\\", "");
            filePath = filePath.Replace("|", "");
            filePath = filePath.Replace("<", "");
            filePath = filePath.Replace(">", "");
            filePath = filePath.Replace("*", "");
            filePath = filePath.Replace("...", "");
            filePath = filePath.Replace("&#039;", "");

            return filePath;
        }

        public static string MakeValidFileBase(string filePath, int maxLength)
        {
            string returnValue = MakeValidFileBase(filePath);

            if (returnValue.Length > maxLength)
                returnValue = returnValue.Substring(0, maxLength);

            return returnValue;
        }

        public static string GetPunctuationString(string text)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char chr in text)
            {
                if (LanguageLookup.SentenceTerminatorCharacters.Contains(chr))
                    sb.Append(chr);
            }

            return sb.ToString();
        }

        public static bool Match(string pattern, string text)
        {
            pattern = GetMatchableString(pattern);
            text = GetMatchableString(text);

            if (pattern == "*")
                return true;
            else if (pattern == text)
                return true;
            else if (Regex.IsMatch(text, "^" + pattern + "$"))
                return true;

            return false;
        }

        public static string GetMatchableString(string str)
        {
            string matchableString = TextUtilities.GetCanonicalText(str, LanguageLookup.English);

            matchableString = matchableString.Replace("—", "-");

            return matchableString;
        }

        public static bool DirectoryExistsCheck(string filePath)
        {
            return FileSingleton.DirectoryExistsCheck(filePath);
        }

        public static string ReplaceFatWithNonFatPunctuation(string str)
        {
            int length = LanguageLookup.FatPunctuationConversion.Length;

            for (int index = 0; index < length; index += 2)
            {
                str = str.Replace(LanguageLookup.FatPunctuationConversion[index], LanguageLookup.FatPunctuationConversion[index + 1]);
            }

            return str;
        }

        public static string ReplaceCharacterEntities(string str)
        {
            return TextUtilities.EntityDecode(str);
        }

        public static List<string> GetStringList(string filePath)
        {
            List<string> stringList = null;

            try
            {
                using (StreamReader streamReader = FileSingleton.OpenText(filePath))
                {
                    stringList = new List<string>();

                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        stringList.Add(line);
                    }

                    FileSingleton.Close(streamReader);
                }
            }
            catch
            {
                stringList = null;
            }

            return stringList;
        }

        public static int GetIntegerFromString(string str, int defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                int value = Convert.ToInt32(str);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}

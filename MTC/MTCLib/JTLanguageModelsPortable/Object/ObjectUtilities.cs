using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Object
{
    public static class ObjectUtilities
    {
        public static string ToLower(string input, CultureInfo cultureInfo)
        {
            if (ApplicationData.ToLowerFunction != null)
                return ApplicationData.ToLowerFunction(input, cultureInfo);

            if (input == null)
                return input;

            return input.ToLower();
        }

        public static List<string> GetStringListFromRawLinesIndent(string rawLines, int indent)
        {
            List<string> list = GetStringListFromRawLines(rawLines);
            string indentString = IndentSpaces(indent);
            int count = list.Count();
            int index;
            for (index = 0; index < count; index++)
                list[index] = indentString + list[index];
            return list;
        }

        public static List<string> GetStringListFromRawLines(string rawLines)
        {
            string[] strings = rawLines.Split(LanguageLookup.NewLine, StringSplitOptions.None);
            List<string> list = new List<string>(strings);
            return list;
        }

        public static string GetIndentedElementString(XElement element, int indent)
        {
            string str = element.ToString(SaveOptions.DisableFormatting);
            string returnValue = IndentStringLines(str, indent);
            return returnValue;
        }

        public static string IndentStringLines(string str, int indent)
        {
            if (indent <= 0)
                return str;
            string[] strings = str.Split(LanguageLookup.NewLine, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();
            string indentString = IndentSpaces(indent);
            foreach (string s in strings)
                sb.AppendLine(indentString + s);
            return sb.ToString();
        }

        public static string IndentSpaces(int indent)
        {
            string indentString = String.Empty;

            while (indent-- > 0)
                indentString += "    ";

            return indentString;
        }

        public static void Display(IBaseObject obj, string label, DisplayDetail detail, int indent)
        {
            DisplayLabel(obj, label, indent);

            switch (detail)
            {
                case DisplayDetail.Lite:
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Full:
                case DisplayDetail.Xml:
                    {
                        XElement element = obj.Xml;
                        string str = ObjectUtilities.GetIndentedElementString(element, indent + 1);
                        DisplayMessage(str, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        public static void DisplayLabel(IBaseObject obj, string label, int indent)
        {
            string msg;

            if (!String.IsNullOrEmpty(label))
            {
                if (label.Contains(":"))
                    msg = label;
                else
                    msg = label + ":";

                if (obj == null)
                    msg += " (null)";
            }
            else if (obj != null)
                msg = obj.GetType().Name + ":";
            else
                msg = "(null object)";

            DisplayMessage(msg, indent);
        }

        public static void DisplayLabelArgument(IBaseObject obj, string label, string argument, int indent)
        {
            string msg;

            if (!String.IsNullOrEmpty(label))
            {
                if (label.Contains(":"))
                    msg = label;
                else
                    msg = label + ":";

                if (obj == null)
                    msg += " (null)";
            }
            else if (obj != null)
                msg = obj.GetType().Name + ":";
            else
                msg = "(null object)";

            if (!String.IsNullOrEmpty(argument))
                msg += " " + argument;

            DisplayMessage(msg, indent);
        }

        public static void DisplayLabelArgument(IBaseObject obj, string label, int indent, string argument)
        {
            string msg;

            if (!String.IsNullOrEmpty(label))
            {
                if (label.Contains(":"))
                    msg = label;
                else
                    msg = label + ":";

                if (obj == null)
                    msg += " (null)";
            }
            else if (obj != null)
                msg = obj.GetType().Name + ":";
            else
                msg = "(null object)";

            if (!String.IsNullOrEmpty(argument))
                msg += " " + argument;

            DisplayMessage(msg, indent);
        }

        public static void DisplayField(string name, string value, int indent)
        {
            if (name == null)
                name = "(null)";
            if (value == null)
                value = "(null)";
            DisplayMessage(name + " = " + value, indent);
        }

        public static void DisplayFieldObject(string name, IBaseObject obj, int indent)
        {
            if (obj == null)
                DisplayLabel(obj, name, indent);
            else
                obj.Display(name, DisplayDetail.Full, indent);
        }

        public static void DisplayMessage(string msg, int indent)
        {
            string str = IndentStringLines(msg, indent);
            ApplicationData.Global.PutConsoleMessage(str);
        }

        public static List<string> GetFormatTokens(string formatPattern)
        {
            return GetDelimitedSubstrings(formatPattern, '{', '}', true);
        }

        public static List<string> GetDelimitedSubstrings(
            string str,
            char openChr,
            char closeChr,
            bool includeDelimiters)
        {
            List<string> list = new List<string>();

            if (String.IsNullOrEmpty(str))
                return list;

            int length = str.Length;
            int startIndex = 0;
            int openIndex = 0;
            int closeIndex = 0;
            string substring;

            while (startIndex < length)
            {
                openIndex = str.IndexOf(openChr, startIndex);

                if (openIndex < 0)
                    break;

                closeIndex = str.IndexOf(closeChr, openIndex);

                if (closeIndex < 0)
                    break;

                if (includeDelimiters)
                    substring = str.Substring(openIndex, (closeIndex + 1) - openIndex);
                else
                    substring = str.Substring(openIndex + 1, closeIndex - (openIndex + 1));

                list.Add(substring);

                startIndex = closeIndex + 1;
            }

            return list;
        }

        public static List<string> GetStringListFromElement(XElement element)
        {
            string value = element.Value.Trim();
            char[] seps = { ',', ' ' };
            string[] strings = value.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>(strings);
            return list;
        }

        public static XElement GetElementFromStringList(string name, List<string> obj)
        {
            if (obj == null)
                return null;
            XElement element = new XElement(name);
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (string s in obj)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.Append(s);
            }
            element.SetValue(sb.ToString());
            return element;
        }

        public static XElement GetElementFromStringListFiltered(string name, List<string> obj, Dictionary<string, bool> keyFlags)
        {
            if (obj == null)
                return null;
            XElement element = new XElement(name);
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (string s in obj)
            {
                bool useIt = true;

                if ((keyFlags != null) && keyFlags.TryGetValue(s, out useIt) && !useIt)
                    continue;

                if (first)
                    first = false;
                else
                    sb.Append(",");

                sb.Append(s);
            }
            element.SetValue(sb.ToString());
            return element;
        }

        public static List<int> GetIntListFromElement(XElement element)
        {
            string value = element.Value.Trim();
            char[] seps = { ',', ' ' };
            string[] strings = value.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            List<int> list = new List<int>();
            try
            {
                foreach (string s in strings)
                {
                    int i = Convert.ToInt32(s);
                    list.Add(i);
                }
            }
            catch
            {
            }
            return list;
        }

        public static XElement GetElementFromIntList(string name, List<int> obj)
        {
            if (obj == null)
                return null;
            XElement element = new XElement(name);
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (int i in obj)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.Append(i.ToString());
            }
            element.SetValue(sb.ToString());
            return element;
        }

        public static XElement GetElementFromIntListFiltered(string name, List<int> obj, Dictionary<int, bool> keyFlags)
        {
            if (obj == null)
                return null;
            XElement element = new XElement(name);
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (int i in obj)
            {
                bool useIt = true;

                if ((keyFlags != null) && keyFlags.TryGetValue(i, out useIt) && !useIt)
                    continue;

                if (first)
                    first = false;
                else
                    sb.Append(",");

                sb.Append(i.ToString());
            }
            element.SetValue(sb.ToString());
            return element;
        }

        public static Dictionary<KeyType, ValueType> GetDictionaryFromElement<KeyType, ValueType>(
            XElement element, string keyTypeName, string valueTypeName)
        {
            Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>();
            List<string> list = GetStringListFromElement(element);
            int count = list.Count();
            int index;
            int emptyCount = 0;
            for (index = 0; index < count; index += 2)
            {
                string keyString = list[index];
                if (String.IsNullOrEmpty(keyString))
                {
                    keyString = "Empty" + emptyCount.ToString();
                    emptyCount++;
                }
                string valueString = list[index + 1];
                KeyType key = (KeyType)GetKeyFromString(keyString, keyTypeName);
                ValueType value = (ValueType)GetKeyFromString(valueString, valueTypeName);
                try
                {
                    dictionary.Add(key, value);
                }
                catch (Exception exc)
                {
                    ApplicationData.Global.PutConsoleErrorMessage(
                        "Exception during dictionary add: " +
                        keyString +
                        "/" +
                        valueString +
                        exc.Message);
                }
            }
            return dictionary;
        }

        public static XElement GetElementFromDictionary<KeyType, ValueType>(
            string name, Dictionary<KeyType, ValueType> dictionary)
        {
            if (dictionary == null)
                return null;
            XElement element = new XElement(name);
            StringBuilder sb = new StringBuilder(128);
            bool first = true;
            foreach (KeyValuePair<KeyType, ValueType> kvp in dictionary)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.Append(kvp.Key.ToString());
                sb.Append(",");
                sb.Append(kvp.Value.ToString());
            }
            element.SetValue(sb.ToString());
            return element;
        }

        public static Dictionary<KeyType, ValueType> GetDictionaryFromElementGroupedSimple<KeyType, ValueType>(
            XElement element, string keyTypeName, string valueTypeName)
        {
            Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>();
            int emptyCount = 0;
            foreach (XElement childElement in element.Elements())
            {
                List<XElement> grandChildElements = childElement.Elements().ToList();
                XElement keyElement = grandChildElements[0];
                XElement valueElement = grandChildElements[1];
                string keyString = keyElement.Value.Trim();
                if (String.IsNullOrEmpty(keyString))
                {
                    keyString = "Empty" + emptyCount.ToString();
                    emptyCount++;
                }
                string valueString = valueElement.Value.Trim();
                KeyType key = (KeyType)GetKeyFromString(keyString, keyTypeName);
                ValueType value = (ValueType)GetKeyFromString(valueString, valueTypeName);
                try
                {
                    dictionary.Add(key, value);
                }
                catch (Exception exc)
                {
                    ApplicationData.Global.PutConsoleErrorMessage(
                        "Exception during dictionary add: " +
                        keyString +
                        "/" +
                        valueString +
                        exc.Message);
                }
            }
            return dictionary;
        }

        public static XElement GetElementFromDictionaryGroupedSimple<KeyType, ValueType>(
            string name, Dictionary<KeyType, ValueType> dictionary)
        {
            if (dictionary == null)
                return null;
            XElement element = new XElement(name);
            foreach (KeyValuePair<KeyType, ValueType> kvp in dictionary)
            {
                XElement entry = new XElement("Entry");
                entry.Add(new XElement("Key", kvp.Key.ToString()));
                entry.Add(new XElement("Value", kvp.Value.ToString()));
                element.Add(entry);
            }
            return element;
        }

        public static Dictionary<KeyType, ValueType> GetDictionaryFromElementGroupedComplex<KeyType, ValueType>(
            XElement element, string keyTypeName, string valueTypeName) where ValueType : IBaseObjectKeyed, new()
        {
            Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>();
            int emptyCount = 0;
            foreach (XElement childElement in element.Elements())
            {
                List<XElement> grandChildElements = childElement.Elements().ToList();
                XElement keyElement = grandChildElements[0];
                XElement valueElement = grandChildElements[1];
                string keyString = keyElement.Value;
                if (String.IsNullOrEmpty(keyString))
                {
                    keyString = "Empty" + emptyCount.ToString();
                    emptyCount++;
                }
                ValueType value = new ValueType();
                value.OnElement(valueElement);
                KeyType key = (KeyType)GetKeyFromString(keyString, keyTypeName);
                try
                {
                    dictionary.Add(key, value);
                }
                catch (Exception exc)
                {
                    ApplicationData.Global.PutConsoleErrorMessage(
                        "Exception during dictionary add: " +
                        keyString +
                        "/" +
                        value.ToString() +
                        exc.Message);
                }
            }
            return dictionary;
        }

        public static XElement GetElementFromDictionaryGroupedComplex<KeyType, ValueType>(
            string name, Dictionary<KeyType, ValueType> dictionary) where ValueType : IBaseObjectKeyed
        {
            if (dictionary == null)
                return null;
            XElement element = new XElement(name);
            foreach (KeyValuePair<KeyType, ValueType> kvp in dictionary)
            {
                XElement entry = new XElement("Entry");
                entry.Add(new XElement("Key", kvp.Key.ToString()));
                entry.Add(kvp.Value.GetElement("Value"));
                element.Add(entry);
            }
            return element;
        }

        public static Dictionary<KeyType, ValueType> ReadDictionaryFromFile<KeyType, ValueType>(
            string filePath, string keyTypeName, string valueTypeName)
        {
            Dictionary<KeyType, ValueType> dictionary = null;
            try
            {
                string[] list = FileSingleton.ReadAllLines(filePath);
                dictionary = new Dictionary<KeyType, ValueType>();
                int count = list.Count();
                int index;
                int emptyCount = 0;
                for (index = 0; index < count; index += 2)
                {
                    string keyString = list[index];
                    if (String.IsNullOrEmpty(keyString))
                    {
                        keyString = "Empty" + emptyCount.ToString();
                        emptyCount++;
                    }
                    string valueString = list[index + 1];
                    KeyType key = (KeyType)GetKeyFromString(keyString, keyTypeName);
                    ValueType value = (ValueType)GetKeyFromString(valueString, valueTypeName);
                    try
                    {
                        dictionary.Add(key, value);
                    }
                    catch (Exception exc)
                    {
                        ApplicationData.Global.PutConsoleErrorMessage(
                            "Exception during dictionary add: " +
                            keyString +
                            "/" +
                            valueString +
                            exc.Message);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return dictionary;
        }

        public static bool WriteDictionaryToFile<KeyType, ValueType>(
            string filePath, Dictionary<KeyType, ValueType> dictionary)
        {
            if (dictionary == null)
                return false;
            List<string> list = new List<string>(dictionary.Count() * 2);
            foreach (KeyValuePair<KeyType, ValueType> kvp in dictionary)
            {
                list.Add(kvp.Key.ToString());
                list.Add(kvp.Value.ToString());
            }
            try
            {
                FileSingleton.DirectoryExistsCheck(filePath);
                FileSingleton.WriteAllLines(filePath, list.ToArray());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static char GetHighNibbleChar(int value)
        {
            return GetLowNibbleChar(value >> 4);
        }

        public static char GetLowNibbleChar(int value)
        {
            value &= 0xf;
            if (value > 9)
                value += 'a' - 10;
            else
                value += '0';
            return (char)value;
        }

        public static string GetDataStringFromByteArray(byte[] bytes, bool addNewLines)
        {
            if (bytes == null)
                return String.Empty;
            return GetDataStringFromByteArray(bytes, bytes.Count(), addNewLines);
        }

        public static string GetDataStringFromByteArray(byte[] bytes, int count, bool addNewLines)
        {
            if (bytes == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            for (index = 0; index < count; index++)
            {
                if (addNewLines && ((index % 32) == 0))
                    sb.Append("\r\n");
                sb.Append(GetHighNibbleChar(bytes[index]));
                sb.Append(GetLowNibbleChar(bytes[index]));
            }
            if (addNewLines)
                sb.Append("\r\n");
            return sb.ToString();
        }

        public static int GetNibble(char c)
        {
            if ((c >= '0') && (c <= '9'))
                return c - '0';
            if ((c >= 'A') && (c <= 'F'))
                return 10 + (c - 'A');
            if ((c >= 'a') && (c <= 'f'))
                return 10 + (c - 'a');
            return -1;
        }

        public static byte[] GetByteArrayFromDataString(string data)
        {
            if (data == null)
                return null;
            int dataLength = data.Length;
            if (dataLength == 0)
                return new byte[0];
            List<byte> list = new List<byte>(dataLength / 2);
            int index;
            int value;
            byte b;
            for (index = 0; index < dataLength; index++)
            {
                char chr = data[index];

                switch (chr)
                {
                    case '\r':
                    case '\n':
                    case ' ':
                        break;
                    case '%':
                        index += 2;
                        break;
                    default:
                        value = GetNibble(data[index]);
                        if (value != -1)
                        {
                            b = (byte)(value << 4);
                            index++;
                            if (index == dataLength)
                                break;
                            value = GetNibble(data[index]);
                            b += (byte)value;
                            list.Add(b);
                        }
                        break;
                }
            }
            return list.ToArray();
        }

        public static string GetDataStringFromStream(Stream stream, bool addNewLines)
        {
            if (stream == null)
                return String.Empty;

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            int length = (int)stream.Length;
            byte[] buffer = new byte[length];

            stream.Read(buffer, 0, length);

            return GetDataStringFromByteArray(buffer, addNewLines);
        }

        public static Stream GetStreamFromDataString(string data)
        {
            byte[] buffer = GetByteArrayFromDataString(data);
            MemoryStream stream = new MemoryStream(buffer);
            return stream;
        }

        public static void GetRecordBytesFromString(byte[] data, int inIndex, out int outIndex, string str)
        {
            if (str == null)
            {
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
            }
            else
            {
                byte[] sdata = ApplicationData.Encoding.GetBytes(str);
                int length = sdata.Count();
                data[inIndex++] = (byte)(length >> 24);
                data[inIndex++] = (byte)(length >> 16);
                data[inIndex++] = (byte)(length >> 8);
                data[inIndex++] = (byte)length;
                for (int index = 0; index < length; index++)
                    data[inIndex++] = (byte)sdata[index];
            }
            outIndex = inIndex;
        }

        public static string GetStringFromRecordBytes(byte[] data, int inIndex, out int outIndex)
        {
            outIndex = inIndex;
            if ((data == null) || (inIndex >= data.Count()))
                return "";
            else
            {
                int length = ((int)data[inIndex++] << 24);
                length += ((int)data[inIndex++] << 16);
                length += ((int)data[inIndex++] << 8);
                length += ((int)data[inIndex++]);
                string str = ApplicationData.Encoding.GetString(data, inIndex, length);
                outIndex = inIndex + length;
                return str;
            }
        }

        public static void GetRecordBytesFromByteArray(byte[] data, int inIndex, out int outIndex, byte[] bytes)
        {
            if (bytes == null)
            {
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
            }
            else
            {
                int length = bytes.Length;
                data[inIndex++] = (byte)(length >> 24);
                data[inIndex++] = (byte)(length >> 16);
                data[inIndex++] = (byte)(length >> 8);
                data[inIndex++] = (byte)length;
                for (int index = 0; index < length; index++)
                    data[inIndex++] = bytes[index];
            }
            outIndex = inIndex;
        }

        public static byte[] GetByteArrayFromRecordBytes(byte[] data, int inIndex, out int outIndex)
        {
            outIndex = inIndex;
            if ((data == null) || (inIndex >= data.Count()))
                return new byte[0];
            else
            {
                int length = ((int)data[inIndex++] << 24);
                length += ((int)data[inIndex++] << 16);
                length += ((int)data[inIndex++] << 8);
                length += ((int)data[inIndex++]);
                byte[] bytes = new byte[length];
                for (int index = 0; index < length; index++)
                    bytes[index] = data[inIndex++];
                outIndex = inIndex;
                return bytes;
            }
        }

        public static void GetRecordBytesFromStream(byte[] data, int inIndex, out int outIndex, Stream stream)
        {
            if (stream == null)
            {
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
                data[inIndex++] = 0;
            }
            else
            {
                int length = (int)stream.Length;
                data[inIndex++] = (byte)(length >> 24);
                data[inIndex++] = (byte)(length >> 16);
                data[inIndex++] = (byte)(length >> 8);
                data[inIndex++] = (byte)length;
                stream.Read(data, inIndex, length);
                inIndex += length;
            }
            outIndex = inIndex;
        }

        public static Stream GetStreamFromRecordBytes(byte[] data, int inIndex, out int outIndex)
        {
            outIndex = inIndex;
            if ((data == null) || (inIndex >= data.Count()))
                return new MemoryStream();
            else
            {
                int length = ((int)data[inIndex++] << 24);
                length += ((int)data[inIndex++] << 16);
                length += ((int)data[inIndex++] << 8);
                length += ((int)data[inIndex++]);
                MemoryStream stream = new MemoryStream(data, inIndex, length);
                outIndex = inIndex + length;
                return stream;
            }
        }

        public static byte[] GetSubBytes(byte[] data, int inIndex, out int outIndex, int count)
        {
            if (data == null)
            {
                outIndex = inIndex;
                return null;
            }
            if (count == -1)
                count = data.Count() - inIndex;
            byte[] subData = new byte[count];
            for (int index = 0; index < count; index++)
                subData[index] = data[inIndex++];
            outIndex = inIndex;
            return subData;
        }

        public static byte[] GetByteArrayFromString(string str)
        {
            return ApplicationData.Encoding.GetBytes(str);
        }

        public static string GetStringFromByteArray(byte[] bytes)
        {
            return ApplicationData.Encoding.GetString(bytes, 0, bytes.Count());
        }

        public static void PrepareMultiLanguageString(MultiLanguageString multiLanguageString, string defaultValue,
            List<LanguageDescriptor> languageDescriptors)
        {
            string stringKey = multiLanguageString.KeyString;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                {
                    if (multiLanguageString.LanguageString(languageDescriptor.LanguageID) == null)
                    {
                        if (languageDescriptor.Name == "Host")
                            multiLanguageString.Add(
                                new LanguageString(stringKey, languageDescriptor.LanguageID, defaultValue));
                        else
                            multiLanguageString.Add(
                                new LanguageString(stringKey, languageDescriptor.LanguageID, String.Empty));
                    }
                }
            }
        }

        public static void PrepareMultiLanguageString(MultiLanguageString multiLanguageString, string defaultValue,
            List<LanguageID> languageIDs)
        {
            string stringKey = multiLanguageString.KeyString;

            foreach (LanguageID languageID in languageIDs)
            {
                if (String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
                    continue;

                if (multiLanguageString.LanguageString(languageID) == null)
                    multiLanguageString.Add(
                        new LanguageString(stringKey, languageID, defaultValue));
            }
        }

        public static void SynchronizeMultiLanguageStringLanguages(MultiLanguageString multiLanguageString, string defaultValue,
            List<LanguageID> languageIDs)
        {
            if (multiLanguageString == null)
                return;

            if ((languageIDs == null) || (languageIDs.Count() == 0))
            {
                multiLanguageString.LanguageStrings = new List<LanguageString>();
                return;
            }

            List<LanguageString> languageStrings = new List<LanguageString>(languageIDs.Count());
            string stringKey = multiLanguageString.KeyString;

            foreach (LanguageID languageID in languageIDs)
            {
                if (String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
                {
                    languageStrings.AddRange(multiLanguageString.CloneLanguageStrings());
                    break;
                }

                LanguageString languageString = multiLanguageString.LanguageString(languageID);

                if (languageString == null)
                    languageString = new LanguageString(stringKey, languageID, defaultValue);

                languageStrings.Add(languageString);
            }

            multiLanguageString.LanguageStrings = languageStrings;
        }

        public static MultiLanguageString CreateMultiLanguageString(object key, string defaultValue,
            List<LanguageDescriptor> languageDescriptors)
        {
            MultiLanguageString mutliLanguageString = new MultiLanguageString(key);

            PrepareMultiLanguageString(mutliLanguageString, defaultValue, languageDescriptors);

            return mutliLanguageString;
        }

        public static MultiLanguageString CreateMultiLanguageString(object key, string defaultValue,
            List<LanguageID> languageIDs)
        {
            MultiLanguageString mutliLanguageString = new MultiLanguageString(key);

            PrepareMultiLanguageString(mutliLanguageString, defaultValue, languageIDs);

            return mutliLanguageString;
        }

        public static MultiLanguageString CreateMultiLanguageString(object key, List<string> languageCodes,
            List<string> strings)
        {
            MultiLanguageString mutliLanguageString = new MultiLanguageString(key);

            if ((languageCodes != null) && (strings != null))
            {
                int index;
                int lc = languageCodes.Count();
                int sc = strings.Count();

                if (lc < sc)
                    sc = lc;

                for (index = 0; index < lc; index++)
                {
                    LanguageString ls = new LanguageString(key, LanguageLookup.GetLanguageIDNoAdd(languageCodes[index]), strings[index]);
                    mutliLanguageString.Add(ls);
                }
            }

            return mutliLanguageString;
        }

        public static MultiLanguageString CreateMultiLanguageString(object key, string defaultValue, UserProfile userProfile)
        {
            MultiLanguageString multiLanguageString = new MultiLanguageString(key);

            PrepareMultiLanguageString(multiLanguageString, String.Empty, userProfile.LanguageDescriptors);

            foreach (LanguageString languageString in multiLanguageString.LanguageStrings)
            {
                if (languageString.LanguageID == userProfile.UILanguageID)
                    languageString.Text = defaultValue;
            }

            return multiLanguageString;
        }

        public static MultiLanguageString CloneMultiLanguageString(MultiLanguageString other)
        {
            if (other == null)
                return null;

            MultiLanguageString mutliLanguageString = other.CloneMultiLanguageString();

            return mutliLanguageString;
        }

        public static List<string> CloneStringList(List<string> list)
        {
            if (list == null)
                return null;

            return new List<string>(list);
        }

        public static void AddStringListElementCheck(
            XElement element,
            string name,
            List<string> stringList)
        {
            if ((stringList != null) && (stringList.Count() != 0))
                element.Add(new XElement(name, TextUtilities.GetStringFromStringList(stringList)));
        }

        public static bool IsEmptyValue(object value)
        {
            if (value == null)
                return true;

            Type valueType = value.GetType();

            switch (valueType.Name)
            {
                case "String":
                    return (value as string) == String.Empty;
                case "Char":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Byte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                case "Single":
                case "Double":
                case "Boolean":
                case "DateTime":
                case "TimeSpan":
                    return false;
                case "Guid":
                    return (Guid)value == Guid.Empty;
                default:
                    return value.ToString() == String.Empty;
            }
        }

        public static int ValueLength(object value)
        {
            if (value == null)
                return 0;

            Type valueType = value.GetType();

            switch (valueType.Name)
            {
                case "String":
                    return (value as string).Length;
                case "Char":
                    return sizeof(char);
                case "Int16":
                    return sizeof(short);
                case "Int32":
                    return sizeof(int);
                case "Int64":
                    return sizeof(long);
                case "Byte":
                    return sizeof(byte);
                case "UInt16":
                    return sizeof(ushort);
                case "UInt32":
                    return sizeof(uint);
                case "UInt64":
                    return sizeof(ulong);
                case "Single":
                    return sizeof(float);
                case "Double":
                    return sizeof(double);
                case "Boolean":
                    return sizeof(bool);
                case "Guid":
                    return value.ToString().Length;
                case "DateTime":
                    return value.ToString().Length;
                case "TimeSpan":
                    return value.ToString().Length;
                default:
                    return -1;
            }
        }

        public static List<IBaseObjectKeyed> CopyBaseList(List<IBaseObjectKeyed> baseList)
        {
            List<IBaseObjectKeyed> list = null;

            if (baseList != null)
            {
                list = new List<IBaseObjectKeyed>(baseList.Count());

                foreach (IBaseObjectKeyed item in baseList)
                {
                    IBaseObjectKeyed newItem = (IBaseObjectKeyed)item.Clone();
                    list.Add(newItem);
                }
            }

            return list;
        }

        public static List<object> CopyObjectList(List<object> objectList)
        {
            List<object> list = null;

            if (objectList != null)
            {
                list = new List<object>(objectList.Count());

                foreach (object item in objectList)
                {
                    if (item is IBaseObjectKeyed)
                    {
                        IBaseObjectKeyed newItem = (IBaseObjectKeyed)((IBaseObjectKeyed)item).Clone();
                        list.Add(newItem);
                    }
                    else
                        list.Add(item);
                }
            }

            return list;
        }

        public static List<T> CopyTypedBaseObjectList<T>(List<T> baseList) where T : class, IBaseObjectKeyed
        {
            List<T> list = null;

            if (baseList != null)
            {
                list = new List<T>(baseList.Count());

                foreach (T item in baseList)
                {
                    T newItem = (T)item.Clone();
                    list.Add(newItem);
                }
            }

            return list;
        }

        public static object CreateObject(Type type)
        {
            object obj = Activator.CreateInstance(type);
            return obj;
        }

        public static IBaseObject CreateBaseObjectObject(Type type)
        {
            object obj = CreateObject(type);
            IBaseObject item = null;

            if (obj != null)
            {
                item = obj as IBaseObject;

                if (item == null)
                    throw new ObjectException("ObjectUtilities.CreateBaseObjectObject:  Class " + type.Name + " is not derived from IBaseObject.");
            }

            return item;
        }

        public static object Resurrect(XElement element)
        {
            if (element == null)
                return null;

            string className = element.Name.LocalName;
            Type type = ObjectTypes.FindType(className);

            if (type != null)
            {
                IBaseObject item = CreateObject(type) as IBaseObject;

                if (item != null)
                {
                    item.Xml = element;

                    if (item is IBaseObjectKeyed)
                        (item as IBaseObjectKeyed).Modified = false;
                }

                return item;
            }

            string elementValue = element.Value;

            if (!String.IsNullOrEmpty(elementValue))
            {
                switch (className)
                {
                    case "String":
                        return elementValue;
                    case "Char":
                        return Convert.ToChar(elementValue);
                    case "Int16":
                        return Convert.ToInt16(elementValue);
                    case "Int32":
                        return Convert.ToInt32(elementValue);
                    case "Int64":
                        return Convert.ToInt64(elementValue);
                    case "Byte":
                        return Convert.ToByte(elementValue);
                    case "UInt16":
                        return Convert.ToUInt16(elementValue);
                    case "UInt32":
                        return Convert.ToUInt32(elementValue);
                    case "UInt64":
                        return Convert.ToUInt64(elementValue);
                    case "Single":
                        return Convert.ToSingle(elementValue);
                    case "Double":
                        return Convert.ToDouble(elementValue);
                    case "Boolean":
                        return Convert.ToBoolean(elementValue);
                    case "Guid":
                        return new Guid(elementValue);
                    case "DateTime":
                        return Convert.ToDateTime(elementValue);
                    case "TimeSpan":
                        return TimeSpan.Parse(elementValue);
                    default:
                        break;
                }
            }

            return null;
        }

        public static List<object> ResurrectObjectList(XElement element)
        {
            if (element == null)
                return null;

            List<object> objs = new List<object>();

            foreach (XElement valueElement in element.Elements())
            {
                object obj = Resurrect(valueElement);

                if (obj != null)
                    objs.Add(obj);
            }

            return objs;
        }

        public static IBaseObjectKeyed ResurrectBase(XElement element)
        {
            object obj = Resurrect(element);

            if (obj != null)
            {
                IBaseObjectKeyed item = obj as IBaseObjectKeyed;

                if (item == null)
                    throw new ObjectException("ResurrectBase:  Item " + element.Name.LocalName + " is not derived from IBaseObjectKeyed.");

                return item;
            }

            return null;
        }

        public static IBaseObject ResurrectBaseObject(XElement element)
        {
            if (element == null)
                return null;

            string className = element.Name.ToString();
            Type type = ObjectTypes.FindType(className);

            if (type == null)
                return null;

            IBaseObject item = CreateBaseObjectObject(type);

            if (item != null)
                item.Xml = element;

            return item;
        }

        public static XElement FlattenObjectList(string elementName, List<object> objs)
        {
            XElement valuesElement = new XElement(elementName);

            if ((objs != null) && (objs.Count() != 0))
            {
                foreach (object value in objs)
                {
                    if (value == null)
                        continue;
                    if (value is IBaseObject)
                        valuesElement.Add((value as IBaseObject).GetElement(value.GetType().Name));
                    else
                        valuesElement.Add(new XElement(value.GetType().Name, value.ToString()));
                }
            }

            return valuesElement;
        }

        public static bool IsIntegerType(Type type)
        {
            if (type == null)
                return false;

            switch (type.Name)
            {
                case "String":
                case "Char":
                    return false;
                case "Int16":
                case "Int32":
                case "Int64":
                case "Byte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return true;
                case "Single":
                case "Double":
                case "Boolean":
                case "Guid":
                case "DateTime":
                case "TimeSpan":
                default:
                    return false;
            }
        }

        public static bool IsNumberString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;
            if (str.StartsWith("-"))
                str = str.Substring(1);

            foreach (char c in str)
            {
                if ((c < '0') || (c > '9'))
                    return false;
            }
            return true;
        }

        public static bool IsIntegerString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            foreach (char c in str)
            {
                if ((c < '0') || (c > '9'))
                    return false;
            }

            return true;
        }

        public static bool IsFloatString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;
            if (str.StartsWith("-"))
                str = str.Substring(1);

            foreach (char c in str)
            {
                if (c == '.')
                    continue;
                if ((c < '0') || (c > '9'))
                    return false;
            }
            return true;
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

        public static int GetIntegerFromHexString(string str, int defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                int value = Convert.ToInt32(str, 16);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static long GetLongFromString(string str, long defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                long value = Convert.ToInt64(str);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static int GetIntegerFromStringEnd(string str, int defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            if (!char.IsDigit(str[str.Length - 1]))
                return defaultValue;

            int offset = str.Length - 2;

            while (offset >= 0)
            {
                if (!char.IsDigit(str[offset]))
                    break;

                offset--;
            }

            offset++;
            string numStr = str.Substring(offset);

            try
            {
                int value = Convert.ToInt32(numStr);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string RemoveNumberFromStringEnd(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (!char.IsDigit(str[str.Length - 1]))
                return str;

            int offset = str.Length - 2;

            while (offset >= 0)
            {
                if (!char.IsDigit(str[offset]))
                    break;

                offset--;
            }

            offset++;
            str = str.Substring(0, offset);

            return str;
        }

        public static string RemoveNumberAndUnderscoreFromStringEnd(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (!char.IsDigit(str[str.Length - 1]))
                return str;

            int offset = str.Length - 2;

            while (offset >= 0)
            {
                if (!char.IsDigit(str[offset]))
                    break;

                if (str[offset] == '_')
                    break;

                offset--;
            }

            str = str.Substring(0, offset);

            return str;
        }

        public static bool ParseStringUnderscoreInteger(string str, out string prefixString, out int suffixNumber)
        {
            prefixString = String.Empty;
            suffixNumber = -1;

            if (String.IsNullOrEmpty(str))
                return false;

            int offset = str.LastIndexOf('_');

            if (offset == -1)
                return false;

            prefixString = str.Substring(0, offset);
            string suffixString = str.Substring(offset + 1);
            suffixNumber = GetIntegerFromString(suffixString, -1);

            if (suffixNumber == -1)
                return false;

            return true;
        }

        public static bool ParseNumberUnderscoreString(string str, out int prefixNumber, out string suffixString)
        {
            prefixNumber = -1;
            suffixString = String.Empty;

            if (String.IsNullOrEmpty(str))
                return false;

            int offset = str.LastIndexOf('_');

            if (offset == -1)
                return false;

            string prefixString = str.Substring(0, offset);
            prefixNumber = GetIntegerFromString(prefixString, -1);

            if (prefixNumber == -1)
                return false;

            suffixString = str.Substring(offset + 1);

            return true;
        }

        public static List<int> GetIntegerListFromStringDelimited(string str, int defaultValue, char[] delimiters)
        {
            List<int> returnValue = new List<int>();

            if (String.IsNullOrEmpty(str))
                return returnValue;

            string[] parts = str.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
                returnValue.Add(GetIntegerFromString(part, defaultValue));

            return returnValue;
        }

        public static float GetFloatFromString(string str, float defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                float value = (float)Convert.ToDouble(str);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static double GetDoubleFromString(string str, double defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                double value = Convert.ToDouble(str);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static bool GetBoolFromString(string str, bool defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            switch (str)
            {
                case "true":
                case "True":
                case "TRUE":
                case "1":
                case "yes":
                case "Yes":
                case "YES":
                case "on":
                case "On":
                case "ON":
                    return true;
                default:
                    return false;
            }
        }

        public static TimeSpan GetTimeSpanFromString(string str, TimeSpan defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                TimeSpan value;
                if ((str[0] == '-') || str.Contains("."))
                    value = TimeSpan.Parse(str);
                else
                {
                    string[] parts = str.Split(LanguageLookup.Colon, StringSplitOptions.None);
                    if (parts.Length == 3)
                    {
                        int hr = GetIntegerFromString(parts[0], 0);
                        int mn = GetIntegerFromString(parts[1], 0);
                        int sc = GetIntegerFromString(parts[2], 0);
                        int totalSeconds = (hr * 3600) + (mn * 60) + sc;
                        value = TimeSpan.FromSeconds(totalSeconds);
                    }
                    else
                        value = TimeSpan.Parse(str);
                }
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string GetStringFromTimeSpan(TimeSpan value)
        {
            if (value == null)
                return string.Empty;

            return value.ToString();
        }

        public static string GetStringFromTimeSpanAbbrev(
            TimeSpan value,
            LanguageUtilities languageUtilities)
        {
            if (value == null)
                return string.Empty;

            string timeString;
            double timeValue;
            string word;
            string abbrev;
            string format;

            if (value.TotalSeconds < 60)
            {
                timeValue = value.TotalSeconds;
                word = "seconds";
                abbrev = "s";
            }
            else if (value.TotalMinutes < 60)
            {
                timeValue = value.TotalMinutes;
                word = "minutes";
                abbrev = "m";
            }
            else if (value.TotalHours < 24)
            {
                timeValue = value.TotalHours;
                word = "hours";
                abbrev = "h";
            }
            else
            {
                timeValue = value.TotalDays;
                word = "days";
                abbrev = "d";
            }

            if ((languageUtilities != null) && (languageUtilities.UILanguage != LanguageLookup.English))
            {
                word = languageUtilities.TranslateUIString(word);

                if (!String.IsNullOrEmpty(word))
                    abbrev = word.Substring(0, 1);
            }

            format = "{0:f0}" + abbrev;
            timeString = String.Format(format, timeValue);
            return timeString;
        }

        public static DateTime GetDateTimeFromString(string str, DateTime defaultValue)
        {
            if (String.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                DateTime value = DateTime.Parse(str, CultureInfo.InvariantCulture);
                return value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string GetStringFromDateTime(DateTime value)
        {
            if (value == null)
                return string.Empty;

            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static object GetKeyFromString(string str, string type)
        {
            if (String.IsNullOrEmpty(type))
            {
                if (IsNumberString(str))
                    return Convert.ToInt32(str);
            }
            else
            {
                switch (type)
                {
                    case "String":
                        break;
                    case "Char":
                        return Convert.ToChar(str);
                    case "Int16":
                        return Convert.ToInt16(str);
                    case "Int32":
                        return Convert.ToInt32(str);
                    case "Int64":
                        return Convert.ToInt64(str);
                    case "Byte":
                        return Convert.ToByte(str);
                    case "UInt16":
                        return Convert.ToUInt16(str);
                    case "UInt32":
                        return Convert.ToUInt32(str);
                    case "UInt64":
                        return Convert.ToUInt64(str);
                    case "Single":
                        return Convert.ToSingle(str);
                    case "Double":
                        return Convert.ToDouble(str);
                    case "Boolean":
                        return Convert.ToBoolean(str);
                    case "Guid":
                        return new Guid(str);
                    case "DateTime":
                        return Convert.ToDateTime(str);
                    case "TimeSpan":
                        return TimeSpan.Parse(str);
                    case "LiteralString":
                        return new LiteralString(str);
                    default:
                        break;
                }
            }
            return str;
        }

        public static Type GetTypeFromString(string str)
        {
            switch (str)
            {
                case "":
                case null:
                    return null;
                case "String":
                    return typeof(string);
                case "Char":
                    return typeof(Char);
                case "Int16":
                    return typeof(Int16);
                case "Int32":
                    return typeof(Int32);
                case "Int64":
                    return typeof(Int64);
                case "Byte":
                    return typeof(Byte);
                case "UInt16":
                    return typeof(UInt16);
                case "UInt32":
                    return typeof(UInt32);
                case "UInt64":
                    return typeof(UInt64);
                case "Single":
                    return typeof(Single);
                case "Double":
                    return typeof(Double);
                case "Boolean":
                    return typeof(Boolean);
                case "Guid":
                    return typeof(Guid);
                case "DateTime":
                    return typeof(DateTime);
                case "TimeSpan":
                    return typeof(TimeSpan);
                default:
                    return ObjectTypes.FindType(str);
            }
        }

        public static string KeyAndSourceString(string keyString, string source)
        {
            return keyString + "." + source;
        }

        private static char[] periodChar = new char[] { '.' };

        public static bool ParseKeyAndSourceString(string keyAndSource, out string keyString, out string source)
        {
            string[] parts = keyAndSource.Split(periodChar);

            if (parts.Count() < 2)
            {
                keyString = keyAndSource;
                source = "";
                return false;
            }

            keyString = parts[0];
            source = parts[1];
            return true;
        }

        public static string PrefixSourceKeyString(string prefix, string source, object key)
        {
            return prefix + "." + source + "." + key.ToString();
        }

        private static char[] commaChar = new char[] { ',' };

        public static bool ParseTypeAndKeyString(string typeAndKey, out string typeName, out string keyString)
        {
            string[] parts = typeAndKey.Split(commaChar);

            if (parts.Count() < 2)
            {
                keyString = typeAndKey;
                typeName = "";
                return false;
            }

            typeName = parts[0];
            keyString = parts[1];
            return true;
        }

        public static int ParseIntKeyString(string keyString)
        {
            int KeyValue = -1;

            try
            {
                KeyValue = Convert.ToInt32(keyString);
            }
            catch (Exception)
            {
            }

            return KeyValue;
        }

        public static string KeySourceComponentString(string keyString, string source, string componentKey)
        {
            return keyString + "." + source + "." + componentKey;
        }

        public static bool ParseKeySourceComponentString(string keySourceComponent, out string keyString, out string source, out string componentKey)
        {
            string[] parts = keySourceComponent.Split(periodChar);

            if (parts.Count() < 3)
            {
                keyString = keySourceComponent;
                source = "";
                componentKey = "";
                return false;
            }

            keyString = parts[0];
            source = parts[1];
            componentKey = parts[2];
            return true;
        }

        public static string GetMixedCaseStringFromString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            str = str.Trim();

            if (String.IsNullOrEmpty(str))
                return str;

            if (!char.IsUpper(str[0]))
                str = str.Substring(0, 1).ToUpper() + str.Substring(1);

            int index = 0;

            while ((index = str.IndexOf(' ', index)) != -1)
            {
                str = str.Substring(0, index + 1) + str.Substring(index + 1, 1).ToUpper() + str.Substring(index + 2);
                index++;
            }

            return str;
        }

        public static string GetMixedCaseLabelFromString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            str = str.Trim();

            if (String.IsNullOrEmpty(str))
                return str;

            if (!char.IsUpper(str[0]))
                str = str.Substring(0, 1).ToUpper() + str.Substring(1);

            int index;

            while ((index = str.LastIndexOf(' ')) != -1)
                str = str.Substring(0, index) + str.Substring(index, 2).Trim().ToUpper() + str.Substring(index + 2);

            return str;
        }

        public static string GetTypeAbbreviation(object obj)
        {
            if (obj == null)
                return string.Empty;

            Type type = obj.GetType();

            string name = type.Name;
            string abbrev = GetTypeNameAbbreviation(name);

            return abbrev;
        }

        public static string GetTypeNameAbbreviation(string typeName)
        {
            string abbrev = typeName.Substring(0, 1);
            int nameLength = typeName.Length;

            for (int i = 1; i < nameLength; i++)
            {
                if (Char.IsUpper(typeName[i]))
                    abbrev += typeName[i];
            }

            return abbrev;
        }

        public static string GetStringFromObjectList<T>(List<T> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(',');

                sb.Append(list[index].ToString());
            }

            return TextUtilities.GetStringFromObjectList<T>(list);
        }

        public static string GetStringFromKeyList(List<object> list)
        {
            return TextUtilities.GetStringFromObjectList<object>(list);
        }

        public static List<object> GetKeyListFromString(string str)
        {
            return TextUtilities.GetObjectListFromString<object>(str);
        }

        public static string GetStringFromStringList(List<string> list)
        {
            return TextUtilities.GetStringFromObjectList<string>(list);
        }

        public static string GetStringFromStringListDelimited(List<string> list, string delimiter)
        {
            return TextUtilities.GetStringFromObjectListDelimited<string>(list, delimiter);
        }

        public static List<string> GetStringListFromString(string str)
        {
            return TextUtilities.GetStringListFromString(str);
        }

        public static List<string> GetStringListFromStringNoTrim(string str)
        {
            return TextUtilities.GetStringListFromStringNoTrim(str);
        }

        public static string GetStringFromIntList(List<int> list)
        {
            return TextUtilities.GetStringFromIntList(list);
        }

        public static List<int> GetIntListFromString(string str)
        {
            return TextUtilities.GetIntListFromString(str);
        }

        public static string GetStringFromFloatList(List<float> list)
        {
            return TextUtilities.GetStringFromFloatList(list);
        }

        public static List<float> GetFloatListFromString(string str)
        {
            return TextUtilities.GetFloatListFromString(str);
        }

        public static string GetStringFromDoubleList(List<double> list)
        {
            return TextUtilities.GetStringFromDoubleList(list);
        }

        public static List<double> GetDoubleListFromString(string str)
        {
            return TextUtilities.GetDoubleListFromString(str);
        }

        public static string GetStringFromLanguageIDList(List<LanguageID> list)
        {
            return TextUtilities.GetStringFromLanguageIDList(list);
        }

        public static List<LanguageID> GetLanguageIDListFromString(string str)
        {
            return TextUtilities.GetLanguageIDListFromString(str);
        }

        public static LanguageID GetLanguageIDFromElement(XElement element)
        {
            return LanguageLookup.GetLanguageIDNoAdd(element.Value.Trim());
        }

        public static XElement GetElementFromLanguageID(string name, LanguageID languageID)
        {
            return new XElement(name, languageID.LanguageCultureExtensionCode);
        }

        public static string GetStringFromTimeSpanList(List<TimeSpan> list)
        {
            return TextUtilities.GetStringFromTimeSpanList(list);
        }

        public static List<TimeSpan> GetTimeSpanListFromString(string str)
        {
            return TextUtilities.GetTimeSpanListFromString(str);
        }

        public static string GetStringFromBaseStringList(List<BaseString> list)
        {
            return TextUtilities.GetStringFromObjectList<BaseString>(list);
        }

        public static string GetQuotedStringsFromBaseStringList(List<BaseString> list, string quoteChr)
        {
            return GetQuotedStringsFromObjectList<BaseString>(list, quoteChr);
        }

        public static string GetQuotedStringsFromObjectList<T>(List<T> list, string quoteChr)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(',');
                sb.Append(quoteChr);
                sb.Append(list[index].ToString());
                sb.Append(quoteChr);
            }

            return sb.ToString();
        }

        public static List<string> GetStringListFromQuotedStrings(string str, string quoteChr)
        {
            List<string> stringList = GetStringListFromString(str);

            for (int i = 0; i < stringList.Count(); i++)
            {
                string text = stringList[i];

                if (text.StartsWith(quoteChr) && text.EndsWith(quoteChr))
                    stringList[i] = text.Substring(quoteChr.Length, text.Length - (2 * quoteChr.Length));
            }

            return stringList;
        }

        public static List<string> GetStringListFromDoubleQuotedStrings(string str)
        {
            return GetStringListFromQuotedStrings(str, "\"");
        }

        public static List<string> GetStringListFromSingleQuotedStrings(string str)
        {
            return GetStringListFromQuotedStrings(str, "'");
        }

        public static string GetQuotedStringsFromStringList(List<string> list, string quoteChr)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(',');
                sb.Append(quoteChr);
                sb.Append(list[index]);
                sb.Append(quoteChr);
            }

            return sb.ToString();
        }

        public static string GetDoubleQuotedStringsFromStringList(List<string> list)
        {
            return GetQuotedStringsFromObjectList<string>(list, "\"");
        }

        public static string GetSingleQuotedStringsFromStringList(List<string> list)
        {
            return GetQuotedStringsFromObjectList<string>(list, "'");
        }

        public static string GetDoubleQuotedNewLinedStringsFromStringList(List<string> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.AppendLine(",");
                sb.Append("\"");
                sb.Append(list[index]);
                sb.Append("\"");
            }

            if (sb.Length != 0)
                sb.AppendLine("");

            return sb.ToString();
        }

        public static string GetNewLinedStringsFromStringList(List<string> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.AppendLine(",");
                sb.Append(list[index]);
            }

            if (sb.Length != 0)
                sb.AppendLine("");

            return sb.ToString();
        }

        public static string GetNewLinedIndentedStringsFromStringList(
            List<string> list,
            int indentationCount,
            int tabSize)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            sb.AppendLine("");

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.AppendLine(",");
                sb.Append(TextUtilities.GetSpaces((indentationCount + 1) * tabSize));
                sb.Append(list[index]);
            }

            if (sb.Length != 0)
            {
                sb.AppendLine("");
                sb.Append(TextUtilities.GetSpaces(indentationCount * tabSize));
            }

            return sb.ToString();
        }

        public static string GetEscapedDoubleQuotedNewLinedStringsFromStringList(List<string> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(",\\n");
                sb.Append("\"");
                sb.Append(list[index]);
                sb.Append("\"");
            }

            if (sb.Length != 0)
                sb.Append("\\n");

            return sb.ToString();
        }

        public static string GetEscapedNewLinedStringsFromStringList(List<string> list)
        {
            if ((list == null) || (list.Count() == 0))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            int index;
            int count = list.Count();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(",\\n");
                sb.Append(list[index]);
            }

            if (sb.Length != 0)
                sb.Append("\\n");

            return sb.ToString();
        }

        public static int Compare(IBaseObjectKeyed other1, IBaseObjectKeyed other2)
        {
            if (other1 != null)
                return other1.Compare(other2);
            else if (other2 != null)
                return -1;
            else
                return 0;
        }

        public static int CompareBools(bool other1, bool other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1)
                return 1;
            return -1;
        }

        public static int CompareInts(int other1, int other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1 > other2)
                return 1;
            return -1;
        }

        public static int CompareFloats(float other1, float other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1 > other2)
                return 1;
            return -1;
        }

        public static int CompareStrings(string other1, string other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            return String.Compare(other1, other2);
        }

        public static int CompareStringLists(List<string> other1, List<string> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return CompareStrings(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareIntLists(List<int> other1, List<int> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return CompareInts(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareFloatLists(List<float> other1, List<float> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return CompareFloats(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareTimeSpanLists(List<TimeSpan> other1, List<TimeSpan> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return CompareTimeSpans(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareObjectLists(List<object> other1, List<object> other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return CompareObjects(other1[index], other2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareTypedObjectLists<T>(List<T> other1, List<T> other2) where T : IBaseObjectKeyed
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                int diff = CompareObjects(other1[index], other2[index]);
                if (diff != 0)
                    return diff;
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareDateTimes(DateTime other1, DateTime other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1 > other2)
                return 1;
            return -1;
        }

        public static int CompareTimeSpans(TimeSpan other1, TimeSpan other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1 > other2)
                return 1;
            return -1;
        }

        public static int CompareLanguageIDs(LanguageID other1, LanguageID other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            return other1.Compare(other2);
        }

        public static int CompareByteArrays(byte[] other1, byte[] other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            int count1 = other1.Count();
            int count2 = other2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (other1[index] != other2[index])
                    return other1[index] - other2[index];
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareStreams(Stream other1, Stream other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            int count1 = (int)other1.Length;
            int count2 = (int)other2.Length;
            int count = (count1 > count2 ? count2 : count1);

            if (other1.CanSeek)
                other1.Seek(0, SeekOrigin.Begin);

            if (other2.CanSeek)
                other2.Seek(0, SeekOrigin.Begin);

            for (int index = 0; index < count; index++)
            {
                int byte1 = other1.ReadByte();
                int byte2 = other2.ReadByte();

                if (byte1 != byte2)
                    return byte1 - byte2;
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }

        public static int CompareKeys(IBaseObjectKeyed other1, IBaseObjectKeyed other2)
        {
            if ((other2 == null) || (other2.Key == null))
            {
                if (other1.Key == null)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (other1.Key == null)
                    return -1;
                else
                {
                    switch (other1.Key.GetType().Name)
                    {
                        case "String":
                            return (other1.Key as string).CompareTo(other2.Key as string);
                        case "Char":
                            return ((char)other1.Key).CompareTo((char)other2.Key);
                        case "Int16":
                            return ((short)other1.Key).CompareTo((short)other2.Key);
                        case "Int32":
                            return ((int)other1.Key).CompareTo((int)other2.Key);
                        case "Int64":
                            return ((long)other1.Key).CompareTo((long)other2.Key);
                        case "Byte":
                            return ((byte)other1.Key).CompareTo((byte)other2.Key);
                        case "UInt16":
                            return ((ushort)other1.Key).CompareTo((ushort)other2.Key);
                        case "UInt32":
                            return ((uint)other1.Key).CompareTo((uint)other2.Key);
                        case "UInt64":
                            return ((ulong)other1.Key).CompareTo((ulong)other2.Key);
                        case "Single":
                            return ((float)other1.Key).CompareTo((float)other2.Key);
                        case "Double":
                            return ((double)other1.Key).CompareTo((double)other2.Key);
                        case "Boolean":
                            return ((bool)other1.Key).CompareTo((bool)other2.Key);
                        case "Guid":
                            return ((Guid)other1.Key).CompareTo((Guid)other2.Key);
                        case "DateTime":
                            return ((DateTime)other1.Key).CompareTo((DateTime)other2.Key);
                        case "TimeSpan":
                            return ((TimeSpan)other1.Key).CompareTo((TimeSpan)other2.Key);
                        default:
                            return other1.Key.ToString().CompareTo(other2.Key.ToString());
                    }
                }
            }
        }

        public static bool MatchTypes(object obj1, object obj2)
        {
            if ((obj1 == null) || (obj2 == null))
                return false;

            if (obj1.GetType().Name == obj2.GetType().Name)
                return true;

            return false;
        }

        public static bool MatchKeys(object key1, object key2)
        {
            return CompareObjects(key1, key2) == 0;
        }

        public static bool MatchKeysCaseInsensitive(object key1, object key2)
        {
            return CompareObjectsCaseInsensitive(key1, key2) == 0;
        }

        public static int CompareObjects(object other1, object other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            Type objType1 = other1.GetType();
            Type objType2 = other2.GetType();

            if (objType1 != objType2)
                return String.Compare(objType1.Name, objType2.Name);

            if (objType1 == typeof(string))
                return String.Compare(other1 as string, other2 as string);
            else if (objType1 == typeof(int))
                return (int)other1 - (int)other2;
            else if (objType1 == typeof(float))
            {
                float f1 = (float)other1;
                float f2 = (float)other2;

                if (f1 > f2)
                    return 1;
                else if (f1 < f2)
                    return -1;

                return 0;
            }
            else if (objType1 == typeof(double))
            {
                double f1 = (double)other1;
                double f2 = (double)other2;

                if (f1 > f2)
                    return 1;
                else if (f1 < f2)
                    return -1;

                return 0;
            }
            else if (objType1 == typeof(bool))
            {
                bool f1 = (bool)other1;
                bool f2 = (bool)other2;

                if (f1 == f2)
                    return 0;
                else if (f2)
                    return -1;

                return 1;
            }
            else if (other1 is IBaseObjectKeyed)
            {
                IBaseObjectKeyed base1 = other1 as IBaseObjectKeyed;
                IBaseObjectKeyed base2 = other2 as IBaseObjectKeyed;
                return base1.Compare(base2);
            }
            else
                throw new ObjectException("ObjectUtilities.CompareObjects:  Unsupported type: " + objType1.Name);
        }

        public static int CompareObjectsCaseInsensitive(object other1, object other2)
        {
            if (other1 == other2)
                return 0;

            if ((other1 == null) || (other2 == null))
            {
                if (other1 == null)
                    return -1;
                else
                    return 1;
            }

            Type objType1 = other1.GetType();
            Type objType2 = other2.GetType();

            if (objType1 == typeof(string))
                return String.Compare((other1 as string), (other2 as string), StringComparison.OrdinalIgnoreCase);
            else if (objType1 == typeof(int))
                return (int)other1 - (int)other2;
            else if (objType1 == typeof(float))
            {
                float f1 = (float)other1;
                float f2 = (float)other2;

                if (f1 > f2)
                    return 1;
                else if (f1 < f2)
                    return -1;

                return 0;
            }
            else if (objType1 == typeof(double))
            {
                double f1 = (double)other1;
                double f2 = (double)other2;

                if (f1 > f2)
                    return 1;
                else if (f1 < f2)
                    return -1;

                return 0;
            }
            else if (objType1 == typeof(bool))
            {
                bool f1 = (bool)other1;
                bool f2 = (bool)other2;

                if (f1 == f2)
                    return 0;
                else if (f2)
                    return -1;

                return 1;
            }
            else if (other1 is IBaseObjectKeyed)
            {
                IBaseObjectKeyed base1 = other1 as IBaseObjectKeyed;
                IBaseObjectKeyed base2 = other2 as IBaseObjectKeyed;
                return base1.Compare(base2);
            }
            else
                throw new ObjectException("ObjectUtilities.CompareObjectsCaseInsensitive:  Unsupported type: " + objType1.Name);
        }

        public static int CompareBaseLists<T>(List<T> list1, List<T> list2) where T : IBaseObjectKeyed
        {
            int diff;

            if (list1 == null)
            {
                if ((list2 == null) || (list2.Count() == 0))
                    diff = 0;
                else
                    diff = -1;
            }
            else if (list2 == null)
            {
                if (list1.Count() == 0)
                    diff = 0;
                else
                    diff = 1;
            }
            else
            {
                int count = list1.Count();
                diff = count - list2.Count();
                if (diff == 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        diff = list1[i].Compare(list2[i]);
                        if (diff != 0)
                            return diff;
                    }
                }
            }
            return diff;
        }

        public static List<T> ListGetUnique<T>(List<T> list)
        {
            List<T> newList = new List<T>();

            foreach (T obj in list)
            {
                if (!newList.Contains(obj))
                    newList.Add(obj);
            }

            return newList;
        }

        public static void ListAddUnique<T>(List<T> list, T obj)
        {
            if (!list.Contains(obj))
                list.Add(obj);
        }

        public static void ListAddUniqueList<T>(List<T> list, List<T> objs)
        {
            if (objs == null)
                return;

            if (list != null)
            {
                foreach (T obj in objs)
                    ListAddUnique(list, obj);
            }
        }

        public static List<T> ListConcatenateUnique<T>(List<T> list1, List<T> list2)
        {
            List<T> returnValue = new List<T>();

            ListAddUniqueList(returnValue, list1);
            ListAddUniqueList(returnValue, list2);

            return returnValue;
        }

        public static List<T> ListConcatenateUnique<T>(List<T> list1, List<T> list2, List<T> list3)
        {
            List<T> returnValue = new List<T>();

            ListAddUniqueList(returnValue, list1);
            ListAddUniqueList(returnValue, list2);
            ListAddUniqueList(returnValue, list3);

            return returnValue;
        }

        public static void MergeStrings(ref string string1, ref string string2)
        {
            if (String.IsNullOrEmpty(string1))
                string1 = string2;
            else if (String.IsNullOrEmpty(string2))
                string2 = string1;
        }

        public static Random RandomGenerator = new Random();

        public static void Shuffle<T>(this IList<T> list, Random rnd = null)
        {
            if (rnd == null)
                rnd = RandomGenerator;

            for (var i = 0; i < list.Count; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static bool TimeRunsOverlap(
            TimeSpan startTime1,
            TimeSpan stopTime1,
            TimeSpan startTime2,
            TimeSpan stopTime2)
        {
            if (stopTime2 < startTime1)
                return false;
            if (startTime2 > stopTime1)
                return false;
            return true;
        }
    }
}

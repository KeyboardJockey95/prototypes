using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class LiteralString
    {
        public string[] Strings;

        public LiteralString(string str)
        {
            if (str.Contains(","))
                StringListString = str;
            else
                Strings = new string[1] { str };
        }

        public LiteralString(string[] strs)
        {
            Strings = strs;
        }

        public LiteralString(List<string> strs)
        {
            Strings = strs.ToArray();
        }

        public LiteralString(string str1, string str2)
        {
            Strings = new string[] { str1, str2 };
        }

        public LiteralString(string str1, string str2, string str3)
        {
            Strings = new string[] { str1, str2, str3 };
        }

        public LiteralString(string str1, string str2, string str3, string str4)
        {
            Strings = new string[] { str1, str2, str3, str4 };
        }

        public LiteralString(MultiLanguageString mls)
        {
            Strings = mls.StringArray();
        }

        public LiteralString(MultiLanguageString mls, List<LanguageDescriptor> languageDescriptors)
        {
            Strings = mls.StringArray(languageDescriptors);
        }

        public LiteralString(MultiLanguageString mls, List<LanguageID> languageIDs)
        {
            Strings = mls.StringArray(languageIDs);
        }

        public LiteralString(LiteralString ls1, LiteralString sep, LiteralString ls2)
        {
            int count = ls1.Count();

            if (count == 1)
            {
                string s1 = ls1.Strings[0];
                string ss = (sep != null ? sep.Strings[0] : String.Empty);
                string s2 = ls2.Strings[0];
                string s = s1 + ss + s2;
                Strings = new string[1] { s };
            }
            else
            {
                List<string> strs = new List<string>(count);
                int index;
                for (index = 0; index < count; index++)
                {
                    string s1 = ls1.Strings[index];
                    string ss = (sep != null ? sep.Strings[index] : String.Empty);
                    string s2 = ls2.Strings[index];
                    string s = s1 + ss + s2;
                    strs.Add(s);
                }
                StringList = strs;
            }
        }

        public LiteralString(XElement element)
        {
            string str = element.Value.Trim();

            if (str.Contains(","))
                StringListString = str;
            else
                Strings = new string[1] { str };
        }

        public LiteralString(LiteralString other)
        {
            Strings = other.CloneStrings();
        }

        public LiteralString()
        {
            Strings = null;
        }

        public static implicit operator string(LiteralString obj) => obj.StringListString;

        public override string ToString()
        {
            return TextUtilities.GetStringFromStringList(StringList);
        }

        public string[] CloneStrings()
        {
            return StringList.ToArray();
        }

        public List<string> StringList
        {
            get
            {
                return Strings.ToList();
            }
            set
            {
                Strings = value.ToArray();
            }
        }

        public string StringListString
        {
            get
            {
                string stringListString = TextUtilities.GetStringFromStringList(StringList);
                return stringListString;
            }
            set
            {
                List<String> stringList = TextUtilities.GetStringListFromString(value);
                StringList = stringList;
            }
        }

        public MultiLanguageString GetMultiLanguageString(object key, List<LanguageID> languageIDs)
        {
            MultiLanguageString mls = new MultiLanguageString(key, languageIDs);

            for (int i = 0; i < languageIDs.Count(); i++)
                mls.LanguageString(i).Text = GetIndexedString(i);

            return mls;
        }

        public string GetIndexedString(int index)
        {
            if ((Strings != null) && (index >= 0) && (index < Strings.Length))
                return Strings[index];

            return String.Empty;
        }

        public int GetStringIndex(string str)
        {
            if (Strings != null)
            {
                int index = 0;

                foreach (string s in Strings)
                {
                    if (s == str)
                        return index;

                    index++;
                }
            }

            return -1;
        }

        public void AddString(string str)
        {
            List<string> stringList = StringList;
            stringList.Add(str);
            StringList = stringList;
        }

        public void AddStrings(List<string> strs)
        {
            List<string> stringList = StringList;
            stringList.AddRange(strs);
            StringList = stringList;
        }

        public void InsertString(int index, string str)
        {
            List<string> stringList = StringList;
            stringList.Insert(index, str);
            StringList = stringList;
        }

        public void InsertStrings(int index, List<string> strs)
        {
            List<string> stringList = StringList;
            stringList.InsertRange(index, strs);
            StringList = stringList;
        }

        public bool Contains(string str)
        {
            if (Strings != null)
            {
                foreach (string s in Strings)
                {
                    if (s == str)
                        return true;
                }
            }

            return false;
        }

        public bool IsEmpty()
        {
            if (Strings == null)
                return true;

            foreach (string str in Strings)
            {
                if (!String.IsNullOrEmpty(str))
                    return false;
            }

            return true;
        }

        public int Count()
        {
            if (Strings != null)
                return Strings.Count();

            return 0;
        }

        public XElement GetElement(string name)
        {
            XElement element = new XElement(name, StringListString);
            return element;
        }

        public override int GetHashCode()
        {
            return StringListString.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == (object)this)
                return true;
            if (obj == null)
                return false;
            if (obj as LiteralString == null)
                return false;
            return Compare(obj as LiteralString) == 0 ? true : false;
        }

        public virtual bool Equals(LiteralString obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(LiteralString other1, LiteralString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            return (other1.Compare(other2) == 0 ? true : false);
        }

        public static bool operator !=(LiteralString other1, LiteralString other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            return (other1.Compare(other2) == 0 ? false : true);
        }

        public int Compare(LiteralString other)
        {
            if (other == null)
                return 1;

            if (Strings == other.Strings)
                return 0;

            if (Strings == null)
                return -1;

            if (other.Strings == null)
                return 1;

            if (Strings.Length != other.Strings.Length)
                return Strings.Length - other.Strings.Length;

            int count = Strings.Length;

            for (int index = 0; index < count; index++)
            {
                if (Strings[index] != other.Strings[index])
                    return String.Compare(Strings[index], other.Strings[index]);
            }

            return 0;
        }

        public static int Compare(LiteralString string1, LiteralString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            return string1.Compare(string2);
        }

        public static LiteralString Clone(LiteralString ls)
        {
            if (ls == null)
                return null;

            return new LiteralString(ls);
        }

        public static LiteralString Concatenate(LiteralString ls1, LiteralString sep, LiteralString ls2)
        {
            int count = ls1.Count();
            LiteralString newString;

            if (count == 1)
            {
                string s1 = ls1.Strings[0];
                string ss = (sep != null ? sep.Strings[0] : String.Empty);
                string s2 = ls2.Strings[0];
                string s = s1 + ss + s2;
                newString = new LiteralString(s);
            }
            else
            {
                List<string> strs = new List<string>(count);
                int index;
                for (index = 0; index < count; index++)
                {
                    string s1 = ls1.Strings[index];
                    string ss = (sep != null ? sep.Strings[index] : String.Empty);
                    string s2 = ls2.Strings[index];
                    string s = s1 + ss + s2;
                    strs.Add(s);
                }
                newString = new LiteralString(strs);
            }

            return newString;
        }

        // Returns true if replaced.
        public static bool Replace(LiteralString target, LiteralString input, LiteralString output)
        {
            int count = input.Count();
            int index;
            bool returnValue = false;

            for (index = 0; index < count; index++)
            {
                if (target.Strings[index] == input.Strings[index])
                {
                    target.Strings[index] = output.Strings[index];
                    returnValue = true;
                }
            }

            return returnValue;
        }

        // Returns true if replaced.
        public static bool ReplaceIn(LiteralString target, LiteralString input, LiteralString output)
        {
            int count = input.Count();
            int index;
            bool returnValue = false;

            for (index = 0; index < count; index++)
            {
                if (target.Strings[index].Contains(input.Strings[index]))
                {
                    target.Strings[index] = target.Strings[index].Replace(input.Strings[index], output.Strings[index]);
                    returnValue = true;
                }
            }

            return returnValue;
        }
    }
}

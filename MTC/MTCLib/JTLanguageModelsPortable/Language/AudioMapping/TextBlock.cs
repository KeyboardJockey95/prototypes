using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Language
{
    public class TextBlock : BaseString
    {
        public TextBlock(string key, string text) : base(key, text)
        {
        }

        public TextBlock(string text) : base(text)
        {
        }

        public TextBlock(TextBlock other) : base(other)
        {
        }

        public TextBlock(string key, TextBlock other) : base(key, other)
        {
        }

        public TextBlock()
        {
        }

        public string Substring(int startIndex, int count)
        {
            if ((startIndex >= 0) && (count >= 0) && ((startIndex + count) <= TextLength))
                return Text.Substring(startIndex, count);

            return String.Empty;
        }

        public void RemoveRange(int startIndex, int count)
        {
            if ((startIndex >= 0) && (count >= 0) && ((startIndex + count) <= TextLength))
                Text = Text.Remove(startIndex, count);
        }

        public void Insert(int startIndex, string str)
        {
            if (String.IsNullOrEmpty(str))
                return;

            if ((startIndex >= 0) && (startIndex <= TextLength))
                Text = Text.Insert(startIndex, str);
        }
    }
}

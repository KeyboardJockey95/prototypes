using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Content
{
    public class WordMapping
    {
        public string LanguageCode;
        public int[] WordIndexes;

        public WordMapping(LanguageID toLanguageID, int[] toWordIndexes)
        {
            LanguageCode = toLanguageID.LanguageCultureExtensionCode;
            WordIndexes = toWordIndexes;
        }

        public WordMapping(string toLanguageCode, int[] toWordIndexes)
        {
            LanguageCode = toLanguageCode;
            WordIndexes = toWordIndexes;
        }

        public WordMapping(WordMapping other)
        {
            LanguageCode = other.LanguageCode;
            WordIndexes = other.WordIndexes;
        }

        public WordMapping()
        {
            LanguageCode = null;
            WordIndexes = null;
        }

        public void Copy(WordMapping other)
        {
            LanguageCode = other.LanguageCode;
            WordIndexes = other.WordIndexes;
        }

        public override string ToString()
        {
            string display = LanguageCode + ": ";

            if (WordIndexes == null)
                display += "(null)";
            else
            {
                bool first = true;
                foreach (int wordIndex in WordIndexes)
                {
                    if (first)
                        first = false;
                    else
                        display += ",";
                    display += wordIndex.ToString();
                }
            }

            return display;
        }

        public bool HasWordIndexes()
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                return false;

            return true;
        }

        public int GetWordIndex(int index)
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                return -1;

            return WordIndexes[index];
        }

        public int GetWordIndexIndex(int wordIndex)
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                return -1;

            int index = Array.IndexOf(WordIndexes, wordIndex);

            return index;
        }

        public int[] CloneWordIndexes()
        {
            int[] newWordIndexes = new int[WordIndexCount()];
            Array.Copy(WordIndexes, 0, newWordIndexes, 0, WordIndexCount());
            return newWordIndexes;
        }
        public bool ContainsWordIndex(int wordIndex)
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                return false;

            return WordIndexes.Contains(wordIndex);
        }

        public void AddWordIndex(int wordIndex)
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                WordIndexes = new int[] { wordIndex };
            else
                WordIndexes = Add(WordIndexes, wordIndex);
        }

        protected static int[] Add(int[] source, int wordIndex)
        {
            int[] dest = new int[source.Length + 1];

            Array.Copy(source, 0, dest, 0, source.Length);

            dest[source.Length] = wordIndex;

            return dest;
        }

        public bool DeleteWordIndexIndexed(int index)
        {
            if ((WordIndexes == null) || (WordIndexes.Length == 0))
                return false;

            if ((index < 0) || (index >= WordIndexes.Length))
                return false;

            WordIndexes = RemoveAt(WordIndexes, index);

            return true;
        }

        protected static int[] RemoveAt(int[] source, int index)
        {
            int[] dest = new int[source.Length - 1];

            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public int WordIndexCount()
        {
            if (WordIndexes != null)
                return WordIndexes.Length;

            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatDictionaryRecord
    {
        public string TargetText;
        public LanguageID TargetLanguageID;
        public string Lemma;
        public string IpaReading;
        public List<int> SourceIDs;
        public List<LanguageString> Alternates;
        public List<string> HostSynonyms;
        public LanguageID HostLanguageID;
        public LexicalCategory Category;
        public string CategoryString;
        public List<LexicalAttribute> Attributes;
        public int Priority;
        public List<MultiLanguageString> Examples;
        public DictionaryEntry Entry;

        public FormatDictionaryRecord(
            string targetText,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID)
        {
            ClearFormatDictionaryRecord();
            TargetText = targetText;
            TargetLanguageID = targetLanguageID;
            HostLanguageID = hostLanguageID;
            HostSynonyms = new List<string>();
        }

        public FormatDictionaryRecord(FormatDictionaryRecord other)
        {
            CopyFormatDictionaryRecord(other);
        }

        public FormatDictionaryRecord()
        {
            ClearFormatDictionaryRecord();
        }

        public void ClearFormatDictionaryRecord()
        {
            TargetText = null;
            TargetLanguageID = null;
            Lemma = null;
            IpaReading = null;
            SourceIDs = null;
            Alternates = null;
            HostSynonyms = null;
            HostLanguageID = null;
            Category = LexicalCategory.Unknown;
            CategoryString = null;
            Attributes = null;
            Priority = 0;
            Examples = null;
            Entry = null;
        }

        public void CopyFormatDictionaryRecord(FormatDictionaryRecord other)
        {
            TargetText = other.TargetText;
            TargetLanguageID = other.TargetLanguageID;
            Lemma = other.Lemma;
            IpaReading = other.IpaReading;
            SourceIDs = other.SourceIDs;
            Alternates = other.CloneAlternates();
            HostSynonyms = other.CloneHostSynonyms();
            HostLanguageID = other.HostLanguageID;
            Category = other.Category;
            CategoryString = other.CategoryString;
            Attributes = other.CloneAttributes();
            Priority = other.Priority;
            Examples = other.CloneExamples();
            Entry = other.Entry;
        }

        public void AddAlternateText(
            LanguageID languageID,
            string text)
        {
            LanguageID rootLanguageID = LanguageLookup.GetRootLanguageID(languageID);
            int familyCount = LanguageLookup.GetFamilyLanguageIDCount(rootLanguageID);
            int index = 0;

            if (Alternates == null)
                Alternates = new List<LanguageString>() { new LanguageString(index, languageID, text) };
            else
            {
                index = Alternates.Count() / familyCount;

                LanguageString alternate = new LanguageString(index, languageID, text);

                Alternates.Add(alternate);
            }
        }

        public List<LanguageString> CloneAlternates()
        {
            if (Alternates == null)
                return null;

            return new List<LanguageString>(Alternates);
        }

        public bool HasCategoryString()
        {
            return !String.IsNullOrEmpty(CategoryString);
        }

        public void PrependCategoryString(string categoryString)
        {
            if (String.IsNullOrEmpty(CategoryString))
                CategoryString = categoryString;
            else
                CategoryString = categoryString + "," + CategoryString;
        }

        public void AppendCategoryString(string categoryString)
        {
            if (String.IsNullOrEmpty(CategoryString))
                CategoryString = categoryString;
            else
                CategoryString += "," + categoryString;
        }

        public void AppendHostSynonym(string synonym)
        {
            if (HostSynonyms == null)
                HostSynonyms = new List<string>() { synonym };
            else
                HostSynonyms.Add(synonym);
        }

        public List<string> CloneHostSynonyms()
        {
            if (HostSynonyms == null)
                return null;

            return new List<string>(HostSynonyms);
        }

        public List<LexicalAttribute> CloneAttributes()
        {
            if (Attributes == null)
                return null;

            return new List<LexicalAttribute>(Attributes);
        }

        public List<MultiLanguageString> CloneExamples()
        {
            if (Examples == null)
                return null;

            return new List<MultiLanguageString>(Examples);
        }
    }
}

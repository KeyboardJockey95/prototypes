using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tables
{
    public class ItemStatistics
    {
        public string NodePath { get; set; }
        public StringBuilder TextCollector { get; set; }
        protected string _Text;
        public string Text
        {
            get
            {
                if (_Text.Length == 0)
                    _Text = TextCollector.ToString().ToLower();

                return _Text;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _Text = value.ToLower();
                else
                    _Text = String.Empty;
            }
        }
        protected string _TextLower;
        public string TextLower
        {
            get
            {
                if (_TextLower.Length == 0)
                    _TextLower = TextCollector.ToString().ToLower();

                return _Text;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _TextLower = value.ToLower();
                else
                    _TextLower = String.Empty;
            }
        }
        public ItemStatistics ParentStatistics { get; set; }
        public List<ItemStatistics> ChildrenStatistics { get; set; }
        public Dictionary<string, ProbableDefinition> WordDictionary { get; set; }
        public List<ProbableDefinition> WordList { get; set; }
        public Dictionary<string, int> WordFrequencyCache { get; set; }

        public ItemStatistics(
            string nodePath,
            ItemStatistics parentStatistics)
        {
            ClearItemStatistics();
            NodePath = nodePath;
            ParentStatistics = parentStatistics;
        }

        public ItemStatistics(string nodePath)
        {
            ClearItemStatistics();
            NodePath = nodePath;
        }

        public ItemStatistics(ItemStatistics other)
        {
            CopyItemStatistics(other);
        }

        public ItemStatistics()
        {
            ClearItemStatistics();
        }

        public void ClearItemStatistics()
        {
            NodePath = null;
            TextCollector = new StringBuilder();
            _Text = string.Empty;
            _TextLower = string.Empty;
            ParentStatistics = null;
            ChildrenStatistics = null;
            WordDictionary = new Dictionary<string, ProbableDefinition>(StringComparer.OrdinalIgnoreCase);
            WordList = new List<ProbableDefinition>();
            WordFrequencyCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public void CopyItemStatistics(ItemStatistics other)
        {
            NodePath = other.NodePath;
            TextCollector = other.TextCollector;
            _Text = other._Text;
            _TextLower = other._TextLower;
            ParentStatistics = other.ParentStatistics;

            if (other.ChildrenStatistics != null)
                ChildrenStatistics = new List<ItemStatistics>(other.ChildrenStatistics);
            else
                ChildrenStatistics = null;

            if (other.WordDictionary != null)
                WordDictionary = new Dictionary<string, ProbableDefinition>(
                    other.WordDictionary, StringComparer.OrdinalIgnoreCase);
            else
                WordDictionary = new Dictionary<string, ProbableDefinition>(StringComparer.OrdinalIgnoreCase);

            if (other.WordList != null)
                WordList = new List<ProbableDefinition>(other.WordList);
            else
                WordList = new List<ProbableDefinition>();

            WordFrequencyCache = new Dictionary<string, int>(
                other.WordFrequencyCache, StringComparer.OrdinalIgnoreCase);
        }

        public void AppendWordText(string word)
        {
            if (TextCollector.Length != 0)
                TextCollector.Append(" " + word);
            else
                TextCollector.Append(word);
        }

        public bool HasWord(string word)
        {
            ProbableDefinition definition;

            if (!WordDictionary.TryGetValue(word, out definition))
                return false;

            return true;
        }

        public string GetWord(string word)
        {
            ProbableDefinition definition;

            if (WordDictionary.TryGetValue(word, out definition))
                return definition.TargetMeaning;

            return String.Empty;
        }

        public ProbableDefinition GetDefinition(string word)
        {
            ProbableDefinition definition;

            if (WordDictionary.TryGetValue(word, out definition))
                return definition;

            return null;
        }

        public int WordCount()
        {
            return WordList.Count();
        }

        public bool HasChildStatistics(string nodePath)
        {
            if (ChildrenStatistics == null)
                return false;

            if (ChildrenStatistics.FirstOrDefault(x => x.NodePath == nodePath) != null)
                return true;

            return false;
        }

        public ItemStatistics GetChildStatistics(string nodePath)
        {
            if (ChildrenStatistics == null)
                return null;

            return ChildrenStatistics.FirstOrDefault(x => x.NodePath == nodePath);
        }

        public void AddChildStatistics(ItemStatistics childStatistics)
        {
            if (ChildrenStatistics == null)
                ChildrenStatistics = new List<ItemStatistics>();

            ChildrenStatistics.Add(childStatistics);
        }

        public int GetWordFrequency(string word)
        {
            ProbableDefinition definition;
            int frequency = 0;

            if (WordDictionary.TryGetValue(word, out definition))
                frequency = definition.Frequency;

            return frequency;
        }

        public int GetPhraseFrequency(string phrase)
        {
            int frequency = 0;

            phrase = phrase.ToLower();

            if (WordFrequencyCache.TryGetValue(phrase, out frequency))
                return frequency;

            if (phrase.Contains(" "))
                frequency = TextUtilities.CountWordsOrPhrases(TextLower, phrase.ToLower(), false);
            else
                frequency = GetWordFrequency(phrase);

            WordFrequencyCache.Add(phrase, frequency);

            return frequency;
        }

        public int GetPhraseWordMinimumFrequency(string phrase)
        {
            int frequency = int.MaxValue;

            string[] parts = phrase.Split(LanguageLookup.Space);

            foreach (string part in parts)
            {
                int testFrequency = GetPhraseFrequency(part);

                if (testFrequency < frequency)
                    frequency = testFrequency;
            }

            if (frequency == int.MaxValue)
                frequency = 0;

            return frequency;
        }

        public int GetPhraseWordMaximumFrequency(string phrase)
        {
            int frequency = 0;

            string[] parts = phrase.Split(LanguageLookup.Space);

            foreach (string part in parts)
            {
                int testFrequency = GetPhraseFrequency(part);

                if (testFrequency > frequency)
                    frequency = testFrequency;
            }

            return frequency;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public class FrequencyTable : BaseObjectLanguage
    {
        protected string _Type;
        protected Dictionary<string, int> _Table;
        protected string _FilePath;
        public string[] Types =
        {
            "Lemma",
            "Inflection"
        };

        public FrequencyTable(
            LanguageID languageID,
            string type) : base(ComposeKey(languageID, type), languageID)
        {
            ClearFrequencyTable();
            _Type = type;
            _FilePath = DefaultFilePath(type);
            LoadTable(_FilePath);
        }

        public FrequencyTable(
            LanguageID languageID,
            string type,
            string filePath) : base(ComposeKey(languageID, type), languageID)
        {
            ClearFrequencyTable();
            _Type = type;
            _FilePath = filePath;
            LoadTable(_FilePath);
        }

        public FrequencyTable(FrequencyTable other)
            : base(other)
        {
            CopyFrequencyTable(other);
        }

        public FrequencyTable()
        {
            ClearFrequencyTable();
        }

        public override void Clear()
        {
            base.Clear();
            ClearFrequencyTable();
        }

        public void ClearFrequencyTable()
        {
            _Table = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _Type = null;
            _FilePath = null;
        }

        public void CopyFrequencyTable(FrequencyTable other)
        {
            _Table = new Dictionary<string, int>(other._Table);
            _Type = other._Type;
            _FilePath = other._FilePath;
        }

        public override IBaseObject Clone()
        {
            return new FrequencyTable(this);
        }

        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        public int GetWordFrequency(string word)
        {
            int frequency;

            if (_Table.TryGetValue(word, out frequency))
                return frequency;

            return 0;
        }

        public void SetFrequency(string word, int frequency)
        {
            int testFrequency;

            if (_Table.TryGetValue(word, out testFrequency))
                _Table[word] = frequency;
            else
                _Table.Add(word, frequency);
        }

        public int GetWordCount()
        {
            return _Table.Count();
        }

        public bool HasWords()
        {
            return _Table.Count() != 0;
        }

        public static string ComposeKey(
            LanguageID languageID,
            string type)
        {
            string key = languageID.SymbolName + "|" + type;
            return key;
        }

        protected string DefaultFilePath(string type)
        {
            string fileName = "Frequency_" + type + "_" + LanguageID.SymbolName + ".txt";
            string filePath = ApplicationData.Global.GetLocalDataFilePath("Frequency", fileName);
            return filePath;
        }

        protected bool LoadTable(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return false;

            if (!FileSingleton.Exists(filePath))
                return false;

            try
            {
                string[] lines = FileSingleton.ReadAllLines(filePath, ApplicationData.Encoding);

                foreach (string line in lines)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    int separatorOffset = line.IndexOf('\t');

                    if (separatorOffset == -1)
                        continue;

                    string word = line.Substring(0, separatorOffset);
                    string numString = line.Substring(separatorOffset + 1);
                    int frequency = ObjectUtilities.GetIntegerFromString(numString, 0);
                    int testFrequency;

                    if (_Table.TryGetValue(word, out testFrequency))
                    {
                        if (frequency > testFrequency)
                            _Table[word] = frequency + testFrequency;
                    }
                    else
                        _Table.Add(word, frequency);
                }

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        protected bool SaveTable(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return false;

            if (!FileSingleton.Exists(filePath))
                return false;

            List<string> lines = new List<string>();

            foreach (KeyValuePair<string, int> kvp in _Table)
            {
                string line = kvp.Key + "\t" + kvp.Value.ToString();
                lines.Add(line);
            }

            try
            {
                FileSingleton.WriteAllLines(filePath, lines.ToArray(), ApplicationData.Encoding);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        public bool Update()
        {
            return SaveTable(_FilePath);
        }
    }
}

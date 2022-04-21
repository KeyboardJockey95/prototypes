using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class LanguageToolFactory
    {
        protected Dictionary<string, LanguageTool> _LanguageTools;

        public static LanguageToolFactory Factory;

        public LanguageToolFactory()
        {
            _LanguageTools = new Dictionary<string, LanguageTool>();
            Factory = this;
        }

        public Dictionary<string, LanguageTool> LanguageTools
        {
            get
            {
                return _LanguageTools;
            }
            set
            {
                _LanguageTools = value;
            }
        }

        public void Add(LanguageTool languageTool)
        {
            if (_LanguageTools.ContainsKey(languageTool.Name))
                return;

            _LanguageTools.Add(languageTool.Name, languageTool);
        }

        public void AddDefaults()
        {
            Add(new ChineseTool());
            Add(new EnglishTool());
            Add(new FrenchTool());
            Add(new ItalianTool());
            Add(new GermanTool());

            // Still under development.
            //Add(new JapaneseTool());
            Add(new JapaneseToolCode());

            Add(new PortugueseTool());
            Add(new SpanishTool());
        }

        public virtual LanguageTool Create(LanguageID languageID)
        {
            LanguageTool languageTool = null;

            if (_LanguageTools.TryGetValue(languageID.LanguageCode, out languageTool))
                languageTool = languageTool.Clone() as LanguageTool;
            else if (_LanguageTools.TryGetValue(languageID.LanguageCultureExtensionCode, out languageTool))
                languageTool = languageTool.Clone() as LanguageTool;
            return languageTool;
        }

        public virtual LanguageTool GetCached(LanguageID languageID)
        {
            LanguageTool languageTool = null;

            if (_LanguageTools.TryGetValue(languageID.LanguageCode, out languageTool))
                return languageTool;
            else if (_LanguageTools.TryGetValue(languageID.LanguageCultureExtensionCode, out languageTool))
                return languageTool;

            return null;
        }

        public virtual T CreateTyped<T>(LanguageID languageID) where T : LanguageTool
        {
            return Create(languageID) as T;
        }

        public List<LanguageID> LanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();

                foreach (KeyValuePair<string, LanguageTool> kvp in _LanguageTools)
                    languageIDs.Add(kvp.Value.LanguageID);

                return languageIDs;
            }
        }
    }
}

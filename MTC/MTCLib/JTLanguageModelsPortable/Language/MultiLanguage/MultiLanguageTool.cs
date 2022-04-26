using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class MultiLanguageTool : BaseObjectLanguages
    {
        protected List<LanguageTool> _TargetLanguageTools;
        protected List<LanguageTool> _HostLanguageTools;
        protected Dictionary<string, Dictionary<string, List<string>>> _DesignatorMatchCache;

        public MultiLanguageTool(
                List<LanguageTool> targetLanguageTools,
                List<LanguageTool> hostLanguageTools)
        {
            _TargetLanguageTools = targetLanguageTools;
            _HostLanguageTools = hostLanguageTools;
            _DesignatorMatchCache = null;
            InitializeLanguages();
        }

        public MultiLanguageTool(
                LanguageTool targetLanguageTool,
                LanguageTool hostLanguageTool)
        {
            if (targetLanguageTool != null)
                _TargetLanguageTools = new List<LanguageTool>(1) { targetLanguageTool };
            else
                _TargetLanguageTools = new List<LanguageTool>();
            if (hostLanguageTool != null)
                _HostLanguageTools = new List<LanguageTool>(1) { hostLanguageTool };
            else
                _HostLanguageTools = new List<LanguageTool>();
            _DesignatorMatchCache = null;
            InitializeLanguages();
        }

        public MultiLanguageTool(MultiLanguageTool other)
        {
            CopyMultiLanguageTool(other);
            Modified = false;
        }

        public MultiLanguageTool()
        {
            ClearMultiLanguageTool();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMultiLanguageTool();
        }

        public void CopyMultiLanguageTool(MultiLanguageTool other)
        {
            _TargetLanguageTools = CloneTargetLanguageTools();
            _HostLanguageTools = CloneHostLanguageTools();
            _DesignatorMatchCache = null;
            InitializeLanguages();
        }

        public void ClearMultiLanguageTool()
        {
            _TargetLanguageTools = new List<LanguageTool>();
            _HostLanguageTools = new List<LanguageTool>();
            _DesignatorMatchCache = null;
        }

        public override IBaseObject Clone()
        {
            return new MultiLanguageTool(this);
        }

        public void InitializeLanguages()
        {
            if (_TargetLanguageIDs == null)
                _TargetLanguageIDs = new List<LanguageID>();
            else
                _TargetLanguageIDs.Clear();

            if (_TargetLanguageTools != null)
            {
                foreach (LanguageTool languageTool in _TargetLanguageTools)
                    ObjectUtilities.ListAddUniqueList(_TargetLanguageIDs, languageTool.TargetLanguageIDs);
            }

            if (_HostLanguageIDs == null)
                _HostLanguageIDs = new List<LanguageID>();
            else
                _HostLanguageIDs.Clear();

            if (_HostLanguageTools != null)
            {
                foreach (LanguageTool languageTool in _HostLanguageTools)
                    ObjectUtilities.ListAddUniqueList(_HostLanguageIDs, languageTool.TargetLanguageIDs);
            }

            Key = ComposeLanguagesKey(TargetLanguageIDs, HostLanguageIDs);
            Modified = false;
        }

        public string ComposeLanguagesKey(List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs)
        {
            StringBuilder sb = new StringBuilder();

            if (targetLanguageIDs != null)
            {
                foreach (LanguageID languageID in targetLanguageIDs)
                    sb.Append(languageID.LanguageCode);
            }

            sb.Append("_");

            if (hostLanguageIDs != null)
            {
                foreach (LanguageID languageID in hostLanguageIDs)
                    sb.Append(languageID.LanguageCode);
            }

            return sb.ToString();
        }

        public List<LanguageTool> LanguageTools
        {
            get
            {
                return ObjectUtilities.ListConcatenateUnique<LanguageTool>(TargetLanguageTools, HostLanguageTools);
            }
        }

        public List<LanguageTool> CloneLanguageTools()
        {
            List<LanguageTool> languageTools = null;
            List<LanguageTool> sourceLanguageTools = LanguageTools;

            if (sourceLanguageTools != null)
            {
                languageTools = new List<LanguageTool>();

                foreach (LanguageTool languageTool in sourceLanguageTools)
                    languageTools.Add(languageTool.Clone() as LanguageTool);
            }

            return languageTools;
        }

        public int LanguageToolCount()
        {
            List<LanguageTool> sourceLanguageTools = LanguageTools;

            if (sourceLanguageTools != null)
                return sourceLanguageTools.Count();

            return 0;
        }

        public LanguageTool GetLanguageToolIndexed(int index)
        {
            List<LanguageTool> sourceLanguageTools = LanguageTools;

            if (sourceLanguageTools != null)
            {
                if ((index >= 0) && (index <= sourceLanguageTools.Count()))
                    return sourceLanguageTools[index];
            }

            return null;
        }

        public LanguageTool GetLanguageTool(LanguageID languageID)
        {
            List<LanguageTool> sourceLanguageTools = LanguageTools;

            if (sourceLanguageTools != null)
                return sourceLanguageTools.FirstOrDefault(x => x.LanguageID == languageID);

            return null;
        }

        public List<LanguageTool> TargetLanguageTools
        {
            get
            {
                return _TargetLanguageTools;
            }
            set
            {
                if (_TargetLanguageTools != value)
                {
                    _TargetLanguageTools = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageTool> CloneTargetLanguageTools()
        {
            List<LanguageTool> languageTools = null;

            if (_TargetLanguageTools != null)
            {
                languageTools = new List<LanguageTool>();

                foreach (LanguageTool languageTool in _TargetLanguageTools)
                    languageTools.Add(languageTool.Clone() as LanguageTool);
            }

            return languageTools;
        }

        public int TargetLanguageToolCount()
        {
            if (_TargetLanguageTools != null)
                return _TargetLanguageTools.Count();

            return 0;
        }

        public LanguageTool GetTargetLanguageToolIndexed(int index)
        {
            if (_TargetLanguageTools != null)
            {
                if ((index >= 0) && (index <= _TargetLanguageTools.Count()))
                    return _TargetLanguageTools[index];
            }

            return null;
        }

        public LanguageTool GetTargetLanguageTool(LanguageID languageID)
        {
            if (_TargetLanguageTools != null)
                return _TargetLanguageTools.FirstOrDefault(x => x.LanguageID == languageID);

            return null;
        }

        public void AddTargetLanguageTool(LanguageTool languageTool)
        {
            if (_TargetLanguageTools == null)
                _TargetLanguageTools = new List<LanguageTool>();

            _TargetLanguageTools.Add(languageTool);
        }

        public List<LanguageTool> HostLanguageTools
        {
            get
            {
                return _HostLanguageTools;
            }
            set
            {
                if (_HostLanguageTools != value)
                {
                    _HostLanguageTools = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageTool> CloneHostLanguageTools()
        {
            List<LanguageTool> languageTools = null;

            if (_HostLanguageTools != null)
            {
                languageTools = new List<LanguageTool>();

                foreach (LanguageTool languageTool in _HostLanguageTools)
                    languageTools.Add(languageTool.Clone() as LanguageTool);
            }

            return languageTools;
        }

        public int HostLanguageToolCount()
        {
            if (_HostLanguageTools != null)
                return _HostLanguageTools.Count();

            return 0;
        }

        public LanguageTool GetHostLanguageToolIndexed(int index)
        {
            if (_HostLanguageTools != null)
            {
                if ((index >= 0) && (index <= _HostLanguageTools.Count()))
                    return _HostLanguageTools[index];
            }

            return null;
        }

        public LanguageTool GetHostLanguageTool(LanguageID languageID)
        {
            if (_HostLanguageTools != null)
                return _HostLanguageTools.FirstOrDefault(x => x.LanguageID == languageID);

            return null;
        }

        public void AddHostLanguageTool(LanguageTool languageTool)
        {
            if (_HostLanguageTools == null)
                _HostLanguageTools = new List<LanguageTool>();

            _HostLanguageTools.Add(languageTool);
        }
    }
}

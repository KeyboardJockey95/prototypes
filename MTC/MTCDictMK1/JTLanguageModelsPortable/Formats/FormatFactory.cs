using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatFactory
    {
        protected Dictionary<string, Format> _Formats;

        public FormatFactory()
        {
            _Formats = new Dictionary<string, Format>();
        }

        public Dictionary<string, Format> Formats
        {
            get
            {
                return _Formats;
            }
            set
            {
                _Formats = value;
            }
        }

        public void Add(Format format)
        {
            if (_Formats.ContainsKey(format.Name))
                return;

            _Formats.Add(format.Name, format);
        }

        public virtual Format Create(string name, string targetType, string targetLabel,
            UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities)
        {
            Format format = null;

            if (_Formats.TryGetValue(name, out format))
            {
                format = format.Clone();
                format.TargetType = targetType;
                format.TargetLabel = targetLabel;
                format.UserRecord = userRecord;
                format.UserProfile = userProfile;
                format.Repositories = repositories;
                format.LanguageUtilities = languageUtilities;
                if ((nodeUtilities == null) && (userProfile != null))
                    nodeUtilities = new NodeUtilities(repositories, null, userRecord, userProfile, null, languageUtilities);
                format.NodeUtilities = nodeUtilities;
                format.SetupLanguages();
            }

            return format;
        }

        public virtual T CreateTyped<T>(string name, string targetType, string targetLabel,
            UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities) where T : Format
        {
            return Create(name, targetType, targetLabel, userRecord, userProfile, repositories, languageUtilities, nodeUtilities) as T;
        }

        public List<string> Names
        {
            get
            {
                List<string> names = new List<string>();

                foreach (KeyValuePair<string, Format> kvp in _Formats)
                    names.Add(kvp.Value.Name);

                return names;
            }
        }

        public List<string> Types
        {
            get
            {
                List<string> types = new List<string>();

                foreach (KeyValuePair<string, Format> kvp in _Formats)
                    types.Add(kvp.Value.Type);

                return types;
            }
        }

        public List<string> GetSupportedNames(
            string importExport,
            string componentName,
            string capability,
            UserRecord userRecord)
        {
            List<string> names = new List<string>();

            foreach (KeyValuePair<string, Format> kvp in _Formats)
            {
                if (kvp.Value.IsSupportedVirtual(importExport, componentName, capability))
                {
                    string errorMessage = null;

                    if (kvp.Value.PermissionsCheck(userRecord, ref errorMessage))
                        names.Add(kvp.Value.Name);
                }
            }

            return names;
        }

        public List<string> GetSupportedTypes(string importExport, string componentName, string capability)
        {
            List<string> types = new List<string>();

            foreach (KeyValuePair<string, Format> kvp in _Formats)
            {
                if (kvp.Value.IsSupportedVirtual(importExport, componentName, capability))
                    types.Add(kvp.Value.Type);
            }

            return types;
        }
    }
}

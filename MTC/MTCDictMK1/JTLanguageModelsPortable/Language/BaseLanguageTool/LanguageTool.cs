using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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
    public partial class LanguageTool : BaseObjectLanguage
    {
        protected List<LanguageID> _TargetLanguageIDs;
        protected List<LanguageID> _HostLanguageIDs;
        protected List<LanguageID> _UserLanguageIDs;
        protected Dictionary<string, bool> _Capabilities;
        protected Dictionary<string, InflectionsLayout> _InflectionsLayoutDictionary;   // Type is key.
        protected NodeUtilities _NodeUtilities;
        protected MultiLanguageTool _MultiTool;
        protected bool _AllowArchaicInflections;
        public bool UseFileBasedEndingsTable = false;
        public List<string> EndingsTableSource = null;
        protected int EndingsTableVersion = 0;
        protected LexTable _EndingsTable = null;
        protected bool _EndingsTableChecked = false;
        protected InflectionOutputMode _DefaultInflectionOutputMode;
        protected bool _UsesImplicitPronouns;
        protected List<string> _VerbClasses;    // i.e. "ar", "er", "ir" for Spanish.
        protected string _VerbStemType;         // "RemoveVerbClass"
        protected static Dictionary<string, LexTable> _EndingsTableCache = null;        // Obsolete.
        protected Dictionary<string, InflectorTable> _InflectorTableDictionary;         // Type is key.
        protected static Dictionary<string, InflectorTable> _InflectorTableCache = null;        // Used by all LanguageTools.
        protected static Dictionary<string, InflectionsLayout> _InflectionsLayoutCache = null;  // Used by all LanguageTools.
        protected static Dictionary<string, FrequencyTable> _FrequencyTableCache = null;        // Used by all LanguageTools.

        // List of standard capability names.
        public static List<string> StandardCapabilityNames = new List<string>()
        {
            "CanInflectVerbs",
            "InflectorTableCheckedVerbs",
            "InflectionsLayoutCheckedVerbs",
            "CanInflectNouns",
            "InflectorTableCheckedNouns",
            "InflectionsLayoutCheckedNouns",
            "CanInflectAdjectives",
            "InflectorTableCheckedAdjectives",
            "InflectionsLayoutCheckedAdjectives",
            "CanInflectUnknown",
            "InflectorTableCheckedUnknown",
            "InflectionsLayoutCheckedUnknown",
            "CanDeinflect"
        };

        // For derived classes.
        public LanguageTool(LanguageID languageID) : base(languageID.LanguageCode, languageID)
        {
            ClearLanguageTool();
            _TargetLanguageIDs = new List<LanguageID>() { languageID };
        }

        public LanguageTool()
        {
            ClearLanguageTool();
        }

        public void ClearLanguageTool()
        {
            _TargetLanguageIDs = null;
            _HostLanguageIDs = null;
            _UserLanguageIDs = null;
            _Capabilities = new Dictionary<string, bool>();
            //_InflectorTableDictionary = new Dictionary<string, InflectorTable>();
            _InflectionsLayoutDictionary = new Dictionary<string, InflectionsLayout>();
            _AllowArchaicInflections = false;
            _DictionaryDatabase = null;
            _DeinflectionDatabase = null;
            _NodeUtilities = null;
            _MultiTool = null;
            _SentenceFixes = null;
            _WordFixes = null;
            //_DefaultInflectionOutputMode = InflectionOutputMode.All;
            _DefaultInflectionOutputMode = InflectionOutputMode.FullNoMain;
            _UsesImplicitPronouns = false;
            _VerbClasses = null;
            _VerbStemType = null;
            ClearDictionaryCaches();
        }

        public override IBaseObject Clone()
        {
            return new LanguageTool();
        }

        public virtual List<LanguageID> TargetLanguageIDs
        {
            get
            {
                return _TargetLanguageIDs;
            }
            set
            {
                if (LanguageID.CompareLanguageIDLists(_TargetLanguageIDs, value) != 0)
                {
                    _TargetLanguageIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual LanguageID RootLanguageID
        {
            get
            {
                LanguageID rootLanguageID = LanguageLookup.GetRoot(_TargetLanguageIDs);
                return rootLanguageID;
            }
        }

        public virtual LanguageID RomanizedLanguageID
        {
            get
            {
                LanguageID romanizedLanguageID = LanguageLookup.GetRomanized(_TargetLanguageIDs);
                return romanizedLanguageID;
            }
        }

        public virtual LanguageID NonRomanizedPhoneticLanguageID
        {
            get
            {
                LanguageID nonRomanizedPhoneticLanguageID = LanguageLookup.GetNonRomanizedPhonetic(_TargetLanguageIDs);
                return nonRomanizedPhoneticLanguageID;
            }
        }

        public virtual List<LanguageID> HostLanguageIDs
        {
            get
            {
                return _HostLanguageIDs;
            }
            set
            {
                if (LanguageID.CompareLanguageIDLists(_HostLanguageIDs, value) != 0)
                {
                    _HostLanguageIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual List<LanguageID> UserLanguageIDs
        {
            get
            {
                return _UserLanguageIDs;
            }
            set
            {
                if (LanguageID.CompareLanguageIDLists(_UserLanguageIDs, value) != 0)
                {
                    _UserLanguageIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual List<LanguageID> LanguageIDs
        {
            get
            {
                return ObjectUtilities.ListConcatenateUnique<LanguageID>(TargetLanguageIDs, HostLanguageIDs);
            }
        }

        public LanguageDescription Description
        {
            get
            {
                return LanguageLookup.GetLanguageDescription(RootLanguageID);
            }
        }

        public Dictionary<string, bool> Capabilities
        {
            get
            {
                return _Capabilities;
            }
            set
            {
                if (value != _Capabilities)
                {
                    _Capabilities = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasCapability(string name)
        {
            bool state;

            if (_Capabilities.TryGetValue(name, out state))
                return state;

            return false;
        }

        public void SetCapability(string name, bool state)
        {
            bool oldState;

            if (_Capabilities.TryGetValue(name, out oldState))
                _Capabilities[name] = state;
            else
                _Capabilities.Add(name, state);
        }

        // type can be "Verb", "Noun", "Adjective", "Unknown", etc. depending on the language.
        public bool CanInflect(string type)
        {
            return HasCapability("CanInflect" + type);
        }

        public void SetCanInflect(string type, bool state)
        {
            SetCapability("CanInflect" + type, state);
        }

        public bool CanInflectCategory(LexicalCategory category)
        {
            if ((category == LexicalCategory.Stem) || (category == LexicalCategory.IrregularStem))
                category = LexicalCategory.Verb;

            string type = category.ToString();
            bool canInflect = CanInflect(type);

            return canInflect;
        }

        public bool CanInflectSense(Sense sense)
        {
            bool canInflect = CanInflectCategory(sense.Category);
            return canInflect;
        }

        public bool CanDeinflect
        {
            get
            {
                return HasCapability("CanDeinflect");
            }
            set
            {
                SetCapability("CanDeinflect", value);
            }
        }

        public bool GetInflectorTableChecked(string type)
        {
            return HasCapability("InflectorTableChecked" + type);
        }

        public void SetInflectorTableChecked(string type, bool state)
        {
            SetCapability("InflectorTableChecked" + type, state);
        }

        public bool GetInflectionsLayoutChecked(string type, string layout)
        {
            return HasCapability("InflectionsLayoutChecked" + type + layout);
        }

        public void SetInflectionsLayoutChecked(string type, string layout, bool state)
        {
            SetCapability("InflectionsLayoutChecked" + type + layout, state);
        }

        public Dictionary<string, InflectorTable> InflectorTableDictionary
        {
            get
            {
                return _InflectorTableDictionary;
            }
            set
            {
                if (value != _InflectorTableDictionary)
                {
                    InflectorTableDictionary = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasInflectorTableDictionary(string type)
        {
            InflectorTable inflectorTable;

            if (_InflectorTableDictionary.TryGetValue(type, out inflectorTable))
                return true;

            return false;
        }

        public InflectorTable GetInflectorTable(string type)
        {
            InflectorTable inflectorTable;

            if (_InflectorTableDictionary == null)
                return null;

            if (_InflectorTableDictionary.TryGetValue(type, out inflectorTable))
                return inflectorTable;

            return null;
        }

        public void SetInflectorTable(InflectorTable inflectorTable)
        {
            string type = inflectorTable.Type;
            InflectorTable oldInflectorTable;

            if (_InflectorTableDictionary == null)
                _InflectorTableDictionary = new Dictionary<string, Language.InflectorTable>();

            if (_InflectorTableDictionary.TryGetValue(type, out oldInflectorTable))
                _InflectorTableDictionary[type] = inflectorTable;
            else
                _InflectorTableDictionary.Add(type, inflectorTable);
        }

        public InflectorTable InflectorTable(string type)
        {
            InflectorTable inflectorTable = null;

            if (CanInflect(type) &&
                ((inflectorTable = GetInflectorTable(type)) == null) &&
                !GetInflectorTableChecked(type))
            {
                inflectorTable = LoadInflectorTable(type);
                SetInflectorTableChecked(type, true);

                if (inflectorTable != null)
                {
                    SetInflectorTable(inflectorTable);
                    inflectorTable.ResolveInflectorTableCheck(this);
                }
            }

            return inflectorTable;
        }

        public void PrimeInflectorTables()
        {
            foreach (string type in LikelyInflectionTypes)
            {
                if (CanInflect(type))
                    InflectorTable(type);
            }

            if (DeinflectionDatabase.Count(LanguageID) != 0)
                DeinflectionDatabase.GetIndexed(0, LanguageID);
        }

        public string ComposeInflectorTableKey(string category)
        {
            return category + "_" + LanguageID.LanguageCode;
        }

        protected virtual InflectorTable LoadInflectorTable(string type)
        {
            string languageCode = LanguageID.LanguageCode;
            string key = ComposeInflectorTableKey(type);
            InflectorTable inflectorTable = null;
            Stream stream = null;

            if (_InflectorTableCache != null)
            {
                lock (_InflectorTableCache)
                {
                    if (_InflectorTableCache.TryGetValue(key, out inflectorTable))
                    {
                        if (inflectorTable != null)
                            return inflectorTable;
                    }
                }
            }

            string fileName = "InflectorTable_" + key + ".xml";
            string filePath = MediaUtilities.ConcatenateFilePath(
                ApplicationData.LocalDataPath,
                MediaUtilities.ConcatenateFilePath(
                    "InflectorTables",
                    MediaUtilities.ConcatenateFilePath(
                        languageCode,
                        fileName)));
            bool fileExists = FileSingleton.Exists(filePath);

            if (ApplicationData.IsMobileVersion)
            {
                if (!fileExists)
                {
                    string fileUrl = MediaUtilities.ConcatenateUrlPath(
                        ApplicationData.LocalDataTildeUrl,
                        MediaUtilities.ConcatenateUrlPath(
                            "InflectorTables",
                            MediaUtilities.ConcatenateUrlPath(
                                languageCode,
                                fileName)));

                    if ((ApplicationData.RemoteClient == null) || !ApplicationData.Global.IsConnectedToANetwork())
                        return null;

                    string errorMessage = String.Empty;

                    if (!ApplicationData.Global.GetRemoteMediaFile(fileUrl, filePath, ref errorMessage))
                        return null;

                    fileExists = FileSingleton.Exists(filePath);
                }
            }

            if (fileExists)
            {
                try
                {
                    stream = FileSingleton.OpenRead(filePath);

                    if (stream != null)
                    {
                        XElement inflectorTableElement = XElement.Load(stream);
                        inflectorTable = new InflectorTable(inflectorTableElement);
                    }
                    else
                    {
                        string message = "Error opening InflectorTable file: " +
                            filePath;
                        ApplicationData.Global.PutConsoleMessage(message);
                    }
                }
                catch (Exception exc)
                {
                    string message = "Exception while loading InflectorTable: " +
                        exc.Message;
                    if (exc.InnerException != null)
                        message += ":\n    " +
                            exc.InnerException.Message;
                    ApplicationData.Global.PutConsoleMessage(message);
                }
                finally
                {
                    if (stream != null)
                        FileSingleton.Close(stream);
                }
            }

            if (inflectorTable != null)
            {
                if (_InflectorTableCache == null)
                    _InflectorTableCache = new Dictionary<string, InflectorTable>();

                lock (_InflectorTableCache)
                {
                    _InflectorTableCache.Add(key, inflectorTable);
                }
            }

            return inflectorTable;
        }

        public InflectorDescription InflectorDescription(string type)
        {
            string filePath = GetInflectorDescriptionFilePath(type);
            string message;
            InflectorDescription inflectorDescription = LoadInflectorDescription(filePath, out message);
            return inflectorDescription;
        }

        public string GetInflectorDescriptionFilePath(string type)
        {
            string fileName = "InflectorDescription_" + type + "_" + LanguageID.LanguageCode + ".xml";
            string filePath = MediaUtilities.ConcatenateFilePath4(
                ApplicationData.LocalDataPath, "InflectorTables", LanguageID.LanguageCode, fileName);
            return filePath;
        }

        public InflectorDescription LoadInflectorDescription(string filePath, out string message)
        {
            InflectorDescription inflectorDescription = null;
            Stream stream = null;
            bool fileExists = FileSingleton.Exists(filePath);

            message = null;

            if (!fileExists)
            {
                message = "Inflector description file not found: " + filePath;
                return null;
            }

            try
            {
                stream = FileSingleton.OpenRead(filePath);

                if (stream != null)
                {
                    XElement inflectorDescriptionElement = XElement.Load(stream);
                    inflectorDescription = new InflectorDescription(inflectorDescriptionElement);
                }
                else
                    message = "Error opening InflectorDescription file: " + filePath;
            }
            catch (Exception exc)
            {
                message = "Exception while loading InflectorDescription: " + exc.Message;
                if (exc.InnerException != null)
                    message += ":\n    " +
                        exc.InnerException.Message;
            }
            finally
            {
                if (stream != null)
                    FileSingleton.Close(stream);
            }

            return inflectorDescription;
        }

        public bool SaveInflectorDescription(
            InflectorDescription inflectorDescription,
            string filePath,
            bool doBackup,
            out string message)
        {
            Stream stream = null;
            bool fileExists = FileSingleton.Exists(filePath);
            bool returnValue = false;

            message = null;

            if (fileExists && doBackup)
            {
                if (!MediaUtilities.BackupFile(filePath, out message))
                    return false;
            }

            try
            {
                stream = FileSingleton.OpenWrite(filePath);

                if (stream != null)
                {
                    XElement inflectorDescriptionElement = inflectorDescription.Xml;
                    inflectorDescriptionElement.Save(stream, SaveOptions.None);
                    returnValue = true;
                }
                else
                    message = "Error saving InflectorDescription file: " + filePath;
            }
            catch (Exception exc)
            {
                message = "Exception while saving InflectorDescription: " + exc.Message;
                if (exc.InnerException != null)
                    message += ":\n    " +
                        exc.InnerException.Message;
            }
            finally
            {
                if (stream != null)
                    FileSingleton.Close(stream);
            }

            return returnValue;
        }

        public static string[] LikelyInflectionTypes =
        {
            "Verb",
            "Noun",
            "Adjective",
            "Unknown"
        };

        public Designator GetDesignator(
            string type,
            string label)
        {
            InflectorTable inflectorTable = null;
            Designator designation = null;

            if (String.IsNullOrEmpty(type) || type == "Unknown")
            {
                foreach (string aType in LikelyInflectionTypes)
                {
                    inflectorTable = InflectorTable(aType.ToString());

                    if (inflectorTable != null)
                    {
                        designation = inflectorTable.GetDesignator("All", label);

                        if (designation != null)
                            return designation;

                        designation = inflectorTable.GetDesignator("Archaic", label);

                        if (designation != null)
                            return designation;

                        designation = GetExtendedDesignator(aType, label, "All", inflectorTable);

                        if (designation != null)
                            return designation;
                    }
                }

                return null;
            }

            inflectorTable = InflectorTable(type);

            if (inflectorTable == null)
                return null;

            designation = inflectorTable.GetDesignator("All", label);

            if (designation == null)
                designation = inflectorTable.GetDesignator("Archaic", label);

            if (designation == null)
                designation = GetExtendedDesignator(type, label, "All", inflectorTable);

            return designation;
        }

        public virtual Designator GetExtendedDesignator(
            string type,
            string label,
            string scope,
            InflectorTable inflectorTable)
        {
            Designator designation = null;
            return designation;
        }

        public Dictionary<string, InflectionsLayout> InflectionsLayoutDictionary
        {
            get
            {
                return _InflectionsLayoutDictionary;
            }
            set
            {
                if (value != _InflectionsLayoutDictionary)
                {
                    InflectionsLayoutDictionary = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ComposeCategoryLayoutKey(string category, string layout)
        {
            return category + "_" + layout + "_" + LanguageID.LanguageCode;
        }

        public bool HasInflectionsLayout(string category, string type)
        {
            string key = ComposeCategoryLayoutKey(category, type);
            InflectionsLayout inflectionsLayout;

            if (_InflectionsLayoutDictionary.TryGetValue(key, out inflectionsLayout))
                return true;

            return false;
        }

        public InflectionsLayout GetInflectionsLayout(string type, string layout)
        {
            InflectionsLayout inflectionsLayout;
            string key = ComposeCategoryLayoutKey(type, layout);

            if (_InflectionsLayoutDictionary.TryGetValue(key, out inflectionsLayout))
                return inflectionsLayout;

            return null;
        }

        public void SetInflectionsLayout(InflectionsLayout inflectionsLayout)
        {
            string type = inflectionsLayout.Type;
            string layout = inflectionsLayout.Layout;
            InflectionsLayout oldInflectionsLayout = GetInflectionsLayout(type, layout);
            string key = ComposeCategoryLayoutKey(type, layout);

            if (oldInflectionsLayout != inflectionsLayout)
                ModifiedFlag = true;

            if (oldInflectionsLayout != null)
                _InflectionsLayoutDictionary[key] = inflectionsLayout;
            else
                _InflectionsLayoutDictionary.Add(key, inflectionsLayout);
        }

        public InflectionsLayout InflectionsLayout(
            string type,
            string layout,
            LanguageUtilities languageUtilities)
        {
            InflectionsLayout inflectionsLayout = null;

            if (CanInflect(type) &&
                ((inflectionsLayout = GetInflectionsLayout(type, layout)) == null) &&
                !GetInflectionsLayoutChecked(type, layout))
            {
                inflectionsLayout = LoadInflectionsLayout(type, layout, languageUtilities);
                SetInflectionsLayoutChecked(type, layout, true);

                if (inflectionsLayout != null)
                    SetInflectionsLayout(inflectionsLayout);
            }

            return inflectionsLayout;
        }

        protected virtual InflectionsLayout LoadInflectionsLayout(
            string type,
            string layout,
            LanguageUtilities languageUtilities)
        {
            string languageCode = LanguageID.LanguageCode;
            string key = ComposeCategoryLayoutKey(type, layout);
            InflectionsLayout inflectionsLayout = null;

            if (_InflectionsLayoutCache != null)
            {
                if (_InflectionsLayoutCache.TryGetValue(key, out inflectionsLayout))
                {
                    if (inflectionsLayout != null)
                        return inflectionsLayout;
                }
            }

            inflectionsLayout = GetAutomatedInflectionsLayout(
                type,
                layout,
                languageUtilities);

            if (inflectionsLayout == null)
            {
                string fileName = "InflectionsLayout_" + key + ".xml";
                string filePath = MediaUtilities.ConcatenateFilePath(
                    ApplicationData.LocalDataPath,
                    MediaUtilities.ConcatenateFilePath(
                        "LayoutTables",
                        MediaUtilities.ConcatenateFilePath(
                            languageCode,
                            fileName)));
                bool fileExists = FileSingleton.Exists(filePath);

                if (ApplicationData.IsMobileVersion)
                {
                    if (!fileExists)
                    {
                        string fileUrl = MediaUtilities.ConcatenateUrlPath(
                        ApplicationData.LocalDataTildeUrl,
                        MediaUtilities.ConcatenateFilePath(
                            "LayoutTables",
                            MediaUtilities.ConcatenateFilePath(
                                languageCode,
                                fileName)));

                        if ((ApplicationData.RemoteClient == null) || !ApplicationData.Global.IsConnectedToANetwork())
                            return null;

                        string errorMessage = String.Empty;

                        if (!ApplicationData.Global.GetRemoteMediaFile(fileUrl, filePath, ref errorMessage))
                            return null;

                        fileExists = FileSingleton.Exists(filePath);
                    }
                }

                if (fileExists)
                {
                    try
                    {
                        Stream stream = FileSingleton.OpenRead(filePath);

                        if (stream != null)
                        {
                            XElement inflectionsLayoutElement = XElement.Load(stream);
                            inflectionsLayout = new InflectionsLayout(inflectionsLayoutElement);
                            InitializeInflectionsLayout(
                                type,
                                layout,
                                inflectionsLayout,
                                languageUtilities);
                        }
                        else
                        {
                            string message = "Error opening InflectionsLayout file: " +
                                filePath;
                            ApplicationData.Global.PutConsoleMessage(message);
                        }
                    }
                    catch (Exception exc)
                    {
                        string message = "Exception while loading InflectionsLayout: " +
                            exc.Message;
                        if (exc.InnerException != null)
                            message += ":\n    " +
                                exc.InnerException.Message;
                        ApplicationData.Global.PutConsoleMessage(message);
                    }
                }
            }

            if (inflectionsLayout != null)
            {
                if (_InflectionsLayoutCache == null)
                    _InflectionsLayoutCache = new Dictionary<string, InflectionsLayout>();

                _InflectionsLayoutCache.Add(key, inflectionsLayout);
            }

            return inflectionsLayout;
        }

        public virtual List<KeyValuePair<string, string>> GetInflectionsLayoutTypes(
            string category,
            LanguageUtilities languageUtilities)
        {
            string languageCode = LanguageID.LanguageCode;
            List<KeyValuePair<string, string>> layoutTypes = new List<KeyValuePair<string, string>>();
            string layoutTablesPath = MediaUtilities.ConcatenateFilePath(
                    ApplicationData.LocalDataPath,
                    MediaUtilities.ConcatenateFilePath(
                        "LayoutTables",
                        languageCode));
            List<string> availableFiles = null;

            try
            {
                if (FileSingleton.DirectoryExists(layoutTablesPath))
                    availableFiles = FileSingleton.GetFiles(layoutTablesPath);
            }
            catch (Exception)
            {
                availableFiles = null;
            }

            if (availableFiles != null)
            {
                foreach (string filePath in availableFiles)
                {
                    string fileName = MediaUtilities.GetFileName(filePath);
                    string[] parts = fileName.Split(LanguageLookup.FileNameKeySeparatorCharacters);

                    if (parts.Count() == 5)
                    {
                        string fileField = parts[0];
                        string categoryField = parts[1];
                        string typeField = parts[2];
                        string languageField = parts[3];
                        string fileExt = parts[4];

                        if (fileField != "InflectionsLayout")
                            continue;

                        if (categoryField != category)
                            continue;

                        if (languageField != languageCode)
                            continue;

                        if (fileExt != "xml")
                            continue;

                        layoutTypes.Add(
                            new KeyValuePair<string, string>(
                                typeField,
                                languageUtilities.TranslateUIString(typeField)));
                    }
                }
            }

            layoutTypes.Add(
                new KeyValuePair<string, string>(
                    "Automated",
                    languageUtilities.TranslateUIString("Automated")));

            return layoutTypes;
        }

        public virtual List<LanguageID> InflectionLanguageIDs
        {
            get
            {
                return new List<LanguageID>();
            }
        }

        public bool AllowArchaicInflections
        {
            get
            {
                return _AllowArchaicInflections;
            }
            set
            {
                _AllowArchaicInflections = value;
            }
        }

        public NodeUtilities NodeUtilities
        {
            get
            {
                return _NodeUtilities;
            }
            set
            {
                _NodeUtilities = value;
            }
        }

        public MultiLanguageTool MultiTool
        {
            get
            {
                return _MultiTool;
            }
            set
            {
                _MultiTool = value;
            }
        }
    }
}

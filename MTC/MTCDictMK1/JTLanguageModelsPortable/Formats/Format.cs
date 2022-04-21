using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public partial class Format : TaskUtilities
    {
        public string Name { get; set; }                // Format name.
        public string Type { get; set; }                // Format type.
        public string Description { get; set; }         // Format description;
        public string TargetType { get; set; }          // Target type.  This is a class name, i.e. BaseObjectNode.
        public string TargetLabel { get; set; }         // Target type.  This is a name like Course, Plan, Group, Lesson, Words, etc.
        public string Origin { get; set; }              // "UI", "Worker"
        public string Direction { get; set; }           // "Import", "Export"
        public string ImportExportType { get; set; }    // "File", "Text", "Web", "Content", "Dictionary", "Transfer" for in case the format needs this.
        public bool NoDataStreamExport { get; set; }    // For formats like Extract, that don't output data in the stream.
        public string MimeType { get; set; }            // Standard Mime type.
        public string DefaultFileExtension { get; set; }
        public string WebUrl { get; set; }
        public List<FormatArgument> Arguments { get; set; }
        public Dictionary<int, bool> NodeKeyFlags { get; set; }
        public Dictionary<string, bool> ContentKeyFlags { get; set; }
        public Dictionary<string, bool> ItemKeyFlags { get; set; }
        public Dictionary<string, bool> LanguageFlags { get; set; }
        public List<string> LanguageFlagNames { get; set; }
        public bool RecurseStudyItems { get; set; }
        public List<MultiLanguageItem> RecursedStudyItems { get; set; }
        public bool DeleteBeforeImport { get; set; }
        public bool DeleteBeforeExport { get; set; }
        public bool PreserveGuid { get; set; }
        public string TitlePrefix { get; set; }
        public string DefaultContentType { get; set; }
        public string DefaultContentSubType { get; set; }
        public string Label { get; set; }
        public string MasterName { get; set; }
        public NodeMaster Master { get; set; }
        public bool IsDoMerge { get; set; }
        public bool IsParagraphsOnly { get; set; }
        public bool IsFallbackToParagraphs { get; set; }
        public bool IsExcludePrior { get; set; }
        public bool SubDivide { get; set; }
        public bool SubDivideToStudyListsOnly { get; set; }
        public int StudyItemSubDivideCount { get; set; }
        public int MinorSubDivideCount { get; set; }
        public int MajorSubDivideCount { get; set; }
        private int ItemCounter { get; set; }
        private int LessonCounter { get; set; }
        private Dictionary<string, int> ItemCounters { get; set; }
        private Dictionary<string, int> LessonCounters { get; set; }
        public bool Sort { get; set; }
        public int Ordinal { get; set; }
        public bool PreserveTargetNames { get; set; }
        public bool MakeTargetPublic { get; set; }
        public string TargetMediaDirectory { get; set; }
        protected List<IBaseObject> Stack;
        protected bool DeleteMediaFiles { get; set; }
        public string MediaPath { get; set; }
        public bool IsIncludeMedia { get; set; }
        public bool IsLookupDictionaryAudio { get; set; }
        public bool IsLookupDictionaryPictures { get; set; }
        public bool IsSynthesizeMissingAudio { get; set; }
        public bool IsForceAudio { get; set; }
        public bool IsTranslateMissingItems { get; set; }
        public bool IsAddNewItemsToDictionary { get; set; }
        public List<LanguageDescriptor> LanguageDescriptors { get; set; }
        public List<LanguageID> TargetLanguageIDs { get; set; }
        public List<LanguageID> HostLanguageIDs { get; set; }
        public List<LanguageID> UniqueLanguageIDs { get; set; }
        public LanguageID UILanguageID { get; set; }
        public List<LanguageID> RomanizationLanguageIDs;
        public List<LanguageID> TargetRomanizationHostLanguageIDs;
        public List<LanguageID> HostTargetRomanizationLanguageIDs;
        public List<LanguageID> TargetRomanizationLanguageIDs;
        public bool IsFilterDuplicates { get; set; }
        public Dictionary<string, bool> AnchorLanguageFlags { get; set; }
        public int TitleCount { get; set; }
        public int ComponentIndex { get; set; }
        public int BaseIndex { get; set; }
        public int ItemIndex { get; set; }
        public int LineNumber { get; set; }
        public string SourceFileName { get; set; }
        public BaseObjectTitled TreeHeaderSource { get; set; }
        public BaseObjectNodeTree TreeSource { get; set; }
        public BaseObjectNode NodeSource { get; set; }
        public BaseObjectContent ContentSource { get; set; }
        public BaseObjectNodeTree Tree { get; set; }
        public IBaseObject Component { get; set; }
        public List<IBaseObject> Targets { get; set; }
        public List<IBaseObjectKeyed> AddObjects { get; set; }
        public List<IBaseObjectKeyed> UpdateObjects { get; set; }
        public List<IBaseObjectKeyed> BlockObjects { get; set; }
        public List<IBaseObject> ReadObjects { get; set; }
        public List<ContentStudyList> ReadStudyLists { get; set; }
        public FixupDictionary Fixups { get; set; }
        public UserRecord UserRecord { get; set; }
        public UserProfile UserProfile { get; set; }
        public IMainRepository Repositories { get; set; }
        public LanguageUtilities LanguageUtilities { get; set; }
        public NodeUtilities NodeUtilities { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public int ItemCount;
        private Dictionary<string, LanguageTool> LanguageToolCache;
        public bool AllowStudent { get; set; }
        public bool AllowTeacher { get; set; }
        public bool AllowAdministrator { get; set; }
        protected StudyItemCache PriorStudyItemCache;

        public Format(string name, string type, string description, string targetType, string importExportType,
            string mimeType, string defaultExtension, UserRecord userRecord, UserProfile userProfile,
            IMainRepository repositories, LanguageUtilities languageUtilities, NodeUtilities nodeUtilities)
        {
            ClearFormat();
            Name = name;
            Type = type;
            Description = description;
            TargetType = targetType;
            ImportExportType = importExportType;
            NoDataStreamExport = false;
            MimeType = mimeType;
            DefaultFileExtension = defaultExtension;
            WebUrl = String.Empty;
            UserRecord = userRecord;
            UserProfile = userProfile;
            Repositories = repositories;
            LanguageUtilities = languageUtilities;
            NodeUtilities = nodeUtilities;
            SetupLanguages();
        }

        public Format(Format other)
        {
            CopyFormat(other);
        }

        public Format()
        {
            ClearFormat();
        }

        public void ClearFormat()
        {
            Name = null;
            Type = null;
            Description = null;
            TargetType = null;
            TargetLabel = null;
            ImportExportType = null;
            NoDataStreamExport = false;
            MimeType = null;
            DefaultFileExtension = ".txt";
            WebUrl = String.Empty;
            Arguments = null;
            NodeKeyFlags = null;
            ContentKeyFlags = null;
            ItemKeyFlags = null;
            LanguageFlags = null;
            LanguageFlagNames = null;
            RecurseStudyItems = false;
            RecursedStudyItems = null;
            DeleteBeforeImport = false;
            DeleteBeforeExport = false;
            PreserveGuid = false;
            IsDoMerge = false;
            IsParagraphsOnly = false;
            IsFallbackToParagraphs = false;
            IsExcludePrior = false;
            TitlePrefix = "Default";
            DefaultContentType = "Words";
            DefaultContentSubType = "Vocabulary";
            Label = "Words";
            MasterName = "";
            Master = null;
            SubDivide = false;
            SubDivideToStudyListsOnly = true;
            StudyItemSubDivideCount = 20;
            MinorSubDivideCount = 5;
            MajorSubDivideCount = 5;
            ItemCounter = 0;
            LessonCounter = 0;
            ItemCounters = null;
            LessonCounters = null;
            Sort = false;
            Ordinal = 0;
            PreserveTargetNames = false;
            MakeTargetPublic = true;
            TargetMediaDirectory = null;
            Stack = null;
            DeleteMediaFiles = true;
            MediaPath = "";
            IsIncludeMedia = false;
            IsLookupDictionaryAudio = false;
            IsLookupDictionaryPictures = false;
            IsSynthesizeMissingAudio = false;
            IsForceAudio = false;
            IsTranslateMissingItems = false;
            IsAddNewItemsToDictionary = false;
            LanguageDescriptors = null;
            TargetLanguageIDs = null;
            HostLanguageIDs = null;
            UniqueLanguageIDs = null;
            UILanguageID = null;
            RomanizationLanguageIDs = null;
            TargetRomanizationHostLanguageIDs = null;
            HostTargetRomanizationLanguageIDs = null;
            TargetRomanizationLanguageIDs = null;
            IsFilterDuplicates = false;
            AnchorLanguageFlags = null;
            BaseIndex = 0;
            ItemIndex = 0;
            LineNumber = 0;
            SourceFileName = String.Empty;
            TreeHeaderSource = null;
            TreeSource = null;
            NodeSource = null;
            ContentSource = null;
            Component = null;
            Tree = null;
            Targets = null;
            AddObjects = null;
            UpdateObjects = null;
            BlockObjects = null;
            ReadObjects = null;
            ReadStudyLists = null;
            Fixups = null;
            UserRecord = null;
            UserProfile = null;
            Repositories = null;
            LanguageUtilities = null;
            NodeUtilities = null;
            Error = null;
            Message = null;
            ItemCount = 0;
            LanguageToolCache = null;
            AllowStudent = true;
            AllowTeacher = true;
            AllowAdministrator = true;
            PriorStudyItemCache = null;
        }

        public void CopyFormat(Format other)
        {
            Name = other.Name;
            Type = other.Type;
            Description = other.Description;
            TargetType = other.TargetType;
            TargetLabel = other.TargetLabel;
            ImportExportType = other.ImportExportType;
            MimeType = other.MimeType;
            DefaultFileExtension = other.DefaultFileExtension;
            WebUrl = other.WebUrl;
            if (other.Arguments != null)
            {
                Arguments = new List<FormatArgument>();
                foreach (FormatArgument argument in other.Arguments)
                {
                    FormatArgument newArgument = new FormatArgument(argument);
                    Arguments.Add(newArgument);
                }
            }
            else
                Arguments = null;
            NodeKeyFlags = other.NodeKeyFlags;
            ContentKeyFlags = other.ContentKeyFlags;
            ItemKeyFlags = other.ItemKeyFlags;
            LanguageFlags = other.LanguageFlags;
            LanguageFlagNames = other.LanguageFlagNames;
            RecurseStudyItems = other.RecurseStudyItems;
            RecursedStudyItems = other.RecursedStudyItems;
            DeleteBeforeImport = other.DeleteBeforeImport;
            DeleteBeforeExport = other.DeleteBeforeExport;
            PreserveGuid = other.PreserveGuid;
            IsDoMerge = other.IsDoMerge;
            IsParagraphsOnly = other.IsParagraphsOnly;
            IsFallbackToParagraphs = other.IsFallbackToParagraphs;
            IsExcludePrior = other.IsExcludePrior;
            IsExcludePrior = other.IsExcludePrior;
            TitlePrefix = other.TitlePrefix;
            DefaultContentType = other.DefaultContentType;
            DefaultContentSubType = other.DefaultContentSubType;
            Label = other.Label;
            MasterName = other.MasterName;
            Master = other.Master;
            SubDivide = other.SubDivide;
            SubDivideToStudyListsOnly = other.SubDivideToStudyListsOnly;
            StudyItemSubDivideCount = other.StudyItemSubDivideCount;
            MinorSubDivideCount = other.MinorSubDivideCount;
            MajorSubDivideCount = other.MajorSubDivideCount;
            ItemCounter = other.ItemCounter;
            LessonCounter = other.LessonCounter;
            ItemCounters = other.ItemCounters;
            LessonCounters = other.LessonCounters;
            Sort = other.Sort;
            Ordinal = other.Ordinal;
            PreserveTargetNames = other.PreserveTargetNames;
            MakeTargetPublic = other.MakeTargetPublic;
            TargetMediaDirectory = other.TargetMediaDirectory;
            Stack = null;
            DeleteMediaFiles = other.DeleteMediaFiles;
            MediaPath = other.MediaPath;
            IsIncludeMedia = other.IsIncludeMedia;
            IsLookupDictionaryAudio = other.IsLookupDictionaryAudio;
            IsLookupDictionaryPictures = other.IsLookupDictionaryPictures;
            IsSynthesizeMissingAudio = other.IsSynthesizeMissingAudio;
            IsForceAudio = other.IsForceAudio;
            IsTranslateMissingItems = other.IsTranslateMissingItems;
            IsAddNewItemsToDictionary = other.IsAddNewItemsToDictionary;
            LanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(other.LanguageDescriptors);
            TargetLanguageIDs = other.CloneTargetLanguageIDs();
            HostLanguageIDs = other.CloneHostLanguageIDs();
            UniqueLanguageIDs = LanguageID.CopyList(UniqueLanguageIDs);
            UILanguageID = other.UILanguageID;
            RomanizationLanguageIDs = other.CloneRomanizationLanguageIDs();
            TargetRomanizationHostLanguageIDs = other.CloneTargetRomanizationHostLanguageIDs();
            HostTargetRomanizationLanguageIDs = other.CloneHostTargetRomanizationLanguageIDs();
            TargetRomanizationLanguageIDs = other.CloneTargetRomanizationLanguageIDs();
            IsFilterDuplicates = other.IsFilterDuplicates;
            AnchorLanguageFlags = other.AnchorLanguageFlags;
            BaseIndex = other.BaseIndex;
            ItemIndex = other.ItemIndex;
            LineNumber = other.LineNumber;
            SourceFileName = other.SourceFileName;
            Tree = null;
            Component = null;
            if (other.Targets != null)
                Targets = new List<IBaseObject>(other.Targets);
            else
                Targets = null;
            if (other.AddObjects != null)
                AddObjects = new List<IBaseObjectKeyed>(other.AddObjects);
            else
                AddObjects = null;
            if (other.UpdateObjects != null)
                UpdateObjects = new List<IBaseObjectKeyed>(other.UpdateObjects);
            else
                UpdateObjects = null;
            if (other.BlockObjects != null)
                BlockObjects = new List<IBaseObjectKeyed>(other.BlockObjects);
            else
                BlockObjects = null;
            if (other.ReadObjects != null)
                ReadObjects = new List<IBaseObject>(other.ReadObjects);
            else
                ReadObjects = null;
            ReadStudyLists = null;
            Fixups = null;
            UserRecord = other.UserRecord;
            UserProfile = other.UserProfile;
            Repositories = other.Repositories;
            LanguageUtilities = other.LanguageUtilities;
            NodeUtilities = other.NodeUtilities;
            Error = null;
            Message = null;
            ItemCount = 0;
            LanguageToolCache = null;
            AllowStudent = other.AllowStudent;
            AllowTeacher = other.AllowTeacher;
            AllowAdministrator = other.AllowAdministrator;
            PriorStudyItemCache = null;
        }

        public virtual Format Clone()
        {
            return new Format(this);
        }

        public virtual void Read(Stream stream)
        {
            ItemCount = 0;
            LineNumber = 0;

            throw new ObjectException("Format: Read not implemented.");
        }

        protected virtual void PreRead(int progressCount)
        {
            if (Timer != null)
                Timer.Start();

            ContinueProgress(ProgressCountBase + progressCount);
        }

        protected virtual void PostRead()
        {
            UpdateTreeCheck();

            EndContinuedProgress();

            if (Timer != null)
                OperationTime = Timer.GetTimeInSeconds();

            if (!String.IsNullOrEmpty(Error))
                throw new Exception(Error);
        }

        public bool UpdateTreeCheck()
        {
            if ((Targets == null) && (Targets.Count() == 0))
                return true;

            bool returnValue = true;

            foreach (IBaseObject target in Targets)
            {
                if (target is BaseObjectContent)
                {
                    BaseObjectContent content = target as BaseObjectContent;
                    BaseObjectNodeTree tree = content.Tree;

                    if (tree != null)
                    {
                        if (tree.Modified)
                        {
                            tree.TouchAndClearModified();

                            if (!Repositories.UpdateReference(
                                    tree.Source,
                                    null,
                                    tree))
                                returnValue = false;
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual void Write(Stream stream)
        {
            ItemCount = 0;
            LineNumber = 0;
            throw new ObjectException("Format: Write not implemented.");
        }

        public virtual bool PermissionsCheck(UserRecord userRecord, ref string errorMessage)
        {
            bool returnValue = false;

            switch (userRecord.UserRole)
            {
                case "student":
                    returnValue = AllowStudent;
                    break;
                case "teacher":
                    returnValue = AllowTeacher;
                    break;
                case "administrator":
                    returnValue = AllowAdministrator;
                    break;
                default:
                    returnValue = false;
                    break;
            }

            if (!returnValue)
                errorMessage = "Sorry, you don't have a sufficient role to use this format.";

            return returnValue;
        }

        public string Owner
        {
            get
            {
                if (UserRecord != null)
                    return UserRecord.UserName;
                return String.Empty;
            }
        }

        public void InitializePriorStudyItemCache()
        {
            PriorStudyItemCache = new StudyItemCache();
        }

        public void ClearPriorStudyItemCache()
        {
            PriorStudyItemCache = null;
        }

        public bool SetSources(BaseObjectTitled treeHeader, BaseObjectNodeTree tree,
            BaseObjectNode node, BaseObjectContent content)
        {
            bool returnValue = true;

            TreeHeaderSource = treeHeader;
            TreeSource = tree;
            NodeSource = node;
            ContentSource = content;

            List<LanguageID> languageIDs;

            if (ContentSource != null)
                languageIDs = ContentSource.ExpandLanguageIDs(UserProfile);
            else if (NodeSource != null)
                languageIDs = NodeSource.ExpandLanguageIDs(UserProfile);
            else if (TreeSource != null)
                languageIDs = TreeSource.ExpandLanguageIDs(UserProfile);
            else if (TreeHeaderSource != null)
                languageIDs = TreeHeaderSource.ExpandLanguageIDs(UserProfile);
            else
            {
                languageIDs = UserProfile.LanguageIDs;
                returnValue = false;
            }

            SetLanguages(languageIDs);

            return returnValue;
        }

        public void SetLanguages(List<LanguageID> languageIDs)
        {
            if (languageIDs != null)
                LanguageFlagNames = LanguageID.GetLanguageCultureExtensionCodes(languageIDs);

            if (LanguageFlagNames != null)
                LanguageFlags = LanguageID.GetLanguageFlagsDictionaryFromStringList(LanguageFlagNames, false);

            if ((LanguageFlags != null) && (languageIDs != null))
            {
                foreach (LanguageID languageID in UserProfile.LanguageIDs)
                {
                    LanguageFlags[languageID.LanguageCultureExtensionCode] = true;
                }
            }
        }

        public void SetLanguages(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors != null)
                LanguageFlagNames = LanguageDescriptor.GetLanguageCultureExtensionCodes(languageDescriptors);

            if (LanguageFlagNames != null)
                LanguageFlags = LanguageDescriptor.GetLanguageFlagsDictionaryFromStringList(
                    LanguageFlagNames, languageDescriptors);
        }

        public List<LanguageID> GetLanguageIDsFromLanguageFlags()
        {
            List<LanguageID> languageIDs = new List<LanguageID>();
            foreach (KeyValuePair<string, bool> kvp in LanguageFlags)
            {
                if (kvp.Value)
                    languageIDs.Add(LanguageLookup.GetLanguageIDNoAdd(kvp.Key));
            }
            return languageIDs;
        }

        public List<LanguageID> CloneTargetLanguageIDs()
        {
            if (TargetLanguageIDs == null)
                return null;
            List<LanguageID> languageIDs = new List<LanguageID>(TargetLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneRomanizationLanguageIDs()
        {
            if (RomanizationLanguageIDs == null)
                return null;
            return new List<LanguageID>(RomanizationLanguageIDs);
        }

        public List<LanguageID> CloneHostLanguageIDs()
        {
            if (HostLanguageIDs == null)
                return null;
            return new List<LanguageID>(HostLanguageIDs);
        }

        public List<LanguageID> CloneTargetRomanizationHostLanguageIDs()
        {
            if (TargetRomanizationHostLanguageIDs == null)
                return null;
            List<LanguageID> languageIDs = new List<LanguageID>(TargetRomanizationHostLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneHostTargetRomanizationLanguageIDs()
        {
            if (HostTargetRomanizationLanguageIDs == null)
                return null;
            List<LanguageID> languageIDs = new List<LanguageID>(HostTargetRomanizationLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneTargetRomanizationLanguageIDs()
        {
            if (TargetRomanizationLanguageIDs == null)
                return null;
            List<LanguageID> languageIDs = new List<LanguageID>(TargetRomanizationLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneUniqueLanguageIDs()
        {
            if (UniqueLanguageIDs == null)
                return null;
            List<LanguageID> languageIDs = new List<LanguageID>(UniqueLanguageIDs);
            return languageIDs;
        }

        // Some arguments common to multiple formats.
        public static string TargetLanguageIDsHelp = "Select the target languages.";
        public static string RomanizationLanguageIDsHelp = "Select the romanized languages.";
        public static string HostLanguageIDsHelp = "Select the host languages.";
        public static string DeleteBeforeImportHelp = "Check this to delete prior content before importing.";
        public static string DeleteBeforeExportHelp = "Check this to delete prior content before exporting.";
        public static string PreserveTargetNamesHelp = "Check this to preserve the target title, description, and directory names."
            + " Check this if you want to ignore the names in the imported item."
            + " This is a way to avoid duplicate names if the item being imported already exists elsewhere.";
        public static string MakeTargetPublicHelp = "Check this to make imported targets public.";
        public static string IsFilterDuplicatesHelp = "Check this to filter out duplicates for anchored languages.";
        public static string AnchorLanguagesHelp = "Set languages that existing items will be matched to if filtering duplicates.";
        public static string IsDoMergeHelp = "If checked, will attempt to merge with existing study items."
            + " If unchecked, merge with anchored item, or append if no anchor.";
        public static string IsParagraphsOnlyHelp = "If checked, will exclude items appearing in earier lesson in the same content type.";
        public static string IsFallbackToParagraphsHelp = "If checked, will set sentence runs to the full paragraph is there is a sentence mismatch.";
        public static string IsExcludePriorHelp = "If checked, sill not to sentence runs. A paragraph will be a sentence run.";
        public static string IsTranslateMissingItemsHelp = "If checked, will translate missing items.";
        public static string IsAddNewItemsToDictionaryHelp = "If checked, will add translated items to the dictionary.";
        public static string IsIncludeMediaHelp = "If checked, will include media.";
        public static string IsLookupDictionaryAudioHelp = "If checked, will lookup missing audio in dictionary.";
        public static string IsLookupDictionaryPicturesHelp = "If checked, will lookup missing pictures in dictionary.";
        public static string IsSynthesizeMissingAudioHelp = "If checked, will synthesize missing audio, if not found in dictionary.";
        public static string IsForceAudioHelp = "If checked, will force synthesis of audio.";
        public static string WebUrlHelp = "Enter the web address URL to import from.";
        public static string TitlePrefixHelp = "Used only in importing tree or node members, for specifying a prefix for the created titles.";
        public static string DefaultContentTypeHelp = "Used only in importing tree or node members, for specifying which content to select/create.";
        public static string DefaultContentSubTypeHelp = "Used only in importing tree or node members, for specifying which content to select/create.";
        public static string MasterNameHelp = "Used only in node members, for specifying which node master to use.";
        public static string SubDivideHelp = "If checked, will subdivide big study lists into smaller ones.";
        public static string SubDivideToStudyListsOnlyHelp = "If checked, will subdivide into child studylists only."
            + " Otherwise lessons and group nodes will be created for the subdivided study lists.";
        public static string StudyItemSubDivideCountHelp = "Maximum number of study items in a leaf content.";
        public static string MinorSubDivideCountHelp = "Maximum number of lessons or leaf parent contents before subdividing.";
        public static string MajorSubDivideCountHelp = "Maximum number of groups or leaf grandparent contents before subdividing.";

        public virtual void PrefetchArguments(FormReader formReader)
        {
        }

        public virtual void LoadFromArguments()
        {
        }

        public virtual void SaveToArguments()
        {
        }

        public virtual string GetArgumentKeyName(string name)
        {
            string keyName = Type + "." + TargetType + "." + name;
            return keyName;
        }

        public string GetUserOptionString(string name, string defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserProfile != null)
                return UserProfile.GetUserOptionString(keyName, defaultValue);
            return defaultValue;
        }

        public int GetUserOptionInteger(string name, int defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserProfile != null)
                return UserProfile.GetUserOptionInteger(keyName, defaultValue);
            return defaultValue;
        }

        public bool GetUserOptionFlag(string name, bool defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserProfile != null)
                return (UserProfile.GetUserOptionString(keyName, (defaultValue ? "on" : "off")) == "on");
            return defaultValue;
        }

        public FormatArgument FindArgument(string name)
        {
            if (Arguments == null)
                return null;
            FormatArgument argument = Arguments.FirstOrDefault(x => x.Name == name);
            return argument;
        }

        public FormatArgument FindOrCreateArgument(string name, string type, string direction, string value,
            List<object> values, string label, string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, flagOnDependents, flagOffDependents);
                argument.Values = values;
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            return argument;
        }

        public string GetArgument(string name)
        {
            FormatArgument argument = FindArgument(name);

            if (argument != null)
                return argument.Value;

            return "";
        }

        public string GetArgumentDefaulted(string name, string type, string direction, string defaultValue, string label, string help)
        {
            if (UserProfile != null)
            {
                string profileValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValue);

                if (!String.IsNullOrEmpty(profileValue))
                    defaultValue = profileValue;
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue, null, label, help, null, null);

            if (argument != null)
                return argument.Value;

            return defaultValue;
        }

        public List<string> GetArgumentStringListDefaulted(string name, string type, string direction, List<string> defaultValue, string label, string help)
        {
            string stringValue = TextUtilities.GetStringFromStringList(defaultValue);

            if (UserProfile != null)
            {
                string profileValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), stringValue);

                if (!String.IsNullOrEmpty(profileValue))
                    stringValue = profileValue;
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, stringValue, null, label, help, null, null);

            if (argument != null)
            {
                stringValue = argument.Value;
                List<string> stringList = TextUtilities.GetStringListFromStringDelimited(stringValue, "\r\n,");
                int c = stringList.Count();
                int i;
                for (i = c - 1; i >= 0; i--)
                {
                    string s = stringList[i].Trim();
                    if (String.IsNullOrEmpty(s))
                        stringList.RemoveAt(i);
                    else
                        stringList[i] = s;
                }
                return stringList;
            }

            return defaultValue;
        }

        public int GetIntegerArgumentDefaulted(string name, string type, string direction, int defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionInteger(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue.ToString(), null, label, help, null, null);

            int value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                try
                {
                    value = Convert.ToInt32(argument.Value);
                }
                catch (Exception)
                {
                }
            }

            return value;
        }

        public float GetFloatArgumentDefaulted(string name, string type, string direction, float defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionFloat(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue.ToString(), null, label, help, null, null);

            float value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                try
                {
                    value = (float)Convert.ToDouble(argument.Value);
                }
                catch (Exception)
                {
                }
            }

            return value;
        }

        public bool GetFlagArgumentDefaulted(string name, string type, string direction, bool defaultValue, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            if (UserProfile != null)
            {
                string key = GetArgumentKeyName(name);

                if (UserProfile.HasUserOption(key))
                    defaultValue = (UserProfile.GetUserOptionString(GetArgumentKeyName(name), (defaultValue ? "on" : "off")) == "on");
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue ? "on" : "off", null, label,
                help, flagOnDependents, flagOffDependents);

            bool value = defaultValue;

            if ((argument != null) && (argument.Value != null))
            {
                if (String.IsNullOrEmpty(argument.Value))
                    value = false;
                else
                    value = ObjectUtilities.GetBoolFromString(argument.Value, defaultValue);
            }

            return value;
        }

        public Dictionary<string, bool> GetFlagListArgumentDefaulted(string name, string type, string direction,
            Dictionary<string, bool> defaultValue, List<string> values,
            string label, string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string defaultValueString = String.Empty;

            if (defaultValue != null)
                defaultValueString = TextUtilities.GetStringFromFlagDictionary(defaultValue);

            List<object> valuesObjectList = null;

            if (values != null)
                valuesObjectList = values.Cast<object>().ToList();

            //if (UserProfile != null)
            //    defaultValueString = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValueString);

            FormatArgument argument = FindOrCreateArgument(name, type, direction,
                defaultValueString, valuesObjectList, label,
                help, flagOnDependents, flagOffDependents);

            Dictionary<string, bool> value = defaultValue;

            if ((argument != null) && (argument.Value != null))
                value = TextUtilities.GetFlagDictionaryFromString(argument.Value, null);

            return value;
        }

        public LanguageID GetLanguageIDArgumentDefaulted(string name, string type, string direction, LanguageID defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionLanguageID(GetArgumentKeyName(name), defaultValue);

            string defaultString = (defaultValue != null ? defaultValue.ToString() : String.Empty);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultString, null, label, help, null, null);

            LanguageID value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
                value = LanguageLookup.GetLanguageIDNoAdd(argument.Value);

            return value;
        }

        public List<LanguageID> GetLanguageIDListArgumentDefaulted(string name, string type, string direction, List<LanguageID> defaultValue, string label, string help)
        {
            if (UserProfile != null)
                defaultValue = UserProfile.GetUserOptionLanguageIDList(GetArgumentKeyName(name), defaultValue);

            string stringValue = TextUtilities.GetStringFromLanguageIDList(defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, stringValue,
                null, label, help, null, null);

            List<LanguageID> value;

            if (argument != null)
                value = TextUtilities.GetLanguageIDListFromString(argument.Value);
            else
                value = defaultValue;

            return value;
        }

        public string GetStringListArgumentDefaulted(string name, string type, string direction, string defaultValue,
            List<string> stringValues, string label, string help)
        {
            if (UserProfile != null)
            {
                string testValue = UserProfile.GetUserOptionString(GetArgumentKeyName(name), defaultValue);

                if (!String.IsNullOrEmpty(testValue))
                    defaultValue = testValue;
            }

            List<object> values = null;

            if (stringValues != null)
            {
                values = new List<object>();

                foreach (string stringValue in stringValues)
                {
                    string translatedString;
                    if (LanguageUtilities != null)
                        translatedString = LanguageUtilities.TranslateUIString(stringValue);
                    else
                        translatedString = stringValue;
                    values.Add(new BaseString(stringValue, translatedString));
                }
            }

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue, values, label, help, null, null);

            string value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                value = argument.Value;

                if (!stringValues.Contains(value))
                    value = "(invalid)";
            }

            return value;
        }

        public string GetMasterListArgumentDefaulted(string name, string type, string direction, string defaultValue,
            string label, string help)
        {
            List<string> masterList = GetMasterStringList(defaultValue);
            return GetStringListArgumentDefaulted(name, type, direction, defaultValue,
                masterList, label, help);
        }

        public FormatArgument SetArgument(string name, string type, string direction, string value, string label, string help,
            List<string> flagOnDependents, List<string> flagOffDependents)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, flagOnDependents, flagOffDependents);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = value;

            return argument;
        }

        public FormatArgument SetArgumentStringList(string name, string type, string direction, List<string> value, string label, string help,
            List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string stringValue = TextUtilities.GetStringFromStringListDelimited(value, "\r\n");
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, stringValue, label, help, flagOnDependents, flagOffDependents);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = stringValue;

            return argument;
        }

        public FormatArgument SetIntegerArgument(string name, string type, string direction, int value, string label, string help)
        {
            return SetArgument(name, type, direction, value.ToString(), label, help, null, null);
        }

        public FormatArgument SetFloatArgument(string name, string type, string direction, float value, string label, string help)
        {
            return SetArgument(name, type, direction, value.ToString(), label, help, null, null);
        }

        public FormatArgument SetFlagArgument(string name, string type, string direction, bool value, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            return SetArgument(name, type, direction, (value ? "on" : "off"), label, help, flagOnDependents, flagOffDependents);
        }

        public FormatArgument SetFlagListArgument(string name, string type, string direction,
            Dictionary<string, bool> value, List<string> values, string label,
            string help, List<string> flagOnDependents, List<string> flagOffDependents)
        {
            string valueString = String.Empty;

            if (value != null)
                valueString = TextUtilities.GetStringFromFlagDictionary(value);

            List<object> valuesObjectList = null;

            if (values != null)
                valuesObjectList = values.Cast<object>().ToList();

            FormatArgument argument = SetArgument(name, type, direction, valueString, label, help, flagOnDependents, flagOffDependents);

            if (argument != null)
                argument.Values = valuesObjectList;

            return argument;
        }

        public FormatArgument SetLanguageIDArgument(string name, string type, string direction, LanguageID value, string label, string help)
        {
            string stringValue;
            if (value != null)
                stringValue = value.LanguageCultureExtensionCode;
            else
                stringValue = "(any)";
            return SetArgument(name, type, direction, stringValue, label, help, null, null);
        }

        public FormatArgument SetLanguageIDListArgument(string name, string type, string direction, List<LanguageID> value, string label, string help)
        {
            string stringValue = TextUtilities.GetStringFromLanguageIDList(value);
            return SetArgument(name, type, direction, stringValue, label, help, null, null);
        }

        public FormatArgument SetStringListArgument(string name, string type, string direction, string value, List<string> stringValues,
            string label, string help)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help, null, null);
                List<object> values = null;
                if (stringValues != null)
                {
                    values = new List<object>();
                    foreach (string stringValue in stringValues)
                    {
                        string translatedString;
                        if (LanguageUtilities != null)
                            translatedString = LanguageUtilities.TranslateUIString(stringValue);
                        else
                            translatedString = stringValue;
                        values.Add(new BaseString(stringValue, translatedString));
                    }
                }
                argument.Values = values;
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = value;

            return argument;
        }

        public FormatArgument SetMasterListArgument(string name, string type, string direction, string value,
            string label, string help)
        {
            List<string> masterList = GetMasterStringList(value);
            return SetStringListArgument(name, type, direction, value, masterList, label, help);
        }

        public List<string> GetMasterStringList(string currentMasterName)
        {
            List<NodeMaster> masters = Repositories.NodeMasters.GetList(UserRecord.UserName);

            if (masters == null)
                masters = new List<NodeMaster>();

            List<string> masterStrings = new List<string>();

            foreach (NodeMaster master in masters)
                masterStrings.Add(master.GetTitleString(UILanguageID));

            masterStrings.Add("(none)");

            if (!String.IsNullOrEmpty(currentMasterName) && !masterStrings.Contains(currentMasterName))
                masterStrings.Add(currentMasterName);

            return masterStrings;
        }

        public virtual void CopyArgumentValues(Format other)
        {
            if (Arguments == null)
                return;

            foreach (FormatArgument argument in Arguments)
            {
                FormatArgument otherArgument = other.FindArgument(argument.Name);

                if (otherArgument != null)
                    argument.Value = otherArgument.Value;
            }
        }

        public virtual void SaveUserOptions()
        {
            if (UserProfile == null)
                return;

            if (Arguments == null)
                return;

            foreach (FormatArgument argument in Arguments)
                UserProfile.SetUserOptionString(GetArgumentKeyName(argument.Name), argument.Value);
        }

        public void AddTarget(IBaseObject target)
        {
            if (Targets == null)
                Targets = new List<IBaseObject>() { target };
            else
                Targets.Add(target);
        }

        public void AddTargets(List<IBaseObject> targets)
        {
            if (Targets == null)
                Targets = new List<IBaseObject>(targets);
            else
                Targets.AddRange(targets);
        }

        public virtual void DeleteFirst()
        {
            ItemCount = 0;
            DeleteTargetsFirst();
        }

        public virtual void DeleteTargetsFirst()
        {
            if (Targets != null)
            {
                bool deleteMediaFiles = DeleteMediaFiles;

                foreach (IBaseObject target in Targets)
                {
                    if (target is BaseObjectTitled)
                    {
                        BaseObjectTitled titledObject = target as BaseObjectTitled;

                        if (titledObject is BaseObjectContent)
                        {
                            BaseObjectContent content = titledObject as BaseObjectContent;
                            NodeUtilities.DeleteContentHelper(content, deleteMediaFiles);
                            NodeUtilities.UpdateTreeCheck(content.Tree, false, false);
                        }
                        else if (titledObject is BaseObjectNodeTree)
                        {
                            BaseObjectNodeTree tree = titledObject as BaseObjectNodeTree;
                            NodeUtilities.DeleteTreeChildrenHelper(tree, deleteMediaFiles);
                            NodeUtilities.UpdateTreeCheck(tree, false, false);
                        }
                        else if (titledObject is BaseObjectNode)
                        {
                            BaseObjectNode node = titledObject as BaseObjectNode;
                            NodeUtilities.DeleteNodeChildrenAndContentHelper(node.Tree, node, deleteMediaFiles);
                            NodeUtilities.UpdateTreeCheck(node.Tree, false, false);
                        }
                    }
                }
            }
        }

        // Save a repository object.
        public virtual void SaveObject(IBaseObject obj)
        {
            if (obj == null)
                throw new ObjectException(FormatErrorPrefix() + "Format.SaveObject: Null object.");

            IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;
            string source;

            if (keyedObject != null)
            {
                if (!String.IsNullOrEmpty(source = keyedObject.Source) && (source != "Nodes"))
                {
                    if (AddFixupCheck(keyedObject))
                    {
                        keyedObject.ResetKeyNoModify();

                        if (!PreserveGuid)
                            keyedObject.NewGuid();
                        else
                            keyedObject.EnsureGuid();
                    }

                    if (keyedObject.CreationTime == DateTime.MinValue)
                        keyedObject.TouchAndClearModified();
                    else
                        keyedObject.Modified = false;

                    if (!Repositories.SaveReference(keyedObject.Source, null, keyedObject))
                        throw new ObjectException(FormatErrorPrefix() + "Error saving read object: " + keyedObject.Name);

                    if (obj is BaseObjectNodeTree)
                    {
                        BaseObjectNodeTree tree = obj as BaseObjectNodeTree;
                        string treeHeaderSource = tree.Label + "Headers";
                        ObjectReferenceNodeTree treeReference = new ObjectReferenceNodeTree(tree.Source, tree);
                        treeReference.ModifiedTime = tree.ModifiedTime;
                        treeReference.Modified = false;

                        if (!Repositories.SaveReference(treeHeaderSource, null, treeReference))
                            throw new ObjectException(FormatErrorPrefix() + "Error saving tree reference: " + tree.Name);
                    }
                }
                else if ((keyedObject is BaseObjectNode) && (keyedObject.KeyInt <= 0))
                {
                    BaseObjectNode node = keyedObject as BaseObjectNode;

                    node.TouchAndClearModified();

                    if (Tree != null)
                    {
                        node.EnsureGuid();
                        Tree.AddNode(node);
                    }
                }
                else if (keyedObject is DictionaryEntry)
                {
                    AddDictionaryEntry(keyedObject as DictionaryEntry);
                    return;
                }
            }
            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>() { obj };
            else
                ReadObjects.Add(obj);
        }

        // Save a reference repository object.
        public virtual void SaveReferenceObject(IBaseObjectKeyed obj)
        {
            if (obj == null)
                throw new ObjectException(FormatErrorPrefix() + "Format.SaveReferenceObject: Null object.");

            string source = obj.Source;

            if (!String.IsNullOrEmpty(source) && (source != "Nodes"))
            {
                if (Fixups != null)
                    Fixups.Add(obj);

                if (obj.Guid == Guid.Empty)
                {
                    obj.EnsureGuid();
                    obj.Modified = false;
                }

                if (!Repositories.SaveGuidReference(source, null, obj, false))
                    throw new ObjectException(FormatErrorPrefix() + "Error saving reference type \"" + obj.Name + "\" guid \"" + obj.Guid.ToString() + "\".");
            }
            else
                throw new ObjectException(FormatErrorPrefix() + "Objects which are not stored cannot be references: " + obj.Name);

            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>() { obj };
            else
                ReadObjects.Add(obj);
        }

        // Save a non-repository object.
        public virtual void SaveNonSavedObject(IBaseObjectKeyed obj)
        {
            if (obj == null)
                throw new ObjectException(FormatErrorPrefix() + "Format.SaveNonSavedObject: Null object.");

            // We need to do this now so that the key will be correct when the fixups are done.
            if ((obj is BaseObjectNode) && (Tree != null))
            {
                if (Fixups != null)
                    Fixups.Add(obj, "Nodes");

                BaseObjectNode node = obj as BaseObjectNode;
                Tree.AddNode(node);
            }
            else if (Fixups != null)
                Fixups.Add(obj);

            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>() { obj };
            else
                ReadObjects.Add(obj);
        }

        protected virtual void CollectReferences(
            IBaseObject obj,
            List<IBaseObjectKeyed> references,
            List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren,
            Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags,
            Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags)
        {
            obj.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                intSelectFlags, stringSelectFlags, itemSelectFlags, languageSelectFlags, null, null);
        }

        protected virtual void CollectMediaFiles(IBaseObject obj, List<string> mediaFiles)
        {
            obj.CollectReferences(null, null, null, NodeKeyFlags, ContentKeyFlags, ItemKeyFlags,
                LanguageFlags, mediaFiles, null);
        }

        protected virtual void CollectNodeImageFiles(IBaseObject obj, List<string> mediaFiles)
        {
        }

        // For relative paths.
        protected virtual void PostProcessMediaFiles(string mediaDir, List<string> mediaFiles)
        {
            switch (TargetType)
            {
                case "DictionaryEntry":
                    if (ReadObjects != null)
                    {
                        List<LanguageID> languageIDs = GetLanguageIDsFromLanguageFlags();
                        string errorMessage = String.Empty;
                        bool returnValue = true;
                        foreach (IBaseObjectKeyed obj in ReadObjects)
                        {
                            DictionaryEntry dictionaryEntry = obj as DictionaryEntry;
                            if (dictionaryEntry != null)
                                returnValue = dictionaryEntry.MediaCheck(languageIDs, ref errorMessage) && returnValue;
                        }
                        if (!returnValue)
                        {
                            Error = errorMessage;
                        }
                    }
                    break;
                default:
                    break;
            }

            LazyConvertMediaFiles(mediaDir, mediaFiles);
        }

        // For absolute paths.
        protected virtual void PostProcessMediaFiles(List<string> mediaFiles)
        {
            switch (TargetType)
            {
                case "DictionaryEntry":
                    if (ReadObjects != null)
                    {
                        List<LanguageID> languageIDs = GetLanguageIDsFromLanguageFlags();
                        string errorMessage = String.Empty;
                        bool returnValue = true;
                        foreach (IBaseObjectKeyed obj in ReadObjects)
                        {
                            DictionaryEntry dictionaryEntry = obj as DictionaryEntry;
                            if (dictionaryEntry != null)
                                returnValue = dictionaryEntry.MediaCheck(languageIDs, ref errorMessage) && returnValue;
                        }
                        if (!returnValue)
                        {
                            Error = errorMessage;
                        }
                    }
                    break;
                default:
                    break;
            }

            LazyConvertMediaFiles(mediaFiles);
        }

        // For relative paths.
        protected virtual void LazyConvertMediaFiles(string mediaDir, List<string> mediaFiles)
        {
            foreach (string mediaFile in mediaFiles)
            {
                string mimeType = MediaUtilities.GetMimeTypeFromFileName(mediaFile);
                string mediaFilePath = mediaDir + mediaFile;

                try
                {
                    if (!FileSingleton.Exists(mediaFilePath))
                    {
                        continue;
                    }

                    string message;
                    MediaConvertSingleton.LazyConvert(mediaFilePath, mimeType, false, out message);
                }
                catch (Exception)
                {
                }
            }
        }

        // For absolute paths.
        protected virtual void LazyConvertMediaFiles(List<string> mediaFiles)
        {
            foreach (string mediaFile in mediaFiles)
            {
                string mimeType = MediaUtilities.GetMimeTypeFromFileName(mediaFile);

                try
                {
                    if (!FileSingleton.Exists(mediaFile))
                    {
                        continue;
                    }

                    string message;
                    MediaConvertSingleton.LazyConvert(mediaFile, mimeType, false, out message);
                }
                catch (Exception)
                {
                }
            }
        }

        public bool WriteToTemporary(Stream stream, string filePath)
        {
            try
            {
                if (!FileSingleton.DirectoryExistsCheck(filePath))
                    return false;

                Stream outStream = FileSingleton.OpenWrite(filePath);

                stream.CopyTo(outStream);

                FileSingleton.Close(outStream);
            }
            catch (Exception exc)
            {
                string msg = "Exception during WriteToTemporary: " + exc.Message;
                if (exc.InnerException != null)
                    msg += exc.InnerException.Message;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }

            return true;
        }

        public bool AddFixupCheck(IBaseObjectKeyed obj)
        {
            if (obj != null)
            {
                string source = obj.Source;

                if (String.IsNullOrEmpty(source) || (source == "Nodes"))
                {
                    if (obj is BaseObjectNode)
                    {
                        if (Fixups != null)
                            Fixups.Add(obj, "Nodes");
                    }
                }
                else if (Repositories.IsGenerateKeys(source))
                {
                    if (ApplicationData.IsMobileVersion)
                    {
#if false
                        bool doUpdate = false;

                        if (obj is BaseContentStorage)
                        {
                            (obj as BaseContentStorage).RemoteKey = obj.KeyInt;
                            doUpdate = true;
                        }
                        else if (obj is BaseObjectNodeTree)
                        {
                            (obj as BaseObjectNodeTree).RemoteKey = obj.KeyInt;
                            doUpdate = true;
                        }

                        if (doUpdate && obj.Modified)
                        {
                            obj.Modified = false;
                            Repositories.UpdateReference(source, null, obj);
                        }
#else
                        if (obj is BaseContentStorage)
                            (obj as BaseContentStorage).RemoteKey = obj.KeyInt;
                        else if (obj is BaseObjectNodeTree)
                            (obj as BaseObjectNodeTree).RemoteKey = obj.KeyInt;
#endif
                    }

                    if (Fixups != null)
                        Fixups.Add(obj);

                    return true;
                }
            }

            return false;
        }

        public void StartFixups()
        {
            Fixups = new FixupDictionary(Repositories);
        }

        public void EndFixups()
        {
            if (Fixups != null)
            {
                Fixups.DoFixups();
                Fixups = null;
            }
        }

        public void FixupObject(IBaseObjectKeyed obj)
        {
            if (obj == null)
                return;

            if (Fixups == null)
                return;

            obj.OnFixup(Fixups);
        }

        public T GetComponent<T>() where T : BaseObjectKeyed
        {
            return Component as T;
        }

        protected string GetComponentType()
        {
            string componentType;

            if (Component is BaseObjectTitled)
            {
                BaseObjectTitled titledObject = GetComponent<BaseObjectTitled>();

                if (titledObject is BaseObjectContent)
                {
                    BaseObjectContent content = GetComponent<BaseObjectContent>();
                    componentType = "BaseObjectContent";
                }
                else if (titledObject is BaseObjectNode)
                {
                    BaseObjectNode node = titledObject as BaseObjectNode;

                    if (node.IsTree())
                        componentType = "BaseObjectNodeTree";
                    else
                        componentType = "BaseObjectNode";
                }
                else
                    componentType = Component.GetType().Name;
            }
            else if (Component != null)
                componentType = Component.GetType().Name;
            else
                componentType = TargetType;

            return componentType;
        }

        protected void PushComponent(string componentType, string contentType, string contentSubType,
            MultiLanguageString title, MultiLanguageString description,
            NodeMasterReference masterReference, MarkupTemplate markupTemplate,
            MarkupTemplateReference markupReference, List<IBaseObjectKeyed> options)
        {
            BaseObjectNodeTree tree = Tree;
            BaseObjectNode parentNode = null;
            BaseObjectNode node = null;
            BaseObjectContent parentContent = null;
            BaseObjectContent content = null;
            IBaseObject oldComponent = Component;
            string titleString = (title != null ? title.Text(HostLanguageID) : String.Empty);
            bool doAdd = true;

            switch (componentType)
            {
                case "BaseObjectNodeTree":
                    if (!DeleteBeforeImport)
                    {
                        ObjectReferenceNodeTree testTreeReference = Repositories.ResolveNamedReference(
                            contentType + "Headers", null, UserRecord.UserName, titleString) as ObjectReferenceNodeTree;
                        BaseObjectNodeTree testTree = null;
                        if (testTreeReference != null)
                            testTree = Repositories.ResolveReference(contentType + "s", null, testTreeReference.Key) as BaseObjectNodeTree;
                        if (testTree != null)
                        {
                            tree = testTree;
                            doAdd = false;
                        }
                        else
                            tree = NodeUtilities.CreateTree(UserRecord, UserProfile, contentType, contentType + "s");
                    }
                    else
                        tree = NodeUtilities.CreateTree(UserRecord, UserProfile, contentType, contentType + "s");
                    Tree = tree;
                    Component = Tree;
                    tree.IsPublic = MakeTargetPublic;
                    SetComponentTypesAndTitle(componentType, contentType, contentSubType, title, description);
                    if (markupTemplate != null)
                        tree.LocalMarkupTemplate = markupTemplate;
                    if (markupReference != null)
                        tree.MarkupReference = markupReference;
                    if (options != null)
                        tree.Options = options;
                    if (masterReference != null)
                    {
                        tree.MasterReference = masterReference;
                        if (tree.Master != null)
                            NodeUtilities.SetupNodeFromMaster(tree);
                    }
                    if (doAdd)
                    {
                        if (!NodeUtilities.AddTree(Tree, true))
                            throw new Exception(FormatErrorPrefix() + "Error adding tree.");
                    }
                    break;
                case "BaseObjectNode":
                    if (GetComponentType() == "BaseObjectNode")
                        parentNode = GetComponent<BaseObjectNode>();
                    if (!DeleteBeforeImport)
                    {
                        BaseObjectNode testNode;
                        if (parentNode != null)
                            testNode = parentNode.FindChild(titleString);
                        else
                            testNode = tree.FindChild(titleString);
                        if (testNode != null)
                        {
                            node = testNode;
                            doAdd = false;
                        }
                    }
                    if (node == null)
                    {
                        node = NodeUtilities.CreateNode(UserRecord, UserProfile, Tree, parentNode, contentType);
                        tree.AddNode(parentNode, node);
                    }
                    node.IsPublic = MakeTargetPublic;
                    Component = node;
                    SetComponentTypesAndTitle(componentType, contentType, contentSubType, title, description);
                    if (markupTemplate != null)
                        node.LocalMarkupTemplate = markupTemplate;
                    if (markupReference != null)
                        node.MarkupReference = markupReference;
                    if (options != null)
                        node.Options = options;
                    if (masterReference != null)
                    {
                        node.MasterReference = masterReference;
                        if (node.Master != null)
                            NodeUtilities.SetupNodeFromMaster(node);
                    }
                    break;
                case "BaseObjectContent":
                    switch (GetComponentType())
                    {
                        case "BaseObjectNodeTree":
                            node = tree;
                            break;
                        case "BaseObjectNode":
                            node = GetComponent<BaseObjectNode>();
                            break;
                        case "BaseObjectContent":
                            parentContent = GetComponent<BaseObjectContent>();
                            node = parentContent.Node;
                            break;
                        default:
                            break;
                    }
                    if (!DeleteBeforeImport)
                    {
                        string nodeContentKey = MasterContentItem.ComposeKey(null, contentType, contentSubType, UserProfile, false);
                        if (parentContent != null)
                            content = parentContent.GetContent(nodeContentKey);
                        else
                            content = node.GetContent(nodeContentKey);
                    }
                    if (content == null)
                    {
                        content = NodeUtilities.CreateContent(node, parentContent, contentType, contentSubType);
                        if (parentContent != null)
                            parentContent.AddContentChild(content);
                        else if (node != null)
                            node.AddContentChild(content);
                    }
                    Component = content;
                    ItemIndex = 0;
                    content.IsPublic = MakeTargetPublic;
                    SetComponentTypesAndTitle(componentType, null, null, title, description);
                    BaseContentStorage contentStorage = content.ContentStorage;
                    if (contentStorage != null)
                    {
                        if (markupTemplate != null)
                            contentStorage.LocalMarkupTemplate = markupTemplate;
                        if (markupReference != null)
                            contentStorage.MarkupReference = markupReference;
                    }
                    else
                    {
                        if (markupTemplate != null)
                            content.LocalMarkupTemplate = markupTemplate;
                        if (markupReference != null)
                            content.MarkupReference = markupReference;
                    }
                    if (options != null)
                        content.Options = options;
                    break;
                default:
                    throw new Exception(FormatErrorPrefix() + "Unexpected component type: " + componentType);
            }

            Stack.Add(oldComponent);
        }

        protected void PopComponent()
        {
            if (Component == null)
            {
                if ((TargetType == "BaseObjectNodeTree") && (Tree != null))
                    Component = Tree;
                else
                    return;
            }

            EmptyTitleCheck();

            BaseObjectContent content;
            string componentType = GetComponentType();

            switch (componentType)
            {
                case "BaseObjectNodeTree":
                    if (Tree.KeyInt > 0)
                    {
                        Tree.UpdateReferencesCheck(Repositories, false, true);
                        if (!NodeUtilities.UpdateTree(Tree, false, false))
                            throw new Exception(FormatErrorPrefix() + "Error updating tree.");
                    }
                    else
                        SaveObject(Tree);
                    break;
                case "BaseObjectNode":
                    if (Tree.KeyInt > 0)
                        GetComponent<BaseObjectNode>().UpdateReferencesCheck(Repositories, false, true);
                    else
                        SaveObject(Component);
                    break;
                case "BaseObjectContent":
                    content = GetComponent<BaseObjectContent>();
                    if (content.HasContentStorageKey)
                    {
                        //if (TargetType != "BaseObjectContent")
                        //    CleanNonTouchedStudyItems(content);

                        if (!NodeUtilities.UpdateContentStorageCheck(content, false))
                            throw new Exception(FormatErrorPrefix() + "Error updating content: " + content.GetTitleString(UILanguageID));
                    }
                    else
                    {
                        if (!NodeUtilities.AddContent(content))
                            throw new Exception(FormatErrorPrefix() + "Error adding content: " + content.GetTitleString(UILanguageID));
                    }
                    break;
                default:
                    throw new Exception(FormatErrorPrefix() + "Unexpected component type: " + componentType);
            }

            if (Stack.Count() >= 1)
            {
                Component = Stack.Last();
                Stack.RemoveAt(Stack.Count() - 1);
                content = Component as BaseObjectContent;
                if (content != null)
                {
                    ContentStudyList studyList = content.ContentStorageStudyList;
                    if (studyList != null)
                        ItemIndex = studyList.StudyItemCount();
                }
            }
        }

        protected BaseObjectContent AddStudyContentChildReplaceTarget(BaseObjectContent parentContent)
        {
            BaseObjectContent content = NodeUtilities.CreateContent(
                parentContent.Node,
                parentContent,
                parentContent.ContentType,
                parentContent.ContentSubType);
            NodeUtilities.SetupContentFromMaster(
                parentContent.Node,
                content,
                content.KeyString);
            content.SetupDirectory();
            NodeUtilities.AddContent(content);
            parentContent.AddContentChild(content);
            int index = Targets.IndexOf(parentContent);
            if (index != -1)
            {
                Targets.RemoveAt(index);
                Targets.Insert(index, content);
            }
            return content;
        }

        protected void UpdateTargets()
        {
            if (Targets != null)
            {
                foreach (IBaseObject target in Targets)
                {
                    if (target is BaseObjectNodeTree)
                    {
                        BaseObjectNodeTree tree = target as BaseObjectNodeTree;
                        if (tree.KeyInt > 0)
                        {
                            tree.UpdateReferencesCheck(Repositories, false, true);
                            if (!NodeUtilities.UpdateTree(tree, false, false))
                                throw new Exception(FormatErrorPrefix() + "Error updating tree.");
                        }
                        else
                            SaveObject(Tree);
                    }
                    else if (target is BaseObjectNode)
                    {
                        BaseObjectNode node = target as BaseObjectNode;
                        BaseObjectNodeTree tree = node.Tree;
                        if (tree.KeyInt > 0)
                            node.UpdateReferencesCheck(Repositories, false, true);
                        else
                            SaveObject(node);
                    }
                    else if (target is BaseObjectContent)
                    {
                        BaseObjectContent content = target as BaseObjectContent;
                        if (content.HasContentStorageKey)
                        {
                            if (!NodeUtilities.UpdateContentStorageCheck(content, false))
                                throw new Exception(FormatErrorPrefix() + "Error updating content: " + content.GetTitleString(UILanguageID));
                        }
                        else
                        {
                            if (!NodeUtilities.AddContent(content))
                                throw new Exception(FormatErrorPrefix() + "Error adding content: " + content.GetTitleString(UILanguageID));
                        }
                    }
                }
            }
        }

        protected void EmptyTitleCheck()
        {
            BaseObjectTitled titledObject = GetComponent<BaseObjectTitled>();
            MultiLanguageString title = null;

            if (titledObject == null)
                return;

            title = titledObject.Title;

            if (title == null)
                titledObject.Title = title = new MultiLanguageString("Title");

            ObjectUtilities.PrepareMultiLanguageString(title, String.Empty, UniqueLanguageIDs);

            LanguageString languageString = title.LanguageString(HostLanguageID);

            if (languageString == null)
            {
                languageString = title.LanguageStrings.FirstOrDefault();

                if (languageString == null)
                    return;
            }

            if (languageString.HasText())
                return;

            string label = titledObject.Label;

            if (String.IsNullOrEmpty(label))
                label = "Object";

            string titleString = UserRecord.UserName + " "
                + (!String.IsNullOrEmpty(TitlePrefix) ? TitlePrefix + " " : "")
                + label;

            if (languageString.LanguageID != LanguageLookup.English)
                titleString = LanguageUtilities.TranslateString(titleString, languageString.LanguageID);

            languageString.Text = titleString;
        }

        protected void SetComponentOptions(NodeMasterReference masterReference, MarkupTemplate markupTemplate,
            MarkupTemplateReference markupReference, List<IBaseObjectKeyed> options)
        {
            BaseObjectNodeTree tree = Tree;
            BaseObjectNode node = null;
            BaseObjectContent content = null;

            switch (GetComponentType())
            {
                case "BaseObjectNodeTree":
                    if (markupTemplate != null)
                        tree.LocalMarkupTemplate = markupTemplate;
                    if (markupReference != null)
                        tree.MarkupReference = markupReference;
                    if (options != null)
                        tree.Options = options;
                    if (masterReference != null)
                    {
                        tree.MasterReference = masterReference;
                        if (tree.Master != null)
                            NodeUtilities.SetupNodeFromMaster(tree);
                    }
                    break;
                case "BaseObjectNode":
                    node = GetComponent<BaseObjectNode>();
                    if (markupTemplate != null)
                        node.LocalMarkupTemplate = markupTemplate;
                    if (markupReference != null)
                        node.MarkupReference = markupReference;
                    if (options != null)
                        node.Options = options;
                    if (masterReference != null)
                    {
                        node.MasterReference = masterReference;
                        if (node.Master != null)
                            NodeUtilities.SetupNodeFromMaster(node);
                    }
                    break;
                case "BaseObjectContent":
                    content = GetComponent<BaseObjectContent>();
                    BaseContentStorage contentStorage = content.ContentStorage;
                    if (contentStorage != null)
                    {
                        if (markupTemplate != null)
                            contentStorage.LocalMarkupTemplate = markupTemplate;
                        if (markupReference != null)
                            contentStorage.MarkupReference = markupReference;
                    }
                    else
                    {
                        if (markupTemplate != null)
                            content.LocalMarkupTemplate = markupTemplate;
                        if (markupReference != null)
                            content.MarkupReference = markupReference;
                    }
                    if (options != null)
                        content.Options = options;
                    break;
                default:
                    throw new Exception(FormatErrorPrefix() + "Unexpected component type: " + GetComponentType());
            }
        }

        protected void CleanNonTouchedStudyItems(BaseObjectContent content)
        {
            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                return;

            int count = studyList.StudyItemCount();
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                MultiLanguageItem studyItem = studyList.GetStudyItemIndexed(index);

                if (studyItem == null)
                    continue;

                if (!studyItem.Modified)
                {
                    if (DeleteMediaFiles)
                    {
                        string mediaDirectoryTildeUrl = studyItem.MediaTildeUrl;
                        ContentUtilities.DeleteStudyItemMediaRunsAndMedia(studyItem, mediaDirectoryTildeUrl, DeleteMediaFiles);
                    }

                    studyList.DeleteStudyItemIndexed(index);
                }
            }
        }

        protected BaseObjectContent CreateVocabularyContentCheck(
            BaseObjectNode node,
            BaseObjectContent contentParent,
            string label,
            string nodeContentKey,
            bool isDeleteBefore)
        {
            return CreateContentCheck(
                node,
                contentParent,
                label,
                "Vocabulary",
                ContentClassType.StudyList,
                nodeContentKey,
                isDeleteBefore);
        }

        protected BaseObjectContent CreateContentCheck(
            BaseObjectNode node,
            BaseObjectContent contentParent,
            string contentType,
            string contentSubType,
            ContentClassType contentClass,
            string nodeContentKey,
            bool isDeleteBefore)
        {
            BaseObjectContent content = node.GetContent(nodeContentKey);

            if (content == null)
            {
                content = NodeUtilities.CreateContent(
                    node,
                    contentParent,
                    nodeContentKey,
                    contentClass,
                    contentType,
                    contentSubType);

                if (contentParent != null)
                    contentParent.AddContentChild(content);
                else
                    node.AddContentChild(content);

                /*
                content.IsPublic = true;

                string titleString = MasterContentItem.ComposeTitleString(node, contentType, contentSubType, UserProfile, false);
                string descriptionString = MasterContentItem.ComposeDescriptionString(node, contentType, contentSubType, UserProfile, false);

                MultiLanguageString title = new MultiLanguageString("Title");
                ObjectUtilities.PrepareMultiLanguageString(title, String.Empty, UniqueLanguageIDs);
                title.SetText(HostLanguageID, titleString);

                MultiLanguageString description = new MultiLanguageString("Title");
                ObjectUtilities.PrepareMultiLanguageString(description, String.Empty, UniqueLanguageIDs);
                description.SetText(HostLanguageID, descriptionString);

                Component = content;

                SetComponentTypesAndTitle("BaseObjectContent", null, null, title, description);
                */

                Component = content;

                if (!NodeUtilities.AddContent(content))
                    throw new Exception(FormatErrorPrefix() + "Error adding content: " + content.GetTitleString());
            }
            else if (isDeleteBefore)
            {
                if (DeleteMediaFiles)
                    NodeUtilities.DeleteContentMediaHelper(content);

                ContentStudyList studyList = content.ContentStorageStudyList;

                if (studyList != null)
                    studyList.DeleteAllStudyItems();
            }

            return content;
        }

        protected void SetComponentTypesAndTitle(string componentType, string contentType, string contentSubType,
            MultiLanguageString title, MultiLanguageString description)
        {
            BaseObjectTitled titledObject = GetComponent<BaseObjectTitled>();

            titledObject.Title = title;
            titledObject.Description = description;

            titledObject.SetupDirectoryCheck();

            if (componentType == "BaseObjectContent")
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();

                if (!String.IsNullOrEmpty(contentType) && (content.ContentType != contentType))
                    content.ContentType = contentType;

                if (!String.IsNullOrEmpty(contentSubType) && (content.ContentSubType != contentSubType))
                    content.ContentSubType = contentSubType;

                if (content.ContentStorage == null)
                    content.SetupContentStorage();

                if (!NodeUtilities.AddContent(content))
                    throw new Exception(FormatErrorPrefix() + "Error adding content: " + title);
            }
        }

        protected int GetItemCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                return ItemCounter;
            else
            {
                int itemCounter = 0;

                if (ItemCounters == null)
                    ItemCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                ItemCounters.TryGetValue(tag, out itemCounter);

                return itemCounter;
            }
        }

        protected void IncrementItemCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                ItemCounter = ItemCounter + 1;
            else
            {
                if (ItemCounters == null)
                    ItemCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                int itemCounter = 0;

                if (ItemCounters.TryGetValue(tag, out itemCounter))
                    ItemCounters[tag] = itemCounter + 1;
                else
                    ItemCounters.Add(tag, 1);
            }
        }

        protected int GetLessonCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                return LessonCounter;
            else
            {
                int lessonCounter = 0;

                if (LessonCounters == null)
                    LessonCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                LessonCounters.TryGetValue(tag, out lessonCounter);

                return lessonCounter;
            }
        }

        protected void IncrementLessonCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                LessonCounter = LessonCounter + 1;
            else
            {
                if (LessonCounters == null)
                    LessonCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                int lessonCounter = 0;

                if (LessonCounters.TryGetValue(tag, out lessonCounter))
                    LessonCounters[tag] = lessonCounter + 1;
                else
                    LessonCounters.Add(tag, 1);
            }
        }

        protected string FormatNumber(int number)
        {
            return String.Format("{0,4:d4}", number);
        }

        protected string GetGroupTitle(string tag)
        {
            int startGroupNumber = (GetItemCounter(tag) / (StudyItemSubDivideCount * MinorSubDivideCount)) * (StudyItemSubDivideCount * MinorSubDivideCount);
            int endGroupNumber = startGroupNumber + (StudyItemSubDivideCount * MinorSubDivideCount) - 1;
            string groupTitle = tag + (SubDivide ? (!String.IsNullOrEmpty(tag) ? " - " : "") + "(" + FormatNumber(startGroupNumber) + "-" + FormatNumber(endGroupNumber) + ")" : "");
            return groupTitle;
        }

        protected string GetLessonTitle(string tag)
        {
            int startLessonNumber = (GetItemCounter(tag) / StudyItemSubDivideCount) * StudyItemSubDivideCount;
            int endLessonNumber = startLessonNumber + StudyItemSubDivideCount - 1;
            string lessonTitle = tag + (SubDivide ? (!String.IsNullOrEmpty(tag) ? " - " : "") + "(" + FormatNumber(startLessonNumber) + "-" + FormatNumber(endLessonNumber) + ")" : "");
            return lessonTitle;
        }

        protected void AddEntry(
            MultiLanguageItem multiLanguageItem,
            string speakerKey = null,
            Dictionary<string, string> tags = null)
        {
            string tag = null;
            string contentType = DefaultContentType;
            string contentSubType = DefaultContentSubType;
            string label = multiLanguageItem.KeyString;
            string speakerNameKey = multiLanguageItem.SpeakerNameKey;
            string speakerNameControl = String.Empty;
            ContentStudyList studyList = null;
            MultiLanguageItem oldStudyItem = null;

            if ((tags != null) && (tags.Count != 0))
            {
                if (!tags.TryGetValue("tag", out tag))
                    tags.TryGetValue("node", out tag);

                if (!tags.TryGetValue("contentType", out contentType))
                    contentType = DefaultContentType;

                if (!tags.TryGetValue("contentSubType", out contentSubType))
                    contentSubType = DefaultContentSubType;

                if (!tags.TryGetValue("label", out label))
                    label = multiLanguageItem.KeyString;

                if (!tags.TryGetValue("n", out speakerNameKey))
                    speakerNameKey = multiLanguageItem.SpeakerNameKey;

                if (!tags.TryGetValue("nl", out speakerNameControl))
                    speakerNameControl = String.Empty;
            }

            string contentKey = MasterContentItem.ComposeKey(null, contentType, contentSubType, UserProfile, false);

            if (Component == null)
                Component = Tree;

            if (Component == null)
                return;

            bool allow = true;
            string flagsName = (contentSubType == "Vocabulary" ? contentSubType + " " + label : contentType);

            if ((ContentKeyFlags != null) && ContentKeyFlags.TryGetValue(flagsName, out allow) && !allow)
                return;

            if (Component is ContentStudyList)
            {
                studyList = GetComponent<ContentStudyList>();
                if (IsDoMerge)
                    oldStudyItem = studyList.GetStudyItemIndexed(ItemIndex);
                else if (IsFilterDuplicates)
                    oldStudyItem = studyList.FindOverlappingStudyItemAnchored(multiLanguageItem, AnchorLanguageFlags);
                if (oldStudyItem == null)
                {
                    if (IsExcludePrior)
                    {
                        BaseObjectContent oldContent;
                        if (NodeUtilities.StudyItemExistsInPriorLessonsCheck(
                                studyList.Content,
                                multiLanguageItem,
                                null,
                                out oldContent,
                                out oldStudyItem))
                            return;
                    }
                    multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                    studyList.InsertStudyItemIndexed(ItemIndex, multiLanguageItem);
                    multiLanguageItem.TouchAndSetModified();
                    ItemIndex = ItemIndex + 1;
                }
                else
                {
                    if (TargetType != "BaseObjectContent")
                    {
                        int oldIndex = studyList.GetStudyItemIndex(oldStudyItem);
                        if (ItemIndex != oldIndex)
                        {
                            studyList.DeleteStudyItemIndexed(oldIndex);
                            studyList.InsertStudyItemIndexed(ItemIndex, oldStudyItem);
                        }
                        ItemIndex = ItemIndex + 1;
                    }
                    else if (IsDoMerge)
                        ItemIndex = ItemIndex + 1;
                    multiLanguageItem.Rekey(oldStudyItem.Key);
                    oldStudyItem.Merge(multiLanguageItem);
                    oldStudyItem.TouchAndSetModified();
                }
            }
            else if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                if (content.ContentStorage == null)
                {
                    content.ContentStorageKey = 0;
                    content.SetupContentStorage();
                    NodeUtilities.AddContent(content);
                }
                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        {
                            studyList = content.ContentStorageStudyList;
                            if (studyList.KeyInt == 0)
                                NodeUtilities.AddContent(content);
                            if (IsDoMerge)
                                oldStudyItem = studyList.GetStudyItemIndexed(ItemIndex);
                            else if (IsFilterDuplicates)
                                oldStudyItem = studyList.FindOverlappingStudyItemAnchored(multiLanguageItem, AnchorLanguageFlags);
                            if (oldStudyItem == null)
                            {
                                if (IsExcludePrior)
                                {
                                    BaseObjectContent oldContent;
                                    if (NodeUtilities.StudyItemExistsInPriorLessonsCheck(
                                            studyList.Content,
                                            multiLanguageItem,
                                            null,
                                            out oldContent,
                                            out oldStudyItem))
                                        return;
                                }
                                multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                                studyList.InsertStudyItemIndexed(ItemIndex, multiLanguageItem);
                                multiLanguageItem.TouchAndSetModified();
                                ItemIndex = ItemIndex + 1;
                                AddStudyListCheck(studyList);
                            }
                            else
                            {
                                if (TargetType != "BaseObjectContent")
                                {
                                    int oldIndex = studyList.GetStudyItemIndex(oldStudyItem);
                                    if (ItemIndex != oldIndex)
                                    {
                                        studyList.DeleteStudyItemIndexed(oldIndex);
                                        studyList.InsertStudyItemIndexed(ItemIndex, oldStudyItem);
                                    }
                                    ItemIndex = ItemIndex + 1;
                                }
                                else if (IsDoMerge)
                                    ItemIndex = ItemIndex + 1;
                                multiLanguageItem.Rekey(oldStudyItem.Key);
                                oldStudyItem.MergeOverwrite(multiLanguageItem);
                                oldStudyItem.TouchAndSetModified();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (Component is ToolStudyList)
            {
                /* FIXME
                if (GetComponent<FlashList>().LoadEntryIndexed(ItemIndex, multiLanguageItem))
                    ItemIndex = ItemIndex + 1;
                */
            }
            else if (Component is BaseObjectNode)
            {
                BaseObjectNode node = GetComponent<BaseObjectNode>();
                BaseObjectContent content = node.GetContent(contentKey);
                studyList = null;
                if (content == null)
                    content = node.GetContentWithTypeAndSubType(contentType, contentSubType);
                if (content == null)
                {
                    if (Master != null)
                    {
                        MasterContentItem contentItem = Master.GetContentItem(contentKey);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(label);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentType);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentSubType);
                        if (contentItem != null)
                        {
                            contentKey = contentItem.KeyString;
                            NodeUtilities.SetupContentFromContentItem(node, null, contentItem);
                            content = node.GetContent(contentKey);
                        }
                    }
                    if (content == null)
                        content = NodeUtilities.CreateContent(node, null, contentType, contentSubType);
                    if ((content != null) && !node.ContentList.Contains(content))
                        node.AddContentChild(content);
                }
                if (content != null)
                {
                    studyList = content.ContentStorageStudyList;
                    contentKey = content.KeyString;
                }
                if (studyList == null)
                    return;
                if (studyList.KeyInt == 0)
                    NodeUtilities.AddContent(content);
                AddStudyListCheck(studyList);
                /*
                if (IsExcludePrior)
                {
                    BaseObjectContent oldContent;
                    if (NodeUtilities.StudyItemExistsInPriorLessonsCheck(
                            studyList.Content,
                            multiLanguageItem,
                            null,
                            out oldContent,
                            out oldStudyItem))
                        return;
                }
                multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                studyList.AddStudyItem(multiLanguageItem);
                multiLanguageItem.TouchAndSetModified();
                */
                if (IsDoMerge)
                    oldStudyItem = studyList.GetStudyItemIndexed(ItemIndex);
                else if (IsFilterDuplicates)
                    oldStudyItem = studyList.FindOverlappingStudyItemAnchored(multiLanguageItem, AnchorLanguageFlags);
                if (oldStudyItem == null)
                {
                    if (IsExcludePrior)
                    {
                        BaseObjectContent oldContent;
                        if (NodeUtilities.StudyItemExistsInPriorLessonsCheck(
                                studyList.Content,
                                multiLanguageItem,
                                null,
                                out oldContent,
                                out oldStudyItem))
                            return;
                    }
                    multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                    studyList.InsertStudyItemIndexed(ItemIndex, multiLanguageItem);
                    multiLanguageItem.TouchAndSetModified();
                    ItemIndex = ItemIndex + 1;
                }
                else
                {
                    if (TargetType != "BaseObjectContent")
                    {
                        int oldIndex = studyList.GetStudyItemIndex(oldStudyItem);
                        if (ItemIndex != oldIndex)
                        {
                            studyList.DeleteStudyItemIndexed(oldIndex);
                            studyList.InsertStudyItemIndexed(ItemIndex, oldStudyItem);
                        }
                        ItemIndex = ItemIndex + 1;
                    }
                    else if (IsDoMerge)
                        ItemIndex = ItemIndex + 1;
                    multiLanguageItem.Rekey(oldStudyItem.Key);
                    oldStudyItem.Merge(multiLanguageItem);
                    oldStudyItem.TouchAndSetModified();
                }
            }

            if (studyList != null)
            {
                if (!String.IsNullOrEmpty(speakerNameKey))
                {
                    MultiLanguageString speakerNameMLS = studyList.GetSpeakerName(speakerNameKey);

                    if (speakerNameMLS == null)
                        speakerNameMLS = studyList.FindSpeakerName(speakerNameKey, null);

                    string speakerNameDescriptorName = "";
                    int speakerNameDescriptorIndex = 0;

                    switch (speakerNameControl)
                    {
                        case "n":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nt":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nt1":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 1;
                            break;
                        case "nt2":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 2;
                            break;
                        case "nt3":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 3;
                            break;
                        case "nh":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nh1":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 1;
                            break;
                        case "nh2":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 2;
                            break;
                        case "nh3":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 3;
                            break;
                        default:
                            break;
                    }

                    LanguageDescriptor languageDescriptor = null;
                    LanguageID languageID = UILanguageID;

                    if (LanguageDescriptors != null)
                    {
                        int index = 0;
                        foreach (LanguageDescriptor ld in LanguageDescriptors)
                        {
                            if (ld.Used && (ld.LanguageID != null) && (ld.Name == speakerNameDescriptorName))
                            {
                                if (index == speakerNameDescriptorIndex)
                                    languageDescriptor = ld;

                                index++;
                            }
                        }
                        if (languageDescriptor == null)
                            languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == speakerNameDescriptorName) && x.Used && (x.LanguageID != null));
                    }

                    if (languageDescriptor != null)
                        languageID = languageDescriptor.LanguageID;

                    if (speakerNameMLS == null)
                    {
                        speakerNameMLS = new MultiLanguageString(
                            speakerNameKey,
                            new LanguageString(speakerNameKey, languageID, speakerNameKey));
                        studyList.AddSpeakerName(speakerNameMLS);
                    }
                }

                multiLanguageItem.SpeakerNameKey = speakerNameKey;
            }

            if (IsTranslateMissingItems)
            {
                BaseObjectContent content = multiLanguageItem.Content;
                List<LanguageID> languageIDs;
                if (content != null)
                    languageIDs = content.LanguageIDs;
                else
                    languageIDs = UserProfile.LanguageIDs;
                string errorMessage;
                LanguageUtilities.Translator.TranslateMultiLanguageItem(
                    multiLanguageItem,
                    languageIDs,
                    true,
                    true,
                    out errorMessage,
                    false);
            }
        }

        protected void MergeEntry(MultiLanguageItem multiLanguageItem, string speakerKey = null, Dictionary<string, string> tags = null)
        {
            string tag = null;
            string contentType = DefaultContentType;
            string contentSubType = DefaultContentSubType;
            string label = multiLanguageItem.KeyString;
            string speakerNameKey = multiLanguageItem.SpeakerNameKey;
            string speakerNameControl = String.Empty;
            ContentStudyList studyList = null;
            MultiLanguageItem oldStudyItem = null;

            if ((tags != null) && (tags.Count != 0))
            {
                if (!tags.TryGetValue("tag", out tag))
                    tags.TryGetValue("node", out tag);

                if (!tags.TryGetValue("contentType", out contentType))
                    contentType = DefaultContentType;

                if (!tags.TryGetValue("contentSubType", out contentSubType))
                    contentSubType = DefaultContentSubType;

                if (!tags.TryGetValue("label", out label))
                    label = multiLanguageItem.KeyString;

                if (!tags.TryGetValue("n", out speakerNameKey))
                    speakerNameKey = multiLanguageItem.SpeakerNameKey;

                if (!tags.TryGetValue("nl", out speakerNameControl))
                    speakerNameControl = String.Empty;
            }

            string contentKey = MasterContentItem.ComposeKey(null, contentType, contentSubType, UserProfile, false);

            if (Component == null)
                Component = Tree;

            if (Component == null)
                return;

            bool allow = true;
            string flagsName = (contentSubType == "Vocabulary" ? contentSubType + " " + label : contentType);

            if ((ContentKeyFlags != null) && ContentKeyFlags.TryGetValue(flagsName, out allow) && !allow)
                return;

            if (Component is ContentStudyList)
            {
                studyList = GetComponent<ContentStudyList>();
                oldStudyItem = studyList.GetStudyItemIndexed(ItemIndex);
                if (oldStudyItem == null)
                {
                    multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                    studyList.InsertStudyItemIndexed(ItemIndex, multiLanguageItem);
                    multiLanguageItem.TouchAndSetModified();
                    ItemIndex = ItemIndex + 1;
                }
                else
                {
                    oldStudyItem.Merge(multiLanguageItem);
                    oldStudyItem.TouchAndSetModified();
                    ItemIndex = ItemIndex + 1;
                }
            }
            else if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                if (content.ContentStorage == null)
                {
                    content.ContentStorageKey = 0;
                    content.SetupContentStorage();
                    NodeUtilities.AddContent(content);
                }
                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        {
                            studyList = content.ContentStorageStudyList;
                            if (studyList.KeyInt == 0)
                                NodeUtilities.AddContent(content);
                            oldStudyItem = studyList.GetStudyItemIndexed(ItemIndex);
                            if (oldStudyItem == null)
                            {
                                multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                                studyList.InsertStudyItemIndexed(ItemIndex, multiLanguageItem);
                                multiLanguageItem.TouchAndSetModified();
                                ItemIndex = ItemIndex + 1;
                                AddStudyListCheck(studyList);
                            }
                            else
                            {
                                oldStudyItem.MergeOverwrite(multiLanguageItem);
                                oldStudyItem.TouchAndSetModified();
                                ItemIndex = ItemIndex + 1;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (Component is ToolStudyList)
            {
                /* FIXME
                if (GetComponent<FlashList>().LoadEntryIndexed(ItemIndex, multiLanguageItem))
                    ItemIndex = ItemIndex + 1;
                */
            }
            else if (Component is BaseObjectNode)
            {
                BaseObjectNode node = GetComponent<BaseObjectNode>();
                BaseObjectContent content = node.GetContent(contentKey);
                studyList = null;
                if (content == null)
                    content = node.GetContentWithTypeAndSubType(contentType, contentSubType);
                if (content == null)
                {
                    if (Master != null)
                    {
                        MasterContentItem contentItem = Master.GetContentItem(contentKey);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(label);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentType);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentSubType);
                        if (contentItem != null)
                        {
                            contentKey = contentItem.KeyString;
                            NodeUtilities.SetupContentFromContentItem(node, null, contentItem);
                            content = node.GetContent(contentKey);
                        }
                    }
                    if (content == null)
                        content = NodeUtilities.CreateContent(node, null, contentType, contentSubType);
                    if (content != null)
                        node.AddContentChild(content);
                }
                if (content != null)
                {
                    studyList = content.ContentStorageStudyList;
                    contentKey = content.KeyString;
                }
                if (studyList == null)
                    return;
                if (studyList.KeyInt == 0)
                    NodeUtilities.AddContent(content);
                AddStudyListCheck(studyList);
                multiLanguageItem.Rekey(studyList.AllocateStudyItemKey());
                studyList.AddStudyItem(multiLanguageItem);
                multiLanguageItem.TouchAndSetModified();
            }

            if (studyList != null)
            {
                if (!String.IsNullOrEmpty(speakerNameKey))
                {
                    MultiLanguageString speakerNameMLS = studyList.GetSpeakerName(speakerNameKey);

                    if (speakerNameMLS == null)
                        speakerNameMLS = studyList.FindSpeakerName(speakerNameKey, null);

                    string speakerNameDescriptorName = "";
                    int speakerNameDescriptorIndex = 0;

                    switch (speakerNameControl)
                    {
                        case "n":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nt":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nt1":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 1;
                            break;
                        case "nt2":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 2;
                            break;
                        case "nt3":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = 3;
                            break;
                        case "nh":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 0;
                            break;
                        case "nh1":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 1;
                            break;
                        case "nh2":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 2;
                            break;
                        case "nh3":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = 3;
                            break;
                        default:
                            break;
                    }

                    LanguageDescriptor languageDescriptor = null;
                    LanguageID languageID = UILanguageID;

                    if (LanguageDescriptors != null)
                    {
                        int index = 0;
                        foreach (LanguageDescriptor ld in LanguageDescriptors)
                        {
                            if (ld.Used && (ld.LanguageID != null) && (ld.Name == speakerNameDescriptorName))
                            {
                                if (index == speakerNameDescriptorIndex)
                                    languageDescriptor = ld;

                                index++;
                            }
                        }
                        if (languageDescriptor == null)
                            languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == speakerNameDescriptorName) && x.Used && (x.LanguageID != null));
                    }

                    if (languageDescriptor != null)
                        languageID = languageDescriptor.LanguageID;

                    if (speakerNameMLS == null)
                    {
                        speakerNameMLS = new MultiLanguageString(
                            speakerNameKey,
                            new LanguageString(speakerNameKey, languageID, speakerNameKey));
                        studyList.AddSpeakerName(speakerNameMLS);
                    }
                }

                multiLanguageItem.SpeakerNameKey = speakerNameKey;
            }

            if (IsTranslateMissingItems)
            {
                BaseObjectContent content = multiLanguageItem.Content;
                List<LanguageID> languageIDs;
                if (content != null)
                    languageIDs = content.LanguageIDs;
                else
                    languageIDs = UserProfile.LanguageIDs;
                string errorMessage;
                LanguageUtilities.Translator.TranslateMultiLanguageItem(
                    multiLanguageItem,
                    languageIDs,
                    false,
                    false,
                    out errorMessage,
                    false);
            }
        }

        protected ContentStudyList GetTargetStudyList()
        {
            ContentStudyList studyList = null;

            IBaseObject component = Component;

            if (component == null)
            {
                if ((Targets == null) || (Targets.Count() == 0))
                {
                    if (ContentSource != null)
                        component = ContentSource;
                    else if (NodeSource != null)
                        component = NodeSource;
                    else if (TreeSource != null)
                        component = TreeSource;
                    else if (Tree != null)
                        component = Tree;
                }
                else
                    component = Targets.First();
            }

            if (component == null)
                return null;

            if (component is ContentStudyList)
                studyList = component as ContentStudyList;
            else if (component is BaseObjectContent)
            {
                BaseObjectContent content = component as BaseObjectContent;

                if (content.ContentStorage == null)
                {
                    content.ContentStorageKey = 0;
                    content.SetupContentStorage();
                    NodeUtilities.AddContent(content);
                }

                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        studyList = content.ContentStorageStudyList;
                        break;
                    default:
                        break;
                }
            }
            else if (component is BaseObjectNode)
            {
                string contentType = DefaultContentType;
                string contentSubType = DefaultContentSubType;
                string label = TargetLabel;

                if (String.IsNullOrEmpty(contentType))
                    contentType = "Words";

                string contentKey = MasterContentItem.ComposeKey(null, contentType, contentSubType, UserProfile, false);
                BaseObjectNode node = component as BaseObjectNode;
                BaseObjectContent content = node.GetContent(contentKey);

                if (content == null)
                    content = node.GetContentWithTypeAndSubType(contentType, contentSubType);

                if (content == null)
                {
                    if (Master != null)
                    {
                        MasterContentItem contentItem = Master.GetContentItem(contentKey);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(label);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentType);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentSubType);
                        if (contentItem != null)
                        {
                            contentKey = contentItem.KeyString;
                            NodeUtilities.SetupContentFromContentItem(node, null, contentItem);
                            content = node.GetContent(contentKey);
                        }
                    }
                    if (content == null)
                        content = NodeUtilities.CreateContent(node, null, contentType, contentSubType);
                    if ((content != null) && !node.ContentList.Contains(content))
                        node.AddContentChild(content);
                }

                if (content != null)
                {
                    studyList = content.ContentStorageStudyList;
                    contentKey = content.KeyString;
                }

                if (studyList == null)
                    return null;

                if (studyList.KeyInt == 0)
                    NodeUtilities.AddContent(content);

                AddStudyListCheck(studyList);
            }

            return studyList;
        }

        protected ContentStudyList GetTargetStudyList(string contentKey)
        {
            ContentStudyList studyList = null;

            IBaseObject component = Component;

            if (component == null)
            {
                if ((Targets == null) || (Targets.Count() == 0))
                {
                    if (ContentSource != null)
                        component = ContentSource;
                    else if (NodeSource != null)
                        component = NodeSource;
                    else if (TreeSource != null)
                        component = TreeSource;
                    else if (Tree != null)
                        component = Tree;
                }
                else
                    component = Targets.First();
            }

            if (component == null)
                return null;

            if (component is ContentStudyList)
                studyList = component as ContentStudyList;
            else if (component is BaseObjectContent)
            {
                BaseObjectContent content = component as BaseObjectContent;

                if (content.ContentStorage == null)
                {
                    content.ContentStorageKey = 0;
                    content.SetupContentStorage();
                    NodeUtilities.AddContent(content);
                }

                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        studyList = content.ContentStorageStudyList;
                        break;
                    default:
                        break;
                }
            }
            else if (component is BaseObjectNode)
            {
                BaseObjectNode node = component as BaseObjectNode;
                BaseObjectContent content = node.GetContent(contentKey);
                string label;
                string contentType;
                string contentSubType;

                switch (contentKey)
                {
                    case "Text":
                        label = "Text";
                        contentType = "Text";
                        contentSubType = "Text";
                        break;
                    case "Sentences":
                        label = "Sentences";
                        contentType = "Sentences";
                        contentSubType = "Vocabulary";
                        break;
                    case "Words":
                    default:
                        label = "Words";
                        contentType = "Words";
                        contentSubType = "Vocabulary";
                        break;
                }

                if (content == null)
                {
                    if (Master != null)
                    {
                        MasterContentItem contentItem = Master.GetContentItem(contentKey);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(label);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentType);
                        if (contentItem == null)
                            contentItem = Master.GetContentItem(contentSubType);
                        if (contentItem != null)
                        {
                            contentKey = contentItem.KeyString;
                            NodeUtilities.SetupContentFromContentItem(node, null, contentItem);
                            content = node.GetContent(contentKey);
                        }
                    }
                    if (content == null)
                        content = NodeUtilities.CreateContent(node, null, contentType, contentSubType);
                    if ((content != null) && !node.ContentList.Contains(content))
                        node.AddContentChild(content);
                }

                if (content != null)
                {
                    studyList = content.ContentStorageStudyList;
                    contentKey = content.KeyString;
                }

                if (studyList == null)
                    return null;

                if (studyList.KeyInt == 0)
                    NodeUtilities.AddContent(content);

                AddStudyListCheck(studyList);
            }

            return studyList;
        }

        protected void SaveBlockObjects()
        {
            if (BlockObjects != null)
            {
                if (TargetType == "MultiLanguageItem")
                {
                    throw new Exception("SaveBlockObjects not implemented for MultiLanguageItem.");
                }
                else if (TargetType == "MultiLanguageString")
                {
                    throw new Exception("SaveBlockObjects not implemented for MultiLanguageString.");
                }
                else if (TargetType == "DictionaryEntry")
                {
                    List<DictionaryEntry> dictionaryEntries = BlockObjects.Cast<DictionaryEntry>().ToList();
                    BlockObjects = null;

                    foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                        AddDictionaryEntry(dictionaryEntry);

                    SaveDictionaryEntries();
                }
            }
        }

        protected void AddDictionaryEntry(DictionaryEntry dictionaryEntry)
        {
            if (BlockObjects != null)
            {
                BlockObjects.Add(dictionaryEntry);
                return;
            }

            string key = dictionaryEntry.KeyString;
            LanguageID languageID = dictionaryEntry.LanguageID;
            DictionaryEntry oldDictionaryEntry = Repositories.Dictionary.Get(key, languageID);

            if (oldDictionaryEntry == null)
            {
                if (AddObjects == null)
                    AddObjects = new List<IBaseObjectKeyed>();

                AddObjects.Add(dictionaryEntry);
            }
            else
            {
                oldDictionaryEntry.MergeEntry(dictionaryEntry);

                if (oldDictionaryEntry.Modified)
                {
                    if (UpdateObjects == null)
                        UpdateObjects = new List<IBaseObjectKeyed>();

                    UpdateObjects.Add(oldDictionaryEntry);
                }
            }
            ItemIndex = ItemIndex + 1;
        }

        protected void MergeDictionaryEntry(DictionaryEntry dictionaryEntry)
        {
            string key = dictionaryEntry.KeyString;
            LanguageID languageID = dictionaryEntry.LanguageID;
            DictionaryEntry oldDictionaryEntry = null;

            if (AddObjects != null)
            {
                foreach (IBaseObjectKeyed obj in AddObjects)
                {
                    DictionaryEntry testObj = obj as DictionaryEntry;

                    if (testObj != null)
                    {
                        if ((testObj.KeyString == key) && (testObj.LanguageID == languageID))
                        {
                            oldDictionaryEntry = testObj;
                            break;
                        }
                    }
                }
            }

            if ((oldDictionaryEntry == null) && (UpdateObjects != null))
            {
                foreach (IBaseObjectKeyed obj in AddObjects)
                {
                    DictionaryEntry testObj = obj as DictionaryEntry;

                    if (testObj != null)
                    {
                        if ((testObj.KeyString == key) && (testObj.LanguageID == languageID))
                        {
                            oldDictionaryEntry = testObj;
                            break;
                        }
                    }
                }
            }

            if (oldDictionaryEntry != null)
                oldDictionaryEntry.MergeEntry(dictionaryEntry);
            else
                AddDictionaryEntry(dictionaryEntry);
        }

        protected void SaveDictionaryEntries()
        {
            if ((UpdateObjects == null) && (AddObjects == null))
                return;

            if ((UpdateObjects != null) && (UpdateObjects.Count() != 0) && !(UpdateObjects[0] is DictionaryEntry))
                return;

            if ((AddObjects != null) && (AddObjects.Count() != 0) && !(AddObjects[0] is DictionaryEntry))
                return;

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            List<IBaseObjectKeyed> readObjects = new List<IBaseObjectKeyed>();

            if (UpdateObjects != null)
            {
                foreach (LanguageID languageID in UniqueLanguageIDs)
                {
                    dictionaryEntries.Clear();

                    foreach (IBaseObjectKeyed obj in UpdateObjects)
                    {
                        DictionaryEntry dictionaryEntry = obj as DictionaryEntry;

                        if (dictionaryEntry != null)
                        {
                            if (dictionaryEntry.LanguageID == languageID)
                                dictionaryEntries.Add(dictionaryEntry);
                        }
                    }

                    if (dictionaryEntries.Count() != 0)
                    {
                        try
                        {
                            if (!Repositories.Dictionary.UpdateList(dictionaryEntries, languageID))
                            {
                                Error = Error + LanguageUtilities.TranslateUIString("Error updating dictionary entries.") + "\n";
                            }
                        }
                        catch (Exception exc)
                        {
                            Error = Error + exc.Message;
                            if (exc.InnerException != null)
                                Error = Error + ": " + exc.InnerException.Message;
                            Error = Error + "\n";
                        }
                    }
                }

                readObjects.AddRange(UpdateObjects);
            }

            if (AddObjects != null)
            {
                foreach (LanguageID languageID in UniqueLanguageIDs)
                {
                    dictionaryEntries.Clear();

                    foreach (IBaseObjectKeyed obj in AddObjects)
                    {
                        DictionaryEntry dictionaryEntry = obj as DictionaryEntry;

                        if (dictionaryEntry != null)
                        {
                            if (dictionaryEntry.LanguageID == languageID)
                                dictionaryEntries.Add(dictionaryEntry);
                        }
                    }

                    if (dictionaryEntries.Count() != 0)
                    {
                        try
                        {
                            if (!Repositories.Dictionary.AddList(dictionaryEntries, languageID))
                            {
                                Error = Error + LanguageUtilities.TranslateUIString("Error adding dictionary entries.") + "\n";
                            }
                        }
                        catch (Exception exc)
                        {
                            Error = Error + exc.Message;
                            if (exc.InnerException != null)
                                Error = Error + ": " + exc.InnerException.Message;
                            Error = Error + "\n";
                        }
                    }
                }

                readObjects.AddRange(AddObjects);
            }

            readObjects.Sort(ObjectUtilities.CompareKeys);

            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>();

            ReadObjects.AddRange(readObjects);
        }

        protected void SaveCachedObjects()
        {
            switch (TargetType)
            {
                case "DictionaryEntry":
                    SaveDictionaryEntries();
                    break;
                default:
                    break;
            }
        }

        protected void AddStudyListCheck(ContentStudyList studyList)
        {
            if (!SubDivide)
                return;

            if (ReadStudyLists == null)
                ReadStudyLists = new List<ContentStudyList>() { studyList };
            else if (!ReadStudyLists.Contains(studyList))
                ReadStudyLists.Add(studyList);
        }

        protected void SubDivideStudyLists()
        {
            if (ReadStudyLists == null)
                return;

            foreach (ContentStudyList studyList in ReadStudyLists)
                SubDivideStudyList(studyList);
        }

        protected void SubDivideStudyList(ContentStudyList studyList)
        {
            NodeUtilities.SubDivideStudyList(studyList, TitlePrefix, Master,
                SubDivideToStudyListsOnly, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);
        }

        protected void AddEntryList(List<MultiLanguageItem> multiLanguageItems)
        {
            if (Component == null)
                return;

            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
                AddEntry(multiLanguageItem);
        }

        protected MultiLanguageItem GetEntry()
        {
            MultiLanguageItem multiLanguageItem = null;

            if (Component == null)
                return multiLanguageItem;

            if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        if (RecurseStudyItems)
                            multiLanguageItem = RecursedStudyItems[ItemIndex];
                        else
                        {
                            ContentStudyList studyList = content.ContentStorageStudyList;
                            if (studyList != null)
                                multiLanguageItem = studyList.GetStudyItemIndexed(ItemIndex);
                        }
                        break;
                    case ContentClassType.DocumentItem:
                        break;
                    case ContentClassType.MediaList:
                        break;
                    case ContentClassType.MediaItem:
                        ContentMediaItem mediaItem = content.ContentStorageMediaItem;
                        if (mediaItem != null)
                            multiLanguageItem = mediaItem.GetLocalStudyItemIndexed(ItemIndex);
                        break;
                    default:
                        break;
                }
            }
            else if (Component is ContentStudyList)
            {
                if (RecurseStudyItems)
                    multiLanguageItem = RecursedStudyItems[ItemIndex];
                else
                    multiLanguageItem = GetComponent<ContentStudyList>().GetStudyItemIndexed(ItemIndex);
            }
            else if (Component is ContentMediaItem)
                multiLanguageItem = GetComponent<ContentMediaItem>().GetLocalStudyItemIndexed(ItemIndex);
            else if (Component is ToolStudyList)
            {
                ToolStudyItem toolStudyItem = GetComponent<ToolStudyList>().GetToolStudyItemIndexed(ItemIndex);

                if (toolStudyItem != null)
                    multiLanguageItem = toolStudyItem.StudyItem;
            }

            if (multiLanguageItem != null)
                ItemIndex = ItemIndex + 1;

            return multiLanguageItem;
        }

        protected List<MultiLanguageItem> GetEntryList()
        {
            List<MultiLanguageItem> multiLanguageItems = new List<MultiLanguageItem>(GetEntryCount());
            MultiLanguageItem multiLanguageItem = null;

            if (Component == null)
                return multiLanguageItems;

            for (ItemIndex = 0; ItemIndex < ItemCount; ItemIndex = ItemIndex + 1)
            {
                multiLanguageItem = GetEntry();
                multiLanguageItems.Add(multiLanguageItem);
            }

            ItemIndex = 0;

            return multiLanguageItems;
        }

        protected int GetEntryCount()
        {
            int returnValue = 0;

            if (Component == null)
                return returnValue;

            if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        if (RecurseStudyItems)
                            returnValue = RecursedStudyItems.Count() - ItemIndex;
                        else
                        {
                            ContentStudyList studyList = content.ContentStorageStudyList;
                            if (studyList != null)
                                returnValue = studyList.StudyItemCount() - ItemIndex;
                        }
                        break;
                    case ContentClassType.DocumentItem:
                        break;
                    case ContentClassType.MediaList:
                        break;
                    case ContentClassType.MediaItem:
                        ContentMediaItem mediaItem = content.ContentStorageMediaItem;
                        if (mediaItem != null)
                            returnValue = mediaItem.LocalStudyItemCount() - ItemIndex;
                        break;
                    default:
                        break;
                }
            }
            else if (Component is ContentStudyList)
            {
                if (RecurseStudyItems)
                    returnValue = RecursedStudyItems.Count() - ItemIndex;
                else
                    returnValue = GetComponent<ContentStudyList>().StudyItemCount() - ItemIndex;
            }
            else if (Component is ContentMediaItem)
                returnValue = GetComponent<ContentMediaItem>().LocalStudyItemCount() - ItemIndex;
            else if (Component is ToolStudyList)
            {
                ToolStudyList toolStudyList = GetComponent<ToolStudyList>();
                if (toolStudyList != null)
                    returnValue = toolStudyList.ToolStudyItemCount();
            }

            return returnValue;
        }

        public List<MultiLanguageItem> GetStudyItemsList()
        {
            IBaseObject obj = Component;
            List<MultiLanguageItem> studyItems = null;
            ContentStudyList studyList = null;
            ContentMediaItem mediaItem = null;
            if (obj is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                studyList = content.ContentStorageStudyList;
                mediaItem = content.ContentStorageMediaItem;
            }
            else if (obj is ContentStudyList)
                studyList = GetComponent<ContentStudyList>();
            else if (obj is ContentStudyList)
                mediaItem = GetComponent<ContentMediaItem>();
            if (studyList != null)
                studyItems = studyList.StudyItems;
            else if (mediaItem != null)
                studyItems = mediaItem.LocalStudyItems;
            return studyItems;
        }

        public MultiLanguageItem GetStudyItemIndexed(int studyItemIndex)
        {
            List<MultiLanguageItem> studyItems = GetStudyItemsList();
            if ((studyItemIndex >= 0) && (studyItemIndex < studyItems.Count))
                return studyItems[studyItemIndex];
            return null;
        }

        public bool ComponentHasStudyItems()
        {
            IBaseObject obj = Component;
            ContentStudyList studyList = null;
            ContentMediaItem mediaItem = null;
            bool returnValue = false;
            if (obj is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                studyList = content.ContentStorageStudyList;
                mediaItem = content.ContentStorageMediaItem;
            }
            else if (obj is ContentStudyList)
                studyList = GetComponent<ContentStudyList>();
            else if (obj is ContentStudyList)
                mediaItem = GetComponent<ContentMediaItem>();
            if (studyList != null)
                returnValue = (studyList.StudyItemCount() == 0 ? false : true);
            else if (mediaItem != null)
                returnValue = (mediaItem.LocalStudyItemCount() == 0 ? false : true);
            return returnValue;
        }

        public virtual void SetupLanguages()
        {
            if (UserProfile != null)
            {
                LanguageDescriptors = UserProfile.LanguageDescriptors;
                TargetLanguageIDs = UserProfile.TargetLanguageIDs;
                HostLanguageIDs = UserProfile.HostLanguageIDs;
                UniqueLanguageIDs = LanguageID.ConcatenateUnqueList(HostLanguageIDs, TargetLanguageIDs);
                TargetRomanizationHostLanguageIDs = LanguageID.ConcatenateUnqueList(TargetLanguageIDs, HostLanguageIDs);
                UILanguageID = UserProfile.UILanguageID;
                List<string> anchorLanguageFlagNames = LanguageID.GetLanguageCultureExtensionCodes(UniqueLanguageIDs);
                if (anchorLanguageFlagNames != null)
                    AnchorLanguageFlags = LanguageID.GetLanguageFlagsDictionaryFromStringList(anchorLanguageFlagNames, true);
            }
        }

        public virtual void SetupLanguages(
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            LanguageID uiLanguageID)
        {
            LanguageDescriptors = UserProfile.LanguageDescriptors;
            TargetLanguageIDs = targetLanguageIDs;
            HostLanguageIDs = hostLanguageIDs;
            UniqueLanguageIDs = LanguageID.ConcatenateUnqueList(HostLanguageIDs, TargetLanguageIDs);
            UILanguageID = uiLanguageID;
            List<string> anchorLanguageFlagNames = LanguageID.GetLanguageCultureExtensionCodes(UniqueLanguageIDs);
            if (anchorLanguageFlagNames != null)
                AnchorLanguageFlags = LanguageID.GetLanguageFlagsDictionaryFromStringList(anchorLanguageFlagNames, true);
        }

        public virtual LanguageTool GetLanguageTool(LanguageID languageID)
        {
            LanguageTool languageTool;

            if (languageID == null)
                return null;

            if (LanguageToolCache == null)
                LanguageToolCache = new Dictionary<string, LanguageTool>();
            else if (LanguageToolCache.TryGetValue(languageID.LanguageCode, out languageTool))
                return languageTool;

            languageTool = ApplicationData.LanguageTools.Create(languageID);

            if (languageTool == null)
                languageTool = new GenericLanguageTool(languageID, UserProfile.HostLanguageIDs, UserProfile.LanguageIDs);
            else
            {
                languageTool.HostLanguageIDs = UserProfile.HostLanguageIDs;
                languageTool.UserLanguageIDs = UserProfile.LanguageIDs;
            }

            languageTool.NodeUtilities = NodeUtilities;

            LanguageToolCache.Add(languageID.LanguageCode, languageTool);

            return languageTool;
        }

        public virtual LanguageID HostLanguageID
        {
            get
            {
                if (HostLanguageIDs == null)
                    return null;
                return HostLanguageIDs.FirstOrDefault();
            }
            set
            {
                if (HostLanguageIDs == null)
                    HostLanguageIDs = new List<LanguageID>(1) { HostLanguageID };
                else if (HostLanguageIDs.Count() == 0)
                    HostLanguageIDs.Add(HostLanguageID);
                else if (HostLanguageIDs[0] != value)
                    HostLanguageIDs[0] = value;
            }
        }

        public virtual LanguageID TargetLanguageID
        {
            get
            {
                if (TargetLanguageIDs == null)
                    return null;
                return TargetLanguageIDs.FirstOrDefault();
            }
            set
            {
                if (TargetLanguageIDs == null)
                    TargetLanguageIDs = new List<LanguageID>(1) { TargetLanguageID };
                else if (TargetLanguageIDs.Count() == 0)
                    TargetLanguageIDs.Add(TargetLanguageID);
                else if (TargetLanguageIDs[0] != value)
                    TargetLanguageIDs[0] = value;
            }
        }

        // File utilities.

        // Open read stream.
        protected Stream OpenReadStream(string path)
        {
            Stream stream = null;

            try
            {
                FileSingleton.DirectoryExistsCheck(path);
                stream = FileSingleton.OpenRead(path);
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception during file open: " + path, exc);
                return null;
            }

            return stream;
        }

        // Open create/write stream.
        protected Stream OpenWriteStream(string path)
        {
            try
            {
                if (FileSingleton.Exists(path))
                    FileSingleton.Delete(path);
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception deleting previous file: " + path, exc);
                return null;
            }

            Stream stream = null;

            try
            {
                FileSingleton.DirectoryExistsCheck(path);
                stream = FileSingleton.OpenWrite(path);
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception during file create: " + path, exc);
                return null;
            }

            return stream;
        }

        // Close read or write stream.
        protected void CloseStream(Stream stream)
        {
            if (stream != null)
                FileSingleton.Close(stream);
        }

        // Message and error utilities.

        public void UpdateErrorFromNodeUtilities()
        {
            if (NodeUtilities.HasError)
                PutLogError(NodeUtilities.Error);

            NodeUtilities.ClearMessageAndError();
        }

        public bool HasError()
        {
            return !String.IsNullOrEmpty(Error);
        }

        public void PutExceptionError(Exception exc)
        {
            string message = exc.Message;

            if (exc.InnerException != null)
                message = message + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = message;
            else if (!Error.Contains(message))
                Error = Error + "\r\n" + message;
        }

        public void PutExceptionError(string message, Exception exc)
        {
            string fullMessage = message + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = fullMessage;
            else if (!Error.Contains(fullMessage))
                Error = Error + "\r\n" + fullMessage;
        }

        public virtual void PutError(string fieldName, string message, string argument)
        {
            string str;

            if (!String.IsNullOrEmpty(fieldName))
                str = fieldName + ": " + message + ": " + argument;
            else
                str = message + ": " + argument;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutError(string fieldName, string message)
        {
            string str;

            if (!String.IsNullOrEmpty(fieldName))
                str = fieldName + ": " + message;
            else
                str = message;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutError(string message)
        {
            string str = message;

            if (String.IsNullOrEmpty(str))
                return;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutErrorArgument(string message, string arg1)
        {
            string str = message + ": " + arg1;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public string FormatErrorPrefix()
        {
            string prefix;

            if (!String.IsNullOrEmpty(SourceFileName))
            {
                if (LineNumber > 0)
                    prefix = SourceFileName + "(" + LineNumber.ToString() + "): ";
                else
                    prefix = SourceFileName + ": ";
            }
            else if (LineNumber > 0)
                prefix = "Line " + LineNumber.ToString() + ": ";
            else
                prefix = String.Empty;

            return prefix;
        }

        public virtual void PutMessage(string message)
        {
            string str = message;

            if (String.IsNullOrEmpty(Message))
                Message = str;
            else if (!Message.Contains(str))
                Message = Message + "\r\n" + str;
        }

        public virtual void PutMessage(string message, string arg1)
        {
            string str = S(message) + ": " + arg1;

            if (String.IsNullOrEmpty(Message))
                Message = str;
            else if (!Message.Contains(str))
                Message = Message + "\r\n" + str;
        }

        public virtual void PutMessageAlways(string message)
        {
            string str = S(message);

            if (String.IsNullOrEmpty(Message))
                Message = str;
            else
                Message = Message + "\r\n" + str;
        }

        public virtual void PutMessageAlways(string message, string arg1)
        {
            string str = S(message) + ": " + arg1;

            if (String.IsNullOrEmpty(Message))
                Message = str;
            else
                Message = Message + "\r\n" + str;
        }

        public bool HasMessage()
        {
            return !String.IsNullOrEmpty(Message);
        }

        public bool HasMessageOrError()
        {
            return !String.IsNullOrEmpty(Message) || !String.IsNullOrEmpty(Error);
        }

        public void PutLogError(string message)
        {
            SetOperationStatusLabel(message);
            PutError(message);
            WriteLog(message);
        }

        public void PutLogError(string message, string argument)
        {
            string msg = message + ": " + argument;
            SetOperationStatusLabel(msg);
            PutErrorArgument(message, argument);
            WriteLog(msg);
        }

        public void PutLogErrorArgument(string message, string argument)
        {
            string msg = message + ": " + argument;
            SetOperationStatusLabel(msg);
            PutErrorArgument(message, argument);
            WriteLog(msg);
        }

        public void PutLogExceptionError(Exception exc)
        {
            string message = exc.Message;

            if (exc.InnerException != null)
                message = message + ": " + exc.InnerException.Message;

            PutError(message);
            WriteLog(message);
        }

        public void PutLogExceptionError(string message, Exception exc)
        {
            string fullMessage = S(message) + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            PutError(fullMessage);
            WriteLog(fullMessage);
        }

        public void PutLogExceptionError(string message, string argument, Exception exc)
        {
            string fullMessage = S(message) + ": " + argument + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            PutError(fullMessage);
            WriteLog(fullMessage);
        }

        protected void LogMessage(string logMessage, string action)
        {
            Repositories.Log(logMessage, action, UserRecord);
        }

        public string S(string str)
        {
            if (LanguageUtilities != null)
                return LanguageUtilities.TranslateUIString(str);
            return str;
        }

        public virtual void DumpArguments(string label)
        {
            if (!String.IsNullOrEmpty(label))
                DumpString(label + ":\n");

            if (Arguments == null)
            {
                DumpString("(no arguments");
                return;
            }

            foreach (FormatArgument argument in Arguments)
                DumpArgument(argument);
        }

        public virtual void DumpArgument(FormatArgument argument)
        {
            DumpArgument(argument.Label, argument.Name, argument.Value);
        }

        public virtual void DumpArgument(string label, object value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
               msg += label + " = ";

            if (value != null)
                msg += value.ToString();
            else
                msg += "(null)";

            DumpString(msg);
        }

        public virtual void DumpArgument(string label, string name, object value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            if (value != null)
                msg += value.ToString();
            else
                msg += "(null)";

            if (!String.IsNullOrEmpty(name))
            {
                msg += " (--" + name + " ";

                if (value != null)
                    msg += value.ToString();

                msg += ")";
            }

            DumpString(msg);
        }

        public virtual void DumpArgumentList<T>(string label, List<T> value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            if (value != null)
            {
                int index = 0;

                foreach (object item in value)
                {
                    if (index != 0)
                        msg += ", ";

                    msg += item.ToString();
                    index++;
                }
            }
            else
                msg += "(null)";

            DumpString(msg);
        }

        public virtual void DumpStatistics()
        {
        }

        // Check for supported capability.
        // importExport:  "Import" or "Export"
        // contentType: class name or GetComponentName value
        // capability: "Supported" for general support,
        //  "UseFlags" for component item select support,
        //  "ContentKeyFlags" for node component select support,
        //  "NodeFlags" for sub-object select support,
        //  "Text" for support of text import/export,
        //  "Web" for support of web import.
        public static bool IsSupportedStatic(string importExport, string contentType, string capability)
        {
            return false;
        }

        public virtual bool IsSupportedVirtual(string importExport, string contentType, string capability)
        {
            return false;
        }

        public static string TypeStringStatic { get { return "Format"; } }

        public virtual string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

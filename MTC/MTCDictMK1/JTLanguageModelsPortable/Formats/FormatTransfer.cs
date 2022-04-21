using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatTransfer : Format
    {
        // Argument data.
        public override LanguageID TargetLanguageID { get; set; }
        public static string TargetLanguageIDPrompt = "Target language code";
        public static string TargetLanguageIDHelp = "Enter the target language code.";

        public override LanguageID HostLanguageID { get; set; }
        public static string HostLanguageIDPrompt = "Host language code";
        public static string HostLanguageIDHelp = "Enter the host language code.";

        public string TargetRepositoryName { get; set; }
        public static string TargetRepositoryNamePrompt = "Target repository name";
        public static string TargetRepositoryNameHelp = "Set this to the target repository name (\"JTLanguageWeb\" or \"MTCImport\").";

        public string StoreName { get; set; }
        protected static string StoreNamePrompt = "Store name";
        protected static string StoreNameHelp = "Enter the name of the object store in the repository.";
        protected static List<string> StoreNameList = new List<string>()
            {
                "Courses",
                "Plans",
                "MarkupTemplates",
                "NodeMasters",
                "ToolProfiles",
                "Dictionary",
                "DictionaryMultiAudio",
                "DictionaryPictures",
                "UserRecords",
                "UIText",
                "UIStrings"
            };

        public string ObjectName { get; set; }
        protected static string ObjectNamePrompt = "Object name";
        protected static string ObjectNameHelp = "Enter the name or key of the object to transfer, or \"*\" to transfer everything.";

        public string SourceListFile { get; set; }
        protected static string SourceListFilePrompt = "Source list file";
        protected static string SourceListFileHelp = "Enter the path to a file containing a list of files to transfer, in the format of the DisplayCourses command output.";

        public List<string> SourceDesignators { get; set; }
        protected static string SourceDesignatorsPrompt = "Source list file";
        protected static string SourceDesignatorsHelp = "Enter the list of files to transfer, in the format of the DisplayCourses command output.";

        public new string Owner { get; set; }
        protected static string OwnerPrompt = "Owner name";
        protected static string OwnerHelp = "Enter the object owner name, if relevant.";

        // Implementation data.
        protected string SourceTreeMediaPath;
        protected string TargetTreeMediaPath;

        private static string FormatDescription = "Tranfer objects or object stores between repositories.";

        public FormatTransfer(
                LanguageID targetLanguageID,
                LanguageID hostLanguageID,
                string targetRepositoryName,
                string storeName,
                string objectName,
                string sourceListFile,
                List<string> sourceDesignators,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Transfer", "FormatTransfer", FormatDescription, GetTargetTypeFromStoreName(storeName), "File", "text/plain", ".txt",
                userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            TargetLanguageID = targetLanguageID;
            HostLanguageID = hostLanguageID;
            TargetRepositoryName = targetRepositoryName;
            StoreName = storeName;
            ObjectName = objectName;
            SourceListFile = sourceListFile;
            SourceDesignators = sourceDesignators;

            ClearFormatTransfer();
        }

        public FormatTransfer(
                string targetType,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Transfer", "FormatTransfer", FormatDescription, targetType, "File", "text/plain", ".txt",
                userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatTransfer();
        }

        public FormatTransfer(FormatTransfer other)
            : base(other)
        {
            ClearFormatTransfer();
        }

        public FormatTransfer()
            : base("Transfer", "FormatTransfer", FormatDescription, String.Empty, "File", "text/plain", ".txt",
                null, null, null, null, null)
        {
            ClearFormatTransfer();
        }

        // For derived classes.
        public FormatTransfer(string arrangement, string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatTransfer();
        }

        public void ClearFormatTransfer()
        {
            // Base members.
            AllowStudent = false;
            AllowTeacher = false;
            AllowAdministrator = true;
            ImportExportType = "Transfer";
            NoDataStreamExport = true;

            // Format arguments.
            TargetLanguageID = null;
            HostLanguageID = null;
            TargetRepositoryName = null;
            StoreName = null;
            ObjectName = null;
            SourceListFile = null;
            SourceDesignators = null;
            Owner = null;

            // Implementation data.
            SourceTreeMediaPath = null;
            TargetTreeMediaPath = null;

    }

    public void CopyFormatTransfer(FormatTransfer other)
        {
            // Base members.
            AllowStudent = false;
            AllowTeacher = false;
            AllowAdministrator = true;
            ImportExportType = "Transfer";
            NoDataStreamExport = true;

            // Format arguments.
            TargetLanguageID = other.TargetLanguageID;
            HostLanguageID = other.HostLanguageID;
            TargetRepositoryName = other.TargetRepositoryName;
            StoreName = other.StoreName;
            ObjectName = other.ObjectName;
            SourceListFile = other.SourceListFile;
            SourceDesignators = other.SourceDesignators;
            Owner = other.Owner;

            // Implementation data.
            SourceTreeMediaPath = null;
            TargetTreeMediaPath = null;
        }

        public static string GetTargetTypeFromStoreName(string storeName)
        {
            string targetType;

            switch (storeName)
            {
                case "Courses":
                    targetType = "BaseObjectNodeTree";
                    break;
                case "Plans":
                    targetType = "BaseObjectNodeTree";
                    break;
                case "MarkupTemplates":
                    targetType = "MarkupTemplate";
                    break;
                case "NodeMasters":
                    targetType = "NodeMaster";
                    break;
                case "ToolProfiles":
                    targetType = "ToolProfile";
                    break;
                case "Dictionary":
                    targetType = "DictionaryEntry";
                    break;
                case "DictionaryMultiAudio":
                    targetType = "AudioMultiRecord";
                    break;
                case "DictionaryPictures":
                    targetType = "PictureRecord";
                    break;
                case "UserRecords":
                    targetType = "UserRecord";
                    break;
                case "UIText":
                    targetType = "BaseString";
                    break;
                case "UIStrings":
                    targetType = "BaseString";
                    break;
                default:
                    throw new Exception("Unexpected store name: " + storeName);
            }

            return targetType;
        }

        public override Format Clone()
        {
            return new FormatTransfer(this);
        }

        public override void Read(Stream stream)
        {
            Transfer();
        }

        public override void Write(Stream stream)
        {
            Transfer();
        }

        protected IMainRepository GetUserRepository(string name)
        {
            IMainRepository repository = ApplicationData.GetUserRepository(name);

            if (repository == null)
            {
                if (ApplicationData.UserRepositories.Count() <= 1)
                {
                    ApplicationData.Global.LoadUserRepositories();
                    repository = ApplicationData.GetUserRepository(name);

                    if (repository == null)
                    {
                        PutError("Unknown repository: " + name);
                        return null;
                    }
                }
                else
                {
                    PutError("Unknown repository: " + name);
                    return null;
                }
            }

            return repository;
        }

        public virtual bool LoadTargets()
        {
            if (ObjectName == "*")
                return LoadAllObjects();
            else if (!String.IsNullOrEmpty(ObjectName))
                return LoadOneObject(ObjectName, Owner);
            else
                return true;
        }

        public virtual bool LoadAllObjects()
        {
            IMainRepository sourceRepository = Repositories;
            IObjectStore objectStore = sourceRepository.FindObjectStore(StoreName, TargetLanguageID, HostLanguageID);

            if (objectStore == null)
            {
                PutError("Unknown object store name, or object store never created: " + StoreName);
                return false;
            }

            List<IBaseObjectKeyed> objList = objectStore.GetAll();

            if (objList == null)
            {
                PutError("Getting all objects failed for object store: " + StoreName);
                return false;
            }

            AddTargets(objList.Cast<IBaseObject>().ToList());

            return true;
        }

        public virtual bool LoadOneObject(
            string name,
            string owner)
        {
            IMainRepository sourceRepository = Repositories;
            string storeName = StoreName;

            switch (storeName)
            {
                case "Courses":
                    storeName = "CourseHeaders";
                    break;
                case "Plans":
                    storeName = "PlanHeaders";
                    break;
                default:
                    break;
            }

            IBaseObjectKeyed obj = sourceRepository.ResolveNamedReference(storeName, TargetLanguageID, owner, name);

            if (obj != null)
            {
                switch (storeName)
                {
                    case "Courses":
                        {
                            BaseObjectNodeTree tree = sourceRepository.Courses.Get(obj.Key);
                            if (tree == null)
                            {
                                PutError("Error getting course from course header: " + obj.Name);
                                return false;
                            }
                            AddTarget(tree);
                        }
                        break;
                    case "Plans":
                        {
                            BaseObjectNodeTree tree = sourceRepository.Plans.Get(obj.Key);
                            if (tree == null)
                            {
                                PutError("Error getting plan from plan header: " + obj.Name);
                                return false;
                            }
                            AddTarget(tree);
                        }
                        break;
                    default:
                        AddTarget(obj);
                        break;
                }

                return true;
            }
            else
            {
                PutError("Can't find object: " + name + " for owner: " + owner);
                return true;
            }
        }

        // Assumes sources loaded in Targets.
        public virtual bool Transfer()
        {
            if (!String.IsNullOrEmpty(SourceListFile) && ((SourceDesignators == null) || (SourceDesignators.Count() == 0)))
            {
                try
                {
                    SourceDesignators = FileSingleton.ReadAllLines(SourceListFile).ToList();
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error getting source list file", exc);
                    return false;
                }
            }

            if ((SourceDesignators != null) && (SourceDesignators.Count() != 0))
            {
                ContinueProgress(SourceDesignators.Count() + ProgressCountBase);

                foreach (string line in SourceDesignators)
                {
                    string[] parts = line.Split(LanguageLookup.Comma);

                    if (parts.Length < 1)
                        continue;

                    string title = parts[0].Trim();
                    string owner;

                    if (title.StartsWith("Title: "))
                    {
                        string[] titlePair = title.Split(LanguageLookup.Colon);
                        title = titlePair[1].Trim();
                    }

                    if (title.StartsWith("\""))
                        title = title.Substring(1);

                    if (title.EndsWith("\""))
                        title = title.Substring(0, title.Length - 1);

                    if (parts.Length >= 2)
                    {
                        owner = parts[1].Trim();

                        if (owner.StartsWith("Owner: "))
                        {
                            string[] ownerPair = parts[1].Split(LanguageLookup.Colon);
                            owner = ownerPair[1].Trim();
                        }
                    }
                    else
                        owner = Owner;

                    if (owner.StartsWith("\""))
                        owner = owner.Substring(1);

                    if (owner.EndsWith("\""))
                        owner = owner.Substring(0, owner.Length - 1);

                    if (Targets != null)
                        Targets.Clear();

                    if (!LoadOneObject(title, owner))
                        return false;

                    if (!TransferTargets(title, owner))
                        return false;
                }

                EndContinuedProgress();

                return true;
            }

            ContinueProgress(Targets.Count() + ProgressCountBase);

            bool returnValue = TransferTargets(ObjectName, Owner);

            EndContinuedProgress();

            return returnValue;
        }

        // Assumes sources loaded in Targets.
        public virtual bool TransferTargets(string title, string owner)
        {
            if ((Targets == null) || (Targets.Count() == 0))
            {
                PutError("Nothing to transfer. Targets empty.");
                return false;
            }

            IMainRepository sourceRepository = Repositories;
            IMainRepository targetRepository = GetUserRepository(TargetRepositoryName);

            if (targetRepository == null)
                return false;

            IObjectStore targetObjectStore = targetRepository.FindObjectStore(
                StoreName,
                TargetLanguageID,
                HostLanguageID);

            if (targetObjectStore == null)
            {
                PutError("Unknown target object store name, or object store never created: " + StoreName);
                return false;
            }

            NodeUtilities sourceNodeUtilities = new NodeUtilities(
                sourceRepository,
                null,
                UserRecord,
                UserProfile,
                null,
                null);
            NodeUtilities targetNodeUtilities = new NodeUtilities(
                targetRepository,
                null,
                UserRecord,
                UserProfile,
                null,
                null);

            bool isAll = (ObjectName == "*");

            bool returnValue = sourceNodeUtilities.TransferTargets(
                sourceRepository,
                targetRepository,
                StoreName,
                TargetLanguageID,
                HostLanguageID,
                sourceNodeUtilities,
                targetNodeUtilities,
                isAll,
                DeleteBeforeImport,
                Targets.Cast<IBaseObjectKeyed>().ToList());

            return returnValue;
        }

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            SetDefaultArguments();

            TargetRepositoryName = GetArgumentDefaulted("TargetRepositoryName", "string", "rw", TargetRepositoryName,
                TargetRepositoryNamePrompt, TargetRepositoryNameHelp);

            if (Origin == "Worker")
            {
                StoreName = GetStringListArgumentDefaulted("StoreName", "string", "rw", StoreName, StoreNameList,
                    StoreNamePrompt, StoreNameHelp);

                ObjectName = GetArgumentDefaulted("ObjectName", "string", "rw", ObjectName,
                    ObjectNamePrompt, ObjectNameHelp);

                SourceListFile = GetArgumentDefaulted("SourceListFile", "string", "rw", SourceListFile,
                    "SourceListFile", SourceListFileHelp);

                SourceDesignators = GetArgumentStringListDefaulted("SourceDesignators", "bigstring", "rw",
                    SourceDesignators, SourceDesignatorsPrompt, SourceDesignatorsHelp);

                Owner = GetArgumentDefaulted("Owner", "string", "rw", Owner,
                    OwnerPrompt, ObjectNameHelp);

                TargetLanguageID = GetLanguageIDArgumentDefaulted("TargetLanguageID", "languageID", "rw",
                    TargetLanguageID, TargetLanguageIDPrompt, TargetLanguageIDHelp);

                HostLanguageID = GetLanguageIDArgumentDefaulted("HostLanguageID", "languageID", "rw",
                    HostLanguageID, HostLanguageIDPrompt, HostLanguageIDHelp);
            }

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete target before transfer", DeleteBeforeImportHelp, null, null);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetDefaultArguments();

            SetArgument("TargetRepositoryName", "string", "rw", TargetRepositoryName,
                TargetRepositoryNamePrompt, TargetRepositoryNameHelp, null, null);

            if (Origin == "Worker")
            {
                SetStringListArgument("StoreName", "string", "rw", StoreName, StoreNameList,
                    StoreNamePrompt, StoreNameHelp);

                SetArgument("ObjectName", "string", "rw", "ObjectName",
                    ObjectNamePrompt, ObjectNameHelp, null, null);

                SetArgument("SourceListFile", "string", "rw", SourceListFile,
                    "SourceListFile", SourceListFileHelp, null, null);

                SetArgumentStringList("SourceDesignators", "bigstring", "rw",
                    SourceDesignators, SourceDesignatorsPrompt, SourceDesignatorsHelp, null, null);

                SetArgument("Owner", "string", "rw", Owner,
                    OwnerPrompt, ObjectNameHelp, null, null);

                SetLanguageIDArgument("TargetLanguageID", "languageID", "rw",
                    TargetLanguageID, TargetLanguageIDPrompt, TargetLanguageIDHelp);

                SetLanguageIDArgument("HostLanguageID", "languageID", "rw",
                    HostLanguageID, HostLanguageIDPrompt, HostLanguageIDHelp);
            }

            SetFlagArgument("DeleteBeforeImport", "flag", "rw", DeleteBeforeImport, "Delete before target transfer",
                DeleteBeforeImportHelp, null, null);
        }

        protected void SetDefaultArguments()
        {
            if (TargetLanguageID == null)
            {
                if ((TargetLanguageIDs != null) && (TargetLanguageIDs.Count() != 0))
                    TargetLanguageID = TargetLanguageIDs[0];
            }

            if (HostLanguageID == null)
            {
                if ((HostLanguageIDs != null) && (HostLanguageIDs.Count() != 0))
                    HostLanguageID = HostLanguageIDs[0];
            }

            if (String.IsNullOrEmpty(TargetRepositoryName))
            {
                string userRepositoryName;
                List<string> userRepositoryDefinitions = ApplicationData.Settings.GetStringList("UserRepositoryDefinitions");

                if ((userRepositoryDefinitions != null) && (userRepositoryDefinitions.Count() != 0))
                    userRepositoryName = userRepositoryDefinitions[0];
                else
                    userRepositoryName = String.Empty;

                if (String.IsNullOrEmpty(TargetRepositoryName))
                {
                    switch (Direction)
                    {
                        case "Import":
                            TargetRepositoryName = ApplicationData.BaseRepositoryName;
                            break;
                        case "Export":
                            TargetRepositoryName = userRepositoryName;
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(StoreName))
            {
                if (!String.IsNullOrEmpty(TargetLabel))
                {
                    switch (TargetLabel)
                    {
                        case "Course":
                            StoreName = "Courses";
                            break;
                    }
                }
                else if (!String.IsNullOrEmpty(TargetType))
                {
                    switch (TargetType)
                    {
                        case "BaseObjectNodeTree":
                            StoreName = "Courses";
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(ObjectName))
            {
                if (TreeHeaderSource != null)
                    ObjectName = TreeHeaderSource.GetTitleString(UILanguageID);
            }
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "ObjectReferenceNodeTree":
                case "MarkupTemplate":
                case "NodeMaster":
                case "ToolProfile":
                case "AudioMultiReference":
                case "PictureReference":
                case "DictionaryEntry":
                case "LanguageDescription":
                case "BaseString":
                case "UserRecord":
                    if (capability == "Support")
                        return true;
                    else if (capability == "Transfer")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string contentName, string capability)
        {
            return IsSupportedStatic(importExport, contentName, capability);
        }

        public static new string TypeStringStatic { get { return "Transfer"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

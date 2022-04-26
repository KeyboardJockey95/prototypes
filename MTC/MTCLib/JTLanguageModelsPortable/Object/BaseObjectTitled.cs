using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObjectTitled : BaseObjectLanguages
    {
        protected Guid _Guid;
        protected MultiLanguageString _Title;
        protected MultiLanguageString _Description;
        protected string _Source;
        protected string _Label;
        protected string _ImageFileName;
        protected MediaStorageState _ImageFileStorageState;
        protected int _Index;
        protected bool _IsPublic;
        protected string _Package;
        // Content directory name.  Just one directory, composed from the title.
        protected string _Directory;
        protected List<MultiLanguageString> _Attributions;

        public BaseObjectTitled(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner)
            : base(key, targetLanguageIDs, hostLanguageIDs, owner)
        {
            _Guid = Guid.Empty;
            _Title = title;
            _Description = description;
            _Source = source;
            _Package = package;
            _Label = label;
            _ImageFileName = imageFileName;
            _ImageFileStorageState = MediaStorageState.Unknown;
            _Index = index;
            _IsPublic = isPublic;
            _Directory = null;
            _Attributions = null;
        }

        public BaseObjectTitled(object key)
            : base(key)
        {
            ClearBaseObjectTitled();
        }

        public BaseObjectTitled(BaseObjectTitled other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectTitled(BaseObjectTitled other, object key)
            : base(key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectTitled(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectTitled()
        {
            ClearBaseObjectTitled();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectTitled();
        }

        public void ClearBaseObjectTitled()
        {
            _Guid = Guid.Empty;
            _Title = null;
            _Description = null;
            _Source = null;
            _Package = null;
            _Label = null;
            _ImageFileName = null;
            _ImageFileStorageState = MediaStorageState.Unknown;
            _Index = -1;
            _IsPublic = false;
            _Directory = null;
            _Attributions = null;
        }

        public void Copy(BaseObjectTitled other)
        {
            base.Copy(other);
            CopyTitledObject(other);
        }

        public void CopyDeep(BaseObjectTitled other)
        {
            CopyTitledObject(other);
            base.CopyDeep(other);
        }

        public void CopyTitledObject(BaseObjectTitled other)
        {
            if (other == null)
            {
                ClearBaseObjectTitled();
                return;
            }

            if (other.Guid != Guid.Empty)
                _Guid = other.Guid;

            if (other.Title != null)
                _Title = new MultiLanguageString(other.Title);
            else
                _Title = null;

            if (other.Description != null)
                _Description = new MultiLanguageString(other.Description);
            else
                _Description = null;

            _Source = other.Source;
            _Package = other.Package;
            _Label = other.Label;
            _ImageFileName = other.ImageFileName;
            _ImageFileStorageState = other.ImageFileStorageState;
            _Index = other.Index;
            _IsPublic = other.IsPublic;
            //_Directory = other.Directory;
            SetupDirectory();
            _Attributions = other.Attributions;
            ModifiedFlag = true;

        }

        public void CopyTitledObjectAndLanguages(BaseObjectTitled other)
        {
            CopyLanguages(other);
            CopyTitledObject(other);
        }

        public void CopyTitledObjectAndLanguagesExpand(BaseObjectTitled other, UserProfile userProfile)
        {
            CopyLanguagesExpand(other, userProfile);
            CopyTitledObject(other);
        }

        public override void CopyLanguages(BaseObjectLanguages other)
        {
            base.CopyLanguages(other);

            List<LanguageID> otherLanguageIDs = other.LanguageIDs;

            if ((otherLanguageIDs != null) && otherLanguageIDs.Contains(LanguageLookup.AllLanguages))
                otherLanguageIDs = LanguageLookup.ExpandLanguageIDs(otherLanguageIDs, null);

            if (Title != null)
                ObjectUtilities.SynchronizeMultiLanguageStringLanguages(Title, String.Empty, otherLanguageIDs);

            if (Description != null)
                ObjectUtilities.SynchronizeMultiLanguageStringLanguages(Title, String.Empty, otherLanguageIDs);
        }

        public override void CopyLanguagesExpand(BaseObjectLanguages other, UserProfile userProfile)
        {
            base.CopyLanguagesExpand(other, userProfile);

            List<LanguageID> otherLanguageIDs = other.ExpandLanguageIDs(userProfile);

            if (Title != null)
                ObjectUtilities.SynchronizeMultiLanguageStringLanguages(Title, String.Empty, otherLanguageIDs);

            if (Description != null)
                ObjectUtilities.SynchronizeMultiLanguageStringLanguages(Title, String.Empty, otherLanguageIDs);
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectTitled(this);
        }

        public override Guid Guid
        {
            get
            {
                return _Guid;
            }
            set
            {
                if (value != _Guid)
                {
                    _Guid = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override bool EnsureGuid()
        {
            bool returnValue;

            if (Guid == Guid.Empty)
            {
                Guid = Guid.NewGuid();
                returnValue = false;
            }
            else
                returnValue = true;

            return returnValue;
        }

        public override void NewGuid()
        {
            _Guid = Guid.NewGuid();
        }

        public MultiLanguageString Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageString CloneTitle()
        {
            if (_Title != null)
                return new MultiLanguageString(_Title);

            return null;
        }

        public virtual string GetTitleString()
        {
            string title;
            if (_Title != null)
            {
                LanguageString titleLS = _Title.LanguageString(LanguageLookup.English);
                if ((titleLS == null) || !titleLS.HasText())
                    title = GetTitleStringFirst();
                else
                    title = titleLS.Text;
            }
            else
                title = "";
            return title;
        }

        public virtual void SetTitleString(string titleString)
        {
            SetTitleString(LanguageLookup.English, titleString);
        }

        public virtual string GetTitleString(LanguageID uiLanguageID)
        {
            if (_Title != null)
            {
                if (uiLanguageID != null)
                    return _Title.Text(uiLanguageID);
                else if (_Title.Count() != 0)
                    return _Title.LanguageString(0).Text;
            }
            return String.Empty;
        }

        public virtual void SetTitleString(LanguageID uiLanguageID, string titleString)
        {
            if (_Title != null)
                _Title.SetText(uiLanguageID, titleString);
            else
                _Title = new MultiLanguageString("Title", uiLanguageID, titleString);
        }

        public virtual string GetTitleStringFirst()
        {
            if ((_Title != null) && (_Title.Count() != 0))
                return _Title.LanguageString(0).Text;
            else
                return "";
        }

        public virtual LanguageID GetTitleLanguageIDFirst()
        {
            if ((_Title != null) && (_Title.Count() != 0))
                return _Title.LanguageString(0).LanguageID;
            else
                return null;
        }

        public static string GetTitleStringListString(List<BaseObjectTitled> objs, LanguageID uiLanguageID)
        {
            if (objs == null)
                return String.Empty;

            if (uiLanguageID == null)
                uiLanguageID = LanguageLookup.English;

            StringBuilder sb = new StringBuilder();

            foreach (BaseObjectTitled obj in objs)
            {
                string title = obj.GetTitleString(uiLanguageID);
                string prefix;

                if (sb.Length != 0)
                    prefix = ", ";
                else
                    prefix = String.Empty;

                if (!String.IsNullOrEmpty(title))
                    sb.Append(prefix + title);
            }

            return sb.ToString();
        }

        public MultiLanguageString Description
        {
            get
            {
                return _Description;
            }
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageString CloneDescription()
        {
            if (_Description != null)
                return new MultiLanguageString(_Description);

            return null;
        }

        public virtual string GetDescriptionString()
        {
            string description;
            if (_Description != null)
            {
                LanguageString descriptionLS = _Description.LanguageString(LanguageLookup.English);
                if ((descriptionLS == null) || !descriptionLS.HasText())
                    description = GetDescriptionStringFirst();
                else
                    description = descriptionLS.Text;
            }
            else
                description = "";
            return description;
        }

        public virtual void SetDescriptionString(string descriptionString)
        {
            SetDescriptionString(LanguageLookup.English, descriptionString);
        }

        public virtual string GetDescriptionString(LanguageID uiLanguageID)
        {
            if (_Description != null)
                return _Description.Text(uiLanguageID);
            else
                return "";
        }

        public virtual void SetDescriptionString(LanguageID uiLanguageID, string descriptionString)
        {
            if (_Description != null)
                _Description.SetText(uiLanguageID, descriptionString);
            else
                _Description = new MultiLanguageString("Description", uiLanguageID, descriptionString);
        }

        public virtual string GetDescriptionStringFirst()
        {
            if ((_Description != null) && (_Description.Count() != 0))
                return _Description.LanguageString(0).Text;
            else
                return "";
        }

        public override string Name
        {
            get
            {
                if ((_Title != null) && (_Title.Count() != 0))
                {
                    LanguageString languageString = _Title.LanguageString(LanguageLookup.English);
                    if (languageString == null)
                        languageString = _Title.LanguageString(0);
                    return languageString.Text;
                }
                return String.Empty;
            }
            set
            {
                if ((_Title != null) && (_Title.Count() != 0))
                {
                    LanguageString languageString = _Title.LanguageString(LanguageLookup.English);
                    if (languageString == null)
                        languageString = _Title.LanguageString(0);
                    languageString.Text = value;
                }
            }
        }

        public string GetName(LanguageID languageID)
        {
            if ((_Title != null) && (_Title.Count() != 0))
            {
                LanguageString languageString = _Title.LanguageString(languageID);
                if (languageString == null)
                    languageString = _Title.LanguageString(0);
                return languageString.Text;
            }
            return String.Empty;
        }

        public override string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                if (_Source != value)
                {
                    _Source = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Package
        {
            get
            {
                return _Package;
            }
            set
            {
                if (_Package != value)
                {
                    _Package = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                if (_Label != value)
                {
                    _Label = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string LabelLower
        {
            get
            {
                if (!String.IsNullOrEmpty(_Label))
                    return _Label.ToLower();
                return String.Empty;
            }
        }

        public string LabelPlural
        {
            get
            {
                if (!String.IsNullOrEmpty(_Label))
                    return _Label + "s";
                return String.Empty;
            }
        }

        public string LabelLowerPlural
        {
            get
            {
                if (!String.IsNullOrEmpty(_Label))
                    return _Label.ToLower() + "s";
                return String.Empty;
            }
        }

        public string ImageFileName
        {
            get
            {
                return _ImageFileName;
            }
            set
            {
                if (value != _ImageFileName)
                {
                    _ImageFileName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ImageFileTildeUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_ImageFileName))
                    return null;
                if (_ImageFileName.StartsWith(".."))
                {
                    string url = MediaTildeUrl + "/" + _ImageFileName;
                    url = MediaUtilities.NormalizeUrlPath(url);
                    return url;
                }
                if (_ImageFileName.Contains(":") || _ImageFileName.Contains("/"))
                    return _ImageFileName;
                string fileUrl = MediaTildeUrl;
                if (String.IsNullOrEmpty(fileUrl))
                    return null;
                if (!fileUrl.EndsWith("/"))
                    fileUrl += "/";
                fileUrl += _ImageFileName;
                return fileUrl;
            }
        }

        public string ImageFileUrl
        {
            get
            {
                string fileUrl = ImageFileTildeUrl;
                if (String.IsNullOrEmpty(fileUrl))
                    return null;
                if (fileUrl.StartsWith("~"))
                    fileUrl = fileUrl.Substring(1);
                return fileUrl;
            }
        }

        public string ImageFileUrlWithMediaCheck
        {
            get
            {
                string url = ImageFileUrl;

                if (!ImageFileIsExternal)
                {
                    if (ApplicationData.IsMobileVersion)
                    {
                        bool changed;
                        ApplicationData.Global.HandleMediaAccess(ref url, ref _ImageFileStorageState, out changed);
                        if (changed)
                            Modified = true;
                    }
                }

                return url;
            }
        }

        public bool ImageFileIsExternal
        {
            get
            {
                if (String.IsNullOrEmpty(_ImageFileName))
                    return false;
                if (_ImageFileName.Contains(":"))
                    return true;
                return false;
            }
        }

        public string ImageFilePath
        {
            get
            {
                string filePath = ImageFileUrl;
                if (String.IsNullOrEmpty(filePath))
                    return null;
                filePath = ApplicationData.MapToFilePath(filePath);
                return filePath;
            }
        }

        public bool ImageFileExists
        {
            get
            {
                if (String.IsNullOrEmpty(_ImageFileName))
                    return false;
                if (ImageFileIsExternal)
                    return true;
                if (FileSingleton.Exists(ImageFilePath))
                    return true;
                return false;
            }
        }

        public bool HasImageFile
        {
            get
            {
                return !String.IsNullOrEmpty(_ImageFileName);
            }
        }

        public MediaStorageState ImageFileStorageState
        {
            get
            {
                return _ImageFileStorageState;
            }
            set
            {
                if (_ImageFileStorageState != value)
                {
                    _ImageFileStorageState = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (_Index != value)
                {
                    _Index = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsPublic
        {
            get
            {
                return _IsPublic;
            }
            set
            {
                if (_IsPublic != value)
                {
                    _IsPublic = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual string Directory
        {
            get
            {
                return _Directory;
            }
            set
            {
                if (_Directory != value)
                {
                    _Directory = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual string MediaTildeUrl
        {
            get
            {
                string mediaTildeUrl = ApplicationData.MediaTildeUrl;

                if (!mediaTildeUrl.EndsWith("/"))
                    mediaTildeUrl += "/";

                mediaTildeUrl += MediaUtilities.FileFriendlyName(Owner) + "/" + Directory;

                return mediaTildeUrl;
            }
        }

        public virtual string MediaDirectoryPath
        {
            get
            {
                try
                {
                    return ApplicationData.MapToFilePath(MediaTildeUrl);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public virtual string ComposeDirectory()
        {
            string directory = MediaUtilities.FileFriendlyName(GetTitleString(), 40);
            if (String.IsNullOrEmpty(directory))
                directory = KeyString;
            return directory;
        }

        public virtual void SetupDirectory()
        {
            string directory = ComposeDirectory();

            if (!String.IsNullOrEmpty(directory))
                UpdateDirectory(directory);

            Directory = directory;
        }

        public virtual void SetupDirectoryCheck()
        {
            if (String.IsNullOrEmpty(Directory))
                SetupDirectory();
        }

        public virtual bool UpdateDirectory(string newDirectory)
        {
            string oldDirectory = Directory;

            if (!String.IsNullOrEmpty(oldDirectory))
            {
                if (newDirectory != oldDirectory)
                {
                    try
                    {
                        string oldPath = ApplicationData.MapToFilePath(MediaTildeUrl);
                        string newPath;

                        if (!String.IsNullOrEmpty(oldPath) && !oldPath.EndsWith(ApplicationData.PlatformPathSeparator))
                            oldPath += ApplicationData.PlatformPathSeparator;

                        if (!String.IsNullOrEmpty(oldDirectory))
                        {
                            int oldDirLength = oldDirectory.Length + 1;  // Add separator.
                            newPath = oldPath.Substring(0, oldPath.Length - oldDirLength) + newDirectory;
                        }
                        else
                            newPath = oldPath + newDirectory;

                        if (!String.IsNullOrEmpty(oldDirectory) && FileSingleton.DirectoryExists(oldPath))
                            FileSingleton.RenameDirectory(oldPath, newPath);
                        else if (!FileSingleton.DirectoryExists(newPath))
                            FileSingleton.CreateDirectory(newPath);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<MultiLanguageString> Attributions
        {
            get
            {
                return _Attributions;
            }
            set
            {
                if (MultiLanguageString.CompareMultiLanguageStringLists(_Attributions, value) != 0)
                    ModifiedFlag = true;
                _Attributions = value;
            }
        }

        public List<MultiLanguageString> CloneAttributions()
        {
            return MultiLanguageString.CopyMultiLanguageStringLists(_Attributions);
        }

        public MultiLanguageString GetAttribution(string key)
        {
            if ((_Attributions != null) && !String.IsNullOrEmpty(key))
                return _Attributions.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public MultiLanguageString GetAttributionIndexed(int index)
        {
            if ((_Attributions != null) && (index >= 0) && (index < _Attributions.Count()))
                return _Attributions[index];
            return null;
        }

        public int GetAttributionIndex(MultiLanguageString attribution)
        {
            if (_Attributions != null)
                return _Attributions.IndexOf(attribution);
            return -1;
        }

        public int GetAttributionIndexKey(string key)
        {
            if (_Attributions != null)
            {
                int index = 0;
                foreach (MultiLanguageString attribution in _Attributions)
                {
                    if (attribution.MatchKey(key))
                        return index;
                    index++;
                }
            }
            return -1;
        }

        public bool AddAttribution(MultiLanguageString attribution)
        {
            if (_Attributions == null)
                _Attributions = new List<MultiLanguageString>(1) { attribution };
            else
                _Attributions.Add(attribution);
            ModifiedFlag = true;
            return true;
        }

        public bool InsertAttribution(int index, MultiLanguageString attribution)
        {
            if (_Attributions == null)
                _Attributions = new List<MultiLanguageString>(1) { attribution };
            else if (index >= _Attributions.Count)
                _Attributions.Add(attribution);
            else
                _Attributions.Insert(index, attribution);
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteAttribution(MultiLanguageString attribution)
        {
            if (_Attributions != null)
            {
                if (_Attributions.Remove(attribution))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteAttributionKey(string key)
        {
            if ((_Attributions != null) && !String.IsNullOrEmpty(key))
            {
                MultiLanguageString attribution = GetAttribution(key);
                if (attribution != null)
                {
                    _Attributions.Remove(attribution);
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteAttributionIndexed(int index)
        {
            if ((_Attributions != null) && (index >= 0) && (index < _Attributions.Count()))
            {
                _Attributions.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllAttributions()
        {
            if (_Attributions != null)
            {
                if (_Attributions.Count() != 0)
                    ModifiedFlag = true;
                _Attributions.Clear();
            }
        }

        public int AttributionCount()
        {
            if (_Attributions != null)
                return (_Attributions.Count());
            return 0;
        }

        public bool HasAttributions()
        {
            if (_Attributions != null)
                return (_Attributions.Count != 0);
            return false;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if ((_Title != null) && _Title.Modified)
                    return true;

                if ((_Description != null) && _Description.Modified)
                    return true;

                if (_Attributions != null)
                {
                    foreach (MultiLanguageString attribution in _Attributions)
                    {
                        if (attribution.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Title != null)
                    _Title.Modified = false;

                if (_Description != null)
                    _Description.Modified = false;

                if (_Attributions != null)
                {
                    foreach (MultiLanguageString attribution in _Attributions)
                        attribution.Modified = false;
                }
            }
        }

        public bool IsVisible(UserProfile userProfile, string targetLanguageCode, string hostLanguageCode)
        {
            if ((targetLanguageCode != null) && !UseTargetLanguage(targetLanguageCode, userProfile))
                return false;

            if ((hostLanguageCode != null) && !UseHostLanguage(hostLanguageCode, userProfile))
                return false;

            return true;
        }

        public bool IsVisible(UserRecord userRecord, UserProfile userProfile, string targetLanguageCode, string hostLanguageCode)
        {
            if (!IsPublic && (Owner != userRecord.UserName) && (Owner != userRecord.Team) && !userRecord.IsAdministrator())
                return false;

            if ((targetLanguageCode != null) && !UseTargetLanguage(targetLanguageCode, userProfile))
                return false;

            if ((hostLanguageCode != null) && !UseHostLanguage(hostLanguageCode, userProfile))
                return false;

            if (!userRecord.HavePackage(_Package) && (Owner != userRecord.UserName) && (Owner != userRecord.Team))
                return false;

            if (userProfile.ShowAllTeachers || (userProfile.Teachers == null))
                return true;
            else
            {
                if (userRecord.UserName == Owner)
                    return true;

                if (!userProfile.ShowAllTeachers && !userProfile.Teachers.Contains(Owner))
                    return false;
            }

            return true;
        }

        public bool IsVisible(UserRecord userRecord, UserProfile userProfile, string targetLanguageCode,
            string hostLanguageCode, string teacher)
        {
            if (!IsPublic && (Owner != userRecord.UserName) && (Owner != userRecord.Team) && !userRecord.IsAdministrator())
                return false;

            if (!String.IsNullOrEmpty(targetLanguageCode) && !UseTargetLanguage(targetLanguageCode, userProfile))
                return false;

            if (!String.IsNullOrEmpty(hostLanguageCode) && !UseHostLanguage(hostLanguageCode, userProfile))
                return false;

            if (!userRecord.HavePackage(_Package) && (Owner != userRecord.UserName) && (Owner != userRecord.Team))
                return false;

            if (teacher == "My Teachers")
            {
                if (userRecord.UserName == Owner)
                    return true;

                if (!userProfile.ShowAllTeachers && !userProfile.Teachers.Contains(Owner))
                    return false;
            }
            else if ((teacher == "Any Teacher") || String.IsNullOrEmpty(teacher))
                return true;
            else if (teacher != Owner)
                return false;

            return true;
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            base.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                mediaFiles, visitFunction);

            if (!String.IsNullOrEmpty(ImageFileName))
            {
                string filePath = ImageFilePath;

                if (visitFunction != null)
                    visitFunction(mediaFiles, this, this, filePath, null);
                else if (mediaFiles != null)
                {
                    if (!mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                }
            }
        }

        public virtual void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
        }

        public virtual bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            return true;
        }

        public virtual bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            return true;
        }

        public virtual bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            return true;
        }

        public virtual void ClearReferences(bool recurseParents, bool recurseChildren)
        {
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Guid != Guid.Empty)
                element.Add(new XAttribute("Guid", _Guid.ToString()));
            if (_Index != -1)
                element.Add(new XAttribute("Index", _Index.ToString()));
            element.Add(new XAttribute("IsPublic", _IsPublic.ToString()));
            if (!String.IsNullOrEmpty(_ImageFileName))
            {
                element.Add(new XAttribute("ImageFileName", _ImageFileName));
                if (_ImageFileStorageState != MediaStorageState.Unknown)
                    element.Add(new XAttribute("ImageFileStorageState", _ImageFileStorageState.ToString()));
            }
            if (_Title != null)
                element.Add(_Title.GetElement("Title"));
            if (_Description != null)
                element.Add(_Description.GetElement("Description"));
            if (!String.IsNullOrEmpty(_Label))
                element.Add(new XAttribute("Label", _Label));
            if (!String.IsNullOrEmpty(_Package))
                element.Add(new XAttribute("Package", _Package));
            if (!String.IsNullOrEmpty(_Source))
                element.Add(new XAttribute("Source", _Source));
            if (!String.IsNullOrEmpty(_Directory))
                element.Add(new XAttribute("Directory", _Directory));
            if ((_Attributions != null) && (_Attributions.Count() != 0))
            {
                XElement opattributionsElement = new XElement("Attributions");
                foreach (MultiLanguageString attribution in _Attributions)
                    opattributionsElement.Add(attribution.GetElement("Attribution"));
                element.Add(opattributionsElement);
            }
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Guid":
                    _Guid = Guid.Parse(attributeValue);
                    break;
                case "Label":
                    _Label = attributeValue;
                    break;
                case "Index":
                    _Index = Convert.ToInt32(attributeValue);
                    break;
                case "IsPublic":
                    _IsPublic = (attributeValue == "True" ? true : false);
                    break;
                case "Package":
                    _Package = attributeValue;
                    break;
                case "ImageFileName":
                    _ImageFileName = attributeValue;
                    break;
                case "ImageFileStorageState":
                    _ImageFileStorageState = ApplicationData.GetStorageStateFromString(attributeValue);
                    break;
                case "Source":
                    _Source = attributeValue;
                    break;
                case "Directory":
                    _Directory = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Title":
                    _Title = new MultiLanguageString(childElement);
                    break;
                case "Description":
                    _Description = new MultiLanguageString(childElement);
                    break;
                case "Attributions":
                    foreach (XElement attributionElement in childElement.Elements())
                    {
                        MultiLanguageString attribution = new MultiLanguageString(attributionElement);
                        AddAttribution(attribution);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectTitled otherBaseObjectTitled = other as BaseObjectTitled;

            if (otherBaseObjectTitled == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = _Guid.CompareTo(otherBaseObjectTitled.Guid);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Label, otherBaseObjectTitled.Label);
            if (diff != 0)
                return diff;
            if (otherBaseObjectTitled == null)
            {
                if ((_Title == null) && (_Description == null))
                    return 0;
                else
                    return 1;
            }
            if (_Title != null)
            {
                diff = _Title.Compare(otherBaseObjectTitled.Title);
                if (diff != 0)
                    return diff;
            }
            if (_Description != null)
            {
                diff = _Description.Compare(otherBaseObjectTitled.Description);
                if (diff != 0)
                    return diff;
            }
            diff = ObjectUtilities.CompareStrings(_Source, otherBaseObjectTitled.Source);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Package, otherBaseObjectTitled.Package);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Label, otherBaseObjectTitled.Label);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_ImageFileName, otherBaseObjectTitled.ImageFileName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Directory, otherBaseObjectTitled.Directory);
            if (diff != 0)
                return diff;
            if (_Index != otherBaseObjectTitled.Index)
                return _Index - otherBaseObjectTitled.Index;
            if (_IsPublic != otherBaseObjectTitled.IsPublic)
                return (_IsPublic ? 1 : -1);
            return diff;
        }

        public static int Compare(BaseObjectTitled object1, BaseObjectTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(BaseObjectTitled object1, BaseObjectTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareIndices(BaseObjectTitled object1, BaseObjectTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Index - object2.Index;
        }

        public static int CompareOwnerThenIndices(BaseObjectTitled object1, BaseObjectTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            int diff = String.Compare(object1.Owner, object2.Owner);
            if (diff != 0)
                return diff;
            return object1.Index - object2.Index;
        }

        public static int CompareTitle(BaseObjectTitled object1, BaseObjectTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            if (object1.Title == null)
                return -1;
            if (object2.Title == null)
                return 1;
            if (object1.Title.LanguageStrings == null)
                return -1;
            if (object2.Title.LanguageStrings == null)
                return 1;
            if (object1.Title.LanguageStrings.Count() == 0)
                return -1;
            if (object2.Title.LanguageStrings.Count() == 0)
                return 1;
            int diff = String.Compare(object1.Title.LanguageStrings[0].Text, object2.Title.LanguageStrings[0].Text);
            return diff;
        }
    }
}

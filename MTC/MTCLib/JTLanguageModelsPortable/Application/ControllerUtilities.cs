using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Service;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Helpers;

namespace JTLanguageModelsPortable.Application
{
    public class ControllerUtilities : TaskUtilities
    {
        public IMainRepository Repositories;
        public IApplicationCookies Cookies;
        public UserRecord UserRecord;
        public UserProfile UserProfile;
        public AnonymousUserRecord AnonymousUserRecord;
        public string ProfileName;
        public string TeamOrUserName;
        public LanguageID UILanguageID;
        public ILanguageTranslator Translator;
        public LanguageUtilities LanguageUtilities;
        public string Message;
        public string Error;
        public object PlatformController;
        public object PlatformContext;
        public int AutomatedMarkupTemplateKey;
        private Dictionary<string, LanguageTool> LanguageToolCache;
        private Dictionary<string, LanguageTool> RemoteLanguageToolCache;

        public ControllerUtilities(IMainRepository repositories, IApplicationCookies cookies,
            UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
            LanguageUtilities languageUtilities)
        {
            ClearControllerUtilities();
            Initialize(repositories, cookies, userRecord, userProfile, translator, languageUtilities);
        }

        public ControllerUtilities(ControllerUtilities other)
        {
            CopyControllerUtilities(other);
        }

        public ControllerUtilities()
        {
            ClearControllerUtilities();
        }

        public void ClearControllerUtilities()
        {
            Repositories = null;
            Cookies = null;
            UserRecord = null;
            UserProfile = null;
            AnonymousUserRecord = null;
            UserName = String.Empty;
            TeamOrUserName = String.Empty;
            UILanguageID = null;
            Translator = null;
            LanguageUtilities = null;
            Message = String.Empty;
            Error = String.Empty;
            PlatformController = null;
            PlatformContext = null;
            AutomatedMarkupTemplateKey = -1;
            LanguageToolCache = null;
            RemoteLanguageToolCache = null;
        }

        public void CopyControllerUtilities(ControllerUtilities other)
        {
            Repositories = other.Repositories;
            Cookies = other.Cookies;
            UserRecord = other.UserRecord;
            UserProfile = other.UserProfile;
            AnonymousUserRecord = other.AnonymousUserRecord;
            UserName = other.UserName;
            ProfileName = other.ProfileName;
            TeamOrUserName = other.TeamOrUserName;
            UILanguageID = LanguageID.CloneLanguageID(other.UILanguageID);
            Translator = other.Translator;
            LanguageUtilities = other.LanguageUtilities;
            Message = other.Message;
            Error = other.Error;
            PlatformController = other.PlatformController;
            PlatformContext = other.PlatformContext;
            AutomatedMarkupTemplateKey = other.AutomatedMarkupTemplateKey;
            LanguageToolCache = null;
            RemoteLanguageToolCache = null;
        }

        public virtual void Initialize(IMainRepository repositories, IApplicationCookies cookies,
            UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
            LanguageUtilities languageUtilities)
        {
            Repositories = repositories;
            Cookies = cookies;
            UserRecord = userRecord;
            UserProfile = userProfile;

            if ((userRecord != null) && (userProfile != null))
            {
                if (userRecord.IsAnonymous())
                    AnonymousUserRecord = userRecord as AnonymousUserRecord;

                UILanguageID = userProfile.UILanguageID;

                if (userProfile.GetUserOption("DefaultAutomatedMarkupTemplateKey") != null)
                {
                    AutomatedMarkupTemplateKey = userProfile.GetUserOptionInteger("DefaultAutomatedMarkupTemplateKey", -1);
                    if (AutomatedMarkupTemplateKey == 0)
                        AutomatedMarkupTemplateKey = ApplicationData.DefaultAutomatedMarkupTemplateKey;
                }
                else
                    AutomatedMarkupTemplateKey = ApplicationData.DefaultAutomatedMarkupTemplateKey;
            }

            Translator = translator;

            if (languageUtilities != null)
            {
                LanguageLookup.LanguageUtilities = LanguageUtilities = languageUtilities;

                if (Translator == null)
                    Translator = languageUtilities.Translator;
            }
            else if (Translator != null)
                LanguageLookup.LanguageUtilities = LanguageUtilities = new LanguageUtilities(
                    Repositories.UIStrings, Repositories.UIText, UILanguageID, Translator, UserRecord, Repositories);

            if (userRecord != null)
            {
                if (userRecord.IsAnonymous())
                {
                    UserName = S("Guest");
                    TeamOrUserName = UserName;
                }
                else
                {
                    UserName = userRecord.UserName;
                    TeamOrUserName = userRecord.TeamOrUserName;

                    DateTime accessed = DateTime.UtcNow;
                    DateTime lastAccessed = userRecord.AccountAccessed;

                    if (lastAccessed.Date != accessed.Date)
                    {
                        userRecord.AccountAccessed = accessed;
                        UpdateUserRecord(userRecord);
                    }
                }
            }

            if (UserProfile != null)
                ProfileName = UserProfile.ProfileName;
        }

        public ClientServiceBase RemoteClient
        {
            get
            {
                return ApplicationData.RemoteClient;
            }
        }

        public ClientMainRepository RemoteRepositories
        {
            get
            {
                return ApplicationData.RemoteRepositories;
            }
        }

        public bool IsAnonymous
        {
            get { return UserRecord.IsAnonymous(); }
        }

        public bool IsAdministrator
        {
            get { return UserRecord.IsAdministrator(); }
        }

        public bool IsTeacher
        {
            get { return UserRecord.IsTeacher(); }
        }

        public bool IsStudent
        {
            get { return UserRecord.IsStudent(); }
        }

        public bool HasMessage
        {
            get
            {
                return !String.IsNullOrEmpty(Message);
            }
        }

        public bool HasError
        {
            get
            {
                return !String.IsNullOrEmpty(Error);
            }
        }

        public bool HasMessageOrError
        {
            get
            {
                return !String.IsNullOrEmpty(Error) || !String.IsNullOrEmpty(Message);
            }
        }

        public virtual void UpdateError(string error)
        {
        }

        public virtual void UpdateError()
        {
        }

        public virtual void UpdateMessage(string message)
        {
        }

        public virtual void UpdateMessage()
        {
        }

        public virtual void UpdateMessageAndError(string message, string error)
        {
            UpdateMessage(message);
            UpdateError(error);
        }

        public virtual void UpdateMessageAndError()
        {
            UpdateMessage();
            UpdateError();
        }

        public string UserNameOrGuest()
        {
            UserRecord userRecord = UserRecord;
            if (userRecord == null)
                return "(null)";
            if (userRecord.IsAnonymous())
                return S("Guest");
            else
                return SafeString(userRecord.UserName);
        }

        public string GetHelpText(string key, string defaultText)
        {
            return GetUIText(key, defaultText);
        }

        public string GetUIText(string key, string defaultText)
        {
            string text = defaultText;

            if (String.IsNullOrEmpty(key))
                return text;

            try
            {
                BaseString targetObject = Repositories.UIText.Get(key, UILanguageID);

                if ((targetObject != null) && !String.IsNullOrEmpty(targetObject.Text))
                    return targetObject.Text;

                BaseString englishObject = Repositories.UIText.Get(key, LanguageLookup.English);

                if (englishObject == null)
                {
                    if (!String.IsNullOrEmpty(text))
                    {
                        englishObject = new BaseString(key, text);

                        if (!Repositories.UIText.Add(englishObject, LanguageLookup.English))
                            Error = "Error adding English UI text: " + key;
                    }
                    //else if (LanguageUtilities != null)
                    //    return LanguageUtilities.TranslateUIString("(no text)");
                    //else
                    //    return "(no text)";
                    else
                        return String.Empty;
                }
                else if (String.IsNullOrEmpty(englishObject.Text))
                {
                    if (LanguageUtilities != null)
                        return LanguageUtilities.TranslateUIString("(no text)");
                    else
                        // return "(no text)";
                        return String.Empty;
                }

                if (UILanguageID == LanguageLookup.English)
                    return englishObject.Text;

                if (Translator == null)
                    return "(no translator)";

                LanguageTranslatorSource translatorSource;
                string error;

                if (Translator.TranslateString(
                    "UITranslation",
                    "UIText",
                    key,
                    englishObject.Text,
                    LanguageLookup.English,
                    UILanguageID,
                    out text,
                    out translatorSource,
                    out error))
                {
                    if (targetObject == null)
                    {
                        targetObject = new BaseString(key, text);

                        if (!Repositories.UIText.Add(targetObject, UILanguageID))
                            Error = "Error adding " + UILanguageID.LanguageCultureExtensionCode + " UI text: " + key;
                    }
                    else
                    {
                        targetObject.Text = text;
                        targetObject.TouchAndClearModified();

                        if (!Repositories.UIText.Update(targetObject, UILanguageID))
                            Error = "Error updating " + UILanguageID.LanguageCultureExtensionCode + " UI text: " + key;
                    }
                }
                else
                {
                    Error = error;
                    return defaultText;
                }
            }
            catch (Exception)
            {
            }

            return text;
        }

        public bool UpdateUserRecord(UserRecord userRecord)
        {
            bool returnValue = false;

            try
            {
                userRecord.ModifiedTime = DateTime.UtcNow;
                userRecord.Modified = false;

                if (userRecord.IsAnonymous())
                    returnValue = UpdateAnonymousUserRecord(userRecord as AnonymousUserRecord);
                else
                {
                    if (Repositories.UserRecords.Update(userRecord))
                        returnValue = true;
                    else
                    {
                        Error = S("Error updating user record for ") + userRecord.UserName + ".";
                    }
                }
            }
            catch (Exception exception)
            {
                Error = S("Error updating user record for ") + userRecord.UserName + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + " " + exception.InnerException.Message;
            }

            return returnValue;
        }

        protected bool UpdateUserRecord()
        {
            return UpdateUserRecord(UserRecord);
        }

        protected bool UpdateUserRecordCheck(UserRecord userRecord)
        {
            bool returnValue = true;

            if (userRecord == null)
                userRecord = UserRecord;

            if (userRecord.Modified)
                returnValue = UpdateUserRecord(userRecord);

            return true;
        }

        protected bool UpdateUserRecordCheck()
        {
            return UpdateUserRecordCheck(UserRecord);
        }

        protected bool UpdateAnonymousUserRecord(AnonymousUserRecord userRecord)
        {
            Cookies.UserProfile = userRecord.CurrentUserProfile;
            return true;
        }

        protected bool UpdateAnonymousUserRecord()
        {
            Cookies.UserProfile = AnonymousUserRecord.CurrentUserProfile;
            return true;
        }

        public bool UpdateUserProfile(UserProfile userProfile)
        {
            bool returnValue = UpdateUserRecord();

            UILanguageID = userProfile.UILanguageID;

            LanguageLookup.LanguageUtilities = LanguageUtilities = new LanguageUtilities(
                Repositories.UIStrings, Repositories.UIText, UILanguageID, Translator, UserRecord, Repositories);

            return returnValue;
        }

        public bool UpdateUserProfile()
        {
            return UpdateUserProfile(UserProfile);
        }

        public bool UpdateUserProfileCheck(UserProfile userProfile)
        {
            if (userProfile == null)
                return false;

            if (userProfile.Modified)
                return UpdateUserProfile(userProfile);

            return false;
        }

        public bool UpdateUserProfileCheck()
        {
            return UpdateUserProfileCheck(UserProfile);
        }

        public bool HasUserOption(string key)
        {
            bool value = UserProfile.HasUserOption(key);
            return value;
        }

        public bool DeleteUserOption(string key)
        {
            return UserProfile.DeleteUserOption(key);
        }

        public string GetUserOptionString(string key, string defaultValue = null)
        {
            string value = UserProfile.GetUserOptionString(key, defaultValue);
            return value;
        }

        // Returns if the value changed or was set for the first time.
        public bool SetUserOptionString(string key, string value)
        {
            string oldValue = UserProfile.GetUserOptionString(key, null);
            bool returnValue = (oldValue != value);
            UserProfile.SetUserOptionString(key, value);
            return returnValue;
        }

        // Returns if the value changed or was set for the first time.
        public bool SetUserOptionStringDefaulted(string key, string value, string defaultValue = null)
        {
            string oldValue = UserProfile.GetUserOptionString(key, null);
            bool returnValue = (oldValue != value);

            if (!String.IsNullOrEmpty(value))
                UserProfile.SetUserOptionString(key, value);
            else
                UserProfile.SetUserOptionString(key, defaultValue);

            return returnValue;
        }

        public string GetSetUserOptionString(string key, string setValue, string defaultValue = null)
        {
            if (!String.IsNullOrEmpty(setValue))
            {
                SetUserOptionString(key, setValue);
                return setValue;
            }

            string value = GetUserOptionString(key, defaultValue);
            return value;
        }

        public int GetUserOptionInteger(string key, int defaultValue = 0)
        {
            int value = UserProfile.GetUserOptionInteger(key, defaultValue);
            return value;
        }

        public int GetSetUserOptionInteger(string key, int setValue, int defaultValue = 0, int notSetValue = 0)
        {
            if (setValue != notSetValue)
            {
                SetUserOptionInteger(key, setValue);
                return setValue;
            }

            int value = GetUserOptionInteger(key, defaultValue);
            return value;
        }

        // Returns if the value changed or was set for the first time.
        public bool SetUserOptionInteger(string key, int value)
        {
            int oldValue = UserProfile.GetUserOptionInteger(key, -1);
            bool returnValue = (oldValue != value);
            UserProfile.SetUserOptionInteger(key, value);
            return returnValue;
        }

        // Returns if the value changed or was set for the first time.
        public bool SetUserOptionIntegerDefaulted(string key, int value, int defaultValue = 0)
        {
            int oldValue = UserProfile.GetUserOptionInteger(key, -1);
            bool returnValue = (oldValue != value);
            UserProfile.SetUserOptionInteger(key, defaultValue);
            return returnValue;
        }

        public bool GetUserOptionFlag(string key, bool defaultValue = false)
        {
            bool value = UserProfile.GetUserOptionFlag(key, defaultValue);
            return value;
        }

        // Returns old value.
        public bool SetUserOptionFlag(string key, bool value)
        {
            bool returnValue = UserProfile.GetUserOptionFlag(key, false);
            UserProfile.SetUserOptionFlag(key, value);
            return returnValue;
        }

        // Returns new value.
        public bool ToggleUserOptionFlag(string key, bool defaultValue)
        {
            bool returnValue = !UserProfile.GetUserOptionFlag(key, !defaultValue);
            UserProfile.SetUserOptionFlag(key, returnValue);
            return returnValue;
        }

        public List<int> GetUserOptionIntList(string key, List<int> defaultValue)
        {
            string str = UserProfile.GetUserOptionString(key);

            if (!String.IsNullOrEmpty(str))
                return TextUtilities.GetIntListFromString(str);

            return defaultValue;
        }

        public void SetUserOptionIntList(string key, List<int> value)
        {
            string str = TextUtilities.GetStringFromIntList(value);
            UserProfile.SetUserOptionString(key, str);
        }

        public List<string> GetUserOptionStringList(string key)
        {
            string str = UserProfile.GetUserOptionString(key);

            if (!String.IsNullOrEmpty(str))
                return TextUtilities.GetStringListFromString(str);

            return null;
        }

        public void SetUserOptionStringList(string key, List<string> value)
        {
            string str = TextUtilities.GetStringFromStringList(value);
            UserProfile.SetUserOptionString(key, str);
        }

        public Dictionary<string, bool> GetUserOptionFlagDictionary(string key, Dictionary<string, bool> value = null)
        {
            string str = UserProfile.GetUserOptionString(key);

            if (!String.IsNullOrEmpty(str))
                return TextUtilities.GetFlagDictionaryFromString(str, value);

            if (value == null)
                value = new Dictionary<string, bool>();

            return value;
        }

        public void SetUserOptionIntFlagDictionary(string key, Dictionary<int, bool> value)
        {
            string str = TextUtilities.GetStringFromIntFlagDictionary(value);
            UserProfile.SetUserOptionString(key, str);
        }

        public Dictionary<int, bool> GetUserOptionIntFlagDictionary(string key, Dictionary<int, bool> value = null)
        {
            string str = UserProfile.GetUserOptionString(key);

            if (!String.IsNullOrEmpty(str))
                return TextUtilities.GetIntFlagDictionaryFromString(str, value);

            if (value == null)
                value = new Dictionary<int, bool>();

            return value;
        }

        public void SetUserOptionFlagDictionary(string key, Dictionary<string, bool> value)
        {
            string str = TextUtilities.GetStringFromFlagDictionary(value);
            UserProfile.SetUserOptionString(key, str);
        }

        public LanguageID GetUserOptionLanguageID(string key, LanguageID defaultValue)
        {
            string defValue;
            if (defaultValue == null)
                defValue = String.Empty;
            else
                defValue = defaultValue.LanguageCultureExtensionCode;
            string value = UserProfile.GetUserOptionString(key, defValue);
            LanguageID returnValue = defaultValue;
            if (!String.IsNullOrEmpty(value))
                returnValue = LanguageLookup.GetLanguageIDNoAdd(value);
            return returnValue;
        }

        public LanguageID GetSetUserOptionLanguageID(string key, LanguageID setValue, LanguageID defaultValue, LanguageID notSetValue = null)
        {
            if (setValue != notSetValue)
            {
                SetUserOptionLanguageID(key, setValue);
                return setValue;
            }

            LanguageID value = GetUserOptionLanguageID(key, defaultValue);
            return value;
        }

        public bool SetUserOptionLanguageID(string key, LanguageID value)
        {
            string oldValue = UserProfile.GetUserOptionString(key);
            string newValue = (value != null ? value.LanguageCultureExtensionCode : String.Empty);
            bool returnValue = (oldValue != newValue);
            UserProfile.SetUserOptionString(key, newValue);
            return returnValue;
        }

        public DateTime GetUserOptionDateTime(string key, DateTime defaultValue)
        {
            string value = UserProfile.GetUserOptionString(key, defaultValue.ToString());
            DateTime returnValue = defaultValue;
            if (!String.IsNullOrEmpty(value))
            {
                try
                {
                    returnValue = Convert.ToDateTime(value);
                }
                catch
                {
                }
            }
            return returnValue;
        }

        // Returns if the value changed or was set for the first time.
        public bool SetUserOptionDateTime(string key, DateTime value)
        {
            string oldValue = UserProfile.GetUserOptionString(key);
            string newValue = value.ToString();
            bool returnValue = (oldValue != newValue);
            UserProfile.SetUserOptionString(key, newValue);
            return returnValue;
        }

        public List<LanguageDescriptor> GetLanguageDescriptors(BaseObjectLanguages languagesItem,
            string targetLanguageCode, string hostLanguageCode)
        {
            List<LanguageDescriptor> languageDescriptors;
            LanguageDescriptor languageDescriptor;
            List<LanguageID> languageIDs;
            List<LanguageID> targetLanguageIDs;
            List<LanguageID> hostLanguageIDs;
            string languageDescriptorKey = null;

            if (languagesItem is BaseObjectContent)
            {
                languageDescriptorKey = ((BaseObjectContent)languagesItem).ContentType;

                BaseObjectContent content = languagesItem as BaseObjectContent;

                if (content.ContentClass == ContentClassType.MediaItem)
                    languagesItem = content.NodeOrTree;
            }

            if (languagesItem != null)
            {
                languageIDs = languagesItem.ExpandLanguageIDs(UserProfile);
                targetLanguageIDs = languagesItem.ExpandTargetLanguageIDs(UserProfile);
                hostLanguageIDs = languagesItem.ExpandHostLanguageIDs(UserProfile);
            }
            else
            {
                languageIDs = UserProfile.LanguageIDs;
                targetLanguageIDs = UserProfile.TargetLanguageIDs;
                hostLanguageIDs = UserProfile.HostLanguageIDs;
            }

            if ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0))
                hostLanguageIDs = UserProfile.HostLanguageIDs;

            if (String.IsNullOrEmpty(targetLanguageCode) || (targetLanguageCode == "(any)") || (targetLanguageCode == "(all languages)"))
                languageDescriptors = LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Target", languageIDs);
            else if ((targetLanguageCode == "(my languages)") || (targetLanguageCode == "(target languages)"))
                languageDescriptors = UserProfile.CloneTargetLanguageDescriptors(targetLanguageIDs, "Target");
            else if (targetLanguageCode == "(host languages)")
                languageDescriptors = UserProfile.CloneHostLanguageDescriptors(hostLanguageIDs, "Target");
            else
            {
                languageDescriptors = new List<LanguageDescriptor>();
                languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Target", LanguageLookup.GetLanguageIDNoAdd(targetLanguageCode));
                languageDescriptor.Show = UserProfile.GetShowState(languageDescriptorKey, languageDescriptor.Name, languageDescriptor.LanguageID.LanguageCultureExtensionCode);
                languageDescriptors.Add(languageDescriptor);
            }

            if (String.IsNullOrEmpty(hostLanguageCode) || (hostLanguageCode == "(any)") || (hostLanguageCode == "(all languages)"))
                languageDescriptors.AddRange(LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Host", languageIDs));
            else if ((hostLanguageCode == "(my languages)") || (hostLanguageCode == "(host languages)"))
                languageDescriptors.AddRange(UserProfile.CloneHostLanguageDescriptors(hostLanguageIDs, "Host"));
            else if (hostLanguageCode == "(target languages)")
                languageDescriptors.AddRange(UserProfile.CloneTargetLanguageDescriptors(targetLanguageIDs, "Host"));
            else
            {
                languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Host", LanguageLookup.GetLanguageIDNoAdd(hostLanguageCode));
                languageDescriptor.Show = UserProfile.GetShowState(languageDescriptorKey, languageDescriptor.Name, languageDescriptor.LanguageID.LanguageCultureExtensionCode);
                languageDescriptors.Add(languageDescriptor);
            }

            languageDescriptors = LanguageDescriptor.FilterDuplicates(languageDescriptors, false);
            UserProfile.UpdateShowStates(languageDescriptorKey, languageDescriptors);

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetTargetLanguageDescriptors(BaseObjectLanguages languagesItem,
            string targetLanguageCode)
        {
            List<LanguageDescriptor> languageDescriptors;
            LanguageDescriptor languageDescriptor;
            List<LanguageID> languageIDs;
            string languageDescriptorKey = null;

            if (languagesItem is BaseObjectContent)
                languageDescriptorKey = ((BaseObjectContent)languagesItem).ContentType;

            if (languagesItem != null)
                languageIDs = languagesItem.ExpandLanguageIDs(UserProfile);
            else
                languageIDs = UserProfile.LanguageIDs;

            if (String.IsNullOrEmpty(targetLanguageCode) || (targetLanguageCode == "(any)") || (targetLanguageCode == "(all languages)"))
                languageDescriptors = LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Target", languageIDs);
            else if ((targetLanguageCode == "(my languages)") || (targetLanguageCode == "(target languages)"))
                languageDescriptors = UserProfile.CloneTargetLanguageDescriptors(languageIDs, "Target");
            else if (targetLanguageCode == "(host languages)")
                languageDescriptors = UserProfile.CloneHostLanguageDescriptors(languageIDs, "Target");
            else
            {
                languageDescriptors = new List<LanguageDescriptor>();
                languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Target", LanguageLookup.GetLanguageIDNoAdd(targetLanguageCode));
                languageDescriptor.Show = UserProfile.GetShowState(languageDescriptorKey, languageDescriptor.Name, languageDescriptor.LanguageID.LanguageCultureExtensionCode);
                languageDescriptors.Add(languageDescriptor);
            }

            languageDescriptors = LanguageDescriptor.FilterDuplicates(languageDescriptors, false);
            UserProfile.UpdateShowStates(languageDescriptorKey, languageDescriptors);

            return languageDescriptors;
        }

        public List<LanguageDescriptor> GetHostLanguageDescriptors(BaseObjectLanguages languagesItem,
            string hostLanguageCode)
        {
            List<LanguageDescriptor> languageDescriptors;
            LanguageDescriptor languageDescriptor;
            List<LanguageID> languageIDs;
            string languageDescriptorKey = null;

            if (languagesItem is BaseObjectContent)
                languageDescriptorKey = ((BaseObjectContent)languagesItem).ContentType;

            if (languagesItem != null)
                languageIDs = languagesItem.ExpandLanguageIDs(UserProfile);
            else
                languageIDs = UserProfile.LanguageIDs;

            if (String.IsNullOrEmpty(hostLanguageCode) || (hostLanguageCode == "(any)") || (hostLanguageCode == "(all languages)"))
                languageDescriptors = LanguageDescriptor.LanguageDescriptorsFromLanguageIDs("Host", languageIDs);
            else if ((hostLanguageCode == "(my languages)") || (hostLanguageCode == "(host languages)"))
                languageDescriptors = UserProfile.CloneHostLanguageDescriptors(languageIDs, "Host");
            else if (hostLanguageCode == "(target languages)")
                languageDescriptors = UserProfile.CloneTargetLanguageDescriptors(languageIDs, "Host");
            else
            {
                languageDescriptors = new List<LanguageDescriptor>();
                languageDescriptor = LanguageDescriptor.LanguageDescriptorFromLanguageID("Host", LanguageLookup.GetLanguageIDNoAdd(hostLanguageCode));
                languageDescriptor.Show = UserProfile.GetShowState(languageDescriptorKey, languageDescriptor.Name, languageDescriptor.LanguageID.LanguageCultureExtensionCode);
                languageDescriptors.Add(languageDescriptor);
            }

            languageDescriptors = LanguageDescriptor.FilterDuplicates(languageDescriptors, true);
            UserProfile.UpdateShowStates(languageDescriptorKey, languageDescriptors);

            return languageDescriptors;
        }

        public virtual MultiLanguageTool GetMultiLanguageTool(
            LanguageID targetLanguageID,
            List<LanguageID> hostLanguageIDs)
        {
            List<LanguageTool> targetLanguageTools = new List<LanguageTool>() { GetLanguageTool(targetLanguageID) };
            List<LanguageTool> hostLanguageTools = GetLanguageTools(hostLanguageIDs);
            MultiLanguageTool multiLanguageTool = new MultiLanguageTool(
                targetLanguageTools,
                hostLanguageTools);
            return multiLanguageTool;
        }

        public List<LanguageTool> GetLanguageTools(List<LanguageID> languageIDs)
        {
            List<LanguageID> uniqueLanguageIDs = LanguageLookup.GetFamilyRootLanguageIDs(languageIDs);
            List<LanguageTool> languageTools = new List<LanguageTool>();

            foreach (LanguageID languageID in uniqueLanguageIDs)
            {
                LanguageTool languageTool = GetLanguageTool(languageID);

                if (languageTool != null)
                    languageTools.Add(languageTool);
            }

            return languageTools;
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
                languageTool.InitializeWordFixes(null);
            }

            LanguageToolCache.Add(languageID.LanguageCode, languageTool);

            return languageTool;
        }

        public virtual LanguageTool GetLanguageToolWithSentenceFixes(LanguageID languageID, BaseObjectContent content)
        {
            LanguageTool languageTool = GetLanguageTool(languageID);

            if ((languageTool != null) && (content != null))
            {
                string sentenceFixesKey = null;

                if (languageTool.SentenceFixes == null)
                {
                    if ((sentenceFixesKey = content.GetInheritedOptionValue("SentenceFixesKey")) != null)
                        languageTool.InitializeSentenceFixes(sentenceFixesKey);
                }
            }

            return languageTool;
        }

        public virtual LanguageTool GetLanguageToolWithWordFixes(LanguageID languageID, BaseObjectContent content)
        {
            LanguageTool languageTool = GetLanguageTool(languageID);

            if ((languageTool != null) && (content != null))
            {
                string wordFixesKey = null;

                if (languageTool.WordFixes == null)
                {
                    if ((wordFixesKey = content.GetInheritedOptionValue("WordFixesKey")) != null)
                        languageTool.InitializeWordFixes(wordFixesKey);
                }
            }

            return languageTool;
        }

        public virtual LanguageTool GetLanguageToolWithSentenceAndWordFixes(LanguageID languageID, BaseObjectContent content)
        {
            LanguageTool languageTool = GetLanguageTool(languageID);

            if ((languageTool != null) && (content != null))
            {
                string sentenceFixesKey = null;
                string wordFixesKey = null;

                if (languageTool.SentenceFixes == null)
                {
                    if ((sentenceFixesKey = content.GetInheritedOptionValue("SentenceFixesKey")) != null)
                        languageTool.InitializeSentenceFixes(sentenceFixesKey);
                }

                if (languageTool.WordFixes == null)
                {
                    if ((wordFixesKey = content.GetInheritedOptionValue("WordFixesKey")) != null)
                        languageTool.InitializeWordFixes(wordFixesKey);
                }
            }

            return languageTool;
        }

        public virtual LanguageTool GetLanguageToolForRemote(LanguageID languageID)
        {
            LanguageTool languageTool;

            if (languageID == null)
                return null;

            if (RemoteLanguageToolCache == null)
                RemoteLanguageToolCache = new Dictionary<string, LanguageTool>();
            else if (RemoteLanguageToolCache.TryGetValue(languageID.LanguageCode, out languageTool))
                return languageTool;

            languageTool = ApplicationData.LanguageTools.Create(languageID);

            if (languageTool == null)
                languageTool = new GenericLanguageTool(languageID, UserProfile.HostLanguageIDs, UserProfile.LanguageIDs);
            else
            {
                languageTool.HostLanguageIDs = UserProfile.HostLanguageIDs;
                languageTool.UserLanguageIDs = UserProfile.LanguageIDs;
            }

            languageTool.DictionaryDatabase = RemoteRepositories.Dictionary;

            RemoteLanguageToolCache.Add(languageID.LanguageCode, languageTool);

            return languageTool;
        }

        public List<LanguageTool> GetLanguageToolsForRemote(List<LanguageID> languageIDs)
        {
            List<LanguageID> uniqueLanguageIDs = LanguageLookup.GetFamilyRootLanguageIDs(languageIDs);
            List<LanguageTool> languageTools = new List<LanguageTool>();

            foreach (LanguageID languageID in uniqueLanguageIDs)
            {
                LanguageTool languageTool = GetLanguageToolForRemote(languageID);

                if (languageTool != null)
                    languageTools.Add(languageTool);
            }

            return languageTools;
        }

        public virtual MultiLanguageTool GetMultiLanguageToolForRemote(
            LanguageID targetLanguageID,
            List<LanguageID> hostLanguageIDs)
        {
            List<LanguageTool> targetLanguageTools = new List<LanguageTool>() { GetLanguageToolForRemote(targetLanguageID) };
            List<LanguageTool> hostLanguageTools = GetLanguageToolsForRemote(hostLanguageIDs);
            MultiLanguageTool multiLanguageTool = new MultiLanguageTool(
                targetLanguageTools,
                hostLanguageTools);
            return multiLanguageTool;
        }

        protected MultiLanguageString CreateMultiLanguageString(object key, string defaultValue)
        {
            return ObjectUtilities.CreateMultiLanguageString(key, defaultValue, UserProfile);
        }

        protected MultiLanguageString CreateMultiLanguageString(object key, string defaultValue, LanguageID languageID)
        {
            LanguageString languageString = new LanguageString(key, UILanguageID, defaultValue);
            MultiLanguageString multiLanguageString = new MultiLanguageString(key, languageString);
            return multiLanguageString;
        }

        protected MultiLanguageString CreateUIMultiLanguageString(object key, string defaultValue)
        {
            return CreateMultiLanguageString(key, defaultValue, UILanguageID);
        }

        protected MultiLanguageString CreateAndTranslateMultiLanguageString(object key, string defaultValue)
        {
            MultiLanguageString multiLanguageString = ObjectUtilities.CreateMultiLanguageString(key, defaultValue, UserProfile);
            Translate(multiLanguageString);
            return multiLanguageString;
        }

        protected void TranslateCheck(MultiLanguageString multiLanguageString, bool isTranslate)
        {
            if ((multiLanguageString != null) && isTranslate)
            {
                string message;
                if (!LanguageUtilities.Translator.TranslateMultiLanguageString(multiLanguageString, UserProfile.LanguageIDs, out message, false))
                    Error = message;
            }
        }

        protected void Translate(MultiLanguageString multiLanguageString)
        {
            if (multiLanguageString != null)
            {
                string message;
                if (!LanguageUtilities.Translator.TranslateMultiLanguageString(multiLanguageString, UserProfile.LanguageIDs, out message, false))
                    Error = message;
            }
        }

        public bool GetRemoteMediaFilesFromFilePaths(
            List<string> mediaFiles,
            bool overwrite,
            LanguageUtilities languageUtilities,
            out string errorMessage)
        {
            int count = mediaFiles.Count + 2;
            int index = 0;
            List<string> savedFiles = new List<string>(count);
            bool returnValue = true;

            errorMessage = String.Empty;

            if (ApplicationData.Global.IsConnectedToANetwork())
            {
                ContinueProgress(count);
                UpdateProgressElapsed("Downloading media files...");

                foreach (string filePath in mediaFiles)
                {
                    if (ProgressCancelCheck() || IsCanceled())
                    {
                        errorMessage = errorMessage
                            + S("Operation cancelled by user.") + "\n";
                        returnValue = false;
                        break;
                    }

                    string remoteUrl = ApplicationData.GetRemoteMediaUrlFromFilePath(filePath);
                    string fileName = MediaUtilities.GetFileName(remoteUrl);

                    if (overwrite || !FileSingleton.Exists(filePath))
                    {
                        UpdateProgressElapsed("Getting: " + fileName);

                        if (ApplicationData.Global.GetRemoteMediaFile(remoteUrl, filePath, ref errorMessage))
                            savedFiles.Add(filePath);
                        else
                            returnValue = false;
                    }

                    index++;
                }

                UpdateProgressElapsed("Updating file bookkeeping...");
                ApplicationData.Global.BookkeepMediaFlushCache();

                //ProgressOperation_Dispatch(ProgressMode.Stop, index, null);
                //// Let PostHandleRequest operation stop the progress.
                //ProgressOperation_Dispatch(ProgressMode.Update, index, "Finished server communication.");
            }
            else
            {
                errorMessage = languageUtilities.TranslateUIString(
                    "Sorry, can't download files because there is no network connection.");
                returnValue = false;
            }

            return returnValue;
        }

        // Make sure we are not anonymous or a bogus user.
        public bool ValidateUser(string actionName)
        {
            if (UserRecord == null)
            {
                Error = S("Sorry, something is wrong with your authorization, so I can't show you this page.");
                return false;
            }

            if (UserRecord.IsAnonymous() && (UserRecord.UserName != "johnAdministrator") && (UserRecord.UserName != "johnTeacher") &&
                (UserRecord.UserName != "johnKorean"))
            {
                Error = S("Sorry, you need to log in or register an account to see this page or do this action.  Registering is free!");
                return false;
            }

            return true;
        }

        // Make sure we are not anonymous or a bogus user.
        public bool ValidateUserOrMobile(string actionName)
        {
            if (UserRecord == null)
            {
                Error = S("Sorry, something is wrong with your authorization, so I can't show you this page.");
                return false;
            }

            if (!ApplicationData.IsMobileVersion && !ApplicationData.IsTestMobileVersion)
            {
                if (UserRecord.IsAnonymous() && (UserRecord.UserName != "johnAdministrator") && (UserRecord.UserName != "johnTeacher") &&
                    (UserRecord.UserName != "johnKorean"))
                {
                    Error = S("Sorry, you need to log in or register an account to see this page or do this action.  Registering is free!");
                    return false;
                }
            }

            return true;
        }

        // Make sure we are not anonymous or a bogus user and have at least the indicated role.
        public bool ValidateUser(string role, string actionName)
        {
            if (!ValidateUser(actionName))
            {
                Error = S("Sorry, you need to be a registered user and a " + role + " to see this page or do this action.");
                return false;
            }

            if (UserRecord.UserName == ApplicationData.ApplicationName)
                return true;

            if (!UserRecord.HaveRole(role))
            {
                Error = S("Sorry, you need to be a " + role + " to see this page or do this action.");
                return false;
            }

            return true;
        }

        // Make sure we are an administrator.
        public bool ValidateAdministrator(string actionName)
        {
            return ValidateUser("administrator", actionName);
        }

        // Make sure we are a teacher or administrator.
        public bool ValidateTeacherOrAdministrator(string actionName)
        {
            if (UserRecord.UserName == ApplicationData.ApplicationName)
                return true;

            if (!UserRecord.HaveRole("teacher") && !UserRecord.HaveRole("administrator"))
            {
                Error = S("Sorry, you need to be a teacher or administrator to see this page or do this action.");
                return false;
            }

            return true;
        }

        // Make sure we are mobile or an administrator.
        public bool ValidateMobileOrAdministrator(string actionName)
        {
            if (!ApplicationData.IsMobileVersion && !ApplicationData.IsTestMobileVersion)
            {
                if (!UserRecord.HaveRole("administrator"))
                {
                    Error = S("Sorry, you need to be a administrator to see this page or do this action.");
                    return false;
                }
            }

            return true;
        }

        // Make sure we are mobile.
        public bool ValidateMobile(string actionName)
        {
            if (!ApplicationData.IsMobileVersion && !ApplicationData.IsTestMobileVersion)
            {
                Error = S("Sorry, this feature is only for mobile devices.");
                return false;
            }

            return true;
        }

        // Make sure we own an item, or are administrator.
        public bool ValidateOwner(BaseObjectKeyed item)
        {
            if (UserRecord.IsAdministrator() || (UserRecord.UserName == "johnAdministrator") || (UserRecord.UserName == "johnTeacher") ||
                    (UserRecord.UserName == "johnKorean"))
                return true;

            if ((item == null) || (item.Owner == UserRecord.UserName) || (item.Owner == UserRecord.Team))
                return true;

            return false;
        }

        // Make sure we own an item, or are administrator.
        public bool ValidateOwner(string owner)
        {
            if (UserRecord.IsAdministrator() || (UserRecord.UserName == "johnAdministrator") || (UserRecord.UserName == "johnTeacher") ||
                    (UserRecord.UserName == "johnKorean"))
                return true;

            if (String.IsNullOrEmpty(owner) || (owner == UserRecord.UserName) || (owner == UserRecord.Team))
                return true;

            return false;
        }

        // Make sure we own an item, or are administrator, or we are mobile.
        public bool ValidateOwnerOrMobile(string owner)
        {
            if (UserRecord.IsAdministrator() || (UserRecord.UserName == "johnAdministrator") || (UserRecord.UserName == "johnTeacher") ||
                    (UserRecord.UserName == "johnKorean"))
                return true;

            if (String.IsNullOrEmpty(owner) || (owner == UserRecord.UserNameOrGuest) || (owner == UserRecord.Team))
                return true;

            return false;
        }

        // Make sure we own an item, or are a specific role of user.
        public bool ValidateOwnerOrUser(BaseObjectLanguages item, string role, string actionName)
        {
            if (ValidateOwner(item))
                return true;

            if (ValidateUser(role, actionName))
                return true;

            return false;
        }

        // Make sure we own an item, or are a specific role of user.
        public bool ValidateOwnerOrUser(string owner, string role, string actionName)
        {
            if (ValidateOwner(owner))
                return true;

            if (ValidateUser(role, actionName))
                return true;

            return false;
        }

        // Make sure we are logged in and own an item.
        public bool ValidateOwnerOrUser(BaseObjectLanguages item, string actionName)
        {
            if (!ValidateUser(actionName))
                return false;

            if (!ValidateOwner(item))
                return false;

            return true;
        }

        // Make sure we are logged in and the owner.
        public bool ValidateOwnerOrUser(string owner, string actionName)
        {
            if (!ValidateUser(actionName))
                return false;

            if (!ValidateOwner(owner))
                return false;

            return false;
        }

        // Make sure we own an item, or are a teacher.
        public bool ValidateOwnerOrTeacher(BaseObjectLanguages item, string actionName)
        {
            if (ValidateOwner(item))
                return true;

            if (ValidateUser("teacher", actionName) || ValidateUser("administrator", actionName))
                return true;

            return false;
        }

        // Make sure we own an item, or are a teacher.
        public bool ValidateOwnerOrTeacher(string owner, string actionName)
        {
            if (ValidateOwner(owner))
                return true;

            if (ValidateUser("teacher", actionName) || ValidateUser("administrator", actionName))
                return true;

            return false;
        }

        // Make sure we own an item, or are an administrator.
        public bool ValidateOwnerOrAdministrator(BaseObjectLanguages item, string actionName)
        {
            if (ValidateOwner(item))
                return true;

            if (ValidateUser("administrator", actionName))
                return true;

            Error = S("Sorry, you need to be the owner or an administrator to see this page to do this action.");

            return false;
        }

        // Make sure we own an item, or are an administrator.
        public bool ValidateOwnerOrAdministrator(string owner, string actionName)
        {
            if (ValidateOwner(owner))
                return true;

            if (ValidateUser("administrator", actionName))
                return true;

            Error = S("Sorry, you need to be the owner or an administrator to see this page to do this action.");

            return false;
        }

        // Make sure we own an item, or are administrator.
        public bool ValidatePackage(BaseObjectTitled item)
        {
            if (UserRecord.IsAdministrator() || (UserRecord.UserName == "johnAdministrator") || (UserRecord.UserName == "johnTeacher") ||
                    (UserRecord.UserName == "johnKorean"))
                return true;

            if ((item == null) || String.IsNullOrEmpty(item.Package) || UserRecord.HavePackage(item.Package))
                return true;

            return false;
        }

        // Make sure we own an item, or are administrator.
        public bool ValidatePackage(string package)
        {
            if (UserRecord.IsAdministrator() || (UserRecord.UserName == "johnAdministrator") || (UserRecord.UserName == "johnTeacher") ||
                    (UserRecord.UserName == "johnKorean"))
                return true;

            if (String.IsNullOrEmpty(package) || UserRecord.HavePackage(package))
                return true;

            return false;
        }

        public virtual void PutMessage(string message)
        {
            string str = S(message);

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
            string fullMessage = S(message) + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = fullMessage;
            else if (!Error.Contains(fullMessage))
                Error = Error + "\r\n" + fullMessage;
        }

        public void PutExceptionError(string message, string argument, Exception exc)
        {
            string fullMessage = S(message) + ": " + argument + ": " + exc.Message;

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
                str = S(fieldName) + ": " + S(message) + ": " + argument;
            else
                str = S(message) + ": " + argument;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutError(string fieldName, string message)
        {
            string str;

            if (!String.IsNullOrEmpty(fieldName))
                str = S(fieldName) + ": " + S(message);
            else
                str = S(message);

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutError(string message)
        {
            string str = S(message);

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PutErrorArgument(string message, string arg1)
        {
            string str = S(message) + ": " + arg1;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public virtual void PrefixMessageOrError(string prefix)
        {
            string str = S(prefix) + ": ";

            if (!String.IsNullOrEmpty(Error))
                Error = str + Error;

            if (!String.IsNullOrEmpty(Message))
                Message = str + Message;
        }

        public virtual void AppendMessage(string message)
        {
            string str = S(message);

            if (!String.IsNullOrEmpty(Message))
                Message = Message + "\r\n" + str;
            else
                Message = str;
        }

        public virtual void AppendMessage(string message, string arg1)
        {
            string str = S(message) + ": " + arg1;

            if (!String.IsNullOrEmpty(Message))
                Message = Message + "\r\n" + str;
            else
                Message = str;
        }

        public virtual void AppendError(string error)
        {
            string str = S(error);

            if (!String.IsNullOrEmpty(Error))
                Error = Error + "\r\n" + str;
            else
                Error = str;
        }

        public virtual void AppendError(string message, string arg1)
        {
            string str = S(message) + ": " + arg1;

            if (!String.IsNullOrEmpty(Error))
                Error = Error + "\r\n" + str;
            else
                Error = str;
        }

        public virtual void AppendExceptionError(string error, Exception exception)
        {
            string message = S(error) + ": " + exception.Message;

            if (exception.InnerException != null)
                message += ":\n    " + exception.InnerException.Message;
        }

        public string AppendMessageOrError(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (value.StartsWith("!"))
                {
                    value = value.Substring(1);

                    if (!String.IsNullOrEmpty(Error))
                    {
                        string str = S(value);

                        if (!Error.Contains(str))
                            AppendError(str);
                    }
                    else
                        Error = S(value);
                }
                else
                {
                    if (!String.IsNullOrEmpty(Message))
                    {
                        string str = S(value);

                        if (!Message.Contains(str))
                            AppendMessage(str);
                    }
                    else
                        Message = S(value);
                }
            }

            return MessageOrError;
        }

        public string MessageOrError
        {
            get
            {
                if (!String.IsNullOrEmpty(Error))
                    return "!" + Error;
                else
                    return Message;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (value.StartsWith("!"))
                    {
                        value = value.Substring(1);

                        if (!String.IsNullOrEmpty(Error))
                        {
                            if (!Error.Contains(value))
                                Error = Error + "\n" + value;
                        }
                        else
                            Error = value;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(Message))
                        {
                            if (!Message.Contains(value))
                                Message = Message + "\n" + value;
                        }
                        else
                            Message = value;
                    }
                }
            }
        }

        protected void PostError(string errorMessage, string action)
        {
            Repositories.Log(errorMessage, action, UserRecord);
            if (errorMessage.Length > 256)
                errorMessage = errorMessage.Substring(0, 256);
            Error = errorMessage;
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

        public string UI(MultiLanguageString multiLanguageString)
        {
            if (multiLanguageString == null)
                return String.Empty;

            LanguageString languageString = multiLanguageString.LanguageString(UILanguageID);

            if (languageString != null)
                return languageString.Text;

            languageString = multiLanguageString.LanguageStringFuzzy(UILanguageID);

            if (languageString != null)
                return languageString.Text;

            return H(multiLanguageString);
        }

        public string H(MultiLanguageString multiLanguageString)
        {
            if (multiLanguageString == null)
                return String.Empty;

            LanguageString languageString = multiLanguageString.LanguageString(UserProfile.HostLanguageID);

            if (languageString != null)
                return languageString.Text;

            languageString = multiLanguageString.LanguageStringFuzzy(UserProfile.HostLanguageID);

            if (languageString != null)
                return languageString.Text;

            return "(oops)";
        }

        public string SafeString(string text)
        {
            if (text != null)
            {
                text = text.Replace("<", "&lt;");
                text = text.Replace(">", "&gt;");
            }
            else
                text = String.Empty;
            return text;
        }
    }
}

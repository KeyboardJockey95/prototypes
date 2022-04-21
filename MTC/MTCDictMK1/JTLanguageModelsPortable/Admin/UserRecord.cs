using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Admin
{
    public enum UserStanding
    {
        Active,
        Monitor,
        Block
    }

    public class UserRecord : BaseObjectTitled
    {
        protected string _RealName;
        protected string _Email;
        protected byte[] _EncryptedPassword;
        protected string _UserRole;
        public static string[] UserRoles = { "administrator", "teacher", "student" };
        protected UserStanding _Standing;
        protected string _ProfileImageFileName;
        protected MultiLanguageString _AboutMe;
        protected bool _IsMinor;
        protected ObjectReference<BaseObjectKeyed> _MarkupReference;
        protected string _Team;
        protected List<string> _Packages;
        protected List<string> _WatchedBlogs;
        protected List<string> _BlockedUsers;
        protected string _TimeZoneID;
        protected TimeSpan _TimeZoneOffset;
        protected DateTime _AccountCreated;
        protected DateTime _AccountAccessed;
        protected int _UsageCount;   // 1 per day
        protected string _IPAddress;
        protected bool _IsHidden;
        protected List<UserProfile> _UserProfiles;
        protected UserProfile _CurrentUserProfile;
        protected int _ProfileOrdinal;
        protected List<BaseString> _UserOptions;
        protected List<Quota> _Quotas;

        public UserRecord(
                string userName,
                string realName,
                string email,
                string userRole,
                UserStanding standing,
                LanguageDescriptor uiLanguageDescriptor,
                List<LanguageDescriptor> hostLanguageDescriptors,
                List<LanguageDescriptor> targetLanguageDescriptors,
                string profileImageFileName,
                MultiLanguageString title,
                MultiLanguageString description,
                MultiLanguageString aboutMe,
                bool isMinor)
            : base(userName)
        {
            ClearUserRecord();
            _RealName = realName;
            _Email = email;
            _UserRole = userRole;
            _Standing = standing;
            _ProfileImageFileName = profileImageFileName;
            _AboutMe = aboutMe;
            _IsMinor = isMinor;
            _CurrentUserProfile = new UserProfile("Default", uiLanguageDescriptor, hostLanguageDescriptors, targetLanguageDescriptors);
            _UserProfiles = new List<UserProfile>(1) { _CurrentUserProfile };
            _ProfileOrdinal = 1;
            Title = title;
            Description = description;
        }

        public UserRecord(
                string userName,
                List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs,
                LanguageID uiLanguageID)
            : base(userName)
        {
            ClearUserRecord();
            LanguageDescriptor uiLanguageDescriptor = new LanguageDescriptor(
                "UI",
                uiLanguageID,
                true);
            List<LanguageDescriptor> targetLanguageDescriptors =
                LanguageDescriptor.GetLanguageDescriptorsFromLanguageIDs(targetLanguageIDs, "Target");
            List<LanguageDescriptor> hostLanguageDescriptors =
                LanguageDescriptor.GetLanguageDescriptorsFromLanguageIDs(hostLanguageIDs, "Host");
            _CurrentUserProfile = new UserProfile(
                "Default",
                uiLanguageDescriptor,
                hostLanguageDescriptors,
                targetLanguageDescriptors);
            _UserProfiles = new List<UserProfile>(1) { _CurrentUserProfile };
        }

        public UserRecord(string userName)
        {
            ClearUserRecord();
            UserName = userName;
            ModifiedFlag = false;
        }

        public UserRecord(UserRecord other)
            : base(other)
        {
            ClearUserRecord();
            Copy(other);
            ModifiedFlag = false;
        }

        public UserRecord(XElement element)
        {
            OnElement(element);
        }

        public UserRecord()
        {
            ClearUserRecord();
        }

        public override void Clear()
        {
            ClearUserRecord();
            base.Clear();
        }

        public void ClearUserRecord()
        {
            _RealName = null;
            _Email = null;
            _EncryptedPassword = null;
            _UserRole = "student";
            _Standing = UserStanding.Active;
            _ProfileImageFileName = null;
            _AboutMe = null;
            _IsMinor = false;
            _MarkupReference = null;
            _Team = null;
            _Packages = null;
            _WatchedBlogs = null;
            _BlockedUsers = null;
            _TimeZoneID = null;
            _TimeZoneOffset = TimeSpan.Zero;
            _AccountCreated = DateTime.MinValue;
            _AccountAccessed = DateTime.MinValue;
            _UsageCount = 0;
            _IPAddress = "";
            _IsHidden = false;
            _UserProfiles = null;
            _CurrentUserProfile = null;
            _ProfileOrdinal = 0;
            _UserOptions = new List<BaseString>();
            _Quotas = new List<Quota>();
        }

        public void Copy(UserRecord other)
        {
            base.Copy(other);
            RealName = other.RealName;
            Email = other.Email;
            UserRole = other.UserRole;
            Standing = other.Standing;
            _IsMinor = other.IsMinor;
            ProfileImageFileName = other.ProfileImageFileName;
            if (other.AboutMe != null)
                AboutMe = other.AboutMe;
            else
                AboutMe = null;
            if (other.MarkupReference != null)
                _MarkupReference = new ObjectReference<BaseObjectKeyed>(other.MarkupTemplateKey, "MarkupTemplates", other.MarkupTemplate);
            else
                _MarkupReference = null;
            _Team = other.Team;
            if (other.Packages != null)
                _Packages = new List<string>(other.Packages);
            else
                _Packages = null;
            if (other.WatchedBlogs != null)
                WatchedBlogs = new List<string>(other.WatchedBlogs);
            else
                WatchedBlogs = null;
            if (other.BlockedUsers != null)
                BlockedUsers = new List<string>(other.BlockedUsers);
            else
                BlockedUsers = null;
            TimeZoneID = other.TimeZoneID;
            TimeZoneOffset = other.TimeZoneOffset;
            AccountCreated = other.AccountCreated;
            AccountAccessed = other.AccountAccessed;
            UsageCount = other.UsageCount;
            IPAddress = other.IPAddress;
            _IsHidden = other.IsHidden;

            if (other._UserProfiles != null)
                _UserProfiles = ObjectUtilities.CopyTypedBaseObjectList<UserProfile>(other.UserProfiles);
            else
                _UserProfiles = null;

            _ProfileOrdinal = other.ProfileOrdinal;

            if (other.CurrentUserProfile != null)
                _CurrentUserProfile = FindUserProfile(other.CurrentUserProfile.ProfileName);
            else
                _CurrentUserProfile = null;

            _UserOptions = BaseString.CopyBaseStrings(other.UserOptions);
            _Quotas = other.CloneQuotas();
        }

        public void CopyAnonymous(UserRecord other)
        {
            if (other == null)
                return;
            ProfileImageFileName = other.ProfileImageFileName;
            if (other.MarkupReference != null)
                _MarkupReference = new ObjectReference<BaseObjectKeyed>(other.MarkupTemplateKey, "MarkupTemplates", other.MarkupTemplate);
            else
                _MarkupReference = null;
            _Team = other.Team;
            if (other.Packages != null)
                _Packages = new List<string>(other.Packages);
            else
                _Packages = null;
            if (other.WatchedBlogs != null)
                WatchedBlogs = new List<string>(other.WatchedBlogs);
            else
                WatchedBlogs = null;
            if (other.BlockedUsers != null)
                BlockedUsers = new List<string>(other.BlockedUsers);
            else
                BlockedUsers = null;

            if (other._UserProfiles != null)
                _UserProfiles = ObjectUtilities.CopyTypedBaseObjectList<UserProfile>(other.UserProfiles);
            else
                _UserProfiles = null;

            if (other.CurrentUserProfile != null)
                _CurrentUserProfile = FindUserProfile(other.CurrentUserProfile.ProfileName);
            else
                _CurrentUserProfile = null;
        }

        public string UserName
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public virtual string UserNameOrGuest
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public string RealName
        {
            get
            {
                return _RealName;
            }
            set
            {
                if (value != _RealName)
                {
                    _RealName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                if (value != _Email)
                {
                    _Email = value;
                    ModifiedFlag = true;
                }
            }
        }

        public byte[] EncryptedPassword
        {
            get
            {
                return _EncryptedPassword;
            }
            set
            {
                if (value != _EncryptedPassword)
                {
                    _EncryptedPassword = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string UserRole
        {
            get
            {
                return _UserRole;
            }
            set
            {
                if (value != _UserRole)
                {
                    _UserRole = value;
                    ModifiedFlag = true;
                }
            }
        }

        public UserStanding Standing
        {
            get
            {
                return _Standing;
            }
            set
            {
                if (value != _Standing)
                {
                    _Standing = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ProfileImageFileName
        {
            get
            {
                return _ProfileImageFileName;
            }
            set
            {
                if (value != _ProfileImageFileName)
                {
                    _ProfileImageFileName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageString AboutMe
        {
            get
            {
                return _AboutMe;
            }
            set
            {
                if (value != _AboutMe)
                {
                    _AboutMe = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsMinor
        {
            get
            {
                return _IsMinor;
            }
            set
            {
                if (value != _IsMinor)
                {
                    _IsMinor = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ObjectReference<BaseObjectKeyed> MarkupReference
        {
            get
            {
                return _MarkupReference;
            }
            set
            {
                if (value != _MarkupReference)
                {
                    _MarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public object MarkupTemplateKey
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.Key;
                return null;
            }
            set
            {
                if (value != MarkupTemplate)
                    MarkupReference = new ObjectReference<BaseObjectKeyed>(value, "MarkupTemplates", null);
            }
        }

        public BaseObjectKeyed MarkupTemplate
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.Item;
                return null;
            }
            set
            {
                if (value != MarkupTemplate)
                    MarkupReference = new ObjectReference<BaseObjectKeyed>(value.Key, "MarkupTemplates", value);
            }
        }

        public string Team
        {
            get
            {
                return _Team;
            }
            set
            {
                if (value != _Team)
                {
                    _Team = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string TeamOrUserName
        {
            get
            {
                if (!String.IsNullOrEmpty(_Team))
                    return _Team;
                return UserName;
            }
        }

        public List<string> Packages
        {
            get
            {
                return _Packages;
            }
            set
            {
                if (value != _Packages)
                {
                    _Packages = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HavePackage(string package)
        {
            if (String.IsNullOrEmpty(package))
                return true;

            if (_Packages == null)
                return false;

            return _Packages.Contains(package);
        }

        public bool AddPackage(string package)
        {
            if (String.IsNullOrEmpty(package))
                return false;

            if (_Packages == null)
                _Packages = new List<string>();
            else if (HavePackage(package))
                return false;

            _Packages.Add(package);
            ModifiedFlag = true;

            return true;
        }

        public string PackageCodes
        {
            get
            {
                if (_Packages != null)
                    return ObjectUtilities.GetStringFromStringList(_Packages);
                else
                    return String.Empty;
            }
        }

        public List<string> WatchedBlogs
        {
            get
            {
                return _WatchedBlogs;
            }
            set
            {
                if (value != _WatchedBlogs)
                {
                    _WatchedBlogs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void DeleteAllWatchedBlogs()
        {
            if (_WatchedBlogs != null)
            {
                _WatchedBlogs = null;
                ModifiedFlag = true;
            }
        }

        public List<string> BlockedUsers
        {
            get
            {
                return _BlockedUsers;
            }
            set
            {
                if (value != _BlockedUsers)
                {
                    _BlockedUsers = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void DeleteAllBlockedUsers()
        {
            if (_BlockedUsers != null)
            {
                _BlockedUsers = null;
                ModifiedFlag = true;
            }
        }

        public string TimeZoneID
        {
            get
            {
                return _TimeZoneID;
            }
            set
            {
                if (value != _TimeZoneID)
                {
                    _TimeZoneID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public TimeSpan TimeZoneOffset
        {
            get
            {
                return _TimeZoneOffset;
            }
            set
            {
                if (value != _TimeZoneOffset)
                {
                    _TimeZoneOffset = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime GetLocalTime(DateTime utcTime)
        {
            if (utcTime != DateTime.MinValue)
            {
                TimeZoneInfo timeZoneInfo;

                if (ApplicationData.IsMobileVersion)
                    timeZoneInfo = ApplicationData.Global.GetLocalTimeZone();
                else
                    timeZoneInfo = ApplicationData.Global.FindSystemTimeZoneById(_TimeZoneID);

                TimeSpan timeZoneOffset = timeZoneInfo.BaseUtcOffset;
                DateTime localTime = utcTime + _TimeZoneOffset;

                if (timeZoneInfo.IsDaylightSavingTime(localTime))
                    localTime = localTime.AddHours(1);

                string timeZoneID = ApplicationData.Global.GetTimeZoneID(timeZoneInfo);

                if (_TimeZoneID != timeZoneID)
                    TimeZoneID = timeZoneID;

                return localTime;
            }

            return utcTime;
        }

        public DateTime AccountCreated
        {
            get
            {
                return _AccountCreated;
            }
            set
            {
                if (value != _AccountCreated)
                {
                    _AccountCreated = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime AccountAccessed
        {
            get
            {
                return _AccountAccessed;
            }
            set
            {
                if (value != _AccountAccessed)
                {
                    _AccountAccessed = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int UsageCount
        {
            get
            {
                return _UsageCount;
            }
            set
            {
                if (value != _UsageCount)
                {
                    _UsageCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string IPAddress
        {
            get
            {
                return _IPAddress;
            }
            set
            {
                if (value != _IPAddress)
                {
                    _IPAddress = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool Touch(string ipAddress)
        {
            bool returnValue = false;

            DateTime now = DateTime.UtcNow;

            if (_AccountAccessed == DateTime.MinValue)
            {
                _AccountCreated = now;
                _AccountAccessed = now;
                _UsageCount = 1;
                ModifiedFlag = true;
                returnValue = true;
            }
            else if (_AccountAccessed.Date != now.Date)
            {
                _AccountAccessed = now;
                _UsageCount++;
                ModifiedFlag = true;
                returnValue = true;
            }
            else
                AccountAccessed = now;

            if (_IPAddress != ipAddress)
            {
                _IPAddress = ipAddress;
                ModifiedFlag = true;
                returnValue = true;
            }

            return returnValue;
        }

        public bool IsHidden
        {
            get
            {
                return _IsHidden;
            }
            set
            {
                if (_IsHidden != value)
                {
                    _IsHidden = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<UserProfile> UserProfiles
        {
            get
            {
                return _UserProfiles;
            }
            set
            {
                if (_UserProfiles != null)
                {
                    _UserProfiles = value;
                    ModifiedFlag = true;
                }
            }
        }

        public UserProfile CurrentUserProfile
        {
            get
            {
                if (_CurrentUserProfile == null)
                {
                    if ((_UserProfiles != null) && (_UserProfiles.Count != 0))
                        _CurrentUserProfile = _UserProfiles.First();
                    else
                    {
                        LanguageID uiLanguageID;

                        if (ApplicationData.GetCurrentUILanguageID != null)
                            uiLanguageID = ApplicationData.GetCurrentUILanguageID();
                        else
                            uiLanguageID = LanguageLookup.English;

                        LanguageDescriptor uiLanguageDescriptor = new LanguageDescriptor("UI", uiLanguageID, true, true);
                        LanguageDescriptor hostLanguageDescriptor = new LanguageDescriptor("Host", uiLanguageID, true, true);
                        List<LanguageDescriptor> hostLanguageDescriptors = new List<LanguageDescriptor>(1) { hostLanguageDescriptor };
                        List<LanguageDescriptor> targetLanguageDescriptors = new List<LanguageDescriptor>(1);

                        UserProfile userProfile = 
                            new UserProfile(
                                "Default", uiLanguageDescriptor, hostLanguageDescriptors, targetLanguageDescriptors);
                        AddUserProfile(userProfile);
                        _CurrentUserProfile = userProfile;
                    }
                    ModifiedFlag = true;
                }

                return _CurrentUserProfile;
            }
            set
            {
                if (_CurrentUserProfile != value)
                {
                    _CurrentUserProfile = value;
                    ModifiedFlag = true;
                }
            }
        }

        public UserProfile FindUserProfile(string profileName)
        {
            if (_UserProfiles != null)
                return _UserProfiles.FirstOrDefault(x => x.ProfileName == profileName);

            return null;
        }

        public UserProfile FindOtherUserProfile(string profileName)
        {
            if (_UserProfiles != null)
                return _UserProfiles.FirstOrDefault(x => (x.ProfileName == profileName)
                    && (x != _CurrentUserProfile));

            return null;
        }

        public void AddUserProfile(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            if (_UserProfiles == null)
            {
                _ProfileOrdinal = 1;
                userProfile.ProfileOrdinal = _ProfileOrdinal;
                _UserProfiles = new List<UserProfile>(1) { userProfile };
            }
            else
            {
                _ProfileOrdinal++;
                userProfile.ProfileOrdinal = _ProfileOrdinal;
                _UserProfiles.Add(userProfile);
            }

            ModifiedFlag = true;
        }

        public bool DeleteUserProfile(UserProfile userProfile)
        {
            if (_UserProfiles != null)
            {
                if (_UserProfiles.Remove(userProfile))
                {
                    if (_UserProfiles.Count != 0)
                        _CurrentUserProfile = _UserProfiles.First();
                    else
                    {
                        _CurrentUserProfile = null;
                        UserProfile newProfile = CurrentUserProfile;
                    }

                    return true;
                }
            }

            return false;
        }

        public void DeleteAllUserProfiles()
        {
            if (_UserProfiles != null)
                _UserProfiles.Clear();

            _CurrentUserProfile = null;
            UserProfile newProfile = CurrentUserProfile;
        }

        public int UserProfileCount()
        {
            if (_UserProfiles != null)
                return _UserProfiles.Count();

            return 0;
        }


        public int ProfileOrdinal
        {
            get
            {
                return _ProfileOrdinal;
            }
            set
            {
                if (value != _ProfileOrdinal)
                {
                    _ProfileOrdinal = value;
                    ModifiedFlag = false;
                }
            }
        }

        public List<string> GetProfileNames()
        {
            List<string> profileNames = new List<string>();

            if (_UserProfiles != null)
            {
                foreach (UserProfile userProfile in _UserProfiles)
                    profileNames.Add(userProfile.ProfileName);
            }

            return profileNames;
        }

        public virtual bool IsAnonymous()
        {
            return false;
        }

        public bool IsAdministrator()
        {
            if (!String.IsNullOrEmpty(UserRole))
                return (UserRole.ToLower() == "administrator");

            return false;
        }

        public bool IsTeacher()
        {
            if (!String.IsNullOrEmpty(UserRole))
                return (UserRole.ToLower() == "teacher");

            return false;
        }

        public bool IsStudent()
        {
            if (!String.IsNullOrEmpty(UserRole))
                return (UserRole.ToLower() == "student");

            return false;
        }

        public bool HaveRole(string role)
        {
            if (IsAnonymous())
                return false;

            if (String.IsNullOrEmpty(role))
                return false;

            if (role.ToLower() == "administrator")
                return IsAdministrator();

            if (role.ToLower() == "teacher")
                return IsTeacher() || IsAdministrator();

            if (role.ToLower() == "student")
                return IsStudent() || IsTeacher() || IsAdministrator();

            return false;
        }

        public void UpdateTargetLanguageIDs()
        {
            if (_TargetLanguageIDs != null)
                _TargetLanguageIDs.Clear();
            else
                _TargetLanguageIDs = new List<LanguageID>();

            if (_UserProfiles != null)
            {
                foreach (UserProfile userProfile in _UserProfiles)
                {
                    List<LanguageID> languageIDs = userProfile.TargetLanguageIDs;

                    if (languageIDs != null)
                    {
                        foreach (LanguageID languageID in languageIDs)
                        {
                            if (!_TargetLanguageIDs.Contains(languageID))
                                _TargetLanguageIDs.Add(languageID);
                        }
                    }
                }
            }
        }

        public override List<LanguageID> TargetLanguageIDs
        {
            get
            {
                UpdateTargetLanguageIDs();
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

        public void UpdateHostLanguageIDs()
        {
            if (_HostLanguageIDs != null)
                _HostLanguageIDs.Clear();
            else
                _HostLanguageIDs = new List<LanguageID>();

            if (_UserProfiles != null)
            {
                foreach (UserProfile userProfile in _UserProfiles)
                {
                    List<LanguageID> languageIDs = userProfile.HostLanguageIDs;

                    if (languageIDs != null)
                    {
                        foreach (LanguageID languageID in languageIDs)
                        {
                            if (!_HostLanguageIDs.Contains(languageID))
                                _HostLanguageIDs.Add(languageID);
                        }
                    }
                }
            }
        }

        public override List<LanguageID> HostLanguageIDs
        {
            get
            {
                UpdateHostLanguageIDs();
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

        public List<BaseString> UserOptions
        {
            get
            {
                return _UserOptions;
            }
            set
            {
                if (value != _UserOptions)
                {
                    if (value != null)
                        _UserOptions = value;
                    else
                        _UserOptions = new List<BaseString>();

                    ModifiedFlag = true;
                }
            }
        }

        public List<BaseString> CloneUserOptions()
        {
            List<BaseString> userOptions = null;

            if (_UserOptions != null)
                userOptions = new List<BaseString>(_UserOptions);

            return userOptions;
        }

        public bool HasUserOption(string key)
        {
            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key) != null;
            return false;
        }

        public BaseString GetUserOption(string key)
        {
            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public void SetUserOption(BaseString option)
        {
            if (_UserOptions == null)
            {
                _UserOptions = new List<BaseString>(1) { option };
                ModifiedFlag = true;
            }
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == option.KeyString);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(option);
                    ModifiedFlag = true;
                }
                else if (!String.IsNullOrEmpty(option.Text))
                {
                    if (oldUserOption.Text != option.Text)
                    {
                        oldUserOption.Text = option.Text;
                        ModifiedFlag = true;
                    }
                }
                else
                {
                    _UserOptions.Remove(oldUserOption);
                    ModifiedFlag = true;
                }
            }
        }

        public string GetUserOptionString(string key, string defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if ((option != null) && (option.Text != null))
                return option.Text;
            if (defaultValue == null)
                return String.Empty;
            return defaultValue;
        }

        public void SetUserOptionString(string key, string value)
        {
            if (_UserOptions == null)
            {
                _UserOptions = new List<BaseString>(1) { new BaseString(key, value) };
                ModifiedFlag = true;
            }
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == key);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(new BaseString(key, value));
                    ModifiedFlag = true;
                }
                else
                {
                    if (oldUserOption.Text != value)
                    {
                        oldUserOption.Text = value;
                        ModifiedFlag = true;
                    }
                }
            }
        }

        public int GetUserOptionInteger(string key, int defaultValue = 0)
        {
            string stringValue = GetUserOptionString(key);
            int integerValue = defaultValue;

            if (ObjectUtilities.IsNumberString(stringValue))
            {
                try
                {
                    integerValue = Convert.ToInt32(stringValue);
                }
                catch
                {
                }
            }

            return integerValue;
        }

        public void SetUserOptionInteger(string key, int value)
        {
            SetUserOptionString(key, value.ToString());
        }

        public float GetUserOptionFloat(string key, float defaultValue = 0)
        {
            string stringValue = GetUserOptionString(key);
            float floatValue = defaultValue;

            if (ObjectUtilities.IsFloatString(stringValue))
            {
                try
                {
                    floatValue = (float)Convert.ToDouble(stringValue);
                }
                catch
                {
                }
            }

            return floatValue;
        }

        public void SetUserOptionFloat(string key, float value)
        {
            SetUserOptionString(key, value.ToString());
        }

        public bool GetUserOptionFlag(string key, bool defaultValue = false)
        {
            switch (GetUserOptionString(key).ToLower())
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return defaultValue;
            }
        }

        public void SetUserOptionFlag(string key, bool value)
        {
            SetUserOptionString(key, (value ? "true" : "false"));
        }

        public LanguageID GetUserOptionLanguageID(string key, LanguageID defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return LanguageLookup.GetLanguageIDNoAdd(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringList(string key, List<string> value)
        {
            SetUserOptionString(key, (value != null ? TextUtilities.GetStringFromStringList(value) : String.Empty));
        }

        public List<string> GetUserOptionStringList(string key, List<string> defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringListFromString(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringDictionary(string key, Dictionary<string, string> value)
        {
            SetUserOptionString(key, (value != null ? TextUtilities.GetStringFromStringDictionary(value) : String.Empty));
        }

        public Dictionary<string, string> GetUserOptionStringDictionary(string key, Dictionary<string, string> defaultValue = null)
        {
            BaseString option = GetUserOption(key);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringDictionaryFromString(oldValue, defaultValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionLanguageID(string key, LanguageID value)
        {
            SetUserOptionString(key, (value != null ? value.LanguageCultureExtensionCode : String.Empty));
        }

        public bool DeleteUserOption(string key)
        {
            if (_UserOptions == null)
                return false;

            BaseString option = GetUserOption(key);

            if (option == null)
                return false;

            ModifiedFlag = true;

            return _UserOptions.Remove(option);
        }

        public void DeleteAllUserOptions()
        {
            _UserOptions = null;

            if (_UserProfiles != null)
            {
                foreach (UserProfile userProfile in _UserProfiles)
                    userProfile.DeleteAllUserOptions();
            }

            ModifiedFlag = true;
        }

        public List<Quota> Quotas
        {
            get
            {
                return _Quotas;
            }
            set
            {
                if (value != _Quotas)
                {
                    if (value != null)
                        _Quotas = value;
                    else
                        _Quotas = new List<Quota>();

                    ModifiedFlag = true;
                }
            }
        }

        public List<Quota> CloneQuotas()
        {
            List<Quota> userOptions = null;

            if (_Quotas != null)
                userOptions = new List<Quota>(_Quotas);

            return userOptions;
        }

        public bool HasQuota(string key)
        {
            if (_Quotas != null)
                return _Quotas.FirstOrDefault(x => x.KeyString == key) != null;
            return false;
        }

        public Quota GetQuota(string key)
        {
            if (_Quotas != null)
                return _Quotas.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public void SetQuota(Quota quota)
        {
            if (_Quotas == null)
            {
                _Quotas = new List<Quota>(1) { quota };
                ModifiedFlag = true;
            }
            else
            {
                Quota oldQuota = _Quotas.FirstOrDefault(x => x.KeyString == quota.KeyString);

                if (oldQuota == null)
                    _Quotas.Add(quota);
                else
                    oldQuota.Copy(quota);

                ModifiedFlag = true;
            }
        }

        public int GetQuotaLimit(string key, int defaultValue = 0)
        {
            Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);
            int limit = defaultValue;

            if (quota != null)
                limit = quota.Limit;

            return limit;
        }

        public void SetQuotaLimit(string key, int value)
        {
            if (_Quotas == null)
            {
                _Quotas = new List<Quota>(1) { new Quota(key, value, 0, 0, DateTime.MaxValue) };
                ModifiedFlag = true;
            }
            else
            {
                Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);

                if (quota == null)
                    _Quotas.Add(new Admin.Quota(key, value, 0, 0, DateTime.MaxValue));
                else if (quota.Limit == value)
                    return;
                else
                    quota.Limit = value;

                ModifiedFlag = true;
            }
        }

        public int GetQuotaCount(string key, int defaultValue = 0)
        {
            Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);
            int count = defaultValue;

            if (quota != null)
                count = quota.Count;

            return count;
        }

        public void SetQuotaCount(string key, int value)
        {
            if (_Quotas == null)
            {
                _Quotas = new List<Quota>(1) { new Quota(key, 0, value, 0, DateTime.MaxValue) };
                ModifiedFlag = true;
            }
            else
            {
                Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);

                if (quota == null)
                    _Quotas.Add(new Admin.Quota(key, value, 0, 0, DateTime.MaxValue));
                else if (quota.Count == value)
                    return;
                else
                    quota.Count = value;

                ModifiedFlag = true;
            }
        }

        public DateTime GetQuotaExpiration(string key, DateTime defaultValue)
        {
            Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);
            DateTime expiration = defaultValue;

            if (quota != null)
                expiration = quota.Expiration;

            return expiration;
        }

        public void SetQuotaExpiration(string key, DateTime value)
        {
            if (_Quotas == null)
            {
                _Quotas = new List<Quota>(1) { new Quota(key, 0, 0, 0, value) };
                ModifiedFlag = true;
            }
            else
            {
                Quota quota = _Quotas.FirstOrDefault(x => x.KeyString == key);

                if (quota == null)
                    _Quotas.Add(new Admin.Quota(key, 0, 0, 0, value));
                else if (quota.Expiration == value)
                    return;
                else
                    quota.Expiration = value;

                ModifiedFlag = true;
            }
        }

        public bool DeleteQuota(string key)
        {
            if (_Quotas == null)
                return false;

            Quota option = GetQuota(key);

            if (option == null)
                return false;

            ModifiedFlag = true;

            return _Quotas.Remove(option);
        }

        public void DeleteAllQuotas()
        {
            if (_Quotas != null)
            {
                _Quotas.Clear();
                ModifiedFlag = true;
            }
        }

        // Returns true if quota not reached.
        public bool QuotaCheck(
            string name,
            int count)
        {
            Quota quota = GetQuota(name);
            bool returnValue = true;

            if (quota == null)
            {
                quota = CreateQuota(name);

                if (quota == null)
                    return false;
            }

            if (quota.PeriodDays != 0)
            {
                if (quota.Expiration <= DateTime.UtcNow)
                {
                    TimeSpan timeSpan = new TimeSpan(quota.PeriodDays, 0, 0, 0);
                    quota.Expiration = DateTime.UtcNow + timeSpan;
                    quota.Count = 0;
                }
            }

            quota.Count = quota.Count + count;

            if (!IsAdministrator())
            {
                if (quota.Limit > 0)
                {
                    if (quota.Count > quota.Limit)
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public Quota CreateQuota(
            string name)
        {
            int limit;
            int days;
            DateTime expiration;

            if (!GetQuotaData(name, out limit, out days))
                return null;

            if (days != 0)
            {
                TimeSpan timeSpan = new TimeSpan(days, 0, 0, 0);
                expiration = DateTime.UtcNow + timeSpan;
            }
            else
                expiration = DateTime.MaxValue;

            Quota quota = new Quota(name, limit, 0, days, expiration);
            SetQuota(quota);

            return quota;
        }

        public bool GetQuotaData(
            string name,
            out int limit,
            out int expirationDays)
        {
            switch (name)
            {
                case "TranslateTransaction":
                    limit = ApplicationData.PerUserTranslateTransactionLimit;
                    expirationDays = ApplicationData.PerUserTranslateTransactionPeriod;
                    break;
                case "TranslateCharacters":
                    limit = ApplicationData.PerUserTranslateCharCountLimit;
                    expirationDays = ApplicationData.PerUserTranslateTransactionPeriod;
                    break;
                case "ContentTranslateTransaction":
                    limit = ApplicationData.PerUserContentTranslateTransactionLimit;
                    expirationDays = ApplicationData.PerUserTranslateTransactionPeriod;
                    break;
                case "ContentTranslateCharacters":
                    limit = ApplicationData.PerUserContentTranslateCharCountLimit;
                    expirationDays = ApplicationData.PerUserTranslateTransactionPeriod;
                    break;
                case "UITranslationTransaction":
                case "UITranslationCharacters":
                case "SynchronizeLanguagesTranslationTransaction":
                case "SynchronizeLanguagesTranslationCharacters":
                    limit = -1;
                    expirationDays = 0;
                    break;
                default:
                    limit = -1;
                    expirationDays = 0;
                    return false;
            }

            return true;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;
                if ((_AboutMe != null) && _AboutMe.Modified)
                    return true;
                if ((_MarkupReference != null) && _MarkupReference.Modified)
                    return true;

                if (_UserProfiles != null)
                {
                    foreach (UserProfile userProfile in _UserProfiles)
                    {
                        if (userProfile.Modified)
                            return true;
                    }
                }

                return ModifiedFlag;
            }
            set
            {
                base.Modified = value;
                if (_AboutMe != null)
                    _AboutMe.Modified = false;
                if (_MarkupReference != null)
                    _MarkupReference.Modified = false;

                if (_UserProfiles != null)
                {
                    foreach (UserProfile userProfile in _UserProfiles)
                        userProfile.Modified = false;
                }

                ModifiedFlag = value;
            }
        }

        public BaseObjectTitled GetBaseObjectTitled()
        {
            BaseObjectTitled baseObjectTitled = new BaseObjectTitled(
                UserName,
                new MultiLanguageString(UserName, CurrentUserProfile.UILanguageID, UserName),
                (_AboutMe != null ? new MultiLanguageString(_AboutMe) : new MultiLanguageString()),
                Source,
                Package,
                Label,
                ProfileImageFileName,
                -1,
                false,
                CurrentUserProfile.TargetLanguageIDs,
                CurrentUserProfile.HostLanguageIDs,
                UserName);
            return baseObjectTitled;
        }

        public ObjectReferenceTitled GetObjectReferenceTitled()
        {
            ObjectReferenceTitled titledReference = new ObjectReferenceTitled(
                UserName,
                new MultiLanguageString(UserName, CurrentUserProfile.UILanguageID, UserName),
                (_AboutMe != null ? new MultiLanguageString(_AboutMe) : new MultiLanguageString()),
                Source,
                Package,
                Label,
                ProfileImageFileName,
                -1,
                false,
                CurrentUserProfile.TargetLanguageIDs,
                CurrentUserProfile.HostLanguageIDs,
                UserName);
            return titledReference;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);
            if (_MarkupReference != null)
                _MarkupReference.ResolveReferences(mainRepository, recurseParents, recurseChildren);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.SaveReferences(mainRepository, recurseParents, recurseChildren);
            if (_MarkupReference != null)
                returnValue = _MarkupReference.SaveReferences(mainRepository, recurseParents, recurseChildren) && returnValue;
            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReferences(mainRepository, recurseParents, recurseChildren);
            if (_MarkupReference != null)
                returnValue = _MarkupReference.UpdateReferences(mainRepository, recurseParents, recurseChildren) && returnValue;
            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren);
            if (_MarkupReference != null)
                returnValue = _MarkupReference.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren) && returnValue;
            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseParents, recurseChildren);
            if (recurseChildren)
            {
                if (_MarkupReference != null)
                    _MarkupReference.ClearReferences(recurseParents, recurseChildren);
            }
        }

        public static UserStanding GetStandingFromString(string str)
        {
            switch (str)
            {
                case "Active":
                    return UserStanding.Active;
                case "Monitor":
                    return UserStanding.Monitor;
                case "Block":
                    return UserStanding.Block;
                default:
                    throw new ObjectException("Unknown user standing: " + str);
            }
        }

        public override string Directory
        {
            get
            {
                return UserName;
            }
            set
            {
            }
        }

        public override string MediaTildeUrl
        {
            get
            {
                string mediaTildeUrl = ApplicationData.MediaTildeUrl;

                if (!mediaTildeUrl.EndsWith("/"))
                    mediaTildeUrl += "/";

                mediaTildeUrl += MediaUtilities.FileFriendlyName(Owner);

                return mediaTildeUrl;
            }
        }

        public override string ComposeDirectory()
        {
            string directory = UserName;
            return directory;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_RealName != null)
                element.Add(new XElement("RealName", _RealName));
            if (_Email != null)
                element.Add(new XElement("Email", _Email));
            if (_EncryptedPassword != null)
                element.Add(new XElement("EncryptedPassword", ObjectUtilities.GetDataStringFromByteArray(_EncryptedPassword, false)));
            if (_UserRole != null)
                element.Add(new XElement("UserRole", UserRole));
            element.Add(new XElement("Standing", _Standing.ToString()));
            if (!String.IsNullOrEmpty(_ProfileImageFileName))
                element.Add(new XElement("ProfileImageFileName", ProfileImageFileName));
            if (_AboutMe != null)
                element.Add(AboutMe.GetElement("AboutMe"));
            //element.Add(new XElement("IsMinor", _IsMinor.ToString()));
            if (_MarkupReference != null)
                element.Add(MarkupReference.GetElement("MarkupReference"));
            if (!String.IsNullOrEmpty(_Team))
                element.Add(new XElement("Team", _Team));
            if ((_Packages != null) && (_Packages.Count() != 0))
                element.Add(ObjectUtilities.GetElementFromStringList("Packages", _Packages));
            if (_WatchedBlogs != null)
                element.Add(ObjectUtilities.GetElementFromStringList("WatchedBlogs", _WatchedBlogs));
            if (_BlockedUsers != null)
                element.Add(ObjectUtilities.GetElementFromStringList("BlockedUsers", _BlockedUsers));
            if (!String.IsNullOrEmpty(_TimeZoneID))
                element.Add(new XElement("TimeZoneID", _TimeZoneID));
            if (_TimeZoneOffset != TimeSpan.Zero)
                element.Add(new XElement("TimeZoneOffset", ObjectUtilities.GetStringFromTimeSpan(_TimeZoneOffset)));
            if (_AccountCreated != DateTime.MinValue)
                element.Add(new XElement("AccountCreated", ObjectUtilities.GetStringFromDateTime(_AccountCreated)));
            if (_AccountAccessed != DateTime.MinValue)
                element.Add(new XElement("AccountAccessed", ObjectUtilities.GetStringFromDateTime(_AccountAccessed)));
            if (_UsageCount != 0)
                element.Add(new XElement("UsageCount", _UsageCount.ToString()));
            if (!String.IsNullOrEmpty(_IPAddress))
                element.Add(new XElement("IPAddress", _IPAddress));
            element.Add(new XElement("IsHidden", IsHidden.ToString()));
            if (_UserProfiles != null)
            {
                foreach (UserProfile userProfile in _UserProfiles)
                    element.Add(userProfile.Xml);
            }
            else if (_CurrentUserProfile != null)
            {
                element.Add(_CurrentUserProfile.Xml);
            }
            if (_CurrentUserProfile != null)
                element.Add(new XElement("CurrentUserProfileName", _CurrentUserProfile.Name));
            element.Add(new XElement("ProfileOrdinal", _ProfileOrdinal.ToString()));
            if ((_UserOptions != null) && (_UserOptions.Count() != 0))
                element.Add(BaseString.GetElementFromBaseStringList("UserOptions", _UserOptions));
            if (_Quotas != null)
            {
                foreach (Quota quota in _Quotas)
                {
                    XElement quotaElement = quota.GetElement("Quota");
                    element.Add(quotaElement);
                }
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "RealName":
                    _RealName = childElement.Value.Trim();
                    break;
                case "Email":
                    _Email = childElement.Value.Trim();
                    break;
                case "EncryptedPassword":
                    _EncryptedPassword = ObjectUtilities.GetByteArrayFromDataString(childElement.Value);
                    break;
                case "UserRole":
                    _UserRole = childElement.Value.Trim();
                    break;
                case "Standing":
                    _Standing = GetStandingFromString(childElement.Value.Trim());
                    break;
                case "ProfileImageKey": // legacy
                case "ProfileImageReference":
                    break;
                case "ProfileImageFileName":
                    _ProfileImageFileName = childElement.Value.Trim();
                    break;
                case "AboutMe":
                    _AboutMe = new MultiLanguageString(childElement);
                    break;
                case "IsMinor":
                    _IsMinor = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "MarkupReference":
                    _MarkupReference = new ObjectReference<BaseObjectKeyed>(childElement);
                    break;
                case "Team":
                    _Team = childElement.Value.Trim();
                    break;
                case "Packages":
                    _Packages = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "WatchedBlogs":
                    _WatchedBlogs = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "BlockedUsers":
                    _BlockedUsers = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "TimeZoneID":
                    _TimeZoneID = childElement.Value.Trim();
                    break;
                case "TimeZoneOffset":
                    _TimeZoneOffset = ObjectUtilities.GetTimeSpanFromString(childElement.Value.Trim(), TimeSpan.MinValue);
                    break;
                case "AccountCreated":
                    _AccountCreated = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "AccountAccessed":
                    _AccountAccessed = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "UsageCount":
                    _UsageCount = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "IPAddress":
                    _IPAddress = childElement.Value.Trim();
                    break;
                case "IsHidden":
                    _IsHidden = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "UserProfile":
                    {
                        UserProfile userProfile = new UserProfile(childElement);
                        AddUserProfile(userProfile);
                    }
                    break;
                case "CurrentUserProfileName":
                    {
                        string currentProfileName = childElement.Value.Trim();
                        CurrentUserProfile = FindUserProfile(currentProfileName);
                    }
                    break;
                case "ProfileOrdinal":
                    _ProfileOrdinal = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "UserOptions":
                    _UserOptions = BaseString.GetBaseStringListFromElement(childElement);
                    break;
                case "Quota":
                    {
                        Quota quota = new Quota(childElement);
                        if (_Quotas == null)
                            _Quotas = new List<Quota>() { quota };
                        else
                            _Quotas.Add(quota);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            UserRecord otherObject = other as UserRecord;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_RealName, otherObject.RealName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Email, otherObject.Email);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareByteArrays(_EncryptedPassword, otherObject.EncryptedPassword);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_UserRole, otherObject.UserRole);
            if (diff != 0)
                return diff;
            diff = (int)_Standing - (int)(otherObject.Standing);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_ProfileImageFileName, otherObject.ProfileImageFileName);
            if (diff != 0)
                return diff;
            diff = MultiLanguageString.Compare(_AboutMe, otherObject.AboutMe);
            if (_IsMinor != otherObject.IsMinor)
                return (_IsMinor ? 1 : -1);
            if (diff != 0)
                return diff;
            diff = MarkupReference.Compare(otherObject.MarkupReference);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Team, otherObject.Team);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_Packages, otherObject.Packages);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_WatchedBlogs, otherObject.WatchedBlogs);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_BlockedUsers, otherObject.BlockedUsers);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_TimeZoneID, otherObject.TimeZoneID);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpans(_TimeZoneOffset, otherObject.TimeZoneOffset);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_AccountCreated, otherObject.AccountCreated);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_AccountAccessed, otherObject.AccountAccessed);
            if (diff != 0)
                return diff;
            diff = _UsageCount - otherObject.UsageCount;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_IPAddress, otherObject.IPAddress);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTypedObjectLists<UserProfile>(_UserProfiles, otherObject.UserProfiles);
            if (diff != 0)
                return diff;
            ObjectUtilities.Compare(_CurrentUserProfile, otherObject.CurrentUserProfile);
            return 0;
        }

        public static int Compare(UserRecord object1, UserRecord object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareUserKeys(UserRecord object1, UserRecord object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareStrings(object1.UserName, object2.UserName);
        }

        public static List<Tuple<string, int>> CopyQuotas(List<Tuple<string, int>> source)
        {
            if (source == null)
                return null;

            List<Tuple<string, int>> quotas = new List<Tuple<string, int>>();

            foreach (Tuple<string, int> quota in source)
                quotas.Add(new Tuple<string, int>(quota.Item1, quota.Item2));

            return quotas;
        }

        public static void Merge(UserRecord object1, UserRecord object2)
        {
            if (ObjectUtilities.CompareStrings(object1.RealName, object2.RealName) != 0)
            {
                if (String.IsNullOrEmpty(object1.RealName))
                    object1.RealName = object2.RealName;
                else if (String.IsNullOrEmpty(object2.RealName))
                    object2.RealName = object1.RealName;
            }

            if (MultiLanguageString.Compare(object1.AboutMe, object2.AboutMe) != 0)
                MultiLanguageString.MergeOrCreate(ref object1._AboutMe, ref object2._AboutMe);

            if (object1.IsMinor != object2.IsMinor)
                object1.IsMinor = object2.IsMinor = true;

            if (ObjectUtilities.CompareStrings(object1.Team, object2.Team) != 0)
            {
                if (String.IsNullOrEmpty(object1.Team))
                    object1.Team = object2.Team;
                else if (String.IsNullOrEmpty(object2.Team))
                    object2.Team = object1.Team;
            }

            if (ObjectUtilities.CompareStringLists(object1.Packages, object2.Packages) != 0)
            {
                List<string> packages = ObjectUtilities.ListConcatenateUnique(object1.Packages, object2.Packages);
                if (ObjectUtilities.CompareStringLists(object1.Packages, packages) != 0)
                    object1.Packages = packages;
                if (ObjectUtilities.CompareStringLists(object2.Packages, packages) != 0)
                    object2.Packages = packages;
            }

            if (ObjectUtilities.CompareTypedObjectLists<UserProfile>(object1.UserProfiles, object2.UserProfiles) != 0)
            {
                foreach (UserProfile userProfile in object1.UserProfiles)
                {
                    if (object2.FindUserProfile(userProfile.ProfileName) == null)
                    {
                        UserProfile newUserProfile = new UserProfile(userProfile);
                        object2.AddUserProfile(newUserProfile);
                    }
                }
                foreach (UserProfile userProfile in object2.UserProfiles)
                {
                    if (object1.FindUserProfile(userProfile.ProfileName) == null)
                    {
                        UserProfile newUserProfile = new UserProfile(userProfile);
                        object1.AddUserProfile(newUserProfile);
                    }
                }
            }
        }
    }
}

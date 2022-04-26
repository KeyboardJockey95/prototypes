using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Tool
{
    public enum ToolTypeCode
    {
        Unknown,
        Flash,
        Flip,
        HandsFree,
        Match,
        Test,
        Text,
        List
    }

    public enum ToolSourceCode
    {
        Unknown,
        StudyList,
        VocabularyList,
        StudyListInflections,
        VocabularyListInflections
    }

    public enum ToolViewCode
    {
        Unknown,
        Study,
        Select,
        List
    }

    public enum ToolResponseMode
    {
        Answer,
        Confirm,
        Rescore,
        Next,
        Nothing,
        Unknown
    }

    public enum ToolResponseCommand
    {
        NewSession,
        Score,
        Retry,
        Skip,
        MarkLearned,
        Reset,
        Forget,
        ForgetAll,
        ForgetLearned,
        LearnedAll,
        LearnedNew,
        Edit,
        Recreate,
        Check,
        Forward,
        Back,
        SetPause,
        SetText,
        DefinitionAction,
        DeletePhrases,
        EditStudyList,
        EditSentenceRuns,
        EditWordRuns,
        SetStudyItems,
        SetStage,
        SetNextReviewTime,
        Unknown
    }

    public class ToolSession : BaseObjectKeyed
    {
        protected ToolTypeCode _ToolType;
        protected ToolSourceCode _ToolSource;
        protected ToolViewCode _ToolView;
        protected List<ToolProfile> _ToolProfiles;
        protected string _ToolProfileKey;
        protected string _ToolConfigurationKey;
        protected Dictionary<string, string> _ToolTypeConfigurationKeys;
        protected string _ToolStudyListKey;
        protected ToolStudyList _ToolStudyList;             // Not saved.
        protected ToolIndexHistory _ToolIndexHistory;
        protected ToolItemSelector _ToolItemSelector;
        protected DateTime _SessionStart;
        protected int _CardIndex;
        protected ToolResponseMode _ResponseMode;
        protected int _HistoryIndex;
        protected int _NewCount;
        protected int _ReviewCount;
        protected DateTime _LastLimitCheck;
        public ContentStudyList StudyListHint;
        public static int MaxSessions = 4;
        public bool _StudySessionComplete;

        public ToolSession(string toolSessionKey)
            : base(toolSessionKey)
        {
            ClearToolSession();
            ToolItemSelector = new ToolItemSelector(this);
        }

        public ToolSession(string toolSessionKey, string toolConfigurationKey, string toolStudyListKey)
            : base(toolSessionKey)
        {
            ClearToolSession();
            _ToolConfigurationKey = toolConfigurationKey;
            _ToolStudyListKey = toolStudyListKey;
            _ToolItemSelector = new ToolItemSelector(this);
        }

        public ToolSession(string toolSessionKey, List<ToolProfile> toolProfiles,
                string toolProfileKey, string toolConfigurationKey,
                ToolStudyList toolStudyList, ToolItemSelector toolItemSelector)
            : base(toolSessionKey)
        {
            ClearToolSession();
            _ToolProfiles = toolProfiles;
            _ToolProfileKey = toolProfileKey;
            _ToolConfigurationKey = toolConfigurationKey;
            _ToolStudyListKey = (toolStudyList != null ? toolStudyList.KeyString : String.Empty);
            _ToolStudyList = toolStudyList;
            SetupHistory();
            if (toolItemSelector == null)
                _ToolItemSelector = new ToolItemSelector(this);
            else
            {
                _ToolItemSelector = toolItemSelector;
                _ToolItemSelector.Session = this;
            }
        }

        public ToolSession(ToolSession other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolSession(object key, ToolSession other)
            : base(key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolSession(XElement element)
        {
            OnElement(element);
        }

        public ToolSession()
        {
            ClearToolSession();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolSession();
        }

        public void ClearToolSession()
        {
            _ToolSource = ToolSourceCode.Unknown;
            _ToolView = ToolViewCode.Unknown;
            _ToolProfiles = new List<ToolProfile>();
            _ToolProfileKey = String.Empty;
            _ToolConfigurationKey = String.Empty;
            _ToolTypeConfigurationKeys = new Dictionary<string, string>();
            _ToolStudyListKey = String.Empty;
            _ToolStudyList = null;
            _ToolItemSelector = new ToolItemSelector(this);
            _SessionStart = DateTime.UtcNow;
            _CardIndex = 0;
            _ResponseMode = ToolResponseMode.Unknown;
            _HistoryIndex = 0;
            _NewCount = 0;
            _ReviewCount = 0;
            _LastLimitCheck = DateTime.MinValue;
            StudyListHint = null;
            _StudySessionComplete = false;
        }

        public void Copy(ToolSession other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            _ToolSource = other.ToolSource;
            _ToolView = other.ToolView;
            _ToolProfiles = new List<ToolProfile>(other.ToolProfiles);
            _ToolProfileKey = other.ToolProfileKey;
            _ToolConfigurationKey = other.ToolConfigurationKey;
            _ToolTypeConfigurationKeys = new Dictionary<string, string>(other.ToolTypeConfigurationKeys);
            _ToolStudyListKey = other.ToolStudyListKey;
            _ToolStudyList = other.ToolStudyList;

            if (other.ToolItemSelector != null)
            {
                _ToolItemSelector = new ToolItemSelector(other.ToolItemSelector);
                _ToolItemSelector.Session = this;
            }
            else
                _ToolItemSelector = null;

            if (other.ToolIndexHistory != null)
                _ToolIndexHistory = new ToolIndexHistory(other.ToolIndexHistory);
            else
                _ToolIndexHistory = null;

            _CardIndex = other.CardIndex;
            _ResponseMode = other.ResponseMode;
            _HistoryIndex = other.HistoryIndex;
            _NewCount = other.NewCount;
            _ReviewCount = other.ReviewCount;
            _LastLimitCheck = other.LastLimitCheck;
            StudySessionComplete = other.StudySessionComplete;
        }

        public void Validate()
        {
            if (ToolProfile == null)
            {
                if ((_ToolProfiles != null) && (_ToolProfiles.Count() != 0))
                    ToolProfileKey = _ToolProfiles.First().KeyString;
            }

            if (ToolConfiguration == null)
            {
                if (ToolProfile != null)
                {
                    ToolConfiguration configuration = ToolProfile.GetToolConfigurationIndexed(0);

                    if (configuration != null)
                        ToolConfigurationKey = configuration.KeyString;
                }
            }
        }

        public ToolTypeCode ToolType
        {
            get
            {
                return _ToolType;
            }
            set
            {
                if (_ToolType != value)
                {
                    _ToolType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolSourceCode ToolSource
        {
            get
            {
                return _ToolSource;
            }
            set
            {
                if (_ToolSource != value)
                {
                    _ToolSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolViewCode ToolView
        {
            get
            {
                return _ToolView;
            }
            set
            {
                if (_ToolView != value)
                {
                    _ToolView = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<ToolProfile> ToolProfiles
        {
            get
            {
                return _ToolProfiles;
            }
            set
            {
                if (_ToolProfiles != value)
                {
                    _ToolProfiles = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ToolProfileKey
        {
            get
            {
                return _ToolProfileKey;
            }
            set
            {
                if (_ToolProfileKey != value)
                {
                    _ToolProfileKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ToolProfileName
        {
            get
            {
                if (String.IsNullOrEmpty(_ToolProfileKey))
                    return String.Empty;
                return ToolUtilities.GetToolProfileName(_ToolProfileKey);
            }
        }

        public ToolProfile ToolProfile
        {
            get
            {
                if (_ToolProfiles != null)
                    return _ToolProfiles.FirstOrDefault(x => x.MatchKey(_ToolProfileKey));
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (!value.MatchKey(_ToolProfileKey))
                    {
                        _ToolProfileKey = value.KeyString;
                        ModifiedFlag = true;
                    }
                }
                else if (!String.IsNullOrEmpty(_ToolProfileKey))
                {
                    _ToolProfileKey = String.Empty;
                    ModifiedFlag = true;
                }
            }
        }

        public string ToolConfigurationKey
        {
            get
            {
                string configurationKey;
                if (!_ToolTypeConfigurationKeys.TryGetValue(_ToolType.ToString(), out configurationKey))
                    configurationKey = _ToolConfigurationKey;
                else if (String.IsNullOrEmpty(configurationKey))
                    configurationKey = _ToolConfigurationKey;
                return configurationKey;
            }
            set
            {
                if (ToolConfigurationKey != value)
                {
                    _ToolConfigurationKey = value;

                    if (!String.IsNullOrEmpty(value))
                        _ToolTypeConfigurationKeys[_ToolType.ToString()] = value;

                    ModifiedFlag = true;
                }
            }
        }

        public Dictionary<string, string> ToolTypeConfigurationKeys
        {
            get
            {
                return _ToolTypeConfigurationKeys;
            }
            set
            {
                if (_ToolTypeConfigurationKeys != value)
                {
                    _ToolTypeConfigurationKeys = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolConfiguration ToolConfiguration
        {
            get
            {
                ToolConfiguration toolConfiguration = null;

                if (ToolProfile != null)
                {
                    if (!String.IsNullOrEmpty(_ToolConfigurationKey))
                        toolConfiguration = ToolProfile.GetToolConfiguration(ToolConfigurationKey);

                    if (toolConfiguration == null)
                        toolConfiguration = ToolProfile.GetToolConfigurationFuzzy(ToolConfigurationKey, null);

                    if (toolConfiguration == null)
                    {
                        toolConfiguration = ToolProfile.GetToolConfigurationIndexed(0);

                        if (toolConfiguration != null)
                        {
                            _ToolConfigurationKey = toolConfiguration.KeyString;
                            ModifiedFlag = true;
                        }
                    }
                }

                return toolConfiguration;
            }
            set
            {
                if (value != null)
                    ToolConfigurationKey = value.KeyString;
            }
        }

        public string ToolStudyListKey
        {
            get
            {
                return _ToolStudyListKey;
            }
            set
            {
                if (_ToolStudyListKey != value)
                {
                    _ToolStudyListKey = value;
                    _ToolStudyList = null;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolStudyList ToolStudyList
        {
            get
            {
                return _ToolStudyList;
            }
            set
            {
                if (_ToolStudyList != value)
                {
                    _ToolStudyList = value;

                    if (_ToolStudyList != null)
                        _ToolStudyListKey = _ToolStudyList.KeyString;
                    else
                        _ToolStudyListKey = String.Empty;

                    ModifiedFlag = true;
                }
            }
        }

        public int ToolStudyItemCount
        {
            get
            {
                ToolStudyList toolStudyList = _ToolStudyList;
                int count = 0;

                if (toolStudyList != null)
                {
                    switch (toolStudyList.ToolSource)
                    {
                        case ToolSourceCode.StudyList:
                        case ToolSourceCode.VocabularyList:
                        case ToolSourceCode.Unknown:
                        default:
                            count = toolStudyList.ToolStudyItemCount();
                            break;
                        case ToolSourceCode.StudyListInflections:
                        case ToolSourceCode.VocabularyListInflections:
                            count = toolStudyList.ToolInflectionStudyItemCount();
                            break;
                    }
                }

                return count;
            }
        }

        public int FilteredToolStudyItemCount
        {
            get
            {
                int count;

                if (_ToolItemSelector != null)
                    count = _ToolItemSelector.ItemCount;
                else
                    count = ToolStudyItemCount;

                return count;
            }
        }

        public List<ToolStudyItem> ToolStudyItems
        {
            get
            {
                ToolStudyList toolStudyList = _ToolStudyList;
                List<ToolStudyItem> toolStudyItems = null;

                if (toolStudyList != null)
                {
                    switch (toolStudyList.ToolSource)
                    {
                        case ToolSourceCode.StudyList:
                        case ToolSourceCode.VocabularyList:
                        case ToolSourceCode.Unknown:
                        default:
                            toolStudyItems = toolStudyList.ToolStudyItems;
                            break;
                        case ToolSourceCode.StudyListInflections:
                        case ToolSourceCode.VocabularyListInflections:
                            toolStudyItems = toolStudyList.InflectionToolStudyItems;
                            break;
                    }
                }

                return toolStudyItems;
            }
        }

        public List<ToolStudyItem> FilteredToolStudyItems
        {
            get
            {
                List<ToolStudyItem> allToolStudyItems = ToolStudyItems;
                List<ToolStudyItem> toolStudyItems = new List<ToolStudyItem>();

                foreach (ToolStudyItem toolStudyItem in allToolStudyItems)
                {
                    if (!toolStudyItem.IsStudyItemHidden)
                        toolStudyItems.Add(toolStudyItem);
                }

                return toolStudyItems;
            }
        }

        public ToolItemSelector ToolItemSelector
        {
            get
            {
                return _ToolItemSelector;
            }
            set
            {
                if (_ToolItemSelector != value)
                {
                    _ToolItemSelector = value;

                    if (_ToolItemSelector != null)
                        _ToolItemSelector.Session = this;

                    ModifiedFlag = true;
                }
            }
        }

        public ToolSelectorMode SelectorMode
        {
            get
            {
                if (_ToolItemSelector != null)
                    return _ToolItemSelector.Mode;
                return ToolSelectorMode.Unknown;
            }
            set
            {
                if (_ToolItemSelector != null)
                    _ToolItemSelector.Mode = value;
            }
        }

        public int ToolStudyItemIndex
        {
            get
            {
                if (_HistoryIndex == 0)
                    return _ToolItemSelector.CurrentIndex;
                else
                    return _ToolIndexHistory.GetHistoryItemIndex(_HistoryIndex - 1);
            }
            set
            {
                if (_HistoryIndex == 0)
                    _ToolItemSelector.CurrentIndex = value;
                else
                    _ToolIndexHistory.SetHistoryItemIndex(_HistoryIndex - 1, value);
            }
        }

        public string ToolStudyItemKey
        {
            get
            {
                if (_ToolStudyList == null)
                    return null;
                int index = ToolStudyItemIndex;
                ToolStudyItem toolStudyItem = _ToolStudyList.GetToolStudyItemIndexed(index);
                if (toolStudyItem != null)
                    return toolStudyItem.KeyString;
                return String.Empty;
            }
        }

        public ToolStudyItem ToolStudyItem
        {
            get
            {
                if (_ToolStudyList == null)
                    return null;
                int index = ToolStudyItemIndex;
                ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);
                return toolStudyItem;
            }
        }

        public ToolStudyItem GetToolStudyItemIndexed(int index)
        {
            ToolStudyItem toolStudyItem = null;

            if (ToolStudyList != null)
            {
                switch (ToolStudyList.ToolSource)
                {
                    case ToolSourceCode.StudyList:
                    case ToolSourceCode.VocabularyList:
                    case ToolSourceCode.Unknown:
                    default:
                        toolStudyItem = ToolStudyList.GetToolStudyItemIndexed(index);
                        break;
                    case ToolSourceCode.StudyListInflections:
                    case ToolSourceCode.VocabularyListInflections:
                        toolStudyItem = ToolStudyList.GetInflectionToolStudyItemIndexed(index);
                        break;
                }
            }

            return toolStudyItem;
        }

        public ToolItemStatus ToolItemStatus
        {
            get
            {
                string toolConfigurationKey = ToolConfigurationKey;

                if (!String.IsNullOrEmpty(toolConfigurationKey))
                {
                    ToolStudyItem toolStudyItem = ToolStudyItem;

                    if (toolStudyItem != null)
                    {
                        ToolItemStatus toolItemStatus = toolStudyItem.GetStatus(toolConfigurationKey);
                        return toolItemStatus;
                    }
                }
                return new ToolItemStatus();
            }
        }

        public ToolItemStatus SubToolItemStatus
        {
            get
            {
                string toolConfigurationKey = ToolConfigurationKey;

                if (!String.IsNullOrEmpty(toolConfigurationKey))
                {
                    ToolStudyItem toolStudyItem = ToolStudyItem;

                    if (toolStudyItem != null)
                    {
                        ToolItemStatus toolItemStatus = toolStudyItem.GetStatus(toolConfigurationKey);

                        if (ToolConfiguration.Label == "Hybrid")
                        {
                            toolConfigurationKey = toolItemStatus.SubConfigurationKey;
                            toolItemStatus = toolStudyItem.GetStatus(toolConfigurationKey);
                        }

                        return toolItemStatus;
                    }
                }
                return new ToolItemStatus();
            }
        }

        public DateTime SessionStart
        {
            get
            {
                return _SessionStart;
            }
            set
            {
                if (_SessionStart != value)
                {
                    _SessionStart = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime SessionStartLocal
        {
            get
            {
                return _SessionStart.ToLocalTime();
            }
        }

        public TimeSpan SessionDuration
        {
            get
            {
                TimeSpan duration = DateTime.UtcNow - _SessionStart;
                return duration;
            }
        }

        public int CardIndex
        {
            get
            {
                return _CardIndex;
            }
            set
            {
                if (_CardIndex != value)
                {
                    _CardIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolResponseMode ResponseMode
        {
            get
            {
                return _ResponseMode;
            }
            set
            {
                if (_ResponseMode != value)
                {
                    _ResponseMode = value;
                    ModifiedFlag = true;
                }
            }
        }

        public static ToolResponseMode GetResponseModeFromString(string str)
        {
            switch (str)
            {
                case "Answer":
                    return ToolResponseMode.Answer;
                case "Confirm":
                    return ToolResponseMode.Confirm;
                case "Rescore":
                    return ToolResponseMode.Rescore;
                case "Nothing":
                    return ToolResponseMode.Nothing;
                case "Unknown":
                default:
                    return ToolResponseMode.Unknown;
            }
        }

        public int HistoryIndex
        {
            get
            {
                return _HistoryIndex;
            }
            set
            {
                if (_HistoryIndex != value)
                {
                    _HistoryIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int HistoryCount
        {
            get
            {
                return ToolIndexHistory.HistoryCount;
            }
        }

        public ToolIndexHistory ToolIndexHistory
        {
            get
            {
                if (_ToolIndexHistory == null)
                    SetupHistory();
                return _ToolIndexHistory;
            }
            set
            {
                if (_ToolIndexHistory != value)
                {
                    _ToolIndexHistory = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void SetupHistory()
        {
            ToolConfiguration toolConfiguration = ToolConfiguration;

            if (toolConfiguration == null)
                return;

            _HistoryIndex = 0;
            int historySize = toolConfiguration.HistorySize;

            _ToolIndexHistory = new ToolIndexHistory(historySize);
        }

        public int NewLimit
        {
            get
            {
                ToolProfile toolProfile = ToolProfile;

                if (toolProfile == null)
                    return 0;

                int newLimit = toolProfile.NewLimit;

                return newLimit;
            }
        }

        public bool HaveNewLimit()
        {
            if (NewLimit != 0)
                return true;
            else
                return false;
        }

        public int NewCount
        {
            get
            {
                return _NewCount;
            }
            set
            {
                if (_NewCount != value)
                {
                    _NewCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void IncrementNewCount()
        {
            _NewCount++;
        }

        public bool IsNewLimitReached()
        {
            ToolProfile toolProfile = ToolProfile;

            if (toolProfile == null)
                return false;

            int newLimit = toolProfile.NewLimit;

            if (newLimit <= 0)
                return false;

            if (_NewCount >= newLimit)
                return true;

            return false;
        }

        public int ReviewLimit
        {
            get
            {
                ToolProfile toolProfile = ToolProfile;

                if (toolProfile == null)
                    return 0;

                int reviewLimit = toolProfile.ReviewLimit;

                return reviewLimit;
            }
        }

        public bool HaveReviewLimit()
        {
            if (ReviewLimit != 0)
                return true;
            else
                return false;
        }

        public int ReviewCount
        {
            get
            {
                return _ReviewCount;
            }
            set
            {
                if (_ReviewCount != value)
                {
                    _ReviewCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void IncrementReviewCount()
        {
            _ReviewCount++;
        }

        public bool IsReviewLimitReached()
        {
            ToolProfile toolProfile = ToolProfile;

            if (toolProfile == null)
                return false;

            int reviewLimit = toolProfile.ReviewLimit;

            if (reviewLimit <= 0)
                return false;

            if (_ReviewCount >= reviewLimit)
                return true;

            return false;
        }

        public bool UpdateCounts()
        {
            if (_ToolItemSelector == null)
                return false;

            int currentIndex = _ToolItemSelector.CurrentIndex;

            if (currentIndex == -1)
                return false;

            ToolItemStatus toolItemStatus = ToolItemStatus;

            if (toolItemStatus == null)
                return false;

            switch (toolItemStatus.StatusCode)
            {
                case ToolItemStatusCode.Future:
                    IncrementNewCount();
                    break;
                case ToolItemStatusCode.Active:
                case ToolItemStatusCode.Learned:
                    if (toolItemStatus.TouchCount == 0)
                        IncrementNewCount();
                    else if (toolItemStatus.LastTouchTime < SessionStart)
                        IncrementReviewCount();
                    break;
                default:
                    break;
            }

            return true;
        }

        public bool CheckLimits()
        {
            bool returnValue = true;

            switch (SelectorMode)
            {
                case ToolSelectorMode.Normal:
                case ToolSelectorMode.Unknown:
                default:
                    if (HaveNewLimit() && HaveReviewLimit())
                    {
                        if (IsNewLimitReached() && IsReviewLimitReached())
                            returnValue = false;
                    }
                    else if (HaveNewLimit())
                    {
                        if (IsNewLimitReached())
                            returnValue = false;
                    }
                    else if (HaveReviewLimit())
                    {
                        if (IsReviewLimitReached())
                            returnValue = false;
                    }
                    break;
                case ToolSelectorMode.NewOnly:
                    if (HaveNewLimit())
                    {
                        if (IsNewLimitReached())
                            returnValue = false;
                    }
                    break;
                case ToolSelectorMode.Review:
                    if (HaveReviewLimit())
                    {
                        if (IsReviewLimitReached())
                            returnValue = false;
                    }
                    break;
                case ToolSelectorMode.Test:
                case ToolSelectorMode.ReviewTest:
                    break;
            }

            return returnValue;
        }

        public string LimitMessage()
        {
            if (IsNewLimitReached())
                return "New item limit reached.";
            else if (IsReviewLimitReached())
                return "Review limit reached.";

            return "No limits reached.";
        }

        public bool CheckLimitExpiration(UserRecord userRecord)
        {
            ToolProfile toolProfile = ToolProfile;
            bool returnValue = false;

            if ((toolProfile != null) &&
                ((toolProfile.NewLimit != 0) || (toolProfile.ReviewLimit != 0)))
            {
                DateTime currentDateTime = userRecord.GetLocalTime(DateTime.UtcNow);
                DateTime expirationDateTime = currentDateTime.Date + toolProfile.LimitExpiration;

                if ((_LastLimitCheck < expirationDateTime) && (currentDateTime > expirationDateTime))
                {
                    _NewCount = 0;
                    _ReviewCount = 0;
                    returnValue = true;
                }

                _LastLimitCheck = currentDateTime;
            }

            return returnValue;
        }

        public DateTime LastLimitCheck
        {
            get
            {
                return _LastLimitCheck;
            }
            set
            {
                if (_LastLimitCheck != value)
                {
                    _LastLimitCheck = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool StudySessionComplete
        {
            get
            {
                return _StudySessionComplete;
            }
            set
            {
                _StudySessionComplete = value;
            }
        }

        public void NewSession()
        {
            NewCount = 0;
            ReviewCount = 0;
            SetupHistory();
            _ToolItemSelector.Reset();
            CardIndex = 0;
            ResponseMode = ToolResponseMode.Answer;
            SessionStart = DateTime.UtcNow;
            StudySessionComplete = false;
        }

        public List<string> GetGradeNextTimes(
            ToolItemStatus toolItemStatus,
            LanguageUtilities languageUtilities)
        {
            List<string> nextTimes = new List<string>();
            ToolProfile toolProfile = ToolProfile;

            if (toolProfile == null)
                return nextTimes;

            ToolConfiguration toolConfiguration = ToolConfiguration;

            if (toolConfiguration == null)
                return nextTimes;

            float lastGrade = toolItemStatus.LastGrade;
            int stage = toolItemStatus.Stage;
            DateTime now = DateTime.UtcNow;

            for (int grade = 1; grade <= toolProfile.GradeCount; grade++)
            {
                ToolItemStatus tmpStatus = new ToolItemStatus(toolItemStatus);

                if (_HistoryIndex == 0)
                    toolProfile.TouchApplyGrade(tmpStatus, grade, now, toolConfiguration);
                else
                    toolProfile.RetouchApplyGrade(tmpStatus, grade, now, toolConfiguration);

                switch (toolProfile.SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Adaptive:
                    case SelectorAlgorithmCode.SpacedRepetition:
                    case SelectorAlgorithmCode.Spanki:
                    case SelectorAlgorithmCode.Manual:
                        {
                            TimeSpan timeOffset = tmpStatus.NextTouchTime - now;
                            string timeString = ObjectUtilities.GetStringFromTimeSpanAbbrev(timeOffset, languageUtilities);
                            int stageDiff = tmpStatus.Stage - stage;
                            string stageAction = (stageDiff >= 0 ? " +" + stageDiff.ToString() : " " + stageDiff.ToString());
                            timeString = String.Format("{0:f1}", tmpStatus.Grade) + stageAction + " " + timeString;
                            nextTimes.Add(timeString);
                        }
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        {
                            int stageDiff = tmpStatus.Stage - stage;
                            string stageAction = (stageDiff >= 0 ? " +" + stageDiff.ToString() : " " + stageDiff.ToString());
                            string statusString = String.Format("{0:f1}", tmpStatus.Grade) + stageAction;
                            nextTimes.Add(statusString);
                        }
                        break;
                    default:
                        break;
                }
            }

            return nextTimes;
        }

        public void Retry()
        {
            CardIndex = 0;
            ResponseMode = ToolResponseMode.Answer;
        }

        // Returns false if end of study or no items to study.
        public bool SetNext()
        {
            //if ((ToolItemStatus != null) && (ToolItemStatus.StatusCode != ToolItemStatusCode.Learned))
            if (ToolItemStatus != null)
            {
                if ((ToolStudyItemIndex != -1) && (ToolIndexHistory.GetHistoryItemIndex(0) != ToolStudyItemIndex))
                    ToolIndexHistory.AddHistoryItemIndex(ToolStudyItemIndex);
            }

            CardIndex = 0;
            ResponseMode = ToolResponseMode.Answer;

            bool returnValue = ToolItemSelector.SetNextIndex();

            //if (!returnValue || (ToolItemStatus == null) || (ToolItemStatus.StatusCode == ToolItemStatusCode.Learned))
            if (!returnValue || (ToolItemStatus == null))
                StudySessionComplete = true;
            else if (!CheckLimits())
                StudySessionComplete = true;
            else
                StudySessionComplete = false;

            return returnValue;
        }

        public int Check(MultiLanguageString multiLanguageString)
        {
            int responseIndex = ToolProfile.GradeCount;

            if ((multiLanguageString == null) || (SubToolItemStatus == null))
                return 0;

            ToolItemStatus toolItemStatus = SubToolItemStatus;
            MultiLanguageString lastTextInput = new MultiLanguageString();
            ObjectUtilities.PrepareMultiLanguageString(lastTextInput, "", multiLanguageString.LanguageIDs);

            foreach (LanguageString languageString in multiLanguageString.LanguageStrings)
            {
                LanguageID languageID = languageString.LanguageID;
                string saveTextInput = TextUtilities.GetNormalizedString(languageString.Text);
                string textInput = TextUtilities.GetNormalizedString(languageString.Text.ToLower());
                LanguageItem languageItem = ToolStudyItem.StudyItem.LanguageItem(languageID);
                if (languageItem == null)
                    languageItem = ToolStudyItem.StudyItem.LanguageItemFuzzy(languageID);
                string referenceText = String.Empty;
                if (languageItem != null)
                    referenceText = TextUtilities.GetNormalizedString(languageItem.Text.ToLower());
                int thisResponse = 0;

                lastTextInput.LanguageString(languageID).Text = saveTextInput;

                if (String.IsNullOrEmpty(textInput))
                    thisResponse = 1;
                else
                {
                    if (textInput == referenceText)
                        thisResponse = ToolProfile.GradeCount;
                    else
                    {
                        ConvertCanonical canonicalConverter = new ConvertCanonical(languageID, false);
                        string canonicalTextInput;
                        string canonicalReferenceText;

                        canonicalConverter.To(out canonicalTextInput, textInput);
                        canonicalConverter.To(out canonicalReferenceText, referenceText);

                        if (canonicalTextInput == canonicalReferenceText)
                            thisResponse = ToolProfile.GradeCount;
                        else
                        {
                            ConvertCanonical.NoPunctuation(out canonicalTextInput, canonicalTextInput);
                            ConvertCanonical.NoPunctuation(out canonicalReferenceText, canonicalReferenceText);

                            if (canonicalTextInput == canonicalReferenceText)
                                thisResponse = ToolProfile.GradeCount;
                            else
                                thisResponse = 1;
                        }
                    }
                }

                if (thisResponse < responseIndex)
                    responseIndex = thisResponse;
            }

            if (CardIndex < ToolConfiguration.CardSideCount() - 1)
                CardIndex = CardIndex + 1;

            ResponseMode = ToolResponseMode.Confirm;
            toolItemStatus.LastTextInput = lastTextInput;

            return responseIndex;
        }

        public bool Learned()
        {
            bool returnValue = false;

            UpdateCounts();

            if ((SubToolItemStatus != null) && (SubToolItemStatus.StatusCode != ToolItemStatusCode.Learned))
            {
                SubToolItemStatus.Learned();
                returnValue = SetNext();
            }

            return returnValue;
        }

        public bool Forget(bool allConfigurations)
        {
            bool returnValue = false;

            if (ToolStudyItem != null)
            {
                if (allConfigurations)
                    ToolStudyItem.Forget();
                else
                {
                    ToolStudyItem.Forget(ToolConfigurationKey);

                    if ((ToolConfiguration != null)
                        && ToolConfiguration.IsHybrid()
                        && (ToolConfiguration.SubConfigurationCount() != 0))
                    {
                        foreach (string subConfigurationKey in ToolConfiguration.SubConfigurationKeys)
                            ToolStudyItem.Forget(subConfigurationKey);
                    }
                }

                returnValue = true;
            }

            StudySessionComplete = false;

            return returnValue;
        }

        public bool ForgetAll(bool allConfigurations)
        {
            bool returnValue = false;

            if (ToolStudyList != null)
            {
                if (allConfigurations)
                    ToolStudyList.ForgetAll();
                else
                {
                    ToolStudyList.ForgetAll(ToolConfigurationKey);

                    if ((ToolConfiguration != null)
                        && ToolConfiguration.IsHybrid()
                        && (ToolConfiguration.SubConfigurationCount() != 0))
                    {
                        foreach (string subConfigurationKey in ToolConfiguration.SubConfigurationKeys)
                            ToolStudyList.ForgetAll(subConfigurationKey);
                    }
                }

                returnValue = true;
            }

            NewSession();

            return returnValue;
        }

        public bool ForgetLearned(bool allConfigurations)
        {
            bool returnValue = false;

            if (ToolStudyList != null)
            {
                if (allConfigurations)
                    ToolStudyList.ForgetLearned();
                else
                {
                    ToolStudyList.ForgetLearned(ToolConfigurationKey);

                    if ((ToolConfiguration != null)
                        && ToolConfiguration.IsHybrid()
                        && (ToolConfiguration.SubConfigurationCount() != 0))
                    {
                        foreach (string subConfigurationKey in ToolConfiguration.SubConfigurationKeys)
                            ToolStudyList.ForgetLearned(subConfigurationKey);
                    }
                }

                returnValue = true;
            }

            StudySessionComplete = false;

            return returnValue;
        }

        public bool LearnedAll(bool allConfigurations)
        {
            bool returnValue = false;

            if (ToolStudyList != null)
            {
                if (allConfigurations)
                    ToolStudyList.LearnedAll();
                else
                {
                    ToolStudyList.LearnedAll(ToolConfigurationKey);

                    if ((ToolConfiguration != null)
                        && ToolConfiguration.IsHybrid()
                        && (ToolConfiguration.SubConfigurationCount() != 0))
                    {
                        foreach (string subConfigurationKey in ToolConfiguration.SubConfigurationKeys)
                            ToolStudyList.LearnedAll(subConfigurationKey);
                    }
                }

                returnValue = true;
            }

            StudySessionComplete = false;

            return returnValue;
        }

        public bool Back()
        {
            bool returnValue = true;

            if (_HistoryIndex < ToolIndexHistory.HistoryCount)
            {
                _HistoryIndex++;
                _ResponseMode = ToolResponseMode.Rescore;
                _CardIndex = ToolConfiguration.CardSideCount() - 1;
                ModifiedFlag = true;
            }
            else
                returnValue = false;

            return returnValue;
        }

        public bool Forward()
        {
            bool returnValue = true;

            if (_HistoryIndex > 0)
            {
                _HistoryIndex--;

                if (_HistoryIndex == 0)
                {
                    _ResponseMode = ToolResponseMode.Answer;
                    _CardIndex = 0;
                }
                else
                {
                    _ResponseMode = ToolResponseMode.Rescore;
                    _CardIndex = ToolConfiguration.CardSideCount() - 1;
                }

                ModifiedFlag = true;
            }
            else
                returnValue = false;

            return returnValue;
        }

        public bool AtStartOfHistory()
        {
            bool returnValue = false;

            if (_HistoryIndex == 0)
                returnValue = true;

            return returnValue;
        }

        public virtual int GetLastGradeCount(int grade)
        {
            if (ToolStudyList == null)
                return 0;
            int count = FilteredToolStudyItemCount;
            int index;
            DateTime now = ToolItemSelector.NowTime;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            string configurationKey = ToolConfigurationKey;
            int gradeCount = 0;

            for (index = 0; index < count; index++)
            {
                toolStudyItem = ToolStudyList.GetToolStudyItemIndexed(index);

                if (toolStudyItem == null)
                    break;

                toolItemStatus = toolStudyItem.GetStatus(configurationKey);

                if (toolItemStatus == null)
                    break;

                if (toolItemStatus.TouchCount != 0)
                {
                    if ((grade == -1) || ((int)toolItemStatus.LastGrade == grade))
                        gradeCount++;
                }
            }

            return gradeCount;
        }

        public virtual float GetLastGradeAverage()
        {
            if (ToolStudyList == null)
                return 0.0f;
            int count = FilteredToolStudyItemCount;
            int index;
            DateTime now = ToolItemSelector.NowTime;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            string configurationKey = ToolConfigurationKey;
            int gradeCount = 0;
            float gradeSum = 0.0f;

            for (index = 0; index < count; index++)
            {
                toolStudyItem = ToolStudyList.GetToolStudyItemIndexed(index);

                if (toolStudyItem == null)
                    break;

                toolItemStatus = toolStudyItem.GetStatus(configurationKey);

                if (toolItemStatus == null)
                    break;

                if (toolItemStatus.TouchCount != 0)
                {
                    gradeSum += toolItemStatus.LastGrade;
                    gradeCount++;
                }
            }

            if (gradeCount == 0)
                return 0.0f;

            return gradeSum / gradeCount;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_ToolProfiles != null)
                {
                    foreach (ToolProfile profile in _ToolProfiles)
                    {
                        if (profile.Modified)
                            return true;
                    }
                }

                if (_ToolIndexHistory != null)
                {
                    if (_ToolIndexHistory.Modified)
                        return true;
                }

                if (_ToolItemSelector != null)
                {
                    if (_ToolItemSelector.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_ToolProfiles != null)
                {
                    foreach (ToolProfile profile in _ToolProfiles)
                        profile.Modified = false;
                }

                if (_ToolIndexHistory != null)
                    _ToolIndexHistory.Modified = false;

                if (_ToolItemSelector != null)
                    _ToolItemSelector.Modified = false;
            }
        }

        public void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if ((ToolStudyList == null) && !String.IsNullOrEmpty(ToolStudyListKey))
                ToolStudyList = mainRepository.ResolveReference("ToolStudyLists", null, ToolStudyListKey) as ToolStudyList;
        }

        public bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (ToolStudyList != null)
                returnValue = mainRepository.SaveReference("ToolStudyLists", null, ToolStudyList);

            return returnValue;
        }

        public bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (ToolStudyList != null)
                returnValue = mainRepository.UpdateReference("ToolStudyLists", null, ToolStudyList);

            return returnValue;
        }

        public bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((ToolStudyList != null) && ToolStudyList.Modified)
                returnValue = mainRepository.UpdateReference("ToolStudyLists", null, ToolStudyList);

            return returnValue;
        }

        public void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            ToolStudyList = null;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XAttribute("ToolType", ToolUtilities.GetToolTypeStringFromCode(ToolType)));
            element.Add(new XAttribute("ToolSource", ToolUtilities.GetToolSourceStringFromCode(ToolSource)));
            element.Add(new XAttribute("ToolView", ToolUtilities.GetToolViewStringFromCode(ToolView)));
            element.Add(new XAttribute("ToolProfileCount", ToolProfiles.Count().ToString()));

            if (!String.IsNullOrEmpty(ToolProfileKey))
                element.Add(new XAttribute("ToolProfileKey", ToolProfileKey));

            if (!String.IsNullOrEmpty(ToolConfigurationKey))
                element.Add(new XAttribute("ToolConfigurationKey", ToolConfigurationKey));

            element.Add(ObjectUtilities.GetElementFromDictionary<string, string>(
                "ToolTypeConfigurationKeys", ToolTypeConfigurationKeys));

            if (!String.IsNullOrEmpty(ToolStudyListKey))
                element.Add(new XAttribute("ToolStudyListKey", ToolStudyListKey));

            if (_ToolProfiles != null)
            {
                foreach (ToolProfile profile in _ToolProfiles)
                    element.Add(profile.Xml);
            }

            if (_ToolIndexHistory != null)
                element.Add(_ToolIndexHistory.GetElement("ToolIndexHistory"));

            if (_ToolItemSelector != null)
                element.Add(_ToolItemSelector.GetElement("ToolItemSelector"));

            element.Add(new XAttribute("SessionStart", ObjectUtilities.GetStringFromDateTime(_SessionStart)));
            element.Add(new XAttribute("CardIndex", _CardIndex));
            element.Add(new XAttribute("ResponseMode", _ResponseMode.ToString()));
            element.Add(new XAttribute("HistoryIndex", _HistoryIndex));
            element.Add(new XAttribute("StudySessionComplete", _StudySessionComplete));

            element.Add(new XAttribute("NewCount", _NewCount));
            element.Add(new XAttribute("ReviewCount", _ReviewCount));

            if (_LastLimitCheck != DateTime.MinValue)
                element.Add(new XAttribute("LastLimitCheck", ObjectUtilities.GetStringFromDateTime(_LastLimitCheck)));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "ToolType":
                    _ToolType = ToolUtilities.GetToolTypeCodeFromString(attributeValue);
                    break;
                case "ToolSource":
                    _ToolSource = ToolUtilities.GetToolSourceCodeFromString(attributeValue);
                    break;
                case "ToolView":
                    _ToolView = ToolUtilities.GetToolViewCodeFromString(attributeValue);
                    break;
                case "ToolProfileCount":
                    _ToolProfiles = new List<ToolProfile>(Convert.ToInt32(attributeValue));
                    break;
                case "ToolProfileKey":
                    ToolProfileKey = attributeValue;
                    break;
                case "ToolConfigurationKey":
                    ToolConfigurationKey = attributeValue;
                    break;
                case "ToolStudyListKey":
                    ToolStudyListKey = attributeValue;
                    break;
                case "SessionStart":
                    _SessionStart = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                case "CardIndex":
                    _CardIndex = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "ResponseMode":
                    _ResponseMode = GetResponseModeFromString(attributeValue);
                    break;
                case "HistoryIndex":
                    _HistoryIndex = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "StudySessionComplete":
                    _StudySessionComplete = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "NewCount":
                    _NewCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "ReviewCount":
                    _ReviewCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "LastLimitCheck":
                    _LastLimitCheck = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                default:
                    returnValue = base.OnAttribute(attribute);
                    break;
            }

            return returnValue;
        }

        public override bool OnChildElement(XElement childElement)
        {
            bool returnValue = true;

            switch (childElement.Name.LocalName)
            {
                case "ToolProfile":
                    ToolProfiles.Add(new ToolProfile(childElement));
                    break;
                case "ToolIndexHistory":
                    ToolIndexHistory = new ToolIndexHistory(childElement);
                    break;
                case "ToolItemSelector":
                    ToolItemSelector = new ToolItemSelector(childElement, this);
                    break;
                case "ToolTypeConfigurationKeys":
                    ToolTypeConfigurationKeys = ObjectUtilities.GetDictionaryFromElement<string, string>(
                        childElement, "String", "String");
                    break;
                default:
                    returnValue = base.OnChildElement(childElement);
                    break;
            }

            return returnValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Tool
{
    public enum ToolSelectorMode
    {
        Normal,
        NewOnly,
        Review,
        Test,
        ReviewTest,
        Unknown
    }

    public class ToolItemSelector : BaseObjectKeyed
    {
        public ToolSession Session { get; set; }
        protected int _LastIndex;
        protected int _CurrentIndex;
        protected int _Ordinal;
        protected List<int> SequentialIndices;
        protected List<int> RandomIndices;
        protected int _CachedStudyItemCount;
        protected int _LogicalIndex;
        protected bool _IsCustomTime;
        protected DateTime _CustomNowTime;
        protected bool _IsEndOfStudy;
        protected ToolSelectorMode _Mode;
        protected string _HybridConfigurationKey;
        public bool IsNormal { get { return Mode == ToolSelectorMode.Normal; } }
        public bool IsNewOnly { get { return Mode == ToolSelectorMode.NewOnly; } }
        public bool IsReview { get { return Mode == ToolSelectorMode.Review; } }
        public bool IsTest { get { return Mode == ToolSelectorMode.Test; } }
        public bool IsReviewTest { get { return Mode == ToolSelectorMode.ReviewTest; } }
        private bool WasReset;
        public static List<string> SelectorModeNames = new List<string>()
        {
            "Normal",
            "New Only",
            "Review",
            "Test",
            "Review Test"
        };
        protected static Random random = new Random();

        public ToolItemSelector(ToolSession session)
        {
            ClearToolItemSelector();
            Session = session;
        }

        public ToolItemSelector(ToolItemSelector other)
            : base(other)
        {
        }

        public ToolItemSelector(XElement element, ToolSession session)
        {
            OnElement(element);
            Session = session;
        }

        public ToolItemSelector()
        {
            ClearToolItemSelector();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolItemSelector();
        }

        public void ClearToolItemSelector()
        {
            Session = null;
            _LastIndex = -1;
            _CurrentIndex = -1;
            _Ordinal = 0;
            SequentialIndices = null;
            RandomIndices = null;
            _CachedStudyItemCount = -1;
            _LogicalIndex = -1;
            _IsCustomTime = false;
            _CustomNowTime = DateTime.MinValue;
            _IsEndOfStudy = false;
            _Mode = ToolSelectorMode.Normal;
            _HybridConfigurationKey = null;
            WasReset = false;
        }

        public void Copy(ToolItemSelector other)
        {
            if (other == null)
            {
                Clear();
                return;
            }
            Session = other.Session;
            _LastIndex = other.LastIndex;
            _CurrentIndex = other.CurrentIndex;
            _Ordinal = other.Ordinal;
            SequentialIndices = other.SequentialIndices;
            RandomIndices = other.RandomIndices;
            _CachedStudyItemCount = other._CachedStudyItemCount;
            _LogicalIndex = other.LogicalIndex;
            _IsCustomTime = other.IsCustomTime;
            _CustomNowTime = other.CustomNowTime;
            _IsEndOfStudy = other.IsEndOfStudy;
            _Mode = other.Mode;
            _HybridConfigurationKey = null;
            WasReset = false;
        }

        public int LastIndex
        {
            get
            {
                return _LastIndex;
            }
            set
            {
                if (value != _LastIndex)
                {
                    _LastIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int CurrentIndex
        {
            get
            {
                return _CurrentIndex;
            }
            set
            {
                if (value != _CurrentIndex)
                {
                    _CurrentIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int FilteredIndex
        {
            get
            {
                List<ToolStudyItem> toolStudyItems = Session.ToolStudyItems;
                int count = (toolStudyItems != null ? toolStudyItems.Count() : 0);
                int index;
                int filteredIndex = 0;

                for (index = 0; index < count; index++)
                {
                    ToolStudyItem toolStudyItem = toolStudyItems[index];
                    if (index == _CurrentIndex)
                        break;
                    if (!toolStudyItem.IsStudyItemHidden)
                        filteredIndex++;
                }

                return filteredIndex;
            }
        }

        public int Ordinal
        {
            get
            {
                return _Ordinal;
            }
            set
            {
                if (value != _Ordinal)
                {
                    _Ordinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int LogicalIndex
        {
            get
            {
                return _LogicalIndex;
            }
            set
            {
                if (value != _LogicalIndex)
                {
                    _LogicalIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsCustomTime
        {
            get
            {
                return _IsCustomTime;
            }
            set
            {
                if (value != _IsCustomTime)
                {
                    _IsCustomTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime CustomNowTime
        {
            get
            {
                return _CustomNowTime;
            }
            set
            {
                if (value != _CustomNowTime)
                {
                    _CustomNowTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsEndOfStudy
        {
            get
            {
                return _IsEndOfStudy;
            }
            set
            {
                if (value != _IsEndOfStudy)
                {
                    _IsEndOfStudy = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolSelectorMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                if (value != _Mode)
                {
                    _Mode = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ToolProfile Profile
        {
            get
            {
                return Session.ToolProfile;
            }
        }

        public SelectorAlgorithmCode SelectorAlgorithm
        {
            get
            {
                return Profile.SelectorAlgorithm;
            }
        }

        public bool IsRandomUnique
        {
            get
            {
                return Profile.IsRandomUnique;
            }
        }

        public bool IsRandomNew
        {
            get
            {
                return Profile.IsRandomNew;
            }
        }

        public bool IsAdaptiveMixNew
        {
            get
            {
                return Profile.IsAdaptiveMixNew;
            }
        }

        public int ReviewLevel
        {
            get
            {
                return Profile.ReviewLevel;
            }
        }

        public int ChunkSize
        {
            get
            {
                return Profile.ChunkSize;
            }
        }

        public string ConfigurationKey
        {
            get
            {
                if (!String.IsNullOrEmpty(_HybridConfigurationKey))
                    return _HybridConfigurationKey;

                return Session.ToolConfigurationKey;
            }
            set
            {
                _HybridConfigurationKey = value;
            }
        }

        public ToolConfiguration Configuration
        {
            get
            {
                if (!String.IsNullOrEmpty(_HybridConfigurationKey))
                    return Session.ToolProfile.GetToolConfiguration(_HybridConfigurationKey);

                return Session.ToolConfiguration;
            }
        }

        public ToolStudyList ToolStudyList
        {
            get
            {
                return Session.ToolStudyList;
            }
        }

        public int ToolStudyItemCount
        {
            get
            {
                ToolStudyList toolStudyList = ToolStudyList;
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

        public int ItemCount
        {
            get
            {
                return SequentialIndices.Count();
            }
        }

        public ToolStudyItem ToolStudyItem
        {
            get
            {
                ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(CurrentIndex);
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
                if (!String.IsNullOrEmpty(ConfigurationKey))
                {
                    ToolStudyItem toolStudyItem = ToolStudyItem;
                    if (toolStudyItem != null)
                        return toolStudyItem.GetStatus(ConfigurationKey);
                }
                return null;
            }
        }

        public ToolItemStatus SubToolItemStatus
        {
            get
            {
                ToolItemStatus toolItemStatus = ToolItemStatus;

                if ((toolItemStatus != null) && !String.IsNullOrEmpty(toolItemStatus.SubConfigurationKey))
                {
                    ToolStudyItem toolStudyItem = ToolStudyItem;

                    if (toolStudyItem != null)
                        toolItemStatus = toolStudyItem.GetStatus(toolItemStatus.SubConfigurationKey);
                }

                return toolItemStatus;
            }
        }

        public bool IsNewAndReviewLimitReached
        {
            get
            {
                return false;
            }
        }

        public DateTime SessionStart
        {
            get
            {
                return Session.SessionStart;
            }
        }

        public DateTime NowTime
        {
            get
            {
                if (IsCustomTime)
                    return CustomNowTime;
                else
                    return DateTime.UtcNow;
            }
        }

        public DateTime NextReviewTime
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public virtual void GetStatistics(
            ContentStatisticsCache csCache,
            out int readyForReview,
            out int newReadyForReview,
            out int newThisSession,
            out int totalItems,
            out int futureItems,
            out int activeItems,
            out int learnedItems)
        {
            int count = ToolStudyItemCount;
            int index;
            DateTime sessionStart = Session.SessionStart;
            DateTime now = NowTime;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            BaseObjectContent content;
            ContentStatistics cs;

            readyForReview = 0;
            newReadyForReview = 0;
            newThisSession = 0;
            totalItems = 0;
            activeItems = 0;
            learnedItems = 0;
            futureItems = 0;

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);

                if (toolStudyItem == null)
                    break;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if (toolItemStatus == null)
                    break;

                totalItems++;

                content = toolStudyItem.Content;

                if ((csCache != null) && (content != null))
                    cs = csCache.Get(content);
                else
                    cs = null;

                switch (toolItemStatus.StatusCode)
                {
                    case ToolItemStatusCode.Future:
                        futureItems++;
                        if (cs != null)
                            cs.FutureCountLocal += 1;
                        break;
                    case ToolItemStatusCode.Active:
                        activeItems++;
                        if (toolItemStatus.TouchCount != 0)
                        {
                            if (toolItemStatus.NextTouchTime <= now)
                            {
                                readyForReview++;

                                if (toolItemStatus.FirstTouchTime >= sessionStart)
                                    newReadyForReview++;

                                if (cs != null)
                                    cs.DueCountLocal += 1;
                            }
                            else if (cs != null)
                                cs.ActiveCountLocal += 1;

                            if (toolItemStatus.FirstTouchTime >= sessionStart)
                                newThisSession++;

                            if (cs != null)
                            {
                                if (toolItemStatus.NextTouchTime < cs.NextReviewTimeLocal)
                                    cs.NextReviewTimeLocal = toolItemStatus.NextTouchTime;

                                if ((toolItemStatus.NextTouchTime > now) && (toolItemStatus.NextTouchTime < cs.UpdateReviewTimeLocal))
                                    cs.UpdateReviewTimeLocal = toolItemStatus.NextTouchTime;
                            }
                        }
                        continue;
                    case ToolItemStatusCode.Learned:
                        learnedItems++;
                        if (cs != null)
                            cs.CompleteCountLocal += 1;
                        break;
                }
            }
        }

        public void ResetChoiceIndices()
        {
            ToolItemStatus toolItemStatus = ToolItemStatus;

            if (toolItemStatus != null)
            {
                toolItemStatus.ChoiceIndices = null;
                toolItemStatus.CorrectChoiceIndex = -1;
                toolItemStatus.ChosenChoiceIndex = -1;
            }
        }

        public void SetChoiceIndices()
        {
            ToolItemStatus toolItemStatus = SubToolItemStatus;

            if (toolItemStatus == null)
                return;

            int size = Profile.ChoiceSize;

            if (ItemCount < size)
                size = ItemCount;

            int correctIndex = random.Next(0, size);

            if ((toolItemStatus.ChoiceIndices == null) || (toolItemStatus.ChoiceIndices.Count != size))
                toolItemStatus.ChoiceIndices = new List<int>(size);
            else
                toolItemStatus.ChoiceIndices.Clear();
            for (int index = 0; index < size; index++)
            {
                int choiceIndex = 0;

                if (index == correctIndex)
                    choiceIndex = CurrentIndex;
                else
                {
                    int limit = 100;

                    while (limit-- != 0)
                    {
                        choiceIndex = random.Next(0, ItemCount);

                        if (choiceIndex == CurrentIndex)
                            continue;

                        if (toolItemStatus.ChoiceIndices.Contains(choiceIndex))
                            continue;

                        break;
                    }
                }

                toolItemStatus.ChoiceIndices.Add(choiceIndex);
            }

            toolItemStatus.CorrectChoiceIndex = correctIndex;
            toolItemStatus.ChosenChoiceIndex = -1;
        }

        public void SetWordBlanksIndices()
        {
            if (Configuration == null)
                return;

            ToolItemStatus toolItemStatus = SubToolItemStatus;

            if (toolItemStatus == null)
                return;

            LanguageID languageID = null;
            
            foreach (ToolSide card in Configuration.CardSides)
            {
                if (card.HasBlanksInput)
                {
                    if ((card.WriteLanguageIDs != null) && (card.WriteLanguageIDs.Count != 0))
                        languageID = card.WriteLanguageIDs.First();

                    break;
                }
            }

            if (languageID == null)
                throw new Exception("Blanks language ID not set.");

            ToolStudyItem toolStudyItem = ToolStudyItem;

            if (toolStudyItem == null)
                return;

            MultiLanguageItem multiLanguageItem = toolStudyItem.StudyItem;
            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (!languageItem.HasWordRuns())
                languageItem.LoadWordRunsFromText(ApplicationData.Repositories.Dictionary);

            int wordCount = languageItem.WordRunCount();
            int selectedWordIndex = 0;

            if (wordCount > 1)
                selectedWordIndex = random.Next(0, wordCount);

            if (toolItemStatus.WordBlanksIndices == null)
                toolItemStatus.WordBlanksIndices = new List<int>(1) { selectedWordIndex };
            else
            {
                toolItemStatus.WordBlanksIndices.Clear();
                toolItemStatus.WordBlanksIndices.Add(0);
            }

            BaseObjectContent content = multiLanguageItem.Content;

            if (content == null)
                return;

            ContentStudyList studyList = content.ContentStorageStudyList;

            if ((studyList != null) && studyList.Modified)
            {
                studyList.TouchAndClearModified();
                ApplicationData.Repositories.StudyLists.Update(studyList);
            }
        }

        public void ResetWordBlanksIndices()
        {
            ToolItemStatus toolItemStatus = SubToolItemStatus;

            if (toolItemStatus != null)
                toolItemStatus.WordBlanksIndices = null;
        }

        public void SetHybridConfiguration()
        {
            ToolConfiguration toolConfiguration = Session.ToolConfiguration;

            if ((toolConfiguration == null) || !toolConfiguration.IsHybrid())
                return;

            ToolProfile toolProfile = Session.ToolProfile;

            if (toolProfile == null)
                return;

            ToolItemStatus toolItemStatus = Session.ToolItemStatus;

            if (toolItemStatus == null)
                return;

            int subConfigurationIndex = random.Next(0, toolConfiguration.SubConfigurationCount());
            string subConfigurationKey = toolConfiguration.GetSubConfigurationKeyIndexed(subConfigurationIndex);
            _HybridConfigurationKey = subConfigurationKey;

            toolItemStatus.SubConfigurationKey = subConfigurationKey;
        }

        public void ResetHybrid()
        {
            ToolItemStatus toolItemStatus = Session.ToolItemStatus;

            if (toolItemStatus != null)
                toolItemStatus.SubConfigurationKey = null;

            _HybridConfigurationKey = null;
        }

        // Returns false if end of study or no items to study.
        public bool SetNextIndex()
        {
            bool returnValue;

            //ResetCheck();

            ToolConfiguration toolConfiguration = Session.ToolConfiguration;

            if ((toolConfiguration != null) && toolConfiguration.IsHybrid())
            {
                ToolProfile toolProfile = Session.ToolProfile;
                List<string> subConfigurationKeys = new List<string>(toolConfiguration.SubConfigurationKeys);

                if (toolProfile == null)
                    return false;

                returnValue = false;

                while (subConfigurationKeys.Count > 0)
                {
                    int subConfigurationIndex = random.Next(0, subConfigurationKeys.Count);
                    string subConfigurationKey = subConfigurationKeys[subConfigurationIndex];
                    ToolConfiguration toolSubConfiguration = toolProfile.GetToolConfiguration(subConfigurationKey);

                    _HybridConfigurationKey = subConfigurationKey;

                    returnValue = SetNextIndexLocal();

                    if (returnValue)
                    {
                        ToolItemStatus toolItemStatus = Session.ToolItemStatus;
                        toolItemStatus.SubConfigurationKey = subConfigurationKey;
                        break;
                    }

                    subConfigurationKeys.Remove(subConfigurationKey);
                }
            }
            else
                returnValue = SetNextIndexLocal();

            Ordinal = Ordinal + 1;
            WasReset = false;

            return returnValue;
        }

        // Returns false if end of study or no items to study.
        protected bool SetNextIndexLocal()
        {
            bool returnValue;

            IsEndOfStudy = false;

            switch (Profile.SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                    returnValue = SetNextIndexForward();
                    break;
                case SelectorAlgorithmCode.Reverse:
                    returnValue = SetNextIndexReverse();
                    break;
                case SelectorAlgorithmCode.Random:
                    returnValue = SetNextIndexRandom();
                    break;
                case SelectorAlgorithmCode.Adaptive:
                    returnValue = SetNextIndexAdaptive();
                    break;
                case SelectorAlgorithmCode.Chunky:
                    returnValue = SetNextIndexChunky();
                    break;
                case SelectorAlgorithmCode.Leitner:
                    returnValue = SetNextIndexLeitner();
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    returnValue = SetNextIndexSpacedRepetition();
                    break;
                case SelectorAlgorithmCode.Spanki:
                    returnValue = SetNextIndexSpanki();
                    break;
                case SelectorAlgorithmCode.Manual:
                    returnValue = SetNextIndexManual();
                    break;
                default:
                    returnValue = false;
                    break;
            }

            if (returnValue)
            {
                if (Configuration != null)
                {
                    switch (Configuration.Label)
                    {
                        case "Choice":
                            SetChoiceIndices();
                            break;
                        case "Blanks":
                            SetWordBlanksIndices();
                            break;
                        case "Hybrid":
                            break;
                        default:
                            break;
                    }
                }
            }

            return returnValue;
        }

        // Items are selected sequentially in the order they appear in the study list,
        // without regard for whether they have been seen before.
        protected bool SetNextIndexForward()
        {
            int count = ItemCount;

            LogicalIndex++;

            if (LogicalIndex >= count)
            {
                //if (IsTest || IsReviewTest)
                //{
                IsEndOfStudy = true;
                LogicalIndex = count;
                //}
                //else
                //    LogicalIndex = 0;
            }

            if (LogicalIndex < ItemCount)
            {
                CurrentIndex = SequentialIndices[LogicalIndex];

                if (Session.IsNewLimitReached() || Session.IsReviewLimitReached())
                    IsEndOfStudy = true;
            }
            else
            {
                CurrentIndex = -1;
                IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        // Items are selected sequentially in the reverse order they appear in the study list,
        // without regard for whether they have been seen before.
        protected bool SetNextIndexReverse()
        {
            int count = ItemCount;

            if (LogicalIndex == -1)
                LogicalIndex = count;

            LogicalIndex--;

            if (LogicalIndex < 0)
            {
                //if (IsTest || IsReviewTest)
                //{
                    IsEndOfStudy = true;
                    LogicalIndex = 0;
                //}
                //else if (count != 0)
                //    LogicalIndex = count - 1;
                //else
                //    LogicalIndex = 0;
            }

            if (LogicalIndex < ItemCount)
            {
                CurrentIndex = SequentialIndices[LogicalIndex];

                if (Session.IsNewLimitReached() || Session.IsReviewLimitReached())
                    IsEndOfStudy = true;
            }
            else
            {
                CurrentIndex = -1;
                IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        // Items are selected randomly from the study list, without regard for whether they
        // have been seen before.  The IsRandomUnique option indicates that all items should
        // be seen once before seeing an item again.
        protected bool SetNextIndexRandom()
        {
            if (IsRandomUnique)
            {
                int count = ItemCount;

                LogicalIndex++;

                if (LogicalIndex >= count)
                {
                    if (IsTest || IsReviewTest)
                    {
                        IsEndOfStudy = true;
                        LogicalIndex = count;
                    }
                    else
                        InitializeRandomIndices();
                }

                if (LogicalIndex < ItemCount)
                    CurrentIndex = RandomIndices[LogicalIndex];
                else
                {
                    CurrentIndex = -1;
                    IsEndOfStudy = true;
                }
            }
            else if (ItemCount != 0)
                CurrentIndex = SequentialIndices[random.Next(0, ItemCount)];
            else
            {
                CurrentIndex = -1;
                IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        // Uses the user response of how well they know an item to determine when the item will
        // be reviewed again.  Items not known well will be reviewed sooner (after a shorter interval)
        // than items known well.
        protected bool SetNextIndexAdaptive()
        {
            bool returnValue;

            if (IsAdaptiveMixNew)
            {
                if (IsRandomNew)
                    returnValue = SetNextIndexAdaptiveMixRandom();
                else
                    returnValue = SetNextIndexAdaptiveMixSequential();
            }
            else
            {
                if (IsRandomNew)
                    returnValue = SetNextIndexAdaptiveRandom();
                else
                    returnValue = SetNextIndexAdaptiveSequential();
            }

            return returnValue;
        }

        protected bool SetNextIndexAdaptiveSequential()
        {
            if (IsTest || IsReviewTest)
                return SetNextIndexForward();

            int count = ToolStudyItemCount;
            int index;
            int bestTouchCount = 0x7fffffff;
            int bestTouchCountIndex = -1;
            DateTime nowTime = NowTime;
            DateTime bestNewTime = nowTime;
            DateTime bestTime = nowTime;
            int bestNewTimeIndex = -1;
            int bestTimeIndex = -1;
            int newCount = 0;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            List<int> nonDueEntries = new List<int>(count);

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if ((toolStudyItem.StudyItem != null) && !toolStudyItem.StudyItem.HasText())
                    continue;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                if (index != CurrentIndex)
                {
                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            if (IsReview)
                                break;
                            if (Session.IsNewLimitReached())
                                break;
                            if ((bestTouchCountIndex == -1) || (bestTouchCount != 0))
                            {
                                bestTouchCountIndex = index;
                                bestTouchCount = 0;
                            }
                            newCount++;
                            break;
                        case ToolItemStatusCode.Active:
                            if (IsReview && (toolItemStatus.TouchCount == 0))
                                break;
                            if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                                break;
                            if (Session.IsReviewLimitReached())
                                break;
                            if (toolItemStatus.TouchCount < bestTouchCount)
                            {
                                bestTouchCountIndex = index;
                                bestTouchCount = toolItemStatus.TouchCount;
                            }
                            if (toolItemStatus.TouchCount > 0)
                            {
                                if (toolItemStatus.LastTouchTime > SessionStart)
                                {
                                    if (toolItemStatus.NextTouchTime < bestNewTime)
                                    {
                                        bestNewTimeIndex = index;
                                        bestNewTime = toolItemStatus.NextTouchTime;
                                    }
                                }
                                if (toolItemStatus.NextTouchTime < bestTime)
                                {
                                    bestTimeIndex = index;
                                    bestTime = toolItemStatus.NextTouchTime;
                                }
                                if (toolItemStatus.NextTouchTime > nowTime)
                                    nonDueEntries.Add(index);
                            }
                            else
                                newCount++;
                            break;
                        case ToolItemStatusCode.Learned:
                            break;
                        default:
                            break;
                    }
                }
            }
            if (bestNewTimeIndex != -1)
                CurrentIndex = bestNewTimeIndex;
            else if (bestTimeIndex != -1)
                CurrentIndex = bestTimeIndex;
            else
            {
                if ((bestTouchCount != 0) && (nonDueEntries.Count() > 1))
                    CurrentIndex = nonDueEntries[random.Next(0, nonDueEntries.Count())];
                else if (bestTouchCountIndex != -1)
                    CurrentIndex = bestTouchCountIndex;
                else
                    CurrentIndex = -1;

                if (newCount == 0)
                    IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        protected bool SetNextIndexAdaptiveRandom()
        {
            if (IsTest || IsReviewTest)
                return SetNextIndexRandom();

            int count = ToolStudyItemCount;
            int index;
            int bestTouchCount = 0x7fffffff;
            int bestTouchCountIndex = -1;
            DateTime nowTime = NowTime;
            DateTime bestNewTime = nowTime;
            DateTime bestTime = nowTime;
            int bestNewTimeIndex = -1;
            int bestTimeIndex = -1;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            List<int> newEntries = new List<int>(count);
            List<int> nonDueEntries = new List<int>(count);

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if ((toolStudyItem.StudyItem != null) && !toolStudyItem.StudyItem.HasText())
                    continue;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                if (index != CurrentIndex)
                {
                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            if (IsReview)
                                break;
                            if (!Session.IsNewLimitReached())
                            {
                                newEntries.Add(index);
                                if ((bestTouchCountIndex == -1) || (bestTouchCount != 0))
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = 0;
                                }
                            }
                            break;
                        case ToolItemStatusCode.Active:
                            if (IsReview && (toolItemStatus.TouchCount == 0))
                                break;
                            if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                                break;
                            if (toolItemStatus.TouchCount < bestTouchCount)
                            {
                                if (!Session.IsReviewLimitReached())
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = toolItemStatus.TouchCount;
                                }
                            }
                            if (toolItemStatus.TouchCount == 0)
                            {
                                if (!Session.IsNewLimitReached())
                                    newEntries.Add(index);
                            }
                            else if (!Session.IsReviewLimitReached())
                            {
                                if (toolItemStatus.LastTouchTime > SessionStart)
                                {
                                    if (toolItemStatus.NextTouchTime < bestNewTime)
                                    {
                                        bestNewTimeIndex = index;
                                        bestNewTime = toolItemStatus.NextTouchTime;
                                    }
                                }
                                if (toolItemStatus.NextTouchTime < bestTime)
                                {
                                    bestTimeIndex = index;
                                    bestTime = toolItemStatus.NextTouchTime;
                                }
                            }
                            if (toolItemStatus.NextTouchTime > nowTime)
                                nonDueEntries.Add(index);
                            break;
                        case ToolItemStatusCode.Learned:
                            break;
                        default:
                            break;
                    }
                }
            }
            if (bestNewTimeIndex != -1)
                CurrentIndex = bestNewTimeIndex;
            else if (bestTimeIndex != -1)
                CurrentIndex = bestTimeIndex;
            else if (newEntries.Count() != 0)
                CurrentIndex = newEntries[random.Next(0, newEntries.Count())];
            else
            {
                if (nonDueEntries.Count() > 1)
                    CurrentIndex = nonDueEntries[random.Next(0, nonDueEntries.Count())];
                else if (bestTouchCountIndex != -1)
                    CurrentIndex = bestTouchCountIndex;
                else
                    CurrentIndex = -1;

                IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        protected bool SetNextIndexAdaptiveMixSequential()
        {
            if (IsTest)
                return SetNextIndexForward();

            switch (random.Next(0, 2))
            {
                case 0:
                    return SetNextIndexAdaptiveSequential();
                case 1:
                    if (SetNextIndexNew())
                        return true;
                    return SetNextIndexAdaptiveSequential();
                default:
                    return false;
            }
        }

        protected bool SetNextIndexAdaptiveMixRandom()
        {
            if (IsTest)
                return SetNextIndexRandom();

            switch (random.Next(0, 2))
            {
                case 0:
                    return SetNextIndexAdaptiveRandom();
                case 1:
                    if (SetNextIndexNew())
                        return true;
                    return SetNextIndexAdaptiveRandom();
                default:
                    return false;
            }
        }

        protected bool SetNextIndexNew()
        {
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            int index;
            int itemIndex;

            if (Session.IsNewLimitReached())
                return false;

            for (index = 0; index < ItemCount; index++)
            {
                LogicalIndex++;

                if (LogicalIndex >= ItemCount)
                    LogicalIndex = 0;

                if (IsRandomNew)
                    itemIndex = RandomIndices[LogicalIndex];
                else
                    itemIndex = SequentialIndices[LogicalIndex];

                toolStudyItem = GetToolStudyItemIndexed(itemIndex);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if (toolItemStatus.TouchCount == 0)
                {
                    if (toolItemStatus.StatusCode != ToolItemStatusCode.Learned)
                    {
                        CurrentIndex = itemIndex;
                        return true;
                    }
                }
            }

            return false;
        }

        // Selects a smaller subgroup of items to see or review, until all items can be recalled
        // from memory.  Then the next subgroup of items will be selected for review.  The groups
        // (or "chunks") can be selected sequentially or randomly via the IsRandomNew option.
        protected bool SetNextIndexChunky()
        {
            if (IsTest || IsReviewTest)
            {
                if (IsRandomNew)
                    return SetNextIndexRandom();
                else
                    return SetNextIndexForward();
            }

            int count = ItemCount;
            int index;
            int bestTouchCount = 0x7fffffff;
            int bestTouchCountIndex = -1;
            int startIndex;
            int endIndex;
            int subIndex;
            float goodLimit = 1.0f + (1.0f / Profile.SampleSize);
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;

            if (WasReset)
            {
                for (index = 0; index < count; index++)
                {
                    int entryIndex = (IsRandomNew ? RandomIndices[index] : SequentialIndices[index]);

                    toolStudyItem = GetToolStudyItemIndexed(entryIndex);
                    toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                    if ((toolStudyItem.StudyItem != null) && !toolStudyItem.StudyItem.HasText())
                        continue;

                    if (toolItemStatus.TouchCount < bestTouchCount)
                    {
                        bestTouchCountIndex = index;
                        bestTouchCount = toolItemStatus.TouchCount;
                    }
                }

                if (bestTouchCountIndex == -1)
                    bestTouchCountIndex = 0;

                LogicalIndex = (bestTouchCountIndex / ChunkSize) * ChunkSize;
                InitializeCurrentChunk();
            }

            if (count == 0)
            {
                IsEndOfStudy = true;
                return !IsEndOfStudy;
            }

            if (LogicalIndex >= count)
            {
                LogicalIndex = 0;
                IsEndOfStudy = true;
            }
            else if (LogicalIndex == -1)
                LogicalIndex = 0;

            CurrentIndex = (IsRandomNew ? RandomIndices[LogicalIndex] : SequentialIndices[LogicalIndex]);

            // If we are at the start of a chunk.
            if ((LogicalIndex % ChunkSize) == 0)
            {
                startIndex = ((LogicalIndex / ChunkSize) * ChunkSize) - ChunkSize;
                endIndex = startIndex + ChunkSize;

                if (startIndex >= 0)
                {
                    if (endIndex > count)
                        endIndex = count;

                    for (index = startIndex; index < endIndex; index++)
                    {
                        subIndex = (IsRandomNew ? RandomIndices[index] : SequentialIndices[index]);
                        toolStudyItem = GetToolStudyItemIndexed(subIndex);
                        toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                        if (toolItemStatus.LastGrade < goodLimit)
                            break;
                    }

                    if (index < endIndex)
                    {
                        LogicalIndex = startIndex;
                        InitializeCurrentChunk();
                        CurrentIndex = (IsRandomNew ? RandomIndices[LogicalIndex] : SequentialIndices[LogicalIndex]);
                    }
                }
            }

            LogicalIndex++;

            if (LogicalIndex > count)
            {
                startIndex = ((LogicalIndex / ChunkSize) * ChunkSize) - ChunkSize;
                endIndex = startIndex + ChunkSize;

                if (startIndex >= 0)
                {
                    if (endIndex > count)
                        endIndex = count;

                    for (index = startIndex; index < endIndex; index++)
                    {
                        subIndex = (IsRandomNew ? RandomIndices[index] : SequentialIndices[index]);
                        toolStudyItem = GetToolStudyItemIndexed(subIndex);
                        toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                        if (toolItemStatus.LastGrade < goodLimit)
                            break;
                    }

                    if (index < endIndex)
                    {
                        LogicalIndex = startIndex;
                        InitializeCurrentChunk();
                        CurrentIndex = (IsRandomNew ? RandomIndices[LogicalIndex] : SequentialIndices[LogicalIndex]);
                    }
                    else
                        LogicalIndex = 0;
                }
                else
                    LogicalIndex = 0;

                IsEndOfStudy = true;
            }

            return !IsEndOfStudy;
        }

        // Implements a "tiered" algorithm, where knowing an item moves it to the next higher teir.
        // Items not known are kicked back to the lowest tier or level.  New unseen items are
        // selected either randomly or sequentially via the IsRandomNew option.
        protected bool SetNextIndexLeitner()
        {
            if (IsRandomNew)
                return SetNextIndexLeitnerRandom();
            else
                return SetNextIndexLeitnerSequential();
        }

        protected bool SetNextIndexLeitnerSequential()
        {
            if (IsTest || IsReviewTest)
                return SetNextIndexForward();

            int count = ToolStudyItemCount;
            int index;
            int bestTouchCount = 0x7fffffff;
            int bestTouchCountIndex = -1;
            int bestStage = -1;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if ((toolStudyItem.StudyItem != null) && !toolStudyItem.StudyItem.HasText())
                    continue;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                if (index != CurrentIndex)
                {
                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            break;
                        case ToolItemStatusCode.Active:
                            if (IsReview && (toolItemStatus.TouchCount == 0))
                                break;
                            if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                                break;
                            if (bestStage == -1)
                                bestStage = toolItemStatus.Stage;
                            else if (toolItemStatus.Stage < bestStage)
                                bestStage = toolItemStatus.Stage;
                            break;
                        case ToolItemStatusCode.Learned:
                            break;
                        default:
                            break;
                    }
                }
            }

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                if (toolStudyItem.IsStudyItemHidden)
                    continue;
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);
                if (index != CurrentIndex)
                {
                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            if (IsReview)
                                break;
                            if (!Session.IsNewLimitReached())
                            {
                                if ((bestTouchCountIndex == -1) || (bestTouchCount != 0))
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = 0;
                                }
                            }
                            break;
                        case ToolItemStatusCode.Active:
                            if (IsReview && (toolItemStatus.TouchCount == 0))
                                break;
                            if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                                break;
                            if ((bestStage != -1) && (toolItemStatus.Stage > bestStage))
                                break;
                            if (!Session.IsReviewLimitReached())
                            {
                                if (toolItemStatus.TouchCount < bestTouchCount)
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = toolItemStatus.TouchCount;
                                    bestStage = toolItemStatus.Stage;
                                }
                            }
                            break;
                        case ToolItemStatusCode.Learned:
                            break;
                        default:
                            break;
                    }
                }
            }

            if (bestTouchCountIndex != -1)
                CurrentIndex = bestTouchCountIndex;
            else
                CurrentIndex = -1;

            IsEndOfStudy = (CurrentIndex == -1);

            return !IsEndOfStudy;
        }

        protected bool SetNextIndexLeitnerRandom()
        {
            if (IsTest || IsReviewTest)
                return SetNextIndexRandom();

            int count = ToolStudyItemCount;
            int index;
            int bestTouchCount = 0x7fffffff;
            int bestTouchCountIndex = -1;
            int bestStage = -1;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;

            List<int> newEntries = new List<int>(count);

            for (index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                if ((toolStudyItem.StudyItem != null) && !toolStudyItem.StudyItem.HasText())
                    continue;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                if (index != CurrentIndex)
                {
                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            if (IsReview)
                                break;
                            if (!Session.IsNewLimitReached())
                            {
                                newEntries.Add(index);
                                if ((bestTouchCountIndex == -1) || (bestTouchCount != 0))
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = 0;
                                }
                            }
                            break;
                        case ToolItemStatusCode.Active:
                            if (IsReview && (toolItemStatus.TouchCount == 0))
                                break;
                            if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                                break;
                            if ((bestStage != -1) && (toolItemStatus.Stage > bestStage))
                                break;
                            if (!Session.IsReviewLimitReached())
                            {
                                if (toolItemStatus.TouchCount < bestTouchCount)
                                {
                                    bestTouchCountIndex = index;
                                    bestTouchCount = toolItemStatus.TouchCount;
                                    bestStage = toolItemStatus.Stage;
                                }
                                if (toolItemStatus.TouchCount == 0)
                                    newEntries.Add(index);
                            }
                            break;
                        case ToolItemStatusCode.Learned:
                            break;
                        default:
                            break;
                    }
                }
            }

            if ((bestTouchCount == 0) && (newEntries.Count() > 1))
                CurrentIndex = newEntries[random.Next(0, newEntries.Count())];
            else if (bestTouchCountIndex != -1)
                CurrentIndex = bestTouchCountIndex;
            else
                CurrentIndex = -1;

            IsEndOfStudy = (CurrentIndex == -1);

            return !IsEndOfStudy;
        }

        // Implements a "spaced-repetition algorithm, where items are reviewed on a schedule
        // of decreasing frequency.  New unseen items are selected either randomly or sequentially
        // via the IsRandomNew option.
        protected bool SetNextIndexSpacedRepetition()
        {
            if (IsRandomNew)
                return SetNextIndexAdaptiveRandom();
            else
                return SetNextIndexAdaptiveSequential();
        }

        // Implements a "spaced-repetition algorithm remeniscent of Anki.
        // New unseen items are selected either randomly or sequentially
        // via the IsRandomNew option.
        protected bool SetNextIndexSpanki()
        {
            if (IsRandomNew)
                return SetNextIndexAdaptiveRandom();
            else
                return SetNextIndexAdaptiveSequential();
        }

        // Implements a "spaced-repetition algorithm where the user selected the next review time offset.
        // New unseen items are selected either randomly or sequentially
        // via the IsRandomNew option.
        protected bool SetNextIndexManual()
        {
            if (IsRandomNew)
                return SetNextIndexAdaptiveRandom();
            else
                return SetNextIndexAdaptiveSequential();
        }

        public void ResetCheck()
        {
            if ((SequentialIndices == null) || (LogicalIndex == -1))
                Reset();
        }

        public void Reset()
        {
            _HybridConfigurationKey = null;

            switch (SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                    InitializeSequentialIndices();
                    break;
                case SelectorAlgorithmCode.Random:
                    if (IsRandomUnique)
                        InitializeRandomIndices();
                    else
                        InitializeSequentialIndices();
                    break;
                case SelectorAlgorithmCode.Adaptive:
                    if (IsRandomNew)
                        InitializeRandomIndices();
                    else
                        InitializeSequentialIndices();
                    break;
                case SelectorAlgorithmCode.Chunky:
                    if (IsRandomNew)
                        InitializeRandomIndices();
                    else
                        InitializeSequentialIndices();
                    break;
                case SelectorAlgorithmCode.Leitner:
                    if (IsRandomNew)
                        InitializeRandomIndices();
                    else
                        InitializeSequentialIndices();
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    if (IsRandomNew)
                        InitializeRandomIndices();
                    else
                        InitializeSequentialIndices();
                    break;
                default:
                    break;
            }

            Ordinal = 0;
            ResetChoiceIndices();
            ResetWordBlanksIndices();
            ResetHybrid();
            WasReset = true;
        }

        protected void InitializeSequentialIndices()
        {
            SetupSequentialIndices();

            /*
            if (SelectorAlgorithm == SelectorAlgorithmCode.Reverse)
                LogicalIndex = SequentialIndices.Count() - 1;
            else
                LogicalIndex = 0;
            */
            LogicalIndex = -1;
            CurrentIndex = -1;

            if (ItemCount == 0)
                IsEndOfStudy = true;
            else
            {
                //CurrentIndex = SequentialIndices[LogicalIndex];
                IsEndOfStudy = false;
                SetNextIndex();
                InitializeHybridIndices();
            }

            Ordinal = 0;
            ModifiedFlag = true;
        }

        protected void SetupSequentialIndices()
        {
            _HybridConfigurationKey = null;

            int count = ToolStudyItemCount;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;

            if (SequentialIndices != null)
                SequentialIndices.Clear();
            else
                SequentialIndices = new List<int>(count);

            _CachedStudyItemCount = count;

            if (count == 0)
            {
                IsEndOfStudy = true;
                CurrentIndex = -1;
                LastIndex = -1;
                LogicalIndex = -1;
                return;
            }

            for (int index = 0; index < count; index++)
            {
                toolStudyItem = GetToolStudyItemIndexed(index);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);

                toolItemStatus.Index = -1;

                if ((toolStudyItem.StudyItem == null) || !toolStudyItem.StudyItem.HasText())
                    continue;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                switch (toolItemStatus.StatusCode)
                {
                    case ToolItemStatusCode.Future:
                        if (IsReview || IsReviewTest)
                            break;
                        toolItemStatus.Index = SequentialIndices.Count();
                        SequentialIndices.Add(index);
                        break;
                    case ToolItemStatusCode.Active:
                        if (IsNewOnly && (toolItemStatus.TouchCount != 0) && (toolItemStatus.LastTouchTime < SessionStart))
                            break;
                        if ((IsReview || IsReviewTest) && (toolItemStatus.TouchCount == 0))
                            break;
                        toolItemStatus.Index = SequentialIndices.Count();
                        SequentialIndices.Add(index);
                        break;
                    case ToolItemStatusCode.Learned:
                        if ((IsTest || IsReviewTest) && !IsNewOnly)
                        {
                            toolItemStatus.Index = SequentialIndices.Count();
                            SequentialIndices.Add(index);
                        }
                        break;
                    default:
                        break;
                }
            }

            ModifiedFlag = true;
        }

        protected void InitializeRandomIndices()
        {
            int index;
            int count = ToolStudyItemCount;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;

            SetupSequentialIndices();

            if (RandomIndices != null)
                RandomIndices.Clear();
            else
                RandomIndices = new List<int>(count);

            LogicalIndex = 0;
            CurrentIndex = -1;

            if (count == 0)
                return;

            List<int> tmpIndices = new List<int>(SequentialIndices);

            count = tmpIndices.Count();

            for (index = 0; index < count; index++)
            {
                int tmpIndex = random.Next(0, tmpIndices.Count());
                int tmpValue = tmpIndices[tmpIndex];
                tmpIndices.RemoveAt(tmpIndex);
                RandomIndices.Add(tmpValue);
                toolStudyItem = GetToolStudyItemIndexed(tmpValue);
                toolItemStatus = toolStudyItem.GetStatus(ConfigurationKey);
                toolItemStatus.Index = index;
            }

            if (RandomIndices.Count() == 0)
                IsEndOfStudy = true;
            else
            {
                //CurrentIndex = RandomIndices[LogicalIndex];
                IsEndOfStudy = false;
                SetNextIndex();
                InitializeHybridIndices();
            }

            ModifiedFlag = true;
        }

        public List<int> GetRandomIndexArray(bool initialize)
        {
            if (initialize || (RandomIndices == null))
                InitializeRandomIndices();

            return RandomIndices;
        }

        public bool ToolStudyItemCountChanged()
        {
            if ((ToolStudyList != null) && (_CachedStudyItemCount != -1))
            {
                if (_CachedStudyItemCount != ToolStudyItemCount)
                    return true;
            }

            return false;
        }

        public bool RandomAndSequentialOutOfSync()
        {
            if (RandomIndices != null)
            {
                if (SequentialIndices == null)
                    return true;

                if (RandomIndices.Count() != SequentialIndices.Count())
                    return true;
            }
            else if (SequentialIndices != null)
                return true;

            return false;
        }

        public int SequentialIndexCount()
        {
            if (SequentialIndices != null)
                return SequentialIndices.Count();

            return 0;
        }

        public int GetSequentialIndex(int index)
        {
            if ((SequentialIndices != null) && (index >= 0) && (index < SequentialIndices.Count()))
                return SequentialIndices[index];

            return 0;
        }

        public int RandomIndexCount()
        {
            if (RandomIndices != null)
                return RandomIndices.Count();

            return 0;
        }

        public int GetRandomIndex(int index)
        {
            if ((RandomIndices != null) && (index >= 0) && (index < RandomIndices.Count()))
                return RandomIndices[index];

            return 0;
        }

        protected void InitializeHybridIndices()
        {
            ToolConfiguration configuration = Session.ToolConfiguration;

            if ((configuration != null) && configuration.IsHybrid()
                && (configuration.SubConfigurationCount() != 0)
                && !IsEndOfStudy && (CurrentIndex != -1))
            {
                ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(CurrentIndex);
                ToolItemStatus toolItemStatus = toolStudyItem.GetStatus(configuration.KeyString);

                foreach (string subConfigurationKey in configuration.SubConfigurationKeys)
                {
                    toolItemStatus = toolStudyItem.GetStatus(subConfigurationKey);
                    toolItemStatus.Index = CurrentIndex;
                }
            }
        }

        public void InitializeCurrentChunk()
        {
            int startIndex = (LogicalIndex / ChunkSize) * ChunkSize;
            int endIndex = startIndex + ChunkSize;
            List<int> tmpIndices = new List<int>(ChunkSize);

            if (endIndex > ItemCount)
                endIndex = ItemCount;

            for (int index = startIndex; index < endIndex; index++)
            {
                if (IsRandomNew)
                    tmpIndices.Add(RandomIndices[index]);
                else
                    tmpIndices.Add(SequentialIndices[index]);
            }

            for (int index = 0; index < ChunkSize; index++)
            {
                int tmpIndex = random.Next(0, tmpIndices.Count());
                int tmpCount = tmpIndices.Count();

                if (tmpIndex < tmpCount)
                {
                    int tmpValue = tmpIndices[tmpIndex];
                    tmpIndices.RemoveAt(tmpIndex);

                    if (IsRandomNew)
                        RandomIndices[startIndex + index] = tmpValue;
                    else
                        SequentialIndices[startIndex + index] = tmpValue;
                }
            }

            LogicalIndex = startIndex;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XElement("LastIndex", LastIndex));
            element.Add(new XElement("CurrentIndex", CurrentIndex));
            element.Add(new XElement("Ordinal", Ordinal));
            element.Add(new XElement("SequentialIndices", ObjectUtilities.GetStringFromIntList(SequentialIndices)));
            element.Add(new XElement("RandomIndices", ObjectUtilities.GetStringFromIntList(RandomIndices)));
            element.Add(new XElement("CachedStudyItemCount", _CachedStudyItemCount));
            element.Add(new XElement("LogicalIndex", LogicalIndex));
            element.Add(new XElement("IsCustomTime", IsCustomTime));
            element.Add(new XElement("IsEndOfStudy", IsEndOfStudy));
            element.Add(new XElement("Mode", Mode.ToString()));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            string value = childElement.Value;
            bool returnValue = true;

            switch (childElement.Name.LocalName)
            {
                case "LastIndex":
                    LastIndex = ObjectUtilities.GetIntegerFromString(value, 0);
                    break;
                case "CurrentIndex":
                    CurrentIndex = ObjectUtilities.GetIntegerFromString(value, 0);
                    break;
                case "Ordinal":
                    Ordinal = ObjectUtilities.GetIntegerFromString(value, 0);
                    break;
                case "SequentialIndices":
                    SequentialIndices = TextUtilities.GetIntListFromString(value);
                    break;
                case "RandomIndices":
                    RandomIndices = TextUtilities.GetIntListFromString(value);
                    break;
                case "CachedStudyItemCount":
                    _CachedStudyItemCount = ObjectUtilities.GetIntegerFromString(value, -1);
                    break;
                case "ChoiceIndices":       // Legacy
                    break;
                case "CorrectChoiceIndex":  // Legacy
                    break;
                case "ChosenChoiceIndex":   // Legacy
                    break;
                case "WordBlanksIndices":   // Legacy
                    break;
                case "LogicalIndex":
                    LogicalIndex = ObjectUtilities.GetIntegerFromString(value, 0);
                    break;
                case "IsCustomTime":
                    IsCustomTime = ObjectUtilities.GetBoolFromString(value, false);
                    break;
                case "IsEndOfStudy":
                    IsEndOfStudy = ObjectUtilities.GetBoolFromString(value, false);
                    break;
                case "Mode":
                    Mode = GetToolSelectorModeFromString(value);
                    break;
                default:
                    returnValue = base.OnChildElement(childElement);
                    break;
            }

            return returnValue;
        }

        public static ToolSelectorMode GetToolSelectorModeFromString(string str)
        {
            ToolSelectorMode mode;

            if (str == null)
                str = String.Empty;

            switch (str.ToLower())
            {
                case "normal":
                    mode = ToolSelectorMode.Normal;
                    break;
                case "new only":
                case "newonly":
                    mode = ToolSelectorMode.NewOnly;
                    break;
                case "review":
                    mode = ToolSelectorMode.Review;
                    break;
                case "test":
                    mode = ToolSelectorMode.Test;
                    break;
                case "review test":
                case "reviewtest":
                    mode = ToolSelectorMode.ReviewTest;
                    break;
                case "unknown":
                case "":
                case null:
                    mode = ToolSelectorMode.Unknown;
                    break;
                default:
                    if (ObjectUtilities.IsNumberString(str))
                    {
                        mode = (ToolSelectorMode)Convert.ToInt32(str);
                        break;
                    }
                    else
                        throw new Exception("GetToolSelectorModeFromString: Unknown code: " + str);
            }

            return mode;
        }
    }
}

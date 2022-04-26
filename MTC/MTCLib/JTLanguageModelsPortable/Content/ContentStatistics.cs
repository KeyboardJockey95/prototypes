using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Content
{
    // Overall status for item.
    // Note that the order is significant.  The lowest numerical values have dominance over the higher.
    // 
    public enum ContentStatisticsStatusCode
    {
        Due,                                            // Item is active and due.
        Future,                                         // Item hasn't been seen yet.
        Active,                                         // Item is active and not due.
        Complete,                                       // Item has been completed.
        Disabled                                        // Status is disabled for this item.
    }

    public class ContentStatistics : BaseObjectKeyed
    {
        // The type of the source object key.
        protected Type _SourceKeyType;

        // The key of the source.
        protected object _SourceKey;

        // The type of the source object.
        protected Type _SourceType;

        // The content type ("Courses", "Plans", "Course", "Plan", "Group", "Lesson",
        // "Words", "Sentences", "Text", "AudioLesson", etc.).
        protected string _ContentType;

        // The content type ("Tree", "Node", "StudyList", "MediaItem", "DocumentItem").
        protected string _ContentClass;

        // If true, collects content descendent items.
        protected bool _CollectDescendents;

        // The tool configuration key this status is associated with.
        protected string _ToolConfigurationKey;

        // The tool source code this status is associated with.
        protected ToolSourceCode _ToolSource;

        // The status of the item.
        protected ContentStatisticsStatusCode _Status;

        // If true, status has been overridden by the user.
        protected bool _StatusOverridden;

        // If true, status should not be propagated to the parent.
        protected bool _HideStatisticsFromParent;

        // The count of all leaves with Future status.
        protected int _FutureCount;

        // The count of all leaves with Active status that are due.
        protected int _DueCount;

        // The count of all leaves with Active status and not due or due-ness unknown.
        protected int _ActiveCount;

        // The count of all leaves with Complete status.
        protected int _CompleteCount;

        // How many times the source was visited.
        protected int _VisitedCount;

        // Next review time of study item just above now (to determine if status should be updated).
        protected DateTime _UpdateReviewTime;

        // Earliest next review time.
        protected DateTime _NextReviewTime;

        // Last time the review status was checked.
        protected DateTime _LastCheckTime;

        // Current media playback time.
        protected TimeSpan _MediaTime;

        // Cached media time length.
        protected TimeSpan _MediaLength;

        // Parent - not saved.
        protected ContentStatistics _Parent;

        // Children.
        protected List<ContentStatistics> _Children;

        public ContentStatistics(
            IBaseObjectKeyed sourceObject,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            UserRecord userRecord,
            UserProfile userProfile)
            : base(ComposeKey(sourceObject, userRecord, userProfile))
        {
            ClearContentStatistics();
            InitializeFromSourceObject(sourceObject);
            _ToolConfigurationKey = toolConfigurationKey;
            _ToolSource = toolSource;
        }

        public ContentStatistics(
            ContentStatisticsCache csCache,
            string contentType,
            bool isLesson,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            UserRecord userRecord,
            UserProfile userProfile)
            : base(ComposeRootKey(userRecord, userProfile))
        {
            ClearContentStatistics();
            InitializeFromCache(csCache, contentType, isLesson);
            _ToolConfigurationKey = toolConfigurationKey;
            _ToolSource = toolSource;
        }

        public ContentStatistics(
                IBaseObjectKeyed sourceObject,
                string toolConfigurationKey,
                ToolSourceCode toolSource,
                UserRecord userRecord,
                UserProfile userProfile,
                ContentStatisticsStatusCode status,
                int futureCount,
                int dueCount,
                int activeCount,
                int completeCount,
                int visitedCount,
                DateTime nextReviewTime)
            : base(ComposeKey(sourceObject, userRecord, userProfile))
        {
            ClearContentStatistics();
            InitializeFromSourceObject(sourceObject);
            _ToolConfigurationKey = toolConfigurationKey;
            _ToolSource = toolSource;
            _Status = status;
            _FutureCount = futureCount;
            _DueCount = dueCount;
            _ActiveCount = activeCount;
            _CompleteCount = completeCount;
            _VisitedCount = visitedCount;
            _NextReviewTime = nextReviewTime;
        }

        public ContentStatistics(ContentStatistics other)
            : base(other)
        {
            CopyContentStatistics(other);
        }

        public ContentStatistics(XElement element)
        {
            ClearContentStatistics();
            OnElement(element);
        }

        public ContentStatistics()
        {
            ClearContentStatistics();
        }

        public override void Clear()
        {
            ClearContentStatistics();
        }

        public void ClearContentStatistics()
        {
            _SourceKeyType = null;
            _SourceKey = null;
            _SourceType = null;
            _ContentType = null;
            _ToolConfigurationKey = null;
            _ToolSource = ToolSourceCode.StudyList;
            _Status = ContentStatisticsStatusCode.Future;
            _StatusOverridden = false;
            _HideStatisticsFromParent = false;
            _FutureCount = 0;
            _DueCount = 0;
            _ActiveCount = 0;
            _CompleteCount = 0;
            _VisitedCount = 0;
            _UpdateReviewTime = DateTime.MaxValue;
            _NextReviewTime = DateTime.MaxValue;
            _LastCheckTime = DateTime.MinValue;
            _MediaTime = TimeSpan.Zero;
            _MediaLength = TimeSpan.Zero;
            _Children = null;
        }

        public void CopyContentStatistics(ContentStatistics other)
        {
            if (other != null)
            {
                _SourceKeyType = other.SourceKeyType;
                _SourceKey = other.SourceKey;
                _SourceType = other.SourceType;
                _ContentType = other.ContentType;
                _ToolConfigurationKey = other.ToolConfigurationKey;
                _ToolSource = other.ToolSource;
                _Status = other.Status;
                _StatusOverridden = other.StatusOverridden;
                _HideStatisticsFromParent = other.HideStatisticsFromParent;
                _FutureCount = other.FutureCountLocal;
                _DueCount = other.DueCountLocal;
                _ActiveCount = other.ActiveCountLocal;
                _CompleteCount = other.CompleteCountLocal;
                _VisitedCount = other.VisitedCountLocal;
                _UpdateReviewTime = other.UpdateReviewTimeLocal;
                _NextReviewTime = other.NextReviewTimeLocal;
                _LastCheckTime = other.LastCheckTimeLocal;
                _MediaTime = other.MediaTime;
                _MediaLength = other.MediaLength;

                if (other.HasChildren())
                    _Children = CloneChildren();
                else
                    _Children = null;
            }
            else
                ClearContentStatistics();
        }

        public override IBaseObject Clone()
        {
            return new ContentStatistics(this);
        }

        public void InitializeFromSourceObject(IBaseObjectKeyed sourceObject)
        {
            if (sourceObject != null)
            {
                _SourceKeyType = sourceObject.KeyType;
                _SourceKey = sourceObject.Key;

                if (sourceObject is ObjectReferenceNodeTree)
                    _SourceType = ObjectTypes.FindType("BaseObjectNodeTree");
                else
                    _SourceType = sourceObject.GetType();

                if (sourceObject is BaseObjectContent)
                {
                    BaseObjectContent content = (BaseObjectContent)sourceObject;
                    _ContentType = content.ContentRootKey;
                    _ContentClass = content.ContentClass.ToString();
                    _CollectDescendents = content.GetOptionFlag("CollectDescendentItems", false);
                }
                else if (sourceObject is BaseObjectNodeTree)
                {
                    BaseObjectNodeTree tree = (BaseObjectNodeTree)sourceObject;
                    _ContentType = tree.Label;
                    _ContentClass = "Tree";
                }
                else if (sourceObject is BaseObjectNode)
                {
                    BaseObjectNode node = (BaseObjectNode)sourceObject;
                    _ContentType = node.Label;
                    _ContentClass = "Node";
                }
                else if (sourceObject is ObjectReferenceNodeTree)
                {
                    ObjectReferenceNodeTree treeReference = (ObjectReferenceNodeTree)sourceObject;
                    _ContentType = treeReference.Label;
                    _ContentClass = "Tree";
                }
                else
                {
                    _ContentType = _SourceType.Name;
                    _ContentClass = _SourceType.Name;
                }
            }
            else
            {
                _SourceKeyType = null;
                _SourceKey = null;
                _SourceType = null;
                _ContentType = null;
                _ContentClass = null;
                _CollectDescendents = false;
            }
        }

        public void InitializeFromCache(
            ContentStatisticsCache csCache,
            string contentType,
            bool isLesson)
        {
            if (csCache != null)
            {
                _SourceKey = "root";
                _SourceKeyType = _SourceKey.GetType();

                _SourceType = null;

                _ContentType = contentType;
                _ContentClass = "Trees";
            }

            _CollectDescendents = false;

            ContentStatisticsStatusCode rootStatus = ContentStatisticsStatusCode.Future;
            int totalCount = 0;
            ClearStudyCountsLocal();

            foreach (ContentStatistics cs in csCache.List)
            {
                cs.GetCountsGlobal(
                    "Any",
                    isLesson,
                    ref _FutureCount,
                    ref _DueCount,
                    ref _ActiveCount,
                    ref _CompleteCount,
                    ref totalCount);

                _VisitedCount += cs.GetVisitedCountGlobal("Any", isLesson);

                if (cs.Status < rootStatus)
                    rootStatus = cs.Status;
            }

            _Status = rootStatus;
        }

        // Type type of the source object key.
        public Type SourceKeyType
        {
            get
            {
                return _SourceKeyType;
            }
            set
            {
                _SourceKeyType = value;
            }
        }

        // Returns true if the source object key is an integer.
        public bool IsIntegerSourceKeyType
        {
            get
            {
                return ObjectUtilities.IsIntegerType(_SourceKeyType);
            }
        }

        // The key of the source object.
        public object SourceKey
        {
            get
            {
                return _SourceKey;
            }
            set
            {
                if (ObjectUtilities.CompareObjects(_SourceKey, value) != 0)
                {
                    ModifiedFlag = true;
                    _SourceKey = value;
                }
            }
        }

        // The key of the source object as a string.
        public string SourceKeyString
        {
            get
            {
                if (_SourceKey == null)
                    return String.Empty;
                else
                    return _SourceKey.ToString();
            }
        }

        // The key of the source object as an int.
        public int SourceKeyInt
        {
            get
            {
                if ((_SourceKey != null) && (_Key is int))
                    return (int)_SourceKey;
                return 0;
            }
        }

        // Type of the source object.
        public Type SourceType
        {
            get
            {
                return _SourceType;
            }
            set
            {
                if (value != _SourceType)
                {
                    _SourceType = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Content type string of the source object ("Tree", "Node", "Words", "Sentences", "Text", "AudioLesson", etc.).
        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                if (value != _ContentType)
                {
                    _ContentType = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Pattern may be a content type string, "Any", "Content", content storage class,
        // source type name, or source key.
        public bool MatchContentType(string pattern)
        {
            if (_ContentType == pattern)
                return true;
            else if (pattern == "Any")
                return true;
            else if (pattern == "Content")
            {
                if (_SourceType.Name == "BaseObjectContent")
                    return true;
            }
            else if (pattern == _ContentClass)
                return true;
            else if (pattern == (_SourceType != null ? _SourceType.Name : null))
                return true;
            else if (pattern == SourceKeyString)
                return true;

            return false;
        }

        // Content class string of the source object ("Tree", "Node", "StudyList", "MediaItem", "DocumentItem", etc.).
        public string ContentClass
        {
            get
            {
                return _ContentClass;
            }
            set
            {
                if (value != _ContentClass)
                {
                    _ContentClass = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Returns true if this item is for a node or tree.
        public bool IsNodeOrTree()
        {
            switch (_ContentClass)
            {
                case "Node":
                case "Tree":
                    return true;
                default:
                    return false;
            }
        }

        // Returns true if this item is for a content item.
        public bool IsContent()
        {
            switch (_ContentClass)
            {
                case "StudyList":
                case "MediaItem":
                case "DocumentItem":
                    return true;
                default:
                    return false;
            }
        }

        // Returns true if this item is for a lesson.
        public bool IsLesson()
        {
            return (_ContentType == "Lesson");
        }

        // If true, collect descendent items.
        public bool CollectDescendents
        {
            get
            {
                return _CollectDescendents;
            }
            set
            {
                if (value != _CollectDescendents)
                {
                    _CollectDescendents = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The tool configuration key this status is associated with.
        public string ToolConfigurationKey
        {
            get
            {
                return _ToolConfigurationKey;
            }
            set
            {
                if (value != _ToolConfigurationKey)
                {
                    _ToolConfigurationKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The tool source this status is associated with.
        public ToolSourceCode ToolSource
        {
            get
            {
                return _ToolSource;
            }
            set
            {
                if (value != _ToolSource)
                {
                    _ToolSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Propagate tool source and status, also reseting counts and status, if not overridden.
        // Returns true if any were different.
        public bool PropagateToolSourceAndStatus(
            string toolConfigurationKey,
            ToolSourceCode toolSource)
        {
            bool wasDifferent = (toolConfigurationKey != _ToolConfigurationKey) ||
                (toolSource != _ToolSource);

            if (wasDifferent)
            {
                ToolConfigurationKey = toolConfigurationKey;
                ToolSource = toolSource;
                ClearStudyCountsLocal();
                _NextReviewTime = DateTime.MaxValue;
                _LastCheckTime = DateTime.MinValue;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.PropagateToolSourceAndStatus(toolConfigurationKey, toolSource))
                        wasDifferent = true;
                }
            }

            return wasDifferent;
        }

        // The status of the item.
        public ContentStatisticsStatusCode Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Returns true if this status has been disabled.
        public bool IsDisabled()
        {
            return (_Status == ContentStatisticsStatusCode.Disabled);
        }

        // The count of objects with a given status of a given content type pattern.
        public int GetGlobalStatusCount(
            ContentStatisticsStatusCode status,
            string pattern)
        {
            int count = 0;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    count += cs.GetGlobalStatusCount(status, pattern);

                    if ((cs.Status == status) && cs.MatchContentType(pattern))
                        count++;
                }
            }

            return count;
        }

        // Set local status of this from local counts.
        public ContentStatisticsStatusCode SetStatusFromCountsLocal(string pattern)
        {
            if (!_StatusOverridden && MatchContentType(pattern) && !IsDisabled())
            {
                ContentStatisticsStatusCode thisStatus = ContentStatisticsStatusCode.Future;
                int totalCount = TotalCountLocal;

                if (totalCount != 0)
                {
                    if (_DueCount != 0)
                        thisStatus = ContentStatisticsStatusCode.Due;
                    else if (_ActiveCount != 0)
                        thisStatus = ContentStatisticsStatusCode.Active;
                    else if (_CompleteCount == totalCount)
                        thisStatus = ContentStatisticsStatusCode.Complete;
                    else if (_FutureCount == totalCount)
                        thisStatus = ContentStatisticsStatusCode.Future;

                    _Status = thisStatus;
                }
            }

            return _Status;
        }

        // Set status of this and descendents from status.
        public ContentStatisticsStatusCode SetStatusFromCountsGlobal(string pattern)
        {
            if (IsDisabled())
                return ContentStatisticsStatusCode.Disabled;

            ContentStatisticsStatusCode descendentStatus = ContentStatisticsStatusCode.Future;
            bool isHaveDescendentStatus = false;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent || (cs.GetContentTypedTotalCount(pattern) == 0))
                        continue;

                    ContentStatisticsStatusCode childstatus = cs.SetStatusFromCountsGlobal(pattern);

                    isHaveDescendentStatus = true;

                    if (childstatus < descendentStatus)
                        descendentStatus = childstatus;
                }
            }

            if (!_StatusOverridden && MatchContentType(pattern))
            {
                ContentStatisticsStatusCode thisStatus = ContentStatisticsStatusCode.Future;

                if ((ContentClass == "StudyList") && BaseObjectContent.NonStudyListTextContentTypes.Contains(_ContentType))
                    _Status = ContentStatisticsStatusCode.Disabled;
                else if (ContentClass == "DocumentItem")
                    _Status = ContentStatisticsStatusCode.Disabled;
                else
                {
                    int totalCount = TotalCountLocal;

                    if (totalCount != 0)
                    {
                        if (_DueCount != 0)
                            thisStatus = ContentStatisticsStatusCode.Due;
                        else if (_ActiveCount != 0)
                            thisStatus = ContentStatisticsStatusCode.Active;
                        else if (_CompleteCount == totalCount)
                            thisStatus = ContentStatisticsStatusCode.Complete;
                        else if (_FutureCount == totalCount)
                            thisStatus = ContentStatisticsStatusCode.Future;

                        if (isHaveDescendentStatus && (descendentStatus < thisStatus))
                            _Status = descendentStatus;
                        else
                            _Status = thisStatus;
                    }
                    else
                        _Status = descendentStatus;
                }
            }

            return _Status;
        }

        // Propagate status to children.
        public void PropagateChildStatus(
            string pattern,
            ContentStatisticsStatusCode status,
            bool isOverride)
        {
            if (_CollectDescendents && (_Parent != null) && _Parent.HasChildren())
            {
                foreach (ContentStatistics cs in _Parent.Children)
                {
                    if ((cs != this) && ((cs.ContentClass == "Tree") || (cs.ContentClass == "Node")))
                    {
                        cs.PropagateChildStatus(
                            pattern,
                            status,
                            isOverride);
                    }
                }
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                    cs.PropagateChildStatus(pattern, status, isOverride);
            }

            if (MatchContentType(pattern))
            {
                _Status = status;
                _StatusOverridden = isOverride;
            }
        }

        // Propagate status to parents.
        public void PropagateParentStatus()
        {
            ContentStatistics csParent = _Parent;

            while (csParent != null)
            {
                // Stop if parent status was overridden.
                if (csParent.StatusOverridden)
                    break;

                // Status with lower numeric value has priority.
                if (_Status < csParent.Status)
                    csParent.Status = _Status;

                csParent = csParent.Parent;
            }
        }

        // Status has been overridden by the user.
        public bool StatusOverridden
        {
            get
            {
                return _StatusOverridden;
            }
            set
            {
                if (_StatusOverridden != value)
                {
                    _StatusOverridden = value;
                    ModifiedFlag = true;
                }
            }
        }

        // If true, hide statistics from parent.
        public bool HideStatisticsFromParent
        {
            get
            {
                return _HideStatisticsFromParent;
            }
            set
            {
                if (value != _HideStatisticsFromParent)
                {
                    _HideStatisticsFromParent = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The total leaf count of a given content type pattern.
        public int GetContentTypedLeafCount(string pattern)
        {
            int leafCount = 0;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                    leafCount += cs.GetContentTypedLeafCount(pattern);
            }
            else if (MatchContentType(pattern))
                return 1;

            return leafCount;
        }

        // The total descendents count of a given content type pattern.
        public int GetContentTypedDescendentsCount(string pattern)
        {
            int descendentsCount = 0;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    descendentsCount += cs.GetContentTypedDescendentsCount(pattern);

                    if (cs.MatchContentType(pattern))
                        descendentsCount++;
                }
            }

            return descendentsCount;
        }

        // The total content statistics object count of a given content type pattern.
        public int GetContentTypedTotalCount(string pattern)
        {
            int totalCount = GetContentTypedDescendentsCount(pattern);

            if (MatchContentType(pattern))
                totalCount++;

            return totalCount;
        }

        // The total study item count of this object.
        public int TotalCountLocal
        {
            get
            {
                return _FutureCount + _DueCount + _ActiveCount + _CompleteCount;
            }
        }

        // The future study item count of this object.
        public int FutureCountLocal
        {
            get
            {
                return _FutureCount;
            }
            set
            {
                if (_FutureCount != value)
                {
                    _FutureCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The sum of the counts of the study items with future status of all nodes.
        public int GetFutureCountGlobal(string pattern, bool isLesson)
        {
            int count = 0;

            if (isLesson == IsLesson())
            {
                if (_Status == ContentStatisticsStatusCode.Future)
                    return 1;
                else
                    return 0;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    count += cs.GetFutureCountGlobal(pattern, isLesson);
                }
            }

            if (MatchContentType(pattern))
                count += _FutureCount;

            return count;
        }

        // The count of all leaves with Active status that are due.
        public int DueCountLocal
        {
            get
            {
                return _DueCount;
            }
            set
            {
                if (_DueCount != value)
                {
                    _DueCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The sum of the counts of the study items with due status of all nodes.
        public int GetDueCountGlobal(string pattern, bool isLesson)
        {
            int count = 0;

            if (isLesson == IsLesson())
            {
                if (_Status == ContentStatisticsStatusCode.Due)
                    return 1;
                else
                    return 0;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    count += cs.GetDueCountGlobal(pattern, isLesson);
                }
            }
            else if (MatchContentType(pattern))
                count += _DueCount;

            return count;
        }

        // The count of all leaves with Active status that are not due or due-ness is unknown.
        public int ActiveCountLocal
        {
            get
            {
                return _ActiveCount;
            }
            set
            {
                if (_ActiveCount != value)
                {
                    _ActiveCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The sum of the counts of the study items that are not due or due-ness is unknown of all nodes.
        public int GetActiveCountGlobal(string pattern, bool isLesson)
        {
            int count = 0;

            if (isLesson == IsLesson())
            {
                if (_Status == ContentStatisticsStatusCode.Active)
                    return 1;
                else
                    return 0;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    count += cs.GetActiveCountGlobal(pattern, isLesson);
                }
            }
            else if (MatchContentType(pattern))
                count += _ActiveCount;

            return count;
        }

        // The count of all leaves with Complete status.
        public int CompleteCountLocal
        {
            get
            {
                return _CompleteCount;
            }
            set
            {
                if (_CompleteCount != value)
                {
                    _CompleteCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The sum of the counts of the study items with learned status of all nodes.
        public int GetCompleteCountGlobal(string pattern, bool isLesson)
        {
            int count = 0;

            if (isLesson == IsLesson())
            {
                if (_Status == ContentStatisticsStatusCode.Complete)
                    return 1;
                else
                    return 0;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    count += cs.GetCompleteCountGlobal(pattern, isLesson);
                }
            }
            else if (MatchContentType(pattern))
                count += _CompleteCount;

            return count;
        }

        // Get the study item counts in one function.
        public void GetCountsLocal(
            string pattern,
            ref int futureCount,
            ref int dueCount,
            ref int activeCount,
            ref int completeCount,
            ref int totalCount)
        {
            if (IsDisabled())
                return;

            if (MatchContentType(pattern))
            {
                futureCount += _FutureCount;
                dueCount += _DueCount;
                activeCount += _ActiveCount;
                completeCount += _CompleteCount;
                totalCount = futureCount + dueCount + activeCount + completeCount;
            }
        }

        // The sum of the counts of the study items with learned status of all nodes.
        public void GetCountsGlobal(
            string pattern,
            bool isLesson,
            ref int futureCount,
            ref int dueCount,
            ref int activeCount,
            ref int completeCount,
            ref int totalCount)
        {
            if (isLesson && IsLesson())
            {
                futureCount += (_Status == ContentStatisticsStatusCode.Future ? 1 : 0);
                dueCount += (_Status == ContentStatisticsStatusCode.Due ? 1 : 0);
                activeCount += (_Status == ContentStatisticsStatusCode.Active ? 1 : 0);
                completeCount += (_Status == ContentStatisticsStatusCode.Complete ? 1 : 0);
                totalCount = futureCount + dueCount + activeCount + completeCount;
                return;
            }

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    cs.GetCountsGlobal(
                        pattern,
                        isLesson,
                        ref futureCount,
                        ref dueCount,
                        ref activeCount,
                        ref completeCount,
                        ref totalCount);
                }
            }

            if (MatchContentType(pattern))
            {
                futureCount += _FutureCount;
                dueCount += _DueCount;
                activeCount += _ActiveCount;
                completeCount += _CompleteCount;
                totalCount = futureCount + dueCount + activeCount + completeCount;
            }
        }

        // The sum of the counts of the study items of all nodes.
        public void GetCountsCollectDescendents(
            string pattern,
            bool isLesson,
            ref int futureCount,
            ref int dueCount,
            ref int activeCount,
            ref int completeCount,
            ref int totalCount)
        {
            if (MatchContentType(pattern))
            {
                if ((_Parent != null) && _Parent.HasChildren())
                {
                    foreach (ContentStatistics cs in _Parent.Children)
                    {
                        if ((cs != this) && cs.IsNodeOrTree())
                        {
                            if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                                continue;

                            cs.GetCountsGlobal(
                                pattern,
                                isLesson,
                                ref futureCount,
                                ref dueCount,
                                ref activeCount,
                                ref completeCount,
                                ref totalCount);
                        }
                    }
                }

                futureCount += _FutureCount;
                dueCount += _DueCount;
                activeCount += _ActiveCount;
                completeCount += _CompleteCount;
                totalCount = futureCount + dueCount + activeCount + completeCount;
            }
        }

        // Clear global study counts.
        public void ClearStudyCountsGlobal(string pattern)
        {
            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                    cs.ClearStudyCountsGlobal(pattern);
            }

            if (MatchContentType(pattern))
            {
                _FutureCount = 0;
                _DueCount = 0;
                _ActiveCount = 0;
                _CompleteCount = 0;
            }
        }

        // Clear local study counts.
        public void ClearStudyCountsLocal()
        {
            _FutureCount = 0;
            _DueCount = 0;
            _ActiveCount = 0;
            _CompleteCount = 0;
        }

        // How many times the source was visited.
        public int VisitedCountLocal
        {
            get
            {
                return _VisitedCount;
            }
            set
            {
                if (_VisitedCount != value)
                {
                    _VisitedCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        // The sum of the visted counts of all nodes.
        public int GetVisitedCountGlobal(string pattern, bool isLesson)
        {
            int count = 0;

            if (IsDisabled())
                return 0;

            if (isLesson)
            {
                if (!IsNodeOrTree())
                    return 0;

                if (IsLesson())
                {
                    if ((_Children != null) && (_Children.Count() != 0))
                    {
                        int bestCount = 0;

                        foreach (ContentStatistics cs in _Children)
                        {
                            if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                                continue;

                            int contentCount = cs.GetMostVisitedContentCount(pattern);

                            if (contentCount > bestCount)
                                bestCount = contentCount;
                        }

                        count = bestCount;
                    }
                }
                else
                {
                    if ((_Children != null) && (_Children.Count() != 0))
                    {
                        foreach (ContentStatistics cs in _Children)
                        {
                            if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                                continue;

                            count += cs.GetVisitedCountGlobal(pattern, isLesson);
                        }
                    }
                }
            }
            else
            {
                if ((_Children != null) && (_Children.Count() != 0))
                {
                    foreach (ContentStatistics cs in _Children)
                    {
                        if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                            continue;

                        count += cs.GetVisitedCountGlobal(pattern, isLesson);
                    }
                }
                else if (MatchContentType(pattern))
                    count += _VisitedCount;
            }

            return count;
        }

        // Get most visited count for content.
        public int GetMostVisitedContentCount(string pattern)
        {
            int count = 0;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                int bestCount = 0;

                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    int contentCount = cs.GetMostVisitedContentCount(pattern);

                    if (contentCount > bestCount)
                        bestCount = contentCount;
                }

                count = bestCount;
            }

            if (_VisitedCount > count)
                count = _VisitedCount;

            return count;
        }

        // Increment visited count if first time or
        public void UpdateVisitedCount(DateTime now)
        {
            if (IsDisabled())
                return;

            if (_LastCheckTime == DateTime.MaxValue)
                _VisitedCount = 1;
            else
            {
                DateTime expirationDateTime = now.AddHours(-3);

                if (expirationDateTime > _LastCheckTime)
                    _VisitedCount = _VisitedCount + 1;

                if (_VisitedCount == 0)
                    _VisitedCount = 1;
            }
        }

        // Next review time to likely cause need for update.
        public DateTime UpdateReviewTimeLocal
        {
            get
            {
                return _UpdateReviewTime;
            }
            set
            {
                if (value != _UpdateReviewTime)
                {
                    _UpdateReviewTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Returns the earliest update review time of all nodes.
        public DateTime GetUpdateReviewTimeGlobal(string pattern)
        {
            DateTime updateReviewTime = DateTime.MaxValue;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    DateTime childTime = cs.GetUpdateReviewTimeGlobal(pattern);

                    if (childTime < updateReviewTime)
                        updateReviewTime = childTime;
                }
            }

            if (MatchContentType(pattern))
            {
                if (_UpdateReviewTime < updateReviewTime)
                    updateReviewTime = _UpdateReviewTime;
            }

            return updateReviewTime;
        }

        // Propagate update review time to parents.
        public void PropagateUpdateReviewTime(DateTime now)
        {
            if (_UpdateReviewTime < now)
                return;

            ContentStatistics csParent = _Parent;

            while (csParent != null)
            {
                if (csParent.UpdateReviewTimeLocal == DateTime.MaxValue)
                    csParent.UpdateReviewTimeLocal = _UpdateReviewTime;
                else if (csParent.UpdateReviewTimeLocal < now)
                    csParent.UpdateReviewTimeLocal = _UpdateReviewTime;
                else if (_UpdateReviewTime < csParent.UpdateReviewTimeLocal)
                    csParent.UpdateReviewTimeLocal = _UpdateReviewTime;

                csParent = csParent.Parent;
            }
        }

        // Next review time.
        public DateTime NextReviewTimeLocal
        {
            get
            {
                return _NextReviewTime;
            }
            set
            {
                if (value != _NextReviewTime)
                {
                    _NextReviewTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Returns the earliest next review time of all nodes.
        public DateTime GetNextReviewTimeGlobal(string pattern)
        {
            DateTime nextReviewTime = DateTime.MaxValue;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    DateTime childTime = cs.GetNextReviewTimeGlobal(pattern);

                    if (childTime < nextReviewTime)
                        nextReviewTime = childTime;
                }
            }

            if (MatchContentType(pattern))
            {
                if (_NextReviewTime < nextReviewTime)
                    nextReviewTime = _NextReviewTime;
            }

            return nextReviewTime;
        }

        // Clear next review time of all nodes.
        public void ClearNextReviewTimeGlobal(string pattern)
        {
            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                    cs.ClearNextReviewTimeGlobal(pattern);
            }

            if (MatchContentType(pattern))
                _UpdateReviewTime = _NextReviewTime = DateTime.MaxValue;
        }

        // Last check time.
        public DateTime LastCheckTimeLocal
        {
            get
            {
                return _LastCheckTime;
            }
            set
            {
                if (value != _LastCheckTime)
                {
                    _LastCheckTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Returns the earliest last check time of all nodes.
        public DateTime GetLastCheckTimeGlobal(string pattern)
        {
            DateTime lastCheckTime = DateTime.MaxValue;

            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                {
                    if (cs.IsDisabled() || cs.HideStatisticsFromParent)
                        continue;

                    DateTime childTime = cs.GetLastCheckTimeGlobal(pattern);

                    if (childTime < lastCheckTime)
                        lastCheckTime = childTime;
                }
            }

            if (MatchContentType(pattern))
            {
                if (_LastCheckTime < lastCheckTime)
                    lastCheckTime = _LastCheckTime;
            }

            return lastCheckTime;
        }

        public bool IsNeedsCheckLocal(string pattern, DateTime now, bool isUpdateCheckTime)
        {
            bool returnValue = false;

            if (_LastCheckTime == DateTime.MinValue)
                returnValue = true;
            else if ((_UpdateReviewTime < now) && (_LastCheckTime < _UpdateReviewTime))
                returnValue = true;

            if (isUpdateCheckTime)
                _LastCheckTime = now;

            return returnValue;
        }

        public bool IsNeedsCheckGlobal(string pattern, DateTime now, bool isUpdateCheckTime)
        {
            DateTime lastCheckTime = GetLastCheckTimeGlobal(pattern);
            DateTime updateReviewTime = GetUpdateReviewTimeGlobal(pattern);
            bool returnValue = false;

            if ((updateReviewTime < now) && (lastCheckTime < updateReviewTime))
                returnValue = true;

            if (isUpdateCheckTime)
                SetLastCheckTimeGlobal(pattern, now);

            return returnValue;
        }

        public void SetLastCheckTimeGlobal(string pattern, DateTime lastCheckTime)
        {
            if ((_Children != null) && (_Children.Count() != 0))
            {
                foreach (ContentStatistics cs in _Children)
                    cs.SetLastCheckTimeGlobal(pattern, lastCheckTime);
            }
            else if (MatchContentType(pattern))
                _LastCheckTime = lastCheckTime;
        }

        public TimeSpan MediaTime
        {
            get
            {
                return _MediaTime;
            }
            set
            {
                if (value != _MediaTime)
                {
                    _MediaTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        public TimeSpan MediaLength
        {
            get
            {
                return _MediaLength;
            }
            set
            {
                if (value != _MediaLength)
                {
                    _MediaLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ContentStatistics Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
            }
        }

        public List<ContentStatistics> Children
        {
            get
            {
                return _Children;
            }
            set
            {
                if (value != _Children)
                {
                    _Children = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<ContentStatistics> CloneChildren()
        {
            if (_Children == null)
                return null;

            List<ContentStatistics> children = new List<ContentStatistics>();

            foreach (ContentStatistics cs in _Children)
                children.Add(new ContentStatistics(cs));

            return children;
        }

        public bool HasChildren()
        {
            return (_Children != null) && (_Children.Count() != 0);
        }

        public ContentStatistics GetChild(string key)
        {
            if ((_Children == null) || String.IsNullOrEmpty(key))
                return null;

            ContentStatistics cs = _Children.FirstOrDefault(x => x.KeyString == key);

            return cs;
        }

        public ContentStatistics GetChildIndexed(int index)
        {
            if ((_Children == null) || (index < 0) || (index >= _Children.Count()))
                return null;

            ContentStatistics cs = _Children[index];

            return cs;
        }

        public ContentStatistics GetDescendentFromKey(string key)
        {
            if ((_Children == null) || String.IsNullOrEmpty(key))
                return null;

            foreach (ContentStatistics cs in _Children)
            {
                if (cs.KeyString == key)
                    return cs;

                ContentStatistics child = cs.GetDescendentFromKey(key);

                if (child != null)
                    return child;
            }

            return null;
        }

        public ContentStatistics GetDescendentFromSourceObject(
            IBaseObjectKeyed sourceObject,
            UserRecord userRecord,
            UserProfile userProfile)
        {
            string key = ComposeKey(sourceObject, userRecord, userProfile);
            return GetDescendentFromKey(key);
        }

        public ContentStatistics GetDescendentFromSourceObjectKeyAndType(
            object sourceObjectKey,
            Type sourceObjectType)
        {
            if ((_Children == null) || (sourceObjectKey == null))
                return null;

            foreach (ContentStatistics cs in _Children)
            {
                if ((sourceObjectType == cs.SourceType) && cs.MatchKey(sourceObjectKey))
                    return cs;

                ContentStatistics child = cs.GetDescendentFromSourceObjectKeyAndType(sourceObjectKey, sourceObjectType);

                if (child != null)
                    return child;
            }

            return null;
        }

        public ContentStatistics GetDescendentFromContentType(string pattern)
        {
            if ((_Children == null) || String.IsNullOrEmpty(pattern))
                return null;

            foreach (ContentStatistics cs in _Children)
            {
                if (cs.MatchContentType(pattern))
                    return cs;

                ContentStatistics child = cs.GetDescendentFromContentType(pattern);

                if (child != null)
                    return child;
            }

            return null;
        }

        public void AddChild(ContentStatistics cs)
        {
            if (cs == null)
                return;

            if (_Children != null)
                _Children.Add(cs);
            else
                _Children = new List<ContentStatistics>() { cs };

            cs.Parent = this;
        }

        public bool InsertChild(int index, ContentStatistics cs)
        {
            if (cs == null)
                return false;

            if (_Children != null)
            {
                if ((index >= 0) && (index <= _Children.Count()))
                {
                    _Children.Insert(index, cs);
                    cs.Parent = this;
                }
                else
                    return false;
            }
            else if (index == 0)
            {
                _Children = new List<ContentStatistics>() { cs };
                cs.Parent = this;
            }
            else
                return false;

            return true;
        }

        public bool DeleteChild(ContentStatistics cs)
        {
            if ((cs == null) || (_Children == null))
                return false;

            bool returnValue = _Children.Remove(cs);

            cs.Parent = null;

            return returnValue;
        }

        public bool DeleteChildIndexed(int index)
        {
            if ((_Children == null) || (index < 0) || (index >= _Children.Count()))
                return false;

            _Children.RemoveAt(index);

            return true;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_Children != null)
                {
                    foreach (ContentStatistics cs in _Children)
                    {
                        if (cs.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Children != null)
                {
                    foreach (ContentStatistics cs in _Children)
                        cs.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_SourceKeyType != null)
                element.Add(new XAttribute("SourceKeyType", _SourceKeyType.Name));

            if (_SourceKey != null)
                element.Add(new XAttribute("SourceKey", _SourceKey.ToString()));

            if (_SourceType != null)
                element.Add(new XAttribute("SourceType", _SourceType.Name));

            if (_ContentType != null)
                element.Add(new XAttribute("ContentType", _ContentType));

            if (_ContentClass != null)
                element.Add(new XAttribute("ContentClass", _ContentClass));

            if (_CollectDescendents)
                element.Add(new XAttribute("CollectDescendents", "true"));

            if (_ToolConfigurationKey != null)
                element.Add(new XAttribute("ToolConfigurationKey", ToolConfigurationKey));

            if (_ToolSource != ToolSourceCode.Unknown)
                element.Add(new XAttribute("ToolSource", _ToolSource.ToString()));

            if (_Status != ContentStatisticsStatusCode.Future)
                element.Add(new XAttribute("Status", _Status.ToString()));

            if (_StatusOverridden)
                element.Add(new XAttribute("StatusOverridden", "true"));

            if (_HideStatisticsFromParent)
                element.Add(new XAttribute("HideStatisticsFromParent", "true"));

            if (_FutureCount != 0)
                element.Add(new XAttribute("FutureCount", _FutureCount));

            if (_DueCount != 0)
                element.Add(new XAttribute("DueCount", _DueCount));

            if (_ActiveCount != 0)
                element.Add(new XAttribute("ActiveCount", _ActiveCount));

            if (_CompleteCount != 0)
                element.Add(new XAttribute("CompleteCount", _CompleteCount));

            if (_VisitedCount != 0)
                element.Add(new XAttribute("VisitedCount", _VisitedCount));

            if (_UpdateReviewTime != DateTime.MaxValue)
                element.Add(new XAttribute("UpdateReviewTime", ObjectUtilities.GetStringFromDateTime(_UpdateReviewTime)));

            if (_NextReviewTime != DateTime.MaxValue)
                element.Add(new XAttribute("NextReviewTime", ObjectUtilities.GetStringFromDateTime(_NextReviewTime)));

            if (_LastCheckTime != DateTime.MaxValue)
                element.Add(new XAttribute("LastCheckTime", ObjectUtilities.GetStringFromDateTime(_LastCheckTime)));

            if (_MediaTime != TimeSpan.Zero)
                element.Add(new XAttribute("MediaTime", ObjectUtilities.GetStringFromTimeSpan(_MediaTime)));

            if (_MediaLength != TimeSpan.Zero)
                element.Add(new XAttribute("MediaLength", ObjectUtilities.GetStringFromTimeSpan(_MediaLength)));

            if (_Children != null)
            {
                foreach (ContentStatistics cs in _Children)
                {
                    XElement csElement = cs.GetElement("CS");
                    element.Add(csElement);
                }
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "SourceKeyType":
                    _SourceKeyType = ObjectUtilities.GetTypeFromString(attributeValue);
                    break;
                case "SourceKey":
                    if (_SourceKeyType != null)
                        _SourceKey = ObjectUtilities.GetKeyFromString(attributeValue, _SourceKeyType.Name);
                    else
                        _SourceKey = ObjectUtilities.GetKeyFromString(attributeValue, null);
                    break;
                case "SourceType":
                    _SourceType = ObjectTypes.FindType(attributeValue);
                    break;
                case "ContentType":
                    _ContentType = attributeValue;
                    break;
                case "ContentClass":
                    _ContentClass = attributeValue;
                    break;
                case "CollectDescendents":
                    _CollectDescendents = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "ToolConfigurationKey":
                    _ToolConfigurationKey = attributeValue;
                    break;
                case "ToolSource":
                    _ToolSource = ToolUtilities.GetToolSourceCodeFromString(attributeValue);
                    break;
                case "Status":
                    _Status = GetContentStatisticsStatusCodeFromString(attributeValue);
                    break;
                case "StatusOverridden":
                    _StatusOverridden = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "HideStatisticsFromParent":
                    _HideStatisticsFromParent = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "FutureCount":
                    _FutureCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "DueCount":
                    _DueCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "ActiveCount":
                    _ActiveCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "CompleteCount":
                    _CompleteCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "VisitedCount":
                    _VisitedCount = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "UpdateReviewTime":
                    _UpdateReviewTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MaxValue);
                    break;
                case "NextReviewTime":
                    _NextReviewTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MaxValue);
                    break;
                case "LastCheckTime":
                    _LastCheckTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                case "MediaTime":
                    _MediaTime = ObjectUtilities.GetTimeSpanFromString(attributeValue, TimeSpan.Zero);
                    break;
                case "MediaLength":
                    _MediaLength = ObjectUtilities.GetTimeSpanFromString(attributeValue, TimeSpan.Zero);
                    break;
                default:
                    returnValue = base.OnAttribute(attribute);
                    break;
            }

            return returnValue;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "CS":
                    {
                        ContentStatistics cs = new ContentStatistics(childElement);
                        AddChild(cs);
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static ContentStatisticsStatusCode GetContentStatisticsStatusCodeFromString(string str)
        {
            ContentStatisticsStatusCode code;

            switch (str)
            {
                case "Future":
                    code = ContentStatisticsStatusCode.Future;
                    break;
                case "Due":
                    code = ContentStatisticsStatusCode.Due;
                    break;
                case "Active":
                    code = ContentStatisticsStatusCode.Active;
                    break;
                case "Complete":
                    code = ContentStatisticsStatusCode.Complete;
                    break;
                case "Disabled":
                    code = ContentStatisticsStatusCode.Disabled;
                    break;
                default:
                    throw new ObjectException("ContentStatisticsStatus.GetContentStatisticsStatusCodeFromString:  Unknown code:  " + str);
            }

            return code;
        }

        public static string ComposeKey(
            IBaseObjectKeyed sourceObject,
            UserRecord userRecord,
            UserProfile userProfile)
        {
            string userName = (userRecord != null ? userRecord.UserName : "null");
            string profileName = (userProfile != null ? userProfile.ProfileName : "null");
            string typeAbbrev;
            string guidString = (sourceObject != null ? sourceObject.GuidString : "null");

            if (sourceObject == null)
                typeAbbrev = "null";
            else if (sourceObject is ObjectReferenceNodeTree)
                typeAbbrev = "BONT";
            else
                typeAbbrev = ObjectUtilities.GetTypeAbbreviation(sourceObject);

            string key = userName + "|"
                + profileName + "|"
                + typeAbbrev + "|"
                + guidString;

            return key;
        }

        public static string ComposeRootKey(
            UserRecord userRecord,
            UserProfile userProfile)
        {
            string userName = (userRecord != null ? userRecord.UserName : "null");
            string profileName = (userProfile != null ? userProfile.ProfileName : "null");
            string typeAbbrev;
            string guidString = Guid.Empty.ToString();

            typeAbbrev = "root";

            string key = userName + "|"
                + profileName + "|"
                + typeAbbrev + "|"
                + guidString;

            return key;
        }
    }
}

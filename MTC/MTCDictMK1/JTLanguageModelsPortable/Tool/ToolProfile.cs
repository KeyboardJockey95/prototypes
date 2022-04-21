// ToolProfile.cs - Tool profile.
// Stores a list of ToolConfiguration objects.
// Copyright (c) John Thompson, 2015.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Tool
{
    // Basic item selection algorithm.  This code determines which algorithm is used
    // for selecting items to be seen or reviewed next in a tool.
    public enum SelectorAlgorithmCode
    {
        // Items are selected sequentially in the order they appear in the study list,
        // without regard for whether they have been seen before.
        Forward,
        // Items are selected sequentially in the reverse order they appear in the study list,
        // without regard for whether they have been seen before.
        Reverse,
        // Items are selected randomly from the study list, without regard for whether they
        // have been seen before.  The IsRandomUnique option indicates that all items should
        // be seen once before seeing an item again.
        Random,
        // Implements a table-based spaced repetition algorithm, where items are reviewed on a schedule
        // of decreasing frequency. The grading buttons select an indexed table interval entry relative
        // to a stage number that determines the starting index. Buttons can also control whether the
        // stage number is incremented, kept the same, or reset to 0, based on two thresholds. 
        // New unseen items are selected either randomly or sequentially via the IsRandomNew option.
        Adaptive,
        // Selects a smaller subgroup of items to see or review, until all items can be recalled
        // from memory.  Then the next subgroup of items will be selected for review.  The groups
        // (or "chunks") can be selected sequentially or randomly via the IsRandomNew option.
        Chunky,
        // Implements a "tiered" algorithm, where knowing an item moves it to the next higher teir.
        // Items not known are kicked back to the lowest tier or level.  New unseen items are
        // selected either randomly or sequentially via the IsRandomNew option.
        Leitner,
        // Implements a "spaced-repetition" algorithm with some adaptive enhancements, where items
        // are reviewed on a schedule of decreasing frequency, and grading buttons determine whether
        // to reset, decrement, keep same, advance, or jump the interval table stage.
        // New unseen items are selected either randomly or sequentially via the IsRandomNew option.
        SpacedRepetition,
        // Implements a spaced-interval algorithm similar to the popular Anki program,
        // where items are reviewed on a schedule of decreasing frequency with a factor
        // of how well you indicated you know the items.
        // New unseen items are selected either randomly or sequentially via the IsRandomNew option.
        Spanki,
        // Let's you select the next next review time, which is obtained from the first entries
        // of the interval table up to the grade count parameter, which defaults to 15. For gradings
        // at or below the stage reset threshold, the stage is reset to 0.
        // For grades above the stage increment threshold, the stage is incremented.
        // The stage actually has no effect with this algorithm, but is affected in case you later switch
        // to an algorithm that does use the stage, for the purpose of continuity.
        // New unseen items are selected either randomly or sequentially via the IsRandomNew option.
        Manual
    }

    public class ToolProfile : BaseObjectTitled
    {
        // Local members.  See static members for default values.

        // Selector mode (Forward, Reverse, Random, RandomUnique, Adaptive, AdaptiveRandom,
        //  AdaptiveMix, AdoptiveMixRandom).
        protected SelectorAlgorithmCode _SelectorAlgorithm;
        // New item limit.  Specifies how many new items you will be shown before you get
        // a message indicating such.
        protected int _NewLimit;
        // Review item limit.  Specifies how many review items you will be shown before it
        // starts silently ignoring them, or if NewLimit is not set, a message indication such.
        protected int _ReviewLimit;
        // Limit expiration time.
        protected TimeSpan _LimitExpiration;
        // Option for selecting random algorithm items only once.
        protected bool _IsRandomUnique;
        // Option for selecting adaptive, chunky, Leitner, or spaced-repetition new items randomly
        // instead of sequentially.  If true, new items will be selected randomly.
        // If false, items will be selected sequentially in the order they appear in the study list.
        protected bool _IsRandomNew;
        // Option for selecting adaptive new items from either new or touched items, i.e. randomly
        // mixing the source of next-to-be-touched items.  If true (mixed), this helps
        // the tool not get stuck reviewing only old items.  If false, new items will only be selected
        // if no previously touched items are due for review.
        protected bool _IsAdaptiveMixNew;
        // Review level [-5 (conservative) - 10 (liberal)].  This option determines how conservatively or
        // liberally the adaptive or spaced repetition algorithms schedule items for review.  A
        // lower level means items are scheduled for review more conservatively, meaning items will be reviewed
        // sooner to insure they will be learned.  A higher level means items are scheduled for review
        // more liberally, meaning items will be review later, assuming you will remember them better.
        // A level of 0 is moderate.
        protected int _ReviewLevel;
        // Threshold for resetting interval stage.
        protected float _StageResetGradeThreshold;
        // Threshold for incrementing interval stage.
        protected float _StageIncrementGradeThreshold;
        // High grade multiplier for Adaptive.
        public float _HighGradeMultiplier;
        // Easy multiplier for Spanki.
        public float _EasyGradeMultiplier;
        // Hard multiplier for Spanki.
        public float _HardGradeMultiplier;
        // Running average sample size [1 - 5]
        // This option is the number of samples averaged to get the final grade.
        // A value of 1 means the grades aren't averaged, and the final grade is the button number.
        // A value greater than 1 causes an average of the current grade and the saved prior grades,
        // which are shifted and truncated if this size is reached.
        // If this parameter is 0, a default sample size based on the review level will be used.
        protected int _SampleSize;
        // Select size.  Specifies how many items in a select (multiple choice) configuration.
        protected int _ChoiceSize;
        // Chunk size.  Specifies how many items must be known correctly before moving to the next
        // chunk of items.
        protected int _ChunkSize;
        // History size.  Determines how many items back you can go via clicking the "back" button of a tool.
        protected int _HistorySize;
        // Show item index.  If true, the tool will display the items' index in the study list, to give
        // you an indication of your current position in the study list.
        protected bool _IsShowIndex;
        // Show item ordinal.  If true, the tool will display the items' index in the randomized or sequential
        // study list, to give you an indication of your current position in the study list, without regard to
        // the actual index of the item in the underlying study list.
        protected bool _IsShowOrdinal;
        // Table of review time offsets for the spaced-interval mode.
        protected List<TimeSpan> _SpacedIntervalTable;
        // Table of review time offsets, adjusted for the review level.
        protected List<TimeSpan> _IntervalTable;
        // Font family.
        protected string _FontFamily;
        // Flash tool font size.
        protected string _FlashFontSize;
        // List tool font size.
        protected string _ListFontSize;
        // Maximum line length.
        protected int _MaximumLineLength;
        // Number of grade levels. 1 is the lowest grade, up to this number, minimum 2, maximum of 5.
        // For Adaptive only.
        protected int _GradeCount;
        // Grade button labels.
        protected List<string> _GradeButtonLabelsSequential;
        protected List<string> _GradeButtonLabelsAdaptive;
        protected List<string> _GradeButtonLabelsChunky;
        protected List<string> _GradeButtonLabelsLeitner;
        protected List<string> _GradeButtonLabelsSpacedRepetition;
        protected List<string> _GradeButtonLabelsSpanki;
        protected List<string> _GradeButtonLabelsManual;
        // Grade button tips.
        protected List<string> _GradeButtonTipsSequential;
        protected List<string> _GradeButtonTipsAdaptive;
        protected List<string> _GradeButtonTipsChunky;
        protected List<string> _GradeButtonTipsLeitner;
        protected List<string> _GradeButtonTipsSpacedRepetition;
        protected List<string> _GradeButtonTipsSpanki;
        protected List<string> _GradeButtonTipsManual;
        // Tool configurations.
        protected List<ToolConfiguration> _ToolConfigurations;

        // Static members.

        // Name strings for selector algorithm codes.
        public static List<string> SelectorAlgorithmCodeStrings = new List<string>()
        {
            "Forward",
            "Reverse",
            "Random",
            "Adaptive",
            "Chunky",
            "Leitner",
            "Spaced Repetition",
            "Spanki",
            "Manual"
        };
        // Default grade count.
        public static int DefaultGradeCount = 5;
        // Default selector algorithm.
        public static SelectorAlgorithmCode DefaultSelectorAlgorithm = SelectorAlgorithmCode.Adaptive;
        // Default NewLimit option value.
        public static int DefaultNewLimit = 0;
        // Default ReviewLimit option value.
        public static int DefaultReviewLimit = 0;
        // Default IsRandomUnique option value.
        public static bool DefaultIsRandomUnique = true;
        // Default IsRandomNew option value.
        public static bool DefaultIsRandomNew = false;
        // Default IsAdaptiveMixNew option value.
        public static bool DefaultIsAdaptiveMixNew = false;
        // Default ReviewLevel option value.
        public static int DefaultReviewLevel = 0;
        // Default threshold for resetting interval stage.
        public static float DefaultStageResetGradeThreshold = 3.0f;
        // Default threshold for incrementing interval stage.
        public static float DefaultStageIncrementGradeThreshold = 3.5f;
        // Default high grade multiplier for Adaptive.
        public static float DefaultHighGradeMultiplier = 4.0f;
        // Default easy multiplier for Spanki.
        public static float DefaultEasyGradeMultiplier = 0.5f;
        // Default hard multiplier for Spanki.
        public static float DefaultHardGradeMultiplier = 1.5f;
        // Default SampleSize option value.
        public static int DefaultSampleSize = 1;
        // Default ChoiceSize option value.
        public static int DefaultChoiceSize = 4;
        // Default ChunkSize option value.
        public static int DefaultChunkSize = 5;
        // Default HistorySize option value.
        public static int DefaultHistorySize = 10;
        // Default IsShowIndex option value.
        public static bool DefaultIsShowIndex = true;
        // Default IsShowOrdinal option value.
        public static bool DefaultIsShowOrdinal = true;
        // Default FontFamily option value.
        public static string DefaultFontFamily = "Arial";  // Keep these in sync with UserRecord.
        // Default FlashFontSize option value.
        public static string DefaultFlashFontSize = "18";
        // Default ListFontSize option value.
        public static string DefaultListFontSize = "12";
        // Default MaximumLineLength option value.  0 means not set.
        public static int DefaultMaximumLineLength = 0;
        // Base lookup table for getting spaced repetition time interval.
#if true
        // Mine new
        public static List<TimeSpan> DefaultSpacedIntervalTable =
            new List<TimeSpan>()
            {
                new TimeSpan(0,0,1,0),
                new TimeSpan(0,0,10,0),
                new TimeSpan(0,12,0,0),
                new TimeSpan(1,12,0,0),
                new TimeSpan(3,0,0,0),
                new TimeSpan(7,0,0,0),
                new TimeSpan(14,0,0,0),
                new TimeSpan(21,0,0,0),
                new TimeSpan(30,0,0,0),
                new TimeSpan(45,0,0,0),
                new TimeSpan(60,0,0,0),
                new TimeSpan(90,0,0,0),
                new TimeSpan(120,0,0,0),
                new TimeSpan(180,0,0,0),
                new TimeSpan(365,0,0,0),
                new TimeSpan(730,0,0,0)
            };
#elif true
        // Mine
        public static List<TimeSpan> DefaultSpacedIntervalTable =
            new List<TimeSpan>()
            {
                new TimeSpan(0, 0, 0, 15),
                new TimeSpan(0, 0, 2, 0),
                new TimeSpan(0, 0, 10, 0),
                new TimeSpan(0, 1, 0, 0),
                new TimeSpan(1, 0, 0, 0),
                new TimeSpan(2, 0, 0, 0),
                new TimeSpan(4, 0, 0, 0),
                new TimeSpan(10, 0, 0, 0),
                new TimeSpan(30, 0, 0, 0),
                new TimeSpan(75, 0, 0, 0),
                new TimeSpan(180, 0, 0, 0),
                new TimeSpan(365, 0, 0, 0),
            };
#else
        // Pimsleur
        public static List<TimeSpan> DefaultSpacedIntervalTable =
            new List<TimeSpan>()
            {
                new TimeSpan(0, 0, 0, 5),
                new TimeSpan(0, 0, 0, 25),
                new TimeSpan(0, 0, 2, 0),
                new TimeSpan(0, 0, 10, 0),
                new TimeSpan(0, 1, 0, 0),
                new TimeSpan(0, 5, 0, 0),
                new TimeSpan(1, 0, 0, 0),
                new TimeSpan(5, 0, 0, 0),
                new TimeSpan(25, 0, 0, 0),
                new TimeSpan(120, 0, 0, 0),
                new TimeSpan(730, 0, 0, 0)
            };
#endif

        public static int ReviewLevelMinimum = -5;
        public static int ReviewLevelMaximum = 10;

        public static double[] LevelToFactorMap =
        {
            .25,    // -5
            .35,    // -4
            .50,    // -3
            .70,    // -2
            .85,    // -1
            1.0,    // 0
            1.2,    // 1
            1.5,    // 2
            2.0,    // 3
            3.5,    // 4
            4.0,    // 5
            5.0,    // 6
            10.0,   // 7
            20.0,   // 8
            30.0,   // 9
            50.0    // 10
        };

        // Grade button labels.
        public static List<string> _DefaultGradeButtonLabelsSequential =
            new List<string>(1) { "Next" };
        public static List<string> _DefaultGradeButtonLabelsAdaptive =
            new List<string>(5) { "1", "2", "3", "4", "5" };
        public static List<string> _DefaultGradeButtonLabelsChunky =
            new List<string>(2) { "No", "Yes" };
        public static List<string> _DefaultGradeButtonLabelsLeitner =
            new List<string>(2) { "No", "Yes" };
        public static List<string> _DefaultGradeButtonLabelsSpacedRepetition =
            new List<string>(5) { "Restart", "Back", "Repeat", "Next", "Jump" };
        public static List<string> _DefaultGradeButtonLabelsSpanki =
            new List<string>(4) { "Restart", "Hard", "Good", "Easy" };
        // Default Manual labels are the grade numbers.
        // Grade button tips.
        public static List<string> _DefaultGradeButtonTipsSequential =
            new List<string>(1) { "Go to next item" };
        private static List<string> _DefaultGradeButtonTipsAdaptive2 =
            new List<string>(2) { "Start over", "Good" };
        private static List<string> _DefaultGradeButtonTipsAdaptive3 =
            new List<string>(3) { "Start over", "Almost", "Good" };
        private static List<string> _DefaultGradeButtonTipsAdaptive4 =
            new List<string>(4) { "Start over", "Almost", "Good", "Know it" };
        private static List<string> _DefaultGradeButtonTipsAdaptive5 =
            new List<string>(5) { "Don't know it", "Almost", "Good", "Know it", "Really Know it" };
        public static List<string> _DefaultGradeButtonTipsChunky =
            new List<string>(2) { "Don't know it", "Know it" };
        public static List<string> _DefaultGradeButtonTipsLeitner =
            new List<string>(2) { "Don't know it", "Know it" };
        public static List<string> _DefaultGradeButtonTipsSpacedRepetition =
            new List<string>(5) { "Don't know it", "Back up a little", "One more time", "Good", "Very good" };
        public static List<string> _DefaultGradeButtonTipsSpanki =
            new List<string>(4) { "Don't know it", "Had to think", "Know it", "Too easy" };
        // Default Manual tips are the review time offset.

        public ToolProfile(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                int gradeCount, SelectorAlgorithmCode selectorAlgorithm, int newLimit, int reviewLimit,
                bool isRandomUnique, bool isRandomNew, bool isAdaptiveMixNew,
                int reviewLevel, int choiceSize, int chunkSize, int historySize, bool isShowIndex, bool isShowOrdinal,
                List<TimeSpan> intervalTable,
                string fontFamily, string flashFontSize, string listFontSize, int maximumLineLength,
                List<ToolConfiguration> toolConfigurations)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic, targetLanguageIDs, hostLanguageIDs, owner)
        {
            ClearToolProfile();
            _GradeCount = gradeCount;
            _SelectorAlgorithm = selectorAlgorithm;
            _NewLimit = newLimit;
            _ReviewLimit = reviewLimit;
            _LimitExpiration = TimeSpan.Zero;
            _IsRandomUnique = isRandomUnique;
            _IsRandomNew = isRandomNew;
            _IsAdaptiveMixNew = isAdaptiveMixNew;
            _ReviewLevel = reviewLevel;
            _StageResetGradeThreshold = DefaultStageResetGradeThreshold;
            _StageIncrementGradeThreshold = DefaultStageIncrementGradeThreshold;
            _HighGradeMultiplier = DefaultHighGradeMultiplier;
            _EasyGradeMultiplier = DefaultEasyGradeMultiplier;
            _HardGradeMultiplier = DefaultHardGradeMultiplier;
            _SampleSize = DefaultSampleSize;
            _ChoiceSize = choiceSize;
            _ChunkSize = chunkSize;
            _HistorySize = historySize;
            _IsShowIndex = isShowIndex;
            _IsShowOrdinal = isShowOrdinal;
            _SpacedIntervalTable = intervalTable;
            _FontFamily = fontFamily;
            _FlashFontSize = flashFontSize;
            _ListFontSize = listFontSize;
            _MaximumLineLength = maximumLineLength;
            _ToolConfigurations = toolConfigurations;

            ComputeTable();
        }

        public ToolProfile(object key)
            : base(key)
        {
            ClearToolProfile();
        }

        public ToolProfile(ToolProfile other, object key)
            : base(other, key)
        {
            Copy(other);
            Modified = false;
        }

        public ToolProfile(ToolProfile other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ToolProfile(XElement element)
        {
            OnElement(element);
        }

        public ToolProfile()
        {
            ClearToolProfile();
        }

        public void Copy(ToolProfile other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearToolProfile();
                return;
            }

            _GradeCount = other._GradeCount;
            _SelectorAlgorithm = other.SelectorAlgorithm;
            _NewLimit = other.NewLimit;
            _ReviewLimit = other.ReviewLimit;
            _LimitExpiration = other.LimitExpiration;
            _IsRandomUnique = other.IsRandomUnique;
            _IsRandomNew = other.IsRandomNew;
            _IsAdaptiveMixNew = other.IsAdaptiveMixNew;
            _ReviewLevel = other.ReviewLevel;
            _StageResetGradeThreshold = other.StageResetGradeThreshold;
            _StageIncrementGradeThreshold = other.StageIncrementGradeThreshold;
            _HighGradeMultiplier = other.HighGradeMultiplier;
            _EasyGradeMultiplier = other.EasyGradeMultiplier;
            _HardGradeMultiplier = other.HardGradeMultiplier;
            _SampleSize = other.SampleSize;
            _ChoiceSize = other.ChoiceSize;
            _ChunkSize = other.ChunkSize;
            _HistorySize = other.HistorySize;
            _IsShowIndex = other.IsShowIndex;
            _IsShowOrdinal = other.IsShowOrdinal;
            if (other._SpacedIntervalTable != null)
                _SpacedIntervalTable = new List<TimeSpan>(other._SpacedIntervalTable);
            else
                _SpacedIntervalTable = null;
            if (other._IntervalTable != null)
                _IntervalTable = new List<TimeSpan>(other._IntervalTable);
            else
                _IntervalTable = null;
            _FontFamily = other.FontFamily;
            _FlashFontSize = other.FlashFontSize;
            _ListFontSize = other.ListFontSize;
            _MaximumLineLength = other.MaximumLineLength;

            _GradeButtonLabelsSequential = ObjectUtilities.CloneStringList(other._GradeButtonLabelsSequential);
            _GradeButtonLabelsAdaptive = ObjectUtilities.CloneStringList(other._GradeButtonLabelsAdaptive);
            _GradeButtonLabelsChunky = ObjectUtilities.CloneStringList(other._GradeButtonLabelsChunky);
            _GradeButtonLabelsLeitner = ObjectUtilities.CloneStringList(other._GradeButtonLabelsLeitner);
            _GradeButtonLabelsSpacedRepetition = ObjectUtilities.CloneStringList(other._GradeButtonLabelsSpacedRepetition);
            _GradeButtonLabelsSpanki = ObjectUtilities.CloneStringList(other._GradeButtonLabelsSpanki);
            _GradeButtonTipsSequential = ObjectUtilities.CloneStringList(other._GradeButtonTipsSequential);
            _GradeButtonTipsAdaptive = ObjectUtilities.CloneStringList(other._GradeButtonTipsAdaptive);
            _GradeButtonTipsChunky = ObjectUtilities.CloneStringList(other._GradeButtonTipsChunky);
            _GradeButtonTipsLeitner = ObjectUtilities.CloneStringList(other._GradeButtonTipsLeitner);
            _GradeButtonTipsSpacedRepetition = ObjectUtilities.CloneStringList(other._GradeButtonTipsSpacedRepetition);
            _GradeButtonTipsSpanki = ObjectUtilities.CloneStringList(other._GradeButtonTipsSpanki);

            _ToolConfigurations = other.CloneToolConfigurations();

            ModifiedFlag = true;
        }

        public void CopyDeep(ToolProfile other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolProfile();
        }

        public void ClearToolProfile()
        {
            _GradeCount = DefaultGradeCount;
            _SelectorAlgorithm = DefaultSelectorAlgorithm;
            _NewLimit = 0;
            _ReviewLimit = 0;
            _LimitExpiration = TimeSpan.Zero;
            _IsRandomUnique = DefaultIsRandomUnique;
            _IsRandomNew = DefaultIsRandomNew;
            _IsAdaptiveMixNew = DefaultIsAdaptiveMixNew;
            _ReviewLevel = DefaultReviewLevel;
            _StageResetGradeThreshold = DefaultStageResetGradeThreshold;
            _StageIncrementGradeThreshold = DefaultStageIncrementGradeThreshold;
            _HighGradeMultiplier = DefaultHighGradeMultiplier;
            _EasyGradeMultiplier = DefaultEasyGradeMultiplier;
            _HardGradeMultiplier = DefaultHardGradeMultiplier;
            _SampleSize = DefaultSampleSize;
            _ChoiceSize = DefaultChoiceSize;
            _ChunkSize = DefaultChunkSize;
            _HistorySize = DefaultHistorySize;
            _IsShowIndex = DefaultIsShowIndex;
            _IsShowOrdinal = DefaultIsShowOrdinal;
            _SpacedIntervalTable = null;
            _FontFamily = null;
            _FlashFontSize = null;
            _ListFontSize = null;
            _MaximumLineLength = 0;
            _GradeButtonLabelsSequential = null;
            _GradeButtonLabelsAdaptive = null;
            _GradeButtonLabelsChunky = null;
            _GradeButtonLabelsLeitner = null;
            _GradeButtonLabelsSpacedRepetition = null;
            _GradeButtonLabelsSpanki = null;
            _GradeButtonTipsSequential = null;
            _GradeButtonTipsAdaptive = null;
            _GradeButtonTipsChunky = null;
            _GradeButtonTipsLeitner = null;
            _GradeButtonTipsSpacedRepetition = null;
            _GradeButtonTipsSpanki = null;
            _ToolConfigurations = new List<ToolConfiguration>();
        }

        public override IBaseObject Clone()
        {
            return new ToolProfile(this);
        }

        public override string Name
        {
            get
            {
                return ToolUtilities.GetToolProfileName(KeyString);
            }
        }

        public SelectorAlgorithmCode SelectorAlgorithm
        {
            get
            {
                return _SelectorAlgorithm;
            }
            set
            {
                if (value != _SelectorAlgorithm)
                {
                    if ((_SelectorAlgorithm == SelectorAlgorithmCode.Manual) &&
                            (value != SelectorAlgorithmCode.Manual))
                        _GradeCount = DefaultGradeCount;
                    else if ((_SelectorAlgorithm != SelectorAlgorithmCode.Manual) &&
                            (value == SelectorAlgorithmCode.Manual))
                        _GradeCount = 15;
                    _SelectorAlgorithm = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SelectorAlgorithmString
        {
            get
            {
                return GetSelectorStringFromCodeString(_SelectorAlgorithm.ToString());
            }
        }

        public bool IsIntervalBased()
        {
            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Adaptive:
                case SelectorAlgorithmCode.SpacedRepetition:
                case SelectorAlgorithmCode.Spanki:
                case SelectorAlgorithmCode.Manual:
                    return true;
                default:
                    break;
            }

            return false;
        }

        public bool IsStageBased()
        {
            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Adaptive:
                case SelectorAlgorithmCode.Leitner:
                case SelectorAlgorithmCode.SpacedRepetition:
                case SelectorAlgorithmCode.Spanki:
                case SelectorAlgorithmCode.Manual:
                    return true;
                default:
                    break;
            }

            return false;
        }

        public int NewLimit
        {
            get
            {
                return _NewLimit;
            }
            set
            {
                if (value != _NewLimit)
                {
                    _NewLimit = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ReviewLimit
        {
            get
            {
                return _ReviewLimit;
            }
            set
            {
                if (value != _ReviewLimit)
                {
                    _ReviewLimit = value;
                    ModifiedFlag = true;
                }
            }
        }

        public TimeSpan LimitExpiration
        {
            get
            {
                return _LimitExpiration;
            }
            set
            {
                if (value != _LimitExpiration)
                {
                    _LimitExpiration = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsRandomUnique
        {
            get
            {
                return _IsRandomUnique;
            }
            set
            {
                if (value != _IsRandomUnique)
                {
                    _IsRandomUnique = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsRandomNew
        {
            get
            {
                return _IsRandomNew;
            }
            set
            {
                if (value != _IsRandomNew)
                {
                    _IsRandomNew = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsAdaptiveMixNew
        {
            get
            {
                return _IsAdaptiveMixNew;
            }
            set
            {
                if (value != _IsAdaptiveMixNew)
                {
                    _IsAdaptiveMixNew = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ReviewLevel
        {
            get
            {
                return _ReviewLevel;
            }
            set
            {
                if (value != _ReviewLevel)
                {
                    _ReviewLevel = value;
                    ModifiedFlag = true;
                }
            }
        }

        public float StageResetGradeThreshold
        {
            get
            {
                return _StageResetGradeThreshold;
            }
            set
            {
                if (value != _StageResetGradeThreshold)
                {
                    _StageResetGradeThreshold = value;
                    ModifiedFlag = true;
                }
            }
        }

        public float StageIncrementGradeThreshold
        {
            get
            {
                return _StageIncrementGradeThreshold;
            }
            set
            {
                if (value != _StageIncrementGradeThreshold)
                {
                    _StageIncrementGradeThreshold = value;
                    ModifiedFlag = true;
                }
            }
        }

        public float HighGradeMultiplier
        {
            get
            {
                return _HighGradeMultiplier;
            }
            set
            {
                if (value != _HighGradeMultiplier)
                {
                    _HighGradeMultiplier = value;
                    ModifiedFlag = true;
                }
            }
        }

        public float EasyGradeMultiplier
        {
            get
            {
                return _EasyGradeMultiplier;
            }
            set
            {
                if (value != _EasyGradeMultiplier)
                {
                    _EasyGradeMultiplier = value;
                    ModifiedFlag = true;
                }
            }
        }

        public float HardGradeMultiplier
        {
            get
            {
                return _HardGradeMultiplier;
            }
            set
            {
                if (value != _HardGradeMultiplier)
                {
                    _HardGradeMultiplier = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int SampleSize
        {
            get
            {
                return _SampleSize;
            }
            set
            {
                if (value != _SampleSize)
                {
                    _SampleSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ChoiceSize
        {
            get
            {
                return _ChoiceSize;
            }
            set
            {
                if (value != _ChoiceSize)
                {
                    _ChoiceSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ChunkSize
        {
            get
            {
                return _ChunkSize;
            }
            set
            {
                if (value != _ChunkSize)
                {
                    _ChunkSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int HistorySize
        {
            get
            {
                return _HistorySize;
            }
            set
            {
                if (value != _HistorySize)
                {
                    _HistorySize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsShowIndex
        {
            get
            {
                return _IsShowIndex;
            }
            set
            {
                if (value != _IsShowIndex)
                {
                    _IsShowIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsShowOrdinal
        {
            get
            {
                return _IsShowOrdinal;
            }
            set
            {
                if (value != _IsShowOrdinal)
                {
                    _IsShowOrdinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<TimeSpan> SpacedIntervalTable
        {
            get
            {
                if ((_SpacedIntervalTable != null) && (_SpacedIntervalTable.Count() != 0))
                    return _SpacedIntervalTable;
                return new List<TimeSpan>(DefaultSpacedIntervalTable);
            }
            set
            {
                if (value == null)
                {
                    _SpacedIntervalTable = null;
                    ModifiedFlag = true;
                }
                else if ((_SpacedIntervalTable != null) || ((_SpacedIntervalTable != value) && (ObjectUtilities.CompareTimeSpanLists(_SpacedIntervalTable, value) != 0)))
                {
                    if (ObjectUtilities.CompareTimeSpanLists(value, DefaultSpacedIntervalTable) == 0)
                        _SpacedIntervalTable = null;
                    else
                        _SpacedIntervalTable = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<TimeSpan> IntervalTable
        {
            get
            {
                return _IntervalTable;
            }
            set
            {
                if (value != _IntervalTable)
                {
                    _IntervalTable = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void ComputeTable()
        {
            List<TimeSpan> source;
            int count;

            source = SpacedIntervalTable;
            count = source.Count();

            if (_IntervalTable == null)
                _IntervalTable = new List<TimeSpan>(count);
            else
                _IntervalTable.Clear();

            double factor = GetIntervalTimeFactor(_ReviewLevel);

            for (int index = 0; index < count; index++)
            {
                TimeSpan ts = new TimeSpan((long)(source[index].Ticks * factor));
                _IntervalTable.Add(ts);
            }
        }

        // Get adaptive/spaced interval time interval factor.
        public static double GetIntervalTimeFactor(int x)
        {
            if (x < ReviewLevelMinimum)
                x = ReviewLevelMinimum;
            else if (x > 10)
                x = ReviewLevelMaximum;
            return LevelToFactorMap[x - ReviewLevelMinimum];
        }

        public TimeSpan GetTimeOffset(
            float currentGrade,
            float lastGrade,
            float averageGrade,
            int stage)
        {
            List<TimeSpan> table = IntervalTable;
            TimeSpan timeOffset;
            int baseIndex = 0;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                case SelectorAlgorithmCode.Chunky:
                case SelectorAlgorithmCode.Leitner:
                default:
                    timeOffset = table[baseIndex];
                    break;
                case SelectorAlgorithmCode.Adaptive:
                    baseIndex = stage;
#if true
                    if (baseIndex < 0)
                        baseIndex = 0;
                    else if (baseIndex >= table.Count())
                        baseIndex = table.Count() - 1;
                    if (averageGrade < 1.0f)
                        timeOffset = table[0];
                    else if (averageGrade <= _StageResetGradeThreshold)
                    {
                        if (lastGrade != 0.0f)
                        {
                            double x2 = averageGrade;
                            int index0 = (int)x2;
                            int index1 = (int)(x2 + 1.0);
                            double x0 = (double)index0;
                            double x1 = (double)(index1);

                            if (index0 < 1)
                                timeOffset = table[baseIndex];
                            else if (index1 > GradeCount)
                                timeOffset = table[baseIndex + (GradeCount - 1)];
                            else if (x0 == x1)
                                timeOffset = table[baseIndex + (index0 - 1)];
                            else
                            {
                                double y0 = table[baseIndex + (index0 - 1)].Ticks;
                                double y1 = table[baseIndex + (index1 - 1)].Ticks;
                                double y2 = y0 * (x2 - x1) / (x0 - x1) + y1 * (x2 - x0) / (x1 - x0);
                                timeOffset = new TimeSpan((long)y2);
                            }
                        }
                        else
                            timeOffset = table[baseIndex + ((int)currentGrade - 1)];
                    }
                    else
                    {
                        baseIndex += (int)_StageResetGradeThreshold;
                        if (baseIndex < 0)
                            baseIndex = 0;
                        timeOffset = table[baseIndex];
                    }
                    if ((_HighGradeMultiplier > 1.0f) && ((int)currentGrade == _GradeCount))
                    {
                        long ticks = timeOffset.Ticks;
                        ticks = (long)(_HighGradeMultiplier * ticks);
                        timeOffset = new TimeSpan(ticks);
                    }
#else
                    if (baseIndex < 0)
                        baseIndex = 0;
                    else if (baseIndex >= (table.Count() - GradeCount))
                        baseIndex = table.Count() - GradeCount;
                    if (lastGrade != 0.0f)
                    {
                        double x2 = averageGrade;
                        int index0 = (int)x2;
                        int index1 = (int)(x2 + 1.0);
                        double x0 = (double)index0;
                        double x1 = (double)(index1);

                        if (index0 < 1)
                            timeOffset = table[baseIndex];
                        else if (index1 > GradeCount)
                            timeOffset = table[baseIndex + (GradeCount - 1)];
                        else if (x0 == x1)
                            timeOffset = table[baseIndex + (index0 - 1)];
                        else
                        {
                            double y0 = table[baseIndex + (index0 - 1)].Ticks;
                            double y1 = table[baseIndex + (index1 - 1)].Ticks;
                            double y2 = y0 * (x2 - x1) / (x0 - x1) + y1 * (x2 - x0) / (x1 - x0);
                            timeOffset = new TimeSpan((long)y2);
                        }
                    }
                    else if ((currentGrade >= 1.0f) && (currentGrade <= 5.0f))
                        timeOffset = table[baseIndex + ((int)currentGrade - 1)];
                    else
                        timeOffset = table[baseIndex];
#endif
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    baseIndex = stage;
                    if (baseIndex < 0)
                        baseIndex = 0;
                    else if (baseIndex >= (table.Count() - GradeCount))
                        baseIndex = table.Count() - GradeCount;
                    timeOffset = table[baseIndex];
                    break;
                case SelectorAlgorithmCode.Spanki:
                    {
                        float timeFactor;
                        baseIndex = stage;
                        if (baseIndex < 0)
                            baseIndex = 0;
                        else if (baseIndex >= (table.Count() - GradeCount))
                            baseIndex = table.Count() - GradeCount;
                        if (currentGrade < 1.5f)
                        {
                            if (stage == 0)
                                timeOffset = table[0];
                            else
                                timeOffset = table[1];
                        }
                        else if (currentGrade < 2.5f)
                        {
                            timeFactor = _EasyGradeMultiplier;
                            timeOffset = table[baseIndex];
                            long ticks = timeOffset.Ticks;
                            ticks = (long)(timeFactor * ticks);
                            timeOffset = new TimeSpan(ticks);
                        }
                        else if (currentGrade < 3.5f)
                            timeOffset = table[baseIndex];
                        else
                        {
                            timeFactor = _HardGradeMultiplier;
                            timeOffset = table[baseIndex];
                            long ticks = timeOffset.Ticks;
                            ticks = (long)(timeFactor * ticks);
                            timeOffset = new TimeSpan(ticks);
                        }
                    }
                    break;
                case SelectorAlgorithmCode.Manual:
                    baseIndex = (int)currentGrade - 1;
                    if (baseIndex < 0)
                        baseIndex = 0;
                    else if (baseIndex >= table.Count())
                        baseIndex = table.Count() - 1;
                    timeOffset = table[baseIndex];
                    break;
            }

            return timeOffset;
        }

        // Increment touch times and grade in flash cards.
        public void TouchApplyGrade(
            ToolItemStatus toolItemStatus,
            float grade,
            DateTime nowTime,
            ToolConfiguration toolConfiguration)
        {
            float lastGrade = toolItemStatus.LastGrade;
            int stage = toolItemStatus.Stage;

            toolItemStatus.ApplyGrade(grade, _SampleSize);

            float averageGrade = toolItemStatus.Grade;
            TimeSpan timeOffset;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                case SelectorAlgorithmCode.Chunky:
                    timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    break;
                case SelectorAlgorithmCode.Leitner:
                    if (averageGrade < 1.5f)
                    {
                        stage = 0;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    else
                    {
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                        stage++;
                    }
                    break;
                case SelectorAlgorithmCode.Adaptive:
                    if (averageGrade <= _StageResetGradeThreshold)
                        stage = 0;
                    timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    if (averageGrade >= _StageIncrementGradeThreshold)
                        stage++;
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    if (averageGrade < 1.5f)
                    {
                        stage = 0;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    else if (averageGrade < 2.5f)
                    {
                        if (stage >= 1)
                            stage--;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    else if (averageGrade < 3.5f)
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    else if (averageGrade < 4.5f)
                    {
                        if (stage < (IntervalTable.Count() - GradeCount))
                            stage++;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    else
                    {
                        if (stage < (IntervalTable.Count() - (GradeCount + 1)))
                            stage += 2;
                        else if (stage < (IntervalTable.Count() - GradeCount))
                            stage++;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    break;
                case SelectorAlgorithmCode.Spanki:
                    if (averageGrade < 1.5f)
                    {
                        stage = 0;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    else
                    {
                        if (stage < (IntervalTable.Count() - GradeCount))
                            stage++;
                        timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    }
                    break;
                case SelectorAlgorithmCode.Manual:
                    if (averageGrade <= _StageResetGradeThreshold)
                        stage = 0;
                    timeOffset = GetTimeOffset(grade, lastGrade, averageGrade, stage);
                    if (averageGrade >= _StageIncrementGradeThreshold)
                        stage = (int)(grade - _StageIncrementGradeThreshold) + 1;
                    break;
                default:
                    throw new ObjectException("ToolProfile.TouchApplyGrade:  Unknown selector mode:  "
                        + _SelectorAlgorithm.ToString());
            }

            DateTime nextTime = nowTime + timeOffset;
            toolItemStatus.Touch(ToolItemStatusCode.Active, nowTime, nextTime, stage);
        }

        // Rescore last touch and update touch time.
        public void RetouchApplyGrade(
            ToolItemStatus toolItemStatus,
            float grade,
            DateTime nowTime,
            ToolConfiguration toolConfiguration)
        {
            int stageDelta = 0;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                case SelectorAlgorithmCode.Chunky:
                    break;
                case SelectorAlgorithmCode.Leitner:
                    if (grade >= 1.5f)
                        stageDelta--;
                    else
                        stageDelta = -toolItemStatus.Stage;
                    break;
                case SelectorAlgorithmCode.Adaptive:
                    if (grade >= _StageIncrementGradeThreshold)
                        stageDelta--;
                    else if (grade <= _StageResetGradeThreshold)
                        stageDelta = -toolItemStatus.Stage;
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    if (grade < 1.5f)
                        stageDelta = -toolItemStatus.Stage;
                    else if (grade < 2.5f)
                        stageDelta = 1;
                    else if (grade < 3.5f)
                        stageDelta = 0;
                    else if (grade < 4.5f)
                        stageDelta = -1;
                    else
                        stageDelta = -2;
                    break;
                case SelectorAlgorithmCode.Spanki:
                    if (grade < 1.5f)
                        stageDelta = -toolItemStatus.Stage;
                    else
                        stageDelta = -1;
                    break;
                case SelectorAlgorithmCode.Manual:
                    {
                        int stage = (int)(grade - _StageIncrementGradeThreshold) + 1;
                        if (grade >= _StageIncrementGradeThreshold)
                            stageDelta = stage - toolItemStatus.Stage;
                        else if (grade <= _StageResetGradeThreshold)
                            stageDelta = -toolItemStatus.Stage;
                    }
                    break;
                default:
                    throw new ObjectException("ToolProfile.TouchApplyGrade:  Unknown selector mode:  "
                        + _SelectorAlgorithm.ToString());
            }

            toolItemStatus.PrepareForRetouch(stageDelta);
            TouchApplyGrade(toolItemStatus, grade, nowTime, toolConfiguration);
        }

        // Font family.
        public string FontFamily
        {
            get
            {
                return _FontFamily;
            }
            set
            {
                if (value != _FontFamily)
                {
                    _FontFamily = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Flash font size (points).
        public string FlashFontSize
        {
            get
            {
                return _FlashFontSize;
            }
            set
            {
                if (value != _FlashFontSize)
                {
                    _FlashFontSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        // List font size (points).
        public string ListFontSize
        {
            get
            {
                return _ListFontSize;
            }
            set
            {
                if (value != _ListFontSize)
                {
                    _ListFontSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int MaximumLineLength
        {
            get
            {
                return _MaximumLineLength;
            }
            set
            {
                if (value != _MaximumLineLength)
                {
                    _MaximumLineLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<ToolConfiguration> ToolConfigurations
        {
            get
            {
                return _ToolConfigurations;
            }
            set
            {
                if (_ToolConfigurations != value)
                    ModifiedFlag = true;

                _ToolConfigurations = value;
            }
        }

        public List<ToolConfiguration> LookupToolConfiguration(Matcher matcher)
        {
            if (_ToolConfigurations == null)
                return new List<ToolConfiguration>();

            IEnumerable<ToolConfiguration> lookupQuery =
                from configuration in _ToolConfigurations
                where (matcher.Match(configuration))
                select configuration;

            return lookupQuery.ToList();
        }

        public ToolConfiguration FindToolConfiguration(string text, LanguageID languageID)
        {
            if (_ToolConfigurations == null)
                return null;

            foreach (ToolConfiguration configuration in _ToolConfigurations)
            {
                if (configuration.GetTitleString(languageID) == text)
                    return configuration;
            }

            return null;
        }

        public ToolConfiguration GetToolConfiguration(object key)
        {
            if ((_ToolConfigurations != null) && (key != null))
                return _ToolConfigurations.FirstOrDefault(x => x.MatchKey(key));
            return null;
        }

        public ToolConfiguration GetToolConfigurationIndexed(int index)
        {
            if ((_ToolConfigurations != null) && (index >= 0) && (index < _ToolConfigurations.Count()))
                return _ToolConfigurations[index];
            return null;
        }

        public ToolConfiguration GetToolConfigurationFuzzy(string keyOrTitleStart, LanguageID uiLanguageID)
        {
            if ((_ToolConfigurations != null) && (keyOrTitleStart != null))
                return _ToolConfigurations.FirstOrDefault(
                    x =>
                        x.KeyString.ToLower().StartsWith(keyOrTitleStart.ToLower()) ||
                        x.GetTitleString(uiLanguageID).ToLower().StartsWith(keyOrTitleStart.ToLower()));
            return null;
        }

        public List<ToolConfiguration> CloneToolConfigurations()
        {
            if (_ToolConfigurations == null)
                return null;

            List<ToolConfiguration> returnValue = new List<ToolConfiguration>(_ToolConfigurations.Count());

            foreach (ToolConfiguration configuration in _ToolConfigurations)
                returnValue.Add(new ToolConfiguration(configuration));

            return returnValue;
        }

        public bool AddToolConfiguration(ToolConfiguration configuration)
        {
            if (_ToolConfigurations == null)
                _ToolConfigurations = new List<ToolConfiguration>(1) { configuration };
            else
                _ToolConfigurations.Add(configuration);

            configuration.Profile = this;

            ModifiedFlag = true;

            return true;
        }

        public bool AddToolConfigurations(List<ToolConfiguration> toolConfigurations)
        {
            foreach (ToolConfiguration configuration in toolConfigurations)
                configuration.Profile = this;

            if (_ToolConfigurations == null)
                _ToolConfigurations = new List<ToolConfiguration>(toolConfigurations);
            else
                _ToolConfigurations.AddRange(toolConfigurations);

            ModifiedFlag = true;

            return true;
        }

        public bool InsertToolConfigurationIndexed(int index, ToolConfiguration configuration)
        {
            if (_ToolConfigurations == null)
                _ToolConfigurations = new List<ToolConfiguration>(1) { configuration };
            else if ((index >= 0) && (index <= _ToolConfigurations.Count()))
                _ToolConfigurations.Insert(index, configuration);
            else
                return false;

            configuration.Profile = this;

            ModifiedFlag = true;

            return true;

        }

        public bool InsertToolConfigurationsIndexed(int index, List<ToolConfiguration> toolConfigurations)
        {
            foreach (ToolConfiguration configuration in toolConfigurations)
                configuration.Profile = this;

            if (_ToolConfigurations == null)
                _ToolConfigurations = new List<ToolConfiguration>(toolConfigurations);
            else if ((index >= 0) && (index <= _ToolConfigurations.Count()))
                _ToolConfigurations.InsertRange(index, toolConfigurations);
            else
                return false;

            ModifiedFlag = true;

            return true;

        }

        public bool DeleteToolConfiguration(ToolConfiguration configuration)
        {
            if (_ToolConfigurations != null)
            {
                if (_ToolConfigurations.Remove(configuration))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteToolConfigurationIndexed(int index)
        {
            if ((_ToolConfigurations != null) && (index >= 0) && (index < _ToolConfigurations.Count()))
            {
                _ToolConfigurations.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllToolConfigurations()
        {
            if (_ToolConfigurations != null)
                ModifiedFlag = true;
            _ToolConfigurations = null;
        }

        public int ToolConfigurationCount()
        {
            if (_ToolConfigurations != null)
                return (_ToolConfigurations.Count());

            return 0;
        }

        public int ToolConfigurationLabelCount(string label)
        {
            if (_ToolConfigurations != null)
                return (_ToolConfigurations.Count(x => x.Label == label));

            return 0;
        }

        public void RekeyToolConfigurations()
        {
            if (_ToolConfigurations == null)
                return;

            int index = 0;

            foreach (ToolConfiguration configuration in _ToolConfigurations)
            {
                object key = "I" + index.ToString();
                configuration.Rekey(key);
                index++;
            }

            if (_ToolConfigurations.Count() != 0)
                ModifiedFlag = true;
        }

        public void RekeyToolConfigurations(int startIndex)
        {
            if (_ToolConfigurations == null)
                return;

            int count = _ToolConfigurations.Count();
            int index = startIndex;

            for (; index < count; index++)
            {
                ToolConfiguration configuration = _ToolConfigurations[index];
                object key = "I" + index.ToString();
                configuration.Rekey(key);
            }

            if (startIndex < _ToolConfigurations.Count())
                ModifiedFlag = true;
        }

        public int GradeCount
        {
            get
            {
                int count;

                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        count = 1;
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        count = _GradeCount;
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        count = 2;
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        count = 2;
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        count = 5;
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        count = 4;
                        break;
                    case SelectorAlgorithmCode.Manual:
                        count = _GradeCount;
                        break;
                }

                return count;
            }
            set
            {
                if (value != _GradeCount)
                {
                    _GradeCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeCountStrings()
        {
            int first = (_SelectorAlgorithm == SelectorAlgorithmCode.Manual ? 1 : 2);
            int last = (_SelectorAlgorithm == SelectorAlgorithmCode.Manual ? 15 : 5);
            List<string> list = new List<string>();
            for (int i = first; i <= last; i++)
                list.Add(i.ToString());
            return list;
        }

        public List<string> GradeButtonLabelsSequential
        {
            get
            {
                return _GradeButtonLabelsSequential;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSequential) != 0)
                {
                    _GradeButtonLabelsSequential = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsAdaptive
        {
            get
            {
                return _GradeButtonLabelsAdaptive;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsAdaptive) != 0)
                {
                    _GradeButtonLabelsAdaptive = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsChunky
        {
            get
            {
                return _GradeButtonLabelsChunky;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsChunky) != 0)
                {
                    _GradeButtonLabelsChunky = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsLeitner
        {
            get
            {
                return _GradeButtonLabelsLeitner;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsLeitner) != 0)
                {
                    _GradeButtonLabelsLeitner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsSpacedRepetition
        {
            get
            {
                return _GradeButtonLabelsSpacedRepetition;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSpacedRepetition) != 0)
                {
                    _GradeButtonLabelsSpacedRepetition = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsSpanki
        {
            get
            {
                return _GradeButtonLabelsSpanki;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSpanki) != 0)
                {
                    _GradeButtonLabelsSpanki = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonLabelsManual
        {
            get
            {
                List<string> list = new List<string>(_GradeCount);
                for (int i = 1; i <= _GradeCount; i++)
                    list.Add(i.ToString());
                return list;
            }
        }

        public List<string> GetGradeButtonLabelsTranslated(LanguageUtilities languageUtilities)
        {
            List<string> table;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                    if (_GradeButtonLabelsSequential != null)
                        table = _GradeButtonLabelsSequential;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedSequential(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Adaptive:
                default:
                    if (_GradeButtonLabelsAdaptive != null)
                        table = _GradeButtonLabelsAdaptive;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedAdaptive(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Chunky:
                    if (_GradeButtonLabelsChunky != null)
                        table = _GradeButtonLabelsChunky;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedChunky(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Leitner:
                    if (_GradeButtonLabelsLeitner != null)
                        table = _GradeButtonLabelsLeitner;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedLeitner(languageUtilities);
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    if (_GradeButtonLabelsSpacedRepetition != null)
                        table = _GradeButtonLabelsSpacedRepetition;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedSpacedRepetition(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Spanki:
                    if (_GradeButtonLabelsSpanki != null)
                        table = _GradeButtonLabelsSpanki;
                    else
                        table = GetDefaultGradeButtonLabelsTranslatedSpanki(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Manual:
                    table = GradeButtonLabelsManual;
                    break;
            }

            return table;
        }

        public void SetGradeButtonLabelsTranslated(
            List<string> value,
            LanguageUtilities languageUtilities)
        {
            if (ObjectUtilities.CompareStringLists(value, GetGradeButtonLabelsTranslated(languageUtilities)) != 0)
                GradeButtonLabelsNoDefault = value;
        }

        public List<string> GradeButtonLabelsNoDefault
        {
            get
            {
                List<string> table;

                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        table = _GradeButtonLabelsSequential;
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        table = _GradeButtonLabelsAdaptive;
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        table = _GradeButtonLabelsChunky;
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        table = _GradeButtonLabelsLeitner;
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        table = _GradeButtonLabelsSpacedRepetition;
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        table = _GradeButtonLabelsSpanki;
                        break;
                    case SelectorAlgorithmCode.Manual:
                        table = null;
                        break;
                }

                return table;
            }
            set
            {
                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSequential) != 0)
                        {
                            _GradeButtonLabelsSequential = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsAdaptive) != 0)
                        {
                            _GradeButtonLabelsAdaptive = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsChunky) != 0)
                        {
                            _GradeButtonLabelsChunky = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsLeitner) != 0)
                        {
                            _GradeButtonLabelsLeitner = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSpacedRepetition) != 0)
                        {
                            _GradeButtonLabelsSpacedRepetition = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonLabelsSpanki) != 0)
                        {
                            _GradeButtonLabelsSpanki = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Manual:
                        break;
                }
            }
        }

        public string GetGradeButtonLabel(int grade, LanguageUtilities languageUtilities)
        {
            List<string> values = GetGradeButtonLabelsTranslated(languageUtilities);
            if (values == null)
                return String.Empty;
            if ((grade >= 1) && (grade <= values.Count()))
                return values[grade - 1];
            return String.Empty;
        }

        public int GetGradeNumber(string label, LanguageUtilities languageUtilities)
        {
            int grade = GetGradeButtonLabelsTranslated(languageUtilities).IndexOf(label) + 1;
            return grade;
        }

        public List<string> GetGradeButtonLabelsTranslatedSequential(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsSequential == null)
                return GetDefaultGradeButtonLabelsTranslatedSequential(languageUtilities);

            return _GradeButtonLabelsSequential;
        }

        public List<string> GetGradeButtonLabelsTranslatedAdaptive(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsAdaptive == null)
                return GetDefaultGradeButtonLabelsTranslatedAdaptive(languageUtilities);

            return _GradeButtonLabelsAdaptive;
        }

        public List<string> GetGradeButtonLabelsTranslatedChunky(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsChunky == null)
                return GetDefaultGradeButtonLabelsTranslatedChunky(languageUtilities);

            return _GradeButtonLabelsChunky;
        }

        public List<string> GetGradeButtonLabelsTranslatedLeitner(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsLeitner == null)
                return GetDefaultGradeButtonLabelsTranslatedLeitner(languageUtilities);

            return _GradeButtonLabelsLeitner;
        }

        public List<string> GetGradeButtonLabelsTranslatedSpacedRepetition(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsSpacedRepetition == null)
                return GetDefaultGradeButtonLabelsTranslatedSpacedRepetition(languageUtilities);

            return _GradeButtonLabelsSpacedRepetition;
        }

        public List<string> GetGradeButtonLabelsTranslatedSpanki(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonLabelsSpanki == null)
                return GetDefaultGradeButtonLabelsTranslatedSpanki(languageUtilities);

            return _GradeButtonLabelsSpanki;
        }

        public List<string> GetGradeButtonLabelsTranslatedManual(LanguageUtilities languageUtilities)
        {
            return GradeButtonLabelsManual;
        }

        public List<string> GetDefaultGradeButtonLabelsTranslated(LanguageUtilities languageUtilities)
        {
            List<string> defaults;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                    defaults = GetDefaultGradeButtonLabelsTranslatedSequential(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Adaptive:
                default:
                    defaults = GetDefaultGradeButtonLabelsTranslatedAdaptive(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Chunky:
                    defaults = GetDefaultGradeButtonLabelsTranslatedChunky(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Leitner:
                    defaults = GetDefaultGradeButtonLabelsTranslatedLeitner(languageUtilities);
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    defaults = GetDefaultGradeButtonLabelsTranslatedSpacedRepetition(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Spanki:
                    defaults = GetDefaultGradeButtonLabelsTranslatedSpanki(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Manual:
                    defaults = GetDefaultGradeButtonLabelsTranslatedManual(languageUtilities);
                    break;
            }

            return defaults;
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedSequential(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SequentialLabels",
                _DefaultGradeButtonLabelsSequential);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedAdaptive(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "AdaptiveLabels",
                _DefaultGradeButtonLabelsAdaptive);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedChunky(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "ChunkyLabels",
                _DefaultGradeButtonLabelsChunky);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedLeitner(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "LeitnerLabels",
                _DefaultGradeButtonLabelsLeitner);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedSpacedRepetition(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SpacedRepetitionLabels",
                _DefaultGradeButtonLabelsSpacedRepetition);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedSpanki(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SpankiLabels",
                _DefaultGradeButtonLabelsSpanki);
        }

        public List<string> GetDefaultGradeButtonLabelsTranslatedManual(LanguageUtilities languageUtilities)
        {
            return GradeButtonLabelsManual;
        }

        public List<string> GradeButtonTipsSequential
        {
            get
            {
                if (_GradeButtonTipsSequential == null)
                    return _DefaultGradeButtonTipsSequential;

                return _GradeButtonTipsSequential;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSequential) != 0)
                {
                    _GradeButtonTipsSequential = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsAdaptive
        {
            get
            {
                if (_GradeButtonTipsAdaptive == null)
                {
                    List<string> defaults;

                    switch (_GradeCount)
                    {
                        case 1:
                        case 2:
                            defaults = _DefaultGradeButtonTipsAdaptive2;
                            break;
                        case 3:
                            defaults = _DefaultGradeButtonTipsAdaptive3;
                            break;
                        case 4:
                            defaults = _DefaultGradeButtonTipsAdaptive4;
                            break;
                        case 5:
                        default:
                            defaults = _DefaultGradeButtonTipsAdaptive5;
                            break;
                    }

                    return defaults;
                }

                return _GradeButtonTipsAdaptive;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsAdaptive) != 0)
                {
                    _GradeButtonTipsAdaptive = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsChunky
        {
            get
            {
                return _GradeButtonTipsChunky;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsChunky) != 0)
                {
                    _GradeButtonTipsChunky = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsLeitner
        {
            get
            {
                return _GradeButtonTipsLeitner;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsLeitner) != 0)
                {
                    _GradeButtonTipsLeitner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsSpacedRepetition
        {
            get
            {
                return _GradeButtonTipsSpacedRepetition;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSpacedRepetition) != 0)
                {
                    _GradeButtonTipsSpacedRepetition = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsSpanki
        {
            get
            {
                return _GradeButtonTipsSpanki;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSpanki) != 0)
                {
                    _GradeButtonTipsSpanki = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> GradeButtonTipsManual
        {
            get
            {
                List<string> list = new List<string>(_GradeCount);
                int c = _GradeCount;
                int sitCount = (SpacedIntervalTable != null ? SpacedIntervalTable.Count() : 0);
                if (c >= sitCount)
                    c = sitCount;
                for (int i = 1; i <= c; i++)
                {
                    TimeSpan timeOffset = GetTimeOffset((float)i, 0.0f, (float)i, 0);
                    string tip = ObjectUtilities.GetStringFromTimeSpanAbbrev(timeOffset, null);
                    list.Add(tip);
                }
                return list;
            }
        }

        public List<string> GetGradeButtonTipsTranslatedSequential(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsSequential == null)
                return GetDefaultGradeButtonTipsTranslatedSequential(languageUtilities);

            return _GradeButtonTipsSequential;
        }

        public List<string> GetGradeButtonTipsTranslatedAdaptive(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsAdaptive == null)
                return GetDefaultGradeButtonTipsTranslatedAdaptive(languageUtilities);

            return _GradeButtonTipsAdaptive;
        }

        public List<string> GetGradeButtonTipsTranslatedChunky(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsChunky == null)
                return GetDefaultGradeButtonTipsTranslatedChunky(languageUtilities);

            return _GradeButtonTipsChunky;
        }

        public List<string> GetGradeButtonTipsTranslatedLeitner(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsLeitner == null)
                return GetDefaultGradeButtonTipsTranslatedLeitner(languageUtilities);

            return _GradeButtonTipsLeitner;
        }

        public List<string> GetGradeButtonTipsTranslatedSpacedRepetition(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsSpacedRepetition == null)
                return GetDefaultGradeButtonTipsTranslatedSpacedRepetition(languageUtilities);

            return _GradeButtonTipsSpacedRepetition;
        }

        public List<string> GetGradeButtonTipsTranslatedSpanki(LanguageUtilities languageUtilities)
        {
            if (_GradeButtonTipsSpanki == null)
                return GetDefaultGradeButtonTipsTranslatedSpanki(languageUtilities);

            return _GradeButtonTipsSpanki;
        }

        public List<string> GetGradeButtonTipsTranslatedManual(LanguageUtilities languageUtilities)
        {
            return GetDefaultGradeButtonTipsTranslatedManual(languageUtilities);
        }

        public List<string> GetGradeButtonTipsTranslated(LanguageUtilities languageUtilities)
        {
            List<string> table;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                    if (_GradeButtonTipsSequential != null)
                        table = _GradeButtonTipsSequential;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedSequential(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Adaptive:
                default:
                    if (_GradeButtonTipsAdaptive != null)
                        table = _GradeButtonTipsAdaptive;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedAdaptive(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Chunky:
                    if (_GradeButtonTipsChunky != null)
                        table = _GradeButtonTipsChunky;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedLeitner(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Leitner:
                    if (_GradeButtonTipsLeitner != null)
                        table = _GradeButtonTipsLeitner;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedLeitner(languageUtilities);
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    if (_GradeButtonTipsSpacedRepetition != null)
                        table = _GradeButtonTipsSpacedRepetition;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedSpacedRepetition(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Spanki:
                    if (_GradeButtonTipsSpanki != null)
                        table = _GradeButtonTipsSpanki;
                    else
                        table = GetDefaultGradeButtonTipsTranslatedSpanki(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Manual:
                    table = GetDefaultGradeButtonTipsTranslatedManual(languageUtilities);
                    break;
            }

            return table;
        }

        public void SetGradeButtonTipsTranslated(
            List<string> value,
            LanguageUtilities languageUtilities)
        {
            if (ObjectUtilities.CompareStringLists(value, GetGradeButtonTipsTranslated(languageUtilities)) != 0)
                GradeButtonTipsNoDefault = value;
        }

        public List<string> GradeButtonTipsNoDefault
        {
            get
            {
                List<string> table;

                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        table = _GradeButtonTipsSequential;
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        table = _GradeButtonTipsAdaptive;
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        table = _GradeButtonTipsChunky;
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        table = _GradeButtonTipsLeitner;
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        table = _GradeButtonTipsSpacedRepetition;
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        table = _GradeButtonTipsSpanki;
                        break;
                    case SelectorAlgorithmCode.Manual:
                        table = null;
                        break;
                }

                return table;
            }
            set
            {
                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSequential) != 0)
                        {
                            _GradeButtonTipsSequential = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsAdaptive) != 0)
                        {
                            _GradeButtonTipsAdaptive = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsChunky) != 0)
                        {
                            _GradeButtonTipsChunky = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsLeitner) != 0)
                        {
                            _GradeButtonTipsLeitner = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSpacedRepetition) != 0)
                        {
                            _GradeButtonTipsSpacedRepetition = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        if (ObjectUtilities.CompareStringLists(value, _GradeButtonTipsSpanki) != 0)
                        {
                            _GradeButtonTipsSpanki = value;
                            ModifiedFlag = true;
                        }
                        break;
                    case SelectorAlgorithmCode.Manual:
                        break;
                }
            }
        }

        public List<string> DefaultGradeButtonTips
        {
            get
            {
                List<string> defaults;

                switch (_SelectorAlgorithm)
                {
                    case SelectorAlgorithmCode.Forward:
                    case SelectorAlgorithmCode.Reverse:
                    case SelectorAlgorithmCode.Random:
                        defaults = _DefaultGradeButtonTipsSequential;
                        break;
                    case SelectorAlgorithmCode.Adaptive:
                    default:
                        switch (_GradeCount)
                        {
                            case 1:
                            case 2:
                                defaults = _DefaultGradeButtonTipsAdaptive2;
                                break;
                            case 3:
                                defaults = _DefaultGradeButtonTipsAdaptive3;
                                break;
                            case 4:
                                defaults = _DefaultGradeButtonTipsAdaptive4;
                                break;
                            case 5:
                            default:
                                defaults = _DefaultGradeButtonTipsAdaptive5;
                                break;
                        }
                        break;
                    case SelectorAlgorithmCode.Chunky:
                        defaults = _DefaultGradeButtonTipsChunky;
                        break;
                    case SelectorAlgorithmCode.Leitner:
                        defaults = _DefaultGradeButtonTipsLeitner;
                        break;
                    case SelectorAlgorithmCode.SpacedRepetition:
                        defaults = _DefaultGradeButtonTipsSpacedRepetition;
                        break;
                    case SelectorAlgorithmCode.Spanki:
                        defaults = _DefaultGradeButtonTipsSpanki;
                        break;
                    case SelectorAlgorithmCode.Manual:
                        defaults = GradeButtonLabelsManual;
                        break;
                }

                return defaults;
            }
        }

        public List<string> GetDefaultGradeButtonTipsTranslated(LanguageUtilities languageUtilities)
        {
            List<string> defaults;

            switch (_SelectorAlgorithm)
            {
                case SelectorAlgorithmCode.Forward:
                case SelectorAlgorithmCode.Reverse:
                case SelectorAlgorithmCode.Random:
                    defaults = GetDefaultGradeButtonTipsTranslatedSequential(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Adaptive:
                default:
                    defaults = GetDefaultGradeButtonTipsTranslatedAdaptive(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Chunky:
                    defaults = GetDefaultGradeButtonTipsTranslatedChunky(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Leitner:
                    defaults = GetDefaultGradeButtonTipsTranslatedLeitner(languageUtilities);
                    break;
                case SelectorAlgorithmCode.SpacedRepetition:
                    defaults = GetDefaultGradeButtonTipsTranslatedSpacedRepetition(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Spanki:
                    defaults = GetDefaultGradeButtonTipsTranslatedSpanki(languageUtilities);
                    break;
                case SelectorAlgorithmCode.Manual:
                    defaults = GetDefaultGradeButtonTipsTranslatedManual(languageUtilities);
                    break;
            }

            return defaults;
        }

        public string GetGradeButtonTip(int grade, LanguageUtilities languageUtilities)
        {
            List<string> values = GetGradeButtonTipsTranslated(languageUtilities);
            if (values == null)
                return String.Empty;
            if ((grade >= 1) && (grade <= values.Count()))
                return values[grade - 1];
            return String.Empty;
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedSequential(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SequentialTips",
                _DefaultGradeButtonTipsSequential);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedAdaptive(LanguageUtilities languageUtilities)
        {
            List<string> defaults;

            switch (_GradeCount)
            {
                case 1:
                case 2:
                    defaults = _DefaultGradeButtonTipsAdaptive2;
                    break;
                case 3:
                    defaults = _DefaultGradeButtonTipsAdaptive3;
                    break;
                case 4:
                    defaults = _DefaultGradeButtonTipsAdaptive4;
                    break;
                case 5:
                default:
                    defaults = _DefaultGradeButtonTipsAdaptive5;
                    break;
            }

            return languageUtilities.TranslateUITextListNonNumeric(
                "Adaptive" + _GradeCount.ToString() + "Tips",
                defaults);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedChunky(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "ChunkyTips",
                _DefaultGradeButtonTipsChunky);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedLeitner(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "LeitnerTips",
                _DefaultGradeButtonTipsLeitner);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedSpacedRepetition(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SpacedRepetitionTips",
                _DefaultGradeButtonTipsSpacedRepetition);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedSpanki(LanguageUtilities languageUtilities)
        {
            return languageUtilities.TranslateUITextListNonNumeric(
                "SpankiTips",
                _DefaultGradeButtonTipsSpanki);
        }

        public List<string> GetDefaultGradeButtonTipsTranslatedManual(LanguageUtilities languageUtilities)
        {
            List<string> list = new List<string>(_GradeCount);
            int c = _GradeCount;
            int sitCount = (SpacedIntervalTable != null ? SpacedIntervalTable.Count() : 0);
            if (c >= sitCount)
                c = sitCount;
            for (int i = 1; i <= c; i++)
            {
                TimeSpan timeOffset = GetTimeOffset((float)i, 0.0f, (float)i, 0);
                string tip = ObjectUtilities.GetStringFromTimeSpanAbbrev(timeOffset, languageUtilities);
                list.Add(tip);
            }
            return list;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_ToolConfigurations != null)
                {
                    foreach (ToolConfiguration configuration in _ToolConfigurations)
                    {
                        if (configuration.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_ToolConfigurations != null)
                {
                    foreach (ToolConfiguration configuration in _ToolConfigurations)
                        configuration.Modified = false;
                }
            }
        }

        public static SelectorAlgorithmCode GetSelectorAlgorithmCodeFromString(string str)
        {
            SelectorAlgorithmCode code;

            switch (str.ToLower().Replace(" ", ""))
            {
                case "forward":
                    code = SelectorAlgorithmCode.Forward;
                    break;
                case "reverse":
                    code = SelectorAlgorithmCode.Reverse;
                    break;
                case "random":
                    code = SelectorAlgorithmCode.Random;
                    break;
                case "adaptive":
                    code = SelectorAlgorithmCode.Adaptive;
                    break;
                case "chunky":
                    code = SelectorAlgorithmCode.Chunky;
                    break;
                case "leitner":
                    code = SelectorAlgorithmCode.Leitner;
                    break;
                case "spacedrepetition":
                    code = SelectorAlgorithmCode.SpacedRepetition;
                    break;
                case "spanki":
                    code = SelectorAlgorithmCode.Spanki;
                    break;
                case "manual":
                    code = SelectorAlgorithmCode.Manual;
                    break;
                default:
                    throw new ObjectException("ToolProfile.GetSelectorAlgorithmCodeFromString:  Unknown code:  " + str);
            }

            return code;
        }

        public static string GetSelectorStringFromCodeString(string str)
        {
            switch (str)
            {
                case "Forward":
                    str = "Forward";
                    break;
                case "Reverse":
                    str = "Reverse";
                    break;
                case "Random":
                    str = "Random";
                    break;
                case "Adaptive":
                    str = "Adaptive";
                    break;
                case "Chunky":
                    str = "Chunky";
                    break;
                case "Leitner":
                    str = "Leitner";
                    break;
                case "SpacedRepetition":
                case "Spaced Repetition":
                    str = "Spaced Repetition";
                    break;
                case "Spanki":
                    str = "Spanki";
                    break;
                case "Manual":
                    str = "Manual";
                    break;
                default:
                    throw new ObjectException("ToolProfile.GetSelectorStringFromCodeString:  Unknown code:  " + str);
            }

            return str;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XElement("GradeCount", _GradeCount.ToString()));
            element.Add(new XElement("SelectorAlgorithm", _SelectorAlgorithm.ToString()));

            if (_NewLimit > 0)
                element.Add(new XElement("NewLimit", _NewLimit.ToString()));

            if (_ReviewLimit > 0)
                element.Add(new XElement("ReviewLimit", _ReviewLimit.ToString()));

            if (_LimitExpiration != TimeSpan.Zero)
                element.Add(
                    new XElement(
                        "LimitExpiration",
                        ObjectUtilities.GetStringFromTimeSpan(_LimitExpiration)));

            element.Add(new XElement("IsRandomUnique", _IsRandomUnique.ToString()));
            element.Add(new XElement("IsRandomNew", _IsRandomNew.ToString()));
            element.Add(new XElement("IsAdaptiveMixNew", _IsAdaptiveMixNew.ToString()));
            element.Add(new XElement("ReviewLevel", _ReviewLevel.ToString()));
            element.Add(new XElement("StageResetGradeThreshold", _StageResetGradeThreshold.ToString()));
            element.Add(new XElement("StageIncrementGradeThreshold", _StageIncrementGradeThreshold.ToString()));
            element.Add(new XElement("HighGradeMultiplier", _HighGradeMultiplier.ToString()));
            element.Add(new XElement("EasyGradeMultiplier", _EasyGradeMultiplier.ToString()));
            element.Add(new XElement("HardGradeMultiplier", _HardGradeMultiplier.ToString()));
            element.Add(new XElement("SampleSize", _SampleSize.ToString()));
            element.Add(new XElement("ChoiceSize", _ChoiceSize.ToString()));
            element.Add(new XElement("ChunkSize", _ChunkSize.ToString()));
            element.Add(new XElement("HistorySize", _HistorySize.ToString()));
            element.Add(new XElement("IsShowIndex", _IsShowIndex.ToString()));
            element.Add(new XElement("IsShowOrdinal", _IsShowOrdinal.ToString()));

            if (_SpacedIntervalTable != null)
                element.Add(new XElement("SpacedIntervalTable", TextUtilities.GetStringFromTimeSpanList(_SpacedIntervalTable)));

            if (_IntervalTable != null)
                element.Add(new XElement("IntervalTable", TextUtilities.GetStringFromTimeSpanList(_IntervalTable)));

            if (_FontFamily != null)
                element.Add(new XElement("FontFamily", _FontFamily));

            if (_FlashFontSize != null)
                element.Add(new XElement("FlashFontSize", _FlashFontSize));

            if (_ListFontSize != null)
                element.Add(new XElement("ListFontSize", _ListFontSize));

            if (_MaximumLineLength != 0)
                element.Add(new XElement("MaximumLineLength", _MaximumLineLength.ToString()));

            if ((_ToolConfigurations != null) && (_ToolConfigurations.Count() != 0))
                element.Add(new XAttribute("ToolConfigurationCount", _ToolConfigurations.Count().ToString()));

            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsSequential", _GradeButtonLabelsSequential);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsAdaptive", _GradeButtonLabelsAdaptive);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsChunky", _GradeButtonLabelsChunky);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsLeitner", _GradeButtonLabelsLeitner);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsSpacedRepetition", _GradeButtonLabelsSpacedRepetition);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonLabelsSpanki", _GradeButtonLabelsSpanki);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsSequential", _GradeButtonTipsSequential);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsAdaptive", _GradeButtonTipsAdaptive);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsChunky", _GradeButtonTipsChunky);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsLeitner", _GradeButtonTipsLeitner);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsSpacedRepetition", _GradeButtonTipsSpacedRepetition);
            ObjectUtilities.AddStringListElementCheck(element, "GradeButtonTipsSpanki", _GradeButtonTipsSpanki);

            if (_ToolConfigurations != null)
            {
                foreach (ToolConfiguration configuration in _ToolConfigurations)
                    element.Add(configuration.Xml);
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ToolConfigurationCount":
                    _ToolConfigurations = new List<ToolConfiguration>(Convert.ToInt32(attributeValue));
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            ToolConfiguration configuration;
            string elementValue = childElement.Value.Trim();

            switch (childElement.Name.LocalName)
            {
                case "SelectorAlgorithm":
                    _SelectorAlgorithm = GetSelectorAlgorithmCodeFromString(childElement.Value.Trim());
                    break;
                case "NewLimit":
                    _NewLimit = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "ReviewLimit":
                    _ReviewLimit = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "LimitExpiration":
                    _LimitExpiration = ObjectUtilities.GetTimeSpanFromString(
                        childElement.Value.Trim(),
                        TimeSpan.Zero);
                    break;
                case "IsRandomUnique":
                    _IsRandomUnique = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "IsRandomNew":
                    _IsRandomNew = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "IsAdaptiveMixNew":
                    _IsAdaptiveMixNew = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "ReviewLevel":
                    _ReviewLevel = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "StageResetGradeThreshold":
                    _StageResetGradeThreshold = (float)Convert.ToDouble(childElement.Value.Trim());
                    break;
                case "StageIncrementGradeThreshold":
                    _StageIncrementGradeThreshold = (float)Convert.ToDouble(childElement.Value.Trim());
                    break;
                case "HighGradeMultiplier":
                    _HighGradeMultiplier = (float)Convert.ToDouble(childElement.Value.Trim());
                    break;
                case "EasyGradeMultiplier":
                    _EasyGradeMultiplier = (float)Convert.ToDouble(childElement.Value.Trim());
                    break;
                case "HardGradeMultiplier":
                    _HardGradeMultiplier = (float)Convert.ToDouble(childElement.Value.Trim());
                    break;
                case "SampleSize":
                    _SampleSize = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "ChoiceSize":
                    _ChoiceSize = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "ChunkSize":
                    _ChunkSize = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "HistorySize":
                    _HistorySize = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "IsShowIndex":
                    _IsShowIndex = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "IsShowOrdinal":
                    _IsShowOrdinal = (childElement.Value.Trim() == "True" ? true : false);
                    break;
                case "GradeIntervalTable":      // Legacy
                    break;
                case "SpacedIntervalTable":
                    _SpacedIntervalTable = TextUtilities.GetTimeSpanListFromString(childElement.Value.Trim());
                    break;
                case "IntervalTable":
                    _IntervalTable = TextUtilities.GetTimeSpanListFromString(childElement.Value.Trim());
                    break;
                case "FlashFontSize":
                    _FlashFontSize = elementValue;
                    break;
                case "FontFamily":
                    _FontFamily = elementValue;
                    break;
                case "ListFontSize":
                    _ListFontSize = elementValue;
                    break;
                case "MaximumLineLength":
                    MaximumLineLength = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "GradeCount":
                    _GradeCount = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "GradeButtonLabels":   // Legacy
                    break;
                case "GradeButtonLabelsSequential":
                    _GradeButtonLabelsSequential = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonLabelsAdaptive":
                    _GradeButtonLabelsAdaptive = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonLabelsChunky":
                    _GradeButtonLabelsChunky = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonLabelsLeitner":
                    _GradeButtonLabelsLeitner = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonLabelsSpacedRepetition":
                    _GradeButtonLabelsSpacedRepetition = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonLabelsSpanki":
                    _GradeButtonLabelsSpanki = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTips":
                    break;
                case "GradeButtonTipsSequential":
                    _GradeButtonTipsSequential = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTipsAdaptive":
                    _GradeButtonTipsAdaptive = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTipsChunky":
                    _GradeButtonTipsChunky = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTipsLeitner":
                    _GradeButtonTipsLeitner = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTipsSpacedRepetition":
                    _GradeButtonTipsSpacedRepetition = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "GradeButtonTipsSpanki":
                    _GradeButtonTipsSpanki = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "ToolConfiguration":
                    configuration = new ToolConfiguration(childElement);
                    AddToolConfiguration(configuration);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolProfile otherObject = other as ToolProfile;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;
            diff = CompareSelectorAlgorithmCodes(_SelectorAlgorithm, otherObject.SelectorAlgorithm);
            if (diff != 0)
                return diff;
            diff = _NewLimit - otherObject.NewLimit;
            if (diff != 0)
                return diff;
            diff = _ReviewLimit - otherObject.ReviewLimit;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpans(_LimitExpiration, otherObject.LimitExpiration);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_IsRandomUnique, otherObject.IsRandomUnique);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_IsRandomNew, otherObject.IsRandomNew);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_IsAdaptiveMixNew, otherObject.IsAdaptiveMixNew);
            if (diff != 0)
                return diff;
            diff = _ReviewLevel - otherObject.ReviewLevel;
            if (diff != 0)
                return diff;
            if (_StageResetGradeThreshold > otherObject.StageResetGradeThreshold)
                return 1;
            else if (_StageResetGradeThreshold < otherObject.StageResetGradeThreshold)
                return -1;
            if (_StageIncrementGradeThreshold > otherObject.StageIncrementGradeThreshold)
                return 1;
            else if (_StageIncrementGradeThreshold < otherObject.StageIncrementGradeThreshold)
                return -1;
            diff = _SampleSize - otherObject.SampleSize;
            if (diff != 0)
                return diff;
            diff = _ChoiceSize - otherObject.ChoiceSize;
            if (diff != 0)
                return diff;
            diff = _ChunkSize - otherObject.ChunkSize;
            if (diff != 0)
                return diff;
            diff = _HistorySize - otherObject.HistorySize;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_IsShowIndex, otherObject.IsShowIndex);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_IsShowOrdinal, otherObject.IsShowOrdinal);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpanLists(SpacedIntervalTable, otherObject.SpacedIntervalTable);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpanLists(_IntervalTable, otherObject.IntervalTable);
            if (diff != 0)
                return diff;
            diff = String.Compare(_FontFamily, otherObject.FontFamily);
            if (diff != 0)
                return diff;
            diff = String.Compare(_FlashFontSize, otherObject.FlashFontSize);
            if (diff != 0)
                return diff;
            diff = String.Compare(_ListFontSize, otherObject.ListFontSize);
            if (diff != 0)
                return diff;
            diff = _MaximumLineLength - otherObject.MaximumLineLength;
            if (diff != 0)
                return diff;
            diff = ToolConfiguration.CompareToolConfigurationLists(_ToolConfigurations, otherObject.ToolConfigurations);
            return diff;
        }

        public static int Compare(ToolProfile object1, ToolProfile object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(ToolProfile object1, ToolProfile object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareSelectorAlgorithmCodes(SelectorAlgorithmCode item1, SelectorAlgorithmCode item2)
        {
            if (item1 == item2)
                return 0;
            else if (item1 > item2)
                return 1;
            return -1;
        }
    }
}

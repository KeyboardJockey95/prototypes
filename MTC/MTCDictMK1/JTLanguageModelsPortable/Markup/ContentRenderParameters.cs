using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Markup
{
    public class ContentRenderParameters
    {
        public int TreeKey;
        public BaseObjectNodeTree Tree;
        public int NodeKey;
        public BaseObjectNode Node;
        public MarkupTemplate MarkupTemplate;
        public MarkupRenderer MarkupRenderer;
        public string ContentKey;
        public BaseObjectContent Content;
        public ContentStudyList StudyList;
        public ContentMediaItem MediaItem;
        public LanguageMediaItem LanguageMediaItem;
        public string LanguageMediaItemKey;
        public int StudyItemIndex;
        public int StudyItemCount;
        public int SentenceIndex;
        public MultiLanguageItem StudyItem;
        public LanguageItem LanguageItem;
        public TextRun SentenceRun;
        public string FilterTag;
        public LanguageID ItemLanguageID;
        public LanguageID MediaLanguageID;
        public string SubType;
        public string SubSubType;
        public bool HasOptions;
        public string LanguageFormat;
        public string RowFormat;
        public string DisplayFormat;
        public bool ShowTranslations;
        public bool ShowWords;
        public bool ShowWordBorders;
        public bool ShowAnnotations;
        public bool ShowOrdinals;
        public bool ShowAlignment;
        public bool IsCanShowAlignment;
        public bool ShowAlignmentVocabulary;
        public bool ShowAlignmentPhrases;
        public bool UseAudio;
        public bool UsePicture;
        public string ElementType;
        public string StyleAttribute;
        public string ClassAttribute;
        public bool AutoPlay;
        public string Player;
        public string PlayerID;
        public bool ShowVolume;
        public bool ShowTimeSlider;
        public bool ShowTimeText;
        public string EndOfMedia;
        public string EndOfViewCallback;
        public string Speed;
        public string Label;
        public string Action;
        public string ViewName;
        public int SessionIndex;
        public ToolTypeCode ToolType;
        public ToolSourceCode ToolSource;
        public ToolViewCode ToolView;
        public string ProfileName;
        public string ConfigurationKey;
        public ToolSelectorMode Mode;

        public ContentRenderParameters(int treeKey, int nodeKey, string contentKey,
            int studyItemIndex, int studyItemCount, int sentenceIndex, string filterTag, LanguageID itemLanguageID,
            LanguageID mediaLanguageID, string subType, string subSubType, bool hasOptions,
            string languageFormat, string rowFormat, string displayFormat, bool showTranslations,
            bool showWords, bool showAnnotations, bool showOrdinals, bool useAudio, bool usePicture, string elementType,
            string styleAttribute, string classAttribute)
        {
            ClearContentRenderParameters();
            TreeKey = treeKey;
            NodeKey = nodeKey;
            ContentKey = contentKey;
            StudyItemIndex = studyItemIndex;
            StudyItemCount = studyItemCount;
            SentenceIndex = sentenceIndex;
            FilterTag = filterTag;
            ItemLanguageID = itemLanguageID;
            MediaLanguageID = mediaLanguageID;
            SubType = subType;
            SubSubType = subSubType;
            HasOptions = hasOptions;
            LanguageFormat = languageFormat;
            RowFormat = rowFormat;
            DisplayFormat = displayFormat;
            ShowTranslations = showTranslations;
            ShowWords = showWords;
            ShowWordBorders = false;
            ShowAlignment = false;
            IsCanShowAlignment = false;
            ShowAlignmentVocabulary = false;
            ShowAlignmentPhrases = true;
            ShowAnnotations = showAnnotations;
            ShowOrdinals = showOrdinals;
            UseAudio = useAudio;
            UsePicture = usePicture;
            ElementType = elementType;
            StyleAttribute = styleAttribute;
            ClassAttribute = classAttribute;
    }

    public ContentRenderParameters(BaseObjectNodeTree tree, BaseObjectNode node, BaseObjectContent content,
            ContentStudyList studyList, int studyItemIndex, int studyItemCount, int sentenceIndex, string filterTag,
            LanguageID itemLanguageID, LanguageID mediaLanguageID,
            string subType, string subSubType, bool hasOptions, string languageFormat, string rowFormat, string displayFormat,
            bool showTranslations,bool showWords, bool showAnnotations, bool showOrdinals, bool useAudio, bool usePicture, string elementType,
            string styleAttribute, string classAttribute)
        {
            ClearContentRenderParameters();
            Tree = tree;
            if (tree != null)
                TreeKey = tree.KeyInt;
            Node = node;
            if ((node != null) && (node != tree))
                NodeKey = node.KeyInt;
            Content = content;
            if (content != null)
                ContentKey = content.KeyString;
            StudyList = studyList;
            StudyItemIndex = studyItemIndex;
            StudyItemCount = studyItemCount;
            SentenceIndex = sentenceIndex;
            FilterTag = filterTag;
            ItemLanguageID = itemLanguageID;
            MediaLanguageID = mediaLanguageID;
            SubType = subType;
            SubSubType = subSubType;
            HasOptions = hasOptions;
            LanguageFormat = languageFormat;
            RowFormat = rowFormat;
            DisplayFormat = displayFormat;
            ShowTranslations = showTranslations;
            ShowWords = showWords;
            ShowWordBorders = false;
            ShowAlignment = false;
            IsCanShowAlignment = false;
            ShowAlignmentVocabulary = false;
            ShowAlignmentPhrases = true;
            ShowAnnotations = showAnnotations;
            ShowOrdinals = showOrdinals;
            UseAudio = useAudio;
            UsePicture = usePicture;
            ElementType = elementType;
            StyleAttribute = styleAttribute;
            ClassAttribute = classAttribute;
        }

        public ContentRenderParameters(BaseObjectNodeTree tree, BaseObjectNode node, string contentKey,
            int studyItemIndex, int studyItemCount, int sentenceIndex, string filterTag, LanguageID itemLanguageID,
            LanguageID mediaLanguageID, string subType, string subSubType, List<BaseString> optionsList,
            bool useAudio, bool usePicture, string playerID, bool autoPlay, string endOfMedia, string label)
        {
            ClearContentRenderParameters();
            Tree = tree;
            if (tree != null)
                TreeKey = tree.KeyInt;
            Node = node;
            if ((node != null) && (node != tree))
                NodeKey = node.KeyInt;
            ContentKey = contentKey;
            StudyItemIndex = studyItemIndex;
            StudyItemCount = studyItemCount;
            SentenceIndex = sentenceIndex;
            FilterTag = filterTag;
            ItemLanguageID = itemLanguageID;
            MediaLanguageID = mediaLanguageID;
            if (MediaLanguageID != null)
                LanguageMediaItemKey = MediaLanguageID.LanguageCultureExtensionCode;
            SubType = subType;
            SubSubType = subSubType;
            Player = "Full";
            PlayerID = playerID;
            AutoPlay = autoPlay;
            EndOfMedia = endOfMedia;
            EndOfViewCallback = null;
            Speed = null;
            Label = label;
            OptionsList = optionsList;
            UseAudio = useAudio;
            UsePicture = usePicture;
        }

        public ContentRenderParameters(MarkupTemplate markupTemplate,
            MultiLanguageItem multiLanguageItem,
            LanguageID itemLanguageID, LanguageID mediaLanguageID,
            string subType, string subSubType, List<BaseString> optionsList,
            bool useAudio, bool usePicture)
        {
            ClearContentRenderParameters();
            MarkupTemplate = markupTemplate;
            MarkupRenderer = null;
            ItemLanguageID = itemLanguageID;
            MediaLanguageID = mediaLanguageID;
            SubType = subType;
            SubSubType = subSubType;
            AutoPlay = true;
            Player = "Full";
            PlayerID = "1";
            OptionsList = optionsList;
            UseAudio = useAudio;
            UsePicture = usePicture;
        }

        public ContentRenderParameters(
            int treeKey,
            int nodeKey,
            string contentKey,
            string languageMediaItemKey,
            string speed,
            string endOfMedia,
            string endOfViewCallback,
            bool autoPlay,
            string player,
            string playerID,
            bool showVolume,
            bool showTimeSlider,
            bool showTimeText,
            string label,
            string action,
            string viewName)
        {
            ClearContentRenderParameters();
            TreeKey = treeKey;
            NodeKey = nodeKey;
            ContentKey = contentKey;
            LanguageMediaItemKey = languageMediaItemKey;
            Speed = speed;
            EndOfMedia = endOfMedia;
            EndOfViewCallback = endOfViewCallback;
            AutoPlay = autoPlay;
            Player = player;
            PlayerID = playerID;
            ShowVolume = showVolume;
            ShowTimeSlider = showTimeSlider;
            ShowTimeText = showTimeText;
            Label = label;
            Action = action;
            ViewName = viewName;
        }

        public ContentRenderParameters(ContentRenderParameters other)
        {
            CopyContentRenderParameters(other);
        }

        public ContentRenderParameters()
        {
            ClearContentRenderParameters();
        }

        public void ClearContentRenderParameters()
        {
            TreeKey = -1;
            Tree = null;
            Node = null;
            NodeKey = -1;
            MarkupTemplate = null;
            MarkupRenderer = null;
            Content = null;
            ContentKey = null;
            StudyList = null;
            MediaItem = null;
            LanguageMediaItem = null;
            LanguageMediaItemKey = null;
            StudyItemIndex = -1;
            StudyItemCount = -1;
            SentenceIndex = -1;
            StudyItem = null;
            LanguageItem = null;
            SentenceRun = null;
            FilterTag = String.Empty;
            ItemLanguageID = null;
            MediaLanguageID = null;
            SubType = String.Empty;
            SubSubType = String.Empty;
            LanguageFormat = "Mixed";
            RowFormat = "Paragraphs";
            DisplayFormat = "List";
            ShowWords = false;
            ShowWordBorders = false;
            ShowAlignment = false;
            IsCanShowAlignment = false;
            ShowAlignmentVocabulary = false;
            ShowAlignmentPhrases = true;
            ShowAnnotations = true;
            ShowOrdinals = false;
            UseAudio = true;
            UsePicture = true;
            ElementType = String.Empty;
            StyleAttribute = String.Empty;
            ClassAttribute = String.Empty;
            AutoPlay = true;
            Player = "Full";
            PlayerID = "1";
            ShowVolume = false;
            ShowTimeSlider = true;
            ShowTimeText = true;
            EndOfMedia = "Stop";
            EndOfViewCallback = null;
            Speed = null;
            Label = String.Empty;
            Action = "Content";
            ViewName = "BrowseContent";
            SessionIndex = 0;
            ToolType = ToolTypeCode.Unknown;
            ToolSource = ToolSourceCode.Unknown;
            ToolView = ToolViewCode.Unknown;
            ProfileName = String.Empty;
            ConfigurationKey = String.Empty;
            Mode = ToolSelectorMode.Unknown;
    }

    public void CopyContentRenderParameters(ContentRenderParameters other)
        {
            Tree = other.Tree;
            TreeKey = other.TreeKey;
            Node = other.Node;
            NodeKey = other.NodeKey;
            MarkupTemplate = other.MarkupTemplate;
            MarkupRenderer = other.MarkupRenderer;
            Content = other.Content;
            ContentKey = other.ContentKey;
            StudyList = other.StudyList;
            StudyItemIndex = other.StudyItemIndex;
            StudyItemCount = other.StudyItemCount;
            SentenceIndex = other.SentenceIndex;
            StudyItem = other.StudyItem;
            LanguageItem = other.LanguageItem;
            SentenceRun = other.SentenceRun;
            FilterTag = other.FilterTag;
            ItemLanguageID = other.ItemLanguageID;
            MediaLanguageID = other.MediaLanguageID;
            SubType = other.SubType;
            SubSubType = other.SubSubType;
            LanguageFormat = other.LanguageFormat;
            RowFormat = other.RowFormat;
            DisplayFormat = other.DisplayFormat;
            ShowWords = other.ShowWords;
            ShowWordBorders = other.ShowWordBorders;
            ShowAlignment = other.ShowAlignment;
            IsCanShowAlignment = other.IsCanShowAlignment;
            ShowAlignmentVocabulary = other.ShowAlignmentVocabulary;
            ShowAlignmentPhrases = other.ShowAlignmentPhrases;
            ShowAnnotations = other.ShowAnnotations;
            ShowOrdinals = other.ShowOrdinals;
            UseAudio = other.UseAudio;
            UsePicture = other.UsePicture;
            ElementType = other.ElementType;
            StyleAttribute = other.StyleAttribute;
            ClassAttribute = other.ClassAttribute;
            AutoPlay = other.AutoPlay;
            Player = other.Player;
            PlayerID = other.PlayerID;
            ShowVolume = other.ShowVolume;
            ShowTimeSlider = other.ShowTimeSlider;
            ShowTimeText = other.ShowTimeText;
            EndOfMedia = other.EndOfMedia;
            EndOfViewCallback = other.EndOfViewCallback;
            Speed = other.Speed;
            Label = other.Label;
            Action = other.Action;
            ViewName = other.ViewName;
            SessionIndex = other.SessionIndex;
            ToolType = other.ToolType;
            ToolSource = other.ToolSource;
            ToolView = other.ToolView;
            ProfileName = other.ProfileName;
            ConfigurationKey = other.ConfigurationKey;
            Mode = other.Mode;
        }

        public List<BaseString> OptionsList
        {
            get
            {
                List<BaseString> optionsList = new List<BaseString>();
                optionsList.Add(new BaseString("LanguageFormat", LanguageFormat));
                optionsList.Add(new BaseString("RowFormat", RowFormat));
                optionsList.Add(new BaseString("DisplayFormat", DisplayFormat));
                optionsList.Add(new BaseString("ShowWords", (ShowWords ? "true" : "false")));
                optionsList.Add(new BaseString("ShowWordBorders", (ShowWordBorders ? "true" : "false")));
                optionsList.Add(new BaseString("ShowAlignment", (ShowAlignment ? "true" : "false")));
                optionsList.Add(new BaseString("IsCanShowAlignment", (IsCanShowAlignment ? "true" : "false")));
                optionsList.Add(new BaseString("ShowAlignmentVocabulary", (ShowAlignmentVocabulary ? "true" : "false")));
                optionsList.Add(new BaseString("ShowAlignmentPhrases", (ShowAlignmentPhrases ? "true" : "false")));
                optionsList.Add(new BaseString("ShowAnnotations", (ShowAnnotations ? "true" : "false")));
                optionsList.Add(new BaseString("ShowOrdinals", (ShowOrdinals ? "true" : "false")));
                optionsList.Add(new BaseString("UseAudio", (UseAudio ? "true" : "false")));
                optionsList.Add(new BaseString("UsePicture", (UsePicture ? "true" : "false")));
                optionsList.Add(new BaseString("ElementType", ElementType));
                optionsList.Add(new BaseString("Style", StyleAttribute));
                optionsList.Add(new BaseString("Class", ClassAttribute));
                optionsList.Add(new BaseString("AutoPlay", (AutoPlay ? "true" : "false")));
                optionsList.Add(new BaseString("Player", Player));
                optionsList.Add(new BaseString("PlayerID", PlayerID));
                optionsList.Add(new BaseString("ShowVolume", (ShowVolume ? "true" : "false")));
                optionsList.Add(new BaseString("ShowTimeSlider", (ShowTimeSlider ? "true" : "false")));
                optionsList.Add(new BaseString("ShowTimeText", (ShowTimeText ? "true" : "false")));
                optionsList.Add(new BaseString("EndOfMedia", EndOfMedia));
                optionsList.Add(new BaseString("Speed", Speed));
                optionsList.Add(new BaseString("Label", Label));
                optionsList.Add(new BaseString("SessionIndex", SessionIndex.ToString()));
                optionsList.Add(new BaseString("ToolType", ToolType.ToString()));
                optionsList.Add(new BaseString("ToolSource", ToolSource.ToString()));
                optionsList.Add(new BaseString("ToolView", ToolView.ToString()));
                optionsList.Add(new BaseString("ProfileName", ProfileName));
                optionsList.Add(new BaseString("ConfigurationKey", ConfigurationKey));
                optionsList.Add(new BaseString("Mode", Mode.ToString()));
                return optionsList;
            }
            set
            {
                if (value != null)
                {
                    HasOptions = true;
                    foreach (BaseString option in value)
                        SetOption(option.KeyString, option.Text);
                }
                else
                    HasOptions = false;
            }
        }

        public bool SetOption(string optionName, string optionValue)
        {
            switch (optionName.ToLower())
            {
                case "languageformat":
                    SetOption(optionValue, null);
                    break;
                case "mixed":
                    LanguageFormat = "Mixed";
                    break;
                case "separate":
                    LanguageFormat = "Separate";
                    break;
                case "rowformat":
                    SetOption(optionValue, null);
                    break;
                case "paragraphs":
                    RowFormat = "Paragraphs";
                    break;
                case "sentences":
                    RowFormat = "Sentences";
                    break;
                case "displayformat":
                    SetOption(optionValue, null);
                    break;
                case "table":
                    DisplayFormat = "Table";
                    break;
                case "list":
                    DisplayFormat = "List";
                    break;
                case "showannotations":
                    ShowAnnotations = ParseOptionFlag(optionValue, true);
                    break;
                case "hideannotations":
                case "noannotations":
                    ShowAnnotations = !ParseOptionFlag(optionValue, true);
                    break;
                case "showordinals":
                    ShowOrdinals = ParseOptionFlag(optionValue, true);
                    break;
                case "hideordinals":
                case "noordinals":
                    ShowOrdinals = !ParseOptionFlag(optionValue, true);
                    break;
                case "showwords":
                    ShowWords = ParseOptionFlag(optionValue, true);
                    break;
                case "hidewords":
                case "nowords":
                    ShowWords = !ParseOptionFlag(optionValue, true);
                    break;
                case "showwordborders":
                    ShowWordBorders = ParseOptionFlag(optionValue, true);
                    break;
                case "hidewordborders":
                case "nowordborders":
                    ShowWordBorders = !ParseOptionFlag(optionValue, true);
                    break;
                case "showalignment":
                    ShowAlignment = ParseOptionFlag(optionValue, true);
                    break;
                case "hidealignment":
                case "noalignment":
                    ShowAlignment = !ParseOptionFlag(optionValue, true);
                    break;
                case "iscanshowalignment":
                    IsCanShowAlignment = ParseOptionFlag(optionValue, true);
                    break;
                case "noiscanshowalignment":
                    IsCanShowAlignment = !ParseOptionFlag(optionValue, true);
                    break;
                case "showalignmentvocabulary":
                    ShowAlignmentVocabulary = ParseOptionFlag(optionValue, true);
                    break;
                case "hidealignmentvocabulary":
                case "noalignmentvocabulary":
                    ShowAlignmentVocabulary = !ParseOptionFlag(optionValue, true);
                    break;
                case "showalignmentphrases":
                    ShowAlignmentPhrases = ParseOptionFlag(optionValue, true);
                    break;
                case "hidealignmentphrases":
                case "noalignmentphrases":
                    ShowAlignmentPhrases = !ParseOptionFlag(optionValue, true);
                    break;
                case "useaudio":
                    UseAudio = ParseOptionFlag(optionValue, true);
                    break;
                case "noaudio":
                    UseAudio = !ParseOptionFlag(optionValue, true);
                    break;
                case "usepicture":
                    UsePicture = ParseOptionFlag(optionValue, true);
                    break;
                case "nopicture":
                    UsePicture = !ParseOptionFlag(optionValue, true);
                    break;
                case "usemedia":
                    UseAudio = UsePicture = ParseOptionFlag(optionValue, true);
                    break;
                case "nomedia":
                    UseAudio = UsePicture = !ParseOptionFlag(optionValue, true);
                    break;
                case "elementtype":
                    ElementType = optionValue;
                    break;
                case "style":
                    StyleAttribute = optionValue;
                    break;
                case "class":
                    ClassAttribute = optionValue;
                    break;
                case "autoplay":
                    AutoPlay = ParseOptionFlag(optionValue, true);
                    break;
                case "player":
                    Player = optionValue;
                    break;
                case "playerid":
                    PlayerID = optionValue;
                    break;
                case "showvolume":
                    ShowVolume = ParseOptionFlag(optionValue, false);
                    break;
                case "showtimeslider":
                    ShowTimeSlider = ParseOptionFlag(optionValue, true);
                    break;
                case "showtimetext":
                    ShowTimeText = ParseOptionFlag(optionValue, true);
                    break;
                case "endofmedia":
                    EndOfMedia = optionValue;
                    break;
                case "speed":
                    Speed = optionValue;
                    break;
                case "label":
                    Label = optionValue;
                    break;
                case "sessionindex":
                    SessionIndex = ObjectUtilities.GetIntegerFromString(optionValue, 0);
                    break;
                case "tool":
                case "tooltype":
                    ToolType = ToolUtilities.GetToolTypeCodeFromString(optionValue);
                    break;
                case "source":
                case "toolsource":
                    ToolSource = ToolUtilities.GetToolSourceCodeFromString(optionValue);
                    break;
                case "view":
                case "toolview":
                    ToolView = ToolUtilities.GetToolViewCodeFromString(optionValue);
                    break;
                case "profilen":
                case "profilename":
                    ProfileName = optionValue;
                    break;
                case "configuration":
                case "configurationkey":
                    ConfigurationKey = optionValue;
                    break;
                case "mode":
                    Mode = ToolItemSelector.GetToolSelectorModeFromString(optionValue);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public bool ParseOptionFlag(string value, bool defaultValue)
        {
            switch (value.ToLower())
            {
                case "true":
                case "yes":
                case "on":
                    return true;
                case "false":
                case "no":
                case "off":
                    return false;
                case "":
                default:
                    return defaultValue;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Markup
{
    public class AutomatedCompiledMarkup : ControllerUtilities
    {
        public List<AutomatedInstruction> Instructions { get; set; }
        public int LabelOrdinal;
        public BaseObjectContent DocumentContent;
        public BaseObjectNodeTree SourceTree;
        public BaseObjectNode SourceNode;
        public List<BaseObjectContent> SourceContents;
        public List<LanguageDescriptor> LanguageDescriptors;
        public string GenerateMediaDirectoryUrl { get; set; }
        public string SharedMediaDirectoryUrl { get; set; }
        public int MaxFileBaseLength { get; set; }
        public int MaxFilePathLength { get; set; }
        public bool LastInstructionWasWait { get; set; }
        public bool IsWaitingForDone;
        public bool IsInterrupt;
        protected ToolUtilities ToolUtilities;
        protected ContentStudyList WorkingSetStudyList;
        protected ToolStudyList WorkingSet;
        protected List<ContentStudyList> WorkingSetStudyListStack;
        protected List<ToolStudyList> WorkingSetStack;
        protected List<ToolSession> SessionStack;
        protected BaseObjectNode Node;
        protected BaseObjectContent Content;
        protected ContentStudyList StudyList;
        protected MultiLanguageItem StudyItem;
        protected int StudyItemIndex;
        public ToolSession Session;
        protected string DefaultProfileName;
        protected string DefaultConfigurationName;
        protected ToolConfiguration Configuration;
        protected ToolItemSelector Selector;
        protected LanguageID CurrentHostLanguageID;
        protected LanguageID CurrentTargetLanguageID;
        protected DateTime StartTime;
        protected DateTime CurrentTime;
        protected TimeSpan LastItemTime;
        protected TimeSpan ElapsedTimeSpan;
        protected List<AutomatedInstruction> FixupStack;
        protected List<KeyValuePair<string, object>> VariableStack;
        protected List<ContentStudyList> StudyListUpdateCache;
        protected List<ToolStudyList> ToolStudyLists;
        protected Dictionary<string, int> MarkerDictionary;
        protected AutomatedMarkupRenderer Renderer;

        public AutomatedCompiledMarkup(AutomatedMarkupRenderer renderer)
            : base(
                  renderer.Repositories,
                  null,
                  renderer.UserRecord,
                  renderer.UserProfile,
                  null,
                  renderer.LanguageUtilities)
        {
            Instructions = new List<AutomatedInstruction>();
            LabelOrdinal = 0;
            DocumentContent = renderer.DocumentContent;
            SourceTree = renderer.SourceTree;
            SourceNode = renderer.SourceNode;
            SourceContents = renderer.SourceContents;
            //MarkupTemplate = renderer.MarkupTemplate;
            //TargetLanguageID = renderer.TargetLanguageID;
            //HostLanguageID = renderer.HostLanguageID;
            LanguageDescriptors = renderer.LanguageDescriptors;
            GenerateMediaDirectoryUrl = renderer.GenerateMediaDirectoryUrl;
            SharedMediaDirectoryUrl = renderer.SharedMediaDirectoryUrl;
            MaxFileBaseLength = renderer.MaxFileBaseLength;
            MaxFilePathLength = renderer.MaxFilePathLength;
            LastInstructionWasWait = false;
            IsWaitingForDone = false;
            IsInterrupt = false;
            //UseAudio = renderer.UseAudio;
            //UsePicture = renderer.UsePicture;
            //RenderedElement = new XElement("div");
            Node = SourceNode;
            if ((SourceContents != null) && (SourceContents.Count() != 0))
            {
                Content = SourceContents[0];
                StudyList = Content.GetContentStorageTyped<ContentStudyList>();
            }
            StudyItem = null;
            StudyItemIndex = -1;
            //Variables = new List<BaseString>();
            ToolUtilities = new ToolUtilities(
                Repositories, Cookies, UserRecord, UserProfile, Translator, LanguageUtilities, SourceTree, null);
            WorkingSetStudyList = null;
            WorkingSet = null;
            WorkingSetStudyListStack = new List<ContentStudyList>();
            WorkingSetStack = new List<ToolStudyList>();
            SessionStack = new List<ToolSession>();
            Session = null;
            DefaultConfigurationName = "Read0";
            DefaultProfileName = "Markup";
            Configuration = null;
            Selector = null;
            CurrentTargetLanguageID = renderer.TargetLanguageID;
            CurrentHostLanguageID = UILanguageID;
            StartTime = DateTime.UtcNow;
            CurrentTime = StartTime;
            LastItemTime = TimeSpan.Zero;
            ElapsedTimeSpan = TimeSpan.Zero;
            FixupStack = new List<AutomatedInstruction>();
            VariableStack = new List<KeyValuePair<string, object>>();
            StudyListUpdateCache = null;
            ToolStudyLists = new List<ToolStudyList>();
            MarkerDictionary = null;
            Renderer = renderer;
        }

        public void AddViewInstruction(string url, AutomatedContinueMode continueMode, int timeSeconds)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Url", url);
            arguments.Add("ContinueMode", (int)continueMode);
            arguments.Add("TimeSeconds", timeSeconds);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.View,
                    arguments));
        }

        public void AddStudyItemInstruction(string url, int itemIndex,
            AutomatedContinueMode continueMode, int timeSeconds)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Url", url);
            arguments.Add("ItemIndex", itemIndex);
            arguments.Add("ContinueMode", (int)continueMode);
            arguments.Add("TimeSeconds", timeSeconds);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.StudyItem,
                    arguments));
        }

        public void AddChooseInstruction(string url)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Url", url);
            arguments.Add("ContinueMode", (int)AutomatedContinueMode.WaitForUserCommand);
            arguments.Add("TimeSeconds", 0);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Choose,
                    arguments));
        }

        public void AddStudyItemTimeTextMapInstruction(string url)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Url", url);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.StudyItemTimeTextMap,
                    arguments));
        }

        public void AddPlayMediaInstruction(string mediaUrl, bool isAudio, string speed)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("MediaUrl", MediaUtilities.GetContentUrl(mediaUrl));
            arguments.Add("IsAudio", (isAudio ? 1 : 0));
            arguments.Add("Speed", speed);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PlayMedia,
                    arguments));
        }

        public void AddLabelInstruction(string label)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Label", label);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Label,
                    arguments));
        }

        public void AddTextInstruction(string text, string audioUrl)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Text", text);
            arguments.Add("AudioUrl", MediaUtilities.GetContentUrl(audioUrl));
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Text,
                    arguments));
        }

        public void AddTextSegmentInstruction(string text, string audioUrl,
            TimeSpan startTime, TimeSpan lengthTime)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Text", text);
            arguments.Add("AudioUrl", MediaUtilities.GetContentUrl(audioUrl));
            arguments.Add("StartTime", (int)startTime.TotalMilliseconds);
            arguments.Add("EndTime", (int)(startTime.TotalMilliseconds + lengthTime.TotalMilliseconds));
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.TextSegment,
                    arguments));
        }

        public void AddPauseInstruction(string mode, double value, double minimum)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Mode", mode);
            arguments.Add("Value", value);
            arguments.Add("Minimum", minimum);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Pause,
                    arguments));
        }

        public void AddWaitInstruction()
        {
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Wait,
                    null));
        }

        public void AddWaitForDoneInstruction()
        {
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.WaitForDone,
                    null));
        }

        public void AddItemInstruction(
            int index, int startIndex, int endIndex, string label, string tag,
            bool all, LanguageID languageID, string speed)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Index", index);
            arguments.Add("StartIndex", startIndex);
            arguments.Add("endIndex", endIndex);
            arguments.Add("Label", label);
            arguments.Add("Tag", tag);
            arguments.Add("All", all);
            arguments.Add("LanguageID", languageID);
            arguments.Add("Speed", speed);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.Item,
                    arguments));
        }

        public void AddPushWorkingSetInstruction(
            string contentKey, string tag, string label,
            SelectorAlgorithmCode selector, ToolSelectorMode mode,
            bool isRandomUnique, bool isRandomNew, bool isAdaptiveMixNew,
            int chunkSize, int level, string profileName, string configurationKey)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("ContentKey", contentKey);
            arguments.Add("Tag", tag);
            arguments.Add("Label", label);
            arguments.Add("Selector", selector.ToString());
            arguments.Add("Mode", mode.ToString());
            arguments.Add("IsRandomUnique", isRandomUnique);
            arguments.Add("IsRandomNew", isRandomNew);
            arguments.Add("IsAdaptiveMixNew", isAdaptiveMixNew);
            arguments.Add("ChunkSize", chunkSize);
            arguments.Add("Level", level);
            arguments.Add("ProfileName", profileName);
            arguments.Add("Configuration", configurationKey);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PushWorkingSet,
                    arguments));
        }

        public void AddPopWorkingSetInstruction()
        {
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PopWorkingSet,
                    null));
        }

        public void AddPushWorkingSetCountInstruction(string name)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Name", name);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PushWorkingSetCount,
                    arguments));
        }

        public void AddPushDurationTimesInstruction(TimeSpan duration)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Duration", duration);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PushDurationTimes,
                    arguments));
        }

        public void AddGetAndTouchItemInstruction(int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.GetAndTouchItem,
                    arguments));
        }

        public void AddNextItemAndBranchInstruction(int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.NextItemAndBranch,
                    arguments));
        }

        public void AddPushVariableInstruction(string name, object value)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Name", name);
            arguments.Add("Value", value);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PushVariable,
                    arguments));
        }

        public void AddPopVariableInstruction(string name)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Name", name);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.PopVariable,
                    arguments));
        }

        public void AddIncrementVariableInstruction(string name)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Name", name);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.IncrementVariable,
                    arguments));
        }

        public void AddIndexCountConditionalLoopInstruction(int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.IndexCountConditionalLoop,
                    arguments));
        }

        public void AddDurationConditionalLoopInstruction(int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.DurationConditionalLoop,
                    arguments));
        }

        public void AddUnconditionalBranchInstruction(int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.UnconditionalBranch,
                    arguments));
        }

        public void AddChoiceConditionalBranchInstruction(string choiceID, int targetLabel)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("ChoiceID", choiceID);
            arguments.Add("TargetLabel", targetLabel);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.ChoiceConditionalBranch,
                    arguments));
        }

        public void AddMarker(string marker, int targetIndex)
        {
            if (MarkerDictionary == null)
                MarkerDictionary = new Dictionary<string, int>();

            int testIndex;

            if (!MarkerDictionary.TryGetValue(marker, out testIndex))
                MarkerDictionary.Add(marker, targetIndex);
        }

        public void AddGoTo(string marker)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("Marker", marker);
            AddInstruction(
                new AutomatedInstruction(
                    AutomatedInstructionCommand.GoTo,
                    arguments));
        }

        public int GetMarkerTargetIndex(string marker)
        {
            if (MarkerDictionary == null)
                return -1;

            int targetIndex;

            if (!MarkerDictionary.TryGetValue(marker, out targetIndex))
            {
                marker = marker.ToLower();

                foreach (KeyValuePair<string, int> kvp in MarkerDictionary)
                {
                    if (kvp.Key.ToLower() == marker)
                        return kvp.Value;
                }

                return -1;
            }

            return targetIndex;
        }

        public void AddInstruction(AutomatedInstruction instruction)
        {
            instruction.Label = AllocateLabel();
            Instructions.Add(instruction);
            if ((instruction.Command == AutomatedInstructionCommand.Wait)
                    || (instruction.Command == AutomatedInstructionCommand.WaitForDone))
                LastInstructionWasWait = true;
            else
                LastInstructionWasWait = false;
        }

        public void InsertInstruction(int index, AutomatedInstruction instruction)
        {
            instruction.Label = AllocateLabel();
            Instructions.Insert(index, instruction);
        }

        public AutomatedInstruction GetInstructionIndexed(int index)
        {
            if ((index >= 0) && (index < Instructions.Count))
                return Instructions[index];
            return null;
        }

        public AutomatedInstruction GetInstructionLabeled(int label)
        {
            int index;
            int count = Instructions.Count;

            for (index = 0; index < count; index++)
            {
                AutomatedInstruction instruction = Instructions[index];

                if (instruction.Label == label)
                    return instruction;
            }

            return null;
        }

        public int GetLabelIndex(int label)
        {
            int index;
            int count = Instructions.Count;

            for (index = 0; index < count; index++)
            {
                AutomatedInstruction instruction = Instructions[index];

                if (instruction.Label == label)
                    return index;
            }

            return -1;
        }

        public int AllocateLabel()
        {
            return LabelOrdinal++;
        }

        public int NextLabel()
        {
            return LabelOrdinal + 1;
        }

        public void PushFixup()
        {
            FixupStack.Add(Instructions.Last());
        }

        public void PopFixup(int label, string name)
        {
            AutomatedInstruction instruction = FixupStack.Last();
            FixupStack.RemoveAt(FixupStack.Count - 1);
            instruction.SetArgument(name, label);
        }

        protected void PushVariable(string name, object value)
        {
            VariableStack.Add(new KeyValuePair<string, object>(name, value));
        }

        protected void PopVariable(string name)
        {
            int count = VariableStack.Count;
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                KeyValuePair<string, object> variable = VariableStack[index];

                if (variable.Key == name)
                {
                    VariableStack.RemoveAt(index);
                    break;
                }
            }
        }

        protected object GetVariable(string name)
        {
            int count = VariableStack.Count;
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                KeyValuePair<string, object> variable = VariableStack[index];

                if (variable.Key == name)
                    return variable.Value;
            }

            return null;
        }

        protected int GetIntegerVariable(string name)
        {
            object value = GetVariable(name);

            if (value == null)
                return 0;

            return (int)value;
        }

        protected string GetStringVariable(string name)
        {
            object value = GetVariable(name);

            if (value == null)
                return String.Empty;

            return value as string;
        }

        protected TimeSpan GetTimeSpanVariable(string name)
        {
            object value = GetVariable(name);

            if (value == null)
                return TimeSpan.Zero;

            return (TimeSpan)value;
        }

        public void SetVariable(string name, object value)
        {
            int count = VariableStack.Count;
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                KeyValuePair<string, object> variable = VariableStack[index];

                if (variable.Key == name)
                {
                    VariableStack[index] = new KeyValuePair<string, object>(name, value);
                    return;
                }
            }
        }

        public AutomatedExecutionGroup Execute(int instructionIndex)
        {
            List<AutomatedExecutionItem> executionItems = new List<AutomatedExecutionItem>();
            int index;
            int count = Instructions.Count;
            bool stopped = false;

            CurrentTime = DateTime.UtcNow;
            ElapsedTimeSpan = CurrentTime - StartTime;

            if (Selector != null)
                Selector.CustomNowTime = CurrentTime;

            for (index = instructionIndex; index < count;)
            {
                AutomatedInstruction instruction = GetInstructionIndexed(index);

                if (instruction == null)
                    break;

                bool doBreak = false;

                switch (instruction.Command)
                {
                    case AutomatedInstructionCommand.Nop:
                        index++;
                        break;
                    case AutomatedInstructionCommand.View:
                        HandleView(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.StudyItem:
                        HandleStudyItem(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.Choose:
                        HandleChoose(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.StudyItemTimeTextMap:
                        HandleStudyItemTimeTextMap(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PlayMedia:
                        HandlePlayMedia(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.Label:
                        HandleLabel(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.Text:
                        HandleText(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.TextSegment:
                        HandleTextSegment(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.Say:
                        HandleSay(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.Item:
                        HandleItem(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PushWorkingSet:
                        HandlePushWorkingSet(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PopWorkingSet:
                        HandlePopWorkingSet(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PushWorkingSetCount:
                        HandlePushWorkingSetCount(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PushDurationTimes:
                        HandlePushDurationTimes(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.GetAndTouchItem:
                        index = HandleGetAndTouchItem(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.NextItemAndBranch:
                        index = HandleNextItemAndBranch(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.PushVariable:
                        HandlePushVariable(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.PopVariable:
                        HandlePopVariable(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.IncrementVariable:
                        HandleIncrementVariable(instruction, executionItems);
                        index++;
                        break;
                    case AutomatedInstructionCommand.IndexCountConditionalLoop:
                        index = HandleIndexCountConditionalLoop(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.DurationConditionalLoop:
                        index = HandleDurationConditionalLoop(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.UnconditionalBranch:
                        index = HandleUnconditionalBranch(instruction, executionItems);
                        break;
                    case AutomatedInstructionCommand.ChoiceConditionalBranch:
                        index = HandleChoiceConditionalBranch(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.GoTo:
                        index = HandleGoTo(instruction, executionItems, index);
                        break;
                    case AutomatedInstructionCommand.Pause:
                        HandlePause(instruction, executionItems);
                        doBreak = true;
                        index++;
                        break;
                    case AutomatedInstructionCommand.Wait:
                        HandleWait(instruction, executionItems);
                        doBreak = true;
                        index++;
                        break;
                    case AutomatedInstructionCommand.WaitForDone:
                        if (HandleWaitForDone(instruction, executionItems))
                            doBreak = true;
                        else
                            index++;
                        break;
                    case AutomatedInstructionCommand.Stop:
                        stopped = true;
                        doBreak = true;
                        index++;
                        break;
                    default:
                        HandleError(
                            executionItems,
                            "Instruction not implemented: " + instruction.Command.ToString());
                        index++;
                        break;
                }

                if (doBreak)
                    break;
            }

            UpdateStudyLists(executionItems);
            UpdateToolStudyLists(executionItems);

            bool isLastCommand = (index >= count) || stopped;

            AutomatedExecutionGroup automatedItem = new AutomatedExecutionGroup(
                executionItems.Count,
                executionItems.ToArray(),
                index,
                isLastCommand);

            return automatedItem;
        }

        protected void HandleView(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            AutomatedContinueMode waitContinueMode = (AutomatedContinueMode)instruction.GetArgumentInt("ContinueMode");
            if (waitContinueMode == AutomatedContinueMode.WaitForDone)
                IsWaitingForDone = true;
            executionItems.Add(
                new AutomatedExecutionItem(
                    "DisplayView",
                    instruction.GetArgumentString("Url"),
                    (int)waitContinueMode,
                    instruction.GetArgumentInt("TimeSeconds")));
        }

        protected void HandleStudyItem(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string url = instruction.GetArgumentString("Url");
            int itemIndex = instruction.GetArgumentInt("ItemIndex");
            AutomatedContinueMode waitContinueMode = (AutomatedContinueMode)instruction.GetArgumentInt("ContinueMode");
            if (waitContinueMode == AutomatedContinueMode.WaitForDone)
                IsWaitingForDone = true;
            string contentKey = String.Empty;
            if (StudyItemIndex != -1)
                itemIndex = StudyItemIndex;
            if (itemIndex != -1)
                url = url.Replace("itemIndex=-1", "itemIndex=" + itemIndex.ToString());
            if (StudyItem != null)
            {
                BaseObjectContent content = StudyItem.Content;
                if (content != null)
                    contentKey = StudyItem.Content.KeyString;
            }
            url = url.Replace("contentKey=dummy", "contentKey=" + contentKey);
            executionItems.Add(
                new AutomatedExecutionItem(
                    "DisplayView",
                    url,
                    (int)waitContinueMode,
                    instruction.GetArgumentInt("TimeSeconds")));
        }

        protected void HandleChoose(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            AutomatedContinueMode waitContinueMode = (AutomatedContinueMode)instruction.GetArgumentInt("ContinueMode");
            if (waitContinueMode == AutomatedContinueMode.WaitForDone)
                IsWaitingForDone = true;
            executionItems.Add(
                new AutomatedExecutionItem(
                    "Choose",
                    instruction.GetArgumentString("Url"),
                    (int)waitContinueMode,
                    instruction.GetArgumentInt("TimeSeconds")));
        }

        protected void HandlePlayMedia(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string speed = instruction.GetArgumentString("Speed");

            if (String.IsNullOrEmpty(speed))
                speed = "Normal";

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        speed));

            executionItems.Add(
                new AutomatedExecutionItem(
                    "PlayMedia",
                    instruction.GetArgumentString("MediaUrl"),
                    instruction.GetArgumentInt("IsAudio")));

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        "Normal"));
        }

        protected void HandleLabel(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            executionItems.Add(
                new AutomatedExecutionItem(
                    "AppendLabel",
                    instruction.GetArgumentString("Label")));
        }

        protected void HandleStudyItemTimeTextMap(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            executionItems.Add(
                new AutomatedExecutionItem(
                    "StudyItemTimeTextMap",
                    instruction.GetArgumentString("Url")));
        }

        protected void HandleText(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            executionItems.Add(
                new AutomatedExecutionItem(
                    "PlayAudio",
                    instruction.GetArgumentString("AudioUrl")));
            executionItems.Add(
                new AutomatedExecutionItem(
                    "AppendText",
                    instruction.GetArgumentString("Text")));
        }

        protected void HandleTextSegment(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            executionItems.Add(
                new AutomatedExecutionItem(
                    "PlayAudioSegment",
                    instruction.GetArgumentString("AudioUrl"),
                    instruction.GetArgumentInt("StartTime"),
                    instruction.GetArgumentInt("EndTime")));
            executionItems.Add(
                new AutomatedExecutionItem(
                    "AppendText",
                    instruction.GetArgumentString("Text")));
        }

        protected void HandleSay(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
        }

        protected void HandleItem(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            int index = instruction.GetArgumentInt("Index");
            int startIndex = instruction.GetArgumentInt("ContentKey");
            int endIndex = instruction.GetArgumentInt("StartIndex");
            string label = instruction.GetArgumentString("Label");
            string tag = instruction.GetArgumentString("Tag");
            bool all = instruction.GetArgumentFlag("All");
            LanguageID languageID = instruction.GetArgumentLanguageID("LanguageID");
            string speed = instruction.GetArgumentString("Speed");

            if (WorkingSet == null)
                LoadWorkingSet(
                    null,
                    null,
                    null,
                    SelectorAlgorithmCode.Forward,
                    ToolSelectorMode.Normal,
                    0,
                    0,
                    false,
                    false,
                    false,
                    ToolProfile.DefaultChunkSize,
                    ToolProfile.DefaultReviewLevel,
                    DefaultProfileName,
                    ref DefaultConfigurationName);

            if (!String.IsNullOrEmpty(label))
            {
                if (!WorkingSet.GetLabeledToolStudyItemRange(label, CurrentHostLanguageID, out startIndex, out endIndex))
                {
                    HandleError(
                        executionItems,
                        "(Item label not found: " + label + ")");
                    return;
                }
            }

            if (!String.IsNullOrEmpty(tag))
            {
                if (!WorkingSet.GetTaggedToolStudyItemRange(tag, out startIndex, out endIndex))
                {
                    HandleError(
                        executionItems,
                        "(Item tag not found: " + label + ")");
                    return;
                }
            }

            if (all)
            {
                startIndex = 0;
                endIndex = WorkingSet.ToolStudyItemCount();
            }

            if (startIndex == -1)
            {
                if (StudyItem == null)
                    StudyItem = WorkingSet.GetToolStudyItemIndexed(0).StudyItem;

                if (StudyItem == null)
                    return;

                HandleMultiLanguageItem(StudyItem, languageID, speed, executionItems);

                if (StudyItem.Modified && (StudyItem.Content != null))
                    CacheUpdateStudyList(StudyItem.Content.ContentStorageStudyList);
            }
            else
            {
                if (endIndex < startIndex)
                    endIndex = startIndex + 1;

                for (index = startIndex; index < endIndex; index++)
                {
                    MultiLanguageItem multiLanguageItem = WorkingSet.GetToolStudyItemIndexed(index).StudyItem;

                    if (multiLanguageItem == null)
                        continue;

                    if (!HandleMultiLanguageItem(multiLanguageItem, languageID, speed, executionItems))
                        return;

                    if (multiLanguageItem.Modified && (multiLanguageItem.Content != null))
                        CacheUpdateStudyList(multiLanguageItem.Content.ContentStorageStudyList);
                }
            }
        }

        protected bool HandleMultiLanguageItem(
            MultiLanguageItem multiLanguageItem,
            LanguageID languageID,
            string speed,
            List<AutomatedExecutionItem> executionItems)
        {
            if (Content == null)
                return false;

            if (multiLanguageItem == null)
                return false;

            string mediaDirectoryUrl;

            if (multiLanguageItem.Content != null)
                mediaDirectoryUrl = multiLanguageItem.Content.MediaTildeUrl;
            else
                mediaDirectoryUrl = Content.MediaTildeUrl;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);
            string speakerNameKey = multiLanguageItem.SpeakerNameKey;

            if (languageItem == null)
                return false;

            speed = Renderer.GetSpeedKeyFromString(speed);

            int sentenceCount = (languageItem.SentenceRuns != null ? languageItem.SentenceRuns.Count() : 0);
            string contentSpeed = "Normal";
            string renderSpeed = "Normal";
            string message;

            if ((languageItem.SentenceRuns != null) && (sentenceCount != 0))
            {
                int sentenceIndex = 0;

                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                {
                    string sentenceText = languageItem.GetRunText(sentenceRun);
                    MediaRun mediaRun = null;

                    if (speed == "Normal")
                        mediaRun = sentenceRun.GetMediaRun("Audio");
                    else if (speed == "Slow")
                    {
                        contentSpeed = "Slow";
                        mediaRun = sentenceRun.GetMediaRun("SlowAudio");

                        if (mediaRun == null)
                        {
                            mediaRun = sentenceRun.GetMediaRun("Audio");
                            renderSpeed = "Slow";
                        }
                    }

                    string text = languageItem.GetRunText(sentenceRun);

                    if (mediaRun != null)
                    {
                        if (mediaRun.IsReference)
                        {
                            string mediaItemKey = mediaRun.MediaItemKey;
                            string languageMediaItemKey = mediaRun.LanguageMediaItemKey;
                            ContentMediaItem mediaItem = Node.GetMediaItem(mediaItemKey);

                            if (mediaItem != null)
                            {
                                MediaDescription mediaDescription = mediaItem.GetMediaDescriptionIndexed(
                                    languageMediaItemKey, 0);

                                if (mediaDescription != null)
                                {
                                    string url = mediaDescription.GetUrl(mediaItem.MediaTildeUrl);

                                    if (String.IsNullOrEmpty(url))
                                    {
                                        HandleError(executionItems, "Media reference media run has no media reference: " + sentenceText);
                                        return false;
                                    }

                                    string filePath = ApplicationData.MapToFilePath(url);

                                    if (FileSingleton.Exists(filePath))
                                    {
                                        LastItemTime = mediaRun.Length;
                                        sentenceText = AddTextStyle(sentenceText, languageID);
                                        HandleTextSegmentOutput(
                                            sentenceText, url, mediaRun.Start, mediaRun.Length,
                                            speed, executionItems);
                                    }
                                    else
                                    {
                                        mediaRun = Renderer.CreateItemMediaRun(
                                            multiLanguageItem.KeyString, sentenceIndex,
                                            text, languageID, speakerNameKey, speed, mediaDirectoryUrl);
                                        sentenceRun.InsertMediaRun(0, mediaRun);

                                        url = mediaRun.GetUrl(mediaDirectoryUrl);

                                        if (mediaRun.Length == TimeSpan.Zero)
                                            mediaRun.Length = MediaUtilities.GetMediaUrlTimeSpan(url);

                                        LastItemTime = mediaRun.Length;

                                        if (!AppendFileAudio(speakerNameKey, sentenceText, languageID, url,
                                            contentSpeed, renderSpeed, out message, executionItems))
                                        {
                                            HandleError(executionItems, "Error appending item: " + message);
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    HandleError(executionItems, "Media item has no file: " + mediaItemKey + " " + languageMediaItemKey);
                                    return false;
                                }
                            }
                            else
                            {
                                HandleError(executionItems, "Media reference media item not found: " + mediaItemKey);
                                return false;
                            }
                        }
                        else
                        {
                            string url = mediaRun.GetUrl(mediaDirectoryUrl);

                            if (AppendFileAudio(speakerNameKey, sentenceText, languageID, url,
                                contentSpeed, renderSpeed, out message, executionItems))
                            {
                                if (mediaRun.Length == TimeSpan.Zero)
                                    mediaRun.Length = MediaUtilities.GetMediaUrlTimeSpan(url);

                                LastItemTime = mediaRun.Length;
                            }
                            else
                            {
                                HandleError(executionItems, "Error appending item: " + message);
                                return false;
                            }
                        }
                    }
                    else if (Renderer != null)
                    {
                        mediaRun = Renderer.CreateItemMediaRun(
                            multiLanguageItem.KeyString, sentenceIndex,
                            text, languageID, speakerNameKey, speed, mediaDirectoryUrl);
                        sentenceRun.AddMediaRun(mediaRun);

                        string url = mediaRun.GetUrl(mediaDirectoryUrl);

                        if (mediaRun.Length == TimeSpan.Zero)
                            mediaRun.Length = MediaUtilities.GetMediaUrlTimeSpan(url);

                        LastItemTime = mediaRun.Length;

                        if (!AppendFileAudio(speakerNameKey, sentenceText, languageID, url,
                            contentSpeed, renderSpeed, out message, executionItems))
                        {
                            HandleError(executionItems, "Error appending item: " + message);
                            return false;
                        }
                    }

                    sentenceIndex++;
                }
            }
            else if (Renderer != null)
            {
                string sentenceText = languageItem.Text;
                MediaRun mediaRun = Renderer.CreateItemMediaRun(
                    multiLanguageItem.KeyString, 0,
                    sentenceText, languageID, speakerNameKey, speed, mediaDirectoryUrl);
                if (mediaRun != null)
                {
                    TextRun sentenceRun = new TextRun(0, sentenceText.Length, null);
                    sentenceRun.AddMediaRun(mediaRun);
                    languageItem.AddSentenceRun(sentenceRun);

                    string url = mediaRun.GetUrl(mediaDirectoryUrl);

                    if (mediaRun.Length == TimeSpan.Zero)
                        mediaRun.Length = MediaUtilities.GetMediaUrlTimeSpan(url);

                    LastItemTime = mediaRun.Length;

                    if (!AppendFileAudio(speakerNameKey, sentenceText, languageID, url,
                        contentSpeed, renderSpeed, out message, executionItems))
                    {
                        HandleError(executionItems, "Error appending item: " + message);
                        return false;
                    }
                }
            }

            return true;
        }

        protected void HandleSentenceOutput(
            string sentenceText,
            string speakerKey,
            LanguageID languageID,
            string speed,
            List<AutomatedExecutionItem> executionItems)
        {
            MultiLanguageItem multiLanguageItem = null;
            LanguageItem languageItem = null;
            TextRun sentenceRun = null;
            MediaRun sentenceMediaRun = null;
            List<string> sentences;

            if (sentenceText.Contains("\n"))
                sentences = sentenceText.Split(AutomatedMarkupRenderer.newlines, StringSplitOptions.RemoveEmptyEntries).ToList();
            else
                sentences = new List<string>(1) { sentenceText };

            foreach (string str in sentences)
            {
                sentenceMediaRun = null;
                languageItem = null;
                sentenceRun = null;

                if (Renderer.MarkupTemplate != null)
                {
                    string variableName = Renderer.MarkupTemplate.FilterVariableName(str);
                    multiLanguageItem = Renderer.MarkupTemplate.MultiLanguageItem(variableName);
                }

                if (multiLanguageItem != null)
                {
                    languageItem = multiLanguageItem.LanguageItem(languageID);

                    if (languageItem != null)
                    {
                        if (languageItem.HasSentenceRuns())
                        {
                            sentenceRun = languageItem.SentenceRuns.First();
                            MediaRun mediaRun = sentenceRun.GetMediaRun("Audio");

                            if (mediaRun != null)
                            {
                                if (!mediaRun.IsReference)
                                    sentenceMediaRun = new MediaRun(mediaRun);
                            }
                        }
                    }
                }

                if (sentenceMediaRun == null)
                {
                    if (Renderer != null)
                        sentenceMediaRun = Renderer.CreateSentenceMediaRun(str, speakerKey, languageID, speed);

                    if (sentenceMediaRun == null)
                        return;

                    if (languageItem != null)
                    {
                        if (sentenceRun == null)
                        {
                            sentenceRun = new TextRun(languageItem, sentenceMediaRun);
                            languageItem.SentenceRuns = new List<TextRun>(1) { sentenceRun };
                        }
                        else
                            sentenceRun.MediaRuns = new List<MediaRun>(1) { sentenceMediaRun };
                    }
                }

                HandleMediaRun(sentenceText, sentenceMediaRun, languageID, speed, executionItems);
            }
        }

        protected void HandleMediaRun(
            string sentenceText,
            MediaRun sentenceMediaRun,
            LanguageID languageID,
            string speed,
            List<AutomatedExecutionItem> executionItems)
        {
            string fileUrl = sentenceMediaRun.GetUrl(GenerateMediaDirectoryUrl);
            sentenceText = AddTextStyle(sentenceText, languageID);
            HandleTextOutput(sentenceText, fileUrl, "Normal", executionItems);
        }

        protected string AddTextStyle(string text, LanguageID languageID)
        {
            LanguageDescriptor languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageDescriptor != null)
            {
                if (!String.IsNullOrEmpty(languageDescriptor.PreferredFontStyleTextStyle))
                    text = "fontStyle=" + languageDescriptor.PreferredFontStyleTextStyle + "|" + text;
            }

            return text;
        }

        protected void HandleTextOutput(
            string text,
            string audioUrl,
            string speed,
            List<AutomatedExecutionItem> executionItems)
        {
            audioUrl = MediaUtilities.GetContentUrl(audioUrl, String.Empty);

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        speed));

            executionItems.Add(
                new AutomatedExecutionItem(
                    "PlayAudio",
                    audioUrl));
            executionItems.Add(
                new AutomatedExecutionItem(
                    "AppendText",
                    text));

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        "Normal"));
        }

        protected void HandleTextSegmentOutput(
            string text,
            string audioUrl,
            TimeSpan startTime,
            TimeSpan lengthTime,
            string speed,
            List<AutomatedExecutionItem> executionItems)
        {
            audioUrl = MediaUtilities.GetContentUrl(audioUrl, String.Empty);

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        speed));

            executionItems.Add(
                new AutomatedExecutionItem(
                    "PlayAudioSegment",
                    audioUrl,
                    (int)startTime.TotalMilliseconds,
                    (int)(startTime.TotalMilliseconds + lengthTime.TotalMilliseconds)
                ));

            executionItems.Add(
                new AutomatedExecutionItem(
                    "AppendText",
                    text));

            if (speed != "Normal")
                executionItems.Add(
                    new AutomatedExecutionItem(
                        "SetMediaSpeed",
                        "Normal"));
        }

        protected virtual bool AppendFileAudio(
            string speakerNameKey,
            string sentenceText,
            LanguageID languageID,
            string fileUrl,
            string contentSpeed,
            string renderSpeed,
            out string message,
            List<AutomatedExecutionItem> executionItems)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            string mimeType = "audio/mpeg3";

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (Renderer.GetSpeedKeyFromString(contentSpeed) == "Slow")
                    MediaUtilities.ChangeFileExtension(filePath, "_slow" + MediaUtilities.GetFileExtension(filePath));

                Renderer.VoiceCheck(speakerNameKey, languageID, renderSpeed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, false);

                if (String.IsNullOrEmpty(sentenceText))
                    return true;

                if (!Renderer.SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }

                if (!FileSingleton.Exists(filePath))
                    return true;
            }

            sentenceText = AddTextStyle(sentenceText, languageID);

            HandleTextOutput(sentenceText, fileUrl, renderSpeed, executionItems);

            return true;
        }

        protected void HandlePushWorkingSet(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string contentKey = instruction.GetArgumentString("ContentKey");
            string tag = instruction.GetArgumentString("Tag");
            string label = instruction.GetArgumentString("Label");
            string selectorString = instruction.GetArgumentString("Selector");
            SelectorAlgorithmCode selector = 
                (!String.IsNullOrEmpty(selectorString) ?
                    ToolProfile.GetSelectorAlgorithmCodeFromString(selectorString) :
                    SelectorAlgorithmCode.Forward);
            string modeString = instruction.GetArgumentString("Mode");
            ToolSelectorMode mode =
                (!String.IsNullOrEmpty(modeString) ?
                    ToolItemSelector.GetToolSelectorModeFromString(modeString) :
                    ToolSelectorMode.Normal);
            int newLimit = instruction.GetArgumentInt("NewLimit");
            int reviewLimit = instruction.GetArgumentInt("ReviewLimit");
            bool isRandomUnique = instruction.GetArgumentFlag("IsRandomUnique");
            bool isRandomNew = instruction.GetArgumentFlag("IsRandomNew");
            bool isAdaptiveMixNew = instruction.GetArgumentFlag("IsAdaptiveMixNew");
            int chunkSize = instruction.GetArgumentInt("ChunkSize");
            int level = instruction.GetArgumentInt("Level");
            string profileName = instruction.GetArgumentString("ProfileName");
            string configuration = instruction.GetArgumentString("Configuration");
            ContentStudyList savedWorkingSetStudyList;
            ToolStudyList savedWorkingSet;

            SessionStack.Add(Session);

            PushWorkingSet(
                contentKey,
                tag,
                label,
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configuration,
                out savedWorkingSetStudyList,
                out savedWorkingSet);

            WorkingSetStudyListStack.Add(savedWorkingSetStudyList);
            WorkingSetStack.Add(WorkingSet);
            StudyItem = null;
            StudyItemIndex = -1;
        }

        protected void HandlePopWorkingSet(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            ContentStudyList savedWorkingSetStudyList = WorkingSetStudyListStack.Last();
            ToolStudyList savedWorkingSet = WorkingSetStack.Last();
            ToolSession savedSession = SessionStack.Last();

            WorkingSetStudyListStack.RemoveAt(WorkingSetStudyListStack.Count - 1);
            WorkingSetStack.RemoveAt(WorkingSetStack.Count - 1);
            SessionStack.RemoveAt(SessionStack.Count - 1);

            PopWorkingSet(savedWorkingSetStudyList, savedWorkingSet);

            Session = savedSession;
        }

        protected void HandlePushWorkingSetCount(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string name = instruction.GetArgumentString("Name");
            int value = 0;

            if (WorkingSet != null)
                value = WorkingSet.ToolStudyItemCount();

            PushVariable(name, value);
        }

        protected void HandlePushDurationTimes(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            TimeSpan startTime = ElapsedTimeSpan;
            TimeSpan duration = instruction.GetArgumentTimeSpan("Duration");
            PushVariable("startTime", startTime);
            PushVariable("duration", duration);
        }

        protected void HandlePushVariable(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string name = instruction.GetArgumentString("Name");
            object value = instruction.GetArgument("Value");
            PushVariable(name, value);
        }

        protected void HandlePopVariable(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string name = instruction.GetArgumentString("Name");
            PopVariable(name);
        }

        protected void HandleIncrementVariable(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            string name = instruction.GetArgumentString("Name");
            int value = GetIntegerVariable(name);
            SetVariable(name, value + 1);
        }

        protected int HandleIndexCountConditionalLoop(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems,
            int instructionIndex)
        {
            int index = GetIntegerVariable("index");
            int count = GetIntegerVariable("count");

            if (index >= count)
            {
                instructionIndex = instruction.GetArgumentInt("TargetLabel");
                return instructionIndex;
            }

            instructionIndex++;

            return instructionIndex;
        }

        protected int HandleDurationConditionalLoop(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems,
            int instructionIndex)
        {
            TimeSpan startTime = GetTimeSpanVariable("startTime");
            TimeSpan duration = GetTimeSpanVariable("duration");
            TimeSpan currentTime = ElapsedTimeSpan;
            TimeSpan endTime = startTime + duration;

            if (currentTime >= endTime)
            {
                instructionIndex = instruction.GetArgumentInt("TargetLabel");
                return instructionIndex;
            }

            instructionIndex++;

            return instructionIndex;
        }

        protected int HandleUnconditionalBranch(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItem)
        {
            int instructionIndex = instruction.GetArgumentInt("TargetLabel");
            return instructionIndex;
        }

        protected int HandleChoiceConditionalBranch(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItem,
            int instructionIndex)
        {
            string choiceID = instruction.GetArgumentString("ChoiceID");
            if (choiceID != GetStringVariable("ChoiceID"))
                instructionIndex = instruction.GetArgumentInt("TargetLabel");
            else
                instructionIndex++;
            return instructionIndex;
        }

        protected int HandleGoTo(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItem,
            int index)
        {
            WorkingSetStudyListStack.Clear();
            WorkingSetStack.Clear();
            SessionStack.Clear();

            string marker = instruction.GetArgumentString("Marker");
            int instructionIndex = GetMarkerTargetIndex(marker);
            if (instructionIndex < 0)
                instructionIndex = index + 1;
            return instructionIndex;
        }

        protected int HandleGetAndTouchItem(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems,
            int instructionIndex)
        {
            ToolStudyItem entry;
            ToolItemStatus status;

            StudyItemIndex = Selector.CurrentIndex;
            entry = WorkingSet.GetToolStudyItemIndexed(StudyItemIndex);

            if (entry == null)
            {
                instructionIndex = instruction.GetArgumentInt("TargetLabel");
                return instructionIndex;
            }

            StudyItem = entry.StudyItem;

            if (Configuration == null)
                SetUpDefaultConfiguration();

            status = entry.GetStatus(Configuration.Key);
            ToolProfile toolProfile = Configuration.Profile;
            toolProfile.TouchApplyGrade(
                status,
                1.0f,
                CurrentTime,
                Configuration);
            instructionIndex++;

            return instructionIndex;
        }

        protected int HandleNextItemAndBranch(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems,
            int instructionIndex)
        {
            if (IsInterrupt || !Selector.SetNextIndex())
            {
                IsInterrupt = false;
                instructionIndex = instruction.GetArgumentInt("TargetLabel");
                return instructionIndex;
            }

            instructionIndex++;

            return instructionIndex;
        }

        protected void HandlePause(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            double value = instruction.GetArgumentDouble("Value");
            double minimum = instruction.GetArgumentDouble("Minimum");
            executionItems.Add(
                new AutomatedExecutionItem(
                    "Pause",
                    instruction.GetArgumentString("Mode"),
                    (int)(value * 1000.0),
                    (int)(minimum * 1000.0)));
        }

        protected void HandleWait(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            executionItems.Add(
                new AutomatedExecutionItem("Wait"));
        }

        protected bool HandleWaitForDone(
            AutomatedInstruction instruction,
            List<AutomatedExecutionItem> executionItems)
        {
            if (IsWaitingForDone)
                executionItems.Add(
                    new AutomatedExecutionItem("WaitForDone"));
            return IsWaitingForDone;
        }

        protected void HandleError(
            List<AutomatedExecutionItem> executionItems,
            string message)
        {
            executionItems.Add(
                new AutomatedExecutionItem("Error", message));
        }

        protected void PushWorkingSet(
            string contentKey,
            string tag,
            string label,
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey,
            out ContentStudyList savedWorkingSetStudyList,
            out ToolStudyList savedWorkingSet)
        {
            List<BaseObjectTitled> sources = null;

            savedWorkingSetStudyList = WorkingSetStudyList;
            savedWorkingSet = WorkingSet;

            if ((Node == null) || (String.IsNullOrEmpty(contentKey) && String.IsNullOrEmpty(tag) && String.IsNullOrEmpty(label)))
            {
                if (WorkingSet != null)
                {
                    SetupWorkingSetSelector(
                        selector,
                        mode,
                        newLimit,
                        reviewLimit,
                        isRandomUnique,
                        isRandomNew,
                        isAdaptiveMixNew,
                        chunkSize,
                        level,
                        profileName,
                        ref configurationKey);
                }
                else
                    LoadWorkingSet(
                        null,
                        null,
                        null,
                        selector,
                        mode,
                        newLimit,
                        reviewLimit,
                        isRandomUnique,
                        isRandomNew,
                        isAdaptiveMixNew,
                        chunkSize,
                        level,
                        profileName,
                        ref configurationKey);

                return;
            }

            SetupWorkingSet(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);

            if (!String.IsNullOrEmpty(contentKey))
            {
                if ((SourceContents != null) && (SourceContents.Count() != 0))
                    sources = SourceContents.Cast<BaseObjectTitled>().ToList();
                else if (SourceNode != null)
                    sources = new List<BaseObjectTitled>() { SourceNode };
                else if (Content != null)
                    sources = new List<BaseObjectTitled>() { Content };
                else if (Node != null)
                    sources = new List<BaseObjectTitled>() { Node };
                else if (DocumentContent != null)
                    sources = new List<BaseObjectTitled>() { DocumentContent };

                StudyList = null;

                foreach (BaseObjectTitled source in sources)
                {
                    if (source is BaseObjectNode)
                    {
                        Node = (BaseObjectNode)source;
                        Content = Node.GetContent(contentKey);

                        if (Content == null)
                            Content = Node.GetFirstContentWithType(contentKey);

                        if (Content != null)
                            StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                        else
                            StudyList = null;

                        if (StudyList != null)
                            LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                    }
                    else if ((source is BaseObjectContent)
                        && (((source as BaseObjectContent).KeyString == contentKey) || ((source as BaseObjectContent).ContentType == contentKey)))
                    {
                        StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                        if (StudyList != null)
                            LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                    }
                }

                if ((StudyList == null) && (Node != null))
                {
                    Content = Node.GetContent(contentKey);

                    if (Content == null)
                        Content = Node.GetFirstContentWithType(contentKey);

                    if (Content != null)
                        StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                    else
                        StudyList = null;

                    if (StudyList != null)
                        LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                }
            }
            else
                LoadWorkingSetStudyItems(savedWorkingSetStudyList, tag, label, configurationKey);

            Selector.Reset();
        }

        protected void PopWorkingSet(ContentStudyList savedWorkingSetStudyList, ToolStudyList savedWorkingSet)
        {
            if (WorkingSet == savedWorkingSet)
            {
                if (Selector != null)
                    Selector.Reset();
            }
            else
                WorkingSet = savedWorkingSet;

            WorkingSetStudyList = savedWorkingSetStudyList;
        }

        protected void LoadWorkingSet(
            string contentKey,
            string tag,
            string label,
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey)
        {
            List<BaseObjectTitled> sources = null;

            if ((SourceContents != null) && (SourceContents.Count() != 0))
                sources = SourceContents.Cast<BaseObjectTitled>().ToList();
            else if (SourceNode != null)
                sources = new List<BaseObjectTitled>() { SourceNode };
            else if (Content != null)
                sources = new List<BaseObjectTitled>() { Content };
            else if (Node != null)
                sources = new List<BaseObjectTitled>() { Node };
            else if (DocumentContent != null)
                sources = new List<BaseObjectTitled>() { DocumentContent };

            SetupWorkingSet(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);

            if (sources != null)
            {
                if (!String.IsNullOrEmpty(contentKey))
                {
                    foreach (BaseObjectTitled source in sources)
                    {
                        if (source is BaseObjectNode)
                        {
                            Node = (BaseObjectNode)source;
                            Content = Node.GetFirstContentWithType(contentKey);

                            if (Content != null)
                                StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                            else
                                StudyList = null;

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                        else if ((source is BaseObjectContent) && ((source as BaseObjectContent).ContentType == contentKey))
                        {
                            StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                    }
                }
                else
                {
                    foreach (BaseObjectTitled source in sources)
                    {
                        if (source is BaseObjectNode)
                        {
                            Node = (BaseObjectNode)source;

                            int contentCount = Node.ContentCount();

                            for (int contentIndex = 0; contentIndex < contentCount; contentIndex++)
                            {
                                Content = Node.GetContentIndexed(contentIndex);

                                if (Content != null)
                                    StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                                else
                                    StudyList = null;

                                if (StudyList != null)
                                    LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                            }
                        }
                        else if (source is BaseObjectContent)
                        {
                            StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                    }
                }
            }

            Selector.Reset();
        }

        protected void LoadWorkingSetStudyItems(ContentStudyList studyList, string tag, string label,
            string configurationKey)
        {
            int count = studyList.StudyItemCount();
            int index = 0;
            int startIndex;
            int endIndex;

            if (!String.IsNullOrEmpty(tag) && studyList.GetTaggedStudyItemRange(tag, out startIndex, out endIndex))
            {
                index = startIndex;
                count = endIndex;
            }

            if (!String.IsNullOrEmpty(label) && studyList.GetLabeledStudyItemRange(label, UILanguageID, out startIndex, out endIndex))
            {
                index = startIndex;
                count = endIndex;
            }

            for (; index < count; index++)
            {
                MultiLanguageItem multiLanguageItem = studyList.GetStudyItemIndexed(index);
                string sourceItemKey = multiLanguageItem.KeyString;
                BaseObjectContent sourceContent = multiLanguageItem.Content;
                ContentStudyList sourceStudyList = sourceContent.ContentStorageStudyList;
                object sourceNodeKey = sourceContent.Node.Key;
                object sourceStudyListKey = sourceStudyList.Key;
                string sourceContentKey = sourceContent.KeyString;
                string key = WorkingSetStudyList.AllocateStudyItemKey();
                MultiLanguageItem workingSetItem = new MultiLanguageItem(
                    key,
                    multiLanguageItem.LanguageItems,
                    multiLanguageItem.SpeakerNameKey,
                    null,
                    null,
                    studyList);
                WorkingSetStudyList.AddStudyItem(workingSetItem);
                ToolStudyItem toolStudyItem = WorkingSet.AddStudyItem(workingSetItem);
                workingSetItem.Content = multiLanguageItem.Content;
                workingSetItem.ItemSource = new MultiLanguageItemReference(
                    sourceItemKey, sourceStudyListKey, sourceContentKey, sourceNodeKey,
                    multiLanguageItem);
                string toolStudyListKey = ToolUtilities.ComposeToolStudyListKey(UserRecord, sourceContent, ToolSourceCode.Unknown);
                ToolStudyList sourceToolStudyList = ToolStudyLists.FirstOrDefault(x => x.KeyString == toolStudyListKey);
                if (sourceToolStudyList == null)
                {
                    sourceToolStudyList = Repositories.ToolStudyLists.Get(toolStudyListKey);
                    if (sourceToolStudyList == null)
                        sourceToolStudyList = CreateToolStudyList(sourceContent);
                    else
                        sourceToolStudyList.ResolveToolStudyItems(
                            studyList,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null);
                    ToolStudyLists.Add(sourceToolStudyList);
                }
                ToolStudyItem sourceToolStudyItem = sourceToolStudyList.GetToolStudyItem(sourceItemKey);
                if (sourceToolStudyItem != null)
                {
                    ToolItemStatus souceToolItemStatus = sourceToolStudyItem.GetStatus(configurationKey);
                    toolStudyItem.SetStatus(souceToolItemStatus);
                }
            }
        }

        protected ToolStudyList CreateToolStudyList(BaseObjectContent content)
        {
            ToolStudyList toolStudyList = new ToolStudyList(
                UserRecord,
                content,
                ToolSourceCode.StudyList,
                null,
                null,
                null);

            toolStudyList.EnsureGuid();
            toolStudyList.TouchAndClearModified();

            try
            {
                if (!Repositories.ToolStudyLists.Add(toolStudyList))
                    Error = S("Error adding tool study list.");
            }
            catch (Exception exception)
            {
                Error = S("Error adding tool study list") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error += ": " + exception.InnerException.Message;
            }

            return toolStudyList;
        }

        protected void SetupWorkingSet(
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey)
        {
            WorkingSetStudyList = new ContentStudyList();
            WorkingSet = new ToolStudyList();
            WorkingSet.CopyLanguagesFromLanguageDescriptors(LanguageDescriptors);
            SetupWorkingSetSelector(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);
        }

        protected void SetupWorkingSetSelector(
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey)
        {
            List<ToolProfile> profiles = null;
            ToolProfile profile = null;
            string profileKey = ToolUtilities.ComposeToolProfileKey(UserRecord, profileName);

            if (profileName != "Markup")
            {
                profiles = ToolUtilities.GetToolProfiles();
                profile = profiles.FirstOrDefault(x => x.MatchKey(profileKey));

                if (profile == null)
                {
                    string defaultProfileKey = ToolUtilities.ComposeToolProfileDefaultKey(UserRecord);
                    ToolProfile defaultProfile = Repositories.ToolProfiles.Get(defaultProfileKey);
                    profile = ToolUtilities.CreateAndAddToolProfile(profileName, profiles.Count,
                        defaultProfile, out Message);
                    profiles.Add(profile);
                }
            }

            if (profile == null)
            {
                profile = new ToolProfile(
                    profileKey,                         // key
                    null,                               // MultiLanguageString title,
                    null,                               // MultiLanguageString description,
                    null,                               // string source,
                    null,                               // string package,
                    null,                               // string label,
                    null,                               // string imageFileName,
                    0,                                  // int index,
                    true,                               // bool isPublic,
                    WorkingSet.TargetLanguageIDs,       // List<LanguageID> targetLanguageIDs,
                    WorkingSet.HostLanguageIDs,         // List<LanguageID> hostLanguageIDs,
                    UserRecord.UserName,                // string owner,
                    ToolProfile.DefaultGradeCount,      // int gradeCount,
                    selector,                           // SelectorAlgorithmCode selectorMode,
                    newLimit,                           // int newLimit,
                    reviewLimit,                        // int reviewLimit,
                    isRandomUnique,                     // bool isRandomUnique,
                    isRandomNew,                        // bool isRandomNew,
                    isAdaptiveMixNew,                   // bool isAdaptiveMixNew,
                    level,                              // int reviewLevel,
                    ToolProfile.DefaultChoiceSize,      // int choiceSize,
                    chunkSize,                          // int chunkSize,
                    ToolProfile.DefaultHistorySize,     // int historySize,
                    ToolProfile.DefaultIsShowIndex,     // bool isShowIndex,
                    ToolProfile.DefaultIsShowOrdinal,   // bool isShowOrdinal,
                    ToolProfile.DefaultSpacedIntervalTable,  // List<TimeSpan> intervalTable,
                    null,                               // string fontFamily,
                    null,                               // string flashFontSize,
                    null,                               // string listFontSize,
                    ToolProfile.DefaultMaximumLineLength, // int maximumLineLength,
                    new List<ToolConfiguration>());     // List<ToolConfiguration> toolConfigurations);
                profiles = new List<ToolProfile>() { profile };
                string specs = UserProfile.GetUserOptionString(
                    "DefaultToolConfigurations",
                    ToolUtilities.DefaultToolConfigurationSpecifications);
                string message;
                ToolUtilities.AddToolProfileConfigurations(profile, specs, out message);
            }

            SetUpConfiguration(profile, ref configurationKey);
            Selector = new ToolItemSelector();
            Session = new ToolSession(
                null,
                profiles,
                profileKey,
                configurationKey,
                WorkingSet,
                Selector);
            Session.ToolType = ToolTypeCode.Flash;
            Selector.IsCustomTime = true;
            Session.SessionStart = StartTime;
            Selector.CustomNowTime = CurrentTime;
            Selector.Mode = mode;
            Selector.Reset();
        }

        public void SetUpConfiguration(ToolProfile profile, ref string configurationKey)
        {
            ToolConfiguration configuration = profile.GetToolConfiguration(configurationKey);
            if (configuration == null)
            {
                configuration = profile.GetToolConfigurationFuzzy(configurationKey, UILanguageID);
                if (configuration != null)
                    configurationKey = configuration.KeyString;
            }
            if (configuration == null)
            {
                Configuration = new ToolConfiguration(profile, configurationKey, null, null, configurationKey, 0, null);
                profile.AddToolConfiguration(Configuration);
            }
            else
                Configuration = configuration;
        }

        public void SetUpDefaultConfiguration()
        {
            ToolProfile profile = Session.ToolProfile;
            SetUpConfiguration(profile, ref DefaultConfigurationName);
        }

        public bool ValidateSelector(string selector)
        {
            try
            {
                ToolProfile.GetSelectorAlgorithmCodeFromString(selector);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void CacheUpdateStudyList(ContentStudyList studyList)
        {
            if (studyList == null)
                return;

            if (StudyListUpdateCache == null)
                StudyListUpdateCache = new List<ContentStudyList>();

            if (!StudyListUpdateCache.Contains(studyList))
                StudyListUpdateCache.Add(studyList);
        }

        protected void UpdateStudyLists(List<AutomatedExecutionItem> executionItems)
        {
            if ((StudyListUpdateCache == null) || (StudyListUpdateCache.Count == 0))
                return;

            foreach (ContentStudyList studyList in StudyListUpdateCache)
            {
                if (!studyList.Modified)
                    continue;

                studyList.TouchAndClearModified();

                if (!Repositories.StudyLists.Update(studyList))
                    HandleError(executionItems, "Error updating study list: " + studyList.Content.GetTitleString());
            }
        }

        protected void UpdateToolStudyLists(List<AutomatedExecutionItem> executionItems)
        {
            if ((ToolStudyLists == null) || (ToolStudyLists.Count == 0))
                return;

            foreach (ToolStudyList toolStudyList in ToolStudyLists)
            {
                if (!toolStudyList.Modified)
                    continue;

                toolStudyList.TouchAndClearModified();

                if (!Repositories.ToolStudyLists.Update(toolStudyList))
                    HandleError(executionItems, "Error updating tool study list.");
            }
        }

        public void ForgetAll()
        {
            if ((SourceContents != null) && (SourceContents.Count() != 0))
            {
                foreach (BaseObjectContent content in SourceContents)
                {
                    ContentStudyList studyList = content.ContentStorageStudyList;
                    string toolStudyListKey = ToolUtilities.ComposeToolStudyListKey(UserRecord, content, ToolSourceCode.Unknown);
                    ToolStudyList toolStudyList = Repositories.ToolStudyLists.Get(toolStudyListKey);
                    if (toolStudyList != null)
                    {
                        toolStudyList.ResolveToolStudyItems(
                            studyList,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null);
                        toolStudyList.ForgetAll();
                        toolStudyList.TouchAndClearModified();

                        if (!Repositories.ToolStudyLists.Update(toolStudyList))
                            HandleError(null, "Error updating tool study list.");
                    }
                }
            }

            if (Session != null)
                Session.ForgetAll(true);
        }
    }
}

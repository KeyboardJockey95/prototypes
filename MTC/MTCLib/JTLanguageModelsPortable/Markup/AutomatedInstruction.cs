using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Markup
{
    public enum AutomatedInstructionCommand
    {
        Nop,                        // Do nothing.
        View,                       // Output a view.
        StudyItem,                  // Output a study item.
        Choose,                     // Output a choose view.
        StudyItemTimeTextMap,       // Get study item times map.
        PlayMedia,                  // Output media.
        Label,                      // Output label text.
        Text,                       // Output some generic text and audio.
        TextSegment,                // Output text and audio segment.
        Say,                        // Say something from a field.
        Item,                       // Handle study item.
        PushWorkingSet,             // Push working set.
        PopWorkingSet,              // Pop working set.
        PushWorkingSetCount,        // Push working set count variable.
        PushDurationTimes,          // Push duration times.
        GetAndTouchItem,            // Get and touch current item, branch if error.
        NextItemAndBranch,          // Set next item and branch if done.
        PushVariable,               // Push variable.
        PopVariable,                // Pop variable.
        IncrementVariable,          // increment integer variable.
        IndexCountConditionalLoop,  // "if (index >= count) goto Label".
        DurationConditionalLoop,    // "if (currentTime >= endTime) goto Label".
        UnconditionalBranch,        // Unconditional branch.
        ChoiceConditionalBranch,    // "if (choicID)".
        GoTo,                       // Go to marker.
        Pause,                      // Pause for a period.
        Wait,                       // Wait for audio to finish.
        WaitForDone,                // Wait for done command.
        Stop                        // Stop execution.
    }

    public enum AutomatedContinueMode
    {
        None = 0,                   // 0 = None
        WaitForDone = 1,            // 1 = WaitForDone
        WaitForUserCommand = 2,     // 2 = WaitForUserCommand
        WaitForTime = 3,            // 3 = WaitForTime
        WaitForEndOfMedia = 4,      // 4 = WaitForEndOfMedia
        WaitForEndOfStudyItem = 5   // 5 = WaitForEndOfStudyItem
    }

    public class AutomatedInstruction
    {
        public AutomatedInstructionCommand Command { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
        public int Label { get; set; }

        public AutomatedInstruction(
            AutomatedInstructionCommand command,
            Dictionary<string, object> arguments)
        {
            Command = command;
            Arguments = arguments;
            Label = -1;
        }

        public AutomatedInstruction(AutomatedInstructionCommand command)
        {
            Command = command;
            Arguments = null;
            Label = -1;
        }

        public string GetArgumentString(string argumentName)
        {
            if (Arguments == null)
                return String.Empty;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (string)value;

            return String.Empty;
        }

        public int GetArgumentInt(string argumentName)
        {
            if (Arguments == null)
                return -1;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (int)value;

            return -1;
        }

        public double GetArgumentDouble(string argumentName)
        {
            if (Arguments == null)
                return -1;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (double)value;

            return 0.0;
        }

        public bool GetArgumentFlag(string argumentName)
        {
            if (Arguments == null)
                return false;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (bool)value;

            return false;
        }

        public TimeSpan GetArgumentTimeSpan(string argumentName)
        {
            if (Arguments == null)
                return TimeSpan.Zero;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (TimeSpan)value;

            return TimeSpan.Zero;
        }

        public LanguageID GetArgumentLanguageID(string argumentName)
        {
            if (Arguments == null)
                return null;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return (LanguageID)value;

            return null;
        }

        public object GetArgument(string argumentName)
        {
            if (Arguments == null)
                return String.Empty;

            object value = null;

            if (Arguments.TryGetValue(argumentName, out value))
                return value;

            return null;
        }

        public void SetArgument(string argumentName, object value)
        {
            if (Arguments == null)
                return;

            Arguments[argumentName] = value;
        }
    }
}

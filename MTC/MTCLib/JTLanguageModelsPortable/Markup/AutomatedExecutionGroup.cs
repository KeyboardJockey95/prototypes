using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Markup
{
    public class AutomatedExecutionGroup
    {
        public int CommandCount;
        public AutomatedExecutionItem[] CommandItems;
        public int NextInstructionIndex;
        public bool IsLastCommand;

        public AutomatedExecutionGroup(
            int commandCount,
            AutomatedExecutionItem[] commandItems,
            int nextInstructionIndex,
            bool isLastCommand)
        {
            CommandCount = commandCount;
            CommandItems = commandItems;
            NextInstructionIndex = nextInstructionIndex;
            IsLastCommand = isLastCommand;
        }

        public AutomatedExecutionGroup()
        {
            CommandCount = 0;
            CommandItems = null;
            NextInstructionIndex = 0;
            IsLastCommand = false;
        }
    }
}

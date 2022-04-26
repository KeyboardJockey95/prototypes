using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Markup
{
    public class AutomatedExecutionItem
    {
        public string Command;
        public string StringValue;
        public int IntValue;
        public int ExtraIntValue;

        public AutomatedExecutionItem(
            string command,
            string stringValue,
            int intValue,
            int extraIntValue)
        {
            Command = command;
            StringValue = stringValue;
            IntValue = intValue;
            ExtraIntValue = extraIntValue;
        }

        public AutomatedExecutionItem(
            string command,
            string stringValue,
            int intValue)
        {
            Command = command;
            StringValue = stringValue;
            IntValue = intValue;
            ExtraIntValue = 0;
        }

        public AutomatedExecutionItem(
            string command,
            string stringValue)
        {
            Command = command;
            StringValue = stringValue;
            IntValue = 0;
            ExtraIntValue = 0;
        }

        public AutomatedExecutionItem(
            string command,
            int intValue)
        {
            Command = command;
            StringValue = String.Empty;
            IntValue = intValue;
            ExtraIntValue = 0;
        }

        public AutomatedExecutionItem(
            string command,
            int intValue,
            int extraIntValue)
        {
            Command = command;
            StringValue = String.Empty;
            IntValue = intValue;
            ExtraIntValue = extraIntValue;
        }

        public AutomatedExecutionItem(string command)
        {
            Command = command;
            StringValue = String.Empty;
            IntValue = 0;
            ExtraIntValue = 0;
        }

        public AutomatedExecutionItem()
        {
            Command = String.Empty;
            StringValue = String.Empty;
            IntValue = 0;
            ExtraIntValue = 0;
        }
    }
}

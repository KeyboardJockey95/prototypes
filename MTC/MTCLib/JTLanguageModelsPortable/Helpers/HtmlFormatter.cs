using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Helpers
{
    public class HtmlFormatter
    {
        private string Input;
        private StringBuilder OutputSB;
        private int Level;
        private int Index;
        private int Length;
        private string Newline;
        private string Indent;
        private enum StateCode
        {
            InOuter,
            InBeginTag,
            InEndTag,
            InInner,
            InString,
            Error
        };
        private StateCode State;
        private List<StateCode> StateStack;

        public HtmlFormatter()
        {
            Indent = "    ";
        }

        public string Beautify(string html)
        {
            if (String.IsNullOrEmpty(html))
                return html;

            Input = html;
            OutputSB = new StringBuilder();
            Level = 0;
            Index = 0;
            Length = Input.Length;
            Newline = (html.Contains("\r\n") ? "\r\n" : "\n");
            if (html.Contains("\r\n"))
                Newline = "\r\n";
            else if (html.Contains("\n"))
                Newline = "\n";
            else
                Newline = "\r\n";
            State = StateCode.InOuter;
            StateStack = new List<StateCode>();

            for (Index = 0; Index < Length; )
            {
                switch (State)
                {
                    case StateCode.InOuter:
                    case StateCode.InInner:
                        LexOuterOrInner();
                        break;
                    case StateCode.InBeginTag:
                        LexBeginTag();
                        break;
                    case StateCode.InEndTag:
                        LexEndTag();
                        break;
                    case StateCode.InString:
                        LexString();
                        break;
                    case StateCode.Error:
                        break;
                    default:
                        break;
                }
            }

            string returnValue = OutputSB.ToString().Trim() + Newline;
            return returnValue;
        }

        private void LexOuterOrInner()
        {
            char c = Input[Index];
            char nc;
            int nextIndex = Index + 1;

            switch (c)
            {
                case '<':
                    while ((nextIndex < Length) && IsSpace(Input[nextIndex]))
                        nextIndex++;
                    nc = Input[nextIndex];
                    if (nc == '/')
                    {
                        if (Level > 0)
                            Level--;
                        OutputSB.Append(Newline);
                        for (int index = 0; index < Level; index++)
                            OutputSB.Append(Indent);
                        OutputSB.Append("</");
                        nextIndex++;
                        while ((nextIndex < Length) && IsSpace(Input[nextIndex]))
                            nextIndex++;
                        Index = nextIndex;
                        PushState(StateCode.InEndTag);
                        break;
                    }
                    OutputSB.Append(Newline);
                    for (int index = 0; index < Level; index++)
                        OutputSB.Append(Indent);
                    Level++;
                    OutputSB.Append(c);
                    Index = nextIndex;
                    State = StateCode.InBeginTag;
                    break;
                case '"':
                    OutputSB.Append(c);
                    PushState(StateCode.InString);
                    Index++;
                    break;
                case '\r':
                    Index++;
                    break;
                case '\n':
                    Index++;
                    while ((nextIndex < Length) && IsSpace(Input[nextIndex]))
                        nextIndex++;
                    if ((nextIndex < Length) && (Input[nextIndex] != '<'))
                    {
                        OutputSB.Append(Newline);
                        for (int index = 0; index < Level; index++)
                            OutputSB.Append(Indent);
                    }
                    Index = nextIndex;
                    break;
                default:
                    if (IsSpace(c))
                    {
                        while ((nextIndex < Length) && IsWhiteSpace(Input[nextIndex]))
                            nextIndex++;
                        if (Input[nextIndex] == '<')
                        {
                            Index = nextIndex;
                            break;
                        }
                    }
                    OutputSB.Append(c);
                    Index++;
                    break;
            }
        }

        private void LexBeginTag()
        {
            char c = Input[Index];
            char nc;
            int nextIndex = Index + 1;

            switch (c)
            {
                case '>':
                    OutputSB.Append(c);
                    while ((nextIndex < Length) && IsWhiteSpace(Input[nextIndex]))
                        nextIndex++;
                    Index = nextIndex;
                    if ((nextIndex < Length) && (Input[nextIndex] != '<'))
                    {
                        OutputSB.Append(Newline);
                        for (int index = 0; index < Level; index++)
                            OutputSB.Append(Indent);
                    }
                    PopState();
                    PushState(StateCode.InInner);
                    break;
                case '/':
                    while ((nextIndex < Length) && IsSpace(Input[nextIndex]))
                        nextIndex++;
                    nc = Input[nextIndex];
                    if (nc == '>')
                    {
                        OutputSB.Append("/>");
                        Index = nextIndex + 1;
                        PushState(StateCode.InInner);
                        break;
                    }
                    OutputSB.Append(c);
                    Index++;
                    break;
                case '"':
                    OutputSB.Append(c);
                    PushState(StateCode.InString);
                    Index++;
                    break;
                default:
                    OutputSB.Append(c);
                    Index++;
                    break;
            }
        }

        private void LexEndTag()
        {
            char c = Input[Index];
            int nextIndex = Index + 1;

            switch (c)
            {
                case '>':
                    OutputSB.Append(c);
                    while ((nextIndex < Length) && IsWhiteSpace(Input[nextIndex]))
                        nextIndex++;
                    Index = nextIndex;
                    if ((nextIndex < Length) && (Input[nextIndex] != '<'))
                    {
                        OutputSB.Append(Newline);
                        for (int index = 0; index < Level; index++)
                            OutputSB.Append(Indent);
                        if (Level > 0)
                            Level--;
                    }
                    PopState();
                    break;
                default:
                    if (!IsSpace(c))
                        OutputSB.Append(c);
                    Index++;
                    break;
            }
        }

        private void LexString()
        {
            char c = Input[Index];

            switch (c)
            {
                case '"':
                    OutputSB.Append(c);
                    Index++;
                    PopState();
                    break;
                case '\\':
                    OutputSB.Append(c);
                    Index++;
                    c = Input[Index];
                    OutputSB.Append(c);
                    Index++;
                    break;
                default:
                    OutputSB.Append(c);
                    Index++;
                    break;
            }
        }

        private void PushState(StateCode state)
        {
            StateStack.Add(State);
            State = state;
        }

        private void PopState()
        {
            if (StateStack.Count() != 0)
            {
                State = StateStack.Last();
                StateStack.RemoveAt(StateStack.Count() - 1);
            }
        }

        private StateCode PeekState()
        {
            if (StateStack.Count() != 0)
                return StateStack.Last();
            return StateCode.Error;
        }

        private bool IsSpace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                    return true;
                case '\r':
                case '\n':
                    return false;
                default:
                    // In case character is some foreign language kind of space.
                    if (char.IsWhiteSpace(c))
                        return true;
                    return false;
            }
        }

        private bool IsWhiteSpace(char c)
        {
            if (char.IsWhiteSpace(c))
                return true;
            return false;
        }

        private static string[] SomeHtmlTags = new string[]
        {
            "<html>",
            "<p>",
            "</p>",
            "<div>",
            "</div>"
        };

        public static bool IsHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            bool returnValue = false;

            foreach (string tag in SomeHtmlTags)
            {
                if (text.Contains(tag))
                {
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }
    }
}

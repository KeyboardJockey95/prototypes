using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatSubtitleItem
    {
        public FormatSubtitleItem(
            int entryNumber,
            TimeSpan startTime,
            TimeSpan endTime,
            List<string> text)
        {
            EntryNumber = entryNumber;
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public FormatSubtitleItem(FormatSubtitleItem other)
        {
            EntryNumber = other.EntryNumber;
            StartTime = other.StartTime;
            EndTime = other.EndTime;
            Text = other.Text;
        }

        public FormatSubtitleItem()
        {
            EntryNumber = 0;
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;
            Text = new List<string>();
        }

        public int EntryNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<string> Text { get; set; }

        public string Line(int index)
        {
            if ((index >= 0) && (index < Text.Count()))
                return Text.First();

            return String.Empty;
        }

        public string FirstLine
        {
            get
            {
                if (Text.Count() != 0)
                    return Text.First();

                return String.Empty;
            }
        }

        public string LastLine
        {
            get
            {
                if (Text.Count() != 0)
                    return Text.Last();

                return String.Empty;
            }
        }

        public bool HasLine(string line)
        {
            if (Text.Contains(line))
                return true;

            return false;
        }

        public void AddLine(string line)
        {
            Text.Add(line);
        }

        public void DeleteLine(int index)
        {
            if ((index >= 0) && (index < Text.Count()))
                Text.RemoveAt(index);
        }

        public void DeleteAllButFirstLine()
        {
            while (Text.Count() > 1)
                DeleteLastLine();
        }

        public void DeleteFirstLine()
        {
            if (Text.Count() != 0)
                Text.RemoveAt(0);
        }

        public void DeleteLastLine()
        {
            if (Text.Count() != 0)
                Text.RemoveAt(Text.Count() - 1);
        }

        public void DeleteOverlappingLines()
        {
            for (int index = Text.Count() - 1; index > 0; index--)
            {
                if (Text[index].StartsWith(Text[index - 1]))
                    DeleteLine(index - 1);
                else if (Text[index - 1].StartsWith(Text[index]))
                    DeleteLine(index);
            }
        }

        public int LineCount
        {
            get
            {
                return Text.Count();
            }
        }
    }
}

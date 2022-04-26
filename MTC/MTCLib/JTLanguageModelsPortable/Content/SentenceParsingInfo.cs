using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Content
{
    public class SentenceParsingInfo
    {
        public int TargetSentenceStart { get; set; }
        public int HostSentenceStart { get; set; }
        public int TargetSentenceLength { get; set; }
        public int HostSentenceLength { get; set; }
        public int TargetSentenceStop
        {
            get
            {
                return TargetSentenceStart + TargetSentenceLength;
            }
            set
            {
                TargetSentenceLength = value - TargetSentenceStart;
            }
        }
        public int HostSentenceStop
        {
            get
            {
                return HostSentenceStart + HostSentenceLength;
            }
            set
            {
                HostSentenceLength = value - HostSentenceStart;
            }
        }
        public int TargetParagraphLength { get; set; }
        public int HostParagraphLength { get; set; }
        public string ParagraphDelimiters { get; set; }
        public int TargetParagraphDelimiterCount { get; set; }
        public int HostParagraphDelimiterCount { get; set; }
        public string TargetUnusedSentenceDelimiters { get; set; }
        public string HostUnusedSentenceDelimiters { get; set; }
        public int TargetUnusedSentenceDelimiterCount { get; set; }
        public int HostUnusedSentenceDelimiterCount { get; set; }
        public int DelimiterDifference { get; set; }
        public int TargetParagraphWordCount { get; set; }
        public int HostParagraphWordCount { get; set; }
        public int TargetSentenceWordCount { get; set; }
        public int HostSentenceWordCount { get; set; }
        public double TargetHostParagraphRatio { get; set; }
        public double TargetHostSentenceRatio { get; set; }
        public double TargetSentenceRatio { get; set; }
        public double HostSentenceRatio { get; set; }
        public double TargetHostSentenceRatioRatio { get; set; }
        public double HostTargetSentenceRatioRatio { get; set; }
        public bool TargetSentenceCollapsed { get; set; }
        public bool HostSentenceCollapsed { get; set; }
        public bool SentenceFailed { get; set; }

        public SentenceParsingInfo()
        {
            TargetSentenceStart = 0;
            HostSentenceStart = 0;
            TargetSentenceLength = 0;
            HostSentenceLength = 0;
            //TargetSentenceStop = 0;
            //HostSentenceStop = 0;
            TargetParagraphLength = 0;
            HostParagraphLength = 0;
            ParagraphDelimiters = String.Empty;
            TargetParagraphDelimiterCount = 0;
            HostParagraphDelimiterCount = 0;
            TargetUnusedSentenceDelimiters = String.Empty;
            HostUnusedSentenceDelimiters = String.Empty;
            TargetUnusedSentenceDelimiterCount = 0;
            HostUnusedSentenceDelimiterCount = 0;
            DelimiterDifference = 0;
            TargetParagraphWordCount = 0;
            HostParagraphWordCount = 0;
            TargetSentenceWordCount = 0;
            HostSentenceWordCount = 0;
            TargetHostParagraphRatio = 0.0;
            TargetHostSentenceRatio = 0.0;
            TargetSentenceRatio = 0.0;
            HostSentenceRatio = 0.0;
            TargetHostSentenceRatioRatio = 0.0;
            HostTargetSentenceRatioRatio = 0.0;
            TargetSentenceCollapsed = false;
            HostSentenceCollapsed = false;
            SentenceFailed = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Language
{
    public class SentenceMapping
    {
        // Sentence index.
        public int OriginalSentenceIndex { get; set; }

        // Original sentence run.
        public MappingSentenceRun OriginalSentence { get; set; }

        // Transcribed sentence run.
        public MappingSentenceRun TranscribedSentence { get; set; }

        // Original extra sentence run.
        public MappingSentenceRun OriginalExtraSentence { get; set; }

        // Transcribed extra sentence run.
        public MappingSentenceRun TranscribedExtraSentence { get; set; }

        // The main sentence score.
        public double MainScore { get; set; }

        // The extra sentence score.
        public double ExtraScore { get; set; }

        // The combined sentence score.
        public double CombinedScore { get; set; }

        // Edit distance of original and transcribed raw sentences.
        public int EditDistanceRaw { get; set; }

        // Edit distance of original and transcribed processed sentences.
        public int EditDistanceProcessed { get; set; }

        // Edit distance of original and transcribed matched word sentences.
        public int EditDistanceMatched { get; set; }

        // The main score prior to resyncing.
        public double MainScorePriorToResync { get; set; }

        // The candidate sentence before resyncing.
        public string BestCandidateSentencePriorToResync { get; set; }

        // Sentence start time.
        public TimeSpan StartTime { get; set; }

        // Sentence stop time.
        public TimeSpan StopTime { get; set; }

        // Sentence duration time.
        public TimeSpan Duration
        {
            get
            {
                return StopTime - StartTime;
            }
        }

        // Start time of silence before sentence.
        public TimeSpan SilenceStartFront { get; set; }

        // Stop time of silence before sentence.
        public TimeSpan SilenceStopFront { get; set; }

        // Duration of silence before sentence.
        public TimeSpan SilenceLengthFront { get { return SilenceStopFront - SilenceStartFront; } }

        // Start time of silence after sentence.
        public TimeSpan SilenceStartBack { get; set; }

        // Stop time of silence after sentence.
        public TimeSpan SilenceStopBack { get; set; }

        // Duration of silence after sentence.
        public TimeSpan SilenceLengthBack { get { return SilenceStopBack - SilenceStartBack; } }

        // Previous transcribed word start time.
        public TimeSpan PreviousTranscribedWordStartTime { get; set; }

        // Previous transcribed word middle time.
        public TimeSpan PreviousTranscribedWordMiddleTime { get; set; }

        // Previous transcribed word stop time.
        public TimeSpan PreviousTranscribedWordStopTime { get; set; }

        // First transcribed word start time.
        public TimeSpan FirstTranscribedWordStartTime { get; set; }

        // First transcribed word middle time.
        public TimeSpan FirstTranscribedWordMiddleTime { get; set; }

        // First transcribed word stop time.
        public TimeSpan FirstTranscribedWordStopTime { get; set; }

        // Second transcribed word middle time.
        public TimeSpan SecondTranscribedWordMiddleTime { get; set; }

        // Last transcribed word start time.
        public TimeSpan LastTranscribedWordStartTime { get; set; }

        // Last transcribed word middle time.
        public TimeSpan LastTranscribedWordMiddleTime { get; set; }

        // Last transcribed word stop time.
        public TimeSpan LastTranscribedWordStopTime { get; set; }

        // Next transcribed word start time.
        public TimeSpan NextTranscribedWordStartTime { get; set; }

        // Next transcribed word middle time.
        public TimeSpan NextTranscribedWordMiddleTime { get; set; }

        // Next transcribed word stop time.
        public TimeSpan NextTranscribedWordStopTime { get; set; }

        // Sentence require a resync.
        public bool RequiredResync { get; set; }

        // Couldn't map sentence text.
        public bool Ignored { get; set; }

        // Store any errors during mapping for this sentence. (For inclusion in report.)
        public string Error { get; set; }

        // Study item index (index of study item in study item array given to mapping function).
        public int StudyItemIndex { get; set; }

        // The study item.
        public MultiLanguageItem StudyItem { get; set; }

        // The index of the sentence in the study item.
        public int SentenceIndex { get; set; }

        // The study item sentence run in the study item.
        public TextRun StudySentence { get; set; }

        // Construct from original sentence run and study item info.
        public SentenceMapping(
            int originalSentenceIndex,
            MappingSentenceRun originalSentence,
            MultiLanguageItem studyItem,
            int studyItemIndex,
            TextRun studyItemSentenceRun,
            int studyItemSentenceIndex,
            int studyItemWordRunStartIndex)
        {
            ClearSentenceMapping();
            OriginalSentenceIndex = originalSentenceIndex;
            OriginalSentence = originalSentence;
            StudyItemIndex = studyItemIndex;
            StudyItem = studyItem;
            SentenceIndex = studyItemSentenceIndex;
            StudySentence = studyItemSentenceRun;
        }

        // Copy constructor. Deep copy.
        public SentenceMapping(SentenceMapping other)
        {
            ClearSentenceMapping();
            CopySentenceMapping(other);
        }

        // Default constructor.
        public SentenceMapping()
        {
            ClearSentenceMapping();
        }

        // Clear sentence mapping.
        protected void ClearSentenceMapping()
        {
            OriginalSentenceIndex = -1;
            OriginalSentence = null;
            TranscribedSentence = null;
            OriginalExtraSentence = null;
            TranscribedExtraSentence = null;
            MainScore = 0.0;
            ExtraScore = 0.0;
            CombinedScore = 0.0;
            EditDistanceRaw = 0;
            EditDistanceProcessed = 0;
            EditDistanceMatched = 0;
            MainScorePriorToResync = 0.0;
            BestCandidateSentencePriorToResync = String.Empty;
            StartTime = TimeSpan.Zero;
            StopTime = TimeSpan.Zero;
            SilenceStartFront = TimeSpan.Zero;
            SilenceStopFront = TimeSpan.Zero;
            SilenceStartBack = TimeSpan.Zero;
            SilenceStopBack = TimeSpan.Zero;
            PreviousTranscribedWordStartTime = TimeSpan.Zero;
            PreviousTranscribedWordMiddleTime = TimeSpan.Zero;
            PreviousTranscribedWordStopTime = TimeSpan.Zero;
            FirstTranscribedWordStartTime = TimeSpan.Zero;
            FirstTranscribedWordMiddleTime = TimeSpan.Zero;
            FirstTranscribedWordStopTime = TimeSpan.Zero;
            SecondTranscribedWordMiddleTime = TimeSpan.Zero;
            LastTranscribedWordStartTime = TimeSpan.Zero;
            LastTranscribedWordMiddleTime = TimeSpan.Zero;
            LastTranscribedWordStopTime = TimeSpan.Zero;
            NextTranscribedWordStartTime = TimeSpan.Zero;
            NextTranscribedWordMiddleTime = TimeSpan.Zero;
            NextTranscribedWordStopTime = TimeSpan.Zero;
            RequiredResync = false;
            Ignored = false;
            Error = String.Empty;
            StudyItemIndex = -1;
            StudyItem = null;
            SentenceIndex = -1;
            StudySentence = null;
        }

        // Copy sentence mapping, full cloning and deep copy.
        protected void CopySentenceMapping(SentenceMapping other)
        {
            OriginalSentenceIndex = other.OriginalSentenceIndex;
            OriginalSentence = MappingSentenceRun.CloneMappingSentenceRun(other.OriginalSentence);
            TranscribedSentence = MappingSentenceRun.CloneMappingSentenceRun(other.TranscribedSentence);
            OriginalExtraSentence = MappingSentenceRun.CloneMappingSentenceRun(other.OriginalExtraSentence);
            TranscribedExtraSentence = MappingSentenceRun.CloneMappingSentenceRun(other.TranscribedExtraSentence);
            MainScore = other.MainScore;
            ExtraScore = other.ExtraScore;
            CombinedScore = other.CombinedScore;
            EditDistanceRaw = other.EditDistanceRaw;
            EditDistanceProcessed = other.EditDistanceProcessed;
            EditDistanceMatched = other.EditDistanceMatched;
            MainScorePriorToResync = other.MainScorePriorToResync;
            BestCandidateSentencePriorToResync = other.BestCandidateSentencePriorToResync;
            StartTime = other.StartTime;
            StopTime = other.StopTime;
            SilenceStartFront = other.SilenceStartFront;
            SilenceStopFront = other.SilenceStopFront;
            SilenceStartBack = other.SilenceStartBack;
            SilenceStopBack = other.SilenceStopBack;
            PreviousTranscribedWordStartTime = other.PreviousTranscribedWordStartTime;
            PreviousTranscribedWordMiddleTime = other.PreviousTranscribedWordMiddleTime;
            PreviousTranscribedWordStopTime = other.PreviousTranscribedWordStopTime;
            FirstTranscribedWordStartTime = other.FirstTranscribedWordStartTime;
            FirstTranscribedWordMiddleTime = other.FirstTranscribedWordMiddleTime;
            FirstTranscribedWordStopTime = other.FirstTranscribedWordStopTime;
            SecondTranscribedWordMiddleTime = other.SecondTranscribedWordMiddleTime;
            LastTranscribedWordStartTime = other.LastTranscribedWordStartTime;
            LastTranscribedWordMiddleTime = other.LastTranscribedWordMiddleTime;
            LastTranscribedWordStopTime = other.LastTranscribedWordStopTime;
            NextTranscribedWordStartTime = other.NextTranscribedWordStartTime;
            NextTranscribedWordMiddleTime = other.NextTranscribedWordMiddleTime;
            NextTranscribedWordStopTime = other.NextTranscribedWordStopTime;
            RequiredResync = other.RequiredResync;
            Ignored = other.Ignored;
            Error = other.Error;
            StudyItemIndex = other.StudyItemIndex;
            StudyItem = other.StudyItem;
            SentenceIndex = other.SentenceIndex;
            StudySentence = other.StudySentence;
        }

        // Copy sentence mapping, shallow original only.
        public void CopySentenceMappingShallowOriginal(SentenceMapping other)
        {
            ClearSentenceMapping();
            OriginalSentenceIndex = other.OriginalSentenceIndex;
            OriginalSentence = other.OriginalSentence;
            OriginalExtraSentence = other.OriginalExtraSentence;
            StudyItemIndex = other.StudyItemIndex;
            StudyItem = other.StudyItem;
            SentenceIndex = other.SentenceIndex;
            StudySentence = other.StudySentence;
            PreviousTranscribedWordStartTime = other.PreviousTranscribedWordStartTime;
            PreviousTranscribedWordMiddleTime = other.PreviousTranscribedWordMiddleTime;
            PreviousTranscribedWordStopTime = other.PreviousTranscribedWordStopTime;
            FirstTranscribedWordStartTime = other.FirstTranscribedWordStartTime;
            FirstTranscribedWordMiddleTime = other.FirstTranscribedWordMiddleTime;
            FirstTranscribedWordStopTime = other.FirstTranscribedWordStopTime;
            SecondTranscribedWordMiddleTime = other.SecondTranscribedWordMiddleTime;
            LastTranscribedWordStartTime = other.LastTranscribedWordStartTime;
            LastTranscribedWordMiddleTime = other.LastTranscribedWordMiddleTime;
            LastTranscribedWordStopTime = other.LastTranscribedWordStopTime;
            NextTranscribedWordStartTime = other.NextTranscribedWordStartTime;
            NextTranscribedWordMiddleTime = other.NextTranscribedWordMiddleTime;
            NextTranscribedWordStopTime = other.NextTranscribedWordStopTime;
        }

        // Copy sentence mapping, shallow copy only.
        public void CopySentenceMappingShallow(SentenceMapping other)
        {
            OriginalSentenceIndex = other.OriginalSentenceIndex;
            OriginalSentence = other.OriginalSentence;
            TranscribedSentence = other.TranscribedSentence;
            OriginalExtraSentence = other.OriginalExtraSentence;
            TranscribedExtraSentence = other.TranscribedExtraSentence;
            MainScore = other.MainScore;
            ExtraScore = other.ExtraScore;
            CombinedScore = other.CombinedScore;
            EditDistanceRaw = other.EditDistanceRaw;
            EditDistanceProcessed = other.EditDistanceProcessed;
            EditDistanceMatched = other.EditDistanceMatched;
            MainScorePriorToResync = other.MainScorePriorToResync;
            BestCandidateSentencePriorToResync = other.BestCandidateSentencePriorToResync;
            StartTime = other.StartTime;
            StopTime = other.StopTime;
            SilenceStartFront = other.SilenceStartFront;
            SilenceStopFront = other.SilenceStopFront;
            SilenceStartBack = other.SilenceStartBack;
            SilenceStopBack = other.SilenceStopBack;
            PreviousTranscribedWordStartTime = other.PreviousTranscribedWordStartTime;
            PreviousTranscribedWordMiddleTime = other.PreviousTranscribedWordMiddleTime;
            PreviousTranscribedWordStopTime = other.PreviousTranscribedWordStopTime;
            FirstTranscribedWordStartTime = other.FirstTranscribedWordStartTime;
            FirstTranscribedWordMiddleTime = other.FirstTranscribedWordMiddleTime;
            FirstTranscribedWordStopTime = other.FirstTranscribedWordStopTime;
            SecondTranscribedWordMiddleTime = other.SecondTranscribedWordMiddleTime;
            LastTranscribedWordStartTime = other.LastTranscribedWordStartTime;
            LastTranscribedWordMiddleTime = other.LastTranscribedWordMiddleTime;
            LastTranscribedWordStopTime = other.LastTranscribedWordStopTime;
            NextTranscribedWordStartTime = other.NextTranscribedWordStartTime;
            NextTranscribedWordMiddleTime = other.NextTranscribedWordMiddleTime;
            NextTranscribedWordStopTime = other.NextTranscribedWordStopTime;
            RequiredResync = other.RequiredResync;
            Ignored = other.Ignored;
            Error = other.Error;
            StudyItemIndex = other.StudyItemIndex;
            StudyItem = other.StudyItem;
            SentenceIndex = other.SentenceIndex;
            StudySentence = other.StudySentence;
        }

        // Create shallow limited copy, reusing only originals.
        public SentenceMapping CloneShallowOriginalCopy()
        {
            SentenceMapping sentenceMapping = new SentenceMapping();
            sentenceMapping.CopySentenceMappingShallowOriginal(this);
            return sentenceMapping;
        }

        // Returns true if first word in the sentence was matched.
        public bool IsFirstWordMatched()
        {
            if (OriginalSentence == null)
                return false;

            MappingWordRun firstWordRun = OriginalSentence.FirstWordRun;

            if (firstWordRun == null)
                return false;

            return firstWordRun.Matched;
        }

        // Returns first word score.
        public double FirstWordScore()
        {
            if (OriginalSentence == null)
                return 0.0;

            MappingWordRun firstWordRun = OriginalSentence.FirstWordRun;

            if (firstWordRun == null)
                return 0.0;

            return firstWordRun.WordMatchScore;
        }

        // Returns true if last word in the sentence was matched.
        public bool IsLastWordMatched()
        {
            if (OriginalSentence == null)
                return false;

            MappingWordRun lastWordRun = OriginalSentence.LastWordRun;

            if (lastWordRun == null)
                return false;

            return lastWordRun.Matched;
        }

        // Returns Last word score.
        public double LastWordScore()
        {
            if (OriginalSentence == null)
                return 0.0;

            MappingWordRun lastWordRun = OriginalSentence.LastWordRun;

            if (lastWordRun == null)
                return 0.0;

            return lastWordRun.WordMatchScore;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\"");

            if (OriginalSentence != null)
                sb.Append(OriginalSentence.SentenceText);

            sb.Append("\",");
            sb.Append(MainScore.ToString());
            sb.Append(",");
            sb.Append(CombinedScore.ToString());

            return sb.ToString();
        }
    }
}

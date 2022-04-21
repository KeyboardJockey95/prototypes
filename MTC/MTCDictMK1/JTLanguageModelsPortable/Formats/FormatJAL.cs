using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatJAL : Format
    {
        protected string CourseName;
        protected static string DefaultCourseName = "Japanese Audio Lessons";
        protected bool IsPublic;
        protected string Package;
        protected NodeMaster _CourseMaster;
        protected BaseObjectNodeTree Course;
        protected NodeMaster _LessonMaster;
        protected BaseObjectNode Lesson;
        protected BaseObjectContent TextContent;
        protected ContentStudyList TextStudyList;
        protected BaseObjectContent SentencesContent;
        protected ContentStudyList SentencesStudyList;
        protected BaseObjectContent WordsContent;
        protected ContentStudyList WordsStudyList;
        protected MultiLanguageItem StudyItem;
        protected bool ExtractSentences { get; set; }
        protected bool ExtractWords { get; set; }
        protected bool DontDeleteMedia;
        protected bool DontUseWordsRomaji;
        protected LanguageTool TargetTool;
        protected LanguageTool HostTool;
        protected MultiLanguageTool MultiTool;

        private static string FormatDescription = "Import content from text in JapaneseAudioLessons.com lesson format.";

        public FormatJAL(
                string targetType,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("JAL", "FormatJAL", FormatDescription, targetType, "File", "text/plain", ".txt",
                userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatJAL();
        }

        public FormatJAL(FormatJAL other)
            : base(other)
        {
            ClearFormatJAL();
        }

        public FormatJAL()
            : base("JAL", "FormatJAL", FormatDescription, String.Empty, "File", "text/plain", ".txt",
                null, null, null, null, null)
        {
            ClearFormatJAL();
        }

        // For derived classes.
        public FormatJAL(string arrangement, string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatJAL();
        }

        public void ClearFormatJAL()
        {
            AllowStudent = false;
            AllowTeacher = false;
            AllowAdministrator = true;
            CourseName = null;
            IsPublic = false;
            Package = null;
            _CourseMaster = null;
            Course = null;
            _LessonMaster = null;
            Lesson = null;
            TextContent = null;
            TextStudyList = null;
            SentencesContent = null;
            SentencesStudyList = null;
            WordsContent = null;
            WordsStudyList = null;
            StudyItem = null;
            ExtractSentences = false;
            ExtractWords = false;
            DontDeleteMedia = false;
            DontUseWordsRomaji = true;
            TargetTool = null;
            HostTool = null;
            MultiTool = null;
        }

        public void CopyFormatJAL(FormatJAL other)
        {
        }

        public override Format Clone()
        {
            return new FormatJAL(this);
        }

        private enum State
        {
            New,
            PageNumber,
            Title,
            Chapter,
            English,
            Kanji,
            Romaji,
            Notes
        }

        protected enum LineType
        {
            Text,
            NumberedLine,
            Title,
            ChapterHeading,
            PageNumber,
            Notes,
            Unknown
        }

        public override void Read(Stream stream)
        {
            StreamReader textReader = new StreamReader(stream);
            List<string> lines = new List<string>();
            string str;
            State state = State.New;
            int lineCount;
            int lineIndex;
            string line = String.Empty;
            LineType lineType = LineType.Unknown;
            int ordinal = -1;

            ResetProgress();

            InitializePriorStudyItemCache();

            TargetTool = GetLanguageTool(TargetLanguageID);
            HostTool = GetLanguageTool(HostLanguageID);
            MultiTool = new MultiLanguageTool(TargetTool, HostTool);

            UpdateProgressElapsed("Reading lines.");

            while ((str = textReader.ReadLine()) != null)
                lines.Add(str);

            lineCount = lines.Count();

            if ((Targets != null) && (Targets.Count != 0))
            {
                IBaseObject target = Targets[0];

                if (target is BaseObjectNodeTree)
                    Course = target as BaseObjectNodeTree;
                else if (target is BaseObjectNode)
                {
                    Lesson = target as BaseObjectNode;
                    Course = Lesson.Tree;
                }
                else if (target is BaseObjectContent)
                {
                    TextContent = target as BaseObjectContent;
                    TextStudyList = TextContent.ContentStorageStudyList;
                }
                else
                    throw new Exception("Unexpected target type.");
            }

            ContinueProgress((lineCount / 500) * 3);

            try
            {
                for (lineIndex = 0; lineIndex < lineCount;)
                {
                    //if (ordinal == 155)
                    //{
                    //    PutMessage("Ordinal " + ordinal.ToString());
                    //}

                    switch (state)
                    {
                        case State.New:
                            line = lines[lineIndex];
                            lineType = ParseLine(line);
                            switch (lineType)
                            {
                                case LineType.NumberedLine:
                                case LineType.Text:
                                    state = State.English;
                                    break;
                                case LineType.Title:
                                    state = State.Title;
                                    break;
                                case LineType.ChapterHeading:
                                    state = State.Chapter;
                                    break;
                                case LineType.PageNumber:
                                    state = State.PageNumber;
                                    break;
                                case LineType.Notes:
                                    state = State.Notes;
                                    break;
                                case LineType.Unknown:
                                    throw new Exception("Unknown line type.");
                                default:
                                    throw new Exception("Unexpected line type.");
                            }
                            break;
                        case State.PageNumber:
                            state = State.New;
                            lineIndex++;
                            break;
                        case State.Title:
                            CreateCourse(!String.IsNullOrEmpty(CourseName) ? CourseName : line);
                            state = State.New;
                            lineIndex++;
                            break;
                        case State.Chapter:
                            FinalizeLesson();
                            UpdateProgressElapsed("Starting " + line + ".");
                            CreateLesson(line);
                            state = State.New;
                            lineIndex++;
                            break;
                        case State.English:
                            line = lines[lineIndex];
                            lineType = ParseLine(line);
                            if (lineType == LineType.PageNumber)
                            {
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.NumberedLine)
                            {
                                NewStudyItem();
                                TextUtilities.ParseLineNumber(ref line, out ordinal);
                                SetStudyItemOrdinal(ordinal);
                                FilterLine(ref line);
                                SetStudyItemText(LanguageLookup.English, line);
                                state = State.Kanji;
                                lineIndex++;
                            }
                            else if (lineType == LineType.Text)
                            {
                                SetStudyItemNote(line, "AnswerNote");
                                lineIndex++;
                            }
                            else
                                state = State.New;
                            break;
                        case State.Kanji:
                            line = lines[lineIndex];
                            lineType = ParseLine(line);
                            if (lineType == LineType.PageNumber)
                            {
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.Notes)
                            {
                                SetStudyItemNote(line, "AnswerNote");
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.NumberedLine)
                            {
                                state = State.New;
                                break;
                            }
                            if (lineType != LineType.Text)
                                throw new Exception("Expected Kanji text.");
                            FilterLine(ref line);
                            if (!IsJapanese(line))
                            {
                                if (StudyItem.HasAnnotation("QuestionNote"))
                                    AppendStudyItemNote(line, "QuestionNote");
                                else if ((ParseLine(lines[lineIndex + 1]) == LineType.Text) && IsJapanese(lines[lineIndex + 1]))
                                {
                                    SetStudyItemText(LanguageLookup.JapaneseRomaji, line);
                                    line = lines[lineIndex + 1];
                                    FilterLine(ref line);
                                    SetStudyItemText(LanguageLookup.Japanese, line);
                                    SetStudyItemText(LanguageLookup.JapaneseKana, String.Empty);
                                    lineIndex += 2;
                                    state = State.New;
                                    break;
                                }
                                else
                                    AppendStudyItemText(LanguageLookup.English, line);
                                lineIndex++;
                                break;
                            }
                            SetStudyItemText(LanguageLookup.Japanese, line);
                            SetStudyItemText(LanguageLookup.JapaneseKana, String.Empty);
                            state = State.Romaji;
                            lineIndex++;
                            break;
                        case State.Romaji:
                            line = lines[lineIndex];
                            lineType = ParseLine(line);
                            if (lineType == LineType.PageNumber)
                            {
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.Notes)
                            {
                                SetStudyItemNote(line, "AnswerNote");
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.NumberedLine)
                            {
                                state = State.New;
                                break;
                            }
                            if (lineType != LineType.Text)
                                throw new Exception("Expected Romaji text.");
                            FilterLine(ref line);
                            SetStudyItemText(LanguageLookup.JapaneseRomaji, line);
                            state = State.Notes;
                            lineIndex++;
                            break;
                        case State.Notes:
                            line = lines[lineIndex];
                            lineType = ParseLine(line);
                            if (lineType == LineType.PageNumber)
                            {
                                lineIndex++;
                                break;
                            }
                            if (lineType == LineType.Notes)
                            {
                                SetStudyItemNote(line, "AnswerNote");
                                lineIndex++;
                            }
                            else if (lineType == LineType.Text)
                            {
                                if (StudyItem.HasAnnotation("AnswerNote"))
                                    AppendStudyItemNote(line, "AnswerNote");
                                else
                                    AppendStudyItemText(LanguageLookup.JapaneseRomaji, line);
                                lineIndex++;
                            }
                            state = State.New;
                            break;
                        default:
                            throw new Exception("Unexpected state.");
                    }
                }
            }
            catch (Exception exc)
            {
                PutExceptionError(exc);
            }
            finally
            {
                FinalizeLesson();
                FinalizeCourse();
                ClearPriorStudyItemCache();
            }
        }

        protected LineType ParseLine(string line)
        {
            int i;
            int c = line.Length;

            if (line.StartsWith("Japanese Audio Flashcard Lessons"))
                return LineType.Title;

            if (line.StartsWith("Chapter "))
            {
                for (i = 8; i < c; i++)
                {
                    if (!char.IsDigit(line[i]))
                        break;
                }

                if (i == c)
                    return LineType.ChapterHeading;
            }

            if (char.IsDigit(line[0]))
            {
                for (i = 1; i < c; i++)
                {
                    if (!char.IsDigit(line[i]))
                        break;
                }

                if (i < c)
                {
                    if (line[i] == '-')
                    {
                        i++;

                        if (char.IsDigit(line[i]))
                        {
                            i++;

                            for (; i < c; i++)
                            {
                                if (!char.IsDigit(line[i]))
                                    break;
                            }

                            if (i == c)
                                return LineType.PageNumber;
                        }
                    }
                    else if (line[i] == '.')
                        return LineType.NumberedLine;
                }
            }

            if (line.StartsWith("(") && line.EndsWith(")"))
                return LineType.Notes;

            if (line.StartsWith("[") && line.EndsWith("]"))
                return LineType.Notes;

            return LineType.Text;
        }

        protected static string[] RawBeginningNotes =
        {
            "The following",
            "Here is another"
        };

        protected static string[] RawMidNotes =
        {
            "is understood"
        };

        protected static string[] RawCommaNotes =
        {
            ", meaning"
        };

        protected static string[] RawEndNotes =
        {
            ". Use",
            ". Meaning",
            ". This implies",
            ". In this lesson",
            ". In this context",
            ". In this case",
            ". This could also mean",
            ". From now on",
            ". Plain speech",
            ". Polite speech",
            ". Formal speech",
            ". Humble speech",
            ". 1 response",
            ". 2 responses",

            "? Use",
            "? Meaning",
            "? This implies",
            "? In this lesson",
            "? In this context",
            "? In this case",
            "? This could also mean",
            "? From now on",
            "? Plain speech",
            "? Polite speech",
            "? Formal speech",
            "? Humble speech",
            "? 1 response",
            "? 2 responses",
        };

        protected void FilterLine(ref string line)
        {
            int start = 0;

            while ((start = line.IndexOf('(')) != -1)
            {
                int end = line.IndexOf(')', start + 1);

                if ((end == -1) || (end < start))
                    end = line.Length;
                else
                    end++;

                string note = line.Substring(start, end - start);
                SetStudyItemNote(note, "AnswerNote");
                line = line.Remove(start, end - start);
            }

            foreach (string pattern in RawEndNotes)
            {
                start = line.IndexOf(pattern);

                if (start != -1)
                {
                    if (char.IsPunctuation(pattern[0]))
                        start++;
                    string note = line.Substring(start);
                    SetStudyItemNote(note, "QuestionNote");
                    line = line.Remove(start);
                }
            }

            foreach (string pattern in RawMidNotes)
            {
                start = line.IndexOf(pattern);

                if (start != -1)
                {
                    int end = line.LastIndexOf('.', start, start);

                    if (end != -1)
                    {
                        end++;

                        while ((end < line.Length) && char.IsWhiteSpace(line[end]))
                            end++;

                        start = end;

                        end = line.IndexOf('.', start);

                        if (end == -1)
                            end = line.Length;
                        else
                            end++;

                        string note = line.Substring(start, end - start);
                        SetStudyItemNote(note, "QuestionNote");
                        line = line.Remove(start, end - start);
                    }
                }
            }

            foreach (string pattern in RawCommaNotes)
            {
                start = line.IndexOf(pattern);

                if (start != -1)
                {
                    string note = "... " + line.Substring(start);
                    SetStudyItemNote(note, "QuestionNote");
                    int end = line.IndexOf('.', start);

                    if (end != -1)
                        line = line.Remove(start, end - start);
                    else
                        line = line.Remove(start);
                }
            }

            foreach (string pattern in RawBeginningNotes)
            {
                if (line.StartsWith(pattern))
                {
                    start = line.IndexOf('.');

                    if (start != -1)
                    {
                        start++;
                        while ((start < line.Length) && char.IsWhiteSpace(line[start]))
                            start++;
                        string note = line.Substring(0, start);
                        SetStudyItemNote(note, "QuestionNote");
                        line = line.Remove(0, start);
                    }
                }
            }
        }

        protected void CreateCourse(string title)
        {
            string description = "From JapaneseAudioLessons.com.";

            if (Course == null)
            {
                ObjectReferenceNodeTree treeHeader =
                    Repositories.ResolveNamedReference("CourseHeaders", null, Owner, title) as ObjectReferenceNodeTree;

                if (treeHeader != null)
                    Course = Repositories.Courses.Get(treeHeader.Key);
            }

            if (Course == null)
            {
                string targetLabel = TargetLabel;
                string treeLabel;
                string treeSource;
                if ((targetLabel == "Lessons") || (targetLabel == "Plans"))
                {
                    treeLabel = (targetLabel == "Lessons" ? "Course" : "Plan");
                    treeSource = (targetLabel == "Lessons" ? "Courses" : "Plans");
                }
                else
                {
                    treeLabel = "Course";
                    treeSource = "Lessons";
                }
                string imageFileName = null;
                int courseOrdinal = 0;
                List<BaseObjectNode> treeNodeChildren = null;
                List<IBaseObjectKeyed> treeOptions = null;
                MarkupTemplate treeMarkupTemplate = null;
                MarkupTemplateReference treeMarkupReference = null;
                List<BaseObjectContent> treeContentChildren = null;
                List<BaseObjectContent> treeContentList = null;
                NodeMaster treeMaster = CourseMaster;
                List<BaseObjectNode> treeNodes = null;

                Course = new BaseObjectNodeTree(
                    0,
                    new MultiLanguageString("Title", UILanguageID, title),
                    new MultiLanguageString("Description", UILanguageID, description),
                    treeSource,
                    Package,
                    treeLabel,
                    imageFileName,
                    courseOrdinal,
                    IsPublic,
                    LanguageID.CopyList(TargetTool.TargetLanguageIDs),
                    CloneHostLanguageIDs(),
                    Owner,
                    treeNodeChildren,
                    treeOptions,
                    treeMarkupTemplate,
                    treeMarkupReference,
                    treeContentChildren,
                    treeContentList,
                    treeMaster,
                    treeNodes);

                Course.EnsureGuid();
                Course.SetupDirectory();

                NodeUtilities.SetupNodeFromMaster(Course);

                if (!NodeUtilities.AddTree(Course, false))
                {
                    UpdateErrorFromNodeUtilities();
                    return;
                }
            }
            else
            {
                if (DeleteBeforeImport)
                    NodeUtilities.DeleteTreeChildrenHelper(Course);

                if (!String.IsNullOrEmpty(title))
                    Course.SetTitleString(UILanguageID, title);

                if (!String.IsNullOrEmpty(description))
                    Course.SetDescriptionString(UILanguageID, description);

                if (Course.TargetLanguageIDs == null)
                    Course.TargetLanguageIDs = LanguageID.CopyList(TargetTool.TargetLanguageIDs);

                if (Course.HostLanguageIDs == null)
                    Course.HostLanguageIDs = CloneHostLanguageIDs();

                Course.Package = Package;
                Course.IsPublic = IsPublic;

                bool ownerChanged = Course.Owner != Owner;
                string oldMediaPath = Course.MediaDirectoryPath;

                if (ownerChanged)
                {
                    Course.Owner = Owner;

                    NodeUtilities.PropagateOwnerNoUpdate(Course, null);

                    if (FileSingleton.DirectoryExists(oldMediaPath))
                    {
                        Course.Directory = Course.ComposeDirectory();
                        string newMediaPath = Course.MediaDirectoryPath;

                        if (!FileSingleton.DirectoryExists(newMediaPath))
                        {
                            try
                            {
                                FileSingleton.MoveDirectory(oldMediaPath, newMediaPath);
                            }
                            catch (Exception exc)
                            {
                                PutLogExceptionError("Error moving media directory", exc);
                            }
                        }
                        else
                            PutLogError("New media directory already exists. Not copying it.");
                    }
                    else
                        Course.SetupDirectory();
                }
                else
                    Course.SetupDirectory();
            }
        }

        protected void CreateCourseCheck()
        {
            if (Course == null)
                CreateCourse(!String.IsNullOrEmpty(CourseName) ? CourseName : DefaultCourseName);
        }

        protected void FinalizeCourse()
        {
            if (Course == null)
                return;

            UpdateProgressElapsed("Updating course.");

            NodeUtilities.UpdateTree(Course, true, false);
        }

        protected void CreateLesson(string lessonTitle)
        {
            string lessonDescription = "From JapaneseAudioLessons.com.";
            string lessonImageFileName = null;
            int lessonOrdinal = ObjectUtilities.GetIntegerFromStringEnd(lessonTitle, 0);

            CreateCourseCheck();

            Lesson = Course.GetNodeWithTitle(lessonTitle, LanguageLookup.English);

            if (Lesson != null)
            {
                if (!String.IsNullOrEmpty(lessonTitle))
                    Lesson.SetTitleString(UILanguageID, lessonTitle);

                if (!String.IsNullOrEmpty(lessonDescription))
                    Lesson.SetDescriptionString(UILanguageID, lessonDescription);

                if (Lesson.TargetLanguageIDs == null)
                    Lesson.TargetLanguageIDs = LanguageID.CopyList(TargetTool.TargetLanguageIDs);

                if (Lesson.HostLanguageIDs == null)
                    Lesson.HostLanguageIDs = CloneHostLanguageIDs();

                Lesson.Package = Package;
                Lesson.IsPublic = IsPublic;
                if (Lesson.Master == null)
                    Lesson.Master = LessonMaster;
                NodeUtilities.CheckNodeFromMaster(Lesson);
                GetContent();
                return;
            }

            string lessonSource = null;
            string lessonLabel = "Lesson";
            BaseObjectNode lessonParent = null;
            List<BaseObjectNode> lessonNodeChildren = null;
            List<IBaseObjectKeyed> lessonOptions = null;
            MarkupTemplate lessonMarkupTemplate = null;
            MarkupTemplateReference lessonMarkupReference = null;
            List<BaseObjectContent> lessonContentChildren = null;
            List<BaseObjectContent> lessonContentList = null;
            NodeMaster lessonMaster = LessonMaster;

            Lesson = new BaseObjectNode(
                0,
                new MultiLanguageString("Title", UILanguageID, lessonTitle),
                new MultiLanguageString("Description", UILanguageID, lessonDescription),
                lessonSource,
                Package,
                lessonLabel,
                lessonImageFileName,
                lessonOrdinal,
                IsPublic,
                LanguageID.CopyList(TargetTool.TargetLanguageIDs),
                CloneHostLanguageIDs(),
                Owner,
                Course,
                lessonParent,
                lessonNodeChildren,
                lessonOptions,
                lessonMarkupTemplate,
                lessonMarkupReference,
                lessonContentChildren,
                lessonContentList,
                lessonMaster);

            Lesson.EnsureGuid();
            Lesson.SetupDirectory();

            NodeUtilities.SetupNodeFromMaster(Lesson);

            Course.AddChildNode(Lesson);

            Lesson.TouchAndClearModified();

            GetContent();
        }

        public void GetContent()
        {
            if (Lesson == null)
                throw new Exception("No lesson created.");

            TextContent = Lesson.GetContent("Text");

            if (TextContent == null)
                throw new Exception("No text content.");

            TextStudyList = TextContent.ContentStorageStudyList;

            if (TextStudyList == null)
                throw new Exception("No text study list.");

            NodeUtilities.DeleteContentContentsHelper(TextContent, true);
            TextStudyList.DeleteAllStudyItems();

            SentencesContent = Lesson.GetContent("Sentences");

            if (SentencesContent == null)
                throw new Exception("No sentences content.");

            SentencesStudyList = SentencesContent.ContentStorageStudyList;

            if (SentencesStudyList == null)
                throw new Exception("No sentences study list.");

            NodeUtilities.DeleteContentContentsHelper(SentencesContent, true);
            SentencesStudyList.DeleteAllStudyItems();

            WordsContent = Lesson.GetContent("Words");

            if (WordsContent == null)
                throw new Exception("No words content.");

            WordsStudyList = WordsContent.ContentStorageStudyList;

            if (WordsStudyList == null)
                throw new Exception("No words study list.");

            NodeUtilities.DeleteContentContentsHelper(WordsContent, true);
            WordsStudyList.DeleteAllStudyItems();
        }

        protected void FinalizeLesson()
        {
            if (TextStudyList == null)
                return;

            UpdateProgressElapsed("RunCheck " + Lesson.GetTitleString() + ".");

            List<MultiLanguageItem> studyItems = TextStudyList.StudyItems;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                TargetTool.GetMultiLanguageItemSentenceAndWordRuns(studyItem, true);
                HostTool.GetMultiLanguageItemSentenceAndWordRuns(studyItem, true);

                if (!studyItem.HasText(LanguageLookup.JapaneseKana))
                {
                    string errorMessage;
                    LanguageUtilities.Translator.TranslateMultiLanguageItem(
                        studyItem,
                        TargetTool.TargetLanguageIDs,
                        true,
                        true,
                        out errorMessage,
                        false);
                }
            }

            if (IsSynthesizeMissingAudio || IsForceAudio)
            {
                UpdateProgressElapsed("Generating audio for " + Lesson.GetTitleString() + ".");

                int lineNumber = 1;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    Annotation ordinalAnnotation = studyItem.FindAnnotation("Ordinal");
                    string line = (ordinalAnnotation != null ?
                        " ordinal " + ordinalAnnotation.Value :
                        " line " + lineNumber.ToString());
                    string label = Lesson.GetTitleString() + line;

                    UpdateProgressMessageElapsed("Generating audio for " + label + ".");

                    if (!NodeUtilities.GetMediaForStudyItem(
                            studyItem,
                            UniqueLanguageIDs,
                            true,
                            true,
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            false,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            false))
                    {
                        //returnValue = false;
                        PutMessage("Ignoring audio generation error for " + label);
                    }

                    lineNumber++;
                }
            }
            else
                UpdateProgressElapsed("Skipping audio generation for " + Lesson.GetTitleString() + ".");

            if (ExtractSentences || ExtractWords)
                UpdateProgressElapsed("Doing extractions " + Lesson.GetTitleString() + ".");
            else
                UpdateProgressElapsed("Skipping extractions " + Lesson.GetTitleString() + ".");

            ExtractionCheck();

            NodeUtilities.UpdateContentStorage(TextContent, false);
        }

        protected bool ExtractionCheck()
        {
            bool returnValue = true;

            if (ExtractSentences && (SentencesStudyList != null))
            {
                if (!ExtractSentencesStudyList(SentencesStudyList, TextStudyList, TargetLanguageID))
                    returnValue = false;
            }

            if (ExtractWords && (WordsStudyList != null))
            {
                if (!ExtractWordsStudyList(WordsStudyList, TextStudyList, TargetLanguageID))
                    returnValue = false;
            }

            return returnValue;
        }

        protected bool ExtractSentencesStudyList(
            ContentStudyList sentencesStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent sentencesContent = sentencesStudyList.Content;
            string contentKey = sentencesContent.KeyString;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = sentencesContent.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            int paragraphCount = studyItems.Count();
            int paragraphIndex = 0;
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            UpdateProgressMessageElapsed("Extracting sentences for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(sentencesContent);

            sentencesStudyList.DeleteAllStudyItems();
            sentencesStudyList.StudyItemOrdinal = 0;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                if (IsCanceled())
                    break;

                if (!studyItem.HasText())
                    continue;

                /*
                PutStatusMessageElapsed(
                    "Extracting sentences from paragraph " +
                        paragraphIndex.ToString() +
                        " of " +
                        paragraphCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

                paragraphIndex++;

                int sentenceCount = studyItem.GetMaxSentenceCount(UniqueLanguageIDs);

                if (sentenceCount == 0)
                    continue;

                string sourceTildeUrl = studyItem.MediaTildeUrl + "/";
                string relativePathToSource = MediaUtilities.MakeRelativeUrl(destTildeUrl, sourceTildeUrl);

                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    string sentenceText = studyItem.RunText(languageID, sentenceIndex);

                    if (PriorStudyItemCache != null)
                    {
                        if (PriorStudyItemCache.Find(contentKey, languageID, sentenceText, out oldStudyItem))
                            continue;
                    }
                    else
                    {
                        if (sentencesStudyList.FindStudyItem(sentenceText, languageID) != null)
                            continue;

                        if (NodeUtilities.StringExistsInPriorContentCheck(
                                sentencesContent,
                                sentenceText,
                                languageID,
                                out oldContent,
                                out oldStudyItem))
                            continue;

                        if (IsExcludePrior)
                        {
                            if (NodeUtilities.StringExistsInPriorLessonsCheck(
                                    sentencesContent,
                                    sentenceText,
                                    languageID,
                                    out oldContent,
                                    out oldStudyItem))
                                continue;
                        }
                    }

                    string studyItemKey = sentencesStudyList.AllocateStudyItemKey();
                    MultiLanguageItem targetStudyItem = new MultiLanguageItem(studyItemKey, new List<LanguageItem>());
                    bool haveMediaRuns = false;

                    foreach (LanguageItem sourceLanguageItem in studyItem.LanguageItems)
                    {
                        TextRun sourceSentenceRun = sourceLanguageItem.GetSentenceRun(sentenceIndex);
                        if (sourceSentenceRun == null)
                        {
                            PutStatusMessage(
                                "Missmatched sentences in " +
                                    sourceLanguageItem.Text +
                                    " ...");
                            continue;
                        }
                        string text = sourceLanguageItem.GetRunText(sourceSentenceRun);
                        LanguageID sourceLanguageID = sourceLanguageItem.LanguageID;
                        List<MediaRun> targetMediaRuns = null;
                        if (sourceSentenceRun.HasMediaRunWithKey("Audio"))
                        {
                            targetMediaRuns = sourceSentenceRun.CloneAndRetargetMediaRuns(relativePathToSource);
                            if ((targetMediaRuns != null) && (targetMediaRuns.Count() != 0))
                                haveMediaRuns = true;
                        }
                        TextRun targetSentenceRun = new TextRun(0, text.Length, targetMediaRuns);
                        List<TextRun> wordRuns = null;
                        if (sourceLanguageItem.HasWordRuns())
                            wordRuns = sourceLanguageItem.ExtractWordRuns(
                                sourceSentenceRun.Start,
                                sourceSentenceRun.Stop,
                                0);
                        LanguageItem targetLanguageItem = new LanguageItem(
                            studyItemKey,
                            sourceLanguageID,
                            text,
                            new List<TextRun>(1) { targetSentenceRun },
                            wordRuns);
                        targetStudyItem.Add(targetLanguageItem);
                    }

                    if ((sentenceIndex == 0) && studyItem.HasAnnotation("Ordinal"))
                        targetStudyItem.Annotations = studyItem.CloneAnnotations();

                    sentencesStudyList.AddStudyItem(targetStudyItem);

                    if (PriorStudyItemCache != null)
                        PriorStudyItemCache.Add(contentKey, languageID, targetStudyItem);

                    targetStudyItem.SentenceAndWordRunCheck(Repositories.Dictionary);

                    if (!haveMediaRuns && (IsSynthesizeMissingAudio || IsForceAudio))
                    {
                        string label = Lesson.GetTitleString() + targetStudyItem.Text(TargetLanguageIDs[0]);

                        UpdateProgressMessageElapsed("Generating audio for " + label + ".");

                        if (!NodeUtilities.GetMediaForStudyItem(
                                targetStudyItem,
                                UniqueLanguageIDs,
                                true,
                                true,
                                ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                                false,
                                IsSynthesizeMissingAudio,
                                IsForceAudio,
                                false))
                        {
                            //returnValue = false;
                            PutMessage("Ignoring audio generation error.");
                        }
                    }
                }
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(sentencesStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(sentencesContent);

            return returnValue;
        }

        protected bool ExtractWordsStudyList(
            ContentStudyList wordsStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent wordsContent = wordsStudyList.Content;
            string contentKey = wordsContent.KeyString;
            BaseObjectNode node = wordsContent.Node;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = wordsStudyList.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            List<LanguageID> languageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            int targetLanguageIDCount = languageIDs.Count();
            LanguageID firstAlternateLanguageID = null;
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            if (targetLanguageIDCount > 1)
                firstAlternateLanguageID = languageIDs[1];

            UpdateProgressMessageElapsed("Extracting words for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(wordsContent);

            wordsStudyList.DeleteAllStudyItems();
            wordsStudyList.StudyItemOrdinal = 0;

            List<MultiLanguageString> words = NodeUtilities.CollectWordInstances(studyItems, languageIDs);
            int wordCount = words.Count();
            int wordIndex;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MultiLanguageString word = words[wordIndex];
                string targetWord = word.Text(languageID);

                if (ProgressCancelCheck())
                    break;

                /*
                PutStatusMessageElapsed(
                    "Extracting word " +
                        wordIndex.ToString() +
                        " of " +
                        wordCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

#if false
                string wordHash = word.GetStringListString(languageIDs);

                if (PriorStudyItemCache != null)
                {
                    if (PriorStudyItemCache.Find(contentKey, languageID, wordHash, out oldStudyItem))
                        continue;

                    MultiLanguageItem tmpStudyItem = new MultiLanguageItem(word);

                    PriorStudyItemCache.Add(contentKey, languageID, wordHash, tmpStudyItem);
                }
                else
                {
                    if (wordsStudyList.FindStudyItemInstance(word, languageIDs) != null)
                        continue;

                    if (NodeUtilities.TextExistsInPriorContentCheck(
                            wordsContent,
                            word,
                            languageIDs,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.TextExistsInPriorLessonsCheck(
                                wordsContent,
                                word,
                                languageIDs,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }
                }
#endif

                string studyItemKey = wordsStudyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    UniqueLanguageIDs,
                    word);

                targetStudyItem.PrimeSentenceRunsForWordItem(languageIDs);
                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(languageID);

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries;
                bool isInflection = false;
                LanguageTool tool = TargetTool;

                if (tool != null)
                {
                    dictionaryEntry = tool.LookupDictionaryEntry(
                        targetWord,
                        MatchCode.Exact,
                        languageIDs,
                        null,
                        out isInflection);

                    if (dictionaryEntry != null)
                        dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };
                    else
                        dictionaryEntries = null;
                }
                else
                    dictionaryEntries = Repositories.Dictionary.Lookup(
                        targetWord,
                        MatchCode.Exact,
                        languageID,
                        0,
                        0);

                if (ApplicationData.IsMobileVersion)
                {
                    if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                    {
                        if (ApplicationData.RemoteRepositories != null)
                        {
                            dictionaryEntries = ApplicationData.RemoteRepositories.Dictionary.Lookup(
                                targetWord,
                                JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                                languageID,
                                0,
                                0);

                            if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                            {
                                try
                                {
                                    if (!Repositories.Dictionary.AddList(dictionaryEntries, languageID))
                                        PutError("Error saving local dictionary entry");
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError("Exception saving local dictionary entry", exc);
                                }
                            }
                        }
                    }
                }

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;
                    int reading = -1;

                    if (dictionaryEntry.LanguageID != TargetLanguageID)
                    {
                        String baseText = dictionaryEntry.GetFirstAlternateText(languageID);
                        string suffix = targetLanguageItem.Text;
                        targetLanguageItem.Text = baseText;
                        targetLanguageItem.PrimeSentenceRunsForWordItem();
                        Annotation suffixAnnotation = new Annotation(
                            "Suffix",
                            null,
                            new Object.MultiLanguageString(
                                null,
                                new LanguageString(null, languageID, suffix)));
                        targetStudyItem.AddAnnotation(suffixAnnotation);
                    }

                    if ((dictionaryEntry.AlternateCount != 0) && (targetLanguageIDCount > 1))
                    {
                        string altText = word.Text(firstAlternateLanguageID);

                        foreach (LanguageString alternate in dictionaryEntry.Alternates)
                        {
                            if ((alternate.Text == altText) && (alternate.LanguageID == firstAlternateLanguageID))
                            {
                                reading = alternate.KeyInt;
                                break;
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        if ((reading != -1) && (sense.Reading != reading))
                            continue;

                        foreach (LanguageID hostLanguageID in HostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(hostLanguageID);

                            if (sense.HasLanguage(hostLanguageID))
                            {
                                string definitionString = sense.GetDefinition(hostLanguageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (DontUseWordsRomaji)
                    targetStudyItem.SetText(LanguageLookup.JapaneseRomaji, String.Empty);

                if (IsTranslateMissingItems)
                {
                    string errorMessage;

                    if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                            targetStudyItem,
                            UniqueLanguageIDs,
                            false,
                            false,
                            out errorMessage,
                            false))
                    {
                        PutLogError(errorMessage);
                        returnValue = false;
                    }
                }

                string wordHash = targetStudyItem.GetStringListString(languageIDs);

                if (PriorStudyItemCache != null)
                {
                    if (PriorStudyItemCache.Find(contentKey, languageID, wordHash, out oldStudyItem))
                        continue;

                    PriorStudyItemCache.Add(contentKey, languageID, wordHash, targetStudyItem);
                }
                else
                {
                    word = new MultiLanguageString(targetStudyItem);

                    if (wordsStudyList.FindStudyItemInstance(word, languageIDs) != null)
                        continue;

                    if (NodeUtilities.TextExistsInPriorContentCheck(
                            wordsContent,
                            word,
                            languageIDs,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.TextExistsInPriorLessonsCheck(
                                wordsContent,
                                word,
                                languageIDs,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }
                }

                // Need content set before getting media.
                wordsStudyList.AddStudyItem(targetStudyItem);

                if (IsLookupDictionaryAudio || IsLookupDictionaryPictures || IsSynthesizeMissingAudio || IsForceAudio)
                {
                    string label = Lesson.GetTitleString() + targetStudyItem.Text(TargetLanguageIDs[0]);

                    UpdateProgressMessageElapsed("Generating audio for " + label + ".");

                    if (!NodeUtilities.GetMediaForStudyItem(
                            targetStudyItem,
                            UniqueLanguageIDs,
                            true,
                            true,
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            IsLookupDictionaryAudio,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            IsLookupDictionaryPictures))
                    {
                        //returnValue = false;
                        PutMessage("GetMediaForStudyItem failed", NodeUtilities.MessageOrError);
                    }
                }

                /*
                {
                    string errorMessage = String.Empty;
                    NodeUtilities.AddStudyItemToDictionary(
                        node,
                        wordsContent,
                        targetStudyItem,
                        LanguageIDs,
                        LanguageIDs,
                        LexicalCategory.Unknown,
                        String.Empty,
                        false,
                        false,
                        ref errorMessage);
                }
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(wordsStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(wordsContent);

            return returnValue;
        }

        protected bool IsJapanese(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            int c = str.Length;

            for (int i = 0; i < c; i++)
            {
                if (str[i] > '~')
                    return true;
            }

            return false;
        }

        protected void NewStudyItem()
        {
            if (TextStudyList == null)
                throw new Exception("No text study list.");

            StudyItem = new Content.MultiLanguageItem(TextStudyList.AllocateStudyItemKey(), UniqueLanguageIDs);

            TextStudyList.AddStudyItem(StudyItem);
        }

        protected void SetStudyItemText(LanguageID languageID, string text)
        {
            if (StudyItem == null)
                NewStudyItem();

            LanguageItem languageItem = StudyItem.LanguageItem(languageID);

            if (languageItem != null)
            {
                languageItem.Text = text;
                languageItem.ResetSentenceAndWordRuns();
            }
            else
            {
                languageItem = new LanguageItem(StudyItem.Key, languageID, text);
                StudyItem.Add(languageItem);
            }
        }

        protected void AppendStudyItemText(LanguageID languageID, string text)
        {
            if (StudyItem == null)
                NewStudyItem();

            LanguageItem languageItem = StudyItem.LanguageItem(languageID);

            if (languageItem != null)
            {
                if (!String.IsNullOrEmpty(languageItem.Text))
                    languageItem.Text += " " + text;
                else
                    languageItem.Text = text;

                languageItem.ResetSentenceAndWordRuns();
            }
            else
            {
                languageItem = new LanguageItem(StudyItem.Key, languageID, text);
                StudyItem.Add(languageItem);
            }
        }

        protected void SetStudyItemNote(string text, string noteType)
        {
            if (StudyItem == null)
                NewStudyItem();

            Annotation annotation = StudyItem.FindAnnotation(noteType);

            if (annotation != null)
            {
                LanguageString languageString = annotation.Text.LanguageString(LanguageLookup.English);

                if (languageString == null)
                    throw new Exception("Missing annotation language string.");

                if (languageString.Text.Contains(text))
                    return;

                languageString.Text += "\n" + text;
            }
            else
            {
                MultiLanguageString note = new MultiLanguageString(noteType, HostLanguageID, text);
                annotation = new Annotation(noteType, null, note);
                StudyItem.AddAnnotation(annotation);
            }
        }

        protected void AppendStudyItemNote(string text, string noteType)
        {
            if (StudyItem == null)
                NewStudyItem();

            Annotation annotation = StudyItem.FindAnnotation(noteType);

            if (annotation != null)
            {
                LanguageString languageString = annotation.Text.LanguageString(LanguageLookup.English);

                if (languageString == null)
                    throw new Exception("Missing annotation language string.");

                languageString.Text += text;
            }
            else
            {
                MultiLanguageString note = new MultiLanguageString(noteType, HostLanguageID, text);
                annotation = new Annotation(noteType, null, note);
                StudyItem.AddAnnotation(annotation);
            }
        }

        protected void SetStudyItemOrdinal(int ordinal)
        {
            if (StudyItem == null)
                NewStudyItem();

            Annotation annotation = StudyItem.FindAnnotation("Ordinal");

            if (annotation != null)
                annotation.Value = ordinal.ToString();
            else
            {
                annotation = new Annotation("Ordinal", ordinal.ToString(), null);
                StudyItem.AddAnnotation(annotation);
            }
        }

        public NodeMaster CourseMaster
        {
            get
            {
                if (_CourseMaster == null)
                {
                    string masterName = "Audio Flash Cards Course";

                    _CourseMaster = Repositories.ResolveNamedReference(
                        "NodeMasters", null, Owner, masterName) as NodeMaster;

                    if (_CourseMaster == null)
                        throw new Exception("Master \"Audio Flash Cards Course\" not defined.");
                }

                return _CourseMaster;
            }
            set
            {
                _CourseMaster = value;
            }
        }

        public NodeMaster LessonMaster
        {
            get
            {
                if (_LessonMaster == null)
                {
                    string masterName = "Audio Flash Cards Course";

                    _LessonMaster = Repositories.ResolveNamedReference(
                        "NodeMasters", null, Owner, masterName) as NodeMaster;

                    if (_LessonMaster == null)
                        throw new Exception("Master \"Audio Flash Cards Lesson\" not defined.");
                }

                return _LessonMaster;
            }
            set
            {
                _LessonMaster = value;
            }
        }

        public override void Write(Stream stream)
        {
            throw new ObjectException("FormatJAL: Write not implemented.");
        }

        public static string CourseNameHelp = "The name of the course.";
        public static string IsPublicHelp = "If checked, the course will be marked as public.";
        public static string PackageHelp = "Enter package name. This will restrict access to users with the package name set in their account.";
        public static string ExtractSentencesHelp = "Check this to extract sentences.";
        public static string ExtractWordsHelp = "Check this to extract words.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            CourseName = GetArgumentDefaulted("CourseName", "string", "r", "", "CourseName", CourseNameHelp);

            IsPublic = GetFlagArgumentDefaulted("IsPublic", "flag", "r", IsPublic, "Make public",
                IsPublicHelp, null, null);
            Package = GetArgumentDefaulted("Package", "string", "r", Package, "Package",
                PackageHelp);

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            ExtractSentences = GetFlagArgumentDefaulted("ExtractSentences", "flag", "r", ExtractSentences, "Extract sentences",
                ExtractSentencesHelp, null, null);
            ExtractWords = GetFlagArgumentDefaulted("ExtractWords", "flag", "r", ExtractWords, "Extract words",
                ExtractWordsHelp, null, null);

            IsExcludePrior = GetFlagArgumentDefaulted("IsExcludePrior", "flag", "r", IsExcludePrior, "Exclude prior items", IsExcludePriorHelp,
                null, null);
            IsTranslateMissingItems = GetFlagArgumentDefaulted("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);
            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            IsForceAudio = GetFlagArgumentDefaulted("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            if (NodeUtilities != null)
                Label = NodeUtilities.GetLabelFromContentType(DefaultContentType, DefaultContentSubType);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            GetArgumentDefaulted("CourseName", "string", "r", CourseName, "CourseName", CourseNameHelp);

            SetFlagArgument("IsPublic", "flag", "r", IsPublic, "Make public",
                IsPublicHelp, null, null);
            SetArgument("Package", "string", "r", Package, "Package",
                PackageHelp, null, null);

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetFlagArgument("ExtractSentences", "flag", "r", ExtractSentences, "Extract sentences",
                ExtractSentencesHelp, null, null);
            SetFlagArgument("ExtractWords", "flag", "r", ExtractWords, "Extract words",
                ExtractWordsHelp, null, null);

            SetFlagArgument("IsExcludePrior", "flag", "r", IsExcludePrior, "Exclude prior items", IsExcludePriorHelp,
                null, null);
            SetFlagArgument("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);
            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "rw", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            SetFlagArgument("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                    if (capability == "Support")
                    {
                        if (importExport == "Import")
                            return true;
                    }
                    else if (capability == "Text")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string contentName, string capability)
        {
            return IsSupportedStatic(importExport, contentName, capability);
        }

        public static new string TypeStringStatic { get { return "JAL"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

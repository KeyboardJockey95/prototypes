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
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Formats
{
    public partial class FormatEmbark : Format
    {
        // Object layout for JSON lesson representation.

        // Current format version. Change this as the format changes.
        public static string CurrentFormatVersion = "1.0";

        // Represents the download package.
        public class EmbarkPackage
        {
            public string Key;                      // Package key, a unique identifier derived from the English title.
            public string FormatVersion;            // Format version, for protecting against format changes.
            public string[] TargetLanguageCodes;    // The target language codes (main only) supported in this package.
            public string[] HostLanguageCodes;      // The host language codes (main only) supported in this package.
            public Node[] Nodes;                    // The array of root nodes for the target and host languages.
            public WordList[] WordLists;            // The array of glossaries for each target/host language pair.
            public Dictionary<string, LanguageCodeToNameTable> LanguageCodeToNameTables;
                                                    // Maps language codes to language names per main language.
        }

        //  Start of Node classes. *******************************************

        // Represents one self-contained node or a hierarchy of nodes for one language.
        // The leaf nodes can be considered an individual lesson.
        // Higher nodes are groups of lessons.
        public class Node
        {
            public string Key;                      // Node key, a unique identifier.
            public string[] TargetLanguageCodes;    // Target language codes (main plus alternates).
            public string HostLanguageCode;       // Host language code.
            public string[] Title;                  // The lesson title (main plus alternates plus host).
            public string[] Description;            // The lesson description (main plus alternates plus host).
            public string[] TitlePath;              // Array of node titles up to and including this node
                                                    // (main plus alternates plus host multiplied by path node count).
            public TextUnit[] TextUnits;            // The array of text units.
            public string MediaPath;                // Partial path to the node media files (using URL '/' separator).
            public string MediaFileName;            // Media file name.
            public string IconFileName;             // Icon image file name.
            public Node[] Children;                 // The array of child nodes. Will be empty if this is a leaf node.
        }

        // Text unit types.
        // Add new codes here as needed.
        public enum TextType
        {
            SimpleText = 0,                         // 0 = Simple text.
            Verse = 1,                              // 1 = Verse.
            Heading = 2,                            // 2 = Chapter heading.
            SubHeading = 3,                         // 3 = Chapter subheading.
            ByLine = 4,                             // 4 = Byline.
            Kicker = 5,                             // 5 = Kicker.
            Intro = 6,                              // 6 = Introduction.
            StudyIntro = 7,                         // 7 = Study introduction.
            Title = 8,                              // 8 = General title.
            TitleNumber = 9,                        // 9 = Title with a number.
            SubTitle = 10,                          // 10 = General subtitle.
            Summary = 11,                           // 11 = Summary.
            SuperScript = 12,                       // 12 = Superscript to footnote.
            Footnote = 13,                          // 13 = Footnote.
            Suffix = 14,                            // 14 = Suffix.
            InsetHeadingRed = 15,                   // 15 = Red inset heading.
            InsetHeadingGreen = 16,                 // 16 = Green inset heading.
            InsetHeadingOrange = 17,                // 17 = Orange inset heading.
            InsetHeadingTeal = 18,                  // 18 = Teal inset heading.
            InsetText = 19,                         // 19 = Inset text.
            AsideHeading = 20,                      // 20 = Aside heading.
            AsideText = 21                          // 21 = Aside text.
        };

        // A unit of text.
        // This is a group of text displayed as a unit.
        // It may be a paragraph, verse, title, description, synopsis,
        // or any text that can be studied as a unit.
        // The index of this item can be used in composing the div element ID.
        public class TextUnit
        {
            public string[] Prefix;                 // The prefix strings (i.e. verse number) for the unit, in node language order.
            public string[] Text;                   // The text strings for the unit, in node language order.
            public string Translation;              // The text strings for the unit, in node language order.
            public RunGroup[] RunGroups;            // The run groups.
                                                    // May be only one if sentences runs not supported.
            public int Type;                        // The type or styling of paragraph:
                                                    // See TextType.
                                                    // This might be an index to a literal CSS styling list, or just canned type codes.
            public float StartTime;                 // Start time in Node media (seconds).
            public float StopTime;                  // Stop time in Node media (seconds).
        }

        // Run group.  May be a sentence, line, or whole paragraph.
        public class RunGroup
        {
            public LanguageRuns[] LanguageRuns;     // The runs that make up the paragraph for each language instance,
                                                    // in the same order as the node language list.
            public float StartTime;                 // Start time in Node media (seconds).
            public float StopTime;                  // Stop time in Node media (seconds).
            //public string MediaFileName;          // Stand-alone media file name for this text unit.
                                                    // Disabled now since we are not using stand-alone media
        }

        public class LanguageRuns
        {
            public string LanguageCode;             // Language code plus extension.
                                                    // For example: spa, jpn, jpn-k, jpn-r, zho-t, zho-s, zho-r, ara-v
                                                    // where:
                                                    //      (no "-" suffix) = main language
                                                    //      -k = Kana
                                                    //      -r = Romanization
                                                    //      -v = Vowelled (i.e. Arabic or Hebrew)
                                                    //      -t = Traditional
                                                    //      -s = Simplified
            public int GroupStart;                  // Group text start index.
            public int GroupStop;                   // Group text stop index (non-inclusive).
            public Run[] Runs;                      // The runs that make up the paragraph.
        }

        // A run of text. A word or phrase unit.
        public class Run
        {
            public int RunStart;                    // Index of start of text run.
            public int RunStop;                     // Index of stop of text run (non-inclusive).
            public int GlossaryKey;                 // Identifies the glossary entry. May be just an index.
            public int[] TranslationIndexes;        // Indicate the fitting translation indexes, or null if unknown.
            //public float StartTime;               // Start time in TextUnit media.
            //public float StopTime;                // Stop time in TextUnit media.
                                                    // Disabled now since we don't map words to media
        }

        //  End of Node classes. *******************************************
        //  Start of WordList classes. *******************************************

        // The glossary for one target/host language pair.
        public class WordList
        {
            public string[] TargetLanguageCodes;    // Target language codes (main plus alternates).
            public string[] HostLanguageCodes;      // Host language code (main plus alternates).
            public Word[] Words;                    // The array of words or phrases.
            public string MediaPath;                // Partial path to the word media files (using URL '/' separator).
            public Inflection[] Inflections;        // The array of inflection identifiers.
#if FUTURE
            public PartOfSpeech[] PartsOfSpeech;    // The array of parts of speech
#endif
        }

        // Represents one word or phrase.
        public class Word
        {
            public string Text;                     // The word or phrase string.
            public string[] Alternates;             // Alternate readings and transliterations.
                                                    // Note that we don't identify the languages here,
                                                    // but we use a consistent order, // i.e. pairs of (Kana and Romaji),
                                                    // so the reading index is multiplied by the number of alternate languages.
            public string[] MediaFileNames;         // Media file name per reading.
            public Translation[] Translations;      // The array of translations.
        }

        // One translation of a word.
        public class Translation
        {
            public string Text;                     // The word string.
            public int Reading;                     // Index to reading (multiply by alternate language count)
                                                    // or -1 if no other readings.
            public int LemmaIndex;                  // Index to the lemma or -1 for no lemma.
            public int InflectionIndex;             // Index to the inflection or -1 for no inflection.
#if FUTURE
            public string[] Alternates;             // Alternate transliterations.
            public int SenseID;                     // A number that can be used to match up senses (translations
                                                    // that are synonyms and have the same or similar meaning).
            public int PartOfSpeechIndex;           // Index to part of speech.
#endif
        }

#if FUTURE
        // Describes a part of speech.
        public class PartOfSpeech
        {
            public string Abbreviation;             // Part of speech abbreviation.
            public string FullName;                 // Part of speech full name.
        }
#endif

        // Describes one inflection.
        public class Inflection
        {
            public string Abbreviation;             // Part of speech abbreviation.
            public string FullName;                 // Part of speech full name.
            public NameValuePair[] Designators;     // Inflection designator name/value pairs.
        }

        // Pairs a name and a value.
        public class NameValuePair
        {
            public string Name;                     // The name string.
            public string Value;                    // The value string.
        }

        //  End of WordList classes. *******************************************

        //  Start of LanguageCodeToNameTable classes. *******************************************

        // Maps language codes to language names for one language.
        public class LanguageCodeToNameTable
        {
            public string LanguageCode;                 // Main language code.
            public Dictionary<string, string> Table;    // Map table.
        }

        //  End of LanguageCodeToNameTable classes. *******************************************

    }
}

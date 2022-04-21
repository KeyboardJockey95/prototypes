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
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Formats
{
    public partial class FormatPassage : Format
    {
        // Object layout for JSON lesson representation.

        // Current format version. Change this as the format changes.
        public static string currentFormatVersion = "1.0";

        // Represents the download package.
        public class EmbarkPackage
        {
            public string formatVersion;            // Format version, for protecting against format changes.
            public string targetLanguageCode;       // The target language code supported in this package.
                                                    // For example: es_ES, ja_JP, ja_JP_ph, ja_JP_rm, cmn_TW, cmn_CN, cmn_TW_rm, ar_SA_vw, ar_SA_rm
                                                    // where:
                                                    //      (no "_" suffix) = main language
                                                    //      _ph = Non-romanized phonetic (i.e. Japanese Kana)
                                                    //      _rm = Romanization
                                                    //      _vw = Vowelled (i.e. Arabic or Hebrew)
            public string nativeLanguageCode;       // The native language code supported in this package (i.e. en_US").
            public string mediaPath;                // Partial path to the node audio/visual media files (using URL '/' separator).
            public string imagePath;                // Partial path to the node icon or image files (using URL '/' separator).
            public Node[] nodes;                    // The array of root nodes for the target and native languages.
            public WordList wordList;               // The glossary for each target/native language pair.
            public string errorMessage;             // Error message if package creation failed.
            public int? totalUniqueWordCount;       // The number of unique words.
            public int? foundInGlossaryCount;       // The number of words found in the global glossary.
            public int? foundInDictionaryCount;     // The number of words found in the dictionary.
            public int? recognizedAsInflectionCount;// The number of words recognized as inflected.
            public int? googleTranslateCount;       // The number of words requiring Google Translate.
            public int? translatorCacheCount;       // The number of words from the translator cache or other means.
            public int? lookupDictionaryCount;          // The number of dictionary lookups.
            public int? lookupInflectionCount;          // The number of inflection lookups.
            public int? lookupGoogleTranslateCount;     // The number of Google Translate successful lookups.
            public int? lookupTranslatorCacheCount;     // The number of translator cache lookups.
            public float? sumDictionaryLookupTime;      // The sum of dictionary lookup times.
            public float? sumInflectionLookupTime;      // The sum of recognize-as-inflection times.
            public float? sumGoogleTranslateTime;       // The sum of successful Google Translate times.
            public float? sumTranslatorCacheTime;       // The sum of translator cache lookup times.
            public float? averageDictionaryLookupTime;  // The average dictionary lookup time.
            public float? averageInflectionLookupTime;  // The average inflection lookup time.
            public float? averageGoogleTranslateTime;   // The average Google Translate time.
            public float? averageTranslatorCacheTime;   // The average translator cache lookup time.
            public float? totalGenerationTime;          // The total generation time.
            public float? totalServerTime;              // The total ajax handler time in the server.
        }

        //  Start of Node classes. *******************************************

        // Represents one self-contained node or a hierarchy of nodes for one language.
        // The leaf nodes can be considered an individual lesson.
        // Higher nodes are groups of lessons.
        public class Node
        {
            public string key;                      // Node key, a unique identifier.
            public string targetTitle;              // The node title (target language).
            public string nativeTitle;              // The node title (native language).
            public TextUnit[] textUnits;            // The array of text units.
            public string mediaFileName;            // Media file name.
            public string iconFileName;             // Icon image file name.
            public Node[] children;                 // The array of child nodes. Will be null or
                                                    // empty if this is a leaf node.
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
            public string targetPrefix;             // The prefix strings (i.e. verse number) for the unit, in target language.
            public string nativePrefix;             // The prefix strings (i.e. verse number) for the unit, in native language.
            public string targetText;               // The text strings for the unit, in target language.
            public string nativeText;               // The text strings for the unit, in native language.
            public int type;                        // The type or styling of paragraph:
                                                    // See TextType.
                                                    // This might be an index to a literal CSS styling list, or just canned type codes.
            public float? startTime;                // Start time in Node media (seconds).
            public float? stopTime;                 // Stop time in Node media (seconds).
            public RunGroup[] runGroups;            // The run groups.
                                                    // May be only one if sentences runs not supported.
        }

        // Run group.  May be a sentence, line, or whole paragraph.
        public class RunGroup
        {
            public int sentenceNumber;              // Sentence number.
            public int groupStart;                  // Group text start index.
            public int groupStop;                   // Group text stop index (non-inclusive).
            public float? startTime;                // Start time in Node media (seconds).
            public float? stopTime;                 // Stop time in Node media (seconds).
            //public string mediaFileName;          // Stand-alone media file name for this text unit.
            public Run[] runs;                      // The runs that make up the paragraph.
                                                    // Disabled now since we are not using stand-alone media
        }

        // A run of text. A word or phrase unit.
        public class Run
        {
            public int runStart;                    // Index of start of text run.
            public int runStop;                     // Index of stop of text run (non-inclusive).
            public int glossaryKey;                 // Identifies the glossary entry. May be just an index.
            public int[] fitsContext;               // Indicate the fitting translation indexes, or null if unknown.
            //public float? startTime;              // Start time in TextUnit media.
            //public float? stopTime;               // Stop time in TextUnit media.
            // Disabled now since we don't map words to media
        }

        //  End of Node classes. *******************************************
        //  Start of WordList classes. *******************************************

        // The glossary for one target/native language pair.
        public class WordList
        {
            public Word[] words;                    // The array of words or phrases.
            public string mediaPath;                // Partial path to the word media files (using URL '/' separator).
            public string[] sourceNames;            // Array of source names.
            public Inflection[] inflections;        // The array of inflection identifiers.
#if FUTURE
            public PartOfSpeech[] partsOfSpeech;    // The array of parts of speech
#endif
        }

        // Represents one word or phrase.
        public class Word
        {
            public string text;                     // The word or phrase string (main target language only).
            public AudioReading[] audioReadings;    // The array of audio readings.
            public Translation[] translations;      // The array of translations.
        }

        public class AudioReading
        {
            public Audio[] instances;               // The array of accepted audio instances.
        }

        public class WordAudio
        {
            public string text;                     // The word or phrase string (main target language only).
            public AudioReading[] audioReadings;    // The array of audio readings.
            public int glossaryKey;                 // The index of the glossary entry.
        }

        public class Audio
        {
            public string file;                     // Audio file name.
            public string gender;                   // Speaker gender.
            public string region;                   // Speaker region.
            public string country;                  // Speaker country.
            public string source;                   // Audio source.
        }

        // One translation of a word.
        public class Translation
        {
            public string text;                     // The word string (main native language only).
            public int? reading;                    // Index to reading (multiply by alternate language count)
                                                    // or -1 if no other readings.
            public int? lemmaIndex;                 // Index to the lemma or -1 for no lemma.
            public int? inflectionIndex;            // Index to the inflection or -1 for no inflection.
            public int[] sourceIndexes;             // Indexes to the source names.
            public int? fitFrequency;               // The number of time in the work the "Predicted to fit" was true for this word/translation pair.
#if FUTURE
            public int? partOfSpeechIndex;          // Index to part of speech.
            public int? senseID;                    // A number that can be used to match up senses (translations
                                                    // that are synonyms and have the same or similar meaning).
#endif
        }

#if FUTURE
        // Describes a part of speech.
        public class PartOfSpeech
        {
            public string abbreviation;             // Part of speech abbreviation (native language).
            public string fullName;                 // Part of speech full name (native language).
        }
#endif

        // Describes one inflection.
        public class Inflection
        {
            public string abbreviation;             // Part of speech abbreviation (native language).
            public string fullName;                 // Part of speech full name (native language).
            public NameValuePair[] designators;     // Inflection designator name/value pairs (native language).
        }

        // Pairs a name and a value.
        public class NameValuePair
        {
            public string name;                     // The name string.
            public string value;                    // The value string.
        }

        //  End of WordList classes. *******************************************
    }
}

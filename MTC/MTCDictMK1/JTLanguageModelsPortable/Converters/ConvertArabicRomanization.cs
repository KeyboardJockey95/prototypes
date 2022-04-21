using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Converters
{
    public class ConvertArabicRomanization : ConvertRomanization
    {
        private static Dictionary<string, string> _TheTableDictionary;
        private static char[] _TheCharacters;
        private static HashSet<char> _TheCharacterSet;
        private static int _TheMaxInputLength = -1;
        private static int _TheMaxOutputLength = -1;

        public ConvertArabicRomanization(
                char symbolSeparator,
                DictionaryRepository dictionary,
                bool useQuickDictionary) :
            base(
                TheTablePairs,
                TheTableDictionary,
                TheCharacters,
                TheCharacterSet,
                LanguageLookup.Arabic,
                LanguageLookup.ArabicRomanization,
                symbolSeparator,
                dictionary,
                useQuickDictionary,
                TheMaxInputLength,
                TheMaxOutputLength)
        {
        }

        public override bool To(out string output, string input)
        {
            string tempOutput;

            bool returnValue = base.To(out tempOutput, input);

            if (returnValue)
            {
                int index;
                int startIndex = 0;

                tempOutput = tempOutput.Replace("(SUKUN)", "");

                while ((index = tempOutput.IndexOf("ū", startIndex)) != -1)
                {
                    if (index < tempOutput.Length - 1)
                    {
                        switch (tempOutput[index + 1])
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                            case 'u':
                            case 'ā':
                            case 'ē':
                            case 'ī':
                            case 'ō':
                            case 'ū':
                            case 'ʾ':
                                tempOutput = tempOutput.Remove(index, 1);
                                tempOutput = tempOutput.Insert(index, "w");
                                break;
                            default:
                                break;
                        }
                    }
                    if (index > 0)
                    {
                        switch (tempOutput[index - 1])
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                            //case 'u':
                            case 'ā':
                            case 'ē':
                            case 'ī':
                            case 'ō':
                            //case 'ū':
                            case 'ʾ':
                                tempOutput = tempOutput.Remove(index, 1);
                                tempOutput = tempOutput.Insert(index, "w");
                                break;
                            default:
                                break;
                        }
                    }

                    if ((index == 0) || Char.IsWhiteSpace(tempOutput[index - 1]))
                    {
                        if ((index == tempOutput.Length - 1) ||
                            ((index < tempOutput.Length - 1) &&
                                char.IsWhiteSpace(tempOutput[index + 1])))
                        {
                            tempOutput = tempOutput.Remove(index, 1);
                            tempOutput = tempOutput.Insert(index, "wa");
                        }
                        else
                        {
                            tempOutput = tempOutput.Remove(index, 1);
                            tempOutput = tempOutput.Insert(index, "w");
                        }
                    }

                    startIndex = index + 1;
                }

                startIndex = 0;

                while ((index = tempOutput.IndexOf("y", startIndex)) != -1)
                {
                    if (index < tempOutput.Length - 1)
                    {
                        int nextIndex = index + 1;
                        char nc = tempOutput[nextIndex];
                        if (nc == '(')
                        {
                            nextIndex = tempOutput.IndexOf(')', startIndex) + 1;
                            if (nextIndex == -1)
                                continue;
                            nc = tempOutput[nextIndex];
                        }
                        switch (nc)
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                            case 'u':
                            case 'ā':
                            case 'ē':
                            case 'ī':
                            case 'ō':
                            case 'ū':
                            case 'ʾ':
                                break;
                            default:
                                tempOutput = tempOutput.Remove(index, 1);
                                tempOutput = tempOutput.Insert(index, "ī");
                                break;
                        }
                    }
                    if (index > 0)
                    {
                        int nextIndex = index - 1;
                        char nc = tempOutput[nextIndex];
                        if (nc == ')')
                        {
                            nextIndex = tempOutput.LastIndexOf('(', index) + 1;
                            if (nextIndex == -1)
                                continue;
                            nc = tempOutput[nextIndex];
                        }
                        switch (nc)
                        {
                            case 'a':
                            case 'e':
                            case 'o':
                            case 'u':
                            case 'ā':
                            case 'ē':
                            case 'ō':
                            case 'ū':
                            case 'ʾ':
                                break;
                            case 'i':
                            case 'ī':
                                if ((index == tempOutput.Length - 1) || Char.IsWhiteSpace(tempOutput[index + 1]))
                                {
                                    tempOutput = tempOutput.Remove(index, 1);
                                    tempOutput = tempOutput.Insert(index, "ī");
                                }
                                break;
                            default:
                                //if (!Char.IsWhiteSpace(nc) && !Char.IsPunctuation(nc))
                                //{
                                //    tempOutput = tempOutput.Remove(index, 1);
                                //    tempOutput = tempOutput.Insert(index, "ī");
                                //}
                                break;
                        }
                    }
                    startIndex = index + 1;
                }

                startIndex = 0;

                while ((index = tempOutput.IndexOf("ẗ", startIndex)) != -1)
                {
                    if (index < tempOutput.Length - 1)
                    {
                        switch (tempOutput[index + 1])
                        {
                            case '.':
                            case ':':
                                tempOutput = tempOutput.Remove(index, 1);
                                tempOutput = tempOutput.Insert(index, "h");
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        tempOutput = tempOutput.Remove(index, 1);
                        tempOutput = tempOutput.Insert(index, "h");
                    }
                    startIndex = index + 1;
                }

                startIndex = 0;

                while ((index = tempOutput.IndexOf("ʾ", startIndex)) != -1)
                {
                    if (index < tempOutput.Length - 1)
                    {
                        switch (tempOutput[index + 1])
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                            case 'u':
                            case 'ā':
                            case 'ē':
                            case 'ī':
                            case 'ō':
                            case 'ū':
                                break;
                            default:
                                if (!Char.IsWhiteSpace(tempOutput[index + 1]) && !Char.IsPunctuation(tempOutput[index + 1]))
                                    tempOutput = tempOutput.Insert(index + 1, "a");
                                break;
                        }
                    }
                    startIndex = index + 1;
                }

                startIndex = 0;

                // Handle al-
                while ((index = tempOutput.IndexOf("al-", startIndex)) != -1)
                {
                    // Handle embedded al-
                    if ((index > 0) && !Char.IsWhiteSpace(tempOutput[index - 1]))
                        tempOutput = tempOutput.Remove(index + 2, 1);
                    // handle sun letters.
                    else if ((index + 3 < tempOutput.Length) && IsSunLetter(tempOutput[index + 3]))
                    {
                        string tmp = tempOutput.Substring(index + 3, 1);
                        tempOutput = tempOutput.Remove(index + 1, 1);
                        tempOutput = tempOutput.Insert(index + 1, tmp);
                    }
                    startIndex = index + 1;
                }

                while ((index = tempOutput.IndexOf("(SHADDA)")) != -1)
                {
                    tempOutput = tempOutput.Remove(index, 8);
                    char dc = tempOutput[index - 1];
                    if ((index >= 2) && (tempOutput[index - 2] == dc))
                        continue;
                    if ((index >= 3) && (tempOutput[index - 2] == '-') && (tempOutput[index - 3] == dc))
                        continue;
                    tempOutput = tempOutput.Insert(index, tempOutput[index - 1].ToString());
                }
            }

            output = tempOutput;

            return returnValue;
        }

        protected bool IsSunLetter(char c)
        {
            switch (c)
            {
                case 'ẗ':    // U+0629 \xd8\xa9 ARABIC LETTER TEH MARBUTA 'ة'
                case 't':    // U+062A \xd8\xaa ARABIC LETTER TEH 'ت'
                //case 'th':    // U+062B \xd8\xab ARABIC LETTER THEH 'ث'
                case 'd':    // U+062F \xd8\xaf ARABIC LETTER DAL 'د'
                case 'ḏ':    // U+0630 \xd8\xb0 ARABIC LETTER THAL 'ذ'
                case 'r':    // U+0631 \xd8\xb1 ARABIC LETTER REH 'ر'
                case 'z':    // U+0632 \xd8\xb2 ARABIC LETTER ZAIN 'ز'
                case 's':    // U+0633 \xd8\xb3 ARABIC LETTER SEEN 'س'
                //case 'sh':    // U+0634 \xd8\xb4 ARABIC LETTER SHEEN 'ش'
                case 'ṣ':    // U+0635 \xd8\xb5 ARABIC LETTER SAD 'ص'
                case 'ḍ':    // U+0636 \xd8\xb6 ARABIC LETTER DAD 'ض'
                case 'ṭ':    // U+0637 \xd8\xb7 ARABIC LETTER TAH 'ط'
                case 'ẓ':    // U+0638 \xd8\xb8 ARABIC LETTER ZAH 'ظ'
                case 'l':    // U+0644 \xd9\x84 ARABIC LETTER LAM 'ل'
                case 'n':    // U+0646 \xd9\x86 ARABIC LETTER NOON 'ن'
                    return true;
                default:
                    break;
            }
            return false;
        }

        public static string[] TheTablePairs =
        {
            "أَ", "ʾa",
            "أ", "ʾ",
            "اً", "ān",
            "الأ", "al-ʾ",
            "الإ", "al-a",
            "ال", "al-",
            "لأ", "lʾa",
            "لإ", "la",
            "؀", "(num)",    // U+0600 \xd8\x80 ARABIC NUMBER SIGN
            "؁", "(sanah)",    // U+0601 \xd8\x81 ARABIC SIGN SANAH
            "؂", "(footnote)",    // U+0602 \xd8\x82 ARABIC FOOTNOTE MARKER
            "؃", "(safha)",    // U+0603 \xd8\x83 ARABIC SIGN SAFHA
            "؄", "(samvat)",    // U+0604 \xd8\x84 ARABIC SIGN SAMVAT
            "؅", "(num above)",    // U+0605 \xd8\x85 ARABIC NUMBER MARK ABOVE
            "؆", "(cube root)",    // U+0606 \xd8\x86 ARABIC-INDIC CUBE ROOT
            "؇", "(fourth root)",    // U+0607 \xd8\x87 ARABIC-INDIC FOURTH ROOT
            "؈", "(ray)",    // U+0608 \xd8\x88 ARABIC RAY
            "؉", "(per mille)",    // U+0609 \xd8\x89 ARABIC-INDIC PER MILLE SIGN
            "؊", "(per 10000)",    // U+060A \xd8\x8a ARABIC-INDIC PER TEN THOUSAND SIGN
            "؋", "q",    // U+060B \xd8\x8b AFGHANI SIGN
            "،", ",",    // U+060C \xd8\x8c ARABIC COMMA
            "؍", "/",    // U+060D \xd8\x8d ARABIC DATE SEPARATOR
            "؎", "-",    // U+060E \xd8\x8e ARABIC POETIC VERSE SIGN
            "؏", "(MISRA)",    // U+060F \xd8\x8f ARABIC SIGN MISRA
            "ؐ", "(SALLALLAHOU ALAYHE WASSALLAM)", // U+0610 \xd8\x90 ARABIC SIGN SALLALLAHOU ALAYHE WASSALLAM
            "ؑ", "(ALAYHE ASSALLAM)", // U+0611 \xd8\x91 ARABIC SIGN ALAYHE ASSALLAM
            "ؒ", "(RAHMATULLAH ALAYHE)", // U+0612 \xd8\x92 ARABIC SIGN RAHMATULLAH ALAYHE
            "ؓ", "(RADI ALLAHOU ANHU)", // U+0613 \xd8\x93 ARABIC SIGN RADI ALLAHOU ANHU
            "ؔ", "(TAKHALLUS)", // U+0614 \xd8\x94 ARABIC SIGN TAKHALLUS
            "ؕ", "(TAH)", // U+0615 \xd8\x95 ARABIC SMALL HIGH TAH
            "ؖ", "l", // U+0616 \xd8\x96 ARABIC SMALL HIGH LIGATURE ALEF WITH LAM WITH YEH
            "ؗ", "z", // U+0617 \xd8\x97 ARABIC SMALL HIGH ZAIN
            "ؘ", "a", // U+0618 \xd8\x98 ARABIC SMALL FATHA
            "ؙ", "u", // U+0619 \xd8\x99 ARABIC SMALL DAMMA
            "ؚ", "i", // U+061A \xd8\x9a ARABIC SMALL KASRA
            "؛", ";",    // U+061B \xd8\x9b ARABIC SEMICOLON
            "؜", " ",    // U+061C \xd8\x9c ARABIC LETTER MARK
            "؝", "*",    // U+061D \xd8\x9d 
            "؞", ".",    // U+061E \xd8\x9e ARABIC TRIPLE DOT PUNCTUATION MARK
            "؟", "?",    // U+061F \xd8\x9f ARABIC QUESTION MARK
            "ؠ", "y",    // U+0620 \xd8\xa0 ARABIC LETTER KASHMIRI YEH
            "ء", "ʾ",    // U+0621 \xd8\xa1 ARABIC LETTER HAMZA
            "آ", "a",    // U+0622 \xd8\xa2 ARABIC LETTER ALEF WITH MADDA ABOVE
       /*dup "أ", "a",*/    // U+0623 \xd8\xa3 ARABIC LETTER ALEF WITH HAMZA ABOVE
            "ؤ", "u",    // U+0624 \xd8\xa4 ARABIC LETTER WAW WITH HAMZA ABOVE
            "إ", "i",    // U+0625 \xd8\xa5 ARABIC LETTER ALEF WITH HAMZA BELOW
            "ئ", "ʾ",    // U+0626 \xd8\xa6 ARABIC LETTER YEH WITH HAMZA ABOVE
            "ا", "ā",    // U+0627 \xd8\xa7 ARABIC LETTER ALEF
            "ب", "b",    // U+0628 \xd8\xa8 ARABIC LETTER BEH
            "ة", "ẗ",    // U+0629 \xd8\xa9 ARABIC LETTER TEH MARBUTA
            "ت", "t",    // U+062A \xd8\xaa ARABIC LETTER TEH
            "ث", "th",    // U+062B \xd8\xab ARABIC LETTER THEH
            "ج", "j",    // U+062C \xd8\xac ARABIC LETTER JEEM
            "ح", "ḥ",    // U+062D \xd8\xad ARABIC LETTER HAH
            "خ", "kh",    // U+062E \xd8\xae ARABIC LETTER KHAH
            "د", "d",    // U+062F \xd8\xaf ARABIC LETTER DAL
            "ذ", "ḏ",    // U+0630 \xd8\xb0 ARABIC LETTER THAL
            "ر", "r",    // U+0631 \xd8\xb1 ARABIC LETTER REH
            "ز", "z",    // U+0632 \xd8\xb2 ARABIC LETTER ZAIN
            "س", "s",    // U+0633 \xd8\xb3 ARABIC LETTER SEEN
            "ش", "sh",    // U+0634 \xd8\xb4 ARABIC LETTER SHEEN
            "ص", "ṣ",    // U+0635 \xd8\xb5 ARABIC LETTER SAD
            "ض", "ḍ",    // U+0636 \xd8\xb6 ARABIC LETTER DAD
            "ط", "ṭ",    // U+0637 \xd8\xb7 ARABIC LETTER TAH
            "ظ", "ẓ",    // U+0638 \xd8\xb8 ARABIC LETTER ZAH
            "ع", "ʿ",    // U+0639 \xd8\xb9 ARABIC LETTER AIN
            "غ", "gh",    // U+063A \xd8\xba ARABIC LETTER GHAIN
            "ػ", "k",    // U+063B \xd8\xbb ARABIC LETTER KEHEH WITH TWO DOTS ABOVE
            "ؼ", "k",    // U+063C \xd8\xbc ARABIC LETTER KEHEH WITH THREE DOTS BELOW
            "ؽ", "y",    // U+063D \xd8\xbd ARABIC LETTER FARSI YEH WITH INVERTED V
            "ؾ", "y",    // U+063E \xd8\xbe ARABIC LETTER FARSI YEH WITH TWO DOTS ABOVE
            "ؿ", "y",    // U+063F \xd8\xbf ARABIC LETTER FARSI YEH WITH THREE DOTS ABOVE
            "ـ", "_",    // U+0640 \xd9\x80 ARABIC TATWEEL
            "ف", "f",    // U+0641 \xd9\x81 ARABIC LETTER FEH
            "ق", "q",    // U+0642 \xd9\x82 ARABIC LETTER QAF
            "ك", "k",    // U+0643 \xd9\x83 ARABIC LETTER KAF
            "ل", "l",    // U+0644 \xd9\x84 ARABIC LETTER LAM
            "م", "m",    // U+0645 \xd9\x85 ARABIC LETTER MEEM
            "ن", "n",    // U+0646 \xd9\x86 ARABIC LETTER NOON
            "ه", "h",    // U+0647 \xd9\x87 ARABIC LETTER HEH
            "و", "ū",    // U+0648 \xd9\x88 ARABIC LETTER WAW
            "ى", "y",    // U+0649 \xd9\x89 ARABIC LETTER ALEF MAKSURA
            "ي", "y",    // U+064A \xd9\x8a ARABIC LETTER YEH
            "ً", "an", // U+064B \xd9\x8b ARABIC FATHATAN
            "ٌ", "un", // U+064C \xd9\x8c ARABIC DAMMATAN
            "ٍ", "in", // U+064D \xd9\x8d ARABIC KASRATAN
            "َ", "a", // U+064E \xd9\x8e ARABIC FATHA
            "ُ", "u", // U+064F \xd9\x8f ARABIC DAMMA
            "ِ", "i", // U+0650 \xd9\x90 ARABIC KASRA
            "ّ", "(SHADDA)", // U+0651 \xd9\x91 ARABIC SHADDA
            "ْ", "(SUKUN)", // U+0652 \xd9\x92 ARABIC SUKUN
            "ٓ", "(MADDAH ABOVE)", // U+0653 \xd9\x93 ARABIC MADDAH ABOVE
            "ٔ", "(HAMZA ABOVE)", // U+0654 \xd9\x94 ARABIC HAMZA ABOVE
            "ٕ", "(HAMZA BELOW)", // U+0655 \xd9\x95 ARABIC HAMZA BELOW
            "ٖ", "(SUBSCRIPT ALEF)", // U+0656 \xd9\x96 ARABIC SUBSCRIPT ALEF
            "ٗ", "(INVERTED DAMMA)", // U+0657 \xd9\x97 ARABIC INVERTED DAMMA
            "٘", "(NOON GHUNNA)", // U+0658 \xd9\x98 ARABIC MARK NOON GHUNNA
            "ٙ", "(ZWARAKAY)", // U+0659 \xd9\x99 ARABIC ZWARAKAY
            "ٚ", "(SMALL V ABOVE)", // U+065A \xd9\x9a ARABIC VOWEL SIGN SMALL V ABOVE
            "ٛ", "(INVERTED SMALL V ABOVE)", // U+065B \xd9\x9b ARABIC VOWEL SIGN INVERTED SMALL V ABOVE
            "ٜ", "(VOWEL SIGN DOT BELOW)", // U+065C \xd9\x9c ARABIC VOWEL SIGN DOT BELOW
            "ٝ", "(REVERSED DAMMA)", // U+065D \xd9\x9d ARABIC REVERSED DAMMA
            "ٞ", "(FATHA WITH TWO DOTS)", // U+065E \xd9\x9e ARABIC FATHA WITH TWO DOTS
            "ٟ", "(WAVY HAMZA BELOW)", // U+065F \xd9\x9f ARABIC WAVY HAMZA BELOW
            "٠", "0",    // U+0660 \xd9\xa0 ARABIC-INDIC DIGIT ZERO
            "١", "1",    // U+0661 \xd9\xa1 ARABIC-INDIC DIGIT ONE
            "٢", "2",    // U+0662 \xd9\xa2 ARABIC-INDIC DIGIT TWO
            "٣", "3",    // U+0663 \xd9\xa3 ARABIC-INDIC DIGIT THREE
            "٤", "4",    // U+0664 \xd9\xa4 ARABIC-INDIC DIGIT FOUR
            "٥", "5",    // U+0665 \xd9\xa5 ARABIC-INDIC DIGIT FIVE
            "٦", "6",    // U+0666 \xd9\xa6 ARABIC-INDIC DIGIT SIX
            "٧", "7",    // U+0667 \xd9\xa7 ARABIC-INDIC DIGIT SEVEN
            "٨", "8",    // U+0668 \xd9\xa8 ARABIC-INDIC DIGIT EIGHT
            "٩", "9",    // U+0669 \xd9\xa9 ARABIC-INDIC DIGIT NINE
            "٪", "%",    // U+066A \xd9\xaa ARABIC PERCENT SIGN
            "٫", ".",    // U+066B \xd9\xab ARABIC DECIMAL SEPARATOR
            "٬", ",",    // U+066C \xd9\xac ARABIC THOUSANDS SEPARATOR
            "٭", "*",    // U+066D \xd9\xad ARABIC FIVE POINTED STAR
            "ٮ", "b",    // U+066E \xd9\xae ARABIC LETTER DOTLESS BEH
            "ٯ", "q",    // U+066F \xd9\xaf ARABIC LETTER DOTLESS QAF
            "ٰ", "(a)", // U+0670 \xd9\xb0 ARABIC LETTER SUPERSCRIPT ALEF
            "ٱ", "a",    // U+0671 \xd9\xb1 ARABIC LETTER ALEF WASLA
            "ٲ", "a",    // U+0672 \xd9\xb2 ARABIC LETTER ALEF WITH WAVY HAMZA ABOVE
            "ٳ", "a",    // U+0673 \xd9\xb3 ARABIC LETTER ALEF WITH WAVY HAMZA BELOW
            "ٴ",  "(HIGH HAMZA)",   // U+0674 \xd9\xb4 ARABIC LETTER HIGH HAMZA
            "ٵ", "a",    // U+0675 \xd9\xb5 ARABIC LETTER HIGH HAMZA ALEF
            "ٶ", "u",    // U+0676 \xd9\xb6 ARABIC LETTER HIGH HAMZA WAW
            "ٷ", "u",    // U+0677 \xd9\xb7 ARABIC LETTER U WITH HAMZA ABOVE
            "ٸ", "y",    // U+0678 \xd9\xb8 ARABIC LETTER HIGH HAMZA YEH
            "ٹ", "t",    // U+0679 \xd9\xb9 ARABIC LETTER TTEH
            "ٺ", "t",    // U+067A \xd9\xba ARABIC LETTER TTEHEH
            "ٻ", "b",    // U+067B \xd9\xbb ARABIC LETTER BEEH
            "ټ", "t",    // U+067C \xd9\xbc ARABIC LETTER TEH WITH RING
            "ٽ", "t",    // U+067D \xd9\xbd ARABIC LETTER TEH WITH THREE DOTS ABOVE DOWNWARDS
            "پ", "p",    // U+067E \xd9\xbe ARABIC LETTER PEH
            "ٿ", "t",    // U+067F \xd9\xbf ARABIC LETTER TEHEH
            "ڀ", "b",    // U+0680 \xda\x80 ARABIC LETTER BEHEH
            "ځ", "ḥʾ",    // U+0681 \xda\x81 ARABIC LETTER HAH WITH HAMZA ABOVE
            "ڂ", "ḥ",    // U+0682 \xda\x82 ARABIC LETTER HAH WITH TWO DOTS VERTICAL ABOVE
            "ڃ", "ny",    // U+0683 \xda\x83 ARABIC LETTER NYEH
            "ڄ", "dy",    // U+0684 \xda\x84 ARABIC LETTER DYEH
            "څ", "h",    // U+0685 \xda\x85 ARABIC LETTER HAH WITH THREE DOTS ABOVE
            "چ", "tc",    // U+0686 \xda\x86 ARABIC LETTER TCHEH
            "ڇ", "tch",    // U+0687 \xda\x87 ARABIC LETTER TCHEHEH
            "ڈ", "dd",    // U+0688 \xda\x88 ARABIC LETTER DDAL
            "ډ", "d",    // U+0689 \xda\x89 ARABIC LETTER DAL WITH RING
            "ڊ", "d",    // U+068A \xda\x8a ARABIC LETTER DAL WITH DOT BELOW
            "ڋ", "d",    // U+068B \xda\x8b ARABIC LETTER DAL WITH DOT BELOW AND SMALL TAH
            "ڌ", "d",    // U+068C \xda\x8c ARABIC LETTER DAHAL
            "ڍ", "d",    // U+068D \xda\x8d ARABIC LETTER DDAHAL
            "ڎ", "du",    // U+068E \xda\x8e ARABIC LETTER DUL
            "ڏ", "d",    // U+068F \xda\x8f ARABIC LETTER DAL WITH THREE DOTS ABOVE DOWNWARDS
            "ڐ", "d",    // U+0690 \xda\x90 ARABIC LETTER DAL WITH FOUR DOTS ABOVE
            "ڑ", "rr",    // U+0691 \xda\x91 ARABIC LETTER RREH
            "ڒ", "r",    // U+0692 \xda\x92 ARABIC LETTER REH WITH SMALL V
            "ړ", "r",    // U+0693 \xda\x93 ARABIC LETTER REH WITH RING
            "ڔ", "r",    // U+0694 \xda\x94 ARABIC LETTER REH WITH DOT BELOW
            "ڕ", "r",    // U+0695 \xda\x95 ARABIC LETTER REH WITH SMALL V BELOW
            "ږ", "r",    // U+0696 \xda\x96 ARABIC LETTER REH WITH DOT BELOW AND DOT ABOVE
            "ڗ", "r",    // U+0697 \xda\x97 ARABIC LETTER REH WITH TWO DOTS ABOVE
            "ژ", "j",    // U+0698 \xda\x98 ARABIC LETTER JEH
            "ڙ", "r",    // U+0699 \xda\x99 ARABIC LETTER REH WITH FOUR DOTS ABOVE
            "ښ", "s",    // U+069A \xda\x9a ARABIC LETTER SEEN WITH DOT BELOW AND DOT ABOVE
            "ڛ", "s",    // U+069B \xda\x9b ARABIC LETTER SEEN WITH THREE DOTS BELOW
            "ڜ", "s",    // U+069C \xda\x9c ARABIC LETTER SEEN WITH THREE DOTS BELOW AND THREE DOTS ABOVE
            "ڝ", "s",    // U+069D \xda\x9d ARABIC LETTER SAD WITH TWO DOTS BELOW
            "ڞ", "s",    // U+069E \xda\x9e ARABIC LETTER SAD WITH THREE DOTS ABOVE
            "ڟ", "t",    // U+069F \xda\x9f ARABIC LETTER TAH WITH THREE DOTS ABOVE
            "ڠ", "a",    // U+06A0 \xda\xa0 ARABIC LETTER AIN WITH THREE DOTS ABOVE
            "ڡ", "f",    // U+06A1 \xda\xa1 ARABIC LETTER DOTLESS FEH
            "ڢ", "f",    // U+06A2 \xda\xa2 ARABIC LETTER FEH WITH DOT MOVED BELOW
            "ڣ", "f",    // U+06A3 \xda\xa3 ARABIC LETTER FEH WITH DOT BELOW
            "ڤ", "v",    // U+06A4 \xda\xa4 ARABIC LETTER VEH
            "ڥ", "f",    // U+06A5 \xda\xa5 ARABIC LETTER FEH WITH THREE DOTS BELOW
            "ڦ", "p",    // U+06A6 \xda\xa6 ARABIC LETTER PEHEH
            "ڧ", "q",    // U+06A7 \xda\xa7 ARABIC LETTER QAF WITH DOT ABOVE
            "ڨ", "q",    // U+06A8 \xda\xa8 ARABIC LETTER QAF WITH THREE DOTS ABOVE
            "ک", "k",    // U+06A9 \xda\xa9 ARABIC LETTER KEHEH
            "ڪ", "k",    // U+06AA \xda\xaa ARABIC LETTER SWASH KAF
            "ګ", "k",    // U+06AB \xda\xab ARABIC LETTER KAF WITH RING
            "ڬ", "k",    // U+06AC \xda\xac ARABIC LETTER KAF WITH DOT ABOVE
            "ڭ", "k",    // U+06AD \xda\xad ARABIC LETTER NG
            "ڮ", "k",    // U+06AE \xda\xae ARABIC LETTER KAF WITH THREE DOTS BELOW
            "گ", "g",    // U+06AF \xda\xaf ARABIC LETTER GAF
            "ڰ", "g",    // U+06B0 \xda\xb0 ARABIC LETTER GAF WITH RING
            "ڱ", "ng",    // U+06B1 \xda\xb1 ARABIC LETTER NGOEH
            "ڲ", "g",    // U+06B2 \xda\xb2 ARABIC LETTER GAF WITH TWO DOTS BELOW
            "ڳ", "gu",    // U+06B3 \xda\xb3 ARABIC LETTER GUEH
            "ڴ", "g",    // U+06B4 \xda\xb4 ARABIC LETTER GAF WITH THREE DOTS ABOVE
            "ڵ", "l",    // U+06B5 \xda\xb5 ARABIC LETTER LAM WITH SMALL V
            "ڶ", "l",    // U+06B6 \xda\xb6 ARABIC LETTER LAM WITH DOT ABOVE
            "ڷ", "l",    // U+06B7 \xda\xb7 ARABIC LETTER LAM WITH THREE DOTS ABOVE
            "ڸ", "l",    // U+06B8 \xda\xb8 ARABIC LETTER LAM WITH THREE DOTS BELOW
            "ڹ", "n",    // U+06B9 \xda\xb9 ARABIC LETTER NOON WITH DOT BELOW
            "ں", "n",    // U+06BA \xda\xba ARABIC LETTER NOON GHUNNA
            "ڻ", "n",    // U+06BB \xda\xbb ARABIC LETTER RNOON
            "ڼ", "n",    // U+06BC \xda\xbc ARABIC LETTER NOON WITH RING
            "ڽ", "n",    // U+06BD \xda\xbd ARABIC LETTER NOON WITH THREE DOTS ABOVE
            "ھ", "h",    // U+06BE \xda\xbe ARABIC LETTER HEH DOACHASHMEE
            "ڿ", "tch",    // U+06BF \xda\xbf ARABIC LETTER TCHEH WITH DOT ABOVE
            "ۀ", "h",    // U+06C0 \xdb\x80 ARABIC LETTER HEH WITH YEH ABOVE
            "ہ", "h",    // U+06C1 \xdb\x81 ARABIC LETTER HEH GOAL
            "ۂ", "h",    // U+06C2 \xdb\x82 ARABIC LETTER HEH GOAL WITH HAMZA ABOVE
            "ۃ", "h",    // U+06C3 \xdb\x83 ARABIC LETTER TEH MARBUTA GOAL
            "ۄ", "u",    // U+06C4 \xdb\x84 ARABIC LETTER WAW WITH RING
            "ۅ", "u",    // U+06C5 \xdb\x85 ARABIC LETTER KIRGHIZ OE
            "ۆ", "u",    // U+06C6 \xdb\x86 ARABIC LETTER OE
            "ۇ", "u",    // U+06C7 \xdb\x87 ARABIC LETTER U
            "ۈ", "u",    // U+06C8 \xdb\x88 ARABIC LETTER YU
            "ۉ", "u",    // U+06C9 \xdb\x89 ARABIC LETTER KIRGHIZ YU
            "ۊ", "u",    // U+06CA \xdb\x8a ARABIC LETTER WAW WITH TWO DOTS ABOVE
            "ۋ", "v",    // U+06CB \xdb\x8b ARABIC LETTER VE
            "ی", "y",    // U+06CC \xdb\x8c ARABIC LETTER FARSI YEH
            "ۍ", "y",    // U+06CD \xdb\x8d ARABIC LETTER YEH WITH TAIL
            "ێ", "y",    // U+06CE \xdb\x8e ARABIC LETTER YEH WITH SMALL V
            "ۏ", "u",    // U+06CF \xdb\x8f ARABIC LETTER WAW WITH DOT ABOVE
            "ې", "e",    // U+06D0 \xdb\x90 ARABIC LETTER E
            "ۑ", "y",    // U+06D1 \xdb\x91 ARABIC LETTER YEH WITH THREE DOTS BELOW
            "ے", "y",    // U+06D2 \xdb\x92 ARABIC LETTER YEH BARREE
            "ۓ", "y",    // U+06D3 \xdb\x93 ARABIC LETTER YEH BARREE WITH HAMZA ABOVE
            "۔", "'",    // U+06D4 \xdb\x94 ARABIC FULL STOP
            "ە", "ae",    // U+06D5 \xdb\x95 ARABIC LETTER AE
            "ۖ", "(SMALL HIGH LIGATURE SAD WITH LAM WITH ALEF MAKSURA)", // U+06D6 \xdb\x96 ARABIC SMALL HIGH LIGATURE SAD WITH LAM WITH ALEF MAKSURA
            "ۗ", "(SMALL HIGH LIGATURE QAF WITH LAM WITH ALEF MAKSURA)", // U+06D7 \xdb\x97 ARABIC SMALL HIGH LIGATURE QAF WITH LAM WITH ALEF MAKSURA
            "ۘ", "(SMALL HIGH MEEM INITIAL FORM)", // U+06D8 \xdb\x98 ARABIC SMALL HIGH MEEM INITIAL FORM
            "ۙ", "(SMALL HIGH LAM ALEF)", // U+06D9 \xdb\x99 ARABIC SMALL HIGH LAM ALEF
            "ۚ", "(SMALL HIGH JEEM)", // U+06DA \xdb\x9a ARABIC SMALL HIGH JEEM
            "ۛ", "(SMALL HIGH THREE DOTS)", // U+06DB \xdb\x9b ARABIC SMALL HIGH THREE DOTS
            "ۜ", "(SMALL HIGH SEEN)", // U+06DC \xdb\x9c ARABIC SMALL HIGH SEEN
            "۝", "(END OF AYAH)",    // U+06DD \xdb\x9d ARABIC END OF AYAH
            "۞", "(START OF RUB EL HIZB)",    // U+06DE \xdb\x9e ARABIC START OF RUB EL HIZB
            "۟", "(SMALL HIGH ROUNDED ZERO)", // U+06DF \xdb\x9f ARABIC SMALL HIGH ROUNDED ZERO
            "۠", "(SMALL HIGH UPRIGHT RECTANGULAR ZERO)", // U+06E0 \xdb\xa0 ARABIC SMALL HIGH UPRIGHT RECTANGULAR ZERO
            "ۡ", "(SMALL HIGH DOTLESS HEAD OF KHAH)", // U+06E1 \xdb\xa1 ARABIC SMALL HIGH DOTLESS HEAD OF KHAH
            "ۢ", "(SMALL HIGH MEEM ISOLATED FORM)", // U+06E2 \xdb\xa2 ARABIC SMALL HIGH MEEM ISOLATED FORM
            "ۣ", "(SMALL LOW SEEN)", // U+06E3 \xdb\xa3 ARABIC SMALL LOW SEEN
            "ۤ", "(SMALL HIGH MADDA)", // U+06E4 \xdb\xa4 ARABIC SMALL HIGH MADDA
            "ۥ",  "(SMALL WAW)",   // U+06E5 \xdb\xa5 ARABIC SMALL WAW
            "ۦ",  "(SMALL YEH)",   // U+06E6 \xdb\xa6 ARABIC SMALL YEH
            "ۧ", "(SMALL HIGH YEH)", // U+06E7 \xdb\xa7 ARABIC SMALL HIGH YEH
            "ۨ", "(SMALL HIGH NOON)", // U+06E8 \xdb\xa8 ARABIC SMALL HIGH NOON
            "۩", "(PLACE OF SAJDAH)",    // U+06E9 \xdb\xa9 ARABIC PLACE OF SAJDAH
            "۪", "(EMPTY CENTRE LOW STOP)", // U+06EA \xdb\xaa ARABIC EMPTY CENTRE LOW STOP
            "۫", "(EMPTY CENTRE HIGH STOP)", // U+06EB \xdb\xab ARABIC EMPTY CENTRE HIGH STOP
            "۬", "()", // U+06EC \xdb\xac ARABIC ROUNDED HIGH STOP WITH FILLED CENTRE
            "ۭ", "()", // U+06ED \xdb\xad ARABIC SMALL LOW MEEM
            "ۮ", "d",    // U+06EE \xdb\xae ARABIC LETTER DAL WITH INVERTED V
            "ۯ", "r",    // U+06EF \xdb\xaf ARABIC LETTER REH WITH INVERTED V
            "۰", "0",    // U+06F0 \xdb\xb0 EXTENDED ARABIC-INDIC DIGIT ZERO
            "۱", "1",    // U+06F1 \xdb\xb1 EXTENDED ARABIC-INDIC DIGIT ONE
            "۲", "2",    // U+06F2 \xdb\xb2 EXTENDED ARABIC-INDIC DIGIT TWO
            "۳", "3",    // U+06F3 \xdb\xb3 EXTENDED ARABIC-INDIC DIGIT THREE
            "۴", "4",    // U+06F4 \xdb\xb4 EXTENDED ARABIC-INDIC DIGIT FOUR
            "۵", "5",    // U+06F5 \xdb\xb5 EXTENDED ARABIC-INDIC DIGIT FIVE
            "۶", "6",    // U+06F6 \xdb\xb6 EXTENDED ARABIC-INDIC DIGIT SIX
            "۷", "7",    // U+06F7 \xdb\xb7 EXTENDED ARABIC-INDIC DIGIT SEVEN
            "۸", "8",    // U+06F8 \xdb\xb8 EXTENDED ARABIC-INDIC DIGIT EIGHT
            "۹", "9",    // U+06F9 \xdb\xb9 EXTENDED ARABIC-INDIC DIGIT NINE
            "ۺ", "sh",    // U+06FA \xdb\xba ARABIC LETTER SHEEN WITH DOT BELOW
            "ۻ", "d",    // U+06FB \xdb\xbb ARABIC LETTER DAD WITH DOT BELOW
            "ۼ", "gh",    // U+06FC \xdb\xbc ARABIC LETTER GHAIN WITH DOT BELOW
            "۽", "&",    // U+06FD \xdb\xbd ARABIC SIGN SINDHI AMPERSAND
            "۾", "(SINDHI POSTPOSITION MEN)",    // U+06FE \xdb\xbe ARABIC SIGN SINDHI POSTPOSITION MEN
            "ۿ", "h",    // U+06FF \xdb\xbf ARABIC LETTER HEH WITH INVERTED V
            "ݐ", "p",    // U+0750 dd 90 ARABIC LETTER BEH WITH THREE DOTS HORIZONTALLY BELOW
            "ݑ", "p",    // U+0751 dd 91 ARABIC LETTER BEH WITH DOT BELOW AND THREE DOTS ABOVE
            "ݒ", "p",    // U+0752 dd 92 ARABIC LETTER BEH WITH THREE DOTS POINTING UPWARDS BELOW
            "ݓ", "p",    // U+0753 dd 93 ARABIC LETTER BEH WITH THREE DOTS POINTING UPWARDS BELOW AND TWO DOTS ABOVE
            "ݔ", "b",    // U+0754 dd 94 ARABIC LETTER BEH WITH TWO DOTS BELOW AND DOT ABOVE
            "ݕ", "b",    // U+0755 dd 95 ARABIC LETTER BEH WITH INVERTED SMALL V BELOW
            "ݖ", "b",    // U+0756 dd 96 ARABIC LETTER BEH WITH SMALL V
            "ݗ", "h",    // U+0757 dd 97 ARABIC LETTER HAH WITH TWO DOTS ABOVE
            "ݘ", "h",    // U+0758 dd 98 ARABIC LETTER HAH WITH THREE DOTS POINTING UPWARDS BELOW
            "ݙ", "d",    // U+0759 dd 99 ARABIC LETTER DAL WITH TWO DOTS VERTICALLY BELOW AND SMALL TAH
            "ݚ", "d",    // U+075A dd 9a ARABIC LETTER DAL WITH INVERTED SMALL V BELOW
            "ݛ", "r",    // U+075B dd 9b ARABIC LETTER REH WITH STROKE
            "ݜ", "s",    // U+075C dd 9c ARABIC LETTER SEEN WITH FOUR DOTS ABOVE
            "ݝ", "a",    // U+075D dd 9d ARABIC LETTER AIN WITH TWO DOTS ABOVE
            "ݞ", "a",    // U+075E dd 9e ARABIC LETTER AIN WITH THREE DOTS POINTING DOWNWARDS ABOVE
            "ݟ", "a",    // U+075F dd 9f ARABIC LETTER AIN WITH TWO DOTS VERTICALLY ABOVE
            "ݠ", "f",    // U+0760 dd a0 ARABIC LETTER FEH WITH TWO DOTS BELOW
            "ݡ", "f",    // U+0761 dd a1 ARABIC LETTER FEH WITH THREE DOTS POINTING UPWARDS BELOW
            "ݢ", "k",    // U+0762 dd a2 ARABIC LETTER KEHEH WITH DOT ABOVE
            "ݣ", "k",    // U+0763 dd a3 ARABIC LETTER KEHEH WITH THREE DOTS ABOVE
            "ݤ", "k",    // U+0764 dd a4 ARABIC LETTER KEHEH WITH THREE DOTS POINTING UPWARDS BELOW
            "ݥ", "m",    // U+0765 dd a5 ARABIC LETTER MEEM WITH DOT ABOVE
            "ݦ", "m",    // U+0766 dd a6 ARABIC LETTER MEEM WITH DOT BELOW
            "ݧ", "n",    // U+0767 dd a7 ARABIC LETTER NOON WITH TWO DOTS BELOW
            "ݨ", "n",    // U+0768 dd a8 ARABIC LETTER NOON WITH SMALL TAH
            "ݩ", "n",    // U+0769 dd a9 ARABIC LETTER NOON WITH SMALL V
            "ݪ", "l",    // U+076A dd aa ARABIC LETTER LAM WITH BAR
            "ݫ", "r",    // U+076B dd ab ARABIC LETTER REH WITH TWO DOTS VERTICALLY ABOVE
            "ݬ", "r",    // U+076C dd ac ARABIC LETTER REH WITH HAMZA ABOVE
            "ݭ", "s",    // U+076D dd ad ARABIC LETTER SEEN WITH TWO DOTS VERTICALLY ABOVE
            "ݮ", "h",    // U+076E dd ae ARABIC LETTER HAH WITH SMALL ARABIC LETTER TAH BELOW
            "ݯ", "h",    // U+076F dd af ARABIC LETTER HAH WITH SMALL ARABIC LETTER TAH AND TWO DOTS
            "ݰ", "s",    // U+0770 dd b0 ARABIC LETTER SEEN WITH SMALL ARABIC LETTER TAH AND TWO DOTS
            "ݱ", "r",    // U+0771 dd b1 ARABIC LETTER REH WITH SMALL ARABIC LETTER TAH AND TWO DOTS
            "ݲ", "h",    // U+0772 dd b2 ARABIC LETTER HAH WITH SMALL ARABIC LETTER TAH ABOVE
            "ݳ", "a",    // U+0773 dd b3 ARABIC LETTER ALEF WITH EXTENDED ARABIC-INDIC DIGIT TWO ABOVE
            "ݴ", "a",    // U+0774 dd b4 ARABIC LETTER ALEF WITH EXTENDED ARABIC-INDIC DIGIT THREE ABOVE
            "ݵ", "y",    // U+0775 dd b5 ARABIC LETTER FARSI YEH WITH EXTENDED ARABIC-INDIC DIGIT TWO ABOVE
            "ݶ", "y",    // U+0776 dd b6 ARABIC LETTER FARSI YEH WITH EXTENDED ARABIC-INDIC DIGIT THREE ABOVE
            "ݷ", "y",    // U+0777 dd b7 ARABIC LETTER FARSI YEH WITH EXTENDED ARABIC-INDIC DIGIT FOUR BELOW
            "ݸ", "u",    // U+0778 dd b8 ARABIC LETTER WAW WITH EXTENDED ARABIC-INDIC DIGIT TWO ABOVE
            "ݹ", "u",    // U+0779 dd b9 ARABIC LETTER WAW WITH EXTENDED ARABIC-INDIC DIGIT THREE ABOVE
            "ݺ", "y",    // U+077A dd ba ARABIC LETTER YEH BARREE WITH EXTENDED ARABIC-INDIC DIGIT TWO ABOVE
            "ݻ", "y",    // U+077B dd bb ARABIC LETTER YEH BARREE WITH EXTENDED ARABIC-INDIC DIGIT THREE ABOVE
            "ݼ", "h",    // U+077C dd bc ARABIC LETTER HAH WITH EXTENDED ARABIC-INDIC DIGIT FOUR BELOW
            "ݽ", "s",    // U+077D dd bd ARABIC LETTER SEEN WITH EXTENDED ARABIC-INDIC DIGIT FOUR ABOVE
            "ݾ", "s",    // U+077E dd be ARABIC LETTER SEEN WITH INVERTED V
            "ݿ", "k"     // U+077F dd bf ARABIC LETTER KAF WITH TWO DOTS ABOVE

            /*
            "ال", "al-",
            "لإ", "la",
            "ا", "?",
            "ا", "?",
            "ا", "?",
            "ا", "?",
            "أ", "a",
            "إ", "e",
            "ﺏ", "b",
            "ﺐ", "b",
            "ﺒ", "b",
            "ﺑ", "b",
            "ﺕ", "t",
            "ﺖ", "t",
            "ﺘ", "t",
            "ﺗ", "t",
            "ﺙ", "th",
            "ﺚ", "th",
            "ﺜ", "th",
            "ﺛ", "th",
            "ﺝ", "j",
            "ﺞ", "j",
            "ﺠ", "j",
            "ﺟ", "j",
            "ﺡ", "ḥ",
            "ﺢ", "ḥ",
            "ﺤ", "ḥ",
            "ﺣ", "ḥ",
            "ﺥ", "kh",
            "ﺦ", "kh",
            "ﺨ", "kh",
            "ﺧ", "kh",
            "ﺩ", "d",
            "ﺪ", "d",
            "ﺪ", "d",
            "ﺩ", "d",
            "ﺫ", "dh",
            "ﺬ", "dh",
            "ﺬ", "dh",
            "ﺫ", "dh",
            "ﺭ", "r",
            "ﺮ", "r",
            "ﺮ", "r",
            "ﺭ", "r",
            "ﺯ", "z",
            "ﺰ", "z",
            "ﺰ", "z",
            "ﺯ", "z",
            "ﺱ", "s",
            "ﺲ", "s",
            "ﺴ", "s",
            "ﺳ", "s",
            "ﺵ", "sh",
            "ﺶ", "sh",
            "ﺸ", "sh",
            "ﺷ", "sh",
            "ﺹ", "ṣ",
            "ﺺ", "ṣ",
            "ﺼ", "ṣ",
            "ﺻ", "ṣ",
            "ﺽ", "ḍ",
            "ﺾ", "ḍ",
            "ﻀ", "ḍ",
            "ﺿ", "ḍ",
            "ﻁ", "ṭ",
            "ﻂ", "ṭ",
            "ﻄ", "ṭ",
            "ﻃ", "ṭ",
            "ﻅ", "ẓ",
            "ﻆ", "ẓ",
            "ﻈ", "ẓ",
            "ﻇ", "ẓ",
            "ﻉ", "‘",
            "ﻊ", "‘",
            "ﻌ", "‘",
            "ﻋ", "‘",
            "ﻍ", "gh",
            "ﻎ", "gh",
            "ﻐ", "gh",
            "ﻏ", "gh",
            "ﻑ", "f",
            "ﻒ", "f",
            "ﻔ", "f",
            "ﻓ", "f",
            "ﻕ", "q",
            "ﻖ", "q",
            "ﻘ", "q",
            "ﻗ", "q",
            "ﻙ", "k",
            "ﻚ", "k",
            "ﻜ", "k",
            "ﻛ", "k",
            "ﻝ", "l",
            "ﻞ", "l",
            "ﻠ", "l",
            "ﻟ", "l",
            "ﻡ", "m",
            "ﻢ", "m",
            "ﻤ", "m",
            "ﻣ", "m",
            "ﻥ", "n",
            "ﻦ", "n",
            "ﻨ", "n",
            "ﻧ", "n",
            "ـﺔ", "h",
            "ﻬ", "h",
            "ﻫ", "h",
            "ة", "h",
            "ه", "h",
            "ﻪ", "h",
            "ﻭ", "w",
            "ﻮ", "w",
            "ﻮ", "w",
            "ﻭ", "w",
            "ي", "y",
            "ي", "y",
            "ﻴ", "y",
            "ﻳ", "y",
            "گ", "g",
            "چ", "c",
            "ڤ", "v",
            "ڴ", "ñ",
            "چ", "zh",
            "ۋ", "v",
            "پ", "p",
            "ژ", "zh",
            "ڥ", "v"
            */
        };

        public static Dictionary<string, string> TheTableDictionary
        {
            get
            {
                if (_TheTableDictionary == null)
                    _TheTableDictionary = GetDictionaryFromTablePairs(TheTablePairs);

                return _TheTableDictionary;
            }
            set
            {
                _TheTableDictionary = value;
            }
        }

        public static char[] TheCharacters
        {
            get
            {
                if (_TheCharacters == null)
                    _TheCharacters = GetCharactersFromTablePairs(TheTablePairs);

                return _TheCharacters;
            }
            set
            {
                _TheCharacters = value;
            }
        }

        public static HashSet<char> TheCharacterSet
        {
            get
            {
                if (_TheCharacterSet == null)
                    _TheCharacterSet = GetCharacterSetFromCharacters(TheCharacters);

                return _TheCharacterSet;
            }
            set
            {
                _TheCharacterSet = value;
            }
        }

        public static int TheMaxInputLength
        {
            get
            {
                if (_TheMaxInputLength == -1)
                    _TheMaxOutputLength = GetMaxInputLengthFromTablePairs(TheTablePairs);

                return _TheMaxInputLength;
            }
            set
            {
                _TheMaxInputLength = value;
            }
        }

        public static int TheMaxOutputLength
        {
            get
            {
                if (_TheMaxOutputLength == -1)
                    _TheMaxOutputLength = GetMaxOutputLengthFromTablePairs(TheTablePairs);

                return _TheMaxOutputLength;
            }
            set
            {
                _TheMaxOutputLength = value;
            }
        }
    }
}

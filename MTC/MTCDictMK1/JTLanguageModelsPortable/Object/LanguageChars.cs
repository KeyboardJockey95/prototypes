using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Object
{
    public static partial class LanguageLookup
    {
        public static char[] NewLine = new char[] { '\n' };
        public static char[] CrLf = new char[] { '\r', '\n' };
        public static char[] Space = new char[] { ' ' };
        public static char[] Tab = new char[] { '\t' };
        public static char[] Dot = new char[] { '.' };
        public static char[] Comma = new char[] { ',' };
        public static char[] Commas = new char[] { ',', '，', '、' };
        public static string[] CommaStrings = new string[] { ",", "，", "、" };
        public static char[] Colon = new char[] { ':' };
        public static char[] Semicolon = new char[] { ';' };
        public static char[] SemicolonChars = new char[] { ';', '；' };
        public static string[] SemicolonMatchFamily = new string[] { ";", ",", "，", "、" };
        public static char[] Slash = new char[] { '/' };
        public static char[] BackSlash = new char[] { '\\' };
        public static char[] Assigns = new char[] { '=' };
        public static char[] Bar = new char[] { '|' };
        public static char[] Pound = new char[] { '#' };
        public static char[] Underscore = new char[] { '_' };
        public static char[] EmDash = new char[] { '—' };
        public static char[] PeriodCharacters = new char[] { '.', '。', '…', '．' };
        public static char[] PunctuationCharacters = new char[] { '.', ',', '。', '．', '，', ';', '；', '：', ':', '-', '"', '“', '”', '\'', '‘', '’', '?', '¿', '？', '!', '！', '(', '（', ')', '）', '[', '【', ']', '】', '「', '」', '『', '』', '《', '》', '»', '«', '›', '‹', '【', '】', '〖', '〗', '〔', '〕', '…', '－', '—', '、', '\\', '・', '¡', '―' };
        public static char[] NonMatchedPunctuationCharacters = new char[] { '.', ',', '。', '．', '，', ';', '；', '：', ':', '-', '?', '¿', '？', '!', '！', '…', '－', '—', '、', '\\', '・', '―' };
        public static char[] SentenceTerminatorCharacters = new char[] { '.', '。', '．', '?', '？', '!', '！', '…', /*'－', '—',*/ ';', '；' , ':', '：', '\x200E' /* left-to-right marker */ };
        public static char[] RawSentenceParsingPunctuationCharacters = new char[] { /* non-fat */ '.', ';', ':', /* '-' check for second '-', */ '!', '?', '—', /* fat */ '。', '．', '；', '：', '？', '！', '－', /* maybe this too */ '…' };
        public static string[] RawSentenceParsingPunctuationStrings = new string[] { /* non-fat */ ".", ";", ":", "--", "–", "!", "?", "—", /* fat */ "。", "．", "；", "：", "？", "！", "－", /* maybe this too */ "…" };
        public static string[] MatchSentenceParsingPunctuationStrings = new string[] { /* non-fat */ ".", ",", ";", ":", "--", "–", "!", "?", "—", /* fat */ "。", "，", "．", "；", "：", "？", "！", "－", /* maybe this too */ "…" };
        public static char[] SentenceStartMarkers = { '\x200E' /* left-to-right marker */ };
        public static char[] ColonCharacters = new char[] { ':', '：' };
        public static char[] ColonCharacter = new char[] { ':' };
        public static char[] DashCharacters = new char[] { '-', '－', '―' };
        public static char[] DoubleQuote = new char[] { '"' };
        public static char[] DoubleQuoteCharacters = new char[] { '"', '“', '”', '„', '»', '«', '›', '‹' };
        public static char[] MatchedCharacters = new char[] { '"', '“', '”', '„', '\'', '‘', '’', '‚', '(', '（', ')', '）', '[', '【', ']', '】', '「', '」', '『', '』', '《', '》', '»', '«', '›', '‹', '【', '】', '〖', '〗', '〔', '〕' };
        public static char[] MatchedStartCharacters = new char[] { '"', '“', '„', '\'', '‘', '(', '（', '[', '【', '「', '『', '《', '»', '›', '【', '〖', '〔' };
        public static char[] MatchedFatStartCharacters = new char[] { '（', '【', '「', '『', '《', '【', '〖', '〔' };
        public static char[] MatchedEndCharacters = new char[] { '"', '”', '\'', '’', ')', '）', ']', '】', '」',  '』', '》', '«', '‹' };
        public static char[] MatchedFatEndCharacters = new char[] { '）', '】', '」', '』', '》' };
        public static char[] MatchedAsideCharacters = new char[] { '(', '（', '[', '【', '「', '『', '《', '«', '‹' };
        public static char[] MatchedSpeechAsideCharacters = new char[] { '(', '（', '[', '{' };
        public static char[] PunctuationNeedingSpaceAfter = new char[] { '.', ',', ';', ':', '”', '’', '?', '!', ')', ']', '…'};
        public static char[] PunctuationNeedingSpaceBefore = new char[] { '“', '‘', '(', '（', '[', '【', '「', '『', '《', '«', '‹' };
        public static char[] ParenthesisOpenCharacters = new char[] { '(', '（' };
        public static char[] ParenthesisCloseCharacters = new char[] { ')', '）' };
        public static char[] ParenthesisCharacters = new char[] { '(', ')' };
        public static char[] FileNameKeySeparatorCharacters = new char[] { '_', '.' };
        public static char[] DigitSeparatorCharacters = new char[] { ' ', '-' };

        // These next four arrays need to be kept in sync with each other.
        public static string[] FatPunctuationCharacters = new string[]           {  "。",  "，", "．", "：", "；", "？", "¿",      "！", "¡", "（", "）",    "【", "】", "\"", "\"",       "“", "”",  "'",   "'",  "‘",  "’",       "…", "、",  "－",  "—",  "\\", "/", "・" };
        public static string[] FatPunctuationWithSpaceCharacters = new string[]  { "。 ", "， ", "． ", "： ", "； ", "？ ", "¿ ", "！ ", "¡", " （", "） ", " 【", "】 ", " \"", "\" ",  " “", "” ", " '", "' ", " ‘", "’ ",       "…", "、 ", "－",  "—",  "\\", "/", "・" };
        public static string[] ThinPunctuationCharacters = new string[]          {   ".",   ",", ".", ":", ";", "?", "¿",         "!", "¡", "(", ")",      "[", "]", "\"", "\"",        "“", "”",  "'",   "'",  "‘",  "’",       "…", ",",   "-",   "-",  "\\", "/", "-" };
        public static string[] ThinPunctuationWithSpaceCharacters = new string[] {  ". ",  ", ", ". ", ": ", "; ", "? ", " ¿",    "! ", " ¡", " (", ") ",  " [", "] ", " \"", "\" ",    "“", "”",  " '", "' ",  "‘",  "’",       "…", ", ",  " - ", "-", "\\", "/", "-" };

        public static string[] FatPunctuationConversion = new string[] { "。", ". ", "．", ". ", "，", ", ", "：", ": ", "；", "; ",
            "？", "? ", "！", "! ", "（", " (", "）", ") ", "【", " [", "】", "] ", "「", " \"", "」", "\" ", "『", " \"", "』", "《", "》", "\" ", "・", " ", "　", " " };

        public static char[] NeedSpaceBeforeCharacters = new char[] { '“', '„', '‘', '(', '[' };
        public static char[] NeedSpaceAfterCharacters = new char[] { '.', ',', '.', ':', ';', '?', '!', ')', ']', '”', '’', '…', ','};

        public static char[] NonAlphanumericCharacters = new char[] {
            '.',
            ',',
            '。',
            '．',
            '，',
            ';',
            '；',
            ':',
            '：',
            '-',
            '"',
            '“',
            '”',
            '\'',
            '‘',
            '’',
            '?',
            '¿',
            '？',
            '!',
            '¡',
            '！',
            '@',
            '#',
            '$',
            '%',
            '^',
            '&',
            '*',
            '(',
            ')',
            '-',
            '_',
            '=',
            '+',
            '[',
            ']',
            '{',
            '}',
            '<',
            '>',
            '/',
            '\\',
            '~',
            '「',
            '」',
            '『',
            '』',
            '《',
            '》',
            '»',
            '«',
            '›',
            '‹',
            '【',
            '】',
            '〖',
            '〗',
            '〔',
            '〕',
            '…',
            '－',
            '—',
            '、',
            '\\',
            '・',
            '·',
            '―'
        };

        public static char[] SpaceAndPunctuationCharacters = new char[]
        {
            // ' ',
            '\x0020',
            '\x00A0',
            '\x1680',
            '\x180E',
            '\x2000',
            '\x2001',
            '\x2002',
            '\x2003',
            '\x2004',
            '\x2005',
            '\x2006',
            '\x2007',
            '\x2008',
            '\x2009',
            '\x200A',
            '\x200B',
            '\x200E',   // Left-to-right marker
            '\x202F',
            '\x205F',
            '\x3000',
            '\xFEFF',
            '\t',
            '\r',
            '\n',
            '\b',
            '\f',
            '.',
            ',',
            '。',
            '．',
            '，',
            ';',
            '；',
            '：',
            ':',
            '-',
            '"',
            '“',
            '”',
            '\'',
            '‘',
            '’',
            '?',
            '¿',
            '？',
            '!',
            '¡',
            '！',
            '(',
            '（',
            ')',
            '）',
            '[',
            '【',
            ']',
            '】',
            '「',
            '」',
            '『',
            '』',
            '《',
            '》',
            '»',
            '«',
            '›',
            '‹',
            '【',
            '】',
            '〖',
            '〗',
            '〔',
            '〕',
            '…',
            '－',
            '—',
            '―',
            '、',
            '\\',
            '~',
            '～',
            '・',
            '·'
            // '　'
        };

        public static List<string> SpaceAndPunctuationCharactersList = new List<string>
        {
            // " ",
            "\x0020",
            "\x00A0",
            "\x1680",
            "\x180E",
            "\x2000",
            "\x2001",
            "\x2002",
            "\x2003",
            "\x2004",
            "\x2005",
            "\x2006",
            "\x2007",
            "\x2008",
            "\x2009",
            "\x200A",
            "\x200B",
            "\x200E",   // Left-to-right marker
            "\x202F",
            "\x205F",
            "\x3000",
            "\xFEFF",
            "\t",
            "\r",
            "\n",
            "\b",
            "\f",
            ".",
            ",",
            "。",
            "．",
            "，",
            ";",
            "；",
            "：",
            ":",
            "-",
            "\"",
            "“",
            "”",
            "'",
            "‘",
            "’",
            "?",
            "¿",
            "？",
            "!",
            "¡",
            "！",
            "(",
            "（",
            ")",
            "）",
            "[",
            "【",
            "]",
            "】",
            "「",
            "」",
            "『",
            "』",
            "《",
            "》",
            "»",
            "«",
            "›",
            "‹",
            "【",
            "】",
            "〖",
            "〗",
            "〔",
            "〕",
            "…",
            "－",
            "—",
            "、",
            "\\",
            "~",
            "～",
            "・",
            "·",
            "―",
            // "　"
        };

        public static char[] JustSpaces = new char[]
        {
            '\x0020',
            '\x00A0',
            '\x1680',
            '\x180E',
            '\x2000',
            '\x2001',
            '\x2002',
            '\x2003',
            '\x2004',
            '\x2005',
            '\x2006',
            '\x2007',
            '\x2008',
            '\x2009',
            '\x200A',
            '\x200B',
            '\x200E',   // Left-to-right marker
            '\x202F',
            '\x205F',
            '\x3000',
            '\xFEFF'
        };

        public static char[] SpaceCharacter = new char[]
        {
            ' '
        };

        public static char[] SpaceCharacters = new char[]
        {
            // ' '
            '\x0020',
            '\x00A0',
            '\x1680',
            '\x180E',
            '\x2000',
            '\x2001',
            '\x2002',
            '\x2003',
            '\x2004',
            '\x2005',
            '\x2006',
            '\x2007',
            '\x2008',
            '\x2009',
            '\x200A',
            '\x200B',
            '\x200E',   // Left-to-right marker
            '\x202F',
            '\x205F',
            '\x3000',
            '\xFEFF',
            '\t',
            '\r',
            '\n',
            '\b',
            '\f'
            // '　'
        };

        public static string[] SpaceStrings = new string[]
        {
            // " ",
            "\x0020",
            "\x00A0",
            "\x1680",
            "\x180E",
            "\x2000",
            "\x2001",
            "\x2002",
            "\x2003",
            "\x2004",
            "\x2005",
            "\x2006",
            "\x2007",
            "\x2008",
            "\x2009",
            "\x200A",
            "\x200B",
            "\x200E",   // Left-to-right marker
            "\x202F",
            "\x205F",
            "\x3000",
            "\xFEFF",
            "\t",
            "\r",
            "\n",
            "\b",
            "\f"
            //"　"
        };

        public static char ZeroWidthSpace = '\x200B';
        public static char ZeroWidthNoBreakSpace = '\xFEFF';
        public static string ZeroWidthSpaceString = "\x200B";
        public static string ZeroWidthNoBreakSpaceString = "\xFEFF";
        public static string ZeroWidthSpaceStringWithSpace = "\x200B ";
        public static string ZeroWidthNoBreakSpaceStringWithSpace = "\xFEFF ";
        public static char NonBreakSpace = '\x00A0';
        public static string NonBreakSpaceString = "\x00A0";

        public static char[] ZeroWidthSpaceCharacters = new char[]
        {
            '\x200B',
            '\xFEFF'
        };

        public static char[] WhiteSpaces = new char[]
        {
            '\r',
            '\n',
            '\f',
            '\b',
            '\x0020',
            '\x00A0',
            '\x1680',
            '\x180E',
            '\x2000',
            '\x2001',
            '\x2002',
            '\x2003',
            '\x2004',
            '\x2005',
            '\x2006',
            '\x2007',
            '\x2008',
            '\x2009',
            '\x200A',
            '\x200B',
            '\x202F',
            '\x205F',
            '\x3000',
            '\xFEFF'
        };

        public static string UTF8SignatureString = "\uFEFF";
        public static char UTF8SignatureChar = '\uFEFF';

        public static List<string> LanguageCodesNeedingFixups = null;

        public static bool LanguagesInitialized = false;

        public static char[] NonAlphanumericAndSpaceAndPunctuationCharacters;

        public static string ToRomanizedSpaceOrPunctuationString(string str)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in str)
                sb.Append(ToRomanizedSpaceOrPunctuation(c));

            return sb.ToString();
        }

        public static string ToRomanizedSpaceOrPunctuation(char c)
        {
            string s;

            switch (c)
            {
                case '\x0020':
                case '\x00A0':
                case '\x1680':
                case '\x180E':
                case '\x2000':
                case '\x2001':
                case '\x2002':
                case '\x2003':
                case '\x2004':
                case '\x2005':
                case '\x2006':
                case '\x2007':
                case '\x2008':
                case '\x2009':
                case '\x200A':
                    s = " ";
                    break;
                case '\x200B':
                    //s = "";
                    s = " ";
                    break;
                case '\x202F':
                case '\x205F':
                case '\x3000':
                    s = " ";
                    break;
                case '\xFEFF':
                    //s = "";
                    s = " ";
                    break;
                case '。':
                    s = ". ";
                    break;
                case '，':
                    s = ", ";
                    break;
                case '．':
                    s = ". ";
                    break;
                case '：':
                    s = ": ";
                    break;
                case '；':
                    s = "; ";
                    break;
                case '？':
                    s = "? ";
                    break;
                case '¿':
                    s = " ¿";
                    break;
                case '！':
                    s = "! ";
                    break;
                case '¡':
                    s = " ¡";
                    break;
                case '（':
                    s = " (";
                    break;
                case '）':
                    s = ") ";
                    break;
                case '【':
                    s = " [";
                    break;
                case '】':
                    s = "] ";
                    break;
                case '「':
                    s = " “";
                    break;
                case '」':
                    s = "” ";
                    break;
                case '『':
                    s = " “";
                    break;
                case '』':
                    s = "” ";
                    break;
                case '《':
                    s = " “";
                    break;
                case '》':
                    s = "” ";
                    break;
                case '»':
                    s = "\"";
                    break;
                case '«':
                    s = "\"";
                    break;
                case '›':
                    s = "\"";
                    break;
                case '‹':
                    s = "\"";
                    break;
                case '〖':
                    s = "\"";
                    break;
                case '〗':
                    s = "\"";
                    break;
                case '〔':
                    s = "\"";
                    break;
                case '〕':
                    s = "\"";
                    break;
                //case '…':
                //    s = " ";
                //    break;
                case '、':
                    s = ", ";
                    break;
                case '・':
                    s = " ";
                    break;
                default:
                    s = c.ToString();
                    break;
            }

            return s;
        }

        public static string ToNormalizedPunctuation(char c)
        {
            string s;

            switch (c)
            {
                case '。':
                    s = ". ";
                    break;
                case '，':
                    s = ", ";
                    break;
                case '．':
                    s = ". ";
                    break;
                case '：':
                    s = ": ";
                    break;
                case '；':
                    s = "; ";
                    break;
                case '？':
                    s = "? ";
                    break;
                case '¿':
                    s = "";
                    break;
                case '！':
                    s = "! ";
                    break;
                case '¡':
                    s = "";
                    break;
                case '、':
                    s = ", ";
                    break;
                case '‘':
                    s = "'";
                    break;
                case '’':
                    s = "'";
                    break;
                case '“':
                    s = "\"";
                    break;
                case '”':
                    s = "\"";
                    break;
                case '・':
                    s = " ";
                    break;
                case '—':
                    s = "-";
                    break;
                case '－':
                    s = "-";
                    break;
                case '―':
                    s = "-";
                    break;
                case '…':
                    s = "...";
                    break;
                case '（':
                    s = " (";
                    break;
                case '）':
                    s = ") ";
                    break;
                case '【':
                    s = " [";
                    break;
                case '】':
                    s = "] ";
                    break;
                case '「':
                    s = " “";
                    break;
                case '」':
                    s = "” ";
                    break;
                case '『':
                    s = " “";
                    break;
                case '』':
                    s = "” ";
                    break;
                case '《':
                    s = " “";
                    break;
                case '》':
                    s = "” ";
                    break;
                case '»':
                    s = "\"";
                    break;
                case '«':
                    s = "\"";
                    break;
                case '›':
                    s = "\"";
                    break;
                case '‹':
                    s = "\"";
                    break;
                case '〖':
                    s = "\"";
                    break;
                case '〗':
                    s = "\"";
                    break;
                case '〔':
                    s = "\"";
                    break;
                case '〕':
                    s = "\"";
                    break;
                default:
                    s = c.ToString();
                    break;
            }

            return s;
        }

        public static string ConvertZeroWidthSpacesToNormalSpaces(string str)
        {
            foreach (char zspc in ZeroWidthSpaceCharacters)
                str = str.Replace(zspc, ' ');
            return str;
        }

        public static bool IsPunctuation(char c)
        {
            if (PunctuationCharacters.Contains(c))
                return true;
            return false;
        }

        public static bool IsSpaceOrPunctuation(char c)
        {
            if (SpaceAndPunctuationCharacters.Contains(c))
                return true;
            return false;
        }

        public static bool IsSpaceOrPunctuation(string str)
        {
            if (String.IsNullOrEmpty(str))
                return true;

            foreach (char c in str)
            {
                if (!SpaceAndPunctuationCharacters.Contains(c))
                    return false;
            }

            return true;
        }
    }
}

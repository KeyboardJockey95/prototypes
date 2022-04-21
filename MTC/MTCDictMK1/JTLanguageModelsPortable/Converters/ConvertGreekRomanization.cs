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
    public class ConvertGreekRomanization : ConvertRomanization
    {
        private static Dictionary<string, string> _TheTableDictionary;
        private static char[] _TheCharacters;
        private static HashSet<char> _TheCharacterSet;
        private static int _TheMaxInputLength = -1;
        private static int _TheMaxOutputLength = -1;

        public ConvertGreekRomanization(
                char symbolSeparator,
                DictionaryRepository dictionary,
                bool useQuickDictionary)
            : base(
                TheTablePairs,
                TheTableDictionary,
                TheCharacters,
                TheCharacterSet,
                LanguageLookup.Greek,
                LanguageLookup.GreekRomanization,
                symbolSeparator,
                dictionary,
                useQuickDictionary,
                TheMaxInputLength,
                TheMaxOutputLength)
        {
        }

        public static string[] TheTablePairs =
        {
            "Α", "A",
            "α", "a",
            "Β", "B",
            "β", "b",
            "Γ", "G",
            "γ", "g",
            "Δ", "D",
            "δ", "d",
            "Ε", "E",
            "ε", "e",
            "Ζ", "Z",
            "ζ", "z",
            "Η", "Ē",
            "η", "ē",
            "Θ", "Th",
            "θ", "th",
            "Ι", "I",
            "ι", "i",
            "Κ", "K",
            "κ", "k",
            "Λ", "L",
            "λ", "l",
            "Μ", "M",
            "μ", "m",
            "Ν", "N",
            "ν", "n",
            "Ξ", "X",
            "ξ", "x",
            "Ο", "O",
            "ο", "o",
            "Π", "P",
            "π", "p",
            "Ρ", "R",
            "ρ", "r",
            "Σ", "S",
            "σ", "s",
            "Τ", "T",
            "τ", "t",
            "Υ", "Y",
            "υ", "y",
            "Φ", "Ph",
            "φ", "ph",
            "X", "Ch",
            "x", "ch",
            "Ψ", "Ps",
            "ψ", "ps",
            "Ω", "Ō",
            "ω", "ō",
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

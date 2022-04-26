using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class EnglishTool : LanguageTool
    {
        public EnglishTool() : base(LanguageLookup.English)
        {
            SetCanInflect("Verb", true);
            SetCanInflect("Noun", true);
            CanDeinflect = true;
            SetHasFrequencyTable(true);
        }

        public override IBaseObject Clone()
        {
            return new EnglishTool();
        }

        public static string[] EnglishCommonNonInflectableWords =
        {
            "no",
            "not",
            "never",
            "nothing",
            "a",
            "an",
            "any",
            "none",
            "nothing",
            "the",
            "this",
            "that",
            "these",
            "those",
            "here",
            "there",
            "and",
            "but",
            "in",
            "of",
            "by",
            "for",
            "because",
            "as",
            "while",
            "if",
            "what",
            "when",
            "where",
            "why",
            "how",
            "wherefore",
            "therefore",
            "i",
            "you",
            "he",
            "she",
            "we",
            "they",
            "me",
            "him",
            "her",
            "us",
            "them",
            "my",
            "your",
            "yours",
            "his",
            "her",
            "hers",
            "our",
            "ours",
            "their",
            "myself",
            "yourself",
            "ourselves",
            "themselves",
            "is",
            "to",
            "are",
            "be",
            "were",
            "was",
            "one",
            "do",
            "will",
            "would",
            "have",
            "been",
            "has",
            "did",
            "had",
            "used",
            "am",
            "doth",
            "does"
        };

        public override string[] CommonNonInflectableWords
        {
            get
            {
                return EnglishCommonNonInflectableWords;
            }
        }

        public static string[] EnglishNumberDigitTable =
        {
            "0", "zero",
            "1", "one",
            "2", "two",
            "3", "three",
            "4", "four",
            "5", "five",
            "6", "six",
            "7", "seven",
            "8", "eight",
            "9", "nine",
            "10", "ten",
            "11", "eleven",
            "12", "twelve",
            "13", "thirteen",
            "14", "fourteen",
            "15", "fifteen",
            "16", "sixteen",
            "17", "seventeen",
            "18", "eighteen",
            "19", "nineteen",
            "20", "twenty",
            "21", "twenty one",
            "22", "twenty two",
            "23", "twenty three",
            "24", "twenty four",
            "25", "twenty five",
            "26", "twenty six",
            "27", "twenty seven",
            "28", "twenty eight",
            "29", "twenty nine",
            "30", "thirty",
            "31", "thirty one",
            "32", "thirty two",
            "33", "thirty three",
            "34", "thirty four",
            "35", "thirty five",
            "36", "thirty six",
            "37", "thirty seven",
            "38", "thirty eight",
            "39", "thirty nine",
            "40", "forty",
            "41", "forty one",
            "42", "forty two",
            "43", "forty three",
            "44", "forty four",
            "45", "forty five",
            "46", "forty six",
            "47", "forty seven",
            "48", "forty eight",
            "49", "forty nine",
            "50", "fifty",
            "51", "fifty one",
            "52", "fifty two",
            "53", "fifty three",
            "54", "fifty four",
            "55", "fifty five",
            "56", "fifty six",
            "57", "fifty seven",
            "58", "fifty eight",
            "59", "fifty nine",
            "60", "sixty",
            "61", "sixty one",
            "62", "sixty two",
            "63", "sixty three",
            "64", "sixty four",
            "65", "sixty five",
            "66", "sixty six",
            "67", "sixty seven",
            "68", "sixty eight",
            "69", "sixty nine",
            "70", "seventy",
            "71", "seventy one",
            "72", "seventy two",
            "73", "seventy three",
            "74", "seventy four",
            "75", "seventy five",
            "76", "seventy six",
            "77", "seventy seven",
            "78", "seventy eight",
            "79", "seventy nine",
            "80", "eighty",
            "81", "eighty one",
            "82", "eighty two",
            "83", "eighty three",
            "84", "eighty four",
            "85", "eighty five",
            "86", "eighty six",
            "87", "eighty seven",
            "88", "eighty eight",
            "89", "eighty nine",
            "90", "ninety",
            "91", "ninety one",
            "92", "ninety two",
            "93", "ninety three",
            "94", "ninety four",
            "95", "ninety five",
            "96", "ninety six",
            "97", "ninety seven",
            "98", "ninety eight",
            "99", "ninety nine",
            "100", "hundred",
            "200", "two hundred",
            "300", "three hundred",
            "400", "four hundred",
            "500", "five hundred",
            "600", "six hundred",
            "700", "seven hundred",
            "800", "eight hundred",
            "900", "nine hundred",
            "1000", "thousand",
            "1000000", "million",
            "1000000000", "billion"
        };

        public override string[] NumberDigitTable
        {
            get
            {
                return EnglishNumberDigitTable;
            }
        }

        public static string[] EnglishNumberNameTable =
        {
            "zero", "0",
            "one", "1",
            "two", "2",
            "three", "3",
            "four", "4",
            "five", "5",
            "six", "6",
            "seven", "7",
            "eight", "8",
            "nine", "9",
            "ten", "10",
            "eleven", "11",
            "twelve", "12",
            "thirteen", "13",
            "fourteen", "14",
            "fifteen", "15",
            "sixteen", "16",
            "seventeen", "17",
            "eighteen", "18",
            "nineteen", "19",
            "twenty", "20",
            "twenty one", "21",
            "twenty two", "22",
            "twenty three", "23",
            "twenty four", "24",
            "twenty five", "25",
            "twenty six", "26",
            "twenty seven", "27",
            "twenty eight", "28",
            "twenty nine", "29",
            "thirty", "30",
            "thirty one", "31",
            "thirty two", "32",
            "thirty three", "33",
            "thirty four", "34",
            "thirty five", "35",
            "thirty six", "36",
            "thirty seven", "37",
            "thirty eight", "38",
            "thirty nine", "39",
            "forty", "40",
            "forty one", "41",
            "forty two", "42",
            "forty three", "43",
            "forty four", "44",
            "forty five", "45",
            "forty six", "46",
            "forty seven", "47",
            "forty eight", "48",
            "forty nine", "49",
            "fifty", "50",
            "fifty one", "51",
            "fifty two", "52",
            "fifty three", "53",
            "fifty four", "54",
            "fifty five", "55",
            "fifty six", "56",
            "fifty seven", "57",
            "fifty eight", "58",
            "fifty nine", "59",
            "sixty", "60",
            "sixty one", "61",
            "sixty two", "62",
            "sixty three", "63",
            "sixty four", "64",
            "sixty five", "65",
            "sixty six", "66",
            "sixty seven", "67",
            "sixty eight", "68",
            "sixty nine", "69",
            "seventy", "70",
            "seventy one", "71",
            "seventy two", "72",
            "seventy three", "73",
            "seventy four", "74",
            "seventy five", "75",
            "seventy six", "76",
            "seventy seven", "77",
            "seventy eight", "78",
            "seventy nine", "79",
            "eighty", "80",
            "eighty one", "81",
            "eighty two", "82",
            "eighty three", "83",
            "eighty four", "84",
            "eighty five", "85",
            "eighty six", "86",
            "eighty seven", "87",
            "eighty eight", "88",
            "eighty nine", "89",
            "ninety", "90",
            "ninety one", "91",
            "ninety two", "92",
            "ninety three", "93",
            "ninety four", "94",
            "ninety five", "95",
            "ninety six", "96",
            "ninety seven", "97",
            "ninety eight", "98",
            "ninety nine", "99",
            "hundred", "100",
            "hundred", "100",
            "hundred", "100",
            "hundred", "100",
            "hundred", "100",
            "two hundred", "200",
            "three hundred", "300",
            "four hundred", "400",
            "five hundred", "500",
            "six hundred", "600",
            "seven hundred", "700",
            "eight hundred", "800",
            "nine hundred", "900",
            "thousand", "1000",
            "million", "1000000",
            "billion", "1000000000"
        };

        public override string[] NumberNameTable
        {
            get
            {
                return EnglishNumberNameTable;
            }
        }

        public static string[] EnglishNumberCombinationTable =
        {
            "twenty one", "twenty-one",
            "twenty two", "twenty-two",
            "twenty three", "twenty-three",
            "twenty four", "twenty-four",
            "twenty five", "twenty-five",
            "twenty six", "twenty-six",
            "twenty seven", "twenty-seven",
            "twenty eight", "twenty-eight",
            "twenty nine", "twenty-nine",
            "thirty one", "thirty-one",
            "thirty two", "thirty-two",
            "thirty three", "thirty-three",
            "thirty four", "thirty-four",
            "thirty five", "thirty-five",
            "thirty six", "thirty-six",
            "thirty seven", "thirty-seven",
            "thirty eight", "thirty-eight",
            "thirty nine", "thirty-nine",
            "forty one", "forty-one",
            "forty two", "forty-two",
            "forty three", "forty-three",
            "forty four", "forty-four",
            "forty five", "forty-five",
            "forty six", "forty-six",
            "forty seven", "forty-seven",
            "forty eight", "forty-eight",
            "forty nine", "forty-nine",
            "fifty one", "fifty-one",
            "fifty two", "fifty-two",
            "fifty three", "fifty-three",
            "fifty four", "fifty-four",
            "fifty five", "fifty-five",
            "fifty six", "fifty-six",
            "fifty seven", "fifty-seven",
            "fifty eight", "fifty-eight",
            "fifty nine", "fifty-nine",
            "sixty one", "sixty-one",
            "sixty two", "sixty-two",
            "sixty three", "sixty-three",
            "sixty four", "sixty-four",
            "sixty five", "sixty-five",
            "sixty six", "sixty-six",
            "sixty seven", "sixty-seven",
            "sixty eight", "sixty-eight",
            "sixty nine", "sixty-nine",
            "seventy one", "seventy-one",
            "seventy two", "seventy-two",
            "seventy three", "seventy-three",
            "seventy four", "seventy-four",
            "seventy five", "seventy-five",
            "seventy six", "seventy-six",
            "seventy seven", "seventy-seven",
            "seventy eight", "seventy-eight",
            "seventy nine", "seventy-nine",
            "eighty one", "eighty-one",
            "eighty two", "eighty-two",
            "eighty three", "eighty-three",
            "eighty four", "eighty-four",
            "eighty five", "eighty-five",
            "eighty six", "eighty-six",
            "eighty seven", "eighty-seven",
            "eighty eight", "eighty-eight",
            "eighty nine", "eighty-nine",
            "ninety one", "ninety-one",
            "ninety two", "ninety-two",
            "ninety three", "ninety-three",
            "ninety four", "ninety-four",
            "ninety five", "ninety-five",
            "ninety six", "ninety-six",
            "ninety seven", "ninety-seven",
            "ninety eight", "ninety-eight",
            "ninety nine", "ninety-nine",
        };

        private Dictionary<string, string> _EnglishNumberCombinationDictionary;
        public virtual Dictionary<string, string> EnglishNumberCombinationDictionary
        {
            get
            {
                if (_EnglishNumberCombinationDictionary == null)
                {
                    string[] digitTable = EnglishNumberCombinationTable;

                    if (digitTable == null)
                        return null;

                    _EnglishNumberCombinationDictionary = new Dictionary<string, string>();

                    for (int i = 0; i < EnglishNumberCombinationTable.Length; i += 2)
                    {
                        if (!_EnglishNumberCombinationDictionary.ContainsKey(digitTable[i]))
                            _EnglishNumberCombinationDictionary.Add(digitTable[i], digitTable[i + 1]);
                    }
                }

                return _EnglishNumberCombinationDictionary;
            }
        }

        public static string[] EnglishDigitConnectorTable =
        {
            "and",
            "-"
        };

        public override string[] DigitConnectorTable
        {
            get
            {
                return EnglishDigitConnectorTable;
            }
        }

        // Derived from: https://www.c-sharpcorner.com/article/convert-numeric-value-into-words-currency-in-c-sharp/
        protected override string ConvertWholeNumber(string Number)
        {
            string word = "";

            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range
                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private string tens(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;

            switch (_Number)
            {
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 30:
                case 40:
                case 50:
                case 60:
                case 70:
                case 80:
                case 90:
                    if (NumberDigitToNameDictionary.TryGetValue(Number, out name))
                        return name;
                    break;
                default:
                    break;
            }

            if (_Number > 0)
            {
                name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
            }

            return name;
        }

        private string ones(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    if (NumberDigitToNameDictionary.TryGetValue(Number, out name))
                        return name;
                    break;
                default:
                    break;
            }
            return name;
        }
        // End derived code.

        public override string NormalizeNumberWord(string numberString)
        {
            return NormalizeNumberWordStatic(numberString);
        }

        public static string NormalizeNumberWordStatic(string numberString)
        {
            if (String.IsNullOrEmpty(numberString))
                return numberString;

            string numberStringLower = numberString.ToLower();

            numberStringLower = numberStringLower.Replace(" - ", " ");
            numberStringLower = numberStringLower.Replace("-", " ");
            numberStringLower = numberStringLower.Replace(" -", " ");
            numberStringLower = numberStringLower.Replace("- ", " ");

            string normalizedNumberString;

            switch (numberStringLower)
            {
                case "zero":
                    normalizedNumberString = "0";
                    break;
                case "one":
                    normalizedNumberString = "1";
                    break;
                case "two":
                    normalizedNumberString = "2";
                    break;
                case "three":
                    normalizedNumberString = "3";
                    break;
                case "four":
                    normalizedNumberString = "4";
                    break;
                case "five":
                    normalizedNumberString = "5";
                    break;
                case "six":
                    normalizedNumberString = "6";
                    break;
                case "seven":
                    normalizedNumberString = "7";
                    break;
                case "eight":
                    normalizedNumberString = "8";
                    break;
                case "nine":
                    normalizedNumberString = "9";
                    break;
                case "ten":
                    normalizedNumberString = "10";
                    break;
                case "eleven":
                    normalizedNumberString = "11";
                    break;
                case "twelve":
                    normalizedNumberString = "12";
                    break;
                case "thirteen":
                    normalizedNumberString = "13";
                    break;
                case "fourteen":
                    normalizedNumberString = "14";
                    break;
                case "fifteen":
                    normalizedNumberString = "15";
                    break;
                case "sixteen":
                    normalizedNumberString = "16";
                    break;
                case "seventeen":
                    normalizedNumberString = "17";
                    break;
                case "eighteen":
                    normalizedNumberString = "18";
                    break;
                case "nineteen":
                    normalizedNumberString = "19";
                    break;
                case "twenty":
                    normalizedNumberString = "20";
                    break;
                case "twenty one":
                    normalizedNumberString = "21";
                    break;
                case "twenty two":
                    normalizedNumberString = "22";
                    break;
                case "twenty three":
                    normalizedNumberString = "23";
                    break;
                case "twenty four":
                    normalizedNumberString = "24";
                    break;
                case "twenty five":
                    normalizedNumberString = "25";
                    break;
                case "twenty six":
                    normalizedNumberString = "26";
                    break;
                case "twenty seven":
                    normalizedNumberString = "27";
                    break;
                case "twenty eight":
                    normalizedNumberString = "28";
                    break;
                case "twenty nine":
                    normalizedNumberString = "29";
                    break;
                case "thirty":
                    normalizedNumberString = "30";
                    break;
                case "thirty one":
                    normalizedNumberString = "31";
                    break;
                case "thirty two":
                    normalizedNumberString = "32";
                    break;
                case "thirty three":
                    normalizedNumberString = "33";
                    break;
                case "thirty four":
                    normalizedNumberString = "34";
                    break;
                case "thirty five":
                    normalizedNumberString = "35";
                    break;
                case "thirty six":
                    normalizedNumberString = "36";
                    break;
                case "thirty seven":
                    normalizedNumberString = "37";
                    break;
                case "thirty eight":
                    normalizedNumberString = "38";
                    break;
                case "thirty nine":
                    normalizedNumberString = "39";
                    break;
                case "forty":
                    normalizedNumberString = "40";
                    break;
                case "forty one":
                    normalizedNumberString = "41";
                    break;
                case "forty two":
                    normalizedNumberString = "42";
                    break;
                case "forty three":
                    normalizedNumberString = "43";
                    break;
                case "forty four":
                    normalizedNumberString = "44";
                    break;
                case "forty five":
                    normalizedNumberString = "45";
                    break;
                case "forty six":
                    normalizedNumberString = "46";
                    break;
                case "forty seven":
                    normalizedNumberString = "47";
                    break;
                case "forty eight":
                    normalizedNumberString = "48";
                    break;
                case "forty nine":
                    normalizedNumberString = "49";
                    break;
                case "fifty":
                    normalizedNumberString = "50";
                    break;
                case "fifty one":
                    normalizedNumberString = "51";
                    break;
                case "fifty two":
                    normalizedNumberString = "52";
                    break;
                case "fifty three":
                    normalizedNumberString = "53";
                    break;
                case "fifty four":
                    normalizedNumberString = "54";
                    break;
                case "fifty five":
                    normalizedNumberString = "55";
                    break;
                case "fifty six":
                    normalizedNumberString = "56";
                    break;
                case "fifty seven":
                    normalizedNumberString = "57";
                    break;
                case "fifty eight":
                    normalizedNumberString = "58";
                    break;
                case "fifty nine":
                    normalizedNumberString = "59";
                    break;
                case "sixty":
                    normalizedNumberString = "60";
                    break;
                case "sixty one":
                    normalizedNumberString = "61";
                    break;
                case "sixty two":
                    normalizedNumberString = "62";
                    break;
                case "sixty three":
                    normalizedNumberString = "63";
                    break;
                case "sixty four":
                    normalizedNumberString = "64";
                    break;
                case "sixty five":
                    normalizedNumberString = "65";
                    break;
                case "sixty six":
                    normalizedNumberString = "66";
                    break;
                case "sixty seven":
                    normalizedNumberString = "67";
                    break;
                case "sixty eight":
                    normalizedNumberString = "68";
                    break;
                case "sixty nine":
                    normalizedNumberString = "69";
                    break;
                case "seventy":
                    normalizedNumberString = "70";
                    break;
                case "seventy one":
                    normalizedNumberString = "71";
                    break;
                case "seventy two":
                    normalizedNumberString = "72";
                    break;
                case "seventy three":
                    normalizedNumberString = "73";
                    break;
                case "seventy four":
                    normalizedNumberString = "74";
                    break;
                case "seventy five":
                    normalizedNumberString = "75";
                    break;
                case "seventy six":
                    normalizedNumberString = "76";
                    break;
                case "seventy seven":
                    normalizedNumberString = "77";
                    break;
                case "seventy eight":
                    normalizedNumberString = "78";
                    break;
                case "seventy nine":
                    normalizedNumberString = "79";
                    break;
                case "eighty":
                    normalizedNumberString = "80";
                    break;
                case "eighty one":
                    normalizedNumberString = "81";
                    break;
                case "eighty two":
                    normalizedNumberString = "82";
                    break;
                case "eighty three":
                    normalizedNumberString = "83";
                    break;
                case "eighty four":
                    normalizedNumberString = "84";
                    break;
                case "eighty five":
                    normalizedNumberString = "85";
                    break;
                case "eighty six":
                    normalizedNumberString = "86";
                    break;
                case "eighty seven":
                    normalizedNumberString = "87";
                    break;
                case "eighty eight":
                    normalizedNumberString = "88";
                    break;
                case "eighty nine":
                    normalizedNumberString = "89";
                    break;
                case "ninety":
                    normalizedNumberString = "90";
                    break;
                case "ninety one":
                    normalizedNumberString = "91";
                    break;
                case "ninety two":
                    normalizedNumberString = "92";
                    break;
                case "ninety three":
                    normalizedNumberString = "93";
                    break;
                case "ninety four":
                    normalizedNumberString = "94";
                    break;
                case "ninety five":
                    normalizedNumberString = "95";
                    break;
                case "ninety six":
                    normalizedNumberString = "96";
                    break;
                case "ninety seven":
                    normalizedNumberString = "97";
                    break;
                case "ninety eight":
                    normalizedNumberString = "98";
                    break;
                case "ninety nine":
                    normalizedNumberString = "99";
                    break;
                case "one hundred":
                case "hundred":
                    normalizedNumberString = "100";
                    break;
                case "one thousand":
                case "thousand":
                    normalizedNumberString = "1000";
                    break;
                case "one million":
                case "million":
                    normalizedNumberString = "1000000";
                    break;
                default:
                    normalizedNumberString = numberString;
                    break;
            }

            return normalizedNumberString;
        }

        public static Dictionary<string, string> EnglishAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "B.C.", "Beore Christ" },
            { "A.D.", "Anno Domini" },
            { "C.E.", "Common Era" },
            { "Mr.", "Mister" },
            { "Mrs.", "Misses" },
            { "Ms.", "Miz" },
            { "Sr.", "Senior" },
            { "Jr.", "Junior" },
            { "Jun.", "Junior" },
            { "etc.", "etcetera" },
            { "a.m.", "before noon" },
            { "p.m.", "after noon" },
            { "No.", "number" },
            { "Fig.", "figure" }
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return EnglishAbbreviationDictionary;
            }
        }

        public string[] EnglishNormallyCapitalized =
        {
            "i"
        };

        public override bool IsNormallyCapitalized(string str)
        {
            int ofs = str.IndexOf(' ');

            if (ofs != -1)
                str = str.Substring(0, ofs);

            return EnglishNormallyCapitalized.Contains(str.ToLower());

        }

        public override string CleanDictionaryFormString(
            string dictionaryForm,
            out string compoundFixupPattern)
        {
            compoundFixupPattern = null;

            if (String.IsNullOrEmpty(dictionaryForm))
                return dictionaryForm;

            string dictionaryFormLower = dictionaryForm.ToLower();

            if (dictionaryFormLower.StartsWith("to "))
                dictionaryForm = dictionaryForm.Substring(3);
            else if (dictionaryFormLower.StartsWith("the "))
                dictionaryForm = dictionaryForm.Substring(4);

            dictionaryForm = TextUtilities.FilterAsides(dictionaryForm);
            dictionaryForm = dictionaryForm.Trim();

            int ofs = dictionaryForm.IndexOf(' ');

            if (ofs != -1)
            {
                string remainder = dictionaryForm.Substring(ofs + 1).Trim();
                dictionaryForm = dictionaryForm.Substring(0, ofs).Trim();
                compoundFixupPattern = "%s " + remainder;
            }

            return dictionaryForm;
        }

        public override string GetStemAndClasses(
            string word,
            LanguageID languageID,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            string stem = word;

            categoryString = null;
            classCode = String.Empty;
            subClassCode = String.Empty;

            if (String.IsNullOrEmpty(word))
                return null;

            if (languageID != LanguageID)
                return null;

            if (stem.StartsWith("to "))
                stem = stem.Substring(3);

            stem = TextUtilities.FilterAsides(stem).Trim();

            return stem;
        }

#if false
        public override List<string> AutomaticRowKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> rowKeys = null;

            if (inflectorTable != null)
                rowKeys = inflectorTable.AutomaticRowKeys;

            if ((rowKeys == null) || (rowKeys.Count() == 0))
            {
                rowKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        rowKeys.Add("Number");
                        rowKeys.Add("Person");
                        rowKeys.Add("Gender");
                        break;
                    case "Noun":
                        break;
                    case "Adjective":
                        break;
                    default:
                        break;
                }
            }

            return rowKeys;
        }

        public override List<string> AutomaticColumnKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> columnKeys = null;

            if (inflectorTable != null)
                columnKeys = inflectorTable.AutomaticColumnKeys;

            if ((columnKeys == null) || (columnKeys.Count() == 0))
            {
                columnKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        columnKeys.Add("Tense");
                        break;
                    case "Noun":
                        columnKeys.Add("Number");
                        break;
                    case "Adjective":
                        break;
                    default:
                        break;
                }
            }

            return columnKeys;
        }
#else

        public override List<string> AutomaticRowKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> rowKeys = null;

            if (inflectorTable != null)
                rowKeys = inflectorTable.AutomaticRowKeys;

            if ((rowKeys == null) || (rowKeys.Count() == 0))
            {
                rowKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        rowKeys.Add("Number");
                        break;
                    case "Noun":
                        rowKeys.Add("Number");
                        break;
                    case "Adjective":
                        rowKeys.Add("Number");
                        break;
                    default:
                        break;
                }
            }

            return rowKeys;
        }

        public override List<string> AutomaticColumnKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> columnKeys = null;

            if (inflectorTable != null)
                columnKeys = inflectorTable.AutomaticColumnKeys;

            if ((columnKeys == null) || (columnKeys.Count() == 0))
            {
                columnKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        columnKeys.Add("Person");
                        columnKeys.Add("Gender");
                        break;
                    case "Noun":
                        columnKeys.Add("Gender");
                        break;
                    case "Adjective":
                        columnKeys.Add("Gender");
                        break;
                    default:
                        break;
                }
            }

            return columnKeys;
        }
#endif

        // Handle adding things like reflexive, direct pronouns, indirect pronouns, and other suffixes and prefixes
        // not normally handle in inflecting.
        public override bool ExtendInflection(Inflection inflection)
        {
            Designator designator = inflection.Designation;
            int count = designator.ClassificationCount();
            int index;
            InflectorTable inflectorTable = null;
            TokenDescriptor token = null;
            Designator iteratorDesignator = null;
            bool changed = false;
            bool returnValue = true;

            for (index = 0; index < count; index++)
            {
                Classifier classifier = designator.GetClassificationIndexed(index);

                switch (classifier.KeyString)
                {
                    case "Suffixed":
                        inflectorTable = InflectorTable("Verb");
                        if (inflectorTable == null)
                            break;
                        iteratorDesignator = designator.GetPrefixedDesignator(classifier.Text);
                        token = inflectorTable.FindIteratorTokenFuzzy(classifier.Text + "Pronouns", iteratorDesignator);
                        if (token == null)
                            break;
                        inflection.AppendToPostWordsSpaced(LanguageID, token.TextFirst);
                        changed = true;
                        break;
                    default:
                        break;
                }
            }

            if (changed)
                inflection.RegenerateLanguage(LanguageID);

            return returnValue;
        }

        public override void ProcessOtherWordsInInflection(
            string word,
            LanguageID languageID,
            bool isPost,
            ref string preWords,
            ref string postWords,
            out Designator overideDesignator)
        {
            if ((word == "not") || (word == "Not"))
                overideDesignator = new Designator("Polarity", "Negative");
            else
                overideDesignator = null;
        }

        public override string GetFrequencyNormalizedText(
            string input)
        {
            string output;

            if (input.EndsWith("'s"))
            {
                int ofs = input.IndexOf('\'');

                if (ofs != -1)
                    output = input.Substring(0, ofs);
                else
                    output = input;
            }
            else
                output = input;

            return output;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Language
{
    public class EnglishStems
    {
        public string Root;
        public string Preterite;
        public string PresentParticiple;
        public string PastParticiple;

        public EnglishStems(
            string root,
            string preterite,
            string presentParticiple,
            string pastParticiple)
        {
            Root = root;
            Preterite = preterite;
            PresentParticiple = presentParticiple;
            PastParticiple = pastParticiple;
        }

        public EnglishStems(EnglishStems other)
        {
            Root = other.Root;
            Preterite = other.Preterite;
            PresentParticiple = other.PresentParticiple;
            PastParticiple = other.PastParticiple;
        }

        public EnglishStems()
        {
            Root = String.Empty;
            Preterite = String.Empty;
            PresentParticiple = String.Empty;
            PastParticiple = String.Empty;
        }

        public void SetUnknown(string untranslated)
        {
            Root = "(" + untranslated + ")";
            Preterite = "(" + untranslated + ")'ed";
            PresentParticiple = "(" + untranslated + ")'ing";
            PastParticiple = "(" + untranslated + ")'ed";
        }

        public void SetPrefix(string prefix)
        {
            Root = prefix + " " + Root;
            Preterite = prefix + " " + Preterite;
            PresentParticiple = prefix + " " + PresentParticiple;
            PastParticiple = prefix + " " + PastParticiple;
        }

        public void SetSuffix(string suffix)
        {
            Root = Root + " " + suffix;
            Preterite = Preterite + " " + suffix;
            PresentParticiple = PresentParticiple + " " + suffix;
            PastParticiple = PastParticiple + " " + suffix;
        }

        public bool GetStemsFromDictionaryEntry(DictionaryEntry dictionaryEntry, int senseIndex, int synonymIndex)
        {
            if ((dictionaryEntry == null) || (dictionaryEntry.SenseCount == 0))
                return false;

            Sense sense;

            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
            {
                sense = dictionaryEntry.Senses.FirstOrDefault(x => x.Category == LexicalCategory.Verb);

                if (sense == null)
                    sense = dictionaryEntry.GetSenseIndexed(0);
            }

            return GetStemsFromSense(sense, synonymIndex);
        }

        public bool GetStemsFromSense(Sense sense, int synonymIndex)
        {
            if (sense == null)
                return false;

            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(LanguageLookup.English);

            if (languageSynonyms != null)
            {
                if ((synonymIndex < 0) || (synonymIndex >= languageSynonyms.SynonymCount))
                    synonymIndex = 0;

                string infinitive = languageSynonyms.GetSynonymIndexed(synonymIndex);
                return GetStemsFromInfinitive(infinitive);
            }

            return false;
        }

        public bool GetStemsFromInfinitive(string infinitive)
        {
            string fullRoot = infinitive.Trim().ToLower();
            int ofs = fullRoot.IndexOf('(');
            string suffix = (ofs != -1 ? fullRoot.Substring(ofs).Trim() : String.Empty);
            Root = (ofs != -1 ? fullRoot.Substring(0, ofs).Trim() : fullRoot);

            if (Root.StartsWith("to "))
                Root = Root.Substring(3).Trim();

            int spaceIndex = Root.IndexOf(' ');

            if (spaceIndex > 0)
            {
                if (!String.IsNullOrEmpty(suffix))
                    suffix = Root.Substring(spaceIndex).Trim() + " " + suffix;
                else
                    suffix = Root.Substring(spaceIndex).Trim();

                ofs = spaceIndex;
                Root = Root.Substring(0, spaceIndex);
            }

            int index;

            for (index = 0; EnglishIrregularVerbs[index] != null; index += 3)
            {
                if (Root == EnglishIrregularVerbs[index])
                {
                    Preterite = EnglishIrregularVerbs[index + 1];
                    PresentParticiple = ComposePresentParticiple(Root);
                    PastParticiple = EnglishIrregularVerbs[index + 2];

                    int lastCommaIndex = Preterite.LastIndexOf(',');

                    if (lastCommaIndex > 0)
                        Preterite = Preterite.Substring(lastCommaIndex + 1).Trim();

                    lastCommaIndex = PastParticiple.LastIndexOf(',');

                    if (lastCommaIndex > 0)
                        PastParticiple = PastParticiple.Substring(lastCommaIndex + 1).Trim();

                    if (ofs != -1)
                    {
                        Preterite = Preterite + " " + suffix;
                        PresentParticiple = PresentParticiple + " " + suffix;
                        PastParticiple = PastParticiple + " " + suffix;
                        Root = Root + " " + suffix;
                    }

                    return true;
                }
            }

            string preteriteRoot = Root;
            string presentParticipleRoot = Root;

            if (Root.EndsWith("ie"))
            {
                preteriteRoot = Root.Substring(0, Root.Length - 1);
                presentParticipleRoot = Root.Substring(0, Root.Length - 2) + "y";
            }
            else if (Root.EndsWith("e"))
                preteriteRoot = Root.Substring(0, Root.Length - 1);

            if (ofs != -1)
            {
                Preterite = preteriteRoot + "ed " + suffix;
                PresentParticiple = ComposePresentParticiple(presentParticipleRoot) + " " + suffix;
                PastParticiple = Preterite;
                Root = Root + " " + suffix;
            }
            else
            {
                Preterite = preteriteRoot + "ed";
                PresentParticiple = ComposePresentParticiple(presentParticipleRoot);
                PastParticiple = Preterite;
            }

            return true;
        }

        public string ComposePresentParticiple(string root)
        {
            if ((root == null) || (root.Length < 2))
                return root + "ing";
            int length = root.Length;
            char last = root[length - 1];
            char nextToLast = (length >= 3 ? root[length - 2] : '\0');
            char nextNextToLast = (length >= 3 ? root[length - 3] : '\0');
            string pp;

            if (last == 'e')
            {
                if (root.Length == 2)
                    pp = root + "ing";
                else if (nextToLast == 'i')
                    pp = root.Substring(0, length - 2) + "ying";
                else if (IsVowel(nextToLast))
                    pp = root + "ing";
                else
                    pp = root.Substring(0, length - 1) + "ing";
            }
            else if ((length > 2) && IsConsonant(last) && IsVowel(nextToLast) && IsConsonant(nextNextToLast))
            {
                if (root.EndsWith("ic"))
                    pp = root + "king";
                else if (last == 'l')
                    pp = root + last + "ing";
                else if (SyllableCount(root) == 1)
                    pp = root + last + "ing";
                else if (IsVerbLastSyllableStressed(root))
                    pp = root + last + "ing";
                else
                    pp = root + "ing";
            }
            else
                pp = root + "ing";

            return pp;
        }

        public bool IsConsonant(char c)
        {
            switch (c)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'y':
                case 'A':
                case 'E':
                case 'I':
                case 'O':
                case 'U':
                case 'Y':
                    return false;
                default:
                    return true;
            }
        }

        public bool IsVowel(char c)
        {
            switch (c)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'y':
                case 'A':
                case 'E':
                case 'I':
                case 'O':
                case 'U':
                case 'Y':
                    return true;
                default:
                    return false;
            }
        }

        public static char[] vowels = new[] { 'a', 'e', 'i', 'o', 'u', 'y' };

        public int SyllableCount(string word)
        {
            word = word.ToLower().Trim();
            bool lastWasVowel = false;
            int count = 0;

            //a string is an IEnumerable<char>; convenient.
            foreach (var c in word)
            {
                if (vowels.Contains(c))
                {
                    if (!lastWasVowel)
                        count++;
                    lastWasVowel = true;
                }
                else
                    lastWasVowel = false;
            }

            if ((word.EndsWith("e") || (word.EndsWith("es") || word.EndsWith("ed")))
                  && !word.EndsWith("le"))
                count--;

            return count;
        }

        public static string[] nonStressEndings =
        {
            "en",
            "it",
            "w"
        };

        public bool IsVerbLastSyllableStressed(string word)
        {
            foreach (string ending in nonStressEndings)
            {
                if (word.EndsWith(ending))
                    return false;
            }
            return true;
        }

        // English irregular verbs in triples of root, simple past, and past participle.
        public string[] EnglishIrregularVerbs =
        {
            "be", "was/were", "been",
            "bear", "bore", "borne/born",
            "beat", "beat", "beaten",
            "become", "became", "become",
            "begin", "began", "begun",
            "bend", "bent", "bent",
            "bet", "bet", "bet",
            "bid", "bid, bade", "bid, bidden",
            "bind", "bound", "bound",
            "bite", "bit", "bitten",
            "bleed", "bled", "bled",
            "blow", "blew", "blown",
            "break", "broke", "broken",
            "breed", "bred", "bred",
            "bring", "brought", "brought",
            "broadcast", "broadcast", "broadcast",
            "build", "built", "built",
            "burn", "burnt", "burnt",
            "burst", "burst", "burst",
            "bust", "bust", "bust",
            "buy", "bought", "bought",
            "cast", "cast", "cast",
            "catch", "caught", "caught",
            "choose", "chose", "chosen",
            "cling", "clung", "clung",
            "come", "came", "come",
            "cost", "cost", "cost",
            "creep", "crept", "crept",
            "cut", "cut", "cut",
            "deal", "dealt", "dealt",
            "dig", "dug", "dug",
            "dive", "dived/dove", "dived",
            "do", "did", "done",
            "draw", "drew", "drawn",
            "dream", "dreamt", "dreamt",
            "drink", "drank", "drunk",
            "drive", "drove", "driven",
            "eat", "ate", "eaten",
            "fall", "fell", "fallen",
            "feed", "fed", "fed",
            "feel", "felt", "felt",
            "fight", "fought", "fought",
            "find", "found", "found",
            "flee", "fled", "fled",
            "fling", "flung", "flung",
            "fly", "flew", "flown",
            "forbid", "forbade, forbad", "forbidden",
            "forecast", "forecast", "forecast",
            "forget", "forgot", "forgotten",
            "forsake", "forsook", "forsaken",
            "freeze", "froze", "frozen",
            "get", "got", "got, gotten",
            "give", "gave", "given",
            "grind", "ground", "ground",
            "go", "went", "gone",
            "grow", "grew", "grown",
            "hang", "hung", "hung",
            "have", "had", "had",
            "hear", "heard", "heard",
            "hide", "hid", "hidden",
            "hit", "hit", "hit",
            "hold", "held", "held",
            "hurt", "hurt", "hurt",
            "keep", "kept", "kept",
            "know", "knew", "known",
            "lay", "laid", "laid",
            "lead", "led", "led",
            "learn", "learnt", "learnt",
            "leave", "left", "left",
            "lend", "lent", "lent",
            "let", "let", "let",
            "lie", "lay", "lain",
            "light", "lit", "lit",
            "lose", "lost", "lost",
            "make", "made", "made",
            "mean", "meant", "meant",
            "meet", "met", "met",
            "pay", "paid", "paid",
            "prove", "proved", "proven",
            "put", "put", "put",
            "quit", "quit", "quit",
            "read", "read", "read",
            "rid", "rid", "rid",
            "ride", "rode", "ridden",
            "ring", "rang", "rung",
            "rise", "rose", "risen",
            "run", "ran", "run",
            "say", "said", "said",
            "see", "saw", "seen",
            "seek", "sought", "sought",
            "sell", "sold", "sold",
            "send", "sent", "sent",
            "set", "set", "set",
            "sew", "sewed", "sewn",
            "shake", "shook", "shaken",
            "shear", "sheared", "shorn",
            "shed", "shed", "shed",
            "shine", "shone", "shone",
            "shoot", "shot", "shot",
            "show", "showed", "shown",
            "shut", "shut", "shut",
            "sing", "sang", "sung",
            "sink", "sank", "sunk",
            "sit", "sat", "sat",
            "slay", "slew", "slain",
            "sleep", "slept", "slept",
            "slide", "slid", "slid",
            "sling", "slung", "slung",
            "slink", "slunk", "slunk",
            "slit", "slit", "slit",
            "sow", "sowed", "sown",
            "speak", "spoke", "spoken",
            "speed", "sped", "sped",
            "spend", "spent", "spent",
            "spin", "spun", "spun",
            "spit", "spat, spit", "spat, spit",
            "split", "split", "split",
            "spread", "spread", "spread",
            "spring", "sprang", "sprung",
            "stand", "stood", "stood",
            "steal", "stole", "stolen",
            "stick", "stuck", "stuck",
            "sting", "stung", "stung",
            "stink", "stank, stunk", "stunk",
            "stride", "strode", "stridden",
            "strike", "struck", "struck",
            "string", "strung", "strung",
            "strive", "strove", "striven",
            "swear", "swore", "sworn",
            "sweep", "swept", "swept",
            "swell", "swelled", "swollen",
            "swim", "swam", "swum",
            "swing", "swung", "swung",
            "take", "took", "taken",
            "teach", "taught", "taught",
            "tear", "tore", "torn",
            "tell", "told", "told",
            "think", "thought", "thought",
            "thrive", "throve", "thrived",
            "throw", "threw", "thrown",
            "thrust", "thrust", "thrust",
            "tread", "trod", "trodden, trod",
            "understand", "understood", "understood",
            "wake", "woke", "woken",
            "wear", "wore", "worn",
            "weave", "wove", "woven",
            "weep", "wept", "wept",
            "wet", "wet", "wet",
            "win", "won", "won",
            "wind", "wound", "wound",
            "wring", "wrung", "wrung",
            "write", "wrote", "written",
            null, null, null
        };
    }
}

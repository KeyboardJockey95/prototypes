using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Object
{
    public static partial class TextUtilities
    {
        public static string[] HtmlEntityToStringMap =
        {
            "&quot;", "\"",
            "&amp;", "&",
            "&apos;", "'",
            "&lt;", "<",
            "&gt;", ">",
            "&nbsp;", " ",
            "&iexcl;", "¡",
            "&cent;", "¢",
            "&pound;", "£",
            "&curren;", "¤",
            "&yen;", "¥",
            "&brvbar;", "¦",
            "&sect;", "§",
            "&uml;", "¨",
            "&copy;", "©",
            "&ordf;", "ª",
            "&laquo;", "«",
            "&not;", "¬",
            "&shy;", " ",
            "&reg;", "®",
            "&macr;", "¯",
            "&deg;", "°",
            "&plusmn;", "±",
            "&sup2;", "²",
            "&sup3;", "³",
            "&acute;", "´",
            "&micro;", "µ",
            "&para;", "¶",
            "&middot;", "·",
            "&cedil;", "¸",
            "&sup1;", "¹",
            "&ordm;", "º",
            "&raquo;", "»",
            "&frac14;", "¼",
            "&frac12;", "½",
            "&frac34;", "¾",
            "&iquest;", "¿",
            "&Agrave;", "À",
            "&Aacute;", "Á",
            "&Acirc;", "Â",
            "&Atilde;", "Ã",
            "&Auml;", "Ä",
            "&Aring;", "Å",
            "&AElig;", "Æ",
            "&Ccedil;", "Ç",
            "&Egrave;", "È",
            "&Eacute;", "É",
            "&Ecirc;", "Ê",
            "&Euml;", "Ë",
            "&Igrave;", "Ì",
            "&Iacute;", "Í",
            "&Icirc;", "Î",
            "&Iuml;", "Ï",
            "&ETH;", "Ð",
            "&Ntilde;", "Ñ",
            "&Ograve;", "Ò",
            "&Oacute;", "Ó",
            "&Ocirc;", "Ô",
            "&Otilde;", "Õ",
            "&Ouml;", "Ö",
            "&times;", "×",
            "&Oslash;", "Ø",
            "&Ugrave;", "Ù",
            "&Uacute;", "Ú",
            "&Ucirc;", "Û",
            "&Uuml;", "Ü",
            "&Yacute;", "Ý",
            "&THORN;", "Þ",
            "&szlig;", "ß",
            "&agrave;", "à",
            "&aacute;", "á",
            "&acirc;", "â",
            "&atilde;", "ã",
            "&auml;", "ä",
            "&aring;", "å",
            "&aelig;", "æ",
            "&ccedil;", "ç",
            "&egrave;", "è",
            "&eacute;", "é",
            "&ecirc;", "ê",
            "&euml;", "ë",
            "&igrave;", "ì",
            "&iacute;", "í",
            "&icirc;", "î",
            "&iuml;", "ï",
            "&eth;", "ð",
            "&ntilde;", "ñ",
            "&ograve;", "ò",
            "&oacute;", "ó",
            "&ocirc;", "ô",
            "&otilde;", "õ",
            "&ouml;", "ö",
            "&divide;", "÷",
            "&oslash;", "ø",
            "&ugrave;", "ù",
            "&uacute;", "ú",
            "&ucirc;", "û",
            "&uuml;", "ü",
            "&yacute;", "ý",
            "&thorn;", "þ",
            "&yuml;", "ÿ",
            "&OElig;", "Œ",
            "&oelig;", "œ",
            "&Scaron;", "Š",
            "&scaron;", "š",
            "&Yuml;", "Ÿ",
            "&fnof;", "ƒ",
            "&circ;", "ˆ",
            "&tilde;", "˜",
            "&Alpha;", "Α",
            "&Beta;", "Β",
            "&Gamma;", "Γ",
            "&Delta;", "Δ",
            "&Epsilon;", "Ε",
            "&Zeta;", "Ζ",
            "&Eta;", "Η",
            "&Theta;", "Θ",
            "&Iota;", "Ι",
            "&Kappa;", "Κ",
            "&Lambda;", "Λ",
            "&Mu;", "Μ",
            "&Nu;", "Ν",
            "&Xi;", "Ξ",
            "&Omicron;", "Ο",
            "&Pi;", "Π",
            "&Rho;", "Ρ",
            "&Sigma;", "Σ",
            "&Tau;", "Τ",
            "&Upsilon;", "Υ",
            "&Phi;", "Φ",
            "&Chi;", "Χ",
            "&Psi;", "Ψ",
            "&Omega;", "Ω",
            "&alpha;", "α",
            "&beta;", "β",
            "&gamma;", "γ",
            "&delta;", "δ",
            "&epsilon;", "ε",
            "&zeta;", "ζ",
            "&eta;", "η",
            "&theta;", "θ",
            "&iota;", "ι",
            "&kappa;", "κ",
            "&lambda;", "λ",
            "&mu;", "μ",
            "&nu;", "ν",
            "&xi;", "ξ",
            "&omicron;", "ο",
            "&pi;", "π",
            "&rho;", "ρ",
            "&sigmaf;", "ς",
            "&sigma;", "σ",
            "&tau;", "τ",
            "&upsilon;", "υ",
            "&phi;", "φ",
            "&chi;", "χ",
            "&psi;", "ψ",
            "&omega;", "ω",
            "&thetasym;", "ϑ",
            "&upsih;", "ϒ",
            "&piv;", "ϖ",
            "&ensp;", " ",
            "&emsp;", " ",
            "&thinsp;", " ",
            "&zwnj;", "",
            "&zwj;", "",
            "&lrm;", "",
            "&rlm;", "",
            "&ndash;", "–",
            "&mdash;", "—",
            "&horbar;", "―",
            "&lsquo;", "‘",
            "&rsquo;", "’",
            "&sbquo;", "‚",
            "&ldquo;", "“",
            "&rdquo;", "”",
            "&bdquo;", "„",
            "&dagger;", "†",
            "&Dagger;", "‡",
            "&bull;", "•",
            "&hellip;", "…",
            "&permil;", "‰",
            "&prime;", "′",
            "&Prime;", "″",
            "&lsaquo;", "‹",
            "&rsaquo;", "›",
            "&oline;", "‾",
            "&frasl;", "⁄",
            "&euro;", "€",
            "&image;", "ℑ",
            "&weierp;", "℘",
            "&real;", "ℜ",
            "&trade;", "™",
            "&alefsym;", "ℵ",
            "&larr;", "←",
            "&uarr;", "↑",
            "&rarr;", "→",
            "&darr;", "↓",
            "&harr;", "↔",
            "&crarr;", "↵",
            "&lArr;", "⇐",
            "&uArr;", "⇑",
            "&rArr;", "⇒",
            "&dArr;", "⇓",
            "&hArr;", "⇔",
            "&forall;", "∀",
            "&part;", "∂",
            "&exist;", "∃",
            "&empty;", "∅",
            "&nabla;", "∇",
            "&isin;", "∈",
            "&notin;", "∉",
            "&ni;", "∋",
            "&prod;", "∏",
            "&sum;", "∑",
            "&minus;", "−",
            "&lowast;", "∗",
            "&radic;", "√",
            "&prop;", "∝",
            "&infin;", "∞",
            "&ang;", "∠",
            "&and;", "∧",
            "&or;", "∨",
            "&cap;", "∩",
            "&cup;", "∪",
            "&int;", "∫",
            "&there4;", "∴",
            "&sim;", "∼",
            "&cong;", "≅",
            "&asymp;", "≈",
            "&ne;", "≠",
            "&equiv;", "≡",
            "&le;", "≤",
            "&ge;", "≥",
            "&sub;", "⊂",
            "&sup;", "⊃",
            "&nsub;", "⊄",
            "&sube;", "⊆",
            "&supe;", "⊇",
            "&oplus;", "⊕",
            "&otimes;", "⊗",
            "&perp;", "⊥",
            "&sdot;", "⋅",
            "&lceil;", "⌈",
            "&rceil;", "⌉",
            "&lfloor;", "⌊",
            "&rfloor;", "⌋",
            "&lang;", "〈",
            "&rang;", "〉",
            "&loz;", "◊",
            "&spades;", "♠",
            "&clubs;", "♣",
            "&hearts;", "♥",
            "&diams;", "♦"
        };

        public static Dictionary<string, string> HtmlEntityToCharDictionary = null;
        public static Dictionary<char, string> HtmlCharToEntityDictionary = null;

        public static string EntityEncode(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (HtmlCharToEntityDictionary == null)
            {
                HtmlCharToEntityDictionary = new Dictionary<char, string>();
                int c = HtmlEntityToStringMap.Count();
                string t;

                for (int i = 0; i < c; i +=2)
                {
                    string entity = HtmlEntityToStringMap[i];

                    if (HtmlEntityToStringMap[i + 1].Length > 0)
                    {
                        char chr = HtmlEntityToStringMap[i + 1][0];

                        if (!HtmlCharToEntityDictionary.TryGetValue(chr, out t))
                            HtmlCharToEntityDictionary.Add(chr, entity);
                    }
                }
            }

            int index;
            int count = str.Length;
            char ch;
            string en;
            int ic;

            for (index = count - 1; index >= 0; index--)
            {
                ch = str[index];

                if (ch < ' ')
                {
                    ic = ch;
                    en = "%" + ((ic % 0xf0) >> 4).ToString() + (ic % 0xf).ToString() + ";";
                    str = str.Remove(index, 1);
                    str = str.Insert(index, en);
                }
                else if (ch <= '~')
                {
                }
                else
                {
                    if (HtmlCharToEntityDictionary.TryGetValue(ch, out en))
                    {
                        str = str.Remove(index, 1);
                        str = str.Insert(index, en);
                    }
                }
            }

            return str;
        }

        public static string EntityDecode(string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (HtmlEntityToCharDictionary == null)
            {
                HtmlEntityToCharDictionary = new Dictionary<string, string>();
                int c = HtmlEntityToStringMap.Count();

                for (int i = 0; i < c; i += 2)
                {
                    string entity = HtmlEntityToStringMap[i];
                    string chr = HtmlEntityToStringMap[i + 1];
                    HtmlEntityToCharDictionary.Add(entity, chr);
                }
            }

            int index;
            int count = str.Length;
            char ch;
            string en;
            int ic;
            int si;
            int ei;
            char sc;
            string es;
            int el;
            char c1, c2;

            for (index = count - 1; index >= 0; index--)
            {
                ch = str[index];

                if (ch == ';')
                {
                    for (si = index - 1; si >= 0; si--)
                    {
                        sc = str[si];

                        if (char.IsWhiteSpace(sc))
                            break;
                        else if (sc == '&')
                        {
                            el = (index - si) + 1;

                            if (((si + 1) < index) && (str[si + 1] == '#'))
                            {
                                if (((si + 2) < index) && (str[si + 2] == 'x'))
                                {
                                    ic = 0;

                                    for (ei = si + 3; ei < index; ei++)
                                        ic = (ic << 4) + GetHexNibble(str[ei]);

                                    es = ((char)ic).ToString();
                                }
                                else
                                {
                                    ic = 0;

                                    for (ei = si + 2; ei < index; ei++)
                                        ic = (ic * 10) + GetDecimalNibble(str[ei]);

                                    es = ((char)ic).ToString();
                                }

                                str = str.Remove(si, el);
                                str = str.Insert(si, es);
                                index = si;
                            }
                            else if ((index - si) > 1)
                            {
                                en = str.Substring(si, el);

                                if (HtmlEntityToCharDictionary.TryGetValue(en, out es))
                                {
                                    str = str.Remove(si, el);
                                    str = str.Insert(si, es);
                                    index = si;
                                }
                            }
                            break;
                        }
                    }
                }
                else if (ch == '%')
                {
                    if (index <= count - 3)
                    {
                        c1 = str[index + 1];
                        c2 = str[index + 2];

                        if (IsHex(c1) && IsHex(c2))
                        {
                            ic = (GetHexNibble(c1) << 4) + GetHexNibble(c2);
                            es = ((char)ic).ToString();
                            str = str.Remove(index, 3);
                            str = str.Insert(index, es);
                        }
                    }
                }
            }

            return str;
        }
    }
}

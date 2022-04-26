using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Object
{
    public static class LanguageCodes
    {
        public static Dictionary<string, string> Code3ToCode2;
        public static Dictionary<string, string> Code2ToCode3;
        public static Dictionary<string, string> Code2ToCode_ll_CC;
        public static Dictionary<string, string> Code3ToName;
        public static Dictionary<string, string> Code2ToName;
        public static Dictionary<string, string> NameToCode3;
        public static Dictionary<string, string> NameToCode2;

        public static string GetCode2FromCode3(string code3)
        {
            string code2;

            if (Code3ToCode2.TryGetValue(code3, out code2))
                return code2;

            return String.Empty;
        }

        public static string GetCode3FromCode2(string code2)
        {
            string code3;

            if (Code2ToCode3.TryGetValue(code2, out code3))
                return code3;

            return String.Empty;
        }

        public static string GetCode_ll_CC_FromCode2(string code2)
        {
            string code_ll_CC;

            if (Code2ToCode_ll_CC.TryGetValue(code2, out code_ll_CC))
                return code_ll_CC;

            return String.Empty;
        }

        public static string GetNameFromCode3(string code3)
        {
            string name;

            if (Code3ToName.TryGetValue(code3, out name))
                return name;

            return String.Empty;
        }

        public static string GetNameFromCode2(string code2)
        {
            string name;

            if (Code2ToName.TryGetValue(code2, out name))
                return name;

            return String.Empty;
        }

        public static string GetCode2FromName(string name)
        {
            string code2;

            if (NameToCode2.TryGetValue(name, out code2))
                return code2;

            return String.Empty;
        }

        public static string GetCode3FromName(string name)
        {
            string code3;

            if (NameToCode3.TryGetValue(name, out code3))
                return code3;

            return String.Empty;
        }

        public static char[] Comma = { ',' };

        static LanguageCodes()
        {
            Code3ToCode2 = new Dictionary<string, string>();
            Code2ToCode3 = new Dictionary<string, string>();
            Code2ToCode_ll_CC = new Dictionary<string, string>();
            Code3ToName = new Dictionary<string, string>();
            Code2ToName = new Dictionary<string, string>();
            NameToCode3 = new Dictionary<string, string>();
            NameToCode2 = new Dictionary<string, string>();

            for (int index = 0; index < LanguageCodesTable.Count(); index += 4)
            {
                string code3 = LanguageCodesTable[index];
                string code2 = LanguageCodesTable[index + 1];
                string code_ll_CC = LanguageCodesTable[index + 2];
                string name = LanguageCodesTable[index + 3];

                try
                {
                    if (code3.Contains(","))
                    {
                        string[] parts = code3.Split(Comma);
                        string code3B = parts[0];
                        string code3T = parts[1];

                        Code3ToCode2.Add(code3B, code2);
                        Code3ToCode2.Add(code3T, code2);

                        if (!String.IsNullOrEmpty(code2))
                        {
                            Code2ToCode3.Add(code2, code3T);
                            Code2ToName.Add(code2, name);
                        }

                        Code3ToName.Add(code3B, name);
                        Code3ToName.Add(code3T, name);

                        NameToCode3.Add(name, code3T);
                        NameToCode2.Add(name, code2);
                    }
                    else if (!String.IsNullOrEmpty(code3))
                    {
                        Code3ToCode2.Add(code3, code2);

                        if (!String.IsNullOrEmpty(code2))
                        {
                            Code2ToCode3.Add(code2, code3);
                            Code2ToName.Add(code2, name);
                            Code2ToCode_ll_CC.Add(code2, code_ll_CC);
                        }

                        Code3ToName.Add(code3, name);
                        NameToCode3.Add(name, code3);
                        NameToCode2.Add(name, code2);
                    }
                    else if (!String.IsNullOrEmpty(code2))
                    {
                        Code2ToCode_ll_CC.Add(code2, code_ll_CC);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public static string[] LanguageCodesTable = new string[]
        {
            "aar", "aa", "ll_CC", "Afar",
            "abk", "ab", "ll_CC", "Abkhazian",
            "ace", "", "ll_CC",  "Achinese",
            "ach", "", "ll_CC", "Acoli",
            "ada", "", "ll_CC", "Adangme",
            "ady", "", "ll_CC", "Adyghe; Adygei",
            "afa", "", "ll_CC", "Afro-Asiatic languages",
            "afh", "", "ll_CC", "Afrihili",
            "afr", "af", "af_ZA", "Afrikaans",
            "ain", "", "ll_CC", "Ainu",
            "aka", "ak", "ak_", "Akan",
            "akk", "", "ll_CC", "Akkadian",
            "alb,sqi", "sq", "sq_AL", "Albanian",
            "ale", "", "ll_CC", "Aleut",
            "alg", "", "ll_CC", "Algonquian languages",
            "alt", "", "ll_CC", "Southern Altai",
            "amh", "am", "amh_ET", "Amharic",
            "ang", "", "ll_CC", "English, Old (ca.450-1100)",
            "anp", "", "ll_CC", "Angika",
            "apa", "", "ll_CC", "Apache languages",
            "ara", "ar", "ar_SA", "Arabic",
            "arc", "", "ll_CC", "Official Aramaic (700-300 BCE); Imperial Aramaic (700-300 BCE)",
            "arg", "an", "an_", "Aragonese",
            "arm,hye", "hy", "hy_AM", "Armenian",
            "arn", "", "ll_CC", "Mapudungun; Mapuche",
            "arp", "", "ll_CC", "Arapaho",
            "art", "", "ll_CC", "Artificial languages",
            "arw", "", "ll_CC", "Arawak",
            "ase", "", "ase_US", "ASL",
            "asm", "as", "as_", "Assamese",
            "ast", "", "ll_CC", "Asturian; Bable; Leonese; Asturleonese",
            "ath", "", "ll_CC", "Athapascan languages",
            "aus", "", "ll_CC", "Australian languages",
            "ava", "av", "av_", "Avaric",
            "ave", "ae", "ae_", "Avestan",
            "awa", "", "ll_CC", "Awadhi",
            "aym", "ay", "ay_", "Aymara",
            "aze", "az", "az_AZ", "Azerbaijani",
            "bad", "", "ll_CC", "Banda languages",
            "bai", "", "ll_CC", "Bamileke languages",
            "bak", "ba", "ba_", "Bashkir",
            "bal", "", "ll_CC", "Baluchi",
            "bam", "bm", "bm_", "Bambara",
            "ban", "", "ll_CC", "Balinese",
            "baq,eus", "eu", "eu_ES", "Basque",
            "bas", "", "ll_CC", "Basa",
            "bat", "", "ll_CC", "Baltic languages",
            "bej", "", "ll_CC", "Beja; Bedawiyet",
            "bel", "be", "be_BY", "Belarusian",
            "bem", "", "ll_CC", "Bemba",
            "ben", "bn", "bn_BD", "Bengali",
            "ber", "", "ll_CC", "Berber languages",
            "bho", "", "ll_CC", "Bhojpuri",
            "bih", "bh", "bh_", "Bihari languages",
            "bik", "", "ll_CC", "Bikol",
            "bin", "", "ll_CC", "Bini; Edo",
            "bis", "bi", "bi_VU", "Bislama",
            "bla", "", "ll_CC", "Siksika",
            "bnt", "", "ll_CC", "Bantu languages",
            "bos", "bs", "bs_BA", "Bosnian",
            "bra", "", "ll_CC", "Braj",
            "bre", "br", "br_", "Breton",
            "btk", "", "ll_CC", "Batak languages",
            "bua", "", "ll_CC", "Buriat",
            "bug", "", "ll_CC", "Buginese",
            "bul", "bg", "bg_BG", "Bulgarian",
            "bur,mya", "my", "my_MM", "Burmese",
            "byn", "", "ll_CC", "Blin; Bilin",
            "cad", "", "ll_CC", "Caddo",
            "cai", "", "ll_CC", "Central American Indian languages",
            "car", "", "ll_CC", "Galibi Carib",
            "cat", "ca", "ca_ES", "Catalan; Valencian",
            "cau", "", "ll_CC", "Caucasian languages",
            "ceb", "", "ceb_PH", "Cebuano",
            "cel", "", "ll_CC", "Celtic languages",
            "cha", "ch", "ch_", "Chamorro",
            "chb", "", "ll_CC", "Chibcha",
            "che", "ce", "ce_", "Chechen",
            "chg", "", "ll_CC", "Chagatai",
            "chk", "", "ll_CC", "Chuukese",
            "chm", "", "ll_CC", "Mari",
            "chn", "", "ll_CC", "Chinook jargon",
            "cho", "", "ll_CC", "Choctaw",
            "chp", "", "ll_CC", "Chipewyan; Dene Suline",
            "chr", "", "ll_CC", "Cherokee",
            "chu", "cu", "cu_", "Church Slavic; Old Slavonic; Church Slavonic; Old Bulgarian; Old Church Slavonic",
            "chv", "cv", "cv_", "Chuvash",
            "chy", "", "ll_CC", "Cheyenne",
            "cmc", "", "ll_CC", "Chamic languages",
            "cnr", "", "ll_CC", "Montenegrin",
            "cop", "", "ll_CC", "Coptic",
            "cor", "kw", "kw_", "Cornish",
            "cos", "co", "co_FR", "Corsican",
            "cpe", "", "ll_CC", "Creoles and pidgins, English based",
            "cpf", "", "ll_CC", "Creoles and pidgins, French-based",
            "cpp", "", "ll_CC", "Creoles and pidgins, Portuguese-based",
            "cre", "cr", "cr_", "Cree",
            "crh", "", "ll_CC", "Crimean Tatar; Crimean Turkish",
            "crp", "", "ll_CC", "Creoles and pidgins",
            "csb", "", "ll_CC", "Kashubian",
            "cus", "", "ll_CC", "Cushitic languages",
            "cze,ces", "cs", "cs_CZ", "Czech",
            "dak", "", "ll_CC", "Dakota",
            "dan", "da", "da_DK", "Danish",
            "dar", "", "ll_CC", "Dargwa",
            "day", "", "ll_CC", "Land Dayak languages",
            "del", "", "ll_CC", "Delaware",
            "den", "", "ll_CC", "Slave (Athapascan)",
            "ger,deu", "de", "de_", "German",
            "dgr", "", "ll_CC", "Dogrib",
            "din", "", "ll_CC", "Dinka",
            "div", "dv", "dv_", "Divehi; Dhivehi; Maldivian",
            "doi", "", "ll_CC", "Dogri",
            "dra", "", "ll_CC", "Dravidian languages",
            "dsb", "", "ll_CC", "Lower Sorbian",
            "dua", "", "ll_CC", "Duala",
            "dum", "", "ll_CC", "Dutch, Middle (ca.1050-1350)",
            "dut,nld", "nl", "nl_NL", "Dutch; Flemish",
            "dyu", "", "ll_CC", "Dyula",
            "dzo", "dz", "dz_", "Dzongkha",
            "efi", "", "ll_CC", "Efik",
            "egy", "", "ll_CC", "Egyptian (Ancient)",
            "eka", "", "ll_CC", "Ekajuk",
            "gre,ell", "el", "el_", "Greek, Modern (1453-)",
            "elx", "", "ll_CC", "Elamite",
            "eng", "en", "en_US", "English",
            "enm", "", "ll_CC", "English, Middle (1100-1500)",
            "epo", "eo", "eo_US", "Esperanto",
            "est", "et", "et_EE", "Estonian",
            "ewe", "ee", "ee_", "Ewe",
            "ewo", "", "ll_CC", "Ewondo",
            "fan", "", "ll_CC", "Fang",
            "fao", "fo", "fo_", "Faroese",
            "fat", "", "ll_CC", "Fanti",
            "fij", "fj", "fj_FJ", "Fijian",
            "fil", "", "ll_CC", "Filipino; Pilipino",
            "fin", "fi", "fi_FI", "Finnish",
            "fiu", "", "ll_CC", "Finno-Ugrian languages",
            "fon", "", "ll_CC", "Fon",
            "fre,fra", "fr", "fr_FR", "French",
            "frm", "", "ll_CC", "French, Middle (ca.1400-1600)",
            "fro", "", "ll_CC", "French, Old (842-ca.1400)",
            "frr", "", "ll_CC", "Northern Frisian",
            "frs", "", "ll_CC", "Eastern Frisian",
            "fry", "fy", "fy_NL", "Western Frisian",
            "ful", "ff", "ff_", "Fulah",
            "fur", "", "ll_CC", "Friulian",
            "gaa", "", "ll_CC", "Ga",
            "gay", "", "ll_CC", "Gayo",
            "gba", "", "ll_CC", "Gbaya",
            "gem", "", "ll_CC", "Germanic languages",
            "geo,kat", "ka", "ka_GE", "Georgian",
            "gez", "", "ll_CC", "Geez",
            "gil", "", "ll_CC", "Gilbertese",
            "gla", "gd", "gd_", "Gaelic; Scottish Gaelic",
            "gle", "ga", "ga_", "Irish",
            "glg", "gl", "gl_ES", "Galician",
            "glv", "gv", "gv_", "Manx",
            "gmh", "", "ll_CC", "German, Middle High (ca.1050-1500)",
            "goh", "", "ll_CC", "German, Old High (ca.750-1050)",
            "gon", "", "ll_CC", "Gondi",
            "gor", "", "ll_CC", "Gorontalo",
            "got", "", "ll_CC", "Gothic",
            "grb", "", "ll_CC", "Grebo",
            "grc", "", "ll_CC", "Greek, Ancient (to 1453)",
            "grn", "gn", "gn_", "Guarani",
            "gsw", "", "ll_CC", "Swiss German; Alemannic; Alsatian",
            "guj", "gu", "gu_", "Gujarati",
            "gwi", "", "ll_CC", "Gwich'in",
            "hai", "", "ll_CC", "Haida",
            "hat", "ht", "ht_", "Haitian; Haitian Creole",
            "hau", "ha", "ha_", "Hausa",
            "haw", "", "ll_CC", "Hawaiian",
            "heb", "he", "he_", "Hebrew",
            "her", "hz", "hz_", "Herero",
            "hil", "", "ll_CC", "Hiligaynon",
            "him", "", "ll_CC", "Himachali languages; Western Pahari languages",
            "hin", "hi", "hi_IN", "Hindi",
            "hit", "", "ll_CC", "Hittite",
            "hmn", "", "ll_CC", "Hmong; Mong",
            "hmo", "ho", "ho_", "Hiri Motu",
            "hrv", "hr", "hr_HR", "Croatian",
            "hsb", "", "ll_CC", "Upper Sorbian",
            "hun", "hu", "hu_", "Hungarian",
            "hup", "", "ll_CC", "Hupa",
            "iba", "", "ll_CC", "Iban",
            "ibo", "ig", "ig_", "Igbo",
            "ice,isl", "is", "ll_CC", "Icelandic",
            "ido", "io", "io_", "Ido",
            "iii", "ii", "ii_", "Sichuan Yi; Nuosu",
            "ijo", "", "ll_CC", "Ijo languages",
            "iku", "iu", "iu_", "Inuktitut",
            "ile", "ie", "ie_", "Interlingue; Occidental",
            "ilo", "", "ll_CC", "Iloko",
            "ina", "ia", "ia_", "Interlingua (International Auxiliary Language Association)",
            "inc", "", "ll_CC", "Indic languages",
            "ind", "id", "id_", "Indonesian",
            "ine", "", "ll_CC", "Indo-European languages",
            "inh", "", "ll_CC", "Ingush",
            "ipk", "ik", "ik_", "Inupiaq",
            "ira", "", "ll_CC", "Iranian languages",
            "iro", "", "ll_CC", "Iroquoian languages",
            "ita", "it", "it_IT", "Italian",
            "jav", "jv", "ll_CC", "Javanese",
            "jbo", "", "ll_CC", "Lojban",
            "jpn", "ja", "ja_JP", "Japanese",
            "jpr", "", "ll_CC", "Judeo-Persian",
            "jrb", "", "ll_CC", "Judeo-Arabic",
            "kaa", "", "ll_CC", "Kara-Kalpak",
            "kab", "", "ll_CC", "Kabyle",
            "kac", "", "ll_CC", "Kachin; Jingpho",
            "kal", "kl", "kl_", "Kalaallisut; Greenlandic",
            "kam", "", "ll_CC", "Kamba",
            "kan", "kn", "kn_", "Kannada",
            "kar", "", "ll_CC", "Karen languages",
            "kas", "ks", "ks_", "Kashmiri",
            "kau", "kr", "kr_", "Kanuri",
            "kaw", "", "ll_CC", "Kawi",
            "kaz", "kk", "kk_", "Kazakh",
            "kbd", "", "ll_CC", "Kabardian",
            "kha", "", "ll_CC", "Khasi",
            "khi", "", "ll_CC", "Khoisan languages",
            "khm", "km", "km_", "Central Khmer",
            "kho", "", "ll_CC", "Khotanese; Sakan",
            "kik", "ki", "ki_", "Kikuyu; Gikuyu",
            "kin", "rw", "rw_", "Kinyarwanda",
            "kir", "ky", "ky_", "Kirghiz; Kyrgyz",
            "kmb", "", "ll_CC", "Kimbundu",
            "kok", "", "ll_CC", "Konkani",
            "kom", "kv", "kv_", "Komi",
            "kon", "kg", "kg_", "Kongo",
            "kor", "ko", "ko_", "Korean",
            "kos", "", "ll_CC", "Kosraean",
            "kpe", "", "ll_CC", "Kpelle",
            "krc", "", "ll_CC", "Karachay-Balkar",
            "krl", "", "ll_CC", "Karelian",
            "kro", "", "ll_CC", "Kru languages",
            "kru", "", "ll_CC", "Kurukh",
            "kua", "kj", "kj_", "Kuanyama; Kwanyama",
            "kum", "", "ll_CC", "Kumyk",
            "kur", "ku", "ku_", "Kurdish",
            "kut", "", "ll_CC", "Kutenai",
            "lad", "", "ll_CC", "Ladino",
            "lah", "", "ll_CC", "Lahnda",
            "lam", "", "ll_CC", "Lamba",
            "lao", "lo", "lo_", "Lao",
            "lat", "la", "la_", "Latin",
            "lav", "lv", "lv_", "Latvian",
            "lez", "", "ll_CC", "Lezghian",
            "lim", "li", "li_", "Limburgan; Limburger; Limburgish",
            "lin", "ln", "ln_", "Lingala",
            "lit", "lt", "lt_", "Lithuanian",
            "lol", "", "ll_CC", "Mongo",
            "loz", "", "ll_CC", "Lozi",
            "ltz", "lb", "lb_", "Luxembourgish; Letzeburgesch",
            "lua", "", "ll_CC", "Luba-Lulua",
            "lub", "lu", "lu_", "Luba-Katanga",
            "lug", "lg", "lg_", "Ganda",
            "lui", "", "ll_CC", "Luiseno",
            "lun", "", "ll_CC", "Lunda",
            "luo", "", "ll_CC", "Luo (Kenya and Tanzania)",
            "lus", "", "ll_CC", "Lushai",
            "mac,mkd", "mk", "mk_", "Macedonian",
            "mad", "", "ll_CC", "Madurese",
            "mag", "", "ll_CC", "Magahi",
            "mah", "mh", "mh_MH", "Marshallese",
            "mai", "", "ll_CC", "Maithili",
            "mak", "", "ll_CC", "Makasar",
            "mal", "ml", "ml_", "Malayalam",
            "man", "", "ll_CC", "Mandingo",
            "mao,mri", "mi", "mi_", "Maori",
            "map", "", "ll_CC", "Austronesian languages",
            "mar", "mr", "mr_", "Marathi",
            "mas", "", "ll_CC", "Masai",
            "may,msa", "ms", "ms_MY", "Malay",
            "", "", "ms_BL", "Bidayuh (Biatah)",
            "mdf", "", "ll_CC", "Moksha",
            "mdr", "", "ll_CC", "Mandar",
            "men", "", "ll_CC", "Mende",
            "mga", "", "ll_CC", "Irish, Middle (900-1200)",
            "mic", "", "ll_CC", "Mi'kmaq; Micmac",
            "min", "", "ll_CC", "Minangkabau",
            "mis", "", "ll_CC", "Uncoded languages",
            "mkh", "", "ll_CC", "Mon-Khmer languages",
            "mlg", "mg", "mg_", "Malagasy",
            "mlt", "mt", "mt_", "Maltese",
            "mnc", "", "ll_CC", "Manchu",
            "mni", "", "ll_CC", "Manipuri",
            "mno", "", "ll_CC", "Manobo languages",
            "moh", "", "ll_CC", "Mohawk",
            "mon", "mn", "mn_", "Mongolian",
            "mos", "", "ll_CC", "Mossi",
            "mul", "", "ll_CC", "Multiple languages",
            "mun", "", "ll_CC", "Munda languages",
            "mus", "", "ll_CC", "Creek",
            "mwl", "", "ll_CC", "Mirandese",
            "mwr", "", "ll_CC", "Marwari",
            "myn", "", "ll_CC", "Mayan languages",
            "myv", "", "ll_CC", "Erzya",
            "nah", "", "ll_CC", "Nahuatl languages",
            "nai", "", "ll_CC", "North American Indian languages",
            "nap", "", "ll_CC", "Neapolitan",
            "nau", "na", "na_", "Nauru",
            "nav", "nv", "nv_", "Navajo; Navaho",
            "nbl", "nr", "nr_", "Ndebele, South; South Ndebele",
            "nde", "nd", "nd_", "Ndebele, North; North Ndebele",
            "ndo", "ng", "ng_", "Ndonga",
            "nds", "", "ll_CC", "Low German; Low Saxon; German, Low; Saxon, Low",
            "nep", "ne", "ne_", "Nepali",
            "new", "", "ll_CC", "Nepal Bhasa; Newari",
            "nia", "", "ll_CC", "Nias",
            "nic", "", "ll_CC", "Niger-Kordofanian languages",
            "niu", "", "ll_CC", "Niuean",
            "nno", "nn", "nn_", "Norwegian Nynorsk; Nynorsk, Norwegian",
            "nob", "nb", "nb_", "Bokmål, Norwegian; Norwegian Bokmål",
            "nog", "", "ll_CC", "Nogai",
            "non", "", "ll_CC", "Norse, Old",
            "nor", "no", "no_", "Norwegian",
            "nqo", "", "ll_CC", "N'Ko",
            "nso", "", "ll_CC", "Pedi; Sepedi; Northern Sotho",
            "nub", "", "ll_CC", "Nubian languages",
            "nwc", "", "ll_CC", "Classical Newari; Old Newari; Classical Nepal Bhasa",
            "nya", "ny", "ny_MW", "Chichewa; Chewa; Nyanja",
            "nym", "", "ll_CC", "Nyamwezi",
            "nyn", "", "ll_CC", "Nyankole",
            "nyo", "", "ll_CC", "Nyoro",
            "nzi", "", "ll_CC", "Nzima",
            "oci", "oc", "oc_", "Occitan (post 1500)",
            "oji", "oj", "oj_", "Ojibwa",
            "ori", "or", "or_", "Oriya",
            "orm", "om", "om_", "Oromo",
            "osa", "", "ll_CC", "Osage",
            "oss", "os", "os_", "Ossetian; Ossetic",
            "ota", "", "ll_CC", "Turkish, Ottoman (1500-1928)",
            "oto", "", "ll_CC", "Otomian languages",
            "paa", "", "ll_CC", "Papuan languages",
            "pag", "", "ll_CC", "Pangasinan",
            "pal", "", "ll_CC", "Pahlavi",
            "pam", "", "ll_CC", "Pampanga; Kapampangan",
            "pan", "pa", "pa_", "Panjabi; Punjabi",
            "pap", "", "ll_CC", "Papiamento",
            "pau", "", "ll_CC", "Palauan",
            "peo", "", "ll_CC", "Persian, Old (ca.600-400 B.C.)",
            "per,fas", "fa", "fa_", "Persian",
            "phi", "", "ll_CC", "Philippine languages",
            "phn", "", "ll_CC", "Phoenician",
            "pli", "pi", "pi_", "Pali",
            "pol", "pl", "pl_", "Polish",
            "pon", "", "ll_CC", "Pohnpeian",
            "por", "pt", "pt_", "Portuguese",
            "pra", "", "ll_CC", "Prakrit languages",
            "pro", "", "ll_CC", "Provençal, Old (to 1500);Occitan, Old (to 1500)",
            "pus", "ps", "ps_", "Pushto; Pashto",
            "qaa-qtz", "", "ll_CC", "Reserved for local use",
            "que", "qu", "qu_", "Quechua",
            "raj", "", "ll_CC", "Rajasthani",
            "rap", "", "ll_CC", "Rapanui",
            "rar", "", "ll_CC", "Rarotongan; Cook Islands Maori",
            "roa", "", "ll_CC", "Romance languages",
            "roh", "rm", "rm_", "Romansh",
            "rom", "", "ll_CC", "Romany",
            "rum,ron", "ro", "ro_", "Romanian; Moldavian; Moldovan",
            "run", "rn", "rn_", "Rundi",
            "rup", "", "ll_CC", "Aromanian; Arumanian; Macedo-Romanian",
            "rus", "ru", "ru_", "Russian",
            "sad", "", "ll_CC", "Sandawe",
            "sag", "sg", "sg_", "Sango",
            "sah", "", "ll_CC", "Yakut",
            "sai", "", "ll_CC", "South American Indian languages",
            "sal", "", "ll_CC", "Salishan languages",
            "sam", "", "ll_CC", "Samaritan Aramaic",
            "san", "sa", "sa_", "Sanskrit",
            "sas", "", "ll_CC", "Sasak",
            "sat", "", "ll_CC", "Santali",
            "scn", "", "ll_CC", "Sicilian",
            "sco", "", "ll_CC", "Scots",
            "sel", "", "ll_CC", "Selkup",
            "sem", "", "ll_CC", "Semitic languages",
            "sga", "", "ll_CC", "Irish, Old (to 900)",
            "sgn", "", "ll_CC", "Sign Languages",
            "shn", "", "ll_CC", "Shan",
            "sid", "", "ll_CC", "Sidamo",
            "sin", "si", "si_", "Sinhala; Sinhalese",
            "sio", "", "ll_CC", "Siouan languages",
            "sit", "", "ll_CC", "Sino-Tibetan languages",
            "sla", "", "ll_CC", "Slavic languages",
            "slo,slk", "sk", "sk_", "Slovak",
            "slv", "sl", "sl_", "Slovenian",
            "sma", "", "ll_CC", "Southern Sami",
            "sme", "se", "se_", "Northern Sami",
            "smi", "", "ll_CC", "Sami languages",
            "smj", "", "ll_CC", "Lule Sami",
            "smn", "", "ll_CC", "Inari Sami",
            "smo", "sm", "sm_", "Samoan",
            "sms", "", "ll_CC", "Skolt Sami",
            "sna", "sn", "sn_", "Shona",
            "snd", "sd", "sd_", "Sindhi",
            "snk", "", "ll_CC", "Soninke",
            "sog", "", "ll_CC", "Sogdian",
            "som", "so", "so_", "Somali",
            "son", "", "ll_CC", "Songhai languages",
            "sot", "st", "st_", "Sotho, Southern",
            "spa", "es", "es_ES", "Spanish; Castilian",
            "srd", "sc", "ll_CC", "Sardinian",
            "srn", "", "ll_CC", "Sranan Tongo",
            "srp", "sr", "sr_", "Serbian",
            "srr", "", "ll_CC", "Serer",
            "ssa", "", "ll_CC", "Nilo-Saharan languages",
            "ssw", "ss", "ss_", "Swati",
            "suk", "", "ll_CC", "Sukuma",
            "sun", "su", "su_", "Sundanese",
            "sus", "", "ll_CC", "Susu",
            "sux", "", "ll_CC", "Sumerian",
            "swa", "sw", "sw_", "Swahili",
            "swe", "sv", "sv_", "Swedish",
            "syc", "", "ll_CC", "Classical Syriac",
            "syr", "", "ll_CC", "Syriac",
            "tah", "ty", "ty_", "Tahitian",
            "tai", "", "ll_CC", "Tai languages",
            "tam", "ta", "ta_", "Tamil",
            "tat", "tt", "tt_", "Tatar",
            "tel", "te", "te_", "Telugu",
            "tem", "", "ll_CC", "Timne",
            "ter", "", "ll_CC", "Tereno",
            "tet", "", "ll_CC", "Tetum",
            "tgk", "tg", "tg_", "Tajik",
            "tgl", "tl", "tl_", "Tagalog",
            "tha", "th", "th_", "Thai",
            "tib,bod", "bo", "bo_", "Tibetan",
            "tig", "", "ll_CC", "Tigre",
            "tir", "ti", "ti_", "Tigrinya",
            "tiv", "", "ll_CC", "Tiv",
            "tkl", "", "ll_CC", "Tokelau",
            "tlh", "", "ll_CC", "Klingon; tlhIngan-Hol",
            "tli", "", "ll_CC", "Tlingit",
            "tmh", "", "ll_CC", "Tamashek",
            "tog", "", "ll_CC", "Tonga (Nyasa)",
            "ton", "to", "to_", "Tonga (Tonga Islands)",
            "tpi", "", "ll_CC", "Tok Pisin",
            "tsi", "", "ll_CC", "Tsimshian",
            "tsn", "tn", "tn_", "Tswana",
            "tso", "ts", "ts_", "Tsonga",
            "tuk", "tk", "tk_", "Turkmen",
            "tum", "", "ll_CC", "Tumbuka",
            "tup", "", "ll_CC", "Tupi languages",
            "tur", "tr", "tr_", "Turkish",
            "tut", "", "ll_CC", "Altaic languages",
            "tvl", "", "ll_CC", "Tuvalu",
            "twi", "tw", "tw_", "Twi",
            "tyv", "", "ll_CC", "Tuvinian",
            "udm", "", "ll_CC", "Udmurt",
            "uga", "", "ll_CC", "Ugaritic",
            "uig", "ug", "ug_", "Uighur; Uyghur",
            "ukr", "uk", "uk_", "Ukrainian",
            "umb", "", "ll_CC", "Umbundu",
            "und", "", "ll_CC", "Undetermined",
            "urd", "ur", "ur_", "Urdu",
            "uzb", "uz", "uz_", "Uzbek",
            "vai", "", "ll_CC", "Vai",
            "ven", "ve", "ve_", "Venda",
            "vie", "vi", "vi_", "Vietnamese",
            "vol", "vo", "vo_", "Volapük",
            "vot", "", "ll_CC", "Votic",
            "wak", "", "ll_CC", "Wakashan languages",
            "wal", "", "ll_CC", "Wolaitta; Wolaytta",
            "war", "", "ll_CC", "Waray",
            "was", "", "ll_CC", "Washo",
            "wel,cym", "cy", "cy_", "Welsh",
            "wen", "", "ll_CC", "Sorbian languages",
            "wln", "wa", "wa_", "Walloon",
            "wol", "wo", "wo_", "Wolof",
            "xal", "", "ll_CC", "Kalmyk; Oirat",
            "xho", "xh", "xh_", "Xhosa",
            "yao", "", "ll_CC", "Yao",
            "yap", "", "ll_CC", "Yapese",
            "yid", "yi", "yi_", "Yiddish",
            "yor", "yo", "yo_", "Yoruba",
            "ypk", "", "ll_CC", "Yupik languages",
            "zap", "", "ll_CC", "Zapotec",
            "zbl", "", "ll_CC", "Blissymbols; Blissymbolics; Bliss",
            "yue", "", "yue_CN", "Cantonese",
            "zen", "", "ll_CC", "Zenaga",
            "zgh", "", "ll_CC", "Standard Moroccan Tamazight",
            "zha", "za", "za_", "Zhuang; Chuang",
            "chi,zho", "zh", "cmn_TW", "Chinese",
            "znd", "", "ll_CC", "Zande languages",
            "zul", "zu", "zu_", "Zulu",
            "zun", "", "ll_CC", "Zuni",
            "zxx", "", "ll_CC", "No linguistic content; Not applicable",
            "zza", "", "ll_CC", "Zaza; Dimili; Dimli; Kirdki; Kirmanjki; Zazaki",
            "Zazaki", "", "ll_CC", "",
            "", "", "ll_CC", ""
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Converters
{
    public static class ConvertTimeZone
    {
        private static List<string> _WindowsTimeZoneIDs;

        public static List<string> WindowsTimeZoneIDs
        {
            get
            {
                if (_WindowsTimeZoneIDs == null)
                {
                    _WindowsTimeZoneIDs = new List<string>()
                    {
                        "Afghanistan Standard Time",
                        "Alaskan Standard Time",
                        "Aleutian Standard Time",
                        "Altai Standard Time",
                        "Arab Standard Time",
                        "Arabian Standard Time",
                        "Arabic Standard Time",
                        "Argentina Standard Time",
                        "Astrakhan Standard Time",
                        "Atlantic Standard Time",
                        "AUS Central Standard Time",
                        "Aus Central W. Standard Time",
                        "AUS Eastern Standard Time",
                        "Azerbaijan Standard Time",
                        "Azores Standard Time",
                        "Bahia Standard Time",
                        "Bangladesh Standard Time",
                        "Belarus Standard Time",
                        "Bougainville Standard Time",
                        "Cabo Verde Standard Time",
                        "Canada Central Standard Time",
                        "Caucasus Standard Time",
                        "Cen. Australia Standard Time",
                        "Central America Standard Time",
                        "Central Asia Standard Time",
                        "Central Brazilian Standard Time",
                        "Central Europe Standard Time",
                        "Central European Standard Time",
                        "Central Pacific Standard Time",
                        "Central Standard Time (Mexico)",
                        "Central Standard Time",
                        "Chatham Islands Standard Time",
                        "China Standard Time",
                        "Coordinated Universal Time",
                        "Cuba Standard Time",
                        "Dateline Standard Time",
                        "E. Africa Standard Time",
                        "E. Australia Standard Time",
                        "E. Europe Standard Time",
                        "E. South America Standard Time",
                        "Easter Island Standard Time",
                        "Eastern Standard Time (Mexico)",
                        "Eastern Standard Time",
                        "Egypt Standard Time",
                        "Fiji Standard Time",
                        "FLE Standard Time",
                        "Georgian Standard Time",
                        "GMT Standard Time",
                        "Greenland Standard Time",
                        "Greenwich Standard Time",
                        "GTB Standard Time",
                        "Haiti Standard Time",
                        "Hawaiian Standard Time",
                        "India Standard Time",
                        "Iran Standard Time",
                        "Jerusalem Standard Time",
                        "Jordan Standard Time",
                        "Kamchatka Standard Time",
                        "Korea Standard Time",
                        "Libya Standard Time",
                        "Line Islands Standard Time",
                        "Lord Howe Standard Time",
                        "Magadan Standard Time",
                        "Magallanes Standard Time",
                        "Malay Peninsula Standard Time",
                        "Marquesas Standard Time",
                        "Mauritius Standard Time",
                        "Mid-Atlantic Standard Time",
                        "Middle East Standard Time",
                        "Montevideo Standard Time",
                        "Morocco Standard Time",
                        "Mountain Standard Time (Mexico)",
                        "Mountain Standard Time",
                        "Myanmar Standard Time",
                        "Namibia Standard Time",
                        "Nepal Standard Time",
                        "New Zealand Standard Time",
                        "Newfoundland Standard Time",
                        "Norfolk Standard Time",
                        "North Korea Standard Time",
                        "Novosibirsk Standard Time",
                        "Omsk Standard Time",
                        "Pacific SA Standard Time",
                        "Pacific Standard Time (Mexico)",
                        "Pacific Standard Time",
                        "Pakistan Standard Time",
                        "Paraguay Standard Time",
                        "Qyzylorda Standard Time",
                        "Romance Standard Time",
                        "Russia TZ 1 Standard Time",
                        "Russia TZ 10 Standard Time",
                        "Russia TZ 11 Standard Time",
                        "Russia TZ 2 Standard Time",
                        "Russia TZ 3 Standard Time",
                        "Russia TZ 4 Standard Time",
                        "Russia TZ 6 Standard Time",
                        "Russia TZ 7 Standard Time",
                        "Russia TZ 8 Standard Time",
                        "Russia TZ 9 Standard Time",
                        "SA Eastern Standard Time",
                        "SA Pacific Standard Time",
                        "SA Western Standard Time",
                        "Saint Pierre Standard Time",
                        "Sakhalin Standard Time",
                        "Samoa Standard Time",
                        "Sao Tome Standard Time",
                        "Saratov Standard Time",
                        "SE Asia Standard Time",
                        "South Africa Standard Time",
                        "Sri Lanka Standard Time",
                        "Sudan Standard Time",
                        "Syria Standard Time",
                        "Taipei Standard Time",
                        "Tasmania Standard Time",
                        "Tocantins Standard Time",
                        "Tokyo Standard Time",
                        "Tomsk Standard Time",
                        "Tonga Standard Time",
                        "Transbaikal Standard Time",
                        "Turkey Standard Time",
                        "Turks and Caicos Standard Time",
                        "Ulaanbaatar Standard Time",
                        "US Eastern Standard Time",
                        "US Mountain Standard Time",
                        "UTC-02",
                        "UTC-08",
                        "UTC-09",
                        "UTC-11",
                        "UTC+12",
                        "UTC+13",
                        "Venezuela Standard Time",
                        "Volgograd Standard Time",
                        "W. Australia Standard Time",
                        "W. Central Africa Standard Time",
                        "W. Europe Standard Time",
                        "W. Mongolia Standard Time",
                        "West Asia Standard Time",
                        "West Bank Gaza Standard Time",
                        "West Pacific Standard Time",
                    };
                }

                return _WindowsTimeZoneIDs;
            }
        }

        public static bool IsWindowsTimeZone(string timeZoneID)
        {
            if (String.IsNullOrEmpty(timeZoneID))
                return false;

            if (WindowsTimeZoneIDs.Contains(timeZoneID))
                return true;

            return false;
        }

        private static Dictionary<string, List<string>> _WindowsToOlsenDictionary;
        public static Dictionary<string, List<string>> WindowsToOlsenDictionary
        {
            get
            {
                if (_WindowsToOlsenDictionary == null)
                {
                    _WindowsToOlsenDictionary = new Dictionary<string, List<string>>()
                    {
                        {"AUS Central Standard Time", new List<string>{"Australia/Darwin"}},
                        {"AUS Eastern Standard Time", new List<string>{"Australia/Sydney", "Australia/Melbourne"}},
                        {"Afghanistan Standard Time", new List<string>{"Asia/Kabul"}},
                        {"Alaskan Standard Time", new List<string>{"America/Anchorage", "America/Juneau", "America/Metlakatla", "America/Nome", "America/Sitka", "America/Yakutat"}},
                        {"Aleutian Standard Time", new List<string>{"America/Adak"}},
                        {"Altai Standard Time", new List<string>{"Asia/Barnaul"}},
                        {"Arab Standard Time", new List<string>{"Asia/Riyadh", "Asia/Bahrain", "Asia/Kuwait", "Asia/Qatar", "Asia/Aden"}},
                        {"Arabian Standard Time", new List<string>{"Asia/Dubai", "Asia/Muscat", "Etc/GMT-4"}},
                        {"Arabic Standard Time", new List<string>{"Asia/Baghdad"}},
                        {"Argentina Standard Time", new List<string>{"America/Buenos_Aires", "America/Argentina/La_Rioja", "America/Argentina/Rio_Gallegos", "America/Argentina/Salta", "America/Argentina/San_Juan", "America/Argentina/San_Luis", "America/Argentina/Tucuman", "America/Argentina/Ushuaia", "America/Catamarca", "America/Cordoba", "America/Jujuy", "America/Mendoza"}},
                        {"Astrakhan Standard Time", new List<string>{"Europe/Astrakhan", "Europe/Ulyanovsk"}},
                        {"Atlantic Standard Time", new List<string>{"America/Halifax", "Atlantic/Bermuda", "America/Glace_Bay", "America/Goose_Bay", "America/Moncton", "America/Thule"}},
                        {"Aus Central W. Standard Time", new List<string>{"Australia/Eucla"}},
                        {"Azerbaijan Standard Time", new List<string>{"Asia/Baku"}},
                        {"Azores Standard Time", new List<string>{"Atlantic/Azores", "America/Scoresbysund"}},
                        {"Bahia Standard Time", new List<string>{"America/Bahia"}},
                        {"Bangladesh Standard Time", new List<string>{"Asia/Dhaka", "Asia/Thimphu"}},
                        {"Belarus Standard Time", new List<string>{"Europe/Minsk"}},
                        {"Bougainville Standard Time", new List<string>{"Pacific/Bougainville"}},
                        {"Canada Central Standard Time", new List<string>{"America/Regina", "America/Swift_Current"}},
                        {"Cape Verde Standard Time", new List<string>{"Atlantic/Cape_Verde", "Etc/GMT+1"}},
                        {"Caucasus Standard Time", new List<string>{"Asia/Yerevan"}},
                        {"Cen. Australia Standard Time", new List<string>{"Australia/Adelaide", "Australia/Broken_Hill"}},
                        {"Central America Standard Time", new List<string>{"America/Guatemala", "America/Belize", "America/Costa_Rica", "Pacific/Galapagos", "America/Tegucigalpa", "America/Managua", "America/El_Salvador", "Etc/GMT+6"}},
                        {"Central Asia Standard Time", new List<string>{"Asia/Almaty", "Antarctica/Vostok", "Asia/Urumqi", "Indian/Chagos", "Asia/Bishkek", "Asia/Qostanay", "Etc/GMT-6"}},
                        {"Central Brazilian Standard Time", new List<string>{"America/Cuiaba", "America/Campo_Grande"}},
                        {"Central Europe Standard Time", new List<string>{"Europe/Budapest", "Europe/Tirane", "Europe/Prague", "Europe/Podgorica", "Europe/Belgrade", "Europe/Ljubljana", "Europe/Bratislava"}},
                        {"Central European Standard Time", new List<string>{"Europe/Warsaw", "Europe/Sarajevo", "Europe/Zagreb", "Europe/Skopje"}},
                        {"Central Pacific Standard Time", new List<string>{"Pacific/Guadalcanal", "Antarctica/Macquarie", "Pacific/Ponape", "Pacific/Kosrae", "Pacific/Noumea", "Pacific/Efate", "Etc/GMT-11"}},
                        {"Central Standard Time", new List<string>{"America/Chicago", "America/Winnipeg", "America/Rainy_River", "America/Rankin_Inlet", "America/Resolute", "America/Matamoros", "America/Indiana/Knox", "America/Indiana/Tell_City", "America/Menominee", "America/North_Dakota/Beulah", "America/North_Dakota/Center", "America/North_Dakota/New_Salem", "CST6CDT"}},
                        {"Central Standard Time (Mexico)", new List<string>{"America/Mexico_City", "America/Bahia_Banderas", "America/Merida", "America/Monterrey"}},
                        {"Chatham Islands Standard Time", new List<string>{"Pacific/Chatham"}},
                        {"China Standard Time", new List<string>{"Asia/Shanghai", "Asia/Hong_Kong", "Asia/Macau"}},
                        {"Cuba Standard Time", new List<string>{"America/Havana"}},
                        {"Dateline Standard Time", new List<string>{"Etc/GMT+12"}},
                        {"E. Africa Standard Time", new List<string>{"Africa/Nairobi", "Antarctica/Syowa", "Africa/Djibouti", "Africa/Asmera", "Africa/Addis_Ababa", "Indian/Comoro", "Indian/Antananarivo", "Africa/Mogadishu", "Africa/Juba", "Africa/Dar_es_Salaam", "Africa/Kampala", "Indian/Mayotte", "Etc/GMT-3"}},
                        {"E. Australia Standard Time", new List<string>{"Australia/Brisbane", "Australia/Lindeman"}},
                        {"E. Europe Standard Time", new List<string>{"Europe/Chisinau"}},
                        {"E. South America Standard Time", new List<string>{"America/Sao_Paulo"}},
                        {"Easter Island Standard Time", new List<string>{"Pacific/Easter"}},
                        {"Eastern Standard Time", new List<string>{"America/New_York", "America/Nassau", "America/Toronto", "America/Iqaluit", "America/Montreal", "America/Nipigon", "America/Pangnirtung", "America/Thunder_Bay", "America/Detroit", "America/Indiana/Petersburg", "America/Indiana/Vincennes", "America/Indiana/Winamac", "America/Kentucky/Monticello", "America/Louisville", "EST5EDT"}},
                        {"Eastern Standard Time (Mexico)", new List<string>{"America/Cancun"}},
                        {"Egypt Standard Time", new List<string>{"Africa/Cairo"}},
                        {"Ekaterinburg Standard Time", new List<string>{"Asia/Yekaterinburg"}},
                        {"FLE Standard Time", new List<string>{"Europe/Kiev", "Europe/Mariehamn", "Europe/Sofia", "Europe/Tallinn", "Europe/Helsinki", "Europe/Vilnius", "Europe/Riga", "Europe/Uzhgorod", "Europe/Zaporozhye"}},
                        {"Fiji Standard Time", new List<string>{"Pacific/Fiji"}},
                        {"GMT Standard Time", new List<string>{"Europe/London", "Atlantic/Canary", "Atlantic/Faeroe", "Europe/Guernsey", "Europe/Dublin", "Europe/Isle_of_Man", "Europe/Jersey", "Europe/Lisbon", "Atlantic/Madeira"}},
                        {"GTB Standard Time", new List<string>{"Europe/Bucharest", "Asia/Nicosia", "Asia/Famagusta", "Europe/Athens"}},
                        {"Georgian Standard Time", new List<string>{"Asia/Tbilisi"}},
                        {"Greenland Standard Time", new List<string>{"America/Godthab"}},
                        {"Greenwich Standard Time", new List<string>{"Atlantic/Reykjavik", "Africa/Ouagadougou", "Africa/Abidjan", "Africa/Accra", "Africa/Banjul", "Africa/Conakry", "Africa/Bissau", "Africa/Monrovia", "Africa/Bamako", "Africa/Nouakchott", "Atlantic/St_Helena", "Africa/Freetown", "Africa/Dakar", "Africa/Lome"}},
                        {"Haiti Standard Time", new List<string>{"America/Port-au-Prince"}},
                        {"Hawaiian Standard Time", new List<string>{"Pacific/Honolulu", "Pacific/Rarotonga", "Pacific/Tahiti", "Pacific/Johnston", "Etc/GMT+10"}},
                        {"India Standard Time", new List<string>{"Asia/Calcutta"}},
                        {"Iran Standard Time", new List<string>{"Asia/Tehran"}},
                        {"Israel Standard Time", new List<string>{"Asia/Jerusalem"}},
                        {"Jordan Standard Time", new List<string>{"Asia/Amman"}},
                        {"Kaliningrad Standard Time", new List<string>{"Europe/Kaliningrad"}},
                        {"Korea Standard Time", new List<string>{"Asia/Seoul"}},
                        {"Libya Standard Time", new List<string>{"Africa/Tripoli"}},
                        {"Line Islands Standard Time", new List<string>{"Pacific/Kiritimati", "Etc/GMT-14"}},
                        {"Lord Howe Standard Time", new List<string>{"Australia/Lord_Howe"}},
                        {"Magadan Standard Time", new List<string>{"Asia/Magadan"}},
                        {"Magallanes Standard Time", new List<string>{"America/Punta_Arenas"}},
                        {"Marquesas Standard Time", new List<string>{"Pacific/Marquesas"}},
                        {"Mauritius Standard Time", new List<string>{"Indian/Mauritius", "Indian/Reunion", "Indian/Mahe"}},
                        {"Middle East Standard Time", new List<string>{"Asia/Beirut"}},
                        {"Montevideo Standard Time", new List<string>{"America/Montevideo"}},
                        {"Morocco Standard Time", new List<string>{"Africa/Casablanca", "Africa/El_Aaiun"}},
                        {"Mountain Standard Time", new List<string>{"America/Denver", "America/Edmonton", "America/Cambridge_Bay", "America/Inuvik", "America/Yellowknife", "America/Ojinaga", "America/Boise", "MST7MDT"}},
                        {"Mountain Standard Time (Mexico)", new List<string>{"America/Chihuahua", "America/Mazatlan"}},
                        {"Myanmar Standard Time", new List<string>{"Asia/Rangoon", "Indian/Cocos"}},
                        {"N. Central Asia Standard Time", new List<string>{"Asia/Novosibirsk"}},
                        {"Namibia Standard Time", new List<string>{"Africa/Windhoek"}},
                        {"Nepal Standard Time", new List<string>{"Asia/Katmandu"}},
                        {"New Zealand Standard Time", new List<string>{"Pacific/Auckland", "Antarctica/McMurdo"}},
                        {"Newfoundland Standard Time", new List<string>{"America/St_Johns"}},
                        {"Norfolk Standard Time", new List<string>{"Pacific/Norfolk"}},
                        {"North Asia East Standard Time", new List<string>{"Asia/Irkutsk"}},
                        {"North Asia Standard Time", new List<string>{"Asia/Krasnoyarsk", "Asia/Novokuznetsk"}},
                        {"North Korea Standard Time", new List<string>{"Asia/Pyongyang"}},
                        {"Omsk Standard Time", new List<string>{"Asia/Omsk"}},
                        {"Pacific SA Standard Time", new List<string>{"America/Santiago"}},
                        {"Pacific Standard Time", new List<string>{"America/Los_Angeles", "America/Vancouver", "America/Dawson", "America/Whitehorse", "PST8PDT"}},
                        {"Pacific Standard Time (Mexico)", new List<string>{"America/Tijuana", "America/Santa_Isabel"}},
                        {"Pakistan Standard Time", new List<string>{"Asia/Karachi"}},
                        {"Paraguay Standard Time", new List<string>{"America/Asuncion"}},
                        {"Qyzylorda Standard Time", new List<string>{"Asia/Qyzylorda"}},
                        {"Romance Standard Time", new List<string>{"Europe/Paris", "Europe/Brussels", "Europe/Copenhagen", "Europe/Madrid", "Africa/Ceuta"}},
                        {"Russia Time Zone 10", new List<string>{"Asia/Srednekolymsk"}},
                        {"Russia Time Zone 11", new List<string>{"Asia/Kamchatka", "Asia/Anadyr"}},
                        {"Russia Time Zone 3", new List<string>{"Europe/Samara"}},
                        {"Russian Standard Time", new List<string>{"Europe/Moscow", "Europe/Kirov", "Europe/Simferopol"}},
                        {"SA Eastern Standard Time", new List<string>{"America/Cayenne", "Antarctica/Rothera", "Antarctica/Palmer", "America/Fortaleza", "America/Belem", "America/Maceio", "America/Recife", "America/Santarem", "Atlantic/Stanley", "America/Paramaribo", "Etc/GMT+3"}},
                        {"SA Pacific Standard Time", new List<string>{"America/Bogota", "America/Rio_Branco", "America/Eirunepe", "America/Coral_Harbour", "America/Guayaquil", "America/Jamaica", "America/Cayman", "America/Panama", "America/Lima", "Etc/GMT+5"}},
                        {"SA Western Standard Time", new List<string>{"America/La_Paz", "America/Antigua", "America/Anguilla", "America/Aruba", "America/Barbados", "America/St_Barthelemy", "America/Kralendijk", "America/Manaus", "America/Boa_Vista", "America/Porto_Velho", "America/Blanc-Sablon", "America/Curacao", "America/Dominica", "America/Santo_Domingo", "America/Grenada", "America/Guadeloupe", "America/Guyana", "America/St_Kitts", "America/St_Lucia", "America/Marigot", "America/Martinique", "America/Montserrat", "America/Puerto_Rico", "America/Lower_Princes", "America/Port_of_Spain", "America/St_Vincent", "America/Tortola", "America/St_Thomas", "Etc/GMT+4"}},
                        {"SE Asia Standard Time", new List<string>{"Asia/Bangkok", "Antarctica/Davis", "Indian/Christmas", "Asia/Jakarta", "Asia/Pontianak", "Asia/Phnom_Penh", "Asia/Vientiane", "Asia/Saigon", "Etc/GMT-7"}},
                        {"Saint Pierre Standard Time", new List<string>{"America/Miquelon"}},
                        {"Sakhalin Standard Time", new List<string>{"Asia/Sakhalin"}},
                        {"Samoa Standard Time", new List<string>{"Pacific/Apia"}},
                        {"Sao Tome Standard Time", new List<string>{"Africa/Sao_Tome"}},
                        {"Saratov Standard Time", new List<string>{"Europe/Saratov"}},
                        {"Singapore Standard Time", new List<string>{"Asia/Singapore", "Antarctica/Casey", "Asia/Brunei", "Asia/Makassar", "Asia/Kuala_Lumpur", "Asia/Kuching", "Asia/Manila", "Etc/GMT-8"}},
                        {"South Africa Standard Time", new List<string>{"Africa/Johannesburg", "Africa/Bujumbura", "Africa/Gaborone", "Africa/Lubumbashi", "Africa/Maseru", "Africa/Blantyre", "Africa/Maputo", "Africa/Kigali", "Africa/Mbabane", "Africa/Lusaka", "Africa/Harare", "Etc/GMT-2"}},
                        {"Sri Lanka Standard Time", new List<string>{"Asia/Colombo"}},
                        {"Sudan Standard Time", new List<string>{"Africa/Khartoum"}},
                        {"Syria Standard Time", new List<string>{"Asia/Damascus"}},
                        {"Taipei Standard Time", new List<string>{"Asia/Taipei"}},
                        {"Tasmania Standard Time", new List<string>{"Australia/Hobart", "Australia/Currie"}},
                        {"Tocantins Standard Time", new List<string>{"America/Araguaina"}},
                        {"Tokyo Standard Time", new List<string>{"Asia/Tokyo", "Asia/Jayapura", "Pacific/Palau", "Asia/Dili", "Etc/GMT-9"}},
                        {"Tomsk Standard Time", new List<string>{"Asia/Tomsk"}},
                        {"Tonga Standard Time", new List<string>{"Pacific/Tongatapu"}},
                        {"Transbaikal Standard Time", new List<string>{"Asia/Chita"}},
                        {"Turkey Standard Time", new List<string>{"Europe/Istanbul"}},
                        {"Turks And Caicos Standard Time", new List<string>{"America/Grand_Turk"}},
                        {"US Eastern Standard Time", new List<string>{"America/Indianapolis", "America/Indiana/Marengo", "America/Indiana/Vevay"}},
                        {"US Mountain Standard Time", new List<string>{"America/Phoenix", "America/Dawson_Creek", "America/Creston", "America/Fort_Nelson", "America/Hermosillo", "Etc/GMT+7"}},
                        {"UTC", new List<string>{"Etc/GMT", "America/Danmarkshavn", "Etc/UTC"}},
                        {"UTC+12", new List<string>{"Etc/GMT-12", "Pacific/Tarawa", "Pacific/Majuro", "Pacific/Kwajalein", "Pacific/Nauru", "Pacific/Funafuti", "Pacific/Wake", "Pacific/Wallis"}},
                        {"UTC+13", new List<string>{"Etc/GMT-13", "Pacific/Enderbury", "Pacific/Fakaofo"}},
                        {"UTC-02", new List<string>{"Etc/GMT+2", "America/Noronha", "Atlantic/South_Georgia"}},
                        {"UTC-08", new List<string>{"Etc/GMT+8", "Pacific/Pitcairn"}},
                        {"UTC-09", new List<string>{"Etc/GMT+9", "Pacific/Gambier"}},
                        {"UTC-11", new List<string>{"Etc/GMT+11", "Pacific/Pago_Pago", "Pacific/Niue", "Pacific/Midway"}},
                        {"Ulaanbaatar Standard Time", new List<string>{"Asia/Ulaanbaatar", "Asia/Choibalsan"}},
                        {"Venezuela Standard Time", new List<string>{"America/Caracas"}},
                        {"Vladivostok Standard Time", new List<string>{"Asia/Vladivostok", "Asia/Ust-Nera"}},
                        {"Volgograd Standard Time", new List<string>{"Europe/Volgograd"}},
                        {"W. Australia Standard Time", new List<string>{"Australia/Perth"}},
                        {"W. Central Africa Standard Time", new List<string>{"Africa/Lagos", "Africa/Luanda", "Africa/Porto-Novo", "Africa/Kinshasa", "Africa/Bangui", "Africa/Brazzaville", "Africa/Douala", "Africa/Algiers", "Africa/Libreville", "Africa/Malabo", "Africa/Niamey", "Africa/Ndjamena", "Africa/Tunis", "Etc/GMT-1"}},
                        {"W. Europe Standard Time", new List<string>{"Europe/Berlin", "Europe/Andorra", "Europe/Vienna", "Europe/Zurich", "Europe/Busingen", "Europe/Gibraltar", "Europe/Rome", "Europe/Vaduz", "Europe/Luxembourg", "Europe/Monaco", "Europe/Malta", "Europe/Amsterdam", "Europe/Oslo", "Europe/Stockholm", "Arctic/Longyearbyen", "Europe/San_Marino", "Europe/Vatican"}},
                        {"W. Mongolia Standard Time", new List<string>{"Asia/Hovd"}},
                        {"West Asia Standard Time", new List<string>{"Asia/Tashkent", "Antarctica/Mawson", "Asia/Oral", "Asia/Aqtau", "Asia/Aqtobe", "Asia/Atyrau", "Indian/Maldives", "Indian/Kerguelen", "Asia/Dushanbe", "Asia/Ashgabat", "Asia/Samarkand", "Etc/GMT-5"}},
                        {"West Bank Standard Time", new List<string>{"Asia/Hebron", "Asia/Gaza"}},
                        {"West Pacific Standard Time", new List<string>{"Pacific/Port_Moresby", "Antarctica/DumontDUrville", "Pacific/Truk", "Pacific/Guam", "Pacific/Saipan", "Etc/GMT-10"}},
                        {"Yakutsk Standard Time", new List<string>{"Asia/Yakutsk", "Asia/Khandyga"}}
                    };
                }

                return _WindowsToOlsenDictionary;
            }
            set
            {
                _WindowsToOlsenDictionary = value;
            }
        }

        private static Dictionary<string, string> _OlsenToWindowsDictionary;
        public static Dictionary<string, string> OlsenToWindowsDictionary
        {
            get
            {
                if (_OlsenToWindowsDictionary == null)
                {
                    _OlsenToWindowsDictionary = new Dictionary<string, string>()
                    {
                        {"Australia/Darwin", "AUS Central Standard Time"},
                        {"Australia/Sydney", "AUS Eastern Standard Time"},
                        {"Australia/Melbourne", "AUS Eastern Standard Time"},
                        {"Asia/Kabul", "Afghanistan Standard Time"},
                        {"America/Anchorage", "Alaskan Standard Time"},
                        {"America/Juneau", "Alaskan Standard Time"},
                        {"America/Metlakatla", "Alaskan Standard Time"},
                        {"America/Nome", "Alaskan Standard Time"},
                        {"America/Sitka", "Alaskan Standard Time"},
                        {"America/Yakutat", "Alaskan Standard Time"},
                        {"America/Adak", "Aleutian Standard Time"},
                        {"Asia/Barnaul", "Altai Standard Time"},
                        {"Asia/Riyadh", "Arab Standard Time"},
                        {"Asia/Bahrain", "Arab Standard Time"},
                        {"Asia/Kuwait", "Arab Standard Time"},
                        {"Asia/Qatar", "Arab Standard Time"},
                        {"Asia/Aden", "Arab Standard Time"},
                        {"Asia/Dubai", "Arabian Standard Time"},
                        {"Asia/Muscat", "Arabian Standard Time"},
                        {"Etc/GMT-4", "Arabian Standard Time"},
                        {"Asia/Baghdad", "Arabic Standard Time"},
                        {"America/Buenos_Aires", "Argentina Standard Time"},
                        {"America/Argentina/La_Rioja", "Argentina Standard Time"},
                        {"America/Argentina/Rio_Gallegos", "Argentina Standard Time"},
                        {"America/Argentina/Salta", "Argentina Standard Time"},
                        {"America/Argentina/San_Juan", "Argentina Standard Time"},
                        {"America/Argentina/San_Luis", "Argentina Standard Time"},
                        {"America/Argentina/Tucuman", "Argentina Standard Time"},
                        {"America/Argentina/Ushuaia", "Argentina Standard Time"},
                        {"America/Catamarca", "Argentina Standard Time"},
                        {"America/Cordoba", "Argentina Standard Time"},
                        {"America/Jujuy", "Argentina Standard Time"},
                        {"America/Mendoza", "Argentina Standard Time"},
                        {"Europe/Astrakhan", "Astrakhan Standard Time"},
                        {"Europe/Ulyanovsk", "Astrakhan Standard Time"},
                        {"America/Halifax", "Atlantic Standard Time"},
                        {"Atlantic/Bermuda", "Atlantic Standard Time"},
                        {"America/Glace_Bay", "Atlantic Standard Time"},
                        {"America/Goose_Bay", "Atlantic Standard Time"},
                        {"America/Moncton", "Atlantic Standard Time"},
                        {"America/Thule", "Atlantic Standard Time"},
                        {"Australia/Eucla", "Aus Central W. Standard Time"},
                        {"Asia/Baku", "Azerbaijan Standard Time"},
                        {"Atlantic/Azores", "Azores Standard Time"},
                        {"America/Scoresbysund", "Azores Standard Time"},
                        {"America/Bahia", "Bahia Standard Time"},
                        {"Asia/Dhaka", "Bangladesh Standard Time"},
                        {"Asia/Thimphu", "Bangladesh Standard Time"},
                        {"Europe/Minsk", "Belarus Standard Time"},
                        {"Pacific/Bougainville", "Bougainville Standard Time"},
                        {"America/Regina", "Canada Central Standard Time"},
                        {"America/Swift_Current", "Canada Central Standard Time"},
                        {"Atlantic/Cape_Verde", "Cape Verde Standard Time"},
                        {"Etc/GMT+1", "Cape Verde Standard Time"},
                        {"Asia/Yerevan", "Caucasus Standard Time"},
                        {"Australia/Adelaide", "Cen. Australia Standard Time"},
                        {"Australia/Broken_Hill", "Cen. Australia Standard Time"},
                        {"America/Guatemala", "Central America Standard Time"},
                        {"America/Belize", "Central America Standard Time"},
                        {"America/Costa_Rica", "Central America Standard Time"},
                        {"Pacific/Galapagos", "Central America Standard Time"},
                        {"America/Tegucigalpa", "Central America Standard Time"},
                        {"America/Managua", "Central America Standard Time"},
                        {"America/El_Salvador", "Central America Standard Time"},
                        {"Etc/GMT+6", "Central America Standard Time"},
                        {"Asia/Almaty", "Central Asia Standard Time"},
                        {"Antarctica/Vostok", "Central Asia Standard Time"},
                        {"Asia/Urumqi", "Central Asia Standard Time"},
                        {"Indian/Chagos", "Central Asia Standard Time"},
                        {"Asia/Bishkek", "Central Asia Standard Time"},
                        {"Asia/Qostanay", "Central Asia Standard Time"},
                        {"Etc/GMT-6", "Central Asia Standard Time"},
                        {"America/Cuiaba", "Central Brazilian Standard Time"},
                        {"America/Campo_Grande", "Central Brazilian Standard Time"},
                        {"Europe/Budapest", "Central Europe Standard Time"},
                        {"Europe/Tirane", "Central Europe Standard Time"},
                        {"Europe/Prague", "Central Europe Standard Time"},
                        {"Europe/Podgorica", "Central Europe Standard Time"},
                        {"Europe/Belgrade", "Central Europe Standard Time"},
                        {"Europe/Ljubljana", "Central Europe Standard Time"},
                        {"Europe/Bratislava", "Central Europe Standard Time"},
                        {"Europe/Warsaw", "Central European Standard Time"},
                        {"Europe/Sarajevo", "Central European Standard Time"},
                        {"Europe/Zagreb", "Central European Standard Time"},
                        {"Europe/Skopje", "Central European Standard Time"},
                        {"Pacific/Guadalcanal", "Central Pacific Standard Time"},
                        {"Antarctica/Macquarie", "Central Pacific Standard Time"},
                        {"Pacific/Ponape", "Central Pacific Standard Time"},
                        {"Pacific/Kosrae", "Central Pacific Standard Time"},
                        {"Pacific/Noumea", "Central Pacific Standard Time"},
                        {"Pacific/Efate", "Central Pacific Standard Time"},
                        {"Etc/GMT-11", "Central Pacific Standard Time"},
                        {"America/Chicago", "Central Standard Time"},
                        {"America/Winnipeg", "Central Standard Time"},
                        {"America/Rainy_River", "Central Standard Time"},
                        {"America/Rankin_Inlet", "Central Standard Time"},
                        {"America/Resolute", "Central Standard Time"},
                        {"America/Matamoros", "Central Standard Time"},
                        {"America/Indiana/Knox", "Central Standard Time"},
                        {"America/Indiana/Tell_City", "Central Standard Time"},
                        {"America/Menominee", "Central Standard Time"},
                        {"America/North_Dakota/Beulah", "Central Standard Time"},
                        {"America/North_Dakota/Center", "Central Standard Time"},
                        {"America/North_Dakota/New_Salem", "Central Standard Time"},
                        {"CST6CDT", "Central Standard Time"},
                        {"America/Mexico_City", "Central Standard Time (Mexico)"},
                        {"America/Bahia_Banderas", "Central Standard Time (Mexico)"},
                        {"America/Merida", "Central Standard Time (Mexico)"},
                        {"America/Monterrey", "Central Standard Time (Mexico)"},
                        {"Pacific/Chatham", "Chatham Islands Standard Time"},
                        {"Asia/Shanghai", "China Standard Time"},
                        {"Asia/Hong_Kong", "China Standard Time"},
                        {"Asia/Macau", "China Standard Time"},
                        {"America/Havana", "Cuba Standard Time"},
                        {"Etc/GMT+12", "Dateline Standard Time"},
                        {"Africa/Nairobi", "E. Africa Standard Time"},
                        {"Antarctica/Syowa", "E. Africa Standard Time"},
                        {"Africa/Djibouti", "E. Africa Standard Time"},
                        {"Africa/Asmera", "E. Africa Standard Time"},
                        {"Africa/Addis_Ababa", "E. Africa Standard Time"},
                        {"Indian/Comoro", "E. Africa Standard Time"},
                        {"Indian/Antananarivo", "E. Africa Standard Time"},
                        {"Africa/Mogadishu", "E. Africa Standard Time"},
                        {"Africa/Juba", "E. Africa Standard Time"},
                        {"Africa/Dar_es_Salaam", "E. Africa Standard Time"},
                        {"Africa/Kampala", "E. Africa Standard Time"},
                        {"Indian/Mayotte", "E. Africa Standard Time"},
                        {"Etc/GMT-3", "E. Africa Standard Time"},
                        {"Australia/Brisbane", "E. Australia Standard Time"},
                        {"Australia/Lindeman", "E. Australia Standard Time"},
                        {"Europe/Chisinau", "E. Europe Standard Time"},
                        {"America/Sao_Paulo", "E. South America Standard Time"},
                        {"Pacific/Easter", "Easter Island Standard Time"},
                        {"America/New_York", "Eastern Standard Time"},
                        {"America/Nassau", "Eastern Standard Time"},
                        {"America/Toronto", "Eastern Standard Time"},
                        {"America/Iqaluit", "Eastern Standard Time"},
                        {"America/Montreal", "Eastern Standard Time"},
                        {"America/Nipigon", "Eastern Standard Time"},
                        {"America/Pangnirtung", "Eastern Standard Time"},
                        {"America/Thunder_Bay", "Eastern Standard Time"},
                        {"America/Detroit", "Eastern Standard Time"},
                        {"America/Indiana/Petersburg", "Eastern Standard Time"},
                        {"America/Indiana/Vincennes", "Eastern Standard Time"},
                        {"America/Indiana/Winamac", "Eastern Standard Time"},
                        {"America/Kentucky/Monticello", "Eastern Standard Time"},
                        {"America/Louisville", "Eastern Standard Time"},
                        {"EST5EDT", "Eastern Standard Time"},
                        {"America/Cancun", "Eastern Standard Time (Mexico)"},
                        {"Africa/Cairo", "Egypt Standard Time"},
                        {"Asia/Yekaterinburg", "Ekaterinburg Standard Time"},
                        {"Europe/Kiev", "FLE Standard Time"},
                        {"Europe/Mariehamn", "FLE Standard Time"},
                        {"Europe/Sofia", "FLE Standard Time"},
                        {"Europe/Tallinn", "FLE Standard Time"},
                        {"Europe/Helsinki", "FLE Standard Time"},
                        {"Europe/Vilnius", "FLE Standard Time"},
                        {"Europe/Riga", "FLE Standard Time"},
                        {"Europe/Uzhgorod", "FLE Standard Time"},
                        {"Europe/Zaporozhye", "FLE Standard Time"},
                        {"Pacific/Fiji", "Fiji Standard Time"},
                        {"Europe/London", "GMT Standard Time"},
                        {"Atlantic/Canary", "GMT Standard Time"},
                        {"Atlantic/Faeroe", "GMT Standard Time"},
                        {"Europe/Guernsey", "GMT Standard Time"},
                        {"Europe/Dublin", "GMT Standard Time"},
                        {"Europe/Isle_of_Man", "GMT Standard Time"},
                        {"Europe/Jersey", "GMT Standard Time"},
                        {"Europe/Lisbon", "GMT Standard Time"},
                        {"Atlantic/Madeira", "GMT Standard Time"},
                        {"Europe/Bucharest", "GTB Standard Time"},
                        {"Asia/Nicosia", "GTB Standard Time"},
                        {"Asia/Famagusta", "GTB Standard Time"},
                        {"Europe/Athens", "GTB Standard Time"},
                        {"Asia/Tbilisi", "Georgian Standard Time"},
                        {"America/Godthab", "Greenland Standard Time"},
                        {"Atlantic/Reykjavik", "Greenwich Standard Time"},
                        {"Africa/Ouagadougou", "Greenwich Standard Time"},
                        {"Africa/Abidjan", "Greenwich Standard Time"},
                        {"Africa/Accra", "Greenwich Standard Time"},
                        {"Africa/Banjul", "Greenwich Standard Time"},
                        {"Africa/Conakry", "Greenwich Standard Time"},
                        {"Africa/Bissau", "Greenwich Standard Time"},
                        {"Africa/Monrovia", "Greenwich Standard Time"},
                        {"Africa/Bamako", "Greenwich Standard Time"},
                        {"Africa/Nouakchott", "Greenwich Standard Time"},
                        {"Atlantic/St_Helena", "Greenwich Standard Time"},
                        {"Africa/Freetown", "Greenwich Standard Time"},
                        {"Africa/Dakar", "Greenwich Standard Time"},
                        {"Africa/Lome", "Greenwich Standard Time"},
                        {"America/Port-au-Prince", "Haiti Standard Time"},
                        {"Pacific/Honolulu", "Hawaiian Standard Time"},
                        {"Pacific/Rarotonga", "Hawaiian Standard Time"},
                        {"Pacific/Tahiti", "Hawaiian Standard Time"},
                        {"Pacific/Johnston", "Hawaiian Standard Time"},
                        {"Etc/GMT+10", "Hawaiian Standard Time"},
                        {"Asia/Calcutta", "India Standard Time"},
                        {"Asia/Tehran", "Iran Standard Time"},
                        {"Asia/Jerusalem", "Israel Standard Time"},
                        {"Asia/Amman", "Jordan Standard Time"},
                        {"Europe/Kaliningrad", "Kaliningrad Standard Time"},
                        {"Asia/Seoul", "Korea Standard Time"},
                        {"Africa/Tripoli", "Libya Standard Time"},
                        {"Pacific/Kiritimati", "Line Islands Standard Time"},
                        {"Etc/GMT-14", "Line Islands Standard Time"},
                        {"Australia/Lord_Howe", "Lord Howe Standard Time"},
                        {"Asia/Magadan", "Magadan Standard Time"},
                        {"America/Punta_Arenas", "Magallanes Standard Time"},
                        {"Pacific/Marquesas", "Marquesas Standard Time"},
                        {"Indian/Mauritius", "Mauritius Standard Time"},
                        {"Indian/Reunion", "Mauritius Standard Time"},
                        {"Indian/Mahe", "Mauritius Standard Time"},
                        {"Asia/Beirut", "Middle East Standard Time"},
                        {"America/Montevideo", "Montevideo Standard Time"},
                        {"Africa/Casablanca", "Morocco Standard Time"},
                        {"Africa/El_Aaiun", "Morocco Standard Time"},
                        {"America/Denver", "Mountain Standard Time"},
                        {"America/Edmonton", "Mountain Standard Time"},
                        {"America/Cambridge_Bay", "Mountain Standard Time"},
                        {"America/Inuvik", "Mountain Standard Time"},
                        {"America/Yellowknife", "Mountain Standard Time"},
                        {"America/Ojinaga", "Mountain Standard Time"},
                        {"America/Boise", "Mountain Standard Time"},
                        {"MST7MDT", "Mountain Standard Time"},
                        {"America/Chihuahua", "Mountain Standard Time (Mexico)"},
                        {"America/Mazatlan", "Mountain Standard Time (Mexico)"},
                        {"Asia/Rangoon", "Myanmar Standard Time"},
                        {"Indian/Cocos", "Myanmar Standard Time"},
                        {"Asia/Novosibirsk", "N. Central Asia Standard Time"},
                        {"Africa/Windhoek", "Namibia Standard Time"},
                        {"Asia/Katmandu", "Nepal Standard Time"},
                        {"Pacific/Auckland", "New Zealand Standard Time"},
                        {"Antarctica/McMurdo", "New Zealand Standard Time"},
                        {"America/St_Johns", "Newfoundland Standard Time"},
                        {"Pacific/Norfolk", "Norfolk Standard Time"},
                        {"Asia/Irkutsk", "North Asia East Standard Time"},
                        {"Asia/Krasnoyarsk", "North Asia Standard Time"},
                        {"Asia/Novokuznetsk", "North Asia Standard Time"},
                        {"Asia/Pyongyang", "North Korea Standard Time"},
                        {"Asia/Omsk", "Omsk Standard Time"},
                        {"America/Santiago", "Pacific SA Standard Time"},
                        {"America/Los_Angeles", "Pacific Standard Time"},
                        {"America/Vancouver", "Pacific Standard Time"},
                        {"America/Dawson", "Pacific Standard Time"},
                        {"America/Whitehorse", "Pacific Standard Time"},
                        {"PST8PDT", "Pacific Standard Time"},
                        {"America/Tijuana", "Pacific Standard Time (Mexico)"},
                        {"America/Santa_Isabel", "Pacific Standard Time (Mexico)"},
                        {"Asia/Karachi", "Pakistan Standard Time"},
                        {"America/Asuncion", "Paraguay Standard Time"},
                        {"Asia/Qyzylorda", "Qyzylorda Standard Time"},
                        {"Europe/Paris", "Romance Standard Time"},
                        {"Europe/Brussels", "Romance Standard Time"},
                        {"Europe/Copenhagen", "Romance Standard Time"},
                        {"Europe/Madrid", "Romance Standard Time"},
                        {"Africa/Ceuta", "Romance Standard Time"},
                        {"Asia/Srednekolymsk", "Russia Time Zone 10"},
                        {"Asia/Kamchatka", "Russia Time Zone 11"},
                        {"Asia/Anadyr", "Russia Time Zone 11"},
                        {"Europe/Samara", "Russia Time Zone 3"},
                        {"Europe/Moscow", "Russian Standard Time"},
                        {"Europe/Kirov", "Russian Standard Time"},
                        {"Europe/Simferopol", "Russian Standard Time"},
                        {"America/Cayenne", "SA Eastern Standard Time"},
                        {"Antarctica/Rothera", "SA Eastern Standard Time"},
                        {"Antarctica/Palmer", "SA Eastern Standard Time"},
                        {"America/Fortaleza", "SA Eastern Standard Time"},
                        {"America/Belem", "SA Eastern Standard Time"},
                        {"America/Maceio", "SA Eastern Standard Time"},
                        {"America/Recife", "SA Eastern Standard Time"},
                        {"America/Santarem", "SA Eastern Standard Time"},
                        {"Atlantic/Stanley", "SA Eastern Standard Time"},
                        {"America/Paramaribo", "SA Eastern Standard Time"},
                        {"Etc/GMT+3", "SA Eastern Standard Time"},
                        {"America/Bogota", "SA Pacific Standard Time"},
                        {"America/Rio_Branco", "SA Pacific Standard Time"},
                        {"America/Eirunepe", "SA Pacific Standard Time"},
                        {"America/Coral_Harbour", "SA Pacific Standard Time"},
                        {"America/Guayaquil", "SA Pacific Standard Time"},
                        {"America/Jamaica", "SA Pacific Standard Time"},
                        {"America/Cayman", "SA Pacific Standard Time"},
                        {"America/Panama", "SA Pacific Standard Time"},
                        {"America/Lima", "SA Pacific Standard Time"},
                        {"Etc/GMT+5", "SA Pacific Standard Time"},
                        {"America/La_Paz", "SA Western Standard Time"},
                        {"America/Antigua", "SA Western Standard Time"},
                        {"America/Anguilla", "SA Western Standard Time"},
                        {"America/Aruba", "SA Western Standard Time"},
                        {"America/Barbados", "SA Western Standard Time"},
                        {"America/St_Barthelemy", "SA Western Standard Time"},
                        {"America/Kralendijk", "SA Western Standard Time"},
                        {"America/Manaus", "SA Western Standard Time"},
                        {"America/Boa_Vista", "SA Western Standard Time"},
                        {"America/Porto_Velho", "SA Western Standard Time"},
                        {"America/Blanc-Sablon", "SA Western Standard Time"},
                        {"America/Curacao", "SA Western Standard Time"},
                        {"America/Dominica", "SA Western Standard Time"},
                        {"America/Santo_Domingo", "SA Western Standard Time"},
                        {"America/Grenada", "SA Western Standard Time"},
                        {"America/Guadeloupe", "SA Western Standard Time"},
                        {"America/Guyana", "SA Western Standard Time"},
                        {"America/St_Kitts", "SA Western Standard Time"},
                        {"America/St_Lucia", "SA Western Standard Time"},
                        {"America/Marigot", "SA Western Standard Time"},
                        {"America/Martinique", "SA Western Standard Time"},
                        {"America/Montserrat", "SA Western Standard Time"},
                        {"America/Puerto_Rico", "SA Western Standard Time"},
                        {"America/Lower_Princes", "SA Western Standard Time"},
                        {"America/Port_of_Spain", "SA Western Standard Time"},
                        {"America/St_Vincent", "SA Western Standard Time"},
                        {"America/Tortola", "SA Western Standard Time"},
                        {"America/St_Thomas", "SA Western Standard Time"},
                        {"Etc/GMT+4", "SA Western Standard Time"},
                        {"Asia/Bangkok", "SE Asia Standard Time"},
                        {"Antarctica/Davis", "SE Asia Standard Time"},
                        {"Indian/Christmas", "SE Asia Standard Time"},
                        {"Asia/Jakarta", "SE Asia Standard Time"},
                        {"Asia/Pontianak", "SE Asia Standard Time"},
                        {"Asia/Phnom_Penh", "SE Asia Standard Time"},
                        {"Asia/Vientiane", "SE Asia Standard Time"},
                        {"Asia/Saigon", "SE Asia Standard Time"},
                        {"Etc/GMT-7", "SE Asia Standard Time"},
                        {"America/Miquelon", "Saint Pierre Standard Time"},
                        {"Asia/Sakhalin", "Sakhalin Standard Time"},
                        {"Pacific/Apia", "Samoa Standard Time"},
                        {"Africa/Sao_Tome", "Sao Tome Standard Time"},
                        {"Europe/Saratov", "Saratov Standard Time"},
                        {"Asia/Singapore", "Singapore Standard Time"},
                        {"Antarctica/Casey", "Singapore Standard Time"},
                        {"Asia/Brunei", "Singapore Standard Time"},
                        {"Asia/Makassar", "Singapore Standard Time"},
                        {"Asia/Kuala_Lumpur", "Singapore Standard Time"},
                        {"Asia/Kuching", "Singapore Standard Time"},
                        {"Asia/Manila", "Singapore Standard Time"},
                        {"Etc/GMT-8", "Singapore Standard Time"},
                        {"Africa/Johannesburg", "South Africa Standard Time"},
                        {"Africa/Bujumbura", "South Africa Standard Time"},
                        {"Africa/Gaborone", "South Africa Standard Time"},
                        {"Africa/Lubumbashi", "South Africa Standard Time"},
                        {"Africa/Maseru", "South Africa Standard Time"},
                        {"Africa/Blantyre", "South Africa Standard Time"},
                        {"Africa/Maputo", "South Africa Standard Time"},
                        {"Africa/Kigali", "South Africa Standard Time"},
                        {"Africa/Mbabane", "South Africa Standard Time"},
                        {"Africa/Lusaka", "South Africa Standard Time"},
                        {"Africa/Harare", "South Africa Standard Time"},
                        {"Etc/GMT-2", "South Africa Standard Time"},
                        {"Asia/Colombo", "Sri Lanka Standard Time"},
                        {"Africa/Khartoum", "Sudan Standard Time"},
                        {"Asia/Damascus", "Syria Standard Time"},
                        {"Asia/Taipei", "Taipei Standard Time"},
                        {"Australia/Hobart", "Tasmania Standard Time"},
                        {"Australia/Currie", "Tasmania Standard Time"},
                        {"America/Araguaina", "Tocantins Standard Time"},
                        {"Asia/Tokyo", "Tokyo Standard Time"},
                        {"Asia/Jayapura", "Tokyo Standard Time"},
                        {"Pacific/Palau", "Tokyo Standard Time"},
                        {"Asia/Dili", "Tokyo Standard Time"},
                        {"Etc/GMT-9", "Tokyo Standard Time"},
                        {"Asia/Tomsk", "Tomsk Standard Time"},
                        {"Pacific/Tongatapu", "Tonga Standard Time"},
                        {"Asia/Chita", "Transbaikal Standard Time"},
                        {"Europe/Istanbul", "Turkey Standard Time"},
                        {"America/Grand_Turk", "Turks And Caicos Standard Time"},
                        {"America/Indianapolis", "US Eastern Standard Time"},
                        {"America/Indiana/Marengo", "US Eastern Standard Time"},
                        {"America/Indiana/Vevay", "US Eastern Standard Time"},
                        {"America/Phoenix", "US Mountain Standard Time"},
                        {"America/Dawson_Creek", "US Mountain Standard Time"},
                        {"America/Creston", "US Mountain Standard Time"},
                        {"America/Fort_Nelson", "US Mountain Standard Time"},
                        {"America/Hermosillo", "US Mountain Standard Time"},
                        {"Etc/GMT+7", "US Mountain Standard Time"},
                        {"Etc/GMT", "UTC"},
                        {"America/Danmarkshavn", "UTC"},
                        {"Etc/UTC", "UTC"},
                        {"Etc/GMT-12", "UTC+12"},
                        {"Pacific/Tarawa", "UTC+12"},
                        {"Pacific/Majuro", "UTC+12"},
                        {"Pacific/Kwajalein", "UTC+12"},
                        {"Pacific/Nauru", "UTC+12"},
                        {"Pacific/Funafuti", "UTC+12"},
                        {"Pacific/Wake", "UTC+12"},
                        {"Pacific/Wallis", "UTC+12"},
                        {"Etc/GMT-13", "UTC+13"},
                        {"Pacific/Enderbury", "UTC+13"},
                        {"Pacific/Fakaofo", "UTC+13"},
                        {"Etc/GMT+2", "UTC-02"},
                        {"America/Noronha", "UTC-02"},
                        {"Atlantic/South_Georgia", "UTC-02"},
                        {"Etc/GMT+8", "UTC-08"},
                        {"Pacific/Pitcairn", "UTC-08"},
                        {"Etc/GMT+9", "UTC-09"},
                        {"Pacific/Gambier", "UTC-09"},
                        {"Etc/GMT+11", "UTC-11"},
                        {"Pacific/Pago_Pago", "UTC-11"},
                        {"Pacific/Niue", "UTC-11"},
                        {"Pacific/Midway", "UTC-11"},
                        {"Asia/Ulaanbaatar", "Ulaanbaatar Standard Time"},
                        {"Asia/Choibalsan", "Ulaanbaatar Standard Time"},
                        {"America/Caracas", "Venezuela Standard Time"},
                        {"Asia/Vladivostok", "Vladivostok Standard Time"},
                        {"Asia/Ust-Nera", "Vladivostok Standard Time"},
                        {"Europe/Volgograd", "Volgograd Standard Time"},
                        {"Australia/Perth", "W. Australia Standard Time"},
                        {"Africa/Lagos", "W. Central Africa Standard Time"},
                        {"Africa/Luanda", "W. Central Africa Standard Time"},
                        {"Africa/Porto-Novo", "W. Central Africa Standard Time"},
                        {"Africa/Kinshasa", "W. Central Africa Standard Time"},
                        {"Africa/Bangui", "W. Central Africa Standard Time"},
                        {"Africa/Brazzaville", "W. Central Africa Standard Time"},
                        {"Africa/Douala", "W. Central Africa Standard Time"},
                        {"Africa/Algiers", "W. Central Africa Standard Time"},
                        {"Africa/Libreville", "W. Central Africa Standard Time"},
                        {"Africa/Malabo", "W. Central Africa Standard Time"},
                        {"Africa/Niamey", "W. Central Africa Standard Time"},
                        {"Africa/Ndjamena", "W. Central Africa Standard Time"},
                        {"Africa/Tunis", "W. Central Africa Standard Time"},
                        {"Etc/GMT-1", "W. Central Africa Standard Time"},
                        {"Europe/Berlin", "W. Europe Standard Time"},
                        {"Europe/Andorra", "W. Europe Standard Time"},
                        {"Europe/Vienna", "W. Europe Standard Time"},
                        {"Europe/Zurich", "W. Europe Standard Time"},
                        {"Europe/Busingen", "W. Europe Standard Time"},
                        {"Europe/Gibraltar", "W. Europe Standard Time"},
                        {"Europe/Rome", "W. Europe Standard Time"},
                        {"Europe/Vaduz", "W. Europe Standard Time"},
                        {"Europe/Luxembourg", "W. Europe Standard Time"},
                        {"Europe/Monaco", "W. Europe Standard Time"},
                        {"Europe/Malta", "W. Europe Standard Time"},
                        {"Europe/Amsterdam", "W. Europe Standard Time"},
                        {"Europe/Oslo", "W. Europe Standard Time"},
                        {"Europe/Stockholm", "W. Europe Standard Time"},
                        {"Arctic/Longyearbyen", "W. Europe Standard Time"},
                        {"Europe/San_Marino", "W. Europe Standard Time"},
                        {"Europe/Vatican", "W. Europe Standard Time"},
                        {"Asia/Hovd", "W. Mongolia Standard Time"},
                        {"Asia/Tashkent", "West Asia Standard Time"},
                        {"Antarctica/Mawson", "West Asia Standard Time"},
                        {"Asia/Oral", "West Asia Standard Time"},
                        {"Asia/Aqtau", "West Asia Standard Time"},
                        {"Asia/Aqtobe", "West Asia Standard Time"},
                        {"Asia/Atyrau", "West Asia Standard Time"},
                        {"Indian/Maldives", "West Asia Standard Time"},
                        {"Indian/Kerguelen", "West Asia Standard Time"},
                        {"Asia/Dushanbe", "West Asia Standard Time"},
                        {"Asia/Ashgabat", "West Asia Standard Time"},
                        {"Asia/Samarkand", "West Asia Standard Time"},
                        {"Etc/GMT-5", "West Asia Standard Time"},
                        {"Asia/Hebron", "West Bank Standard Time"},
                        {"Asia/Gaza", "West Bank Standard Time"},
                        {"Pacific/Port_Moresby", "West Pacific Standard Time"},
                        {"Antarctica/DumontDUrville", "West Pacific Standard Time"},
                        {"Pacific/Truk", "West Pacific Standard Time"},
                        {"Pacific/Guam", "West Pacific Standard Time"},
                        {"Pacific/Saipan", "West Pacific Standard Time"},
                        {"Etc/GMT-10", "West Pacific Standard Time"},
                        {"Asia/Yakutsk", "Yakutsk Standard Time"},
                        {"Asia/Khandyga", "Yakutsk Standard Time"}
                    };
                }

                return _OlsenToWindowsDictionary;
            }
            set
            {
                _OlsenToWindowsDictionary = value;
            }
        }

        private static Dictionary<string, string> _StandardToWindowsDictionary = null;
        public static Dictionary<string, string> StandardToWindowsDictionary
        {
            get
            {
                if (_StandardToWindowsDictionary == null)
                {
                    _StandardToWindowsDictionary = new Dictionary<string, string>()
                    {
                        {"AKDT", "Alaskan Standard Time"},
                        {"GST", "Arabian Standard Time"},
                        {"ADT", "Atlantic Standard Time"},
                        {"BDT", "Bangladesh Standard Time"},
                        {"CDT", "Central Standard Time"},
                        {"HKT", "China Standard Time"},
                        {"EAT", "E. Africa Standard Time"},
                        {"BRST", "E. South America Standard Time"},
                        {"EDT", "Eastern Standard Time"},
                        {"BST", "GMT Standard Time"},
                        {"WEST", "GMT Standard Time"},
                        {"HST", "Hawaiian Standard Time"},
                        {"IST", "India Standard Time"},
                        {"IRST", "Iran Standard Time"},
                        {"KST", "Korea Standard Time"},
                        {"MDT", "Mountain Standard Time"},
                        {"NZDT", "New Zealand Standard Time"},
                        {"CLST", "Pacific SA Standard Time"},
                        {"PDT", "Pacific Standard Time"},
                        {"PKT", "Pakistan Standard Time"},
                        {"CEST", "Romance Standard Time"},
                        {"MSD", "Russian Standard Time"},
                        {"COT", "SA Pacific Standard Time"},
                        {"PET", "SA Pacific Standard Time"},
                        {"ICT", "SE Asia Standard Time"},
                        {"WIT", "SE Asia Standard Time"},
                        {"SGT", "Singapore Standard Time"},
                        {"PHT", "Singapore Standard Time"},
                        {"CAT", "South Africa Standard Time"},
                        {"JST", "Tokyo Standard Time"},
                        {"EEST", "Turkey Standard Time"},
                        {"WAT", "W. Central Africa Standard Time"}
                    };
                }

                return _StandardToWindowsDictionary;
            }
            set
            {
                _StandardToWindowsDictionary = value;
            }
        }

        private static Dictionary<string, string> _WindowsToStandardDictionary = null;
        public static Dictionary<string, string> WindowsToStandardDictionary
        {
            get
            {
                if (_WindowsToStandardDictionary == null)
                {
                    _WindowsToStandardDictionary = new Dictionary<string, string>()
                    {
                        {"Alaskan Standard Time", "AKDT"},
                        {"Arabian Standard Time", "GST"},
                        {"Atlantic Standard Time", "ADT"},
                        {"Bangladesh Standard Time", "BDT"},
                        {"Central Standard Time", "CDT"},
                        {"China Standard Time", "HKT"},
                        {"E. Africa Standard Time", "EAT"},
                        {"E. South America Standard Time", "BRST"},
                        {"Eastern Standard Time", "EDT"},
                        {"GMT Standard Time", "BST"},
                        {"GMT Standard Time", "WEST"},
                        {"Hawaiian Standard Time", "HST"},
                        {"India Standard Time", "IST"},
                        {"Iran Standard Time", "IRST"},
                        {"Korea Standard Time", "KST"},
                        {"Mountain Standard Time", "MDT"},
                        {"New Zealand Standard Time", "NZDT"},
                        {"Pacific SA Standard Time", "CLST"},
                        {"Pacific Standard Time", "PDT"},
                        {"Pakistan Standard Time", "PKT"},
                        {"Romance Standard Time", "CEST"},
                        {"Russian Standard Time", "MSD"},
                        {"SA Pacific Standard Time", "COT"},
                        {"SA Pacific Standard Time", "PET"},
                        {"SE Asia Standard Time", "ICT"},
                        {"SE Asia Standard Time", "WIT"},
                        {"Singapore Standard Time", "SGT"},
                        {"Singapore Standard Time", "PHT"},
                        {"South Africa Standard Time", "CAT"},
                        {"Tokyo Standard Time", "JST"},
                        {"Turkey Standard Time", "EEST"},
                        {"W. Central Africa Standard Time", "WAT"}
                    };
                }

                return _WindowsToStandardDictionary;
            }
            set
            {
                _WindowsToStandardDictionary = value;
            }
        }

        private static Dictionary<string, string> _OlsenToStandardDictionary = null;
        public static Dictionary<string, string> OlsenToStandardDictionary
        {
            get
            {
                if (_OlsenToStandardDictionary == null)
                {
                    _OlsenToStandardDictionary = new Dictionary<string, string>()
                    {
                        {"Africa/Abidjan", "GMT"},
                        {"Africa/Accra", "GMT"},
                        {"Africa/Addis_Ababa", "EAT"},
                        {"Africa/Algiers", "CET"},
                        {"Africa/Asmara", "EAT"},
                        {"Africa/Asmera", "EAT"},
                        {"Africa/Bamako", "GMT"},
                        {"Africa/Bangui", "WAT"},
                        {"Africa/Banjul", "GMT"},
                        {"Africa/Bissau", "GMT"},
                        {"Africa/Blantyre", "CAT"},
                        {"Africa/Brazzaville", "WAT"},
                        {"Africa/Bujumbura", "CAT"},
                        {"Africa/Cairo", "EET"},
                        {"Africa/Casablanca", "WET"},
                        {"Africa/Ceuta", "CET"},
                        {"Africa/Conakry", "GMT"},
                        {"Africa/Dakar", "GMT"},
                        {"Africa/Dar_es_Salaam", "EAT"},
                        {"Africa/Djibouti", "EAT"},
                        {"Africa/Douala", "WAT"},
                        {"Africa/El_Aaiun", "WET"},
                        {"Africa/Freetown", "GMT"},
                        {"Africa/Gaborone", "CAT"},
                        {"Africa/Harare", "CAT"},
                        {"Africa/Johannesburg", "SAST"},
                        {"Africa/Juba", "EAT"},
                        {"Africa/Kampala", "EAT"},
                        {"Africa/Khartoum", "CAT"},
                        {"Africa/Kigali", "CAT"},
                        {"Africa/Kinshasa", "WAT"},
                        {"Africa/Lagos", "WAT"},
                        {"Africa/Libreville", "WAT"},
                        {"Africa/Lome", "GMT"},
                        {"Africa/Luanda", "WAT"},
                        {"Africa/Lubumbashi", "CAT"},
                        {"Africa/Lusaka", "CAT"},
                        {"Africa/Malabo", "WAT"},
                        {"Africa/Maputo", "CAT"},
                        {"Africa/Maseru", "SAST"},
                        {"Africa/Mbabane", "SAST"},
                        {"Africa/Mogadishu", "EAT"},
                        {"Africa/Monrovia", "GMT"},
                        {"Africa/Nairobi", "EAT"},
                        {"Africa/Ndjamena", "WAT"},
                        {"Africa/Niamey", "WAT"},
                        {"Africa/Nouakchott", "GMT"},
                        {"Africa/Ouagadougou", "GMT"},
                        {"Africa/Porto-Novo", "WAT"},
                        {"Africa/Sao_Tome", "WAT"},
                        {"Africa/Timbuktu", "GMT"},
                        {"Africa/Tripoli", "EET"},
                        {"Africa/Tunis", "CET"},
                        {"Africa/Windhoek", "CAT"},
                        {"America/Adak", "HST"},
                        {"America/Anchorage", "AKST"},
                        {"America/Anguilla", "AST"},
                        {"America/Antigua", "AST"},
                        {"America/Araguaina", "-03"},
                        {"America/Argentina/Buenos_Aires", "-03"},
                        {"America/Argentina/Catamarca", "-03"},
                        {"America/Argentina/ComodRivadavia", "-03"},
                        {"America/Argentina/Cordoba", "-03"},
                        {"America/Argentina/Jujuy", "-03"},
                        {"America/Argentina/La_Rioja", "-03"},
                        {"America/Argentina/Mendoza", "-03"},
                        {"America/Argentina/Rio_Gallegos", "-03"},
                        {"America/Argentina/Salta", "-03"},
                        {"America/Argentina/San_Juan", "-03"},
                        {"America/Argentina/San_Luis", "-03"},
                        {"America/Argentina/Tucuman", "-03"},
                        {"America/Argentina/Ushuaia", "-03"},
                        {"America/Aruba", "AST"},
                        {"America/Asuncion", "-04"},
                        {"America/Atikokan", "EST"},
                        {"America/Atka", "HST"},
                        {"America/Bahia", "-03"},
                        {"America/Bahia_Banderas", "CST"},
                        {"America/Barbados", "AST"},
                        {"America/Belem", "-03"},
                        {"America/Belize", "CST"},
                        {"America/Blanc-Sablon", "AST"},
                        {"America/Boa_Vista", "-04"},
                        {"America/Bogota", "-05"},
                        {"America/Boise", "MST"},
                        {"America/Buenos_Aires", "-03"},
                        {"America/Cambridge_Bay", "MST"},
                        {"America/Campo_Grande", "-04"},
                        {"America/Cancun", "EST"},
                        {"America/Caracas", "-04"},
                        {"America/Catamarca", "-03"},
                        {"America/Cayenne", "-03"},
                        {"America/Cayman", "EST"},
                        {"America/Chicago", "CST"},
                        {"America/Chihuahua", "MST"},
                        {"America/Coral_Harbour", "EST"},
                        {"America/Cordoba", "-03"},
                        {"America/Costa_Rica", "CST"},
                        {"America/Creston", "MST"},
                        {"America/Cuiaba", "-04"},
                        {"America/Curacao", "AST"},
                        {"America/Danmarkshavn", "GMT"},
                        {"America/Dawson", "PST"},
                        {"America/Dawson_Creek", "MST"},
                        {"America/Denver", "MST"},
                        {"America/Detroit", "EST"},
                        {"America/Dominica", "AST"},
                        {"America/Edmonton", "MST"},
                        {"America/Eirunepe", "-05"},
                        {"America/El_Salvador", "CST"},
                        {"America/Ensenada", "PST"},
                        {"America/Fort_Nelson", "MST"},
                        {"America/Fort_Wayne", "EST"},
                        {"America/Fortaleza", "-03"},
                        {"America/Glace_Bay", "AST"},
                        {"America/Godthab", "-03"},
                        {"America/Goose_Bay", "AST"},
                        {"America/Grand_Turk", "EST"},
                        {"America/Grenada", "AST"},
                        {"America/Guadeloupe", "AST"},
                        {"America/Guatemala", "CST"},
                        {"America/Guayaquil", "-05"},
                        {"America/Guyana", "-04"},
                        {"America/Halifax", "AST"},
                        {"America/Havana", "CST"},
                        {"America/Hermosillo", "MST"},
                        {"America/Indiana/Indianapolis", "EST"},
                        {"America/Indiana/Knox", "CST"},
                        {"America/Indiana/Marengo", "EST"},
                        {"America/Indiana/Petersburg", "EST"},
                        {"America/Indiana/Tell_City", "CST"},
                        {"America/Indiana/Vevay", "EST"},
                        {"America/Indiana/Vincennes", "EST"},
                        {"America/Indiana/Winamac", "EST"},
                        {"America/Indianapolis", "EST"},
                        {"America/Inuvik", "MST"},
                        {"America/Iqaluit", "EST"},
                        {"America/Jamaica", "EST"},
                        {"America/Jujuy", "-03"},
                        {"America/Juneau", "AKST"},
                        {"America/Kentucky/Louisville", "EST"},
                        {"America/Kentucky/Monticello", "EST"},
                        {"America/Knox_IN", "CST"},
                        {"America/Kralendijk", "AST"},
                        {"America/La_Paz", "-04"},
                        {"America/Lima", "-05"},
                        {"America/Los_Angeles", "PST"},
                        {"America/Louisville", "EST"},
                        {"America/Lower_Princes", "AST"},
                        {"America/Maceio", "-03"},
                        {"America/Managua", "CST"},
                        {"America/Manaus", "-04"},
                        {"America/Marigot", "AST"},
                        {"America/Martinique", "AST"},
                        {"America/Matamoros", "CST"},
                        {"America/Mazatlan", "MST"},
                        {"America/Mendoza", "-03"},
                        {"America/Menominee", "CST"},
                        {"America/Merida", "CST"},
                        {"America/Metlakatla", "AKST"},
                        {"America/Mexico_City", "CST"},
                        {"America/Miquelon", "-03"},
                        {"America/Moncton", "AST"},
                        {"America/Monterrey", "CST"},
                        {"America/Montevideo", "-03"},
                        {"America/Montreal", "EST"},
                        {"America/Montserrat", "AST"},
                        {"America/Nassau", "EST"},
                        {"America/New_York", "EST"},
                        {"America/Nipigon", "EST"},
                        {"America/Nome", "AKST"},
                        {"America/Noronha", "-02"},
                        {"America/North_Dakota/Beulah", "CST"},
                        {"America/North_Dakota/Center", "CST"},
                        {"America/North_Dakota/New_Salem", "CST"},
                        {"America/Ojinaga", "MST"},
                        {"America/Panama", "EST"},
                        {"America/Pangnirtung", "EST"},
                        {"America/Paramaribo", "-03"},
                        {"America/Phoenix", "MST"},
                        {"America/Port-au-Prince", "EST"},
                        {"America/Port_of_Spain", "AST"},
                        {"America/Porto_Acre", "-05"},
                        {"America/Porto_Velho", "-04"},
                        {"America/Puerto_Rico", "AST"},
                        {"America/Punta_Arenas", "-03"},
                        {"America/Rainy_River", "CST"},
                        {"America/Rankin_Inlet", "CST"},
                        {"America/Recife", "-03"},
                        {"America/Regina", "CST"},
                        {"America/Resolute", "CST"},
                        {"America/Rio_Branco", "-05"},
                        {"America/Rosario", "-03"},
                        {"America/Santa_Isabel", "PST"},
                        {"America/Santarem", "-03"},
                        {"America/Santiago", "-04"},
                        {"America/Santo_Domingo", "AST"},
                        {"America/Sao_Paulo", "-03"},
                        {"America/Scoresbysund", "-01"},
                        {"America/Shiprock", "MST"},
                        {"America/Sitka", "AKST"},
                        {"America/St_Barthelemy", "AST"},
                        {"America/St_Johns", "NST"},
                        {"America/St_Kitts", "AST"},
                        {"America/St_Lucia", "AST"},
                        {"America/St_Thomas", "AST"},
                        {"America/St_Vincent", "AST"},
                        {"America/Swift_Current", "CST"},
                        {"America/Tegucigalpa", "CST"},
                        {"America/Thule", "AST"},
                        {"America/Thunder_Bay", "EST"},
                        {"America/Tijuana", "PST"},
                        {"America/Toronto", "EST"},
                        {"America/Tortola", "AST"},
                        {"America/Vancouver", "PST"},
                        {"America/Virgin", "AST"},
                        {"America/Whitehorse", "PST"},
                        {"America/Winnipeg", "CST"},
                        {"America/Yakutat", "AKST"},
                        {"America/Yellowknife", "MST"},
                        {"Antarctica/Casey", "+08"},
                        {"Antarctica/Davis", "+07"},
                        {"Antarctica/DumontDUrville", "+10"},
                        {"Antarctica/Macquarie", "+11"},
                        {"Antarctica/Mawson", "+05"},
                        {"Antarctica/McMurdo", "NZST"},
                        {"Antarctica/Palmer", "-03"},
                        {"Antarctica/Rothera", "-03"},
                        {"Antarctica/South_Pole", "NZST"},
                        {"Antarctica/Syowa", "+03"},
                        {"Antarctica/Troll", "+00"},
                        {"Antarctica/Vostok", "+06"},
                        {"Arctic/Longyearbyen", "CET"},
                        {"Asia/Aden", "+03"},
                        {"Asia/Almaty", "+06"},
                        {"Asia/Amman", "EET"},
                        {"Asia/Anadyr", "+12"},
                        {"Asia/Aqtau", "+05"},
                        {"Asia/Aqtobe", "+05"},
                        {"Asia/Ashgabat", "+05"},
                        {"Asia/Ashkhabad", "+05"},
                        {"Asia/Atyrau", "+05"},
                        {"Asia/Baghdad", "+03"},
                        {"Asia/Bahrain", "+03"},
                        {"Asia/Baku", "+04"},
                        {"Asia/Bangkok", "+07"},
                        {"Asia/Barnaul", "+07"},
                        {"Asia/Beirut", "EET"},
                        {"Asia/Bishkek", "+06"},
                        {"Asia/Brunei", "+08"},
                        {"Asia/Calcutta", "IST"},
                        {"Asia/Chita", "+09"},
                        {"Asia/Choibalsan", "+08"},
                        {"Asia/Chongqing", "CST"},
                        {"Asia/Chungking", "CST"},
                        {"Asia/Colombo", "+0530"},
                        {"Asia/Dacca", "+06"},
                        {"Asia/Damascus", "EET"},
                        {"Asia/Dhaka", "+06"},
                        {"Asia/Dili", "+09"},
                        {"Asia/Dubai", "+04"},
                        {"Asia/Dushanbe", "+05"},
                        {"Asia/Famagusta", "EET"},
                        {"Asia/Gaza", "EET"},
                        {"Asia/Harbin", "CST"},
                        {"Asia/Hebron", "EET"},
                        {"Asia/Ho_Chi_Minh", "+07"},
                        {"Asia/Hong_Kong", "HKT"},
                        {"Asia/Hovd", "+07"},
                        {"Asia/Irkutsk", "+08"},
                        {"Asia/Istanbul", "+03"},
                        {"Asia/Jakarta", "WIB"},
                        {"Asia/Jayapura", "WIT"},
                        {"Asia/Jerusalem", "IST"},
                        {"Asia/Kabul", "+0430"},
                        {"Asia/Kamchatka", "+12"},
                        {"Asia/Karachi", "PKT"},
                        {"Asia/Kashgar", "+06"},
                        {"Asia/Kathmandu", "+0545"},
                        {"Asia/Katmandu", "+0545"},
                        {"Asia/Khandyga", "+09"},
                        {"Asia/Kolkata", "IST"},
                        {"Asia/Krasnoyarsk", "+07"},
                        {"Asia/Kuala_Lumpur", "+08"},
                        {"Asia/Kuching", "+08"},
                        {"Asia/Kuwait", "+03"},
                        {"Asia/Macao", "CST"},
                        {"Asia/Macau", "CST"},
                        {"Asia/Magadan", "+11"},
                        {"Asia/Makassar", "WITA"},
                        {"Asia/Manila", "+08"},
                        {"Asia/Muscat", "+04"},
                        {"Asia/Nicosia", "EET"},
                        {"Asia/Novokuznetsk", "+07"},
                        {"Asia/Novosibirsk", "+07"},
                        {"Asia/Omsk", "+06"},
                        {"Asia/Oral", "+05"},
                        {"Asia/Phnom_Penh", "+07"},
                        {"Asia/Pontianak", "WIB"},
                        {"Asia/Pyongyang", "KST"},
                        {"Asia/Qatar", "+03"},
                        {"Asia/Qyzylorda", "+06"},
                        {"Asia/Rangoon", "+0630"},
                        {"Asia/Riyadh", "+03"},
                        {"Asia/Saigon", "+07"},
                        {"Asia/Sakhalin", "+11"},
                        {"Asia/Samarkand", "+05"},
                        {"Asia/Seoul", "KST"},
                        {"Asia/Shanghai", "CST"},
                        {"Asia/Singapore", "+08"},
                        {"Asia/Srednekolymsk", "+11"},
                        {"Asia/Taipei", "CST"},
                        {"Asia/Tashkent", "+05"},
                        {"Asia/Tbilisi", "+04"},
                        {"Asia/Tehran", "+0330"},
                        {"Asia/Tel_Aviv", "IST"},
                        {"Asia/Thimbu", "+06"},
                        {"Asia/Thimphu", "+06"},
                        {"Asia/Tokyo", "JST"},
                        {"Asia/Tomsk", "+07"},
                        {"Asia/Ujung_Pandang", "WITA"},
                        {"Asia/Ulaanbaatar", "+08"},
                        {"Asia/Ulan_Bator", "+08"},
                        {"Asia/Urumqi", "+06"},
                        {"Asia/Ust-Nera", "+10"},
                        {"Asia/Vientiane", "+07"},
                        {"Asia/Vladivostok", "+10"},
                        {"Asia/Yakutsk", "+09"},
                        {"Asia/Yangon", "+0630"},
                        {"Asia/Yekaterinburg", "+05"},
                        {"Asia/Yerevan", "+04"},
                        {"Atlantic/Azores", "-01"},
                        {"Atlantic/Bermuda", "AST"},
                        {"Atlantic/Canary", "WET"},
                        {"Atlantic/Cape_Verde", "-01"},
                        {"Atlantic/Faeroe", "WET"},
                        {"Atlantic/Faroe", "WET"},
                        {"Atlantic/Jan_Mayen", "CET"},
                        {"Atlantic/Madeira", "WET"},
                        {"Atlantic/Reykjavik", "GMT"},
                        {"Atlantic/South_Georgia", "-02"},
                        {"Atlantic/St_Helena", "GMT"},
                        {"Atlantic/Stanley", "-03"},
                        {"Australia/ACT", "AEST"},
                        {"Australia/Adelaide", "ACST"},
                        {"Australia/Brisbane", "AEST"},
                        {"Australia/Broken_Hill", "ACST"},
                        {"Australia/Canberra", "AEST"},
                        {"Australia/Currie", "AEST"},
                        {"Australia/Darwin", "ACST"},
                        {"Australia/Eucla", "+0845"},
                        {"Australia/Hobart", "AEST"},
                        {"Australia/LHI", "+1030"},
                        {"Australia/Lindeman", "AEST"},
                        {"Australia/Lord_Howe", "+1030"},
                        {"Australia/Melbourne", "AEST"},
                        {"Australia/NSW", "AEST"},
                        {"Australia/North", "ACST"},
                        {"Australia/Perth", "AWST"},
                        {"Australia/Queensland", "AEST"},
                        {"Australia/South", "ACST"},
                        {"Australia/Sydney", "AEST"},
                        {"Australia/Tasmania", "AEST"},
                        {"Australia/Victoria", "AEST"},
                        {"Australia/West", "AWST"},
                        {"Australia/Yancowinna", "ACST"},
                        {"Brazil/Acre", "-05"},
                        {"Brazil/DeNoronha", "-02"},
                        {"Brazil/East", "-03"},
                        {"Brazil/West", "-04"},
                        {"CET", "CET"},
                        {"CST6CDT", "CST"},
                        {"Canada/Atlantic", "AST"},
                        {"Canada/Central", "CST"},
                        {"Canada/Eastern", "EST"},
                        {"Canada/Mountain", "MST"},
                        {"Canada/Newfoundland", "NST"},
                        {"Canada/Pacific", "PST"},
                        {"Canada/Saskatchewan", "CST"},
                        {"Canada/Yukon", "PST"},
                        {"Chile/Continental", "-04"},
                        {"Chile/EasterIsland", "-06"},
                        {"Cuba", "CST"},
                        {"EET", "EET"},
                        {"EST", "EST"},
                        {"EST5EDT", "EST"},
                        {"Egypt", "EET"},
                        {"Eire", "GMT"},
                        {"Etc/GMT", "GMT"},
                        {"Etc/GMT+0", "GMT"},
                        {"Etc/GMT+1", "-01"},
                        {"Etc/GMT+10", "-10"},
                        {"Etc/GMT+11", "-11"},
                        {"Etc/GMT+12", "-12"},
                        {"Etc/GMT+2", "-02"},
                        {"Etc/GMT+3", "-03"},
                        {"Etc/GMT+4", "-04"},
                        {"Etc/GMT+5", "-05"},
                        {"Etc/GMT+6", "-06"},
                        {"Etc/GMT+7", "-07"},
                        {"Etc/GMT+8", "-08"},
                        {"Etc/GMT+9", "-09"},
                        {"Etc/GMT-0", "GMT"},
                        {"Etc/GMT-1", "+01"},
                        {"Etc/GMT-10", "+10"},
                        {"Etc/GMT-11", "+11"},
                        {"Etc/GMT-12", "+12"},
                        {"Etc/GMT-13", "+13"},
                        {"Etc/GMT-14", "+14"},
                        {"Etc/GMT-2", "+02"},
                        {"Etc/GMT-3", "+03"},
                        {"Etc/GMT-4", "+04"},
                        {"Etc/GMT-5", "+05"},
                        {"Etc/GMT-6", "+06"},
                        {"Etc/GMT-7", "+07"},
                        {"Etc/GMT-8", "+08"},
                        {"Etc/GMT-9", "+09"},
                        {"Etc/GMT0", "GMT"},
                        {"Etc/Greenwich", "GMT"},
                        {"Etc/UCT", "UCT"},
                        {"Etc/UTC", "UTC"},
                        {"Etc/Universal", "UTC"},
                        {"Etc/Zulu", "UTC"},
                        {"Europe/Amsterdam", "CET"},
                        {"Europe/Andorra", "CET"},
                        {"Europe/Astrakhan", "+04"},
                        {"Europe/Athens", "EET"},
                        {"Europe/Belfast", "GMT"},
                        {"Europe/Belgrade", "CET"},
                        {"Europe/Berlin", "CET"},
                        {"Europe/Bratislava", "CET"},
                        {"Europe/Brussels", "CET"},
                        {"Europe/Bucharest", "EET"},
                        {"Europe/Budapest", "CET"},
                        {"Europe/Busingen", "CET"},
                        {"Europe/Chisinau", "EET"},
                        {"Europe/Copenhagen", "CET"},
                        {"Europe/Dublin", "GMT"},
                        {"Europe/Gibraltar", "CET"},
                        {"Europe/Guernsey", "GMT"},
                        {"Europe/Helsinki", "EET"},
                        {"Europe/Isle_of_Man", "GMT"},
                        {"Europe/Istanbul", "+03"},
                        {"Europe/Jersey", "GMT"},
                        {"Europe/Kaliningrad", "EET"},
                        {"Europe/Kiev", "EET"},
                        {"Europe/Kirov", "+03"},
                        {"Europe/Lisbon", "WET"},
                        {"Europe/Ljubljana", "CET"},
                        {"Europe/London", "GMT"},
                        {"Europe/Luxembourg", "CET"},
                        {"Europe/Madrid", "CET"},
                        {"Europe/Malta", "CET"},
                        {"Europe/Mariehamn", "EET"},
                        {"Europe/Minsk", "+03"},
                        {"Europe/Monaco", "CET"},
                        {"Europe/Moscow", "MSK"},
                        {"Europe/Nicosia", "EET"},
                        {"Europe/Oslo", "CET"},
                        {"Europe/Paris", "CET"},
                        {"Europe/Podgorica", "CET"},
                        {"Europe/Prague", "CET"},
                        {"Europe/Riga", "EET"},
                        {"Europe/Rome", "CET"},
                        {"Europe/Samara", "+04"},
                        {"Europe/San_Marino", "CET"},
                        {"Europe/Sarajevo", "CET"},
                        {"Europe/Saratov", "+04"},
                        {"Europe/Simferopol", "MSK"},
                        {"Europe/Skopje", "CET"},
                        {"Europe/Sofia", "EET"},
                        {"Europe/Stockholm", "CET"},
                        {"Europe/Tallinn", "EET"},
                        {"Europe/Tirane", "CET"},
                        {"Europe/Tiraspol", "EET"},
                        {"Europe/Ulyanovsk", "+04"},
                        {"Europe/Uzhgorod", "EET"},
                        {"Europe/Vaduz", "CET"},
                        {"Europe/Vatican", "CET"},
                        {"Europe/Vienna", "CET"},
                        {"Europe/Vilnius", "EET"},
                        {"Europe/Volgograd", "+03"},
                        {"Europe/Warsaw", "CET"},
                        {"Europe/Zagreb", "CET"},
                        {"Europe/Zaporozhye", "EET"},
                        {"Europe/Zurich", "CET"},
                        {"GB", "GMT"},
                        {"GB-Eire", "GMT"},
                        {"GMT", "GMT"},
                        {"GMT+0", "GMT+0"},
                        {"GMT-0", "GMT-0"},
                        {"GMT0", "GMT0"},
                        {"Greenwich", "GMT"},
                        {"HST", "HST"},
                        {"Hongkong", "HKT"},
                        {"Iceland", "GMT"},
                        {"Indian/Antananarivo", "EAT"},
                        {"Indian/Chagos", "+06"},
                        {"Indian/Christmas", "+07"},
                        {"Indian/Cocos", "+0630"},
                        {"Indian/Comoro", "EAT"},
                        {"Indian/Kerguelen", "+05"},
                        {"Indian/Mahe", "+04"},
                        {"Indian/Maldives", "+05"},
                        {"Indian/Mauritius", "+04"},
                        {"Indian/Mayotte", "EAT"},
                        {"Indian/Reunion", "+04"},
                        {"Iran", "+0330"},
                        {"Israel", "IST"},
                        {"Jamaica", "EST"},
                        {"Japan", "JST"},
                        {"Kwajalein", "+12"},
                        {"Libya", "EET"},
                        {"MET", "MET"},
                        {"MST", "MST"},
                        {"MST7MDT", "MST"},
                        {"Mexico/BajaNorte", "PST"},
                        {"Mexico/BajaSur", "MST"},
                        {"Mexico/General", "CST"},
                        {"NZ", "NZST"},
                        {"NZ-CHAT", "+1245"},
                        {"Navajo", "MST"},
                        {"PRC", "CST"},
                        {"PST8PDT", "PST"},
                        {"Pacific/Apia", "+13"},
                        {"Pacific/Auckland", "NZST"},
                        {"Pacific/Bougainville", "+11"},
                        {"Pacific/Chatham", "+1245"},
                        {"Pacific/Chuuk", "+10"},
                        {"Pacific/Easter", "-06"},
                        {"Pacific/Efate", "+11"},
                        {"Pacific/Enderbury", "+13"},
                        {"Pacific/Fakaofo", "+13"},
                        {"Pacific/Fiji", "+12"},
                        {"Pacific/Funafuti", "+12"},
                        {"Pacific/Galapagos", "-06"},
                        {"Pacific/Gambier", "-09"},
                        {"Pacific/Guadalcanal", "+11"},
                        {"Pacific/Guam", "ChST"},
                        {"Pacific/Honolulu", "HST"},
                        {"Pacific/Johnston", "HST"},
                        {"Pacific/Kiritimati", "+14"},
                        {"Pacific/Kosrae", "+11"},
                        {"Pacific/Kwajalein", "+12"},
                        {"Pacific/Majuro", "+12"},
                        {"Pacific/Marquesas", "-0930"},
                        {"Pacific/Midway", "SST"},
                        {"Pacific/Nauru", "+12"},
                        {"Pacific/Niue", "-11"},
                        {"Pacific/Norfolk", "+11"},
                        {"Pacific/Noumea", "+11"},
                        {"Pacific/Pago_Pago", "SST"},
                        {"Pacific/Palau", "+09"},
                        {"Pacific/Pitcairn", "-08"},
                        {"Pacific/Pohnpei", "+11"},
                        {"Pacific/Ponape", "+11"},
                        {"Pacific/Port_Moresby", "+10"},
                        {"Pacific/Rarotonga", "-10"},
                        {"Pacific/Saipan", "ChST"},
                        {"Pacific/Samoa", "SST"},
                        {"Pacific/Tahiti", "-10"},
                        {"Pacific/Tarawa", "+12"},
                        {"Pacific/Tongatapu", "+13"},
                        {"Pacific/Truk", "+10"},
                        {"Pacific/Wake", "+12"},
                        {"Pacific/Wallis", "+12"},
                        {"Pacific/Yap", "+10"},
                        {"Poland", "CET"},
                        {"Portugal", "WET"},
                        {"ROC", "CST"},
                        {"ROK", "KST"},
                        {"Singapore", "+08"},
                        {"Turkey", "+03"},
                        {"UCT", "UCT"},
                        {"US/Alaska", "AKST"},
                        {"US/Aleutian", "HST"},
                        {"US/Arizona", "MST"},
                        {"US/Central", "CST"},
                        {"US/East-Indiana", "EST"},
                        {"US/Eastern", "EST"},
                        {"US/Hawaii", "HST"},
                        {"US/Indiana-Starke", "CST"},
                        {"US/Michigan", "EST"},
                        {"US/Mountain", "MST"},
                        {"US/Pacific", "PST"},
                        {"US/Samoa", "SST"},
                        {"UTC", "UTC"},
                        {"Universal", "UTC"},
                        {"W-SU", "MSK"},
                        {"WET", "WET"},
                        {"Zulu", "UTC"}
                    };
                }

                return _OlsenToStandardDictionary;
            }
            set
            {
                _OlsenToStandardDictionary = value;
            }
        }

        private static Dictionary<string, string> _StandardToOlsenDictionary = null;

        public static Dictionary<string, string> StandardToOlsenDictionary
        {
            get
            {
                if (_StandardToOlsenDictionary == null)
                {
                    _StandardToOlsenDictionary = new Dictionary<string, string>();

                    foreach (KeyValuePair<string, string> kvp in OlsenToStandardDictionary)
                    {
                        string olsen = kvp.Key;
                        string standard = kvp.Value;
                        string test;

                        if (!_StandardToOlsenDictionary.TryGetValue(olsen, out test))
                            _StandardToOlsenDictionary.Add(standard, olsen);
                    }
                }

                return _StandardToOlsenDictionary;
            }
            set
            {
                _StandardToOlsenDictionary = value;
            }
        }

        public static string FindWindowsTimeZoneID(string str)
        {
            string windowsTimeZoneID;

            if (OlsenToWindowsDictionary.TryGetValue(str, out windowsTimeZoneID))
                return windowsTimeZoneID;

            if (StandardToWindowsDictionary.TryGetValue(str, out windowsTimeZoneID))
                return windowsTimeZoneID;

            return null;
        }

        public static string FindOlsenTimeZoneID(string str)
        {
            string olsenTimeZoneID;
            List<string> olsenList;

            if (WindowsToOlsenDictionary.TryGetValue(str, out olsenList))
            {
                if ((olsenList != null) && (olsenList.Count() != 0))
                    olsenTimeZoneID = olsenList.First();
                else
                    olsenTimeZoneID = String.Empty;

                return olsenTimeZoneID;
            }

            if (StandardToOlsenDictionary.TryGetValue(str, out olsenTimeZoneID))
                return olsenTimeZoneID;

            return null;
        }

        public static string FindStandardTimeZoneID(string str)
        {
            string nsTimeZoneID;

            if (OlsenToStandardDictionary.TryGetValue(str, out nsTimeZoneID))
                return nsTimeZoneID;

            if (WindowsToStandardDictionary.TryGetValue(str, out nsTimeZoneID))
                return nsTimeZoneID;

            return null;
        }

        // For creating the initializers.
#if false
        public static string TimeZoneList
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                foreach (string tz in WindowsTimeZoneIDs)
                    sb.AppendLine("\"" + tz + "\"");

                string tzl = sb.ToString();
                return tzl;
            }
        }

        private static List<string> _WindowsOlsenStandardTimeZones;
        private static List<string> WindowsOlsenStandardTimeZones
        {
            get
            {
                if (_WindowsOlsenStandardTimeZones == null)
                {
                    _WindowsOlsenStandardTimeZones = new List<string>()
                    {
                        // Deleted - Get first part of \tmp\WindowsOlsenNSTimeZones.txt
                    };
                }
                return _WindowsOlsenStandardTimeZones;
            }
            set
            {
                _WindowsOlsenStandardTimeZones = value;
            }
        }

        public static void FixConversionList()
        {
            Dictionary<string, List<string>> windowsToOlsenDict = new Dictionary<string, List<string>>();
            Dictionary<string, string> olsenToWindowsDict = new Dictionary<string, string>();
            Dictionary<string, string> standardToWindowsDict = new Dictionary<string, string>();
            Dictionary<string, string> windowsToStandardDict = new Dictionary<string, string>();

            for (int i = 0, c = WindowsOlsenStandardTimeZones.Count(); i < c; i += 3)
            {
                string windows = WindowsOlsenStandardTimeZones[i];
                string olsenMulti = WindowsOlsenStandardTimeZones[i + 1];

                string[] parts = olsenMulti.Split(LanguageLookup.SpaceCharacter, StringSplitOptions.RemoveEmptyEntries);

                List<string> olsenList;

                if (!windowsToOlsenDict.TryGetValue(windows, out olsenList))
                {
                    olsenList = new List<string>();
                    windowsToOlsenDict.Add(windows, olsenList);
                }

                foreach (string olsen in parts)
                {
                    if (!olsenList.Contains(olsen))
                    {
                        olsenList.Add(olsen);

                        string testWindows;

                        if (!olsenToWindowsDict.TryGetValue(olsen, out testWindows))
                            olsenToWindowsDict.Add(olsen, windows);

                        string standard, testStandard;

                        if (OlsenToStandardDictionary.TryGetValue(olsen, out standard))
                        {
                            if (!windowsToStandardDict.TryGetValue(windows, out testStandard))
                                windowsToStandardDict.Add(windows, standard);

                            if (!standardToWindowsDict.TryGetValue(standard, out testWindows))
                                standardToWindowsDict.Add(standard, windows);
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            // Do WindowsToOlsenDictionary
            OutputStringListInitializerList(sb, "Windows to Olsen:", windowsToOlsenDict);

            // Do OlsenToWindowsDictionary
            OutputStringStringInitializerList(sb, "Olsen to Windows:", olsenToWindowsDict);

            // Do WindowsToStandardDictionary
            OutputStringStringInitializerList(sb, "Windows to Standard:", standardToWindowsDict);

            // Do StandardToWindowsDictionary
            OutputStringStringInitializerList(sb, "Standard to Windows:", standardToWindowsDict);

            string str = sb.ToString();
            ApplicationData.Global.PutConsoleMessage(str);
        }

        private static void OutputStringListInitializerList(
            StringBuilder sb,
            string label,
            Dictionary<string, List<string>> dict)
        {
            int entryIndex = 0;

            sb.AppendLine("Windows to Olsen:");
            sb.AppendLine("");

            foreach (KeyValuePair<string, List<string>> kvp in dict)
            {
                string key = kvp.Key;
                List<string> value = kvp.Value;

                if (entryIndex != 0)
                    sb.AppendLine(",");

                sb.Append("                        {\"");
                sb.Append(key);
                sb.Append("\", new List<string>{");

                if ((value != null) && (value.Count() != 0))
                {
                    int listIndex = 0;

                    foreach (string str in value)
                    {
                        if (listIndex != 0)
                            sb.Append(", ");

                        sb.Append("\"");
                        sb.Append(str);
                        sb.Append("\"");

                        listIndex++;
                    }
                }

                sb.Append("}}");
                entryIndex++;
            }

            sb.AppendLine("");
            sb.AppendLine("");
        }

        private static void OutputStringStringInitializerList(
            StringBuilder sb,
            string label,
            Dictionary<string, string> dict)
        {
            int entryIndex = 0;

            sb.AppendLine(label);
            sb.AppendLine("");

            foreach (KeyValuePair<string, string> kvp in dict)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                if (entryIndex != 0)
                    sb.AppendLine(",");

                sb.Append("                        {\"");
                sb.Append(key);
                sb.Append("\", \"");
                sb.Append(value);
                sb.Append("\"}");

                entryIndex++;
            }

            sb.AppendLine("");
            sb.AppendLine("");
        }
#endif
    }
}

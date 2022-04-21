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
    public class GermanTool : LanguageTool
    {
        public GermanTool() : base(LanguageLookup.German)
        {
        }

        public override IBaseObject Clone()
        {
            return new GermanTool();
        }

        // From https://spanishdictionary.cc/common-spanish-abbreviations
        public static Dictionary<string, string> GermanAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "v. Chr.", "vor Christus" }               // B.C.
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return GermanAbbreviationDictionary;
            }
        }
    }
}

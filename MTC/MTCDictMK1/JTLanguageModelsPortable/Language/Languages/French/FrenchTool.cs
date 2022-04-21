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
    public class FrenchTool : LanguageTool
    {
        public FrenchTool() : base(LanguageLookup.French)
        {
            _UsesImplicitPronouns = true;
        }

        public override IBaseObject Clone()
        {
            return new FrenchTool();
        }

        // From https://spanishdictionary.cc/common-spanish-abbreviations
        public static Dictionary<string, string> FrenchAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "av. J.-C.", "avant Jésus-Christ" }               // B.C.
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return FrenchAbbreviationDictionary;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class GenericLanguageTool : LanguageTool
    {
        public GenericLanguageTool(
            LanguageID languageID,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> userLanguageIDs) : base(languageID)
        {
            TargetLanguageIDs = new List<LanguageID>() { languageID };
            HostLanguageIDs = hostLanguageIDs;
            UserLanguageIDs = userLanguageIDs;
        }

        public GenericLanguageTool()
        {
        }

        public override IBaseObject Clone()
        {
            return new GenericLanguageTool(LanguageID, HostLanguageIDs, UserLanguageIDs);
        }
    }
}

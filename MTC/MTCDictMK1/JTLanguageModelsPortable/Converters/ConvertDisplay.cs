using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Converters
{
    public class ConvertDisplay : ConvertBase
    {
        public LanguageID LanguageID { get; set; }
        DictionaryRepository DictionaryRepository { get; set; }
        public FormatQuickLookup QuickDictionary { get; set; }

        public ConvertDisplay(LanguageID languageID, DictionaryRepository dictionaryRepository, bool useQuickDictionary)
        {
            LanguageID = languageID;
            DictionaryRepository = dictionaryRepository;
            if (useQuickDictionary)
            {
                List<LanguageID> languageIDs = LanguageLookup.GetAlternateLanguageIDs(LanguageID);
                if ((languageIDs != null) && (languageIDs.Count() != 0))
                    QuickDictionary = FormatQuickLookup.GetQuickDictionary(LanguageID, languageIDs[0]);
            }
        }

        public virtual bool Display(out string output, string input)
        {
            bool returnValue = true;

            output = input;

            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    returnValue = ConvertPinyinNumeric.Display(out output, input, LanguageID, false, DictionaryRepository, QuickDictionary);
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override bool To(out string output, string input)
        {
            bool returnValue = true;

            output = input;

            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    returnValue = ConvertPinyinNumeric.Display(out output, input, LanguageID, false, DictionaryRepository, QuickDictionary);
                    break;
                default:
                    output = input;
                    break;
            }

            return returnValue;
        }

        public override bool From(out string output, string input)
        {
            bool returnValue = true;

            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    returnValue = ConvertPinyinNumeric.Canonical(out output, input, false);
                    break;
                default:
                    output = input;
                    break;
            }

            return returnValue;
        }
    }
}

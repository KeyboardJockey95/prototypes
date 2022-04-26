using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPatternedBlock : FormatPatterned
    {
        private static string FormatDescription = "This is a language block-oriented format,"
            + " where each block of lines"
            + " represents the language items for one language in a set of study items,"
            + " formatted according to a user-provided block/line substitution pattern,"
            + " or an optional comment or directive.";

        public FormatPatternedBlock()
            : base("Block", "Patterned Block", "FormatPatternedBlock", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
        }

        public FormatPatternedBlock(FormatPatterned other)
            : base(other)
        {
        }

        // For derived classes.
        public FormatPatternedBlock(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Block", name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
        }

        public override Format Clone()
        {
            return new FormatPatternedBlock(this);
        }
    }
}

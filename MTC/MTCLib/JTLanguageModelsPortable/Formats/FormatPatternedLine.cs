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
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPatternedLine : FormatPatterned
    {
        private static string FormatDescription = "This is a line-oriented format, where each text row represents a study"
            + " item formatted according to a user-provided substitution pattern, or an optional comment or directive.";

        public FormatPatternedLine()
            : base("Line", "Patterned Line", "FormatPatternedLine", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
        }

        public FormatPatternedLine(FormatPatterned other)
            : base(other)
        {
        }

        // For derived classes.
        public FormatPatternedLine(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Line", name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
        }

        public override Format Clone()
        {
            return new FormatPatternedLine(this);
        }
    }
}

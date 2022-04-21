using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatVendant : FormatPatterned
    {
        public String Category { get; set; }

        public FormatVendant(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, TypeStringStatic, userRecord, repositories, translator, languageUtilities)
        {
        }

        public FormatVendant(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, targetName, targetOwner, targetList, languageDescriptors, itemIndex, userRecord,
                repositories, translator, languageUtilities)
        {
        }

        protected override void SetupDefaults()
        {
            base.SetupDefaults();
            MimeType = "text/plain";
            Pattern = "%{t}\\t%{h}\\t%{s}";
        }

        public static string CategoryHelp = "This is the category to be used.";

        public override void LoadFromArguments()
        {
            if (Arguments == null)
                return;

            base.LoadFromArguments();

            Category = GetArgumentDefaulted("Category", "string", "rw", "", "Category", CategoryHelp);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();
            SetArgument("Category", "string", "rw", Category, "Category", CategoryHelp);
        }

        protected override void ReadFormatHeader(StreamReader reader)
        {
            string line;

            if ((line = reader.ReadLine()) != null)
            {
                if (!line.StartsWith("Front") && !line.StartsWith("Back"))
                {
                    Error = "Missing \"Front\tBack\tCategory\" header line.";
                    throw new ModelException(Error);
                };
            }
        }

        protected override void WriteFormatHeader(StreamWriter writer)
        {
            writer.WriteLine("Front\tBack\tCategory");
        }

        protected override void WriteComponent(StreamWriter writer)
        {
            List<object> arguments = new List<object>(1) { Category };
            ItemCount = GetEntryCount();

            while (ItemIndex < ItemCount)
            {
                if ((UseFlags != null) && !UseFlags[ItemIndex])
                {
                    ItemIndex = ItemIndex + 1;
                    continue;
                }

                string line = FormatLine(arguments);

                if (line != null)
                    writer.WriteLine(line);
                else
                    break;
            }
        }

        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            switch (componentName)
            {
                case "Text":
                case "TargetTextComponent":
                case "Transcript":
                case "TranscriptComponent":
                case "Vocabulary":
                case "Vocabulary Characters":
                case "Vocabulary Words":
                case "Vocabulary Sentences":
                case "StudyListComponent":
                case "Notes":
                case "NotesComponent":
                case "Comments":
                case "CommentsComponent":
                case "FlashList":
                    if (capability == "Support")
                        return true;
                    else if ((importExport == "Export") && (capability == "UseFlags"))
                        return true;
                    return false;
                case "Lesson":
                    if (importExport == "Import")
                        return false;
                    if (capability == "Support")
                        return true;
                    else if (capability == "ComponentFlags")
                        return true;
                    return false;
                case "LessonGroup":
                case "Course":
                case "Plan":
                    if (importExport == "Import")
                        return false;
                    if (capability == "Support")
                        return true;
                    else if (capability == "ComponentFlags")
                        return true;
                    else if (capability == "NodeFlags")
                        return true;
                    return false;
                case "Dictionary":
                case "LessonMaster":
                case "MarkupTemplate":
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "Vendant Flash Card Manager"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

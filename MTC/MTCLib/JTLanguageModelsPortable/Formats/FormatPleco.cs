using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPleco : FormatComponent
    {
        LanguageID _HostLanguageID;
        LanguageID _SimplifiedLanguageID;
        LanguageID _TraditionalLanguageID;
        LanguageID _PinyinLanguageID;
        public bool IncludeCategories { get; set; }
        public bool IncludeTraditional { get; set; }
        public bool IncludePinyin { get; set; }
        public bool IncludeHost { get; set; }
        public string ErrorMessage { get; set; }

        public FormatPleco(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, TypeStringStatic, userRecord, repositories, translator, languageUtilities)
        {
            SetupDefaults();
            SetupLanguages();
        }

        public FormatPleco(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, targetName, targetOwner, targetList, languageDescriptors, itemIndex, TypeStringStatic, userRecord,
                repositories, translator, languageUtilities)
        {
            SetupDefaults();
            SetupLanguages();
        }

        public FormatPleco(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex, string type,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, type, userRecord, repositories, translator, languageUtilities)
        {
            SetupDefaults();
            SetupLanguages();
        }

        protected virtual void SetupDefaults()
        {
            MimeType = "text/plain";
            IncludeCategories = true;
            IncludeTraditional = true;
            IncludePinyin = true;
            IncludeHost = true;
        }

        protected virtual void SetupLanguages()
        {
            LanguageDescriptor languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == "Host");

            if (languageDescriptor != null)
                _HostLanguageID = languageDescriptor.LanguageID;

            if ((LanguageDescriptors.FirstOrDefault(x => x.LanguageID == LanguageLookup.ChineseSimplified) == null) &&
                    (LanguageDescriptors.FirstOrDefault(x => x.LanguageID == LanguageLookup.ChineseTraditional) == null))
            {
                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == "Target");
                _SimplifiedLanguageID = languageDescriptor.LanguageID;
            }
            else
                _SimplifiedLanguageID = LanguageLookup.ChineseSimplified;

            _TraditionalLanguageID = LanguageLookup.ChineseTraditional;
            _PinyinLanguageID = LanguageLookup.ChinesePinyin;
        }

        public static string IncludeCategoriesHelp = "Set this to true to include category headings, based on the item title.";
        public static string IncludeTraditionalHelp = "Set this to true to include traditional characters.";
        public static string IncludePinyinHelp = "Set this to true to include pinyin romanization.";
        public static string IncludeHostHelp = "Set this to true to include the host language definition.";

        public override void LoadFromArguments()
        {
            if (Arguments == null)
                return;

            base.LoadFromArguments();

            IncludeCategories = (GetArgumentDefaulted("IncludeCategories", "flag", "w", "on", "Include Categories", IncludeCategoriesHelp) == "on" ? true : false);;
            IncludeTraditional = (GetArgumentDefaulted("IncludeTraditional", "flag", "w", "on", "Include Traditional", IncludeTraditionalHelp) == "on" ? true : false); ;
            IncludePinyin = (GetArgumentDefaulted("IncludePinyin", "flag", "w", "on", "Include Pinyin", IncludePinyinHelp) == "on" ? true : false); ;
            IncludeHost = (GetArgumentDefaulted("IncludeHost", "flag", "w", "on", "Include Host", IncludeHostHelp) == "on" ? true : false); ;
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();
            SetArgument("IncludeCategories", "flag", "w", (IncludeCategories ? "on" : ""), "Include Categories", IncludeCategoriesHelp);
            SetArgument("IncludeTraditional", "flag", "w", (IncludeTraditional ? "on" : ""), "Include Traditional", IncludeTraditionalHelp);
            SetArgument("IncludePinyin", "flag", "w", (IncludePinyin ? "on" : ""), "Include Pinyin", IncludePinyinHelp);
            SetArgument("IncludeHost", "flag", "w", (IncludeHost ? "on" : ""), "Include Host", IncludeHostHelp);
        }

        protected override void ReadObject(StreamReader reader, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList) || (obj is TitledNode))
            {
                Component = obj;
                if (IsSupportedStatic("Import", GetComponentName(), "Support"))
                {
                    ReadComponent(reader);
                    ComponentIndex = ComponentIndex + 1;
                }
                Component = null;
            }
            else
            {
                Error = "Read not supported for this object type: " + obj.GetType().Name;
                throw new ModelException(Error);
            }
        }

        protected override void ReadComponent(StreamReader reader)
        {
            string line;

            ItemCount = 0;

            while ((line = reader.ReadLine()) != null)
                ScanLine(line);
        }

        protected override void WriteComponent(StreamWriter writer)
        {
            writer.WriteLine("// " + GetComponentLongTitle());

            ItemCount = GetEntryCount();

            while (ItemIndex < ItemCount)
            {
                if ((UseFlags != null) && !UseFlags[ItemIndex])
                {
                    ItemIndex = ItemIndex + 1;
                    continue;
                }

                string line = FormatLine();

                if (line != null)
                    writer.WriteLine(line);
                else
                    break;
            }
        }

        private static char[] Separators = new char[] { '\t' };
        private static char[] CharacterSeparators = new char[] { '[', ']', ' '};

        protected void ScanLine(string line)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            if (line.StartsWith("//"))
                return;

            string[] fields = line.Split(Separators);

            string[] characterFields = fields[0].Split(CharacterSeparators, StringSplitOptions.RemoveEmptyEntries);

            string simplified = characterFields[0];

            string traditional;

            if (characterFields.Count() > 1)
                traditional = characterFields[1];
            else
                traditional = simplified;

            string pinyin;

            if (fields.Count() >= 2)
                pinyin = fields[1];
            else
                pinyin = String.Empty;

            string host;

            if (fields.Count() >= 3)
                host = fields[2];
            else
                host = String.Empty;

            MultiLanguageItem multiLanguageItem = new MultiLanguageItem(
                simplified,
                new List<LanguageItem>()
                {
                    new LanguageItem(simplified, _SimplifiedLanguageID, simplified),
                    new LanguageItem(simplified, _TraditionalLanguageID, traditional),
                    new LanguageItem(simplified, _PinyinLanguageID, pinyin),
                    new LanguageItem(simplified, _HostLanguageID, host),
                });

            AddEntry(multiLanguageItem);
        }

        protected string FormatLine(List<object> arguments = null)
        {
            MultiLanguageItem multiLanguageItem = GetEntry();

            if (multiLanguageItem == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            string simplified = multiLanguageItem.Text(_SimplifiedLanguageID);
            string traditional = multiLanguageItem.Text(_TraditionalLanguageID);
            string pinyin = multiLanguageItem.Text(_PinyinLanguageID);
            string host = multiLanguageItem.Text(_HostLanguageID);

            if (String.IsNullOrEmpty(simplified))
                simplified = traditional;

            sb.Append(simplified);

            if (IncludeTraditional && !string.IsNullOrEmpty(traditional) && (traditional != simplified))
                sb.Append("[" + traditional + "]");

            if (IncludePinyin || IncludeHost)
            {
                sb.Append("\t");

                if (IncludePinyin && !string.IsNullOrEmpty(pinyin))
                    sb.Append(pinyin);

                if (IncludeHost)
                {
                    sb.Append("\t");

                    if (!string.IsNullOrEmpty(host))
                        sb.Append(host);
                }
            }

            return sb.ToString();
        }

        // Progress is 0.0f to 1.0f, where 1.0f means complete.
        public override float GetProgress()
        {
            return 1.0f;
        }

        public override string GetProgressMessage()
        {
            string message;

            if (ItemCount == 0)
                message = "Import beginning...";
            else
                message = "Read completed.  " + ItemCount.ToString() + " items created.";

            if (Timer != null)
                message += "  Elapsed time is " + ElapsedTime.ToString();

            return message;
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

        public static new string TypeStringStatic { get { return "Pleco"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

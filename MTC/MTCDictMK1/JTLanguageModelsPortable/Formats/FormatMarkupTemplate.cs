using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatMarkupTemplate : FormatComponent
    {
        protected IMainRepository _Repositories;
        protected MarkupTemplate _MarkupTemplate = null;
        public string MarkupTemplateKey { get; set; }
        public new LanguageID HostLanguageID { get; set; }
        public new LanguageID TargetLanguageID { get; set; }

        public FormatMarkupTemplate(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, TypeStringStatic, userRecord, repositories, translator, languageUtilities)
        {
            _Repositories = repositories;
            SetupDefaults(userRecord);
        }

        protected virtual void SetupDefaults(UserRecord userRecord)
        {
            MimeType = "text/html";
            DefaultFileExtension = ".html";
            HostLanguageID = userRecord.HostLanguage;
            TargetLanguageID = userRecord.TargetLanguage;

            if (TargetLanguageID == null)
                TargetLanguageID = HostLanguageID;
        }

        public static string MarkupTemplateKeyHelp = "Select the markup template to use.";

        public static string HostLanguageIDHelp = "Select the host language to use for strings.";

        public static string TargetLanguageIDHelp = "Select the default language to use for content.";

        public override void LoadFromArguments()
        {
            if (Arguments == null)
                return;

            base.LoadFromArguments();

            MarkupTemplateKey = GetArgumentDefaulted("MarkupTemplateKey", "titledobjectlist", "rw", "(select)", "Markup template", MarkupTemplateKeyHelp);

            HostLanguageID = LanguageLookup.GetLanguageIDNoAdd(GetArgumentDefaulted(
                "HostLanguageID", "languageID", "rw", HostLanguageID.LanguageCultureExtensionCode, "Host language", HostLanguageIDHelp));

            TargetLanguageID = LanguageLookup.GetLanguageIDNoAdd(GetArgumentDefaulted(
                "TargetLanguageID", "languageID", "rw", TargetLanguageID.LanguageCultureExtensionCode, "Target language", TargetLanguageIDHelp));
        }

        public override void SaveToArguments()
        {
            FormatArgument argument;

            base.SaveToArguments();

            argument = SetArgument("MarkupTemplateKey", "titledobjectlist", "rw", MarkupTemplateKey, "Markup template", MarkupTemplateKeyHelp);
            List<MarkupTemplate> markupTemplates = _Repositories.MarkupTemplates.GetList(GetTarget<TitledBase>().Owner);
            argument.SetTitledBaseList<MarkupTemplate>(markupTemplates);

            argument = SetArgument("HostLanguageID", "languageID", "rw", HostLanguageID.LanguageCultureExtensionCode, "Host language", HostLanguageIDHelp);

            SetArgument("TargetLanguageID", "languageID", "rw", TargetLanguageID.LanguageCultureExtensionCode, "Target language", TargetLanguageIDHelp);
        }

        protected override void WriteFormatHeader(StreamWriter writer)
        {
            if (_MarkupTemplate == null)
            {
                if (!String.IsNullOrEmpty(MarkupTemplateKey) && (MarkupTemplateKey != "(select)"))
                {
                    _MarkupTemplate = _Repositories.MarkupTemplates.Get(LessonUtilities.KeyObject(MarkupTemplateKey, "MarkupTemplates"));

                    if (_MarkupTemplate == null)
                        throw new ModelException("Markup template not found: " + MarkupTemplateKey);
                }
                else
                    throw new ModelException("No markup template specified.");
            }
        }

        protected override void WriteObject(StreamWriter writer, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
            {
                Component = obj;
                if (IsSupportedVirtual("Export", GetComponentName(), "Support"))
                {
                    WriteComponent(writer);
                    ComponentIndex = ComponentIndex + 1;
                }
                Component = null;
            }
            else if (obj is TitledNode)
            {
                if (obj is Lesson)
                {
                    string markupText = _MarkupTemplate.RenderedMarkupBodyString(null, Tree, obj as Lesson, TargetLanguageID, HostLanguageID, LanguageDescriptors,
                        UserRecord, Repositories, Translator, LanguageUtilities);
                    writer.Write(markupText);
                }

                string source = LessonUtilities.ObjectSourceFromObject(obj);
                TitledNode titledNode = obj as TitledNode;

                if (titledNode != Tree)
                    titledNode = Tree.GetNodeWithSource(titledNode.Key, source);

                if (titledNode.Children != null)
                {
                    foreach (TitledReference child in titledNode.Children)
                    {
                        if (child.Item != null)
                            WriteObject(writer, child.Item);
                    }
                }
            }
            else if (obj is TitledBase)
            {
                string markupText = _MarkupTemplate.RenderedMarkupBodyString(null, Tree, Target as Lesson, TargetLanguageID, HostLanguageID, LanguageDescriptors,
                    UserRecord, Repositories, Translator, LanguageUtilities);
                writer.Write(markupText);
            }
            else
                throw new ModelException(Error = "Format: Write not implemented for this object type" + ": " + Target.GetType().Name);
        }

        protected override void WriteComponent(StreamWriter writer)
        {
            LessonComponent component = GetComponent<LessonComponent>();

            if (component != null)
            {
                string markupText = _MarkupTemplate.RenderedMarkupBodyString(null, Tree, component.Lesson, TargetLanguageID, HostLanguageID, LanguageDescriptors,
                    UserRecord, Repositories, Translator, LanguageUtilities);
                writer.Write(markupText);
            }
            else
                throw new ModelException("MarkupTemplate format doesn't support this component type.");
        }

        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            if (importExport == "Import")
                return false;
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
                    return false;
                case "Lesson":
                    if (capability == "Support")
                        return true;
                    return false;
                case "LessonGroup":
                case "Course":
                case "Plan":
                    if (capability == "Support")
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

        public static new string TypeStringStatic { get { return "Markup Template"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

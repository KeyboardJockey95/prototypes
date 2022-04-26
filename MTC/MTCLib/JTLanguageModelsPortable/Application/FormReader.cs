using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Application
{
    public class FormReader : FormUtilities
    {
        public FormReader(IMainRepository repositories, IApplicationCookies cookies,
                UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
                LanguageUtilities languageUtilities, Dictionary<string, string> formValues)
            : base(repositories, cookies, userRecord, userProfile, translator, languageUtilities, formValues)
        {
        }

        public MultiLanguageItem GetMultiLanguageItem(string name)
        {
            if (!String.IsNullOrEmpty(name))
                name += "_";
            else
                name = String.Empty;
            List<LanguageItem> languageItems = new List<LanguageItem>();
            string countString = this[name + "count"] as string;
            int count = Convert.ToInt32(countString);
            string itemID = GetString(name + ".sid");
            int index;

            for (index = 0; index < count; index++)
            {
                string languageCode = this[name + "lid_" + index.ToString()];
                LanguageID languageID = new LanguageID(languageCode);
                string value = GetString(name + "text_" + index.ToString());

                languageItems.Add(new LanguageItem(itemID, languageID, value));
            }

            MultiLanguageItem multiLanguageItem = new MultiLanguageItem(itemID, languageItems);

            return multiLanguageItem;
        }

        public MarkupTemplate GetMarkupTemplateProfile()
        {
            MarkupTemplate markupTemplate = new MarkupTemplate(
                0,
                GetMultiLanguageString("MarkupTemplate.Title"),                       // MultiLanguageString Title
                GetMultiLanguageString("MarkupTemplate.Description"),                 // MultiLanguageString Description
                "MarkupTemplates",                                                    // string Source
                GetString("MarkupTemplate.Package"),                                  // string Package
                GetString("MarkupTemplate.Label"),                                    // string Label
                GetString("MarkupTemplate.ImageFileName"),                            // string ImageFileName
                GetInteger("MarkupTemplate.Index"),                                   // int Index
                GetFlag("MarkupTemplate.IsPublic"),                                   // bool IsPublic
                GetLanguageIDList("MarkupTemplate.TargetLanguageIDs"),                // List<LanguageID> TargetLanguageIDs
                GetLanguageIDList("MarkupTemplate.HostLanguageIDs"),                  // List<LanguageID> HostLanguageIDs
                GetString("MarkupTemplate.Owner"),                                    // string Owner
                null,                                                                 // XElement Markup
                null);                                                                // List<MultiLanguageItem> MultiLanguageItems
            markupTemplate.SetupDirectory();
            return markupTemplate;
        }

        public NodeMaster GetNodeMasterProfile()
        {
            NodeMaster masterData = new NodeMaster(
                0,
                GetMultiLanguageString("NodeMaster.Title"),                           // MultiLanguageString Title
                GetMultiLanguageString("NodeMaster.Description"),                     // MultiLanguageString Description
                "NodeMasters",                                                        // string Source
                GetString("NodeMaster.Package"),                                      // string Package
                GetString("NodeMaster.Label"),                                        // string Label
                GetString("NodeMaster.ImageFileName"),                                // string ImageFileName
                GetInteger("NodeMaster.Index"),                                       // int Index
                GetFlag("NodeMaster.IsPublic"),                                       // bool IsPublic
                GetLanguageIDList("NodeMaster.TargetLanguageIDs"),                    // List<LanguageID> TargetLanguageIDs
                GetLanguageIDList("NodeMaster.HostLanguageIDs"),                      // List<LanguageID> HostLanguageIDs
                GetString("NodeMaster.Owner"),                                        // string Owner
                null,                                                                 // List<MasterContentItem> contentItems
                null,                                                                 // List<MasterMenuItem> menuItems
                GetMarkupReference("NodeMaster.MarkupTemplateKey"),                   // MarkupTemplateReference markupReference
                GetMarkupReference("NodeMaster.CopyMarkupTemplateKey"),               // MarkupTemplateReference copyMarkupReference
                null);                                                                // List<OptionDescriptor> optionDescriptors

            return masterData;
        }

        public MasterMenuItem GetMasterMenuItem()
        {
            MasterMenuItem menuItem = new MasterMenuItem(
                GetString("text"),                                                    // string text
                GetString("action"),                                                  // string action
                GetString("controller"),                                              // string controller
                GetString("nodeContentKey"),                                          // string nodeContentKey
                GetString("contentType"),                                             // string contentType
                GetString("contentSubType"));                                         // string contentSubType

            return menuItem;
        }

        public OptionDescriptor GetOptionDescriptor()
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(
                GetString("name"),                                                    // string name
                GetString("type"),                                                    // string type
                GetString("label"),                                                   // string label
                GetString("help"),                                                    // string help
                GetString("value"),                                                   // string value
                GetStringList("values"));                                             // List<string> values

            return optionDescriptor;
        }

        public List<OptionDescriptor> GetOptionDescriptors()
        {
            List<OptionDescriptor> optionDescriptors = new List<OptionDescriptor>();
            int count = GetInteger("option_count");

            for (int index = 0; index < count; index++)
            {
                string type = GetString("option_type_" + index.ToString());
                string optionFieldName = "option_value_" + index.ToString();
                string value = GetString(optionFieldName);
                List<string> values = ObjectUtilities.GetStringListFromString(GetString("option_values_" + index.ToString()));

                switch (type)
                {
                    case "string":
                    case "bigstring":
                    case "text":
                        break;
                    case "flag":
                        if (value == "on")
                            value = "true";
                        else
                            value = "false";
                        break;
                    case "integer":
                        AssertValidNumberString(value, optionFieldName);
                        break;
                    case "float":
                        AssertValidFloatString(value, optionFieldName);
                        break;
                    case "namedLanguage":
                        AssertValidSelectString(value, optionFieldName, LanguageDescriptor.AllNames.ToList());
                        break;
                    case "stringset":
                        AssertValidSelectString(value, optionFieldName, values);
                        break;
                    case "componentset":
                        break;
                    case "textcomponentset":
                        break;
                }

                OptionDescriptor optionDescriptor = new OptionDescriptor(
                    GetString("option_key_" + index.ToString()),
                    type,
                    GetString("option_label_" + index.ToString()),
                    GetString("option_help_" + index.ToString()),
                    value,
                    values);

                optionDescriptors.Add(optionDescriptor);
            }

            return optionDescriptors;
        }

        public MasterContentItem GetMasterContentItem()
        {
            MasterContentItem contentItem = new MasterContentItem(
                GetString("key"),                                                     // string nodeContentKey,
                GetSelectString("contentType"),                                       // string contentType,
                GetSelectString("contentSubType"),                                    // string contentSubType,
                GetMultiLanguageString("title"),                                      // MultiLanguageString title,
                GetMultiLanguageString("description"),                                // MultiLanguageString description,
                GetString("source"),                                                  // string source,
                GetString("package"),                                                 // string package,
                GetString("label"),                                                   // string label,
                GetString("imageFileName"),                                           // string imageFileName,
                GetInteger("index"),                                                  // int index,
                GetLanguageIDList("targetLanguageIDs"),                               // List<LanguageID> targetLanguageIDs,
                GetLanguageIDList("hostLanguageIDs"),                                 // List<LanguageID> hostLanguageIDs,
                GetString("owner"),                                                   // string owner,
                null,                                                                 // List<MasterContentItem> contentItems,
                GetMarkupReference("markupKey"),                                      // MarkupTemplateReference markupReference,
                GetMarkupReference("copyMarkupKey"),                                  // MarkupTemplateReference copyMarkupReference,
                GetOptionDescriptors());                                              // List<OptionDescriptor> optionDescriptors

            contentItem.Source = ContentUtilities.GetSourceFromContentType(contentItem.ContentType);
            contentItem.Label = ContentUtilities.GetLabelFromContentType(contentItem.ContentType);

            return contentItem;
        }

        public MarkupTemplateReference GetMarkupReference(string name)
        {
            string markupKey = GetString(name);

            if (String.IsNullOrEmpty(markupKey))
                return null;
            else if (ObjectUtilities.IsNumberString(markupKey))
            {
                int key = ObjectUtilities.GetIntegerFromString(markupKey, 0);

                if (key != 0)
                {
                    MarkupTemplate markupTemplate = Repositories.MarkupTemplates.Get(key);

                    if (markupTemplate != null)
                        return new MarkupTemplateReference(markupTemplate);
                }

                return null;
            }
            else
            {
                switch (markupKey)
                {
                    case "(local)":
                        return new MarkupTemplateReference(markupKey, String.Empty, UserRecord.UserName);
                    case "(none)":
                    default:
                        return null;
                }
            }
        }

        public LanguageDescription GetLanguageDescription(string owner)
        {
            LanguageDescription languageDescription = new LanguageDescription();
            languageDescription.Key = GetString("LanguageCode", languageDescription.KeyString);
            languageDescription.LanguageID = LanguageLookup.GetLanguageIDNoAdd(languageDescription.KeyString);
            languageDescription.LanguageName = GetTranslateMultiLanguageString("LanguageName");
            languageDescription.LanguageName.LanguageStrings.Sort(LanguageString.CompareLanguageName);
            languageDescription.CharacterBased = GetFlag("CharacterBased");
            languageDescription.ReadTopToBottom = GetFlag("ReadTopToBottom");
            languageDescription.ReadRightToLeft = GetFlag("ReadRightToLeft");
            languageDescription.PreferedFontName = GetString("PreferedFontName");
            languageDescription.GoogleSpeechToTextSupported = GetFlag("GoogleSpeechToTextSupported");
            languageDescription.GoogleTextToSpeechSupported = GetFlag("GoogleSpeechToTextSupported");
            languageDescription.UseGoogleTextToSpeech = GetFlag("UseGoogleTextToSpeech");
            languageDescription.PreferedVoiceName = GetString("PreferedVoiceName");
            languageDescription.Owner = owner;
            return languageDescription;
        }

        public void UpdateLanguageDescription(LanguageDescription languageDescription)
        {
            languageDescription.Key = GetString("LanguageCode", languageDescription.KeyString);
            languageDescription.LanguageID = LanguageLookup.GetLanguageIDNoAdd(languageDescription.KeyString);
            languageDescription.LanguageName = GetTranslateMultiLanguageString("LanguageName");
            languageDescription.LanguageName.LanguageStrings.Sort(LanguageString.CompareLanguageName);
            languageDescription.CharacterBased = GetFlag("CharacterBased");
            languageDescription.ReadTopToBottom = GetFlag("ReadTopToBottom");
            languageDescription.ReadRightToLeft = GetFlag("ReadRightToLeft");
            languageDescription.PreferedFontName = GetString("PreferedFontName");
            languageDescription.GoogleSpeechToTextSupported = GetFlag("GoogleSpeechToTextSupported");
            languageDescription.GoogleTextToSpeechSupported = GetFlag("GoogleSpeechToTextSupported");
            languageDescription.UseGoogleTextToSpeech = GetFlag("UseGoogleTextToSpeech");
            languageDescription.PreferedVoiceName = GetString("PreferedVoiceName");
        }

        public void GetFormatArgumentsGet(string importExportType, string formatType, string formatTarget, string targetLabel,
            NodeUtilities nodeUtilities,
            BaseObjectTitled treeHeaderSource, BaseObjectNodeTree treeSource,
            BaseObjectNode nodeSource, BaseObjectContent contentSource,
            out string importExportTypeOut, out string formatTypeOut, out Format format)
        {
            if (String.IsNullOrEmpty(formatType))
                formatType = "JTLanguage XML";
            formatTypeOut = formatType;
            format = ApplicationData.Formats.Create(formatType, formatTarget, targetLabel, UserRecord, UserProfile, Repositories,
                LanguageUtilities, nodeUtilities);
            if (format != null)
            {
                format.SetSources(treeHeaderSource, treeSource, nodeSource, contentSource);
                format.LoadFromArguments();
                if (String.IsNullOrEmpty(importExportType))
                    importExportType = UserProfile.GetUserOptionString("ImportExportType_" + format.TargetType, "File");
                UserProfile.SetUserOptionString("ImportExportType_" + format.TargetType, importExportType);
                format.ImportExportType = importExportType;
            }
            importExportTypeOut = importExportType;
        }

        public void GetFormatArgumentsPut(string importExportType, string formatType, string formatLastType,
            string formatTarget, string targetLabel, NodeUtilities nodeUtilities,
            BaseObjectTitled treeHeaderSource, BaseObjectNodeTree treeSource,
            BaseObjectNode nodeSource, BaseObjectContent contentSource,
            out string importExportTypeOut, out string formatTypeOut, out Format format)
        {
            string optionName = "FormatType_" + formatTarget;
            if (String.IsNullOrEmpty(formatType))
                formatType = GetUserOptionString(optionName, "JTLanguage XML");
            else
                SetUserOptionString(optionName, formatType);
            formatTypeOut = formatType;
            format = ApplicationData.Formats.Create(formatType, formatTarget, targetLabel,
                UserRecord, UserProfile, Repositories, LanguageUtilities, nodeUtilities);
            if (format != null)
            {
                format.SetSources(treeHeaderSource, treeSource, nodeSource, contentSource);
                List<string> ignoreList = new List<string>();
                format.PrefetchArguments(this);
                format.SaveToArguments();
                if ((formatType == formatLastType) && (format.Arguments != null))
                {
                    foreach (FormatArgument argument in format.Arguments)
                    {
                        string value = String.Empty;
                        if ((argument.Type == "flaglist") || (argument.Type == "languageflaglist"))
                        {
                            if (argument.Values != null)
                            {
                                Dictionary<string, bool> flagDictionary = new Dictionary<string, bool>();
                                foreach (string flagName in argument.Values.Cast<string>().ToList())
                                {
                                    string name = argument.Name + "_" + flagName;
                                    string stringValue = this[name] as string;
                                    bool flagValue;
                                    if (stringValue == "on")
                                        flagValue = true;
                                    else
                                        flagValue = false;
                                    flagDictionary.Add(flagName, flagValue);
                                }
                                value = TextUtilities.GetStringFromFlagDictionary(flagDictionary);
                            }
                        }
                        else if (argument.Type == "languagelist")
                        {
                            if (!HasLanguageIDList(argument.Name))
                                continue;
                            List<LanguageID> list = GetLanguageIDList(argument.Name);
                            value = TextUtilities.GetStringFromLanguageIDList(list);
                        }
                        else
                        {
                            value = this[argument.Name] as string;
                            if (value == null)
                            {
                                if (argument.Type == "flag")
                                    value = "off";
                                else
                                    continue;
                            }
                            if ((argument.Type == "flag") && argument.HasFlagDependents())
                            {
                                List<string> dependents;
                                if (value == "on")
                                    dependents = argument.FlagOffDependents;
                                else
                                    dependents = argument.FlagOnDependents;
                                if (dependents != null)
                                    ignoreList.AddRange(dependents);
                            }
                        }
                        bool ignoreIt = ignoreList.Contains(argument.Name);
                        if (ignoreIt)
                            continue;
                        argument.Value = value;
                    }
                    format.LoadFromArguments();
                    format.SaveUserOptions();
                }
                if (String.IsNullOrEmpty(importExportType))
                    importExportType = UserProfile.GetUserOptionString("ImportExportType_" + format.TargetType, "File");
                UserProfile.SetUserOptionString("ImportExportType_" + format.TargetType, importExportType);
                format.ImportExportType = importExportType;
            }
            importExportTypeOut = importExportType;
        }

        public void GetFormatArgumentsPutFormat(Format format)
        {
            if (format != null)
            {
                List<string> ignoreList = new List<string>();
                format.PrefetchArguments(this);
                format.SaveToArguments();
                if (format.Arguments != null)
                {
                    foreach (FormatArgument argument in format.Arguments)
                    {
                        string value = String.Empty;
                        if ((argument.Type == "flaglist") || (argument.Type == "languageflaglist"))
                        {
                            if (argument.Values != null)
                            {
                                Dictionary<string, bool> flagDictionary = new Dictionary<string, bool>();
                                foreach (string flagName in argument.Values.Cast<string>().ToList())
                                {
                                    string name = argument.Name + "_" + flagName;
                                    string stringValue = this[name] as string;
                                    bool flagValue;
                                    if (stringValue == "on")
                                        flagValue = true;
                                    else
                                        flagValue = false;
                                    flagDictionary.Add(flagName, flagValue);
                                }
                                value = TextUtilities.GetStringFromFlagDictionary(flagDictionary);
                            }
                        }
                        else if (argument.Type == "languagelist")
                        {
                            if (!HasLanguageIDList(argument.Name))
                                continue;
                            List<LanguageID> list = GetLanguageIDList(argument.Name);
                            value = TextUtilities.GetStringFromLanguageIDList(list);
                        }
                        else
                        {
                            value = this[argument.Name] as string;
                            if (value == null)
                            {
                                if (argument.Type == "flag")
                                    value = "off";
                                else
                                    continue;
                            }
                            if ((argument.Type == "flag") && argument.HasFlagDependents())
                            {
                                List<string> dependents;
                                if (value == "on")
                                    dependents = argument.FlagOffDependents;
                                else
                                    dependents = argument.FlagOnDependents;
                                if (dependents != null)
                                    ignoreList.AddRange(dependents);
                            }
                        }
                        bool ignoreIt = ignoreList.Contains(argument.Name);
                        if (ignoreIt)
                            continue;
                        argument.Value = value;
                    }
                    format.LoadFromArguments();
                    format.SaveUserOptions();
                }
            }
        }

        public void GetFormatArguments(List<FormatArgument> arguments)
        {
            List<string> ignoreList = new List<string>();
            foreach (FormatArgument argument in arguments)
            {
                string value = String.Empty;
                if ((argument.Type == "flaglist") || (argument.Type == "languageflaglist"))
                {
                    if (argument.Values != null)
                    {
                        Dictionary<string, bool> flagDictionary = new Dictionary<string, bool>();
                        foreach (string flagName in argument.Values.Cast<string>().ToList())
                        {
                            string name = argument.Name + "_" + flagName;
                            string stringValue = this[name] as string;
                            bool flagValue;
                            if (stringValue == "on")
                                flagValue = true;
                            else
                                flagValue = false;
                            flagDictionary.Add(flagName, flagValue);
                        }
                        value = TextUtilities.GetStringFromFlagDictionary(flagDictionary);
                    }
                }
                else if (argument.Type == "languagelist")
                {
                    if (!HasLanguageIDList(argument.Name))
                        continue;
                    List<LanguageID> list = GetLanguageIDList(argument.Name);
                    value = TextUtilities.GetStringFromLanguageIDList(list);
                }
                else
                {
                    value = this[argument.Name] as string;
                    if (value == null)
                    {
                        if (argument.Type == "flag")
                            value = "off";
                        else
                            continue;
                    }
                    if ((argument.Type == "flag") && argument.HasFlagDependents())
                    {
                        List<string> dependents;
                        if (value == "on")
                            dependents = argument.FlagOffDependents;
                        else
                            dependents = argument.FlagOnDependents;
                        if (dependents != null)
                            ignoreList.AddRange(dependents);
                    }
                }
                bool ignoreIt = ignoreList.Contains(argument.Name);
                if (ignoreIt)
                    continue;
                argument.Value = value;
            }
        }

        public NodeMaster GetPlanMaster()
        {
            NodeMaster master = null;
            int masterKey = 0;
            string masterKeyString = this["MasterKey"] as string;
            if (!String.IsNullOrEmpty(masterKeyString) && (masterKeyString != "(none)"))
                masterKey = GetInteger("MasterKey");
            if (masterKey != 0)
                master = Repositories.NodeMasters.Get(masterKey);
            return master;
        }

        public BaseObjectNodeTree GetTreeProfile(string source)
        {
            NodeMaster master = null;
            int masterKey = 0;
            string masterKeyString = this["Tree.MasterKey"] as string;
            if (!String.IsNullOrEmpty(masterKeyString) && (masterKeyString != "(none)"))
                masterKey = GetInteger("Tree.MasterKey");
            if (masterKey != 0)
                master = Repositories.NodeMasters.Get(masterKey);
            BaseObjectNodeTree nodeData = new BaseObjectNodeTree(
                0,
                GetMultiLanguageString("Tree.Title"),                         // MultiLanguageString Title
                GetMultiLanguageString("Tree.Description"),                   // MultiLanguageString Description
                source,                                                       // string Source
                GetString("Tree.Package"),                                    // string Package
                GetString("Tree.Label"),                                      // string Label
                GetString("Tree.ImageFileName"),                              // string ImageFileName
                GetInteger("Tree.Index"),                                     // int Index
                GetFlag("Tree.IsPublic"),                                     // bool IsPublic
                GetLanguageIDList("Tree.TargetLanguageIDs"),                  // List<LanguageID> TargetLanguageIDs
                GetLanguageIDList("Tree.HostLanguageIDs"),                    // List<LanguageID> HostLanguageIDs
                GetString("Tree.Owner"),                                      // string Owner
                null,                                                         // List<BaseObjectNode> children
                null,                                                         // List<IBaseObjectKeyed> options
                null,                                                         // MarkupTemplate markupTemplate
                GetMarkupReference("Tree.MarkupTemplateKey"),                 // MarkupTemplateReference markupReference
                null,                                                         // List<BaseObjectContent> contentList
                null,                                                         // List<BaseObjectContent> contentChildren
                master,                                                       // NodeMaster master
                null);                                                        // List<BaseObjectNode> nodes
            nodeData.LevelString = GetString("Tree.Level");
            nodeData.MarkupUse = GetString("Tree.MarkupUse");
            nodeData.Modified = false;
            return nodeData;
        }

        public BaseObjectNode GetNodeProfile(string source)
        {
            NodeMaster master = null;
            int masterKey = 0;
            string masterKeyString = this["Node.MasterKey"] as string;
            if (!String.IsNullOrEmpty(masterKeyString) && (masterKeyString != "(none)"))
                masterKey = GetInteger("Node.MasterKey");
            if (masterKey != 0)
                master = Repositories.NodeMasters.Get(masterKey);
            BaseObjectNode nodeData = new BaseObjectNode(
                0,
                GetMultiLanguageString("Node.Title"),                         // MultiLanguageString Title
                GetMultiLanguageString("Node.Description"),                   // MultiLanguageString Description
                source,                                                       // string Source
                GetString("Node.Package"),                                    // string Package
                GetString("Node.Label"),                                      // string Label
                GetString("Node.ImageFileName"),                              // string ImageFileName
                GetInteger("Node.Index"),                                     // int Index
                GetFlag("Node.IsPublic"),                                     // bool IsPublic
                GetLanguageIDList("Node.TargetLanguageIDs"),                  // List<LanguageID> TargetLanguageIDs
                GetLanguageIDList("Node.HostLanguageIDs"),                    // List<LanguageID> HostLanguageIDs
                GetString("Node.Owner"),                                      // string Owner
                null,                                                         // BaseObjectNodeTree tree
                null,                                                         // BaseObjectNode parent
                null,                                                         // List<BaseObjectNode> children
                null,                                                         // List<IBaseObjectKeyed> options
                null,                                                         // MarkupTemplate markupTemplate
                GetMarkupReference("Node.MarkupTemplateKey"),                 // MarkupTemplateReference markupReference,
                null,                                                         // List<BaseObjectContent> contentList
                null,                                                         // List<BaseObjectContent> contentChildren
                master);                                                      // NodeMaster master
            nodeData.MarkupUse = GetString("Node.MarkupUse");
            nodeData.Modified = false;
            return nodeData;
        }

        public BaseObjectNode GetMultipleNode(BaseObjectNodeTree tree, NodeMaster master, int ordinal, out int outOrdinal)
        {
            LanguageID uiLanguageID = UILanguageID;
            MultiLanguageString title;
            string titleString;
            bool changed = false;

            do
            {
                title = GetMultiLanguageString("Node.Title");
                MergeMultiLanguageStringOrdinal(title, ordinal, true);
                titleString = title.Text(uiLanguageID);
                changed = false;

                if (tree.NodeCount() != 0)
                {
                    foreach (BaseObjectNode testNode in tree.Nodes)
                    {
                        if (testNode.GetTitleString(uiLanguageID) == titleString)
                        {
                            ordinal++;
                            changed = true;
                            break;
                        }
                    }
                }
            }
            while (changed);

            outOrdinal = ordinal;

            MultiLanguageString description = GetMultiLanguageString("Node.Description");
            MergeMultiLanguageStringOrdinal(description, ordinal, false);
            string package = GetString("Node.Package");
            string label = GetString("Node.Label");
            string imageFileName = GetString("Node.ImageFileName");
            int index = GetInteger("Node.Index");
            bool isPublic = GetFlag("Node.IsPublic");
            List<LanguageID> targetLanguageIDs = GetLanguageIDList("Node.TargetLanguageIDs");
            List<LanguageID> hostLanguageIDs = GetLanguageIDList("Node.HostLanguageIDs");
            string owner = GetString("Node.Owner");
            BaseObjectNode nodeData = new BaseObjectNode(
                0,
                title,                                                        // MultiLanguageString Title
                description,                                                  // MultiLanguageString Description
                null,                                                         // string Source
                package,                                                      // string Package
                label,                                                        // string Label
                imageFileName,                                                // string ImageFileName
                index,                                                        // int Index
                isPublic,                                                     // bool IsPublic
                targetLanguageIDs,                                            // List<LanguageID> TargetLanguageIDs
                hostLanguageIDs,                                              // List<LanguageID> HostLanguageIDs
                owner,                                                        // string Owner
                null,                                                         // BaseObjectNodeTree tree
                null,                                                         // BaseObjectNode parent
                null,                                                         // List<BaseObjectNode> children
                null,                                                         // List<IBaseObjectKeyed> options
                null,                                                         // MarkupTemplate markupTemplate
                null,                                                         // MarkupTemplateReference markupReference
                null,                                                         // List<BaseObjectContent> contentList
                null,                                                         // List<BaseObjectContent> contentChildren
                master);                                                      // NodeMaster master
            return nodeData;
        }

        public BaseObjectContent GetContentProfile()
        {
            string contentType = GetString("ContentType");
            string source = ContentUtilities.GetSourceFromContentType(contentType);
            string label = ContentUtilities.GetLabelFromContentType(contentType);
            BaseObjectContent contentData = new BaseObjectContent(
                GetString("Key"),
                GetMultiLanguageString("Title"),                              // MultiLanguageString Title
                GetMultiLanguageString("Description"),                        // MultiLanguageString Description
                GetString("Source"),                                          // string Source
                null,                                                         // string Package
                GetString("Label"),                                           // string Label
                null,                                                         // string ImageFileName
                0,                                                            // int Index
                GetFlag("IsPublic"),                                          // bool IsPublic
                GetLanguageIDList("TargetLanguageIDs"),                       // List<LanguageID> TargetLanguageIDs
                GetLanguageIDList("HostLanguageIDs"),                         // List<LanguageID> HostLanguageIDs
                GetString("Owner"),                                           // string Owner
                null,                                                         // BaseObjectNode node
                GetString("ContentType"),                                     // string contentType
                GetString("ContentSubType"),                                  // string contentSubType
                null,                                                         // BaseContentStorage contentStorage
                null,                                                         // List<IBaseObjectKeyed> options
                null,                                                         // MarkupTemplate markupTemplate
                null,                                                         // MarkupTemplateReference markupReference
                null,                                                         // BaseObjectContent contentParent
                null);                                                        // List<BaseObjectContent> contentChildren
            return contentData;
        }

        public void UpdateContentProfile(BaseObjectContent content)
        {
            string contentKey = GetString("Key");
            BaseObjectNode node = content.Node;

            if (node != null)
            {
                BaseObjectContent priorContent = node.GetContent(contentKey);

                if ((priorContent != null) && (priorContent != content))
                    PutError("Key", S("Content with this key already exists: ") + contentKey);
            }

            content.ContentType = GetString("ContentType");
            content.Source = ContentUtilities.GetSourceFromContentType(content.ContentType);
            content.Label = ContentUtilities.GetLabelFromContentType(content.ContentType);
            content.Key = contentKey;
            content.Title = GetMultiLanguageString("Title");
            content.Description = GetMultiLanguageString("Description");
            content.TargetLanguageIDs = GetLanguageIDList("TargetLanguageIDs");
            content.HostLanguageIDs = GetLanguageIDList("HostLanguageIDs");
            content.Owner = GetString("Owner");
        }

        public void UpdateContentMultipleProfile(BaseObjectContent content, int ordinal)
        {
            content.ContentType = GetString("ContentType");
            content.Source = ContentUtilities.GetSourceFromContentType(content.ContentType);
            content.Label = ContentUtilities.GetLabelFromContentType(content.ContentType);
            content.Key = GetString("Key");
            if (content.KeyString.Contains("#"))
                content.Key = content.KeyString.Replace("#", ordinal.ToString());
            else
                content.Key = content.KeyString + ordinal.ToString();
            content.Title = GetMultiLanguageString("Title");
            MergeMultiLanguageStringOrdinal(content.Title, ordinal, true);
            content.Description = GetMultiLanguageString("Description");
            MergeMultiLanguageStringOrdinal(content.Description, ordinal, false);
            content.TargetLanguageIDs = GetLanguageIDList("TargetLanguageIDs");
            content.HostLanguageIDs = GetLanguageIDList("HostLanguageIDs");
            content.Owner = GetString("Owner");
        }

        public void MergeMultiLanguageStringOrdinal(MultiLanguageString mls, int ordinal, bool doDefault)
        {
            if (mls.Count() == 0)
                return;
            foreach (LanguageString ls in mls.LanguageStrings)
            {
                if (ls.Text.Contains("#"))
                    ls.Text = ls.Text.Replace("#", ordinal.ToString());
                else if (doDefault && !String.IsNullOrEmpty(ls.Text))
                {
                    if (char.IsDigit(ls.Text[ls.Text.Length - 1]))
                    {
                        int num = ObjectUtilities.GetIntegerFromStringEnd(ls.Text, -1);
                        if (num != -1)
                        {
                            num += ordinal - 1;
                            ls.Text = ObjectUtilities.RemoveNumberFromStringEnd(ls.Text) + num.ToString();
                        }
                        else
                            ls.Text = ls.Text + " " + ordinal.ToString();
                    }
                    else
                       ls.Text = ls.Text + " " + ordinal.ToString();
                }
            }
        }

        public void UpdateMediaRun(MediaRun mediaRun)
        {
            mediaRun.Key = GetString("MediaRunType", mediaRun.KeyString);
            bool isReference = GetFlag("MediaRunIsReference");

            if (isReference)
            {
                mediaRun.FileName = String.Empty;
                mediaRun.MediaItemKey = GetString("MediaItemReferenceKey", mediaRun.MediaItemKey);
                mediaRun.LanguageMediaItemKey = GetString("LanguageMediaItemReferenceKey", mediaRun.LanguageMediaItemKey);
            }
            else
            {
                mediaRun.FileName = GetString("MediaRunFileName", mediaRun.FileName);
                if (mediaRun.FileName != null)
                    mediaRun.FileName = mediaRun.FileName.Trim();
                mediaRun.MediaItemKey = String.Empty;
                mediaRun.LanguageMediaItemKey = String.Empty;
            }

            if (mediaRun.KeyString.EndsWith("Picture"))
            {
                mediaRun.Start = TimeSpan.Zero;
                mediaRun.Stop = TimeSpan.Zero;
            }
            else
            {
                TimeSpan start = mediaRun.Start;
                TimeSpan stop = mediaRun.Stop;
                GetTimeStartStop("MediaRunStart", "MediaRunStop", out start, out stop);
                mediaRun.Start = start;
                mediaRun.Stop = stop;
            }
        }

        public Annotation GetAnnotation()
        {
            Annotation annotation = new Annotation();
            annotation.Type = GetString("AnnotationType");
            annotation.Text = UpdateMultiLanguageString("AnnotationText", null);
            annotation.Value = GetString("AnnotationValue");
            return annotation;
        }

        public void UpdateAnnotation(Annotation annotation)
        {
            annotation.Type = GetString("AnnotationType", annotation.Type);
            annotation.Text = UpdateMultiLanguageString("AnnotationText", annotation.Text);
            annotation.Value = GetString("AnnotationValue", annotation.Value);
        }

        public Dictionary<string, bool> GetStringSelectFlags(string name, List<string> strings,
            Dictionary<string, bool> stringSelectFlags)
        {
            int count = strings.Count();
            int index;

            if (stringSelectFlags == null)
                stringSelectFlags = new Dictionary<string, bool>();

            for (index = 0; index < count; index++)
            {
                string str = strings[index];
                string checkboxName = name + index.ToString();
                string checkboxValue = this[checkboxName] as string;
                bool flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                stringSelectFlags.Add(str, flag);
            }

            return stringSelectFlags;
        }

        public static Dictionary<string, bool> ResetStringSelectFlags(string value, List<string> strings,
            Dictionary<string, bool> stringSelectFlags)
        {
            bool flag = false;

            if (stringSelectFlags == null)
                stringSelectFlags = new Dictionary<string, bool>();

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (string str in strings)
                    stringSelectFlags[str] = flag;
            }
            else if (value == "Between")
            {
                int count = strings.Count();
                int index;
                string str;

                for (index = 0; index < count;)
                {
                    // Find selected item.
                    for (; index < count; index++)
                    {
                        str = strings[index];

                        if (stringSelectFlags[str])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (index++; index < count; index++)
                    {
                        str = strings[index];

                        if (!stringSelectFlags[str])
                            stringSelectFlags[str] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; index < count; index++)
                    {
                        str = strings[index];

                        if (!stringSelectFlags[str])
                            break;
                    }
                }
            }

            return stringSelectFlags;
        }

        public Dictionary<int, bool> GetIndexSelectFlags(
            string name,
            int startIndex,
            int endIndex,
            Dictionary<int, bool> indexSelectFlags)
        {
            int count = endIndex - startIndex;
            int index;

            if (indexSelectFlags == null)
                indexSelectFlags = new Dictionary<int, bool>();

            for (index = startIndex; index < endIndex; index++)
            {
                string checkboxName = name + index.ToString();
                string checkboxValue = this[checkboxName] as string;
                bool flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                indexSelectFlags.Add(index, flag);
            }

            return indexSelectFlags;
        }

        public static Dictionary<int, bool> ResetIndexSelectFlags(
            int startIndex,
            int endIndex,
            string value,
            Dictionary<int, bool> indexSelectFlags)
        {
            bool flag = false;

            if (indexSelectFlags == null)
                indexSelectFlags = new Dictionary<int, bool>();

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                for (int index = startIndex; index < endIndex; index++)
                    indexSelectFlags[index] = flag;
            }
            else if (value == "Between")
            {
                int index;

                for (index = startIndex; index < endIndex;)
                {
                    // Find selected item.
                    for (; index < endIndex; index++)
                    {
                        if (indexSelectFlags[index])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (index++; index < endIndex; index++)
                    {
                        if (!indexSelectFlags[index])
                            indexSelectFlags[index] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; index < endIndex; index++)
                    {
                        if (!indexSelectFlags[index])
                            break;
                    }
                }
            }

            return indexSelectFlags;
        }

        public void GetItemSelectFlags(List<MultiLanguageItem> studyItems, Dictionary<string, bool> itemSelectFlags)
        {
            if (studyItems == null)
                return;

            int itemCount = studyItems.Count;
            string checkboxName;
            string checkboxValue;
            bool flag;

            itemSelectFlags.Clear();

            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                MultiLanguageItem studyItem = studyItems[itemIndex];
                if (studyItem == null)
                    continue;
                checkboxName = "item_select_" + itemIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                itemSelectFlags.Add(studyItem.CompoundStudyItemKey, flag);
            }
        }

        public void GetItemSelectFlags(int itemCount, List<bool> itemSelectFlags)
        {
            string checkboxName;
            string checkboxValue;
            bool flag;

            itemSelectFlags.Clear();

            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                checkboxName = "item_select_" + itemIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                itemSelectFlags.Add(flag);
            }
        }

        public void GetItemSelectFlags(List<MultiLanguageItem> studyItems, List<LanguageID> languageIDs,
            string languageTextFormat, string rowTextFormat, List<bool> itemSelectFlags)
        {
            if (studyItems == null)
                return;

            int studyItemCount = studyItems.Count;
            int itemIndex = 0;
            string languageCode;
            string checkboxName;
            string checkboxValue;
            bool flag;

            itemSelectFlags.Clear();

            switch (languageTextFormat)
            {
                default:
                case "Mixed":
                    switch (rowTextFormat)
                    {
                        default:
                        case "Paragraphs":
                            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
                            {
                                checkboxName = "item_select_" + studyItemIndex.ToString();
                                checkboxValue = this[checkboxName] as string;
                                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                                itemSelectFlags.Add(flag);
                            }
                            break;
                        case "Sentences":
                            foreach (MultiLanguageItem studyItem in studyItems)
                            {
                                int sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);
                                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++, itemIndex++)
                                {
                                    checkboxName = "item_select_" + itemIndex.ToString();
                                    checkboxValue = this[checkboxName] as string;
                                    flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                                    itemSelectFlags.Add(flag);
                                }
                            }
                            break;
                    }
                    break;
                case "Separate":
                    switch (rowTextFormat)
                    {
                        default:
                        case "Paragraphs":
                            foreach (LanguageID languageID in languageIDs)
                            {
                                languageCode = languageID.LanguageCultureExtensionCode;
                                for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
                                {
                                    checkboxName = "item_select_" + studyItemIndex.ToString() + "_" + languageCode;
                                    checkboxValue = this[checkboxName] as string;
                                    flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                                    itemSelectFlags.Add(flag);
                                }
                            }
                            break;
                        case "Sentences":
                            foreach (LanguageID languageID in languageIDs)
                            {
                                languageCode = languageID.LanguageCultureExtensionCode;
                                itemIndex = 0;
                                foreach (MultiLanguageItem studyItem in studyItems)
                                {
                                    int sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);
                                    for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++, itemIndex++)
                                    {
                                        checkboxName = "item_select_" + itemIndex.ToString() + "_" + languageCode;
                                        checkboxValue = this[checkboxName] as string;
                                        flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                                        itemSelectFlags.Add(flag);
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        public void ResetItemSelectFlags(int studyItemCount, string value, List<bool> itemSelectFlags)
        {
            bool flag = false;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                itemSelectFlags.Clear();

                for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
                    itemSelectFlags.Add(flag);
            }
            else if (value == "Between")
            {
                int studyItemIndex;

                for (studyItemIndex = 0; studyItemIndex < studyItemCount;)
                {
                    // Find selected item.
                    for (; studyItemIndex < studyItemCount; studyItemIndex++)
                    {
                        if (itemSelectFlags[studyItemIndex])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (studyItemIndex++; studyItemIndex < studyItemCount; studyItemIndex++)
                    {
                        if (!itemSelectFlags[studyItemIndex])
                            itemSelectFlags[studyItemIndex] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; studyItemIndex < studyItemCount; studyItemIndex++)
                    {
                        if (!itemSelectFlags[studyItemIndex])
                            break;
                    }
                }
            }
        }

        public void GetNodeSelectFlags(BaseObjectNode node, Dictionary<int, bool> nodeSelectFlags)
        {
            int childCount = node.ChildCount();
            int childIndex;
            BaseObjectNode childNode;
            int childKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (childIndex = 0; childIndex < childCount; childIndex++)
            {
                childNode = node.GetChildIndexed(childIndex);
                childKey = childNode.KeyInt;
                checkboxName = "item_select_" + childKey.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                nodeSelectFlags[childKey] = flag;
                GetNodeSelectFlags(childNode, nodeSelectFlags);
            }
        }

        public void ResetNodeSelectFlags(BaseObjectNode node, string value, Dictionary<int, bool> nodeSelectFlags)
        {
            bool flag = false;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (KeyValuePair<int, bool> kvp in nodeSelectFlags.ToList())
                    nodeSelectFlags[kvp.Key] = flag;
            }
            else if (value == "Between")
            {
                List<int> nodeKeys = node.GetDecendentKeys(null);
                int nodeCount = nodeKeys.Count();
                int nodeIndex;
                int nodeKey;

                for (nodeIndex = 0; nodeIndex < nodeCount;)
                {
                    // Find selected item.
                    for (; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (nodeSelectFlags[nodeKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (nodeIndex++; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (!nodeSelectFlags[nodeKey])
                            nodeSelectFlags[nodeKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (!nodeSelectFlags[nodeKey])
                            break;
                    }
                }
            }
        }

        public void GetContentSelectFlags(BaseObjectNode node, Dictionary<string, bool> contentSelectFlags)
        {
            int contentCount = node.ContentCount();
            int contentIndex;
            BaseObjectContent content;
            string contentKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                content = node.GetContentIndexed(contentIndex);
                contentKey = content.KeyString;
                checkboxName = "content_select_" + contentIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                contentSelectFlags[contentKey] = flag;
            }
        }

        public void ResetContentSelectFlags(BaseObjectNode node, string value, Dictionary<string, bool> contentSelectFlags)
        {
            bool flag = false;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (string contentKey in node.ContentKeysList)
                    contentSelectFlags[contentKey] = flag;
            }
            else if (value == "Between")
            {
                List<BaseObjectContent> contentList = node.ContentList;
                List<string> contentKeys = new List<string>();
                int contentCount = contentList.Count();
                int contentIndex;
                string contentKey;

                // Get keys in displayed order, media first, then others.

                for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
                {
                    BaseObjectContent content = contentList[contentIndex];

                    if (content.HasContentParent())
                    {
                        if (contentList.FirstOrDefault(x => x.MatchKey(content.ContentParentKey)) != null)
                        {
                            continue;
                        }
                    }

                    WalkContent(contentKeys, content);
                }

                for (contentIndex = 0; contentIndex < contentCount;)
                {
                    // Find selected item.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (contentSelectFlags[contentKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (contentIndex++; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            contentSelectFlags[contentKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            break;
                    }
                }
            }
        }

        public void GetSubContentSelectFlags(BaseObjectContent content, Dictionary<string, bool> contentSelectFlags)
        {
            int contentCount = content.ContentCount();
            int contentIndex;
            BaseObjectContent subContent;
            string contentKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                subContent = content.GetContentIndexed(contentIndex);
                contentKey = subContent.KeyString;
                checkboxName = "content_select_" + contentIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                contentSelectFlags[contentKey] = flag;
            }
        }

        public void ResetSubContentSelectFlags(BaseObjectContent content, string value, Dictionary<string, bool> contentSelectFlags)
        {
            bool flag = false;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (string contentKey in content.ContentKeysList)
                    contentSelectFlags[contentKey] = flag;
            }
            else if (value == "Between")
            {
                List<BaseObjectContent> contentList = content.ContentList;
                List<string> contentKeys = new List<string>();
                int contentCount = contentList.Count();
                int contentIndex;
                string contentKey;

                // Get keys in displayed order, media first, then others.

                for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
                {
                    BaseObjectContent subContent = contentList[contentIndex];

                    if (subContent.HasContentParent())
                    {
                        if (contentList.FirstOrDefault(x => x.MatchKey(subContent.ContentParentKey)) != null)
                        {
                            continue;
                        }
                    }

                    WalkContent(contentKeys, subContent);
                }

                for (contentIndex = 0; contentIndex < contentCount;)
                {
                    // Find selected item.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (contentSelectFlags[contentKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (contentIndex++; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            contentSelectFlags[contentKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            break;
                    }
                }
            }
        }

        private void WalkContent(List<string> contentKeys, BaseObjectContent content)
        {
            if (contentKeys == null)
                return;

            contentKeys.Add(content.KeyString);

            if (content.ContentCount() != 0)
            {
                foreach (BaseObjectContent subContent in content.ContentList)
                {
                    if (subContent.KeyString == content.KeyString)
                        continue;
                    WalkContent(contentKeys, subContent);
                }
            }
        }

        public void GetNodeSelectFlags(List<int> nodeKeys, Dictionary<int, bool> nodeSelectFlags)
        {
            if (nodeKeys == null)
                return;

            int nodeCount = nodeKeys.Count();
            int nodeIndex;
            int nodeKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                nodeKey = nodeKeys[nodeIndex];
                checkboxName = "item_select_" + nodeKey.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                nodeSelectFlags[nodeKey] = flag;
            }
        }

        public void ResetNodeSelectFlags(List<int> nodeKeys, string value, Dictionary<int, bool> nodeSelectFlags)
        {
            bool flag = false;

            if (nodeKeys == null)
                return;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (KeyValuePair<int, bool> kvp in nodeSelectFlags.ToList())
                    nodeSelectFlags[kvp.Key] = flag;
            }
            else if (value == "Between")
            {
                int nodeCount = nodeKeys.Count();
                int nodeIndex;
                int nodeKey;

                for (nodeIndex = 0; nodeIndex < nodeCount;)
                {
                    // Find selected item.
                    for (; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (nodeSelectFlags[nodeKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (nodeIndex++; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (!nodeSelectFlags[nodeKey])
                            nodeSelectFlags[nodeKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; nodeIndex < nodeCount; nodeIndex++)
                    {
                        nodeKey = nodeKeys[nodeIndex];

                        if (!nodeSelectFlags[nodeKey])
                            break;
                    }
                }
            }
        }

        public void GetContentSelectFlags(List<string> contentKeys, Dictionary<string, bool> contentSelectFlags)
        {
            if (contentKeys == null)
                return;

            int contentCount = contentKeys.Count();
            int contentIndex;
            string contentKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                contentKey = contentKeys[contentIndex];
                checkboxName = "content_select_" + contentIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                contentSelectFlags[contentKey] = flag;
            }
        }

        public void GetMediaSelectFlags(
            List<BaseObjectContent> contents,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            ref Dictionary<string, bool> contentSelectFlags)
        {
            if (contents == null)
                return;

            if (contentSelectFlags == null)
                contentSelectFlags = new Dictionary<string, bool>();

            int contentCount = contents.Count();
            int contentIndex;
            BaseObjectContent content;
            ContentMediaItem mediaItem;
            List<LanguageMediaItem> languageMediaItems;
            int mediaIndex = 0;
            MediaDescription mediaDescription;
            string mediaDirectory;
            string filePath;
            bool flag;
            bool testFlag;
            string checkboxName;
            string checkboxValue;

            for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                content = contents[contentIndex];
                if (content == null)
                    continue;
                mediaItem = content.ContentStorageMediaItem;
                if (mediaItem == null)
                    continue;
                languageMediaItems = mediaItem.GetLanguageMediaItemsWithLanguages(targetLanguageIDs, hostLanguageIDs);
                if ((languageMediaItems == null) || (languageMediaItems.Count() == 0))
                    continue;
                mediaDirectory = content.MediaTildeUrl;
                foreach (LanguageMediaItem languageMediaItem in languageMediaItems)
                {
                    mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(0);
                    if (mediaDescription == null)
                        continue;
                    filePath = mediaDescription.GetDirectoryPath(mediaDirectory);
                    checkboxName = "content_select_" + mediaIndex.ToString();
                    checkboxValue = this[checkboxName] as string;
                    flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                    if (!contentSelectFlags.TryGetValue(filePath, out testFlag))
                        contentSelectFlags.Add(filePath, flag);
                    else
                        contentSelectFlags[filePath] = flag;
                    mediaIndex++;
                }
            }
        }

        public static void ResetContentSelectFlags(List<string> contentKeys, string value, Dictionary<string, bool> contentSelectFlags)
        {
            bool flag = false;

            if (contentKeys == null)
                return;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (string contentKey in contentKeys)
                    contentSelectFlags[contentKey] = flag;
            }
            else if (value == "Between")
            {
                int contentCount = contentKeys.Count();
                int contentIndex;
                string contentKey;

                for (contentIndex = 0; contentIndex < contentCount;)
                {
                    // Find selected item.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (contentSelectFlags[contentKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (contentIndex++; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            contentSelectFlags[contentKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; contentIndex < contentCount; contentIndex++)
                    {
                        contentKey = contentKeys[contentIndex];

                        if (!contentSelectFlags[contentKey])
                            break;
                    }
                }
            }
        }

        public void GetLanguageSelectFlags(List<LanguageID> languageIDs,
            Dictionary<string, bool> languageSelectFlags)
        {
            int languageCount = languageIDs.Count();
            int languageIndex;
            LanguageID languageID;
            string languageKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                languageID = languageIDs[languageIndex];
                languageKey = languageID.LanguageCultureExtensionCode;
                checkboxName = "languageSelect_" + languageIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                languageSelectFlags[languageKey] = flag;
            }
        }

        public void GetLanguageSelectFlags(List<LanguageDescriptor> languageDescriptors,
            Dictionary<string, bool> languageSelectFlags)
        {
            int languageCount = languageDescriptors.Count();
            int languageIndex;
            LanguageDescriptor languageDescriptor;
            string languageKey;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                languageDescriptor = languageDescriptors[languageIndex];
                languageKey = languageDescriptor.LanguageID.LanguageCultureExtensionCode;
                checkboxName = "languageSelect_" + languageIndex.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);
                languageSelectFlags[languageKey] = flag;
            }
        }

        public void ResetLanguageSelectFlags(List<LanguageDescriptor> languageDescriptors,
            string value, Dictionary<string, bool> languageSelectFlags)
        {
            bool flag = false;

            if ((value == "All") || (value == "None"))
            {
                if (value == "All")
                    flag = true;

                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                    languageSelectFlags[languageDescriptor.LanguageID.LanguageCultureExtensionCode] = flag;
            }
            else if (value == "Between")
            {
                int languageCount = languageDescriptors.Count();
                int languageIndex;
                string languageKey;

                for (languageIndex = 0; languageIndex < languageCount;)
                {
                    // Find selected item.
                    for (; languageIndex < languageCount; languageIndex++)
                    {
                        languageKey = languageDescriptors[languageIndex].LanguageID.LanguageCultureExtensionCode;

                        if (languageSelectFlags[languageKey])
                            break;
                    }

                    // Select items inbetween selected items.
                    for (languageIndex++; languageIndex < languageCount; languageIndex++)
                    {
                        languageKey = languageDescriptors[languageIndex].LanguageID.LanguageCultureExtensionCode;

                        if (!languageSelectFlags[languageKey])
                            languageSelectFlags[languageKey] = true;
                        else
                            break;
                    }

                    // Skip selected items.
                    for (; languageIndex < languageCount; languageIndex++)
                    {
                        languageKey = languageDescriptors[languageIndex].LanguageID.LanguageCultureExtensionCode;

                        if (!languageSelectFlags[languageKey])
                            break;
                    }
                }
            }
        }

        public List<UserRecord> GetSelectedUserRecords(string name, List<UserRecord> userRecords)
        {
            List<UserRecord> selectedUserRecords = new List<UserRecord>();

            if (userRecords == null)
                return selectedUserRecords;

            int count = userRecords.Count();
            int index;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (index = 0; index < count; index++)
            {
                checkboxName = name + index.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);

                if (flag)
                    selectedUserRecords.Add(userRecords[index]);
            }

            return selectedUserRecords;
        }

        public Dictionary<string, bool> GetSelectedUserRecordFlags(string name, List<UserRecord> userRecords)
        {
            Dictionary<string, bool> selectedUserRecordFlags = new Dictionary<string, bool>();

            if (userRecords == null)
                return selectedUserRecordFlags;

            int count = userRecords.Count();
            int index;
            string checkboxName;
            string checkboxValue;
            bool flag;

            for (index = 0; index < count; index++)
            {
                UserRecord userRecord = userRecords[index];
                checkboxName = name + index.ToString();
                checkboxValue = this[checkboxName] as string;
                flag = ((checkboxValue == "on") || (checkboxValue == "true") ? true : false);

                try
                {
                    selectedUserRecordFlags.Add(userRecord.UserName, flag);
                }
                catch (Exception)
                {
                }
            }

            return selectedUserRecordFlags;
        }

        public ToolProfile GetToolProfile(out string profileName, out string oldProfileName,
            out string copyProfileKey)
        {
            ToolProfile toolProfile = new ToolProfile();
            oldProfileName = GetString("oldProfileName");
            profileName = GetString("profileName");
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", profileName, UserProfile);
            string descriptionString = GetString("description");
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", descriptionString, UserProfile);
            toolProfile.Title = title;
            toolProfile.Description = description;
            toolProfile.TargetLanguageIDs = GetLanguageIDList("targetLanguageIDs");
            toolProfile.HostLanguageIDs = GetLanguageIDList("hostLanguageIDs");
            toolProfile.Index = GetInteger("Index", 0);
            toolProfile.SelectorAlgorithm = ToolProfile.GetSelectorAlgorithmCodeFromString(GetString("selectorAlgorithm"));
            // This should be after SelectorAlgorithm.
            toolProfile.GradeCount = GetInteger("gradeCount", ToolProfile.DefaultGradeCount);
            toolProfile.NewLimit = GetInteger("newLimit");
            toolProfile.ReviewLimit = GetInteger("reviewLimit");
            toolProfile.LimitExpiration = GetTimeOffset("limitExpiration");
            toolProfile.IsRandomUnique = GetFlag("isRandomUnique");
            toolProfile.IsRandomNew = GetFlag("isRandomNew");
            toolProfile.IsAdaptiveMixNew = GetFlag("isAdaptiveMixNew");
            toolProfile.ReviewLevel = GetInteger("reviewLevel");
            toolProfile.StageResetGradeThreshold = GetFloat("stageResetGradeThreshold", ToolProfile.DefaultStageResetGradeThreshold);
            toolProfile.StageIncrementGradeThreshold = GetFloat("stageIncrementGradeThreshold", ToolProfile.DefaultStageIncrementGradeThreshold);
            toolProfile.HighGradeMultiplier = GetFloat("highGradeMultiplier", ToolProfile.DefaultHighGradeMultiplier);
            toolProfile.EasyGradeMultiplier = GetFloat("easyGradeMultiplier", ToolProfile.DefaultEasyGradeMultiplier);
            toolProfile.HardGradeMultiplier = GetFloat("hardGradeMultiplier", ToolProfile.DefaultHardGradeMultiplier);
            toolProfile.SampleSize = GetInteger("sampleSize");
            toolProfile.ChunkSize = GetInteger("chunkSize");
            toolProfile.ChoiceSize = GetInteger("choiceSize");
            toolProfile.HistorySize = GetInteger("historySize");
            toolProfile.IsShowIndex = GetFlag("isShowIndex");
            toolProfile.IsShowOrdinal = GetFlag("isShowOrdinal");
            toolProfile.SpacedIntervalTable = GetTimeSpanList("spacedIntervalTable", ToolProfile.DefaultSpacedIntervalTable);
            toolProfile.FontFamily = GetString("fontFamily");
            toolProfile.FlashFontSize = GetString("flashFontSize");
            toolProfile.ListFontSize = GetString("listFontSize");
            toolProfile.MaximumLineLength = GetInteger("maximumLineLength");
            // Do this before grade button labels and tips.
            toolProfile.ComputeTable();
            toolProfile.SetGradeButtonLabelsTranslated(GetStringList("gradeButtonLabels"), LanguageUtilities);
            toolProfile.SetGradeButtonTipsTranslated(GetStringList("gradeButtonTips"), LanguageUtilities);
            toolProfile.ToolConfigurations = new List<ToolConfiguration>();
            copyProfileKey = GetString("copyProfile");
            return toolProfile;
        }

        public ToolConfiguration GetToolConfiguration(string configurationKey)
        {
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", GetString("Title"), UserProfile);
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", GetString("Description"), UserProfile);
            string label = GetString("Label");
            string newKey = GetString("NewKey");
            int cardSideCount = GetInteger("CardSideCount");
            List<ToolSide> cardSides = new List<ToolSide>(cardSideCount);
            Assert(label != "(select)", "Label", "A label must be selected.");
            Assert((cardSideCount >= 1) && (cardSideCount <= 5), "CardSideCount", "Card side count must be from 1 to 5.");
            for (int sideIndex = 1; sideIndex <= cardSideCount; sideIndex++)
            {
                ToolSide cardSide = GetToolSide(sideIndex);
                cardSides.Add(cardSide);
            }
            ToolConfiguration configuration = new ToolConfiguration(
                null,                                                               // ToolProfile toolProfile,
                newKey,                                                             // string key,
                title,                                                              // MultiLanguageString Title
                description,                                                        // MultiLanguageString Description
                label,                                                              // string label
                GetInteger("Index", 0),                              // int Index
                cardSides);                                                         // List<ToolSide> cardSides
            configuration.SubConfigurationKeys = GetToolSubConfigurationKeys();
            return configuration;
        }

        public ToolSide GetToolSide(int sideIndex)
        {
            ToolSide toolSide = new ToolSide(sideIndex, null);
            string name = "Side" + sideIndex.ToString() + ".";
            toolSide.HasTextOutput = GetFlag(name + "HasTextOutput");
            toolSide.HasPictureOutput = GetFlag(name + "HasPictureOutput");
            toolSide.HasAudioOutput = GetFlag(name + "HasAudioOutput");
            toolSide.HasVideoOutput = GetFlag(name + "HasVideoOutput");
            toolSide.HasTextInput = GetFlag(name + "HasTextInput");
            toolSide.HasAudioInput = GetFlag(name + "HasAudioInput");
            toolSide.HasVoiceRecognition = GetFlag(name + "HasVoiceRecognition");
            toolSide.HasDescrambleInput = GetFlag(name + "HasDescrambleInput");
            toolSide.HasChoiceInput = GetFlag(name + "HasChoiceInput");
            toolSide.HasBlanksInput = GetFlag(name + "HasBlanksInput");
            toolSide.TextLanguageIDs = GetLanguageIDCheckboxes(name + "TextLanguageIDs");
            toolSide.MediaLanguageIDs = GetLanguageIDCheckboxes(name + "MediaLanguageIDs");
            string writeLanguageIDName;
            if (toolSide.HasDescrambleInput)
                writeLanguageIDName = "DescrambleLanguageIDs";
            else if (toolSide.HasChoiceInput)
                writeLanguageIDName = "ChoiceLanguageIDs";
            else if (toolSide.HasBlanksInput)
                writeLanguageIDName = "BlanksLanguageIDs";
            else
                writeLanguageIDName = "WriteLanguageIDs";
            toolSide.WriteLanguageIDs = GetLanguageIDCheckboxes(name + writeLanguageIDName);
            toolSide.TextFormat = GetString(name + "TextFormat");
            toolSide.FontFamily = GetString(name + "FontFamily");
            if (toolSide.FontFamily == "(select)")
                toolSide.FontFamily = null;
            toolSide.FlashFontSize = GetString(name + "FlashFontSize");
            if (toolSide.FlashFontSize == "(select)")
                toolSide.FlashFontSize = null;
            toolSide.ListFontSize = GetString(name + "ListFontSize");
            if (toolSide.ListFontSize == "(select)")
                toolSide.ListFontSize = null;
            if (this[name + "MaximumLineLength"] != null)
                toolSide.MaximumLineLength = GetInteger(name + "MaximumLineLength");
            else
                toolSide.MaximumLineLength = 0;
            return toolSide;
        }

        public ToolSelectorMode GetToolSelectorMode(string name)
        {
            string mode = GetString(name);
            ToolSelectorMode toolSelectorMode = ToolItemSelector.GetToolSelectorModeFromString(mode);
            return toolSelectorMode;
        }

        public ToolResponseCommand GetToolResponseCommand(string name, out string grade)
        {
            string response = GetString(name);
            ToolResponseCommand toolResponseCommand = GetToolResponseCommandFromString(response, out grade);
            return toolResponseCommand;
        }

        public ToolResponseCommand GetToolResponseCommandFromString(string response, out string grade)
        {
            ToolResponseCommand toolResponseCommand = ToolResponseCommand.Unknown;

            grade = null;

            if (!String.IsNullOrEmpty(response))
            {
                if (response.StartsWith("<"))
                    toolResponseCommand = ToolResponseCommand.Back;
                else if (response.EndsWith(">"))
                    toolResponseCommand = ToolResponseCommand.Forward;
                else
                {
                    switch (response)
                    {
                        case "Check":
                            toolResponseCommand = ToolResponseCommand.Check;
                            break;
                        case "Retry":
                            toolResponseCommand = ToolResponseCommand.Retry;
                            break;
                        case "Skip":
                            toolResponseCommand = ToolResponseCommand.Skip;
                            break;
                        case "Learned":
                            toolResponseCommand = ToolResponseCommand.MarkLearned;
                            break;
                        case "Reset":
                            toolResponseCommand = ToolResponseCommand.Reset;
                            break;
                        case "Forget":
                            toolResponseCommand = ToolResponseCommand.Forget;
                            break;
                        case "New":
                            toolResponseCommand = ToolResponseCommand.Forget;
                            break;
                        case "Forget All":
                            toolResponseCommand = ToolResponseCommand.ForgetAll;
                            break;
                        case "Forget Learned":
                            toolResponseCommand = ToolResponseCommand.ForgetLearned;
                            break;
                        case "Learned All":
                            toolResponseCommand = ToolResponseCommand.LearnedAll;
                            break;
                        case "Learned New":
                            toolResponseCommand = ToolResponseCommand.LearnedNew;
                            break;
                        case "Edit":
                            toolResponseCommand = ToolResponseCommand.Edit;
                            break;
                        case "Recreate":
                            toolResponseCommand = ToolResponseCommand.Recreate;
                            break;
                        case "New Session":
                            toolResponseCommand = ToolResponseCommand.NewSession;
                            break;
                        case "SetPause":
                            toolResponseCommand = ToolResponseCommand.SetPause;
                            break;
                        case "SetText":
                            toolResponseCommand = ToolResponseCommand.SetText;
                            break;
                        case "DefinitionAction":
                            toolResponseCommand = ToolResponseCommand.DefinitionAction;
                            break;
                        case "Delete Phrases":
                            toolResponseCommand = ToolResponseCommand.DeletePhrases;
                            break;
                        case "EditStudyList":
                            toolResponseCommand = ToolResponseCommand.EditStudyList;
                            break;
                        case "EditSentenceRuns":
                            toolResponseCommand = ToolResponseCommand.EditSentenceRuns;
                            break;
                        case "EditWordRuns":
                            toolResponseCommand = ToolResponseCommand.EditWordRuns;
                            break;
                        case "SetStudyItems":
                            toolResponseCommand = ToolResponseCommand.SetStudyItems;
                            break;
                        case "SetStage":
                            toolResponseCommand = ToolResponseCommand.SetStage;
                            break;
                        case "SetNextReviewTime":
                            toolResponseCommand = ToolResponseCommand.SetNextReviewTime;
                            break;
                        default:
                            grade = response;
                            toolResponseCommand = ToolResponseCommand.Score;
                            break;
                    }
                }
            }

            return toolResponseCommand;
        }

        public List<string> GetToolSubConfigurationKeys()
        {
            int count = GetInteger("SubConfigurationCount", 0);
            int index;
            List<string> subConfigurationKeys = null;

            for (index = 0; index < count; index++)
            {
                string flagName = "subconfiguration_" + index.ToString();
                string keyName = "subconfiguration_" + index.ToString() + "_key";
                string configurationKey = GetString(keyName);
                if (String.IsNullOrEmpty(configurationKey))
                    continue;
                bool isChecked = GetFlag(flagName, false);
                if (!isChecked)
                    continue;
                if (subConfigurationKeys == null)
                    subConfigurationKeys = new List<string>();
                subConfigurationKeys.Add(configurationKey);
            }

            return subConfigurationKeys;
        }

        public void UpdateForumCategory(ForumCategory obj, string name)
        {
            UpdateTranslateMultiLanguageString(name + ".Title", obj.Title);
            obj.Owner = GetString(name + ".Owner");
            obj.Index = GetInteger(name + ".Index");
        }

        public void UpdateForumHeading(ForumHeading obj, string name)
        {
            UpdateTranslateMultiLanguageString(name + ".Title", obj.Title);
            UpdateTranslateMultiLanguageString(name + ".Description", obj.Description);
            obj.Owner = GetString(name + ".Owner");
            string categoryKeyPair = GetString(name + ".Category");
            if (AssertSelectNotEmpty(categoryKeyPair, name + ".Category"))
            {
                string typeName;
                string keyString;
                ObjectUtilities.ParseTypeAndKeyString(categoryKeyPair, out typeName, out keyString);
                obj.CategoryKey = ObjectUtilities.GetIntegerFromString(keyString, 0);
            }
            obj.Index = GetInteger(name + ".Index");
        }

        public void UpdateForumTopic(ForumTopic obj, string name)
        {
            UpdateTranslateMultiLanguageString(name + ".Title", obj.Title);
            obj.Owner = GetString(name + ".Owner");
            obj.Index = GetInteger(name + ".Index");
        }

        public void UpdateForumPosting(ForumPosting obj, string name)
        {
            UpdateTranslateMultiLanguageString(name + ".Text", obj.Text);
            obj.Owner = GetString(name + ".Owner");
            obj.Index = GetInteger(name + ".Index");
            obj.Level = GetInteger(name + ".Level");
            obj.PostingParentKey = GetInteger(name + ".PostingParentKey");
        }
    }
}

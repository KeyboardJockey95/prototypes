using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Markup
{
    public class MarkupRendererContent : MarkupRenderer
    {
        public MarkupRendererContent(BaseObjectContent documentContent, BaseObjectNodeTree sourceTree, BaseObjectNode sourceNode,
                List<BaseObjectContent> sourceContents, MarkupTemplate markupTemplate, UserRecord userRecord, UserProfile userProfile,
                LanguageID targetLanguageID, LanguageID hostLanguageID, LanguageUtilities languageUtilities, IMainRepository repositories,
                bool useAudio, bool usePicture) :
            base(documentContent, sourceTree, sourceNode, sourceContents, markupTemplate, userRecord, userProfile,
                targetLanguageID, hostLanguageID, languageUtilities, repositories, useAudio, usePicture)
        {
        }

        protected override XNode HandleContent(string referenceString, ContentRenderParameters parameters)
        {
            BaseObjectNode node = parameters.Node;
            BaseObjectContent content = parameters.Content;
            string contentKey = parameters.ContentKey;
            ContentStudyList studyList = parameters.StudyList;
            int studyItemIndex = parameters.StudyItemIndex;
            int studyItemCount = parameters.StudyItemCount;
            int sentenceIndex = parameters.SentenceIndex;
            LanguageID languageID = parameters.ItemLanguageID;
            string subType = parameters.SubType;
            string subSubType = parameters.SubSubType;
            bool hasOptions = parameters.HasOptions;
            BaseMarkupContainer sourceTitledObject = null;
            BaseObjectKeyed sourceObject = null;
            MultiLanguageString multiLanguageString = null;
            MultiLanguageItem studyItem = null;
            LanguageItem languageItem = null;
            TextRun sentenceRun = null;
            string singleString = null;

            if ((languageID != null) && (languageID == LanguageLookup.Any))
                languageID = null;

            if (subType == null)
                subType = String.Empty;

            if (subSubType == null)
                subSubType = String.Empty;

            if ((content == null) && !String.IsNullOrEmpty(contentKey))
            {
                if (node != null)
                {
                    parameters.Content = content = Content = Node.GetContent(contentKey);

                    if ((content == null) && Node.HasContentWithType(contentKey))
                        parameters.Content = content = Content = Node.GetContentWithType(contentKey).FirstOrDefault();

                    if (content != null)
                    {
                        if (studyList == null)
                            parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                    }
                    else
                    {
                        switch (contentKey)
                        {
                            case "Tree":
                            case "Course":
                            case "Plan":
                                sourceObject = sourceTitledObject = SourceTree;
                                break;
                            case "Node":
                            case "Lesson":
                                sourceObject = sourceTitledObject = node;
                                break;
                            case "Document":
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                            case "Option":
                                subSubType = subType;
                                subType = "Option";
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                            default:
                                if (String.IsNullOrEmpty(subType))
                                    subType = contentKey;
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                        }
                    }
                }
            }

            if (studyList == null)
            {
                if (content != null)
                    parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                else
                    parameters.StudyList = studyList = StudyList = null;
            }

            if (sourceObject == null)
            {
                if (studyList != null)
                {
                    sourceTitledObject = content;
                    sourceObject = studyList;
                }
                else if (content != null)
                    sourceObject = sourceTitledObject = content;
                else if (DocumentContent != null)
                    sourceObject = sourceTitledObject = DocumentContent;
                else if (node != null)
                    sourceObject = sourceTitledObject = node;
                else
                    sourceObject = sourceTitledObject = SourceTree;
            }

            if (sourceObject != null)
            {
                switch (subType.ToLower())
                {
                    case null:
                    case "":
                    case "text":
                    case "speakername":
                    case "speakerindex":
                    case "languageid":
                    case "languagecode":
                    case "language":
                    case "languagename":
                        if (studyList != null)
                        {
                            if (!hasOptions && (studyItemCount == 1))
                            {
                                studyItem = StudyList.GetStudyItemIndexed(studyItemIndex);

                                if (studyItem == null)
                                {
                                    List<MultiLanguageItem> studyItems = StudyList.StudyItemsRecurse;
                                    if ((studyItemIndex >= 0) && (studyItems != null) && (studyItemIndex < studyItems.Count()))
                                        studyItem = studyItems[studyItemIndex];
                                }
                                if (studyItem != null)
                                {
                                    switch (subType.ToLower())
                                    {
                                        case null:
                                        case "":
                                        case "text":
                                            if (languageID != null)
                                            {
                                                languageItem = studyItem.LanguageItem(languageID);

                                                if (languageItem != null)
                                                {
                                                    if (sentenceIndex == -1)
                                                        singleString = languageItem.Text;
                                                    else
                                                        singleString = languageItem.GetRunText(sentenceIndex);
                                                }
                                            }
                                            else
                                                return RenderContentView(parameters);
                                            break;
                                        case "speakername":
                                            if (languageID == null)
                                                languageID = UILanguageID;
                                            singleString = studyList.GetSpeakerNameText(studyItem.SpeakerNameKey, languageID);
                                            break;
                                        case "speakerindex":
                                            singleString = StudyList.GetSpeakerNameIndex(studyItem.SpeakerNameKey).ToString();
                                            break;
                                        case "languageid":
                                        case "languagecode":
                                            if (languageID != null)
                                                singleString = languageID.LanguageCultureExtensionCode;
                                            else
                                                singleString = "(my languages)";
                                            break;
                                        case "language":
                                        case "languagename":
                                            if (languageID != null)
                                                singleString = languageID.LanguageName(UILanguageID);
                                            else
                                                singleString = "(my languages)";
                                            break;
                                        case "count":
                                            if (studyItemIndex == -1)
                                                singleString = studyList.StudyItemCount().ToString();
                                            else if (languageID == null)
                                                singleString = studyItem.Count().ToString();
                                            else
                                            {
                                                languageItem = studyItem.LanguageItem(languageID);
                                                if (languageItem != null)
                                                    singleString = languageItem.SentenceRunCount().ToString();
                                                else
                                                    singleString = "0";
                                            }
                                            break;
                                        case "index":
                                            if (studyItemIndex == -1)
                                                singleString = sourceTitledObject.Index.ToString();
                                            else if (languageID == null)
                                                singleString = studyItem.Count().ToString();
                                            else
                                                singleString = studyItemIndex.ToString();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    singleString = "(Invalid study item index.)";
                            }
                            else
                                return RenderContentView(parameters);
                        }
                        else
                        {
                            switch (subType.ToLower())
                            {
                                case null:
                                case "":
                                case "text":
                                    return RenderContentView(parameters);
                                case "speakername":
                                    break;
                                case "speakerindex":
                                    break;
                                case "languageid":
                                case "languagecode":
                                    if (languageID != null)
                                        singleString = languageID.LanguageCultureExtensionCode;
                                    else if (sourceTitledObject.TargetLanguageIDs != null)
                                        singleString = TextUtilities.GetStringFromLanguageIDList(sourceTitledObject.TargetLanguageIDs);
                                    else
                                        singleString = "(my languages)";
                                    break;
                                case "language":
                                case "languagename":
                                    if (languageID != null)
                                        singleString = languageID.LanguageName(UILanguageID);
                                    else if (sourceTitledObject.TargetLanguageIDs != null)
                                        singleString = LanguageID.ConvertLanguageIDListToNameString(sourceTitledObject.TargetLanguageIDs, UILanguageID);
                                    else
                                        singleString = "(my languages)";
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "count":
                        if (studyList != null)
                        {
                            if (studyItemIndex == -1)
                                singleString = studyList.StudyItemCount().ToString();
                            else if (languageID == null)
                                singleString = studyItem.Count().ToString();
                            else
                            {
                                languageItem = studyItem.LanguageItem(languageID);
                                if (languageItem != null)
                                    singleString = languageItem.SentenceRunCount().ToString();
                                else
                                    singleString = "0";
                            }
                        }
                        else
                            singleString = "0";
                        break;
                    case "index":
                        if (studyItemIndex == -1)
                            singleString = sourceTitledObject.Index.ToString();
                        else if ((studyList != null) && (languageID == null))
                            singleString = studyItem.Count().ToString();
                        else
                            singleString = studyItemIndex.ToString();
                        break;
                    case "media":
                    case "audio":
                    case "video":
                    case "picture":
                        if ((studyList != null) && (studyItemCount == 1))
                        {
                            studyItem = studyList.GetStudyItemIndexedRecursed(studyItemIndex);

                            if ((studyItem != null) && (languageID != null))
                            {
                                languageItem = studyItem.LanguageItem(languageID);

                                if (languageItem != null)
                                {
                                    if (sentenceIndex != -1)
                                        sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                                }
                            }
                            parameters.StudyItem = studyItem;
                            parameters.LanguageItem = languageItem;
                            parameters.SentenceRun = sentenceRun;
                            switch (subType.ToLower())
                            {
                                case "media":
                                    parameters.UseAudio = true;
                                    parameters.UsePicture = true;
                                    break;
                                case "audio":
                                    parameters.UseAudio = true;
                                    break;
                                case "video":
                                    parameters.UseAudio = true;
                                    break;
                                case "picture":
                                    parameters.UsePicture = true;
                                    break;
                                default:
                                    break;
                            }
                            return RenderStudyItemMediaView(parameters);
                        }
                        else
                            singleString = "(rendered content): " + S("Can only use this sub-type with one item") + ": " + subType;
                        break;
                    case "key":
                        singleString = sourceObject.KeyString;
                        break;
                    case "title":
                        multiLanguageString = sourceTitledObject.Title;
                        break;
                    case "description":
                        multiLanguageString = sourceTitledObject.Description;
                        break;
                    case "label":
                        singleString = sourceTitledObject.Label;
                        break;
                    case "owner":
                        singleString = sourceTitledObject.Owner;
                        break;
                    case "ispublic":
                        singleString = (sourceTitledObject.IsPublic ? "true" : "false");
                        break;
                    case "package":
                        singleString = sourceTitledObject.Package;
                        break;
                    case "creationtime":
                        if (UserRecord != null)
                            singleString = UserRecord.GetLocalTime(sourceObject.CreationTime).ToString();
                        else
                            singleString = sourceObject.CreationTime.ToString();
                        break;
                    case "modifiedtime":
                        if (UserRecord != null)
                            singleString = UserRecord.GetLocalTime(sourceObject.ModifiedTime).ToString();
                        else
                            singleString = sourceObject.ModifiedTime.ToString();
                        break;
                    case "option":
                        if (!String.IsNullOrEmpty(subSubType))
                        {
                            singleString = sourceTitledObject.GetOptionString(subSubType);
                            if (singleString == null)
                                singleString = S("Option not found") + ": " + subSubType;
                        }
                        else
                            singleString = S("Need option name subSubType.");
                        break;
                    case "image":
                        if (node != null)
                        {
                            string imageFileUrl = node.ImageFileTildeUrl;
                            if (!String.IsNullOrEmpty(imageFileUrl))
                                    return RenderImage(imageFileUrl);
                        }
                        return RenderImage(null);
                    default:
                        singleString = "(rendered content): " + S("Unknown sub-type") + ": " + subType;
                        break;
                }

                if (multiLanguageString != null)
                {
                    if (languageID != null)
                        singleString = multiLanguageString.Text(languageID);
                    else if (multiLanguageString.LanguageString(UILanguageID) != null)
                        singleString = multiLanguageString.Text(UILanguageID);
                    else if (multiLanguageString.Count() != 0)
                        singleString = multiLanguageString.Text(0);
                }
            }
            else
                singleString = referenceString;

            return RunWrapString(singleString, parameters);
        }

        protected virtual XNode RenderContentView(ContentRenderParameters parameters)
        {
            return HandleElementError("Not implemented: RenderContentView");
        }

        protected override XNode HandleMediaContent(string referenceString, ContentRenderParameters parameters)
        {
            BaseObjectNode node = parameters.Node;
            MarkupTemplate markupTemplate = parameters.MarkupTemplate;
            BaseObjectContent content = parameters.Content;
            string contentKey = parameters.ContentKey;
            ContentStudyList studyList = parameters.StudyList;
            int studyItemIndex = parameters.StudyItemIndex;
            int studyItemCount = parameters.StudyItemCount;
            int sentenceIndex = parameters.SentenceIndex;
            LanguageID languageID = parameters.ItemLanguageID;
            string subType = parameters.SubType;
            string subSubType = parameters.SubSubType;
            bool hasOptions = parameters.HasOptions;
            BaseMarkupContainer sourceTitledObject = null;
            BaseObjectKeyed sourceObject = null;
            MultiLanguageItem studyItem = null;
            LanguageItem languageItem = null;
            TextRun sentenceRun = null;

            if ((languageID != null) && (languageID == LanguageLookup.Any))
                languageID = null;

            if (subType == null)
                subType = String.Empty;

            if (subSubType == null)
                subSubType = String.Empty;

            if ((content == null) && !String.IsNullOrEmpty(contentKey))
            {
                if (node != null)
                {
                    parameters.Content = content = Content = Node.GetContent(contentKey);

                    if ((content == null) && Node.HasContentWithType(contentKey))
                        parameters.Content = content = Content = Node.GetContentWithType(contentKey).FirstOrDefault();

                    if (content != null)
                    {
                        if (studyList == null)
                            parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                    }
                    else
                    {
                        switch (contentKey)
                        {
                            case "Tree":
                            case "Course":
                            case "Plan":
                                sourceObject = sourceTitledObject = SourceTree;
                                break;
                            case "Node":
                            case "Lesson":
                                sourceObject = sourceTitledObject = node;
                                break;
                            case "Document":
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                            case "Option":
                                subSubType = subType;
                                subType = "Option";
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                            default:
                                if (String.IsNullOrEmpty(subType))
                                    subType = contentKey;
                                sourceObject = sourceTitledObject = DocumentContent;
                                break;
                        }
                    }
                }
            }

            if (studyList == null)
            {
                if (content != null)
                    parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                else
                    parameters.StudyList = studyList = StudyList = null;
            }

            if (sourceObject == null)
            {
                if (studyList != null)
                {
                    sourceTitledObject = content;
                    sourceObject = studyList;
                }
                else if (content != null)
                    sourceObject = sourceTitledObject = content;
                else if (DocumentContent != null)
                    sourceObject = sourceTitledObject = DocumentContent;
                else if (node != null)
                    sourceObject = sourceTitledObject = node;
                else
                    sourceObject = sourceTitledObject = SourceTree;
            }

            if (sourceObject != null)
            {
                switch (subType.ToLower())
                {
                    case null:
                    case "":
                    case "languageid":
                    case "languagecode":
                    case "language":
                    case "languagename":
                        if (studyList != null)
                        {
                            if (!hasOptions && (studyItemCount == 1))
                            {
                                studyItem = StudyList.GetStudyItemIndexed(studyItemIndex);

                                if (studyItem == null)
                                {
                                    List<MultiLanguageItem> studyItems = StudyList.StudyItemsRecurse;
                                    if ((studyItemIndex >= 0) && (studyItems != null) && (studyItemIndex < studyItems.Count()))
                                        studyItem = studyItems[studyItemIndex];
                                }
                                if (studyItem != null)
                                {
                                    parameters.StudyItem = studyItem;
                                    return RenderStudyItemMediaView(parameters);
                                }
                            }
                            else
                                return RenderMediaView(parameters);
                        }
                        else
                            return RenderMediaView(parameters);
                        break;
                    case "media":
                    case "audio":
                    case "video":
                    case "picture":
                        if ((studyList != null) && (studyItemCount == 1))
                        {
                            studyItem = studyList.GetStudyItemIndexedRecursed(studyItemIndex);

                            if ((studyItem != null) && (languageID != null))
                            {
                                languageItem = studyItem.LanguageItem(languageID);

                                if (languageItem != null)
                                {
                                    if (sentenceIndex != -1)
                                        sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                                }
                            }
                            parameters.StudyItem = studyItem;
                            parameters.LanguageItem = languageItem;
                            parameters.SentenceRun = sentenceRun;
                            switch (subType.ToLower())
                            {
                                case "media":
                                    parameters.UseAudio = true;
                                    parameters.UsePicture = true;
                                    break;
                                case "audio":
                                    parameters.UseAudio = true;
                                    break;
                                case "video":
                                    parameters.UseAudio = true;
                                    break;
                                case "picture":
                                    parameters.UsePicture = true;
                                    break;
                                default:
                                    break;
                            }
                            return RenderStudyItemMediaView(parameters);
                        }
                        break;
                    case "image":
                        if (node != null)
                        {
                            string imageFileUrl = node.ImageFileTildeUrl;
                            if (!String.IsNullOrEmpty(imageFileUrl))
                                return RenderImage(imageFileUrl);
                        }
                        return RenderImage(null);
                    default:
                        break;
                }
            }

            return RunWrapString("(nothing to render)", parameters);
        }

        protected virtual XNode RenderStudyItemMediaView(ContentRenderParameters parameters)
        {
            return HandleElementError("Not implemented: RenderStudyItemMediaView");
        }

        protected virtual XNode RenderImage(string imagePath)
        {
            string url = MediaUtilities.GetContentUrl(MediaUtilities.ImageMissingCheck(imagePath));
            XElement element = new XElement("img");
            element.Add(new XAttribute("src", url));
            return element;
        }

        protected virtual XNode RenderMediaView(ContentRenderParameters parameters)
        {
            return HandleElementError("Not implemented: RenderMediaView");
        }

        protected override XNode HandleStudyContent(string referenceString, ContentRenderParameters parameters)
        {
            BaseObjectNode node = parameters.Node;
            MarkupTemplate markupTemplate = parameters.MarkupTemplate;
            BaseObjectContent content = parameters.Content;
            string contentKey = parameters.ContentKey;
            ContentStudyList studyList = parameters.StudyList;
            int studyItemIndex = parameters.StudyItemIndex;
            int studyItemCount = parameters.StudyItemCount;
            int sentenceIndex = parameters.SentenceIndex;
            LanguageID languageID = parameters.ItemLanguageID;
            bool hasOptions = parameters.HasOptions;

            if ((languageID != null) && (languageID == LanguageLookup.Any))
                languageID = null;

            if (String.IsNullOrEmpty(contentKey))
                contentKey = "Words";

            if (content == null)
            {
                if (node != null)
                {
                    parameters.Content = content = Content = Node.GetContent(contentKey);

                    if ((content == null) && Node.HasContentWithType(contentKey))
                        parameters.Content = content = Content = Node.GetContentWithType(contentKey).FirstOrDefault();

                    if (content != null)
                    {
                        if (studyList == null)
                            parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                    }
                    else
                        return RunWrapString("(study needs content)", parameters);
                }
                else
                    return RunWrapString("(study needs content)", parameters);
            }

            if (studyList == null)
            {
                if (content != null)
                    parameters.StudyList = studyList = StudyList = content.GetContentStorageTyped<ContentStudyList>();
                else
                    parameters.StudyList = studyList = StudyList = null;
            }

            return RenderStudyView(parameters);
        }

        protected virtual XNode RenderStudyView(ContentRenderParameters parameters)
        {
            return HandleElementError("Not implemented: RenderStudyView");
        }

        protected override XNode HandleStudyItemContent(string referenceString, ContentRenderParameters parameters)
        {
            if (parameters.StudyItem == null)
                return RunWrapString("(no study item)", parameters);

            return RenderStudyItemView(parameters);
        }

        protected virtual XNode RenderStudyItemView(ContentRenderParameters parameters)
        {
            return HandleElementError("Not implemented: RenderStudyView");
        }

        protected XNode RunWrapString(string singleString, ContentRenderParameters parameters)
        {
            if (singleString == null)
                singleString = String.Empty;

            XNode textNode = new XText(singleString);
            return RunWrapTextNode(textNode, parameters);
        }

        protected XNode RunWrapTextNode(XNode textNode, ContentRenderParameters parameters)
        {
            XNode topElement;
            XElement textElement;
            XElement tr;
            string elementType = parameters.ElementType;
            string style = parameters.StyleAttribute;
            string className = parameters.ClassAttribute;
            XAttribute classAttribute;
            XAttribute styleAttribute;

            switch (elementType.ToLower())
            {
                case "span":
                    topElement = textElement = new XElement("span", textNode);
                    break;
                case "div":
                    topElement = textElement = new XElement("div", textNode);
                    break;
                case "td":
                    topElement = textElement = new XElement("td", textNode);
                    break;
                case "tr":
                    textElement = new XElement("td", textNode);
                    topElement = new XElement("tr", textElement);
                    break;
                case "table":
                    {
                        textElement = new XElement("td", textNode);
                        tr = new XElement("tr", textElement);
                        topElement = new XElement("table", tr);
                    }
                    break;
                case "":
                case null:
                default:
                    if (!String.IsNullOrEmpty(className) || !String.IsNullOrEmpty(style))
                        topElement = textElement = new XElement("span", textNode);
                    else
                        return textNode;
                    break;
            }

            if (!String.IsNullOrEmpty(className))
            {
                classAttribute = new XAttribute("class", className);
                textElement.Add(classAttribute);
            }

            if (!String.IsNullOrEmpty(style))
            {
                styleAttribute = new XAttribute("style", style);
                textElement.Add(styleAttribute);
            }

            return topElement;
        }
    }
}

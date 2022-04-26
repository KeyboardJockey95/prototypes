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
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Markup
{
    public class MarkupRenderer : ControllerUtilities
    {
        public BaseObjectContent DocumentContent;
        public BaseObjectNodeTree SourceTree;
        public BaseObjectNode SourceNode;
        public List<BaseObjectContent> SourceContents;
        public MarkupTemplate MarkupTemplate;
        public List<LanguageDescriptor> LanguageDescriptors;
        public LanguageID TargetLanguageID;
        public LanguageID HostLanguageID;
        public bool UseAudio;
        public bool UsePicture;
        protected XElement RenderedElement;
        protected BaseObjectNode Node;
        protected BaseObjectContent Content;
        protected ContentStudyList StudyList;
        protected MultiLanguageItem StudyItem;
        protected List<BaseString> Variables;
        public List<string> Markers;
        public bool IsPreviewGenerate;
        public bool InGenerate;
        public bool InChoice;
        protected ToolUtilities ToolUtilities;
        protected ContentStudyList WorkingSetStudyList;
        protected ToolStudyList WorkingSet;
        public ToolSession Session;
        protected string DefaultProfileName;
        protected string DefaultConfigurationName;
        protected ToolConfiguration Configuration;
        protected ToolItemSelector Selector;
        protected LanguageID CurrentHostLanguageID;
        protected LanguageID CurrentTargetLanguageID;
        protected DateTime StartTime;
        protected DateTime CurrentTime;
        protected TimeSpan LastItemTime;
        protected List<string> Scripts;
        protected int PlayerIDOrdinal;
        protected int ChooseIDOrdinal;
        protected int ChoiceIDOrdinal;

        public MarkupRenderer(BaseObjectContent documentContent, BaseObjectNodeTree sourceTree, BaseObjectNode sourceNode,
                List<BaseObjectContent> sourceContents, MarkupTemplate markupTemplate,
                UserRecord userRecord, UserProfile userProfile,
                LanguageID targetLanguageID, LanguageID hostLanguageID,
                LanguageUtilities languageUtilities, IMainRepository repositories,
                bool useAudio, bool usePicture)
            : base(repositories, null, userRecord, userProfile, null, languageUtilities)
        {
            DocumentContent = documentContent;
            SourceTree = sourceTree;
            SourceNode = sourceNode;
            SourceContents = sourceContents;
            MarkupTemplate = markupTemplate;
            TargetLanguageID = targetLanguageID;
            HostLanguageID = hostLanguageID;
            LanguageDescriptors = UserProfile.LanguageDescriptors;
            if ((DocumentContent != null) && (DocumentContent.ContentClass == ContentClassType.MediaItem))
                FilterLanguageDescriptors();
            UseAudio = useAudio;
            UsePicture = usePicture;
            RenderedElement = new XElement("div");
            Node = sourceNode;
            if ((SourceContents != null) && (SourceContents.Count() != 0))
            {
                Content = SourceContents[0];
                StudyList = Content.GetContentStorageTyped<ContentStudyList>();
            }
            StudyItem = null;
            Variables = new List<BaseString>();
            Markers = new List<string>();
            IsPreviewGenerate = false;
            InGenerate = false;
            InChoice = false;
            ToolUtilities = new ToolUtilities(
                Repositories, Cookies, UserRecord, UserProfile, Translator, LanguageUtilities, SourceTree, null);
            WorkingSetStudyList = null;
            WorkingSet = null;
            Session = null;
            DefaultConfigurationName = "Read0";
            DefaultProfileName = "Markup";
            Configuration = null;
            Selector = null;
            CurrentHostLanguageID = null;
            CurrentTargetLanguageID = null;
            StartTime = DateTime.MinValue;
            CurrentTime = DateTime.MinValue;
            LastItemTime = TimeSpan.Zero;
            Scripts = null;
            PlayerIDOrdinal = 0;
            ChooseIDOrdinal = 0;
            ChoiceIDOrdinal = 0;
        }

        protected void FilterLanguageDescriptors()
        {
            List<LanguageDescriptor> documentLanguageDescriptors = DocumentContent.LanguageDescriptors;
            List<LanguageDescriptor> profileLanguageDescriptors = UserProfile.LanguageDescriptors;
            List<LanguageDescriptor> languageDescriptors =
                LanguageDescriptor.GetLanguageDescriptorListIntersection(profileLanguageDescriptors, documentLanguageDescriptors);
            if (languageDescriptors.FirstOrDefault(x => (x.Name == "Target")) == null)
            {
                if (TargetLanguageID != null)
                    languageDescriptors.Insert(0, new LanguageDescriptor("Target", TargetLanguageID, true, true));
            }
            if (languageDescriptors.FirstOrDefault(x => (x.Name == "Host")) == null)
            {
                if (HostLanguageID != null)
                   languageDescriptors.Add(new LanguageDescriptor("Host", HostLanguageID, true, true));
            }
            UserProfile.UpdateShowStates(DocumentContent.ContentType, languageDescriptors);
            LanguageDescriptors = languageDescriptors;
        }

        public virtual String RenderGeneratePreviewString()
        {
            IsPreviewGenerate = true;
            string html = RenderElement().ToString();
            html = RestoreScripts(html);
            IsPreviewGenerate = false;
            return html;
        }

        public virtual String RenderString()
        {
            string html = RenderElement().ToString();
            html = RestoreScripts(html);
            return html;
        }

        public virtual String RenderElementString(string elementName, string name,
            int nth, bool childrenOnly)
        {
            XElement renderedElement = RenderElement(elementName, name, nth, childrenOnly);
            string html;

            if (renderedElement != null)
            {
                html = renderedElement.ToString();
                html = RestoreScripts(html);
            }
            else
                html = "(No markup.)";

            return html;
        }

        public virtual String RenderBodyString()
        {
            XElement renderedMarkup = RenderElement();
            XElement bodyElement = null;

            foreach (XElement childElement in renderedMarkup.Elements())
            {
                if (childElement.Name.LocalName == "body")
                {
                    bodyElement = childElement;
                    break;
                }
            }

            if (bodyElement == null)
                bodyElement = renderedMarkup;

            StringBuilder sb = new StringBuilder();

            foreach (var child in bodyElement.Nodes())
            {
                sb.Append(child.ToString());
                sb.Append("\n");
            }

            string markupText = sb.ToString();

            markupText = RestoreScripts(markupText);

            return markupText;
        }

        public virtual XElement RenderElement()
        {
            if ((MarkupTemplate == null) || (MarkupTemplate.Markup == null))
            {
                RenderedElement = new XElement("div");
                return RenderedElement;
            }
            else
                return RenderElement(MarkupTemplate.Markup);
        }

        public virtual XElement RenderElement(string elementName, string name,
            int nth, bool childrenOnly)
        {
            XElement element = MarkupTemplate.FindMarkupElement(elementName, name, nth);

            if (element == null)
            {
                RenderedElement = null;
                return RenderedElement;
            }

            if (childrenOnly)
                return RenderElementChildren(element);
            else
                return RenderElement(element);
        }

        public virtual XElement RenderElement(XElement element)
        {
            PlayerIDOrdinal = 0;
            ChooseIDOrdinal = 0;
            ChoiceIDOrdinal = 0;
            Markers.Clear();

            if (element == null)
            {
                RenderedElement = null;
                return RenderedElement;
            }

            RenderedElement = new XElement(element);
            HandleElement(RenderedElement);

            return RenderedElement;
        }

        public virtual XElement RenderElementChildren(XElement element)
        {
            PlayerIDOrdinal = 0;
            ChooseIDOrdinal = 0;
            ChoiceIDOrdinal = 0;
            Markers.Clear();

            if (element == null)
            {
                RenderedElement = null;
                return RenderedElement;
            }

            RenderedElement = new XElement("div", element.Nodes());
            HandleElement(RenderedElement);

            return RenderedElement;
        }

        protected string RestoreScripts(string html)
        {
            int startIndex = 0;
            int length = html.Length;
            int scriptIndex = 0;

            if ((Scripts == null) || (Scripts.Count == 0))
                return html;

            for (;;)
            {
                startIndex = html.IndexOf("<script", startIndex, length - startIndex);

                if (startIndex < 0)
                    break;

                int tagEnd = html.IndexOf(">", startIndex, length - startIndex);
                tagEnd++;

                int endIndex = html.IndexOf("</script>", tagEnd, length - tagEnd);

                if (endIndex < 0)
                    break;

                if (scriptIndex >= Scripts.Count)
                    break;

                string script = Scripts[scriptIndex++];

                html = html.Insert(tagEnd, script);
                startIndex = tagEnd + script.Length + 9;
                length = html.Length;
            }

            startIndex = 0;

            for (;;)
            {
                startIndex = html.IndexOf("href=\"__href__\"", startIndex, length - startIndex);

                if (startIndex < 0)
                    break;

                startIndex += 6;

                if (scriptIndex >= Scripts.Count)
                    break;

                string href = Scripts[scriptIndex++];

                html = html.Remove(startIndex, 8);
                html = html.Insert(startIndex, href);
                startIndex = startIndex + href.Length + 1;
                length = html.Length;
            }

            html = html.Replace("__nbsp__", "&nbsp;");

            return html;
        }

        // Returns true if node replaced.
        protected virtual bool HandleElement(XElement element)
        {
            bool nodeWasReplaced = false;

            switch (element.Name.LocalName.ToLower())
            {
                case "markup":
                    nodeWasReplaced = HandleMarkupTop(element);
                    break;
                case "insert":
                    nodeWasReplaced = HandleInsert(element);
                    break;
                case "if":
                    nodeWasReplaced = HandleIf(element);
                    break;
                case "foreach":
                    nodeWasReplaced = HandleForEach(element);
                    break;
                case "for":
                    nodeWasReplaced = HandleFor(element);
                    break;
                case "loop":
                    nodeWasReplaced = HandleLoop(element);
                    break;
                case "generate":
                    nodeWasReplaced = HandleGenerate(element);
                    break;
                case "say":
                    nodeWasReplaced = HandleSay(element);
                    break;
                case "pause":
                    nodeWasReplaced = HandlePause(element);
                    break;
                case "item":
                    nodeWasReplaced = HandleItem(element);
                    break;
                case "each":
                    nodeWasReplaced = HandleEach(element);
                    break;
                case "duration":
                    nodeWasReplaced = HandleDuration(element);
                    break;
                case "forget":
                    nodeWasReplaced = HandleForget(element);
                    break;
                case "choose":
                    nodeWasReplaced = HandleChoose(element);
                    break;
                case "choice":
                    nodeWasReplaced = HandleChoice(element);
                    break;
                case "view":
                    nodeWasReplaced = HandleView(element);
                    break;
                case "show":
                    nodeWasReplaced = HandleShow(element);
                    break;
                case "hide":
                    nodeWasReplaced = HandleHide(element);
                    break;
                case "play":
                    nodeWasReplaced = HandlePlay(element);
                    break;
                case "study":
                    nodeWasReplaced = HandleStudy(element);
                    break;
                case "studyitem":
                    nodeWasReplaced = HandleStudyItem(element);
                    break;
                case "marker":
                    HandleMarker(element);
                    break;
                case "goto":
                    HandleGoTo(element);
                    break;
                case "style":
                    break;
                default:
                    if (!HandleExtensionMarkupElement(element, out nodeWasReplaced))
                    {
                        if (HandleElementChildren(element))
                            nodeWasReplaced = true;
                    }
                    break;
            }

            return nodeWasReplaced;
        }

        protected virtual bool HandleElementChildren(XElement element)
        {
            IEnumerable<XNode> nodes = element.Nodes();
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
                HandleMarkupAttribute(attribute);

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count(); i++)
                {
                    XNode node = nodes.ElementAt(i);

                    if (HandleNode(node))
                    {
                        nodes = element.Nodes();
                        nodeWasReplaced = true;
                    }
                }
            }

            return nodeWasReplaced;
        }

        public virtual bool HandleNode(XNode node)
        {
            XElement subElement;
            bool nodeWasReplaced = false;

            switch (node.GetType().Name)
            {
                case "XText":
                    nodeWasReplaced = HandleTextNode(node as XText);
                    break;
                case "XElement":
                    subElement = node as XElement;
                    nodeWasReplaced = HandleElement(subElement);
                    break;
                default:
                    break;
            }

            return nodeWasReplaced;
        }

        protected virtual void HandleMarkupAttribute(XAttribute attribute)
        {
            string value = attribute.Value;
            string newValue = EvaluateAttributeExpression(value);

            if (newValue != value)
                attribute.SetValue(newValue);
        }

        public static char[] newlines = { '\r', '\n' };

        // Returns true if node was replaced.
        protected virtual bool HandleTextNode(XText node)
        {
            string input = node.Value;
            List<XNode> results = new List<XNode>();
            XNode subNode = null;
            LanguageDescriptor hostLanguageDescriptor = (LanguageDescriptors != null ? LanguageDescriptors.FirstOrDefault(x => x.Name == "Host") : null);
            LanguageID hostLanguageID = UILanguageID;
            bool nodeWasReplaced = false;

            if (hostLanguageDescriptor != null)
                hostLanguageID = hostLanguageDescriptor.LanguageID;

            if (!input.Contains("$(") && !input.Contains("{"))
            {
                string generateInput;

                if (InGenerate && !String.IsNullOrEmpty(generateInput = input.Trim()))
                {
                    if (generateInput.Contains("\n"))
                    {
                        string[] parts = generateInput.Split(newlines, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string part in parts)
                            results.Add(new XElement("div", part));

                        node.ReplaceWith(results);
                        nodeWasReplaced = true;

                        UpdateCurrentTime(results, UILanguageID);
                    }
                }

                return nodeWasReplaced;
            }

            int count = input.Length;
            int index;
            int eindex;
            StringBuilder sb = new StringBuilder();
            string fullReferenceString;
            string referenceString;
            string variableName = null;
            string contentKey = null;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            string tag = String.Empty;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            MultiLanguageItem multiLanguageItem;
            LanguageItem languageItem;
            LanguageID languageID = TargetLanguageID;
            LanguageID mediaLanguageID = null;
            List<BaseString> optionsList = null;
            string text;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCultureExtensionCode))
                languageID = hostLanguageID;

            for (index = 0; index < count; index++)
            {
                if ((input[index] == '\\') && ((input[index + 1] == '{') || ((input[index + 1] == '$') && (input[index + 2] == '('))))
                {
                    sb.Append(input[index + 1]);

                    if (input[index + 2] == '(')
                    {
                        sb.Append('(');
                        index += 2;
                    }
                    else
                        index++;
                }
                else if (input[index] == '{')
                {
                    int i = index + 1;
                    int e = i;

                    while (e < count)
                    {
                        if (input[e] == '}')
                        {
                            text = variableName = input.Substring(i, e - i);
                            variableName = MarkupTemplate.FilterVariableName(variableName);
                            multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                            if (multiLanguageItem != null)
                            {
                                languageItem = multiLanguageItem.LanguageItem(UILanguageID);

                                if (languageItem != null)
                                    text = languageItem.Text;
                                else if (multiLanguageItem.LanguageItems != null)
                                    text = multiLanguageItem.LanguageItem(0).Text;
                            }
                            else
                            {
                                BaseString variable = GetVariable(variableName);

                                if (variable != null)
                                    text = variable.Text;
                            }

                            if (text != null)
                                sb.Append(text);

                            break;
                        }

                        e++;
                    }

                    if (variableName == null)
                        sb.Append("(unmatched '}')");

                    index = e;
                }
                else if ((input[index] == '$') && (index < (count - 1)) && (input[index + 1] == '('))
                {
                    for (eindex = index + 2; eindex < count; eindex++)
                    {
                        if (input[eindex] == '(')
                        {
                            for (eindex = eindex + 1; eindex < count; eindex++)
                            {
                                if (input[eindex] == ')')
                                    break;
                            }
                        }
                        else if (input[eindex] == ')')
                        {
                            fullReferenceString = input.Substring(index, eindex - index + 1);
                            referenceString = input.Substring(index + 2, eindex - (index + 2));

                            ParseReference(referenceString, null,
                                out variableName, out contentKey, out itemIndex, out itemCount, out sentenceIndex, out tag,
                                out subType, out subSubType, out languageID, out optionsList, out errorMessage);

                            if (errorMessage != null)
                                sb.Append("(Error parsing reference: \"" + errorMessage + "\")");
                            else if (contentKey != null)
                            {
                                if (!SubstituteKeyWord(contentKey, itemIndex, itemCount, sentenceIndex, subType, languageID, hostLanguageID, optionsList, sb))
                                {
                                    ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                                        SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                                        languageID, mediaLanguageID, subType, subSubType, optionsList,
                                        UseAudio, UsePicture, "1", false, "Stop", String.Empty);

                                    subNode = HandleContent(fullReferenceString, contentRenderParameters);

                                    if (subNode != null)
                                    {
                                        string prefix = sb.ToString();

                                        if (!String.IsNullOrEmpty(prefix))
                                        {
                                            XElement prefixNode = new XElement("div", prefix);
                                            results.Add(prefixNode);
                                            sb = new StringBuilder();
                                        }

                                        results.Add(subNode);
                                    }
                                    else
                                        sb.Append("(error rendering content \"" + contentKey + "\" from: " + referenceString + "\")");
                                }
                            }
                            else if (variableName != null)
                            {
                                multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                                if (multiLanguageItem != null)
                                {
                                    languageItem = multiLanguageItem.LanguageItem(languageID);

                                    if (languageItem != null)
                                        text = languageItem.Text;
                                    else if (multiLanguageItem.LanguageItems != null)
                                        text = "(no translation: \"" + multiLanguageItem.LanguageItem(0).Text + "\")";
                                    else
                                        text = "(no translation)";
                                }
                                else
                                {
                                    BaseString variable = GetVariable(variableName);

                                    if (variable != null)
                                        text = variable.Text;
                                    else
                                    {
                                        //multiLanguageItem = new MultiLanguageItem(variableName, hostLanguageID, variableName);
                                        //Add(multiLanguageItem);
                                        text = variableName;
                                    }
                                }

                                if (text != null)
                                    sb.Append(text);
                            }

                            index = eindex;
                            break;
                        }
                    }
                }
                else
                    sb.Append(input[index]);
            }

            if (results.Count() != 0)
            {
                string postfix = sb.ToString();

                if (!String.IsNullOrEmpty(postfix))
                    results.Add(new XElement("div", postfix));

                node.ReplaceWith(results);
                nodeWasReplaced = true;
            }
            else
            {
                string inputText = sb.ToString();
                string generateInput;

                if (InGenerate && !String.IsNullOrEmpty(generateInput = inputText.Trim()))
                {
                    if (generateInput.Contains("\n"))
                    {
                        string[] parts = generateInput.Split(newlines, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string part in parts)
                            results.Add(new XElement("div", part));

                        node.ReplaceWith(results);
                        nodeWasReplaced = true;
                    }
                    else
                        node.Value = inputText;
                }
                else
                    node.Value = inputText;
            }

            return nodeWasReplaced;
        }

        // Return true if node replaced.
        protected virtual bool HandleMarkupTop(XElement element)
        {
            bool nodeWasReplaced = false;

            element.Name = "div";

            IEnumerable<XNode> nodes = element.Nodes();

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count(); i++)
                {
                    XNode node = nodes.ElementAt(i);

                    if (HandleNode(node))
                    {
                        nodes = element.Nodes();
                        nodeWasReplaced = true;
                    }
                }

                if (!String.IsNullOrEmpty(Error))
                    element.Add(HandleElementError(Error));

                if (!String.IsNullOrEmpty(Message))
                    element.Add(new XElement("div", Message));
            }

            return nodeWasReplaced;
        }

        protected virtual bool HandleInsert(XElement element)
        {
            LanguageID overriddenLanguageID = null;
            LanguageID mediaLanguageID = null;
            MultiLanguageItem variableValue = null;
            string contentKey = null;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            string tag = String.Empty;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "component":  // Legacy
                        ParseContent(attributeValue, overriddenLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out overriddenLanguageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                    case "subcomponent":  // Legacy
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "source":
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out overriddenLanguageID);
                        break;
                    case "medialanguage":
                        ParseMediaLanguageID(attributeValue, out mediaLanguageID);
                        break;
                    case "studyitemindex":
                    case "itemindex":
                    case "index":   // Legacy
                        ParseInteger(attributeValue, out itemIndex);
                        if ((itemIndex != -1) && (itemCount == -1))
                            itemCount = 1;
                        break;
                    case "itemcount":
                        ParseInteger(attributeValue, out itemCount);
                        break;
                    case "sentenceindex":
                        ParseInteger(attributeValue, out sentenceIndex);
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "name":
                        ParseVariableName(attributeValue, out variableValue);
                        break;
                    case "options":
                        optionsList = BaseString.GetBaseStringListFromString(attributeValue);
                        break;
                    case "elementtype":
                    case "class":
                    case "style":
                    case "languageformat":
                    case "rowformat":
                    case "displayformat":
                    case "showannotations":
                    case "useaudio":
                    case "usepicture":
                    case "usemedia":
                    case "autoplay":
                    case "player":
                    case "noplayer":
                    case "playerid":
                    case "showvolume":
                    case "showtimeslider":
                    case "showtimetext":
                    case "endofmedia":
                    default:
                        if (!ParseRenderOption(attribute.Name.LocalName, attributeValue, ref optionsList))
                        {
                            HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                            return false;
                        }
                        break;
                }
            }

            if (errorMessage != null)
            {
                XText textNode = new XText("(" + errorMessage + ")");
                element.ReplaceWith(textNode);
            }
            else if (variableValue != null)
            {
                string text;

                if ((overriddenLanguageID != null) || String.IsNullOrEmpty(overriddenLanguageID.LanguageCultureExtensionCode))
                {
                    if (variableValue.LanguageItems != null)
                    {
                        List<XNode> substitutions = new List<XNode>(variableValue.Count() * 2);

                        foreach (LanguageItem languageItem in variableValue.LanguageItems)
                        {
                            substitutions.Add(new XElement("div", languageItem.Text));
                            substitutions.Add(new XText("\r\n"));
                        }

                        element.ReplaceWith(substitutions.ToArray());
                    }
                    else
                        element.Remove();
                }
                else
                {
                    text = variableValue.Text(overriddenLanguageID);
                    XText textNode = new XText(text);
                    element.ReplaceWith(textNode);
                }

                nodeWasReplaced = true;
            }
            else if (contentKey != null)
            {
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), false, "Stop", String.Empty);

                //XNode result = HandleContent(TextUtilities.HtmlEncode(element.ToString()), contentRenderParameters);
                XNode result = HandleContent(element.ToString(), contentRenderParameters);

                if (result != null)
                {
                    element.ReplaceWith(result);
                    nodeWasReplaced = true;
                }
            }
            else
            {
                XText textNode = new XText("(invalid insert element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected virtual XNode HandleContent(string referenceString, ContentRenderParameters contentRenderParameters)
        {
            return HandleElementError("Not implemented: HandleContent");
        }

        protected virtual bool HandleIf(XElement element)
        {
            LanguageID originalLanguageID = TargetLanguageID;
            LanguageID languageID = TargetLanguageID;
            string contentKey = null;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            string tag = String.Empty;
            string subType = null;
            string subSubType = null;
            string condition = null;
            string operand = null;
            string value = null;
            MultiLanguageItem variableValue = null;
            int intValue = 0;
            int intOperand = 0;
            string errorMessage = null;
            List<XNode> nodes = new List<XNode>();
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "component":   // Legacy
                        ParseContent(attributeValue, TargetLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out languageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                    case "subcomponent":  // Legacy
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "source":
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "studyitemindex":
                    case "itemindex":
                    case "index":   // Legacy
                        ParseInteger(attributeValue, out itemIndex);
                        if ((itemIndex != -1) && (itemCount == -1))
                            itemCount = 1;
                        break;
                    case "name":
                        ParseVariableName(attributeValue, out variableValue);
                        break;
                    case "have":
                        ParseContent(attributeValue, languageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType,
                            out subSubType, out languageID, out optionsList, out errorMessage);
                        condition = "have";
                        break;
                    case "havelanguage":
                        ParseLanguageIDCheck(attributeValue, out languageID);
                        condition = "havelanguage";
                        if (languageID != null)
                            value = languageID.LanguageCultureExtensionCode;
                        else
                            value = String.Empty;
                        break;
                    case "havemedia":
                        condition = "havemedia";
                        operand = attributeValue;
                        break;
                    case "hascontent":
                        condition = "hascontent";
                        operand = attributeValue;
                        ParseContent(attributeValue, TargetLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out languageID, out optionsList, out errorMessage);
                        break;
                    case "equals":
                    case "equal":
                        condition = "==";
                        operand = attributeValue;
                        break;
                    case "notequal":
                    case "notequals":
                        condition = "!=";
                        operand = attributeValue;
                        break;
                    case "greater":
                    case "greaterthan":
                        condition = ">";
                        operand = attributeValue;
                        break;
                    case "less":
                    case "lessthan":
                        condition = "<";
                        operand = attributeValue;
                        break;
                    case "greaterorequal":
                    case "greaterthanorequal":
                        condition = ">=";
                        operand = attributeValue;
                        break;
                    case "lessorequal":
                    case "lessthanorequal":
                        condition = "<=";
                        operand = attributeValue;
                        break;
                    case "condition":
                        condition = attributeValue;
                        break;
                    case "operand":
                        operand = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (subType == null)
                subType = String.Empty;

            if (condition == null)
                condition = String.Empty;

            GetContent(contentKey, itemIndex, out Content, out StudyList, out StudyItem);

            if (!String.IsNullOrEmpty(condition))
            {
                if (!String.IsNullOrEmpty(contentKey))
                {
                    if (StudyList != null)
                    {
                        if (StudyItem == null)
                        {
                            if (!String.IsNullOrEmpty(tag))
                            {
                                if (itemIndex == -1)
                                    itemIndex = StudyList.GetTaggedStudyItemIndex(tag);

                                if (itemIndex == -1)
                                    StudyItem = StudyList.GetStudyItemIndexed(0);
                                else
                                    StudyItem = StudyList.GetStudyItemIndexed(itemIndex);
                            }
                            else
                            {
                                if (itemIndex == -1)
                                    StudyItem = StudyList.GetStudyItemIndexed(0);
                                else
                                    StudyItem = StudyList.GetStudyItemIndexed(itemIndex);
                            }
                        }

                        if (StudyItem != null)
                        {
                            switch (subType.ToLower())
                            {
                                case "text":
                                    if (sentenceIndex == -1)
                                        value = StudyItem.Text(languageID);
                                    else
                                        value = StudyItem.RunText(languageID, sentenceIndex);
                                    break;
                                case "speakername":
                                    value = StudyList.GetSpeakerNameText(StudyItem.SpeakerNameKey, languageID);
                                    break;
                                case "speakerindex":
                                    value = StudyList.GetSpeakerNameIndex(StudyItem.SpeakerNameKey).ToString();
                                    break;
                                case "languageid":
                                    if (StudyItem.HasLanguageID(languageID))
                                        nodes.AddRange(element.Nodes());
                                    break;
                                case "count":
                                    value = StudyList.StudyItemCount().ToString();
                                    break;
                                case "index":
                                    value = itemIndex.ToString();
                                    break;
                                default:
                                    value = "true";
                                    break;
                            }
                        }
                    }
                    else if (variableValue != null)
                    {
                        if ((languageID != null) || String.IsNullOrEmpty(languageID.LanguageCultureExtensionCode))
                        {
                            value = variableValue.Text(languageID);
                        }
                    }
                }
                if (IsInteger(value) && IsInteger(operand))
                {
                    ParseInteger(value, out intValue);
                    ParseInteger(operand, out intOperand);

                    switch (condition.ToLower())
                    {
                        case "have":
                            if (!String.IsNullOrEmpty(value))
                                nodes.AddRange(element.Nodes());
                            break;
                        case "havelanguage":
                            if (!String.IsNullOrEmpty(value))
                                nodes.AddRange(element.Nodes());
                            break;
                        case "==":
                        case "equal":
                        case "equals":
                            if (intValue == intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "!=":
                        case "notequal":
                        case "notequals":
                            if (intValue != intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        case ">":
                        case "greater":
                        case "greaterthan":
                            if (intValue > intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "<":
                        case "less":
                        case "lessthan":
                            if (intValue < intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        case ">=":
                        case "greaterorequal":
                        case "greaterthanorequal":
                            if (intValue >= intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "<=":
                        case "lessorequal":
                        case "lessthanorequal":
                            if (intValue <= intOperand)
                                nodes.AddRange(element.Nodes());
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (condition.ToLower())
                    {
                        case "have":
                            if (!String.IsNullOrEmpty(value))
                                nodes.AddRange(element.Nodes());
                            break;
                        case "havelanguage":
                            if (!String.IsNullOrEmpty(value))
                                nodes.AddRange(element.Nodes());
                            break;
                        case "havemedia":
                            if (StudyList != null)
                            {
                                if (ContentUtilities.HasMedia(Content, itemIndex, languageID, operand, UserProfile,
                                        Content.ContentType))
                                    nodes.AddRange(element.Nodes());
                            }
                            break;
                        case "hascontent":
                            if (Content != null)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "==":
                            if (String.Compare(value, operand) == 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "!=":
                            if (String.Compare(value, operand) != 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        case ">":
                            if (String.Compare(value, operand) > 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "<":
                            if (String.Compare(value, operand) < 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        case ">=":
                            if (String.Compare(value, operand) >= 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        case "<=":
                            if (String.Compare(value, operand) <= 0)
                                nodes.AddRange(element.Nodes());
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                HandleElementError(element, "Invalid if element");
                //XText textNode = new XText("Invalid if element");
                //nodes.Add(textNode);
            }

            XElement parent = element.Parent;

            if (parent != null)
            {
                //foreach (XNode node in nodes)
                //    HandleNode(node);

                element.ReplaceWith(nodes);
            }
            else
            {
                element.Name = "div";
                element.RemoveAttributes();
                element.ReplaceNodes(nodes);
                HandleElement(element);
            }

            return nodeWasReplaced;
        }

        protected virtual bool HandleForEach(XElement element)
        {
            string contentKey = null;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            LanguageID languageID = TargetLanguageID;
            string tag = String.Empty;
            string subType = null;
            string subSubType = null;
            string key = null;
            string subKey = null;
            string errorMessage = null;
            string name = null;
            string[] values = null;
            string[] seps = new string[] { "," };
            BaseString variable = null;
            LanguageItem languageItem;
            XElement elementInstance;
            List<XNode> nodes = new List<XNode>();
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "component":   // Legacy
                        ParseContent(attributeValue, languageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out languageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                    case "subcomponent":  // Legacy
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "key":
                        key = attributeValue;
                        break;
                    case "subkey":
                        subKey = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "source":
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "name":
                        name = attributeValue;
                        variable = new BaseString(name, "");
                        AddVariable(variable);
                        break;
                    case "seps":
                        ParseValues(attributeValue, seps, out seps);
                        break;
                    case "values":
                        ParseValues(attributeValue, seps, out values);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (subType == null)
                subType = String.Empty;

            if (!String.IsNullOrEmpty(contentKey))
            {
                if (Node != null)
                {
                    List<BaseObjectContent> lessonComponents;

                    if (contentKey == "Lesson")
                    {
                        if (!String.IsNullOrEmpty(subType))
                            lessonComponents = Node.GetContentWithType(subType);
                        else
                            lessonComponents = Node.ContentList;

                        foreach (BaseObjectContent content in lessonComponents)
                        {
                            variable.Text = content.KeyString;
                            elementInstance = new XElement(element);
                            HandleVariableSubstitutions(elementInstance);
                            nodes.AddRange(elementInstance.Nodes());
                        }
                    }
                    else
                    {
                        Content = Node.GetFirstContentWithType(contentKey);

                        if (Content != null)
                            StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                        else
                            StudyList = null;

                        if (StudyList != null)
                        {
                            int count = StudyList.StudyItemCount();
                            int index = 0;
                            int startIndex;
                            int endIndex;

                            if (!String.IsNullOrEmpty(tag) && StudyList.GetTaggedStudyItemRange(tag, out startIndex, out endIndex))
                            {
                                index = startIndex;
                                count = endIndex;
                            }

                            for (; index < count; index++)
                            {
                                StudyItem = StudyList.GetStudyItemIndexed(index);

                                switch (subType.ToLower())
                                {
                                    case "key":
                                        variable.Text = StudyItem.KeyString;
                                        break;
                                    case "index":
                                        variable.Text = index.ToString();
                                        break;
                                    case "text":
                                        languageItem = StudyItem.LanguageItem(languageID);
                                        if (languageItem != null)
                                        {
                                            if (sentenceIndex == -1)
                                                variable.Text = languageItem.Text;
                                            else
                                                variable.Text = languageItem.GetRunText(sentenceIndex);
                                        }
                                        else
                                            variable.Text = "(no translation: \"" + StudyItem.Text(0) + "\")";
                                        break;
                                    case "speakername":
                                        variable.Text = StudyList.GetSpeakerNameText(StudyItem.SpeakerNameKey, languageID);
                                        break;
                                    case "":
                                        if (!String.IsNullOrEmpty(StudyItem.SpeakerNameKey))
                                            variable.Text = StudyList.GetSpeakerNameText(StudyItem.SpeakerNameKey, languageID) + ":";
                                        else
                                            variable.Text = "";
                                        if (LanguageDescriptors != null)
                                        {
                                            foreach (LanguageDescriptor languageDescriptor in LanguageDescriptors)
                                            {
                                                if (!languageDescriptor.Show || !languageDescriptor.Used)
                                                    continue;

                                                languageItem = StudyItem.LanguageItem(languageDescriptor.LanguageID);

                                                if (languageItem != null)
                                                    variable.Text += " " + languageItem.Text;
                                                if (languageItem != null)
                                                {
                                                    if (sentenceIndex == -1)
                                                        variable.Text += " " + languageItem.Text;
                                                    else
                                                        variable.Text += " " + languageItem.GetRunText(sentenceIndex);
                                                }
                                            }
                                        }
                                        else if (sentenceIndex == -1)
                                        {
                                            foreach (LanguageItem item in StudyItem.LanguageItems)
                                                variable.Text += " " + item.Text;
                                        }
                                        else
                                        {
                                            foreach (LanguageItem item in StudyItem.LanguageItems)
                                                variable.Text += " " + item.GetRunText(sentenceIndex);
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                elementInstance = new XElement(element);
                                HandleVariableSubstitutions(elementInstance);
                                nodes.AddRange(elementInstance.Nodes());
                            }
                        }
                    }
                }
            }
            else if (values != null)
            {
                foreach (string value in values)
                {
                    variable.Text = value;

                    elementInstance = new XElement(element);
                    HandleVariableSubstitutions(elementInstance);
                    nodes.AddRange(elementInstance.Nodes());
                }
            }
            else
            {
                XText textNode = new XText("(invalid foreach element: \"" + element.ToString() + "\")");
                nodes.Add(textNode);
            }

            XElement parent = element.Parent;

            if (parent != null)
            {
                int elementIndex = parent.Nodes().ToList().IndexOf(element);
                int replaceCount = nodes.Count();
                int replaceIndex;

                element.ReplaceWith(nodes.ToArray());

                if (replaceCount != 0)
                {
                    for (replaceIndex = elementIndex + replaceCount - 1; replaceIndex >= elementIndex; replaceIndex--)
                    {
                        XNode node = parent.Nodes().ElementAt(replaceIndex);
                        HandleNode(node);
                    }
                }
            }
            else
            {
                element.Name = "div";
                element.RemoveAttributes();
                element.ReplaceNodes(nodes);
                HandleElement(element);
            }

            if (variable != null)
                DeleteVariable(variable);

            return nodeWasReplaced;
        }

        // Returns true if the element was handled.
        protected virtual bool HandleExtensionMarkupElement(XElement element, out bool nodeWasReplaced)
        {
            nodeWasReplaced = false;
            return false;
        }

        protected virtual bool HandleVariableSubstitutions(XElement element)
        {
            XElement subElement;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
                attribute.SetValue(EvaluateAttributeExpression(attribute.Value));

            IEnumerable<XNode> nodes = element.Nodes();

            if (nodes != null)
            {
                int count = nodes.Count();

                for (int i = 0; i < count; i++)
                {
                    XNode node = nodes.ElementAt(i);

                    switch (node.GetType().Name)
                    {
                        case "XText":
                            if (HandleTextNode(node as XText))
                                nodeWasReplaced = true;
                            break;
                        case "XElement":
                            subElement = node as XElement;
                            if (HandleVariableSubstitutions(subElement))
                                nodeWasReplaced = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            return nodeWasReplaced;
        }

        protected virtual bool HandleFor(XElement element)
        {
            string name = null;
            int first = 0;
            int last = 0;
            int limit = 0;
            bool hasLimit = false;
            int step = 1;
            int index;
            BaseString variable = null;
            XElement elementInstance;
            List<XNode> nodes = new List<XNode>();
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        variable = new BaseString(name, "");
                        AddVariable(variable);
                        break;
                    case "first":
                        ParseInteger(attributeValue, out first);
                        break;
                    case "last":
                        ParseInteger(attributeValue, out last);
                        break;
                    case "limit":
                        ParseInteger(attributeValue, out limit);
                        hasLimit = true;
                        break;
                    case "step":
                        ParseInteger(attributeValue, out step);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (String.IsNullOrEmpty(name))
            {
                XText textNode = new XText("(invalid for element: \"" + element.ToString() + "\")");
                if (element.Parent != null)
                    element.ReplaceWith(textNode);
                else
                    element.AddFirst(textNode);
            }
            else
            {
                if (hasLimit)
                {
                    if (limit > first)
                    {
                        for (index = first; index < limit; index += step)
                        {
                            variable.Text = index.ToString();
                            elementInstance = new XElement(element);
                            HandleVariableSubstitutions(elementInstance);
                            nodes.AddRange(elementInstance.Nodes());
                        }
                    }
                    else
                    {
                        for (index = first; index > limit; index += step)
                        {
                            variable.Text = index.ToString();
                            elementInstance = new XElement(element);
                            HandleVariableSubstitutions(elementInstance);
                            nodes.AddRange(elementInstance.Nodes());
                        }
                    }
                }
                else
                {
                    if (last > first)
                    {
                        for (index = first; index <= last; index += step)
                        {
                            variable.Text = index.ToString();
                            elementInstance = new XElement(element);
                            HandleVariableSubstitutions(elementInstance);
                            nodes.AddRange(elementInstance.Nodes());
                        }
                    }
                    else
                    {
                        for (index = first; index >= last; index += step)
                        {
                            variable.Text = index.ToString();
                            elementInstance = new XElement(element);
                            HandleVariableSubstitutions(elementInstance);
                            nodes.AddRange(elementInstance.Nodes());
                        }
                    }
                }

                XElement parent = element.Parent;

                if (parent != null)
                {
                    int elementIndex = parent.Nodes().ToList().IndexOf(element);
                    int replaceCount = nodes.Count();
                    int replaceIndex;

                    element.ReplaceWith(nodes.ToArray());

                    if (replaceCount != 0)
                    {
                        for (replaceIndex = elementIndex + replaceCount - 1; replaceIndex >= elementIndex; replaceIndex--)
                        {
                            XNode node = parent.Nodes().ElementAt(replaceIndex);
                            HandleNode(node);
                        }
                    }
                }
                else
                {
                    element.Name = "div";
                    element.RemoveAttributes();
                    element.ReplaceNodes(nodes);
                    HandleElement(element);
                }
            }

            if (variable != null)
                DeleteVariable(variable);

            return nodeWasReplaced;
        }

        protected virtual bool HandleLoop(XElement element)
        {
            bool nodeWasReplaced = false;
            element.Name = "div";
            element.RemoveAttributes();
            nodeWasReplaced = HandleElement(element);
            return nodeWasReplaced;
        }

        protected virtual bool HandleNonTemplateGeneration()
        {
            return false;
        }

        protected virtual bool HandleGenerate(XElement element)
        {
            if (!IsPreviewGenerate)
            {
                element.Name = "div";
                element.RemoveAttributes();
                element.RemoveNodes();
                element.Add(new XText(""));
                return false;
            }

            IEnumerable<XNode> nodes = element.Nodes();
            XElement subElement;
            bool returnValue = true;

            InGenerate = true;

            StartTime = DateTime.MinValue;
            CurrentTime = StartTime;
            LoadWorkingSet(
                null,
                null,
                null,
                SelectorAlgorithmCode.Forward,
                ToolSelectorMode.Normal,
                ToolProfile.DefaultNewLimit,
                ToolProfile.DefaultReviewLimit,
                false,
                false,
                false,
                ToolProfile.DefaultChunkSize,
                ToolProfile.DefaultReviewLevel,
                DefaultProfileName,
                DefaultConfigurationName);

            StudyItem = null;
            CurrentTargetLanguageID = TargetLanguageID;
            CurrentHostLanguageID = UILanguageID;

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count(); i++)
                {
                    XNode node = nodes.ElementAt(i);

                    switch (node.GetType().Name)
                    {
                        case "XText":
                            HandleTextNode(node as XText);
                            break;
                        case "XElement":
                            subElement = node as XElement;
                            if (HandleElement(subElement))
                            {
                                nodes = element.Nodes();
                                returnValue = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            element.Name = "div";

            InGenerate = false;

            return returnValue;
        }

        protected virtual bool HandleSay(XElement element)
        {
            string field = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "field":
                        field = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (String.IsNullOrEmpty(field))
            {
                XText textNode = new XText("(invalid for element: \"" + element.ToString() + "\")");
                if (element.Parent != null)
                    element.ReplaceWith(textNode);
                else
                    element.AddFirst(textNode);

                UpdateCurrentTime(textNode, TargetLanguageID);
                nodeWasReplaced = false;
            }
            else
            {
                string text = EvaluateField(field);
                XElement textNode = new XElement("div", text);
                element.ReplaceWith(textNode);
                UpdateCurrentTime(textNode, TargetLanguageID);
            }

            return nodeWasReplaced;
        }

        protected virtual bool HandlePause(XElement element)
        {
            double seconds = LastItemTime.TotalSeconds;
            double minimum = 0.0;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "seconds":
                        try
                        {
                            seconds = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            HandleElementError(element, "\nError converting pause seconds attribute to a number: " + attributeValue);
                            return false;
                        }
                        break;
                    case "multiply":
                        try
                        {
                            double factor = Convert.ToDouble(attributeValue);
                            seconds *= factor;
                        }
                        catch (Exception)
                        {
                            HandleElementError(element, "\nError converting multiply attribute to a number: " + attributeValue);
                            return false;
                        }
                        break;
                    case "add":
                        try
                        {
                            double add = Convert.ToDouble(attributeValue);
                            seconds += add;
                        }
                        catch (Exception)
                        {
                            HandleElementError(element, "\nError converting add attribute to a number: " + attributeValue);
                            return false;
                        }
                        break;
                    case "minimum":
                        try
                        {
                            minimum = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            HandleElementError(element, "\nError converting pause seconds attribute to a number: " + attributeValue);
                            return false;
                        }
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (seconds < minimum)
                seconds = minimum;

            TimeSpan duration = TimeSpan.FromSeconds(seconds);
            UpdateCurrentTime(duration);

            element.ReplaceWith(new XText(""));

            return nodeWasReplaced;
        }

        protected virtual bool HandleItem(XElement element)
        {
            int startIndex = -1;
            int endIndex = 1;
            List<XNode> nodes = new List<XNode>();
            LanguageID languageID = TargetLanguageID;
            bool nodeWasReplaced = true;

            if (WorkingSet == null)
            {
                HandleElementError(element, "(no working set created--do you have an enclosing generate element?)");
                return false;
            }

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "index":
                        ParseInteger(attributeValue, out startIndex);
                        break;
                    case "start":
                        ParseInteger(attributeValue, out startIndex);
                        break;
                    case "end":
                        ParseInteger(attributeValue, out endIndex);
                        break;
                    case "label":
                        if (!WorkingSet.GetLabeledToolStudyItemRange(attributeValue, CurrentHostLanguageID, out startIndex, out endIndex))
                        {
                            HandleElementError(element, "(Item label not found in element \"" + element.Name.LocalName + "\": \"" + attributeValue + "\")");
                            return false;
                        }
                        break;
                    case "tag":
                        if (!WorkingSet.GetTaggedToolStudyItemRange(attributeValue, out startIndex, out endIndex))
                        {
                            HandleElementError(element, "(Item tag not found in element \"" + element.Name.LocalName + "\": \"" + attributeValue + "\")");
                            return false;
                        }
                        break;
                    case "all":
                        startIndex = 0;
                        endIndex = (WorkingSet != null ? WorkingSet.ToolStudyItemCount() : 0);
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "speed":
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (startIndex == -1)
            {
                if (StudyItem == null)
                    StudyItem = WorkingSet.GetToolStudyItemIndexed(0).StudyItem;

                if (StudyItem == null)
                    return true;

                LanguageItem languageItem = StudyItem.LanguageItem(languageID);

                if (languageItem == null)
                    return true;

                if ((languageItem.SentenceRuns != null) && (languageItem.SentenceRuns.Count() != 0))
                {
                    foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                    {
                        string text = languageItem.GetRunText(sentenceRun);
                        XElement divElement = new XElement("div", text);
                        nodes.Add(divElement);
                    }
                }
                else
                {
                    string text = languageItem.Text;
                    XElement divElement = new XElement("div", text);
                    nodes.Add(divElement);
                }
            }
            else
            {
                if (endIndex < startIndex)
                    endIndex = startIndex + 1;

                for (int index = startIndex; index < endIndex; index++)
                {
                    MultiLanguageItem multiLanguageItem = WorkingSet.GetToolStudyItemIndexed(index).StudyItem;

                    if (multiLanguageItem == null)
                        continue;

                    LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    if ((languageItem.SentenceRuns != null) && (languageItem.SentenceRuns.Count() != 0))
                    {
                        foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                        {
                            string text = languageItem.GetRunText(sentenceRun);
                            XElement divElement = new XElement("div", text);
                            nodes.Add(divElement);
                        }
                    }
                    else
                    {
                        string text = languageItem.Text;
                        XElement divElement = new XElement("div", text);
                        nodes.Add(divElement);
                    }
                }
            }

            if (nodes.Count() != 0)
            {
                element.ReplaceWith(nodes.ToArray());
                DateTime saveCurrentTime = CurrentTime;
                UpdateCurrentTime(nodes, languageID);
                LastItemTime = CurrentTime - saveCurrentTime;
            }

            return nodeWasReplaced;
        }

        // Side effect is that StudyItem is set, for use by RenderedItem.
        protected virtual bool HandleEach(XElement element)
        {
            string contentKey = null;
            string tag = String.Empty;
            string label = String.Empty;
            LanguageID languageID = TargetLanguageID;
            SelectorAlgorithmCode selector = SelectorAlgorithmCode.Forward;
            ToolSelectorMode mode = ToolSelectorMode.Normal;
            int newLimit = ToolProfile.DefaultNewLimit;
            int reviewLimit = ToolProfile.DefaultReviewLimit;
            bool isRandomUnique = false;
            bool isRandomNew = false;
            bool isAdaptiveMixNew = false;
            int chunkSize = ToolProfile.DefaultChunkSize;
            int level = ToolProfile.DefaultReviewLevel;
            string profileName = DefaultProfileName;
            string configurationKey = DefaultConfigurationName;
            int totalCount = -1;
            ContentStudyList savedWorkingSetStudyList;
            ToolStudyList savedWorkingSet;
            ToolStudyItem entry;
            ToolItemStatus status;
            XElement elementInstance;
            List<XNode> nodes = new List<XNode>();
            XNode node;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "componentkey":
                        contentKey = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "label":
                        label = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "selector":
                        try
                        {
                            selector = ToolProfile.GetSelectorAlgorithmCodeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "mode":
                        try
                        {
                            mode = ToolItemSelector.GetToolSelectorModeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "newlimit":
                        ParseInteger(attributeValue, out newLimit);
                        break;
                    case "reviewlimit":
                        ParseInteger(attributeValue, out reviewLimit);
                        break;
                    case "level":
                        ParseInteger(attributeValue, out level);
                        break;
                    case "profile":
                        profileName = attributeValue;
                        break;
                    case "configuration":
                        configurationKey = attributeValue;
                        break;
                    case "count":
                        ParseInteger(attributeValue, out totalCount);
                        break;
                    case "randomunique":
                        ParseBoolean(attributeValue, out isRandomUnique);
                        break;
                    case "randomnew":
                        ParseBoolean(attributeValue, out isRandomNew);
                        break;
                    case "adaptivemixnew":
                        ParseBoolean(attributeValue, out isAdaptiveMixNew);
                        break;
                    case "chunksize":
                        ParseInteger(attributeValue, out chunkSize);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            PushWorkingSet(
                contentKey,
                tag,
                label,
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey,
                out savedWorkingSetStudyList,
                out savedWorkingSet);

            int count = (WorkingSet != null ? WorkingSet.ToolStudyItemCount() : 0);
            int index = 0;

            if (totalCount == -1)
                totalCount = count;

            for (; index < totalCount; index++)
            {
                entry = WorkingSet.GetToolStudyItemIndexed(Selector.CurrentIndex);

                if (entry == null)
                    break;

                StudyItem = entry.StudyItem;
                elementInstance = new XElement(element);

                for (int i = 0; i < elementInstance.Nodes().Count(); i++)
                {
                    node = elementInstance.Nodes().ElementAt(i);
                    HandleNode(node);
                }

                nodes.AddRange(elementInstance.Nodes());

                status = entry.GetStatus(Configuration.Key);
                ToolProfile toolProfile = Configuration.Profile;
                toolProfile.TouchApplyGrade(
                    status,
                    1.0f,
                    CurrentTime,
                    Configuration);

                if (!Selector.SetNextIndex())
                    break;
            }

            PopWorkingSet(savedWorkingSetStudyList, savedWorkingSet);

            XElement divElement = new XElement("div", nodes);
            element.ReplaceWith(divElement);

            return nodeWasReplaced;
        }

        // Side effect is that StudyItem is set, for use by RenderedItem.
        protected virtual bool HandleDuration(XElement element)
        {
            string contentKey = null;
            string tag = String.Empty;
            string label = String.Empty;
            SelectorAlgorithmCode selector = SelectorAlgorithmCode.Forward;
            ToolSelectorMode mode = ToolSelectorMode.Normal;
            int newLimit = ToolProfile.DefaultNewLimit;
            int reviewLimit = ToolProfile.DefaultReviewLimit;
            bool isRandomUnique = false;
            bool isRandomNew = false;
            bool isAdaptiveMixNew = false;
            int chunkSize = ToolProfile.DefaultChunkSize;
            int level = ToolProfile.DefaultReviewLevel;
            string profileName = DefaultProfileName;
            string configurationKey = DefaultConfigurationName;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            bool stopAtEnd = false;
            LanguageID languageID = TargetLanguageID;
            TimeSpan duration;
            DateTime endTime;
            ContentStudyList savedWorkingSetStudyList;
            ToolStudyList savedWorkingSet;
            ToolStudyItem entry;
            ToolItemStatus status;
            XElement elementInstance;
            List<XNode> nodes = new List<XNode>();
            XNode node;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "componentkey":
                        contentKey = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "label":
                        label = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "selector":
                        try
                        {
                            selector = ToolProfile.GetSelectorAlgorithmCodeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "mode":
                        try
                        {
                            mode = ToolItemSelector.GetToolSelectorModeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "newlimit":
                        ParseInteger(attributeValue, out newLimit);
                        break;
                    case "reviewlimit":
                        ParseInteger(attributeValue, out reviewLimit);
                        break;
                    case "randomunique":
                        ParseBoolean(attributeValue, out isRandomUnique);
                        break;
                    case "randomnew":
                        ParseBoolean(attributeValue, out isRandomNew);
                        break;
                    case "adaptivemixnew":
                        ParseBoolean(attributeValue, out isAdaptiveMixNew);
                        break;
                    case "chunksize":
                        ParseInteger(attributeValue, out chunkSize);
                        break;
                    case "level":
                        ParseInteger(attributeValue, out level);
                        break;
                    case "profile":
                        profileName = attributeValue;
                        break;
                    case "configuration":
                        configurationKey = attributeValue;
                        break;
                    case "hours":
                        ParseInteger(attributeValue, out hours);
                        break;
                    case "minutes":
                        ParseInteger(attributeValue, out minutes);
                        break;
                    case "seconds":
                        ParseInteger(attributeValue, out seconds);
                        break;
                    case "stopatend":
                        stopAtEnd = (attributeValue == "true" ? true : false);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            duration = new TimeSpan(hours, minutes, seconds);
            endTime = CurrentTime + duration;

            PushWorkingSet(
                contentKey,
                tag,
                label,
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey,
                out savedWorkingSetStudyList,
                out savedWorkingSet);

            while (CurrentTime < endTime)
            {
                entry = WorkingSet.GetToolStudyItemIndexed(Selector.CurrentIndex);

                if (entry == null)
                    break;

                StudyItem = entry.StudyItem;
                elementInstance = new XElement(element);

                for (int i = 0; i < elementInstance.Nodes().Count(); i++)
                {
                    node = elementInstance.Nodes().ElementAt(i);
                    HandleNode(node);
                }

                nodes.AddRange(elementInstance.Nodes());

                status = entry.GetStatus(Configuration.Key);
                ToolProfile toolProfile = Configuration.Profile;
                toolProfile.TouchApplyGrade(
                    status,
                    1.0f,
                    CurrentTime,
                    Configuration);

                if (!Selector.SetNextIndex())
                    break;
            }

            PopWorkingSet(savedWorkingSetStudyList, savedWorkingSet);

            XElement divElement = new XElement("div", nodes);
            element.ReplaceWith(divElement);

            return nodeWasReplaced;
        }

        // Side effect is that StudyItem is set, for use by RenderedItem.
        protected virtual bool HandleForget(XElement element)
        {
            if (WorkingSet != null)
                WorkingSet.ForgetAll();

            return false;
        }

        protected int _ChoiceCount;
        protected string _ButtonContainerID;
        protected string _ChoiceContainerID;
        protected IEnumerable<XAttribute> _ChooseAttributes;
        protected XElement _ButtonDivElement;
        protected XElement _ChoiceDivElement;

        protected virtual bool HandleChoose(XElement element)
        {
            int chooseIDOrdinal = ++ChooseIDOrdinal;
            List<XNode> childNodes = element.Nodes().ToList();
            string id = "ChooseDiv" + chooseIDOrdinal.ToString();
            LanguageID originalLanguageID = HostLanguageID;
            LanguageID languageID = HostLanguageID;
            string prompt = null;
            string orientation = "vertical";
            string width = String.Empty;
            string className = "automated_choose";
            string style = String.Empty;
            XElement topElement = element;
            XElement buttonTopElement = null;
            XElement tableElement;
            int saveChoiceCount = _ChoiceCount;
            string saveButtonContainerID = "ChooseButtonContainer" + chooseIDOrdinal.ToString();
            string saveChoiceContainerID = "ChooseChoiceContainer" + chooseIDOrdinal.ToString();
            IEnumerable<XAttribute> saveChooseAttributes = _ChooseAttributes;
            XElement saveButtonDivElement = _ButtonDivElement;
            XElement saveChoiceDivElement = _ChoiceDivElement;
            bool nodeWasReplaced = false;

            _ChoiceCount = 0;
            _ButtonContainerID = "ChooseButtonContainer" + chooseIDOrdinal.ToString();
            _ChoiceContainerID = "ChooseChoiceContainer" + chooseIDOrdinal.ToString();
            _ChooseAttributes = new List<XAttribute>(element.Attributes());

            foreach (XElement descendent in element.Descendants())
            {
                if (descendent.Name.LocalName.ToLower() == "choice")
                    _ChoiceCount++;
            }

            foreach (XAttribute attribute in _ChooseAttributes)
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "id":
                        id = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "prompt":
                        prompt = attributeValue;
                        break;
                    case "orientation":
                        orientation = attributeValue.ToLower();
                        break;
                    case "width":
                        width = attributeValue.ToLower();
                        break;
                    case "choicewidth":
                        break;
                    case "class":
                        className = attributeValue;
                        break;
                    case "choiceclass":
                        break;
                    case "style":
                        style = attributeValue;
                        break;
                    case "choicestyle":
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (width == "full")
                width = "100%";

            element.RemoveAttributes();
            element.RemoveNodes();

            topElement.Name = "div";
            topElement.Add(new XAttribute("style", "width: 100%;"));

            if (!String.IsNullOrEmpty(prompt))
            {
                if ((languageID != null) && (languageID != UILanguageID))
                    prompt = LanguageUtilities.TranslateString(prompt, languageID);

                buttonTopElement = new XElement("div");
                buttonTopElement.Add(new XAttribute("id", _ButtonContainerID));
                buttonTopElement.Add(new XElement("p", prompt));
                topElement.Add(buttonTopElement);
            }

            if (orientation.StartsWith("hor"))
            {
                tableElement = new XElement("table");
                _ButtonDivElement = new XElement("tr");
                tableElement.Add(_ButtonDivElement);
                style = "border: none; " + style.Trim();
            } 
            else
            {
                _ButtonDivElement = new XElement("div");
                tableElement = _ButtonDivElement;
            }

            if (buttonTopElement == null)
            {
                tableElement.Add(new XAttribute("id", _ButtonContainerID));
                buttonTopElement = tableElement;
                topElement.Add(tableElement);
            }
            else
                buttonTopElement.Add(tableElement);

            if (!String.IsNullOrEmpty(width))
                style += "width: " + width + "; ";

            if (!String.IsNullOrEmpty(id))
                topElement.Add(new XAttribute("id", id.Trim()));

            if (!String.IsNullOrEmpty(className))
                buttonTopElement.Add(new XAttribute("class", className.Trim()));

            if (!String.IsNullOrEmpty(style))
                buttonTopElement.Add(new XAttribute("style", style.Trim()));

            _ChoiceDivElement = new XElement("div");
            _ChoiceDivElement.Add(new XAttribute("id", _ChoiceContainerID));

            _ChoiceDivElement.ReplaceNodes(childNodes);
            HandleElementChildren(_ChoiceDivElement);

            topElement.Add(_ChoiceDivElement);

            _ChoiceCount = saveChoiceCount;
            _ButtonContainerID = saveButtonContainerID;
            _ChoiceContainerID = saveChoiceContainerID;
            _ChooseAttributes = saveChooseAttributes;
            _ButtonDivElement = saveButtonDivElement;
            _ChoiceDivElement = saveChoiceDivElement;

            return nodeWasReplaced;
        }

        protected virtual bool HandleChoice(XElement element)
        {
            int chooseIDOrdinal = ChooseIDOrdinal;
            int choiceIDOrdinal = ++ChoiceIDOrdinal;
            XElement buttonContainer;
            XElement buttonElement;
            XElement choiceContainer;
            string prompt = choiceIDOrdinal.ToString();
            string command = String.Empty;
            string commandTarget = String.Empty;
            string mode = "Display";
            LanguageID originalLanguageID = HostLanguageID;
            LanguageID languageID = HostLanguageID;
            string orientation = "vertical";
            string width = "";
            string containerWidth = "";
            string className = "automated_choice";
            string style = String.Empty;
            string elementName = "choice";
            string name = String.Empty;
            string buttonClass = "btn btn-default";
            string buttonStyle = null;
            string choiceID = "Choose" + chooseIDOrdinal.ToString() + "Choice" + (choiceIDOrdinal).ToString();
            string buttonID = choiceID + "Button";
            string buttonClick;
            string url = String.Empty;
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            BaseObjectNodeTree tree = SourceTree;
            BaseObjectNode node = SourceNode;
            BaseObjectContent content = DocumentContent;
            int treeKey = (tree != null ? tree.KeyInt : -1);
            int nodeKey;
            string contentKey = (content != null ? content.KeyString : null);
            if (node == tree)
            {
                node = null;
                nodeKey = -1;
            }
            else
                nodeKey = node.KeyInt;
            bool isRenderElement = true;
            bool nodeWasReplaced = false;

            //List<XNode> childNodes = element.Nodes().ToList();

            if (_ChooseAttributes != null)
            {
                foreach (XAttribute attribute in _ChooseAttributes)
                {
                    string attributeValue = attribute.Value.Trim();

                    attributeValue = EvaluateAttributeExpression(attributeValue);

                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "language":
                            ParseLanguageID(attributeValue, out languageID);
                            break;
                        case "orientation":
                            orientation = attributeValue.ToLower();
                            break;
                        case "choicewidth":
                            width = attributeValue.ToLower();
                            break;
                        case "choiceclass":
                            className = attributeValue;
                            break;
                        case "choicestyle":
                            style = attributeValue;
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "id":
                        choiceID = attributeValue;
                        break;
                    case "prompt":
                        prompt = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "width":
                        width = attributeValue.ToLower();
                        break;
                    case "class":
                        className = attributeValue;
                        break;
                    case "style":
                        style = attributeValue;
                        break;
                    case "name":
                        name = attributeValue;
                        elementName = "view";
                        break;
                    case "mode":
                        // "automated" or "display".
                        mode = attributeValue;
                        break;
                    case "command":
                        // "home", "play", "pause", "stop", "end", "backstep", "repeat", "forwardstep",
                        // "nextlesson", "previouslesson", "nextcontent", "previouscontent"
                        command = attributeValue.ToLower();
                        break;
                    case "target":
                        // "automated", "media" (default "" is "automated")
                        commandTarget = attributeValue.ToLower();
                        break;
                    case "goto":
                        command = "goto";
                        commandTarget = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            foreach (XElement descendent in element.Descendants())
            {
                switch (descendent.Name.LocalName.ToLower())
                {
                    case "each":
                    case "duration":
                        isRenderElement = false;
                        break;
                    default:
                        break;
                }
            }

            if (mode.ToLower() == "automated")
                isRenderElement = false;

            if (isRenderElement)
            {
                int nth = (!String.IsNullOrEmpty(name)
                    ? -1
                    : MarkupTemplate.GetElementNth(MarkupTemplate.Markup, element));

                object routeValues = new
                {
                    treeKey,
                    nodeKey,
                    contentKey,
                    elementName,
                    name,
                    nth,
                    childrenOnly = true,
                    inGenerate = false
                };
                url = ApplicationData.Global.CreateHostUrl(controller, "RenderMarkupElement", routeValues);
            }

            buttonClick = "jt_choice_click(this, '" + _ButtonContainerID + "', '"
                + choiceID + "', '" + url + "', '" + command + "', '" + commandTarget + "')";

            if (!InGenerate)
            {
                bool saveInChoice = InChoice;
                InChoice = true;
                HandleElementChildren(element);
                InChoice = saveInChoice;
            }

            element.RemoveAttributes();
            element.RemoveNodes();

            if (orientation.StartsWith("hor"))
                containerWidth = (100 / _ChoiceCount).ToString() + "%";

            if (width == "full")
                buttonStyle = "width: 100%; ";
            else if (!String.IsNullOrEmpty(width))
                buttonStyle = "width: " + width + "; ";

            if (!String.IsNullOrEmpty(containerWidth))
                style += "width: " + containerWidth + "; ";

            if (orientation.StartsWith("hor"))
            {
                buttonContainer = new XElement("td");
                style = "border: none; " + style.Trim();
            }
            else if (orientation.StartsWith("float"))
            {
                buttonContainer = new XElement("span");
                buttonStyle += " margin-right: 8px; margin-bottom: 8px;";
                if (className == "automated_choice")
                    className = String.Empty;
            }
            else
                buttonContainer = new XElement("div");

            if (!String.IsNullOrEmpty(className))
                buttonContainer.Add(new XAttribute("class", className.Trim()));

            if (!String.IsNullOrEmpty(style))
                buttonContainer.Add(new XAttribute("style", style.Trim()));

            buttonElement = FormatButton(prompt, languageID, buttonClass, buttonStyle, buttonID, buttonClick);

            buttonContainer.Add(buttonElement);
            _ButtonDivElement.Add(buttonContainer);

            choiceContainer = element;
            choiceContainer.Name = "div";
            choiceContainer.Add(new XAttribute("id", choiceID));
            choiceContainer.Add(new XAttribute("style", "display: none;"));
            choiceContainer.Value = "\n";

            return nodeWasReplaced;
        }

        public XElement FormatButton(string prompt, LanguageID languageID, string className,
            string style, string id, string onclick)
        {
            if ((languageID != null) && (languageID != UILanguageID))
                prompt = LanguageUtilities.TranslateString(prompt, languageID);
            XElement buttonElement = new XElement("input");
            buttonElement.Add(new XAttribute("type", "button"));
            if (!String.IsNullOrEmpty(className))
                buttonElement.Add(new XAttribute("class", className));
            if (!String.IsNullOrEmpty(style))
                buttonElement.Add(new XAttribute("style", style));
            if (!String.IsNullOrEmpty(id))
                buttonElement.Add(new XAttribute("id", id));
            buttonElement.Add(new XAttribute("value", prompt));
            if (!String.IsNullOrEmpty(onclick))
                buttonElement.Add(new XAttribute("onclick", onclick));
            return buttonElement;
        }

        protected virtual bool HandleView(XElement element)
        {
            string name = null;
            LanguageID languageID = HostLanguageID;
            string width = "";
            string height = "";
            string className = "automated_choice";
            string style = String.Empty;
            bool hidden = true;
            bool remote = false;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "width":
                        width = attributeValue.ToLower();
                        break;
                    case "height":
                        height = attributeValue.ToLower();
                        break;
                    case "class":
                        className = attributeValue;
                        break;
                    case "style":
                        style = attributeValue;
                        break;
                    case "hidden":
                        ParseBoolean(attributeValue, out hidden);
                        break;
                    case "remote":
                        ParseBoolean(attributeValue, out remote);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            element.RemoveAttributes();
            element.RemoveNodes();

            if (remote)
                return false;

            if (width == "full")
                style += " width: 100%;";
            else if (!String.IsNullOrEmpty(width))
                style += " width: " + width + ";";

            if (height == "full")
                style += " height: 100%;";
            else if (!String.IsNullOrEmpty(height))
                style += " height: " + height + ";";

            if (hidden)
                style += " display: none;";

            if (!String.IsNullOrEmpty(className))
                element.Add(new XAttribute("class", className.Trim()));

            if (!String.IsNullOrEmpty(style))
                element.Add(new XAttribute("style", style.Trim()));

            HandleElementChildren(element);

            return nodeWasReplaced;
        }

        protected virtual bool HandleShow(XElement element)
        {
            string name = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            element.RemoveAttributes();
            element.RemoveNodes();

            return nodeWasReplaced;
        }

        protected virtual bool HandleHide(XElement element)
        {
            string name = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            element.RemoveAttributes();
            element.RemoveNodes();

            return nodeWasReplaced;
        }

        protected virtual bool HandlePlay(XElement element)
        {
            LanguageID overriddenLanguageID = null;
            LanguageID mediaLanguageID = null;
            MultiLanguageItem variableValue = null;
            string contentKey = null;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            string tag = String.Empty;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            bool isView = false;
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                        ParseContent(attributeValue, overriddenLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out overriddenLanguageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out overriddenLanguageID);
                        if (mediaLanguageID == null)
                            mediaLanguageID = overriddenLanguageID.MediaLanguageID();
                        break;
                    case "medialanguage":
                        ParseMediaLanguageID(attributeValue, out mediaLanguageID);
                        break;
                    case "studyitemindex":
                    case "itemindex":
                    case "index":   // Legacy
                        ParseInteger(attributeValue, out itemIndex);
                        if ((itemIndex != -1) && (itemCount == -1))
                            itemCount = 1;
                        break;
                    case "itemcount":
                        ParseInteger(attributeValue, out itemCount);
                        break;
                    case "sentenceindex":
                        ParseInteger(attributeValue, out sentenceIndex);
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "name":
                        ParseVariableName(attributeValue, out variableValue);
                        break;
                    case "view":
                        ParseBoolean(attributeValue, out isView);
                        break;
                    case "player":
                        ParseRenderOption(attribute.Name.LocalName, attributeValue, ref optionsList);
                        if (attributeValue.ToLower() == "inline")
                            isView = false;
                        else
                            isView = true;
                        break;
                    case "options":
                        optionsList = BaseString.GetBaseStringListFromString(attributeValue);
                        break;
                    case "elementtype":
                    case "class":
                    case "style":
                    case "languageformat":
                    case "rowformat":
                    case "displayformat":
                    case "showannotations":
                    case "useaudio":
                    case "usepicture":
                    case "usemedia":
                    case "autoplay":
                    case "playerid":
                    case "showvolume":
                    case "showtimeslider":
                    case "showtimetext":
                    case "endofmedia":
                    case "speed":
                    default:
                        if (!ParseRenderOption(attribute.Name.LocalName, attributeValue, ref optionsList))
                        {
                            HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                            return false;
                        }
                        break;
                }
            }

            if (errorMessage != null)
            {
                XText textNode = new XText("(" + errorMessage + ")");
                element.ReplaceWith(textNode);
            }
            else if (!isView)
            {
            }
            else if (variableValue != null)
            {
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    MarkupTemplate, variableValue,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture);
                XNode result = HandleMediaContent(element.ToString(), contentRenderParameters);

                if (result != null)
                {
                    element.ReplaceWith(result);
                    nodeWasReplaced = true;
                }
            }
            else if (contentKey != null)
            {
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), true, null, String.Empty);
                XNode result = HandleMediaContent(element.ToString(), contentRenderParameters);

                if (result != null)
                {
                    element.ReplaceWith(result);
                    nodeWasReplaced = true;
                }
            }
            else
            {
                XText textNode = new XText("(invalid play element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected virtual XNode HandleMediaContent(string referenceString, ContentRenderParameters contentRenderParameters)
        {
            return HandleElementError("Not implemented: HandleMediaContent");
        }

        protected virtual bool HandleStudy(XElement element)
        {
            LanguageID overriddenLanguageID = null;
            LanguageID mediaLanguageID = null;
            MultiLanguageItem variableValue = null;
            string contentKey = null;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            string tag = String.Empty;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                        ParseContent(attributeValue, overriddenLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out overriddenLanguageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out overriddenLanguageID);
                        if (mediaLanguageID == null)
                            mediaLanguageID = overriddenLanguageID.MediaLanguageID();
                        break;
                    case "medialanguage":
                        ParseMediaLanguageID(attributeValue, out mediaLanguageID);
                        break;
                    case "studyitemindex":
                    case "itemindex":
                    case "index":   // Legacy
                        ParseInteger(attributeValue, out itemIndex);
                        if ((itemIndex != -1) && (itemCount == -1))
                            itemCount = 1;
                        break;
                    case "itemcount":
                        ParseInteger(attributeValue, out itemCount);
                        break;
                    case "sentenceindex":
                        ParseInteger(attributeValue, out sentenceIndex);
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "name":
                        ParseVariableName(attributeValue, out variableValue);
                        break;
                    case "view":
                        break;
                    case "options":
                        optionsList = BaseString.GetBaseStringListFromString(attributeValue);
                        break;
                    case "useaudio":
                    case "usepicture":
                    case "usemedia":
                    case "endofstudy":
                    case "session":
                    case "sessionindex":
                    case "tool":
                    case "tooltype":
                    case "profile":
                    case "configuration":
                    case "mode":
                    default:
                        if (!ParseRenderOption(attribute.Name.LocalName, attributeValue, ref optionsList))
                        {
                            HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                            return false;
                        }
                        break;
                }
            }

            if (errorMessage != null)
            {
                XText textNode = new XText("(" + errorMessage + ")");
                element.ReplaceWith(textNode);
            }
            else if (variableValue != null)
            {
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    MarkupTemplate, variableValue,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture);
                contentRenderParameters.Action = "StudyToolDiv";
                contentRenderParameters.ViewName = "StudyToolDiv";
                XNode result = HandleStudyContent(element.ToString(), contentRenderParameters);

                if (result != null)
                {
                    element.ReplaceWith(result);
                    nodeWasReplaced = true;
                }
            }
            else if (contentKey != null)
            {
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), false, null, String.Empty);
                contentRenderParameters.Action = "StudyToolDiv";
                contentRenderParameters.ViewName = "StudyToolDiv";
                XNode result = HandleStudyContent(element.ToString(), contentRenderParameters);

                if (result != null)
                {
                    element.ReplaceWith(result);
                    nodeWasReplaced = true;
                }
            }
            else
            {
                XText textNode = new XText("(invalid study element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected virtual XNode HandleStudyContent(string referenceString, ContentRenderParameters contentRenderParameters)
        {
            return HandleElementError("Not implemented: HandleStudyContent");
        }

        protected virtual bool HandleStudyItem(XElement element)
        {
            int index = -1;
            bool nodeWasReplaced = true;

            if (WorkingSet == null)
            {
                HandleElementError(element, "(no working set created--do you have an enclosing generate element?)");
                return false;
            }

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "index":
                        ParseInteger(attributeValue, out index);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (StudyItem == null)
                StudyItem = WorkingSet.GetToolStudyItemIndexed(0).StudyItem;

            if (StudyItem == null)
                return false;

            if (index == -1)
                index = WorkingSet.GetStudyItemIndex(StudyItem);

            string contentKey = StudyItem.Content.KeyString;

            ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                SourceTree, SourceNode, contentKey, index, 1, 0, null,
                null, null, null, null, null,
                UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), false, null, String.Empty);
            contentRenderParameters.StudyItem = StudyItem;
            contentRenderParameters.MarkupRenderer = this;
            XNode result = HandleStudyItemContent(element.ToString(), contentRenderParameters);

            if (result != null)
            {
                element.ReplaceWith(result);
                nodeWasReplaced = true;
            }

            return nodeWasReplaced;
        }

        protected virtual XNode HandleStudyItemContent(string referenceString, ContentRenderParameters contentRenderParameters)
        {
            return HandleElementError("Not implemented: HandleStudyContent");
        }

        protected virtual bool HandleMarker(XElement element)
        {
            string name = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (String.IsNullOrEmpty(name))
                HandleElementError(element, "Marker element needs a name attribute.");
            else if (Markers.Contains(name))
                HandleElementError(element, "Marker names must be unique.");
            else
                Markers.Add(name);

            element.Name = "a";
            element.RemoveAttributes();
            element.Add(new XAttribute("id", name));

            HandleMarkerContent(name);

            HandleElementChildren(element);

            return nodeWasReplaced;
        }

        protected virtual void HandleMarkerContent(string marker)
        {
        }

        protected virtual bool HandleGoTo(XElement element)
        {
            string name = null;
            bool nodeWasReplaced = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "name":
                        name = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (String.IsNullOrEmpty(name))
                HandleElementError(element, "Marker element needs a name attribute.");

            element.Name = "a";
            element.RemoveAttributes();
            element.Add(new XAttribute("href", "#" + name));

            HandleGoToContent(name);

            HandleElementChildren(element);

            return nodeWasReplaced;
        }

        protected virtual void HandleGoToContent(string marker)
        {
        }

        public string GetSpeedKeyFromString(string speed)
        {
            string speedValue;

            switch (speed.ToLower())
            {
                case "slow":
                    speedValue = "Slow";
                    break;
                case "normal":
                    speedValue = "Normal";
                    break;
                case "fast":
                    speedValue = "Fast";
                    break;
                default:
                    int speedInt;
                    ParseInteger(speed, out speedInt);
                    if (speedInt < -3)
                        speedValue = "Slow";
                    else if (speedInt > 3)
                        speedValue = "Fast";
                    else
                        speedValue = "Normal";
                    break;
            }

            return speedValue;
        }

        public float GetSpeedMultiplier(string speed)
        {
            float speedValue = 1.0f;

            switch (speed.ToLower())
            {
                case "slow":
                    speedValue = 0.5f;
                    break;
                case "normal":
                    speedValue = 1.0f;
                    break;
                case "fast":
                    speedValue = 1.5f;
                    break;
                default:
                    int intValue;
                    ParseInteger(speed, out intValue);
                    speedValue = 1.0f + ((float)intValue / 20);
                    break;
            }

            return speedValue;
        }

        protected void PushWorkingSet(
            string contentKey,
            string tag,
            string label,
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey,
            out ContentStudyList savedWorkingSetStudyList,
            out ToolStudyList savedWorkingSet)
        {
            List<BaseObjectTitled> sources = null;

            savedWorkingSetStudyList = WorkingSetStudyList;
            savedWorkingSet = WorkingSet;

            if ((Node == null) || (String.IsNullOrEmpty(contentKey) && String.IsNullOrEmpty(tag) && String.IsNullOrEmpty(label)))
            {
                if (WorkingSet != null)
                {
                    SetupWorkingSetSelector(
                        selector,
                        mode,
                        newLimit,
                        reviewLimit,
                        isRandomUnique,
                        isRandomNew,
                        isAdaptiveMixNew,
                        chunkSize,
                        level,
                        profileName,
                        ref configurationKey);
                }
                return;
            }

            SetupWorkingSet(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);

            if (!String.IsNullOrEmpty(contentKey))
            {
                if ((SourceContents != null) && (SourceContents.Count() != 0))
                    sources = SourceContents.Cast<BaseObjectTitled>().ToList();
                else if (SourceNode != null)
                    sources = new List<BaseObjectTitled>() { SourceNode };
                else if (Content != null)
                    sources = new List<BaseObjectTitled>() { Content };
                else if (Node != null)
                    sources = new List<BaseObjectTitled>() { Node };
                else if (DocumentContent != null)
                    sources = new List<BaseObjectTitled>() { DocumentContent };

                StudyList = null;

                foreach (BaseObjectTitled source in sources)
                {
                    if (source is BaseObjectNode)
                    {
                        Node = (BaseObjectNode)source;
                        Content = Node.GetContent(contentKey);

                        if (Content == null)
                            Content = Node.GetFirstContentWithType(contentKey);

                        if (Content != null)
                            StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                        else
                            StudyList = null;

                        if (StudyList != null)
                            LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                    }
                    else if ((source is BaseObjectContent)
                        && (((source as BaseObjectContent).KeyString == contentKey) || ((source as BaseObjectContent).ContentType == contentKey)))
                    {
                        StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                        if (StudyList != null)
                            LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                    }
                }

                if ((StudyList == null) && (Node != null))
                {
                    Content = Node.GetContent(contentKey);

                    if (Content == null)
                        Content = Node.GetFirstContentWithType(contentKey);

                    if (Content != null)
                        StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                    else
                        StudyList = null;

                    if (StudyList != null)
                        LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                }
            }
            else
                LoadWorkingSetStudyItems(savedWorkingSetStudyList, tag, label, configurationKey);

            Selector.Reset();
        }

        protected void PopWorkingSet(ContentStudyList savedWorkingSetStudyList, ToolStudyList savedWorkingSet)
        {
            if (WorkingSet == savedWorkingSet)
            {
                if (Selector != null)
                    Selector.Reset();
            }
            else
                WorkingSet = savedWorkingSet;

            WorkingSetStudyList = savedWorkingSetStudyList;
        }

        protected void LoadWorkingSet(
            string contentKey,
            string tag,
            string label,
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            string configurationKey)
        {
            List<BaseObjectTitled> sources = null;

            if ((SourceContents != null) && (SourceContents.Count() != 0))
                sources = SourceContents.Cast<BaseObjectTitled>().ToList();
            else if (SourceNode != null)
                sources = new List<BaseObjectTitled>() { SourceNode };
            else if (Content != null)
                sources = new List<BaseObjectTitled>() { Content };
            else if (Node != null)
                sources = new List<BaseObjectTitled>() { Node };
            else if (DocumentContent != null)
                sources = new List<BaseObjectTitled>() { DocumentContent };

            SetupWorkingSet(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);

            if (sources != null)
            {
                if (!String.IsNullOrEmpty(contentKey))
                {
                    foreach (BaseObjectTitled source in sources)
                    {
                        if (source is BaseObjectNode)
                        {
                            Node = (BaseObjectNode)source;
                            Content = Node.GetFirstContentWithType(contentKey);

                            if (Content != null)
                                StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                            else
                                StudyList = null;

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                        else if ((source is BaseObjectContent) && ((source as BaseObjectContent).ContentType == contentKey))
                        {
                            StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                    }
                }
                else
                {
                    foreach (BaseObjectTitled source in sources)
                    {
                        if (source is BaseObjectNode)
                        {
                            Node = (BaseObjectNode)source;

                            int contentCount = Node.ContentCount();

                            for (int contentIndex = 0; contentIndex < contentCount; contentIndex++)
                            {
                                Content = Node.GetContentIndexed(contentIndex);

                                if (Content != null)
                                    StudyList = Content.GetContentStorageTyped<ContentStudyList>();
                                else
                                    StudyList = null;

                                if (StudyList != null)
                                    LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                            }
                        }
                        else if (source is BaseObjectContent)
                        {
                            StudyList = (source as BaseObjectContent).GetContentStorageTyped<ContentStudyList>();

                            if (StudyList != null)
                                LoadWorkingSetStudyItems(StudyList, tag, label, configurationKey);
                        }
                    }
                }
            }

            Selector.Reset();
        }

        protected void LoadWorkingSetStudyItems(ContentStudyList studyList, string tag, string label,
            string configurationKey)
        {
            int count = studyList.StudyItemCount();
            int index = 0;
            int startIndex;
            int endIndex;

            if (!String.IsNullOrEmpty(tag) && StudyList.GetTaggedStudyItemRange(tag, out startIndex, out endIndex))
            {
                index = startIndex;
                count = endIndex;
            }

            if (!String.IsNullOrEmpty(label) && StudyList.GetLabeledStudyItemRange(label, UILanguageID, out startIndex, out endIndex))
            {
                index = startIndex;
                count = endIndex;
            }

            for (; index < count; index++)
            {
                MultiLanguageItem multiLanguageItem = StudyList.GetStudyItemIndexed(index);
                string key = WorkingSetStudyList.AllocateStudyItemKey();
                MultiLanguageItem workingSetItem = new MultiLanguageItem(
                    key,
                    multiLanguageItem.LanguageItems,
                    multiLanguageItem.SpeakerNameKey,
                    null,
                    null,
                    studyList);
                WorkingSetStudyList.AddStudyItem(workingSetItem);
                WorkingSet.AddStudyItem(workingSetItem);
                workingSetItem.Content = multiLanguageItem.Content;
            }
        }

        protected void SetupWorkingSet(
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey)
        {
            WorkingSetStudyList = new ContentStudyList();
            WorkingSet = new ToolStudyList();
            WorkingSet.CopyLanguagesFromLanguageDescriptors(LanguageDescriptors);
            SetupWorkingSetSelector(
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey);
        }

        protected void SetupWorkingSetSelector(
            SelectorAlgorithmCode selector,
            ToolSelectorMode mode,
            int newLimit,
            int reviewLimit,
            bool isRandomUnique,
            bool isRandomNew,
            bool isAdaptiveMixNew,
            int chunkSize,
            int level,
            string profileName,
            ref string configurationKey)
        {
            List<ToolProfile> profiles = null;
            ToolProfile profile = null;
            string profileKey = ToolUtilities.ComposeToolProfileKey(UserRecord, profileName);

            if (profileName != "Markup")
            {
                profiles = ToolUtilities.GetToolProfiles();
                profile = profiles.FirstOrDefault(x => x.MatchKey(profileKey));

                if (profile == null)
                {
                    string defaultProfileKey = ToolUtilities.ComposeToolProfileDefaultKey(UserRecord);
                    ToolProfile defaultProfile = Repositories.ToolProfiles.Get(defaultProfileKey);
                    profile = ToolUtilities.CreateAndAddToolProfile(profileName, profiles.Count,
                        defaultProfile, out Message);
                    profiles.Add(profile);
                }
            }

            if (profile == null)
            {
                profile = new ToolProfile(
                    profileKey,                         // key
                    null,                               // MultiLanguageString title,
                    null,                               // MultiLanguageString description,
                    null,                               // string source,
                    null,                               // string package,
                    null,                               // string label,
                    null,                               // string imageFileName,
                    0,                                  // int index,
                    true,                               // bool isPublic,
                    WorkingSet.TargetLanguageIDs,       // List<LanguageID> targetLanguageIDs,
                    WorkingSet.HostLanguageIDs,         // List<LanguageID> hostLanguageIDs,
                    UserRecord.UserName,                // string owner,
                    ToolProfile.DefaultGradeCount,      // int gradeCount,
                    selector,                           // SelectorAlgorithmCode selectorMode,
                    newLimit,                           // int newLimit,
                    reviewLimit,                        // int reviewLimit,
                    isRandomUnique,                     // bool isRandomUnique,
                    isRandomNew,                        // bool isRandomNew,
                    isAdaptiveMixNew,                   // bool isAdaptiveMixNew,
                    level,                              // int reviewLevel,
                    ToolProfile.DefaultChoiceSize,      // int choiceSize,
                    chunkSize,                          // int chunkSize,
                    ToolProfile.DefaultHistorySize,     // int historySize,
                    ToolProfile.DefaultIsShowIndex,     // bool isShowIndex,
                    ToolProfile.DefaultIsShowOrdinal,   // bool isShowOrdinal,
                    ToolProfile.DefaultSpacedIntervalTable,  // List<TimeSpan> intervalTable,
                    null,                               // string fontFamily,
                    null,                               // string flashFontSize,
                    null,                               // string listFontSize,
                    ToolProfile.DefaultMaximumLineLength, // int maximumLineLength,
                    new List<ToolConfiguration>());     // List<ToolConfiguration> toolConfigurations);
                profiles = new List<ToolProfile>() { profile };
                string specs = UserProfile.GetUserOptionString(
                    "DefaultToolConfigurations",
                    ToolUtilities.DefaultToolConfigurationSpecifications);
                string message;
                ToolUtilities.AddToolProfileConfigurations(profile, specs, out message);
            }

            SetUpConfiguration(profile, ref configurationKey);
            Selector = new ToolItemSelector();
            Session = new ToolSession(
                null,
                profiles,
                profileKey,
                configurationKey,
                WorkingSet,
                Selector);
            Session.ToolType = ToolTypeCode.Flash;
            Selector.IsCustomTime = true;
            Session.SessionStart = StartTime;
            Selector.CustomNowTime = CurrentTime;
            Selector.Mode = mode;
            Selector.Reset();
        }

        public void SetUpConfiguration(ToolProfile profile, ref string configurationKey)
        {
            ToolConfiguration configuration = profile.GetToolConfiguration(configurationKey);
            if (configuration == null)
            {
                configuration = profile.GetToolConfigurationFuzzy(configurationKey, UILanguageID);
                if (configuration != null)
                    configurationKey = configuration.KeyString;
            }
            if (configuration == null)
            {
                Configuration = new ToolConfiguration(profile, configurationKey, null, null, configurationKey, 0, null);
                profile.AddToolConfiguration(Configuration);
            }
            else
                Configuration = configuration;
        }

        public void SetUpDefaultConfiguration()
        {
            ToolProfile profile = Session.ToolProfile;
            SetUpConfiguration(profile, ref DefaultConfigurationName);
        }

        public bool ValidateSelector(string selector)
        {
            try
            {
                ToolProfile.GetSelectorAlgorithmCodeFromString(selector);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void UpdateCurrentTime()
        {
            Selector.CustomNowTime = CurrentTime;
        }

        protected void UpdateCurrentTime(TimeSpan delta)
        {
            CurrentTime += delta;
            Selector.CustomNowTime = CurrentTime;
        }

        protected void UpdateCurrentTime(List<XNode> nodes, LanguageID languageID)
        {
            if (!InGenerate)
                return;

            TimeSpan duration = TimeSpan.Zero;

            if (nodes == null)
                return;

            foreach (XNode node in nodes)
                duration += EstimateDuration(node, languageID);

            UpdateCurrentTime(duration);
        }

        protected void UpdateCurrentTime(XNode node, LanguageID languageID)
        {
            if (!InGenerate)
                return;

            XElement subElement;
            XText textNode;
            TimeSpan duration = TimeSpan.Zero;

            if (node == null)
                return;

            switch (node.GetType().Name)
            {
                case "XText":
                    textNode = node as XText;
                    duration += EstimateDuration(textNode.Value, languageID);
                    break;
                case "XElement":
                    subElement = node as XElement;
                    foreach (XNode subNode in subElement.Nodes())
                        duration += EstimateDuration(subNode, languageID);
                    break;
                default:
                    break;
            }

            UpdateCurrentTime(duration);
        }

        protected void UpdateCurrentTime(string text, LanguageID languageID)
        {
            if (!InGenerate)
                return;

            TimeSpan duration = TimeSpan.Zero;
            bool isCharacterBased = LanguageLookup.IsCharacterBased(languageID);

            if (!String.IsNullOrEmpty(text))
            {
                foreach (char chr in text)
                {
                    if (!Char.IsWhiteSpace(chr))
                    {
                        if (Char.IsDigit(chr))
                            duration += TimeSpan.FromSeconds(0.25);
                        else if (Char.IsPunctuation(chr))
                            duration += TimeSpan.FromSeconds(0.4);
                        else if (isCharacterBased)
                            duration += TimeSpan.FromSeconds(0.3);
                        else
                            duration += TimeSpan.FromSeconds(0.1);
                    }
                }
            }

            UpdateCurrentTime(duration);
        }

        protected TimeSpan EstimateDuration(List<XNode> nodes, LanguageID languageID)
        {
            TimeSpan duration = TimeSpan.Zero;

            if (nodes == null)
                return duration;

            foreach (XNode node in nodes)
                duration += EstimateDuration(node, languageID);

            return duration;
        }

        protected TimeSpan EstimateDuration(XNode node, LanguageID languageID)
        {
            XElement subElement;
            XText textNode;
            TimeSpan duration = TimeSpan.Zero;

            if (node == null)
                return duration;

            switch (node.GetType().Name)
            {
                case "XText":
                    textNode = node as XText;
                    duration += EstimateDuration(textNode.Value, languageID);
                    break;
                case "XElement":
                    subElement = node as XElement;
                    foreach (XNode subNode in subElement.Nodes())
                        duration += EstimateDuration(subNode, languageID);
                    break;
                default:
                    break;
            }

            return duration;
        }

        protected TimeSpan EstimateDuration(string text, LanguageID languageID)
        {
            TimeSpan duration = TimeSpan.Zero;
            bool isCharacterBased = LanguageLookup.IsCharacterBased(languageID);

            if (!String.IsNullOrEmpty(text))
            {
                foreach (char chr in text)
                {
                    if (!Char.IsWhiteSpace(chr))
                    {
                        if (Char.IsDigit(chr))
                            duration += TimeSpan.FromSeconds(0.25);
                        else if (Char.IsPunctuation(chr))
                            duration += TimeSpan.FromSeconds(0.4);
                        else if (isCharacterBased)
                            duration += TimeSpan.FromSeconds(0.3);
                        else
                            duration += TimeSpan.FromSeconds(0.1);
                    }
                }
            }

            return duration;
        }

        private static char[] memberSeps = { '.' };

        public string EvaluateField(string fieldName)
        {
            string[] parts = fieldName.Split(memberSeps, StringSplitOptions.RemoveEmptyEntries);
            BaseObjectTitled source = null;
            BaseObjectTitled obj = null;
            int partIndex;
            int partCount = parts.Count();
            string objName = String.Empty;
            string returnValue = null;

            if (Content != null)
                source = Content;
            else if (Node != null)
                source = Node;
            else if ((SourceContents != null) && (SourceContents.Count() != 0))
                source = SourceContents.First();
            else if (SourceNode != null)
                source = SourceNode;
            else if (DocumentContent != null)
                source = DocumentContent;

            for (partIndex = 0; partIndex < partCount; partIndex++)
            {
                string part = parts[partIndex];

                switch (part.ToLower())
                {
                    case "lesson":
                        if (source is BaseObjectNode)
                            obj = source as BaseObjectNode;
                        else if (source is BaseObjectContent)
                            obj = (source as BaseObjectContent).Node;
                        objName = part;
                        break;
                    case "group":
                        if (source is BaseObjectNode)
                            obj = (source as BaseObjectNode).Parent;
                        else if (source is BaseObjectContent)
                            obj = (source as BaseObjectContent).Node.Parent;
                        objName = part;
                        break;
                    case "course":
                        if (SourceTree != null)
                            obj = SourceTree;
                        objName = part;
                        break;
                    case "plan":
                        if (SourceTree != null)
                            obj = SourceTree;
                        objName = part;
                        break;
                    case "studylist":
                        if (source is BaseObjectContent)
                            obj = source;
                        objName = part;
                        break;
                    case "title":
                        if (obj != null)
                            returnValue = obj.GetTitleString(UILanguageID);
                        else if (source != null)
                            returnValue = source.GetTitleString(UILanguageID);
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        break;
                    case "description":
                        if (obj != null)
                            returnValue = obj.GetDescriptionString(UILanguageID);
                        else if (source != null)
                            returnValue = source.GetDescriptionString(UILanguageID);
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        break;
                    case "label":
                        if (obj != null)
                            returnValue = obj.Label;
                        else if (source != null)
                            returnValue = source.Label;
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "owner":
                        if (obj != null)
                            returnValue = obj.Owner;
                        else if (source != null)
                            returnValue = source.Owner;
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "index":
                        if (obj != null)
                            returnValue = obj.Index.ToString();
                        else if (source != null)
                            returnValue = source.Index.ToString();
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "ispublic":
                        if (obj != null)
                            returnValue = (obj.IsPublic ? "public" : "private");
                        else if (source != null)
                            returnValue = (source.IsPublic ? "public" : "private");
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "package":
                        if (obj != null)
                            returnValue = obj.Package;
                        else if (source != null)
                            returnValue = source.Package;
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "creationtime":
                        if (obj != null)
                        {
                            if (UserRecord != null)
                                returnValue = UserRecord.GetLocalTime(obj.CreationTime).ToString();
                            else
                                returnValue = obj.CreationTime.ToString();
                        }
                        else if (source != null)
                        {
                            if (UserRecord != null)
                                returnValue = UserRecord.GetLocalTime(source.CreationTime).ToString();
                            else
                                returnValue = source.CreationTime.ToString();
                        }
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "modifiedtime":
                        if (obj != null)
                        {
                            if (UserRecord != null)
                                returnValue = UserRecord.GetLocalTime(obj.ModifiedTime).ToString();
                            else
                                returnValue = obj.ModifiedTime.ToString();
                        }
                        else if (source != null)
                        {
                            if (UserRecord != null)
                                returnValue = UserRecord.GetLocalTime(source.ModifiedTime).ToString();
                            else
                                returnValue = source.ModifiedTime.ToString();
                        }
                        else
                            returnValue = (!String.IsNullOrEmpty(objName) ? objName + "." : "") + part;
                        returnValue = Translate(returnValue, UILanguageID);
                        break;
                    case "target":
                    case "t":
                    case "target0":
                    case "t0":
                        if (obj != null)
                        {
                            if ((obj.TargetLanguageIDs != null) && (obj.TargetLanguageIDs.Count() != 0))
                                returnValue = obj.TargetLanguageIDs[0].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.TargetLanguageIDs != null) && (source.TargetLanguageIDs.Count() >= 1))
                                returnValue = source.TargetLanguageIDs[0].LanguageName(UILanguageID);
                        }
                        else
                            returnValue = TargetLanguageID.LanguageName(UILanguageID);
                        break;
                    case "targetalternate1":
                    case "target1":
                    case "ta1":
                    case "t1":
                        if (obj != null)
                        {
                            if ((obj.TargetLanguageIDs != null) && (obj.TargetLanguageIDs.Count() >= 2))
                                returnValue = obj.TargetLanguageIDs[1].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.TargetLanguageIDs != null) && (source.TargetLanguageIDs.Count() >= 2))
                                returnValue = source.TargetLanguageIDs[1].LanguageName(UILanguageID);
                        }
                        break;
                    case "targetalternate2":
                    case "target2":
                    case "ta2":
                    case "t2":
                        if (obj != null)
                        {
                            if ((obj.TargetLanguageIDs != null) && (obj.TargetLanguageIDs.Count() >= 3))
                                returnValue = obj.TargetLanguageIDs[2].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.TargetLanguageIDs != null) && (source.TargetLanguageIDs.Count() >= 3))
                                returnValue = source.TargetLanguageIDs[2].LanguageName(UILanguageID);
                        }
                        break;
                    case "targetalternate3":
                    case "target3":
                    case "ta3":
                    case "t3":
                        if (obj != null)
                        {
                            if ((obj.TargetLanguageIDs != null) && (obj.TargetLanguageIDs.Count() >= 4))
                                returnValue = obj.TargetLanguageIDs[3].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.TargetLanguageIDs != null) && (source.TargetLanguageIDs.Count() >= 4))
                                returnValue = source.TargetLanguageIDs[3].LanguageName(UILanguageID);
                        }
                        break;
                    case "host":
                    case "h":
                    case "host0":
                    case "h0":
                        if (obj != null)
                        {
                            if ((obj.HostLanguageIDs != null) && (obj.HostLanguageIDs.Count() != 0))
                                returnValue = obj.HostLanguageIDs[0].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.HostLanguageIDs != null) && (source.HostLanguageIDs.Count() >= 1))
                                returnValue = source.HostLanguageIDs[0].LanguageName(UILanguageID);
                        }
                        else
                            returnValue = UILanguageID.LanguageName(UILanguageID);
                        break;
                    case "host1":
                    case "h1":
                        if (obj != null)
                        {
                            if ((obj.HostLanguageIDs != null) && (obj.HostLanguageIDs.Count() >= 2))
                                returnValue = obj.HostLanguageIDs[1].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.HostLanguageIDs != null) && (source.HostLanguageIDs.Count() >= 2))
                                returnValue = source.HostLanguageIDs[1].LanguageName(UILanguageID);
                        }
                        break;
                    case "host2":
                    case "h2":
                        if (obj != null)
                        {
                            if ((obj.HostLanguageIDs != null) && (obj.HostLanguageIDs.Count() >= 3))
                                returnValue = obj.HostLanguageIDs[2].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.HostLanguageIDs != null) && (source.HostLanguageIDs.Count() >= 3))
                                returnValue = source.HostLanguageIDs[2].LanguageName(UILanguageID);
                        }
                        break;
                    case "host3":
                    case "h3":
                        if (obj != null)
                        {
                            if ((obj.HostLanguageIDs != null) && (obj.HostLanguageIDs.Count() >= 4))
                                returnValue = obj.HostLanguageIDs[3].LanguageName(UILanguageID);
                        }
                        else if (source != null)
                        {
                            if ((source.HostLanguageIDs != null) && (source.HostLanguageIDs.Count() >= 4))
                                returnValue = source.HostLanguageIDs[3].LanguageName(UILanguageID);
                        }
                        break;
                    case "ui":
                        returnValue = UILanguageID.LanguageName(UILanguageID);
                        break;
                    default:
                        {
                            BaseObjectNode node = obj as BaseObjectNode;
                            if (node == null)
                                node = source as BaseObjectNode;
                            if (node == null)
                                node = Node;
                            if (node == null)
                                node = SourceNode;
                            if (node == null)
                                node = SourceTree;
                            if (node == null)
                                return "(no node set)";
                            BaseObjectContent content = node.GetContent(part);
                            if (content == null)
                                content = node.GetFirstContentWithType(part);
                            if (content == null)
                                return "(unknown field)";
                            obj = content;
                        }
                        break;
                }
            }

            if (returnValue == null)
                returnValue = "(not defined)";

            return returnValue;
        }

        protected virtual bool HandleElementError(XElement element, string message)
        {
            XElement errorElement = HandleElementError(message);
            element.ReplaceWith(errorElement);
            return true;
        }

        protected virtual XElement HandleElementError(string message)
        {
            string text;
            if (LanguageUtilities != null)
                text = LanguageUtilities.TranslateUIString(message);
            else
                text = message;
            XElement errorElement = new XElement("div", message);
            errorElement.SetAttributeValue("class", "error");
            return errorElement;
        }

        protected virtual XElement HandleElementMessage(string message)
        {
            string text;
            if (LanguageUtilities != null)
                text = LanguageUtilities.TranslateUIString(message);
            else
                text = message;
            XElement messageElement = new XElement("div", message);
            return messageElement;
        }

        protected virtual void HandleError(string message)
        {
            string text;
            if (LanguageUtilities != null)
                text = LanguageUtilities.TranslateUIString(message);
            else
                text = message;
            Error = Error + text + "\n";
        }

        protected virtual XElement HandleDivPair(XNode node1, XNode node2)
        {
            XElement divElement = new XElement("div");
            divElement.Add(node1);
            divElement.Add(node2);
            return divElement;
        }

        public string EvaluateAttributeExpression(string input)
        {
            if (!input.Contains("$(") && !input.Contains("{"))
                return input;

            int count = input.Length;
            int index;
            int eindex;
            StringBuilder sb = new StringBuilder();
            string fullReferenceString;
            string referenceString;
            string variableName = null;
            string contentKey = null;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            string tag = String.Empty;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            MultiLanguageItem multiLanguageItem;
            LanguageItem languageItem;
            string text;
            LanguageDescriptor hostLanguageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == "Host");
            LanguageID languageID = TargetLanguageID;
            LanguageID hostLanguageID = UILanguageID;
            LanguageID mediaLanguageID = null;
            List<BaseString> optionsList = null;

            if (hostLanguageDescriptor != null)
                hostLanguageID = hostLanguageDescriptor.LanguageID;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCultureExtensionCode))
                languageID = hostLanguageID;

            for (index = 0; index < count; index++)
            {
                if ((input[index] == '\\') && ((input[index + 1] == '{') || ((input[index + 1] == '$') && (input[index + 2] == '('))))
                {
                    sb.Append(input[index + 1]);

                    if (input[index + 2] == '(')
                        sb.Append('(');
                }
                else if (input[index] == '{')
                {
                    int i = index + 1;
                    int e = i;

                    while (e < count)
                    {
                        if (input[e] == '}')
                        {
                            text = variableName = input.Substring(i, e - i);
                            variableName = MarkupTemplate.FilterVariableName(variableName);
                            multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                            if (multiLanguageItem != null)
                            {
                                languageItem = multiLanguageItem.LanguageItem(hostLanguageID);

                                if (languageItem != null)
                                    text = languageItem.Text;
                            }
                            else
                            {
                                BaseString variable = GetVariable(variableName);

                                if (variable != null)
                                    text = variable.Text;
                                else
                                {
                                    multiLanguageItem = new MultiLanguageItem(variableName, hostLanguageID, text);
                                    MarkupTemplate.AddMultiLanguageItem(multiLanguageItem);
                                }
                            }

                            if (text != null)
                                sb.Append(text);

                            break;
                        }

                        e++;
                    }

                    if (variableName == null)
                        sb.Append("(unmatched '}')");

                    index = e;
                }
                else if ((input[index] == '$') && (index < (count - 1)) && (input[index + 1] == '('))
                {
                    for (eindex = index + 2; eindex < count; eindex++)
                    {
                        if (input[eindex] == '(')
                        {
                            for (eindex = eindex + 1; eindex < count; eindex++)
                            {
                                if (input[eindex] == ')')
                                    break;
                            }
                        }
                        else if (input[eindex] == ')')
                        {
                            fullReferenceString = input.Substring(index, eindex - index + 1);
                            referenceString = input.Substring(index + 2, eindex - (index + 2));

                            ParseReference(referenceString, hostLanguageID,
                                out variableName, out contentKey, out itemIndex, out itemCount, out sentenceIndex,
                                out tag, out subType, out subSubType, out languageID, out optionsList, out errorMessage);

                            if (errorMessage != null)
                                sb.Append("(" + errorMessage + ")");
                            else if (contentKey != null)
                            {
                                if (!SubstituteKeyWord(contentKey, itemIndex, itemCount, sentenceIndex, subType, languageID, hostLanguageID, optionsList, sb))
                                {
                                    ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                                        SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                                        languageID, mediaLanguageID, subType, subSubType, optionsList,
                                        UseAudio, UsePicture, "1", false, null, String.Empty);

                                    XNode subNode = HandleContent(fullReferenceString, contentRenderParameters);
                                    sb.Append(subNode);
                                }
                            }
                            else if (variableName != null)
                            {
                                multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                                if (multiLanguageItem != null)
                                {
                                    languageItem = multiLanguageItem.LanguageItem(languageID);

                                    if (sentenceIndex == -1)
                                    {
                                        if (languageItem != null)
                                            text = languageItem.Text;
                                        else
                                            text = multiLanguageItem.Text(0);
                                    }
                                    else
                                    {
                                        if (languageItem != null)
                                            text = languageItem.GetRunText(sentenceIndex);
                                        else if (multiLanguageItem.LanguageItems != null)
                                            text = multiLanguageItem.LanguageItem(0).GetRunText(sentenceIndex);
                                        else
                                            text = null;
                                    }

                                    if (text != null)
                                        sb.Append(text);
                                    else
                                        sb.Append("(no translation:  \"" + variableName + "\")");
                                }
                                else
                                {
                                    BaseString variable = GetVariable(variableName);

                                    if (variable != null)
                                        text = variable.Text;
                                    else
                                    {
                                        multiLanguageItem = new MultiLanguageItem(variableName, hostLanguageID, variableName);
                                        MarkupTemplate.AddMultiLanguageItem(multiLanguageItem);
                                        text = variableName;
                                    }
                                }
                            }

                            index = eindex;
                        }
                    }
                }
                else
                    sb.Append(input[index]);
            }

            return sb.ToString();
        }

        public bool SubstituteKeyWord(string contentKey, int itemIndex, int itemcount, int sentenceIndex,
            string subType, LanguageID languageID, LanguageID uiLanguageID, List<BaseString> optionsList, StringBuilder sb)
        {
            bool nodeWasReplaced = true;

            if (String.IsNullOrEmpty(contentKey))
                return false;

            switch (contentKey.ToLower())
            {
                case "ui":
                case "host":
                case "host1":
                case "host2":
                case "host3":
                case "target":
                case "target1":
                case "target2":
                case "target3":
                case "targetalternate1":
                case "targetalternate2":
                case "targetalternate3":
                    {
                        if (String.IsNullOrEmpty(subType))
                            subType = String.Empty;
                        LanguageID substituteLanguageID;
                        ParseLanguageID(contentKey, out substituteLanguageID);
                        string languageString;
                        switch (subType.ToLower())
                        {
                            case "languagecode":
                                languageString = substituteLanguageID.LanguageCode;
                                break;
                            case "culturecode":
                                languageString = substituteLanguageID.CultureCode;
                                break;
                            case "extensioncode":
                                languageString = substituteLanguageID.ExtensionCode;
                                break;
                            case "languagecultureextensioncode":
                                languageString = substituteLanguageID.LanguageCultureExtensionCode;
                                break;
                            case "language":
                                languageString = substituteLanguageID.Language;
                                if (LanguageUtilities != null)
                                    languageString = LanguageUtilities.TranslateUIString(languageString);
                                break;
                            case "languageonly":
                                languageString = substituteLanguageID.LanguageOnly;
                                if (LanguageUtilities != null)
                                    languageString = LanguageUtilities.TranslateUIString(languageString);
                                break;
                            case "languagecultureextension":
                                languageString = substituteLanguageID.LanguageCultureExtension;
                                if (LanguageUtilities != null)
                                    languageString = LanguageUtilities.TranslateUIString(languageString);
                                break;
                            case "media":
                                languageString = substituteLanguageID.MediaLanguageID().Language;
                                if (LanguageUtilities != null)
                                    languageString = LanguageUtilities.TranslateUIString(languageString);
                                break;
                            case "medialanguagecode":
                                languageString = substituteLanguageID.MediaLanguageID().LanguageCode;
                                break;
                            case "":
                            case null:
                            default:
                                languageString = substituteLanguageID.Language;
                                if (LanguageUtilities != null)
                                    languageString = LanguageUtilities.TranslateUIString(languageString);
                                break;
                        }
                        sb.Append(languageString);
                    }
                    break;
                default:
                    nodeWasReplaced = false;
                    break;
            }

            return nodeWasReplaced;
        }

        protected bool GetContent(string contentKey, int studyItemIndex, out BaseObjectContent content, out ContentStudyList studyList,
            out MultiLanguageItem studyItem)
        {
            content = Content;
            studyList = StudyList;
            studyItem = StudyItem;

            if (Node == null)
                return false;

            if (!String.IsNullOrEmpty(contentKey))
            {
                content = Node.GetContent(contentKey);

                if (content != null)
                    studyList = content.ContentStorageStudyList;

                if ((studyItemIndex != -1) && (studyList != null))
                    studyItem = studyList.GetStudyItemIndexed(studyItemIndex);

                if (content != null)
                    return true;
            }

            return false;
        }

        protected void ParseContent(string str, LanguageID defaultLanguageID,
            out string contentKey, out int itemIndex, out int itemCount, out int sentenceIndex, out string tag, out string subType,
            out string subSubType, out LanguageID languageID, out List<BaseString> optionList, out string errorMessage)
        {
            string variableName = null;
            ParseReference(str, defaultLanguageID, out variableName, out contentKey, out itemIndex, out itemCount,
                out sentenceIndex, out tag, out subType, out subSubType, out languageID, out optionList, out errorMessage);
            if (contentKey.ToLower() == "this")
                contentKey = DocumentContent.KeyString;
        }

        public void ParseReference(string str, LanguageID defaultLanguageID,
            out string variableName, out string contentKey, out int itemIndex, out int itemCount, out int sentenceIndex,
            out string tag, out string subType, out string subSubType, out LanguageID languageID, out List<BaseString> optionList,
            out string errorMessage)
        {
            variableName = null;
            contentKey = null;
            itemIndex = -1;
            itemCount = -1;
            sentenceIndex = -1;
            tag = String.Empty;
            languageID = defaultLanguageID;
            subType = null;
            subSubType = null;
            optionList = null;
            errorMessage = null;

            int i, e;
            int count = str.Length;
            string tmpString;
            int sentenceCount = -1;

            for (i = 0; i < count; i++)
            {
                if (str[i] == '{')
                {
                    e = ++i;

                    while (e < count)
                    {
                        if (str[e] == '}')
                        {
                            variableName = str.Substring(i, e - i);
                            break;
                        }

                        e++;
                    }

                    if (variableName == null)
                        errorMessage = "Unmatched '\"'";

                    i = e;
                }
                else if ((str[i] == '[') || (str[i] == '.') || (str[i] == '('))
                {
                    if (variableName == null)
                        contentKey = str.Substring(0, i);
                    break;
                }
            }

            if ((variableName == null) && (contentKey == null))
            {
                if (i != 0)
                    contentKey = str;
                else
                    errorMessage = "Missing source content key: " + str;
            }

            if ((i != count) && (str[i] == '['))
                ParseContentIndex(str, ref i, count, "item", out itemIndex, out itemCount, ref tag, ref errorMessage);

            if ((i != count) && (str[i] == '['))
                ParseContentIndex(str, ref i, count, "sentence", out sentenceIndex, out sentenceCount, ref tag, ref errorMessage);

            LanguageID tmpLanguageID = null;

            while ((i != count) && (str[i] == '.'))
            {
                i++;

                tmpString = null;

                for (e = i; e < count; e++)
                {
                    switch (str[e])
                    {
                        case '.':
                        case '[':
                        case '(':
                            tmpString = str.Substring(i, e - i);
                            goto Skip;
                        default:
                            break;
                    }
                }

            Skip:
                if (tmpString == null)
                    tmpString = str.Substring(i);

                i = e;

                if (tmpLanguageID == null)
                {
                    if ((tmpString == "All") || (tmpString == "Any"))
                    {
                        languageID = tmpLanguageID = LanguageLookup.Any;
                        tmpString = null;
                    }
                    else if ((tmpString == "Target") && (TargetLanguageID != null))
                    {
                        languageID = tmpLanguageID = TargetLanguageID;
                        tmpString = null;
                    }
                    else if ((tmpString == "UI") && (UILanguageID != null))
                    {
                        languageID = tmpLanguageID = UILanguageID;
                        tmpString = null;
                    }
                    else if (LanguageDescriptor.Names.Contains(tmpString))
                    {
                        if (LanguageDescriptors != null)
                        {
                            LanguageDescriptor languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == tmpString);

                            if (languageDescriptor != null)
                                languageID = tmpLanguageID = languageDescriptor.LanguageID;
                            else
                                languageID = tmpLanguageID = new LanguageID(tmpString);
                        }
                        else
                            languageID = tmpLanguageID = new LanguageID(tmpString);

                        tmpString = null;
                    }

                    if (tmpLanguageID == null)
                    {
                        tmpLanguageID = LanguageLookup.GetLanguageIDCheck(tmpString);

                        if (tmpLanguageID != null)
                        {
                            languageID = tmpLanguageID;
                            tmpString = null;
                        }
                    }
                }

                if (tmpString != null)
                {
                    if (subType == null)
                    {
                        subType = tmpString;
                        BaseString variable = GetVariable(variableName);
                        if (variable != null)
                        {
                            contentKey = variable.Text;
                            variableName = null;
                        }
                    }
                    else if (subSubType == null)
                        subSubType = tmpString;
                    else
                        errorMessage = "Unknown trailing string \"" + tmpString + "\" in reference: " + str;
                }
            }

            if ((i != count) && (str[i] == '['))
                ParseContentIndex(str, ref i, count, "sentence", out sentenceIndex, out sentenceCount, ref tag, ref errorMessage);

            if ((i != count) && (str[i] == '('))
            {
                string message;
                if (!ParseRenderOptions(str, ref i, count, out optionList, out message))
                    errorMessage = message;
            }
        }

        public bool ParseContentIndex(string str, ref int i, int count, string label,
            out int index, out int outCount, ref string tag, ref string errorMessage)
        {
            string indexStr = null;
            int indexStart = i + 1;

            index = -1;
            outCount = 1;

            for (i = indexStart; i < count; i++)
            {
                if ((str[i] == ']') || (str[i] == ','))
                {
                    indexStr = str.Substring(indexStart, i - indexStart);
                    break;
                }
            }

            if (indexStr == null)
            {
                errorMessage = "Missing source " + label + " index subscript closer ']': " + str;
                return false;
            }

            if (indexStr.StartsWith("{") && indexStr.EndsWith("}"))
            {
                string varName = indexStr.Substring(1, indexStr.Length - 2);
                BaseString variable = GetVariable(varName);

                if (variable != null)
                    indexStr = variable.Text;
            }

            if (IsInteger(indexStr))
                ParseInteger(indexStr, out index);
            else
                tag = indexStr;

            if ((i < count) && (str[i] == ','))
            {
                i++;

                while ((i < count) && char.IsWhiteSpace(str[i]))
                    i++;

                indexStr = null;
                indexStart = i;

                for (; i < count; i++)
                {
                    if (str[i] == ']')
                    {
                        indexStr = str.Substring(indexStart, i - indexStart);
                        break;
                    }
                }

                if (indexStr == null)
                {
                    errorMessage = "Missing source " + label + " count subscript closer ']': " + str;
                    return false;
                }

                if (indexStr.StartsWith("{") && indexStr.EndsWith("}"))
                {
                    string varName = indexStr.Substring(1, indexStr.Length - 2);
                    BaseString variable = GetVariable(varName);

                    if (variable != null)
                        indexStr = variable.Text;
                }

                if (IsInteger(indexStr))
                    ParseInteger(indexStr, out outCount);
                else
                    tag = indexStr;
            }

            if ((i < count) && (str[i] == ']'))
                i++;

            return true;
        }

        public bool ParseRenderOptions(string str, ref int i, int count, out List<BaseString> optionList, out string errorMessage)
        {
            string optionsStr = null;
            int optionsStart = i + 1;

            optionList = null;
            errorMessage = null;

            for (i = optionsStart; i < count; i++)
            {
                if (str[i] == ')')
                {
                    optionsStr = str.Substring(optionsStart, i - optionsStart);
                    i++;
                    break;
                }
            }

            if (optionsStr == null)
                return false;

            if (optionsStr.StartsWith("{") && optionsStr.EndsWith("}"))
            {
                string varName = optionsStr.Substring(1, optionsStr.Length - 2);
                BaseString variable = GetVariable(varName);

                if (variable != null)
                    optionsStr = variable.Text;
            }

            List<BaseString> rawOptionList = BaseString.GetBaseStringListFromString(optionsStr);

            if (rawOptionList.Count() != 0)
            {
                foreach (BaseString option in rawOptionList)
                {
                    if (!ParseRenderOption(option.KeyString, option.Text, ref optionList))
                    {
                        errorMessage = "Unknown option: " + option.KeyString;
                        return false;
                    }
                }
            }
            else
                optionList = new List<BaseString>();

            return true;
        }

        public bool ParseRenderOption(string optionName, string optionValue, ref List<BaseString> optionList)
        {
            BaseString option = null;

            if (optionList == null)
                optionList = new List<BaseString>();

            switch (optionName.ToLower())
            {
                case "languageformat":
                    option = new BaseString("LanguageFormat", optionValue);
                    break;
                case "mixed":
                    option = new BaseString("LanguageFormat", "Mixed");
                    break;
                case "separate":
                    option = new BaseString("LanguageFormat", "Separate");
                    break;
                case "rowformat":
                    option = new BaseString("RowFormat", optionValue);
                    break;
                case "paragraphs":
                    option = new BaseString("RowFormat", "Paragraphs");
                    break;
                case "sentences":
                    option = new BaseString("RowFormat", "Sentences");
                    break;
                case "displayformat":
                    option = new BaseString("DisplayFormat", optionValue);
                    break;
                case "table":
                    option = new BaseString("DisplayFormat", "Table");
                    break;
                case "list":
                    option = new BaseString("DisplayFormat", "List");
                    break;
                case "showannotations":
                    option = new BaseString("ShowAnnotations", optionValue);
                    break;
                case "hideannotations":
                case "noannotations":
                    option = new BaseString("ShowAnnotations", "false");
                    break;
                case "showwords":
                    option = new BaseString("ShowWords", optionValue);
                    break;
                case "hidewords":
                case "nowords":
                    option = new BaseString("ShowWords", "false");
                    break;
                case "useaudio":
                    option = new BaseString("UseAudio", optionValue);
                    break;
                case "noaudio":
                    option = new BaseString("UseAudio", "false");
                    break;
                case "usepicture":
                    option = new BaseString("UsePicture", optionValue);
                    break;
                case "nopicture":
                    option = new BaseString("UsePicture", "false");
                    break;
                case "usemedia":
                    option = new BaseString("UseAudio", optionValue);
                    optionList.Add(option);
                    option = new BaseString("UsePicture", optionValue);
                    break;
                case "nomedia":
                    option = new BaseString("UseAudio", "false");
                    optionList.Add(option);
                    option = new BaseString("UsePicture", "false");
                    break;
                case "elementtype":
                    option = new BaseString("ElementType", optionValue);
                    break;
                case "style":
                    option = new BaseString("Style", optionValue);
                    break;
                case "class":
                    option = new BaseString("Class", optionValue);
                    break;
                case "autoplay":
                    option = new BaseString("AutoPlay", (String.IsNullOrEmpty(optionValue) ? "true" : optionValue));
                    break;
                case "player":
                    option = new BaseString("Player", optionValue);
                    break;
                case "noplayer":
                    option = new BaseString("Player", "false");
                    break;
                case "playerid":
                    option = new BaseString("PlayerID", optionValue);
                    break;
                case "showvolume":
                    option = new BaseString("ShowVolume", (String.IsNullOrEmpty(optionValue) ? "false" : optionValue));
                    break;
                case "showtimeslider":
                    option = new BaseString("ShowTimeSlider", (String.IsNullOrEmpty(optionValue) ? "true" : optionValue));
                    break;
                case "showtimetext":
                    option = new BaseString("ShowTimeText", (String.IsNullOrEmpty(optionValue) ? "true" : optionValue));
                    break;
                case "endofmedia":
                    option = new BaseString("EndOfMedia", optionValue);
                    break;
                case "speed":
                    option = new BaseString("Speed", optionValue);
                    break;
                case "session":
                case "sessionindex":
                    option = new BaseString("SessionIndex", optionValue);
                    break;
                case "tool":
                case "tooltype":
                    option = new BaseString("ToolType", optionValue);
                    break;
                case "profilename":
                case "profile":
                    option = new BaseString("ProfileName", optionValue);
                    break;
                case "configurationkey":
                case "configuration":
                    option = new BaseString("ConfigurationKey", optionValue);
                    break;
                case "mode":
                    option = new BaseString("Mode", optionValue);
                    break;
                default:
                    if (optionList.Count() == 0)
                        optionList = null;
                    return false;
            }

            optionList.Add(option);

            return true;
        }

        public void ParseLanguageID(string str, out LanguageID languageID)
        {
            if (!ParseLanguageIDCheck(str, out languageID))
                languageID = UserProfile.UILanguageID;
        }

        public void ParseMediaLanguageID(string str, out LanguageID languageID)
        {
            ParseLanguageID(str, out languageID);
            languageID.CultureCode = null;
            languageID.ExtensionCode = null;
        }

        protected bool ParseLanguageIDCheck(string str, out LanguageID languageID)
        {
            int index = 0;

            languageID = null;

            if (String.IsNullOrEmpty(str))
                return false;

            while (char.IsDigit(str[str.Length - 1]))
            {
                string digit = str.Substring(str.Length - 1);
                int value = Convert.ToInt32(digit);
                index = (index * 10) + value;
                str = str.Substring(0, str.Length - 1);
            }

            switch (str.ToLower())
            {
                case "host":
                case "h":
                    languageID = HostLanguageID;
                    str = "Host";
                    break;
                case "target":
                case "t":
                case "targetalternate":
                case "ta":
                    languageID = TargetLanguageID;
                    str = "Target";
                    break;
                case "ui":
                    languageID = UILanguageID;
                    str = "UI";
                    return true;
                default:
                    languageID = LanguageLookup.GetLanguageIDNoAdd(str);
                    if (languageID != null)
                        return true;
                    return false;
            }

            if (languageID != null)
                return true;

            if (LanguageDescriptors != null)
            {
                List<LanguageDescriptor> languageDescriptors = LanguageDescriptors.Where(x => x.Name == str).ToList();

                if (index < languageDescriptors.Count())
                {
                    LanguageDescriptor languageDescriptor = languageDescriptors[index];

                    if ((languageDescriptor != null) && languageDescriptor.Show)
                    {
                        languageID = languageDescriptor.LanguageID;
                        return true;
                    }
                }
            }

            return false;
        }

        public void ParseInteger(string str, out int value)
        {
            if (IsInteger(str))
                value = Convert.ToInt32(str);
            else
                value = 0;
        }

        public void ParseBoolean(string str, out bool value)
        {
            if (str == null)
                str = String.Empty;

            switch (str.ToLower())
            {
                case "true":
                case "yes":
                case "on":
                    value = true;
                    break;
                default:
                    value = false;
                    break;
            }
        }

        protected void ParseVariableName(string str, out MultiLanguageItem multiLanguageItem)
        {
            multiLanguageItem = MarkupTemplate.MultiLanguageItem(str);
        }

        protected void ParseValues(string str, string[] seps, out string[] values)
        {
            values = str.Split(seps, StringSplitOptions.RemoveEmptyEntries);
        }

        protected bool IsInteger(string str)
        {
            return ObjectUtilities.IsNumberString(str);
        }

        public string Translate(string text, LanguageID languageID)
        {
            string returnValue = text;

            if (LanguageUtilities != null)
                returnValue = LanguageUtilities.TranslateString(text, languageID);

            return returnValue;
        }

        public BaseString GetVariable(string key)
        {
            if (!String.IsNullOrEmpty(key))
                return Variables.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public BaseString GetVariableIndexed(int index)
        {
            if ((index >= 0) && (index < Variables.Count()))
                return Variables.ElementAt(index);
            return null;
        }

        public string GetVariableText(string key)
        {
            BaseString variable = GetVariable(key);
            if (variable != null)
                return variable.Text;
            return null;
        }

        public string VariableTextIndexed(int index, LanguageID languageID)
        {
            BaseString variable = GetVariableIndexed(index);
            if (variable != null)
                return variable.Text;
            return null;
        }

        public bool AddVariable(BaseString variable)
        {
            if (Variables == null)
                Variables = new List<BaseString>(1) { variable };
            else
                Variables.Add(variable);
            return true;
        }

        public bool DeleteVariable(BaseString variable)
        {
            if (Variables.Remove(variable))
                return true;
            return false;
        }

        public bool DeleteVariableKey(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                BaseString variable = GetVariable(key);
                if (variable != null)
                {
                    Variables.Remove(variable);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteVariableIndexed(int index)
        {
            if ((index >= 0) && (index < Variables.Count()))
            {
                Variables.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void DeleteAllVariables()
        {
            Variables.Clear();
        }

        public int VariableCount()
        {
            return Variables.Count();
        }

        public int SourceTreeKey
        {
            get
            {
                if (SourceTree != null)
                    return SourceTree.KeyInt;
                return -1;
            }
        }

        public int SourceNodeKey
        {
            get
            {
                if (SourceNode != null)
                    return SourceNode.KeyInt;
                return -1;
            }
        }
    }
}

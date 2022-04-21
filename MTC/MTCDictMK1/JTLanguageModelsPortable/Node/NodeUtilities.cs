using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    public partial class NodeUtilities : ControllerUtilities
    {
        private DictionaryCache _RemoteDictionaryEntryCache = null;
        private int _ShowAllLanguageItems;
        public bool ShowEmptyContentItemsInTreeView;

        public NodeUtilities(IMainRepository repositories, IApplicationCookies cookies,
                UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
                LanguageUtilities languageUtilities)
            : base(repositories, cookies, userRecord, userProfile, translator, languageUtilities)
        {
            _ShowAllLanguageItems = 0;
        }

        public NodeUtilities(NodeUtilities other) : base(other)
        {
            _ShowAllLanguageItems = other._ShowAllLanguageItems;
        }

        public NodeUtilities()
        {
            _ShowAllLanguageItems = 0;
        }

        public static NodeUtilities CopyNodeUtilities(NodeUtilities nodeUtilities)
        {
            if (nodeUtilities == null)
                return null;

            return new NodeUtilities(nodeUtilities);
        }

        public DictionaryCache RemoteDictionaryEntryCache
        {
            get
            {
                if (_RemoteDictionaryEntryCache == null)
                    _RemoteDictionaryEntryCache = new DictionaryCache();
                return _RemoteDictionaryEntryCache;
            }
            set
            {
                _RemoteDictionaryEntryCache = value;
            }
        }

        public void ClearMessageAndError()
        {
            Error = null;
            Message = null;
        }

        public void ClearMessage()
        {
            Message = null;
        }

        public void SetChangesSavedMessage()
        {
            Message = S("Changes saved.");
        }

        public void SetNothingChangedMessage()
        {
            Message = S("Nothing changed.");
        }

        public string BrowseTargetLanguageCode { get; set; }
        public string BrowseHostLanguageCode { get; set; }

        public bool ShowAllLanguageItems
        {
            get
            {
                if ((_ShowAllLanguageItems == 0) && (UserProfile != null))
                {
                    if (UserProfile.GetUserOptionFlag("ShowAllLanguageItems", false))
                        _ShowAllLanguageItems = 1;
                    else
                        _ShowAllLanguageItems = 2;
                }

                switch (_ShowAllLanguageItems)
                {
                    case 0:
                    case 1:
                    default:
                        return true;
                    case 2:
                        return false;
                }
            }
            set
            {
                if (value)
                    _ShowAllLanguageItems = 1;
                else
                    _ShowAllLanguageItems = 2;
            }
        }

        public bool IsShowContent(BaseObjectContent content)
        {
            if (ShowAllLanguageItems)
                return true;

            if (content != null)
            {
                if (String.IsNullOrEmpty(BrowseTargetLanguageCode))
                    BrowseTargetLanguageCode = "(target languages)";

                if (String.IsNullOrEmpty(BrowseHostLanguageCode))
                    BrowseHostLanguageCode = "(host languages)";

                if (content.IsVisible(UserProfile, BrowseTargetLanguageCode, BrowseHostLanguageCode))
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public string ComposeTreeCacheString(BaseObjectNodeTree tree)
        {
            string treeCacheKey;
            string treeKey = tree.KeyString;
            string automatedMarkupTemplateFlag = (AutomatedMarkupTemplateKey <= 0 ? "_withoutautomated" : "_withautomated");
            bool showEmptyContentItemsInTreeView = UserProfile.GetUserOptionFlag("ShowEmptyContentItemsInTreeView", true);
            string showEmptyContentItemsInTreeViewFlag = (showEmptyContentItemsInTreeView ? "_showempty" : "_hideempty");

            if (ShowAllLanguageItems)
                treeCacheKey = treeKey;
            else
            {
                if (String.IsNullOrEmpty(BrowseTargetLanguageCode))
                    BrowseTargetLanguageCode = "(target languages)";

                if (String.IsNullOrEmpty(BrowseHostLanguageCode))
                    BrowseHostLanguageCode = "(host languages)";

                string languagesKey = UserProfile.ExpandLanguageCode(BrowseTargetLanguageCode);
                languagesKey = languagesKey.Replace("|", String.Empty);
                languagesKey = languagesKey.Replace(")", String.Empty);
                languagesKey = languagesKey.Replace("(", String.Empty);
                languagesKey = languagesKey.Replace("-", String.Empty);
                languagesKey = languagesKey.Replace(" ", String.Empty);
                treeCacheKey = treeKey + "_" + languagesKey;
            }

            treeCacheKey += automatedMarkupTemplateFlag + showEmptyContentItemsInTreeViewFlag;

            return treeCacheKey;
        }

        public bool PropagateLanguagesHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = PropagateLanguagesNoUpdate(tree, node);

            if (!UpdateTree(tree, false, false))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateLanguagesNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = true;

            if (!PropagateLanguagesChildrenAndContentHelper(tree, node))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateLanguagesChildrenAndContentHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            BaseObjectNode child;
            bool returnValue = true;

            if (tree == null)
                return true;

            if (node == null)
                node = tree;

            node.ResolveReferences(Repositories, false, false);

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (child != null)
                    {
                        child.CopyLanguages(node);
                        returnValue = PropagateLanguagesChildrenAndContentHelper(child.Tree, child);
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + key.ToString() + "\n";
                        returnValue = false;
                    }
                }
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentList)
                {
                    switch (content.ContentClass)
                    {
                        case ContentClassType.StudyList:
                        case ContentClassType.DocumentItem:
                            content.ResolveReferences(Repositories, false, false);
                            content.CopyLanguages(node);
                            UpdateContentStorageCheck(content, false);
                            break;
                        case ContentClassType.MediaList:
                        case ContentClassType.MediaItem:
                        default:
                            break;
                    }
                }
            }

            return returnValue;
        }

        public bool PropagateVisibilityHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = PropagateVisibilityNoUpdate(tree, node);

            if (!UpdateTree(tree, false, false))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateVisibilityNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = true;

            if (!PropagateVisibilityChildrenAndContentHelper(tree, node))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateVisibilityChildrenAndContentHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            BaseObjectNode child;
            bool returnValue = true;

            if (tree == null)
                return true;

            if (node == null)
                node = tree;

            node.ResolveReferences(Repositories, false, false);

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (child != null)
                    {
                        child.IsPublic = node.IsPublic;
                        returnValue = PropagateVisibilityChildrenAndContentHelper(child.Tree, child);
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + key.ToString() + "\n";
                        returnValue = false;
                    }
                }
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentList)
                {
                    switch (content.ContentClass)
                    {
                        case ContentClassType.StudyList:
                        case ContentClassType.DocumentItem:
                            content.IsPublic = node.IsPublic;
                            break;
                        case ContentClassType.MediaList:
                        case ContentClassType.MediaItem:
                        default:
                            break;
                    }
                }
            }

            return returnValue;
        }

        public bool PropagateOwnerHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = PropagateOwnerNoUpdate(tree, node);

            if (!UpdateTree(tree, false, false))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateOwnerNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = true;

            if (!PropagateOwnerChildrenAndContentHelper(tree, node))
                returnValue = false;

            return returnValue;
        }

        public bool PropagateOwnerChildrenAndContentHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            BaseObjectNode child;
            bool returnValue = true;

            if (tree == null)
                return true;

            if (node == null)
                node = tree;

            node.ResolveReferences(Repositories, false, false);

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (child != null)
                    {
                        child.Owner = node.Owner;
                        returnValue = PropagateOwnerChildrenAndContentHelper(child.Tree, child);
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + key.ToString() + "\n";
                        returnValue = false;
                    }
                }
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentList)
                    content.Owner = node.Owner;
            }

            return returnValue;
        }

        public bool StringExistsInPriorLessonsCheck(
            BaseObjectContent content,
            string str,
            LanguageID languageID,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem)
        {
            bool isDone = false;
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            BaseObjectNode node = content.Node;
            BaseObjectNodeTree tree = content.Tree;

            if ((tree == null) || (node == null))
                return returnValue;

            if (tree.ChildCount() == 0)
                return returnValue;

            foreach (BaseObjectNode testNode in tree.Children)
            {
                if (testNode == node)
                    break;

                returnValue = StringExistsInPriorLessonsRecurse(
                    tree,
                    node,
                    content,
                    str,
                    languageID,
                    testNode,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        protected bool StringExistsInPriorLessonsRecurse(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            string str,
            LanguageID languageID,
            BaseObjectNode currentNode,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem,
            ref bool isDone)
        {
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            if (currentNode == null)
                return returnValue;

            if (node == currentNode)
            {
                isDone = true;
                return returnValue;
            }

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent testContent in currentNode.ContentList)
            {
                if ((testContent.ContentClass != contentClass) ||
                        (testContent.ContentType != contentType) ||
                        (testContent.ContentSubType != contentSubType))
                    continue;

                testContent.ResolveReferences(Repositories, false, false);

                ContentStudyList testStudyList = testContent.ContentStorageStudyList;

                if ((testStudyList != null) && (testStudyList.StudyItemCount() != 0))
                {
                    foreach (MultiLanguageItem testStudyItem in testStudyList.StudyItems)
                    {
                        if (testStudyItem.IsCaseInsensitiveTextMatch(str, languageID))
                        {
                            returnValue = true;
                            oldContent = testContent;
                            oldStudyItem = testStudyItem;
                            isDone = true;
                            break;
                        }
                    }
                }
            }

            if (!isDone && (currentNode.ChildCount() != 0))
            {
                foreach (BaseObjectNode testNode in currentNode.Children)
                {
                    if (testNode == node)
                        break;

                    returnValue = StringExistsInPriorLessonsRecurse(
                        tree,
                        node,
                        content,
                        str,
                        languageID,
                        testNode,
                        out oldContent,
                        out oldStudyItem,
                        ref isDone);

                    if (returnValue)
                        break;
                }
            }

            return returnValue;
        }

        public bool TextExistsInPriorLessonsCheck(
            BaseObjectContent content,
            MultiLanguageString str,
            List<LanguageID> languageIDs,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem)
        {
            bool isDone = false;
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            BaseObjectNode node = content.Node;
            BaseObjectNodeTree tree = content.Tree;

            if ((tree == null) || (node == null))
                return returnValue;

            if (tree.ChildCount() == 0)
                return returnValue;

            foreach (BaseObjectNode testNode in tree.Children)
            {
                if (testNode == node)
                    break;

                returnValue = TextExistsInPriorLessonsRecurse(
                    tree,
                    node,
                    content,
                    str,
                    languageIDs,
                    testNode,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        protected bool TextExistsInPriorLessonsRecurse(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            MultiLanguageString str,
            List<LanguageID> languageIDs,
            BaseObjectNode currentNode,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem,
            ref bool isDone)
        {
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            if (currentNode == null)
                return returnValue;

            if (node == currentNode)
            {
                isDone = true;
                return returnValue;
            }

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent testContent in currentNode.ContentList)
            {
                if ((testContent.ContentClass != contentClass) ||
                        (testContent.ContentType != contentType) ||
                        (testContent.ContentSubType != contentSubType))
                    continue;

                testContent.ResolveReferences(Repositories, false, false);

                ContentStudyList testStudyList = testContent.ContentStorageStudyList;

                if ((testStudyList != null) && (testStudyList.StudyItemCount() != 0))
                {
                    foreach (MultiLanguageItem testStudyItem in testStudyList.StudyItems)
                    {
                        if (testStudyItem.IsCaseInsensitiveTextMatch(str, languageIDs))
                        {
                            returnValue = true;
                            oldContent = testContent;
                            oldStudyItem = testStudyItem;
                            isDone = true;
                            break;
                        }
                    }
                }
            }

            if (!isDone && (currentNode.ChildCount() != 0))
            {
                foreach (BaseObjectNode testNode in currentNode.Children)
                {
                    if (testNode == node)
                        break;

                    returnValue = TextExistsInPriorLessonsRecurse(
                        tree,
                        node,
                        content,
                        str,
                        languageIDs,
                        testNode,
                        out oldContent,
                        out oldStudyItem,
                        ref isDone);

                    if (returnValue)
                        break;
                }
            }

            return returnValue;
        }

        public bool StringExistsInPriorContentCheck(
            BaseObjectContent content,
            string str,
            LanguageID languageID,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem)
        {
            bool isDone = false;
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            BaseObjectNode node = content.Node;

            if (node == null)
                return returnValue;

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent childContent in node.ContentChildren)
            {
                if (childContent == content)
                    break;

                if ((childContent.ContentClass != contentClass) ||
                        (childContent.ContentType != contentType) ||
                        (childContent.ContentSubType != contentSubType))
                    continue;

                returnValue = StringExistsInPriorContentRecurse(
                    node,
                    content,
                    str,
                    languageID,
                    childContent,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        protected bool StringExistsInPriorContentRecurse(
            BaseObjectNode node,
            BaseObjectContent content,
            string str,
            LanguageID languageID,
            BaseObjectContent currentContent,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem,
            ref bool isDone)
        {
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            if (currentContent == null)
                return returnValue;

            if (currentContent == content)
            {
                isDone = true;
                return returnValue;
            }

            currentContent.ResolveReferences(Repositories, false, false);

            ContentStudyList testStudyList = currentContent.ContentStorageStudyList;

            if (testStudyList.StudyItemCount() != 0)
            {
                foreach (MultiLanguageItem testStudyItem in testStudyList.StudyItems)
                {
                    if (testStudyItem.IsCaseInsensitiveTextMatch(str, languageID))
                    {
                        returnValue = true;
                        oldContent = currentContent;
                        oldStudyItem = testStudyItem;
                        isDone = true;
                        break;
                    }
                }
            }

            if (!currentContent.HasContentChildren())
                return returnValue;

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent childContent in currentContent.ContentChildren)
            {
                if ((childContent.KeyString == currentContent.KeyString) || (childContent.KeyString == content.KeyString))
                    continue;

                if ((childContent.ContentClass != contentClass) ||
                        (childContent.ContentType != contentType) ||
                        (childContent.ContentSubType != contentSubType))
                    continue;

                returnValue = StringExistsInPriorContentRecurse(
                    node,
                    content,
                    str,
                    languageID,
                    childContent,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        public bool TextExistsInPriorContentCheck(
            BaseObjectContent content,
            MultiLanguageString str,
            List<LanguageID> languageIDs,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem)
        {
            bool isDone = false;
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            BaseObjectNode node = content.Node;

            if (node == null)
                return returnValue;

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent childContent in node.ContentChildren)
            {
                if (childContent == content)
                    break;

                if ((childContent.ContentClass != contentClass) ||
                        (childContent.ContentType != contentType) ||
                        (childContent.ContentSubType != contentSubType))
                    continue;

                returnValue = TextExistsInPriorContentRecurse(
                    node,
                    content,
                    str,
                    languageIDs,
                    childContent,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        protected bool TextExistsInPriorContentRecurse(
            BaseObjectNode node,
            BaseObjectContent content,
            MultiLanguageString str,
            List<LanguageID> languageIDs,
            BaseObjectContent currentContent,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem,
            ref bool isDone)
        {
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            if (currentContent == null)
                return returnValue;

            if (currentContent == content)
            {
                isDone = true;
                return returnValue;
            }

            currentContent.ResolveReferences(Repositories, false, false);

            ContentStudyList testStudyList = currentContent.ContentStorageStudyList;

            if (testStudyList.StudyItemCount() != 0)
            {
                foreach (MultiLanguageItem testStudyItem in testStudyList.StudyItems)
                {
                    if (testStudyItem.IsCaseInsensitiveTextMatch(str, languageIDs))
                    {
                        returnValue = true;
                        oldContent = currentContent;
                        oldStudyItem = testStudyItem;
                        isDone = true;
                        break;
                    }
                }
            }

            if (!currentContent.HasContentChildren())
                return returnValue;

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent childContent in currentContent.ContentChildren)
            {
                if ((childContent.KeyString == currentContent.KeyString) || (childContent.KeyString == content.KeyString))
                    continue;

                if ((childContent.ContentClass != contentClass) ||
                        (childContent.ContentType != contentType) ||
                        (childContent.ContentSubType != contentSubType))
                    continue;

                returnValue = TextExistsInPriorContentRecurse(
                    node,
                    content,
                    str,
                    languageIDs,
                    childContent,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        public bool StudyItemExistsInPriorLessonsCheck(
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem)
        {
            bool isDone = false;
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            BaseObjectNode node = content.Node;
            BaseObjectNodeTree tree = content.Tree;

            if ((tree == null) || (node == null))
                return returnValue;

            if (tree.ChildCount() == 0)
                return returnValue;

            foreach (BaseObjectNode testNode in tree.Children)
            {
                if (testNode == node)
                    break;

                returnValue = StudyItemExistsInPriorLessonsRecurse(
                    tree,
                    node,
                    content,
                    studyItem,
                    languageIDs,
                    testNode,
                    out oldContent,
                    out oldStudyItem,
                    ref isDone);

                if (returnValue || isDone)
                    break;
            }

            return returnValue;
        }

        protected bool StudyItemExistsInPriorLessonsRecurse(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            BaseObjectNode currentNode,
            out BaseObjectContent oldContent,
            out MultiLanguageItem oldStudyItem,
            ref bool isDone)
        {
            bool returnValue = false;

            oldContent = null;
            oldStudyItem = null;

            if (currentNode == null)
                return returnValue;

            if (node == currentNode)
            {
                isDone = true;
                return returnValue;
            }

            ContentClassType contentClass = content.ContentClass;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;

            foreach (BaseObjectContent testContent in currentNode.ContentList)
            {
                if ((testContent.ContentClass != contentClass) ||
                        (testContent.ContentType != contentType) ||
                        (testContent.ContentSubType != contentSubType))
                    continue;

                testContent.ResolveReferences(Repositories, false, false);

                ContentStudyList testStudyList = testContent.ContentStorageStudyList;

                if (testStudyList.StudyItemCount() != 0)
                {
                    foreach (MultiLanguageItem testStudyItem in testStudyList.StudyItems)
                    {
                        if (studyItem.IsCaseInsensitiveTextMatch(testStudyItem, languageIDs))
                        {
                            returnValue = true;
                            oldContent = testContent;
                            oldStudyItem = testStudyItem;
                            isDone = true;
                            break;
                        }
                    }
                }
            }

            if (!isDone && (currentNode.ChildCount() != 0))
            {
                foreach (BaseObjectNode testNode in currentNode.Children)
                {
                    if (testNode == node)
                        break;

                    returnValue = StudyItemExistsInPriorLessonsRecurse(
                        tree,
                        node,
                        content,
                        studyItem,
                        languageIDs,
                        testNode,
                        out oldContent,
                        out oldStudyItem,
                        ref isDone);

                    if (returnValue || isDone)
                        break;
                }
            }

            return returnValue;
        }

        public bool AddMissingNodeTranslationsHelper(
            BaseObjectNodeTree tree, BaseObjectNode node, List<LanguageID> languageIDs,
            bool translateTitlesAndDescriptions,
            Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags)
        {
            bool returnValue = false;
            try
            {
                returnValue = AddMissingNodeTranslationsChildrenAndContentHelper(
                    tree, node, languageIDs, translateTitlesAndDescriptions,
                    nodeSelectFlags, contentSelectFlags);
            }
            catch (OperationCanceledException exc)
            {
                Error = exc.Message;
                if (exc.InnerException != null)
                    Error = Error + ": " + exc.InnerException.Message;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
                if (exc.InnerException != null)
                    Error = Error + ": " + exc.InnerException.Message;
            }
            return returnValue;
        }

        public bool AddMissingNodeTranslationsChildrenAndContentHelper(
            BaseObjectNodeTree tree, BaseObjectNode node, List<LanguageID> languageIDs,
            bool translateTitlesAndDescriptions,
            Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags)
        {
            BaseObjectNode child;
            bool translated;
            bool useIt = false;
            bool returnValue = true;

            if (tree == null)
                return true;

            if (node == null)
                node = tree;

            node.ResolveReferences(Repositories, false, false);

            if ((nodeSelectFlags == null) || !nodeSelectFlags.TryGetValue(node.KeyInt, out useIt))
                useIt = true;

            if (useIt && translateTitlesAndDescriptions)
            {
                if (!Translator.AddTitledObjectTranslationsCheck(node, languageIDs, true, out Error, out translated))
                    returnValue = false;
            }

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (child != null)
                    {
                        child.CopyLanguages(node);

                        if (!AddMissingNodeTranslationsChildrenAndContentHelper(
                                child.Tree, child, languageIDs,
                                translateTitlesAndDescriptions, nodeSelectFlags, contentSelectFlags))
                            returnValue = false;
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + key.ToString() + "\n";
                        returnValue = false;
                    }
                }
            }

            if (useIt && node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentList)
                {
                    if (!AddMissingContentTranslationsHelper(
                            content, languageIDs, translateTitlesAndDescriptions,
                            contentSelectFlags))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddMissingContentTranslationsHelper(
            BaseObjectContent content, List<LanguageID> languageIDs,
            bool translateTitlesAndDescriptions,
            Dictionary<string, bool> contentSelectFlags)
        {
            string contentKey = content.KeyString;
            bool useIt = false;
            bool translated;
            bool returnValue = true;

            CheckForCancel("AddNodeTranslations");

            content.ResolveReferences(Repositories, false, false);

            if (contentSelectFlags != null)
                contentSelectFlags.TryGetValue(contentKey, out useIt);
            else
                useIt = true;

            if (useIt)
            {
                if (translateTitlesAndDescriptions)
                {
                    if (!Translator.AddTitledObjectTranslationsCheck(content, languageIDs, true, out Error, out translated))
                        returnValue = false;
                }

                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        {
                            ContentStudyList studyList = content.ContentStorageStudyList;
                            if (studyList != null)
                            {
                                if (!AddMissingStudyListTranslationsHelper(studyList, languageIDs))
                                    returnValue = false;
                            }
                        }
                        break;
                    case ContentClassType.DocumentItem:
                    case ContentClassType.MediaList:
                    case ContentClassType.MediaItem:
                    default:
                        break;
                }
            }

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent childContent in content.ContentChildren)
                {
                    if (childContent.KeyString == content.KeyString)
                        continue;

                    if (!AddMissingContentTranslationsHelper(
                            childContent, languageIDs, translateTitlesAndDescriptions,
                            contentSelectFlags))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddMissingStudyListTranslationsHelper(ContentStudyList studyList, List<LanguageID> languageIDs)
        {
            BaseObjectContent content = studyList.Content;
            bool needsSentenceParsing = content.NeedsSentenceParsing;
            bool needsWordParsing = content.NeedsWordParsing;
            bool returnValue = true;

            if (studyList.StudyItemCount() != 0)
            {
                foreach (MultiLanguageItem studyItem in studyList.StudyItems)
                {
                    ContentUtilities.PrepareMultiLanguageItem(studyItem, "", languageIDs);

                    UpdateBackgroundStatus("AddNodeTranslations", studyList, studyItem, null);

                    if (!Translator.TranslateMultiLanguageItem(
                            studyItem, languageIDs, needsSentenceParsing, needsWordParsing, out Error, false))
                        returnValue = false;
                }

                if (studyList.Modified)
                    UpdateContentStorage(studyList.Content, false);
            }

            return returnValue;
        }

        public void UpdateBackgroundStatus(string operationName,
            ContentStudyList studyList, MultiLanguageItem studyItem, LanguageID languageID)
        {
            string sep = " - ";
            string statusLabel;

            if (studyItem == null)
                return;

            if (studyList == null)
                studyList = studyItem.StudyList;

            if (studyList != null)
            {
                BaseObjectNode node = studyList.Node;
                if (node == null)
                    node = studyList.Tree;
                statusLabel = studyList.Content.GetTitleString(UILanguageID) + sep + studyItem.Text(UILanguageID);
                if (languageID != null)
                    statusLabel += " (" + studyItem.Text(languageID) + ")" + sep +
                        languageID.LanguageName(UILanguageID);
                while (node != null)
                {
                    statusLabel = node.GetTitleString(UILanguageID) + sep + statusLabel;
                    node = node.Parent;
                }
            }
            else
                statusLabel = studyItem.Text(UILanguageID);

            ApplicationData.Global.SetOperationStatusLabel(UserName, operationName, statusLabel);
        }

        public void PrimeLanguageToolInflections()
        {
            if (UserProfile == null)
                return;

            List<LanguageID> languageIDs = UserProfile.LanguageIDs;
            List<LanguageID> rootLanguageIDs = LanguageLookup.GetRoots(languageIDs);

            if (rootLanguageIDs == null)
                return;

            foreach (LanguageID languageID in rootLanguageIDs)
            {
                LanguageTool tool = GetLanguageTool(languageID);

                if (tool == null)
                    continue;

                tool.PrimeInflectorTables();
            }
        }

        public override LanguageTool GetLanguageTool(LanguageID languageID)
        {
            LanguageTool languageTool = base.GetLanguageTool(languageID);

            if (languageTool != null)
                languageTool.NodeUtilities = this;

            return languageTool;
        }

        public override LanguageTool GetLanguageToolForRemote(LanguageID languageID)
        {
            LanguageTool languageTool = base.GetLanguageToolForRemote(languageID);

            if (languageTool != null)
                languageTool.NodeUtilities = this;

            return languageTool;
        }
    }
}

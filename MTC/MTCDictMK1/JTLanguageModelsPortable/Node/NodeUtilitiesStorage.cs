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
        public List<ObjectReferenceNodeTree> GetUserTreeHeaders(NodeTreeReferenceRepository treeHeaderRepository)
        {
            StringMatcher matcher = new StringMatcher(UserName, "Owner", MatchCode.Exact, 0, 0);
            List<ObjectReferenceNodeTree> treeHeaders = treeHeaderRepository.Query(matcher);
            return treeHeaders;
        }

        public bool AddTree(BaseObjectNodeTree tree, bool doMessage)
        {
            bool returnValue = false;

            tree.Key = 0;
            tree.EnsureGuid();
            tree.TouchAndClearModified();

            try
            {
                if (Repositories.SaveReference(tree.Source, null, tree))
                {
                    if (AddTreeReference(tree, doMessage))
                    {
                        if (!HasMessageOrError && doMessage)
                            SetChangesSavedMessage();

                        returnValue = true;
                    }
                    else
                        Repositories.DeleteReference(tree.Source, null, tree);
                }
                else
                    Error += S("Error adding tree.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error adding tree") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error += Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool AddTreeReference(BaseObjectNodeTree tree, bool doMessage)
        {
            string treeHeaderSource = tree.Source.Substring(0, tree.Source.Length - 1) + "Headers";
            ObjectReferenceNodeTree treeReference = new ObjectReferenceNodeTree(tree.Source, tree);
            bool returnValue = false;

            treeReference.Directory = tree.Directory;
            treeReference.ModifiedTime = tree.ModifiedTime;
            treeReference.Modified = false;

            try
            {
                if (Repositories.SaveReference(treeHeaderSource, null, treeReference))
                {
                    if (!HasMessageOrError && doMessage)
                        SetChangesSavedMessage();
                    returnValue = true;
                }
                else
                    Error += S("Error adding tree reference.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error adding tree reference") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool AddSandboxTree(BaseObjectNodeTree tree)
        {
            bool returnValue = false;

            tree.Key = 0;
            tree.EnsureGuid();
            tree.TouchAndClearModified();

            try
            {
                if (Repositories.SaveReference(tree.Source, null, tree))
                    returnValue = true;
                else
                    Error += S("Error adding sandbox tree.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error adding sandbox tree") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error += Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool UpdateTreeNodeMediaCheck(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            bool doUpdate,
            bool doMessage)
        {
            if ((tree == null) || (node == null))
                return false;

            node.CollectReferences(null, null, null, null, null, null, null,
                    null, MediaUtilities.VisitMediaConditionalCollectAndSetState);

            if (node == tree)
                UpdateTreeMediaCheck(tree, false, false);
            else
                UpdateNodeMediaCheck(node);

            if (doUpdate)
            {
                node.UpdateReferencesCheck(Repositories, false, true);
                UpdateTreeCheck(tree, false, doMessage);
            }

            return true;
        }

        private bool UpdateTreeMediaCheck(
            BaseObjectNodeTree tree,
            bool doUpdate,
            bool doMessage)
        {
            if (tree == null)
                return false;

            UpdateNodeMediaCheck(tree);

            if (doUpdate)
            {
                tree.UpdateReferencesCheck(Repositories, false, true);
                UpdateTreeCheck(tree, false, doMessage);
            }

            return true;
        }

        private bool UpdateNodeMediaCheck(BaseObjectNode node)
        {
            if (node == null)
                return false;

            List<BaseObjectContent> contents = node.ContentChildren;
            ContentStorageStateCode contentStorageState = ContentStorageStateCode.Empty;

            if (contents != null)
            {
                foreach (BaseObjectContent content in contents)
                {
                    UpdateContentMediaCheck(content);

                    if (content.ContentStorageState == ContentStorageStateCode.NotEmpty)
                        contentStorageState = ContentStorageStateCode.NotEmpty;
                }
            }

            if (node.HasChildren())
            {
                List<BaseObjectNode> nodeChildren = node.Children;

                foreach (BaseObjectNode nodeChild in nodeChildren)
                {
                    UpdateNodeMediaCheck(nodeChild);

                    if (nodeChild.ContentStorageState == ContentStorageStateCode.NotEmpty)
                        contentStorageState = ContentStorageStateCode.NotEmpty;
                }
            }

            node.ContentStorageState = contentStorageState;

            return true;
        }

        private bool UpdateContentMediaCheck(BaseObjectContent content)
        {
            if (content == null)
                return false;

            ContentStorageStateCode contentStorageState = ContentStorageStateCode.Empty;
            bool isVideo = (content.ContentType == "Video");

            switch (content.ContentClass)
            {
                case ContentClassType.DocumentItem:
                    {
                        ContentDocumentItem documentItem = content.ContentStorageDocumentItem;
                        if (documentItem != null)
                        {
                            if (documentItem.HasMarkup)
                            {
                                MarkupTemplate markupTemplate = documentItem.MarkupTemplate;
                                if (markupTemplate != null)
                                {
                                    if (markupTemplate.HasMarkupContents())
                                        contentStorageState = ContentStorageStateCode.NotEmpty;
                                    else
                                        contentStorageState = ContentStorageStateCode.Empty;
                                }
                                else
                                    contentStorageState = ContentStorageStateCode.Empty;
                            }
                            else
                                contentStorageState = ContentStorageStateCode.Empty;
                        }
                    }
                    break;
                case ContentClassType.MediaItem:
                    ContentMediaItem mediaItem = content.ContentStorageMediaItem;
                    if (mediaItem != null)
                    {
                        if (mediaItem.LanguageMediaItemCount() != 0)
                        {
                            foreach (LanguageMediaItem languageMediaItem in mediaItem.LanguageMediaItems)
                            {
                                if (languageMediaItem.MediaDescriptions == null)
                                    continue;
                                foreach (MediaDescription mediaDescription in languageMediaItem.MediaDescriptions)
                                {
                                    if (isVideo && (mediaDescription.MediaType == MediaTypeCode.Audio))
                                        continue;

                                    switch (mediaDescription.StorageState)
                                    {
                                        case MediaStorageState.Present:
                                        case MediaStorageState.Downloaded:
                                        case MediaStorageState.External:
                                            contentStorageState = ContentStorageStateCode.NotEmpty;
                                            break;
                                        case MediaStorageState.BadLink:
                                        case MediaStorageState.Absent:
                                        case MediaStorageState.Unknown:
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ContentClassType.MediaList:
                    break;
                case ContentClassType.StudyList:
                    {
                        ContentStudyList studyList = content.ContentStorageStudyList;
                        if (studyList != null)
                        {
                            if (studyList.StudyItemCount() == 0)
                                contentStorageState = ContentStorageStateCode.Empty;
                            else
                                contentStorageState = ContentStorageStateCode.NotEmpty;
                        }
                    }
                    break;
                case ContentClassType.None:
                    contentStorageState = ContentStorageStateCode.Empty;
                    break;
            }

            if (content.HasContentChildren())
            {
                List<BaseObjectContent> contentChildren = content.ContentChildren;

                foreach (BaseObjectContent childContent in contentChildren)
                {
                    if (childContent.KeyString == content.KeyString)
                        continue;

                    UpdateContentMediaCheck(childContent);

                    if (childContent.ContentStorageState == ContentStorageStateCode.NotEmpty)
                        contentStorageState = ContentStorageStateCode.NotEmpty;
                }
            }

            content.ContentStorageState = contentStorageState;

            return true;
        }

        public bool UpdateTreeCheck(BaseObjectNodeTree tree, bool updateReference, bool doMessage)
        {
            bool returnValue = false;

            if (tree.Modified)
                return UpdateTree(tree, updateReference, doMessage);
            //else if (!HasMessageOrError && doMessage)
            //    SetNothingChangedMessage();

            return returnValue;
        }

        public bool UpdateTree(BaseObjectNodeTree tree, bool updateReference, bool doMessage)
        {
            bool returnValue = false;

            if ((tree == null) || (tree.Key == null) || (tree.KeyInt <= 0))
            {
                Error += S("Trying to update tree with no key.") + "\n";
                return false;
            }

            tree.TouchAndClearModified();

            try
            {
                if (Repositories.UpdateReference(tree.Source, null, tree))
                {
                    if (updateReference)
                        returnValue = UpdateTreeReference(tree, doMessage);
                    else
                    {
                        TouchTreeReference(tree, doMessage);
                        if (!HasMessageOrError && doMessage)
                            SetChangesSavedMessage();
                        returnValue = true;
                    }
                }
                else
                    Error += S("Error updating tree.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error updating tree") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool UpdateTreeReference(BaseObjectNodeTree tree, bool doMessage)
        {
            string treeHeaderSource = tree.Label + "Headers";
            ObjectReferenceNodeTree treeReference = Repositories.ResolveReference(treeHeaderSource, null, tree.Key) as ObjectReferenceNodeTree;
            bool returnValue = false;

            if (treeReference != null)
            {
                treeReference.UpdateReference(tree);
                treeReference.ModifiedTime = tree.ModifiedTime;
                treeReference.Modified = false;

                try
                {
                    if (Repositories.UpdateReference(treeHeaderSource, null, treeReference))
                    {
                        if (!HasMessageOrError && doMessage)
                            SetChangesSavedMessage();
                        returnValue = true;
                    }
                    else
                        Error += S("Error updating tree header.") + "\n";
                }
                catch (Exception exception)
                {
                    Error += S("Error updating tree header") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        Error = Error + ": " + exception.InnerException.Message + "\n";
                    else
                        Error += "\n";
                }
            }
            else
                Error += S("Unable to find tree header for") + ": " + tree.GetTitleString(UILanguageID) + "\n";

            return returnValue;
        }

        public bool TouchTreeReference(BaseObjectNodeTree tree, bool doMessage)
        {
            string treeHeaderSource = tree.Label + "Headers";
            ObjectReferenceNodeTree treeReference = Repositories.ResolveReference(treeHeaderSource, null, tree.Key) as ObjectReferenceNodeTree;
            bool returnValue = false;

            if (treeReference != null)
            {
                treeReference.ModifiedTime = tree.ModifiedTime;
                treeReference.Modified = false;

                try
                {
                    if (Repositories.UpdateReference(treeHeaderSource, null, treeReference))
                    {
                        if (!HasMessageOrError && doMessage)
                            SetChangesSavedMessage();
                        returnValue = true;
                    }
                    else
                        Error += S("Error updating tree header.") + "\n";
                }
                catch (Exception exception)
                {
                    Error += S("Error updating tree header") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        Error = Error + ": " + exception.InnerException.Message + "\n";
                    else
                        Error += "\n";
                }
            }
            else
                Error += S("Unable to find tree header for") + ": " + tree.GetTitleString(UILanguageID) + "\n";

            return returnValue;
        }

        public bool UpdateTreeAndReference(BaseObjectNodeTree tree, ObjectReferenceNodeTree treeReference, bool doMessage)
        {
            string treeHeaderSource = tree.Label + "Headers";
            bool returnValue = false;

            tree.TouchAndClearModified();

            if (treeReference != null)
            {
                treeReference.ModifiedTime = tree.ModifiedTime;
                treeReference.Modified = false;

                try
                {
                    if (Repositories.UpdateReference(treeHeaderSource, null, treeReference))
                    {
                        if (!HasMessageOrError && doMessage)
                            SetChangesSavedMessage();
                        returnValue = true;
                    }
                    else
                        Error += S("Error updating tree header.") + "\n";
                }
                catch (Exception exception)
                {
                    Error += S("Error updating tree header") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        Error = Error + ": " + exception.InnerException.Message + "\n";
                    else
                        Error += "\n";
                }
            }
            else
                Error += S("Unable to find tree header for") + ": " + tree.GetTitleString(UILanguageID) + "\n";

            try
            {
                if (Repositories.UpdateReference(tree.Source, null, tree))
                {
                    if (!HasMessageOrError && doMessage)
                        SetChangesSavedMessage();
                    returnValue = true;
                }
                else
                    Error += S("Error updating tree.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error updating tree") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool UpdateLocalTreeModifiedTimeCheck(BaseObjectNodeTree tree, ObjectReferenceNodeTree remoteTreeReference)
        {
            bool returnValue = true;

            if (tree.ModifiedTime != remoteTreeReference.ModifiedTime)
            {
                string treeHeaderSource = tree.Label + "Headers";
                ObjectReferenceNodeTree treeReference = Repositories.ResolveReference(treeHeaderSource, null, tree.Key) as ObjectReferenceNodeTree;

                if (treeReference != null)
                {
                    treeReference.ModifiedTime = remoteTreeReference.ModifiedTime;
                    treeReference.Modified = false;

                    if (!Repositories.UpdateReference(treeHeaderSource, null, treeReference))
                    {
                        Error = S("Error updating tree reference.");
                        returnValue = false;
                    }
                }

                tree.ModifiedTime = remoteTreeReference.ModifiedTime;
                tree.Modified = false;

                if (!Repositories.UpdateReference(tree.Source, null, tree))
                {
                    Error = S("Error updating tree.");
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool UpdateNodeStorageCheck(BaseObjectNode node, bool recurse)
        {
            List<BaseObjectContent> contentList = node.ContentList;
            bool returnValue = true;

            if (contentList == null)
                return true;

            foreach (BaseObjectContent content in contentList)
                UpdateContentStorageCheck(content, false);

            if (recurse && node.HasChildren())
            {
                foreach (BaseObjectNode child in node.Children)
                    returnValue = UpdateNodeStorageCheck(child, recurse);
            }

            return returnValue;
        }

        public bool UpdateNodeStorage(BaseObjectNode node, bool recurse)
        {
            List<BaseObjectContent> contentList = node.ContentList;
            bool returnValue = true;

            if (contentList == null)
                return true;

            foreach (BaseObjectContent content in contentList)
                UpdateContentStorage(content, false);

            if (recurse && node.HasChildren())
            {
                foreach (BaseObjectNode child in node.Children)
                    returnValue = UpdateNodeStorage(child, recurse);
            }

            return returnValue;
        }

        public bool UpdateContentStorageCheck(BaseObjectContent content, bool doMessage)
        {
            if (content == null)
                return true;

            BaseContentStorage contentStorage = content.ContentStorage;
            bool returnValue = true;

            if (contentStorage == null)
                return true;

            if (contentStorage.Modified)
                return UpdateContentStorage(content, doMessage);
            else if (!contentStorage.Modified && !HasMessageOrError && doMessage)
                SetNothingChangedMessage();

            return returnValue;
        }

        public bool UpdateContentStorage(BaseObjectContent content, bool doMessage)
        {
            if (content == null)
                return true;

            BaseContentStorage contentStorage = content.ContentStorage;
            bool returnValue = false;

            if (contentStorage == null)
                return true;

            if ((contentStorage.Key == null) || (contentStorage.KeyInt <= 0))
            {
                Error += S("Trying to update content storage with no key.") + "\n";
                return false;
            }

            contentStorage.UpdateReferences(Repositories, false, false);

            contentStorage.TouchAndClearModified();

            try
            {
                if (Repositories.UpdateReference(contentStorage.Source, null, contentStorage))
                {
                    if (!HasMessageOrError && doMessage)
                        SetChangesSavedMessage();
                    returnValue = true;
                }
                else
                    Error += S("Error updating content storage.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error updating content storage") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public bool UpdateContentStorageRecurseCheck(BaseObjectContent content)
        {
            if (content == null)
                return true;

            bool returnValue = true;

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent aContent in content.ContentChildren)
                {
                    if (aContent.KeyString == content.KeyString)
                        continue;

                    if (!UpdateContentStorageRecurseCheck(aContent))
                        returnValue = false;
                }
            }

            if (!UpdateContentStorageCheck(content, true))
                returnValue = false;

            return returnValue;
        }

        public bool UpdateContentStorageRecurse(BaseObjectContent content)
        {
            if (content == null)
                return true;

            bool returnValue = true;

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent aContent in content.ContentChildren)
                {
                    if (aContent.KeyString == content.KeyString)
                        continue;

                    if (!UpdateContentStorageRecurse(aContent))
                        returnValue = false;
                }
            }

            if (!UpdateContentStorage(content, true))
                returnValue = false;

            return returnValue;
        }

        public string SharedMediaTildeUrl
        {
            get
            {
                return ApplicationData.MediaTildeUrl + "/"
                    + MediaUtilities.FileFriendlyName(UserRecord.UserName) + "/"
                    + "Shared";
            }
        }

        public class WalkContentData
        {
            public string ContentKey;
            public List<BaseObjectContent> ContentList;

            public WalkContentData(string contentKey)
            {
                ContentKey = contentKey;
                ContentList = new List<BaseObjectContent>();
            }
        }

        public static bool UpdateContentStorageCheckFunction(
            BaseObjectContent content,
            ItemWalker<WalkContentData> walker,
            WalkContentData context)
        {
            if (content.KeyString == context.ContentKey)
                context.ContentList.Add(content);

            return true;
        }

        public bool UpdateContentStorageRecurseCheck(BaseObjectContent content, bool doMessage)
        {
            ItemWalker<WalkContentData> itemWalker = new ItemWalker<WalkContentData>();
            itemWalker.VisitContentFunction = UpdateContentStorageCheckFunction;
            WalkContentData context = new WalkContentData(content.KeyString);
            itemWalker.VisitNode(content.Node, context);

            bool returnValue = true;

            foreach (BaseObjectContent aContent in context.ContentList)
            {
                if (aContent.KeyString == content.KeyString)
                    continue;

                bool doAMessage = ((aContent == content) && doMessage);

                if (!UpdateContentStorageCheck(aContent, doAMessage))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool UpdateContentStorageRecurse(BaseObjectContent content, bool doMessage)
        {
            ItemWalker<WalkContentData> itemWalker = new ItemWalker<WalkContentData>();
            itemWalker.VisitContentFunction = UpdateContentStorageCheckFunction;
            WalkContentData context = new WalkContentData(content.KeyString);
            itemWalker.VisitNode(content.Node, context);

            bool returnValue = true;

            foreach (BaseObjectContent aContent in context.ContentList)
            {
                if (aContent.KeyString == content.KeyString)
                    continue;

                bool doAMessage = ((aContent == content) && doMessage);

                if (!UpdateContentStorage(aContent, doAMessage))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool UpdateStatistics(ContentStatistics cs, bool doMessage)
        {
            bool returnValue;

            if (cs == null)
                return false;

            if (Repositories.ContentStatistics.Update(cs))
            {
                if (doMessage)
                    SetChangesSavedMessage();

                returnValue = true;
            }
            else
            {
                PutError("Error updating statistics.");
                returnValue = false;
            }

            return returnValue;
        }

        public Sandbox GetSandbox()
        {
            Sandbox sandbox = Repositories.Sandboxes.Get(UserName);

            if (sandbox == null)
            {
                sandbox = new Sandbox(UserName);

                sandbox.EnsureGuid();
                sandbox.TouchAndClearModified();

                try
                {
                    if (!Repositories.Sandboxes.Add(sandbox))
                        Error = S("Error adding sandbox.");
                }
                catch (Exception exception)
                {
                    Error += S("Error adding sandbox") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        Error = Error + ": " + exception.InnerException.Message + "\n";
                    else
                        Error += "\n";
                }
            }

            return sandbox;
        }

        public bool UpdateSandboxCheck(Sandbox sandbox)
        {
            if (sandbox == null)
                return false;

            if (sandbox.Modified)
                return UpdateSandbox(sandbox);
            else
                return true;
        }

        public bool UpdateSandbox(Sandbox sandbox)
        {
            bool returnValue = false;

            if (sandbox == null)
                return false;

            try
            {
                sandbox.TouchAndClearModified();

                if (Repositories.Sandboxes.Update(sandbox))
                    returnValue = true;
                else
                    Error += S("Error updating sandbox.") + "\n";
            }
            catch (Exception exception)
            {
                Error += S("Error updating sandbox") + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error = Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public BaseObjectNodeTree GetSandboxTree(Sandbox sandbox)
        {
            BaseObjectNodeTree tree = null;

            if (sandbox == null)
                return null;

            if (sandbox.TreeKey == 0)
            {
                tree = CreateSandboxTree();

                if (AddSandboxTree(tree))
                {
                    sandbox.TreeKey = tree.KeyInt;
                    UpdateSandbox(sandbox);
                }
                else
                    tree = null;
            }
            else
            {
                try
                {
                    tree = Repositories.ResolveReference("Courses", null, sandbox.TreeKey) as BaseObjectNodeTree;

                    if (tree == null)
                        Error += S("Error getting sandbox tree.") + "\n";
                    else
                    {
                        tree.SetupDirectoryCheck();

                        if (tree.Modified)
                            UpdateTree(tree, false, false);
                    }
                }
                catch (Exception exception)
                {
                    Error += S("Error getting sandbox tree") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        Error = Error + ": " + exception.InnerException.Message + "\n";
                    else
                        Error += "\n";
                }
            }

            return tree;
        }

        public bool GetSandboxAndTree(out Sandbox sandbox, out BaseObjectNodeTree tree)
        {
            sandbox = GetSandbox();
            tree = GetSandboxTree(sandbox);

            if ((sandbox == null) || (tree == null))
                return false;

            return true;
        }

        public bool UpdateSandboxAndTreeCheck(Sandbox sandbox, BaseObjectNodeTree tree, bool doMessage)
        {
            bool returnValue = true;

            if (!UpdateSandboxCheck(sandbox))
                returnValue = false;

            if (tree.Modified)
            {
                if (!UpdateTree(tree, false, doMessage))
                    returnValue = false;
            }

            return returnValue;
        }
    }
}

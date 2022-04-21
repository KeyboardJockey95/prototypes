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
        public bool ClearSandbox()
        {
            Sandbox sandbox = GetSandbox();

            if (sandbox == null)
                return false;

            BaseObjectNodeTree tree = GetSandboxTree(sandbox);

            if (tree != null)
                DeleteNodeChildrenAndContentHelper(tree, tree, true);

            sandbox.DeleteAllStudyItems();
            sandbox.DeleteAllSpeakerNames();

            return UpdateSandboxCheck(sandbox);
        }

        public bool DeleteAllTreesHelper(List<ObjectReferenceNodeTree> treeHeaders)
        {
            bool returnValue = true;

            if (treeHeaders != null)
            {
                foreach (ObjectReferenceNodeTree treeHeader in treeHeaders)
                {
                    treeHeader.ResolveReference(Repositories);

                    BaseObjectNodeTree tree = treeHeader.TypedItem<BaseObjectNodeTree>();

                    if (tree != null)
                    {
                        if (!DeleteTreeHelper(tree))
                            returnValue = false;
                    }
                    else
                    {
                        Error += S("Can't find " + treeHeader.Label) + ": " + treeHeader.GetTitleString(UILanguageID) + "\n";
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public bool DeleteTreeAndToolStudyListsHelper(BaseObjectNodeTree tree)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            DeleteNodeToolStudyListsHelper(tree);
            return DeleteTreeHelper(tree, deleteMediaFiles);
        }

        public bool DeleteTreeHelper(BaseObjectNodeTree tree)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            return DeleteTreeHelper(tree, deleteMediaFiles);
        }

        public bool DeleteTreeHelper(BaseObjectNodeTree tree, bool deleteMediaFiles)
        {
            string treeHeaderSource = tree.Label + "Headers";

            DeleteNodeToolStudyListsHelper(tree);

            switch (tree.Label)
            {
                case "Course":
                case "Plan":
                    treeHeaderSource = tree.Label + "Headers";
                    break;
                default:
                    switch (tree.Source)
                    {
                        case "Courses":
                            treeHeaderSource = "CourseHeaders";
                            break;
                        case "Plans":
                            treeHeaderSource = "PlanHeaders";
                            break;
                        default:
                            treeHeaderSource = null;
                            break;
                    }
                    break;
            }

            string mediaDirectory;
            bool returnValue = true;

            if (tree.Owner != UserRecord.UserName)
            {
                if (!ApplicationData.IsMobileVersion && !UserRecord.IsAdministrator())
                {
                    Error += S("Sorry, you need to be an administrator to delete another person's " + tree.Label + ".\n");
                    return false;
                }
            }

            if (!String.IsNullOrEmpty(treeHeaderSource))
            {
                if (!Repositories.DeleteReference(treeHeaderSource, null, tree.Key))
                {
                    Error += S("Error deleting " + tree.LabelLower + " header") + ": " + tree.GetTitleString(UILanguageID) + "\n";
                    returnValue = false;
                }
            }

            if (tree.NodeCount() != 0)
            {
                foreach (BaseObjectNode node in tree.Nodes)
                {
                    if (!node.HasContent())
                        continue;

                    mediaDirectory = node.MediaDirectoryPath;

                    int c = node.ContentList.Count();
                    int i;

                    for (i = c - 1; i >= 0; i--)
                    {
                        BaseObjectContent content = node.ContentList[i];
                        if (content != null)
                            node.DeleteContent(content);
                    }
                }
            }

            tree.DeleteAllContent();
            tree.DeleteAllChildren();
            tree.DeleteAllNodes();

            if (deleteMediaFiles)
            {
                mediaDirectory = tree.MediaDirectoryPath;

                if (!String.IsNullOrEmpty(mediaDirectory) && !MediaUtilities.DirectoryDeleteSafe(mediaDirectory))
                {
                    Error += S("Error deleting directory: " + mediaDirectory);
                    returnValue = false;
                }
            }

            DeleteTreeDataHelper(tree);

            if (!Repositories.DeleteReference(tree.Source, null, tree.Key))
            {
                Error += S("Error deleting " + tree.LabelLower) + ": " + tree.GetTitleString(UILanguageID) + "\n";
                returnValue = false;
            }

            return returnValue;
        }

        public bool DeleteTreeHeaderHelper(ObjectReferenceNodeTree treeHeader)
        {
            string treeHeaderSource = treeHeader.Label + "Headers";
            return Repositories.DeleteReference(treeHeaderSource, null, treeHeader.Key);
        }

        public bool DeleteTreeOnlyHelper(BaseObjectNodeTree tree)
        {
            string treeHeaderSource = tree.Label + "Headers";
            bool returnValue = true;

            if (tree.Owner != UserRecord.UserName)
            {
                if (!UserRecord.IsAdministrator())
                {
                    Error += S("Sorry, you need to be an administrator to delete another person's " + tree.Label + ".\n");
                    return false;
                }
            }

            if (!Repositories.DeleteReference(treeHeaderSource, null, tree.Key))
            {
                Error += S("Error deleting " + tree.LabelLower + " header") + ": " + tree.GetTitleString(UILanguageID) + "\n";
                returnValue = false;
            }

            if (!Repositories.DeleteReference(tree.Source, null, tree.Key))
            {
                Error += S("Error deleting " + tree.LabelLower) + ": " + tree.GetTitleString(UILanguageID) + "\n";
                returnValue = false;
            }

            return returnValue;
        }

        public bool DeleteTreeChildrenHelper(BaseObjectNodeTree tree)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            return DeleteTreeChildrenHelper(tree, deleteMediaFiles);
        }

        public bool DeleteTreeChildrenHelper(BaseObjectNodeTree tree, bool deleteMediaFiles)
        {
            bool returnValue = DeleteTreeChildrenHelper(tree, deleteMediaFiles, true);
            return returnValue;
        }

        public bool DeleteTreeChildrenHelper(
            BaseObjectNodeTree tree,
            bool deleteMediaFiles,
            bool doMessage)
        {
            string mediaDirectory;
            bool returnValue = true;

            DeleteNodeToolStudyListsHelper(tree);

            if (tree.Owner != UserRecord.UserName)
            {
                if (!UserRecord.IsAdministrator())
                {
                    Error += S("Sorry, you need to be an administrator to delete another person's " + tree.Label + ".\n");
                    return false;
                }
            }

            if (tree.NodeCount() != 0)
            {
                int nodeCount = tree.NodeCount();
                int nodeIndex;

                for (nodeIndex = nodeCount - 1; nodeIndex >= 0; nodeIndex--)
                {
                    BaseObjectNode node = tree.GetNodeIndexed(nodeIndex);

                    if (!node.HasContent())
                        continue;

                    int c = node.ContentList.Count();
                    int i;

                    for (i = c - 1; i >= 0; i--)
                    {
                        BaseObjectContent content = node.ContentList[i];
                        if (content != null)
                            returnValue = DeleteContentHelper(content, deleteMediaFiles) && returnValue;
                    }

                    if (deleteMediaFiles && !node.IsReference)
                    {
                        mediaDirectory = node.MediaDirectoryPath;

                        if (!String.IsNullOrEmpty(mediaDirectory) && !MediaUtilities.DirectoryDeleteSafe(mediaDirectory))
                        {
                            Error += S("Error deleting directory: " + mediaDirectory);
                            returnValue = false;
                        }
                    }
                }
            }

            tree.DeleteAllContent();
            tree.DeleteAllChildren();
            tree.DeleteAllNodes();

            DeleteTreeDataHelper(tree);

            if (!UpdateTree(tree, false, doMessage))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteTreeDataHelper(BaseObjectNodeTree tree)
        {
            string treeCacheKey = ComposeTreeCacheString(tree);
            bool returnValue;
            switch (tree.Label)
            {
                case "Course":
                    returnValue = Repositories.CourseTreeCache.DeleteKey(treeCacheKey, UILanguageID);
                    break;
                case "Plan":
                    returnValue = Repositories.PlanTreeCache.DeleteKey(treeCacheKey, UILanguageID);
                    break;
                default:
                    returnValue = false;
                    break;
            }
            return returnValue;
        }

        public bool DeleteSelectedNodes(BaseObjectNode parentTreeOrNode, Dictionary<int, bool> selectedNodeFlags)
        {
            bool returnValue = true;

            if (parentTreeOrNode == null)
                return false;

            int index;
            int count = parentTreeOrNode.ChildCount();

            for (index = count - 1; index >= 0; index--)
            {
                BaseObjectNode childNode = parentTreeOrNode.GetChildIndexed(index);

                if (childNode == null)
                    continue;

                if (!DeleteSelectedNodes(childNode, selectedNodeFlags))
                    returnValue = false;

                bool deleteIt = false;

                if (selectedNodeFlags == null)
                    deleteIt = true;
                else
                {
                    if (selectedNodeFlags.TryGetValue(childNode.KeyInt, out deleteIt) && deleteIt)
                        selectedNodeFlags.Remove(childNode.KeyInt);
                }

                if (deleteIt)
                {
                    DeleteNodeToolStudyListsHelper(childNode);
                    DeleteNodeHelperNoUpdate(childNode.Tree, childNode, true);
                }
            }

            return returnValue;
        }

        public bool DeleteSelectedNodesMedia(BaseObjectNode parentTreeOrNode, Dictionary<int, bool> selectedNodeFlags)
        {
            bool returnValue = true;

            if (parentTreeOrNode == null)
                return false;

            int index;
            int count = parentTreeOrNode.ChildCount();

            for (index = count - 1; index >= 0; index--)
            {
                BaseObjectNode childNode = parentTreeOrNode.GetChildIndexed(index);

                if (childNode == null)
                    continue;

                if (!DeleteSelectedNodesMedia(childNode, selectedNodeFlags))
                    returnValue = false;

                bool deleteIt = false;

                if (selectedNodeFlags == null)
                    deleteIt = true;
                else
                    selectedNodeFlags.TryGetValue(childNode.KeyInt, out deleteIt);

                if (deleteIt)
                    DeleteNodeMediaHelper(childNode.Tree, childNode);
            }

            return returnValue;
        }

        public bool DeleteNodeHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);

            if (node.Owner != UserRecord.UserName)
            {
                if ((tree.Owner != UserRecord.UserName) && !UserRecord.IsAdministrator())
                {
                    Error += S("Sorry, you need to be an administrator to delete another person's " + tree.Label + ".\n");
                    return false;
                }
            }

            bool returnValue = DeleteNodeHelperNoUpdate(tree, node, deleteMediaFiles);

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeHelperNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node, bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (!DeleteNodeChildrenAndContentHelper(tree, node, deleteMediaFiles))
                returnValue = false;

            if (node.HasParent())
            {
                BaseObjectNode parentNode = node.Parent;

                if (parentNode != null)
                    parentNode.DeleteChildKey(node.Key);
            }

            if (tree.HasChild(node.Key))
                tree.DeleteChildKey(node.Key);

            if (!tree.DeleteNode(node))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeChildrenHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            BaseObjectNode child;
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            bool returnValue = true;

            if (node.Owner != UserRecord.UserName)
            {
                if ((tree.Owner != UserRecord.UserName) && !UserRecord.IsAdministrator())
                {
                    Error += S("Sorry, you need to be an administrator to delete another person's " + tree.Label + ".\n");
                    return false;
                }
            }

            node.ResolveReferences(Repositories, false, false);

            DeleteNodeToolStudyListsHelper(node);

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (tree.HasChild(key))
                        tree.DeleteChildKey(key);

                    if (child != null)
                    {
                        returnValue = DeleteNodeChildrenAndContentHelper(tree, child, deleteMediaFiles) && returnValue;

                        if (!tree.DeleteNode(child))
                        {
                            Error += S("Error deleting " + node.LabelLower) + ": " + UI(child.Title) + "\n";
                            returnValue = false;
                        }
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + UI(child.Title) + "\n";
                        returnValue = false;
                    }
                }

                node.DeleteAllChildren();
            }

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeChildrenAndContentHelper(BaseObjectNodeTree tree, BaseObjectNode node, bool deleteMediaFiles)
        {
            BaseObjectNode child;
            string mediaDirectory = node.MediaDirectoryPath;
            bool returnValue = true;

            node.ResolveReferences(Repositories, false, false);

            DeleteNodeToolStudyListsHelper(node);

            if (node.HasChildren())
            {
                foreach (object key in node.ChildrenKeys)
                {
                    child = tree.GetNode(key);

                    if (tree.HasChild(key))
                        tree.DeleteChildKey(key);

                    if (child != null)
                    {
                        returnValue = DeleteNodeChildrenAndContentHelper(tree, child, deleteMediaFiles) && returnValue;

                        if (!tree.DeleteNode(child))
                        {
                            Error += S("Error deleting " + node.LabelLower) + ": " + UI(child.Title) + "\n";
                            returnValue = false;
                        }
                    }
                    else
                    {
                        Error += S("Child node was null") + ": " + key.ToString() + "\n";
                        returnValue = false;
                    }
                }

                node.DeleteAllChildren();
            }

            if (node.HasContent())
            {
                int c = node.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent content = node.ContentList[i];
                    content.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(content, deleteMediaFiles))
                        returnValue = false;
                }

                node.DeleteAllContent();
            }

            if (deleteMediaFiles && !node.IsReference && !String.IsNullOrEmpty(mediaDirectory))
            {
                if (!MediaUtilities.DirectoryDeleteSafe(mediaDirectory))
                {
                    Error += S("Error deleting directory: " + mediaDirectory);
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool DeleteNodeAllContentHelper(BaseObjectNodeTree tree, BaseObjectNode node, bool deleteMediaFiles)
        {
            string mediaDirectory = node.MediaDirectoryPath;
            bool returnValue = true;

            node.ResolveReferences(Repositories, false, false);

            if (node.HasContent())
            {
                int c = node.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent content = node.ContentList[i];
                    content.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(content, deleteMediaFiles))
                        returnValue = false;
                }

                node.DeleteAllContent();
            }

            if (deleteMediaFiles && !String.IsNullOrEmpty(mediaDirectory) && !node.IsReference)
            {
                if (!MediaUtilities.DirectoryDeleteSafe(mediaDirectory))
                {
                    Error += S("Error deleting directory: " + mediaDirectory);
                    returnValue = false;
                }
            }

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeAllContentHelperNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            string mediaDirectory = node.MediaDirectoryPath;
            bool returnValue = true;

            node.ResolveReferences(Repositories, false, false);

            if (node.HasContent())
            {
                int c = node.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent content = node.ContentList[i];
                    content.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(content, deleteMediaFiles))
                        returnValue = false;
                }

                node.DeleteAllContent();
            }

            return returnValue;
        }

        public bool DeleteContentAllSubContentHelperNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node,
            BaseObjectContent content)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            string mediaDirectory = node.MediaDirectoryPath;
            bool returnValue = true;

            content.ResolveReferences(Repositories, false, false);

            if (content.HasContent())
            {
                int c = content.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent subContent = content.ContentList[i];
                    subContent.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(subContent, deleteMediaFiles))
                        returnValue = false;
                }

                content.DeleteAllContent();
            }

            return returnValue;
        }

        public bool DeleteNodeMediaHelper(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            bool returnValue = true;
            if (node == null)
                return false;
            if (!node.HasContent())
                return true;
            foreach (BaseObjectContent content in node.ContentList)
            {
                switch (content.ContentClass)
                {
                    case ContentClassType.MediaList:
                        if (!DeleteMediaListMediaHelper(content))
                            returnValue = false;
                        break;
                    case ContentClassType.MediaItem:
                        if (!DeleteMediaItemMediaHelper(content))
                            returnValue = false;
                        break;
                    case ContentClassType.StudyList:
                        if (!DeleteStudyListMediaHelper(content))
                            returnValue = false;
                        break;
                    default:
                        break;
                }
            }
            string mediaSubDirectory = node.MediaDirectoryPath;
            if (!node.IsReference && !MediaUtilities.DirectoryDeleteSafe(mediaSubDirectory))
            {
                Error += S("Error deleting directory: " + mediaSubDirectory);
                returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteNodeSelectedContentMediaHelper(BaseObjectNodeTree tree, BaseObjectNode node,
            Dictionary<string, bool> contentSelectFlags)
        {
            bool returnValue = true;

            foreach (KeyValuePair<string, bool> kvp in contentSelectFlags)
            {
                if (!kvp.Value)
                    continue;

                BaseObjectContent content = node.GetContent(kvp.Key);

                if (content == null)
                    continue;

                DeleteContentMediaHelper(content);
                UpdateContentHelper(content);
            }

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeSelectedContentHelper(BaseObjectNodeTree tree, BaseObjectNode node,
            Dictionary<string, bool> contentSelectFlags)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            List<string> contentKeys = new List<string>();
            bool returnValue = true;

            foreach (KeyValuePair<string, bool> kvp in contentSelectFlags)
            {
                if (!kvp.Value)
                    continue;

                contentKeys.Add(kvp.Key);

                BaseObjectContent content = node.GetContent(kvp.Key);

                if (content == null)
                    continue;

                content.ResolveReferences(Repositories, false, false);

                if (content.HasContent())
                {
                    int c = content.ContentList.Count();
                    int i;

                    for (i = c - 1; i >= 0; i--)
                    {
                        BaseObjectContent subContent = content.ContentList[i];
                        subContent.ResolveReferences(Repositories, false, false);

                        if (!DeleteContentHelper(subContent, deleteMediaFiles))
                            returnValue = false;
                    }

                    content.DeleteAllContent();
                }

                if (!DeleteContentHelper(content, deleteMediaFiles))
                    returnValue = false;

                BaseObjectContent contentParent = content.ContentParent;

                if (contentParent != null)
                    contentParent.DeleteContentChildKey(content.KeyString);

                if (!node.DeleteContent(content))
                {
                    Error += S("Error deleting " + content.ContentType) + ": " + UI(content.Title) + "\n";
                    returnValue = false;
                }
            }

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            foreach (string key in contentKeys)
                contentSelectFlags.Remove(key);

            return returnValue;
        }

        public bool DeleteNodeSelectedContentHelperNoUpdate(BaseObjectNodeTree tree, BaseObjectNode node,
            Dictionary<string, bool> contentSelectFlags)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            List<string> contentKeys = new List<string>();
            bool returnValue = true;

            foreach (KeyValuePair<string, bool> kvp in contentSelectFlags)
            {
                if (!kvp.Value)
                    continue;

                contentKeys.Add(kvp.Key);

                BaseObjectContent content = node.GetContent(kvp.Key);

                if (content == null)
                    continue;

                content.ResolveReferences(Repositories, false, false);

                if (content.HasContent())
                {
                    int c = content.ContentList.Count();
                    int i;

                    for (i = c - 1; i >= 0; i--)
                    {
                        BaseObjectContent subContent = content.ContentList[i];
                        subContent.ResolveReferences(Repositories, false, false);

                        if (!DeleteContentHelper(subContent, deleteMediaFiles))
                            returnValue = false;
                    }

                    content.DeleteAllContent();
                }

                if (!DeleteContentHelper(content, deleteMediaFiles))
                    returnValue = false;

                BaseObjectContent contentParent = content.ContentParent;

                if (contentParent != null)
                    contentParent.DeleteContentChildKey(content.KeyString);

                if (!node.DeleteContent(content))
                {
                    Error += S("Error deleting " + content.ContentType) + ": " + UI(content.Title) + "\n";
                    returnValue = false;
                }
            }

            foreach (string key in contentKeys)
                contentSelectFlags.Remove(key);

            return returnValue;
        }

        public bool DeleteNodeContentHelper(BaseObjectNodeTree tree, BaseObjectNode node, BaseObjectContent content)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            bool returnValue = DeleteNodeContentHelper(node, content, deleteMediaFiles);

            if (!UpdateTree(tree, false, true))
                returnValue = false;

            return returnValue;
        }

        public bool DeleteNodeContentHelper(BaseObjectNode node, BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            content.ResolveReferences(Repositories, false, false);

            if (content.HasContent())
            {
                int c = content.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent subContent = content.ContentList[i];
                    subContent.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(subContent, deleteMediaFiles))
                        returnValue = false;
                }

                content.DeleteAllContent();
            }

            if (!DeleteContentHelper(content, deleteMediaFiles))
                returnValue = false;

            BaseObjectContent contentParent = content.ContentParent;

            if (contentParent != null)
                contentParent.DeleteContentChildKey(content.KeyString);

            if (!node.DeleteContent(content))
            {
                Error += S("Error deleting " + content.ContentType) + ": " + UI(content.Title) + "\n";
                returnValue = false;
            }

            return returnValue;
        }

        public bool DeleteContentHelper(BaseObjectContent content, bool deleteMediaFiles)
        {
            bool returnValue = DeleteContentChildrenHelper(content, deleteMediaFiles);

            if (!String.IsNullOrEmpty(content.Source) && (content.ContentStorageKey > 0))
            {
                BaseContentStorage contentStorage = content.ContentStorage;

                if (contentStorage == null)
                    contentStorage = Repositories.ResolveReference(content.Source, null, content.ContentStorageKey) as BaseContentStorage;

                if (contentStorage != null)
                {
                    if (contentStorage.ReferenceCount <= 1)
                    {
                        if (content.ContentClass == ContentClassType.StudyList)
                            DeleteContentToolStudyListsHelper(content);

                        if (deleteMediaFiles)
                        {
                            string contentDirectoryPath = content.MediaDirectoryPath;
                            returnValue = DeleteContentMediaHelper(content);
                            if (FileSingleton.DirectoryExists(contentDirectoryPath))
                                FileSingleton.DeleteEmptyDirectory(contentDirectoryPath);
                        }

                        if (Repositories.DeleteReference(content.Source, null, content.ContentStorageKey))
                        {
                            content.ContentStorage = null;
                            content.ContentStorageKey = 0;
                        }
                        else
                        {
                            Error += S("Error deleting content") + ": " + content.GetTitleString(UILanguageID);
                            returnValue = false;
                        }
                    }
                    else
                    {
                        contentStorage.ReferenceCount = contentStorage.ReferenceCount - 1;
                        contentStorage.Modified = false;

                        if (!Repositories.UpdateReference(content.Source, null, contentStorage))
                        {
                            Error += S("Error updating content storage after reference decrement")
                                + ": " + content.GetTitleString(UILanguageID);
                            returnValue = false;
                        }
                    }
                }
                else if (!ApplicationData.IsMobileVersion)
                {
                    Error += S("Content already deleted") + ": " + content.GetTitleString(UILanguageID);
                    returnValue = false;
                }
            }
            else
            {
                if (deleteMediaFiles)
                    returnValue = DeleteContentMediaHelper(content);
            }
            return returnValue;
        }

        public bool UpdateContentHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            if (!String.IsNullOrEmpty(content.Source) && (content.ContentStorageKey > 0) && (content.ContentStorage != null))
            {
                if (!content.ContentStorage.Modified)
                    return returnValue;
                content.ContentStorage.TouchAndClearModified();
                if (Repositories.UpdateReference(content.Source, null, content.ContentStorage))
                {
                    if (!HasMessageOrError)
                        SetChangesSavedMessage();
                }
                else
                {
                    Error += S("Error updating content") + ": " + content.GetTitleString(UILanguageID);
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool UpdateContentHelperNoMessage(BaseObjectContent content)
        {
            bool returnValue = true;
            if (!String.IsNullOrEmpty(content.Source) && (content.ContentStorageKey > 0) && (content.ContentStorage != null))
            {
                if (!content.ContentStorage.Modified)
                    return returnValue;
                content.ContentStorage.TouchAndClearModified();
                if (!Repositories.UpdateReference(content.Source, null, content.ContentStorage))
                {
                    Error += S("Error updating content") + ": " + content.GetTitleString(UILanguageID);
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteContentChildrenHelper(BaseObjectContent content)
        {
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
            bool returnValue = DeleteContentChildrenHelper(content, deleteMediaFiles);
            return returnValue;
        }

        public bool DeleteContentChildrenHelper(BaseObjectContent content, bool deleteMediaFiles)
        {
            bool returnValue = true;

            content.ResolveReferences(Repositories, false, false);

            if (content.HasContent())
            {
                int c = content.ContentList.Count();
                int i;

                for (i = c - 1; i >= 0; i--)
                {
                    BaseObjectContent subContent = content.ContentList[i];
                    subContent.ResolveReferences(Repositories, false, false);

                    if (!DeleteContentHelper(subContent, deleteMediaFiles))
                        returnValue = false;
                }

                content.DeleteAllContent();
            }

            return returnValue;
        }

        public bool DeleteContentContentsHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            switch (content.ContentClass)
            {
                case ContentClassType.DocumentItem:
                    returnValue = DeleteDocumentItemContentsHelper(content, deleteMediaFiles);
                    break;
                case ContentClassType.MediaItem:
                    returnValue = DeleteMediaItemContentsHelper(content, deleteMediaFiles);
                    break;
                case ContentClassType.MediaList:
                    returnValue = DeleteMediaListContentsHelper(content, deleteMediaFiles);
                    break;
                case ContentClassType.StudyList:
                    returnValue = DeleteStudyListContentsHelper(content, deleteMediaFiles);
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public bool DeleteDocumentItemContentsHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            if (deleteMediaFiles)
                returnValue = DeleteContentMarkupTemplateMediaHelper(content) && returnValue;

            returnValue = DeleteContentLocalMarkupTemplateHelper(content, deleteMediaFiles) && returnValue;

            return returnValue;
        }

        public bool DeleteMediaItemContentsHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            if (deleteMediaFiles)
                returnValue = DeleteMediaItemMediaHelper(content) && returnValue;

            returnValue = DeleteContentLocalMarkupTemplateHelper(content, deleteMediaFiles) && returnValue;

            ContentMediaItem mediaItem = content.ContentStorageMediaItem;

            if (mediaItem == null)
                return true;

            List<LanguageMediaItem> languageMediaItems = mediaItem.LanguageMediaItems;

            if (languageMediaItems != null)
            {
                foreach (LanguageMediaItem languageMediaItem in languageMediaItems)
                    returnValue = DeleteLanguageMediaItemContentsHelper(languageMediaItem, false) && returnValue;
            }

            mediaItem.DeleteAllLocalStudyItems();
            mediaItem.DeleteAllLocalSpeakerNames();

            return returnValue;
        }

        public bool DeleteLanguageMediaItemContentsHelper(
            LanguageMediaItem languageMediaItem,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (languageMediaItem == null)
                return false;

            if (deleteMediaFiles)
                returnValue = DeleteLanguageMediaItemMediaHelper(languageMediaItem) && returnValue;

            return returnValue;
        }

        public bool DeleteMediaListContentsHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            return returnValue;
        }

        public bool DeleteStudyListContentsHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            if (deleteMediaFiles)
                returnValue = DeleteStudyListMediaHelper(content) && returnValue;

            returnValue = DeleteContentLocalMarkupTemplateHelper(content, deleteMediaFiles) && returnValue;

            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                return true;

            studyList.DeleteAllStudyItems();
            studyList.DeleteAllSpeakerNames();

            return returnValue;
        }

        public bool DeleteContentLocalMarkupTemplateHelper(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            bool returnValue = true;
            if (content.HasContentStorageKey && (content.ContentStorageReferenceCount <= 1))
            {
                MarkupTemplate markupTemplate = content.LocalMarkupTemplate;
                if (markupTemplate == null)
                    return true;
                if (markupTemplate.KeyString != "(local)")
                    return true;
                List<MultiLanguageItem> multiLanguageItems = markupTemplate.MultiLanguageItems;
                if (multiLanguageItems == null)
                    return true;
                if (deleteMediaFiles)
                {
                    content.SetupDirectoryCheck();
                    string mediaDirectoryTildeUrl = content.MediaTildeUrl;
                    foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
                    {
                        if (multiLanguageItem.Count() == 0)
                            continue;
                        if (multiLanguageItem.HasItemSource)
                            continue;
                        if (!ContentUtilities.DeleteStudyItemMediaRunsAndMedia(multiLanguageItem, mediaDirectoryTildeUrl, true))
                            returnValue = false;
                    }
                }
                markupTemplate.DeleteAllAttributions();
                markupTemplate.DeleteAllMultiLanguageItems();
                markupTemplate.DeleteAllVariables();
            }
            return returnValue;
        }

        public bool DeleteContentMediaHelper(BaseObjectContent content)
        {
            bool returnValue = true;

            switch (content.ContentClass)
            {
                case ContentClassType.DocumentItem:
                    if (!DeleteContentMarkupTemplateMediaHelper(content))
                        returnValue = false;
                    break;
                case ContentClassType.MediaList:
                    break;
                case ContentClassType.MediaItem:
                    if (!DeleteMediaItemMediaHelper(content))
                        returnValue = false;
                    break;
                case ContentClassType.StudyList:
                    if (!DeleteStudyListMediaHelper(content))
                        returnValue = false;
                    break;
                default:
                    break;
            }

            if (content.ContentList != null)
            {
                foreach (BaseObjectContent subContent in content.ContentList)
                {
                    if (subContent.KeyString == content.KeyString)
                        continue;

                    returnValue = DeleteContentMediaHelper(subContent) && returnValue;
                }
            }

            return returnValue;
        }

        public bool DeleteMediaListMediaHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            foreach (BaseObjectContent subContent in content.ContentList)
            {
                if (subContent.KeyString == content.KeyString)
                    continue;

                returnValue = DeleteContentMediaHelper(subContent) && returnValue;
            }

            return returnValue;
        }

        public bool DeleteMediaItemMediaHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            if (content.ContentStorageReferenceCount <= 1)
            {
                if (!DeleteMediaItemMediaOnlyHelper(content))
                    returnValue = false;
                if (!DeleteContentMarkupTemplateMediaHelper(content))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteMediaItemMediaOnlyHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            if (content.ContentStorageReferenceCount <= 1)
            {
                ContentMediaItem contentMediaItem = content.GetContentStorageTyped<ContentMediaItem>();
                if (contentMediaItem == null)
                    return true;
                if (contentMediaItem.LanguageMediaItems == null)
                    return true;
                if (contentMediaItem.IsReference)
                    return true;
                content.SetupDirectoryCheck();
                string mediaPath = content.MediaTildeUrl;
                foreach (LanguageMediaItem languageMediaItem in contentMediaItem.LanguageMediaItems)
                {
                    if (languageMediaItem.MediaDescriptions == null)
                        continue;
                    foreach (MediaDescription mediaDescription in languageMediaItem.MediaDescriptions)
                    {
                        if (mediaDescription.IsFullUrl || mediaDescription.IsEmbedded)
                            continue;
                        string filePath = mediaDescription.GetDirectoryPath(mediaPath);
                        try
                        {
                            bool fileUploaded = FileSingleton.Exists(filePath);
                            if (fileUploaded)
                                FileSingleton.Delete(filePath);
                            MediaConvertSingleton.DeleteAlternates(filePath, mediaDescription.MimeType);
                        }
                        catch (Exception)
                        {
                            returnValue = false;
                        }
                    }
                }
            }
            return returnValue;
        }

        public bool DeleteLanguageMediaItemMediaHelper(LanguageMediaItem languageMediaItem)
        {
            bool returnValue = true;
            BaseObjectContent content = languageMediaItem.Content;
            content.SetupDirectoryCheck();
            string mediaPath = content.MediaTildeUrl;
            foreach (MediaDescription mediaDescription in languageMediaItem.MediaDescriptions)
            {
                if (mediaDescription.IsFullUrl || mediaDescription.IsEmbedded)
                    continue;
                string filePath = mediaDescription.GetDirectoryPath(mediaPath);
                try
                {
                    bool fileUploaded = FileSingleton.Exists(filePath);
                    if (fileUploaded)
                        FileSingleton.Delete(filePath);
                    MediaConvertSingleton.DeleteAlternates(filePath, mediaDescription.MimeType);
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteMediaDescriptionMediaHelper(BaseObjectContent content,
            MediaDescription mediaDescription)
        {
            bool returnValue = true;
            content.SetupDirectoryCheck();
            string mediaPath = content.MediaTildeUrl;
            string filePath = mediaDescription.GetDirectoryPath(mediaPath);
            if (mediaDescription.IsFullUrl || mediaDescription.IsEmbedded || mediaDescription.IsRelativeUrl)
                return true;
            try
            {
                bool fileUploaded = FileSingleton.Exists(filePath);
                if (fileUploaded)
                    FileSingleton.Delete(filePath);
            }
            catch (Exception exc)
            {
                returnValue = false;
                PutExceptionError("Error deleting media description media", exc);
            }
            return returnValue;
        }

        public bool DeleteStudyListMediaHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            if (content.ContentStorageReferenceCount <= 1)
            {
                ContentStudyList contentStudyList = content.GetContentStorageTyped<ContentStudyList>();
                if (contentStudyList == null)
                    return true;
                List<MultiLanguageItem> studyItems = contentStudyList.StudyItems;
                if (studyItems == null)
                    return true;
                content.SetupDirectoryCheck();
                string mediaDirectoryTildeUrl = contentStudyList.MediaTildeUrl;
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.Count() == 0)
                        continue;
                    if (studyItem.HasItemSource)
                        continue;
                    if (!ContentUtilities.DeleteStudyItemMediaRunsAndMedia(studyItem, mediaDirectoryTildeUrl, true))
                        returnValue = false;
                }
                if (!DeleteContentMarkupTemplateMediaHelper(content))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteSandboxMediaHelper(Sandbox sandbox)
        {
            bool returnValue = true;
            if (sandbox != null)
            {
                ContentStudyList contentStudyList = sandbox;
                if (contentStudyList == null)
                    return true;
                List<MultiLanguageItem> studyItems = contentStudyList.StudyItems;
                if (studyItems == null)
                    return true;
                string mediaDirectoryTildeUrl = contentStudyList.MediaTildeUrl;
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.Count() == 0)
                        continue;
                    if (studyItem.HasItemSource)
                        continue;
                    if (!ContentUtilities.DeleteStudyItemMediaRunsAndMedia(studyItem, mediaDirectoryTildeUrl, true))
                        returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteContentMarkupTemplateMediaHelper(BaseObjectContent content)
        {
            bool returnValue = true;
            if (content.HasContentStorageKey && (content.ContentStorageReferenceCount <= 1))
            {
                MarkupTemplate markupTemplate = content.LocalMarkupTemplate;
                if (markupTemplate == null)
                    return true;
                if (markupTemplate.KeyString != "(local)")
                    return true;
                List<MultiLanguageItem> multiLanguageItems = markupTemplate.MultiLanguageItems;
                if (multiLanguageItems == null)
                    return true;
                content.SetupDirectoryCheck();
                string mediaDirectoryTildeUrl = content.MediaTildeUrl;
                foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
                {
                    if (multiLanguageItem.Count() == 0)
                        continue;
                    if (multiLanguageItem.HasItemSource)
                        continue;
                    if (!ContentUtilities.DeleteStudyItemMediaRunsAndMedia(multiLanguageItem, mediaDirectoryTildeUrl, true))
                        returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteTeacherMarkupTemplateMediaHelper(MarkupTemplate markupTemplate)
        {
            bool returnValue = true;
            List<MultiLanguageItem> multiLanguageItems = markupTemplate.MultiLanguageItems;
            if (multiLanguageItems == null)
                return true;
            markupTemplate.SetupDirectoryCheck();
            string mediaDirectoryTildeUrl = markupTemplate.MediaTildeUrl;
            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
            {
                if (multiLanguageItem.Count() == 0)
                    continue;
                if (!ContentUtilities.DeleteStudyItemMediaRunsAndMedia(multiLanguageItem, mediaDirectoryTildeUrl, true))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteEmptyTreeNodeContentCheck(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            bool deleteMediaFiles,
            bool doUpdate,
            bool doMessage)
        {
            if ((tree == null) || (node == null))
                return false;

            if (node == tree)
                DeleteEmptyTreeContentCheck(tree, deleteMediaFiles, false, false);
            else
                DeleteEmptyNodeContentCheck(node, deleteMediaFiles);

            if (doUpdate)
            {
                node.UpdateReferencesCheck(Repositories, false, true);
                UpdateTreeCheck(tree, false, doMessage);
            }

            return true;
        }

        private bool DeleteEmptyTreeContentCheck(
            BaseObjectNodeTree tree,
            bool deleteMediaFiles,
            bool doUpdate,
            bool doMessage)
        {
            if (tree == null)
                return false;

            DeleteEmptyNodeContentCheck(tree, deleteMediaFiles);

            if (doUpdate)
            {
                tree.UpdateReferencesCheck(Repositories, false, true);
                UpdateTreeCheck(tree, false, doMessage);
            }

            return true;
        }

        private bool DeleteEmptyNodeContentCheck(
            BaseObjectNode node,
            bool deleteMediaFiles)
        {
            if (node == null)
                return false;

            if (node.HasChildren())
            {
                List<BaseObjectNode> nodeChildren = new List<BaseObjectNode>(node.Children);

                foreach (BaseObjectNode nodeChild in nodeChildren)
                    DeleteEmptyNodeContentCheck(nodeChild, deleteMediaFiles);
            }

            if (node.HasContentChildren())
            {
                List<BaseObjectContent> contents = new List<BaseObjectContent>(node.ContentChildren);

                foreach (BaseObjectContent content in contents)
                    DeleteEmptyContentCheck(content, deleteMediaFiles);
            }

            switch (node.ContentStorageState)
            {
                case ContentStorageStateCode.Unknown:
                    break;
                case ContentStorageStateCode.Empty:
                    if (node.Tree != null)
                        DeleteNodeHelperNoUpdate(node.Tree, node, deleteMediaFiles);
                    break;
                case ContentStorageStateCode.NotEmpty:
                    break;
                default:
                    break;
            }

            return true;
        }

        private bool DeleteEmptyContentCheck(
            BaseObjectContent content,
            bool deleteMediaFiles)
        {
            if (content == null)
                return false;

            if (content.HasContentChildren())
            {
                List<BaseObjectContent> contentChildren = new List<BaseObjectContent>(content.ContentChildren);

                foreach (BaseObjectContent childContent in contentChildren)
                {
                    if (childContent.KeyString == content.KeyString)
                        continue;

                    DeleteEmptyContentCheck(childContent, deleteMediaFiles);
                }
            }

            switch (content.ContentStorageState)
            {
                case ContentStorageStateCode.Unknown:
                    break;
                case ContentStorageStateCode.Empty:
                    if (content.Node != null)
                    {
                        if (!HaveNonEmptyContentDescendentCheck(content))
                        {
                            DeleteContentHelper(content, deleteMediaFiles);
                            content.Node.DeleteContent(content);
                        }
                    }
                    break;
                case ContentStorageStateCode.NotEmpty:
                    break;
                default:
                    break;
            }

            return true;
        }

        private bool HaveNonEmptyContentDescendentCheck(
            BaseObjectContent content)
        {
            bool returnValue = false;

            if ((content == null) || (content.NodeOrTree == null))
                return false;

            bool isPlaceholder = false;

            switch (content.ContentClass)
            {
                case ContentClassType.MediaList:
                    isPlaceholder = true;
                    break;
                case ContentClassType.MediaItem:
                    isPlaceholder = content.GetOptionFlag("DescendentMediaPlaceholder", false);
                    break;
                case ContentClassType.StudyList:
                    isPlaceholder = content.GetOptionFlag("CollectDescendentItems", false);
                    break;
                default:
                    break;
            }

            if (isPlaceholder)
            {
                BaseObjectNode node = content.NodeOrTree;

                if (node.HasChildren())
                {
                    List<BaseObjectNode> nodeChildren = node.Children;

                    foreach (BaseObjectNode nodeChild in nodeChildren)
                    {
                        if (HaveNonEmptyNodeContentDescendentCheck(nodeChild, content))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }

            return returnValue;
        }

        private bool HaveNonEmptyNodeContentDescendentCheck(
            BaseObjectNode node,
            BaseObjectContent referenceContent)
        {
            if (node == null)
                return false;

            if (node.HasChildren())
            {
                List<BaseObjectNode> nodeChildren = new List<BaseObjectNode>(node.Children);

                foreach (BaseObjectNode nodeChild in nodeChildren)
                {
                    if (HaveNonEmptyNodeContentDescendentCheck(nodeChild, referenceContent))
                        return true;
                }
            }

            if (node.HasContentChildren())
            {
                BaseObjectContent content = node.GetContent(referenceContent.KeyString);

                if (content != null)
                {
                    switch (content.ContentStorageState)
                    {
                        case ContentStorageStateCode.Unknown:
                            break;
                        case ContentStorageStateCode.Empty:
                            break;
                        case ContentStorageStateCode.NotEmpty:
                            return true;
                        default:
                            break;
                    }
                }
            }


            return false;
        }

        public bool DeleteNodeToolStudyListsHelper(BaseObjectNode node)
        {
            if (node == null)
                return false;

            List<string> deletePatterns = new List<string>();

            DeleteNodeToolStudyListsHelperRecurse(node, deletePatterns);

            if (deletePatterns.Count() == 0)
                return true;

            StringMatcher matcher = new StringMatcher(deletePatterns, "Key", MatchCode.RegEx, 0, 0);

            int count = Repositories.ToolStudyLists.DeleteQuery(matcher);

            //ApplicationData.Global.PutConsoleMessage(
            //    "Deleted " + count.ToString() + " tool study lists from node " + node.GetTitleString());

            return true;
        }

        public void DeleteNodeToolStudyListsHelperRecurse(
            BaseObjectNode node,
            List<string> deletePatterns)
        {
            if (node == null)
                return;

            if (node.HasChildren())
            {
                foreach (BaseObjectNode childNode in node.Children)
                    DeleteNodeToolStudyListsHelperRecurse(childNode, deletePatterns);
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentChildren)
                {
                    if (content.ContentClass == ContentClassType.StudyList)
                        DeleteContentToolStudyListsHelperRecurse(content, deletePatterns);
                }
            }
        }

        public bool DeleteContentToolStudyListsHelper(BaseObjectContent content)
        {
            if (content == null)
                return false;

            if (content.ContentClass != ContentClassType.StudyList)
                return true;

            List<string> deletePatterns = new List<string>();

            DeleteContentToolStudyListsHelperRecurse(content, deletePatterns);

            if (deletePatterns.Count() == 0)
                return true;

            StringMatcher matcher = new StringMatcher(deletePatterns, "Key", MatchCode.RegEx, 0, 0);

            int count = Repositories.ToolStudyLists.DeleteQuery(matcher);

            //ApplicationData.Global.PutConsoleMessage(
            //    "Deleted " + count.ToString() + " tool study lists from content " + content.GetTitleString());

            return true;
        }

        public void DeleteContentToolStudyListsHelperRecurse(
            BaseObjectContent content,
            List<string> deletePatterns)
        {
            if (content.ContentClass != ContentClassType.StudyList)
                return;

            string toolStudyListKeyPattern = ToolUtilities.ComposeToolStudyListSearchKeyPattern(content);

            deletePatterns.Add(toolStudyListKeyPattern);

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent contentChild in content.ContentChildren)
                {
                    if (contentChild.KeyString == content.KeyString)
                        continue;

                    if (contentChild.ContentClass == ContentClassType.StudyList)
                        DeleteContentToolStudyListsHelperRecurse(contentChild, deletePatterns);
                }
            }
        }
    }
}

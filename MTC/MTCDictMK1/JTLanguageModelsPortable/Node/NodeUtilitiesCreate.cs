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
        public static BaseObjectNodeTree CreateTree(UserRecord userRecord, UserProfile userProfile, string label, string source)
        {
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", "", userProfile.LanguageDescriptors);
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", "", userProfile.LanguageDescriptors);
            int index = 0;
            List<LanguageID> targetLanguageIDs = new List<LanguageID>(userProfile.TargetLanguageIDs);
            List<LanguageID> hostLanguageIDs = new List<LanguageID>(userProfile.HostLanguageIDs);
            string owner = userRecord.UserName;
            BaseObjectNodeTree tree = new BaseObjectNodeTree(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                null,                   // string package
                label,                  // string label
                null,                   // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs
                owner,                  // string owner
                null,                   // List<BaseObjectNode> children
                null,                   // List<IBaseObjectKeyed> options
                null,                   // MarkupTemplate markupTemplate
                null,                   // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                null,                   // NodeMaster master
                null);                  // List<BaseObjectNode> nodes
            tree.EnsureGuid();
            return tree;
        }

        public static BaseObjectNodeTree CreateTree(MultiLanguageString title, MultiLanguageString description,
            string label, string source, string owner, LanguageID uiLanguageID,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs)
        {
            int index = 0;
            BaseObjectNodeTree tree = new BaseObjectNodeTree(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                null,                   // string package
                label,                  // string label
                null,                   // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs
                owner,                  // string owner
                null,                   // List<BaseObjectNode> children
                null,                   // List<IBaseObjectKeyed> options
                null,                   // MarkupTemplate markupTemplate
                null,                   // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                null,                   // NodeMaster master
                null);                  // List<BaseObjectNode> nodes
            tree.EnsureGuid();
            return tree;
        }

        public BaseObjectNodeTree CreateSandboxTree()
        {
            string source = "Courses";
            string label = "SandboxTree";
            string owner = UserRecord.UserName;
            string titleString = owner + "'s Sandbox Tree";
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", titleString, UserProfile.LanguageDescriptors);
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", "", UserProfile.LanguageDescriptors);
            int index = 0;
            List<LanguageID> targetLanguageIDs = new List<LanguageID>(UserProfile.TargetLanguageIDs);
            List<LanguageID> hostLanguageIDs = new List<LanguageID>(UserProfile.HostLanguageIDs);
            BaseObjectNodeTree tree = new BaseObjectNodeTree(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                null,                   // string package
                label,                  // string label
                null,                   // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs
                owner,                  // string owner
                null,                   // List<BaseObjectNode> children
                null,                   // List<IBaseObjectKeyed> options
                null,                   // MarkupTemplate markupTemplate
                null,                   // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                null,                   // NodeMaster master
                null);                  // List<BaseObjectNode> nodes
            tree.EnsureGuid();
            tree.SetupDirectory();
            return tree;
        }

        public BaseObjectNodeTree CloneTree(BaseObjectNodeTree sourceTree, bool isPlan,
            bool cloneChildren, bool cloneContent, bool isReference,
            MultiLanguageString title, MultiLanguageString description,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags,
            List<ObjectReferenceNodeTree> treeHeaders)
        {
            if (sourceTree.IsPlan())
                isPlan = true;
            string source = (isPlan ? "Plans" : "Courses");
            int index = (isPlan ? Repositories.Plans.Count() : Repositories.Courses.Count());
            string owner = UserRecord.UserName;
            if (title == null)
                title = sourceTree.CloneTitle();
            if (description == null)
                description = sourceTree.CloneDescription();
            string label = (isPlan ? "Plan" : sourceTree.Label);
            string imageFileName = sourceTree.ImageFileName;
            List<IBaseObjectKeyed> options = sourceTree.CloneOptions();
            MarkupTemplate markupTemplate = sourceTree.CloneMarkupTemplate();
            MarkupTemplateReference markupReference = sourceTree.CloneMarkupReference();
            NodeMaster master = sourceTree.Master;
            BaseObjectNodeTree tree = new BaseObjectNodeTree(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                null,                   // string package
                label,                  // string label
                imageFileName,          // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hosttLanguageIDs
                owner,                  // string owner
                null,                   // List<BaseObjectNode> children
                options,                // List<IBaseObjectKeyed> options
                markupTemplate,         // MarkupTemplate markupTemplate
                markupReference,        // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                master,                 // NodeMaster master
                null);                  // List<BaseObjectNode> nodes
            tree.EnsureGuid();

            if (isPlan)
                tree.Directory = "Plan_" + sourceTree.Directory;
            else
                tree.Directory = sourceTree.Directory;

            EnsureUniqueTree(tree, treeHeaders);

            CloneNodeChildrenAndContent(sourceTree, tree, tree, true,
                cloneChildren, cloneContent, isReference, nodeSelectFlags, contentSelectFlags);

            return tree;
        }

        public static BaseObjectNode CreateNode(UserRecord userRecord, UserProfile userProfile,
            BaseObjectNodeTree tree, BaseObjectNode parent, string label)
        {
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", "", userProfile.LanguageDescriptors);
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", "", userProfile.LanguageDescriptors);
            BaseObjectNode node = CreateNode(userRecord, userProfile, tree, parent, title, description, label);
            return node;
        }

        public static BaseObjectNode CreateNode(UserRecord userRecord, UserProfile userProfile,
            BaseObjectNodeTree tree, BaseObjectNode parent, MultiLanguageString title,
            MultiLanguageString description, string label)
        {
            int index = 0;
            List<LanguageID> targetLanguageIDs = tree.CloneTargetLanguageIDs();
            List<LanguageID> hostLanguageIDs = tree.CloneHostLanguageIDs();
            string owner = userRecord.UserName;
            if (parent != null)
                index = parent.ChildCount();
            else if (tree != null)
                index = tree.ChildCount();
            BaseObjectNode node = new BaseObjectNode(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                String.Empty,           // string source
                null,                   // string package
                label,                  // string label
                null,                   // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hosttLanguageIDs
                owner,                  // string owner
                tree,                   // BaseObjectNodeTree tree
                parent,                 // BaseObjectNode parent
                null,                   // List<BaseObjectNode> children
                null,                   // List<IBaseObjectKeyed> options
                null,                   // MarkupTemplate markupTemplate
                null,                   // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                null);                  // NodeMaster master
            node.EnsureGuid();
            return node;
        }

        public BaseObjectNode CloneNode(BaseObjectNode sourceNode, BaseObjectNodeTree tree,
            BaseObjectNode parent, bool addToTree, bool cloneChildren, bool cloneContent,
            bool isReference, Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags)
        {
            int index = 0;
            string owner = UserRecord.UserName;
            if (parent != null)
                index = parent.ChildCount();
            else if (tree != null)
                index = tree.ChildCount();
            MultiLanguageString title = sourceNode.CloneTitle();
            MultiLanguageString description = sourceNode.CloneDescription();
            string label = (sourceNode.IsTree() ? "Group" : sourceNode.Label);
            string imageFileName = sourceNode.ImageFileName;
            List<LanguageID> targetLanguageIDs = sourceNode.CloneTargetLanguageIDs(tree.TargetLanguageIDs);
            List<LanguageID> hostLanguageIDs = sourceNode.CloneHostLanguageIDs(tree.HostLanguageIDs);
            List<IBaseObjectKeyed> options = sourceNode.CloneOptions();
            MarkupTemplate markupTemplate = sourceNode.CloneMarkupTemplate();
            MarkupTemplateReference markupReference = sourceNode.CloneMarkupReference();
            NodeMaster master = sourceNode.Master;
            BaseObjectNode node = new BaseObjectNode(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                String.Empty,           // string source
                null,                   // string package
                label,                  // string label
                imageFileName,          // string imageFileName
                index,                  // int index
                false,                  // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hosttLanguageIDs
                owner,                  // string owner
                tree,                   // BaseObjectNodeTree tree
                parent,                 // BaseObjectNode parent
                null,                   // List<BaseObjectNode> children
                options,                // List<IBaseObjectKeyed> options
                markupTemplate,         // MarkupTemplate markupTemplate
                markupReference,        // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                master);                // NodeMaster master

            node.EnsureGuid();
            node.Directory = sourceNode.Directory;
            node.IsReference = isReference;

            if (addToTree)
            {
                if (isReference)
                    EnsureUniqueNode(tree, node);

                if (parent != null)
                    parent.AddChildNode(node);
                else
                    tree.AddChildNode(node);
            }

            CloneNodeChildrenAndContent(sourceNode, tree, node, addToTree,
                cloneChildren, cloneContent, isReference, nodeSelectFlags, contentSelectFlags);

            return node;
        }

        public bool CloneNodeChildrenAndContent(BaseObjectNode sourceNode,
            BaseObjectNodeTree tree, BaseObjectNode node,
            bool addToTree, bool cloneChildren, bool cloneContent, bool isReference,
            Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags)
        {
            if (node == null)
                node = tree;

            if (cloneContent && sourceNode.HasContent())
            {
                foreach (BaseObjectContent sourceContent in sourceNode.ContentChildren)
                {
                    if (contentSelectFlags != null)
                    {
                        bool useIt;

                        if (contentSelectFlags.TryGetValue(sourceContent.KeyString, out useIt))
                        {
                            if (!useIt)
                                continue;
                        }
                    }

                    BaseObjectContent existingContent = node.GetContent(sourceContent.KeyString);

                    if (existingContent != null)
                    {
                        bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);
                        DeleteNodeContentHelper(node, existingContent, deleteMediaFiles);
                    }

                    BaseObjectContent content = CopyContent(node, sourceContent, isReference);

                    if (content != null)
                        node.AddContentChild(content);

                    if (sourceContent.HasContentChildren())
                        CopyContentChildren(node, content, sourceContent, isReference, contentSelectFlags);
                }
            }

            if (cloneChildren && sourceNode.HasChildren())
            {
                foreach (BaseObjectNode sourceChildNode in sourceNode.Children)
                {
                    bool useIt = true;

                    if (nodeSelectFlags != null)
                        nodeSelectFlags.TryGetValue(sourceChildNode.KeyInt, out useIt);

                    if (useIt)
                        CloneNode(sourceChildNode, tree, node,
                            addToTree, cloneChildren, cloneContent, isReference, nodeSelectFlags, contentSelectFlags);
                    else
                        CloneChildNodeChildrenAndContent(sourceChildNode,
                            tree, node,
                            addToTree, cloneChildren, cloneContent, isReference,
                            nodeSelectFlags, contentSelectFlags);
                }
            }

            return true;
        }

        public bool CloneChildNodeChildrenAndContent(BaseObjectNode sourceNode,
            BaseObjectNodeTree tree, BaseObjectNode node,
            bool addToTree, bool cloneChildren, bool cloneContent, bool isReference,
            Dictionary<int, bool> nodeSelectFlags, Dictionary<string, bool> contentSelectFlags)
        {
            if (node == null)
                node = tree;

            if (cloneChildren && sourceNode.HasChildren())
            {
                foreach (BaseObjectNode sourceChildNode in sourceNode.Children)
                {
                    bool useIt = true;

                    if (nodeSelectFlags != null)
                        nodeSelectFlags.TryGetValue(sourceChildNode.KeyInt, out useIt);

                    if (useIt)
                        CloneNode(sourceChildNode, tree, node,
                            addToTree, cloneChildren, cloneContent, isReference, nodeSelectFlags, contentSelectFlags);
                    else
                        CloneChildNodeChildrenAndContent(sourceChildNode,
                            tree, node,
                            addToTree, cloneChildren, cloneContent, isReference,
                            nodeSelectFlags, contentSelectFlags);
                }
            }

            return true;
        }

        public BaseObjectContent CloneContent(BaseObjectContent sourceContent, BaseObjectNodeTree tree,
            BaseObjectNode parent, bool addToTree, bool cloneContent,
            bool isReference, Dictionary<string, bool> contentSelectFlags)
        {
            BaseObjectContent content = CopyContent(parent, sourceContent, isReference);

            if (addToTree)
            {
                EnsureUniqueContent(parent, content);
                parent.AddContentChild(content);

                if (cloneContent && sourceContent.HasContentChildren())
                    CopyContentChildren(tree, content, sourceContent, isReference, contentSelectFlags);
            }

            return content;
        }

        public BaseObjectContent CloneSubContent(BaseObjectContent sourceContent, BaseObjectNodeTree tree,
            BaseObjectContent parent, bool addToTree, bool cloneContent,
            bool isReference, Dictionary<string, bool> contentSelectFlags)
        {
            BaseObjectContent content = CopyContent(parent.Node, sourceContent, isReference);

            if (addToTree)
            {
                EnsureUniqueContent(parent.Node, content);
                parent.AddContentChild(content);

                if (cloneContent && sourceContent.HasContentChildren())
                    CopyContentChildren(tree, content, sourceContent, isReference, contentSelectFlags);
            }

            return content;
        }

        public bool CopyContentChildren(BaseObjectNode node, BaseObjectContent content,
            BaseObjectContent sourceContent, bool isReference, Dictionary<string, bool> contentSelectFlags)
        {
            if (sourceContent.HasContentChildren())
            {
                foreach (BaseObjectContent sourceContentChild in sourceContent.ContentChildren)
                {
                    BaseObjectContent contentChild = CopyContent(node, sourceContentChild, isReference);

                    if (contentChild != null)
                    {
                        content.AddContentChild(contentChild);

                        if (sourceContentChild.HasContentChildren())
                            CopyContentChildren(node, contentChild, sourceContentChild, isReference, contentSelectFlags);
                    }
                }
            }

            return true;
        }

        // Returns true if names changed.
        public bool EnsureUniqueTree(BaseObjectNodeTree tree, List<ObjectReferenceNodeTree> treeHeaders)
        {
            LanguageID uiLanguageID = UserProfile.UILanguageID;
            string title = tree.GetTitleString(uiLanguageID);
            string originalTitle = title;
            string directory = tree.Directory;
            string originalDirectory = directory;
            bool returnValue = false;

            if (tree != null)
            {
                int ordinal = 1;
                bool changed = false;

                do
                {
                    changed = false;

                    foreach (ObjectReferenceNodeTree testTree in treeHeaders)
                    {
                        if (testTree.Key == tree.Key)
                            continue;

                        if ((testTree.GetTitleString(uiLanguageID) == title) || (testTree.Directory == directory))
                        {
                            title = originalTitle + " (" + ordinal.ToString() + ")";
                            directory = originalDirectory + "_" + ordinal.ToString();
                            changed = true;
                            break;
                        }
                    }

                    ordinal++;
                }
                while (changed);

                if (title != originalTitle)
                {
                    if (tree.Title != null)
                    {
                        tree.Title.SetText(uiLanguageID, title);
                        returnValue = true;
                    }
                }

                if (directory != originalDirectory)
                {
                    tree.Directory = directory;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        // Returns true if names changed.
        public bool EnsureUniqueNode(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            LanguageID uiLanguageID = UserProfile.UILanguageID;
            string title = node.GetTitleString(uiLanguageID);
            string originalTitle = title;
            string directory = node.Directory;
            string originalDirectory = directory;
            bool returnValue = false;

            if (tree.Nodes != null)
            {
                int ordinal = 1;
                bool changed = false;

                do
                {
                    changed = false;

                    foreach (BaseObjectNode testNode in tree.Nodes)
                    {
                        if (testNode == node)
                            continue;

                        if ((testNode.GetTitleString(uiLanguageID) == title) || (testNode.Directory == directory))
                        {
                            title = originalTitle + " (" + ordinal.ToString() + ")";
                            directory = originalDirectory + "_" + ordinal.ToString();
                            changed = true;
                            break;
                        }
                    }

                    ordinal++;
                }
                while (changed);

                if (title != originalTitle)
                {
                    if (node.Title != null)
                    {
                        node.Title.SetText(uiLanguageID, title);
                        returnValue = true;
                    }
                }

                if (directory != originalDirectory)
                {
                    node.Directory = directory;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public void ResetAllNodesFromMaster(BaseObjectNode node)
        {
            if (node == null)
                return;

            SetupNodeFromMaster(node);

            if (node.HasChildren())
            {
                foreach (BaseObjectNode childNode in node.Children)
                {
                    childNode.ResolveReferences(Repositories, false, false);
                    ResetAllNodesFromMaster(childNode);
                }
            }
        }

        public void SetupNodeFromMaster(BaseObjectNode node)
        {
            if (node == null)
                return;

            NodeMaster master = node.Master;

            if ((master == null) || (master.ContentItems == null))
                return;

            RemoveContentNotInMaster(node);

            if (master.HasOptionDescriptors())
                node.ResetOptionsFromDescriptors(master.OptionDescriptors);

            foreach (MasterContentItem contentItem in master.ContentItems)
                SetupContentFromContentItem(node, null, contentItem);

            node.SortContentChildrenByIndex(true);

            node.SetupMarkupTemplateFromMaster();
        }

        public void CheckNodeFromMaster(BaseObjectNode node)
        {
            if (node == null)
                return;

            NodeMaster master = node.Master;

            if ((master == null) || (master.ContentItems == null))
                return;

            RemoveContentNotInMaster(node);

            if (master.HasOptionDescriptors())
                node.ResetOptionsFromDescriptors(master.OptionDescriptors);

            foreach (MasterContentItem contentItem in master.ContentItems)
                CheckContentFromContentItem(node, null, contentItem);

            node.SortContentChildrenByIndex(true);

            node.SetupMarkupTemplateFromMaster();
        }

        public void RemoveContentNotInMaster(BaseObjectNode node)
        {
            if (node == null)
                return;

            NodeMaster master = node.Master;

            if ((master == null) || (master.ContentItems == null))
                return;

            List<BaseObjectContent> contentList = node.ContentList;
            bool deleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);

            if (contentList != null)
            {
                int count = contentList.Count;
                int index;

                for (index = count - 1; index >= 0; index--)
                {
                    BaseObjectContent content = contentList[index];
                    MasterContentItem contentItem = master.GetContentItem(content.KeyString);

                    if (contentItem == null)
                        DeleteNodeContentHelper(node, content, deleteMediaFiles);
                }
            }
        }

        public void SetupContentFromMaster(BaseObjectNode node, BaseObjectContent content, string contentKey)
        {
            if (node == null)
                return;

            NodeMaster master = node.Master;

            if ((master == null) || (master.ContentItems == null))
                return;

            MasterContentItem contentItem = master.GetContentItem(contentKey);

            if (contentItem == null)
            {
                if (content.ContentStorage == null)
                    content.SetupContentStorage();
                return;
            }

            content.SetupContent(contentItem, UserProfile);
        }

        public void SetupContentFromContentItem(BaseObjectNode node, BaseObjectContent contentParent,
            MasterContentItem contentItem)
        {
            BaseObjectContent content = node.GetContent(contentItem.KeyString);

            if (content == null)
            {
                content = CreateContentFromMasterItem(node, null, contentItem, true);

                if (contentParent != null)
                    contentParent.InsertContentChildIndexed(contentItem.Index, content);
                else
                    node.InsertContentChildIndexed(contentItem.Index, content);
            }
            else
            {
                if (contentParent != null)
                {
                    if (contentParent.ContentChildrenKeys != null)
                    {
                        if (!contentParent.ContentChildrenKeys.Contains(content.KeyString))
                            contentParent.ContentChildrenKeys.Insert(content.Index, content.KeyString);
                    }
                    else
                        contentParent.ContentChildrenKeys = new List<string>() { content.KeyString };
                }
                else
                {
                    if (node.ContentChildrenKeys != null)
                    {
                        if (!node.ContentChildrenKeys.Contains(content.KeyString))
                            node.ContentChildrenKeys.Insert(content.Index, content.KeyString);
                    }
                    else
                        node.ContentChildrenKeys = new List<string>() { content.KeyString };
                }

                content.SetupContent(contentItem, UserProfile);

                if (contentItem.ContentItemCount() != 0)
                {
                    foreach (MasterContentItem subContentItem in contentItem.ContentItems)
                        SetupContentFromContentItem(node, content, subContentItem);
                }
            }
        }

        public void CheckContentFromContentItem(BaseObjectNode node, BaseObjectContent contentParent,
            MasterContentItem contentItem)
        {
            BaseObjectContent content = node.GetContent(contentItem.KeyString);

            if (content == null)
            {
                content = CreateContentFromMasterItem(node, null, contentItem, true);

                if (contentParent != null)
                    contentParent.InsertContentChildIndexed(contentItem.Index, content);
                else
                    node.InsertContentChildIndexed(contentItem.Index, content);
            }
            else
            {
                if (contentParent != null)
                {
                    if (contentParent.ContentChildrenKeys != null)
                    {
                        if (!contentParent.ContentChildrenKeys.Contains(content.KeyString))
                            contentParent.ContentChildrenKeys.Insert(content.Index, content.KeyString);
                    }
                    else
                        contentParent.ContentChildrenKeys = new List<string>() { content.KeyString };
                }
                else
                {
                    if (node.ContentChildrenKeys != null)
                    {
                        if (!node.ContentChildrenKeys.Contains(content.KeyString))
                            node.ContentChildrenKeys.Insert(content.Index, content.KeyString);
                    }
                    else
                        node.ContentChildrenKeys = new List<string>() { content.KeyString };
                }

                content.CheckContent(contentItem, UserProfile);

                if (contentItem.ContentItemCount() != 0)
                {
                    foreach (MasterContentItem subContentItem in contentItem.ContentItems)
                        CheckContentFromContentItem(node, content, subContentItem);
                }
            }
        }

        public BaseObjectContent CreateContent(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            string contentType,
            string contentSubType)
        {
            ContentClassType contentClass = ContentUtilities.GetContentClassFromContentType(contentType);
            if (String.IsNullOrEmpty(contentSubType))
                contentSubType = ContentUtilities.GetContentSubTypeFromType(contentType);
            return CreateContent(node, parentContent, contentClass, contentType, contentSubType);
        }

        public BaseObjectContent CreateContent(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            ContentClassType contentClass,
            string contentType,
            string contentSubType)
        {
            string nodeContentKey = MasterContentItem.ComposeKey(node, contentType, contentSubType, UserProfile, false);
            return CreateContent(node, parentContent, nodeContentKey, contentClass, contentType, contentSubType);
        }


        public BaseObjectContent CreateContent(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            string nodeContentKey,
            ContentClassType contentClass,
            string contentType,
            string contentSubType)
        {
            LanguageID targetLanguageID = UserProfile.TargetLanguageID;
            if (targetLanguageID == null)
                targetLanguageID = UserProfile.HostLanguageID;
            LanguageID hostLanguageID = UserProfile.HostLanguageID;
            LanguageID uiLanguageID = UserProfile.UILanguageID;
            string label = GetLabelFromContentType(contentType, contentSubType);
            if (String.IsNullOrEmpty(contentSubType))
                contentSubType = ContentUtilities.GetContentSubTypeFromType(contentType);
            string titleString = S(MasterContentItem.ComposeTitleString(node, nodeContentKey, contentType, contentSubType, UserProfile, false));
            MultiLanguageString title = ObjectUtilities.CreateMultiLanguageString("Title", titleString,
                UserProfile.LanguageDescriptors);
            string descriptionString = S(MasterContentItem.ComposeDescriptionString(node, nodeContentKey, contentType, contentSubType, UserProfile, false));
            MultiLanguageString description = ObjectUtilities.CreateMultiLanguageString("Description", descriptionString,
                UserProfile.LanguageDescriptors);
            string source = GetSourceFromContentClass(contentClass);
            string package = node.Package;
            int index = node.ContentCount();
            List<LanguageID> targetLanguageIDs;
            List<LanguageID> hostLanguageIDs;
            if (contentClass == ContentClassType.MediaItem)
            {
                List<BaseObjectContent> contentList = node.ContentList;
                List<LanguageID> testTargetLanguageIDs = UserProfile.TargetLanguageIDs;
                if ((testTargetLanguageIDs != null) && (testTargetLanguageIDs.Count != 0))
                {
                    foreach (LanguageID testTargetLanguageID in testTargetLanguageIDs)
                    {
                        string abbrev = testTargetLanguageID.MediaLanguageAbbreviation(uiLanguageID);
                        string testKey = contentType + contentSubType + abbrev;
                        if (hostLanguageID != null)
                        {
                            string hostAbbrev = hostLanguageID.MediaLanguageAbbreviation(uiLanguageID);
                            testKey += hostAbbrev;
                        }
                        if (contentList != null)
                        {
                            if (contentList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                continue;
                        }
                        targetLanguageID = testTargetLanguageID;
                        break;
                    }
                }
                targetLanguageIDs = new List<LanguageID>(1) { new LanguageID(targetLanguageID) };
                hostLanguageIDs = new List<LanguageID>(1) { new LanguageID(hostLanguageID) };
            }
            else
            {
                targetLanguageIDs = node.CloneTargetLanguageIDs();
                hostLanguageIDs = node.CloneHostLanguageIDs();
            }
            string owner = UserRecord.UserName;
            List<MasterContentItem> contentItems = null;
            MarkupTemplateReference markupReference = null;
            MarkupTemplateReference copyMarkupReference = null;
            List<OptionDescriptor> optionDescriptors = MasterContentItem.GetDefaultDescriptors(contentType, contentSubType, UserProfile);
            MasterContentItem contentItem = new MasterContentItem(
                nodeContentKey,         // string nodeContentKey,
                contentType,            // string contentType,
                contentSubType,         // string contentSubType,
                title,                  // MultiLanguageString title,
                description,            // MultiLanguageString description,
                source,                 // string source,
                package,                // string package,
                label,                  // string label,
                null,                   // string imageFileName,
                index,                  // int index,
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs,
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs,
                owner,                  // string owner,
                contentItems,           // List<MasterContentItem> contentItems,
                markupReference,        // MarkupTemplateReference markupReference,
                copyMarkupReference,    // MarkupTemplateReference copyMarkupReference,
                optionDescriptors       // List<OptionDescriptor> optionDescriptors
            );
            BaseObjectContent content = CreateContentFromMasterItem(node, parentContent, contentItem, false);
            return content;
        }

        public BaseObjectContent CreateContent(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            string contentType,
            string contentSubType,
            int nth,
            out int ordinal)
        {
            ContentClassType contentClass = ContentUtilities.GetContentClassFromContentType(contentType);
            if (String.IsNullOrEmpty(contentSubType))
                contentSubType = ContentUtilities.GetContentSubTypeFromType(contentType);
            return CreateContent(node, parentContent, contentClass, contentType, contentSubType, nth, out ordinal);
        }

        public BaseObjectContent CreateContent(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            ContentClassType contentClass,
            string contentType,
            string contentSubType,
            int nth,
            out int ordinal)
        {
            string keyBase = MasterContentItem.ComposeKey(node, contentType, contentSubType, UserProfile, false);
            string nodeContentKey = keyBase + nth.ToString();
            for (; node.GetContent(nodeContentKey) != null; ++nth, nodeContentKey = keyBase + nth.ToString())
                ;
            ordinal = nth;
            return CreateContent(node, parentContent, nodeContentKey, contentClass, contentType, contentSubType);
        }

        public BaseObjectContent CreateContentFromMasterItem(
            BaseObjectNode node,
            BaseObjectContent parentContent,
            MasterContentItem contentItem,
            bool doAdd)
        {
            BaseObjectContent content = null;
            ContentClassType contentClass = contentItem.ContentClass;

            contentItem.ResolveReferences(Repositories, false, false);

            content = new BaseObjectContent(node, null);

            content.SetupContent(contentItem, UserProfile);

            if (parentContent != null)
                parentContent.AddContentChild(content);

            if (doAdd)
                AddContent(content);

            if (contentItem.ContentItemCount() != 0)
            {
                foreach (MasterContentItem subContentItem in contentItem.ContentItems)
                    CreateContentFromMasterItem(node, content, subContentItem, doAdd);
            }

            return content;
        }

        public bool EnsureUniqueContent(BaseObjectNode node, BaseObjectContent content)
        {
            string key = content.KeyString;
            string originalKey = key;
            LanguageID uiLanguageID = UserProfile.UILanguageID;
            string title = content.GetTitleString(uiLanguageID);
            string originalTitle = title;
            string directory = content.Directory;
            string originalDirectory = directory;
            List<BaseObjectContent> contentList = node.ContentList;
            bool isMedia = ((content.ContentClass == ContentClassType.MediaItem) || (content.ContentClass == ContentClassType.MediaList));
            bool returnValue = false;

            if (contentList != null)
            {
                int ordinal = 1;
                bool changed = false;

                do
                {
                    changed = false;

                    foreach (BaseObjectContent testContent in contentList)
                    {
                        if (testContent == content)
                            continue;

                        if (testContent.KeyString == key)
                        {
                            key = originalKey + ordinal.ToString();
                            changed = true;
                        }

                        if (testContent.GetTitleString(uiLanguageID) == title)
                        {
                            title = originalTitle + " (" + ordinal.ToString() + ")";
                            changed = true;
                        }

                        if (!isMedia && (testContent.Directory == directory))
                        {
                            directory = originalDirectory + "_" + ordinal.ToString();
                            changed = true;
                        }

                        testContent.NewGuid();
                    }

                    ordinal++;
                }
                while (changed);

                if (key != originalKey)
                {
                    content.Key = key;
                    returnValue = true;
                }

                if (title != originalTitle)
                {
                    if (content.Title != null)
                    {
                        content.Title.SetText(uiLanguageID, title);
                        returnValue = true;
                    }
                }

                if (!isMedia && (directory != originalDirectory))
                {
                    content.Directory = directory;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public BaseObjectContent CopyContent(BaseObjectNode targetNode, BaseObjectContent sourceContent,
            bool isReference)
        {
            MultiLanguageString title = ObjectUtilities.CloneMultiLanguageString(sourceContent.Title);
            MultiLanguageString description = ObjectUtilities.CloneMultiLanguageString(sourceContent.Description);
            List<LanguageID> targetLanguageIDs = sourceContent.CloneTargetLanguageIDs(targetNode.TargetLanguageIDs);
            List<LanguageID> hostLanguageIDs = sourceContent.CloneHostLanguageIDs(targetNode.HostLanguageIDs);
            MarkupTemplate markupTemplate =
                (sourceContent.LocalMarkupTemplate != null ? new MarkupTemplate(sourceContent.LocalMarkupTemplate) : null);
            MarkupTemplateReference markupReference =
                (sourceContent.MarkupReference != null ? new MarkupTemplateReference(sourceContent.MarkupReference) : null);
            BaseContentStorage copiedContentStorage = null;
            List<IBaseObjectKeyed> options = sourceContent.CloneOptions();
            BaseObjectContent contentParent = null;
            BaseContentStorage sourceContentStorage = sourceContent.ContentStorage;

            if (sourceContent.HasContentParent())
                contentParent = targetNode.GetContent(sourceContent.ContentParentKey);

            if (sourceContentStorage != null)
            {
                if (isReference)
                {
                    copiedContentStorage = sourceContentStorage;
                    copiedContentStorage.ReferenceTreeKey = sourceContent.Tree.Key;
                    copiedContentStorage.ReferenceTree = sourceContent.Tree;
                    copiedContentStorage.ReferenceCount = copiedContentStorage.ReferenceCount + 1;
                    copiedContentStorage.Modified = false;
                    Repositories.UpdateReference(copiedContentStorage.Source, null, copiedContentStorage);
                }
                else
                {
                    copiedContentStorage = sourceContentStorage.Clone() as BaseContentStorage;
                    copiedContentStorage.NewGuid();
                }
            }

            BaseObjectContent copiedContent = new BaseObjectContent(
                sourceContent.KeyString,        // string key,
                title,                          // MultiLanguageString title,
                description,                    // MultiLanguageString description,
                sourceContent.Source,           // string source,
                sourceContent.Package,          // string package,
                sourceContent.Label,            // string label,
                sourceContent.ImageFileName,    // string imageFileName,
                sourceContent.Index,            // int index,
                sourceContent.IsPublic,         // bool isPublic,
                targetLanguageIDs,              // List<LanguageID> targetLanguageIDs,
                hostLanguageIDs,                // List<LanguageID> hostLanguageIDs,
                sourceContent.Owner,            // string owner,
                targetNode,                     // BaseObjectNode node,
                sourceContent.ContentType,      // string contentType,
                sourceContent.ContentSubType,   // string contentSubType,
                copiedContentStorage,           // BaseContentStorage contentStorage,
                options,                        // List<IBaseObjectKeyed> options,
                markupTemplate,                 // MarkupTemplate markupTemplate,
                markupReference,                // MarkupTemplateReference markupReference,
                contentParent,                  // BaseObjectContent contentParent,
                null);                          // List<BaseObjectContent> contentChildren

            copiedContent.EnsureGuid();

            if (isReference)
            {
                copiedContent.IsReference = true;
                copiedContent.Directory = sourceContent.Directory;
                copiedContent.ReferenceMediaTildeUrl = sourceContent.MediaTildeUrl;
                copiedContent.ReferenceTreeKey = sourceContent.Tree.Key;
                copiedContent.ReferenceTree = sourceContent.Tree;

                //if (copiedContentStorage != null)
                //{
                //    copiedContentStorage.NewGuid();
                //    copiedContentStorage.ConvertToReference(sourceContentStorage);
                //}

                //if (!AddContentStorage(copiedContent, copiedContentStorage))
                //    copiedContent = null;
            }
            else
            {
                copiedContent.SetupDirectory();

                EnsureUniqueContent(targetNode, copiedContent);

                if (sourceContentStorage != null)
                {
                    if (AddContentStorage(copiedContent, copiedContentStorage))
                        CopyContentMedia(sourceContent, copiedContent);
                    else
                        copiedContent = null;
                }
            }

            return copiedContent;
        }

        public bool CopyContentMedia(BaseObjectContent sourceContent, BaseObjectContent targetContent)
        {
            if ((sourceContent == null) || (targetContent == null))
                return false;

            string targetDirectory = targetContent.MediaDirectoryPath;
            List<string> copiedFiles = new List<string>();
            string errorMessage = String.Empty;

            if (!FileSingleton.DirectoryExists(targetDirectory))
                FileSingleton.CreateDirectory(targetDirectory);

            if (sourceContent.CopyMedia(targetDirectory, copiedFiles, ref errorMessage))
                return true;

            Error = errorMessage;
            return false;
        }

        public bool ConvertContentToReference(BaseObjectContent content,
            ref ContentStudyList contentStorage, out bool converted, out string errorMessage)
        {
            BaseContentStorage copiedContentStorage = null;

            converted = false;
            errorMessage = null;

            if ((content == null) || (contentStorage == null))
            {
                errorMessage = S("Null content or content storage.");
                return false;
            }

            if (contentStorage.ReferenceCount > 1)
            {
                BaseContentStorage sourceContentStorage = contentStorage;
                copiedContentStorage = sourceContentStorage.Clone() as BaseContentStorage;
                copiedContentStorage.Key = 0;
                copiedContentStorage.NewGuid();
                copiedContentStorage.ConvertToReference(sourceContentStorage);
                copiedContentStorage.TouchAndClearModified();

                if (!Repositories.SaveReference(copiedContentStorage.Source, null, copiedContentStorage))
                {
                    errorMessage = S("Error saving copied content storage.");
                    return false;
                }

                sourceContentStorage.ReferenceCount = sourceContentStorage.ReferenceCount - 1;

                if (!Repositories.UpdateReference(sourceContentStorage.Source, null, sourceContentStorage))
                {
                    errorMessage = S("Error updated content storage reference count.");
                    return false;
                }

                content.ContentStorage = copiedContentStorage;
                content.IsReference = false;
                contentStorage = copiedContentStorage as ContentStudyList;
                converted = true;
            }
            else if (content.IsReference)
                content.IsReference = false;

            return true;
        }

        public bool AddContent(BaseObjectContent content)
        {
            BaseContentStorage contentStorage = content.ContentStorage;

            if (contentStorage == null)
                return true;

            return AddContentStorage(content, contentStorage);
        }

        public bool AddContentStorage(BaseObjectContent content, BaseContentStorage contentStorage)
        {
            bool returnValue = false;

            if (contentStorage == null)
                return true;

            contentStorage.Key = 0;
            contentStorage.EnsureGuid();
            contentStorage.TouchAndClearModified();

            try
            {
                if (Repositories.SaveReference(contentStorage.Source, null, contentStorage))
                {
                    content.ContentStorage = contentStorage;
                    contentStorage.Modified = false;
                    returnValue = true;
                }
                else
                    Error = S("Error adding content") + ": " + content.GetTitleString(UILanguageID);
            }
            catch (Exception exception)
            {
                Error += S("Error adding content") + ": " + content.GetTitleString(UILanguageID)
                    + ": " + exception.Message;

                if (exception.InnerException != null)
                    Error += Error + ": " + exception.InnerException.Message + "\n";
                else
                    Error += "\n";
            }

            return returnValue;
        }

        public static string GetSourceFromContentClass(ContentClassType contentClass)
        {
            string source;

            switch (contentClass)
            {
                case ContentClassType.DocumentItem:
                    source = "DocumentItems";
                    break;
                case ContentClassType.MediaList:
                    source = "MediaLists";
                    break;
                case ContentClassType.MediaItem:
                    source = "MediaItems";
                    break;
                case ContentClassType.StudyList:
                    source = "StudyLists";
                    break;
                default:
                    source = String.Empty;
                    break;
            }

            return source;
        }

        public static string GetLabelFromContentType(string contentType, string contentSubType)
        {
            string label;

            switch (contentType)
            {
                case "Document":
                    label = contentType;
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    label = contentType + contentSubType;
                    break;
                case "Transcript":
                case "Text":
                    label = "Paragraphs";
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    label = contentType;
                    break;
                default:
                    label = contentType;
                    break;
            }

            return label;
        }

        public void SubDivideStudyList(ContentStudyList studyList, string titlePrefix, NodeMaster master,
            bool studyListsOnly, int itemSubDivideCount, int minorSubDivideCount, int majorSubDivideCount)
        {
            if (studyListsOnly)
                SubDivideStudyListIntoContentChildren(studyList,
                    itemSubDivideCount, minorSubDivideCount, majorSubDivideCount);
            else
                SubDivideStudyListIntoNodeChildren(studyList, titlePrefix, master,
                    itemSubDivideCount, minorSubDivideCount, majorSubDivideCount);
        }

        public void SubDivideStudyListIntoContentChildren(ContentStudyList studyList,
            int itemSubDivideCount, int minorSubDivideCount, int majorSubDivideCount)
        {
            BaseObjectContent content = studyList.Content;
            BaseObjectNode node = content.Node;
            int totalStudyItemCount = studyList.StudyItemCount();
            int studyItemIndex = 0;
            int totalLeafStudyListCount = (totalStudyItemCount + (itemSubDivideCount - 1)) / itemSubDivideCount;
            int leafStudyItemCount;
            List<MultiLanguageItem> leafStudyItems;

            if (totalStudyItemCount <= itemSubDivideCount)
                return;

            if ((minorSubDivideCount > 0) && (totalLeafStudyListCount > minorSubDivideCount))
            {
                int minorStudyListCount = (totalLeafStudyListCount + (minorSubDivideCount - 1)) / minorSubDivideCount;

                if ((majorSubDivideCount > 0) && (minorStudyListCount > majorSubDivideCount))
                {
                    int majorStudyListCount = (minorStudyListCount + (majorSubDivideCount - 1)) / majorSubDivideCount;

                    if (majorStudyListCount > majorSubDivideCount)
                    {
                        int majorMinorSubDivideCount = majorSubDivideCount * minorSubDivideCount;
                        int superStudyListCount = (totalLeafStudyListCount + (majorMinorSubDivideCount - 1)) / majorMinorSubDivideCount;

                        for (int majorIndex = 0; majorIndex < superStudyListCount; majorIndex++)
                        {
                            BaseObjectContent majorContent = CreateSubStudyListContent(content, majorIndex, null);

                            for (int minorIndex = 0; minorIndex < majorSubDivideCount; minorIndex++)
                            {
                                BaseObjectContent minorContent = CreateSubStudyListContent(majorContent, minorIndex, null);

                                for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                                {
                                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                                    if (leafStudyItemCount > itemSubDivideCount)
                                        leafStudyItemCount = itemSubDivideCount;
                                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                                    CreateSubStudyListContent(minorContent, leafIndex, leafStudyItems);
                                    studyItemIndex += leafStudyItemCount;
                                    if (studyItemIndex >= totalStudyItemCount)
                                        goto Exit;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int majorIndex = 0; majorIndex < majorStudyListCount; majorIndex++)
                        {
                            BaseObjectContent majorContent = CreateSubStudyListContent(content, majorIndex, null);

                            for (int minorIndex = 0; minorIndex < majorSubDivideCount; minorIndex++)
                            {
                                BaseObjectContent minorContent = CreateSubStudyListContent(majorContent, minorIndex, null);

                                for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                                {
                                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                                    if (leafStudyItemCount > itemSubDivideCount)
                                        leafStudyItemCount = itemSubDivideCount;
                                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                                    CreateSubStudyListContent(minorContent, leafIndex, leafStudyItems);
                                    studyItemIndex += leafStudyItemCount;
                                    if (studyItemIndex >= totalStudyItemCount)
                                        goto Exit;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int minorIndex = 0; minorIndex < minorStudyListCount; minorIndex++)
                    {
                        BaseObjectContent minorContent = CreateSubStudyListContent(content, minorIndex, null);

                        for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                        {
                            leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                            if (leafStudyItemCount > itemSubDivideCount)
                                leafStudyItemCount = itemSubDivideCount;
                            leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                            CreateSubStudyListContent(minorContent, leafIndex, leafStudyItems);
                            studyItemIndex += leafStudyItemCount;
                            if (studyItemIndex >= totalStudyItemCount)
                                goto Exit;
                        }
                    }
                }
            }
            else
            {
                for (int leafIndex = 0; leafIndex < totalLeafStudyListCount; leafIndex++)
                {
                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                    if (leafStudyItemCount > itemSubDivideCount)
                        leafStudyItemCount = itemSubDivideCount;
                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                    CreateSubStudyListContent(content, leafIndex, leafStudyItems);
                    studyItemIndex += leafStudyItemCount;
                    if (studyItemIndex >= totalStudyItemCount)
                        goto Exit;
                }
            }

        Exit:
            studyList.DeleteAllStudyItems();
            UpdateContentHelper(content);
        }

        public void SubDivideStudyListIntoNodeChildren(ContentStudyList studyList, string titlePrefix, NodeMaster master,
            int itemSubDivideCount, int minorSubDivideCount, int majorSubDivideCount)
        {
            BaseObjectContent content = studyList.Content;
            BaseObjectNode node = content.Node;
            int totalStudyItemCount = studyList.StudyItemCount();
            int totalLeafStudyListCount = (totalStudyItemCount + (itemSubDivideCount - 1)) / itemSubDivideCount;
            int studyItemIndex = 0;
            int leafStudyItemCount;
            List<MultiLanguageItem> leafStudyItems;
            int totalLessonCount = totalLeafStudyListCount;
            int totalMinorGroupCount = (totalLessonCount + (minorSubDivideCount - 1)) / minorSubDivideCount;
            int totalMajorGroupCount = (totalMinorGroupCount + (majorSubDivideCount - 1)) / majorSubDivideCount;

            if (totalStudyItemCount <= itemSubDivideCount)
                return;

            if ((minorSubDivideCount > 0) && (totalLeafStudyListCount > minorSubDivideCount))
            {
                int minorStudyListCount = (totalLeafStudyListCount + (minorSubDivideCount - 1)) / minorSubDivideCount;

                if ((majorSubDivideCount > 0) && (minorStudyListCount > majorSubDivideCount))
                {
                    int majorStudyListCount = (minorStudyListCount + (majorSubDivideCount - 1)) / majorSubDivideCount;

                    if (majorStudyListCount > majorSubDivideCount)
                    {
                        int majorMinorSubDivideCount = majorSubDivideCount * minorSubDivideCount;
                        int superStudyListCount = (totalLeafStudyListCount + (majorMinorSubDivideCount - 1)) / majorMinorSubDivideCount;

                        for (int majorIndex = 0; majorIndex < superStudyListCount; majorIndex++)
                        {
                            BaseObjectContent majorContent = CreateSubNodeAndContent(
                                node, content, titlePrefix, majorIndex, master, null);
                            BaseObjectNode majorNode = majorContent.Node;
                            string minorTitlePrefix = titlePrefix + "-" + (majorIndex + 1).ToString();

                            for (int minorIndex = 0; minorIndex < majorSubDivideCount; minorIndex++)
                            {
                                BaseObjectContent minorContent = CreateSubNodeAndContent(
                                    majorNode, majorContent, minorTitlePrefix, minorIndex, master, null);
                                BaseObjectNode minorNode = minorContent.Node;
                                string leafTitlePrefix = minorTitlePrefix + "-" + (minorIndex + 1).ToString();

                                for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                                {
                                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                                    if (leafStudyItemCount > itemSubDivideCount)
                                        leafStudyItemCount = itemSubDivideCount;
                                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                                    CreateSubNodeAndContent(minorNode, minorContent, leafTitlePrefix, leafIndex, master, leafStudyItems);
                                    studyItemIndex += leafStudyItemCount;
                                    if (studyItemIndex >= totalStudyItemCount)
                                        goto Exit;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int majorIndex = 0; majorIndex < majorStudyListCount; majorIndex++)
                        {
                            BaseObjectContent majorContent = CreateSubNodeAndContent(
                                node, content, titlePrefix, majorIndex, master, null);
                            BaseObjectNode majorNode = majorContent.Node;
                            string minorTitlePrefix = titlePrefix + "-" + (majorIndex + 1).ToString();

                            for (int minorIndex = 0; minorIndex < majorSubDivideCount; minorIndex++)
                            {
                                BaseObjectContent minorContent = CreateSubNodeAndContent(
                                    majorNode, majorContent, minorTitlePrefix, minorIndex, master, null);
                                BaseObjectNode minorNode = minorContent.Node;
                                string leafTitlePrefix = minorTitlePrefix + "-" + (minorIndex + 1).ToString();

                                for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                                {
                                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                                    if (leafStudyItemCount > itemSubDivideCount)
                                        leafStudyItemCount = itemSubDivideCount;
                                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                                    CreateSubNodeAndContent(minorNode, minorContent, leafTitlePrefix, leafIndex, master, leafStudyItems);
                                    studyItemIndex += leafStudyItemCount;
                                    if (studyItemIndex >= totalStudyItemCount)
                                        goto Exit;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int minorIndex = 0; minorIndex < minorStudyListCount; minorIndex++)
                    {
                        BaseObjectContent minorContent = CreateSubNodeAndContent(
                            node, content, titlePrefix, minorIndex, master, null);
                        BaseObjectNode minorNode = minorContent.Node;
                        string leafTitlePrefix = titlePrefix + "-" + (minorIndex + 1).ToString();

                        for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                        {
                            leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                            if (leafStudyItemCount > itemSubDivideCount)
                                leafStudyItemCount = itemSubDivideCount;
                            leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                            CreateSubNodeAndContent(minorNode, minorContent, leafTitlePrefix, leafIndex, master, leafStudyItems);
                            studyItemIndex += leafStudyItemCount;
                            if (studyItemIndex >= totalStudyItemCount)
                                goto Exit;
                        }
                    }
                }
            }
            else
            {
                for (int leafIndex = 0; leafIndex < totalLeafStudyListCount; leafIndex++)
                {
                    leafStudyItemCount = totalStudyItemCount - studyItemIndex;
                    if (leafStudyItemCount > itemSubDivideCount)
                        leafStudyItemCount = itemSubDivideCount;
                    leafStudyItems = studyList.GetStudyItemRange(studyItemIndex, leafStudyItemCount);
                    CreateSubNodeAndContent(node, content, titlePrefix, leafIndex, master, leafStudyItems);
                    studyItemIndex += leafStudyItemCount;
                    if (studyItemIndex >= totalStudyItemCount)
                        goto Exit;
                }
            }

        Exit:
            studyList.DeleteAllStudyItems();
            UpdateContentHelper(content);
        }

#if fase
        public void SubDivideStudyListIntoNodeChildren(ContentStudyList studyList, string titlePrefix, NodeMaster master,
            int itemSubDivideCount, int minorSubDivideCount, int majorSubDivideCount)
        {
            BaseObjectContent content = studyList.Content;
            BaseObjectNode node = content.Node;
            int totalStudyItemCount = studyList.StudyItemCount();
            int totalLeafStudyListCount = (totalStudyItemCount + (itemSubDivideCount - 1)) / itemSubDivideCount;
            int studyListIndex = 0;
            int leafStudyItemStartIndex;
            int leafStudyItemCount;
            List<MultiLanguageItem> leafStudyItems;
            int totalLessonCount = totalLeafStudyListCount;
            int totalMinorGroupCount = (totalLessonCount + (minorSubDivideCount - 1)) / minorSubDivideCount;
            int totalMajorGroupCount = (totalMinorGroupCount + (majorSubDivideCount - 1)) / majorSubDivideCount;
            bool needDeleteAll = false;

            if (totalMajorGroupCount > 1)
            {
                needDeleteAll = true;

                for (int majorIndex = 0; majorIndex < totalMajorGroupCount; majorIndex++)
                {
                    BaseObjectContent majorContent = CreateSubNodeAndContent(
                        node, content, titlePrefix, majorIndex, master, null);
                    BaseObjectNode majorNode = majorContent.Node;
                    string minorTitlePrefix = titlePrefix + "-" + (majorIndex + 1).ToString();

                    for (int minorIndex = 0; minorIndex < totalMinorGroupCount; minorIndex++)
                    {
                        BaseObjectContent minorContent = CreateSubNodeAndContent(
                            majorNode, majorContent, minorTitlePrefix, minorIndex, master, null);
                        BaseObjectNode minorNode = minorContent.Node;
                        string leafTitlePrefix = minorTitlePrefix + "-" + (minorIndex + 1).ToString();

                        for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                        {
                            int leafContentIndex = (leafIndex * minorSubDivideCount) + leafIndex;
                            if (leafContentIndex >= totalLeafStudyListCount)
                                break;
                            leafStudyItemStartIndex = (studyListIndex * itemSubDivideCount);
                            leafStudyItemCount = totalStudyItemCount - leafStudyItemStartIndex;
                            if (leafStudyItemCount > itemSubDivideCount)
                                leafStudyItemCount = itemSubDivideCount;
                            leafStudyItems = studyList.GetStudyItemRange(leafStudyItemStartIndex, leafStudyItemCount);
                            CreateSubNodeAndContent(minorNode, minorContent, leafTitlePrefix, leafIndex, master, leafStudyItems);
                            studyListIndex++;
                        }
                    }
                }
            }
            else if (totalMinorGroupCount > 1)
            {
                needDeleteAll = true;

                for (int minorIndex = 0; minorIndex < totalMinorGroupCount; minorIndex++)
                {
                    BaseObjectContent minorContent = CreateSubNodeAndContent(
                        node, content, titlePrefix, minorIndex, master, null);
                    BaseObjectNode minorNode = minorContent.Node;
                    string leafTitlePrefix = titlePrefix + "-" + (minorIndex + 1).ToString();

                    for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                    {
                        int leafContentIndex = (minorIndex * minorSubDivideCount) + leafIndex;
                        if (leafContentIndex >= totalLeafStudyListCount)
                            break;
                        leafStudyItemStartIndex = (studyListIndex * itemSubDivideCount);
                        leafStudyItemCount = totalStudyItemCount - leafStudyItemStartIndex;
                        if (leafStudyItemCount > itemSubDivideCount)
                            leafStudyItemCount = itemSubDivideCount;
                        leafStudyItems = studyList.GetStudyItemRange(leafStudyItemStartIndex, leafStudyItemCount);
                        CreateSubNodeAndContent(minorNode, minorContent, leafTitlePrefix, leafIndex, master, leafStudyItems);
                        studyListIndex++;
                    }
                }
            }
            else if (totalLessonCount > 1)
            {
                needDeleteAll = true;

                for (int leafIndex = 0; leafIndex < minorSubDivideCount; leafIndex++)
                {
                    leafStudyItemStartIndex = (studyListIndex * itemSubDivideCount);
                    if (leafStudyItemStartIndex >= totalStudyItemCount)
                        break;
                    leafStudyItemCount = totalStudyItemCount - leafStudyItemStartIndex;
                    if (leafStudyItemCount > itemSubDivideCount)
                        leafStudyItemCount = itemSubDivideCount;
                    leafStudyItems = studyList.GetStudyItemRange(leafStudyItemStartIndex, leafStudyItemCount);
                    CreateSubNodeAndContent(node, content, titlePrefix, leafIndex, master, leafStudyItems);
                    studyListIndex++;
                }
            }

            if (needDeleteAll)
            {
                studyList.DeleteAllStudyItems();
                UpdateContentHelper(content);
            }
        }
#endif

        public BaseObjectContent CreateSubNodeAndContent(BaseObjectNode parentNode, BaseObjectContent parentContent,
            string titlePrefix, int index, NodeMaster master, List<MultiLanguageItem> childStudyItems)
        {
            string nodeTitleString = titlePrefix + "-" + (index + 1).ToString();
            MultiLanguageString nodeTitle = new MultiLanguageString("Title", UILanguageID, nodeTitleString);
            MultiLanguageString nodeDescription = null;
            BaseObjectNodeTree tree = parentNode.LocalTree;
            BaseObjectNode realParentNode = (parentNode is BaseObjectNodeTree ? null : parentNode);

            BaseObjectNode node = new BaseObjectNode(
                0,                                              // object Key
                nodeTitle,                                      // MultiLanguageString title
                nodeDescription,                                // MultiLanguageString description
                String.Empty,                                   // string source
                null,                                           // string package
                "Lesson",                                       // string label
                null,                                           // string imageFileName
                index,                                          // int index
                parentNode.IsPublic,                            // bool isPublic
                parentNode.CloneTargetLanguageIDs(),            // List<LanguageID> targetLanguageIDs,
                parentNode.CloneHostLanguageIDs(),              // List<LanguageID> hostLanguageIDs,
                parentNode.Owner,                               // string owner
                tree,                                           // BaseObjectNodeTree tree
                realParentNode,                                 // BaseObjectNode parent
                null,                                           // List<BaseObjectNode> children
                parentNode.CloneOptions(),                      // List<IBaseObjectKeyed> options
                parentNode.CloneMarkupTemplate(),               // MarkupTemplate markupTemplate,
                parentNode.CloneMarkupReference(),              // MarkupTemplateReference markupReference,
                null,                                           // List<BaseObjectContent> contentList
                null,                                           // List<BaseObjectContent> contentChildren
                master);                                        // NodeMaster master
            node.EnsureGuid();

            if (master != null)
                SetupNodeFromMaster(node);

            if (parentNode.Label == "Lesson")
                parentNode.Label = "Group";

            parentNode.AddChildNode(node);

            string contentKey = parentContent.KeyString;
            ContentStudyList childStudyList = null;
            BaseObjectContent childContent = node.GetContent(contentKey);
            MultiLanguageString contentTitle = parentContent.CloneTitle();
            MultiLanguageString contentDescription = parentContent.CloneDescription();

            if (childContent == null)
            {
                childContent = new BaseObjectContent(
                    contentKey,                                     // string key,
                    contentTitle,                                   // MultiLanguageString title,
                    contentDescription,                             // MultiLanguageString description,
                    parentContent.Source,                           // string source,
                    parentContent.Package,                          // string package,
                    parentContent.Label,                            // string label,
                    parentContent.ImageFileName,                    // string imageFileName,
                    index,                                          // int index,
                    parentContent.IsPublic,                         // bool isPublic,
                    parentContent.CloneTargetLanguageIDs(),         // List<LanguageID> targetLanguageIDs,
                    parentContent.CloneHostLanguageIDs(),           // List<LanguageID> hostLanguageIDs,
                    parentContent.Owner,                            // string owner,
                    node,                                           // BaseObjectNode node,
                    parentContent.ContentType,                      // string contentType,
                    parentContent.ContentSubType,                   // string contentSubType,
                    childStudyList,                                 // BaseContentStorage contentStorage,
                    parentContent.CloneOptions(),                   // List<IBaseObjectKeyed> options,
                    parentContent.CloneMarkupTemplate(),            // MarkupTemplate markupTemplate,
                    parentContent.CloneMarkupReference(),           // MarkupTemplateReference markupReference,
                    null,                                           // BaseObjectContent contentParent,
                    null);                                          // List<BaseObjectContent> contentChildren
                childContent.EnsureGuid();
                node.AddContentChild(childContent);
            }
            else
            {
                childContent.Title = contentTitle;
                childContent.Description = contentDescription;
            }

            if (childStudyItems != null)
            {
                childStudyList = childContent.ContentStorageStudyList;

                if (childStudyList == null)
                {
                    childStudyList = new ContentStudyList(0, null, null, null);
                    childContent.ContentStorage = childStudyList;
                    AddContent(childContent);
                    CopyAndMoveStudyItems(childStudyList, childStudyItems);
                }
                else
                {
                    childStudyList.DeleteAllStudyItems();
                    CopyAndMoveStudyItems(childStudyList, childStudyItems);
                    UpdateContentHelper(childContent);
                }
            }
            else if (childContent.ContentStorageStudyList == null)
            {
                childContent.SetupContentStorage();
                AddContent(childContent);
            }

            return childContent;
        }

        public BaseObjectContent CreateSubStudyListContent(BaseObjectContent parentContent, int index,
            List<MultiLanguageItem> childStudyItems)
        {
            string key = parentContent.KeyString
                + "_" + (index + 1).ToString();
            string titleString = parentContent.GetTitleString(UILanguageID)
                + "-" + (index + 1).ToString();
            MultiLanguageString title = new MultiLanguageString("Title", UILanguageID, titleString);
            ContentStudyList childStudyList = null;
            if (childStudyItems != null)
                childStudyList = new ContentStudyList(0, null, null, null);
            BaseObjectContent childContent = new BaseObjectContent(
                key,                                            // string key,
                title,                                          // MultiLanguageString title,
                parentContent.CloneDescription(),               // MultiLanguageString description,
                parentContent.Source,                           // string source,
                parentContent.Package,                          // string package,
                parentContent.Label,                            // string label,
                parentContent.ImageFileName,                    // string imageFileName,
                index,                                          // int index,
                parentContent.IsPublic,                         // bool isPublic,
                parentContent.CloneTargetLanguageIDs(),         // List<LanguageID> targetLanguageIDs,
                parentContent.CloneHostLanguageIDs(),           // List<LanguageID> hostLanguageIDs,
                parentContent.Owner,                            // string owner,
                parentContent.Node,                             // BaseObjectNode node,
                parentContent.ContentType,                      // string contentType,
                parentContent.ContentSubType,                   // string contentSubType,
                childStudyList,                                 // BaseContentStorage contentStorage,
                parentContent.CloneOptions(),                   // List<IBaseObjectKeyed> options,
                parentContent.CloneMarkupTemplate(),            // MarkupTemplate markupTemplate,
                parentContent.CloneMarkupReference(),           // MarkupTemplateReference markupReference,
                parentContent,                                  // BaseObjectContent contentParent,
                null);                                          // List<BaseObjectContent> contentChildren
            childContent.EnsureGuid();
            if (childStudyItems == null)
                childContent.SetupContentStorage();
            childContent.SetupDirectoryCheck();
            parentContent.AddContentChild(childContent);
            ContentStudyList newStudyList = childContent.ContentStorageStudyList;
            newStudyList.Content = childContent;
            List<MultiLanguageItem> newStudyItems = CopyAndMoveStudyItems(newStudyList, childStudyItems);
            AddContent(childContent);
            return childContent;
        }

        public List<MultiLanguageItem> CopyAndMoveStudyItems(
            ContentStudyList newStudyList,
            List<MultiLanguageItem> oldStudyItems)
        {
            List<MultiLanguageItem> newStudyItems = new List<MultiLanguageItem>();

            if (oldStudyItems != null)
            {
                foreach (MultiLanguageItem oldStudyItem in oldStudyItems)
                {
                    MultiLanguageItem newStudyItem = new MultiLanguageItem(oldStudyItem);
                    newStudyItem.MediaTildeUrl = null;
                    newStudyItem.StudyList = newStudyList;
                    newStudyItems.Add(newStudyItem);
                    newStudyItem.Modified = false;
                }

                int firstIndex = newStudyList.StudyItemCount();
                newStudyList.AddStudyItems(newStudyItems);
                newStudyList.RekeyStudyItems(firstIndex);

                MoveStudyItemsMedia(oldStudyItems, newStudyItems);
            }

            return newStudyItems;
        }

        public bool MoveStudyItemsMedia(
            List<MultiLanguageItem> oldStudyItems,
            List<MultiLanguageItem> newStudyItems)
        {
            int index;
            int count = oldStudyItems.Count();
            bool returnValue = true;

            for (index = 0; index < count; index++)
            {
                MultiLanguageItem oldStudyItem = oldStudyItems[index];
                MultiLanguageItem newStudyItem = newStudyItems[index];

                if (!MoveStudyItemMedia(oldStudyItem, newStudyItem))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool MoveStudyItemMedia(
            MultiLanguageItem oldStudyItem,
            MultiLanguageItem newStudyItem)
        {
            string oldMediaTildeUrl = oldStudyItem.MediaTildeUrl;
            string newMediaTildeUrl = newStudyItem.MediaTildeUrl;
            int index;
            int count = oldStudyItem.Count();
            bool returnValue = true;

            for (index = 0; index < count; index++)
            {
                LanguageItem oldLanguageItem = oldStudyItem.LanguageItem(index);

                if (oldLanguageItem == null)
                    throw new Exception("MoveStudyItemMedia: no old language ltem.");

                LanguageID languageID = oldLanguageItem.LanguageID;
                LanguageItem newLanguageItem = newStudyItem.LanguageItem(languageID);

                if (newLanguageItem == null)
                    throw new Exception("MoveStudyItemMedia: no new language ltem.");

                if (!MoveLanguageItemMedia(
                        oldMediaTildeUrl,
                        oldLanguageItem,
                        newMediaTildeUrl,
                        newLanguageItem))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool MoveLanguageItemMedia(
            string oldMediaTildeUrl,
            LanguageItem oldLanguageItem,
            string newMediaTildeUrl,
            LanguageItem newLanguageItem)
        {
            bool returnValue = true;

            if (newLanguageItem.HasSentenceRuns())
            {
                foreach (TextRun textRun in newLanguageItem.SentenceRuns)
                {
                    if (!MoveTextRunMedia(oldMediaTildeUrl, newMediaTildeUrl, textRun))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool MoveTextRunMedia(
            string oldMediaTildeUrl,
            string newMediaTildeUrl,
            TextRun textRun)
        {
            bool returnValue = true;

            if (textRun.MediaRunCount() != 0)
            {
                foreach (MediaRun mediaRun in textRun.MediaRuns)
                {
                    if (!mediaRun.IsReference && !mediaRun.IsFullUrl)
                    {
                        string oldMediaFilePath = mediaRun.GetDirectoryPath(oldMediaTildeUrl);
                        string newMediaFilePath = mediaRun.GetDirectoryPath(newMediaTildeUrl);

                        if (String.IsNullOrEmpty(oldMediaFilePath))
                            throw new Exception("Old media file path is empty.");

                        if (String.IsNullOrEmpty(newMediaFilePath))
                            throw new Exception("New media file path is empty.");

                        if (newMediaFilePath != oldMediaFilePath)
                        {
                            if (mediaRun.IsRelativeUrl)
                            {
                                string relativePathToSource = MediaUtilities.MakeRelativeUrl(newMediaFilePath, oldMediaFilePath);
                                mediaRun.FileName = relativePathToSource;
                            }
                            else
                            {
                                try
                                {
                                    if (FileSingleton.Exists(oldMediaFilePath))
                                    {
                                        FileSingleton.DirectoryExistsCheck(newMediaFilePath);

                                        if (FileSingleton.Exists(newMediaFilePath))
                                            FileSingleton.Delete(newMediaFilePath);

                                        if (mediaRun.IsRelativeUrl)
                                            FileSingleton.Copy(oldMediaFilePath, newMediaFilePath);
                                        else
                                            FileSingleton.Move(oldMediaFilePath, newMediaFilePath);
                                    }
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError("Error moving media file", exc);
                                }
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool LinkWordsAndExpansion(BaseObjectNode node, bool recurse, ref string errorMessage)
        {
            List<BaseObjectContent> wordContents = node.GetContentListWithTypeAndSubType("Words", "Vocabulary");
            List<BaseObjectContent> expansionContents = node.GetContentListWithTypeAndSubType("Expansion", "Vocabulary");
            bool returnValue = true;

            foreach (BaseObjectContent wordContent in wordContents)
            {
                foreach (BaseObjectContent expansionContent in expansionContents)
                {
                    if (wordContent.ContentStorage != null)
                        wordContent.ResolveReferences(Repositories, false, false);

                    if (expansionContent.ContentStorage != null)
                        expansionContent.ResolveReferences(Repositories, false, false);

                    if (!LinkWordsAndExpansion(node, wordContent, expansionContent, ref errorMessage))
                        returnValue = false;
                }
            }

            if (recurse && node.HasChildren())
            {
                foreach (BaseObjectNode childNode in node.Children)
                {
                    if (!LinkWordsAndExpansion(childNode, true, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool LinkWordsAndExpansion(BaseObjectNode node, BaseObjectContent wordContent,
            BaseObjectContent expansionContent, ref string errorMessage)
        {
            ContentStudyList wordStudyList = wordContent.ContentStorageStudyList;
            ContentStudyList expansionStudyList = expansionContent.ContentStorageStudyList;
            LanguageID targetLanguageID = node.FirstTargetLanguageID;
            bool returnValue = true;

            if ((wordStudyList.StudyItemCount() != 0) && (expansionStudyList.StudyItemCount() != 0))
            {
                MultiLanguageItemReference currentExpansionToWordItemReference = null;
                MultiLanguageItem currentWordStudyItem = null;
                string expansionMediaTildeUrl = expansionContent.MediaTildeUrl;
                object wordContentStorageKey = wordStudyList.Key;
                string wordContentKey = wordContent.KeyString;
                object expansionContentStorageKey = expansionStudyList.Key;
                string expansionContentKey = expansionContent.KeyString;
                object nodeKey = wordStudyList.Node.Key;
                int expansionCount = expansionStudyList.StudyItemCount();
                int expansionIndex;

                foreach (MultiLanguageItem wordItem in wordStudyList.StudyItems)
                    wordItem.DeleteExpansionReferences();

                for (expansionIndex = 0; expansionIndex < expansionCount;)
                {
                    MultiLanguageItem expansionStudyItem = expansionStudyList.GetStudyItemIndexed(expansionIndex);
                    string expansionTargetText = expansionStudyItem.Text(targetLanguageID);
                    MultiLanguageItem wordStudyItem = wordStudyList.FindStudyItem(expansionTargetText, targetLanguageID);

                    if (wordStudyItem != null)
                    {
                        currentWordStudyItem = wordStudyItem;
                        currentExpansionToWordItemReference = new MultiLanguageItemReference(
                            wordStudyItem.Key, wordContentStorageKey, wordContentKey, nodeKey, wordStudyItem);

                        ContentUtilities.DeleteStudyItemMediaRunsAndMedia(expansionStudyItem, expansionMediaTildeUrl, true);
                        expansionStudyList.DeleteStudyItemIndexed(expansionIndex);
                        expansionCount--;
                    }
                    else
                    {
                        if (currentExpansionToWordItemReference != null)
                        {
                            MultiLanguageItemReference wordToExpansionReference = new MultiLanguageItemReference(
                                expansionStudyItem.Key, expansionContentStorageKey, expansionContentKey,
                                nodeKey, expansionStudyItem);
                            currentWordStudyItem.AddExpansionReference(wordToExpansionReference);
                            expansionStudyItem.AddExpansionReference(currentExpansionToWordItemReference);

                            if (currentWordStudyItem.ExpansionReferenceCount() == 1)
                            {
                                if (!expansionStudyItem.HasAnnotation("Heading"))
                                {
                                    Annotation heading = currentWordStudyItem.GetHeadingAnnotation();
                                    expansionStudyItem.AddAnnotation(heading);
                                }
                            }
                        }

                        expansionIndex++;
                    }
                }
            }

            UpdateContentStorageCheck(wordContent, true);
            UpdateContentStorageCheck(expansionContent, true);

            return returnValue;
        }

        public NodeMaster FixupNodeMaster(BaseObjectNodeTree tree, BaseObjectNode node, bool update, IMainRepository repositories)
        {
            NodeMaster master = null;

            if (node.MasterReference != null)
            {
                master = node.Master;

                if (master == null)
                {
                    node.MasterReference.ResolveReference(repositories);
                    master = node.Master;
                }

                if (update && node.Modified)
                    UpdateTree(tree, false, false);
            }

            if (master != null)
                master.ResolveReferences(repositories, false, true);

            return master;
        }

        public bool HandleCopyMarkup(BaseMarkupContainer markupContainer, int copyMarkupTemplateKey)
        {
            MarkupTemplate markupTemplate = markupContainer.LocalMarkupTemplate;
            MarkupTemplateReference markupReference = markupContainer.MarkupReference;

            if (copyMarkupTemplateKey <= 0)
            {
                if (markupTemplate == null)
                {
                    if (markupReference != null)
                    {
                        if (markupReference.KeyString == "(local)")
                            markupContainer.LocalMarkupTemplate = new Markup.MarkupTemplate("(local)");
                    }
                }
                return true;
            }

            if ((markupTemplate != null) && !markupTemplate.IsLocal)
                return true;

            MarkupTemplate copyMarkupTemplate = Repositories.MarkupTemplates.Get(copyMarkupTemplateKey);

            if (copyMarkupTemplate != null)
            {
                copyMarkupTemplate.ResolveReferences(Repositories, false, true);

                if (markupTemplate != null)
                    markupTemplate.CopyFrom(copyMarkupTemplate);
                else
                    markupTemplate = new MarkupTemplate(copyMarkupTemplate);

                markupTemplate.Key = "(Local)";

                markupContainer.LocalMarkupTemplate = markupTemplate;
                markupContainer.MarkupReference = new MarkupTemplateReference(markupTemplate);

                return true;
            }
            else
                Error = S("Markup to be copied not found") + ": " + copyMarkupTemplateKey;

            return false;
        }

        public ContentStatistics GetOrCreateContentStatisticsFromTreeReference(
            ObjectReferenceNodeTree treeReference,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now)
        {
            if (treeReference == null)
                return null;

            lock (treeReference)
            {
                string key = ContentStatistics.ComposeKey(treeReference, UserRecord, UserProfile);

                ContentStatistics cs = Repositories.ContentStatistics.Get(key);

                if (cs == null)
                {
                    cs = new Content.ContentStatistics(
                        treeReference,
                        toolConfigurationKey,
                        toolSource,
                        UserRecord,
                        UserProfile);
                }
                else if (cs.IsNeedsCheckLocal("Any", now, true))
                {
                    BaseObjectNodeTree tree = Repositories.ResolveReference(
                        treeReference.Source, null, treeReference.Key) as BaseObjectNodeTree;

                    if (tree != null)
                    {
                        ContentStatisticsCache csCache = new Content.ContentStatisticsCache(UserRecord, UserProfile);

                        tree.ResolveReferences(Repositories, false, true);

                        cs.PropagateToolSourceAndStatus(toolConfigurationKey, toolSource);

                        csCache.AddHierarchy(cs);

                        RefreshContentStatisticsNodeRecurse(
                            tree,
                            now,
                            cs,
                            csCache);
                        cs.SetStatusFromCountsGlobal("Any");
                        Repositories.ContentStatistics.Update(cs);
                    }
                }

                return cs;
            }
        }

        public ContentStatistics GetOrCreateContentStatisticsFromTree(
            BaseObjectNodeTree tree)
        {
            if (tree == null)
                return null;

            string toolConfigurationKey = UserProfile.GetUserOptionString(
                "LastToolConfigurationKey",
                "Read0");
            ToolSourceCode toolSource = ToolUtilities.GetToolSourceCodeFromString(
                UserProfile.GetUserOptionString("LastToolSource", "StudyList"));
            DateTime now = DateTime.UtcNow;

            ContentStatistics cs = GetOrCreateContentStatisticsFromTree(
                tree, toolConfigurationKey, toolSource, now);

            return cs;
        }

        public ContentStatistics GetOrCreateContentStatisticsFromTree(
            BaseObjectNodeTree tree,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now)
        {
            if (tree == null)
                return null;

            lock (tree)
            {
                string key = ContentStatistics.ComposeKey(tree, UserRecord, UserProfile);

                ContentStatistics cs = Repositories.ContentStatistics.Get(key);

                if (cs == null)
                {
                    cs = CreateContentStatisticsFromTree(tree, toolConfigurationKey, toolSource, now);

                    if (cs != null)
                    {
                        if (!Repositories.ContentStatistics.Add(cs))
                        {
                            PutErrorArgument("Error adding content statistics for tree", tree.GetTitleString(UILanguageID));
                            return null;
                        }
                    }
                }
                else if (cs.PropagateToolSourceAndStatus(toolConfigurationKey, toolSource))
                {
                    ContentStatisticsCache csCache = new Content.ContentStatisticsCache(UserRecord, UserProfile);

                    tree.ResolveReferences(Repositories, false, true);

                    csCache.AddHierarchy(cs);

                    RefreshContentStatisticsNodeRecurse(
                        tree,
                        now,
                        cs,
                        csCache);
                    cs.SetStatusFromCountsGlobal("Any");

                    if (!Repositories.ContentStatistics.Update(cs))
                    {
                        PutErrorArgument("Error reseting content statistics for tree", tree.GetTitleString(UILanguageID));
                        return null;
                    }
                }

                return cs;
            }
        }

        public ContentStatistics RecreateContentStatisticsFromTree(
            BaseObjectNodeTree tree,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now)
        {
            if (tree == null)
                return null;

            lock (tree)
            {
                string key = ContentStatistics.ComposeKey(tree, UserRecord, UserProfile);

                ContentStatistics cs = CreateContentStatisticsFromTree(tree, toolConfigurationKey, toolSource, now);

                if (cs != null)
                {
                    if (!Repositories.ContentStatistics.Update(cs))
                    {
                        PutErrorArgument("Error recreating content statistics for tree", tree.GetTitleString(UILanguageID));
                        return null;
                    }
                }

                return cs;
            }
        }

        public ContentStatistics CreateContentStatisticsFromTree(
            BaseObjectNodeTree tree,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now)
        {
            if (tree == null)
                return null;

            ContentStatistics cs = CreateContentStatisticsFromNode(
                tree, toolConfigurationKey, toolSource, now, null);
            return cs;
        }


        public ContentStatistics CreateContentStatisticsFromNode(
            BaseObjectNode node,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now,
            ContentStatistics parentCS)
        {
            if (node == null)
                return null;

            ContentStatistics cs = new ContentStatistics(
                node, toolConfigurationKey, toolSource, UserRecord, UserProfile);

            cs.HideStatisticsFromParent = node.GetOptionFlag("HideStatisticsFromParent", false);

            if (node.GetOptionFlag("DisableStatistics", false))
                cs.Status = ContentStatisticsStatusCode.Disabled;

            if (node.HasChildren())
            {
                foreach (BaseObjectNode child in node.Children)
                    CreateContentStatisticsFromNode(
                        child, toolConfigurationKey, toolSource, now, cs);
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentChildren)
                    CreateContentStatisticsFromContent(
                        content, toolConfigurationKey, toolSource, now, cs);
            }

            if (parentCS != null)
                parentCS.AddChild(cs);

            return cs;
        }

        public ContentStatistics CreateContentStatisticsFromContent(
            BaseObjectContent content,
            string toolConfigurationKey,
            ToolSourceCode toolSource,
            DateTime now,
            ContentStatistics parentCS)
        {
            if ((content == null) || !content.HasContentStorageKey)
                return null;

            content.ResolveReferences(Repositories, false, false);

            if (content.ContentStorage == null)
                return null;

            ContentStatistics cs = new ContentStatistics(
                content, toolConfigurationKey, toolSource, UserRecord, UserProfile);

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent contentChild in content.ContentChildren)
                {
                    if (contentChild.KeyString == content.KeyString)
                        continue;

                    CreateContentStatisticsFromContent(
                        contentChild, toolConfigurationKey, toolSource, now, cs);
                }
            }

            cs.HideStatisticsFromParent = content.GetOptionFlag("HideStatisticsFromParent", false);

            if (content.GetOptionFlag("DisableStatistics", false))
                cs.Status = ContentStatisticsStatusCode.Disabled;

            switch (content.ContentClass)
            {
                case ContentClassType.StudyList:
                    InitializeContentStatisticsFromStudyList(content, now, cs);
                    break;
                case ContentClassType.MediaItem:
                    InitializeContentStatisticsFromMediaItem(content, now, cs);
                    break;
                case ContentClassType.DocumentItem:
                    InitializeContentStatisticsFromDocumentItem(content, now, cs);
                    break;
                default:
                    break;
            }

            if (parentCS != null)
                parentCS.AddChild(cs);

            return cs;
        }

        public bool InitializeContentStatistics(
            IBaseObjectKeyed sourceObject,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = false;

            if (sourceObject == null)
                return false;

            if (sourceObject is BaseObjectContent)
                returnValue = InitializeContentStatisticsFromContent(sourceObject as BaseObjectContent, now, cs);
            else if (sourceObject is BaseObjectNode)
                returnValue = InitializeContentStatisticsFromNode(sourceObject as BaseObjectNode, now, cs);

            return returnValue;
        }

        public bool InitializeContentStatisticsFromNode(
            BaseObjectNode node,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = true;

            if (node == null)
                return false;

            if (node.HasChildren())
            {
                foreach (BaseObjectNode child in node.Children)
                    InitializeContentStatisticsFromNode(
                        child, now, cs);
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent content in node.ContentChildren)
                    InitializeContentStatisticsFromContent(
                        content, now, cs);
            }

            return returnValue;
        }

        public bool InitializeContentStatisticsFromContent(
            BaseObjectContent content,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = true;

            switch (content.ContentClass)
            {
                case ContentClassType.StudyList:
                    returnValue = InitializeContentStatisticsFromStudyList(content, now, cs);
                    break;
                case ContentClassType.MediaItem:
                    returnValue = InitializeContentStatisticsFromMediaItem(content, now, cs);
                    break;
                case ContentClassType.DocumentItem:
                    returnValue = InitializeContentStatisticsFromDocumentItem(content, now, cs);
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public bool InitializeContentStatisticsFromStudyList(
            BaseObjectContent content,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            if (BaseObjectContent.NonStudyListTextContentTypes.Contains(cs.ContentType))
            {
                cs.Status = ContentStatisticsStatusCode.Disabled;
                return true;
            }

            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                return false;

            string toolStudyListKey = ToolUtilities.ComposeToolStudyListKey(
                UserRecord, content, ToolSourceCode.StudyList);

            ToolStudyList toolStudyList = Repositories.ToolStudyLists.Get(toolStudyListKey);

            if (toolStudyList != null)
            {
                int newReadyForReview;
                int newThisSession;
                toolStudyList.CollectStatistics(
                    cs,
                    out newReadyForReview,
                    out newThisSession,
                    cs.ToolConfigurationKey,
                    now,
                    now,
                    false);
            }
            else if (BaseObjectContent.TypicalToolStudyListContentTypes.Contains(cs.ContentType))
                cs.FutureCountLocal = studyList.StudyItemCount();

            return returnValue;
        }

        public bool InitializeContentStatisticsFromMediaItem(
            BaseObjectContent content,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            ContentMediaItem mediaItem = content.ContentStorageMediaItem;

            if (mediaItem == null)
                return false;

            List<ContentStudyList> studyLists = mediaItem.SourceStudyLists;

            if (studyLists != null)
            {
                int count = 0;

                foreach (ContentStudyList studyList in studyLists)
                    count += studyList.StudyItemCountRecurse();

                switch (cs.Status)
                {
                    case ContentStatisticsStatusCode.Future:
                        cs.FutureCountLocal = count;
                        break;
                    case ContentStatisticsStatusCode.Due:
                        cs.DueCountLocal = count;
                        break;
                    case ContentStatisticsStatusCode.Active:
                        cs.ActiveCountLocal = count;
                        break;
                    case ContentStatisticsStatusCode.Complete:
                        cs.CompleteCountLocal = count;
                        break;
                    case ContentStatisticsStatusCode.Disabled:
                        cs.FutureCountLocal = count;
                        break;
                    default:
                        break;
                }
            }

            return returnValue;
        }

        public bool InitializeContentStatisticsFromDocumentItem(
            BaseObjectContent content,
            DateTime now,
            ContentStatistics cs)
        {
            bool returnValue = true;

            if (content == null)
                return false;

            cs.Status = ContentStatisticsStatusCode.Disabled;

            return returnValue;
        }

        public virtual void RefreshContentStatisticsNodeRecurse(
            BaseObjectNode node,
            DateTime now,
            ContentStatistics cs,
            ContentStatisticsCache csCache)
        {
            if (node.HasChildren())
            {
                foreach (BaseObjectNode childNode in node.Children)
                {
                    ContentStatistics childCS = csCache.Get(childNode);

                    if (childCS == null)
                        continue;

                    RefreshContentStatisticsNodeRecurse(childNode, now, childCS, csCache);
                }
            }

            if (node.HasContent())
            {
                foreach (BaseObjectContent childContent in node.ContentChildren)
                {
                    ContentStatistics childCS = csCache.Get(childContent);

                    if (childCS == null)
                        continue;

                    RefreshContentStatisticsContentRecurse(childContent, now, childCS);
                }
            }
        }

        public virtual void RefreshContentStatisticsContentRecurse(
            BaseObjectContent content,
            DateTime now,
            ContentStatistics cs)
        {
            if (cs.ContentClass == "StudyList")
            {
                if (cs.IsNeedsCheckLocal("Any", now, true))
                    InitializeContentStatisticsFromStudyList(
                        content,
                        now,
                        cs);
            }
            else if (cs.ContentClass == "MediaItem")
            {
                if (cs.IsNeedsCheckLocal("Any", now, true))
                    InitializeContentStatisticsFromMediaItem(
                        content,
                        now,
                        cs);
            }
            else if (cs.ContentClass == "DocumentItem")
                InitializeContentStatisticsFromDocumentItem(content, now, cs);
        }
    }
}

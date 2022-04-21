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
        // Assumes sources loaded in Targets.
        public virtual bool TransferTargets(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string storeName,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            NodeUtilities sourceNodeUtilities,
            NodeUtilities targetNodeUtilities,
            bool isAll,
            bool deleteBeforeTransfer,
            List<IBaseObjectKeyed> targets)
        {
            if ((targets == null) || (targets.Count() == 0))
            {
                PutError("Nothing to transfer. Targets empty.");
                return false;
            }

            IObjectStore targetObjectStore = targetRepository.FindObjectStore(
                storeName,
                targetLanguageID,
                hostLanguageID);

            if (targetObjectStore == null)
            {
                PutError("Unknown target object store name, or object store never created: " + storeName);
                return false;
            }

            if (deleteBeforeTransfer)
            {
                if (!DeleteCheck(
                        sourceRepository,
                        targetRepository,
                        targetObjectStore,
                        targetNodeUtilities,
                        targetLanguageID,
                        hostLanguageID,
                        storeName,
                        isAll,
                        targets))
                    return false;
            }

            bool returnValue = true;

            switch (storeName)
            {
                case "Courses":
                case "Plans":
                    // These need to be done individually.
                    foreach (IBaseObject sourceObject in targets)
                    {
                        if (!TransferOne(
                                sourceRepository,
                                targetRepository,
                                targetObjectStore,
                                targetNodeUtilities,
                                storeName,
                                sourceObject as IBaseObjectKeyed))
                            returnValue = false;
                    }
                    break;
                case "MarkupTemplates":
                case "NodeMasters":
                case "Dictionary":
                    // These can be done as a list.
                    if (!TransferList(
                            sourceRepository,
                            targetRepository,
                            targetObjectStore,
                            targetNodeUtilities,
                            targets))
                        returnValue = false;
                    break;
                case "DictionaryMultiAudio":
                    returnValue = TransferAudio(
                        sourceRepository,
                        targetRepository,
                        targetObjectStore,
                        targetNodeUtilities,
                        targetLanguageID,
                        targets.Cast<AudioMultiReference>().ToList());
                    break;
                case "DictionaryPictures":
                    returnValue = TransferPictures(
                        sourceRepository,
                        targetRepository,
                        targetObjectStore,
                        targetNodeUtilities,
                        targetLanguageID,
                        targets.Cast<PictureReference>().ToList());
                    break;
                case "UserRecords":
                case "UIText":
                case "UIStrings":
                case "ToolProfiles":
                case "TranslationCache":
                    foreach (IBaseObjectKeyed sourceObject in targets)
                    {
                        // These need to be done individually.
                        if (!TransferOne(
                                sourceRepository,
                                targetRepository,
                                targetObjectStore,
                                targetNodeUtilities,
                                storeName,
                                sourceObject))
                            returnValue = false;
                    }
                    break;
                default:
                    throw new Exception("Unexpected store name: " + storeName);
            }

            return returnValue;
        }

        protected virtual bool TransferList(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            List<IBaseObjectKeyed> sourceObjects)
        {
            List<IBaseObjectKeyed> targetObjects = new List<IBaseObjectKeyed>();
            bool returnValue = true;

            UpdateProgressElapsed("Transfering objects...");

            foreach (IBaseObjectKeyed sourceObject in sourceObjects)
            {
                IBaseObjectKeyed targetObject = sourceObject.Clone() as IBaseObjectKeyed;

                if (targetObject.IsIntegerKeyType)
                    targetObject.ResetKeyNoModify();

                targetObjects.Add(targetObject);
            }

            returnValue = targetObjectStore.AddList(targetObjects);

            return returnValue;
        }

        protected virtual bool TransferOne(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            string storeName,
            IBaseObjectKeyed sourceObject)
        {
            bool returnValue = false;

            switch (storeName)
            {
                case "Courses":
                case "Plans":
                    returnValue = TransferTree(
                        sourceRepository,
                        targetRepository,
                        targetObjectStore,
                        targetNodeUtilities,
                        storeName,
                        sourceObject as ObjectReferenceNodeTree);
                    break;
                case "MarkupTemplates":
                case "NodeMasters":
                case "Dictionary":
                    returnValue = TransferSimpleRekeyableObject(
                        targetRepository,
                        targetObjectStore,
                        sourceObject);
                    break;
                case "DictionaryMultiAudio":
                case "DictionaryPictures":
                    throw new Exception("Unsupported transfer for object store: " + storeName);
                case "UserRecords":
                    returnValue = TransferUserRecord(
                        targetRepository,
                        targetObjectStore,
                        sourceObject as UserRecord);
                    break;
                case "UIText":
                case "UIStrings":
                case "ToolProfiles":
                    throw new Exception("Unsupported transfer for object store: " + storeName);
                default:
                    throw new Exception("Unexpected store name: " + storeName);
            }

            return returnValue;
        }

        protected virtual bool TransferSimpleRekeyableObject(
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            IBaseObjectKeyed sourceObject)
        {
            IBaseObjectKeyed targetObject = sourceObject.Clone() as IBaseObjectKeyed;

            UpdateProgressElapsed("Transfering object " + targetObject.Name + "...");

            if (targetObject == null)
            {
                PutError("Cloning of source object failed: " + sourceObject.Name);
                return false;
            }

            if (targetObject.IsIntegerKeyType)
                targetObject.ResetKeyNoModify();

            try
            {
                if (!targetObjectStore.Add(targetObject))
                {
                    PutError("Error adding cloned target object: " + targetObject.Name);
                    return false;
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception adding cloned target object: " + targetObject.Name, exc);
                return false;
            }

            return true;
        }

        protected virtual bool TransferUserRecord(
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            UserRecord sourceUserRecord)
        {
            UserRecord targetUserRecord = new UserRecord(sourceUserRecord);

            UpdateProgressElapsed("Transfering user record " + sourceUserRecord.UserName + "...");

            try
            {
                if (targetObjectStore.Contains(targetUserRecord.Key))
                {
                    if (!targetObjectStore.Update(targetUserRecord))
                    {
                        PutError("Error updating existing user record: " + targetUserRecord.Name);
                        return false;
                    }
                }
                else
                {
                    if (!targetObjectStore.Add(targetUserRecord))
                    {
                        PutError("Error adding cloned user record: " + targetUserRecord.Name);
                        return false;
                    }
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception adding user record: " + targetUserRecord.Name, exc);
                return false;
            }

            return true;
        }

        protected virtual bool TransferTree(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            string storeName,
            ObjectReferenceNodeTree sourceTreeHeader)
        {
            BaseObjectNodeTree sourceTree = sourceRepository.ResolveReference(storeName, null, sourceTreeHeader.Key) as BaseObjectNodeTree;
            bool returnValue = true;

            if (sourceTree == null)
            {
                PutError("Couldn't find tree: " + sourceTreeHeader.GetTitleString(UILanguageID));
                return false;
            }

            UpdateProgressElapsed("Transfering tree " + sourceTreeHeader.GetTitleString(UILanguageID) + "...");

            sourceTree.ResolveReferences(sourceRepository, false, true);

            string sourceTreeMediaPath = MediaUtilities.ConcatenateFilePath4(
                sourceRepository.ContentPath,
                "Media",
                sourceTree.Owner,
                sourceTree.Directory);
            string targetTreeMediaPath = MediaUtilities.ConcatenateFilePath4(
                targetRepository.ContentPath,
                "Media",
                sourceTree.Owner,
                sourceTree.Directory);

            Guid guid = sourceTree.Guid;
            MultiLanguageString title = sourceTree.CloneTitle();
            MultiLanguageString description = sourceTree.CloneDescription();
            List<LanguageID> targetLanguageIDs = sourceTree.CloneTargetLanguageIDs();
            List<LanguageID> hostLanguageIDs = sourceTree.CloneHostLanguageIDs();
            string source = sourceTree.Source;
            string package = sourceTree.Package;
            string label = sourceTree.Label;
            string imageFileName = sourceTree.ImageFileName;
            int index = 0;
            bool isPublic = sourceTree.IsPublic;
            string owner = sourceTree.Owner;
            List<IBaseObjectKeyed> options = sourceTree.CloneOptions();
            MarkupTemplate markupTemplate = GetTargetLocalMarkupTemplate(
                sourceRepository,
                targetRepository,
                sourceTree.LocalMarkupTemplate);
            MarkupTemplateReference markupReference = GetTargetMarkupTemplateReference(
                sourceRepository,
                targetRepository,
                sourceTree.MarkupReference);
            NodeMaster nodeMaster = GetTargetNodeMaster(
                sourceRepository,
                targetRepository,
                owner,
                sourceTree.Master);
            BaseObjectNodeTree targetTree = new BaseObjectNodeTree(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                package,                // string package
                label,                  // string label
                imageFileName,          // string imageFileName
                index,                  // int index
                isPublic,               // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs
                owner,                  // string owner
                null,                   // List<BaseObjectNode> children
                options,                // List<IBaseObjectKeyed> options
                markupTemplate,         // MarkupTemplate markupTemplate
                markupReference,        // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                nodeMaster,             // NodeMaster master
                null);                  // List<BaseObjectNode> nodes
            targetTree.Guid = guid;
            targetTree.Directory = sourceTree.Directory;

            if (!targetNodeUtilities.AddTree(targetTree, false))
            {
                PutError("Error adding target tree: " + targetTree.Name);
                return false;
            }

            try
            {
                if (!String.IsNullOrEmpty(imageFileName))
                {
                    if (!CopyMediaFile(sourceTreeMediaPath, targetTreeMediaPath, imageFileName))
                        return false;
                }

                if (!TransferNodeContent(
                        sourceRepository,
                        targetRepository,
                        sourceTreeMediaPath,
                        targetTreeMediaPath,
                        targetNodeUtilities,
                        targetTree,
                        targetTree,
                        sourceTree))
                    return false;

                if (sourceTree.Nodes != null)
                {
                    foreach (BaseObjectNode sourceNode in sourceTree.Nodes)
                    {
                        if (!TransferNode(
                                sourceRepository,
                                targetRepository,
                                targetNodeUtilities,
                                targetTree,
                                sourceTree,
                                sourceTreeMediaPath,
                                targetTreeMediaPath,
                                sourceNode))
                            return false;
                    }
                }
            }
            catch (Exception exc)
            {
                PutExceptionError(exc);
            }
            finally
            {
                if (!targetNodeUtilities.UpdateTree(targetTree, false, false))
                {
                    PutError("Error updating added tree: " + targetTree.GetTitleString(UILanguageID));
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected virtual bool TransferNode(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            NodeUtilities targetNodeUtilities,
            BaseObjectNodeTree targetTree,
            BaseObjectNodeTree sourceTree,
            string sourceTreeMediaPath,
            string targetTreeMediaPath,
            BaseObjectNode sourceNode)
        {
            string sourceNodeMediaPath = sourceNode.MediaDirectoryPath;
            string targetNodeMediaPath = sourceNodeMediaPath.Replace(sourceTreeMediaPath, targetTreeMediaPath);
            BaseObjectNode parentNode = null;

            if (sourceNode.HasParent())
                parentNode = targetTree.GetNodeWithTitle(sourceNode.Parent.GetTitleString(UILanguageID), UILanguageID);

            Guid guid = sourceNode.Guid;
            MultiLanguageString title = sourceNode.CloneTitle();
            MultiLanguageString description = sourceNode.CloneDescription();
            List<LanguageID> targetLanguageIDs = sourceNode.CloneTargetLanguageIDs();
            List<LanguageID> hostLanguageIDs = sourceNode.CloneHostLanguageIDs();
            string source = sourceNode.Source;
            string package = sourceNode.Package;
            string label = sourceNode.Label;
            string imageFileName = sourceNode.ImageFileName;
            int index = 0;
            bool isPublic = sourceNode.IsPublic;
            string owner = sourceNode.Owner;
            List<IBaseObjectKeyed> options = sourceNode.CloneOptions();
            MarkupTemplate markupTemplate = GetTargetLocalMarkupTemplate(
                sourceRepository,
                targetRepository,
                sourceNode.LocalMarkupTemplate);
            MarkupTemplateReference markupReference = GetTargetMarkupTemplateReference(
                sourceRepository,
                targetRepository,
                sourceNode.MarkupReference);
            NodeMaster nodeMaster = GetTargetNodeMaster(
                sourceRepository,
                targetRepository,
                owner,
                sourceNode.Master);
            BaseObjectNode targetNode = new BaseObjectNode(
                0,                      // object Key
                title,                  // MultiLanguageString title
                description,            // MultiLanguageString description
                source,                 // string source
                package,                // string package
                label,                  // string label
                imageFileName,          // string imageFileName
                index,                  // int index
                isPublic,               // bool isPublic
                targetLanguageIDs,      // List<LanguageID> targetLanguageIDs
                hostLanguageIDs,        // List<LanguageID> hostLanguageIDs
                owner,                  // string owner
                targetTree,             // BaseObjectNodeTree tree
                parentNode,             // BaseObjectNode parent
                null,                   // List<BaseObjectNode> children
                options,                // List<IBaseObjectKeyed> options
                markupTemplate,         // MarkupTemplate markupTemplate
                markupReference,        // MarkupTemplateReference markupReference
                null,                   // List<BaseObjectContent> contentList
                null,                   // List<BaseObjectContent> contentChildren
                nodeMaster);            // NodeMaster master
            targetNode.Guid = guid;
            targetNode.Directory = sourceNode.Directory;

            targetTree.AddNode(parentNode, targetNode);

            if (!String.IsNullOrEmpty(imageFileName))
            {
                if (!CopyMediaFile(sourceNodeMediaPath, targetNodeMediaPath, imageFileName))
                    return false;
            }

            if (!TransferNodeContent(
                    sourceRepository,
                    targetRepository,
                    sourceTreeMediaPath,
                    targetTreeMediaPath,
                    targetNodeUtilities,
                    targetTree,
                    targetNode,
                    sourceNode))
                return false;

            return true;
        }

        protected virtual bool TransferNodeContent(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string sourceTreeMediaPath,
            string targetTreeMediaPath,
            NodeUtilities targetNodeUtilities,
            BaseObjectNodeTree targetTree,
            BaseObjectNode targetNode,
            BaseObjectNode sourceNode)
        {
            if (!sourceNode.HasContent())
                return true;

            List<BaseObjectContent> sourceContentList = sourceNode.ContentChildren;

            foreach (BaseObjectContent sourceContent in sourceContentList)
            {
                if (!TransferContent(
                        sourceRepository,
                        targetRepository,
                        sourceTreeMediaPath,
                        targetTreeMediaPath,
                        targetNodeUtilities,
                        targetNode,
                        null,
                        sourceNode,
                        sourceContent))
                    return false;
            }

            return true;
        }

        protected virtual bool TransferContent(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string sourceTreeMediaPath,
            string targetTreeMediaPath,
            NodeUtilities targetNodeUtilities,
            BaseObjectNode targetNode,
            BaseObjectContent targetContentParent,
            BaseObjectNode sourceNode,
            BaseObjectContent sourceContent)
        {
            string contentKey = sourceContent.KeyString;
            Guid guid = sourceContent.Guid;
            MultiLanguageString title = sourceContent.CloneTitle();
            MultiLanguageString description = sourceContent.CloneDescription();
            List<LanguageID> targetLanguageIDs = sourceContent.CloneTargetLanguageIDs();
            List<LanguageID> hostLanguageIDs = sourceContent.CloneHostLanguageIDs();
            string source = sourceContent.Source;
            string package = sourceContent.Package;
            string label = sourceContent.Label;
            string imageFileName = sourceContent.ImageFileName;
            int index = sourceContent.Index;
            bool isPublic = sourceContent.IsPublic;
            string owner = sourceContent.Owner;
            string contentType = sourceContent.ContentType;
            string contentSubType = sourceContent.ContentSubType;
            BaseContentStorage targetContentStorage;

            if (!TransferContentStorage(
                    sourceRepository,
                    targetRepository,
                    sourceTreeMediaPath,
                    targetTreeMediaPath,
                    targetNodeUtilities,
                    sourceContent,
                    out targetContentStorage))
                return false;

            List<IBaseObjectKeyed> options = sourceContent.CloneOptions();
            MarkupTemplate markupTemplate = GetTargetLocalMarkupTemplate(
                sourceRepository,
                targetRepository,
                sourceContent.LocalMarkupTemplate);
            MarkupTemplateReference markupReference = GetTargetMarkupTemplateReference(
                sourceRepository,
                targetRepository,
                sourceContent.MarkupReference);
            string sourceContentMediaPath = sourceContent.MediaDirectoryPath;
            string targetContentMediaPath = sourceContentMediaPath.Replace(sourceTreeMediaPath, targetTreeMediaPath);
            BaseObjectContent targetContent = new BaseObjectContent(
                contentKey,                     // string key,
                title,                          // MultiLanguageString title,
                description,                    // MultiLanguageString description,
                source,                         // string source,
                package,                        // string package,
                label,                          // string label,
                imageFileName,                  // string imageFileName,
                index,                          // int index,
                isPublic,                       // bool isPublic,
                targetLanguageIDs,              // List<LanguageID> targetLanguageIDs,
                hostLanguageIDs,                // List<LanguageID> hostLanguageIDs,
                owner,                          // string owner,
                targetNode,                     // BaseObjectNode node,
                sourceContent.ContentType,      // string contentType,
                sourceContent.ContentSubType,   // string contentSubType,
                targetContentStorage,           // BaseContentStorage contentStorage,
                options,                        // List<IBaseObjectKeyed> options,
                markupTemplate,                 // MarkupTemplate markupTemplate,
                markupReference,                // MarkupTemplateReference markupReference,
                targetContentParent,            // BaseObjectContent contentParent,
                null);                          // List<BaseObjectContent> contentChildren

            targetContent.Guid = guid;
            targetContent.Directory = sourceContent.Directory;

            if (targetContentParent != null)
                targetContentParent.AddContentChild(targetContent);
            else
                targetNode.AddContentChild(targetContent);

            if (!String.IsNullOrEmpty(imageFileName))
            {
                if (!CopyMediaFile(sourceContentMediaPath, targetContentMediaPath, imageFileName))
                    return false;
            }

            List<string> copiedFiles = new List<string>();
            string errorMessage = null;

            if (sourceContent.CopyMedia(targetContentMediaPath, copiedFiles, ref errorMessage))
            {
                if (!TransferContentChildren(
                        sourceRepository,
                        targetRepository,
                        sourceTreeMediaPath,
                        targetTreeMediaPath,
                        targetNodeUtilities,
                        targetNode,
                        targetContent,
                        sourceNode,
                        sourceContent))
                    return false;
            }
            else
                PutError(errorMessage);

            return true;
        }

        protected virtual bool TransferContentChildren(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string sourceTreeMediaPath,
            string targetTreeMediaPath,
            NodeUtilities targetNodeUtilities,
            BaseObjectNode targetNode,
            BaseObjectContent targetContentParent,
            BaseObjectNode sourceNode,
            BaseObjectContent sourceContent)
        {
            List<BaseObjectContent> sourceContentList = sourceContent.ContentChildren;

            if (sourceContentList == null)
                return true;

            foreach (BaseObjectContent sourceContentChild in sourceContentList)
            {
                if (sourceContentChild.KeyString == sourceContent.KeyString)
                    continue;

                if (!TransferContent(
                        sourceRepository,
                        targetRepository,
                        sourceTreeMediaPath,
                        targetTreeMediaPath,
                        targetNodeUtilities,
                        targetNode,
                        targetContentParent,
                        sourceNode,
                        sourceContentChild))
                    return false;
            }

            return true;
        }

        protected bool TransferContentStorage(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string sourceTreeMediaPath,
            string targetTreeMediaPath,
            NodeUtilities targetNodeUtilities,
            BaseObjectContent sourceContent,
            out BaseContentStorage targetContentStorage)
        {
            int contentStorageKey = sourceContent.ContentStorageKey;
            BaseContentStorage sourceContentStorage = sourceContent.ContentStorage;

            targetContentStorage = null;

            if (sourceContent.HasContentStorageKey)
            {
                if (sourceContentStorage == null)
                {
                    sourceContent.ResolveReferences(sourceRepository, false, false);
                    sourceContentStorage = sourceContent.ContentStorage;

                    if (sourceContentStorage == null)
                    {
                        PutError("Failed to resolve content storage: " + sourceContent.KeyString
                            + " node: " + sourceContent.Node.GetTitleString(UILanguageID));
                        //return false;
                    }
                }
            }

            if (sourceContentStorage != null)
            {
                targetContentStorage = sourceContentStorage.Clone() as BaseContentStorage;

                MarkupTemplate localMarkupTemplate = GetTargetLocalMarkupTemplate(
                    sourceRepository,
                    targetRepository,
                    sourceContentStorage.LocalMarkupTemplate);
                MarkupTemplateReference markupReference = GetTargetMarkupTemplateReference(
                    sourceRepository,
                    targetRepository,
                    sourceContentStorage.MarkupReference);

                if (localMarkupTemplate != null)
                    targetContentStorage.LocalMarkupTemplate = localMarkupTemplate;

                if (markupReference != null)
                    targetContentStorage.MarkupReference = markupReference;

                targetContentStorage.Key = 0;
                targetContentStorage.Guid = sourceContentStorage.Guid;
                targetContentStorage.TouchAndClearModified();

                try
                {
                    if (targetRepository.SaveReference(targetContentStorage.Source, null, targetContentStorage))
                        return true;
                    else
                        PutError("Error adding content: " + sourceContent.GetTitleString(UILanguageID)
                            + " node: " + sourceContent.Node.GetTitleString(UILanguageID));
                }
                catch (Exception exception)
                {
                    PutExceptionError("Error adding content: " + sourceContent.GetTitleString(UILanguageID)
                            + " node: " + sourceContent.Node.GetTitleString(UILanguageID),
                        exception);
                }
            }

            return true;
        }

        protected MarkupTemplate GetTargetLocalMarkupTemplate(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            MarkupTemplate sourceMarkupTemplate)
        {
            MarkupTemplate targetMarkupTemplate = null;

            if (sourceMarkupTemplate != null)
            {
                targetMarkupTemplate = new MarkupTemplate(sourceMarkupTemplate);

                string sourceMediaDirectoryPath = sourceMarkupTemplate.MediaDirectoryPath;

                if (!String.IsNullOrEmpty(sourceMediaDirectoryPath))
                {
                    string targetMediaDirectoryPath = sourceMediaDirectoryPath.Replace(sourceRepository.ContentPath, targetRepository.ContentPath);

                    CopyMediaDirectory(
                        sourceMediaDirectoryPath,
                        targetMediaDirectoryPath);
                }
            }

            return targetMarkupTemplate;
        }

        protected MarkupTemplateReference GetTargetMarkupTemplateReference(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            MarkupTemplateReference sourceMarkupReference)
        {
            MarkupTemplateReference targetMarkupReference = null;

            if ((sourceMarkupReference != null) && (sourceMarkupReference.KeyString != "(none)") && (sourceMarkupReference.Key != null))
            {
                MarkupTemplate sourceMarkupTemplate = sourceMarkupReference.Item;

                if ((sourceMarkupTemplate == null) && (sourceMarkupReference.KeyString != "(local)"))
                    sourceMarkupTemplate = sourceRepository.MarkupTemplates.Get(sourceMarkupReference.Key);

                if (sourceMarkupTemplate != null)
                {
                    MarkupTemplate targetMarkupTemplate;

                    targetMarkupTemplate = new MarkupTemplate(0, sourceMarkupTemplate);

                    try
                    {
                        targetMarkupTemplate.Key = 0;
                        targetMarkupTemplate.Guid = sourceMarkupTemplate.Guid;
                        targetMarkupTemplate.TouchAndClearModified();

                        if (targetRepository.MarkupTemplates.Add(targetMarkupTemplate))
                        {
                            targetMarkupReference = new MarkupTemplateReference(targetMarkupTemplate);

                            string sourceMediaDirectoryPath = sourceMarkupTemplate.MediaDirectoryPath;

                            if (!String.IsNullOrEmpty(sourceMediaDirectoryPath))
                            {
                                string targetMediaDirectoryPath = sourceMediaDirectoryPath.Replace(sourceRepository.ContentPath, targetRepository.ContentPath);

                                CopyMediaDirectory(
                                    sourceMediaDirectoryPath,
                                    targetMediaDirectoryPath);
                            }
                        }
                        else
                        {
                            PutError("Error adding cloned target markup template: " + targetMarkupTemplate.Name);
                            return null;
                        }
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Exception adding cloned target markup template: " + targetMarkupTemplate.Name, exc);
                        return null;
                    }
                }
            }

            return targetMarkupReference;
        }

        protected NodeMaster GetTargetNodeMaster(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            string owner,
            NodeMaster sourceNodeMaster)
        {
            NodeMaster targetNodeMaster = null;

            if (sourceNodeMaster != null)
            {
                targetNodeMaster = targetRepository.ResolveNamedReference(
                    "NodeMasters",
                    null,
                    owner,
                    sourceNodeMaster.Name) as NodeMaster;

                if (targetNodeMaster == null)
                {
                    targetNodeMaster = new NodeMaster(0, sourceNodeMaster);

                    MarkupTemplateReference markupReference = GetTargetMarkupTemplateReference(
                        sourceRepository,
                        targetRepository,
                        sourceNodeMaster.MarkupReference);

                    if (markupReference != null)
                        targetNodeMaster.MarkupReference = markupReference;

                    markupReference = GetTargetMarkupTemplateReference(
                        sourceRepository,
                        targetRepository,
                        sourceNodeMaster.CopyMarkupReference);

                    if (markupReference != null)
                        targetNodeMaster.CopyMarkupReference = markupReference;

                    if (sourceNodeMaster.ContentItems != null)
                    {
                        foreach (MasterContentItem sourceItem in sourceNodeMaster.ContentItems)
                        {
                            MasterContentItem targetItem = targetNodeMaster.GetContentItem(sourceItem.KeyString);

                            if (targetItem != null)
                            {
                                markupReference = GetTargetMarkupTemplateReference(
                                    sourceRepository,
                                    targetRepository,
                                    sourceItem.MarkupReference);

                                if (markupReference != null)
                                    targetItem.MarkupReference = markupReference;

                                markupReference = GetTargetMarkupTemplateReference(
                                    sourceRepository,
                                    targetRepository,
                                    sourceItem.CopyMarkupReference);

                                if (markupReference != null)
                                    targetItem.CopyMarkupReference = markupReference;
                            }
                        }
                    }

                    try
                    {
                        targetNodeMaster.Key = 0;
                        targetNodeMaster.Guid = sourceNodeMaster.Guid;
                        targetNodeMaster.TouchAndClearModified();

                        if (!targetRepository.NodeMasters.Add(targetNodeMaster))
                        {
                            PutError("Error adding cloned target markup template: " + targetNodeMaster.Name);
                            return null;
                        }
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Exception adding cloned target markup template: " + targetNodeMaster.Name, exc);
                        return null;
                    }
                }
            }

            return targetNodeMaster;
        }

        protected virtual bool TransferAudio(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            LanguageID targetLanguageID,
            List<AudioMultiReference> audioReferences)
        {
            List<IBaseObjectKeyed> targetObjects = new List<IBaseObjectKeyed>();
            string sourceDirectoryPath = MediaUtilities.ConcatenateFilePath4(
                sourceRepository.ContentPath,
                "Dictionary",
                "Audio",
                targetLanguageID.LanguageCultureExtensionCode);
            string targetDirectoryPath = MediaUtilities.ConcatenateFilePath4(
                targetRepository.ContentPath,
                "Dictionary",
                "Audio",
                targetLanguageID.LanguageCultureExtensionCode);
            bool returnValue = true;

            UpdateProgressElapsed("Transfering audio...");

            FileSingleton.DirectoryExistsCheck(targetDirectoryPath);

            foreach (AudioMultiReference audioReference in audioReferences)
            {
                if (audioReference.HasAudioInstances())
                {
                    foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                    {
                        string sourceFilePath = MediaUtilities.ConcatenateFilePath(sourceDirectoryPath, audioInstance.FileName);
                        string targetFilePath = MediaUtilities.ConcatenateFilePath(targetDirectoryPath, audioInstance.FileName);

                        try
                        {
                            if (FileSingleton.Exists(sourceFilePath))
                                FileSingleton.Copy(sourceFilePath, targetFilePath, true);
                        }
                        catch (Exception exc)
                        {
                            PutExceptionError("Error copying audio file: " + sourceFilePath + " to: " + targetFilePath, exc);
                            returnValue = false;
                        }
                    }
                }

                IBaseObjectKeyed targetObject = audioReference.Clone() as IBaseObjectKeyed;

                targetObjects.Add(targetObject);
            }

            returnValue = targetObjectStore.AddList(targetObjects);

            return returnValue;
        }

        protected virtual bool TransferPictures(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            LanguageID targetLanguageID,
            List<PictureReference> pictureReferences)
        {
            List<IBaseObjectKeyed> targetObjects = new List<IBaseObjectKeyed>();
            string sourceDirectoryPath = MediaUtilities.ConcatenateFilePath4(
                sourceRepository.ContentPath,
                "Dictionary",
                "Pictures",
                targetLanguageID.LanguageCultureExtensionCode);
            string targetDirectoryPath = MediaUtilities.ConcatenateFilePath4(
                targetRepository.ContentPath,
                "Dictionary",
                "Pictures",
                targetLanguageID.LanguageCultureExtensionCode);
            bool returnValue = true;

            UpdateProgressElapsed("Transfering pictures...");

            FileSingleton.DirectoryExistsCheck(targetDirectoryPath);

            foreach (PictureReference pictureReference in pictureReferences)
            {
                string sourceFilePath = MediaUtilities.ConcatenateFilePath(sourceDirectoryPath, pictureReference.PictureFilePath);
                string targetFilePath = MediaUtilities.ConcatenateFilePath(targetDirectoryPath, pictureReference.PictureFilePath);

                try
                {
                    if (FileSingleton.Exists(sourceFilePath))
                        FileSingleton.Copy(sourceFilePath, targetFilePath, true);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error copying picdture file: " + sourceFilePath + " to: " + targetFilePath, exc);
                    returnValue = false;
                }

                IBaseObjectKeyed targetObject = pictureReference.Clone() as IBaseObjectKeyed;

                targetObjects.Add(targetObject);
            }

            returnValue = targetObjectStore.AddList(targetObjects);

            return returnValue;
        }

        protected bool DeleteCheck(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            NodeUtilities targetNodeUtilities,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            string storeName,
            bool isAll,
            List<IBaseObjectKeyed> targets)
        {
            List<IBaseObjectKeyed> deleteList = new List<IBaseObjectKeyed>();
            bool returnValue = true;

            if (isAll)
            {
                switch (storeName)
                {
                    case "Dictionary":
                        returnValue = targetObjectStore.DeleteAll();
                        return returnValue;
                    default:
                        break;
                }
            }

            foreach (IBaseObjectKeyed target in targets)
            {
                IBaseObjectKeyed priorObject;
                string namedObjectStoreName = null;
                ObjectReferenceNodeTree treeHeader;
                BaseObjectNodeTree tree;

                switch (storeName)
                {
                    case "Courses":
                        namedObjectStoreName = "CourseHeaders";
                        break;
                    case "Plans":
                        namedObjectStoreName = "PlanHeaders";
                        break;
                    default:
                        namedObjectStoreName = storeName;
                        break;
                }

                if ((priorObject = targetRepository.ResolveNamedReference(
                       namedObjectStoreName,
                       targetLanguageID,
                       target.Owner,
                       target.Name)) != null)
                {
                    switch (storeName)
                    {
                        case "Courses":
                            treeHeader = priorObject as ObjectReferenceNodeTree;
                            if ((tree = targetRepository.Courses.Get(treeHeader.Key)) != null)
                                deleteList.Add(tree);
                            break;
                        case "Plans":
                            treeHeader = priorObject as ObjectReferenceNodeTree;
                            if ((tree = targetRepository.Plans.Get(treeHeader.Key)) != null)
                                deleteList.Add(tree);
                            break;
                        default:
                            deleteList.Add(priorObject);
                            break;
                    }
                }
            }

            if (deleteList.Count() == 0)
                return true;

            switch (storeName)
            {
                case "Courses":
                case "Plans":
                    foreach (IBaseObjectKeyed deleteObject in deleteList)
                    {
                        BaseObjectNodeTree tree = deleteObject as BaseObjectNodeTree;

                        if (!DeletePriorTree(
                                sourceRepository,
                                targetRepository,
                                targetObjectStore,
                                tree,
                                storeName,
                                targetLanguageID,
                                targetNodeUtilities))
                            returnValue = false;
                    }
                    break;
                case "MarkupTemplates":
                case "NodeMasters":
                case "Dictionary":
                    if (isAll)
                        returnValue = targetObjectStore.DeleteAll();
                    else
                        returnValue = targetObjectStore.DeleteList(deleteList);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DeleteAudio(
                        targetRepository,
                        targetObjectStore,
                        targetLanguageID,
                        isAll,
                        targets.Cast<AudioMultiReference>().ToList());
                    break;
                case "DictionaryPictures":
                    returnValue = DeletePictures(
                        targetRepository,
                        targetObjectStore,
                        targetLanguageID,
                        isAll,
                        targets.Cast<PictureReference>().ToList());
                    break;
                case "UserRecords":
                    // Don't delete user records.
                    break;
                case "UIText":
                case "UIStrings":
                case "ToolProfiles":
                    throw new Exception("Unsupported delete for object store: " + storeName);
                default:
                    throw new Exception("Unexpected store name: " + storeName);
            }

            return true;
        }

        protected bool DeletePriorTree(
            IMainRepository sourceRepository,
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            BaseObjectNodeTree sourceTree,
            string storeName,
            LanguageID targetLanguageID,
            NodeUtilities targetNodeUtilities)
        {
            string headersStoreName;

            switch (storeName)
            {
                case "Courses":
                    headersStoreName = "CourseHeaders";
                    break;
                case "Plans":
                    headersStoreName = "PlanHeaders";
                    break;
                default:
                    return false;
            }

            ObjectReferenceNodeTree priorTreeReference = targetRepository.ResolveNamedReference(
                headersStoreName,
                targetLanguageID,
                sourceTree.Owner,
                sourceTree.Name) as ObjectReferenceNodeTree;

            if (priorTreeReference != null)
            {
                BaseObjectNodeTree priorTree = targetObjectStore.Get(priorTreeReference.Key) as BaseObjectNodeTree;

                if (priorTree != null)
                {
                    string mediaDirectory = priorTree.MediaDirectoryPath;

                    if (targetNodeUtilities.DeleteTreeHelper(priorTree, false))
                    {
                        string sourceTreeMediaPath = MediaUtilities.ConcatenateFilePath4(
                            sourceRepository.ContentPath,
                            "Media",
                            sourceTree.Owner,
                            sourceTree.Directory);
                        string targetTreeMediaPath = MediaUtilities.ConcatenateFilePath4(
                            targetRepository.ContentPath,
                            "Media",
                            sourceTree.Owner,
                            sourceTree.Directory);
                        mediaDirectory = mediaDirectory.Replace(sourceTreeMediaPath, targetTreeMediaPath);

                        if (!String.IsNullOrEmpty(mediaDirectory) && !MediaUtilities.DirectoryDeleteSafe(mediaDirectory))
                        {
                            PutError("Error deleting target tree media directory: " + mediaDirectory);
                            return false;
                        }
                    }
                    else
                    {
                        PutError("Error deleting prior target tree: " + priorTree.Name);
                        return false;
                    }
                }
                else
                {
                    if (!targetRepository.DeleteReference(headersStoreName, null, priorTreeReference.Key))
                    {
                        PutError("Error deleting prior target tree header: " + priorTreeReference.Name);
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool CopyMediaDirectory(
            string sourceMediaDirectoryPath,
            string targetMediaDirectoryPath)
        {
            if (String.IsNullOrEmpty(sourceMediaDirectoryPath))
                return true;

            if (String.IsNullOrEmpty(targetMediaDirectoryPath))
                return true;

            try
            {
                if (FileSingleton.DirectoryExists(sourceMediaDirectoryPath))
                {
                    if (!FileSingleton.Exists(targetMediaDirectoryPath))
                        FileSingleton.CreateDirectory(targetMediaDirectoryPath);

                    FileSingleton.CopyDirectory(sourceMediaDirectoryPath, targetMediaDirectoryPath, true);
                }

                return true;
            }
            catch (Exception exc)
            {
                PutExceptionError("Error copying " + sourceMediaDirectoryPath + " to " + targetMediaDirectoryPath, exc);
                return false;
            }
        }

        protected bool CopyMediaFile(
            string sourceDirectoryPath,
            string targetDirectoryPath,
            string fileName)
        {
            fileName = EnsurePlatformFilePath(fileName);

            string sourcePath = MediaUtilities.ConcatenateFilePath(sourceDirectoryPath, fileName);
            string targetPath = MediaUtilities.ConcatenateFilePath(targetDirectoryPath, fileName);

            try
            {
                if (FileSingleton.Exists(sourcePath))
                {
                    FileSingleton.DirectoryExistsCheck(targetPath);

                    if (!FileSingleton.Exists(targetPath))
                        FileSingleton.Copy(sourcePath, targetPath);
                }

                return true;
            }
            catch (Exception exc)
            {
                PutExceptionError("Error copying " + sourcePath + " to " + targetPath, exc);
                return false;
            }
        }

        protected bool DeleteAudio(
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            LanguageID targetLanguageID,
            bool isAll,
            List<AudioMultiReference> audioReferences)
        {
            string directoryPath = MediaUtilities.ConcatenateFilePath4(
                targetRepository.ContentPath,
                "Dictionary",
                "Audio",
                targetLanguageID.LanguageCultureExtensionCode);
            bool returnValue = true;

            if (isAll)
            {
                try
                {
                    FileSingleton.DeleteDirectory(directoryPath);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error deleting audio directory: " + directoryPath, exc);
                    return false;
                }
                try
                {
                    returnValue = targetObjectStore.DeleteAll();
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error deleting audio records", exc);
                    return false;
                }
            }
            else
            {
                foreach (AudioMultiReference audioReference in audioReferences)
                {
                    if (audioReference.HasAudioInstances())
                    {
                        foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                        {
                            string filePath = MediaUtilities.ConcatenateFilePath(directoryPath, audioInstance.FileName);
                            try
                            {
                                if (FileSingleton.Exists(filePath))
                                    FileSingleton.Delete(filePath);
                            }
                            catch (Exception exc)
                            {
                                PutExceptionError("Error deleting audio instance file: " + filePath, exc);
                                returnValue = false;
                            }
                        }
                    }
                }

                if (!targetObjectStore.DeleteList(audioReferences.Cast<IBaseObjectKeyed>().ToList()))
                {
                    PutError("Error deleting audio multi reference list.");
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected bool DeletePictures(
            IMainRepository targetRepository,
            IObjectStore targetObjectStore,
            LanguageID targetLanguageID,
            bool isAll,
            List<PictureReference> pictureReferences)
        {
            string directoryPath = MediaUtilities.ConcatenateFilePath4(
                targetRepository.ContentPath,
                "Dictionary",
                "Pictures",
                targetLanguageID.LanguageCultureExtensionCode);
            bool returnValue = true;

            if (isAll)
            {
                try
                {
                    FileSingleton.DeleteDirectory(directoryPath);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error deleting picture directory: " + directoryPath, exc);
                    return false;
                }
                try
                {
                    returnValue = targetObjectStore.DeleteAll();
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error deleting picture records", exc);
                    return false;
                }
            }
            else
            {
                foreach (PictureReference pictureReference in pictureReferences)
                {
                    string filePath = MediaUtilities.ConcatenateFilePath(directoryPath, pictureReference.PictureFilePath);

                    try
                    {
                        if (FileSingleton.Exists(filePath))
                            FileSingleton.Delete(filePath);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Error deleting audio instance file: " + filePath, exc);
                        returnValue = false;
                    }
                }

                if (!targetObjectStore.DeleteList(pictureReferences.Cast<IBaseObjectKeyed>().ToList()))
                {
                    PutError("Error deleting picture reference list.");
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected string EnsurePlatformFilePath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            if ((ApplicationData.PlatformPathSeparator == "\\") && path.Contains("/"))
                return path.Replace('/', '\\');
            else if ((ApplicationData.PlatformPathSeparator == "/") && path.Contains("\\"))
                return path.Replace('\\', '/');

            return path;
        }
    }
}

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
        public void InitializeContentSelectFlags(BaseObjectNode node, Dictionary<string, bool> contentSelectFlags, bool state)
        {
            if ((contentSelectFlags == null) || (node == null))
                return;

            List<string> contentKeys = node.ContentKeysList;
            InitializeContentSelectFlags(contentKeys, contentSelectFlags, state);
        }

        public void InitializeContentSelectFlags(List<string> contentKeys, Dictionary<string, bool> contentSelectFlags, bool state)
        {
            if ((contentSelectFlags == null) || (contentKeys == null))
                return;

            int contentCount = contentKeys.Count();
            int contentIndex;
            string contentKey;

            for (contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                contentKey = contentKeys[contentIndex];
                contentSelectFlags[contentKey] = state;
            }
        }

        public void InitializeSubContentSelectFlags(BaseObjectContent content, Dictionary<string, bool> contentSelectFlags, bool state)
        {
            if (contentSelectFlags == null)
                return;

            List<string> contentKeys = content.ContentKeysList;
            InitializeContentSelectFlags(contentKeys, contentSelectFlags, state);
        }

        public void InitializeMediaSelectFlags(
            List<BaseObjectContent> contents,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            ref Dictionary<string, bool> contentSelectFlags,
            bool state)
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
            bool testFlag;

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
                    if (!contentSelectFlags.TryGetValue(filePath, out testFlag))
                        contentSelectFlags.Add(filePath, state);
                    else
                        contentSelectFlags[filePath] = state;
                    mediaIndex++;
                }
            }
        }

        public List<int> GetNodeKeys(BaseObjectNode node)
        {
            List<int> nodeKeys = null;

            if (node != null)
            {
                List<BaseObjectNode> nodes = new List<BaseObjectNode>();
                node.CollectDescendents(nodes);
                if (nodes.Count() != 0)
                {
                    nodeKeys = new List<int>(nodes.Count());

                    foreach (BaseObjectNode childNode in nodes)
                        nodeKeys.Add(childNode.KeyInt);
                }
            }

            return nodeKeys;
        }

        public void GetNodeAndContentKeys(BaseObjectNode node, out List<int> nodeKeys, out List<string> contentKeys)
        {
            nodeKeys = null;
            contentKeys = null;

            if (node != null)
            {
                node.ResolveReferences(Repositories, false, true);
                nodeKeys = new List<int>();
                contentKeys = new List<string>();
                node.CollectDescendentKeys(nodeKeys);
                node.CollectContentKeys(contentKeys);
            }
        }

        public void GetPrintNodeContentKeys(BaseObjectNode node, List<string> contentKeys)
        {
            if (node != null)
                node.CollectStudyListAndDocumentContentKeys(contentKeys);
        }

        public void GetPrintNodeAndContentKeys(BaseObjectNode node, out List<int> nodeKeys, out List<string> contentKeys)
        {
            nodeKeys = null;
            contentKeys = null;

            if (node != null)
            {
                node.ResolveReferences(Repositories, false, true);
                nodeKeys = new List<int>();
                contentKeys = new List<string>();
                node.CollectDescendentKeys(nodeKeys);
                node.CollectStudyListAndDocumentContentKeys(contentKeys);
            }
        }

        public void GetDownloadNodeAndContentKeys(BaseObjectNode node, out List<int> nodeKeys, out List<string> contentKeys)
        {
            nodeKeys = null;
            contentKeys = null;

            if (node != null)
            {
                node.ResolveReferences(Repositories, false, true);
                nodeKeys = new List<int>();
                contentKeys = new List<string>();
                node.CollectDescendentKeys(nodeKeys);
                node.CollectMediaContentKeys(contentKeys);
                node.CollectStudyListAndDocumentContentKeys(contentKeys);
            }
        }

        public void GetNodeContentKeys(BaseObjectNode node, List<string> contentKeys)
        {
            if (node != null)
                node.CollectContentKeys(contentKeys);
        }

        public void InitializeNodeSelectFlags(BaseObjectNode node, Dictionary<int, bool> nodeSelectFlags, bool state)
        {
            if ((node == null) || (nodeSelectFlags == null))
                return;

            List<int> nodeKeys = node.GetDecendentKeys(null);
            InitializeNodeSelectFlags(nodeKeys, nodeSelectFlags, state);
        }

        public void InitializeNodeSelectFlags(List<int> nodeKeys, Dictionary<int, bool> nodeSelectFlags, bool state)
        {
            if ((nodeSelectFlags == null) || (nodeKeys == null))
                return;

            int nodeCount = nodeKeys.Count();
            int nodeIndex;
            int nodeKey;

            for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                nodeKey = nodeKeys[nodeIndex];
                nodeSelectFlags[nodeKey] = state;
            }
        }

        public void InitializeLanguageSelectFlags(List<LanguageDescriptor> languageDescriptors,
            Dictionary<string, bool> languageSelectFlags, bool state)
        {
            if ((languageSelectFlags == null) || (languageDescriptors == null))
                return;

            int languageCount = languageDescriptors.Count();
            int languageIndex;
            string languageKey;

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                languageKey = languageDescriptors[languageIndex].LanguageID.LanguageCultureExtensionCode;
                languageSelectFlags[languageKey] = state;
            }
        }

        public void InitializeLanguageIDFlags(List<LanguageID> languageIDs,
            Dictionary<string, bool> languageSelectFlags, bool state)
        {
            if ((languageSelectFlags == null) || (languageIDs == null))
                return;

            int languageCount = languageIDs.Count();
            int languageIndex;
            string languageKey;

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                languageKey = languageIDs[languageIndex].LanguageCultureExtensionCode;
                languageSelectFlags[languageKey] = state;
            }
        }

        public void InitializeItemSelectFlags(
            List<MultiLanguageItem> studyItems, Dictionary<string, bool> itemSelectFlags, bool state)
        {
            if ((itemSelectFlags == null) || (studyItems == null))
                return;

            int itemCount = studyItems.Count;
            int itemIndex;
            string itemKey;

            itemSelectFlags.Clear();

            for (itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                MultiLanguageItem studyItem = studyItems[itemIndex];
                if (studyItem == null)
                    continue;
                itemKey = studyItem.CompoundStudyItemKey;
                itemSelectFlags.Add(itemKey, state);
            }
        }

        public static void CollectMediaFiles(IBaseObject obj, List<string> mediaFiles, VisitMedia visitFunction)
        {
            obj.CollectReferences(null, null, null, null, null, null, null, mediaFiles, visitFunction);
        }

        public static long CollectMediaFilesSize(IBaseObject obj, Dictionary<string, bool> languageSelectFlags,
            VisitMedia visitFunction)
        {
            List<string> mediaFiles = new List<string>();
            long size = 0L;

            if (obj != null)
            {
                obj.CollectReferences(null, null, null, null, null, null, languageSelectFlags, mediaFiles,
                    visitFunction);
                size = MediaUtilities.GetMediaFilesSize(mediaFiles);
            }

            return size;
        }
    }
}

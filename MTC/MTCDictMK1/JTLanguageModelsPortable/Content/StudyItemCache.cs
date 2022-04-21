using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Content
{
    public class StudyItemCache : BaseObjectKeyed
    {
        Dictionary<string, Dictionary<string, MultiLanguageItem>> MasterCache;

        public StudyItemCache(object key) : base(key)
        {
            MasterCache = new Dictionary<string, Dictionary<string, MultiLanguageItem>>();
        }

        public StudyItemCache()
        {
            MasterCache = new Dictionary<string, Dictionary<string, MultiLanguageItem>>();
        }

        public bool Find(
            string contentKey,
            LanguageID targetLanguageID,
            string text,
            out MultiLanguageItem priorStudyItem)
        {
            Dictionary<string, MultiLanguageItem> subCache = GetSubCache(contentKey, targetLanguageID);

            if (subCache.TryGetValue(text, out priorStudyItem))
                return true;

            return false;
        }

        public bool Add(
            string contentKey,
            LanguageID targetLanguageID,
            MultiLanguageItem studyItem)
        {
            Dictionary<string, MultiLanguageItem> subCache = GetSubCache(contentKey, targetLanguageID);
            string key = studyItem.Text(targetLanguageID);

            if (!String.IsNullOrEmpty(key))
            {
                subCache.Add(key, studyItem);
                return true;
            }

            return false;
        }

        public bool Add(
            string contentKey,
            LanguageID targetLanguageID,
            string textKey,
            MultiLanguageItem studyItem)
        {
            Dictionary<string, MultiLanguageItem> subCache = GetSubCache(contentKey, targetLanguageID);
            subCache.Add(textKey, studyItem);
            return true;
        }

        // Returns false if any study items were already cached.
        public bool Add(
            string contentKey,
            LanguageID targetLanguageID,
            ContentStudyList studyList,
            bool recurse)
        {
            Dictionary<string, MultiLanguageItem> subCache = GetSubCache(contentKey, targetLanguageID);
            return AddRecurse(contentKey, targetLanguageID, subCache, studyList, recurse);
        }

        // Returns false if any study items were already cached.
        protected bool AddRecurse(
            string contentKey,
            LanguageID targetLanguageID,
            Dictionary<string, MultiLanguageItem> subCache,
            ContentStudyList studyList,
            bool recurse)
        {
            if (studyList == null)
                return true;

            List<MultiLanguageItem> studyItems = studyList.StudyItems;
            bool returnValue = true;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                string key = studyItem.Text(targetLanguageID);
                MultiLanguageItem testStudyItem;

                if (!String.IsNullOrEmpty(key))
                {
                    if (!subCache.TryGetValue(key, out testStudyItem))
                        subCache.Add(key, studyItem);
                    else
                        returnValue = false;
                }
            }

            BaseObjectContent content = studyList.Content;

            if (recurse && (content != null) && content.HasContentChildren())
            {
                foreach (BaseObjectContent contentChild in content.ContentChildren)
                {
                    if (contentChild.KeyString == content.KeyString)
                        continue;
                    ContentStudyList studyListChild = contentChild.ContentStorageStudyList;
                    returnValue = AddRecurse(
                        contentKey,
                        targetLanguageID,
                        subCache,
                        studyListChild,
                        recurse) && returnValue;
                }
            }

            return returnValue;
        }

        // Returns false if any study items were already cached.
        public bool Add(
            string contentKey,
            LanguageID targetLanguageID,
            BaseObjectNode node,
            bool recurse)
        {
            Dictionary<string, MultiLanguageItem> subCache = GetSubCache(contentKey, targetLanguageID);
            return AddRecurse(contentKey, targetLanguageID, subCache, node, recurse);
        }

        // Returns false if any study items were already cached.
        protected bool AddRecurse(
            string contentKey,
            LanguageID targetLanguageID,
            Dictionary<string, MultiLanguageItem> subCache,
            BaseObjectNode node,
            bool recurse)
        {
            if (node == null)
                return true;

            BaseObjectContent content = node.GetContent(contentKey);
            bool returnValue = true;

            if (content != null)
            {
                ContentStudyList studyList = content.ContentStorageStudyList;

                if (studyList != null)
                    returnValue = AddRecurse(contentKey, targetLanguageID, subCache, studyList, recurse) && returnValue;
            }

            if (recurse && node.HasChildren())
            {
                foreach (BaseObjectNode nodeChild in node.Children)
                    returnValue = AddRecurse(
                        contentKey,
                        targetLanguageID,
                        subCache,
                        nodeChild,
                        recurse) && returnValue;
            }

            return returnValue;
        }

        protected Dictionary<string, MultiLanguageItem> GetSubCache(
            string contentKey,
            LanguageID targetLanguageID)
        {
            Dictionary<string, MultiLanguageItem> subCache;
            string key = ComposeSubCacheKey(contentKey, targetLanguageID);
            if (MasterCache.TryGetValue(key, out subCache))
                return subCache;
            subCache = new Dictionary<string, MultiLanguageItem>();
            MasterCache.Add(key, subCache);
            return subCache;
        }

        protected string ComposeSubCacheKey(
            string contentKey,
            LanguageID targetLanguageID)
        {
            string key = contentKey + "_" + targetLanguageID.LanguageCultureExtensionCode;
            return key;
        }
    }
}

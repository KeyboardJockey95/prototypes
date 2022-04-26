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
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class ContentStudyList : BaseContentStorage
    {
        protected List<MultiLanguageString> _SpeakerNames;
        protected List<MultiLanguageItem> _StudyItems;
        protected int _StudyItemOrdinal;
        protected ContentStudyList _StudyListSource;    // If referencing only one study list complete.
        public Dictionary<object, ContentStudyList> StudyListCache;   // For reference items. Not saved.

        public ContentStudyList(object key, BaseObjectContent content,
                List<MultiLanguageString> speakerNames, List<MultiLanguageItem> items)
            : base(key, "StudyLists", content)
        {
            _SpeakerNames = speakerNames;
            if (items == null)
                items = new List<MultiLanguageItem>();
            MultiLanguageItem.SetStudyItemsStudyLists(items, this);
            _StudyItems = items;
            _StudyItemOrdinal = (items != null ? items.Count() : 0);
            _StudyListSource = null;
            StudyListCache = null;
        }

        public ContentStudyList(object key, string source)
            : base(key, source, null)
        {
            ClearContentStudyList();
        }

        public ContentStudyList(object key)
            : base(key, "StudyLists", null)
        {
            ClearContentStudyList();
        }

        public ContentStudyList(ContentStudyList other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ContentStudyList(XElement element)
        {
            OnElement(element);
        }

        public ContentStudyList()
        {
            ClearContentStudyList();
        }

        public void Copy(ContentStudyList other)
        {
            base.Copy(other);
            CopyShallow(other);

        }

        public void CopyShallow(ContentStudyList other)
        {
            if (other == null)
                return;

            _SpeakerNames = other.CloneSpeakerNames();
            _StudyItems = other.CloneStudyItems();
            MultiLanguageItem.SetStudyItemsStudyLists(_StudyItems, this);
            _StudyItemOrdinal = other.StudyItemOrdinal;
            _StudyListSource = other.StudyListSource;
            StudyListCache = null;

            ModifiedFlag = true;
        }

        public void CopyDeep(ContentStudyList other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void PropagateLanguages(BaseObjectLanguages other)
        {
            if (_StudyItems != null)
            {
                List<LanguageID> languageIDs = other.ExpandLanguageIDs(null);

                foreach (MultiLanguageItem studyItem in _StudyItems)
                    studyItem.PropagateLanguages(languageIDs);
            }
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentStudyList();
        }

        public void ClearContentStudyList()
        {
            _Source = "StudyLists";
            _SpeakerNames = new List<MultiLanguageString>();
            _StudyItems = new List<MultiLanguageItem>();
            _StudyItemOrdinal = 0;
            _StudyListSource = null;
            StudyListCache = null;
        }

        public override IBaseObject Clone()
        {
            return new ContentStudyList(this);
        }

        public override ContentClassType ContentClass
        {
            get
            {
                return ContentClassType.StudyList;
            }
        }

        public List<MultiLanguageString> SpeakerNames
        {
            get
            {
                return _SpeakerNames;
            }
            set
            {
                if (_SpeakerNames != value)
                    ModifiedFlag = true;

                _SpeakerNames = value;
            }
        }

        public List<MultiLanguageString> SpeakerNamesRecurse
        {
            get
            {
                List<MultiLanguageString> speakerNames = new List<MultiLanguageString>();
                CollectSpeakerNames(speakerNames, true);
                return speakerNames;
            }
        }

        public void CollectSpeakerNames(List<MultiLanguageString> speakerNames, bool recurse)
        {
            if (_SpeakerNames != null)
            {
                foreach (MultiLanguageString speakerName in _SpeakerNames)
                {
                    if (speakerNames.FirstOrDefault(x => x.MatchKey(speakerName.Key)) == null)
                        speakerNames.Add(speakerName);
                }
            }

            if (recurse)
            {
                BaseObjectContent content = Content;

                if (content != null)
                {
                    BaseObjectNode node = content.Node;
                    string contentKey = content.KeyString;

                    if (content.HasContent())
                    {
                        List<BaseObjectContent> contentChildren = content.ContentList;

                        foreach (BaseObjectContent childContent in contentChildren)
                        {
                            if (childContent.KeyString == content.KeyString)
                                continue;

                            ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                            if (childStudyList != null)
                                childStudyList.CollectSpeakerNames(speakerNames, true);
                        }
                    }

                    if (node != null)
                    {
                        if (node.HasChildren())
                        {
                            foreach (BaseObjectNode childNode in node.Children)
                            {
                                BaseObjectContent childContent = childNode.GetContent(contentKey);

                                if (childContent != null)
                                {
                                    ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                                    if (childStudyList != null)
                                        childStudyList.CollectSpeakerNames(speakerNames, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<MultiLanguageItem> StudyItems
        {
            get
            {
                return _StudyItems;
            }
            set
            {
                if (_StudyItems != value)
                    ModifiedFlag = true;

                if (value == null)
                    _StudyItems = new List<MultiLanguageItem>();
                else
                   _StudyItems = value;

                if (_StudyItemOrdinal < _StudyItems.Count())
                    _StudyItemOrdinal = _StudyItems.Count();

                MultiLanguageItem.SetStudyItemsStudyLists(_StudyItems, this);
            }
        }

        public List<MultiLanguageItem> StudyItemsRecurse
        {
            get
            {
                List<MultiLanguageItem> studyItems = new List<MultiLanguageItem>();
                CollectStudyItems(studyItems, true);
                return studyItems;
            }
        }

        public List<MultiLanguageItem> GetStudyItems(bool recurse)
        {
            if (recurse)
                return StudyItemsRecurse;
            else
                return _StudyItems;
        }

        public void CollectStudyItems(List<MultiLanguageItem> studyItems, bool recurse)
        {
            if (_StudyItems != null)
                studyItems.AddRange(_StudyItems);

            if (recurse)
            {
                BaseObjectContent content = Content;

                if (content != null)
                {
                    BaseObjectNode node = content.Node;
                    string contentKey = content.KeyString;

                    if (content.HasContent())
                    {
                        List<BaseObjectContent> contentChildren = content.ContentList;

                        foreach (BaseObjectContent childContent in contentChildren)
                        {
                            if (childContent.KeyString == content.KeyString)
                                continue;

                            ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                            if (childStudyList != null)
                                childStudyList.CollectStudyItems(studyItems, true);
                        }
                    }

                    CollectNodeDescendentStudyItems(node, contentKey, studyItems);
                }
            }
        }

        public void CollectChildrenStudyItems(List<MultiLanguageItem> studyItems)
        {
            BaseObjectContent content = Content;

            if (content != null)
            {
                if (content.HasContent())
                {
                    List<BaseObjectContent> contentChildren = content.ContentList;

                    foreach (BaseObjectContent childContent in contentChildren)
                    {
                        if (childContent.KeyString == content.KeyString)
                            continue;

                        ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                        if (childStudyList != null)
                            childStudyList.CollectStudyItems(studyItems, true);
                    }
                }
            }
        }

        public void CollectNodeDescendentStudyItems(BaseObjectNode node, string contentKey,
            List<MultiLanguageItem> studyItems)
        {
            if (node != null)
            {
                if (node.HasChildren())
                {
                    foreach (BaseObjectNode childNode in node.Children)
                    {
                        BaseObjectContent childContent = childNode.GetContent(contentKey);

                        if (childContent != null)
                        {
                            ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                            if (childStudyList != null)
                            {
                                childStudyList.CollectStudyItems(studyItems, false);
                                childStudyList.CollectChildrenStudyItems(studyItems);
                            }
                        }

                        CollectNodeDescendentStudyItems(childNode, contentKey, studyItems);
                    }
                }
            }
        }

        public List<MultiLanguageItem> LookupStudyItem(Matcher matcher)
        {
            if (_StudyItems == null)
                return new List<MultiLanguageItem>();

            IEnumerable<MultiLanguageItem> lookupQuery =
                from studyItem in _StudyItems
                where (matcher.Match(studyItem))
                select studyItem;

            return lookupQuery.ToList();
        }

        public MultiLanguageItem FindStudyItem(string text, LanguageID languageID)
        {
            if (_StudyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                if (studyItem.Text(languageID) == text)
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem FindStudyItemRecurse(string text, LanguageID languageID)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;
            string searchText = text.ToLower();

            if (studyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                if (studyItem.Text(languageID).ToLower() == searchText)
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem FindStudyItemInstance(
            MultiLanguageString text,
            List<LanguageID> languageIDs)
        {
            if (_StudyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                if (studyItem.IsCaseInsensitiveTextMatch(text, languageIDs))
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem FindStudyItemInstanceRecurse(
            MultiLanguageString text,
            List<LanguageID> languageIDs)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                if (studyItem.IsCaseInsensitiveTextMatch(text, languageIDs))
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem FindOverlappingStudyItem(MultiLanguageItem newStudyItem)
        {
            if (_StudyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                if (studyItem.IsOverlapping(newStudyItem))
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem FindOverlappingStudyItemAnchored(MultiLanguageItem newStudyItem,
            Dictionary<string, bool> anchorLanguageFlags)
        {
            if (_StudyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                if (studyItem.IsOverlappingAnchored(newStudyItem, anchorLanguageFlags))
                    return studyItem;
            }

            return null;
        }

        public MultiLanguageItem GetStudyItem(object key)
        {
            if ((_StudyItems != null) && (key != null))
                return _StudyItems.FirstOrDefault(x => x.MatchKey(key));
            return null;
        }

        public List<MultiLanguageItem> GetStudyItemRange(int startIndex, int count)
        {
            if ((_StudyItems != null) && (startIndex < _StudyItems.Count()))
            {
                int rangeCount = _StudyItems.Count() - startIndex;
                if (rangeCount > count)
                    rangeCount = count;
                return _StudyItems.GetRange(startIndex, rangeCount);
            }
            return new List<MultiLanguageItem>();
        }

        public MultiLanguageItem GetStudyItemIndexed(int index, bool recurse)
        {
            MultiLanguageItem studyItem;

            if (recurse)
                studyItem = GetStudyItemIndexedRecursed(index);
            else
                studyItem = GetStudyItemIndexed(index);

            return studyItem;
        }

        public MultiLanguageItem GetStudyItemIndexed(int index)
        {
            if ((_StudyItems != null) && (index >= 0) && (index < _StudyItems.Count()))
                return _StudyItems[index];
            return null;
        }

        public MultiLanguageItem GetStudyItemIndexedRecursed(int index)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;
            if ((studyItems != null) && (index >= 0) && (index < studyItems.Count()))
                return studyItems[index];
            return null;
        }

        public int GetStudyItemIndex(MultiLanguageItem studyItem)
        {
            if (studyItem != null)
                return GetStudyItemIndexFromKey(studyItem.KeyString);

            return -1;
        }

        public int GetStudyItemIndexFromKey(string key)
        {
            if ((_StudyItems != null) && (key != null))
            {
                int index;
                int count = _StudyItems.Count();

                for (index = 0; index < count; index++)
                {
                    MultiLanguageItem studyItem = _StudyItems[index];

                    if ((studyItem != null) && studyItem.MatchKey(key))
                        return index;
                }
            }

            return -1;
        }

        public List<MultiLanguageItem> GetStudyItemsSelected(List<bool> itemSelectFlags)
        {
            int studyItemCount = StudyItemCount();
            MultiLanguageItem studyItem;
            List<MultiLanguageItem> returnValue = new List<MultiLanguageItem>();

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = GetStudyItemIndexed(studyItemIndex);

                if (studyItem == null)
                    continue;

                returnValue.Add(studyItem);
            }

            return returnValue;
        }

        public List<MultiLanguageItem> CloneStudyItems()
        {
            if (_StudyItems == null)
                return null;

            List<MultiLanguageItem> returnValue = new List<MultiLanguageItem>(_StudyItems.Count());

            foreach (MultiLanguageItem multiLanguageItem in _StudyItems)
                returnValue.Add(new MultiLanguageItem(multiLanguageItem));

            return returnValue;
        }

        public string Text(int index, LanguageID languageID)
        {
            MultiLanguageItem studyItem = GetStudyItemIndexed(index);

            if (studyItem != null)
                return studyItem.Text(languageID);

            return null;
        }

        public string TextFuzzy(int index, LanguageID languageID)
        {
            MultiLanguageItem studyItem = GetStudyItemIndexed(index);

            if (studyItem != null)
                return studyItem.TextFuzzy(languageID);

            return null;
        }

        public bool AddStudyItem(MultiLanguageItem studyItem)
        {
            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>(1) { studyItem };
            else
                _StudyItems.Add(studyItem);

            studyItem.StudyList = this;

            ModifiedFlag = true;

            return true;
        }

        public bool AddStudyItems(List<MultiLanguageItem> studyItems)
        {
            MultiLanguageItem.SetStudyItemsStudyLists(studyItems, this);

            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>(studyItems);
            else
                _StudyItems.AddRange(studyItems);

            ModifiedFlag = true;

            return true;
        }

        public bool InsertStudyItemIndexed(int index, MultiLanguageItem studyItem)
        {
            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>(1) { studyItem };
            else if ((index >= 0) && (index <= _StudyItems.Count()))
                _StudyItems.Insert(index, studyItem);
            else
                return false;

            studyItem.StudyList = this;

            ModifiedFlag = true;

            return true;
        }

        public bool InsertStudyItemsIndexed(int index, List<MultiLanguageItem> studyItems)
        {
            MultiLanguageItem.SetStudyItemsStudyLists(studyItems, this);

            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>(studyItems);
            else if ((index >= 0) && (index <= _StudyItems.Count()))
                _StudyItems.InsertRange(index, studyItems);
            else
                return false;

            ModifiedFlag = true;

            return true;
        }

        public bool ReplaceStudyItemIndexed(int index, MultiLanguageItem studyItem)
        {
            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>(1) { studyItem };
            else if ((index >= 0) && (index <= _StudyItems.Count()))
                _StudyItems[index] = studyItem;
            else
                return false;

            studyItem.StudyList = this;

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteStudyItem(MultiLanguageItem studyItem)
        {
            if (_StudyItems != null)
            {
                if (_StudyItems.Remove(studyItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteStudyItemIndexed(int index)
        {
            if ((_StudyItems != null) && (index >= 0) && (index < _StudyItems.Count()))
            {
                _StudyItems.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllStudyItems()
        {
            if ((_StudyItems != null) && (_StudyItems.Count != 0))
                ModifiedFlag = true;
            if (_StudyItems == null)
                _StudyItems = new List<MultiLanguageItem>();
            else
               _StudyItems.Clear();
        }

        public bool CloneStudyItemsSelected(ContentStudyList sourceStudyList, List<bool> itemSelectFlags,
            bool isReference)
        {
            BaseObjectContent sourceContent = sourceStudyList.Content;
            int studyItemCount = sourceStudyList.StudyItemCount();
            MultiLanguageItem sourceStudyItem;
            MultiLanguageItem studyItem;
            string itemKey;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                sourceStudyItem = sourceStudyList.GetStudyItemIndexed(studyItemIndex);

                if (sourceStudyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                itemKey = AllocateStudyItemKey();
                studyItem = new MultiLanguageItem(itemKey, sourceStudyItem);

                if (isReference)
                {
                    MultiLanguageItemReference sourceStudyItemReference = new MultiLanguageItemReference(
                        itemKey, sourceStudyItem.Key, sourceStudyList.Key, sourceContent.KeyString,
                        sourceContent.Node.Key, sourceContent.Tree.Key, sourceStudyItem);

                    studyItem.ItemSource = sourceStudyItemReference;
                }

                AddStudyItem(studyItem);
            }

            return returnValue;
        }

        public bool CopyStudyItemsSelected(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int studyItemCount = StudyItemCount();
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = GetStudyItemIndexed(studyItemIndex);

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                studyItems.Add(new MultiLanguageItem(studyItem));
            }

            return returnValue;
        }

        public bool CutStudyItemsSelected(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int studyItemCount = StudyItemCount();
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = GetStudyItemIndexed(studyItemIndex);

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                ContentStudyList targetStudyList = studyItem.StudyList;

                if (targetStudyList == null)
                {
                    returnValue = false;
                    continue;
                }

                studyItems.Add(studyItem);

                targetStudyList.DeleteStudyItem(studyItem);
                ContentUtilities.DeleteSelectFlags(itemSelectFlags, studyItemIndex, 1);
                studyItemCount--;
                studyItemIndex--;
            }

            return returnValue;
        }

        public bool PasteStudyItemsPrepend(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            ContentUtilities.InsertSelectFlags(itemSelectFlags, 0, studyItems.Count(), true);
            RekeyPasteStudyItems(studyItems);
            return InsertStudyItemsIndexed(0, studyItems);
        }

        public bool PasteStudyItemsInsertBefore(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int index = 0;

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.IndexOf(true);

                if (index < 0)
                    index = 0;
            }

            MultiLanguageItem studyItem = GetStudyItemIndexedRecursed(index);

            if (studyItem == null)
                return false;

            ContentStudyList targetStudyList = studyItem.StudyList;

            if (targetStudyList == null)
                return false;

            int targetIndex = targetStudyList.GetStudyItemIndex(studyItem);

            ContentUtilities.InsertSelectFlags(itemSelectFlags, index, studyItems.Count(), true);
            targetStudyList.RekeyPasteStudyItems(studyItems);

            return targetStudyList.InsertStudyItemsIndexed(targetIndex, studyItems);
        }

        public bool PasteStudyItemsOverwrite(
            List<MultiLanguageItem> targetStudyItems,
            List<MultiLanguageItem> sourceStudyItems,
            List<bool> itemSelectFlags)
        {
            ContentStudyList targetStudyList;
            int studyItemIndex;
            int studyItemCount = targetStudyItems.Count;
            int sourceIndex = 0;
            MultiLanguageItem sourceStudyItem;
            MultiLanguageItem targetStudyItem;
            string key;
            bool returnValue = true;

            if ((sourceStudyItems == null) || (sourceStudyItems.Count() == 0))
                return true;

            for (studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                if (sourceIndex < sourceStudyItems.Count())
                    sourceStudyItem = sourceStudyItems[sourceIndex];
                else
                    sourceStudyItem = null;

                targetStudyItem = targetStudyItems[studyItemIndex];
                targetStudyList = targetStudyItem.StudyList;
                int targetIndex = targetStudyList.GetStudyItemIndex(targetStudyItem);
                if (targetIndex < 0)
                {
                    returnValue = false;
                    continue;
                }

                if (sourceStudyItem != null)
                {
                    key = targetStudyList.AllocateStudyItemKey();
                    sourceStudyItem.Rekey(key);
                    targetStudyList.ReplaceStudyItemIndexed(targetIndex, sourceStudyItem);
                }
                else
                {
                    targetStudyList.DeleteStudyItemIndexed(targetIndex);
                    ContentUtilities.DeleteSelectFlags(itemSelectFlags, studyItemIndex, 1);
                    studyItemCount--;
                    studyItemIndex--;
                }

                sourceIndex++;
                ModifiedFlag = true;
            }

            studyItemIndex = itemSelectFlags.LastIndexOf(true);

            if (studyItemIndex < 0)
                studyItemIndex = StudyItemCount();
            else
                studyItemIndex++;

            for (;  sourceIndex < sourceStudyItems.Count(); sourceIndex++)
            {
                sourceStudyItem = sourceStudyItems[sourceIndex];
                key = AllocateStudyItemKey();
                sourceStudyItem.Rekey(key);
                InsertStudyItemIndexed(studyItemIndex, sourceStudyItem);
                ContentUtilities.InsertSelectFlags(itemSelectFlags, studyItemIndex, 1, true);
                studyItemIndex++;
            }

            return returnValue;
        }

        public bool PasteStudyItemsInsertAfter(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            int index = 0;
            MultiLanguageItem studyItem = null;

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.LastIndexOf(true);

                if (index < 0)
                    index = StudyItemCountRecurse() - 1;
            }
            else
                index = StudyItemCountRecurse() - 1;

            studyItem = GetStudyItemIndexedRecursed(index);

            if (studyItem == null)
                return false;

            index++;

            ContentStudyList targetStudyList;

            if (studyItem != null)
            {
                targetStudyList = studyItem.StudyList;

                if (targetStudyList == null)
                    return false;

                int targetIndex = targetStudyList.GetStudyItemIndex(studyItem) + 1;

                ContentUtilities.InsertSelectFlags(itemSelectFlags, index, studyItems.Count(), true);
                targetStudyList.RekeyPasteStudyItems(studyItems);

                return targetStudyList.InsertStudyItemsIndexed(targetIndex, studyItems);
            }
            else
                return PasteStudyItemsAppend(studyItems, itemSelectFlags);
        }

        public bool PasteStudyItemsAppend(List<MultiLanguageItem> studyItems, List<bool> itemSelectFlags)
        {
            bool returnValue;
            ContentUtilities.InsertSelectFlags(itemSelectFlags, StudyItemCount(), studyItems.Count(), true);
            RekeyPasteStudyItems(studyItems);
            returnValue = AddStudyItems(studyItems);
            return returnValue;
        }

        public void RekeyPasteStudyItems(List<MultiLanguageItem> studyItems)
        {
            int studyItemCount = studyItems.Count();
            MultiLanguageItem studyItem;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                studyItem = studyItems[studyItemIndex];

                if (studyItem == null)
                    continue;

                string key = AllocateStudyItemKey();
                studyItem.Rekey(key);
            }
        }

        public int GetSentenceIndexFromStudyItem(int studyItemIndex, LanguageID languageID, int wordRunIndex)
        {
            MultiLanguageItem studyItem = GetStudyItemIndexed(studyItemIndex, true);
            int sentenceIndex = -1;

            if (studyItem != null)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if ((languageItem != null) && languageItem.HasSentenceRuns())
                {
                    TextRun wordRun = languageItem.GetWordRun(wordRunIndex);

                    if (wordRun != null)
                    {
                        int characterIndex = wordRun.Start;
                        sentenceIndex = languageItem.GetSentenceIndexContaining(characterIndex);
                        return sentenceIndex;
                    }
                }
            }

            return -1;
        }

        public int StudyItemCount()
        {
            if (_StudyItems != null)
                return (_StudyItems.Count());

            return 0;
        }

        public int StudyItemCountRecurse()
        {
            return (StudyItemsRecurse.Count);
        }

        public void RekeyStudyItems()
        {
            if (_StudyItems == null)
                return;

            int index = 0;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                object key = "I" + index.ToString();
                studyItem.Rekey(key);
                index++;
            }

            if (_StudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void RekeyStudyItems(int startIndex)
        {
            if (_StudyItems == null)
                return;

            int count = _StudyItems.Count();
            int index = startIndex;

            for (; index < count; index++)
            {
                MultiLanguageItem studyItem = _StudyItems[index];
                object key = "I" + index.ToString();
                studyItem.Rekey(key);
            }

            if (startIndex < _StudyItems.Count())
                ModifiedFlag = true;
        }

        public int SentenceCount(List<LanguageID> languageIDs)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;
            int returnValue = 0;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    returnValue += studyItem.GetMaxSentenceCount(languageIDs);
            }

            return returnValue;
        }

        public void ComputeRuns(DictionaryRepository dictionary)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    studyItem.ComputeSentenceRuns();
                    studyItem.ComputeWordRuns(dictionary);
                }
            }
        }

        public void ComputeSentenceRuns()
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.ComputeSentenceRuns();
            }
        }

        public void ComputeWordRuns(DictionaryRepository dictionary)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.ComputeWordRuns(dictionary);
            }
        }

        public void SentenceRunCheck()
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.SentenceRunCheck();
            }
        }

        public void WordRunCheck(DictionaryRepository dictionary)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.WordRunCheck(dictionary);
            }
        }

        public void WordRunCheckLanguages(List<LanguageID> languageIDs, DictionaryRepository dictionary)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.WordRunCheckLanguages(languageIDs, dictionary);
            }
        }

        public void RunCheck(DictionaryRepository dictionary)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.RunCheck(dictionary);
            }
        }

        public bool HasWordAlignment(
            LanguageID targetLanguageID,
            LanguageID hostLanguageID)
        {
            if ((targetLanguageID == null) || (hostLanguageID == null))
                return false;

            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.HasAlignment(targetLanguageID, hostLanguageID))
                        return true;
                }
            }

            return false;
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, LanguageID mediaLanguageID,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID,
                        mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, List<LanguageID> mediaLanguageIDs,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                    MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID,
                        mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, LanguageID mediaLanguageID)
        {
            List<MultiLanguageItem> studyItems = StudyItemsRecurse;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, List<LanguageID> mediaLanguageIDs)
        {
            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                    MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID);
            }
        }

        public bool GetMediaInfo(string mediaRunKey, LanguageID languageID, bool recurse,
            out bool hasAudio, out bool hasVideo, out bool hasSlow, out bool hasPicture,
            List<string> audioVideoUrls)
        {
            bool hasAudioTemp;
            bool hasVideoTemp;
            bool hasSlowTemp;
            bool hasPictureTemp;
            List<MultiLanguageItem> studyItems;
            bool returnValue = false;

            hasAudio = false;
            hasVideo = false;
            hasSlow = false;
            hasPicture = false;

            if (recurse)
                studyItems = StudyItemsRecurse;
            else
                studyItems = _StudyItems;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.GetMediaInfo(mediaRunKey, languageID, studyItem.MediaTildeUrl, Node,
                            out hasAudioTemp, out hasVideoTemp, out hasSlowTemp, out hasPictureTemp, audioVideoUrls))
                        returnValue = true;

                    if (hasAudioTemp)
                        hasAudio = true;

                    if (hasVideoTemp)
                        hasVideo = true;

                    if (hasSlowTemp)
                        hasSlow = true;

                    if (hasPictureTemp)
                        hasPicture = true;
                }
            }

            return returnValue;
        }

        public bool HasMediaUrlRuns(LanguageID languageID)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasMediaUrlRun(languageID))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithUrl(string url, LanguageID languageID)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasMediaRunWithUrl(url, languageID))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithKey(object key, LanguageID languageID)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasMediaRunWithKey(key, languageID))
                        return true;
                }
            }

            return false;
        }

        public void GetMediaRunsWithReferenceKeys(List<MediaRun> mediaRuns,
            LanguageID mediaLanguageID, string mediaItemKey, string languageMediaItemKey)
        {
            if (_StudyItems == null)
                return;

            foreach (MultiLanguageItem studyItem in _StudyItems)
                studyItem.GetMediaRunsWithReferenceKeys(mediaRuns, mediaLanguageID, mediaItemKey, languageMediaItemKey);
        }

        public void DeleteMediaRunsWithReferenceKeys(string mediaItemKey, string languageMediaItemKey)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                    studyItem.DeleteMediaRunsWithReferenceKey(mediaItemKey, languageMediaItemKey);
            }
        }

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        // Returns true if any media found.
        public bool CollectMediaUrls(string mediaRunKey, List<string> mediaUrls,
            VisitMedia visitFunction, LanguageID mediaLanguageID)
        {
            bool returnValue = false;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.CollectMediaUrls(mediaRunKey, studyItem.MediaTildeUrl, Node,
                            Content, mediaUrls, visitFunction, mediaLanguageID))
                        returnValue = true;
                }
            }

            return returnValue;
        }

        public void CollectMediaFiles(Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    bool useIt = true;

                    if (itemSelectFlags != null)
                    {
                        if (!itemSelectFlags.TryGetValue(studyItem.KeyString, out useIt))
                            useIt = true;
                    }

                    if (useIt)
                        studyItem.CollectMediaFiles(studyItem.MediaTildeUrl, languageSelectFlags,
                            mediaFiles, Content, visitFunction);
                }
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            CollectMediaFiles(itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
        }

        public override bool CopyMedia(string targetDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            bool returnValue = true;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    string mediaTildeUrl = studyItem.MediaTildeUrl;
                    string sourceMediaDirectory = ApplicationData.MapToFilePath(mediaTildeUrl);

                    if (!studyItem.CopyMedia(sourceMediaDirectory, targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public override void ConvertToReference(BaseContentStorage sourceContentStorage)
        {
            base.ConvertToReference(sourceContentStorage);

            ContentStudyList sourceStudyList = sourceContentStorage as ContentStudyList;

            if (sourceStudyList == null)
                return;

            if (_StudyItems == null)
                return;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                MultiLanguageItem sourceStudyItem = sourceStudyList.GetStudyItem(studyItem.Key);

                if (sourceStudyItem == null)
                    continue;

                MultiLanguageItemReference sourceStudyItemReference = new MultiLanguageItemReference(
                    studyItem.Key, sourceStudyItem.Key, sourceContentStorage.Key, sourceContentStorage.Content.KeyString,
                    sourceContentStorage.Node.Key, sourceContentStorage.ReferenceTreeKey,
                    sourceContentStorage.Node, sourceContentStorage.Tree, sourceStudyItem);

                studyItem.ItemSource = sourceStudyItemReference;
            }
        }

        public override void SaveToReference()
        {
            base.SaveToReference();

            if (_StudyItems == null)
                return;

            foreach (MultiLanguageItem studyItem in _StudyItems)
            {
                if (studyItem.HasItemSourceItem && !studyItem.ItemSourceItem.Modified && studyItem.Modified)
                    studyItem.SaveToReference();
            }
        }

        public bool HasAnnotations(bool recurse = false)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasAnnotations())
                        return true;
                }
            }

            if (recurse && (_Content != null) && _Content.HasContentChildren())
            {
                foreach (BaseObjectContent childContent in _Content.ContentChildren)
                {
                    if (childContent.KeyString == _Content.KeyString)
                        continue;

                    ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                    if (childStudyList != null)
                    {
                        if (childStudyList.HasAnnotations(recurse))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool HasAnnotation(string type, bool recurse = false)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasAnnotation(type))
                        return true;
                }
            }

            if (recurse && (_Content != null) && _Content.HasContentChildren())
            {
                foreach (BaseObjectContent childContent in _Content.ContentChildren)
                {
                    if (childContent.KeyString == _Content.KeyString)
                        continue;

                    ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                    if (childStudyList != null)
                    {
                        if (childStudyList.HasAnnotation(type, recurse))
                            return true;
                    }
                }
            }

            return false;
        }

        public List<List<Annotation>> GetAnnotationLists(bool addChildTitles, bool recurse, LanguageID uiLanguageID)
        {
            List<MultiLanguageItem> studyItems;
            List<List<Annotation>> annotationLists;

            if (recurse)
                studyItems = StudyItemsRecurse;
            else
                studyItems = _StudyItems;

            annotationLists = MultiLanguageItem.GetAnnotationLists(studyItems, addChildTitles, uiLanguageID, this);

            return annotationLists;
        }

        public int AnnotationStudyItemIndex(string type)
        {
            if (_StudyItems != null)
            {
                int index = 0;

                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem.HasAnnotation(type))
                        return index;

                    index++;
                }
            }

            return -1;
        }

        public int GetTaggedStudyItemIndex(string tag)
        {
            if (_StudyItems != null)
            {
                int index = 0;

                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    Annotation annotation = studyItem.FindAnnotation("Tag");

                    if ((annotation != null) && (annotation.Type == "Tag") && (annotation.Value == tag))
                        return index;

                    index++;
                }
            }

            return -1;
        }

        public bool GetTaggedStudyItemRange(string tag, out int startIndex, out int endIndex)
        {
            startIndex = endIndex = -1;

            if (_StudyItems != null)
            {
                int index = 0;

                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    Annotation annotation = studyItem.FindAnnotation("Tag");

                    if ((annotation != null) && (annotation.Type == "Tag"))
                    {
                        if (startIndex == -1)
                        {
                            if (annotation.Value == tag)
                                startIndex = index;
                        }
                        else if (endIndex == -1)
                        {
                            if (annotation.Value != tag)
                            {
                                endIndex = index;
                                return true;
                            }
                        }
                    }

                    index++;
                }

                if (startIndex != -1)
                {
                    if (endIndex == -1)
                        endIndex = _StudyItems.Count();
                }
            }

            return (startIndex != -1 ? true : false);
        }

        public List<MultiLanguageItem> GetTaggedStudyItems(string tag, bool recurse)
        {
            List<MultiLanguageItem> studyItems = new List<MultiLanguageItem>();
            CollectTaggedStudyItems(tag, studyItems, recurse);
            return studyItems;
        }

        void CollectTaggedStudyItems(string tag, List<MultiLanguageItem> studyItems, bool recurse)
        {
            if (_StudyItems != null)
            {
                string currentTag = null;

                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    Annotation annotation = studyItem.FindAnnotation("Tag");

                    if (annotation != null)
                        currentTag = annotation.Value;

                    if (currentTag == tag)
                        studyItems.Add(studyItem);
                }
            }

            if (recurse)
            {
                BaseObjectContent content = Content;

                if (content != null)
                {
                    BaseObjectNode node = content.Node;
                    string contentKey = content.KeyString;

                    if (node != null)
                    {
                        if (node.HasChildren())
                        {
                            foreach (BaseObjectNode childNode in node.Children)
                            {
                                BaseObjectContent childContent = childNode.GetContent(contentKey);

                                if (childContent != null)
                                {
                                    ContentStudyList childStudyList = childContent.ContentStorageStudyList;

                                    if (childStudyList != null)
                                        childStudyList.CollectTaggedStudyItems(tag, studyItems, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool GetLabeledStudyItemRange(string label, LanguageID languageID, out int startIndex, out int endIndex)
        {
            startIndex = endIndex = -1;

            if (_StudyItems != null)
            {
                int index = 0;

                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    Annotation annotation = studyItem.FindAnnotation("Label");

                    if ((annotation != null) && (annotation.Type == "Label"))
                    {
                        if (startIndex == -1)
                        {
                            if (annotation.GetTextString(languageID) == label)
                                startIndex = index;
                        }
                        else if (endIndex == -1)
                        {
                            if (annotation.GetTextString(languageID) != label)
                            {
                                endIndex = index;
                                return true;
                            }
                        }
                    }

                    index++;
                }

                if (startIndex != -1)
                {
                    if (endIndex == -1)
                        endIndex = _StudyItems.Count();
                }
            }

            return (startIndex != -1 ? true : false);
        }

        public List<string> CollectAnnotationValues(string annotationType)
        {
            List<string> returnValue = new List<string>();

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    Annotation annotation = studyItem.FindAnnotation(annotationType);

                    if (annotation != null)
                        returnValue.Add(annotation.Value);
                }
            }

            return returnValue;
        }

        public bool HasSpeakerName(string key)
        {
            if (key == null)
                key = "";

            if (_SpeakerNames != null)
                return _SpeakerNames.FirstOrDefault(x => x.KeyString == key) != null;

            return false;
        }

        public MultiLanguageString GetSpeakerName(string key)
        {
            if (key == null)
                key = "";

            if (_SpeakerNames != null)
                return _SpeakerNames.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public MultiLanguageString FindSpeakerName(string name, LanguageID languageID)
        {
            if (name == null)
                name = "";

            if (_SpeakerNames != null)
            {
                if (languageID != null)
                    return _SpeakerNames.FirstOrDefault(x => x.Text(languageID) == name);
                else
                {
                    foreach (MultiLanguageString mls in _SpeakerNames)
                    {
                        foreach (LanguageString ls in mls.LanguageStrings)
                        {
                            if (ls.Text == name)
                                return mls;
                        }
                    }
                }
            }

            return null;
        }

        public MultiLanguageString GetSpeakerNameIndexed(int index)
        {
            if ((_SpeakerNames != null) && (index >= 0) && (index < _SpeakerNames.Count()))
                return _SpeakerNames[index];

            return null;
        }

        public string GetSpeakerNameText(string key, LanguageID languageID)
        {
            MultiLanguageString speakerName = GetSpeakerName(key);

            if (speakerName != null)
                return speakerName.Text(languageID);

            return "";
        }

        public int GetSpeakerNameIndex(string key)
        {
            if ((_SpeakerNames != null) && (key != null))
                return _SpeakerNames.IndexOf(GetSpeakerName(key));

            return -1;
        }

        public List<MultiLanguageString> CloneSpeakerNames()
        {
            if (_SpeakerNames == null)
                return null;

            List<MultiLanguageString> returnValue = new List<MultiLanguageString>(_SpeakerNames.Count());

            foreach (MultiLanguageString multiLanguageItem in _SpeakerNames)
                returnValue.Add(new MultiLanguageString(multiLanguageItem));

            return returnValue;
        }

        public bool InsertSpeakerNameIndexed(int index, MultiLanguageString speakerName)
        {
            if (_SpeakerNames == null)
                _SpeakerNames = new List<MultiLanguageString>(1) { speakerName };
            else if ((index >= 0) && (index <= _SpeakerNames.Count()))
                _SpeakerNames.Insert(index, speakerName);
            else
                return false;

            ModifiedFlag = true;

            return true;
        }

        public bool InsertSpeakerNamesIndexed(int index, List<MultiLanguageString> speakerNames)
        {
            if (speakerNames == null)
                return true;

            if (_SpeakerNames == null)
                _SpeakerNames = new List<MultiLanguageString>(speakerNames);
            else if ((index >= 0) && (index <= _SpeakerNames.Count()))
                _SpeakerNames.InsertRange(index, speakerNames);
            else
                return false;

            ModifiedFlag = true;

            return true;
        }

        public bool AddSpeakerName(MultiLanguageString speakerName)
        {
            if (_SpeakerNames == null)
                _SpeakerNames = new List<MultiLanguageString>(1) { speakerName };
            else
                _SpeakerNames.Add(speakerName);

            ModifiedFlag = true;

            return true;
        }

        public bool AddSpeakerNames(List<MultiLanguageString> speakerNames)
        {
            if (_SpeakerNames == null)
                _SpeakerNames = new List<MultiLanguageString>(speakerNames);
            else
                _SpeakerNames.AddRange(speakerNames);

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteSpeakerName(MultiLanguageString speakerName)
        {
            if (_SpeakerNames != null)
            {
                if (_SpeakerNames.Remove(speakerName))
                {
                    ModifiedFlag = true;
                    return true;
                }

                if (_StudyItems != null)
                {
                    int count = _StudyItems.Count();
                    for (int index = 0; index < count; index++)
                    {
                        MultiLanguageItem studyItem = _StudyItems[index];
                        if (studyItem.SpeakerNameKey == speakerName.KeyString)
                            studyItem.SpeakerNameKey = null;
                    }
                }
            }
            return false;
        }

        public bool DeleteSpeakerNameIndexed(int index)
        {
            MultiLanguageString speakerName = GetSpeakerNameIndexed(index);

            if (speakerName != null)
                return DeleteSpeakerName(speakerName);

            return false;
        }

        public void DeleteAllSpeakerNames()
        {
            if (_SpeakerNames != null)
                ModifiedFlag = true;
            _SpeakerNames = null;
            if (_StudyItems != null)
            {
                int count = _StudyItems.Count();
                for (int index = 0; index < count; index++)
                {
                    MultiLanguageItem studyItem = _StudyItems[index];
                    studyItem.SpeakerNameKey = null;
                }
            }
        }

        public bool CopySpeakerNamesSelected(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            int speakerNameCount = StudyItemCount();
            MultiLanguageString speakerName;
            bool returnValue = true;

            for (int speakerNameIndex = 0; speakerNameIndex < speakerNameCount; speakerNameIndex++)
            {
                if (!itemSelectFlags[speakerNameIndex])
                    continue;

                speakerName = GetSpeakerNameIndexed(speakerNameIndex);

                if (speakerName == null)
                {
                    returnValue = false;
                    continue;
                }

                speakerNames.Add(speakerName);
            }

            return returnValue;
        }

        public bool CutSpeakerNamesSelected(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            int speakerNameCount = SpeakerNameCount();
            MultiLanguageString speakerName;
            bool returnValue = true;

            for (int speakerNameIndex = 0; speakerNameIndex < speakerNameCount; speakerNameIndex++)
            {
                if (!itemSelectFlags[speakerNameIndex])
                    continue;

                speakerName = GetSpeakerNameIndexed(speakerNameIndex);

                if (speakerName == null)
                {
                    returnValue = false;
                    continue;
                }

                speakerNames.Add(speakerName);

                _SpeakerNames.RemoveAt(speakerNameIndex);
                ContentUtilities.DeleteSelectFlags(itemSelectFlags, speakerNameIndex, 1);
                speakerNameCount--;
                speakerNameIndex--;
                ModifiedFlag = true;
            }

            return returnValue;
        }

        public bool PasteSpeakerNamesPrepend(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            ContentUtilities.InsertSelectFlags(itemSelectFlags, 0, speakerNames.Count(), true);
            RekeyPasteSpeakerNames(speakerNames);
            return InsertSpeakerNamesIndexed(0, speakerNames);
        }

        public bool PasteSpeakerNamesInsertBefore(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            int index = 0;

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.IndexOf(true);

                if (index < 0)
                    index = 0;
            }

            ContentUtilities.InsertSelectFlags(itemSelectFlags, index, speakerNames.Count(), true);
            RekeyPasteSpeakerNames(speakerNames);

            return InsertSpeakerNamesIndexed(index, speakerNames);
        }

        public bool PasteSpeakerNamesOverwrite(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            int speakerNameIndex;
            int speakerNameCount = SpeakerNameCount();
            int sourceIndex = 0;
            MultiLanguageString speakerName;
            string key;
            bool returnValue = true;

            if ((speakerNames == null) || (speakerNames.Count() == 0))
                return true;

            for (speakerNameIndex = 0; speakerNameIndex < speakerNameCount; speakerNameIndex++)
            {
                if (!itemSelectFlags[speakerNameIndex])
                    continue;

                if (sourceIndex < speakerNames.Count())
                    speakerName = speakerNames[sourceIndex];
                else
                    speakerName = null;

                if (speakerName != null)
                {
                    key = AllocateStudyItemKey();
                    speakerName.SetKeys(key);
                    _SpeakerNames[speakerNameIndex] = speakerName;
                }
                else
                {
                    _SpeakerNames.RemoveAt(speakerNameIndex);
                    ContentUtilities.DeleteSelectFlags(itemSelectFlags, speakerNameIndex, 1);
                    speakerNameCount--;
                    speakerNameIndex--;
                }

                sourceIndex++;
                ModifiedFlag = true;
            }

            speakerNameIndex = itemSelectFlags.LastIndexOf(true);

            if (speakerNameIndex < 0)
                speakerNameIndex = SpeakerNameCount();
            else
                speakerNameIndex++;

            for (; sourceIndex < speakerNames.Count(); sourceIndex++)
            {
                speakerName = speakerNames[sourceIndex];
                key = AllocateStudyItemKey();
                speakerName.SetKeys(key);
                InsertSpeakerNameIndexed(speakerNameIndex, speakerName);
                ContentUtilities.InsertSelectFlags(itemSelectFlags, speakerNameIndex, 1, true);
                speakerNameIndex++;
            }

            return returnValue;
        }

        public bool PasteSpeakerNamesInsertAfter(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            int index = SpeakerNameCount();

            if (itemSelectFlags != null)
            {
                index = itemSelectFlags.LastIndexOf(true);

                if (index < 0)
                    index = SpeakerNameCount();
                else
                    index++;
            }

            ContentUtilities.InsertSelectFlags(itemSelectFlags, index, speakerNames.Count(), true);
            RekeyPasteSpeakerNames(speakerNames);

            return InsertSpeakerNamesIndexed(index, speakerNames);
        }

        public bool PasteSpeakerNamesAppend(List<MultiLanguageString> speakerNames, List<bool> itemSelectFlags)
        {
            bool returnValue;
            ContentUtilities.InsertSelectFlags(itemSelectFlags, SpeakerNameCount(), speakerNames.Count(), true);
            RekeyPasteSpeakerNames(speakerNames);
            returnValue = AddSpeakerNames(speakerNames);
            return returnValue;
        }

        public void CopySpeakerNamesFromStudyItems(List<MultiLanguageItem> studyItems)
        {
            foreach (MultiLanguageItem studyItem in studyItems)
            {
                if (!studyItem.HasSpeakerNameKey)
                    continue;

                string speakerNameKey = studyItem.SpeakerNameKey;

                if (!HasSpeakerName(speakerNameKey))
                {
                    MultiLanguageString speakerName = studyItem.SpeakerName;
                    AddSpeakerName(speakerName);
                }
            }
        }

        public void RekeyPasteSpeakerNames(List<MultiLanguageString> speakerNames)
        {
            int speakerNameCount = speakerNames.Count();
            MultiLanguageString speakerName;

            for (int speakerNameIndex = 0; speakerNameIndex < speakerNameCount; speakerNameIndex++)
            {
                speakerName = GetSpeakerNameIndexed(speakerNameIndex);

                if (speakerName == null)
                    continue;

                string key = AllocateStudyItemKey();
                speakerName.SetKeys(key);
            }
        }

        public int SpeakerNameCount()
        {
            if (_SpeakerNames != null)
                return (_SpeakerNames.Count());
            return 0;
        }

        public int StudyItemOrdinal
        {
            get
            {
                if (_StudyItems != null)
                {
                    if (_StudyItemOrdinal < _StudyItems.Count())
                        StudyItemOrdinal = _StudyItems.Count();
                }
                return _StudyItemOrdinal;
            }
            set
            {
                if (value != _StudyItemOrdinal)
                {
                    _StudyItemOrdinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int GetAndIncrementStudyItemOrdinal()
        {
            int value = StudyItemOrdinal;
            StudyItemOrdinal = value + 1;
            return value;
        }

        public string AllocateStudyItemKey()
        {
            int ordinal = GetAndIncrementStudyItemOrdinal();
            string value = "I" + ordinal.ToString();
            return value;
        }

        public ContentStudyList StudyListSource
        {
            get
            {
                return _StudyListSource;
            }
            set
            {
                if (_StudyListSource != value)
                {
                    _StudyListSource = value;

                    if (_StudyListSource != null)
                        _ReferenceSourceKey = _StudyListSource.Key;
                    else
                        _ReferenceSourceKey = null;

                    ModifiedFlag = true;
                }
            }
        }

        public override BaseContentStorage ReferenceSource
        {
            get
            {
                return StudyListSource;
            }
            set
            {
                StudyListSource = value as ContentStudyList;
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_SpeakerNames != null)
                {
                    foreach (MultiLanguageString speakerName in _SpeakerNames)
                    {
                        if ((speakerName != null) && speakerName.Modified)
                            return true;
                    }
                }

                if (_StudyItems != null)
                {
                    foreach (MultiLanguageItem studyItem in _StudyItems)
                    {
                        if ((studyItem != null) && studyItem.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_SpeakerNames != null)
                {
                    foreach (MultiLanguageString speakerName in _SpeakerNames)
                    {
                        if (speakerName != null)
                            speakerName.Modified = false;
                    }
                }

                if (_StudyItems != null)
                {
                    foreach (MultiLanguageItem studyItem in _StudyItems)
                    {
                        if (studyItem != null)
                            studyItem.Modified = false;
                    }
                }
            }
        }

        public static List<OptionDescriptor> GetDefaultDescriptors(string contentType, string contentSubType,
            UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = null;
            string collectDefault;
            string warnIfEmpty;
            string otherTeachersCanEdit = "Inherit";

            switch (contentType)
            {
                case "Transcript":
                case "Notes":
                case "Comments":
                    collectDefault = "false";
                    break;
                default:
                    collectDefault = "true";
                    break;
            }

            switch (contentType)
            {
                case "Audio":
                case "Video":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                    warnIfEmpty = "true";
                    break;
                case "Automated":
                case "Notes":
                case "Comments":
                case "Document":
                case "Media":
                default:
                    warnIfEmpty = "false";
                    break;
            }

            switch (contentType)
            {
                case "Document":
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    break;
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("ShowTitle", "flag", "Show title",
                            "This option determines whether the normal page title should be displayed.", "true"),
                        new OptionDescriptor("ShowComponentOptions", "flag", "Show component options",
                            "This option determines whether the component options panel will be optionally displayed.", "true"),
                        new OptionDescriptor("ShowTargetLanguageSelect", "flag", "Show target and host language select",
                            "This option determines whether the target and host language drop-down menus will be displayed.", "false"),
                        new OptionDescriptor("ShowLanguageLinks", "flag", "Show language links",
                            "This option determines whether the show/hide language links will be displayed.", "true"),
                        new OptionDescriptor("StringDisplayLanguage", "namedLanguage", "String display language",
                            "This option determines string display language.", "UI"),
                        new OptionDescriptor("TargetDisplayLanguage", "namedLanguage", "Target display language",
                            "This option determines the default language label of the content items.", "Target"),
                        new OptionDescriptor("CollectDescendentItems", "flag", "Collect descendent items",
                            "This option indicates that when displaying the study items, also collect and display items from any descendent node study lists with the same key.", collectDefault),
                        new OptionDescriptor("WarnIfEmpty", "flag", "Warn if empty",
                            "This option indicates that when displaying the content in a content list, don't display a warning if the content is empty.", warnIfEmpty),
                        new OptionDescriptor("DisableStatistics", "flag", "Disable statistics",
                            "This option indicates that this content item should not calculate statistics if true.", "false"),
                        new OptionDescriptor("HideStatisticsFromParent", "flag", "Hide statistics from parent",
                            "This option indicates that this content item should hide statistics from parents if true.", "false"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                        //new OptionDescriptor("HtmlHeadings", "string", "Html headings",
                        //    "Use this to add some additional HTML headings.", "")
                    };
                    break;
                default:
                    throw new Exception("ContentStudyList.GetDefaultDescriptors: Unknown content type: " + contentType);
            }

            return newOptionDescriptors;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                    studyItem.ResolveReferences(mainRepository, recurseParents, recurseChildren);
            }

            if (_ReferenceSourceKey != null)
            {
                if (_StudyListSource == null)
                    _StudyListSource = mainRepository.StudyLists.Get(_ReferenceSourceKey);
            }
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (!studyItem.SaveReferences(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            if (_StudyListSource != null)
            {
                if (_StudyListSource.Modified)
                {
                    _StudyListSource.TouchAndClearModified();

                    if (!mainRepository.UpdateReference(_StudyListSource.Source, null, _StudyListSource))
                        returnValue = false;
                }
            }

            if (StudyListCache != null)
            {
                foreach (KeyValuePair<object, ContentStudyList> kvp in StudyListCache)
                {
                    if (ObjectUtilities.CompareObjects(kvp.Key, _ReferenceSourceKey) == 0)
                        continue;

                    ContentStudyList studyList = kvp.Value;

                    if (studyList.Modified)
                    {
                        studyList.TouchAndClearModified();

                        if (!mainRepository.UpdateReference(studyList.Source, null, studyList))
                            returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (!studyItem.UpdateReferences(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (!studyItem.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                    studyItem.ClearReferences(recurseParents, recurseChildren);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if ((_SpeakerNames != null) && (_SpeakerNames.Count() != 0))
                element.Add(new XAttribute("SpeakerNameCount", _SpeakerNames.Count().ToString()));

            if (_SpeakerNames != null)
            {
                foreach (MultiLanguageString speakerName in _SpeakerNames)
                {
                    if (speakerName != null)
                        element.Add(speakerName.GetElement("SpeakerName"));
                }
            }

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    if (studyItem != null)
                        element.Add(studyItem.GetElement("StudyItem"));
                }
            }

            if ((_StudyItems != null) && (_StudyItems.Count() != 0))
                element.Add(new XAttribute("StudyItemCount", _StudyItems.Count().ToString()));

            if (_StudyItemOrdinal != 0)
                element.Add(new XAttribute("StudyItemOrdinal", _StudyItemOrdinal));

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<string, bool> itemKeyFlags)
        {
            XElement element = base.GetElementFiltered(name, itemKeyFlags);

            if ((_SpeakerNames != null) && (_SpeakerNames.Count() != 0))
                element.Add(new XAttribute("SpeakerNameCount", _SpeakerNames.Count().ToString()));

            if (_SpeakerNames != null)
            {
                foreach (MultiLanguageString speakerName in _SpeakerNames)
                    element.Add(speakerName.GetElement("SpeakerName"));
            }

            int studyItemCount = 0;

            if (_StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _StudyItems)
                {
                    bool useIt = true;

                    if (itemKeyFlags != null)
                    {
                        if (!itemKeyFlags.TryGetValue(studyItem.CompoundStudyItemKey, out useIt))
                            useIt = true;
                    }

                    if (useIt)
                    {
                        element.Add(studyItem.GetElement("StudyItem"));
                        studyItemCount++;
                    }
                }
            }

            if (studyItemCount != 0)
                element.Add(new XAttribute("StudyItemCount", studyItemCount.ToString()));

            if (_StudyItemOrdinal != 0)
                element.Add(new XAttribute("StudyItemOrdinal", _StudyItemOrdinal));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "SpeakerNameCount":
                    _SpeakerNames = new List<MultiLanguageString>(Convert.ToInt32(attributeValue));
                    break;
                case "StudyItemCount":
                    _StudyItems = new List<MultiLanguageItem>(Convert.ToInt32(attributeValue));
                    break;
                case "StudyItemOrdinal":
                    _StudyItemOrdinal = Convert.ToInt32(attributeValue);
                    break;
                case "StudyListSourceKey":  // Legacy
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            MultiLanguageItem studyItem;
            MultiLanguageString speakerName;

            switch (childElement.Name.LocalName)
            {
                case "StudyItem":
                    studyItem = new MultiLanguageItem(childElement);
                    AddStudyItem(studyItem);
                    break;
                case "SpeakerName":
                    speakerName = new MultiLanguageString(childElement);
                    AddSpeakerName(speakerName);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ContentStudyList otherContentStudyList = other as ContentStudyList;

            if (otherContentStudyList == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = MultiLanguageString.CompareMultiLanguageStringLists(_SpeakerNames, otherContentStudyList.SpeakerNames);

            if (diff != 0)
                return diff;

            diff = MultiLanguageItem.CompareMultiLanguageItemLists(_StudyItems, otherContentStudyList.StudyItems);

            return diff;
        }

        public static int Compare(ContentStudyList object1, ContentStudyList object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(ContentStudyList object1, ContentStudyList object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public void DumpWords(List<LanguageID> languageIDs)
        {
            List<MultiLanguageItem> paragraphs = StudyItemsRecurse;

            foreach (MultiLanguageItem paragraph in paragraphs)
            {
                ApplicationData.Global.PutConsoleMessage("------");
                ApplicationData.Global.PutConsoleMessage("");
                paragraph.DumpWords(languageIDs);
            }

            ApplicationData.Global.PutConsoleMessage("------");
        }
    }
}

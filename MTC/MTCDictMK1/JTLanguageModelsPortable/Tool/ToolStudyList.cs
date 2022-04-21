// ToolStudyList.cs - Tool study list.
// Stores a list of ToolStudyItem objects.
// Copyright (c) John Thompson, 2015.  All rights reserved.
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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tool
{
    public class ToolStudyList : BaseObjectTitled
    {
        protected ToolSourceCode _ToolSource;
        protected List<ToolStudyItem> _ToolStudyItems;
        protected List<ToolStudyItem> _AllToolStudyItems;                   // Not saved.
        protected List<ToolStudyItem> _InflectionToolStudyItems;
        protected List<ToolStudyItem> _AllInflectionToolStudyItems;         // Not saved.
        protected Dictionary<string, ToolStudyList> _ToolStudyListCache;    // Not saved.
        protected string _UserRunItemKey;
        protected string _SelectionStartID;
        protected string _SelectionStopID;
        protected List<string> _UserRunItemPhrases;

        public ToolStudyList(string key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                string owner, ToolSourceCode toolSource, List<ToolStudyItem> toolStudyItems)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                null, null, owner)
        {
            _ToolSource = toolSource;
            _ToolStudyItems = toolStudyItems;
            _AllToolStudyItems = null;
            _InflectionToolStudyItems = null;
            _AllInflectionToolStudyItems = null;
            _ToolStudyListCache = null;
            _UserRunItemKey = null;
            _SelectionStartID = null;
            _SelectionStopID = null;
            _UserRunItemPhrases = null;
        }

        public ToolStudyList(
                UserRecord userRecord,
                BaseObjectContent content,
                ToolSourceCode toolSource,
                List<UserRunItem> userRunItems,
                List<LanguageDescriptor> languageDescriptors,
                ToolProfile toolProfile)
            : base(ToolUtilities.ComposeToolStudyListKey(userRecord, content, toolSource))
        {
            ClearToolStudyList();
            _ToolSource = toolSource;
            if (content != null)
                CreateToolStudyItems(
                    content.ContentStorageStudyList,
                    userRunItems,
                    languageDescriptors,
                    toolProfile);
        }

        public ToolStudyList(object key)
            : base(key)
        {
            ClearToolStudyList();
        }

        public ToolStudyList(ToolStudyList other, object key)
            : base(other, key)
        {
            Copy(other);
            Modified = false;
        }

        public ToolStudyList(ToolStudyList other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ToolStudyList(XElement element)
        {
            OnElement(element);
        }

        public ToolStudyList()
        {
            ClearToolStudyList();
        }

        public void Copy(ToolStudyList other)
        {
            _ToolSource = other.ToolSource;
            _AllToolStudyItems = null;
            _InflectionToolStudyItems = null;
            _AllInflectionToolStudyItems = null;
            _ToolStudyListCache = null;
            _UserRunItemKey = other.UserRunItemKey;
            _SelectionStartID = other._SelectionStartID;
            _SelectionStopID = other._SelectionStopID;

            base.Copy(other);

            if (other == null)
            {
                ClearToolStudyList();
                return;
            }

            _ToolStudyItems = CloneToolStudyItems();
            _InflectionToolStudyItems = CloneInflectionToolStudyItems();
            _UserRunItemPhrases = CloneUserRunItemPhrases();

            ModifiedFlag = true;
        }

        public void CopyDeep(ToolStudyList other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolStudyList();
        }

        public void ClearToolStudyList()
        {
            _ToolSource = ToolSourceCode.Unknown;
            _ToolStudyItems = null;
            _AllToolStudyItems = null;
            _InflectionToolStudyItems = null;
            _AllInflectionToolStudyItems = null;
            _ToolStudyListCache = null;
            _UserRunItemKey = null;
            _SelectionStartID = null;
            _SelectionStopID = null;
            _UserRunItemPhrases = null;
        }

        public override IBaseObject Clone()
        {
            return new ToolStudyList(this);
        }

        public ToolSourceCode ToolSource
        {
            get
            {
                return _ToolSource;
            }
            set
            {
                if (_ToolSource != value)
                {
                    _ToolSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<ToolStudyItem> ToolStudyItems
        {
            get
            {
                if (_AllToolStudyItems != null)
                    return _AllToolStudyItems;

                return _ToolStudyItems;
            }
            set
            {
                if (_ToolStudyItems != value)
                    ModifiedFlag = true;

                _ToolStudyItems = value;
            }
        }

        public List<ToolStudyItem> LookupToolStudyItem(Matcher matcher)
        {
            if (ToolStudyItems == null)
                return new List<ToolStudyItem>();

            IEnumerable<ToolStudyItem> lookupQuery =
                from toolStudyItem in ToolStudyItems
                where (matcher.Match(toolStudyItem))
                select toolStudyItem;

            return lookupQuery.ToList();
        }

        public ToolStudyItem FindToolStudyItem(string text, LanguageID languageID)
        {
            if (ToolStudyItems == null)
                return null;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
            {
                if (toolStudyItem.StudyItem.Text(languageID) == text)
                    return toolStudyItem;
            }

            return null;
        }

        public ToolStudyItem GetToolStudyItem(object key)
        {
            if ((ToolStudyItems != null) && (key != null))
                return ToolStudyItems.FirstOrDefault(x => x.MatchKey(key));
            return null;
        }

        public ToolStudyItem GetToolStudyItemIndexed(int index)
        {
            if ((ToolStudyItems != null) && (index >= 0) && (index < ToolStudyItems.Count()))
                return ToolStudyItems[index];
            return null;
        }

        public int GetToolStudyItemIndex(ToolStudyItem toolStudyItem)
        {
            if ((ToolStudyItems != null) && (toolStudyItem != null))
                return ToolStudyItems.IndexOf(toolStudyItem);
            return -1;
        }

        public int GetStudyItemIndex(MultiLanguageItem studyItem)
        {
            if ((ToolStudyItems != null) && (studyItem != null))
            {
                int count = ToolStudyItems.Count;
                int index;

                for (index = 0; index < count; index++)
                {
                    if (ToolStudyItems[index].StudyItem == studyItem)
                        return index;
                }
            }

            return -1;
        }

        public List<ToolStudyItem> CloneToolStudyItems()
        {
            if (ToolStudyItems == null)
                return null;

            List<ToolStudyItem> returnValue = new List<ToolStudyItem>(ToolStudyItems.Count());

            foreach (ToolStudyItem multiLanguageItem in ToolStudyItems)
                returnValue.Add(new ToolStudyItem(multiLanguageItem));

            return returnValue;
        }

        public string Text(int index, LanguageID languageID)
        {
            ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);

            if (toolStudyItem != null)
                return toolStudyItem.StudyItem.Text(languageID);

            return null;
        }

        public string TextFuzzy(int index, LanguageID languageID)
        {
            ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);

            if (toolStudyItem != null)
                return toolStudyItem.StudyItem.TextFuzzy(languageID);

            return null;
        }

        public bool AddToolStudyItem(ToolStudyItem toolStudyItem)
        {
            if (_ToolStudyItems == null)
                _ToolStudyItems = new List<ToolStudyItem>(1) { toolStudyItem };
            else
                _ToolStudyItems.Add(toolStudyItem);

            toolStudyItem.ToolStudyList = this;

            _AllToolStudyItems = null;
            ModifiedFlag = true;

            return true;
        }

        public bool AddToolStudyItems(List<ToolStudyItem> toolStudyItems)
        {
            foreach (ToolStudyItem toolStudyItem in toolStudyItems)
                toolStudyItem.ToolStudyList = this;

            if (_ToolStudyItems == null)
                _ToolStudyItems = new List<ToolStudyItem>(toolStudyItems);
            else
                _ToolStudyItems.AddRange(toolStudyItems);

            _AllToolStudyItems = null;
            ModifiedFlag = true;

            return true;
        }

        public bool InsertToolStudyItemIndexed(int index, ToolStudyItem toolStudyItem)
        {
            if (_ToolStudyItems == null)
                _ToolStudyItems = new List<ToolStudyItem>(1) { toolStudyItem };
            else if ((index >= 0) && (index <= _ToolStudyItems.Count()))
                _ToolStudyItems.Insert(index, toolStudyItem);
            else
                return false;

            toolStudyItem.ToolStudyList = this;

            _AllToolStudyItems = null;
            ModifiedFlag = true;

            return true;

        }

        public bool InsertToolStudyItemsIndexed(int index, List<ToolStudyItem> toolStudyItems)
        {
            foreach (ToolStudyItem toolStudyItem in toolStudyItems)
                toolStudyItem.ToolStudyList = this;

            if (_ToolStudyItems == null)
                _ToolStudyItems = new List<ToolStudyItem>(toolStudyItems);
            else if ((index >= 0) && (index <= _ToolStudyItems.Count()))
                _ToolStudyItems.InsertRange(index, toolStudyItems);
            else
                return false;

            _AllToolStudyItems = null;
            ModifiedFlag = true;

            return true;

        }

        public bool DeleteToolStudyItem(ToolStudyItem toolStudyItem)
        {
            _AllToolStudyItems = null;

            if (_ToolStudyItems != null)
            {
                if (_ToolStudyItems.Remove(toolStudyItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteToolStudyItemIndexed(int index)
        {
            if ((_ToolStudyItems != null) && (index >= 0) && (index < _ToolStudyItems.Count()))
            {
                _ToolStudyItems.RemoveAt(index);
                _AllToolStudyItems = null;
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllToolStudyItems()
        {
            if (_ToolStudyItems != null)
                ModifiedFlag = true;
            _ToolStudyItems = null;
            _AllToolStudyItems = null;
        }

        public int ToolStudyItemCount()
        {
            if (ToolStudyItems != null)
                return (ToolStudyItems.Count());

            return 0;
        }

        public void RekeyToolStudyItems()
        {
            if (_ToolStudyItems == null)
                return;

            int index = 0;

            foreach (ToolStudyItem toolStudyItem in _ToolStudyItems)
            {
                object key = "I" + index.ToString();
                toolStudyItem.Rekey(key);
                index++;
            }

            if (_ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void RekeyToolStudyItems(int startIndex)
        {
            if (_ToolStudyItems == null)
                return;

            int count = _ToolStudyItems.Count();
            int index = startIndex;

            for (; index < count; index++)
            {
                ToolStudyItem toolStudyItem = _ToolStudyItems[index];
                object key = "I" + index.ToString();
                toolStudyItem.Rekey(key);
            }

            if (startIndex < _ToolStudyItems.Count())
                ModifiedFlag = true;
        }

        public void CreateToolStudyItems(
            ContentStudyList studyList,
            List<UserRunItem> userRunItems,
            List<LanguageDescriptor> languageDescriptors,
            ToolProfile toolProfile)
        {
            int index = 0;

            if (((ToolSource == ToolSourceCode.VocabularyList) ||
                    (_ToolSource == ToolSourceCode.VocabularyListInflections))
                && (userRunItems != null))
            {
                foreach (UserRunItem userRunItem in userRunItems)
                {
                    MultiLanguageItem studyItem = userRunItem.GetStudyItem(languageDescriptors);
                    string toolStudyItemKey = studyItem.KeyString;
                    List<ToolItemStatus> toolItemStatuses = new List<ToolItemStatus>();
                    ToolStudyItem toolStudyItem = new ToolStudyItem(
                        toolStudyItemKey,
                        this,
                        studyItem,
                        userRunItem,
                        toolItemStatuses);
                    AddToolStudyItem(toolStudyItem);
                    index++;
                }
            }
            else if ((studyList != null) && (studyList.StudyItems != null))
            {
                foreach (MultiLanguageItem studyItem in studyList.StudyItems)
                {
                    ToolStudyItem toolStudyItem = new ToolStudyItem(studyItem.KeyString, this, studyItem, null, null);
                    AddToolStudyItem(toolStudyItem);
                    index++;
                }
            }
        }

        public ToolStudyItem AddStudyItem(MultiLanguageItem studyItem)
        {
            int index = ToolStudyItemCount();
            ToolStudyItem toolStudyItem = new ToolStudyItem(studyItem.KeyString, this, studyItem, null, null);
            AddToolStudyItem(toolStudyItem);
            return toolStudyItem;
        }

        public List<ToolStudyItem> InflectionToolStudyItems
        {
            get
            {
                if (_AllInflectionToolStudyItems != null)
                    return _AllInflectionToolStudyItems;

                return _InflectionToolStudyItems;
            }
            set
            {
                if (_InflectionToolStudyItems != value)
                    ModifiedFlag = true;

                _InflectionToolStudyItems = value;
            }
        }

        public List<ToolStudyItem> LookupInflectionToolStudyItem(Matcher matcher)
        {
            if (InflectionToolStudyItems == null)
                return new List<ToolStudyItem>();

            IEnumerable<ToolStudyItem> lookupQuery =
                from toolStudyItem in InflectionToolStudyItems
                where (matcher.Match(toolStudyItem))
                select toolStudyItem;

            return lookupQuery.ToList();
        }

        public ToolStudyItem FindInflectionToolStudyItem(string text, LanguageID languageID)
        {
            if (InflectionToolStudyItems == null)
                return null;

            foreach (ToolStudyItem toolStudyItem in InflectionToolStudyItems)
            {
                if (toolStudyItem.StudyItem.Text(languageID) == text)
                    return toolStudyItem;
            }

            return null;
        }

        public ToolStudyItem GetInflectionToolStudyItem(object key)
        {
            if ((InflectionToolStudyItems != null) && (key != null))
                return InflectionToolStudyItems.FirstOrDefault(x => x.MatchKey(key));
            return null;
        }

        public ToolStudyItem GetInflectionToolStudyItemIndexed(int index)
        {
            if ((InflectionToolStudyItems != null) && (index >= 0) && (index < InflectionToolStudyItems.Count()))
                return InflectionToolStudyItems[index];
            return null;
        }

        public int GetInflectionToolStudyItemIndex(ToolStudyItem toolStudyItem)
        {
            if ((InflectionToolStudyItems != null) && (toolStudyItem != null))
                return InflectionToolStudyItems.IndexOf(toolStudyItem);
            return -1;
        }

        public int GetInflectionStudyItemIndex(MultiLanguageItem studyItem)
        {
            if ((InflectionToolStudyItems != null) && (studyItem != null))
            {
                int count = InflectionToolStudyItems.Count;
                int index;

                for (index = 0; index < count; index++)
                {
                    if (InflectionToolStudyItems[index].StudyItem == studyItem)
                        return index;
                }
            }

            return -1;
        }

        public List<ToolStudyItem> CloneInflectionToolStudyItems()
        {
            if (InflectionToolStudyItems == null)
                return null;

            List<ToolStudyItem> returnValue = new List<ToolStudyItem>(InflectionToolStudyItems.Count());

            foreach (ToolStudyItem multiLanguageItem in InflectionToolStudyItems)
                returnValue.Add(new ToolStudyItem(multiLanguageItem));

            return returnValue;
        }

        public string InflectionText(int index, LanguageID languageID)
        {
            ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);

            if (toolStudyItem != null)
                return toolStudyItem.StudyItem.Text(languageID);

            return null;
        }

        public string InflectionTextFuzzy(int index, LanguageID languageID)
        {
            ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);

            if (toolStudyItem != null)
                return toolStudyItem.StudyItem.TextFuzzy(languageID);

            return null;
        }

        public bool AddInflectionToolStudyItem(ToolStudyItem toolStudyItem)
        {
            if (_InflectionToolStudyItems == null)
                _InflectionToolStudyItems = new List<ToolStudyItem>(1) { toolStudyItem };
            else
                _InflectionToolStudyItems.Add(toolStudyItem);

            toolStudyItem.ToolStudyList = this;

            _AllInflectionToolStudyItems = null;
            ModifiedFlag = true;

            return true;
        }

        public bool AddInflectionToolStudyItems(List<ToolStudyItem> toolStudyItems)
        {
            foreach (ToolStudyItem toolStudyItem in toolStudyItems)
                toolStudyItem.ToolStudyList = this;

            if (_InflectionToolStudyItems == null)
                _InflectionToolStudyItems = new List<ToolStudyItem>(toolStudyItems);
            else
                _InflectionToolStudyItems.AddRange(toolStudyItems);

            _AllInflectionToolStudyItems = null;
            ModifiedFlag = true;

            return true;
        }

        public bool InsertInflectionToolStudyItemIndexed(int index, ToolStudyItem toolStudyItem)
        {
            if (_InflectionToolStudyItems == null)
                _InflectionToolStudyItems = new List<ToolStudyItem>(1) { toolStudyItem };
            else if ((index >= 0) && (index <= _InflectionToolStudyItems.Count()))
                _InflectionToolStudyItems.Insert(index, toolStudyItem);
            else
                return false;

            toolStudyItem.ToolStudyList = this;

            _AllInflectionToolStudyItems = null;
            ModifiedFlag = true;

            return true;

        }

        public bool InsertInflectionToolStudyItemsIndexed(int index, List<ToolStudyItem> toolStudyItems)
        {
            foreach (ToolStudyItem toolStudyItem in toolStudyItems)
                toolStudyItem.ToolStudyList = this;

            if (_InflectionToolStudyItems == null)
                _InflectionToolStudyItems = new List<ToolStudyItem>(toolStudyItems);
            else if ((index >= 0) && (index <= _InflectionToolStudyItems.Count()))
                _InflectionToolStudyItems.InsertRange(index, toolStudyItems);
            else
                return false;

            _AllInflectionToolStudyItems = null;
            ModifiedFlag = true;

            return true;

        }

        public bool DeleteInflectionToolStudyItem(ToolStudyItem toolStudyItem)
        {
            _AllInflectionToolStudyItems = null;

            if (_InflectionToolStudyItems != null)
            {
                if (_InflectionToolStudyItems.Remove(toolStudyItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteInflectionToolStudyItemIndexed(int index)
        {
            if ((_InflectionToolStudyItems != null) && (index >= 0) && (index < _InflectionToolStudyItems.Count()))
            {
                _InflectionToolStudyItems.RemoveAt(index);
                _AllInflectionToolStudyItems = null;
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllInflectionToolStudyItems()
        {
            if (_InflectionToolStudyItems != null)
                ModifiedFlag = true;
            _InflectionToolStudyItems = null;
            _AllInflectionToolStudyItems = null;
        }

        public int ToolInflectionStudyItemCount()
        {
            if (InflectionToolStudyItems != null)
                return (InflectionToolStudyItems.Count());

            return 0;
        }

        public void RekeyInflectionToolStudyItems()
        {
            if (_InflectionToolStudyItems == null)
                return;

            int index = 0;

            foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
            {
                object key = "I" + index.ToString();
                toolStudyItem.Rekey(key);
                index++;
            }

            if (_InflectionToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void RekeyInflectionToolStudyItems(int startIndex)
        {
            if (_InflectionToolStudyItems == null)
                return;

            int count = _InflectionToolStudyItems.Count();
            int index = startIndex;

            for (; index < count; index++)
            {
                ToolStudyItem toolStudyItem = _InflectionToolStudyItems[index];
                object key = "I" + index.ToString();
                toolStudyItem.Rekey(key);
            }

            if (startIndex < _InflectionToolStudyItems.Count())
                ModifiedFlag = true;
        }

        public void ForgetAll()
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.Forget();

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.Forget();
            }

            if (_ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void ForgetAll(object configurationKey)
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.Forget(configurationKey);

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.Forget(configurationKey);
            }

            if (ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void ForgetLearned()
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.ForgetLearned();

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.ForgetLearned();
            }

            if (_ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void ForgetLearned(object configurationKey)
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.ForgetLearned(configurationKey);

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.ForgetLearned(configurationKey);
            }

            if (ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void LearnedAll()
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.Learned();

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.Learned();
            }

            if (_ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public void LearnedAll(object configurationKey)
        {
            if (ToolStudyItems == null)
                return;

            foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                toolStudyItem.Learned(configurationKey);

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    toolStudyItem.Learned(configurationKey);
            }

            if (ToolStudyItems.Count() != 0)
                ModifiedFlag = true;
        }

        public bool ResolveToolStudyItems(
            ContentStudyList studyListHint,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            List<LanguageDescriptor> languageDescriptors,
            ToolConfiguration toolConfiguration,
            LanguageID targetLanguageID,
            ToolSession toolSession,
            NodeUtilities nodeUtilities)
        {
            IMainRepository repositories = null;
            Dictionary<int, ContentStudyList> studyListCache = null;
            bool returnValue = true;

            if (ToolStudyItems != null)
            {
                if ((_ToolSource == ToolSourceCode.VocabularyList) ||
                    (_ToolSource == ToolSourceCode.VocabularyListInflections))
                {
                    if (userRunItemDictionary == null)
                        return false;

                    bool needUpdate = false;
                    int count = ToolStudyItemCount();
                    int index;
                    int usedCount = 0;

                    for (index = count - 1; index >= 0; index--)
                    {
                        ToolStudyItem toolStudyItem = GetToolStudyItemIndexed(index);

                        if (!String.IsNullOrEmpty(toolStudyItem.ContentStudyItemKey))
                        {
                            UserRunItem userRunItem = null;

                            if (userRunItemDictionary.TryGetValue(
                                    toolStudyItem.ContentStudyItemKey,
                                    out userRunItem))
                            {
                                if (SyncStudyItem(
                                        toolStudyItem,
                                        userRunItem,
                                        languageDescriptors,
                                        toolConfiguration,
                                        targetLanguageID,
                                        toolSession))
                                    needUpdate = true;
                                usedCount++;
                            }
                            else
                            {
                                DeleteToolStudyItemIndexed(index);
                                needUpdate = true;
                            }
                        }
                    }

                    if (usedCount < userRunItemList.Count())
                    {
                        int studyItemIndex = ToolStudyItemCount();

                        foreach (UserRunItem userRunItem in userRunItemList)
                        {
                            if (GetToolStudyItem(userRunItem.Key) == null)
                            {
                                MultiLanguageItem studyItem = userRunItem.GetStudyItem(languageDescriptors);
                                ToolStudyItem toolStudyItem = new ToolStudyItem(studyItem.KeyString, this, studyItem, userRunItem, null);
                                SyncStudyItem(
                                    toolStudyItem,
                                    userRunItem,
                                    languageDescriptors,
                                    toolConfiguration,
                                    targetLanguageID,
                                    toolSession);
                                AddToolStudyItem(toolStudyItem);
                                needUpdate = true;
                                studyItemIndex++;
                            }
                        }
                    }

                    if (needUpdate)
                    {
                        RekeyToolStudyItems(0);

                        if ((ToolSource == ToolSourceCode.VocabularyListInflections) &&
                            (nodeUtilities != null) && (toolSession != null))
                        {
                            _InflectionToolStudyItems = null;
                            nodeUtilities.LoadToolStudyListInflections(
                                toolSession,
                                this,
                                targetLanguageID);
                        }

                        if (repositories == null)
                            repositories = ApplicationData.Repositories;

                        TouchAndClearModified();

                        if (!repositories.ToolStudyLists.Update(this))
                            returnValue = false;
                    }
                }
                else
                {
                    foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                    {
                        if ((toolStudyItem.StudyItem == null)
                            && (toolStudyItem.ContentStudyListKey != 0)
                            && !String.IsNullOrEmpty(toolStudyItem.ContentStudyItemKey))
                        {
                            ContentStudyList studyList = null;

                            if ((studyListHint != null) && (toolStudyItem.ContentStudyListKey == studyListHint.KeyInt))
                                studyList = studyListHint;
                            else
                            {
                                if (studyListCache == null)
                                {
                                    studyListCache = new Dictionary<int, ContentStudyList>();

                                    if (studyListHint != null)
                                        studyListCache.Add(studyListHint.KeyInt, studyListHint);
                                }

                                if (!studyListCache.TryGetValue(toolStudyItem.ContentStudyListKey, out studyList))
                                {
                                    if (repositories == null)
                                        repositories = ApplicationData.Repositories;

                                    studyList = repositories.StudyLists.Get(toolStudyItem.ContentStudyListKey);

                                    if (studyList != null)
                                    {
                                        if (studyListHint != null)
                                        {
                                            BaseObjectNodeTree tree = studyListHint.Content.Node.Tree;
                                            if (tree != null)
                                            {
                                                BaseObjectContent content = tree.GetNodeContentWithStorageKey(
                                                    toolStudyItem.ContentStudyListKey, ContentClassType.StudyList);
                                                studyList.Content = content;
                                            }
                                        }
                                        studyListCache.Add(studyList.KeyInt, studyList);
                                    }
                                    else
                                        returnValue = false;
                                }
                            }

                            if (studyList != null)
                            {
                                MultiLanguageItem studyItem = studyList.GetStudyItem(toolStudyItem.ContentStudyItemKey);

                                if (studyItem != null)
                                    toolStudyItem.StudyItem = studyItem;
                                else
                                    returnValue = false;
                            }
                        }
                    }

                    if ((studyListHint != null) && ((studyListCache == null) || (studyListCache.Count == 1)))
                    {
                        int count = studyListHint.StudyItemCount();
                        int toolItemCount = (_ToolStudyItems != null ? _ToolStudyItems.Count : 0);
                        int index;
                        int startIndex = -1;
                        bool needToolStudyListUpdate = false;
                        int studyListKey = studyListHint.KeyInt;

                        for (index = 0; index < count; index++)
                        {
                            MultiLanguageItem studyItem = studyListHint.GetStudyItemIndexed(index);
                            ToolStudyItem toolStudyItem = _ToolStudyItems.FirstOrDefault(
                                x => (x.KeyString == studyItem.KeyString) && (x.ContentStudyListKey == studyListKey));

                            if (toolStudyItem == null)
                            {
                                toolStudyItem = new ToolStudyItem(studyItem.KeyString, this, studyItem, null, null);
                                InsertToolStudyItemIndexed(index, toolStudyItem);
                                needToolStudyListUpdate = true;
                                toolItemCount++;

                                if (startIndex == -1)
                                    startIndex = index;
                            }
                        }

                        if (toolItemCount > count)
                        {
                            for (index = toolItemCount - 1; index >= 0; index--)
                            {
                                ToolStudyItem toolStudyItem = _ToolStudyItems[index];

                                if (toolStudyItem.StudyItem == null)
                                {
                                    _ToolStudyItems.RemoveAt(index);
                                    needToolStudyListUpdate = true;
                                    toolItemCount--;
                                    startIndex = index;
                                }
                            }
                        }

                        if (needToolStudyListUpdate)
                        {
                            RekeyToolStudyItems(startIndex);

                            if ((ToolSource == ToolSourceCode.StudyListInflections) &&
                                (nodeUtilities != null) && (toolSession != null))
                            {
                                _InflectionToolStudyItems = null;
                                nodeUtilities.LoadToolStudyListInflections(
                                    toolSession,
                                    this,
                                    targetLanguageID);
                            }

                            if (repositories == null)
                                repositories = ApplicationData.Repositories;

                            TouchAndClearModified();

                            if (!repositories.ToolStudyLists.Update(this))
                                returnValue = false;
                        }
                    }
                }
            }

            if (InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in InflectionToolStudyItems)
                {
                    int studyListKey = toolStudyItem.ContentStudyListKey;
                    string studyItemKey = toolStudyItem.ContentStudyItemKey;
                    toolStudyItem.StudyItem = toolStudyItem.InflectionStudyItem;
                    toolStudyItem.ContentStudyListKey = studyListKey;
                    toolStudyItem.ContentStudyItemKey = studyItemKey;

                    BaseObjectContent content = null;
                    ContentStudyList studyList = null;

                    if (studyListHint != null)
                    {
                        if (studyListHint.CompareKey(studyListKey) == 0)
                        {
                            studyList = studyListHint;
                            content = studyList.Content;
                        }
                        else if (studyListCache != null)
                        {
                            if (studyListCache.TryGetValue(studyListKey, out studyList))
                                content = studyList.Content;
                        }
                    }
                    toolStudyItem.StudyItem.Content = content;
                    toolStudyItem.Modified = false;
                }
            }

            return returnValue;
        }

        public bool SyncStudyItem(
            ToolStudyItem toolStudyItem,
            UserRunItem userRunItem,
            List<LanguageDescriptor> languageDescriptors,
            ToolConfiguration toolConfiguration,
            LanguageID targetLanguageID,
            ToolSession toolSession)
        {
            bool returnValue = false;

            if (toolStudyItem.StudyItem == null)
                toolStudyItem.StudyItem = userRunItem.GetStudyItem(languageDescriptors);

            if (toolConfiguration != null)
            {
                ToolItemStatus toolItemStatus = toolStudyItem.GetStatusNoCreate(toolConfiguration.Key);
                ToolItemStatusCode toolItemStatusCode = ToolUtilities.GetToolItemStatusCodeFromUserRunStateCode(userRunItem.UserRunState);

                if (toolItemStatus == null)
                {
                    toolItemStatus = new ToolItemStatus(
                        toolConfiguration.Key,
                        toolItemStatusCode);

                    if ((toolItemStatusCode == ToolItemStatusCode.Active) &&
                        (userRunItem.Grade > 0))
                    {
                        if ((toolSession != null) && (toolSession.ToolProfile != null))
                        {
                            DateTime nowTime = DateTime.UtcNow;

                            if (userRunItem.Grade <= 1)
                                toolItemStatus.Touch(
                                    ToolItemStatusCode.Active,
                                    nowTime,
                                    nowTime,
                                    0);
                            else
                                toolSession.ToolProfile.TouchApplyGrade(
                                    toolItemStatus,
                                    userRunItem.Grade,
                                    nowTime,
                                    toolConfiguration);
                        }
                        else
                        {
                            toolItemStatus.Grade = userRunItem.Grade;
                            toolItemStatus.TouchCount = 1;
                        }
                    }

                    toolStudyItem.SetStatus(toolItemStatus);
                    toolItemStatus.ModifiedTime = userRunItem.ModifiedTime;
                    returnValue = true;
                }
                else if (toolItemStatus.ModifiedTime < userRunItem.ModifiedTime)
                {
                    bool statusDifferent = (toolItemStatus.StatusCode != toolItemStatusCode);

                    if (statusDifferent)
                    {
                        toolItemStatus.StatusCode = toolItemStatusCode;
                        toolItemStatus.ModifiedTime = userRunItem.ModifiedTime;
                        returnValue = true;
                    }

                    if ((toolItemStatusCode == ToolItemStatusCode.Active) &&
                        ((int)toolItemStatus.Grade != userRunItem.Grade))
                    {
                        if ((toolSession != null) && (toolSession.ToolProfile != null))
                            toolSession.ToolProfile.TouchApplyGrade(
                            toolItemStatus,
                            userRunItem.Grade,
                            DateTime.UtcNow,
                            toolConfiguration);
                        else
                            toolItemStatus.ResetGrade(userRunItem.Grade);
                        toolItemStatus.ModifiedTime = userRunItem.ModifiedTime;
                        returnValue = true;
                    }
                }
                else if (toolItemStatus.ModifiedTime > userRunItem.ModifiedTime)
                {
                    UserRunStateCode userRunStatusCode = ToolUtilities.GetUserRunStateCodeFromToolItemStatusCode(toolItemStatus.StatusCode);
                    bool statusDifferent = (userRunItem.UserRunState != userRunStatusCode);
                    userRunItem.UserRunState = userRunStatusCode;

                    if (userRunStatusCode == UserRunStateCode.Active)
                    {
                        userRunItem.Grade = (int)(toolItemStatus.Grade + 0.499f);
                        userRunItem.ModifiedTime = toolItemStatus.ModifiedTime;
                    }
                    else if (statusDifferent)
                        userRunItem.ModifiedTime = toolItemStatus.ModifiedTime;

                    if (userRunItem.Modified)
                    {
                        userRunItem.Modified = false;
                        IMainRepository repositories = ApplicationData.Repositories;

                        try
                        {
                            repositories.UserRunItems.Update(userRunItem, targetLanguageID);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool GetMediaInfo(string mediaRunKey, LanguageID languageID, string mediaPathUrl, BaseObjectNode node,
            out bool hasAudio, out bool hasVideo, out bool hasSlow, out bool hasPicture,
            List<string> audioVideoUrls)
        {
            bool hasAudioTemp;
            bool hasVideoTemp;
            bool hasSlowTemp;
            bool hasPictureTemp;
            bool returnValue = false;

            hasAudio = false;
            hasVideo = false;
            hasSlow = false;
            hasPicture = false;

            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.GetMediaInfo(mediaRunKey, languageID, mediaPathUrl, node,
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
            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasMediaUrlRun(languageID))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithUrl(string url, LanguageID languageID)
        {
            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasMediaRunWithUrl(url, languageID))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithKey(object key, LanguageID languageID)
        {
            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasMediaRunWithKey(key, languageID))
                        return true;
                }
            }

            return false;
        }

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        // Returns true if any media found.
        public bool CollectMediaUrls(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
            List<string> mediaUrls, VisitMedia visitFunction, LanguageID mediaLanguageID)
        {
            bool returnValue = false;

            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.CollectMediaUrls(mediaRunKey, mediaPathUrl, node,
                        toolStudyItem.StudyItem.Content, mediaUrls, visitFunction, mediaLanguageID))
                        returnValue = true;
                }
            }

            return returnValue;
        }

        public bool HasAnnotations()
        {
            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasAnnotations())
                        return true;
                }
            }

            return false;
        }

        public bool HasAnnotation(string type)
        {
            if (ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasAnnotation(type))
                        return true;
                }
            }

            return false;
        }

        public int AnnotationToolStudyItemIndex(string type)
        {
            if (ToolStudyItems != null)
            {
                int index = 0;

                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    if (toolStudyItem.StudyItem.HasAnnotation(type))
                        return index;

                    index++;
                }
            }

            return -1;
        }

        public int GetTaggedToolStudyItemIndex(string tag)
        {
            if (ToolStudyItems != null)
            {
                int index = 0;

                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    Annotation annotation = toolStudyItem.StudyItem.FindAnnotation("Tag");

                    if ((annotation != null) && (annotation.Type == "Tag") && (annotation.Value == tag))
                        return index;

                    index++;
                }
            }

            return -1;
        }

        public bool GetTaggedToolStudyItemRange(string tag, out int startIndex, out int endIndex)
        {
            startIndex = endIndex = -1;

            if (ToolStudyItems != null)
            {
                int index = 0;

                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    Annotation annotation = toolStudyItem.StudyItem.FindAnnotation("Tag");

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
                        endIndex = ToolStudyItems.Count();
                }
            }

            return (startIndex != -1 ? true : false);
        }

        public bool GetLabeledToolStudyItemRange(string label, LanguageID languageID, out int startIndex, out int endIndex)
        {
            startIndex = endIndex = -1;

            if (ToolStudyItems != null)
            {
                int index = 0;

                foreach (ToolStudyItem toolStudyItem in ToolStudyItems)
                {
                    Annotation annotation = toolStudyItem.StudyItem.FindAnnotation("Label");

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
                        endIndex = ToolStudyItems.Count();
                }
            }

            return (startIndex != -1 ? true : false);
        }

        public List<ToolStudyItem> AllToolStudyItems
        {
            get
            {
                return _AllToolStudyItems;
            }
            set
            {
                _AllToolStudyItems = value;
            }
        }

        public int AllToolStudyItemsCount
        {
            get
            {
                if (_AllToolStudyItems != null)
                    return _AllToolStudyItems.Count();
                return 0;
            }
        }

        public List<ToolStudyItem> AllInflectionToolStudyItems
        {
            get
            {
                return _AllInflectionToolStudyItems;
            }
            set
            {
                _AllInflectionToolStudyItems = value;
            }
        }

        public Dictionary<string, ToolStudyList> ToolStudyListCache
        {
            get
            {
                return _ToolStudyListCache;
            }
            set
            {
                _ToolStudyListCache = value;
            }
        }

        public List<ToolStudyList> CachedToolStudyLists
        {
            get
            {
                List<ToolStudyList> toolStudyLists = new List<ToolStudyList>();

                if (_ToolStudyListCache != null)
                {
                    foreach (KeyValuePair<string, ToolStudyList> kvp in _ToolStudyListCache)
                        toolStudyLists.Add(kvp.Value);
                }

                return toolStudyLists;
            }
        }

        public void AddDescendentToolStudyListToCacheCheck(ToolStudyList toolStudyList)
        {
            if (_ToolStudyListCache == null)
                _ToolStudyListCache = new Dictionary<string, ToolStudyList>();

            ToolStudyList testToolStudyList;

            if (!_ToolStudyListCache.TryGetValue(toolStudyList.KeyString, out testToolStudyList))
                _ToolStudyListCache.Add(toolStudyList.KeyString, toolStudyList);

            Dictionary<string, ToolStudyList> childCache = toolStudyList.ToolStudyListCache;

            if (childCache != null)
            {
                foreach (KeyValuePair<string, ToolStudyList> kvp in childCache)
                    AddDescendentToolStudyListToCacheCheck(kvp.Value);
            }
        }

        public string UserRunItemKey
        {
            get
            {
                return _UserRunItemKey;
            }
            set
            {
                if (value != _UserRunItemKey)
                {
                    _UserRunItemKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SelectionStartID
        {
            get
            {
                return _SelectionStartID;
            }
            set
            {
                if (value != _SelectionStartID)
                {
                    _SelectionStartID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SelectionStopID
        {
            get
            {
                return _SelectionStopID;
            }
            set
            {
                if (value != _SelectionStopID)
                {
                    _SelectionStopID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> UserRunItemPhrases
        {
            get
            {
                return _UserRunItemPhrases;
            }
            set
            {
                if (value != _UserRunItemPhrases)
                {
                    _UserRunItemPhrases = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> CloneUserRunItemPhrases()
        {
            if (_UserRunItemPhrases == null)
                return null;

            List<string> userRunItemPhrases = new List<string>(_UserRunItemPhrases);

            return userRunItemPhrases;
        }

        public bool HasUserRunItemPhrase(string text)
        {
            if (_UserRunItemPhrases == null)
                return false;

            string textLower = text.ToLower();

            if (_UserRunItemPhrases.Contains(textLower))
                return true;

            return false;
        }

        public bool AddUserRunItemPhrase(string userRunItemPhrase)
        {
            string newKey = userRunItemPhrase.ToLower();

            if (_UserRunItemPhrases == null)
            {
                _UserRunItemPhrases = new List<string>() { newKey };
                ModifiedFlag = true;
                return true;
            }

            if (!_UserRunItemPhrases.Contains(newKey))
            {
                _UserRunItemPhrases.Add(newKey);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool DeleteUserRunItemPhrase(string text)
        {
            if (_UserRunItemPhrases == null)
                return false;

            string textLower = text.ToLower();

            return _UserRunItemPhrases.Remove(textLower);
        }

        public bool DeleteAllUserRunItemPhrases()
        {
            if ((_UserRunItemPhrases != null) && (_UserRunItemPhrases.Count() != 0))
            {
                _UserRunItemPhrases = null;
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void CollectStatistics(
            ContentStatistics cs,
            out int newReadyForReview,
            out int newThisSession,
            object toolConfigurationKey,
            DateTime now,
            DateTime sessionStart,
            bool isAll)
        {
            int count;
            List<ToolStudyItem> toolStudyItems;
            int index;
            ToolStudyItem toolStudyItem;
            ToolItemStatus toolItemStatus;
            int readyForReview = 0;
            int totalItems = 0;
            int futureItems = 0;
            int activeItems = 0;
            int learnedItems = 0;

            newReadyForReview = 0;
            newThisSession = 0;

            if (isAll)
            {
                count = AllToolStudyItemsCount;
                toolStudyItems = AllToolStudyItems;
            }
            else
            {
                count = ToolStudyItemCount();
                toolStudyItems = ToolStudyItems;
            }

            for (index = 0; index < count; index++)
            {
                toolStudyItem = toolStudyItems[index];

                if (toolStudyItem == null)
                    break;

                if (toolStudyItem.IsStudyItemHidden)
                    continue;

                toolItemStatus = toolStudyItem.GetStatus(toolConfigurationKey);

                if (toolItemStatus == null)
                    break;

                totalItems++;

                switch (toolItemStatus.StatusCode)
                {
                    case ToolItemStatusCode.Future:
                        futureItems++;
                        break;
                    case ToolItemStatusCode.Active:
                        activeItems++;
                        if (toolItemStatus.TouchCount != 0)
                        {
                            if (toolItemStatus.NextTouchTime <= now)
                            {
                                readyForReview++;

                                if (toolItemStatus.FirstTouchTime >= sessionStart)
                                    newReadyForReview++;
                            }

                            if (toolItemStatus.FirstTouchTime >= sessionStart)
                                newThisSession++;
                        }
                        continue;
                    case ToolItemStatusCode.Learned:
                        learnedItems++;
                        break;
                }

                cs.LastCheckTimeLocal = now;
            }

            cs.FutureCountLocal = futureItems;
            cs.DueCountLocal = readyForReview;
            cs.ActiveCountLocal = activeItems - readyForReview;
            cs.CompleteCountLocal = learnedItems;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_ToolStudyItems != null)
                {
                    foreach (ToolStudyItem toolStudyItem in _ToolStudyItems)
                    {
                        if (toolStudyItem.Modified)
                            return true;
                    }
                }

                if (_InflectionToolStudyItems != null)
                {
                    foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    {
                        if (toolStudyItem.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_ToolStudyItems != null)
                {
                    foreach (ToolStudyItem toolStudyItem in _ToolStudyItems)
                        toolStudyItem.Modified = false;
                }

                if (_InflectionToolStudyItems != null)
                {
                    foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                        toolStudyItem.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XAttribute("ToolSource", ToolUtilities.GetToolSourceStringFromCode(ToolSource)));

            if ((_ToolStudyItems != null) && (_ToolStudyItems.Count() != 0))
                element.Add(new XAttribute("ToolStudyItemCount", _ToolStudyItems.Count().ToString()));

            if (_ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _ToolStudyItems)
                    element.Add(toolStudyItem.Xml);
            }

            if (!String.IsNullOrEmpty(_UserRunItemKey))
                element.Add(new XElement("UserRunItemKey", _UserRunItemKey));

            if (!String.IsNullOrEmpty(_SelectionStartID))
                element.Add(new XElement("SelectionStartID", _SelectionStartID));

            if (!String.IsNullOrEmpty(_SelectionStopID))
                element.Add(new XElement("SelectionStopID", _SelectionStopID));

            if (_UserRunItemPhrases != null)
                element.Add(new XElement(
                    "UserRunItemPhrases",
                    ObjectUtilities.GetStringFromStringList(_UserRunItemPhrases)));

            if (_InflectionToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _InflectionToolStudyItems)
                    element.Add(toolStudyItem.GetElement("InflectionToolStudyItem"));
            }

            return element;
        }

        public virtual XElement GetElementFiltered(string name, Dictionary<string, bool> itemKeyFlags)
        {
            XElement element = base.GetElement(name);

            int studyItemCount = 0;

            element.Add(new XAttribute("ToolSource", ToolUtilities.GetToolSourceStringFromCode(ToolSource)));

            if (_ToolStudyItems != null)
            {
                foreach (ToolStudyItem toolStudyItem in _ToolStudyItems)
                {
                    bool useIt = true;

                    if ((itemKeyFlags != null) && itemKeyFlags.TryGetValue(toolStudyItem.KeyString, out useIt) && !useIt)
                        continue;

                    element.Add(toolStudyItem.GetElement("ToolStudyItem"));
                    studyItemCount++;
                }
            }

            if (studyItemCount != 0)
                element.Add(new XAttribute("ToolStudyItemCount", studyItemCount.ToString()));

            if (!String.IsNullOrEmpty(_UserRunItemKey))
                element.Add(new XElement("UserRunItemKey", _UserRunItemKey));

            if (!String.IsNullOrEmpty(_SelectionStartID))
                element.Add(new XElement("SelectionStartID", _SelectionStartID));

            if (!String.IsNullOrEmpty(_SelectionStopID))
                element.Add(new XElement("SelectionStopID", _SelectionStopID));

            if (_UserRunItemPhrases != null)
                element.Add(new XElement(
                    "UserRunItemPhrases",
                    ObjectUtilities.GetStringFromStringList(_UserRunItemPhrases)));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ToolSource":
                    _ToolSource = ToolUtilities.GetToolSourceCodeFromString(attributeValue);
                    break;
                case "ToolStudyItemCount":
                    _ToolStudyItems = new List<ToolStudyItem>(Convert.ToInt32(attributeValue));
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            ToolStudyItem toolStudyItem;

            switch (childElement.Name.LocalName)
            {
                case "ToolStudyItem":
                    toolStudyItem = new ToolStudyItem(childElement);
                    AddToolStudyItem(toolStudyItem);
                    break;
                case "UserRunItemKey":
                    UserRunItemKey = childElement.Value.ToLower().Trim();
                    break;
                case "SelectionStartID":
                    SelectionStartID = childElement.Value.ToLower().Trim();
                    break;
                case "SelectionStopID":
                    SelectionStopID = childElement.Value.ToLower().Trim();
                    break;
                case "UserRunItemPhrases":
                    UserRunItemPhrases = ObjectUtilities.GetStringListFromString(
                        childElement.Value.ToLower().Trim());
                    break;
                case "InflectionToolStudyItem":
                    toolStudyItem = new ToolStudyItem(childElement);
                    AddInflectionToolStudyItem(toolStudyItem);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolStudyList otherToolStudyList = other as ToolStudyList;

            if (otherToolStudyList == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ToolStudyItem.CompareToolStudyItemLists(_ToolStudyItems, otherToolStudyList.ToolStudyItems);

            if (diff != 0)
                return diff;

            diff = ToolStudyItem.CompareToolStudyItemLists(_InflectionToolStudyItems, otherToolStudyList.InflectionToolStudyItems);

            return diff;
        }

        public static int Compare(ToolStudyList object1, ToolStudyList object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(ToolStudyList object1, ToolStudyList object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }
    }
}

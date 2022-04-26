using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Content
{
    public class StudySentenceList
    {
        protected List<MultiLanguageItem> _StudyItems;
        protected List<int> _StudyItemSentenceIndexes;
        protected List<LanguageID> _LanguageIDs;

        public StudySentenceList(List<LanguageID> languageIDs)
        {
            ClearStudySentenceList();
            _LanguageIDs = languageIDs;
        }

        public StudySentenceList(StudySentenceList other)
        {
            CopyStudySentenceList(other);
        }

        public StudySentenceList()
        {
            ClearStudySentenceList();
        }

        public void ClearStudySentenceList()
        {
            _StudyItems = new List<MultiLanguageItem>();
            _StudyItemSentenceIndexes = new List<int>();
        }

        public void CopyStudySentenceList(StudySentenceList other)
        {
            _StudyItems = new List<MultiLanguageItem>(other._StudyItems);
            _StudyItemSentenceIndexes = new List<int>(other._StudyItemSentenceIndexes);
            _LanguageIDs = null;
        }

        public List<MultiLanguageItem> StudyItems
        {
            get
            {
                return _StudyItems;
            }
        }

        public int StudySentenceCount()
        {
            return _StudyItemSentenceIndexes.Count() / 2;
        }

        // Returns false if sentence counts are mismatched.
        public bool Add(MultiLanguageItem studyItem)
        {
            int sentenceCount = studyItem.GetMaxSentenceCount(_LanguageIDs);
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                // Add study item index.
                _StudyItemSentenceIndexes.Add(_StudyItems.Count());
                // Add sentence index.
                _StudyItemSentenceIndexes.Add(sentenceIndex);
            }

            _StudyItems.Add(studyItem);

            return !studyItem.IsSentenceMismatch(_LanguageIDs);
        }

        public bool GetStudyItemAndSentenceIndex(
            int globalSentenceIndex,
            out MultiLanguageItem studyItem,
            out int localSentenceIndex)
        {
            studyItem = null;
            localSentenceIndex = -1;

            if (globalSentenceIndex >= _StudyItemSentenceIndexes.Count() - 1)
                return false;

            int studyItemSentencePairIndex = globalSentenceIndex * 2;
            int studyItemIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex];
            studyItem = _StudyItems[studyItemIndex];
            localSentenceIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex + 1];

            return true;
        }

        public bool GetStudyItem(
            int globalSentenceIndex,
            out MultiLanguageItem studyItem)
        {
            studyItem = null;

            if (globalSentenceIndex >= _StudyItemSentenceIndexes.Count() - 1)
                return false;

            int studyItemSentencePairIndex = globalSentenceIndex * 2;
            int studyItemIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex];
            studyItem = _StudyItems[studyItemIndex];

            return true;
        }

        public bool GetStudyItemIndexed(
            int studyItemIndex,
            out MultiLanguageItem studyItem)
        {
            studyItem = null;

            if (studyItemIndex >= _StudyItems.Count())
                return false;

            studyItem = _StudyItems[studyItemIndex];

            return true;
        }

        public bool GetStudyItemLanguageItemSentenceRun(
            int globalSentenceIndex,
            LanguageID languageID,
            out MultiLanguageItem studyItem,
            out LanguageItem languageItem,
            out TextRun sentenceRun,
            out int localSentenceIndex)
        {
            studyItem = null;
            languageItem = null;
            sentenceRun = null;
            localSentenceIndex = -1;

            if (globalSentenceIndex >= _StudyItemSentenceIndexes.Count() - 1)
                return false;

            int studyItemSentencePairIndex = globalSentenceIndex * 2;
            int studyItemIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex];
            studyItem = _StudyItems[studyItemIndex];
            localSentenceIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex + 1];
            languageItem = studyItem.LanguageItem(languageID);

            if (languageItem == null)
                return false;

            sentenceRun = languageItem.GetSentenceRun(localSentenceIndex);

            if (sentenceRun == null)
                return false;

            return true;
        }

        public bool GetTargetHostStudyItemLanguageItemSentenceRun(
            int globalSentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            out MultiLanguageItem studyItem,
            out LanguageItem targetLanguageItem,
            out LanguageItem hostLanguageItem,
            out TextRun targetSentenceRun,
            out TextRun hostSentenceRun,
            out int localSentenceIndex)
        {
            studyItem = null;
            targetLanguageItem = null;
            hostLanguageItem = null;
            targetSentenceRun = null;
            hostSentenceRun = null;
            localSentenceIndex = -1;

            if (globalSentenceIndex >= _StudyItemSentenceIndexes.Count() - 1)
                return false;

            int studyItemSentencePairIndex = globalSentenceIndex * 2;
            int studyItemIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex];
            studyItem = _StudyItems[studyItemIndex];
            localSentenceIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex + 1];
            targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            if (targetLanguageItem == null)
                return false;

            if (hostLanguageItem == null)
                return false;

            targetSentenceRun = targetLanguageItem.GetSentenceRun(localSentenceIndex);
            hostSentenceRun = hostLanguageItem.GetSentenceRun(localSentenceIndex);

            if (targetSentenceRun == null)
                return false;

            if (hostSentenceRun == null)
                return false;

            return true;
        }

        public bool Delete(int globalSentenceIndex)
        {
            if (globalSentenceIndex >= _StudyItemSentenceIndexes.Count() - 1)
                return false;

            int studyItemSentencePairIndex = globalSentenceIndex * 2;
            int studyItemIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex];
            int localSentenceIndex = _StudyItemSentenceIndexes[studyItemSentencePairIndex + 1];
            MultiLanguageItem studyItem = _StudyItems[studyItemIndex];
            int sentenceCount = studyItem.GetMaxSentenceCount(_LanguageIDs);

            _StudyItems.RemoveAt(studyItemIndex);

            if (sentenceCount != 0)
                _StudyItemSentenceIndexes.RemoveRange(globalSentenceIndex * 2, sentenceCount);

            return true;
        }

        public void DumpString(StringBuilder sb)
        {
            if ((_StudyItemSentenceIndexes != null) && (_LanguageIDs != null))
            {
                int count = _StudyItemSentenceIndexes.Count();
                int index;

                for (index = 0; index < count; index += 2)
                {
                    int studyItemIndex = _StudyItemSentenceIndexes[index];
                    int globalSentenceIndex = index / 2;
                    int localSentenceIndex = _StudyItemSentenceIndexes[index + 1];
                    MultiLanguageItem studyItem = _StudyItems[studyItemIndex];

                    foreach (LanguageID languageID in _LanguageIDs)
                    {
                        LanguageItem li = studyItem.LanguageItem(languageID);

                        if (li != null)
                        {
                            string text = li.Text;

                            if (text.Length > 40)
                                text = text.Substring(0, 40) + "...";

                            sb.Append(
                                "table index=" + index.ToString()
                                + " globalSentenceIndex=" + globalSentenceIndex.ToString()
                                + " localSentenceIndex=" + localSentenceIndex
                                + " studyItem index=" + studyItemIndex.ToString()
                                + " studyItem key=" + studyItem.KeyString
                                + " language=" + languageID.LanguageCultureExtensionCode
                                + " text=" + text);
                        }
                        else
                        {
                            sb.Append("LanguageItem is null for language=" + languageID.LanguageCultureExtensionCode
                                + " studyItem index=" + studyItemIndex.ToString()
                                + " table index=" + index.ToString()
                                + " studyItem key=" + studyItem.KeyString);
                        }
                    }
                }
            }
            else
                sb.Append("Can't dump StudySentenceList. Something is null.");
        }
    }
}

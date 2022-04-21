using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class SentenceFixes : BaseObject
    {
        public Dictionary<string, SentenceFix> Fixes;
        public Dictionary<string, MultiLanguageItem> ParagraphEdits;

        public SentenceFixes(
            Dictionary<string, SentenceFix> sentenceFixes,
            Dictionary<string, MultiLanguageItem> paragraphEdits)
        {
            Fixes = sentenceFixes;
            ParagraphEdits = paragraphEdits;
        }

        public SentenceFixes(XElement element)
        {
            OnElement(element);
        }

        public SentenceFixes()
        {
            Fixes = null;
            ParagraphEdits = null;
        }

        public bool FixStudyItemCheck(
            MultiLanguageItem studyItem,
            LanguageID hostLanguageID)
        {
            if ((studyItem == null) || (studyItem.LanguageItems == null))
                return false;

            bool returnValue = false;

            returnValue = FixStudyItemSentenceCheck(studyItem) || returnValue;

            return returnValue;
        }

        public bool FixStudyItemSentenceCheck(
            MultiLanguageItem studyItem)
        {
            bool returnValue = false;

            foreach (LanguageItem languageItem in studyItem.LanguageItems)
            {
                if (languageItem == null)
                    return false;

                returnValue = FixLanguageItemCheck(languageItem) || returnValue;
            }

            return returnValue;
        }

        public bool FixLanguageItemCheck(
            LanguageItem languageItem)
        {
            bool returnValue = false;

            if (!languageItem.HasSentenceRuns())
                return false;

            if (Fixes == null)
                return false;

            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < languageItem.SentenceRunCount(); sentenceIndex++)
            {
                TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                if (sentenceRun == null)
                    continue;

                string sentence = languageItem.GetRunText(sentenceRun);

                SentenceFix sentenceFix;
                List<TextRun> newSentenceRuns = new List<TextRun>();

                if (Fixes.TryGetValue(sentence, out sentenceFix))
                {
                    if (sentenceFix.IsJoin())
                    {
                        int inputCount = sentenceFix.InputCount;
                        bool isMatch = true;

                        for (int inputIndex = 1; inputIndex < inputCount; inputIndex++)
                        {
                            TextRun testSentenceRun = languageItem.GetSentenceRun(sentenceIndex + inputIndex);

                            if (testSentenceRun == null)
                            {
                                isMatch = false;
                                break;
                            }

                            string testSentence = languageItem.GetRunText(testSentenceRun);

                            if (testSentence != sentenceFix.GetInputIndexed(inputIndex))
                            {
                                isMatch = false;
                                break;
                            }
                        }

                        if (isMatch)
                            languageItem.JoinSentenceRuns(sentenceIndex, inputCount);

                        returnValue = true;
                    }
                    else if (sentenceFix.IsSplit())
                    {
                        int startIndex = sentenceRun.Start;
                        bool isOkay = true;

                        newSentenceRuns.Clear();

                        foreach (string newSentence in sentenceFix.Output)
                        {
                            int length = newSentence.Length;
                            int foundOffset = languageItem.Text.IndexOf(newSentence, startIndex);

                            if (foundOffset != -1)
                            {
                                TextRun newSentenceRun = new TextRun(foundOffset, length, null);
                                newSentenceRuns.Add(newSentenceRun);
                                startIndex += length;
                            }
                            else
                                isOkay = false;
                        }

                        if (isOkay)
                        {
                            if (newSentenceRuns.Count() != 0)
                                newSentenceRuns[0].MediaRuns = sentenceRun.CloneMediaRuns();
                            languageItem.SentenceRuns.RemoveAt(sentenceIndex);
                            languageItem.SentenceRuns.InsertRange(sentenceIndex, newSentenceRuns);
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool FixStudyItemParagraphCheck(
            MultiLanguageItem studyItem,
            LanguageID hostLanguageID)
        {
            if (ParagraphEdits == null)
                return false;

            string studyItemPath = studyItem.GetNamePathStringInclusive(hostLanguageID);
            MultiLanguageItem paragraphEdit;
            bool returnValue = false;

            if (ParagraphEdits.TryGetValue(studyItemPath, out paragraphEdit))
            {
                if (paragraphEdit.LanguageItems == null)
                    return false;

                foreach (LanguageItem languageItemEdit in paragraphEdit.LanguageItems)
                {
                    LanguageItem languageItem = studyItem.LanguageItem(languageItemEdit.LanguageID);

                    if (languageItem == null)
                        continue;

                    EditLanguageItem(languageItem, languageItemEdit);
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public void EditLanguageItem(
            LanguageItem languageItem,
            LanguageItem languageItemEdit)
        {
            languageItem.Text = languageItemEdit.Text;
            languageItem.SentenceRuns = languageItemEdit.CloneSentenceRuns();
            languageItem.WordRuns = languageItemEdit.CloneWordRuns();
        }

        public bool AddSentenceFix(SentenceFix sentenceFix)
        {
            SentenceFix testSentenceFix;

            if (Fixes == null)
            {
                Fixes = new Dictionary<string, SentenceFix>();
                Fixes.Add(sentenceFix.KeyString, sentenceFix);
            }
            else if (Fixes.TryGetValue(sentenceFix.KeyString, out testSentenceFix))
            {
                Fixes[sentenceFix.KeyString] = sentenceFix;
                return false;
            }
            else
                Fixes.Add(sentenceFix.KeyString, sentenceFix);

            return true;
        }

        public MultiLanguageItem GetParagraphEdit(string studyItemPath)
        {
            if (ParagraphEdits == null)
                return null;

            MultiLanguageItem paragraphEdit;

            if (ParagraphEdits.TryGetValue(studyItemPath, out paragraphEdit))
                return paragraphEdit;

            return null;
        }

        public void AddParagraphEdit(string studyItemPath, MultiLanguageItem paragraphEdit)
        {
            if (ParagraphEdits == null)
                ParagraphEdits = new Dictionary<string, MultiLanguageItem>();
            if (!String.IsNullOrEmpty(paragraphEdit.KeyString))
                ParagraphEdits.Add(paragraphEdit.KeyString, paragraphEdit);
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if ((Fixes != null) && (Fixes.Count() != 0))
            {
                foreach (KeyValuePair<string, SentenceFix> kvp in Fixes)
                    element.Add(kvp.Value.GetElement("SentenceFix"));
            }

            if ((ParagraphEdits != null) && (ParagraphEdits.Count() != 0))
            {
                foreach (KeyValuePair<string, MultiLanguageItem> kvp in ParagraphEdits)
                    element.Add(kvp.Value.GetElement("ParagraphEdit"));
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "SentenceFix":
                    if (Fixes == null)
                        Fixes = new Dictionary<string, SentenceFix>();
                    SentenceFix sentenceFix = new SentenceFix(childElement);
                    if (!String.IsNullOrEmpty(sentenceFix.KeyString))
                        Fixes.Add(sentenceFix.KeyString, sentenceFix);
                    break;
                case "ParagraphEdit":
                    if (ParagraphEdits == null)
                        ParagraphEdits = new Dictionary<string, MultiLanguageItem>();
                    MultiLanguageItem paragraphEdit = new MultiLanguageItem(childElement);
                    if (!String.IsNullOrEmpty(paragraphEdit.KeyString))
                        ParagraphEdits.Add(paragraphEdit.KeyString, paragraphEdit);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static string GetFilePath(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string sentenceFixesKey = optionContainer.GetInheritedOptionValue("SentenceFixesKey");
            string sentenceFixesPath = GetFilePath(sentenceFixesKey, targetLanguageID);
            return sentenceFixesPath;
        }

        public static string GetFilePath(string key, LanguageID targetLanguageID)
        {
            string sentenceFixesName;
            if (targetLanguageID != null)
            {
                string languageSymbol = targetLanguageID.SymbolName;
                sentenceFixesName = key + "_" + languageSymbol + ".xml";
            }
            else
                sentenceFixesName = key + ".xml";
            string sentenceFixesPath = MediaUtilities.ConcatenateFilePath(ApplicationData.LocalDataPath, "SentenceFixes");
            sentenceFixesPath = MediaUtilities.ConcatenateFilePath(sentenceFixesPath, sentenceFixesName);
            return sentenceFixesPath;
        }

        public static bool CreateAndLoad(
            BaseMarkupContainer optionContainer,
            LanguageID targetLanguageID,
            LanguageID uiLanguageID,
            out SentenceFixes sentenceFixes)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return CreateAndLoad(filePath, out sentenceFixes);
        }

        public static bool CreateAndLoad(string filePath, out SentenceFixes sentenceFixes)
        {
            sentenceFixes = new SentenceFixes();
            return sentenceFixes.Load(filePath);
        }

        public bool Load(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return Load(filePath);
        }

        public bool Load(string filePath)
        {
            if (!FileSingleton.Exists(filePath))
                return false;

            try
            {
                XElement element = ApplicationData.Global.LoadXml(filePath);

                if (element != null)
                    OnElement(element);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Save(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return Save(filePath);
        }

        public bool Save(string filePath)
        {
            try
            {
                XElement element = Xml;
                FileSingleton.DirectoryExistsCheck(filePath);
                ApplicationData.Global.SaveXml(element, filePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

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
        public static bool DoCollectUserRunItemsTiming = false;
        public static string WordBreakpoint = null;

        public bool CollectUserRunItems(
            ContentStudyList studyList,
            string owner,
            string profile,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> languageIDs,
            List<LanguageDescriptor> languageDescriptors,
            BaseObjectNode node,
            BaseObjectContent content,
            bool showNew,
            bool showActive,
            bool showLearned,
            List<string> sortOrder,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList)
        {
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            LanguageID hostLanguageID = hostLanguageIDs.FirstOrDefault();
            List<UserRunItem> newUserRunItems = new List<UserRunItem>();
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            bool returnValue;

            targetLanguageIDs = new List<LanguageID>(targetLanguageIDs);
            targetLanguageIDs.Remove(hostLanguageID);

            if (DoCollectUserRunItemsTiming)
            {
                CreateTimerCheck();
                SetUpDumpStringCheck();
                TimerStart();
            }

            /*
            if ((content.ContentType == "Words") && (content.ContentSubType == "Vocabulary"))
            {
                returnValue = CollectTargetWordUserRunItems(
                    studyList, owner, profile, targetLanguageID, hostLanguageID,
                    targetLanguageIDs, null, hostLanguageIDs, languageDescriptors,
                    showNew, showActive, showLearned, sortOrder,
                    userRunItemDictionary, userRunItemList, newUserRunItems);
            }
            else
            */
            {
                Dictionary<string, UserRunItem> wordUserRunItemDictionary = null;
                List<UserRunItem> wordUserRunItemList = null;
                List<UserRunItem> newWordUserRunItems = null;
                /*
                BaseObjectContent wordContent = node.GetContentWithTypeAndSubType("Words", "Vocabulary");

                if (wordContent != null)
                {
                    ContentStudyList wordStudyList = wordContent.ContentStorageStudyList;

                    if (wordStudyList == null)
                    {
                        wordContent.ResolveReferences(Repositories, false, true);
                        wordStudyList = wordContent.ContentStorageStudyList;
                    }

                    if (wordStudyList != null)
                    {
                        wordUserRunItemDictionary = new Dictionary<string, UserRunItem>(StringComparer.OrdinalIgnoreCase);
                        wordUserRunItemList = new List<UserRunItem>();
                        newWordUserRunItems = new List<UserRunItem>();

                        CollectTargetWordUserRunItems(
                            wordStudyList, owner, profile, targetLanguageID, hostLanguageID,
                            targetLanguageIDs, null, hostLanguageIDs, languageDescriptors,
                            showNew, showActive, showLearned, sortOrder,
                            wordUserRunItemDictionary, wordUserRunItemList, newWordUserRunItems);
                    }
                }
                */

                studyList.WordRunCheckLanguages(targetLanguageIDs, Repositories.Dictionary);

                returnValue = CollectTargetUserRunItems(
                    studyList, owner, profile, targetLanguageID, hostLanguageID,
                    languageDescriptors, showNew, showActive, showLearned, sortOrder,
                    userRunItemDictionary, userRunItemList, newUserRunItems,
                    wordUserRunItemDictionary, wordUserRunItemList, newWordUserRunItems);

                FixupAlternateUserRunItems(
                    userRunItemDictionary, studyList, targetLanguageIDs);

                if ((hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0))
                {
                    int ordinal = 0;

                    foreach (UserRunItem testUserRunItem in userRunItemList)
                    {
                        if (!((testUserRunItem.MainDefinitions == null) ||
                                    (testUserRunItem.MainDefinitions.Count() == 0)) &&
                                !((testUserRunItem.OtherDefinitions == null) ||
                                    (testUserRunItem.OtherDefinitions.Count() == 0)))
                            continue;

                        GetUserRunDefinitions(
                            testUserRunItem,
                            targetLanguageIDs,
                            hostLanguageIDs,
                            languageIDs,
                            node,
                            content);

                        ordinal++;
                    }
                }
            }

            if (newUserRunItems.Count() != 0)
            {
                foreach (UserRunItem testUserRunItem in newUserRunItems)
                    testUserRunItem.TouchAndClearModified();

                try
                {
                    if (!Repositories.UserRunItems.AddList(newUserRunItems, targetLanguageID))
                    {
                        PutError("Error adding new vocabulary items.");
                        returnValue = false;
                    }
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception adding new vocabulary items", exc);
                    returnValue = false;
                }
            }

            List<UserRunItem> userRunItemsToUpdate = new List<UserRunItem>();

            foreach (UserRunItem testUserRunItem in userRunItemList)
            {
                if (testUserRunItem.Modified)
                {
                    testUserRunItem.TouchAndClearModified();
                    userRunItemsToUpdate.Add(testUserRunItem);
                }
            }

            if (userRunItemsToUpdate.Count() != 0)
            {
                try
                {
                    if (!Repositories.UserRunItems.UpdateList(userRunItemsToUpdate, targetLanguageID))
                    {
                        PutError("Error updating existing vocabulary items.");
                        returnValue = false;
                    }
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception updating existing vocabulary items", exc);
                    returnValue = false;
                }
            }

            if (targetLanguageIDs.Count() > 1)
                CollectAlternateUserRunItems(
                    userRunItemDictionary, userRunItemList, targetLanguageIDs);

            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if (tool != null)
                tool.SaveDictionaryEntriesAddedAndUpdated();

            if (!FlushRemoteDictionaryCache())
                returnValue = false;

            if (DoCollectUserRunItemsTiming)
                TimerStopAndDumpReport("CollectUserRunItems");

            return returnValue;
        }

        public bool CollectTargetWordUserRunItems(
            ContentStudyList studyList,
            string owner,
            string profile,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> alternateLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageDescriptor> languageDescriptors,
            bool showNew,
            bool showActive,
            bool showLearned,
            List<string> sortOrder,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            List<UserRunItem> newUserRunItems)
        {
            BaseObjectContent content = studyList.Content;
            BaseObjectNode node = content.NodeOrTree;
            BaseObjectNodeTree tree = content.Tree;
            string treeName = (tree != null ? tree.GetName(hostLanguageID) : String.Empty);
            string nodeName = (node != null ? node.GetName(hostLanguageID) : String.Empty);
            string contentName = (content != null ? content.GetName(hostLanguageID) : String.Empty);
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            List<string> words = new List<string>();

            foreach (MultiLanguageItem wordItem in studyItems)
            {
                string word = wordItem.Text(targetLanguageID);
                if (!String.IsNullOrEmpty(word))
                    words.Add(word);
            }

            int totalCount;
            int pageCount;

            bool returnValue = Repositories.UserRunItems.GetUserRunItems(
                words, owner, profile, targetLanguageID, languageDescriptors,
                showNew, showActive, showLearned,
                sortOrder, userRunItemDictionary, userRunItemList,
                0, 0, out pageCount, out totalCount);

            foreach (string word in words)
            {
                UserRunItem userRunItem = null;

                if (!userRunItemDictionary.TryGetValue(word.ToLower(), out userRunItem))
                {
                    MultiLanguageItem studyItem = studyItems.FirstOrDefault(
                        x => TextUtilities.IsEqualStringsIgnoreCase(x.Text(targetLanguageID), word));

                    if (studyItem == null)
                        continue;

                    List<LanguageString> alternates = null;

                    foreach (LanguageID alternateLanguageID in targetLanguageIDs)
                    {
                        if (alternateLanguageID == targetLanguageID)
                            continue;

                        if (!studyItem.HasText(alternateLanguageID))
                            continue;

                        LanguageString alternate = new LanguageString(
                            0,
                            alternateLanguageID,
                            studyItem.Text(alternateLanguageID));

                        if (alternates == null)
                            alternates = new List<LanguageString>() { alternate };
                        else
                            alternates.Add(alternate);
                    }

                    MultiLanguageString definition = new MultiLanguageString(0);

                    foreach (LanguageID languageID in hostLanguageIDs)
                    {
                        if (!studyItem.HasText(languageID))
                            continue;

                        LanguageString languageDefinition = new LanguageString(
                            0,
                            languageID,
                            studyItem.Text(languageID));

                        definition.Add(languageDefinition);
                    }

                    List<MultiLanguageString> mainDefinitions = new List<MultiLanguageString>() { definition };
                    List<MultiLanguageString> otherDefinitions = new List<MultiLanguageString>() { definition };

                    bool isPhrase = IsPhrase(word, targetLanguageID);

                    userRunItem = new UserRunItem(
                        word,
                        alternates,
                        mainDefinitions,
                        otherDefinitions,
                        UserRunStateCode.Future,
                        0,
                        owner,
                        profile,
                        treeName,
                        nodeName,
                        contentName,
                        isPhrase);

                    userRunItemDictionary.Add(word.ToLower(), userRunItem);
                    userRunItemList.Add(userRunItem);
                    newUserRunItems.Add(userRunItem);
                }
            }

            return returnValue;
        }

        public bool CollectTargetUserRunItems(
            ContentStudyList studyList,
            string owner,
            string profile,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<LanguageDescriptor> languageDescriptors,
            bool showNew,
            bool showActive,
            bool showLearned,
            List<string> sortOrder,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            List<UserRunItem> newUserRunItems,
            Dictionary<string, UserRunItem> wordUserRunItemDictionary,
            List<UserRunItem> wordUserRunItemList,
            List<UserRunItem> newWordUserRunItems)
        {
            BaseObjectContent content = studyList.Content;
            BaseObjectNode node = content.NodeOrTree;
            BaseObjectNodeTree tree = content.Tree;
            string treeName = (tree != null ? tree.GetName(hostLanguageID) : String.Empty);
            string nodeName = (node != null ? node.GetName(hostLanguageID) : String.Empty);
            string contentName = (content != null ? content.GetName(hostLanguageID) : String.Empty);
            List<string> words = new List<string>();
            HashSet<string> wordsSet = new HashSet<string>();
            List<string> phrases = new List<string>();
            HashSet<string> phrasesSet = new HashSet<string>();
            List<string> wordsAndPhrases = new List<string>();
            HashSet<string> wordsAndPhrasesSet = new HashSet<string>();
            int totalCount;
            int pageCount;

            CollectWordsAndPhrases(studyList, targetLanguageID, words, wordsSet, phrases, phrasesSet, wordsAndPhrases, wordsAndPhrasesSet);

            bool returnValue = Repositories.UserRunItems.GetUserRunItems(
                wordsAndPhrases, owner, profile, targetLanguageID, languageDescriptors, showNew, showActive, showLearned,
                sortOrder, userRunItemDictionary, userRunItemList, 0, 0, out pageCount, out totalCount);

            if (showNew && showActive && showLearned)
            {
                foreach (string word in words)
                {
                    UserRunItem userRunItem = null;

                    if ((WordBreakpoint != null) && (word == WordBreakpoint))
                        ApplicationData.Global.PutConsoleMessage("CollectTargetUserRunItems (word): " + word);

                    if (!userRunItemDictionary.TryGetValue(word.ToLower(), out userRunItem))
                    {
                        if ((wordUserRunItemDictionary != null) &&
                            wordUserRunItemDictionary.TryGetValue(word.ToLower(), out userRunItem))
                        {
                            userRunItemDictionary.Add(word.ToLower(), userRunItem);
                            userRunItemList.Add(userRunItem);
                            if (newWordUserRunItems.Contains(userRunItem))
                                newUserRunItems.Add(userRunItem);
                        }
                        else
                        {
                            userRunItem = new UserRunItem(
                                word,
                                null,
                                null,
                                null,
                                UserRunStateCode.Future,
                                0,
                                owner,
                                profile,
                                treeName,
                                nodeName,
                                contentName,
                                false);
                            userRunItemDictionary.Add(word.ToLower(), userRunItem);
                            userRunItemList.Add(userRunItem);
                            newUserRunItems.Add(userRunItem);
                        }
                    }
                }

                foreach (string phrase in phrases)
                {
                    UserRunItem userRunItem = null;

                    if ((WordBreakpoint != null) && (phrase == WordBreakpoint))
                        ApplicationData.Global.PutConsoleMessage("CollectTargetUserRunItems (phrase): " + phrase);

                    if (!userRunItemDictionary.TryGetValue(phrase.ToLower(), out userRunItem))
                    {
                        if ((wordUserRunItemDictionary != null) &&
                            wordUserRunItemDictionary.TryGetValue(phrase.ToLower(), out userRunItem))
                        {
                            userRunItemDictionary.Add(phrase.ToLower(), userRunItem);
                            userRunItemList.Add(userRunItem);
                            if (newWordUserRunItems.Contains(userRunItem))
                                newUserRunItems.Add(userRunItem);
                        }
                        else
                        {
                            userRunItem = new UserRunItem(
                                phrase,
                                null,
                                null,
                                null,
                                UserRunStateCode.Future,
                                0,
                                owner,
                                profile,
                                treeName,
                                nodeName,
                                contentName,
                                true);
                            userRunItemDictionary.Add(phrase.ToLower(), userRunItem);
                            userRunItemList.Add(userRunItem);
                            newUserRunItems.Add(userRunItem);
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool ResetUserRunItems(
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            ContentStudyList studyList,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> languageIDs)
        {
            if ((targetLanguageIDs == null) || (targetLanguageIDs.Count() == 0))
            {
                PutError("No target languageIDs.");
                return false;
            }

            if (userRunItemList.Count() == 0)
                return true;

            LanguageID targetLanguageID = targetLanguageIDs.First();
            BaseObjectContent content = (studyList != null ? studyList.Content : null);
            BaseObjectNode node = (content != null ? content.Node : null);

            foreach (UserRunItem userRunItem in userRunItemList)
            {
                userRunItem.MainDefinitions = null;
                userRunItem.OtherDefinitions = null;
            }

            if (studyList != null)
                FixupAlternateUserRunItems(
                    userRunItemDictionary, studyList, targetLanguageIDs);

            if ((hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0))
            {
                int ordinal = 0;

                foreach (UserRunItem testUserRunItem in userRunItemList)
                {
                    GetUserRunDefinitions(
                        testUserRunItem,
                        targetLanguageIDs,
                        hostLanguageIDs,
                        languageIDs,
                        node,
                        content);

                    ordinal++;
                }
            }

            foreach (UserRunItem testUserRunItem in userRunItemList)
                testUserRunItem.TouchAndClearModified();

            try
            {
                if (!Repositories.UserRunItems.UpdateList(userRunItemList, targetLanguageID))
                {
                    PutError("Error updating existing vocabulary items.");
                    return false;
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception updating existing vocabulary items", exc);
                return false;
            }

            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if (tool != null)
                tool.SaveDictionaryEntriesAddedAndUpdated();

            if (!FlushRemoteDictionaryCache())
                return false;

            return true;
        }

        public bool ResetUserRunItem(
            UserRunItem userRunItem,
            ContentStudyList studyList,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> languageIDs)
        {
            if ((targetLanguageIDs == null) || (targetLanguageIDs.Count() == 0))
            {
                PutError("No target languageIDs.");
                return false;
            }

            LanguageID targetLanguageID = targetLanguageIDs.First();

            if (studyList != null)
                FixupAlternateUserRunItem(userRunItem, studyList, targetLanguageIDs);

            if ((hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0))
            {
                userRunItem.MainDefinitions = null;
                userRunItem.OtherDefinitions = null;

                BaseObjectContent content = (studyList != null ? studyList.Content : null);
                BaseObjectNode node = (content != null ? content.Node : null);

                GetUserRunDefinitions(
                    userRunItem,
                    targetLanguageIDs,
                    hostLanguageIDs,
                    languageIDs,
                    node,
                    content);
            }

            userRunItem.TouchAndClearModified();

            try
            {
                if (!Repositories.UserRunItems.Update(userRunItem, targetLanguageID))
                {
                    PutError("Error updating existing vocabulary items.");
                    return false;
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception updating existing vocabulary items", exc);
                return false;
            }

            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if (tool != null)
                tool.SaveDictionaryEntriesAddedAndUpdated();

            if (!FlushRemoteDictionaryCache())
                return false;

            return true;
        }

        protected void FixupAlternateUserRunItem(
            UserRunItem userRunItem,
            ContentStudyList studyList,
            List<LanguageID> targetLanguageIDs)
        {
            targetLanguageIDs = LanguageID.GetExtendedLanguageIDsFromLanguageIDs(targetLanguageIDs);
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            LanguageID alternateLanguageID;
            int languageCount = targetLanguageIDs.Count();
            int languageIndex;
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            string word;
            TextRun wordRun;
            string alternateWord;
            TextRun alternateWordRun;
            int wordIndex;
            int wordCount;
            bool found;

            if ((targetLanguageID == null) || (languageCount < 2))
                return;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(targetLanguageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRunCount() == 0)
                {
                    word = languageItem.Text;

                    if (userRunItem.MatchTextIgnoreCase(word))
                    {
                        for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
                        {
                            alternateLanguageID = targetLanguageIDs[languageIndex];
                            LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                            if (alternateLanguageItem == null)
                                continue;

                            alternateWord = alternateLanguageItem.Text;

                            if (string.IsNullOrEmpty(alternateWord))
                                continue;

                            found = false;

                            if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                            {
                                foreach (LanguageString alternate in userRunItem.Alternates)
                                {
                                    if (alternate.LanguageID != alternateLanguageID)
                                        continue;

                                    if (TextUtilities.IsEqualStringsIgnoreCase(alternate.Text, alternateWord))
                                        found = true;
                                }
                            }

                            if (!found)
                            {
                                LanguageString alternate = new LanguageString(0, alternateLanguageID, alternateWord);

                                if (userRunItem.Alternates == null)
                                    userRunItem.Alternates = new List<LanguageString>() { alternate };
                                else
                                {
                                    userRunItem.Alternates.Add(alternate);
                                    userRunItem.Modified = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    wordCount = languageItem.WordRunCount();

                    for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                    {
                        wordRun = languageItem.GetWordRun(wordIndex);
                        word = languageItem.GetRunText(wordRun);

                        if ((WordBreakpoint != null) && (word == WordBreakpoint))
                            ApplicationData.Global.PutConsoleMessage("FixupAlternateUserRunItem (word): " + word);

                        if (userRunItem.MatchTextIgnoreCase(word))
                        {
                            for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
                            {
                                alternateLanguageID = targetLanguageIDs[languageIndex];
                                LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                                if (alternateLanguageItem == null)
                                    continue;

                                alternateWordRun = alternateLanguageItem.GetWordRun(wordIndex);

                                if (alternateWordRun == null)
                                    continue;

                                alternateWord = alternateLanguageItem.GetRunText(alternateWordRun);

                                if (string.IsNullOrEmpty(alternateWord))
                                    continue;

                                if ((alternateLanguageItem.WordRunCount() != wordCount) && (alternateWord != word))
                                    alternateWord = S("(word runs mismatch)");

                                found = false;

                                if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                                {
                                    foreach (LanguageString alternate in userRunItem.Alternates)
                                    {
                                        if (alternate.LanguageID != alternateLanguageID)
                                            continue;

                                        if (TextUtilities.IsEqualStringsIgnoreCase(alternate.Text, alternateWord))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    LanguageString alternate = new LanguageString(0, alternateLanguageID, alternateWord);

                                    if (userRunItem.Alternates == null)
                                        userRunItem.Alternates = new List<LanguageString>() { alternate };
                                    else
                                    {
                                        userRunItem.Alternates.Add(alternate);
                                        userRunItem.Modified = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void FixupAlternateUserRunItems(
            Dictionary<string, UserRunItem> userRunItemDictionary,
            ContentStudyList studyList,
            List<LanguageID> targetLanguageIDs)
        {
            targetLanguageIDs = LanguageID.GetExtendedLanguageIDsFromLanguageIDs(targetLanguageIDs);
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            LanguageID alternateLanguageID;
            int languageCount = targetLanguageIDs.Count();
            int languageIndex;
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            string word;
            TextRun wordRun;
            string alternateWord;
            TextRun alternateWordRun;
            int wordIndex;
            int wordCount;
            UserRunItem userRunItem;
            bool found;

            if ((targetLanguageID == null) || (languageCount < 2))
                return;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(targetLanguageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRunCount() == 0)
                {
                    word = languageItem.Text;

                    if (userRunItemDictionary.TryGetValue(word, out userRunItem))
                    {
                        for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
                        {
                            alternateLanguageID = targetLanguageIDs[languageIndex];
                            LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                            if (alternateLanguageItem == null)
                                continue;

                            alternateWord = alternateLanguageItem.Text;

                            if (string.IsNullOrEmpty(alternateWord))
                                continue;

                            found = false;

                            if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                            {
                                foreach (LanguageString alternate in userRunItem.Alternates)
                                {
                                    if (alternate.LanguageID != alternateLanguageID)
                                        continue;

                                    if (TextUtilities.IsEqualStringsIgnoreCase(alternate.Text, alternateWord))
                                        found = true;
                                }
                            }

                            if (!found)
                            {
                                LanguageString alternate = new LanguageString(0, alternateLanguageID, alternateWord);

                                if (userRunItem.Alternates == null)
                                    userRunItem.Alternates = new List<LanguageString>() { alternate };
                                else
                                {
                                    userRunItem.Alternates.Add(alternate);
                                    userRunItem.Modified = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    wordCount = languageItem.WordRunCount();

                    for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                    {
                        wordRun = languageItem.GetWordRun(wordIndex);
                        word = languageItem.GetRunText(wordRun);

                        if ((WordBreakpoint != null) && (word == WordBreakpoint))
                            ApplicationData.Global.PutConsoleMessage("FixupAlternateUserRunItems (word): " + word);

                        if (userRunItemDictionary.TryGetValue(word, out userRunItem))
                        {
                            for (languageIndex = 1; languageIndex < languageCount; languageIndex++)
                            {
                                alternateLanguageID = targetLanguageIDs[languageIndex];
                                LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                                if (alternateLanguageItem == null)
                                    continue;

                                alternateWordRun = alternateLanguageItem.GetWordRun(wordIndex);

                                if (alternateWordRun == null)
                                    continue;

                                alternateWord = alternateLanguageItem.GetRunText(alternateWordRun);

                                if (string.IsNullOrEmpty(alternateWord))
                                    continue;

                                if ((alternateLanguageItem.WordRunCount() != wordCount) && (alternateWord != word))
                                    alternateWord = S("(word runs mismatch)");

                                found = false;

                                if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                                {
                                    foreach (LanguageString alternate in userRunItem.Alternates)
                                    {
                                        if (alternate.LanguageID != alternateLanguageID)
                                            continue;

                                        if (TextUtilities.IsEqualStringsIgnoreCase(alternate.Text, alternateWord))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    LanguageString alternate = new LanguageString(0, alternateLanguageID, alternateWord);

                                    if (userRunItem.Alternates == null)
                                        userRunItem.Alternates = new List<LanguageString>() { alternate };
                                    else
                                    {
                                        userRunItem.Alternates.Add(alternate);
                                        userRunItem.Modified = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CollectAlternateUserRunItems(
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            List<LanguageID> languageIDs)
        {
            LanguageID targetLanguageID = languageIDs.FirstOrDefault();
            int languageCount = languageIDs.Count();
            UserRunItem testUserRunItem;
            string word;

            if ((targetLanguageID == null) || (languageCount < 2))
                return;

            foreach (UserRunItem userRunItem in userRunItemList)
            {
                if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                {
                    foreach (LanguageString alternate in userRunItem.Alternates)
                    {
                        if (alternate.LanguageID == targetLanguageID)
                            continue;

                        if (!languageIDs.Contains(alternate.LanguageID))
                            continue;

                        word = alternate.Text.ToLower();

                        if (!userRunItemDictionary.TryGetValue(word, out testUserRunItem))
                            userRunItemDictionary.Add(word, userRunItem);
                    }
                }
            }
        }

        public List<UserRunItem> GetUserRunItemsFromDictionary(
            Dictionary<string, UserRunItem> userRunItemDictionary)
        {
            List<UserRunItem> userRunItemList = new List<UserRunItem>();

            foreach (KeyValuePair<string, UserRunItem> kvp in userRunItemDictionary)
            {
                if (TextUtilities.IsEqualStringsIgnoreCase(kvp.Key, kvp.Value.Text))
                    userRunItemList.Add(kvp.Value);
            }

            return userRunItemList;
        }

        public bool GetUserRunDefinitions(
            UserRunItem userRunItem,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> languageIDs,
            BaseObjectNode node,
            BaseObjectContent content)
        {
            string studyText = userRunItem.TextLower;
            ContentStudyList studyList = null;
            MultiLanguageItem studyItem = null;
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            DictionaryEntry dictionaryEntry = null;
            List<DictionaryEntry> dictionaryEntries = null;
            MultiLanguageString definition;
            LanguageTool tool = GetLanguageTool(targetLanguageID);
            string errorMessage;
            bool returnValue = true;

            if ((WordBreakpoint != null) && (studyText == WordBreakpoint))
                ApplicationData.Global.PutConsoleMessage("GetUserRunDefinitions: " + studyText);

            if (userRunItem.HasAlternates())
            {
                int count = userRunItem.AlternateCount();
                int index1;

                for (index1 = 0; index1 < count; index1++)
                {
                    LanguageString alternate1 = userRunItem.GetAlternateIndexed(index1);

                    for (int index2 = count - 1; index2 >= 0; index2--)
                    {
                        LanguageString alternate2 = userRunItem.GetAlternateIndexed(index2);

                        if (TextUtilities.IsEqualStringsIgnoreCase(alternate1.Text, alternate2.Text))
                        {
                            userRunItem.DeleteAlternateIndexed(index2);
                            count--;
                        }
                    }
                }
            }

            userRunItem.OtherDefinitions = null;

            if ((content != null) && (content.ContentClass == ContentClassType.StudyList))
            {
                studyList = content.ContentStorageStudyList;

                if (studyItem == null)
                {
                    if ((studyList != null) && (studyList.StudyItemCount() != 0))
                        studyItem = studyList.FindStudyItemRecurse(studyText, targetLanguageID);
                }

                if (studyItem == null)
                {
                    foreach (BaseObjectContent aContent in node.ContentList)
                    {
                        if (aContent == content)
                            continue;

                        if (aContent.ContentClass == ContentClassType.StudyList)
                        {
                            studyList = aContent.ContentStorageStudyList;

                            if (studyList == null)
                            {
                                aContent.ResolveReferences(Repositories, false, false);
                                studyList = aContent.ContentStorageStudyList;
                            }

                            if (studyList != null)
                            {
                                studyItem = studyList.FindStudyItemRecurse(studyText, targetLanguageID);

                                if (studyItem != null)
                                    break;
                            }
                        }
                    }
                }

                if (studyItem != null)
                {
                    foreach (LanguageID languageID in targetLanguageIDs)
                    {
                        if (languageID == targetLanguageID)
                            continue;

                        string alternateString = studyItem.Text(languageID);

                        if (!String.IsNullOrEmpty(alternateString))
                        {
                            if (!userRunItem.HasAlternate(alternateString, languageID))
                                userRunItem.AddAlternate(alternateString, languageID);
                        }
                    }

                    definition = new MultiLanguageString(studyText, hostLanguageIDs);

                    foreach (LanguageID languageID in hostLanguageIDs)
                    {
                        string definitionString = studyItem.Text(languageID);
                        definition.SetText(languageID, definitionString);
                    }

                    if (userRunItem.OtherDefinitions == null)
                        userRunItem.OtherDefinitions = new List<MultiLanguageString>() { definition };
                    else
                        userRunItem.OtherDefinitions.Add(definition);
                }

                studyList = content.ContentStorageStudyList;

                if ((studyItem == null) && (studyList != null))
                {
                    int c = studyList.StudyItemCountRecurse();
                    int i;
                    List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;

                    for (i = 0; i < c; i++)
                    {
                        studyItem = studyItems[i];

                        foreach (LanguageID tLid in targetLanguageIDs)
                        {
                            LanguageItem languageItem = studyItem.LanguageItem(tLid);

                            if ((languageItem == null) || !languageItem.HasText())
                                continue;

                            if (languageItem.Text.IndexOf(studyText, StringComparison.OrdinalIgnoreCase) == -1)
                                continue;

                            List<TextRun> wordRuns = languageItem.WordRuns;
                            int wordCount = wordRuns.Count();
                            int wordIndex = 0;

                            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                            {
                                TextRun wordRun = wordRuns[wordIndex];
                                string word = languageItem.GetRunText(wordRun);

                                if (!TextUtilities.IsEqualStringsIgnoreCase(studyText, word))
                                    continue;

                                definition = studyItem.GetAlignedText(tLid, wordIndex, hostLanguageIDs);

                                if (definition == null)
                                    continue;

                                if (userRunItem.OtherDefinitions == null)
                                    userRunItem.OtherDefinitions = new List<MultiLanguageString>() { definition };
                                else
                                    userRunItem.OtherDefinitions.Add(definition);
                            }

                            List<TextRun> phraseRuns = languageItem.PhraseRuns;

                            if (phraseRuns != null)
                            {
                                int phraseCount = phraseRuns.Count();
                                int phraseIndex = 0;

                                for (phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
                                {
                                    TextRun phraseRun = phraseRuns[phraseIndex];
                                    string phrase = languageItem.GetRunText(phraseRun);

                                    if (!TextUtilities.IsEqualStringsIgnoreCase(studyText, phrase))
                                        continue;

                                    definition = studyItem.GetAlignedPhrase(tLid, phraseIndex, hostLanguageIDs);

                                    if (definition == null)
                                        continue;

                                    if (userRunItem.OtherDefinitions == null)
                                        userRunItem.OtherDefinitions = new List<MultiLanguageString>() { definition };
                                    else
                                        userRunItem.OtherDefinitions.Add(definition);
                                }
                            }
                        }
                    }
                }
            }

            dictionaryEntries = LookupLocalOrRemoteDictionaryEntries(
                studyText,
                targetLanguageIDs,
                hostLanguageIDs);

            if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
            {
                dictionaryEntry = dictionaryEntries.First();
                int senseCount = dictionaryEntry.SenseCount;
                int senseIndex;
                bool isStemOnly = dictionaryEntry.HasSenseWithStemOnly();

                for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                {
                    Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                    if (((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem)) && !isStemOnly)
                        continue;

                    if (dictionaryEntry.HasAlternates() && userRunItem.HasAlternates())
                    {
                        int reading = sense.Reading;
                        bool found = false;

                        foreach (LanguageString itemAlternate in userRunItem.Alternates)
                        {
                            LanguageString alternate = dictionaryEntry.GetAlternate(itemAlternate.LanguageID, reading);

                            if ((alternate != null) && TextUtilities.IsEqualStringsIgnoreCase(alternate.Text, itemAlternate.Text))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            continue;
                    }

                    definition = null;

                    foreach (LanguageID languageID in hostLanguageIDs)
                    {
                        if (sense.HasLanguage(languageID))
                        {
                            string definitionString = sense.GetDefinition(languageID, false, false);
                            bool found = false;

                            definitionString = definitionString.Replace(" / ", ", ");

                            if (userRunItem.OtherDefinitions != null)
                            {
                                foreach (MultiLanguageString def in userRunItem.OtherDefinitions)
                                {
                                    if (TextUtilities.IsEqualStringsIgnoreCase(def.Text(languageID), definitionString))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }

                            if (!found)
                            {
                                if (definition == null)
                                    definition = new MultiLanguageString("StudyItem", hostLanguageIDs);

                                string categoryString = sense.CategoryString;

                                if (sense.Category != LexicalCategory.Unknown)
                                {
                                    string category = sense.GetLexicalCategoryName(UILanguageID);

                                    if (sense.Category == LexicalCategory.Inflection)
                                    {
                                        int si = 0;
                                        foreach (LanguageID lid in targetLanguageIDs)
                                        {
                                            Sense dictionaryFormSense = dictionaryEntry.GetSenseWithLanguageID(lid);
                                            if (dictionaryFormSense != null)
                                            {
                                                string dictionaryForm = dictionaryFormSense.GetDefinition(lid, false, false);
                                                if (si == 0)
                                                    category += " of " + "(" + dictionaryForm;
                                                else
                                                    category += " - " + dictionaryForm;
                                                si++;
                                            }
                                        }
                                        if (si != 0)
                                            category += ")";
                                    }

                                    if (!String.IsNullOrEmpty(categoryString))
                                    {
                                        if (tool != null)
                                            categoryString = tool.DecodeCategoryString(categoryString);

                                        definitionString += " (" + category + " - " + categoryString + ")";
                                    }
                                    else
                                        definitionString += " (" + category + ")";
                                }
                                else if (!String.IsNullOrEmpty(categoryString))
                                {
                                    if (tool != null)
                                        categoryString = tool.DecodeCategoryString(categoryString);

                                    definitionString += " (" + categoryString + ")";
                                }

                                definition.SetText(languageID, definitionString);
                            }
                        }
                    }

                    if (definition != null)
                    {
                        if (userRunItem.OtherDefinitions == null)
                            userRunItem.OtherDefinitions = new List<MultiLanguageString>() { definition };
                        else
                            userRunItem.OtherDefinitions.Add(definition);
                    }
                }

                if (dictionaryEntry.HasAlternates())
                {
                    if (userRunItem.HasAlternates())
                    {
                        int count = userRunItem.AlternateCount();
                        int index;

                        for(index = count - 1; index >= 0; index--)
                        {
                            LanguageString alternate = userRunItem.GetAlternateIndexed(index);

                            if ((alternate != null) && !dictionaryEntry.HasAlternate(alternate.Text, alternate.LanguageID))
                                userRunItem.DeleteAlternateIndexed(index);
                        }
                    }

                    foreach (LanguageString alternate in dictionaryEntry.Alternates)
                    {
                        if (alternate.LanguageID == targetLanguageID)
                            continue;

                        if (!userRunItem.HasAlternate(alternate.Text, alternate.LanguageID))
                            userRunItem.AddAlternate(alternate.Text, alternate.LanguageID);
                    }
                }
            }

            if ((userRunItem.OtherDefinitions == null) || (userRunItem.OtherDefinitions.Count() == 0))
            {
                studyItem = new MultiLanguageItem(
                    studyText,
                    languageIDs);
                studyItem.SetText(targetLanguageID, studyText);

                if ((userRunItem.Alternates != null) && (userRunItem.Alternates.Count() != 0))
                {
                    foreach (LanguageString alternate in userRunItem.Alternates)
                    {
                        if (!studyItem.HasText(alternate.LanguageID))
                            studyItem.SetText(alternate.LanguageID, alternate.Text);
                    }
                }

                if (Translator.TranslateMultiLanguageItem(
                        studyItem,
                        languageIDs,
                        false,
                        false,
                        out errorMessage,
                        false))
                {
                    bool found = false;
                    bool isHasText = false;

                    definition = new MultiLanguageString("StudyItem", hostLanguageIDs);

                    foreach (LanguageID languageID in targetLanguageIDs)
                    {
                        if (languageID == targetLanguageID)
                            continue;

                        string alternateString = studyItem.Text(languageID);

                        if (!String.IsNullOrEmpty(alternateString))
                        {
                            if (!userRunItem.HasAlternate(alternateString, languageID))
                                userRunItem.AddAlternate(alternateString, languageID);
                        }
                    }

                    foreach (LanguageID languageID in hostLanguageIDs)
                    {
                        string definitionString = studyItem.Text(languageID);

                        if (!String.IsNullOrEmpty(definitionString))
                            isHasText = true;
                        else
                            continue;

                        definition.SetText(languageID, definitionString);

                        if (userRunItem.OtherDefinitions != null)
                        {
                            foreach (MultiLanguageString def in userRunItem.OtherDefinitions)
                            {
                                string otherText = def.Text(languageID);
                                if (TextUtilities.IsEqualStringsIgnoreCase(otherText, definitionString))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!found && isHasText)
                    {
                        if (userRunItem.OtherDefinitions == null)
                            userRunItem.OtherDefinitions = new List<MultiLanguageString>() { definition };
                        else
                            userRunItem.OtherDefinitions.Add(definition);

                        /*
                        if (!AddStudyItemToDictionary(
                            node,
                            content,
                            studyItem,
                            languageIDs,
                            languageIDs,
                            LexicalCategory.Unknown,
                            String.Empty,
                            false,
                            false,
                            ref errorMessage))
                        {
                            if (HasError)
                                Error = Error + "\n" + errorMessage;
                            else
                                Error = errorMessage;

                            returnValue = false;
                        }
                        */
                    }
                }
                else
                {
                    studyItem = null;
                    Error = errorMessage;
                    returnValue = false;
                }
            }

            UserRunItemDefinitionsCheck(userRunItem);

            return returnValue;
        }

        public bool UserRunItemDefinitionsCheck(UserRunItem userRunItem)
        {
            bool returnValue = false;

            if ((userRunItem.MainDefinitions == null) || (userRunItem.MainDefinitions.Count() == 0))
            {
                if ((userRunItem.OtherDefinitions != null) && (userRunItem.OtherDefinitions.Count() != 0))
                {
                    MultiLanguageString definition = userRunItem.OtherDefinitions.First();
                    definition = new MultiLanguageString(definition);
                    userRunItem.MainDefinitions = new List<MultiLanguageString>() { definition };
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public static void FilterUserRunItems(
            List<UserRunItem> userRunItemList,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            bool showNew,
            bool showActive,
            bool showLearned)
        {
            if (!showNew || !showActive || !showLearned)
            {
                for (int i = userRunItemList.Count() - 1; i >= 0; i--)
                {
                    UserRunItem testItem = userRunItemList[i];

                    switch (testItem.UserRunState)
                    {
                        case UserRunStateCode.Future:
                            if (!showNew)
                            {
                                userRunItemList.RemoveAt(i);
                                userRunItemDictionary.Remove(testItem.TextLower);
                            }
                            break;
                        case UserRunStateCode.Active:
                            if (!showActive)
                            {
                                userRunItemList.RemoveAt(i);
                                userRunItemDictionary.Remove(testItem.TextLower);
                            }
                            break;
                        case UserRunStateCode.Learned:
                            if (!showLearned)
                            {
                                userRunItemList.RemoveAt(i);
                                userRunItemDictionary.Remove(testItem.TextLower);
                            }
                            break;
                        default:
                            userRunItemList.RemoveAt(i);
                            userRunItemDictionary.Remove(testItem.TextLower);
                            break;
                    }
                }
            }
        }

        public static void SortUserRunItems(
            List<UserRunItem> userRunItems,
            List<string> sortOrder,
            List<LanguageDescriptor> languageDescriptors)
        {
            if ((sortOrder != null) && (sortOrder.Count() != 0))
                userRunItems.Sort(UserRunItem.GetComparer(sortOrder, languageDescriptors));
        }

        public bool IsPhrase(string text, LanguageID languageID)
        {
            if (TextUtilities.HasWhiteSpace(text))
                return true;

            return false;
        }

        public List<string> CollectSentences(ContentStudyList studyList, LanguageID languageID)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            List<string> sentences = CollectSentences(studyItems, languageID);
            return sentences;
        }

        public List<string> CollectSentences(List<MultiLanguageItem> studyItems, LanguageID languageID)
        {
            List<string> sentences = new List<string>();
            HashSet<string> sentencesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.SentenceRunCount() == 0)
                {
                    string sentence = languageItem.Text;

                    if (!String.IsNullOrEmpty(sentence))
                    {
                        if (sentencesSet.Add(sentence))
                            sentences.Add(sentence);
                    }
                }
                else
                {
                    foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                    {
                        string sentence = languageItem.GetRunText(sentenceRun);

                        if (!String.IsNullOrEmpty(sentence))
                        {
                            if (sentencesSet.Add(sentence))
                                sentences.Add(sentence);
                        }
                    }
                }
            }

            return sentences;
        }

        public List<MultiLanguageString> CollectWordInstances(
            ContentStudyList studyList,
            List<LanguageID> targetLanguageIDs)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            List<MultiLanguageString> words = CollectWordInstances(studyItems, targetLanguageIDs);
            return words;
        }

        public List<MultiLanguageString> CollectWordInstances(
            List<MultiLanguageItem> studyItems,
            List<LanguageID> targetLanguageIDs)
        {
            List<MultiLanguageString> words = new List<MultiLanguageString>();
            HashSet<string> wordsSet = new HashSet<string>();
            LanguageID targetLanguageID = targetLanguageIDs.First();
            int targetLanguageCount = targetLanguageIDs.Count();
            List<string> currentWordStrings = new List<string>(targetLanguageCount);
            MultiLanguageString currentWord;
            string word;
            string wordHash;
            LanguageID altLanguageID;
            string altWord;
            int sentenceWordCount;
            int sentenceWordIndex;
            TextRun wordRun;
            TextRun altWordRun;
            LanguageItem altLanguageItem;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);

                if (targetLanguageItem == null)
                    continue;

                if (targetLanguageItem.WordRunCount() == 0)
                {
                    word = targetLanguageItem.Text;

                    if (!String.IsNullOrEmpty(word))
                    {
                        wordHash = word.ToLower();
                        currentWordStrings.Clear();
                        currentWordStrings.Add(word);

                        for (int li = 1; li < targetLanguageCount; li++)
                        {
                            altLanguageID = targetLanguageIDs[li];
                            altWord = studyItem.Text(altLanguageID);
                            currentWordStrings.Add(altWord);
                            wordHash += "," + altWord.ToLower();
                        }

                        if (wordsSet.Add(wordHash))
                        {
                            currentWord = new MultiLanguageString(
                                null,
                                targetLanguageIDs,
                                currentWordStrings);
                            words.Add(currentWord);
                        }
                    }
                }
                else
                {
                    sentenceWordCount = targetLanguageItem.WordRunCount();

                    for (sentenceWordIndex = 0; sentenceWordIndex < sentenceWordCount; sentenceWordIndex++)
                    {
                        wordRun = targetLanguageItem.GetWordRun(sentenceWordIndex);
                        word = targetLanguageItem.GetRunText(wordRun);

                        if (!String.IsNullOrEmpty(word))
                        {
                            wordHash = word.ToLower();
                            currentWordStrings.Clear();
                            currentWordStrings.Add(word);

                            for (int li = 1; li < targetLanguageCount; li++)
                            {
                                altLanguageID = targetLanguageIDs[li];
                                altLanguageItem = studyItem.LanguageItem(altLanguageID);

                                if (altLanguageItem != null)
                                {
                                    altWordRun = altLanguageItem.GetWordRun(sentenceWordIndex);
                                    altWord = altLanguageItem.GetRunText(altWordRun);
                                    currentWordStrings.Add(altWord);
                                    wordHash += "," + altWord.ToLower();
                                }
                                else
                                {
                                    currentWordStrings.Add(String.Empty);
                                    wordHash += ",";
                                }
                            }

                            if (wordsSet.Add(wordHash))
                            {
                                currentWord = new MultiLanguageString(
                                    null,
                                    targetLanguageIDs,
                                    currentWordStrings);
                                words.Add(currentWord);
                            }
                        }
                    }
                }
            }

            return words;
        }

        // Collect dictionary entrys from words in study list.
        public void CollectDictionaryWords(
            ContentStudyList studyList,
            LanguageID targetLanguageID,
            List<LanguageID> alternateLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool isUsePhrasedRuns,
            bool isTranslateMissingItems,
            bool isAddTranslatedItemsToDictionary,
            List<DictionaryEntry> wordsList,
            Dictionary<string, DictionaryEntry> wordsDictionary)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            CollectDictionaryWords(
                studyItems,
                targetLanguageID,
                alternateLanguageIDs,
                hostLanguageIDs,
                isUsePhrasedRuns,
                isTranslateMissingItems,
                isAddTranslatedItemsToDictionary,
                wordsList,
                wordsDictionary);
        }

        // Collect dictionary entrys from a list of study items.
        public void CollectDictionaryWords(
            List<MultiLanguageItem> studyItems,
            LanguageID targetLanguageID,
            List<LanguageID> alternateLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool isUsePhrasedRuns,
            bool isTranslateMissingItems,
            bool isAddTranslatedItemsToDictionary,
            List<DictionaryEntry> entryList,
            Dictionary<string, DictionaryEntry> entryDictionary)
        {
            CollectDictionaryWordsSpecial(
                studyItems,
                targetLanguageID,
                alternateLanguageIDs,
                hostLanguageIDs,
                isUsePhrasedRuns,
                isTranslateMissingItems,
                isAddTranslatedItemsToDictionary,
                entryList,
                entryDictionary,
                LookupLocalOrRemoteDictionaryEntries);
        }

        // Collect dictionary entrys from a list of study items.
        public void CollectDictionaryWordsSpecial(
            List<MultiLanguageItem> studyItems,
            LanguageID targetLanguageID,
            List<LanguageID> alternateLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool isUsePhrasedRuns,
            bool isTranslateMissingItems,
            bool isAddTranslatedItemsToDictionary,
            List<DictionaryEntry> entryList,
            Dictionary<string, DictionaryEntry> entryDictionary,
            LookupDictionaryEntriesDelegate lookupFunction)
        {
            DictionaryEntry entry;
            List<string> wordList = new List<string>();
            HashSet<string> wordsSet = new HashSet<string>();
            List<LanguageID> targetLanguageIDs = new List<LanguageID>() { targetLanguageID };

            if ((alternateLanguageIDs != null) && (alternateLanguageIDs.Count() != 0))
                targetLanguageIDs.AddRange(alternateLanguageIDs);

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(targetLanguageID);

                if (languageItem == null)
                    continue;

                List<TextRun> wordRuns = (isUsePhrasedRuns ? languageItem.PhrasedWordRuns : languageItem.WordRuns);

                if ((wordRuns == null) || (wordRuns.Count() == 0))
                {
                    string word = languageItem.Text;

                    if (!String.IsNullOrEmpty(word))
                    {
                        word = word.ToLower();

                        if (wordsSet.Add(word))
                        {
                            if (!entryDictionary.TryGetValue(word, out entry))
                                wordList.Add(word);
                        }
                    }
                }
                else if (wordRuns != null)
                {
                    foreach (TextRun wordRun in wordRuns)
                    {
                        string word = languageItem.GetRunText(wordRun);

                        if (!String.IsNullOrEmpty(word))
                        {
                            word = word.ToLower();

                            if (wordsSet.Add(word))
                            {
                                if (!entryDictionary.TryGetValue(word, out entry))
                                    wordList.Add(word);
                            }
                        }
                    }
                }
            }

            if (wordList.Count() != 0)
            {
                foreach (string word in wordList)
                {
                    List<DictionaryEntry> wordEntries = lookupFunction(
                        word,
                        targetLanguageIDs,
                        hostLanguageIDs);

                    //if (word == "cuando")
                    //    ApplicationData.Global.PutConsoleMessage("CollectDictionaryWordsSpecial: word = " + word);

                    if ((wordEntries != null) && (wordEntries.Count() != 0))
                    {
                        if (wordEntries.Count() > 1)
                        {
                            foreach (DictionaryEntry wordEntry in wordEntries)
                            {
                                DictionaryEntry tmpEntry;

                                if (!entryDictionary.TryGetValue(word, out tmpEntry))
                                {
                                    entryDictionary.Add(word, wordEntry);
                                    entryList.Add(wordEntry);
                                }
                            }
                        }
                        else
                        {
                            DictionaryEntry wordEntry = wordEntries.First();
                            entryDictionary.Add(word, wordEntry);
                            entryList.Add(wordEntry);
                        }
                    }
                    else if (isTranslateMissingItems && (LanguageUtilities != null))
                    {
                        DictionaryEntry wordEntry = new DictionaryEntry(
                            word,
                            targetLanguageID);

                        string errorMessage;

                        if (LanguageUtilities.TranslateDictionaryEntry(
                                wordEntry,
                                hostLanguageIDs,
                                out errorMessage))
                        {
                            entryDictionary.Add(word, wordEntry);
                            entryList.Add(wordEntry);
                            PutMessage("Auto translation for word: " + word +
                                ": " + wordEntry.GetDefinition(hostLanguageIDs.First(), false, false));

                            if (isAddTranslatedItemsToDictionary &&
                                !Repositories.Dictionary.Contains(word, targetLanguageID))
                            {
                                try
                                {
                                    if (Repositories.Dictionary.Add(wordEntry, targetLanguageID))
                                        PutMessage("Added auto translated word to dictionary: " + word +
                                            ": " + wordEntry.GetDefinition(hostLanguageIDs.First(), false, false));
                                    else
                                        PutError("Error saving translated dictionary entry: ", word);
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError(
                                        "Error saving translated dictionary entry: ", word, exc);
                                }
                            }
                        }
                        else
                            PutMessage("Auto translation failed for word: " + word + ": " + errorMessage);
                    }
                    else
                        PutMessage("No dictionary entry for: " + word);
                }
            }
        }

        public void CollectWords(
            ContentStudyList studyList,
            LanguageID languageID,
            List<string> words,
            HashSet<string> wordsSet)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            CollectWords(studyItems, languageID, words, wordsSet);
        }

        public void CollectWords(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            List<string> words,
            HashSet<string> wordsSet)
        {
            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRunCount() == 0)
                {
                    string word = languageItem.Text;

                    if (!String.IsNullOrEmpty(word))
                    {
                        word = word.ToLower();

                        if (wordsSet.Add(word))
                            words.Add(word);
                    }
                }
                else
                {
                    foreach (TextRun wordRun in languageItem.WordRuns)
                    {
                        string word = languageItem.GetRunText(wordRun);

                        if (!String.IsNullOrEmpty(word))
                        {
                            word = word.ToLower();

                            if (wordsSet.Add(word))
                                words.Add(word);
                        }
                    }
                }
            }
        }

        public void CollectPhrases(
            ContentStudyList studyList,
            LanguageID languageID,
            List<string> phrases,
            HashSet<string> phrasesSet)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            CollectPhrases(studyItems, languageID, phrases, phrasesSet);
        }

        public void CollectPhrases(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            List<string> phrases,
            HashSet<string> phrasesSet)
        {
            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.HasPhraseRuns())
                {
                    foreach (TextRun phraseRun in languageItem.PhraseRuns)
                    {
                        string phrase = languageItem.GetRunText(phraseRun);

                        if (!String.IsNullOrEmpty(phrase))
                        {
                            phrase = phrase.ToLower();

                            if (phrasesSet.Add(phrase))
                                phrases.Add(phrase);
                        }
                    }
                }
            }
        }

        public void CollectWordsAndPhrases(
            ContentStudyList studyList,
            LanguageID languageID,
            List<string> words,
            HashSet<string> wordsSet,
            List<string> phrases,
            HashSet<string> phrasesSet,
            List<string> wordsAndPhrases,
            HashSet<string> wordsAndPhrasesSet)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            CollectWordsAndPhrases(studyItems, languageID, words, wordsSet, phrases, phrasesSet, wordsAndPhrases, wordsAndPhrasesSet);
        }

        public void CollectWordsAndPhrases(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            List<string> words,
            HashSet<string> wordsSet,
            List<string> phrases,
            HashSet<string> phrasesSet,
            List<string> wordsAndPhrases,
            HashSet<string> wordsAndPhrasesSet)
        {
            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRunCount() == 0)
                {
                    string word = languageItem.Text;

                    if (!String.IsNullOrEmpty(word))
                    {
                        word = word.ToLower();

                        if (wordsSet.Add(word))
                            words.Add(word);
                    }
                }
                else
                {
                    foreach (TextRun wordRun in languageItem.WordRuns)
                    {
                        string word = languageItem.GetRunText(wordRun);

                        if (!String.IsNullOrEmpty(word))
                        {
                            word = word.ToLower();

                            if (wordsSet.Add(word))
                                words.Add(word);

                            if (wordsAndPhrasesSet.Add(word))
                                wordsAndPhrases.Add(word);
                        }
                    }

                    if (languageItem.HasPhraseRuns())
                    {
                        foreach (TextRun phraseRun in languageItem.PhraseRuns)
                        {
                            string phrase = languageItem.GetRunText(phraseRun);

                            if (!String.IsNullOrEmpty(phrase))
                            {
                                phrase = phrase.ToLower();

                                if (phrasesSet.Add(phrase))
                                    phrases.Add(phrase);

                                if (wordsAndPhrasesSet.Add(phrase))
                                    wordsAndPhrases.Add(phrase);
                            }
                        }
                    }
                }
            }
        }

        public List<string> CollectCharacters(ContentStudyList studyList, LanguageID languageID)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            List<string> characters = CollectCharacters(studyItems, languageID);
            return characters;
        }

        public List<string> CollectCharacters(List<MultiLanguageItem> studyItems, LanguageID languageID)
        {
            List<string> characters = new List<string>();
            HashSet<char> charactersSet = new HashSet<char>();

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (!languageItem.HasText())
                    continue;

                foreach (char character in languageItem.Text)
                {
                    if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(character))
                        continue;

                    if (charactersSet.Add(character))
                        characters.Add(character.ToString());
                }
            }

            return characters;
        }

        // Returns true if dictionary item added
        public bool DictionaryCheckLanguagesSentenceItem(List<LanguageID> languageIDs, MultiLanguageItem sentenceItem)
        {
            bool returnValue = false;

            if (sentenceItem == null)
                return false;

            if (sentenceItem.LanguageItems == null)
                return false;

            foreach (LanguageItem languageItem in sentenceItem.LanguageItems)
            {
                if (DictionaryCheckSentenceItem(languageItem.LanguageID, languageIDs, sentenceItem))
                    returnValue = true;
            }

            return returnValue;
        }

        // Returns true if dictionary item added
        public bool DictionaryCheckSentenceItem(LanguageID languageID, List<LanguageID> languageIDs, MultiLanguageItem sentenceItem)
        {
            bool returnValue = false;

            if (sentenceItem == null)
                return false;

            LanguageItem languageItem = sentenceItem.LanguageItem(languageID);

            if (languageItem == null)
                return false;

            if (!String.IsNullOrEmpty(languageID.ExtensionCode))
            {
                if (!languageItem.HasWordRuns() || sentenceItem.IsWordMismatch(languageID))
                {
                    List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
                    sentenceItem.AutoResetWordRuns(familyLanguageIDs, Repositories.Dictionary);
                }
            }
            else
            {
                if (!languageItem.HasWordRuns())
                    languageItem.LoadWordRunsFromText(Repositories.Dictionary);

                if (!languageItem.HasWordRuns())
                    return false;
            }

            foreach (TextRun wordRun in languageItem.WordRuns)
            {
                string word = languageItem.GetRunText(wordRun);

                if (!String.IsNullOrEmpty(word))
                {
                    if (DictionaryCheckWord(languageID, languageIDs, word))
                        returnValue = true;
                }
            }

            return returnValue;
        }

        // Returns true if dictionary item added
        public bool DictionaryCheckWord(LanguageID languageID, List<LanguageID> languageIDs, string word)
        {
            List<LanguageID> searchLanguageIDs = new List<LanguageID>(1) { languageID };
            List<DictionaryEntry> dictionaryEntries = null;
            string errorMessage;
            bool useRemote = ApplicationData.IsMobileVersion && ApplicationData.Global.IsConnectedToANetwork();
            bool returnValue = false;

            dictionaryEntries = LookupLocalOrRemoteDictionaryEntries(
                word,
                searchLanguageIDs,
                null);

            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
            {
                MultiLanguageItem studyItem = new MultiLanguageItem(
                    word,
                    languageIDs);
                studyItem.SetText(languageID, word);

                if (Translator.TranslateMultiLanguageItem(
                        studyItem,
                        languageIDs,
                        false,
                        false,
                        out errorMessage,
                        false))
                {
                    /*
                    if (AddStudyItemToDictionary(
                            null,
                            null,
                            studyItem,
                            languageIDs,
                            languageIDs,
                            LexicalCategory.Unknown,
                            String.Empty,
                            false,
                            false,
                            ref errorMessage))
                        returnValue = true;
                    else
                    {
                        if (HasError)
                            Error = Error + "\n" + errorMessage;
                        else
                            Error = errorMessage;
                    }
                    */
                }
                else
                    Error = errorMessage;
            }

            string audioUrl;

            if (!LanguageLookup.IsAlternatePhonetic(languageID))
                ProcessAudio(
                    word,
                    languageID,
                    String.Empty,
                    null,
                    false,
                    String.Empty,
                    true,
                    useRemote,
                    null,
                    out audioUrl);

            return returnValue;
        }

        // Returns true if dictionary item added
        public bool DictionaryCheckWordItem(LanguageID languageID, List<LanguageID> languageIDs, MultiLanguageItem wordItem)
        {
            if (wordItem == null)
                return false;

            LanguageItem languageItem = wordItem.LanguageItem(languageID);

            if (languageItem == null)
                return false;

            string word = languageItem.Text;
            List<LanguageID> searchLanguageIDs = new List<LanguageID>(1) { languageID };
            List<DictionaryEntry> dictionaryEntries = null;
            bool useRemote = ApplicationData.IsMobileVersion && ApplicationData.Global.IsConnectedToANetwork();
            string errorMessage = String.Empty;
            bool returnValue = false;

            dictionaryEntries = LookupLocalOrRemoteDictionaryEntries(
                word,
                searchLanguageIDs,
                null);

            /*
            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
            {
                if (AddStudyItemToDictionary(
                        null,
                        null,
                        wordItem,
                        languageIDs,
                        languageIDs,
                        LexicalCategory.Unknown,
                        String.Empty,
                        false,
                        false,
                        ref errorMessage))
                    returnValue = true;
                else
                {
                    if (HasError)
                        Error = Error + "\n" + errorMessage;
                    else
                        Error = errorMessage;
                }
            }
            */

            string audioUrl;

            if (!LanguageLookup.IsAlternatePhonetic(languageID))
                ProcessAudio(
                    word,
                    languageID,
                    String.Empty,
                    null,
                    true,
                    String.Empty,
                    true,
                    useRemote,
                    null,
                    out audioUrl);

            return returnValue;
        }


        public void AutoResetWordRuns(MultiLanguageItem studyItem, List<LanguageID> languageIDs)
        {
            if ((studyItem == null) || (studyItem.LanguageItems == null))
                return;

            List<LanguageID> doneLanguageIDs = new List<LanguageID>();

#if true
            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.PhraseRuns = null;
                languageItem.WordRuns = null;
            }

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageID rootLanguageID = LanguageLookup.GetRootLanguageID(languageID);

                if (doneLanguageIDs.Contains(rootLanguageID))
                    continue;

                LanguageTool tool = GetLanguageToolWithWordFixes(rootLanguageID, studyItem.Content);

                if (tool != null)
                {
                    tool.GetMultiLanguageItemWordRuns(studyItem, true);
                    List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(
                        rootLanguageID);
                    doneLanguageIDs.AddRange(familyLanguageIDs);
                    continue;
                }
                else
                {
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    languageItem.AutoResetWordRuns(Repositories.Dictionary);

                    doneLanguageIDs.Add(languageID);
                }
            }
#else
            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem;

                if (doneLanguageIDs.Contains(languageID))
                    continue;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                {
                    LanguageID baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);

                    if (!studyItem.HasLanguageID(baseLanguageID))
                    {
                        languageItem = studyItem.LanguageItem(languageID);

                        if (languageItem == null)
                            continue;

                        languageItem.AutoResetWordRuns(Repositories.Dictionary);
                        doneLanguageIDs.Add(languageID);
                        continue;
                    }
                }

                if (LanguageLookup.HasAlternatePhonetic(languageID))
                {
                    LanguageID baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);
                    LanguageTool tool = GetLanguageToolWithWordFixes(baseLanguageID, studyItem.Content);

                    if (tool != null)
                    {
                        tool.ResetMultiLanguageItemWordRuns(studyItem);
                        List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(baseLanguageID);
                        doneLanguageIDs.AddRange(familyLanguageIDs);
                        continue;
                    }
                }

                languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.AutoResetWordRuns(Repositories.Dictionary);

                doneLanguageIDs.Add(languageID);
            }
#endif
        }
    }
}

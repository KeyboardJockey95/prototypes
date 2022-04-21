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
        public string WalkerOperationName;
        private bool WalkerNotFoundOnly;
        private bool WalkerSynthesizeMissingAudio;
        private LanguageID WalkerTargetLanguageID;
        private List<string> WalkerWordsList;
        private HashSet<string> WalkerWordsSet;
        private List<AudioMultiReference> WalkerAudioReferenceList;
        private HashSet<string> WalkerAudioSet;
        private List<string> WalkerAudioKeys;

        public bool DownloadMissingTreeVocabulary(
            BaseObjectNodeTree tree,
            string label,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            List<string> dictionaryWordsList = new List<string>();
            List<AudioMultiReference> audioReferenceList = new List<AudioMultiReference>();
            List<DictionaryEntry> dictionaryEntries;
            List<string> dictionaryKeysNotFound;
            List<string> audioKeysList = new List<string>();
            bool returnValue = true;
            string languageName = targetLanguageID.LanguageName(UILanguageID);

            int progressCount = 5; // graph table + collect + dictionary get + download audio + audio save
            ContinueProgress(ProgressCountBase + progressCount);

            UpdateProgressElapsed(S("Initializing conjugation table for ") + languageName + "...");

            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if (tool != null)
                tool.InitializeConjugationTableCheck();

            UpdateProgressElapsed(S("Collecting missing item information for ") + languageName + "...");

            CollectDictionaryItemsFromTree(
                tree,
                label,
                true,               // bool notFoundOnly
                targetLanguageID,
                false,              // bool doInBackground
                dictionaryWordsList,
                audioReferenceList,
                audioKeysList,
                false);             // bool synthesizeMissingAudio

            int dictionaryCount = dictionaryWordsList.Count();

            UpdateProgressElapsed(S("Downloading dictionary entries for ") +
                languageName + " (" + dictionaryCount.ToString() + " entries) ...");

            returnValue = GetServerVocabularyItems(
                targetLanguageID,
                targetLanguageIDs,
                hostLanguageIDs,
                true,                       // bool translateMissingEntries
                true,                       // bool synthesizeMissingAudio
                dictionaryWordsList,
                out dictionaryEntries,
                out dictionaryKeysNotFound);

            // What to do with not found list here?

            if ((audioReferenceList != null) && (dictionaryKeysNotFound != null))
            {
                foreach (string notFoundKey in dictionaryKeysNotFound)
                {
                    AudioMultiReference audioReference = audioReferenceList.FirstOrDefault(x => x.Name == notFoundKey);

                    if (audioReference != null)
                        audioReferenceList.Remove(audioReference);
                }
            }

            returnValue = DownloadDictionaryAudio(targetLanguageID, audioReferenceList) && returnValue;

            UpdateProgressElapsed(S("Saving audio references for ") + languageName + "...");

            returnValue = SaveAudioReferences(targetLanguageID, audioReferenceList) && returnValue;

            EndContinuedProgress();

            return returnValue;
        }

        public bool DownloadMissingNodeVocabulary(
            BaseObjectNode node,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            List<string> dictionaryWordsList = new List<string>();
            List<AudioMultiReference> audioReferenceList = new List<AudioMultiReference>();
            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();
            List<string> dictionaryKeysNotFound;
            List<string> audioKeysList = new List<string>();
            bool returnValue = true;

            int progressCount = 4; // collect + dictionary get + download audio + audio save
            ContinueProgress(ProgressCountBase + progressCount);

            string languageName = targetLanguageID.LanguageName(UILanguageID);
            UpdateProgressElapsed(S("Collecting missing item information for ") + languageName + "...");

            CollectDictionaryItemsFromNode(
                node,
                true,                   // bool notFoundOnly
                targetLanguageID,
                false,                  // bool doInBackground
                dictionaryWordsList,
                audioReferenceList,
                audioKeysList,
                false);                 // synthesizeMissingAudio

            int dictionaryCount = dictionaryWordsList.Count();

            UpdateProgressElapsed(S("Downloading dictionary entries for ") +
                languageName + " (" + dictionaryCount.ToString() + " entries) ...");

            returnValue = GetServerVocabularyItems(
                targetLanguageID,
                targetLanguageIDs,
                hostLanguageIDs,
                true,                       // bool translateMissingEntries
                true,                       // bool synthesizeMissingAudio
                dictionaryWordsList,
                out dictionaryEntries,
                out dictionaryKeysNotFound);

            returnValue = DownloadDictionaryAudio(targetLanguageID, audioReferenceList) && returnValue;

            UpdateProgressElapsed(S("Saving audio references for ") + languageName + "...");

            returnValue = SaveAudioReferences(targetLanguageID, audioReferenceList) && returnValue;

            EndContinuedProgress();

            return returnValue;
        }

        public bool GetServerVocabularyItems(
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            bool translateMissingEntries,
            bool synthesizeMissingAudio,
            List<string> dictionaryWordsList,
            out List<DictionaryEntry> dictionaryEntriesFound,
            out List<string> dictionaryKeysNotFound)
        {
            byte[] dictionaryEntriesFoundData = null;
            string[] dictionaryKeysNotFoundData = null;
            string errorMessage;
            bool returnValue = false;

            dictionaryEntriesFound = null;
            dictionaryKeysNotFound = null;

            try
            {
                returnValue = RemoteClient.GetVocabularyDictionaryEntries(
                    targetLanguageID.LanguageCultureExtensionCode,
                    UILanguageID.LanguageCultureExtensionCode,
                    LanguageID.GetLanguageCodeListFromLanguageIDs(targetLanguageIDs),
                    LanguageID.GetLanguageCodeListFromLanguageIDs(hostLanguageIDs),
                    UserName,
                    dictionaryWordsList,
                    translateMissingEntries,
                    synthesizeMissingAudio,
                    out dictionaryEntriesFoundData,
                    out dictionaryKeysNotFoundData,
                    out errorMessage);

                if (!String.IsNullOrEmpty(errorMessage))
                    PutError(errorMessage);
            }
            catch (Exception exc)
            {
                PutExceptionError(
                    "Error downloading dictionary entries for",
                    targetLanguageID.LanguageName(UILanguageID),
                    exc);
                returnValue = false;
            }

            if (returnValue)
            {
                if (dictionaryEntriesFoundData != null)
                {
                    FormatChunky format = new FormatChunky();
                    format.TargetType = "DictionaryEntry";
                    format.Repositories = Repositories;
                    format.UserRecord = UserRecord;
                    format.UserProfile = UserProfile;
                    format.TargetLanguageIDs = targetLanguageIDs;
                    format.HostLanguageIDs = hostLanguageIDs;
                    format.UniqueLanguageIDs = LanguageID.ConcatenateUnqueList(targetLanguageIDs, hostLanguageIDs);
                    format.UILanguageID = UILanguageID;
                    format.SetLanguages(format.UniqueLanguageIDs);
                    format.IncludeMedia = false;
                    format.IsIncludeMedia = false;

                    try
                    {
                        MemoryStream outStream = new MemoryStream(dictionaryEntriesFoundData);

                        format.TransferProgress(this);
                        format.Read(outStream);
                        TransferProgress(format);

                        if (format.AddObjects != null)
                            dictionaryEntriesFound = format.AddObjects.Cast<DictionaryEntry>().ToList();
                        else
                        {
                            PutError("Downloaded dicionary entries list is null.");
                            returnValue = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        errorMessage = "Exception while extraction dictionary entries from downloaded stream: " + exception.Message;

                        if (exception.InnerException != null)
                            errorMessage += " (" + exception.InnerException.Message + ")";

                        PutError(errorMessage);
                        returnValue = false;
                    }
                }

                if (dictionaryKeysNotFoundData != null)
                    dictionaryKeysNotFound = dictionaryKeysNotFoundData.ToList();
            }

            return returnValue;
        }

        public bool DownloadDictionaryEntries(
            LanguageID targetLanguageID,
            List<string> dictionaryWordsList,
            out List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = false;

            ConvertCanonical canonical = new ConvertCanonical(targetLanguageID, false);
            string canonicalPattern;
            MatchCode matchType = MatchCode.Exact;
            List<string> canonicalPatterns = new List<string>(dictionaryWordsList.Count());
            List<MatchCode> matchTypes = new List<MatchCode>(dictionaryWordsList.Count());
            bool allExact = true;
            bool notRegEx = false;

            dictionaryEntries = null;

            foreach (string pattern in dictionaryWordsList)
            {
                if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                    canonicalPattern = pattern;
                if (matchType != MatchCode.Exact)
                {
                    allExact = false;
                    if (matchType != MatchCode.RegEx)
                        notRegEx = true;
                }
                canonicalPatterns.Add(canonicalPattern);
                matchTypes.Add(matchType);
            }

            if (allExact || notRegEx)
            {
                try
                {
                    matchType = (allExact ? MatchCode.Exact : MatchCode.RegEx);
                    StringMatcher matcher = new StringMatcher(canonicalPatterns, "Key", matchType, 0, 0);
                    dictionaryEntries = RemoteRepositories.Dictionary.Query(matcher, targetLanguageID);
                }
                catch (Exception exc)
                {
                    PutExceptionError(
                        "Error downloading dictionary entries for",
                        targetLanguageID.LanguageName(UILanguageID),
                        exc);
                    returnValue = false;
                }
            }
            else
            {
                int count = canonicalPatterns.Count();

                dictionaryEntries = new List<DictionaryEntry>(count);

                for (int index = 0; index < count; index++)
                {
                    canonicalPattern = canonicalPatterns[index];
                    matchType = matchTypes[index];
                    StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, 0, 0);

                    try
                    {
                        List<DictionaryEntry> entries = RemoteRepositories.Dictionary.Query(matcher, targetLanguageID);

                        if ((entries != null) && (entries.Count() != 0))
                            dictionaryEntries.AddRange(entries);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError(
                            "Error downloading dictionary entries for",
                            targetLanguageID.LanguageName(UILanguageID),
                            exc);
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool SaveDictionaryEntries(
            List<LanguageID> targetLanguageIDs,
            List<DictionaryEntry> dictionaryEntries)
        {
            if (dictionaryEntries == null)
                return false;

            List<DictionaryEntry> languageDictionaryEntries = new List<DictionaryEntry>(dictionaryEntries.Count());
            bool returnValue = false;

            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                return true;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                languageDictionaryEntries.Clear();

                foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                {
                    if (dictionaryEntry.LanguageID == targetLanguageID)
                        languageDictionaryEntries.Add(dictionaryEntry);
                }

                if (languageDictionaryEntries.Count() == 0)
                    continue;

                try
                {
                    returnValue = Repositories.Dictionary.AddList(languageDictionaryEntries, targetLanguageID);
                }
                catch (Exception exc)
                {
                    PutExceptionError(
                        "Error saving dictionary entries for",
                        targetLanguageID.LanguageName(UILanguageID),
                        exc);
                }
            }

            return returnValue;
        }

#if true
        // New chunky file version.
        public bool DownloadDictionaryAudio(
            LanguageID targetLanguageID,
            List<AudioMultiReference> audioReferenceList)
        {
            if (audioReferenceList == null)
                return false;

            int audioReferenceCount = audioReferenceList.Count();
            string languageName = targetLanguageID.LanguageName(UILanguageID);
            int index;
            AudioMultiReference audioReference;

            UpdateProgressElapsed(S("Downloading audio files for ") +
                languageName + " (" + audioReferenceCount.ToString() + ") ...");

            string directoryUrl = MediaUtilities.ConcatenateFilePath("Dictionary", "Audio");
            directoryUrl = MediaUtilities.ConcatenateFilePath(directoryUrl, targetLanguageID.MediaLanguageCode());

            string directoryFilePath = MediaUtilities.ConcatenateFilePath(ApplicationData.ContentPath, "Dictionary");
            directoryFilePath = MediaUtilities.ConcatenateFilePath(directoryFilePath, "Audio");
            directoryFilePath = MediaUtilities.ConcatenateFilePath(directoryFilePath, targetLanguageID.MediaLanguageCode());

            List<string> filePartialPaths = new List<string>(audioReferenceList.Count());

            for (index = 0; index < audioReferenceCount; index++)
            {
                audioReference = audioReferenceList[index];

                if (audioReference.AudioInstanceCount() != 0)
                {
                    foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                    {
                        string fileName = audioInstance.FileName;
                        filePartialPaths.Add(fileName);
                    }
                }
            }

            byte[] fileChunkyData = null;
            string errorMessage = null;

            try
            {
                UserID userID = new UserID("LetMeIn");
                string userIDString = userID.StringData;

                if (!RemoteClient.GetContentFiles(directoryUrl, ".mp3", filePartialPaths, userIDString, out fileChunkyData, out errorMessage))
                {
                    PutError(errorMessage);
                    return false;
                }
            }
            catch (Exception exception)
            {
                PutExceptionError("Exception waiting for JTLanguageService.GetContentFiles", exception);
                return false;
            }

            if ((fileChunkyData == null) || (fileChunkyData.Length == 0))
            {
                PutError("GetContentFiles returned empty data.");
                return false;
            }

            try
            {
                FormatChunkyFiles format = new FormatChunkyFiles(directoryFilePath, true);
                MemoryStream inStream = new MemoryStream(fileChunkyData);
                format.Read(inStream);

                List<string> readFiles = format.FileList;

                for (index = audioReferenceCount - 1; index >= 0; index--)
                {
                    audioReference = audioReferenceList[index];

                    if (audioReference.AudioInstanceCount() != 0)
                    {
                        foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                        {
                            string fileName = audioInstance.FileName;

                            if (!readFiles.Contains(fileName))
                                audioReferenceList.RemoveAt(index);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                PutExceptionError("Exception waiting for JTLanguageService.GetContentFiles", exception);
                return false;
            }

            return true;
        }
#else
        // Old individual file version.
        public bool DownloadDictionaryAudio(
            LanguageID targetLanguageID,
            List<AudioReference> audioReferenceList)
        {
            if (audioReferenceList == null)
                return false;

            int audioReferenceCount = audioReferenceList.Count();
            string languageName = targetLanguageID.LanguageName(UILanguageID);
            int index;
            AudioReference audioReference;
            bool returnValue = true;

            ContinueProgress(audioReferenceCount);

            UpdateProgressElapsed(S("Downloading audio files for ") +
                languageName + " (" + audioReferenceCount.ToString() + ") ...");

            for (index = audioReferenceCount - 1; index >= 0; index--)
            {
                audioReference = audioReferenceList[index];
                string name = audioReference.Name;

                UpdateProgressElapsed(S("Downloading audio file") +
                    " " + index.ToString() + " of " + audioReferenceCount.ToString() + ": " + name + " ...");

                if (!DownloadDictionaryAudioFile(targetLanguageID, audioReference))
                {
                    audioReferenceList.RemoveAt(index);
                    returnValue = false;
                }
            }

            return returnValue;
        }
#endif

        public bool DownloadDictionaryAudioFile(
            LanguageID targetLanguageID,
            AudioReference audioReference)
        {
            string fileFriendlyName = audioReference.AudioFilePath;
            string mimeType = "audio/mpeg3";
            string audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, targetLanguageID);
            string audioFilePath = ApplicationData.MapToFilePath(audioUrl);
            bool returnValue = ApplicationData.Global.GetRemoteMediaFile(audioUrl, audioFilePath, ref Error);
            return returnValue;
        }

        public bool SaveAudioReferences(
            LanguageID targetLanguageID,
            List<AudioMultiReference> audioReferenceList)
        {
            bool returnValue = false;

            if (audioReferenceList == null)
                return false;

            if ((audioReferenceList == null) || (audioReferenceList.Count() == 0))
                return true;

            try
            {
                returnValue = Repositories.DictionaryMultiAudio.AddList(audioReferenceList, targetLanguageID);
            }
            catch (Exception exc)
            {
                PutExceptionError(
                    "Error saving audio references for",
                    targetLanguageID.LanguageName(UILanguageID),
                    exc);
            }

            return returnValue;
        }

        public void CollectDictionaryItemsFromTree(
            BaseObjectNodeTree tree,
            string label,
            bool notFoundOnly,
            LanguageID targetLanguageID,
            bool doInBackground,
            List<string> dictionaryWordsList,
            List<AudioMultiReference> audioReferenceList,
            List<string> audioKeysList,
            bool synthesizeMissingAudio)
        {
            WalkerOperationName = "CollectDictionaryItemsFromTree";
            WalkerNotFoundOnly = notFoundOnly;
            WalkerTargetLanguageID = targetLanguageID;
            WalkerWordsList = dictionaryWordsList;
            WalkerWordsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WalkerAudioReferenceList = audioReferenceList;
            WalkerAudioSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WalkerAudioKeys = audioKeysList;
            WalkerSynthesizeMissingAudio = synthesizeMissingAudio;

            ItemWalker<NodeUtilities> walker = new ItemWalker<NodeUtilities>();
            walker.VisitStudyItemFunction = CollectDictionaryItemsFromStudyItem;
            walker.VisitStudyListPostFunction = StudyListUpdateCheck;

            WalkTrees(
                WalkerOperationName,
                tree,
                label,
                walker,
                doInBackground);
        }

        public void CollectDictionaryItemsFromNode(
            BaseObjectNode node,
            bool notFoundOnly,
            LanguageID targetLanguageID,
            bool doInBackground,
            List<string> dictionaryWordsList,
            List<AudioMultiReference> audioReferenceList,
            List<string> audioKeysList,
            bool synthesizeMissingAudio)
        {
            WalkerOperationName = "CollectDictionaryItemsFromNode";
            WalkerNotFoundOnly = notFoundOnly;
            WalkerTargetLanguageID = targetLanguageID;
            WalkerWordsList = dictionaryWordsList;
            WalkerWordsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WalkerAudioReferenceList = audioReferenceList;
            WalkerAudioSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WalkerAudioKeys = audioKeysList;
            WalkerSynthesizeMissingAudio = synthesizeMissingAudio;

            ItemWalker<NodeUtilities> walker = new ItemWalker<NodeUtilities>();
            walker.VisitStudyItemFunction = CollectDictionaryItemsFromStudyItem;
            walker.VisitStudyListPostFunction = StudyListUpdateCheck;

            WalkNodes(
                WalkerOperationName,
                node,
                walker,
                doInBackground);
        }

        public static bool StudyListUpdateCheck(ContentStudyList studyList,
            ItemWalker<NodeUtilities> walker, NodeUtilities context)
        {
            if (studyList.Modified)
            {
                if (!context.UpdateContentStorage(studyList.Content, true))
                    context.Error = context.Error + "\n" + context.Error;
            }

            return true;
        }

        public static bool CollectDictionaryItemsFromStudyItem(MultiLanguageItem multiLanguageItem,
            ItemWalker<NodeUtilities> walker, NodeUtilities context)
        {
            string operationName = context.WalkerOperationName;
            LanguageID targetLanguageID = context.WalkerTargetLanguageID;
            string text = multiLanguageItem.Text(targetLanguageID);
            string currentMessage = context.S("Language item") + ": " + text;

            ApplicationData.Global.SetOperationStatusLabel(context.UserName, operationName, currentMessage);

            switch (walker.Content.ContentType)
            {
                case "Words":
                case "Characters":
                    context.CollectDictionaryItemsFromWordItem(targetLanguageID, multiLanguageItem);
                    break;
                default:
                    context.CollectDictionaryItemsFromSentenceItem(targetLanguageID, multiLanguageItem);
                    break;
            }

            return true;
        }

        public void CollectDictionaryItemsFromSentenceItem(
            LanguageID languageID,
            MultiLanguageItem sentenceItem)
        {
            if (sentenceItem == null)
                return;

            LanguageItem languageItem = sentenceItem.LanguageItem(languageID);

            if (languageItem == null)
                return;

            if (!languageItem.HasWordRuns())
                return;

            foreach (TextRun wordRun in languageItem.WordRuns)
            {
                string word = languageItem.GetRunText(wordRun);

                if (!String.IsNullOrEmpty(word))
                    CollectDictionaryItemsFromWord(languageID, word);
            }
        }

        public void CollectDictionaryItemsFromWordItem(
            LanguageID languageID,
            MultiLanguageItem wordItem)
        {
            if (wordItem == null)
                return;

            LanguageItem languageItem = wordItem.LanguageItem(languageID);

            if (languageItem == null)
                return;

            string word = languageItem.Text;

            if (!String.IsNullOrEmpty(word))
                CollectDictionaryItemsFromWord(languageID, word);
        }

        public void CollectDictionaryItemsFromWord(
            LanguageID languageID,
            string word)
        {
            bool inWordList = WalkerWordsSet.Contains(word);
            bool inAudioList = WalkerAudioSet.Contains(word);
            string canonicalWord = word;

            if (!inWordList)
            {
                if (WalkerNotFoundOnly)
                {
                    LanguageTool tool = GetLanguageTool(languageID);

                    if (tool != null)
                    {
                        bool isInflection;

                        DictionaryEntry dictionaryEntry = tool.LookupDictionaryEntry(
                            word,
                            MatchCode.Exact,
                            languageID,
                            null,
                            out isInflection);

                        if (dictionaryEntry == null)
                        {
                            WalkerWordsList.Add(word);
                            WalkerWordsSet.Add(word);
                        }
                    }
                    else
                    {
                        List<DictionaryEntry> dictionaryEntries = Repositories.Dictionary.Lookup(
                            word,
                            MatchCode.Exact,
                            languageID,
                            0,
                            0);

                        if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                        {
                            WalkerWordsList.Add(word);
                            WalkerWordsSet.Add(word);
                        }
                    }
                }
                else
                {
                    WalkerWordsList.Add(word);
                    WalkerWordsSet.Add(word);
                }
            }

            if (!inAudioList)
            {
                string audioUrl;
                string owner = UserName;
                string mimeType = "audio/mpeg3";
                string fileFriendlyName = MediaUtilities.FileFriendlyName(
                    word,
                    MediaUtilities.MaxMediaFileNameLength);

                // Returns true if audio file exists.
                if (ProcessAudio(
                    word,                           // string normalKey
                    languageID,                     // LanguageID languageID
                    String.Empty,                   // string alternateKey
                    null,                           // LanguageID alternateLanguageID
                    true,                           // bool existingOnly
                    String.Empty,                   // string mainAudioUrl
                    WalkerSynthesizeMissingAudio,   // bool isSynthesizeMissingAudio
                    false,                          // bool useRemoteMedia
                    null,                           // List<LanguageString> audioRecordKeys
                    out audioUrl))                  // out string audioUrl
                {
                    if (!WalkerNotFoundOnly)
                    {
                        AudioInstance audioInstance = new AudioInstance(
                            word,
                            owner,
                            mimeType,
                            fileFriendlyName + ".mp3",
                            AudioReference.SynthesizedSourceName,
                            null);
                        List<AudioInstance> audioInstances = new List<AudioInstance>();
                        AudioMultiReference audioReference = new AudioMultiReference(
                            word,
                            languageID,
                            audioInstances);
                        if (WalkerAudioReferenceList != null)
                            WalkerAudioReferenceList.Add(audioReference);
                        WalkerAudioSet.Add(word);
                        if (WalkerAudioKeys != null)
                            WalkerAudioKeys.Add(word);
                    }
                }
                else
                {
                    AudioInstance audioInstance = new AudioInstance(
                        word,
                        owner,
                        mimeType,
                        fileFriendlyName + ".mp3",
                        AudioReference.SynthesizedSourceName,
                        null);
                    List<AudioInstance> audioInstances = new List<AudioInstance>();
                    AudioMultiReference audioReference = new AudioMultiReference(
                        word,
                        languageID,
                        audioInstances);
                    if (WalkerAudioReferenceList != null)
                        WalkerAudioReferenceList.Add(audioReference);
                    WalkerAudioSet.Add(word);
                    if (WalkerAudioKeys != null)
                        WalkerAudioKeys.Add(word);
                }
            }
        }

        public void WalkTrees(
            string operationName,
            BaseObjectNodeTree tree,        // Null if doing all trees.
            string label,                   // "Course" or "Plan".
            ItemWalker<NodeUtilities> walker,
            bool doInBackground)
        {
            if (doInBackground)
                WalkTreesBackground(operationName, tree, label, walker);
            else
                WalkTreesForeground(tree, label, walker);
        }

        public void WalkTreesForeground(
            BaseObjectNodeTree tree,        // Null if doing all trees.
            string label,                   // "Course" or "Plan".
            ItemWalker<NodeUtilities> walker)
        {
            if (tree != null)
            {
                tree.ResolveReferences(Repositories, false, true);

                walker.WalkTree(tree, this);

                if (tree.Modified)
                {
                    UpdateTree(tree, false, false);
                    PutMessage(label + " modified", tree.GetTitleString(UILanguageID));
                }
            }
            else
            {
                int count = Repositories.Courses.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    tree = Repositories.Courses.GetIndexed(index);

                    if (tree != null)
                    {
                        tree.ResolveReferences(Repositories, false, true);

                        walker.WalkTree(tree, this);

                        if (tree.Modified)
                        {
                            UpdateTree(tree, false, false);
                            PutMessage(label + " modified", tree.GetTitleString(UILanguageID));
                        }
                    }
                }
            }
        }

        public void WalkTreesBackground(
            string operationName,
            BaseObjectNodeTree tree,        // Null if doing all trees.
            string label,                   // "Course" or "Plan".
            ItemWalker<NodeUtilities> walker)
        {
            if (GetOperationStatusState(operationName) == "Running")
            {
                PutError(operationName + " operation already running.  Click cancel to abort.");
                return;
            }

            WalkTreesThread(
                operationName,
                tree,
                label,
                walker,
                Repositories,
                this);
        }

        public static string FixupUser;
        public static NodeUtilities FixupNodeUtilities;
        public static string FixupTreeMessage;

        public static void WalkTreesThread(
            string operationName,
            BaseObjectNodeTree tree,        // Null if doing all trees.
            string label,                   // "Course" or "Plan".
            ItemWalker<NodeUtilities> walker,
            IMainRepository repositories,
            NodeUtilities nodeUtilities)
        {
            string userName = nodeUtilities.UserRecord.UserName;
            string currentMessage = String.Empty;

            FixupUser = userName;
            FixupNodeUtilities = nodeUtilities;

            ApplicationData.Global.SetOperationStatus(userName, operationName, "Running", "Not started.");
            ApplicationData.Global.RunAsThread(
                threadOp =>
                {
                    string error = String.Empty;
                    ApplicationData.Global.SetOperationStatusLabel(userName, operationName, "Started in background.");
                    try
                    {
                        if (tree != null)
                        {
                            currentMessage = operationName + " on tree 1 of 1 " + tree.GetTitleString();
                            ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);
                            tree.ResolveReferences(repositories, false, true);

                            walker.WalkTree(tree, nodeUtilities);

                            if (tree.Modified)
                            {
                                nodeUtilities.UpdateTree(tree, false, false);
                                FixupTreeMessage = currentMessage = tree.GetTitleString(nodeUtilities.UILanguageID) + " modified.";
                            }
                            else
                                FixupTreeMessage = currentMessage = tree.GetTitleString(nodeUtilities.UILanguageID) + " not changed.";

                            currentMessage = operationName + " on " + tree.GetTitleString();
                            nodeUtilities.Message = nodeUtilities.Message + "\r\n" + currentMessage;
                            ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);
                        }
                        else
                        {
                            int count = repositories.Courses.Count();
                            int index;

                            for (index = 0; index < count; index++)
                            {
                                tree = repositories.Courses.GetIndexed(index);

                                if (tree != null)
                                {
                                    FixupTreeMessage = currentMessage = operationName + " on " + index.ToString() + " of " +
                                        count.ToString() + ": " + tree.GetTitleString();

                                    ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);

                                    tree.ResolveReferences(repositories, false, true);

                                    walker.WalkTree(tree, nodeUtilities);

                                    if (tree.Modified)
                                    {
                                        nodeUtilities.UpdateTree(tree, false, false);
                                        FixupTreeMessage = currentMessage = nodeUtilities.S(label + " modified") + ": " + tree.GetTitleString(nodeUtilities.UILanguageID);
                                    }
                                    else
                                        FixupTreeMessage = currentMessage = nodeUtilities.S(label + " not modified") + ": " + tree.GetTitleString(nodeUtilities.UILanguageID);

                                    nodeUtilities.Message = nodeUtilities.Message + "\r\n" + index.ToString() + ". " + currentMessage;
                                    ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);
                                }

                                nodeUtilities.CheckForCancel(operationName);
                            }
                        }
                    }
                    catch (OperationCanceledException exc)
                    {
                        error = exc.Message;
                        if (exc.InnerException != null)
                            error = error + ": " + exc.InnerException.Message;
                    }
                    catch (Exception exc)
                    {
                        error = exc.Message;
                        if (exc.InnerException != null)
                            error = error + ": " + exc.InnerException.Message;
                    }


                    ApplicationData.Global.SetOperationStatus(
                        userName,
                        operationName,
                        "Completed",
                        (!String.IsNullOrEmpty(error) ? error : operationName + " completed."));
                },
                continueOp =>
                {
                }
            );
        }

        public void WalkNodes(
            string operationName,
            BaseObjectNode node,
            ItemWalker<NodeUtilities> walker,
            bool doInBackground)
        {
            if (doInBackground)
                WalkNodesBackground(operationName, node, walker);
            else
                WalkNodesForeground(node, walker);
        }

        public void WalkNodesForeground(
            BaseObjectNode node,
            ItemWalker<NodeUtilities> walker)
        {
            node.ResolveReferences(Repositories, false, true);
            walker.WalkNode(node, this);

            if (node.Modified)
            {
                UpdateTree(node.Tree, false, false);
                PutMessage(node.Label + " modified", node.GetTitleString(UILanguageID));
            }
        }

        public void WalkNodesBackground(
            string operationName,
            BaseObjectNode node,
            ItemWalker<NodeUtilities> walker)
        {
            if (GetOperationStatusState(operationName) == "Running")
            {
                PutError(operationName + " operation already running.  Click cancel to abort.");
                return;
            }

            WalkNodesThread(
                operationName,
                node,
                walker,
                Repositories,
                this);
        }

        public static void WalkNodesThread(
            string operationName,
            BaseObjectNode node,
            ItemWalker<NodeUtilities> walker,
            IMainRepository repositories,
            NodeUtilities nodeUtilities)
        {
            string userName = nodeUtilities.UserRecord.UserName;
            string currentMessage = String.Empty;

            FixupUser = userName;
            FixupNodeUtilities = nodeUtilities;

            ApplicationData.Global.SetOperationStatus(userName, operationName, "Running", "Not started.");
            ApplicationData.Global.RunAsThread(
                threadOp =>
                {
                    string error = String.Empty;
                    ApplicationData.Global.SetOperationStatusLabel(userName, operationName, "Started in background.");
                    try
                    {
                        currentMessage = operationName + " on node: " + node.GetTitleString();
                        ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);
                        node.ResolveReferences(repositories, false, true);

                        walker.WalkNode(node, nodeUtilities);

                        if (node.Modified)
                        {
                            nodeUtilities.UpdateTree(node.Tree, false, false);
                            FixupTreeMessage = currentMessage = node.GetTitleString(nodeUtilities.UILanguageID) + " modified.";
                        }
                        else
                            FixupTreeMessage = currentMessage = node.GetTitleString(nodeUtilities.UILanguageID) + " not changed.";

                        currentMessage = operationName + " on " + node.GetTitleString();
                        nodeUtilities.Message = nodeUtilities.Message + "\r\n" + currentMessage;
                        ApplicationData.Global.SetOperationStatusLabel(userName, operationName, currentMessage);
                    }
                    catch (OperationCanceledException exc)
                    {
                        error = exc.Message;
                        if (exc.InnerException != null)
                            error = error + ": " + exc.InnerException.Message;
                    }
                    catch (Exception exc)
                    {
                        error = exc.Message;
                        if (exc.InnerException != null)
                            error = error + ": " + exc.InnerException.Message;
                    }


                    ApplicationData.Global.SetOperationStatus(
                        userName,
                        operationName,
                        "Completed",
                        (!String.IsNullOrEmpty(error) ? error : operationName + " completed."));
                },
                continueOp =>
                {
                }
            );
        }
    }
}

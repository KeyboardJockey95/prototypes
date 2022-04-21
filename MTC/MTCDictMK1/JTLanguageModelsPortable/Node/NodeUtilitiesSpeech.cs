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
        public static bool UseGoogleTextToSpeech = ApplicationData.UseGoogleSpeechToTextAPI;

        public string GetDefaultVoiceName(LanguageID languageID)
        {
            string voice = String.Empty;

            if (languageID == null)
                return voice;

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(languageID);

            if (languageDescription != null)
            {
                if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                    voice = languageDescription.PreferedVoiceName;
                else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                    voice = languageDescription.VoiceNames[0];
            }

            return voice;
        }

        public bool AddSynthesizedVoiceToNodeAndChildren(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            if (String.IsNullOrEmpty(voice))
            {
                PutError("No voice is selected.");
                return false;
            }

            try
            {
                returnValue = AddSynthesizedVoiceToNodeAndChildren(tree, node, languageID,
                    voice, speed, overwriteMedia, filterAsides, nodeSelectFlags, contentSelectFlags,
                    speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToNodeAndChildren(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags,
            ITextToSpeech speechEngine)
        {
            bool returnValue = true;

            if ((nodeSelectFlags == null) || nodeSelectFlags[node.KeyInt])
            {
                if (!AddSynthesizedVoiceToNode(tree, node, languageID, voice, speed, overwriteMedia,
                        filterAsides, contentSelectFlags, false, speechEngine))
                    returnValue = false;
            }

            if (!AddSynthesizedVoiceToNodeChildren(tree, node, languageID, voice, speed, overwriteMedia,
                    filterAsides, nodeSelectFlags, contentSelectFlags, speechEngine))
                returnValue = false;

            return returnValue;
        }

        public bool AddSynthesizedVoiceToNodeChildren(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            if (String.IsNullOrEmpty(voice))
            {
                PutError("No voice is selected.");
                return false;
            }

            try
            {
                returnValue = AddSynthesizedVoiceToNodeChildren(tree, node, languageID,
                    voice, speed, overwriteMedia, filterAsides, nodeSelectFlags, contentSelectFlags, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToNodeChildren(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags,
            ITextToSpeech speechEngine)
        {
            bool returnValue = true;

            List<BaseObjectNode> nodeChildren = node.Children;

            if (nodeChildren == null)
                return returnValue;

            foreach (BaseObjectNode childNode in nodeChildren)
            {
                if ((nodeSelectFlags == null) || nodeSelectFlags[childNode.KeyInt])
                {
                    if (!AddSynthesizedVoiceToNodeAndChildren(tree, childNode, languageID, voice, speed, overwriteMedia,
                            filterAsides, nodeSelectFlags, contentSelectFlags, speechEngine))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToNode(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<string, bool> contentSelectFlags,
            bool recurse)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            if (String.IsNullOrEmpty(voice))
            {
                PutError("No voice is selected.");
                return false;
            }

            try
            {
                returnValue = AddSynthesizedVoiceToNode(tree, node, languageID,
                    voice, speed, overwriteMedia, filterAsides, contentSelectFlags, recurse, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToNode(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            Dictionary<string, bool> contentSelectFlags,
            bool recurse,
            ITextToSpeech speechEngine)
        {
            List<BaseObjectContent> contentList = node.GetContentWithStorageClass(ContentClassType.StudyList);
            bool returnValue = true;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    string contentKey = content.KeyString;
                    bool useIt = false;

                    if ((contentSelectFlags != null) && (contentSelectFlags.Count() != 0))
                    {
                        if (!contentSelectFlags.TryGetValue(contentKey, out useIt))
                        {
                            BaseObjectContent aContent = content;
                            while (aContent.HasContentParent())
                            {
                                aContent = aContent.ContentParent;
                                if (contentSelectFlags.TryGetValue(aContent.KeyString, out useIt))
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (content.ContentType)
                        {
                            case "Transcript":
                            case "Text":
                            case "Sentences":
                            case "Words":
                            case "Characters":
                            case "Expansion":
                            case "Exercises":
                            case "Notes":
                                useIt = true;
                                break;
                            default:
                                break;
                        }
                    }

                    if (useIt)
                    {
                        if (!AddSynthesizedVoiceToContent(tree, node, content, languageID, voice, speed, overwriteMedia,
                                filterAsides, false, speechEngine))
                            returnValue = false;
                    }
                }
            }

            if (recurse)
            {
                if (node.HasChildren())
                {
                    foreach (BaseObjectNode childNode in node.Children)
                    {
                        if (!AddSynthesizedVoiceToNode(tree, childNode, languageID,
                                voice, speed, overwriteMedia, filterAsides, contentSelectFlags, recurse, speechEngine))
                            returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToContent(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            bool recurse,
            ITextToSpeech speechEngine)
        {
            ContentStudyList studyList = content.ContentStorageStudyList;
            bool returnValue = true;

            if (studyList != null)
            {
                if (recurse)
                {
                    ItemWalker<WalkContentData> itemWalker = new ItemWalker<WalkContentData>();
                    itemWalker.VisitContentFunction = UpdateContentStorageCheckFunction;
                    WalkContentData context = new WalkContentData(content.KeyString);
                    itemWalker.VisitNode(content.Node, context);

                    foreach (BaseObjectContent aContent in context.ContentList)
                    {
                        if (!AddSynthesizedVoiceToStudyList(tree, node, aContent, studyList, languageID,
                                voice, speed, overwriteMedia, filterAsides, null, false, speechEngine))
                            returnValue = false;

                        bool doAMessage = (aContent == content);

                        if (!UpdateContentStorageCheck(aContent, doAMessage))
                            returnValue = false;
                    }
                }
                else
                {
                    if (!AddSynthesizedVoiceToStudyList(tree, node, content, studyList, languageID,
                            voice, speed, overwriteMedia, filterAsides, null, recurse, speechEngine))
                        returnValue = false;

                    if (!UpdateContentStorageCheck(content, true))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyList(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            ContentStudyList studyList,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            bool recurse)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            try
            {
                returnValue = AddSynthesizedVoiceToStudyList(tree, node, content, studyList, languageID,
                    voice, speed, overwriteMedia, filterAsides, studyItemFlags, recurse, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyList(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            ContentStudyList studyList,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            bool recurse,
            ITextToSpeech speechEngine)
        {
            LanguageDescription languageDescription;
            List<LanguageID> languageIDs = new List<LanguageID>();
            bool returnValue = true;

            switch (languageID.LanguageCode)
            {
                case "(my languages)":
                    languageIDs = UserProfile.LanguageIDs;
                    break;
                case "(target languages)":
                    languageIDs = UserProfile.TargetLanguageIDs;
                    break;
                case "(host languages)":
                    languageIDs = UserProfile.HostLanguageIDs;
                    break;
                case "(any)":
                case "(all languages)":
                    languageIDs = content.ExpandLanguageIDs(UserProfile);
                    break;
                default:
                    languageIDs = new List<LanguageID>(1) { languageID };
                    break;
            }

            if (!UseGoogleTextToSpeech)
            {
                int languageIndex;
                int languageCount = languageIDs.Count();

                for (languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
                {
                    LanguageID lid = languageIDs[languageIndex];

                    languageDescription = LanguageLookup.GetLanguageDescription(lid);

                    if (languageDescription != null)
                    {
                        if ((languageDescription.VoiceNames == null) || (languageDescription.VoiceNames.Count() == 0))
                            languageIDs.Remove(lid);
                        else
                        {
                            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(lid);

                            if (alternateLanguageIDs != null)
                            {
                                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                                {
                                    if (languageIDs.Contains(alternateLanguageID))
                                        languageIDs.Remove(alternateLanguageID);
                                }

                                if (languageIndex > languageIDs.Count())
                                    languageIndex = languageIDs.Count();
                            }
                        }
                    }
                }
            }

            if (languageIDs.Count() == 0)
                return returnValue;
            else if (languageIDs.Count() == 1)
            {
                LanguageID voiceLanguageID = languageIDs[0];

                if (voice == "(default)")
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(voiceLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }
                }

                if (voice != "(none)")
                {
                    if (!speechEngine.SetVoice(voice, voiceLanguageID))
                    {
                        PutError("Error selecting voice.");
                        return false;
                    }
                }

                if (speed != 0)
                    speechEngine.SetSpeed(speed);

                List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(voiceLanguageID);

                returnValue = AddSynthesizedVoiceToStudyListLanguage(tree, node, content, studyList,
                    voiceLanguageID, alternateLanguageIDs, voice, speed, overwriteMedia, filterAsides,
                    studyItemFlags, recurse, speechEngine);

                if (!UpdateContentStorageRecurseCheck(content))
                    returnValue = false;
            }
            else
            {
                foreach (LanguageID aLanguageID in languageIDs)
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(aLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }

                    if (!AddSynthesizedVoiceToStudyList(tree, node, content, studyList,
                            aLanguageID, voice, speed, overwriteMedia, filterAsides, studyItemFlags, recurse, speechEngine))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyListLanguage(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            ContentStudyList studyList,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            bool recurse,
            ITextToSpeech speechEngine)
        {
            List<MultiLanguageItem> studyItems = new List<MultiLanguageItem>();

            studyList.CollectStudyItems(studyItems, recurse);

            bool returnValue = AddSynthesizedVoiceToStudyItemsLanguage(studyItems,
                languageID, alternateLanguageIDs, content.Owner,
                voice, speed, overwriteMedia, filterAsides, studyItemFlags,
                content.MediaTildeUrl, speechEngine);

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyItemsListLanguage(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            try
            {
                LanguageDescription languageDescription;
                LanguageID voiceLanguageID = languageID;

                if (voice == "(default)")
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(voiceLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }
                }

                if (voice != "(none)")
                {
                    if (!speechEngine.SetVoice(voice, voiceLanguageID))
                    {
                        PutError("Error selecting voice.");
                        return false;
                    }
                }

                if (speed != 0)
                    speechEngine.SetSpeed(speed);

                returnValue = AddSynthesizedVoiceToStudyItemsLanguage(studyItems,
                    languageID, alternateLanguageIDs, content.Owner,
                    voice, speed, overwriteMedia, filterAsides, null,
                    content.MediaTildeUrl, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyListItem(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            ContentStudyList studyList,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            int studyItemIndex,
            int sentenceIndex)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            try
            {
                returnValue = AddSynthesizedVoiceToStudyListItem(tree, node, content, studyList, languageID,
                    voice, speed, overwriteMedia, filterAsides, studyItemIndex, sentenceIndex, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyListItem(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content,
            ContentStudyList studyList,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            int studyItemIndex,
            int sentenceIndex,
            ITextToSpeech speechEngine)
        {
            LanguageDescription languageDescription;
            List<LanguageID> languageIDs = new List<LanguageID>();
            bool returnValue = true;

            switch (languageID.LanguageCode)
            {
                case "(my languages)":
                    languageIDs = UserProfile.LanguageIDs;
                    break;
                case "(target languages)":
                    languageIDs = UserProfile.TargetLanguageIDs;
                    break;
                case "(host languages)":
                    languageIDs = UserProfile.HostLanguageIDs;
                    break;
                case "(any)":
                case "(all languages)":
                    languageIDs = content.ExpandLanguageIDs(UserProfile);
                    break;
                default:
                    languageIDs = new List<LanguageID>(1) { languageID };
                    break;
            }

            if (!UseGoogleTextToSpeech)
            {
                int languageIndex;
                int languageCount = languageIDs.Count();

                for (languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
                {
                    LanguageID lid = languageIDs[languageIndex];

                    languageDescription = LanguageLookup.GetLanguageDescription(lid);

                    if (languageDescription != null)
                    {
                        if ((languageDescription.VoiceNames == null) || (languageDescription.VoiceNames.Count() == 0))
                            languageIDs.Remove(lid);
                        else
                        {
                            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(lid);

                            if (alternateLanguageIDs != null)
                            {
                                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                                {
                                    if (languageIDs.Contains(alternateLanguageID))
                                        languageIDs.Remove(alternateLanguageID);
                                }

                                if (languageIndex > languageIDs.Count())
                                    languageIndex = languageIDs.Count();
                            }
                        }
                    }
                }
            }

            if (languageIDs.Count() == 0)
                return returnValue;
            else if (languageIDs.Count() == 1)
            {
                LanguageID voiceLanguageID = languageIDs[0];

                if (voice == "(default)")
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(voiceLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }
                }

                if (voice != "(none)")
                {
                    if (!speechEngine.SetVoice(voice, voiceLanguageID))
                    {
                        PutError("Error selecting voice.");
                        return false;
                    }
                }

                if (speed != 0)
                    speechEngine.SetSpeed(speed);

                List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(voiceLanguageID);

                MultiLanguageItem studyItem = studyList.GetStudyItemIndexed(studyItemIndex, true);

                if (studyItem != null)
                {
                    if (sentenceIndex == -1)
                        returnValue = AddSynthesizedAudioToStudyItem(studyItem,
                                voiceLanguageID, alternateLanguageIDs, studyItemIndex, overwriteMedia,
                                filterAsides, content.MediaTildeUrl, content.Owner, speechEngine);
                    else
                        returnValue = AddSynthesizedAudioToStudyItemSentence(studyItem,
                                voiceLanguageID, alternateLanguageIDs, studyItemIndex, sentenceIndex, overwriteMedia,
                                filterAsides, content.MediaTildeUrl, content.Owner, speechEngine);
                }
            }
            else
            {
                MultiLanguageItem studyItem = studyList.GetStudyItemIndexed(studyItemIndex, true);

                if (studyItem != null)
                {
                    foreach (LanguageID aLanguageID in languageIDs)
                    {
                        string aVoice = voice;

                        if (aVoice == "(default)")
                        {
                            languageDescription = LanguageLookup.GetLanguageDescription(aLanguageID);

                            if (languageDescription != null)
                            {
                                if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                                    aVoice = languageDescription.PreferedVoiceName;
                                else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                                    aVoice = languageDescription.VoiceNames[0];
                            }
                        }

                        if (aVoice != "(none)")
                        {
                            if (!speechEngine.SetVoice(aVoice, aLanguageID))
                            {
                                PutError("Error selecting voice.");
                                return false;
                            }
                        }

                        if (speed != 0)
                            speechEngine.SetSpeed(speed);

                        List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(aLanguageID);

                        if (sentenceIndex == -1)
                            returnValue = AddSynthesizedAudioToStudyItem(studyItem,
                                    aLanguageID, null, studyItemIndex, overwriteMedia,
                                    filterAsides, content.MediaTildeUrl, content.Owner, speechEngine);
                        else
                            returnValue = AddSynthesizedAudioToStudyItemSentence(studyItem,
                                    aLanguageID, null, studyItemIndex, sentenceIndex, overwriteMedia,
                                    filterAsides, content.MediaTildeUrl, content.Owner, speechEngine);
                    }
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToStudyItemsLanguage(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            string owner,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            string defaultDirectory,
            ITextToSpeech speechEngine)
        {
            bool returnValue = true;

            if (studyItems != null)
            {
                int index = 0;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.Count() == 0)
                        continue;

                    if ((studyItemFlags == null) || studyItemFlags[index])
                    {
                        string directory = studyItem.MediaTildeUrl;

                        if (String.IsNullOrEmpty(directory))
                            directory = defaultDirectory;

                        returnValue = AddSynthesizedAudioToStudyItem(studyItem,
                                languageID, alternateLanguageIDs, index, overwriteMedia,
                                filterAsides, directory, owner, speechEngine)
                            && returnValue;
                    }

                    index++;
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedAudioToStudyItem(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            int index,
            bool overwriteMedia,
            bool filterAsides,
            string directory,
            string owner,
            ITextToSpeech speechEngine)
        {
            string studyItemKey = studyItem.KeyString;
            bool returnValue = true;

            if (studyItem.Count() == 0)
                return returnValue;

            if (String.IsNullOrEmpty(directory))
            {
                PutError("Directory is empty.");
                return false;
            }

            studyItem.SentenceRunCheck();

            LanguageItem languageItem = studyItem.LanguageItem(languageID);

            if (languageItem == null)
                return true;

            if (!languageItem.HasSentenceRuns())
                return true;

            //ApplicationData.Global.Sleep(100);
            CheckForCancel("AddSynthesizedVoice");
            UpdateBackgroundStatus("AddSynthesizedVoice", null, studyItem, languageID);

            int sentenceRunIndex = 0;

            foreach (TextRun sentenceRun in languageItem.SentenceRuns)
            {
                if (sentenceRun.Length == 0)
                    continue;

                string fileName = null;
                string urlPath = null;
                string filePath = null;
                MediaRun theMediaRun = null;
                bool dontDoIt = false;

                if ((sentenceRun.MediaRuns != null) && (sentenceRun.MediaRuns.Count() != 0))
                {
                    foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                    {
                        switch (mediaRun.KeyString)
                        {
                            case "Audio":
                                if (mediaRun.IsReference)
                                {
                                    dontDoIt = true;
                                    break;
                                }
                                theMediaRun = mediaRun;
                                fileName = mediaRun.FileName;
                                if (String.IsNullOrEmpty(mediaRun.FileName))
                                {
                                    fileName = MediaUtilities.ComposeStudyItemFileName(
                                        studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio", ".mp3");
                                    urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                                    filePath = ApplicationData.MapToFilePath(urlPath);
                                    mediaRun.FileName = fileName;
                                    string testFileUrl = mediaRun.GetUrl(directory);
                                    string testFilePath = ApplicationData.MapToFilePath(testFileUrl);
                                    if (FileSingleton.Exists(testFilePath))
                                        dontDoIt = true;
                                    urlPath = testFileUrl;
                                    filePath = testFilePath;
                                }
                                else if (mediaRun.FileName.StartsWith("http://") || mediaRun.FileName.StartsWith("https://"))
                                    dontDoIt = true;
                                else
                                {
                                    string testFileUrl = mediaRun.GetUrl(directory);
                                    string testFilePath = ApplicationData.MapToFilePath(testFileUrl);
                                    if (FileSingleton.Exists(testFilePath))
                                        dontDoIt = true;
                                    urlPath = testFileUrl;
                                    filePath = testFilePath;
                                }
                                break;
                            case "SlowAudio":
                            case "Video":
                            case "SlowVideo":
                                dontDoIt = true;
                                break;
                            case "Picture":
                            case "BigPicture":
                            case "SmallPicture":
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (!overwriteMedia)
                {
                    if (dontDoIt)
                        continue;
                }

                // Avoid file name collisions.
                if (theMediaRun == null)
                {
                    int retry = 1;

                    fileName = MediaUtilities.ComposeStudyItemFileName(
                        studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio", ".mp3");
                    urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                    filePath = ApplicationData.MapToFilePath(urlPath);

                    while (FileSingleton.Exists(filePath))
                    {
                        fileName = MediaUtilities.ComposeStudyItemFileName(
                            studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio",
                            "_r" + retry.ToString() + ".mp3");
                        urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                        filePath = ApplicationData.MapToFilePath(urlPath);
                        retry++;
                    }
                }

                if (overwriteMedia || !FileSingleton.Exists(filePath))
                {
                    string text = MediaUtilities.FilterTextBeforeSpeech(
                        languageItem.GetRunText(sentenceRun), languageID, UserProfile, filterAsides);
                    string message;

                    if (!speechEngine.SpeakToFile(text, filePath, "audio/mpeg3", out message))
                    {
                        PutError(message);
                        //Error += S("\nError speaking to file: " + filePath + "\n");
                        return false;
                    }
                }

                AddAudioToTextRun(sentenceRun, "Audio", fileName, overwriteMedia, owner);

                if (alternateLanguageIDs != null)
                {
                    foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                    {
                        LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                        if (alternateLanguageItem == null)
                            continue;

                        if (alternateLanguageItem.SentenceRunCount() != languageItem.SentenceRunCount())
                            continue;

                        TextRun alternateRun = alternateLanguageItem.GetSentenceRun(sentenceRunIndex);

                        AddAudioToTextRun(alternateRun, "Audio", fileName, overwriteMedia, owner);
                    }
                }

                sentenceRunIndex++;
            }

            return returnValue;
        }

        public bool AddSynthesizedAudioToStudyItemSentence(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            int index,
            int sentenceIndex,
            bool overwriteMedia,
            bool filterAsides,
            string directory,
            string owner,
            ITextToSpeech speechEngine)
        {
            string studyItemKey = studyItem.KeyString;
            bool returnValue = true;

            if (studyItem.Count() == 0)
                return returnValue;

            if (String.IsNullOrEmpty(directory))
            {
                PutError("Directory is empty.");
                return false;
            }

            studyItem.SentenceRunCheck();

            LanguageItem languageItem = studyItem.LanguageItem(languageID);

            if (languageItem == null)
                return true;

            if (!languageItem.HasSentenceRuns())
                return true;

            //ApplicationData.Global.Sleep(100);
            CheckForCancel("AddSynthesizedVoice");
            UpdateBackgroundStatus("AddSynthesizedVoice", null, studyItem, languageID);

            int sentenceRunIndex = 0;

            TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

            if ((sentenceRun != null) && (sentenceRun.Length != 0))
            {
                string fileName = null;
                string urlPath = null;
                string filePath = null;
                MediaRun theMediaRun = null;
                bool dontDoIt = false;

                if ((sentenceRun.MediaRuns != null) && (sentenceRun.MediaRuns.Count() != 0))
                {
                    foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                    {
                        switch (mediaRun.KeyString)
                        {
                            case "Audio":
                                if (mediaRun.IsReference)
                                {
                                    dontDoIt = true;
                                    break;
                                }
                                theMediaRun = mediaRun;
                                fileName = mediaRun.FileName;
                                if (String.IsNullOrEmpty(mediaRun.FileName))
                                {
                                    fileName = MediaUtilities.ComposeStudyItemFileName(
                                        studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio", ".mp3");
                                    urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                                    filePath = ApplicationData.MapToFilePath(urlPath);
                                    mediaRun.FileName = fileName;
                                    string testFileUrl = mediaRun.GetUrl(directory);
                                    string testFilePath = ApplicationData.MapToFilePath(testFileUrl);
                                    if (FileSingleton.Exists(testFilePath))
                                        dontDoIt = true;
                                    urlPath = testFileUrl;
                                    filePath = testFilePath;
                                }
                                else if (mediaRun.FileName.StartsWith("http://") || mediaRun.FileName.StartsWith("https://"))
                                    dontDoIt = true;
                                else
                                {
                                    string testFileUrl = mediaRun.GetUrl(directory);
                                    string testFilePath = ApplicationData.MapToFilePath(testFileUrl);
                                    if (FileSingleton.Exists(testFilePath))
                                        dontDoIt = true;
                                    urlPath = testFileUrl;
                                    filePath = testFilePath;
                                }
                                break;
                            case "SlowAudio":
                            case "Video":
                            case "SlowVideo":
                                dontDoIt = true;
                                break;
                            case "Picture":
                            case "BigPicture":
                            case "SmallPicture":
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (overwriteMedia || !dontDoIt)
                {
                    // Avoid file name collisions.
                    if (theMediaRun == null)
                    {
                        int retry = 1;

                        fileName = MediaUtilities.ComposeStudyItemFileName(
                            studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio", ".mp3");
                        urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                        filePath = ApplicationData.MapToFilePath(urlPath);

                        while (FileSingleton.Exists(filePath))
                        {
                            fileName = MediaUtilities.ComposeStudyItemFileName(
                                studyItemKey, sentenceRunIndex, languageItem.LanguageID, "Audio",
                                "_r" + retry.ToString() + ".mp3");
                            urlPath = MediaUtilities.ConcatenateUrlPath(directory, fileName);
                            filePath = ApplicationData.MapToFilePath(urlPath);
                            retry++;
                        }
                    }

                    if (overwriteMedia || !FileSingleton.Exists(filePath))
                    {
                        string text = MediaUtilities.FilterTextBeforeSpeech(
                            languageItem.GetRunText(sentenceRun), languageID, UserProfile, filterAsides);
                        string message;

                        if (!speechEngine.SpeakToFile(text, filePath, "audio/mpeg3", out message))
                        {
                            PutError(message);
                            //Error += S("\nError speaking to file: " + filePath + "\n");
                            return false;
                        }
                    }

                    AddAudioToTextRun(sentenceRun, "Audio", fileName, overwriteMedia, owner);

                    if (alternateLanguageIDs != null)
                    {
                        foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                        {
                            LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                            if (alternateLanguageItem == null)
                                continue;

                            if (alternateLanguageItem.SentenceRunCount() != languageItem.SentenceRunCount())
                                continue;

                            TextRun alternateRun = alternateLanguageItem.GetSentenceRun(sentenceRunIndex);

                            AddAudioToTextRun(alternateRun, "Audio", fileName, overwriteMedia, owner);
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool AddAudioToTextRun(TextRun textRun, string key, string fileName, bool overwriteMedia, string owner)
        {
            if (!textRun.HasAudioVideo() || overwriteMedia)
            {
                MediaRun mediaRun = new MediaRun(key, fileName, null, null, owner, TimeSpan.Zero, TimeSpan.Zero);

                if (textRun.MediaRuns == null)
                    textRun.MediaRuns = new List<MediaRun>(1) { mediaRun };
                else
                {
                    int index = textRun.MediaRuns.Count() - 1;

                    for (; index >= 0; index--)
                    {
                        MediaRun oldMediaRun = textRun.MediaRuns[index];

                        if (oldMediaRun.IsAudioVideo() && !oldMediaRun.IsReference)
                            textRun.MediaRuns.Remove(oldMediaRun);
                    }

                    textRun.MediaRuns.Add(mediaRun);
                    textRun.Modified = true;
                }
            }

            return true;
        }

        public bool AddSynthesizedVoiceToMarkupStrings(
            MarkupTemplate markupTemplate,
            string mediaTildeUrl,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            try
            {
                returnValue = AddSynthesizedVoiceToMarkupStrings(markupTemplate, mediaTildeUrl, languageID,
                voice, speed, overwriteMedia, filterAsides, studyItemFlags, speechEngine);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToMarkupStrings(
            MarkupTemplate markupTemplate,
            string mediaTildeUrl,
            LanguageID languageID,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            ITextToSpeech speechEngine)
        {
            LanguageDescription languageDescription;
            List<LanguageID> languageIDs = new List<LanguageID>();
            bool returnValue = true;

            switch (languageID.LanguageCode)
            {
                case "(my languages)":
                    languageIDs = UserProfile.LanguageIDs;
                    break;
                case "(target languages)":
                    languageIDs = UserProfile.TargetLanguageIDs;
                    break;
                case "(host languages)":
                    languageIDs = UserProfile.HostLanguageIDs;
                    break;
                case "(any)":
                case "(all languages)":
                    languageIDs = markupTemplate.ExpandLanguageIDs(UserProfile);
                    break;
                default:
                    languageIDs = new List<LanguageID>(1) { languageID };
                    break;
            }

            if (!UseGoogleTextToSpeech)
            {
                int languageIndex;
                int languageCount = languageIDs.Count();

                for (languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
                {
                    LanguageID lid = languageIDs[languageIndex];

                    languageDescription = LanguageLookup.GetLanguageDescription(lid);

                    if (languageDescription != null)
                    {
                        if ((languageDescription.VoiceNames == null) || (languageDescription.VoiceNames.Count() == 0))
                            languageIDs.Remove(lid);
                        else
                        {
                            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(lid);

                            if (alternateLanguageIDs != null)
                            {
                                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                                {
                                    if (languageIDs.Contains(alternateLanguageID))
                                        languageIDs.Remove(alternateLanguageID);
                                }

                                if (languageIndex > languageIDs.Count())
                                    languageIndex = languageIDs.Count();
                            }
                        }
                    }
                }
            }

            if (languageIDs.Count() == 0)
                return returnValue;
            else if (languageIDs.Count() == 1)
            {
                LanguageID voiceLanguageID = languageIDs[0];

                if (voice == "(default)")
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(voiceLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }
                }

                if (voice != "(none)")
                {
                    if (!speechEngine.SetVoice(voice, voiceLanguageID))
                    {
                        PutError("Error selecting voice.");
                        return false;
                    }
                }

                if (speed != 0)
                    speechEngine.SetSpeed(speed);

                List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);

                returnValue = AddSynthesizedVoiceToMarkupStringsLanguage(markupTemplate, mediaTildeUrl,
                    languageID, alternateLanguageIDs, voice, speed, overwriteMedia, filterAsides, studyItemFlags,
                    speechEngine);
            }
            else
            {
                foreach (LanguageID aLanguageID in languageIDs)
                {
                    languageDescription = LanguageLookup.GetLanguageDescription(aLanguageID);

                    if (languageDescription != null)
                    {
                        if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceName))
                            voice = languageDescription.PreferedVoiceName;
                        else if ((languageDescription.VoiceNames != null) && (languageDescription.VoiceNames.Count() != 0))
                            voice = languageDescription.VoiceNames[0];
                    }

                    if (!AddSynthesizedVoiceToMarkupStrings(markupTemplate, mediaTildeUrl,
                            aLanguageID, voice, speed, overwriteMedia, filterAsides, studyItemFlags))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToMarkupStringsLanguage(
            MarkupTemplate markupTemplate,
            string mediaTildeUrl,
            LanguageID languageID,
            List<LanguageID> alternateLanguageIDs,
            string voice,
            int speed,
            bool overwriteMedia,
            bool filterAsides,
            List<bool> studyItemFlags,
            ITextToSpeech speechEngine)
        {
            List<MultiLanguageItem> studyItems = markupTemplate.MultiLanguageItems;

            bool returnValue = AddSynthesizedVoiceToStudyItemsLanguage(studyItems,
                languageID, alternateLanguageIDs, markupTemplate.Owner,
                voice, speed, overwriteMedia, filterAsides, studyItemFlags,
                mediaTildeUrl, speechEngine);

            return returnValue;
        }

        public bool AddSynthesizedVoiceDefault(
            string text,
            string filePath,
            LanguageID synthesizerLanguageID)
        {
            string message;
            string voiceName = "(default)";
            int speed = -2;
            bool returnValue = true;

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(synthesizerLanguageID);

            if (languageDescription != null)
            {
                if (!String.IsNullOrEmpty(languageDescription.PreferedVoiceNameOrDefault))
                    voiceName = languageDescription.PreferedVoiceNameOrDefault;
            }

            try
            {
                ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();

                if (voiceName != "(none)")
                {
                    if (!speechEngine.SetVoice(voiceName, synthesizerLanguageID))
                    {
                        Error = "Error selecting voice.";
                        return false;
                    }
                }

                if (speed != 0)
                    speechEngine.SetSpeed(speed);

                if (speechEngine.SpeakToFile(text, filePath, "audio/mpeg3", out message))
                    returnValue = true;
                else
                {
                    if (!String.IsNullOrEmpty(Error))
                        Error += "\n";

                    Error += message;
                    returnValue = false;
                }
            }
            catch (Exception exc)
            {
                Error = Error + S("Exception while synthesising audio: ") +
                    text +
                    "\n    " +
                    exc.Message;

                if (exc.InnerException != null)
                    Error = Error + ": " + exc.InnerException.Message;
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntries(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, string voice, int speed, bool overwriteMedia,
            ref int itemIndex)
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();
            bool returnValue = false;

            if (String.IsNullOrEmpty(voice))
            {
                PutError("No voice is selected.");
                return false;
            }

            try
            {
                returnValue = AddSynthesizedVoiceToDictionaryEntries(dictionaryEntries, languageID,
                    voice, speed, overwriteMedia,
                    speechEngine, ref itemIndex);
            }
            catch (OperationCanceledException exc)
            {
                PutExceptionError(exc);
                returnValue = false;
            }
            finally
            {
                speechEngine.Reset();
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntries(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, string voice, int speed, bool overwriteMedias,
            ITextToSpeech speechEngine, ref int itemIndex)
        {
            LanguageID preferedLanguageID = LanguageLookup.GetPreferedMediaLanguageID(languageID);
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);
            bool returnValue;

            if (preferedLanguageID != languageID)
                returnValue = AddSynthesizedVoiceToDictionaryEntriesPrefered(
                    dictionaryEntries,
                    languageID,
                    voice,
                    speed,
                    overwriteMedias,
                    speechEngine,
                    ref itemIndex);
            else if (alternateLanguageIDs != null)
                returnValue = AddSynthesizedVoiceToDictionaryEntriesWithAlternates(
                    dictionaryEntries,
                    languageID,
                    voice,
                    speed,
                    overwriteMedias,
                    speechEngine,
                    ref itemIndex);
            else
                returnValue = AddSynthesizedVoiceToDictionaryEntriesNoAlternates(
                    dictionaryEntries,
                    languageID,
                    voice,
                    speed,
                    overwriteMedias,
                    speechEngine,
                    ref itemIndex);

            AddAudioReferencesCheck(dictionaryEntries, languageID);

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntriesNoAlternates(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, string voice, int speed, bool overwriteMedias,
            ITextToSpeech speechEngine, ref int itemIndex)
        {
            List<string> textList = new List<string>();
            List<string> fileList = new List<string>();
            string directoryPath = GetAudioDictionaryPath(languageID);
            string text;
            string fileName;
            string filePath;
            bool fileExists;
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                text = dictionaryEntry.KeyString;
                fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength) + ".mp3";
                filePath = MediaUtilities.ConcatenateFilePath(directoryPath, fileName);
                fileExists = FileSingleton.Exists(filePath);
                if (overwriteMedias)
                {
                    if (fileExists)
                        FileSingleton.Delete(filePath);
                }
                else if (fileExists)
                    continue;
                textList.Add(text);
                fileList.Add(fileName);
            }

            if (voice != "(none)")
            {
                if (!speechEngine.SetVoice(voice, languageID))
                {
                    PutError("Error selecting voice.");
                    return false;
                }
            }

            returnValue = SpeakListToFiles(
                textList,
                fileList,
                directoryPath,
                languageID,
                voice,
                speed,
                speechEngine,
                ref itemIndex);

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntriesPrefered(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, string voice, int speed, bool overwriteMedias,
            ITextToSpeech speechEngine, ref int itemIndex)
        {
            List<string> textList = new List<string>();
            List<string> fileList = new List<string>();
            HashSet<string> alternateSet = new HashSet<string>();
            LanguageID preferedLanguageID = LanguageLookup.GetPreferedMediaLanguageID(languageID);
            string preferedDirectoryPath = GetAudioDictionaryPath(preferedLanguageID);
            string directoryPath = GetAudioDictionaryPath(languageID);
            string text;
            string fileName;
            string filePath;
            bool fileExists;
            string sourceText;
            string sourceFileName;
            string sourceFilePath;
            bool sourceFileExists;
            LanguageID alternateLanguageID;
            string alternateDirectoryPath;
            bool returnValue = true;

            itemIndex = 0;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                if (!dictionaryEntry.HasAlternates())
                    continue;

                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (alternate.LanguageID != preferedLanguageID)
                        continue;

                    text = dictionaryEntry.KeyString;

                    if (alternateSet.Contains(text))
                        continue;

                    alternateSet.Add(text);

                    fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength) + ".mp3";
                    filePath = MediaUtilities.ConcatenateFilePath(preferedDirectoryPath, fileName);
                    fileExists = FileSingleton.Exists(filePath);

                    if (overwriteMedias)
                    {
                        if (fileExists)
                            FileSingleton.Delete(filePath);
                    }
                    else if (fileExists)
                        continue;

                    textList.Add(text);
                    fileList.Add(fileName);
                }
            }

            returnValue = SpeakListToFiles(
                textList,
                fileList,
                preferedDirectoryPath,
                languageID,
                voice,
                speed,
                speechEngine,
                ref itemIndex);

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                int bestPriorityLevel = -0x7fffffff;
                int bestReading = 0;

                text = dictionaryEntry.KeyString;
                fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength);

                if (dictionaryEntry.SenseCount != 0)
                {
                    foreach (Sense sense in dictionaryEntry.Senses)
                    {
                        int priorityLevel = sense.PriorityLevel;

                        if (priorityLevel > bestPriorityLevel)
                        {
                            bestPriorityLevel = priorityLevel;
                            bestReading = sense.Reading;
                        }
                    }
                }

                LanguageString preferedAlternate = dictionaryEntry.GetAlternate(preferedLanguageID, bestReading);

                if (preferedAlternate != null)
                {
                    fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength) + ".mp3";
                    filePath = MediaUtilities.ConcatenateFilePath(directoryPath, fileName);
                    fileExists = FileSingleton.Exists(filePath);

                    if (overwriteMedias)
                    {
                        if (fileExists)
                            FileSingleton.Delete(filePath);
                    }
                    else if (fileExists)
                        continue;

                    sourceText = preferedAlternate.Text;
                    sourceFileName = MediaUtilities.FileFriendlyName(sourceText, MaxMediaFileNameLength) + ".mp3";
                    sourceFilePath = MediaUtilities.ConcatenateFilePath(preferedDirectoryPath, sourceFileName);
                    sourceFileExists = FileSingleton.Exists(sourceFilePath);

                    if (!sourceFileExists)
                        continue;

                    try
                    {
                        FileSingleton.Copy(sourceFilePath, filePath);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError(exc);
                        returnValue = false;
                    }
                }

                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (alternate.LanguageID == preferedLanguageID)
                        continue;

                    alternateLanguageID = alternate.LanguageID;
                    alternateDirectoryPath = GetAudioDictionaryPath(alternateLanguageID);

                    preferedAlternate = dictionaryEntry.GetAlternate(preferedLanguageID, alternate.KeyInt);
                    sourceText = preferedAlternate.Text;
                    sourceFileName = MediaUtilities.FileFriendlyName(sourceText, MaxMediaFileNameLength) + ".mp3";
                    sourceFilePath = MediaUtilities.ConcatenateFilePath(preferedDirectoryPath, sourceFileName);
                    sourceFileExists = FileSingleton.Exists(sourceFilePath);

                    if (!sourceFileExists)
                        continue;

                    text = alternate.Text;

                    fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength) + ".mp3";
                    filePath = MediaUtilities.ConcatenateFilePath(alternateDirectoryPath, fileName);
                    fileExists = FileSingleton.Exists(filePath);

                    if (overwriteMedias)
                    {
                        if (fileExists)
                            FileSingleton.Delete(filePath);
                    }
                    else if (fileExists)
                        continue;

                    try
                    {
                        FileSingleton.Copy(sourceFilePath, filePath);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError(exc);
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntriesWithAlternates(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, string voice, int speed, bool overwriteMedias,
            ITextToSpeech speechEngine, ref int itemIndex)
        {
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);
            bool returnValue = AddSynthesizedVoiceToDictionaryEntriesNoAlternates(
                dictionaryEntries, languageID, voice, speed, overwriteMedias, speechEngine, ref itemIndex);

            foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
            {
                if (!AddSynthesizedVoiceToDictionaryEntriesAlternate(
                        dictionaryEntries, alternateLanguageID, overwriteMedias, speechEngine, ref itemIndex))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool AddSynthesizedVoiceToDictionaryEntriesAlternate(List<DictionaryEntry> dictionaryEntries,
            LanguageID languageID, bool overwriteMedias, ITextToSpeech speechEngine, ref int itemIndex)
        {
            HashSet<string> alternateSet = new HashSet<string>();
            LanguageID voiceLanguageID = languageID.MediaLanguageID();
            string directoryPath = GetAudioDictionaryPath(voiceLanguageID);
            string text;
            string fileName;
            string filePath;
            bool fileExists;
            string sourceDirectoryPath = GetAudioDictionaryPath(voiceLanguageID);
            string sourceText;
            string sourceFileName;
            string sourceFilePath;
            bool sourceFileExists;
            bool returnValue = true;

            itemIndex = 0;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                itemIndex++;

                if (!dictionaryEntry.HasAlternates())
                    continue;

                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    text = dictionaryEntry.KeyString;

                    if (alternateSet.Contains(text))
                        continue;

                    alternateSet.Add(text);

                    fileName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength) + ".mp3";
                    filePath = MediaUtilities.ConcatenateFilePath(directoryPath, fileName);
                    fileExists = FileSingleton.Exists(filePath);

                    if (overwriteMedias)
                    {
                        if (fileExists)
                            FileSingleton.Delete(filePath);
                    }
                    else if (fileExists)
                        continue;

                    sourceText = dictionaryEntry.KeyString;
                    sourceFileName = MediaUtilities.FileFriendlyName(sourceText, MaxMediaFileNameLength) + ".mp3";
                    sourceFilePath = MediaUtilities.ConcatenateFilePath(sourceDirectoryPath, fileName);
                    sourceFileExists = FileSingleton.Exists(sourceFilePath);

                    if (!sourceFileExists)
                        continue;

                    try
                    {
                        FileSingleton.Copy(sourceFilePath, filePath);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError(exc);
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public bool SpeakListToFiles(
            List<string> textList,
            List<string> fileList,
            string directoryPath,
            LanguageID languageID,
            string voice,
            int speed,
            ITextToSpeech speechEngine,
            ref int itemIndex)
        {
            string message;
            bool returnValue;

            if (voice != "(none)")
            {
                if (!speechEngine.SetVoice(voice, languageID))
                {
                    PutError("Error selecting voice.");
                    return false;
                }
            }

            if (speed != 0)
                speechEngine.SetSpeed(speed);

            if (speechEngine.SpeakListToFiles(
                    textList, fileList, directoryPath, "audio/mpeg3", ref itemIndex, out message))
                returnValue = true;
            else
            {
                if (!String.IsNullOrEmpty(Error))
                    Error += "\n";

                Error += message;
                returnValue = false;
            }

            return returnValue;
        }

        public bool AddAudioReferencesCheck(List<DictionaryEntry> dictionaryEntries, LanguageID languageID)
        {
            List<LanguageID> languageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            Dictionary<LanguageID, List<string>> textLists = new Dictionary<LanguageID, List<string>>();
            Dictionary<LanguageID, HashSet<string>> textSetLists = new Dictionary<LanguageID, HashSet<string>>();
            string text;
            LanguageID textLanguageID;
            string fileFriendlyName;
            string mimeType = "audio/mpeg3";
            AudioMultiReference audioReference;
            bool returnValue = true;

            foreach (LanguageID lid in languageIDs)
            {
                textLists.Add(lid, new List<string>());
                textSetLists.Add(lid, new HashSet<string>());
            }

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                text = dictionaryEntry.KeyString;
                textLanguageID = dictionaryEntry.LanguageID;

                if (!textSetLists[textLanguageID].Contains(text))
                {
                    textLists[textLanguageID].Add(text);
                    textSetLists[textLanguageID].Add(text);
                }

                if (dictionaryEntry.HasAlternates())
                {
                    foreach (LanguageString alternate in dictionaryEntry.Alternates)
                    {
                        text = alternate.Text;
                        textLanguageID = alternate.LanguageID;

                        if (!textSetLists[textLanguageID].Contains(text))
                        {
                            textLists[textLanguageID].Add(text);
                            textSetLists[textLanguageID].Add(text);
                        }
                    }
                }
            }

            foreach (LanguageID lid in languageIDs)
            {
                List<string> textList = textLists[lid];
                List<AudioMultiReference> audioReferences = new List<AudioMultiReference>();
                List<AudioMultiReference> audioReferenceListAdd = new List<AudioMultiReference>();
                List<AudioMultiReference> audioReferenceListUpdate = new List<AudioMultiReference>();
                List<AudioInstance> audioInstances = null;
                AudioInstance audioInstance = null;

                foreach (string key in textList)
                {
                    audioReference = Repositories.DictionaryMultiAudio.Get(key, lid);
                    fileFriendlyName = MediaUtilities.FileFriendlyName(key, MediaUtilities.MaxMediaFileNameLength);

                    if (audioReference == null)
                    {
                        audioInstance = new AudioInstance(
                            key,
                            ApplicationData.ApplicationName,
                            mimeType,
                            fileFriendlyName + ".mp3",
                            AudioReference.SynthesizedSourceName,
                            null);
                        audioInstances = new List<AudioInstance>();
                        audioReference = new AudioMultiReference(
                            key,
                            languageID,
                            audioInstances);
                        audioReferenceListAdd.Add(audioReference);
                    }
                    else
                    {
                        audioInstance = audioReference.GetAudioInstanceBySource(AudioReference.SynthesizedSourceName);
                        if (audioInstance != null)
                        {
                            if (audioInstance.FileName != fileFriendlyName + ".mp3")
                            {
                                string audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                                string audioFilePath = ApplicationData.MapToFilePath(audioUrl);

                                if (FileSingleton.Exists(audioFilePath))
                                    FileSingleton.Delete(audioFilePath);

                                audioInstance.FileName = fileFriendlyName + ".mp3";
                                audioReferenceListUpdate.Add(audioReference);
                            }
                        }
                        else
                        {
                            audioInstance = new AudioInstance(
                                key,
                                ApplicationData.ApplicationName,
                                mimeType,
                                fileFriendlyName + ".mp3",
                                AudioReference.SynthesizedSourceName,
                                null);
                            audioReference.AddAudioInstance(audioInstance);
                            audioReferenceListUpdate.Add(audioReference);
                        }
                    }
                }

                if (audioReferenceListUpdate.Count() != 0)
                {
                    if (!Repositories.DictionaryMultiAudio.UpdateList(audioReferences, lid))
                    {
                        PutErrorArgument("Error updating audio references: ", lid.LanguageName(UILanguageID));
                        returnValue = false;
                    }
                }

                if (audioReferenceListAdd.Count() != 0)
                {
                    if (!Repositories.DictionaryMultiAudio.AddList(audioReferences, lid))
                    {
                        PutErrorArgument("Error adding audio references: ", lid.LanguageName(UILanguageID));
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }
    }
}

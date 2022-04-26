using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Service
{
    public enum ClientMode { StartMessage, WaitMessage, StopMessage, Canceled, Error };
    public delegate void ClientWaitCallback(ClientMode mode, string messageString);

    public class ClientServiceBase
    {
        public static string ServiceEndpointPrefix;
        public ClientWaitCallback ServiceCallback;
        public bool ResponseReceived;
        public bool Succeeded;
        public string ErrorMessage;
        public bool CancelFlag;

        public ClientServiceBase(string serviceEndpointPrefix)
        {
            ServiceEndpointPrefix = serviceEndpointPrefix;
            ServiceCallback = null;
            ResponseReceived = false;
            Succeeded = false;
            ErrorMessage = null;
            CancelFlag = false;
        }

        public ClientServiceBase()
        {
            ServiceEndpointPrefix = null;
            ServiceCallback = null;
            ResponseReceived = false;
            Succeeded = false;
            ErrorMessage = null;
            CancelFlag = false;
        }

        public virtual void Initialize()
        {
        }

        public virtual void ChangeServiceEndpointPrefix(string serviceEndpointPrefix)
        {
            ServiceEndpointPrefix = serviceEndpointPrefix;
        }

        public virtual MessageBase ExecuteCommand(MessageBase command)
        {
            return null;
        }

        /*
        public virtual string UpdateFlashEntryListStatus(object planKey, object flashListKey, List<FlashEntry> flashEntries)
        {
            return null;
        }
        */

        public virtual UserID LogOn(string userName, string password)
        {
            return null;
        }

        public virtual bool LogOff(string userIDString)
        {
            return false;
        }

        public virtual bool Register(string userName, string realName, string email, string password,
            string userRole, string timeZoneID, bool isMinor, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }

        public virtual bool GetContentFile(string fileName, string fileType, string userName, string userIDString,
            out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool SetContentFile(string fileName, string fileType,
            string userName, string userIDString, byte[] bytes, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }

        public virtual bool ConvertAndSetContentFile(string fromFileName, string toFileName,
            string fileType, string fromMimeType, string toMimeType, string userName, string userIDString,
            byte[] bytes, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }

        public virtual bool ConvertData(string fileType, string fromMimeType, string toMimeType,
            string userName, string userIDString, byte[] bytes, out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool DeleteContentFile(string fileName, string fileType,
            string userName, string userIDString, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }

        // Legacy
        public virtual bool GetTree(string source, int treeKey, Guid guid, bool includeMedia,
            Dictionary<string, bool> languageSelectFlags, out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool GetTreeFormat(string source, int treeKey, Guid guid, bool includeMedia,
            Dictionary<string, bool> languageSelectFlags, string formatName,
            out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool GetRepository(string name, string languageCode,
            out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual long GetTreeMediaSize(string source, int treeKey,
            Dictionary<string, bool> languageSelectFlags, out string errorMessage)
        {
            errorMessage = null;
            return 0L;
        }

        public virtual long GetMediaSize(string source, int treeKey, int nodeKey, Guid treeGuid,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags,
            Dictionary<string, bool> languageSelectFlags,
            out string errorMessage)
        {
            errorMessage = null;
            return 0L;
        }

        public virtual bool GetMediaFiles(string source, int treeKey, int nodeKey, Guid treeGuid,
            Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags,
            Dictionary<string, bool> languageSelectFlags,
            out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool GetMediaDirectoryList(
            string remoteUrl,
            List<string> extensions,
            out List<string> fileNames)
        {
            fileNames = null;
            return false;
        }

        public virtual long GetMediaDirectorySize(
            string remoteUrl,
            List<string> extensions)
        {
            return -1L;
        }

        public virtual bool SynthesizeSpeech(string voiceName, string languageCode, int speed,
            string text, string tildeUrl, string userName, string userIDString,
            out byte[] outData, out string errorMessage)
        {
            outData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool TranslateDatabaseString(string databaseName, string stringID,
            string inputString, string inputLanguageCode, string outputLanguageCode,
            out string outputString, out string errorMessage)
        {
            outputString = null;
            errorMessage = null;
            return false;
        }

        public virtual bool TranslateDatabaseStringUser(
            string operationPrefix, string userName, string databaseName, string stringID,
            string inputString, string inputLanguageCode, string outputLanguageCode,
            out string outputString, out string errorMessage)
        {
            outputString = null;
            errorMessage = null;
            return false;
        }

        public virtual bool CheckVocabularyItems(
            string targetLanguageCode,
            string uiLanguageCode,
            List<string> targetLanguageCodes,
            List<string> hostLanguageCodes,
            string userName,
            List<string> dictionaryKeys,
            List<string> audioKeys,
            out string errorMessage)
        {
            bool returnValue = false;
            errorMessage = null;
            return returnValue;
        }

        public virtual bool GetVocabularyDictionaryEntries(
            string targetLanguageCode,
            string uiLanguageCode,
            List<string> targetLanguageCodes,
            List<string> hostLanguageCodes,
            string userName,
            List<string> words,
            bool translateMissingEntries,
            bool synthesizeMissingAudio,
            out byte[] dictionaryEntriesFoundData,
            out string[] dictionaryKeysNotFound,
            out string errorMessage)
        {
            bool returnValue = false;
            dictionaryEntriesFoundData = null;
            dictionaryKeysNotFound = null;
            errorMessage = null;
            return returnValue;
        }

        public virtual bool GetContentFiles(
            string commonDirectory,
            string commonExtension,
            List<string> filePartialPaths,
            string userIDString,
            out byte[] outChunkyData,
            out string errorMessage)
        {
            outChunkyData = null;
            errorMessage = null;
            return false;
        }

        public virtual bool SendEmail(
            string subject,
            string messageBody,
            string fromAddress,
            string toAddress,
            string ccAddress,
            out string errorMessage)
        {
            errorMessage = null;
            return false;
        }

        public virtual bool CheckForAudios(
            string[] texts,
            string languageCode,
            out string[] failedTexts,
            out string errorMessage)
        {
            failedTexts = null;
            errorMessage = null;
            return false;
        }

        public virtual bool PrimeAudios(
            string[] texts,
            string userName,
            string profileName,
            string targetLanguageCode,
            string hostLanguageCode,
            string source,
            out string[] failedTexts,
            out string errorMessage)
        {
            failedTexts = null;
            errorMessage = null;
            return false;
        }

        public virtual bool CrawlPassageSingle(
            string passageName,
            string passageUrl,
            string userName,
            string profileName,
            string targetLanguageCode,
            string hostLanguageCode,
            out string targetPassage,
            out string errorMessage)
        {
            targetPassage = null;
            errorMessage = null;
            return false;
        }

        public virtual bool CrawlPassageDouble(
            string passageName,
            string passageUrl,
            string translationUrl,
            string userName,
            string profileName,
            string targetLanguageCode,
            string hostLanguageCode,
            out string targetPassage,
            out string hostPassage,
            out string errorMessage)
        {
            targetPassage = null;
            hostPassage = null;
            errorMessage = null;
            return false;
        }

        public virtual bool ConvertTextToJsonPackage(
            string userName,
            string profileName,
            string targetLanguageCode,
            string hostLanguageCode,
            string targetText,
            string hostText,
            bool textUnitsSentences,
            bool showTranslation,
            bool showStatistics,
            bool showGlossary,
            bool showJSON,
            string wordAudioMode,
            out string module,
            out string errorMessage)
        {
            module = null;
            errorMessage = null;
            return false;
        }

        public virtual string Test(string message)
        {
            return null;
        }

        public virtual void PutError(string errorMessage)
        {
            ErrorMessage = errorMessage;

            if (ServiceCallback != null)
                Service(ClientMode.Error, errorMessage);
            //else
            //    ApplicationData.Global.ShowMessageBox("Error", errorMessage);

        }

        public virtual void Service(ClientMode mode, string messageString)
        {
            if (ServiceCallback != null)
                ServiceCallback(mode, messageString);
        }
    }
}

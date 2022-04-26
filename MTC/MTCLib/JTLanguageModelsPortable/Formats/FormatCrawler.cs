using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Crawlers;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatCrawler : Format
    {
        // "Generic Text", "LDS Scriptures", "Innovative", "Forvo Audio"
        public string WebFormatType { get; set; }
        public static string WebFormatTypePrompt = "Web format type";
        public static string WebFormatTypeHelp = "Select the web format to expect.";

        public Crawler WebCrawler { get; set; }
        public Stream ImportStream { get; set; }

        private static string FormatDescription = "Crawl a website and extract content from it.";

        public FormatCrawler()
            : base("Crawler", "FormatCrawler", FormatDescription, String.Empty, String.Empty,
                  "text/html", ".html", null, null, null, null, null)
        {
            ClearFormatCrawler();
        }

        public FormatCrawler(FormatCrawler other)
            : base(other)
        {
            CopyFormatCrawler(other);
        }

        // For derived classes.
        public FormatCrawler(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatCrawler();
        }

        public override Format Clone()
        {
            return new FormatCrawler(this);
        }

        public void ClearFormatCrawler()
        {
            // Local parameters.

            WebFormatType = "Generic Text";
            WebCrawler = null;
            ImportStream = null;
        }

        public void CopyFormatCrawler(FormatCrawler other)
        {
            // Local parameters.

            WebFormatType = other.WebFormatType;
            WebCrawler = null;
            ImportStream = null;
        }

        public override void Read(Stream stream)
        {
            if (Timer == null)
                Timer = new SoftwareTimer();

            if (Timer != null)
                Timer.Start();

            try
            {
                switch (ImportExportType)
                {
                    case "File":
                    case "Text":
                        ImportStream = stream;
                        break;
                    case "Web":
                        if (stream != null)
                        {
                            byte[] buffer = new byte[2048];
                            int urlSize = stream.Read(buffer, 0, 2047);
                            buffer[2047] = 0;
                            string webUrl = TextUtilities.GetStringFromBytes(buffer, 0, urlSize);
                            WebUrl = webUrl;
                            UserProfile.SetUserOptionString(GetArgumentKeyName("WebUrl"), WebUrl);
                        }
                        /*
                        else
                        {
                            WebUrl = String.Empty;
                            UserProfile.SetUserOptionString(GetArgumentKeyName("WebUrl"), WebUrl);
                        }
                        */
                        break;
                    default:
                        PutErrorArgument("Unknown import type: ", ImportExportType);
                        return;
                }

                if (InitializeCrawler())
                {
                    string errorMessage = null;

                    WebCrawler.Timer = Timer;
                    WebCrawler.TaskName = TaskName;
                    WebCrawler.InitializeSiteInformation();

                    if (!WebCrawler.PermissionsCheck(UserRecord, ref errorMessage))
                        PutError(errorMessage);
                    else
                    {
                        if (!WebCrawler.HandleCrawl())
                            Error = WebCrawler.Error;
                        else if (WebCrawler.HasError)
                            Error = WebCrawler.Error;
                        else if (WebCrawler.HasMessage)
                            Message = WebCrawler.Message;
                    }
                }
            }
            catch (Exception exc)
            {
                PutExceptionError(exc);
            }
            finally
            {
                if (Timer != null)
                {
                    Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }

                if (HasError())
                    throw new Exception(Error);
            }
        }

        public override void Write(Stream stream)
        {
            PutError("Sorry, export not support for the crawler format.");
        }

        public bool InitializeCrawler()
        {
            if (HasError())
                return false;

            if ((WebCrawler == null) || (WebCrawler.Name != WebFormatType))
            {
                if (ApplicationData.Crawlers == null)
                {
                    PutError("Sorry, this version of JTLanguage does not support web crawling.");
                    return false;
                }

                WebCrawler = ApplicationData.Crawlers.Create(WebFormatType, this);

                if (WebCrawler == null)
                {
                    PutErrorArgument("Sorry, this is an unknown web format type", WebFormatType);
                    return false;
                }
            }

            if ((DumpStringDelegate != null) && (WebCrawler.DumpStringDelegate == null))
                WebCrawler.DumpStringDelegate = DumpStringDelegate;

            return true;
        }

        public override void PrefetchArguments(FormReader formReader)
        {
            if (formReader.HasField("WebFormatType"))
                WebFormatType = formReader["WebFormatType"];
            SetStringListArgument("WebFormatType", "stringlist", "r", WebFormatType,
                GetWebFormatTypes(UserRecord), "Web format type", WebFormatTypeHelp);
        }

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            WebFormatType = GetStringListArgumentDefaulted("WebFormatType", "stringlist", "r", WebFormatType,
                GetWebFormatTypes(UserRecord), WebFormatTypePrompt, WebFormatTypeHelp);

            if (InitializeCrawler())
                WebCrawler.LoadFromArguments();

            FixupArguments();

            WebUrl = UserProfile.GetUserOptionString(GetArgumentKeyName("WebUrl"), WebUrl);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetStringListArgument("WebFormatType", "stringlist", "r", WebFormatType,
                GetWebFormatTypes(UserRecord), "Web format type", WebFormatTypeHelp);

            if (InitializeCrawler())
                WebCrawler.SaveToArguments();

            FixupArguments();
        }

        public override string GetArgumentKeyName(string name)
        {
            string keyName;
            if (name == "WebFormatType")
                keyName = Type + "." + TargetType + "." + name;
            else
                keyName = Type + "." + WebFormatType + "." + TargetType + "." + name;
            return keyName;
        }

        public static List<string> GetWebFormatTypes(UserRecord userRecord)
        {
            List<string> webFormatTypes;

            if (ApplicationData.Crawlers != null)
                webFormatTypes = ApplicationData.Crawlers.GetFilteredNames(userRecord);
            else
                webFormatTypes = new List<string>();

            return webFormatTypes;
        }

        public void FixupArguments()
        {
            FormatArgument argument = FindArgument("WebFormatType");
            argument.PostOnChange = true;
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);

            DumpArgument("Web format type", WebFormatType);

            if (WebCrawler != null)
                WebCrawler.DumpArguments(null);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            if (importExport == "Export")
                return false;

            if (ApplicationData.Crawlers == null)
                return false;

            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                case "ContentMediaList":
                case "ContentMediaItem":
                    if (capability == "Support")
                        return true;
                    else if (capability == "Web")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string contentName, string capability)
        {
            return IsSupportedStatic(importExport, contentName, capability);
        }

        public static new string TypeStringStatic { get { return "Crawler"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}

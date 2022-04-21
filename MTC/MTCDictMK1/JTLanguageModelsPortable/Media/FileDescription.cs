using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class FileDescription : BaseObjectLanguages
    {
        protected string _FileType;
        protected string _MimeType;
        protected string _FileName;

        public FileDescription(object key, string fileType, string mimeType, string fileName,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner)
            : base(key, targetLanguageIDs, hostLanguageIDs, owner)
        {
            _FileType = fileType;
            _MimeType = mimeType;
            _FileName = fileName;
        }

        public FileDescription(FileDescription other)
            : base(other)
        {
            CopyFileDescription(other);
            _Modified = false;
        }

        public FileDescription(FileDescription other, object key)
            : base(other, key)
        {
            CopyFileDescription(other);
            _Modified = false;
        }

        public FileDescription(object key)
            : base(key)
        {
            _FileType = String.Empty;
            _MimeType = String.Empty;
            _FileName = String.Empty;
            _Owner = String.Empty;
        }

        public FileDescription(XElement element)
        {
            OnElement(element);
        }

        public FileDescription()
        {
            ClearFileDescription();
        }

        public override void Clear()
        {
            base.Clear();
            ClearFileDescription();
        }

        public void ClearFileDescription()
        {
            _FileType = String.Empty;
            _MimeType = String.Empty;
            _FileName = String.Empty;
            _Owner = String.Empty;
        }

        public void CopyFileDescription(FileDescription other)
        {
            FileType = other.FileType;
            MimeType = other.MimeType;
            FileName = other.FileName;
            Owner = other.Owner;
        }

        public override IBaseObject Clone()
        {
            return new FileDescription(this);
        }

        public string FileType
        {
            get
            {
                return _FileType;
            }
            set
            {
                if (_FileType != value)
                {
                    _FileType = value;
                    _Modified = true;
                }
            }
        }

        public string MimeType
        {
            get
            {
                return _MimeType;
            }
            set
            {
                if (_MimeType != value)
                {
                    _MimeType = value;
                    _Modified = true;
                }
            }
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    _Modified = true;
                }
            }
        }

        public bool IsFullUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.Contains(":"))
                    return true;

                return false;
            }
        }

        public string GetUrl(string directoryUrl)
        {
            string url = FileName;

            if (String.IsNullOrEmpty(url))
                return String.Empty;

            if (IsFullUrl)
                return url;

            if (String.IsNullOrEmpty(directoryUrl))
                directoryUrl = "~/";

            if (!directoryUrl.EndsWith("/"))
                directoryUrl += "/";

            url = directoryUrl + url;

            return url;
        }

        public string GetDirectoryPath(string directoryUrl)
        {
            string url = GetUrl(directoryUrl);
            string path = ApplicationData.MapToFilePath(url);
            return path;
        }

        public virtual void SetFileAndMimeTypeFromFileName()
        {
            if (_FileName == null)
                return;

            string fileName = _FileName.ToLower();

            if (fileName.EndsWith(".wmv") || fileName.EndsWith(".mp4") || fileName.EndsWith(".ogg") || fileName.EndsWith(".webm"))
            {
                FileType = "Video";
                MimeType = "video/x-ms-wmv";
            }
            else if (fileName.EndsWith(".mp4"))
            {
                FileType = "Video";
                MimeType = "video/mp4";
            }
            else if (fileName.EndsWith(".ogg"))
            {
                FileType = "Video";
                MimeType = "video/ogg";
            }
            else if (fileName.EndsWith(".webm"))
            {
                FileType = "Video";
                MimeType = "video/webm";
            }
            else if (fileName.EndsWith(".mp3"))
            {
                FileType = "Audio";
                MimeType = "audio/mp3";
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_FileType != String.Empty)
                element.Add(new XAttribute("FileType", _FileType));
            if (!String.IsNullOrEmpty(_MimeType))
                element.Add(new XAttribute("MimeType", _MimeType));
            if (_FileName != null)
                element.Add(new XAttribute("FileName", _FileName));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "FileType":
                    _FileType = attributeValue;
                    break;
                case "MimeType":
                    _MimeType = attributeValue;
                    break;
                case "FileName":
                    _FileName = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public static int CompareFileTypes(FileDescription object1, FileDescription object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareStrings(object1.FileType, object2.FileType);
        }

        public static int CompareUris(Uri object1, Uri object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            if (object1 == object2)
                return 0;
            else
                return String.Compare(object1.AbsoluteUri, object2.AbsolutePath); ;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            FileDescription otherFileDescription = other as FileDescription;

            if (otherFileDescription == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_FileType, otherFileDescription.FileType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_MimeType, otherFileDescription.MimeType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherFileDescription.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_FileName, otherFileDescription.FileName);
            return diff;
        }

        public static int Compare(FileDescription object1, FileDescription object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(FileDescription object1, FileDescription object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }
    }
}

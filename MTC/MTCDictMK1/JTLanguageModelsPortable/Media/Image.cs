using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class Image : BaseObjectKeyed
    {
        public string _Name;
        public string _Owner;
        public string _ImageMimeType;
        public byte[] _ImageData;
        private static string[] _SupportedTypes = new string[]
        {
            "image/jpeg", ".jpg",
            "image/bmp", ".bmp",
            "image/gif", ".gif",
            "image/png", ".png"
        };

        public Image(object key, string name, string owner, string imageMimeType, byte[] imageData)
            : base(key)
        {
            _Name = name;
            _Owner = owner;
            _ImageMimeType = imageMimeType;
            _ImageData = imageData;
        }

        public Image(object key, string owner, string filePath)
            : base(key)
        {
            _Name = filePath;
            _Owner = owner;
            _ImageMimeType = GetMimeTypeFromFileName(filePath);
            _ImageData = ReadFileBytes(filePath);
        }

        public Image(Image other)
            : base(other)
        {
            CopyImage(other);
            ModifiedFlag = false;
        }

        public Image(XElement element)
        {
            OnElement(element);
        }

        public Image()
        {
            ClearImage();
        }

        public override void Clear()
        {
            base.Clear();
            ClearImage();
        }

        public void ClearImage()
        {
            _Name = null;
            _Owner = null;
            _ImageMimeType = null;
            _ImageData = null;
        }

        public virtual void CopyImage(Image other)
        {
            Name = other.Name;
            _Owner = other.Owner;
            ImageMimeType = other.ImageMimeType;
            ImageData = other.ImageData;
        }

        public override IBaseObject Clone()
        {
            return new Image(this);
        }

        public override string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string ImageMimeType
        {
            get
            {
                return _ImageMimeType;
            }
            set
            {
                if (value != _ImageMimeType)
                {
                    _ImageMimeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public byte[] ImageData
        {
            get
            {
                return _ImageData;
            }
            set
            {
                if (value != _ImageData)
                {
                    _ImageData = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ImageSize
        {
            get
            {
                return (_ImageData != null ? ImageData.Length : 0);
            }
            set
            {
                _ImageData = new byte[value];
                ModifiedFlag = true;
            }
        }

        public static string GetFileExtension(string fileName)
        {
            char[] delimiters = { '.' };
            string[] parts = fileName.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if ((parts != null) && (parts.Count() != 0))
                return "." + parts[parts.Count() - 1];
            return null;
        }

        public static string GetMimeTypeFromFileName(string fileName)
        {
            int count = _SupportedTypes.Count();
            int index;
            string mimeType = null;
            string extension = GetFileExtension(fileName);
            for (index = 0; index < count; index += 2)
            {
                if (_SupportedTypes[index + 1] == extension)
                {
                    mimeType = _SupportedTypes[index];
                    break;
                }
            }
            return mimeType;
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            return FileSingleton.ReadAllBytes(filePath);
        }

        public override byte[] BinaryData
        {
            get
            {
                byte[] baseData = base.BinaryData;
                int baseLength = baseData.Length + 4;
                int nameLength = (_Name != null ? ApplicationData.Encoding.GetBytes(_Name).Count() : 0) + 4;
                int typeLength = (_ImageMimeType != null ? _ImageMimeType.Length : 0) + 4;
                int dataLength = (_ImageData != null ? _ImageData.Count() : 0) + 4;
                byte[] imageData = _ImageData;
                if (imageData == null)
                    imageData = new byte[0];
                byte[] data = new byte[baseLength + nameLength + typeLength + dataLength];
                int dindex = 0;
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, baseData);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _Name);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _ImageMimeType);
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, _ImageData);
                return data;
            }
            set
            {
                int dindex = 0;
                base.BinaryData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
                _Name = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _ImageMimeType = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _ImageData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Name != null)
                element.Add(new XAttribute("Name", _Name));
            if (_Owner != null)
                element.Add(new XAttribute("Owner", _Owner));
            if (_ImageMimeType != null)
                element.Add(new XAttribute("ImageMimeType", _ImageMimeType));
            if (_ImageData != null)
                element.Add(new XElement("ImageData", ObjectUtilities.GetDataStringFromByteArray(_ImageData, true)));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Name = attributeValue;
                    break;
                case "Owner":
                    Owner = attributeValue;
                    break;
                case "ImageMimeType":
                    ImageMimeType = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "ImageData":
                    _ImageData = ObjectUtilities.GetByteArrayFromDataString(childElement.Value);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            Image otherImage = other as Image;

            if (otherImage == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Name, otherImage.Name);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherImage.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_ImageMimeType, otherImage.ImageMimeType);
            if (diff != 0)
                return diff;
            return ObjectUtilities.CompareByteArrays(_ImageData, otherImage.ImageData);
        }

        public static int Compare(Image object1, Image object2)
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

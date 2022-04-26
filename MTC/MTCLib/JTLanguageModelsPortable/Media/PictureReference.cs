using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class PictureReference : BaseObjectKeyed
    {
        public string _Name;
        public string _Owner;
        public string _PictureMimeType;
        public string _PictureFilePath;

        public PictureReference(object key, string name, string owner, string audioMimeType, string audioFilePath)
            : base(key)
        {
            _Name = name;
            _Owner = owner;
            _PictureMimeType = audioMimeType;
            _PictureFilePath = audioFilePath;
        }

        public PictureReference(PictureReference other)
            : base(other)
        {
            CopyPicture(other);
            ModifiedFlag = false;
        }

        public PictureReference(XElement element)
        {
            OnElement(element);
        }

        public PictureReference()
        {
            ClearPicture();
        }

        public override void Clear()
        {
            base.Clear();
            ClearPicture();
        }

        public void ClearPicture()
        {
            _Name = null;
            _Owner = null;
            _PictureMimeType = null;
            _PictureFilePath = null;
        }

        public virtual void CopyPicture(PictureReference other)
        {
            Name = other.Name;
            _Owner = other.Owner;
            PictureMimeType = other.PictureMimeType;
            PictureFilePath = other.PictureFilePath;
        }

        public override IBaseObject Clone()
        {
            return new PictureReference(this);
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

        public string PictureMimeType
        {
            get
            {
                return _PictureMimeType;
            }
            set
            {
                if (value != _PictureMimeType)
                {
                    _PictureMimeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PictureFilePath
        {
            get
            {
                return _PictureFilePath;
            }
            set
            {
                if (value != _PictureFilePath)
                {
                    _PictureFilePath = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override byte[] BinaryData
        {
            get
            {
                byte[] baseData = base.BinaryData;
                int baseLength = baseData.Length + 4;
                int nameLength = (_Name != null ? ApplicationData.Encoding.GetBytes(_Name).Count() : 0) + 4;
                int typeLength = (_PictureMimeType != null ? _PictureMimeType.Length : 0) + 4;
                int filePathLength = (_PictureFilePath != null ? ApplicationData.Encoding.GetBytes(_PictureFilePath).Count() : 0) + 4;
                byte[] data = new byte[baseLength + nameLength + typeLength + filePathLength];
                int dindex = 0;
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, baseData);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _Name);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _PictureMimeType);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _PictureFilePath);
                return data;
            }
            set
            {
                int dindex = 0;
                base.BinaryData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
                _Name = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _PictureMimeType = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _PictureFilePath = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Name != null)
                element.Add(new XAttribute("Name", _Name));
            if (_Owner != null)
                element.Add(new XAttribute("Owner", _Owner));
            if (_PictureMimeType != null)
                element.Add(new XAttribute("PictureMimeType", _PictureMimeType));
            if (_PictureFilePath != null)
                element.Add(new XAttribute("PictureFilePath", _PictureFilePath));
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
                case "PictureMimeType":
                    PictureMimeType = attributeValue;
                    break;
                case "PictureFilePath":
                    PictureFilePath = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            PictureReference otherPicture = other as PictureReference;

            if (otherPicture == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Name, otherPicture.Name);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherPicture.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_PictureMimeType, otherPicture.PictureMimeType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_PictureFilePath, otherPicture.PictureFilePath);
            return diff;
        }

        public static int Compare(PictureReference object1, PictureReference object2)
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

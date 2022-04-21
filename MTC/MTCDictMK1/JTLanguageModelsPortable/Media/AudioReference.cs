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
    public class AudioReference : BaseObjectKeyed
    {
        public string _Name;
        public string _Owner;
        public string _AudioMimeType;
        public string _AudioFilePath;
        public int _SourceID;
        public static string SynthesizedSourceName = "Synthesized";
        public static string ForvoSourceName = "Forvo";
        public static string DontCareSourceName = "DontCare";

        public AudioReference(
                object key,
                string name,
                string owner,
                string audioMimeType,
                string audioFilePath,
                string sourceName)
            : base(key)
        {
            _Name = name;
            _Owner = owner;
            _AudioMimeType = audioMimeType;
            _AudioFilePath = audioFilePath;

            if (!String.IsNullOrEmpty(sourceName))
                _SourceID = ApplicationData.DictionarySourcesLazy.Add(sourceName);
            else
                _SourceID = -1;
        }

        public AudioReference(AudioReference other)
            : base(other)
        {
            CopyAudio(other);
            ModifiedFlag = false;
        }

        public AudioReference(XElement element)
        {
            OnElement(element);
        }

        public AudioReference()
        {
            ClearAudio();
        }

        public override void Clear()
        {
            base.Clear();
            ClearAudio();
        }

        public void ClearAudio()
        {
            _Name = null;
            _Owner = null;
            _AudioMimeType = null;
            _AudioFilePath = null;
            _SourceID = -1;
        }

        public virtual void CopyAudio(AudioReference other)
        {
            _Name = other.Name;
            _Owner = other.Owner;
            _AudioMimeType = other.AudioMimeType;
            _AudioFilePath = other.AudioFilePath;
            _SourceID = other.SourceID;
        }

        public override IBaseObject Clone()
        {
            return new AudioReference(this);
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

        public string AudioMimeType
        {
            get
            {
                return _AudioMimeType;
            }
            set
            {
                if (value != _AudioMimeType)
                {
                    _AudioMimeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string AudioFilePath
        {
            get
            {
                return _AudioFilePath;
            }
            set
            {
                if (value != _AudioFilePath)
                {
                    _AudioFilePath = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int SourceID
        {
            get
            {
                return _SourceID;
            }
            set
            {
                if (value != _SourceID)
                {
                    _SourceID = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SourceName
        {
            get
            {
                return ApplicationData.DictionarySourcesLazy.GetByID(_SourceID);
            }
            set
            {
                SourceID = ApplicationData.DictionarySourcesLazy.Add(value);
            }
        }

        public override byte[] BinaryData
        {
            get
            {
                byte[] baseData = base.BinaryData;
                int baseLength = baseData.Length + 4;
                int nameLength = (_Name != null ? ApplicationData.Encoding.GetBytes(_Name).Count() : 0) + 4;
                int typeLength = (_AudioMimeType != null ? _AudioMimeType.Length : 0) + 4;
                int filePathLength = (_AudioFilePath != null ? ApplicationData.Encoding.GetBytes(_AudioFilePath).Count() : 0) + 4;
                byte[] data = new byte[baseLength + nameLength + typeLength + filePathLength + 1];
                int dindex = 0;
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, baseData);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _Name);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _AudioMimeType);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _AudioFilePath);
                data[dindex++] = (byte)_SourceID;
                return data;
            }
            set
            {
                int dindex = 0;
                base.BinaryData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
                _Name = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _AudioMimeType = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _AudioFilePath = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                if (dindex > value.Length)
                    _SourceID = value[dindex++];
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Name != null)
                element.Add(new XAttribute("Name", _Name));
            if (_Owner != null)
                element.Add(new XAttribute("Owner", _Owner));
            if (_AudioMimeType != null)
                element.Add(new XAttribute("AudioMimeType", _AudioMimeType));
            if (_AudioFilePath != null)
                element.Add(new XAttribute("AudioFilePath", _AudioFilePath));
            if (_SourceID != -1)
                element.Add(new XAttribute("SourceID", _SourceID));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    _Name = attributeValue;
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                case "AudioMimeType":
                    _AudioMimeType = attributeValue;
                    break;
                case "AudioFilePath":
                    _AudioFilePath = attributeValue;
                    break;
                case "SourceID":
                    _SourceID = ObjectUtilities.GetIntegerFromString(attributeValue, -1); ;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            AudioReference otherAudio = other as AudioReference;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Name, otherAudio.Name);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherAudio.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_AudioMimeType, otherAudio.AudioMimeType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_AudioFilePath, otherAudio.AudioFilePath);
            if (diff != 0)
                return diff;
            diff = _SourceID = otherAudio.SourceID;
            return diff;
        }

        public static int Compare(AudioReference object1, AudioReference object2)
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

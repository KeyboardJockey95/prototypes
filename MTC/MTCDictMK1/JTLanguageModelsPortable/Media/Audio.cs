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
    public class Audio : BaseObjectKeyed
    {
        public string _Name;
        public string _Owner;
        public string _AudioMimeType;
        public IDataBuffer _AudioDataBuffer;
        public WaveAudioBuffer _AudioDataDecoded; // Not saved.
        private static string[] _SupportedTypes = new string[]
        {
            "audio/wav", ".wav",
            "audio/mpeg3", ".mp3",
            "audio/speex", ".spx"
        };

        public Audio(object key, string name, string owner, string audioMimeType, IDataBuffer audioStream)
            : base(key)
        {
            _Name = name;
            _Owner = owner;
            _AudioMimeType = audioMimeType;
            _AudioDataBuffer = audioStream;
            _AudioDataDecoded = null;
        }

        public Audio(object key, string owner, string filePath)
            : base(key)
        {
            _Name = MediaUtilities.GetFileNameFromPath(filePath);
            _Owner = owner;
            _AudioMimeType = MediaUtilities.GetMimeTypeFromFileName(filePath);
            if (FileSingleton.Exists(filePath))
            {
                byte[] audioData = FileSingleton.ReadAllBytes(filePath);
                if (audioData != null)
                    _AudioDataBuffer = new MemoryBuffer(audioData);
            }
            _AudioDataDecoded = null;
        }

        public Audio(Audio other)
            : base(other)
        {
            CopyAudio(other);
            ModifiedFlag = false;
        }

        public Audio(XElement element)
        {
            OnElement(element);
        }

        public Audio()
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
            _AudioDataBuffer = null;
            _AudioDataDecoded = null;
        }

        public virtual void CopyAudio(Audio other)
        {
            Name = other.Name;
            _Owner = other.Owner;
            AudioMimeType = other.AudioMimeType;
            AudioData = other.AudioData;
            _AudioDataDecoded = other.AudioDataDecoded;
        }

        public override IBaseObject Clone()
        {
            return new Audio(this);
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

        public override string TypeLabel
        {
            get
            {
                return "Audio";
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

        public IDataBuffer AudioDataBuffer
        {
            get
            {
                return _AudioDataBuffer;
            }
            set
            {
                if (value != _AudioDataBuffer)
                {
                    _AudioDataBuffer = value;
                    ModifiedFlag = true;
                }
            }
        }

        public byte[] AudioData
        {
            get
            {
                if (_AudioDataBuffer == null)
                    return null;
                if (_AudioDataBuffer.Open(PortableFileMode.Open))
                {
                    byte[] data = _AudioDataBuffer.GetBytes(0, _AudioDataBuffer.Length);
                    _AudioDataBuffer.Close();
                    return data;
                }
                return null;
            }
            set
            {
                if (value == null)
                    _AudioDataBuffer = null;
                else
                    _AudioDataBuffer = new MemoryBuffer(value);
                ModifiedFlag = true;
            }
        }

        public Stream AudioStream
        {
            get
            {
                if (_AudioDataBuffer == null)
                    return null;
                return _AudioDataBuffer.GetReadableStream();
            }
            set
            {
                if (value == null)
                    _AudioDataBuffer = null;
                else
                {
                    int length = (int)value.Length;
                    byte[] audioData = new byte[length];
                    value.Read(audioData, 0, length);
                    if (_AudioDataBuffer != null)
                    {
                        _AudioDataBuffer.ReplaceBytes(audioData, 0, length, 0, _AudioDataBuffer.Length);
                    }
                    else
                        _AudioDataBuffer = new MemoryBuffer(audioData);
                }
            }
        }

        public WaveAudioBuffer AudioDataDecoded
        {
            get
            {
                return _AudioDataDecoded;
            }
            set
            {
                _AudioDataDecoded = value;
            }
        }

        public byte[] AudioDataDecodedData
        {
            get
            {
                if ((_AudioDataDecoded == null) || (_AudioDataDecoded.Storage == null))
                    return null;
                int length = (int)_AudioDataDecoded.Storage.Length;
                byte[] buffer = _AudioDataDecoded.Storage.GetBytes(0, length);
                return buffer;
            }
            set
            {
                if (value == null)
                    _AudioDataDecoded = null;
                else
                    _AudioDataDecoded = new WaveAudioBuffer(new MemoryBuffer(value));
            }
        }

        public int AudioSize
        {
            get
            {
                return (_AudioDataBuffer != null ? (int)AudioData.Length : 0);
            }
            set
            {
                if (_AudioDataBuffer == null)
                    _AudioDataBuffer = new MemoryBuffer(value);
                else
                    _AudioDataBuffer.Length = value;
                ModifiedFlag = true;
            }
        }

        public static bool IsSupportedFile(string fileName)
        {
            int count = _SupportedTypes.Count();
            int index;
            string extension = MediaUtilities.GetFileExtension(fileName);
            for (index = 0; index < count; index += 2)
            {
                if (_SupportedTypes[index + 1] == extension)
                    return true;
            }
            return false;
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            return FileSingleton.ReadAllBytes(filePath);
        }

        public bool ValidateFileName()
        {
            bool returnValue = false;

            if (!String.IsNullOrEmpty(KeyString) && !MediaUtilities.HasFileExtension(KeyString))
            {
                Key = KeyString + MediaUtilities.GetFileExtensionFromMimeType(_AudioMimeType);
                returnValue = true;
            }

            if (!String.IsNullOrEmpty(Name) && !MediaUtilities.HasFileExtension(Name))
            {
                Name += MediaUtilities.GetFileExtensionFromMimeType(_AudioMimeType);
                returnValue = true;
            }

            return returnValue;
        }

        public override byte[] BinaryData
        {
            get
            {
                byte[] baseData = base.BinaryData;
                int baseLength = baseData.Length + 4;
                int nameLength = (_Name != null ? ApplicationData.Encoding.GetBytes(_Name).Count() : 0) + 4;
                int typeLength = (_AudioMimeType != null ? _AudioMimeType.Length : 0) + 4;
                int dataLength = (_AudioDataBuffer != null ? (int)_AudioDataBuffer.Length : 0) + 4;
                byte[] audioData = AudioData;
                if (audioData == null)
                    audioData = new byte[0];
                byte[] data = new byte[baseLength + nameLength + typeLength + dataLength];
                int dindex = 0;
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, baseData);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _Name);
                ObjectUtilities.GetRecordBytesFromString(data, dindex, out dindex, _AudioMimeType);
                ObjectUtilities.GetRecordBytesFromByteArray(data, dindex, out dindex, audioData);
                return data;
            }
            set
            {
                int dindex = 0;
                base.BinaryData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
                _Name = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                _AudioMimeType = ObjectUtilities.GetStringFromRecordBytes(value, dindex, out dindex);
                byte[] audioData = ObjectUtilities.GetByteArrayFromRecordBytes(value, dindex, out dindex);
                _AudioDataBuffer = new MemoryBuffer(audioData);
                _AudioDataDecoded = null;
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
            if (_AudioDataBuffer != null)
                element.Add(new XElement("AudioData", ObjectUtilities.GetDataStringFromByteArray(AudioData, true)));
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
                case "AudioMimeType":
                    AudioMimeType = attributeValue;
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
                case "AudioData":
                    AudioData = ObjectUtilities.GetByteArrayFromDataString(childElement.Value);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            Audio otherAudio = other as Audio;

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
            return ObjectUtilities.CompareByteArrays(AudioData, otherAudio.AudioData);
        }

        public static int Compare(Audio object1, Audio object2)
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

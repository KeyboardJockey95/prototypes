using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class AudioMultiReference : BaseObjectLanguage
    {
        public List<AudioInstance> _AudioInstances;
        public int _AudioInstanceOrdinal;

        public AudioMultiReference(
                string name,
                LanguageID languageID,
                List<AudioInstance> audioInstances)
            : base(name, languageID)
        {
            _AudioInstances = audioInstances;
        }

        public AudioMultiReference(AudioMultiReference other)
            : base(other)
        {
            CopyAudioMultiReference(other);
            ModifiedFlag = false;
        }

        public AudioMultiReference(
                string name,
                AudioMultiReference other)
            : base(other)
        {
            CopyAudioMultiReference(name, other);
            ModifiedFlag = false;
        }

        public AudioMultiReference(XElement element)
        {
            OnElement(element);
        }

        public AudioMultiReference()
        {
            ClearAudioMultiReference();
        }

        public override void Clear()
        {
            base.Clear();
            ClearAudioMultiReference();
        }

        public void ClearAudioMultiReference()
        {
            _AudioInstances = null;
            _AudioInstanceOrdinal = 0;
        }

        public virtual void CopyAudioMultiReference(AudioMultiReference other)
        {
            _AudioInstances = other.CloneAudioInstances();
            _AudioInstanceOrdinal = other.AudioInstanceOrdinal;
        }

        public virtual void CopyAudioMultiReference(
            string name,
            AudioMultiReference other)
        {
            CopyAudioMultiReference(other);
            Rekey(name);
        }

        public override void Rekey(object newKey)
        {
            base.Rekey(newKey);

            if (_AudioInstances != null)
            {
                foreach (AudioInstance audioInstance in _AudioInstances)
                    audioInstance.Rekey(audioInstance.Key);
            }
        }

        public override IBaseObject Clone()
        {
            return new AudioMultiReference(this);
        }

        public List<AudioInstance> CloneAudioInstances()
        {
            if (_AudioInstances == null)
                return null;

            return new List<AudioInstance>(_AudioInstances);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Name=");
            sb.Append(Name);
            sb.Append(", ");
            sb.AppendLine("AudioInstances =");

            if (_AudioInstances != null)
            {
                foreach (AudioInstance audioInstance in _AudioInstances)
                    sb.AppendLine("{" + audioInstance.ToString() + "}");
            }

            return sb.ToString();
        }

        public override string Name
        {
            get
            {
                return KeyString;
            }
            set
            {
                if (value != KeyString)
                {
                    Key = value;
                }
            }
        }

        public List<AudioInstance> AudioInstances
        {
            get
            {
                return _AudioInstances;
            }
            set
            {
                if (value != _AudioInstances)
                {
                    _AudioInstances = value;
                    ModifiedFlag = true;
                }
            }
        }

        public AudioInstance GetAudioInstanceIndexed(int index)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if ((index < 0) || (index >= _AudioInstances.Count()))
                return null;

            return _AudioInstances[index];
        }

        public AudioInstance GetAudioInstanceByAttribute(string key, string value)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(key))
                return _AudioInstances.First();

            AudioInstance audioInstance = _AudioInstances.FirstOrDefault(x => x.MatchAttribute(key, value));

            return audioInstance;
        }

        public AudioInstance GetAudioInstanceByAttributes(List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if ((attributePatterns == null) || (attributePatterns.Count() == 0))
                return _AudioInstances.First();

            AudioInstance audioInstance = _AudioInstances.FirstOrDefault(x => x.MatchAttributesExact(attributePatterns));

            if (audioInstance != null)
                return audioInstance;

            audioInstance = _AudioInstances.FirstOrDefault(x => x.MatchAttributesPresent(attributePatterns));

            return audioInstance;
        }

        public AudioInstance GetAudioInstanceBySourceAndAttributes(
            string sourceName,
            List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            return _AudioInstances.FirstOrDefault(x => ((x.SourceName == sourceName) && x.MatchAttributesPresent(attributePatterns)));
        }

        public AudioInstance GetAudioInstanceBySourceAndAttribute(
            string sourceName,
            string key,
            string value)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(key))
                return null;

            return _AudioInstances.FirstOrDefault(x => ((x.SourceName == sourceName) && x.MatchAttribute(key, value)));
        }

        public AudioInstance GetAudioInstanceBySource(string sourceName)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(sourceName))
                return _AudioInstances.First();

            return _AudioInstances.FirstOrDefault(x => x.SourceName == sourceName);
        }

        public List<AudioInstance> GetAudioInstancesBySource(string sourceName)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(sourceName))
                return new List<AudioInstance>(_AudioInstances);

            return _AudioInstances.Where(x => x.SourceName == sourceName).ToList();
        }

        public List<AudioInstance> GetAudioInstancesByNative()
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            return _AudioInstances.Where(x => x.SourceName != AudioInstance.SynthesizedSourceName).ToList();
        }

        public List<AudioInstance> GetAudioInstancesBySynthesizer()
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            return _AudioInstances.Where(x => x.SourceName == AudioInstance.SynthesizedSourceName).ToList();
        }

        public AudioInstance GetAudioInstanceByFileName(string fileName)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(fileName))
                return _AudioInstances.First();

            return _AudioInstances.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.FileName, fileName));
        }

        public int GetAudioInstanceIndex(AudioInstance audioInstance)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return -1;

            return _AudioInstances.IndexOf(audioInstance);
        }

        public void AddAudioInstance(AudioInstance audioInstance)
        {
            if (_AudioInstances == null)
                _AudioInstances = new List<AudioInstance>() { audioInstance };
            else
                _AudioInstances.Add(audioInstance);

            string fileBase = MediaUtilities.GetBaseFileName(audioInstance.FileName);
            int ordinal = ObjectUtilities.GetIntegerFromStringEnd(fileBase, 0);

            if (ordinal >= _AudioInstanceOrdinal)
                _AudioInstanceOrdinal = ordinal + 1;

            ModifiedFlag = true;
        }

        public bool InsertAudioInstance(int index, AudioInstance audioInstance)
        {
            string fileBase;
            int ordinal;

            if (_AudioInstances == null)
            {
                if (index == 0)
                {
                    _AudioInstances = new List<AudioInstance>() { audioInstance };
                    ModifiedFlag = true;

                    fileBase = MediaUtilities.GetBaseFileName(audioInstance.FileName);
                    ordinal = ObjectUtilities.GetIntegerFromStringEnd(fileBase, 0);

                    if (ordinal >= _AudioInstanceOrdinal)
                        _AudioInstanceOrdinal = ordinal + 1;

                    return true;
                }

                return false;
            }

            if ((index < 0) || (index > _AudioInstances.Count()))
                return false;

            _AudioInstances.Insert(index, audioInstance);
            ModifiedFlag = true;

            fileBase = MediaUtilities.GetBaseFileName(audioInstance.FileName);
            ordinal = ObjectUtilities.GetIntegerFromStringEnd(fileBase, 0);

            if (ordinal >= _AudioInstanceOrdinal)
                _AudioInstanceOrdinal = ordinal + 1;

            return true;
        }

        public void DeleteAudioInstanceIndexed(int index)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return;

            if ((index < 0) || (index >= _AudioInstances.Count()))
                return;

            _AudioInstances.RemoveAt(index);
            ModifiedFlag = true;
        }

        public void DeleteAudioInstance(AudioInstance audioInstance)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return;

            _AudioInstances.Remove(audioInstance);
            ModifiedFlag = true;
        }

        public void DeleteAllAudioInstances()
        {
            if (_AudioInstances != null)
            {
                _AudioInstances = null;
                ModifiedFlag = true;
            }
        }

        public int AudioInstanceCount()
        {
            if (_AudioInstances != null)
                return _AudioInstances.Count();

            return 0;
        }

        public bool HasAudioInstances()
        {
            if ((_AudioInstances != null) && (_AudioInstances.Count() != 0))
                return true;

            return false;
        }

        public bool HasTaggedAudioInstances(string tag)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return false;

            foreach (AudioInstance audioInstance in _AudioInstances)
            {
                if (audioInstance.IsTagged(tag))
                    return true;
            }

            return false;
        }

        public bool HasTaggedAudioInstances(List<string> tagsList)
        {
            if ((_AudioInstances == null) || (_AudioInstances.Count() == 0))
                return false;

            if ((tagsList == null) || (tagsList.Count() == 0))
                return false;

            foreach (AudioInstance audioInstance in _AudioInstances)
            {
                if (audioInstance.IsAnyTagged(tagsList))
                    return true;
            }

            return false;
        }

        public int AudioInstanceOrdinal
        {
            get
            {
                return _AudioInstanceOrdinal;
            }
            set
            {
                if (value != _AudioInstanceOrdinal)
                {
                    _AudioInstanceOrdinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int AllocateAudioInstanceOrdinal()
        {
            ModifiedFlag = true;
            return _AudioInstanceOrdinal++;
        }

        public string AllocateAudioFileName()
        {
            string fileName = MediaUtilities.FileFriendlyName(Name) + "_" + AllocateAudioInstanceOrdinal().ToString() + MediaUtilities.AudioFileExtension;
            return fileName;
        }

        public string AllocateAudioTildeUrl()
        {
            string url = GetAudioTildeDirectoryUrl() + "/" + AllocateAudioFileName();
            return url;
        }

        public string AllocateAudioFilePath()
        {
            string filePath = ApplicationData.MapToFilePath(AllocateAudioTildeUrl());
            return filePath;
        }

        public bool IsAllInstancesExist()
        {
            if ((_AudioInstances != null) && (_AudioInstances.Count() != 0))
            {
                foreach (AudioInstance audioInstance in _AudioInstances)
                {
                    if (!audioInstance.Exists(LanguageID))
                        return false;
                }

                return true;
            }

            return false;
        }

        public bool IsAnyInstancesExist()
        {
            if ((_AudioInstances != null) && (_AudioInstances.Count() != 0))
            {
                foreach (AudioInstance audioInstance in _AudioInstances)
                {
                    if (audioInstance.Exists(LanguageID))
                        return true;
                }
            }

            return false;
        }

        public string GetAudioTildeDirectoryUrl()
        {
            return MediaUtilities.GetAudioTildeDirectoryUrl(LanguageID);
        }

        public string GetAudioDirectoryPath()
        {
            return ApplicationData.MapToFilePath(MediaUtilities.GetAudioTildeDirectoryUrl(LanguageID));
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_AudioInstances != null)
                {
                    foreach (AudioInstance audioInstance in _AudioInstances)
                    {
                        if (audioInstance.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                ModifiedFlag = value;

                if (_AudioInstances != null)
                {
                    foreach (AudioInstance audioInstance in _AudioInstances)
                        audioInstance.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((_AudioInstances != null) && (_AudioInstances.Count() != 0))
            {
                foreach (AudioInstance audioInstance in _AudioInstances)
                    element.Add(audioInstance.GetElement("AudioInstance"));
            }
            if (_AudioInstanceOrdinal != 0)
                element.Add(new XAttribute("AudioInstanceOrdinal", _AudioInstanceOrdinal));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "AudioInstanceOrdinal":
                    _AudioInstanceOrdinal = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
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
                case "AudioInstance":
                    {
                        AudioInstance audioInstance = new AudioInstance(childElement);
                        AddAudioInstance(audioInstance);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            AudioMultiReference otherAudio = other as AudioMultiReference;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBaseLists<AudioInstance>(_AudioInstances, otherAudio.AudioInstances);
            return diff;
        }

        public static int Compare(AudioMultiReference object1, AudioMultiReference object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static bool FindAudioRecordAndInstanceWithFileName(
            List<AudioMultiReference> audioRecords,
            string fileName,
            out AudioMultiReference audioRecord,
            out AudioInstance audioInstance)
        {
            audioRecord = null;
            audioInstance = null;

            if (audioRecords == null)
                return false;

            foreach (AudioMultiReference record in audioRecords)
            {
                AudioInstance instance = record.GetAudioInstanceByFileName(fileName);

                if (instance != null)
                {
                    audioRecord = record;
                    audioInstance = instance;
                    return true;
                }
            }

            return false;
        }
    }
}

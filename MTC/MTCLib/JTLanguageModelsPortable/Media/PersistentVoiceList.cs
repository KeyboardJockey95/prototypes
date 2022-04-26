using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public class PersistentVoiceList : PersistentObject
    {
        protected VoiceList _VoiceList;

        public PersistentVoiceList(
                string name,
                string filePath)
            : base(name, filePath)
        {
            _VoiceList = new VoiceList();
            Load();
        }

        public PersistentVoiceList(PersistentVoiceList other)
            : base(other)
        {
            CopyPersistentVoiceList(other);
            ModifiedFlag = false;
        }

        public PersistentVoiceList(XElement element)
        {
            OnElement(element);
        }

        public PersistentVoiceList()
        {
            ClearPersistentVoiceList();
        }

        public override void Clear()
        {
            base.Clear();
            ClearPersistentVoiceList();
        }

        public void ClearPersistentVoiceList()
        {
            _VoiceList.ClearVoiceList();
        }

        public void CopyPersistentVoiceList(PersistentVoiceList other)
        {
            _VoiceList.CopyVoiceList(other.VoiceList);
        }

        public override IBaseObject Clone()
        {
            return new PersistentVoiceList(this);
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
                    Key = value;
            }
        }

        public VoiceList VoiceList
        {
            get
            {
                return _VoiceList;
            }
            set
            {
                if (value != _VoiceList)
                {
                    _VoiceList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Voice> Voices
        {
            get
            {
                return _VoiceList.Voices;
            }
            set
            {
                if (value != _VoiceList.Voices)
                {
                    _VoiceList.Voices = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Voice GetVoiceIndexed(int index)
        {
            return _VoiceList.GetVoiceIndexed(index);
        }

        public Voice GetVoice(string name)
        {
            return _VoiceList.GetVoice(name);
        }

        public Voice GetVoiceByNameAndLanguageID(string name, LanguageID languageID)
        {
            return _VoiceList.GetVoiceByNameAndLanguageID(name, languageID);
        }

        public Voice GetVoiceByAttribute(List<KeyValuePair<string, string>> attributePatterns)
        {
            return _VoiceList.GetVoiceByAttribute(attributePatterns);
        }

        public Voice GetVoiceBySource(string sourceName)
        {
            return _VoiceList.GetVoiceBySource(sourceName);
        }

        public Voice GetVoiceByLanguageID(LanguageID languageID)
        {
            return _VoiceList.GetVoiceByLanguageID(languageID);
        }

        public List<Voice> GetVoicesByLanguageID(LanguageID languageID)
        {
            return _VoiceList.GetVoicesByLanguageID(languageID);
        }

        public void AddVoice(Voice voice)
        {
            bool saveModified = Modified;

            _VoiceList.AddVoice(voice);

            if (_VoiceList.Modified)
                ModifiedFlag = true;

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;
        }

        public bool InsertVoice(int index, Voice voice)
        {
            bool saveModified = Modified;

            bool returnValue = _VoiceList.InsertVoice(index, voice);

            if (_VoiceList.Modified)
                ModifiedFlag = true;

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;

            return returnValue;
        }

        public void DeleteVoiceIndexed(int index)
        {
            bool saveModified = Modified;

            _VoiceList.DeleteVoiceIndexed(index);

            if (_VoiceList.Modified)
                ModifiedFlag = true;

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;
        }

        public void DeleteVoice(Voice voice)
        {
            bool saveModified = Modified;

            _VoiceList.DeleteVoice(voice);

            if (_VoiceList.Modified)
                ModifiedFlag = true;

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;
        }

        public void DeleteAllVoices()
        {
            bool saveModified = Modified;

            _VoiceList.DeleteAllVoices();

            if (_VoiceList.Modified)
                ModifiedFlag = true;

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;
        }

        public override XElement GetElement(string name)
        {
            XElement element = _VoiceList.GetElement(name);
            return element;
        }

        public override void OnElement(XElement element)
        {
            _VoiceList.OnElement(element);
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            return _VoiceList.OnAttribute(attribute);
        }

        public override bool OnChildElement(XElement childElement)
        {
            return _VoiceList.OnChildElement(childElement);
        }
    }
}

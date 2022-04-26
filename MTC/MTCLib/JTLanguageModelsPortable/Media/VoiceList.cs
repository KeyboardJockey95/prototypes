using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Media
{
    public class VoiceList : BaseObjectKeyed
    {
        public List<Voice> _Voices;

        public VoiceList(
                string name,
                List<Voice> voices)
            : base(name)
        {
            _Voices = voices;
        }

        public VoiceList(VoiceList other)
            : base(other)
        {
            CopyVoiceList(other);
            ModifiedFlag = false;
        }

        public VoiceList(XElement element)
        {
            OnElement(element);
        }

        public VoiceList()
        {
            ClearVoiceList();
        }

        public override void Clear()
        {
            base.Clear();
            ClearVoiceList();
        }

        public void ClearVoiceList()
        {
            _Voices = null;
        }

        public virtual void CopyVoiceList(VoiceList other)
        {
            _Voices = other.CloneVoices();
        }

        public override IBaseObject Clone()
        {
            return new VoiceList(this);
        }

        public List<Voice> CloneVoices()
        {
            if (_Voices == null)
                return null;

            List<Voice> voices = new List<Voice>();

            foreach (Voice voice in _Voices)
                voices.Add(new Voice(voice));

            return voices;
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

        public List<Voice> Voices
        {
            get
            {
                return _Voices;
            }
            set
            {
                if (value != _Voices)
                {
                    _Voices = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Voice GetVoiceIndexed(int index)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            if ((index < 0) || (index >= _Voices.Count()))
                return null;

            return _Voices[index];
        }

        public Voice GetVoice(string name)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            Voice voice = _Voices.FirstOrDefault(x => x.Name == name);

            if (voice == null)
                voice = _Voices.FirstOrDefault(x => x.Name.Contains(name));

            return voice;
        }

        public Voice GetVoiceByNameAndLanguageID(string name, LanguageID languageID)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            Voice voice = _Voices.FirstOrDefault(x => (x.Name == name) && (x.LanguageID == languageID));

            if (voice == null)
                voice = _Voices.FirstOrDefault(x => x.Name.Contains(name) && (x.LanguageID == languageID));

            return voice;
        }

        public Voice GetVoiceByAttribute(List<KeyValuePair<string, string>> attributePatterns)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            if ((attributePatterns == null) || (attributePatterns.Count() == 0))
                return _Voices.First();

            Voice voice = _Voices.FirstOrDefault(x => x.MatchAttributesExact(attributePatterns));

            if (voice != null)
                return voice;

            voice = _Voices.FirstOrDefault(x => x.MatchAttributesPresent(attributePatterns));

            return voice;
        }

        public Voice GetVoiceBySource(string sourceName)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            if (String.IsNullOrEmpty(sourceName))
                return _Voices.First();

            return _Voices.FirstOrDefault(x => x.SourceName == sourceName);
        }

        public Voice GetVoiceByLanguageID(LanguageID languageID)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            return _Voices.FirstOrDefault(x => x.LanguageID.LanguageCode == languageID.LanguageCode);
        }

        public List<Voice> GetVoicesByLanguageID(LanguageID languageID)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return null;

            return _Voices.Where(x => x.LanguageID.LanguageCode == languageID.LanguageCode).ToList();
        }

        public void AddVoice(Voice voice)
        {
            if (_Voices == null)
                _Voices = new List<Voice>() { voice };
            else
                _Voices.Add(voice);

            ModifiedFlag = true;
        }

        public bool InsertVoice(int index, Voice voice)
        {
            if (_Voices == null)
            {
                if (index == 0)
                {
                    _Voices = new List<Voice>() { voice };
                    ModifiedFlag = true;
                    return true;
                }

                return false;
            }

            if ((index < 0) || (index > _Voices.Count()))
                return false;

            _Voices.Insert(index, voice);
            ModifiedFlag = true;

            return true;
        }

        public void DeleteVoiceIndexed(int index)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return;

            if ((index < 0) || (index >= _Voices.Count()))
                return;

            _Voices.RemoveAt(index);
            ModifiedFlag = true;
        }

        public void DeleteVoice(Voice voice)
        {
            if ((_Voices == null) || (_Voices.Count() == 0))
                return;

            _Voices.Remove(voice);
            ModifiedFlag = true;
        }

        public void DeleteAllVoices()
        {
            if (_Voices != null)
            {
                _Voices = null;
                ModifiedFlag = true;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((_Voices != null) && (_Voices.Count() != 0))
            {
                foreach (Voice voice in _Voices)
                    element.Add(voice.GetElement("Voice"));
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Voice":
                    {
                        Voice voice = new Voice(childElement);
                        AddVoice(voice);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            VoiceList otherAudio = other as VoiceList;

            if (otherAudio == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBaseLists<Voice>(_Voices, otherAudio.Voices);
            return diff;
        }

        public static int Compare(VoiceList object1, VoiceList object2)
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

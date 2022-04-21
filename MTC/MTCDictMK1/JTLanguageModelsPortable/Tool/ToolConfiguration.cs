using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tool
{
    public class ToolConfiguration : BaseObjectTitled
    {
        // Owning profile - not owned and doesn't affect modified state.
        protected ToolProfile _Profile;
        // Tool or flash card side descriptions.
        protected List<ToolSide> _CardSides;
        // Hybrid configuration sub-configurations.
        protected List<string> _SubConfigurationKeys;

        public ToolConfiguration(ToolProfile toolProfile, string key, MultiLanguageString title, MultiLanguageString description,
                string label, int index, List<ToolSide> cardSides)
            : base(key, title, description, null, null, label, null, index, false, null, null, null)
        {
            _Profile = toolProfile;
            _CardSides = cardSides;
            _SubConfigurationKeys = null;
        }

        public ToolConfiguration(ToolConfiguration other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolConfiguration(object key, ToolConfiguration other)
            : base(key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolConfiguration(XElement element)
        {
            OnElement(element);
        }

        public ToolConfiguration()
        {
            ClearToolConfiguration();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolConfiguration();
        }

        public void ClearToolConfiguration()
        {
            _Profile = null;
            _CardSides = null;
            _SubConfigurationKeys = null;
        }

        public void Copy(ToolConfiguration other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            base.Copy(other);

            _Profile = other.Profile;

            if (other.CardSides != null)
            {
                _CardSides = new List<ToolSide>(other.CardSideCount());

                foreach (ToolSide flashSide in other.CardSides)
                    AddCardSide(new ToolSide(flashSide));
            }
            else
                _CardSides = null;

            if (other.SubConfigurationKeys != null)
                _SubConfigurationKeys = new List<string>(other.SubConfigurationKeys);
            else
                _SubConfigurationKeys = null;
        }

        public ToolProfile Profile
        {
            get
            {
                return _Profile;
            }
            set
            {
                _Profile = value;
            }
        }

        public SelectorAlgorithmCode SelectorAlgorithm
        {
            get
            {
                if (Profile != null)
                    return Profile.SelectorAlgorithm;
                return ToolProfile.DefaultSelectorAlgorithm;
            }
        }

        public bool IsRandomUnique
        {
            get
            {
                if (Profile != null)
                    return Profile.IsRandomUnique;
                return ToolProfile.DefaultIsRandomUnique;
            }
        }

        public bool IsRandomNew
        {
            get
            {
                if (Profile != null)
                    return Profile.IsRandomNew;
                return ToolProfile.DefaultIsRandomNew;
            }
        }

        public bool IsAdaptiveMixNew
        {
            get
            {
                if (Profile != null)
                    return Profile.IsAdaptiveMixNew;
                return ToolProfile.DefaultIsAdaptiveMixNew;
            }
        }

        public int ReviewLevel
        {
            get
            {
                if (Profile != null)
                    return Profile.ReviewLevel;
                return ToolProfile.DefaultReviewLevel;
            }
        }

        public int ChunkSize
        {
            get
            {
                if (Profile != null)
                    return Profile.ChunkSize;
                return ToolProfile.DefaultChunkSize;
            }
        }

        public int HistorySize
        {
            get
            {
                if (Profile != null)
                    return Profile.HistorySize;
                return ToolProfile.DefaultHistorySize;
            }
        }

        public bool IsShowIndex
        {
            get
            {
                if (Profile != null)
                    return Profile.IsShowIndex;
                return ToolProfile.DefaultIsShowIndex;
            }
        }

        public bool IsShowOrdinal
        {
            get
            {
                if (Profile != null)
                    return Profile.IsShowOrdinal;
                return ToolProfile.DefaultIsShowOrdinal;
            }
        }

        public List<TimeSpan> IntervalTable
        {
            get
            {
                if (Profile != null)
                    return Profile.IntervalTable;
                return ToolProfile.DefaultSpacedIntervalTable;
            }
        }

        public bool IsHybrid()
        {
            if (Label == "Hybrid")
                return true;

            return false;
        }

        public List<ToolSide> CardSides
        {
            get
            {
                return _CardSides;
            }
            set
            {
                if (value != _CardSides)
                {
                    _CardSides = value;
                    ModifiedFlag = true;
                }
            }
        }

        public TimeSpan GetTimeOffset(float currentGrade, float lastGrade, float averageGrade, int stage)
        {
            if (Profile != null)
                return Profile.GetTimeOffset(currentGrade, lastGrade, averageGrade, stage);
            return TimeSpan.Zero;
        }

        // Get card side description (side = (1|2|...).
        public ToolSide GetCardSide(int side)
        {
            int index = side - 1;

            if ((_CardSides != null) && (index >= 0) && (index < _CardSides.Count()))
                return _CardSides[index];

            return null;
        }

        // Set a card side.
        public void SetCardSide(int side, ToolSide cardSide)
        {
            int index = side - 1;

            if (_CardSides == null)
                _CardSides = new List<ToolSide>(side) { cardSide };
            else
            {
                while (_CardSides.Count() < index)
                    _CardSides.Add(new ToolSide());

                if (_CardSides.Count() == index)
                    _CardSides.Add(cardSide);
                else if (index < _CardSides.Count())
                    _CardSides[index] = cardSide;
            }

            ModifiedFlag = true;
        }

        // Add a card side.
        public void AddCardSide(ToolSide cardSide)
        {
            if (_CardSides == null)
                _CardSides = new List<ToolSide>(2) { cardSide };
            else
                _CardSides.Add(cardSide);

            ModifiedFlag = true;
        }

        // Delete card side description (side = (1|2|...).
        public bool DeleteCardSide(int side)
        {
            if (_CardSides != null)
            {
                int index = side - 1;

                if ((index >= 0) && (index < _CardSides.Count()))
                {
                    _CardSides.RemoveAt(index);
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        // Delete all card side descriptions.
        public void DeleteAllCardSides()
        {
            if ((_CardSides != null) && (_CardSides.Count() != 0))
            {
                _CardSides.Clear();
                ModifiedFlag = true;
            }
        }

        // Get card side Count.
        public int CardSideCount()
        {
            if (_CardSides != null)
                return _CardSides.Count();

            return 0;
        }

        // Get card side Count.
        public void SetCardSideCount(int cardSideCount)
        {
            int currentSideCount = CardSideCount();

            if (cardSideCount == currentSideCount)
                return;

            if (CardSides == null)
                CardSides = new List<ToolSide>(cardSideCount);

            while (currentSideCount < cardSideCount)
            {
                currentSideCount++;
                SetCardSide(currentSideCount, new ToolSide(currentSideCount, Profile));
            }

            if (currentSideCount > cardSideCount)
                CardSides.RemoveRange(cardSideCount, currentSideCount - cardSideCount);

            ModifiedFlag = true;
        }

        public List<string> SubConfigurationKeys
        {
            get
            {
                return _SubConfigurationKeys;
            }
            set
            {
                if (value != _SubConfigurationKeys)
                {
                    _SubConfigurationKeys = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasSubConfiguration(string configurationKey)
        {
            if (_SubConfigurationKeys == null)
                return false;

            return _SubConfigurationKeys.Contains(configurationKey);
        }

        public string GetSubConfigurationKeyIndexed(int index)
        {
            if (_SubConfigurationKeys == null)
                return String.Empty;

            if ((index < 0) || (index >= _SubConfigurationKeys.Count))
                return String.Empty;

            return _SubConfigurationKeys[index];
        }

        public int SubConfigurationCount()
        {
            if (_SubConfigurationKeys == null)
                return 0;

            return _SubConfigurationKeys.Count;
        }

        public bool UsesMedia()
        {
            if (_CardSides != null)
            {
                foreach (ToolSide side in _CardSides)
                {
                    if (side.HasAudioOutput || side.HasPictureOutput || side.HasVideoOutput || side.HasAudioInput || side.HasVoiceRecognition)
                        return true;
                }
            }

            return false;
        }

        public bool UsesAudioVideo()
        {
            if (_CardSides != null)
            {
                foreach (ToolSide side in _CardSides)
                {
                    if (side.HasAudioOutput || side.HasVideoOutput || side.HasAudioInput || side.HasVoiceRecognition)
                        return true;
                }
            }

            return false;
        }

        public bool UsesAudioInput()
        {
            if (_CardSides != null)
            {
                foreach (ToolSide side in _CardSides)
                {
                    if (side.HasAudioInput || side.HasVoiceRecognition)
                        return true;
                }
            }

            return false;
        }

        public bool UsesVoiceRecognition()
        {
            if (_CardSides != null)
            {
                foreach (ToolSide side in _CardSides)
                {
                    if (side.HasVoiceRecognition)
                        return true;
                }
            }

            return false;
        }

        private bool AreLanguageIDsTarget(List<LanguageDescriptor> languageDescriptors, List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0) || (languageDescriptors == null))
                return false;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.LanguageID.LanguageCode == languageID.LanguageCode);

                if (languageDescriptor == null)
                    return false;

                if (!languageDescriptor.Name.StartsWith("Target"))
                    return false;
            }

            return true;
        }

        private bool IsLanguageIDTarget(List<LanguageDescriptor> languageDescriptors, LanguageID languageID)
        {
            if ((languageID == null) || (languageDescriptors == null))
                return false;

            LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.LanguageID.LanguageCode == languageID.LanguageCode);

            if (languageDescriptor == null)
                return false;

            if (!languageDescriptor.Name.StartsWith("Target"))
                return false;

            return true;
        }

        public List<LanguageDescriptor> GetOrderedLanguageDescriptors(List<LanguageDescriptor> sourceDescriptors)
        {
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>();

            if (_CardSides != null)
            {
                foreach (ToolSide side in _CardSides)
                {
                    List<LanguageID> languageIDs = side.TextLanguageIDs;

                    if ((languageIDs == null) || (languageIDs.Count() == 0))
                        languageIDs = side.WriteLanguageIDs;

                    if ((languageIDs == null) || (languageIDs.Count() == 0))
                        languageIDs = side.MediaLanguageIDs;

                    if ((languageIDs == null) || (languageIDs.Count() == 0))
                        continue;

                    foreach (LanguageID languageID in languageIDs)
                    {
                        LanguageDescriptor languageDescriptor = sourceDescriptors.FirstOrDefault(
                            x => x.LanguageID == languageID);

                        if (languageDescriptor == null)
                            languageDescriptor = sourceDescriptors.FirstOrDefault(
                                x => x.LanguageID.LanguageCode == languageID.LanguageCode);

                        if (languageDescriptor == null)
                            continue;

                        if (!languageDescriptors.Contains(languageDescriptor))
                            languageDescriptors.Add(languageDescriptor);
                    }
                }
            }

            return languageDescriptors;
        }

        public void SetDefaultProfileInformation()
        {
            if ((_CardSides == null) || (Profile == null))
                return;

            foreach (ToolSide cardSide in _CardSides)
                cardSide.SetDefaultProfileInformation(Profile);
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_CardSides != null)
                {
                    foreach (ToolSide cardSize in _CardSides)
                    {
                        if (cardSize.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_CardSides != null)
                {
                    foreach (ToolSide cardSize in _CardSides)
                        cardSize.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_CardSides != null)
            {
                foreach (ToolSide cardSide in _CardSides)
                    element.Add(cardSide.GetElement("CardSide"));
            }

            if (_SubConfigurationKeys != null)
                element.Add(new XElement("SubConfigurationKeys", TextUtilities.GetStringFromStringList(_SubConfigurationKeys)));

            return element;
        }

       public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "CardSide":
                    AddCardSide(new ToolSide(childElement));
                    break;
                case "SubConfigurationKeys":
                    _SubConfigurationKeys = TextUtilities.GetStringListFromString(childElement.Value);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolConfiguration otherObject = other as ToolConfiguration;
            int diff;

            if (otherObject == null)
                return base.Compare(other);

            diff = base.Compare(other);
            return diff;
        }

        public static int Compare(ToolConfiguration object1, ToolConfiguration object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareToolConfigurationLists(List<ToolConfiguration> list1, List<ToolConfiguration> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ToolConfiguration>(list1, list2);
        }
    }
}

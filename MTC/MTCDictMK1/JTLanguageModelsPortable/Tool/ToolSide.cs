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
    public class ToolSide : BaseObjectKeyed
    {
        protected bool _HasTextOutput;
        protected bool _HasPictureOutput;
        protected bool _HasAudioOutput;
        protected bool _HasVideoOutput;
        protected bool _HasDescrambleInput;
        protected bool _HasChoiceInput;
        protected bool _HasBlanksInput;
        protected bool _HasTextInput;
        protected bool _HasAudioInput;
        protected bool _HasVoiceRecognition;
        protected List<LanguageID> _TextLanguageIDs;
        protected List<LanguageID> _MediaLanguageIDs;
        protected List<LanguageID> _WriteLanguageIDs;
        protected string _TextFormat;
        protected string _FontFamily;
        protected string _FlashFontSize;
        protected string _ListFontSize;
        protected int _MaximumLineLength;

        public ToolSide(int side,
                bool hasTextOutput, bool hasPictureOutput, bool hasAudioOutput, bool hasVideoOutput,
                bool hasDescrambleInput, bool hasChoiceInput, bool hasBlanksInput, bool hasTextInput,
                bool hasAudioInput, bool hasVoiceRecognition,
                List<LanguageID> textLanguageIDs, List<LanguageID> writeLanguageIDs, LanguageID mediaLanguageID,
                string textFormat, string fontFamily, string flashFontSize, string listFontSize,
                int maximumLineLength)
            : base(side)
        {
            _HasTextOutput = hasTextOutput;
            _HasPictureOutput = hasPictureOutput;
            _HasAudioOutput = hasAudioOutput;
            _HasVideoOutput = hasVideoOutput;
            _HasDescrambleInput = hasDescrambleInput;
            _HasChoiceInput = hasChoiceInput;
            _HasBlanksInput = hasBlanksInput;
            _HasTextInput = hasTextInput;
            _HasAudioInput = hasAudioInput;
            _HasVoiceRecognition = hasVoiceRecognition;
            _TextLanguageIDs = textLanguageIDs;
            _WriteLanguageIDs = writeLanguageIDs;
            _MediaLanguageIDs = (mediaLanguageID != null ? new List<LanguageID>(1) { mediaLanguageID } : null);
            _TextFormat = textFormat;
            _FontFamily = fontFamily;
            _FlashFontSize = flashFontSize;
            _ListFontSize = listFontSize;
            _MaximumLineLength = maximumLineLength;
        }

        public ToolSide(int side, ToolProfile toolProfile)
            : base(side)
        {
            ClearToolSide();

            if (toolProfile != null)
            {
                _FontFamily = toolProfile.FontFamily;
                _FlashFontSize = toolProfile.FlashFontSize;
                _ListFontSize = toolProfile.ListFontSize;
            }
        }

        public ToolSide(ToolSide other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolSide(XElement element)
        {
            OnElement(element);
        }

        public ToolSide()
        {
            ClearToolSide();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolSide();
        }

        public void ClearToolSide()
        {
            _HasTextOutput = false;
            _HasPictureOutput = false;
            _HasAudioOutput = false;
            _HasVideoOutput = false;
            _HasDescrambleInput = false;
            _HasChoiceInput = false;
            _HasBlanksInput = false;
            _HasTextInput = false;
            _HasAudioInput = false;
            _HasVoiceRecognition = false;
            _TextLanguageIDs = null;
            _MediaLanguageIDs = null;
            _TextFormat = null;
            _FontFamily = null;
            _FlashFontSize = null;
            _ListFontSize = null;
            _MaximumLineLength = 0;
        }

        public void Copy(ToolSide other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            HasTextOutput = other.HasTextOutput;
            HasPictureOutput = other.HasPictureOutput;
            HasAudioOutput = other.HasAudioOutput;
            HasVideoOutput = other.HasVideoOutput;
            HasTextInput = other.HasTextInput;
            HasChoiceInput = other.HasChoiceInput;
            HasBlanksInput = other.HasBlanksInput;
            HasAudioInput = other.HasAudioInput;
            HasVoiceRecognition = other.HasVoiceRecognition;

            if (other.TextLanguageIDs != null)
                TextLanguageIDs = new List<LanguageID>(other.TextLanguageIDs);
            else
                TextLanguageIDs = null;

            if (other.MediaLanguageIDs != null)
                MediaLanguageIDs = new List<LanguageID>(other.MediaLanguageIDs);
            else
                MediaLanguageIDs = null;

            if (other.WriteLanguageIDs != null)
                WriteLanguageIDs = new List<LanguageID>(other.WriteLanguageIDs);
            else
                WriteLanguageIDs = null;

            TextFormat = other.TextFormat;
            FontFamily = other.FontFamily;
            FlashFontSize = other.FlashFontSize;
            ListFontSize = other.ListFontSize;
            MaximumLineLength = other.MaximumLineLength;
        }

        public int Side
        {
            get
            {
                return KeyInt;
            }
            set
            {
                if (value != KeyInt)
                {
                    _Key = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasTextOutput
        {
            get
            {
                return _HasTextOutput;
            }
            set
            {
                if (value != _HasTextOutput)
                {
                    _HasTextOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasPictureOutput
        {
            get
            {
                return _HasPictureOutput;
            }
            set
            {
                if (value != _HasPictureOutput)
                {
                    _HasPictureOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasAudioOutput
        {
            get
            {
                return _HasAudioOutput;
            }
            set
            {
                if (value != _HasAudioOutput)
                {
                    _HasAudioOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasVideoOutput
        {
            get
            {
                return _HasVideoOutput;
            }
            set
            {
                if (value != _HasVideoOutput)
                {
                    _HasVideoOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasDescrambleInput
        {
            get
            {
                return _HasDescrambleInput;
            }
            set
            {
                if (value != _HasDescrambleInput)
                {
                    _HasDescrambleInput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasChoiceInput
        {
            get
            {
                return _HasChoiceInput;
            }
            set
            {
                if (value != _HasChoiceInput)
                {
                    _HasChoiceInput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasBlanksInput
        {
            get
            {
                return _HasBlanksInput;
            }
            set
            {
                if (value != _HasBlanksInput)
                {
                    _HasBlanksInput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasTextInput
        {
            get
            {
                return _HasTextInput;
            }
            set
            {
                if (value != _HasTextInput)
                {
                    _HasTextInput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasAudioInput
        {
            get
            {
                return _HasAudioInput;
            }
            set
            {
                if (value != _HasAudioInput)
                {
                    _HasAudioInput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasVoiceRecognition
        {
            get
            {
                return _HasVoiceRecognition;
            }
            set
            {
                if (value != _HasVoiceRecognition)
                {
                    _HasVoiceRecognition = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageID> TextLanguageIDs
        {
            get
            {
                return _TextLanguageIDs;
            }
            set
            {
                if (value != _TextLanguageIDs)
                {
                    if (LanguageID.CompareLanguageIDLists(value, _TextLanguageIDs) != 0)
                        ModifiedFlag = true;

                    _TextLanguageIDs = value;
                }
            }
        }

        public string TextLanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(_TextLanguageIDs, "|", "|", "|");
            }
            set
            {
                char[] delimiters = { '|' };
                TextLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(value, delimiters);
            }
        }

        public List<LanguageID> MediaLanguageIDs
        {
            get
            {
                return _MediaLanguageIDs;
            }
            set
            {
                if (value != _MediaLanguageIDs)
                {
                    if (LanguageID.CompareLanguageIDLists(value, _MediaLanguageIDs) != 0)
                        ModifiedFlag = true;

                    _MediaLanguageIDs = value;
                }
            }
        }

        public string MediaLanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(_MediaLanguageIDs, "|", "|", "|");
            }
            set
            {
                char[] delimiters = { '|' };
                MediaLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(value, delimiters);
            }
        }

        public List<LanguageID> WriteLanguageIDs
        {
            get
            {
                return _WriteLanguageIDs;
            }
            set
            {
                if (value != _WriteLanguageIDs)
                {
                    if (LanguageID.CompareLanguageIDLists(value, _WriteLanguageIDs) != 0)
                        ModifiedFlag = true;

                    _WriteLanguageIDs = value;
                }
            }
        }

        public string WriteLanguagesKey
        {
            get
            {
                return LanguageID.ConvertLanguageIDListToDelimitedString(_WriteLanguageIDs, "|", "|", "|");
            }
            set
            {
                char[] delimiters = { '|' };
                WriteLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(value, delimiters);
            }
        }

        // Text format.
        public string TextFormat
        {
            get
            {
                return _TextFormat;
            }
            set
            {
                if (value != _TextFormat)
                {
                    _TextFormat = value;
                    ModifiedFlag = true;
                }
            }
        }

        public static string TextFormatHelp = "This is a coded string that will be used to format the text output."
            + " It uses substitutions of the form \"%{c}\" where \"c\" is a control string indicating what is to be substituted."
            + " The control strings supported are:  \"t\" (set target language item), \"ta1\"-\"ta3\" (set target alternate language item),"
            + " \"h\" (set host language item), \"e\" output the entry index, \"o\" output the entry ordinal (i.e. the \"nth\" card),"
            + " \"l\" output the entry label annotation if any, \"l:\" output the entry label annotation if any with a colon and space."
            + " Or, you can use some simple HTML elements, i.e. \"%{b}bold%{/b}\" will display the text between in bold."
            + " The supported elements are: b, br, em, i, p, strong, and u."
            + " Any other text outside of the \"%{}\" substitutions is just passed through."
            + " For example: Say your target language is Spanish, host language is English, and you have some alternate language."
            + " You could use a pattern like: \"%{o}: %{t}\" on card side 1 to output something like: \"23: Spanish\" for the 23rd card shown,"
            + " and then use: \"%{h} (%{ta1})\" on card side 2 to output something like: \"English (alternate)\"."
            + " If you leave the field blank, a default format will be used based on the languages selected, i.e. something like:"
            + " \"first (second) [third]\" for a first, second, and third language, if selected.";

        // Font family.
        public string FontFamily
        {
            get
            {
                return _FontFamily;
            }
            set
            {
                if (value != _FontFamily)
                {
                    _FontFamily = value;
                    ModifiedFlag = true;
                }
            }
        }

        // Flash font size (points).
        public string FlashFontSize
        {
            get
            {
                return _FlashFontSize;
            }
            set
            {
                if (value != _FlashFontSize)
                {
                    _FlashFontSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        // List font size (points).
        public string ListFontSize
        {
            get
            {
                return _ListFontSize;
            }
            set
            {
                if (value != _ListFontSize)
                {
                    _ListFontSize = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void SetDefaultProfileInformation(ToolProfile profile)
        {
            if (profile == null)
                return;

            if (String.IsNullOrEmpty(FontFamily))
                FontFamily = profile.FontFamily;

            if (String.IsNullOrEmpty(FlashFontSize))
                FlashFontSize = profile.FlashFontSize;

            if (String.IsNullOrEmpty(ListFontSize))
                ListFontSize = profile.ListFontSize;
        }

        // List font size (points).
        public int MaximumLineLength
        {
            get
            {
                return _MaximumLineLength;
            }
            set
            {
                if (value != _MaximumLineLength)
                {
                    _MaximumLineLength = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XElement("HasTextOutput", _HasTextOutput.ToString()));
            element.Add(new XElement("HasPictureOutput", _HasPictureOutput.ToString()));
            element.Add(new XElement("HasAudioOutput", _HasAudioOutput.ToString()));
            element.Add(new XElement("HasVideoOutput", _HasVideoOutput.ToString()));
            element.Add(new XElement("HasDescrambleInput", _HasDescrambleInput.ToString()));
            element.Add(new XElement("HasChoiceInput", _HasChoiceInput.ToString()));
            element.Add(new XElement("HasBlanksInput", _HasBlanksInput.ToString()));
            element.Add(new XElement("HasTextInput", _HasTextInput.ToString()));
            element.Add(new XElement("HasAudioInput", _HasAudioInput.ToString()));
            element.Add(new XElement("HasVoiceRecognition", _HasVoiceRecognition.ToString()));
            string textLanguagesKey = TextLanguagesKey;
            if (!String.IsNullOrEmpty(textLanguagesKey))
                element.Add(new XElement("TextLanguagesKey", textLanguagesKey));
            string mediaLanguagesKey = MediaLanguagesKey;
            if (!String.IsNullOrEmpty(mediaLanguagesKey))
                element.Add(new XElement("MediaLanguagesKey", mediaLanguagesKey));
            string writeLanguagesKey = WriteLanguagesKey;
            if (!String.IsNullOrEmpty(writeLanguagesKey))
                element.Add(new XElement("WriteLanguagesKey", writeLanguagesKey));
            if (_TextFormat != null)
                element.Add(new XElement("TextFormat", _TextFormat));
            if (_FontFamily != null)
                element.Add(new XElement("FontFamily", _FontFamily));
            if (_FlashFontSize != null)
                element.Add(new XElement("FlashFontSize", _FlashFontSize));
            if (_ListFontSize != null)
                element.Add(new XElement("ListFontSize", _ListFontSize));
            if (_MaximumLineLength > 0)
                element.Add(new XElement("MaximumLineLength", _MaximumLineLength.ToString()));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            string elementValue = childElement.Value.Trim();
            bool returnValue = true;

            switch (childElement.Name.LocalName)
            {
                case "HasTextOutput":
                    _HasTextOutput = (elementValue == "True" ? true : false);
                    break;
                case "HasPictureOutput":
                    _HasPictureOutput = (elementValue == "True" ? true : false);
                    break;
                case "HasAudioOutput":
                    _HasAudioOutput = (elementValue == "True" ? true : false);
                    break;
                case "HasVideoOutput":
                    _HasVideoOutput = (elementValue == "True" ? true : false);
                    break;
                case "HasDescrambleInput":
                    _HasDescrambleInput = (elementValue == "True" ? true : false);
                    break;
                case "HasChoiceInput":
                    _HasChoiceInput = (elementValue == "True" ? true : false);
                    break;
                case "HasBlanksInput":
                    _HasBlanksInput = (elementValue == "True" ? true : false);
                    break;
                case "HasTextInput":
                    _HasTextInput = (elementValue == "True" ? true : false);
                    break;
                case "HasAudioInput":
                    _HasAudioInput = (elementValue == "True" ? true : false);
                    break;
                case "HasVoiceRecognition":
                    _HasVoiceRecognition = (elementValue == "True" ? true : false);
                    break;
                case "TextLanguagesKey":
                    TextLanguagesKey = elementValue;
                    break;
                case "MediaLanguagesKey":
                    MediaLanguagesKey = elementValue;
                    break;
                case "WriteLanguagesKey":
                    WriteLanguagesKey = elementValue;
                    break;
                case "TextFormat":
                    _TextFormat = elementValue;
                    break;
                case "FlashFontSize":
                    _FlashFontSize = elementValue;
                    break;
                case "FontFamily":
                    _FontFamily = elementValue;
                    break;
                case "ListFontSize":
                    _ListFontSize = elementValue;
                    break;
                case "MaximumLineLength":
                    if (ObjectUtilities.IsNumberString(elementValue))
                        _MaximumLineLength = Convert.ToInt32(elementValue);
                    break;
                default:
                    returnValue = base.OnChildElement(childElement);
                    break;
            }

            return returnValue;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolSide otherObject = other as ToolSide;
            int diff;

            if (otherObject == null)
                return base.Compare(other);

            diff = ObjectUtilities.CompareKeys(this, other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasTextOutput, otherObject.HasTextOutput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasPictureOutput, otherObject.HasPictureOutput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasAudioOutput, otherObject.HasAudioOutput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasVideoOutput, otherObject.HasVideoOutput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasDescrambleInput, otherObject.HasDescrambleInput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasChoiceInput, otherObject.HasChoiceInput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasBlanksInput, otherObject.HasBlanksInput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasTextInput, otherObject.HasTextInput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasAudioInput, otherObject.HasAudioInput);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareBools(_HasVoiceRecognition, otherObject.HasVoiceRecognition);
            if (diff != 0)
                return diff;
            diff = LanguageID.CompareLanguageIDLists(_TextLanguageIDs, otherObject.TextLanguageIDs);
            if (diff != 0)
                return diff;
            diff = LanguageID.CompareLanguageIDLists(_MediaLanguageIDs, otherObject.MediaLanguageIDs);
            if (diff != 0)
                return diff;
            diff = LanguageID.CompareLanguageIDLists(_WriteLanguageIDs, otherObject.WriteLanguageIDs);
            if (diff != 0)
                return diff;
            diff = String.Compare(_TextFormat, otherObject.TextFormat);
            if (diff != 0)
                return diff;
            diff = String.Compare(_FontFamily, otherObject.FontFamily);
            if (diff != 0)
                return diff;
            diff = String.Compare(_FlashFontSize, otherObject.FlashFontSize);
            if (diff != 0)
                return diff;
            diff = String.Compare(_ListFontSize, otherObject.ListFontSize);
            if (diff != 0)
                return diff;
            return _MaximumLineLength - otherObject.MaximumLineLength;
        }

        public static int Compare(ToolSide other1, ToolSide other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return 0;
            if ((object)other1 == null)
                return -1;
            if ((object)other2 == null)
                return 1;
            return other1.Compare(other2);
        }
    }
}

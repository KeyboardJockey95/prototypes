using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class Inflection : BaseObjectKeyed
    {
        protected DictionaryEntry _Input;
        protected Designator _Designation;
        protected MultiLanguageString _MainInflected;
        protected MultiLanguageString _Output;
        protected MultiLanguageString _PronounOutput;
        protected List<MultiLanguageString> _ContractedOutput;
        protected MultiLanguageString _RegularOutput;
        protected MultiLanguageString _RegularPronounOutput;
        protected MultiLanguageString _DictionaryForm;
        protected MultiLanguageString _PrePronoun;
        protected MultiLanguageString _Pronoun;
        protected MultiLanguageString _PreWords;
        protected MultiLanguageString _Prefix;
        protected MultiLanguageString _Root;
        protected MultiLanguageString _Suffix;
        protected MultiLanguageString _PostWords;
        protected MultiLanguageString _PostPronoun;
        protected LexicalCategory _Category;
        protected string _CategoryString;
        protected bool _IsRegular;
        protected string _Error;

        public Inflection(
                string inflected,
                DictionaryEntry input,
                Designator designation,
                MultiLanguageString mainInflected,
                MultiLanguageString output,
                MultiLanguageString pronounOutput,
                List<MultiLanguageString> contractedOutput,
                MultiLanguageString regularOutput,
                MultiLanguageString regularPronounOutput,
                MultiLanguageString dictionaryForm,
                MultiLanguageString prePronoun,
                MultiLanguageString pronoun,
                MultiLanguageString preWords,
                MultiLanguageString prefix,
                MultiLanguageString root,
                MultiLanguageString suffix,
                MultiLanguageString postWords,
                MultiLanguageString postPronoun,
                LexicalCategory category,
                string categoryString,
                bool isRegular,
                string error) :
            base(inflected)
        {
            _Input = input;
            _Designation = designation;
            _MainInflected = mainInflected;
            _Output = output;
            _PronounOutput = pronounOutput;
            _ContractedOutput = contractedOutput;
            _RegularOutput = regularOutput;
            _RegularPronounOutput = regularPronounOutput;
            _DictionaryForm = dictionaryForm;
            _PrePronoun = prePronoun;
            _Pronoun = pronoun;
            _PreWords = preWords;
            _Prefix = prefix;
            _Root = root;
            _Suffix = suffix;
            _PostWords = postWords;
            _PostPronoun = postPronoun;
            _Category = category;
            _CategoryString = categoryString;
            _IsRegular = isRegular;
            _Error = error;
        }

        public Inflection(
                string inflected,
                DictionaryEntry input,
                Designator designation,
                MultiLanguageString mainInflected,
                MultiLanguageString output,
                MultiLanguageString dictionaryForm,
                MultiLanguageString prefix,
                MultiLanguageString root,
                MultiLanguageString suffix,
                LexicalCategory category,
                string categoryString,
                bool isRegular,
                string error) :
            base(inflected)
        {
            List<LanguageID> languageIDs = dictionaryForm.LanguageIDs;
            _Input = input;
            _Designation = designation;
            _MainInflected = mainInflected;
            _Output = output;
            _PronounOutput = new MultiLanguageString(null, languageIDs);
            _ContractedOutput = null;
            _RegularOutput = new MultiLanguageString(null, languageIDs);
            _RegularPronounOutput = new MultiLanguageString(null, languageIDs);
            _DictionaryForm = dictionaryForm;
            _PrePronoun = new MultiLanguageString(null, languageIDs);
            _Pronoun = new MultiLanguageString(null, languageIDs);
            _PreWords = new MultiLanguageString(null, languageIDs);
            _Prefix = prefix;
            _Root = root;
            _Suffix = suffix;
            _PostWords = new MultiLanguageString(null, languageIDs);
            _PostPronoun = new MultiLanguageString(null, languageIDs);
            _Category = category;
            _CategoryString = categoryString;
            _IsRegular = isRegular;
            _Error = error;
        }

        public Inflection(
            DictionaryEntry input,
            Designator designation,
            List<LanguageID> languageIDs)
        {
            _Input = input;
            _Designation = designation;
            _MainInflected = new MultiLanguageString(null, languageIDs);
            _Output = new MultiLanguageString(null, languageIDs);
            _PronounOutput = new MultiLanguageString(null, languageIDs);
            _ContractedOutput = null;
            _RegularOutput = new MultiLanguageString(null, languageIDs);
            _RegularPronounOutput = new MultiLanguageString(null, languageIDs);
            _DictionaryForm = new MultiLanguageString(null, languageIDs);
            _PrePronoun = new MultiLanguageString(null, languageIDs);
            _Pronoun = new MultiLanguageString(null, languageIDs);
            _PreWords = new MultiLanguageString(null, languageIDs);
            _Prefix = new MultiLanguageString(null, languageIDs);
            _Root = new MultiLanguageString(null, languageIDs);
            _Suffix = new MultiLanguageString(null, languageIDs);
            _PostWords = new MultiLanguageString(null, languageIDs);
            _PostPronoun = new MultiLanguageString(null, languageIDs);
            _Category = LexicalCategory.Unknown;
            _CategoryString = String.Empty;
            _IsRegular = true;
            _Error = String.Empty;
        }

        public Inflection(Inflection other1, Inflection other2)
        {
            ClearInflection();

            if (other1 != null)
            {
                CopyInflection(other1);

                if (other2 != null)
                    Merge(other2);
            }
            else if (other2 != null)
                CopyInflection(other2);

            Modified = false;
        }

        public Inflection(List<Inflection> inflections)
        {
            ClearInflection();

            int count = inflections.Count();
            int index;

            for (index = 0; index < count; index++)
            {
                Inflection inflection = inflections[index];

                if (index == 0)
                    CopyInflection(inflection);
                else
                    Merge(inflection);
            }
        }

        public Inflection(Inflection other) :
            base(other)
        {
            CopyInflection(other);
        }

        public Inflection()
        {
            ClearInflection();
        }

        public void ClearInflection()
        {
            _Input = null;
            _Designation = null;
            _MainInflected = null;
            _Output = null;
            _PronounOutput = null;
            _ContractedOutput = null;
            _RegularOutput = null;
            _RegularPronounOutput = null;
            _DictionaryForm = null;
            _PrePronoun = null;
            _Pronoun = null;
            _PreWords = null;
            _Prefix = null;
            _Root = null;
            _Suffix = null;
            _PostWords = null;
            _PostPronoun = null;
            _Category = LexicalCategory.Unknown;
            _CategoryString = String.Empty;
            _IsRegular = true;
            _Error = String.Empty;
        }

        public void CopyInflection(Inflection other)
        {
            _Input = other.Input;

            if (other._Designation != null)
                _Designation = new Designator(other.Designation);
            else
                _Designation = null;

            if (other._MainInflected != null)
                _MainInflected = new Object.MultiLanguageString(other.MainInflected);
            else
                _MainInflected = null;

            if (other._Output != null)
                _Output = new Object.MultiLanguageString(other.Output);
            else
                _Output = null;

            if (other._PronounOutput != null)
                _PronounOutput = new Object.MultiLanguageString(other.PronounOutput);
            else
                _PronounOutput = null;

            if (other._ContractedOutput != null)
            {
                _ContractedOutput = new List<MultiLanguageString>();
                foreach (MultiLanguageString mls in other.ContractedOutput)
                    _ContractedOutput.Add(new MultiLanguageString(mls));
            }
            else
                _ContractedOutput = null;

            if (other._RegularOutput != null)
                _RegularOutput = new Object.MultiLanguageString(other.RegularOutput);
            else
                _RegularOutput = null;

            if (other._RegularPronounOutput != null)
                _RegularPronounOutput = new Object.MultiLanguageString(other.RegularPronounOutput);
            else
                _RegularPronounOutput = null;

            if (other._DictionaryForm != null)
                _DictionaryForm = new Object.MultiLanguageString(other.DictionaryForm);
            else
                _DictionaryForm = null;

            if (other._PrePronoun != null)
                _PrePronoun = new Object.MultiLanguageString(other.PrePronoun);
            else
                _PrePronoun = null;

            if (other._Pronoun != null)
                _Pronoun = new Object.MultiLanguageString(other.Pronoun);
            else
                _Pronoun = null;

            if (other._PreWords != null)
                _PreWords = new Object.MultiLanguageString(other.PreWords);
            else
                _PreWords = null;

            if (other._Prefix != null)
                _Prefix = new Object.MultiLanguageString(other.Prefix);
            else
                _Prefix = null;

            if (other._Root != null)
                _Root = new Object.MultiLanguageString(other.Root);
            else
                _Root = null;

            if (other._Suffix != null)
                _Suffix = new Object.MultiLanguageString(other.Suffix);
            else
                _Suffix = null;

            if (other._PostWords != null)
                _PostWords = new Object.MultiLanguageString(other.PostWords);
            else
                _PostWords = null;

            if (other._PostPronoun != null)
                _PostPronoun = new Object.MultiLanguageString(other.PostPronoun);
            else
                _PostPronoun = null;

            _Category = other.Category;
            _CategoryString = other.CategoryString;
            _IsRegular = other.IsRegular;
            _Error = other.Error;
        }

        public override string ToString()
        {
            string returnValue = String.Empty;

            if ((_PronounOutput != null) && (_PronounOutput.Count() != 0))
            {
                foreach (LanguageString ls in _PronounOutput.LanguageStrings)
                {
                    if (!String.IsNullOrEmpty(returnValue))
                        returnValue += "/";

                    returnValue += "\"" + ls.Text + "\"";
                }

                returnValue += " " + Label;
            }
            else
                returnValue = base.ToString();

            return returnValue;
        }

        public string Inflected
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public DictionaryEntry Input
        {
            get
            {
                return _Input;
            }
            set
            {
                if (value != _Input)
                {
                    _Input = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Designator Designation
        {
            get
            {
                return _Designation;
            }
            set
            {
                if (value != _Designation)
                {
                    _Designation = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string Label
        {
            get
            {
                if (_Designation != null)
                    return _Designation.Label;
                return String.Empty;
            }
        }

        public List<Classifier> Classifications
        {
            get
            {
                if (_Designation != null)
                    return _Designation.Classifications;
                return null;
            }
        }

        public MultiLanguageString MainInflected
        {
            get
            {
                return _MainInflected;
            }
            set
            {
                if (value != _MainInflected)
                {
                    _MainInflected = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasMainInflected(List<LanguageID> languageIDs)
        {
            return HasText(_MainInflected, languageIDs);
        }

        public string GetMainInflected(LanguageID languageID)
        {
            if (_MainInflected != null)
                return _MainInflected.Text(languageID);
            return String.Empty;
        }

        public void SetMainInflected(LanguageID languageID, string text)
        {
            if (_MainInflected != null)
                _MainInflected.SetText(languageID, text);
        }

        public void PrependToMainInflected(LanguageID languageID, string text)
        {
            if (_MainInflected != null)
                _MainInflected.SetText(languageID, text + _MainInflected.Text(languageID));
        }

        public void AppendToMainInflected(LanguageID languageID, string text)
        {
            if (_MainInflected != null)
                _MainInflected.SetText(languageID, _MainInflected.Text(languageID) + text);
        }

        public MultiLanguageString Output
        {
            get
            {
                return _Output;
            }
            set
            {
                if (value != _Output)
                {
                    _Output = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasOutput(List<LanguageID> languageIDs)
        {
            return HasText(_Output, languageIDs);
        }

        public string GetOutput(LanguageID languageID)
        {
            if (_Output != null)
                return _Output.Text(languageID);
            return String.Empty;
        }

        public void SetOutput(LanguageID languageID, string text)
        {
            if (_Output != null)
                _Output.SetText(languageID, text);
        }

        public void PrependToOutput(LanguageID languageID, string text)
        {
            if (_Output != null)
                _Output.SetText(languageID, text + _Output.Text(languageID));
        }

        public void AppendToOutput(LanguageID languageID, string text)
        {
            if (_Output != null)
                _Output.SetText(languageID, _Output.Text(languageID) + text);
        }

        public MultiLanguageString PronounOutput
        {
            get
            {
                return _PronounOutput;
            }
            set
            {
                if (value != _PronounOutput)
                {
                    _PronounOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasPronounOutput(List<LanguageID> languageIDs)
        {
            return HasText(_PronounOutput, languageIDs);
        }

        public bool HasPronounOutput(LanguageID languageID)
        {
            return HasText(_PronounOutput, languageID);
        }

        public string GetPronounOutput(LanguageID languageID)
        {
            if (_PronounOutput != null)
                return _PronounOutput.Text(languageID);
            return String.Empty;
        }

        public void SetPronounOutput(LanguageID languageID, string text)
        {
            if (_PronounOutput != null)
                _PronounOutput.SetText(languageID, text);
        }

        public void PrependToPronounOutput(LanguageID languageID, string text)
        {
            if (_PronounOutput != null)
                _PronounOutput.SetText(languageID, text + _PronounOutput.Text(languageID));
        }

        public void AppendToPronounOutput(LanguageID languageID, string text)
        {
            if (_PronounOutput != null)
                _PronounOutput.SetText(languageID, _PronounOutput.Text(languageID) + text);
        }

        public List<MultiLanguageString> ContractedOutput
        {
            get
            {
                return _ContractedOutput;
            }
            set
            {
                if (value != _ContractedOutput)
                {
                    _ContractedOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasAnyContractedOutput()
        {
            if ((_ContractedOutput == null) || (_ContractedOutput.Count() == 0))
                return false;

            return true;
        }

        public bool HasContractedOutput(List<LanguageID> languageIDs)
        {
            if (_ContractedOutput == null)
                return false;

            foreach (MultiLanguageString mls in _ContractedOutput)
            {
                if (mls.HasText(languageIDs))
                    return true;
            }

            return false;
        }

        public MultiLanguageString GetContractedOutputKeyed(string key)
        {
            if (_ContractedOutput != null)
                return _ContractedOutput.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public MultiLanguageString GetContractedOutputIndexed(int index)
        {
            if ((_ContractedOutput != null) && (index >= 0) && (index < _ContractedOutput.Count()))
                return _ContractedOutput[index];

            return null;
        }

        public string GetContractedOutputKeyed(string key, LanguageID languageID)
        {
            MultiLanguageString mls = GetContractedOutputKeyed(key);

            if (mls != null)
                return mls.Text(languageID);

            return String.Empty;
        }

        public string GetContractedOutputIndexed(int index, LanguageID languageID)
        {
            MultiLanguageString mls = GetContractedOutputIndexed(index);

            if (mls != null)
                return mls.Text(languageID);

            return String.Empty;
        }

        public void SetContractedOutput(string key, LanguageID languageID, string text)
        {
            MultiLanguageString mls = GetContractedOutputKeyed(key);

            if (mls != null)
                mls.SetText(languageID, text);
            else
            {
                mls = new MultiLanguageString(key, languageID, text);

                if (_ContractedOutput == null)
                    _ContractedOutput = new List<MultiLanguageString>();

                _ContractedOutput.Add(mls);
            }
        }

        public void PrependToContractedOutput(string key, LanguageID languageID, string text)
        {
            MultiLanguageString mls = GetContractedOutputKeyed(key);

            if (mls != null)
                mls.SetText(languageID, text + mls.Text(languageID));
            else
            {
                mls = new MultiLanguageString(key, languageID, text);

                if (_ContractedOutput == null)
                    _ContractedOutput = new List<MultiLanguageString>();

                _ContractedOutput.Add(mls);
            }
        }

        public void AppendToContractedOutput(string key, LanguageID languageID, string text)
        {
            MultiLanguageString mls = GetContractedOutputKeyed(key);

            if (mls != null)
                mls.SetText(languageID, mls.Text(languageID) + text);
            else
            {
                mls = new MultiLanguageString(key, languageID, text);

                if (_ContractedOutput == null)
                    _ContractedOutput = new List<MultiLanguageString>();

                _ContractedOutput.Add(mls);
            }
        }

        public MultiLanguageString FindContractedOutput(string contracted, LanguageID languageID)
        {
            if ((_ContractedOutput == null) || (_ContractedOutput.Count() == 0))
                return null;

            MultiLanguageString mls = _ContractedOutput.FirstOrDefault(x => x.Text(languageID) == contracted);

            return mls;
        }

        public MultiLanguageString RegularOutput
        {
            get
            {
                return _RegularOutput;
            }
            set
            {
                if (value != _RegularOutput)
                {
                    _RegularOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasRegularOutput(List<LanguageID> languageIDs)
        {
            return HasText(_RegularOutput, languageIDs);
        }

        public string GetRegularOutput(LanguageID languageID)
        {
            if (_RegularOutput != null)
                return _RegularOutput.Text(languageID);
            return String.Empty;
        }

        public void SetRegularOutput(LanguageID languageID, string text)
        {
            if (_RegularOutput != null)
                _RegularOutput.SetText(languageID, text);
        }

        public void PrependToRegularOutput(LanguageID languageID, string text)
        {
            if (_RegularOutput != null)
                _RegularOutput.SetText(languageID, text + _RegularOutput.Text(languageID));
        }

        public void AppendToRegularOutput(LanguageID languageID, string text)
        {
            if (_RegularOutput != null)
                _RegularOutput.SetText(languageID, _RegularOutput.Text(languageID) + text);
        }

        public MultiLanguageString RegularPronounOutput
        {
            get
            {
                return _RegularPronounOutput;
            }
            set
            {
                if (value != _RegularPronounOutput)
                {
                    _RegularPronounOutput = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasRegularPronounOutput(List<LanguageID> languageIDs)
        {
            return HasText(_RegularPronounOutput, languageIDs);
        }

        public string GetRegularPronounOutput(LanguageID languageID)
        {
            if (_RegularPronounOutput != null)
                return _RegularPronounOutput.Text(languageID);
            return String.Empty;
        }

        public void SetRegularPronounOutput(LanguageID languageID, string text)
        {
            if (_RegularPronounOutput != null)
                _RegularPronounOutput.SetText(languageID, text);
        }

        public void PrependToRegularPronounOutput(LanguageID languageID, string text)
        {
            if (_RegularPronounOutput != null)
                _RegularPronounOutput.SetText(languageID, text + _RegularPronounOutput.Text(languageID));
        }

        public void AppendToRegularPronounOutput(LanguageID languageID, string text)
        {
            if (_RegularPronounOutput != null)
                _RegularPronounOutput.SetText(languageID, _RegularPronounOutput.Text(languageID) + text);
        }

        public List<string> GetMultipleUniqueOutputsFullNoMain(LanguageID languageID)
        {
            string pronounOutput = GetPronounOutput(languageID);
            List<string> strs = new List<string>() { pronounOutput };

            if (HasAnyContractedOutput())
            {
                foreach (MultiLanguageString mls in ContractedOutput)
                {
                    if (!mls.KeyString.StartsWith("Pronoun"))
                        continue;

                    string contracted = mls.Text(languageID);

                    if (!strs.Contains(contracted))
                        strs.Add(contracted);
                }
            }

            string output = GetOutput(languageID);

            if (!strs.Contains(output))
                strs.Add(output);

            if (HasAnyContractedOutput())
            {
                foreach (MultiLanguageString mls in ContractedOutput)
                {
                    if (!mls.KeyString.StartsWith("Output"))
                        continue;

                    string contracted = mls.Text(languageID);

                    if (!strs.Contains(contracted))
                        strs.Add(contracted);
                }
            }

            return strs;
        }

        public List<string> GetMultipleUniqueOutputsAll(LanguageID languageID)
        {
            List<string> strs = GetMultipleUniqueOutputsFullNoMain(languageID);

            string mainPlusPrePost = GetMainWordPlusPrePostWords(languageID);

            if (!strs.Contains(mainPlusPrePost))
                strs.Add(mainPlusPrePost);

            return strs;
        }

        public List<string> GetMultipleUniqueOutputs(
            LanguageID languageID,
            InflectionOutputMode inflectionOutputMode)
        {
            List<string> texts;

            switch (inflectionOutputMode)
            {
                case InflectionOutputMode.MainWordPlusPrePostWords:
                    texts = new List<string>(1) { GetMainWordPlusPrePostWords(languageID) };
                    break;
                case InflectionOutputMode.FullNoPronouns:
                    texts = new List<string>(1) { GetOutput(languageID) };
                    break;
                case InflectionOutputMode.FullWithPronouns:
                    texts = new List<string>(1) { GetPronounOutput(languageID) };
                    break;
                case InflectionOutputMode.FullNoMain:
                    texts = GetMultipleUniqueOutputsFullNoMain(languageID);
                    break;
                case InflectionOutputMode.All:
                    texts = GetMultipleUniqueOutputsAll(languageID);
                    break;
                default:
                    throw new Exception("GetMultipleUniqueOutputs: Unsupported inflection output mode: " + inflectionOutputMode.ToString());
            }

            return texts;
        }

        public MultiLanguageString DictionaryForm
        {
            get
            {
                return _DictionaryForm;
            }
            set
            {
                if (value != _DictionaryForm)
                {
                    _DictionaryForm = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetDictionaryForm(LanguageID languageID)
        {
            if (_DictionaryForm != null)
                return _DictionaryForm.Text(languageID);
            return String.Empty;
        }

        public MultiLanguageString PrePronoun
        {
            get
            {
                return _PrePronoun;
            }
            set
            {
                if (value != _PrePronoun)
                {
                    _PrePronoun = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPrePronoun(LanguageID languageID)
        {
            if (_PrePronoun != null)
                return _PrePronoun.Text(languageID);
            return String.Empty;
        }

        public bool HasPrePronoun(List<LanguageID> languageIDs)
        {
            return HasText(_PrePronoun, languageIDs);
        }

        public void SetPrePronoun(LanguageID languageID, string text)
        {
            if (_PrePronoun != null)
                _PrePronoun.SetText(languageID, text);
        }

        public void PrependToPrePronoun(LanguageID languageID, string text)
        {
            if (_PrePronoun != null)
                _PrePronoun.SetText(languageID, text + _PrePronoun.Text(languageID));
        }

        public void AppendToPrePronoun(LanguageID languageID, string text)
        {
            if (_PrePronoun != null)
                _PrePronoun.SetText(languageID, _PrePronoun.Text(languageID) + text);
        }

        public MultiLanguageString Pronoun
        {
            get
            {
                return _Pronoun;
            }
            set
            {
                if (value != _Pronoun)
                {
                    _Pronoun = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPronoun(LanguageID languageID)
        {
            if (_Pronoun != null)
                return _Pronoun.Text(languageID);
            return String.Empty;
        }

        public bool HasPronoun(List<LanguageID> languageIDs)
        {
            return HasText(_Pronoun, languageIDs);
        }

        public bool HasPronoun(LanguageID languageID)
        {
            return HasText(_Pronoun, languageID);
        }

        public void SetPronoun(LanguageID languageID, string text)
        {
            if (_Pronoun != null)
                _Pronoun.SetText(languageID, text);
        }

        public void PrependToPronoun(LanguageID languageID, string text)
        {
            if (_Pronoun != null)
                _Pronoun.SetText(languageID, text + _Pronoun.Text(languageID));
        }

        public void AppendToPronoun(LanguageID languageID, string text)
        {
            if (_Pronoun != null)
                _Pronoun.SetText(languageID, _Pronoun.Text(languageID) + text);
        }

        public MultiLanguageString PreWords
        {
            get
            {
                return _PreWords;
            }
            set
            {
                if (value != _PreWords)
                {
                    _PreWords = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPreWords(LanguageID languageID)
        {
            if (_PreWords != null)
                return _PreWords.Text(languageID);
            return String.Empty;
        }

        public bool HasPreWords(List<LanguageID> languageIDs)
        {
            return HasText(_PreWords, languageIDs);
        }

        public void SetPreWords(LanguageID languageID, string text)
        {
            if (_PreWords != null)
                _PreWords.SetText(languageID, text);
        }

        public void PrependToPreWords(LanguageID languageID, string text)
        {
            if (_PreWords != null)
                _PreWords.SetText(languageID, text + _PreWords.Text(languageID));
        }

        public void AppendToPreWords(LanguageID languageID, string text)
        {
            if (_PreWords != null)
                _PreWords.SetText(languageID, _PreWords.Text(languageID) + text);
        }

        public MultiLanguageString Prefix
        {
            get
            {
                return _Prefix;
            }
            set
            {
                if (value != _Prefix)
                {
                    _Prefix = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPrefix(LanguageID languageID)
        {
            if (_Prefix != null)
                return _Prefix.Text(languageID);
            return String.Empty;
        }

        public bool HasPrefix(List<LanguageID> languageIDs)
        {
            return HasText(_Prefix, languageIDs);
        }

        public void SetPrefix(LanguageID languageID, string text)
        {
            if (_Prefix != null)
                _Prefix.SetText(languageID, text);
        }

        public void PrependToPrefix(LanguageID languageID, string text)
        {
            if (_Prefix != null)
                _Prefix.SetText(languageID, text + _Prefix.Text(languageID));
        }

        public void AppendToPrefix(LanguageID languageID, string text)
        {
            if (_Prefix != null)
                _Prefix.SetText(languageID, _Prefix.Text(languageID) + text);
        }

        public MultiLanguageString Root
        {
            get
            {
                return _Root;
            }
            set
            {
                if (value != _Root)
                {
                    _Root = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetRoot(LanguageID languageID)
        {
            if (_Root != null)
                return _Root.Text(languageID);
            return String.Empty;
        }

        public bool HasRoot(List<LanguageID> languageIDs)
        {
            return HasText(_Root, languageIDs);
        }

        public void SetRoot(LanguageID languageID, string text)
        {
            if (_Root != null)
                _Root.SetText(languageID, text);
        }

        public void PrependToRoot(LanguageID languageID, string text)
        {
            if (_Root != null)
                _Root.SetText(languageID, text + _Root.Text(languageID));
        }

        public void AppendToRoot(LanguageID languageID, string text)
        {
            if (_Root != null)
                _Root.SetText(languageID, _Root.Text(languageID) + text);
        }

        public MultiLanguageString Suffix
        {
            get
            {
                return _Suffix;
            }
            set
            {
                if (value != _Suffix)
                {
                    _Suffix = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetSuffix(LanguageID languageID)
        {
            if (_Suffix != null)
                return _Suffix.Text(languageID);
            return String.Empty;
        }

        public bool HasSuffix(List<LanguageID> languageIDs)
        {
            return HasText(_Suffix, languageIDs);
        }

        public void SetSuffix(LanguageID languageID, string text)
        {
            if (_Suffix != null)
                _Suffix.SetText(languageID, text);
        }

        public void PrependToSuffix(LanguageID languageID, string text)
        {
            if (_Suffix != null)
                _Suffix.SetText(languageID, text + _Suffix.Text(languageID));
        }

        public void AppendToSuffix(LanguageID languageID, string text)
        {
            if (_Suffix != null)
                _Suffix.SetText(languageID, _Suffix.Text(languageID) + text);
        }

        public void TruncateSuffixEnd(LanguageID languageID, int length)
        {
            if (_Suffix != null)
            {
                string suffix = _Suffix.Text(languageID);
                _Suffix.SetText(languageID, suffix.Substring(0, suffix.Length - length));
            }
        }

        public string GetMainWord(LanguageID languageID)
        {
            string mainWord = GetPrefix(languageID) + GetRoot(languageID) + GetSuffix(languageID);
            return mainWord;
        }

        public MultiLanguageString MainWordPlusPrePostWords
        {
            get
            {
                if (_Output == null)
                    return null;

                List<LanguageID> languageIDs = _Output.LanguageIDs;

                MultiLanguageString mls = new MultiLanguageString();

                foreach (LanguageID languageID in languageIDs)
                {
                    string text = GetMainWordPlusPrePostWords(languageID);
                    mls.Add(new LanguageString(null, languageID, text));
                }

                return mls;
            }
        }

        public string GetMainWordPlusPrePostWords(LanguageID languageID)
        {
            string inflected = GetPrefix(languageID) + GetRoot(languageID) + GetSuffix(languageID);
            string preWords = GetPreWords(languageID);
            string postWords = GetPostWords(languageID);

            if (!String.IsNullOrEmpty(preWords))
            {
                string space = (LanguageLookup.IsUseSpacesToSeparateWords(languageID) ? " " : "");
                inflected = preWords + space + inflected;
            }

            if (!String.IsNullOrEmpty(postWords))
            {
                string space = (LanguageLookup.IsUseSpacesToSeparateWords(languageID) ? " " : "");
                inflected += space + postWords;
            }

            return inflected;
        }

        public MultiLanguageString PostWords
        {
            get
            {
                return _PostWords;
            }
            set
            {
                if (value != _PostWords)
                {
                    _PostWords = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPostWords(LanguageID languageID)
        {
            if (_PostWords != null)
                return _PostWords.Text(languageID);
            return String.Empty;
        }

        public bool HasPostWords(List<LanguageID> languageIDs)
        {
            return HasText(_PostWords, languageIDs);
        }

        public void SetPostWords(LanguageID languageID, string text)
        {
            if (_PostWords != null)
                _PostWords.SetText(languageID, text);
        }

        public void PrependToPostWords(LanguageID languageID, string text)
        {
            if (_PostWords != null)
                _PostWords.SetText(languageID, text + _PostWords.Text(languageID));
        }

        public void AppendToPostWords(LanguageID languageID, string text)
        {
            if (_PostWords != null)
                _PostWords.SetText(languageID, _PostWords.Text(languageID) + text);
        }

        public void AppendToPostWordsSpaced(LanguageID languageID, string text)
        {
            if (_PostWords != null)
            {
                string str = _PostWords.Text(languageID);

                if (!String.IsNullOrEmpty(str))
                    str += " ";

                _PostWords.SetText(languageID, str + text);
            }
        }

        public MultiLanguageString PostPronoun
        {
            get
            {
                return _PostPronoun;
            }
            set
            {
                if (value != _PostPronoun)
                {
                    _PostPronoun = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetPostPronoun(LanguageID languageID)
        {
            if (_PostPronoun != null)
                return _PostPronoun.Text(languageID);
            return String.Empty;
        }

        public bool HasPostPronoun(List<LanguageID> languageIDs)
        {
            return HasText(_PostPronoun, languageIDs);
        }

        public void SetPostPronoun(LanguageID languageID, string text)
        {
            if (_PostPronoun != null)
                _PostPronoun.SetText(languageID, text);
        }

        public void PrependToPostPronoun(LanguageID languageID, string text)
        {
            if (_PostPronoun != null)
                _PostPronoun.SetText(languageID, text + _PostPronoun.Text(languageID));
        }

        public void AppendToPostPronoun(LanguageID languageID, string text)
        {
            if (_PostPronoun != null)
                _PostPronoun.SetText(languageID, _PostPronoun.Text(languageID) + text);
        }

        public void ExtendLanguage(
            LanguageID languageID,
            string preWords,
            string prefix,
            string suffix,
            string postWords)
        {
            if (!String.IsNullOrEmpty(preWords))
                PrependToPreWords(languageID, AppendLanguageSpace(preWords, languageID));

            if (!String.IsNullOrEmpty(prefix))
                PrependToPrefix(languageID, prefix);

            if (!String.IsNullOrEmpty(suffix))
                AppendToSuffix(languageID, suffix);

            if (!String.IsNullOrEmpty(suffix))
                AppendToPostWords(languageID, PrependLanguageSpace(postWords, languageID));

            RegenerateLanguage(languageID);
        }

        public void RegenerateLanguage(LanguageID languageID)
        {
            string output =
                AppendLanguageSpace(GetPreWords(languageID), languageID)
                + GetPrefix(languageID)
                + GetRoot(languageID)
                + GetSuffix(languageID)
                + PrependLanguageSpace(GetPostWords(languageID), languageID);
            SetOutput(
                languageID,
                output);

            string pronounOutput =
                AppendLanguageSpace(GetPrePronoun(languageID), languageID)
                + AppendLanguageSpace(GetPronoun(languageID), languageID)
                + output;
            SetPronounOutput(
                languageID,
                pronounOutput);
        }

        public string PrependLanguageSpace(string str, LanguageID languageID)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (LanguageLookup.IsUseSpacesToSeparateWords(languageID))
                return " " + str;
            else
                return str;
        }

        public string AppendLanguageSpace(string str, LanguageID languageID)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (LanguageLookup.IsUseSpacesToSeparateWords(languageID))
                return str + " ";
            else
                return str;
        }

        protected bool HasText(
            MultiLanguageString mls,
            List<LanguageID> languageIDs)
        {
            if (mls == null)
                return false;

            if (!mls.HasText(languageIDs))
                return false;

            return true;
        }

        protected bool HasText(
            MultiLanguageString mls,
            LanguageID languageID)
        {
            if (mls == null)
                return false;

            if (!mls.HasText(languageID))
                return false;

            return true;
        }

        public LexicalCategory Category
        {
            get
            {
                return _Category;
            }
            set
            {
                _Category = value;
            }
        }

        public string CategoryString
        {
            get
            {
                return _CategoryString;
            }
            set
            {
                _CategoryString = value;
            }
        }

        public bool IsRegular
        {
            get
            {
                return _IsRegular;
            }
            set
            {
                _IsRegular = value;
            }
        }

        public bool IsIrregular()
        {
            bool isIrregular = !_IsRegular;

            if ((_Output != null) && (_RegularOutput != null))
                isIrregular = (_Output.Text(0) != _RegularOutput.Text(0));

            if (isIrregular != !_IsRegular)
                ApplicationData.Global.PutConsoleMessage("Regularness mismatch: " + _Output.Text(0) + ", " + _RegularOutput.Text(0));

            return isIrregular;
        }

        public string Error
        {
            get
            {
                return _Error;
            }
            set
            {
                if (value != _Error)
                {
                    _Error = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasError
        {
            get
            {
                return !String.IsNullOrEmpty(_Error);
            }
        }

        public void AppendError(string error)
        {
            if (!HasError)
                Error = error;
            else
                Error = Error + "\n" + error;
        }

        public void Merge(Inflection other)
        {
            _Designation = new Designator(Designation, other.Designation, Designator.CombineCode.Intersect);
            _MainInflected = MergeMultiLanguageStrings(_MainInflected, other.MainInflected, "|");
            _Output = MergeMultiLanguageStrings(_Output, other.Output, "|");
            _PronounOutput = MergeMultiLanguageStrings(_PronounOutput, other.PronounOutput, "|");
            _ContractedOutput = MergeMultiLanguageStringLists(_ContractedOutput, other.ContractedOutput, "|");
            _RegularOutput = MergeMultiLanguageStrings(_RegularOutput, other.RegularOutput, "|");
            _RegularPronounOutput = MergeMultiLanguageStrings(_RegularPronounOutput, other.RegularPronounOutput, "|");
            _DictionaryForm = MergeMultiLanguageStrings(_DictionaryForm, other.DictionaryForm, "/");
            _PrePronoun = MergeMultiLanguageStrings(_PrePronoun, other.PrePronoun, "/");
            _Pronoun = MergeMultiLanguageStrings(_Pronoun, other.Pronoun, "/");
            _PreWords = MergeMultiLanguageStrings(_PreWords, other.PreWords, "/");
            _Prefix = MergeMultiLanguageStrings(_Prefix, other.Prefix, "/");
            _Root = MergeMultiLanguageStrings(_Root, other.Root, "/");
            _Suffix = MergeMultiLanguageStrings(_Suffix, other.Suffix, "/");
            _PostWords = MergeMultiLanguageStrings(_PostWords, other.PostWords, "/");
            _PostPronoun = MergeMultiLanguageStrings(_PostPronoun, other.PostPronoun, "/");
            _IsRegular = _IsRegular && other.IsRegular;
        }

        public List<MultiLanguageString> MergeMultiLanguageStringLists(
            List<MultiLanguageString> mlsl1,
            List<MultiLanguageString> mlsl2,
            string separator)
        {
            List<MultiLanguageString> mlsl = null;

            if ((mlsl1 == null) && (mlsl2 == null))
                return null;
            else if (mlsl2 == null)
                return mlsl1;
            else if (mlsl1 == null)
            {
                mlsl = new List<MultiLanguageString>();

                foreach (MultiLanguageString mls2 in mlsl2)
                    mlsl.Add(new MultiLanguageString(mls2));

                return mlsl;
            }
            else
            {
                mlsl = new List<MultiLanguageString>();

                foreach (MultiLanguageString mls2 in mlsl2)
                {
                    string key = mls2.KeyString;
                    MultiLanguageString mls1 = mlsl1.FirstOrDefault(x => x.KeyString == key);
                    if (mls1 != null)
                        mlsl.Add(MergeMultiLanguageStrings(mls1, mls2, separator));
                    else
                        mlsl.Add(new MultiLanguageString(mls2));
                }

                int index = 0;

                foreach (MultiLanguageString mls1 in mlsl1)
                {
                    string key = mls1.KeyString;
                    MultiLanguageString mls = mlsl.FirstOrDefault(x => x.KeyString == key);

                    if (mls == null)
                        mlsl.Insert(index, mls1);

                    index++;
                }

                return mlsl;
            }
        }

        public MultiLanguageString MergeMultiLanguageStrings(
            MultiLanguageString mls1,
            MultiLanguageString mls2,
            string separator)
        {
            MultiLanguageString mls = null;

            if (mls1 != null)
            {
                mls = new MultiLanguageString(mls1);

                if (mls2.LanguageStrings != null)
                {
                    foreach (LanguageString ls2 in mls2.LanguageStrings)
                    {
                        LanguageString ls = mls.LanguageString(ls2.LanguageID);

                        if (ls != null)
                        {
                            if (ls.HasText())
                            {
                                string text = ls.Text;

                                if (ls2.HasText() && (ls2.Text != text))
                                {
                                    if (!text.StartsWith(ls2.Text + separator) &&
                                            !text.EndsWith(separator + ls2.Text) &&
                                            !text.Contains(separator + ls2.Text + separator))
                                        ls.Text = text + separator + ls2.Text;
                                }
                            }
                            else
                                ls.Text = ls2.Text;
                        }
                        else
                            mls.Add(new LanguageString(ls2));
                    }
                }
            }
            else if (mls2 != null)
                mls = new Object.MultiLanguageString(mls2);

            return mls;
        }

        public void ApplyCompoundFixup(MultiLanguageString compoundFixupPatternMLS)
        {
            ApplyCompoundFixupToMLS(Output, compoundFixupPatternMLS);
            ApplyCompoundFixupToMLS(PronounOutput, compoundFixupPatternMLS);
        }

        // Substitution token "%s".
        public static void ApplyCompoundFixupToMLS(
            MultiLanguageString mls,
            MultiLanguageString compoundFixupPatternMLS)
        {
            if (mls.LanguageStrings == null)
                return;

            foreach (LanguageString ls in mls.LanguageStrings)
            {
                string pattern = compoundFixupPatternMLS.Text(ls.LanguageID);

                if (!String.IsNullOrEmpty(pattern))
                    ls.Text = pattern.Replace("%s", ls.Text);
            }
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    {
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, Label);
                        if (_PronounOutput != null)
                            _PronounOutput.Display("PronounOutput", detail, indent + 1);
                        else if (_Output != null)
                            _Output.Display("Output", detail, indent + 1);
                    }
                    break;
                case DisplayDetail.Full:
                    {
                        if (!String.IsNullOrEmpty(KeyString))
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                        else
                            ObjectUtilities.DisplayLabel(this, label, indent);
                        if (_Input != null)
                            _Input.Display("Input", detail, indent + 1);
                        if (_Designation != null)
                            _Designation.Display("Designation", detail, indent + 1);
                        if (_MainInflected != null)
                            _MainInflected.Display("MainInflected", detail, indent + 1);
                        if (_Output != null)
                            _Output.Display("Output", detail, indent + 1);
                        if (_PronounOutput != null)
                            _PronounOutput.Display("PronounOutput", detail, indent + 1);
                        if (_ContractedOutput != null)
                        {
                            foreach (MultiLanguageString mls in _ContractedOutput)
                                mls.Display("ContractedOutput", detail, indent + 1);
                        }
                        if (_RegularOutput != null)
                            _RegularOutput.Display("RegularOutput", detail, indent + 1);
                        if (_RegularPronounOutput != null)
                            _RegularPronounOutput.Display("RegularPronounOutput", detail, indent + 1);
                        if (_DictionaryForm != null)
                            _DictionaryForm.Display("DictionaryForm", detail, indent + 1);
                        if (_PrePronoun != null)
                            _PrePronoun.Display("PrePronoun", detail, indent + 1);
                        if (_Pronoun != null)
                            _Pronoun.Display("Pronoun", detail, indent + 1);
                        if (_PreWords != null)
                            _PreWords.Display("PreWords", detail, indent + 1);
                        if (_Prefix != null)
                            _Prefix.Display("Prefix", detail, indent + 1);
                        if (_Root != null)
                            _Root.Display("Root", detail, indent + 1);
                        if (_Suffix != null)
                            _Suffix.Display("Suffix", detail, indent + 1);
                        if (_PostWords != null)
                            _PostWords.Display("PostWords", detail, indent + 1);
                        if (_PostPronoun != null)
                            _PostPronoun.Display("PostPronoun", detail, indent + 1);
                        DisplayField("Category", _Category.ToString(), indent + 1);
                        if (!String.IsNullOrEmpty(_CategoryString))
                            DisplayField("CategoryString", _CategoryString, indent + 1);
                        if (!String.IsNullOrEmpty(_Error))
                            DisplayField("Error", _Error, indent + 1);
                    }
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Xml:
                    base.Display(label, detail, indent);
                    break;
                default:
                    break;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_Input != null)
                element.Add(_Input.GetElement("Input"));
            if (_Designation != null)
                element.Add(_Designation.GetElement("Designation"));
            if (_MainInflected != null)
                element.Add(_MainInflected.GetElement("MainInflected"));
            if (_Output != null)
                element.Add(_Output.GetElement("Output"));
            if (_PronounOutput != null)
                element.Add(_PronounOutput.GetElement("PronounOutput"));
            if ((_ContractedOutput != null) && (_ContractedOutput.Count() != 0))
            {
                foreach (MultiLanguageString mls in _ContractedOutput)
                {
                    XElement contractedOutputElement = mls.GetElement("ContractedOutput");
                    element.Add(contractedOutputElement);
                }
            }
            if (_RegularOutput != null)
                element.Add(_RegularOutput.GetElement("RegularOutput"));
            if (_RegularPronounOutput != null)
                element.Add(_RegularPronounOutput.GetElement("RegularPronounOutput"));
            if (_DictionaryForm != null)
                element.Add(_DictionaryForm.GetElement("DictionaryForm"));
            if (_PrePronoun != null)
                element.Add(_PrePronoun.GetElement("PrePronoun"));
            if (_Pronoun != null)
                element.Add(_Pronoun.GetElement("Pronoun"));
            if (_PreWords != null)
                element.Add(_PreWords.GetElement("PreWords"));
            if (_Prefix != null)
                element.Add(_Prefix.GetElement("Prefix"));
            if (_Root != null)
                element.Add(_Root.GetElement("Root"));
            if (_Suffix != null)
                element.Add(_Suffix.GetElement("Suffix"));
            if (_PostWords != null)
                element.Add(_PostWords.GetElement("PostWords"));
            if (_PostPronoun != null)
                element.Add(_PostPronoun.GetElement("PostPronoun"));
            element.Add(new XElement("Category", _Category.ToString()));
            if (!String.IsNullOrEmpty(_CategoryString))
                element.Add(new XElement("CategoryString"));
            if (!String.IsNullOrEmpty(_Error))
                element.Add(new XElement("Error"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Input":
                    _Input = new DictionaryEntry(childElement);
                    break;
                case "Designation":
                    _Designation = new Designator(childElement);
                    break;
                case "MainInflected":
                    _MainInflected = new MultiLanguageString(childElement);
                    break;
                case "Output":
                    _Output = new MultiLanguageString(childElement);
                    break;
                case "PronounOutput":
                    _PronounOutput = new MultiLanguageString(childElement);
                    break;
                case "ContractedOutput":
                    {
                        if (_ContractedOutput == null)
                            _ContractedOutput = new List<MultiLanguageString>();
                        MultiLanguageString contractedOutput = new MultiLanguageString(childElement);
                        _ContractedOutput.Add(contractedOutput);
                    }
                    break;
                case "RegularOutput":
                    _RegularOutput = new MultiLanguageString(childElement);
                    break;
                case "RegularPronounOutput":
                    _RegularPronounOutput = new MultiLanguageString(childElement);
                    break;
                case "DictionaryForm":
                    _DictionaryForm = new MultiLanguageString(childElement);
                    break;
                case "PrePronoun":
                    _PrePronoun = new MultiLanguageString(childElement);
                    break;
                case "Pronoun":
                    _Pronoun = new MultiLanguageString(childElement);
                    break;
                case "PreWords":
                    _PreWords = new MultiLanguageString(childElement);
                    break;
                case "Prefix":
                    _Prefix = new MultiLanguageString(childElement);
                    break;
                case "Root":
                    _Root = new MultiLanguageString(childElement);
                    break;
                case "Suffix":
                    _Suffix = new MultiLanguageString(childElement);
                    break;
                case "PostWords":
                    _PostWords = new MultiLanguageString(childElement);
                    break;
                case "PostPronoun":
                    _PostPronoun = new MultiLanguageString(childElement);
                    break;
                case "Category":
                    _Category = Sense.GetLexicalCategoryFromString(childElement.Value.Trim());
                    break;
                case "CategoryString":
                    _CategoryString = childElement.Value.Trim();
                    break;
                case "Error":
                    _Error = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public int QuickCompare(Inflection other)
        {
            if (other == null)
                return 1;

            int diff = String.Compare(Label, other.Label);

            if (diff != 0)
                return diff;

            diff = PronounOutput.Compare(other.PronounOutput);

            return diff;
        }
    }
}

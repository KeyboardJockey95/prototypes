using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class MultiLanguageString : IBaseObjectKeyed
    {
        protected Type _KeyType;
        protected object _Key;
        protected List<LanguageString> _LanguageStrings;
        protected bool _Modified;
        protected DateTime _CreationTime;
        protected DateTime _ModifiedTime;

        public MultiLanguageString(object key)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            _LanguageStrings = new List<LanguageString>();
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, List<LanguageString> languageStrings)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            if (languageStrings == null)
                _LanguageStrings = new List<LanguageString>();
            else
                _LanguageStrings = languageStrings;
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, LanguageString languageString)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            if (languageString == null)
                _LanguageStrings = new List<LanguageString>();
            else
                _LanguageStrings = new List<LanguageString>() { languageString };
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, LanguageID languageID, string text)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            LanguageString languageString = new LanguageString(key, languageID, text);
            _LanguageStrings = new List<LanguageString>() { languageString };
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, LanguageID languageID, string text, List<LanguageID> languageIDs, string defaultText)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            int count = (languageIDs != null ? languageIDs.Count() : 0);
            _LanguageStrings = new List<LanguageString>(count);
            foreach (LanguageID lid in languageIDs)
            {
                string str;
                if (lid == languageID)
                    str = text;
                else
                    str = defaultText;
                LanguageString languageString = new LanguageString(key, lid, str);
                _LanguageStrings.Add(languageString);
            }
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, List<LanguageDescriptor> languageDescriptors)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            _LanguageStrings = new List<LanguageString>(languageDescriptors.Count());
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                    _LanguageStrings.Add(new LanguageString(key, languageDescriptor.LanguageID, ""));
            }
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, List<LanguageID> languageIDs)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            _LanguageStrings = new List<LanguageString>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
                _LanguageStrings.Add(new LanguageString(key, languageID, ""));
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, List<LanguageID> languageIDs, List<string> textList)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            _LanguageStrings = new List<LanguageString>(languageIDs.Count());
            int c = languageIDs.Count();
            for (int i = 0; i < c; i++)
                _LanguageStrings.Add(new LanguageString(key, languageIDs[i], textList[i]));
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(
            object key,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> targetLanguageIDs)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            int count = (hostLanguageIDs != null ? hostLanguageIDs.Count() : 0) + (targetLanguageIDs != null ? targetLanguageIDs.Count() : 0);
            _LanguageStrings = new List<LanguageString>(count);
            foreach (LanguageID languageID in hostLanguageIDs)
                _LanguageStrings.Add(new LanguageString(key, languageID, ""));
            foreach (LanguageID languageID in targetLanguageIDs)
            {
                if (!HasLanguageID(languageID))
                    _LanguageStrings.Add(new LanguageString(key, languageID, ""));
            }
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(MultiLanguageString other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            _Key = other.Key;
            if (_Key != null)
                _KeyType = _Key.GetType();
            else
                _KeyType = null;
            _LanguageStrings = new List<LanguageString>();
            Copy(other);
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public MultiLanguageString(object key, MultiLanguageString other)
        {
            Copy(other);
            Rekey(key);
        }

        public MultiLanguageString(XElement element)
        {
            OnElement(element);
        }

        public MultiLanguageString()
        {
            ClearMultiLanguageString();
        }

        public virtual void Clear()
        {
            ClearMultiLanguageString();
        }

        public void ClearMultiLanguageString()
        {
            _KeyType = null;
            _Key = null;
            _LanguageStrings = new List<LanguageString>();
            _Modified = false;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public virtual void Copy(MultiLanguageString other)
        {
            if (other != null)
                _Key = other.Key;
            else
                _Key = null;

            if (_Key != null)
                _KeyType = _Key.GetType();
            else
                _KeyType = null;

            CopyText(other);

            _Modified = false;

            if (other != null)
            {
                _CreationTime = other.CreationTime;
                _ModifiedTime = other.ModifiedTime;
            }
            else
            {
                _CreationTime = DateTime.MinValue;
                _ModifiedTime = DateTime.MinValue;
            }
        }

        public virtual void CopyDeep(MultiLanguageString other)
        {
            this.Copy(other);
        }

        public virtual IBaseObject Clone()
        {
            return new MultiLanguageString(this);
        }

        public MultiLanguageString CloneMultiLanguageString()
        {
            return new MultiLanguageString(this);
        }

        public Type KeyType
        {
            get
            {
                return _KeyType;
            }
            set
            {
                _KeyType = value;
            }
        }

        public bool IsIntegerKeyType
        {
            get
            {
                return ObjectUtilities.IsIntegerType(_KeyType);
            }
        }

        public virtual object Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (ObjectUtilities.CompareObjects(_Key, value) != 0)
                {
                    _Modified = true;
                    _Key = value;
                    if (_Key != null)
                        _KeyType = _Key.GetType();
                    else
                        _KeyType = null;
                }
            }
        }

        public void SetKeyNoModify(object key)
        {
            _Key = key;
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
        }

        public void ResetKeyNoModify()
        {
            _Key = BaseObjectKeyed.GetResetKeyValue(_KeyType);
        }

        public void SetKeys(object key)
        {
            Key = key;

            if (_LanguageStrings != null)
            {
                foreach (LanguageString languageString in _LanguageStrings)
                    languageString.Key = key;
            }
        }

        public void Rekey(object newKey)
        {
            SetKeyNoModify(newKey);

            if (_LanguageStrings != null)
            {
                foreach (LanguageString languageString in _LanguageStrings)
                    languageString.SetKeyNoModify(newKey);
            }
        }

        public string KeyString
        {
            get
            {
                if (_Key == null)
                    return String.Empty;
                else
                    return _Key.ToString();
            }
        }

        public int KeyInt
        {
            get
            {
                if ((_Key != null) && IsIntegerKeyType)
                    return (int)_Key;
                return 0;
            }
        }

        public List<LanguageString> LanguageStrings
        {
            get
            {
                return _LanguageStrings;
            }
            set
            {
                if (_LanguageStrings != value)
                    _Modified = true;

                _LanguageStrings = value;
            }
        }

        public List<LanguageString> CloneLanguageStrings()
        {
            return JTLanguageModelsPortable.Object.LanguageString.CloneLanguageStringList(_LanguageStrings);
        }

        public virtual string Name
        {
            get
            {
                return KeyString;
            }
            set
            {
            }
        }

        public virtual string TypeLabel
        {
            get
            {
                return "Multi-Language String";
            }
        }

        public virtual Guid Guid
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
            }
        }

        public virtual string GuidString
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public virtual bool EnsureGuid()
        {
            return true;
        }

        public virtual void NewGuid()
        {
        }

        public virtual string Owner
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public virtual string Source
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public virtual bool Modified
        {
            get
            {
                if (_Modified)
                    return true;

                foreach (LanguageString languageString in _LanguageStrings)
                {
                    if (languageString.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                _Modified = value;

                foreach (LanguageString languageString in _LanguageStrings)
                    languageString.Modified = false;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return _CreationTime;
            }
            set
            {
                if (_CreationTime != value)
                {
                    _CreationTime = value;
                    _Modified = true;
                }
            }
        }

        public DateTime ModifiedTime
        {
            get
            {
                return _ModifiedTime;
            }
            set
            {
                if (_ModifiedTime != value)
                {
                    _ModifiedTime = value;
                    _Modified = true;
                }
            }
        }

        public void Touch()
        {
            if (_CreationTime == DateTime.MinValue)
            {
                CreationTime = DateTime.UtcNow;
                ModifiedTime = _CreationTime;
            }
            else
                ModifiedTime = DateTime.UtcNow;
        }

        public void TouchAndClearModified()
        {
            Touch();
            Modified = false;
        }

        public List<LanguageString> Lookup(Matcher matcher)
        {
            IEnumerable<LanguageString> lookupQuery =
                from languageString in _LanguageStrings
                where (matcher.Match(languageString))
                select languageString;
            return lookupQuery.ToList();
        }

        public LanguageString LanguageString(LanguageID languageID)
        {
            if (_LanguageStrings != null) 
                return _LanguageStrings.FirstOrDefault(ls => ls.LanguageID == languageID);
            return (null);
        }

        public LanguageString LanguageString(string languageCode)
        {
            if (_LanguageStrings != null)
                return _LanguageStrings.FirstOrDefault(ls => ls.LanguageID.LanguageCultureExtensionCode == languageCode);
            return (null);
        }

        public LanguageString LanguageString(int index)
        {
            if ((_LanguageStrings != null) && (index >= 0) && (index < _LanguageStrings.Count()))
                return _LanguageStrings.ElementAt(index);
            return null;
        }

        public LanguageString LanguageStringFuzzy(LanguageID languageID)
        {
            if (_LanguageStrings != null)
            {
                LanguageString languageString = _LanguageStrings.FirstOrDefault(ls => ls.LanguageID == languageID);

                if (languageString == null)
                    languageString = _LanguageStrings.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode)
                            && (ls.LanguageID.CultureCode == null)
                            && (ls.LanguageID.ExtensionCode == languageID.ExtensionCode));

                if ((languageString == null) && (languageID.ExtensionCode != null))
                    languageString = _LanguageStrings.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode)
                            && (ls.LanguageID.ExtensionCode == languageID.ExtensionCode));

                if ((languageString == null) && (languageID.CultureCode == null) && (languageID.ExtensionCode == null))
                    languageString = _LanguageStrings.FirstOrDefault(ls => ls.LanguageID.LanguageCode == languageID.LanguageCode);

                if (languageString == null)
                    languageString = _LanguageStrings.FirstOrDefault(ls => ls.LanguageID.LanguageCode == null);

                return languageString;
            }

            return (null);
        }

        public LanguageString LanguageStringMedia(LanguageID languageID)
        {
            if (_LanguageStrings != null)
            {
                LanguageString languageString = _LanguageStrings.FirstOrDefault(ls => ls.LanguageID == languageID);

                if (languageString == null)
                    languageString = _LanguageStrings.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode));

                return languageString;
            }

            return (null);
        }

        public string Text(LanguageID languageID)
        {
            LanguageString ls = LanguageString(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public string Text(string languageCode)
        {
            LanguageString ls = LanguageString(languageCode);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public string Text(int index)
        {
            LanguageString ls = LanguageString(index);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public string TextMedia(LanguageID languageID)
        {
            LanguageString ls = LanguageStringMedia(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public void SetText(LanguageID languageID, string str)
        {
            LanguageString ls = LanguageString(languageID);
            if (ls == null)
                Add(new LanguageString(Key, languageID, str));
            else
                ls.Text = str;
        }

        public void SetText(int index, string str)
        {
            LanguageString ls = LanguageString(index);
            if (ls != null)
                ls.Text = str;
        }

        public string TextFuzzy(LanguageID languageID)
        {
            LanguageString ls = LanguageStringFuzzy(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public void SetTextFuzzy(LanguageID languageID, string str)
        {
            LanguageString ls = LanguageStringFuzzy(languageID);
            if (ls == null)
                Add(new LanguageString(Key, languageID, str));
            else
                ls.Text = str;
        }

        public bool HasText()
        {
            if (_LanguageStrings == null)
                return false;

            foreach (LanguageString ls in _LanguageStrings)
            {
                if (ls.HasText())
                    return true;
            }

            return false;
        }

        public bool HasText(LanguageID languageID)
        {
            LanguageString ls = LanguageString(languageID);
            if (ls != null)
                return ls.HasText();
            return false;
        }

        public bool HasText(string languageCode)
        {
            LanguageString ls = LanguageString(languageCode);
            if (ls != null)
                return ls.HasText();
            return false;
        }

        public bool HasText(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return false;

            foreach (LanguageID languageID in languageIDs)
            {
                if (HasText(languageID))
                    return true;
            }

            return false;
        }

        public bool HasText(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return false;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Show || !languageDescriptor.Used)
                    continue;

                if (HasText(languageDescriptor.LanguageID))
                    return true;
            }

            return false;
        }

        public bool AnyTextMatchesExact(string text)
        {
            if (LanguageStrings == null)
                return false;

            foreach (LanguageString ls in LanguageStrings)
            {
                if (ls.Text == text)
                    return true;
            }

            return false;
        }

        public List<string> StringList()
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageStrings != null)
            {
                foreach (LanguageString ls in _LanguageStrings)
                    stringList.Add(ls.Text == null ? String.Empty : ls.Text);
            }

            return stringList;
        }

        public List<string> StringList(List<LanguageDescriptor> languageDescriptors)
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageStrings != null)
            {
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                {
                    if (!languageDescriptor.Used || !languageDescriptor.Show)
                        continue;

                    LanguageString languageString = LanguageString(languageDescriptor.LanguageID);

                    if (languageString != null)
                        stringList.Add(languageString.Text == null ? String.Empty : languageString.Text);
                }
            }

            return stringList;
        }

        public List<string> StringList(List<LanguageID> languageIDs)
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageStrings != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageString languageString = LanguageString(languageID);

                    if (languageString != null)
                        stringList.Add(languageString.Text == null ? String.Empty : languageString.Text);
                }
            }

            return stringList;
        }

        public string[] StringArray()
        {
            return StringList().ToArray();
        }

        public string[] StringArray(List<LanguageDescriptor> languageDescriptors)
        {
            return StringList(languageDescriptors).ToArray();
        }

        public string[] StringArray(List<LanguageID> languageIDs)
        {
            return StringList(languageIDs).ToArray();
        }

        public string GetStringListString(List<LanguageID> languageIDs)
        {
            string stringList = String.Empty;
            int count = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                string text = Text(languageID);

                if (count != 0)
                    stringList += ",";

                stringList += text;
                count++;
            }

            return stringList;
        }

        public LanguageID LanguageID(int index)
        {
            LanguageString ls = LanguageString(index);
            if (ls != null)
                return ls.LanguageID;
            return null;
        }

        public string Language(int index)
        {
            LanguageString ls = LanguageString(index);
            if (ls != null)
                return ls.LanguageID.Language;
            return null;
        }

        public bool HasLanguageID(LanguageID languageID)
        {
            LanguageString ls = LanguageString(languageID);
            if (ls != null)
                return true;
            return false;
        }

        public bool HasLanguageID(Matcher filter)
        {
            foreach (LanguageString languageString in LanguageStrings)
            {
                if (filter.Match(languageString.LanguageID))
                    return true;
            }
            return false;
        }

        public bool Add(LanguageString languageString)
        {
            if (LanguageString(languageString.LanguageID) == null)
            {
                if (_LanguageStrings == null)
                    _LanguageStrings = new List<LanguageString>(1) { languageString };
                else
                    _LanguageStrings.Add(languageString);
                _Modified = true;
                return true;
            }
            return false;
        }

        public bool Delete(LanguageString languageString)
        {
            if (_LanguageStrings != null)
            {
                if (_LanguageStrings.Remove(languageString))
                {
                    _Modified = true;
                    return true;
                }
            }
            return false;
        }

        public bool Delete(int index)
        {
            if ((_LanguageStrings != null) && (index >= 0) && (index < _LanguageStrings.Count()))
            {
                _LanguageStrings.RemoveAt(index);
                _Modified = true;
                return true;
            }
            return false;
        }

        public void DeleteAll()
        {
            if (_LanguageStrings != null)
                _Modified = true;
            _LanguageStrings = null;
        }

        public bool Reorder(LanguageIDMatcher languageIDMatcher)
        {
            int index = 0;
            bool wasReordered = false;
            int count = languageIDMatcher.LanguageIDs.Count();
            LanguageID languageID;

            foreach (LanguageString ls in _LanguageStrings)
            {
                if (index == count)
                {
                    wasReordered = true;
                    break;
                }


                languageID = languageIDMatcher.LanguageIDs[index++];

                if (!LanguageIDMatcher.MatchLanguageIDs(MatchCode.Exact, languageID, ls.LanguageID))
                {
                    wasReordered = true;
                    break;
                }
            }

            if (wasReordered)
            {
                List<LanguageString> list = new List<LanguageString>(_LanguageStrings.Count());

                foreach (LanguageID lid in languageIDMatcher.LanguageIDs)
                {
                    LanguageString ls = _LanguageStrings.FirstOrDefault(s => s.LanguageID == lid);

                    if (ls != null)
                        list.Add(ls);
                    else
                    {
                        ls = _LanguageStrings.FirstOrDefault(s => LanguageIDMatcher.MatchLanguageIDs(MatchCode.Exact, lid, s.LanguageID));

                        if (ls != null)
                            list.Add(ls);
                    }
                }

                _LanguageStrings = list;
                _Modified = true;
            }

            return wasReordered;
        }

        public bool IsOverlapping(MultiLanguageString other)
        {
            if ((Count() == 0) || other.Count() == 0)
                return false;

            foreach (LanguageString newLanguageString in other.LanguageStrings)
            {
                LanguageString oldLanguageString = LanguageString(newLanguageString.LanguageID);

                if (oldLanguageString == null)
                    return false;

                if (!oldLanguageString.IsOverlapping(newLanguageString))
                    return false;
            }

            return true;
        }

        public bool IsOverlappingAnchored(MultiLanguageString other,
            Dictionary<string, bool> anchorLanguageFlags)
        {
            if ((Count() == 0) || other.Count() == 0)
                return false;

            foreach (LanguageString newLanguageString in other.LanguageStrings)
            {
                LanguageID languageID = newLanguageString.LanguageID;

                if (anchorLanguageFlags != null)
                {
                    bool useIt = false;

                    if (anchorLanguageFlags.TryGetValue(languageID.LanguageCultureExtensionCode, out useIt))
                    {
                        if (!useIt)
                            continue;
                    }
                }

                LanguageString oldLanguageString = LanguageString(languageID);

                if (oldLanguageString == null)
                    return false;

                if (!oldLanguageString.IsOverlapping(newLanguageString))
                    return false;
            }

            return true;
        }

        public void ClearText()
        {
            if (_LanguageStrings == null)
                return;

            foreach (LanguageString languageString in _LanguageStrings)
                languageString.Text = String.Empty;
        }

        public void CopyText(MultiLanguageString other)
        {
            if (_LanguageStrings != null)
            {
                _LanguageStrings.Clear();
                _LanguageStrings.Capacity = other._LanguageStrings.Capacity;
            }
            else
                _LanguageStrings = new List<LanguageString>(other.Count());

            foreach (LanguageString languageString in other.LanguageStrings)
                _LanguageStrings.Add(new LanguageString(Key, languageString.LanguageID, languageString.Text));

            _Modified = true;
        }

        // Returns false if any strings conflict.
        public bool Merge(MultiLanguageString other)
        {
            List<LanguageString> list = new List<LanguageString>(_LanguageStrings.Count() + other.Count());
            bool found = false;
            foreach (LanguageString ls1 in _LanguageStrings)
                list.Add(ls1);
            foreach (LanguageString ls2 in other.LanguageStrings)
            {
                foreach (LanguageString ls3 in list)
                {
                    if (ObjectUtilities.MatchKeys(ls3.Key, ls2.Key) && (ls3.LanguageID == ls2.LanguageID))
                    {
                        if (ls3.Text != ls2.Text)
                        {
                            if (!String.IsNullOrEmpty(ls3.Text) && !String.IsNullOrEmpty(ls2.Text))
                                return false;
                            else if (!String.IsNullOrEmpty(ls2.Text))
                                ls3.Text = ls2.Text;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                    list.Add(ls2);
            }
            _LanguageStrings = list;
            _Modified = true;
            return true;
        }

        public void Union(MultiLanguageString other)
        {
            List<LanguageString> list = new List<LanguageString>(_LanguageStrings.Count() + other.Count());
            foreach (LanguageString ls1 in _LanguageStrings)
                list.Add(ls1);
            foreach (LanguageString ls2 in other.LanguageStrings)
            {
                LanguageString ls = list.FirstOrDefault(x => (x.KeyString == ls2.KeyString) && (x.LanguageID == ls2.LanguageID));
                if (ls == null)
                    list.Add(new LanguageString(ls2));
                else if (String.IsNullOrEmpty(ls.Text))
                    ls.Text = ls2.Text;  
            }
            _LanguageStrings = list;
            _Modified = true;
        }

        public void UnionOverride(MultiLanguageString other)
        {
            List<LanguageString> list = new List<LanguageString>(_LanguageStrings.Count() + other.Count());
            foreach (LanguageString ls1 in _LanguageStrings)
                list.Add(ls1);
            foreach (LanguageString ls2 in other.LanguageStrings)
            {
                LanguageString ls = list.FirstOrDefault(x => (x.KeyString == ls2.KeyString) && (x.LanguageID == ls2.LanguageID));
                if (ls == null)
                    list.Add(new LanguageString(ls2));
                else
                    ls.Text = ls2.Text;
            }
            _LanguageStrings = list;
            _Modified = true;
        }

        // "other" strings will override existing strings.
        public bool Combine(MultiLanguageString other)
        {
            List<LanguageString> list = new List<LanguageString>(_LanguageStrings.Count() + other.Count());
            bool found = false;
            foreach (LanguageString ls1 in _LanguageStrings)
                list.Add(ls1);
            foreach (LanguageString ls2 in other.LanguageStrings)
            {
                foreach (LanguageString ls3 in list)
                {
                    if (ObjectUtilities.MatchKeys(ls3.Key, ls2.Key) && (ls3.LanguageID == ls2.LanguageID))
                    {
                        ls3.Text = ls2.Text;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    list.Add(ls2);
            }
            _LanguageStrings = list;
            _Modified = true;
            return true;
        }

        public List<LanguageString> FilteredLanguageStrings(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return new List<LanguageString>();
            List<LanguageString> languageStrings = new List<LanguageString>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
            {
                LanguageString languageString = LanguageString(languageID);
                if (languageString != null)
                    languageStrings.Add(new LanguageString(languageString));
            }
            return languageStrings;
        }

        public List<LanguageString> FilteredLanguageStrings(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return new List<LanguageString>();
            List<LanguageString> languageStrings = new List<LanguageString>(languageDescriptors.Count());
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used)
                    continue;
                LanguageID languageID = languageDescriptor.LanguageID;
                LanguageString languageString = LanguageString(languageID);
                if (languageString != null)
                    languageStrings.Add(new LanguageString(languageString));
            }
            return languageStrings;
        }

        public MultiLanguageString FilteredMultiLanguageString(List<LanguageID> languageIDs)
        {
            return new MultiLanguageString(Key, FilteredLanguageStrings(languageIDs));
        }

        public MultiLanguageString FilteredMultiLanguageString(List<LanguageDescriptor> languageDescriptors)
        {
            return new MultiLanguageString(Key, FilteredLanguageStrings(languageDescriptors));
        }

        public List<LanguageID> LanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();

                if (_LanguageStrings != null)
                {
                    foreach (LanguageString languageString in _LanguageStrings)
                    {
                        if (languageString.LanguageID != null)
                            languageIDs.Add(languageString.LanguageID);
                    }
                }

                return languageIDs;
            }
        }

        public int Count()
        {
            if (_LanguageStrings != null)
                return (_LanguageStrings.Count());
            return 0;
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            if (_KeyType != null)
                element.Add(new XAttribute("KeyType", _KeyType.Name));
            if (_Key != null)
                element.Add(new XAttribute("Key", _Key.ToString()));
            element.Add(new XAttribute("Count", _LanguageStrings.Count().ToString()));
            if (_CreationTime != DateTime.MinValue)
                element.Add(new XAttribute("CreationTime", ObjectUtilities.GetStringFromDateTime(_CreationTime)));
            if (_ModifiedTime != DateTime.MinValue)
                element.Add(new XAttribute("ModifiedTime", ObjectUtilities.GetStringFromDateTime(_ModifiedTime)));
            foreach (LanguageString ls in _LanguageStrings)
                element.Add(ls.Xml);
            return element;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "KeyType":
                    KeyType = ObjectUtilities.GetTypeFromString(attributeValue);
                    break;
                case "Key":
                    if (_KeyType != null)
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, _KeyType.Name);
                    else
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, null);
                    break;
                case "Count":
                    _LanguageStrings = new List<LanguageString>(Convert.ToInt32(attributeValue));
                    break;
                case "CreationTime":
                    _CreationTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                case "ModifiedTime":
                    _ModifiedTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            LanguageString languageString;

            switch (childElement.Name.LocalName)
            {
                case "LanguageString":
                    languageString = new LanguageString(childElement);
                    Add(languageString);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }

            Modified = false;
        }

        public virtual XElement Xml
        {
            get
            {
                return GetElement(GetType().Name);
            }
            set
            {
                OnElement(value);
            }
        }

        public virtual string StringData
        {
            get
            {
                XElement element = Xml;
                string xmlString = element.ToString();
                return xmlString;
            }
            set
            {
                XElement element = XElement.Parse(value, LoadOptions.PreserveWhitespace);
                Xml = element;
            }
        }

        public virtual byte[] BinaryData
        {
            get
            {
                XElement element = Xml;
                string xmlString = element.ToString();
                byte[] data = ApplicationData.Encoding.GetBytes(xmlString);
                return data;
            }
            set
            {
                string xmlString = ApplicationData.Encoding.GetString(value, 0, value.Count());
                XElement element = XElement.Parse(xmlString, LoadOptions.PreserveWhitespace);
                Xml = element;
            }
        }

        public void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public bool FromString(string value)
        {
            Xml = XElement.Parse(value, LoadOptions.PreserveWhitespace);
            return true;
        }

        public override int GetHashCode()
        {
            return _Key.GetHashCode();
        }

        public void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    {
                        string strs = String.Empty;
                        int c = Count();
                        int i;
                        for (i = 0; i < c; i++)
                        {
                            if (i != 0)
                                strs += "|";
                            strs += Text(i);
                        }
                        if (!String.IsNullOrEmpty(KeyString))
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString + ": " + strs);
                        else
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, strs);
                    }
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Full:
                    {
                        if (!String.IsNullOrEmpty(KeyString))
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                        else
                            ObjectUtilities.DisplayLabel(this, label, indent);
                        int c = Count();
                        int i;
                        for (i = 0; i < c; i++)
                        {
                            LanguageString ls = LanguageString(i);
                            string fmt = ls.LanguageAbbrev + ": " + Text(i);
                            ObjectUtilities.DisplayMessage(fmt, indent + 1);
                        }
                    }
                    break;
                case DisplayDetail.Xml:
                    {
                        XElement element = Xml;
                        string str = ObjectUtilities.GetIndentedElementString(element, indent + 1);
                        ObjectUtilities.DisplayLabel(this, label, indent);
                        ObjectUtilities.DisplayMessage(str, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        /*
        public override bool Equals(object obj)
        {
            return this.Equals(obj as MultiLanguageString);
        }

        public virtual bool Equals(IBaseObjectKeyed obj)
        {
            return this.Equals(obj as MultiLanguageString);
        }

        public virtual bool Equals(MultiLanguageString obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(MultiLanguageString other1, MultiLanguageString other2)
        {
            return Compare(other1, other2) == 0;
        }

        public static bool operator !=(MultiLanguageString other1, MultiLanguageString other2)
        {
            return Compare(other1, other2) != 0;
        }
        */

        public static int Compare(MultiLanguageString string1, MultiLanguageString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            int diff = ObjectUtilities.CompareKeys(string1, string2);
            if (diff != 0)
                return diff;
            if ((string1.LanguageStrings == null) && (string2.LanguageStrings == null))
                return 0;
            if (string1.LanguageStrings == null)
                return -1;
            if (string2.LanguageStrings == null)
                return 1;
            if (string1.LanguageStrings.Count() != string2.LanguageStrings.Count())
                return string1.LanguageStrings.Count() - string2.LanguageStrings.Count();
            int count = string1.Count();
            for (int i = 0; i < count; i++)
            {
                diff = JTLanguageModelsPortable.Object.LanguageString.Compare(string1.LanguageStrings[i], string2.LanguageStrings[i]);
                if (diff != 0)
                    return diff;
            }
            return 0;
        }

        public virtual void OnFixup(FixupDictionary fixups)
        {
        }

        public static int CompareKeys(MultiLanguageString string1, MultiLanguageString string2)
        {
            return ObjectUtilities.CompareKeys(string1, string2);
        }

        public static LanguageID LanguageToCompare;

        public static int CompareMultiLanguageString(MultiLanguageString string1, MultiLanguageString string2)
        {
            if (((object)string1 == null) && ((object)string2 == null))
                return 0;
            if ((object)string1 == null)
                return -1;
            if ((object)string2 == null)
                return 1;
            if (LanguageToCompare == null)
                LanguageToCompare = LanguageLookup.English;
            LanguageString ls1 = string1.LanguageString(LanguageToCompare);
            LanguageString ls2 = string2.LanguageString(LanguageToCompare);
            return JTLanguageModelsPortable.Object.LanguageString.Compare(ls1, ls2);
        }

        public virtual int Compare(IBaseObjectKeyed other)
        {
            return Compare(this, other as MultiLanguageString);
        }

        public virtual int CompareKey(object key)
        {
            return ObjectUtilities.CompareObjects(_Key, key);
        }

        public virtual bool MatchKey(object key)
        {
            return CompareKey(key) == 0;
        }

        public static int CompareMultiLanguageStringLists(List<MultiLanguageString> list1, List<MultiLanguageString> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<MultiLanguageString>(list1, list2);
        }

        public static List<MultiLanguageString> CopyMultiLanguageStringLists(List<MultiLanguageString> list)
        {
            return ObjectUtilities.CopyTypedBaseObjectList<MultiLanguageString>(list);
        }

        public static bool MergeOrCreate(ref MultiLanguageString mls1, ref MultiLanguageString mls2)
        {
            if ((mls1 != null) && (mls2 != null))
                return Merge(mls1, mls2);
            else if ((mls1 == null) && (mls2 == null))
            {
                return true;
            }
            else if (mls1 != null)
            {
                mls2 = new Object.MultiLanguageString(mls1);
                return true;
            }
            else if (mls2 != null)
            {
                mls1 = new Object.MultiLanguageString(mls2);
                return true;
            }

            return true;
        }

        public static bool Merge(MultiLanguageString mls1, MultiLanguageString mls2)
        {
            if ((mls1 != null) && (mls2 != null))
            {
                List<LanguageString> list = new List<LanguageString>(mls1.Count() + mls2.Count());
                bool found = false;
                foreach (LanguageString ls1 in mls1.LanguageStrings)
                    list.Add(ls1);
                foreach (LanguageString ls2 in mls2.LanguageStrings)
                {
                    foreach (LanguageString ls3 in list)
                    {
                        if (ObjectUtilities.MatchKeys(ls3.Key, ls2.Key) && (ls3.LanguageID == ls2.LanguageID))
                        {
                            if (ls3.Text != ls2.Text)
                            {
                                if (!String.IsNullOrEmpty(ls3.Text) && !String.IsNullOrEmpty(ls2.Text))
                                    return false;
                                else if (!String.IsNullOrEmpty(ls2.Text))
                                    ls3.Text = ls2.Text;
                            }
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        list.Add(ls2);
                }
                mls1.LanguageStrings = list;
                mls2.LanguageStrings = list;
                return true;
            }

            return false;
        }
    }
}

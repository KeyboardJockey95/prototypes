using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Content
{
    public class OptionContainer : BaseObjectKeyed
    {
        protected List<IBaseObjectKeyed> _Options;

        public OptionContainer(List<IBaseObjectKeyed> options)
        {
            _Options = options;
        }

        public OptionContainer(OptionContainer other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public OptionContainer(XElement element)
        {
            OnElement(element);
        }

        public OptionContainer()
        {
            ClearOptionContainer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearOptionContainer();
        }

        public void ClearOptionContainer()
        {
            _Options = null;
        }

        public void Copy(OptionContainer other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearOptionContainer();
                return;
            }

            _Options = ObjectUtilities.CopyBaseList(other.Options);
        }

        public void CopyDeep(OptionContainer other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new OptionContainer(this);
        }

        public virtual List<IBaseObjectKeyed> Options
        {
            get
            {
                return _Options;
            }
            set
            {
                if (value != _Options)
                {
                    _Options = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<IBaseObjectKeyed> CloneOptions()
        {
            return ObjectUtilities.CopyBaseList(Options);
        }

        public IBaseObjectKeyed GetOption(string key)
        {
            if ((_Options != null) && !String.IsNullOrEmpty(key))
                return _Options.FirstOrDefault(x => x.KeyString == key);
            return null;
        }

        public string GetOptionString(string key)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
                return option.Text;

            return null;
        }

        public string GetOptionString(string key, string defaultValue)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
                return option.Text;

            return defaultValue;
        }

        public List<string> GetOptionStringList(string key)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
                return ObjectUtilities.GetStringListFromString(option.Text);

            return null;
        }

        public List<string> GetOptionStringList(string key, List<string> defaultValue)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
                return ObjectUtilities.GetStringListFromString(option.Text);

            return defaultValue;
        }

        public bool GetOptionFlag(string key, bool defaultValue)
        {
            string value = GetOptionString(key);

            if (!String.IsNullOrEmpty(value))
                value = value.ToLower();

            switch (value)
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return defaultValue;
            }
        }

        public IBaseObjectKeyed GetOptionIndexed(int index)
        {
            if ((_Options != null) && (index >= 0) && (index < _Options.Count()))
                return _Options.ElementAt(index);
            return null;
        }

        public T GetOptionTyped<T>(string key) where T : IBaseObjectKeyed
        {
            if ((_Options != null) && !String.IsNullOrEmpty(key))
                return (T)_Options.FirstOrDefault(x => x.KeyString == key);
            return default(T);
        }

        public T GetOptionIndexedTyped<T>(int index) where T : IBaseObjectKeyed
        {
            if ((_Options != null) && (index >= 0) && (index < _Options.Count()))
                return (T)_Options.ElementAt(index);
            return default(T);
        }

        public bool AddOption(IBaseObjectKeyed option)
        {
            if (_Options == null)
                _Options = new List<IBaseObjectKeyed>(1) { option };
            else
                _Options.Add(option);
            ModifiedFlag = true;
            return true;
        }

        public bool InsertOption(int index, IBaseObjectKeyed option)
        {
            if (_Options == null)
                _Options = new List<IBaseObjectKeyed>(1) { option };
            else if (index >= _Options.Count)
                _Options.Add(option);
            else
                _Options.Insert(index, option);
            ModifiedFlag = true;
            return true;
        }

        public bool AddOptionString(string key, string value)
        {
            return AddOption(new BaseString(key, value));
        }

        public bool AddOptionFlag(string key, bool value)
        {
            return AddOption(new BaseString(key, (value ? "true" : "false")));
        }

        public bool SetOptionString(string key, string value)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
            {
                option.Text = value;
                return true;
            }

            return AddOptionString(key, value);
        }

        public bool SetOptionFlag(string key, bool value)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
            {
                option.Text = (value ? "true" : "false");
                return true;
            }

            return AddOptionFlag(key, value);
        }

        public bool CopyOptions(List<IBaseObjectKeyed> optionList)
        {
            DeleteAllOptions();

            if (optionList != null)
            {
                foreach (IBaseObjectKeyed option in optionList)
                {
                    IBaseObjectKeyed newOption = option.Clone() as IBaseObjectKeyed;
                    AddOption(newOption);
                }
            }

            return true;
        }

        public bool ResetOptionsFromDescriptors(List<OptionDescriptor> optionDescriptors)
        {
            if (optionDescriptors != null)
            {
                List<IBaseObjectKeyed> newOptions = new List<IBaseObjectKeyed>();

                foreach (OptionDescriptor optionDescriptor in optionDescriptors)
                {
                    IBaseObjectKeyed oldOption = GetOption(optionDescriptor.Name);

                    if (oldOption != null)
                    {
                        if (oldOption is BaseString)
                        {
                            BaseString oldOptionString = oldOption as BaseString;
                            oldOptionString.Text = optionDescriptor.Value;
                            newOptions.Add(oldOptionString);
                        }
                    }
                    else
                    {
                        IBaseObjectKeyed newOption = new BaseString(optionDescriptor.Name, optionDescriptor.Value);
                        newOptions.Add(newOption);
                    }
                }

                if (HasOptions())
                {
                    foreach (IBaseObjectKeyed oldOption in _Options)
                    {
                        string optionKey = oldOption.Name;

                        if (optionDescriptors.FirstOrDefault(x => x.Name == optionKey) == null)
                            newOptions.Add(oldOption);
                    }
                }

                Options = newOptions;
            }

            return true;
        }

        public bool DeleteOption(IBaseObjectKeyed option)
        {
            if (_Options != null)
            {
                if (_Options.Remove(option))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteOptionKey(string key)
        {
            if ((_Options != null) && !String.IsNullOrEmpty(key))
            {
                IBaseObjectKeyed option = GetOption(key);
                if (option != null)
                {
                    _Options.Remove(option);
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteOptionIndexed(int index)
        {
            if ((_Options != null) && (index >= 0) && (index < _Options.Count()))
            {
                _Options.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllOptions()
        {
            if (_Options != null)
            {
                if (_Options.Count() != 0)
                    ModifiedFlag = true;
                _Options.Clear();
            }
        }

        public int OptionCount()
        {
            if (_Options != null)
                return (_Options.Count());
            return 0;
        }

        public bool HasOptions()
        {
            if (_Options != null)
                return (_Options.Count != 0);
            return false;
        }

        public virtual List<IBaseObjectKeyed> GetDefaultOptions()
        {
            return null;
        }

        public virtual List<OptionDescriptor> OptionDescriptors
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public OptionDescriptor GetOptionDescriptor(string key)
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public OptionDescriptor GetOptionDescriptorIndexed(int index)
        {
            if ((OptionDescriptors != null) && (index >= 0) && (index < OptionDescriptors.Count()))
                return OptionDescriptors[index];

            return null;
        }

        public void AddOptionDescriptor(OptionDescriptor optionDescriptor)
        {
            if (OptionDescriptors == null)
                OptionDescriptors = new List<OptionDescriptor>(1) { optionDescriptor };
            else
                OptionDescriptors.Add(optionDescriptor);
            ModifiedFlag = true;
        }

        public void AddStringOptionDescriptor(string name, string label, string help, string value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "String", label, help, value);
            AddOptionDescriptor(optionDescriptor);
        }

        public void AddIntegerOptionDescriptor(string name, string label, string help, int value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "Int32", label, help, value.ToString());
            AddOptionDescriptor(optionDescriptor);
        }

        public void AddFlagOptionDescriptor(string name, string label, string help, bool value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "Boolean", label, help, value.ToString());
            AddOptionDescriptor(optionDescriptor);
        }

        public bool InsertOptionDescriptor(int index, OptionDescriptor optionDescriptor)
        {
            if (OptionDescriptors == null)
                OptionDescriptors = new List<OptionDescriptor>(1) { optionDescriptor };
            else if ((index >= 0) && (index < OptionDescriptors.Count()))
                OptionDescriptors.Insert(index, optionDescriptor);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteOptionDescriptorIndexed(int index)
        {
            if ((OptionDescriptors != null) && (index >= 0) && (index < OptionDescriptors.Count()))
                OptionDescriptors.RemoveAt(index);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public int OptionDescriptorCount()
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.Count();

            return 0;
        }

        public void CopyOptionDescriptors(List<OptionDescriptor> others)
        {
            if (others == null)
            {
                OptionDescriptors = null;
                return;
            }

            OptionDescriptors = new List<OptionDescriptor>(others.Count());

            foreach (OptionDescriptor other in others)
                OptionDescriptors.Add(new OptionDescriptor(other));
        }

        public virtual string GetOptionLabel(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Label;
            return null;
        }

        public virtual string GetOptionType(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Type;
            return null;
        }

        public virtual string GetOptionHelp(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Help;
            return null;
        }

        public virtual string GetOptionValue(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Value;
            return null;
        }

        public virtual List<string> GetOptionValues(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Values;
            return null;
        }

        public virtual List<IBaseObjectKeyed> GetOptionsFromDescriptors(List<OptionDescriptor> optionDescriptors)
        {
            if (optionDescriptors == null)
                return null;

            List<IBaseObjectKeyed> options = new List<IBaseObjectKeyed>(optionDescriptors.Count());

            foreach (OptionDescriptor optionDescriptor in optionDescriptors)
            {
                IBaseObjectKeyed option = null;

                if (optionDescriptor.Value != null)
                    option = new BaseString(optionDescriptor.KeyString, optionDescriptor.Value);

                if (option != null)
                    options.Add(option);
            }

            return options;
        }

        public virtual List<IBaseObjectKeyed> GetOptionsFromDescriptors()
        {
            List<IBaseObjectKeyed> options = GetOptionsFromDescriptors(OptionDescriptors);
            return options;
        }

        public virtual string GetOptionsStringFromDescriptors()
        {
            List<OptionDescriptor> optionDescriptors = OptionDescriptors;
            StringBuilder sb = new StringBuilder();
            int index = 0;

            if (optionDescriptors == null)
                return "";

            foreach (OptionDescriptor optionDescriptor in optionDescriptors)
            {
                if (optionDescriptor.Value != null)
                {
                    sb.Append((index != 0 ? "," : "") + optionDescriptor.KeyString + "=" + optionDescriptor.Value);
                    index++;
                }
            }

            return sb.ToString();
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_Options != null)
                {
                    foreach (IBaseObjectKeyed option in _Options)
                    {
                        if (option.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Options != null)
                {
                    foreach (IBaseObjectKeyed option in _Options)
                        option.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if ((_Options != null) && (_Options.Count() != 0))
            {
                XElement optionsElement = new XElement("Options");
                foreach (IBaseObjectKeyed option in _Options)
                    optionsElement.Add(option.Xml);
                element.Add(optionsElement);
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Options":
                    foreach (XElement optionElement in childElement.Elements())
                    {
                        IBaseObjectKeyed option = ObjectUtilities.ResurrectBase(optionElement);
                        AddOption(option);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            OptionContainer otherOptionContainer = other as OptionContainer;

            if (otherOptionContainer == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = CompareOptions(_Options, otherOptionContainer.Options);

            if (diff != 0)
                return diff;

            return diff;
        }

        public static int Compare(OptionContainer item1, OptionContainer item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            int diff = item1.Compare(item2);
            return diff;
        }

        public static int CompareKeys(OptionContainer object1, OptionContainer object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareContainerLists(List<OptionContainer> object1, List<OptionContainer> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<OptionContainer>(object1, object2);
        }

        public static int CompareOptions(List<IBaseObjectKeyed> list1, List<IBaseObjectKeyed> list2)
        {
            if (list1 == list2)
                return 0;

            if (list2 == null)
                return 1;

            if (list1 == null)
                return -1;

            int count1 = list1.Count();
            int count2 = list2.Count();
            int count = (count1 > count2 ? count2 : count1);
            int index;
            int diff;
            List<IBaseObjectKeyed> options1 = new List<IBaseObjectKeyed>(list1);
            List<IBaseObjectKeyed> options2 = new List<IBaseObjectKeyed>(list2);

            options1.Sort();
            options2.Sort();

            for (index = 0; index > count; index++)
            {
                diff = ObjectUtilities.Compare(options1[index], options2[index]);

                if (diff != 0)
                    return diff;
            }

            diff = count1 - count2;

            return diff;
        }
    }
}

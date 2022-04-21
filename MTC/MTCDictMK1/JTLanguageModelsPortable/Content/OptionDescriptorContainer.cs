using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Content
{
    public class OptionDescriptorContainer : BaseObjectKeyed
    {
        protected List<OptionDescriptor> _OptionDescriptors;

        public OptionDescriptorContainer(List<OptionDescriptor> options)
        {
            _OptionDescriptors = options;
        }

        public OptionDescriptorContainer(OptionDescriptorContainer other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public OptionDescriptorContainer(XElement element)
        {
            OnElement(element);
        }

        public OptionDescriptorContainer()
        {
            ClearOptionDescriptorContainer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearOptionDescriptorContainer();
        }

        public void ClearOptionDescriptorContainer()
        {
            _OptionDescriptors = null;
        }

        public void Copy(OptionDescriptorContainer other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearOptionDescriptorContainer();
                return;
            }

            _OptionDescriptors = OptionDescriptor.CopyOptionDescriptorList(other.OptionDescriptors);
        }

        public void CopyDeep(OptionDescriptorContainer other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new OptionDescriptorContainer(this);
        }

        public virtual List<OptionDescriptor> OptionDescriptors
        {
            get
            {
                return _OptionDescriptors;
            }
            set
            {
                if (value != _OptionDescriptors)
                {
                    _OptionDescriptors = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<OptionDescriptor> CloneOptionDescriptors()
        {
            return OptionDescriptor.CopyOptionDescriptorList(OptionDescriptors);
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

        public virtual List<IBaseObjectKeyed> GetOptions(List<OptionDescriptor> optionDescriptors)
        {
            if (optionDescriptors == null)
                return null;

            List<IBaseObjectKeyed> options = new List<IBaseObjectKeyed>(optionDescriptors.Count);

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

        public virtual List<IBaseObjectKeyed> GetOptions()
        {
            List<IBaseObjectKeyed> options = GetOptions(OptionDescriptors);
            return options;
        }

        public virtual string GetOptionDescriptorsStringFromDescriptors()
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

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor option in _OptionDescriptors)
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

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor option in _OptionDescriptors)
                        option.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if ((_OptionDescriptors != null) && (_OptionDescriptors.Count() != 0))
            {
                XElement optionsElement = new XElement("OptionDescriptors");
                foreach (OptionDescriptor option in _OptionDescriptors)
                    optionsElement.Add(option.Xml);
                element.Add(optionsElement);
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "OptionDescriptors":
                    foreach (XElement optionDesriptorElement in childElement.Elements())
                    {
                        OptionDescriptor optionDesriptor = ObjectUtilities.ResurrectBase(optionDesriptorElement) as OptionDescriptor;
                        AddOptionDescriptor(optionDesriptor);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            OptionDescriptorContainer otherOptionDescriptorContainer = other as OptionDescriptorContainer;

            if (otherOptionDescriptorContainer == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = CompareOptionDescriptors(_OptionDescriptors, otherOptionDescriptorContainer.OptionDescriptors);

            if (diff != 0)
                return diff;

            return diff;
        }

        public static int Compare(OptionDescriptorContainer item1, OptionDescriptorContainer item2)
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

        public static int CompareKeys(OptionDescriptorContainer object1, OptionDescriptorContainer object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareContainerLists(List<OptionDescriptorContainer> object1, List<OptionDescriptorContainer> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<OptionDescriptorContainer>(object1, object2);
        }

        public static int CompareOptionDescriptors(List<OptionDescriptor> list1, List<OptionDescriptor> list2)
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
            List<OptionDescriptor> options1 = new List<OptionDescriptor>(list1);
            List<OptionDescriptor> options2 = new List<OptionDescriptor>(list2);

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

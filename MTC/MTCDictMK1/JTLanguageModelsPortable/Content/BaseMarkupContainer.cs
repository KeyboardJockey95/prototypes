using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Markup;

namespace JTLanguageModelsPortable.Content
{
    public enum ContentStorageStateCode
    {
        Unknown,        // We don't know right now.
        Empty,          // Content is empty.
        NotEmpty        // Content is not empty.
    };

    public class BaseMarkupContainer : BaseContentContainer
    {
        protected List<IBaseObjectKeyed> _Options;
        protected MarkupTemplate _LocalMarkupTemplate;
        protected MarkupTemplateReference _MarkupReference;
        protected string _MarkupUse;
        public static List<string> MarkupUseStrings = new List<string>()
        {
            "Normal", "Top Notes", "Bottom Notes"
        };
        ContentStorageStateCode _ContentStorageState;

        public BaseMarkupContainer(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                List<IBaseObjectKeyed> options, MarkupTemplate markupTemplate, MarkupTemplateReference markupReference,
                BaseObjectContent contentParent, List<BaseObjectContent> contentChildren)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner, contentParent, contentChildren)
        {
            _Options = options;
            _LocalMarkupTemplate = markupTemplate;
            _MarkupReference = markupReference;
            _MarkupUse = null;
            _ContentStorageState = ContentStorageStateCode.Unknown;
        }

        public BaseMarkupContainer(BaseMarkupContainer other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseMarkupContainer(object key)
            : base(key)
        {
            ClearBaseMarkupContainer();
        }

        public BaseMarkupContainer(BaseMarkupContainer other, object key)
            : base(other, key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseMarkupContainer(XElement element)
        {
            OnElement(element);
        }

        public BaseMarkupContainer()
        {
            ClearBaseMarkupContainer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseMarkupContainer();
        }

        public void ClearBaseMarkupContainer()
        {
            _Options = null;
            _LocalMarkupTemplate = null;
            _MarkupReference = null;
            _MarkupUse = null;
            _ContentStorageState = ContentStorageStateCode.Unknown;
        }

        public void Copy(BaseMarkupContainer other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearBaseMarkupContainer();
                return;
            }

            _Options = ObjectUtilities.CopyBaseList(other.Options);

            CopyMarkup(other);

            _ContentStorageState = other.ContentStorageState;
        }

        public void CopyDeep(BaseMarkupContainer other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public void CopyOptions(BaseMarkupContainer other)
        {
            _Options = ObjectUtilities.CopyBaseList(other.Options);
        }

        public void CopyMarkup(BaseMarkupContainer other)
        {
            if (other.LocalMarkupTemplate != null)
                _LocalMarkupTemplate = new MarkupTemplate(other.LocalMarkupTemplate);
            else
                _LocalMarkupTemplate = null;

            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;

            _MarkupUse = other.MarkupUse;
        }

        public override IBaseObject Clone()
        {
            return new BaseMarkupContainer(this);
        }

        public void CleanOptions()
        {
            Options = null;
        }

        public void CleanMarkup()
        {
            LocalMarkupTemplate = null;
            MarkupReference = null;
            _MarkupUse = null;
        }

        public void PropogateOptions(BaseMarkupContainer content, bool force)
        {
            if (force || (_Options == null))
            {
                if (content.Options != null)
                    Options = ObjectUtilities.CopyBaseList(content.Options);
                else
                    Options = null;
            }
        }

        public void PropogateMarkup(BaseMarkupContainer content, bool force)
        {
            if (force || (_LocalMarkupTemplate == null))
            {
                if (content.LocalMarkupTemplate != null)
                    LocalMarkupTemplate = new MarkupTemplate(content.LocalMarkupTemplate);
                else
                    LocalMarkupTemplate = null;
            }

            if (force || (_MarkupReference == null))
            {
                if (content.MarkupReference != null)
                    MarkupReference = new MarkupTemplateReference(content.MarkupReference);
                else
                    MarkupReference = null;
            }

            if (force || !String.IsNullOrEmpty(_MarkupUse))
                MarkupUse = content.MarkupUse;
        }

        public List<IBaseObjectKeyed> Options
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

        public bool HasOption(string key)
        {
            BaseString option = GetOption(key) as BaseString;

            if (option != null)
                return true;

            return false;
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

        public virtual bool FindContainerAndOptionFlag(string optionKey, out BaseMarkupContainer container, out bool flag)
        {
            string value = GetOptionString(optionKey);

            container = null;
            flag = false;

            if (String.IsNullOrEmpty(value))
                return false;

            value = value.ToLower();

            switch (value)
            {
                case "true":
                    flag = true;
                    container = this;
                    break;
                case "false":
                    flag = false;
                    container = this;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual List<int> GetIndexPath()
        {
            List<int> indexPath = new List<int>();
            return indexPath;
        }

        public virtual List<string> GetNamePath(LanguageID uiLanguageID)
        {
            List<string> namePath = new List<string>() { GetTitleString(uiLanguageID) };
            return namePath;
        }

        public virtual string GetNamePathString(LanguageID uiLanguageID, string separator)
        {
            return GetTitleString(uiLanguageID);
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

        public bool HasOptionDescriptors()
        {
            if (OptionDescriptors != null)
                return (OptionDescriptors.Count != 0);
            return false;
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

        public virtual string GetInheritedOptionValue(string optionKey)
        {
            string optionValue = GetOptionString(optionKey);
            return optionValue;
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

        public bool HasMarkup
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    switch (_MarkupReference.KeyString)
                    {
                        case "(none)":
                            return false;
                        case "(local)":
                            if (_LocalMarkupTemplate != null)
                                return true;
                            return false;
                        default:
                            return true;
                    }
                }
                return false;
            }
        }

        public bool HasMarkupReference
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    if (!_MarkupReference.KeyString.StartsWith("("))
                        return true;
                }
                return false;
            }
        }

        public MarkupTemplateReference MarkupReference
        {
            get
            {
                return _MarkupReference;
            }
            set
            {
                if (value != _MarkupReference)
                {
                    _MarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MarkupTemplateReference CloneMarkupReference()
        {
            if (_MarkupReference != null)
                return new MarkupTemplateReference(_MarkupReference);
            else
                return null;
        }

        public MarkupTemplate LocalMarkupTemplate
        {
            get
            {
                return _LocalMarkupTemplate;
            }
            set
            {
                if (value != _LocalMarkupTemplate)
                {
                    _LocalMarkupTemplate = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MarkupTemplate CloneMarkupTemplate()
        {
            if (_LocalMarkupTemplate != null)
                return new MarkupTemplate(_LocalMarkupTemplate);
            else
                return null;
        }

        public MarkupTemplate MarkupTemplate
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    switch (_MarkupReference.KeyString)
                    {
                        case "(none)":
                            return null;
                        case "(local)":
                            return _LocalMarkupTemplate;
                        default:
                            return _MarkupReference.Item;
                    }
                }
                return _LocalMarkupTemplate;
            }
            set
            {
                if (value != _LocalMarkupTemplate)
                {
                    _LocalMarkupTemplate = value;
                    ModifiedFlag = true;
                }
                if (value != null)
                {
                    if (_MarkupReference != null)
                    {
                        if (_MarkupReference.Key != value.Key)
                        {
                            _MarkupReference.Key = value.Key;
                            _MarkupReference.Item = value;
                            ModifiedFlag = true;
                        }
                    }
                    else
                        _MarkupReference = new MarkupTemplateReference(value);
                }
                else
                    _MarkupReference = null;
            }
        }

        public string MarkupUse
        {
            get
            {
                return _MarkupUse;
            }
            set
            {
                if (value != _MarkupUse)
                {
                    _MarkupUse = value;
                    ModifiedFlag = true;
                }
            }
        }

        public ContentStorageStateCode ContentStorageState
        {
            get
            {
                return _ContentStorageState;
            }
            set
            {
                if (_ContentStorageState != value)
                {
                    _ContentStorageState = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            base.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);

            if (_LocalMarkupTemplate != null)
            {
                _LocalMarkupTemplate.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                    nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
            }

            if ((_MarkupReference != null) && (_MarkupReference.Item != null) &&
                (_MarkupReference.KeyString != "(local)") && (_MarkupReference.KeyString != "(none)"))
            {
                MarkupTemplate markupTemplate = _MarkupReference.Item;
                AddUniqueReference(references, markupTemplate);

                markupTemplate.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                    nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
            }
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

                if (_LocalMarkupTemplate != null)
                {
                    if (_LocalMarkupTemplate.Modified)
                        return true;
                }

                if (_MarkupReference != null)
                {
                    if (_MarkupReference.Modified)
                        return true;
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

                if (_LocalMarkupTemplate != null)
                    _LocalMarkupTemplate.Modified = false;

                if (_MarkupReference != null)
                    _MarkupReference.Modified = false;
            }
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if ((_MarkupReference != null) && (_MarkupReference.Key != null)
                    && !_MarkupReference.KeyString.StartsWith("(") && (_MarkupReference.Item == null))
                _MarkupReference.ResolveReference(mainRepository);

            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.SaveReference(mainRepository);

            if (!base.SaveReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.UpdateReference(mainRepository);

            if (!base.UpdateReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.UpdateReferenceCheck(mainRepository);

            if (!base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if ((_MarkupReference != null))
                _MarkupReference.ClearReference();

            base.ClearReferences(recurseParents, recurseChildren);
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

            if (_LocalMarkupTemplate != null)
                element.Add(_LocalMarkupTemplate.GetElement("LocalMarkupTemplate"));

            if (_MarkupReference != null)
                element.Add(_MarkupReference.GetElement("MarkupReference"));

            if (!String.IsNullOrEmpty(_MarkupUse))
                element.Add(new XElement("MarkupUse", MarkupUse));

            if (_ContentStorageState != ContentStorageStateCode.Unknown)
                element.Add(new XElement("ContentStorageState", _ContentStorageState.ToString()));

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<int, bool> childNodeFlags,
            Dictionary<string, bool> childContentFlags)
        {
            XElement element = base.GetElementFiltered(name, childNodeFlags, childContentFlags);

            if ((_Options != null) && (_Options.Count() != 0))
            {
                XElement optionsElement = new XElement("Options");
                foreach (IBaseObjectKeyed option in _Options)
                    optionsElement.Add(option.Xml);
                element.Add(optionsElement);
            }

            if (_LocalMarkupTemplate != null)
                element.Add(_LocalMarkupTemplate.GetElement("LocalMarkupTemplate"));

            if (_MarkupReference != null)
                element.Add(_MarkupReference.GetElement("MarkupReference"));

            if (!String.IsNullOrEmpty(_MarkupUse))
                element.Add(new XElement("MarkupUse", MarkupUse));

            if (_ContentStorageState != ContentStorageStateCode.Unknown)
                element.Add(new XElement("ContentStorageState", _ContentStorageState.ToString()));

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
                case "LocalMarkupTemplate":
                    _LocalMarkupTemplate = new MarkupTemplate(childElement);
                    if (String.IsNullOrEmpty(_LocalMarkupTemplate.Owner))
                        _LocalMarkupTemplate.Owner = Owner;
                    _LocalMarkupTemplate.LocalOwningObject = this;
                    break;
                case "MarkupReference":
                    _MarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "MarkupUse":
                    _MarkupUse = childElement.Value.Trim();
                    break;
                case "ContentStorageState":
                    _ContentStorageState = GetContentStorageStateFromString(childElement.Value.Trim());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseMarkupContainer otherBaseMarkupContainer = other as BaseMarkupContainer;

            if (otherBaseMarkupContainer == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = CompareOptions(_Options, otherBaseMarkupContainer.Options);

            if (diff != 0)
                return diff;

            return diff;
        }

        public static int Compare(BaseMarkupContainer item1, BaseMarkupContainer item2)
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

        public static int CompareKeys(BaseMarkupContainer object1, BaseMarkupContainer object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareContainerLists(List<BaseMarkupContainer> object1, List<BaseMarkupContainer> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<BaseMarkupContainer>(object1, object2);
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

        public static ContentStorageStateCode GetContentStorageStateFromString(string str)
        {
            ContentStorageStateCode code = ContentStorageStateCode.Unknown;

            switch (str)
            {
                case "Unknown":
                default:
                    break;
                case "Empty":
                    code = ContentStorageStateCode.Empty;
                    break;
                case "NotEmpty":
                    code = ContentStorageStateCode.NotEmpty;
                    break;
            }

            return code;
        }
    }
}

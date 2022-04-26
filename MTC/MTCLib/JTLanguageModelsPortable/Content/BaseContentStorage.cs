using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;

namespace JTLanguageModelsPortable.Content
{
    public class BaseContentStorage : BaseObjectKeyed
    {
        protected Guid _Guid;
        protected string _Source;
        protected bool _IsReference;
        protected object _ReferenceSourceKey;
        protected object _ReferenceTreeKey;
        protected int _ReferenceCount;
        protected int _RemoteKey;
        // Owning content object.  Not stored in database, but set dynamically.
        protected BaseObjectContent _Content;
        // Owning tree.  Not stored in database, but cached here.
        protected BaseObjectNodeTree _ReferenceTree;
        protected List<IBaseObjectKeyed> _Options;
        protected MarkupTemplate _LocalMarkupTemplate;
        protected MarkupTemplateReference _MarkupReference;
        protected string _MarkupUse;  // See MarkupUseString in BaseMarkupContainer.

        public BaseContentStorage(object key, string source, BaseObjectContent content)
            : base(key)
        {
            ClearBaseContentStorage();
            _Source = source;
            _Content = content;
        }

        public BaseContentStorage(BaseContentStorage other)
            : base(other)
        {
            CopyBaseContentStorage(other);
            ModifiedFlag = false;
        }

        public BaseContentStorage(object key)
            : base(key)
        {
            ClearBaseContentStorage();
        }

        public BaseContentStorage(XElement element)
        {
            OnElement(element);
        }

        public BaseContentStorage()
        {
            ClearBaseContentStorage();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseContentStorage();
        }

        public void ClearBaseContentStorage()
        {
            _Guid = Guid.Empty;
            _Source = String.Empty;
            _IsReference = false;
            _ReferenceSourceKey = null;
            _ReferenceTreeKey = null;
            _ReferenceCount = 1;
            _RemoteKey = -1;
            _Content = null;
            _ReferenceTree = null;
            _Options = null;
            _LocalMarkupTemplate = null;
            _MarkupReference = null;
            _MarkupUse = null;
        }

        public void CopyBaseContentStorage(BaseContentStorage other)
        {
            if (other == null)
            {
                ClearBaseContentStorage();
                return;
            }
            _Guid = other.Guid;
            _Source = other.Source;
            _IsReference = other.IsReference;
            _ReferenceSourceKey = other.ReferenceSourceKey;
            _ReferenceTreeKey = other.ReferenceTreeKey;
            _ReferenceCount = 1;
            _RemoteKey = other.RemoteKey;
            _Content = other.Content;
            _ReferenceTree = other.ReferenceTree;

            _Options = ObjectUtilities.CopyBaseList(other.Options);

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
            return new BaseContentStorage(this);
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

        public virtual void PropagateLanguages(BaseObjectLanguages other)
        {
        }

        public virtual ContentClassType ContentClass
        {
            get
            {
                return ContentClassType.StudyList;
            }
        }

        public override Guid Guid
        {
            get
            {
                return _Guid;
            }
            set
            {
                if (value != _Guid)
                {
                    _Guid = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override bool EnsureGuid()
        {
            bool returnValue;

            if (Guid == Guid.Empty)
            {
                Guid = Guid.NewGuid();
                returnValue = false;
            }
            else
                returnValue = true;

            return returnValue;
        }

        public override void NewGuid()
        {
            _Guid = Guid.NewGuid();
        }

        public override string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                if (value != _Source)
                {
                    _Source = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsReference
        {
            get
            {
                return _IsReference;
            }
            set
            {
                if (value != _IsReference)
                {
                    _IsReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public object ReferenceSourceKey
        {
            get
            {
                return _ReferenceSourceKey;
            }
            set
            {
                if (value != _ReferenceSourceKey)
                {
                    _ReferenceSourceKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual BaseContentStorage ReferenceSource
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public object ReferenceTreeKey
        {
            get
            {
                return _ReferenceTreeKey;
            }
            set
            {
                if (value != _ReferenceTreeKey)
                {
                    _ReferenceTreeKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual BaseObjectNodeTree ReferenceTree
        {
            get
            {
                return _ReferenceTree;
            }
            set
            {
                _ReferenceTree = value;
            }
        }

        public int ReferenceCount
        {
            get
            {
                return _ReferenceCount;
            }
            set
            {
                if (_ReferenceCount != value)
                {
                    _ReferenceCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int RemoteKey
        {
            get
            {
                return _RemoteKey;
            }
            set
            {
                if (value != _RemoteKey)
                {
                    _RemoteKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual BaseObjectContent Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }

        public virtual BaseObjectNode Node
        {
            get
            {
                if (_Content != null)
                    return _Content.Node;
                return null;
            }
        }

        public virtual BaseObjectNodeTree Tree
        {
            get
            {
                if (_Content != null)
                    return _Content.Tree;
                return null;
            }
        }

        public virtual string MediaTildeUrl
        {
            get
            {
                if (Content != null)
                    return Content.MediaTildeUrl;

                return String.Empty;
            }
        }

        public virtual void SetupOptions(MasterContentItem contentItem)
        {
        }

        public virtual bool CopyMedia(string newDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            return true;
        }

        public virtual void ConvertToReference(BaseContentStorage sourceContentStorage)
        {
            IsReference = true;
            _ReferenceSourceKey = sourceContentStorage.Key;
            _ReferenceTreeKey = sourceContentStorage.ReferenceTreeKey;
        }

        public virtual void SaveToReference()
        {
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
            if (!String.IsNullOrEmpty(optionValue))
            {
                if (optionValue != "Inherited")
                    return optionValue;
            }
            if (Content != null)
                return Content.GetInheritedOptionValue(optionKey);
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

        public static List<string> EditPermissionStrings = new List<string>()
        {
            "Yes",
            "No",
            "Inherit"
        };

        public static OptionDescriptor CreateEditPermissionOptionDescriptor(string defaultValue)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(
                "OtherTeachersCanEdit",
                "stringset",
                "Other teachers can edit",
                "This option, if \"Yes\", indicates that other teachers can edit this item."
                    + " If \"No\", it indicates that other teachers cannot edit this item."
                    + " If \"Inherit\", the actual setting is obtained from the nearest ancestor with a \"Yes\" or \"No\" setting, or the teachers global flag,",
                defaultValue,
                EditPermissionStrings);
            return optionDescriptor;
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

        public virtual void SetupMarkupTemplate(
            BaseObjectContent content,
            MasterContentItem contentItem)
        {
            if (contentItem.IsLocalMarkupTemplate)
            {
                if (_LocalMarkupTemplate == null)
                {
                    if (contentItem.CopyMarkupTemplate != null)
                        LocalMarkupTemplate = new MarkupTemplate(contentItem.CopyMarkupTemplate);
                    else
                        LocalMarkupTemplate = new MarkupTemplate("(local)");

                    _LocalMarkupTemplate.LocalOwningObject = content;
                }
            }
            if (contentItem.MarkupReference != null)
                MarkupReference = new MarkupTemplateReference(contentItem.MarkupReference);
            else
                MarkupReference = null;
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

        public virtual void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if ((_MarkupReference != null) && (_MarkupReference.Key != null)
                    && !_MarkupReference.KeyString.StartsWith("(") && (_MarkupReference.Item == null))
                _MarkupReference.ResolveReference(mainRepository);
        }

        public virtual bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.SaveReference(mainRepository);

            return returnValue;
        }

        public virtual bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.UpdateReference(mainRepository);

            return returnValue;
        }

        public virtual bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                _MarkupReference.UpdateReferenceCheck(mainRepository);

            return returnValue;
        }

        public virtual void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if ((_MarkupReference != null))
                _MarkupReference.ClearReference();
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

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_Guid != Guid.Empty)
                element.Add(new XAttribute("Guid", _Guid.ToString()));

            if (!String.IsNullOrEmpty(_Source))
                element.Add(new XAttribute("Source", _Source));

            if (_IsReference)
                element.Add(new XAttribute("IsReference", _IsReference.ToString()));

            if (_ReferenceSourceKey != null)
                element.Add(new XAttribute("ReferenceSourceKey", _ReferenceSourceKey.ToString()));

            if (_ReferenceTreeKey != null)
                element.Add(new XAttribute("ReferenceTreeKey", _ReferenceTreeKey.ToString()));

            if (_ReferenceCount != 0)
                element.Add(new XAttribute("ReferenceCount", _ReferenceCount.ToString()));

            if (_RemoteKey != -1)
                element.Add(new XAttribute("RemoteKey", _RemoteKey.ToString()));

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

            return element;
        }

        public virtual XElement GetElementFiltered(string name, Dictionary<string, bool> itemKeyFlags)
        {
            XElement element = base.GetElement(name);

            if (_Guid != Guid.Empty)
                element.Add(new XAttribute("Guid", _Guid.ToString()));

            if (!String.IsNullOrEmpty(_Source))
                element.Add(new XAttribute("Source", _Source));

            if (_IsReference)
                element.Add(new XAttribute("IsReference", _IsReference.ToString()));

            if (_ReferenceSourceKey != null)
                element.Add(new XAttribute("ReferenceSourceKey", _ReferenceSourceKey.ToString()));

            if (_ReferenceTreeKey != null)
                element.Add(new XAttribute("ReferenceTreeKey", _ReferenceTreeKey.ToString()));

            if (_ReferenceCount != 0)
                element.Add(new XAttribute("ReferenceCount", _ReferenceCount.ToString()));

            if (_RemoteKey != -1)
                element.Add(new XAttribute("RemoteKey", _RemoteKey.ToString()));

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

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Guid":
                    _Guid = Guid.Parse(attributeValue);
                    break;
                case "Source":
                    _Source = attributeValue;
                    break;
                case "IsReference":
                    IsReference = (attributeValue == true.ToString());
                    break;
                case "ReferenceSourceKey":
                    ReferenceSourceKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "ReferenceTreeKey":
                    ReferenceTreeKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "ReferenceCount":
                    ReferenceCount = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "RemoteKey":
                    RemoteKey = ObjectUtilities.ParseIntKeyString(attributeValue);
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
                    //_LocalMarkupTemplate.LocalOwningObject = this;
                    break;
                case "MarkupReference":
                    _MarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "MarkupUse":
                    _MarkupUse = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseContentStorage otherBaseContentStorage = other as BaseContentStorage;

            if (otherBaseContentStorage == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = CompareOptions(_Options, otherBaseContentStorage.Options);

            if (diff != 0)
                return diff;

            return diff;
        }

        public static int Compare(BaseContentStorage item1, BaseContentStorage item2)
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

        public static int CompareKeys(BaseContentStorage object1, BaseContentStorage object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareContainerLists(List<BaseContentStorage> object1, List<BaseContentStorage> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<BaseContentStorage>(object1, object2);
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

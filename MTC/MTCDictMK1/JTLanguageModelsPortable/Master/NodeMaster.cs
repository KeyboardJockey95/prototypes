using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Master
{
    public class NodeMaster : BaseObjectTitled
    {
        protected List<MasterContentItem> _ContentItems;
        protected List<MasterMenuItem> _MenuItems;
        protected MarkupTemplateReference _MarkupReference;
        protected MarkupTemplateReference _CopyMarkupReference;
        protected List<OptionDescriptor> _OptionDescriptors;

        public NodeMaster(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                List<MasterContentItem> contentItems, List<MasterMenuItem> menuItems,
                MarkupTemplateReference markupReference, MarkupTemplateReference copyMarkupReference,
                List<OptionDescriptor> optionDescriptors)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            _ContentItems = contentItems;
            _MenuItems = menuItems;
            _MarkupReference = markupReference;
            _CopyMarkupReference = copyMarkupReference;
            _OptionDescriptors = optionDescriptors;
        }

        public NodeMaster(NodeMaster other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public NodeMaster(object key, NodeMaster other)
            : base(other, key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public NodeMaster(XElement element)
        {
            OnElement(element);
        }

        public NodeMaster()
        {
            ClearNodeMaster();
        }

        public override void Clear()
        {
            base.Clear();
            ClearNodeMaster();
        }

        public void ClearNodeMaster()
        {
            _ContentItems = null;
            _MenuItems = null;
            _MarkupReference = null;
            _CopyMarkupReference = null;
            _OptionDescriptors = null;
        }

        public void Copy(NodeMaster other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            base.Copy(other);

            _ContentItems = MasterContentItem.CopyMasterContentItemList(other.ContentItems);
            _MenuItems = MasterMenuItem.CopyMasterMenuItemList(other.MenuItems);
            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;
            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
            _OptionDescriptors = OptionDescriptor.CopyOptionDescriptorList(other.OptionDescriptors);
            PropogateLanguages();
            SetOwner(Owner);
        }

        public void CopyFrom(NodeMaster other)
        {
            _ContentItems = MasterContentItem.CopyMasterContentItemList(other.ContentItems);
            _MenuItems = MasterMenuItem.CopyMasterMenuItemList(other.MenuItems);
            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;
            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
            _OptionDescriptors = OptionDescriptor.CopyOptionDescriptorList(other.OptionDescriptors);
            SetOwner(Owner);
        }

        public void CopyProfile(NodeMaster other)
        {
            CopyTitledObjectAndLanguages(other);

            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;

            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
        }

        public void CopyProfileExpand(NodeMaster other, UserProfile userProfile)
        {
            CopyTitledObjectAndLanguagesExpand(other, userProfile);

            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;

            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
        }

        public void PropogateLanguages()
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.CopyLanguages(this);
            }
        }

        public void SetOwner(string owner)
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.Owner = owner;
            }

            Owner = owner;
        }

        public override IBaseObject Clone()
        {
            return new NodeMaster(this);
        }

        public List<MasterContentItem> ContentItems
        {
            get
            {
                return _ContentItems;
            }
            set
            {
                if (_ContentItems == value)
                    return;

                if (MasterContentItem.CompareMasterContentItemLists(_ContentItems, value) != 0)
                {
                    _ContentItems = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<MasterMenuItem> MenuItems
        {
            get
            {
                return _MenuItems;
            }
            set
            {
                if (_MenuItems == value)
                    return;

                if (MasterMenuItem.CompareMasterMenuItemLists(_MenuItems, value) != 0)
                {
                    _MenuItems = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_ContentItems != null)
                {
                    foreach (MasterContentItem item in _ContentItems)
                    {
                        if (item.Modified)
                            return true;
                    }
                }

                if (_MenuItems != null)
                {
                    foreach (MasterMenuItem item in _MenuItems)
                    {
                        if (item.Modified)
                            return true;
                    }
                }

                if (_MarkupReference != null)
                {
                    if (_MarkupReference.Modified)
                        return true;
                }

                if (_CopyMarkupReference != null)
                {
                    if (_CopyMarkupReference.Modified)
                        return true;
                }

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor item in _OptionDescriptors)
                    {
                        if (item.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_ContentItems != null)
                {
                    foreach (MasterContentItem item in _ContentItems)
                        item.Modified = false;
                }

                if (_MenuItems != null)
                {
                    foreach (MasterMenuItem item in _MenuItems)
                        item.Modified = false;
                }

                if (_MarkupReference != null)
                    _MarkupReference.Modified = false;

                if (_CopyMarkupReference != null)
                    _CopyMarkupReference.Modified = false;

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor item in _OptionDescriptors)
                        item.Modified = false;
                }
            }
        }

        public bool HasContentType(string contentType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.ContentType == contentType);
                if (item != null)
                    return true;
            }

            return false;
        }

        public bool HasContentKey(string contentKey)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.KeyString == contentKey);
                if (item != null)
                    return true;
            }

            return false;
        }

        public MasterContentItem GetContentItem(string key)
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                {
                    if (item.KeyString == key)
                        return item;

                    MasterContentItem subItem = item.GetContentItem(key);

                    if (subItem != null)
                        return subItem;
                }
            }

            return null;
        }

        public MasterContentItem GetContentItemWithTypeAndSubType(string contentType, string contentSubType)
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                {
                    if ((item.ContentType == contentType) && (item.ContentSubType == contentSubType))
                        return item;

                    MasterContentItem subItem = item.GetContentItemTypeAndSubType(contentType, contentSubType);

                    if (subItem != null)
                        return subItem;
                }
            }

            return null;
        }

        public bool InsertContentItem(int index, MasterContentItem contentItem)
        {
            bool returnValue = false;

            if ((index >= 0) && (index < ContentItemCount()))
            {
                if (_ContentItems == null)
                {
                    _ContentItems = new List<MasterContentItem>() { contentItem };
                    index = 0;
                }
                else
                    _ContentItems.Insert(index, contentItem);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;

                returnValue = true;
            }
            else if (index == ContentItemCount())
            {
                AddContentItem(contentItem);
                returnValue = true;
            }

            return returnValue;
        }

        public void AddContentItem(MasterContentItem contentItem)
        {
            if (_ContentItems == null)
                _ContentItems = new List<MasterContentItem>() { contentItem };
            else
                _ContentItems.Add(contentItem);

            contentItem.Index = _ContentItems.Count() - 1;

            ModifiedFlag = true;
        }

        public void AddContentItemGrouped(MasterContentItem contentItem)
        {
            if (_ContentItems == null)
            {
                _ContentItems = new List<MasterContentItem>() { contentItem };
                contentItem.Index = _ContentItems.Count() - 1;
                ModifiedFlag = true;
            }
            else
            {
                int bestIndex = -1;
                int index = 0;

                foreach (MasterContentItem mci in _ContentItems)
                {
                    if (mci.ContentType == contentItem.ContentType)
                        bestIndex = Index;

                    index++;
                }

                if (bestIndex != -1)
                    InsertContentItem(bestIndex + 1, contentItem);
                else
                {
                    _ContentItems.Add(contentItem);
                    contentItem.Index = _ContentItems.Count() - 1;
                    ModifiedFlag = true;
                }
            }
        }

        public bool MoveContentItemUp(int index)
        {
            if ((index > 0) && (index < ContentItemCount()))
            {
                MasterContentItem contentItem = _ContentItems[index];
                _ContentItems.RemoveAt(index);
                index--;
                _ContentItems.Insert(index, contentItem);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool MoveContentItemDown(int index)
        {
            if ((index >= 0) && (index < ContentItemCount() - 1))
            {
                MasterContentItem contentItem = _ContentItems[index];
                _ContentItems.RemoveAt(index);
                _ContentItems.Insert(index + 1, contentItem);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool DeleteContentItemIndexed(int index)
        {
            if ((index >= 0) && (index < ContentItemCount()))
            {
                _ContentItems.RemoveAt(index);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllContentItems()
        {
            _ContentItems = null;
            ModifiedFlag = true;
        }

        public int ContentItemCount()
        {
            if (_ContentItems != null)
                return _ContentItems.Count();

            return 0;
        }

        public bool ReindexContentItems()
        {
            bool returnValue = false;

            if (_ContentItems == null)
                return returnValue;

            int index = 0;

            foreach (MasterContentItem contentItem in _ContentItems)
            {
                if (contentItem.Index != index)
                {
                    contentItem.Index = index;
                    ModifiedFlag = true;
                    returnValue = true;
                }

                contentItem.ReindexContentItems();

                index++;
            }

            return returnValue;
        }

        public bool HasMenuAction(string action)
        {
            if (_MenuItems != null)
            {
                MasterMenuItem item = _MenuItems.FirstOrDefault(x => x.Action == action);
                if (item != null)
                    return true;
            }

            return false;
        }

        public string GetMenuFirstAction()
        {
            if ((_MenuItems != null) && (_MenuItems.Count() != 0))
            {
                MasterMenuItem item = _MenuItems[0];
                if (item != null)
                    return item.Action;
            }

            return null;
        }

        public MasterMenuItem FindMenuItem(string nodeContentKey)
        {
            if ((_MenuItems != null) && (_MenuItems.Count() != 0))
            {
                MasterMenuItem item = _MenuItems.FirstOrDefault(x => x.KeyString == nodeContentKey);
                if (item != null)
                    return item;
            }

            return null;
        }

        public bool InsertMenuItem(int index, MasterMenuItem menuItem)
        {
            if ((index >= 0) && (index < MenuItemsCount()))
            {
                if (_MenuItems == null)
                    _MenuItems = new List<MasterMenuItem>() { menuItem };
                else
                    _MenuItems.Insert(index, menuItem);

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void AddMenuItem(MasterMenuItem menuItem)
        {
            if (_MenuItems == null)
                _MenuItems = new List<MasterMenuItem>() { menuItem };
            else
                _MenuItems.Add(menuItem);

            ModifiedFlag = true;
        }

        public bool MoveMenuItemUp(int index)
        {
            if ((index > 0) && (index < MenuItemsCount()))
            {
                MasterMenuItem menuItem = _MenuItems[index];
                _MenuItems.RemoveAt(index);
                _MenuItems.Insert(index - 1, menuItem);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool MoveMenuItemDown(int index)
        {
            if ((index >= 0) && (index < MenuItemsCount() - 1))
            {
                MasterMenuItem menuItem = _MenuItems[index];
                _MenuItems.RemoveAt(index);
                _MenuItems.Insert(index + 1, menuItem);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool DeleteMenuItemIndexed(int index)
        {
            if ((index >= 0) && (index < MenuItemsCount()))
            {
                _MenuItems.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllMenuItems()
        {
            _MenuItems = null;
            ModifiedFlag = true;
        }

        public int MenuItemsCount()
        {
            if (_MenuItems != null)
                return _MenuItems.Count();

            return 0;
        }


        public MarkupTemplateReference MarkupReference
        {
            get
            {
                return _MarkupReference;
            }
            set
            {
                if (_MarkupReference != value)
                {
                    _MarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string MarkupTemplateKey
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.KeyString;
                return null;
            }
            set
            {
                if (_MarkupReference != null)
                {
                    if (_MarkupReference.KeyString != value)
                    {
                        _MarkupReference.Key = value;
                        ModifiedFlag = true;
                    }
                }
                else if (value != null)
                {
                    _MarkupReference = new MarkupTemplateReference(value, null, null);
                }
            }
        }

        public bool IsLocalMarkupTemplate
        {
            get
            {
                return MarkupTemplateKey == "(local)";
            }
        }

        public MarkupTemplate MarkupTemplate
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.Item;

                return null;
            }
        }

        public MarkupTemplateReference CopyMarkupReference
        {
            get
            {
                return _CopyMarkupReference;
            }
            set
            {
                if (_CopyMarkupReference != value)
                {
                    _CopyMarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string CopyMarkupTemplateKey
        {
            get
            {
                if (_CopyMarkupReference != null)
                    return _CopyMarkupReference.KeyString;
                return null;
            }
            set
            {
                if (_CopyMarkupReference != null)
                {
                    if (_CopyMarkupReference.KeyString != value)
                    {
                        _CopyMarkupReference.Key = value;
                        ModifiedFlag = true;
                    }
                }
                else if (value != null)
                {
                    _CopyMarkupReference = new MarkupTemplateReference(value, null, null);
                }
            }
        }

        public MarkupTemplate CopyMarkupTemplate
        {
            get
            {
                if (_CopyMarkupReference != null)
                    return _CopyMarkupReference.Item;

                return null;
            }
        }

        public List<OptionDescriptor> OptionDescriptors
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

        public bool MoveOptionDescriptorUp(int index)
        {
            if ((index > 0) && (index < OptionDescriptorCount()))
            {
                OptionDescriptor optionDescriptors = _OptionDescriptors[index];
                _OptionDescriptors.RemoveAt(index);
                _OptionDescriptors.Insert(index - 1, optionDescriptors);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool MoveOptionDescriptorDown(int index)
        {
            if ((index >= 0) && (index < OptionDescriptorCount() - 1))
            {
                OptionDescriptor optionDescriptors = _OptionDescriptors[index];
                _OptionDescriptors.RemoveAt(index);
                _OptionDescriptors.Insert(index + 1, optionDescriptors);
                ModifiedFlag = true;
                return true;
            }

            return false;
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

        public void DeleteAllOptionDescriptors()
        {
            OptionDescriptors = null;
        }

        public int OptionDescriptorCount()
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.Count();

            return 0;
        }

        public bool HasOptionDescriptors()
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.Count() != 0;

            return false;
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

        public virtual List<IBaseObjectKeyed> GetOptionsFromDescriptors()
        {
            List<OptionDescriptor> optionDescriptors = OptionDescriptors;

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

        public bool MasterOptionsCheck(UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = BaseObjectNode.GetDefaultDescriptors();
            bool wasChanged = false;

            foreach (OptionDescriptor newOptionDescriptor in newOptionDescriptors)
            {
                OptionDescriptor oldOptionDescriptor = GetOptionDescriptor(newOptionDescriptor.KeyString);

                if (oldOptionDescriptor == null)
                {
                    int index = newOptionDescriptors.IndexOf(newOptionDescriptor);

                    if (_OptionDescriptors == null)
                        OptionDescriptors = new List<OptionDescriptor>() { newOptionDescriptor };
                    else
                        InsertOptionDescriptor(index, newOptionDescriptor);

                    wasChanged = true;
                }
                else if (oldOptionDescriptor.Type != newOptionDescriptor.Type)
                {
                    oldOptionDescriptor.Copy(newOptionDescriptor);
                    wasChanged = true;
                }
            }

            return wasChanged;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.ResolveReferences(mainRepository, false, recurseChildren);
            }

            if ((_MarkupReference != null) && (_MarkupReference.Key != null)
                    && !_MarkupReference.KeyString.StartsWith("(") && (_MarkupReference.Item == null))
                _MarkupReference.ResolveReference(mainRepository);

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.ResolveReference(mainRepository);
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseParents, recurseChildren);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.ClearReferences(false, recurseChildren);
            }

            if ((_MarkupReference != null))
                _MarkupReference.ClearReference();

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.ClearReference();
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.OnFixup(fixups);
            }

            if (_MarkupReference != null)
                _MarkupReference.OnFixup(fixups);

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.OnFixup(fixups);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_ContentItems != null)
            {
                XElement contentItemsElement = new XElement("ContentItems");
                foreach (MasterContentItem item in _ContentItems)
                    contentItemsElement.Add(item.Xml);
                element.Add(contentItemsElement);
            }
            if (_MenuItems != null)
            {
                XElement menuItemsElement = new XElement("MenuItems");
                foreach (MasterMenuItem item in _MenuItems)
                    menuItemsElement.Add(item.Xml);
                element.Add(menuItemsElement);
            }
            if ((_MarkupReference != null) &&
                    (!String.IsNullOrEmpty(_MarkupReference.KeyString) || !String.IsNullOrEmpty(_MarkupReference.Name)))
                element.Add(_MarkupReference.GetElement("MarkupReference"));
            if ((_CopyMarkupReference != null) &&
                    (!String.IsNullOrEmpty(_CopyMarkupReference.KeyString) || !String.IsNullOrEmpty(_CopyMarkupReference.Name)))
                element.Add(_CopyMarkupReference.GetElement("CopyMarkupReference"));
            if ((_OptionDescriptors != null) && (_OptionDescriptors.Count() != 0))
            {
                XElement optionDescriptorsElement = new XElement("OptionDescriptors");
                foreach (OptionDescriptor optionDescriptor in _OptionDescriptors)
                    optionDescriptorsElement.Add(optionDescriptor.Xml);
                element.Add(optionDescriptorsElement);
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "ContentItems":
                    {
                        List<MasterContentItem> contentItems = new List<MasterContentItem>();
                        foreach (XElement grandChildElement in childElement.Elements())
                            contentItems.Add(new MasterContentItem(grandChildElement));
                        ContentItems = contentItems;
                    }
                    break;
                case "MenuItems":
                    {
                        List<MasterMenuItem> menuItems = new List<MasterMenuItem>();
                        foreach (XElement grandChildElement in childElement.Elements())
                            menuItems.Add(new MasterMenuItem(grandChildElement));
                        MenuItems = menuItems;
                    }
                    break;
                case "MarkupReference":
                    _MarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "CopyMarkupReference":
                    _CopyMarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "OptionDescriptors":
                    foreach (XElement optionElement in childElement.Elements())
                    {
                        OptionDescriptor optionDescriptor = new OptionDescriptor(optionElement);
                        AddOptionDescriptor(optionDescriptor);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            if (ContentItems != null)
            {
                foreach (MasterContentItem contentItem in ContentItems)
                {
                    if (!contentItem.IsLocalMarkupTemplate && (contentItem.MarkupTemplate != null))
                        AddUniqueReference(references, contentItem.MarkupTemplate);
                    else if (contentItem.CopyMarkupTemplate != null)
                        AddUniqueReference(references, contentItem.CopyMarkupTemplate);
                }
            }

            if ((_MarkupReference != null) && (_MarkupReference.Item != null) &&
                (_MarkupReference.KeyString != "(local)") && (_MarkupReference.KeyString != "(none)"))
            {
                MarkupTemplate markupTemplate = _MarkupReference.Item;
                AddUniqueReference(references, markupTemplate);

                markupTemplate.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                    null, null, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
            }

            if ((_CopyMarkupReference != null) && (_CopyMarkupReference.Item != null) &&
                (_CopyMarkupReference.KeyString != "(local)") && (_CopyMarkupReference.KeyString != "(none)"))
            {
                MarkupTemplate copyMarkupTemplate = _CopyMarkupReference.Item;
                AddUniqueReference(references, copyMarkupTemplate);

                copyMarkupTemplate.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                    null, null, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            NodeMaster otherNodeMaster = other as NodeMaster;

            if (otherNodeMaster == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = MasterContentItem.CompareMasterContentItemLists(_ContentItems, otherNodeMaster.ContentItems);

            if (diff != 0)
                return diff;

            diff = MasterMenuItem.CompareMasterMenuItemLists(_MenuItems, otherNodeMaster.MenuItems);

            if (diff != 0)
                return diff;

            diff = OptionDescriptor.CompareOptionDescriptors(_OptionDescriptors, otherNodeMaster.OptionDescriptors);

            if (diff != 0)
                return diff;

            return 0;
        }

        public static int CompareNodeMaster(NodeMaster item1, NodeMaster item2)
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
    }
}

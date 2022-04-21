using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class InflectorFilter : Designator
    {
        public List<InflectorFilterItem> ItemsList;
        public Dictionary<string, InflectorFilterItem> ItemsDictionary;

        public InflectorFilter(
                string name,
                List<Classifier> classifications,
                List<InflectorFilterItem> itemsList) :
            base(name, classifications)
        {
            ItemsList = itemsList;
            SyncDictionary();
        }

        public InflectorFilter(XElement element)
        {
            ClearInflectorFilter();
            OnElement(element);
            DefaultLabelCheck();
        }

        public InflectorFilter(InflectorFilter other) :
            base(other)
        {
            CopyInflectorFilter(other);
        }

        public InflectorFilter()
        {
            ClearInflectorFilter();
        }

        public void ClearInflectorFilter()
        {
            ItemsList = null;
        }

        public void CopyInflectorFilter(InflectorFilter other)
        {
            ItemsList = other.CloneItemsList();
            SyncDictionary();
        }

        public List<InflectorFilterItem> CloneItemsList()
        {
            if (ItemsList == null)
                return null;

            List<InflectorFilterItem> newList = new List<InflectorFilterItem>(ItemsList.Count());

            foreach (InflectorFilterItem item in ItemsList)
                newList.Add(new InflectorFilterItem(item));

            return newList;
        }

        public int InflectorFilterItemCount()
        {
            if (ItemsList == null)
                return 0;

            return ItemsList.Count();
        }

        public bool HasInflectorFilterItem(string name)
        {
            InflectorFilterItem item = null;

            if (String.IsNullOrEmpty(name) || (ItemsDictionary == null))
                return false;

            if (!ItemsDictionary.TryGetValue(name, out item))
                return false;

            return true;
        }

        public InflectorFilterItem GetInflectorFilterItem(string name)
        {
            InflectorFilterItem item = null;

            if (String.IsNullOrEmpty(name) || (ItemsDictionary == null))
                return item;

            ItemsDictionary.TryGetValue(name, out item);

            return item;
        }

        public InflectorFilterItem GetInflectorFilterItemIndexed(int index)
        {
            if ((ItemsList == null) || (index < 0) || (index >= ItemsList.Count()))
                return null;

            InflectorFilterItem item = ItemsList[index];

            return item;
        }

        public void AppendInflectorFilterItem(InflectorFilterItem item)
        {
            if (ItemsList != null)
                ItemsList.Add(item);
            else
                ItemsList = new List<InflectorFilterItem>() { item };

            AddInflectorFilterItemToDictionary(item);
        }

        public void AppendInflectorFilterItems(List<InflectorFilterItem> items)
        {
            if (items == null)
                return;

            foreach (InflectorFilterItem item in items)
                AppendInflectorFilterItem(item);
        }

        public bool DeleteInflectorFilterItem(InflectorFilterItem item)
        {
            if ((item == null) || (ItemsList == null))
                return false;

            RemoveInflectorFilterItemFromDictionary(item);
            bool returnValue = ItemsList.Remove(item);

            return returnValue;
        }

        public bool DeleteInflectorFilterItemIndexed(int index)
        {
            if ((ItemsList == null) || (index < 0) || (index >= ItemsList.Count()))
                return false;

            RemoveInflectorFilterItemFromDictionary(ItemsList[index]);
            ItemsList.RemoveAt(index);

            return true;
        }

        protected void SyncDictionary()
        {
            ItemsDictionary = new Dictionary<string, InflectorFilterItem>();

            if (ItemsList == null)
                return;

            LoadInflectorFilterItemsDictionary(ItemsList);
        }

        protected void LoadInflectorFilterItemsDictionary(List<InflectorFilterItem> items)
        {
            foreach (InflectorFilterItem item in ItemsList)
                AddInflectorFilterItemToDictionary(item);
        }

        protected void AddInflectorFilterItemToDictionary(InflectorFilterItem item)
        {
            if (item != null)
            {
                try
                {
                    if (ItemsDictionary == null)
                        ItemsDictionary = new Dictionary<string, InflectorFilterItem>();

                    LiteralString input = item.Input;
                    InflectorFilterItem tmp;

                    foreach (String s in input.Strings)
                    {
                        if (!ItemsDictionary.TryGetValue(s, out tmp))
                            ItemsDictionary.Add(s, item);
                    }
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorFilter.AddInflectorFilterItemToDictionary duplicate entry: " + item.KeyString);
                }
            }
        }

        protected void RemoveInflectorFilterItemFromDictionary(InflectorFilterItem item)
        {
            if ((item != null) && (ItemsDictionary != null))
                ItemsDictionary.Remove(item.KeyString);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(Name))
                element.Add(new XAttribute("Name", Name));

            if (ItemsList != null)
            {
                foreach (InflectorFilterItem item in ItemsList)
                {
                    XElement itemElement = item.GetElement(item.ItemType);
                    element.Add(itemElement);
                }
            }
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value;

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Key = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Redirection":
                case "Disallow":
                    {
                        InflectorFilterItem item = new InflectorFilterItem(childElement);
                        AppendInflectorFilterItem(item);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

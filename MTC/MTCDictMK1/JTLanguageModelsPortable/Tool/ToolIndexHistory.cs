using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Tool
{
    public class ToolIndexHistory : BaseObjectKeyed
    {
        protected readonly int _DefaultHistorySize = 100;
        protected int _HistorySize;
        protected int _HistoryIndex;
        protected List<int> _History;

        public ToolIndexHistory(int size)
        {
            _HistorySize = size;
            _HistoryIndex = 0;
            _History = new List<int>(_HistorySize);
        }

        public ToolIndexHistory(ToolIndexHistory other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolIndexHistory(XElement element)
        {
            OnElement(element);
        }

        public ToolIndexHistory()
        {
            ClearIndexHistory();
        }

        public override void Clear()
        {
            base.Clear();
            ClearIndexHistory();
        }

        public void ClearIndexHistory()
        {
            _HistorySize = _DefaultHistorySize;
            _HistoryIndex = 0;
            _History = new List<int>(_HistorySize);
        }

        public void Copy(ToolIndexHistory other)
        {
            _HistorySize = other.HistorySize;
            _HistoryIndex = 0;
            _History = new List<int>(_HistorySize);
            CopyHistory(other);
        }

        public override IBaseObject Clone()
        {
            return new ToolIndexHistory(this);
        }

        public int HistorySize
        {
            get
            {
                return _HistorySize;
            }
        }

        public int HistoryCount
        {
            get
            {
                return _History.Count();
            }
        }

        public int HistoryIndex
        {
            get
            {
                return _HistoryIndex;
            }
        }

        // Get the index of a status item, where 0 was the last item used, 1 is the next-to-last, etc.
        // Returns -1 if history index exceeds the maximum history index.
        public int GetHistoryItemIndex(int historyIndex)
        {
            int index;

            if (historyIndex >= _History.Count())
                return -1;

            index = _HistoryIndex - (historyIndex + 1);

            if (index < 0)
                index += _HistorySize;

            if (index > _History.Count())
                return -1;

            int itemIndex = _History[index];

            return itemIndex;
        }

        // Set history item index.
        public void SetHistoryItemIndex(int itemIndex, int value)
        {
            if (_History.Count() < _HistorySize)
                _History[itemIndex] = value;
            else
            {
                if (_HistoryIndex == _HistorySize)
                    _HistoryIndex = 0;

                _History[_HistoryIndex++] = itemIndex;
            }
            ModifiedFlag = true;
        }

        // Add history item index.
        public void AddHistoryItemIndex(int itemIndex)
        {
            if (_History.Count() < _HistorySize)
            {
                _History.Add(itemIndex);
                _HistoryIndex = _History.Count();
            }
            else
            {
                if (_HistoryIndex == _HistorySize)
                    _HistoryIndex = 0;

                _History[_HistoryIndex++] = itemIndex;
            }
            ModifiedFlag = true;
        }

        // Copy history.
        public void CopyHistory(ToolIndexHistory other)
        {
            int count = other.HistoryCount;

            for (int index = count - 1; index >= 0; index--)
            {
                int itemIndex = other.GetHistoryItemIndex(index);

                if (itemIndex == -1)
                    continue;

                AddHistoryItemIndex(itemIndex);
            }
        }

        // Clear history.
        public void ClearHistory()
        {
            if ((_History.Count() != 0) || (_HistoryIndex != 0))
                ModifiedFlag = true;

            _History.Clear();
            _HistoryIndex = 0;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Size", _HistorySize));
            element.Add(new XAttribute("Index", _HistoryIndex));
            if (_History != null)
            {
                foreach (int index in _History)
                    element.Add(new XElement("Value", index.ToString()));
            }
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "Size":
                    _HistorySize = Convert.ToInt32(attributeValue);
                    if (_History != null)
                    {
                        _History.Clear();
                        _History.Capacity = _HistorySize;
                    }
                    else
                        _History = new List<int>(_HistorySize);
                    break;
                case "Index":
                    _HistoryIndex = Convert.ToInt32(attributeValue);
                    break;
                default:
                    returnValue = base.OnAttribute(attribute);
                    break;
            }

            return returnValue;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Value":
                    _History.Add(Convert.ToInt32(childElement.Value));
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

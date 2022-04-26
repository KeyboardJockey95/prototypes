using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class StringMapper : BaseObjectKeyed
    {
        protected List<string> _StringList;
        protected Dictionary<string, int> _KeyMap;

        public StringMapper(string name) : base(name)
        {
            ClearStringMapper();
        }

        public StringMapper(StringMapper other) : base(other)
        {
            CopyStringMapper(other);
        }

        public StringMapper(XElement element)
        {
            ClearStringMapper();
            OnElement(element);
        }

        public StringMapper()
        {
            ClearStringMapper();
        }

        public override void Clear()
        {
            base.Clear();
            ClearStringMapper();
        }

        public void ClearStringMapper()
        {
            _StringList = new List<string>();
            _KeyMap = new Dictionary<string, int>();
            Modified = false;
        }

        public virtual void CopyStringMapper(StringMapper other)
        {
            _StringList = new List<string>(other.StringList);
            _KeyMap = new Dictionary<string, int>(other.KeyMap);
            Modified = false;
        }

        public override IBaseObject Clone()
        {
            return new StringMapper(this);
        }

        public List<string> StringList
        {
            get
            {
                return _StringList;
            }
            set
            {
                if (ObjectUtilities.CompareStringLists(value, _StringList) != 0)
                {
                    _StringList = value;
                    if (_StringList == null)
                        _StringList = new List<string>();
                    LoadKeyMap();
                    ModifiedFlag = true;
                }
            }
        }

        public Dictionary<string, int> KeyMap
        {
            get
            {
                return _KeyMap;
            }
        }

        protected void LoadKeyMap()
        {
            _KeyMap.Clear();
            int count = _StringList.Count();

            for (int index = 0; index < count; index++)
                _KeyMap.Add(_StringList[index], index + 1);
        }

        public bool Contains(int sourceID)
        {
            if (sourceID <= 0)
                return false;
            else if (sourceID > _StringList.Count())
                return false;
            else if (String.IsNullOrEmpty(_StringList[sourceID]))
                return false;

            return true;
        }

        public override string ToString()
        {
            return Format(0, 4);
        }

        public string Format(int indentCount, int indentSize)
        {
            StringBuilder sb = new StringBuilder();
            string indent = TextUtilities.GetSpaces(indentCount * indentSize);
            string subIndent = TextUtilities.GetSpaces((indentCount + 1) * indentSize);
            sb.AppendLine(indent + Name);
            sb.AppendLine(indent + "{");

            int count = _StringList.Count();
            int index;

            for (index = 0; index < count; index++)
            {
                int id = index + 1;
                string key = _StringList[index];

                if (key == null)
                    key = "null";
                else
                    key = "\"" + key + "\"";

                sb.AppendLine(subIndent + key);
            }

            sb.AppendLine(indent + "}");

            return sb.ToString();
        }

        public string GetByID(int id)
        {
            if ((id > 0) && (id <= _StringList.Count()))
                return _StringList[id - 1];
            return null;
        }

        public string GetByIDList(List<int> ids)
        {
            if (ids == null)
                return String.Empty;

            string str = String.Empty;

            foreach (int id in ids)
            {
                string name = GetByID(id);

                if (name == null)
                    continue;

                if (!String.IsNullOrEmpty(str))
                    str += ",";

                str += name;
            }

            return str;
        }

        public string GetByIDListWithExclusion(List<int> ids, List<int> excludeIDs)
        {
            if (ids == null)
                return String.Empty;

            string str = String.Empty;

            foreach (int id in ids)
            {
                if (excludeIDs.Contains(id))
                    continue;

                string name = GetByID(id);

                if (name == null)
                    continue;

                if (!String.IsNullOrEmpty(str))
                    str += ",";

                str += name;
            }

            return str;
        }

        public int GetID(string str)
        {
            int id;

            if (_KeyMap.TryGetValue(str, out id))
                return id;

            return -1;
        }

        public virtual int GetOrAdd(string str)
        {
            return GetOrAddNoSave(str);
        }

        public virtual int GetOrAddNoSave(string str)
        {
            int id;

            if (_KeyMap.TryGetValue(str, out id))
                return id;

            id = Add(str);

            return id;
        }

        public virtual int Add(string str)
        {
            return AddNoSave(str);
        }

        public virtual int AddNoSave(string str)
        {
            if (str == null)
                return 0;

            int id;

            if (_KeyMap.TryGetValue(str, out id))
                return id;

            _StringList.Add(str);
            id = _StringList.Count();
            _KeyMap.Add(str, id);
            ModifiedFlag = true;

            return id;
        }

        public virtual void AddList(List<string> list)
        {
            if (list == null)
                return;

            foreach (string str in list)
                Add(str);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            foreach (string str in _StringList)
                element.Add(new XElement("S", str));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "S":
                    AddNoSave(childElement.Value);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static int Compare(StringMapper object1, StringMapper object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareStringLists(object1.StringList, object2.StringList);
        }
    }
}

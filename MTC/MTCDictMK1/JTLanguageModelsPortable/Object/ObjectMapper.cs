using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class ObjectMapper<T> : BaseObjectKeyed where T : class, IBaseObjectKeyed, new()
    {
        protected List<T> _ObjectList;
        protected Dictionary<string, int> _KeyMap;

        public ObjectMapper(string name) : base(name)
        {
            ClearObjectMapper();
        }

        public ObjectMapper(ObjectMapper<T> other)
        {
            CopyObjectMapper(other);
        }

        public ObjectMapper(XElement element)
        {
            OnElement(element);
        }

        public ObjectMapper()
        {
            ClearObjectMapper();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectMapper();
        }

        public void ClearObjectMapper()
        {
            _ObjectList = new List<T>();
            _KeyMap = new Dictionary<string, int>();
        }

        public virtual void CopyObjectMapper(ObjectMapper<T> other)
        {
            _ObjectList = new List<T>(other.ObjectList);
            _KeyMap = new Dictionary<string, int>(other.KeyMap);
        }

        public List<T> ObjectList
        {
            get
            {
                return _ObjectList;
            }
            set
            {
                if (ObjectUtilities.CompareTypedObjectLists<T>(value, _ObjectList) != 0)
                {
                    _ObjectList = value;
                    if (_ObjectList == null)
                        _ObjectList = new List<T>();
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
            int count = _ObjectList.Count();

            for (int index = 0; index < count; index++)
                _KeyMap.Add(_ObjectList[index].KeyString, index + 1);
        }

        public T GetByID(int id)
        {
            if ((id > 0) && (id <= _ObjectList.Count()))
                return _ObjectList[id - 1];
            return null;
        }

        public T GetByKey(string key)
        {
            int id;

            if (!_KeyMap.TryGetValue(key, out id))
                return null;

            return _ObjectList[id - 1];
        }

        public virtual int Add(T obj)
        {
            if (obj == null)
                return 0;

            string key = obj.KeyString;
            int id;

            if (_KeyMap.TryGetValue(key, out id))
                return id;

            _ObjectList.Add(obj);
            id = _ObjectList.Count();
            _KeyMap.Add(key, id);

            return id;
        }

        public virtual void AddList(List<T> list)
        {
            if (list == null)
                return;

            foreach (T obj in list)
                Add(obj);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            foreach (T obj in _ObjectList)
                element.Add(obj.GetElement("Obj"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Obj":
                    {
                        T obj = new T();
                        obj.OnElement(childElement);
                        Add(obj);
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}

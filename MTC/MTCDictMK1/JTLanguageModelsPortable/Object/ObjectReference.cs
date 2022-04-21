using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Object
{
    public class ObjectReference<T> : BaseObjectKeyed where T : class, IBaseObjectKeyed, new()
    {
        protected string _Source;
        protected T _Item;

        public ObjectReference(object key, string source, T item)
            : base(key)
        {
            _Source = source;
            _Item = item;
        }

        public ObjectReference(ObjectReference<T> other)
            : base(other)
        {
            _Source = other.Source;
            _Item = other.Item;
        }

        public ObjectReference(XElement element)
        {
            OnElement(element);
        }

        public ObjectReference()
        {
            ClearObjectReference();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectReference();
        }

        public void ClearObjectReference()
        {
            _Item = null;
        }

        public void CopyObjectReference(ObjectReference<T> other)
        {
            base.Copy(other);
            _Source = other.Source;
            _Item = other.Item;
        }

        public void CopyReference(ObjectReference<T> other)
        {
            _Source = other.Source;
            _Item = other.Item;
        }

        public void UpdateReference(T other)
        {
            Copy(other);
            _Item = other;
        }

        public void Copy(ObjectReference<T> other)
        {
            if (other == null)
            {
                ClearBaseObjectKeyed();
                ClearObjectReference();
                return;
            }

            CopyObjectReference(other);
            base.Copy(other);
        }

        public void CopyDeep(ObjectReference<T> other)
        {
            _Source = other.Source;
            _Item = other.Item;
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new ObjectReference<T>(this);
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

        public virtual T Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;

                if (value != null)
                    Key = value.Key;
            }
        }

        public void ResolveReference(IMainRepository mainRepository)
        {
            if (_Item == null)
                _Item = (T)mainRepository.ResolveReference(_Source, null, Key);
        }

        public void ResolveReference(IMainRepository mainRepository, LanguageID languageID)
        {
            if (_Item == null)
                _Item = (T)mainRepository.ResolveReference(_Source, languageID, Key);
        }

        public bool SaveReference(IMainRepository mainRepository)
        {
            if (_Item != null)
                return mainRepository.SaveReference(_Source, null, _Item);
            return true;
        }

        public bool UpdateReference(IMainRepository mainRepository)
        {
            if (_Item != null)
            {
                _Item.TouchAndClearModified();
                return mainRepository.UpdateReference(_Source, null, _Item);
            }
            return true;
        }

        public bool UpdateReferenceCheck(IMainRepository mainRepository)
        {
            if ((_Item != null) && _Item.Modified)
            {
                _Item.TouchAndClearModified();
                return mainRepository.UpdateReference(_Source, null, _Item);
            }
            return true;
        }

        public bool SaveReference(IMainRepository mainRepository, LanguageID languageID)
        {
            if (_Item != null)
                return mainRepository.SaveReference(_Source, languageID, _Item);
            return true;
        }

        public void ClearReference()
        {
            _Item = null;
        }

        public virtual void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_Item == null)
                _Item = (T)mainRepository.ResolveReference(_Source, null, Key);
        }

        public virtual bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            if (_Item != null)
            {
                returnValue = mainRepository.SaveReference(_Source, null, _Item);
                if (returnValue)
                    Key = _Item.Key;
            }
            return returnValue;
        }

        public virtual bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            if (_Item != null)
            {
                returnValue = mainRepository.UpdateReference(_Source, null, _Item);
                if (returnValue)
                    Key = _Item.Key;
            }
            return returnValue;
        }

        public virtual bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            if ((_Item != null) && _Item.Modified)
            {
                returnValue = mainRepository.UpdateReference(_Source, null, _Item);
                if (returnValue)
                    Key = _Item.Key;
            }
            return returnValue;
        }

        public virtual void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            _Item = null;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(_Source))
                element.Add(new XAttribute("Source", _Source));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Source":
                    _Source = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            string xmlKey = KeyString;

            if (!String.IsNullOrEmpty(xmlKey) && !String.IsNullOrEmpty(_Source))
            {
                IBaseObjectKeyed target = fixups.Get(_Source, xmlKey);

                if (target != null)
                {
                    Key = target.Key;
                    _Item = (T)target;
                }
                else
                    ResolveReference(fixups.Repositories);
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ObjectReference<T> otherObjectReference = other as ObjectReference<T>;

            if (otherObjectReference == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            return diff;
        }

        public static int Compare(ObjectReference<T> object1, ObjectReference<T> object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }
    }
}

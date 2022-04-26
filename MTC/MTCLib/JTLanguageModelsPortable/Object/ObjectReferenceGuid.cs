using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class ObjectReferenceGuid<T> : ObjectReference<T> where T : class, IBaseObjectKeyed, new()
    {
        protected Guid _Guid;

        public ObjectReferenceGuid(object key, string source, T item)
            : base(key, source, item)
        {
            if (item != null)
                _Guid = item.Guid;
            else
                _Guid = Guid.Empty;
        }

        public ObjectReferenceGuid(object key, string source, Guid guid, T item)
            : base(key, source, item)
        {
            _Guid = guid;
        }

        public ObjectReferenceGuid(ObjectReferenceGuid<T> other)
            : base(other)
        {
            _Guid = other.Guid;
        }

        public ObjectReferenceGuid(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceGuid()
        {
            ClearObjectReferenceGuid();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectReferenceGuid();
        }

        public void ClearObjectReferenceGuid()
        {
            _Guid = Guid.Empty;
        }

        public override IBaseObject Clone()
        {
            return new ObjectReferenceGuid<T>(this);
        }

        public override T Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;

                if (value != null)
                {
                    Key = value.Key;
                    Guid = value.Guid;
                }
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
                if (_Guid != value)
                {
                    _Guid = value;
                    ModifiedFlag = true;
                }
            }
        }

        public new void ResolveReference(IMainRepository mainRepository)
        {
            if (_Item == null)
            {
                if (/*InImport &&*/ (_Guid != Guid.Empty))
                    _Item = (T)mainRepository.ResolveGuidReference(_Source, null, _Guid);
                else if (!KeyString.StartsWith("("))
                {
                    _Item = (T)mainRepository.ResolveReference(_Source, null, Key);

                    if ((_Item == null) && (_Guid != Guid.Empty))
                        _Item = (T)mainRepository.ResolveGuidReference(_Source, null, _Guid);
                }

                if (_Item != null)
                {
                    Key = _Item.Key;
                    Guid = _Item.Guid;
                }
            }
        }

        public new void ResolveReference(IMainRepository mainRepository, LanguageID languageID)
        {
            if (_Item == null)
            {
                if (/*InImport &&*/ (_Guid != Guid.Empty))
                    _Item = (T)mainRepository.ResolveGuidReference(_Source, languageID, _Guid);
                else if (!KeyString.StartsWith("("))
                {
                    _Item = (T)mainRepository.ResolveReference(_Source, languageID, Key);

                    if ((_Item == null) && (_Guid != Guid.Empty))
                        _Item = (T)mainRepository.ResolveGuidReference(_Source, languageID, _Guid);
                }

                if (_Item != null)
                {
                    Key = _Item.Key;
                    Guid = _Item.Guid;
                }
            }
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            ResolveReference(mainRepository);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.SaveReference(mainRepository);
            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReference(mainRepository);
            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReferenceCheck(mainRepository);
            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            _Item = null;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((_Guid != Guid.Empty) && (_Item != null))
                _Guid = _Item.Guid;
            if (_Guid != Guid.Empty)
                element.Add(new XAttribute("Guid", _Guid));
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
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ObjectReferenceGuid<T> otherObjectReferenceGuid = other as ObjectReferenceGuid<T>;

            if (otherObjectReferenceGuid == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = _Guid.CompareTo(otherObjectReferenceGuid.Guid);
            return diff;
        }

        public static int Compare(ObjectReferenceGuid<T> object1, ObjectReferenceGuid<T> object2)
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

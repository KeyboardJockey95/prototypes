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
    public class ObjectReferenceNamed<T> : ObjectReference<T> where T : class, IBaseObjectKeyed, new()
    {
        protected string _Name;
        protected string _Owner;

        public ObjectReferenceNamed(object key, string source, T item)
            : base(key, source, item)
        {
            if (item != null)
            {
                _Name = item.Name;
                _Owner = item.Owner;
            }
            else
            {
                _Name = null;
                _Owner = null;
            }
        }

        public ObjectReferenceNamed(object key, string source, string name, string owner, T item)
            : base(key, source, item)
        {
            _Name = name;
            _Owner = owner;
        }

        public ObjectReferenceNamed(ObjectReferenceNamed<T> other)
            : base(other)
        {
            _Name = other.Name;
            _Owner = other.Owner;
        }

        public ObjectReferenceNamed(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceNamed()
        {
            ClearObjectReferenceNamed();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectReferenceNamed();
        }

        public void ClearObjectReferenceNamed()
        {
            _Name = null;
            _Owner = null;
        }

        public override IBaseObject Clone()
        {
            return new ObjectReferenceNamed<T>(this);
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
                    Name = value.Name;
                    Owner = value.Owner;
                }
            }
        }

        public override string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public new void ResolveReference(IMainRepository mainRepository)
        {
            if (_Item == null)
            {
                if (/*InImport &&*/ !String.IsNullOrEmpty(_Name))
                    _Item = (T)mainRepository.ResolveNamedReference(_Source, null, _Owner, _Name);
                else if (!KeyString.StartsWith("("))
                {
                    _Item = (T)mainRepository.ResolveReference(_Source, null, Key);

                    if ((_Item == null) && !String.IsNullOrEmpty(_Name))
                        _Item = (T)mainRepository.ResolveNamedReference(_Source, null, _Owner, _Name);
                }

                if (_Item != null)
                {
                    Key = _Item.Key;
                    Name = _Item.Name;
                    Owner = _Item.Owner;
                }
            }
        }

        public new void ResolveReference(IMainRepository mainRepository, LanguageID languageID)
        {
            if (_Item == null)
            {
                if (/*InImport &&*/ !String.IsNullOrEmpty(_Name))
                    _Item = (T)mainRepository.ResolveNamedReference(_Source, languageID, _Owner, _Name);
                else if (!KeyString.StartsWith("("))
                {
                    _Item = (T)mainRepository.ResolveReference(_Source, languageID, Key);

                    if ((_Item == null) && !String.IsNullOrEmpty(_Name))
                        _Item = (T)mainRepository.ResolveNamedReference(_Source, languageID, _Owner, _Name);
                }

                if (_Item != null)
                {
                    Key = _Item.Key;
                    Name = _Item.Name;
                    Owner = _Item.Owner;
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
            if (String.IsNullOrEmpty(_Name) && (_Item != null))
                _Name = _Item.Name;
            if (String.IsNullOrEmpty(_Owner) && (_Item != null))
                _Owner = _Item.Owner;
            if (_Name != null)
                element.Add(new XAttribute("Name", _Name));
            if (_Owner != null)
                element.Add(new XAttribute("Owner", _Owner));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    _Name = attributeValue;
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ObjectReferenceNamed<T> otherObjectReferenceNamed = other as ObjectReferenceNamed<T>;

            if (otherObjectReferenceNamed == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_Name, otherObjectReferenceNamed.Name);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_Owner, otherObjectReferenceNamed.Owner);

            if (diff != 0)
                return diff;
            return 0;
        }

        public static int Compare(ObjectReferenceNamed<T> object1, ObjectReferenceNamed<T> object2)
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

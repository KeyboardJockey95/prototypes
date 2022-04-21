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
    public class ObjectReferenceTitled : BaseObjectTitled
    {
        protected BaseObjectTitled _Item;

        public ObjectReferenceTitled(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            _Item = null;
        }

        public ObjectReferenceTitled(object key, string source)
            : base(key)
        {
            _Source = source;
            _Item = null;
        }

        public ObjectReferenceTitled(string source, BaseObjectTitled item)
            : base(item)
        {
            _Source = source;
            _Item = item;
        }

        public ObjectReferenceTitled(string source, object key, BaseObjectTitled item)
            : base(item, key)
        {
            _Source = source;
            _Item = item;
        }

        public ObjectReferenceTitled(ObjectReferenceTitled other, object key, string source)
            : base(other, key)
        {
            CopyReference(other);
            ModifiedFlag = false;
        }

        public ObjectReferenceTitled(ObjectReferenceTitled other)
            : base(other, other.Key)
        {
            CopyReference(other);
            ModifiedFlag = false;
        }

        public ObjectReferenceTitled(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceTitled()
        {
            ClearObjectReferenceTitled();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectReferenceTitled();
        }

        public void ClearObjectReferenceTitled()
        {
            _Source = null;
            _Item = null;
        }

        public void CopyObjectReferenceTitled(ObjectReferenceTitled other)
        {
            base.Copy(other);
            _Source = other.Source;
            _Item = other.Item;
        }

        public void CopyReference(ObjectReferenceTitled other)
        {
            _Source = other.Source;
            _Item = other.Item;
        }

        public void UpdateReference(BaseObjectTitled other)
        {
            CopyTitledObjectAndLanguages(other);
            _Item = other;
        }

        public void Copy(ObjectReferenceTitled other)
        {
            if (other == null)
            {
                ClearBaseObjectTitled();
                ClearObjectReferenceTitled();
                return;
            }

            CopyObjectReferenceTitled(other);
            base.Copy(other);
        }

        public void CopyDeep(ObjectReferenceTitled other)
        {
            _Source = other.Source;
            _Item = other.Item;
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new ObjectReferenceTitled(this);
        }

        public bool HasItem()
        {
            return _Item != null;
        }

        public BaseObjectTitled Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;
            }
        }

        public T TypedItem<T>() where T : BaseObjectTitled
        {
            return (T)_Item;
        }

        public T TypedItemAs<T>() where T : BaseObjectTitled
        {
            return _Item as T;
        }

        public void ResolveReference(IMainRepository mainRepository)
        {
            if (_Item == null)
                _Item = (BaseObjectTitled)mainRepository.ResolveReference(_Source, null, Key);
        }

        public void ResolveReference(IMainRepository mainRepository, LanguageID languageID)
        {
            if (_Item == null)
                _Item = (BaseObjectTitled)mainRepository.ResolveReference(_Source, languageID, Key);
        }

        public bool SaveReference(IMainRepository mainRepository)
        {
            if (_Item != null)
                return mainRepository.SaveReference(_Source, null, _Item);
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

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_Item == null)
                _Item = (BaseObjectTitled)mainRepository.ResolveReference(_Source, null, Key);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
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

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
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

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
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

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            _Item = null;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            return element;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ObjectReferenceTitled otherObjectReferenceTitled = other as ObjectReferenceTitled;

            if (otherObjectReferenceTitled == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            return diff;
        }

        public static int Compare(ObjectReferenceTitled object1, ObjectReferenceTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareLists(List<ObjectReferenceTitled> object1, List<ObjectReferenceTitled> object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            int count1 = object1.Count();
            int count2 = object2.Count();
            if (count1 != count2)
                return count1 - count2;
            int index;
            for (index = 0; index < count1; index++)
            {
                int diff = Compare(object1[index], object2[index]);
                if (diff != 0)
                    return diff;
            }
            return 0;
        }

        public static int CompareKeys(ObjectReferenceTitled object1, ObjectReferenceTitled object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareObjectReferenceTitledLists(List<ObjectReferenceTitled> object1, List<ObjectReferenceTitled> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ObjectReferenceTitled>(object1, object2);
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            string xmlKey = KeyString;

            if (!String.IsNullOrEmpty(xmlKey) && !String.IsNullOrEmpty(_Source))
            {
                IBaseObjectKeyed target = fixups.Get(_Source, xmlKey);

                if (target != null)
                {
                    Key = target.Key;
                    _Item = (BaseObjectTitled)target;
                }
            }
        }
    }
}

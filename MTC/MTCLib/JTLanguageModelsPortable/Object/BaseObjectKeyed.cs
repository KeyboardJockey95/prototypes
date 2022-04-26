using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObjectKeyed : BaseObject, IBaseObjectKeyed
    {
        protected Type _KeyType;
        protected object _Key;
        protected DateTime _CreationTime;
        protected DateTime _ModifiedTime;
        private int _ObjectFlags;

        // Flag definitions for _ObjectFlags.

        // Persitent flags.

        // For use by derived class.  Try to confine to leaves.
        protected const int ObjectFlagPersistentChild1  = 0x00000001;

        // For use by derived class.  Try to confine to leaves.
        protected const int ObjectFlagPersistentChild2  = 0x00000002;

        // Flag space for expansion.

        // Persistent flag mask.
        protected const int ObjectMaskPersistent        = 0x0000ffff;

        // non-ersistent flags.

        // Object has been modified.  For use in determining the need to update the object store.
        protected const int ObjectFlagModified          = 0x00010000;

        // For temporary use in walking objects.
        protected const int ObjectFlagMarked            = 0x00020000;


        public BaseObjectKeyed(object key)
        {
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
            _Key = key;
            _ObjectFlags = 0;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public BaseObjectKeyed(IBaseObjectKeyed other)
        {
            Copy(other);
            _ObjectFlags = 0;
        }

        public BaseObjectKeyed()
        {
            ClearBaseObjectKeyed();
        }

        public void Copy(IBaseObjectKeyed other)
        {
            if (other != null)
                _Key = other.Key;
            else
                _Key = null;
            if (_Key != null)
                _KeyType = _Key.GetType();
            else
                _KeyType = null;
            if (other != null)
            {
                _CreationTime = other.CreationTime;
                _ModifiedTime = other.ModifiedTime;
            }
            else
            {
                _CreationTime = DateTime.MinValue;
                _ModifiedTime = DateTime.MinValue;
            }
        }

        public void CopyDeep(IBaseObjectKeyed other)
        {
            Copy(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectKeyed();
        }

        public void ClearBaseObjectKeyed()
        {
            _KeyType = null;
            _Key = null;
            _ObjectFlags = 0;
            _CreationTime = DateTime.MinValue;
            _ModifiedTime = DateTime.MinValue;
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectKeyed(this);
        }

        public Type KeyType
        {
            get
            {
                return _KeyType;
            }
            set
            {
                _KeyType = value;
            }
        }

        public bool IsIntegerKeyType
        {
            get
            {
                return ObjectUtilities.IsIntegerType(_KeyType);
            }
        }

        public object Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (ObjectUtilities.CompareObjects(_Key, value) != 0)
                {
                    ModifiedFlag = true;
                    _Key = value;
                    if (_Key != null)
                        _KeyType = _Key.GetType();
                    else
                        _KeyType = null;
                }
            }
        }

        public void SetKeyNoModify(object key)
        {
            _Key = key;
            if (key != null)
                _KeyType = key.GetType();
            else
                _KeyType = null;
        }

        public void ResetKeyNoModify()
        {
            _Key = GetResetKeyValue(_KeyType);
        }

        public static object GetResetKeyValue(Type keyType)
        {
            object key = null;

            if (keyType != null)
            {
                switch (keyType.Name)
                {
                    case "String":
                        key = String.Empty;
                        break;
                    case "Char":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Byte":
                    case "UInt16":
                    case "UInt32":
                    case "UInt64":
                        key = 0;
                        break;
                    case "Single":
                        key = float.NaN;
                        break;
                    case "Double":
                        key = double.NaN;
                        break;
                    case "Boolean":
                        key = null;
                        break;
                    case "Guid":
                        key = Guid.Empty;
                        break;
                    default:
                        key = null;
                        break;
                }
            }

            return key;
        }

        public string KeyString
        {
            get
            {
                if (_Key == null)
                    return String.Empty;
                else
                    return _Key.ToString();
            }
        }

        public int KeyInt
        {
            get
            {
                if ((_Key != null) && IsIntegerKeyType)
                    return (int)_Key;
                return 0;
            }
        }

        public virtual void Rekey(object newKey)
        {
            Key = newKey;
        }

        public virtual string Name
        {
            get
            {
                return KeyString;
            }
            set
            {
            }
        }

        public virtual string TypeLabel
        {
            get
            {
                return GetType().Name;
            }
        }

        public virtual Guid Guid
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
            }
        }

        public virtual string GuidString
        {
            get
            {
                Guid guid = Guid;
                if (guid != null)
                    return guid.ToString();
                return String.Empty;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    Guid = new Guid(value);
                    ModifiedFlag = true;
                }
                else
                    Guid = Guid.Empty;
            }
        }

        public virtual bool EnsureGuid()
        {
            return true;
        }

        public virtual void NewGuid()
        {
        }

        public virtual string Owner
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public virtual string Source
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        protected int ObjectFlags
        {
            get
            {
                return _ObjectFlags;
            }
            set
            {
                _ObjectFlags = value;
            }
        }

        protected bool ModifiedFlag
        {
            get
            {
                return (_ObjectFlags & ObjectFlagModified) == ObjectFlagModified;
            }
            set
            {
                _ObjectFlags = (value ? (_ObjectFlags | ObjectFlagModified) : (_ObjectFlags & ~ObjectFlagModified));
            }
        }

        public virtual bool Modified
        {
            get
            {
                return ModifiedFlag;
            }
            set
            {
                ModifiedFlag = value;
            }
        }

        protected bool MarkedFlag
        {
            get
            {
                return (_ObjectFlags & ObjectFlagMarked) == ObjectFlagMarked;
            }
            set
            {
                _ObjectFlags = (value ? (_ObjectFlags | ObjectFlagMarked) : (_ObjectFlags & ~ObjectFlagMarked));
            }
        }

        protected int PersistentFlags
        {
            get
            {
                return _ObjectFlags & ObjectMaskPersistent;
            }
            set
            {
                _ObjectFlags = (value & ObjectMaskPersistent) | (_ObjectFlags & ~ObjectMaskPersistent);
            }
        }

        protected bool PersistentChild1Flag
        {
            get
            {
                return (_ObjectFlags & ObjectFlagPersistentChild1) == ObjectFlagPersistentChild1;
            }
            set
            {
                _ObjectFlags = (value ? (_ObjectFlags | ObjectFlagPersistentChild1) : (_ObjectFlags & ~ObjectFlagPersistentChild1));
            }
        }

        protected bool PersistentChild2Flag
        {
            get
            {
                return (_ObjectFlags & ObjectFlagPersistentChild2) == ObjectFlagPersistentChild2;
            }
            set
            {
                _ObjectFlags = (value ? (_ObjectFlags | ObjectFlagPersistentChild2) : (_ObjectFlags & ~ObjectFlagPersistentChild2));
            }
        }

        public virtual DateTime CreationTime
        {
            get
            {
                return _CreationTime;
            }
            set
            {
                if (_CreationTime != value)
                {
                    _CreationTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual DateTime ModifiedTime
        {
            get
            {
                return _ModifiedTime;
            }
            set
            {
                if (_ModifiedTime != value)
                {
                    _ModifiedTime = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual void Touch()
        {
            if (CreationTime == DateTime.MinValue)
            {
                CreationTime = DateTime.UtcNow;
                ModifiedTime = _CreationTime;
            }
            else
                ModifiedTime = DateTime.UtcNow;
        }

        public virtual void TouchAndClearModified()
        {
            Touch();
            Modified = false;
        }

        public virtual void TouchAndSetModified()
        {
            Touch();
            ModifiedFlag = true;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            if (_KeyType != null)
                element.Add(new XAttribute("KeyType", _KeyType.Name));
            if (_Key != null)
                element.Add(new XAttribute("Key", _Key.ToString()));
            int persistentFlags = PersistentFlags;
            if (persistentFlags != 0)
                element.Add(new XAttribute("PersistentFlags", persistentFlags.ToString()));
            if (_CreationTime != DateTime.MinValue)
                element.Add(new XAttribute("CreationTime", ObjectUtilities.GetStringFromDateTime(_CreationTime)));
            if (_ModifiedTime != DateTime.MinValue)
                element.Add(new XAttribute("ModifiedTime", ObjectUtilities.GetStringFromDateTime(_ModifiedTime)));
            return element;
        }

        public override void OnElement(XElement element)
        {
            base.OnElement(element);
            ModifiedFlag = false;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "KeyType":
                    KeyType = ObjectUtilities.GetTypeFromString(attributeValue);
                    break;
                case "Key":
                    if (_KeyType != null)
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, _KeyType.Name);
                    else
                        Key = ObjectUtilities.GetKeyFromString(attributeValue, null);
                    break;
                case "PersistentFlags":
                    PersistentFlags = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "CreationTime":
                    _CreationTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                case "ModifiedTime":
                    _ModifiedTime = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MinValue);
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public virtual bool FromString(string value)
        {
            try
            {
                Xml = XElement.Parse(value, LoadOptions.PreserveWhitespace);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                    else
                        ObjectUtilities.DisplayLabel(this, label, indent);
                    break;
                case DisplayDetail.Full:
                    if (!String.IsNullOrEmpty(KeyString))
                        ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                    else
                        ObjectUtilities.DisplayLabel(this, label, indent);
                    DisplayField("Modified", (Modified ? "true" : "false"), indent + 1);
                    DisplayField("CreationTime", _CreationTime.ToString(), indent + 1);
                    DisplayField("ModifiedTime", _ModifiedTime.ToString(), indent + 1);
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Xml:
                    base.Display(label, detail, indent);
                    break;
                default:
                    break;
            }
        }

        public virtual int Compare(IBaseObjectKeyed other)
        {
            if (other == null)
                return 1;

            int returnValue = ObjectUtilities.CompareKeys(this, other);

            if (returnValue != 0)
                return returnValue;

            returnValue = ObjectUtilities.CompareDateTimes(_CreationTime, other.CreationTime);

            if (returnValue != 0)
                return returnValue;

            return ObjectUtilities.CompareDateTimes(_ModifiedTime, other.ModifiedTime);
        }

        public virtual int CompareKey(object key)
        {
            return ObjectUtilities.CompareObjects(_Key, key);
        }

        public static int CompareKeys(BaseObjectKeyed object1, BaseObjectKeyed object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public virtual bool MatchKey(object key)
        {
            return CompareKey(key) == 0;
        }

        public virtual void OnFixup(FixupDictionary fixups)
        {
        }
    }
}

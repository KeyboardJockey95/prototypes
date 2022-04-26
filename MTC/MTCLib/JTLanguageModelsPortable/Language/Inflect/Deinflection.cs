using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    // Key inflected word.
    public class Deinflection : BaseObjectKeyed
    {
        protected List<DeinflectionInstance> _Instances;

        public Deinflection(string inflectedWord, List<DeinflectionInstance> instances) : base(inflectedWord)
        {
            ClearDeinflection();
            _Instances = instances;
        }

        public Deinflection(string inflectedWord, DeinflectionInstance instance) : base(inflectedWord)
        {
            ClearDeinflection();
            _Instances = new List<DeinflectionInstance>() { instance };
        }

        public Deinflection(Deinflection other) :
            base(other)
        {
            CopyDeinflection(other);
        }

        public Deinflection()
        {
            ClearDeinflection();
        }

        public void ClearDeinflection()
        {
            _Instances = null;
        }

        public void CopyDeinflection(Deinflection other)
        {
            if (other.Instances != null)
            {
                Instances = new List<DeinflectionInstance>();

                foreach (DeinflectionInstance di in other.Instances)
                {
                    DeinflectionInstance newDI = new DeinflectionInstance(di);
                    Instances.Add(newDI);
                }
            }
            else
                _Instances = null;
        }

        public override string ToString()
        {
            string returnValue = KeyString + ": ";

            if ((_Instances != null) && (_Instances.Count() != 0))
            {
                bool first = true;

                foreach (DeinflectionInstance di in _Instances)
                {
                    if (!first)
                        returnValue += "/";
                    else
                        first = false;

                    returnValue += di.ToString();
                }
            }
            else
                returnValue = base.ToString();

            return returnValue;
        }

        public List<DeinflectionInstance> Instances
        {
            get
            {
                return _Instances;
            }
            set
            {
                if (value != _Instances)
                {
                    _Instances = new List<DeinflectionInstance>();
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasInstance(DeinflectionInstance instance)
        {
            if ((_Instances == null) || (_Instances.Count() == 0))
                return false;

            if (_Instances.FirstOrDefault(x => x.Compare(instance) == 0) != null)
                return true;

            return false;
        }

        public DeinflectionInstance GetInstanceIndexed(int index)
        {
            if ((_Instances == null) || (_Instances.Count() == 0))
                return null;

            return _Instances[index];
        }

        public void AddInstance(DeinflectionInstance deinflectionInstance)
        {
            if (_Instances == null)
                _Instances = new List<DeinflectionInstance>();

            _Instances.Add(deinflectionInstance);
        }

        public int InstanceCount()
        {
            if ((_Instances == null) || (_Instances.Count() == 0))
                return 0;

            return _Instances.Count();
        }

        public bool HasCategory(LexicalCategory category)
        {
            if ((_Instances == null) || (_Instances.Count() == 0))
                return false;

            if (_Instances.FirstOrDefault(x => x.Category == category) != null)
                return true;

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XAttribute("K", KeyString));

            if (_Instances != null)
            {
                foreach (DeinflectionInstance di in _Instances)
                    element.Add(di.GetElement("I"));
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "K":
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
                case "I":
                    {
                        DeinflectionInstance deinflectionInstance = new DeinflectionInstance(childElement);
                        AddInstance(deinflectionInstance);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

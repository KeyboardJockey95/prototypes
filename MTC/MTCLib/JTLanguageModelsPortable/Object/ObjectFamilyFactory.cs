using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class ObjectFamilyFactory<T> : BaseObjectKeyed where T : class, new()
    {
        protected Dictionary<string, ObjectFactory<T>> _Factories;

        public ObjectFamilyFactory(object key) : base(key)
        {
            _Factories = new Dictionary<string, ObjectFactory<T>>();
        }

        public ObjectFamilyFactory()
        {
            _Factories = new Dictionary<string, ObjectFactory<T>>();
        }

        public Dictionary<string, ObjectFactory<T>> Factories
        {
            get
            {
                return _Factories;
            }
            set
            {
                _Factories = value;
            }
        }

        public void Add(ObjectFactory<T> factory)
        {
            if (factory == null)
                return;

            if (_Factories.ContainsKey(factory.KeyString))
                return;

            _Factories.Add(factory.KeyString, factory);
        }

        public void AddTyped<TT>(ObjectFactory<TT> factory) where TT : class, T, new()
        {
            if (factory == null)
                return;

            if (_Factories.ContainsKey(factory.KeyString))
                return;

            _Factories.Add(factory.KeyString, factory as ObjectFactory<T>);
        }

        public virtual T Create(string type)
        {
            ObjectFactory<T> factory = null;

            if (_Factories.TryGetValue(type, out factory))
                return factory.Create();

            return null;
        }

        public virtual TT CreateTyped<TT>(string type) where TT : class, T
        {
            ObjectFactory<T> factory = null;

            if (_Factories.TryGetValue(type, out factory))
                return factory.Create() as TT;

            return null;
        }

        public List<string> Keys
        {
            get
            {
                List<string> keys = _Factories.Keys.ToList();
                return keys;
            }
        }
    }
}

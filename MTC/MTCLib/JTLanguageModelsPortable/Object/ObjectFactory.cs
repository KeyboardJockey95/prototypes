using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class ObjectFactory<T> : BaseObjectKeyed where T : class, new()
    {
        public ObjectFactory(object key)
            : base(key)
        {
        }

        public ObjectFactory()
        {
        }

        public virtual T Create()
        {
            return new T();
        }
    }
}

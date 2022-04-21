using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Dictionary
{
    public class DictionarySources : PersistentStringMapper
    {
        public DictionarySources(string filePath) : base("DictionarySources", filePath)
        {
        }

        public DictionarySources(DictionarySources other) : base(other)
        {
        }

        public DictionarySources(XElement element) : base(element)
        {
        }

        public DictionarySources()
        {
        }

        public override IBaseObject Clone()
        {
            return new DictionarySources(this);
        }
    }
}

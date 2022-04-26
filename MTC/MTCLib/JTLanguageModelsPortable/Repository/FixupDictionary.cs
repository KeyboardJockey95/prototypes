using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Repository
{
    public class FixupDictionary
    {
        public Dictionary<string, Dictionary<string, IBaseObjectKeyed>> AllDictionary { get; set; }
        public IMainRepository Repositories { get; set; }

        public FixupDictionary(IMainRepository repositories)
        {
            AllDictionary = new Dictionary<string, Dictionary<string, IBaseObjectKeyed>>(32);
            Repositories = repositories;
        }

        public void Add(IBaseObjectKeyed obj)
        {
            if (obj == null)
                return;

            string source = obj.Source;
            Dictionary<string, IBaseObjectKeyed> storeDictionary = null;

            if (!AllDictionary.TryGetValue(source, out storeDictionary))
            {
                storeDictionary = new Dictionary<string, IBaseObjectKeyed>(256);
                AllDictionary.Add(source, storeDictionary);
            }

            string xmlKey = obj.KeyString;

            if (!String.IsNullOrEmpty(xmlKey))
            {
                IBaseObjectKeyed testObject;

                if (!storeDictionary.TryGetValue(xmlKey, out testObject))
                    storeDictionary.Add(xmlKey, obj);
            }
        }

        public void Add(IBaseObjectKeyed obj, string source)
        {
            if (obj == null)
                return;

            Dictionary<string, IBaseObjectKeyed> storeDictionary = null;

            if (!AllDictionary.TryGetValue(source, out storeDictionary))
            {
                storeDictionary = new Dictionary<string, IBaseObjectKeyed>(256);
                AllDictionary.Add(source, storeDictionary);
            }

            string xmlKey = obj.KeyString;

            if (!String.IsNullOrEmpty(xmlKey))
                storeDictionary.Add(xmlKey, obj);
        }

        public IBaseObjectKeyed Get(string source, string xmlKey)
        {
            Dictionary<string, IBaseObjectKeyed> storeDictionary = null;
            IBaseObjectKeyed obj = null;

            if (!AllDictionary.TryGetValue(source, out storeDictionary))
                return null;

            storeDictionary.TryGetValue(xmlKey, out obj);

            return obj;
        }

        public virtual void DoFixups()
        {
            foreach (KeyValuePair<string, Dictionary<string, IBaseObjectKeyed>> storeKVP in AllDictionary)
            {
                string source = storeKVP.Key;

                foreach (KeyValuePair<string, IBaseObjectKeyed> itemKVP in storeKVP.Value)
                {
                    IBaseObjectKeyed obj = itemKVP.Value;

                    obj.Modified = false;

                    obj.OnFixup(this);

                    if (!String.IsNullOrEmpty(source) && (source != "Nodes") && obj.Modified)
                    {
                        if (!Repositories.UpdateReference(source, null, obj))
                            throw new ObjectException("FixupDictionary.DoFixups: Error updating " + itemKVP.Key + " in " + source + ".");
                    }
                }
            }

            Repositories = null;
        }
    }
}

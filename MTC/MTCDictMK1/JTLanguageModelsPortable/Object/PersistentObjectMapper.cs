using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class PersistentObjectMapper<T> : ObjectMapper<T> where T : class, IBaseObjectKeyed, new()
    {
        protected string _FilePath;

        public PersistentObjectMapper(string name, string filePath) : base(name)
        {
            _FilePath = filePath;
            Load();
        }

        public PersistentObjectMapper(PersistentObjectMapper<T> other) : base(other)
        {
            CopyPersistentObjectMapper(other);
        }

        public PersistentObjectMapper(XElement element)
        {
            ClearPersistentObjectMapper();
            OnElement(element);
        }

        public PersistentObjectMapper()
        {
            ClearPersistentObjectMapper();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectMapper();
            ClearPersistentObjectMapper();
        }

        public void ClearPersistentObjectMapper()
        {
        }

        public virtual void CopyPersistentObjectMapper(PersistentObjectMapper<T> other)
        {
            _FilePath = other.FilePath;
        }

        public override IBaseObject Clone()
        {
            return new PersistentObjectMapper<T>(this);
        }

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                if (value != _FilePath)
                {
                    _FilePath = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override int Add(T obj)
        {
            bool saveModifiedFlag = ModifiedFlag;
            ModifiedFlag = false;

            int id = base.Add(obj);

            if (ModifiedFlag)
                Save();

            if (saveModifiedFlag)
                ModifiedFlag = saveModifiedFlag;

            return id;
        }

        public override void AddList(List<T> list)
        {
            bool saveModifiedFlag = ModifiedFlag;
            ModifiedFlag = false;

            base.AddList(list);

            if (ModifiedFlag)
                Save();

            if (saveModifiedFlag)
                ModifiedFlag = saveModifiedFlag;
        }

        public void Load()
        {
            if (String.IsNullOrEmpty(_FilePath))
                return;

            Stream stream = null;

            if (!FileSingleton.Exists(_FilePath))
                return;

            try
            {
                stream = FileSingleton.OpenRead(_FilePath);

                if (stream != null)
                {
                    XElement element = XElement.Load(stream, LoadOptions.PreserveWhitespace);
                    _ObjectList.Clear();
                    OnElement(element);
                }
            }
            catch (Exception exc)
            {
                string message = "Exception while loading object mapper " + Name + ": " +
                    exc.Message;

                if (exc.InnerException != null)
                    message += ": " + exc.InnerException.Message;

                ApplicationData.Global.PutConsoleErrorMessage(message);
            }
            finally
            {
                if (stream != null)
                    FileSingleton.Close(stream);
            }
        }

        public void SaveCheck()
        {
            if (Modified)
                Save();
        }

        public void Save()
        {
            if (String.IsNullOrEmpty(_FilePath))
                return;

            XElement element = GetElement(Name);
            Stream stream = null;

            if (!FileSingleton.Exists(_FilePath))
                FileSingleton.DirectoryExistsCheck(_FilePath);
            else
                FileSingleton.Delete(_FilePath);

            try
            {
                stream = FileSingleton.OpenWrite(_FilePath);

                if (stream != null)
                    element.Save(stream);
            }
            catch (Exception exc)
            {
                string message = "Exception while saving object mapper " + Name + ": " +
                    exc.Message;

                if (exc.InnerException != null)
                    message += ": " + exc.InnerException.Message;

                ApplicationData.Global.PutConsoleErrorMessage(message);
            }
            finally
            {
                if (stream != null)
                    FileSingleton.Close(stream);
            }
        }
    }
}

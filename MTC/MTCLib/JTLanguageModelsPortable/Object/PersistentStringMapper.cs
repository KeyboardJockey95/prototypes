using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class PersistentStringMapper : StringMapper
    {
        protected string _FilePath;

        public PersistentStringMapper(string name, string filePath) : base(name)
        {
            _FilePath = filePath;
            Load();
        }

        public PersistentStringMapper(PersistentStringMapper other) : base(other)
        {
            CopyPersistentStringMapper(other);
        }

        public PersistentStringMapper(XElement element)
        {
            ClearPersistentStringMapper();
            OnElement(element);
        }

        public PersistentStringMapper()
        {
            ClearPersistentStringMapper();
        }

        public override void Clear()
        {
            base.Clear();
            ClearStringMapper();
            ClearPersistentStringMapper();
        }

        public void ClearPersistentStringMapper()
        {
        }

        public virtual void CopyPersistentStringMapper(PersistentStringMapper other)
        {
            _FilePath = other.FilePath;
        }

        public override IBaseObject Clone()
        {
            return new PersistentStringMapper(this);
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
                    Modified = true;
                }
            }
        }

        public override int GetOrAdd(string str)
        {
            bool saveModified = Modified;
            Modified = false;

            int id = base.GetOrAdd(str);

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;

            return id;
        }

        public override int Add(string str)
        {
            bool saveModified = Modified;
            Modified = false;

            int id = base.Add(str);

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;

            return id;
        }

        public override void AddList(List<string> list)
        {
            bool saveModified = Modified;
            Modified = false;

            base.AddList(list);

            if (Modified)
                Save();

            if (saveModified)
                Modified = saveModified;
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
                    _StringList.Clear();
                    OnElement(element);
                }
            }
            catch (Exception exc)
            {
                string message = "Exception while loading string mapper " + Name + ": " +
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
                string message = "Exception while saving string mapper " + Name + ": " +
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class PersistentObject : BaseObjectKeyed
    {
        protected string _FilePath;

        public PersistentObject(string name, string filePath) : base(name)
        {
            _FilePath = filePath;
        }

        public PersistentObject(PersistentObject other) : base(other)
        {
            CopyPersistentObject(other);
        }

        public PersistentObject(XElement element)
        {
            ClearPersistentObject();
            OnElement(element);
        }

        public PersistentObject()
        {
            ClearPersistentObject();
        }

        public override void Clear()
        {
            base.Clear();
            ClearPersistentObject();
            ClearPersistentObject();
        }

        public void ClearPersistentObject()
        {
        }

        public virtual void CopyPersistentObject(PersistentObject other)
        {
            _FilePath = other.FilePath;
        }

        public override IBaseObject Clone()
        {
            return new PersistentObject(this);
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
                    base.Clear();
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

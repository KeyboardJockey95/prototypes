using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Object
{
    public class BaseObjectTagged : BaseObjectKeyed
    {
        private string _TagKey;

        public BaseObjectTagged(object key) : base(key)
        {
            InitializeTag();
        }

        public BaseObjectTagged(IBaseObjectKeyed other) : base(other)
        {
            InitializeTag();
        }

        public BaseObjectTagged()
        {
        }

        public new object Key
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
                    InitializeTag();
                }
            }
        }

        public string TagKey
        {
            get
            {
                return _TagKey;
            }
        }

        private void InitializeTag()
        {
            if (Key == null)
                return;
            if (PortableFile.Singleton == null)
                return;
            _TagKey = GetType().Name + "." + Key.ToString() + ".";
            LoadCreationTime();
            LoadModifiedTime();
        }

        protected void LoadCreationTime()
        {
            if (_TagKey == null)
                return;
            LoadTime("C.txt", ref _CreationTime);
        }

        protected void SaveCreationTime()
        {
            if (_TagKey == null)
                return;
            SaveTime("C.txt", _CreationTime);
        }

        protected void LoadModifiedTime()
        {
            if (_TagKey == null)
                return;
            LoadTime("M.txt", ref _ModifiedTime);
        }

        protected void SaveModifiedTime()
        {
            if (_TagKey == null)
                return;
            SaveTime("M.txt", _ModifiedTime);
        }

        protected virtual void LoadTime(string suffix, ref DateTime date)
        {
            lock (this)
            {
                LoadTimeStatic(TagKey, suffix, ref date);
            }
        }

        private bool InSaveTime = false;

        protected virtual void SaveTime(string suffix, DateTime date)
        {
            if (!InSaveTime)
            {
                lock (this)
                {
                    InSaveTime = true;
                    SaveTimeStatic(TagKey, suffix, date);
                    InSaveTime = false;
                }
            }
        }

        public static void LoadTimeStatic(string tagKey, string suffix, ref DateTime date)
        {
            if ((PortableFile.Singleton == null) || (ApplicationData.Global == null))
                return;
            string value = null;
            try
            {
                string filePath = ApplicationData.Global.GetTagsFileName(tagKey + suffix);
                if (FileSingleton.Exists(filePath))
                {
                    Stream stream = FileSingleton.OpenRead(filePath);
                    BinaryReader reader = new BinaryReader(stream);
                    value = reader.ReadString();
                    FileSingleton.Close(stream);
                }
                else
                {
                    date = DateTime.MinValue;
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
            date = ObjectUtilities.GetDateTimeFromString(value, DateTime.MinValue);
        }

        public static void SaveTimeStatic(string tagKey, string suffix, DateTime date)
        {
            if ((PortableFile.Singleton == null) || (ApplicationData.Global == null))
                return;
            string filePath = ApplicationData.Global.GetTagsFileName(tagKey + suffix);
            FileSingleton.DirectoryExistsCheck(filePath);
            Stream stream = FileSingleton.OpenWrite(filePath);
            if (stream == null)
                return;
            BinaryWriter writer = new BinaryWriter(stream);
            string value = ObjectUtilities.GetStringFromDateTime(date);
            writer.Write(value);
            FileSingleton.Close(stream);
        }

        public override DateTime CreationTime
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
                    SaveCreationTime();
                }
            }
        }

        public override DateTime ModifiedTime
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
                    SaveModifiedTime();
                }
            }
        }
    }
}

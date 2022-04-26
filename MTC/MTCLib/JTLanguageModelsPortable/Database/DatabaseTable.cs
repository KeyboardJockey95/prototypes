using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;
using Lex.Db;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseTable : BaseObjectKeyed
    {
        public string DatabaseDirectory { get; set; }
        public string LocalDirectory { get; set; }
        public bool GenerateKeys { get; set; }
        public LanguageID LanguageID { get; set; }
        public LanguageID AdditionalLanguageID { get; set; }
        public bool CaseInsensitive { get; set; }
        protected DbInstance _DB;

        public DatabaseTable(string name, string databaseDirectory, bool generateKeys,
                LanguageID languageID, bool caseInsensitive)
        {
            Key = name;
            DatabaseDirectory = databaseDirectory;
            if (languageID != null)
            {
                string symbolName = languageID.SymbolName;
                if (symbolName == null)
                    symbolName = "none";
                LocalDirectory = name + ApplicationData.PlatformPathSeparator + name + "_" + symbolName;
                string dir = MediaUtilities.ConcatenateFilePath(DatabaseDirectory, "Lex.Db");
                string path = MediaUtilities.ConcatenateFilePath(dir, LocalDirectory);
                FileSingleton.DirectoryExistsCheck(path);
            }
            else
                LocalDirectory = name;
            GenerateKeys = generateKeys;
            LanguageID = languageID;
            AdditionalLanguageID = null;
            CaseInsensitive = caseInsensitive;
            _DB = null;
        }

        public DatabaseTable(string name, string databaseDirectory, bool generateKeys,
                LanguageID languageID1, LanguageID languageID2, bool caseInsensitive)
        {
            Key = name;
            DatabaseDirectory = databaseDirectory;
            string symbolName1 = (languageID1 != null ? languageID1.SymbolName : "none");
            string symbolName2 = (languageID2 != null ? languageID2.SymbolName : "none");
            LocalDirectory = name + ApplicationData.PlatformPathSeparator + name + "_" + symbolName1 + "_" + symbolName2;
            string dir = MediaUtilities.ConcatenateFilePath(DatabaseDirectory, "Lex.Db");
            string path = MediaUtilities.ConcatenateFilePath(dir, LocalDirectory);
            FileSingleton.DirectoryExistsCheck(path);
            GenerateKeys = generateKeys;
            LanguageID = languageID1;
            AdditionalLanguageID = languageID2;
            CaseInsensitive = caseInsensitive;
            _DB = null;
        }

        public DatabaseTable()
        {
            DatabaseDirectory = null;
            LocalDirectory = String.Empty;
            GenerateKeys = false;
            LanguageID = null;
            CaseInsensitive = true;
            _DB = null;
        }

        public virtual void Map()
        {
            throw new ObjectException("DatabaseTable:  Map should not be called.");
        }

        public virtual DatabaseMatcher CreateDatabaseMatcher(Matcher subMatcher)
        {
            return new DatabaseMatcher(this, subMatcher);
        }

        public virtual void Initialize()
        {
            try
            {
                _DB = DatabaseObjectFactory.CreateDbInstance(LocalDirectory, DatabaseDirectory);

                Map();

                _DB.Initialize();
            }
            catch (Exception)
            {
                Reset();
                throw;
            }
        }

        public virtual void Reset()
        {
            if (_DB != null)
            {
                _DB.Dispose();
                _DB = null;
            }
        }

        public virtual bool TableExists()
        {
            bool exists = false;

            if (_DB != null)
            {
                string path = _DB.Path;

                if (!String.IsNullOrEmpty(path))
                {
                    if (FileSingleton.DirectoryExists(path))
                        exists = true;
                }
            }

            return exists;
        }

        public virtual bool CreateTable()
        {
            DeleteTableCheck();

            try
            {
                Reset();
                Initialize();
                DeleteAll();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public virtual bool CreateTableCheck()
        {
            bool returnValue = true;
            return returnValue;
        }

        public virtual bool DeleteTable()
        {
            if (_DB != null)
            {
                string path = _DB.Path;

                if (!String.IsNullOrEmpty(path))
                {
                    if (FileSingleton.DirectoryExists(path))
                    {
                        try
                        {
                            Reset();
                            FileSingleton.DeleteDirectory(path);
                            Initialize();
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public virtual bool DeleteTableCheck()
        {
            if (TableExists())
                return DeleteTable();

            return true;
        }

        public virtual bool RecreateTableCheck()
        {
            return CreateTable();
        }

        public virtual bool ContainsKey(object key)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool Contains(Matcher matcher)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual List<object> GetKeys()
        {
            List<object> keys = new List<object>();
            return keys;
        }

        public virtual IBaseObject Get(object key)
        {
            IBaseObject obj = null;
            return obj;
        }

        public virtual IBaseObject GetFirst(Matcher matcher)
        {
            return null;
        }

        public virtual IBaseObject GetIndexed(int index)
        {
            IBaseObject obj = null;
            return obj;
        }

        public virtual List<object> QueryKeys(Matcher matcher)
        {
            return null;
        }

        public virtual List<IBaseObject> Query(Matcher matcher)
        {
            return null;
        }

        public virtual int QueryCount(Matcher matcher)
        {
            return 0;
        }

        public virtual List<IBaseObject> GetAll()
        {
            List<IBaseObject> objs = new List<IBaseObject>();
            return objs;
        }

        public virtual bool Add(object key, IBaseObject obj)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool Add(IBaseObjectKeyed obj)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool AddList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool Update(object key, IBaseObject obj)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool Update(IBaseObjectKeyed obj)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool UpdateList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool DeleteKey(object key)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool Delete(IBaseObjectKeyed obj)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool DeleteKeyList(List<object> keys)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual int DeleteQuery(Matcher matcher)
        {
            return 0;
        }

        public virtual bool DeleteList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool DeleteAll()
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual int Count()
        {
            int count = 0;
            return count;
        }
    }
}

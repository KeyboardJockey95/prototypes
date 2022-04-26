using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseTableFactory
    {
        public string DatabaseDirectory { get; set; }
        public bool GenerateKeys { get; set; }
        public bool CaseInsensitive { get; set; }

        public DatabaseTableFactory(string databaseDirectory, bool generateKeys, bool caseInsensitive)
        {
            DatabaseDirectory = databaseDirectory;
            GenerateKeys = generateKeys;
            CaseInsensitive = caseInsensitive;
        }

        public virtual DatabaseTable Create(string name, LanguageID languageID)
        {
            return null;
        }

        public virtual DatabaseTable Create(string name, LanguageID languageID1, LanguageID languageID2)
        {
            return null;
        }
    }

    public class DatabaseTableFactoryTyped<TableType> : DatabaseTableFactory
        where TableType : DatabaseTable, new()
    {
        public DatabaseTableFactoryTyped(string databaseDirectory, bool generateKeys, bool caseInsensitive) :
            base(databaseDirectory, generateKeys, caseInsensitive)
        {
        }

        public override DatabaseTable Create(string name, LanguageID languageID)
        {
            TableType table = new TableType();
            table.Key = name;
            table.DatabaseDirectory = DatabaseDirectory;
            if (languageID != null)
            {
                string symbolName = languageID.SymbolName;
                if (symbolName == null)
                    symbolName = "none";
                table.LocalDirectory = name + ApplicationData.PlatformPathSeparator + name + "_" + symbolName;
                string dir = MediaUtilities.ConcatenateFilePath(table.DatabaseDirectory, "Lex.Db");
                string path = MediaUtilities.ConcatenateFilePath(dir, table.LocalDirectory);
                FileSingleton.DirectoryExistsCheck(path);
            }
            else
                table.LocalDirectory = name;
            table.GenerateKeys = GenerateKeys;
            table.LanguageID = languageID;
            table.CaseInsensitive = CaseInsensitive;
            return table;
        }

        public override DatabaseTable Create(string name, LanguageID languageID1, LanguageID languageID2)
        {
            TableType table = new TableType();
            table.Key = name;
            table.DatabaseDirectory = DatabaseDirectory;
            string symbolName1 = (languageID1 != null ? languageID1.SymbolName : "none");
            string symbolName2 = (languageID2 != null ? languageID2.SymbolName : "none");
            table.LocalDirectory = name + ApplicationData.PlatformPathSeparator + name + "_" + symbolName1 + "_" + symbolName2;
            string dir = MediaUtilities.ConcatenateFilePath(table.DatabaseDirectory, "Lex.Db");
            string path = MediaUtilities.ConcatenateFilePath(dir, table.LocalDirectory);
            FileSingleton.DirectoryExistsCheck(path);
            table.GenerateKeys = GenerateKeys;
            table.LanguageID = languageID1;
            table.AdditionalLanguageID = languageID2;
            table.CaseInsensitive = CaseInsensitive;
            return table;
        }
    }
}

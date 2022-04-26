using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lex.Db;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseObjectFactory
    {
        public static DatabaseObjectFactory Singleton;
        public static string DatabaseDirectory = @"Content\Database";

        public static DbInstance CreateDbInstance(string repositoryDirectory, string databaseDirectory = null)
        {
            if (Singleton != null)
                return Singleton.OnCreateDbInstance(repositoryDirectory, databaseDirectory);

            return null;
        }

        public DatabaseObjectFactory()
        {
        }

        public virtual DbInstance OnCreateDbInstance(string repositoryDirectory, string databaseDirectory)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Repository
{
    public class CacheOptions
    {
        public int CacheSize { get; set; }  // 0 if not caching, -1 if not limited.
        public bool PreloadAll { get; set; }
        public bool FileStorage { get; set; }

        public CacheOptions(int cacheSize, bool preloadAll, bool fileStorage)
        {
            CacheSize = cacheSize;
            PreloadAll = preloadAll;
            FileStorage = fileStorage;
        }
    }
}

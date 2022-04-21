using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Repository
{
    public static class RepositoryUtilities
    {
        public static bool ReindexTitled<T>(List<T> list, IMainRepository repositories) where T : BaseObjectTitled
        {
            int listCount = (list != null ? list.Count() : 0);
            bool returnValue = true;
            if (listCount != 0)
            {
                int index = 0;
                list.Sort(BaseObjectTitled.CompareIndices);
                foreach (T tmp in list)
                {
                    if (tmp.Index != index)
                    {
                        tmp.Index = index;

                        if (!repositories.UpdateReference(tmp.Source, null, tmp))
                            returnValue = false;
                    }
                    index++;
                }
            }
            return returnValue;
        }
    }
}

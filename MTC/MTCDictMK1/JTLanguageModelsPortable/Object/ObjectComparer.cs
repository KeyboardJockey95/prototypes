using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Object
{
    public class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] obj1, byte[] obj2)
        {
            return ObjectUtilities.CompareByteArrays(obj1, obj2);
        }
    }
}

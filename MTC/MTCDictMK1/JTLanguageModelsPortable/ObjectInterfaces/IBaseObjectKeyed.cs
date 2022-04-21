using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.ObjectInterfaces
{
    public interface IBaseObjectKeyed : IBaseObject
    {
        Type KeyType { get; set; }
        bool IsIntegerKeyType { get; }
        object Key { get; set; }
        void SetKeyNoModify(object key);
        void ResetKeyNoModify();
        string KeyString { get; }
        int KeyInt { get; }
        string Name { get; set; }
        string TypeLabel { get; }
        Guid Guid { get; set; }
        string GuidString { get; set; }
        bool EnsureGuid();
        void NewGuid();
        string Owner { get; set; }
        string Source { get; set; }
        bool Modified { get; set; }
        DateTime CreationTime { get; set; }
        DateTime ModifiedTime { get; set; }
        void Touch();
        void TouchAndClearModified();
        string ToString();
        bool FromString(string value);
        int Compare(IBaseObjectKeyed other);
        int CompareKey(object key);
        bool MatchKey(object key);
        void OnFixup(FixupDictionary fixups);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;
using Lex.Db;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseItem
    {
        public byte[] Data { get; set; }

        public DatabaseItem()
        {
            Data = null;
        }

        public virtual object GetKey()
        {
            return null;
        }

        public virtual IBaseObject Get()
        {
            IBaseObject obj = new BaseObject();
            obj.BinaryData = Data;
            return obj;
        }

        public virtual void Set(object key, IBaseObject target)
        {
            Data = target.BinaryData;
        }

        public virtual void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
        }
    }

    public class DatabaseItemTyped<KeyType, TargetType> : DatabaseItem
        where TargetType : class, IBaseObject, new()
    {
        public KeyType Key { get; set; }

        public override object GetKey()
        {
            return Key;
        }

        public override void Set(object key, IBaseObject target)
        {
            Key = (KeyType)key;
            Data = target.BinaryData;
        }

        public override IBaseObject Get()
        {
            TargetType obj = new TargetType();
            obj.BinaryData = Data;
            return obj;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemTyped<KeyType, TargetType>>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemIndexed<KeyType, IndexKeyType, TargetType> : DatabaseItemTyped<KeyType, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public IndexKeyType IndexKey { get; set; }

        public DatabaseItemIndexed()
        {
            IndexKey = default(IndexKeyType);
        }

        public override void Set(object key, IBaseObject target)
        {
            Key = (KeyType)key;
            IndexKey = default(IndexKeyType);
            Data = target.BinaryData;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIndexed<KeyType, IndexKeyType, TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey);
        }
    }

    public class DatabaseItemIntKey<TargetType> : DatabaseItemTyped<int, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKey<TargetType>>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemStringKey<TargetType> : DatabaseItemTyped<string, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemStringKey<TargetType>>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }

        public IComparer<string> GetComparer(LanguageID languageID, bool caseInsensitive)
        {
            IComparer<string> comparer = ApplicationData.GetComparer<string>(caseInsensitive);
            return comparer;
        }
    }

    public class DatabaseItemLanguageStringKey<TargetType> : DatabaseItemTyped<string, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemLanguageStringKey<TargetType>>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }

        public IComparer<string> GetComparer(LanguageID languageID, bool caseInsensitive)
        {
            IComparer<string> comparer = ApplicationData.GetLanguageComparer<string>(languageID, caseInsensitive);
            return comparer;
        }
    }

    public class DatabaseItemIntKeyIndexed<IndexKeyType, TargetType> :
            DatabaseItemIndexed<int, IndexKeyType, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyIndexed<IndexKeyType, TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey);
        }
    }

    public class DatabaseItemIntKeyDataIndexKey<TargetType> :
            DatabaseItemIntKeyIndexed<byte[], TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Set(object key, IBaseObject target)
        {
            Key = (int)key;
            IndexKey = null;
            Data = target.BinaryData;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyDataIndexKey<TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey);
        }
    }

    public class DatabaseItemStringKeyIndexed<IndexKeyType, TargetType> :
            DatabaseItemIndexed<string, IndexKeyType, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyIndexed<IndexKeyType, TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey);
        }
    }

    public class DatabaseItemStringKeyDataIndexKey<TargetType> :
            DatabaseItemStringKeyIndexed<byte[], TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Set(object key, IBaseObject target)
        {
            Key = key.ToString();
            IndexKey = null;
            Data = target.BinaryData;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyDataIndexKey<TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey);
        }
    }

    public class DatabaseItemIntKeyStringIndexKey<TargetType> :
            DatabaseItemIndexed<int, string, TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyStringIndexKey<TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, GetComparer(languageID, caseInsensitive));
        }

        public IComparer<string> GetComparer(LanguageID languageID, bool caseInsensitive)
        {
            IComparer<string> comparer = ApplicationData.GetLanguageComparer<string>(languageID, caseInsensitive);
            return comparer;
        }
    }

    public class DatabaseItemIntKeyOwnerLanguagesIndexKey<TargetType> :
            DatabaseItemIntKeyDataIndexKey<TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Set(object key, IBaseObject target)
        {
            Key = (int)key;
            BaseObjectLanguages indexKeySource = target as BaseObjectLanguages;
            BaseObjectLanguages indexKey;

            if (indexKeySource != null)
                indexKey = new BaseObjectLanguages(indexKeySource);
            else
                indexKey = new BaseObjectLanguages();

            IndexKey = indexKey.BinaryData;

            Data = target.BinaryData;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyOwnerLanguagesIndexKey<TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemStringKeyOwnerLanguagesIndexKey<TargetType> :
            DatabaseItemStringKeyDataIndexKey<TargetType>
        where TargetType : class, IBaseObject, new()
    {
        public override void Set(object key, IBaseObject target)
        {
            Key = key.ToString();
            BaseObjectLanguages indexKeySource = target as BaseObjectLanguages;
            BaseObjectLanguages indexKey;

            if (indexKeySource != null)
                indexKey = new BaseObjectLanguages(indexKeySource);
            else
                indexKey = new BaseObjectLanguages();

            IndexKey = indexKey.BinaryData;

            Data = target.BinaryData;
        }

        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemIntKeyOwnerLanguagesIndexKey<TargetType>>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemBaseString : DatabaseItemStringKey<BaseString>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemBaseString>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }

    public class DatabaseItemBaseStrings : DatabaseItemStringKey<BaseStrings>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemBaseStrings>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }

    public class DatabaseItemLanguageDescription : DatabaseItemStringKey<LanguageDescription>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemLanguageDescription>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemDictionaryEntry : DatabaseItemLanguageStringKey<DictionaryEntry>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemDictionaryEntry>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }

    public class DatabaseItemDeinflection : DatabaseItemLanguageStringKey<Deinflection>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemDeinflection>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }

    public class DatabaseItemBaseObjectTitled : DatabaseItemIntKeyOwnerLanguagesIndexKey<BaseObjectTitled>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemBaseObjectTitled>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemObjectReferenceTitled : DatabaseItemIntKeyOwnerLanguagesIndexKey<ObjectReferenceTitled>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemObjectReferenceTitled>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemObjectReferenceNodeTree : DatabaseItemIntKeyOwnerLanguagesIndexKey<ObjectReferenceNodeTree>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemObjectReferenceNodeTree>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemMarkupTemplate : DatabaseItemIntKeyOwnerLanguagesIndexKey<MarkupTemplate>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemMarkupTemplate>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemNodeMaster : DatabaseItemIntKeyOwnerLanguagesIndexKey<NodeMaster>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemNodeMaster>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemBaseObjectNodeTree : DatabaseItemIntKeyOwnerLanguagesIndexKey<BaseObjectNodeTree>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemBaseObjectNodeTree>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemContentStudyList : DatabaseItemIntKeyOwnerLanguagesIndexKey<ContentStudyList>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemContentStudyList>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemContentMediaItem : DatabaseItemIntKeyOwnerLanguagesIndexKey<ContentMediaItem>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemContentMediaItem>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemContentDocumentItem : DatabaseItemIntKeyOwnerLanguagesIndexKey<ContentDocumentItem>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemContentDocumentItem>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemSandbox : DatabaseItemStringKey<Sandbox>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemSandbox>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemUserRunItem : DatabaseItemStringKey<UserRunItem>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemUserRunItem>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemContentStatistics : DatabaseItemStringKey<ContentStatistics>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemContentStatistics>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemToolProfile : DatabaseItemStringKey<ToolProfile>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemToolProfile>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemToolStudyList : DatabaseItemStringKey<ToolStudyList>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemToolStudyList>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemToolSession : DatabaseItemStringKey<ToolSession>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemToolSession>()
              .Automap(i => i.Key, generateKeys);
        }
    }

    public class DatabaseItemImage : DatabaseItemIntKeyOwnerLanguagesIndexKey<Image>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemImage>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemAudioReference : DatabaseItemStringKeyOwnerLanguagesIndexKey<AudioReference>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemAudioReference>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemAudioMultiReference : DatabaseItemStringKeyOwnerLanguagesIndexKey<AudioMultiReference>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemAudioMultiReference>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemPictureReference : DatabaseItemStringKeyOwnerLanguagesIndexKey<PictureReference>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemPictureReference>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemUserRecord : DatabaseItemStringKey<UserRecord>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemUserRecord>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }

    public class DatabaseItemChangeLogItem : DatabaseItemIntKeyDataIndexKey<ChangeLogItem>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemChangeLogItem>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemForumCategory : DatabaseItemIntKeyOwnerLanguagesIndexKey<ForumCategory>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemForumCategory>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemForumHeading : DatabaseItemIntKeyOwnerLanguagesIndexKey<ForumHeading>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemForumHeading>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemForumTopic : DatabaseItemIntKeyOwnerLanguagesIndexKey<ForumTopic>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemForumTopic>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemForumPosting : DatabaseItemIntKeyOwnerLanguagesIndexKey<ForumPosting>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemForumPosting>()
              .Automap(i => i.Key, generateKeys)
              .WithIndex("Index", i => i.IndexKey, new ByteArrayComparer());
        }
    }

    public class DatabaseItemTreeCache : DatabaseItemStringKey<BaseString>
    {
        public override void Map(DbInstance db, bool generateKeys, LanguageID languageID, bool caseInsensitive)
        {
            db.Map<DatabaseItemTreeCache>()
              .Automap(i => i.Key, generateKeys, GetComparer(languageID, caseInsensitive));
        }
    }
}

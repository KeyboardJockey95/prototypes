using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Object
{
    public static class ObjectTypes
    {
        public static Type FindType(string className)
        {
            Type type = null;

            switch (className)
            {
                // Object

                case "BaseObject":
                    type = typeof(BaseObject);
                    break;
                case "BaseObjectKeyed":
                    type = typeof(BaseObjectKeyed);
                    break;
                case "BaseObjectLanguage":
                    type = typeof(BaseObjectLanguage);
                    break;
                case "BaseObjectLanguages":
                    type = typeof(BaseObjectLanguages);
                    break;
                case "BaseObjectTitled":
                    type = typeof(BaseObjectTitled);
                    break;
                case "BaseString":
                    type = typeof(BaseString);
                    break;
                case "BaseStrings":
                    type = typeof(BaseStrings);
                    break;
                case "LanguageString":
                    type = typeof(LanguageString);
                    break;
                case "MultiLanguageString":
                    type = typeof(MultiLanguageString);
                    break;
                case "ObjectReferenceTitled":
                    type = typeof(ObjectReferenceTitled);
                    break;
                case "ObjectReferenceNodeTree":
                    type = typeof(ObjectReferenceNodeTree);
                    break;
                case "LanguageID":
                    type = typeof(LanguageID);
                    break;
                case "LanguageDescription":
                    type = typeof(LanguageDescription);
                    break;

                // Matchers

                case "CompoundMatcher":
                    type = typeof(CompoundMatcher);
                    break;
                case "FloatMatcher":
                    type = typeof(FloatMatcher);
                    break;
                case "GuidMatcher":
                    type = typeof(GuidMatcher);
                    break;
                case "IntMatcher":
                    type = typeof(IntMatcher);
                    break;
                case "LanguageIDMatcher":
                    type = typeof(LanguageIDMatcher);
                    break;
                case "LanguageStringMatcher":
                    type = typeof(LanguageStringMatcher);
                    break;
                case "Matcher":
                    type = typeof(Matcher);
                    break;
                case "StringMatcher":
                    type = typeof(StringMatcher);
                    break;

                // Markup

                case "MarkupTemplate":
                    type = typeof(MarkupTemplate);
                    break;

                // Master

                case "NodeMaster":
                    type = typeof(NodeMaster);
                    break;

                // Content

                case "BaseObjectContent":
                    type = typeof(BaseObjectContent);
                    break;
                case "ContentMediaItem":
                    type = typeof(ContentMediaItem);
                    break;
                case "ContentStudyList":
                    type = typeof(ContentStudyList);
                    break;
                case "ContentDocumentItem":
                    type = typeof(ContentDocumentItem);
                    break;

                // Node

                case "BaseObjectNode":
                    type = typeof(BaseObjectNode);
                    break;
                case "BaseObjectNodeTree":
                    type = typeof(BaseObjectNodeTree);
                    break;
                case "Sandbox":
                    type = typeof(Sandbox);
                    break;

                // Tool

                case "ToolSession":
                    type = typeof(ToolSession);
                    break;
                case "ToolProfile":
                    type = typeof(ToolProfile);
                    break;
                case "ToolStudyList":
                    type = typeof(ToolStudyList);
                    break;

                // Dictionary

                case "DictionaryEntry":
                    type = typeof(DictionaryEntry);
                    break;
                case "Sense":
                    type = typeof(Sense);
                    break;
                case "LanguageSynonyms":
                    type = typeof(LanguageSynonyms);
                    break;

                // Forum

                case "ForumCategory":
                    type = typeof(ForumCategory);
                    break;
                case "ForumHeading":
                    type = typeof(ForumHeading);
                    break;
                case "ForumTopic":
                    type = typeof(ForumTopic);
                    break;
                case "ForumPosting":
                    type = typeof(ForumPosting);
                    break;

                // Media

                case "AudioReference":
                    type = typeof(AudioReference);
                    break;
                case "AudioMultiReference":
                    type = typeof(AudioMultiReference);
                    break;
                case "PictureReference":
                    type = typeof(PictureReference);
                    break;

                // Admin

                case "UserID":
                    type = typeof(UserID);
                    break;
                case "UserRecord":
                    type = typeof(UserRecord);
                    break;
                case "UserProfile":
                    type = typeof(UserProfile);
                    break;

                // Others.

                case "Guid":
                    type = typeof(Guid);
                    break;
                case "DateTime":
                    type = typeof(DateTime);
                    break;
                case "TimeSpan":
                    type = typeof(TimeSpan);
                    break;
                case "NodeTreeRepository":
                    type = typeof(NodeTreeRepository);
                    break;
                case "Deinflection":
                    type = typeof(Deinflection);
                    break;
                case "DeinflectionInstance":
                    type = typeof(DeinflectionInstance);
                    break;
                default:
                    throw new ObjectException("BaseObject.FindType:  Class " + className + " not found.");
            }

            return type;
        }

        public static object GetMemberValue(object obj, string memberName)
        {
            if (obj == null)
                return null;

            switch (memberName)
            {
                case "Key":
                    if (obj is IBaseObjectKeyed)
                        return ((IBaseObjectKeyed)obj).Key;
                    break;
                case "KeyString":
                    if (obj is IBaseObjectKeyed)
                        return ((IBaseObjectKeyed)obj).KeyString;
                    break;
                case "Guid":
                    if (obj is IBaseObjectKeyed)
                        return ((IBaseObjectKeyed)obj).Guid;
                    break;
                case "Name":
                    if (obj is IBaseObjectKeyed)
                        return ((IBaseObjectKeyed)obj).Name;
                    break;
                case "Owner":
                    if (obj is IBaseObjectKeyed)
                        return ((IBaseObjectKeyed)obj).Owner;
                    break;
                case "LanguageID":
                    if (obj is IBaseObjectLanguage)
                        return ((IBaseObjectLanguage)obj).LanguageID;
                    break;
                case "Text":
                    if (obj is BaseString)
                        return ((BaseString)obj).Text;
                    else if (obj is LanguageString)
                        return ((LanguageString)obj).Text;
                    break;
                case "LanguageIDs":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).LanguageIDs;
                    break;
                case "TargetLanguageIDs":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).TargetLanguageIDs;
                    break;
                case "HostLanguageIDs":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).HostLanguageIDs;
                    break;
                case "LanguagesKey":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).LanguagesKey;
                    break;
                case "TargetLanguagesKey":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).TargetLanguagesKey;
                    break;
                case "HostLanguagesKey":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).HostLanguagesKey;
                    break;
                case "LanguagesKeyExpanded":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).LanguagesKeyExpanded;
                    break;
                case "TargetLanguagesKeyExpanded":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).TargetLanguagesKeyExpanded;
                    break;
                case "HostLanguagesKeyExpanded":
                    if (obj is BaseObjectLanguages)
                        return ((BaseObjectLanguages)obj).HostLanguagesKeyExpanded;
                    break;
                case "UserRole":
                    if (obj is UserRecord)
                        return ((UserRecord)obj).UserRole;
                    break;
                case "HeadingKey":
                    if (obj is ForumTopic)
                        return ((ForumTopic)obj).HeadingKey;
                    break;
                case "TopicKey":
                    if (obj is ForumPosting)
                        return ((ForumPosting)obj).TopicKey;
                    break;
                default:
                    throw new ObjectException("BaseObject.GetMemberValue:  Member " + memberName + " not found.");
            }

            return null;
        }
    }
}

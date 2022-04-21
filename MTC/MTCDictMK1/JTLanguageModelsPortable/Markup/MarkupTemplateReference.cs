using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Markup
{
    public class MarkupTemplateReference : ObjectReferenceNamed<MarkupTemplate>
    {
        public MarkupTemplateReference(MarkupTemplate item)
            : base(
                (item != null ? item.Key : null),
                "MarkupTemplates",
                ((item != null) && (item.Title != null) && (item.Title.LanguageString(LanguageLookup.English) != null)
                    ? item.Title.Text(LanguageLookup.English)
                    : null),
                ((item != null) ? item.Owner : null),
                item)
        {
        }

        public MarkupTemplateReference(object key, string name, string owner)
            : base(key, "MarkupTemplates", name, owner, null)
        {
        }

        public MarkupTemplateReference(MarkupTemplateReference other)
            : base(other)
        {
        }

        public MarkupTemplateReference(XElement element)
            : base(element)
        {
        }

        public MarkupTemplateReference()
            : base("(none)", "MarkupTemplates", "(none)", null, null)
        {
        }

        public override IBaseObject Clone()
        {
            return new MarkupTemplateReference(this);
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            string xmlKey = KeyString;

            if (!String.IsNullOrEmpty(xmlKey) && !xmlKey.StartsWith("("))
            {
                IBaseObjectKeyed target = fixups.Get(Source, xmlKey);

                if (target != null)
                {
                    Key = target.Key;
                    _Item = (MarkupTemplate)target;
                }
                else
                    ResolveReference(fixups.Repositories);
            }
        }
    }
}

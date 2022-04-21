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
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class ContentDocumentItem : BaseContentStorage
    {

        public ContentDocumentItem(object key, BaseObjectContent content,
                List<IBaseObjectKeyed> options, MarkupTemplate markupTemplate,
                MarkupTemplateReference markupReference)
            : base(key, "DocumentItems", content)
        {
            _Options = options;
            _LocalMarkupTemplate = markupTemplate;
            _MarkupReference = markupReference;
        }

        public ContentDocumentItem(object key)
            : base(key, "DocumentItems", null)
        {
            ClearContentDocumentItem();
        }

        public ContentDocumentItem(ContentDocumentItem other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ContentDocumentItem(XElement element)
        {
            OnElement(element);
        }

        public ContentDocumentItem()
        {
            ClearContentDocumentItem();
        }

        public void Copy(ContentDocumentItem other)
        {
            base.Copy(other);
        }

        public void CopyDeep(ContentDocumentItem other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentDocumentItem();
        }

        public void ClearContentDocumentItem()
        {
            _Source = "DocumentItems";
        }

        public override IBaseObject Clone()
        {
            return new ContentDocumentItem(this);
        }

        public override ContentClassType ContentClass
        {
            get
            {
                return ContentClassType.DocumentItem;
            }
        }

        public static List<OptionDescriptor> GetDefaultDescriptors(string contentType, string contentSubType,
            UserProfile userProfile)
        {
            string otherTeachersCanEdit = "Inherit";
            List<OptionDescriptor> newOptionDescriptors = new List<OptionDescriptor>()
                {
                    new OptionDescriptor("ShowTitle", "flag", "Show title",
                        "This option determines whether the normal page title should be displayed.", "true"),
                    new OptionDescriptor("ShowComponentOptions", "flag", "Show component options",
                        "This option determines whether the component options panel will be optionally displayed.", "false"),
                    new OptionDescriptor("ShowTargetLanguageSelect", "flag", "Show target and host language select",
                        "This option determines whether the target and host language drop-down menus will be displayed.", "false"),
                    new OptionDescriptor("ShowLanguageLinks", "flag", "Show language links",
                        "This option determines whether the show/hide language links will be displayed.", "true"),
                    new OptionDescriptor("StringDisplayLanguage", "namedLanguage", "String display language",
                        "This option determines string display language.", "UI"),
                    new OptionDescriptor("TargetDisplayLanguage", "namedLanguage", "Target display language",
                        "This option determines the default language label of the content items.", "Target"),
                    CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                    //new OptionDescriptor("HtmlHeadings", "string", "Html headings",
                    //    "Use this to add some additional HTML headings.", "")
                };
            return newOptionDescriptors;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.SaveReferences(mainRepository, recurseParents, recurseChildren);
            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReferences(mainRepository, recurseParents, recurseChildren);
            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren);
            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseParents, recurseChildren);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<string, bool> itemKeyFlags)
        {
            XElement element = base.GetElementFiltered(name, itemKeyFlags);

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                default:
                    return base.OnAttribute(attribute);
            }

            //return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                default:
                    return base.OnChildElement(childElement);
            }

            //return true;
        }

        public static int Compare(ContentDocumentItem item1, ContentDocumentItem item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            int diff = item1.Compare(item2);
            return diff;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ContentDocumentItem otherContentDocumentItem = other as ContentDocumentItem;
            int diff;

            if (otherContentDocumentItem != null)
            {
                diff = base.Compare(other);

                return diff;
            }

            return base.Compare(other);
        }

        public static int CompareContentDocumentItemLists(List<ContentDocumentItem> list1, List<ContentDocumentItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ContentDocumentItem>(list1, list2);
        }
    }
}

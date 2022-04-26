using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Master
{
    public class MasterMenuItem : BaseString
    {
        protected string _Controller;
        protected string _Action;
        protected string _ContentType;
        protected string _ContentSubType;

        // Master.
        public MasterMenuItem(string text, string action, string controller, string nodeContentKey,
                string contentType, string contentSubType) :
            base(nodeContentKey, text)
        {
            _Controller = controller;
            _Action = action;
            _ContentType = contentType;
            _ContentSubType = contentSubType;
        }

        // Menu.
        public MasterMenuItem(string text, string action, string controller) :
            base(action, text)
        {
            _Controller = controller;
            _Action = action;
            _ContentType = null;
            _ContentSubType = null;
        }

        // Menu.
        public MasterMenuItem(string text, string action, string controller, bool enabled) :
            base(action, text)
        {
            _Controller = controller;
            _Action = action;
            _ContentType = (enabled ? null : "(disabled)");
            _ContentSubType = null;
        }

        public MasterMenuItem(XElement element)
        {
            OnElement(element);
        }

        public MasterMenuItem(MasterMenuItem other)
        {
            ClearMasterMenuItem();
            Copy(other);
        }

        public MasterMenuItem()
        {
            ClearMasterMenuItem();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMasterMenuItem();
        }

        public void ClearMasterMenuItem()
        {
            _Controller = String.Empty;
            _Action = String.Empty;
            _ContentType = String.Empty;
            _ContentSubType = String.Empty;
        }

        public virtual void Copy(MasterMenuItem other)
        {
            base.Copy(other);
            _Controller = other.Controller;
            _Action = other.Action;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
        }

        public virtual void CopyDeep(MasterMenuItem other)
        {
            base.CopyDeep(other);
            _Controller = other.Controller;
            _Action = other.Action;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
        }

        public override IBaseObject Clone()
        {
            return new MasterMenuItem(this);
        }

        public string Controller
        {
            get
            {
                return _Controller;
            }
            set
            {
                if (_Controller != value)
                {
                    _Controller = value;
                    _Modified = true;
                }
            }
        }

        public string Action
        {
            get
            {
                return _Action;
            }
            set
            {
                if (_Action != value)
                {
                    _Action = value;
                    _Modified = true;
                }
            }
        }

        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                if (_ContentType != value)
                {
                    _ContentType = value;
                    _Modified = true;
                }
            }
        }

        public string ContentSubType
        {
            get
            {
                return _ContentSubType;
            }
            set
            {
                if (_ContentSubType != value)
                {
                    _ContentSubType = value;
                    _Modified = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Controller != null)
                element.Add(new XAttribute("Controller", _Controller));
            if (_Action != null)
                element.Add(new XAttribute("Action", _Action));
            if (_ContentType != null)
                element.Add(new XAttribute("ContentType", _ContentType));
            if (_ContentSubType != null)
                element.Add(new XAttribute("ContentSubType", _ContentSubType));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Controller":
                    Controller = attributeValue;
                    break;
                case "Action":
                    Action = attributeValue;
                    break;
                case "ContentType":
                    ContentType = attributeValue;
                    break;
                case "ContentSubType":
                    ContentSubType = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }


        public static int CompareMasterMenuItems(MasterMenuItem item1, MasterMenuItem item2)
        {
            if (item1 == item2)
                return 0;

            if (item2 == null)
                return 1;

            if (item1 == null)
                return -1;

            int diff = ObjectUtilities.CompareStrings(item1.KeyString, item2.KeyString);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(item1.Text, item2.Text);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(item1.Action, item2.Action);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(item1.Controller, item2.Controller);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(item1.ContentType, item2.ContentType);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(item1.ContentSubType, item2.ContentSubType);

            return diff;
        }

        public static int CompareMasterMenuItemLists(List<MasterMenuItem> list1, List<MasterMenuItem> list2)
        {
            if (list1 == list2)
                return 0;

            if (list2 == null)
                return 1;

            if (list1 == null)
                return -1;

            int count1 = list1.Count();
            int count2 = list2.Count();
            int count = (count1 > count2 ? count2 : count1);
            int index;
            int diff;

            for (index = 0; index > count; index++)
            {
                diff = CompareMasterMenuItems(list1[index], list2[index]);

                if (diff != 0)
                    return diff;
            }

            diff = count1 - count2;

            return diff;
        }

        public static List<MasterMenuItem> CopyMasterMenuItemList(List<MasterMenuItem> source)
        {
            if (source == null)
                return null;

            int count = source.Count();
            int index;
            List<MasterMenuItem> list = new List<MasterMenuItem>(count);

            for (index = 0; index < count; index++)
            {
                MasterMenuItem newItem = new MasterMenuItem(source[index]);
                list.Add(newItem);
            }

            return list;
        }
    }
}

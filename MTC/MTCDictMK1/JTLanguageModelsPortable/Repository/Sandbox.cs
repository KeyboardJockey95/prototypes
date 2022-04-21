using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Repository
{
    public class Sandbox : ContentStudyList
    {
        protected int _TreeKey;
        protected List<BaseContentStorage> _ContentStorageList;

        public Sandbox(object key)
            : base(key, "Sandboxes")
        {
            ClearSandbox();
            Owner = key.ToString();
        }

        public Sandbox(Sandbox other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public Sandbox(XElement element)
        {
            OnElement(element);
        }

        public Sandbox()
        {
            ClearSandbox();
        }

        public void Copy(Sandbox other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearSandbox();
                return;
            }

            _TreeKey = other.TreeKey;
        }

        public void CopyDeep(Sandbox other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearSandbox();
        }

        public void ClearSandbox()
        {
            _TreeKey = 0;
            _Source = "Sandboxes";
        }

        public override IBaseObject Clone()
        {
            return new Sandbox(this);
        }

        public int TreeKey
        {
            get
            {
                return _TreeKey;
            }
            set
            {
                if (_TreeKey != value)
                {
                    _TreeKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override string MediaTildeUrl
        {
            get
            {
                return ApplicationData.SandboxTildeUrl + "/" + KeyString + "/Media";
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_TreeKey != 0)
                element.Add(new XElement("TreeKey", _TreeKey.ToString()));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "TreeKey":
                    _TreeKey = ObjectUtilities.GetIntegerFromString(childElement.Value, 0);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

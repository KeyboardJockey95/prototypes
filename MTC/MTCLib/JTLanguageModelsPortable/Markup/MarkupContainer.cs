using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Markup;

namespace JTLanguageModelsPortable.Markup
{
    public class MarkupContainer : BaseObjectKeyed
    {
        protected MarkupTemplate _LocalMarkupTemplate;
        protected MarkupTemplateReference _MarkupReference;
        public static List<string> MarkupUseStrings = new List<string>()
        {
            "Normal", "Top Notes", "Bottom Notes"
        };

        public MarkupContainer(object key, MarkupTemplate markupTemplate,
            MarkupTemplateReference markupReference)
        {
            _LocalMarkupTemplate = markupTemplate;
            _MarkupReference = markupReference;
        }

        public MarkupContainer(MarkupContainer other)
            : base(other)
        {
            Copy(other);
            _Modified = false;
        }

        public MarkupContainer(XElement element)
        {
            OnElement(element);
        }

        public MarkupContainer()
        {
            ClearMarkupContainer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMarkupContainer();
        }

        public void ClearMarkupContainer()
        {
            _LocalMarkupTemplate = null;
            _MarkupReference = null;
        }

        public void Copy(MarkupContainer other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearMarkupContainer();
                return;
            }

            if (other.LocalMarkupTemplate != null)
                _LocalMarkupTemplate = new MarkupTemplate(other.LocalMarkupTemplate);
            else
                _LocalMarkupTemplate = null;

            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;
        }

        public void CopyDeep(MarkupContainer other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new MarkupContainer(this);
        }

        public bool HasMarkup
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    switch (_MarkupReference.KeyString)
                    {
                        case "(none)":
                            return false;
                        case "(local)":
                            if (_LocalMarkupTemplate != null)
                                return true;
                            return false;
                        default:
                            return true;
                    }
                }
                return false;
            }
        }

        public bool HasMarkupReference
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    if (!_MarkupReference.KeyString.StartsWith("("))
                        return true;
                }
                return false;
            }
        }

        public MarkupTemplateReference MarkupReference
        {
            get
            {
                return _MarkupReference;
            }
            set
            {
                if (value != _MarkupReference)
                {
                    _MarkupReference = value;
                    _Modified = true;
                }
            }
        }

        public MarkupTemplateReference CloneMarkupReference()
        {
            if (_MarkupReference != null)
                return new MarkupTemplateReference(_MarkupReference);
            else
                return null;
        }

        public MarkupTemplate LocalMarkupTemplate
        {
            get
            {
                return _LocalMarkupTemplate;
            }
            set
            {
                if (value != _LocalMarkupTemplate)
                {
                    _LocalMarkupTemplate = value;
                    _Modified = true;
                }
            }
        }

        public MarkupTemplate CloneMarkupTemplate()
        {
            if (_LocalMarkupTemplate != null)
                return new MarkupTemplate(_LocalMarkupTemplate);
            else
                return null;
        }

        public MarkupTemplate MarkupTemplate
        {
            get
            {
                if ((_MarkupReference != null) && (_MarkupReference.Key != null))
                {
                    switch (_MarkupReference.KeyString)
                    {
                        case "(none)":
                            return null;
                        case "(local)":
                            return _LocalMarkupTemplate;
                        default:
                            return _MarkupReference.Item;
                    }
                }
                return _LocalMarkupTemplate;
            }
            set
            {
                if (value != _LocalMarkupTemplate)
                {
                    _LocalMarkupTemplate = value;
                    _Modified = true;
                }
                if (value != null)
                {
                    if (_MarkupReference != null)
                    {
                        if (_MarkupReference.Key != value.Key)
                        {
                            _MarkupReference.Key = value.Key;
                            _MarkupReference.Item = value;
                            _Modified = true;
                        }
                    }
                    else
                        _MarkupReference = new MarkupTemplateReference(value);
                }
                else
                    _MarkupReference = null;
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_LocalMarkupTemplate != null)
                {
                    if (_LocalMarkupTemplate.Modified)
                        return true;
                }

                if (_MarkupReference != null)
                {
                    if (_MarkupReference.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_LocalMarkupTemplate != null)
                    _LocalMarkupTemplate.Modified = false;

                if (_MarkupReference != null)
                    _MarkupReference.Modified = false;
            }
        }

        public virtual void ResolveReferences(IMainRepository mainRepository)
        {
            if ((_MarkupReference != null) && (_MarkupReference.Key != null)
                    && !_MarkupReference.KeyString.StartsWith("(") && (_MarkupReference.Item == null))
                _MarkupReference.ResolveReference(mainRepository);
        }

        public virtual bool SaveReferences(IMainRepository mainRepository)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                returnValue = _MarkupReference.SaveReference(mainRepository);

            return returnValue;
        }

        public virtual bool UpdateReferences(IMainRepository mainRepository)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                returnValue = _MarkupReference.UpdateReference(mainRepository);

            return returnValue;
        }

        public virtual bool UpdateReferencesCheck(IMainRepository mainRepository)
        {
            bool returnValue = true;

            if ((_MarkupReference != null) && (_MarkupReference.Key != null) && !_MarkupReference.KeyString.StartsWith("("))
                returnValue = _MarkupReference.UpdateReferenceCheck(mainRepository);

            return returnValue;
        }

        public virtual void ClearReferences()
        {
            if ((_MarkupReference != null))
                _MarkupReference.ClearReference();
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_LocalMarkupTemplate != null)
                element.Add(_LocalMarkupTemplate.GetElement("LocalMarkupTemplate"));

            if (_MarkupReference != null)
                element.Add(_MarkupReference.GetElement("MarkupReference"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "LocalMarkupTemplate":
                    _LocalMarkupTemplate = new MarkupTemplate(childElement);
                    if (String.IsNullOrEmpty(_LocalMarkupTemplate.Owner))
                        _LocalMarkupTemplate.Owner = Owner;
                    break;
                case "MarkupReference":
                    _MarkupReference = new MarkupTemplateReference(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}

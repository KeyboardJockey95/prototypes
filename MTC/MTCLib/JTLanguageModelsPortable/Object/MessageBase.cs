using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Object
{
    public class MessageBase : BaseObject
    {
        protected int _MessageID;
        protected UserID _UserID;
        protected bool _RequiresAuthentication;
        protected string _MessageTarget;
        protected string _MessageName;
        protected List<IBaseObject> _Arguments;
        protected static int _MessageCounter;

        public MessageBase(int messageID, UserID userID, bool requiresAuthentication, string messageTarget, string messageName, List<IBaseObject> arguments)
        {
            if (messageID < 0)
                _MessageID = ++_MessageCounter;
            else
                _MessageID = messageID;
            _UserID = userID;
            _RequiresAuthentication = requiresAuthentication;
            _MessageTarget = messageTarget;
            _MessageName = messageName;
            _Arguments = arguments;
        }

        public MessageBase(MessageBase other)
        {
            CopyMessageBase(other);
        }

        public MessageBase(XElement element)
        {
            OnElement(element);
        }

        public MessageBase()
        {
            ClearMessageBase();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMessageBase();
        }

        public void ClearMessageBase()
        {
            _MessageID = 0;
            _UserID = null;
            _RequiresAuthentication = false;
            _MessageTarget = null;
            _MessageName = null;
            _Arguments = null;
        }

        public virtual void CopyMessageBase(MessageBase other)
        {
            _MessageID = other.MessageID;
            _UserID = other.UserID;
            _RequiresAuthentication = other.RequiresAuthentication;
            _MessageTarget = other.MessageTarget;
            _MessageName = other.MessageName;

            if (other._Arguments != null)
                _Arguments = other.Arguments;
            else
                _Arguments = null;
        }

        public int MessageID
        {
            get
            {
                return _MessageID;
            }
            set
            {
                _MessageID = value;
            }
        }

        public bool RequiresAuthentication
        {
            get
            {
                return _RequiresAuthentication;
            }
            set
            {
                _RequiresAuthentication = value;
            }
        }

        public UserID UserID
        {
            get
            {
                return _UserID;
            }
            set
            {
                _UserID = value;
            }
        }

        public string MessageTarget
        {
            get
            {
                return _MessageTarget;
            }
            set
            {
                _MessageTarget = value;
            }
        }

        public string MessageName
        {
            get
            {
                return _MessageName;
            }
            set
            {
                _MessageName = value;
            }
        }

        public List<IBaseObject> Arguments
        {
            get
            {
                return _Arguments;
            }
            set
            {
                _Arguments = value;
            }
        }

        public List<IBaseObjectKeyed> BaseArguments
        {
            get
            {
                if (_Arguments != null)
                    return _Arguments.Cast<IBaseObjectKeyed>().ToList();
                return null;
            }
            set
            {
                if (value != null)
                    _Arguments = value.Cast<IBaseObject>().ToList();
                else
                    _Arguments = null;
            }
        }

        public List<object> BaseArgumentKeys
        {
            get
            {
                List<object> keys = new List<object>();
                if (_Arguments != null)
                {
                    foreach (IBaseObjectKeyed obj in _Arguments)
                        keys.Add(obj.Key);
                    return keys;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    List<IBaseObjectKeyed> objs = new List<IBaseObjectKeyed>();
                    foreach (object key in value)
                        objs.Add(new BaseObjectKeyed(key));
                    _Arguments = objs.Cast<IBaseObject>().ToList();
                }
                else
                    _Arguments = null;
            }
        }

        public IBaseObject GetArgumentIndexed(int index)
        {
            if (_Arguments != null)
                return _Arguments[index];
            return null;
        }

        public IBaseObjectKeyed GetBaseArgumentIndexed(int index)
        {
            if (_Arguments != null)
                return (IBaseObjectKeyed)_Arguments[index];
            return null;
        }

        public int GetArgumentCount()
        {
            if (_Arguments != null)
                return _Arguments.Count();
            return 0;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("MessageID", _MessageID));
            if (_MessageTarget != null)
                element.Add(new XAttribute("MessageTarget", _MessageTarget));
            if (_MessageName != null)
                element.Add(new XAttribute("MessageName", _MessageName));
            if (_UserID != null)
                element.Add(_UserID.GetElement("UserID"));
            element.Add(new XAttribute("RequiresAuthentication", _RequiresAuthentication.ToString()));
            if ((_Arguments != null) && (_Arguments.Count() != 0))
            {
                XElement subElement = new XElement("Arguments");
                foreach (IBaseObject argument in _Arguments)
                    subElement.Add(argument.Xml);
                element.Add(subElement);
            }
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "MessageID":
                    MessageID = Convert.ToInt32(attributeValue);
                    break;
                case "MessageTarget":
                    MessageTarget = attributeValue;
                    break;
                case "MessageName":
                    MessageName = attributeValue;
                    break;
                case "RequiresAuthentication":
                    RequiresAuthentication = (attributeValue == "True" ? true : false);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Arguments":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        _Arguments = new List<IBaseObject>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(subElement);
                            if (obj != null)
                                _Arguments.Add(obj);
                        }
                    }
                    break;
                case "UserID":
                    _UserID = new UserID(childElement);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}

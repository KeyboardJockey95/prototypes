using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Admin
{
    public class Quota : BaseObjectKeyed
    {
        // The limit for this quota.
        protected int _Limit;
        // The current count for this quota.
        protected int _Count;
        // The number of days for the period of this quota.
        protected int _PeriodDays;
        // The expiration time for this quota.
        protected DateTime _Expiration;

        public Quota(
            string name,
            int limit,
            int count,
            int periodDays,
            DateTime expiration) : base(name)
        {
            _Limit = limit;
            _Count = count;
            _PeriodDays = periodDays;
            _Expiration = expiration;
        }

        public Quota(Quota other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public Quota(XElement element)
        {
            OnElement(element);
        }

        public Quota()
        {
            ClearQuota();
        }

        public override void Clear()
        {
            base.Clear();
            ClearQuota();
        }

        public void ClearQuota()
        {
            _Limit = -1;
            _Count = 0;
            _PeriodDays = 0;
            _Expiration = DateTime.MaxValue;
        }

        public void CopyQuota(Quota other)
        {
            _Limit = other.Limit;
            _Count = other.Count;
            _PeriodDays = other.PeriodDays;
            _Expiration = other.Expiration;
        }

        public int Limit
        {
            get
            {
                return _Limit;
            }
            set
            {
                if (value != _Limit)
                {
                    _Limit = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                if (value != _Count)
                {
                    _Count = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int PeriodDays
        {
            get
            {
                return _PeriodDays;
            }
            set
            {
                if (value != _PeriodDays)
                {
                    _PeriodDays = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual DateTime Expiration
        {
            get
            {
                return _Expiration;
            }
            set
            {
                if (_Expiration != value)
                {
                    _Expiration = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Limit", _Limit.ToString()));
            element.Add(new XAttribute("Count", _Count.ToString()));
            element.Add(new XAttribute("PeriodDays", _PeriodDays.ToString()));
            element.Add(new XAttribute("Expiration", ObjectUtilities.GetStringFromDateTime(_Expiration)));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Limit":
                    _Limit = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "Count":
                    _Count = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "PeriodDays":
                    _PeriodDays = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "Expiration":
                    _Expiration = ObjectUtilities.GetDateTimeFromString(attributeValue, DateTime.MaxValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Tool
{
    public enum ToolItemStatusCode
    {
        Future,                                         // Item hasn't been seen yet.
        Active,                                         // Item has been seen but not learned.
        Learned                                         // Item has been learned.
    }

    public class ToolItemStatus : BaseObjectKeyed
    {
        protected ToolItemStatusCode _StatusCode;       // Indication of status (Learned, Active, Future).
        protected List<float> _Grades;                  // Grade history queue.
        protected int _TouchCount;                      // How many times touched.
        protected DateTime _FirstTouchTime;             // First time touched.
        protected DateTime _LastTouchTime;              // Last time touched.
        protected DateTime _NextTouchTime;              // Scheduled time for review.
        protected MultiLanguageString _LastTextInput;   // Last text input.
        protected int _Stage;                           // Stage in progression, starting at 0.
        protected int _Index;                           // Sequential or random index.
        protected List<int> _ChoiceIndices;             // Multiple choice item indices.
        protected int _CorrectChoiceIndex;              // Last correct choice index.
        protected int _ChosenChoiceIndex;               // Last chosen choice index.
        protected List<int> _WordBlanksIndices;         // Last fill-in-the-blanks indices.
        protected string _SubConfigurationKey;          // Sub-configuration key for hybrid.

        public ToolItemStatus(object key, ToolItemStatusCode statusCode, List<float> grades, int touchCount,
                DateTime firstTouchTime, DateTime lastTouchTime, DateTime nextTouchTime, MultiLanguageString lastTextInput,
                int stage, int index)
            : base(key)
        {
            _StatusCode = statusCode;
            _Grades = grades;
            if (grades == null)
                _Grades = new List<float>();
            _TouchCount = touchCount;
            _FirstTouchTime = firstTouchTime;
            _LastTouchTime = lastTouchTime;
            _NextTouchTime = nextTouchTime;
            _LastTextInput = lastTextInput;
            _Stage = stage;
            _Index = index;
            _ChoiceIndices = null;
            _CorrectChoiceIndex = -1;
            _ChosenChoiceIndex = -1;
            _WordBlanksIndices = null;
            _SubConfigurationKey = null;
        }

        public ToolItemStatus(object key, ToolItemStatusCode statusCode)
            : base(key)
        {
            ClearToolItemStatus();
            _StatusCode = statusCode;
        }

        public ToolItemStatus(XElement element)
        {
            OnElement(element);
        }

        public ToolItemStatus(ToolItemStatus other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ToolItemStatus()
        {
            ClearToolItemStatus();
        }

        public override void Clear()
        {
            base.Clear();
            ClearToolItemStatus();
        }

        public void ClearToolItemStatus()
        {
            _StatusCode = ToolItemStatusCode.Future;
            _Grades = new List<float>();
            _TouchCount = 0;
            _FirstTouchTime = DateTime.MinValue;
            _LastTouchTime = DateTime.MinValue;
            _NextTouchTime = DateTime.MinValue;
            _LastTextInput = null;
            _Stage = 0;
            _Index = -1;
            _ChoiceIndices = null;
            _CorrectChoiceIndex = -1;
            _ChosenChoiceIndex = -1;
            _WordBlanksIndices = null;
            _SubConfigurationKey = null;
        }

        public void Copy(ToolItemStatus other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            _StatusCode = other.StatusCode;
            _Grades = new List<float>(other.Grades);
            _TouchCount = other.TouchCount;
            _FirstTouchTime = other.FirstTouchTime;
            _LastTouchTime = other.LastTouchTime;
            _NextTouchTime = other.NextTouchTime;

            if (other.LastTextInput != null)
                _LastTextInput = new MultiLanguageString(other.LastTextInput);
            else
                _LastTextInput = null;

            _Stage = other.Stage;
            _Index = other.Index;
            _ChoiceIndices = other.ChoiceIndices;
            _CorrectChoiceIndex = other.CorrectChoiceIndex;
            _ChosenChoiceIndex = other.ChosenChoiceIndex;
            _WordBlanksIndices = other.WordBlanksIndices;
            _SubConfigurationKey = other.SubConfigurationKey;
        }

        public void CopyDeep(ToolItemStatus other)
        {
            base.CopyDeep(other);
            Copy(other);
        }

        public ToolItemStatusCode StatusCode               // The flash item's status.
        {
            get
            {
                return _StatusCode;
            }
            set
            {
                if (value != _StatusCode)
                {
                    _StatusCode = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public List<float> Grades                        // Grade history.
        {
            get
            {
                return _Grades;
            }
            set
            {
                if (value != _Grades)
                {
                    _Grades = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public string GradeArrayString(string prefix, string separator, string suffix)
        {
            if ((_Grades == null) || (_Grades.Count() == 0))
                return String.Empty;

            string returnValue = String.Empty;

            if (!String.IsNullOrEmpty(prefix))
                returnValue += prefix;

            int c = _Grades.Count();

            for (int i = 0; i < c; i++)
            {
                if ((i != 0) && !String.IsNullOrEmpty(separator))
                    returnValue += separator;

                returnValue += _Grades[c - i - 1].ToString();
            }

            if (!String.IsNullOrEmpty(suffix))
                returnValue += suffix;

            return returnValue;
        }

        public void ResetGrade(float grade)
        {
            if (_Grades == null)
                _Grades = new List<float>(1) { grade };
            else
            {
                _Grades.Clear();
                _Grades.Add(grade);
            }
            ModifiedFlag = true;
            Touch();
        }

        public int GradeCount                           // How many times graded.
        {
            get
            {
                return _Grades.Count();
            }
            set
            {
                int count = _Grades.Count();

                if (value <= count)
                {
                    while (count > value)
                    {
                        count--;
                        _Grades.RemoveAt(count);
                    }

                    ModifiedFlag = true;
                    Touch();
                }
                else if (value > count)
                {
                    float grade = Grade;

                    while (value > count)
                    {
                        _Grades.Add(grade);
                        count++;
                    }

                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public float Grade                              // Running indication of how well known.
        {
            get
            {
                if (_Grades.Count() == 0)
                    return 0.0f;

                float sum = 0.0f;

                foreach (float grade in _Grades)
                    sum += grade;

                return sum / _Grades.Count();
            }
            set
            {
                _Grades.Insert(0, value);

                if (_StatusCode == ToolItemStatusCode.Future)
                    _StatusCode = ToolItemStatusCode.Active;

                ModifiedFlag = true;
                Touch();
            }
        }

        public int RoundedIntegerGrade                  // Assuming one decimal point.
        {
            get
            {
                return (int)(Grade + 0.4f);
            }
        }

        public float LastGrade                          // Last touch grade.
        {
            get
            {
                if (_Grades.Count() == 0)
                    return 0.0f;

                return _Grades.FirstOrDefault();
            }
        }

        public float PreviousToLastGrade                 // Previous to last touch grade.
        {
            get
            {
                if (_Grades.Count() <= 1)
                    return 0.0f;

                return _Grades[1];
            }
        }

        public float GradeSum                           // Sum of grades.
        {
            get
            {
                if (_Grades.Count() == 0)
                    return 0.0f;

                float sum = 0.0f;

                foreach (float grade in _Grades)
                    sum += grade;

                return sum;
            }
            set
            {
                int count = _Grades.Count();

                if (count == 0)
                {
                    count = 1;
                    _Grades.Add(value);
                }

                float grade = value / count;

                for (int index = 0; index < count; index++)
                    _Grades[index] = grade;

                ModifiedFlag = true;
                Touch();
            }
        }

        public void ApplyGrade(
            float grade,
            int sampleSize)
        {
            if (sampleSize <= 0)
                sampleSize = 1;

            if (_Grades.Count() >= sampleSize)
                GradeCount = sampleSize - 1;

            _Grades.Insert(0, grade);
        }

        public int TouchCount                           // How many times touched.
        {
            get
            {
                return _TouchCount;
            }
            set
            {
                if (value != _TouchCount)
                {
                    _TouchCount = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public void ResetTouchCount()
        {
            _TouchCount = 0;
            ModifiedFlag = true;
            Touch();
        }

        public DateTime FirstTouchTime                  // First time touched.
        {
            get
            {
                return _FirstTouchTime;
            }
            set
            {
                if (value != _FirstTouchTime)
                {
                    _FirstTouchTime = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public DateTime LastTouchTime                   // Last time touched.
        {
            get
            {
                return _LastTouchTime;
            }
            set
            {
                if (value != _LastTouchTime)
                {
                    _LastTouchTime = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public DateTime NextTouchTime                   // Scheduled time for review.
        {
            get
            {
                return _NextTouchTime;
            }
            set
            {
                if (value != _NextTouchTime)
                {
                    _NextTouchTime = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public MultiLanguageString LastTextInput
        {
            get
            {
                return _LastTextInput;
            }
            set
            {
                if (value != _LastTextInput)
                {
                    _LastTextInput = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public int Stage
        {
            get
            {
                return _Stage;
            }
            set
            {
                if (value != _Stage)
                {
                    _Stage = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public List<int> ChoiceIndices
        {
            get
            {
                return _ChoiceIndices;
            }
            set
            {
                if (value != _ChoiceIndices)
                {
                    _ChoiceIndices = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public int CorrectChoiceIndex
        {
            get
            {
                return _CorrectChoiceIndex;
            }
            set
            {
                if (value != _CorrectChoiceIndex)
                {
                    _CorrectChoiceIndex = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public int ChosenChoiceIndex
        {
            get
            {
                return _ChosenChoiceIndex;
            }
            set
            {
                if (value != _ChosenChoiceIndex)
                {
                    _ChosenChoiceIndex = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public List<int> WordBlanksIndices
        {
            get
            {
                return _WordBlanksIndices;
            }
            set
            {
                if (value != _WordBlanksIndices)
                {
                    _WordBlanksIndices = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public string SubConfigurationKey
        {
            get
            {
                return _SubConfigurationKey;
            }
            set
            {
                if (value != _SubConfigurationKey)
                {
                    _SubConfigurationKey = value;
                    ModifiedFlag = true;
                    Touch();
                }
            }
        }

        public void Touch(
            ToolItemStatusCode statusCode,
            DateTime nowTime,
            DateTime nextTime,
            int stage)
        {
            _TouchCount++;

            if (_TouchCount == 1)
                _FirstTouchTime = nowTime;

            _LastTouchTime = nowTime;
            _NextTouchTime = nextTime;
            _StatusCode = statusCode;
            _Stage = stage;
            ModifiedFlag = true;
            Touch();
        }

        public void PrepareForRetouch(int stageDelta)
        {
            if (_TouchCount > 0)
                _TouchCount--;

            if (_Grades.Count() != 0)
                _Grades.RemoveAt(0);

            _Stage += stageDelta;
        }

        public void ClearGrade()
        {
            StatusCode = ToolItemStatusCode.Future;
            _Grades.Clear();
            _TouchCount = 0;
            _FirstTouchTime = DateTime.MinValue;
            _LastTouchTime = DateTime.MinValue;
            _NextTouchTime = DateTime.MinValue;
            _LastTextInput = null;
            _Stage = 0;
            ModifiedFlag = true;
            Touch();
        }

        public void Forget()
        {
            StatusCode = ToolItemStatusCode.Future;
            _Grades.Clear();
            _TouchCount = 0;
            _FirstTouchTime = DateTime.MinValue;
            _LastTouchTime = DateTime.MinValue;
            _NextTouchTime = DateTime.MinValue;
            _LastTextInput = null;
            _Stage = 0;
            _ChoiceIndices = null;
            _CorrectChoiceIndex = -1;
            _ChosenChoiceIndex = -1;
            _WordBlanksIndices = null;
            _SubConfigurationKey = null;
            ModifiedFlag = true;
            Touch();
        }

        public void ForgetLearned()
        {
            if (StatusCode != ToolItemStatusCode.Learned)
                return;
            StatusCode = ToolItemStatusCode.Future;
            _Grades.Clear();
            _TouchCount = 0;
            _FirstTouchTime = DateTime.MinValue;
            _LastTouchTime = DateTime.MinValue;
            _NextTouchTime = DateTime.MinValue;
            _LastTextInput = null;
            _Stage = 0;
            _ChoiceIndices = null;
            _CorrectChoiceIndex = -1;
            _ChosenChoiceIndex = -1;
            _WordBlanksIndices = null;
            _SubConfigurationKey = null;
            ModifiedFlag = true;
            Touch();
        }

        public void Learned()
        {
            StatusCode = ToolItemStatusCode.Learned;
            ModifiedFlag = true;
            Touch();
        }

        public void Restore()
        {
            StatusCode = ToolItemStatusCode.Active;
            ModifiedFlag = true;
            Touch();
        }

        // Returns true if the other item was changed more recently.
        public bool MergeStatus(ToolItemStatus other)
        {
            if (other.LastTouchTime > _LastTouchTime)
            {
                Copy(other);
                return true;
            }
            else
                other.Copy(this);
            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("StatusCode", _StatusCode.ToString()));
            element.Add(new XElement("Grades", TextUtilities.GetStringFromFloatList(_Grades)));
            element.Add(new XAttribute("TouchCount", _TouchCount.ToString()));
            element.Add(new XAttribute("FirstTouchTime", _FirstTouchTime.ToString()));
            element.Add(new XAttribute("LastTouchTime", _LastTouchTime.ToString()));
            element.Add(new XAttribute("NextTouchTime", _NextTouchTime.ToString()));
            element.Add(new XAttribute("Stage", _Stage.ToString()));
            element.Add(new XAttribute("Index", _Index.ToString()));
            if (_LastTextInput != null)
                element.Add(_LastTextInput.GetElement("LastTextInput"));
            if ((_ChoiceIndices != null) && (_ChoiceIndices.Count != 0))
                element.Add(new XElement("ChoiceIndices", ObjectUtilities.GetStringFromIntList(_ChoiceIndices)));
            if (_CorrectChoiceIndex != -1)
                element.Add(new XElement("CorrectChoiceIndex", _CorrectChoiceIndex));
            if (_ChosenChoiceIndex != -1)
                element.Add(new XElement("ChosenChoiceIndex", _ChosenChoiceIndex));
            if ((_WordBlanksIndices != null) && (_WordBlanksIndices.Count != 0))
                element.Add(new XElement("WordBlanksIndices", ObjectUtilities.GetStringFromIntList(_WordBlanksIndices)));
            if (!String.IsNullOrEmpty(_SubConfigurationKey))
                element.Add(new XElement("SubConfigurationKey", _SubConfigurationKey));

            return element;
        }

        public static ToolItemStatusCode GetToolItemStatusCodeFromString(string str)
        {
            ToolItemStatusCode code;

            switch (str)
            {
                case "Future":
                    code = ToolItemStatusCode.Future;
                    break;
                case "Active":
                    code = ToolItemStatusCode.Active;
                    break;
                case "Learned":
                    code = ToolItemStatusCode.Learned;
                    break;
                default:
                    throw new ObjectException("ToolItemStatus.GetToolItemStatusCodeFromString:  Unknown code:  " + str);
            }

            return code;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();
            bool returnValue = true;

            switch (attribute.Name.LocalName)
            {
                case "StatusCode":
                    _StatusCode = GetToolItemStatusCodeFromString(attributeValue);
                    break;
                case "TouchCount":
                    _TouchCount = Convert.ToInt32(attributeValue);
                    break;
                case "FirstTouchTime":
                    _FirstTouchTime = Convert.ToDateTime(attributeValue);
                    break;
                case "LastTouchTime":
                    _LastTouchTime = Convert.ToDateTime(attributeValue);
                    break;
                case "NextTouchTime":
                    _NextTouchTime = Convert.ToDateTime(attributeValue);
                    break;
                case "Stage":
                    _Stage = Convert.ToInt32(attributeValue);
                    break;
                case "Index":
                    _Index = Convert.ToInt32(attributeValue);
                    break;
                default:
                    returnValue = base.OnAttribute(attribute);
                    break;
            }

            return returnValue;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Grades":
                    _Grades = TextUtilities.GetFloatListFromString(childElement.Value);
                    break;
                case "LastTextInput":
                    _LastTextInput = new MultiLanguageString(childElement);
                    break;
                case "ChoiceIndices":
                    _ChoiceIndices = TextUtilities.GetIntListFromString(childElement.Value);
                    break;
                case "CorrectChoiceIndex":
                    _CorrectChoiceIndex = ObjectUtilities.GetIntegerFromString(childElement.Value, 0);
                    break;
                case "ChosenChoiceIndex":
                    _ChosenChoiceIndex = ObjectUtilities.GetIntegerFromString(childElement.Value, 0);
                    break;
                case "WordBlanksIndices":
                    _WordBlanksIndices = TextUtilities.GetIntListFromString(childElement.Value);
                    break;
                case "SubConfigurationKey":
                    _SubConfigurationKey = childElement.Value;
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int CompareToolItemStatusCodes(ToolItemStatusCode item1, ToolItemStatusCode item2)
        {
            if (item1 == item2)
                return 0;
            else if (item1 > item2)
                return 1;
            return -1;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ToolItemStatus otherObject = other as ToolItemStatus;
            int diff;

            if (otherObject == null)
                return base.Compare(other);

            diff = ObjectUtilities.CompareKeys(this, other);
            if (diff != 0)
                return diff;
            diff = CompareToolItemStatusCodes(_StatusCode, otherObject.StatusCode);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareFloatLists(_Grades, otherObject.Grades);
            if (diff != 0)
                return diff;
            diff = _TouchCount - otherObject.TouchCount;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_FirstTouchTime, otherObject.FirstTouchTime);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_LastTouchTime, otherObject.LastTouchTime);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_NextTouchTime, otherObject.NextTouchTime);
            if (diff != 0)
                return diff;
            diff = MultiLanguageString.Compare(_LastTextInput, otherObject.LastTextInput);
            if (diff != 0)
                return diff;
            diff = _Stage - otherObject.Stage;
            if (diff != 0)
                return diff;
            diff = _Index - otherObject.Index;
            return diff;
        }

        public static int Compare(ToolItemStatus other1, ToolItemStatus other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return 0;
            if ((object)other1 == null)
                return -1;
            if ((object)other2 == null)
                return 1;
            return other1.Compare(other2);
        }
    }
}

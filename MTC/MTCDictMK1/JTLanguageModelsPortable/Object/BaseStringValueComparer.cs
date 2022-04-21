using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Object
{
    public class BaseStringValueComparer : IComparer<BaseString>
    {
        public BaseStringValueComparer()
        {
        }

        // Compares length of nodes.  If x length is greater than y length returns 1.
        public int Compare(BaseString x, BaseString y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            string textX = x.Text;
            string textY = y.Text;

            if ((textX == null) || (textY == null))
            {
                if (textX == textY)
                    return 0;
                else if (textX == null)
                    return -1;
                else
                    return 1;
            }

            int returnValue = textX.CompareTo(textY);

            return returnValue;
        }
    }
}

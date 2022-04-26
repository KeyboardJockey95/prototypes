using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Dictionary
{
    public class ProbablePairingComparer : IComparer<ProbablePairing>
    {
        public ProbablePairingComparer()
        {
        }

        public int Compare(ProbablePairing x, ProbablePairing y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.Frequency == y.Frequency)
            {
                if (x.Probability == y.Probability)
                {
                    int diff = String.Compare(x.TargetMeaning, y.TargetMeaning);
                    if (diff != 0)
                        return diff;
                    diff = String.Compare(x.HostMeaning, y.HostMeaning);
                    return diff;
                }
                else if (float.IsNaN(x.Probability))
                    return 1;
                else if (float.IsNaN(y.Probability))
                    return -1;
                else if (x.Probability < y.Probability)
                    return 1;
                else
                    return -1;
            }
            else if (x.Frequency < y.Frequency)
                return 1;
            else
                return -1;
        }
    }
}

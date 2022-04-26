using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class CompoundMatcher : Matcher
    {
        public CompoundMatcher(Matcher matcher, string memberName, MatchCode logic, int page, int pageSize)
            : base(matcher, memberName, logic, page, pageSize)
        {
        }

        public CompoundMatcher(List<Matcher> matchers, string memberName, MatchCode logic, int page, int pageSize)
            : base(matchers, memberName, logic, page, pageSize)
        {
        }

        public CompoundMatcher(CompoundMatcher other)
            : base(other)
        {
        }

        public CompoundMatcher(XElement element)
        {
            OnElement(element);
        }

        public CompoundMatcher()
        {
        }

        public override bool Match(object obj)
        {
            if (Matchers == null)
                return true;

            switch (MatchType)
            {
                case MatchCode.Or:
                    {
                        foreach (Matcher matcher in Matchers)
                        {
                            if (matcher == null)
                                return true;
                            if (matcher.Match(obj))
                                return true;
                        }
                    }
                    break;
                case MatchCode.And:
                    {
                        foreach (Matcher matcher in Matchers)
                        {
                            if (matcher == null)
                                continue;
                            if (!matcher.Match(obj))
                                return false;
                        }

                        return true;
                    }
                case MatchCode.Xor:
                    {
                        int count = 0;

                        foreach (Matcher matcher in Matchers)
                        {
                            if (matcher == null)
                                count++;
                            if (matcher.Match(obj))
                                count++;
                        }

                        if (count == 1)
                            return true;
                    }
                    break;
                case MatchCode.Not:
                    {
                        foreach (Matcher matcher in Matchers)
                        {
                            if (matcher == null)
                                return false;
                            if (matcher.Match(obj))
                                return false;
                        }

                        return true;
                    }
                default:
                    throw new ObjectException("CompoundMatcher.Match:  MatchCode " + MatchType.ToString() + " not supported here.");
            }

            return false;
        }
    }
}

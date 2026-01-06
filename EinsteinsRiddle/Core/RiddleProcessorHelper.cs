using System.Collections.Generic;
using System.Linq;

namespace EinsteinsRiddle.Core
{
    public class RiddleProcessorHelper
    {
        public static Dictionary<string, List<string>> GetAttributeValuesDictionary(RiddleObjectsSet set, List<RiddleRule> rules = null, RiddleObjectAttribute toFit = null)
        {
            var retval = new Dictionary<string, List<string>>();

            if (set != null)
            {
                foreach (var setObject in set.Objects)
                {
                    foreach (var setObjectAttribute in setObject.Attributes.Where(x => x.IsSet))
                    {
                        if (!retval.TryAdd(setObjectAttribute.Key, new List<string> { setObjectAttribute.Value })
                            && !retval[setObjectAttribute.Key].Contains(setObjectAttribute.Value))
                        {
                            retval[setObjectAttribute.Key].Add(setObjectAttribute.Value);
                        }
                    }
                }
            }

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (!retval.TryAdd(rule.Attribute.Key, new List<string> { rule.Attribute.Value })
                        && !retval[rule.Attribute.Key].Contains(rule.Attribute.Value))
                    {
                        retval[rule.Attribute.Key].Add(rule.Attribute.Value);
                    }

                    if (!retval.TryAdd(rule.AttributeOfTarget.Key, new List<string> { rule.AttributeOfTarget.Value })
                        && !retval[rule.AttributeOfTarget.Key].Contains(rule.AttributeOfTarget.Value))
                    {
                        retval[rule.AttributeOfTarget.Key].Add(rule.AttributeOfTarget.Value);
                    }
                }
            }

            if (toFit != null)
            {
                if (!retval.TryAdd(toFit.Key, new List<string> { toFit.Value })
                    && !retval[toFit.Key].Contains(toFit.Value))
                {
                    retval[toFit.Key].Add(toFit.Value);
                }
            }

            return retval;
        }
    }
}

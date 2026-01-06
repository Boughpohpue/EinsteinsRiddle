using System;
using System.Collections.Generic;
using System.Linq;

namespace EinsteinsRiddle.Core
{
    public class RiddleObject
    {
        public List<RiddleObjectAttribute> Attributes { get; set; }
        public RiddleObject PrevNeighbor { get; set; }
        public RiddleObject NextNeighbor { get; set; }

        public bool HasPrevNeighbor => PrevNeighbor != null;
        public bool HasNextNeighbor => NextNeighbor != null;

        public bool IsComplete => !Attributes.Any(x => string.IsNullOrEmpty(x.Value));
        public int CompleteValuesCount => Attributes.Where(x => x.IsSet).Count();

        public RiddleObject()
        {
            Attributes = new List<RiddleObjectAttribute>();
        }

        public void AddAttribute(RiddleObjectAttribute a)
        {
            Attributes.Add(a);
        }

        public void SetAttribute(RiddleObjectAttribute a)
        {
            var toUpdate = Attributes.FirstOrDefault(x => x.Key == a.Key);

            if (toUpdate != null && !HasAttributeSet(a))
            {
                toUpdate.Value = a.Value;
            }
        }

        public bool ContainAttribute(RiddleObjectAttribute a)
        {
            return Attributes.Any(x => x.Key == a.Key && x.Value == a.Value);
        }

        public bool MeetsTheRule(RiddleRule rule)
        {
            switch (rule.Target)
            {
                case RiddleRuleTargets.Self:
                    return 
                        ContainAttribute(rule.Attribute) 
                        && ContainAttribute(rule.AttributeOfTarget);

                case RiddleRuleTargets.PrevNeighbor:
                    return
                        ContainAttribute(rule.Attribute)
                        && HasPrevNeighbor
                        && PrevNeighbor.ContainAttribute(rule.AttributeOfTarget);

                case RiddleRuleTargets.NextNeighbor:
                    return
                        ContainAttribute(rule.Attribute)
                        && HasNextNeighbor
                        && NextNeighbor.ContainAttribute(rule.AttributeOfTarget);

                case RiddleRuleTargets.SomeNeighbor:
                    return
                        ContainAttribute(rule.Attribute)
                        && 
                            ((HasNextNeighbor
                                && NextNeighbor.ContainAttribute(rule.AttributeOfTarget))
                            || (HasPrevNeighbor
                                && PrevNeighbor.ContainAttribute(rule.AttributeOfTarget)));

                default:
                    return false;
            }
        }

        public bool HasAttributeSet(RiddleObjectAttribute a)
        {
            return HasAttributeSet(a.Key);
        }

        public bool HasAttributeSet(string attributeName)
        {
            return Attributes.Any(x => x.Key == attributeName && x.IsSet);
        }        

        public string GetAttributeValue(RiddleObjectAttribute a)
        {
            return GetAttributeValue(a.Key);
        }

        public string GetAttributeValue(string attributeName)
        {
            var a = Attributes.FirstOrDefault(x => x.Key == attributeName);

            if (a == null)
            {
                throw new KeyNotFoundException($"Object doesn't contain an attribute named '{attributeName}'!");
            }

            return a.Value;
        }

        public override string ToString()
        {
            var str = "";
            foreach (var a in Attributes)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += " - ";
                }

                str += a.ToString();
            }

            return str;
        }

        public void PrintAndHighlightAttributeValue(string valueToHighlight)
        {
            var originalForeground = Console.ForegroundColor;

            foreach (var a in Attributes)
            {
                Console.ForegroundColor =
                    a.Value == valueToHighlight ?
                    ConsoleColor.DarkRed :
                    originalForeground;

                Console.Write(a);

                if (a != Attributes.Last())
                {
                    Console.Write(" - ");
                }
                else
                {
                    Console.WriteLine();
                }                
            }

            Console.ForegroundColor = originalForeground;
        }
    }
}

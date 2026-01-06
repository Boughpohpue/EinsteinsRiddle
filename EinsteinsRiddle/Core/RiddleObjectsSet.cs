using System;
using System.Collections.Generic;
using System.Linq;

namespace EinsteinsRiddle.Core
{
    public class RiddleObjectsSet
    {
        public List<RiddleObject> Objects { get; set; }

        public RiddleObjectsSet()
        {
            Objects = new List<RiddleObject>();
        }

        public int Count => Objects.Count;


        public override string ToString()
        {
            return
                string.Join(Environment.NewLine, Objects);
        }

        public void PrintSetAndHighlightAttributeValue(string valueToHighlight)
        {
            var originalForeground = Console.ForegroundColor;

            foreach (var obj in Objects)
            {
                if (obj.Attributes.Any(x => x.Value.Equals(valueToHighlight)))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    obj.PrintAndHighlightAttributeValue(valueToHighlight);
                }
                else
                {
                    Console.ForegroundColor = originalForeground;
                    Console.WriteLine(obj);
                }
            }

            Console.ForegroundColor = originalForeground;
        }

        public RiddleObject GetObjectByAttribute(RiddleObjectAttribute a)
        {
            return Objects.FirstOrDefault(x => x.ContainAttribute(a));
        }

        public RiddleObject GetObjectByRule(RiddleRule rule, bool completeMatch)
        {
            switch (rule.Target)
            {
                case RiddleRuleTargets.Self:
                    return
                        completeMatch ?
                        Objects.FirstOrDefault(x => x.ContainAttribute(rule.Attribute) && x.ContainAttribute(rule.AttributeOfTarget)) :
                        Objects.FirstOrDefault(x => x.ContainAttribute(rule.Attribute) || x.ContainAttribute(rule.AttributeOfTarget));

                case RiddleRuleTargets.NextNeighbor:
                    return
                        completeMatch ?
                        Objects.FirstOrDefault(x => 
                            x.ContainAttribute(rule.Attribute) && x.HasNextNeighbor && x.NextNeighbor.ContainAttribute(rule.AttributeOfTarget)) :
                        Objects.FirstOrDefault(x =>
                            x.ContainAttribute(rule.Attribute) || (x.HasNextNeighbor && x.NextNeighbor.ContainAttribute(rule.AttributeOfTarget)));

                case RiddleRuleTargets.PrevNeighbor:
                    return
                        completeMatch ?
                        Objects.FirstOrDefault(x =>
                            x.ContainAttribute(rule.Attribute) && x.HasPrevNeighbor && x.PrevNeighbor.ContainAttribute(rule.AttributeOfTarget)) :
                        Objects.FirstOrDefault(x =>
                            x.ContainAttribute(rule.Attribute) || (x.HasPrevNeighbor && x.PrevNeighbor.ContainAttribute(rule.AttributeOfTarget)));

                case RiddleRuleTargets.SomeNeighbor:
                    return
                        completeMatch ?
                        Objects.FirstOrDefault(x =>
                            x.ContainAttribute(rule.Attribute) 
                            && ((x.HasPrevNeighbor && x.PrevNeighbor.ContainAttribute(rule.AttributeOfTarget))
                                || (x.HasNextNeighbor && x.NextNeighbor.ContainAttribute(rule.AttributeOfTarget)))) :
                        Objects.FirstOrDefault(x =>
                            x.ContainAttribute(rule.Attribute)
                            || (x.HasPrevNeighbor && x.PrevNeighbor.ContainAttribute(rule.AttributeOfTarget)
                                || (x.HasNextNeighbor && x.NextNeighbor.ContainAttribute(rule.AttributeOfTarget))));

                default:
                    return null;
            }
        }

        public bool ContainsAttribute(RiddleObjectAttribute a)
        {
            return GetObjectByAttribute(a) != null;
        }

        public RiddleObjectsSet Clone()
        {
            var clone = new RiddleObjectsSet();

            for (var x = 0; x < Objects.Count; x++)
            {
                var ro = new RiddleObject();
                ro.Attributes = new List<RiddleObjectAttribute>();
                foreach (var an in Objects[x].Attributes)
                    ro.Attributes.Add(new RiddleObjectAttribute(an.Key, an.Value));

                if (x > 0)
                {
                    ro.PrevNeighbor = clone.Objects[x - 1];
                    clone.Objects[x - 1].NextNeighbor = ro;
                }

                clone.Objects.Add(ro);
            }

            return clone;
        }

        public RiddleObjectsSet Merge(RiddleObjectsSetCombo combo)
        {
            var retval = Clone();

            foreach (var comboSet in combo.Sets)
            {
                foreach (var comboItemObject in comboSet.Objects)
                {
                    var originalObject = Objects.First(x => x.ContainAttribute(comboItemObject.Attributes.First()));
                    var retvalObject = retval.Objects.First(x => x.ContainAttribute(comboItemObject.Attributes.First()));

                    foreach (var comboItemObjectAttribute in comboItemObject.Attributes.Where(x => !string.IsNullOrEmpty(x.Value)))
                    {
                        if (originalObject.ContainAttribute(comboItemObjectAttribute))
                        {
                            continue;
                        }
                        if (retvalObject.ContainAttribute(comboItemObjectAttribute))
                        {
                            continue;
                        }

                        if (retval.Objects.Any(x => x.ContainAttribute(comboItemObjectAttribute)))
                        {
                            throw new ArgumentException("Attribute already used!");
                        }

                        if (retvalObject.HasAttributeSet(comboItemObjectAttribute))
                        {
                            throw new ArgumentException("Attribute already set!");
                        }

                        retvalObject.SetAttribute(comboItemObjectAttribute);
                    }
                }
            }

            return retval;
        }

        public RiddleObjectsSet Merge(RiddleObjectsSet set)
        {
            var retval = Clone();

            foreach (var comboItemObject in set.Objects)
            {
                var originalObject = Objects.First(x => x.ContainAttribute(comboItemObject.Attributes.First()));
                var retvalObject = retval.Objects.First(x => x.ContainAttribute(comboItemObject.Attributes.First()));

                foreach (var comboItemObjectAttribute in comboItemObject.Attributes.Where(x => !string.IsNullOrEmpty(x.Value)))
                {
                    if (originalObject.ContainAttribute(comboItemObjectAttribute))
                    {
                        continue;
                    }
                    if (retvalObject.ContainAttribute(comboItemObjectAttribute))
                    {
                        continue;
                    }

                    if (retval.Objects.Any(x => x.ContainAttribute(comboItemObjectAttribute)))
                    {
                        throw new ArgumentException("Attribute already set for other object!");
                    }

                    if (retvalObject.HasAttributeSet(comboItemObjectAttribute))
                    {
                        throw new ArgumentException("Attribute already set!");
                    }

                    retvalObject.SetAttribute(comboItemObjectAttribute);
                }
            }

            return retval;
        }
    }
}

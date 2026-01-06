namespace EinsteinsRiddle.Core
{
    public class RiddleRule
    {
        public RiddleObjectAttribute Attribute { get; set; }
        public RiddleObjectAttribute AttributeOfTarget { get; set; }
        public RiddleRuleTargets Target { get; set; }
        public bool Applied { get; set; }

        public override string ToString()
        {
            var str = $"The one which {Attribute} ";

            switch (Target)
            {
                case RiddleRuleTargets.Self:
                    str += "also ";
                    break;

                case RiddleRuleTargets.SomeNeighbor:
                    str += "has some neighbor which ";
                    break;

                case RiddleRuleTargets.PrevNeighbor:
                    str += "has neighbor on the left which ";
                    break;

                case RiddleRuleTargets.NextNeighbor:
                    str += "has neighbor on the right which ";
                    break;
            }

            str += AttributeOfTarget;

            return str;
        }

        public override bool Equals(object obj)
        {
            if (obj is RiddleRule == false)
            {
                return false;
            }

            var castedObj = obj as RiddleRule;

            return this.Target == castedObj.Target 
                && this.Attribute == castedObj.Attribute 
                && this.AttributeOfTarget == castedObj.AttributeOfTarget;
        }

        public RiddleRule Revert()
        {
            return new RiddleRule
            {
                Attribute = AttributeOfTarget,
                AttributeOfTarget = Attribute,
                Target =
                    Target == RiddleRuleTargets.Self || 
                    Target == RiddleRuleTargets.SomeNeighbor ?
                        Target :
                        Target == RiddleRuleTargets.PrevNeighbor ?
                            RiddleRuleTargets.NextNeighbor :
                            RiddleRuleTargets.PrevNeighbor
            };
        }

        public static RiddleRule Revert(RiddleRule rule)
        {
            return new RiddleRule
            {
                Attribute = rule.AttributeOfTarget,
                AttributeOfTarget = rule.Attribute,
                Target =
                    rule.Target == RiddleRuleTargets.Self || rule.Target == RiddleRuleTargets.SomeNeighbor ?
                        rule.Target :
                        rule.Target == RiddleRuleTargets.PrevNeighbor ?
                            RiddleRuleTargets.NextNeighbor :
                            RiddleRuleTargets.PrevNeighbor
            };
        }
    }



    public enum RiddleRuleTargets
    {
        Self,
        SomeNeighbor,
        PrevNeighbor,
        NextNeighbor,
    }
}

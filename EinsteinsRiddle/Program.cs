using EinsteinsRiddle.Core;
//using System;
using System.Collections.Generic;
//using System.Text;
//using static System.Net.Mime.MediaTypeNames;

namespace EinsteinsRiddle
{
    /// <summary>
    /// Algorithm for solving logic riddles/puzzles similar to the Einsteins "Zebra Puzzle".
    /// https://en.wikipedia.org/wiki/Zebra_Puzzle
    /// </summary>
    class Program
    {
        const string Number = "NUMBER";     // 1, 2, 3, 4, 5
        const string Color = "COLOR";       // YEL(LOW), BLU(E), GR(EE)N, RED, WHI(TE)
        const string Nation = "NATION";     // NOR(WEGIAN), ENG(LISHMAN), GER(MAN), DAN(E), SWE(DE)
        const string Drinks = "DRINKS";     // COF(FEE), TEA, W(A)T(E)R, M(I)LK, BE(E)R
        const string Smokes = "SMOKES";     // CIG(AR), PIP(E), NOF(ILTER), L(I)G(H)T, M(E)NT(HOL)
        const string Breeds = "BREEDS";     // DOG, CAT, B(I)RD, F(I)SH, H(O)RS(E)

        static void Main(string[] args)
        {
            var attributeNames = new List<string>
            {
                Number,
                Color,
                Nation,
                Drinks,
                Smokes,
                Breeds
            };

            var attributeToFit = new RiddleObjectAttribute(Breeds, "FSH");

            var set = RiddleObjectsSetFactory.Create(5, attributeNames, Number);
            var rules = GetRules();

            var processor = new RiddleProcessor(set, rules, attributeToFit);
            processor.ProcessTheRiddle();
        }

        public static List<RiddleRule> GetRules()
        {
            var retval = new List<RiddleRule> {
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "NOR"),
                    AttributeOfTarget = new RiddleObjectAttribute(Number, "1" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "ENG" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Color, "RED" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "DAN" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Drinks, "TEA" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Color, "YEL" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Smokes, "CIG" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "GER" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Smokes, "PIP" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Number, "3" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Drinks, "MLK" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Smokes, "NOF" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Breeds, "BRD" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "SWE" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Breeds, "DOG" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Smokes, "MNT" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Drinks, "BER" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Color, "GRN" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Drinks, "COF" ),
                    Target = RiddleRuleTargets.Self
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Color, "GRN" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Color, "WHI" ),
                    Target = RiddleRuleTargets.NextNeighbor
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Smokes, "LGT" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Drinks, "WTR" ),
                    Target = RiddleRuleTargets.SomeNeighbor
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Smokes, "LGT" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Breeds, "CAT" ),
                    Target = RiddleRuleTargets.SomeNeighbor
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Nation, "NOR" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Color, "BLU" ),
                    Target = RiddleRuleTargets.SomeNeighbor
                },
                new RiddleRule
                {
                    Attribute = new RiddleObjectAttribute(Breeds, "HRS" ),
                    AttributeOfTarget = new RiddleObjectAttribute(Color, "YEL" ),
                    Target = RiddleRuleTargets.SomeNeighbor
                }
            };

            return retval;
        }
    }
}

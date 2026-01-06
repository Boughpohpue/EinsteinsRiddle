using System;
using System.Collections.Generic;
using System.Linq;

namespace EinsteinsRiddle.Core
{
    public class RiddleProcessor
    {
        private RiddleObjectsSet _set;
        private RiddleObjectsSet _originalSet;
        private List<RiddleRule> _rules;
        private List<RiddleRule> _selfRules;
        private List<RiddleRule> _specifiedNeighborRules;
        private List<RiddleRule> _unspecifiedNeighborRules;

        private RiddleObjectAttribute _attributeToFit;
        private Dictionary<string, List<string>> _attributeValuesDictionary;

        public RiddleProcessor(RiddleObjectsSet set, List<RiddleRule> rules, RiddleObjectAttribute attributeToFit)
        {
            _set = set;
            _originalSet = _set.Clone();
            _attributeToFit = attributeToFit;

            _rules = rules;
            _selfRules = _rules.Where(x => x.Target == RiddleRuleTargets.Self).ToList();
            _specifiedNeighborRules = _rules.Where(x => x.Target == RiddleRuleTargets.PrevNeighbor || x.Target == RiddleRuleTargets.NextNeighbor).ToList();
            _unspecifiedNeighborRules = _rules.Where(x => x.Target == RiddleRuleTargets.SomeNeighbor).ToList();

            _attributeValuesDictionary = RiddleProcessorHelper.GetAttributeValuesDictionary(_set, _rules, _attributeToFit);

            PrintRiddleInfo();
        }

        public void PrintRiddleInfo()
        {
            Console.WriteLine("Initial object set:");
            Console.WriteLine(_set);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Attribute to fit in order to solve the riddle:");
            Console.WriteLine(_attributeToFit);
            Console.WriteLine();
            Console.WriteLine();


            Console.WriteLine($"Found {_selfRules.Count} self rules:");
            foreach (var rule in _selfRules)
            {
                Console.WriteLine(rule);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"Found {_specifiedNeighborRules.Count} rules for specified neighbor");
            foreach (var rule in _specifiedNeighborRules)
            {
                Console.WriteLine(rule);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"Found {_unspecifiedNeighborRules.Count} rules for unspecified neighbor");
            foreach (var rule in _unspecifiedNeighborRules)
            {
                Console.WriteLine(rule);
            }
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
        }

        public void ProcessTheRiddle()
        {
            Console.WriteLine("PROCESSING THE RIDDLE...");

            // apply self rules
            Console.WriteLine();
            Console.WriteLine("Step 1. - Applying self rules...");
            var selfRulesCombosDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
            ApplySelfRules(_selfRules, out selfRulesCombosDict);
            Console.WriteLine();
            Console.WriteLine("Current set:");
            Console.WriteLine(_set);
            Console.WriteLine();
            

            // apply neighbor rules
            Console.WriteLine();
            Console.WriteLine("Step 2. - Applying neighbor rules...");
            var neighborRulesCombosDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
            ApplyNeighborRules(_specifiedNeighborRules, _unspecifiedNeighborRules, out neighborRulesCombosDict);
            Console.WriteLine();
            Console.WriteLine("Current set:");
            Console.WriteLine(_set);
            Console.WriteLine();


            // process combinations
            Console.WriteLine();
            Console.WriteLine("Step 3. - Processing combinations for unassigned rules...");
            Console.WriteLine();
            var combinations = ProcessCombinations(selfRulesCombosDict, neighborRulesCombosDict);


            // print conclusion
            Conclude(combinations);
        }

        private void ApplySelfRules(List<RiddleRule> rules, 
            out Dictionary<RiddleRule, RiddleObjectsSetCombo> unassignedRulesComboDict)
        {
            unassignedRulesComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();

            var prev = 0;
            // apply in loop as last rule may be a solution for the first one
            while (prev < _set.Objects.Sum(x => x.CompleteValuesCount))
            {
                prev = _set.Objects.Sum(x => x.CompleteValuesCount);

                if (unassignedRulesComboDict.Count > 0)
                {
                    unassignedRulesComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
                }

                foreach (var rule in rules)
                {
                    var current = _set.GetObjectByRule(rule, false);

                    if (current != null && current.MeetsTheRule(rule))
                    {
                        continue;
                    }

                    if (current != null)
                    {
                        var attributeToUpdate =
                            current.ContainAttribute(rule.Attribute) ?
                            rule.AttributeOfTarget :
                            rule.Attribute;

                        current.SetAttribute(attributeToUpdate);
                        rule.Applied = true;
                        continue;
                    }

                    var combinations = new RiddleObjectsSetCombo();
                    foreach (var setObject in _set.Objects)
                    {
                        if (setObject.HasAttributeSet(rule.Attribute)
                            || setObject.HasAttributeSet(rule.AttributeOfTarget))
                        {
                            continue;
                        }

                        var tempSet = _originalSet.Clone();
                        var tempObject = tempSet.GetObjectByAttribute(setObject.Attributes.First());
                        tempObject.SetAttribute(rule.Attribute);
                        tempObject.SetAttribute(rule.AttributeOfTarget);

                        combinations.AddSet(tempSet);
                    }

                    unassignedRulesComboDict.Add(rule, combinations);
                }
            }
        }

        private void ApplyNeighborRules(List<RiddleRule> specifiedNeighborRules, 
            List<RiddleRule> unspecifiedNeighborRules, 
            out Dictionary<RiddleRule, RiddleObjectsSetCombo> unassignedRulesComboDict)
        {
            var unassignedSpecifiedNeighborRuleComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
            var unassignedUnspecifiedNeighborRuleComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();

            var prev = 0;
            // loop as last rule may be a solution for the first one
            while (prev < _set.Objects.Sum(x => x.CompleteValuesCount))
            {
                prev = _set.Objects.Sum(x => x.CompleteValuesCount);

                unassignedSpecifiedNeighborRuleComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
                ApplySpecifiedNeighborRules(specifiedNeighborRules, out unassignedSpecifiedNeighborRuleComboDict);

                unassignedUnspecifiedNeighborRuleComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();
                ApplyUnspecifiedNeighborRules(unspecifiedNeighborRules, out unassignedUnspecifiedNeighborRuleComboDict);
            }

            unassignedRulesComboDict = 
                (new[]
                {
                    unassignedSpecifiedNeighborRuleComboDict,
                    unassignedUnspecifiedNeighborRuleComboDict
                }).
                SelectMany(dict => dict).
                ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private void ApplySpecifiedNeighborRules(List<RiddleRule> rules, 
            out Dictionary<RiddleRule, RiddleObjectsSetCombo> unassignedRulesComboDict)
        {
            unassignedRulesComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();

            foreach (var rule in rules)
            {
                var attributeOwner = _set.GetObjectByAttribute(rule.Attribute);
                var targetAttributeOwner = _set.GetObjectByAttribute(rule.AttributeOfTarget);

                if (attributeOwner != null && targetAttributeOwner != null)
                {
                    continue;
                }

                if (attributeOwner != null)
                {
                    if (rule.Target == RiddleRuleTargets.PrevNeighbor)
                    {
                        attributeOwner.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                    }
                    else if (rule.Target == RiddleRuleTargets.NextNeighbor)
                    {
                        attributeOwner.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                    }
                    rule.Applied = true;
                }
                else if (targetAttributeOwner != null)
                {
                    if (rule.Target == RiddleRuleTargets.PrevNeighbor)
                    {
                        attributeOwner.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                    }
                    else if (rule.Target == RiddleRuleTargets.NextNeighbor)
                    {
                        attributeOwner.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                    }
                    rule.Applied = true;
                }
                else
                {
                    var combos = new RiddleObjectsSetCombo();

                    foreach (var o in _set.Objects)
                    {
                        if ((rule.Target == RiddleRuleTargets.PrevNeighbor && !o.HasPrevNeighbor)
                            || (rule.Target == RiddleRuleTargets.NextNeighbor && !o.HasNextNeighbor))
                        {
                            continue;
                        }

                        if (o.HasAttributeSet(rule.Attribute)
                            || (rule.Target == RiddleRuleTargets.PrevNeighbor && o.PrevNeighbor.HasAttributeSet(rule.AttributeOfTarget))
                            || (rule.Target == RiddleRuleTargets.NextNeighbor && o.NextNeighbor.HasAttributeSet(rule.AttributeOfTarget)))
                        {
                            continue;
                        }

                        var temp = _originalSet.Clone();
                        var tempObject = temp.GetObjectByAttribute(o.Attributes.First());
                        tempObject.SetAttribute(rule.Attribute);

                        if (rule.Target == RiddleRuleTargets.PrevNeighbor)
                        {
                            tempObject.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                        }
                        else if (rule.Target == RiddleRuleTargets.NextNeighbor)
                        {
                            tempObject.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                        }

                        combos.AddSet(temp);
                    }

                    if (combos.Sets.Any())
                    {
                        unassignedRulesComboDict.Add(rule, combos);
                    }
                }
            }
        }

        private void ApplyUnspecifiedNeighborRules(List<RiddleRule> rules, 
            out Dictionary<RiddleRule, RiddleObjectsSetCombo> unassignedRulesComboDict)
        {
            unassignedRulesComboDict = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();

            foreach (var rule in rules)
            {
                var attributeOwner = _set.Objects.FirstOrDefault(x => x.ContainAttribute(rule.Attribute));
                var targetAttributeOwner = _set.Objects.FirstOrDefault(x => x.ContainAttribute(rule.AttributeOfTarget));

                if (attributeOwner != null && targetAttributeOwner != null)
                {
                    continue;
                }

                if (attributeOwner != null)
                {
                    if (!attributeOwner.HasPrevNeighbor)
                    {
                        attributeOwner.SetAttribute(rule.Attribute);
                        attributeOwner.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                        rule.Applied = true;
                    }
                    else if (!attributeOwner.HasNextNeighbor)
                    {
                        attributeOwner.SetAttribute(rule.Attribute);
                        attributeOwner.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                        rule.Applied = true;
                    }
                    else if (attributeOwner.NextNeighbor.HasAttributeSet(rule.AttributeOfTarget))
                    {
                        attributeOwner.SetAttribute(rule.Attribute);
                        attributeOwner.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                        rule.Applied = true;
                    }
                    else if (attributeOwner.PrevNeighbor.HasAttributeSet(rule.AttributeOfTarget))
                    {
                        attributeOwner.SetAttribute(rule.Attribute);
                        attributeOwner.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                        rule.Applied = true;
                    }
                    else
                    {
                        var combos = new RiddleObjectsSetCombo();

                        var cloneForPrev = _originalSet.Clone();
                        var cloneForPrevAttributeOwner = cloneForPrev.GetObjectByAttribute(rule.Attribute);
                        cloneForPrevAttributeOwner.SetAttribute(rule.Attribute);
                        cloneForPrevAttributeOwner.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                        combos.Sets.Add(cloneForPrev);

                        var cloneForNext = _originalSet.Clone();
                        var cloneForNextAttributeOwner = cloneForNext.GetObjectByAttribute(rule.Attribute);
                        cloneForNextAttributeOwner.SetAttribute(rule.Attribute);
                        cloneForNextAttributeOwner.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                        combos.Sets.Add(cloneForNext);

                        unassignedRulesComboDict.Add(rule, combos);
                    }
                }
                else if (targetAttributeOwner != null)
                {
                    if (!targetAttributeOwner.HasPrevNeighbor)
                    {
                        targetAttributeOwner.SetAttribute(rule.AttributeOfTarget);
                        targetAttributeOwner.NextNeighbor.SetAttribute(rule.Attribute);
                        rule.Applied = true;
                    }
                    else if (!targetAttributeOwner.HasNextNeighbor)
                    {
                        targetAttributeOwner.SetAttribute(rule.AttributeOfTarget);
                        targetAttributeOwner.PrevNeighbor.SetAttribute(rule.Attribute);
                        rule.Applied = true;
                    }
                    else if (targetAttributeOwner.NextNeighbor.HasAttributeSet(rule.Attribute))
                    {
                        targetAttributeOwner.SetAttribute(rule.AttributeOfTarget);
                        targetAttributeOwner.PrevNeighbor.SetAttribute(rule.Attribute);
                        rule.Applied = true;
                    }
                    else if (targetAttributeOwner.PrevNeighbor.HasAttributeSet(rule.Attribute))
                    {
                        targetAttributeOwner.SetAttribute(rule.AttributeOfTarget);
                        targetAttributeOwner.NextNeighbor.SetAttribute(rule.Attribute);
                        rule.Applied = true;
                    }
                    else
                    {
                        var combos = new RiddleObjectsSetCombo();

                        var cloneForPrev = _originalSet.Clone();
                        var cloneForPrevAttributeOwner = cloneForPrev.GetObjectByAttribute(rule.AttributeOfTarget);
                        cloneForPrevAttributeOwner.SetAttribute(rule.AttributeOfTarget);
                        cloneForPrevAttributeOwner.PrevNeighbor.SetAttribute(rule.Attribute);
                        combos.AddSet(cloneForPrev);

                        var cloneForNext = _originalSet.Clone();
                        var cloneForNextAttributeOwner = cloneForNext.GetObjectByAttribute(rule.AttributeOfTarget);
                        cloneForNextAttributeOwner.SetAttribute(rule.Attribute);
                        cloneForNextAttributeOwner.PrevNeighbor.SetAttribute(rule.Attribute);
                        combos.AddSet(cloneForNext);

                        unassignedRulesComboDict.Add(rule, combos);
                    }
                }
                else
                {
                    var combos = new RiddleObjectsSetCombo();

                    foreach (var o in _set.Objects)
                    {
                        if (o.HasAttributeSet(rule.Attribute)
                            || (o.HasPrevNeighbor && o.PrevNeighbor.HasAttributeSet(rule.AttributeOfTarget)
                            && o.HasNextNeighbor && o.NextNeighbor.HasAttributeSet(rule.AttributeOfTarget)))
                        {
                            continue;
                        }

                        if (o.HasNextNeighbor)
                        {
                            var tempForNext = _originalSet.Clone();
                            var tempForNextObject = tempForNext.GetObjectByAttribute(o.Attributes.First());
                            tempForNextObject.SetAttribute(rule.Attribute);
                            tempForNextObject.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                            combos.AddSet(tempForNext);
                        }

                        if (o.HasPrevNeighbor)
                        {
                            var tempForPrev = _originalSet.Clone();
                            var tempForPrevObject = tempForPrev.GetObjectByAttribute(o.Attributes.First());
                            tempForPrevObject.SetAttribute(rule.Attribute);
                            tempForPrevObject.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                            combos.AddSet(tempForPrev);
                        }
                    }

                    if (combos.Sets.Any())
                    {
                        unassignedRulesComboDict.Add(rule, combos);
                    }
                }
            }
        }

        private List<RiddleObjectsSetCombo> ProcessCombinations(Dictionary<RiddleRule, RiddleObjectsSetCombo> selfRulesCombosDict, 
            Dictionary<RiddleRule, RiddleObjectsSetCombo> neighborRulesCombosDict)
        {
            selfRulesCombosDict = ValidateCombinations(selfRulesCombosDict);
            neighborRulesCombosDict = ValidateCombinations(neighborRulesCombosDict);

            var joinedDictionary = 
                (new [] 
                { 
                    selfRulesCombosDict, 
                    neighborRulesCombosDict 
                }).
                SelectMany(dict => dict).
                ToDictionary(pair => pair.Key, pair => pair.Value);

            return CreateCombos(joinedDictionary);
        }

        private Dictionary<RiddleRule, RiddleObjectsSetCombo> ValidateCombinations(Dictionary<RiddleRule, RiddleObjectsSetCombo> combosDict)
        {
            var validCombos = new Dictionary<RiddleRule, RiddleObjectsSetCombo>();

            foreach (var combo in combosDict)
            {
                var validCombo = new RiddleObjectsSetCombo();
                foreach (var comboSet in combo.Value.Sets)
                {
                    try
                    {
                        validCombo.AddSet(_set.Merge(comboSet));
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (validCombo.Sets.Count > 0)
                {
                    validCombos.Add(combo.Key, validCombo);
                }
            }

            return validCombos;
        }

        public List<RiddleObjectsSetCombo> CreateCombos(Dictionary<RiddleRule, RiddleObjectsSetCombo> rulesCombosDictionary)
        {
            var retval = new List<RiddleObjectsSetCombo>();

            var counter = 1;
            foreach (var rulesCombo in rulesCombosDictionary.OrderBy(x => x.Value.Sets.Count))
            {
                //skip if rule has been already processed by nearly finished attribute
                if (rulesCombo.Key.Applied 
                    || retval.Any(x => x.Sets.Any(y => 
                        y.ContainsAttribute(rulesCombo.Key.Attribute) 
                        && y.ContainsAttribute(rulesCombo.Key.AttributeOfTarget))))
                {
                    continue;
                }

                //Console.WriteLine();
                Console.WriteLine($"Step 3.{counter++}. - Processing '{rulesCombo.Key}' rule combinations...");
                Console.WriteLine();

                retval = CreateCombos(rulesCombo.Value, retval);

                rulesCombo.Key.Applied = true;

                var combinationToRemove = new List<RiddleObjectsSetCombo>();

                _lastNearlyCompleteAttributesAdded = new();
                _lastRulesAppliedByNearlyResolvedAttribute = new();

                foreach (var c in retval)
                {
                    var merged = _set.Merge(c);

                    if (ProcessNearlyCompleteAttributes(c, merged, rulesCombosDictionary))
                    {
                        try
                        {
                            var updatedMerged = _set.Merge(c);
                        }
                        catch (Exception ex)
                        {
                            combinationToRemove.Add(c);
                        }
                    }
                }

                foreach (var c2r in combinationToRemove)
                {
                    retval.Remove(c2r);
                }

                if (_lastRulesAppliedByNearlyResolvedAttribute.Any() || _lastNearlyCompleteAttributesAdded.Any())
                {                    
                    foreach (var ra in _lastNearlyCompleteAttributesAdded)
                    {
                        Console.WriteLine($"Resolved: {ra}!");
                    }
                    foreach (var ra in _lastRulesAppliedByNearlyResolvedAttribute)
                    {
                        Console.WriteLine($"Applied rule: {ra}!");
                    }

                    Console.WriteLine();
                }
                
                Console.WriteLine($"Step resulted in {retval.Count} valid combinations...");
                Console.WriteLine();

                var comboCntr = 1;
                foreach (var c in retval)
                {
                    var merged = _set.Merge(c);
                    Console.WriteLine($"{comboCntr++}.");
                    Console.WriteLine(merged);
                    Console.WriteLine();
                }
            }

            return retval;
        }

        public List<RiddleObjectsSetCombo> CreateCombos(RiddleObjectsSetCombo currentRulesCombo, List<RiddleObjectsSetCombo> existingCombos)
        {
            var retval = new List<RiddleObjectsSetCombo>();

            if (existingCombos.Count == 0)
            {
                foreach (var objectsSet in currentRulesCombo.Sets)
                {
                    var newCombo = new RiddleObjectsSetCombo
                    {
                        Sets = new List<RiddleObjectsSet> { objectsSet }
                    };

                    retval.Add(newCombo);
                }
            }
            else
            {
                foreach (var objectsSet in currentRulesCombo.Sets)
                {
                    foreach (var combo in existingCombos)
                    {
                        var newCombo = new RiddleObjectsSetCombo
                        {
                            Sets = new List<RiddleObjectsSet> { objectsSet }
                        };

                        newCombo.Sets.AddRange(combo.Sets);

                        try
                        {
                            var merged = _set.Merge(newCombo);
                            retval.Add(newCombo);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

            return retval;
        }

        private List<RiddleObjectAttribute> _lastNearlyCompleteAttributesAdded = new();
        public bool ProcessNearlyCompleteAttributes(RiddleObjectsSetCombo combo, RiddleObjectsSet merged, Dictionary<RiddleRule, RiddleObjectsSetCombo> rulesCombosDictionary)
        {
            var retval = false;
            foreach (var mad in RiddleProcessorHelper.GetAttributeValuesDictionary(merged).Where(x => x.Value.Count == _originalSet.Objects.Count - 1))
            {
                var newComboSet = _originalSet.Clone();
                var missingValue = _attributeValuesDictionary[mad.Key].FirstOrDefault(x => !mad.Value.Contains(x));

                for (var x = 0; x < merged.Objects.Count; x++)
                {
                    if (!merged.Objects[x].HasAttributeSet(mad.Key))
                    {                        
                        var newAttr = new RiddleObjectAttribute(mad.Key, missingValue);
                        newComboSet.Objects[x].SetAttribute(newAttr);

                        if (!_lastNearlyCompleteAttributesAdded.Contains(newAttr))
                            _lastNearlyCompleteAttributesAdded.Add(newAttr);

                        ApplyRulesForNearlyCompleteAttribute(rulesCombosDictionary, newComboSet.Objects[x], merged.Objects[x], merged, mad, missingValue);

                        combo.AddSet(newComboSet);

                        retval = true;
                    }
                }
            }

            return retval;
        }

        private List<RiddleRule> _lastRulesAppliedByNearlyResolvedAttribute = new();
        public void ApplyRulesForNearlyCompleteAttribute(Dictionary<RiddleRule, RiddleObjectsSetCombo> rulesCombosDictionary, 
            RiddleObject currentObject, RiddleObject currentMergedObject, RiddleObjectsSet currentMergedSet,
            KeyValuePair<string, List<string>> mad, string missingValue)
        {
            var rulesKeys = rulesCombosDictionary.Keys.Where(x => !x.Applied).ToList();
            var attrRules = rulesKeys.Where(x => x.Attribute.Key == mad.Key && x.Attribute.Value == missingValue).ToList();
            attrRules.AddRange(rulesKeys.Where(x => x.AttributeOfTarget.Key == mad.Key && x.AttributeOfTarget.Value == missingValue).Select(x => x.Revert()));
            attrRules = attrRules.Where(x => !currentMergedSet.ContainsAttribute(x.AttributeOfTarget)).ToList();

            if (!attrRules.Any())
            {
                return;
            }

            foreach (var rule in attrRules)
            {                
                switch (rule.Target)
                {
                    case RiddleRuleTargets.Self:
                        currentObject.SetAttribute(rule.AttributeOfTarget);
                        if (!_lastRulesAppliedByNearlyResolvedAttribute.Contains(rule))
                            _lastRulesAppliedByNearlyResolvedAttribute.Add(rule);
                        break;

                    case RiddleRuleTargets.PrevNeighbor:
                        currentObject.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                        if (!_lastRulesAppliedByNearlyResolvedAttribute.Contains(rule))
                            _lastRulesAppliedByNearlyResolvedAttribute.Add(rule);
                        break;

                    case RiddleRuleTargets.NextNeighbor:
                        currentObject.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                        if (!_lastRulesAppliedByNearlyResolvedAttribute.Contains(rule))
                            _lastRulesAppliedByNearlyResolvedAttribute.Add(rule);
                        break;

                    case RiddleRuleTargets.SomeNeighbor:
                        if (currentMergedObject.HasNextNeighbor
                            && (!currentMergedObject.HasPrevNeighbor || currentMergedObject.PrevNeighbor.HasAttributeSet(rule.AttributeOfTarget.Key)))
                        {
                            currentObject.NextNeighbor.SetAttribute(rule.AttributeOfTarget);
                            if (!_lastRulesAppliedByNearlyResolvedAttribute.Contains(rule))
                                _lastRulesAppliedByNearlyResolvedAttribute.Add(rule);
                        }

                        if (currentMergedObject.HasPrevNeighbor
                            && (!currentMergedObject.HasNextNeighbor || currentMergedObject.NextNeighbor.HasAttributeSet(rule.AttributeOfTarget.Key)))
                        {
                            currentObject.PrevNeighbor.SetAttribute(rule.AttributeOfTarget);
                            if (!_lastRulesAppliedByNearlyResolvedAttribute.Contains(rule))
                                _lastRulesAppliedByNearlyResolvedAttribute.Add(rule);
                        }
                        break;
                }
            }
        }

        private void Conclude(List<RiddleObjectsSetCombo> combinations)
        {
            if (combinations.Count == 1)
            {

                var merged = _set.Merge(combinations[0]);
                var incompleteSets = merged.Objects.Where(x => !x.HasAttributeSet(_attributeToFit));

                if (incompleteSets.Count() == 0)
                {
                    Console.WriteLine("THE RIDDLE HAS BEEN COMPLETELY SOLVED!!");
                    Console.WriteLine();

                    merged.PrintSetAndHighlightAttributeValue(_attributeToFit.Value);
                    return;
                }

                if (incompleteSets.Count() == 1)
                {
                    var incompleteSet = incompleteSets.First();
                    var incompleteAttributesCount = incompleteSet.Attributes.Count(x => !x.IsSet);

                    incompleteSet.SetAttribute(_attributeToFit);

                    if (incompleteAttributesCount == 1)
                    {
                        Console.WriteLine("THE RIDDLE HAS BEEN COMPLETELY SOLVED!!");
                        Console.WriteLine();

                        merged.PrintSetAndHighlightAttributeValue(_attributeToFit.Value);
                    }
                    else
                    {
                        Console.WriteLine("THE RIDDLE HAS BEEN PARTIALLY SOLVED, NOT ALL ATTRIBUTES HAS BEEN ASSIGNED!");
                        Console.WriteLine();
                        Console.WriteLine(merged);
                    }
                }
                else
                {
                    Console.WriteLine("THE RIDDLE HAS NOT BEEN SOLVED AS THERE IS MORE THAN ONE CANDIDATE FOR THE ATTRIBUTE TO FIT!");
                    Console.WriteLine();
                    Console.WriteLine(merged);
                }
            }
            else
            {
                Console.WriteLine("THE RIDDLE HAS NOT BEEN SOLVED!");
                Console.WriteLine();

                Console.WriteLine("Listing combinations:");
                Console.WriteLine();

                foreach (var c in combinations)
                {
                    var merged = _set.Merge(c);

                    Console.WriteLine(merged);
                    Console.WriteLine();
                }
            }
        }
    }
}

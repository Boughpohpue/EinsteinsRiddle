using System.Collections.Generic;
using System.Linq;

namespace EinsteinsRiddle.Core
{
    public class RiddleObjectsSetFactory
    {
        public static RiddleObjectsSet Create(int amount, List<string> attributeNames, string identifierKey)
        {
            var setObjects = new List<RiddleObject>();

            for (var x = 0; x < amount; x++)
            {                
                var ro = new RiddleObject
                {
                    Attributes = attributeNames.
                        Select(a => 
                            a == identifierKey ? 
                            new RiddleObjectAttribute(a, (x + 1).ToString()) : 
                            new RiddleObjectAttribute(a, string.Empty)).
                        ToList()
                };

                if (x > 0)
                {
                    ro.PrevNeighbor = setObjects[x - 1];
                    setObjects[x - 1].NextNeighbor = ro;
                }

                setObjects.Add(ro);
            }

            return new RiddleObjectsSet
            {
                Objects = setObjects
            };
        }
    }
}

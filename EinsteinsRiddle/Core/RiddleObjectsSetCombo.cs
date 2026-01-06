using System;
using System.Collections.Generic;

namespace EinsteinsRiddle.Core
{
    public class RiddleObjectsSetCombo
    {
        public List<RiddleObjectsSet> Sets { get; set; }

        public RiddleObjectsSetCombo()
        {
            Sets = new List<RiddleObjectsSet>();
        }

        public void AddSet(RiddleObjectsSet set)
        {
            Sets.Add(set);
        }

        public override string ToString()
        {
            var str = string.Empty;

            foreach (var s in Sets)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += Environment.NewLine;
                }

                str += s.ToString();
            }

            return str;
        }
    }
}

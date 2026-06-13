using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public static class Utils
    {
        public static string GetChildPath(Transform parent, Transform child)
        {
            var current = child;
            var path = new List<string>();
            while (current != parent)
            {
                path.Add(current.name);
                current = current.parent;
                if (!current)
                {
                    throw new Exception($"{parent.name} is not a parent of {child.name}");
                }
            }

            path.Reverse();
            return string.Join("/", path);
        }

        public static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }
    }
}

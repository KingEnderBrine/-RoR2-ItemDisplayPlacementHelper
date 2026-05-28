using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDisplayPlacementHelper
{
    public static class Utils
    {
        public static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDisplayPlacementHelper
{
    public static class StringExtensions
    {
        public static bool ContainsInSequence(this string target, string input, string delimiter = null)
        {
            var lowerCaseTarget = target.ToLowerInvariant();
            var lowerCaseInput = input.ToLowerInvariant();

            if (input.Length == 0 || target.Length == 0)
            {
                return true;
            }
            if (string.IsNullOrEmpty(delimiter))
            {
                return ProcessString(lowerCaseTarget, lowerCaseInput);
            }

            var lowerCaseDelimiter = delimiter.ToLowerInvariant();
            foreach (var targetPart in lowerCaseTarget.Split(new[] { lowerCaseDelimiter }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (ProcessString(targetPart, lowerCaseInput))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ProcessString(string target, string input)
        {
            var j = 0;
            for (var i = 0; i < target.Length; i++)
            {
                if (target[i] == input[j])
                {
                    j++;
                }
                if (j == input.Length)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

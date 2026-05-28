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

            if (input.Length == 0)
            {
                return true;
            }

            if (target.Length == 0)
            {
                return false;
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

        public static string FormatAsSplitWords(this string str)
        {
            if (str.Length == 0)
            {
                return str;
            }

            var builder = new StringBuilder(str.Length);
            builder.Append(char.ToUpper(str[0]));

            for (var i = 1; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsUpper(c))
                {
                    builder.Append(' ');
                    builder.Append(char.ToLower(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            if (builder[^1] == ' ')
            {
                builder.Remove(builder.Length - 1, 1);
            }

            return builder.ToString();
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

using System;
using System.Text;

namespace ItemDisplayPlacementHelper
{
    public static class StringExtensions
    {
        public static bool ContainsInSequence(this string target, string input, string delimiter = null)
        {
            if (input.Length == 0)
            {
                return true;
            }

            if (target.Length == 0)
            {
                return false;
            }

            var comparer = StringComparer.OrdinalIgnoreCase;
            var startIndex = 0;
            do
            {
                var delimiterIndex = string.IsNullOrEmpty(delimiter) ? -1 : target.IndexOf(delimiter, startIndex, StringComparison.OrdinalIgnoreCase);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = target.Length;
                }

                for (int i = startIndex, j = 0; i < delimiterIndex; i++)
                {
                    if (char.ToUpperInvariant(target[i]) == char.ToUpperInvariant(input[j]))
                    {
                        j++;
                    }

                    if (j == input.Length)
                    {
                        return true;
                    }
                }
                startIndex = delimiterIndex + (delimiter?.Length ?? 0);
            } while (startIndex < target.Length);

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
    }
}

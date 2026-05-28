using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDisplayPlacementHelper
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder StartBlock(this StringBuilder sb, ref int indent)
        {
            sb.L(indent).Append('{');
            indent++;
            return sb;
        }

        public static StringBuilder EndBlock(this StringBuilder sb, ref int indent)
        {
            indent--;
            sb.L(indent).Append('}');
            return sb;
        }

        public static StringBuilder L(this StringBuilder sb, int indent)
        {
            sb.AppendLine();

            if (indent == 0)
            {
                return sb;
            }

            sb.Append(' ', indent * 4);
            return sb;
        }

        public static StringBuilder A(this StringBuilder sb, string text)
        {
            return sb.Append(text);
        }
    }
}

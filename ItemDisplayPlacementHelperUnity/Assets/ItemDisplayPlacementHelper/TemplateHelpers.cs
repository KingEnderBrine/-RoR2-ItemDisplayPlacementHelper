using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public static class TemplateHelpers
    {
        public static string EnumFlagsValue<T>(T value, string modificator) where T : Enum
        {
            var intValue = Convert.ToInt32(value);
            switch (modificator)
            {
                case "r":
                    return $"{intValue}";
                default:
                {
                    var typeName = typeof(T).Name;
                    var names = Enum.GetNames(typeof(T));
                    var values = Enum.GetValues(typeof(T)) as IList;
                    var sb = new StringBuilder();
                    for (int i = 0; i < values.Count; i++)
                    {
                        var v = (int)values[i];
                        if (!Utils.IsPowerOfTwo(v))
                        {
                            continue;
                        }

                        if ((v & intValue) == v)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(" | ");
                            }
                            sb.Append(typeName).Append('.').Append(names[i]);
                        }
                    }
                    return sb.ToString();
                }
            }
        }

        public static string EnumValue<T>(T value, string modificator) where T : Enum
        {
            switch (modificator)
            {
                case "r":
                    return $"{Convert.ToInt32(value)}";
                default:
                    return $@"{typeof(T).Name}.{value}";
            }
        }

        public static string StringText(string str, string modificator)
        {
            switch (modificator)
            {
                case "r":
                    return str ?? "";
                default:
                    return $@"""{str}""";
            }
        }

        public static string Vector3Text(Vector3 vector, int precision, string modificator, string subField)
        {
            bool raw = modificator == "r";
            switch (subField)
            {
                case "x":
                    return FloatInvariant(vector.x, precision) + (raw ? "" : "F");
                case "y":
                    return FloatInvariant(vector.y, precision) + (raw ? "" : "F");
                case "z":
                    return FloatInvariant(vector.z, precision) + (raw ? "" : "F");
            }
            return $"new Vector3({FloatInvariant(vector.x, precision)}F, {FloatInvariant(vector.y, precision)}F, {FloatInvariant(vector.z, precision)}F)";
        }

        public static string FloatInvariant(float num, int precision)
        {
            return num.ToString($"0.{"".PadLeft(precision, '#')}", CultureInfo.InvariantCulture);
        }
    }
}

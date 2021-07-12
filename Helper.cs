using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkUtilities
{
    public static class Helper
    {
        public static string Clear(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return name.Replace("[", "").Replace("]", "");
        }

        public static string ToCamelCase(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = name.ToLower();

            string[] array = name.Split('_');
            for (int i = 0; i < array.Length; i++)
            {
                string s = array[i];
                string first = string.Empty;
                string rest = string.Empty;
                if (s.Length > 0)
                {
                    first = Char.ToUpperInvariant(s[0]).ToString();
                }
                if (s.Length > 1)
                {
                    rest = s.Substring(1).ToLowerInvariant();
                }
                array[i] = first + rest;
            }
            string newname = string.Join("", array);
            if (newname.Length == 0)
            {
                newname = name;
            }
            return newname;
        }
    }
}

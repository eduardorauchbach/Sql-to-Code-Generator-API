using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkUtilities
{
    public static class Helper
    {
        public const string NameMap = @"CREATE TABLE[ ]*(\[[\S]*\]\.)?(\[[\S_]*\])";
        public const string PropertieMap = @"(\[[\S_]*\])[ ]*(\[[\S_]*\])[ ]*(\([\d\, ]*\))?[ ]*([\S ]*)?";
        public const string KeyZoneMap = @"CONSTRAINT (\[[\S_]*\]) PRIMARY KEY [CLUSTERED]*[\s]*\([\s]*(\[[^\s_)]*\][ ]*[\S]*[\s]*)*\)";
        public const string KeyMap = @"(\[[^\s)]*\])[ ]*[\S]*";
        public const string IndexZoneMap = @"INDEX (\[[\S_]*\]) ON (\[[\S]*\]\.)?(\[[\S_]*\])[\s]*\([\s]*(\[[^\s_)]*\][ ]*[\S]*[\s]*)*\)";
        public const string IndexMap = @"(\[[^\s)]*\])[ ]*[\S]*";

        public static string Clear(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return name.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "");
        }


        private static List<string> ReservedWords = new List<string>
        {
            "ID"
        };

        public static string ToCamelCase(this string name)
        {
            string newName;

            if (string.IsNullOrEmpty(name))
            {
                newName = name;
            }
            else if ((name.Contains('_') || name == name.ToLower() || name == name.ToUpper()) && !ReservedWords.Contains(name))
            {
                newName = name.ToLower();

                string[] array = newName.Split('_');
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

                newName = string.Join("", array);
                if (newName.Length == 0)
                {
                    newName = name;
                }
            }
            else
            {
                newName = name;
            }

            return newName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkUtilities.Services
{
    public class SqlToEntityService
    {
        public string Parse(string script)
        {
            const string propertieMap = @"(\[[\S_]*\])[ ]*(\[[\S_]*\])[ ]*(\([\d\, ]*\))?[ ]*([\S ]*)?";

            StringBuilder result = new StringBuilder();

            MatchCollection matches = Regex.Matches(script, propertieMap);

            foreach (Match x in matches.ToList())
            {
                var paramName = x.Groups[1].Value.Clear();
                var paramType = x.Groups[2].Value.Clear();
                var paramLength = x.Groups[3].Value.Clear();
                var paramRequired = x.Groups[4].Value.Clear();

                result.AppendLine($"_ = entity.Property(e => e.{paramName.ToCamelCase()})");

                if (paramType.ToLower().Contains("char"))
                {
                    result.AppendLine($".IsUnicode(false)");
                }
                else if (paramType.ToLower().Contains("date"))
                {
                    result.AppendLine($".HasColumnType(\"datetime\")");
                }
                else if (paramType.ToLower().Contains("numeric"))
                {
                    result.AppendLine($".HasColumnType(\"numeric({paramLength})\")");
                }
                else if (paramType.ToLower().Contains("money"))
                {
                    result.AppendLine($".HasColumnType(\"money({paramLength})\")");
                }

                if (!string.IsNullOrEmpty(paramLength) && !paramLength.Contains(','))
                {
                    result.AppendLine($".HasMaxLength({paramLength})");
                }

                if (paramRequired.ToLower().Contains("not null"))
                {
                    result.AppendLine($".IsRequired(true)");
                }

                result.AppendLine($".HasColumnName(\"{paramName}\");");
                result.AppendLine();
            }

            return result.ToString();
        }


    }
}

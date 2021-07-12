using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkUtilities.Services
{
    public class SqlToClassService
    {
        public string Parse(string script)
        {
            const string propertieMap = @"(\[[\S_]*\])[ ]*(\[[\S_]*\])[ ]*(\([\d]*\))?[ ]*([\S ]*)?";

            StringBuilder result = new StringBuilder();
            MatchCollection matches = Regex.Matches(script, propertieMap);

            foreach (Match x in matches.ToList())
            {
                var paramName = x.Groups[1].Value.Clear();
                var paramType = x.Groups[2].Value.Clear();
                var paramLength = x.Groups[3].Value.Clear();
                var paramRequired = x.Groups[4].Value.Clear();

                string outName = paramName.ToCamelCase();
                string outRequired = paramRequired.ToLower().Contains("not null") ? "" : "?";
                string outType;

                switch (paramType.ToLower())
                {
                    case "int":
                        {
                            outType = "int" + outRequired;
                        }
                        break;

                    case "bigint":
                        {
                            outType = "long" + outRequired;
                        }
                        break;

                    case "char":
                        {
                            outType = "string";
                        }
                        break;

                    case "varchar":
                    case "nvarchar":
                        {
                            outType = "string";
                        }
                        break;

                    case "date":
                    case "datetime":
                        {
                            outType = "DateTime" + outRequired;
                        }
                        break;

                    default:
                        outType = "_?????_";
                        break;
                }

                result.AppendLine($"public {outType} {outName} {{get;set;}}");
                result.AppendLine();
            }

            return result.ToString();
        }
    }
}

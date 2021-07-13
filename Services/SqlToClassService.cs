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
            const string propertieMap = @"(\[[\S_]*\])[ ]*(\[[\S_]*\])[ ]*(\([\d\,]*\))?[ ]*([\S ]*)?";

            StringBuilder result = new StringBuilder();
            MatchCollection matches = Regex.Matches(script, propertieMap);

            string lastType = null;

            foreach (Match x in matches.ToList())
            {
                var paramName = x.Groups[1].Value.Clear();
                var paramType = x.Groups[2].Value.Clear();
                var paramLength = x.Groups[3].Value.Clear();
                var paramRequired = x.Groups[4].Value.Clear();

                string outName = paramName.ToCamelCase();
                string outRequired = paramRequired.ToLower().Contains("not null") ? "" : "?";
                string outType = null;

                switch (paramType.ToLower())
                {
                    case "smallint":
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

                    case "numeric":
                        {
                            outType = "decimal" + outRequired;
                        }
                        break;

                    case "char":
                        {
                            if (paramLength != "1")
                            {
                                outType = "char" + outRequired;
                            }
                            else
                            {
                                outType = "string";
                            }
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
                
                if (lastType != outType)
                {
                    result.AppendLine();
                }
                result.AppendLine($"public {outType} {outName} {{get;set;}}");

                lastType = outType;
            }

            return result.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Models;

namespace WorkUtilities.Helpers
{
	public static class TypesHelper
	{
        public static void GetNetType(this MapperProperty property)
        {
            switch (property.TypeDB.ToLower())
            {
                case "bit":
                    {
                        property.Type = "bool";
                    }
                    break;

                case "smallint":
                    {
                        property.Type = "Int16";
                    }
                    break;
                case "int":
                    {
                        property.Type = "int";
                    }
                    break;
                case "bigint":
                    {
                        property.Type = "long";
                    }
                    break;

                case "smallmoney":
                case "money":
                case "numeric":
                case "decimal":
                    {
                        property.Type = "decimal";
                        property.LengthMain ??= 17;
                        property.LengthDecimal ??= 2;
                    }
                    break;
                case "float":
                    {
                        property.Type = "double";
                    }
                    break;
                case "real":
                    {
                        property.Type = "Single";
                    }
                    break;

                case "date":
                case "smalldatetime":
                case "datetime":
                    {
                        property.Type = "DateTime";
                    }
                    break;

                case "sql_variant":
                    {
                        property.Type = "object";
                    }
                    break;

                case "varbinary":
                case "binary":
                    {
                        property.Type = (property.LengthMain > 1) ? "byte[]" : "byte";
                    }
                    break;
                case "tinyint":
                    {
                        property.Type = "byte";
                    }
                    break;
                case "rowversion":
                    {
                        property.Type = "byte[]";
                    }
                    break;

                case "varchar":
                case "nvarchar":
                case "char":
                    {
                        property.Type = "string";
                    }
                    break;

                case "uniqueidentifier":
                    {
                        property.Type = "Guid";
                    }
                    break;

                default:
                    {
                        property.Type = StringHelper.Unidentified;
                    }
                    break;
            }
        }

        public static string GetSqlType(this MapperProperty property)
        {
            string outType;
            string outLength = null;

            switch (property.Type.ToLower())
            {
                case "bool":
                case "boolean":
                    {
                        outType = "bit";
                    }
                    break;

                case "int16":
                    {
                        outType = "smallint";
                    }
                    break;
                case "int":
                    {
                        outType = "int";
                    }
                    break;
                case "long":
                    {
                        outType = "bigint";
                    }
                    break;

                case "decimal":
                    {
                        outType = "decimal";
                        property.LengthMain ??= 17;
                        property.LengthDecimal ??= 2;

                        outLength = $"({property.LengthMain},{property.LengthDecimal})";
                    }
                    break;
                case "double":
                    {
                        outType = "float";
                    }
                    break;
                case "single":
                    {
                        outType = "real";
                    }
                    break;

                case "datetime":
                    {
                        outType = "datetime";
                    }
                    break;

                case "object":
                    {
                        outType = "sql_variant";
                    }
                    break;

                case "byte":
                    {
                        outType = "byte";
                    }
                    break;
                case "byte[]":
                    {
                        outType = "varbinary";
                        outLength = $"({(property.LengthMain > 0 ? property.LengthMain.ToString() : "MAX")})";
                    }
                    break;

                case "char":
                    {
                        outType = "char";
                        outLength = $"({(property.LengthMain > 0 ? property.LengthMain.ToString() : "MAX")})";
                    }
                    break;
                case "string":
                    {
                        outType = "nvarchar";
                        outLength = $"({(property.LengthMain > 0 ? property.LengthMain.ToString() : "MAX")})";
                    }
                    break;

                case "uniqueidentifier":
                    {
                        outType = "Guid";
                    }
                    break;

                default:
                    {
                        outType = StringHelper.Unidentified;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(property.TypeDB))
            {
                outType = property.TypeDB;
            }
			else
			{
                property.TypeDB = outType;
            }            

            return $"[{outType}]{outLength}";
        }
    }
}

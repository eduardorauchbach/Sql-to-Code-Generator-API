using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkUtilities.Model;

namespace WorkUtilities.Services.Translation
{
    public class TSqlTranslatorService
    {
        public const string MainZoneMap = @"CREATE TABLE ([^() ]*\.)*([^() ]+)[ ]*\((([\s]*([^() ]*)[\s]*([^() ]*)(\([\d\, ]*\))?[ ]?([\S]{4,})?[ ]?(NULL|NOT NULL)[\,\s]*)*)";
        public const string PropertieMap = @"([\s]*([^() ]*)[\s]*([^() ]*)(\([\d\, ]*\))?[ ]?([\S]{4,})?[ ]?(NULL|NOT NULL)[\,\s]?)";

        public const string KeyZoneMap = @"CONSTRAINT ([^()\s]+) PRIMARY KEY ([^()\s]+)*[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        public const string KeyMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";

        public const string IndexZoneMap = @"INDEX ([^()\s]*) ON ([^()\s\.]+\.)?([\[]?####[\]]?)[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        public const string IndexMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";


        public List<EntryModel> Translate(string script)
        {
            List<EntryModel> entryModels;
            EntryModel entryModel;

            MapperProperty entryProperty;

            MatchCollection matchMains;
            MatchCollection matchProperties;
            MatchCollection matchKeyZone;
            MatchCollection matchIndexZone;
            MatchCollection matchIndexers;

            Match matchMain;
            Match matchProperty;
            Match matchKey;
            Match matchIndex;

            string keyZone;

            try
            {
                entryModels = new List<EntryModel>();

                matchMains = Regex.Matches(script, $"{MainZoneMap}({KeyZoneMap})?");
                if (matchMains?.Count > 0)
                {
                    foreach (Match m in matchMains)
                    {
                        matchMain = m; //For Debug Purposes

                        entryModel = new EntryModel();
                        entryModel.Properties = new List<MapperProperty>();
                        entryModel.Relationships = new List<EntryRelationship>();
                        entryModels.Add(entryModel);


                        entryModel.NameDB = m.Groups[2].Value.Clear();
                        entryModel.Name = entryModel.NameDB.ToCamelCase();

                        #region Properties

                        matchProperties = Regex.Matches(m.Groups[3].Value, Helper.PropertieMap);

                        foreach (Match p in matchProperties)
                        {
                            matchProperty = p;

                            var paramName = p.Groups[1].Value.Clear();
                            var paramType = p.Groups[2].Value.Clear().ToLower();
                            var paramLength = p.Groups[3].Value.Clear();
                            var paramRequired = p.Groups[4].Value.Clear();

                            entryProperty = new MapperProperty();
                            entryModel.Properties.Add(entryProperty);

                            entryProperty.Name = paramName.ToCamelCase();
                            entryProperty.NameDB = paramName;

                            if (paramLength?.Length > 0)
                            {
                                var lengths = paramLength.Split(',');

                                entryProperty.LengthMain = int.TryParse(lengths[0], out int lmain) ? lmain : default;

                                if (paramLength.Contains(','))
                                {
                                    entryProperty.LengthDecimal = int.TryParse(lengths[1], out int ldec) ? ldec : default;
                                }
                            }

                            entryProperty.Type = GetType(paramType, entryProperty.LengthMain);
                            entryProperty.TypeDB = paramType;
                            entryProperty.IsFixedLength = (paramType == "char");

                            entryProperty.IsRequired = paramRequired.ToLower().Contains("not null");
                        }

                        #endregion

                        #region Keys

                        keyZone = matchMain.Groups[13]?.Value;

                        if (!string.IsNullOrEmpty(keyZone))
                        {
                            matchKeyZone = Regex.Matches(keyZone, KeyMap);
                            foreach (Match k in matchKeyZone)
                            {
                                matchKey = k;

                                var keyName = k.Groups[1].Value.Clear();

                                foreach (MapperProperty p in entryModel.Properties.FindAll(p => p.NameDB == keyName))
                                {
                                    p.IsKey = true;
                                }
                            }
                        }

                        #endregion

                        #region Indexers

                        matchIndexZone = Regex.Matches(script, IndexZoneMap.Replace("####", entryModel.NameDB));

                        foreach (Match iz in matchIndexZone)
                        {
                            matchIndexers = Regex.Matches(iz.Groups[4]?.Value, IndexMap);
                            foreach (Match i in matchIndexers)
                            {
                                matchIndex = i;

                                var indexName = i.Groups[1].Value.Clear();

                                foreach (MapperProperty p in entryModel.Properties.FindAll(p => p.NameDB == indexName))
                                {
                                    p.IsIndex = true;
                                }
                            }
                        }

                        #endregion
                    }
                }
            }
            catch
            {
                throw;
            }

            return entryModels;
        }

        private string GetType(string sqlType, int? lengthMain)
        {
            string outType;

            switch (sqlType.ToLower())
            {
                case "bit":
                    {
                        outType = "bool";
                    }
                    break;

                case "smallint":
                    {
                        outType = "Int16";
                    }
                    break;
                case "int":
                    {
                        outType = "int";
                    }
                    break;
                case "bigint":
                    {
                        outType = "long";
                    }
                    break;

                case "smallmoney":
                case "money":
                case "numeric":
                case "decimal":
                    {
                        outType = "long";
                    }
                    break;
                case "float":
                    {
                        outType = "double";
                    }
                    break;
                case "real":
                    {
                        outType = "Single";
                    }
                    break;

                case "smalldatetime":
                case "datetime":
                    {
                        outType = "DateTime";
                    }
                    break;

                case "sql_variant":
                    {
                        outType = "object";
                    }
                    break;

                case "varbinary":
                case "binary":
                    {
                        outType = (lengthMain > 1) ? "byte[]" : "byte";
                    }
                    break;
                case "tinyint":
                    {
                        outType = "byte";
                    }
                    break;
                case "rowversion":
                    {
                        outType = "byte[]";
                    }
                    break;

                case "varchar":
                case "nvarchar":
                case "char":
                    {
                        outType = "string";
                    }
                    break;

                case "uniqueidentifier":
                    {
                        outType = "Guid";
                    }
                    break;

                default:
                    {
                        outType = "??????????";
                    }
                    break;
            }

            return outType;
        }
    }
}

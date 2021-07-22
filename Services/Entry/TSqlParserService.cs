using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkUtilities.Model;

namespace WorkUtilities.Services.Entry
{
    public class TSqlParserService
    {
        private const string MainZoneMap = @"CREATE TABLE ([^() ]*\.)*([^() ]+)[ ]*\((([\s]*([^() ]*)[\s]*([^() ]*)(\([\d\, ]*\))?[ ]?([\S]{4,})?[ ]?(NULL|NOT NULL)[\,\s]*)*)";
        private const string PropertieMap = @"([\s]*([^() ]*)[\s]*([^() ]*)(\([\d\, ]*\))?[ ]?([\S]{4,})?[ ]?(NULL|NOT NULL)[\,\s]?)";

        private const string KeyZoneMap = @"CONSTRAINT ([^()\s]+) PRIMARY KEY ([^()\s]+)*[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        private const string KeyMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";

        private const string IndexZoneMap = @"INDEX ([^()\s]*) ON ([^()\s\.]+\.)?([\[]?####[\]]?)[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        private const string IndexMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";

        private const string RelationshipZoneMap = @"ALTER TABLE ([^() ]+\.)?([^() ]*)[^()\.\[\]]*([^() ]*)[\s]?FOREIGN KEY\(([^() ]*)\)[\s]*REFERENCES ([^() ]+\.)?([^() ]*)[\s]?\(([^() ]*)\)";

        public List<EntryModel> Translate(string script)
        {
            List<EntryModel> entryModels;
            EntryModel entryModel;
            EntryModel entryModelParent;
            EntryRelationship entryRelationship;

            MapperProperty entryProperty;

            MatchCollection matchMains;
            MatchCollection matchProperties;
            MatchCollection matchKeyZone;
            MatchCollection matchIndexZone;
            MatchCollection matchIndexers;
            MatchCollection matchRelationships;

            Match matchMain;
            Match matchProperty;
            Match matchKey;
            Match matchIndex;
            Match matchRelationship;

            string keyZone;

            string paramName;
            string paramType;
            string paramLength;
            string paramRequired;

            string originName;
            string originProperty;
            string parentName;
            string parentProperty;

            string keyName;
            string indexName;

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

                        matchProperties = Regex.Matches(m.Groups[3].Value, PropertieMap, RegexOptions.IgnoreCase);

                        foreach (Match p in matchProperties)
                        {
                            matchProperty = p;

                            paramName = p.Groups[2].Value.Clear();
                            paramType = p.Groups[3].Value.Clear().ToLower();
                            paramLength = p.Groups[4].Value.Clear();
                            paramRequired = p.Groups[6].Value.Clear();

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
                            matchKeyZone = Regex.Matches(keyZone, KeyMap, RegexOptions.IgnoreCase);
                            foreach (Match k in matchKeyZone)
                            {
                                matchKey = k;

                                keyName = k.Groups[1].Value.Clear();

                                foreach (MapperProperty p in entryModel.Properties.FindAll(p => p.NameDB == keyName))
                                {
                                    p.IsKey = true;
                                }
                            }
                        }

                        #endregion

                        #region Indexers

                        matchIndexZone = Regex.Matches(script, IndexZoneMap.Replace("####", entryModel.NameDB), RegexOptions.IgnoreCase);

                        foreach (Match iz in matchIndexZone)
                        {
                            matchIndexers = Regex.Matches(iz.Groups[4]?.Value, IndexMap, RegexOptions.IgnoreCase);
                            foreach (Match i in matchIndexers)
                            {
                                matchIndex = i;

                                indexName = i.Groups[1].Value.Clear();

                                foreach (MapperProperty p in entryModel.Properties.FindAll(p => p.NameDB == indexName))
                                {
                                    p.IsIndex = true;
                                }
                            }
                        }

                        #endregion                        
                    }

                    #region Relationship

                    matchRelationships = Regex.Matches(script, RelationshipZoneMap, RegexOptions.IgnoreCase);

                    foreach (Match r in matchRelationships)
                    {
                        matchRelationship = r;

                        originName = r.Groups[2].Value.Clear();
                        originProperty = r.Groups[4].Value.Clear();
                        parentName = r.Groups[6].Value.Clear();
                        parentProperty = r.Groups[7].Value.Clear();

                        entryModel = entryModels.Find(e => e.NameDB == originName);
                        entryProperty = entryModel.Properties.Find(p => p.NameDB == originProperty);
                        if (entryProperty != null)
                        {
                            entryProperty.ParentName = parentName.ToCamelCase();
                            entryProperty.ParentKey = parentProperty.ToCamelCase();
                        }

                        entryModelParent = entryModels.Find(e => e.NameDB == parentName);
                        if (entryModelParent != null)
                        {
                            entryRelationship = new EntryRelationship();
                            entryModelParent.Relationships.Add(entryRelationship);

                            entryRelationship.TargetName = originName;

                            if (entryProperty.IsKey)
                            {
                                entryRelationship.Type = RelationshipType.IN_1_OUT_1;
                            }
                            else
                            {
                                entryRelationship.Type = RelationshipType.IN_1_OUT_N;
                            }
                        }
                    }

                    #endregion
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
                        outType = "decimal";
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Services.Parser
{
    public class EntryParserService
    {
        #region Sql to EntryModel

        private const string MainZoneMap = @"CREATE TABLE ([^() ]*\.)*([^() ]+)[ ]*\((####*)[\n\r\t ]*";
        private const string PropertieMap = @"([\s]*([^(), ]*)[\s]*([^(), ]*)(\([\d\, ]*\))?[ ]?([\S]{4,})?[ ]?(NULL|NOT NULL)([^,]*)?[\,\s]?)";

        private const string KeyZoneMap = @"CONSTRAINT ([^()\s]+) PRIMARY KEY ([^()\s]+)*[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        private const string KeyMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";

        private const string IndexZoneMap = @"INDEX ([^()\s]*) ON ([^()\s\.]+\.)?([\[]?####[\]]?)[\s]*\(([\s]*([^()\s]+[\s]*[^()\s]+[\s]*)*)\)";
        private const string IndexMap = @"([^()\s)]+)[ ]*[^()\s]*[\,]?";

        private const string RelationshipZoneMap = @"ALTER TABLE ([^() ]+\.)?([^()\n ]*)[^()\.\[\]]*CONSTRAINT ([^() ]*)[\s]?FOREIGN KEY[ ]*\(([^() ]*)\)[\s]*REFERENCES ([^() ]+\.)?([^() ]*)[\s]?\(([^() ]*)\)";

        public List<EntryModel> ParseFromSql(string script)
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
            string paramIdentity;
            string paramRequired;

            string originName;
            string originProperty;
            string parentName;
            string parentProperty;

            string keyName;
            string indexName;
            string indexGroup;

            try
            {
                entryModels = new List<EntryModel>();

                matchMains = Regex.Matches(script, $"{MainZoneMap.Replace("####", PropertieMap)}({KeyZoneMap})?", RegexOptions.IgnoreCase);
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
                            paramIdentity = p.Groups[5].Value.Clear();
                            paramRequired = p.Groups[6].Value;

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

                            entryProperty.TypeDB = paramType;
                            entryProperty.GetNetType();

                            entryProperty.IsAutoGenerated = paramIdentity.ToLower().Contains("identity");
                            entryProperty.IsFixedLength = (paramType == "char");
                            entryProperty.IsRequired = paramRequired.ToLower().Contains("not null");
                        }

                        #endregion

                        #region Keys

                        keyZone = matchMain.Groups[14]?.Value;

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
                            indexGroup = iz.Groups[1]?.Value.Clear();

                            matchIndexers = Regex.Matches(iz.Groups[4]?.Value, IndexMap, RegexOptions.IgnoreCase);
                            foreach (Match i in matchIndexers)
                            {
                                matchIndex = i;

                                indexName = i.Groups[1].Value.Clear();

                                foreach (MapperProperty p in entryModel.Properties.FindAll(p => p.NameDB == indexName))
                                {
                                    p.IsIndex = true;
                                    p.IndexGroup ??= new List<string>();

                                    p.IndexGroup.Add(indexGroup);
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
                            entryProperty.ParentName = parentName;
                            entryProperty.ParentKey = parentProperty;
                        }

                        entryModelParent = entryModels.Find(e => e.NameDB == parentName);
                        if (entryModelParent != null)
                        {
                            entryRelationship = new EntryRelationship();
                            entryModelParent.Relationships.Add(entryRelationship);

                            entryRelationship.TargetName = entryModel.Name;

                            if (entryProperty.IsKey && entryModel.Properties.Count(x=>x.IsKey) == 1)
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

        #endregion

        #region EntryModel to Sql

        public string ParseToSql(List<EntryModel> entryModels)
        {
            StringBuilder script;
            StringBuilder scriptRelations;
            StringBuilder scriptIndexs;

            EntryModel entryModel;
            EntryModel entryParent;
            MapperProperty property;
            MapperProperty propertyParent;

            string currentTable;
            string currentProperty;
            string currentType;
            string currentIdentity;
            string currentNullable;

            string currentParentTable;
            string currentParentKey;

            try
            {
                script = new StringBuilder();
                scriptRelations = new StringBuilder();
                scriptIndexs = new StringBuilder();

                foreach (EntryModel e in entryModels)
                {
                    entryModel = e;
                    currentTable = e.NameDB;

                    script.AppendLine($"CREATE TABLE [{currentTable}]\n(");

                    foreach (MapperProperty p in e.Properties)
                    {
                        property = p;                        

                        currentProperty = p.NameDB;
                        currentType = p.GetSqlType();
                        currentIdentity = GetAutoGeneration(p);
                        currentNullable = p.IsRequired ? "NOT NULL" : "NULL";

                        script.Append($"\t[{currentProperty}] {currentType} {currentIdentity} {currentNullable}");

                        if (!e.Properties.Last().Equals(p))
                        {
                            script.AppendLine(",");
                        }


                        if (!string.IsNullOrEmpty(p.ParentName) && !string.IsNullOrEmpty(p.ParentKey))
                        {
                            entryParent = entryModels.Find(x => x.Name == p.ParentName || x.NameDB == p.ParentName);
                            if (entryParent != null)
                            {
                                propertyParent = entryParent.Properties.Find(x => x.Name == p.ParentKey || x.NameDB == p.ParentKey);

                                currentParentTable = entryParent.NameDB;
                                currentParentKey = propertyParent.NameDB;
                            }
                            else
                            {
                                currentParentTable = p.ParentName;
                                currentParentKey = p.ParentKey;
                            }

                            scriptRelations.AppendLine($"ALTER TABLE [{currentTable}] WITH NOCHECK ADD CONSTRAINT[FK_{currentTable}_{currentParentTable}_{currentProperty}]");
                            scriptRelations.AppendLine($"FOREIGN KEY([{currentProperty}]) REFERENCES [{currentParentTable}]([{currentParentKey}])");
                            scriptRelations.AppendLine();
                        }
                    }

                    if (e.Properties.Any(x => x.IsKey))
                    {
                        script.AppendLine(",");
                        script.AppendLine($"\tCONSTRAINT [PK_{currentTable}] PRIMARY KEY CLUSTERED (");

                        var keysSelections = e.Properties.Where(x => x.IsKey);

                        foreach (MapperProperty p in keysSelections)
                        {
                            property = p;
                            currentProperty = $"[{p.NameDB}]";

                            script.Append($"\t\t{currentProperty} ASC");

                            if (!keysSelections.Last().Equals(p))
                            {
                                script.AppendLine(",");
                            }
                        }

                        script.AppendLine("\n\t)");
                    }
                    script.AppendLine(")\n");

                    if (e.Properties.Any(x => x.IsIndex))
                    {
                        var indexGroupsSelections = e.Properties.Where(x => x.IndexGroup != null).SelectMany(x => x.IndexGroup).Distinct();

                        foreach (string indexGroup in indexGroupsSelections)
                        {
                            scriptIndexs.AppendLine($"CREATE NONCLUSTERED INDEX [{indexGroup}] ON [{currentTable}] \n(");

                            var indexersSelections = e.Properties.Where(x => x.IsIndex && x.IndexGroup.Contains(indexGroup));

                            foreach (MapperProperty p in indexersSelections)
                            {
                                property = p;
                                currentProperty = $"[{p.NameDB}]";

                                scriptIndexs.Append($"\t{currentProperty} ASC");

                                if (!indexersSelections.Last().Equals(p))
                                {
                                    scriptIndexs.AppendLine(",");
                                }
                            }

                            scriptIndexs.AppendLine("\n)");
                            scriptIndexs.AppendLine();
                        }
                    }
                }

                script.AppendLine();
                script.Append(scriptRelations);
                script.AppendLine();
                script.Append(scriptIndexs);
            }
            catch
            {
                throw;
            }

            return script.ToString();
        }

        private static string GetAutoGeneration(MapperProperty property)
        {
            string outIdentity = null;

            if (property.IsAutoGenerated)
            {
                switch (property.Type.ToLower())
                {
                    case "guid":
                        {
                            outIdentity = "DEFAULT NEWID()";
                        }
                        break;
                    default:
                        {
                            outIdentity = "IDENTITY(1,1)";
                        }
                        break;
                }
            }

            return outIdentity;
        }        

        #endregion
    }
}

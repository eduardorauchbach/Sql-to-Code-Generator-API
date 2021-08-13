using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Services.Generator
{
    public class RepositoryGeneratorService
    {
        public List<string> ParseFromGenerator(GeneratorModel model)
        {
            List<string> result = new List<string>();

            foreach (EntryModel e in model.EntryModels)
            {
                e.PreProcess();
                result.Add(ParseFromEntry(model.ProjectName, model, e));
            }

            return result;
        }

        public string ParseFromEntry(string projectName, GeneratorModel model, EntryModel entry)
        {
            StringBuilder result;
            int tab;

            try
            {
                result = new StringBuilder();
                tab = 0;

                result.AppendCode(tab, "using Microsoft.Data.SqlClient;", 1);
                result.AppendCode(tab, "using Microsoft.Extensions.Configuration;", 1);
                result.AppendCode(tab, "using System;", 1);
                result.AppendCode(tab, "using System.Collections.Generic;", 1);
                result.AppendCode(tab, "using System.Data;", 1);
                result.AppendCode(tab, "using System.Linq;", 1);
                result.AppendCode(tab, "using RauchTech.Common.Extensions;", 1);
                result.AppendCode(tab, "using RauchTech.Common.Model;", 1);
                result.AppendCode(tab, "using RauchTech.DataExtensions.Sql;", 1);
                result.AppendCode(tab, $"using {projectName}.Domain.Model;", 2);

                result.AppendCode(tab, $"namespace {projectName}.Domain.Repository", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"public class {entry.Name}Repository", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        BuildConstructor(result, tab, entry);

                        result.AppendCode(tab, "#region LoadModel", 2);

                        BuildModelLoader(result, tab, entry);

                        result.AppendCode(tab, "#endregion", 2);

                        result.AppendCode(tab, "#region Change Data", 2);

                        BuildInserter(result, tab, entry);

                        BuildUpdater(result, tab, entry);

                        BuildDeleter(result, tab, model, entry);

                        result.AppendCode(tab, "#endregion", 2);

                        result.AppendCode(tab, "#region Retrieve Data", 2);

                        BuildGetterByID(result, tab, entry);

                        BuildGetter(result, tab, model, entry);

                        result.AppendCode(tab, "#endregion", 2);
                    }
                    tab--;
                    result.AppendCode(tab, "}", 2);
                }
                tab--;
                result.AppendCode(tab, "}", 1);
            }
            catch
            {
                throw;
            }

            return result.ToString();
        }

        #region Header

        private void BuildConstructor(StringBuilder result, int tab, EntryModel entry)
        {
            result.AppendCode(tab, "private readonly IConfiguration _config;", 1);
            result.AppendCode(tab, "private readonly ISqlHelper _sqlHelper;", 2);

            result.AppendCode(tab, $"public {entry.Name}Repository(IConfiguration configuration, ISqlHelper sqlHelper)", 1);
            result.AppendCode(tab, "{", 1);
            tab++;

            result.AppendCode(tab, "_config = configuration;", 1);
            result.AppendCode(tab, "_sqlHelper = sqlHelper;", 1);

            tab--;
            result.AppendCode(tab, "}", 2);
        }

        private void BuildModelLoader(StringBuilder result, int tab, EntryModel entry)
        {
            string varModelName;
            string required;

            MapperProperty property;

            try
            {
                varModelName = entry.Name.ToCamelCase(true);

                result.AppendCode(tab, $"private List<{entry.Name}> Load(DataSet data)", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"List<{entry.Name}> {varModelName}s;", 1);
                    result.AppendCode(tab, $"{entry.Name} {varModelName};", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, $"{varModelName}s = new List<{entry.Name}>();", 2);

                        result.AppendCode(tab, $"foreach (DataRow row in data.Tables[0].Rows)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            result.AppendCode(tab, $"{varModelName} = new {entry.Name}();", 2);

                            foreach (MapperProperty p in entry.Properties)
                            {
                                property = p;
                                required = string.Empty;

                                if (!p.IsRequired && p.Type.ToLower() != "string")
                                {
                                    required = "?";
                                }

                                result.AppendCode(tab, $"{varModelName}.{p.Name} = row.Field<{(p.Type + required)}>(\"{p.NameDB}\");", 1);
                            }
                            result.AppendLine();

                            result.AppendCode(tab, $"{varModelName}s.Add({varModelName});", 1);

                            tab--; //end foreach
                            result.AppendCode(tab, "}", 1);
                        }
                        tab--; //end try
                        result.AppendCode(tab, "}", 1);

                        result.AppendCode(tab, "catch", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;

                        result.AppendCode(tab, "throw;", 1);
                    }
                    tab--; //end catch
                    result.AppendCode(tab, "}", 2);
                    result.AppendCode(tab, $"return {varModelName}s;", 1);
                }
                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Insert

        private void BuildInserter(StringBuilder result, int tab, EntryModel entry)
        {
            bool onlyKeys;

            try
            {
                onlyKeys = entry.Properties.All(x => x.IsKey);

                if (onlyKeys)
                {
                    BuildLinkInserter(result, tab, entry);
                }
                else
                {
                    BuildModelInserter(result, tab, entry);
                }
            }
            catch
            {
                throw;
            }
        }

        private void BuildLinkInserter(StringBuilder result, int tab, EntryModel entry)
        {
            string varModelName;

            MapperProperty mainKey;
            MapperProperty property;

            try
            {
                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                result.AppendCode(tab, $"public void Insert(List<{entry.Name}> {varModelName}s)", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"SqlCommand command;", 2);
                    result.AppendCode(tab, $"{entry.Name} {varModelName};", 1);
                    result.AppendCode(tab, $"List<string> clauses;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, $"if ({varModelName}s.Count > 0)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            #region Insert Command

                            result.AppendCode(tab, $"command = new SqlCommand(\" INSERT INTO {entry.NameDB} \" +", 1);
                            tab += 3;
                            {
                                result.AppendCode(tab, "\" (\" +", 1);
                                tab++;
                                {
                                    foreach (MapperProperty p in entry.Properties)
                                    {
                                        property = p;

                                        result.AppendCode(tab, $"\" {(!entry.Properties.First().Equals(p) ? "," : " ")}");

                                        result.AppendLine($"{p.NameDB}\" +");
                                    }
                                }
                                tab--;
                                result.AppendCode(tab, "\" )\" +", 1);
                                result.AppendCode(tab, "\" VALUES \");", 1);
                            }
                            tab -= 3;
                            result.AppendLine();

                            #endregion

                            result.AppendCode(tab, "clauses = new List<string>();", 2);

                            result.AppendCode(tab, $"for (int i = 0; i < {varModelName}s.Count; i++)", 1);
                            result.AppendCode(tab, "{", 1);
                            tab++;
                            {
                                result.AppendCode(tab, $"{varModelName} = {varModelName}s[i];", 2);

                                result.AppendCode(tab, $"clauses.Add($\"({string.Join(", ", (entry.Properties.Select(x => "@" + x.Name + "{i}")))})\");", 1);
                                foreach (MapperProperty p in entry.Properties)
                                {
                                    property = p;

                                    result.AppendCode(tab, $"command.Parameters.AddWithValue($\"{p.Name + "{i}"}\", {varModelName}.{p.Name}.AsDbValue());", 1);
                                }
                            }
                            tab--; //end for
                            result.AppendCode(tab, "}", 1);
                            result.AppendLine();

                            result.AppendCode(tab, $"command.CommandText += string.Join(\", \", clauses);", 1);
                            result.AppendCode(tab, $"_sqlHelper.ExecuteScalar(command);", 1);
                        }
                        tab--; //end if
                        result.AppendCode(tab, "}", 1);

                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 1);
                }

                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        private void BuildModelInserter(StringBuilder result, int tab, EntryModel entry)
        {
            string varModelName;

            MapperProperty mainKey;
            MapperProperty property;

            List<MapperProperty> propertiesToInsert;

            try
            {
                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                result.AppendCode(tab, $"public {entry.Name} Insert({entry.Name} {varModelName})", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"SqlCommand command;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        #region Insert Command

                        propertiesToInsert = entry.Properties.Where(x => x != mainKey).ToList();

                        result.AppendCode(tab, $"command = new SqlCommand(\" INSERT INTO {entry.NameDB} \" +", 1);
                        tab += 3;
                        {
                            result.AppendCode(tab, "\" (\" +", 1);
                            tab++;
                            {
                                foreach (MapperProperty p in propertiesToInsert)
                                {
                                    property = p;

                                    result.AppendCode(tab, $"\" {(!propertiesToInsert.First().Equals(p) ? "," : " ")}");

                                    result.AppendLine($"{p.NameDB}\" +");
                                }
                            }
                            tab--;
                            result.AppendCode(tab, "\" )\" +", 1);
                            result.AppendCode(tab, $"\" OUTPUT inserted.{mainKey.NameDB} \" +", 1);
                            result.AppendCode(tab, $"\" VALUES \" +", 1);
                            result.AppendCode(tab, "\" (\" +", 1);
                            tab++;
                            {
                                foreach (MapperProperty p in propertiesToInsert)
                                {
                                    property = p;

                                    result.AppendCode(tab, $"\" {(!propertiesToInsert.First().Equals(p) ? "," : " ")}");

                                    result.AppendLine($"@{p.Name}\" +");
                                }
                            }
                            tab--; //end insert
                            result.AppendCode(tab, "\" )\");", 1);
                        }
                        tab -= 3;
                        result.AppendLine();

                        #endregion

                        foreach (MapperProperty p in propertiesToInsert)
                        {
                            property = p;

                            result.AppendCode(tab, $"command.Parameters.AddWithValue(\"{p.Name}\", {varModelName}.{p.Name}.AsDbValue());", 1);
                        }
                        result.AppendLine();

                        result.AppendCode(tab, $"{varModelName}.{mainKey.NameDB} = ({mainKey.Type})_sqlHelper.ExecuteScalar(command);", 1);
                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 2);
                    result.AppendCode(tab, $"return {varModelName};", 1);
                }
                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Update

        private void BuildUpdater(StringBuilder result, int tab, EntryModel entry)
        {
            bool onlyKeys;
            string varModelName;

            MapperProperty mainKey;
            MapperProperty property;
            List<MapperProperty> propertiesNotKeys;

            try
            {
                onlyKeys = entry.Properties.All(x => x.IsKey);
                if (!onlyKeys)
                {
                    mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                    varModelName = entry.Name.ToCamelCase(true);

                    result.AppendCode(tab, $"public {entry.Name} Update({entry.Name} {varModelName})", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, $"SqlCommand command;", 2);

                        result.AppendCode(tab, "try", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            #region Update Command

                            result.AppendCode(tab, $"command = new SqlCommand(\" UPDATE {entry.Name} SET \" +", 2);
                            tab += 3;
                            {
                                propertiesNotKeys = entry.Properties.Where(x => x != mainKey).ToList();
                                foreach (MapperProperty p in propertiesNotKeys)
                                {
                                    property = p;

                                    result.AppendCode(tab, $"\" {(!propertiesNotKeys.First().Equals(p) ? "," : " ")}");

                                    result.AppendLine($"{p.NameDB} = @{p.Name}\" +");
                                }
                                result.AppendLine();

                                result.AppendCode(tab, $"\" WHERE {mainKey.NameDB} = @{mainKey.Name}\");", 2);
                            }
                            tab -= 3;

                            #endregion

                            foreach (MapperProperty p in entry.Properties)
                            {
                                property = p;

                                result.AppendCode(tab, $"command.Parameters.AddWithValue(\"{p.Name}\", {varModelName}.{p.Name}.AsDbValue());", 1);
                            }

                            result.AppendLine();
                            result.AppendCode(tab, "_sqlHelper.ExecuteNonQuery(command);", 1);
                        }
                        tab--; //end try
                        result.AppendCode(tab, "}", 1);

                        result.AppendCode(tab, "catch", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;

                        result.AppendCode(tab, "throw;", 1);

                        tab--; //end catch
                        result.AppendCode(tab, "}", 2);
                        result.AppendCode(tab, $"return {varModelName};", 1);
                    }
                    tab--;
                    result.AppendCode(tab, "}", 2);
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Delete

        private void BuildDeleter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            bool onlyKeys;

            try
            {
                onlyKeys = entry.Properties.All(x => x.IsKey);

                if (onlyKeys)
                {
                    BuildLinkDeleter(result, tab, model, entry);
                }
                else
                {
                    BuildModelDeleter(result, tab, model, entry);
                }
            }
            catch
            {
                throw;
            }
        }

        private void BuildLinkDeleter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            string varModelName;

            MapperProperty mainKey;
            MapperProperty property;

            try
            {
                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                result.AppendCode(tab, $"public void Delete(List<{entry.Name}> {varModelName}s)", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"SqlCommand command;", 2);
                    result.AppendCode(tab, $"{entry.Name} {varModelName};", 1);
                    result.AppendCode(tab, $"List<string> clauses;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, $"if ({varModelName}s.Count > 0)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            #region Delete Command

                            result.AppendCode(tab, $"command = new SqlCommand(\" DELETE from {entry.NameDB} \");", 2);

                            result.AppendCode(tab, "clauses = new List<string>();", 2);

                            result.AppendCode(tab, $"for (int i = 0; i < {varModelName}s.Count; i++)", 1);
                            result.AppendCode(tab, "{", 1);
                            tab++;
                            {
                                result.AppendCode(tab, $"{varModelName} = {varModelName}s[i];", 2);

                                result.AppendCode(tab, $"clauses.Add($\"({string.Join(" AND ", (entry.Properties.Select(x => $"{x.NameDB} = @{x.Name}{{i}}")))})\");", 1);
                                foreach (MapperProperty p in entry.Properties)
                                {
                                    property = p;

                                    result.AppendCode(tab, $"command.Parameters.AddWithValue($\"{p.Name + "{i}"}\", {varModelName}.{p.Name}.AsDbValue());", 1);
                                }
                            }
                            tab--; //end for
                            result.AppendCode(tab, "}", 1);
                            result.AppendLine();

                            #endregion

                            result.AppendCode(tab, $"if (clauses.Count > 0)", 1);
                            result.AppendCode(tab, "{", 1);
                            tab++;

                            result.AppendCode(tab, "command.CommandText += $\" WHERE { string.Join(\" OR \", clauses)}\";", 1);

                            tab--; //end if
                            result.AppendCode(tab, "}", 2);
                            result.AppendCode(tab, $"_sqlHelper.ExecuteScalar(command);", 1);
                        }
                        tab--; //end if
                        result.AppendCode(tab, "}", 1);

                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 1);
                }

                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        private void BuildModelDeleter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            string varModelName;
            string linkForeignKey;
            string deleteBlock;

            MapperProperty mainKey;
            EntryModel currentEntry;
            List<EntryModel> linkedEntries;

            try
            {
                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                result.AppendCode(tab, $"public bool Delete({mainKey.Type} id)", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"SqlCommand command;", 2);
                    result.AppendCode(tab, $"int result;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        #region Delete Command

                        linkedEntries = entry.GetDependents(model);

                        result.AppendCode(tab, $"command = new SqlCommand(", 0);
                        tab += 4;
                        {
                            foreach (EntryModel l in linkedEntries)
                            {
                                currentEntry = l;
                                linkForeignKey = l.Properties.Find(x => x.ParentName == entry.Name).Name;

                                deleteBlock = $"\" DELETE from {l.Name} where {linkForeignKey} = @{mainKey.Name} \" +";

                                if (linkedEntries.First().Equals(l))
                                {
                                    result.AppendLine(deleteBlock);
                                }
                                else
                                {
                                    result.AppendCode(tab, deleteBlock, 1);
                                }
                            }
                            result.AppendCode(tab, $"\" DELETE from {entry.Name} where {mainKey.NameDB} = @{mainKey.Name} \");", 2);
                        }
                        tab -= 4;

                        #endregion

                        result.AppendCode(tab, $"command.Parameters.AddWithValue(\"{mainKey.Name}\", id.AsDbValue());", 1);

                        result.AppendCode(tab, "result = _sqlHelper.ExecuteNonQuery(command);", 1);
                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 2);
                    result.AppendCode(tab, $"return (result > 0);", 1);
                }
                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Retrieve Data

        private void BuildGetterByID(StringBuilder result, int tab, EntryModel entry)
        {
            bool onlyKeys;
            string varModelName;

            MapperProperty mainKey;

            try
            {
                onlyKeys = entry.Properties.All(x => x.IsKey);
                if (!onlyKeys)
                {
                    mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                    varModelName = entry.Name.ToCamelCase(true);

                    result.AppendCode(tab, $"public {entry.Name} Get({mainKey.Type} id)", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, "SqlCommand command;", 1);
                        result.AppendCode(tab, "DataSet dataSet;", 2);

                        result.AppendCode(tab, $"{entry.Name} {varModelName};", 2);

                        result.AppendCode(tab, "try", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            result.AppendCode(tab, $"command = new SqlCommand(\" SELECT * FROM {entry.NameDB} WHERE {mainKey.NameDB} = @ID\");", 1);
                            result.AppendCode(tab, "command.Parameters.AddWithValue(\"ID\", id.AsDbValue());", 2);

                            result.AppendCode(tab, "dataSet = _sqlHelper.ExecuteDataSet(command);", 2);

                            result.AppendCode(tab, $"{varModelName} = Load(dataSet).FirstOrDefault();", 2);
                        }
                        tab--; //end try
                        result.AppendCode(tab, "}", 1);

                        result.AppendCode(tab, "catch", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;

                        result.AppendCode(tab, "throw;", 1);

                        tab--; //end catch
                        result.AppendCode(tab, "}", 2);
                        result.AppendCode(tab, $"return {varModelName};", 1);
                    }
                    tab--;
                    result.AppendCode(tab, "}", 2);
                }
            }
            catch
            {
                throw;
            }
        }

        private void BuildGetter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            bool onlyKeys;

            try
            {
                onlyKeys = entry.Properties.All(x => x.IsKey);

                if (onlyKeys)
                {
                    BuildLinkGetter(result, tab, model, entry);
                }
                else
                {
                    BuildModelGetter(result, tab, model, entry);
                }
            }
            catch
            {
                throw;
            }
        }

        private void BuildLinkGetter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            string varModelName;
            List<string> functionParameters;

            MapperProperty mainKey;
            MapperProperty property;

            try
            {
                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                functionParameters = entry.Properties.GetFunctionParameters();

                result.AppendCode(tab, $"public List<{entry.Name}> Get({string.Join(", ", functionParameters)})", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"SqlCommand command;", 1);
                    result.AppendCode(tab, $"DataSet dataSet;", 2);

                    result.AppendCode(tab, $"List<{entry.Name}> {varModelName}s;", 1);
                    result.AppendCode(tab, $"List<string> clauses;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        #region Select Command

                        result.AppendCode(tab, $"command = new SqlCommand(\" SELECT * FROM {entry.NameDB} \");", 2);

                        result.AppendCode(tab, "clauses = new List<string>();", 2);

                        foreach (MapperProperty p in entry.Properties)
                        {
                            property = p;
                            BuildClauseAggregator(result, tab, p);
                        }

                        #endregion

                        result.AppendCode(tab, $"if (clauses.Count > 0)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;

                        result.AppendCode(tab, "command.CommandText += $\" WHERE { string.Join(\" AND \", clauses)}\";", 1);

                        tab--; //end if
                        result.AppendCode(tab, "}", 2);
                        result.AppendCode(tab, $"dataSet = _sqlHelper.ExecuteDataSet(command);", 2);

                        result.AppendCode(tab, $"{varModelName}s = Load(dataSet);", 2);
                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 2);
                    result.AppendCode(tab, $"return {varModelName}s;", 1);
                }

                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        private void BuildModelGetter(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            StringBuilder commandBase;

            string varModelName;
            List<string> functionParameters;
            List<string> joinParameters;

            MapperProperty mainKey;
            List<MapperProperty> functionProperties;
            List<MapperProperty> linkedEntriesProperties;
            List<MapperProperty> JoinProperties;

            List<(string Prefix, EntryModel Entry)> linkedEntries;

            try
            {
                commandBase = new StringBuilder();

                mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ??
                            new MapperProperty
                            {
                                Name = StringHelper.Unidentified,
                                NameDB = StringHelper.Unidentified,
                                Type = StringHelper.Unidentified
                            };
                varModelName = entry.Name.ToCamelCase(true);

                //Desired Usage
                //functionProperties = entry.Properties.Where(x => x.IsIndex).ToList();
                functionProperties = entry.Properties.Where(x => !x.IsKey).ToList();
                functionParameters = functionProperties.GetFunctionParameters();

                #region External Links

                linkedEntries = entry.GetDependents(model).Select(x => ("", x)).ToList();
                if (linkedEntries?.Count > 0)
                {
                    linkedEntries = linkedEntries.Where(x => x.Entry.Properties.All(y => y.IsKey)).ToList();

                    for (int i = 0; i < linkedEntries.Count; i++)
                    {
                        linkedEntries[i] = ((i + 1).ToLetters(), linkedEntries[i].Entry);
                    }

                    linkedEntriesProperties = linkedEntries.SelectMany(x => x.Entry.Properties).Where(x => x.ParentName != entry.NameDB).ToList();
                    functionParameters.AddRange(linkedEntriesProperties.GetFunctionParameters());
                }

                #endregion

                result.AppendCode(tab, $"public PageModel<{entry.Name}> Get({string.Join(", ", functionParameters)}, PageModel<{entry.Name}> page = null)", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, "SqlCommand commandCount;", 1);
                    result.AppendCode(tab, "SqlCommand commandWhere;", 1);
                    result.AppendCode(tab, "DataSet dataSet;", 2);

                    result.AppendCode(tab, "List<string> clauses;", 2);
                    result.AppendCode(tab, "int count;", 2);

                    result.AppendCode(tab, "try", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        #region Select Command

                        result.AppendCode(tab, $"page ??= new PageModel<{entry.Name}>();", 2);

                        tab += 4;
                        {
                            commandBase.AppendCode(tab, $"\" FROM {entry.NameDB} A ");

                            foreach ((string Prefix, EntryModel Entry) l in linkedEntries)
                            {
                                JoinProperties = l.Entry.Properties.Where(x => x.ParentName == entry.NameDB).ToList();
                                joinParameters = JoinProperties.Select(x => $"A.{x.ParentKey} = {l.Prefix}.{x.NameDB}").ToList();

                                commandBase.AppendCode(0, "LEFT JOIN\" +", 1);
                                commandBase.AppendCode(tab, $"\" {l.Entry.NameDB} {l.Prefix} ON {(string.Join(" AND ", joinParameters))}");
                            }
                            commandBase.AppendCode(0, "\");", 2);
                        }
                        tab -= 4;

                        result.AppendCode(tab, "commandCount = new SqlCommand(\" SELECT DISTINCT COUNT(*) \" +", 1);
                        result.Append(commandBase);

                        result.AppendCode(tab, "commandWhere = new SqlCommand(\" SELECT DISTINCT A.* \" +", 1);
                        result.Append(commandBase);

                        result.AppendCode(tab, "clauses = new List<string>();", 2);

                        result.AppendCode(tab, "//Internal Columns", 1);
                        foreach (MapperProperty p in functionProperties)
                        {
                            BuildClausePagingAggregator(result, tab, p, "A");
                        }
                        result.AppendLine();

                        result.AppendCode(tab, "//Outer Columns", 1);
                        foreach ((string Prefix, EntryModel Entry) l in linkedEntries)
                        {
                            linkedEntriesProperties = l.Entry.Properties.Where(x => x.ParentName != entry.NameDB).ToList();
                            foreach (MapperProperty p in linkedEntriesProperties)
                            {
                                BuildClausePagingAggregator(result, tab, p, l.Prefix);
                            }
                        }
                        result.AppendLine();

                        #endregion

                        result.AppendCode(tab, "if (clauses.Count > 0)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            result.AppendCode(tab, "commandCount.CommandText += $\" WHERE { string.Join(\" AND \", clauses)}\";", 1);
                            result.AppendCode(tab, "commandWhere.CommandText += $\" WHERE { string.Join(\" AND \", clauses)}\";", 1);
                        }
                        tab--; //end if
                        result.AppendCode(tab, "}", 2);

                        result.AppendCode(tab, "if (page.OrderBy?.Count == 0)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            result.AppendCode(tab, $"page.OrderBy.Add((\"{mainKey.NameDB}\", true));", 1);
                        }
                        tab--; //end if
                        result.AppendCode(tab, "}", 2);

                        result.AppendCode(tab, "commandWhere.CommandText += page.ToOrderByScript(\"A\");", 1);
                        result.AppendCode(tab, "commandWhere.CommandText += page.ToFetchScript();", 2);

                        result.AppendCode(tab, "count = (int)_sqlHelper.ExecuteScalar(commandCount);", 2);

                        result.AppendCode(tab, "dataSet = _sqlHelper.ExecuteDataSet(commandWhere);", 2);

                        result.AppendCode(tab, "page.ItemsCount = count;", 1);
                        result.AppendCode(tab, "page.Items = Load(dataSet);", 1);
                    }
                    tab--; //end try
                    result.AppendCode(tab, "}", 1);

                    result.AppendCode(tab, "catch", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;

                    result.AppendCode(tab, "throw;", 1);

                    tab--; //end catch
                    result.AppendCode(tab, "}", 2);
                    result.AppendCode(tab, $"return page;", 1);
                }

                tab--;
                result.AppendCode(tab, "}", 2);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region SQL Helper

        private void BuildClauseAggregator(StringBuilder result, int tab, MapperProperty p, string prefix = "")
        {
            string varPropName = p.Name.ToCamelCase(true);

            prefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ".";

            if (p.Type == "string")
            {
                result.AppendCode(tab, $"if (!string.IsNullOrEmpty({varPropName}))", 1);
            }
            else
            {
                result.AppendCode(tab, $"if ({varPropName}.HasValue)", 1);
            }

            result.AppendCode(tab, "{", 1);
            tab++;
            {
                if (p.Type == "string")
                {
                    result.AppendCode(tab, $"clauses.Add($\"{prefix}{p.NameDB} LIKE '%' + @{p.Name} + '%'\");", 1);
                }
                else
                {
                    result.AppendCode(tab, $"clauses.Add($\"{prefix}{p.NameDB} = @{p.Name}\");", 1);
                }

                result.AppendCode(tab, $"command.Parameters.AddWithValue($\"{p.Name}\", {varPropName}.AsDbValue());", 1);
            }
            tab--; //end if
            result.AppendCode(tab, "}", 1);
        }

        private void BuildClausePagingAggregator(StringBuilder result, int tab, MapperProperty p, string prefix = "")
        {
            string varPropName = p.Name.ToCamelCase(true);

            prefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ".";

            if (p.Type == "string")
            {
                result.AppendCode(tab, $"if (!string.IsNullOrEmpty({varPropName}))", 1);
            }
            else
            {
                result.AppendCode(tab, $"if ({varPropName}.HasValue)", 1);
            }

            result.AppendCode(tab, "{", 1);
            tab++;
            {
                if (p.Type == "string")
                {
                    result.AppendCode(tab, $"clauses.Add($\"{prefix}{p.NameDB} LIKE '%' + @{p.Name} + '%'\");", 1);
                }
                else
                {
                    result.AppendCode(tab, $"clauses.Add($\"{prefix}{p.NameDB} = @{p.Name}\");", 1);
                }

                result.AppendCode(tab, $"commandCount.Parameters.AddWithValue($\"{p.Name}\", {varPropName}.AsDbValue());", 1);
                result.AppendCode(tab, $"commandWhere.Parameters.AddWithValue($\"{p.Name}\", {varPropName}.AsDbValue());", 1);
            }
            tab--; //end if
            result.AppendCode(tab, "}", 1);
        }

        #endregion
    }
}

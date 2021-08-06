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
				result.AppendCode(tab, "using RauchTech.DataExtensions.Sql;", 1);
				result.AppendCode(tab, $"using {projectName}.Domain.Model;", 2);

				result.AppendCode(tab, $"namespace {projectName}.Domain.Repository", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, $"public class {entry.Name}Repository", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				BuildConstructor(result, tab, entry);
				result.AppendLine();

				result.AppendCode(tab, "#region LoadModel", 2);

				BuildModelLoader(result, tab, entry);
				result.AppendLine();

				result.AppendCode(tab, "#endregion", 2);

				result.AppendCode(tab, "#region Change Data", 2);

				BuildInserter(result, tab, entry);

				result.AppendCode(tab, "#endregion", 2);

				tab--;
				result.AppendCode(tab, "}", 2);

				tab--;
				result.AppendCode(tab, "}", 1);
			}
			catch
			{
				throw;
			}

			return result.ToString();
		}

		private void BuildConstructor(StringBuilder result, int tab, EntryModel entry)
		{
			result.AppendCode(tab, "private readonly IConfiguration _config;", 1);
			result.AppendCode(tab, "private readonly ISqlHelper _dataConnection;", 2);

			result.AppendCode(tab, $"public {entry.Name}Repository(IConfiguration configuration, ISqlHelper sqlHelper)", 1);
			result.AppendCode(tab, "{", 1);
			tab++;

			result.AppendCode(tab, "_config = configuration;", 1);
			result.AppendCode(tab, "_dataConnection = sqlHelper;", 1);

			tab--;
			result.AppendCode(tab, "}", 1);
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

				result.AppendCode(tab, $"List<{entry.Name}> {varModelName}s;", 1);
				result.AppendCode(tab, $"{entry.Name} {varModelName};", 2);

				result.AppendCode(tab, "try", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, $"{varModelName}s = new List<{entry.Name}>();", 2);

				result.AppendCode(tab, $"foreach (DataRow row in data.Tables[0].Rows)", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, $"{varModelName} = new {entry.Name}();", 2);

				foreach (MapperProperty p in entry.Properties)
				{
					property = p;
					required = string.Empty;

					if (p.IsRequired && p.Type.ToLower() != "string")
					{
						required = "?";
					}

					result.AppendCode(tab, $"{varModelName}.{p.Name} = row.Field<{(p.Type + required)}>(\"{p.NameDB}\");", 1);
				}

				result.AppendLine();
				result.AppendCode(tab, $"{varModelName}s.Add({varModelName});", 1);

				tab--; //end foreach
				result.AppendCode(tab, "}", 1);

				tab--; //end try
				result.AppendCode(tab, "}", 1);

				result.AppendCode(tab, "catch", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, "throw;", 1);

				tab--; //end catch
				result.AppendCode(tab, "}", 1);

				tab--;
				result.AppendCode(tab, "}", 1);
			}
			catch
			{
				throw;
			}
		}

		private void BuildInserter(StringBuilder result, int tab, EntryModel entry)
		{
			string varModelName;

			MapperProperty mainKey;
			MapperProperty property;

			try
			{
				varModelName = entry.Name.ToCamelCase(true);
				mainKey = entry.Properties.FirstOrDefault(x => x.IsKey) ?? new MapperProperty { NameDB = "????", Type = "????" };

				result.AppendCode(tab, $"public {entry.Name} Insert({entry.Name} {varModelName})", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, $"SqlCommand command;", 2);

				result.AppendCode(tab, "try", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				#region Insert Command

				result.AppendCode(tab, $"command = new SqlCommand(\" INSERT INTO {entry.NameDB} \" +", 1);
				tab += 3;

				result.AppendCode(tab, "\" (\" +", 1);
				tab++;

				foreach (MapperProperty p in entry.Properties)
				{
					property = p;

					result.AppendCode(tab, $"\" {(!entry.Properties.First().Equals(p) ? "," : " ")}");

					result.AppendLine($"{p.NameDB}\" +");
				}

				tab--;
				result.AppendCode(tab, "\" )\" +", 1);
				result.AppendCode(tab, $"\" OUTPUT inserted.{mainKey.NameDB} \" +", 1);
				result.AppendCode(tab, $"\" VALUES \" +", 1);
				result.AppendCode(tab, "\" (\" +", 1);
				tab++;

				foreach (MapperProperty p in entry.Properties)
				{
					property = p;

					result.AppendCode(tab, $"\" {(!entry.Properties.First().Equals(p) ? "," : " ")}");

					result.AppendLine($"@{p.Name}\" +");
				}

				tab--; //end insert
				result.AppendCode(tab, "\" )\");", 1);
				tab -= 3;

				#endregion

				foreach (MapperProperty p in entry.Properties)
				{
					property = p;

					result.AppendCode(tab, $"command.Parameters.AddWithValue(\"{p.Name}\", {varModelName}.{p.Name}.AsDbValue());", 1);
				}
				result.AppendLine();

				result.AppendCode(tab, $"{varModelName}.{mainKey.NameDB} = ({mainKey.Type})_dataConnection.ExecuteScalar(command);", 1);

				tab--; //end try
				result.AppendCode(tab, "}", 1);

				result.AppendCode(tab, "catch", 1);
				result.AppendCode(tab, "{", 1);
				tab++;

				result.AppendCode(tab, "throw;", 1);

				tab--; //end catch
				result.AppendCode(tab, "}", 1);

				tab--;
				result.AppendCode(tab, "}", 1);
			}
			catch
			{
				throw;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Services.Generator
{
	public class EntityGeneratorService
	{
		public List<string> ParseFromGenerator(GeneratorModel model)
		{
			List<string> result = new List<string>();

			foreach (EntryModel e in model.EntryModels)
			{
				result.Add(ParseFromEntry(model.ProjectName, e));
			}

			return result;
		}

		public string ParseFromEntry(string projectName, EntryModel entry)
		{
			StringBuilder result;
			int tab;

			string tableName;
			string[] keys;
			string[] indexers;
			string length;

			try
			{
				result = new StringBuilder();
				tab = 0;

				#region Usings & NameSpace

				result.AppendCode(tab, "using Microsoft.EntityFrameworkCore;", 1);
				result.AppendCode(tab, "using System;", 2);

				result.AppendCode(tab, $"namespace {projectName}.Domain.Model.Builder", 1);
				result.AppendCode(tab, "{", 1);

				#endregion

				tab++;

				result.AppendCode(tab, $"public static class {entry.Name}ModelBuilder", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				result.AppendCode(tab, $"public static ModelBuilder Build(ModelBuilder modelBuilder)", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				result.AppendCode(tab, $"return modelBuilder is null", 1);
				result.AppendCode(tab, $"? throw new ArgumentNullException(nameof(modelBuilder))", 1);
				result.AppendCode(tab, $": modelBuilder.Entity<{entry.Name}>(entity =>", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				#region Name

				tableName = entry.NameDB ?? entry.Name;

				result.AppendCode(tab, $"_ = entity.ToTable(\"{tableName}\");", 2);

				#endregion

				#region Keys

				keys = entry.Properties.Where(x => x.IsKey).Select(x => "e." + x.Name).ToArray();

				result.AppendCode(tab, $"_ = entity.HasKey(e => new {{ {string.Join(", ", keys)} }});", 2);

				#endregion

				#region Index

				var indexGroupsSelections = entry.Properties.Where(x => x.IndexGroup != null).SelectMany(x => x.IndexGroup).Distinct();

				foreach (string indexGroup in indexGroupsSelections)
				{
					indexers = entry.Properties.Where(x => x.IsIndex && x.IndexGroup.Contains(indexGroup)).Select(x => x.Name).ToArray();

					result.AppendCode(tab, $"_ = entity.HasIndex(e => new {{ {string.Join(", ", indexers)} }}, \"{indexGroup}\" );", 2);
				}

				#endregion

				#region Parameters

				foreach (MapperProperty p in entry.Properties)
				{
					result.AppendCode(tab, $"_ = entity.Property(e => e.{p.Name})", 1);

					tab++;

					if (p.TypeDB.Contains("char"))
					{
						result.AppendCode(tab, $".IsUnicode(false)", 1);
						if (p.TypeDB == "char")
						{
							result.AppendCode(tab, $".IsFixedLength(true)", 1);
						}

						if (p.LengthMain.HasValue)
						{
							result.AppendCode(tab, $".HasMaxLength({p.LengthMain.Value})", 1);
						}
					}
					else
					{
						if (p.LengthMain.HasValue)
						{
							length = $"({p.LengthMain}{(p.LengthDecimal.HasValue ? "," + p.LengthDecimal : "")})";
						}
						else
						{
							length = string.Empty;
						}

						result.AppendCode(tab, $".HasColumnType(\"{p.TypeDB}{length}\")", 1);
					}

					if (p.IsRequired)
					{
						result.AppendCode(tab, $".IsRequired(true)", 1);
					}

					result.AppendCode(tab, $".HasColumnName(\"{p.NameDB}\");", 2);

					tab--;
				}

				#endregion

				result.AppendCode(tab, "});", 1);
				tab--;
				result.AppendCode(tab, "}", 1);
				tab--;
				result.AppendCode(tab, "}", 1);
				tab--;
				result.AppendCode(tab, "}", 1);
			}
			catch
			{
				throw;
			}

			return result.ToString();
		}
	}
}


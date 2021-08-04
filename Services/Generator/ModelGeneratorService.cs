using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Services.Generator
{
	public class ModelGeneratorService
	{
		public const string MessageRequired = "Valor preencher o campo";
		public const string MessageMaxLength = "Tamanho máximo excedido";
		public const string MessageSpecificLength = "Tamanho específico não respeitado";

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

			EntryModel chield;

			try
			{
				result = new StringBuilder();
				tab = 0;

				#region Usings & NameSpace

				result.AppendCode(tab, "using FluentValidation;", 1);
				result.AppendCode(tab, "using System.Collections.Generic;", 1);
				result.AppendCode(tab, "using System.ComponentModel.DataAnnotations;", 1);
				result.AppendCode(tab, "using System.Text.Json.Serialization;", 2);

				result.AppendCode(tab, $"namespace {projectName}.Domain.Model", 1);
				result.AppendCode(tab, "{", 1);

				#endregion

				tab++;

				result.AppendCode(tab, $"public class {entry.Name}", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				#region Parameters

				foreach (MapperProperty p in entry.Properties)
				{
					result.AppendCode(tab, $"public {p.Type} {p.Name} {{ get; set; }}", 1);
				}

				#endregion

				#region Relations

				if (entry.Relationships.Any())
				{
					result.AppendLine();
				}

				foreach (EntryRelationship r in entry.Relationships)
				{
					chield = model.EntryModels.Find(x => x.Name == r.TargetName);

					if (chield != null)
					{
						if (chield.Properties.All(x => x.IsKey))
						{
							foreach (MapperProperty c in chield.Properties.Where(x => x.ParentName != entry.Name))
							{
								result.AppendCode(tab, $"public List<{c.ParentName}> {c.ParentName} {{ get; set; }}", 1);
							}

							result.AppendCode(tab, $"public List<{chield.Name}> {chield.Name}s {{ get; set; }}", 1);
						}
						else
						{
							switch (r.Type)
							{
								case RelationshipType.IN_1_OUT_1:
									{
										result.AppendCode(tab, $"public {chield.Name} {chield.Name} {{ get; set; }}", 1);
									}
									break;
								case RelationshipType.IN_1_OUT_N:
									{
										result.AppendCode(tab, $"public List<{chield.Name}> {chield.Name}s {{ get; set; }}", 1);
									}
									break;
							}
						}
					}
				}

				#endregion

				tab--;
				result.AppendCode(tab, "}", 2);

				#region Validation

				result.AppendCode(tab, $"public class Validator{entry.Name} : AbstractValidator<{entry.Name}>", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				result.AppendCode(tab, "public ValidatorEntryModel()", 1);
				result.AppendCode(tab, "{", 1);

				tab++;

				foreach (MapperProperty p in entry.Properties)
				{
					if (p.IsRequired)
					{
						result.AppendCode(tab, $"RuleFor(x => x.{p.Name}).NotEmpty().WithMessage(\"{MessageRequired}\");", 1);
					}

					if (p.LengthMain > 0)
					{
						if (p.IsFixedLength)
						{
							result.AppendCode(tab, $"RuleFor(x => x.{p.Name}).Must(x=>x.Length == {p.LengthMain.Value}).WithMessage(\"{MessageSpecificLength}: {p.LengthMain.Value}\");", 1);
						}
						else
						{
							int maxLength = p.LengthMain.Value + (p.LengthDecimal.HasValue ? (p.LengthDecimal.Value + 1) : 0);
							result.AppendCode(tab, $"RuleFor(x => x.{p.Name}).Must(x=>x.Length <= {maxLength}).WithMessage(\"{MessageMaxLength}: {maxLength}\");", 1);
						}
					}
				}

				tab--;
				result.AppendCode(tab, "}", 1);

				tab--;
				result.AppendCode(tab, "}", 1);

				#endregion

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkUtilities.Domain.Models;
using WorkUtilities.Domain.Services.Package;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Domain.Services.Generator
{
    public class ModelGeneratorService
    {
        public const string MessageRequired = "Favor preencher o campo";
        public const string MessageMaxLength = "Tamanho máximo excedido";
        public const string MessageSpecificLength = "Tamanho específico não respeitado";

        private readonly FilePackagerService _filePackagerService;

        public ModelGeneratorService(FilePackagerService filePackagerService)
        {
            _filePackagerService = filePackagerService;
        }

        public List<InMemoryFile> ParseFromGenerator(GeneratorModel model)
        {
            List<InMemoryFile> result = new List<InMemoryFile>();

            foreach (EntryModel e in model.EntryModels)
            {
                e.PreProcess();
                result.Add(_filePackagerService.BuildFile("", $"{e.Name}", ".cs", ParseFromEntry(model.ProjectName, model, e)));
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

                result.AppendCode(tab, "using FluentValidation;", 1);
                result.AppendCode(tab, "using System.Collections.Generic;", 1);
                result.AppendCode(tab, "using System.ComponentModel.DataAnnotations;", 1);
                result.AppendCode(tab, "using System.Text.Json.Serialization;", 2);

                result.AppendCode(tab, $"namespace {projectName}.Domain.Model", 1);
                result.AppendCode(tab, "{", 1);
                tab++;

                result.AppendCode(tab, $"public class {entry.Name}", 1);
                result.AppendCode(tab, "{", 1);
                tab++;

                BuildProperties(result, tab, entry);

                BuildRelations(result, tab, model, entry);

                tab--;
                result.AppendCode(tab, "}", 2);

                BuildValidator(result, tab, entry);

                tab--;
                result.AppendCode(tab, "}", 1);
            }
            catch
            {
                throw;
            }

            return result.ToString();
        }

        private static void BuildProperties(StringBuilder result, int tab, EntryModel entry)
        {
            foreach (MapperProperty p in entry.Properties)
            {
                result.AppendCode(tab, $"public {p.Type} {p.Name} {{ get; set; }} // {p.NameDB}", 1);
            }
        }

        private static void BuildRelations(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            EntryModel chield;

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
        }

        private static void BuildValidator(StringBuilder result, int tab, EntryModel entry)
        {
            result.AppendCode(tab, $"public class Validator{entry.Name} : AbstractValidator<{entry.Name}>", 1);
            result.AppendCode(tab, "{", 1);

            tab++;

            result.AppendCode(tab, $"public Validator{entry.Name}()", 1);
            result.AppendCode(tab, "{", 1);

            tab++;

            foreach (MapperProperty p in entry.Properties.Where(x => !x.IsKey))
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
        }
    }
}

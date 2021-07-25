using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkUtilities.Models
{
    /// <summary>
    /// Representação de Tabela ou Model
    /// </summary>
    public sealed class EntryModel
    {
        /// <summary>
        /// Nome da Model
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Nome da Tabela
        /// </summary>
        public string NameDB { get; set; }

        /// <summary>
        /// Propriedades contendo suas características
        /// </summary>
        public List<MapperProperty> Properties { get; set; }
        /// <summary>
        /// Relacionamentos filhos ou paralelos a esta tabela 
        /// </summary>
        public List<EntryRelationship> Relationships { get; set; }
    }

    public class EntryRelationship
    {
        public string TargetName { get; set; }        
        public RelationshipType Type { get; set; }
    }

    /// <summary>
    /// Tipo de relacionamentos
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RelationshipType
    {
        /// <summary>
        /// Relacionamento 1 para 1 (O filho contém a PK sendo uma FK para esta tabela)
        /// </summary>
        IN_1_OUT_1,
        /// <summary>
        /// Relacionamento 1 para N (O filho contém uma FK para esta tabela)
        /// </summary>
        IN_1_OUT_N,
        /// <summary>
        /// Relacionamento N para N (Será gerada uma tabela intermediária e esta fará a ponte entre as duas tabelas)
        /// </summary>
        IN_N_OUT_N
    }

    public static class ProcessorEntryModel
    {
        public static void PreProcess(this EntryModel model)
        {
            model.NameDB ??= model.Name;

            foreach(MapperProperty p in model.Properties)
			{
                p.NameDB ??= p.Name;
            }
        }
    }

    public class ValidatorEntryModel : AbstractValidator<EntryModel>
    {
        public ValidatorEntryModel()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Please specify a name");
            RuleFor(x => x.Properties).Must(x => x.Count > 0).WithMessage("Please fill any Property");
            //RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");
        }
    }
}

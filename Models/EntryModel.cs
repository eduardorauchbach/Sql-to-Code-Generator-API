using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkUtilities.Models
{
    public sealed class EntryModel
    {
        public string Name { get; set; }
        public string NameDB { get; set; }
        public List<MapperProperty> Properties { get; set; }
        public List<EntryRelationship> Relationships { get; set; }
    }    

    public class EntryRelationship
    {
        public string TargetName { get; set; }
        public RelationshipType Type { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RelationshipType
    {
        IN_1_OUT_1,
        IN_1_OUT_N,
        IN_N_OUT_N
    }

    public class EntryModelValidator : AbstractValidator<EntryModel>
    {
        public EntryModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Please specify a name");
            RuleFor(x => x.Properties).Must(x => x.Count > 0).WithMessage("Please fill any Property");
            //RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");
        }
    }
}

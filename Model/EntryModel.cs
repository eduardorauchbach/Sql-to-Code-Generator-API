using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkUtilities.Model
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

            //RuleFor(x => x.Surname).NotEmpty();
            //RuleFor(x => x.Forename).NotEmpty().WithMessage("Please specify a first name");
            //RuleFor(x => x.Discount).NotEqual(0).When(x => x.HasDiscount);
            //RuleFor(x => x.Address).Length(20, 250);
            //RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");
        }

        //private bool BeAValidPostcode(string postcode)
        //{
        //    // custom postcode validating logic goes here
        //}
    }
}

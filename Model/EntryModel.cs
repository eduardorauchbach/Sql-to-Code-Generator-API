using System;
using System.Collections.Generic;
using System.Linq;
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

    public enum RelationshipType
    {
        IN_1_OUT_1,
        IN_1_OUT_N,
        IN_N_OUT_N
    }
}

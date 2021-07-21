using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkUtilities.Model
{
    public class MapperProperty
    {
        public bool IsKey { get; set; } = false;
        public bool IsIndex { get; set; } = false;

        public string ParentName { get; set; }
        public string ParentKey { get; set; }

        public string Name { get; set; }
        public string NameDB { get; set; }

        public int? LengthMain { get; set; }
        public int? LengthDecimal { get; set; }
        public bool IsFixedLength { get; set; }
        public bool IsRequired { get; set; }

        public string Type { get; set; }
        public string TypeDB { get; set; }
    }
}

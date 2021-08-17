using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkUtilities.Domain.Models
{
    public class InMemoryFile
    {
        public string Basepath { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string ContentText { get; set; }
        public byte[] ContentData { get; set; }
    }
}

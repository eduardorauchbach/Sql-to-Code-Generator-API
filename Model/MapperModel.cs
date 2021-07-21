﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkUtilities.Model
{
    public class MapperModel
    {
        public string Name { get; set; }
        public List<MapperProperty> Keys { get; set; }
        public List<MapperProperty> Indexers { get; set; }
        public List<MapperProperty> Default { get; set; }

        public List<string> SingleLinks { get; set; }
        public List<string> MultipleLinks { get; set; }
        public List<string> Selectors { get; set; }
    }
}

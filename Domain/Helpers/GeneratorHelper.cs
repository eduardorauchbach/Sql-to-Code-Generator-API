using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Models;

namespace WorkUtilities.Helpers
{
    public static class GeneratorHelper
    {
        public static List<EntryModel> GetDependents(this GeneratorModel model, EntryModel entry)
        {
            List<EntryModel> entryModels;

            EntryModel currentEntry;
            EntryRelationship relationship;

            try
            {
                entryModels = new List<EntryModel>();

                foreach (EntryRelationship r in entry.Relationships)
                {
                    relationship = r;

                    currentEntry = model.EntryModels.Find(x => x.Name == r.TargetName);

                    if (currentEntry != null)
                    {
                        entryModels.Add(currentEntry);
                    }
                }
            }
            catch
            {
                throw;
            }

            return entryModels;
        }

        public static List<EntryModel> GetDependents(this EntryModel entry, GeneratorModel model)
        {
            List<EntryModel> entryModels;

            EntryModel currentEntry;
            EntryRelationship relationship;

            try
            {
                entryModels = new List<EntryModel>();

                foreach (EntryRelationship r in entry.Relationships)
                {
                    relationship = r;

                    currentEntry = model.EntryModels.Find(x => x.Name == r.TargetName);

                    if (currentEntry != null)
                    {
                        entryModels.Add(currentEntry);
                    }
                }
            }
            catch
            {
                throw;
            }

            return entryModels;
        }


        public static List<string> GetFunctionParameters(this IEnumerable<MapperProperty> mapperProperties)
        {
            return mapperProperties.ToList().GetFunctionParameters();
        }
        public static List<string> GetFunctionParameters(this List<MapperProperty> mapperProperties)
        {
            return mapperProperties.Select(x => $"{x.Type}{(x.Type == "string" ? "":"?")} {x.Name.ToCamelCase(true)} = null").ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WorkUtilities.Domain.Models;
using WorkUtilities.Domain.Services.Package;
using WorkUtilities.Helpers;
using WorkUtilities.Models;

namespace WorkUtilities.Domain.Services.Generator
{
    public class DiagramGeneratorService
    {
        private const string CheckNo = "&#x2610;";
        private const string CheckYes = "&check;";

        private StringBuilder diagram = new StringBuilder();
        private StringBuilder entityList = new StringBuilder();

        public DiagramGeneratorService()
        {
        }

        public string ParseFromGenerator(GeneratorModel model)
        {
            var multigraph = TopologicalSort(model.EntryModels, 6);

            foreach (var graphInfo in multigraph)
            {
                diagram.AppendLine("```mermaid");
                //diagram.AppendLine("classDiagram");
                diagram.AppendLine("graph TB");
                diagram.AppendLine("%%{ init: { 'theme': 'neutral', 'flowchart': { 'useMaxWidth': false, 'curve': 'basis' }}}%%");

                foreach (EntryModel e in graphInfo)
                {
                    e.PreProcess();
                    ParseFromEntry(diagram, model, e);
                }

                foreach (EntryModel e in graphInfo)
                {
                    BuildRelations(diagram, 1, model, e);
                }

                diagram.AppendLine("```");
                diagram.AppendLine();
            }
            diagram.AppendLine("<br/><br/><br/>");
            diagram.Append(entityList);

            return diagram.ToString();


        }

        private List<List<EntryModel>> TopologicalSort(List<EntryModel> entries, int maxLimit)
        {
            var sortedEntries = new List<EntryModel>();
            var visited = new HashSet<string>();
            var groups = new List<List<EntryModel>>();
            var group = new List<EntryModel>();

            // Helper function to perform topological sort
            void DFS(EntryModel entry)
            {
                visited.Add(entry.Name);
                foreach (var child in entries.Where(x => x.Relationships.Any(y => y.TargetName == entry.Name)))
                {
                    DFS(child);
                }
                sortedEntries.Add(entry);
            }

            // Perform topological sort
            foreach (var entry in entries)
            {
                if (!visited.Contains(entry.Name))
                {
                    DFS(entry);
                }
            }

            // Iterate through sorted entries and form groups
            for (int i = sortedEntries.Count - 1; i >= 0; i--)
            {
                var current = sortedEntries[i];

                // Check if current entry has a parent in current group
                if (group.Any(x => x.Relationships.Any(y => y.TargetName == current.Name)))
                {
                    group.Add(current);
                }
                else
                {
                    // Check if current entry has a child in any previous groups
                    var hasChildInPreviousGroup = groups.Any(g => g.Any(x => x.Relationships.Any(y => y.TargetName == current.Name)));

                    if (hasChildInPreviousGroup)
                    {
                        if (!group.Any(x => x.NameDB == current.NameDB))
                        {
                            group.Add(current);
                        }
                    }
                    else
                    {
                        // Check if group has reached max limit
                        if (group.Count >= maxLimit)
                        {
                            // Add current entry to new group
                            group = new List<EntryModel> { current };
                            groups.Add(group);
                        }
                        else
                        {
                            if (!group.Any(x => x.NameDB == current.NameDB))
                            {
                                group.Add(current);
                            }
                        }
                    }
                }
            }

            // Add final group to list of groups
            groups.Add(group);
            MergeListsWithRecurringEntries(groups);

            return SplitClear(groups).Where(x => x.Any()).ToList();

            void MergeListsWithRecurringEntries(List<List<EntryModel>> listOfLists)
            {
                bool executed = false;
                for (int i = 0; i < listOfLists.Count; i++)
                {
                    for (int j = i + 1; j < listOfLists.Count; j++)
                    {
                        var recurringEntries = listOfLists[i].Intersect(listOfLists[j]).ToList();
                        if (recurringEntries.Any())
                        {
                            executed = true;

                            listOfLists[i].AddRange(listOfLists[j]);
                            listOfLists[i] = listOfLists[i].Distinct().ToList();
                            listOfLists.RemoveAt(j);
                            j--;
                        }
                    }
                }
                if (executed)
                {
                    listOfLists.ForEach(x =>
                    {
                        x = x.DistinctBy(x => x.NameDB).ToList();
                        x.ForEach(y =>
                        {
                            y.Relationships = y.Relationships.DistinctBy(r => r.TargetName).ToList();
                        });
                    });
                    MergeListsWithRecurringEntries(listOfLists);
                }
            }

            List<List<EntryModel>> SplitClear(List<List<EntryModel>> listOfLists)
            {
                var listAgg = new List<List<EntryModel>>();
                var agg = new List<EntryModel>();

                for (int i = listOfLists.Count - 1; i >= 0; i--)
                {
                    var exiting = listOfLists[i].Where(x => (x.Relationships?.Count == 0) && !listOfLists[i].Any(y => y.Relationships.Any(z => z.TargetName == x.Name)));
                    if (exiting.Any())
                    {
                        foreach (var item in exiting)
                        {
                            if (agg.Count <= maxLimit)
                            {
                                if (!agg.Any(x => x.NameDB == item.NameDB))
                                {
                                    agg.Add(item);
                                }
                            }
                            else
                            {
                                listAgg.Add(agg);
                                agg = new List<EntryModel>();
                            }
                        }
                        listOfLists[i].RemoveAll(x => exiting.Any(y => y.Name == x.Name));
                    }
                }
                if (agg.Any())
                {
                    listAgg.Add(agg);
                }

                return listOfLists.Concat(listAgg).ToList();
            }
        }

        public void ParseFromEntry(StringBuilder builder, GeneratorModel model, EntryModel entry)
        {
            int tab;

            try
            {
                tab = 1;

                //diagram.AppendCode(tab, "class " + entry.NameDB + "{", 1);
                diagram.AppendCode(tab, $"{entry.NameDB}[\"{entry.NameDB}", 0);
                tab++;

                entityList.AppendCode(0, $"## {entry.NameDB}", 2);
                entityList.AppendLine($"|**Name**|**Type**|**Key**|**Auto**|**Parent**|**Description**|");
                entityList.AppendLine($"|-|-|-|-|-|-|");

                BuildProperties(diagram, tab, entry);

                entityList.AppendLine();

                tab--;
                //diagram.AppendCode(tab, "}", 2);
                diagram.AppendCode(0, "\"]", 2);
            }
            catch
            {
                throw;
            }
        }

        private void BuildProperties(StringBuilder result, int tab, EntryModel entry)
        {
            foreach (MapperProperty p in entry.Properties)
            {
                var parent = !string.IsNullOrEmpty(p.ParentName) ? $"[{p.ParentName}](#{p.ParentName})" : "";
                var key = p.IsKey ? CheckYes : CheckNo;
                var auto = p.IsAutoGenerated ? CheckYes : CheckNo;

                //result.AppendCode(tab, $"+{p.TypeDB} {p.NameDB}", 1);
                //result.AppendCode(tab, $"<br/>{p.TypeDB} - {p.NameDB}", 1);
                entityList.AppendLine($"|{p.NameDB}|{p.TypeDB}|{key}|{auto}|{parent}||");
            }
        }

        private static void BuildRelations(StringBuilder result, int tab, GeneratorModel model, EntryModel entry)
        {
            foreach (EntryRelationship r in entry.Relationships)
            {
                var tableName = model.EntryModels.First(x => x.Name == r.TargetName)?.NameDB;

                switch (r.Type)
                {
                    case RelationshipType.IN_1_OUT_1:
                        {
                            //result.AppendCode(tab, $"{tableName} -- {entry.NameDB}", 1);                            
                        }
                        break;
                    case RelationshipType.IN_1_OUT_N:
                        {
                            //result.AppendCode(tab, $"{tableName} <|-- {entry.NameDB}", 1);
                            result.AppendCode(tab, $"{entry.NameDB} --> {tableName}", 1);
                        }
                        break;
                    case RelationshipType.IN_N_OUT_N:
                        {
                            //result.AppendCode(tab, $"{tableName} <|--|> {entry.NameDB}", 1);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

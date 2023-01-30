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
            var sorted = SortParents(model.EntryModels);
            var entryColletions = SplitRelationGroups(sorted, 6);

            foreach (var entries in entryColletions)
            {
                diagram.AppendLine("```mermaid");
                //diagram.AppendLine("classDiagram");
                diagram.AppendLine("graph TB");
                diagram.AppendLine("%%{ init: { 'theme': 'neutral', 'flowchart': { 'useMaxWidth': false, 'curve': 'basis' }}}%%");

                foreach (EntryModel e in entries)
                {
                    e.PreProcess();
                    ParseFromEntry(diagram, model, e);
                }

                foreach (EntryModel e in entries)
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

        private List<EntryModel> SortParents(List<EntryModel> original)
        {
            List<(EntryModel Entry, string Name, string[] Relations)> newList = new();

            foreach (var o in original)
            {
                newList.Add((o, o.Name, o.Relationships.Select(x => x.TargetName).ToArray()));
            }

            var executed = false;
            do
            {
                executed = false;

                for (int i = newList.Count - 1; i >= 0; i--)
                {
                    for (int j = newList.Count - 1; j > i; j--)
                    {
                        if (newList[i].Relations.Contains(newList[j].Name))
                        {
                            executed = true;

                            var temp = newList[i];
                            newList[i] = newList[j];
                            newList[j] = temp;
                        }
                    }
                }

            } while (executed);

            return newList.Select(x => x.Entry).ToList();
        }

        private IEnumerable<IEnumerable<EntryModel>> SplitRelationGroups(IEnumerable<EntryModel> original, int? defaultLimit = null)
        {
            List<(EntryModel Entry, int? group, string[] Relations)> newList = new();

            foreach (var o in original)
            {
                newList.Add((o, null, o.Relationships.Select(x => x.TargetName).ToArray()));
            }

            var groupIndex = 0;
            var executed = false;
            do
            {
                executed = false;

                for (int i = newList.Count - 1; i >= 0; i--)
                {
                    for (int j = newList.Count - 1; j >= 0; j--)
                    {
                        if (i != j)
                        {
                            if (newList[i].Relations.Contains(newList[j].Entry.Name))
                            {
                                if (newList[i].group != newList[j].group || (newList[i].group is null))
                                {
                                    executed = true;

                                    var currentGroup = 0;
                                    if (newList[i].group is null && newList[j].group is null)
                                    {
                                        groupIndex++;
                                        currentGroup = groupIndex;
                                    }
                                    else if (newList[i].group.HasValue && newList[j].group.HasValue)
                                    {
                                        currentGroup = newList[i].group < newList[j].group ? newList[i].group.Value : newList[j].group.Value;
                                    }
                                    else
                                    {
                                        currentGroup = newList[i].group ?? newList[j].group.Value;
                                    }
                                    newList[i] = (newList[i].Entry, currentGroup, newList[i].Relations);
                                    newList[j] = (newList[j].Entry, currentGroup, newList[j].Relations);
                                }
                            }
                        }
                    }
                }

            } while (executed);

            var result = newList.GroupBy(x => x.group).Select(x => x.Select(y => y.Entry).ToList()).ToList();

            if (defaultLimit.HasValue)
            {
                do
                {
                    executed = false;

                    for (int i = result.Count - 1; i >= 0; i--)
                    {
                        var iHasRelation = result[i].Any(x => x.Relationships.Any());

                        for (int j = result.Count - 1; j >= 0; j--)
                        {
                            var jHasRelation = result[j].Any(x => x.Relationships.Any());

                            if (i != j)
                            {
                                if (iHasRelation && jHasRelation)
                                {
                                    if ((result[i].Count + result[j].Count) <= defaultLimit)
                                    {
                                        executed = true;
                                        result[i] = result[i].Concat(result[j]).ToList();
                                        result.RemoveAt(j);
                                        break;
                                    }
                                }
                            }
                            else if (!iHasRelation && !jHasRelation)
                            {
                                if (result[i].Count > defaultLimit)
                                {
                                    executed = true;
                                    foreach (var chunk in result[i].Chunk(defaultLimit.Value))
                                    {
                                        result.Add(chunk.ToList());
                                    }
                                    result.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                } while (executed);
            }

            return result.Where(x => x.Any()).OrderByDescending(x=>x.Sum(y=>y.Relationships.Count()));
        }

        //private List<List<EntryModel>> TopologicalSort(List<EntryModel> entries, int maxLimit)
        //{
        //    var sortedEntries = new List<EntryModel>();
        //    var visited = new HashSet<string>();
        //    var groups = new List<List<EntryModel>>();
        //    var group = new List<EntryModel>();

        //    // Helper function to perform topological sort
        //    void DFS(EntryModel entry)
        //    {
        //        visited.Add(entry.Name);
        //        foreach (var child in entries.Where(x => x.Relationships.Any(y => y.TargetName == entry.Name)))
        //        {
        //            DFS(child);
        //        }
        //        sortedEntries.Add(entry);
        //    }

        //    // Perform topological sort
        //    foreach (var entry in entries)
        //    {
        //        if (!visited.Contains(entry.Name))
        //        {
        //            DFS(entry);
        //        }
        //    }

        //    // Iterate through sorted entries and form groups
        //    for (int i = sortedEntries.Count - 1; i >= 0; i--)
        //    {
        //        var current = sortedEntries[i];

        //        // Check if current entry has a parent in current group
        //        if (group.Any(x => x.Relationships.Any(y => y.TargetName == current.Name)))
        //        {
        //            group.Add(current);
        //        }
        //        else
        //        {
        //            // Check if current entry has a child in any previous groups
        //            var hasChildInPreviousGroup = groups.Any(g => g.Any(x => x.Relationships.Any(y => y.TargetName == current.Name)));

        //            if (hasChildInPreviousGroup)
        //            {
        //                if (!group.Any(x => x.NameDB == current.NameDB))
        //                {
        //                    group.Add(current);
        //                }
        //            }
        //            else
        //            {
        //                // Check if group has reached max limit
        //                if (group.Count >= maxLimit)
        //                {
        //                    // Add current entry to new group
        //                    group = new List<EntryModel> { current };
        //                    groups.Add(group);
        //                }
        //                else
        //                {
        //                    if (!group.Any(x => x.NameDB == current.NameDB))
        //                    {
        //                        group.Add(current);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Add final group to list of groups
        //    groups.Add(group);
        //    MergeListsWithRecurringEntries(groups);

        //    return SplitClear(groups).Where(x => x.Any()).ToList();

        //    void MergeListsWithRecurringEntries(List<List<EntryModel>> listOfLists)
        //    {
        //        bool executed = false;
        //        for (int i = 0; i < listOfLists.Count; i++)
        //        {
        //            for (int j = i + 1; j < listOfLists.Count; j++)
        //            {
        //                var recurringEntries = listOfLists[i].Intersect(listOfLists[j]).ToList();
        //                if (recurringEntries.Any())
        //                {
        //                    executed = true;

        //                    listOfLists[i].AddRange(listOfLists[j]);
        //                    listOfLists[i] = listOfLists[i].Distinct().ToList();
        //                    listOfLists.RemoveAt(j);
        //                    j--;
        //                }
        //            }
        //        }
        //        if (executed)
        //        {
        //            listOfLists.ForEach(x =>
        //            {
        //                x = x.DistinctBy(x => x.NameDB).ToList();
        //                x.ForEach(y =>
        //                {
        //                    y.Relationships = y.Relationships.DistinctBy(r => r.TargetName).ToList();
        //                });
        //            });
        //            MergeListsWithRecurringEntries(listOfLists);
        //        }
        //    }

        //    List<List<EntryModel>> SplitClear(List<List<EntryModel>> listOfLists)
        //    {
        //        var listAgg = new List<List<EntryModel>>();
        //        var agg = new List<EntryModel>();

        //        for (int i = listOfLists.Count - 1; i >= 0; i--)
        //        {
        //            var exiting = listOfLists[i].Where(x => (x.Relationships?.Count == 0) && !listOfLists[i].Any(y => y.Relationships.Any(z => z.TargetName == x.Name)));
        //            if (exiting.Any())
        //            {
        //                foreach (var item in exiting)
        //                {
        //                    if (agg.Count <= maxLimit)
        //                    {
        //                        if (!agg.Any(x => x.NameDB == item.NameDB))
        //                        {
        //                            agg.Add(item);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        listAgg.Add(agg);
        //                        agg = new List<EntryModel>();
        //                    }
        //                }
        //                listOfLists[i].RemoveAll(x => exiting.Any(y => y.Name == x.Name));
        //            }
        //        }
        //        if (agg.Any())
        //        {
        //            listAgg.Add(agg);
        //        }

        //        return listOfLists.Concat(listAgg).ToList();
        //    }
        //}

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

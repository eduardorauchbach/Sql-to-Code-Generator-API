using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkUtilities.Services
{
    public class SqlToEntityService
    {
        public string Parse(string script)
        {
            StringBuilder result = new StringBuilder();
            Match matchHeader;
            MatchCollection matchesHeaders;
            MatchCollection matchesItems;

            #region Name

            matchHeader = Regex.Match(script, Helper.NameMap);
            if (matchHeader?.Groups.Count > 0)
            {
                var tableName = matchHeader.Groups[2].Value.Clear();
                result.AppendLine($"_ = entity.ToTable(\"{tableName}\");");
                result.AppendLine();
            }

            #endregion

            #region Keys

            matchHeader = Regex.Match(script, Helper.KeyZoneMap);
            if (!string.IsNullOrEmpty(matchHeader.Value))
            {
                string[] keys;

                matchesItems = Regex.Matches(matchHeader.Value, Helper.KeyMap);
                if (matchesItems?.Count > 0)
                {
                    keys = matchesItems.Skip(1).Select(x => "e." + x.Groups[1].Value.Clear().ToCamelCase()).ToArray();

                    result.AppendLine($"_ = entity.HasKey(e => new {{ {string.Join(", ", keys)} }});");
                    result.AppendLine();
                }
            }
            //_ = entity.HasIndex(e => new {{ {string.Join(", ", keys)} }}, "IDX_BEM");

            #endregion

            #region Index

            matchesHeaders = Regex.Matches(script, Helper.IndexZoneMap);
            foreach (Match x in matchesHeaders.ToList())
            {
                string[] keys;
                string indexName = x.Groups[1].Value;

                matchesItems = Regex.Matches(x.Value, Helper.IndexMap);
                if (matchesItems?.Count > 0)
                {
                    keys = matchesItems.Skip(2).Select(x => "e." + x.Groups[1].Value.Clear().ToCamelCase()).ToArray();

                    result.AppendLine($"_ = entity.HasIndex(e => new {{ {string.Join(", ", keys)} }}, \"{indexName}\" );");
                    result.AppendLine();
                }
            }

            #endregion

            #region Properties

            matchesItems = Regex.Matches(script, Helper.PropertieMap);

            foreach (Match x in matchesItems.ToList())
            {
                var paramName = x.Groups[1].Value.Clear();
                var paramType = x.Groups[2].Value.Clear().ToLower();
                var paramLength = x.Groups[3].Value.Clear();
                var paramRequired = x.Groups[4].Value.Clear().ToLower();

                result.AppendLine($"_ = entity.Property(e => e.{paramName.ToCamelCase()})");

                if (paramType.Contains("char"))
                {
                    result.AppendLine($".IsUnicode(false)");
                    if (paramType == "char")
                    {
                        result.AppendLine($".IsFixedLength(true)");
                    }
                }
                else if (paramType.Contains("date"))
                {
                    result.AppendLine($".HasColumnType(\"datetime\")");
                }
                else if (paramType.Contains("smallint"))
                {
                    result.AppendLine($".HasColumnType(\"smallint\")");
                }
                else if (paramType.Contains("numeric"))
                {
                    result.AppendLine($".HasColumnType(\"numeric({paramLength})\")");
                }
                else if (paramType.Contains("money"))
                {
                    result.AppendLine($".HasColumnType(\"money({paramLength})\")");
                }

                if (!string.IsNullOrEmpty(paramLength) && !paramLength.Contains(','))
                {
                    result.AppendLine($".HasMaxLength({paramLength})");
                }

                if (paramRequired.Contains("not null"))
                {
                    result.AppendLine($".IsRequired(true)");
                }

                result.AppendLine($".HasColumnName(\"{paramName}\");");
                result.AppendLine();
            }

            #endregion

            return result.ToString();
        }


    }
}

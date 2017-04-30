using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema
{
    public class PgColumn
    {
        private static readonly Regex PatternNull =
            new Regex("^(.+)[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private static readonly Regex PatternNotNull =
            new Regex("^(.+)[\\s]+NOT[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PatternDefault =
            new Regex("^(.+)[\\s]+DEFAULT[\\s]+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public PgColumn(string name) => Name = name;

        public string Comment { get; set; }

        public string DefaultValue { get; set; }

        public string Name { get; set; }

        public bool NullValue { get; set; } = true;

        public int? Statistics { get; set; }

        public string Storage { get; set; }

        public string Type { get; set; }


        public string GetFullDefinition(bool addDefaults)
        {
            var sbDefinition = new StringBuilder(100);
            sbDefinition.Append(PgDiffUtils.GetQuotedName(Name));
            sbDefinition.Append(' ');
            sbDefinition.Append(Type);

            if (DefaultValue != null && !string.IsNullOrEmpty(DefaultValue))
            {
                sbDefinition.Append(" DEFAULT ");
                sbDefinition.Append(DefaultValue);
            }
            else if (!NullValue && addDefaults)
            {
                var defaultColValue = PgColumnUtils.GetDefaultValue(Type);

                if (defaultColValue != null)
                {
                    sbDefinition.Append(" DEFAULT ");
                    sbDefinition.Append(defaultColValue);
                }
            }

            if (!NullValue) sbDefinition.Append(" NOT NULL");

            return sbDefinition.ToString();
        }

        


        public void ParseDefinition(string definition)
        {
            var str = definition;

            var matcher = PatternNotNull;

            if (matcher.IsMatch(str))
            {
                str = matcher.Matches(str)[0].Groups[1].Value.Trim();
                NullValue = false;
            }
            else
            {
                matcher = PatternNull;

                if (matcher.IsMatch(str))
                {
                    str = matcher.Matches(str)[0].Groups[1].Value.Trim();
                    NullValue = true;
                }
            }

            matcher = PatternDefault;

            if (matcher.IsMatch(str))
            {
                var matches = matcher.Matches(str);
                str = matches[0].Groups[1].Value.Trim();
                DefaultValue = matches[0].Groups[2].Value.Trim();
            }

            Type = str;
        }
    }
}
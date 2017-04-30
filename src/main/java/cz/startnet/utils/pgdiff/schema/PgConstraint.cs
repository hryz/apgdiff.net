using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema
{
    public class PgConstraint
    {
        private static readonly Regex PatternPrimaryKey = new Regex(".*PRIMARY[\\s]+KEY.*", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Comment { get; set; }

        public string Definition { get; set; }

        public string Name { get; set; }

        public string TableName { get; set; }


        public PgConstraint(string name)
        {
            Name = name;
        }


        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(100);
            sbSql.Append("ALTER TABLE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
            sbSql.Append("\n\tADD CONSTRAINT ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append(' ');
            sbSql.Append(Definition);
            sbSql.Append(';');

            if (Comment != null && !string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON CONSTRAINT ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" ON ");
                sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }


        


        public string GetDropSql()
        {
            var sbSql = new StringBuilder(100);
            sbSql.Append("ALTER TABLE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
            sbSql.Append("\n\tDROP CONSTRAINT ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append(';');

            return sbSql.ToString();
        }


        


        public bool IsPrimaryKeyConstraint()
        {
            return PatternPrimaryKey.IsMatch(Definition);
        }


       


        public override bool Equals(object @object)
        {
            var equals = false;

            if (this == @object)
            {
                equals = true;
            }
            else if (@object is PgConstraint)
            {
                var constraint = (PgConstraint) @object;
                equals = Definition.Equals(constraint.Definition)
                         && Name.Equals(constraint.Name)
                         && TableName.Equals(constraint.TableName);
            }

            return equals;
        }


        public override int GetHashCode()
        {
            return (GetType().Name + "|" + Definition + "|" + Name + "|" + TableName).GetHashCode();
        }
    }
}
using System.Text;

namespace pgdiff.schema
{
    public class PgIndex
    {
        public PgIndex(string name)
        {
            Name = name;
        }

        public string Comment { get; set; }

        public string Definition { get; set; }

        public string Name { get; }

        public string TableName { get; set; }

        public bool Unique { get; set; }

        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(100);
            sbSql.Append("CREATE ");

            if (Unique)
                sbSql.Append("UNIQUE ");

            sbSql.Append("INDEX ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append(" ON ");
            sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
            sbSql.Append(' ');
            sbSql.Append(Definition);
            sbSql.Append(';');

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON INDEX ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }

        public string GetDropSql()
        {
            return "DROP INDEX " + PgDiffUtils.GetQuotedName(Name) + ";";
        }

        public override bool Equals(object obj)
        {
            var equals = false;

            if (this == obj)
            {
                equals = true;
            }
            else if (obj is PgIndex index)
            {
                equals = Definition.Equals(index.Definition)
                         && Name.Equals(index.Name)
                         && TableName.Equals(index.TableName)
                         && Unique == index.Unique;
            }

            return equals;
        }


        public override int GetHashCode()
        {
            return (GetType().Name + "|" + Definition + "|" + Name + "|" + TableName + "|" + Unique).GetHashCode();
        }
    }
}
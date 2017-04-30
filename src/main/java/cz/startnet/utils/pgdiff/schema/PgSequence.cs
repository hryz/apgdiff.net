using System.Text;

namespace pgdiff.schema
{
    public class PgSequence
    {
        public string Cache { get; set; }

        public string Comment { get; set; }

        public bool Cycle { get; set; }

        public string Increment { get; set; }

        public string MaxValue { get; set; }

        public string MinValue { get; set; }

        public string Name { get; set; }

        public string OwnedBy { get; set; }

        public string StartWith { get; set; }


        public PgSequence(string name)
        {
            Name = name;
        }


        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(100);
            sbSql.Append("CREATE SEQUENCE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));

            if (StartWith != null)
            {
                sbSql.Append("\n\tSTART WITH ");
                sbSql.Append(StartWith);
            }

            if (Increment != null)
            {
                sbSql.Append("\n\tINCREMENT BY ");
                sbSql.Append(Increment);
            }

            sbSql.Append("\n\t");

            if (MaxValue == null)
            {
                sbSql.Append("NO MAXVALUE");
            }
            else
            {
                sbSql.Append("MAXVALUE ");
                sbSql.Append(MaxValue);
            }

            sbSql.Append("\n\t");

            if (MinValue == null)
            {
                sbSql.Append("NO MINVALUE");
            }
            else
            {
                sbSql.Append("MINVALUE ");
                sbSql.Append(MinValue);
            }

            if (Cache != null)
            {
                sbSql.Append("\n\tCACHE ");
                sbSql.Append(Cache);
            }

            if (Cycle) sbSql.Append("\n\tCYCLE");

            sbSql.Append(';');

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON SEQUENCE ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }


        public string GetOwnedBySql()
        {
            var sbSql = new StringBuilder(100);

            sbSql.Append("ALTER SEQUENCE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));

            if (!string.IsNullOrEmpty(OwnedBy))
            {
                sbSql.Append("\n\tOWNED BY ");
                sbSql.Append(OwnedBy);
            }

            sbSql.Append(';');

            return sbSql.ToString();
        }


        public string GetDropSql()
        {
            return "DROP SEQUENCE " + PgDiffUtils.GetQuotedName(Name) + ";";
        }
    }
}
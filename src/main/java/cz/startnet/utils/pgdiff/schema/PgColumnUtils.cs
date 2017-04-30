namespace pgdiff.schema
{
    public class PgColumnUtils
    {
        private PgColumnUtils()
        {
        }


        public static string GetDefaultValue(string type)
        {
            string defaultValue;
            var adjType = type.ToLower();

            if ("smallint".Equals(adjType)
                || "integer".Equals(adjType)
                || "bigint".Equals(adjType)
                || adjType.StartsWith("decimal")
                || adjType.StartsWith("numeric")
                || "real".Equals(adjType)
                || "double precision".Equals(adjType)
                || "int2".Equals(adjType)
                || "int4".Equals(adjType)
                || "int8".Equals(adjType)
                || adjType.StartsWith("float")
                || "double".Equals(adjType)
                || "money".Equals(adjType))
                defaultValue = "0";
            else if (adjType.StartsWith("character varying")
                     || adjType.StartsWith("varchar")
                     || adjType.StartsWith("character")
                     || adjType.StartsWith("char")
                     || "text".Equals(adjType))
                defaultValue = "''";
            else if ("boolean".Equals(adjType))
                defaultValue = "false";
            else
                defaultValue = null;

            return defaultValue;
        }
    }
}

using System;

namespace cz.startnet.utils.pgdiff.schema {




public class PgColumnUtils {

    
    public static String getDefaultValue(String type) {
        String defaultValue;
        String adjType = type.toLowerCase(Locale.ENGLISH);

        if ("smallint".Equals(adjType)
                || "integer".Equals(adjType)
                || "bigint".Equals(adjType)
                || adjType.startsWith("decimal")
                || adjType.startsWith("numeric")
                || "real".Equals(adjType)
                || "double precision".Equals(adjType)
                || "int2".Equals(adjType)
                || "int4".Equals(adjType)
                || "int8".Equals(adjType)
                || adjType.startsWith("float")
                || "double".Equals(adjType)
                || "money".Equals(adjType)) {
            defaultValue = "0";
        } else if (adjType.startsWith("character varying")
                || adjType.startsWith("varchar")
                || adjType.startsWith("character")
                || adjType.startsWith("char")
                || "text".Equals(adjType)) {
            defaultValue = "''";
        } else if ("boolean".Equals(adjType)) {
            defaultValue = "false";
        } else {
            defaultValue = null;
        }

        return defaultValue;
    }

    
    private PgColumnUtils() {
    }
}
}
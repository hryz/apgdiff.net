
namespace cz.startnet.utils.pgdiff.schema {




public class PgColumnUtils {

    
    public static String getDefaultValue(String type) {
        String defaultValue;
        String adjType = type.toLowerCase(Locale.ENGLISH);

        if ("smallint".equals(adjType)
                || "integer".equals(adjType)
                || "bigint".equals(adjType)
                || adjType.startsWith("decimal")
                || adjType.startsWith("numeric")
                || "real".equals(adjType)
                || "double precision".equals(adjType)
                || "int2".equals(adjType)
                || "int4".equals(adjType)
                || "int8".equals(adjType)
                || adjType.startsWith("float")
                || "double".equals(adjType)
                || "money".equals(adjType)) {
            defaultValue = "0";
        } else if (adjType.startsWith("character varying")
                || adjType.startsWith("varchar")
                || adjType.startsWith("character")
                || adjType.startsWith("char")
                || "text".equals(adjType)) {
            defaultValue = "''";
        } else if ("boolean".equals(adjType)) {
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
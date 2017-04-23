
namespace cz.startnet.utils.pgdiff.parsers {

using cz.startnet.utils.pgdiff.schema.PgDatabase;




public class ParserUtils {

    
    public static String getObjectName(String name) {
        String[] names = splitNames(name);

        return names[names.length - 1];
    }

    
    public static String getSecondObjectName(String name) {
        String[] names = splitNames(name);

        return names[names.length - 2];
    }

    
    public static String getThirdObjectName(String name) {
        String[] names = splitNames(name);

        return names.length >= 3 ? names[names.length - 3] : null;
    }

    
    public static String getSchemaName(String name,
            PgDatabase database) {
        String[] names = splitNames(name);

        if (names.length < 2) {
            return database.getDefaultSchema().getName();
        } else {
            return names[0];
        }
    }

    
    public static String generateName(String prefix,
            List<String> names, String postfix) {
        String adjName;

        if (names.size() == 1) {
            adjName = names.get(0);
        } else {
            StringBuilder sbString = new StringBuilder(names.size() * 15);

            for (String name : names) {
                if (sbString.length() > 0) {
                    sbString.append(',');
                }

                sbString.append(name);
            }

            adjName = Integer.toHexString(sbString.toString().hashCode());
        }

        StringBuilder sbResult = new StringBuilder(30);

        if (prefix != null && !prefix.isEmpty()) {
            sbResult.append(prefix);
        }

        sbResult.append(adjName);

        if (postfix != null && !postfix.isEmpty()) {
            sbResult.append(postfix);
        }

        return sbResult.toString();
    }

    
    private static String[] splitNames(String string) {
        if (string.indexOf('"') == -1) {
            return string.split("\\.");
        } else {
            List<String> strings = new ArrayList<String>(2);
            int startPos = 0;

            while (true) {
                if (string.charAt(startPos) == '"') {
                    int endPos = string.indexOf('"', startPos + 1);
                    strings.add(string.substring(startPos + 1, endPos));

                    if (endPos + 1 == string.length()) {
                        break;
                    } else if (string.charAt(endPos + 1) == '.') {
                        startPos = endPos + 2;
                    } else {
                        startPos = endPos + 1;
                    }
                } else {
                    int endPos = string.indexOf('.', startPos);

                    if (endPos == -1) {
                        strings.add(string.substring(startPos));
                        break;
                    } else {
                        strings.add(string.substring(startPos, endPos));
                        startPos = endPos + 1;
                    }
                }
            }

            return strings.toArray(new String[strings.size()]);
        }
    }

    
    private ParserUtils() {
    }
}
}
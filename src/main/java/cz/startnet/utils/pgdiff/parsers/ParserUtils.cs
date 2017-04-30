using System;
using System.Collections.Generic;
using System.Text;
using pgdiff.schema;

namespace pgdiff.parsers {





public class ParserUtils {

    
    public static String GetObjectName(String name) {
        String[] names = SplitNames(name);

        return names[names.Length - 1];
    }

    
    public static String GetSecondObjectName(String name) {
        String[] names = SplitNames(name);

        return names[names.Length - 2];
    }

    
    public static String GetThirdObjectName(String name) {
        String[] names = SplitNames(name);

        return names.Length >= 3 ? names[names.Length - 3] : null;
    }

    
    public static String GetSchemaName(String name,
            PgDatabase database) {
        String[] names = SplitNames(name);

        if (names.Length < 2) {
            return database.GetDefaultSchema().GetName();
        } else {
            return names[0];
        }
    }

    
    public static String GenerateName(String prefix,
            List<String> names, String postfix) {
        String adjName;

        if (names.Count == 1) {
            adjName = names[0];
        } else {
            StringBuilder sbString = new StringBuilder(names.Count * 15);

            foreach(String name in names) {
                if (sbString.Length > 0) {
                    sbString.Append(',');
                }

                sbString.Append(name);
            }

            adjName = sbString.ToString().GetHashCode().ToString("X4");
        }

        StringBuilder sbResult = new StringBuilder(30);

        if (!String.IsNullOrEmpty(prefix)) {
            sbResult.Append(prefix);
        }

        sbResult.Append(adjName);

        if (!String.IsNullOrEmpty(postfix)) {
            sbResult.Append(postfix);
        }

        return sbResult.ToString();
    }

    
    private static String[] SplitNames(String @string) {
        if (@string.IndexOf('"') == -1) {
            return @string.Split('.');
        } else {
            List<String> strings = new List<string>();
            int startPos = 0;

            while (true) {
                if (@string[startPos] == '"') {
                    int endPos = @string.IndexOf('"', startPos + 1);
                    strings.Add(@string.Substring(startPos + 1, endPos - startPos -1));

                    if (endPos + 1 == @string.Length) {
                        break;
                    } else if (@string[endPos + 1] == '.') {
                        startPos = endPos + 2;
                    } else {
                        startPos = endPos + 1;
                    }
                } else {
                    int endPos = @string.IndexOf('.', startPos);

                    if (endPos == -1) {
                        strings.Add(@string.Substring(startPos));
                        break;
                    } else {
                        strings.Add(@string.Substring(startPos, endPos - startPos));
                        startPos = endPos + 1;
                    }
                }
            }

            return strings.ToArray();
        }
    }

    
    private ParserUtils() {
    }
}
}
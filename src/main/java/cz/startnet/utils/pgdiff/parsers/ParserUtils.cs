using System.Collections.Generic;
using System.Text;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class ParserUtils
    {
        private ParserUtils()
        {
        }


        public static string GetObjectName(string name)
        {
            var names = SplitNames(name);
            return names[names.Length - 1];
        }


        public static string GetSecondObjectName(string name)
        {
            var names = SplitNames(name);
            return names[names.Length - 2];
        }


        public static string GetThirdObjectName(string name)
        {
            var names = SplitNames(name);
            return names.Length >= 3
                ? names[names.Length - 3]
                : null;
        }


        public static string GetSchemaName(string name, PgDatabase database)
        {
            var names = SplitNames(name);

            return names.Length < 2
                ? database.DefaultSchema.Name
                : names[0];
        }


        public static string GenerateName(string prefix, List<string> names, string postfix)
        {
            string adjName;

            if (names.Count == 1)
            {
                adjName = names[0];
            }
            else
            {
                var sbString = new StringBuilder(names.Count * 15);

                foreach (var name in names)
                {
                    if (sbString.Length > 0)
                        sbString.Append(',');
                    sbString.Append(name);
                }

                adjName = sbString.ToString().GetHashCode().ToString("X4");
            }

            var sbResult = new StringBuilder(30);

            if (!string.IsNullOrEmpty(prefix))
                sbResult.Append(prefix);

            sbResult.Append(adjName);

            if (!string.IsNullOrEmpty(postfix))
                sbResult.Append(postfix);

            return sbResult.ToString();
        }


        private static string[] SplitNames(string str)
        {
            if (str.IndexOf('"') == -1)
                return str.Split('.');
            var strings = new List<string>();
            var startPos = 0;

            while (true)
                if (str[startPos] == '"')
                {
                    var endPos = str.IndexOf('"', startPos + 1);
                    strings.Add(str.Substring(startPos + 1, endPos - startPos - 1));

                    if (endPos + 1 == str.Length)
                        break;
                    if (str[endPos + 1] == '.')
                        startPos = endPos + 2;
                    else
                        startPos = endPos + 1;
                }
                else
                {
                    var endPos = str.IndexOf('.', startPos);

                    if (endPos == -1)
                    {
                        strings.Add(str.Substring(startPos));
                        break;
                    }
                    strings.Add(str.Substring(startPos, endPos - startPos));
                    startPos = endPos + 1;
                }

            return strings.ToArray();
        }
    }
}
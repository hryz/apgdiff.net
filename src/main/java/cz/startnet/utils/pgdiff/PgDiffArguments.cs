using System.IO;
using System.Text;
using pgdiff.Properties;

namespace pgdiff
{
    public class PgDiffArguments
    {
        public bool AddDefaults { get; set; }

        public bool AddTransaction { get; set; }

        public bool IgnoreFunctionWhitespace { get; set; }

        public bool IgnoreSlonyTriggers { get; set; }

        public bool IgnoreStartWith { get; set; }

        public string InCharsetName { get; set; } = "UTF-8";

        public bool ListCharsets { get; set; }

        public string NewDumpFile { get; set; }

        public string OldDumpFile { get; set; }

        public string OutCharsetName { get; set; } = "UTF-8";

        public bool OutputIgnoredStatements { get; set; }

        public bool Version { get; set; }

        public bool Parse(TextReader reader, TextWriter writer, string[] args)
        {
            var success = true;
            int argsLength;

            if (args.Length >= 2)
                argsLength = args.Length - 2;
            else
                argsLength = args.Length;

            for (var i = 0; i < argsLength; i++)
                if ("--add-defaults".Equals(args[i]))
                {
                    AddDefaults = true;
                }
                else if ("--add-transaction".Equals(args[i]))
                {
                    AddTransaction = true;
                }
                else if ("--ignore-function-whitespace".Equals(args[i]))
                {
                    IgnoreFunctionWhitespace = true;
                }
                else if ("--ignore-slony-triggers".Equals(args[i]))
                {
                    IgnoreSlonyTriggers = true;
                }
                else if ("--ignore-start-with".Equals(args[i]))
                {
                    IgnoreStartWith = true;
                }
                else if ("--in-charset-name".Equals(args[i]))
                {
                    InCharsetName = args[i + 1];
                    i++;
                }
                else if ("--list-charsets".Equals(args[i]))
                {
                    ListCharsets = true;
                }
                else if ("--out-charset-name".Equals(args[i]))
                {
                    OutCharsetName = args[i + 1];
                    i++;
                }
                else if ("--output-ignored-statements".Equals(args[i]))
                {
                    OutputIgnoredStatements = true;
                }
                else if ("--version".Equals(args[i]))
                {
                    Version = true;
                }
                else
                {
                    writer.Write(Resources.ErrorUnknownOption);
                    writer.Write(": ");
                    writer.WriteLine(args[i]);
                    success = false;

                    break;
                }

            if (args.Length == 1 && Version)
            {
                PrintVersion(writer);
                success = false;
            }
            else if (args.Length == 1 && ListCharsets)
            {
                _ListCharsets(writer);
                success = false;
            }
            else if (args.Length < 2)
            {
                PrintUsage(writer);
                success = false;
            }
            else if (success)
            {
                OldDumpFile = args[args.Length - 2];
                NewDumpFile = args[args.Length - 1];
            }

            return success;
        }


        private static void PrintUsage(TextWriter writer) => writer.WriteLine(Resources.UsageHelp);
        
        private static void PrintVersion(TextWriter writer)
        {
            writer.Write(Resources.Version);
            writer.Write(": ");
            writer.WriteLine(Resources.VersionNumber);
        }

        private void _ListCharsets(TextWriter writer)
        {
            var charsets = Encoding.GetEncodings();

            foreach (var name in charsets)
                writer.WriteLine(name.Name);
        }
    }
}
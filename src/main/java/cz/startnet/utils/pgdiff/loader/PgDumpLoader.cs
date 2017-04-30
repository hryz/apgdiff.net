using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using pgdiff.parsers;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.loader
{
    public class PgDumpLoader
    {
        //NOPMD


        private static readonly Regex PatternCreateSchema = new Regex(
            "^CREATE[\\s]+SCHEMA[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);


        private static readonly Regex PatternDefaultSchema = new Regex(
            "^SET[\\s]+search_path[\\s]*=[\\s]*\"?([^,\\s\"]+)\"?(?:,[\\s]+.*)?;$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateTable = new Regex(
            "^CREATE[\\s]+TABLE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateView = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternAlterTable =
            new Regex("^ALTER[\\s]+TABLE[\\s]+.*$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateSequence = new Regex(
            "^CREATE[\\s]+SEQUENCE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternAlterSequence =
            new Regex("^ALTER[\\s]+SEQUENCE[\\s]+.*$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateIndex = new Regex(
            "^CREATE[\\s]+(?:UNIQUE[\\s]+)?INDEX[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternSelect = new Regex(
            "^SELECT[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternInsertInto = new Regex(
            "^INSERT[\\s]+INTO[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternUpdate = new Regex(
            "^UPDATE[\\s].*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternDeleteFrom = new Regex(
            "^DELETE[\\s]+FROM[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateTrigger = new Regex(
            "^CREATE[\\s]+TRIGGER[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternCreateFunction = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?FUNCTION[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternAlterView = new Regex(
            "^ALTER[\\s]+VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex PatternComment = new Regex(
            "^COMMENT[\\s]+ON[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static string _lineBuffer;


        private PgDumpLoader()
        {
        }


        public static PgDatabase LoadDatabaseSchema(TextReader reader, string charsetName, bool outputIgnoredStatements,
            bool ignoreSlonyTriggers)
        {
            var database = new PgDatabase();

            var statement = GetWholeStatement(reader);

            while (statement != null)
            {
                if (PatternCreateSchema.IsMatch(statement))
                {
                    CreateSchemaParser.Parse(database, statement);
                }
                else if (PatternDefaultSchema.IsMatch(statement))
                {
                    var matches = PatternDefaultSchema.Matches(statement);
                    database.SetDefaultSchema(matches[0].Groups[1].Value);
                }
                else if (PatternCreateTable.IsMatch(statement))
                {
                    CreateTableParser.Parse(database, statement);
                }
                else if (PatternAlterTable.IsMatch(statement))
                {
                    AlterTableParser.Parse(database, statement, outputIgnoredStatements);
                }
                else if (PatternCreateSequence.IsMatch(statement))
                {
                    CreateSequenceParser.Parse(database, statement);
                }
                else if (PatternAlterSequence.IsMatch(statement))
                {
                    AlterSequenceParser.Parse(database, statement, outputIgnoredStatements);
                }
                else if (PatternCreateIndex.IsMatch(statement))
                {
                    CreateIndexParser.Parse(database, statement);
                }
                else if (PatternCreateView.IsMatch(statement))
                {
                    CreateViewParser.Parse(database, statement);
                }
                else if (PatternAlterView.IsMatch(statement))
                {
                    AlterViewParser.Parse(database, statement, outputIgnoredStatements);
                }
                else if (PatternCreateTrigger.IsMatch(statement))
                {
                    CreateTriggerParser.Parse(database, statement, ignoreSlonyTriggers);
                }
                else if (PatternCreateFunction.IsMatch(statement))
                {
                    CreateFunctionParser.Parse(database, statement);
                }
                else if (PatternComment.IsMatch(statement))
                {
                    CommentParser.Parse(database, statement, outputIgnoredStatements);
                }
                else if (PatternSelect.IsMatch(statement)
                         || PatternInsertInto.IsMatch(statement)
                         || PatternUpdate.IsMatch(statement)
                         || PatternDeleteFrom.IsMatch(statement))
                {
                    // we just ignore these statements
                }
                else if (outputIgnoredStatements)
                {
                    database.IgnoredStatements.Add(statement);
                }

                statement = GetWholeStatement(reader);
            }

            return database;
        }


        public static PgDatabase LoadDatabaseSchema(string file, string charsetName, bool outputIgnoredStatements,bool ignoreSlonyTriggers)
        {
            try
            {
                return LoadDatabaseSchema(File.OpenText(file), charsetName, outputIgnoredStatements,ignoreSlonyTriggers);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileException(string.Format(Resources.FileNotFound, file), ex);
            }
        }


        private static string GetWholeStatement(TextReader reader)
        {
            var sbStatement = new StringBuilder(1024);

            if (_lineBuffer != null)
            {
                sbStatement.Append(_lineBuffer);
                _lineBuffer = null;
                StripComment(sbStatement);
            }

            var pos = sbStatement.ToString().IndexOf(";", StringComparison.Ordinal);

            while (true)
                if (pos == -1)
                {
                    string newLine;

                    try
                    {
                        newLine = reader.ReadLine();
                    }
                    catch (IOException ex)
                    {
                        throw new FileException(Resources.CannotReadFile, ex);
                    }

                    if (newLine == null)
                        if (sbStatement.ToString().Trim().Length == 0)
                            return null;
                        else
                            throw new Exception(string.Format(Resources.EndOfStatementNotFound, sbStatement));

                    if (sbStatement.Length > 0) sbStatement.Append('\n');

                    pos = sbStatement.Length;
                    sbStatement.Append(newLine);
                    StripComment(sbStatement);

                    pos = sbStatement.ToString().IndexOf(";", pos, StringComparison.Ordinal);
                }
                else
                {
                    if (!IsQuoted(sbStatement, pos))
                    {
                        if (pos == sbStatement.Length - 1)
                        {
                            _lineBuffer = null;
                        }
                        else
                        {
                            _lineBuffer = sbStatement.ToString().Substring(pos + 1);
                            sbStatement.Length = pos + 1;
                        }

                        return sbStatement.ToString().Trim();
                    }

                    pos = sbStatement.ToString().IndexOf(";", pos + 1, StringComparison.Ordinal);
                }
        }


        private static void StripComment(StringBuilder sbStatement)
        {
            var pos = sbStatement.ToString().IndexOf("--", StringComparison.Ordinal);

            while (pos >= 0)
            {
                if (pos == 0)
                {
                    sbStatement.Length = 0;
                    return;
                }
                if (!IsQuoted(sbStatement, pos))
                {
                    sbStatement.Length = pos;
                    return;
                }

                pos = sbStatement.ToString().IndexOf("--", pos + 1, StringComparison.Ordinal);
            }
        }


        private static bool IsQuoted(StringBuilder sbString, int pos)
        {
            var isQuoted = false;

            for (var curPos = 0; curPos < pos; curPos++)
                if (sbString[curPos] == '\'')
                {
                    isQuoted = !isQuoted;

                    // if quote was escaped by backslash, it's like double quote
                    if (pos > 0 && sbString[pos - 1] == '\\') isQuoted = !isQuoted;
                }
                else if (sbString[curPos] == '$' && !isQuoted)
                {
                    var endPos = sbString.ToString().IndexOf("$", curPos + 1, StringComparison.Ordinal);

                    if (endPos == -1)
                        return true;

                    var tag = sbString.ToString().Substring(curPos, endPos + 1 - curPos);
                    var endTagPos = sbString.ToString().IndexOf(tag, endPos + 1, StringComparison.Ordinal);

                    // if end tag was not found or it was found after the checked
                    // position, it's quoted
                    if (endTagPos == -1 || endTagPos > pos)
                        return true;

                    curPos = endTagPos + tag.Length - 1;
                }

            return isQuoted;
        }
    }
}
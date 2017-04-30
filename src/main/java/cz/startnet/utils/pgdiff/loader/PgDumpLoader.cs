using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using pgdiff.parsers;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.loader {













    public class PgDumpLoader
    {
        //NOPMD


        private static Regex _patternCreateSchema = new Regex(
            "^CREATE[\\s]+SCHEMA[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline); 
    

    

    private static Regex _patternDefaultSchema = new Regex(
            "^SET[\\s]+search_path[\\s]*=[\\s]*\"?([^,\\s\"]+)\"?"
            + "(?:,[\\s]+.*)?;$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateTable = new Regex(
            "^CREATE[\\s]+TABLE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateView = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternAlterTable =
            new Regex("^ALTER[\\s]+TABLE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateSequence = new Regex(
            "^CREATE[\\s]+SEQUENCE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternAlterSequence =
            new Regex("^ALTER[\\s]+SEQUENCE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateIndex = new Regex(
            "^CREATE[\\s]+(?:UNIQUE[\\s]+)?INDEX[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternSelect = new Regex(
            "^SELECT[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternInsertInto = new Regex(
            "^INSERT[\\s]+INTO[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternUpdate = new Regex(
            "^UPDATE[\\s].*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternDeleteFrom = new Regex(
            "^DELETE[\\s]+FROM[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateTrigger = new Regex(
            "^CREATE[\\s]+TRIGGER[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternCreateFunction = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?FUNCTION[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternAlterView = new Regex(
            "^ALTER[\\s]+VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static Regex _patternComment = new Regex(
            "^COMMENT[\\s]+ON[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    
    private static String _lineBuffer;

    
    public static PgDatabase LoadDatabaseSchema(TextReader reader, String charsetName, bool outputIgnoredStatements, bool ignoreSlonyTriggers) {

        PgDatabase database = new PgDatabase();

        String statement = GetWholeStatement(reader);

        while (statement != null) {
            if (_patternCreateSchema.IsMatch(statement)) {
                CreateSchemaParser.Parse(database, statement);
            } else if (_patternDefaultSchema.IsMatch(statement)) {
                var matches = _patternDefaultSchema.Matches(statement);
                database.SetDefaultSchema(matches[0].Groups[1].Value);
            } else if (_patternCreateTable.IsMatch(statement)) {
                CreateTableParser.Parse(database, statement);
            } else if (_patternAlterTable.IsMatch(statement)) {
                AlterTableParser.Parse(
                        database, statement, outputIgnoredStatements);
            } else if (_patternCreateSequence.IsMatch(statement)) {
                CreateSequenceParser.Parse(database, statement);
            } else if (_patternAlterSequence.IsMatch(statement)) {
                AlterSequenceParser.Parse(
                        database, statement, outputIgnoredStatements);
            } else if (_patternCreateIndex.IsMatch(statement)) {
                CreateIndexParser.Parse(database, statement);
            } else if (_patternCreateView.IsMatch(statement)) {
                CreateViewParser.Parse(database, statement);
            } else if (_patternAlterView.IsMatch(statement)) {
                AlterViewParser.Parse(
                        database, statement, outputIgnoredStatements);
            } else if (_patternCreateTrigger.IsMatch(statement)) {
                CreateTriggerParser.Parse(
                        database, statement, ignoreSlonyTriggers);
            } else if (_patternCreateFunction.IsMatch(statement)) {
                CreateFunctionParser.Parse(database, statement);
            } else if (_patternComment.IsMatch(statement)) {
                CommentParser.Parse(
                        database, statement, outputIgnoredStatements);
            } else if (_patternSelect.IsMatch(statement)
                    || _patternInsertInto.IsMatch(statement)
                    || _patternUpdate.IsMatch(statement)
                    || _patternDeleteFrom.IsMatch(statement)) {
                // we just ignore these statements
            } else if (outputIgnoredStatements) {
                database.AddIgnoredStatement(statement);
            } else {
                // these statements are ignored if outputIgnoredStatements
                // is false
            }

            statement = GetWholeStatement(reader);
        }

        return database;
    }

    
    public static PgDatabase LoadDatabaseSchema(String file, String charsetName, bool outputIgnoredStatements, bool ignoreSlonyTriggers) {
        try {
            return LoadDatabaseSchema(File.OpenText(file), charsetName, outputIgnoredStatements, ignoreSlonyTriggers);
        } catch (FileNotFoundException ex) {
            throw new FileException(String.Format(Resources.FileNotFound, file), ex);
        }
    }


    private static String GetWholeStatement(TextReader reader) {
        StringBuilder sbStatement = new StringBuilder(1024);

        if (_lineBuffer != null) {
            sbStatement.Append(_lineBuffer);
            _lineBuffer = null;
            StripComment(sbStatement);
        }

        int pos = sbStatement.ToString().IndexOf(";", StringComparison.Ordinal);

        while (true) {
            if (pos == -1) {
                String newLine;

                try {
                    newLine = reader.ReadLine();
                } catch (IOException ex) {
                    throw new FileException(Resources.CannotReadFile, ex);
                }

                if (newLine == null) {
                    if (sbStatement.ToString().Trim().Length == 0) {
                        return null;
                    } else {
                        throw new Exception(String.Format(Resources.EndOfStatementNotFound, sbStatement));
                    }
                }

                if (sbStatement.Length > 0) {
                    sbStatement.Append('\n');
                }

                pos = sbStatement.Length;
                sbStatement.Append(newLine);
                StripComment(sbStatement);

                pos = sbStatement.ToString().IndexOf(";", pos, StringComparison.Ordinal);
            } else {
                if (!IsQuoted(sbStatement, pos)) {
                    if (pos == sbStatement.Length - 1) {
                        _lineBuffer = null;
                    } else {
                        _lineBuffer = sbStatement.ToString().Substring(pos + 1);
                        sbStatement.Length = pos + 1;
                    }

                    return sbStatement.ToString().Trim();
                }

                pos = sbStatement.ToString().IndexOf(";", pos + 1, StringComparison.Ordinal);
            }
        }
    }

    
    private static void StripComment(StringBuilder sbStatement) {
        int pos = sbStatement.ToString().IndexOf("--", StringComparison.Ordinal);

        while (pos >= 0) {
            if (pos == 0) {
                sbStatement.Length = 0;

                return;
            } else {
                if (!IsQuoted(sbStatement, pos)) {
                    sbStatement.Length = pos;

                    return;
                }
            }

            pos = sbStatement.ToString().IndexOf("--", pos + 1, StringComparison.Ordinal);
        }
    }

    
    
    private static bool IsQuoted(StringBuilder sbString,
            int pos) {
        bool isQuoted = false;

        for (int curPos = 0; curPos < pos; curPos++) {
            if (sbString[curPos] == '\'') {
                isQuoted = !isQuoted;

                // if quote was escaped by backslash, it's like double quote
                if (pos > 0 && sbString[pos - 1] == '\\') {
                    isQuoted = !isQuoted;
                }
            } else if (sbString[curPos] == '$' && !isQuoted) {
                int endPos = sbString.ToString().IndexOf("$", curPos + 1, StringComparison.Ordinal);

                if (endPos == -1) {
                    return true;
                }

                String tag = sbString.ToString().Substring(curPos, endPos + 1 - curPos);
                int endTagPos = sbString.ToString().IndexOf(tag, endPos + 1, StringComparison.Ordinal);

                // if end tag was not found or it was found after the checked
                // position, it's quoted
                if (endTagPos == -1 || endTagPos > pos) {
                    return true;
                }

                curPos = endTagPos + tag.Length - 1;
            }
        }

        return isQuoted;
    }

    
    private PgDumpLoader() {
    }
}
}
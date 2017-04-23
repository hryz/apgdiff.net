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


        private static Regex PATTERN_CREATE_SCHEMA = new Regex(
            "^CREATE[\\s]+SCHEMA[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase); 
    

    

    private static Regex PATTERN_DEFAULT_SCHEMA = new Regex(
            "^SET[\\s]+search_path[\\s]*=[\\s]*\"?([^,\\s\"]+)\"?"
            + "(?:,[\\s]+.*)?;$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_TABLE = new Regex(
            "^CREATE[\\s]+TABLE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_VIEW = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_ALTER_TABLE =
            new Regex("^ALTER[\\s]+TABLE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_SEQUENCE = new Regex(
            "^CREATE[\\s]+SEQUENCE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_ALTER_SEQUENCE =
            new Regex("^ALTER[\\s]+SEQUENCE[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_INDEX = new Regex(
            "^CREATE[\\s]+(?:UNIQUE[\\s]+)?INDEX[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_SELECT = new Regex(
            "^SELECT[\\s]+.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_INSERT_INTO = new Regex(
            "^INSERT[\\s]+INTO[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_UPDATE = new Regex(
            "^UPDATE[\\s].*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_DELETE_FROM = new Regex(
            "^DELETE[\\s]+FROM[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_TRIGGER = new Regex(
            "^CREATE[\\s]+TRIGGER[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_CREATE_FUNCTION = new Regex(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?FUNCTION[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_ALTER_VIEW = new Regex(
            "^ALTER[\\s]+VIEW[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_COMMENT = new Regex(
            "^COMMENT[\\s]+ON[\\s]+.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static String lineBuffer;

    
    public static PgDatabase loadDatabaseSchema(TextReader reader, String charsetName, bool outputIgnoredStatements, bool ignoreSlonyTriggers) {

        PgDatabase database = new PgDatabase();

        String statement = getWholeStatement(reader);

        while (statement != null) {
            if (PATTERN_CREATE_SCHEMA.IsMatch(statement)) {
                CreateSchemaParser.parse(database, statement);
            } else if (PATTERN_DEFAULT_SCHEMA.IsMatch(statement)) {
                var matches = PATTERN_DEFAULT_SCHEMA.Matches(statement);
                database.setDefaultSchema(matches[0].Groups[1].Value);
            } else if (PATTERN_CREATE_TABLE.IsMatch(statement)) {
                CreateTableParser.parse(database, statement);
            } else if (PATTERN_ALTER_TABLE.IsMatch(statement)) {
                AlterTableParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_SEQUENCE.IsMatch(statement)) {
                CreateSequenceParser.parse(database, statement);
            } else if (PATTERN_ALTER_SEQUENCE.IsMatch(statement)) {
                AlterSequenceParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_INDEX.IsMatch(statement)) {
                CreateIndexParser.parse(database, statement);
            } else if (PATTERN_CREATE_VIEW.IsMatch(statement)) {
                CreateViewParser.parse(database, statement);
            } else if (PATTERN_ALTER_VIEW.IsMatch(statement)) {
                AlterViewParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_TRIGGER.IsMatch(statement)) {
                CreateTriggerParser.parse(
                        database, statement, ignoreSlonyTriggers);
            } else if (PATTERN_CREATE_FUNCTION.IsMatch(statement)) {
                CreateFunctionParser.parse(database, statement);
            } else if (PATTERN_COMMENT.IsMatch(statement)) {
                CommentParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_SELECT.IsMatch(statement)
                    || PATTERN_INSERT_INTO.IsMatch(statement)
                    || PATTERN_UPDATE.IsMatch(statement)
                    || PATTERN_DELETE_FROM.IsMatch(statement)) {
                // we just ignore these statements
            } else if (outputIgnoredStatements) {
                database.addIgnoredStatement(statement);
            } else {
                // these statements are ignored if outputIgnoredStatements
                // is false
            }

            statement = getWholeStatement(reader);
        }

        return database;
    }

    
    public static PgDatabase loadDatabaseSchema(String file, String charsetName, bool outputIgnoredStatements, bool ignoreSlonyTriggers) {
        try {
            return loadDatabaseSchema(File.OpenText(file), charsetName, outputIgnoredStatements, ignoreSlonyTriggers);
        } catch (FileNotFoundException ex) {
            throw new FileException(String.Format(Resources.FileNotFound, file), ex);
        }
    }


    private static String getWholeStatement(TextReader reader) {
        StringBuilder sbStatement = new StringBuilder(1024);

        if (lineBuffer != null) {
            sbStatement.Append(lineBuffer);
            lineBuffer = null;
            stripComment(sbStatement);
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
                stripComment(sbStatement);

                pos = sbStatement.ToString().IndexOf(";", pos, StringComparison.Ordinal);
            } else {
                if (!isQuoted(sbStatement, pos)) {
                    if (pos == sbStatement.Length - 1) {
                        lineBuffer = null;
                    } else {
                        lineBuffer = sbStatement.ToString().Substring(pos + 1);
                        sbStatement.Length = pos + 1;
                    }

                    return sbStatement.ToString().Trim();
                }

                pos = sbStatement.ToString().IndexOf(";", pos + 1, StringComparison.Ordinal);
            }
        }
    }

    
    private static void stripComment(StringBuilder sbStatement) {
        int pos = sbStatement.ToString().IndexOf("--", StringComparison.Ordinal);

        while (pos >= 0) {
            if (pos == 0) {
                sbStatement.Length = 0;

                return;
            } else {
                if (!isQuoted(sbStatement, pos)) {
                    sbStatement.Length = pos;

                    return;
                }
            }

            pos = sbStatement.ToString().IndexOf("--", pos + 1, StringComparison.Ordinal);
        }
    }

    
    
    private static bool isQuoted(StringBuilder sbString,
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

                String tag = sbString.ToString().Substring(curPos, endPos + 1);
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
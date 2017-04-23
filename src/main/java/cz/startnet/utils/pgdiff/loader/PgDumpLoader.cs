
namespace cz.startnet.utils.pgdiff.loader {

using cz.startnet.utils.pgdiff.Resources;
using cz.startnet.utils.pgdiff.parsers.AlterSequenceParser;
using cz.startnet.utils.pgdiff.parsers.AlterTableParser;
using cz.startnet.utils.pgdiff.parsers.AlterViewParser;
using cz.startnet.utils.pgdiff.parsers.CommentParser;
using cz.startnet.utils.pgdiff.parsers.CreateFunctionParser;
using cz.startnet.utils.pgdiff.parsers.CreateIndexParser;
using cz.startnet.utils.pgdiff.parsers.CreateSchemaParser;
using cz.startnet.utils.pgdiff.parsers.CreateSequenceParser;
using cz.startnet.utils.pgdiff.parsers.CreateTableParser;
using cz.startnet.utils.pgdiff.parsers.CreateTriggerParser;
using cz.startnet.utils.pgdiff.parsers.CreateViewParser;
using cz.startnet.utils.pgdiff.schema.PgDatabase;












public class PgDumpLoader { //NOPMD

    
    private static Pattern PATTERN_CREATE_SCHEMA = Pattern.compile(
            "^CREATE[\\s]+SCHEMA[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);

    private static Pattern PATTERN_DEFAULT_SCHEMA = Pattern.compile(
            "^SET[\\s]+search_path[\\s]*=[\\s]*\"?([^,\\s\"]+)\"?"
            + "(?:,[\\s]+.*)?;$", Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_TABLE = Pattern.compile(
            "^CREATE[\\s]+TABLE[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_VIEW = Pattern.compile(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?VIEW[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_ALTER_TABLE =
            Pattern.compile("^ALTER[\\s]+TABLE[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_SEQUENCE = Pattern.compile(
            "^CREATE[\\s]+SEQUENCE[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_ALTER_SEQUENCE =
            Pattern.compile("^ALTER[\\s]+SEQUENCE[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_INDEX = Pattern.compile(
            "^CREATE[\\s]+(?:UNIQUE[\\s]+)?INDEX[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_SELECT = Pattern.compile(
            "^SELECT[\\s]+.*$", Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_INSERT_INTO = Pattern.compile(
            "^INSERT[\\s]+INTO[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_UPDATE = Pattern.compile(
            "^UPDATE[\\s].*$", Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_DELETE_FROM = Pattern.compile(
            "^DELETE[\\s]+FROM[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_TRIGGER = Pattern.compile(
            "^CREATE[\\s]+TRIGGER[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_CREATE_FUNCTION = Pattern.compile(
            "^CREATE[\\s]+(?:OR[\\s]+REPLACE[\\s]+)?FUNCTION[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_ALTER_VIEW = Pattern.compile(
            "^ALTER[\\s]+VIEW[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static Pattern PATTERN_COMMENT = Pattern.compile(
            "^COMMENT[\\s]+ON[\\s]+.*$",
            Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
    
    private static String lineBuffer;

    
    public static PgDatabase loadDatabaseSchema(InputStream inputStream,
            String charsetName, boolean outputIgnoredStatements,
            boolean ignoreSlonyTriggers) {

        PgDatabase database = new PgDatabase();
        BufferedReader reader = null;

        try {
            reader = new BufferedReader(
                    new InputStreamReader(inputStream, charsetName));
        } catch (UnsupportedEncodingException ex) {
            throw new UnsupportedOperationException(
                    Resources.getString("UnsupportedEncoding") + ": "
                    + charsetName, ex);
        }

        String statement = getWholeStatement(reader);

        while (statement != null) {
            if (PATTERN_CREATE_SCHEMA.matcher(statement).matches()) {
                CreateSchemaParser.parse(database, statement);
            } else if (PATTERN_DEFAULT_SCHEMA.matcher(statement).matches()) {
                Matcher matcher =
                        PATTERN_DEFAULT_SCHEMA.matcher(statement);
                matcher.matches();
                database.setDefaultSchema(matcher.group(1));
            } else if (PATTERN_CREATE_TABLE.matcher(statement).matches()) {
                CreateTableParser.parse(database, statement);
            } else if (PATTERN_ALTER_TABLE.matcher(statement).matches()) {
                AlterTableParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_SEQUENCE.matcher(statement).matches()) {
                CreateSequenceParser.parse(database, statement);
            } else if (PATTERN_ALTER_SEQUENCE.matcher(statement).matches()) {
                AlterSequenceParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_INDEX.matcher(statement).matches()) {
                CreateIndexParser.parse(database, statement);
            } else if (PATTERN_CREATE_VIEW.matcher(statement).matches()) {
                CreateViewParser.parse(database, statement);
            } else if (PATTERN_ALTER_VIEW.matcher(statement).matches()) {
                AlterViewParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_CREATE_TRIGGER.matcher(statement).matches()) {
                CreateTriggerParser.parse(
                        database, statement, ignoreSlonyTriggers);
            } else if (PATTERN_CREATE_FUNCTION.matcher(statement).matches()) {
                CreateFunctionParser.parse(database, statement);
            } else if (PATTERN_COMMENT.matcher(statement).matches()) {
                CommentParser.parse(
                        database, statement, outputIgnoredStatements);
            } else if (PATTERN_SELECT.matcher(statement).matches()
                    || PATTERN_INSERT_INTO.matcher(statement).matches()
                    || PATTERN_UPDATE.matcher(statement).matches()
                    || PATTERN_DELETE_FROM.matcher(statement).matches()) {
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

    
    public static PgDatabase loadDatabaseSchema(String file,
            String charsetName, boolean outputIgnoredStatements,
            boolean ignoreSlonyTriggers) {
        try {
            return loadDatabaseSchema(new FileInputStream(file), charsetName,
                    outputIgnoredStatements, ignoreSlonyTriggers);
        } catch (FileNotFoundException ex) {
            throw new FileException(MessageFormat.format(
                    Resources.getString("FileNotFound"), file), ex);
        }
    }


    private static String getWholeStatement(BufferedReader reader) {
        StringBuilder sbStatement = new StringBuilder(1024);

        if (lineBuffer != null) {
            sbStatement.append(lineBuffer);
            lineBuffer = null;
            stripComment(sbStatement);
        }

        int pos = sbStatement.indexOf(";");

        while (true) {
            if (pos == -1) {
                String newLine;

                try {
                    newLine = reader.readLine();
                } catch (IOException ex) {
                    throw new FileException(
                            Resources.getString("CannotReadFile"), ex);
                }

                if (newLine == null) {
                    if (sbStatement.toString().trim().length() == 0) {
                        return null;
                    } else {
                        throw new RuntimeException(MessageFormat.format(
                                Resources.getString("EndOfStatementNotFound"),
                                sbStatement.toString()));
                    }
                }

                if (sbStatement.length() > 0) {
                    sbStatement.append('\n');
                }

                pos = sbStatement.length();
                sbStatement.append(newLine);
                stripComment(sbStatement);

                pos = sbStatement.indexOf(";", pos);
            } else {
                if (!isQuoted(sbStatement, pos)) {
                    if (pos == sbStatement.length() - 1) {
                        lineBuffer = null;
                    } else {
                        lineBuffer = sbStatement.substring(pos + 1);
                        sbStatement.setLength(pos + 1);
                    }

                    return sbStatement.toString().trim();
                }

                pos = sbStatement.indexOf(";", pos + 1);
            }
        }
    }

    
    private static void stripComment(StringBuilder sbStatement) {
        int pos = sbStatement.indexOf("--");

        while (pos >= 0) {
            if (pos == 0) {
                sbStatement.setLength(0);

                return;
            } else {
                if (!isQuoted(sbStatement, pos)) {
                    sbStatement.setLength(pos);

                    return;
                }
            }

            pos = sbStatement.indexOf("--", pos + 1);
        }
    }

    
    @SuppressWarnings("AssignmentToForLoopParameter")
    private static boolean isQuoted(StringBuilder sbString,
            int pos) {
        boolean isQuoted = false;

        for (int curPos = 0; curPos < pos; curPos++) {
            if (sbString.charAt(curPos) == '\'') {
                isQuoted = !isQuoted;

                // if quote was escaped by backslash, it's like double quote
                if (pos > 0 && sbString.charAt(pos - 1) == '\\') {
                    isQuoted = !isQuoted;
                }
            } else if (sbString.charAt(curPos) == '$' && !isQuoted) {
                int endPos = sbString.indexOf("$", curPos + 1);

                if (endPos == -1) {
                    return true;
                }

                String tag = sbString.substring(curPos, endPos + 1);
                int endTagPos = sbString.indexOf(tag, endPos + 1);

                // if end tag was not found or it was found after the checked
                // position, it's quoted
                if (endTagPos == -1 || endTagPos > pos) {
                    return true;
                }

                curPos = endTagPos + tag.length() - 1;
            }
        }

        return isQuoted;
    }

    
    private PgDumpLoader() {
    }
}
}
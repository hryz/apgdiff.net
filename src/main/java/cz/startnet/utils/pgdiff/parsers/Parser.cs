
package cz.startnet.utils.pgdiff.parsers;

import cz.startnet.utils.pgdiff.Resources;
import java.text.MessageFormat;
import java.util.Locale;


@SuppressWarnings("FinalClass")
public final class Parser {

    
    private String string;
    
    private int position;

    
    public Parser(final String string) {
        this.string = string;
        skipWhitespace();
    }

    
    public void expect(final String... words) {
        for (final String word : words) {
            expect(word, false);
        }
    }

    public boolean expect(final String word, final boolean optional) {
        final int wordEnd = position + word.length();

        if (wordEnd <= string.length()
                && string.substring(position, wordEnd).equalsIgnoreCase(word)
                && (wordEnd == string.length()
                || Character.isWhitespace(string.charAt(wordEnd))
                || string.charAt(wordEnd) == ';'
                || string.charAt(wordEnd) == ')'
                || string.charAt(wordEnd) == ','
                || string.charAt(wordEnd) == '['
                || "(".equals(word) || ",".equals(word) || "[".equals(word)
                || "]".equals(word))) {
            position = wordEnd;
            skipWhitespace();

            return true;
        }

        if (optional) {
            return false;
        }

        throw new ParserException(MessageFormat.format(
                Resources.getString("CannotParseStringExpectedWord"), string,
                word, position + 1, string.substring(position, position + 20)));
    }

    
    public boolean expectOptional(final String... words) {
        final boolean found = expect(words[0], true);

        if (!found) {
            return false;
        }

        for (int i = 1; i < words.length; i++) {
            skipWhitespace();
            expect(words[i]);
        }

        return true;
    }

    
    public void skipWhitespace() {
        for (; position < string.length(); position++) {
            if (!Character.isWhitespace(string.charAt(position))) {
                break;
            }
        }
    }

    
    public String parseIdentifier() {
        String identifier = parseIdentifierInternal();

        if (string.charAt(position) == '.') {
            position++;
            identifier += '.' + parseIdentifierInternal();
        }

        skipWhitespace();

        return identifier;
    }

    
    private String parseIdentifierInternal() {
        final boolean quoted = string.charAt(position) == '"';

        if (quoted) {
            final int endPos = string.indexOf('"', position + 1);
            final String result = string.substring(position, endPos + 1);
            position = endPos + 1;

            return result;
        } else {
            int endPos = position;

            for (; endPos < string.length(); endPos++) {
                final char chr = string.charAt(endPos);

                if (Character.isWhitespace(chr) || chr == ',' || chr == ')'
                        || chr == '(' || chr == ';' || chr == '.') {
                    break;
                }
            }

            final String result =
                    string.substring(position, endPos).toLowerCase(
                    Locale.ENGLISH);

            position = endPos;

            return result;
        }
    }

    
    public String getRest() {
        final String result;

        if (string.charAt(string.length() - 1) == ';') {
            if (position == string.length() - 1) {
                return null;
            } else {
                result = string.substring(position, string.length() - 1);
            }
        } else {
            result = string.substring(position);
        }

        position = string.length();

        return result;
    }

    
    public int parseInteger() {
        int endPos = position;

        for (; endPos < string.length(); endPos++) {
            if (!Character.isLetterOrDigit(string.charAt(endPos))) {
                break;
            }
        }

        try {
            final int result =
                    Integer.parseInt(string.substring(position, endPos));

            position = endPos;
            skipWhitespace();

            return result;
        } catch (final NumberFormatException ex) {
            throw new ParserException(MessageFormat.format(
                    Resources.getString("CannotParseStringExpectedInteger"),
                    string, position + 1,
                    string.substring(position, position + 20)), ex);
        }
    }

    
    public String parseString() {
        final boolean quoted = string.charAt(position) == '\'';

        if (quoted) {
            boolean escape = false;
            int endPos = position + 1;

            for (; endPos < string.length(); endPos++) {
                final char chr = string.charAt(endPos);

                if (chr == '\\') {
                    escape = !escape;
                } else if (!escape && chr == '\'') {
                    if (endPos + 1 < string.length()
                            && string.charAt(endPos + 1) == '\'') {
                        endPos++;
                    } else {
                        break;
                    }
                }
            }

            final String result;

            try {
                result = string.substring(position, endPos + 1);
            } catch (final Throwable ex) {
                throw new RuntimeException("Failed to get substring: " + string
                        + " start pos: " + position + " end pos: "
                        + (endPos + 1), ex);
            }

            position = endPos + 1;
            skipWhitespace();

            return result;
        } else {
            int endPos = position;

            for (; endPos < string.length(); endPos++) {
                final char chr = string.charAt(endPos);

                if (Character.isWhitespace(chr) || chr == ',' || chr == ')'
                        || chr == ';') {
                    break;
                }
            }

            if (position == endPos) {
                throw new ParserException(MessageFormat.format(
                        Resources.getString("CannotParseStringExpectedString"),
                        string, position + 1));
            }

            final String result = string.substring(position, endPos);

            position = endPos;
            skipWhitespace();

            return result;
        }
    }

    
    public String getExpression() {
        final int endPos = getExpressionEnd();

        if (position == endPos) {
            throw new ParserException(MessageFormat.format(
                    Resources.getString("CannotParseStringExpectedExpression"),
                    string, position + 1,
                    string.substring(position, position + 20)));
        }

        final String result = string.substring(position, endPos).trim();

        position = endPos;

        return result;
    }

    
    private int getExpressionEnd() {
        int bracesCount = 0;
        boolean singleQuoteOn = false;
        int charPos = position;

        for (; charPos < string.length(); charPos++) {
            final char chr = string.charAt(charPos);

            if (chr == '(') {
                bracesCount++;
            } else if (chr == ')') {
                if (bracesCount == 0) {
                    break;
                } else {
                    bracesCount--;
                }
            } else if (chr == '\'') {
                singleQuoteOn = !singleQuoteOn;
            } else if ((chr == ',') && !singleQuoteOn && (bracesCount == 0)) {
                break;
            } else if (chr == ';' && bracesCount == 0 && !singleQuoteOn) {
                break;
            }
        }

        return charPos;
    }

    
    public int getPosition() {
        return position;
    }

    
    public String getString() {
        return string;
    }

    
    public void throwUnsupportedCommand() {
        throw new ParserException(MessageFormat.format(
                Resources.getString("CannotParseStringUnsupportedCommand"),
                string, position + 1,
                string.substring(position, position + 20)));
    }

  
    public String expectOptionalOneOf(final String... words) {
        for (final String word : words) {
            if (expectOptional(word)) {
                return word;
            }
        }

        return null;
    }

    
    public String getSubString(final int startPos, final int endPos) {
        return string.substring(startPos, endPos);
    }

    
    public void setPosition(final int position) {
        this.position = position;
    }

    
    public String parseDataType() {
        int endPos = position;

        while (endPos < string.length()
                && !Character.isWhitespace(string.charAt(endPos))
                && string.charAt(endPos) != '('
                && string.charAt(endPos) != ')'
                && string.charAt(endPos) != ',') {
            endPos++;
        }

        if (endPos == position) {
            throw new ParserException(MessageFormat.format(
                    Resources.getString("CannotParseStringExpectedDataType"),
                    string, position + 1,
                    string.substring(position, position + 20)));
        }

        String dataType = string.substring(position, endPos);

        position = endPos;
        skipWhitespace();

        if ("character".equalsIgnoreCase(dataType)
                && expectOptional("varying")) {
            dataType = "character varying";
        } else if ("double".equalsIgnoreCase(dataType)
                && expectOptional("precision")) {
            dataType = "double precision";
        }

        final boolean timestamp = "timestamp".equalsIgnoreCase(dataType)
                || "time".equalsIgnoreCase(dataType);

        if (string.charAt(position) == '(') {
            dataType += getExpression();
        }

        if (timestamp) {
            if (expectOptional("with", "time", "zone")) {
                dataType += " with time zone";
            } else if (expectOptional("without", "time", "zone")) {
                dataType += " without time zone";
            }
        }

        if (expectOptional("[")) {
            expect("]");
            dataType += "[]";
        }

        return dataType;
    }

    
    public boolean isConsumed() {
        return position == string.length()
                || position + 1 == string.length()
                && string.charAt(position) == ';';
    }
}

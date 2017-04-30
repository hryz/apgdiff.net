
using System;
using pgdiff.Properties;

namespace pgdiff.parsers {






public class Parser {

    
    private string _string;
    
    private int position;

    
    public Parser(string _string) {
        this._string = _string;
        skipWhitespace();
    }

    
    public void expect(params string[] words) {
        foreach(string word in words) {
            expect(word, false);
        }
    }

    public bool expect(string word, bool optional) {
        int wordEnd = position + word.Length;

        if (wordEnd <= _string.Length
                && _string.Substring(position, wordEnd - position).Equals(word, StringComparison.InvariantCultureIgnoreCase)
                && (wordEnd == _string.Length
                || Char.IsWhiteSpace(_string[wordEnd])
                || _string[wordEnd] == ';'
                || _string[wordEnd] == ')'
                || _string[wordEnd] == ','
                || _string[wordEnd] == '['
                || "(".Equals(word) || ",".Equals(word) || "[".Equals(word)
                || "]".Equals(word))) {
            position = wordEnd;
            skipWhitespace();

            return true;
        }

        if (optional) {
            return false;
        }

        throw new ParserException(String.Format(
                Resources.CannotParseStringExpectedWord, _string,
                word, position + 1, _string.Substring(position, 20)));
    }

    
    public bool expectOptional(params string[] words) {
        bool found = expect(words[0], true);

        if (!found) {
            return false;
        }

        for (int i = 1; i < words.Length; i++) {
            skipWhitespace();
            expect(words[i]);
        }

        return true;
    }

    
    public void skipWhitespace() {
        for (; position < _string.Length; position++) {
            if (!Char.IsWhiteSpace(_string[position])) {
                break;
            }
        }
    }

    
    public string parseIdentifier() {
        string identifier = parseIdentifierInternal();

        if (_string[position] == '.') {
            position++;
            identifier += '.' + parseIdentifierInternal();
        }

        skipWhitespace();

        return identifier;
    }

    
    private string parseIdentifierInternal() {
        bool quoted = _string[position] == '"';

        if (quoted) {
            int endPos = _string.IndexOf('"', position + 1);
            string result = _string.Substring(position, endPos + 1 - position);
            position = endPos + 1;

            return result;
        } else {
            int endPos = position;

            for (; endPos < _string.Length; endPos++) {
                char chr = _string[endPos];

                if (Char.IsWhiteSpace(chr) || chr == ',' || chr == ')'
                        || chr == '(' || chr == ';' || chr == '.') {
                    break;
                }
            }

            string result =
                    _string.Substring(position, endPos - position).ToLower();

            position = endPos;

            return result;
        }
    }

    
    public string getRest() {
        string result;

        if (_string[_string.Length - 1] == ';') {
            if (position == _string.Length - 1) {
                return null;
            } else {
                result = _string.Substring(position, _string.Length - 1 - position);
            }
        } else {
            result = _string.Substring(position);
        }

        position = _string.Length;

        return result;
    }

    
    public int parseInteger() {
        int endPos = position;

        for (; endPos < _string.Length; endPos++) {
            if (!Char.IsLetterOrDigit(_string[endPos])) {
                break;
            }
        }

        try {
            int result =
                    Int32.Parse(_string.Substring(position, endPos - position));

            position = endPos;
            skipWhitespace();

            return result;
        } catch (FormatException ex) {
            throw new ParserException(String.Format(
                    Resources.CannotParseStringExpectedInteger,
                    _string, position + 1,
                    _string.Substring(position, 20)), ex);
        }
    }

    
    public string parseString() {
        bool quoted = _string[position] == '\'';

        if (quoted) {
            bool escape = false;
            int endPos = position + 1;

            for (; endPos < _string.Length; endPos++) {
                char chr = _string[endPos];

                if (chr == '\\') {
                    escape = !escape;
                } else if (!escape && chr == '\'') {
                    if (endPos + 1 < _string.Length
                            && _string[endPos + 1] == '\'') {
                        endPos++;
                    } else {
                        break;
                    }
                }
            }

            string result;

            try {
                result = _string.Substring(position, endPos + 1 - position);
            } catch (Exception ex) {
                throw new Exception("Failed to get sub_string: " + _string
                        + " start pos: " + position + " end pos: "
                        + (endPos + 1), ex);
            }

            position = endPos + 1;
            skipWhitespace();

            return result;
        } else {
            int endPos = position;

            for (; endPos < _string.Length; endPos++) {
                char chr = _string[endPos];

                if (Char.IsWhiteSpace(chr) || chr == ',' || chr == ')'
                        || chr == ';') {
                    break;
                }
            }

            if (position == endPos) {
                throw new ParserException(String.Format(
                        Resources.CannotParseStringExpectedString,
                        _string, position + 1));
            }

            string result = _string.Substring(position, endPos - position);

            position = endPos;
            skipWhitespace();

            return result;
        }
    }

    
    public string getExpression() {
        int endPos = getExpressionEnd();

        if (position == endPos) {
            throw new ParserException(String.Format(
                    Resources.CannotParseStringExpectedExpression,
                    _string, position + 1,
                    _string.Substring(position, 20)));
        }

        string result = _string.Substring(position, endPos - position).Trim();

        position = endPos;

        return result;
    }

    
    private int getExpressionEnd() {
        int bracesCount = 0;
        bool singleQuoteOn = false;
        int charPos = position;

        for (; charPos < _string.Length; charPos++) {
            char chr = _string[charPos];

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

    
    public string getString() {
        return _string;
    }

    
    public void throwUnsupportedCommand() {
        throw new ParserException(String.Format(
                Resources.CannotParseStringUnsupportedCommand,
                _string, position + 1,
                _string.Substring(position, 20)));
    }

  
    public string expectOptionalOneOf(params string[] words) {
        foreach(string word in words) {
            if (expectOptional(word)) {
                return word;
            }
        }

        return null;
    }

    
    public string getSub_string(int startPos, int endPos) {
        return _string.Substring(startPos, endPos - startPos);
    }

    
    public void setPosition(int position) {
        this.position = position;
    }

    
    public string parseDataType() {
        int endPos = position;

        while (endPos < _string.Length
                && !Char.IsWhiteSpace(_string[endPos])
                && _string[endPos] != '('
                && _string[endPos] != ')'
                && _string[endPos] != ',') {
            endPos++;
        }

        if (endPos == position) {
            throw new ParserException(String.Format(
                    Resources.CannotParseStringExpectedDataType,
                    _string, position + 1,
                    _string.Substring(position, 20)));
        }

        string dataType = _string.Substring(position, endPos - position);

        position = endPos;
        skipWhitespace();

        if ("character".Equals(dataType,StringComparison.InvariantCultureIgnoreCase)
                && expectOptional("varying")) {
            dataType = "character varying";
        } else if ("double".Equals(dataType,StringComparison.InvariantCultureIgnoreCase)
                && expectOptional("precision")) {
            dataType = "double precision";
        }

        bool timestamp = "timestamp".Equals(dataType,StringComparison.InvariantCultureIgnoreCase)
                || "time".Equals(dataType,StringComparison.InvariantCultureIgnoreCase);

        if (_string[position] == '(') {
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

    
    public bool isConsumed() {
        return position == _string.Length
                || position + 1 == _string.Length
                && _string[position] == ';';
    }
}
}
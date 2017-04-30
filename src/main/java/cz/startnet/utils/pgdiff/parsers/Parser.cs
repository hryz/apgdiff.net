using System;
using pgdiff.Properties;

namespace pgdiff.parsers
{
    public class Parser
    {
        private int _position;
        private readonly string _string;

        public Parser(string _string)
        {
            this._string = _string;
            SkipWhitespace();
        }
        
        public void Expect(params string[] words)
        {
            foreach (var word in words) Expect(word, false);
        }

        public bool Expect(string word, bool optional)
        {
            var wordEnd = _position + word.Length;

            if (wordEnd <= _string.Length
                && _string.Substring(_position, wordEnd - _position)
                    .Equals(word, StringComparison.InvariantCultureIgnoreCase)
                && (wordEnd == _string.Length
                    || char.IsWhiteSpace(_string[wordEnd])
                    || _string[wordEnd] == ';'
                    || _string[wordEnd] == ')'
                    || _string[wordEnd] == ','
                    || _string[wordEnd] == '['
                    || "(".Equals(word) || ",".Equals(word) || "[".Equals(word)
                    || "]".Equals(word)))
            {
                _position = wordEnd;
                SkipWhitespace();

                return true;
            }

            if (optional)
                return false;

            throw new ParserException(string.Format(Resources.CannotParseStringExpectedWord, 
                _string, word, _position + 1, _string.Substring(_position, 20)));
        }


        public bool ExpectOptional(params string[] words)
        {
            var found = Expect(words[0], true);

            if (!found)
                return false;

            for (var i = 1; i < words.Length; i++)
            {
                SkipWhitespace();
                Expect(words[i]);
            }

            return true;
        }


        public void SkipWhitespace()
        {
            for (; _position < _string.Length; _position++)
                if (!char.IsWhiteSpace(_string[_position]))
                    break;
        }


        public string ParseIdentifier()
        {
            var identifier = ParseIdentifierInternal();

            if (_string[_position] == '.')
            {
                _position++;
                identifier += '.' + ParseIdentifierInternal();
            }

            SkipWhitespace();

            return identifier;
        }


        private string ParseIdentifierInternal()
        {
            var quoted = _string[_position] == '"';

            if (quoted)
            {
                var endPos = _string.IndexOf('"', _position + 1);
                var result = _string.Substring(_position, endPos + 1 - _position);
                _position = endPos + 1;

                return result;
            }
            else
            {
                var endPos = _position;

                for (; endPos < _string.Length; endPos++)
                {
                    var chr = _string[endPos];
                    if (char.IsWhiteSpace(chr) 
                        || chr == ',' 
                        || chr == ')'
                        || chr == '(' 
                        || chr == ';' 
                        || chr == '.')
                        break;
                }

                var result = _string.Substring(_position, endPos - _position).ToLower();
                _position = endPos;

                return result;
            }
        }


        public string GetRest()
        {
            string result;

            if (_string[_string.Length - 1] == ';')
                if (_position == _string.Length - 1)
                    return null;
                else
                    result = _string.Substring(_position, _string.Length - 1 - _position);
            else
                result = _string.Substring(_position);

            _position = _string.Length;
            return result;
        }


        public int ParseInteger()
        {
            var endPos = _position;

            for (; endPos < _string.Length; endPos++)
                if (!char.IsLetterOrDigit(_string[endPos]))
                    break;

            try
            {
                var result = int.Parse(_string.Substring(_position, endPos - _position));

                _position = endPos;
                SkipWhitespace();

                return result;
            }
            catch (FormatException ex)
            {
                throw new ParserException(string.Format(Resources.CannotParseStringExpectedInteger,
                    _string, _position + 1, _string.Substring(_position, 20)), ex);
            }
        }


        public string ParseString()
        {
            var quoted = _string[_position] == '\'';

            if (quoted)
            {
                var escape = false;
                var endPos = _position + 1;

                for (; endPos < _string.Length; endPos++)
                {
                    var chr = _string[endPos];

                    if (chr == '\\')
                        escape = !escape;
                    else if (!escape && chr == '\'')
                        if (endPos + 1 < _string.Length && _string[endPos + 1] == '\'')
                            endPos++;
                        else break;
                }

                string result;

                try
                {
                    result = _string.Substring(_position, endPos + 1 - _position);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to get sub_string: {_string} start pos: {_position} end pos: {endPos + 1}", ex);
                }

                _position = endPos + 1;
                SkipWhitespace();

                return result;
            }
            else
            {
                var endPos = _position;

                for (; endPos < _string.Length; endPos++)
                {
                    var chr = _string[endPos];
                    if (char.IsWhiteSpace(chr) || chr == ',' || chr == ')'|| chr == ';')
                        break;
                }

                if (_position == endPos)
                    throw new ParserException(string.Format(Resources.CannotParseStringExpectedString, _string, _position + 1));

                var result = _string.Substring(_position, endPos - _position);

                _position = endPos;
                SkipWhitespace();

                return result;
            }
        }


        public string GetExpression()
        {
            var endPos = GetExpressionEnd();

            if (_position == endPos)
                throw new ParserException(string.Format(Resources.CannotParseStringExpectedExpression,
                    _string, _position + 1, _string.Substring(_position, 20)));

            var result = _string.Substring(_position, endPos - _position).Trim();
            _position = endPos;

            return result;
        }


        private int GetExpressionEnd()
        {
            var bracesCount = 0;
            var singleQuoteOn = false;
            var charPos = _position;

            for (; charPos < _string.Length; charPos++)
            {
                var chr = _string[charPos];

                if (chr == '(')
                    bracesCount++;
                else if (chr == ')')
                    if (bracesCount == 0)
                        break;
                    else bracesCount--;
                else if (chr == '\'')
                    singleQuoteOn = !singleQuoteOn;
                else if (chr == ',' && !singleQuoteOn && bracesCount == 0)
                    break;
                else if (chr == ';' && bracesCount == 0 && !singleQuoteOn)
                    break;
            }

            return charPos;
        }


        public int GetPosition()
        {
            return _position;
        }


        public string GetString()
        {
            return _string;
        }


        public void ThrowUnsupportedCommand()
        {
            throw new ParserException(string.Format(Resources.CannotParseStringUnsupportedCommand,
                _string, _position + 1,_string.Substring(_position, 20)));
        }


        public string ExpectOptionalOneOf(params string[] words)
        {
            foreach (var word in words)
                if (ExpectOptional(word))
                    return word;

            return null;
        }


        public string getSub_string(int startPos, int endPos)
        {
            return _string.Substring(startPos, endPos - startPos);
        }


        public void SetPosition(int position)
        {
            _position = position;
        }


        public string ParseDataType()
        {
            var endPos = _position;

            while (endPos < _string.Length
                   && !char.IsWhiteSpace(_string[endPos])
                   && _string[endPos] != '('
                   && _string[endPos] != ')'
                   && _string[endPos] != ',')
                endPos++;

            if (endPos == _position)
                throw new ParserException(string.Format(Resources.CannotParseStringExpectedDataType,
                    _string, _position + 1, _string.Substring(_position, 20)));

            var dataType = _string.Substring(_position, endPos - _position);

            _position = endPos;
            SkipWhitespace();

            if ("character".Equals(dataType, StringComparison.InvariantCultureIgnoreCase) && ExpectOptional("varying"))
                dataType = "character varying";
            else if ("double".Equals(dataType, StringComparison.InvariantCultureIgnoreCase) && ExpectOptional("precision"))
                dataType = "double precision";

            var timestamp = "timestamp".Equals(dataType, StringComparison.InvariantCultureIgnoreCase)
                            || "time".Equals(dataType, StringComparison.InvariantCultureIgnoreCase);

            if (_string[_position] == '(') dataType += GetExpression();

            if (timestamp)
                if (ExpectOptional("with", "time", "zone"))
                    dataType += " with time zone";
                else if (ExpectOptional("without", "time", "zone"))
                    dataType += " without time zone";

            if (ExpectOptional("["))
            {
                Expect("]");
                dataType += "[]";
            }

            return dataType;
        }


        public bool IsConsumed()
        {
            return _position == _string.Length
                   || _position + 1 == _string.Length
                   && _string[_position] == ';';
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema {







public class PgFunction {

    
    private String _name;
    
    
    private List<Argument> _arguments = new List<Argument>();
    
    private String _body;
    
    private String _comment;

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(500);
        sbSql.Append("CREATE OR REPLACE FUNCTION ");
        sbSql.Append(PgDiffUtils.GetQuotedName(_name));
        sbSql.Append('(');

        bool addComma = false;

        foreach(Argument argument in _arguments) {
            if (addComma) {
                sbSql.Append(", ");
            }

            sbSql.Append(argument.GetDeclaration(true));

            addComma = true;
        }

        sbSql.Append(") ");
        sbSql.Append(_body);
        sbSql.Append(';');

        if (_comment != null && ! String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON FUNCTION ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append('(');

            addComma = false;

            foreach(Argument argument in _arguments) {
                if (addComma) {
                    sbSql.Append(", ");
                }

                sbSql.Append(argument.GetDeclaration(false));

                addComma = true;
            }

            sbSql.Append(") IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        return sbSql.ToString();
    }

    
    public void SetBody(String body) {
        this._body = body;
    }

    
    public String GetBody() {
        return _body;
    }

    
    public String GetDropSql() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.Append("DROP FUNCTION ");
        sbString.Append(_name);
        sbString.Append('(');

        bool addComma = false;

        foreach(Argument argument in _arguments) {
            if ("OUT".Equals(argument.GetMode(),StringComparison.InvariantCultureIgnoreCase)) {
                continue;
            }

            if (addComma) {
                sbString.Append(", ");
            }

            sbString.Append(argument.GetDeclaration(false));

            addComma = true;
        }

        sbString.Append(");");

        return sbString.ToString();
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public List<Argument> GetArguments() {
        return new List<Argument>(_arguments);
    }

    
    public void AddArgument(Argument argument) {
        _arguments.Add(argument);
    }

    
    public String GetSignature() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.Append(_name);
        sbString.Append('(');

        bool addComma = false;

        foreach(Argument argument in _arguments) {
            if ("OUT".Equals(argument.GetMode(), StringComparison.InvariantCultureIgnoreCase)) {
                continue;
            }

            if (addComma) {
                sbString.Append(',');
            }

            sbString.Append(argument.GetDataType().ToLower());

            addComma = true;
        }

        sbString.Append(')');

        return sbString.ToString();
    }

    
    public override bool Equals(Object @object) {
        if (!(@object is PgFunction)) {
            return false;
        } else if (@object == this) {
            return true;
        }

        return Equals(@object, false);
    }

    
    public bool Equals(Object @object, bool ignoreFunctionWhitespace) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgFunction) {
            PgFunction function = (PgFunction) @object;

            if (_name == null && function.GetName() != null
                    || _name != null && !_name.Equals(function.GetName())) {
                return false;
            }

            String thisBody;
            String thatBody;

            if (ignoreFunctionWhitespace) {
                thisBody = Regex.Replace(_body, "\\s+", " ");
                thatBody = Regex.Replace(function.GetBody(), "\\s+", " ");
            } else {
                thisBody = _body;
                thatBody = function.GetBody();
            }

            if (thisBody == null && thatBody != null
                    || thisBody != null && !thisBody.Equals(thatBody)) {
                return false;
            }

            if (_arguments.Count != function.GetArguments().Count) {
                return false;
            } else {
                for (int i = 0; i < _arguments.Count; i++) {
                    if (!_arguments[i].Equals(function.GetArguments()[i])) {
                        return false;
                    }
                }
            }

            return true;
        }

        return equals;
    }

    
    public override int GetHashCode() {
        StringBuilder sbString = new StringBuilder(500);
        sbString.Append(_body);
        sbString.Append('|');
        sbString.Append(_name);

        foreach(Argument argument in _arguments) {
            sbString.Append('|');
            sbString.Append(argument.GetDeclaration(true));
        }

        return sbString.ToString().GetHashCode();
    }

    
    
    public class Argument {

        
        private String _mode = "IN";
        
        private String _name;
        
        private String _dataType;
        
        private String _defaultExpression;

        
        public String GetDataType() {
            return _dataType;
        }

        
        public void SetDataType(String dataType) {
            this._dataType = dataType;
        }

        
        public String GetDefaultExpression() {
            return _defaultExpression;
        }

        
        public void SetDefaultExpression(String defaultExpression) {
            this._defaultExpression = defaultExpression;
        }

        
        public String GetMode() {
            return _mode;
        }

        
        public void SetMode(String mode) {
            this._mode = String.IsNullOrEmpty(mode) ? "IN" : mode;
        }

        
        public String GetName() {
            return _name;
        }

        
        public void SetName(String name) {
            this._name = name;
        }

        
        public String GetDeclaration(bool includeDefaultValue) {
            StringBuilder sbString = new StringBuilder(50);

            if (_mode != null && !"IN".Equals(_mode, StringComparison.InvariantCultureIgnoreCase)) {
                sbString.Append(_mode);
                sbString.Append(' ');
            }

            if (_name != null && !String.IsNullOrEmpty(_name)) {
                sbString.Append(PgDiffUtils.GetQuotedName(_name));
                sbString.Append(' ');
            }

            sbString.Append(_dataType);

            if (includeDefaultValue && !String.IsNullOrEmpty(_defaultExpression)) {
                sbString.Append(" = ");
                sbString.Append(_defaultExpression);
            }

            return sbString.ToString();
        }

        public override bool Equals(Object obj) {
            if (!(obj is Argument)) {
                return false;
            } else if (this == obj) {
                return true;
            }

            Argument argument = (Argument) obj;

            return (_dataType == null ? argument.GetDataType() == null
                    : _dataType.Equals(argument.GetDataType(),StringComparison.InvariantCultureIgnoreCase))
                    && (_defaultExpression == null
                    ? argument.GetDefaultExpression() == null
                    : _defaultExpression.Equals(_defaultExpression))
                    && (_mode == null ? argument.GetMode() == null
                    : _mode.Equals(argument.GetMode(),StringComparison.InvariantCultureIgnoreCase))
                    && (_name == null ? argument.GetName() == null
                    : _name.Equals(argument.GetName()));
        }

        
        public override int GetHashCode() {
            StringBuilder sbString = new StringBuilder(50);
            sbString.Append(_mode == null ? null : _mode.ToUpper());
            sbString.Append('|');
            sbString.Append(_name);
            sbString.Append('|');
            sbString.Append(_dataType == null ? null: _dataType.ToUpper());
            sbString.Append('|');
            sbString.Append(_defaultExpression);

            return sbString.ToString().GetHashCode();
        }
    }
}
}
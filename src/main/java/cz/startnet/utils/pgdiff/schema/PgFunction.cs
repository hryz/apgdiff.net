using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema {







public class PgFunction {

    
    private String name;
    
    
    private List<Argument> arguments = new List<Argument>();
    
    private String body;
    
    private String comment;

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(500);
        sbSQL.Append("CREATE OR REPLACE FUNCTION ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));
        sbSQL.Append('(');

        bool addComma = false;

        foreach(Argument argument in arguments) {
            if (addComma) {
                sbSQL.Append(", ");
            }

            sbSQL.Append(argument.getDeclaration(true));

            addComma = true;
        }

        sbSQL.Append(") ");
        sbSQL.Append(body);
        sbSQL.Append(';');

        if (comment != null && ! String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON FUNCTION ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append('(');

            addComma = false;

            foreach(Argument argument in arguments) {
                if (addComma) {
                    sbSQL.Append(", ");
                }

                sbSQL.Append(argument.getDeclaration(false));

                addComma = true;
            }

            sbSQL.Append(") IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
    }

    
    public void setBody(String body) {
        this.body = body;
    }

    
    public String getBody() {
        return body;
    }

    
    public String getDropSQL() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.Append("DROP FUNCTION ");
        sbString.Append(name);
        sbString.Append('(');

        bool addComma = false;

        foreach(Argument argument in arguments) {
            if ("OUT".Equals(argument.getMode(),StringComparison.InvariantCultureIgnoreCase)) {
                continue;
            }

            if (addComma) {
                sbString.Append(", ");
            }

            sbString.Append(argument.getDeclaration(false));

            addComma = true;
        }

        sbString.Append(");");

        return sbString.ToString();
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public List<Argument> getArguments() {
        return new List<Argument>(arguments);
    }

    
    public void addArgument(Argument argument) {
        arguments.Add(argument);
    }

    
    public String getSignature() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.Append(name);
        sbString.Append('(');

        bool addComma = false;

        foreach(Argument argument in arguments) {
            if ("OUT".Equals(argument.getMode(), StringComparison.InvariantCultureIgnoreCase)) {
                continue;
            }

            if (addComma) {
                sbString.Append(',');
            }

            sbString.Append(argument.getDataType().ToLower());

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

            if (name == null && function.getName() != null
                    || name != null && !name.Equals(function.getName())) {
                return false;
            }

            String thisBody;
            String thatBody;

            if (ignoreFunctionWhitespace) {
                thisBody = Regex.Replace(body, "\\s+", " ");
                thatBody = Regex.Replace(function.getBody(), "\\s+", " ");
            } else {
                thisBody = body;
                thatBody = function.getBody();
            }

            if (thisBody == null && thatBody != null
                    || thisBody != null && !thisBody.Equals(thatBody)) {
                return false;
            }

            if (arguments.Count != function.getArguments().Count) {
                return false;
            } else {
                for (int i = 0; i < arguments.Count; i++) {
                    if (!arguments[i].Equals(function.getArguments()[i])) {
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
        sbString.Append(body);
        sbString.Append('|');
        sbString.Append(name);

        foreach(Argument argument in arguments) {
            sbString.Append('|');
            sbString.Append(argument.getDeclaration(true));
        }

        return sbString.ToString().GetHashCode();
    }

    
    
    public class Argument {

        
        private String mode = "IN";
        
        private String name;
        
        private String dataType;
        
        private String defaultExpression;

        
        public String getDataType() {
            return dataType;
        }

        
        public void setDataType(String dataType) {
            this.dataType = dataType;
        }

        
        public String getDefaultExpression() {
            return defaultExpression;
        }

        
        public void setDefaultExpression(String defaultExpression) {
            this.defaultExpression = defaultExpression;
        }

        
        public String getMode() {
            return mode;
        }

        
        public void setMode(String mode) {
            this.mode = String.IsNullOrEmpty(mode) ? "IN" : mode;
        }

        
        public String getName() {
            return name;
        }

        
        public void setName(String name) {
            this.name = name;
        }

        
        public String getDeclaration(bool includeDefaultValue) {
            StringBuilder sbString = new StringBuilder(50);

            if (mode != null && !"IN".Equals(mode, StringComparison.InvariantCultureIgnoreCase)) {
                sbString.Append(mode);
                sbString.Append(' ');
            }

            if (name != null && !String.IsNullOrEmpty(name)) {
                sbString.Append(PgDiffUtils.getQuotedName(name));
                sbString.Append(' ');
            }

            sbString.Append(dataType);

            if (includeDefaultValue && !String.IsNullOrEmpty(defaultExpression)) {
                sbString.Append(" = ");
                sbString.Append(defaultExpression);
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

            return (dataType == null ? argument.getDataType() == null
                    : dataType.Equals(argument.getDataType(),StringComparison.InvariantCultureIgnoreCase))
                    && (defaultExpression == null
                    ? argument.getDefaultExpression() == null
                    : defaultExpression.Equals(defaultExpression))
                    && (mode == null ? argument.getMode() == null
                    : mode.Equals(argument.getMode(),StringComparison.InvariantCultureIgnoreCase))
                    && (name == null ? argument.getName() == null
                    : name.Equals(argument.getName()));
        }

        
        public override int GetHashCode() {
            StringBuilder sbString = new StringBuilder(50);
            sbString.Append(mode == null ? null : mode.ToUpper());
            sbString.Append('|');
            sbString.Append(name);
            sbString.Append('|');
            sbString.Append(dataType == null ? null: dataType.ToUpper());
            sbString.Append('|');
            sbString.Append(defaultExpression);

            return sbString.ToString().GetHashCode();
        }
    }
}
}
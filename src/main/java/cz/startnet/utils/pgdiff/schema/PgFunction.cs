
namespace cz.startnet.utils.pgdiff.schema {

using cz.startnet.utils.pgdiff.PgDiffUtils;






public class PgFunction {

    
    private String name;
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<Argument> arguments = new ArrayList<Argument>();
    
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
        sbSQL.append("CREATE OR REPLACE FUNCTION ");
        sbSQL.append(PgDiffUtils.getQuotedName(name));
        sbSQL.append('(');

        boolean addComma = false;

        for (Argument argument : arguments) {
            if (addComma) {
                sbSQL.append(", ");
            }

            sbSQL.append(argument.getDeclaration(true));

            addComma = true;
        }

        sbSQL.append(") ");
        sbSQL.append(body);
        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON FUNCTION ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append('(');

            addComma = false;

            for (Argument argument : arguments) {
                if (addComma) {
                    sbSQL.append(", ");
                }

                sbSQL.append(argument.getDeclaration(false));

                addComma = true;
            }

            sbSQL.append(") IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
    }

    
    public void setBody(String body) {
        this.body = body;
    }

    
    public String getBody() {
        return body;
    }

    
    public String getDropSQL() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.append("DROP FUNCTION ");
        sbString.append(name);
        sbString.append('(');

        boolean addComma = false;

        for (Argument argument : arguments) {
            if ("OUT".equalsIgnoreCase(argument.getMode())) {
                continue;
            }

            if (addComma) {
                sbString.append(", ");
            }

            sbString.append(argument.getDeclaration(false));

            addComma = true;
        }

        sbString.append(");");

        return sbString.toString();
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public List<Argument> getArguments() {
        return Collections.unmodifiableList(arguments);
    }

    
    public void addArgument(Argument argument) {
        arguments.add(argument);
    }

    
    public String getSignature() {
        StringBuilder sbString = new StringBuilder(100);
        sbString.append(name);
        sbString.append('(');

        boolean addComma = false;

        for (Argument argument : arguments) {
            if ("OUT".equalsIgnoreCase(argument.getMode())) {
                continue;
            }

            if (addComma) {
                sbString.append(',');
            }

            sbString.append(argument.getDataType().toLowerCase(Locale.ENGLISH));

            addComma = true;
        }

        sbString.append(')');

        return sbString.toString();
    }

    @Override
    public boolean equals(Object object) {
        if (!(object instanceof PgFunction)) {
            return false;
        } else if (object == this) {
            return true;
        }

        return equals(object, false);
    }

    
    public boolean equals(Object object,
            boolean ignoreFunctionWhitespace) {
        boolean equals = false;

        if (this == object) {
            equals = true;
        } else if (object instanceof PgFunction) {
            PgFunction function = (PgFunction) object;

            if (name == null && function.getName() != null
                    || name != null && !name.equals(function.getName())) {
                return false;
            }

            String thisBody;
            String thatBody;

            if (ignoreFunctionWhitespace) {
                thisBody = body.replaceAll("\\s+", " ");
                thatBody =
                        function.getBody().replaceAll("\\s+", " ");
            } else {
                thisBody = body;
                thatBody = function.getBody();
            }

            if (thisBody == null && thatBody != null
                    || thisBody != null && !thisBody.equals(thatBody)) {
                return false;
            }

            if (arguments.size() != function.getArguments().size()) {
                return false;
            } else {
                for (int i = 0; i < arguments.size(); i++) {
                    if (!arguments.get(i).equals(function.getArguments().get(i))) {
                        return false;
                    }
                }
            }

            return true;
        }

        return equals;
    }

    @Override
    public int hashCode() {
        StringBuilder sbString = new StringBuilder(500);
        sbString.append(body);
        sbString.append('|');
        sbString.append(name);

        for (Argument argument : arguments) {
            sbString.append('|');
            sbString.append(argument.getDeclaration(true));
        }

        return sbString.toString().hashCode();
    }

    
    @SuppressWarnings("PublicInnerClass")
    public static class Argument {

        
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
            this.mode = mode == null || mode.isEmpty() ? "IN" : mode;
        }

        
        public String getName() {
            return name;
        }

        
        public void setName(String name) {
            this.name = name;
        }

        
        public String getDeclaration(boolean includeDefaultValue) {
            StringBuilder sbString = new StringBuilder(50);

            if (mode != null && !"IN".equalsIgnoreCase(mode)) {
                sbString.append(mode);
                sbString.append(' ');
            }

            if (name != null && !name.isEmpty()) {
                sbString.append(PgDiffUtils.getQuotedName(name));
                sbString.append(' ');
            }

            sbString.append(dataType);

            if (includeDefaultValue && defaultExpression != null
                    && !defaultExpression.isEmpty()) {
                sbString.append(" = ");
                sbString.append(defaultExpression);
            }

            return sbString.toString();
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Argument)) {
                return false;
            } else if (this == obj) {
                return true;
            }

            Argument argument = (Argument) obj;

            return (dataType == null ? argument.getDataType() == null
                    : dataType.equalsIgnoreCase(argument.getDataType()))
                    && (defaultExpression == null
                    ? argument.getDefaultExpression() == null
                    : defaultExpression.equals(defaultExpression))
                    && (mode == null ? argument.getMode() == null
                    : mode.equalsIgnoreCase(argument.getMode()))
                    && (name == null ? argument.getName() == null
                    : name.equals(argument.getName()));
        }

        @Override
        public int hashCode() {
            StringBuilder sbString = new StringBuilder(50);
            sbString.append(
                    mode == null ? null : mode.toUpperCase(Locale.ENGLISH));
            sbString.append('|');
            sbString.append(name);
            sbString.append('|');
            sbString.append(dataType == null ? null
                    : dataType.toUpperCase(Locale.ENGLISH));
            sbString.append('|');
            sbString.append(defaultExpression);

            return sbString.toString().hashCode();
        }
    }
}
}
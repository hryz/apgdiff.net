
package cz.startnet.utils.pgdiff.schema;

import cz.startnet.utils.pgdiff.PgDiffUtils;
import java.util.regex.Matcher;
import java.util.regex.Pattern;


public class PgColumn {

    
    private static final Pattern PATTERN_NULL =
            Pattern.compile("^(.+)[\\s]+NULL$", Pattern.CASE_INSENSITIVE);
    
    private static final Pattern PATTERN_NOT_NULL = Pattern.compile(
            "^(.+)[\\s]+NOT[\\s]+NULL$", Pattern.CASE_INSENSITIVE);
    
    private static final Pattern PATTERN_DEFAULT = Pattern.compile(
            "^(.+)[\\s]+DEFAULT[\\s]+(.+)$", Pattern.CASE_INSENSITIVE);
    
    private Integer statistics;
    
    private String defaultValue;
    
    private String name;
    
    private String type;
    
    private boolean nullValue = true;
    
    private String storage;
    
    private String comment;

    
    public PgColumn(final String name) {
        this.name = name;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(final String comment) {
        this.comment = comment;
    }

    
    public void setDefaultValue(final String defaultValue) {
        this.defaultValue = defaultValue;
    }

    
    public String getDefaultValue() {
        return defaultValue;
    }

    
    public String getFullDefinition(final boolean addDefaults) {
        final StringBuilder sbDefinition = new StringBuilder(100);
        sbDefinition.append(PgDiffUtils.getQuotedName(name));
        sbDefinition.append(' ');
        sbDefinition.append(type);

        if (defaultValue != null && !defaultValue.isEmpty()) {
            sbDefinition.append(" DEFAULT ");
            sbDefinition.append(defaultValue);
        } else if (!nullValue && addDefaults) {
            final String defaultColValue = PgColumnUtils.getDefaultValue(type);

            if (defaultColValue != null) {
                sbDefinition.append(" DEFAULT ");
                sbDefinition.append(defaultColValue);
            }
        }

        if (!nullValue) {
            sbDefinition.append(" NOT NULL");
        }

        return sbDefinition.toString();
    }

    
    public void setName(final String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setNullValue(final boolean nullValue) {
        this.nullValue = nullValue;
    }

    
    public boolean getNullValue() {
        return nullValue;
    }

    
    public void setStatistics(final Integer statistics) {
        this.statistics = statistics;
    }

    
    public Integer getStatistics() {
        return statistics;
    }

    
    public String getStorage() {
        return storage;
    }

    
    public void setStorage(final String storage) {
        this.storage = storage;
    }

    
    public void setType(final String type) {
        this.type = type;
    }

    
    public String getType() {
        return type;
    }

    
    public void parseDefinition(final String definition) {
        String string = definition;

        Matcher matcher = PATTERN_NOT_NULL.matcher(string);

        if (matcher.matches()) {
            string = matcher.group(1).trim();
            setNullValue(false);
        } else {
            matcher = PATTERN_NULL.matcher(string);

            if (matcher.matches()) {
                string = matcher.group(1).trim();
                setNullValue(true);
            }
        }

        matcher = PATTERN_DEFAULT.matcher(string);

        if (matcher.matches()) {
            string = matcher.group(1).trim();
            setDefaultValue(matcher.group(2).trim());
        }

        setType(string);
    }
}

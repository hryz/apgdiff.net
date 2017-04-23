
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace cz.startnet.utils.pgdiff.schema {





public class PgColumn {

    
    private static Regex PATTERN_NULL = new Regex("^(.+)[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
    
    private static Regex PATTERN_NOT_NULL = new Regex("^(.+)[\\s]+NOT[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex PATTERN_DEFAULT = new Regex("^(.+)[\\s]+DEFAULT[\\s]+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private Int32 statistics;
    
    private String defaultValue;
    
    private String name;
    
    private String type;
    
    private bool nullValue = true;
    
    private String storage;
    
    private String comment;

    
    public PgColumn(String name) {
        this.name = name;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public void setDefaultValue(String defaultValue) {
        this.defaultValue = defaultValue;
    }

    
    public String getDefaultValue() {
        return defaultValue;
    }

    
    public String getFullDefinition(bool addDefaults) {
        StringBuilder sbDefinition = new StringBuilder(100);
        sbDefinition.Append(PgDiffUtils.getQuotedName(name));
        sbDefinition.Append(' ');
        sbDefinition.Append(type);

        if (defaultValue != null && !String.IsNullOrEmpty(defaultValue)) {
            sbDefinition.Append(" DEFAULT ");
            sbDefinition.Append(defaultValue);
        } else if (!nullValue && addDefaults) {
            String defaultColValue = PgColumnUtils.getDefaultValue(type);

            if (defaultColValue != null) {
                sbDefinition.Append(" DEFAULT ");
                sbDefinition.Append(defaultColValue);
            }
        }

        if (!nullValue) {
            sbDefinition.Append(" NOT NULL");
        }

        return sbDefinition.ToString();
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setNullValue(bool nullValue) {
        this.nullValue = nullValue;
    }

    
    public bool getNullValue() {
        return nullValue;
    }

    
    public void setStatistics(Int32 statistics) {
        this.statistics = statistics;
    }

    
    public Int32 getStatistics() {
        return statistics;
    }

    
    public String getStorage() {
        return storage;
    }

    
    public void setStorage(String storage) {
        this.storage = storage;
    }

    
    public void setType(String type) {
        this.type = type;
    }

    
    public String getType() {
        return type;
    }

    
    public void parseDefinition(String definition) {
        String @string = definition;

        Regex matcher = PATTERN_NOT_NULL;

        if (matcher.IsMatch(@string)) {
            @string = matcher.Matches(@string)[0].Groups[1].Value.Trim();
            setNullValue(false);
        } else {
            matcher = PATTERN_NULL;

            if (matcher.IsMatch(@string)) {
                @string = matcher.Matches(@string)[0].Groups[1].Value.Trim();
                    setNullValue(true);
            }
        }

        matcher = PATTERN_DEFAULT;

        if (matcher.IsMatch(@string)) {
            @string = matcher.Matches(@string)[0].Groups[1].Value.Trim(); 
            setDefaultValue(matcher.Matches(@string)[0].Groups[2].Value.Trim());
        }

        setType(@string);
    }
}
}
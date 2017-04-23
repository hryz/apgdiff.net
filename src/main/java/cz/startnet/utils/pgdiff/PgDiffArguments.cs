
package cz.startnet.utils.pgdiff;

import java.io.PrintWriter;
import java.nio.charset.Charset;
import java.util.SortedMap;


public class PgDiffArguments {

    
    private String inCharsetName = "UTF-8";
    
    private String newDumpFile;
    
    private String oldDumpFile;
    
    private String outCharsetName = "UTF-8";
    
    private boolean addDefaults;
    
    private boolean addTransaction;
    
    private boolean ignoreFunctionWhitespace;
    
    private boolean ignoreStartWith;
    
    private boolean version;
    
    private boolean outputIgnoredStatements;
    
    private boolean listCharsets;
    
    private boolean ignoreSlonyTriggers;

    
    public void setAddDefaults(final boolean addDefaults) {
        this.addDefaults = addDefaults;
    }

    
    public boolean isAddDefaults() {
        return addDefaults;
    }

    
    public void setAddTransaction(final boolean addTransaction) {
        this.addTransaction = addTransaction;
    }

    
    public boolean isAddTransaction() {
        return addTransaction;
    }

    
    public void setIgnoreFunctionWhitespace(
            final boolean ignoreFunctionWhitespace) {
        this.ignoreFunctionWhitespace = ignoreFunctionWhitespace;
    }

    
    public boolean isIgnoreFunctionWhitespace() {
        return ignoreFunctionWhitespace;
    }

    
    public void setIgnoreStartWith(final boolean ignoreStartWith) {
        this.ignoreStartWith = ignoreStartWith;
    }

    
    public boolean isIgnoreStartWith() {
        return ignoreStartWith;
    }

    
    public void setNewDumpFile(final String newDumpFile) {
        this.newDumpFile = newDumpFile;
    }

    
    public String getNewDumpFile() {
        return newDumpFile;
    }

    
    public void setOldDumpFile(final String oldDumpFile) {
        this.oldDumpFile = oldDumpFile;
    }

    
    public String getOldDumpFile() {
        return oldDumpFile;
    }

    
    public boolean isOutputIgnoredStatements() {
        return outputIgnoredStatements;
    }

    
    public void setOutputIgnoredStatements(
            final boolean outputIgnoredStatements) {
        this.outputIgnoredStatements = outputIgnoredStatements;
    }

    
    public void setVersion(final boolean version) {
        this.version = version;
    }

    
    public boolean isVersion() {
        return version;
    }

    
    @SuppressWarnings("AssignmentToForLoopParameter")
    public boolean parse(final PrintWriter writer, final String[] args) {
        boolean success = true;
        final int argsLength;

        if (args.length >= 2) {
            argsLength = args.length - 2;
        } else {
            argsLength = args.length;
        }

        for (int i = 0; i < argsLength; i++) {
            if ("--add-defaults".equals(args[i])) {
                setAddDefaults(true);
            } else if ("--add-transaction".equals(args[i])) {
                setAddTransaction(true);
            } else if ("--ignore-function-whitespace".equals(args[i])) {
                setIgnoreFunctionWhitespace(true);
            } else if ("--ignore-slony-triggers".equals(args[i])) {
                setIgnoreSlonyTriggers(true);
            } else if ("--ignore-start-with".equals(args[i])) {
                setIgnoreStartWith(true);
            } else if ("--in-charset-name".equals(args[i])) {
                setInCharsetName(args[i + 1]);
                i++;
            } else if ("--list-charsets".equals(args[i])) {
                setListCharsets(true);
            } else if ("--out-charset-name".equals(args[i])) {
                setOutCharsetName(args[i + 1]);
                i++;
            } else if ("--output-ignored-statements".equals(args[i])) {
                setOutputIgnoredStatements(true);
            } else if ("--version".equals(args[i])) {
                setVersion(true);
            } else {
                writer.print(Resources.getString("ErrorUnknownOption"));
                writer.print(": ");
                writer.println(args[i]);
                success = false;

                break;
            }
        }

        if (args.length == 1 && isVersion()) {
            printVersion(writer);
            success = false;
        } else if (args.length == 1 && isListCharsets()) {
            listCharsets(writer);
            success = false;
        } else if (args.length < 2) {
            printUsage(writer);
            success = false;
        } else if (success) {
            setOldDumpFile(args[args.length - 2]);
            setNewDumpFile(args[args.length - 1]);
        }

        return success;
    }

    
    private void printUsage(final PrintWriter writer) {
        writer.println(
                Resources.getString("UsageHelp").replace("${tab}", "\t"));
    }

    
    private void printVersion(final PrintWriter writer) {
        writer.print(Resources.getString("Version"));
        writer.print(": ");
        writer.println(Resources.getString("VersionNumber"));
    }

    
    public String getInCharsetName() {
        return inCharsetName;
    }

    
    public void setInCharsetName(final String inCharsetName) {
        this.inCharsetName = inCharsetName;
    }

    
    public String getOutCharsetName() {
        return outCharsetName;
    }

    
    public void setOutCharsetName(final String outCharsetName) {
        this.outCharsetName = outCharsetName;
    }

    
    public boolean isListCharsets() {
        return listCharsets;
    }

    
    public void setListCharsets(final boolean listCharsets) {
        this.listCharsets = listCharsets;
    }

    
    private void listCharsets(final PrintWriter writer) {
        final SortedMap<String, Charset> charsets = Charset.availableCharsets();

        for (final String name : charsets.keySet()) {
            writer.println(name);
        }
    }

    
    public boolean isIgnoreSlonyTriggers() {
        return ignoreSlonyTriggers;
    }

    
    public void setIgnoreSlonyTriggers(final boolean ignoreSlonyTriggers) {
        this.ignoreSlonyTriggers = ignoreSlonyTriggers;
    }
}

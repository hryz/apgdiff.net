using System;
using System.IO;
using System.Text;
using pgdiff.Properties;

namespace pgdiff {






public class PgDiffArguments {

    
    private String inCharsetName = "UTF-8";
    
    private String newDumpFile;
    
    private String oldDumpFile;
    
    private String outCharsetName = "UTF-8";
    
    private bool addDefaults;
    
    private bool addTransaction;
    
    private bool ignoreFunctionWhitespace;
    
    private bool ignoreStartWith;
    
    private bool version;
    
    private bool outputIgnoredStatements;
    
    private bool listCharsets;
    
    private bool ignoreSlonyTriggers;

    
    public void setAddDefaults(bool addDefaults) {
        this.addDefaults = addDefaults;
    }

    
    public bool isAddDefaults() {
        return addDefaults;
    }

    
    public void setAddTransaction(bool addTransaction) {
        this.addTransaction = addTransaction;
    }

    
    public bool isAddTransaction() {
        return addTransaction;
    }

    
    public void setIgnoreFunctionWhitespace(
            bool ignoreFunctionWhitespace) {
        this.ignoreFunctionWhitespace = ignoreFunctionWhitespace;
    }

    
    public bool isIgnoreFunctionWhitespace() {
        return ignoreFunctionWhitespace;
    }

    
    public void setIgnoreStartWith(bool ignoreStartWith) {
        this.ignoreStartWith = ignoreStartWith;
    }

    
    public bool isIgnoreStartWith() {
        return ignoreStartWith;
    }

    
    public void setNewDumpFile(String newDumpFile) {
        this.newDumpFile = newDumpFile;
    }

    
    public String getNewDumpFile() {
        return newDumpFile;
    }

    
    public void setOldDumpFile(String oldDumpFile) {
        this.oldDumpFile = oldDumpFile;
    }

    
    public String getOldDumpFile() {
        return oldDumpFile;
    }

    
    public bool isOutputIgnoredStatements() {
        return outputIgnoredStatements;
    }

    
    public void setOutputIgnoredStatements(
            bool outputIgnoredStatements) {
        this.outputIgnoredStatements = outputIgnoredStatements;
    }

    
    public void setVersion(bool version) {
        this.version = version;
    }

    
    public bool isVersion() {
        return version;
    }

    
    
    public bool parse(TextReader reader, TextWriter writer, String[] args) {
        bool success = true;
        int argsLength;

        if (args.Length >= 2) {
            argsLength = args.Length - 2;
        } else {
            argsLength = args.Length;
        }

        for (int i = 0; i < argsLength; i++) {
            if ("--add-defaults".Equals(args[i])) {
                setAddDefaults(true);
            } else if ("--add-transaction".Equals(args[i])) {
                setAddTransaction(true);
            } else if ("--ignore-function-whitespace".Equals(args[i])) {
                setIgnoreFunctionWhitespace(true);
            } else if ("--ignore-slony-triggers".Equals(args[i])) {
                setIgnoreSlonyTriggers(true);
            } else if ("--ignore-start-with".Equals(args[i])) {
                setIgnoreStartWith(true);
            } else if ("--in-charset-name".Equals(args[i])) {
                setInCharsetName(args[i + 1]);
                i++;
            } else if ("--list-charsets".Equals(args[i])) {
                setListCharsets(true);
            } else if ("--out-charset-name".Equals(args[i])) {
                setOutCharsetName(args[i + 1]);
                i++;
            } else if ("--output-ignored-statements".Equals(args[i])) {
                setOutputIgnoredStatements(true);
            } else if ("--version".Equals(args[i])) {
                setVersion(true);
            } else {
                writer.Write(Resources.ErrorUnknownOption);
                writer.Write(": ");
                writer.WriteLine(args[i]);
                success = false;

                break;
            }
        }

        if (args.Length == 1 && isVersion()) {
            printVersion(writer);
            success = false;
        } else if (args.Length == 1 && isListCharsets()) {
            ListCharsets(writer);
            success = false;
        } else if (args.Length < 2) {
            printUsage(writer);
            success = false;
        } else if (success) {
            setOldDumpFile(args[args.Length - 2]);
            setNewDumpFile(args[args.Length - 1]);
        }

        return success;
    }

    
    private void printUsage(TextWriter writer) {
        writer.WriteLine(Resources.UsageHelp);
    }

    
    private void printVersion(TextWriter writer) {
        writer.Write(Resources.Version);
        writer.Write(": ");
        writer.WriteLine(Resources.VersionNumber);
    }

    
    public String getInCharsetName() {
        return inCharsetName;
    }

    
    public void setInCharsetName(String inCharsetName) {
        this.inCharsetName = inCharsetName;
    }

    
    public String getOutCharsetName() {
        return outCharsetName;
    }

    
    public void setOutCharsetName(String outCharsetName) {
        this.outCharsetName = outCharsetName;
    }

    
    public bool isListCharsets() {
        return listCharsets;
    }

    
    public void setListCharsets(bool listCharsets) {
        this.listCharsets = listCharsets;
    }

    
    private void ListCharsets(TextWriter writer) {
        var charsets = Encoding.GetEncodings();

        foreach (var name in charsets) {
            writer.WriteLine(name.Name);
        }
    }

    
    public bool isIgnoreSlonyTriggers() {
        return ignoreSlonyTriggers;
    }

    
    public void setIgnoreSlonyTriggers(bool ignoreSlonyTriggers) {
        this.ignoreSlonyTriggers = ignoreSlonyTriggers;
    }
}
}
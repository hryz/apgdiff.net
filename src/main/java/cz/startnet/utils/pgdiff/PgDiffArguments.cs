using System;
using System.IO;
using System.Text;
using pgdiff.Properties;

namespace pgdiff {






public class PgDiffArguments {

    
    private String _inCharsetName = "UTF-8";
    
    private String _newDumpFile;
    
    private String _oldDumpFile;
    
    private String _outCharsetName = "UTF-8";
    
    private bool _addDefaults;
    
    private bool _addTransaction;
    
    private bool _ignoreFunctionWhitespace;
    
    private bool _ignoreStartWith;
    
    private bool _version;
    
    private bool _outputIgnoredStatements;
    
    private bool _listCharsets;
    
    private bool _ignoreSlonyTriggers;

    
    public void SetAddDefaults(bool addDefaults) {
        this._addDefaults = addDefaults;
    }

    
    public bool IsAddDefaults() {
        return _addDefaults;
    }

    
    public void SetAddTransaction(bool addTransaction) {
        this._addTransaction = addTransaction;
    }

    
    public bool IsAddTransaction() {
        return _addTransaction;
    }

    
    public void SetIgnoreFunctionWhitespace(
            bool ignoreFunctionWhitespace) {
        this._ignoreFunctionWhitespace = ignoreFunctionWhitespace;
    }

    
    public bool IsIgnoreFunctionWhitespace() {
        return _ignoreFunctionWhitespace;
    }

    
    public void SetIgnoreStartWith(bool ignoreStartWith) {
        this._ignoreStartWith = ignoreStartWith;
    }

    
    public bool IsIgnoreStartWith() {
        return _ignoreStartWith;
    }

    
    public void SetNewDumpFile(String newDumpFile) {
        this._newDumpFile = newDumpFile;
    }

    
    public String GetNewDumpFile() {
        return _newDumpFile;
    }

    
    public void SetOldDumpFile(String oldDumpFile) {
        this._oldDumpFile = oldDumpFile;
    }

    
    public String GetOldDumpFile() {
        return _oldDumpFile;
    }

    
    public bool IsOutputIgnoredStatements() {
        return _outputIgnoredStatements;
    }

    
    public void SetOutputIgnoredStatements(
            bool outputIgnoredStatements) {
        this._outputIgnoredStatements = outputIgnoredStatements;
    }

    
    public void SetVersion(bool version) {
        this._version = version;
    }

    
    public bool IsVersion() {
        return _version;
    }

    
    
    public bool Parse(TextReader reader, TextWriter writer, String[] args) {
        bool success = true;
        int argsLength;

        if (args.Length >= 2) {
            argsLength = args.Length - 2;
        } else {
            argsLength = args.Length;
        }

        for (int i = 0; i < argsLength; i++) {
            if ("--add-defaults".Equals(args[i])) {
                SetAddDefaults(true);
            } else if ("--add-transaction".Equals(args[i])) {
                SetAddTransaction(true);
            } else if ("--ignore-function-whitespace".Equals(args[i])) {
                SetIgnoreFunctionWhitespace(true);
            } else if ("--ignore-slony-triggers".Equals(args[i])) {
                SetIgnoreSlonyTriggers(true);
            } else if ("--ignore-start-with".Equals(args[i])) {
                SetIgnoreStartWith(true);
            } else if ("--in-charset-name".Equals(args[i])) {
                SetInCharsetName(args[i + 1]);
                i++;
            } else if ("--list-charsets".Equals(args[i])) {
                SetListCharsets(true);
            } else if ("--out-charset-name".Equals(args[i])) {
                SetOutCharsetName(args[i + 1]);
                i++;
            } else if ("--output-ignored-statements".Equals(args[i])) {
                SetOutputIgnoredStatements(true);
            } else if ("--version".Equals(args[i])) {
                SetVersion(true);
            } else {
                writer.Write(Resources.ErrorUnknownOption);
                writer.Write(": ");
                writer.WriteLine(args[i]);
                success = false;

                break;
            }
        }

        if (args.Length == 1 && IsVersion()) {
            PrintVersion(writer);
            success = false;
        } else if (args.Length == 1 && IsListCharsets()) {
            ListCharsets(writer);
            success = false;
        } else if (args.Length < 2) {
            PrintUsage(writer);
            success = false;
        } else if (success) {
            SetOldDumpFile(args[args.Length - 2]);
            SetNewDumpFile(args[args.Length - 1]);
        }

        return success;
    }

    
    private void PrintUsage(TextWriter writer) {
        writer.WriteLine(Resources.UsageHelp);
    }

    
    private void PrintVersion(TextWriter writer) {
        writer.Write(Resources.Version);
        writer.Write(": ");
        writer.WriteLine(Resources.VersionNumber);
    }

    
    public String GetInCharsetName() {
        return _inCharsetName;
    }

    
    public void SetInCharsetName(String inCharsetName) {
        this._inCharsetName = inCharsetName;
    }

    
    public String GetOutCharsetName() {
        return _outCharsetName;
    }

    
    public void SetOutCharsetName(String outCharsetName) {
        this._outCharsetName = outCharsetName;
    }

    
    public bool IsListCharsets() {
        return _listCharsets;
    }

    
    public void SetListCharsets(bool listCharsets) {
        this._listCharsets = listCharsets;
    }

    
    private void ListCharsets(TextWriter writer) {
        var charsets = Encoding.GetEncodings();

        foreach (var name in charsets) {
            writer.WriteLine(name.Name);
        }
    }

    
    public bool IsIgnoreSlonyTriggers() {
        return _ignoreSlonyTriggers;
    }

    
    public void SetIgnoreSlonyTriggers(bool ignoreSlonyTriggers) {
        this._ignoreSlonyTriggers = ignoreSlonyTriggers;
    }
}
}
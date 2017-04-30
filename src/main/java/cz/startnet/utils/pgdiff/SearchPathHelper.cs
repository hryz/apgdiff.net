using System;
using System.IO;

namespace pgdiff {




public class SearchPathHelper {

    
    private String _searchPath;
    
    private bool _wasOutput;

    
    public SearchPathHelper(String searchPath) {
        this._searchPath = searchPath;
    }

    
    public void OutputSearchPath(TextWriter writer) {
        if (!_wasOutput && _searchPath != null && ! String.IsNullOrEmpty(_searchPath)) {
            writer.WriteLine();
            writer.WriteLine(_searchPath);
            _wasOutput = true;
        }
    }
}
}
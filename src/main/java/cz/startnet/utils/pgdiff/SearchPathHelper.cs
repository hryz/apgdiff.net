
using System;
using System.IO;

namespace cz.startnet.utils.pgdiff {




public class SearchPathHelper {

    
    private String searchPath;
    
    private bool wasOutput;

    
    public SearchPathHelper(String searchPath) {
        this.searchPath = searchPath;
    }

    
    public void outputSearchPath(TextWriter writer) {
        if (!wasOutput && searchPath != null && ! String.IsNullOrEmpty(searchPath)) {
            writer.WriteLine();
            writer.WriteLine(searchPath);
            wasOutput = true;
        }
    }
}
}
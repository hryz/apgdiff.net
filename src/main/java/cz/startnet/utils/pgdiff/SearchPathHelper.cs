
package cz.startnet.utils.pgdiff;

import java.io.PrintWriter;


public class SearchPathHelper {

    
    private final String searchPath;
    
    private boolean wasOutput;

    
    public SearchPathHelper(final String searchPath) {
        this.searchPath = searchPath;
    }

    
    public void outputSearchPath(final PrintWriter writer) {
        if (!wasOutput && searchPath != null && !searchPath.isEmpty()) {
            writer.println();
            writer.println(searchPath);
            wasOutput = true;
        }
    }
}

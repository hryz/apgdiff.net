
namespace cz.startnet.utils.pgdiff {

import java.io.PrintWriter;


public class SearchPathHelper {

    
    private String searchPath;
    
    private boolean wasOutput;

    
    public SearchPathHelper(String searchPath) {
        this.searchPath = searchPath;
    }

    
    public void outputSearchPath(PrintWriter writer) {
        if (!wasOutput && searchPath != null && !searchPath.isEmpty()) {
            writer.println();
            writer.println(searchPath);
            wasOutput = true;
        }
    }
}
}
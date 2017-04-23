
namespace cz.startnet.utils.pgdiff {




public class SearchPathHelper {

    
    private String searchPath;
    
    private bool wasOutput;

    
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
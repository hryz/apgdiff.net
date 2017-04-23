
using System;

namespace cz.startnet.utils.pgdiff {




public class Resources {

    
    private static ResourceBundle RESOURCE_BUNDLE =
            ResourceBundle.getBundle("cz/startnet/utils/pgdiff/Resources");

    
    public static String getString(String key) {
        return RESOURCE_BUNDLE.getString(key);
    }

    
    private Resources() {
    }
}
}
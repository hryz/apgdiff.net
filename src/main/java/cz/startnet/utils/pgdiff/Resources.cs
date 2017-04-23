
package cz.startnet.utils.pgdiff;

import java.util.ResourceBundle;


public class Resources {

    
    private static final ResourceBundle RESOURCE_BUNDLE =
            ResourceBundle.getBundle("cz/startnet/utils/pgdiff/Resources");

    
    public static String getString(final String key) {
        return RESOURCE_BUNDLE.getString(key);
    }

    
    private Resources() {
    }
}

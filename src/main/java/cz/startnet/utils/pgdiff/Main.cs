using System;

namespace pgdiff {






public class Programm {

    
    public static void Main(String[] args)
    {
        PgDiffArguments arguments = new PgDiffArguments();

        if (arguments.parse(Console.In, Console.Out, args)) {
            PgDiff.createDiff(Console.Out, arguments);
        }
    }
        
}
}
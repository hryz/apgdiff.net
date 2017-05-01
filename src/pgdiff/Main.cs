using System;

namespace pgdiff
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var arguments = new PgDiffArguments();

            if (arguments.Parse(Console.In, Console.Out, args))
                PgDiff.CreateDiff(Console.Out, arguments);
        }
    }
}
using System.IO;

namespace pgdiff
{
    public class SearchPathHelper
    {
        private readonly string _searchPath;

        private bool _wasOutput;


        public SearchPathHelper(string searchPath)
        {
            _searchPath = searchPath;
        }


        public void OutputSearchPath(TextWriter writer)
        {
            if (!_wasOutput && !string.IsNullOrEmpty(_searchPath))
            {
                writer.WriteLine();
                writer.WriteLine(_searchPath);
                _wasOutput = true;
            }
        }
    }
}
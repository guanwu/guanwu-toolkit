using System.Collections.Generic;

namespace Guanwu.Toolkit.FileProviders
{
    internal class DirectoryComparer : IEqualityComparer<KeyValuePair<string, FileSnapshot>>
    {
        public bool Equals(KeyValuePair<string, FileSnapshot> x, KeyValuePair<string, FileSnapshot> y)
        {
            return x.Key == y.Key;
        }

        public int GetHashCode(KeyValuePair<string, FileSnapshot> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}

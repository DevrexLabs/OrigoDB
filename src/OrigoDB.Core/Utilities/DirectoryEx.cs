using System.Linq;
using System.IO;

namespace OrigoDB.Core
{
    public static class DirectoryEx
    {

        public static bool IsEmpty(string directory)
        {
            int numEntries = new DirectoryInfo(directory).GetFileSystemInfos().Count();
            return numEntries == 0;
        }

    }
}

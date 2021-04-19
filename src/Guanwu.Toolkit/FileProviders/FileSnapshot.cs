using System.IO;
using Guanwu.Toolkit.Extensions.TimeSpan;

namespace Guanwu.Toolkit.FileProviders
{
    public class FileSnapshot
    {
        public string Name { get; set; }
        public long LastModified { get; set; }

        public FileSnapshot(string name)
        {
            try
            {
                var fileInfo = new FileInfo(name);
                if (fileInfo.Exists) {
                    this.Name = fileInfo.FullName;
                    this.LastModified = fileInfo.LastWriteTimeUtc.ToUnixTime();
                }
            }
            catch { }
        }

        public bool Equals(FileSnapshot other)
        {
            return other != null
                && other.Name == Name
                && other.LastModified == LastModified;
        }
    }
}
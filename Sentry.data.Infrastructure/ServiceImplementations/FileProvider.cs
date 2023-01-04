using Sentry.data.Core;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class FileProvider : IFileProvider
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public Stream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }
}

using System.IO;

namespace Sentry.data.Core
{
    public interface IFileProvider
    {
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        Stream GetFileStream(string path, FileMode mode, FileAccess access);
    }
}

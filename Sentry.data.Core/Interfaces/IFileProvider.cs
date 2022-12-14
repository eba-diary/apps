using System.IO;

namespace Sentry.data.Core
{
    public interface IFileProvider
    {
        void CreateDirectory(string path);
        void DeleteFile(string path);
        Stream GetFileStream(string path, FileMode mode, FileAccess access);
    }
}

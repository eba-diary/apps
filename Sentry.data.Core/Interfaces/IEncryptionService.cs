using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEncryptionService
    {
        Tuple<string, string> EncryptString(string inputString, string key, string iv = null);
        string DecryptString(string inputString, string key, string IV);
        string GenerateNewIV();
        string DecryptEncryptUsingNewIV(string origValue, string key, string oldIv, string newIv);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISASService
    {

        void ConvertToSASFormat(string filename, string category, string delimiter, int guessingrows);

        string GenerateSASFileName(string filename);

        event EventHandler<TransferProgressEventArgs> OnPushToProgressEvent;

    }
}

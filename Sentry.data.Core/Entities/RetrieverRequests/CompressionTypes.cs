using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{

    //If new compression types are added, the appropriate logic needs to be added to the exension method

    public enum CompressionTypes
    {
        ZIP = 0,
        GZIP = 1
    }

    public static class CompressionTypesExtensions
    {
        public static Boolean FileMatchCompression(this CompressionTypes compression, string file)
        {
            switch (compression)
            {
                case CompressionTypes.ZIP:
                    if (Path.GetExtension(file) == ".zip")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }      
                case CompressionTypes.GZIP:
                    if (Path.GetExtension(file) == ".gz")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
                    break;
            }
        }
    }
}

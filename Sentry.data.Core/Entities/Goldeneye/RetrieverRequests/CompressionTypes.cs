using System;
using System.IO;

namespace Sentry.data.Core
{

    //If new compression types are added, the appropriate logic needs to be added to the exension method

    // The following static script needs to be updated when adjustements are made to the CompressionTypes enum
    //   Sentry.data.Database\Scripts\Post-Deploy\StaticData\DataFlowCompressionTypes.sql
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
            }
        }
    }
}

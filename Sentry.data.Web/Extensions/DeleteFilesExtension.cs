using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public static class DeleteFilesExtension
    {
        public static DeleteFilesParamDto ToDto(this DeleteFilesParamModel model)
        {
            DeleteFilesParamDto dto = new DeleteFilesParamDto()
            {
                UserFileNameList = model.UserFileNameList,
                UserFileIdList = model.UserFileIdList
            };

            return dto;
        }
    }
}
using Sentry.data.Core.DTO.Retriever;
using Sentry.data.Web.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Extensions
{
    public static class DataSourceTokenExtension
    {
        public static DataSourceTokenDto ToDto(this DataSourceTokenModel dataSourceTokenModel)
        {
            return new DataSourceTokenDto
            {
                Id = dataSourceTokenModel.Id,
                CurrentToken = dataSourceTokenModel.CurrentToken,
                RefreshToken = dataSourceTokenModel.RefreshToken,
                CurrentTokenExp = dataSourceTokenModel.CurrentTokenExp,
                TokenName = dataSourceTokenModel.TokenName,
                TokenUrl = dataSourceTokenModel.TokenUrl,
                Scope = dataSourceTokenModel.Scope,
                TokenExp = dataSourceTokenModel.TokenExp,
                ToDelete = dataSourceTokenModel.ToDelete,
                Enabled = dataSourceTokenModel.Enabled,
            };
        }
        public static DataSourceTokenModel ToModel(this DataSourceTokenDto dataSourceTokenDto)
        {
            return new DataSourceTokenModel
            {
                Id = dataSourceTokenDto.Id,
                CurrentToken = dataSourceTokenDto.CurrentToken,
                RefreshToken = dataSourceTokenDto.RefreshToken,
                CurrentTokenExp = dataSourceTokenDto.CurrentTokenExp,
                TokenName = dataSourceTokenDto.TokenName,
                TokenUrl = dataSourceTokenDto.TokenUrl,
                Scope = dataSourceTokenDto.Scope,
                TokenExp = dataSourceTokenDto.TokenExp,
                Enabled = dataSourceTokenDto.Enabled
            };
        }
    }
}
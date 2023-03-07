using AutoMapper;
using System;

namespace Sentry.data.Web.API
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<DateTime, DateTime>().ConvertUsing(x => x.ToLocalTime());
        }
    }
}
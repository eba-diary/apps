using AutoMapper;
using System;

namespace Sentry.data.Web.API
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            //setting to unspecified kind for dates to return as they are saved in the DB
            CreateMap<DateTime, DateTime>().ConvertUsing(x => DateTime.SpecifyKind(x, DateTimeKind.Unspecified));
        }
    }
}
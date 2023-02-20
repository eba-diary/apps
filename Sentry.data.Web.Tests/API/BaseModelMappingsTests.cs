using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.API;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public abstract class BaseModelMappingsTests
    {
        protected IMapper _mapper;

        [TestInitialize]
        public void Initialize()
        {
            _mapper = AutoMapperHelper.InitMapper();
        }
    }
}

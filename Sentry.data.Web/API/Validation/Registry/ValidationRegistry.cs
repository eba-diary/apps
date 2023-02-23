using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.API
{
    public class ValidationRegistry : IValidationRegistry
    {
        private readonly Dictionary<Type, IRequestModelValidator> _validators;

        public ValidationRegistry(IList<IRequestModelValidator> validators)
        {
            _validators = new Dictionary<Type, IRequestModelValidator>();

            foreach (IRequestModelValidator validator in validators)
            {
                Type modelType = validator.GetType().GetInterfaces().First(x => x.IsGenericType).GetGenericArguments().First();
                _validators.Add(modelType, validator);
            }
        }

        public bool TryGetValidatorFor<T>(out IRequestModelValidator validator) where T : IRequestModel
        {
            return _validators.TryGetValue(typeof(T), out validator);
        }
    }
}
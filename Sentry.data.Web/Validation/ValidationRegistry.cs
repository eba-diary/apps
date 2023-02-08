using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class ValidationRegistry : IValidationRegistry
    {
        private readonly Dictionary<Type, IViewModelValidator> _validators;

        public ValidationRegistry(IList<IViewModelValidator> validators)
        {
            _validators = new Dictionary<Type, IViewModelValidator>();

            foreach (IViewModelValidator validator in validators)
            {
                Type modelType = validator.GetType().GetInterfaces().First(x => x.IsGenericType).GetGenericArguments().First();
                _validators.Add(modelType, validator);
            }
        }

        public bool TryGetValidatorFor<T>(out IViewModelValidator validator) where T : IRequestViewModel
        {
            return _validators.TryGetValue(typeof(T), out validator);
        }
    }
}
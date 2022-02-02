using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sentry.data.Web
{
    public class RequiredDelimiter : ValidationAttribute
    {
        private readonly string _extensionIdPropertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo extensionId = validationContext.ObjectInstance.GetType().GetProperty(_extensionIdPropertyName);

            if (extensionId != null && (int)extensionId.GetValue(validationContext.ObjectInstance) == 7 && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                return new ValidationResult("Delimiter is required for DELIMITED file extension type");
            }

            return ValidationResult.Success;
        }

        public RequiredDelimiter(string extensionIdPropertyName)
        {
            _extensionIdPropertyName = extensionIdPropertyName;
        }
    }
}
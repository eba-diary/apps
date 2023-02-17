using Sentry.data.Core.GlobalEnums;
using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sentry.data.Web.API
{
    public static class ValidationExtensions
    {
        public static FluentValidationResponse Validate(this ValidationResponseModel validationResponse, Expression<Func<string>> propertyValueExpression)
        {
            return new FluentValidationResponse
            {
                PropertyValueExpression = propertyValueExpression,
                ValidationResponse = validationResponse
            };
        }

        public static FluentValidationResponse Validate(this FluentValidationResponse fluentResponse, Expression<Func<string>> propertyValueExpression)
        {
            fluentResponse.PropertyValueExpression = propertyValueExpression;
            return fluentResponse;
        }

        public static FluentValidationResponse Required(this FluentValidationResponse fluentResponse)
        {
            if (string.IsNullOrWhiteSpace(fluentResponse.PropertyValueExpression.Compile()()))
            {
                MemberExpression memberExp = fluentResponse.PropertyValueExpression.Body as MemberExpression;
                fluentResponse.ValidationResponse.AddFieldValidation(memberExp.Member.Name, $"Required field");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse MaxLength(this FluentValidationResponse fluentResponse, int maxLength)
        {
            if (fluentResponse.PropertyValueExpression.Compile()()?.Length > maxLength)
            {
                MemberExpression memberExp = fluentResponse.PropertyValueExpression.Body as MemberExpression;
                fluentResponse.ValidationResponse.AddFieldValidation(memberExp.Member.Name, $"Max length of {maxLength} characters");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse RegularExpression(this FluentValidationResponse fluentResponse, string pattern, string message)
        {
            string value = fluentResponse.PropertyValueExpression.Compile()();
            if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(fluentResponse.PropertyValueExpression.Compile()(), pattern))
            {
                MemberExpression memberExp = fluentResponse.PropertyValueExpression.Body as MemberExpression;
                fluentResponse.ValidationResponse.AddFieldValidation(memberExp.Member.Name, message);
            }

            return fluentResponse;
        }

        public static FluentValidationResponse EnumValue<T>(this FluentValidationResponse fluentResponse) where T : struct
        {
            string value = fluentResponse.PropertyValueExpression.Compile()();
            if (!Enum.TryParse(value, true, out T result))
            {
                string[] values = Enum.GetNames(typeof(T));
                MemberExpression memberExp = fluentResponse.PropertyValueExpression.Body as MemberExpression;
                fluentResponse.ValidationResponse.AddFieldValidation(memberExp.Member.Name, $"Must provide a valid value - {string.Join(" | ", values)}");
            }

            return fluentResponse;
        }
    }
}
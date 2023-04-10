using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sentry.data.Web.API
{
    public static class FluentValidationExtensions
    {
        public static FluentValidationResponse<TModel, TProperty> Validate<TModel, TProperty>(this TModel requestModel, Expression<Func<TModel, TProperty>> propertyValueExpression) where TModel : IRequestModel
        {
            MemberExpression memberExp = propertyValueExpression.Body as MemberExpression;

            return new FluentValidationResponse<TModel, TProperty>
            {
                ValidationResponse = new ConcurrentValidationResponse(),
                PropertyName = memberExp.Member.Name,
                PropertyValue = propertyValueExpression.Compile().Invoke(requestModel),
                RequestModel = requestModel
            };
        }

        public static FluentValidationResponse<TModel, TProperty> Validate<TModel, TProperty>(this FluentValidationResponse<TModel, TProperty> fluentResponse, Expression<Func<TModel, TProperty>> propertyValueExpression) where TModel : IRequestModel
        {
            MemberExpression memberExp = propertyValueExpression.Body as MemberExpression;

            fluentResponse.PropertyName = memberExp.Member.Name;
            fluentResponse.PropertyValue = propertyValueExpression.Compile().Invoke(fluentResponse.RequestModel);
            fluentResponse.IsRequiredProperty = false;

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, string> Required<TModel>(this FluentValidationResponse<TModel, string> fluentResponse) where TModel : IRequestModel
        {
            fluentResponse.IsRequiredProperty = true;

            if (string.IsNullOrWhiteSpace(fluentResponse.PropertyValue))
            {
                fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, $"Required field");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, List<TList>> Required<TModel, TList>(this FluentValidationResponse<TModel, List<TList>> fluentResponse) where TModel : IRequestModel
        {
            fluentResponse.IsRequiredProperty = true;

            if (fluentResponse.PropertyValue?.Any() != true)
            {
                fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, $"Required field");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, string> MaxLength<TModel>(this FluentValidationResponse<TModel, string> fluentResponse, int maxLength) where TModel : IRequestModel
        {
            if (fluentResponse.PropertyValue?.Length > maxLength)
            {
                fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, $"Max length of {maxLength} characters");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, List<TList>> MaxLength<TModel, TList>(this FluentValidationResponse<TModel, List<TList>> fluentResponse, int maxLength) where TModel : IRequestModel
        {
            if (fluentResponse.PropertyValue?.Count > maxLength)
            {
                fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, $"Max length of {maxLength} values");
            }

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, string> RegularExpression<TModel>(this FluentValidationResponse<TModel, string> fluentResponse, string pattern, string message) where TModel : IRequestModel
        {
            if (!string.IsNullOrEmpty(fluentResponse.PropertyValue) && !Regex.IsMatch(fluentResponse.PropertyValue, pattern))
            {
                fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, message);
            }

            return fluentResponse;
        }

        public static FluentValidationResponse<TModel, string> EnumValue<TModel>(this FluentValidationResponse<TModel, string> fluentResponse, Type enumType, string invalidOption = null) where TModel : IRequestModel
        {
            if (fluentResponse.IsRequiredProperty || !string.IsNullOrWhiteSpace(fluentResponse.PropertyValue))
            {
                bool invalid = !string.IsNullOrEmpty(invalidOption) && string.Equals(fluentResponse.PropertyValue, invalidOption, StringComparison.OrdinalIgnoreCase);

                if (!invalid)
                {
                    try
                    {
                        Enum.Parse(enumType, fluentResponse.PropertyValue, true);
                    }
                    catch (ArgumentException)
                    {
                        invalid = true;
                    }
                }

                if (invalid)
                {
                    string[] values = Enum.GetNames(enumType);

                    if (!string.IsNullOrEmpty(invalidOption))
                    {
                        values = values.Where(x => x != invalidOption).ToArray();
                    }

                    fluentResponse.ValidationResponse.AddFieldValidation(fluentResponse.PropertyName, $"Must provide a valid value - {string.Join(" | ", values)}");
                }
            }

            return fluentResponse;
        }
    }
}
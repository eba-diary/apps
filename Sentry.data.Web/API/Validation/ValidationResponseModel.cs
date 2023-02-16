using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sentry.data.Web.API
{
    public class ValidationResponseModel : IResponseModel
    {
        public List<FieldValidationResponseModel> FieldValidations { get; set; }

        public bool IsValid()
        {
            return FieldValidations?.Any() == false;
        }

        /// <summary>
        /// Adds validation message for a field. If field already has validation, adds the message to the existing field validation.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        public void AddFieldValidation(string field, string message)
        {
            if (FieldValidations?.Any() == false)
            {
                FieldValidations = new List<FieldValidationResponseModel>();
            }

            FieldValidationResponseModel fieldValidation = FieldValidations.FirstOrDefault(x => x.Field == field);
            
            if (fieldValidation == null)
            {
                fieldValidation = new FieldValidationResponseModel { Field = field };
                fieldValidation.AddValidationMessage(message);

                FieldValidations.Add(fieldValidation);
            }
            else
            {
                fieldValidation.AddValidationMessage(message);
            }
        }

        public ValidationResponseModel Required(Expression<Func<string>> getPropertyValue)
        {
            if (string.IsNullOrWhiteSpace(getPropertyValue.Compile()()))
            {
                MemberExpression memberExp = getPropertyValue.Body as MemberExpression;
                AddFieldValidation(memberExp.Member.Name, $"Required field");
            }

            return this;
        }

        public ValidationResponseModel MaxLength(Expression<Func<string>> getPropertyValue, int maxLength)
        {
            if (getPropertyValue.Compile()()?.Length > maxLength)
            {
                MemberExpression memberExp = getPropertyValue.Body as MemberExpression;
                AddFieldValidation(memberExp.Member.Name, $"Max length of {maxLength} characters");
            }

            return this;
        }

        public ValidationResponseModel RegularExpression(Expression<Func<string>> getPropertyValue, string pattern, string message)
        {
            string value = getPropertyValue.Compile()();
            if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(getPropertyValue.Compile()(), pattern))
            {
                MemberExpression memberExp = getPropertyValue.Body as MemberExpression;
                AddFieldValidation(memberExp.Member.Name, message);
            }

            return this;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Sentry.data.Web.API
{
    public class ValidationResponseModel : IResponseModel
    {
        public List<FieldValidationResponseModel> FieldValidations { get; set; }

        public bool IsValid()
        {
            return FieldValidations?.Any() == false;
        }

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
    }
}
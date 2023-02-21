using System.Collections.Concurrent;
using System.Linq;

namespace Sentry.data.Web.API
{
    public class ConcurrentValidationResponse
    {
        public ConcurrentQueue<ConcurrentFieldValidationResponse> FieldValidations { get; protected set; }

        public bool IsValid()
        {
            return FieldValidations == null || !FieldValidations.Any();
        }

        /// <summary>
        /// Adds validation message for a field. If field already has validation, adds the message to the existing field validation.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        public void AddFieldValidation(string field, string message)
        {
            if (FieldValidations == null)
            {
                FieldValidations = new ConcurrentQueue<ConcurrentFieldValidationResponse>();
            }

            ConcurrentFieldValidationResponse fieldValidation = FieldValidations.FirstOrDefault(x => x.Field == field);

            if (fieldValidation == null)
            {
                fieldValidation = new ConcurrentFieldValidationResponse { Field = field };
                fieldValidation.AddValidationMessage(message);

                FieldValidations.Enqueue(fieldValidation);
            }
            else
            {
                fieldValidation.AddValidationMessage(message);
            }
        }

        public bool HasValidationsFor(string field)
        {
            return FieldValidations?.Any(x => x.Field == field) == true;
        }
    }
}
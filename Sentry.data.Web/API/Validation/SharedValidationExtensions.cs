using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public static class SharedValidationExtensions
    {
        public static async Task ValidatePrimaryContactIdAsync(this IPrimaryContactModel model, IAssociateInfoProvider associateInfoProvider, ConcurrentValidationResponse validationResponse)
        {
            if (!string.IsNullOrEmpty(model.PrimaryContactId))
            {
                Associate associate = await associateInfoProvider.GetActiveAssociateByIdAsync(model.PrimaryContactId);

                if (associate == null)
                {
                    validationResponse.AddFieldValidation(nameof(model.PrimaryContactId), "Must be a valid active associate");
                }
            }
        }        

        public static async Task ValidateSaidEnvironmentAsync<T>(this T requestModel, ISAIDService saidService, IQuartermasterService quartermasterService, ConcurrentValidationResponse validationResponse) where T : ISaidEnvironmentModel, IRequestModel
        {
            validationResponse.AddValidationsFrom(requestModel.Validate(x => x.SaidAssetCode).Required()
                .Validate(x => x.NamedEnvironment).Required().RegularExpression("^[A-Z0-9]{1,10}$", "Must be alphanumeric, all caps, and less than 10 characters")
                .Validate(x => x.NamedEnvironmentTypeCode).Required().EnumValue(typeof(NamedEnvironmentType))
                .ValidationResponse);

            if (!validationResponse.HasValidationsFor(nameof(requestModel.SaidAssetCode)))
            {
                if (await saidService.VerifyAssetExistsAsync(requestModel.SaidAssetCode))
                {
                    if (!validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment)) && Enum.TryParse(requestModel.NamedEnvironmentTypeCode, true, out NamedEnvironmentType namedEnvironmentType))
                    {
                        ValidationResults validationResults = await quartermasterService.VerifyNamedEnvironmentAsync(requestModel.SaidAssetCode, requestModel.NamedEnvironment, namedEnvironmentType);

                        TranslateNamedEnvironmentValidationResults(validationResults, validationResponse);
                    }
                }
                else
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.SaidAssetCode), "Must be a valid SAID asset code");
                }
            }
        }

        #region Private
        private static void TranslateNamedEnvironmentValidationResults(ValidationResults validationResults, ConcurrentValidationResponse validationResponse)
        {
            if (!validationResults.IsValid())
            {
                //loop over results and align properties with the validation error returned
                foreach (ValidationResult result in validationResults.GetAll())
                {
                    switch (result.Id)
                    {
                        case ValidationErrors.NAMED_ENVIRONMENT_INVALID:
                            validationResponse.AddFieldValidation(nameof(ISaidEnvironmentModel.NamedEnvironment), result.Description);
                            break;
                        case ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID:
                            validationResponse.AddFieldValidation(nameof(ISaidEnvironmentModel.NamedEnvironmentTypeCode), result.Description);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
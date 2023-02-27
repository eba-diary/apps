using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public static class SharedValidationExtensions
    {
        public static async Task ValidatePrimaryContactIdAsync(this BaseDatasetModel model, IAssociateInfoProvider associateInfoProvider, ConcurrentValidationResponse validationResponse)
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

        public static async Task ValidateAlternateContactEmailAsync(this BaseDatasetModel model, ConcurrentValidationResponse validationResponse)
        {
            await Task.Run(() =>
            {
                //validate alternate email is sentry email
                if (!ValidationHelper.IsDSCEmailValid(model.AlternateContactEmail))
                {
                    validationResponse.AddFieldValidation(nameof(model.AlternateContactEmail), "Must be valid sentry.com email address");
                }
            });
        }

        public static void ValidateCategoryCode(this BaseDatasetModel model, IDatasetContext datasetContext, ConcurrentValidationResponse validationResponse)
        {
            if (validationResponse.HasValidationsFor(nameof(model.CategoryCode)) || 
               (!string.IsNullOrEmpty(model.CategoryCode) && !datasetContext.Categories.Any(x => x.Name.ToLower() == model.CategoryCode.ToLower() && x.ObjectType == DataEntityCodes.DATASET)))
            {
                List<string> categoryNames = datasetContext.Categories.Where(x => x.ObjectType == DataEntityCodes.DATASET).Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(model.CategoryCode), $"Must provide a valid value - {string.Join(" | ", categoryNames)}");
            }
        }

        public static async Task ValidateSaidEnvironmentAsync(this ISaidEnvironmentModel requestModel, ISAIDService saidService, IQuartermasterService quartermasterService, ConcurrentValidationResponse validationResponse)
        {
            if (!validationResponse.HasValidationsFor(nameof(requestModel.SaidAssetCode)))
            {
                if (await saidService.VerifyAssetExistsAsync(requestModel.SaidAssetCode))
                {
                    if (!validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment)) && Enum.TryParse(requestModel.NamedEnvironmentTypeCode, true, out NamedEnvironmentType namedEnvironmentType))
                    {
                        ValidationResults validationResults = await quartermasterService.VerifyNamedEnvironmentAsync(requestModel.SaidAssetCode, requestModel.NamedEnvironment, namedEnvironmentType);

                        if (!validationResults.IsValid())
                        {
                            //loop over results and align properties with the validation error returned
                            foreach (ValidationResult result in validationResults.GetAll())
                            {
                                switch (result.Id)
                                {
                                    case ValidationErrors.NAMED_ENVIRONMENT_INVALID:
                                        validationResponse.AddFieldValidation(nameof(requestModel.NamedEnvironment), result.Description);
                                        break;
                                    case ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID:
                                        validationResponse.AddFieldValidation(nameof(requestModel.NamedEnvironmentTypeCode), result.Description);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.SaidAssetCode), "Must be a valid SAID asset code");
                }
            }
        }
    }
}
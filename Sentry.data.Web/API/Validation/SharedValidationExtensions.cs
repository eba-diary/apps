using Sentry.Associates;
using Sentry.data.Core;
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
               (!string.IsNullOrEmpty(model.PrimaryContactId) && !datasetContext.Categories.Any(x => x.Name.ToLower() == model.CategoryCode.ToLower() && x.ObjectType == DataEntityCodes.DATASET)))
            {
                List<string> categoryNames = datasetContext.Categories.Where(x => x.ObjectType == DataEntityCodes.DATASET).Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(model.CategoryCode), $"Must provide a valid value - {string.Join(" | ", categoryNames)}");
            }
        }
    }
}
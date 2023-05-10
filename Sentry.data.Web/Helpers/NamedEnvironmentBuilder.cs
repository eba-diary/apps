using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Helpers
{
    public class NamedEnvironmentBuilder
    {
        private readonly IQuartermasterService _quartermasterService;
        private readonly IDataFeatures _dataFeatures;

        public NamedEnvironmentBuilder(IQuartermasterService quartermasterService, IDataFeatures dataFeatures)
        {
            _quartermasterService = quartermasterService;
            _dataFeatures = dataFeatures;
        }

        /// <summary>
        /// Returns a List of SelectListItems for the NamedEnvironment drop-down,
        /// as well as a List of SelectListItems for the NamedEnvironmentType drop-down.
        /// Used by both the Dataflow and Dataset controllers
        /// </summary>
        /// <param name="saidAssetKeyCode">SAID Asset key code</param>
        /// <param name="namedEnvironmentToSelect">If a named environment has already been selected by the user; otherwise leave blank</param>
        /// <param name="namedEnvironmentToExclude">Exclude option remove that namedEnvironment from the list of Q named Environment</param>
        public async Task<(List<SelectListItem> namedEnvironmentList, List<SelectListItem> namedEnvironmentTypeList)> BuildNamedEnvironmentDropDownsAsync(string saidAssetKeyCode, string namedEnvironmentToSelect, string namedEnvironmentToExclude=null)
        {
            //if no keyCode has been selected yet, skip the call to Quartermaster
            List<NamedEnvironmentDto> qNamedEnvironmentList = new List<NamedEnvironmentDto>();
            if (!string.IsNullOrWhiteSpace(saidAssetKeyCode))
            {
                qNamedEnvironmentList = await _quartermasterService.GetNamedEnvironmentsAsync(saidAssetKeyCode);
            }

            //FIGURE OUT WHICH NAMED ENVIRONMENT SHOULD BE MARKED WITH  "Selected" = TRUE
            //ONLY APPLY LOGIC IF namedEnvironmentToExclude!=NULL AND namedEnvironmentToSelect=NULL
            if (namedEnvironmentToExclude != null && namedEnvironmentToSelect == null)
            {
                //GOAL IS TO default the first item in the list to be "selected" BUT IGNORE namedEnvironmentToExclude,  NOTE: qNamedEnvironmentList is already ordered by name so no need to re-order
                namedEnvironmentToSelect = qNamedEnvironmentList.FirstOrDefault(w => w.NamedEnvironment != namedEnvironmentToExclude)?.NamedEnvironment;
            }

            //NOTE: pass namedEnvironmentToSelect which tells this function to mark the correct namedEnvironment with "select"
            List<SelectListItem> namedEnvironmentList = BuildNamedEnvironmentDropDown(namedEnvironmentToSelect, qNamedEnvironmentList);

            //NOTE: pass namedEnvironmentToSelect which tells this function to mark the correct namedEnvironment's Type with "select" (e.g. NonProd vs Prod)
            // REMEMBER EACH NAMEDENVIRONMENT DROP DOWN HAS AN ASSOCIATED NAMEDENVIRONMENTTYPE DROP DOWN THAT NEEDS TO BE SELECTED
            List<SelectListItem> namedEnvironmentTypeList = BuildNamedEnvironmentTypeDropDown(namedEnvironmentToSelect, qNamedEnvironmentList);
            return (namedEnvironmentList, namedEnvironmentTypeList);
        }

        /// <summary>
        /// Creates the "Named Environment" drop down used on the DataFlow and Dataset pages
        /// </summary>
        /// <param name="namedEnvironmentToSelect">What Named Environment is selected</param>
        /// <param name="qNamedEnvironmentList">The list of Environments from Quartermaster</param>
        public static List<SelectListItem> BuildNamedEnvironmentDropDown(string namedEnvironmentToSelect, List<NamedEnvironmentDto> qNamedEnvironmentList)
        {
            //convert the list of Quartermaster environments into SelectListItems
            return qNamedEnvironmentList.Select(env => new SelectListItem()
            {
                Value = env.NamedEnvironment,
                Text = env.NamedEnvironment,
                Selected = (!string.IsNullOrWhiteSpace(namedEnvironmentToSelect) && env.NamedEnvironment == namedEnvironmentToSelect)
            }).ToList();
        }

        /// <summary>
        /// Creates the "Named Environment type" drop down used on the DataFlow and Dataset pages
        /// </summary>
        /// <param name="namedEnvironmentToSelect">What Named Environment is selected</param>
        /// <param name="qNamedEnvironmentList">The list of Environments from Quartermaster</param>
        public List<SelectListItem> BuildNamedEnvironmentTypeDropDown(string namedEnvironmentToSelect, List<NamedEnvironmentDto> qNamedEnvironmentList)
        {
            //figure out the correct NamedEnvironmentType for the selected NamedEnvironment
            string namedEnvironmentType = NamedEnvironmentType.NonProd.ToString();

            //if an Environment Type filter is configured, create the filter and default to that environment type
            var environmentTypeFilter = _dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue();
            Func<string, bool> filter = envType => true;
            if (!string.IsNullOrWhiteSpace(environmentTypeFilter))
            {
                filter = envType => envType == environmentTypeFilter;
                namedEnvironmentType = environmentTypeFilter;
            }

            //if there are named environments, select the correct namedEnvironmentType for the chosen environment
            //(the DataFlowService will already have filtered them down to only the appropriate namedEnvironmentTypes)
            if (qNamedEnvironmentList.Any())
            {
                if (string.IsNullOrWhiteSpace(namedEnvironmentToSelect))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First().NamedEnvironmentType.ToString();
                }
                else if (qNamedEnvironmentList.Any(e => e.NamedEnvironment == namedEnvironmentToSelect))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First(e => e.NamedEnvironment == namedEnvironmentToSelect).NamedEnvironmentType.ToString();
                }
            }

            //convert the list of named environment types into SelectListLitems
            var namedEnvironmentTypeList = Enum.GetNames(typeof(NamedEnvironmentType)).Where(filter).Select(env => new SelectListItem()
            {
                Value = env,
                Text = env,
                Selected = namedEnvironmentType == env
            }).ToList();

            return namedEnvironmentTypeList;
        }
    }
}
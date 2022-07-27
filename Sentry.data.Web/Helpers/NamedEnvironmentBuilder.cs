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
        /// <param name="namedEnvironment">If a named environment has already been selected by the user; otherwise leave blank</param>
        public async Task<(List<SelectListItem> namedEnvironmentList, List<SelectListItem> namedEnvironmentTypeList)> BuildNamedEnvironmentDropDownsAsync(string saidAssetKeyCode, string namedEnvironment)
        {
            //if no keyCode has been selected yet, skip the call to Quartermaster
            List<NamedEnvironmentDto> qNamedEnvironmentList = new List<NamedEnvironmentDto>();
            if (!string.IsNullOrWhiteSpace(saidAssetKeyCode))
            {
                qNamedEnvironmentList = await _quartermasterService.GetNamedEnvironmentsAsync(saidAssetKeyCode);
            }

            List<SelectListItem> namedEnvironmentList = BuildNamedEnvironmentDropDown(namedEnvironment, qNamedEnvironmentList);

            List<SelectListItem> namedEnvironmentTypeList = BuildNamedEnvironmentTypeDropDown(namedEnvironment, qNamedEnvironmentList);

            return (namedEnvironmentList, namedEnvironmentTypeList);
        }

        /// <summary>
        /// Creates the "Named Environment" drop down used on the DataFlow and Dataset pages
        /// </summary>
        /// <param name="namedEnvironment">What Named Environment is selected</param>
        /// <param name="qNamedEnvironmentList">The list of Environments from Quartermaster</param>
        public static List<SelectListItem> BuildNamedEnvironmentDropDown(string namedEnvironment, List<NamedEnvironmentDto> qNamedEnvironmentList)
        {
            //convert the list of Quartermaster environments into SelectListItems
            return qNamedEnvironmentList.Select(env => new SelectListItem()
            {
                Value = env.NamedEnvironment,
                Text = env.NamedEnvironment,
                Selected = (!string.IsNullOrWhiteSpace(namedEnvironment) && env.NamedEnvironment == namedEnvironment)
            }).ToList();
        }

        /// <summary>
        /// Creates the "Named Environment type" drop down used on the DataFlow and Dataset pages
        /// </summary>
        /// <param name="namedEnvironment">What Named Environment is selected</param>
        /// <param name="qNamedEnvironmentList">The list of Environments from Quartermaster</param>
        public List<SelectListItem> BuildNamedEnvironmentTypeDropDown(string namedEnvironment, List<NamedEnvironmentDto> qNamedEnvironmentList)
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
                if (string.IsNullOrWhiteSpace(namedEnvironment))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First().NamedEnvironmentType.ToString();
                }
                else if (qNamedEnvironmentList.Any(e => e.NamedEnvironment == namedEnvironment))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First(e => e.NamedEnvironment == namedEnvironment).NamedEnvironmentType.ToString();
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
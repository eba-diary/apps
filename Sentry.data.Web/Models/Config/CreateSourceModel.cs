using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CreateSourceModel
    {
        public CreateSourceModel()
        {

        }

        public virtual int Id { get; set; }

        public virtual string ReturnUrl { get; set; }

        [Required]
        [DisplayName("Data Source Name")]
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        [Required]
        [DisplayName("Source Type")]
        public virtual string SourceType { get; set; }

        [Required]
        [DisplayName("Authentication Type")]
        public virtual string AuthID { get; set; }

        [Required]
        [DisplayName("Are Username and Password Required?")]
        // For certain types of Data Sources this value is required to be true.  Setting a checkboxfor to readonly still allows
        //   a user to change the value.  A workaround being used for this property is using jquery to disable the property
        //   for the specific data source types which require it to be true.  Then a $('form').on('submit') function
        //   enables the checkbox right before submitting the form.  This allows the value to be passed back to the controller.
        //https://stackoverflow.com/questions/40134337/set-mvc-checkbox-to-readonly-or-disabled-on-client-side
        public virtual bool IsUserPassRequired { get; set; }

        [DisplayName("Port Number")]
        public virtual int PortNumber { get; set; }

        [Required]
        [DisplayName("Base URL")]
        public virtual Uri BaseUri { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> AuthTypesDropdown { get; set; }
    }
}
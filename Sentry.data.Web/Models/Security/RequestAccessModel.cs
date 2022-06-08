
using Sentry.data.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class AccessRequestModel
    {
        //shared
        public int SecurableObjectId { get; set; }

        public string SecurableObjectName { get; set; }

        [DisplayName("Dataset")]
        public string DatasetName { get; set; }

        [MaxLength(64)]
        [DisplayName("AD Group")]
        public string AdGroupName { get; set; }

        [Required]
        [MaxLength(512)]
        [DisplayName("Business Reason")]
        public string BusinessReason { get; set; }

        [Required]
        [DisplayName("Approver")]
        public string SelectedApprover { get; set; }

        [DisplayName("Please select the permissions you would like to request")]
        public string SelectedPermissions { get; set; }


        public List<PermissionModel> AllPermissions { get; set; }
        public List<SelectListItem> AllApprovers { get; set; }
        public string SaidKeyCode { get; set; }
        public bool IsAddingPermission { get; set; }
        public AccessRequestType Type { get; set; }
        [DisplayName("Amazon Resource Name")]
        public string AwsArn { get; set; }
        public AccessScope Scope { get; set; }
    }
}
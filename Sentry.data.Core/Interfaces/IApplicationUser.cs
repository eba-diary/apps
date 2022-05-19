using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    /// <summary>
    /// The IApplicationUser represents one user of the application.  This may be the "current user" or
    /// any other user.  It acts as an aggregate of all information we care about pertaining to the user,
    /// no matter the source.  Typical sources include our own domain, HR, and Obsidian.  It is essential
    /// that an instance of IApplicationUser is NOT cached or persisted in any way, as it may contain "hooks" to
    /// a particular domain context, for example.  Instead, use the UserService to get a new instance of
    /// an IApplicationUser whenever one is needed.  They are lightweight to create, and external information sources
    /// should be caching data anyway, so no need to cache the IApplicationUser.
    /// </summary>
    /// <remarks></remarks>
    public interface IApplicationUser
    {
        string AssociateId { get; }
        string EmailAddress { get; }
        string DisplayName { get; }

        Boolean CanUseApp { get; }
        Boolean CanUserSwitch { get; } 
        IEnumerable<string> Permissions { get; }
        Boolean CanViewDataset { get; } //DSC_Dataset_View - DatasetView
        Boolean CanModifyDataset { get; } //DSC_Dataset_Modify - DatasetModify
        Boolean CanViewReports { get; } //DSC_Report_View - ReportView
        Boolean CanManageReports { get; } //DSC_Report_Modify - ReportModify
        Boolean CanViewDataAsset { get; } //DSC_Asset_View - DataAssetView
        Boolean CanManageAssetAlerts { get; } //DSC_Asset_Modify - DataAssetModify
        bool IsAdmin { get; } //App_DataMgmt_Admin - AdminUser
        bool IsInGroup(string group);
        //Calculated values - may come from external data sources and/or our domain User object
        DomainUser DomainUser { get; }
        Boolean CanViewSensitiveDataInventory { get; }
        Boolean CanViewDataInventory { get; }
        Boolean CanEditSensitiveDataInventory { get; }
        Boolean CanEditOwnerVerifiedDataInventory { get; }
    }
}

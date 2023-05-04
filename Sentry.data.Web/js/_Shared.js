/******************************************************************************************
 * TIP: Use XML documentation comments so they work with Visual Studio Intellisense:
 * http://msdn.microsoft.com/en-us/library/bb514138.aspx 
 * Check out the "JavaScript vsdoc Generator" Visual Studio extension to make it easy:
 * https://visualstudiogallery.msdn.microsoft.com/0cb7304b-ad78-4283-ba2b-42804657fcdd
 ******************************************************************************************/



// Plug-in initialization functions that should run on every page when it loads 
$(function () {
    // The following supports anti request forgery tokens for AJAX POSTs.  
    $.ajaxSetup({
        beforeSend: function (xhr) {
            var securityToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
            xhr.setRequestHeader("__RequestVerificationToken", securityToken);
        }
    });
});

// Put your application's custom functions in a JavaScript "namespace" to avoid collisions
// with other libraries
window.data = new function () {

    //NOTE: UNVIVERSAL PLACE TO STORE WHICH DSC API VERSION ENTIRE APP WILL LEVERAGE.  CHANGE AT YOUR OWN RISK.
    this.ApiVersion = "v20220609";
    this.GetApiVersion = function () {
        return this.ApiVersion;
    };

    this.BetaApiVersion = "v20230315";

    // Use the format below for adding global methods
    this.HelperMethod = function (param1, param2) {
        //logic goes here
    };

    this.RemoveSpinner = function (selector) {
        $(selector).children('.sentry-spinner-container').remove();
    };
}

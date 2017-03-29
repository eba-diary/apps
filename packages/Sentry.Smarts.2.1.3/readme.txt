
## SMARTS code setup ##

If you're using MVC, add the following route-ignore:

    routes.IgnoreRoute("_status/{*pathInfo}")

If you have code in your Global.asax event handlers that looks like:

    HttpContext.Current.Request.Url.AbsoluteUri.ToUpper.EndsWith("/_STATUS/AVAILABLE.ASPX")

Replace it with:

    HttpContext.Current.Request.Url.AbsoluteUri.ToUpper.Contains("/_STATUS/")

You can delete this file when you no longer need to reference it.

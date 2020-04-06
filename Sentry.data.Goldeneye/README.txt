Sentry.Web Release Notes

V 6.2.0
Includes major fixes for bugs occuring in 6.1.0 including experiencing redirect loops when authenticating users with MFA enabled.

V 6.1.0
Includes updates to enforce MFA validation for external SentryLogin users who have opted in to MFA.  (Applications only serving internal users will not be affected.)
Please reach out to ApplicationServices@sentry.com to validate your configured sentrySettings -> SentryLoginSettings "AuthenticationServiceProfile" and "RequestingApplication" values when updating, 
in order to verify that users getting redirected to login.sentry.com to complete MFA will be served with the proper user experience.

Required Changes for 6.1.0
You will need to update your application’s sentrySettings -> SentryLoginSettings property “AuthenticationServiceUrl” to environmental values of:

NonProd -> https://authenticationwsqual.sentry.com/authentication/v20190101 
Prod -> https://authenticationws.sentry.com/authentication/v20190101 
If this update is not made, you’ll receive the error: “Login redirect loop detected.  User is likely authenticated to a scheme not supported by this application but is supported by Sentry Login.”
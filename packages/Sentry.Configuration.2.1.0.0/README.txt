Sentry.Configuration 20140225 2.1.0.0 (CLR version 2.0.5.0)
=========================================
New features
    - Now Sentry.Configuration leverages Sentry.Common.Logging, so you can add a logger
	  named Sentry.Configuration, and get log messages out of this library

Sentry.Configuration 20131213  2.0.10.0 (CLR version 2.0.5.0)
=========================================
Bug fixes
    - Fixed a thread synchronization issue where Sentry.Configuration would become increasingly
	  slower in the scenario where no webHost entries in the defaultEnvironmentList would match a given
	  HTTP request, but a machine entry would match.

Sentry.Configuration 20130419  2.0.8.0 (CLR version 2.0.5.0)
=========================================
New features
    - In the defaultEnvironmentList section, for webHost or machine entries, you can now specify
	  a nameRegex attribute instead of the name.  This allows you to match several hosts or machine
	  names with a single entry.

	  Old:
          <webHost name="mywebsitequal"/>
          <webHost name="mywebsitequal1"/>
          <webHost name="mywebsitequal2"/>
          <webHost name="mywebsitequal3"/>
          <webHost name="mywebsitequal4"/>
          <machine name="dixis78b0"/>
          <machine name="dixit7300"/>
          <machine name="shoit70s0"/>

	  New:
          <webHost nameRegex="mywebsitequal.*"/>
          <machine nameRegex="(sho|dix)(is|it).*"/>

Sentry.Configuration 20121218  2.0.7.3 (CLR version 2.0.5.0)
=========================================
Bug fixes
    - Changed the method with which Sentry.Configuration determines web application's root
	  directory in order to load imported files correctly (fixes problem when application is
	  running from "Temporary ASP.NET Files" directory)

Sentry.Configuration 20121031  2.0.7.1 (CLR version 2.0.5.0)
=========================================
Bug fixes
    - Provided additional information in the exception message when Sentry.Configuration can't load 
	  a file referenced in an importSettings element

Sentry.Configuration 20121026  2.0.7.0 (CLR version 2.0.5.0)
=========================================
New features
	- Allows for custom filter elements under an environment signature, including
	  HttpHeaderPresent and HttpHeaderAbsent

Sentry.Configuration 20121015  2.0.5.1 (CLR version 2.0.5.0)
=========================================
Bug fixes
    - Fixed bug which occurred when two threads were attempting to load an external value file at the
	  exact same moment, causing an "Index was outside the bounds of the array." exception.

Sentry.Configuration 20111111  2.0.4.0
=========================================
Bug fixes
	- webHost environments now take precedence over machine environments.
	- webHost overrides will no longer throw null reference exceptions under certain circumstances.
	- when a default setting is overridden for a specific environment, remaining default environment settings 
	  are now retained
	- regular expression evaluation for path has been changed to ignore case.  Previously, ZENA job entry with 
	  non case matched path would not evaluate correctly.

New features
	- new shared method GetDefaultEnvironmentName that will return the default environment in use.
	- better error message details when an item is not found.

Sentry.Configuration  20110224	2.0.0.0
===============================================
Significant enhancements to the Sentry.Configuration framework. Imported settings support added.  External value
files now support encryption.  Default Environment List support added.  ExecutablePathRegex feature added to
better support console apps.  Ability to read in portions of external values files added.

For more details, please see the Architecture Blog post entitied "Major Enhancements to Sentry.Configuration
framework", dated Feb 24, 2011 (http://sharepoint/IT/AppSvcs/architecture/Archblog/Lists/Posts/AllPosts.aspx)



Sentry.Configuration  20100115  1.0.2.0
===============================================
Feature 0002

Sentry.Configuration now supports storing "sensitive" information (such as connection strings with passwords) in
"external value files".  This allows a separation between non-sensitive configuration (which can be stored as part
of a Visual Studio solution in source control) and sensitive configuration (which is stored solely on the app server).
Please see the blog post on the SharePoint site for more information.



Sentry.Configuration  20080424  1.0.1.0
===============================================
Bug Fix 0001

Situation: When adding XML comments into the sentrySettings section of your *.config file, you could get a run-time error.

Fix:       Fixed the configuration parser to ignore comments.  You can now add comments to your *.config files within the
           sentrySettings section.

   
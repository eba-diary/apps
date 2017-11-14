::--------------------------------------------------------------------------------------
::	Start WindowsService
::	This section will start the services for the local machine
::--------------------------------------------------------------------------------------

:ENABLESERVICES
sc config Sentry.data.Goldeneye start= auto > tempfile 2>&1
@IF %ERRORLEVEL% NEQ 0 GOTO ENABLEFAILURE 

:STARTSERVICE
sc start Sentry.data.Goldeneye 2> tempfile
@IF %ERRORLEVEL% NEQ 0 IF %ERRORLEVEL% NEQ 1056 GOTO STARTFAILURE

:SUCCESS
SET ERRLVL=0
GOTO Quit


::--------------------------------------------------------------------------------------
::	Error Handling
::	The following sections are used for error handling.  Certain sections will 
::	forward execution to these areas to handle and log the error that the 
::	script encountered.
::--------------------------------------------------------------------------------------
:ENABLEFAILURE
@ECHO:
@ECHO One of the services failed to start successfully.
@ECHO:
@ECHO Results of last command before quitting are below
@ECHO =================================================
@TYPE tempfile
@SET ERRLVL=1
@GOTO Quit

:STARTFAILURE
@ECHO:
@ECHO A service failed to start successfully.
@ECHO An error code of 1056, which indicates that the service
@ECHO is already started, is acceptable.
@ECHO:
@ECHO Results of last command before quitting are below
@ECHO =================================================
@TYPE tempfile
@SET ERRLVL=1
@GOTO Quit


::--------------------------------------------------------------------------------------
::	Quit
::	This section is the end of the script.  It's only function is to 
::	return back to the environment the error level of the script, 
::	indicating either success or failure.
::--------------------------------------------------------------------------------------
:Quit
@del tempfile
@SET ERRORLEVEL=%ERRLVL%
@EXIT %ERRLVL%


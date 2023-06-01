DECLARE @ScriptVersion AS VARCHAR(50); 
DECLARE @ErrorMessage NVARCHAR(4000); 
DECLARE @ErrorSeverity INT; 
DECLARE @ErrorState INT; 

--Execute scripts intended to run before post-deploy scripts
:r ..\Pre-Deploy\SupportingScripts\Release_03_02_36\CLA-5277_RemoveSentryCategory.sql
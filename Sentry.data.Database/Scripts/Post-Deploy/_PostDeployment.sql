/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]
			   
All new files added for staic data or scripts should have it's properties updated to set build options of NONE.
--------------------------------------------------------------------------------------
*/

--Execute scripts to insert/delete/merge static data
:r ..\Post-Deploy\StaticData\Category.sql




--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion AS VARCHAR(50) 
SET @ScriptVersion = '2019.01.15_01'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\MTP_2019_01_15\UpdateSomeRow.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 

END TRY 
BEGIN CATCH 
    DECLARE @ErrorMessage NVARCHAR(4000); 
    DECLARE @ErrorSeverity INT; 
    DECLARE @ErrorState INT; 
  
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(), 
        @ErrorSeverity = ERROR_SEVERITY(), 
        @ErrorState = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage, 
               @ErrorSeverity, 
               @ErrorState 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN


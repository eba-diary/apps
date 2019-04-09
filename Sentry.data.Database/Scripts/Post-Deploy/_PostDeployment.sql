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
:r ..\Post-Deploy\StaticData\EventType.sql
:r ..\Post-Deploy\StaticData\Permission.sql
:r ..\Post-Deploy\StaticData\BusinessUnit.sql
:r ..\Post-Deploy\StaticData\DatasetFunction.sql
:r ..\Post-Deploy\StaticData\DataAsset.sql
:r ..\Post-Deploy\StaticData\BusinessArea.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTile.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTileRow.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTileRow_BusinessAreaTile.sql


--Now only run these scripts if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion AS VARCHAR(50); 
DECLARE @ErrorMessage NVARCHAR(4000); 
DECLARE @ErrorSeverity INT; 
DECLARE @ErrorState INT; 

DECLARE @ScriptVersion AS VARCHAR(50) 
SET @ScriptVersion = '2019.02.4_01_PostDeploy'


BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 

END TRY 

BEGIN CATCH 
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(), 
        @ErrorSeverity = ERROR_SEVERITY(), 
        @ErrorState = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 

COMMIT TRAN


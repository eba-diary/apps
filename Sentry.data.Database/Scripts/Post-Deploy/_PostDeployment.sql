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
:r ..\Post-Deploy\StaticData\BusinessUnit.sql
:r ..\Post-Deploy\StaticData\DatasetFunction.sql




--Now only run these scripts if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion AS VARCHAR(50) 
SET @ScriptVersion = '2019.01.29_01_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_1\InsertDatasetCategories.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\BusinessUnitChanges.sql

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

--Now only run these scripts if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion2 AS VARCHAR(50) 
SET @ScriptVersion2 = '2019.02.4_01_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion2) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\AddFileExtension.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion2, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @ErrorMessage2 NVARCHAR(4000); 
    DECLARE @ErrorSeverity2 INT; 
    DECLARE @ErrorState2 INT; 
  
    SELECT 
        @ErrorMessage2 = ERROR_MESSAGE(), 
        @ErrorSeverity2 = ERROR_SEVERITY(), 
        @ErrorState2 = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage2, 
               @ErrorSeverity2, 
               @ErrorState2 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 

COMMIT TRAN
--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion3 AS VARCHAR(50) 
SET @ScriptVersion3 = '2019.02.13_01_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion3) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_1\InitializeGetLatest.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion3, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @ErrorMessage3 NVARCHAR(4000); 
    DECLARE @ErrorSeverity3 INT; 
    DECLARE @ErrorState3 INT; 
  
    SELECT 
        @ErrorMessage3 = ERROR_MESSAGE(), 
        @ErrorSeverity3 = ERROR_SEVERITY(), 
        @ErrorState3 = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage3, 
               @ErrorSeverity3, 
               @ErrorState3 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion4 AS VARCHAR(50) 
SET @ScriptVersion4 = '2019.02.13_02_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion4) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\ReAlignTableauReportTypeIdsql.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion4, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @ErrorMessage4 NVARCHAR(4000); 
    DECLARE @ErrorSeverity4 INT; 
    DECLARE @ErrorState4 INT; 
  
    SELECT 
        @ErrorMessage4 = ERROR_MESSAGE(), 
        @ErrorSeverity4 = ERROR_SEVERITY(), 
        @ErrorState4 = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage4, 
               @ErrorSeverity4, 
               @ErrorState4 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN
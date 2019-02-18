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


--Now only run these scripts if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ErrorMessage NVARCHAR(4000); 
DECLARE @ErrorSeverity INT; 
DECLARE @ErrorState INT; 

DECLARE @ScriptVersion AS VARCHAR(50) 
SET @ScriptVersion = '2019.02.4_01_PostDeploy'


BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\AddFileExtension.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\BusinessUnitChanges.sql

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

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
SET @ScriptVersion = '2019.02.13_01_PostDeploy'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_1\InitializeGetLatest.sql

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

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
SET @ScriptVersion = '2019.02.13_02_PostDeploy'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\ReAlignTableauReportTypeIdsql.sql

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

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion5 AS VARCHAR(50) 
SET @ScriptVersion5 = '2019.02.13_03_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion5) 
BEGIN TRY 

  --insert one off script files here
	:r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\UpdateJavaApplicationOptionstoArray.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion5, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @ErrorMessage5 NVARCHAR(4000); 
    DECLARE @ErrorSeverity5 INT; 
    DECLARE @ErrorState5 INT; 
  
    SELECT 
        @ErrorMessage5 = ERROR_MESSAGE(), 
        @ErrorSeverity5 = ERROR_SEVERITY(), 
        @ErrorState5 = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage5, 
               @ErrorSeverity5, 
               @ErrorState5 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN



--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion6 AS VARCHAR(50) 
SET @ScriptVersion6 = '2019.02.13_04_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion6) 
BEGIN TRY 

  --insert one off script files here
	:r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\AddHiveLocation.sql

  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion6, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @ErrorMessage6 NVARCHAR(4000); 
    DECLARE @ErrorSeverity6 INT; 
    DECLARE @ErrorState6 INT; 
  
    SELECT 
        @ErrorMessage6 = ERROR_MESSAGE(), 
        @ErrorSeverity6 = ERROR_SEVERITY(), 
        @ErrorState6 = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage6, 
               @ErrorSeverity6, 
               @ErrorState6 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
SET @ScriptVersion = '2019.02.15_01_PostDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_19_2_2\SetPrimaryContactId.sql

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
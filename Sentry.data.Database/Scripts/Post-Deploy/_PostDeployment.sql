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
:r ..\Post-Deploy\StaticData\AuthenticationType.sql
:r ..\Post-Deploy\StaticData\DatasetFunction.sql
:r ..\Post-Deploy\StaticData\DataSourceType.sql
:r ..\Post-Deploy\StaticData\DataSource.sql
:r ..\Post-Deploy\StaticData\DatasetScopeTypes.sql
:r ..\Post-Deploy\StaticData\Security.sql
:r ..\Post-Deploy\StaticData\DataAsset.sql
:r ..\Post-Deploy\StaticData\BusinessArea.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTile.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTileRow.sql
:r ..\Post-Deploy\StaticData\BusinessAreaTileRow_BusinessAreaTile.sql
:r ..\Post-Deploy\StaticData\DataActionTypes.sql
:r ..\Post-Deploy\StaticData\DataAction.sql
:r ..\Post-Deploy\StaticData\ObjectStatus.sql
:r ..\Post-Deploy\StaticData\DataFlowCompressionTypes.sql
:r ..\Post-Deploy\StaticData\DataFlowPreProcessingTypes.sql
:r ..\Post-Deploy\StaticData\StatusType.sql
:r ..\Post-Deploy\StaticData\FileExtension.sql
:r ..\Post-Deploy\StaticData\FeatureEntity.sql


--Create Sequences in Database if they don't exist
--These should be defined in the Dacpac, but there are problems with Sequences in Dacpacs
--See https://stackoverflow.com/questions/34019442/dacpac-and-sql-sequence
IF NOT EXISTS(SELECT * FROM sys.sequences WHERE name='seq_DataFlowStorageCDE')
BEGIN
    SELECT 'Creating Sequence seq_DataFlowStorageCDE...'
    CREATE SEQUENCE [dbo].[seq_DataFlowStorageCDE]
        AS BIGINT
        START WITH 1
        INCREMENT BY 1
END;
IF NOT EXISTS(SELECT * FROM sys.sequences WHERE name='seq_StorageCDE')
BEGIN
    SELECT 'Creating Sequence seq_StorageCDE...'
    CREATE SEQUENCE [dbo].[seq_StorageCDE]
        AS BIGINT
        START WITH 1000501
        INCREMENT BY 1
END;


--Now only run these scripts if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
DECLARE @ScriptVersion AS VARCHAR(50); 
DECLARE @ErrorMessage NVARCHAR(4000); 
DECLARE @ErrorSeverity INT; 
DECLARE @ErrorState INT; 

--Now only run these scritps if the versioning allows us.
--ALTER THE SCRIPT VERSION BELOW FOR EVERY NEW SCRIPT 
--SCRIPT VERSION should be in format yyyy.MM.dd_rr where rr is 2-digit revision number for day. 
SET @ScriptVersion = '2021.07.21.01_PostDeploy'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_03_06\CLA_2946_Initialize_DataFlow_IngestionType_Column.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_03_06\CLA_2946_Initialize_DataFlow_IsDecompressionRequired_Column.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_03_06\CLA_2946_Initialize_DataFlow_CompressionType_Column.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_03_06\CLA_2946_Initialize_DataFlow_PreProcessing_Columns.sql


  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 

END TRY 

BEGIN CATCH 
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(), 
        @ErrorSeverity = ERROR_SEVERITY(), 
        @ErrorState = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState ); 
  
    ROLLBACK TRAN; 
    RETURN;
END CATCH 

COMMIT TRAN


SET @ScriptVersion = '2021.08.31.01_PostDeploy'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_04_03\HistoryFix_Populate_DataFlow_NamedEnvironments.sql
  :r ..\Post-Deploy\SupportingScripts\Sprint_21_04_03\HistoryFix_Populate_DataFlowStep_TriggerBucketTargetBucket.sql

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
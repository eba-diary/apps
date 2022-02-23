﻿/*
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

--Execute scripts to do history fixes / bulk updates
DECLARE @ScriptVersion AS VARCHAR(50); 
DECLARE @ErrorMessage NVARCHAR(4000); 
DECLARE @ErrorSeverity INT; 
DECLARE @ErrorState INT; 

--Insert Post-Deploy SupportingScript references here
:r ..\Post-Deploy\SupportingScripts\Sprint_21_04_06\HistoryFix_Initialize_ParquetStorageBucket_on_Schema.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_21_04_06\HistoryFix_Initialize_ParquetStoragePrefix_on_Schema.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_21_04_06\HistoryFix_Revert_ProducerS3Drop_Step_TargetBucket_Metadata.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_21_04_06\CLA-3398-Delete_Database_Feature_Flags_that_have_been_moved_to_LaunchDarkly.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_21_04_06\HistoryFix_UPDATE_SCHEMA_SNOWFLAKESTAGE.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_22_01_01\Remove_CLA3332_ConsolidatedDataFlows_For_LaunchDarkly_Implementation.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_22_01_01\Remove_CLA3497_UniqueLivyNames_For_LaunchDarkly_Implementation.sql
:r ..\Post-Deploy\SupportingScripts\Sprint_22_01_01\HOTFIX-CLA-3556-DEFAULT-EXHIBIT-SAID-ASSET.sql
:r ..\Post-Deploy\SupportingScripts\Release_02_00_03\CLA-3465-HISTORY-FIX-SCHEMA-SnowflakeWarehouse.sql
:r ..\Post-Deploy\SupportingScripts\Release_02_00_03\CLA3606_Move_CLA3240_UseDropLocationV2_to_LaunchDarkly.sql
:r ..\Post-Deploy\SupportingScripts\Release_02_00_07\CLA-3729-HISTORY-FIX-EVENT-Reason.sql
:r ..\Post-Deploy\SupportingScripts\Release_02_00_07\CLA-3729-HISTORY-FIX-EVENT-IsProcessed.sql
:r ..\post-deploy\supportingscripts\sprint_22_02_01\historyfix_cla3790saidassetdataflow.sql
:r ..\post-deploy\supportingscripts\sprint_22_02_01\historyfix_cla3790saidassetdatasets.sql
:r ..\post-deploy\supportingscripts\Release_03_00_03\CLA3722_TransitionToDatasetAsset.sql
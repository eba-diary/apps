SET @ScriptVersion = 'CLA3991-InitializeDataflowSecurity'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

	DECLARE @RC INT
    DECLARE @msg varchar(250)
	DECLARE @UniqueSecIdEntries INT
	DECLARE @NoSecIdCount INT
	DECLARE @SecIdAssociatedWithMultipleFlows INT
	DECLARE @DataflowUpdateCount INT


	/* Create Temp table with new columns */
	IF OBJECT_ID('tempdb..#TempDF') IS NOT NULL DROP TABLE #TempDf
	--CREATE TABLE #TempDf(
	--	[Id] [int] NOT NULL,
	--	[FlowGuid] [uniqueidentifier] NOT NULL,
	--	[Name] [varchar](250) NOT NULL,
	--	[Create_DTM] [datetime] NOT NULL,
	--	[CreatedBy] [varchar](10) NOT NULL,
	--	[Questionnaire] [varchar](max) NULL,
	--	[FlowStorageCode] [varchar](7) NULL,
	--	[SaidKeyCode] [varchar](10) NULL,
	--	[ObjectStatus] [int] NOT NULL,
	--	[DeleteIssuer] [varchar](10) NULL,
	--	[DeleteIssueDTM] [datetime] NOT NULL,
	--	[UserDropLocationBucket] [varchar](1000) NULL,
	--	[UserDropLocationPrefix] [varchar](1000) NULL,
	--	[NamedEnvironment] [varchar](25) NULL,
	--	[NamedEnvironmentType] [varchar](25) NULL,
	--	[IngestionType] [int] NULL,
	--	[IsDecompressionRequired] [bit] NULL,
	--	[CompressionType] [int] NULL,
	--	[IsPreProcessingRequired] [bit] NULL,
	--	[PreProcessingOption] [int] NULL,
	--	[DatasetId] [int] NULL,
	--	[SchemaId] [int] NULL
	--	,PrimaryContact_id VARCHAR(8) NULL
	--	,ISSecure bit null
	--	,Security_Id UniqueIdentifier null
	--	)

	/* Populate temp table with null values for new columns*/
	select 
	*
	into #TempDF 
	from DataFlow

	/***************************************************************************************** 
		Distinct Dataset\Schema Id's for all dataflows 
		Create New Security Id for each row

			select * from #UniqueDFDatasetSchemaIds
	*****************************************************************************************/
	IF OBJECT_ID('tempdb..#UniqueDFDatasetSchemaIds') IS NOT NULL DROP TABLE #UniqueDFDatasetSchemaIds
	Select Distinct DatasetId as 'DF_DS_Id', SchemaId as 'DF_SCM_Id', NEWID() as 'DF_Sec_Id'
	into #UniqueDFDatasetSchemaIds
	from #TempDF 
	where DatasetID <> 0 and SchemaId <> 0


	/* Update Dataflow metadata with new Security Id values */
	UPDATE #TempDF
	Set PrimaryContact_ID = CreatedBy, IsSecured_IND = 1, Security_ID = x.DF_Sec_Id
	from #UniqueDFDatasetSchemaIds x
	where DatasetId = x.DF_DS_Id and SchemaId = x.DF_SCM_Id


	/******************************************* 
		SELECT V2 Dataflows 
		
			select * from DataFlow order by DatasetID, SchemaID, ID DESC
			Select * from #TempDF
			Select * from #OldDataflowConfiguration
			select * from #V2Dataflows
			select distinct V2_DF_ID from #v2Dataflows
	********************************************/

	IF OBJECT_ID('tempdb..#V2Dataflows') IS NOT NULL DROP TABLE #V2Dataflows
	select 
	DF.Name as 'V2_DF_NAME',
	DF.Id as 'V2_DF_ID',
	DFS.Id as 'V2_STEP_ID',
	DSTS.SchemaId as 'V2_DF_Schema_ID',
	DFC.Dataset_ID as 'V2_DF_Dataset_ID',
	null as 'V2_DF_Sec_Id'
	into #V2Dataflows
	from #TempDf DF
	join DataFlowStep DFS on
		DF.Id = DFS.DataFlow_Id
	left join DataStepToSchema DSTS on
		DFS.Id = DSTS.DataFlowStepId
	left join DatasetFileConfigs DFC on
		DFC.Schema_Id = DSTS.SchemaId
	where
		DF.DatasetId = 0 and 
		DF.SchemaId = 0 and
		DFS.DataAction_Type_Id = 8 /* SchemaMap */
	order by DF.Name


	/******************************************* 
		V2 dataflows which fan-out to multiple schema
		Script will associate each of these with it's own schema record

			select * from #V2FanOutFlows
			select distinct #V2Dataflows.DF_ID from #V2Dataflows left join #V2FanOutFlows on #V2Dataflows.DF_ID = #V2FanOutFlows.DF_ID where #V2FanOutFlows.DF_ID is not null	-- will filter out 53 records (10 unique Dataflow Id's)
	*******************************************/
	IF OBJECT_ID('tempdb..#V2FanOutFlows') IS NOT NULL DROP TABLE #V2FanOutFlows
	select V2_DF_ID, V2_DF_Dataset_Id, NEWID() as 'V2_DF_Sec_id'
	into #V2FanOutFlows
	from #V2Dataflows
	group by V2_DF_ID,  V2_DF_Dataset_ID
	having count(*) > 1


	UPDATE #TempDf
	SET PrimaryContact_ID = CreatedBy, IsSecured_IND = 1, Security_ID = #V2FanOutFlows.V2_DF_Sec_id
	from #V2FanOutFlows
	where #TempDf.Id = #V2FanOutFlows.V2_DF_ID



	IF OBJECT_ID('tempdb..#V2DataFlowUpdates') IS NOT NULL DROP TABLE #V2DataFlowUpdates
	select 
		#V2Dataflows.V2_DF_ID, #V2Dataflows.V2_DF_Dataset_ID, #TempDf.Security_Id, COUNT(*) as 'V3_Refs'
	into #V2DataFlowUpdates
	from #TempDf
	left join #V2Dataflows on
		#TempDf.DatasetId = #V2Dataflows.V2_DF_Dataset_ID
		and #TempDf.SchemaId = #V2Dataflows.V2_DF_Schema_ID
	left join #V2FanOutFlows on
		#V2Dataflows.V2_DF_ID = #V2FanOutFlows.V2_DF_ID
	where 
		#V2FanOutFlows.V2_DF_ID is null		/* Filter out V2 Fanout rows */
		and #V2Dataflows.V2_DF_ID is not null
	group by #V2Dataflows.V2_DF_ID, #V2Dataflows.V2_DF_Dataset_ID, #TempDf.Security_Id


	UPDATE #TempDf
	SET PrimaryContact_ID = CreatedBy, IsSecured_IND = 1, Security_ID = #V2DataFlowUpdates.Security_Id
	FROM #V2DataFlowUpdates
	where #TempDf.Id = #V2DataFlowUpdates.V2_DF_ID


	/************************************************
		Assign all v2 FileSchemaFlow dataflows with unique security id
			to ensure dataflow Security_Id column can be 
			set to not null
	***********************************************/
	update #TempDf
	SET PrimaryContact_ID = CreatedBy, IsSecured_IND = 1, Security_ID = NEWID()
	where Name like 'FileSchemaFlow%'

	/************************************************ 
		Assign all v2 deleted dataflows with unique security Id
			to ensure dataflow Security_Id column can be 
			set to not null
	***********************************************/

	--select * from #TempDf 
	update #TempDf Set PrimaryContact_ID = CreatedBy, IsSecured_IND = 1, Security_ID = NEWID()
	where (DatasetId = 0 or DatasetId is null) and (SchemaId = 0 or SchemaId is null) and Security_Id is null



	IF OBJECT_ID('tempdb..#UniqueDFSecurityKeys') IS NOT NULL DROP TABLE #UniqueDFSecurityKeys
	select distinct #TempDf.Security_ID into #UniqueDFSecurityKeys from #TempDf left join Security on #TempDf.Security_ID = Security.Security_ID where Security.Security_Id is null and #TempDf.Security_Id is not null

	/**********************************************************
		VALIDATIONS
	**********************************************************/
	SET @DataflowUpdateCount = (select count(*) from DataFlow)
	SET @UniqueSecIdEntries = (select count(*) from #UniqueDFSecurityKeys)

	--'How many dataflows do not have security id?'
	SET @NoSecIdCount = (SELECT COUNT(*) FROM #TempDf WHERE Security_ID IS NULL)

	/**************************************
	select * from #TempDf where Security_Id is null and Name not like 'FileSchemaFlow%' and ObjectStatus = 1
	select [schema].schema_nme, datasteptoschema.*, dataflowstep.* from DataStepToSchema join DataFlowStep on datasteptoschema.dataflowstepid = dataflowstep.id join [schema] on datasteptoschema.schemaid =[schema].schema_id where dataflowstep.DataFlow_Id in (121)
	select * from Dataset where dataset_id = 190
	**************************************/


	/************************************** 
		Are there any V3 dataflows with a null security id? 
	
		select count(*) as 'Are there any V3 dataflows with a null security id?' from #TempDf where DatasetId <> 0 and SchemaId <> 0 and ObjectStatus = 1 and Security_Id is null
		SELECT ObjectStatus, Count(*) as 'Break down of V3 dataflows with null security_id' from #TempDf where DatasetId <> 0 and SchemaId <> 0 and Security_Id is null group by ObjectStatus
	**************************************/


	/* Are there multiple active V3 dataflows, populating same dataset/schema, associated with same Security_Id?*/
	SET @SecIdAssociatedWithMultipleFlows = (select COUNT(*)
			from (select * 
					from #TempDf 
					where 
						DatasetId <> 0 
						and SchemaId <> 0 
						and ObjectStatus = 1 
						and Security_ID in (
							select Security_Id 
							/*select */
							from #TempDf 
							where 
								DatasetId <> 0 
								and SchemaId <> 0 
								and ObjectStatus = 1 
							group by Security_Id 
							having COUNT(*) > 1
							) 
							/*order by Security_Id, Name, Create_DTM*/
					) x
			)

	/* 
		How many v2 dataflows do not have security_id 	
	
		select * from #TempDf where DatasetId = 0 and SchemaId = 0 and Security_Id is null
		select ObjectStatus, count(*) from #TempDf where DatasetId = 0 and SchemaId = 0 and Security_Id is null group by ObjectStatus
	*/


	if @NoSecIdCount > 0 THROW 50001, 'Dataflow w/o Security_Id - Script did not propertly assign all dataflows a Security_Id value', 1
	if @SecIdAssociatedWithMultipleFlows > 0 THROW 50001, 'Security_Id not unique - Multiple dataflows are associated with same Security_Id', 1

	INSERT INTO [Security]
		([Security_ID]
		,[SecurableEntity_NME]
		,[Created_DTM]
		,[Enabled_DTM]
		,[Removed_DTM]
		,[UpdatedBy_ID]
		,[CreatedBy_ID])
	SELECT #UniqueDFSecurityKeys.Security_Id, 'DataFlow', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, null, 'SYSTEM' FROM #UniqueDFSecurityKeys

	SET @RC = @@ROWCOUNT
	SET @msg  = 'Expected ' + CAST(@UniqueSecIdEntries as varchar(11)) + 'Security inserts, actual was ' + CAST(@RC as varchar(11)) + '.'
	if (@RC <> @UniqueSecIdEntries) THROW 50001, @msg, 1


	UPDATE DATAFLOW
	SET	PrimaryContact_ID = #TempDf.PrimaryContact_ID
	FROM #TempDf
	WHERE dataflow.Id = #TempDf.Id and DATAFLOW.PrimaryContact_ID is null

	SET @RC = @@ROWCOUNT
	SET @msg  = 'Expected ' + CAST(@DataflowUpdateCount as varchar(11)) + 'Dataflow PrimaryContact_ID updates, actual was ' + CAST(@RC as varchar(11)) + '.'
	if (@RC <> @DataflowUpdateCount) THROW 50001, @msg, 1

	UPDATE DATAFLOW
	SET	IsSecured_IND = #TempDf.IsSecured_IND
	FROM #TempDf
	WHERE dataflow.Id = #TempDf.Id and DATAFLOW.IsSecured_IND is null

	SET @RC = @@ROWCOUNT
	SET @msg  = 'Expected ' + CAST(@DataflowUpdateCount as varchar(11)) + 'Dataflow IsSecured_IND updates, actual was ' + CAST(@RC as varchar(11)) + '.'
	if (@RC <> @DataflowUpdateCount) THROW 50001, @msg, 1

	UPDATE DATAFLOW
	SET	Security_ID = #TempDf.Security_ID
	FROM #TempDf
	WHERE dataflow.Id = #TempDf.Id and DATAFLOW.Security_ID is null

	SET @RC = @@ROWCOUNT
	SET @msg  = 'Expected ' + CAST(@DataflowUpdateCount as varchar(11)) + 'Dataflow Security_ID updates, actual was ' + CAST(@RC as varchar(11)) + '.'
	if (@RC <> @DataflowUpdateCount) THROW 50001, @msg, 1



    -- END POST-DEPLOY SCRIPT --
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


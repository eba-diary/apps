/***********************
*  History fix to switch existing S3Drop action, on producer data flows, to ProducerS3Drop action
*    All producer dataflows will be assigned to the DATA asset key code.  Intention is for this 
*    to be run in TEST and DEV environments.
*  
*  Change history
*	1.0		1/6		initial creation
*	1.1		1/14	Split Raw Storage and S3 Drop updates, adjusted @OldDropPrefix__CLA2413 logic
*   1.2		3/8		Added populated all producer flows into  #FlowsFromSpreadsheet with DATA asset key code
***********************/
--Use SentryDatasets_NR

IF OBJECT_ID('tempdb..#FlowsFromSpreadsheet') IS NOT NULL DROP TABLE #FlowsFromSpreadsheet
IF OBJECT_ID('tempdb..#ExistingFlows') IS NOT NULL DROP TABLE #ExistingFlows
IF OBJECT_ID('tempdb..#S3DropUpdates') IS NOT NULL DROP TABLE #S3DropUpdates
IF OBJECT_ID('tempdb..#RawStorageUpdates') IS NOT NULL DROP TABLE #RawStorageUpdates

DECLARE @ENV__CLA2413 VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
DECLARE @NewSourceBucket__CLA2413 VARCHAR(40) = (select TargetStorageBucket from DataAction where ActionType = 'ProducerS3Drop')
DECLARE @NewProducerS3DropActionId__CLA2413 INT = (Select Id from DataAction where Name = 'Producer S3 Drop')
DECLARE @OldDropPrefix__CLA2413 VARCHAR(25)

Select @ENV__CLA2413

if (@ENV__CLA2413 = 'PROD' or @ENV__CLA2413 = 'QUAL')
BEGIN
	SET @OldDropPrefix__CLA2413 = 'droplocation/data/'
END
else if (@ENV__CLA2413 = 'NRTEST' or @ENV__CLA2413 = 'TEST')
BEGIN
	SET @OldDropPrefix__CLA2413 = 'droplocation/data-test/'
END
else if (@ENV__CLA2413 = 'NRDEV' or @ENV__CLA2413 = 'DEV')
BEGIN
	SET @OldDropPrefix__CLA2413 = 'droplocation/data-dev/'
END
else
BEGIN
	SET @OldDropPrefix__CLA2413 = 'droplocation/data-' + LOWER(@ENV__CLA2413) + '/'
END

select 'OldDropPrefix: ' + @OldDropPrefix__CLA2413
/**********************************
	Create temp table, contains Id's of all dataflows
	 to be adjusted and associated SAID ASSET key code

***********************************/
Create Table #FlowsFromSpreadsheet
( DataFlowID Int not null,
  SaidAsset varchar(10) not null
)

/***********************************
  Insert statements for #ExistingFlows.
    All dataflows below 

	select * from #FlowsFromSpreadsheet
***********************************/
/*<replace_with_insert_statements_for_existingflows_table>*/

if (@ENV__CLA2413 = 'TEST' or @ENV__CLA2413 = 'NRTEST' or @ENV__CLA2413 = 'DEV' or @ENV__CLA2413 = 'NRDEV')
BEGIN
	insert into #FlowsFromSpreadsheet
	select
	 DF.Id,
	 'DATA'
	from DataFlow DF
	join DataFlowStep DFS on
		DF.Id = DFS.DataFlow_Id
	join DataAction DA on
		DA.Id = DFS.Action_Id
	where DA.Name = 'Schema Map'
END
else if (@ENV__CLA2413 = 'QUAL')
BEGIN
	insert into #FlowsFromSpreadsheet select 15,	'DATA'
	insert into #FlowsFromSpreadsheet select 19,	'DATA'
	insert into #FlowsFromSpreadsheet select 24,	'DATA'
	insert into #FlowsFromSpreadsheet select 40,	'CONE'
	insert into #FlowsFromSpreadsheet select 42,	'ESIG'
	insert into #FlowsFromSpreadsheet select 44,	'ESIG'
	insert into #FlowsFromSpreadsheet select 48,	'DATA'
	insert into #FlowsFromSpreadsheet select 48,	'DATA'
	insert into #FlowsFromSpreadsheet select 48,	'DATA'
	insert into #FlowsFromSpreadsheet select 61,	'DATA'
	insert into #FlowsFromSpreadsheet select 63,	'DATA'
	insert into #FlowsFromSpreadsheet select 65,	'DATA'
	insert into #FlowsFromSpreadsheet select 66,	'DATA'
	insert into #FlowsFromSpreadsheet select 72,	'DATA'
	insert into #FlowsFromSpreadsheet select 75,	'DATA'
	insert into #FlowsFromSpreadsheet select 77,	'DATA'
	insert into #FlowsFromSpreadsheet select 84,	'DATA'
	insert into #FlowsFromSpreadsheet select 89,	'DATA'
	insert into #FlowsFromSpreadsheet select 93,	'DATA'
	insert into #FlowsFromSpreadsheet select 96,	'DATA'
	insert into #FlowsFromSpreadsheet select 97,	'DATA'
	insert into #FlowsFromSpreadsheet select 98,	'DATA'
	insert into #FlowsFromSpreadsheet select 105,	'DATA'
	insert into #FlowsFromSpreadsheet select 107,	'DATA'
	insert into #FlowsFromSpreadsheet select 110,	'DATA'
	insert into #FlowsFromSpreadsheet select 111,	'DATA'
	insert into #FlowsFromSpreadsheet select 114,	'DATA'
	insert into #FlowsFromSpreadsheet select 115,	'DATA'
	insert into #FlowsFromSpreadsheet select 116,	'DATA'
	insert into #FlowsFromSpreadsheet select 155,	'DATA'
	insert into #FlowsFromSpreadsheet select 157,	'DATA'
	insert into #FlowsFromSpreadsheet select 163,	'FNOL'
	insert into #FlowsFromSpreadsheet select 163,	'FNOL'
	insert into #FlowsFromSpreadsheet select 181,	'DATA'
	insert into #FlowsFromSpreadsheet select 182,	'DATA'
	insert into #FlowsFromSpreadsheet select 185,	'DATA'
	insert into #FlowsFromSpreadsheet select 192,	'DATA'
	insert into #FlowsFromSpreadsheet select 206,	'DATA'
	insert into #FlowsFromSpreadsheet select 212,	'DATA'
	insert into #FlowsFromSpreadsheet select 216,	'DATA'
	insert into #FlowsFromSpreadsheet select 218,	'DATA'
	insert into #FlowsFromSpreadsheet select 220,	'DATA'
	insert into #FlowsFromSpreadsheet select 222,	'DATA'
	insert into #FlowsFromSpreadsheet select 224,	'DATA'
	insert into #FlowsFromSpreadsheet select 228,	'DATA'
	insert into #FlowsFromSpreadsheet select 230,	'DATA'
	insert into #FlowsFromSpreadsheet select 233,	'PLBI'
	insert into #FlowsFromSpreadsheet select 241,	'SSPO'
	insert into #FlowsFromSpreadsheet select 242,	'SSPO'
	insert into #FlowsFromSpreadsheet select 243,	'SSPO'
	insert into #FlowsFromSpreadsheet select 244,	'SSPO'
	insert into #FlowsFromSpreadsheet select 245,	'SSPO'
	insert into #FlowsFromSpreadsheet select 246,	'SSPO'
	insert into #FlowsFromSpreadsheet select 247,	'DATA'
	insert into #FlowsFromSpreadsheet select 250,	'CONE'
	insert into #FlowsFromSpreadsheet select 294,	'RTFY'
	insert into #FlowsFromSpreadsheet select 297,	'DATA'
	insert into #FlowsFromSpreadsheet select 299,	'DATA'
	insert into #FlowsFromSpreadsheet select 300,	'DATA'
	insert into #FlowsFromSpreadsheet select 301,	'DATA'
	insert into #FlowsFromSpreadsheet select 302,	'SDOC'
	insert into #FlowsFromSpreadsheet select 303,	'DATA'
	insert into #FlowsFromSpreadsheet select 305,	'DATA'
	insert into #FlowsFromSpreadsheet select 325,	'ACLM'
	insert into #FlowsFromSpreadsheet select 326,	'ACLM'
	insert into #FlowsFromSpreadsheet select 327,	'ACLM'
	insert into #FlowsFromSpreadsheet select 328,	'ACLM'
	insert into #FlowsFromSpreadsheet select 329,	'ACLM'
	insert into #FlowsFromSpreadsheet select 330,	'ACLM'
	insert into #FlowsFromSpreadsheet select 331,	'ACLM'
	insert into #FlowsFromSpreadsheet select 334,	'CTEL'
	insert into #FlowsFromSpreadsheet select 335,	'FNOL'
	insert into #FlowsFromSpreadsheet select 336,	'FNOL'
	insert into #FlowsFromSpreadsheet select 337,	'FNOL'
	insert into #FlowsFromSpreadsheet select 338,	'FNOL'
	insert into #FlowsFromSpreadsheet select 339,	'FNOL'
	insert into #FlowsFromSpreadsheet select 340,	'FNOL'
	insert into #FlowsFromSpreadsheet select 341,	'FNOL'
	insert into #FlowsFromSpreadsheet select 342,	'FNOL'
	insert into #FlowsFromSpreadsheet select 343,	'FNOL'
	insert into #FlowsFromSpreadsheet select 344,	'FNOL'
	insert into #FlowsFromSpreadsheet select 345,	'FNOL'
	insert into #FlowsFromSpreadsheet select 346,	'FNOL'
	insert into #FlowsFromSpreadsheet select 347,	'DATA'
	insert into #FlowsFromSpreadsheet select 349,	'DATA'
	insert into #FlowsFromSpreadsheet select 352,	'CTEL'
	insert into #FlowsFromSpreadsheet select 355,	'DATA'
	insert into #FlowsFromSpreadsheet select 360,	'DATA'
	insert into #FlowsFromSpreadsheet select 362,	'DATA'
	insert into #FlowsFromSpreadsheet select 363,	'DATA'
	insert into #FlowsFromSpreadsheet select 364,	'DATA'
	insert into #FlowsFromSpreadsheet select 366,	'PLBI'
	insert into #FlowsFromSpreadsheet select 370,	'CLBR'
	insert into #FlowsFromSpreadsheet select 373,	'CLBR'
	insert into #FlowsFromSpreadsheet select 376,	'CLBR'
	insert into #FlowsFromSpreadsheet select 379,	'DATA'
	insert into #FlowsFromSpreadsheet select 383,	'PLPC'
	insert into #FlowsFromSpreadsheet select 386,	'ENPR'
	insert into #FlowsFromSpreadsheet select 389,	'LOTT'
	insert into #FlowsFromSpreadsheet select 390,	'LOTT'
	insert into #FlowsFromSpreadsheet select 392,	'PLPC'
	insert into #FlowsFromSpreadsheet select 393,	'PLPC'
	insert into #FlowsFromSpreadsheet select 394,	'PLPC'
	insert into #FlowsFromSpreadsheet select 397,	'LOTT'
	insert into #FlowsFromSpreadsheet select 398,	'LOTT'
	insert into #FlowsFromSpreadsheet select 401,	'DATA'
	insert into #FlowsFromSpreadsheet select 405,	'DATA'
	insert into #FlowsFromSpreadsheet select 408,	'DATA'
	insert into #FlowsFromSpreadsheet select 410,	'DATA'
	insert into #FlowsFromSpreadsheet select 412,	'DATA'
	insert into #FlowsFromSpreadsheet select 413,	'DATA'
	insert into #FlowsFromSpreadsheet select 415,	'DATA'
	insert into #FlowsFromSpreadsheet select 417,	'DATA'
END
else if (@ENV__CLA2413 = 'PROD')
BEGIN
	insert into #FlowsFromSpreadsheet select 34, 'PLBI'
	insert into #FlowsFromSpreadsheet select 55, 'DATA'
	insert into #FlowsFromSpreadsheet select 57, 'DATA'
	insert into #FlowsFromSpreadsheet select 59, 'DATA'
	insert into #FlowsFromSpreadsheet select 61, 'DATA'
	insert into #FlowsFromSpreadsheet select 63, 'DATA'
	insert into #FlowsFromSpreadsheet select 65, 'CLBR'
	insert into #FlowsFromSpreadsheet select 94, 'PLBI'
	insert into #FlowsFromSpreadsheet select 133, 'DATA'
	insert into #FlowsFromSpreadsheet select 135, 'DATA'
	insert into #FlowsFromSpreadsheet select 136, 'PLBI'
	insert into #FlowsFromSpreadsheet select 143, 'DAAM'
	insert into #FlowsFromSpreadsheet select 145, 'DAAM'
	insert into #FlowsFromSpreadsheet select 147, 'DAAM'
	insert into #FlowsFromSpreadsheet select 149, 'DAAM'
	insert into #FlowsFromSpreadsheet select 151, 'DAAM'
	insert into #FlowsFromSpreadsheet select 152, 'DAAM'
	insert into #FlowsFromSpreadsheet select 154, 'DAAM'
	insert into #FlowsFromSpreadsheet select 156, 'DAAM'
	insert into #FlowsFromSpreadsheet select 158, 'DAAM'
	insert into #FlowsFromSpreadsheet select 160, 'DAAM'
	insert into #FlowsFromSpreadsheet select 162, 'DAAM'
	insert into #FlowsFromSpreadsheet select 164, 'DAAM'
	insert into #FlowsFromSpreadsheet select 166, 'DAAM'
	insert into #FlowsFromSpreadsheet select 168, 'DAAM'
	insert into #FlowsFromSpreadsheet select 170, 'DAAM'
	insert into #FlowsFromSpreadsheet select 172, 'DAAM'
	insert into #FlowsFromSpreadsheet select 174, 'DAAM'
	insert into #FlowsFromSpreadsheet select 176, 'DAAM'
	insert into #FlowsFromSpreadsheet select 178, 'DAAM'
	insert into #FlowsFromSpreadsheet select 180, 'DAAM'
	insert into #FlowsFromSpreadsheet select 182, 'DAAM'
	insert into #FlowsFromSpreadsheet select 184, 'DAAM'
END



/*****************************************************
	Exclude dataflows with the following criteria:
		- Dataflows already associated with a Producer S3 Drop action

******************************************************/

Select * 
into #ExistingFlows
from #FlowsFromSpreadsheet FFS
left join		/* exclude dataflows which are already associated with a Producer S3 Drop location */
(
	select
		DFS.DataFlow_Id as 'producerdataflowid'
	from DataFlowStep DFS
	join DataAction DA on
		DFS.Action_Id = DA.Id
	where DA.Name = 'Producer S3 Drop'
) x on
	FFS.DataFlowID = x.producerdataflowid
where (x.producerdataflowid is null)


/* Select * from #ExistingFlows */

/*****************************************
	Update SaidAssetKeyCode for all identified
	  dataflows within #ExistingFlows
*****************************************/

Update DataFlow
SET SaidKeyCode = x.SaidAsset
from (
	Select *
	from #ExistingFlows
) x
where Id = x.DataFlowID

/******************************************
	Select S3 Drop step for all dataflow Id's 
	  identified within #ExistingFlows

	Generate "New" columns for all columns that need
	  to be altered
******************************************/
select 
	DF.Id as 'DataFlowId',
	EF.SaidAsset,
	DFS.Id as 'DataFlowStepId',
	DFS.Action_Id,
	@NewProducerS3DropActionId__CLA2413 as 'NewActionId',
	DFS.DataAction_Type_Id,
	12 as 'NewDataAction_Type_Id',
	DFS.TriggerKey,
	REPLACE(DFS.TriggerKey, @OldDropPrefix__CLA2413, 'droplocation/data/' + EF.SaidAsset + '/') as 'NewTriggerKey',
	DFS.TargetPrefix,
	DAT.Name as 'DataActionTypeName',
	DFS.SourceDependencyBucket
into #S3DropUpdates
from DataFlow DF
join #ExistingFlows EF on
	DF.Id = EF.DataFlowID
join DataFlowStep DFS on
	DF.Id = DFS.DataFlow_ID
join DataActionTypes DAT on
	DFS.DataAction_Type_Id = DAT.Id
join DataAction DA on
	DFS.Action_Id = DA.Id
where DAT.Name in ('S3 Drop')

DECLARE @S3DropUpdateCnt INT = (Select COUNT(*) from #S3DropUpdates)
print (CAST(@S3DropUpdateCnt as VARCHAR(4))) +  ' S3 Drop updates identified'

/* select * from #S3DropUpdates */

Update DataFlowStep
	SET Action_ID = x.NewActionId,
		DataAction_Type_Id = x.NewDataAction_Type_Id,
		TriggerKey = x.NewTriggerKey
from
(select * from #S3DropUpdates) x
where Id = x.DataFlowStepId


select 
	DF.Id as 'DataFlowId',
	DFS.Id as 'DataFlowStepId',
	EF.SaidAsset,
	DFS.SourceDependencyBucket,
	@NewSourceBucket__CLA2413 as 'NewSourceDependencyBucket',
	DFS.SourceDependencyPrefix,
	REPLACE(DFS.SourceDependencyPrefix, @OldDropPrefix__CLA2413, 'droplocation/data/' + EF.SaidAsset + '/') as 'NewSourceDependencyPrefix',	
	DA.Name as 'DataActionName'
into #RawStorageUpdates
from DataFlow DF
join #ExistingFlows EF on
	DF.Id = EF.DataFlowID
join DataFlowStep DFS on
	DF.Id = DFS.DataFlow_ID
join DataActionTypes DAT on
	DFS.DataAction_Type_Id = DAT.Id
join DataAction DA on
	DFS.Action_Id = DA.Id
where DAT.Name in ('Raw Storage')

DECLARE @RawStorageUpdateCnt INT = (Select COUNT(*) from #RawStorageUpdates)
print (CAST(@RawStorageUpdateCnt as VARCHAR(4))) +  'Raw Storage updates identified'

/* select * from #RawStorageUpdates */
Update DataFlowStep
	SET SourceDependencyBucket = x.NewSourceDependencyBucket,
		SourceDependencyPrefix = x.NewSourceDependencyPrefix
from
(select * from #RawStorageUpdates) x
where Id = x.DataFlowStepId
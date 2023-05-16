CREATE PROCEDURE [dbo].[usp_GetDeadJobs] @StartDate datetime, @EndDate datetime

AS

SET NOCOUNT ON

IF OBJECT_ID('tempdb..#tempSubmissionDetails') IS NOT NULL DROP TABLE #tempSubmissionDetails
IF OBJECT_ID('tempdb..#TempSubmissionDetails_RowNum') IS NOT NULL DROP TABLE #TempSubmissionDetails_RowNum
IF OBJECT_ID('tempdb..#IdentifiedDeadJobs') IS NOT NULL DROP TABLE #IdentifiedDeadJobs
IF OBJECT_ID('tempdb..#EventMetadata') IS NOT NULL DROP TABLE #EventMetadata
IF OBJECT_ID('tempdb..#EnvironmentJobIDs') IS NOT NULL DROP TABLE #EnvironmentJobIDs

/************************
Job IDs
prod = 465
QUAL = 529
TEST = 262
************************/
/* Determine current named environment */
DECLARE @ENV VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')

CREATE TABLE #EnvironmentJobIDs (Job_ID int);

/* Select Job IDs associated with current Environment */
IF (@ENV = 'DEV' OR @ENV = 'NRDEV' OR @ENV = 'TEST' OR @ENV = 'NRTEST')
BEGIN 
    INSERT #EnvironmentJobIDs(Job_ID) values(262);
END
ELSE IF @ENV = 'QUAL'
BEGIN 
    INSERT #EnvironmentJobIDs(Job_ID) values(529),(4638),(4637);
END
ELSE IF @ENV = 'PROD'
BEGIN 
    INSERT #EnvironmentJobIDs(Job_ID) values(3190),(3191),(465);
END


Select 
Submission.Submission_ID,
Submission.Job_ID as 'sub_Job_ID',
Submission.Created as 'sub_Created',
CASE
	WHEN Submission.RunInstanceGuid = '00000000000000000' THEN NULL
	ELSE Submission.RunInstanceGuid
END as 'RunInstanceGuid',
Submission.FlowExecutionGuid,
Submission.Serialized_Job_Options,
REPLACE(
    REPLACE(
        REPLACE(
            REPLACE(
                REPLACE(Option_Metadata.arguments,'\\',''),
                '\"', '"'),
        '["{','{'),
    '}"]','}'),
'} "]','}') as 'arguments',
Argument_Metadata.topic,
Argument_Metadata.SourceBucketName,
Argument_Metadata.SourceKey,
Argument_Metadata.TargetBucketName,
Argument_Metadata.TargetKey,
Argument_Metadata.Dataset_ID,
Argument_Metadata.[Schema_ID],
CASE
    WHEN Submission.Created > '2023-03-31 17:05:00' THEN REVERSE(SUBSTRING(REVERSE(Argument_Metadata.TargetKey),0,16))
    ELSE REVERSE(SUBSTRING(REVERSE(Argument_Metadata.TargetKey),0,35))
END as 'FileNameSnippetValidator',  -- The case statement is needed due to flowexecutionguid being prefixed vs suffixed on file name after 2023-03-31 17:05:00
JobHistory.*
into #tempSubmissionDetails
from Submission
join JobHistory on
    Submission.Submission_ID = JobHistory.Submission
CROSS APPLY OPENJSON(Submission.Serialized_Job_Options, '$.args') WITH (
    arguments NVARCHAR(MAX) '$'
) Option_Metadata
CROSS APPLY OPENJSON(Option_Metadata.arguments, '$') WITH ( 
    topic varchar(250) '$.KafkaProducer.TargetTopic',
    SourceBucketName varchar(250) '$.ProgramArguments.SourceBucketName',
    SourceKey varchar(250) '$.ProgramArguments.SourceKey',
    TargetBucketName varchar(250) '$.ProgramArguments.TargetBucketName',
    TargetKey varchar(250) '$.ProgramArguments.TargetKey',
    Dataset_ID bigint '$.Dataset_ID',
    [Schema_ID] bigint '$.Schema_ID'
) Argument_Metadata
where 
    JobHistory.State = 'Dead'
    and Submission.Job_ID in (SELECT * FROM #EnvironmentJobIDs)
    and Submission.Created between @StartDate and @EndDate
order by Submission.Created DESC, JobHistory.History_Id DESC

select 
ROW_NUMBER() OVER(PARTITION BY Dataset_ID,Schema_ID,BatchId ORDER BY History_Id DESC) AS RowNumber,
*,
--REVERSE(SUBSTRING(REVERSE(TargetKey), CHARINDEX('.',REVERSE(TargetKey),0) + 1, 17)) as 'ExecutionGuid'
CASE
    WHEN sub_Created > '2023-03-31 17:05:00' THEN SUBSTRING(REVERSE(SUBSTRING(REVERSE(TargetKey),0,CHARINDEX('/',REVERSE(TargetKey),0))),0,18)
    ELSE REVERSE(SUBSTRING(REVERSE(TargetKey), CHARINDEX('.',REVERSE(TargetKey),0) + 1, 17))
END as 'ExecutionGuid'
into #TempSubmissionDetails_RowNum
from #tempSubmissionDetails

select 
ROW_NUMBER() OVER(PARTITION BY TSD_Num.Submission_Id ORDER BY EM.EventMetricsId DESC) AS RowNumber,
TSD_Num.Submission_Id,
EM.EventMetricsId
into #EventMetadata
from #TempSubmissionDetails_RowNum TSD_Num
join EventMetrics EM on
    TSD_Num.FlowExecutionGuid = EM.FlowExecutionGuid
join DataFlowStep DFS on
    EM.DataFlowStepId = DFS.Id
where 
    TSD_Num.RowNumber = 1
    and DFS.DataAction_Type_Id in (5, 17)
    and EM.MessageValue like '%' + TSD_NUM.FileNameSnippetValidator + '%'   /* Needed due to FlowExecutionGuid duplication within a given schema */


select
TSD.Submission_id,
TSD.BatchId,
DATEPART(DAY, TSD.Created) as 'Day of Month',
DATEPART(HOUR, TSD.Created) as 'Hour of Day',
TSD.Dataset_ID,
SCM.[Schema_Id],
DF.DatasetFile_Id,
DFlow.Id as 'DataFlow_ID',
DFlowStep.Id as 'DataFlowStep_ID',
TSD.sub_Created,
DS.Dataset_NME,
SCM.Schema_NME,
TSD.SourceBucketName,
TSD.SourceKey,
TSD.TargetKey,
TSD.State,
TSD.LivyAppId,
TSD.LivyDriverlogUrl,
TSD.LivySparkUiUrl,
JSON_VALUE(EM.MessageValue, '$.SourceKey') as 'TriggerKey',
JSON_VALUE(EM.MessageValue, '$.SourceBucket') as 'TriggerBucket',
TSD.FlowExecutionGuid,
TSD.RunInstanceGuid
into #IdentifiedDeadJobs
from #TempSubmissionDetails_RowNum TSD
join Dataset DS on
    TSD.Dataset_ID = DS.Dataset_ID
join [Schema] SCM on
    TSD.Schema_ID = SCM.Schema_ID
join #EventMetadata EvMetadata on
    TSD.Submission_ID = EvMetadata.Submission_ID
join EventMetrics EM on
    EvMetadata.EventMetricsId = EM.EventMetricsId
left join DatasetFile DF on
    DF.Dataset_ID = TSD.Dataset_ID and
    DF.Schema_ID = TSD.Schema_ID and
    DF.FlowExecutionGuid = TSD.FlowExecutionGuid and
       COALESCE(DF.RunInstanceGuid, 'none') = COALESCE(TSD.RunInstanceGuid, 'none') and
    DF.FileKey = TSD.SourceKey
left join DataFlow DFlow on
       TSD.Dataset_ID = DFlow.DatasetId and
       TSD.Schema_ID = DFlow.SchemaId
left join DataFlowStep DFlowStep on
       DFlowStep.DataFlow_Id = DFlow.Id and
       DFlowStep.DataAction_Type_Id = 2
where 
    TSD.RowNumber = 1 
    and EvMetadata.RowNumber = 1
order by TSD.sub_Created

SELECT * FROM #IdentifiedDeadJobs

GO
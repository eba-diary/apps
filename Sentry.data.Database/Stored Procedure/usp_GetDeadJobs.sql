CREATE PROCEDURE [dbo].[usp_GetDeadJobs] @TimeCreated datetime

AS

SET NOCOUNT ON

IF OBJECT_ID('tempdb..#tempSubmission') IS NOT NULL DROP TABLE #tempSubmission
IF OBJECT_ID('tempdb..#tempSubs') IS NOT NULL DROP TABLE #tempSubs
IF OBJECT_ID('tempdb..#Submissions') IS NOT NULL DROP TABLE #Submissions
IF OBJECT_ID('tempdb..#tempSubmissionDetails') IS NOT NULL DROP TABLE #tempSubmissionDetails
IF OBJECT_ID('tempdb..#TempSubmissionDetails_RowNum') IS NOT NULL DROP TABLE #TempSubmissionDetails_RowNum
IF OBJECT_ID('tempdb..#IdentifiedDeadJobs') IS NOT NULL DROP TABLE #IdentifiedDeadJobs
IF OBJECT_ID('tempdb..#EventMetadata') IS NOT NULL DROP TABLE #EventMetadata

/************************
This stored procedure is used to produce results to UI

Do not change output format unless UI is tested. 

Job IDs:
PRDO     = 465
QUAL OG  = 529
QUAL NEW = 4638
QUALNP   = 4637
TEST     = 262
************************/


/* Determine current named environment */
DECLARE @ENV VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')

DECLARE @EnvironmentJobIDs table (ID int)
DELETE FROM @EnvironmentJobIDs


/* Select Job IDs associated with current Environment */
IF (@ENV = 'DEV' OR @ENV = 'NRDEV' OR @ENV = 'TEST' OR @ENV = 'NRTEST')
BEGIN 
    INSERT @EnvironmentJobIDs(ID) values(262);
END
ELSE IF @ENV = 'QUAL'
BEGIN 
    INSERT @EnvironmentJobIDs(ID) values(529),(4638),(4367);
END
ELSE IF @ENV = 'PROD'
BEGIN 
    INSERT @EnvironmentJobIDs(ID) values(465);
END


select distinct Submission,History_Id
into #Submissions
from JobHistory
where State = 'Dead' and Job_ID in (SELECT ID FROM @EnvironmentJobIDs) and Created > @TimeCreated
order by History_Id DESC

/* SELECT * FROM #Submissions */

select
subs.Submission_ID,
subs.Job_ID,
subs.Created,
subs.Serialized_Job_Options,
REPLACE(
    REPLACE(
        REPLACE(
                     REPLACE(
                           REPLACE(JSON_QUERY(CAST(subs.Serialized_Job_Options as NVARCHAR(MAX)), '$.args'),'\',''),
                           '"', '"'),
        '["{','{'),
    '}"]','}'),
'} "]','}') as 'arguments'
into #tempSubs
from Submission subs
join #Submissions selectedSubs on
    subs.Submission_ID = selectedSubs.Submission
order by subs.Submission_ID DESC

/* SELECT * FROM #tempSubs */

select
subs.Submission_ID,
subs.Job_ID as 'sub_Job_ID',
subs.Created as 'sub_Created',
subs.Serialized_Job_Options,
JSON_VALUE(arguments, '$.KafkaProducer.TargetTopic') as 'topic',
JSON_VALUE(arguments, '$.ProgramArguments.SourceBucketName') as 'SourceBucketName',
JSON_VALUE(arguments, '$.ProgramArguments.SourceKey') as 'SourceKey',
JSON_VALUE(arguments, '$.ProgramArguments.TargetBucketName') as 'TargetBucketName',
JSON_VALUE(arguments, '$.ProgramArguments.TargetKey') as 'TargetKey',
JSON_VALUE(arguments, '$.Dataset_ID') as 'Dataset_ID',
JSON_VALUE(arguments, '$.Schema_ID') as 'Schema_ID',
JSON_VALUE(arguments, '$.FlowExecutionGuid') as 'FlowExecutionGuid',
JSON_VALUE(arguments, '$.RunInstanceGuid') as 'RunInstanceGuid',
hist.*
into #tempSubmissionDetails
from #tempSubs subs
join JobHistory hist on
    subs.Submission_ID = hist.Submission
order by subs.Created DESC, hist.History_Id DESC

/* SELECT * FROM #tempSubmissionDetails */

select 
ROW_NUMBER() OVER(PARTITION BY Dataset_ID,Schema_ID,BatchId ORDER BY History_Id DESC) AS RowNumber,
*
into #TempSubmissionDetails_RowNum
from #tempSubmissionDetails

/* SELECT * FROM #TempSubmissionDetails_RowNum */

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
       and DFS.DataAction_Type_Id = 5

/* SELECT * FROM #EventMetadata where RowNumber = 1 */


select
TSD.Submission_id,
TSD.sub_Created,
DS.Dataset_NME,
SCM.Schema_NME,
TSD.SourceBucketName,
TSD.SourceKey,
TSD.TargetKey,
TSD.BatchId,
TSD.State,
TSD.LivyAppId,
TSD.LivyDriverlogUrl,
TSD.LivySparkUiUrl,
DATEPART(DAY, TSD.Created) as 'Day of Month',
DATEPART(HOUR, TSD.Created) as 'Hour of Day',
JSON_VALUE(EM.MessageValue, '$.SourceKey') as 'TriggerKey',
JSON_VALUE(EM.MessageValue, '$.SourceBucket') as 'TriggerBucket',
TSD.FlowExecutionGuid,
TSD.Dataset_ID,
TSD.Schema_ID,
DF.DatasetFile_ID,
DFlow.Id as 'DataFlow_ID',
DFlowStep.Id as 'DataFlowStep_Id',
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
    DF.Dataset_Id = TSD.Dataset_ID and
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
where TSD.RowNumber = 1 and EvMetadata.RowNumber = 1
order by TSD.sub_Created

SELECT * FROM #IdentifiedDeadJobs

GO
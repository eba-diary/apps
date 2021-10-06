/**************************************************************
	Identify all dataflow IDs assoicated with RetrieverJobs (non 
**************************************************************/
IF OBJECT_ID('tempdb..#PullDataFlows') IS NOT NULL DROP TABLE #PullDataFlows
select 
	RJ.DataFlow_ID as 'DF_ID',
	RJ.Job_ID
	into #PullDataFlows
from RetrieverJob RJ 
join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id
where 
	RJ.DataFlow_ID is not null
	and DSrc.SourceType_IND not in ('DFSDataFlowBasic')


/*select * into #DataFlowTest from Dataflow*/

IF OBJECT_ID('tempdb..#PushPullresults') IS NOT NULL DROP TABLE #PushPullresults
select
CASE
	WHEN x.DF_ID is not null THEN 'PULL'
	ELSE 'PUSH'
END as 'Push_Pull',
CASE
	WHEN x.DF_ID is not null THEN 2
	ELSE 1
END as 'Push_Pull_Id',
DF.Id as 'DF_Id'
into #PushPullresults
from DataFlow DF
left join #PullDataFlows x on
	DF.Id = x.DF_ID
order by DF.Id desc

DECLARE @PushPull_ExpectedCount int = (select count(*) from DataFlow)
DECLARE @PushPull_ActualCount int = (Select count(*) from #PushPullresults)

if (@PushPull_ExpectedCount = @PushPull_ActualCount)
BEGIN
	PRINT 'Updating ' + CAST(@PushPull_ExpectedCount as varchar(max)) + ' records'
	Update DataFlow
	set IngestionType = Push_Pull_Id
	from #PushPullresults
	where id = DF_Id
END
ELSE
BEGIN
	PRINT 'Update did not execute since ExpectedCount (' + CAST(@PushPull_ExpectedCount as varchar(max)) + ') did not match ActualCount (' + CAST(@PushPull_ActualCount as varchar(max)) + ')'
END


/*********************************************
	Manual validation query
*********************************************/
/*
select 
DF.Name,
DF.Id,
DF.IngestionType,
RJ.Job_ID,
DSrc.SourceType_IND
from DataFlow DF
left join RetrieverJob RJ on
	DF.Id = RJ.DataFlow_ID
left join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id and
	DSrc.SourceType_IND not in ('DFSDataFlowBasic')
*/
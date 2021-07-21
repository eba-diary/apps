IF OBJECT_ID('tempdb..#DecompressionRequired_DataFlows') IS NOT NULL DROP TABLE #DecompressionRequired_DataFlows
select
	DA.Name,
	DFS.DataFlow_Id as 'DF_Id'
into #DecompressionRequired_DataFlows
from DataFlowStep DFS
left join DataAction DA on
	DFS.Action_Id = DA.Id
	and DA.Name in ('Uncompress Zip','Uncompress GZip')
where DA.Name IS NOT NULL

IF OBJECT_ID('tempdb..#Updates') IS NOT NULL DROP TABLE #Updates
select
DF.Id as 'DF_Id',
DF.Name,
CASE
	WHEN DRDF.Name = 'Uncompress Zip' then 0
	WHEN DRDF.Name = 'Uncompress GZip' then 1
	ELSE null
END as 'CompressionType_New'
into #Updates
from DataFlow DF
left join #DecompressionRequired_DataFlows DRDF on
	DF.Id = DRDF.DF_Id

DECLARE @ExpectedCount int = (select count(*) from DataFlow)
DECLARE @ActualCount int = (Select count(*) from #Updates)

if (@ExpectedCount = @ActualCount)
BEGIN
	PRINT 'Updating ' + CAST(@ExpectedCount as varchar(max)) + ' records'
	Update DataFlow
	set CompressionType = CompressionType_New
	from #Updates
	where id = DF_Id
END
ELSE
BEGIN
	PRINT 'Update did not execute since ExpectedCount (' + CAST(@ExpectedCount as varchar(max)) + ') did not match ActualCount (' + CAST(@ActualCount as varchar(max)) + ')'
END
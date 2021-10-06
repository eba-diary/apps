IF OBJECT_ID('tempdb..#CompressionType_DataFlows') IS NOT NULL DROP TABLE #CompressionType_DataFlows
select
	DA.Name,
	DFS.DataFlow_Id as 'DF_Id'
into #CompressionType_DataFlows
from DataFlowStep DFS
left join DataAction DA on
	DFS.Action_Id = DA.Id
	and DA.Name in ('Uncompress Zip','Uncompress GZip')
where DA.Name IS NOT NULL

IF OBJECT_ID('tempdb..#CompressionType_Updates') IS NOT NULL DROP TABLE #CompressionType_Updates
select
DF.Id as 'DF_Id',
DF.Name,
CASE
	WHEN DRDF.Name = 'Uncompress Zip' then 0
	WHEN DRDF.Name = 'Uncompress GZip' then 1
	ELSE null
END as 'CompressionType_New'
into #CompressionType_Updates
from DataFlow DF
left join #CompressionType_DataFlows DRDF on
	DF.Id = DRDF.DF_Id

DECLARE @CompressionType_ExpectedCount int = (select count(*) from DataFlow)
DECLARE @CompressionType_ActualCount int = (Select count(*) from #CompressionType_Updates)

if (@CompressionType_ExpectedCount = @CompressionType_ActualCount)
BEGIN
	PRINT 'Updating ' + CAST(@CompressionType_ExpectedCount as varchar(max)) + ' records'
	Update DataFlow
	set CompressionType = CompressionType_New
	from #CompressionType_Updates
	where id = DF_Id
END
ELSE
BEGIN
	PRINT 'Update did not execute since ExpectedCount (' + CAST(@CompressionType_ExpectedCount as varchar(max)) + ') did not match ActualCount (' + CAST(@CompressionType_ActualCount as varchar(max)) + ')'
END
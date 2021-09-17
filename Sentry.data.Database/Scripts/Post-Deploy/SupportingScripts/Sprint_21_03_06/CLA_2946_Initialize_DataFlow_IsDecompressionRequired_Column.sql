﻿IF OBJECT_ID('tempdb..#DecompressionRequired_DataFlows') IS NOT NULL DROP TABLE #DecompressionRequired_DataFlows
select
	DA.Name,
	DFS.DataFlow_Id as 'DF_Id'
into #DecompressionRequired_DataFlows
from DataFlowStep DFS
left join DataAction DA on
	DFS.Action_Id = DA.Id
	and DA.Name in ('Uncompress Zip','Uncompress GZip')
where DA.Name IS NOT NULL

IF OBJECT_ID('tempdb..#IsDecompressionRequired_Updates') IS NOT NULL DROP TABLE #IsDecompressionRequired_Updates
select
DF.Id as 'DF_Id',
DF.Name,
CASE
	WHEN DRDF.DF_id IS NULL then 0
	ELSE 1
END as 'IsDecompressionRequired_New'
into #IsDecompressionRequired_Updates
from DataFlow DF
left join #DecompressionRequired_DataFlows DRDF on
	DF.Id = DRDF.DF_Id

DECLARE @IsDecompressionRequired_ExpectedCount int = (select count(*) from DataFlow)
DECLARE @IsDecompressionRequired_ActualCount int = (Select count(*) from #IsDecompressionRequired_Updates)

if (@IsDecompressionRequired_ExpectedCount = @IsDecompressionRequired_ActualCount)
BEGIN
	PRINT 'Updating ' + CAST(@IsDecompressionRequired_ExpectedCount as varchar(max)) + ' records'
	Update DataFlow
	set IsDecompressionRequired = IsDecompressionRequired_New
	from #IsDecompressionRequired_Updates
	where id = DF_Id
END
ELSE
BEGIN
	PRINT 'Update did not execute since ExpectedCount (' + CAST(@IsDecompressionRequired_ExpectedCount as varchar(max)) + ') did not match ActualCount (' + CAST(@IsDecompressionRequired_ActualCount as varchar(max)) + ')'
END
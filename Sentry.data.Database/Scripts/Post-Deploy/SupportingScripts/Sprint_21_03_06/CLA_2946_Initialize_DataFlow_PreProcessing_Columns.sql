IF OBJECT_ID('tempdb..#Identified_DataFlows') IS NOT NULL DROP TABLE #Identified_DataFlows
select
	DA.Name,
	DFS.DataFlow_Id as 'DF_Id'
into #Identified_DataFlows
from DataFlowStep DFS
left join DataAction DA on
	DFS.Action_Id = DA.Id
	and DA.Name in ('ClaimIQ','Google Api')
where DA.Name IS NOT NULL


IF OBJECT_ID('tempdb..#Updates') IS NOT NULL DROP TABLE #Updates
select
DF.Id as 'DF_Id',
DF.Name,
CASE
	WHEN DRDF.DF_id IS NULL then 0
	ELSE 1
END as 'New_Value_PreProcessingRequired',
CASE
	WHEN DRDF.Name = 'ClaimIQ' THEN 2
	WHEN DRDF.Name = 'Google Api' THEN 1
	ELSE null
END as 'New_Value_PreProcessingOption'
into #Updates
from DataFlow DF
left join #Identified_DataFlows DRDF on
	DF.Id = DRDF.DF_Id



DECLARE @ExpectedCount int = (select count(*) from DataFlow)
DECLARE @ActualCount int = (Select count(*) from #Updates)

if (@ExpectedCount = @ActualCount)
BEGIN
	PRINT 'Updating ' + CAST(@ExpectedCount as varchar(max)) + ' records'
	Update DataFlow
	set IsPreProcessingRequired = New_Value_PreProcessingRequired,
		PreProcessingOption = New_Value_PreProcessingOption
	from #Updates
	where id = DF_Id
END
ELSE
BEGIN
	PRINT 'Update did not execute since ExpectedCount (' + CAST(@ExpectedCount as varchar(max)) + ') did not match ActualCount (' + CAST(@ActualCount as varchar(max)) + ')'
END
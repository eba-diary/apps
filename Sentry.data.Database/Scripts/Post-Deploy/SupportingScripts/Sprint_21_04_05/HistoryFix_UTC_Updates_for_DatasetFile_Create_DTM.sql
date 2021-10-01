IF OBJECT_ID('tempdb..#files') IS NOT NULL DROP TABLE		#files;
IF OBJECT_ID('tempdb..#files2') IS NOT NULL DROP TABLE		#files2;
IF OBJECT_ID('tempdb..#Create_Matches_FlowGuid') IS NOT NULL DROP TABLE		#Create_Matches_FlowGuid;
IF OBJECT_ID('tempdb..#Create_Matches_UTC_FlowGuid') IS NOT NULL DROP TABLE		#Create_Matches_UTC_FlowGuid;
IF OBJECT_ID('tempdb..#SpecialCase') IS NOT NULL DROP TABLE		#SpecialCase;

select 
DatasetFile_Id as 'F_Id',
Create_DTM,
SWITCHOFFSET(CAST(Create_DTM as datetime) AT TIME ZONE 'Central Standard Time', '+00:00') as 'UTC_Adjusted_CreateDTM',
FlowExecutionGuid,
Substring(FlowExecutionGuid, 0, 15) as 'NoMils_FlowExecutionGuid_Orig',
CONVERT(VARCHAR(4),DATEPART(YEAR,Create_DTM)) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(MONTH,Create_DTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(DAY,Create_DTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(HH,Create_DTM))				,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(MINUTE,Create_DTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(SECOND,Create_DTM))			,2) /*+ 
	RIGHT('000' +	CONVERT(VARCHAR(3),DATEPART(MILLISECOND,Create_DTM))	,3)*/ as 'Generated_FlowGuid'
into #files
from DatasetFile 

select
*,
CONVERT(VARCHAR(4),DATEPART(YEAR,UTC_Adjusted_CreateDTM)) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(MONTH,UTC_Adjusted_CreateDTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(DAY,UTC_Adjusted_CreateDTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(HH,UTC_Adjusted_CreateDTM))				,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(MINUTE,UTC_Adjusted_CreateDTM))			,2) + 
	RIGHT('00' +	CONVERT(VARCHAR(2),DATEPART(SECOND,UTC_Adjusted_CreateDTM))			,2) /*+ 
	RIGHT('000' +	CONVERT(VARCHAR(3),DATEPART(MILLISECOND,UTC_Adjusted_CreateDTM))	,3)*/ as 'Generated_UTC_FlowGuid'
into #files2
from #files
where FlowExecutionGuid IS NOT NULL

/***********************************************************************************
	All identified rows correct.  
		Create_DTM what populated using UTC flowexecutionguid
***********************************************************************************/
select 
f2.*
into #Create_Matches_FlowGuid
from #files2 f2
where  
	f2.FlowExecutionGuid is not null 
	and (NoMils_FlowExecutionGuid_Orig = Generated_FlowGuid or										
		(Substring(NoMils_FlowExecutionGuid_Orig,0,13) = Substring(Generated_FlowGuid,0,13)))
order by f2.Create_DTM

/***********************************************************************************
	All identified rows are incorrect.  
		Create_dtm needs to be adjusted to UTC to match flowexecutionguid
***********************************************************************************/
select
f2.*
into #Create_Matches_UTC_FlowGuid
from #files2 f2
left join #Create_Matches_FlowGuid correctRecords on
	f2.F_Id = correctRecords.F_Id
where 
	correctRecords.F_Id is null
	and (f2.NoMils_FlowExecutionGuid_Orig = f2.Generated_UTC_FlowGuid or
		(Substring(f2.NoMils_FlowExecutionGuid_Orig,0,13) = Substring(f2.Generated_UTC_FlowGuid,0,13)))

/***********************************************************************************
	Special Cases
		Need to review each case to determine what needs to be fixed
***********************************************************************************/
select
f2.*
into #SpecialCase
from #files2 f2
left join #Create_Matches_FlowGuid correctRecords on
	f2.F_Id = correctRecords.F_Id
left join #Create_Matches_UTC_FlowGuid incorrectRecordsMatchUTC on
	f2.F_Id = incorrectRecordsMatchUTC.F_Id
where 
	correctRecords.F_Id is null
	and incorrectRecordsMatchUTC.F_Id is null
	and f2.FlowExecutionGuid is not null



DECLARE @TotalFileCount int = (select count(*) from #files)
print 'Total File Count: ' + CONVERT(VARCHAR(MAX), @TotalFileCount)

DECLARE @CorrectFiles int = (select count(*) from #Create_Matches_FlowGuid)
print 'CorrectFile count:  ' + CONVERT(VARCHAR(max), @CorrectFiles)

DECLARE @Files_Incorrect int = (select count(*) from #Create_Matches_UTC_FlowGuid)
print 'Incorrect File count:  ' + CONVERT(VARCHAR(max), @Files_Incorrect)

DECLARE @SpecialCaseCount int = (select count(*) from #SpecialCase)
print 'Special Case count:  ' + CONVERT(VARCHAR(max), @SpecialCaseCount)

DECLARE @NullExecutionGuids int = (Select COUNT(*) from #files where FlowExecutionGuid is null)
print 'Null FlowExecutionGuid count:  ' + CONVERT(VARCHAR(max), @NullExecutionGuids)

DECLARE @Create_UTC_Corrections INT = (select count(*) from #Create_Matches_UTC_FlowGuid)


IF (@Create_UTC_Corrections > 0)
BEGIN
	print 'Identified ' + CONVERT(VARCHAR(max), @Create_UTC_Corrections) + ' incorrect Create_DTM values in DatasetFile table'
	
	update Datasetfile
	SET Create_DTM = x.UTC_Adjusted_CreateDTM
	from #Create_Matches_UTC_FlowGuid x
	where DatasetFile_id = x.F_Id
END
ELSE
BEGIN
	print 'There were no identified Create_DTM values to be corrected'
END
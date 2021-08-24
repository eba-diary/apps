IF OBJECT_ID('tempdb..#NewRetrieverJob') IS NOT NULL DROP TABLE #NewRetrieverJob
select 
*
into #NewRetrieverJob
from RetrieverJob

IF COL_LENGTH('dbo.RetrieverJob', 'ObjectStatus') IS NULL
BEGIN
	Print 'Did not detect ObjectStatus on RetrieverJob'
    Alter table #NewRetrieverJob
	Add ObjectStatus INT NULL
END

IF COL_LENGTH('dbo.RetrieverJob', 'DeleteIssuer') IS NULL
BEGIN
	Print 'Did not detect ObjectStatus on DeleteIssuer'
    Alter table #NewRetrieverJob
	Add DeleteIssuer VARCHAR(10) NULL
END

IF COL_LENGTH('dbo.RetrieverJob', 'DeleteIssueDTM') IS NULL
BEGIN
	Print 'Did not detect ObjectStatus on DeleteIssueDTM'
    Alter table #NewRetrieverJob
	Add DeleteIssueDTM datetime NULL
END


/* All retriever jobs associated with dataflow */
IF OBJECT_ID('tempdb..#JobsAssoicatedWithDataflows') IS NOT NULL DROP TABLE #JobsAssoicatedWithDataflows
GO
select Job_id as 'Identified_Job_Id', DataFlow_ID into #JobsAssoicatedWithDataflows from #NewRetrieverJob where DataFlow_ID is not null


/****************************************************************************
	Find RetrieverJobs associated Dataflows will ObjectStatus <> ACTIVE (Pending Delete or Deleted)
	Update identified retrieverjob records with appropriate objectstatus metadata
****************************************************************************/
IF OBJECT_ID('tempdb..#IdentifiedJobs_NonACTIVE') IS NOT NULL DROP TABLE #IdentifiedJobs_NonACTIVE
GO
select
DF.Id,
DF.DeleteIssueDTM as 'DF_DeleteIssueDTM',
DF.DeleteIssuer as 'DF_DeleteIssuer',
DF_ObjStat.ObjectStatus_CDE as 'DF_ObjStat',
Jobs.Identified_Job_Id as 'J_ID',
RJ.ObjectStatus as 'RJ_ObjStat',
RJ.IsEnabled,
DSrc.SourceType_IND,
DF_ObjStat.ObjectStatus_Id as 'Job_New_ObjStatus'
into #IdentifiedJobs_NonACTIVE
from DataFlow DF
join #JobsAssoicatedWithDataflows Jobs on
	DF.Id = Jobs.DataFlow_ID
left join ObjectStatus DF_ObjStat on
	DF.ObjectStatus = DF_ObjStat.ObjectStatus_Id
left join #NewRetrieverJob RJ on
	Jobs.Identified_Job_Id = RJ.Job_ID
left join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id
where DF_ObjStat.ObjectStatus_CDE <> 'ACTIVE' 

/*	select * from #IdentifiedJobs_NonACTIVE  */

Update #NewRetrieverJob
	SET ObjectStatus = Job_New_ObjStatus, DeleteIssueDTM = DF_DeleteIssueDTM, DeleteIssuer = DF_DeleteIssuer, IsEnabled = 0, Modified_DTM = GETDATE()
from #IdentifiedJobs_NonACTIVE x
where Job_ID = x.J_ID


/****************************************************************************
	Find RetrieverJobs associated Dataflows will ObjectStatus = ACTIVE
	Update identified retrieverjob records with appropriate objectstatus metadata
****************************************************************************/
IF OBJECT_ID('tempdb..#IdentifiedDFJobs_ACTIVE') IS NOT NULL DROP TABLE #IdentifiedDFJobs_ACTIVE
GO
select
DF.Id,
DF.DeleteIssueDTM as 'DF_DeleteIssueDTM',
DF.DeleteIssuer as 'DF_DeleteIssuer',
DF_ObjStat.ObjectStatus_CDE as 'DF_ObjStat',
Jobs.Identified_Job_Id as 'J_ID',
RJ.ObjectStatus as 'RJ_ObjStat',
RJ.IsEnabled,
DSrc.SourceType_IND,
DF_ObjStat.ObjectStatus_Id as 'Job_New_ObjStatus'
into #IdentifiedDFJobs_ACTIVE
from DataFlow DF
join #JobsAssoicatedWithDataflows Jobs on
	DF.Id = Jobs.DataFlow_ID
left join ObjectStatus DF_ObjStat on
	DF.ObjectStatus = DF_ObjStat.ObjectStatus_Id
left join #NewRetrieverJob RJ on
	Jobs.Identified_Job_Id = RJ.Job_ID
left join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id
where DF_ObjStat.ObjectStatus_CDE = 'ACTIVE' and RJ.ObjectStatus is null and RJ.IsEnabled = 1

/*   select * from #IdentifiedDFJobs_ACTIVE    */

Update #NewRetrieverJob
SET ObjectStatus = 1
from #IdentifiedDFJobs_ACTIVE x
where Job_ID = x.J_ID


/****************************************************************************
	Find RetrieverJobs associated to Schema (NOT Dataflows) and schema objectstatus <> ACTIVE
		Excluding the DataSourceType = JavaApp (SparkConverter), this will be handled separately
	Update identified retrieverjob records with appropriate objectstatus metadata
****************************************************************************/

IF OBJECT_ID('tempdb..#JobsAssoicatedWithSchema') IS NOT NULL DROP TABLE #JobsAssoicatedWithSchema
GO
select 
DFC.Config_ID,
DFC.Schema_Id,
DFC.ObjectStatus as 'DFC_ObjStat',
DFC_ObjStat.ObjectStatus_CDE as 'DFC_ObjStatCDE',
DFC.DeleteIssueDTM as 'DFC_DeleteIssueDTM',
DFC.DeleteIssuer as 'DFC_DeleteIssuer',
RJ.Job_Id as 'J_ID',
DSrc.SourceType_IND,
RJ.IsEnabled,
RJ.Modified_DTM,
RJ.ObjectStatus as 'RJ_ObjStat',
RJ.DeleteIssueDTM,
RJ.DeleteIssuer
into #JobsAssoicatedWithSchema
from #NewRetrieverJob RJ
left join DatasetFileConfigs DFC on
	RJ.Config_ID = DFC.Config_ID
left join ObjectStatus DFC_ObjStat on
	DFC.ObjectStatus = DFC_ObjStat.ObjectStatus_Id
left join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id
where 
	RJ.DataFlow_ID Is Null 
	and RJ.ObjectStatus is null
	and DSrc.SourceType_IND not in ('JavaApp')
	and DFC_ObjStat.ObjectStatus_CDE <> 'ACTIVE'

/* select * from #JobsAssoicatedWithSchema */

update #NewRetrieverJob
SET Modified_DTM = DFC_DeleteIssueDTM, ObjectStatus = DFC_ObjStat, DeleteIssueDTM = DFC_DeleteIssueDTM, DeleteIssuer = DFC_DeleteIssuer
from #JobsAssoicatedWithSchema x
where Job_Id = x.J_ID


/****************************************************************************
	Mark legacy processing platform S3Basic and DFSBasic retriever jobs
	  with DELETED status
****************************************************************************/

IF OBJECT_ID('tempdb..#legacyprocessingplatformjobs') IS NOT NULL DROP TABLE #legacyprocessingplatformjobs
GO
select 
RJ.Job_ID as 'J_ID',
DSrc.SourceType_IND,
RJ.IsEnabled,
RJ.Modified_DTM,
RJ.ObjectStatus as 'RJ_ObjStat',
RJ.DeleteIssueDTM,
RJ.DeleteIssuer
into #legacyprocessingplatformjobs
from #NewRetrieverJob RJ 
join DataSource DSrc on 
	RJ.DataSource_ID = DSrc.DataSource_Id 
where 
	DSrc.SourceType_IND in ('S3Basic', 'DFSBasic') 
	and RJ.ObjectStatus IS NULL

/* select * from #legacyprocessingplatformjobs  */

update #NewRetrieverJob
SET Modified_DTM = GETDATE(), 
	IsEnabled = 0, 
	ObjectStatus = 3,		/* DELETED = 3 */
	DeleteIssueDTM = GETDATE(), 
	DeleteIssuer = '072984'
from #legacyprocessingplatformjobs x
where Job_ID = x.J_ID


/****************************************************************************
	Find legacy external retrieverjob (i.e. googleapi, https, etc..) 
		and mark them DELETED
****************************************************************************/

IF OBJECT_ID('tempdb..#Legacy_nonbasic_external_retrieverjobs') IS NOT NULL DROP TABLE #Legacy_nonbasic_external_retrieverjobs
GO
Select 
RJ.Job_ID as 'J_ID',
DSrc.SourceType_IND,
RJ.IsEnabled,
RJ.Modified_DTM,
RJ.ObjectStatus as 'RJ_ObjStat',
RJ.DeleteIssueDTM,
RJ.DeleteIssuer
into #Legacy_nonbasic_external_retrieverjobs
from #NewRetrieverJob RJ
left join DataSource DSrc on
	RJ.DataSource_ID = DSrc.DataSource_Id
where DataFlow_ID IS NULL and DSrc.SourceType_IND NOT IN ('DFSBasic','S3Basic', 'JavaApp') and RJ.ObjectStatus is null

/* select * from #Legacy_nonbasic_external_retrieverjobs  */

update #NewRetrieverJob
SET	Modified_DTM = GETDATE(),
	IsEnabled = 0, 
	ObjectStatus = 3,		/* DELETED = 3 */
	DeleteIssueDTM = GETDATE(), 
	DeleteIssuer = '072984'
from #Legacy_nonbasic_external_retrieverjobs x
where Job_ID = x.J_ID


/****************************************************************************
	Find all JavaApp retrieverjobs (spark converter jobs) and 
		mark them ACTIVE
****************************************************************************/
Select Job_ID as 'J_ID' into #JavaAppJobs from #NewRetrieverJob N_RJ left join DataSource DSrc on N_RJ.DataSource_ID = DSrc.DataSource_Id  where DSrc.SourceType_IND IN ('JavaApp')

update #NewRetrieverJob 
SET ObjectStatus = 1 /* ACTIVE */
from #JavaAppJobs x
where Job_ID = x.J_ID


DECLARE @ExecuteScript VARCHAR(5) = 'TRUE'

/****************************************************************************
	Check to ensure there are no null ObjectStatus values  
****************************************************************************/
if ((@ExecuteScript = 'TRUE') and (select COUNT(*) from #NewRetrieverJob N_RJ left join DataSource DSrc on N_RJ.DataSource_ID = DSrc.DataSource_Id where N_RJ.ObjectStatus is null) > 0)
BEGIN
	SET @ExecuteScript = 'FALSE'
	print 'Found null ObjectStatus values'
END

/**************************************************************************** 
	Check to ensure there are IsEnabled = 1 where ObjectStatus is DELETED
****************************************************************************/
if ((@ExecuteScript = 'TRUE') and (select COUNT(*) from #NewRetrieverJob N_RJ where N_RJ.ObjectStatus = 3 and N_RJ.IsEnabled = 1) > 0)
BEGIN
	SET @ExecuteScript = 'FALSE'
	print 'Found IsEnabled = 1 for DELETED ObjectStatus records'
END


/****************************************************************************
	If validations check out, perform updates to RetrieverJob table
****************************************************************************/
If (@ExecuteScript = 'TRUE')
BEGIN

	Select
	Job_Id as 'J_Id',
	ObjectStatus as 'new_Ostat',
	DeleteIssuer as 'new_deleteIssuer',
	DeleteIssueDTM as 'new_deleteIssueDTM',
	Modified_DTM as 'new_ModifiedDTM'
	into #Updated_Records
	from #NewRetrieverJob

	Update RetrieverJob
	SET ObjectStatus = x.new_Ostat, DeleteIssuer = x.new_deleteIssuer, DeleteIssueDTM = x.new_deleteIssueDTM, Modified_DTM = x.new_ModifiedDTM
	from #Updated_Records x
	where Job_ID = x.J_Id	
END
else
BEGIN
	print 'Script dectected validation issue, so changes were not issued'
END
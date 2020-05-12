DECLARE	@SASLib VARCHAR(255)		
if @@SERVERNAME like '%FIT-N%' AND @@SERVERNAME like '%12%' AND DB_NAME() like '%_NR'
	SET @SASLib = 'DSCNRDEV'
else if @@SERVERNAME like '%FIT-N%' AND @@SERVERNAME like '%12%' AND DB_NAME() not like '%_NR'
	SET @SASLib = 'DSCDEV'
else if @@SERVERNAME like '%FIT-N%' AND @@SERVERNAME like '%11%' AND DB_NAME() like '%_NR'
	SET @SASLib = 'DSCNRTEST'
else if (@@SERVERNAME like 'FIT-N%' AND @@SERVERNAME like '%11%' AND DB_NAME() not like '%_NR')
	SET @SASLib = 'DSCTEST'
else if (@@SERVERNAME like 'FIT-N%' AND @@SERVERNAME not like '%11%' AND @@SERVERNAME not like '%12%' AND DB_NAME() not like '%_NR')
	SET @SASLib = 'DSCQUAL'
else
	SET @SASLib = 'DSCPROD'

update [schema]
set SASLibrary = x.NewLibName
from (
	select
	scm.schema_ID as 'xID',
	@SASLib + '_' + cat.Name as 'NewLibName'
	from dataset ds
	left join DatasetCategory dscat on
		ds.Dataset_ID = dscat.Dataset_Id
	left join Category cat on
		dscat.Category_Id = cat.Id
	left join datasetfileconfigs dfc on
		ds.dataset_ID = dfc.Dataset_ID
	left join [schema] scm on
		dfc.Schema_Id = scm.Schema_Id
	where ds.Dataset_TYP = 'DS'
		and scm.SASLibrary is null
		and scm.Schema_Id is not null
		and cat.Name is not null
	order by DS.Dataset_NME, SCM.Schema_NME
) x
where Schema_Id = x.xID
DECLARE @OldRegion VARCHAR(20) = 's3-sa-east-1'
DECLARE @OldBucket VARCHAR(40)		
		if @@SERVERNAME like '%' + 'FIT-N' + '%' + '11'  AND DB_NAME() like '%_NR'
			SET @OldBucket = 'sentry-dataset-management-np-nr'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '12' AND DB_NAME() like '%_NR')
			SET @OldBucket = 'sentry-dataset-management-np-nr'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '12' AND DB_NAME() not like '%_NR')
			SET @OldBucket = 'sentry-dataset-management-np'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '11' AND DB_NAME() not like '%_NR')
			SET @OldBucket = 'sentry-dataset-management-np'
		else if ((@@SERVERNAME like 'FIT-N' + '%' + '10' OR @@SERVERNAME like 'FIT-N' + '%' + '20') AND DB_NAME() not like '%_NR')
			SET @OldBucket = 'sentry-dataset-management-np'
		else
			SET @OldBucket = 'sentry-dataset-management'

			
DECLARE @NewRegion VARCHAR(20) = 's3-us-east-2'
DECLARE @NewBucket VARCHAR(40)		
		if @@SERVERNAME like '%' + 'FIT-N' + '%' + '11'  AND DB_NAME() like '%_NR'
			SET @NewBucket = 'sentry-data-nrtest-dataset-ae2'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '12' AND DB_NAME() like '%_NR')
			SET @NewBucket = 'sentry-data-nrdev-dataset-ae2'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '12' AND DB_NAME() not like '%_NR')
			SET @NewBucket = 'sentry-data-dev-dataset-ae2'
		else if (@@SERVERNAME like 'FIT-N' + '%' + '11' AND DB_NAME() not like '%_NR')
			SET @NewBucket = 'sentry-data-test-dataset-ae2'
		else if ((@@SERVERNAME like 'FIT-N' + '%' + '10' OR @@SERVERNAME like 'FIT-N' + '%' + '20') AND DB_NAME() not like '%_NR')
			SET @NewBucket = 'sentry-data-qual-dataset-ae2'
		else
			SET @NewBucket = 'sentry-data-prod-dataset-ae2'


Update DataSource
	set BaseUri = Replace(Replace(BaseUri, @OldRegion, @NewRegion), @OldBucket,@NewBucket),
	Bucket_NME = Replace(Bucket_NME, @OldBucket,@NewBucket)
where Bucket_NME is not null
update [schema] set hivelocation = replace(hivelocation, @OldBucket,@NewBucket) where HiveLocation is not null
update [image] set StorageBucketName = replace(StorageBucketName, @OldBucket, @NewBucket)
update DataElementDetail set DataElementDetailType_VAL = Replace(DataElementDetailType_VAL, @OldBucket, @NewBucket) where DataElementDetailType_CDE = 'HiveLocation'
update DataAction set TargetStorageBucket = replace(TargetStorageBucket, @OldBucket, @NewBucket)
update DataFlowStep Set SourceDependencyBucket = REPLACE(SourceDependencyBucket, @OldBucket, @NewBucket)
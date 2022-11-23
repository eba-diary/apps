BEGIN TRAN 
	BEGIN TRY
		DECLARE @DataSourceEnv VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
		DECLARE	@DfsDropLocation VARCHAR(255), @CustomDfsDropLocation varchar(255), @s3dropLocation varchar(255), @bucketName varchar(255), @DfsNonProdLocation VARCHAR(255), @DfsProdLocation VARCHAR(255)
		if @DataSourceEnv = 'NRDEV'
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
				SET @CustomDfsDropLocation = 'file:///c:/tmp/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-nrdev-dataset-ae2/data-dev/droplocation'
				SET @bucketName = 'sentry-data-nrdev-dataset-ae2'
				SET @DfsNonProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/DevNP/'
				SET @DfsProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/Dev/'
			END
		else if @DataSourceEnv = 'DEV'
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
				SET @CustomDfsDropLocation = 'file:///c:/tmp/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-dev-dataset-ae2/data-dev/droplocation'
				SET @bucketName = 'sentry-data-dev-dataset-ae2'
				SET @DfsNonProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/DevNP/'
				SET @DfsProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/Dev/'
			END
		else if @DataSourceEnv = 'NRTEST'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader_NRTest/'
				SET @CustomDfsDropLocation = 'file:////sentry.com/appfs_nonprod/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-nrtest-dataset-ae2\data-test/droplocation'
				SET @bucketName = 'sentry-data-nrtest-dataset-ae2'
				SET @DfsNonProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/TestNR/'
				SET @DfsProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/TestNR/'
			END
		else if @DataSourceEnv = 'TEST'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader/test/'
				SET @CustomDfsDropLocation = 'file:////sentry.com/appfs_nonprod/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-test-dataset-ae2\data-test/droplocation'
				SET @bucketName = 'sentry-data-test-dataset-ae2'
				SET @DfsNonProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/Test/'
				SET @DfsProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/Test/'
			END
		else if @DataSourceEnv = 'QUAL'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader/'
				SET @CustomDfsDropLocation = 'file:////sentry.com/appfs_nonprod/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-qual-dataset-ae2\data/droplocation'
				SET @bucketName = 'sentry-data-qual-dataset-ae2'
				SET @DfsNonProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/QualNP/'
				SET @DfsProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoaderDSC/Qual/'
			END
		else if @DataSourceEnv = 'PROD'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs/DatasetLoader/'
				SET @CustomDfsDropLocation = 'file:////sentry.com/appfs/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-prod-dataset-ae2\data/droplocation'
				SET @bucketName = 'sentry-data-prod-dataset-ae2'
				SET @DfsNonProdLocation = 'file://sentry.com/appfs_nonprod/DatasetLoader/'
				SET @DfsProdLocation = 'file://sentry.com/appfs/DatasetLoader/'
			END
		else
			--local database settings
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
				SET @CustomDfsDropLocation = 'file:///c:/tmp/'
				SET @s3dropLocation = 'http://s3-us-east-2.amazonaws.com/sentry-data-dev-dataset-ae2/data-dev/droplocation'
				SET @bucketName = 'sentry-data-nrdev-dataset-ae2'
				SET @DfsNonProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/DevNP/'
				SET @DfsProdLocation = 'file:///c:/tmp/DatasetLoaderDSC/Dev/'
			END
		
		

		MERGE INTO DataSource AS Target 
		USING (VALUES 
			('Default Drop Location','Default DFS drop location',@DfsDropLocation,0,'DFSBasic',1,'3bf6c17e-04d0','2018-05-13 23:35:31.000','2018-05-13 23:35:31.000', NULL, '072984','072984'),
			('Custom Drop Location','Custom DFS drop location',@CustomDfsDropLocation,0,'DFSCustom',1,'b92d3fdd-ed5a','2020-03-23 09:58:13.850','2020-03-23 09:58:13.850', NULL, '072984','072984'),
			('Default S3 Drop Location','This is the default S3 drop location for data.sentry.com.  This is used by applications integrated with S3 or associates with access.',@s3dropLocation,0,'S3Basic',2,'fe9afb5e-4d81','2020-03-23 09:58:13.850','2020-03-23 09:58:13.850', @bucketName, '072984','072984'),
			('DFS Drop Location','DFS drop location monitored by data processing platform',@DfsDropLocation,0,'DFSDataFlowBasic',1,'59A19624-AB2','2020-03-23 09:58:13.850','2020-03-23 09:58:13.850', NULL, '072984','072984'),
			('DFS NonProd Drop Location','DFS NonProd drop location monitored by data processing platform',@DfsNonProdLocation,0,'DFSNonProd',1,'c8624cce-078e','2022-11-10 00:00:00.000','2022-11-10 00:00:00.000', NULL, '082116','082116'),
			('DFS Prod Drop Location','DFS Prod drop location monitored by data processing platform',@DfsProdLocation,0,'DFSProd',1,'4bf5ddb8-0916','2022-11-10 00:00:00.000','2022-11-10 00:00:00.000', NULL, '082116','082116')
		)
		AS Source (Source_NME, Source_DSC, BaseUri, IsUriEditable_IND, SourceType_IND, SourceAuth_ID, KeyCode_CDE, [Created_DTM], [Modified_DTM], [Bucket_NME], [PrimaryContact_ID], [PrimaryOwner_ID]) 

		ON Target.SourceType_IND = Source.SourceType_IND
		WHEN MATCHED THEN 
			UPDATE SET
				Source_NME = Source.Source_NME,
				Source_DSC = Source.Source_DSC,
				BaseUri = Source.BaseUri,
				IsUriEditable_IND = Source.IsUriEditable_IND,
				SourceType_IND = Source.SourceType_IND,
				SourceAuth_ID = Source.SourceAuth_ID,
				[Created_DTM] = Source.[Created_DTM],
				[Modified_DTM] = Source.[Modified_DTM],
				[Bucket_NME] = Source.[Bucket_NME],
				[PrimaryContact_ID] = Source.[PrimaryContact_ID],
				[PrimaryOwner_ID] = Source.[PrimaryOwner_ID]
		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (Source_NME, Source_DSC, BaseUri, IsUriEditable_IND, SourceType_IND, SourceAuth_ID, KeyCode_CDE, [Created_DTM], [Modified_DTM], [Bucket_NME], [PrimaryContact_ID], [PrimaryOwner_ID])
			VALUES (Source_NME, Source_DSC, BaseUri, IsUriEditable_IND, SourceType_IND, SourceAuth_ID, KeyCode_CDE, [Created_DTM], [Modified_DTM], [Bucket_NME], [PrimaryContact_ID], [PrimaryOwner_ID]);
			
		--don't delete anything!
		--WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			--DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataSource_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataSource_ErrorSeverity INT; 
		DECLARE @Merge_DataSource_ErrorState INT; 
  
		SELECT 
			@Merge_DataSource_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataSource_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataSource_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataSource_ErrorMessage, 
				   @Merge_DataSource_ErrorSeverity, 
				   @Merge_DataSource_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
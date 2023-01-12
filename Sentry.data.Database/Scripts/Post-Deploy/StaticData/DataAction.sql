BEGIN TRAN 
	BEGIN TRY
		DECLARE @ENV VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
		DECLARE	@Bucket VARCHAR(255), @Bucketv2 VARCHAR(255), @ProducerS3DropBucket VARCHAR(255), @ProducerS3DropBucketv2 VARCHAR(255)
		DECLARE	@HRBucket VARCHAR(255), @HRProducerS3DropBucket VARCHAR(255)
		
		IF @ENV = 'DEV'
		BEGIN
			SET @Bucket = 'sentry-data-dev-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-dev-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-dev-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-dev-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-dev-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-dev-hrdroplocation-ae2'
		END
		ELSE IF @ENV = 'NRDEV'
		BEGIN
			SET @Bucket = 'sentry-data-nrdev-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-nrdev-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-nrdev-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-nrdev-droplocation-ae2'
			
			--HR
			SET @HRBucket = 'sentry-dlst-nrdev-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-nrdev-hrdroplocation-ae2'
		END
		ELSE IF @ENV = 'TEST'
		BEGIN
			SET @Bucket = 'sentry-data-test-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-test-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-test-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-test-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-test-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-test-hrdroplocation-ae2'
		END
		ELSE IF @ENV = 'NRTEST'
		BEGIN
			SET @Bucket = 'sentry-data-nrtest-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-nrtest-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-nrtest-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-nrtest-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-nrtest-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-nrtest-hrdroplocation-ae2'
		END
		ELSE IF @ENV = 'QUAL'
		BEGIN
			SET @Bucket = 'sentry-data-qual-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-qual-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-qual-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-qual-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-qual-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-qual-hrdroplocation-ae2'
		END
		ELSE IF @ENV = 'PROD'
		BEGIN
			SET @Bucket = 'sentry-data-prod-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-prod-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-prod-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-prod-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-prod-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-prod-hrdroplocation-ae2'
		END
		ELSE
		BEGIN
			SET @Bucket = 'sentry-data-dev-dataset-ae2'
			SET @Bucketv2 = 'sentry-dlst-dev-dataset-ae2'
			SET @ProducerS3DropBucket = 'sentry-data-dev-droplocation-ae2'
			SET @ProducerS3DropBucketv2 = 'sentry-dlst-dev-droplocation-ae2'

			--HR
			SET @HRBucket = 'sentry-dlst-dev-hrdataset-ae2'
			SET @HRProducerS3DropBucket = 'sentry-dlst-dev-hrdroplocation-ae2'
		END
		
		

		MERGE INTO DataAction AS Target 
		USING (VALUES 
									(1, '69B68AC9-C6D1-452A-96CD-130BA3B9FFE7', 'S3 Drop', 's3drop/', @Bucket, 'S3Drop', 0, 'S3 drop location monitored by DSC data processing platform'),
									(2, '46F73C7A-D04D-4F35-82F7-1D7A5344A1BB', 'Raw Storage', 'raw/', @Bucket, 'RawStorage', 0, 'Sends copy of unaltered incoming file to long term storage location'),
									(3, 'B8AD3A69-243E-40DC-A09D-25C1E3FF5AAA', 'Query Storage', 'rawquery/', @Bucket, 'QueryStorage', 1, 'Sends copy of raw file to long term storage accessed via Query Tool'),
									(4, '3DE8781B-5E16-4946-A4D9-642B8B0F05FB', 'Schema Load', 'schemaload/', @Bucket, 'SchemaLoad', 0, 'Maps schema related metadata to file for processing'),
									(5, 'D16A5418-070C-4758-A8D9-D3007A7A0B38', 'Uncompress Zip', 'uncompresszip/', @Bucket, 'UncompressZip', 0, 'Uncompresses incoming zip file'),
									(6, 'A63198CB-7EE2-40C2-B43A-09C7F1A3AE64', 'ConvertToParquet', 'parquet/', @Bucket, 'ConvertParquet', 1, 'Converts raw file to parquet format and stores in long term storage accessed via Hive'),
									(7, '4F40C384-A838-42B9-A9E4-8EC3CFE436DA', 'Schema Map', 'schemamap/', @Bucket, 'SchemaMap', 0, 'Maps schema specific data flow metadata to file for processing'),
									(8, '0ED73692-AB69-42D9-8935-6B040164DDA7', 'Google Api', 'googleapipreprocessing/', @Bucket, 'GoogleApi', 0, 'Converts Google API JSON output to data processing friendly format'),
									(9, '23A7A07F-0B54-4990-AEFE-90C4611E5C69', 'ClaimIQ', 'claimiqpreprocessing/', @Bucket, 'ClaimIq', 0, 'Encodes and converts ClaimIQ file to data processing friendly format'),
									(10, 'C9B8BED8-A3C5-48F8-8A46-873240D66952', 'Uncompress Gzip', 'uncompressgzip/', @Bucket, 'UncompressGzip', 0, 'Decompresses incoming gzip file'),
									(11, 'C3B3CC25-2F94-48C4-90E8-084BAF117BFA', 'Fixed Width', 'fixedwidthpreprocessing/', @Bucket, 'FixedWidth', 0, 'Converts fixed width file into data processing friendly format'),
									(12, '7C4BAF8E-697A-477C-8471-57BF5871DA47', 'Producer S3 Drop', 'producers3drop/', @ProducerS3DropBucket, 'ProducerS3Drop', 0, 'S3 drop location exposed to data producers for sending data to DSC data processing platform'),
									(13, '4A543410-1D8E-4132-BD92-A9A299012C85', 'XML', 'xmlpreprocessing/', @Bucket, 'XML', 0, 'Converts xml file into data processing friendly format'),
									(14, '6F45A391-56DB-4C3E-9A03-AFD4B794AAF3', 'JSON Flattening', 'jsonflattening/', @Bucket, 'JsonFlattening', 0, 'Flattens incoming JSON based on specified Schema Root Path property on schema'),
									(15, '224D7E9E-A3F6-4CBA-9E1A-B30B6C8B8F93', 'Producer S3 Drop', 'producers3drop/', @ProducerS3DropBucketv2, 'ProducerS3Drop', 0, 'DLST S3 drop location exposed to data producers for sending data to DSC data processing platform'),
									
									--HR DataAction 
									(16, 'E1CC7647-469F-45EC-8FE2-7E1835B69686', 'HR Raw Storage', 'raw/', @HRBucket, 'RawStorage', 0, 'HR Sends copy of unaltered incoming file to long term storage location'),
									(17, '4C4C2F06-1259-45D3-BF11-CDA105AB7B4E', 'HR Query Storage', 'rawquery/', @HRBucket, 'QueryStorage', 1, 'HR Sends copy of raw file to long term storage accessed via Query Tool'),
									(18, '528F1378-C89D-4056-A8DE-7EC97071441D', 'HR Schema Load', 'schemaload/', @HRBucket, 'SchemaLoad', 0, 'HR Maps schema related metadata to file for processing'),
									(19, 'A0AA6364-A943-449B-907D-DB631B35C746', 'HR ConvertToParquet', 'parquet/', @HRBucket, 'ConvertParquet', 1, 'HR Converts raw file to parquet format and stores in long term storage accessed via Hive'),
									(20, '6CDD6D22-B79F-4A89-8806-8ACE399DF174', 'HR Producer S3 Drop', 'producers3drop/', @HRProducerS3DropBucket, 'ProducerS3Drop', 0, 'HR S3 drop location exposed to data producers for sending data to DSC data processing platform'),
									(21, 'CDFB2967-836A-4778-83DB-2659B14F4BD7', 'HR XML', 'xmlpreprocessing/', @HRBucket, 'XML', 0, 'HR Converts xml file into data processing friendly format'),
									
									(22, '2C66A6E3-883B-416F-AC94-7A7FBD7CB845', 'Raw Storage', 'raw/', @Bucketv2, 'RawStorage', 0, 'Sends copy of unaltered incoming file to long term storage location within DLST bucket'),
									(23, '61C96A9D-E3CB-49C9-9318-D73CC492E5B5', 'Query Storage', 'rawquery/', @Bucketv2, 'QueryStorage', 1, 'Sends copy of raw file to long term storage within DLST bucket'),
									(24, '54E59A6C-FA33-4B9B-A035-7E818F160BC7', 'ConvertToParquet', 'parquet/', @Bucketv2, 'ConvertParquet', 1, 'Converts raw file to parquet format and stores in long term storage within DLST bucket'),
									(25, 'FBCF60AE-D76A-4E7F-83B3-9443544F2034', 'Uncompress Zip', 'uncompresszip/', @Bucketv2, 'UncompressZip', 0, 'Uncompresses incoming zip file within DLST bucket'),
									(26, '708120E2-9961-4559-B2D3-65B9494DD47B', 'Google Api', 'googleapipreprocessing/', @Bucketv2, 'GoogleApi', 0, 'Converts Google API JSON output to data processing friendly format within DLST bucket'),
									(27, '9B32C12B-E721-4C7E-94EE-DCEF5B3E0269', 'ClaimIQ', 'claimiqpreprocessing/', @Bucketv2, 'ClaimIq', 0, 'Encodes and converts ClaimIQ file to data processing friendly format within DLST bucket'),
									(28, '66352D07-2E41-4FAE-B0DA-D2B42F58B313', 'Uncompress Gzip', 'uncompressgzip/', @Bucketv2, 'UncompressGzip', 0, 'Decompresses incoming gzip file within DLST bucket'),
									(29, '6BC1D3CE-9A7A-42E7-9649-E3D4D083B244', 'Fixed Width', 'fixedwidthpreprocessing/', @Bucketv2, 'FixedWidth', 0, 'Converts fixed width file into data processing friendly format within DLST bucket'),
									(30, 'B9EACD9B-B2B8-4956-8ED7-60D9AE27608E', 'XML', 'xmlpreprocessing/', @Bucketv2, 'XML', 0, 'Converts xml file into data processing friendly format within DLST bucket'),
									(31, 'AFFA0462-749E-4130-AFFF-D16747B73F26', 'JSON Flattening', 'jsonflattening/', @Bucketv2, 'JsonFlattening', 0, 'Flattens incoming JSON based on specified Schema Root Path property on schema within DLST bucket'),
									(32, '3DE8781B-5E16-4946-A4D9-642B8B0F05FB', 'Schema Load', 'schemaload/', @Bucketv2, 'SchemaLoad', 0, 'Maps schema related metadata to file for processing within DLST bucket'),
									(33, '1FCB782B-5135-4723-BED9-16CD8468CDA8', 'Google BigQuery API', 'googlebigqueryapipreprocessing/', @Bucketv2, 'GoogleBigQueryApi', 0, 'Converts Google BigQuery API JSON output to data processing friendly format within DLST bucket'),
									(34, '5FE4ADC8-30A4-485C-BF56-29C10C1F6E3A', 'Google Search Console API', 'googlesearchconsoleapipreprocessing/', @Bucketv2, 'GoogleSearchConsoleApi', 0, 'Converts Google Search Console API JSON output to data processing friendly format within DLST bucket')
								)
								AS Source ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware], [Description]) 

		ON Target.[Id] = Source.[Id]
		WHEN MATCHED THEN 
			UPDATE SET
				[Id] = Source.[Id],
				[ActionGuid] = Source.[ActionGuid],
				[Name] = Source.[Name],
				[TargetStoragePrefix] = Source.[TargetStoragePrefix],
				[TargetStorageBucket] = Source.[TargetStorageBucket],
				[ActionType] = Source.[ActionType],
				[TargetStorageSchemaAware] = Source.[TargetStorageSchemaAware],
				[Description] = Source.[Description]
		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware], [Description]) 
			VALUES ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware], [Description])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataAction_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataAction_ErrorSeverity INT; 
		DECLARE @Merge_DataAction_ErrorState INT; 
  
		SELECT 
			@Merge_DataAction_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataAction_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataAction_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataAction_ErrorMessage, 
				   @Merge_DataAction_ErrorSeverity, 
				   @Merge_DataAction_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
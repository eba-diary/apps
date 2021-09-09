BEGIN TRAN 
	BEGIN TRY
		DECLARE @ENV VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
		DECLARE	@Bucket VARCHAR(255), @ProducerS3DropBucket VARCHAR(255), @ProducerS3DropBucketv2 VARCHAR(255)
		if @ENV = 'NRTEST'
			BEGIN
				SET @Bucket = 'sentry-data-nrtest-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-nrtest-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-nrtest-droplocation-ae2'
			END
		else if @ENV = 'NRDEV'
			BEGIN
				SET @Bucket = 'sentry-data-nrdev-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-nrdev-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-nrdev-droplocation-ae2'
			END
		else if @ENV = 'DEV'
			BEGIN
				SET @Bucket = 'sentry-data-dev-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-dev-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-dev-droplocation-ae2'
			END
		else if @ENV = 'TEST'
			BEGIN
				SET @Bucket = 'sentry-data-test-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-test-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-test-droplocation-ae2'
			END
		else if @ENV = 'QUAL'
			BEGIN
				SET @Bucket = 'sentry-data-qual-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-qual-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-qual-droplocation-ae2'
			END
		else if @ENV = 'PROD'
			BEGIN
				SET @Bucket = 'sentry-data-prod-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-prod-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-prod-droplocation-ae2'
			END
		else
			BEGIN
				SET @Bucket = 'sentry-data-dev-dataset-ae2'
				SET @ProducerS3DropBucket = 'sentry-data-dev-droplocation-ae2'
				SET @ProducerS3DropBucketv2 = 'sentry-dlst-dev-droplocation-ae2'
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
									(15, '224D7E9E-A3F6-4CBA-9E1A-B30B6C8B8F93', 'Producer S3 Drop v2', 'producers3drop/', @ProducerS3DropBucketv2, 'ProducerS3Drop_v2', 0, 'New S3 drop location exposed to data producers for sending data to DSC data processing platform')
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
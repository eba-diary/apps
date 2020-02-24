﻿BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO DataAction AS Target 
		USING (VALUES 
									(1, '69B68AC9-C6D1-452A-96CD-130BA3B9FFE7', 'S3 Drop', 's3drop/', 'sentry-dataset-management-np-nr', 'S3Drop', 0),
									(2, '46F73C7A-D04D-4F35-82F7-1D7A5344A1BB', 'Raw Storage', 'raw/', 'sentry-dataset-management-np-nr', 'RawStorage', 0),
									(3, 'B8AD3A69-243E-40DC-A09D-25C1E3FF5AAA', 'Query Storage', 'rawquery/', 'sentry-dataset-management-np-nr', 'QueryStorage', 1),
									(4, '3DE8781B-5E16-4946-A4D9-642B8B0F05FB', 'Schema Load', 'schemaload/', 'sentry-dataset-management-np-nr', 'SchemaLoad', 0),
									(5, 'D16A5418-070C-4758-A8D9-D3007A7A0B38', 'Uncompress Zip', 'uncompresszip/', 'sentry-dataset-management-np-nr', 'UncompressZip', 0),
									(6, 'A63198CB-7EE2-40C2-B43A-09C7F1A3AE64', 'ConvertToParquet', 'parquet/', 'sentry-dataset-management-np-nr', 'ConvertParquet', 1),
									(7, '4F40C384-A838-42B9-A9E4-8EC3CFE436DA', 'Schema Map', 'schemamap/', 'sentry-dataset-management-np-nr', 'SchemaMap', 0),
									(8, '0ED73692-AB69-42D9-8935-6B040164DDA7', 'Google Api', 'googleapipreprocessing/', 'sentry-dataset-management-np-nr', 'GoogleApi', 0),
									(9, '23A7A07F-0B54-4990-AEFE-90C4611E5C69', 'ClaimIQ', 'claimiqpreprocessing/', 'sentry-dataset-management-np-nr', 'ClaimIq', 0)
								)
								AS Source ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware]) 

		ON Target.[Id] = Source.[Id]
		WHEN MATCHED THEN 
			UPDATE SET
				[Id] = Source.[Id],
				[ActionGuid] = Source.[ActionGuid],
				[Name] = Source.[Name],
				[TargetStoragePrefix] = Source.[TargetStoragePrefix],
				[TargetStorageBucket] = Source.[TargetStorageBucket],
				[ActionType] = Source.[ActionType],
				[TargetStorageSchemaAware] = Source.[TargetStorageSchemaAware]
		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware]) 
			VALUES ([Id], [ActionGuid], [Name], [TargetStoragePrefix], [TargetStorageBucket], [ActionType], [TargetStorageSchemaAware])  
					  
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
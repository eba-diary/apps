BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO DataSourceType AS Target 
		USING (VALUES 
									('Basic DFS', 'DFS drop location controlled by data.sentry.com', 'DFSBasic'),
									('Basic DFS HSZ', 'DFS drop location controlled by data.sentry.com within HSZ', 'DFSBasicHsz'),
									('Basic S3', 'S3 Drop location controlled by data.sentry.com', 'S3Basic'),
									('Custom DFS', 'DFS drop location controlled by external team\system', 'DFSCustom'),
									('FTP', 'Retrieves data files from a FTP source', 'FTP'),
									('HTTPS', 'Retrieves data from HTTPS sources', 'HTTPS'),
									('Spark Java Application', 'Java application to run on Spark Cluster', 'JavaApp'),
									('GoogleApi', 'Google Api (v3)', 'GOOGLEAPI'),
									('Basic DataFlow DFS', 'DFS drop location controlled by data.sentry.com', 'DFSDataFlowBasic'),
									('Google BigQuery API', 'Retrieves Google Big Query data', 'GoogleBigQueryApi'),
									('DFS NonProd', 'DFS drop location for NonProd environment types controlled by data.sentry.com', 'DFSNonProd'),
									('DFS Prod', 'DFS drop location Prod environment types controlled by data.sentry.com', 'DFSProd')
								)
								AS Source ([Name], [Description], [DiscrimatorValue]) 

		ON Target.DiscrimatorValue = Source.DiscrimatorValue
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Name] = Source.[Name],  
				[Description] = Source.[Description]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Name], [Description], [DiscrimatorValue]) 
			VALUES ([Name], [Description], [DiscrimatorValue])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataSourceType_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataSourceType_ErrorSeverity INT; 
		DECLARE @Merge_DataSourceType_ErrorState INT; 
  
		SELECT 
			@Merge_DataSourceType_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataSourceType_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataSourceType_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataSourceType_ErrorMessage, 
				   @Merge_DataSourceType_ErrorSeverity, 
				   @Merge_DataSourceType_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
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
									('SFTP', 'Retrieves data files from SFTP sources', 'SFTP'),
									('Spark Java Application', 'Java application to run on Spark Cluster', 'JavaApp')
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
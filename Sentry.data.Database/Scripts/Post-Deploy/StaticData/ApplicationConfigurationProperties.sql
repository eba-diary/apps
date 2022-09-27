BEGIN TRAN 

	BEGIN TRY 
		
		MERGE INTO [ApplicationConfigurationProperties] AS TARGET USING 
		( VALUES 
			( 10002, 'FSTTESTBATCHHANDLER', 'TEST', 'app.key2', 'value12' ),
			( 10001, 'FSTTESTBATCHHANDLER', 'TEST', 'app.key1', 'value11' ),

			( 10101, 'SNDTESTBATCHHANDLER', 'TEST', 'app.key1', 'value21' ),
			( 10102, 'SNDTESTBATCHHANDLER', 'TEST', 'app.key2', 'value22' ),

			( 10201, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.server', 'dblo-test-plur.dbal.sentry.local' ),
			( 10202, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.name', 'INTRADAY_PLUR' ),
			( 10203, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 10204, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 10205, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 10206, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 10207, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 10208, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 10209, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 10210, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 10211, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 10212, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'configuration.queries', 'TEST/DATABASELOADERPLUR/TableConfiguration.csv' ),
			( 10213, 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'folder.queries', 'TEST/DATABASELOADERPLUR/queries/' ),

			( 10301, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 10302, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 10303, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 10304, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 10305, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 10306, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 10307, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 10308, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 10309, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 10310, 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 10311, 'DFSTOS3BATCHHANDLER', 'TEST', 'sql.read.filereply', 'SELECT 'sentry-dataset-management-np' AS [SourceBucket], DatasetFileReply.* FROM [DatasetFileReply] WHERE [ReplyStatus] = 'A';' ),
			( 10312, 'DFSTOS3BATCHHANDLER', 'TEST', 'sql.update.filereply', 'UPDATE [DatasetFileReply] SET [ReplyStatus] = {ReplyStatus} WHERE [DatasetFileReply_Id] = {DatasetFileReply_Id};' ),
			( 10313, 'DFSTOS3BATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 10314, 'DFSTOS3BATCHHANDLER', 'TEST', 'aws.version', 'ae2' ),
			( 10315, 'DFSTOS3BATCHHANDLER', 'TEST', 'target.topic', 'DATA-LOCAL-GOLDENEYE-000000' ),
			( 10316, 'DFSTOS3BATCHHANDLER', 'TEST', 's3.configuration.bucket', 'sentry-data-test-dataset-ae2' ),
			( 10317, 'DFSTOS3BATCHHANDLER', 'TEST', 's3.configuration.key', 'control/dfs_to_s3_batch/dfstos3configuration01.csv' ),

 			( 10401, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.server', 'dlpp-test-sentrydatasets.db.sentry.local' ),
			( 10402, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 10403, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 10404, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 10405, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 10406, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 10407, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 10408, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 10409, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 10410, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 10411, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 10412, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'configuration.queries', 'TEST/DSCDISPATCHERBATCH/TableConfiguration.csv' ),
			( 10413, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'folder.queries', 'TEST/DSCDISPATCHERBATCH/queries/' ),
			( 10414, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 's3.configuration.bucket', 'sentry-dlst-test-dataset-ae2' ),
			( 10415, 'DSCDISPATCHERBATCHHANDLER', 'TEST', 's3.configuration.key', 'conf/dsc_dispatcher_batch/configuration.txt' ),

            ( 20001, 'FSTTESTBATCHHANDLER', 'QUAL', 'app.key1', 'value11' ),
			( 20002, 'FSTTESTBATCHHANDLER', 'QUAL', 'app.key2', 'value12' ),
			( 20003, 'SNDTESTBATCHHANDLER',	'QUAL',	'app.key1',	'value21' ),
			( 20004, 'SNDTESTBATCHHANDLER',	'QUAL',	'app.key2',	'value22' ),
            ( 30001, 'FSTTESTBATCHHANDLER', 'PROD', 'app.key1', 'value11' ),
			( 30002, 'FSTTESTBATCHHANDLER', 'PROD', 'app.key2', 'value12' ),
			( 30003, 'SNDTESTBATCHHANDLER',	'PROD',	'app.key1',	'value21' ),
			( 30004, 'SNDTESTBATCHHANDLER',	'PROD',	'app.key2',	'value22' )
		)
		AS Source ([ID], [Application], [Environment], [ConfigurationKey], [ConfigurationValue]) 

		ON TARGET.[ID] = SOURCE.[ID]

		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[ID] = Source.[ID],  
				[Application] = SOURCE.[Application],
				[Environment] = SOURCE.[Environment],
				[ConfigurationKey] = SOURCE.[ConfigurationKey],
				[ConfigurationValue] = SOURCE.[ConfigurationValue]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([ID], [Application], [Environment], [ConfigurationKey], [ConfigurationValue]) 
			VALUES ([ID], [Application], [Environment], [ConfigurationKey], [ConfigurationValue])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;

	END TRY 

	BEGIN CATCH 

		DECLARE @Merge_ApplicationConfigurationProperties_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_ApplicationConfigurationProperties_ErrorSeverity INT; 
		DECLARE @Merge_ApplicationConfigurationProperties_ErrorState INT; 
  
		SELECT 
			@Merge_ApplicationConfigurationProperties_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_ApplicationConfigurationProperties_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_ApplicationConfigurationProperties_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_ApplicationConfigurationProperties_ErrorMessage, 
				   @Merge_ApplicationConfigurationProperties_ErrorSeverity, 
				   @Merge_ApplicationConfigurationProperties_ErrorState 
				   ); 
  
		ROLLBACK TRAN 

		RETURN

	END CATCH 
  
COMMIT TRAN
BEGIN TRAN 

	BEGIN TRY 
		
		MERGE INTO [ApplicationConfigurationProperties] AS TARGET USING 
		( VALUES 
			( 'FSTTESTBATCHHANDLER', 'TEST', 'app.key2', 'value12' ),
			( 'FSTTESTBATCHHANDLER', 'TEST', 'app.key1', 'value11' ),

			( 'SNDTESTBATCHHANDLER', 'TEST', 'app.key1', 'value21' ),
			( 'SNDTESTBATCHHANDLER', 'TEST', 'app.key2', 'value22' ),

			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.server', 'dblo-test-plur.dbal.sentry.local' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.name', 'INTRADAY_PLUR' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'configuration.queries', 'TEST/DATABASELOADERPLUR/TableConfiguration.csv' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'TEST', 'folder.queries', 'TEST/DATABASELOADERPLUR/queries/' ),

			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'data.sentry.check.published', 'SELECT 1 FROM [dbo].[FileDetector] WHERE [FilePath] = ''{FilePath}'' AND [FileCreatedDTM] = ''{FileCreatedDTM}'' AND [FileModifiedDTM] = ''{FileModifiedDTM}'';' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'TEST', 'data.sentry.update.published', 'INSERT INTO [dbo].[FileDetector] ([FilePath], [FileCreatedDTM], [FileModifiedDTM], [CreatedDTM]) VALUES (''{FilePath}'', ''{FileCreatedDTM}'', ''{FileModifiedDTM}'', GETDATE());' ),

			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'sql.read.filereply', 'SELECT ''sentry-dataset-management-np'' AS [SourceBucket], DatasetFileReply.* FROM [DatasetFileReply] WHERE [ReplyStatus] = ''A'';' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'sql.update.filereply', 'UPDATE [DatasetFileReply] SET [ReplyStatus] = {ReplyStatus} WHERE [DatasetFileReply_Id] = {DatasetFileReply_Id};' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'aws.version', 'ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 'target.topic', 'DATA-LOCAL-GOLDENEYE-000000' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 's3.configuration.bucket', 'sentry-dlst-test-dataset-ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'TEST', 's3.configuration.key', 'conf/dfs_to_s3_batch/dfstos3configuration01.csv' ),

 			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'configuration.queries', 'TEST/DSCDISPATCHERBATCH/TableConfiguration.csv' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 'folder.queries', 'TEST/DSCDISPATCHERBATCH/queries/' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 's3.configuration.bucket', 'sentry-dlst-test-dataset-ae2' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'TEST', 's3.configuration.key', 'conf/dsc_dispatcher_batch/configuration.txt' ),

			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'sql.readDatasetFileRaw', 'SELECT DatasetFileDrop.ObjectBucket AS [SourceBucket], DatasetFileDrop.ObjectKey AS [SourceKey], DatasetFileDrop.ObjectVersionID AS [SourceVersionID], DatasetFileRaw.ObjectBucket AS [TargetBucket], DatasetFileRaw.ObjectKey AS [TargetKey], DatasetFileRaw.DatasetFileRawID AS [DatasetFileRawID] FROM DatasetFileRaw LEFT JOIN DatasetFileDrop ON DatasetFileRaw.DatasetFileDropID = DatasetFileDrop.DatasetFileDropID WHERE DatasetFileRaw.ObjectStatus = 6;' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'sql.updateDatasetFileRaw', 'UPDATE DatasetFileRaw SET ObjectStatus = 1 WHERE DatasetFileRawID = {DatasetFileRawID};' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'sql.readDatasetFileQuery', 'SELECT DatasetFileDrop.ObjectBucket AS [SourceBucket], DatasetFileDrop.ObjectKey AS [SourceKey], DatasetFileDrop.ObjectVersionID AS [SourceVersionID], DatasetFileQuery.ObjectBucket AS [TargetBucket], DatasetFileQuery.ObjectKey AS [TargetKey], DatasetFileQuery.DatasetFileRawID AS [DatasetFileRawID] FROM DatasetFileQuery LEFT JOIN DatasetFileDrop ON DatasetFileQuery.DatasetFileDropID = DatasetFileDrop.DatasetFileDropID WHERE DatasetFileQuery.ObjectStatus = 6;' ),
			( 'FILECOPYBATCHHANDLER', 'TEST', 'sql.updateDatasetFileQuery', 'UPDATE DatasetFileQuery SET ObjectStatus = 1 WHERE DatasetFileRawID = {DatasetFileQueryID};' ),
			
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 's3.configuration.bucket', 'sentry-dlst-test-dataset-ae2' ),
			( 'S3FILESMERGEBATCHHANDLER', 'TEST', 's3.configuration.key', 'conf/s3_files_merge_batch/configuration.txt' ),
			
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.server', 'DLPP-TEST-SentryDatasets.db.sentry.local' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.name', 'SentryDatasets' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.authentication', 'Windows' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.secure.file.path', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.failover', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.pool.minPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.pool.maxPoolSize', '50' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.pool.initialPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.pool.maxIdelTime', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'support.database.pool.maxStatements', '300' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'data.sentry.rest.base.path', 'https://datatest.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'snowflake.server', 'sentry.us-east-2.aws.snowflakecomputing.com/' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'snowflake.useProxy', 'true' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'snowflake.proxyHost', 'serverproxyqual.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'snowflake.proxyPort', '80' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'eg.snowflake.proxyHost', 'app-proxy-nonprod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'eg.snowflake.proxyPort', '8080' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'ld.sdk', 'sdk-ed5ba68e-57e9-425f-b902-41734886628d' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'eg.ld.proxy.flag', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'eg.ld.proxy.address', 'app-proxy-nonprod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'TEST', 'eg.ld.proxy.port', '8080' ),

			( 'FSTTESTBATCHHANDLER', 'QUAL', 'app.key2', 'value12' ),
			( 'FSTQUALBATCHHANDLER', 'QUAL', 'app.key1', 'value11' ),

			( 'SNDQUALBATCHHANDLER', 'QUAL', 'app.key1', 'value21' ),
			( 'SNDQUALBATCHHANDLER', 'QUAL', 'app.key2', 'value22' ),

			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.server', 'dblo-qual-plur.dbal.sentry.local' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.name', 'QUAL_PLUR' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.authentication', 'Windows' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.secure.file.path', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.failover', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.pool.minPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.pool.maxPoolSize', '50' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.pool.initialPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.pool.maxIdelTime', '0' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'support.database.pool.maxStatements', '300' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'data.sentry.rest.base.path', 'https://dataqual.sentry.com' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'configuration.queries', 'QUAL/DATABASELOADERPLUR/TableConfiguration.csv' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'QUAL', 'folder.queries', 'QUAL/DATABASELOADERPLUR/queries/' ),

			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.server', 'DLPP-QUAL-SentryDatasets.dbal.sentry.local' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.name', 'SentryDatasets' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.authentication', 'Windows' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.secure.file.path', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.failover', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'support.database.pool.maxStatements', '300' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'data.sentry.rest.base.path', 'https://dataqual.sentry.com' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'QUAL', 'data.sentry.check.published', 'SELECT 1 FROM [dbo].[FileDetector] WHERE [FilePath] = ''{FilePath}'' AND [FileCreatedDTM] = ''{FileCreatedDTM}'' AND [FileModifiedDTM] = ''{FileModifiedDTM}'';' ),
			( 'DFSFILEDETECTORHANDLER', 'QUAL', 'data.sentry.update.published', 'INSERT INTO [dbo].[FileDetector] ([FilePath], [FileCreatedDTM], [FileModifiedDTM], [CreatedDTM]) VALUES (''{FilePath}'', ''{FileCreatedDTM}'', ''{FileModifiedDTM}'', GETDATE());' ),

			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.server', 'DLPP-QUAL-SentryDatasets.dbal.sentry.local' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.name', 'SentryDatasets' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.authentication', 'Windows' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.secure.file.path', '' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.failover', '' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'support.database.pool.maxStatements', '300' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'sql.read.filereply', 'SELECT ''sentry-dataset-management-np'' AS [SourceBucket], DatasetFileReply.* FROM [DatasetFileReply] WHERE [ReplyStatus] = ''A'';' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'sql.update.filereply', 'UPDATE [DatasetFileReply] SET [ReplyStatus] = {ReplyStatus} WHERE [DatasetFileReply_Id] = {DatasetFileReply_Id};' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'data.sentry.rest.base.path', 'https://dataqual.sentry.com' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'aws.version', 'ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 'target.topic', 'DATA-LOCAL-GOLDENEYE-000000' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 's3.configuration.bucket', 'sentry-dlst-qual-dataset-ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'QUAL', 's3.configuration.key', 'control/dfs_to_s3_batch/dfstos3configuration01.csv' ),

 			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.server', 'DLPP-QUAL-SentryDatasets.dbal.sentry.local' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.name', 'SentryDatasets' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.authentication', 'Windows' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.secure.file.path', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.failover', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.pool.minPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.pool.maxPoolSize', '50' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.pool.initialPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.pool.maxIdelTime', '0' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'support.database.pool.maxStatements', '300' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'data.sentry.rest.base.path', 'https://dataqual.sentry.com' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'configuration.queries', 'QUAL/DSCDISPATCHERBATCH/TableConfiguration.csv' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 'folder.queries', 'QUAL/DSCDISPATCHERBATCH/queries/' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 's3.configuration.bucket', 'sentry-dlst-qual-dataset-ae2' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'QUAL', 's3.configuration.key', 'conf/dsc_dispatcher_batch/configuration.txt' ),

			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.server', 'DLPP-QUAL-SentryDatasets.db.sentry.local' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.name', 'SentryDatasets' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.authentication', 'Windows' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.secure.file.path', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.failover', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.pool.minPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.pool.maxPoolSize', '50' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.pool.initialPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.pool.maxIdelTime', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'support.database.pool.maxStatements', '300' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'data.sentry.rest.base.path', 'https://dataqual.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'snowflake.server', 'sentry.us-east-2.aws.snowflakecomputing.com/' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'snowflake.useProxy', 'true' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'snowflake.proxyHost', 'serverproxyqual.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'snowflake.proxyPort', '80' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'eg.snowflake.proxyHost', 'app-proxy-nonprod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'eg.snowflake.proxyPort', '8080' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'ld.sdk', 'sdk-6dea26fc-9169-4c86-a89f-78b93691a711' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'eg.ld.proxy.flag', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'eg.ld.proxy.address', 'app-proxy-nonprod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'QUAL', 'eg.ld.proxy.port', '8080' ),

			( 'FSTTESTBATCHHANDLER', 'PROD', 'app.key2', 'value12' ),
			( 'FSTPRODBATCHHANDLER', 'PROD', 'app.key1', 'value11' ),

			( 'SNDPRODBATCHHANDLER', 'PROD', 'app.key1', 'value21' ),
			( 'SNDPRODBATCHHANDLER', 'PROD', 'app.key2', 'value22' ),

			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.server', 'dblo-prod-plur.dbal.sentry.local' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.name', 'PLUR' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.authentication', 'Windows' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.secure.file.path', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.failover', '' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.pool.minPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.pool.maxPoolSize', '50' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.pool.initialPoolSize', '10' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.pool.maxIdelTime', '0' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'support.database.pool.maxStatements', '300' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'data.sentry.rest.base.path', 'https://data.sentry.com' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'configuration.queries', 'PROD/DATABASELOADERPLUR/TableConfiguration.csv' ),
			( 'DATABASELOADERPLURBATCHHANDLER', 'PROD', 'folder.queries', 'PROD/DATABASELOADERPLUR/queries/' ),

			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.server', 'DLPP-PROD-SentryDatasets.dbal.sentry.local' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.name', 'SentryDatasets' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.authentication', 'Windows' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.secure.file.path', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.failover', '' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'support.database.pool.maxStatements', '300' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'data.sentry.rest.base.path', 'https://data.sentry.com' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'data.sentry.check.published', 'SELECT 1 FROM [dbo].[FileDetector] WHERE [FilePath] = ''{FilePath}'' AND [FileCreatedDTM] = ''{FileCreatedDTM}'' AND [FileModifiedDTM] = ''{FileModifiedDTM}'';' ),
			( 'DFSFILEDETECTORBATCHHANDLER', 'PROD', 'data.sentry.update.published', 'INSERT INTO [dbo].[FileDetector] ([FilePath], [FileCreatedDTM], [FileModifiedDTM], [CreatedDTM]) VALUES (''{FilePath}'', ''{FileCreatedDTM}'', ''{FileModifiedDTM}'', GETDATE());' ),

			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.server', 'DLPP-PROD-SentryDatasets.dbal.sentry.local' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.name', 'SentryDatasets' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.authentication', 'Windows' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.secure.file.path', '' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.failover', '' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.pool.minPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.pool.maxPoolSize', '50' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.pool.initialPoolSize', '10' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.pool.maxIdelTime', '0' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'support.database.pool.maxStatements', '300' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'sql.read.filereply', 'SELECT ''sentry-dataset-management-np'' AS [SourceBucket], DatasetFileReply.* FROM [DatasetFileReply] WHERE [ReplyStatus] = ''A'';' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'sql.update.filereply', 'UPDATE [DatasetFileReply] SET [ReplyStatus] = {ReplyStatus} WHERE [DatasetFileReply_Id] = {DatasetFileReply_Id};' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'data.sentry.rest.base.path', 'https://data.sentry.com' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'aws.version', 'ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 'target.topic', 'DATA-LOCAL-GOLDENEYE-000000' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 's3.configuration.bucket', 'sentry-dlst-prod-dataset-ae2' ),
			( 'DFSTOS3BATCHHANDLER', 'PROD', 's3.configuration.key', 'control/dfs_to_s3_batch/dfstos3configuration01.csv' ),

 			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.server', 'DLPP-PROD-SentryDatasets.dbal.sentry.local' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.name', 'SentryDatasets' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.authentication', 'Windows' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.secure.file.path', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.failover', '' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.pool.minPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.pool.maxPoolSize', '50' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.pool.initialPoolSize', '10' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.pool.maxIdelTime', '0' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'support.database.pool.maxStatements', '300' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'data.sentry.rest.base.path', 'https://data.sentry.com' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'configuration.queries', 'PROD/DSCDISPATCHERBATCH/TableConfiguration.csv' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 'folder.queries', 'PROD/DSCDISPATCHERBATCH/queries/' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 's3.configuration.bucket', 'sentry-dlst-prod-dataset-ae2' ),
			( 'DSCDISPATCHERBATCHHANDLER', 'PROD', 's3.configuration.key', 'conf/dsc_dispatcher_batch/configuration.txt' ),

			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.server', 'DLPP-PROD-SentryDatasets.db.sentry.local' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.name', 'SentryDatasets' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.authentication', 'Windows' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.secure.file.path', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.failover', '' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.pool.minPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.pool.maxPoolSize', '50' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.pool.initialPoolSize', '10' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.pool.maxIdelTime', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'support.database.pool.maxStatements', '300' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'data.sentry.rest.base.path', 'https://data.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'snowflake.server', 'sentry.us-east-2.aws.snowflakecomputing.com/' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'snowflake.useProxy', 'true' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'snowflake.proxyHost', 'serverproxy.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'snowflake.proxyPort', '80' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'eg.snowflake.proxyHost', 'app-proxy-prod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'eg.snowflake.proxyPort', '8080' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'ld.sdk', 'sdk-2c8a53e0-46a8-4b37-9753-418de81db1a2' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'eg.ld.proxy.flag', '0' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'eg.ld.proxy.address', 'app-proxy-prod.sentry.com' ),
			( 'SNOWFLAKEREFRESHBATCHHANDLER', 'PROD', 'eg.ld.proxy.port', '8080' )
			
		)
		AS Source ([Application], [Environment], [ConfigurationKey], [ConfigurationValue]) 

		ON TARGET.[Application] = SOURCE.[Application] AND TARGET.[Environment] = SOURCE.[Environment] AND TARGET.[ConfigurationKey] = SOURCE.[ConfigurationKey]

		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Application] = SOURCE.[Application],
				[Environment] = SOURCE.[Environment],
				[ConfigurationKey] = SOURCE.[ConfigurationKey],
				[ConfigurationValue] = SOURCE.[ConfigurationValue]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Application], [Environment], [ConfigurationKey], [ConfigurationValue]) 
			VALUES ([Application], [Environment], [ConfigurationKey], [ConfigurationValue])  
					  
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
BEGIN TRAN 
	BEGIN TRY
		DECLARE @DataSourceEnv VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
		DECLARE	@DfsDropLocation VARCHAR(255)
		if @DataSourceEnv = 'NRDEV'
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
			END
		else if @DataSourceEnv = 'DEV'
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
			END
		else if @DataSourceEnv = 'NRTEST'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader_NRTest/'
			END
		else if @DataSourceEnv = 'TEST'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader/test/'
			END
		else if @DataSourceEnv = 'QUAL'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs_nonprod/DatasetLoader/'
			END
		else if @DataSourceEnv = 'PROD'
			BEGIN
				SET @DfsDropLocation = 'file:////sentry.com/appfs/DatasetLoader/'
			END
		else
			BEGIN
				SET @DfsDropLocation = 'file:///c:/tmp/DatasetLoader/'
			END
		
		

		MERGE INTO DataSource AS Target 
		USING (VALUES 
									('DFS Drop Location','DFS drop location monitored by data processing platform',@DfsDropLocation,0,'DFSDataFlowBasic',1,'59A19624-AB2','2020-03-23 09:58:13.850','2020-03-23 09:58:13.850', NULL, '072984','072984')
								)
								AS Source (Source_NME, Source_DSC, BaseUri, IsUriEditable_IND, SourceType_IND, SourceAuth_ID, KeyCode_CDE, [Created_DTM], [Modified_DTM], [Bucket_NME], [PrimaryContact_ID], [PrimaryOwner_ID]) 

		ON Target.KeyCode_CDE = Source.KeyCode_CDE
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
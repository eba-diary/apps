
BEGIN TRAN 
	BEGIN TRY 

		MERGE INTO FeatureEntity AS Target 
		USING (VALUES 
									('CLA2671_RefactoredDataFlows','0000315,0000314',NULL,'Dataflows used for dark lauch testing of refactored events end to end'),
									('CLA2671_RefactorEventsToJava','sentry-data-nrdev-dataset-ae2||temp-file/uncompresszip/',NULL,'Dataflows used for dark lauch testing of refactored events end to end'),
									('CLA3240_UseDropLocationV2','False','CLA3240_UseDropLocationV2','If true, uses the ProducerS3Drop_v2 Data Action Type when creating new Data Flows.  If false, uses the original ProducerS3Drop Data Action Type.')
								)
								AS Source (KeyCol, [Value], [Name], [Description]) 

		ON Target.KeyCol = Source.KeyCol
		WHEN MATCHED THEN 
			-- update name/description of rows - BUT DON'T UPDATE THE VALUE
			UPDATE SET 
				[Name] = Source.[Name],
				[Description] = Source.[Description]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (KeyCol, [Value], [Name], [Description])
			VALUES (KeyCol, [Value], [Name], [Description]);

		--never delete FeatureEntity rows, since the Java team manages them via scripts


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_FeatureEntity_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_FeatureEntity_ErrorSeverity INT; 
		DECLARE @Merge_FeatureEntity_ErrorState INT; 
  
		SELECT 
			@Merge_FeatureEntity_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_FeatureEntity_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_FeatureEntity_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_FeatureEntity_ErrorMessage, 
				   @Merge_FeatureEntity_ErrorSeverity, 
				   @Merge_FeatureEntity_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 

COMMIT TRAN
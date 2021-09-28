
BEGIN TRAN 
	BEGIN TRY 

		MERGE INTO FeatureEntity AS Target 
		USING (VALUES 
									('CLA2671_RefactoredDataFlows','0000315,0000314',NULL,'Dataflows used for dark launch testing of refactored events end to end'),
									('CLA2671_RefactorEventsToJava','sentry-data-nrdev-dataset-ae2||temp-file/uncompresszip/',NULL,'Dataflows used for dark launch testing of refactored events end to end'),
									('CLA3240_UseDropLocationV2','False','CLA3240_UseDropLocationV2','If true, uses the ProducerS3Drop_v2 Data Action Type when creating new Data Flows.  If false, uses the original ProducerS3Drop Data Action Type.'),
									('CLA3241_DisableDfsDropLocation','False','CLA3241_DisableDfsDropLocation','If true, DSC will no longer create the DFS drop location. If false, DSC will continue to create the DFS drop location.'),
									('CLA1656_DataFlowEdit_SubmitEditPage','False','CLA1656_DataFlowEdit_SubmitEditPage','If true, administrators have ability to submit dataflow edits and all other users receive Forbidden page.  If false, all users receive Foridden page.'),
									('CLA1656_DataFlowEdit_ViewEditPage','True','CLA1656_DataFlowEdit_ViewEditPage','If true, administrators have ability to view DataFlowEdit and all other users receive Forbidden page.  If false, all users receive Foridden page.'),
									('CLA3332_ConsolidatedDataFlows','False','CLA3332_ConsolidatedDataFlows','If true, DSC will only create a single flow, and only allow one flow per schema. If false, DSC will create separate data producer and schema flows, and allow multiple flows per schema.'),
									('CLA3329_Expose_HR_Category','False','CLA3329_Expose_HR_Category','If true, show new HR category and mow jereds lawn.'),
									('CLA3048_StandardizeOnUTCTime','False','CLA3048_StandardizeOnUTCTime','Flag for selecting UTC time')
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

BEGIN TRAN 
	BEGIN TRY 

		MERGE INTO FeatureEntity AS Target 
		USING (VALUES 
									('CLA3241_DisableDfsDropLocation','False','CLA3241_DisableDfsDropLocation','If true, DSC will no longer create the DFS drop location. If false, DSC will continue to create the DFS drop location.'),
									('CLA3048_StandardizeOnUTCTime','False','CLA3048_StandardizeOnUTCTime','Flag for selecting UTC time'),
									('CLA3819_EgressEdgeMigration','False','CLA3819_EgressEdgeMigration','If true, will use new Edge Egress proxy.  If false, will use sentry proxy.')
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
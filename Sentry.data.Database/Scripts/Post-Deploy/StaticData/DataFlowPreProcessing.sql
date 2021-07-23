BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO DataFlowPreProcessingOptions AS Target 
		USING (VALUES 
									(1, 'Google API', NULL),
									(2, 'Claim IQ', NULL)
								)
								AS Source ([Id], [Name], [Description]) 

		ON Target.[Id] = Source.[Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Id] = Source.[Id],  
				[Name] = Source.[Name],
				[Description] = Source.[Description]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Id], [Name], [Description]) 
			VALUES ([Id], [Name], [Description])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataFlowPreProcessingOptions_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataFlowPreProcessingOptions_ErrorSeverity INT; 
		DECLARE @Merge_DataFlowPreProcessingOptions_ErrorState INT; 
  
		SELECT 
			@Merge_DataFlowPreProcessingOptions_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataFlowPreProcessingOptions_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataFlowPreProcessingOptions_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataFlowPreProcessingOptions_ErrorMessage, 
				   @Merge_DataFlowPreProcessingOptions_ErrorSeverity, 
				   @Merge_DataFlowPreProcessingOptions_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
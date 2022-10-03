BEGIN TRAN    
    BEGIN TRY    
        MERGE INTO [DataFlowPreProcessingTypes] AS Target    
        USING (VALUES 
          (1,	'googleapi', 'Google API'),
		  (2,	'claimiq', 'Claim IQ'),
		  (3,	'googlebigqueryapi', 'Google BigQuery API')
        )
        AS Source ([ID], [Name], [Description])
        ON Target.[ID] = Source.[ID]   
        -- update matched rows    
        WHEN MATCHED THEN 
        UPDATE SET [Name] = Source.[Name], [Description] = Source.[Description]
        -- insert new rows    
        WHEN NOT MATCHED BY TARGET THEN 
        INSERT ([ID], [Name], [Description])
        VALUES ([ID], [Name], [Description])
        -- delete rows that are in the target but not the source    
        WHEN NOT MATCHED BY SOURCE THEN 
        DELETE;  
	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataFlowPreProcessingTypes_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataFlowPreProcessingTypes_ErrorSeverity INT; 
		DECLARE @Merge_DataFlowPreProcessingTypes_ErrorState INT;   
		  
		SELECT 
			@Merge_DataFlowPreProcessingTypes_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataFlowPreProcessingTypes_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataFlowPreProcessingTypes_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataFlowPreProcessingTypes_ErrorMessage, 
				   @Merge_DataFlowPreProcessingTypes_ErrorSeverity, 
				   @Merge_DataFlowPreProcessingTypes_ErrorState 
				   ); 

	    PRINT ERROR_MESSAGE();
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
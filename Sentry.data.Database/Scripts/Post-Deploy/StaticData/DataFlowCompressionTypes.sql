BEGIN TRAN    
    BEGIN TRY    
        MERGE INTO [DataFlowCompressionTypes] AS Target    
        USING (VALUES 
          (0,	'ZIP', null),
		  (1,	'GZIP', null)
        )
        AS Source ([ID])
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
		DECLARE @Merge_DataFlowCompressionTypes_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataFlowCompressionTypes_ErrorSeverity INT; 
		DECLARE @Merge_DataFlowCompressionTypes_ErrorState INT;   
		  
		SELECT 
			@Merge_DataFlowCompressionTypes_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataFlowCompressionTypes_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataFlowCompressionTypes_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataFlowCompressionTypes_ErrorMessage, 
				   @Merge_DataFlowCompressionTypes_ErrorSeverity, 
				   @Merge_DataFlowCompressionTypes_ErrorState 
				   ); 

	    PRINT ERROR_MESSAGE();
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
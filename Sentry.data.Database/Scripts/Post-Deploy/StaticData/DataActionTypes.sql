BEGIN TRAN    
    BEGIN TRY    
		SET IDENTITY_INSERT [DataActionTypes] ON;  
        MERGE INTO [DataActionTypes] AS Target    
        USING (VALUES 
          (1, 'S3 Drop'),
		  (2, 'Raw Storage'),
		  (3, 'Query Storage'),
		  (4, 'Schema Load'),
		  (5, 'Convert to Parquet')
        )    
        AS Source ([ID], [Name])
        ON Target.[ID] = Source.[ID]   
        -- update matched rows    
        WHEN MATCHED THEN 
        UPDATE SET [Name] = Source.[Name]  
        -- insert new rows    
        WHEN NOT MATCHED BY TARGET THEN 
        INSERT ([ID], [Name])
        VALUES ([ID], [Name])    
        -- delete rows that are in the target but not the source    
        WHEN NOT MATCHED BY SOURCE THEN 
        DELETE;  
		SET IDENTITY_INSERT [DataActionTypes] OFF;  
	END TRY 
	BEGIN CATCH   
		SELECT 
			@ErrorMessage = ERROR_MESSAGE(), 
			@ErrorSeverity = ERROR_SEVERITY(), 
			@ErrorState = ERROR_STATE(); 
  
		RAISERROR (@ErrorMessage, 
				   @ErrorSeverity, 
				   @ErrorState 
				   ); 

	    PRINT ERROR_MESSAGE();
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
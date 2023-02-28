BEGIN TRAN    
    BEGIN TRY    
        MERGE INTO [DataActionTypes] AS Target    
        USING (VALUES 
          (1,	'S3 Drop'),
		  (2,	'Raw Storage'),
		  (3,	'Query Storage'),
		  (4,	'Schema Load'),
		  (5,	'Convert to Parquet'),
		  (6,	'Uncompress Zip'),
		  (7,	'Uncompress Gzip'),
		  (8,	'Schema Map'),
		  (9,	'Google Api'),
		  (10,	'ClaimIQ'),
		  (11,	'Fixed Width'),
		  (12,	'Producer S3 Drop'),
		  (13,	'XML'),
		  (14,	'JSONFlattening'),
		  (15,	'Google BigQuery API'),
		  (16,	'Google Search Console API'),
		  (17,	'Copy To Parquet')
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
	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataActionTypes_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataActionTypes_ErrorSeverity INT; 
		DECLARE @Merge_DataActionTypes_ErrorState INT;   
		  
		SELECT 
			@Merge_DataActionTypes_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataActionTypes_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataActionTypes_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataActionTypes_ErrorMessage, 
				   @Merge_DataActionTypes_ErrorSeverity, 
				   @Merge_DataActionTypes_ErrorState 
				   ); 

	    PRINT ERROR_MESSAGE();
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
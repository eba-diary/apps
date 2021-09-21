
BEGIN TRAN 
	BEGIN TRY 

		MERGE INTO DatasetScopeTypes AS Target 
		USING (VALUES 
									('Point-in-Time','A copy of data at a given point in time.  Data consumption will focus on the latest file that has been uploaded.  Data may be repeated across files.',1),
									('Appending','New data arrives in each file.  The new file can be appended to previous files for the full data picture.  Data will not be repeated across files.',1),
									('Floating-Window','Datafile contains data over a defined period of time.  As new data is added, oldest data is dropped off.  Data is repeated across files.',1)
								)
								AS Source ([Name], Type_DSC, IsEnabled_IND) 

		ON Target.[Name] = Source.[Name]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				Type_DSC = Source.Type_DSC,
				IsEnabled_IND = Source.IsEnabled_IND


		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Name], Type_DSC, IsEnabled_IND)
			VALUES ([Name], Type_DSC, IsEnabled_IND)
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DatasetScopeTypes_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DatasetScopeTypes_ErrorSeverity INT; 
		DECLARE @Merge_DatasetScopeTypes_ErrorState INT; 
  
		SELECT 
			@Merge_DatasetScopeTypes_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DatasetScopeTypes_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DatasetScopeTypes_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DatasetScopeTypes_ErrorMessage, 
				   @Merge_DatasetScopeTypes_ErrorSeverity, 
				   @Merge_DatasetScopeTypes_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 

COMMIT TRAN
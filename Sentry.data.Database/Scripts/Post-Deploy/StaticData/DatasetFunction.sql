BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO DatasetFunction AS Target 
		USING (VALUES 
									(1, 'Management', 1),
									(2, 'Operations',  2),
									(3, 'PL State Management', 3),
									(4, 'Pricing',  4),
									(5, 'Sales',  5),
									(6, 'Underwriting', 6)
								)
								AS Source (DatasetFunction_Id, [Name], [Sequence]) 

		ON Target.DatasetFunction_Id = Source.DatasetFunction_Id
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				DatasetFunction_Id = Source.DatasetFunction_Id,  
				[Name] = Source.[Name],
				[Sequence] = Source.[Sequence]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (DatasetFunction_Id, [Name], [Sequence]) 
			VALUES (DatasetFunction_Id, [Name], [Sequence])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DatasetFunction_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DatasetFunction_ErrorSeverity INT; 
		DECLARE @Merge_DatasetFunction_ErrorState INT; 
  
		SELECT 
			@Merge_DatasetFunction_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DatasetFunction_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DatasetFunction_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DatasetFunction_ErrorMessage, 
				   @Merge_DatasetFunction_ErrorSeverity, 
				   @Merge_DatasetFunction_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
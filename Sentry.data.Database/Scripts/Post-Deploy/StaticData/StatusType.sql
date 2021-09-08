
BEGIN TRAN 
	BEGIN TRY 
		
		SET IDENTITY_INSERT StatusType ON

		MERGE INTO StatusType AS Target 
		USING (VALUES 
									('1','Started'),
									('2','In Progress'),
									('3','Success'),
									('4','Error')
								)
								AS Source (Status_ID, [Description]) 

		ON Target.Status_ID = Source.Status_ID
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Description] = Source.[Description]


		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (Status_ID, [Description])
			VALUES (Status_ID, [Description])
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_StatusType_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_StatusType_ErrorSeverity INT; 
		DECLARE @Merge_StatusType_ErrorState INT; 
  
		SELECT 
			@Merge_StatusType_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_StatusType_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_StatusType_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_StatusType_ErrorMessage, 
				   @Merge_StatusType_ErrorSeverity, 
				   @Merge_StatusType_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
	SET IDENTITY_INSERT StatusType OFF

COMMIT TRAN
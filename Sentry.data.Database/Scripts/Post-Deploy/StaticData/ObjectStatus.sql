BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO ObjectStatus AS Target 
		USING (VALUES 
									(1, 'Active', 'Fully functional'),
									(2, 'Pending Delete', 'Not editiable, no activity will occur for object, disabled in UI'),
									(3, 'Deleted', 'Not visible in UI, all jobs and dataflows are disabled'),
									(4, 'Disabled', 'Temporarily disabled, no activity will occur for object, disabled in UI'),
									(5, 'Pending Delete Failure', 'Pending delete failure awaiting resolution, not editiable, no activity will occur for object, disabled in UI')
								)
								AS Source ([ObjectStatus_Id], [ObjectStatus_CDE], [ObjectStatus_DSC]) 

		ON Target.[ObjectStatus_Id] = Source.[ObjectStatus_Id] AND Target.[ObjectStatus_CDE] = Source.[ObjectStatus_CDE]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[ObjectStatus_CDE] = Source.[ObjectStatus_CDE],
				[ObjectStatus_DSC] = Source.[ObjectStatus_DSC]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([ObjectStatus_Id], [ObjectStatus_CDE], [ObjectStatus_DSC])
			VALUES ([ObjectStatus_Id], [ObjectStatus_CDE], [ObjectStatus_DSC])
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_ObjectStatus_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_ObjectStatus_ErrorSeverity INT; 
		DECLARE @Merge_ObjectStatus_ErrorState INT; 
  
		SELECT 
			@Merge_ObjectStatus_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_ObjectStatus_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_ObjectStatus_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_ObjectStatus_ErrorMessage, 
				   @Merge_ObjectStatus_ErrorSeverity, 
				   @Merge_ObjectStatus_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
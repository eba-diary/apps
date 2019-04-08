BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessAreaTileRow_BusinessAreaTile AS Target 
		USING (VALUES 
									(1, 1),
									(1, 2),
									(2, 3),
									(2, 4),
									(1, 5)
								)
								AS Source ([BusinessAreaTileRow_ID], [BusinessAreaTile_ID]) 

		ON Target.[BusinessAreaTileRow_ID] = Source.[BusinessAreaTileRow_ID]
		AND Target.[BusinessAreaTile_ID] = Source.[BusinessAreaTile_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessAreaTileRow_ID] = Source.[BusinessAreaTileRow_ID],  
				[BusinessAreaTile_ID] = Source.[BusinessAreaTile_ID]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessAreaTileRow_ID], [BusinessAreaTile_ID]) 
			VALUES ([BusinessAreaTileRow_ID], [BusinessAreaTile_ID])
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorSeverity INT; 
		DECLARE @Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorMessage, 
				   @Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorSeverity, 
				   @Merge_BusinessAreaTileRow_BusinessAreaTile_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
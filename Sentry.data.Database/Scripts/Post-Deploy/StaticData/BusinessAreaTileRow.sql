BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessAreaTileRow AS Target 
		USING (VALUES 
									(1, 2, 1, 1),
									(2, 3, 1, 2)
								)
								AS Source ([BusinessAreaTileRow_ID], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ]) 

		ON Target.[BusinessAreaTileRow_ID] = Source.[BusinessAreaTileRow_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessAreaTileRow_ID] = Source.[BusinessAreaTileRow_ID],  
				[NbrOfColumns_CNT] = Source.[NbrOfColumns_CNT],
				[BusinessArea_ID] = Source.[BusinessArea_ID],
				[Order_SEQ] = Source.[Order_SEQ]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessAreaTileRow_ID], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ]) 
			VALUES ([BusinessAreaTileRow_ID], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessAreaTileRow_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessAreaTileRow_ErrorSeverity INT; 
		DECLARE @Merge_BusinessAreaTileRow_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessAreaTileRow_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessAreaTileRow_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessAreaTileRow_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessAreaTileRow_ErrorMessage, 
				   @Merge_BusinessAreaTileRow_ErrorSeverity, 
				   @Merge_BusinessAreaTileRow_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
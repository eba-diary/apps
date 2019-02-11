BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessAreaTileRow AS Target 
		USING (VALUES 
									(1, 2, 1, 1),
									(2, 3, 1, 2)
								)
								AS Source ([BusinessAreaTitleRow_ID], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ]) 

		ON Target.[BusinessAreaTileRow_Id] = Source.[BusinessAreaTileRow_Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessAreaTileRow_Id] = Source.[BusinessAreaTileRow_Id],  
				[NbrOfColumns_CNT] = Source.[NbrOfColumns_CNT],
				[BusinessArea_ID] = Source.[BusinessArea_ID],
				[Order_SEQ] = Source.[Order_SEQ]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessAreaTileRow_Id], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ]) 
			VALUES ([BusinessAreaTileRow_Id], [NbrOfColumns_CNT], [BusinessArea_ID], [Order_SEQ])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessAreaTitleRow_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessAreaTitleRow_ErrorSeverity INT; 
		DECLARE @Merge_BusinessAreaTitleRow_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessAreaTitleRow_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessAreaTitleRow_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessAreaTitleRow_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessAreaTitleRow_ErrorMessage, 
				   @Merge_BusinessAreaTitleRow_ErrorSeverity, 
				   @Merge_BusinessAreaTitleRow_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessAreaTile AS Target 
		USING (VALUES 
									(1, 'PL Report & Data Requests', 'blue', '[image_name]', 'http://www.google.com/', 1, 'View Requests'),
									(2, 'Business Intelligence', 'lt_blue', '[image_name]', 'https://data.sentry.com/Search/BusinessIntelligence/Index?category=Personal%20Lines', 2, 'View Business Intelligence'),
									(3, 'Training', 'green', '[image_name]', 'http://sharepoint.sentry.com/', 3, 'View Training Materials'),
									(4, 'PL Data Services Backlog', 'gold', '[image_name]', 'https://jira.sentry.com/', 4, 'View Current Items'),
									(5, 'PL BI Backlog', 'gray', '[image_name]', 'https://jira.sentry.com/', 5, 'View Current Items')
								)
								AS Source ([BusinessAreaTitle_ID], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC]) 

		ON Target.[BusinessAreaTile_Id] = Source.[BusinessAreaTile_Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessAreaTile_Id] = Source.[BusinessAreaTile_Id],  
				[Title_DSC] = Source.[Title_DSC],
				[TileColor_DSC] = Source.[TileColor_DSC],
				[Image_NME] = Source.[Image_NME],
				[Hyperlink_URL] = Source.[Hyperlink_URL],
				[Order_SEQ] = Source.[Order_SEQ],
				[Hyperlink_DSC] = Source.[Hyperlink_DSC]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessAreaTile_Id], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC]) 
			VALUES ([BusinessAreaTile_Id], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessAreaTitle_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessAreaTitle_ErrorSeverity INT; 
		DECLARE @Merge_BusinessAreaTitle_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessAreaTitle_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessAreaTitle_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessAreaTitle_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessAreaTitle_ErrorMessage, 
				   @Merge_BusinessAreaTitle_ErrorSeverity, 
				   @Merge_BusinessAreaTitle_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
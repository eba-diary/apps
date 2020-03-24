BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessAreaTile AS Target 
		USING (VALUES 
									(1, 'PL Report & Data Requests', 'blue', 'Collaboration5.jpg', 'http://sharepoint.sentry.com/cp/EDWBusInf/Lists/EDW%20Information%20Request/Quick%20View.aspx', 1, 'View Requests'),
									(2, 'Business Intelligence', 'lt_blue', 'Meeting2.jpg', 'https://data.sentry.com/Search/BusinessIntelligence/Index?category=Personal%20Lines', 2, 'View Business Intelligence'),
									(3, 'Training', 'green', 'LearningBooks.jpg', 'https://confluence.sentry.com/display/PLBI/Personal+Lines+Business+Intelligence+', 4, 'View Training Materials'),
									(4, 'PL Data Services Business Dashboard', 'gold', 'Meeting3.jpg', 'https://jira.sentry.com/secure/Dashboard.jspa?selectPageId=19211', 5, 'View Current Items'),
									(5, 'PL Data Questions', 'gray', 'Collaboration1.jpg', 'http://sharepoint.sentry.com/cp/EDWBusInf/Lists/PL%20Data%20Questions/AllItems.aspx', 3, 'View Current Items')
								)
								AS Source ([BusinessAreaTile_ID], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC]) 

		ON Target.[BusinessAreaTile_ID] = Source.[BusinessAreaTile_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessAreaTile_ID] = Source.[BusinessAreaTile_ID],  
				[Title_DSC] = Source.[Title_DSC],
				[TileColor_DSC] = Source.[TileColor_DSC],
				[Image_NME] = Source.[Image_NME],
				[Hyperlink_URL] = Source.[Hyperlink_URL],
				[Order_SEQ] = Source.[Order_SEQ],
				[Hyperlink_DSC] = Source.[Hyperlink_DSC]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessAreaTile_ID], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC]) 
			VALUES ([BusinessAreaTile_ID], [Title_DSC], [TileColor_DSC], [Image_NME], [Hyperlink_URL], [Order_SEQ], [Hyperlink_DSC])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessAreaTile_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessAreaTile_ErrorSeverity INT; 
		DECLARE @Merge_BusinessAreaTile_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessAreaTile_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessAreaTile_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessAreaTile_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessAreaTile_ErrorMessage, 
				   @Merge_BusinessAreaTile_ErrorSeverity, 
				   @Merge_BusinessAreaTile_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
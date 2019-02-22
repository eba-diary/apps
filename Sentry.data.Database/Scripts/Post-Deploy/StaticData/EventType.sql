BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO EventType AS Target 
		USING (VALUES 
									(1, 'Created File', 1,1),
									(2, 'Bundle File Process', 1,1),
									(3, 'Upload Failure', 5,1),
									(4, 'Modified Dataset', 2,0),
									(5, 'Current File Created', 1,0),
									(6, 'Viewed', 1,0),
									(7, 'Search', 1,0),
									(8, 'Created Dataset', 1,0),
									(9, 'Downloaded Data File', 1,0),
									(10, 'Previewed Data File', 1,0),
									(11, 'Edited Data File', 1,0),
									(12, 'Pushed Data File to SAS', 1,0),
									(13, 'Clicked Item in Feed', 1,0),
									(14, 'Created Report', 1,0),
									(15, 'Updated Report', 1,0),
									(16, 'Updated Dataset', 1,0),
									(17, 'Viewed Report', 1,0),
									(18, 'Viewed Dataset', 1,0),
									(19, 'Created Tag', 1,0),
									(19, 'Updated Tag', 1,0)
								)
								AS Source ([Type_ID], [Description], Severity, Display_IND) 

		ON Target.[Type_ID] = Source.[Type_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Type_ID] = Source.[Type_ID],  
				[Description] = Source.[Description],
				Severity = Source.Severity,
				Display_IND = Source.Display_IND

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Type_ID], [Description], Severity, Display_IND) 
			VALUES ([Type_ID], [Description], Severity, Display_IND)  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_EventType_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_EventType_ErrorSeverity INT; 
		DECLARE @Merge_EventType_ErrorState INT; 
  
		SELECT 
			@Merge_EventType_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_EventType_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_EventType_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_EventType_ErrorMessage, 
				   @Merge_EventType_ErrorSeverity, 
				   @Merge_EventType_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
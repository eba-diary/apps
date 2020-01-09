BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO EventType AS Target 
		USING (VALUES 
									(1, 'Created File', 1,1,'DATASET'),
									(2, 'Bundle File Process', 1,0,'DATASET'),
									(3, 'Upload Failure', 5,1,'DATASET'),
									(4, 'Modified Dataset', 2,0,'DATASET'),
									(5, 'Current File Created', 1,0,'DATASET'),
									(6, 'Viewed', 1,0,'DATASET'),
									(7, 'Search', 1,0,'DATASET'),
									(8, 'Created Dataset', 1,0,'DATASET'),
									(9, 'Downloaded Data File', 1,0,'DATASET'),
									(10, 'Previewed Data File', 1,0,'DATASET'),
									(11, 'Edited Data File', 1,0,'DATASET'),
									(12, 'Pushed Data File to SAS', 1,0,'DATASET'),
									(13, 'Clicked Item in Feed', 1,0,'DATASET'),
									(14, 'Created Report', 1,0,'DATASET'),
									(15, 'Updated Report', 1,0,'DATASET'),
									(16, 'Updated Dataset', 1,0,'DATASET'),
									(17, 'Viewed Report', 1,0,'DATASET'),
									(18, 'Viewed Dataset', 1,0,'DATASET'),
									(19, 'Created Tag', 1,0,'DATASET'),
									(20, 'Updated Tag', 1,0,'DATASET'),
									(21, 'Deleted Report', 1,0,'DATASET'),
									(22, 'Created Data Source', 1,0,'DATASET'),
									(23, 'Updated Data Source', 1,0,'DATASET'),
									(24, 'Deleted Dataset', 1,0,'DATASET'),
									(25, 'Downloaded Report', 1,0,'DATASET'),
									(26, 'Sync Schema', 1,0,'DATASET'),
									(27, 'Notifications', 1,1,'BUSINESSAREA')
								)
								AS Source ([Type_ID], [Description], Severity, Display_IND, [Group_CDE]) 

		ON Target.[Type_ID] = Source.[Type_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Type_ID] = Source.[Type_ID],  
				[Description] = Source.[Description],
				Severity = Source.Severity,
				Display_IND = Source.Display_IND,
				[Group_CDE] = Source.[Group_CDE]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Type_ID], [Description], Severity, Display_IND,[Group_CDE]) 
			VALUES ([Type_ID], [Description], Severity, Display_IND,[Group_CDE])  
					  
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
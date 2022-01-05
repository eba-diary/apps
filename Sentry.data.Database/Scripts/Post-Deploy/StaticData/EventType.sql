BEGIN TRAN 
	BEGIN TRY 

		DECLARE	@GroupDATASET VARCHAR(25)				= 'DATASET'
				,@GroupBUSINESSAREA VARCHAR(25)			= 'BUSINESSAREA'
				,@GroupDALE VARCHAR(25)					= 'DALE'
				,@GroupBUSINESSAREA_DSC VARCHAR(25)		= 'BUSINESSAREA_DSC'
		
		MERGE INTO EventType AS Target 
		USING (VALUES 
									(1, 'Created File', 1,1,@GroupDATASET),
									(2, 'Bundle File Process', 1,0,@GroupDATASET),
									(3, 'Upload Failure', 5,1,@GroupDATASET),
									(4, 'Modified Dataset', 2,0,@GroupDATASET),
									(5, 'Current File Created', 1,0,@GroupDATASET),
									(6, 'Viewed', 1,0,@GroupDATASET),
									(7, 'Search', 1,0,@GroupDATASET),
									(8, 'Created Dataset', 1,0,@GroupDATASET),
									(9, 'Downloaded Data File', 1,0,@GroupDATASET),
									(10, 'Previewed Data File', 1,0,@GroupDATASET),
									(11, 'Edited Data File', 1,0,@GroupDATASET),
									(12, 'Pushed Data File to SAS', 1,0,@GroupDATASET),
									(13, 'Clicked Item in Feed', 1,0,@GroupDATASET),
									(14, 'Created Report', 1,0,@GroupDATASET),
									(15, 'Updated Report', 1,0,@GroupDATASET),
									(16, 'Updated Dataset', 1,0,@GroupDATASET),
									(17, 'Viewed Report', 1,0,@GroupDATASET),
									(18, 'Viewed Dataset', 1,0,@GroupDATASET),
									(19, 'Created Tag', 1,0,@GroupDATASET),
									(20, 'Updated Tag', 1,0,@GroupDATASET),
									(21, 'Deleted Report', 1,0,@GroupDATASET),
									(22, 'Created Data Source', 1,0,@GroupDATASET),
									(23, 'Updated Data Source', 1,0,@GroupDATASET),
									(24, 'Deleted Dataset', 1,0,@GroupDATASET),
									(25, 'Downloaded Report', 1,0,@GroupDATASET),
									(26, 'Sync Schema', 1,0,@GroupDATASET),
									
									
									(27, 'Critical Notification'		,1	,1	,@GroupBUSINESSAREA),
									(28, 'Warning Notification'			,2	,1	,@GroupBUSINESSAREA),
									(29, 'Info Notification'			,3	,1	,@GroupBUSINESSAREA),

									(30, 'Critical Notification Add'	,1	,0	,@GroupBUSINESSAREA),
									(31, 'Warning Notification Add'		,2	,0	,@GroupBUSINESSAREA),
									(32, 'Info Notification Add'		,3	,0	,@GroupBUSINESSAREA),

									(33, 'Critical Notification Update'	,1	,0	,@GroupBUSINESSAREA),
									(34, 'Warning Notification Update'	,2	,0	,@GroupBUSINESSAREA),
									(35, 'Info Notification Update'		,3	,0	,@GroupBUSINESSAREA),

									(36, 'DaleQuery'							,3	,0	,@GroupDALE),
									
									(37, 'Deleted Dataset Schema', 1,0,@GroupDATASET),
									(38, 'Created Dataset Schema', 1,0,@GroupDATASET),

									--BUSINESSAREA_DSC is another group that falls under the BUSINESSAREA umbrella
									--I created another EventTypeGroup because BUSINESSAREA PL and DSC don't share the same Subscription Events
									--and therefore don't share the same EventTypeGroup which dictates what EventTypes they can subscribe too
									(39, 'Release Notes', 1,1,@GroupBUSINESSAREA_DSC),
									(40, 'Technical Documentation', 1,1,@GroupBUSINESSAREA_DSC),
									(41, 'News', 1,1,@GroupBUSINESSAREA_DSC)



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
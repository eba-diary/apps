BEGIN TRAN 
	BEGIN TRY 

		DECLARE	@GroupDATASET VARCHAR(25)							= 'DATASET'
				,@GroupBUSINESSAREA VARCHAR(25)						= 'BUSINESSAREA'
				,@GroupDALE VARCHAR(25)								= 'DALE'
				,@GroupBUSINESSAREA_DSC VARCHAR(25)					= 'BUSINESSAREA_DSC'
				,@GroupBUSINESSAREA_DSC_RELEASENOTES VARCHAR(60)	= 'BUSINESSAREA_DSC_RELEASENOTES'
				,@GroupBUSINESSAREA_DSC_NEWS VARCHAR(60)			= 'BUSINESSAREA_DSC_NEWS'
		
		MERGE INTO EventType AS Target 
		USING (VALUES 
									(1, 'Created File'					,1,1,@GroupDATASET				,'Created File'),
									(2, 'Bundle File Process'			,1,0,@GroupDATASET				,'Bundle File Process'),
									(3, 'Upload Failure'				,5,1,@GroupDATASET				,'Upload Failure'),
									(4, 'Modified Dataset'				,2,0,@GroupDATASET				,'Modified Dataset'),
									(5, 'Current File Created'			,1,0,@GroupDATASET				,'Current File Created'),
									(6, 'Viewed'						,1,0,@GroupDATASET				,'Viewed'),
									(7, 'Search'						,1,0,@GroupDATASET				,'Search'),
									(8, 'Created Dataset'				,1,1,@GroupBUSINESSAREA_DSC		,'Dataset'),
									(9, 'Downloaded Data File'			,1,0,@GroupDATASET				,'Downloaded Data File'),
									(10, 'Previewed Data File'			,1,0,@GroupDATASET				,'Previewed Data File'),
									(11, 'Edited Data File'				,1,0,@GroupDATASET				,'Edited Data File'),
									(12, 'Pushed Data File to SAS'		,1,0,@GroupDATASET				,'Pushed Data File to SAS'),
									(13, 'Clicked Item in Feed'			,1,0,@GroupDATASET				,'Clicked Item in Feed'),
									(14, 'Created Report'				,1,1,@GroupBUSINESSAREA_DSC		,'Business Intelligence'),
									(15, 'Updated Report'				,1,0,@GroupDATASET				,'Updated Report'),
									(16, 'Updated Dataset'				,1,0,@GroupDATASET				,'Updated Dataset'),
									(17, 'Viewed Report'				,1,0,@GroupDATASET				,'Viewed Report'),
									(18, 'Viewed Dataset'				,1,0,@GroupDATASET				,'Viewed Dataset'),
									(19, 'Created Tag'					,1,0,@GroupDATASET				,'Created Tag'),
									(20, 'Updated Tag'					,1,0,@GroupDATASET				,'Updated Tag'),
									(21, 'Deleted Report'				,1,0,@GroupDATASET				,'Deleted Report'),
									(22, 'Created Data Source'			,1,0,@GroupDATASET				,'Created Data Source'),
									(23, 'Updated Data Source'			,1,0,@GroupDATASET				,'Updated Data Source'),
									(24, 'Deleted Dataset'				,1,0,@GroupDATASET				,'Deleted Dataset'),
									(25, 'Downloaded Report'			,1,0,@GroupDATASET				,'Downloaded Report'),
									(26, 'Sync Schema'					,1,0,@GroupDATASET				,'Sync Schema'),
									
									
									(27, 'Critical Notification'		,1	,1	,@GroupBUSINESSAREA		,'Critical Notification'),
									(28, 'Warning Notification'			,2	,1	,@GroupBUSINESSAREA		,'Warning Notification'),
									(29, 'Info Notification'			,3	,1	,@GroupBUSINESSAREA		,'Info Notification'),

									(30, 'Critical Notification Add'	,1	,0	,@GroupBUSINESSAREA		,'Critical Notification Add'),
									(31, 'Warning Notification Add'		,2	,0	,@GroupBUSINESSAREA		,'Warning Notification Add'),
									(32, 'Info Notification Add'		,3	,0	,@GroupBUSINESSAREA		,'Info Notification Add'),

									(33, 'Critical Notification Update'	,1	,0	,@GroupBUSINESSAREA		,'Critical Notification Update'),
									(34, 'Warning Notification Update'	,2	,0	,@GroupBUSINESSAREA		,'Warning Notification Update'),
									(35, 'Info Notification Update'		,3	,0	,@GroupBUSINESSAREA		,'Info Notification Update'),

									(36, 'DaleQuery'					,3	,0	,@GroupDALE				,'DaleQuery'),
									
									(37, 'Deleted Dataset Schema'		,1,0,@GroupDATASET				,'Deleted Dataset Schema'),
									(38, 'Created Dataset Schema'		,1,1,@GroupBUSINESSAREA_DSC		,'Schema'),

									--BUSINESSAREA_DSC is another group that falls under the BUSINESSAREA umbrella
									--I created another EventTypeGroup because BUSINESSAREA PL and DSC don't share the same Subscription Events
									--and therefore don't share the same EventTypeGroup which dictates what EventTypes they can subscribe too
									(39, 'Release Notes'				,1,1,@GroupBUSINESSAREA_DSC		,'Release Notes'),
									(40, 'Technical Documentation'		,1,1,@GroupBUSINESSAREA_DSC		,'Technical Documentation'),
									(41, 'News'							,1,1,@GroupBUSINESSAREA_DSC		,'News'),

									--@GroupBUSINESSAREA_DSC_RELEASENOTES is a group that falls under the @GroupBUSINESSAREA_DSC_RELEASENOTES umbrella
									(42, 'Release Notes DSC'			,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'DSC'),
									(43, 'Release Notes CL'				,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'CL'),
									(44, 'Release Notes PL'				,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'PL'),
									(45, 'Release Notes LifeAnnuity'	,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'LifeAnnuity'),
									(46, 'Release Notes Claims'			,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'Claims'),
									(47, 'Release Notes Corporate'		,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'Corporate'),


									--@GroupBUSINESSAREA_DSC_RELEASENOTES is a group that falls under the @GroupBUSINESSAREA_DSC_NEWS umbrella
									(48, 'News DSC'						,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'DSC'),
									(49, 'News Tableau'					,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Tableau'),
									(50, 'News Python'					,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Python'),
									(51, 'News SAS'						,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'SAS'),
									(52, 'News Analytics'				,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Analytics')





								)
								AS Source ([Type_ID], [Description], Severity, Display_IND, [Group_CDE], [DisplayName]) 

		ON Target.[Type_ID] = Source.[Type_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Type_ID] = Source.[Type_ID],  
				[Description] = Source.[Description],
				Severity = Source.Severity,
				Display_IND = Source.Display_IND,
				[Group_CDE] = Source.[Group_CDE],
				[DisplayName] = Source.[DisplayName]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Type_ID], [Description], Severity, Display_IND,[Group_CDE], [DisplayName] ) 
			VALUES ([Type_ID], [Description], Severity, Display_IND,[Group_CDE], [DisplayName])  
					  
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
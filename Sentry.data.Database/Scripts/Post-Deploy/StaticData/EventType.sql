BEGIN TRAN 
	BEGIN TRY 

		DECLARE	@GroupDATASET VARCHAR(25)							= 'DATASET'
				,@GroupBUSINESSAREA VARCHAR(25)						= 'BUSINESSAREA'
				,@GroupDALE VARCHAR(25)								= 'DALE'
				,@GroupBUSINESSAREA_DSC VARCHAR(25)					= 'BUSINESSAREA_DSC'
				,@GroupBUSINESSAREA_DSC_RELEASENOTES VARCHAR(60)	= 'BUSINESSAREA_DSC_RELEASENOTES'
				,@GroupBUSINESSAREA_DSC_NEWS VARCHAR(60)			= 'BUSINESSAREA_DSC_NEWS'
				,@GroupDATA_INVENTORY VARCHAR(60)					= 'DATA_INVENTORY'
				,@GroupDATASETFILE VARCHAR(60)						= 'DATASETFILE'
		
		MERGE INTO EventType AS Target 
		USING (VALUES 
									(1, 'Created File'					,1,1,@GroupDATASET				,'Created File'							,null),
									(2, 'Bundle File Process'			,1,0,@GroupDATASET				,'Bundle File Process'					,null),
									(3, 'Upload Failure'				,5,1,@GroupDATASET				,'Upload Failure'						,null),
									(4, 'Modified Dataset'				,2,0,@GroupDATASET				,'Modified Dataset'						,null),
									(5, 'Current File Created'			,1,0,@GroupDATASET				,'Current File Created'					,null),
									(6, 'Viewed'						,1,0,@GroupDATASET				,'Viewed'								,null),
									(7, 'Search'						,1,0,@GroupDATASET				,'Search'								,null),
									(8, 'Created Dataset'				,1,1,@GroupBUSINESSAREA_DSC		,'Dataset'								,null),
									(9, 'Downloaded Data File'			,1,0,@GroupDATASET				,'Downloaded Data File'					,null),
									(10, 'Previewed Data File'			,1,0,@GroupDATASET				,'Previewed Data File'					,null),
									(11, 'Edited Data File'				,1,0,@GroupDATASET				,'Edited Data File'						,null),
									(12, 'Pushed Data File to SAS'		,1,0,@GroupDATASET				,'Pushed Data File to SAS'				,null),
									(13, 'Clicked Item in Feed'			,1,0,@GroupDATASET				,'Clicked Item in Feed'					,null),
									(14, 'Created Report'				,1,1,@GroupBUSINESSAREA_DSC		,'Business Intelligence'				,null),
									(15, 'Updated Report'				,1,0,@GroupDATASET				,'Updated Report'						,null),
									(16, 'Updated Dataset'				,1,0,@GroupDATASET				,'Updated Dataset'						,null),
									(17, 'Viewed Report'				,1,0,@GroupDATASET				,'Viewed Report'						,null),
									(18, 'Viewed Dataset'				,1,0,@GroupDATASET				,'Viewed Dataset'						,null),
									(19, 'Created Tag'					,1,0,@GroupDATASET				,'Created Tag'							,null),
									(20, 'Updated Tag'					,1,0,@GroupDATASET				,'Updated Tag'							,null),
									(21, 'Deleted Report'				,1,0,@GroupDATASET				,'Deleted Report'						,null),
									(22, 'Created Data Source'			,1,0,@GroupDATASET				,'Created Data Source'					,null),
									(23, 'Updated Data Source'			,1,0,@GroupDATASET				,'Updated Data Source'					,null),
									(24, 'Deleted Dataset'				,1,0,@GroupDATASET				,'Deleted Dataset'						,null),
									(25, 'Downloaded Report'			,1,0,@GroupDATASET				,'Downloaded Report'					,null),
									(26, 'Sync Schema'					,1,0,@GroupDATASET				,'Sync Schema'							,null),
									
									
									(27, 'Critical Notification'		,1	,1	,@GroupBUSINESSAREA		,'Critical Notification'				,null),
									(28, 'Warning Notification'			,2	,1	,@GroupBUSINESSAREA		,'Warning Notification'					,null),
									(29, 'Info Notification'			,3	,1	,@GroupBUSINESSAREA		,'Info Notification'					,null),

									(30, 'Critical Notification Add'	,1	,0	,@GroupBUSINESSAREA		,'Critical Notification Add'			,null),
									(31, 'Warning Notification Add'		,2	,0	,@GroupBUSINESSAREA		,'Warning Notification Add'				,null),
									(32, 'Info Notification Add'		,3	,0	,@GroupBUSINESSAREA		,'Info Notification Add'				,null),

									(33, 'Critical Notification Update'	,1	,0	,@GroupBUSINESSAREA		,'Critical Notification Update'			,null),
									(34, 'Warning Notification Update'	,2	,0	,@GroupBUSINESSAREA		,'Warning Notification Update'			,null),
									(35, 'Info Notification Update'		,3	,0	,@GroupBUSINESSAREA		,'Info Notification Update'				,null),

									(36, 'DaleQuery'					,3	,0	,@GroupDALE				,'DaleQuery'							,null),
									
									(37, 'Deleted Dataset Schema'		,1,0,@GroupDATASET				,'Deleted Dataset Schema'				,null),
									(38, 'Created Dataset Schema'		,1,1,@GroupBUSINESSAREA_DSC		,'Schema'								,null),

									--BUSINESSAREA_DSC is another group that falls under the BUSINESSAREA umbrella
									--I created another EventTypeGroup because BUSINESSAREA PL and DSC don't share the same Subscription Events
									--and therefore don't share the same EventTypeGroup which dictates what EventTypes they can subscribe too
									(39, 'Release Notes'				,1,1,@GroupBUSINESSAREA_DSC		,'Release Notes'						,null),
									(40, 'Technical Documentation'		,1,1,@GroupBUSINESSAREA_DSC		,'Technical Documentation'				,null),
									(41, 'News'							,1,1,@GroupBUSINESSAREA_DSC		,'News'									,null),


									--@GroupBUSINESSAREA_DSC_RELEASENOTES is a group that falls under the @GroupBUSINESSAREA_DSC_RELEASENOTES umbrella
									(42, 'Release Notes DSC'			,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'DSC'					,'Release Notes'),
									(43, 'Release Notes CL'				,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'CL'					,'Release Notes'),
									(44, 'Release Notes PL'				,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'PL'					,'Release Notes'),
									(45, 'Release Notes LifeAnnuity'	,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'LifeAnnuity'			,'Release Notes'),
									(46, 'Release Notes Claims'			,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'Claims'				,'Release Notes'),
									(47, 'Release Notes Corporate'		,1,1,@GroupBUSINESSAREA_DSC_RELEASENOTES		,'Corporate'			,'Release Notes'),


									--@GroupBUSINESSAREA_DSC_RELEASENOTES is a group that falls under the @GroupBUSINESSAREA_DSC_NEWS umbrella
									(48, 'News DSC'						,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'DSC'					,'News'),
									(49, 'News Tableau'					,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Tableau'				,'News'),
									(50, 'News Python'					,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Python'				,'News'),
									(51, 'News SAS'						,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'SAS'					,'News'),
									(52, 'News Analytics'				,1,1,@GroupBUSINESSAREA_DSC_NEWS				,'Analytics'			,'News'),

									(53, 'DataInventoryQuery'			,3,0,@GroupDATA_INVENTORY						,'DataInventoryQuery'							,null),
									(54, 'DatasetFileDeleteS3'			,1,0,@GroupDATASETFILE							,'DatasetFileDeleteS3'							,null),
									(55, 'DatasetFileUpdateObjectStatus',1,0,@GroupDATASETFILE							,'DatasetFileUpdateObjectStatus'				,null)
								)
								AS Source ([Type_ID], [Description], Severity, Display_IND, [Group_CDE], [DisplayName], [ParentDescription]) 

		ON Target.[Type_ID] = Source.[Type_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Type_ID] = Source.[Type_ID],  
				[Description] = Source.[Description],
				Severity = Source.Severity,
				Display_IND = Source.Display_IND,
				[Group_CDE] = Source.[Group_CDE],
				[DisplayName] = Source.[DisplayName],
				[ParentDescription] = Source.[ParentDescription]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Type_ID], [Description], Severity, Display_IND,[Group_CDE], [DisplayName],[ParentDescription] ) 
			VALUES ([Type_ID], [Description], Severity, Display_IND,[Group_CDE], [DisplayName],[ParentDescription])  
					  
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
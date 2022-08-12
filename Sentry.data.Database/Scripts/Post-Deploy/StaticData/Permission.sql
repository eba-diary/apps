BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO Permission AS Target 
		USING (VALUES 
									(1, 'CanPreviewDataset', 'Preview Dataset', 'Access to dataset metadata', 'Dataset'),
									(2, 'CanViewFullDataset', 'View Full Dataset', 'Access to full dataset with download capability', 'Dataset'),
									(3, 'CanUploadToDataset', 'Upload to Dataset', 'Access to upload data files to dataset', 'Dataset'),
									(4, 'CanQueryDataset', 'Query Dataset', 'Access to query dataset using the Query Tool', 'Dataset'),
									(5, 'CanUseDataSource', 'Use Data Source', 'Access to use Data Source', 'DataSource'),
									(6, 'CanModifyNotification', 'Modify Notifications', 'Access to modify notifications for Data.sentry.com', 'DataAsset'),
									(7, 'CanModifyNotification', 'Modify Notifications', 'Access to modify notifications for Data.sentry.com', 'BusinessArea'),
									(8, 'CanManageSchema', 'Manage Schema', 'Access to manage schema within dataset', 'Dataset'),
									(9, 'S3Access', 'S3 Access', 'Direct access to read this dataset from S3', 'Dataset'),
									(10, 'SnowflakeAccess', 'Snowflake Access', 'Access to read this dataset from Snowflake', 'Dataset'),
									(11, 'InheritParentPermissions', 'Inherit Parent Permissions', 'Indicates this dataset inherits permissions from the asset', 'Dataset'),
									(12, 'CanManageDataflow', 'Manage Dataflow', 'Access to manage dataflow', 'Dataflow')
									
									--(7, 'CanConnectToDataset', 'Connect to Dataset', 'Access to connect to dataset using business intelligence tools', 'Dataset'),
									--(enter id here, 'CanConnectToDataset', 'Connect to Dataset', 'Access to connect to dataset using business intelligence tools', 'Dataset'),
								)
								AS Source ([Permission_ID], [Permission_CDE], [Permission_NME], [Permission_DSC], [SecurableObject_TYP]) 

		ON Target.[Permission_ID] = Source.[Permission_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Permission_ID] = Source.[Permission_ID],  
				[Permission_CDE] = Source.[Permission_CDE],
				[Permission_NME] = Source.[Permission_NME],
				[Permission_DSC] = Source.[Permission_DSC],
				[SecurableObject_TYP] = Source.[SecurableObject_TYP]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT([Permission_ID], [Permission_CDE], [Permission_NME], [Permission_DSC], [SecurableObject_TYP])
			VALUES ([Permission_ID], [Permission_CDE], [Permission_NME], [Permission_DSC], [SecurableObject_TYP])
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_Permission_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_Permission_ErrorSeverity INT; 
		DECLARE @Merge_Permission_ErrorState INT; 
  
		SELECT 
			@Merge_Permission_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_Permission_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_Permission_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_Permission_ErrorMessage, 
				   @Merge_Permission_ErrorSeverity, 
				   @Merge_Permission_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN

BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO [Security] AS Target 
		USING (VALUES 
									('4383792F-6B08-4D7F-A86E-A9FA00B5E05D','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('EAB492DE-13CB-4201-A829-A9FA00C90B40','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('4B62F049-69F7-4620-99F9-A9FA00C91260','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('5C55E2A2-13E8-4DA2-A97A-A9FA00C91E8C','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('E8BE35D2-77E7-4AA9-94E2-A9FA00C93D5F','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('1C17C7A4-5B23-4639-B836-A9FA00C94CBA','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('82B137EE-7D1C-4AEA-B6AA-A9FA00C96640','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('570DD10D-EEE7-47BA-A3F0-A9FA00C96B34','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('AEB7D1FE-F021-46F0-8CEF-A9FA00C97054','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),
									('4D95EA59-FE0B-4C45-AD6A-A9FA00C973C0','DataAsset','2019-06-18 08:25:20.143','2019-06-18 08:25:20.143',NULL,NULL,'000000'),

									('619c7006-68ca-4d58-ae6a-acabb562dc19','BusinessArea','2020-04-01 11:02:32.347','2020-04-01 11:02:32.347',NULL,NULL,'000000')
								)
								AS Source (Security_ID, SecurableEntity_NME, Created_DTM, Enabled_DTM, Removed_DTM, UpdatedBy_ID, CreatedBy_ID) 

		ON Target.Security_ID = Source.Security_ID
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				SecurableEntity_NME = Source.SecurableEntity_NME,  
				Created_DTM = Source.Created_DTM,
				Enabled_DTM = Source.Enabled_DTM,
				Removed_DTM = Source.Removed_DTM,
				UpdatedBy_ID = Source.UpdatedBy_ID,
				CreatedBy_ID = Source.CreatedBy_ID


		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (Security_ID, SecurableEntity_NME, Created_DTM, Enabled_DTM, Removed_DTM, UpdatedBy_ID, CreatedBy_ID)
			VALUES (Security_ID, SecurableEntity_NME, Created_DTM, Enabled_DTM, Removed_DTM, UpdatedBy_ID, CreatedBy_ID);
		
		--Unlike most reference data, we DON'T want to delete extra rows - as they are legitimate
		--WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			--DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_Security_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_Security_ErrorSeverity INT; 
		DECLARE @Merge_Security_ErrorState INT; 
  
		SELECT 
			@Merge_Security_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_Security_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_Security_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_Security_ErrorMessage, 
				   @Merge_Security_ErrorSeverity, 
				   @Merge_Security_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
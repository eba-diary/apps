
BEGIN TRAN 
	BEGIN TRY 

		MERGE INTO FileExtension AS Target 
		USING (VALUES 
									('JSON','5/18/2018 7:42:39 AM','072984'),
									('CSV','5/18/2018 7:42:39 AM','072984'),
									('TXT','5/18/2018 7:42:39 AM','072984'),
									('XLSX','5/18/2018 7:42:39 AM','072984'),
									('ANY','5/18/2018 7:42:39 AM','072984'),
									('XML','5/18/2018 7:42:39 AM','072984'),
									('DELIMITED','2/4/2019 10:22:28 AM','082698'),
									('FIXEDWIDTH','2/4/2019 10:22:28 AM','072984'),
									('PARQUET','2/14/2023 10:22:28 AM','082116')
								)
								AS Source (Extension_NME, Created_DTM, CreateUser_ID) 

		ON Target.Extension_NME = Source.Extension_NME
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				Created_DTM = Source.Created_DTM,
				CreateUser_ID = Source.CreateUser_ID



		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (Extension_NME, Created_DTM, CreateUser_ID)
			VALUES (Extension_NME, Created_DTM, CreateUser_ID)
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_FileExtension_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_FileExtension_ErrorSeverity INT; 
		DECLARE @Merge_FileExtension_ErrorState INT; 
  
		SELECT 
			@Merge_FileExtension_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_FileExtension_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_FileExtension_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_FileExtension_ErrorMessage, 
				   @Merge_FileExtension_ErrorSeverity, 
				   @Merge_FileExtension_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 

COMMIT TRAN
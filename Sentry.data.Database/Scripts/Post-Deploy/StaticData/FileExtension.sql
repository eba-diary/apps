
BEGIN TRAN 
	BEGIN TRY 
		
		SET IDENTITY_INSERT FileExtension ON

		MERGE INTO FileExtension AS Target 
		USING (VALUES 
									(7,'JSON','5/18/2018 7:42:39 AM','072984'),
									(8,'CSV','5/18/2018 7:42:39 AM','072984'),
									(9,'TXT','5/18/2018 7:42:39 AM','072984'),
									(10,'XLSX','5/18/2018 7:42:39 AM','072984'),
									(11,'ANY','5/18/2018 7:42:39 AM','072984'),
									(12,'XML','5/18/2018 7:42:39 AM','072984'),
									(14,'DELIMITED','2/4/2019 10:22:28 AM','082698'),
									(26,'FIXEDWIDTH','2/4/2019 10:22:28 AM','072984')
								)
								AS Source (Extension_Id, Extension_NME, Created_DTM, CreateUser_ID) 

		ON Target.Extension_Id = Source.Extension_Id
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				Extension_NME = Source.Extension_NME,
				Created_DTM = Source.Created_DTM,
				CreateUser_ID = Source.CreateUser_ID



		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT (Extension_Id, Extension_NME, Created_DTM, CreateUser_ID)
			VALUES (Extension_Id, Extension_NME, Created_DTM, CreateUser_ID)
					  
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
  
	SET IDENTITY_INSERT FileExtension OFF

COMMIT TRAN
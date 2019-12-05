BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessArea AS Target 
		USING (VALUES 
									(1, 'Personal Lines', 'PL')
								)
								AS Source ([BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC]) 

		ON Target.[BusinessArea_Id] = Source.[BusinessArea_Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessArea_Id] = Source.[BusinessArea_Id],  
				[Name_DSC] = Source.[Name_DSC],
				[AbbreviatedName_DSC] = Source.[AbbreviatedName_DSC]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC]) 
			VALUES ([BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessArea_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessArea_ErrorSeverity INT; 
		DECLARE @Merge_BusinessArea_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessArea_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessArea_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessArea_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessArea_ErrorMessage, 
				   @Merge_BusinessArea_ErrorSeverity, 
				   @Merge_BusinessArea_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
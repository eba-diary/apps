BEGIN TRAN 
	BEGIN TRY 
		
		MERGE	INTO BusinessArea						AS TARGET 
		USING	( VALUES	(1, 'Personal Lines', 'PL','079012','070557',1,'619c7006-68ca-4d58-ae6a-acabb562dc19')	)	AS SOURCE ( [BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC], [PrimaryOwner_ID], [PrimaryContact_ID], [IsSecured_IND], [Security_ID] )  
					ON	TARGET.[BusinessArea_Id]		= SOURCE.[BusinessArea_Id]
		
		WHEN	MATCHED 
		THEN	UPDATE 
			SET	[BusinessArea_Id]						= SOURCE.[BusinessArea_Id],  
				[Name_DSC]								= SOURCE.[Name_DSC],
				[AbbreviatedName_DSC]					= SOURCE.[AbbreviatedName_DSC],
				[PrimaryOwner_ID]						= SOURCE.[PrimaryOwner_ID],
				[PrimaryContact_ID]						= SOURCE.[PrimaryContact_ID],
				[IsSecured_IND]							= SOURCE.[IsSecured_IND],
				[Security_ID]							= SOURCE.[Security_ID]


		WHEN NOT MATCHED BY TARGET THEN 
			INSERT ([BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC], [PrimaryOwner_ID], [PrimaryContact_ID], [IsSecured_IND], [Security_ID]) 
			VALUES ([BusinessArea_Id], [Name_DSC], [AbbreviatedName_DSC], [PrimaryOwner_ID], [PrimaryContact_ID], [IsSecured_IND], [Security_ID])  
					  
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
BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO AuthenticationType AS Target 
		USING (VALUES 
									(1, 'AnonAuth', 'Anonymous Authentication', 'Provides generic credentials with the following properties: User="anonymous" and Password=(generic email address)'),
									(2, 'BasicAuth', 'Basic Authentication', 'User supplied user\password credentials.  User required to work with site Administrators to store credentials.'),
									(3, 'TokenAuth', 'Token Authentication', 'Utilizes a token within the request header to authenticate to site.  User is required to aquire token from source site and enter header name and token below.  NOTE: Token values will be encrypted before being saved.'),
									(4, 'OAuth', 'OAuth 2.0 Authentication', 'Utilizes OAuth flow to retrieve accesstoken to pull data from source')
								)
								AS Source ([Auth_Id], [AuthType_CDE], [Display_NME], [Description]) 

		ON Target.[Auth_Id] = Source.[Auth_Id] AND Target.[AuthType_CDE] = Source.[AuthType_CDE]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[AuthType_CDE] = Source.[AuthType_CDE],
				[Display_NME] = Source.[Display_NME],
				[Description] = Source.[Description]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Auth_Id], [AuthType_CDE], [Display_NME], [Description])
			VALUES ([Auth_Id], [AuthType_CDE], [Display_NME], [Description])
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_AuthenticationType_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_AuthenticationType_ErrorSeverity INT; 
		DECLARE @Merge_AuthenticationType_ErrorState INT; 
  
		SELECT 
			@Merge_AuthenticationTypen_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_AuthenticationType_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_AuthenticationType_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_AuthenticationType_ErrorMessage, 
				   @Merge_AuthenticationType_ErrorSeverity, 
				   @Merge_AuthenticationType_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
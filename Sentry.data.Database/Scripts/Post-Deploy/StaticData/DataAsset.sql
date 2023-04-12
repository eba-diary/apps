
BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO DataAsset AS Target 
		USING (VALUES 
									(1, 'SERACL', 'SERA CL', 'SERACLTopology.vsdx', 'SERACL'						,'https://sentryinsurance.sharepoint.com/sites/29935/Pages/SERA%20CL/SERA%20CL%20Home.aspx', 'ITCLDSTeam@sentry.com', 'SERA CL is implemented as an Analytical Data Store (ADS). SERA CL was created specifically to support the analysis of the business segment of Property & Casualty - Business Products', 'SERA CL', 'CL', 'SERA_BP.dm1', 1, '070650','066284', '4383792F-6B08-4D7F-A86E-A9FA00B5E05D'),
									(2, 'SERAPL', 'SERA PL', 'SERAPLTopology.vsdx', 'SERAPL'						,'https://confluence.sentry.com/display/DSR/SERA+PL+Consumer+Guide+-+Home', 'SERAPLSupport@sentry.com', 'SERA PL is an Analytical Data Store (ADS) for Personal lines. SERA PL was created to support non-standard auto and personal auto lines of business.', 'SERA PL', 'PL', 'SERA PL.dm1', 1, '065520','078863', 'EAB492DE-13CB-4201-A829-A9FA00C90B40'),
									(3, 'PCRCL', 'PCR CL', 'PCRTopology.vsdx', NULL									,NULL, 'PCRBureauSupport@sentry.com', NULL, 'PCR CL', 'CL', NULL, 1, '077028','054314', '4B62F049-69F7-4620-99F9-A9FA00C91260'),
									(4, 'PCRPL', 'PCR PL', 'PCRPLTopology.vsdx', NULL								,NULL, 'PCRBureauSupport@sentry.com', NULL, 'PCR PL', 'PL', NULL, 1, '077028','054314', '5C55E2A2-13E8-4DA2-A97A-A9FA00C91E8C'),
									(5, 'CLPolicyODS', 'CL Policy ODS', 'CLPolicyODSTopology.vsdx', 'CLPolicyODS'	,'https://sentryinsurance.sharepoint.com/sites/29935/Pages/CL%20Policy%20ODS/CL%20Policy%20ODS%20Home.aspx', 'ITCLDSTeam@sentry.com', 'The Commercial Lines (CL) Policy ODS was implemented as an Operational Data Store (ODS).  It was created to integrate policy data from multiple source systems at Sentry Insurance.', 'CL Policy ODS', 'CL', 'CL_Policy_ODS.dm1', 1, '070650','066284', 'E8BE35D2-77E7-4AA9-94E2-A9FA00C93D5F'),
									(6, 'PLPolicyODS', 'PL Policy ODS', 'PLPolicyODSTopology.vsdx', 'PLPolicyODS'	,'https://sentryinsurance.sharepoint.com/sites/29935/Pages/PL%20Policy%20ODS/PL%20Policy%20ODS%20Home.aspx', 'ITPLPolicyODSSupport@sentry.com', 'The Personal Lines (PL) Policy ODS is and Operational Data Store which integrates policy data from multiple source systems as Sentry Insurance. ', 'PL Policy ODS', 'PL', 'CP_Policy_ODS', 1, '065520','076474', '1C17C7A4-5B23-4639-B836-A9FA00C94CBA'),
									(7, 'SERAEnterprise', 'SERA Enterprise', 'SERAEnterpriseTopology.vsdx', NULL	,NULL, 'SERASupport@sentry.com', NULL, 'SERA Enterprise', NULL, NULL, 1, '077028','073973', '82B137EE-7D1C-4AEA-B6AA-A9FA00C96640'),
									(8, 'LASER', 'LASER', 'LaserTopology.vsdx', NULL								,NULL, 'LASERSupport@sentry.com', NULL, 'Laser', NULL, NULL, 1, '077028','075383', '570DD10D-EEE7-47BA-A3F0-A9FA00C96B34'),
									(9, 'ClaimODS', 'Claim ODS', 'ClaimODSTopology.vsdx', 'ClaimODS'				,NULL, 'ITClaimODSSupport@sentry.com', NULL, 'Claim ODS', NULL, 'Claim_ODS.dm1', 1, '077028','077601', 'AEB7D1FE-F021-46F0-8CEF-A9FA00C97054'),
									(10, 'TDM', 'TDM', 'TDMTopology.vsdx', 'TDM'									,NULL, 'ITCLDSTeam@sentry.com', 'The Transportation Data Mart (TDM) is an analytical data store created to support the analysis of the Transportation business unit.', 'TDM', NULL, NULL, 1, '078841','066284', '4D95EA59-FE0B-4C45-AD6A-A9FA00C973C0')
								)
								AS Source ([DataAsset_ID], [DataAsset_NME], Display_NME, ArchDiagram_URL, DataModel_URL, Guide_URL, Contact_EML, DataAsset_DSC, MetadataRepositoryAsset_NME, Line_CDE, Model_NME, IsSecured_IND, PrimaryOwner_ID, PrimaryContact_ID, Security_ID) 

		ON Target.[DataAsset_ID] = Source.[DataAsset_ID]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[DataAsset_ID] = Source.[DataAsset_ID],  
				[DataAsset_NME] = Source.[DataAsset_NME],
				Display_NME = Source.Display_NME,
				ArchDiagram_URL = Source.ArchDiagram_URL,
				DataModel_URL = Source.DataModel_URL,
				Guide_URL = Source.Guide_URL,
				Contact_EML = Source.Contact_EML,
				DataAsset_DSC = Source.DataAsset_DSC,
				MetadataRepositoryAsset_NME = Source.MetadataRepositoryAsset_NME,
				Line_CDE = Source.Line_CDE,
				Model_NME = Source.Model_NME,
				IsSecured_IND = Source.IsSecured_IND,
				PrimaryOwner_ID = Source.PrimaryOwner_ID,
				PrimaryContact_ID = Source.PrimaryContact_ID,
				Security_ID = Source.Security_ID


		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([DataAsset_ID], [DataAsset_NME], Display_NME, ArchDiagram_URL, DataModel_URL, Guide_URL, Contact_EML, DataAsset_DSC, MetadataRepositoryAsset_NME, Line_CDE, Model_NME, IsSecured_IND, PrimaryOwner_ID, PrimaryContact_ID, Security_ID)
			VALUES ([DataAsset_ID], [DataAsset_NME], Display_NME, ArchDiagram_URL, DataModel_URL, Guide_URL, Contact_EML, DataAsset_DSC, MetadataRepositoryAsset_NME, Line_CDE, Model_NME, IsSecured_IND, PrimaryOwner_ID, PrimaryContact_ID, Security_ID)
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_DataAsset_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_DataAsset_ErrorSeverity INT; 
		DECLARE @Merge_DataAsset_ErrorState INT; 
  
		SELECT 
			@Merge_DataAsset_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_DataAsset_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_DataAsset_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_DataAsset_ErrorMessage, 
				   @Merge_DataAsset_ErrorSeverity, 
				   @Merge_DataAsset_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN
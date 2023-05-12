SET @ScriptVersion = 'CLA-5178_UpdateDatasetCategories'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    CREATE TABLE #datasetCategories (
	    DatasetName VARCHAR(100),
	    CategoryId INT
    );

    INSERT INTO #datasetCategories
    VALUES ('Account_Instructions',1),
    ('Bottomline',1),
    ('CCC',1),
    ('ClaimIQ',1),
    ('Copart',1),
    ('Coventry',1),
    ('DecisionPoint',1),
    ('FNOL',1),
    ('FNOL Archive',1),
    ('Inpoint Claims',1),
    ('MessageExtract',1),
    ('ECHO Claims',1),
    ('CAEX Feature Usage',15),
    ('Captain',15),
    ('CL PolicyCenter',15),
    ('Clearinghouse',15),
    ('CustomerOne EPIC Client Quotes',15),
    ('CustomerOneLinking',15),
    ('ECHO-EPIC Clients',15),
    ('ECHO-EPIC Products',15),
    ('S4',15),
    ('Sapphire',15),
    ('Specialty Midwest',15),
    ('Specialty Program Information',15),
    ('SSPO',15),
    ('Agent_Events',16),
    ('Amazon Connect',16),
    ('AmazonTelem',16),
    ('AmazonTrace',16),
    ('Analytics CSO',16),
    ('C1ActivityStream',16),
    ('Calabrio_Call_Evaluation',16),
    ('Clarabridge_Survey',16),
    ('Clarabridge_Survey_QA',16),
    ('CLIVR_Legacy',16),
    ('CLIVR_Legacy_Acct',16),
    ('CnctAgtProd',16),
    ('CnctAgtProdQ',16),
    ('CnctLogInOut',16),
    ('CnctSvcHst',16),
    ('Connect Voicemail Transcripts',16),
    ('ContactTraceRecord_PhoneSurvey',16),
    ('Corvus Aggregations',16),
    ('Corvus Amazon Connect',16),
    ('Corvus Calabrio',16),
    ('Corvus Clarabridge',16),
    ('Corvus CLCRM',16),
    ('Corvus Communications',16),
    ('Corvus CustomerOne',16),
    ('Corvus PLCRM',16),
    ('CRVS_AWSConnectReports',16),
    ('CRVS_Cases',16),
    ('EGTW_ContactLens',16),
    ('EGTW_Verint_WFM_Export',16),
    ('Lex_Utterance_Detected',16),
    ('Lex_Utterance_Missed',16),
    ('PindropReports',16),
    ('WFM_Adherence_Actual_Detail',16),
    ('WFM_Adherence_HighLevel',16),
    ('WFM_Adherence_Scheduled_Detail',16),
    ('Basic Associate Data',17),
    ('CustomerOneNotes',17),
    ('Courier',17),
    ('EPIC Return Mail',17),
    ('Esign Data',17),
    ('Publishing Request Details',17),
    ('Ratify Data',17),
    ('Secure Associate Data',17),
    ('SentryDocs',17),
    ('USPS ZIP Codes',4),
    ('Zipcodes',4),
    ('ActNow',3),
    ('Fatality Analysis Reporting System',3),
    ('Quarterly Census of Employment and Wages',3),
    ('Workday',13),
    ('Agilysys',2),
    ('betterview',2),
    ('CAB Public Data',2),
    ('Experian',2),
    ('HLDI',2),
    ('IHS Markit Data',2),
    ('Lexis Nexis',2),
    ('Motive',2),
    ('Active Directory',18),
    ('AppFoundations',18),
    ('Bitbucket',18),
    ('Change Record',18),
    ('Confluence',18),
    ('Control M',18),
    ('Corvus Enterprise Events',18),
    ('DB Growth Details',18),
    ('DSC Jobs',18),
    ('DSC S3 Inventory',18),
    ('Dynatrace',18),
    ('EventFeed',18),
    ('Functional Telemetry',18),
    ('Gatekeeper Data',18),
    ('HCMU',18),
    ('HPOOIOCUserData',18),
    ('ITPD_Statistics',18),
    ('JIRA',18),
    ('Oculus',18),
    ('OpsGenie ',18),
    ('Powerbeat',18),
    ('Quartermaster',18),
    ('QueryStore',18),
    ('SAID',18),
    ('SASUsageMetrics',18),
    ('SEER',18),
    ('SELG',18),
    ('Skynet',18),
    ('SQL Waitstats Anomaly',18),
    ('ZZQ',18),
    ('ZZQ_v1',18),
    ('ZZZ Test Data',18),
    ('Northern Trust',14),
    ('Sentry Investment Metaverse',14),
    ('BadDebtBatch',19),
    ('CustomerOneLinking_PL',19),
    ('Form Inference',19),
    ('MVRBatch',19),
    ('MYDI - Activity Tracking',19),
    ('PL CRM',19),
    ('PL NCRF',19),
    ('PL PC Change Events',19),
    ('PL PC Hold Data',19),
    ('PL UAM',19),
    ('PL Vendor Data',19),
    ('PL VIN Data',19),
    ('PLAdmin_BC_AccountQuality',19),
    ('PLAdmin_BC_PaymentPlanSelections',19),
    ('PLAdmin_CMC_Data',19),
    ('PLAdmin_PAM_PaymentInstrumentRestriction',19),
    ('PLAdmin_PAM_PayPlanRestrictions',19),
    ('PLAdmin_PAM_ProofFollowup',19),
    ('PLAdmin_PAM_QuoteBlocker',19),
    ('PLAdmin_PAM_TownCodeZipMapping',19),
    ('PLAdmin_PAM_ZipRestrictions',19),
    ('PLAdmin_PC_INCIDENTS',19),
    ('PLAdmin_PC_InitAutoRenewalLkup',19),
    ('PLAdmin_PC_IScoreDefaults',19),
    ('PLAdmin_PC_NonRenewLkup',19),
    ('PLAdmin_PC_NotificationConfigs',19),
    ('PLAdmin_PC_NSFCancelLeadDays',19),
    ('PLAdmin_PC_RateFeesConfig',19),
    ('PLAdmin_PC_RewritePaymentDays',19),
    ('PLAdminRTRIScore',19),
    ('PLAP',19),
    ('PLRateProduct',19),
    ('RMS Factor Trace',19),
    ('SRPL_SCMS',19),
    ('SRPLSalesEnvtCategoryMapping',19),
    ('SRPLTerritoryMapping',19),
    ('SRPLTransformationCategoryMapping',19),
    ('UWRE_MVROrderCost',19),
    ('UWRE_ProducerRecoupMatrix',19),
    ('UWRE_StateDfltRecoupMatrix',19),
    ('Global Historical Climatology Network',5),
    ('Commercial Lines Google Analytics',20),
    ('InsightAgent',20),
    ('InsightCL',20),
    ('InsightRetirement',20),
    ('InsightRiskManagement',20),
    ('Marketing Google',20),
    ('Personal Lines Google Analytics',20),
    ('Retirement Google Analytics',20)

    UPDATE dc
    SET dc.Category_Id = c.CategoryId
    FROM DatasetCategory dc
    JOIN Dataset d
    ON dc.Dataset_Id = d.Dataset_ID
    JOIN #datasetCategories c
    ON d.Dataset_NME = c.DatasetName
    WHERE d.ObjectStatus = 1
    AND d.Dataset_TYP = 'DS'

    DROP TABLE #datasetCategories

    -- END POST-DEPLOY SCRIPT --
    INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 
END TRY 
BEGIN CATCH 
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(), 
        @ErrorSeverity = ERROR_SEVERITY(), 
        @ErrorState = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 

COMMIT TRAN
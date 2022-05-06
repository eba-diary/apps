SET @ScriptVersion = 'CLA3920_Auto_Subscribe'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

		/*
			--PRACTICE QUERY
			Select BAS.*,ET.*
			from BusinessArea_Subscription BAS
			join EventType ET ON ET.Type_ID = BAS.EventType_ID
			Order by SentryOwner_NME
		*/
		INSERT INTO BusinessArea_Subscription
		SELECT DISTINCT 2,ET.[Type_ID],PEOPLE_WITH_SUBSCRIPTIONS.Interval_ID,PEOPLE_WITH_SUBSCRIPTIONS.SentryOwner_NME
		FROM 
		(
			SELECT DISTINCT SentryOwner_NME,ET.Description,ET.Type_ID,Interval_ID
			FROM BusinessArea_Subscription BAS
			JOIN EventType ET ON BAS.EventType_ID = ET.Type_ID
			WHERE	ET.Description IN ('Release Notes','News')
					AND BAS.Interval_ID <> 5
		)AS PEOPLE_WITH_SUBSCRIPTIONS
		JOIN EventType ET  ON PEOPLE_WITH_SUBSCRIPTIONS.Description = ET.ParentDescription		--important thing here is to join parent PA table on ET to ONLY GET CHILDREN
		ORDER BY PEOPLE_WITH_SUBSCRIPTIONS.SentryOwner_NME,ET.Type_ID



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


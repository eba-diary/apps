SET @ScriptVersion = 'CLA-3729-HISTORY-FIX-EVENT-IsProcessed'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --





	/**************************************************************************************************************************************
	This History fix will update Event.IsProcessed for all of history so on Day 1
	select * from [Event]
	***************************************************************************************************************************************/
	--MARK ALL HISTORY IsProcessed = 1
	UPDATE E
	SET E.IsProcessed = 1
	--SELECT E.Reason,E.IsProcessed,ET.Display_IND
	FROM	[Event] E
	JOIN	EventType ET 
				ON E.EventType = ET.[Type_ID]
	WHERE ET.Description IN ( 'Created Dataset','Created Dataset Schema','Created Report')
			AND IsProcessed = 0

			




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


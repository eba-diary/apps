SET @ScriptVersion = 'CLA-4317-HistoryFixControlMTriggerName'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    

	UPDATE S
	SET S.ControlMTriggerName = 'DATA_' + D.NamedEnvironment + '_' + D.Short_NME + '_' + S.Schema_NME + '_COMPLETED'
	--SELECT D.Dataset_ID,D.Dataset_NME,D.NamedEnvironment,D.Short_NME,S.Schema_NME ,'DATA_' + D.NamedEnvironment + '_' + D.Short_NME + '_' + S.Schema_NME + '_COMPLETED'
	FROM dataset D
	JOIN DatasetFileConfigs DFG
		ON DFG.Dataset_ID = D.Dataset_ID
	JOIN [Schema] S on DFG.Schema_Id = S.Schema_Id
	WHERE	S.ControlMTriggerName IS NULL



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


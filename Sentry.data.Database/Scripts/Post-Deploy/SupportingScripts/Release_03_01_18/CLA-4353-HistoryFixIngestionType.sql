SET @ScriptVersion = 'CLA-4353-HistoryFixIngestionType'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    
	--HISTORY FIX
	UPDATE DF
	SET DF.IngestionType = 5  --(IngestionType=Topic=5)
	--select * 
	FROM [schema] S
	JOIN DataFlow DF  ON S.[Schema_Id] = DF.SchemaId
	WHERE	DF.ObjectStatus = 1			--ACTIVE
		AND S.CLA1286_KafkaFlag = 1		



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


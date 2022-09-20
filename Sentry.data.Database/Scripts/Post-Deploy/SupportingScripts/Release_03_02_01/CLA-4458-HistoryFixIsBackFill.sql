SET @ScriptVersion = 'CLA-4458-HistoryFixIsBackFill'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --


    --select * from dataflow
	--HISTORY FIX
	UPDATE DF
	SET DF.IsBackFillRequired = CASE WHEN DF.IngestionType = 4 AND S.CLA1286_KafkaFlag = 1 THEN 1 ELSE 0 END  
	--select DF.Ingestiontype, S.CLA1286_KafkaFlag, DF.IsBackFillRequired,*
	FROM [schema] S
	JOIN DataFlow DF  ON S.[Schema_Id] = DF.SchemaId
	WHERE	DF.ObjectStatus = 1			--ACTIVE



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


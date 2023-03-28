SET @ScriptVersion = 'CLA-5076_SetGlobalDatasetId'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    UPDATE Dataset
    SET GlobalDatasetId = NEXT VALUE FOR seq_GlobalDatasetId
    WHERE ObjectStatus = 1
    AND Dataset_TYP = 'DS'

    SELECT Dataset_ID, MIN(GlobalDatasetId) OVER (PARTITION BY Dataset_NME) as MinGlobalDatasetId
    INTO #globalids
    FROM Dataset
    WHERE ObjectStatus = 1
    AND Dataset_TYP = 'DS'

    UPDATE Dataset
    SET GlobalDatasetId = (SELECT TOP 1 MinGlobalDatasetId
					       FROM #globalids gids
					       WHERE Dataset.Dataset_ID = gids.Dataset_ID)
    WHERE ObjectStatus = 1
    AND Dataset_TYP = 'DS'

    DROP TABLE #globalids

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
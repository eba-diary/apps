SET @ScriptVersion = 'CLA3722_TransitionToAsset'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    -- get list if unique said key codes already being used
    IF OBJECT_ID('tempdb..#UniqueSaidKeys') IS NOT NULL DROP TABLE #UniqueSaidKeys
    -- select distinct SaidKeyCode into #UniqueSaidKeys from Dataset
    -- generate a new GUID for each SAID key code
    IF OBJECT_ID('tempdb..#SecurityEntries') IS NOT NULL DROP TABLE #SecurityEntries
    select SaidKeyCode, NEWID() AS Security_ID into #SecurityEntries from #UniqueSaidKeys

    -- INSERT Security and Asset records
    INSERT INTO [Security]
               ([Security_ID]
               ,[SecurableEntity_NME]
               ,[Created_DTM]
               ,[Enabled_DTM]
               ,[Removed_DTM]
               ,[UpdatedBy_ID]
               ,[CreatedBy_ID])
    SELECT Security_ID, 'Asset', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, null, 'SYSTEM' FROM #SecurityEntries
    INSERT INTO [Asset]
               ([SaidKey_CDE]
               ,[Security_ID])
    SELECT SaidKeyCode, Security_ID FROM #SecurityEntries

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


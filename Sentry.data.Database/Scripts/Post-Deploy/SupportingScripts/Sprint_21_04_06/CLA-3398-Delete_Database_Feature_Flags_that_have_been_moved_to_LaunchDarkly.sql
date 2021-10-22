SET @ScriptVersion = 'CLA-3398-Delete_Database_Feature_Flags_that_have_been_moved_to_LaunchDarkly'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --


    -- These three Feature Flags were originally setup in the database, but have been moved to being managed in LaunchDarkly
    DELETE FROM FeatureEntity WHERE KeyCol in (
        'CLA1656_DataFlowEdit_SubmitEditPage',
        'CLA1656_DataFlowEdit_ViewEditPage',
        'CLA3329_Expose_HR_Category'
    )


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


SET @ScriptVersion = 'CLA3789_Cleanup_RetrieverJobs_Not_Deleted'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    /* Select Pending_Delete RetrieverJobs associated with Deleted DatasetFileConfig records */
    select
    DS.Dataset_NME, 
    DFC.Config_NME,
    RJ.Job_ID as 'J_ID',
    CASE
	    WHEN RJ.DeleteIssuer is null THEN DFC.DeleteIssuer
	    ELSE RJ.DeleteIssuer
    END as 'New_DeleteIssuer',
    CASE
	    WHEN RJ.DeleteIssueDTM is null THEN DFC.DeleteIssueDTM
	    WHEN RJ.DeleteIssueDTM = '9999-12-31 23:59:59.000' THEN DFC.DeleteIssueDTM
	    ELSE RJ.DeleteIssueDTM
    END as 'New_DeleteIssueDTM'
    into #IdentifiedRetrieverJobs
    from RetrieverJob RJ
    join DatasetFileConfigs DFC on
	    RJ.Config_ID = DFC.Config_ID
    join dataset DS on
	    DFC.Dataset_ID = DS.Dataset_ID
    where DFC.ObjectStatus = 3 and RJ. ObjectStatus <> 3
    order by DS.Dataset_NME, DFC.Config_NME

    /* Update idenfified retriever job records */
    UPDATE RetrieverJob
    SET DeleteIssuer = x.New_DeleteIssuer, DeleteIssueDTM = x.New_DeleteIssueDTM
    from #IdentifiedRetrieverJobs x
    where Job_ID = x.J_ID

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


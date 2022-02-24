SET @ScriptVersion = 'CLA3789_Incorrect_Dataflow_Delete_Metadata'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    select 
    DS.Dataset_NME,
    SCM.Schema_NME,
    DF.Id as 'DF_Id',
    DF.Name,
    COALESCE(DF.DeleteIssuer, SCM.DeleteIssuer, DFC.DeleteIssuer, DS.DeleteIssuer, '072984') as 'New_DeleteIssuer',
    CASE
	    WHEN DF.DeleteIssueDTM <> '1900-01-01 00:00:00.000'		AND DF.DeleteIssueDTM <> '9999-12-31 23:59:59.000'	AND DF.DeleteIssueDTM IS NOT NULL	THEN DF.DeleteIssueDTM
	    WHEN SCM.DeleteIssueDTM <> '1900-01-01 00:00:00.000'	AND SCM.DeleteIssueDTM <> '9999-12-31 23:59:59.000'	AND SCM.DeleteIssueDTM IS NOT NULL	THEN SCM.DeleteIssueDTM
	    WHEN DFC.DeleteIssueDTM <> '1900-01-01 00:00:00.000'	AND DFC.DeleteIssueDTM <> 'Dec 31 9999 11:59PM'	AND DFC.DeleteIssueDTM IS NOT NULL	THEN DFC.DeleteIssueDTM
	    WHEN DS.DeleteIssueDTM <> '1900-01-01 00:00:00.000'		AND DS.DeleteIssueDTM <> '9999-12-31 23:59:59.000'	AND DS.DeleteIssueDTM IS NOT NULL	THEN DS.DeleteIssueDTM
	    ELSE GETDATE()
    END as 'New_DeleteIssueDTM'
    into #IdentifiedDataFlowUpdates
    from DataFlow DF
    join DataFlowStep DFS on
	    DF.Id = DFS.DataFlow_Id
    join DataStepToSchema DSTS on
	    DFS.id = DSTS.DataFlowStepId
    left join [Schema] SCM on
	    DSTS.SchemaId = SCM.Schema_Id
    left join DatasetFileConfigs DFC on
	    SCM.Schema_Id = DFC.Schema_Id
    left join Dataset DS on
	    DFC.Dataset_ID = DS.Dataset_ID
    where DF.ObjectStatus <> 1 and DF.DeleteIssuer is null

    update Dataflow
    SET DeleteIssuer = x.New_DeleteIssuer, DeleteIssueDTM = x.New_DeleteIssueDTM
    from #IdentifiedDataFlowUpdates x
    where Id = x.DF_Id

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


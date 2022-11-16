SET @ScriptVersion = 'CLA-4668-RemoveQueryPermissionFromDefaultSecurity'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'


    -- BEGIN POST-DEPLOY SCRIPT --
    DELETE perm FROM SecurityPermission as perm INNER JOIN SecurityTicket on perm.AddedFromTicket_ID = SecurityTicket.SecurityTicket_ID
    WHERE SecurityTicket.Ticket_ID = 'DEFAULT_SECURITY' and perm.Permission_ID = 4
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

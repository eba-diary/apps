/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
SET @ScriptVersion = 'CLA-4479-MigrateTokensFromDataSourceToTokenTable'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'


    -- BEGIN POST-DEPLOY SCRIPT --

    -- If there is a empty OAuthGrantType, move token over, and update to JWT type --

    -- Move token values from DataSource to Token table
    INSERT INTO DataSourceTokens(ParentDataSource_Id, CurrentToken, CurrentTokenExp, TokenUrl, Scope, TokenExp)
    SELECT DataSource_Id,CurrentToken, CurrentTokenExp, TokenUrl, Scope, TokenExp
    FROM DataSource
    WHERE SourceAuth_ID = 4 and OAuthGrantType = null

    -- Set Existing OAuth Data sources to JWT --
    UPDATE DataSource SET OAuthGrantType = 0 WHERE SourceAuth_ID = 4 and OAuthGrantType = null

    -- Add reference to token in addition to datasource for AuthenticationClaims
    UPDATE AuthenticationClaims 
    SET AuthenticationClaims.Token_Id = DataSourceTokens.Id 
    FROM AuthenticationClaims INNER JOIN DataSourceTokens on AuthenticationClaims.DataSource_Id = DataSourceTokens.ParentDataSource_Id 

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


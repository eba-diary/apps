DECLARE @ScriptVersion AS VARCHAR(50) 
SET @ScriptVersion = '2020.04.08_01_PreDeploy'

BEGIN TRAN 
  
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 

  --insert one off script files here
  :r ..\Pre-Deploy\SupportingScripts\Sprint_20_02_05\INSERT_SECURITY_BUSINESS_AREA.sql
  
  --insert into the verision table so these scripts do not run again.
  INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 

END TRY 

BEGIN CATCH 
    DECLARE @Error_PreDepoly_Message NVARCHAR(4000); 
    DECLARE @Error_PreDepoly_Severity INT; 
    DECLARE @Error_PreDepoly_State INT; 
  
    SELECT 
        @Error_PreDepoly_Message = ERROR_MESSAGE(), 
        @Error_PreDepoly_Severity = ERROR_SEVERITY(), 
        @Error_PreDepoly_State = ERROR_STATE(); 
  
    RAISERROR (@Error_PreDepoly_Message, 
               @Error_PreDepoly_Severity, 
               @Error_PreDepoly_State 
               ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 
  
COMMIT TRAN
SET @ScriptVersion = 'CLA_3790DatasetsSaidAssets'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    IF OBJECT_ID('tempdb..#DatasetsFromSpreadsheet') IS NOT NULL DROP TABLE #DatasetsFromSpreadsheet

    DECLARE @ENV_CLA3790_DATASETS VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')

    Select @ENV_CLA3790_DATASETS

    if (@ENV_CLA3790_DATASETS = 'PROD')
    BEGIN

    Create Table #DatasetsFromSpreadsheet
    ( DatasetID Int not null,
      SaidAsset varchar(10) not null
      )

	insert into #DatasetsFromSpreadsheet select 275,	'CRVS'
	insert into #DatasetsFromSpreadsheet select 276,	'CRVS'
	insert into #DatasetsFromSpreadsheet select 307,	'DAPA'
	insert into #DatasetsFromSpreadsheet select 205,	'DACE'
	insert into #DatasetsFromSpreadsheet select 237,	'VINS'
	insert into #DatasetsFromSpreadsheet select 323,	'VINS'
	insert into #DatasetsFromSpreadsheet select 316,	'PLUS'
	insert into #DatasetsFromSpreadsheet select 116,	'NAME'


    END


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


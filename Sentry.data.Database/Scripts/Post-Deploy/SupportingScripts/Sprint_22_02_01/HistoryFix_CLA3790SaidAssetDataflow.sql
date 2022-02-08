SET @ScriptVersion = 'CLA_3790DataflowsSaidAssets'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    IF OBJECT_ID('tempdb..#FlowsFromSpreadsheet') IS NOT NULL DROP TABLE #FlowsFromSpreadsheet

    DECLARE @ENV_CLA3790_DATAFLOW VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')

    Select @ENV_CLA3790_DATAFLOW

    if (@ENV_CLA3790_DATAFLOW = 'PROD')
    BEGIN

    Create Table #FlowsFromSpreadsheet
    ( DataFlowID Int not null,
      SaidAsset varchar(10) not null
      )

	insert into #FlowsFromSpreadsheet select 572,	'DATA'
	insert into #FlowsFromSpreadsheet select 574,	'DATA'
	insert into #FlowsFromSpreadsheet select 697,	'DATA'
	insert into #FlowsFromSpreadsheet select 698,	'DATA'
	insert into #FlowsFromSpreadsheet select 699,	'DATA'
	insert into #FlowsFromSpreadsheet select 703,	'DATA'
	insert into #FlowsFromSpreadsheet select 705,	'DATA'
	insert into #FlowsFromSpreadsheet select 739,	'DATA'
	insert into #FlowsFromSpreadsheet select 740,	'DATA'
	insert into #FlowsFromSpreadsheet select 741,	'DATA'
	insert into #FlowsFromSpreadsheet select 742,	'DATA'
	insert into #FlowsFromSpreadsheet select 743,	'DATA'
	insert into #FlowsFromSpreadsheet select 744,	'DATA'
	insert into #FlowsFromSpreadsheet select 745,	'DATA'
	insert into #FlowsFromSpreadsheet select 746,	'DATA'
	insert into #FlowsFromSpreadsheet select 141,	'DATA'
    insert into #FlowsFromSpreadsheet select 94,	'DATA'
	insert into #FlowsFromSpreadsheet select 142,	'DATA'
	insert into #FlowsFromSpreadsheet select 143,	'DATA'
	insert into #FlowsFromSpreadsheet select 145,	'DATA'
	insert into #FlowsFromSpreadsheet select 147,	'DATA'
	insert into #FlowsFromSpreadsheet select 149,	'DATA'
	insert into #FlowsFromSpreadsheet select 151,	'DATA'
	insert into #FlowsFromSpreadsheet select 152,	'DATA'
	insert into #FlowsFromSpreadsheet select 154,	'DATA'
	insert into #FlowsFromSpreadsheet select 156,	'DATA'
	insert into #FlowsFromSpreadsheet select 158,	'DATA'
	insert into #FlowsFromSpreadsheet select 160,	'DATA'
	insert into #FlowsFromSpreadsheet select 162,	'DATA'
	insert into #FlowsFromSpreadsheet select 164,	'DATA'
	insert into #FlowsFromSpreadsheet select 166,	'DATA'
	insert into #FlowsFromSpreadsheet select 168,	'DATA'
	insert into #FlowsFromSpreadsheet select 170,	'DATA'
	insert into #FlowsFromSpreadsheet select 172,	'DATA'
	insert into #FlowsFromSpreadsheet select 174,	'DATA'
	insert into #FlowsFromSpreadsheet select 176,	'DATA'
	insert into #FlowsFromSpreadsheet select 178,	'DATA'
	insert into #FlowsFromSpreadsheet select 180,	'DATA'
	insert into #FlowsFromSpreadsheet select 182,	'DATA'
	insert into #FlowsFromSpreadsheet select 184,	'DATA'
	insert into #FlowsFromSpreadsheet select 186,	'DATA'
	insert into #FlowsFromSpreadsheet select 719,	'DATA'
	insert into #FlowsFromSpreadsheet select 721,	'DATA'
	insert into #FlowsFromSpreadsheet select 723,	'DATA'
	insert into #FlowsFromSpreadsheet select 760,	'DATA'
	insert into #FlowsFromSpreadsheet select 767,	'DATA'
	insert into #FlowsFromSpreadsheet select 768,	'DATA'
	insert into #FlowsFromSpreadsheet select 769,	'DATA'
	insert into #FlowsFromSpreadsheet select 771,	'DATA'
	insert into #FlowsFromSpreadsheet select 773,	'DATA'
	insert into #FlowsFromSpreadsheet select 775,	'DATA'
	insert into #FlowsFromSpreadsheet select 777,	'DATA'
	insert into #FlowsFromSpreadsheet select 779,	'DATA'
	insert into #FlowsFromSpreadsheet select 782,	'DATA'
	insert into #FlowsFromSpreadsheet select 783,	'DATA'
	insert into #FlowsFromSpreadsheet select 785,	'DATA'
	insert into #FlowsFromSpreadsheet select 787,	'DATA'
	insert into #FlowsFromSpreadsheet select 789,	'DATA'
	insert into #FlowsFromSpreadsheet select 817,	'DATA'
	insert into #FlowsFromSpreadsheet select 814,	'ACLM'
	insert into #FlowsFromSpreadsheet select 851,	'PLUS'
	insert into #FlowsFromSpreadsheet select 852,	'PLUS'
	insert into #FlowsFromSpreadsheet select 853,	'PLUS'
	insert into #FlowsFromSpreadsheet select 854,	'PLUS'
	insert into #FlowsFromSpreadsheet select 872,	'VINS'
	insert into #FlowsFromSpreadsheet select 65,	'NAME'

	Update DataFlow
	SET SaidKeyCode = x.SaidAsset
	from #FlowsFromSpreadsheet x
	where Id = x.DataFlowID

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


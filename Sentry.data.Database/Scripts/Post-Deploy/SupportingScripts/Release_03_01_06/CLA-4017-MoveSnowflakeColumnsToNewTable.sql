SET @ScriptVersion = 'CLA-4017-MoveSnowflakeColumnsToNewTable'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    INSERT INTO SchemaConsumption (Schema_Id)
    SELECT Schema_Id
    FROM [Schema]

    INSERT INTO SchemaConsumptionSnowflake ([SchemaConsumptionSnowflake_Id],
    [SnowflakeWarehouse], 
    [SnowflakeStage],
    [SnowflakeDatabase],
	[SnowflakeSchema],
    [SnowflakeTable], 
    [SnowflakeStatus], 
    [Snowflake_TYP])
    SELECT cs.SchemaConsumption_Id,
    s.SnowflakeWarehouse,
    s.SnowflakeStage,
    s.SnowflakeDatabase,
    s.SnowflakeSchema,
    s.SnowflakeTable,
    s.SnowflakeStatus,
    'CategorySchemaParquet'
    FROM [Schema] s
    JOIN [SchemaConsumption] cs on s.Schema_Id = cs.Schema_Id


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


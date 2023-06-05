CREATE TABLE [dbo].[SchemaConsumptionSnowflake]
(
	[SchemaConsumptionSnowflake_Id] INT NOT NULL, 
    [SnowflakeWarehouse] VARCHAR(250) NOT NULL, 
    [SnowflakeStage] VARCHAR(250) NOT NULL, 
    [SnowflakeDatabase] VARCHAR(250) NOT NULL,
	[SnowflakeSchema] VARCHAR(250) NOT NULL,
    [SnowflakeTable] VARCHAR(250) NOT NULL, 
    [SnowflakeStatus] VARCHAR(250) NOT NULL, 
    [Snowflake_TYP] VARCHAR(50) NOT NULL, 
    [LastChanged] DATETIME2 NULL, 
    CONSTRAINT [PK_SchemaConsumptionSnowflake] PRIMARY KEY ([SchemaConsumptionSnowflake_Id]), 
    CONSTRAINT [FK_SchemaConsumptionSnowflake_SchemaConsumption] FOREIGN KEY ([SchemaConsumptionSnowflake_Id]) REFERENCES [SchemaConsumption]([SchemaConsumption_Id]) ON DELETE CASCADE
)

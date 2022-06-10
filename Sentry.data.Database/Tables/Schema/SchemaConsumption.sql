CREATE TABLE [dbo].[SchemaConsumption]
(
	[SchemaConsumption_Id] INT NOT NULL IDENTITY, 
    [Schema_Id] INT NOT NULL, 
    [SchemaConsumption_TYP] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_SchemaConsumption] PRIMARY KEY ([SchemaConsumption_Id]), 
    CONSTRAINT [FK_SchemaConsumption_Schema] FOREIGN KEY ([Schema_Id]) REFERENCES [Schema]([Schema_Id]) 
)

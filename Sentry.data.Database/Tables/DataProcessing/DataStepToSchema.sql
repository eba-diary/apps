CREATE TABLE [dbo].[DataStepToSchema]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SearchCriteria] VARCHAR(1000) NULL, 
    [DataFlowStepId] INT NOT NULL, 
    [SchemaId] INT NOT NULL, 
    CONSTRAINT [FK_DataStepToSchema_DataFlowStep] FOREIGN KEY ([DataFlowStepId]) REFERENCES [DataFlowStep]([Id]), 
    CONSTRAINT [FK_DataStepToSchema_Schema] FOREIGN KEY ([SchemaId]) REFERENCES [Schema]([Schema_Id])
)

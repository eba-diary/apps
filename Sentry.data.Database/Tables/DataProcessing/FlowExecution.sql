CREATE TABLE [dbo].[FlowExecution]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [ExecutionGuid] NVARCHAR(250) NOT NULL, 
    [DataFlowId] INT NOT NULL, 
    [Log_Entry] VARCHAR(2000) NULL, 
    [ActionId] INT NOT NULL, 
    [LogLevel] INT NOT NULL, 
    [Machine_Name] VARCHAR(200) NULL, 
    [CreatedDTM] DATETIME NOT NULL, 
    CONSTRAINT [FK_FlowExecution_DataFlow] FOREIGN KEY ([DataFlowId]) REFERENCES [DataFlow]([Id])
)

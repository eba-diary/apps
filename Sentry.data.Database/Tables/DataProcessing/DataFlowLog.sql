CREATE TABLE [dbo].[DataFlowLog]
(
	[Log_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataFlow_Id] INT NOT NULL, 
    [Log_Entry] NVARCHAR(MAX) NULL, 
    [FlowExecutionGuid] VARCHAR(250) NOT NULL,
	[RunInstanceGuid] VARCHAR(25) NULL,
    [DataFlowStep_Id] INT NULL, 
    [Level] INT NOT NULL, 
    [Machine_Name] VARCHAR(250) NULL, 
    [CreatedDTM] DATETIME NOT NULL, 
    CONSTRAINT [FK_DataFlowLog_DataFlow] FOREIGN KEY ([DataFlow_Id]) REFERENCES [DataFlow]([Id]), 
    CONSTRAINT [FK_DataFlowLog_DataFlowStep] FOREIGN KEY ([DataFlowStep_Id]) REFERENCES [DataFlowStep]([Id])
)

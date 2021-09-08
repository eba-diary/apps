CREATE TABLE [dbo].[DataFlowStep]
(
	[Id] INT NOT NULL IDENTITY , 
    [DataFlow_Id] INT NOT NULL, 
    [DataAction_Type_Id] INT NOT NULL, 
    [Action_Id] INT NOT NULL, 
    [ExecutionOrder] INT NOT NULL, 
	[TriggerKey] VARCHAR(1000) NULL, 
    [TargetPrefix] VARCHAR(1000) NULL, 
    [SourceDependencyPrefix] VARCHAR(1000) NULL, 
    [SourceDependencyBucket] VARCHAR(1000) NULL, 
    [TriggerBucket] VARCHAR(1000) NULL, 
    [TargetBucket] VARCHAR(1000) NULL, 
    CONSTRAINT [PK_DataFlowStep] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [FK_DataFlowStep_DataFlow] FOREIGN KEY ([DataFlow_Id]) REFERENCES [DataFlow]([Id]), 
    CONSTRAINT [FK_DataFlowStep_DataAction] FOREIGN KEY ([DataAction_Type_Id]) REFERENCES [DataActionTypes]([Id])
) ON [PRIMARY]


GO


CREATE NONCLUSTERED INDEX [IX_DataFlowStep_TriggerKey] ON [dbo].[DataFlowStep] ([TriggerKey])

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Holds bucket name where triggerfile will be landed',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlowStep',
    @level2type = N'COLUMN',
    @level2name = N'TriggerBucket'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Holds bucket name where step will store data, if step stores long term data (i.e. Raw, Parquet)',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlowStep',
    @level2type = N'COLUMN',
    @level2name = N'TargetBucket'
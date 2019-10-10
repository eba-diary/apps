CREATE TABLE [dbo].[DataFlowStep]
(
	[Id] INT NOT NULL , 
    [DataFlow_Id] INT NOT NULL, 
    [DataAction_Type_Id] INT NOT NULL, 
    [Action_Id] INT NOT NULL, 
    [ExecutionOrder] INT NOT NULL, 
	CONSTRAINT [PK_DataFlowStep] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [FK_DataFlowStep_DataFlow] FOREIGN KEY ([DataFlow_Id]) REFERENCES [DataFlow]([Id]), 
    CONSTRAINT [FK_DataFlowStep_DataAction] FOREIGN KEY ([DataAction_Type_Id]) REFERENCES [DataActionTypes]([Id])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


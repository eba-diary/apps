CREATE TABLE [dbo].[EventMetrics](
	[EventMetricsId] [bigint] IDENTITY(1,1) NOT NULL,
	[FlowExecutionGuid] [varchar](250) NULL,
	[RunInstanceGuid] [varchar](25) NULL,
	[ServiceRunGuid] [varchar](25) NULL,
	[ProcessRunGuid] [varchar](25) NULL,
	[DataFlowStepId] [int] NULL,
	[Partition] [int] NULL,
	[Offset] [int] NULL,
	[MessageKey] [varchar](250) NULL,
	[MessageValue] [varchar](max) NULL,
	[ApplicationName] [varchar](250) NULL,
	[MachineName] [varchar](250) NULL,
	[StatusCode] [char](1) NULL,
	[MetricsData] [varchar](max) NULL,
	[CreatedDTM] [datetime] NULL, 
    CONSTRAINT [PK_EventMetrics] PRIMARY KEY NONCLUSTERED ([EventMetricsId])
);

GO

CREATE CLUSTERED INDEX [PK_EventMetrics__DataFlowStepId_EventMetricsId] ON [dbo].[EventMetrics] ([DataFlowStepId], [EventMetricsId])

GO

CREATE NONCLUSTERED INDEX [IX_EventMetrics__Offset] ON [dbo].[EventMetrics] ([Offset])

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
GO


CREATE NONCLUSTERED INDEX [IDX_DARKO_MADE_ME_DO_EVIL] ON [dbo].[EventMetrics]
(
    [FlowExecutionGuid] ASC,
    [DataFlowStepId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

create index IDX_EventMetricsNeedsValidation on EventMetrics (DataFlowStepId) with(online =on)
GO



CREATE INDEX [IX_EventMetrics_CreatedDTM] ON [dbo].[EventMetrics] ([CreatedDTM])

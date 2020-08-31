CREATE TABLE [dbo].[EventHandlerMetrics](
	[EventHandlerMetrics_ID] [int] IDENTITY(1,1) NOT NULL,
	[Service_Run_GUID] [varchar](20) NULL,
	[Process_Run_GUID] [varchar](20) NULL,
	[Event_Handler_Type] [varchar](50) NULL,
	[Metrics_Data] [varchar](max) NULL,
	[Created_DTM] [datetime] NULL, 
    [Partition_ID] INT NULL, 
    [Offset_ID] INT NULL, 
    [Application_CDE] CHAR(1) NULL, 
    [Status_CDE] NCHAR(1) NULL
)
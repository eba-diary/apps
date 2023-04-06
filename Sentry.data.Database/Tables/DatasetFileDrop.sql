CREATE TABLE [dbo].[DatasetFileDrop]
(
	[DatasetFileDropID] [bigint] IDENTITY(1,1) NOT NULL,
	[FileNME] [varchar](250) NOT NULL,
	[FlowExecutionGUID] [varchar](100) NULL,
	[ObjectBucket] [varchar](250) NOT NULL,
	[ObjectKey] [varchar](1000) NOT NULL,
	[ObjectVersionID] [varchar](250) NULL,
	[ObjectETag] [varchar](250) NULL,
	[ObjectSizeAMT] [bigint] NULL,
	[DatasetID] [int] NULL,
	[SchemaID] [int] NULL,
	[CreateDTM] [datetime] NULL, 
    CONSTRAINT [PK_DatasetFileDrop] PRIMARY KEY ([DatasetFileDropId])
)
GO

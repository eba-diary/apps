CREATE TABLE [dbo].[DatasetFileRaw]
(
	[DatasetFileRawID] [bigint] IDENTITY(1,1) NOT NULL,
	[DatasetFileDropID] [bigint] NULL,
	[FileNME] [varchar](250) NULL,
	[ObjectBucket] [varchar](250) NULL,
	[ObjectKey] [varchar](1000) NULL,
	[ObjectVersionID] [varchar](250) NULL,
	[ObjectETag] [varchar](250) NULL,
	[ObjectSizeAMT] [bigint] NULL,
	[ObjectStatus] [int] NULL,
	[FlowExecutionGUID] [varchar](100) NULL,
	[RunInstanceGUID] [varchar](100) NULL,
	[DatasetID] [int] NULL,
	[SchemaID] [int] NULL,
	[UpdateDTM] [datetime] NULL,
	[CreateDTM] [datetime] NULL, 
    CONSTRAINT [PK_DatasetFileRaw] PRIMARY KEY ([DatasetFileRawID]), 
    CONSTRAINT [FK_DatasetFileRaw_DatasetFileDrop] FOREIGN KEY ([DatasetFileDropID]) REFERENCES [DatasetFileDrop]([DatasetFileDropId])
)
GO

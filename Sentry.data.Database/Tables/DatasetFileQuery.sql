CREATE TABLE [dbo].[DatasetFileQuery]
(
	[DatasetFileQueryID] [bigint] IDENTITY(1,1) NOT NULL,
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
    CONSTRAINT [PK_DatasetFileQuery] PRIMARY KEY ([DatasetFileQueryID]), 
    CONSTRAINT [FK_DatasetFileQuery_DatasetFileDrop] FOREIGN KEY ([DatasetFileDropID]) REFERENCES [DatasetFileDrop]([DatasetFileDropID])
)
GO

CREATE INDEX [IX_DatasetFileQuery_DatasetID_SchemaID_FileNME] ON [dbo].[DatasetFileQuery] ([DatasetID],[SchemaID],[FileNME])
